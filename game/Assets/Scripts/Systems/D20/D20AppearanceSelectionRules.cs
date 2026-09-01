// ============================================================
// D20AppearanceSelectionRules —— D20-L09 外观选择校验（纯 C#）
//
// 本文件只消费 D10 的公开外观定义/快照和 D18 稳定内容键，生成换装前置
// 结果。外观所有权、耐久、装备和伤病历史仍由 D10AppearanceSystem 写入。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D10;

namespace Whalebone.Systems.D20
{
    /// <summary>外观选择的逻辑用途。</summary>
    public enum D20AppearanceSelectionMode
    {
        /// <summary>只生成预览，不改变 owner 状态。</summary>
        Preview = 0,
        /// <summary>请求 D10 owner 装备已拥有条目。</summary>
        Equip = 1,
        /// <summary>角色创建阶段的初始选择。</summary>
        Create = 2,
    }

    /// <summary>L09 外观选择失败码。</summary>
    public enum D20AppearanceSelectionCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>请求为空。</summary>
        MissingRequest = 1,
        /// <summary>版本不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>角色 ID 缺失或不稳定。</summary>
        MissingCharacterId = 3,
        /// <summary>外观 ID 不是稳定内容键。</summary>
        InvalidItemId = 4,
        /// <summary>D18 content_id 缺失或与外观不一致。</summary>
        InvalidContentId = 5,
        /// <summary>D18 resource_key 缺失或含不稳定路径。</summary>
        InvalidResourceKey = 6,
        /// <summary>D10 公开目录没有对应定义。</summary>
        DefinitionMissing = 7,
        /// <summary>定义槽位与请求槽位不一致。</summary>
        SlotMismatch = 8,
        /// <summary>普通选择不能写入伤病叠加层。</summary>
        InjuryProtected = 9,
        /// <summary>外观上下文不允许该条目。</summary>
        ContextRestricted = 10,
        /// <summary>装备模式需要 owner 已拥有事实。</summary>
        OwnershipRequired = 11,
        /// <summary>当前组合存在互斥冲突。</summary>
        SlotConflict = 12,
        /// <summary>请求模式未知。</summary>
        InvalidMode = 13,
        /// <summary>定义自身违反 D10/D18 稳定键规则。</summary>
        InvalidDefinition = 14,
        /// <summary>快照缺失必要的组合容器。</summary>
        InvalidSnapshot = 15,
    }

    /// <summary>只读外观选择请求；输入快照会在校验前被防御性复制。</summary>
    [Serializable]
    public sealed class D20AppearanceSelectionRequest
    {
        /// <summary>当前请求版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>请求版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>拥有该选择的角色稳定 ID。</summary>
        public string CharacterId = string.Empty;

        /// <summary>D10 外观条目的稳定 ID。</summary>
        public string ItemId = string.Empty;

        /// <summary>D18 内容注册表中的 content_id。</summary>
        public string ContentId = string.Empty;

        /// <summary>D18 内容注册表中的 resource_key。</summary>
        public string ResourceKey = string.Empty;

        /// <summary>期望写入的 D10 外观槽位。</summary>
        public D10AppearanceSlot Slot;

        /// <summary>选择用途。</summary>
        public D20AppearanceSelectionMode Mode = D20AppearanceSelectionMode.Preview;

        /// <summary>D10 公开上下文快照。</summary>
        public D10AppearanceContext Context = new D10AppearanceContext();

        /// <summary>D10 公开外观快照；只读用于拥有和组合前置。</summary>
        public D10AppearanceSnapshot AppearanceSnapshot = new D10AppearanceSnapshot();

        /// <summary>D10 目录定义的防御性输入副本；为空时由公开目录读取。</summary>
        public D10AppearanceItemDefinition Definition;

        /// <summary>产生请求的世界日。</summary>
        public int WorldDay;

        /// <summary>产生请求时的存档修订。</summary>
        public long SaveRevision;

        /// <summary>创建一个从 D10 公开目录自动填充稳定键的请求。</summary>
        public static D20AppearanceSelectionRequest Create(string characterId,
            string itemId, D10AppearanceSlot slot,
            D20AppearanceSelectionMode mode = D20AppearanceSelectionMode.Preview,
            D10AppearanceSnapshot appearanceSnapshot = null,
            D10AppearanceContext context = null)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            return new D20AppearanceSelectionRequest
            {
                CharacterId = characterId ?? string.Empty,
                ItemId = itemId ?? string.Empty,
                ContentId = itemId ?? string.Empty,
                ResourceKey = definition == null ? string.Empty : definition.ResourceKey ?? string.Empty,
                Slot = slot,
                Mode = mode,
                Context = context == null ? new D10AppearanceContext() : context.Clone(),
                AppearanceSnapshot = appearanceSnapshot == null
                    ? new D10AppearanceSnapshot()
                    : appearanceSnapshot.Clone(),
                Definition = definition == null ? null : definition.Clone(),
            };
        }

        /// <summary>返回防御性副本，不共享 D10 集合引用。</summary>
        public D20AppearanceSelectionRequest Clone()
        {
            return new D20AppearanceSelectionRequest
            {
                SchemaVersion = SchemaVersion,
                CharacterId = CharacterId ?? string.Empty,
                ItemId = ItemId ?? string.Empty,
                ContentId = ContentId ?? string.Empty,
                ResourceKey = ResourceKey ?? string.Empty,
                Slot = Slot,
                Mode = Mode,
                Context = Context == null ? new D10AppearanceContext() : Context.Clone(),
                AppearanceSnapshot = AppearanceSnapshot == null
                    ? new D10AppearanceSnapshot()
                    : AppearanceSnapshot.Clone(),
                Definition = Definition == null ? null : Definition.Clone(),
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>L09 外观选择的纯校验结果。</summary>
    [Serializable]
    public sealed class D20AppearanceSelectionResult
    {
        /// <summary>是否通过 D20 选择前置。</summary>
        public bool Valid;

        /// <summary>稳定结果码。</summary>
        public D20AppearanceSelectionCode Code;

        /// <summary>机器可读原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>合法后仍必须调用 D10 owner。</summary>
        public bool RequiresAppearanceOwner;

        /// <summary>只表示内容声明为占位资源，不表示运行时资源存在。</summary>
        public bool UsesPlaceholder;

        /// <summary>请求的稳定键和槽位。</summary>
        public string CharacterId = string.Empty;
        public string ItemId = string.Empty;
        public string ContentId = string.Empty;
        public string ResourceKey = string.Empty;
        public D10AppearanceSlot Slot;
        public D20AppearanceSelectionMode Mode;

        /// <summary>D10 定义的防御性副本。</summary>
        public D10AppearanceItemDefinition Definition;

        /// <summary>若通过，按当前组合推导的只读预览槽位。</summary>
        public D10AppearanceLoadout ProjectedLoadout;

        /// <summary>返回深拷贝。</summary>
        public D20AppearanceSelectionResult Clone()
        {
            return new D20AppearanceSelectionResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
                RequiresAppearanceOwner = RequiresAppearanceOwner,
                UsesPlaceholder = UsesPlaceholder,
                CharacterId = CharacterId ?? string.Empty,
                ItemId = ItemId ?? string.Empty,
                ContentId = ContentId ?? string.Empty,
                ResourceKey = ResourceKey ?? string.Empty,
                Slot = Slot,
                Mode = Mode,
                Definition = Definition == null ? null : Definition.Clone(),
                ProjectedLoadout = ProjectedLoadout == null ? null : ProjectedLoadout.Clone(),
            };
        }
    }

    /// <summary>D20-L09 只读外观选择规则。</summary>
    public static class D20AppearanceSelectionRules
    {
        /// <summary>当前规则版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>规则 owner 标识；不等于 D10 外观事实 owner。</summary>
        public const string OwnerSystem = "D20AppearanceSelectionRules";

        /// <summary>校验 D10 定义和 D18 稳定键，不修改任何输入快照。</summary>
        public static D20AppearanceSelectionResult Validate(
            D20AppearanceSelectionRequest request)
        {
            if (request == null)
                return Failure(D20AppearanceSelectionCode.MissingRequest, "appearance_request_missing");
            if (request.SchemaVersion != D20AppearanceSelectionRequest.CurrentSchemaVersion)
                return Failure(D20AppearanceSelectionCode.InvalidSchemaVersion,
                    "schema_version_mismatch");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(request.CharacterId))
                return Failure(D20AppearanceSelectionCode.MissingCharacterId, "missing_character_id");
            if (!IsStableContentId(request.ItemId, "outfit_"))
                return Failure(D20AppearanceSelectionCode.InvalidItemId, "invalid_outfit_id");
            if (!IsStableContentId(request.ContentId, "outfit_") ||
                !string.Equals(request.ContentId, request.ItemId, StringComparison.Ordinal))
                return Failure(D20AppearanceSelectionCode.InvalidContentId, "content_id_mismatch");
            if (!IsStableResourceKey(request.ResourceKey))
                return Failure(D20AppearanceSelectionCode.InvalidResourceKey, "invalid_resource_key");
            if (!Enum.IsDefined(typeof(D20AppearanceSelectionMode), request.Mode))
                return Failure(D20AppearanceSelectionCode.InvalidMode, "invalid_selection_mode");
            if (!Enum.IsDefined(typeof(D10AppearanceSlot), request.Slot))
                return Failure(D20AppearanceSelectionCode.InvalidDefinition, "invalid_appearance_slot");

            D10AppearanceItemDefinition definition = request.Definition == null
                ? D10AppearanceCatalog.Find(request.ItemId)
                : request.Definition.Clone();
            if (definition == null)
                return Failure(D20AppearanceSelectionCode.DefinitionMissing, "appearance_definition_missing");
            if (!ValidateDefinitionKeys(definition, request))
                return Failure(D20AppearanceSelectionCode.InvalidDefinition,
                    "appearance_definition_invalid");
            if (definition.Slot != request.Slot)
                return Failure(D20AppearanceSelectionCode.SlotMismatch, "appearance_slot_mismatch");
            if (definition.Slot == D10AppearanceSlot.Injury || definition.IsInjuryOverlay)
                return Failure(D20AppearanceSelectionCode.InjuryProtected, "injury_overlay_owner_only");

            D10AppearanceContext context = request.Context == null
                ? new D10AppearanceContext() : request.Context.Clone();
            D20AppearanceSelectionCode contextCode;
            string contextReason;
            if (!ValidateContext(definition, context, out contextCode, out contextReason))
                return Failure(contextCode, contextReason);

            D10AppearanceSnapshot snapshot = request.AppearanceSnapshot == null
                ? new D10AppearanceSnapshot() : request.AppearanceSnapshot.Clone();
            if (snapshot.ActiveLoadout == null || snapshot.Owned == null)
                return Failure(D20AppearanceSelectionCode.InvalidSnapshot, "appearance_snapshot_invalid");
            if (request.Mode == D20AppearanceSelectionMode.Equip &&
                !snapshot.Owned.ContainsKey(definition.Id))
                return Failure(D20AppearanceSelectionCode.OwnershipRequired,
                    "appearance_owner_ownership_required");

            D20AppearanceSelectionCode compositionCode;
            string compositionReason;
            if (!ValidateComposition(definition, snapshot.ActiveLoadout,
                out compositionCode, out compositionReason))
                return Failure(compositionCode, compositionReason);

            var projected = snapshot.ActiveLoadout.Clone();
            projected.Set(definition.Slot, definition.Id);
            return new D20AppearanceSelectionResult
            {
                Valid = true,
                Code = D20AppearanceSelectionCode.None,
                ReasonCode = "appearance_selection_valid",
                RequiresAppearanceOwner = true,
                UsesPlaceholder = IsDeclaredPlaceholder(definition.AssetId),
                CharacterId = request.CharacterId,
                ItemId = definition.Id,
                ContentId = request.ContentId,
                ResourceKey = request.ResourceKey,
                Slot = request.Slot,
                Mode = request.Mode,
                Definition = definition.Clone(),
                ProjectedLoadout = projected,
            };
        }

        /// <summary>Validate 的语义别名。</summary>
        public static D20AppearanceSelectionResult Evaluate(
            D20AppearanceSelectionRequest request)
        {
            return Validate(request);
        }

        /// <summary>验证 D18 content_id 是否为指定稳定前缀。</summary>
        public static bool IsStableContentId(string value, string requiredPrefix)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(requiredPrefix) ||
                !value.StartsWith(requiredPrefix, StringComparison.Ordinal) ||
                value.Length > 160 || value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }

        /// <summary>验证 D18 resource_key 的稳定引用形状，不检查二进制资源存在性。</summary>
        public static bool IsStableResourceKey(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 256 || value.Trim() != value ||
                value.StartsWith("/", StringComparison.Ordinal) ||
                value.IndexOf('\\') >= 0 || value.IndexOf("://", StringComparison.Ordinal) >= 0 ||
                value.IndexOf("..", StringComparison.Ordinal) >= 0)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == '/' || c == ':' || c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }

        /// <summary>校验代表性 D10 公共目录没有重复或不稳定 ID。</summary>
        public static List<string> ValidateCatalog()
        {
            var errors = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in D10AppearanceCatalog.All)
            {
                if (definition == null)
                {
                    errors.Add("null_definition");
                    continue;
                }
                if (!seen.Add(definition.Id ?? string.Empty))
                    errors.Add("duplicate_item_" + definition.Id);
                if (!IsStableContentId(definition.Id, "outfit_"))
                    errors.Add("invalid_item_" + definition.Id);
                if (!IsStableResourceKey(definition.ResourceKey))
                    errors.Add("invalid_resource_" + definition.Id);
            }
            return errors;
        }

        private static bool ValidateDefinitionKeys(D10AppearanceItemDefinition definition,
            D20AppearanceSelectionRequest request)
        {
            return IsStableContentId(definition.Id, "outfit_") &&
                string.Equals(definition.Id, request.ItemId, StringComparison.Ordinal) &&
                IsStableContentId(definition.AssetId, "asset_") &&
                IsStableResourceKey(definition.ResourceKey) &&
                string.Equals(definition.ResourceKey, request.ResourceKey, StringComparison.Ordinal) &&
                !string.IsNullOrEmpty(definition.DisplayNameKey) &&
                definition.MaxDurability >= 0 && definition.Price >= 0 && definition.RepairCost >= 0;
        }

        private static bool ValidateContext(D10AppearanceItemDefinition definition,
            D10AppearanceContext context, out D20AppearanceSelectionCode code,
            out string reasonCode)
        {
            code = D20AppearanceSelectionCode.None;
            reasonCode = string.Empty;
            if (context == null)
            {
                code = D20AppearanceSelectionCode.ContextRestricted;
                reasonCode = "appearance_context_missing";
                return false;
            }
            if (definition.CultureTags != null && definition.CultureTags.Count > 0 &&
                !definition.CultureTags.Contains(context.CultureId))
            {
                code = D20AppearanceSelectionCode.ContextRestricted;
                reasonCode = "appearance_culture_restricted";
                return false;
            }
            if (definition.CareerTags != null && definition.CareerTags.Count > 0 &&
                !definition.CareerTags.Contains(context.CareerId))
            {
                code = D20AppearanceSelectionCode.ContextRestricted;
                reasonCode = "appearance_career_restricted";
                return false;
            }
            if (definition.Slot == D10AppearanceSlot.CareerEquipment &&
                definition.WeatherTags != null && definition.WeatherTags.Count > 0 &&
                !definition.WeatherTags.Contains(context.WeatherId))
            {
                code = D20AppearanceSelectionCode.ContextRestricted;
                reasonCode = "appearance_weather_restricted";
                return false;
            }
            return true;
        }

        private static bool ValidateComposition(D10AppearanceItemDefinition definition,
            D10AppearanceLoadout loadout, out D20AppearanceSelectionCode code,
            out string reasonCode)
        {
            code = D20AppearanceSelectionCode.None;
            reasonCode = string.Empty;
            if (loadout == null || loadout.Slots == null)
            {
                code = D20AppearanceSelectionCode.InvalidSnapshot;
                reasonCode = "appearance_loadout_missing";
                return false;
            }
            foreach (var pair in loadout.Slots)
            {
                if (string.IsNullOrEmpty(pair.Value) || pair.Value == definition.Id) continue;
                var active = D10AppearanceCatalog.Find(pair.Value);
                if (active == null) continue;
                if (!string.IsNullOrEmpty(definition.ExclusiveGroup) &&
                    definition.ExclusiveGroup == active.ExclusiveGroup &&
                    definition.Slot != active.Slot)
                {
                    code = D20AppearanceSelectionCode.SlotConflict;
                    reasonCode = "appearance_exclusive_group_conflict";
                    return false;
                }
                if ((definition.ExcludesItemIds != null && definition.ExcludesItemIds.Contains(active.Id)) ||
                    (active.ExcludesItemIds != null && active.ExcludesItemIds.Contains(definition.Id)))
                {
                    code = D20AppearanceSelectionCode.SlotConflict;
                    reasonCode = "appearance_excluded_item_conflict";
                    return false;
                }
            }
            return true;
        }

        private static bool IsDeclaredPlaceholder(string assetId)
        {
            return !string.IsNullOrEmpty(assetId) &&
                assetId.StartsWith("asset_ph_", StringComparison.Ordinal);
        }

        private static D20AppearanceSelectionResult Failure(
            D20AppearanceSelectionCode code, string reasonCode)
        {
            return new D20AppearanceSelectionResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
                RequiresAppearanceOwner = false,
            };
        }
    }
}
