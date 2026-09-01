// ============================================================
// D21ShipIdentityContracts —— D21-L02 船只身份契约（纯 C#）
//
// 本文件只拥有船只身份的版本化投影和旧值迁移规则。船况、组件、货物、
// 船员、天气以及港口状态由各自 owner 写入；旧 ShipState 只能作为迁移输入。
// 所有字符串 ID 都是逻辑引用，不包含场景、Prefab、Transform 或资源路径。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>船只所有权语义。所有权变更仍需由 Property/Legal owner 裁决。</summary>
    public enum D21ShipOwnershipKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>尚未分配给具体所有者。</summary>
        Unassigned = 1,
        /// <summary>私人所有。</summary>
        Private = 2,
        /// <summary>租赁使用。</summary>
        Leased = 3,
        /// <summary>存在抵押权。</summary>
        Mortgaged = 4,
        /// <summary>由船队管理。</summary>
        FleetManaged = 5,
        /// <summary>基于合法打捞的待确认权利。</summary>
        SalvageClaim = 6,
        /// <summary>因法律或债务被扣押。</summary>
        Impounded = 7,
        /// <summary>所有权事实已丧失。</summary>
        Lost = 8,
    }

    /// <summary>船籍/旗帜的稳定语义枚举，不等同于法律许可或势力声望。</summary>
    public enum D21ShipFlagKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>没有登记旗帜。</summary>
        Unregistered = 1,
        /// <summary>英格兰船籍。</summary>
        England = 2,
        /// <summary>西班牙船籍。</summary>
        Spain = 3,
        /// <summary>法国船籍。</summary>
        France = 4,
        /// <summary>荷兰共和国船籍。</summary>
        DutchRepublic = 5,
        /// <summary>葡萄牙船籍。</summary>
        Portugal = 6,
        /// <summary>丹麦船籍。</summary>
        Denmark = 7,
        /// <summary>地方社群旗帜。</summary>
        LocalCommunity = 8,
        /// <summary>海盗旗帜语义。</summary>
        Pirate = 9,
        /// <summary>中立或未归类旗帜。</summary>
        Neutral = 10,
    }

    /// <summary>船只运行状态。具体航程和锚泊状态由 D21 后续 owner 细化。</summary>
    public enum D21ShipOperationalState
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>位于港口但未表达锚泊细节。</summary>
        InPort = 1,
        /// <summary>船侧锚泊中。</summary>
        Anchored = 2,
        /// <summary>船侧已系泊。</summary>
        Moored = 3,
        /// <summary>正在航行。</summary>
        Underway = 4,
        /// <summary>没有有效动力而漂泊。</summary>
        Drifting = 5,
        /// <summary>因船况暂时不能航行。</summary>
        Disabled = 6,
        /// <summary>船只已丢失或沉没结算。</summary>
        Lost = 7,
        /// <summary>已退役，不再接受新航程。</summary>
        Decommissioned = 8,
    }

    /// <summary>船舶登记记录状态；弃用不是删除，旧引用仍需可迁移/审计。</summary>
    public enum D21ShipRegistryStatus
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>可被新命令引用。</summary>
        Active = 1,
        /// <summary>登记暂时冻结。</summary>
        Suspended = 2,
        /// <summary>只允许旧引用迁移，不允许新引用。</summary>
        Deprecated = 3,
        /// <summary>保留为历史记录。</summary>
        Archived = 4,
    }

    /// <summary>旧值映射的值域，避免把内容 ID 和船型 ID 混为一谈。</summary>
    public enum D21ShipIdentityValueKind
    {
        /// <summary>船型引用。</summary>
        ShipType = 0,
        /// <summary>内容定义引用。</summary>
        ContentId = 1,
        /// <summary>船实例引用。</summary>
        ShipInstance = 2,
    }

    /// <summary>身份校验失败的稳定原因码。</summary>
    public enum D21ShipIdentityValidationCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>身份对象缺失。</summary>
        MissingIdentity = 1,
        /// <summary>版本不受支持。</summary>
        SchemaVersionMismatch = 2,
        /// <summary>船 ID 缺失。</summary>
        MissingShipId = 3,
        /// <summary>船 ID 格式非法。</summary>
        InvalidShipId = 4,
        /// <summary>船型 ID 缺失。</summary>
        MissingShipTypeId = 5,
        /// <summary>船型 ID 格式非法。</summary>
        InvalidShipTypeId = 6,
        /// <summary>船名缺失。</summary>
        MissingShipName = 7,
        /// <summary>船名超出允许长度。</summary>
        InvalidShipName = 8,
        /// <summary>所有者 ID 缺失。</summary>
        MissingOwnerId = 9,
        /// <summary>所有者 ID 格式非法。</summary>
        InvalidOwnerId = 10,
        /// <summary>所有权枚举非法。</summary>
        UnsupportedOwnership = 11,
        /// <summary>船籍字段非法。</summary>
        InvalidFlag = 12,
        /// <summary>母港引用非法。</summary>
        InvalidHomePort = 13,
        /// <summary>建造港引用非法。</summary>
        InvalidBuildPort = 14,
        /// <summary>建造年份越界。</summary>
        InvalidBuildYear = 15,
        /// <summary>运行状态枚举非法。</summary>
        UnsupportedOperationalState = 16,
        /// <summary>登记状态枚举非法。</summary>
        UnsupportedRegistryStatus = 17,
        /// <summary>旧引用格式非法。</summary>
        InvalidLegacyReference = 18,
        /// <summary>存档修订号非法。</summary>
        InvalidSaveRevision = 19,
        /// <summary>事件引用格式非法。</summary>
        InvalidEventReference = 20,
    }

    /// <summary>船型目录的稳定键。具体属性由 ShipCatalog/内容 owner 提供。</summary>
    public static class D21ShipTypeIds
    {
        /// <summary>独木舟。</summary>
        public const string Canoe = "canoe";
        /// <summary>大型独木舟。</summary>
        public const string Piragua = "piragua";
        /// <summary>小渔船。</summary>
        public const string FishingBoat = "fishing_boat";
        /// <summary>小型平底艇。</summary>
        public const string Shallop = "shallop";
        /// <summary>长艇。</summary>
        public const string Longboat = "longboat";
        /// <summary>小型快艇。</summary>
        public const string Pinnace = "pinnace";
        /// <summary>单桅纵帆船。</summary>
        public const string Sloop = "sloop";
        /// <summary>双桅纵帆船。</summary>
        public const string Schooner = "schooner";
        /// <summary>双桅小型船。</summary>
        public const string Ketch = "ketch";
        /// <summary>双桅快帆船。</summary>
        public const string Brigantine = "brigantine";
        /// <summary>雪帆船。</summary>
        public const string Snow = "snow";
        /// <summary>小型横帆船。</summary>
        public const string Barque = "barque";
        /// <summary>荷式货船。</summary>
        public const string Flute = "flute";
        /// <summary>平底货船。</summary>
        public const string Pink = "pink";
        /// <summary>商用双桅船。</summary>
        public const string MerchantBrig = "merchant_brig";
        /// <summary>武装商船。</summary>
        public const string ArmedMerchantman = "armed_merchantman";
        /// <summary>私掠双桅船。</summary>
        public const string PrivateerBrig = "privateer_brig";
        /// <summary>轻型盖伦。</summary>
        public const string LightGalleon = "light_galleon";
        /// <summary>军用盖伦。</summary>
        public const string WarGalleon = "war_galleon";
        /// <summary>补给支援船。</summary>
        public const string SupplyTender = "supply_tender";

        /// <summary>D4/D7 旧运行时船型键；不是第二艘船。</summary>
        /// <summary>D4/D7 的旧小渔船类型键。</summary>
        public const string LegacySmallFishing = "small_fishing";

        /// <summary>D9 样板内容 ID；它是内容引用，不是旧运行时类型的同义字段。</summary>
        /// <summary>D9 样板小船的内容定义键。</summary>
        public const string MoonwakeContentId = "ship_moonwake";

        private static readonly ReadOnlyCollection<string> _canonicalIds =
            new ReadOnlyCollection<string>(new List<string>
            {
                Canoe, Piragua, FishingBoat, Shallop, Longboat, Pinnace, Sloop,
                Schooner, Ketch, Brigantine, Snow, Barque, Flute, Pink,
                MerchantBrig, ArmedMerchantman, PrivateerBrig, LightGalleon,
                WarGalleon, SupplyTender,
            });

        /// <summary>返回防御性船型 ID 列表，调用方不能修改目录。</summary>
        public static IReadOnlyList<string> CanonicalIds
        {
            get { return new ReadOnlyCollection<string>(new List<string>(_canonicalIds)); }
        }

        /// <summary>判断 ID 是否属于设计冻结的 20 个船型之一。</summary>
        public static bool IsCanonical(string shipTypeId)
        {
            if (string.IsNullOrEmpty(shipTypeId)) return false;
            for (int i = 0; i < _canonicalIds.Count; i++)
            {
                if (string.Equals(_canonicalIds[i], shipTypeId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }

    /// <summary>显式的旧 ID 映射记录。映射自身不可变，不能作为新引用写回。</summary>
    [Serializable]
    public sealed class D21ShipDeprecatedIdMapping
    {
        /// <summary>历史引用值。</summary>
        public string LegacyId = string.Empty;
        /// <summary>迁移后的 canonical 引用值。</summary>
        public string CanonicalId = string.Empty;
        /// <summary>引用所属值域。</summary>
        public D21ShipIdentityValueKind ValueKind;
        /// <summary>稳定迁移原因码。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>是否允许新数据继续使用旧值；当前映射默认为 false。</summary>
        public bool AllowNewReference;

        /// <summary>供序列化器创建空记录。</summary>
        public D21ShipDeprecatedIdMapping() { }

        /// <summary>创建一条只读旧值映射记录。</summary>
        public D21ShipDeprecatedIdMapping(string legacyId, string canonicalId,
            D21ShipIdentityValueKind valueKind, string reasonCode)
        {
            LegacyId = legacyId ?? string.Empty;
            CanonicalId = canonicalId ?? string.Empty;
            ValueKind = valueKind;
            ReasonCode = reasonCode ?? string.Empty;
            AllowNewReference = false;
        }

        /// <summary>返回防御性副本。</summary>
        public D21ShipDeprecatedIdMapping Clone()
        {
            return new D21ShipDeprecatedIdMapping(LegacyId, CanonicalId, ValueKind, ReasonCode)
            {
                AllowNewReference = AllowNewReference,
            };
        }
    }

    /// <summary>旧身份迁移结果；未匹配旧值时不会偷偷改写输入。</summary>
    [Serializable]
    public sealed class D21ShipIdentityMigrationResult
    {
        /// <summary>迁移是否成功。</summary>
        public bool Succeeded;
        /// <summary>输入是否命中弃用映射。</summary>
        public bool WasDeprecated;
        /// <summary>是否需要人工/上游审计确认。</summary>
        public bool RequiresReview;
        /// <summary>原始引用值。</summary>
        public string LegacyId = string.Empty;
        /// <summary>迁移后的引用值。</summary>
        public string CanonicalId = string.Empty;
        /// <summary>引用所属值域。</summary>
        public D21ShipIdentityValueKind ValueKind;
        /// <summary>稳定迁移结果原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>返回未变化的当前值结果。</summary>
        public static D21ShipIdentityMigrationResult Unchanged(string value,
            D21ShipIdentityValueKind valueKind)
        {
            return new D21ShipIdentityMigrationResult
            {
                Succeeded = true,
                WasDeprecated = false,
                RequiresReview = false,
                LegacyId = value ?? string.Empty,
                CanonicalId = value ?? string.Empty,
                ValueKind = valueKind,
                ReasonCode = "identity_value_current",
            };
        }

        /// <summary>返回保留原输入的拒绝结果。</summary>
        public static D21ShipIdentityMigrationResult Rejected(string value,
            D21ShipIdentityValueKind valueKind, string reasonCode)
        {
            return new D21ShipIdentityMigrationResult
            {
                Succeeded = false,
                WasDeprecated = false,
                RequiresReview = true,
                LegacyId = value ?? string.Empty,
                CanonicalId = string.Empty,
                ValueKind = valueKind,
                ReasonCode = reasonCode ?? "identity_value_rejected",
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D21ShipIdentityMigrationResult Clone()
        {
            return new D21ShipIdentityMigrationResult
            {
                Succeeded = Succeeded,
                WasDeprecated = WasDeprecated,
                RequiresReview = RequiresReview,
                LegacyId = LegacyId ?? string.Empty,
                CanonicalId = CanonicalId ?? string.Empty,
                ValueKind = ValueKind,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>身份校验结果，不以异常作为正常业务分支。</summary>
    [Serializable]
    public sealed class D21ShipIdentityValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;
        /// <summary>结构化校验码。</summary>
        public D21ShipIdentityValidationCode Code;
        /// <summary>稳定机器原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D21ShipIdentityValidationResult Success()
        {
            return new D21ShipIdentityValidationResult
            {
                Valid = true,
                Code = D21ShipIdentityValidationCode.None,
                ReasonCode = "ship_identity_valid",
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D21ShipIdentityValidationResult Failure(
            D21ShipIdentityValidationCode code, string reasonCode)
        {
            return new D21ShipIdentityValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? "ship_identity_invalid",
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D21ShipIdentityValidationResult Clone()
        {
            return new D21ShipIdentityValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>船只身份契约常量、稳定键检查、迁移和版本校验入口。</summary>
    public static class D21ShipIdentityContracts
    {
        /// <summary>当前身份投影版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>身份事实 owner 标识。</summary>
        public const string OwnerSystem = "D21ShipIdentity";
        /// <summary>安全未分配所有者引用。</summary>
        public const string DefaultOwnerId = "owner.unassigned";
        /// <summary>安全未登记旗帜引用。</summary>
        public const string DefaultFlagId = "flag.unregistered";
        /// <summary>缺失母港时的显式引用。</summary>
        public const string DefaultHomePortId = "port.unknown";
        /// <summary>缺失建造港时的显式引用。</summary>
        public const string DefaultBuildPortId = "port.unknown";
        /// <summary>船实例 ID 最大长度。</summary>
        public const int ShipIdMaxLength = 128;
        /// <summary>船名最大长度。</summary>
        public const int ShipNameMaxLength = 128;
        /// <summary>一般逻辑引用最大长度。</summary>
        public const int ReferenceMaxLength = 160;
        /// <summary>历史年份的最低允许值。</summary>
        public const int MinimumHistoricalBuildYear = 1000;

        private static readonly ReadOnlyCollection<D21ShipDeprecatedIdMapping> _deprecatedMappings =
            new ReadOnlyCollection<D21ShipDeprecatedIdMapping>(new List<D21ShipDeprecatedIdMapping>
            {
                new D21ShipDeprecatedIdMapping(
                    D21ShipTypeIds.LegacySmallFishing,
                    D21ShipTypeIds.FishingBoat,
                    D21ShipIdentityValueKind.ShipType,
                    "legacy_d4_small_fishing_alias"),
            });

        /// <summary>返回旧映射的防御性副本。</summary>
        public static IReadOnlyList<D21ShipDeprecatedIdMapping> DeprecatedMappings
        {
            get
            {
                var copy = new List<D21ShipDeprecatedIdMapping>();
                for (int i = 0; i < _deprecatedMappings.Count; i++)
                    copy.Add(_deprecatedMappings[i].Clone());
                return new ReadOnlyCollection<D21ShipDeprecatedIdMapping>(copy);
            }
        }

        /// <summary>判断字符串是否为不含路径/空白的稳定逻辑 ID。</summary>
        /// <summary>判断字符串是否为不含路径/空白的稳定逻辑 ID。</summary>
        public static bool IsStableId(string value, int maxLength = ReferenceMaxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength) return false;
            if (!string.Equals(value.Trim(), value, StringComparison.Ordinal)) return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!letter && !digit && !separator) return false;
            }
            return true;
        }

        /// <summary>只去除首尾空白；非法值返回空，不猜测缺失前缀。</summary>
        /// <summary>只去除首尾空白并验证 ID；不猜测前缀或大小写。</summary>
        public static string NormalizeId(string value, int maxLength = ReferenceMaxLength)
        {
            string normalized = (value ?? string.Empty).Trim();
            return IsStableId(normalized, maxLength) ? normalized : string.Empty;
        }

        /// <summary>判断所有权枚举是否受当前契约支持。</summary>
        public static bool IsSupportedOwnership(D21ShipOwnershipKind value)
        {
            return value >= D21ShipOwnershipKind.Unassigned && value <= D21ShipOwnershipKind.Lost;
        }

        /// <summary>判断船籍枚举是否受当前契约支持。</summary>
        public static bool IsSupportedFlag(D21ShipFlagKind value)
        {
            return value >= D21ShipFlagKind.Unregistered && value <= D21ShipFlagKind.Neutral;
        }

        /// <summary>判断运行状态枚举是否受当前契约支持。</summary>
        public static bool IsSupportedOperationalState(D21ShipOperationalState value)
        {
            return value >= D21ShipOperationalState.InPort &&
                   value <= D21ShipOperationalState.Decommissioned;
        }

        /// <summary>判断登记状态枚举是否受当前契约支持。</summary>
        public static bool IsSupportedRegistryStatus(D21ShipRegistryStatus value)
        {
            return value >= D21ShipRegistryStatus.Active &&
                   value <= D21ShipRegistryStatus.Archived;
        }

        /// <summary>映射已登记的 D4/D7 旧船型键；未知值保持拒绝而不是静默改名。</summary>
        public static bool TryMapDeprecatedShipTypeId(string legacyId, out string canonicalId)
        {
            canonicalId = string.Empty;
            if (!IsStableId(legacyId, ReferenceMaxLength)) return false;
            for (int i = 0; i < _deprecatedMappings.Count; i++)
            {
                D21ShipDeprecatedIdMapping mapping = _deprecatedMappings[i];
                if (mapping.ValueKind == D21ShipIdentityValueKind.ShipType &&
                    string.Equals(mapping.LegacyId, legacyId, StringComparison.Ordinal))
                {
                    canonicalId = mapping.CanonicalId;
                    return true;
                }
            }
            return false;
        }

        /// <summary>迁移任意身份引用；当前值原样返回，旧值映射为只读审计结果。</summary>
        public static D21ShipIdentityMigrationResult MigrateReference(
            string value, D21ShipIdentityValueKind valueKind)
        {
            if (!IsStableId(value, ReferenceMaxLength))
                return D21ShipIdentityMigrationResult.Rejected(value, valueKind, "identity_reference_invalid");

            for (int i = 0; i < _deprecatedMappings.Count; i++)
            {
                D21ShipDeprecatedIdMapping mapping = _deprecatedMappings[i];
                if (mapping.ValueKind == valueKind &&
                    string.Equals(mapping.LegacyId, value, StringComparison.Ordinal))
                {
                    return new D21ShipIdentityMigrationResult
                    {
                        Succeeded = true,
                        WasDeprecated = true,
                        RequiresReview = true,
                        LegacyId = mapping.LegacyId,
                        CanonicalId = mapping.CanonicalId,
                        ValueKind = valueKind,
                        ReasonCode = mapping.ReasonCode,
                    };
                }
            }

            return D21ShipIdentityMigrationResult.Unchanged(value, valueKind);
        }

        /// <summary>创建新船的安全默认身份；owner 为空时显式落到未分配状态。</summary>
        public static D21ShipIdentity CreateDefault(string shipId, string shipTypeId,
            string ownerId = "", string shipName = "")
        {
            string safeOwner = string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId;
            return new D21ShipIdentity
            {
                SchemaVersion = CurrentSchemaVersion,
                ShipId = shipId ?? string.Empty,
                ShipTypeId = shipTypeId ?? string.Empty,
                ShipTypeContentId = string.Empty,
                ShipName = string.IsNullOrWhiteSpace(shipName) ? shipTypeId ?? string.Empty : shipName,
                OwnerId = safeOwner,
                Ownership = string.IsNullOrWhiteSpace(ownerId)
                    ? D21ShipOwnershipKind.Unassigned
                    : D21ShipOwnershipKind.Private,
                FlagId = DefaultFlagId,
                FlagKind = D21ShipFlagKind.Unregistered,
                HomePortId = DefaultHomePortId,
                BuildPortId = DefaultBuildPortId,
                BuildYear = 0,
                OperationalState = D21ShipOperationalState.InPort,
                RegistryStatus = D21ShipRegistryStatus.Active,
                LegacyShipId = string.Empty,
                LegacyShipTypeId = string.Empty,
                SaveRevision = 0,
                LastEventId = string.Empty,
            };
        }

        /// <summary>校验身份投影；船型属性存在性由外部 ShipCatalog 继续校验。</summary>
        public static D21ShipIdentityValidationResult Validate(D21ShipIdentity identity)
        {
            if (identity == null)
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.MissingIdentity, "ship_identity_missing");
            if (identity.SchemaVersion != CurrentSchemaVersion)
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.SchemaVersionMismatch, "ship_identity_schema_version_mismatch");
            if (!IsStableId(identity.ShipId, ShipIdMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    string.IsNullOrEmpty(identity.ShipId)
                        ? D21ShipIdentityValidationCode.MissingShipId
                        : D21ShipIdentityValidationCode.InvalidShipId,
                    "ship_identity_ship_id_invalid");
            if (!IsStableId(identity.ShipTypeId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    string.IsNullOrEmpty(identity.ShipTypeId)
                        ? D21ShipIdentityValidationCode.MissingShipTypeId
                        : D21ShipIdentityValidationCode.InvalidShipTypeId,
                    "ship_identity_ship_type_id_invalid");
            if (string.IsNullOrWhiteSpace(identity.ShipName) ||
                identity.ShipName.Length > ShipNameMaxLength)
                return D21ShipIdentityValidationResult.Failure(
                    string.IsNullOrWhiteSpace(identity.ShipName)
                        ? D21ShipIdentityValidationCode.MissingShipName
                        : D21ShipIdentityValidationCode.InvalidShipName,
                    "ship_identity_name_invalid");
            if (!IsStableId(identity.OwnerId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    string.IsNullOrEmpty(identity.OwnerId)
                        ? D21ShipIdentityValidationCode.MissingOwnerId
                        : D21ShipIdentityValidationCode.InvalidOwnerId,
                    "ship_identity_owner_id_invalid");
            if (!IsSupportedOwnership(identity.Ownership))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.UnsupportedOwnership, "ship_identity_ownership_invalid");
            if (!IsStableId(identity.FlagId, ReferenceMaxLength) ||
                !IsSupportedFlag(identity.FlagKind))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidFlag, "ship_identity_flag_invalid");
            if (!IsStableId(identity.HomePortId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidHomePort, "ship_identity_home_port_invalid");
            if (!IsStableId(identity.BuildPortId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidBuildPort, "ship_identity_build_port_invalid");
            if (identity.BuildYear != 0 &&
                (identity.BuildYear < MinimumHistoricalBuildYear || identity.BuildYear > 9999))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidBuildYear, "ship_identity_build_year_invalid");
            if (!IsSupportedOperationalState(identity.OperationalState))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.UnsupportedOperationalState,
                    "ship_identity_operational_state_invalid");
            if (!IsSupportedRegistryStatus(identity.RegistryStatus))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.UnsupportedRegistryStatus,
                    "ship_identity_registry_status_invalid");
            if (!string.IsNullOrEmpty(identity.LegacyShipId) &&
                !IsStableId(identity.LegacyShipId, ShipIdMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidLegacyReference,
                    "ship_identity_legacy_ship_id_invalid");
            if (!string.IsNullOrEmpty(identity.LegacyShipTypeId) &&
                !IsStableId(identity.LegacyShipTypeId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidLegacyReference,
                    "ship_identity_legacy_type_id_invalid");
            if (identity.SaveRevision < 0)
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidSaveRevision,
                    "ship_identity_save_revision_invalid");
            if (!string.IsNullOrEmpty(identity.LastEventId) &&
                !IsStableId(identity.LastEventId, ReferenceMaxLength))
                return D21ShipIdentityValidationResult.Failure(
                    D21ShipIdentityValidationCode.InvalidEventReference,
                    "ship_identity_last_event_id_invalid");
            return D21ShipIdentityValidationResult.Success();
        }
    }

    /// <summary>船只身份的版本化只读投影；所有字段必须经 <see cref="Validate"/> 校验。</summary>
    [Serializable]
    public sealed class D21ShipIdentity
    {
        /// <summary>身份投影版本。</summary>
        public int SchemaVersion = D21ShipIdentityContracts.CurrentSchemaVersion;
        /// <summary>船只实例稳定 ID。</summary>
        public string ShipId = string.Empty;
        /// <summary>设计船型稳定 ID。</summary>
        public string ShipTypeId = string.Empty;
        /// <summary>可选的 D18/D9 内容定义引用。</summary>
        public string ShipTypeContentId = string.Empty;
        /// <summary>玩家可读船名；不承载状态或资源路径。</summary>
        public string ShipName = string.Empty;
        /// <summary>所有者稳定 ID。</summary>
        public string OwnerId = D21ShipIdentityContracts.DefaultOwnerId;
        /// <summary>所有权语义；具体权利由 Property/Legal owner 裁决。</summary>
        public D21ShipOwnershipKind Ownership = D21ShipOwnershipKind.Unassigned;
        /// <summary>船籍或旗帜的稳定引用。</summary>
        public string FlagId = D21ShipIdentityContracts.DefaultFlagId;
        /// <summary>旗帜语义分类。</summary>
        public D21ShipFlagKind FlagKind = D21ShipFlagKind.Unregistered;
        /// <summary>母港稳定 ID。</summary>
        public string HomePortId = D21ShipIdentityContracts.DefaultHomePortId;
        /// <summary>建造港稳定 ID。</summary>
        public string BuildPortId = D21ShipIdentityContracts.DefaultBuildPortId;
        /// <summary>建造年份；0 表示旧档缺失且待补齐。</summary>
        public int BuildYear;
        /// <summary>当前运行状态。</summary>
        public D21ShipOperationalState OperationalState = D21ShipOperationalState.InPort;
        /// <summary>当前登记状态。</summary>
        public D21ShipRegistryStatus RegistryStatus = D21ShipRegistryStatus.Active;
        /// <summary>旧存档船实例 ID；仅用于迁移追溯。</summary>
        public string LegacyShipId = string.Empty;
        /// <summary>旧存档船型 ID；仅用于迁移追溯。</summary>
        public string LegacyShipTypeId = string.Empty;
        /// <summary>生成此投影时的逻辑存档修订号。</summary>
        public long SaveRevision;
        /// <summary>最后一个已应用事实事件 ID。</summary>
        public string LastEventId = string.Empty;

        /// <summary>返回深拷贝，避免调用方修改保存投影。</summary>
        public D21ShipIdentity Clone()
        {
            return new D21ShipIdentity
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId ?? string.Empty,
                ShipTypeId = ShipTypeId ?? string.Empty,
                ShipTypeContentId = ShipTypeContentId ?? string.Empty,
                ShipName = ShipName ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                Ownership = Ownership,
                FlagId = FlagId ?? string.Empty,
                FlagKind = FlagKind,
                HomePortId = HomePortId ?? string.Empty,
                BuildPortId = BuildPortId ?? string.Empty,
                BuildYear = BuildYear,
                OperationalState = OperationalState,
                RegistryStatus = RegistryStatus,
                LegacyShipId = LegacyShipId ?? string.Empty,
                LegacyShipTypeId = LegacyShipTypeId ?? string.Empty,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId ?? string.Empty,
            };
        }

        /// <summary>返回验证结果副本，调用方不能通过结果改变身份。</summary>
        public D21ShipIdentityValidationResult Validate()
        {
            return D21ShipIdentityContracts.Validate(this);
        }

        /// <summary>为迁移审计和幂等比较提供稳定文本指纹，不使用运行时哈希。</summary>
        public string Fingerprint()
        {
            return (ShipId ?? string.Empty) + "|" +
                   (ShipTypeId ?? string.Empty) + "|" +
                   (ShipTypeContentId ?? string.Empty) + "|" +
                   (ShipName ?? string.Empty) + "|" +
                   (OwnerId ?? string.Empty) + "|" + Ownership + "|" +
                   (FlagId ?? string.Empty) + "|" + FlagKind + "|" +
                   (HomePortId ?? string.Empty) + "|" +
                   (BuildPortId ?? string.Empty) + "|" +
                   BuildYear.ToString(CultureInfo.InvariantCulture) + "|" +
                   OperationalState + "|" + RegistryStatus + "|" +
                   (LegacyShipId ?? string.Empty) + "|" +
                   (LegacyShipTypeId ?? string.Empty) + "|" +
                   SaveRevision.ToString(CultureInfo.InvariantCulture) + "|" +
                   (LastEventId ?? string.Empty);
        }
    }
}
