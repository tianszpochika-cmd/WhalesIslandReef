# D15-A04* 建筑/港口批量目录

任务：`D15-A04*`  
日期：2026-08-11  
状态：批量目录、实例绑定和目录级验收已完成；资产缺口显式保留 `placeholder`，等待 D15-S02 抽样接管。

## 1. 产出结论

| 内容 | 结果 | 口径 |
|---|---:|---|
| 功能建筑类 | 80 | 5 个 D13 代表 + 75 个新增 `building_` 定义；功能不同，不按换色计数 |
| 文化/势力/材料变体 | 180 | 每类 1 个 base 变体 + 100 个带文化、势力、材料字段的组合变体 |
| 建筑实例 | 640 | 每类 8 个唯一 `instance_`；覆盖 50 个 D15-A01 岛屿位置引用，落在冻结的 600—1,200 范围 |
| 交互点 | 1,920 | 每个实例 entry/service/inspect 三个锚点交互入口 |
| 历史警告条目 | 23 | `ApprovedWithWarning`，带 warning、上下文、跳过和 `Omitted` 表现级别 |

实例数量由 `D15BuildingPortBatchCatalog.Create()` 确定性生成，稳定 ID、变体引用、位置轮转和保存键可重复得到，不在内容目录中写入运行时库存、产权、损伤或开放结果。

## 2. 主成果

- 内容目录：`game/Assets/Resources/ContentRegistry/entries/building/`，共 80 个 D15-A04 条目，ID 与文件名一致。
- 占位资源登记：`game/Assets/Resources/ContentRegistry/entries/asset/asset_ph_building_*.json`。
- 批量目录生成器：`game/Assets/Content/D15/D15BuildingPortBatchCatalog.cs`。
- 批次清单：`game/Assets/Content/D15/D15BuildingPortBatchCatalog.json`。
- 编辑器/运行时测试：`game/Assets/Tests/EditMode/D15BuildingPortBatchCatalogTests.cs`。
- 目录验收：`tools/d15_a04_acceptance_check.py`、`tools/tests/test_d15_a04_acceptance_check.py`。
- 独立 runner：`tools/d15_a04_runner/`。

80 个功能类按功能分组如下：

| 功能组 | 数量 | 内容 ID |
|---|---:|---|
| port | 8 | `building_saint_marys_dock`、`building_small_boat_pier`、`building_cargo_wharf`、`building_berth_house`、`building_lighthouse`、`building_signal_tower`、`building_pilot_station`、`building_ferry_house` |
| trade | 11 | `building_santa_marys_north_warehouse`、`building_chandlery`、`building_sailcloth_storehouse`、`building_fish_market`、`building_general_market`、`building_market_stall`、`building_warehouse`、`building_bonded_warehouse`、`building_apothecary`、`building_mapmakers_shop`、`building_granary` |
| shipyard | 7 | `building_driftwood_ropewalk_workshop`、`building_santa_marys_shipyard`、`building_drydock`、`building_repair_shed`、`building_sail_loft`、`building_carpenter_workshop`、`building_caulker_workshop` |
| utility | 2 | `building_green_cay_water_well`、`building_water_cistern` |
| management | 8 | `building_harbor_office`、`building_customs_house`、`building_post_house`、`building_council_house`、`building_courthouse`、`building_tax_office`、`building_jail`、`building_bridge_tollhouse` |
| defense | 6 | `building_inspection_station`、`building_guardhouse`、`building_barracks`、`building_powder_magazine`、`building_fort_battery`、`building_watchtower` |
| production | 19 | `building_blacksmith_workshop`、`building_timber_yard`、`building_smokehouse`、`building_fish_drying_shed`、`building_saltworks`、`building_cooperage`、`building_mill`、`building_bakery`、`building_farmstead`、`building_estate_farm`、`building_garden_plot`、`building_livestock_pen`、`building_fish_pond`、`building_nearshore_net_cage`、`building_quarry`、`building_clay_pit`、`building_brick_kiln`、`building_lime_kiln`、`building_pottery_workshop` |
| community | 3 | `building_barter_market`、`building_community_meeting_house`、`building_ceremonial_house` |
| life | 6 | `building_cookhouse`、`building_tavern`、`building_inn`、`building_clinic`、`building_quarantine_house`、`building_bathhouse` |
| social | 5 | `building_schoolhouse`、`building_archive`、`building_chapel`、`building_shrine`、`building_cemetery` |
| ecology | 2 | `building_mangrove_nursery`、`building_salt_marsh_reserve_station` |
| exploration | 3 | `building_salvage_shed`、`building_diving_platform`、`building_cave_waystation` |

完整 ID 集合为：

`building_saint_marys_dock`、`building_santa_marys_north_warehouse`、`building_driftwood_ropewalk_workshop`、`building_santa_marys_shipyard`、`building_green_cay_water_well`、`building_small_boat_pier`、`building_cargo_wharf`、`building_berth_house`、`building_lighthouse`、`building_signal_tower`、`building_harbor_office`、`building_customs_house`、`building_inspection_station`、`building_pilot_station`、`building_ferry_house`、`building_drydock`、`building_repair_shed`、`building_sail_loft`、`building_carpenter_workshop`、`building_blacksmith_workshop`、`building_caulker_workshop`、`building_chandlery`、`building_timber_yard`、`building_sailcloth_storehouse`、`building_fish_market`、`building_general_market`、`building_barter_market`、`building_market_stall`、`building_warehouse`、`building_bonded_warehouse`、`building_smokehouse`、`building_fish_drying_shed`、`building_saltworks`、`building_cooperage`、`building_mill`、`building_bakery`、`building_cookhouse`、`building_tavern`、`building_inn`、`building_clinic`、`building_quarantine_house`、`building_apothecary`、`building_water_cistern`、`building_bathhouse`、`building_schoolhouse`、`building_archive`、`building_mapmakers_shop`、`building_post_house`、`building_chapel`、`building_community_meeting_house`、`building_shrine`、`building_cemetery`、`building_council_house`、`building_courthouse`、`building_tax_office`、`building_guardhouse`、`building_barracks`、`building_powder_magazine`、`building_fort_battery`、`building_watchtower`、`building_jail`、`building_bridge_tollhouse`、`building_farmstead`、`building_estate_farm`、`building_garden_plot`、`building_granary`、`building_livestock_pen`、`building_fish_pond`、`building_nearshore_net_cage`、`building_mangrove_nursery`、`building_salt_marsh_reserve_station`、`building_quarry`、`building_clay_pit`、`building_brick_kiln`、`building_lime_kiln`、`building_pottery_workshop`、`building_salvage_shed`、`building_diving_platform`、`building_cave_waystation`、`building_ceremonial_house`

## 3. 字段契约

### 3.1 建筑类型定义

每个 `building_` 条目保留 D8 必填字段：

`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。

D15 扩展字段为：

`production_batch`、`representative_sample`、`production_stage`、`behavior_profile`、`asset_state`、`model_ref`、`material_ref`、`collision_ref`、`lod_profile`、`anchor_refs`、`audio_profile`、`function_tags`、`default_open_rule_ref`、`test_cases`、`quality_level`、`historical_review`、`review_evidence_refs`、`content_warning_ids`、`context_note_key`、`skip_allowed`、`presentation_levels`。

### 3.2 变体

`D15BuildingVariantDefinition` 固定包含：

`variant_id`、`building_id`、`schema_version`、`display_name_key`、`culture_id`、`faction_id`、`material_id`、`function_profile`、`model_ref`、`material_ref`、`collision_ref`、`anchor_refs`、`audio_profile`、`asset_state`、`production_stage`、`lod_profile`、`owner_system`、`test_id`、`test_cases`、`historical_review`、`content_warning_ids`。

变体不改变功能类 ID，也不携带价格、库存、伤害、维护消耗或开放判定。文化、势力、材料只是可读取的内容维度。

### 3.3 实例

每个唯一 `instance_id` 都有：

`instance_id`、`schema_version`、`building_id`、`variant_id`、`created_day`、`updated_day`、`location_ref`、`location_anchor_ref`、`owner_ref`、`owner_binding_source_ref`、`function_profile_id`、`function_tags`、`inventory_profile_id`、`inventory_binding_ref`、`inventory_owner_system`、`damage_profile_id`、`damage_state_binding_ref`、`damage_owner_system`、`open_rule_id`、`open_rule_binding_ref`、`open_rule_owner_system`、`interaction_points[]`、`audio_profile`、`model_ref`、`collision_ref`、`asset_state`、`lod_profile`、`save_state_key`、`owner_system`、`test_id`、`test_cases`、`historical_review`、`content_warning_ids`、`skip_allowed`。

三个交互点固定提供 `enter`、`service`、`inspect`，每个点都有 `interaction_point_id`、`anchor_ref`、`action_id`、`owner_system`、`test_id`。

## 4. 所有权边界

| 字段/行为 | 唯一写者 | 本批次提供 |
|---|---|---|
| 建筑定义、模型/材质/碰撞/锚点/音频引用 | Content/Asset Registry；运行时建筑行为由 BuildingSystem 接管 | 不可变定义、资源引用、占位状态 |
| 实例位置 | IslandSystem / 场景流式系统 | `location_ref` 和 `location_anchor_ref` 绑定 |
| 实例产权/占有者 | PropertySystem / FactionLegalSystem | `owner_ref` 和来源绑定，不声明最终产权 |
| 库存与库存容量 | Inventory/Cargo | `inventory_profile_id`、`inventory_binding_ref`，不写数量 |
| 损伤、火灾、维护、开放状态 | BuildingSystem | `damage_profile_id`、`damage_state_binding_ref`、`open_rule_id`，不写当前状态 |
| 存档状态 | BuildingSystem 写建筑快照，SaveSystem 持久化 | 稳定 `save_state_key`，不写运行时快照 |
| UI/Agent | 只读目录/状态/事实；Agent 只能提出意图 | 无写入口、无裁决逻辑 |

目录生成器没有引用网络、LLM、HttpClient 或状态系统，不会直接创建建筑状态或伪造 owner 的数值结果。

## 5. 资产、历史和性能

- 目前批量建筑的模型、材质、碰撞、锚点和音频统一登记为可替换 `placeholder`；不存在的资产不被标为 `verified`。
- 占位资源均有 `asset_` ID、来源、授权、商业使用许可和替换责任；没有新增外部下载。
- 主体使用冻结的 `building_functional_lod3`；变体使用 `building_variant_lod2`，实例复用定义资源，不复制网格。
- 23 个历史/制度敏感建筑使用 `ApprovedWithWarning`，都带 `content_warning_historical_institution`、上下文 key、跳过选项和四级表现选择；不把历史制度做成无代价收益。
- 代表样例 `building_saint_marys_dock`、`building_green_cay_water_well` 继续沿用 D13 owner/authority 接线，本批只补充 D15 生产字段。

## 6. 验收入口

```powershell
dotnet build game/Whalebone.Content.Runtime.csproj --no-restore --nologo
dotnet run --project tools/d15_a04_runner/D15A04Runner.csproj --no-build
python tools/d15_a04_acceptance_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools/tests/test_d15_a04_acceptance_check.py
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

本地结果：

- Content Runtime：0 warning / 0 error。
- D15-A04 runner：PASS，80 / 180 / 640。
- D15-A04 directory check：PASS，80 / 180 / 640，23 个历史警告条目。
- Python unittest：1/1 PASS。
- Content Registry：2648 entries / 13 prefixes PASS。
- D15-A04 内容红线扫描：无网络/LLM 标记。

全量 `Save.Tests` 当前未能执行到 D15 测试阶段：仓库中并行 D10 修改导致 `game/Assets/Scripts/Agent/AgentCore/CrewAgent.cs(18)` 缺失 `D10CrewBehaviorState`，属于本批范围外的前置编译阻塞；本批独立 runner、目录 checker 和 Content Runtime 已通过。

## 7. 后续接管

D15-S02 需要对 80 类、180 变体和实例抽样执行加载、交互、保存/恢复、LOD、碰撞、授权、历史审查和孤儿引用复核。正式美术资源替换时只需更新 `asset_*` 引用与资源状态，不能把替换动作变成 BuildingSystem 的第二写者。

