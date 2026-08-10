// ============================================================
// D12DynamicQuestLogSystem —— D12-A08 动态任务与航海日志唯一写者
//
// 本系统只写 Quest state/目标投影/替代路线引用/RewardRequest。它不写
// 金钱、库存、关系、图鉴或其他领域最终数值。任务进度只能消费已经进入
// EventLog 的 FactEvent；日志和 Agent 摘要也只能从同一事实日志读取。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12DynamicQuestLogSystem
    {
        public const string OwnerSystem = "QuestSystem";
        public const string AcceptedEventPrefix = "event_d12_quest_accepted_";
        public const string AlternativeEventPrefix = "event_d12_quest_alternative_";
        public const string CompletedEventPrefix = "event_d12_quest_completed_";

        private readonly EventLog _eventLog;
        private readonly Dictionary<string, D12DynamicQuestDefinition> _definitions =
            new Dictionary<string, D12DynamicQuestDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12DynamicQuestState> _states =
            new Dictionary<string, D12DynamicQuestState>(StringComparer.Ordinal);
        private readonly List<D12QuestFactRef> _journalFacts = new List<D12QuestFactRef>();

        public D12DynamicQuestLogSystem(EventLog eventLog = null,
            IEnumerable<D12DynamicQuestDefinition> definitions = null,
            D12DynamicQuestLogSnapshot initialSnapshot = null)
        {
            _eventLog = eventLog ?? new EventLog();
            var sourceDefinitions = definitions ?? D12DynamicQuestCatalog.CreateVerticalSlice();
            foreach (var definition in sourceDefinitions)
            {
                if (definition == null) throw new ArgumentException("动态任务定义不能为 null。", "definitions");
                var copy = definition.Clone();
                copy.Validate();
                if (_definitions.ContainsKey(copy.QuestId))
                    throw new ArgumentException("动态任务 ID 重复：" + copy.QuestId, "definitions");
                _definitions.Add(copy.QuestId, copy);
            }

            if (initialSnapshot == null)
            {
                foreach (var definition in _definitions.Values)
                    _states.Add(definition.QuestId, NewState(definition));
            }
            else
            {
                Restore(initialSnapshot);
            }
        }

        public EventLog EventLog { get { return _eventLog; } }

        public IReadOnlyList<D12DynamicQuestDefinition> Definitions
        {
            get
            {
                var result = new List<D12DynamicQuestDefinition>();
                foreach (var definition in _definitions.Values) result.Add(definition.Clone());
                return result.AsReadOnly();
            }
        }

        public D12DynamicQuestLogSnapshot Snapshot
        {
            get
            {
                var result = new D12DynamicQuestLogSnapshot();
                foreach (var state in _states.Values) result.Quests.Add(state.Clone());
                foreach (var fact in _journalFacts) result.JournalFacts.Add(fact.Clone());
                return result;
            }
        }

        /// <summary>只读任务状态；调用方不能获得内部写引用。</summary>
        public D12DynamicQuestState ReadQuest(string questId)
        {
            D12DynamicQuestState state;
            return _states.TryGetValue(questId ?? string.Empty, out state) ? state.Clone() : null;
        }

        /// <summary>航海日志是 EventLog 事实的只读投影，缺失事实不会被文本补造。</summary>
        public IReadOnlyList<D12QuestJournalEntry> ReadJournal(int maxEntries = 100)
        {
            var result = new List<D12QuestJournalEntry>();
            if (maxEntries <= 0) return result.AsReadOnly();
            int start = Math.Max(0, _journalFacts.Count - maxEntries);
            for (int i = start; i < _journalFacts.Count; i++)
            {
                var reference = _journalFacts[i];
                var fact = _eventLog.Find(reference.EventId, reference.SaveRevision);
                if (fact == null)
                    throw new InvalidOperationException("航海日志引用的事实不在 EventLog 中：" + reference.EventId);
                result.Add(ToJournalEntry(fact));
            }
            return result.AsReadOnly();
        }

        /// <summary>Agent 只能读取 EventLog 已落盘摘要，不能由摘要创建事实。</summary>
        public D12QuestAgentSummary BuildAgentSummary(int maxEntries = 5)
        {
            var facts = _eventLog.ReadFactsForUi(maxEntries);
            var result = new D12QuestAgentSummary
            {
                SummaryText = _eventLog.BuildAgentSummary(maxEntries),
                FactsOnly = true,
                CanCreateFacts = false,
            };
            foreach (var fact in facts)
                result.FactRefs.Add(new D12QuestFactRef(fact.EventId, fact.SaveRevision));
            return result;
        }

        public D12QuestResult Accept(D12QuestAcceptRequest request)
        {
            if (request == null) return Reject(null, "request_required", "任务接受请求不能为空。");
            if (!ValidActorAndTime(request.ActorRef, request.WorldDay, request.SaveRevision))
                return Reject(request.QuestId, "invalid_request", "任务接受请求缺少执行者或时间版本非法。");

            D12DynamicQuestState state;
            if (!_states.TryGetValue(request.QuestId ?? string.Empty, out state))
                return Reject(request.QuestId, "quest_unknown", "动态任务不存在。");
            if (state.Status == D12DynamicQuestStatus.Active || state.Status == D12DynamicQuestStatus.Recovered)
                return Duplicate(state, "quest_already_active", "任务已经接受，未重复写入。", null);
            if (state.Status == D12DynamicQuestStatus.Completed)
                return Reject(state.QuestId, "quest_completed", "任务已经完成。");
            if (state.Status == D12DynamicQuestStatus.Abandoned)
                return Reject(state.QuestId, "quest_abandoned", "任务已经放弃。");

            string eventId = AcceptedEventPrefix + SafeId(state.QuestId);
            var fact = AppendQuestFact(eventId, request.WorldDay, request.SaveRevision, request.ActorRef,
                "quest_accepted", "任务已接受；目标、事实和替代路线进入航海日志。",
                new Dictionary<string, string>
                {
                    ["quest_id"] = state.QuestId,
                    ["quest_kind"] = _definitions[state.QuestId].Kind.ToString(),
                }, null, new[] { "quest:" + state.QuestId });
            RegisterFact(state, fact);
            state.Status = D12DynamicQuestStatus.Active;
            state.AcceptedWorldDay = request.WorldDay;
            state.LastUpdatedWorldDay = request.WorldDay;
            return Applied(state, "quest_accepted", "任务已接受。", fact, null, null);
        }

        /// <summary>
        /// 只接受已存在的 FactEvent。任务不会根据 UI/Agent 文本或自带数值推进目标。
        /// </summary>
        public D12QuestResult ObserveFact(D12QuestFactObservationRequest request)
        {
            if (request == null) return Reject(null, "request_required", "任务事实观察请求不能为空。");
            if (string.IsNullOrWhiteSpace(request.ActorRef))
                return Reject(request.QuestId, "actor_required", "任务事实观察必须有执行者。");

            D12DynamicQuestState state;
            D12DynamicQuestDefinition definition;
            if (!_states.TryGetValue(request.QuestId ?? string.Empty, out state) ||
                !_definitions.TryGetValue(request.QuestId ?? string.Empty, out definition))
                return Reject(request.QuestId, "quest_unknown", "动态任务不存在。");

            var fact = _eventLog.Find(request.FactEventId, request.FactSaveRevision);
            if (fact == null) return Reject(state.QuestId, "fact_missing", "任务只能消费 EventLog 中已有事实。");
            var factRef = new D12QuestFactRef(fact.EventId, fact.SaveRevision);
            if (ContainsFact(state.FactEvents, factRef))
                return Duplicate(state, "duplicate_fact_observation", "该任务事实已经观察过，未重复推进。", fact);
            if (state.Status == D12DynamicQuestStatus.Available)
                return Reject(state.QuestId, "quest_not_active", "必须先接受任务才能观察目标事实。");
            if (state.Status == D12DynamicQuestStatus.Completed)
                return Reject(state.QuestId, "quest_completed", "任务已经完成，不能继续推进。");
            if (state.Status == D12DynamicQuestStatus.Abandoned)
                return Reject(state.QuestId, "quest_abandoned", "任务已经放弃。");

            if (IsFailure(definition, fact))
            {
                RegisterFact(state, fact);
                state.Status = D12DynamicQuestStatus.Failed;
                state.LastFailureCode = fact.Result == null ? "failed" : fact.Result.ResultCode;
                state.LastFailureFact = factRef.Clone();
                state.RecoveryRouteId = definition.DefaultRecoveryRouteId;
                state.LastUpdatedWorldDay = fact.WorldDay;
                return Applied(state, "quest_failed", "任务记录到失败事实；可选择恢复或替代路线。", fact,
                    null, null, FindRoute(definition, state.RecoveryRouteId));
            }

            if (IsRecovery(definition, fact))
            {
                RegisterFact(state, fact);
                state.Status = D12DynamicQuestStatus.Recovered;
                state.LastRecoveryFact = factRef.Clone();
                state.RecoveryRouteId = null;
                state.LastFailureCode = string.Empty;
                state.LastUpdatedWorldDay = fact.WorldDay;
                return Applied(state, "quest_recovered", "任务恢复事实已记录，可以继续目标路线。", fact, null, null);
            }

            var matched = new List<string>();
            foreach (var objectiveDefinition in definition.Objectives)
            {
                var objectiveState = FindObjectiveState(state, objectiveDefinition.ObjectiveId);
                if (objectiveState == null || objectiveState.Progress >= objectiveDefinition.RequiredCount) continue;
                if (!MatchesObjective(objectiveDefinition, state, fact)) continue;
                if (ContainsFact(objectiveState.AppliedFacts, factRef)) continue;
                objectiveState.Progress++;
                objectiveState.AppliedFacts.Add(factRef.Clone());
                matched.Add(objectiveDefinition.ObjectiveId);
            }

            if (matched.Count == 0)
                return Reject(state.QuestId, "fact_not_for_task", "事实已存在，但不满足当前任务目标。");

            RegisterFact(state, fact);
            state.LastUpdatedWorldDay = fact.WorldDay;
            if (!AllObjectivesComplete(definition, state))
                return Applied(state, "objective_progressed", "任务目标已从统一事实日志推进。", fact, null, matched);

            var completedFact = AppendQuestFact(
                CompletedEventPrefix + SafeId(state.QuestId), fact.WorldDay, fact.SaveRevision, request.ActorRef,
                "quest_completed", "任务目标已全部由事实日志满足；奖励请求等待 RewardSystem 裁决。",
                new Dictionary<string, string>
                {
                    ["quest_id"] = state.QuestId,
                    ["status"] = "completed",
                    ["reward_request_id"] = _definitions[state.QuestId].RewardId,
                },
                BuildCauseIds(state), new[] { "quest:" + state.QuestId, "reward_request:" + _definitions[state.QuestId].RewardId });
            RegisterFact(state, completedFact);
            state.CompletedFact = new D12QuestFactRef(completedFact.EventId, completedFact.SaveRevision);
            state.Status = D12DynamicQuestStatus.Completed;
            state.RewardRequest = new D12QuestRewardRequest
            {
                RequestId = _definitions[state.QuestId].RewardId + "@" + completedFact.SaveRevision.ToString(CultureInfo.InvariantCulture),
                RewardId = _definitions[state.QuestId].RewardId,
                QuestId = state.QuestId,
                SourceEventId = completedFact.EventId,
                SourceSaveRevision = completedFact.SaveRevision,
                RewardType = _definitions[state.QuestId].RewardType,
                OwnerSystem = "RewardSystem",
                Status = "pending",
                CauseEventIds = BuildCauseIds(state),
            };
            return Applied(state, "quest_completed", "任务完成；RewardRequest 已提交但尚未发放。", completedFact,
                state.RewardRequest, matched);
        }

        public D12QuestResult SelectAlternativeRoute(D12QuestAlternativeRouteRequest request)
        {
            if (request == null) return Reject(null, "request_required", "替代路线请求不能为空。");
            if (!ValidActorAndTime(request.ActorRef, request.WorldDay, request.SaveRevision))
                return Reject(request.QuestId, "invalid_request", "替代路线请求缺少执行者或时间版本非法。");

            D12DynamicQuestState state;
            D12DynamicQuestDefinition definition;
            if (!_states.TryGetValue(request.QuestId ?? string.Empty, out state) ||
                !_definitions.TryGetValue(request.QuestId ?? string.Empty, out definition))
                return Reject(request.QuestId, "quest_unknown", "动态任务不存在。");
            if (state.Status != D12DynamicQuestStatus.Failed)
                return Reject(state.QuestId, "alternative_route_unavailable", "只有失败任务可以选择替代路线。");
            var route = FindRoute(definition, request.RouteId);
            if (route == null) return Reject(state.QuestId, "alternative_route_unknown", "替代路线不存在。");

            string eventId = AlternativeEventPrefix + SafeId(state.QuestId) + "_" + SafeId(route.RouteId);
            var fact = AppendQuestFact(eventId, request.WorldDay, request.SaveRevision, request.ActorRef,
                "quest_alternative_route_selected", route.Description,
                new Dictionary<string, string>
                {
                    ["quest_id"] = state.QuestId,
                    ["route_id"] = route.RouteId,
                    ["resume_objective_id"] = route.ResumeObjectiveId,
                    ["resume_from_event_id"] = state.LastFailureFact == null ? string.Empty : state.LastFailureFact.EventId,
                },
                state.LastFailureFact == null ? new string[0] : new[] { state.LastFailureFact.EventId },
                new[] { "quest:" + state.QuestId, "route:" + route.RouteId });
            if (ContainsFact(state.FactEvents, new D12QuestFactRef(fact.EventId, fact.SaveRevision)))
                return Duplicate(state, "duplicate_alternative_route", "替代路线已经选择，未重复修改任务状态。", fact);

            RegisterFact(state, fact);
            state.Status = D12DynamicQuestStatus.Recovered;
            state.RecoveryRouteId = null;
            state.AlternativeRouteId = route.RouteId;
            state.LastRecoveryFact = new D12QuestFactRef(fact.EventId, fact.SaveRevision);
            state.LastFailureCode = string.Empty;
            state.LastUpdatedWorldDay = request.WorldDay;
            return Applied(state, "alternative_route_selected", route.Description, fact, null, null, route);
        }

        public void Restore(D12DynamicQuestLogSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            if (snapshot.Quests == null) throw new InvalidOperationException("动态任务快照缺少任务列表。");

            var restoredStates = new Dictionary<string, D12DynamicQuestState>(StringComparer.Ordinal);
            foreach (var definition in _definitions.Values)
            {
                D12DynamicQuestState found = null;
                foreach (var state in snapshot.Quests)
                    if (state != null && state.QuestId == definition.QuestId) found = state;
                if (found == null) throw new InvalidOperationException("动态任务快照缺少定义：" + definition.QuestId);
                ValidateStateAgainstDefinition(found, definition);
                restoredStates.Add(definition.QuestId, found.Clone());
            }
            if (snapshot.Quests.Count != _definitions.Count)
                throw new InvalidOperationException("动态任务快照包含未知任务状态。");
            var restoredJournal = new List<D12QuestFactRef>();
            foreach (var fact in snapshot.JournalFacts)
            {
                if (_eventLog.Find(fact.EventId, fact.SaveRevision) == null)
                    throw new InvalidOperationException("动态任务快照引用了不存在的日志事实：" + fact.EventId);
                if (!ContainsFact(restoredJournal, fact)) restoredJournal.Add(fact.Clone());
            }
            _states.Clear();
            foreach (var pair in restoredStates) _states.Add(pair.Key, pair.Value);
            _journalFacts.Clear();
            _journalFacts.AddRange(restoredJournal);
        }

        private static D12DynamicQuestState NewState(D12DynamicQuestDefinition definition)
        {
            var result = new D12DynamicQuestState { QuestId = definition.QuestId, LastUpdatedWorldDay = 0 };
            foreach (var objective in definition.Objectives)
                result.Objectives.Add(new D12QuestObjectiveState { ObjectiveId = objective.ObjectiveId });
            return result;
        }

        private void ValidateStateAgainstDefinition(D12DynamicQuestState state, D12DynamicQuestDefinition definition)
        {
            var expected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var objective in definition.Objectives) expected.Add(objective.ObjectiveId);
            var actual = new HashSet<string>(StringComparer.Ordinal);
            foreach (var objective in state.Objectives)
            {
                if (!expected.Contains(objective.ObjectiveId)) throw new InvalidOperationException("任务快照包含未知目标：" + objective.ObjectiveId);
                if (!actual.Add(objective.ObjectiveId)) throw new InvalidOperationException("任务快照目标重复：" + objective.ObjectiveId);
                var definitionObjective = FindObjective(definition, objective.ObjectiveId);
                if (objective.Progress > definitionObjective.RequiredCount)
                    throw new InvalidOperationException("任务目标进度超过定义：" + objective.ObjectiveId);
            }
            if (actual.Count != expected.Count) throw new InvalidOperationException("任务快照目标不完整：" + state.QuestId);
            if (!string.IsNullOrWhiteSpace(state.AlternativeRouteId) && FindRoute(definition, state.AlternativeRouteId) == null)
                throw new InvalidOperationException("任务快照替代路线未知：" + state.AlternativeRouteId);
            if (!string.IsNullOrWhiteSpace(state.RecoveryRouteId) && FindRoute(definition, state.RecoveryRouteId) == null)
                throw new InvalidOperationException("任务快照恢复路线未知：" + state.RecoveryRouteId);
        }

        private bool IsFailure(D12DynamicQuestDefinition definition, FactEvent fact)
        {
            return fact.Result != null && definition.FailureResultCodes.Contains(fact.Result.ResultCode);
        }

        private bool IsRecovery(D12DynamicQuestDefinition definition, FactEvent fact)
        {
            if (fact.OutcomeStatus == WorldEventStatus.Recovered) return true;
            return fact.Result != null && definition.RecoveryResultCodes.Contains(fact.Result.ResultCode);
        }

        private static bool MatchesObjective(D12QuestObjectiveDefinition definition,
            D12DynamicQuestState state, FactEvent fact)
        {
            if (fact == null || fact.Result == null) return false;
            if (definition.AcceptedResultCodes.Contains(fact.Result.ResultCode) &&
                MatchesContext(definition.TargetOwnerSystem, definition.MatchValueKey, definition.MatchValue, fact)) return true;
            if (string.IsNullOrWhiteSpace(state.AlternativeRouteId) || definition.AlternativeAcceptances == null) return false;
            foreach (var alternative in definition.AlternativeAcceptances)
                if (alternative != null && alternative.RouteId == state.AlternativeRouteId &&
                    alternative.AcceptedResultCodes.Contains(fact.Result.ResultCode) &&
                    MatchesContext(
                        string.IsNullOrWhiteSpace(alternative.TargetOwnerSystem) ? definition.TargetOwnerSystem : alternative.TargetOwnerSystem,
                        string.IsNullOrWhiteSpace(alternative.MatchValueKey) ? definition.MatchValueKey : alternative.MatchValueKey,
                        string.IsNullOrWhiteSpace(alternative.MatchValue) ? definition.MatchValue : alternative.MatchValue,
                        fact)) return true;
            return false;
        }

        private static bool MatchesContext(string ownerSystem, string matchValueKey, string matchValue, FactEvent fact)
        {
            if (!string.IsNullOrWhiteSpace(ownerSystem) && fact.OwnerSystem != ownerSystem) return false;
            if (!string.IsNullOrWhiteSpace(matchValueKey))
            {
                string actual;
                if (fact.Result.Values == null || !fact.Result.Values.TryGetValue(matchValueKey, out actual) || actual != matchValue)
                    return false;
            }
            return true;
        }

        private static bool AllObjectivesComplete(D12DynamicQuestDefinition definition, D12DynamicQuestState state)
        {
            foreach (var objective in definition.Objectives)
            {
                var current = FindObjectiveState(state, objective.ObjectiveId);
                if (current == null || current.Progress < objective.RequiredCount) return false;
            }
            return true;
        }

        private static D12QuestObjectiveDefinition FindObjective(D12DynamicQuestDefinition definition, string objectiveId)
        {
            foreach (var objective in definition.Objectives)
                if (objective.ObjectiveId == objectiveId) return objective;
            return null;
        }

        private static D12QuestObjectiveState FindObjectiveState(D12DynamicQuestState state, string objectiveId)
        {
            foreach (var objective in state.Objectives)
                if (objective.ObjectiveId == objectiveId) return objective;
            return null;
        }

        private static D12QuestAlternativeRouteDefinition FindRoute(D12DynamicQuestDefinition definition, string routeId)
        {
            if (definition == null || definition.AlternativeRoutes == null) return null;
            foreach (var route in definition.AlternativeRoutes)
                if (route != null && route.RouteId == routeId) return route.Clone();
            return null;
        }

        private FactEvent AppendQuestFact(string eventId, int worldDay, long saveRevision, string actorRef,
            string resultCode, string summary, Dictionary<string, string> values,
            IEnumerable<string> causeIds, IEnumerable<string> factIds)
        {
            var result = _eventLog.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = Math.Max(0, worldDay),
                LocationRef = "quest_log",
                OwnerSystem = OwnerSystem,
                ActorIds = new List<string> { actorRef },
                InputFacts = new List<string> { "quest_event:" + eventId },
                CauseIds = causeIds == null ? new List<string>() : new List<string>(causeIds),
                SaveRevision = Math.Max(0L, saveRevision),
                Result = new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = factIds == null ? new List<string> { "quest_fact:" + eventId } : new List<string>(factIds),
                    Values = values ?? new Dictionary<string, string>(),
                },
            });
            return result.Fact;
        }

        private void RegisterFact(D12DynamicQuestState state, FactEvent fact)
        {
            if (fact == null) throw new InvalidOperationException("不能把空事实写入任务状态。");
            var reference = new D12QuestFactRef(fact.EventId, fact.SaveRevision);
            if (!ContainsFact(state.FactEvents, reference)) state.FactEvents.Add(reference.Clone());
            if (!ContainsFact(_journalFacts, reference)) _journalFacts.Add(reference.Clone());
        }

        private static List<string> BuildCauseIds(D12DynamicQuestState state)
        {
            var result = new List<string>();
            foreach (var reference in state.FactEvents)
                if (reference != null && !result.Contains(reference.EventId)) result.Add(reference.EventId);
            return result;
        }

        private static bool ContainsFact(IEnumerable<D12QuestFactRef> values, D12QuestFactRef target)
        {
            if (values == null || target == null) return false;
            foreach (var value in values)
                if (value != null && value.Key == target.Key) return true;
            return false;
        }

        private static D12QuestJournalEntry ToJournalEntry(FactEvent fact)
        {
            return new D12QuestJournalEntry
            {
                EventId = fact.EventId,
                SaveRevision = fact.SaveRevision,
                WorldDay = fact.WorldDay,
                LocationRef = fact.LocationRef,
                OwnerSystem = fact.OwnerSystem,
                OutcomeStatus = fact.OutcomeStatus,
                ResultCode = fact.Result == null ? string.Empty : fact.Result.ResultCode,
                Summary = fact.Result == null ? string.Empty : fact.Result.Summary,
                CauseEventIds = fact.CauseIds == null ? new List<string>() : new List<string>(fact.CauseIds),
            };
        }

        private static bool ValidActorAndTime(string actorRef, int worldDay, long saveRevision)
        {
            return !string.IsNullOrWhiteSpace(actorRef) && worldDay >= 0 && saveRevision >= 0;
        }

        private static string SafeId(string value)
        {
            return "q" + StableToken(value ?? string.Empty);
        }

        private static string StableToken(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (var character in value)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }
                return hash.ToString("x8", CultureInfo.InvariantCulture);
            }
        }

        private static D12QuestResult Applied(D12DynamicQuestState state, string code, string message,
            FactEvent fact, D12QuestRewardRequest reward, List<string> matched,
            D12QuestAlternativeRouteDefinition route = null)
        {
            var result = new D12QuestResult
            {
                Accepted = true,
                Applied = true,
                Code = code,
                Message = message,
                QuestId = state.QuestId,
                Status = state.Status,
                Fact = fact == null ? null : fact.CloneFact(),
                RewardRequest = reward == null ? null : reward.Clone(),
                AlternativeRoute = route == null ? null : route.Clone(),
            };
            if (matched != null) result.MatchedObjectiveIds.AddRange(matched);
            return result;
        }

        private static D12QuestResult Duplicate(D12DynamicQuestState state, string code, string message, FactEvent fact)
        {
            return new D12QuestResult
            {
                Accepted = true,
                Duplicate = true,
                Code = code,
                Message = message,
                QuestId = state.QuestId,
                Status = state.Status,
                Fact = fact == null ? null : fact.CloneFact(),
                RewardRequest = state.RewardRequest == null ? null : state.RewardRequest.Clone(),
            };
        }

        private static D12QuestResult Reject(string questId, string code, string message)
        {
            return new D12QuestResult
            {
                Accepted = false,
                Applied = false,
                Code = code,
                Message = message,
                QuestId = questId,
                Status = D12DynamicQuestStatus.Available,
            };
        }
    }
}
