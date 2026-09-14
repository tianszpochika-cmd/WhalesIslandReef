# Unity Pipeline 连接与能力探测交接

日期：2026-09-10  
状态：`PASS_WITH_SCOPE_LIMITS`

## 结论

当前项目已能通过 `com.unity.pipeline@0.6.0-exp.1` 被 Unity CLI 连接。一次有界的后台 Editor 会话真实暴露了 149 个工具，能够读取场景层级和 Transform、执行只读 C#、发现并运行单个 Unity 测试、读取包和 Console、进入/退出 Editor Play Mode，并对场景写操作执行 dry-run 校验。

这条通道适合做“代码/场景结构/运行时状态/日志/测试”检查，也可以在明确授权后修改 GameObject、Transform、组件、Prefab、材质、包和设置。本轮在能力探测后执行了一个最小源码门禁修复，并完成有界回归；没有写入场景、Prefab 或资产。

## 已验证

- 后台 Editor Pipeline 连接成功：`127.0.0.1:7800`，`editor_status=ready/playing`。
- `unity list` 返回 149 个工具，包含 `get_scene_hierarchy`、`get_component_properties`、`set_transform`、`find_gameobjects`、`eval`、`run_tests`、`console`、`screenshot`、`editor_play`、`package_list` 等。
- `InitialHarbor.unity` 层级读取成功：34 个根节点，能直接看到 `ph_ship_fishing`、`D19_R03_HarborPresentation`、`Harbor UI`、`Main Camera` 等对象。
- `/ph_ship_fishing/D2_PlayerSpawnAnchor` 的 Transform 读取成功：local position=`[0, 0.427357763, 0.9712676]`。
- 连接态 `run_tests` 执行一个 EditMode 测试：`1/1 PASS`。
- `batch dry_run` 验证创建 GameObject 的写入计划：`valid=true`、`revertible=true`、`applied=0`。
- 连接态 `editor_play`、`editor_status`、`editor_stop` 均可用；MainMenu 会话保持稳定。

## 本轮开发修复与回归

- 修改 `game/Assets/Scripts/Gameplay/D3/D3RuntimeSmokeProbe.cs`：探针现在仅在 batchmode 且显式带 `-d3Smoke` 时安装，并排除 `-d4Smoke`/`-d5Smoke`；普通 batchmode/Editor Play Mode 不再误装探针。
- 通过 Pipeline 重新打开 `InitialHarbor` 并进入 Play Mode，等待 18 秒（超过原 15 秒超时窗口）后 `editor_status=playing`，运行时 `eval` 返回 `NO_D3_PROBE`；日志没有编译错误匹配。
- 连接态异步 PlayMode 测试 `D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe`：`1/1 PASS`。
- 连接态 EditMode 测试 `D9A07UiAudioLogTests.CursorSkipsRestoredHistoryAndConsumesEachNewFactOnce`：`1/1 PASS`。
- Pipeline 运行时查询 `InitialHarbor`：`ph_ship_fishing` 的 lossyScale=`(2.57,2.57,2.57)`，`D7_HelmPoint` 的世界米制偏移=`(0,0.96,-1.47)`，`D3RuntimeSmokeProbe=NO_D3_PROBE`。
- 发现并同步 `VC_G01WorldLogicTests` 中滞后的 `(0,1.1,-1.8)` 期望到当前 D17/Q05-A03 已采用的 `(0,0.96,-1.47)` 校准；未修改 D7 船控生产逻辑。随后 VC-G01 世界逻辑 EditMode=`12/12 PASS`，D17 Action Quality PlayMode 异步回归=`15/15 PASS`。
- PlayMode 同步 HTTP 调用因 Unity domain reload 返回 `Total=0`，不计为测试结果；改用 `async_tests=true` 后轮询 `test_status` 得到上述真实结果。
- 目标后台 Unity 进程已按记录 PID 停止；没有启动或接管 standalone Player。

权威结果：[UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json)。

权威结果：[UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_PIPELINE_SESSION_20260910_RESULT.json)。

## 重要边界

### 1. 它不是 standalone Player 控制面

本轮只连接了 Editor，未连接或控制 Windows Player。因此仍然不能凭此证明真实 Player 的窗口焦点、物理键盘、WASD、转向、E 登船、I/M/T、Esc、鼠标和画面交互。

### 2. 纯 headless 截图不等于视觉验收

Game View API 能返回 PNG 路径，但 `-nographics` 下生成的是灰底，并由 Pipeline 的 `ScreenshotCommand.RenderToPng` 记录 `RenderTexture.Create failed`。Scene View API 返回“没有活动 Scene view”。因此本轮截图只能证明 API 调用路径，不是 VC-G01 视觉 PASS。

### 3. InitialHarbor 生命周期边界

普通 `InitialHarbor` batchmode 生命周期阻塞已由本轮最小门禁修复解除：无 `-d3Smoke` 时不会安装 `D3RuntimeSmokeProbe`。显式 `-d3Smoke` 的 markers 与完整烟雾通过仍未在本轮证明；这不替代 standalone Player 的真实输入和视觉验收。

## 对开发的实际价值

现在可以在不占用用户桌面的情况下：

- 直接查询某个场景对象、组件属性、层级路径和包状态；
- 运行小范围 EditMode/PlayMode 测试并读取结果；
- 进入 Editor Play Mode，执行只读运行时查询和 Console 日志读取；
- 先用 dry-run 验证潜在的场景/对象修改，再决定是否实际写入；
- 在支持 GPU/GUI 的 Editor 会话中进一步尝试可用截图。

## 下一步建议

1. 继续用 Pipeline 对 `InitialHarbor` 做有界运行时对象/Console/Transform 诊断，并把 `0 tests executed` 和截图灰底视为非通过。
2. 如需验证 D3 smoke runner，必须显式运行 `-d3Smoke` 并单独记录 markers 结果；不得把普通 batchmode 会话当作 D3 smoke 证据。
3. 如要获得可信视觉证据，需要可渲染的 Editor/Player 图形会话；本次插件本身不能把 `-nographics` 变成真实画面，也不能替代 native Player 输入验收。
