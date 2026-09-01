// D20NpcRoleCatalog —— NPC 岗位的纯逻辑目录。
//
// 目录只描述稳定岗位键、语义标签和 owner 可解释的建议引用；它不保存
// NPC 当前状态，不分配岗位、不计算工资，也不把视觉资源存在性当作岗位事实。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    [Flags]
    public enum D20NpcRoleCapability
    {
        None = 0,
        Dialogue = 1,
        Trade = 2,
        Service = 4,
        Repair = 8,
        Guard = 16,
        Inspection = 32,
        Rumor = 64,
        Fishing = 128,
    }

    [Serializable]
    public sealed class D20NpcRoleDefinition
    {
        public string RoleId = string.Empty;
        public string DisplayNameKey = string.Empty;
        public string RoleVariantId = string.Empty;
        public string SuggestedDutyId = string.Empty;
        public D20NpcRoleCapability Capabilities;

        public D20NpcRoleDefinition Clone()
        {
            return new D20NpcRoleDefinition
            {
                RoleId = RoleId,
                DisplayNameKey = DisplayNameKey,
                RoleVariantId = RoleVariantId,
                SuggestedDutyId = SuggestedDutyId,
                Capabilities = Capabilities,
            };
        }
    }

    [Serializable]
    public sealed class D20NpcRoleResolution
    {
        public string InputRoleId = string.Empty;
        public string CanonicalRoleId = string.Empty;
        public bool Known;
        public bool DeprecatedAlias;
        public bool UnknownPreserved;
        public string ReasonCode = string.Empty;

        public D20NpcRoleResolution Clone()
        {
            return new D20NpcRoleResolution
            {
                InputRoleId = InputRoleId,
                CanonicalRoleId = CanonicalRoleId,
                Known = Known,
                DeprecatedAlias = DeprecatedAlias,
                UnknownPreserved = UnknownPreserved,
                ReasonCode = ReasonCode,
            };
        }
    }

    public static class D20NpcRoleCatalog
    {
        public const string OwnerSystem = "D20NpcRoleCatalog";

        public const string MerchantRoleId = "merchant";
        public const string DockhandRoleId = "dockhand";
        public const string TavernRoleId = "tavern";
        public const string TavernKeeperRoleId = TavernRoleId;
        public const string ShipyardRoleId = "shipyard";
        public const string ShipwrightRoleId = ShipyardRoleId;
        public const string GuardRoleId = "guard";
        public const string HarborClerkRoleId = "harbor_clerk";
        public const string LookoutRoleId = "lookout";
        public const string FisherRoleId = "fisher";

        private const D20NpcRoleCapability AllCapabilities =
            D20NpcRoleCapability.Dialogue
            | D20NpcRoleCapability.Trade
            | D20NpcRoleCapability.Service
            | D20NpcRoleCapability.Repair
            | D20NpcRoleCapability.Guard
            | D20NpcRoleCapability.Inspection
            | D20NpcRoleCapability.Rumor
            | D20NpcRoleCapability.Fishing;

        private static readonly IReadOnlyList<D20NpcRoleDefinition> Definitions =
            new List<D20NpcRoleDefinition>
            {
                Role(MerchantRoleId, "npc.role.merchant", "npc_role_variant_merchant", "shore_trade",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Trade | D20NpcRoleCapability.Service),
                Role(DockhandRoleId, "npc.role.dockhand", "npc_role_variant_dockhand", "deckhand",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Service),
                Role(TavernRoleId, "npc.role.tavern", "npc_role_variant_tavern", "shore_service",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Service | D20NpcRoleCapability.Rumor),
                Role(ShipyardRoleId, "npc.role.shipyard", "npc_role_variant_shipyard", "repair",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Service | D20NpcRoleCapability.Repair),
                Role(GuardRoleId, "npc.role.guard", "npc_role_variant_guard", "shore_guard",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Guard | D20NpcRoleCapability.Inspection),
                Role(HarborClerkRoleId, "npc.role.harbor_clerk", "npc_role_variant_harbor_clerk", "shore_trade",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Trade
                    | D20NpcRoleCapability.Service | D20NpcRoleCapability.Inspection),
                Role(LookoutRoleId, "npc.role.lookout", "npc_role_variant_lookout", "lookout",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Guard),
                Role(FisherRoleId, "npc.role.fisher", "npc_role_variant_fisher", "fishing",
                    D20NpcRoleCapability.Dialogue | D20NpcRoleCapability.Service | D20NpcRoleCapability.Fishing),
            }.AsReadOnly();

        private static readonly IReadOnlyDictionary<string, string> DeprecatedRoleMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "trader", MerchantRoleId },
                { "dock_worker", DockhandRoleId },
                { "innkeeper", TavernRoleId },
                { "shipwright", ShipyardRoleId },
                { "watchman", GuardRoleId },
                { "harbor_officer", HarborClerkRoleId },
            };

        /// <summary>返回定义副本，避免调用方修改静态目录。</summary>
        public static IReadOnlyList<D20NpcRoleDefinition> All
        {
            get
            {
                var result = new List<D20NpcRoleDefinition>();
                foreach (var definition in Definitions) result.Add(definition.Clone());
                return result.AsReadOnly();
            }
        }

        public static D20NpcRoleDefinition Find(string roleId)
        {
            if (string.IsNullOrEmpty(roleId)) return null;
            foreach (var definition in Definitions)
                if (string.Equals(definition.RoleId, roleId, StringComparison.Ordinal))
                    return definition.Clone();
            return null;
        }

        public static bool TryGetRole(string roleId, out D20NpcRoleDefinition definition)
        {
            definition = Find(roleId);
            return definition != null;
        }

        public static bool IsKnownRole(string roleId)
        {
            return Find(roleId) != null;
        }

        /// <summary>
        /// 只读解析岗位键。未知但格式正确的岗位会原样保留并明确标记，
        /// 不会被映射到最相近的已知岗位。
        /// </summary>
        public static D20NpcRoleResolution Resolve(string roleId)
        {
            var result = new D20NpcRoleResolution { InputRoleId = roleId ?? string.Empty };
            if (!D20NpcIdentityContracts.IsStableReference(roleId, false))
            {
                result.ReasonCode = "role_id_invalid";
                return result;
            }

            string mapped;
            if (DeprecatedRoleMap.TryGetValue(roleId, out mapped))
            {
                result.CanonicalRoleId = mapped;
                result.Known = true;
                result.DeprecatedAlias = true;
                result.ReasonCode = "role_id_deprecated_alias";
                return result;
            }

            if (IsKnownRole(roleId))
            {
                result.CanonicalRoleId = roleId;
                result.Known = true;
                result.ReasonCode = "role_id_canonical";
                return result;
            }

            result.CanonicalRoleId = roleId;
            result.UnknownPreserved = true;
            result.ReasonCode = "role_id_unknown_preserved";
            return result;
        }

        public static bool TryResolveRoleId(string roleId, out string canonicalRoleId)
        {
            var resolution = Resolve(roleId);
            canonicalRoleId = resolution.CanonicalRoleId;
            return resolution.Known;
        }

        /// <summary>检查目录内部一致性；不改变目录定义。</summary>
        public static List<string> ValidateCatalog()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions)
            {
                if (definition == null)
                {
                    errors.Add("role_definition_missing");
                    continue;
                }
                if (!ids.Add(definition.RoleId)) errors.Add("role_id_duplicate:" + definition.RoleId);
                if (!D20NpcIdentityContracts.IsStableReference(definition.RoleId, false))
                    errors.Add("role_id_invalid:" + definition.RoleId);
                if (!D20NpcIdentityContracts.IsStableReference(definition.DisplayNameKey, false))
                    errors.Add("display_name_key_invalid:" + definition.RoleId);
                if (!D20NpcIdentityContracts.IsStableReference(definition.RoleVariantId, false))
                    errors.Add("role_variant_invalid:" + definition.RoleId);
                if (!D20NpcIdentityContracts.IsStableReference(definition.SuggestedDutyId, true))
                    errors.Add("suggested_duty_invalid:" + definition.RoleId);
                if ((definition.Capabilities & ~AllCapabilities) != 0)
                    errors.Add("role_capability_invalid:" + definition.RoleId);
            }
            return errors;
        }

        private static D20NpcRoleDefinition Role(
            string roleId,
            string displayNameKey,
            string roleVariantId,
            string suggestedDutyId,
            D20NpcRoleCapability capabilities)
        {
            return new D20NpcRoleDefinition
            {
                RoleId = roleId,
                DisplayNameKey = displayNameKey,
                RoleVariantId = roleVariantId,
                SuggestedDutyId = suggestedDutyId,
                Capabilities = capabilities,
            };
        }
    }
}
