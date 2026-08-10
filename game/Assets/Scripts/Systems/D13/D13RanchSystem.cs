// D13-A04 RanchSystem。
// 本系统唯一写家禽/牲畜实例的健康、疾病、单周期状态和待产物。
// Inventory/Cargo 写物资，Ecology 写水源，Property 写经营资格，Economy 写价格。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13RanchSystem
    {
        private readonly Dictionary<string, D13RanchFacilityDefinition> _facilities =
            new Dictionary<string, D13RanchFacilityDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13RanchAnimalDefinition> _definitions =
            new Dictionary<string, D13RanchAnimalDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13RanchAnimalState> _animals =
            new Dictionary<string, D13RanchAnimalState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13RanchSaleRecord> _sales =
            new Dictionary<string, D13RanchSaleRecord>(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly D3InventoryState _inventory;
        private readonly ID13RanchWaterAuthority _water;
        private readonly ID13RanchSourceAuthority _sourceAuthority;
        private readonly ID13RanchLandAuthority _landAuthority;
        private readonly ID13RanchEconomyAuthority _economyAuthority;
        private readonly EventLog _eventLog;
        private long _revision;

        public D13RanchSystem(D3InventoryState inventory,
            ID13RanchWaterAuthority water,
            ID13RanchSourceAuthority sourceAuthority,
            ID13RanchLandAuthority landAuthority,
            ID13RanchEconomyAuthority economyAuthority,
            EventLog eventLog = null,
            D13RanchRuntimeSnapshot initial = null,
            IEnumerable<D13RanchFacilityDefinition> facilities = null,
            IEnumerable<D13RanchAnimalDefinition> definitions = null)
        {
            D13RanchConfiguration.ThrowIfInvalid();
            _inventory = inventory;
            _water = water;
            _sourceAuthority = sourceAuthority ?? new UnavailableSourceAuthority();
            _landAuthority = landAuthority ?? new UnavailableLandAuthority();
            _economyAuthority = economyAuthority ?? new UnavailableEconomyAuthority();
            _eventLog = eventLog ?? new EventLog();
            D13RanchCatalog.RegisterInventoryDefinitions();

            var facilitySource = facilities == null ? D13RanchCatalog.Facilities : facilities.ToList();
            foreach (var facility in facilitySource)
            {
                if (facility == null) continue;
                facility.Validate();
                if (_facilities.ContainsKey(facility.RanchId)) throw new InvalidOperationException("Duplicate ranch facility: " + facility.RanchId);
                _facilities[facility.RanchId] = facility.Clone();
                if (_water != null) _water.EnsureRanch(facility, 0);
            }

            var definitionSource = definitions == null ? D13RanchCatalog.Animals : definitions.ToList();
            foreach (var definition in definitionSource)
            {
                if (definition == null) continue;
                definition.Validate();
                if (_definitions.ContainsKey(definition.DefinitionId)) throw new InvalidOperationException("Duplicate ranch animal definition: " + definition.DefinitionId);
                _definitions[definition.DefinitionId] = definition.Clone();
            }
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13RanchFacilityDefinition FindFacility(string ranchId)
        {
            return _facilities.TryGetValue(ranchId ?? string.Empty, out D13RanchFacilityDefinition result) ? result.Clone() : null;
        }

        public D13RanchAnimalDefinition FindDefinition(string definitionId)
        {
            return _definitions.TryGetValue(definitionId ?? string.Empty, out D13RanchAnimalDefinition result) ? result.Clone() : null;
        }

        public D13RanchAnimalState FindAnimal(string animalId)
        {
            return _animals.TryGetValue(animalId ?? string.Empty, out D13RanchAnimalState result) ? result.Clone() : null;
        }

        public D13RanchReadModel GetReadModel()
        {
            var result = new D13RanchReadModel { Revision = _revision };
            foreach (var item in _facilities.Values.OrderBy(item => item.RanchId, StringComparer.Ordinal)) result.Facilities.Add(item.Clone());
            foreach (var item in _definitions.Values.OrderBy(item => item.DefinitionId, StringComparer.Ordinal)) result.AnimalDefinitions.Add(item.Clone());
            foreach (var item in _animals.Values.OrderBy(item => item.AnimalId, StringComparer.Ordinal))
            {
                result.Animals.Add(item.Clone());
                if (item.Lifecycle == D13RanchAnimalLifecycle.Quarantined || item.Lifecycle == D13RanchAnimalLifecycle.Sick || item.Lifecycle == D13RanchAnimalLifecycle.ProductReady)
                {
                    result.Risks.Add(new D13RanchRiskNotice
                    {
                        SubjectId = item.AnimalId,
                        Risk = "健康 " + item.Health + "; 疾病 " + item.Disease + "; 饲料 " + item.FeedUnits + "; 饮水 " + item.WaterUnits + "; 劳力 " + item.LaborUnits + ".",
                        WorstCase = "缺少饲料、饮水、劳力或治疗会使健康降到 0 并永久进入 Dead；待产物不会凭空生成。",
                        RecoveryRoute = item.Disease == D13RanchDisease.None ? D13RanchIds.FeedRecoveryRoute : D13RanchIds.DiseaseRecoveryRoute,
                    });
                }
            }
            foreach (var item in _sales.Values.OrderBy(item => item.SaleId, StringComparer.Ordinal)) result.Sales.Add(item.Clone());
            if (_water != null)
                foreach (var facility in _facilities.Values.OrderBy(item => item.RanchId, StringComparer.Ordinal))
                {
                    var water = _water.FindRanch(facility.RanchId);
                    if (water != null) result.Water.Add(water);
                }
            return result;
        }

        public D13RanchRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13RanchRuntimeSnapshot
            {
                Revision = _revision,
                Water = _water == null ? new D13RanchWaterRuntimeSnapshot() : _water.CaptureRanchSnapshot(),
            };
            foreach (var item in _animals.Values) result.Animals.Add(item.Clone());
            foreach (var item in _sales.Values) result.Sales.Add(item.Clone());
            foreach (var key in _appliedEventKeys) result.AppliedEventKeys.Add(key);
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_ranch_", StringComparison.Ordinal) && !result.AppliedEventKeys.Contains(fact.DedupeKey)) result.AppliedEventKeys.Add(fact.DedupeKey);
            return result;
        }

        public void Restore(D13RanchRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var item in snapshot.Animals)
            {
                if (!_facilities.ContainsKey(item.RanchId)) throw new InvalidOperationException("Unknown ranch in snapshot: " + item.RanchId);
                if (!_definitions.ContainsKey(item.DefinitionId)) throw new InvalidOperationException("Unknown ranch animal definition in snapshot: " + item.DefinitionId);
            }
            foreach (var water in snapshot.Water.Sources)
                if (!_facilities.ContainsKey(water.RanchId)) throw new InvalidOperationException("Unknown ranch water source in snapshot: " + water.RanchId);
            _animals.Clear();
            foreach (var item in snapshot.Animals) _animals[item.AnimalId] = item.Clone();
            _sales.Clear();
            foreach (var item in snapshot.Sales) _sales[item.SaleId] = item.Clone();
            _appliedEventKeys.Clear();
            foreach (var key in snapshot.AppliedEventKeys) _appliedEventKeys.Add(key);
            if (_water != null) _water.RestoreRanchSnapshot(snapshot.Water);
            _revision = snapshot.Revision;
        }

        public D13RanchResult Acquire(D13RanchAcquireRequest request)
        {
            if (request == null) return Invalid("invalid_request", "养殖来源请求不存在。", D13RanchIds.SourceRecoveryRoute);
            string eventId = EventId(request.OperationId, "acquire_" + request.DefinitionId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null);
            if (!TryFacility(request.RanchId, out D13RanchFacilityDefinition facility) || !_definitions.TryGetValue(request.DefinitionId ?? string.Empty, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "ranch_or_definition_missing", "牧场或家禽/牲畜定义不存在。", D13RanchIds.SourceRecoveryRoute);
            if (_landAuthority == null) return Failure(request, eventId, "property_authority_required", "必须由 PropertySystem 提供牧场经营资格。", D13RanchIds.LandRecoveryRoute);
            var land = _landAuthority.ValidateFacility(facility, request.ActorRef, request.WorldDay);
            if (land == null || !land.Allowed) return Failure(request, eventId, land == null ? "property_authority_required" : land.Code, land == null ? "牧场产权资格不可用。" : land.Message, D13RanchIds.LandRecoveryRoute);
            if (!SourcesExist(land.PropertySourceEventId, land.LegalSourceEventId, request.SaveRevision))
                return Failure(request, eventId, "property_source_missing", "牧场缺少 PropertySystem/法律来源事实。", D13RanchIds.LandRecoveryRoute);
            if (_sourceAuthority == null) return Failure(request, eventId, "source_authority_required", "必须由动物来源 authority 提供来源与检疫决定。", D13RanchIds.SourceRecoveryRoute);
            var source = _sourceAuthority.ValidateSource(definition, request.SourceRef, request.SourceEventId, request.ActorRef, request.WorldDay);
            if (source == null || !source.Allowed || !source.InspectionPassed)
                return Failure(request, eventId, source == null ? "source_authority_required" : source.Code, source == null ? "动物来源和检疫 authority 不可用。" : source.Message, D13RanchIds.SourceRecoveryRoute);
            string sourceEventId = string.IsNullOrWhiteSpace(source.SourceEventId) ? request.SourceEventId : source.SourceEventId;
            string inspectionEventId = string.IsNullOrWhiteSpace(source.InspectionEventId) ? request.InspectionEventId : source.InspectionEventId;
            var sourceFact = FindAnyFact(sourceEventId, request.SaveRevision);
            var inspectionFact = FindAnyFact(inspectionEventId, request.SaveRevision);
            if (string.IsNullOrWhiteSpace(sourceEventId) || string.IsNullOrWhiteSpace(inspectionEventId) || sourceFact == null || inspectionFact == null)
                return Failure(request, eventId, "source_fact_missing", "动物来源和检疫事实必须先进入统一 EventLog。", D13RanchIds.SourceRecoveryRoute);
            if (!string.IsNullOrWhiteSpace(source.SourceOwnerSystem) && source.SourceOwnerSystem != D13RanchIds.SourceOwner)
                return Failure(request, eventId, "source_owner_mismatch", "动物来源必须由 AnimalCareSystem/检疫 authority 提供，牧场不能替代来源裁决。", D13RanchIds.SourceRecoveryRoute);
            if (sourceFact.OwnerSystem != D13RanchIds.SourceOwner || inspectionFact.OwnerSystem != D13RanchIds.SourceOwner)
                return Failure(request, eventId, "source_fact_owner_mismatch", "来源和检疫事实必须来自 AnimalCareSystem，不能由 UI 或 Agent 伪造。", D13RanchIds.SourceRecoveryRoute);
            if (!string.IsNullOrWhiteSpace(source.SourceContentId) && source.SourceContentId != definition.SourceContentId)
                return Failure(request, eventId, "source_species_mismatch", "来源物种与养殖定义不匹配。", D13RanchIds.SourceRecoveryRoute);
            if (_water == null) return Failure(request, eventId, "ecology_authority_required", "必须由 EcologySystem 提供牧场水源状态。", D13RanchIds.WaterRecoveryRoute);
            _water.EnsureRanch(facility, request.WorldDay);
            int occupied = OccupiedSpace(facility.RanchId);
            if (occupied + definition.SpaceRequired > facility.SpaceCapacity)
                return Failure(request, eventId, "ranch_space_full", "牧场空间不足，不能生成新的家禽/牲畜实例。", D13RanchIds.SourceRecoveryRoute);
            string animalId = "ranch_animal_" + SafeId(definition.DefinitionId) + "_" + Stable(request.SourceRef);
            if (_animals.ContainsKey(animalId)) return Failure(request, eventId, "source_already_registered", "同一来源已经登记为牧场动物，不能重复生成。", D13RanchIds.SourceRecoveryRoute);
            int quarantineDays = Math.Max(1, source.QuarantineDays <= 0 ? definition.QuarantineDays : source.QuarantineDays);
            var state = new D13RanchAnimalState
            {
                AnimalId = animalId,
                RanchId = facility.RanchId,
                DefinitionId = definition.DefinitionId,
                SourceContentId = definition.SourceContentId,
                SourceRef = request.SourceRef == null ? string.Empty : request.SourceRef.Trim(),
                SourceEventId = sourceEventId,
                InspectionEventId = inspectionEventId,
                Kind = definition.Kind,
                Lifecycle = D13RanchAnimalLifecycle.Quarantined,
                Disease = D13RanchDisease.None,
                Health = 100,
                QuarantineUntilDay = Math.Max(0, request.WorldDay) + quarantineDays,
                CycleStartDay = Math.Max(0, request.WorldDay),
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            var fact = AppendEvent(eventId, request, "ranch_animal_registered", "动物来源、产权资格和检疫事实已核验，实例进入隔离期。", new Dictionary<string, string>
            {
                ["animal_id"] = animalId, ["ranch_id"] = facility.RanchId, ["definition_id"] = definition.DefinitionId,
                ["source_content_id"] = definition.SourceContentId, ["source_ref"] = state.SourceRef,
                ["source_event_id"] = sourceEventId, ["inspection_event_id"] = inspectionEventId,
                ["quarantine_until_day"] = state.QuarantineUntilDay.ToString(CultureInfo.InvariantCulture),
                ["property_source_event_id"] = land.PropertySourceEventId ?? "", ["legal_source_event_id"] = land.LegalSourceEventId ?? "",
            });
            Touch(state, eventId, request.WorldDay);
            _animals[state.AnimalId] = state;
            _revision++;
            return Success(fact, state, definition, 0, null, null);
        }

        public D13RanchResult ReleaseQuarantine(D13RanchQuarantineRequest request)
        {
            if (request == null) return Invalid("invalid_request", "检疫释放请求不存在。", D13RanchIds.QuarantineRecoveryRoute);
            string eventId = EventId(request.OperationId, "release_quarantine_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.SourceRecoveryRoute);
            if (state.Lifecycle != D13RanchAnimalLifecycle.Quarantined)
                return Failure(request, eventId, "quarantine_not_active", "动物当前不在可释放的检疫状态。", D13RanchIds.QuarantineRecoveryRoute, state, definition);
            if (request.WorldDay < state.QuarantineUntilDay)
                return Failure(request, eventId, "quarantine_incomplete", "检疫期尚未结束，不能提前释放。", D13RanchIds.QuarantineRecoveryRoute, state, definition);
            if (state.Disease != D13RanchDisease.None || state.Health < D13RanchConfiguration.Int(D13RanchConfiguration.MinimumHealthyRelease))
                return Failure(request, eventId, "quarantine_health_failed", "检疫发现疾病或健康不足，必须先治疗。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            state.Lifecycle = D13RanchAnimalLifecycle.Healthy;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_quarantine_released", "检疫期完成且健康达标，动物进入可照料状态。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["health"] = state.Health.ToString(CultureInfo.InvariantCulture),
                ["quarantine_until_day"] = state.QuarantineUntilDay.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, definition, 0, null, null);
        }

        public D13RanchResult Feed(D13RanchFeedRequest request)
        {
            if (request == null) return Invalid("invalid_request", "饲料请求不存在。", D13RanchIds.FeedRecoveryRoute);
            string eventId = EventId(request.OperationId, "feed_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.FeedRecoveryRoute);
            if (!CanCare(state)) return Failure(request, eventId, "animal_not_careable", "隔离、待产物、死亡或出售状态不能直接补充饲料。", D13RanchIds.FeedRecoveryRoute, state, definition);
            if (request.Quantity <= 0 || request.Quantity > D13RanchConfiguration.Int(D13RanchConfiguration.MaximumLaborPerAction) * 5)
                return Failure(request, eventId, "feed_amount_invalid", "饲料数量超出确定性操作边界。", D13RanchIds.FeedRecoveryRoute, state, definition);
            string removeError = null;
            if (_inventory == null || !D3InventorySystem.TryRemoveCargo(_inventory, definition.FeedItemId, request.Quantity, out removeError))
                return Failure(request, eventId, removeError ?? "feed_missing", "货舱没有足够的登记饲料。", D13RanchIds.FeedRecoveryRoute, state, definition);
            state.FeedUnits = SafeAdd(state.FeedUnits, request.Quantity);
            state.Health = Math.Min(100, state.Health + Math.Min(4, request.Quantity));
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_feed_applied", "登记饲料已由 Inventory/Cargo 扣除，动物健康由 RanchSystem 更新。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["feed_item_id"] = definition.FeedItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["feed_units"] = state.FeedUnits.ToString(CultureInfo.InvariantCulture), ["health"] = state.Health.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, definition, request.Quantity, null, null);
        }

        public D13RanchResult Water(D13RanchWaterOperationRequest request)
        {
            if (request == null) return Invalid("invalid_request", "饮水请求不存在。", D13RanchIds.WaterRecoveryRoute);
            string eventId = EventId(request.OperationId, "water_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.WaterRecoveryRoute);
            if (!CanCare(state)) return Failure(request, eventId, "animal_not_careable", "当前生命周期不能补充饮水。", D13RanchIds.WaterRecoveryRoute, state, definition);
            if (request.Quantity <= 0 || request.Quantity > 100)
                return Failure(request, eventId, "water_amount_invalid", "饮水数量超出确定性操作边界。", D13RanchIds.WaterRecoveryRoute, state, definition);
            if (!_facilities.TryGetValue(state.RanchId, out D13RanchFacilityDefinition facility) || _water == null)
                return Failure(request, eventId, "ecology_authority_required", "必须由 EcologySystem 提供牧场水源。", D13RanchIds.WaterRecoveryRoute, state, definition);
            var water = _water.ConsumeWater(new D13RanchWaterRequest
            {
                RanchId = facility.RanchId,
                SourceId = facility.WaterSourceId,
                Amount = request.Quantity,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                EventId = eventId + "_ecology",
            }, _eventLog);
            if (water == null || !water.Succeeded || water.Duplicate)
                return water != null && water.Duplicate ? Duplicate(water.Fact, request.AnimalId) : Failure(request, eventId, water == null ? "water_authority_required" : water.Code, water == null ? "饮水 authority 不可用。" : water.Message, D13RanchIds.WaterRecoveryRoute, state, definition);
            state.WaterUnits = SafeAdd(state.WaterUnits, water.Quantity);
            state.Health = Math.Min(100, state.Health + 1);
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_water_applied", "登记水源饮水已由 EcologySystem 扣除，动物状态由 RanchSystem 更新。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["ranch_id"] = state.RanchId, ["quantity"] = water.Quantity.ToString(CultureInfo.InvariantCulture),
                ["water_source_id"] = facility.WaterSourceId, ["water_ecology_event_id"] = water.EventId,
            });
            _revision++;
            return Success(fact, state, definition, water.Quantity, null, water.State);
        }

        public D13RanchResult AssignLabor(D13RanchLaborRequest request)
        {
            if (request == null) return Invalid("invalid_request", "劳力请求不存在。", D13RanchIds.FeedRecoveryRoute);
            string eventId = EventId(request.OperationId, "labor_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.FeedRecoveryRoute);
            if (!CanCare(state)) return Failure(request, eventId, "animal_not_careable", "当前生命周期不能分配劳力。", D13RanchIds.FeedRecoveryRoute, state, definition);
            if (request.Quantity <= 0 || request.Quantity > D13RanchConfiguration.Int(D13RanchConfiguration.MaximumLaborPerAction))
                return Failure(request, eventId, "labor_amount_invalid", "劳力数量超出确定性操作边界。", D13RanchIds.FeedRecoveryRoute, state, definition);
            state.LaborUnits = SafeAdd(state.LaborUnits, request.Quantity);
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_labor_assigned", "劳力已登记；单周期结算时检查劳力门槛。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["labor_units"] = state.LaborUnits.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, definition, request.Quantity, null, null);
        }

        public D13RanchResult AdvanceCycle(D13RanchCycleRequest request)
        {
            if (request == null) return Invalid("invalid_request", "养殖周期请求不存在。", D13RanchIds.FeedRecoveryRoute);
            string eventId = EventId(request.OperationId, "cycle_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.FeedRecoveryRoute);
            if (state.Lifecycle == D13RanchAnimalLifecycle.Dead) return Failure(request, eventId, "animal_dead", "死亡动物保留记录但不能继续产出。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            if (state.Lifecycle == D13RanchAnimalLifecycle.Quarantined) return Failure(request, eventId, "quarantine_incomplete", "检疫未完成，不能推进产出周期。", D13RanchIds.QuarantineRecoveryRoute, state, definition);
            if (state.Lifecycle == D13RanchAnimalLifecycle.ProductReady) return Failure(request, eventId, "product_pending", "上一周期产物尚未领取，不能开始下一周期。", D13RanchIds.InventoryRecoveryRoute, state, definition);
            if (state.Disease != D13RanchDisease.None) return Failure(request, eventId, "animal_sick", "动物患病，必须先完成治疗。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            int missing = 0;
            if (state.FeedUnits < definition.FeedPerCycle) missing++;
            if (state.WaterUnits < definition.WaterPerCycle) missing++;
            if (state.LaborUnits < definition.LaborRequired) missing++;
            if (missing > 0)
            {
                ApplyHealthLoss(state, missing * D13RanchConfiguration.Int(D13RanchConfiguration.MissingInputHealthLoss), request.WorldDay);
                var blockedFact = AppendEvent(eventId, request, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? "animal_died" : "ranch_cycle_blocked", "饲料、饮水或劳力不足，单周期没有生成产物；健康后果已写入动物状态。", new Dictionary<string, string>
                {
                    ["animal_id"] = state.AnimalId, ["missing_categories"] = missing.ToString(CultureInfo.InvariantCulture),
                    ["feed_required"] = definition.FeedPerCycle.ToString(CultureInfo.InvariantCulture), ["feed_applied"] = state.FeedUnits.ToString(CultureInfo.InvariantCulture),
                    ["water_required"] = definition.WaterPerCycle.ToString(CultureInfo.InvariantCulture), ["water_applied"] = state.WaterUnits.ToString(CultureInfo.InvariantCulture),
                    ["labor_required"] = definition.LaborRequired.ToString(CultureInfo.InvariantCulture), ["labor_applied"] = state.LaborUnits.ToString(CultureInfo.InvariantCulture),
                    ["health"] = state.Health.ToString(CultureInfo.InvariantCulture), ["lifecycle"] = state.Lifecycle.ToString(),
                }, WorldEventStatus.Failed, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? D13RanchIds.DiseaseRecoveryRoute : D13RanchIds.FeedRecoveryRoute);
                Touch(state, eventId, request.WorldDay);
                _revision++;
                return FailureResult(blockedFact, state, definition, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? "animal_died" : "ranch_cycle_blocked", state.Lifecycle == D13RanchAnimalLifecycle.Dead ? D13RanchIds.DiseaseRecoveryRoute : D13RanchIds.FeedRecoveryRoute);
            }

            int roll = Stable(definition.DefinitionId + ":" + state.AnimalId + ":" + Math.Max(0, request.WorldDay) + ":" + request.Seed) % 100;
            if (roll < definition.DiseaseRiskPercent)
            {
                state.Disease = DiseaseForRoll(roll);
                state.LastDiseaseEventId = eventId;
                ApplyHealthLoss(state, D13RanchConfiguration.Int(D13RanchConfiguration.DiseaseHealthLoss), request.WorldDay);
                var diseaseFact = AppendEvent(eventId, request, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? "animal_died" : "animal_diseased", "确定性疾病检查未通过，单周期没有生成产物。", new Dictionary<string, string>
                {
                    ["animal_id"] = state.AnimalId, ["disease"] = state.Disease.ToString(), ["disease_roll"] = roll.ToString(CultureInfo.InvariantCulture),
                    ["disease_risk"] = definition.DiseaseRiskPercent.ToString(CultureInfo.InvariantCulture), ["health"] = state.Health.ToString(CultureInfo.InvariantCulture),
                }, WorldEventStatus.Failed, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? D13RanchIds.DiseaseRecoveryRoute : D13RanchIds.DiseaseRecoveryRoute);
                Touch(state, eventId, request.WorldDay);
                _revision++;
                return FailureResult(diseaseFact, state, definition, state.Lifecycle == D13RanchAnimalLifecycle.Dead ? "animal_died" : "animal_diseased", D13RanchIds.DiseaseRecoveryRoute);
            }

            state.FeedUnits -= definition.FeedPerCycle;
            state.WaterUnits -= definition.WaterPerCycle;
            state.LaborUnits -= definition.LaborRequired;
            state.CycleIndex++;
            state.CycleStartDay = Math.Max(0, request.WorldDay);
            state.ProductReadyQuantity = definition.ProductPerCycle;
            state.ProductItemId = definition.ProductItemId;
            state.Lifecycle = D13RanchAnimalLifecycle.ProductReady;
            state.Health = Math.Min(100, state.Health + D13RanchConfiguration.Int(D13RanchConfiguration.ProductHealthBonus));
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_cycle_completed", "单周期饲料、饮水、空间、劳力和疾病检查通过，产物进入待领取状态。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["definition_id"] = definition.DefinitionId, ["product_item_id"] = definition.ProductItemId,
                ["product_kind"] = definition.ProductKind.ToString(), ["quantity"] = definition.ProductPerCycle.ToString(CultureInfo.InvariantCulture),
                ["cycle_index"] = state.CycleIndex.ToString(CultureInfo.InvariantCulture), ["health"] = state.Health.ToString(CultureInfo.InvariantCulture),
            });
            _revision++;
            return Success(fact, state, definition, definition.ProductPerCycle, definition.ProductItemId, null);
        }

        public D13RanchResult CollectProduct(D13RanchCollectProductRequest request)
        {
            if (request == null) return Invalid("invalid_request", "产物领取请求不存在。", D13RanchIds.InventoryRecoveryRoute);
            string eventId = EventId(request.OperationId, "collect_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.InventoryRecoveryRoute);
            if (state.Lifecycle != D13RanchAnimalLifecycle.ProductReady || state.ProductReadyQuantity <= 0 || string.IsNullOrWhiteSpace(state.ProductItemId))
                return Failure(request, eventId, "product_not_ready", "动物当前没有待领取产物。", D13RanchIds.InventoryRecoveryRoute, state, definition);
            string capacityError = null;
            if (_inventory == null || !D3InventorySystem.CanAddCargo(_inventory, state.ProductItemId, state.ProductReadyQuantity, out capacityError))
                return Failure(request, eventId, capacityError ?? "cargo_full", "产物无法装入货舱，待产物状态保留。", D13RanchIds.InventoryRecoveryRoute, state, definition);
            int quantity = state.ProductReadyQuantity;
            string productItemId = state.ProductItemId;
            if (!D3InventorySystem.TryAddCargo(_inventory, productItemId, quantity, out string addError))
                return Failure(request, eventId, addError, "产物无法写入货舱。", D13RanchIds.InventoryRecoveryRoute, state, definition);
            state.ProductReadyQuantity = 0;
            state.ProductItemId = null;
            state.Lifecycle = D13RanchAnimalLifecycle.Healthy;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_product_collected", "产物已由 Inventory/Cargo 写入货舱，牧场动物进入下一周期准备状态。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["product_item_id"] = productItemId, ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["inventory_owner"] = D13RanchIds.InventoryOwner,
            });
            _revision++;
            return Success(fact, state, definition, quantity, productItemId, null);
        }

        public D13RanchResult TreatDisease(D13RanchDiseaseTreatmentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "疾病治疗请求不存在。", D13RanchIds.DiseaseRecoveryRoute);
            string eventId = EventId(request.OperationId, "treat_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (!TryAnimal(request.AnimalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition))
                return Failure(request, eventId, "animal_missing", "牧场动物实例不存在。", D13RanchIds.DiseaseRecoveryRoute);
            if (state.Lifecycle == D13RanchAnimalLifecycle.Dead) return Failure(request, eventId, "animal_dead", "死亡记录不可治疗或重生。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            if (state.Disease == D13RanchDisease.None) return Failure(request, eventId, "disease_not_present", "动物当前没有登记疾病。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            string removeError = null;
            if (request.Quantity <= 0 || _inventory == null || !D3InventorySystem.TryRemoveCargo(_inventory, D13RanchCatalog.MedicineItemId, request.Quantity, out removeError))
                return Failure(request, eventId, removeError ?? "medicine_missing", "货舱没有足够的检疫治疗剂。", D13RanchIds.DiseaseRecoveryRoute, state, definition);
            state.Disease = D13RanchDisease.None;
            state.Health = Math.Min(100, state.Health + D13RanchConfiguration.Int(D13RanchConfiguration.DiseaseTreatmentHealthGain) * request.Quantity);
            if (state.Health <= 0) ApplyHealthLoss(state, 0, request.WorldDay);
            else state.Lifecycle = D13RanchAnimalLifecycle.Healthy;
            Touch(state, eventId, request.WorldDay);
            var fact = AppendEvent(eventId, request, "ranch_disease_treated", "治疗剂已由 Inventory/Cargo 扣除，疾病和健康由 RanchSystem 更新。", new Dictionary<string, string>
            {
                ["animal_id"] = state.AnimalId, ["medicine_item_id"] = D13RanchCatalog.MedicineItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["health"] = state.Health.ToString(CultureInfo.InvariantCulture), ["lifecycle"] = state.Lifecycle.ToString(),
            });
            _revision++;
            return Success(fact, state, definition, request.Quantity, null, null);
        }

        public D13RanchResult SellProduct(D13RanchSaleRequest request)
        {
            if (request == null) return Invalid("invalid_request", "养殖产品销售请求不存在。", D13RanchIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "sell_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            if (_inventory == null || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.ItemId))
                return Failure(request, eventId, "sale_request_invalid", "销售物品和数量必须有效。", D13RanchIds.EconomyRecoveryRoute);
            var definition = D13RanchCatalog.FindByProduct(request.ItemId);
            if (definition == null) return Failure(request, eventId, "sale_item_invalid", "只有养殖目录产物可以走牧场销售请求。", D13RanchIds.EconomyRecoveryRoute);
            if (D3InventorySystem.CountCargo(_inventory, request.ItemId) < request.Quantity)
                return Failure(request, eventId, "sale_inventory_missing", "货舱没有足够的养殖产物。", D13RanchIds.InventoryRecoveryRoute);
            var decision = _economyAuthority.QuoteAndApproveSale(request.ItemId, request.Quantity, request.MarketRef, request.ActorRef, request.WorldDay, request.SaveRevision);
            if (decision == null || !decision.Allowed)
                return Failure(request, eventId, decision == null ? "economy_authority_required" : decision.Code, decision == null ? "必须由 EconomySystem 裁决养殖产品价格。" : decision.Message, D13RanchIds.EconomyRecoveryRoute);
            if (decision.SourceOwnerSystem != D13RanchIds.EconomyOwner || decision.UnitPrice < D13RanchConfiguration.Int(D13RanchConfiguration.MinimumSalePrice) || decision.TotalPrice != decision.UnitPrice * request.Quantity || string.IsNullOrWhiteSpace(decision.SourceEventId) || FindAnyFact(decision.SourceEventId, request.SaveRevision) == null)
                return Failure(request, eventId, "invalid_economy_decision", "销售价格必须来自 EconomySystem 的可追溯裁决。", D13RanchIds.EconomyRecoveryRoute);
            if (!D3InventorySystem.TryRemoveCargo(_inventory, request.ItemId, request.Quantity, out string removeError))
                return Failure(request, eventId, removeError, "养殖产品无法从货舱扣除。", D13RanchIds.InventoryRecoveryRoute);
            var record = new D13RanchSaleRecord
            {
                SaleId = "ranch_sale_" + SafeId(string.IsNullOrWhiteSpace(request.OperationId) ? eventId : request.OperationId),
                ItemId = request.ItemId,
                Quantity = request.Quantity,
                UnitPrice = decision.UnitPrice,
                TotalPrice = decision.TotalPrice,
                Currency = decision.Currency ?? D13RanchIds.Currency,
                MarketRef = decision.MarketRef ?? request.MarketRef,
                LedgerRef = decision.LedgerRef,
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0L, request.SaveRevision),
            };
            var fact = AppendEvent(eventId, request, "ranch_sale_approved", "EconomySystem 已批准养殖产品销售；RanchSystem 只记录货物离库和账本引用。", new Dictionary<string, string>
            {
                ["item_id"] = request.ItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["unit_price"] = decision.UnitPrice.ToString(CultureInfo.InvariantCulture), ["total_price"] = decision.TotalPrice.ToString(CultureInfo.InvariantCulture),
                ["currency"] = record.Currency, ["market_ref"] = record.MarketRef ?? "", ["ledger_ref"] = record.LedgerRef ?? "",
                ["economy_source_event_id"] = decision.SourceEventId,
            });
            _sales[record.SaleId] = record;
            _revision++;
            return Success(fact, null, definition, request.Quantity, request.ItemId, null, decision.UnitPrice, decision.TotalPrice, record.Currency, record.LedgerRef);
        }

        public D13RanchResult RequestBreeding(D13RanchBreedingRequest request)
        {
            if (request == null) return Invalid("invalid_request", "繁殖请求不存在。", D13RanchIds.BreedingRecoveryRoute);
            string eventId = EventId(request.OperationId, "breeding_" + request.AnimalId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, request.AnimalId);
            D13RanchAnimalState state = FindAnimal(request.AnimalId);
            D13RanchAnimalDefinition definition = state == null ? null : FindDefinition(state.DefinitionId);
            var fact = AppendEvent(eventId, request, "breeding_deferred", "当前版本只支持可保存的单周期产出；繁殖不生成新实例，必须进入后续专门设计。", new Dictionary<string, string>
            {
                ["animal_id"] = request.AnimalId ?? "", ["mate_animal_id"] = request.MateAnimalId ?? "",
                ["breeding_mode"] = definition == null ? D13RanchBreedingMode.Disabled.ToString() : definition.BreedingMode.ToString(),
            }, WorldEventStatus.Failed, D13RanchIds.BreedingRecoveryRoute);
            return FailureResult(fact, state, definition, "breeding_deferred", D13RanchIds.BreedingRecoveryRoute);
        }

        private bool TryFacility(string ranchId, out D13RanchFacilityDefinition facility)
        {
            return _facilities.TryGetValue(ranchId ?? string.Empty, out facility);
        }

        private bool TryAnimal(string animalId, out D13RanchAnimalState state, out D13RanchAnimalDefinition definition)
        {
            state = null;
            definition = null;
            if (!_animals.TryGetValue(animalId ?? string.Empty, out state)) return false;
            _definitions.TryGetValue(state.DefinitionId ?? string.Empty, out definition);
            return definition != null;
        }

        private int OccupiedSpace(string ranchId)
        {
            int result = 0;
            foreach (var item in _animals.Values)
            {
                if (item.RanchId != ranchId || item.Lifecycle == D13RanchAnimalLifecycle.Dead || item.Lifecycle == D13RanchAnimalLifecycle.Sold) continue;
                if (_definitions.TryGetValue(item.DefinitionId ?? string.Empty, out D13RanchAnimalDefinition definition)) result += definition.SpaceRequired;
            }
            return result;
        }

        private static bool CanCare(D13RanchAnimalState state)
        {
            return state != null && (state.Lifecycle == D13RanchAnimalLifecycle.Healthy || state.Lifecycle == D13RanchAnimalLifecycle.Sick);
        }

        private bool SourcesExist(string propertySource, string legalSource, long saveRevision)
        {
            if (!string.IsNullOrWhiteSpace(propertySource) && FindAnyFact(propertySource, saveRevision) == null) return false;
            if (!string.IsNullOrWhiteSpace(legalSource) && FindAnyFact(legalSource, saveRevision) == null) return false;
            return !string.IsNullOrWhiteSpace(propertySource) || !string.IsNullOrWhiteSpace(legalSource);
        }

        private FactEvent FindAnyFact(string eventId, long saveRevision)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            var exact = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            return exact ?? _eventLog.Facts.FirstOrDefault(item => item.EventId == eventId);
        }

        private FactEvent AppendEvent(string eventId, D13RanchOperationRequest request, string resultCode,
            string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal,
            string recoveryRoute = null)
        {
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request == null ? 0 : request.WorldDay), request == null || string.IsNullOrWhiteSpace(request.LocationRef) ? "island_saint_marys" : request.LocationRef,
                D13RanchIds.SystemOwner,
                new EventResult
                {
                    ResultCode = resultCode ?? "ranch_result",
                    Summary = summary ?? string.Empty,
                    FactIds = new List<string> { "ranch_fact:" + eventId },
                    Values = values ?? new Dictionary<string, string>(),
                },
                new[] { request == null || string.IsNullOrWhiteSpace(request.ActorRef) ? D13RanchIds.PlayerRef : request.ActorRef },
                new[] { "ranch:canonical" },
                FilterCauses(request == null ? null : request.CauseEventIds, request == null ? 0 : request.SaveRevision),
                request == null ? 0 : Math.Max(0L, request.SaveRevision),
                status,
                status == WorldEventStatus.Failed ? new RecoveryRoute
                {
                    RouteId = recoveryRoute ?? D13RanchIds.FeedRecoveryRoute,
                    Action = "review_and_retry",
                    ResumeFromEventId = eventId,
                    Description = summary ?? "补充养殖条件后重试。",
                } : null);
            var append = _eventLog.Append(worldEvent);
            _appliedEventKeys.Add(worldEvent.DedupeKey);
            return append.Fact;
        }

        private IEnumerable<string> FilterCauses(IEnumerable<string> causes, long saveRevision)
        {
            return (causes ?? Enumerable.Empty<string>()).Where(item => !string.IsNullOrWhiteSpace(item)).Where(item => FindAnyFact(item, saveRevision) != null).Distinct(StringComparer.Ordinal);
        }

        private FactEvent Existing(string eventId, long saveRevision)
        {
            long revision = Math.Max(0L, saveRevision);
            var fact = _eventLog.Find(eventId, revision);
            if (fact != null) return fact;
            string key = (eventId ?? string.Empty) + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
            if (!_appliedEventKeys.Contains(key)) return null;
            return new FactEvent
            {
                EventId = eventId,
                SaveRevision = revision,
                Result = new EventResult { ResultCode = "duplicate", Summary = "养殖操作已在此前存档中结算。" },
            };
        }

        private D13RanchResult Failure(D13RanchOperationRequest request, string eventId, string code, string message, string route,
            D13RanchAnimalState state = null, D13RanchAnimalDefinition definition = null)
        {
            var fact = AppendEvent(eventId, request, code ?? "ranch_failed", message ?? "养殖操作失败。", new Dictionary<string, string>
            {
                ["failure_code"] = code ?? "ranch_failed", ["recovery_route"] = route ?? D13RanchIds.FeedRecoveryRoute,
                ["ranch_id"] = request == null ? "" : request.RanchId ?? "", ["animal_id"] = request == null ? "" : request.AnimalId ?? "",
            }, WorldEventStatus.Failed, route);
            _revision++;
            return FailureResult(fact, state, definition, code, route);
        }

        private static D13RanchResult FailureResult(FactEvent fact, D13RanchAnimalState state, D13RanchAnimalDefinition definition, string code, string route)
        {
            return new D13RanchResult
            {
                Succeeded = false,
                Code = code,
                Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary,
                EventId = fact == null ? string.Empty : fact.EventId,
                RecoveryRouteId = route,
                State = state == null ? null : state.Clone(),
                Definition = definition == null ? null : definition.Clone(),
                Fact = fact,
            };
        }

        private D13RanchResult Invalid(string code, string message, string route)
        {
            return new D13RanchResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route };
        }

        private D13RanchResult Duplicate(FactEvent fact, string animalId)
        {
            D13RanchAnimalState state = FindAnimal(animalId);
            D13RanchAnimalDefinition definition = state == null ? null : FindDefinition(state.DefinitionId);
            return new D13RanchResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate",
                Message = "同一养殖操作已结算，未重复生成动物、产物、扣货或销售。",
                EventId = fact == null ? string.Empty : fact.EventId,
                State = state,
                Definition = definition,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private D13RanchResult Success(FactEvent fact, D13RanchAnimalState state, D13RanchAnimalDefinition definition,
            int quantity, string productItemId, D13RanchWaterState water, long unitPrice = 0, long totalPrice = 0,
            string currency = null, string ledgerRef = null)
        {
            return new D13RanchResult
            {
                Succeeded = true,
                Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode,
                Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary,
                EventId = fact == null ? string.Empty : fact.EventId,
                Definition = definition == null ? null : definition.Clone(),
                State = state == null ? null : state.Clone(),
                Water = water == null ? null : water.Clone(),
                Quantity = quantity,
                ProductItemId = productItemId,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                Currency = currency,
                LedgerRef = ledgerRef,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private static void Touch(D13RanchAnimalState state, string eventId, int worldDay)
        {
            state.Revision++;
            state.LastEventId = eventId;
            state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, worldDay));
        }

        private static void ApplyHealthLoss(D13RanchAnimalState state, int amount, int worldDay)
        {
            state.Health = Math.Max(0, state.Health - Math.Max(0, amount));
            if (state.Health <= 0)
            {
                state.Health = 0;
                state.Lifecycle = D13RanchAnimalLifecycle.Dead;
                state.DeathDay = Math.Max(0, worldDay);
                state.ProductReadyQuantity = 0;
                state.ProductItemId = null;
            }
            else if (state.Lifecycle != D13RanchAnimalLifecycle.Quarantined)
                state.Lifecycle = D13RanchAnimalLifecycle.Sick;
        }

        private static D13RanchDisease DiseaseForRoll(int roll)
        {
            switch (Math.Abs(roll) % 3)
            {
                case 0: return D13RanchDisease.Respiratory;
                case 1: return D13RanchDisease.Parasite;
                default: return D13RanchDisease.Infection;
            }
        }

        private static string EventId(string operationId, string fallback)
        {
            return "event_d13_ranch_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "none";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++) if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_';
            return new string(chars);
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
            long value = (long)Math.Max(0, left) + Math.Max(0, right);
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }

        private sealed class UnavailableSourceAuthority : ID13RanchSourceAuthority
        {
            public D13RanchSourceDecision ValidateSource(D13RanchAnimalDefinition definition, string sourceRef, string sourceEventId, string actorRef, int worldDay)
            {
                return new D13RanchSourceDecision { Allowed = false, InspectionPassed = false, Code = "source_authority_required", Message = "必须由动物来源/检疫 authority 提供决定。" };
            }
        }

        private sealed class UnavailableLandAuthority : ID13RanchLandAuthority
        {
            public D13RanchLandDecision ValidateFacility(D13RanchFacilityDefinition definition, string actorRef, int worldDay)
            {
                return new D13RanchLandDecision { Allowed = false, Code = "property_authority_required", Message = "必须由 PropertySystem 提供牧场经营资格。" };
            }
        }

        private sealed class UnavailableEconomyAuthority : ID13RanchEconomyAuthority
        {
            public D13RanchEconomyDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision)
            {
                return new D13RanchEconomyDecision { Allowed = false, Code = "economy_authority_required", Message = "必须由 EconomySystem 裁决养殖产品价格。" };
            }
        }
    }
}
