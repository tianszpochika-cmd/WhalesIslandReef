# VC-G01-LOGIC 代码层前置审计交接

日期：2026-09-05
状态：`PASS_WITHIN_SCOPE`（代码层与定向 EditMode 完成；无真实 Player 证据）

## 结论

已完成 D29/D30 交互入口和 `RuntimeHudCoordinator` 的状态、焦点、输入边界审计。确认并修复了三个代码缺陷：暂停/失焦/已打开 owner 页面仍可能放行主 HUD 输入；D29 与 D3 的同帧保存快捷键可能重复消费；D9/D10 全局打开键可以覆盖已有临时页造成面板叠加，且 D9 切换酒馆后仍可能在隐藏页消费输入。

## 本轮修复

- `RuntimeHudCoordinator.PrimaryInputAllowed` 现在拒绝暂停、失焦、对话、D3 交易、D4 内容页、D9 整备页和 D10 酒馆页；`TryClaimPrimaryInput` 为同一 Unity 帧提供一次性主输入声明。
- D29 快捷键和按钮统一经过 `D7ControlCoordinator.GameplayInputAllowed` 与 `TryClaimPrimaryInput`；D29 使用 `DefaultExecutionOrder(-900)`，让声明先于默认 Update owner 生效。
- 仓库 I/Esc 仍在主 HUD 门禁前关闭，保留模态退出路径；天气、画质等 D29 入口也不会在同帧与其他主 owner 竞争。
- D9 的 P、D10 的 V 只有在自身页面已打开，或主 HUD 与 D7 均允许时才打开页面；D9 切换到 D10 前先关闭自身页面；D30 保持只读、不可交互的阶段投影。

## 实际修改文件

- `game/Assets/Scripts/Gameplay/UI/RuntimeHudCoordinator.cs`
- `game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs`
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`
- `game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs`
- `game/Assets/Tests/EditMode/D29.meta`
- `game/Assets/Tests/EditMode/D29/VCG01LogicBoundaryTests.cs`
- `game/Assets/Tests/EditMode/D29/VCG01LogicBoundaryTests.cs.meta`
- `tools/tests/test_vc_g01_logic.py`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_RESULT.json`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_EDITMODE.log`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_EDITMODE_R02.log`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_EDITMODE_R03.log`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_EDITMODE_R04.log`
- `game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_EDITMODE_R04.xml`
- `docs/game/design/common/handoffs/VISUAL-CLEAN/VC-G01-LOGIC_HANDOFF.md`
- `.workbuddy/memory/2026-09-05.md`

未修改 D19 R03 场景/Prefab、D2 输入文件、D16InputRouter、D7ControlCoordinator、港口生成器、Systems/AgentCore、存档和 D31 正式结果；当前工作区既有未提交改动全部保留。

## 验证证据

- `python -m unittest -v tools/tests/test_vc_g01_logic.py tools/tests/test_d29_interaction_hud.py tools/tests/test_d10_tavern_ui.py tools/tests/test_d29_d30_interaction_matrix.py tools/tests/test_d31_interaction_logic_audit.py`：`48/48 PASS`。
- `python tools/d31_interaction_logic_audit.py`：`PASS`，仅静态逻辑级；`python tools/d29_d30_interaction_matrix_audit.py`：`PASS`。
- `git diff --check`：`PASS`。
- Unity `VC_G01_LOGIC_EDITMODE_R04.log` 记录主程序集 Tundra build success；`VC_G01_LOGIC_EDITMODE_R04.xml` 为 `VCG01LogicBoundaryTests` 定向 `3/3 PASS`，failed=0、skipped=0、Unity 返回码 0。期间出现的既有 `VC_G01WorldLogicTests.cs` 反射类型编译阻塞已由其工作区变更消除，不属于本轮修改；本轮没有触碰该文件。
- `dotnet build game/Assembly-CSharp.csproj --no-restore`：因缺失 `game/Temp/obj/Assembly-CSharp/project.assets.json` 阻塞，未执行 restore。

## 未验证与下一步

没有真实 Player 操作，因此仍未验证真实键鼠焦点、暂停/失焦回焦、D29/D30 全入口、按钮点击与键盘边沿的时序，以及完整异常恢复矩阵。下一步执行 VC-G01 真实 Player 路线；D31-S05 保持 `PARTIAL` / `R74`，本轮没有改动或升级其正式门禁。

## 结果文件

`game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_RESULT.json`

## 2026-09-06 主 HUD 非模态边界跟进

用户观察到 W/S 移动同时切换右上角菜单后，代码审计发现 D29 主快捷 HUD 被当作模态焦点 owner。现已调整为：主 HUD 只显示状态和入口，不拥有 `D16ModalFocusScope`；仓库面板、暂停、D4、D9、D10 等真正临时页面继续各自拥有 modal focus。D31 静态审计和 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 已同步该契约。

- 定向 Python：`test_d31_interaction_logic_audit.py` + `test_d29_interaction_hud.py`，`34/34 PASS`。
- 运行时定向 PlayMode：W/S 不导航主 HUD `1/1 PASS`，证据为 [VC_G01_D16_PRIMARY_HUD_NAVIGATION_PLAYMODE_R3.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PRIMARY_HUD_NAVIGATION_PLAYMODE_R3.xml)。
- 全量 EditMode：`593/593 PASS`，证据为 [VC_G01_ALL_EDITMODE_AFTER_UI_GATING_20260906.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_AFTER_UI_GATING_20260906.xml)。
- 该跟进不改变 D31 正式状态，也不替代最后的原生 Player/人工视觉门禁；P2 全量 PlayMode、P3 碰撞和 P4 性能仍按 [pre-native gate](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PRE_NATIVE_GATE_20260906.json) 阻塞。
