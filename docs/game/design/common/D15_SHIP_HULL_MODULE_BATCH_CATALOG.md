# D15-A06* 船体、改装模块与功能点批次目录

任务：`D15-A06*`  
日期：2026-08-11  
状态：**批次目录完成，代表样例可装配烟测通过**

## 1. 交付范围

本批次只生产可被游戏读取的内容定义、模型占位/样例几何、碰撞/锚点/动画/音频/UI 配置和测试元数据，不写运行时强逻辑。目录结果为：

| 类别 | 数量 | ID 规则 | 运行时接管者 |
|---|---:|---|---|
| 船体 | 20 | `ship_` | `ShipSystem` |
| 改装模块 | 80 | `item_ship_module_`（D14/D8 冻结规则） | `ItemCatalog` → `ShipSystem` |
| 功能点 | 60 | `ship_function_` | `ShipSystem` |
| 本批次资源条目 | 166 | `asset_` | `AssetRegistry` |

用户任务中的“船体、模块和功能点使用统一 `ship_`/`asset_` ID”与已冻结的 D14 模块规则存在冲突。本批次遵守 `D15_CONTENT_PRODUCTION_FREEZE.md` 和 D14：船体/功能点使用 `ship_`，库存模块继续使用 `item_`，模型、材质、配置和可读资源使用 `asset_`；没有引入 `module_` 前缀。

批次清单入口：

`game/Assets/Content/D15/A06/catalog.json`

## 2. 目录布局

```text
game/Assets/Content/D15/A06/
├─ catalog.json
├─ entries/
│  ├─ hulls/                  # 20 个 ship_ 船体定义
│  ├─ modules/                # 80 个 item_ship_module_ 定义
│  ├─ functional_points/     # 60 个 ship_function_ 定义
│  └─ assets/                 # 本批次 asset_ 资源登记
├─ assets/
│  ├─ models/
│  │  ├─ hulls/               # 船体 OBJ/现有 Moonwake FBX 引用
│  │  ├─ modules/             # 模块项目内占位几何
│  │  └─ functional_points/  # 功能点项目内占位几何
│  └─ profiles/
│     ├─ collision_profiles.json
│     ├─ anchor_profiles.json
│     ├─ animation_profiles.json
│     ├─ audio_profiles.json
│     ├─ ui_profiles.json
│     └─ material_profiles.json
```

正式 D8 注册表同步登记在：

`game/Assets/Resources/ContentRegistry/entries/{ship,item,asset}/`

生成器是确定性的：`tools/d15_a06_generate_catalog.py`。资源几何为项目内生成的低复杂度可读 OBJ 占位，既可被 Unity 导入，又没有引入无来源外部资源；已有 `asset_ph_ship_fishing` 继续作为 Moonwake 的已登记占位模型。

## 3. 内容字段

每条船体、模块和功能点均包含 D8 目录字段：

- `id`、`schema_version`、`name`、`display_name_key`；
- `source_refs`、`source_note`、`source`、`license`；
- `historical_tag`、`phase`、`status`、`owner_system`、`test_id`；
- `tags`、`description`、`gameplay_role`、区域/生境/天气有效范围、引用和资源引用；
- `production_batch`、`representative_sample`、`production_stage`、`behavior_profile`、`asset_state`；
- `model_ref`/`model_path`、`material_ref`、`collision_ref`/`collision_profile_id`、`lod_profile`；
- `anchor_refs`/`anchor_data_ref`、`animation_refs`/`animation_profile_id`、`audio_refs`/`audio_profile_id`；
- `ui_ref`/`ui_profile_id`/`ui_localization_key`、`test_cases`；
- `historical_review`、`review_evidence_refs`、`content_warning_ids`、`skip_allowed`。

批次还按类型补充：

- 船体：甲板/舱室布局、损伤锚点配置引用、功能点槽位、兼容/不兼容标签；
- 模块：模块组、所需功能点、槽位标签、互斥组、可拆卸标记；
- 功能点：`interaction_entry`、交互动词、船员角色、可用/阻塞/损坏/进水/着火/占用状态和 `ShipSystem` owner handoff。

资产 profile 的职责是提供表现和交互接线，不提供数值裁决：

- 碰撞 profile：船体/交互层、compound convex、`owner_system_only` 触发策略；
- 锚点 profile：船艏、船艉、舵轮、登船、货舱、瞭望和镜头，以及模块/功能点交互和 crew 锚点；
- 动画 profile：漂浮、转舵、升降帆、起落锚、损伤状态，以及模块/功能点启用/损坏表现；
- 音频 profile：木材、帆布、绳索、船体入水、锚链、舵机和功能点/模块交互声事件 key；
- UI profile：名称 key、船体/进水/倾斜/起火/舵/模块状态 key，以及功能点交互提示 key；
- 材质 profile：按船体、模块和功能点的项目内材质槽位/占位引用登记。

## 4. 代表样例

代表三元组：

- 船体：`ship_moonwake`（使用已有 `asset_ph_ship_fishing` 模型占位，并补齐本批次表现/交互 profile）；
- 模块：`item_ship_module_reinforced_keel`（D14 代表模块，引用 `ship_moonwake` 和 `ship_function_main_cargo_hold`）；
- 功能点：`ship_function_helm_wheel`（交互入口 `ShipSystem.FunctionalPoint.helm_wheel`）。

样例装配检查验证了：模块目标船体、所需功能点槽位、功能点 owner handoff、模型文件、碰撞、锚点、动画、音频、UI 和保存/读档测试元数据均可解析。检查不执行 ShipSystem 强逻辑，也不在目录中写入重量、吃水、伤害、速度、价格或模块效果公式。

## 5. 数量与分组

- 船体 20：涵盖 skiff、punt、cutter、sloop、schooner、ketch、brig、brigantine、barque、frigate 等外观/布局样例；
- 模块 80：船体、索具、导航、货运、武装、安全、生活、支援 8 组，每组 10 个；
- 功能点 60：操作、系泊、导航、货运、战斗、生活/医疗/维修 6 组，每组 10 个。

本批次不批量复制运行时状态，不为模块写强逻辑参数；`ShipSystem`、`ItemCatalog`、`CombatSystem`、`EconomySystem` 等原所有者继续裁决装配、效果和交易结果。

## 6. 质量与 LOD 门禁

遵守 D15 冻结表：船体使用 LOD 0/1/2/3 上限 `35,000 / 18,000 / 7,000 / 2,000` 三角面预算；模块和功能点使用 LOD 0/1/2 上限 `8,000 / 3,000 / 1,000`。当前批次的几何是项目内可导入占位/样例资源，实际高模、材质和音频二进制替换仍需在 D15-S02/后续美术验收中按授权和 Q 等级接管，不能以配置条目冒充最终美术。

所有资源条目均记录 `internal-authored` 来源和授权；没有无来源或无授权外部文件进入正式目录。资源缺失时，`asset_state`、`resource_path`、UI 状态 key 和测试字段保持可定位，不静默降级。

## 7. 检查命令与结果

在项目根目录执行：

```text
python tools/d15_a06_generate_catalog.py
python tools/d15_a06_catalog_check.py
python tools/d15_a06_assembly_smoke.py
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools.tests.test_d15_a06_catalog -v
dotnet build game/Whalebone.Content.Runtime.csproj --configuration Release
```

本次结果：

```text
D15-A06 generated: hulls=20 modules=80 functional_points=60 asset_resources=166
D15-A06 catalog PASS: hulls=20 modules=80 functional_points=60 assets=166
D15-A06 assembly smoke PASS: ship_moonwake + item_ship_module_reinforced_keel + ship_function_helm_wheel
Content registry OK: 2812 entries, 13 prefixes
Ran 3 tests in 0.047s — OK
Whalebone.Content.Runtime — 0 warnings, 0 errors
```

专项检查会拒绝：数量不符、前缀不符、资源引用不存在、模型文件缺失、碰撞/锚点/动画/音频/UI/test 字段缺失、代表样例缺失，以及 `weight`、`draft`、`damage`、`speed`、`price`、`stat_modifiers`、`risk_modifiers`、`effect_formula` 等强逻辑字段出现在资产内容目录中。

## 2026-08-12 LOD 元数据复验补充

复验发现 D15-A06 的源目录条目 `game/Assets/Content/D15/A06/entries/assets/asset_ph_ship_fishing.json` 与 Resources 注册表副本不一致：源条目缺少 `lod_profile`，导致 D16-A05 扫描将其列为集成资源元数据缺口。已补齐与已有占位船模型实际登记一致的 `lod0=source`、`lod1/2/3=generated` 声明；未修改 `asset_state`、模型文件、船体强逻辑或授权结论。

复验结果：D15-A06 catalog **PASS**（20 hulls / 80 modules / 60 functional points / 166 assets），D16-A05 集成资源缺失 LOD/测试字段由 1 条降为 **0 条**。A05 仍因 2264 个 placeholder 资源而保持 IN_PROGRESS。

整仓 `Whalebone.Systems`/`Save.Tests` 编译仍受本任务之前的 Agent/D10 工程引用问题阻断：`CrewAgent.cs` 使用 `D10CrewBehaviorState`，但当前项目文件未把该类型纳入对应编译项目。该问题不在本资产任务允许修改范围内；责任模块为 Agent/D10 项目引用，修复建议是补齐项目引用/共享编译项后重跑全量回归。本批次新增内容运行时项目 `Whalebone.Content.Runtime` 已独立编译通过。

## 8. 所有权边界与后续入口

- `AssetRegistry`：登记资源路径、来源、授权和表现资源状态；
- `ShipSystem`：装配船体、模块和功能点，裁决槽位、状态、效果和运行时保存状态；
- `ItemCatalog`/D14：模块库存、生命周期和跨系统装备事实；
- UI：只读取目录、状态快照和交互提示，不维护内容目录；
- Agent：只提出内容/交互意图，不直接写船体、模块、功能点强逻辑。

D15-S02 可从 `catalog.json` 的三类 ID 数组、`representative_samples`、每条 `test_cases` 和五类 profile 入口开始分层抽样；D16 可从 `AssetRegistry` 的 `asset_` 资源登记和 `ShipSystem` 的 owner handoff 接管正式美术替换与运行时装配验收。
