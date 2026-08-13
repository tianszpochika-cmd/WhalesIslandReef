# D15-A06* 交接报告

任务：`D15-A06*`  
日期：2026-08-11  
状态：**完成；目录与代表样例验收通过**

## 1. 交付物

- 主目录：[D15_SHIP_HULL_MODULE_BATCH_CATALOG.md](../../D15_SHIP_HULL_MODULE_BATCH_CATALOG.md)
- 批次 manifest：`game/Assets/Content/D15/A06/catalog.json`
- 内容目录：`game/Assets/Content/D15/A06/entries/`
- 资源 profile/模型：`game/Assets/Content/D15/A06/assets/`
- D8 注册表同步：`game/Assets/Resources/ContentRegistry/entries/{ship,item,asset}/`
- 生成器：`tools/d15_a06_generate_catalog.py`
- 批次检查器：`tools/d15_a06_catalog_check.py`
- 代表装配烟测：`tools/d15_a06_assembly_smoke.py`
- 本报告：`docs/game/design/common/handoffs/D15/D15-A06_HANDOFF.md`
- 当日记忆：`.workbuddy/memory/2026-08-11.md`

## 2. ID 前缀与数量

| 内容 | 前缀 | 数量 | owner |
|---|---|---:|---|
| 船体 | `ship_` | 20 | `ShipSystem` |
| 库存改装模块 | `item_ship_module_` | 80 | `ItemCatalog` → `ShipSystem` |
| 功能点 | `ship_function_` | 60 | `ShipSystem` |
| 资源/模型/profile | `asset_` | 166 | `AssetRegistry` |

模块没有新增 `module_` 前缀；这是 D8/D14 已冻结的兼容要求。每条内容都登记名称、本地化 key、来源、授权、历史标签、阶段、所属系统和 `test_id`，并带 D15 资产扩展字段。

## 3. 代表样例与目录结构

- `ship_moonwake`：船体样例；
- `item_ship_module_reinforced_keel`：D14 模块样例，目标为 `ship_moonwake`，所需功能点为 `ship_function_main_cargo_hold`；
- `ship_function_helm_wheel`：功能点样例，入口为 `ShipSystem.FunctionalPoint.helm_wheel`。

所有样例和批次条目都有模型引用、碰撞引用、锚点、动画、音频、UI、本地化 key 和至少四条测试用例（load/interact/save-restore/owner-handoff）。功能点另有交互动词、船员角色、可用/阻塞/损坏/进水/着火/占用状态。

## 4. 检查命令

```text
python tools/d15_a06_generate_catalog.py
python tools/d15_a06_catalog_check.py
python tools/d15_a06_assembly_smoke.py
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools.tests.test_d15_a06_catalog -v
dotnet build game/Whalebone.Content.Runtime.csproj --configuration Release
```

结果：

```text
D15-A06 generated: hulls=20 modules=80 functional_points=60 asset_resources=166
D15-A06 catalog PASS: hulls=20 modules=80 functional_points=60 assets=166; models/collision/anchors/animation/audio/UI/test fields complete
D15-A06 assembly smoke PASS: ship_moonwake + item_ship_module_reinforced_keel + ship_function_helm_wheel
Content registry OK: 2812 entries, 13 prefixes
Ran 3 tests in 0.047s — OK
Whalebone.Content.Runtime — 0 warnings, 0 errors
```

专项检查覆盖数量、前缀、文件名、资源引用、模型存在性、碰撞/锚点/动画/音频/UI/test 完整性、代表样例和强逻辑字段禁入。注册表检查覆盖 D8 统一必填字段、引用闭包和合法前缀。

整仓 `Whalebone.Systems` 与 `Save.Tests` 编译未通过，原因是既有 `Whalebone.Agent.Runtime` 工程缺少 `D10CrewBehaviorState` 编译引用（`CrewAgent.cs` 第 18 行）。该错误不由本批次引入，也不在资产 Agent 允许修改范围；责任模块为 Agent/D10 工程引用，后续应补齐项目共享编译项后重跑全量编译。本批次可独立消费的 `Whalebone.Content.Runtime` 已通过。

## 5. 约束确认

- 未修改 `game/Assets/Scripts/Systems/`，没有网络、HTTP 或 LLM；
- 未写重量、吃水、伤害、速度、价格或模块效果公式；
- 未把 UI 变成内容目录；UI profile 只提供展示 key 和交互提示 key；
- 所有资源为项目内 authored/可追溯占位，未放入无来源或无授权外部资源；
- 运行时装配、库存、模块效果、损伤和保存状态仍由所属系统裁决；
- D15-S02 仍需按冻结表进行分层抽样，并对后续正式美术/音频替换重新审查授权和 Q 等级。

## 6. 后续入口

1. D15-S02：读取 `catalog.json` 的三类 ID 数组，抽样每类至少一项；复核五类 profile、资源路径、授权、历史审查、代表样例和 owner handoff。
2. `ShipSystem`：以船体 `functional_point_slots`、模块 `required_functional_point`/`slot_tags` 和 `owner_handoff` 接入装配裁决；不要把资产字段当作强逻辑数值。
3. `ItemCatalog`/D14：以 `item_ship_module_*` 继续接管模块库存、获取、维修、装备和拆卸事实。
4. `AssetRegistry`：从 `asset_` 条目替换项目内模型/材质/音频占位，并保留来源、授权和 `asset_state` 变更记录。
5. D16：可将本批次作为船舶正式美术、音频、本地化和运行时接管的输入目录；本任务没有提前实现 D16 逻辑。

## 2026-08-12 源目录 LOD 元数据补充

复验 D16-A05 时发现源目录 `game/Assets/Content/D15/A06/entries/assets/asset_ph_ship_fishing.json` 缺少 `lod_profile`，而 Resources 注册表副本已有对应声明。已在源目录补齐：`lod0=source`、`lod1=generated`、`lod2=generated`、`lod3=generated`。本次只修正表现元数据一致性，没有把占位资源升级为最终资源，也没有改动船体数值、所有权、授权或运行时逻辑。

验证：

- `python tools/d15_a06_catalog_check.py game/Assets/Resources/ContentRegistry`：PASS；
- `python tools/d16_a05_asset_audit.py game game/Assets`：`integrated_missing_visual_metadata_count=0`，`invalid_metadata_count=0`；
- `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：12/12 PASS。

D15-A06 继续保持“目录完成、最终美术未完成”的边界；D16-A05 仍被 placeholder 资源阻断。
