# D16-A05 最终表现 QA 报告

## 最终结论

状态：**IN_PROGRESS，资产登记子门禁已通过，最终表现替换未完成**。

D15-S02 已通过，D16-A04 的机器结构/授权字段已闭合，但仍有人工 Pending 审查；当前完整 `game` 目录有 2264 条 placeholder，继续阻断最终表现替换。因此本次不把 placeholder 标记为最终表现，也不生成虚假材质/灯光/性能验收。

## 门禁复核

命令：

python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json

历史结果：D15_S02_QA_EXIT=1；当前顺序复验结果：D15-S02 PASS（0 errors、0 warnings）。

直接阻断：

- D15-A06 的 166 个 asset_ 资源缺 historical_review、review_evidence_refs、content_warning_ids、skip_allowed；
- D15-A02 有 2 条生境字段冲突；
- D15-A07 有 18 次引用指向 12 个未定义宠物外观 ID；
- D15-A08 缺测试/LOD-N/A/资源状态元数据；
- A01—A07 有 2536 个 display key 缺可验证本地化包；
- 24 个抽样中 9 个缺明确存档/恢复标记；
- D16-A04 已将 1675 时间线、势力、宗教、社群、奴役/强迫劳动、捕鲸、珍稀动物、赌场、土地、身份和外部资产列为部分或待核查；A06 授权/审查证据仍未闭合。

## 已读取的表现基线

### 船体、港口、岛屿和水体

- D7MaterialPolishController 复制材质实例、尽量保留纹理，并对缺失/紫色材质使用可读调色板；这是运行时保真兜底，不是最终资源替换。
- HarborPresentationController 已有港口灯光、雾、海面波动、泡沫、灯笼、标识和相机基础配置。
- HarborCollisionController 已有少量可审计的地面、码头、跳板、岸线和船体碰撞边界。
- D15-A01 岛屿目录有流式 cell、LOD 和保存键声明，但没有通过最终美术资源和场景级性能验收。

### 植被、天气、动物和宠物

- D9WeatherSeaState 提供晴/雨/风暴海况快照和确定性影响；表现层只能读取天气，不得改写天气或生态结果。
- D15-A02/A03/A07 仍包含明确的 placeholder/共享占位状态；未授权或未完成审查的资源不能进入最终构建。
- 本次未对风场、动物/宠物动画、群落远景、材质替换或生态表现做运行时抽样。

### 角色、服装、牌局、奖励和战斗

- D10 外观/身份使用稳定 outfit ID、AppearanceSystem owner 和身份历史；不可因最终表现替换改变实体 ID、身份或法律/关系结果。
- D12 牌局与社群逻辑由 MiniGameSystem/CommunityTradeSystem 裁决；最终 UI 可读性不能修改牌面、胜负、债务或信任。
- D14 特殊奖励、战斗、法律和宗教表现通过 owner 事实接入；本次不修改奖励、战斗、法律、声望或关系数值。

### 音频和 UI

- D7/D9 当前有环境、脚步、船舶、事件和天气 fallback 音频；D15-A08 正式音频仍有 placeholder，且 D15 QA 报告缺 test_cases/LOD-N/A。
- 当前 UI 有相机、港口暂停、静态中英文案和基础 HUD，但不存在本任务所需的全批次最终表现可读性验收记录。

## 最终表现抽样矩阵

| 表现范围 | 当前状态 | 必须在门禁恢复后验证 |
|---|---|---|
| 船体材质、灯光、阴影、碰撞、锚点 | BLOCKED | D15-A06 授权/审查、LOD、碰撞/锚点和真实资源加载 |
| 港口材质、光照、雾、泡沫、相机、碰撞 | PARTIAL | 至少一个港口代表和一个低画质档，验证交互不靠视觉假象 |
| 50 座岛屿与分区表现 | BLOCKED | 每类地貌代表、流式/保存/恢复、远近 LOD 和可读 UI |
| 植被材质、风场、群落代理 | BLOCKED | 各生境代表、风暴/晴天、采集碰撞和资源 owner |
| 水体、天气、能见度和海流表现 | PARTIAL | 读取 D9 快照，三种天气、阴影/反射、降级配置和性能记录 |
| 角色、动物、宠物动画和碰撞 | BLOCKED | 授权/外观引用闭包、角色/宠物代表、行为接管和碰撞 |
| 服装与身份表达 | BLOCKED | outfit/asset 授权、文化审查、长文本和 UI 可读性 |
| 牌局和社群 UI | BLOCKED | 牌面/金额/债务/警告不截断，退出和敏感内容路径可读 |
| 特殊奖励和战斗表现 | BLOCKED | 资源授权、碰撞/锚点、伤害/奖励 owner 不被表现层越权 |
| 音频、混音、动态音乐、字幕 | BLOCKED | D15-A08 二进制/placeholder 状态、LOD-N/A、混音、触发、字幕和跳过 |
| 画质降级 | NOT RUN | 低/中/高配置只改变表现，不删除逻辑、事件或存档 |

## 授权和审查原则

- 未取得商业使用和修改许可的资源不得进入正式构建。
- D16-A04 的 Pending/Rejected 资源不得进入正式逻辑；ApprovedWithWarning 必须保留警告、表现等级和允许的跳过路径。
- 占位资源可用于目录/读取器测试，但必须保持 asset_state=placeholder，不得标记为 verified/Q3。
- 资产问题交给 D15/AssetRegistry；碰撞、锚点和交互问题交给对应 Ship/Building/World owner；审查意见不改玩法数值。

## 本次未做的工作

- 未替换船体、港口、岛屿、植被、水体、天气、角色、动物、宠物、服装、牌局、奖励或战斗最终资产；
- 未修改实体 ID、强逻辑公式、数据所有权、Systems 或 contracts；
- 未将未授权/占位资源放入正式构建；
- 未执行 Unity 场景级材质、灯光、阴影、风场、混音、动画、碰撞、相机和 UI 抽样；
- 未加入网络或 LLM；
- 未删除问题资源来掩盖 QA。

## 重新开工门槛

1. D15-S02 错误归零并通过全量内容 QA；
2. D16-A04 授权/审查阻塞项关闭，或每条保留明确的 Pending 待核查状态并禁止正式构建；
3. 先完成可授权的最终资源替换，再按船体、港口、岛屿、植被、水体、天气、角色、动物、宠物、服装、牌局、奖励和战斗分组抽样；
4. 记录材质、光照、阴影、风场、混音、动态音乐、碰撞、锚点、相机、UI 可读性和低画质配置；
5. 重新运行 Content Registry、D15 QA、相关 owner 测试和 Unity 场景验证。

## 2026-08-11 门禁复验更新

D15-S02 已通过，D16-A04 的目录字段阻断也已关闭。D16-A05 当前状态更新为 **PENDING**：D15 中仍有明确的 placeholder/共享占位资源，尚未进行最终资源替换、Unity 场景表现抽样、碰撞/锚点/相机/动画联动验收，也没有把占位模型标记为最终美术。

## 2026-08-11 资产门禁审计更新

当前状态：**IN_PROGRESS，最终表现替换未开始**。

## 2026-08-11 审计复核更新

`tools/d16_a05_asset_audit.py` 已重新扫描：可视资源 2590 条，其中 integrated 328、placeholder 2262；非法状态 0；仍有 2 条 integrated 记录缺少 `test_cases`。本机 Assets 目录现有 2 个 FBX、159 个 OBJ、6 个 PNG，音频二进制仍未形成最终资产闭包。A05 继续 IN_PROGRESS，不把程序生成几何或 placeholder 标为最终表现。

新增 `tools/d16_a05_asset_audit.py`，当前扫描 2590 条可视资源登记：`placeholder=2262`、`integrated=328`；仍有 2 条 integrated 记录缺 test_cases，因此 A05 保持阻断。实际 Content 目录目前只有 159 个 OBJ，未发现 FBX、贴图、音频二进制等完整最终资源闭包。

复核输出：`tools/results/d16_a05_asset_audit.json`。审计器只报告状态，不会把 placeholder 改成 final/verified；审计器单测已通过。

## 2026-08-11 当前审计复验

A05 重新执行结果：可视登记 `2590` 条，其中 `integrated=328`、`placeholder=2262`、`data_only=125`；非法元数据 `0`，integrated 缺失视觉测试元数据 `0`。当前唯一 A05 阻断仍是 `placeholder_assets_remain`。本轮没有修改任何 `asset_state`，也没有把 Unity 原生几何、程序生成模型或共享材质宣称为最终表现。

## 2026-08-12 占位资源可追踪性复验

为场景实际使用的 `asset_ph_crew_male` 与 `asset_ph_palm_leaves` 补齐资源源路径、运行时 Prefab 路径、`asset_state=placeholder`、`production_stage=prototype_placeholder`、LOD profile 和加载/引用/占位状态/存档恢复测试用例，并补齐各自的 Quaternius CC0 authorization。没有替换模型、材质或场景表现。

最新 A05 审计：可视登记 `2592` 条、`integrated=328`、`placeholder=2264`、`data_only=125`、非法状态 `0`、集成资源缺 LOD/测试字段 `0`；唯一阻断仍为 `placeholder_assets_remain`。D15-S02 继续为 8 batches / 24 samples / 0 errors / 0 warnings PASS。

## 2026-08-12 A05 源目录元数据回归修复

最新扫描发现 `game/Assets/Content/D15/A06/entries/assets/asset_ph_ship_fishing.json` 的源目录副本缺少 `lod_profile`，但 Resources 注册表副本已有有效的 source/generated LOD 声明。已补齐源目录字段并重新运行 D15-A06 与 D16-A05 检查。

结果：D15-A06 catalog PASS；D16-A05 `visual_entry_count=2918`、`integrated=654`、`placeholder=2264`、`invalid_metadata_count=0`、`integrated_missing_visual_metadata_count=0`，唯一当前登记阻断为 `placeholder_assets_remain`。这只修复了元数据一致性，不代表最终船体、材质、动画、碰撞、锚点、相机或音频资源已经完成。

## 2026-08-12 完整项目根目录资产审计口径复验

本轮以完整 `game` 目录作为 D16-A05 的登记根目录，并以 `game/Assets` 检查实际源文件；没有把 D15 Registry 的条目数误用为 A05 的完整项目条目数。权威命令与结果如下：

```text
python tools/d16_a05_asset_audit.py game game/Assets --json-out tools/results/d16_a05_asset_audit.json
  visual_entry_count=2918
  data_only=125, integrated=654, placeholder=2264
  invalid_metadata_count=0
  integrated_missing_visual_metadata_count=0
  source_files: FBX=2, OBJ=159, PNG=6, audio=0
  blocking_reasons=placeholder_assets_remain
```

审计结果与 `tools/results/d16_a05_asset_audit.json` 一致。2264 条 placeholder 仍保持 placeholder 状态；没有替换或虚构最终资源，也没有把程序生成几何、共享材质或占位模型升级为最终表现。A05 仍为 **IN_PROGRESS**，必须先完成真实授权的最终资源替换和 Unity 场景表现抽样。

## 2026-08-12 权威审计再次复验

`d16_a05_asset_audit.py` 在完整 `game` 根目录再次输出 2,918 条可视资源：data_only 125、integrated 654、placeholder 2,264；非法元数据 0，集成资源缺视觉元数据 0。机器阻断仍为 `placeholder_assets_remain`，本轮没有升级任何占位资源，A05 继续 **IN_PROGRESS**。

## 2026-08-12 占位资源替换队列

新增 `tools/d16_a05_replacement_queue.py`，只读取完整 `game` 根目录的 placeholder 登记，生成：

- `tools/results/d16_a05_replacement_queue.json`；
- `tools/results/d16_a05_replacement_queue.md`。

当前真实结果：`status=BLOCKED`、placeholder **2,264**、**51** 个批次（每批最多 50 条）。类别为 animal 168、asset 57、building 80、crew 60、fish 134、island 50、item 335、outfit 165、pet 4、plant 1,211；要求统计为最终资源文件 2,262、运行时 Prefab/消费者 2,262、替换占位源与 Prefab 2、历史/内容审查批准 15。该队列不写 Registry、不改变 `asset_state`，每条记录保留来源、授权、审查、LOD、测试和当前资源路径，只有取得真实授权最终资源后才能执行替换。

专项测试 `python -m unittest tools.tests.test_d16_a05_replacement_queue -v`：**2/2 PASS**。A05 总体仍 **IN_PROGRESS**。

## 2026-08-12 A05 队列刷新

重新运行完整 `game` 根目录审计和占位替换队列：可视资源 **2,918** 条，其中 data_only **125**、integrated **654**、placeholder **2,264**；非法元数据 0，集成资源视觉元数据缺口 0；替换队列 **51** 批，机器阻断仍为 `placeholder_assets_remain`。

本轮没有替换模型、材质、Prefab 或场景引用，没有把程序生成几何/纯色模型升级为最终资产；A05 继续 **IN_PROGRESS**。相关 A04/A06 审计与 A05 队列专项测试为 **22/22 PASS**，但真实最终资源、来源/授权和 Unity 场景表现抽样仍需后续输入。

## 2026-08-12 声明路径闭包审计

为避免最终资源替换时出现“目录有记录、运行时路径已断”的假接入，A05 审计新增对登记中声明路径的只读存在性检查。当前完整 `game` 根目录结果为：

- `resource_path`：334/334 存在；
- `runtime_prefab_path`：2/2 存在；
- `model_path`：320/320 存在；
- `audio_file_ref`：38/38 存在；
- `source_path`：3/3 存在（Markdown 锚点按源文件检查）；
- 声明路径断链：0。

该结果只关闭“已声明入口的文件断链”机器子门禁，不代表资源已经获得最终授权、完成历史审查或达到最终美术质量。当前 A05 仍被 `placeholder_assets_remain` 阻断，2264 条 placeholder 不得升级为最终资源。

全量工具回归 `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**69/69 PASS**（97.745s）。

## 2026-08-12 当前最终资源替换包

- 当前只读队列：`tools/results/d16_a05_replacement_queue_current.json` 与 `docs/game/design/common/D16_A05_REPLACEMENT_QUEUE_CURRENT.md`。
- 队列状态 **BLOCKED**：2,264 个 placeholder，分 51 批（每批最多 50 条）；类别包括 animal 168、asset 57、building 80、crew 60、fish 134、island 50、item 335、outfit 165、pet 4、plant 1,211。
- 当前缺口统计：最终资源文件 2,262、运行时 Prefab/消费者 2,262、需替换占位源与 Prefab 2、需历史/内容审查批准 15。
- 队列不替换资源、不修改 `asset_state`，不把程序几何或纯色模型升级为最终美术；A05 继续 **IN_PROGRESS**，只有真实来源/授权资源到位后才能分批接入和验收。

## 2026-08-12 垂直切片优先队列

为让后续真实资源接入先验证当前最影响可玩性的路径，`tools/d16_a05_replacement_queue.py` 增加了只读的 `vertical_slice_first` 队列范围。优先表按以下顺序覆盖：真人角色、船体、圣玛格丽特码头/小艇码头、圣玛格丽特岛。

- 当前 placeholder 队列实际优先条目：`asset_ph_crew_male`、`building_saint_marys_dock`、`building_small_boat_pier`、`island_saint_marys`；
- `asset_ph_ship_fishing` 已有 CC0 来源和 `asset_state=integrated` 的注册记录，因此不进入 placeholder 替换队列；它仍需在后续船控、上下船、碰撞和表现验收中单独验证；
- 最新队列：2,264 个 placeholder、52 批；第一批为上述垂直切片条目，其余按类别与稳定 ID 排序；
- 角色姿态、脚底落地、地面碰撞、上下船动作和镜头不由该队列宣称完成，仍分别属于 D17 运行时/动画/资源接入门禁。

专项测试 `python -m unittest tools.tests.test_d16_a05_replacement_queue -v`：**3/3 PASS**。队列仍为只读 BLOCKED，不批准历史、不替换资源、不修改 `asset_state`。

本轮全量工具回归 `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**70/70 PASS**（98.016s）；A05 权威审计仍为 visual 2,918、data_only 125、integrated 654、placeholder 2,264、非法元数据 0、声明路径断链 0，机器阻断仍为 `placeholder_assets_remain`。
## 2026-08-12 总纲顺序下的 A05 当前证据

完整项目审计为 2,918 visual、data_only 125、integrated 654、placeholder 2,264；声明路径断链 0，非法元数据 0，集成资源视觉元数据缺口 0。替换队列 52 批，第一批为角色、码头、小艇码头和圣玛格丽特岛。没有替换或升级任何 placeholder；最终资源、授权、动画、碰撞、锚点和场景表现仍未完成，A05 保持 **IN_PROGRESS**。

## 2026-08-13 来源引用闭包修正

- A05 队列现在验证来源、审查和授权引用中的本地文件；仓库根目录文档引用按正确根目录解析，逻辑证据 ID 和远程 URL 不被误报为缺失文件。
- 修正 `plant_conch_bed` 与 `plant_shallow_seaweed` 两处许可证文档短路径；队列当前来源/审查引用断链为 0。
- D15-S02 QA 8/8 批次、24/24 抽样通过；A04/A05/A06 聚焦 28/28，工具全量 84/84。没有接入最终授权模型、材质、动画、碰撞、锚点或音频。

结论仍为：A05 路径/元数据子门禁通过，最终资源门禁 **IN_PROGRESS**。

## 2026-08-13 最终资源包预检

- 新增 A05 只读资源包预检入口，要求最终文件、运行时消费者、授权和人工审核、LOD/碰撞/锚点、测试用例及真实原始材料全部齐备。
- 专项测试 **4/4 PASS**，工具全量 **88/88 PASS**；当前没有可接入的最终资源包，2,264 个 placeholder 仍保持原状态。

## 2026-08-13 A04 逐条复核包

- 新增 `tools/d16_a04_review_packet.py`，输出 267 条 Pending 的逐条人工填写材料，12 批、引用缺口 0、267/267 条已有本地文件型复核入口。
- 当前状态仍为 `HUMAN_DECISION_REQUIRED`，不是人工批准；模板全部保持 `Pending`，生成过程不修改 Registry。
- 专项测试 **4/4 PASS**；A04、D16-S01/S02、正式 D17 和 D18+ 状态不变。

### 当前回归

- 工具全量回归 **96/96 PASS**；复核包保持 `HUMAN_DECISION_REQUIRED`，D16-S01 预检保持 `BLOCKED`，没有自动批准或 Registry 写入。

## 2026-08-13 A05 候选资源包补充

- 第一批 Kenney CC0 候选资源已完成来源、哈希和包内许可证留痕，候选清单为 `tools/results/d16_a05_candidate_packet_01_20260813.json`。
- 角色、码头、泊位、岛屿自然物和小型船文件均保存在候选目录；候选状态为 `CANDIDATE_PENDING_REVIEW`，尚未进入 Registry、Prefab 或场景引用。
- 候选专项 **2/2 PASS**，全量工具 **98/98 PASS**；画面最终品质、LOD、碰撞、锚点、动画和运行时表现仍需责任审核与正式接入验收。
- Unity 导入级审计已确认 7/7 候选 FBX 有 ModelImporter、Mesh 和正 bounds；角色包含 27 个 Generic 动画片段。该结果只证明文件可读，不能替代最终表现验收。

### 最终本机复核

- 工具全量 **100/100 PASS**；Unity 候选导入 **7/7 PASS**；候选包/导入证据专项 **4/4 PASS**。
- 候选资源仍未进入 Registry、Prefab 或场景；A05 最终资源、人工审核、LOD/碰撞/锚点和运行时表现门禁继续 Pending。
