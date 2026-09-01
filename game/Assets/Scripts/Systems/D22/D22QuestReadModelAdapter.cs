// ============================================================
// D22QuestReadModelAdapter —— D22-L22 任务只读 UI 适配器（纯 C#）
//
// D12 只拥有任务状态/目标/事实引用/RewardRequest，D14 只拥有奖励
// 状态。本适配器只投影这些公开结果，保留失败码、奖励状态和来源 key；
// 不把 Message/Summary 当作 UI 文案，也不写入任务、事实或奖励 owner。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D12;
using Whalebone.Systems.D14;

namespace Whalebone.Systems.D22
{
    /// <summary>D12/D14 任务 owner 的输入边界。</summary>
    [Serializable]
    public sealed class D22QuestOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public int WorldDay;
        public long SaveRevision;
        public D12DynamicQuestLogSnapshot QuestLog;
        public List<D12DynamicQuestDefinition> Definitions = new List<D12DynamicQuestDefinition>();
        public List<D12QuestResult> Results = new List<D12QuestResult>();
        public D14RewardRuntimeSnapshot RewardSnapshot;
        public D14RewardResult LastRewardResult;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>任务目标只读投影。</summary>
    [Serializable]
    public sealed class D22QuestObjectiveReadModel
    {
        public string ObjectiveId { get; }
        public int Progress { get; }
        public int RequiredCount { get; }
        public string TargetOwnerSystem { get; }
        public string MatchValueKey { get; }
        public string MatchValue { get; }
        public IReadOnlyList<string> AppliedFactIds { get; }
        public string LocalizationKey { get; }

        internal D22QuestObjectiveReadModel(D12QuestObjectiveDefinition definition, D12QuestObjectiveState state)
        {
            ObjectiveId = state == null ? (definition == null ? string.Empty : definition.ObjectiveId ?? string.Empty) : state.ObjectiveId ?? string.Empty;
            Progress = state == null ? 0 : state.Progress;
            RequiredCount = definition == null ? 0 : definition.RequiredCount;
            TargetOwnerSystem = definition == null ? string.Empty : definition.TargetOwnerSystem ?? string.Empty;
            MatchValueKey = definition == null ? string.Empty : definition.MatchValueKey ?? string.Empty;
            MatchValue = definition == null ? string.Empty : definition.MatchValue ?? string.Empty;
            var factIds = new List<string>();
            if (state != null && state.AppliedFacts != null)
            {
                foreach (var fact in state.AppliedFacts)
                    if (fact != null && !string.IsNullOrEmpty(fact.EventId)) factIds.Add(fact.EventId);
            }
            factIds = D22ReadModelAdapterSupport.SortedDistinct(factIds);
            AppliedFactIds = new ReadOnlyCollection<string>(factIds);
            LocalizationKey = D22ReadModelAdapterSupport.Key("quest", "objective");
        }

        private D22QuestObjectiveReadModel(
            string objectiveId,
            int progress,
            int requiredCount,
            string targetOwnerSystem,
            string matchValueKey,
            string matchValue,
            IEnumerable<string> appliedFactIds,
            string localizationKey)
        {
            ObjectiveId = objectiveId ?? string.Empty;
            Progress = progress;
            RequiredCount = requiredCount;
            TargetOwnerSystem = targetOwnerSystem ?? string.Empty;
            MatchValueKey = matchValueKey ?? string.Empty;
            MatchValue = matchValue ?? string.Empty;
            AppliedFactIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(appliedFactIds));
            LocalizationKey = localizationKey ?? string.Empty;
        }

        public D22QuestObjectiveReadModel Clone()
        {
            return new D22QuestObjectiveReadModel(
                ObjectiveId, Progress, RequiredCount, TargetOwnerSystem, MatchValueKey, MatchValue,
                AppliedFactIds, LocalizationKey);
        }
    }

    /// <summary>任务奖励状态只读投影。</summary>
    [Serializable]
    public sealed class D22QuestRewardReadModel
    {
        public bool HasProjection { get; }
        public string RewardId { get; }
        public string RewardType { get; }
        public string Status { get; }
        public string GrantId { get; }
        public string RecipientRef { get; }
        public string SourceEventId { get; }
        public string RecoveryRoute { get; }
        public string LossReason { get; }
        public string SourceErrorCode { get; }
        public string LocalizationKey { get; }

        internal D22QuestRewardReadModel(D14RewardGrantRecord source, D12QuestRewardRequest request)
        {
            HasProjection = source != null || request != null;
            RewardId = source == null
                ? (request == null ? string.Empty : request.RewardId ?? string.Empty)
                : source.RewardId ?? string.Empty;
            RewardType = request == null ? string.Empty : request.RewardType.ToString();
            Status = source == null
                ? (request == null ? string.Empty : request.Status ?? string.Empty)
                : source.Status.ToString();
            GrantId = source == null ? string.Empty : source.GrantId ?? string.Empty;
            RecipientRef = source == null ? string.Empty : source.RecipientRef ?? string.Empty;
            SourceEventId = source == null
                ? (request == null ? string.Empty : request.SourceEventId ?? string.Empty)
                : source.SourceEventId ?? string.Empty;
            RecoveryRoute = source == null ? string.Empty : source.RecoveryRoute ?? string.Empty;
            LossReason = source == null ? string.Empty : source.LossReason.ToString();
            SourceErrorCode = string.Empty;
            LocalizationKey = D22ReadModelAdapterSupport.Key("quest", "reward");
        }

        private D22QuestRewardReadModel(
            bool hasProjection,
            string rewardId,
            string rewardType,
            string status,
            string grantId,
            string recipientRef,
            string sourceEventId,
            string recoveryRoute,
            string lossReason,
            string sourceErrorCode,
            string localizationKey)
        {
            HasProjection = hasProjection;
            RewardId = rewardId ?? string.Empty;
            RewardType = rewardType ?? string.Empty;
            Status = status ?? string.Empty;
            GrantId = grantId ?? string.Empty;
            RecipientRef = recipientRef ?? string.Empty;
            SourceEventId = sourceEventId ?? string.Empty;
            RecoveryRoute = recoveryRoute ?? string.Empty;
            LossReason = lossReason ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
        }

        public D22QuestRewardReadModel Clone()
        {
            return new D22QuestRewardReadModel(
                HasProjection, RewardId, RewardType, Status, GrantId, RecipientRef,
                SourceEventId, RecoveryRoute, LossReason, SourceErrorCode, LocalizationKey);
        }
    }

    /// <summary>任务单项只读投影。</summary>
    [Serializable]
    public sealed class D22QuestItemReadModel
    {
        public string QuestId { get; }
        public string Status { get; }
        public string Kind { get; }
        public string RewardId { get; }
        public string RewardType { get; }
        public int AcceptedWorldDay { get; }
        public int LastUpdatedWorldDay { get; }
        public string LastFailureCode { get; }
        public string RecoveryRouteId { get; }
        public string AlternativeRouteId { get; }
        public bool HasCompletedFact { get; }
        public string TitleLocalizationKey { get; }
        public string PromptLocalizationKey { get; }
        public string SourceTitleLocalizationKey { get; }
        public string FailureLocalizationKey { get; }
        public IReadOnlyList<D22QuestObjectiveReadModel> Objectives { get; }
        public D22QuestRewardReadModel Reward { get; }
        public D22AdapterOutcomeReadModel LastResult { get; }

        private D22QuestItemReadModel(
            string questId,
            string status,
            string kind,
            string rewardId,
            string rewardType,
            int acceptedWorldDay,
            int lastUpdatedWorldDay,
            string lastFailureCode,
            string recoveryRouteId,
            string alternativeRouteId,
            bool hasCompletedFact,
            string titleLocalizationKey,
            string promptLocalizationKey,
            string sourceTitleLocalizationKey,
            string failureLocalizationKey,
            IEnumerable<D22QuestObjectiveReadModel> objectives,
            D22QuestRewardReadModel reward,
            D22AdapterOutcomeReadModel lastResult)
        {
            QuestId = questId ?? string.Empty;
            Status = status ?? string.Empty;
            Kind = kind ?? string.Empty;
            RewardId = rewardId ?? string.Empty;
            RewardType = rewardType ?? string.Empty;
            AcceptedWorldDay = acceptedWorldDay;
            LastUpdatedWorldDay = lastUpdatedWorldDay;
            LastFailureCode = lastFailureCode ?? string.Empty;
            RecoveryRouteId = recoveryRouteId ?? string.Empty;
            AlternativeRouteId = alternativeRouteId ?? string.Empty;
            HasCompletedFact = hasCompletedFact;
            TitleLocalizationKey = titleLocalizationKey ?? string.Empty;
            PromptLocalizationKey = promptLocalizationKey ?? string.Empty;
            SourceTitleLocalizationKey = sourceTitleLocalizationKey ?? string.Empty;
            FailureLocalizationKey = failureLocalizationKey ?? string.Empty;
            var copied = new List<D22QuestObjectiveReadModel>();
            if (objectives != null)
                foreach (var objective in objectives)
                    if (objective != null) copied.Add(objective.Clone());
            copied.Sort(CompareObjectives);
            Objectives = new ReadOnlyCollection<D22QuestObjectiveReadModel>(copied);
            Reward = reward == null ? null : reward.Clone();
            LastResult = lastResult == null ? null : lastResult.Clone();
        }

        internal D22QuestItemReadModel(
            D12DynamicQuestState state,
            D12DynamicQuestDefinition definition,
            D12QuestResult result,
            D14RewardGrantRecord grant,
            List<D22ReadModelAdapterIssue> issues)
            : this(
                state == null ? string.Empty : state.QuestId,
                state == null ? string.Empty : state.Status.ToString(),
                definition == null ? string.Empty : definition.Kind.ToString(),
                definition == null ? (state == null || state.RewardRequest == null ? string.Empty : state.RewardRequest.RewardId) : definition.RewardId,
                definition == null ? (state == null || state.RewardRequest == null ? string.Empty : state.RewardRequest.RewardType.ToString()) : definition.RewardType.ToString(),
                state == null ? -1 : state.AcceptedWorldDay,
                state == null ? 0 : state.LastUpdatedWorldDay,
                state == null ? string.Empty : state.LastFailureCode,
                state == null ? string.Empty : state.RecoveryRouteId,
                state == null ? string.Empty : state.AlternativeRouteId,
                state != null && state.CompletedFact != null,
                D22ReadModelAdapterSupport.Key("quest", "title"),
                D22ReadModelAdapterSupport.Key("quest", "prompt"),
                definition == null ? string.Empty : definition.DisplayNameKey,
                state == null || string.IsNullOrEmpty(state.LastFailureCode)
                    ? string.Empty : D22ReadModelAdapterSupport.Key("quest", "failure"),
                BuildObjectives(state, definition, issues),
                BuildReward(state, grant),
                BuildResult(result, issues))
        {
            if (definition == null)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_quest_definition", "d22.quest.definition_missing", D22ReadModelAdapterSupport.Key("quest", "partial"),
                    "d22.quest.definition_missing", "D12", "d12.quest.definition", false));
            }
            else
            {
                D22ReadModelAdapterSupport.ValidateSourceKey(definition.DisplayNameKey, "D12", "d12.quest.definition", issues);
                ValidateId(definition.RewardId, "reward_id_invalid", "d12.quest.definition", issues, true);
            }
            if (state != null)
            {
                ValidateId(state.QuestId, "quest_id_invalid", "d12.quest", issues);
                if (!Enum.IsDefined(typeof(D12DynamicQuestStatus), state.Status))
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_quest_status", "d22.quest.status_invalid", D22ReadModelAdapterSupport.Key("quest", "invalid"),
                        "d22.quest.status_invalid", "D12", "d12.quest", false));
                D22ReadModelAdapterSupport.ValidateSourceCode(state.LastFailureCode, "D12", "d12.quest", issues);
            }
            if (result != null)
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(result.Code, "D12", "d12.quest.result", issues);
            }
        }

        public D22QuestItemReadModel Clone()
        {
            return new D22QuestItemReadModel(
                QuestId, Status, Kind, RewardId, RewardType, AcceptedWorldDay, LastUpdatedWorldDay,
                LastFailureCode, RecoveryRouteId, AlternativeRouteId, HasCompletedFact,
                TitleLocalizationKey, PromptLocalizationKey, SourceTitleLocalizationKey,
                FailureLocalizationKey, Objectives, Reward, LastResult);
        }

        private static List<D22QuestObjectiveReadModel> BuildObjectives(
            D12DynamicQuestState state,
            D12DynamicQuestDefinition definition,
            List<D22ReadModelAdapterIssue> issues)
        {
            var states = new Dictionary<string, D12QuestObjectiveState>(StringComparer.Ordinal);
            if (state != null && state.Objectives != null)
            {
                foreach (var item in state.Objectives)
                {
                    if (item == null || !D22ReadModelAdapterSupport.IsStableId(item.ObjectiveId) || states.ContainsKey(item.ObjectiveId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_quest_objective", "d22.quest.objective_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                            "d22.quest.objective_invalid", "D12", "d12.quest.objective", false));
                        continue;
                    }
                    states.Add(item.ObjectiveId, item);
                }
            }
            var definitions = new Dictionary<string, D12QuestObjectiveDefinition>(StringComparer.Ordinal);
            if (definition != null && definition.Objectives != null)
            {
                foreach (var item in definition.Objectives)
                    if (item != null && D22ReadModelAdapterSupport.IsStableId(item.ObjectiveId) && !definitions.ContainsKey(item.ObjectiveId))
                        definitions.Add(item.ObjectiveId, item);
            }
            var ids = new List<string>();
            foreach (var id in states.Keys) ids.Add(id);
            foreach (var id in definitions.Keys) if (!ids.Contains(id)) ids.Add(id);
            ids.Sort(StringComparer.Ordinal);
            var result = new List<D22QuestObjectiveReadModel>();
            foreach (var id in ids)
            {
                D12QuestObjectiveState objectiveState;
                states.TryGetValue(id, out objectiveState);
                D12QuestObjectiveDefinition objectiveDefinition;
                definitions.TryGetValue(id, out objectiveDefinition);
                result.Add(new D22QuestObjectiveReadModel(objectiveDefinition, objectiveState));
            }
            return result;
        }

        private static D22QuestRewardReadModel BuildReward(D12DynamicQuestState state, D14RewardGrantRecord grant)
        {
            return state == null && grant == null
                ? null
                : new D22QuestRewardReadModel(grant, state == null ? null : state.RewardRequest);
        }

        private static D22AdapterOutcomeReadModel BuildResult(
            D12QuestResult result,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (result == null) return null;
            return D22ReadModelAdapterSupport.Outcome(
                "quest", "D12", true, result.Accepted || result.Applied, result.Duplicate,
                result.Code ?? string.Empty, result.Code ?? string.Empty, string.Empty,
                result.Accepted || result.Applied ? string.Empty : "d22.quest.result_rejected", issues);
        }

        private static void ValidateId(
            string value,
            string code,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues,
            bool allowEmpty = false)
        {
            if (!D22ReadModelAdapterSupport.IsStableId(value, allowEmpty))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    code, "d22.quest." + code, D22ReadModelAdapterSupport.Key("quest", "partial"),
                    "d22.quest." + code, "D12", moduleId, false));
            }
        }

        private static int CompareObjectives(D22QuestObjectiveReadModel left, D22QuestObjectiveReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.ObjectiveId,
                right == null ? null : right.ObjectiveId);
        }
    }

    /// <summary>D22-L22 任务 UI 读模型。</summary>
    [Serializable]
    public sealed class D22QuestReadModel : D22ReadModelAdapterModel
    {
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public int JournalFactCount { get; }
        public IReadOnlyList<D22QuestItemReadModel> Quests { get; }
        public D22AdapterOutcomeReadModel LastRewardOperation { get; }

        private D22QuestReadModel(
            D22ReadModelAdapterMetadata metadata,
            int worldDay,
            long saveRevision,
            int journalFactCount,
            IEnumerable<D22QuestItemReadModel> quests,
            D22AdapterOutcomeReadModel lastRewardOperation)
            : base(metadata)
        {
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            JournalFactCount = journalFactCount;
            var copied = new List<D22QuestItemReadModel>();
            if (quests != null)
                foreach (var quest in quests)
                    if (quest != null) copied.Add(quest.Clone());
            copied.Sort(CompareQuests);
            Quests = new ReadOnlyCollection<D22QuestItemReadModel>(copied);
            LastRewardOperation = lastRewardOperation == null ? null : lastRewardOperation.Clone();
        }

        public D22QuestReadModel Clone()
        {
            return new D22QuestReadModel(CopyMetadata(), WorldDay, SaveRevision, JournalFactCount, Quests, LastRewardOperation);
        }

        public static D22QuestReadModel Project(D22QuestOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d12.quest");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.quest.snapshot_missing", D22ReadModelAdapterSupport.Key("quest", "blocked"),
                    "d22.quest.snapshot_missing", "D12", "d12.quest", true));
                return Empty(issues, missing);
            }
            if (snapshot.SchemaVersion != D22QuestOwnerSnapshot.CurrentSchemaVersion)
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.quest.schema_version_mismatch", D22ReadModelAdapterSupport.Key("quest", "invalid"),
                    "d22.quest.schema_version_mismatch", "D22QuestOwnerSnapshot", "d22.quest", true));
            if (snapshot.SourceSchemaVersion <= 0)
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.quest.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("quest", "invalid"),
                    "d22.quest.source_schema_version_invalid", "D12", "d12.quest", true));
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(snapshot.SaveRevision, snapshot.WorldDay, "D12", "d12.quest", issues);
            if (snapshot.QuestLog == null)
            {
                missing.Add("d12.quest");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_quest_log", "d22.quest.log_missing", D22ReadModelAdapterSupport.Key("quest", "blocked"),
                    "d22.quest.log_missing", "D12", "d12.quest", true));
                return Empty(issues, MergeMissing(missing, snapshot.MissingModuleIds));
            }
            if (snapshot.QuestLog.SchemaVersion != D12DynamicQuestLogSnapshot.CurrentSchemaVersion)
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_quest_log_schema", "d22.quest.source_schema_version_mismatch", D22ReadModelAdapterSupport.Key("quest", "invalid"),
                    "d22.quest.source_schema_version_mismatch", "D12", "d12.quest", true));
            if (snapshot.Definitions == null)
            {
                missing.Add("d12.quest.definition");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_quest_definitions", "d22.quest.definitions_missing", D22ReadModelAdapterSupport.Key("quest", "partial"),
                    "d22.quest.definitions_missing", "D12", "d12.quest.definition", false));
            }

            var definitions = new Dictionary<string, D12DynamicQuestDefinition>(StringComparer.Ordinal);
            if (snapshot.Definitions != null)
            {
                foreach (var definition in snapshot.Definitions)
                {
                    if (definition == null || !D22ReadModelAdapterSupport.IsStableId(definition.QuestId) || definitions.ContainsKey(definition.QuestId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_quest_definition", "d22.quest.definition_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                            "d22.quest.definition_invalid", "D12", "d12.quest.definition", false));
                        continue;
                    }
                    definitions.Add(definition.QuestId, definition);
                }
            }
            var results = new Dictionary<string, D12QuestResult>(StringComparer.Ordinal);
            if (snapshot.Results != null)
            {
                foreach (var result in snapshot.Results)
                {
                    if (result == null || !D22ReadModelAdapterSupport.IsStableId(result.QuestId) || results.ContainsKey(result.QuestId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_quest_result", "d22.quest.result_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                            "d22.quest.result_invalid", "D12", "d12.quest.result", false));
                        continue;
                    }
                    results.Add(result.QuestId, result);
                }
            }
            var grants = new Dictionary<string, D14RewardGrantRecord>(StringComparer.Ordinal);
            if (snapshot.RewardSnapshot != null)
            {
                if (snapshot.RewardSnapshot.SchemaVersion != D14RewardRuntimeSnapshot.CurrentSchemaVersion)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_reward_snapshot_schema", "d22.quest.reward_schema_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                        "d22.quest.reward_schema_invalid", "D14", "d14.reward", false));
                if (snapshot.RewardSnapshot.Grants != null)
                {
                    foreach (var grant in snapshot.RewardSnapshot.Grants)
                    {
                        if (grant == null || string.IsNullOrEmpty(grant.GrantId) || grants.ContainsKey(grant.GrantId))
                        {
                            issues.Add(D22ReadModelAdapterSupport.Issue(
                                "invalid_reward_grant", "d22.quest.reward_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                                "d22.quest.reward_invalid", "D14", "d14.reward", false));
                            continue;
                        }
                        grants.Add(grant.GrantId, grant);
                    }
                }
            }

            var questItems = new List<D22QuestItemReadModel>();
            var seenQuestIds = new HashSet<string>(StringComparer.Ordinal);
            if (snapshot.QuestLog.Quests != null)
            {
                foreach (var state in snapshot.QuestLog.Quests)
                {
                    if (state == null || !D22ReadModelAdapterSupport.IsStableId(state.QuestId) || !seenQuestIds.Add(state.QuestId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_quest_state", "d22.quest.state_invalid", D22ReadModelAdapterSupport.Key("quest", "partial"),
                            "d22.quest.state_invalid", "D12", "d12.quest", false));
                        continue;
                    }
                    D12DynamicQuestDefinition definition;
                    definitions.TryGetValue(state.QuestId, out definition);
                    D12QuestResult result;
                    results.TryGetValue(state.QuestId, out result);
                    D14RewardGrantRecord grant = FindGrantForQuest(grants, state.QuestId, state.RewardRequest);
                    questItems.Add(new D22QuestItemReadModel(state, definition, result, grant, issues));
                }
            }

            D22AdapterOutcomeReadModel rewardOperation = null;
            if (snapshot.LastRewardResult != null)
            {
                var reward = snapshot.LastRewardResult;
                D22ReadModelAdapterSupport.ValidateSourceCode(reward.Code, "D14", "d14.reward.result", issues);
                rewardOperation = D22ReadModelAdapterSupport.Outcome(
                    "quest", "D14", true, reward.Accepted, reward.Duplicate,
                    reward.Code ?? string.Empty, reward.Code ?? string.Empty, string.Empty,
                    reward.Accepted ? string.Empty : "d22.quest.reward_rejected", issues);
            }
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22QuestOwnerSnapshot", "d12.quest", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22QuestOwnerSnapshot", "d12.quest", issues);
            }
            var status = D22ReadModelAdapterSupport.ResolveStatus(
                snapshot.SchemaVersion != D22QuestOwnerSnapshot.CurrentSchemaVersion,
                false,
                issues.Count > 0,
                issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "quest", "D12", snapshot.SourceSchemaVersion, snapshot.SaveRevision, snapshot.WorldDay, status,
                string.Empty, string.Empty, sourceReason, sourceReason, sourceLoc,
                MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22QuestReadModel(
                metadata,
                snapshot.WorldDay,
                snapshot.SaveRevision,
                snapshot.QuestLog.JournalFacts == null ? 0 : snapshot.QuestLog.JournalFacts.Count,
                questItems,
                rewardOperation);
        }

        public static D22QuestReadModel Adapt(D22QuestOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22QuestReadModel FromSnapshot(D22QuestOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22QuestReadModel Empty(
            List<D22ReadModelAdapterIssue> issues,
            IEnumerable<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "quest", "D12", 0, 0, 0, D22ReadModelAdapterStatus.Blocked,
                "d22.quest.source_missing", string.Empty, "d22.quest.source_missing",
                "d22.quest.source_missing", string.Empty, missing, issues);
            return new D22QuestReadModel(metadata, 0, 0, 0, new List<D22QuestItemReadModel>(), null);
        }

        private static D14RewardGrantRecord FindGrantForQuest(
            IDictionary<string, D14RewardGrantRecord> grants,
            string questId,
            D12QuestRewardRequest rewardRequest)
        {
            if (grants == null || string.IsNullOrEmpty(questId)) return null;
            D14RewardGrantRecord selected = null;
            foreach (var pair in grants)
            {
                var grant = pair.Value;
                if (grant != null && (string.Equals(grant.RewardId, questId, StringComparison.Ordinal) ||
                    string.Equals(grant.RequestId, questId, StringComparison.Ordinal) ||
                    (rewardRequest != null &&
                        (string.Equals(grant.RewardId, rewardRequest.RewardId, StringComparison.Ordinal) ||
                         string.Equals(grant.RequestId, rewardRequest.RequestId, StringComparison.Ordinal)))))
                {
                    if (selected == null || D22ReadModelAdapterSupport.CompareNullable(grant.GrantId, selected.GrantId) < 0)
                        selected = grant;
                }
            }
            return selected;
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static int CompareQuests(D22QuestItemReadModel left, D22QuestItemReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.QuestId,
                right == null ? null : right.QuestId);
        }
    }
}
