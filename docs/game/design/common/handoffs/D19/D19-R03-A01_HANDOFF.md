# D19-R03-A01 交接

- 任务：D19-R03-A01
- 结论：PARTIAL
- 实际修改：新增只读运行时探针 `game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs` 及其 Unity meta；建立真实 Player 构建和本轮路线结果文件。未修改 Systems、存档、Registry、InitialHarbor 或 D19-A01—A06 工作成果。
- Unity 编译：`D19_R03_A01_UNITY_COMPILE_RERUN1.log`，Unity 6000.0.80f1，退出码 0；仅有废弃 API/既有警告。
- Player：`D19_R03_A01_Player.exe`；真实窗口启动并进入 InitialHarbor，探针时长 200.9018 秒。
- 真实证据：Player、D2 控制器、路由器和落地均已确认；移动观察为 true；交互/保存观察为 false；市场、仓库、酒馆、船坞和返港未完成。
- 结果：允许继续 D19-R03-A02/A03 的独立修复；不能解锁 D19-R03-S01，也不能启动 D20。
- 主要阻断：本轮只能形成离散键击位移，未形成连续长按路线；没有 5—10 分钟路线截图、交互反馈和保存恢复证据。
- 下一责任：D19-R03-A01 需要在视觉/HUD 修复后使用可稳定维持 WASD 的真实 Player 输入重新取证。

## 2026-08-16 RERUN4 增量（真实 Player）

- 使用 D19-R03-A02 RERUN14 Player（1920×1080，D3D11）启动新游戏并持续运行 301.169769 秒；探针文件为 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN4.json`。
- 本轮 `player_found/controller_found/router_found/moved_observed/interaction_input_observed/save_input_observed` 均为 `true`；实际 Player 中 E 产生登船/下船反馈，B 打开贸易页，C 显示保存检查点提示。
- 到达探针地标：码头、泊位、酒馆、仓库和出生点；未到达市场、船坞和返港地标。未验证离港、返港以及保存后重载恢复，因此任务仍为 `PARTIAL`。
- Player 日志记录多次“不可行走碰撞体”恢复提示，说明连续路线和场景碰撞仍需后续修复；不能把本轮 301 秒采样写成完整 5—10 分钟路线通过。

## RERUN7_FULLSCREEN 增量（2026-08-16）

- 使用同一已构建 `D19_R03_A01_Player_RERUN7.exe` 进行窗口化与 1920×1080 全屏主菜单复验；Unity 日志确认主菜单和 EventSystem 初始化。
- Windows Security 防火墙提示覆盖 Player，计算机控制工具的点击被系统报告为 `PickerHost.exe`，Return/Tab/Space 未触发 New Game。本轮没有点击、允许、取消或关闭系统提示。
- 未进入 `InitialHarbor`，所以没有产生路线、交互、保存、退出或重载证据；真实截图 `mainmenu_rerun7_system_blocker.jpg` 只证明主菜单显示和环境阻断。
- 新机器结果 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_MACHINE_RESULT.json`、路线记录 `D19_R03_A01_RERUN7_ROUTE_LOG.json`、说明 `docs/game/design/common/D19_R03_A01_PORT_WALK_AND_INTERACTION_RERUN7.md` 均将本轮标为 `PARTIAL`。历史 RERUN4/RERUN6 不覆盖。

## RERUN10_NONDEVELOPMENT_PLAYER 增量（2026-08-16）

- 为排除 Unity Development PlayerConnection 触发的网络提示，新增不带 `Development/AllowDebugging` 的本地验证 Player；构建日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN10.log`。
- Player 真实启动、MainMenu/EventSystem/路线探针初始化均成功，日志：`D19_R03_A01_MAINMENU_ROUTE_RERUN10_PLAYER.log`。
- Windows 安全提示仍覆盖窗口；没有向该系统提示发送点击、键盘、允许、取消或关闭动作。未进入 InitialHarbor，未产生新路线、交互、保存或重载事实。
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN10_MACHINE_RESULT.json`；路线记录：`D19_R03_A01_RERUN10_ROUTE_LOG.json`。本轮结论保持 **PARTIAL**，D19-R03-S01 仍不放行。

## RERUN10_NONPOPUP_REAL_PLAYER 增量（2026-08-16）

- 关闭历史系统提示干扰后，使用同一非 Development Player 的非 popup 窗口重新完成真实输入取证；未修改 Player 可执行文件、Systems 或存档 Schema。
- 连续运行 `704.296936s`，超过 5 分钟要求；探针记录 `player/controller/router=true`、`moved_observed=true`、`interaction_input_observed=true`、`save_input_observed=true`，并观察到 `Character`/`Ship` 两种控制模式。
- 真实地标：码头/泊位 `41.898s`，酒馆地标 `391.415s`，出生点 `471.180s`；市场、仓库、船坞和返港地标未到达。共 16 张 `1920×1080` Player 截图。
- 本轮真实按 E 触发了登船/下船模式切换，按 F5/C 观察到保存输入边沿；随后关闭 Player，重新启动并在主菜单点击 Continue。重载日志明确记录“存档加载成功，进入游戏”、InitialHarbor 重建和 `D19_TerrainRoot` 落地恢复。
- 新机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_MACHINE_RESULT.json`；路线记录：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN10_ROUTE_LOG.json`；说明：`docs/game/design/common/D19_R03_A01_ROUTE_AND_SAVE_RERUN10.md`。
- 结论仍为 **PARTIAL**：真实连续运行、移动、登船/下船和保存后 Continue 重载已取得，但完整市场/仓库/船坞/离港/返港/NPC 路线没有通过；日志仍有不可行走碰撞体恢复提示。D19-R03-S01 不放行，下一责任优先为 D19-R03-A02 路线/碰撞修复。

## RERUN24/RERUN25 增量（2026-08-16）

- RERUN24 独立探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN24.json`。真实 Player 连续运行 `292.792694s`，`player/controller/router=true`，观察到位移和交互输入，未观察保存边沿；到达码头/泊位/酒馆，未完成完整路线。
- RERUN25 独立机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN25_MACHINE_RESULT.json`；路线说明：`docs/game/design/common/D19_R03_A01_ROUTE_AND_SAVE_RERUN25.md`。
- RERUN25 真实 Player 连续运行 `675.432556s`，1920×1080；探针观察到位移、交互输入和保存键边沿，保存边沿不是保存成功证明。到达 `anchor.player.spawn`、`anchor.dock.primary`、`anchor.berth.primary`、`building:tavern_santa_margarita`；未到达市场、仓库、船坞和返港点。
- 本轮仅有 3 张 Player 检查点截图，未形成要求的 9+ 路线截图；没有保存成功/退出/重载恢复日志。此前 RERUN10 的 Continue 重载证据保留，但不与本轮拼接成完整路线。
- RERUN25 结论继续为 **PARTIAL**；D19-R03-S01 仍不放行，D20 正式集成仍锁定。

## RERUN27 CUA 焦点排查（2026-08-16）

- 以固定的 RERUN26 非调试 Player 真实进入 MainMenu → 新游戏 → InitialHarbor，并确认 E 的模式切换反馈；Player 已干净关闭，Unity/Player 进程均为 0。
- 本轮尝试 W/A/S/D 短脉冲和文字注入，没有形成可复核的连续位移路线；因此不计入 5—10 分钟路线、交互闭环或保存恢复证据。
- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN27.json` 与 RERUN25 SHA-256 完全相同（`12F36D3FA55D162593544746C66B2FF94D108F2F5B449703DEECB4DC79E0C2EF`），已明确标为重复/非独立证据；机器记录见 `D19_R03_A01_CUA_FOCUS_RERUN27_MACHINE_RESULT.json`。
- 根因记录：当前 Computer Use 只提供短脉冲 `press_key`，没有可保持的 key-down/key-up 接口；项目 D2 移动路径读取持续按键状态，不能把本轮脉冲结果解释为真实移动失败或成功。D19-R03-A01 仍为 **PARTIAL**，不放行 D19-R03-S01。

## RERUN29_LONG（2026-08-16）

- 在补齐 D19 Integration `D19_TerrainRoot` 受控路线碰撞后，重新构建非 Development Player：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN29.exe`。
- 使用真实 Player、1920×1080 窗口和 CUA 输入连续运行 `694.13134765625s`；Player、D2 控制器、路由器、位移、交互输入和保存输入均被探针观察到。
- Player 日志出现真实“存档检查点已完成”、`[D3/Save] 贸易、库存和航程快照已保存`、`D2/Ship 控制权切换` 和 `D19_TerrainRoot` 接地恢复；这证明保存反馈、登船/下船模式切换和新路线碰撞体被运行时读取，但本轮没有执行 Continue 重载，因此不声称保存恢复闭环。
- 真实达到：`anchor.player.spawn`、`building:tavern_santa_margarita`、`anchor.dock.primary`、`anchor.berth.primary`；未达到：`building:market_santa_margarita`、`building:warehouse_santa_margarita`、`building:shipyard_santa_margarita`、`anchor.return.harbor`。截图 `route_rerun29_00.png`—`route_rerun29_15.png` 共 16 张，已核验为 1920×1080。
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN29_MACHINE_RESULT.json`；探针与长日志的归档副本分别为 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN29_LONG.json`、`D19_R03_A01_ROUTE_RERUN29_LONG_PLAYER.log`。
- 本轮结论保持 **PARTIAL**：碰撞体从静态 0 个增为运行时可识别的 1 个，但完整市场/仓库/船坞/返港/重载路线仍未通过；D19-R03-S01 继续不放行。D19-R03-A02 仍需负责视觉、HUD、路线可达性与天气/性能证据闭合。

## RERUN39 / RERUN40 定向复验（2026-08-16）

- RERUN39 在当前 R03 碰撞修复后使用真实 Windows Player、1920×1080、CUA 输入运行 `211.7932586669922s`。机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN39_MACHINE_RESULT.json`；探针和 Player 日志：`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN39.json`、`D19_R03_A01_ROUTE_RERUN39_PLAYER_ARCHIVE.log`。
- 真实输入完成 Main Menu → New Game → InitialHarbor；E 上船、E 下船和 C 保存检查点反馈均可见，移动输入也被探针观察到。到达出生点、酒馆、码头、泊位；市场、仓库、船坞、返港未到达；同轮退出后 Continue 未执行。路线仍为 **PARTIAL**，不能拼接旧 RERUN29 形成完整通过。
- 本轮归档 5 张真实 Player 检查点截图：`game/tools/results/D19/D19-R03-A01/screenshots/rerun39/route_rerun39_00.png..04.png`，均为 1920×1080；它们只证明本轮画面/反馈存在，不证明完整路线。
- RERUN40 继续使用真实 Player 做定向 D/W/A/S 与跳跃复测，但 Computer Use 只能提供离散键击，未产生独立探针文件；该尝试不计入路线通过，事实保留在本交接中，不伪造成功。

## RERUN41 参数化探针复验（2026-08-16）

- 为避免历史 RERUN29 文件复用，新增 `-d19-r03-route-rerun41` 专属探针输出并构建独立非 Development Player：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN41.exe`；构建日志为 `D19_R03_A01_PLAYER_BUILD_RERUN41_UNITY_RERUN2.log`。
- 真实 Player 从 Main Menu 通过 CUA 点击 New Game 进入 InitialHarbor；探针运行 `59.76485061645508s`，`player/controller/router=true`，观察到真实位移、E/B 交互边沿和 C 保存边沿。
- 本轮实际到达 `anchor.player.spawn`、`building:market_santa_margarita`、`building:tavern_santa_margarita`、`anchor.dock.primary`、`anchor.berth.primary`；未到达仓库、船坞和返港点。E 末段产生真实 `Ship/boarded` 控制模式，未完成出航/返港。
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN41_MACHINE_RESULT.json`；独立探针：`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json`；Player 日志：`D19_R03_A01_ROUTE_RERUN41_PLAYER_RERUN2.log`。
- 本轮没有通过截图分支（`current_run_count=0`），因此不把历史 RERUN35/RERUN39 截图重新标为 RERUN41；旧截图和旧结论继续保留。RERUN41 结论 **PARTIAL**，未解除 D19-R03-S01 门禁。

## RERUN42 定向 Player 复验（2026-08-16）

- 新建并运行独立非 Development Player `game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN42.exe`，请求 1920×1080，真实运行 `200.1754150390625s`。
- 探针观察到 Player/controller/router、移动输入和交互输入；当前轮生成 6 张真实 Player 截图：`game/tools/results/D19/D19-R03-A01/screenshots/route_rerun42_00.png` 至 `route_rerun42_05.png`。
- 本轮仅到达 `anchor.dock.primary` 与 `anchor.berth.primary`；市场、仓库、酒馆、船坞、返港和出生锚点未形成新的到达记录；保存输入为 false，未执行同轮 Continue。
- 机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN42_MACHINE_RESULT.json`；探针：`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json`；日志：`D19_R03_A01_ROUTE_RERUN42_PLAYER.log`。
- 本轮结论 **PARTIAL**，没有减少 D19-R03-S01 的完整路线、保存恢复、性能、视觉和天气阻断；RERUN41 及更早证据保持不变。

## RERUN44 旧重复碰撞定向修复后复验（2026-08-16）

- 在 R03 Integration 桥接层新增只读范围内的重复碰撞收敛：运行时禁用 `D19_SaintMargaret_Root` 下 3 个启用的 `DockDeck_Collision`/`Gangplank_Collision`，保留稳定交互节点和 D6 `DockGround`/`GangplankGround` 所有权；未修改 Systems、InitialHarbor 资源状态或存档字段。
- 重新构建并启动独立非 Development Windows Player `game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN44.exe`，请求 1920×1080，真实运行 `193.38430786132813s`。Player/controller/router、移动输入和交互输入均观察到，保存输入为 false；本轮落盘 7 张 `route_rerun44_00.png`—`route_rerun44_06.png`。
- 运行时探针确认船位为 `(0,0,0)`，玩家与 `D6_ShipHullCollision` 未重叠；玩家最终位置约为 `(-0.005,0.878,-6.455)`，相对初始点仅约 `0.045m`。实际只到达 `anchor.dock.primary`、`anchor.berth.primary`，没有形成市场、仓库、酒馆、船坞、返港或保存/重载闭环。
- 结论仍为 **PARTIAL**。本轮证实旧重复码头碰撞不是船体重叠根因，但没有取得完整路线证据；当前计算机控制证据仍是离散键击，不能把它解释为持续 WASD 路线通过。机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN44_MACHINE_RESULT.json`；探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN44.json`；日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN44_PLAYER.log`。

## RERUN45/RERUN46 旧地形 Collider 定向修复后复验（2026-08-16）

- RERUN44 后定位到正式场景仍启用的 `D19_SaintMargaret_Root/D19_EnvironmentRoot/D19_TerrainRoot` 旧地形 Collider（静态边界约 256×272）与 R03 受控路线面重叠。仅在 R03 Integration 桥接层禁用该层的 1 个 Collider，保留视觉、稳定层级和 R03 路线面；没有改 D19-A01 源场景/Prefab、Systems、存档或资源状态。Player 日志确认：`legacy terrain colliders disabled=1`。
- RERUN45 独立 Player 真实运行 `120.1856460571289s`、请求 1920×1080，Player/controller/router、移动和交互输入为 true，保存输入为 false；到达酒馆、码头、泊位，生成 2 张当前轮截图。机器结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN45_MACHINE_RESULT.json`；探针：`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN45.json`。
- RERUN46 独立 Player 真实运行 `126.42691040039063s`、请求 1920×1080，Player/controller/router、移动和交互输入为 true，保存输入为 false；到达出生、酒馆、码头、泊位，生成 4 张当前轮截图，市场/仓库/船坞/返港未到达。机器结果：`D19_R03_A01_REAL_PLAYER_RERUN46_MACHINE_RESULT.json`；探针：`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN46.json`。
- 两轮都没有形成 300 秒完整路线、出航—返港、保存—退出—Continue；RERUN46 日志仍有多次“阻止角色攀爬不可行走碰撞体”恢复提示。结论仍 **PARTIAL**，但旧地形 Collider 是真实新增根因修复，不能把短跑结果包装成完整路线通过。
