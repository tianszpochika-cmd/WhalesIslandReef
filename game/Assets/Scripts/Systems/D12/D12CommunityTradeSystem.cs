// ============================================================
// D12CommunityTradeSystem —— 社群礼仪/边界/物物交换唯一裁决者
//
// 该系统只写社群库存、社群访问结果和玩家-社群长期声望；玩家货物仍
// 通过 D3InventorySystem 修改。所有业务结果先进入 EventLog，再提交
// 状态。PropertyIslandSystem 是土地/保护区定义的 owner，本系统只读取
// 边界并拒绝越界交易。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12CommunityTradeSystem
    {
        private readonly Dictionary<string, D12CommunityDefinition> _definitions =
            new Dictionary<string, D12CommunityDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12CommunityBoundaryDefinition> _boundaries =
            new Dictionary<string, D12CommunityBoundaryDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12CommunityInventoryState> _inventories =
            new Dictionary<string, D12CommunityInventoryState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12CommunityReputationState> _reputations =
            new Dictionary<string, D12CommunityReputationState>(StringComparer.Ordinal);
        private readonly EventLog _eventLog;
        private long _revision;
        private int _lastWorldDay;
        private string _lastEventId;

        public D12CommunityTradeSystem(IEnumerable<D12CommunityDefinition> definitions,
            IEnumerable<D12CommunityBoundaryDefinition> boundaries, EventLog eventLog)
            : this(definitions, boundaries, eventLog, null)
        {
        }

        public D12CommunityTradeSystem(IEnumerable<D12CommunityDefinition> definitions,
            IEnumerable<D12CommunityBoundaryDefinition> boundaries, EventLog eventLog,
            D12CommunityTradeSnapshot savedSnapshot)
        {
            if (definitions == null) throw new ArgumentNullException("definitions");
            if (boundaries == null) throw new ArgumentNullException("boundaries");
            _eventLog = eventLog ?? (savedSnapshot == null || savedSnapshot.EventLog == null
                ? new EventLog() : EventLog.FromSnapshot(savedSnapshot.EventLog));

            foreach (var source in definitions)
            {
                if (source == null) throw new InvalidOperationException("社群目录不能包含 null。");
                var definition = source.Clone();
                definition.Validate();
                if (_definitions.ContainsKey(definition.Id)) throw new InvalidOperationException("社群目录重复：" + definition.Id);
                _definitions.Add(definition.Id, definition);
                var state = new D12CommunityInventoryState { CommunityId = definition.Id };
                foreach (var pair in definition.InitialStock) state.Stock[pair.Key] = Math.Max(0, pair.Value);
                _inventories.Add(definition.Id, state);
            }

            foreach (var source in boundaries)
            {
                if (source == null) throw new InvalidOperationException("社群边界目录不能包含 null。");
                var boundary = source.Clone();
                boundary.Validate();
                if (!_definitions.ContainsKey(boundary.CommunityId)) throw new InvalidOperationException("边界引用未知社群：" + boundary.Id);
                if (_boundaries.ContainsKey(boundary.Id)) throw new InvalidOperationException("边界目录重复：" + boundary.Id);
                _boundaries.Add(boundary.Id, boundary);
            }

            foreach (var definition in _definitions.Values)
            {
                if (!_boundaries.ContainsKey(definition.LandBoundaryId) || !_boundaries.ContainsKey(definition.ProtectedAreaId))
                    throw new InvalidOperationException("社群边界引用不完整：" + definition.Id);
            }

            if (savedSnapshot != null)
            {
                savedSnapshot.Validate();
                _revision = savedSnapshot.Revision;
                _lastWorldDay = savedSnapshot.LastWorldDay;
                _lastEventId = savedSnapshot.LastEventId;
                if (savedSnapshot.Communities != null)
                    foreach (var saved in savedSnapshot.Communities)
                    {
                        if (!_inventories.ContainsKey(saved.CommunityId)) throw new InvalidOperationException("存档包含未知社群库存：" + saved.CommunityId);
                        _inventories[saved.CommunityId] = saved.Clone();
                    }
                if (savedSnapshot.Reputations != null)
                    foreach (var saved in savedSnapshot.Reputations)
                    {
                        if (!_definitions.ContainsKey(saved.CommunityId)) throw new InvalidOperationException("存档包含未知社群声望：" + saved.CommunityId);
                        _reputations[D12CommunityTradeSnapshot.Key(saved.ActorRef, saved.CommunityId)] = saved.Clone();
                    }
            }
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D12CommunityTradeSnapshot Snapshot { get { return CaptureSnapshot(); } }

        public D12CommunityTradeSnapshot CaptureSnapshot()
        {
            var result = new D12CommunityTradeSnapshot
            {
                LastWorldDay = _lastWorldDay,
                Revision = _revision,
                LastEventId = _lastEventId,
                EventLog = _eventLog.CaptureSnapshot(),
            };
            foreach (var state in _inventories.Values) result.Communities.Add(state.Clone());
            foreach (var state in _reputations.Values) result.Reputations.Add(state.Clone());
            result.Validate();
            return result;
        }

        /// <summary>只读社群投影，库存/声望均为副本。</summary>
        public IReadOnlyList<D12CommunityView> GetCommunityReadModel(string actorRef = "player")
        {
            var result = new List<D12CommunityView>();
            foreach (var definition in _definitions.Values)
            {
                var state = _inventories[definition.Id];
                result.Add(new D12CommunityView
                {
                    CommunityId = definition.Id,
                    DisplayNameKey = definition.DisplayNameKey,
                    SocialInstitution = definition.SocialInstitution,
                    HistoricalTag = definition.HistoricalTag,
                    SensitiveContentLabel = definition.SensitiveContentLabel,
                    ContentReviewStatus = definition.ContentReviewStatus,
                    LandBoundaryId = definition.LandBoundaryId,
                    ProtectedAreaId = definition.ProtectedAreaId,
                    Stock = CopyStock(state.Stock),
                    Reputation = GetReputation(actorRef, definition.Id),
                });
            }
            return result;
        }

        public D12CommunityDefinition GetDefinition(string communityId)
        {
            D12CommunityDefinition definition;
            return _definitions.TryGetValue(communityId ?? string.Empty, out definition) ? definition.Clone() : null;
        }

        public D12CommunityBoundaryDefinition GetBoundary(string boundaryId)
        {
            D12CommunityBoundaryDefinition boundary;
            return _boundaries.TryGetValue(boundaryId ?? string.Empty, out boundary) ? boundary.Clone() : null;
        }

        public D12CommunityReputationState GetReputation(string actorRef, string communityId)
        {
            actorRef = string.IsNullOrWhiteSpace(actorRef) ? "player" : actorRef;
            D12CommunityReputationState state;
            if (_reputations.TryGetValue(D12CommunityTradeSnapshot.Key(actorRef, communityId), out state)) return state.Clone();
            return new D12CommunityReputationState { ActorRef = actorRef, CommunityId = communityId };
        }

        /// <summary>
        /// Agent/船员只能生成交易意图；此方法不追加事实、不写库存、不改声望。
        /// </summary>
        public D12CommunityTradeIntent ProposeTrade(D12CommunityTradeIntent intent)
        {
            if (intent == null || string.IsNullOrWhiteSpace(intent.RequestId) || string.IsNullOrWhiteSpace(intent.CommunityId)) return null;
            if (!_definitions.ContainsKey(intent.CommunityId)) return null;
            var result = intent.Clone();
            result.ProposedByAgent = true;
            return result;
        }

        public D12CommunityTradeResult SettleTrade(D12CommunityTradeIntent intent, D3InventoryState playerInventory)
        {
            if (intent == null) return Invalid("invalid_request", "社群交易意图不能为空。", null);
            if (string.IsNullOrWhiteSpace(intent.RequestId) || string.IsNullOrWhiteSpace(intent.CommunityId) ||
                string.IsNullOrWhiteSpace(intent.ActorRef) || string.IsNullOrWhiteSpace(intent.LocationRef))
                return Invalid("invalid_request", "社群交易必须有请求、社群、行动者和位置。", intent.CommunityId);
            D12CommunityDefinition definition;
            if (!_definitions.TryGetValue(intent.CommunityId, out definition))
                return Invalid("community_unknown", "未知社群不会被当作默认商店处理。", intent.CommunityId);
            if (intent.GiveQuantity <= 0 || intent.WantQuantity <= 0 || intent.WorldDay < 0 || intent.SaveRevision < 0)
                return Invalid("invalid_trade_parameters", "物物交换数量、世界日或存档修订号非法。", intent.CommunityId);

            string eventId = TradeEventId(intent.RequestId);
            var duplicate = _eventLog.Find(eventId, intent.SaveRevision);
            if (duplicate != null) return DuplicateResult(duplicate, intent, definition);

            var boundaryFailure = CheckBoundary(definition, intent.LocationRef, intent.BoundaryConsentRef);
            if (boundaryFailure != null)
                return Refuse(intent, definition, boundaryFailure.Code, boundaryFailure.Message,
                    boundaryFailure.RouteId, boundaryFailure.ReputationDelta, boundaryFailure.TrustDelta, D12CommunityTradeDisposition.Refused);

            bool languageKnown = string.Equals(intent.LanguageId, definition.RequiredLanguageId, StringComparison.Ordinal) &&
                intent.LanguageLevel >= definition.RequiredLanguageLevel;
            if (!languageKnown)
                return Refuse(intent, definition, "misunderstood_language", "语言门槛不足，社群无法确认这次交换意图。",
                    "recovery_request_translation", -2, -3, D12CommunityTradeDisposition.Misunderstood);
            if (intent.KnowledgeLevel < definition.RequiredKnowledgeLevel)
                return Refuse(intent, definition, "knowledge_required", "社群要求先了解当地交换规约。",
                    "recovery_learn_community_knowledge", 0, 0, D12CommunityTradeDisposition.Refused);
            if (intent.Etiquette != definition.RequiredEtiquette)
                return Refuse(intent, definition, "etiquette_mismatch", "礼仪顺序不合，社群暂缓交换。",
                    "recovery_learn_etiquette", -2, -2, D12CommunityTradeDisposition.Misunderstood);

            if (CommodityCatalog.Find(intent.GiveItemId) == null || CommodityCatalog.Find(intent.WantItemId) == null)
                return Invalid("unknown_item", "物物交换只接受已登记商品，不能由 Agent 生成物品。", intent.CommunityId);
            var inventory = _inventories[definition.Id];
            if (!inventory.Stock.TryGetValue(intent.WantItemId, out int available) || available < intent.WantQuantity)
                return Refuse(intent, definition, "community_out_of_stock", "社群当前没有足够的交换物资。",
                    "recovery_wait_for_restock", 0, 0, D12CommunityTradeDisposition.Refused);
            if (playerInventory == null || D3InventorySystem.CountCargo(playerInventory, intent.GiveItemId) < intent.GiveQuantity)
                return Refuse(intent, definition, "offer_missing", "玩家没有足够的交换物资。",
                    "recovery_return_with_offer", 0, 0, D12CommunityTradeDisposition.Refused);

            var simulated = playerInventory.Clone();
            string inventoryError;
            if (!D3InventorySystem.TryRemoveCargo(simulated, intent.GiveItemId, intent.GiveQuantity, out inventoryError) ||
                !D3InventorySystem.CanAddCargo(simulated, intent.WantItemId, intent.WantQuantity, out inventoryError))
                return Refuse(intent, definition, inventoryError == "weight_full" ? "player_weight_full" : "player_cargo_full",
                    "交换后玩家货舱无法承载物资。", "recovery_make_cargo_space", 0, 0, D12CommunityTradeDisposition.Refused);

            var fact = AppendTradeFact(intent, definition, "community_trade_completed", "社群完成了一次互惠物物交换。",
                WorldEventStatus.Normal, null, 5, 4, "completed");
            if (fact.IsDuplicate) return DuplicateResult(fact.Fact, intent, definition);
            if (!D3InventorySystem.TryRemoveCargo(playerInventory, intent.GiveItemId, intent.GiveQuantity, out inventoryError) ||
                !D3InventorySystem.TryAddCargo(playerInventory, intent.WantItemId, intent.WantQuantity, out inventoryError))
                throw new InvalidOperationException("交易预检与实际库存写入结果不一致：" + inventoryError);

            inventory.Stock[intent.WantItemId] = available - intent.WantQuantity;
            AddStock(inventory.Stock, intent.GiveItemId, intent.GiveQuantity);
            Remember(inventory, fact.Fact);
            ApplyReputation(intent, fact.Fact, 5, 4, "successful_exchange", "礼仪、语言和知识门槛均满足，社群完成互惠交换。", false, true);
            return ResultFromState(intent, definition, fact.Fact, true, false, "community_trade_completed",
                "交换完成：社群库存和玩家货物均由所属系统更新。", D12CommunityTradeDisposition.Completed, null);
        }

        public D12CommunityTradeResult RequestHelp(D12CommunityHelpRequest request)
        {
            if (request == null) return Invalid("invalid_request", "帮助请求不能为空。", null);
            if (string.IsNullOrWhiteSpace(request.RequestId) || string.IsNullOrWhiteSpace(request.CommunityId) ||
                string.IsNullOrWhiteSpace(request.ActorRef) || string.IsNullOrWhiteSpace(request.LocationRef))
                return Invalid("invalid_request", "帮助请求必须有请求、社群、行动者和位置。", request.CommunityId);
            if (!_definitions.TryGetValue(request.CommunityId, out D12CommunityDefinition definition))
                return Invalid("community_unknown", "未知社群没有默认帮助路线。", request.CommunityId);
            if (request.WorldDay < 0 || request.SaveRevision < 0)
                return Invalid("invalid_revision", "帮助请求日期或修订号非法。", request.CommunityId);
            string eventId = HelpEventId(request.RequestId);
            var duplicate = _eventLog.Find(eventId, request.SaveRevision);
            if (duplicate != null)
                return DuplicateResult(duplicate, new D12CommunityTradeIntent
                {
                    RequestId = request.RequestId, CommunityId = request.CommunityId, ActorRef = request.ActorRef,
                    LocationRef = request.LocationRef, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision,
                }, definition);
            var boundaryFailure = CheckBoundary(definition, request.LocationRef, null);
            if (boundaryFailure != null)
                return Refuse(new D12CommunityTradeIntent
                {
                    RequestId = request.RequestId, CommunityId = request.CommunityId, ActorRef = request.ActorRef,
                    LocationRef = request.LocationRef, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision,
                    CauseEventIds = request.CauseEventIds == null ? new List<string>() : new List<string>(request.CauseEventIds),
                }, definition, boundaryFailure.Code, boundaryFailure.Message, boundaryFailure.RouteId,
                    boundaryFailure.ReputationDelta, boundaryFailure.TrustDelta, D12CommunityTradeDisposition.Refused, true);

            var intent = new D12CommunityTradeIntent
            {
                RequestId = request.RequestId,
                CommunityId = request.CommunityId,
                ActorRef = request.ActorRef,
                LocationRef = request.LocationRef,
                LanguageId = request.LanguageId,
                LanguageLevel = request.LanguageLevel,
                KnowledgeLevel = request.KnowledgeLevel,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                CauseEventIds = request.CauseEventIds == null ? new List<string>() : new List<string>(request.CauseEventIds),
            };
            int knowledgeDelta = Math.Max(2, definition.RequiredKnowledgeLevel - Math.Max(0, request.KnowledgeLevel) + 1);
            var fact = AppendTradeFact(intent, definition, "community_help_offered", "社群提供了语言、礼仪或交换规约方面的帮助。",
                WorldEventStatus.Normal, null, 1, 2, "help_offered", true, knowledgeDelta);
            if (fact.IsDuplicate) return DuplicateResult(fact.Fact, intent, definition);
            ApplyReputation(intent, fact.Fact, 1, 2, "community_help_received", "社群提供帮助后，玩家获得了可追踪的文化知识和礼仪熟悉度。", true, false, knowledgeDelta);
            return ResultFromState(intent, definition, fact.Fact, true, false, "community_help_offered",
                "帮助已记录；它不会直接生成物品或交易结果。", D12CommunityTradeDisposition.HelpOffered, null);
        }

        private D12CommunityTradeResult Refuse(D12CommunityTradeIntent intent, D12CommunityDefinition definition,
            string code, string message, string routeId, int reputationDelta, int trustDelta,
            D12CommunityTradeDisposition disposition, bool helpRequest = false)
        {
            var fact = AppendTradeFact(intent, definition, code, message, WorldEventStatus.Failed,
                new RecoveryRoute
                {
                    RouteId = routeId,
                    Action = routeId,
                    ResumeFromEventId = helpRequest ? HelpEventId(intent.RequestId) : TradeEventId(intent.RequestId),
                    Description = "完成恢复路线后，可用新的请求修订号重新提交社群意图。",
                }, reputationDelta, trustDelta, disposition.ToString().ToLowerInvariant(), helpRequest);
            if (fact.IsDuplicate) return DuplicateResult(fact.Fact, intent, definition);
            ApplyReputation(intent, fact.Fact, reputationDelta, trustDelta, code, message,
                false, false, 0, disposition == D12CommunityTradeDisposition.Misunderstood);
            return ResultFromState(intent, definition, fact.Fact, false, false, code, message, disposition, routeId);
        }

        private EventAppendResult AppendTradeFact(D12CommunityTradeIntent intent, D12CommunityDefinition definition,
            string code, string message, WorldEventStatus status, RecoveryRoute recovery,
            int reputationDelta, int trustDelta, string outcome, bool helpRequest = false, int knowledgeDelta = 0)
        {
            var values = new Dictionary<string, string>
            {
                ["community_id"] = definition.Id,
                ["outcome"] = outcome,
                ["give_item_id"] = intent.GiveItemId ?? string.Empty,
                ["give_quantity"] = Math.Max(0, intent.GiveQuantity).ToString(CultureInfo.InvariantCulture),
                ["want_item_id"] = intent.WantItemId ?? string.Empty,
                ["want_quantity"] = Math.Max(0, intent.WantQuantity).ToString(CultureInfo.InvariantCulture),
                ["reputation_delta"] = reputationDelta.ToString(CultureInfo.InvariantCulture),
                ["trust_delta"] = trustDelta.ToString(CultureInfo.InvariantCulture),
                ["knowledge_delta"] = knowledgeDelta.ToString(CultureInfo.InvariantCulture),
                ["help_request"] = helpRequest ? "true" : "false",
                ["social_institution"] = definition.SocialInstitution,
                ["sensitive_content_label"] = definition.SensitiveContentLabel,
            };
            var fact = new WorldEvent(
                helpRequest ? HelpEventId(intent.RequestId) : TradeEventId(intent.RequestId),
                Math.Max(0, intent.WorldDay),
                intent.LocationRef,
                D12CommunityTradeIds.SystemOwner,
                new EventResult
                {
                    ResultCode = code,
                    Summary = message,
                    FactIds = new List<string> { "community:" + definition.Id, "community_outcome:" + code },
                    Values = values,
                },
                new[] { string.IsNullOrWhiteSpace(intent.ActorRef) ? "player" : intent.ActorRef },
                InputFacts(intent, definition),
                Causes(intent, definition),
                intent.SaveRevision,
                status,
                recovery);
            var append = _eventLog.Append(fact);
            if (!append.IsDuplicate && append.Fact != null)
            {
                _lastWorldDay = Math.Max(_lastWorldDay, append.Fact.WorldDay);
                _revision = Math.Max(_revision + 1, append.Fact.SaveRevision);
                _lastEventId = append.Fact.EventId;
            }
            return append;
        }

        private BoundaryFailure CheckBoundary(D12CommunityDefinition definition, string locationRef, string consentRef)
        {
            var protectedArea = _boundaries[definition.ProtectedAreaId];
            if (protectedArea.LocationRef == locationRef && (!protectedArea.TradeAllowed ||
                (protectedArea.RequiresConsent && string.IsNullOrWhiteSpace(consentRef))))
                return new BoundaryFailure("protected_area_refused", "该地点属于社群保护区，未获得明确同意前不能进行交换。",
                    "recovery_request_protected_area_consent", -3, -3);
            var landBoundary = _boundaries[definition.LandBoundaryId];
            if (landBoundary.LocationRef != locationRef || !landBoundary.TradeAllowed)
                return new BoundaryFailure("land_boundary_refused", "当前位置不在社群允许的土地边界/交换地点内。",
                    "recovery_move_to_trade_boundary", -2, -2);
            return null;
        }

        private void ApplyReputation(D12CommunityTradeIntent intent, FactEvent fact, int reputationDelta,
            int trustDelta, string reasonCode, string explanation, bool help, bool successfulTrade,
            int knowledgeDelta = 0, bool misunderstanding = false)
        {
            var state = GetOrCreateReputation(intent.ActorRef, intent.CommunityId);
            state.LongTermReputation = Clamp(state.LongTermReputation + reputationDelta, -100, 100);
            state.Trust = Clamp(state.Trust + trustDelta, 0, 100);
            state.CulturalKnowledge = Clamp(state.CulturalKnowledge + Math.Max(0, knowledgeDelta), 0, 100);
            if (help)
            {
                state.HelpReceivedCount++;
                state.EtiquetteFamiliarity = Clamp(state.EtiquetteFamiliarity + Math.Max(1, knowledgeDelta), 0, 100);
            }
            if (successfulTrade)
            {
                state.SuccessfulTradeCount++;
                state.EtiquetteFamiliarity = Clamp(state.EtiquetteFamiliarity + 1, 0, 100);
            }
            if (misunderstanding) state.MisunderstandingCount++;
            if (fact.OutcomeStatus == WorldEventStatus.Failed) state.RefusalCount++;
            state.Revision = Math.Max(state.Revision + 1, fact.SaveRevision);
            state.History.Add(new D12CommunityReputationChange
            {
                EventId = fact.EventId,
                CauseEventIds = fact.CauseIds == null ? new List<string>() : new List<string>(fact.CauseIds),
                WorldDay = fact.WorldDay,
                ReasonCode = reasonCode,
                ReputationDelta = reputationDelta,
                TrustDelta = trustDelta,
                ReputationAfter = state.LongTermReputation,
                TrustAfter = state.Trust,
                Explanation = explanation,
            });
            _reputations[D12CommunityTradeSnapshot.Key(intent.ActorRef, intent.CommunityId)] = state;
        }

        private D12CommunityReputationState GetOrCreateReputation(string actorRef, string communityId)
        {
            actorRef = string.IsNullOrWhiteSpace(actorRef) ? "player" : actorRef;
            string key = D12CommunityTradeSnapshot.Key(actorRef, communityId);
            if (!_reputations.TryGetValue(key, out D12CommunityReputationState state))
            {
                state = new D12CommunityReputationState { ActorRef = actorRef, CommunityId = communityId };
                _reputations.Add(key, state);
            }
            return state;
        }

        private D12CommunityTradeResult ResultFromState(D12CommunityTradeIntent intent, D12CommunityDefinition definition,
            FactEvent fact, bool succeeded, bool duplicate, string code, string message,
            D12CommunityTradeDisposition disposition, string routeId)
        {
            var reputation = GetReputation(intent.ActorRef, definition.Id);
            return new D12CommunityTradeResult
            {
                Succeeded = succeeded,
                Duplicate = duplicate,
                FactLogged = fact != null,
                Code = code,
                Message = message,
                Disposition = disposition,
                CommunityId = definition.Id,
                EventId = fact == null ? null : fact.EventId,
                GiveItemId = intent.GiveItemId,
                GiveQuantity = intent.GiveQuantity,
                WantItemId = intent.WantItemId,
                WantQuantity = intent.WantQuantity,
                LongTermReputationAfter = reputation.LongTermReputation,
                TrustAfter = reputation.Trust,
                CulturalKnowledgeAfter = reputation.CulturalKnowledge,
                EtiquetteFamiliarityAfter = reputation.EtiquetteFamiliarity,
                RecoveryRouteId = routeId,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private D12CommunityTradeResult DuplicateResult(FactEvent fact, D12CommunityTradeIntent intent,
            D12CommunityDefinition definition)
        {
            string code = fact == null || fact.Result == null ? "duplicate_settlement" : fact.Result.ResultCode;
            D12CommunityTradeDisposition disposition = code == "community_trade_completed"
                ? D12CommunityTradeDisposition.Completed
                : code == "community_help_offered" ? D12CommunityTradeDisposition.HelpOffered
                : code == "misunderstood_language" || code == "etiquette_mismatch"
                    ? D12CommunityTradeDisposition.Misunderstood : D12CommunityTradeDisposition.Refused;
            return ResultFromState(intent, definition, fact, code == "community_trade_completed" || code == "community_help_offered",
                true, "duplicate_settlement", "该社群事实已在日志中结算，未重复修改库存或声望。", disposition,
                fact == null ? null : RecoveryRouteId(fact));
        }

        private static string RecoveryRouteId(FactEvent fact)
        {
            return fact == null || fact.RecoveryRoute == null ? null : fact.RecoveryRoute.RouteId;
        }

        private static D12CommunityTradeResult Invalid(string code, string message, string communityId)
        {
            return new D12CommunityTradeResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                CommunityId = communityId,
                Disposition = D12CommunityTradeDisposition.Refused,
            };
        }

        private static IEnumerable<string> InputFacts(D12CommunityTradeIntent intent, D12CommunityDefinition definition)
        {
            return new[]
            {
                "community:" + definition.Id,
                "location:" + intent.LocationRef,
                "etiquette:" + intent.Etiquette,
                "language:" + (intent.LanguageId ?? "unknown"),
                "knowledge_level:" + intent.KnowledgeLevel.ToString(CultureInfo.InvariantCulture),
            };
        }

        private IEnumerable<string> Causes(D12CommunityTradeIntent intent, D12CommunityDefinition definition)
        {
            var result = new List<string>();
            if (intent.CauseEventIds != null)
                foreach (var cause in intent.CauseEventIds)
                    if (!string.IsNullOrWhiteSpace(cause) && !result.Contains(cause)) result.Add(cause);
            var inventory = _inventories[definition.Id];
            if (!string.IsNullOrWhiteSpace(inventory.LastEventId) && !result.Contains(inventory.LastEventId)) result.Add(inventory.LastEventId);
            if (!string.IsNullOrWhiteSpace(_lastEventId) && !result.Contains(_lastEventId)) result.Add(_lastEventId);
            return result;
        }

        private static void Remember(D12CommunityInventoryState state, FactEvent fact)
        {
            if (state == null || fact == null) return;
            if (!state.AppliedEventKeys.Contains(fact.DedupeKey)) state.AppliedEventKeys.Add(fact.DedupeKey);
            state.LastEventId = fact.EventId;
            state.Revision = Math.Max(state.Revision + 1, fact.SaveRevision);
        }

        private static void AddStock(Dictionary<string, int> stock, string itemId, int quantity)
        {
            stock.TryGetValue(itemId ?? string.Empty, out int current);
            stock[itemId ?? string.Empty] = checked(current + quantity);
        }

        private static Dictionary<string, int> CopyStock(Dictionary<string, int> source)
        {
            var result = new Dictionary<string, int>();
            if (source != null) foreach (var pair in source) result[pair.Key] = pair.Value;
            return result;
        }

        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }

        private static string TradeEventId(string requestId) { return D12CommunityTradeIds.EventPrefix + SafeId(requestId); }
        private static string HelpEventId(string requestId) { return D12CommunityTradeIds.HelpEventPrefix + SafeId(requestId); }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-') chars[i] = '_';
            return new string(chars);
        }

        private sealed class BoundaryFailure
        {
            public readonly string Code;
            public readonly string Message;
            public readonly string RouteId;
            public readonly int ReputationDelta;
            public readonly int TrustDelta;

            public BoundaryFailure(string code, string message, string routeId, int reputationDelta, int trustDelta)
            {
                Code = code; Message = message; RouteId = routeId; ReputationDelta = reputationDelta; TrustDelta = trustDelta;
            }
        }
    }
}
