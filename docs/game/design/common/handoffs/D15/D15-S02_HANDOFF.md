# D15-S02 内容同步 QA 交接报告

## 1. 任务状态

- 任务：D15-S02
- 日期：2026-08-11
- 最终状态：**PASS**
- 前置：D15-A01*—D15-A08* 专项 checker
- 后续：允许进入 D16-A01—A06；D16 仍未完成

## 2. 交付结论

统一内容 QA 已通过：8 个 D15 批次、2,700 个批次 registry 条目、24 个确定性抽样，`errors=0 warnings=0`。Content Registry 检查为 2,812 条、13 个前缀；A01—A08 专项检查全部通过；A07 目录参数命令已支持。

本次通过只覆盖目录内容、字段完整性、引用闭包、本地化包证明和静态 JSON round-trip。placeholder 仍是占位资源，Unity Play Mode、真实存档断电恢复、最终美术、性能与发布门禁交给 D16。

## 3. 修复内容

- `tools/d15_a02_vegetation_batch.py` 与 A02 单测：固定两个代表植物的 canonical habitat/wind profile。
- `tools/d15_a06_generate_catalog.py`：A06 资源生成路径补审查证据、LOD、警告/跳过策略和 save/restore 测试。
- `tools/d15_a07_batch_check.py`：支持从 Content Registry 目录解析 A07 manifest。
- `tools/d15_s02_content_qa.py` 与测试：合并 A08、A01—A07 locale 包；登记 A07 appearance profiles；验证非 A08 key 的双语闭包。
- `tools/d15_s02_repair.py`：可重复执行的内容元数据修复入口；已更新 A02/A06/A07/A08、本地化和 9 个存档标记样本。
- `game/Assets/Resources/Localization/D15_A01_A07/`：新增 zh-CN/en-US 本地化包。
- `game/Assets/Resources/ContentRegistry/`：修复目录字段与 A07 manifest；未删除宠物或改变批次数量。
- `game/Whalebone.Agent.Runtime.csproj`、`game/Whalebone.Systems.csproj`：补齐 D10/D13 standalone build 的显式编译引用。

## 4. 验证记录

```text
Content Registry: 2812 entries / 13 prefixes              PASS
D15-A01: 50 islands                                      PASS
D15-A02: 1211 plants                                     PASS
D15-A03: 300 animals/fish                                PASS
D15-A04: 80 types / 180 variants / 640 instances         PASS
D15-A05: 200 functional / 300 decorative                 PASS
D15-A06: 20 hulls / 80 modules / 60 functional / 166     PASS
D15-A07: 60 crew / 500 NPC / 12 variants                  PASS
D15-A08: 5 mainline / 120 side / 30 themes / 8 layers     PASS
D15-S02: batches=8 samples=24 errors=0 warnings=0         PASS
AgentCore / Systems / Save.Tests standalone build         0 warning / 0 error
```

结果快照：`tools/results/d15_s02_content_qa_result.json`。

## 5. 边界与待办

- 未修改 `game/Assets/Scripts/Systems/` 的强逻辑、系统所有权、网络、HTTP 或 LLM 边界。
- 未把目录完成误报为最终美术完成；D16-A05 仍需处理 placeholder。
- D16-A02 仍需真实执行 Save.Tests、迁移、文件级备份/恢复和断电场景。
- D16-A01、A03、A04、A05、A06 尚未取得各自的发布级证据，不能标记为 DONE。

相关文档：

- 主报告：`docs/game/design/common/D15_BATCH_CONTENT_QA_REPORT.md`
- 最终门禁索引：`docs/game/design/common/D16_FINAL_HANDOFF_INDEX.md`
- 今日记忆：`.workbuddy/memory/2026-08-11.md`

## 6. 2026-08-11 顺序复验补充

最终验收前置阶段再次按顺序执行了 Content Registry、D15-A01—A08 和 D15-S02。A02 生成器会写入植被内容，因此与 Registry 检查并行会产生读取竞态；改为先完成生成、再执行检查后，所有结果稳定通过。

```text
Content Registry: 2812 entries / 13 prefixes                 PASS
D15-A01—A08: 各专项 checker                                      PASS
D15-S02: batches=8 samples=24 errors=0 warnings=0             PASS
```

证据快照：`tools/results/d15_s02_content_qa_result.json`。这次复验未改变 D15 的内容数量，也未修改 Systems 强逻辑、网络、LLM 或数据所有权边界。D16-A01—A06 现在可以继续按串行门禁推进。

## 7. A02 生成器回归保护

复验中发现 A02 生成器会重写 13 个植被 profile，可能擦除 D16-A04 的 Pending 结构。已修正 `tools/d15_a02_vegetation_batch.py`，让生成器直接输出 Pending 审查字段；随后重跑 A02 生成、Registry 检查、D15-S02 和 D16-A04 全项目审计：D15-S02 仍为 `batches=8 samples=24 errors=0 warnings=0`，D16-A04 结构错误仍为 0。没有把 Pending 改成 Approved。

## 8. 2026-08-12 当前工作区复验补充

- Content Registry：2812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部专项检查退出 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- AgentCore 构建 0 warning / 0 error；Save.Tests 恢复 Unity 临时资产后构建 0 warning / 0 error；
- 全量 `python -m unittest discover -s tools/tests -p 'test_*.py' -v`：**48/48 PASS**。

本轮修复了 D15-A06 源目录 `asset_ph_ship_fishing` 缺失 `lod_profile` 的元数据回归，未改变内容数量或占位/最终状态。D15-S02 继续只覆盖内容目录和静态 QA，D16 运行时门禁仍需独立证据。

## 2026-08-12 全量工具回归复验

- 当前工作区重新执行 D15-A01—A08：全部专项检查退出 0；Content Registry 为 2812 entries / 13 prefixes，PASS。
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS。
- AgentCore、Save.Tests 构建：0 warning / 0 error；工具测试全集：**48/48 PASS**。
- 该复验只确认 D15 内容门禁和本地工程依赖没有回归，不关闭 D16 的目标设备、人工审查、最终资产、跨平台存档或发布门禁。

## 2026-08-12 目标命令逐项复验

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：九个专项命令全部退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 工具全量回归：**52/52 PASS**；
- 本轮未改变内容数量、placeholder 状态、授权决定或强逻辑数据所有权。

当前结论仍为 D15-S02 **PASS**、D16-A01—A06 分别按各自交接保持未完成；不能把 D15 静态内容 QA 扩大为 D16 或最终版本完成。

## 2026-08-12 当前命令再次复验

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部专项命令退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 本轮未改变内容数量、placeholder 状态、授权决定或强逻辑数据所有权。

D15-S02 继续 **PASS**；D16 外部设备、人工审查、最终资产、跨平台存档和发布门禁仍未完成。

## 2026-08-12 工具测试全集最新统计

`python -m unittest discover -s tools/tests -p "test_*.py" -q` 当前为 **53/53 PASS**，0 failures、0 errors。旧的 48/48、52/52 仅保留为历史快照；本轮未改变 D15 内容数量、placeholder 状态、授权决定或强逻辑数据所有权。

## 2026-08-12 D15 专项门禁当前权威复验

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08 九个专项命令当前全部退出码 0，关键数量为 50 岛屿、1,211 植物、300 动物/鱼类、500 物品/服装/装饰、166 船舶资产、60 船员预设/500 NPC、452 本地化键；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 本轮为只读复验，没有改变内容数量、placeholder 状态、授权决定或强逻辑数据所有权。D16 的外部设备、人工审查、最终资产、跨平台存档和发布门禁仍按各自报告保持原状态。

## 2026-08-12 工具测试全集当前复验

`python -m unittest discover -s tools/tests -p "test_*.py" -q`：**61/61 PASS**，0 failures、0 errors；本轮只新增 D16-A01 本机硬件基线测试，没有改变 D15 内容或授权状态。

## 2026-08-12 最终验收任务清单复验

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部专项命令退出码 0；A02 植物 1,211 条，A06 船舶资产 166 条，A08 中英本地化抽样无缺失；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 工具测试全集：61/61 PASS；
- AgentCore、Save.Tests：受控 restore 后按 `--no-restore` 编译 0 warning / 0 error。

本轮没有把 placeholder 升级为最终资产、没有伪造授权/历史决定、没有删减内容，也没有触碰 Systems/Agent/contracts 边界。D15-S02 状态为 PASS；D16-A01—A06 及 D16-S01/S02 仍按各自真实门禁状态处理。

## 2026-08-12 D16-A03 后当前权威复验

- Save.Tests 在恢复 Unity 临时依赖索引后构建 0 warning / 0 error；
- Content Registry 2,812 entries / 13 prefixes；D15-A01—A08 全部退出码 0；
- D15-S02 8 batches / 24 samples / 0 errors / 0 warnings，**PASS**；
- 9 个指定保存/恢复样本 `runtime_state` 证据仍为 9/9 verified；A08 中英文抽样无缺失；
- 工具测试全集 `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**69/69 PASS**；
- 本轮为复验，没有把 placeholder 升级为最终资产，没有新增授权/历史结论，也没有触碰 Systems、Agent、contracts 或存档所有权。

下游状态：D15-S02 保持 PASS；D16-A01—A06 仍按各自真实门禁执行，D16-S01/S02 不启动。

## 2026-08-12 D10/D15 工程依赖与专项清单最新复验

### 验证

- AgentCore：`dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`，0 warning / 0 error。
- Save.Tests 首次 `--no-restore` 的失败原因为 Unity 临时 `game/Temp/obj/Save.Tests/project.assets.json` 缺失；执行 `dotnet restore game/Save.Tests.csproj --ignore-failed-sources` 后重新 `--no-restore` 构建，0 warning / 0 error。
- Content Registry、D15-A01—A08 十项专项命令全部退出码 0；D15-S02 为 8 batches / 24 samples / 0 errors / 0 warnings，**PASS**。
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**69/69 PASS**。

### 结论与边界

本轮只复验 D10 工程依赖和 D15 内容 QA，没有修改内容、placeholder、授权/历史决定或 Systems/Agent/contracts 数据所有权。D15-S02 可以保持 PASS；D16-A01—A06 的外部设备、人工审核、最终资源、跨平台存档和发布证据仍未闭合，D16-S01/S02 不启动。

## 2026-08-12 保存/恢复契约修复后回归

- A02 生成器复跑后植物仍为 1,211 条，9 个指定样本的 `runtime_state` 契约保持存在；
- Content Registry 2,812/13 prefixes；D15-A01—A08 全部退出码 0；D15-S02 8 batches / 24 samples / 0 errors / 0 warnings PASS；
- 工具测试全集：**63/63 PASS**，0 failures、0 errors；D15-S02 专项新增契约测试为 5/5 PASS。

本轮只扩充内容 QA 的证据要求和样本元数据，未修改强逻辑、网络、LLM、Systems 数据所有权或最终资产状态。D16-A01—A06 仍按各自交接的真实阻断项处理。

## 2026-08-12 D10/D15 当前权威复验

### 验证结果

- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet restore game/Save.Tests.csproj --ignore-failed-sources` 后，`dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；首次无 restore 失败仅为 Unity 临时 `project.assets.json` 缺失；
- Content Registry：2,812 entries / 13 prefixes；
- D15-A01—A08 专项命令：全部退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，**PASS**；
- 工具全集：本轮上一项 A04 日期安全增量后的 **71/71 PASS**。

### 结论与边界

D15-S02 当前仍为 PASS。A02 植物生成器按冻结批次入口完成本轮复验，数量与 QA 结果保持一致；没有把 placeholder 升级为最终资产，没有新增历史/授权结论，也没有触碰 Systems、Agent、contracts 或存档数据所有权。D16-A01—A06 的真实外部门禁仍独立未闭合，D16-S01/S02 不启动。

## 2026-08-12 保存/恢复契约复核与修复

发现旧 QA 的 `test_cases` marker 会把目录 JSON round-trip 当成保存/恢复证据。已在 `tools/d15_s02_repair.py` 为任务要求的 9 个抽样条目写入 `runtime_state`，并在 `tools/d15_s02_content_qa.py` 中加入结构化校验和 marker-only 反例：

- 2 个 D15-A08 任务绑定真实入口 `SaveSystem.SaveD12Runtime(D12RuntimeSnapshot)` / `SaveSystem.LoadD12Runtime()`，路径为 `D12RuntimeSnapshot.DynamicQuestLog`；
- 7 个资源/定义声明 `definition_only / immutable_definition`，并说明其不可变定义不进入可变运行时存档；
- 统一报告 `tools/results/d15_s02_content_qa_result.json`：`runtime_save_restore.verified_count=9`、`missing_count=0`；
- `python -m unittest tools.tests.test_d15_s02_content_qa -v`：5/5 PASS；统一 D15-S02：0 errors / 0 warnings / PASS。

这项修复只涉及 D15 内容元数据和 QA 证据规则，没有修改 Systems 强逻辑、网络、LLM 或存档数据所有权。D16-A01—A06 的外部设备、人工审核、最终资源、跨平台存档和发布门禁仍未闭合。
## 2026-08-12 总纲顺序下的权威复验

### 状态

**PASS（D15-S02 内容 QA）；D16 外部门禁仍未完成。**

### 验证

- D10 `AgentCore` 与 `Save.Tests` 构建：0 warning / 0 error；
- Content Registry：2,812 entries / 13 prefixes；
- D15-A01—A08 专项命令：全部退出码 0；
- `python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json`：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- A08 中英本地化抽样缺失为 0，9 个指定保存/恢复样本的结构化证据仍完整。

### 边界

本轮为工程顺序复验，没有删除条目、改变 placeholder 状态、伪造历史/授权、修改 contracts 或改变运行时数据所有权。下游为 D16-A01—A06；目标设备、人工审查、最终资产、跨平台存档和发布边界必须分别取得真实证据后才能进入 D16-S01/S02。

### Canonical 植被语义抽查

当前 `plant_red_mangrove` 的主生境为 `mangrove`、风区为 `mangrove_low`；`plant_shallow_seaweed` 的主生境为 `nearshore`、风区为 `nearshore`，且两者主生境均存在于各自 `valid_habitats`。本轮只读核对，没有改动内容数量或资源状态。
