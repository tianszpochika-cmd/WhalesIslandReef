# VC-A04 D2 移动输入入口交接

日期：2026-09-04  
状态：`PARTIAL`（D2 代码与源码审计完成；Unity EditMode 和真实 Player 证据受工作区既有编译阻塞）

## 已完成

- `D2RuntimeBootstrap.cs` 内新增锁存式 `D2MovementInputSourceSelector`：默认只消费 `D16InputRouter`，不会因 D16 轴为零而读取 legacy；只有显式不可用或 D16 调用异常时一次性切换到 WASD/方向键 fallback。
- fallback 切换的当帧返回零，下一帧才读取 legacy，保证同一帧不会把两套移动轴同时消费；诊断属性和读取计数使条件可审计。
- `D7ControlCoordinator.GameplayInputAllowed` 与 `RuntimeHudCoordinator.PrimaryInputAllowed` 在轴读取前共同门禁。任一为 `false` 时清空水平移动意图和残余水平速度，并在根节点旋转前返回。
- 跳跃、冲刺跟随同一锁存 provider，避免兼容键盘路径通过动作读取重新引入输入归属混用。
- 新增 D2 EditMode 测试源和 Python 源码审计，覆盖模态/主 HUD 门禁归零、焦点恢复、单 provider 消费、零轴不触发 fallback、异常后下一帧 fallback。

## 实际改动文件

- `game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`
- `game/Assets/Tests/EditMode/D2/D2MovementInputEditModeTests.cs`
- `game/Assets/Tests/EditMode/D2/D2MovementInputEditModeTests.cs.meta`（Unity 为新增测试生成）
- `tools/tests/test_d2_movement_input.py`
- `game/tools/results/VISUAL-CLEAN/VC-A04/VC_A04_UNITY_EDITMODE.log`
- `game/tools/results/VISUAL-CLEAN/VC-A04/VC_A04_UNITY_EDITMODE_R2.log`
- `game/tools/results/VISUAL-CLEAN/VC-A04/VC_A04_RESULT.json`
- `docs/game/design/common/handoffs/VISUAL-CLEAN/VC-A04_HANDOFF.md`
- `.workbuddy/memory/2026-09-04.md`

未修改 `InitialHarbor.unity`、D29/UI、D16InputRouter、D7ControlCoordinator、港口生成器、Prefab、Systems/AgentCore、存档和 D31 正式结果。

## 验证证据

- `python -m unittest -v tools/tests/test_d2_movement_input.py`：`PASS`，5/5。
- `git diff --check -- game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`：`PASS`，仅有 LF/CRLF 规范化提示。
- `dotnet build game/Assembly-CSharp.csproj --no-restore`：阻塞，缺少 `game/Temp/obj/Assembly-CSharp/project.assets.json`；未执行 restore。
- Unity 6000.0.80f1 定向 EditMode：阻塞在脚本编译阶段。`VC_A04_UNITY_EDITMODE_R2.log` 报告工作区既有 `D2CharacterAnimationController.cs` 在 `PoseFrame` 上缺少 `Bob`/`Lean`；没有生成 XML，也没有真实 Player 证据。R2 日志未报告 `D2RuntimeBootstrap.cs` 编译错误。

## D31 状态

只读核对 `game/tools/results/D31/D31-S05/D31_S05_RESULT.json`：当前为 `PARTIAL`、最新修订 `R74`。本任务没有修改或升级 D31 正式门禁。

## 下一步

由拥有 `D2CharacterAnimationController.cs` 的任务解决 Bob/Lean 编译不一致后，重跑本测试过滤器并检查 XML；随后在真实 InitialHarbor Player 中验证移动、模态开启归零、根旋转停止、关闭并恢复焦点后移动恢复。
