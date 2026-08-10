// D13-A03 FarmingSystem。
// 作物生长、病虫害、风暴损伤、收获和轮作状态的唯一写者。
// 土壤/水源/生态压力交给 EcologySystem，库存交给 D3InventorySystem，
// 产权和价格只消费 authority 的来源化决定。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13FarmingSystem
    {
        private readonly Dictionary<string, D13FarmingPlotDefinition> _definitions =
            new Dictionary<string, D13FarmingPlotDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FarmingCropDefinition> _crops =
            new Dictionary<string, D13FarmingCropDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FarmingPlotState> _states =
            new Dictionary<string, D13FarmingPlotState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FarmingProcessRecord> _processes =
            new Dictionary<string, D13FarmingProcessRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FarmingSaleRecord> _sales =
            new Dictionary<string, D13FarmingSaleRecord>(StringComparer.Ordinal);
        private readonly D3InventoryState _inventory;
        private readonly ID13FarmingEcologyAuthority _ecology;
        private readonly ID13FarmingLandAuthority _landAuthority;
        private readonly ID13FarmingEconomyAuthority _economyAuthority;
        private readonly EventLog _eventLog;
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private long _revision;

        public D13FarmingSystem(D3InventoryState inventory,
            ID13FarmingEcologyAuthority ecology,
            ID13FarmingLandAuthority landAuthority,
            ID13FarmingEconomyAuthority economyAuthority,
            EventLog eventLog = null,
            D13FarmingRuntimeSnapshot initial = null,
            IEnumerable<D13FarmingPlotDefinition> definitions = null,
            IEnumerable<D13FarmingCropDefinition> crops = null)
        {
            D13FarmingConfiguration.ThrowIfInvalid();
            _inventory = inventory;
            _ecology = ecology;
            _landAuthority = landAuthority ?? new UnavailableLandAuthority();
            _economyAuthority = economyAuthority ?? new UnavailableEconomyAuthority();
            _eventLog = eventLog ?? new EventLog();
            D13FarmingCatalog.RegisterInventoryDefinitions();

            var plotSource = definitions == null ? D13FarmingCatalog.Plots : definitions.ToList();
            foreach (var definition in plotSource)
            {
                if (definition == null) continue;
                definition.Validate();
                if (_definitions.ContainsKey(definition.PlotId)) throw new InvalidOperationException("Duplicate farming plot: " + definition.PlotId);
                _definitions[definition.PlotId] = definition.Clone();
                _states[definition.PlotId] = EmptyState(definition);
            }

            var cropSource = crops == null ? D13FarmingCatalog.Crops : crops.ToList();
            foreach (var crop in cropSource)
            {
                if (crop == null) continue;
                crop.Validate();
                if (_crops.ContainsKey(crop.CropId)) throw new InvalidOperationException("Duplicate farming crop: " + crop.CropId);
                _crops[crop.CropId] = crop.Clone();
            }
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13FarmingPlotDefinition FindPlotDefinition(string plotId)
        {
            return _definitions.TryGetValue(plotId ?? string.Empty, out D13FarmingPlotDefinition value) ? value.Clone() : null;
        }

        public D13FarmingCropDefinition FindCrop(string cropId)
        {
            return _crops.TryGetValue(cropId ?? string.Empty, out D13FarmingCropDefinition value) ? value.Clone() : null;
        }

        public D13FarmingPlotState FindPlotState(string plotId)
        {
            return _states.TryGetValue(plotId ?? string.Empty, out D13FarmingPlotState value) ? value.Clone() : null;
        }

        public D13FarmingReadModel GetReadModel()
        {
            var result = new D13FarmingReadModel { Revision = _revision };
            foreach (var definition in _definitions.Values.OrderBy(item => item.PlotId, StringComparer.Ordinal)) result.PlotDefinitions.Add(definition.Clone());
            foreach (var crop in _crops.Values.OrderBy(item => item.CropId, StringComparer.Ordinal)) result.CropDefinitions.Add(crop.Clone());
            foreach (var state in _states.Values.OrderBy(item => item.PlotId, StringComparer.Ordinal))
            {
                result.Plots.Add(state.Clone());
                var environment = _ecology == null ? null : _ecology.FindPlot(state.PlotId);
                if (environment != null) result.Environments.Add(environment);
                if (state.State != D13FarmingCropStateKind.Empty)
                {
                    result.Risks.Add(new D13FarmingRiskNotice
                    {
                        SubjectId = state.PlotId,
                        Risk = "水分 " + (environment == null ? 0 : environment.Moisture) + "; 病虫害压力 " + state.PestPressure + "; 风暴损伤 " + state.StormDamage + ".",
                        WorstCase = "缺水、病虫害或风暴可使作物枯萎，收成可能为 0；生态压力和土壤肥力不会被隐藏。",
                        RecoveryRoute = D13FarmingIds.CropRecoveryRoute,
                    });
                }
            }
            foreach (var item in _processes.Values.OrderBy(item => item.ProcessId, StringComparer.Ordinal)) result.Processes.Add(item.Clone());
            foreach (var item in _sales.Values.OrderBy(item => item.SaleId, StringComparer.Ordinal)) result.Sales.Add(item.Clone());
            return result;
        }

        public D13FarmingRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13FarmingRuntimeSnapshot
            {
                Revision = _revision,
                Ecology = _ecology == null ? new D13FarmingEcologyRuntimeSnapshot() : _ecology.CaptureSnapshot(),
            };
            foreach (var item in _states.Values) result.Plots.Add(item.Clone());
            foreach (var item in _processes.Values) result.Processes.Add(item.Clone());
            foreach (var item in _sales.Values) result.Sales.Add(item.Clone());
            foreach (var key in _appliedEventKeys) result.AppliedEventKeys.Add(key);
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_farming_", StringComparison.Ordinal) && !result.AppliedEventKeys.Contains(fact.DedupeKey)) result.AppliedEventKeys.Add(fact.DedupeKey);
            return result;
        }

        public void Restore(D13FarmingRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var item in snapshot.Plots)
                if (!_definitions.ContainsKey(item.PlotId)) throw new InvalidOperationException("Unknown farming plot in snapshot: " + item.PlotId);
            foreach (var environment in snapshot.Ecology.Environments)
                if (!_definitions.ContainsKey(environment.PlotId)) throw new InvalidOperationException("Unknown farming ecology plot in snapshot: " + environment.PlotId);
            _states.Clear();
            foreach (var definition in _definitions.Values) _states[definition.PlotId] = EmptyState(definition);
            foreach (var item in snapshot.Plots) _states[item.PlotId] = item.Clone();
            _processes.Clear();
            foreach (var item in snapshot.Processes) _processes[item.ProcessId] = item.Clone();
            _sales.Clear();
            foreach (var item in snapshot.Sales) _sales[item.SaleId] = item.Clone();
            if (_ecology != null) _ecology.RestoreSnapshot(snapshot.Ecology);
            _appliedEventKeys.Clear();
            foreach (var key in snapshot.AppliedEventKeys) _appliedEventKeys.Add(key);
            _revision = snapshot.Revision;
        }

        public D13FarmingResult Plant(D13FarmingPlantRequest request)
        {
            if (request == null) return Invalid("invalid_request", "播种请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "plant_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryPlotAndCrop(request.PlotId, request.CropId, out D13FarmingPlotDefinition plot, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "plot_or_crop_missing", "地块或作物定义不存在。", D13FarmingIds.CropRecoveryRoute);
            if (!_states[plot.PlotId].State.Equals(D13FarmingCropStateKind.Empty))
                return Failure(request, eventId, "plot_occupied", "地块已有作物，必须先收获或恢复。", D13FarmingIds.CropRecoveryRoute, _states[plot.PlotId], crop);
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute);
            if (!SourcesExist(land.PropertySourceEventId, land.LegalSourceEventId, request.SaveRevision))
                return Failure(request, eventId, "property_source_missing", "播种缺少 PropertySystem/法律来源事实。", D13FarmingIds.LandRecoveryRoute);
            D13FarmingSeason season = SeasonFor(request.WorldDay);
            if (!crop.AllowedSeasons.Contains(season)) return Failure(request, eventId, "season_invalid", "当前季节不适合该作物。", D13FarmingIds.CropRecoveryRoute, _states[plot.PlotId], crop);
            if (!string.IsNullOrWhiteSpace(request.SeedItemId) && request.SeedItemId != crop.SeedItemId)
                return Failure(request, eventId, "seed_mismatch", "种子 ID 与作物标签不匹配。", D13FarmingIds.InventoryRecoveryRoute, _states[plot.PlotId], crop);
            if (_inventory == null || D3InventorySystem.CountCargo(_inventory, crop.SeedItemId) < 1)
                return Failure(request, eventId, "seed_missing", "货舱没有该作物的种子。", D13FarmingIds.InventoryRecoveryRoute, _states[plot.PlotId], crop);
            if (_ecology == null) return Failure(request, eventId, "ecology_authority_required", "必须由 EcologySystem 提供土壤和水源状态。", D13FarmingIds.EcologyRecoveryRoute);
            var environment = _ecology.EnsurePlot(plot, request.WorldDay, request.Seed);
            if (environment == null || !string.Equals(environment.SoilType, crop.RequiredSoilType, StringComparison.Ordinal))
                return Failure(request, eventId, "soil_mismatch", "作物要求的土壤类型与地块不匹配。", D13FarmingIds.EcologyRecoveryRoute, _states[plot.PlotId], crop);
            if (!D3InventorySystem.TryRemoveCargo(_inventory, crop.SeedItemId, 1, out string removeError))
                return Failure(request, eventId, removeError, "种子无法从货舱扣除。", D13FarmingIds.InventoryRecoveryRoute, _states[plot.PlotId], crop);
            var ecology = ApplyEcology(request, eventId, plot.PlotId, D13FarmingEcologyOperation.Planting, crop.CropId, 1, 1, false, 0, false);
            if (!ecology.Succeeded || ecology.Duplicate)
            {
                D3InventorySystem.TryAddCargo(_inventory, crop.SeedItemId, 1, out _);
                return ecology.Duplicate ? Duplicate(ecology.Fact, request.PlotId) : Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.EcologyRecoveryRoute, _states[plot.PlotId], crop);
            }
            var state = _states[plot.PlotId];
            state.PlantingId = "planting_" + SafeId(plot.PlotId) + "_" + Math.Max(0, request.WorldDay) + "_" + Stable(crop.CropId + ":" + request.Seed);
            state.CropId = crop.CropId;
            state.SeedItemId = crop.SeedItemId;
            state.CropKind = crop.Kind;
            state.State = D13FarmingCropStateKind.Planted;
            state.PlantedSeason = season;
            state.PlantDay = Math.Max(0, request.WorldDay);
            state.LastUpdatedDay = state.PlantDay;
            state.GrowthPoints = 0;
            state.GrowthPercent = 0;
            state.WaterApplied = 0;
            state.LaborApplied = 0;
            state.PestPressure = 0;
            state.StormDamage = 0;
            state.Health = 100;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_planted", "种子已在有产权资格、土壤和季节条件的地块播种。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["property_id"] = plot.PropertyId, ["crop_id"] = crop.CropId,
                ["crop_kind"] = crop.Kind.ToString(), ["tags"] = string.Join(",", crop.Tags),
                ["seed_item_id"] = crop.SeedItemId, ["season"] = season.ToString(),
                ["ecology_event_id"] = ecology.EventId, ["property_source_event_id"] = land.PropertySourceEventId ?? "",
                ["legal_source_event_id"] = land.LegalSourceEventId ?? "",
            });
            _revision++;
            return Success(fact, state, crop, ecology.Environment, 0, null, 0, 0, 0, null);
        }

        public D13FarmingResult Water(D13FarmingWaterRequest request)
        {
            if (request == null) return Invalid("invalid_request", "浇水请求不存在。", D13FarmingIds.WaterRecoveryRoute);
            string eventId = EventId(request.OperationId, "water_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryActive(request.PlotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "crop_missing", "地块没有正在生长的作物。", D13FarmingIds.CropRecoveryRoute);
            if (request.WaterUnits <= 0 || request.WaterUnits > D13FarmingConfiguration.Int(D13FarmingConfiguration.DefaultWaterPerAction) * 5)
                return Failure(request, eventId, "water_amount_invalid", "浇水量超出单次确定性操作边界。", D13FarmingIds.WaterRecoveryRoute, state, crop);
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute, state, crop);
            var ecology = ApplyEcology(request, eventId, plot.PlotId, D13FarmingEcologyOperation.Watering, crop.CropId, request.WaterUnits, 1, false, 0, false);
            if (!ecology.Succeeded || ecology.Duplicate) return ecology.Duplicate ? Duplicate(ecology.Fact, request.PlotId) : Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.WaterRecoveryRoute, state, crop);
            state.WaterApplied = SafeAdd(state.WaterApplied, ecology.Quantity);
            state.Health = Math.Min(100, state.Health + 1);
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_watered", "浇水已由 EcologySystem 从登记水源结算。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["crop_id"] = crop.CropId, ["water_units"] = ecology.Quantity.ToString(CultureInfo.InvariantCulture),
                ["water_source_id"] = ecology.Environment == null ? plot.WaterSourceId : ecology.Environment.WaterSourceId,
                ["ecology_event_id"] = ecology.EventId,
            });
            _revision++;
            return Success(fact, state, crop, ecology.Environment, ecology.Quantity, null, 0, 0, 0, null);
        }

        public D13FarmingResult AssignLabor(D13FarmingLaborRequest request)
        {
            if (request == null) return Invalid("invalid_request", "劳力请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "labor_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryActive(request.PlotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "crop_missing", "地块没有正在生长的作物。", D13FarmingIds.CropRecoveryRoute);
            if (request.LaborUnits <= 0 || request.LaborUnits > D13FarmingConfiguration.Int(D13FarmingConfiguration.MaximumLaborPerAction))
                return Failure(request, eventId, "labor_invalid", "劳力单位超出确定性操作边界。", D13FarmingIds.CropRecoveryRoute, state, crop);
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute, state, crop);
            state.LaborApplied = SafeAdd(state.LaborApplied, request.LaborUnits);
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_labor_assigned", "劳力已登记；生长速度将在下一次日推进时确定性计算。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["crop_id"] = crop.CropId, ["labor_units"] = request.LaborUnits.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, crop, _ecology == null ? null : _ecology.FindPlot(plot.PlotId), request.LaborUnits, null, 0, 0, 0, null);
        }

        public D13FarmingResult TreatPests(D13FarmingPestTreatmentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "病虫害处理请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "pest_treatment_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryActive(request.PlotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "crop_missing", "地块没有正在生长的作物。", D13FarmingIds.CropRecoveryRoute);
            if (request.TreatmentItemId != crop.PestTreatmentItemId || request.TreatmentUnits <= 0)
                return Failure(request, eventId, "treatment_invalid", "病虫害处理物资或数量不正确。", D13FarmingIds.InventoryRecoveryRoute, state, crop);
            if (_inventory == null || D3InventorySystem.CountCargo(_inventory, crop.PestTreatmentItemId) < request.TreatmentUnits)
                return Failure(request, eventId, "treatment_missing", "货舱没有足够的病虫害处理物资。", D13FarmingIds.InventoryRecoveryRoute, state, crop);
            if (!D3InventorySystem.TryRemoveCargo(_inventory, crop.PestTreatmentItemId, request.TreatmentUnits, out string removeError))
                return Failure(request, eventId, removeError, "处理物资无法从货舱扣除。", D13FarmingIds.InventoryRecoveryRoute, state, crop);
            var ecology = ApplyEcology(request, eventId, plot.PlotId, D13FarmingEcologyOperation.PestTreatment, crop.CropId, request.TreatmentUnits, 1, false, 0, false);
            if (!ecology.Succeeded || ecology.Duplicate)
            {
                D3InventorySystem.TryAddCargo(_inventory, crop.PestTreatmentItemId, request.TreatmentUnits, out _);
                return ecology.Duplicate ? Duplicate(ecology.Fact, request.PlotId) : Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.EcologyRecoveryRoute, state, crop);
            }
            state.PestPressure = Math.Max(0, state.PestPressure - request.TreatmentUnits * 25);
            state.Health = Math.Min(100, state.Health + Math.Max(1, request.Skill / 2));
            if (state.State == D13FarmingCropStateKind.Infested) state.State = D13FarmingCropStateKind.Growing;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_pests_treated", "病虫害处理完成，污染和生态代价由 EcologySystem 记录。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["crop_id"] = crop.CropId, ["treatment_item_id"] = crop.PestTreatmentItemId,
                ["treatment_units"] = request.TreatmentUnits.ToString(CultureInfo.InvariantCulture), ["ecology_event_id"] = ecology.EventId,
            });
            _revision++;
            return Success(fact, state, crop, ecology.Environment, request.TreatmentUnits, null, 0, 0, 0, null);
        }

        public D13FarmingResult AdvanceDay(D13FarmingAdvanceRequest request)
        {
            if (request == null) return Invalid("invalid_request", "农业日推进请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "advance_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryActive(request.PlotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "crop_missing", "地块没有正在生长的作物。", D13FarmingIds.CropRecoveryRoute);
            int days = Math.Max(1, request.Days);
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute, state, crop);
            if (request.Storm)
            {
                var storm = ApplyEcology(request, eventId + "_storm", plot.PlotId, D13FarmingEcologyOperation.Storm, crop.CropId, 1, days, true, request.StormIntensity, false);
                if (!storm.Succeeded) return Failure(request, eventId, storm.Code, storm.Message, D13FarmingIds.EcologyRecoveryRoute, state, crop);
                int stormLoss = Math.Max(0, request.StormIntensity - crop.StormResistancePercent);
                state.StormDamage = Math.Min(100, state.StormDamage + stormLoss);
                state.Health = Math.Max(0, state.Health - Math.Max(1, stormLoss / 5));
            }
            var ecology = ApplyEcology(request, eventId + "_ecology", plot.PlotId, D13FarmingEcologyOperation.AdvanceDay, crop.CropId, 1, days, false, 0, request.Rainfall);
            if (!ecology.Succeeded) return Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.EcologyRecoveryRoute, state, crop);
            var environment = ecology.Environment;
            D13FarmingSeason season = SeasonFor(request.WorldDay);
            int daily = D13FarmingConfiguration.Int(D13FarmingConfiguration.GrowthPointsPerDay);
            if (environment != null && environment.Moisture >= D13FarmingConfiguration.Int(D13FarmingConfiguration.LowMoistureThreshold)) daily += D13FarmingConfiguration.Int(D13FarmingConfiguration.WaterGrowthBonus);
            else state.Health = Math.Max(0, state.Health - days * 5);
            if (state.LaborApplied >= crop.LaborRequired) daily += D13FarmingConfiguration.Int(D13FarmingConfiguration.LaborGrowthBonus);
            if (season == D13FarmingSeason.Hurricane) daily = Math.Max(1, daily / 2);
            state.GrowthPoints = SafeAdd(state.GrowthPoints, daily * days);
            int riskRoll = Stable(crop.CropId + ":pest:" + plot.PlotId + ":" + request.WorldDay + ":" + request.Seed) % 100;
            int pestRisk = crop.PestRiskPercent + (environment == null ? 0 : environment.PestPressure / 2);
            if (riskRoll < pestRisk)
            {
                var infestation = ApplyEcology(request, eventId + "_pest", plot.PlotId, D13FarmingEcologyOperation.PestInfestation, crop.CropId, 25, days, false, 0, false);
                if (!infestation.Succeeded) return Failure(request, eventId, infestation.Code, infestation.Message, D13FarmingIds.EcologyRecoveryRoute, state, crop);
                state.PestPressure = Math.Min(100, state.PestPressure + 25);
                state.Health = Math.Max(0, state.Health - 8);
            }
            if (state.Health <= 0)
            {
                state.State = D13FarmingCropStateKind.Withered;
            }
            else if (state.PestPressure >= D13FarmingConfiguration.Int(D13FarmingConfiguration.PestThreshold))
            {
                state.State = D13FarmingCropStateKind.Infested;
            }
            else if (state.GrowthPoints >= crop.GrowthDays * D13FarmingConfiguration.Int(D13FarmingConfiguration.GrowthPointsPerDay))
            {
                state.State = D13FarmingCropStateKind.Ready;
            }
            else if (request.Storm)
            {
                state.State = D13FarmingCropStateKind.StormDamaged;
            }
            else
            {
                state.State = D13FarmingCropStateKind.Growing;
            }
            state.GrowthPercent = Math.Min(100, (int)Math.Floor(state.GrowthPoints * 100m / Math.Max(1, crop.GrowthDays * D13FarmingConfiguration.Int(D13FarmingConfiguration.GrowthPointsPerDay))));
            state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, request.WorldDay));
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_day_advanced", "农业日推进已结算生长、缺水、病虫害和风暴后果。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["crop_id"] = crop.CropId, ["days"] = days.ToString(CultureInfo.InvariantCulture),
                ["season"] = season.ToString(), ["growth_points"] = state.GrowthPoints.ToString(CultureInfo.InvariantCulture),
                ["growth_percent"] = state.GrowthPercent.ToString(CultureInfo.InvariantCulture), ["state"] = state.State.ToString(),
                ["health"] = state.Health.ToString(CultureInfo.InvariantCulture), ["pest_pressure"] = state.PestPressure.ToString(CultureInfo.InvariantCulture),
                ["storm_damage"] = state.StormDamage.ToString(CultureInfo.InvariantCulture), ["ecology_event_id"] = ecology.EventId,
            });
            _revision++;
            return Success(fact, state, crop, environment, days, null, 0, 0, 0, null);
        }

        public D13FarmingResult Harvest(D13FarmingHarvestRequest request)
        {
            if (request == null) return Invalid("invalid_request", "收获请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "harvest_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!TryActive(request.PlotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop))
                return Failure(request, eventId, "crop_missing", "地块没有正在生长的作物。", D13FarmingIds.CropRecoveryRoute);
            if (state.State != D13FarmingCropStateKind.Ready && state.GrowthPoints < crop.GrowthDays * D13FarmingConfiguration.Int(D13FarmingConfiguration.GrowthPointsPerDay))
                return Failure(request, eventId, "not_ready", "作物尚未达到可收获成熟度。", D13FarmingIds.CropRecoveryRoute, state, crop);
            if (state.Health <= 0 || state.State == D13FarmingCropStateKind.Withered)
                return Failure(request, eventId, "crop_withered", "作物已经枯萎，不能生成收成。", D13FarmingIds.CropRecoveryRoute, state, crop);
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute, state, crop);
            var environment = _ecology == null ? null : _ecology.FindPlot(plot.PlotId);
            int fertility = environment == null ? 50 : environment.Fertility;
            int quantity = Math.Max(1, (int)Math.Floor(crop.BaseYield * (long)plot.AreaUnits * (0.50m + state.Health / 200m) * (0.50m + fertility / 200m)));
            string capacityError = null;
            bool canAddCargo = _inventory != null && D3InventorySystem.CanAddCargo(_inventory, crop.HarvestItemId, quantity, out capacityError);
            if (!canAddCargo)
                return Failure(request, eventId, capacityError ?? "cargo_full", "收成无法装入货舱，生态状态未扣除。", D13FarmingIds.InventoryRecoveryRoute, state, crop);
            if (!D3InventorySystem.TryAddCargo(_inventory, crop.HarvestItemId, quantity, out string addError))
                return Failure(request, eventId, addError, "收成无法写入货舱。", D13FarmingIds.InventoryRecoveryRoute, state, crop);
            var ecology = ApplyEcology(request, eventId, plot.PlotId, D13FarmingEcologyOperation.Harvest, crop.CropId, D13FarmingConfiguration.Int(D13FarmingConfiguration.HarvestFertilityCost), 1, false, 0, false);
            if (!ecology.Succeeded || ecology.Duplicate)
            {
                D3InventorySystem.TryRemoveCargo(_inventory, crop.HarvestItemId, quantity, out _);
                return ecology.Duplicate ? Duplicate(ecology.Fact, request.PlotId) : Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.EcologyRecoveryRoute, state, crop);
            }
            state.RotationHistory.Add(crop.RotationFamily);
            state.HarvestCount++;
            state.RotationIndex++;
            ClearCrop(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_harvested", "收成已写入货舱；肥力和生态压力由 EcologySystem 同步扣除。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["crop_id"] = crop.CropId, ["crop_kind"] = crop.Kind.ToString(),
                ["tags"] = string.Join(",", crop.Tags), ["harvest_item_id"] = crop.HarvestItemId,
                ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture), ["ecology_event_id"] = ecology.EventId,
                ["fertility_after"] = ecology.Environment == null ? "" : ecology.Environment.Fertility.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, crop, ecology.Environment, quantity, crop.HarvestItemId, 0, 0, 0, null);
        }

        public D13FarmingResult Rotate(D13FarmingRotationRequest request)
        {
            if (request == null) return Invalid("invalid_request", "轮作请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "rotate_" + request.PlotId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (!_states.TryGetValue(request.PlotId ?? string.Empty, out D13FarmingPlotState state) || state.State != D13FarmingCropStateKind.Empty)
                return Failure(request, eventId, "rotation_requires_empty_plot", "轮作必须在收获后空置地块进行。", D13FarmingIds.CropRecoveryRoute, state, null);
            if (string.IsNullOrWhiteSpace(request.NextCropId) || !_crops.TryGetValue(request.NextCropId, out D13FarmingCropDefinition nextCrop))
                return Failure(request, eventId, "rotation_crop_missing", "轮作目标作物不存在。", D13FarmingIds.CropRecoveryRoute, state, null);
            var plot = _definitions[request.PlotId];
            var land = ValidateLand(plot, request);
            if (!land.Allowed) return Failure(request, eventId, land.Code, land.Message, D13FarmingIds.LandRecoveryRoute, state, nextCrop);
            var ecology = ApplyEcology(request, eventId, plot.PlotId, D13FarmingEcologyOperation.Rotation, nextCrop.CropId, D13FarmingConfiguration.Int(D13FarmingConfiguration.RotationFertilityBonus), 1, false, 0, false);
            if (!ecology.Succeeded || ecology.Duplicate) return ecology.Duplicate ? Duplicate(ecology.Fact, request.PlotId) : Failure(request, eventId, ecology.Code, ecology.Message, D13FarmingIds.EcologyRecoveryRoute, state, nextCrop);
            state.RotationHistory.Add(nextCrop.RotationFamily);
            state.RotationIndex++;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "farming_rotated", "轮作已登记，EcologySystem 提高肥力并降低部分病虫害压力。", new Dictionary<string, string>
            {
                ["plot_id"] = plot.PlotId, ["next_crop_id"] = nextCrop.CropId, ["rotation_family"] = nextCrop.RotationFamily,
                ["ecology_event_id"] = ecology.EventId,
            });
            _revision++;
            return Success(fact, state, nextCrop, ecology.Environment, 0, null, 0, 0, 0, null);
        }

        public D13FarmingResult Process(D13FarmingProcessRequest request)
        {
            if (request == null) return Invalid("invalid_request", "加工请求不存在。", D13FarmingIds.CropRecoveryRoute);
            string eventId = EventId(request.OperationId, "process_" + request.InputItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (_inventory == null || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.InputItemId))
                return Failure(request, eventId, "process_request_invalid", "加工输入和数量必须有效。", D13FarmingIds.InventoryRecoveryRoute);
            var crop = D13FarmingCatalog.FindCropForItem(request.InputItemId);
            if (crop == null || crop.HarvestItemId != request.InputItemId)
                return Failure(request, eventId, "process_input_invalid", "只有农业收成可以进入该加工路径。", D13FarmingIds.CropRecoveryRoute);
            int batches = request.Quantity / crop.ProcessInputPerBatch;
            int inputQuantity = batches * crop.ProcessInputPerBatch;
            int outputQuantity = batches * crop.ProcessOutputPerBatch;
            if (batches <= 0) return Failure(request, eventId, "process_batch_incomplete", "加工数量不足一个完整批次。", D13FarmingIds.InventoryRecoveryRoute);
            if (D3InventorySystem.CountCargo(_inventory, request.InputItemId) < inputQuantity)
                return Failure(request, eventId, "process_input_missing", "货舱没有足够的农业收成。", D13FarmingIds.InventoryRecoveryRoute);
            if (!D3InventorySystem.CanAddCargo(_inventory, crop.ProcessedItemId, outputQuantity, out string capacityError))
                return Failure(request, eventId, capacityError ?? "cargo_full", "加工品无法装入货舱。", D13FarmingIds.InventoryRecoveryRoute);
            if (!D3InventorySystem.TryRemoveCargo(_inventory, request.InputItemId, inputQuantity, out string removeError))
                return Failure(request, eventId, removeError, "加工输入无法从货舱扣除。", D13FarmingIds.InventoryRecoveryRoute);
            if (!D3InventorySystem.TryAddCargo(_inventory, crop.ProcessedItemId, outputQuantity, out string addError))
            {
                D3InventorySystem.TryAddCargo(_inventory, request.InputItemId, inputQuantity, out _);
                return Failure(request, eventId, addError, "加工品无法写入货舱。", D13FarmingIds.InventoryRecoveryRoute);
            }
            var record = new D13FarmingProcessRecord
            {
                ProcessId = "process_" + SafeId(string.IsNullOrWhiteSpace(request.OperationId) ? eventId : request.OperationId),
                PlotId = request.PlotId ?? string.Empty,
                InputItemId = request.InputItemId,
                OutputItemId = crop.ProcessedItemId,
                InputQuantity = inputQuantity,
                OutputQuantity = outputQuantity,
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0L, request.SaveRevision),
            };
            var fact = AppendEvent(eventId, request, "farming_processed", "农业收成已按固定批次加工，输入与产出均已写入货舱。", new Dictionary<string, string>
            {
                ["input_item_id"] = request.InputItemId, ["input_quantity"] = inputQuantity.ToString(CultureInfo.InvariantCulture),
                ["output_item_id"] = crop.ProcessedItemId, ["output_quantity"] = outputQuantity.ToString(CultureInfo.InvariantCulture),
                ["process_days"] = crop.ProcessDays.ToString(CultureInfo.InvariantCulture), ["tags"] = string.Join(",", crop.Tags),
            });
            _processes[record.ProcessId] = record;
            _revision++;
            return Success(fact, request.PlotId == null ? null : FindPlotState(request.PlotId), crop, _ecology == null || request.PlotId == null ? null : _ecology.FindPlot(request.PlotId), outputQuantity, crop.ProcessedItemId, inputQuantity, 0, 0, null);
        }

        public D13FarmingResult Sell(D13FarmingSaleRequest request)
        {
            if (request == null) return Invalid("invalid_request", "销售请求不存在。", D13FarmingIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "sell_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.PlotId);
            if (_inventory == null || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.ItemId))
                return Failure(request, eventId, "sale_request_invalid", "销售物品和数量必须有效。", D13FarmingIds.EconomyRecoveryRoute);
            var crop = D13FarmingCatalog.FindCropForItem(request.ItemId);
            if (crop == null || (crop.HarvestItemId != request.ItemId && crop.ProcessedItemId != request.ItemId))
                return Failure(request, eventId, "sale_item_invalid", "只有农业收成或加工品可以走农业销售请求。", D13FarmingIds.EconomyRecoveryRoute);
            if (D3InventorySystem.CountCargo(_inventory, request.ItemId) < request.Quantity)
                return Failure(request, eventId, "sale_inventory_missing", "货舱没有足够的销售物品。", D13FarmingIds.InventoryRecoveryRoute);
            var decision = _economyAuthority.QuoteAndApproveSale(request.ItemId, request.Quantity, request.MarketRef, request.ActorRef, request.WorldDay, request.SaveRevision);
            if (decision == null || !decision.Allowed)
                return Failure(request, eventId, decision == null ? "economy_authority_required" : decision.Code, decision == null ? "必须由 EconomySystem 裁决农业销售价格。" : decision.Message, D13FarmingIds.EconomyRecoveryRoute);
            if (decision.SourceOwnerSystem != D13FarmingIds.EconomyOwner || decision.UnitPrice < D13FarmingConfiguration.Int(D13FarmingConfiguration.MinimumSalePrice) || decision.TotalPrice != decision.UnitPrice * request.Quantity || string.IsNullOrWhiteSpace(decision.SourceEventId))
                return Failure(request, eventId, "invalid_economy_decision", "销售价格必须来自 EconomySystem 的可追溯裁决。", D13FarmingIds.EconomyRecoveryRoute);
            if (!SourcesExist(decision.SourceEventId, null, request.SaveRevision))
                return Failure(request, eventId, "economy_source_missing", "销售经济来源事实不存在。", D13FarmingIds.EconomyRecoveryRoute);
            if (!D3InventorySystem.TryRemoveCargo(_inventory, request.ItemId, request.Quantity, out string removeError))
                return Failure(request, eventId, removeError, "销售物品无法从货舱扣除。", D13FarmingIds.InventoryRecoveryRoute);
            var record = new D13FarmingSaleRecord
            {
                SaleId = "sale_" + SafeId(string.IsNullOrWhiteSpace(request.OperationId) ? eventId : request.OperationId),
                ItemId = request.ItemId,
                Quantity = request.Quantity,
                UnitPrice = decision.UnitPrice,
                TotalPrice = decision.TotalPrice,
                Currency = decision.Currency ?? D13FarmingIds.Currency,
                MarketRef = decision.MarketRef ?? request.MarketRef,
                LedgerRef = decision.LedgerRef,
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0L, request.SaveRevision),
            };
            var fact = AppendEvent(eventId, request, "farming_sale_approved", "EconomySystem 已批准农业销售；本系统只记录货物离库和来源，不直接写金钱。", new Dictionary<string, string>
            {
                ["item_id"] = request.ItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["unit_price"] = decision.UnitPrice.ToString(CultureInfo.InvariantCulture), ["total_price"] = decision.TotalPrice.ToString(CultureInfo.InvariantCulture),
                ["currency"] = record.Currency, ["market_ref"] = record.MarketRef ?? "", ["ledger_ref"] = record.LedgerRef ?? "",
                ["economy_source_event_id"] = decision.SourceEventId,
            });
            _sales[record.SaleId] = record;
            _revision++;
            return Success(fact, request.PlotId == null ? null : FindPlotState(request.PlotId), crop, _ecology == null || request.PlotId == null ? null : _ecology.FindPlot(request.PlotId), request.Quantity, request.ItemId, 0, decision.UnitPrice, decision.TotalPrice, record.Currency, record.LedgerRef);
        }

        private D13FarmingResult Success(FactEvent fact, D13FarmingPlotState state, D13FarmingCropDefinition crop,
            D13FarmingEcologyEnvironmentState environment, int quantity, string outputItemId,
            int inputQuantity, long unitPrice, long totalPrice, string currency = null, string ledgerRef = null)
        {
            return new D13FarmingResult
            {
                Succeeded = true,
                Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode,
                Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary,
                EventId = fact == null ? string.Empty : fact.EventId,
                Plot = state == null ? null : state.Clone(),
                Crop = crop == null ? null : crop.Clone(),
                Environment = environment == null ? null : environment.Clone(),
                Quantity = quantity,
                InputQuantity = inputQuantity,
                OutputItemId = outputItemId,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                Currency = currency,
                LedgerRef = ledgerRef,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private D13FarmingResult Duplicate(FactEvent fact, string plotId)
        {
            return new D13FarmingResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate",
                Message = "同一农业操作已结算，未重复扣除种子、增加收成、加工或销售。",
                EventId = fact == null ? string.Empty : fact.EventId,
                Plot = FindPlotState(plotId),
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private D13FarmingResult Failure(D13FarmingOperationRequest request, string eventId, string code,
            string message, string route, D13FarmingPlotState state = null, D13FarmingCropDefinition crop = null)
        {
            var fact = AppendEvent(eventId, request, code ?? "farming_failed", message ?? "农业操作失败。", new Dictionary<string, string>
            {
                ["failure_code"] = code ?? "farming_failed", ["recovery_route"] = route ?? D13FarmingIds.CropRecoveryRoute,
                ["plot_id"] = request == null ? "" : request.PlotId ?? "", ["crop_id"] = crop == null ? "" : crop.CropId,
            }, WorldEventStatus.Failed, route);
            return new D13FarmingResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                EventId = fact == null ? eventId : fact.EventId,
                RecoveryRouteId = route,
                Plot = state == null ? null : state.Clone(),
                Crop = crop == null ? null : crop.Clone(),
                Fact = fact,
            };
        }

        private D13FarmingResult Invalid(string code, string message, string route)
        {
            return new D13FarmingResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route };
        }

        private D13FarmingLandDecision ValidateLand(D13FarmingPlotDefinition plot, D13FarmingOperationRequest request)
        {
            return _landAuthority.ValidatePlot(plot, request == null ? D13FarmingIds.PlayerRef : request.ActorRef, request == null ? 0 : request.WorldDay);
        }

        private bool SourcesExist(string propertySource, string legalSource, long saveRevision)
        {
            if (!string.IsNullOrWhiteSpace(propertySource) && FindAnyFact(propertySource, saveRevision) == null) return false;
            if (!string.IsNullOrWhiteSpace(legalSource) && FindAnyFact(legalSource, saveRevision) == null) return false;
            return !string.IsNullOrWhiteSpace(propertySource) || !string.IsNullOrWhiteSpace(legalSource);
        }

        private D13FarmingEnvironmentResult ApplyEcology(D13FarmingOperationRequest request, string eventId,
            string plotId, D13FarmingEcologyOperation operation, string cropId, int amount, int days,
            bool storm, int stormIntensity, bool rainfall)
        {
            return _ecology.Apply(new D13FarmingEnvironmentRequest
            {
                PlotId = plotId,
                Operation = operation,
                CropId = cropId,
                Amount = amount,
                Days = days,
                StormIntensity = stormIntensity,
                Rainfall = rainfall,
                WorldDay = request == null ? 0 : request.WorldDay,
                Seed = request is D13FarmingAdvanceRequest ? ((D13FarmingAdvanceRequest)request).Seed : 0,
                EventId = eventId + "_ecology",
                SaveRevision = request == null ? 0 : request.SaveRevision,
            }, _eventLog);
        }

        private bool TryPlotAndCrop(string plotId, string cropId, out D13FarmingPlotDefinition plot, out D13FarmingCropDefinition crop)
        {
            _definitions.TryGetValue(plotId ?? string.Empty, out plot);
            _crops.TryGetValue(cropId ?? string.Empty, out crop);
            return plot != null && crop != null;
        }

        private bool TryActive(string plotId, out D13FarmingPlotDefinition plot, out D13FarmingPlotState state, out D13FarmingCropDefinition crop)
        {
            plot = null;
            state = null;
            crop = null;
            if (!_definitions.TryGetValue(plotId ?? string.Empty, out plot) || !_states.TryGetValue(plotId ?? string.Empty, out state) || state.State == D13FarmingCropStateKind.Empty || string.IsNullOrWhiteSpace(state.CropId)) return false;
            _crops.TryGetValue(state.CropId, out crop);
            return crop != null;
        }

        private static D13FarmingPlotState EmptyState(D13FarmingPlotDefinition definition)
        {
            return new D13FarmingPlotState { PlotId = definition.PlotId, PropertyId = definition.PropertyId, State = D13FarmingCropStateKind.Empty, PlantedSeason = D13FarmingSeason.Dry, Health = 100 };
        }

        private static void ClearCrop(D13FarmingPlotState state, string eventId, int day)
        {
            state.PlantingId = null;
            state.CropId = null;
            state.SeedItemId = null;
            state.State = D13FarmingCropStateKind.Empty;
            state.GrowthPoints = 0;
            state.GrowthPercent = 0;
            state.WaterApplied = 0;
            state.LaborApplied = 0;
            state.PestPressure = 0;
            state.StormDamage = 0;
            state.Health = 100;
            Touch(state, eventId, day);
        }

        private static void Touch(D13FarmingPlotState state, string eventId, int day)
        {
            state.Revision++;
            state.LastEventId = eventId;
            state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, day));
        }

        private FactEvent AppendEvent(string eventId, D13FarmingOperationRequest request, string resultCode,
            string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal,
            string recoveryRoute = null)
        {
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request == null ? 0 : request.WorldDay), request == null || string.IsNullOrWhiteSpace(request.LocationRef) ? "island_saint_marys" : request.LocationRef, D13FarmingIds.SystemOwner,
                new EventResult { ResultCode = resultCode ?? "farming_result", Summary = summary ?? string.Empty, FactIds = new List<string> { "farming_fact:" + eventId }, Values = values ?? new Dictionary<string, string>() },
                new[] { request == null || string.IsNullOrWhiteSpace(request.ActorRef) ? D13FarmingIds.PlayerRef : request.ActorRef },
                new[] { "farming:canonical" }, FilterCauses(request == null ? null : request.CauseEventIds, request == null ? 0 : request.SaveRevision), request == null ? 0 : Math.Max(0L, request.SaveRevision), status,
                status == WorldEventStatus.Failed ? new RecoveryRoute { RouteId = recoveryRoute ?? D13FarmingIds.CropRecoveryRoute, Action = "review_and_retry", ResumeFromEventId = eventId, Description = summary ?? "补充条件后重试农业操作。" } : null);
            var append = _eventLog.Append(worldEvent);
            _appliedEventKeys.Add(worldEvent.DedupeKey);
            return append.Fact;
        }

        private FactEvent Existing(string eventId, long saveRevision)
        {
            long normalizedRevision = Math.Max(0L, saveRevision);
            var fact = _eventLog.Find(eventId, normalizedRevision);
            if (fact != null) return fact;
            string key = (eventId ?? string.Empty) + "\u001f" + normalizedRevision.ToString(CultureInfo.InvariantCulture);
            if (!_appliedEventKeys.Contains(key)) return null;
            return new FactEvent
            {
                EventId = eventId,
                SaveRevision = normalizedRevision,
                Result = new EventResult { ResultCode = "duplicate", Summary = "农业操作已在此前存档中结算。" },
            };
        }

        private FactEvent FindAnyFact(string eventId, long saveRevision)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            var exact = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            return exact ?? _eventLog.Facts.FirstOrDefault(item => item.EventId == eventId);
        }

        private IEnumerable<string> FilterCauses(IEnumerable<string> causes, long saveRevision)
        {
            return (causes ?? Enumerable.Empty<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Where(item => FindAnyFact(item, saveRevision) != null).Distinct(StringComparer.Ordinal);
        }

        private static string EventId(string operationId, string fallback)
        {
            return "event_d13_farming_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "none";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++) if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_';
            return new string(chars);
        }

        public static D13FarmingSeason SeasonFor(int worldDay)
        {
            int phase = Math.Max(0, worldDay) % 120;
            return phase >= 100 ? D13FarmingSeason.Hurricane : phase >= 60 ? D13FarmingSeason.Wet : D13FarmingSeason.Dry;
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }

        private static int SafeAdd(int left, int right)
        {
            long result = (long)Math.Max(0, left) + Math.Max(0, right);
            return result > int.MaxValue ? int.MaxValue : (int)result;
        }

        private sealed class UnavailableLandAuthority : ID13FarmingLandAuthority
        {
            public D13FarmingLandDecision ValidatePlot(D13FarmingPlotDefinition definition, string actorRef, int worldDay)
            {
                return new D13FarmingLandDecision { Allowed = false, Code = "property_authority_required", Message = "必须由 PropertySystem 提供地块经营资格。" };
            }
        }

        private sealed class UnavailableEconomyAuthority : ID13FarmingEconomyAuthority
        {
            public D13FarmingSaleDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision)
            {
                return new D13FarmingSaleDecision { Allowed = false, Code = "economy_authority_required", Message = "必须由 EconomySystem 裁决农业销售价格。" };
            }
        }
    }

}
