// D14-A03 代表性特殊武器/船体模块目录。
// 这里仅登记不可变定义；运行时装备数量、耐久和装配状态由
// D14SpecialEquipmentSystem 与所属 authority 裁决。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D14
{
    public sealed class D14SpecialEquipmentCatalog
    {
        private readonly Dictionary<string, D14SpecialEquipmentDefinition> _definitions =
            new Dictionary<string, D14SpecialEquipmentDefinition>(StringComparer.Ordinal);

        public D14SpecialEquipmentCatalog()
        {
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_special_weapon_moonwake_cutlass", DisplayName = "月醒号礼仪短剑",
                Kind = D14EquipmentKind.HistoricalWeapon, Slot = D14EquipmentSlot.DeckArmament,
                Unique = true, StrengthCap = 72, MaxDurability = 100, WearPerUse = 3,
                SourceRef = "docs/game/design/common/D12_COLLECTION_ATLAS.md",
                AuthorizationRef = "armory_authorization_moonwake", LicenseRef = "harbor_armory_license_moonwake",
                AcquisitionOwnerSystem = "CollectionAtlasSystem", AuthorizationOwnerSystem = "HarborAuthority",
                HistoricalTag = "fictional", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-001",
                AllowedShipTypeIds = new List<string> { "ship_moonwake", "sloop" },
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_special_weapon_royal_gun_carriage", DisplayName = "加固旋转炮架",
                Kind = D14EquipmentKind.SpecialGunMount, Slot = D14EquipmentSlot.GunMount,
                Unique = true, StrengthCap = 58, MaxDurability = 100, WearPerUse = 5,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "shipyard_special_mount_permit", LicenseRef = "shipyard_service_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "inferred", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-002",
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_firearm_matchlock_carbine", DisplayName = "火绳短枪",
                Kind = D14EquipmentKind.Firearm, Slot = D14EquipmentSlot.DeckArmament,
                Unique = true, StrengthCap = 44, MaxDurability = 80, WearPerUse = 7,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "armory_firearm_register", LicenseRef = "armory_firearm_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "historical", ReviewStatus = D14EquipmentReviewStatus.ApprovedWithWarning,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-003",
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_signal_lantern_amber", DisplayName = "琥珀信号灯",
                Kind = D14EquipmentKind.SignalDevice, Slot = D14EquipmentSlot.Signal,
                Unique = true, StrengthCap = 25, MaxDurability = 90, WearPerUse = 2,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "signal_code_registry", LicenseRef = "harbor_signal_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "inferred", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-004",
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_tool_rescue_grapnel", DisplayName = "捕缆救援钩",
                Kind = D14EquipmentKind.RescueTool, Slot = D14EquipmentSlot.Rescue,
                Unique = true, StrengthCap = 30, MaxDurability = 100, WearPerUse = 4,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "rescue_equipment_register", LicenseRef = "shipyard_safety_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "inferred", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-005",
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_ship_module_reinforced_keel", DisplayName = "独特加固龙骨",
                Kind = D14EquipmentKind.HullModule, Slot = D14EquipmentSlot.Hull,
                Unique = true, StrengthCap = 35, MaxDurability = 100, WearPerUse = 1,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "shipwright_module_register", LicenseRef = "shipyard_module_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "inferred", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-006",
            });
            Add(new D14SpecialEquipmentDefinition
            {
                DefinitionId = "item_ship_module_quick_release_rigging", DisplayName = "快拆索具模块",
                Kind = D14EquipmentKind.HullModule, Slot = D14EquipmentSlot.Rigging,
                Unique = true, StrengthCap = 30, MaxDurability = 100, WearPerUse = 2,
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md",
                AuthorizationRef = "shipwright_rigging_register", LicenseRef = "shipyard_module_license",
                AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner, AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner,
                HistoricalTag = "inferred", ReviewStatus = D14EquipmentReviewStatus.Approved,
                RequiresAuthorization = true, TestId = "D8-A02-D14-ITEM-007",
            });
        }

        public IReadOnlyList<D14SpecialEquipmentDefinition> All()
        {
            var result = new List<D14SpecialEquipmentDefinition>();
            foreach (var definition in _definitions.Values) result.Add(definition.Clone());
            return result;
        }

        public D14SpecialEquipmentDefinition Find(string definitionId)
        {
            D14SpecialEquipmentDefinition value;
            return definitionId != null && _definitions.TryGetValue(definitionId, out value) ? value.Clone() : null;
        }

        private void Add(D14SpecialEquipmentDefinition definition)
        {
            definition.Validate("d14.catalog");
            if (_definitions.ContainsKey(definition.DefinitionId)) throw new InvalidOperationException("Duplicate D14 equipment definition: " + definition.DefinitionId);
            _definitions.Add(definition.DefinitionId, definition.Clone());
        }
    }
}
