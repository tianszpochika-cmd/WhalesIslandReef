# D2 航行与对话闭环验收报告

> 验收日期：2026-08-09
> 状态：代码与自动化验收完成，可进入 D3 规划。

## 结论

D2-01 至 D2-10 已按依赖顺序执行完成。D2 的本地 M0 闭环已经具备：人物移动、第三人称相机、船只前进/减速/转向、海面占位、昼夜、老水手对话、AgentReply 校验、Mock 降级、好感裁决、存档/读档和 ESC 暂停返回。

## 验收证据

| 检查项 | 结果 | 证据 |
|---|---|---|
| AgentCore 独立编译 | ✅ | `dotnet build game/AgentCore/AgentCore.csproj --nologo`，0 警告/0 错误 |
| AgentCore 离线自测 | ✅ | `dotnet run --project game/AgentCore/AgentCore.csproj -- --mock`，全部通过 |
| 服务端编译 | ✅ | `server` 目录 `npm run build` 通过 |
| Unity 6 脚本编译 | ✅ | Unity `6000.0.80f1` 临时副本完成全量资源导入和脚本编译 |
| Unity EditMode 测试 | ✅ | `Save.Tests` 共 23 项：23 通过、0 失败、0 跳过 |
| 航行/昼夜/契约测试 | ✅ | `D2.Tests` 共 15 项全部通过 |
| 好感与存档闭环 | ✅ | `D2Runtime_SaveLoad_RestoresLocationClockAndAffinity` 通过 |
| 强逻辑边界 | ✅ | `Systems/D2` 未引入 LLM、HttpClient、UnityWebRequest 或网络代码 |

## 运行入口

- InitialHarbor 进入后由 `D2RuntimeBootstrap` 幂等接线，缺失占位资产时自动创建可见船、人和海面。
- 人物：WASD/方向键；相机：按住鼠标右键或中键拖动，滚轮缩放；场景有船员模型时玩家复用该模型，不再默认显示 Capsule。
- 靠近船按 E 或点击 D5 HUD 的“上船/下船”进入操舵；登船后玩家模型跟随船体，W/S 前进/减速，A/D 转向；再次按 E 或点击按钮离船。
- 靠近老水手按 E 打开对话；Ollama 不可用时自动使用 Mock/模板。
- ESC 打开暂停菜单；可继续、返回主菜单或退出；返回前保存航行与昼夜快照。

## 已知边界

- 本阶段没有引入 Crest；`OceanAdapter` 使用可替换的占位海面。
- 服务端网关是开发态基础接口，本地游戏闭环不依赖它。
- Unity 编辑器已打开主工程时，自动化验收使用临时副本，避免并行打开同一项目导致 Unity 锁冲突；主工程没有复制 `Library/` 或测试生成物。
- 模型、海面和 UI 仍是灰盒表现，D3 只在 D2 接口稳定后进行内容与资产升级。

## 运行时控制修正（2026-08-09）

- 根因：旧版 `EnsurePlayer` 只创建蓝色 Capsule，没有把场景中的 `ph_crew_male` 作为玩家外观；旧版 `EnterShip` 只切换了控制模式和相机，没有把玩家放到船上或设为船体子物体。
- 修复：优先复制场景船员模型为 `D2_PlayerAvatar`，素材缺失时才使用 Capsule 降级；登船时将玩家放置到甲板并跟随船体移动，下船时解除父子关系并放到船侧安全位置。
- 交互：D2 保留 E 键，同时 D5 右侧 HUD 增加可点击的“上船/下船 [E]”按钮；船舶交互范围扩大为 8 米，并优先处理上船，避免老水手靠近船头时抢占 E 键。
- Unity 6 临时副本运行时冒烟通过：日志确认 `ph_crew_male -> D2_PlayerAvatar`，自动模拟“上船→玩家成为船体子物体→下船→解除绑定”全部成功；无 C# 异常、空引用或编译错误。

## D2.5 角色动作体验层增量验收（2026-08-09）

- 因 D2 原始范围只要求人物能走动，且现有 `ph_crew_male.fbx` 没有 AnimationClip，本次以程序化动作控制器补齐待机、走路、奔跑、跳跃和上下船过渡。
- 控制：WASD/方向键移动，Shift 奔跑，Space 跳跃，E 或 D5 HUD 按钮上下船。
- Unity 6 临时副本冒烟通过：8 个骨骼已绑定，角色模型、上下船父子关系、跳跃状态、D5 主 HUD、海图和 12 个岛标记均通过；无 C# 异常、空引用或编译错误。
- 详细范围、验收门和最终动画资源边界见 `docs/game/design/D2_5_SCOPE.md`。
