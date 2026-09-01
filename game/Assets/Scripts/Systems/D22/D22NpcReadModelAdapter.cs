// ============================================================
// D22NpcReadModelAdapter —— D22-L21 NPC 只读 UI 适配器（纯 C#）
//
// D20 提供身份、岗位与交互资格公开投影。本适配器只复制这些投影，
// 将对话资格、可用性、失败码和本地化 key 映射成 D22 可读字段；
// 不触发对话/交易/关系/库存写入，也不从显示文本猜测角色事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D20;

namespace Whalebone.Systems.D22
{
    /// <summary>单个 D20 NPC 公开投影的 D22 输入边界。</summary>
    [Serializable]
    public sealed class D22NpcOwnerEntry
    {
        public D20NpcIdentity Identity;
        public D20NpcRoleDefinition Role;
        public D20NpcInteractionAvailabilityResult Interaction;
        public string SourceLocalizationKey = string.Empty;
    }

    /// <summary>D20 NPC owner 的输入边界。</summary>
    [Serializable]
    public sealed class D22NpcOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public int WorldDay;
        public long SaveRevision;
        public List<D22NpcOwnerEntry> Npcs = new List<D22NpcOwnerEntry>();
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>NPC 的 D22 防御性只读投影。</summary>
    [Serializable]
    public sealed class D22NpcItemReadModel
    {
        public string NpcId { get; }
        public string RoleId { get; }
        public string FactionId { get; }
        public string ContentId { get; }
        public string Availability { get; }
        public string RoleDisplayNameKey { get; }
        public string RoleVariantId { get; }
        public string SuggestedDutyId { get; }
        public string Capabilities { get; }
        public bool IsAvailable { get; }
        public bool DialogueQualified { get; }
        public bool InteractionAvailable { get; }
        public string InteractionKind { get; }
        public string InteractionReasonCode { get; }
        public string InteractionFailureCode { get; }
        public string RequiredCapability { get; }
        public string RetryPolicy { get; }
        public int CooldownRemainingTicks { get; }
        public string DisabledReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public string SourceErrorCode { get; }
        public D22AdapterOutcomeReadModel InteractionOutcome { get; }

        internal D22NpcItemReadModel(
            D20NpcIdentity identity,
            D20NpcRoleDefinition role,
            D20NpcInteractionAvailabilityResult interaction,
            string sourceLocalizationKey,
            List<D22ReadModelAdapterIssue> issues)
        {
            NpcId = identity == null ? string.Empty : identity.NpcId ?? string.Empty;
            RoleId = identity == null ? string.Empty : identity.RoleId ?? string.Empty;
            FactionId = identity == null ? string.Empty : identity.FactionId ?? string.Empty;
            ContentId = identity == null ? string.Empty : identity.ContentId ?? string.Empty;
            Availability = identity == null ? string.Empty : identity.Availability.ToString();
            RoleDisplayNameKey = role == null ? string.Empty : role.DisplayNameKey ?? string.Empty;
            RoleVariantId = role == null ? string.Empty : role.RoleVariantId ?? string.Empty;
            SuggestedDutyId = role == null ? string.Empty : role.SuggestedDutyId ?? string.Empty;
            Capabilities = role == null ? string.Empty : role.Capabilities.ToString();
            IsAvailable = identity != null && identity.Availability == D20CharacterAvailability.Available;
            InteractionAvailable = interaction != null && interaction.Available;
            DialogueQualified = interaction != null &&
                interaction.InteractionKind == D20NpcInteractionKind.Talk && interaction.Available;
            InteractionKind = interaction == null ? string.Empty : interaction.InteractionKind.ToString();
            InteractionReasonCode = interaction == null ? string.Empty : interaction.ReasonCode ?? string.Empty;
            InteractionFailureCode = interaction == null ? string.Empty : interaction.FailureCode.ToString();
            RequiredCapability = interaction == null ? string.Empty : interaction.RequiredCapability.ToString();
            RetryPolicy = interaction == null ? string.Empty : interaction.RetryPolicy.ToString();
            CooldownRemainingTicks = interaction == null ? 0 : interaction.CooldownRemainingTicks;
            DisabledReasonCode = InteractionAvailable ? string.Empty :
                (interaction == null
                    ? "d22.npc.dialogue_qualification_missing"
                    : (string.IsNullOrEmpty(interaction.ReasonCode)
                        ? "d22.npc.interaction_unavailable" : interaction.ReasonCode));
            LocalizationKey = D22ReadModelAdapterSupport.Key("npc", "item");
            SourceLocalizationKey = string.IsNullOrEmpty(sourceLocalizationKey)
                ? (interaction == null ? string.Empty : interaction.LocalizationKey ?? string.Empty)
                : sourceLocalizationKey;
            SourceErrorCode = interaction == null || interaction.FailureCode == D20NpcInteractionFailureCode.None
                ? string.Empty : interaction.FailureCode.ToString();
            InteractionOutcome = interaction == null ? null : D22ReadModelAdapterSupport.Outcome(
                "npc", "D20", true, interaction.Available, false,
                SourceErrorCode, interaction.ReasonCode, SourceLocalizationKey,
                InteractionAvailable ? string.Empty : DisabledReasonCode, issues);
        }

        private D22NpcItemReadModel(
            string npcId,
            string roleId,
            string factionId,
            string contentId,
            string availability,
            string roleDisplayNameKey,
            string roleVariantId,
            string suggestedDutyId,
            string capabilities,
            bool isAvailable,
            bool dialogueQualified,
            bool interactionAvailable,
            string interactionKind,
            string interactionReasonCode,
            string interactionFailureCode,
            string requiredCapability,
            string retryPolicy,
            int cooldownRemainingTicks,
            string disabledReasonCode,
            string localizationKey,
            string sourceLocalizationKey,
            string sourceErrorCode,
            D22AdapterOutcomeReadModel interactionOutcome)
        {
            NpcId = npcId ?? string.Empty;
            RoleId = roleId ?? string.Empty;
            FactionId = factionId ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            Availability = availability ?? string.Empty;
            RoleDisplayNameKey = roleDisplayNameKey ?? string.Empty;
            RoleVariantId = roleVariantId ?? string.Empty;
            SuggestedDutyId = suggestedDutyId ?? string.Empty;
            Capabilities = capabilities ?? string.Empty;
            IsAvailable = isAvailable;
            DialogueQualified = dialogueQualified;
            InteractionAvailable = interactionAvailable;
            InteractionKind = interactionKind ?? string.Empty;
            InteractionReasonCode = interactionReasonCode ?? string.Empty;
            InteractionFailureCode = interactionFailureCode ?? string.Empty;
            RequiredCapability = requiredCapability ?? string.Empty;
            RetryPolicy = retryPolicy ?? string.Empty;
            CooldownRemainingTicks = cooldownRemainingTicks;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            InteractionOutcome = interactionOutcome == null ? null : interactionOutcome.Clone();
        }

        public D22NpcItemReadModel Clone()
        {
            return new D22NpcItemReadModel(
                NpcId, RoleId, FactionId, ContentId, Availability, RoleDisplayNameKey,
                RoleVariantId, SuggestedDutyId, Capabilities, IsAvailable, DialogueQualified,
                InteractionAvailable, InteractionKind, InteractionReasonCode, InteractionFailureCode,
                RequiredCapability, RetryPolicy, CooldownRemainingTicks, DisabledReasonCode,
                LocalizationKey, SourceLocalizationKey, SourceErrorCode, InteractionOutcome);
        }
    }

    /// <summary>D22-L21 NPC UI 读模型。</summary>
    [Serializable]
    public sealed class D22NpcReadModel : D22ReadModelAdapterModel
    {
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public IReadOnlyList<D22NpcItemReadModel> Npcs { get; }

        private D22NpcReadModel(
            D22ReadModelAdapterMetadata metadata,
            int worldDay,
            long saveRevision,
            IEnumerable<D22NpcItemReadModel> npcs)
            : base(metadata)
        {
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            var copied = new List<D22NpcItemReadModel>();
            if (npcs != null)
                foreach (var npc in npcs)
                    if (npc != null) copied.Add(npc.Clone());
            copied.Sort(CompareNpcs);
            Npcs = new ReadOnlyCollection<D22NpcItemReadModel>(copied);
        }

        public D22NpcReadModel Clone()
        {
            return new D22NpcReadModel(CopyMetadata(), WorldDay, SaveRevision, Npcs);
        }

        public static D22NpcReadModel Project(D22NpcOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d20.npc");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.npc.snapshot_missing", D22ReadModelAdapterSupport.Key("npc", "blocked"),
                    "d22.npc.snapshot_missing", "D20", "d20.npc", true));
                return Empty(issues, missing);
            }
            if (snapshot.SchemaVersion != D22NpcOwnerSnapshot.CurrentSchemaVersion)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.npc.schema_version_mismatch", D22ReadModelAdapterSupport.Key("npc", "invalid"),
                    "d22.npc.schema_version_mismatch", "D22NpcOwnerSnapshot", "d22.npc", true));
            }
            if (snapshot.SourceSchemaVersion <= 0)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.npc.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("npc", "invalid"),
                    "d22.npc.source_schema_version_invalid", "D20", "d20.npc", true));
            }
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(snapshot.SaveRevision, snapshot.WorldDay, "D20", "d20.npc", issues);
            if (snapshot.Npcs == null)
            {
                missing.Add("d20.npc");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_npc_projection", "d22.npc.source_missing", D22ReadModelAdapterSupport.Key("npc", "blocked"),
                    "d22.npc.source_missing", "D20", "d20.npc", true));
            }

            var result = new List<D22NpcItemReadModel>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (snapshot.Npcs != null)
            {
                foreach (var entry in snapshot.Npcs)
                {
                    if (entry == null || entry.Identity == null || !D22ReadModelAdapterSupport.IsStableId(entry.Identity.NpcId) ||
                        !seen.Add(entry.Identity.NpcId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_npc_projection", "d22.npc.identity_invalid", D22ReadModelAdapterSupport.Key("npc", "partial"),
                            "d22.npc.identity_invalid", "D20", "d20.npc", false));
                        continue;
                    }
                    var identity = entry.Identity;
                    if (identity.SchemaVersion != D20NpcIdentityContracts.CurrentSchemaVersion ||
                        !Enum.IsDefined(typeof(D20NpcIdentityMigrationStatus), identity.MigrationStatus))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_npc_identity", "d22.npc.identity_invalid", D22ReadModelAdapterSupport.Key("npc", "partial"),
                            "d22.npc.identity_invalid", "D20", "d20.npc.identity", false));
                    }
                    ValidateId(identity.RoleId, "role_id_invalid", "d20.npc.identity", issues, true);
                    ValidateId(identity.FactionId, "faction_id_invalid", "d20.npc.identity", issues, true);
                    ValidateId(identity.ContentId, "content_id_invalid", "d20.npc.identity", issues, true);
                    var role = entry.Role;
                    if (role == null && !string.IsNullOrEmpty(identity.RoleId)) role = D20NpcRoleCatalog.Find(identity.RoleId);
                    if (role == null)
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "missing_role_projection", "d22.npc.role_missing", D22ReadModelAdapterSupport.Key("npc", "partial"),
                            "d22.npc.role_missing", "D20", "d20.npc.role", false));
                    }
                    else
                    {
                        ValidateId(role.RoleId, "role_id_invalid", "d20.npc.role", issues);
                        D22ReadModelAdapterSupport.ValidateSourceKey(role.DisplayNameKey, "D20", "d20.npc.role", issues);
                    }
                    var interaction = entry.Interaction;
                    if (interaction == null)
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "missing_dialogue_qualification", "d22.npc.dialogue_qualification_missing", D22ReadModelAdapterSupport.Key("npc", "partial"),
                            "d22.npc.dialogue_qualification_missing", "D20", "d20.npc.interaction", false));
                    }
                    else
                    {
                        if (!interaction.IsReadOnly || interaction.NpcId != identity.NpcId)
                        {
                            issues.Add(D22ReadModelAdapterSupport.Issue(
                                "invalid_interaction_projection", "d22.npc.interaction_invalid", D22ReadModelAdapterSupport.Key("npc", "partial"),
                                "d22.npc.interaction_invalid", "D20", "d20.npc.interaction", false));
                        }
                        D22ReadModelAdapterSupport.ValidateSourceCode(interaction.ReasonCode, "D20", "d20.npc.interaction", issues);
                        if (interaction.FailureCode != D20NpcInteractionFailureCode.None)
                            D22ReadModelAdapterSupport.ValidateSourceCode(interaction.FailureCode.ToString(), "D20", "d20.npc.interaction", issues);
                        D22ReadModelAdapterSupport.ValidateSourceKey(interaction.LocalizationKey, "D20", "d20.npc.interaction", issues);
                    }
                    D22ReadModelAdapterSupport.ValidateSourceKey(entry.SourceLocalizationKey, "D22NpcOwnerSnapshot", "d20.npc", issues);
                    result.Add(new D22NpcItemReadModel(identity, role, interaction, entry.SourceLocalizationKey, issues));
                }
            }

            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22NpcOwnerSnapshot", "d20.npc", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22NpcOwnerSnapshot", "d20.npc", issues);
            }
            var status = D22ReadModelAdapterSupport.ResolveStatus(
                snapshot.SchemaVersion != D22NpcOwnerSnapshot.CurrentSchemaVersion,
                snapshot.Npcs == null,
                issues.Count > 0,
                issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "npc", "D20", snapshot.SourceSchemaVersion, snapshot.SaveRevision, snapshot.WorldDay, status,
                string.Empty, string.Empty, sourceReason,
                sourceReason, sourceLoc, MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22NpcReadModel(metadata, snapshot.WorldDay, snapshot.SaveRevision, result);
        }

        public static D22NpcReadModel Adapt(D22NpcOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22NpcReadModel FromSnapshot(D22NpcOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22NpcReadModel Empty(
            List<D22ReadModelAdapterIssue> issues,
            IEnumerable<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "npc", "D20", 0, 0, 0, D22ReadModelAdapterStatus.Blocked,
                "d22.npc.source_missing", string.Empty, "d22.npc.source_missing",
                "d22.npc.source_missing", string.Empty, missing, issues);
            return new D22NpcReadModel(metadata, 0, 0, new List<D22NpcItemReadModel>());
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
                    code, "d22.npc." + code, D22ReadModelAdapterSupport.Key("npc", "partial"),
                    "d22.npc." + code, "D20", moduleId, false));
            }
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static int CompareNpcs(D22NpcItemReadModel left, D22NpcItemReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.NpcId,
                right == null ? null : right.NpcId);
        }
    }
}
