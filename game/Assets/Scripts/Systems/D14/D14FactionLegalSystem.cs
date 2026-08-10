// D14-A05 FactionLegalSystem 唯一写者。
// 战斗、库存、经济和 Agent 只提供事实/意图；本系统写声望、外交关系、法律案件
// 和港口许可状态，最终现金/库存仍由 Economy/Inventory canonical owner 写入。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14FactionLegalSystem
    {
        private readonly EventLog _eventLog;
        private readonly D14FactionLegalCatalog _catalog;
        private readonly ID14FactionLegalCargoAuthority _cargo;
        private readonly ID14FactionLegalInventoryAuthority _inventory;
        private readonly ID14FactionLegalEconomyAuthority _economy;
        private readonly D14FactionLegalRuntimeSnapshot _state;

        public D14FactionLegalSystem(EventLog eventLog = null, D14FactionLegalCatalog catalog = null,
            ID14FactionLegalCargoAuthority cargo = null, ID14FactionLegalInventoryAuthority inventory = null,
            ID14FactionLegalEconomyAuthority economy = null, D14FactionLegalRuntimeSnapshot initialSnapshot = null)
        {
            _catalog = catalog ?? new D14FactionLegalCatalog();
            _cargo = cargo;
            _inventory = inventory;
            _economy = economy;
            if (eventLog != null) _eventLog = eventLog;
            else if (initialSnapshot != null && initialSnapshot.EventLog != null) _eventLog = new EventLog(initialSnapshot.EventLog);
            else _eventLog = new EventLog();
            _state = initialSnapshot == null ? new D14FactionLegalRuntimeSnapshot() : initialSnapshot.Clone();
            if (_state.EventLog == null) _state.EventLog = _eventLog.CaptureSnapshot();
            foreach (var faction in _catalog.Factions())
                if (!_state.Reputation.ContainsKey(faction.FactionId)) _state.Reputation[faction.FactionId] = 0;
            foreach (var port in _catalog.Ports())
                if (FindPortMutable(port.PortId) == null)
                    _state.Ports.Add(new D14PortLegalState
                    {
                        PortId = port.PortId, ControllingFactionId = port.ControllingFactionId, AccessStatus = D14PortAccessStatus.Open,
                        Reason = "catalog_default", LastUpdatedWorldDay = 0, SourceEventId = "catalog:" + port.PortId,
                        EvidenceEventId = "catalog:" + port.PortId, LastEventId = "catalog:" + port.PortId,
                    });
            _state.Validate();
        }

        public const string OwnerSystem = D14FactionLegalIds.OwnerSystem;
        public EventLog EventLog { get { return _eventLog; } }
        public D14FactionLegalRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }
        public IReadOnlyList<D14FactionLegalDefinition> Factions { get { return _catalog.Factions(); } }
        public IReadOnlyList<D14PortLegalDefinition> Ports { get { return _catalog.Ports(); } }

        public int GetReputation(string factionId)
        {
            int value;
            return factionId != null && _state.Reputation.TryGetValue(factionId, out value) ? value : 0;
        }

        public int GetWantedLevel(string subjectRef)
        {
            int value;
            return subjectRef != null && _state.WantedLevels.TryGetValue(subjectRef, out value) ? value : 0;
        }

        public bool IsAtWar(string factionA, string factionB)
        {
            var relation = FindRelationMutable(D14FactionLegalIds.PairKey(factionA, factionB));
            return relation != null && relation.Status == D14FactionRelationStatus.AtWar;
        }

        public bool HasPrivateerPermit(string holderRef, string issuerFactionId, int worldDay)
        {
            foreach (var permit in _state.Permits)
                if (permit.HolderRef == holderRef && permit.IssuerFactionId == issuerFactionId && permit.Status == D14PrivateerPermitStatus.Active && worldDay <= permit.ExpiresWorldDay)
                    return true;
            return false;
        }

        public D14PortLegalState ReadPort(string portId)
        {
            var port = FindPortMutable(portId);
            return port == null ? null : port.Clone();
        }

        public bool CanEnterPort(string portId, string holderRef, string controllingFactionId, int worldDay)
        {
            var port = FindPortMutable(portId);
            if (port == null || port.ControllingFactionId != controllingFactionId) return false;
            if (GetWantedLevel(holderRef) >= 4) return false;
            if (port.AccessStatus == D14PortAccessStatus.Denied || port.AccessStatus == D14PortAccessStatus.Blockaded) return false;
            if (port.AccessStatus == D14PortAccessStatus.EscortRequired) return false;
            return port.AccessStatus == D14PortAccessStatus.Open ||
                (port.AccessStatus == D14PortAccessStatus.Restricted && GetReputation(controllingFactionId) >= 0) ||
                HasPrivateerPermit(holderRef, controllingFactionId, worldDay);
        }

        public D14FactionLegalResult ApplyReputation(D14ReputationRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "reputation", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.FactionId) == null) return Reject("faction_unknown", "未知势力。");
            if (request.Delta < -100 || request.Delta > 100 || string.IsNullOrWhiteSpace(request.Reason)) return Reject("reputation_request_invalid", "声望变化必须有有限增量和理由。");
            int before = GetReputation(request.FactionId);
            int after = Clamp(before + request.Delta, -100, 100);
            var fact = AppendFact("reputation", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId,
                request.EvidenceEventIds, request.CauseEventIds, "reputation_changed", "声望变化由 FactionLegalSystem 根据来源事实裁决。",
                new Dictionary<string, string> { ["faction_id"] = request.FactionId, ["before"] = before.ToString(), ["after"] = after.ToString(), ["delta"] = (after - before).ToString(), ["reason"] = request.Reason });
            _state.Reputation[request.FactionId] = after;
            AddAudit(request.OperationId, "reputation_changed", fact, request.SourceEventId, request.EvidenceEventIds, null, request.FactionId, request.WorldDay, request.Reason, null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("reputation_changed", "声望已记录。", fact, request.WorldDay, before, after);
        }

        public D14FactionLegalResult DeclareWar(D14WarRequest request)
        {
            return SetRelation(request, D14FactionRelationStatus.AtWar, true);
        }

        public D14FactionLegalResult SignTruce(D14TruceRequest request)
        {
            if (request == null) return Reject("request_required", "停战请求不能为空。");
            if (!IsAtWar(request.FactionA, request.FactionB)) return Reject("war_not_active", "只有正在交战的势力可以签署停战。");
            return SetRelation(new D14WarRequest
            {
                OperationId = request.OperationId, ActorRef = request.ActorRef, SourceEventId = request.SourceEventId,
                WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, EvidenceEventIds = request.EvidenceEventIds,
                CauseEventIds = request.CauseEventIds, FactionA = request.FactionA, FactionB = request.FactionB, TreatyRef = request.TreatyRef,
            }, D14FactionRelationStatus.Truce, false);
        }

        public D14FactionLegalResult IssuePrivateerPermit(D14PrivateerPermitRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "privateer_permit", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.IssuerFactionId) == null || (!string.IsNullOrWhiteSpace(request.TargetFactionId) && _catalog.FindFaction(request.TargetFactionId) == null)) return Reject("faction_unknown", "私掠证的势力无效。");
            if (string.IsNullOrWhiteSpace(request.PermitId) || string.IsNullOrWhiteSpace(request.HolderRef) || request.ExpiresWorldDay < request.WorldDay) return Reject("permit_request_invalid", "私掠证必须有持有人、ID 和有效期限。");
            if (GetReputation(request.IssuerFactionId) < 30) return Reject("reputation_insufficient", "发证势力声望不足，不能签发私掠证。");
            if (FindPermitMutable(request.PermitId) != null) return Duplicate("permit_duplicate", "该私掠证已经登记。", permit: FindPermitMutable(request.PermitId));
            var fact = AppendFact("privateer_permit", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId,
                request.EvidenceEventIds, request.CauseEventIds, "privateer_permit_issued", "私掠证由发证势力和法律来源共同登记。",
                new Dictionary<string, string> { ["permit_id"] = request.PermitId, ["issuer_faction_id"] = request.IssuerFactionId, ["holder_ref"] = request.HolderRef, ["target_faction_id"] = request.TargetFactionId ?? "", ["expires_world_day"] = request.ExpiresWorldDay.ToString() });
            var permit = new D14PrivateerPermitState
            {
                PermitId = request.PermitId, IssuerFactionId = request.IssuerFactionId, HolderRef = request.HolderRef, TargetFactionId = request.TargetFactionId,
                Status = D14PrivateerPermitStatus.Active, GrantedWorldDay = request.WorldDay, ExpiresWorldDay = request.ExpiresWorldDay,
                SourceEventId = request.SourceEventId, EvidenceEventId = request.EvidenceEventIds[0], LastEventId = fact.EventId,
            };
            _state.Permits.Add(permit);
            AddAudit(request.OperationId, "privateer_permit_issued", fact, request.SourceEventId, request.EvidenceEventIds, request.HolderRef, request.IssuerFactionId, request.WorldDay, "私掠证来源、期限和证据已登记。", null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("privateer_permit_issued", "私掠证已登记。", fact, request.WorldDay, 0, 0, permit: permit);
        }

        public D14FactionLegalResult Inspect(D14InspectionRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "inspection", true, out source, out failure)) return failure;
            var portDefinition = _catalog.FindPort(request.PortId);
            var controlling = _catalog.FindFaction(request.ControllingFactionId);
            if (portDefinition == null || controlling == null || portDefinition.ControllingFactionId != request.ControllingFactionId || string.IsNullOrWhiteSpace(request.InspectionId) || string.IsNullOrWhiteSpace(request.SubjectRef)) return Reject("port_or_faction_invalid", "检查必须有有效港口、控制势力、案件 ID 和对象。");
            if (_state.Inspections.Exists(delegate(D14InspectionRecord value) { return value.InspectionId == request.InspectionId; })) return Duplicate("inspection_duplicate", "该检查案件已经登记。", inspection: _state.Inspections.Find(delegate(D14InspectionRecord value) { return value.InspectionId == request.InspectionId; }));
            var manifest = FindAnyFact(request.CargoManifestEventId);
            if (manifest == null || manifest.OwnerSystem != D14FactionLegalIds.InventoryOwner) return Reject("manifest_source_invalid", "货物清单必须来自 InventorySystem 事实。");
            if (_cargo == null) return Reject("cargo_authority_required", "检查必须由 Inventory/Cargo authority 返回货物裁决。");
            var cargoDecision = _cargo.Inspect(request, manifest.CloneFact());
            if (cargoDecision == null || !cargoDecision.Allowed || cargoDecision.SourceOwnerSystem != D14FactionLegalIds.InventoryOwner ||
                string.IsNullOrWhiteSpace(cargoDecision.SourceEventId) || FindOwnedFact(cargoDecision.SourceEventId, D14FactionLegalIds.InventoryOwner) == null || cargoDecision.ProhibitedCargoRefs == null)
                return Reject("cargo_authority_invalid", "Inventory/Cargo 检查决定缺少来源或越过唯一写者边界。");

            bool portOpen = IsPortOpenForInspection(request.PortId, request.SubjectRef, request.ControllingFactionId, request.WorldDay);
            bool illegal = !cargoDecision.CargoLegal || cargoDecision.ProhibitedCargoRefs.Count > 0;
            D14EconomicAssessmentDecision fineDecision = null;
            if (illegal)
            {
                if (_inventory == null) return Reject("inventory_authority_required", "违禁货物扣押必须由 InventorySystem 裁决。");
                var seizureDecision = _inventory.Seize(new D14SeizureRequest { OperationId = request.OperationId + "_seizure", ActorRef = request.ActorRef, SourceEventId = request.SourceEventId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, EvidenceEventIds = request.EvidenceEventIds, CauseEventIds = request.CauseEventIds, SeizureId = "seizure_" + D14FactionLegalIds.StableId(request.InspectionId), SubjectRef = request.SubjectRef, FactionId = request.ControllingFactionId, CargoManifestEventId = request.CargoManifestEventId, CargoRefs = new List<string>(cargoDecision.ProhibitedCargoRefs) }, manifest.CloneFact(), cargoDecision.ProhibitedCargoRefs);
                if (seizureDecision == null || !seizureDecision.Allowed || seizureDecision.SourceOwnerSystem != D14FactionLegalIds.InventoryOwner || FindOwnedFact(seizureDecision.SourceEventId, D14FactionLegalIds.InventoryOwner) == null || seizureDecision.SeizedCargoRefs == null || seizureDecision.SeizedCargoRefs.Count == 0)
                    return Reject("seizure_authority_invalid", "扣押决定必须来自 InventorySystem 且包含被扣货物引用。");
                if (_economy == null) return Reject("economy_authority_required", "罚金必须由 EconomySystem 评估。");
                fineDecision = _economy.AssessFine(new D14FineRequest { CaseId = "fine_" + D14FactionLegalIds.StableId(request.InspectionId), SubjectRef = request.SubjectRef, FactionId = request.ControllingFactionId, Reason = "inspection_illegal_cargo", OperationId = request.OperationId + "_fine", ActorRef = request.ActorRef, SourceEventId = request.SourceEventId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, EvidenceEventIds = request.EvidenceEventIds, CauseEventIds = request.CauseEventIds }, source.CloneFact());
                if (!ValidEconomicAssessment(fineDecision)) return Reject("fine_assessment_invalid", "EconomySystem 未返回可追踪罚金评估。");
            }

            var inspectionFact = AppendFact("inspection", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId,
                request.EvidenceEventIds, MergeCauses(request.CauseEventIds, request.CargoManifestEventId, cargoDecision.SourceEventId), "inspection_resolved", "检查、货物合法性和港口许可结果已由 FactionLegalSystem 保存。",
                new Dictionary<string, string> { ["inspection_id"] = request.InspectionId, ["port_id"] = request.PortId, ["subject_ref"] = request.SubjectRef, ["cargo_legal"] = illegal ? "false" : "true", ["port_open"] = portOpen.ToString(), ["seized"] = illegal ? "true" : "false", ["fine_case_id"] = illegal ? "fine_" + D14FactionLegalIds.StableId(request.InspectionId) : "" });
            var inspection = new D14InspectionRecord
            {
                InspectionId = request.InspectionId, SubjectRef = request.SubjectRef, PortId = request.PortId, ControllingFactionId = request.ControllingFactionId,
                Allowed = !illegal && portOpen, CargoLegal = !illegal, ProhibitedCargoRefs = new List<string>(cargoDecision.ProhibitedCargoRefs),
                WantedCaseId = illegal ? "wanted_" + D14FactionLegalIds.StableId(request.InspectionId) : null,
                SeizureId = illegal ? "seizure_" + D14FactionLegalIds.StableId(request.InspectionId) : null,
                FineCaseId = illegal ? "fine_" + D14FactionLegalIds.StableId(request.InspectionId) : null,
                WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, CargoManifestEventId = request.CargoManifestEventId,
                EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = inspectionFact.EventId,
            };
            _state.Inspections.Add(inspection);
            D14WantedCaseState wanted = null; D14SeizureRecord seizure = null; D14FinancialCaseState fine = null;
            if (illegal)
            {
                wanted = new D14WantedCaseState
                {
                    CaseId = inspection.WantedCaseId, SubjectRef = request.SubjectRef, FactionId = request.ControllingFactionId, Severity = 2,
                    Status = D14WantedStatus.Open, IssuedWorldDay = request.WorldDay, Reason = "inspection_illegal_cargo", SourceEventId = request.SourceEventId,
                    EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = inspectionFact.EventId,
                };
                _state.WantedCases.Add(wanted); _state.WantedLevels[request.SubjectRef] = Math.Max(GetWantedLevel(request.SubjectRef), wanted.Severity);
                seizure = new D14SeizureRecord
                {
                    SeizureId = inspection.SeizureId, SubjectRef = request.SubjectRef, FactionId = request.ControllingFactionId, Status = D14LegalCaseStatus.Open,
                    CargoRefs = new List<string>(cargoDecision.ProhibitedCargoRefs), CargoManifestEventId = request.CargoManifestEventId, InventorySourceEventId = cargoDecision.SourceEventId,
                    SeizedWorldDay = request.WorldDay, SourceEventId = request.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = inspectionFact.EventId,
                };
                _state.Seizures.Add(seizure);
                fine = new D14FinancialCaseState
                {
                    CaseId = inspection.FineCaseId, Kind = D14FinancialCaseKind.Fine, SubjectRef = request.SubjectRef, FactionId = request.ControllingFactionId,
                    Status = D14LegalCaseStatus.Open, RequestedAmount = fineDecision.Amount, IssuedWorldDay = request.WorldDay, Reason = "inspection_illegal_cargo",
                    SourceEventId = request.SourceEventId, EconomySourceEventId = fineDecision.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = inspectionFact.EventId,
                };
                _state.FinancialCases.Add(fine);
            }
            AddAudit(request.OperationId, "inspection_resolved", inspectionFact, request.SourceEventId, request.EvidenceEventIds, request.SubjectRef, request.ControllingFactionId, request.WorldDay, illegal ? "违禁货物触发通缉、扣押和罚金案件。" : (portOpen ? "检查通过。" : "港口许可状态阻止入港。"), illegal ? D14FactionLegalIds.SeizureRecoveryRoute : null);
            Touch(inspectionFact, request.OperationId, request.WorldDay);
            return Applied("inspection_resolved", illegal ? "检查发现违禁货物，已记录通缉、扣押和罚金。" : (portOpen ? "检查通过。" : "检查完成但港口许可不允许入港。"), inspectionFact, request.WorldDay, 0, 0, inspection: inspection, wanted: wanted, seizure: seizure, financial: fine);
        }

        public D14FactionLegalResult IssueWanted(D14WantedRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "wanted", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.FactionId) == null || string.IsNullOrWhiteSpace(request.CaseId) || string.IsNullOrWhiteSpace(request.SubjectRef) || request.Severity < 1 || request.Severity > 5 || string.IsNullOrWhiteSpace(request.Reason)) return Reject("wanted_request_invalid", "通缉必须有案件、对象、势力、等级和理由。");
            if (FindWantedMutable(request.CaseId) != null) return Duplicate("wanted_duplicate", "该通缉案件已经登记。", wanted: FindWantedMutable(request.CaseId));
            var fact = AppendFact("wanted", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, "wanted_issued", "通缉由法律事实和证据登记。", new Dictionary<string, string> { ["case_id"] = request.CaseId, ["subject_ref"] = request.SubjectRef, ["faction_id"] = request.FactionId, ["severity"] = request.Severity.ToString(), ["reason"] = request.Reason });
            var wanted = new D14WantedCaseState { CaseId = request.CaseId, SubjectRef = request.SubjectRef, FactionId = request.FactionId, Severity = request.Severity, Status = D14WantedStatus.Open, IssuedWorldDay = request.WorldDay, Reason = request.Reason, SourceEventId = request.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = fact.EventId };
            _state.WantedCases.Add(wanted); _state.WantedLevels[request.SubjectRef] = Math.Max(GetWantedLevel(request.SubjectRef), request.Severity);
            AddAudit(request.OperationId, "wanted_issued", fact, request.SourceEventId, request.EvidenceEventIds, request.SubjectRef, request.FactionId, request.WorldDay, request.Reason, D14FactionLegalIds.WantedRecoveryRoute);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("wanted_issued", "通缉已登记。", fact, request.WorldDay, 0, 0, wanted: wanted);
        }

        public D14FactionLegalResult ClearWanted(D14WantedClearRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "wanted_clear", true, out source, out failure)) return failure;
            var wanted = FindWantedMutable(request.CaseId);
            if (wanted == null) return Reject("wanted_missing", "找不到通缉案件。");
            if (wanted.Status != D14WantedStatus.Open) return Duplicate("wanted_already_cleared", "该通缉案件已经结案。", wanted: wanted);
            var settlement = FindAnyFact(request.SettlementSourceEventId);
            if (settlement == null || settlement.OwnerSystem != D14FactionLegalIds.EconomyOwner) return Reject("settlement_source_invalid", "撤销通缉需要 EconomySystem 的结算事实。");
            var fact = AppendFact("wanted_clear", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, MergeCauses(request.CauseEventIds, request.SettlementSourceEventId), "wanted_cleared", "通缉撤销绑定罚金/赎金结算来源和申诉证据。", new Dictionary<string, string> { ["case_id"] = wanted.CaseId, ["settlement_source_event_id"] = settlement.EventId });
            wanted.Status = D14WantedStatus.Cleared; wanted.ResolvedWorldDay = request.WorldDay; wanted.LastEventId = fact.EventId;
            _state.WantedLevels[wanted.SubjectRef] = Math.Max(0, GetWantedLevel(wanted.SubjectRef) - wanted.Severity);
            AddAudit(request.OperationId, "wanted_cleared", fact, request.SourceEventId, request.EvidenceEventIds, wanted.SubjectRef, wanted.FactionId, request.WorldDay, "结算事实确认后撤销通缉。", null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("wanted_cleared", "通缉已撤销。", fact, request.WorldDay, 0, 0, wanted: wanted);
        }

        public D14FactionLegalResult CompleteEscort(D14EscortRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "escort", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.IssuerFactionId) == null || string.IsNullOrWhiteSpace(request.ContractId) || string.IsNullOrWhiteSpace(request.EscorterRef) || string.IsNullOrWhiteSpace(request.RouteId) || string.IsNullOrWhiteSpace(request.ProtectedCargoManifestEventId)) return Reject("escort_request_invalid", "护航必须有合同、护航者、势力、航线和货物清单。");
            if (_state.Escorts.Exists(delegate(D14EscortRecord value) { return value.ContractId == request.ContractId; })) return Reject("escort_duplicate", "该护航合同已经登记。");
            var manifest = FindAnyFact(request.ProtectedCargoManifestEventId);
            if (manifest == null || manifest.OwnerSystem != D14FactionLegalIds.InventoryOwner) return Reject("manifest_source_invalid", "护航货物清单必须来自 InventorySystem。");
            bool success = IsSuccessFact(source);
            int delta = success ? 8 : -5;
            int before = GetReputation(request.IssuerFactionId); int after = Clamp(before + delta, -100, 100);
            var fact = AppendFact("escort", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, MergeCauses(request.CauseEventIds, request.ProtectedCargoManifestEventId), "escort_resolved", "护航结果从 Escort/Combat 事实结算为势力后果。", new Dictionary<string, string> { ["contract_id"] = request.ContractId, ["success"] = success.ToString(), ["faction_id"] = request.IssuerFactionId, ["reputation_before"] = before.ToString(), ["reputation_after"] = after.ToString() });
            _state.Reputation[request.IssuerFactionId] = after;
            var escort = new D14EscortRecord { ContractId = request.ContractId, EscorterRef = request.EscorterRef, IssuerFactionId = request.IssuerFactionId, RouteId = request.RouteId, ProtectedCargoManifestEventId = request.ProtectedCargoManifestEventId, Success = success, ReputationDelta = after - before, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = fact.EventId };
            _state.Escorts.Add(escort);
            AddAudit(request.OperationId, "escort_resolved", fact, request.SourceEventId, request.EvidenceEventIds, request.EscorterRef, request.IssuerFactionId, request.WorldDay, success ? "护航成功，声望提升。" : "护航失败，声望下降。", null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("escort_resolved", success ? "护航成功，声望已更新。" : "护航失败，声望后果已记录。", fact, request.WorldDay, before, after, escort: escort);
        }

        public D14FactionLegalResult SetPortAccess(D14PortAccessRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "port_access", true, out source, out failure)) return failure;
            var definition = _catalog.FindPort(request.PortId);
            if (definition == null || definition.ControllingFactionId != request.ControllingFactionId || !Enum.IsDefined(typeof(D14PortAccessStatus), request.AccessStatus) || string.IsNullOrWhiteSpace(request.Reason)) return Reject("port_access_invalid", "港口许可、控制势力或理由无效。");
            var port = FindPortMutable(request.PortId);
            var fact = AppendFact("port_access", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, "port_access_changed", "港口禁入、限制或开放状态由法律系统写入。", new Dictionary<string, string> { ["port_id"] = request.PortId, ["access_status"] = request.AccessStatus.ToString(), ["reason"] = request.Reason });
            port.AccessStatus = request.AccessStatus; port.Reason = request.Reason; port.LastUpdatedWorldDay = request.WorldDay; port.SourceEventId = request.SourceEventId; port.EvidenceEventId = request.EvidenceEventIds[0]; port.LastEventId = fact.EventId;
            AddAudit(request.OperationId, "port_access_changed", fact, request.SourceEventId, request.EvidenceEventIds, request.ActorRef, request.ControllingFactionId, request.WorldDay, request.Reason, request.AccessStatus == D14PortAccessStatus.Denied ? D14FactionLegalIds.WantedRecoveryRoute : null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("port_access_changed", "港口许可状态已更新。", fact, request.WorldDay, 0, 0, port: port);
        }

        public D14FactionLegalResult RecordSeizure(D14SeizureRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "seizure", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.FactionId) == null || string.IsNullOrWhiteSpace(request.SeizureId) || string.IsNullOrWhiteSpace(request.SubjectRef) || string.IsNullOrWhiteSpace(request.CargoManifestEventId) || request.CargoRefs == null || request.CargoRefs.Count == 0) return Reject("seizure_request_invalid", "扣押必须有案件、对象、势力和货物引用。");
            var manifest = FindAnyFact(request.CargoManifestEventId);
            if (manifest == null || manifest.OwnerSystem != D14FactionLegalIds.InventoryOwner) return Reject("manifest_source_invalid", "扣押货物清单必须来自 InventorySystem。");
            if (_inventory == null) return Reject("inventory_authority_required", "扣押必须由 InventorySystem 裁决。");
            var decision = _inventory.Seize(request, manifest.CloneFact(), request.CargoRefs);
            if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D14FactionLegalIds.InventoryOwner || FindOwnedFact(decision.SourceEventId, D14FactionLegalIds.InventoryOwner) == null || decision.SeizedCargoRefs == null || decision.SeizedCargoRefs.Count == 0) return Reject("seizure_authority_invalid", "扣押决定缺少 InventorySystem 来源或货物引用。");
            if (FindSeizureMutable(request.SeizureId) != null) return Duplicate("seizure_duplicate", "该扣押案件已经登记。", seizure: FindSeizureMutable(request.SeizureId));
            var fact = AppendFact("seizure", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, MergeCauses(request.CauseEventIds, request.CargoManifestEventId, decision.SourceEventId), "cargo_seized", "扣押仅保存法律状态和 Inventory 来源，不直接改货舱。", new Dictionary<string, string> { ["seizure_id"] = request.SeizureId, ["subject_ref"] = request.SubjectRef, ["faction_id"] = request.FactionId, ["inventory_source_event_id"] = decision.SourceEventId });
            var seizure = new D14SeizureRecord { SeizureId = request.SeizureId, SubjectRef = request.SubjectRef, FactionId = request.FactionId, Status = D14LegalCaseStatus.Open, CargoRefs = new List<string>(decision.SeizedCargoRefs), CargoManifestEventId = request.CargoManifestEventId, InventorySourceEventId = decision.SourceEventId, SeizedWorldDay = request.WorldDay, SourceEventId = request.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = fact.EventId };
            _state.Seizures.Add(seizure);
            AddAudit(request.OperationId, "cargo_seized", fact, request.SourceEventId, request.EvidenceEventIds, request.SubjectRef, request.FactionId, request.WorldDay, "扣押证据和 Inventory 出库来源已登记。", D14FactionLegalIds.SeizureRecoveryRoute);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("cargo_seized", "扣押法律记录已保存。", fact, request.WorldDay, 0, 0, seizure: seizure);
        }

        public D14FactionLegalResult IssueFine(D14FineRequest request)
        {
            return IssueFinancial(request, D14FinancialCaseKind.Fine);
        }

        public D14FactionLegalResult IssueRansom(D14RansomRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PrisonerRef)) return Reject("ransom_request_invalid", "赎金案件必须引用俘虏/救援对象。");
            return IssueFinancial(request, D14FinancialCaseKind.Ransom);
        }

        public D14FactionLegalResult SettleFinancialCase(D14FinancialSettlementRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, "financial_settlement", true, out source, out failure)) return failure;
            var financial = FindFinancialMutable(request.CaseId);
            if (financial == null) return Reject("financial_case_missing", "找不到罚金或赎金案件。");
            if (financial.Status != D14LegalCaseStatus.Open) return Duplicate("financial_case_settled", "该案件已经结算。", financial: financial);
            var settlement = FindAnyFact(request.SettlementSourceEventId);
            if (settlement == null || settlement.OwnerSystem != D14FactionLegalIds.EconomyOwner || _economy == null) return Reject("settlement_source_invalid", "结算必须来自 EconomySystem。");
            var decision = _economy.ConfirmSettlement(request, financial.Clone(), settlement.CloneFact());
            if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D14FactionLegalIds.EconomyOwner || FindOwnedFact(decision.SourceEventId, D14FactionLegalIds.EconomyOwner) == null) return Reject("settlement_authority_invalid", "EconomySystem 未确认结算来源。");
            var fact = AppendFact("financial_settlement", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, MergeCauses(request.CauseEventIds, request.SettlementSourceEventId), "legal_case_settled", "罚金/赎金结算只更新法律案件状态，金额由 EconomySystem 记录。", new Dictionary<string, string> { ["case_id"] = financial.CaseId, ["kind"] = financial.Kind.ToString(), ["economy_source_event_id"] = decision.SourceEventId });
            financial.Status = financial.Kind == D14FinancialCaseKind.Ransom ? D14LegalCaseStatus.Released : D14LegalCaseStatus.Paid; financial.ResolvedWorldDay = request.WorldDay; financial.LastEventId = fact.EventId;
            AddAudit(request.OperationId, "legal_case_settled", fact, request.SourceEventId, request.EvidenceEventIds, financial.SubjectRef, financial.FactionId, request.WorldDay, financial.Kind == D14FinancialCaseKind.Ransom ? "赎金已由 EconomySystem 确认，案件进入释放状态。" : "罚金已由 EconomySystem 确认。", null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("legal_case_settled", "法律金融案件已结算。", fact, request.WorldDay, 0, 0, financial: financial);
        }

        public D14FactionLegalResult DeclareBlockade(D14BlockadeRequest request)
        {
            if (request == null) return Reject("request_required", "封锁请求不能为空。");
            request.Status = D14BlockadeStatus.Declared;
            return ChangeBlockade(request);
        }

        public D14FactionLegalResult LiftBlockade(D14BlockadeRequest request)
        {
            if (request == null) return Reject("request_required", "解除封锁请求不能为空。");
            request.Status = D14BlockadeStatus.Lifted;
            return ChangeBlockade(request);
        }

        public D14FactionLegalResult ProposeDiplomaticIntent(D14DiplomaticIntentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProposalId) || string.IsNullOrWhiteSpace(request.ActorRef) || string.IsNullOrWhiteSpace(request.RequestedBySystem) || string.IsNullOrWhiteSpace(request.TargetFactionId) || string.IsNullOrWhiteSpace(request.Summary) || request.WorldDay < 0 || request.SaveRevision < 0 || !Enum.IsDefined(typeof(D14DiplomaticIntentKind), request.IntentKind)) return Reject("intent_request_invalid", "外交意图缺少目标、摘要或类型。");
            if (_catalog.FindFaction(request.TargetFactionId) == null) return Reject("faction_unknown", "外交意图的目标势力不存在。");
            foreach (var intent in _state.DiplomaticIntents) if (intent.ProposalId == request.ProposalId) return Duplicate("intent_duplicate", "外交意图已经登记。", intent: intent);
            var fact = _eventLog.Append(new WorldEvent(D14FactionLegalIds.EventPrefix + "diplomatic_intent_" + D14FactionLegalIds.StableId(request.ProposalId), request.WorldDay, "d14_faction_legal", OwnerSystem,
                new EventResult { ResultCode = "diplomatic_intent_proposed", Summary = "外交意图已登记，等待势力/法律系统裁决。", FactIds = new List<string> { request.ProposalId }, Values = new Dictionary<string, string> { ["proposal_id"] = request.ProposalId, ["target_faction_id"] = request.TargetFactionId, ["intent_kind"] = request.IntentKind.ToString(), ["requested_by_system"] = request.RequestedBySystem } }, new[] { request.ActorRef }, new[] { "diplomatic_intent:" + request.ProposalId }, new string[0], request.SaveRevision)).Fact;
            var intentRecord = new D14DiplomaticIntentRecord { ProposalId = request.ProposalId, ActorRef = request.ActorRef, RequestedBySystem = request.RequestedBySystem, TargetFactionId = request.TargetFactionId, IntentKind = request.IntentKind, Summary = request.Summary, Status = "pending", WorldDay = request.WorldDay, LastEventId = fact.EventId };
            _state.DiplomaticIntents.Add(intentRecord);
            AddAudit(request.ProposalId, "diplomatic_intent_proposed", fact, fact.EventId, new List<string>(), request.ActorRef, request.TargetFactionId, request.WorldDay, "Agent/UI 只能提出外交意图，不能写外交结果。", null);
            Touch(fact, request.ProposalId, request.WorldDay);
            return Applied("diplomatic_intent_proposed", "外交意图已提交，未改变声望、法律或港口状态。", fact, request.WorldDay, 0, 0, intent: intentRecord);
        }

        public D14FactionLegalRuntimeSnapshot CaptureSnapshot()
        {
            _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
            return _state.Clone();
        }

        private D14FactionLegalResult SetRelation(D14WarRequest request, D14FactionRelationStatus status, bool war)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, war ? "war" : "truce", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.FactionA) == null || _catalog.FindFaction(request.FactionB) == null || string.IsNullOrWhiteSpace(request.TreatyRef) || request.FactionA == request.FactionB) return Reject("relation_request_invalid", "战争或停战必须有两个不同势力和外交文书引用。");
            var key = D14FactionLegalIds.PairKey(request.FactionA, request.FactionB);
            var existing = FindRelationMutable(key);
            if (existing != null && existing.Status == status) return Duplicate(war ? "war_duplicate" : "truce_duplicate", "该外交状态已经登记。", existing);
            var fact = AppendFact(war ? "war" : "truce", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, war ? "war_declared" : "truce_signed", war ? "战争状态已由外交/法律事实登记。" : "停战状态已由外交/法律事实登记。", new Dictionary<string, string> { ["relation_key"] = key, ["faction_a"] = request.FactionA, ["faction_b"] = request.FactionB, ["treaty_ref"] = request.TreatyRef });
            var relation = existing ?? new D14FactionRelationState { RelationKey = key, FactionA = request.FactionA, FactionB = request.FactionB, StartedWorldDay = request.WorldDay };
            relation.Status = status; relation.LastUpdatedWorldDay = request.WorldDay; relation.SourceEventId = request.SourceEventId; relation.EvidenceEventId = request.EvidenceEventIds[0]; relation.LastEventId = fact.EventId;
            if (existing == null) _state.Relations.Add(relation);
            AddAudit(request.OperationId, war ? "war_declared" : "truce_signed", fact, request.SourceEventId, request.EvidenceEventIds, request.ActorRef, request.FactionA, request.WorldDay, war ? "战争状态已登记。" : "停战已登记。", war ? D14FactionLegalIds.BlockadeRecoveryRoute : null);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied(war ? "war_declared" : "truce_signed", war ? "战争已登记。" : "停战已登记。", fact, request.WorldDay, 0, 0, relation: relation);
        }

        private D14FactionLegalResult IssueFinancial(D14FactionLegalOperationRequest request, D14FinancialCaseKind kind)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, kind == D14FinancialCaseKind.Fine ? "fine" : "ransom", true, out source, out failure)) return failure;
            string caseId = kind == D14FinancialCaseKind.Fine ? ((D14FineRequest)request).CaseId : ((D14RansomRequest)request).CaseId;
            string subject = request is D14FineRequest ? ((D14FineRequest)request).SubjectRef : ((D14RansomRequest)request).SubjectRef;
            string faction = request is D14FineRequest ? ((D14FineRequest)request).FactionId : ((D14RansomRequest)request).FactionId;
            string reason = request is D14FineRequest ? ((D14FineRequest)request).Reason : ((D14RansomRequest)request).Reason;
            string prisoner = request is D14RansomRequest ? ((D14RansomRequest)request).PrisonerRef : null;
            if (_catalog.FindFaction(faction) == null || string.IsNullOrWhiteSpace(caseId) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(reason) || (kind == D14FinancialCaseKind.Ransom && string.IsNullOrWhiteSpace(prisoner))) return Reject("financial_request_invalid", "罚金/赎金案件字段不完整。");
            if (FindFinancialMutable(caseId) != null) return Duplicate("financial_case_duplicate", "该罚金/赎金案件已经登记。", financial: FindFinancialMutable(caseId));
            if (_economy == null) return Reject("economy_authority_required", "金额必须由 EconomySystem 评估。");
            D14EconomicAssessmentDecision decision = kind == D14FinancialCaseKind.Fine ? _economy.AssessFine((D14FineRequest)request, source.CloneFact()) : _economy.AssessRansom((D14RansomRequest)request, source.CloneFact());
            if (!ValidEconomicAssessment(decision)) return Reject("economic_assessment_invalid", "EconomySystem 未返回带来源的金额评估。");
            var fact = AppendFact(kind == D14FinancialCaseKind.Fine ? "fine" : "ransom", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, kind == D14FinancialCaseKind.Fine ? "fine_issued" : "ransom_issued", "罚金/赎金法律义务已登记，现金不由 FactionLegalSystem 直接修改。", new Dictionary<string, string> { ["case_id"] = caseId, ["kind"] = kind.ToString(), ["subject_ref"] = subject, ["amount"] = decision.Amount.ToString(), ["economy_source_event_id"] = decision.SourceEventId });
            var financial = new D14FinancialCaseState { CaseId = caseId, Kind = kind, SubjectRef = subject, FactionId = faction, PrisonerRef = prisoner, Status = D14LegalCaseStatus.Open, RequestedAmount = decision.Amount, IssuedWorldDay = request.WorldDay, Reason = reason, SourceEventId = request.SourceEventId, EconomySourceEventId = decision.SourceEventId, EvidenceEventIds = new List<string>(request.EvidenceEventIds), LastEventId = fact.EventId };
            _state.FinancialCases.Add(financial);
            AddAudit(request.OperationId, kind == D14FinancialCaseKind.Fine ? "fine_issued" : "ransom_issued", fact, request.SourceEventId, request.EvidenceEventIds, subject, faction, request.WorldDay, reason, D14FactionLegalIds.WantedRecoveryRoute);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied(kind == D14FinancialCaseKind.Fine ? "fine_issued" : "ransom_issued", kind == D14FinancialCaseKind.Fine ? "罚金案件已登记。" : "赎金案件已登记。", fact, request.WorldDay, 0, 0, financial: financial);
        }

        private D14FactionLegalResult ChangeBlockade(D14BlockadeRequest request)
        {
            D14FactionLegalResult failure; FactEvent source;
            if (!Prepare(request, request.Status == D14BlockadeStatus.Declared ? "blockade_declare" : "blockade_lift", true, out source, out failure)) return failure;
            if (_catalog.FindFaction(request.IssuerFactionId) == null || (string.IsNullOrWhiteSpace(request.TargetFactionId) && string.IsNullOrWhiteSpace(request.TargetPortId)) || (!string.IsNullOrWhiteSpace(request.TargetFactionId) && _catalog.FindFaction(request.TargetFactionId) == null) || (!string.IsNullOrWhiteSpace(request.TargetPortId) && _catalog.FindPort(request.TargetPortId) == null) || string.IsNullOrWhiteSpace(request.BlockadeId) || string.IsNullOrWhiteSpace(request.Reason)) return Reject("blockade_request_invalid", "封锁必须有发起势力、目标、理由和证据。");
            var existing = FindBlockadeMutable(request.BlockadeId);
            if (request.Status == D14BlockadeStatus.Lifted && (existing == null || existing.Status != D14BlockadeStatus.Declared)) return Reject("blockade_not_active", "只有有效封锁可以解除。");
            if (request.Status == D14BlockadeStatus.Declared && existing != null && existing.Status == D14BlockadeStatus.Declared) return Duplicate("blockade_duplicate", "该封锁已经登记。", blockade: existing);
            var fact = AppendFact(request.Status == D14BlockadeStatus.Declared ? "blockade_declare" : "blockade_lift", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, request.Status == D14BlockadeStatus.Declared ? "blockade_declared" : "blockade_lifted", "封锁状态和解除条件由 FactionLegalSystem 保存。", new Dictionary<string, string> { ["blockade_id"] = request.BlockadeId, ["issuer_faction_id"] = request.IssuerFactionId, ["target_faction_id"] = request.TargetFactionId ?? "", ["target_port_id"] = request.TargetPortId ?? "", ["reason"] = request.Reason });
            var blockade = existing ?? new D14BlockadeState { BlockadeId = request.BlockadeId, IssuerFactionId = request.IssuerFactionId, TargetFactionId = request.TargetFactionId, TargetPortId = request.TargetPortId, StartedWorldDay = request.WorldDay };
            blockade.Status = request.Status; blockade.LastUpdatedWorldDay = request.WorldDay; blockade.Reason = request.Reason; blockade.SourceEventId = request.SourceEventId; blockade.EvidenceEventIds = new List<string>(request.EvidenceEventIds); blockade.LastEventId = fact.EventId;
            if (existing == null) _state.Blockades.Add(blockade);
            if (!string.IsNullOrWhiteSpace(request.TargetPortId))
            {
                var port = FindPortMutable(request.TargetPortId); port.AccessStatus = request.Status == D14BlockadeStatus.Declared ? D14PortAccessStatus.Blockaded : D14PortAccessStatus.Open; port.Reason = request.Reason; port.LastUpdatedWorldDay = request.WorldDay; port.SourceEventId = request.SourceEventId; port.EvidenceEventId = request.EvidenceEventIds[0]; port.LastEventId = fact.EventId;
            }
            if (!string.IsNullOrWhiteSpace(request.TargetFactionId))
            {
                var key = D14FactionLegalIds.PairKey(request.IssuerFactionId, request.TargetFactionId); var relation = FindRelationMutable(key) ?? new D14FactionRelationState { RelationKey = key, FactionA = request.IssuerFactionId, FactionB = request.TargetFactionId, StartedWorldDay = request.WorldDay };
                relation.Status = request.Status == D14BlockadeStatus.Declared ? D14FactionRelationStatus.Blockaded : D14FactionRelationStatus.Neutral; relation.LastUpdatedWorldDay = request.WorldDay; relation.SourceEventId = request.SourceEventId; relation.EvidenceEventId = request.EvidenceEventIds[0]; relation.LastEventId = fact.EventId; if (FindRelationMutable(key) == null) _state.Relations.Add(relation);
            }
            AddAudit(request.OperationId, request.Status == D14BlockadeStatus.Declared ? "blockade_declared" : "blockade_lifted", fact, request.SourceEventId, request.EvidenceEventIds, request.ActorRef, request.IssuerFactionId, request.WorldDay, request.Reason, D14FactionLegalIds.BlockadeRecoveryRoute);
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied(request.Status == D14BlockadeStatus.Declared ? "blockade_declared" : "blockade_lifted", request.Status == D14BlockadeStatus.Declared ? "封锁已登记。" : "封锁已解除。", fact, request.WorldDay, 0, 0, blockade: blockade);
        }

        private bool Prepare(D14FactionLegalOperationRequest request, string action, bool evidenceRequired, out FactEvent source, out D14FactionLegalResult failure)
        {
            source = null; failure = null;
            if (request == null) { failure = Reject("request_required", action + " 请求不能为空。"); return false; }
            if (string.Equals(request.ActorRef, "agent", StringComparison.OrdinalIgnoreCase) || string.Equals(request.ActorRef, "AgentReply", StringComparison.OrdinalIgnoreCase))
            {
                failure = Reject("agent_cannot_write", "Agent 只能提交外交意图，不能直接写入声望、法律或港口结果。");
                return false;
            }
            try { request.Validate("d14." + action, evidenceRequired); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; }
            if (IsApplied(request.OperationId)) { failure = Duplicate("operation_duplicate", "同一法律操作已应用，未重复改变状态。", null); return false; }
            source = _eventLog.Find(request.SourceEventId, request.SaveRevision);
            if (source == null) { failure = Reject("source_fact_missing", "法律操作必须引用 EventLog 中已有来源事实。"); return false; }
            foreach (var evidenceId in request.EvidenceEventIds)
                if (FindAnyFact(evidenceId) == null) { failure = Reject("evidence_fact_missing", "证据事实不在 EventLog 中：" + evidenceId); return false; }
            return true;
        }

        private bool IsPortOpenForInspection(string portId, string subjectRef, string factionId, int worldDay)
        {
            var port = FindPortMutable(portId); if (port == null) return false;
            return port.AccessStatus == D14PortAccessStatus.Open && GetWantedLevel(subjectRef) < 4;
        }

        private bool ValidEconomicAssessment(D14EconomicAssessmentDecision decision)
        {
            return decision != null && decision.Allowed && decision.Amount >= 0 && decision.SourceOwnerSystem == D14FactionLegalIds.EconomyOwner && FindOwnedFact(decision.SourceEventId, D14FactionLegalIds.EconomyOwner) != null;
        }

        private bool IsSuccessFact(FactEvent fact)
        {
            string code = fact == null || fact.Result == null ? string.Empty : fact.Result.ResultCode;
            return code == "escort_success" || code == "success" || code == "completed" || code == "protected_cargo";
        }

        private FactEvent AppendFact(string action, string operationId, int worldDay, long saveRevision, string actor, string sourceEventId, IEnumerable<string> evidence, IEnumerable<string> causes, string resultCode, string summary, Dictionary<string, string> values, WorldEventStatus outcome = WorldEventStatus.Normal, RecoveryRoute recovery = null)
        {
            var input = new List<string> { sourceEventId }; if (evidence != null) foreach (var id in evidence) if (!input.Contains(id)) input.Add(id);
            var causeIds = new List<string>(); if (causes != null) foreach (var id in causes) if (!string.IsNullOrWhiteSpace(id) && !causeIds.Contains(id)) causeIds.Add(id); if (!causeIds.Contains(sourceEventId)) causeIds.Insert(0, sourceEventId);
            return _eventLog.Append(new WorldEvent(D14FactionLegalIds.EventPrefix + D14FactionLegalIds.StableId(action) + "_" + D14FactionLegalIds.StableId(operationId), worldDay, "d14_faction_legal", OwnerSystem,
                new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { operationId }, Values = values }, new[] { string.IsNullOrWhiteSpace(actor) ? "player" : actor }, input, causeIds, saveRevision, outcome, recovery)).Fact;
        }

        private void AddAudit(string operationId, string action, FactEvent fact, string sourceEventId, IEnumerable<string> evidence, string subject, string faction, int worldDay, string reason, string route)
        {
            var audit = new D14FactionLegalAuditRecord { AuditId = "audit_" + D14FactionLegalIds.StableId(operationId), Action = action, EventId = fact.EventId, SourceEventId = sourceEventId, SubjectRef = subject, FactionId = faction, WorldDay = worldDay, Reason = reason, RecoveryRoute = route, EvidenceEventIds = evidence == null ? new List<string>() : new List<string>(evidence) };
            _state.Audits.Add(audit);
        }

        private void Touch(FactEvent fact, string operationId, int worldDay)
        {
            _state.Revision++; _state.WorldDay = worldDay; _state.LastEventId = fact.EventId;
            if (!string.IsNullOrWhiteSpace(operationId) && !_state.AppliedEventKeys.Contains("op:" + operationId)) _state.AppliedEventKeys.Add("op:" + operationId);
            if (!_state.AppliedEventKeys.Contains(fact.DedupeKey)) _state.AppliedEventKeys.Add(fact.DedupeKey);
        }

        private D14FactionLegalResult Applied(string code, string message, FactEvent fact, int worldDay, int before, int after,
            D14FactionRelationState relation = null, D14PrivateerPermitState permit = null, D14PortLegalState port = null,
            D14WantedCaseState wanted = null, D14InspectionRecord inspection = null, D14SeizureRecord seizure = null,
            D14FinancialCaseState financial = null, D14EscortRecord escort = null, D14BlockadeState blockade = null,
            D14DiplomaticIntentRecord intent = null)
        {
            return new D14FactionLegalResult { Accepted = true, Code = code, Message = message, EventId = fact.EventId, Fact = fact.CloneFact(),
                Relation = relation == null ? null : relation.Clone(), Permit = permit == null ? null : permit.Clone(), Port = port == null ? null : port.Clone(),
                WantedCase = wanted == null ? null : wanted.Clone(), Inspection = inspection == null ? null : inspection.Clone(), Seizure = seizure == null ? null : seizure.Clone(),
                FinancialCase = financial == null ? null : financial.Clone(), Escort = escort == null ? null : escort.Clone(), Blockade = blockade == null ? null : blockade.Clone(), DiplomaticIntent = intent == null ? null : intent.Clone(),
                ReputationBefore = before, ReputationAfter = after, State = CaptureSnapshot() };
        }

        private D14FactionLegalResult Duplicate(string code, string message, D14FactionRelationState relation = null, D14PrivateerPermitState permit = null, D14PortLegalState port = null, D14WantedCaseState wanted = null, D14InspectionRecord inspection = null, D14SeizureRecord seizure = null, D14FinancialCaseState financial = null, D14BlockadeState blockade = null, D14DiplomaticIntentRecord intent = null)
        {
            return new D14FactionLegalResult { Accepted = true, Duplicate = true, Code = code, Message = message, Relation = relation == null ? null : relation.Clone(), Permit = permit == null ? null : permit.Clone(), Port = port == null ? null : port.Clone(), WantedCase = wanted == null ? null : wanted.Clone(), Inspection = inspection == null ? null : inspection.Clone(), Seizure = seizure == null ? null : seizure.Clone(), FinancialCase = financial == null ? null : financial.Clone(), Blockade = blockade == null ? null : blockade.Clone(), DiplomaticIntent = intent == null ? null : intent.Clone(), State = CaptureSnapshot() };
        }

        private D14FactionLegalResult Reject(string code, string message) { return D14FactionLegalResult.Rejected(code, message, CaptureSnapshot()); }
        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
        private static List<string> MergeCauses(IEnumerable<string> existing, params string[] additions) { var result = new List<string>(); if (existing != null) foreach (var value in existing) if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value); if (additions != null) foreach (var value in additions) if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value); return result; }

        private D14FactionRelationState FindRelationMutable(string key) { foreach (var value in _state.Relations) if (value.RelationKey == key) return value; return null; }
        private D14PrivateerPermitState FindPermitMutable(string id) { foreach (var value in _state.Permits) if (value.PermitId == id) return value; return null; }
        private D14PortLegalState FindPortMutable(string id) { foreach (var value in _state.Ports) if (value.PortId == id) return value; return null; }
        private D14WantedCaseState FindWantedMutable(string id) { foreach (var value in _state.WantedCases) if (value.CaseId == id) return value; return null; }
        private D14SeizureRecord FindSeizureMutable(string id) { foreach (var value in _state.Seizures) if (value.SeizureId == id) return value; return null; }
        private D14FinancialCaseState FindFinancialMutable(string id) { foreach (var value in _state.FinancialCases) if (value.CaseId == id) return value; return null; }
        private D14BlockadeState FindBlockadeMutable(string id) { foreach (var value in _state.Blockades) if (value.BlockadeId == id) return value; return null; }
        private bool IsApplied(string operationId) { return _state.AppliedEventKeys.Contains("op:" + operationId); }

        private FactEvent FindAnyFact(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            foreach (var fact in _eventLog.Facts) if (fact.EventId == eventId) return fact;
            return null;
        }

        private FactEvent FindOwnedFact(string eventId, string owner)
        {
            var fact = FindAnyFact(eventId); return fact != null && fact.OwnerSystem == owner ? fact : null;
        }
    }
}
