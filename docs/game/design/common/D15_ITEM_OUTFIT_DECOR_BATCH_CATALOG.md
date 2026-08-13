# D15-A05* 道具、服装与装饰批量目录

任务：`D15-A05*`  
日期：2026-08-11  
状态：批量目录与共享可交互占位资产已完成；500 条本批内容可扫描、可加载并可交给既有 owner。  
质量口径：本批统一为 `Q2`、`production_stage=integrated`、`asset_state=placeholder`。这表示资源已经有共享模型/材质/碰撞或明确的穿戴层无碰撞理由，并接入了功能/交互入口；不把共享占位误报为最终美术完成。

## 1. 数量结论

本批通过 `production_batch=D15-A05*` 过滤后正好 500 条：

| 批次角色 | 数量 | 内容来源 |
|---|---:|---|
| 功能道具 | 200 | `item_` 155 条 + `outfit_` 45 条 |
| 装饰对象 | 300 | `item_` 180 条 + `outfit_` 120 条 |
| 合计 | **500** | 唯一 ID、唯一测试 ID |

工作区完整 Content Registry 当前包含其他 D15 批次的并行内容；全量扫描结果为 2,812 条、13 个前缀。本文件和专用检查器只统计本批 500 条，避免把其他批次数量混入 D15-A05 验收。

## 2. 分类覆盖

### 功能道具（200）

| 分类 | 数量 | ID 范围/代表 | 所属系统入口 |
|---|---:|---|---|
| 工具 | 30 | `item_tide_rule`、`item_archive_gloves` | `D3InventorySystem/functional_tool_definition` |
| 武器与武器物资 | 25 | `item_boarding_cutlass`、`item_safety_fuse` | `D14SpecialEquipmentSystem/definition_or_item_catalog` |
| 渔具 | 25 | `item_shore_rod_bamboo`、`item_net_float_cork` | `D11FishingHarvestSystem/gear_content_definition` |
| 种子 | 25 | `item_seed` 语义条目，如 `item_plantain`、`item_sorghum` | `D13FarmingSystem/seed_content_definition` |
| 食物 | 25 | `item_salt_biscuit`、`item_conch_chowder` | `D3InventorySystem/food_content_definition` |
| 鱼获处理 | 25 | `item_mackerel_salted`、`item_fish_bone_glue` | `D11FishingHarvestSystem/processing_content_definition` |
| 服装 | 15 | `outfit_rain_smock`、`outfit_storm_cape` | `D10AppearanceSystem/clothing_slot` |
| 发型 | 15 | `outfit_braided_deckhair`、`outfit_coiled_hair` | `D10AppearanceSystem/hair_slot` |
| 饰品 | 15 | `outfit_ropework_bracelet`、`outfit_watch_whistle` | `D10AppearanceSystem/accessory_slot` |

种子条目的运行时语义仍由 D13 Farming/Inventory owner 读取 ID；目录不把发芽时间、产量、价格或收获奖励写进条目。

### 装饰对象（300）

| 分类 | 数量 | ID 结构 | 交互入口 |
|---|---:|---|---|
| 家具 | 90 | 30 个家具原型 × `plain`/`painted`/`weathered` | `D3InventorySystem/inspect_or_place` |
| 船舱装饰 | 60 | 20 个船舱对象 × `shipboard`/`harbor`/`community` | `D3InventorySystem/inspect_or_place` |
| 收藏品 | 30 | `item_` 收藏定义 | `D12CollectionAtlasSystem/collection_entry` |
| 装饰服装 | 50 | 25 个服装对象 × `plain`/`festival` | `AppearanceSystem/try_on_or_equip` |
| 装饰发型 | 30 | 15 个发型对象 × `plain`/`festival` | `AppearanceSystem/try_on_or_equip` |
| 装饰饰品 | 40 | 20 个饰品对象 × `plain`/`festival` | `AppearanceSystem/try_on_or_equip` |

装饰对象不是空壳：家具/船舱对象有可放置入口和共享 BoxCollider；服装/发型/饰品有外观槽位入口，并明确复用角色身体碰撞，不为穿戴层复制物理碰撞。

## 3. 目录字段和唯一 ID

每个新条目位于：

```text
game/Assets/Resources/ContentRegistry/entries/item/<id>.json
game/Assets/Resources/ContentRegistry/entries/outfit/<id>.json
```

所有条目保留 D8 必填字段：

```text
id, schema_version, name, display_name_key, category, tags, description,
gameplay_role, valid_regions, valid_habitats, valid_weather, dependencies,
references, asset_refs, source_refs, source_note, source, license,
historical_tag, phase, status, owner_system, test_id, reviewer
```

D15 扩展字段统一为：

```text
production_batch, representative_sample, production_stage, batch_role,
content_category, behavior_profile, asset_state, quality_level, model_ref,
material_ref, collision_ref, lod_profile, interaction_entry, function_entry,
test_cases, historical_review, review_evidence_refs, content_warning_ids,
skip_allowed, integration
```

所有 ID 使用已冻结前缀和小写 snake_case。功能与装饰通过 `batch_role` 区分；具体覆盖分类写入 `content_category`。`test_id` 使用可追踪的 `D8-A02-D15-A05-*` 形式，避免与既有目录测试入口冲突。

## 4. 资产与加载接入

本批新增两个授权资源引用：

| 资源 ID | 实际资源 | 材质/碰撞 | 用途 |
|---|---|---|---|
| `asset_d15_item_prop_placeholder` | `game/Assets/Prefabs/D15/d15_item_prop_placeholder.prefab` | `ph_mat_dark_wood.mat` + `BoxCollider` | 所有 item 家具、船舱、工具、食物等共享可替换占位 |
| `asset_d15_outfit_placeholder` | 已有 `game/Assets/Prefabs/Placeholder/ph_crew_male.prefab` | `ph_mat_crew_cloth.mat`；穿戴层使用角色身体碰撞 | 所有服装、发型、饰品共享外观层占位 |

`d15_item_prop_placeholder.prefab` 是项目自有 Unity 内置 Cube，包含 `MeshFilter`、`MeshRenderer`、项目材质和 `BoxCollider`；它不是无材质/无碰撞的空对象。服装资源引用既有 Quaternius CC0 船员模型，授权继续指向 `PH_QUATERNIUS_LICENSE.txt`；穿戴层声明 `none:wearable_skinned_layer_uses_character_body_collision`，避免把服装当独立刚体。

ContentRegistryReader 已补齐 D15 扩展字段，包括生产阶段、批次角色、资源状态、模型/材质/碰撞、交互/功能入口、测试、历史审查和 owner 接管元数据。读取器只读定义，不写 Inventory、Appearance、经济、奖励或运行时状态。

## 5. 功能数据与唯一写者

目录只提供不可变定义和接管入口，实际运行时写者不变：

| 内容 | 读取/接管入口 | 运行时唯一写者 | 本批不写入 |
|---|---|---|---|
| 工具、食物、鱼获处理 | `D3InventorySystem` / D11 processing | Inventory/Cargo/Ecology/Processing owner | 数量、消耗、价格、奖励 |
| 武器与武器物资 | D14 SpecialEquipment/Combat | D14 装备、Combat、Ship、Legal owner | 伤害、强度、耐久、法律结果 |
| 渔具 | `D11FishingHarvestSystem` | Ecology/Harvest/Inventory owner | 鱼群数量、捕捞结果、压力 |
| 种子 | `D13FarmingSystem` | Farming/Ecology/Inventory owner | 季节数值、产量、价格、作物状态 |
| 服装、发型、饰品 | `D10AppearanceSystem` | AppearanceSystem；金币由 EconomySystem | 价格、耐久、身份法律结果 |
| 家具、船舱装饰、收藏品 | D3 placement / D12 CollectionAtlas | Inventory/Building/Collection owner | 产权、摆放实例状态、成就奖励 |

每个功能条目都有 `function_entry`、`logic_ref`、`integration.system` 和 `integration.canonical_write_owner`。资产 Agent 不在 JSON 或 Systems 中写价格、伤害、奖励、耐久、重量、体积或装备裁决。

## 6. 历史、授权与敏感内容

- 所有条目均有 `source`、`source_refs`、`source_note` 和商业授权 `license.commercial_use=true`。
- 武器条目标记 `historical_tag=historical`、`historical_review=ApprovedWithWarning` 和 `content_warning_historical_weapon_context`；它们允许跳过表现，但不能绕过 D14 的法律/装备裁决。
- 服装、家具和船舱内容使用 `historical` 或 `inferred` 标签，引用 D10/D15 设计证据；收藏品中明确标记 `fictional` 的为项目自有收藏对象，不冒充史实文物。
- 本批没有使用来源不明的外部模型、材质或网络下载；新增道具占位资源为项目自有，服装占位资源复用已登记 CC0 资源。

## 7. 验收入口

目录扫描：

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a05_content_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools/tests/test_d15_a05_content_check.py
```

Unity EditMode 测试：

```text
D15ItemOutfitDecorTests.D15A05Batch_Loads_AndHasOwnerInteractionAndAssets
```

专用检查器会加载本批全部 JSON，校验 200/300 数量、12 类覆盖、D8 字段、唯一 ID/测试 ID、授权、模型/材质/碰撞、功能/交互入口、owner 一致性、历史审查和禁止的运行时裁决字段，并对 50 条（10%）做加载/交互字段抽样。

当前已验证：Content Registry 全量 `2,812 entries / 13 prefixes` 通过；D15-A05 专用检查 `functional=200 decorative=300 total=500`，分类覆盖完整，50 条抽样通过；Python 单测 `1/1` 通过。Unity EditMode 测试已登记；实际 Unity Editor 执行留给当前工程的批量 QA 门禁。
