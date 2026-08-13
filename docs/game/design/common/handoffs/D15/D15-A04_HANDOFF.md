# D15-A04* 交接报告：建筑/港口批量内容

任务编号：`D15-A04*`  
日期：2026-08-11  
状态：完成（内容目录与确定性批量验收通过；最终美术替换和 D15-S02 抽样仍是后续门禁）。

## 1. 主成果

1. `game/Assets/Content/D15/D15BuildingPortBatchCatalog.cs`：纯内容目录生成器。
2. `game/Assets/Content/D15/D15BuildingPortBatchCatalog.json`：批次目标、字段契约和 owner 绑定清单。
3. `game/Assets/Resources/ContentRegistry/entries/building/`：80 个 D15-A04 `building_` 定义（含 D13 五个代表样例）。
4. `game/Assets/Resources/ContentRegistry/entries/asset/asset_ph_building_*.json`：模型、碰撞、音频占位资源登记。
5. `game/Assets/Tests/EditMode/D15BuildingPortBatchCatalogTests.cs`：Unity/NUnit 目录测试。
6. `tools/d15_a04_runner/`、`tools/d15_a04_acceptance_check.py` 和对应 unittest：独立验收入口。
7. `docs/game/design/common/D15_BUILDING_PORT_BATCH_CATALOG.md`：目录、字段、性能和所有权说明。

## 2. 数量与 ID

- 80 个功能建筑类：所有类型 ID 使用 `building_`，与 Content Registry 文件名相同。
- 180 个变体：80 个每类 base + 100 个文化/势力/材料组合变体；变体引用原建筑类，不生成第二套功能主键。
- 640 个实例：每类 8 个，使用唯一 `instance_<building_suffix>_<slot>`；实例复用模型和碰撞引用，不复制网格。
- 1,920 个交互点：每个实例都有 entry、service、inspect 三类交互点。

## 3. 实例字段清单

每个实例均包含：

- 身份：`instance_id`、`schema_version`、`building_id`、`variant_id`、`created_day`、`updated_day`；
- 位置/归属：`location_ref`、`location_anchor_ref`、`owner_ref`、`owner_binding_source_ref`；
- 功能：`function_profile_id`、`function_tags`；
- 库存绑定：`inventory_profile_id`、`inventory_binding_ref`、`inventory_owner_system`；
- 损伤绑定：`damage_profile_id`、`damage_state_binding_ref`、`damage_owner_system`；
- 开放绑定：`open_rule_id`、`open_rule_binding_ref`、`open_rule_owner_system`；
- 表现/交互：`model_ref`、`collision_ref`、`interaction_points`、`audio_profile`、`asset_state`、`lod_profile`；
- 持久化/测试：`save_state_key`、`test_id`、`test_cases`；
- 审查：`historical_review`、`content_warning_ids`、`skip_allowed`。

这些字段是实例定义/接管绑定，不是当前建筑状态；库存数量、损伤点、开放结果和实际所有者仍只能由对应系统写入。

## 4. 所有权交接

| 状态/数据 | 唯一写者 | 本任务交接方式 |
|---|---|---|
| 建筑状态、损伤、火灾、维护、开放投影 | BuildingSystem | 通过 `building_id`、`damage_*`、`open_rule_*` 和 `save_state_key` 接管 |
| 位置与流式激活 | IslandSystem/场景流式系统 | 通过 `location_ref`、`location_anchor_ref` 接管 |
| 产权、所有者、占领/法律 | PropertySystem/FactionLegalSystem | 通过 `owner_ref`、`owner_binding_source_ref` 接管 |
| 库存、容量、货物 | Inventory/Cargo | 通过 `inventory_profile_id`、`inventory_binding_ref` 接管 |
| 模型、材质、碰撞、音频资源 | AssetRegistry | 通过 `asset_*` 引用；缺失项保持 placeholder |
| 存档持久化 | SaveSystem + BuildingSystem | 通过稳定 `save_state_key` 和对应运行时快照接管 |
| UI/Agent | 只读/意图 | 不得写入任何上述字段的最终值 |

## 5. 测试命令与结果

| 命令 | 结果 |
|---|---|
| `dotnet build game/Whalebone.Content.Runtime.csproj --no-restore --nologo` | PASS，0 warning / 0 error |
| `dotnet run --project tools/d15_a04_runner/D15A04Runner.csproj --no-build` | PASS，types=80 variants=180 instances=640 |
| `python tools/d15_a04_acceptance_check.py game/Assets/Resources/ContentRegistry` | PASS，types=80 variants=180 instances=640 warnings=23 |
| `python -m unittest tools/tests/test_d15_a04_acceptance_check.py` | PASS，1/1 |
| `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` | PASS，2648 entries / 13 prefixes |
| D15-A04 内容红线扫描 | PASS，无网络/LLM 标记 |
| `dotnet build game/Save.Tests.csproj --nologo` | BLOCKED：范围外并行 D10 修改导致 `CrewAgent.cs(18)` 缺失 `D10CrewBehaviorState`，未进入本批测试 |

NUnit 测试文件已加入 `Save.Tests.csproj`；待上述前置编译阻塞清除后，执行 Unity/EditMode 或 `Save.Tests` 即可运行。

## 6. 已知边界

1. 本任务按资产/内容边界完成，不把 640 个实例注册为 BuildingSystem 的运行时状态，也不重复实现 D13 状态机。
2. 模型、材质、碰撞和音频目前是授权占位引用；条目明确为 `asset_state=placeholder`，不能作为最终表现完成宣称。
3. 位置、产权、库存、损伤、开放规则需要 D15-S02/后续运行时接管测试；目录只提供绑定字段和稳定保存键。
4. 变体和实例由确定性生成器展开；若后续需要独立 JSON 导出，应从该生成器导出，不能手工复制成第二套事实源。
5. D15-S02 尚未执行跨全部 D15 批次的最终抽样和发布门禁；本批不宣称 D16 发布候选。

