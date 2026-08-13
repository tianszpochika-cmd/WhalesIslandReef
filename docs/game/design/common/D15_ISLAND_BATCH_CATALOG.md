# D15-A01* 岛屿批量目录

任务：`D15-A01*`  
日期：2026-08-11  
状态：**已生产目录与自然档案；50/50 条目通过目录和抽样检查**  
质量口径：代表样例 `Q3`；其余内容 `Q2`；当前模型/材质为可替换占位，不宣称最终美术完成。

## 1. 交付范围

本批交付 50 条独立 `island_` 内容定义，覆盖 8 类主地貌模板。每条定义都包括：

- 中文名、英文名、历史称呼字段、唯一 ID 和 `D8-A02-D15-A01-ISLAND-*` 测试入口；
- 地质形成、迎风面、背风面、海岸类型、山地/高地、洞穴、淡水和暗礁/浅滩档案；
- 海岸层、内陆层、高地/地下层三个稳定区域，以及生境、植被/动物/鱼群资源引用；
- 资源点的刷新、耗尽、恢复、工具/观察入口和玩家反馈 key；
- 建筑类型锚点、库存/损伤 owner 投影、任务入口、登陆点、替代路线和恢复路线；
- 流式 cell、预加载/卸载策略、LOD 档位、稳定存档 key 和恢复字段；
- 来源、授权、历史标签、审查状态、占位资源状态和测试字段。

目录位置：`game/Assets/Resources/ContentRegistry/entries/island/`。注册表根节点已加入 50 个岛屿 ID，避免新的岛屿条目成为孤儿引用。

## 2. 8 类主地貌模板

| 模板 ID | 中文 | 数量 | 地质 | 主要自然结构 | 主要风险 |
|---|---|---:|---|---|---|
| `volcanic_high_island` | 火山高岛 | 8 | `volcanic_rock` | 火山锥、黑色砾滩、熔岩管、深水湾、迎风雨带 | 落石、火山灰能见度、深水浪涌 |
| `limestone_karst_island` | 石灰岩与喀斯特岛 | 10 | `limestone` | 峰林、白崖、落水洞、地下河、岩池 | 崖边、洞穴涨水、淡水不足 |
| `coral_sandbar_island` | 低矮珊瑚沙洲 | 8 | `coral_deposit` | 沙脊、白沙湾、泻湖、礁口、潮间带 | 搁浅、风暴潮、淡水不足 |
| `mangrove_tidal_island` | 红树林潮汐岛 | 4 | `alluvial_sediment` | 根系水道、泥滩、盐沼、海草浅水 | 泥滩下陷、蚊虫、潮汐倒流 |
| `alluvial_estuary_island` | 冲积河口湿地岛 | 2 | `alluvial_sediment` | 三角洲分汊、河口、湿地、浅湖 | 洪水上涨、泥滩下陷、半咸水 |
| `wet_mountain_rainforest_island` | 湿润山地雨林岛 | 8 | `volcanic_rock` | 云雾山、瀑布、河谷、高冠雨林 | 滑坡、暴涨洪水、低能见度 |
| `leeward_dry_shrub_island` | 背风干旱灌丛岛 | 5 | `limestone` | 旱脊、刺灌、仙人掌坡、季节性水道 | 热暴露、缺水、突发风暴 |
| `atoll_reef_ring` | 环礁与礁环岛群 | 5 | `coral_deposit` | 礁环、浅滩、泻湖、狭窄礁口 | 搁浅、风暴潮、潮汐倒流 |

配额合计：`8 + 10 + 8 + 4 + 2 + 8 + 5 + 5 = 50`。原岛屿设计中的红树林/河口组合在批次中拆成红树林潮汐岛和冲积河口湿地岛，仍保持冲积地貌、河口和湿地约束。

## 3. 50 条内容清单

### 火山高岛（8）

`island_ember_peak` 余烬峰岛 / Ember Peak；`island_ashfall_bay` 灰落湾岛 / Ashfall Bay；`island_cinder_spine` 煤渣脊岛 / Cinder Spine；`island_blackglass_cape` 黑曜岬岛 / Blackglass Cape；`island_sulfur_shelf` 硫磺台岛 / Sulfur Shelf；`island_redcrater` 赤口岛 / Redcrater Isle；`island_windward_cone` 迎风锥岛 / Windward Cone；`island_lava_gate` 熔门岛 / Lava Gate。

### 石灰岩与喀斯特岛（10）

`island_saint_marys` 圣玛丽岛 / Saint Mary's Island；`island_limestone_crown` 石灰冠岛 / Limestone Crown；`island_sinkhole_key` 落水洞礁 / Sinkhole Key；`island_bathedral_ridge` 石堂脊岛 / Cathedral Ridge；`island_blue_hole` 蓝洞岛 / Blue Hole Isle；`island_whitecliff` 白崖岛 / Whitecliff；`island_copperkarst` 铜峰岛 / Copperkarst；`island_cove_caves` 海湾洞岛 / Cove Caves；`island_hollowspine` 空脊岛 / Hollowspine；`island_drydrop` 旱瀑岛 / Drydrop Isle。

### 低矮珊瑚沙洲（8）

`island_driftwood_isle` 漂木岛 / Driftwood Isle；`island_pearl_sand` 珍珠沙岛 / Pearlsand；`island_saltwind_key` 盐风礁 / Saltwind Key；`island_seaglass_cay` 海玻璃礁 / Seaglass Cay；`island_lagoon_drift` 漂潮泻湖岛 / Lagoon Drift；`island_turtle_spit` 海龟沙嘴 / Turtle Spit；`island_brightshoal` 明滩岛 / Brightshoal；`island_whitecap_spit` 白沫沙嘴 / Whitecap Spit。

### 红树林潮汐岛（4）

`island_green_cay` 绿礁岛 / Green Cay；`island_mangrove_six` 六根红树林岛 / Sixroot Mangrove；`island_riverroot` 河根岛 / Riverroot；`island_tidal_reed` 潮芦岛 / Tidal Reed。

### 冲积河口湿地岛（2）

`island_blackmud_delta` 黑泥三角洲 / Blackmud Delta；`island_reedwater_basin` 芦水盆地岛 / Reedwater Basin。

### 湿润山地雨林岛（8）

`island_jaguar_heights` 美洲豹高地岛 / Jaguar Heights；`island_cloudfall` 云瀑岛 / Cloudfall；`island_raincrown` 雨冠岛 / Raincrown；`island_parrot_ridge` 鹦鹉脊岛 / Parrot Ridge；`island_waterfall_isle` 瀑群岛 / Waterfall Isle；`island_deepgreen` 深绿岛 / Deepgreen；`island_mossback` 苔背岛 / Mossback；`island_orchid_slope` 兰坡岛 / Orchid Slope。

### 背风干旱灌丛岛（5）

`island_drythorn` 旱刺岛 / Drythorn；`island_saltbrush` 盐灌岛 / Saltbrush；`island_sunscorch` 灼日岛 / Sunscorch；`island_cactus_bight` 仙掌湾岛 / Cactus Bight；`island_burrow_cape` 穴岬岛 / Burrow Cape。

### 环礁与礁环岛群（5）

`island_ringwatch` 环礁守望岛 / Ringwatch；`island_bluechain` 蓝链礁群 / Bluechain；`island_reefclock` 礁钟岛 / Reefclock；`island_horseshoe_atoll` 马蹄环礁 / Horseshoe Atoll；`island_moonpool` 月池礁 / Moonpool。

## 4. 统一字段和资源边界

### 4.1 D8 目录字段

50 条内容均保留 D8 必填字段：`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。

所有条目使用：

- `category=island`、`owner_system=IslandSystem`、`phase=D15`、`production_batch=D15-A01*`；
- `status=production`、`production_stage=integrated`；代表的圣玛丽岛/漂木岛为 `Q3`，其余为 `Q2`；
- `historical_tag=inferred` 或代表样例保留的 `fictional`；`historical_review=Approved`；本批不把自然档案写成现实地理断言；
- `source.kind=project-authored`、项目自有来源文档、`internal-authored` 商业发行与修改授权；
- `test_cases` 中分别登记加载、保存、恢复三个测试入口；资源引用、建筑入口和任务入口可由 Content Registry 沿图扫描。

### 4.2 D15 岛屿扩展字段

每条自然档案的 `natural_archive` 结构固定包含：

```text
template_id / template_label / variant_code / historical_names
formation / geology / windward_face / leeward_face / coast_types
freshwater / mountain / caves / reef_features / habitats / zones
resources / building_entrances / quest_entrances / landing / routes
hazards / social_structure / streaming / save_contract / recovery
```

`zones` 固定为 `coast`、`inland`、`upper` 三层。每层有 `region_id`、地质、生境、海拔、水源、可达性、危险、植被/动物/资源、建筑/势力/任务标签和 `streaming_cell`。

`resources` 只引用已有 `plant_`、`animal_`、`fish_` 内容 ID。每个资源点声明 `node_type`、生境、工具/观察入口、刷新、耗尽、恢复和反馈 key；实际资源量、采集结果和生态压力仍由 `EcologySystem`/D11 采集 owner 写入。

`building_entrances` 只引用已有 `building_` 定义，并保存位置锚点及 `BuildingSystem_runtime`、`InventoryCargo_runtime`、`BuildingSystem_runtime` 投影标记；不写所有权、库存、损伤或服务价格。

`quest_entrances` 只引用 `quest_first_chart` 代表入口，并保存可达区域、路线和恢复路线；岛屿目录不写任务状态、奖励或任务结果。

## 5. 流式加载、保存和恢复

每座岛都有三个流式 cell：海岸、内陆、高地/地下。目录声明：

- 近景启用碰撞和交互；中景保留音频/动画；远景使用代理；
- 预加载主登陆点和任务入口；离开区域前保存状态再释放高成本对象；
- LOD 使用 `island_terrain_lod3`，遵守 D15 冻结的 50,000 / 20,000 / 6,000 / 2,000 三角面对象预算；
- `save_contract.snapshot_key` 为 `location_json.island.<island_id>`，稳定键是岛屿 ID；保存字段为访问、发现、资源节点状态、建筑投影、任务入口投影和 stream cell 状态；
- 恢复按稳定 `island_` ID 读取，不生成资源、建筑、任务或生态事实，不重复创建运行时实体；天气、生态、建筑、船舶、任务和保存 owner 继续各自裁决。

本任务没有在 `Systems/` 中加入 IslandSystem、SaveSystem、EcologySystem 或网络/LLM 逻辑。`tools/d15_a01_island_check.py` 的抽样测试对 10 座（20%）岛执行目录级 stream → serialize → restore，验证稳定 ID、cell、存档键和资源节点不变；它是内容准入测试，不冒充 Unity 场景性能测试。

## 6. 来源、授权和占位资源

来源引用统一指向：

- `docs/game/design/islands/01_ISLAND_OVERVIEW.md`；
- `docs/game/design/islands/05_ISLAND_CONTENT_PLACEMENT.md`；
- `docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md`；
- `docs/game/design/common/D8_CONTENT_ID_REGISTRY.md#sample-licensing`。

当前 50 条 `asset_refs` 使用已有授权的 `asset_ph_palm_leaves` 作为可替换占位引用，`asset_state=placeholder`，并登记 `model_ref`、材质占位名、碰撞引用和 LOD 档位。正式模型/材质接入后必须保留岛屿 ID，补齐 `asset_` 来源、许可证、商业使用、署名、修改权限和证据，不得删除占位记录来伪造完成。

历史标签为 `inferred` 的自然档案只表达项目游戏化推断；涉及现实社群、土地、殖民、宗教、保护区或历史称呼时，必须在后续内容审查中补警告、上下文、跳过选项和证据，不得从岛屿名称自动推断现实事实。

## 7. 验证

执行：

```text
python tools/d15_a01_island_check.py game/Assets/Resources/ContentRegistry
  D15-A01 island catalog PASS: islands=50, templates=8, stream_save_restore_sample=10

python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
  Content registry OK: 174 entries, 13 prefixes

python -m unittest discover -s tools/tests -p 'test_d15_a01_island_check.py' -v
  Ran 1 test ... OK
```

检查覆盖：50 个唯一 `island_` ID、8 类配额、D8 字段、来源/商业授权、owner、自然档案、三层区域、淡水、洞穴、礁特征、资源恢复、建筑/任务入口、流式 cell、保存键、恢复规则和 10 座岛抽样。

## 8. 后续责任

- D11/Ecology：接管资源量、生境、刷新、枯竭、恢复和采集结果；
- D13 Building/Property/Governance：接管建筑状态、产权、人口、库存投影和服务入口；
- D12/D15-A08：接管任务事实、替代路线和奖励请求；
- ShipSystem：接管海图、吃水、航路、暗礁和登陆风险裁决；
- SaveSystem：接管实际存档写入和版本迁移；
- D15-S02：按冻结表执行 20%/不少于 10 座的加载、保存、恢复抽样，并检查缺材质、缺碰撞、缺 LOD、授权和孤儿引用。

结论：**D15-A01* 50 座岛屿目录、8 类主地貌模板和代表性流式/保存/恢复准入测试已完成；无运行时强逻辑复制。**
