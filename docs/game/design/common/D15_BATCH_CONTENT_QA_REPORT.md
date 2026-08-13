# D15-S02 批量内容生产验收报告

任务：D15-S02  
日期：2026-08-11  
范围：D15-A01*—D15-A08*、Content Registry、批次 manifest、授权/本地化/资产引用和确定性内容 QA。

## 1. 最终结论

**D15-S02：PASS。**

本报告保留了本轮初始扫描的失败事实，并记录修复后的串行复验结果。D15 的通过范围是内容目录、跨批次字段完整性、引用闭包、本地化证明和确定性 JSON round-trip 探针；这不是 Unity 运行时完整验收，也不代表 placeholder 已替换为最终美术资源。

初始扫描发现的六类问题已关闭：A02 两处生境字段冲突、A06 166 个资源的审查/LOD/测试元数据缺口、A07 12 个宠物外观 profile 孤儿引用、A08 任务/音频测试与 LOD 字段缺口、A01—A07 本地化证明缺失，以及 Save.Tests 的 D10 工程引用阻断。

## 2. 批次数量核对

| 批次 | 注册表条目 | 内容条目 | 资产/资源条目 | 结果 |
|---|---:|---:|---:|---|
| D15-A01* 岛屿 | 50 | 50 | 0 | PASS |
| D15-A02* 植被 | 1,211 | 1,211 | 0 | PASS |
| D15-A03* 动物/鱼类 | 300 | 300 | 0 | PASS |
| D15-A04* 建筑/港口 | 83 | 80 | 3 | PASS |
| D15-A05* 道具/服装/装饰 | 500 | 500 | 0 | PASS |
| D15-A06* 船体/模块/功能点 | 326 | 160 | 166 | PASS |
| D15-A07* 角色/宠物 | 66 | 66 | 0 | PASS |
| D15-A08* 任务/音乐/音频 | 164 | 125 | 39 | PASS |
| **合计** | **2,700** | **2,592** | **208** | **PASS** |

完整 Content Registry 当前为 **2,812 条、13 个稳定前缀**。A07 manifest 另包含 60 个 crew presets、500 个 NPC instances、12 个 named pet variants；A08 包含 452 个专用本地化 key。

## 3. 修复与所有权

本轮修复保持内容工具和目录元数据边界，没有修改 `game/Assets/Scripts/Systems/` 的强逻辑、网络或 LLM 边界：

- A02 生成器固定 `plant_red_mangrove=mangrove`、`plant_shallow_seaweed=nearshore`，并同步风区；专项测试锁定这两个 canonical sample。
- A06 生成器和现有资源更新路径补齐真实项目文档证据、审查状态、警告/跳过策略、LOD 和 save/restore 测试入口。
- A07 manifest 增加 12 个由现有宠物变体派生的稳定 `appearance_profiles`，没有删除宠物内容。
- A08 任务和音频目录补齐数据/占位状态、LOD-N/A、加载/触发/交互/回退/保存恢复测试入口。
- A01—A07 新增独立的 `D15_A01_A07` zh-CN/en-US 本地化包；文本值使用项目目录名称或可读 fallback，不把稳定 key 当作翻译值。
- A07 checker 接受 Content Registry 目录参数并解析固定 manifest 路径，和其余 D15 checker 的调用方式一致。
- D10 的 Agent.Runtime、Systems 和 Save.Tests 工程引用已同步，三个独立 dotnet build 均为 0 warning / 0 error。

所有数值裁决和状态写入仍由所属系统负责；QA/修复工具只更新不可变内容定义、资源引用和测试/审查元数据。

## 4. 检查矩阵与最终结果

| 检查项 | 结果 | 证据 |
|---|---|---|
| Content Registry | PASS | 2,812 entries / 13 prefixes |
| A01 岛屿 | PASS | 50 islands / 8 templates |
| A02 植被 | PASS | 1,211 plant entries / 13 categories |
| A03 动物与鱼类 | PASS | 300 entries / 166 animals + 134 fish |
| A04 建筑 | PASS | 80 types / 180 variants / 640 instances |
| A05 道具服装装饰 | PASS | 200 functional + 300 decorative |
| A06 船体模块功能点 | PASS | 20 hulls / 80 modules / 60 functional points / 166 assets |
| A07 角色宠物 | PASS | 60 crew / 500 NPC / 12 variants |
| A08 任务音频本地化 | PASS | 5 mainline / 120 side / 30 themes / 8 layers / 452 keys |
| A07 目录参数调用 | PASS | Content Registry 目录参数可用 |
| 跨批次嵌套引用 | PASS | missing nested IDs = 0 |
| 非占位模型重复 | PASS | real duplicate models = 0 |
| A01—A07 本地化 | PASS | missing zh-CN/en-US proof = 0；合并可验证 key = 2,988 |
| 存档标记抽样 | PASS | 24/24 样本有明确 save/restore marker |
| D10 standalone build | PASS | AgentCore / Systems / Save.Tests 均 0/0 |

统一命令：

```text
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a01_island_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a02_vegetation_batch.py game/Assets/Resources/ContentRegistry
python tools/d15_a03_animal_batch_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a04_acceptance_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a05_content_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a06_catalog_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a07_batch_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a08_content_check.py game/Assets/Resources/ContentRegistry
python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json
```

最终统一结果：`D15-S02 QA PASS: batches=8 samples=24 errors=0 warnings=0`。

## 5. 静态抽样边界

统一 QA 对每批次首/中/尾项进行 JSON 加载、round-trip、owner 和交互/事件入口探针，并确认 Registry root 与嵌套引用可解析。该探针不等价于 Unity Play Mode，也不等价于真实断电恢复；D16-A02 仍需运行 Save.Tests、迁移矩阵和文件级备份/恢复测试。

placeholder、共享 primitive 和 Q1/Q2 资源仍显式保留其状态，不能把本报告解读为最终美术表现通过。D16-A05 负责后续视觉替换与表现抽样。

## 6. D16 放行说明

D15-S02 已解除内容 QA 门禁，D16 可以按以下顺序开始，但 D16 尚未完成：

1. D16-A01 性能、分区流式、LOD、对象池和模拟分级基线；
2. D16-A02 存档迁移、文件级备份、断电恢复和重复结算保护；
3. D16-A03 全局本地化、输入重绑定和可访问性；
4. D16-A04 历史、敏感内容和授权闭包；
5. D16-A05 最终表现替换与可读性/降级抽样；
6. D16-A06 发布技术准备；
7. D16-S01 连续长线全版本验收；
8. D16-S02 最终文档与交接。

在上述门禁全部取得真实证据前，项目不得标记为最终 `PRODUCTION/DONE`。

## 7. 2026-08-11 顺序复验补充

本次最终验收前置复验按生成器与检查器的真实依赖顺序重新执行：先运行会写入植被目录的 A02 生成器，再依次运行 Registry、A01—A08 和 D15-S02 统一检查。A02 是会修改内容文件的生成步骤，不能与 Registry 检查并行；此前并行尝试产生的单次空 JSON 读取属于竞态，不计入 QA 结果，生成完成后的顺序复验未再出现该问题。

顺序复验结果：

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部 PASS；
- D15-S02：`batches=8 samples=24 errors=0 warnings=0`，PASS；
- 结果快照：`tools/results/d15_s02_content_qa_result.json`；
- 本次没有新增系统逻辑、网络、LLM 或数据所有权边界变更。

本次还修正了 A02 生成器的可重复性：`tools/d15_a02_vegetation_batch.py` 现在在生成 13 个项目自有植被 profile 时直接写入 `historical_review=Pending`、候选 evidence、空 warning 列表和 `skip_allowed=false`。因此下一次合法重生成不会擦除 D16-A04 的审查结构；这仍不代表人工历史审查通过。

## 2026-08-12 当前工作区顺序复验

按当前 Registry 和当前工程依赖重新执行 D15-S02：

- Content Registry：2812 entries / 13 prefixes，PASS；
- D15-A01—A08 九个专项检查：全部退出 0；
- D15-S02：`batches=8 samples=24 errors=0 warnings=0`，PASS；
- AgentCore 构建 0/0；Save.Tests 在恢复 Unity 临时资产后构建 0/0；
- 全量工具测试：**48/48 PASS**；
- `tools/results/d15_s02_content_qa_result.json` 为本轮统一 QA 快照。

本轮还修复了 D15-A06 源目录 `asset_ph_ship_fishing` 缺失 LOD 元数据的回归；它不改变 D15 数量、占位状态或任何强逻辑。D15-S02 仍只证明目录和静态内容 QA，不证明 D16 运行时、最终美术或发布。

## 2026-08-12 全量工具回归复验

在当前工作区重新执行全部 D15 专项检查、D15-S02、AgentCore/Save.Tests 构建和 `tools/tests` 全量测试：Content Registry 2812 条 PASS；D15-A01—A08 全部退出 0；D15-S02 为 8 batches / 24 samples / 0 errors / 0 warnings PASS；AgentCore 与 Save.Tests 均 0 warning / 0 error；工具测试 **48/48 PASS**。本轮未改变内容数量、占位状态或 D16 的外部证据边界。

## 2026-08-12 目标命令逐项复验

按目标清单逐项重新执行 Content Registry、D15-A01—A08 九个专项命令以及 D15-S02 统一 QA：

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**52/52 PASS**；
- 本轮未改动内容数量、placeholder 状态、授权结论或 Systems/Agent/契约边界。

这条记录覆盖当前工作区的真实命令输出；历史章节中的 48/48 等数字保留为当时的回归快照。D15-S02 已通过，但 D16-A01—A06 和 S01/S02 仍需独立门禁。

## 2026-08-12 当前命令再次复验

按权威清单再次执行 Content Registry、D15-A01—A08 和 D15-S02：

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01—A08：全部退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 本轮未改变内容数量、placeholder 状态、授权结论或强逻辑数据所有权。

D15-S02 继续保持 **PASS**；D16-A01—A06 的外部真实门禁仍独立有效。

## 2026-08-12 工具测试全集最新统计

`python -m unittest discover -s tools/tests -p "test_*.py" -q` 当前执行结果为 **61/61 PASS**（0 failures，0 errors）。本次增加的是 D16-A01 本机硬件基线的 2 个解析/边界测试；D15 内容数量、placeholder 状态、授权结论和强逻辑数据所有权均未改变。旧章节中的 48/48、52/52、53/53、56/56、58/58、59/59 是各自时间点的历史快照。

## 2026-08-12 D15 专项门禁当前权威复验

本轮重新执行 Content Registry 与 D15-A01—A08 九个专项检查，全部退出码为 0：

- Registry：2,812 entries / 13 prefixes；
- A01 岛屿：50 islands / 8 templates / 10 stream-save-restore samples；
- A02 植物：1,211 entries，抽样与分类校验通过；
- A03 动物与鱼类：300 entries（166 animal / 134 fish）；
- A04 建筑与港口：80 types / 180 variants / 640 instances；
- A05 物品、服装与装饰：500 entries，50 interaction samples；
- A06 船体与模块：20 hulls / 80 modules / 60 functional points / 166 assets；
- A07 角色与宠物：60 crew presets / 500 NPC instances / 12 named variants；
- A08 任务、音频与本地化：5 mainline / 120 side templates / 30 music themes / 8 audio layers / 452 localization keys，孤立键为 0。

同时 `tools/results/d15_s02_content_qa_result.json` 当前为 **8 batches / 24 samples / 0 errors / 0 warnings，PASS**。这只确认 D15 内容门禁没有回归，不改变 D16 外部设备、人工审查、最终资源、跨平台存档和发布门禁的状态。

## 2026-08-12 D15-S02 结果再次确认

本轮重新执行 Content Registry、D15-A01—A08 和 D15-S02 统一 QA：Registry 为 2,812 entries / 13 prefixes；九项专项全部退出码 0；D15-S02 为 8 batches / 24 samples / 0 errors / 0 warnings，结果为 **PASS**。

本轮为只读复验，没有改变内容数量、placeholder、授权结论或 Systems/Agent/契约边界。D15-S02 保持通过，但不替代 D16-A01—A06 的外部真实证据，也不授权启动 D16-S01/S02。

## 2026-08-12 任务清单当前执行结果

按最终验收任务列出的十个命令重新执行：Content Registry 与 D15-A01—A08 全部退出码 0，D15-S02 为 **8 batches / 24 samples / 0 errors / 0 warnings / PASS**；其中 A02 当前植物目录为 1,211 条，A06 为 166 个船舶资产，A08 的中英本地化抽样无缺失。`python -m unittest discover -s tools/tests -p 'test_*.py' -q` 为 **61/61 PASS**。

本轮没有把 placeholder 改成最终资源，没有新增授权/历史结论，没有删除内容，也没有修改 Systems、Agent、contracts 或运行时数据所有权。D15-S02 通过后才允许继续检查 D16，但 D16-A01—A06 的目标设备、人工审查、最终资源、跨平台存档和发布边界仍独立未闭合。

## 2026-08-12 保存/恢复契约修复后的权威回归

- A02 生成器随后复跑，植物目录仍为 1,211 条；新增 `runtime_state` 契约未被生成流程擦除；
- Content Registry：2,812 entries / 13 prefixes，PASS；D15-A01—A08 全部退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，PASS；9/9 指定保存/恢复样本契约 verified，missing=0；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**63/63 PASS**，0 failures、0 errors。

63/63 是本轮当前工作区统计；此前 61/61 为历史快照。D16 外部设备、人工审查、最终资源、跨平台存档和发布门禁仍独立未闭合。

## 2026-08-12 D15-S02 保存/恢复证据修复

复核发现，仅在 `test_cases` 中写入 `save-restore` 不能证明运行时保存/恢复；因此为任务清单指定的 9 个抽样条目补齐结构化 `runtime_state` 契约，并在统一 QA 中拒绝 marker-only 证据：

- `quest_d15_side_039`、`quest_d15_side_120`：`runtime_state.mode=runtime_persisted`，明确绑定现有 `SaveSystem.SaveD12Runtime(D12RuntimeSnapshot)` / `SaveSystem.LoadD12Runtime()`，状态路径为 `D12RuntimeSnapshot.DynamicQuestLog`；
- 其余 7 个资源/定义：`runtime_state.mode=definition_only`、`scope=immutable_definition`，明确说明该 ContentRegistry 条目是不可变定义/资源元数据，不作为可变运行时状态序列化；
- QA 新增反例：只有 `D15-S02-SAVE-RESTORE-*` 测试字符串而无结构化契约时直接失败；
- 结果：9/9 contracts verified，missing=0；D15-S02 仍为 `8 batches / 24 samples / 0 errors / 0 warnings / PASS`；专项 QA 单测 5/5 PASS。

该修复没有新增运行时数值、没有改变系统数据所有权，也没有把目录定义误报为最终资源或已完成的跨平台存档验收。

## 2026-08-12 当前工作区 D15 全套复验

本轮在 D16 PlayMode 回归后重新执行权威 D15 命令清单，所有退出码均为 0：

- Content Registry：2,812 entries / 13 prefixes；
- D15-A01：50 islands / 8 templates / 10 stream-save-restore samples；
- D15-A02：1,211 plants；
- D15-A03：300 entries（166 animal / 134 fish）；
- D15-A04：80 types / 180 variants / 640 instances；
- D15-A05：200 functional / 300 decorative / 500 total，50 interaction samples；
- D15-A06：20 hulls / 80 modules / 60 functional points / 166 assets；
- D15-A07：60 crew presets / 500 NPC instances / 12 named variants；
- D15-A08：5 mainline / 120 side templates / 30 music themes / 8 audio layers / 452 localization keys；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，**PASS**；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**68/68 PASS**。

本轮为复验，没有修改内容数量、placeholder 状态、授权结论、Systems/Agent/contracts 或运行时数据所有权。D15-S02 保持 PASS；D16 外部设备、人工审核、最终资源、跨平台存档和发布门禁仍独立未闭合。

## 2026-08-12 D16-A02 矩阵工具回归后的最新统计

新增严格存档覆盖矩阵的 3 个工具单测后，全量 `tools/tests` 当前为 **66/66 PASS**。63/63、61/61 和更早统计均为历史快照；D15 内容、placeholder 状态、授权结论与 D16 外部门禁边界不变。

## 2026-08-12 当前权威命令复验（D16-A03 后）

- Save.Tests 在恢复 Unity 临时依赖索引后构建 0 warning / 0 error；
- Content Registry：2,812 entries / 13 prefixes；
- D15-A01—A08：全部专项命令退出码 0；
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，**PASS**；
- `runtime_save_restore` 9/9 verified，A08 中英 locale 抽样无缺失；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**69/69 PASS**；
- 本轮没有改变内容数量、placeholder 状态、授权/历史决定或 Systems/Agent/contracts 数据所有权。

D15-S02 的 PASS 只证明内容目录与静态 QA 门禁通过；D16-A01—A06 的目标设备、人工审核、最终资产、跨平台存档和发布边界仍需独立真实证据。

## 2026-08-12 D10/D15 工程依赖与专项清单最新复验

- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：0 warning / 0 error。
- Unity 刷新后首次执行 `dotnet build game/Save.Tests.csproj --no-restore` 仅因临时 `game/Temp/obj/Save.Tests/project.assets.json` 缺失而失败；受控执行 `dotnet restore game/Save.Tests.csproj --ignore-failed-sources` 后，Save.Tests 再次构建为 0 warning / 0 error。该过程没有复制 D10 类型、缩小测试范围或改变数据所有权。
- Content Registry 与 D15-A01—A08 十项专项命令全部退出码 0；当前关键数量仍为 2,812 entries / 13 prefixes、50 岛屿、1,211 植物、300 动物/鱼类、500 物品/服装/装饰、166 船舶资产、60 船员预设/500 NPC、452 本地化键。
- D15-S02：8 batches / 24 samples / 0 errors / 0 warnings，**PASS**；工具测试全集：**69/69 PASS**。

本轮是工程与内容门禁复验，没有修改内容数量、placeholder、历史/授权决定或 Systems/Agent/contracts 数据所有权。D15-S02 保持 PASS；D16-A01—A06 的目标设备、人工审核、最终资产、跨平台存档和发布门禁仍独立未闭合。

## 2026-08-12 总纲顺序复验（追加）

- `python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result_current_20260812.json`：**8 batches / 24 samples / 0 errors / 0 warnings，PASS**。
- D15-S02 当前仍以真实 Registry 检查为依据；本轮没有删除条目、降低 QA 范围、升级 placeholder、伪造历史/授权或修改内容所有权。
- D16-A01—A06 的目标设备、人工审核、最终资产、跨平台存档和发布门禁仍需独立证据，不能由 D15-S02 PASS 代替。

## 2026-08-12 全量工具回归（追加）

- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**71/71 PASS**，用时 96.4 秒。
- 回归输出包含 D15-A06 catalog PASS（20 hulls / 80 modules / 60 functional points / 166 assets）及 assembly smoke PASS；未出现新的内容 QA、存档、D16 审计或安装器回归。
- 该结果只证明工具链当前可重复通过，不改变 D16 目标设备、人工审核、授权素材和跨平台证据的未完成状态。
## 2026-08-12 总纲顺序下的 D15 权威复验

在 D10 工程依赖通过后，按总任务文件重新执行 Content Registry、D15-A01—A08 全部专项命令，以及统一 QA：

- Content Registry：2,812 entries / 13 prefixes，PASS；
- D15-A01：50 islands / 8 templates / 10 stream-save-restore samples；
- D15-A02：1,211 plants；
- D15-A03：300 entries（166 animal / 134 fish）；
- D15-A04：80 types / 180 variants / 640 instances；
- D15-A05：500 entries，50 interaction samples；
- D15-A06：20 hulls / 80 modules / 60 functional points / 166 assets；
- D15-A07：60 crew presets / 500 NPC instances / 12 named variants；
- D15-A08：5 mainline / 120 side templates / 30 music themes / 8 audio layers，locale 缺失为 0；
- `tools/results/d15_s02_content_qa_result.json`：8 batches / 24 samples / 0 errors / 0 warnings，**PASS**。

本轮没有删除内容、缩小 QA 范围、升级 placeholder、伪造历史/授权或改变 Systems、Agent、contracts 与运行时存档数据所有权。D15-S02 通过只解除 D16 的内容前置，不代表 D16-A01—A06 或 D16-S01/S02 完成。

### 植被语义抽查

对当前两个 canonical 植物样本做了内容语义复核：`plant_red_mangrove` 的 `habitat_profile.primary=mangrove` 且存在于 `valid_habitats`，风区为 `mangrove_low`；`plant_shallow_seaweed` 的 `habitat_profile.primary=nearshore` 且存在于 `valid_habitats`，风区为 `nearshore`。该抽查没有修改数据，只确认 A02 生成器修复后的语义与 QA 规则一致。
