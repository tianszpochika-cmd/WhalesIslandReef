# Unity Pipeline Windows Player 运行交互回归交接

日期：2026-09-11  
范围状态：`PASS_WITHIN_SCOPE`  ；VC-G01 正式状态：`IN_PROGRESS`

## 结论

已用 `com.unity.pipeline@0.6.0-exp.1` 控制一个隐藏的 Windows Player，完成一轮有界的 InitialHarbor 交互回归。R7 Player 可启动并被 Pipeline 接管；通过直接 `eval` 加载 InitialHarbor 后，W/S 移动、A/D 转向、E 远端拒绝、E 登船/重复 E 下船、I 仓库、M 市场、T 酒馆、临时页面 Esc 关闭、普通 Esc 暂停/恢复均取得运行时状态和日志证据。`simulate_pointer` 命令本身可达，但新鲜隐藏 Player 复测没有完成 MainMenu→InitialHarbor 点击转场，因此菜单指针路线不登记为 PASS。

这轮发现并修复了真实的输入冲突：模态 owner 关闭页面后，暂停控制器仍可能在同一 Esc 帧接管，导致“页面关闭并同时暂停”。修复后仓库、市场、酒馆的 Esc 关闭都保持 `paused=false`；D4/D9 同类关闭路径也同步声明 Esc 已消费。

权威结果：[UNITY_CLI_RUNTIME_QA_20260911_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_RUNTIME_QA_20260911_RESULT.json)。

## 本轮变更

- `HarborPauseController` 在真正的暂停消费点检查 `RuntimeHudCoordinator.PrimaryInputAllowed`，临时页面打开时不再抢占 Esc。
- `D29InteractionHud` 为仓库 I/Esc 增加 Input System/default keyboard 边沿兜底，并在关闭前标记 Esc 已消费。
- `D7ControlCoordinator` 增加同帧 Esc consumed marker 和默认 Esc 边沿读取。
- D3 市场、D10 酒馆、D9 港口整备、D4 内容页关闭分支在消费 Esc 时调用同一 marker。
- 未改 Systems、AgentCore、D2/D3/D4/D9/D10 的领域裁决；没有新增网络或 LLM 逻辑。

## 验证证据

- Pipeline `recompile`：`completed / failed=false / errors=[]`。
- D29 定向 PlayMode：`D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`，`1/1 PASS`，约 `14.39s`。
- D17 过滤 PlayMode：`21/21 PASS`，`failed=0`、`skipped=0`、`inconclusive=0`，约 `100.42s`；包含 W/S、游泳、上下船、相机、输入焦点和恢复路径。
- R7 build：`build_6cd0c3272cb4`，StandaloneWindows64，`Succeeded`，`totalErrors=0`，`totalWarnings=8`，SHA256=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`。
- R7 Player：Unity `6000.0.80f1`，BuildGuid=`6fbdbc5defdf4b5ea9ddcc71b2d733b4`，Pipeline `127.0.0.1:7801` 可达，InitialHarbor 运行时 error console `0`。
- 运行日志出现 `[D29/Interaction] success`、`[D3/Save]`、`[D10/UI]`、`[D2/Control]`、`[D7/Ship]` 和 `[HarborPause]` 的对应状态切换；临时页关闭后没有新增暂停事件。

## 证据边界

- Pipeline 的 `simulate_key`/`simulate_pointer` 是 Input System 注入，不等于用户物理键盘/鼠标电气路径；因此不能把本结果升级为“物理键盘 PASS”。
- 本轮没有运行 Player 截图采集；Player 使用隐藏启动以避免接管用户桌面。`runtime_status` 报告的 `DrawCalls=0` 也不是视觉质量证据。
- 当前任务动作、正式 Avatar/动画、游泳姿态、舵轮自然接触、树/植被接地、漂浮/穿模和最终性能仍未通过视觉门禁。VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 不解锁，D31 保持 `PARTIAL`。

## 配置收口

QA 构建期间临时启用了 Player Pipeline（端口 7801、Development/AllowDebugging）。完成后已通过 Pipeline 恢复并验证：

- Editor runtime：`enableInBuilds=false`、`port=0`、`enableAuditLogging=true`、`autoStart=true`。
- Build settings：`developmentBuild=false`、`allowDebugging=false`、`connectWithProfiler=false`。
- R7 QA Player 已按精确命令行校验后停止；没有残留 `PIPELINE_QA_PLAYER_20260911_R*.exe`。
- Unity Editor PID `31416` 仍是项目 Pipeline 后台连接进程；如不再需要插件会话，可单独按 PID 安全停止，不要按进程名批量结束。

## 下一步

1. 保持 VC-G01 `IN_PROGRESS`，把本结果作为逻辑/交互控制面证据，而不是最终视觉验收。
2. 继续用 Pipeline 做短时、有界的代码级和 Player 状态回归，避免长时间占用用户电脑。
3. 对剩余视觉缺陷使用可获得的截图/画面证据或一次短人工视觉检查；优先处理正式动作/Avatar、游泳、舵轮接触、植被接地和漂浮/穿模 owner。
4. 只有在视觉证据和实际输入边界都满足 VC-G01 门禁后，才推进 VC-G02 → VC-G03 → D31 收口。
