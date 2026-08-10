// D14-A04 RewardSystem 唯一奖励状态写者。
// 本系统不向 Inventory/Economy/Collection 写最终数值；它只接受已落盘事实，
// 写奖励请求、发放/领取/丢失/恢复记录和审计，并保存所属系统的结算事实引用。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14RewardSystem
    {
        private readonly EventLog _eventLog;
        private readonly D14RewardCatalog _catalog;
        private readonly ID14RewardSettlementAuthority _settlement;
        private readonly ID14RewardRecoveryAuthority _recovery;
        private readonly D14RewardRuntimeSnapshot _state;

        public D14RewardSystem(EventLog eventLog = null, D14RewardCatalog catalog = null,
            ID14RewardSettlementAuthority settlement = null, ID14RewardRecoveryAuthority recovery = null,
            D14RewardRuntimeSnapshot initialSnapshot = null)
        {
            _catalog = catalog ?? new D14RewardCatalog();
            _settlement = settlement;
            _recovery = recovery;
            if (eventLog != null) _eventLog = eventLog;
            else if (initialSnapshot != null && initialSnapshot.EventLog != null) _eventLog = new EventLog(initialSnapshot.EventLog);
            else _eventLog = new EventLog();
            _state = initialSnapshot == null ? new D14RewardRuntimeSnapshot() : initialSnapshot.Clone();
            if (_state.EventLog == null) _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
        }

        public const string OwnerSystem = D14RewardIds.OwnerSystem;
        public EventLog EventLog { get { return _eventLog; } }
        public D14RewardRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }

        public IReadOnlyList<D14RewardDefinition> Definitions { get { return _catalog.All(); } }

        public D14RewardDefinition FindDefinition(string rewardId) { return _catalog.Find(rewardId); }

        public D14RewardRequest FindRequest(string requestId)
        {
            var request = FindRequestMutable(requestId);
            return request == null ? null : request.Clone();
        }

        public D14RewardGrantRecord FindGrant(string grantId)
        {
            var grant = FindGrantMutable(grantId);
            return grant == null ? null : grant.Clone();
        }

        public IReadOnlyList<D14RewardAuditRecord> ReadAudits()
        {
            var result = new List<D14RewardAuditRecord>();
            foreach (var audit in _state.Audits) result.Add(audit.Clone());
            return result.AsReadOnly();
        }

        /// <summary>Quest/所属系统只能提交请求；此方法不发放最终物品、金钱或数量。</summary>
        public D14RewardResult Request(D14RewardRequest request)
        {
            if (request == null) return Reject("request_required", "奖励请求不能为空。");
            try { request.Validate("d14.reward.request"); }
            catch (Exception ex) { return Reject("invalid_request", ex.Message); }

            var definition = _catalog.Find(request.RewardId);
            if (definition == null) return Reject("reward_unknown", "奖励目录中不存在该 reward_id。");
            var source = FindFact(request.SourceEventId, request.SourceSaveRevision);
            if (source == null) return Reject("source_fact_missing", "奖励请求必须引用 EventLog 中已有来源事实。");
            if (!definition.AllowedSourceOwnerSystems.Contains(source.OwnerSystem))
                return Reject("source_owner_not_allowed", "来源事实的所属系统不允许请求该奖励。");

            var duplicate = FindRequestMutable(request.RequestId);
            if (duplicate != null)
            {
                if (!SameRequestIdentity(duplicate, request))
                    return Reject("request_id_conflict", "同一 request_id 不能指向不同奖励、领取者或来源事实。");
                return Duplicate("request_duplicate", "相同 request_id 已提交，未重复写入。", duplicate, FindGrantForRequest(duplicate.RequestId));
            }

            var accepted = request.Clone();
            accepted.Status = D14RewardRequestStatus.Pending;
            var fact = AppendFact("request", request.RequestId, request.WorldDay, request.SaveRevision, request.RecipientRef,
                "reward_requested", "奖励请求已登记，等待 RewardSystem 发放裁决。", request.SourceEventId,
                request.CauseEventIds, new Dictionary<string, string>
                {
                    ["request_id"] = request.RequestId,
                    ["reward_id"] = request.RewardId,
                    ["recipient_ref"] = request.RecipientRef,
                    ["requested_by_system"] = request.RequestedBySystem,
                });
            _state.Requests.Add(accepted);
            AddAudit("audit_request_" + D14RewardIds.StableId(request.RequestId), null, accepted, "requested", fact,
                "奖励请求来自已登记事实；未携带最终奖励值。", null);
            Touch(fact, request.WorldDay);
            return Applied("reward_requested", "奖励请求已登记，尚未发放。", fact, accepted, null);
        }

        public D14RewardResult RequestTaskReward(D14TaskRewardRequest taskRequest)
        {
            if (taskRequest == null) return Reject("request_required", "任务奖励请求不能为空。");
            return Request(new D14RewardRequest
            {
                RequestId = taskRequest.RequestId,
                RewardId = taskRequest.RewardId,
                TaskId = taskRequest.TaskId,
                RecipientRef = taskRequest.RecipientRef,
                RequestedBySystem = "QuestSystem",
                SourceEventId = taskRequest.SourceEventId,
                SourceSaveRevision = taskRequest.SourceSaveRevision,
                WorldDay = taskRequest.WorldDay,
                SaveRevision = taskRequest.SaveRevision,
                CauseEventIds = taskRequest.CauseEventIds == null ? new List<string>() : new List<string>(taskRequest.CauseEventIds),
            });
        }

        /// <summary>RewardSystem 发放奖励记录；实际库存/金钱/图鉴仍由所属系统写入。</summary>
        public D14RewardResult Grant(D14RewardGrantRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RequestId) || request.WorldDay < 0 || request.SaveRevision < 0)
                return Reject("invalid_grant_request", "奖励发放请求缺少 request_id 或时间版本。");
            var rewardRequest = FindRequestMutable(request.RequestId);
            if (rewardRequest == null) return Reject("request_missing", "找不到待发放的奖励请求。");
            if (rewardRequest.Status == D14RewardRequestStatus.Granted || rewardRequest.Status == D14RewardRequestStatus.Claimed || rewardRequest.Status == D14RewardRequestStatus.Recovered)
                return Duplicate("grant_duplicate", "该奖励请求已经发放，未重复写入。", rewardRequest, FindGrantForRequest(rewardRequest.RequestId));
            if (rewardRequest.Status != D14RewardRequestStatus.Pending)
                return Reject("request_not_pending", "只有 pending 奖励请求可以发放。");

            var definition = _catalog.Find(rewardRequest.RewardId);
            var source = FindFact(rewardRequest.SourceEventId, rewardRequest.SourceSaveRevision);
            if (definition == null || source == null) return Reject("source_or_definition_missing", "奖励目录或来源事实缺失。");
            if (definition.Unique && HasActiveUniqueGrant(definition.RewardId, rewardRequest.RecipientRef))
                return Reject("unique_already_granted", "唯一奖励已向该领取者发放，拒绝第二个实例。");
            if (_settlement == null) return Reject("settlement_authority_required", "RewardSystem 必须收到所属系统的结算引用决定。");

            string grantId = D14RewardIds.GrantId(definition.RewardId, rewardRequest.RecipientRef, rewardRequest.SourceEventId);
            var decision = _settlement.Settle(new D14RewardSettlementRequest
            {
                GrantId = grantId, RecipientRef = rewardRequest.RecipientRef, Definition = definition.Clone(),
                RewardRequest = rewardRequest.Clone(), SourceFact = source.CloneFact(),
            });
            string failureCode;
            if (!ValidateSettlementDecision(decision, definition, source, out failureCode))
                return Reject(failureCode, decision == null ? "所属系统未返回结算决定。" : decision.Message);

            var settlementGate = _eventLog.ClaimSettlement(source.EventId, source.SaveRevision);
            if (!settlementGate.CanApplyOwnerSettlement)
                return Reject("source_already_settled", "该来源事实已经被结算，读档或重复请求不会再次发放。");

            var fact = AppendFact("granted", grantId, request.WorldDay, request.SaveRevision, rewardRequest.RecipientRef,
                "reward_granted", "奖励记录已由 RewardSystem 发放；最终物品/金钱由所属系统结算。", source.EventId,
                new[] { source.EventId, EventId("request", rewardRequest.RequestId) },
                new Dictionary<string, string>
                {
                    ["grant_id"] = grantId, ["request_id"] = rewardRequest.RequestId,
                    ["reward_id"] = definition.RewardId, ["rarity"] = definition.Rarity.ToString(),
                    ["content_id"] = definition.ContentId ?? "",
                });
            var grant = new D14RewardGrantRecord
            {
                GrantId = grantId, RequestId = rewardRequest.RequestId, RewardId = definition.RewardId,
                ContentId = definition.ContentId, Rarity = definition.Rarity, Kind = definition.Kind, Unique = definition.Unique,
                RecipientRef = rewardRequest.RecipientRef, Status = D14RewardStatus.Granted,
                SourceEventId = source.EventId, SourceSaveRevision = source.SaveRevision,
                GrantEventId = fact.EventId, GrantSaveRevision = fact.SaveRevision,
                InventorySourceEventId = decision.InventorySourceEventId, EconomySourceEventId = decision.EconomySourceEventId,
                CollectionSourceEventId = decision.CollectionSourceEventId, RecoveryRoute = definition.RecoveryAllowed ? D14RewardIds.RecoveryRoute : null,
                LastEventId = fact.EventId,
            };
            _state.Grants.Add(grant);
            rewardRequest.Status = D14RewardRequestStatus.Granted;
            AddAudit("audit_grant_" + D14RewardIds.StableId(grantId), grant, rewardRequest, "granted", fact,
                "奖励发放已绑定来源事实与所属系统结算引用。", grant.RecoveryRoute);
            Touch(fact, request.WorldDay);
            return Applied("reward_granted", "奖励已记录并交给所属系统结算。", fact, rewardRequest, grant);
        }

        public D14RewardResult Claim(D14RewardClaimRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OperationId) || string.IsNullOrWhiteSpace(request.GrantId) ||
                string.IsNullOrWhiteSpace(request.RecipientRef) || request.WorldDay < 0 || request.SaveRevision < 0)
                return Reject("invalid_claim_request", "领取请求缺少操作、奖励、领取者或时间版本。");
            var grant = FindGrantMutable(request.GrantId);
            if (grant == null) return Reject("grant_missing", "找不到奖励发放记录。");
            if (grant.Status == D14RewardStatus.Claimed || grant.Status == D14RewardStatus.Recovered)
                return Duplicate("claim_duplicate", "奖励已领取或已恢复，未重复结算。", FindRequestMutable(grant.RequestId), grant);
            if (grant.Status != D14RewardStatus.Granted) return Reject("grant_not_claimable", "当前奖励状态不能领取。");
            if (!string.Equals(grant.RecipientRef, request.RecipientRef, StringComparison.Ordinal))
                return Reject("recipient_mismatch", "奖励领取者与发放记录不一致。");
            var grantFact = FindFact(grant.GrantEventId, grant.GrantSaveRevision);
            if (grantFact == null) return Reject("grant_fact_missing", "奖励发放事实不在统一事件日志中。");
            var gate = _eventLog.ClaimSettlement(grantFact.EventId, grantFact.SaveRevision);
            if (!gate.CanApplyOwnerSettlement) return Reject("claim_already_settled", "该奖励发放事实已经领取。");
            var fact = AppendFact("claimed", grant.GrantId + "_" + request.OperationId, request.WorldDay, request.SaveRevision,
                request.RecipientRef, "reward_claimed", "奖励领取记录已保存；不重复写入所属系统的最终值。", grantFact.EventId,
                new[] { grantFact.EventId }, new Dictionary<string, string> { ["grant_id"] = grant.GrantId, ["recipient_ref"] = request.RecipientRef });
            grant.Status = D14RewardStatus.Claimed;
            grant.ClaimEventId = fact.EventId; grant.LastEventId = fact.EventId;
            var rewardRequest = FindRequestMutable(grant.RequestId);
            if (rewardRequest != null) rewardRequest.Status = D14RewardRequestStatus.Claimed;
            AddAudit("audit_claim_" + D14RewardIds.StableId(request.OperationId), grant, rewardRequest, "claimed", fact,
                "领取只写 RewardSystem 领取记录，未复制库存或金钱状态。", null);
            Touch(fact, request.WorldDay);
            return Applied("reward_claimed", "奖励领取事实已保存。", fact, rewardRequest, grant);
        }

        public D14RewardResult MarkLost(D14RewardLossRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OperationId) || string.IsNullOrWhiteSpace(request.GrantId) ||
                string.IsNullOrWhiteSpace(request.SourceEventId) || request.WorldDay < 0 || request.SaveRevision < 0 || request.SourceSaveRevision < 0)
                return Reject("invalid_loss_request", "丢失记录缺少操作、奖励、来源或时间版本。");
            var grant = FindGrantMutable(request.GrantId);
            if (grant == null) return Reject("grant_missing", "找不到奖励发放记录。");
            if (grant.Status == D14RewardStatus.Lost) return Duplicate("loss_duplicate", "该奖励已记录为丢失。", FindRequestMutable(grant.RequestId), grant);
            if (grant.Status == D14RewardStatus.Denied || grant.Status == D14RewardStatus.Expired)
                return Reject("grant_unavailable", "拒绝或过期奖励不能记录丢失。");
            var source = FindFact(request.SourceEventId, request.SourceSaveRevision);
            if (source == null || (source.OwnerSystem != D14RewardIds.InventoryOwner && source.OwnerSystem != "ShipSystem" && source.OwnerSystem != "FactionLegalSystem"))
                return Reject("loss_owner_invalid", "丢失必须来自 Inventory/Ship/Legal 已登记事实。");
            var fact = AppendFact("lost", grant.GrantId + "_" + request.OperationId, request.WorldDay, request.SaveRevision,
                grant.RecipientRef, "reward_lost", "奖励丢失已记录，是否可恢复由目录窗口和恢复裁决决定。", source.EventId,
                new[] { source.EventId, grant.LastEventId }, new Dictionary<string, string>
                {
                    ["grant_id"] = grant.GrantId, ["loss_reason"] = request.Reason.ToString(),
                    ["recovery_route"] = grant.RecoveryRoute ?? "",
                }, WorldEventStatus.Failed, new RecoveryRoute
                {
                    RouteId = D14RewardIds.RecoveryRoute, Action = "request_recovery",
                    ResumeFromEventId = grant.LastEventId, Description = "通过 RewardSystem 提交带来源的恢复请求。",
                });
            grant.Status = D14RewardStatus.Lost; grant.LossEventId = fact.EventId; grant.LossReason = request.Reason;
            grant.LostWorldDay = request.WorldDay; grant.LastEventId = fact.EventId;
            AddAudit("audit_loss_" + D14RewardIds.StableId(request.OperationId), grant, FindRequestMutable(grant.RequestId), "lost", fact,
                "丢失事实来自所属系统，奖励记录进入可审计恢复路线。", grant.RecoveryRoute);
            Touch(fact, request.WorldDay);
            return Applied("reward_lost", "奖励丢失已记录，可按窗口申请恢复。", fact, FindRequestMutable(grant.RequestId), grant);
        }

        public D14RewardResult Recover(D14RewardRecoveryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OperationId) || string.IsNullOrWhiteSpace(request.GrantId) ||
                string.IsNullOrWhiteSpace(request.RecoverySourceEventId) || string.IsNullOrWhiteSpace(request.RecipientRef) ||
                request.WorldDay < 0 || request.SaveRevision < 0 || request.RecoverySourceSaveRevision < 0)
                return Reject("invalid_recovery_request", "恢复请求缺少操作、奖励、恢复来源、领取者或时间版本。");
            var grant = FindGrantMutable(request.GrantId);
            if (grant == null) return Reject("grant_missing", "找不到丢失奖励记录。");
            if (grant.Status == D14RewardStatus.Recovered) return Duplicate("recovery_duplicate", "该奖励已恢复，未重复恢复。", FindRequestMutable(grant.RequestId), grant);
            if (grant.Status != D14RewardStatus.Lost) return Reject("recovery_not_allowed", "只有已记录为 Lost 的奖励可以恢复。");
            if (!string.Equals(grant.RecipientRef, request.RecipientRef, StringComparison.Ordinal)) return Reject("recipient_mismatch", "恢复领取者不一致。");
            var definition = _catalog.Find(grant.RewardId);
            if (definition == null || !definition.RecoveryAllowed) return Reject("recovery_disabled", "该奖励目录关闭了丢失恢复。");
            if (grant.LostWorldDay < 0 || request.WorldDay < grant.LostWorldDay || request.WorldDay - grant.LostWorldDay > definition.RecoveryWindowDays)
                return Reject("recovery_window_expired", "恢复请求超过目录规定的可恢复窗口。");
            var source = FindFact(request.RecoverySourceEventId, request.RecoverySourceSaveRevision);
            if (source == null) return Reject("recovery_source_missing", "恢复必须引用统一事件日志中的来源事实。");
            if (_recovery == null) return Reject("recovery_authority_required", "恢复必须由 RewardSystem 恢复裁决器确认。");
            var decision = _recovery.Recover(request, grant.Clone(), definition.Clone(), source.CloneFact());
            string failureCode;
            if (!ValidateRecoveryDecision(decision, source, out failureCode)) return Reject(failureCode, decision == null ? "恢复裁决器未返回决定。" : decision.Message);
            var fact = AppendFact("recovered", grant.GrantId + "_" + request.OperationId, request.WorldDay, request.SaveRevision,
                request.RecipientRef, "reward_recovered", "奖励丢失恢复已绑定丢失事实和恢复来源，原 grant_id 保持不变。", source.EventId,
                new[] { grant.LossEventId, source.EventId }, new Dictionary<string, string>
                {
                    ["grant_id"] = grant.GrantId, ["recovered_from_event_id"] = grant.LossEventId,
                    ["recovery_route"] = D14RewardIds.RecoveryRoute,
                }, WorldEventStatus.Recovered, null);
            grant.Status = D14RewardStatus.Recovered; grant.RecoveryEventId = fact.EventId; grant.LastEventId = fact.EventId;
            var rewardRequest = FindRequestMutable(grant.RequestId);
            if (rewardRequest != null) rewardRequest.Status = D14RewardRequestStatus.Recovered;
            AddAudit("audit_recovery_" + D14RewardIds.StableId(request.OperationId), grant, rewardRequest, "recovered", fact,
                "恢复只恢复同一奖励记录的可追踪状态，不创建第二个唯一奖励。", D14RewardIds.RecoveryRoute);
            Touch(fact, request.WorldDay);
            return Applied("reward_recovered", "奖励已按审计恢复路线恢复，唯一 grant_id 未改变。", fact, rewardRequest, grant);
        }

        public D14RewardRuntimeSnapshot CaptureSnapshot()
        {
            _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
            return _state.Clone();
        }

        private D14RewardRequest FindRequestMutable(string requestId)
        {
            foreach (var request in _state.Requests) if (request.RequestId == requestId) return request;
            return null;
        }

        private D14RewardGrantRecord FindGrantMutable(string grantId)
        {
            foreach (var grant in _state.Grants) if (grant.GrantId == grantId) return grant;
            return null;
        }

        private D14RewardGrantRecord FindGrantForRequest(string requestId)
        {
            foreach (var grant in _state.Grants) if (grant.RequestId == requestId) return grant;
            return null;
        }

        private bool HasActiveUniqueGrant(string rewardId, string recipientRef)
        {
            foreach (var grant in _state.Grants)
                if (grant.RewardId == rewardId && grant.RecipientRef == recipientRef && grant.Unique &&
                    grant.Status != D14RewardStatus.Denied && grant.Status != D14RewardStatus.Expired)
                    return true;
            return false;
        }

        private bool SameRequestIdentity(D14RewardRequest left, D14RewardRequest right)
        {
            return left != null && right != null && left.RewardId == right.RewardId && left.RecipientRef == right.RecipientRef &&
                left.SourceEventId == right.SourceEventId && left.SourceSaveRevision == right.SourceSaveRevision;
        }

        private FactEvent FindFact(string eventId, long saveRevision) { return _eventLog.Find(eventId, saveRevision); }

        private bool ValidateSettlementDecision(D14RewardSettlementDecision decision, D14RewardDefinition definition, FactEvent source, out string code)
        {
            code = "settlement_decision_invalid";
            if (decision == null || !decision.Allowed) { if (decision != null && !string.IsNullOrWhiteSpace(decision.Code)) code = decision.Code; return false; }
            if (decision.SourceOwnerSystem != D14RewardIds.OwnerSystem || decision.SourceEventId != source.EventId) return false;
            if (!ValidOwnerRef(decision.InventorySourceEventId, D14RewardIds.InventoryOwner) ||
                !ValidOwnerRef(decision.EconomySourceEventId, D14RewardIds.EconomyOwner) ||
                !ValidOwnerRef(decision.CollectionSourceEventId, D14RewardIds.CollectionOwner)) return false;
            if ((definition.Kind == D14RewardKind.Item || definition.Kind == D14RewardKind.ShipModule) && string.IsNullOrWhiteSpace(decision.InventorySourceEventId))
            { code = "inventory_settlement_source_missing"; return false; }
            if (definition.Kind == D14RewardKind.Collection && string.IsNullOrWhiteSpace(decision.CollectionSourceEventId))
            { code = "collection_settlement_source_missing"; return false; }
            return true;
        }

        private bool ValidateRecoveryDecision(D14RewardRecoveryDecision decision, FactEvent source, out string code)
        {
            code = "recovery_decision_invalid";
            if (decision == null || !decision.Allowed) { if (decision != null && !string.IsNullOrWhiteSpace(decision.Code)) code = decision.Code; return false; }
            if (decision.SourceOwnerSystem != D14RewardIds.RecoveryOwner || decision.SourceEventId != source.EventId) return false;
            if (!ValidOwnerRef(decision.InventorySourceEventId, D14RewardIds.InventoryOwner) ||
                !ValidOwnerRef(decision.EconomySourceEventId, D14RewardIds.EconomyOwner) ||
                !ValidOwnerRef(decision.CollectionSourceEventId, D14RewardIds.CollectionOwner)) return false;
            return true;
        }

        private bool ValidOwnerRef(string eventId, string owner)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return true;
            var fact = _eventLog.Find(eventId, FindSaveRevision(eventId));
            return fact != null && fact.OwnerSystem == owner;
        }

        private long FindSaveRevision(string eventId)
        {
            foreach (var fact in _eventLog.Facts) if (fact.EventId == eventId) return fact.SaveRevision;
            return -1;
        }

        private FactEvent AppendFact(string action, string stableId, int worldDay, long saveRevision, string actor,
            string resultCode, string summary, string sourceEventId, IEnumerable<string> causes,
            Dictionary<string, string> values, WorldEventStatus outcome = WorldEventStatus.Normal, RecoveryRoute recoveryRoute = null)
        {
            var causeIds = new List<string>();
            if (causes != null) foreach (var cause in causes) if (!string.IsNullOrWhiteSpace(cause) && !causeIds.Contains(cause)) causeIds.Add(cause);
            if (!string.IsNullOrWhiteSpace(sourceEventId) && !causeIds.Contains(sourceEventId)) causeIds.Insert(0, sourceEventId);
            var result = _eventLog.Append(new WorldEvent(
                EventId("" + action, stableId), worldDay, "d14_reward", OwnerSystem,
                new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { stableId }, Values = values },
                string.IsNullOrWhiteSpace(actor) ? new[] { "player" } : new[] { actor },
                new[] { sourceEventId ?? "" }, causeIds, saveRevision, outcome, recoveryRoute));
            return result.Fact;
        }

        private string EventId(string action, string stableId)
        {
            return D14RewardIds.EventPrefix + D14RewardIds.StableId(action) + "_" + D14RewardIds.StableId(stableId);
        }

        private void AddAudit(string auditId, D14RewardGrantRecord grant, D14RewardRequest request, string action, FactEvent fact, string reason, string route)
        {
            _state.Audits.Add(new D14RewardAuditRecord
            {
                AuditId = auditId, GrantId = grant == null ? null : grant.GrantId,
                RequestId = request == null ? null : request.RequestId, Action = action,
                EventId = fact.EventId, SaveRevision = fact.SaveRevision, WorldDay = fact.WorldDay,
                OwnerSystem = OwnerSystem, SourceEventId = grant == null ? fact.EventId : grant.SourceEventId,
                Reason = reason, RecoveryRoute = route,
            });
        }

        private void Touch(FactEvent fact, int worldDay)
        {
            _state.Revision++;
            _state.WorldDay = worldDay;
            _state.LastEventId = fact.EventId;
            string key = fact.DedupeKey;
            if (!_state.AppliedEventKeys.Contains(key)) _state.AppliedEventKeys.Add(key);
        }

        private D14RewardResult Applied(string code, string message, FactEvent fact, D14RewardRequest request, D14RewardGrantRecord grant)
        {
            return new D14RewardResult { Accepted = true, Code = code, Message = message, EventId = fact.EventId,
                Fact = fact.CloneFact(), Request = request == null ? null : request.Clone(), Grant = grant == null ? null : grant.Clone(), State = CaptureSnapshot() };
        }

        private D14RewardResult Duplicate(string code, string message, D14RewardRequest request, D14RewardGrantRecord grant)
        {
            return new D14RewardResult { Accepted = true, Duplicate = true, Code = code, Message = message,
                Request = request == null ? null : request.Clone(), Grant = grant == null ? null : grant.Clone(), State = CaptureSnapshot() };
        }

        private D14RewardResult Reject(string code, string message)
        {
            return D14RewardResult.Rejected(code, message, CaptureSnapshot());
        }
    }
}
