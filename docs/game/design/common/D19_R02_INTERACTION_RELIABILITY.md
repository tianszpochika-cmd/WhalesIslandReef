# D19-R02-A02 D2 交互可靠性

## 结论

本任务结论为 **PARTIAL**。D2 现在为 NPC、船舶、距离、模式和安全下船失败提供稳定的只读反馈快照；自动化 EditMode 3/3、PlayMode 2/2 通过，其中公开上船/下船循环 10/10，危险下船拒绝 1/1。真实 Player 已从主菜单进入 InitialHarbor 并保存 4 张截图，但键鼠 E/W 尝试没有产生可定位的 D2 控制日志或 HUD 反馈，故不把真实 Player 交互写成通过。

## D2 边界

修改只在 `game/Assets/Scripts/Gameplay/D2/D2ControlRouter.cs`：

- `D2InteractionFeedback` 是显示/测试快照，不是领域事实。
- `TryOpenCrewDialogue()` 公开为现有对话入口，仍调用 `D2DialogueController.Open`，不复制 NPC、关系或任务逻辑。
- `TryToggleShip()` 仍是唯一 D2 上下船请求入口，仍由现有 D7 甲板/角色/船控和安全碰撞逻辑决定结果。
- `ReadLastInteractionFeedback()` 返回 clone，外部不能通过该快照回写 D2 状态。
- `InteractionFeedbackChanged` 只通知表现层，不写 Canvas、不写存档、不发放奖励。

稳定目标沿用现有 `ph_crew_male` 与 `ph_ship_fishing`，没有新建或改动玩法 ID。

## 反馈表

| 场景 | code | 结果 |
|---|---|---|
| NPC 在范围内 | `npc_dialogue_open` | 打开既有对话入口 |
| NPC 太远 | `npc_too_far` | 明确靠近提示 |
| NPC/船目标缺失 | `npc_target_missing` / `ship_target_missing` | 明确目标缺失 |
| 船在范围内 | `boarded` | 进入 Ship 模式 |
| 船太远 | `ship_too_far` | 明确靠近码头提示 |
| 有安全地面 | `disembarked` | 回到 Character 模式 |
| 无安全地面 | `disembark_unavailable` | 保持 Ship 模式，不把玩家放到水面/空中 |
| 模态或锁定 | `gameplay_input_blocked` / `interaction_mode_locked` | 明确当前不可操作 |

## 自动化结果

```text
Unity 6000.0.80f1 Assembly-CSharp 编译：PASS
EditMode WhalesIslandReef.D19R02A02.EditMode.Tests：3/3 PASS
PlayMode D16.PlayMode.Tests / D19R02A02InteractionPlayModeTests：2/2 PASS
  - 10 次公开上船/下船：10/10 成功
  - 无可行走地面下船：1/1 拒绝，控制模式仍为 Ship
```

证据：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_EDITMODE_TESTS_ALL.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYMODE_TESTS_RERUN.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE.json`

## 真实 Player 结果

Player 构建成功并从 Main Menu 创建航程进入 InitialHarbor。真实屏幕采集如下：

- `01_harbor_initial_player.png`：初始港口
- `02_after_e_player.png`：第一次 E 尝试后
- `03_after_focus_and_e_player.png`：重新聚焦后的 E 尝试
- `04_after_repeated_w_player.png`：重复 W 尝试后

鼠标点击能改变镜头，但 E/W 没有在 `Player.log` 中形成 `[D2/Control]` 事件，且底部 HUD 仍显示初始提示。该事实保留为 NOT_VERIFIED；不会用自动化调用替代真人输入。另发现 A01 的 HUD Manager 可在 D2 bootstrap 前缓存 null router，这个跨任务问题已记录给 A01，不在本任务越权修复。

## 后续注意

- A03 必须读取此交接，不得复制 D2 上下船或对话逻辑；应从公开入口驱动菜单/D3路线。
- S02 必须在真实 Player 中再次验证 E 对话、距离失败、登船、下船和反馈显示；自动化 10/10 不能替代键鼠路线。
- 不得把 `D2InteractionFeedback` 升格为 Systems 或 Save Schema；它是瞬时表现读取模型。

## RERUN2 针对性修复（2026-08-15）

本轮没有覆盖原始 PARTIAL 证据，新增结果写入 `game/tools/results/D19/D19-R02-A02/` 的 `RERUN2` 文件。

- 移除了 `D16InputRouter.TryGetPressedKeyboardKey` 的逐帧诊断调用。该调用在 Unity 6 的 `Key` 枚举包含无效值时会抛出 `ArgumentOutOfRangeException`，不是可接受的运行时行为。
- 保留同一 D2 Input System 的 E 兼容边沿；没有新增第二套业务输入裁决。
- D2 物理/下船表面白名单增加冻结布局约定的 `D19_TerrainRoot`、`D19_R02_TerrainSurface`、`D19_R02_WalkableTerrain`。这解决了真实 Player 初始落地后被恢复到旧 `BeachGround` 的跨层冲突，仍仅接受明确命名的行走面。

RERUN2 自动化结果为 EditMode 3/3、PlayMode 2/2、Windows x64 Player 构建 PASS。真实 Player 日志确认 `surface=D19_TerrainRoot` 且未再观察到上述异常；E 键仍未形成可定位的 D2 反馈事件，保持 `NOT_VERIFIED`，所以任务结论仍为 PARTIAL。

新增证据：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN2_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN2.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_EDITMODE_TESTS_RERUN2.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYMODE_TESTS_RERUN2.xml`
- `game/tools/results/D19/D19-R02-A02/05_rerun4_ground_player.jpg`
- `game/tools/results/D19/D19-R02-A02/06_rerun4_e_player.jpg`

## RERUN5 输入兼容复验（2026-08-15）

RERUN5 只改动 D2 控制入口：`D2CharacterController.Update()` 在既有 `D16InputRouter.MoveAxis()` 没有读到轴值时，读取同一组 WASD/方向键的 legacy 状态作为兼容移动意图。速度、碰撞、落地、上下船和领域裁决没有改动，也没有新增业务输入所有者。

真实 x64 Player 以 `1920x1080` 参数启动，主菜单新游戏和 `D19_TerrainRoot` 落地通过；B 打开交易面板通过。单次 W/A 事件或 Win32 持键保持在当前采集工具中不足以稳定采样，但以真实 Player 窗口重复发送 A（80 次，约 25ms 间隔）和 W（120 次，约 25ms 间隔）后，玩家姿态/朝向及视图构图相对前帧发生可见变化；没有 Inspector 调用、Transform 注入或直接调用游戏方法。Player 日志未出现目标异常，Player 已关闭。

RERUN5 的 A02 专用 EditMode 3/3、PlayMode 2/2 通过，仍不能代替 D19-R02-S02 的 300 秒完整路线、仓库/酒馆/NPC/登船/返港/保存重载、14 个固定机位和三档真实性能门禁，因此 A02 保持 **PARTIAL**。

新增证据：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN5_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN5.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_EDITMODE_TESTS_RERUN5_REAL.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYMODE_TESTS_RERUN5_REAL.xml`
- `game/tools/results/D19/D19-R02-A02/04_rerun5_before_move.jpg`
- `game/tools/results/D19/D19-R02-A02/09_rerun5_after_b.jpg`
- `game/tools/results/D19/D19-R02-A02/10_rerun5_repeated_a.jpg`
- `game/tools/results/D19/D19-R02-A02/11_rerun5_repeated_w.jpg`

## RERUN6 E 边沿兼容复验（2026-08-15）

RERUN6 在不改变 D2 所有权和交互裁决的前提下，给 `D2ControlRouter` 的同一 E 输入增加 `Input.GetKeyDown(KeyCode.E)` legacy 边沿兜底。它只在既有 Input System 没有产生边沿时补齐输入意图，不写船舶状态、不绕过距离/安全地面检查，也不新增第二个业务输入入口。

真实 Player 证据来自 D19-R02-S02 RERUN4：

- E 登船：`15_rerun4_e.jpg`，日志出现 `[D2/Input] 通过兼容 E 边沿恢复交互入口` 和 `[D2/Control] 已进入船只控制`。
- E 下船：`22_rerun4_disembark.jpg`，日志出现同一输入边沿和 `[D2/Control] 已退出船只控制`。
- T/N/L 航程状态、F5 保存、退出后 Continue 重载均在同一真实窗口会话中可追溯；这属于 S02 路线证据，A02 不扩大为最终路线验收。

本轮自动化与构建：EditMode 3/3、PlayMode 2/2、Windows x64 Player build 均通过。A02 仍为 **PARTIAL**：仓库/酒馆/NPC、14 个固定镜头、1920×1080 三档真实性能和完整 D19 门禁不属于本任务可单独宣称的证据；D20-S02-R01 仍未放行。

新增证据：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN6_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN6.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_EDITMODE_TESTS_RERUN6.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYMODE_TESTS_RERUN6.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYER_BUILD_RERUN6.log`

## RERUN7 船范围优先级修复（2026-08-15）

RERUN7 针对 S02 RERUN5 暴露的真实 P0 交互冲突做最小修复。原调用链在 `D2ControlRouter.HandleInteraction` 中先尝试老水手对话，再尝试船入口；由于出生/泊位处 NPC 与船距离很近，玩家在登船范围内按 E 会被对话路径持续消费。修复后仍由 D2ControlRouter 唯一处理 E：先复用已有的船登船范围判定，只有在船范围外才进入既有 NPC 对话路径。未修改 HUD、InitialHarbor、D3、D7/Systems、存档或 Registry。

真实 x64 Player 使用键鼠完成：主菜单新游戏进入 InitialHarbor、在相邻 NPC 仍可见的登船区域按 E 上船、再次按 E 下船。日志明确记录兼容 E 边沿、进入船只控制和退出船只控制；未调用 Inspector 方法或直接调用业务 API。该修复的 EditMode 3/3、PlayMode 2/2 和 Windows x64 Player 构建通过。

新增证据：

- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN7_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE_RERUN7.json`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_EDITMODE_TESTS_RERUN7.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYMODE_TESTS_RERUN7.xml`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYER_BUILD_RERUN7.log`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_PLAYER_RERUN7_INPUT.log`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN7_SPAWN.jpg`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN7_BOARD.jpg`
- `game/tools/results/D19/D19-R02-A02/D19_R02_A02_RERUN7_DISEMBARK.jpg`

本任务仍为 **PARTIAL**：RERUN7 只关闭 D2 船/NPC 入口冲突，不替代 D19-R02-S02 的 300 秒路线、仓库/酒馆/完整 NPC 反馈、14 个固定 Player 镜头、天气变体和真实 1920×1080 三档性能门禁。D20-S02-R01 继续保持未放行。
