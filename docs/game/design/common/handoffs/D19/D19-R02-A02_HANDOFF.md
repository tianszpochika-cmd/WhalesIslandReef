# D19-R02-A02 交接

- 任务编号：`D19-R02-A02`
- 结论：**PARTIAL**
- 写者：`D2ControlRouter` 及 D2 专用测试/证据目录。

## 实际修改

- `game/Assets/Scripts/Gameplay/D2/D2ControlRouter.cs`
  - 新增只读 `D2InteractionFeedback`、`ReadLastInteractionFeedback()` 和 `InteractionFeedbackChanged`。
  - 将 NPC 对话、太远、缺目标、模态锁、上船、下船和安全失败统一写入可读 code/message。
  - `TryOpenCrewDialogue()` 公开为既有对话请求入口；未添加第二条业务路径。
- `game/Assets/Tests/EditMode/D19Interaction/`
- `game/Assets/Tests/PlayMode/D19Interaction/`
- `docs/game/design/common/D19_R02_INTERACTION_RELIABILITY.md`
- `game/tools/results/D19/D19-R02-A02/`

没有修改 RuntimeHudCoordinator、MainMenuController、D3、InitialHarbor、D7/D17、Systems、Registry、Save Schema、contracts/server/web。

## 真实验证

| 项目 | 结果 | 证据 |
|---|---|---|
| Unity 编译 | PASS | `D19_R02_A02_UNITY_COMPILE.log` |
| EditMode | 3/3 PASS | `D19_R02_A02_EDITMODE_TESTS_ALL.xml` |
| PlayMode | 2/2 PASS | `D19_R02_A02_PLAYMODE_TESTS_RERUN.xml` |
| 公开上/下船循环 | 10/10 | PlayMode XML 与日志 |
| 不可安全下船 | 1/1 明确拒绝 | PlayMode XML 与日志 |
| Windows Player 构建 | PASS | `D19_R02_A02_PLAYER_BUILD.log` |
| Player 主菜单→InitialHarbor | PASS | `01_harbor_initial_player.png` |
| Player E/W 键鼠交互 | NOT_VERIFIED | 无 D2 控制日志/可见反馈 |

## 未闭合问题

1. 真实 Player 键鼠路线没有形成 D2 事件，A02 不能 PASS。后续 S02 需以实际键鼠重新取证。
2. A01 HUD Manager 可能在 D2 bootstrap 完成前缓存空 router，导致公开反馈不显示；这是 A01 的 HUD 写入范围，不在 A02 修复。
3. 视觉灰盒和精确 1080p 性能属于 A04/S02。

## 下一任务

`D19-R02-A03` 可以继续；它必须使用本交接的公开入口，不要在 D3 或菜单层复制 D2 逻辑。D20 仍未放行，D19-R02-S02 需要等待 A03/A04/A05 的真实证据。

## RERUN2 交接（2026-08-15）

本轮为 D2 范围内的针对性修复与复验，原始 PARTIAL 结论和原始 XML/截图均保留。

### 新增修改

- `game/Assets/Scripts/Gameplay/D2/D2ControlRouter.cs`：移除会触发 Unity 6 `ArgumentOutOfRangeException` 的逐帧键盘枚举诊断；保留既有单一 E 输入兼容边沿；允许冻结 D19 地形表面名称作为明确行走/下船面。
- `game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`：同步允许 `D19_TerrainRoot`、`D19_R02_TerrainSurface`、`D19_R02_WalkableTerrain`，不改变角色移动公式或玩法所有权。

### RERUN2 真实验证

| 项目 | 结果 | 证据 |
|---|---|---|
| x64 Player 构建 | PASS | `game/tools/results/D19/D19-R02-A02/D19_R02_A02_BUILD_RERUN4_GROUND.log` |
| EditMode | 3/3 PASS | `D19_R02_A02_EDITMODE_TESTS_RERUN2.xml` |
| PlayMode | 2/2 PASS | `D19_R02_A02_PLAYMODE_TESTS_RERUN2.xml` |
| 真实 Player 主菜单→InitialHarbor | PASS | `D19_R02_A02_PLAYER_RERUN4_GROUND_INPUT.log` |
| 真实 Player D19 地形落地 | PASS | 日志含 `surface=D19_TerrainRoot, surfaceY=0.53, playerY=0.68`；`05_rerun4_ground_player.jpg` |
| 无效键盘枚举异常 | PASS（未再观察） | RERUN4 Player 日志无 `ArgumentOutOfRangeException` |
| 真实 Player E 交互反馈 | NOT_VERIFIED | `06_rerun4_e_player.jpg` 与日志；没有 D2 feedback/mode event |

### 当前结论与交接

任务仍为 **PARTIAL**。RERUN2 关闭了 D2 逐帧异常和 D19 地形/旧 BeachGround 的落地冲突，但没有把未观察到的 E 键事件写成成功。后续 D19-R02-S02 必须在真实 Player 中用可定位的输入边沿完成 NPC/船/距离反馈和路线取证；A03 仍可独立推进 D3 主菜单/路线闭环。D20 正式集成仍未放行。

机器交接：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN2_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN2.json`

## RERUN5 交接（2026-08-15）

### 本轮范围

本轮仅针对 D2 角色移动输入可靠性。`D2RuntimeBootstrap.cs` 在 D16 移动轴为空时增加同一组 WASD/方向键的兼容移动意图读取；没有改速度、碰撞、地面/船舶裁决、D3、HUD、InitialHarbor、Systems、存档、Registry 或资源状态。

### 真实验证

| 项目 | 结果 | 证据 |
|---|---|---|
| x64 Player 构建 | PASS | `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYER_BUILD_RERUN5.log` |
| EditMode | 3/3 PASS | `D19_R02_A02_EDITMODE_TESTS_RERUN5_REAL.xml` |
| PlayMode | 2/2 PASS | `D19_R02_A02_PLAYMODE_TESTS_RERUN5_REAL.xml` |
| 主菜单→InitialHarbor | PASS | `D19_R02_A02_PLAYER_RERUN5_INPUT.log`; `03_rerun5_spawn.jpg` |
| D19 地形落地 | PASS | `D19_R02_A02_PLAYER_RERUN5_INPUT.log` 含 `surface=D19_TerrainRoot` |
| 交易 B | PASS | `09_rerun5_after_b.jpg` |
| 重复 A/W 玩家输入 | PASS_OBSERVED | `04_rerun5_before_move.jpg`、`10_rerun5_repeated_a.jpg`、`11_rerun5_repeated_w.jpg` |
| Player 关闭 | PASS | RERUN5 结束时进程检查为 stopped |

### 结论与门禁

本轮仍为 **PARTIAL**。重复真实输入可以观察到角色/视图变化，但本轮不是 D19 最终路线，未证明 300 秒路线、NPC/仓库/酒馆、上下船/返港、保存重载、14 个固定 Player 镜头或 1920×1080 三档真实性能。D19-R02-S02 仍必须重新取证并通过后，D20-S02-R01 才可启动。

机器结果：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN5_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN5.json`

## RERUN6 交接（2026-08-15）

### 本轮修改与边界

- `game/Assets/Scripts/Gameplay/D2/D2ControlRouter.cs`：为既有 E 输入增加 Unity legacy `KeyCode.E` 边沿兜底；仍由 D2 唯一裁决船舶交互结果。
- `game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`：沿用 RERUN5 已验证的同一组 WASD/方向键兼容移动意图；没有改变移动公式、碰撞、落地或存档。
- 未修改 D3、HUD、InitialHarbor、Systems、Registry、资源状态或 Save Schema。

### 真实验证

| 项目 | 结果 | 证据 |
|---|---|---|
| Windows x64 Player build | PASS | `D19_R02_A02_PLAYER_BUILD_RERUN6.log` |
| D2 EditMode | 3/3 PASS | `D19_R02_A02_EDITMODE_TESTS_RERUN6.xml` |
| D2 PlayMode | 2/2 PASS | `D19_R02_A02_PLAYMODE_TESTS_RERUN6.xml` |
| 真实 Player E 登船 | PASS | S02 RERUN4 `15_rerun4_e.jpg` + `D19_R02_S02_PLAYER_RERUN4_INPUT.log` |
| 真实 Player E 下船 | PASS | S02 RERUN4 `22_rerun4_disembark.jpg` + 同一日志 |
| 真实 Player 退出后 Continue | PASS | `28_rerun4_continue_loaded.jpg` + `D19_R02_S02_PLAYER_RERUN4_CONTINUE.log` |
| A02 专项范围内最终结论 | PARTIAL | 未覆盖 D19 最终视觉/性能/完整路线门禁 |

### 下一任务门禁

E 输入边沿不再是当前 D2 证据中的未验证项；D19-R02-S02 仍必须以自身的真实路线、14+ Player 镜头、三档 1080p 性能和视觉检查作为最终门禁。D20-S02-R01 继续保持禁止启动，直到 D19-R02-S02 PASS。

机器结果：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN6_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN6.json`

## RERUN7 交接（2026-08-15）

### 修改范围

只修改 `game/Assets/Scripts/Gameplay/D2/D2ControlRouter.cs`。当玩家在既有船登船范围内按 E 时，先执行既有 `TryToggleShip`；船范围外仍走原有老水手对话路径。没有新增输入所有者，没有修改 D3、HUD、InitialHarbor、D7/Systems、存档、Registry 或资源状态。

### 真实验证

| 项目 | 结果 | 证据 |
|---|---|---|
| Windows x64 Player build | PASS | `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYER_BUILD_RERUN7.log` |
| EditMode | 3/3 PASS | `D19_R02_A02_EDITMODE_TESTS_RERUN7.xml` |
| PlayMode | 2/2 PASS | `D19_R02_A02_PLAYMODE_TESTS_RERUN7.xml` |
| Main Menu→InitialHarbor | PASS | `D19_R02_A02_RERUN7_SPAWN.jpg` |
| 相邻 NPC 区域 E 登船 | PASS | `D19_R02_A02_RERUN7_BOARD.jpg`、输入日志含 `[D2/Control] 已进入船只控制` |
| E 下船 | PASS | `D19_R02_A02_RERUN7_DISEMBARK.jpg`、输入日志含 `[D2/Control] 已退出船只控制` |
| 直接方法调用 | 未使用 | 路线 JSON `direct_method_invocation=false` |

### 结论与后续门禁

RERUN7 关闭了 D2 船/NPC E 入口冲突，但 D19-R02-A02 仍为 **PARTIAL**。D19-R02-S02 必须用本次代码重新完成自己的完整真人路线、保存重载、14+ 固定镜头、天气与真实 1920×1080 三档性能；D20-S02-R01 仍不得启动。

机器交接：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN7_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN7.json`
