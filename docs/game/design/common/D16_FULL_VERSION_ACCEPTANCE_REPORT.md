# D16-S01 全版本长线验收报告

## 任务状态

- 任务编号：D16-S01
- 日期：2026-08-12
- 当前状态：**BLOCKED / NOT STARTED**
- 验收结论：未执行全版本长线流程，不得标记 PASS
- 外部动作：未执行 Steam 上传、发布或外部消息发送

本文件是 D16-S01 的前置门禁记录，不是一次虚构的运行结果。按照总纲，只有 D16-A01—A06 全部取得真实 PASS 后，才允许从新游戏开始完整流程。

## 前置门禁快照

| 门禁 | 当前状态 | 当前证据 | 未关闭原因 |
|---|---|---|---|
| D15-S02 | PASS | 8 batches / 24 samples / 0 errors / 0 warnings | 已满足 D16 入口 |
| D16-A01 | IN_PROGRESS | 本机 Unity/RTX 4070 Ti 窗口和 PlayMode 回归 | 缺目标/低端设备、真实大场景、最终资源和完整 GPU/内存证据 |
| D16-A02 | IN_PROGRESS | Save.Tests 303/303、档案矩阵 10/10 类别、故障注入 6/6、100 次长线 | 缺物理断电等价、跨平台恢复和完整生产矩阵 |
| D16-A03 | IN_PROGRESS | 本机 PlayMode/输入/字体/模态回归 | 缺真实手柄、目标设备、目标分辨率和人工可访问性证据 |
| D16-A04 | IN_PROGRESS | 3139 条记录，结构错误 0，授权记录 2918/2918 | 267 条 Pending 仍需责任审核者逐条决定 |
| D16-A05 | IN_PROGRESS | 2918 条可视登记，非法元数据 0 | 2264 个 placeholder 尚未由真实授权最终资源替换 |
| D16-A06 | IN_PROGRESS | 19/19 技术检查，正式便携安装器烟测已留证 | 缺目标设备、原生崩溃、最终隐私人工审查；Steam 未授权 |

权威当前发布审计：`tools/results/d16_a06_release_audit_current.json`，发布清单为 `blocked`，保留 7 条项目级阻断。A01—A06 未全部 PASS，因此本报告不执行下列流程：

```text
创建角色 → 初始船 → 航行 → 钓鱼 → 贸易 → 船员关系
→ 探索寻宝 → 牌局/社群交易 → 资产/金融 → 种植/岛屿经营
→ 战斗/特殊奖励 → 保存 → 迁移 → 恢复
```

## 2026-08-12 当前本机回归补充

- Unity `6000.0.80f1` 图形 PlayMode 全量结果：`tools/results/d16_full_playmode_current_20260812.xml`，**53/53 PASS**，0 failed / 0 skipped / 0 inconclusive；日志 `tools/results/d16_full_playmode_current_20260812.log` 记录 Test Runner 自然完成并以 code 0 退出。
- AgentCore 与 Save.Tests 当前构建均为 0 warning / 0 error；工具全量回归为 71/71 PASS；D15-S02 当前复验为 8 batches / 24 samples / 0 errors / 0 warnings。
- 这些是本机代码和内容门禁证据，不是完整版本流程证据；没有执行 D16-S01 的新游戏到恢复全链路，也没有关闭目标设备、人工审核、最终资源、断电/跨平台、原生崩溃/隐私等项目级阻断。

## 需要执行的验收范围（门禁解除后）

D16-S01 重新启动时必须按固定顺序记录：

1. 新建角色和初始船；
2. 航行、钓鱼、贸易、船员关系、探索寻宝；
3. 牌局/社群交易、资产/金融、种植/岛屿经营、战斗/特殊奖励；
4. 保存、迁移、恢复，并验证长线存档、备份恢复、重复结算和断电恢复；
5. 同时覆盖本地化、可访问性、敏感内容警告/跳过、性能、运行时交互和 Agent/Systems/contracts 边界；
6. 记录原始日志、失败样本、设备信息、版本和每次恢复后的事实一致性。

只有上述完整流程取得可复核的真实结果，且没有 Critical/High 阻塞，才能把本文件更新为 PASS。

## 2026-08-12 最新证据刷新

- 当前本机 Unity PlayMode 全量（含 D17 本机回归夹具）为 `tools/results/d17_full_after_s02_local_20260812.xml`：**60/60 PASS**；
- 当前本机工具测试为 **72/72 PASS**；`Assembly-CSharp`、`AgentCore`、`Save.Tests`、`D16.PlayMode.Tests` 构建均为 0 warning / 0 error；
- D17-S02 本机连续夹具 `tools/results/d17_s02_local_story_ten_cycles_20260812.xml` 为 **1/1 PASS**，日志明确记录 10/10 次；
- 以上结果不覆盖 D16-S01 所要求的全版本长线领域故事，也不替代 A01—A06 的目标设备、人工审核、最终资源、断电/跨平台和原生崩溃/隐私证据。

因此本报告当前状态仍为 **BLOCKED / NOT STARTED**，不能改为 PASS。

## 数据所有权与安全边界

- 本轮只新增前置验收记录，没有修改领域数据、存档、contracts、Systems、AgentCore、历史审核、授权或 `asset_state`。
- 本文件不批准 Pending 条目、不把 placeholder 升级为最终资产，也不把本机回归扩大解释为目标设备证据。
- D16-S02 继续等待本报告真实 PASS；D17 不得借此提前启动正式验收。

## 下游任务

1. 完成 D16-A01—A06 的真实缺口；
2. 重新运行 A06 发布审计，确认六项均为 PASS；
3. 执行本报告中的完整长线流程并保存原始证据；
4. 更新 `docs/game/design/common/handoffs/D16/D16-S01_HANDOFF.md` 和当日记忆；
5. 仅在 S01 PASS 后进入 D16-S02。

## 2026-08-12 当前最终审计

- 本机 Unity 6 垂直切片全量回归：**60/60 PASS**；工具测试：**77/77 PASS**；关键 C# 工程构建：0 warning / 0 error；
- D16-A04/A05/A06 当前只读审计分别为 `Pending=267`、`placeholder=2264`、本机技术准备 `19/19 PASS`；
- `tools/results/d16_external_gate_audit_current_20260812.json` 仍报告 A01—A06 六个真实外部 evidence envelope 缺失，整体 **BLOCKED**；
- D16-S01 仍 **BLOCKED / NOT STARTED**，本机垂直切片不构成全版本长线验收。

## 2026-08-13 当前门禁刷新

- 全量工具测试：**79/79 PASS**；D16-A04/A05/A06 只读审计已生成 20260813 快照，分别保持 Pending 267、placeholder 2,264、A06 本机技术准备 19/19。
- A03 物理设备预检发现 Xbox Wireless Controller，但 XInput API 可用且用户 0—3 均返回 `1167`，当前状态 `NO_ACTIVE_XINPUT_USER`；预检仍为 `IN_PROGRESS/PENDING_MANUAL`，不是 PASS。
- 六个 D16 外部证据 envelope 仍缺失，外部审计继续 `BLOCKED`；D16-S01 不启动。

## 2026-08-13 Unity 6 回归证据补充

- D17 本机十循环聚焦结果：**1/1 PASS**，10/10 cycle 全部通过；
- D17 本机全量 PlayMode：**60/60 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 本轮重新生成的 D16-A04/A05/A06 快照仍为 Pending 267、placeholder 2,264、A06 技术准备 19/19，六个外部 evidence envelope 仍缺失；
- 因此本报告仍为 **BLOCKED / NOT STARTED**，本机回归不能替代 D16-S01 所需的完整版本长线故事和外部真实性证据。

## 2026-08-13 本机工程复验

- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**79/79 PASS**；
- `Assembly-CSharp`、`Save.Tests`、`AgentCore`、`Whalebone.Systems` 和 `D16.PlayMode.Tests` 均构建为 **0 warning / 0 error**；
- `git diff --check` 通过；输出中的 LF/CRLF 提示是工作区换行提示，不是内容错误；
- 本次没有在该旧检查点重新生成 Unity 6 PlayMode 结果；当前实际 Unity 6 编辑器路径已确认是 `C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`，后续保存事务回归已补充新的 60/60 结果；
- A01—A06 的真实目标设备、跨平台/断电、人工可访问性、人工历史/授权审查、最终资源和目标发布/隐私证据仍缺失，外部审计仍为 `BLOCKED`。

这次复验只确认当前源码、工具和可独立构建的本机子门禁没有回退；它不改变 D16-S01/S02 或后续 D17/D18+ 的正式状态。

## 2026-08-12 A03 物理设备预检后的当前审计

- 工具全量回归更新为 **79/79 PASS**；新增探针只记录当前 Windows PnP 设备、显示配置和 GPU，不发送输入、不产生人工 PASS。
- 预检识别到 `Xbox Wireless Controller`、`Bluetooth LE XINPUT compatible input device`、`3840x2160` 和 `NVIDIA GeForce RTX 4070 Ti`；摘要仍为 `IN_PROGRESS/PENDING_MANUAL`。
- A01—A06 外部证据 envelope 仍缺失，D16-S01 继续 `BLOCKED / NOT STARTED`，不把物理设备存在性扩大解释为 A03 通过。

## 2026-08-13 Unity 6 路径更正与角色物理最终回归

- 本机 Unity 6 实际可执行文件为 `C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`；此前“未安装 Unity 6”的表述是路径检查错误。
- 最终 Unity 聚焦十循环：`game/tools/results/d17_s02_local_story_ten_cycles_20260813_final.xml`，**1/1 PASS**，10/10 cycle 全部通过。
- 最终 Unity 全量 PlayMode：`game/tools/results/d17_playmode_full_final_20260813.xml`，**60/60 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 本轮针对 D2 角色世界尺寸、D3 港口安全出生、D5 根节点缩放覆盖、跳板步高与动作误判完成修复；该结果只证明本机代码基线，不构成 D16-S01 长线故事或 A01—A06 外部证据。
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q` 为 **79/79 PASS**；相关 Unity/.NET 工程构建为 0 warning / 0 error。

因此本报告仍为 **BLOCKED / NOT STARTED**；A04/A05/A06 与其他外部门禁未被本机回归关闭。

## 2026-08-13 当前 Unity 路径下的 Save 回归复核

后续路径核对已确认本机 Unity 6 编辑器为 `C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`。在 SaveSystem 多表事务改动后重新执行完整 D17 PlayMode，结果为 `game/tools/results/d17_playmode_full_after_save_transaction_20260813.xml`：**60/60 PASS**，failed/inconclusive/skipped 均为 0。

该结果取代本文件较早的“本轮未生成 Unity 结果”描述，但只证明当前 Windows/Unity 本机回归；A01—A06 外部 evidence envelope、A04 人工审核、A05 最终授权资源、目标设备和正式 D16-S01/S02 仍未完成，整体状态继续 **BLOCKED / NOT STARTED**。

## 2026-08-13 A05 审计入口回归补充

- A05 CLI 现在会把嵌套 `ContentRegistry` 参数归一到完整 Unity 项目根目录，避免静默局部扫描；嵌套路径实测仍得到 2,918 visual、2,264 placeholder、声明路径断链 0。
- 工具全量回归为 **80/80 PASS**，A04/A05/A06 聚焦回归为 **26/26 PASS**；本轮没有修改 Registry、`asset_state` 或 Steam 状态。
- 该增量只提高审计证据可靠性，不关闭 A04 人工 Pending、A05 最终资源、A01/A02/A03/A06 外部门禁或 D16-S01/S02。

## 2026-08-13 外部证据原始材料存在性校验补强

- `tools/d16_external_gate_audit.py` 现在会逐个解析 `raw_artifacts`，检查相对路径（相对于 `tools/results/external/`）或绝对路径确实指向现有文件；缺失路径会阻断对应门禁。
- 新增“完整字段但原始文件缺失”的负例测试；外部门禁聚焦回归 **30/30 PASS**，全量工具测试 **81/81 PASS**。
- 刷新结果 `tools/results/d16_external_gate_audit_current_20260813.json` 仍为 **BLOCKED**：A01—A06 六个 evidence envelope 缺失，Steam 未授权；没有生成或填充任何 PASS 证据。

该增量只加强证据真实性校验，不关闭 D16-A01—A06、D16-S01/S02、正式 D17 或 D18+。

### 路径边界回归补充

- 相对 `raw_artifacts` 现在禁止通过 `../` 逃出 `tools/results/external/`；新增路径逃逸负例后，外部门禁聚焦回归 **31/31 PASS**，全量工具测试 **82/82 PASS**。
- 当前六项外部 envelope 仍缺失，外部审计继续 **BLOCKED**。

## 2026-08-13 最终本机审计收口快照

- A06 当前权威审计 `tools/results/d16_a06_release_audit_current_20260813.json`：19/19 本机与安装器技术检查为 `true`，`missing_static_evidence=[]`，`technical_preparation_status=PASS`；发布清单仍为 `blocked`，保留 7 条项目级阻断。
- A05 来源/审查/授权引用断链已修正为 0；A04 当前结构错误为 0，A05 声明路径断链为 0；这两项只是机器可验证子门禁，不等于人工批准或最终授权资源到位。
- 工具全量回归为 **84/84 PASS**，A04/A05/A06 聚焦回归为 **28/28 PASS**，`git diff --check` 通过；Unity 本机既有最终回归仍为 60/60 PASS。
- 外部门禁审计 `tools/results/d16_external_gate_audit_current_20260813.json` 继续为 **BLOCKED**：A01—A06 六个真实 evidence envelope 缺失，Steam 上传未授权、未执行。

本快照只确认本机代码、工具和审计没有回退；不把本机结果扩大为目标设备、人工审核、最终资产、物理断电/跨平台、原生崩溃/隐私或 Steam 发布证据。D16-S01/S02、正式 D17 和 D18+ 仍未完成。

## 2026-08-13 A05 资源包预检能力复验

- 新增只读入口 `tools/d16_a05_replacement_preflight.py`，验证真实最终资源包的文件、路径边界、来源/许可证/授权、人工审核、LOD/碰撞/锚点、测试用例和原始材料；不复制文件、不写 Registry、不改 `asset_state`。
- 预检测试覆盖完整包通过、缺证据拒绝、placeholder/路径逃逸拒绝和已集成条目重复替换拒绝，专项 **4/4 PASS**。
- 工具全量回归更新为 **88/88 PASS**；A05 仍为 2,264 个 placeholder，当前没有合法最终资源包，D16 外部门禁仍 `BLOCKED`。

## 2026-08-13 A04 逐条人工复核包

- 新增只读 `tools/d16_a04_review_packet.py`，生成 `tools/results/d16_a04_review_packet_current_20260813.json` 与 `docs/game/design/common/D16_A04_REVIEW_PACKET_CURRENT.md`；当前覆盖 267 条 Pending、12 批。
- 包状态为 `HUMAN_DECISION_REQUIRED`，引用缺口 0，267/267 条已有本地文件型复核入口；所有决定模板仍为 `Pending`，没有写回 Registry。
- 专项测试 **4/4 PASS**，确认缺失引用会阻断材料包、敏感字段要求警告/跳过/展示级别，且生成过程无副作用。
- 该增量只补强 D16-R01 的人工交接材料；A04 仍 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-13 D16-S01 启动前预检

- 新增只读工具 `tools/d16_s01_readiness.py` 与 4 项测试；它合并六项 external gate、A04/A05/A06 快照，拒绝在前置门禁未关闭时启动 S01。
- 当前结果 `tools/results/d16_s01_readiness_current_20260813.json`：`BLOCKED`；六项 external gate 均因真实 evidence 缺失而未通过，同时 A04 Pending 267、A05 placeholder 2,264 仍是阻断。
- 专项测试 **4/4 PASS**；工具不会启动故事、写 Registry、审批内容或上传 Steam。Steam 仍单独保持未授权。

## 2026-08-13 A04 复核包全量回归后的权威快照

- 工具全量回归为 **96/96 PASS**，A04 复核包专项为 **4/4 PASS**、D16-S01 启动预检专项为 **4/4 PASS**，`git diff --check` 通过。
- A04：3,139 条登记、授权 2,918/2,918、结构错误 0、Pending 267；A05：2,918 visual、integrated 654、placeholder 2,264、声明路径断链 0；A06 本机技术准备 19/19 PASS。
- A04 复核包状态仍为 `HUMAN_DECISION_REQUIRED`，引用缺口 0，267/267 条已有本地文件型入口；没有新增人工决定或 Registry 写入。
- 外部门禁继续 `BLOCKED`，六个真实 evidence envelope 缺失、Steam 未授权；D16-S01/S02、正式 D17 和 D18+ 仍未完成。

## 2026-08-13 A05 第一批真实候选资源

- 已核对 Kenney Pirate Kit 与 Kenney Blocky Characters 的官方来源、下载包哈希和包内 CC0 许可证，形成候选包 `tools/results/d16_a05_candidate_packet_01_20260813.json`。
- 候选资源只保存到 `game/Assets/Art/Candidates/Kenney/`，并映射到第一批垂直切片目标；候选包状态为 `CANDIDATE_PENDING_REVIEW`，人工决定为 `Pending`，Registry 和 `asset_state` 均无变更。
- 添加候选文件后 A05 权威审计仍为 2,918 visual、125 data_only、654 integrated、2,264 placeholder；非法元数据 0、声明路径断链 0。候选专项 **2/2 PASS**，工具全量 **98/98 PASS**。
- 该材料只推进 A05 的真实来源准备，不关闭最终资源、人工审查或外部门禁；D16-S01 仍 **BLOCKED / NOT STARTED**，D16-S02、正式 D17 和 D18+ 仍未启动。
- Unity 导入级只读复核覆盖 7/7 FBX，全部具备 ModelImporter、Mesh 和正 bounds；角色另有 27 个 Generic 动画片段，结果见 `tools/results/d16_a05_candidate_import_current_20260813.json`。该证据不替代尺寸、材质、LOD、碰撞、锚点、运行时消费者或人工表现验收。

## 2026-08-13 候选导入后的最终本机复核

- Unity 候选导入级审计 **7/7 PASS**；候选包与导入证据专项 **4/4 PASS**。
- 工具全量回归 **100/100 PASS**，`git diff --check` 通过；A04/A05/A06 权威审计重新生成且数字未改变。
- 外部门禁仍 `BLOCKED`：A01—A06 六个真实 envelope 均缺失，Steam 未授权；D16-S01 预检仍 `BLOCKED` 且 `story_executed=false`。
- 本轮修复的 Unity `.meta` 编译问题只恢复了本机脚本编译和候选导入能力，不构成最终资源、人工审核、目标设备或正式 D16/D17 通过。

## 2026-08-13 候选导入后的 PlayMode 回归证据

- 使用候选导入审计后的工程重新执行 Unity PlayMode，`game/tools/results/d17_playmode_full_after_candidate_import_20260813.xml` 为 **60/60 PASS**，失败 0、跳过 0。
- 同一回归日志记录 D17-S02 本机十轮循环 **10/10 passed**；本机回归链保持稳定。
- 该证据不关闭 A04 267 条 Pending、A05 2,264 个 placeholder、六项外部门禁或 Steam 授权要求；D16-S01/S02、正式 D17 和 D18+ 仍未完成。


## 2026-08-18 D16-C10 本轮增量节（外部审计与S01就绪）

- **外部审计快照**: `tools/results/d16_external_gate_audit_current.json`，结论为 `BLOCKED`（缺失 D16-A01/A02/A03/A06 4 项外部硬件实测 envelope）；
- **S01 就绪检查**: 运行 `tools/d16_s01_readiness.py` 评估，判定为 `BLOCKED`（因前置外部硬件门禁未达成真实 PASS）；
- **长线故事执行**: 由于前置门禁未满足，根据门禁协议红线，不启动 S01 真实长线故事，不以开发机测试冒充全版本验收；
- **任务结论**: `D16-C10` 诚实记录为 `GATE_BLOCKED`。
