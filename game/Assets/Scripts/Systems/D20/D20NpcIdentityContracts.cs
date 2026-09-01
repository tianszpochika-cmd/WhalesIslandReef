// D20NpcIdentityContracts —— NPC 身份的纯 C# 契约与迁移边界。
//
// NPC 的角色、派系和可用性在这里仅作为稳定引用/投影保存；D10、D19 或
// 其它 owner 仍负责事实写入。本契约保留原始 ID 和迁移状态，未知值不会
// 被悄悄改成另一个实体，也不会依靠视觉资源猜测身份。

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    public enum D20NpcIdentityMigrationStatus
    {
        Unknown = 0,
        Canonical = 1,
        DeprecatedAlias = 2,
        UnknownPreserved = 3,
        Unresolved = 4,
    }

    [Serializable]
    public sealed class D20NpcIdentity
    {
        public int SchemaVersion = D20NpcIdentityContracts.CurrentSchemaVersion;
        public string NpcId = string.Empty;
        public string RoleId = string.Empty;
        public string FactionId = string.Empty;
        public string ContentId = string.Empty;
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;
        public string SourceNpcId = string.Empty;
        public D20NpcIdentityMigrationStatus MigrationStatus = D20NpcIdentityMigrationStatus.Unknown;

        public D20NpcIdentity Clone()
        {
            return new D20NpcIdentity
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId,
                RoleId = RoleId,
                FactionId = FactionId,
                ContentId = ContentId,
                Availability = Availability,
                SourceNpcId = SourceNpcId,
                MigrationStatus = MigrationStatus,
            };
        }

        /// <summary>验证已解析身份；未知角色/派系引用仍可通过格式校验。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20NpcIdentityContracts.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 NPC identity schema_version mismatch.");
            if (!D20NpcIdentityContracts.IsStableNpcId(NpcId))
                throw new InvalidOperationException("D20 NPC id is missing or unstable.");
            if (!D20NpcIdentityContracts.IsStableReference(RoleId, true)
                || !D20NpcIdentityContracts.IsStableReference(FactionId, true)
                || !D20NpcIdentityContracts.IsStableReference(ContentId, true))
                throw new InvalidOperationException("D20 NPC identity contains an unstable reference.");
            if (!D20CharacterIdentityContracts.IsSupportedAvailability(Availability))
                throw new InvalidOperationException("D20 NPC availability is unknown.");
            if (!D20NpcIdentityContracts.IsPreservedSourceId(SourceNpcId))
                throw new InvalidOperationException("D20 NPC source id is too long or contains a control character.");
            if (!Enum.IsDefined(typeof(D20NpcIdentityMigrationStatus), MigrationStatus)
                || MigrationStatus == D20NpcIdentityMigrationStatus.Unknown
                || MigrationStatus == D20NpcIdentityMigrationStatus.Unresolved)
                throw new InvalidOperationException("D20 NPC migration status is unresolved.");
        }
    }

    public static class D20NpcIdentityContracts
    {
        public const int CurrentSchemaVersion = 1;
        public const string OwnerSystem = "D20NpcIdentityContracts";
        public const int SourceNpcIdMaxLength = D20CharacterIdentityContracts.CharacterIdMaxLength;

        private static readonly IReadOnlyDictionary<string, string> DeprecatedNpcIdMap =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                // These are migration aliases only. They do not declare new NPC
                // entities and retain the documented canonical IDs.
                { "npc_harbor_clerk", "npc_harbor_clerk_001" },
                { "npc_faction_officer_eng", "npc_faction_officer_eng_001" },
                { "npc_santa_margarita_dockhand", "npc:santa_margarita_dockhand" },
                { "npc_santa_margarita_lookout", "npc:santa_margarita_lookout" },
                { "npc_santa_margarita_tavern_keeper", "npc:santa_margarita_tavern_keeper" },
            });

        /// <summary>只读弃用映射；调用方不能通过返回值修改迁移规则。</summary>
        public static IReadOnlyDictionary<string, string> DeprecatedNpcIdMappings
        {
            get { return DeprecatedNpcIdMap; }
        }

        public static bool IsStableNpcId(string npcId)
        {
            return D20CharacterIdentityContracts.IsStableCharacterId(npcId);
        }

        /// <summary>
        /// 引用格式只限制为可跨存档的 token。空值可表示 owner 尚未提供
        /// 角色/派系/内容引用；未知但格式正确的 token 必须原样保留。
        /// </summary>
        public static bool IsStableReference(string reference, bool allowEmpty)
        {
            if (string.IsNullOrEmpty(reference)) return allowEmpty;
            if (reference.Length > D20CharacterIdentityContracts.ReferenceMaxLength
                || reference.Trim() != reference)
                return false;

            for (int i = 0; i < reference.Length; i++)
            {
                char c = reference[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }

        public static bool IsPreservedSourceId(string sourceNpcId)
        {
            if (string.IsNullOrEmpty(sourceNpcId)) return true;
            if (sourceNpcId.Length > SourceNpcIdMaxLength) return false;
            for (int i = 0; i < sourceNpcId.Length; i++)
                if (char.IsControl(sourceNpcId[i])) return false;
            return true;
        }

        public static bool IsDeprecatedNpcId(string npcId)
        {
            string ignored;
            return !string.IsNullOrEmpty(npcId)
                && DeprecatedNpcIdMap.TryGetValue(npcId, out ignored);
        }

        private static bool IsDocumentedCanonicalNpcId(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return false;
            foreach (var pair in DeprecatedNpcIdMap)
                if (string.Equals(pair.Value, npcId, StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>
        /// 解析稳定 NPC ID。未知但格式正确的值按原值保留；非法值返回 false，
        /// 由 Migrate 保留原始文本并标记为 Unresolved，而不是猜测替代 ID。
        /// </summary>
        public static bool TryResolveNpcId(string npcId, out string canonicalNpcId)
        {
            canonicalNpcId = string.Empty;
            if (!IsStableNpcId(npcId)) return false;

            string mapped;
            canonicalNpcId = DeprecatedNpcIdMap.TryGetValue(npcId, out mapped)
                ? mapped : npcId;
            return true;
        }

        /// <summary>将旧/未知输入转换为可审计的身份投影，不写入任何 owner。</summary>
        public static D20NpcIdentity Migrate(
            string sourceNpcId,
            string roleId,
            string factionId,
            D20CharacterAvailability availability,
            string contentId = "")
        {
            string canonical;
            bool resolved = TryResolveNpcId(sourceNpcId, out canonical);
            bool deprecated = resolved && IsDeprecatedNpcId(sourceNpcId);

            return new D20NpcIdentity
            {
                SchemaVersion = CurrentSchemaVersion,
                NpcId = resolved ? canonical : string.Empty,
                RoleId = roleId ?? string.Empty,
                FactionId = factionId ?? string.Empty,
                ContentId = contentId ?? string.Empty,
                Availability = availability,
                SourceNpcId = sourceNpcId ?? string.Empty,
                MigrationStatus = !resolved
                    ? D20NpcIdentityMigrationStatus.Unresolved
                    : deprecated
                        ? D20NpcIdentityMigrationStatus.DeprecatedAlias
                        : (IsDocumentedCanonicalNpcId(canonical)
                            ? D20NpcIdentityMigrationStatus.Canonical
                            : D20NpcIdentityMigrationStatus.UnknownPreserved),
            };
        }

        public static bool IsReadyForRuntime(D20NpcIdentity identity)
        {
            if (identity == null) return false;
            try
            {
                identity.Validate();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
