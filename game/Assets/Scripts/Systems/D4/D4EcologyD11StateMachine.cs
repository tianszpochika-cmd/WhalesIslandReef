// ============================================================
// D4EcologyD11StateMachine —— D11-A01 生态确定性状态机
//
// 这是 D4EcologySystem 的扩展分部：不建立第二套 EcologySystem。
// EcologySystem 仍是鱼群、植被、动物和生境运行时事实的唯一写者；
// EnvironmentSystem 的 D9 天气/海况快照只作为只读输入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D4
{
    public sealed partial class D4EcologySystem
    {
        private const string EcologyOwner = "EcologySystem";
        private const float BaselineTemperatureC = 27f;

        /// <summary>项目自有代表植被目录；数量只在 EcologySystem 的运行时状态中变化。</summary>
        public static IReadOnlyList<PlantResourceDefinition> D11PlantCatalog
        {
            get { return D11EcologyCatalog.Plants; }
        }

        /// <summary>恢复旧档/空档时补齐 D11 字段，不重置已有运行时事实。</summary>
        private void InitializeD11State()
        {
            if (_state.Habitat == null) _state.Habitat = new HabitatConditionState();
            if (_state.Plants == null) _state.Plants = new List<PlantResourceState>();
            if (_state.AppliedEcologyEventKeys == null)
                _state.AppliedEcologyEventKeys = new List<string>();
            if (string.IsNullOrEmpty(_state.Habitat.HabitatId))
                _state.Habitat.HabitatId = CanonicalHabitat(_state.HabitatId);
            else if (!string.IsNullOrEmpty(_state.HabitatId) &&
                _state.Habitat.HabitatId == "nearshore_reef_edge" &&
                CanonicalHabitat(_state.HabitatId) != "nearshore_reef_edge")
                _state.Habitat.HabitatId = CanonicalHabitat(_state.HabitatId);
            if (string.IsNullOrEmpty(_state.Habitat.BiomeId))
                _state.Habitat.BiomeId = BiomeFor(_state.Habitat.HabitatId);
            _state.Habitat.Pollution = Clamp01(_state.Habitat.Pollution);
            _state.Habitat.Turbidity = Clamp01(_state.Habitat.Turbidity);
            _state.Habitat.TemperatureC = FiniteOr(_state.Habitat.TemperatureC, BaselineTemperatureC);
            _state.Habitat.TidePhase = Clamp01(_state.Habitat.TidePhase);
            _state.Habitat.CurrentSpeed = Math.Max(0f, FiniteOr(_state.Habitat.CurrentSpeed, 0.20f));
            _state.Habitat.CurrentDirectionDegrees = NormalizeDegrees(
                FiniteOr(_state.Habitat.CurrentDirectionDegrees, 90f));
        }

        /// <summary>建立一个代表生境；仅由 EcologySystem 写入初始资源状态。</summary>
        private void InitializeD11State(int day, int seed)
        {
            InitializeD11State();
            string habitatId = CanonicalHabitat(_state.HabitatId);
            _state.Habitat = new HabitatConditionState
            {
                HabitatId = habitatId,
                BiomeId = BiomeFor(habitatId),
                TemperatureC = BaselineTemperatureC,
                Turbidity = habitatId == "mangrove" ? 0.22f : 0.10f,
                Pollution = 0f,
                TidePhase = TidePhaseFor(day),
                CurrentSpeed = habitatId == "mangrove" ? 0.15f : 0.20f,
                CurrentDirectionDegrees = 90f,
                LastUpdatedDay = Math.Max(0, day),
                Revision = 0,
            };
            _state.Plants.Clear();
            foreach (var definition in D11EcologyCatalog.Plants)
            {
                if (!PlantMatches(definition, habitatId)) continue;
                int initial = Math.Max(0, definition.Capacity -
                    D11Stable(definition.Id + ":plant:" + day + ":" + seed) % 2);
                _state.Plants.Add(new PlantResourceState
                {
                    PlantId = definition.Id,
                    HabitatId = habitatId,
                    ResourceKind = definition.ResourceKind ?? "plant",
                    Quantity = initial,
                    Capacity = Math.Max(1, definition.Capacity),
                    HarvestPressure = 0,
                    RecoveryRatePerDay = Math.Max(1, definition.RecoveryRatePerDay),
                    LastUpdatedDay = Math.Max(0, day),
                });
            }
            _state.LastEcologyDay = Math.Max(0, day);
            _state.EcologyRevision = 0;
            _state.LastEcologyEventId = "";
            _state.AppliedEcologyEventKeys.Clear();
        }

        public HabitatConditionState FindHabitatConditionSnapshot()
        {
            InitializeD11State();
            return _state.Habitat.Clone();
        }

        public PlantResourceState FindPlantSnapshot(string plantId)
        {
            if (string.IsNullOrEmpty(plantId) || _state.Plants == null) return null;
            foreach (var plant in _state.Plants)
                if (plant != null && string.Equals(plant.PlantId, plantId, StringComparison.Ordinal))
                    return plant.Clone();
            return null;
        }

        /// <summary>为旧档或未经过 SeedHabitat 的运行时补齐 D11 浅水资源，不重置已有数量。</summary>
        public void EnsureD11ShallowResources(int day, int seed)
        {
            InitializeD11State();
            string habitatId = CanonicalHabitat(_state.HabitatId);
            foreach (var definition in D11EcologyCatalog.Plants)
            {
                if (definition == null || definition.ResourceKind == "plant" || !PlantMatches(definition, habitatId)) continue;
                if (FindPlant(definition.Id) != null) continue;
                _state.Plants.Add(new PlantResourceState
                {
                    PlantId = definition.Id,
                    HabitatId = habitatId,
                    ResourceKind = definition.ResourceKind,
                    Quantity = Math.Max(1, definition.Capacity),
                    Capacity = Math.Max(1, definition.Capacity),
                    RecoveryRatePerDay = Math.Max(1, definition.RecoveryRatePerDay),
                    LastUpdatedDay = Math.Max(0, day),
                });
            }
        }

        /// <summary>
        /// 读取统一 D9 天气/海况快照并计算潮汐、海流、温度和浑浊度。
        /// 这是只读计算，不会写 EnvironmentSystem 或 EcologyState。
        /// </summary>
        public D11EcologyConditions ReadConditions(D9WeatherSeaSnapshot weather, int worldDay)
        {
            var safe = weather == null ? D9WeatherSeaSnapshot.CreateDefault() : weather.Clone();
            var fish = D9EcologyWeatherReader.ReadFishConditions(safe);
            int day = Math.Max(0, worldDay);
            float tide = TidePhaseFor(day);
            float tideWave = (float)Math.Abs(Math.Sin(tide * Math.PI * 2d));
            float tidalCurrent = 0.12f + tideWave * 0.28f;
            float current = Math.Max(0f, safe.CurrentSpeed + tidalCurrent);
            float direction = NormalizeDegrees(safe.CurrentDirectionDegrees + (tide >= 0.5f ? 180f : 0f));
            float weatherTemperatureOffset = safe.Weather == D9WeatherState.Storm ? -1.0f :
                safe.Weather == D9WeatherState.Rain ? -0.4f : 0f;
            float temperature = BaselineTemperatureC + weatherTemperatureOffset +
                (tideWave - 0.5f) * 1.5f;
            var habitat = _state.Habitat ?? new HabitatConditionState();
            float pollution = Clamp01(habitat.Pollution);
            float turbidity = Clamp01(0.08f + (1f - safe.Visibility) * 0.45f +
                current * 0.10f + pollution * 0.35f);
            float temperatureSuitability = Clamp01(1f - Math.Abs(temperature - BaselineTemperatureC) / 8f);
            float tideMultiplier = 0.75f + tideWave * 0.25f;
            float fishRecovery = Clamp(0.45f + fish.ActivityMultiplier * 0.35f +
                temperatureSuitability * 0.20f + tideMultiplier * 0.20f -
                turbidity * 0.30f - pollution * 0.55f, 0.05f, 1.50f);
            float plantGrowth = Clamp(0.35f + temperatureSuitability * 0.45f +
                (1f - turbidity) * 0.25f + tideMultiplier * 0.15f - pollution * 0.60f,
                0.05f, 1.50f);
            float animalHealth = Clamp(1f - pollution * 0.60f - turbidity * 0.25f -
                (safe.Weather == D9WeatherState.Storm ? 0.15f : 0f), 0.10f, 1f);
            return new D11EcologyConditions
            {
                WeatherStateId = safe.Weather.ToString(),
                WorldDay = day,
                TidePhase = tide,
                CurrentSpeed = current,
                CurrentDirectionDegrees = direction,
                TemperatureC = temperature,
                Turbidity = turbidity,
                Pollution = pollution,
                FishActivityMultiplier = fish.ActivityMultiplier,
                FishCatchMultiplier = fish.CatchMultiplier,
                FishRecoveryMultiplier = fishRecovery,
                PlantGrowthMultiplier = plantGrowth,
                AnimalHealthMultiplier = animalHealth,
                ObservationBlocked = fish.ObservationBlocked,
            };
        }

        /// <summary>
        /// 按世界日推进生态。鱼群恢复、压力衰减、植物恢复和动物状态均在此唯一写入。
        /// </summary>
        public D11EcologyOperationResult AdvanceEcology(int worldDay, D9WeatherSeaSnapshot weather,
            int seed, EventLog eventLog = null, long saveRevision = 0, string eventId = null)
        {
            InitializeD11State();
            int day = Math.Max(0, worldDay);
            string id = string.IsNullOrWhiteSpace(eventId)
                ? "event_d11_ecology_advance_day_" + day.ToString(CultureInfo.InvariantCulture)
                : eventId;
            if (IsDuplicate(id, saveRevision, eventLog))
                return DuplicateResult(id, day, "生态推进事实已经结算，未重复恢复资源。");
            if (day < _state.LastEcologyDay)
                return RejectResult("ecology_day_rewind", "生态世界日不能倒退。", id, day);

            int days = day - _state.LastEcologyDay;
            var conditions = ReadConditions(weather, day);
            float recovery = conditions.FishRecoveryMultiplier;
            foreach (var plant in _state.Plants)
            {
                if (plant == null) continue;
                int growth = (int)Math.Floor(Math.Max(0f, days * plant.RecoveryRatePerDay *
                    conditions.PlantGrowthMultiplier));
                plant.Quantity = Math.Min(Math.Max(1, plant.Capacity), plant.Quantity + growth);
                plant.HarvestPressure = Math.Max(0, plant.HarvestPressure - days);
                plant.LastUpdatedDay = day;
            }

            foreach (var school in _state.FishSchools)
            {
                if (school == null) continue;
                school.Recovery = Math.Max(0f, school.Recovery + days * school.RecoveryRatePerDay * recovery);
                int biomassGain = (int)Math.Floor(school.Recovery);
                if (biomassGain > 0)
                    school.Recovery -= biomassGain;
                int cap = Math.Max(1, school.Capacity <= 0 ? 12 : school.Capacity);
                school.Biomass = Math.Min(cap, Math.Max(0, school.Biomass + biomassGain));
                school.FishingPressure = Math.Max(0, school.FishingPressure -
                    (int)Math.Round(days * (0.75f + recovery)));
                MoveFishSchool(school.FishSchoolId, day,
                    seed + (int)Math.Round(conditions.CurrentSpeed * 100f));
            }

            float pollutionRecovery = days * 0.02f * recovery;
            _state.Habitat.Pollution = Clamp01(_state.Habitat.Pollution - pollutionRecovery);
            _state.Habitat.HabitatId = CanonicalHabitat(_state.HabitatId);
            _state.Habitat.BiomeId = BiomeFor(_state.Habitat.HabitatId);
            _state.Habitat.TemperatureC = conditions.TemperatureC;
            _state.Habitat.Turbidity = conditions.Turbidity;
            _state.Habitat.TidePhase = conditions.TidePhase;
            _state.Habitat.CurrentSpeed = conditions.CurrentSpeed;
            _state.Habitat.CurrentDirectionDegrees = conditions.CurrentDirectionDegrees;
            _state.Habitat.LastUpdatedDay = day;

            foreach (var animal in _state.Animals)
            {
                if (animal == null) continue;
                if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                    animal.Lifecycle == D11AnimalLifecycleState.Collected) continue;
                var definition = D4AnimalCatalog.Find(animal.AnimalId);
                if (definition == null) continue;
                animal.HabitatId = _state.Habitat.HabitatId;
                animal.EcologyStress = Clamp01(_state.Habitat.Pollution * 0.65f +
                    conditions.Turbidity * 0.25f + (1f - conditions.AnimalHealthMultiplier) * 0.30f);
                if (animal.EcologyStress > 0.55f)
                {
                    animal.Behavior = D4AnimalBehavior.Alert;
                    animal.Health = (int)Whalebone.Game.Config.NumericConfiguration.ClampHealth(
                        animal.Health - Math.Max(1, (int)Math.Round(animal.EcologyStress)));
                }
                else
                {
                    animal.Behavior = BehaviorFor(definition, day,
                        seed + (int)(_state.EcologyRevision % int.MaxValue));
                    animal.Health = (int)Whalebone.Game.Config.NumericConfiguration.ClampHealth(
                        animal.Health + (days > 0 ? 1 : 0));
                }
                animal.LastUpdatedDay = day;
            }

            _state.LastEcologyDay = day;
            _state.EcologyRevision++;
            _state.Habitat.Revision = _state.EcologyRevision;
            _state.LastEcologyEventId = id;
            RememberEvent(id, saveRevision);
            AppendEvent(eventLog, id, day, saveRevision, "ecology_advanced",
                "生态已按天气、潮汐、海流和污染确定性推进。",
                new[] { _state.Habitat.HabitatId },
                new[] { "habitat:" + _state.Habitat.HabitatId, "weather:" + conditions.WeatherStateId },
                new Dictionary<string, string>
                {
                    ["temperature_c"] = Format(conditions.TemperatureC),
                    ["turbidity"] = Format(conditions.Turbidity),
                    ["pollution"] = Format(_state.Habitat.Pollution),
                    ["tide_phase"] = Format(conditions.TidePhase),
                    ["current_speed"] = Format(conditions.CurrentSpeed),
                    ["days_applied"] = days.ToString(CultureInfo.InvariantCulture),
                });
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ecology_advanced",
                Message = "生态状态已推进。",
                EventId = id,
                WorldDay = day,
                DaysApplied = days,
                PollutionAfter = _state.Habitat.Pollution,
                Conditions = conditions,
            };
        }

        /// <summary>从捕捞、季节和海况计算鱼获预览；不改变鱼群状态。</summary>
        public FishCatchPreview PreviewFishCatch(string fishSchoolId, string fishingSpotId,
            string fishItemId, int gearCapacity, int day, int seed, D9WeatherSeaSnapshot weather)
        {
            var school = FindFishSchool(fishSchoolId);
            if (school == null) return RejectCatch("fish_school_missing", "目标鱼群不存在。");
            if (!string.Equals(school.FishingSpotId, fishingSpotId, StringComparison.Ordinal))
                return RejectCatch("wrong_fishing_spot", "鱼群不在当前捕捞点。");
            if (school.Biomass <= 0) return RejectCatch("fish_school_depleted", "这片鱼群暂时离开或已经枯竭。");
            if (gearCapacity <= 0) return RejectCatch("gear_invalid", "渔具无法承载鱼获。");

            var conditions = ReadConditions(weather, day);
            int deterministic = D11Stable(fishSchoolId + ":" + fishingSpotId + ":" + day + ":" + seed);
            int baseQuantity = 1 + deterministic % Math.Max(1, Math.Min(gearCapacity, 3));
            float pressureMultiplier = Clamp(1f - school.FishingPressure / 12f, 0.25f, 1f);
            float tideMultiplier = 0.75f + Math.Abs((float)Math.Sin(conditions.TidePhase * Math.PI * 2d)) * 0.25f;
            float adjusted = baseQuantity * pressureMultiplier * conditions.FishCatchMultiplier * tideMultiplier;
            int quantity = Math.Max(1, (int)Math.Floor(adjusted));
            quantity = Math.Min(quantity, school.Biomass);
            return new FishCatchPreview
            {
                Accepted = quantity > 0,
                Code = quantity > 0 ? "preview" : "fish_school_depleted",
                Message = quantity > 0 ? "生态系统已按捕捞压力和海况计算鱼获。" : "鱼群没有可结算的生物量。",
                FishSchoolId = fishSchoolId,
                FishingSpotId = fishingSpotId,
                FishItemId = fishItemId,
                Quantity = quantity,
                Day = Math.Max(0, day),
                CatchMultiplier = conditions.FishCatchMultiplier,
                PressureMultiplier = pressureMultiplier,
                TideMultiplier = tideMultiplier,
                FishingPressureAfter = school.FishingPressure + quantity,
                EcologyRevision = _state.EcologyRevision,
            };
        }

        /// <summary>植被采集请求的唯一裁决入口；UI/Agent 只能提出 plantId 与意图数量。</summary>
        public D11EcologyOperationResult HarvestPlant(string plantId, int requestedQuantity, int day,
            int seed, EventLog eventLog = null, long saveRevision = 0, string eventId = null,
            D11PlantHarvestContext context = null)
        {
            InitializeD11State();
            var plant = FindPlant(plantId);
            if (plant == null) return RejectResult("plant_missing", "目标植被不存在。", eventId, day);
            if (requestedQuantity <= 0) return RejectResult("invalid_quantity", "采集数量必须大于零。", eventId, day);
            var definition = D11EcologyCatalog.FindPlant(plantId);
            if (definition == null) return RejectResult("plant_definition_missing", "植被定义缺少正式内容来源。", eventId, day);
            string id = string.IsNullOrWhiteSpace(eventId)
                ? "event_d11_plant_harvest_" + plantId + "_" + Math.Max(0, day) : eventId;
            if (IsDuplicate(id, saveRevision, eventLog))
                return DuplicateResult(id, day, "植被采集事实已经结算，未重复扣减资源。");
            int limit = Math.Max(1, definition.HarvestLimitPerAction);
            int quantity = Math.Min(Math.Min(requestedQuantity, limit), plant.Quantity);
            if (quantity <= 0) return RejectResult("plant_depleted", "该植被当前没有可采集数量。", id, day);
            plant.Quantity -= quantity;
            plant.HarvestPressure += quantity;
            plant.ResourceKind = definition.ResourceKind ?? plant.ResourceKind ?? "plant";
            plant.LastUpdatedDay = Math.Max(plant.LastUpdatedDay, Math.Max(0, day));
            _state.EcologyRevision++;
            _state.LastEcologyEventId = id;
            RememberEvent(id, saveRevision);
            var values = new Dictionary<string, string>
            {
                ["plant_id"] = plant.PlantId,
                ["resource_kind"] = plant.ResourceKind ?? "plant",
                ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["remaining"] = plant.Quantity.ToString(CultureInfo.InvariantCulture),
            };
            AppendPlantContext(values, context);
            AppendEvent(eventLog, id, Math.Max(0, day), saveRevision, "plant_harvested",
                "植被采集已由生态系统结算。", new[] { plant.PlantId },
                new[] { "habitat:" + _state.Habitat.HabitatId },
                values);
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = true,
                Code = "plant_harvested",
                Message = "植被采集已结算。",
                EventId = id,
                WorldDay = Math.Max(0, day),
                Quantity = quantity,
                ResourceId = plant.PlantId,
                ResourceKind = plant.ResourceKind ?? "plant",
                RemainingQuantity = plant.Quantity,
                HarvestPressureAfter = plant.HarvestPressure,
            };
        }

        /// <summary>D11 浅水采集的显式入口；状态仍复用 EcologySystem 的资源唯一写者。</summary>
        public D11EcologyOperationResult HarvestShallowResource(string resourceId, int requestedQuantity,
            int day, int seed, EventLog eventLog = null, long saveRevision = 0, string eventId = null)
        {
            return HarvestPlant(resourceId, requestedQuantity, day, seed, eventLog, saveRevision, eventId);
        }

        /// <summary>供上层采集编排层做提交前去重检查；不暴露可写状态。</summary>
        public bool IsEcologyEventApplied(string eventId, long saveRevision, EventLog eventLog = null)
        {
            InitializeD11State();
            return IsDuplicate(eventId, saveRevision, eventLog);
        }

        /// <summary>污染输入只能进入 EcologySystem，由它确定性改变浑浊度与资源压力。</summary>
        public D11EcologyOperationResult ApplyPollution(string sourceId, float amount, int day,
            EventLog eventLog = null, long saveRevision = 0, string eventId = null)
        {
            InitializeD11State();
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                return RejectResult("invalid_pollution", "污染增量必须是正数。", eventId, day);
            string source = string.IsNullOrWhiteSpace(sourceId) ? "unknown_source" : sourceId;
            string id = string.IsNullOrWhiteSpace(eventId)
                ? "event_d11_pollution_" + source + "_" + Math.Max(0, day) : eventId;
            if (IsDuplicate(id, saveRevision, eventLog))
                return DuplicateResult(id, day, "污染事实已经结算，未重复增加污染压力。");
            float safeAmount = Clamp(amount, 0f, 1f);
            _state.Habitat.Pollution = Clamp01(_state.Habitat.Pollution + safeAmount);
            _state.Habitat.Turbidity = Clamp01(_state.Habitat.Turbidity + safeAmount * 0.35f);
            foreach (var plant in _state.Plants)
            {
                if (plant == null) continue;
                int loss = Math.Min(plant.Quantity, (int)Math.Round(plant.Capacity * safeAmount * 0.05f));
                plant.Quantity -= loss;
                plant.HarvestPressure += loss;
            }
            foreach (var school in _state.FishSchools)
            {
                if (school == null) continue;
                school.FishingPressure += Math.Max(0, (int)Math.Round(safeAmount * 2f));
                school.Recovery = Math.Max(0f, school.Recovery - safeAmount);
            }
            _state.Habitat.LastUpdatedDay = Math.Max(_state.Habitat.LastUpdatedDay, Math.Max(0, day));
            _state.EcologyRevision++;
            _state.Habitat.Revision = _state.EcologyRevision;
            _state.LastEcologyEventId = id;
            RememberEvent(id, saveRevision);
            AppendEvent(eventLog, id, Math.Max(0, day), saveRevision, "pollution_applied",
                "污染已进入生境状态并影响资源压力。", new[] { "pollution:" + source },
                new[] { "habitat:" + _state.Habitat.HabitatId },
                new Dictionary<string, string>
                {
                    ["source_id"] = source,
                    ["amount"] = Format(safeAmount),
                    ["pollution_after"] = Format(_state.Habitat.Pollution),
                    ["turbidity_after"] = Format(_state.Habitat.Turbidity),
                });
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = true,
                Code = "pollution_applied",
                Message = "污染已由 EcologySystem 记录并传导到生境。",
                EventId = id,
                WorldDay = Math.Max(0, day),
                PollutionAfter = _state.Habitat.Pollution,
            };
        }

        /// <summary>明确的恢复动作；仍由 EcologySystem 计算恢复量，不接受外部结果值。</summary>
        public D11EcologyOperationResult RestoreHabitat(int day, D9WeatherSeaSnapshot weather,
            int seed, EventLog eventLog = null, long saveRevision = 0, string eventId = null)
        {
            InitializeD11State();
            string id = string.IsNullOrWhiteSpace(eventId)
                ? "event_d11_ecology_restore_day_" + Math.Max(0, day) : eventId;
            if (IsDuplicate(id, saveRevision, eventLog))
                return DuplicateResult(id, day, "生境恢复事实已经结算，未重复恢复资源。");
            var conditions = ReadConditions(weather, day);
            float recovery = Clamp(0.10f + conditions.FishRecoveryMultiplier * 0.08f -
                conditions.Pollution * 0.05f, 0.02f, 0.25f);
            _state.Habitat.Pollution = Clamp01(_state.Habitat.Pollution - recovery);
            foreach (var plant in _state.Plants)
            {
                if (plant == null) continue;
                int gain = Math.Max(1, (int)Math.Floor(plant.RecoveryRatePerDay *
                    conditions.PlantGrowthMultiplier));
                plant.Quantity = Math.Min(Math.Max(1, plant.Capacity), plant.Quantity + gain);
                plant.HarvestPressure = Math.Max(0, plant.HarvestPressure - gain);
            }
            foreach (var school in _state.FishSchools)
            {
                if (school == null) continue;
                school.Biomass = Math.Min(Math.Max(1, school.Capacity), school.Biomass +
                    Math.Max(1, (int)Math.Floor(school.RecoveryRatePerDay *
                        conditions.FishRecoveryMultiplier)));
                school.FishingPressure = Math.Max(0, school.FishingPressure - 1);
                school.Recovery = Math.Max(0f, school.Recovery - 0.25f);
                MoveFishSchool(school.FishSchoolId, Math.Max(0, day), seed + 7);
            }
            _state.EcologyRevision++;
            _state.Habitat.Revision = _state.EcologyRevision;
            _state.Habitat.LastUpdatedDay = Math.Max(_state.Habitat.LastUpdatedDay, Math.Max(0, day));
            _state.LastEcologyEventId = id;
            RememberEvent(id, saveRevision);
            AppendEvent(eventLog, id, Math.Max(0, day), saveRevision, "ecology_restored",
                "生境恢复已按当前海况确定性结算。", new[] { _state.Habitat.HabitatId },
                new[] { "recovery:deterministic", "habitat:" + _state.Habitat.HabitatId },
                new Dictionary<string, string>
                {
                    ["pollution_after"] = Format(_state.Habitat.Pollution),
                    ["recovery_factor"] = Format(recovery),
                    ["seed"] = seed.ToString(CultureInfo.InvariantCulture),
                });
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ecology_restored",
                Message = "生境恢复已结算。",
                EventId = id,
                WorldDay = Math.Max(0, day),
                PollutionAfter = _state.Habitat.Pollution,
                Conditions = conditions,
            };
        }

        private PlantResourceState FindPlant(string plantId)
        {
            if (string.IsNullOrEmpty(plantId) || _state.Plants == null) return null;
            foreach (var plant in _state.Plants)
                if (plant != null && plant.PlantId == plantId) return plant;
            return null;
        }

        private bool IsDuplicate(string eventId, long saveRevision, EventLog eventLog)
        {
            string key = EventKey(eventId, saveRevision);
            if (_state.AppliedEcologyEventKeys.Contains(key)) return true;
            if (eventLog != null && eventLog.Find(eventId, Math.Max(0L, saveRevision)) != null) return true;
            return false;
        }

        private static void AppendPlantContext(Dictionary<string, string> values,
            D11PlantHarvestContext context)
        {
            if (values == null || context == null) return;
            values["item_id"] = context.ItemId ?? string.Empty;
            values["tool_id"] = context.ToolId ?? string.Empty;
            values["maturity_stage"] = context.MaturityStage ?? string.Empty;
            values["quality"] = context.Quality ?? string.Empty;
            values["recognized_plant_id"] = context.RecognizedPlantId ?? string.Empty;
            values["identification_confidence"] = Format(context.IdentificationConfidence);
            values["misidentified"] = context.Misidentified ? "true" : "false";
            values["toxic"] = context.Toxic ? "true" : "false";
            values["output_quantity"] = context.OutputQuantity.ToString(CultureInfo.InvariantCulture);
            values["consequence_id"] = context.ConsequenceId ?? string.Empty;
            values["protection_area_id"] = context.ProtectionAreaId ?? string.Empty;
        }

        private void RememberEvent(string eventId, long saveRevision)
        {
            string key = EventKey(eventId, saveRevision);
            if (!_state.AppliedEcologyEventKeys.Contains(key))
                _state.AppliedEcologyEventKeys.Add(key);
        }

        private static string EventKey(string eventId, long saveRevision)
        {
            return (eventId ?? "") + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture);
        }

        private static void AppendEvent(EventLog eventLog, string eventId, int day, long saveRevision,
            string resultCode, string summary, IEnumerable<string> factIds,
            IEnumerable<string> inputFacts, Dictionary<string, string> values)
        {
            if (eventLog == null) return;
            eventLog.Append(new WorldEvent(eventId, day, "nearshore_saint_marys", EcologyOwner,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string>(factIds ?? new string[0]),
                    Values = values ?? new Dictionary<string, string>(),
                },
                new[] { EcologyOwner }, inputFacts ?? new string[0], new string[0],
                Math.Max(0L, saveRevision)));
        }

        private static D11EcologyOperationResult DuplicateResult(string eventId, int day, string message)
        {
            return new D11EcologyOperationResult
            {
                Succeeded = true,
                Applied = false,
                Duplicate = true,
                Code = "duplicate_ecology_event",
                Message = message,
                EventId = eventId,
                WorldDay = Math.Max(0, day),
            };
        }

        private static D11EcologyOperationResult RejectResult(string code, string message, string eventId, int day)
        {
            return new D11EcologyOperationResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                EventId = eventId ?? "",
                WorldDay = Math.Max(0, day),
            };
        }

        private static bool PlantMatches(PlantResourceDefinition definition, string habitatId)
        {
            if (definition == null) return false;
            if (string.Equals(definition.HabitatId, habitatId, StringComparison.Ordinal)) return true;
            return definition.HabitatId == "coast" &&
                (habitatId == "nearshore_reef_edge" || habitatId == "beach");
        }

        private static string CanonicalHabitat(string habitatId)
        {
            if (string.IsNullOrEmpty(habitatId)) return "nearshore_reef_edge";
            if (habitatId == "reef" || habitatId == "nearshore" || habitatId == "nearshore/reef_edge")
                return "nearshore_reef_edge";
            return habitatId.Replace("/", "_");
        }

        private static string BiomeFor(string habitatId)
        {
            if (habitatId == "mangrove") return "mangrove";
            if (habitatId == "jungle") return "jungle";
            if (habitatId == "rainforest") return "rainforest";
            if (habitatId == "farm") return "managed_land";
            if (habitatId == "dry_shrub") return "dry_shrub";
            if (habitatId == "beach" || habitatId == "coast") return "coast";
            if (habitatId == "open_sea") return "open_sea";
            return "reef";
        }

        private static float TidePhaseFor(int day)
        {
            int cycle = Math.Max(0, day) % 4;
            return cycle / 4f;
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }

        private static float FiniteOr(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
        }

        private static float NormalizeDegrees(float value)
        {
            value = FiniteOr(value, 0f) % 360f;
            return value < 0f ? value + 360f : value;
        }

        private static string Format(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static int D11Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }

    internal static class D11EcologyCatalog
    {
        public static readonly IReadOnlyList<PlantResourceDefinition> Plants =
            new List<PlantResourceDefinition>
            {
                // ID 对应 D8 内容目录的项目自有代表条目 plant_red_mangrove。
                new PlantResourceDefinition
                {
                    Id = "plant_red_mangrove",
                    HabitatId = "mangrove",
                    ResourceKind = "timber",
                    Capacity = 8,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 3,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_orchard_guava_v1",
                    HabitatId = "coast",
                    ResourceKind = "fruit",
                    Capacity = 8,
                    RecoveryRatePerDay = 2,
                    HarvestLimitPerAction = 3,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_farm_cassava_mature",
                    HabitatId = "farm",
                    ResourceKind = "vegetable",
                    Capacity = 7,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 2,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_dry_aloe_medicinal",
                    HabitatId = "dry_shrub",
                    ResourceKind = "medicinal",
                    Capacity = 6,
                    RecoveryRatePerDay = 2,
                    HarvestLimitPerAction = 2,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_coast_agave_fiber",
                    HabitatId = "coast",
                    ResourceKind = "fiber",
                    Capacity = 8,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 3,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_rainforest_mahogany_v1",
                    HabitatId = "rainforest",
                    ResourceKind = "timber",
                    Capacity = 5,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 1,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_rainforest_pine_resin_v1",
                    HabitatId = "rainforest",
                    ResourceKind = "resin",
                    Capacity = 6,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 2,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_farm_indigo_mature",
                    HabitatId = "farm",
                    ResourceKind = "dye",
                    Capacity = 7,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 2,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_dry_datura_toxin_v1",
                    HabitatId = "dry_shrub",
                    ResourceKind = "toxin",
                    Capacity = 5,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 1,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_shallow_seaweed",
                    HabitatId = "coast",
                    ResourceKind = "seaweed",
                    Capacity = 10,
                    RecoveryRatePerDay = 2,
                    HarvestLimitPerAction = 4,
                },
                new PlantResourceDefinition
                {
                    Id = "plant_conch_bed",
                    HabitatId = "coast",
                    ResourceKind = "shellfish",
                    Capacity = 6,
                    RecoveryRatePerDay = 1,
                    HarvestLimitPerAction = 2,
                },
            };

        public static PlantResourceDefinition FindPlant(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var plant in Plants)
                if (plant != null && plant.Id == id) return plant;
            return null;
        }
    }
}
