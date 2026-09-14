# VC-G01-WORLD-LOGIC 代码层前置审计交接

日期：2026-09-05

状态：`PARTIAL`；本轮范围结论：`PASS_WITHIN_SCOPE`

## 结论

已完成 InitialHarbor 的 D19 R03 视觉/碰撞/相机桥接代码层清理。确认并修复了会导致“画面显示空处但玩家无法行走”、可见地形与碰撞错位、旧层相机状态被重新打开以及探针自身异常的确定性缺陷。

本轮没有启动真实 standalone Player，也没有伪造 WASD、转向、首屏画面或镜头遮挡证据。因此 `VC-G01` 仍为 `IN_PROGRESS`，本交接不升级整体 D31；D31 正式结果继续为 `PARTIAL`。

## 确定性问题与修复

- `D19R03HarborRuntimePresentation` 不再全场景按名字找 `ShoreBoundary`，只接受 `D6_HarborCollision/ShoreBoundary` 下启用的非 Trigger Collider，并在移动边界后同步物理变换。
- R03 桥接现在清理 `D19_SaintMargaret_Root` 下所有启用的实体 Collider，保留 Trigger，避免旧建筑、旧地形、旧码头道具在不可见状态下阻挡 CharacterController。
- `HarborCollisionController` 保持 D6 唯一碰撞所有者；若 D2 的一次性 `ConfigurePhysics` 先安装 fallback，组件会在 `InitialHarbor` 以受控间隔等待 R03 网格并升级到同源 MeshCollider，同时关闭被覆盖的 fallback 面。
- 固定镜头探针记录并恢复 `CameraFollow` 原始 `enabled` 状态，且 rerun 参数按数字通用解析，避免新轮次覆盖 RERUN15 证据。
- 路线探针在调用 `Physics.ComputePenetration` 前检查玩家 Collider，并让 R03 landmark 优先从当前启用的 R03 表现子树解析，避免隐藏旧版同名节点污染证据。

## 实际改动文件

- `game/Assets/Scripts/Gameplay/D19/R03/D19R03HarborRuntimePresentation.cs`
- `game/Assets/Scripts/Gameplay/Harbor/HarborCollisionController.cs`
- `game/Assets/Scripts/Gameplay/D19/R03/D19R03FixedShotProbe.cs`
- `game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs`
- `game/Assets/Tests/EditMode/D19Interaction/VC_G01WorldLogicTests.cs`
- `game/Assets/Tests/EditMode/D19Interaction/VC_G01WorldLogicTests.cs.meta`
- `tools/tests/test_vc_g01_world_logic.py`

本轮未触碰 `InitialHarbor.unity`、Prefab、港口生成器、D29/UI、D2RuntimeBootstrap、D2 输入、Systems/AgentCore、存档和 D25-D31 正式状态文件；这些路径的工作区原有其他 Agent 改动保持不动。

## 验证证据

- 定向 Python：`python -m unittest tools/tests/test_vc_g01_world_logic.py tools/tests/test_d19_r03_visual_builder.py tools/tests/test_camera_follow.py`，`31/31 PASS`。
- Unity 6000.0.80f1 定向 EditMode：`WhalesIslandReef.Tests.D19Interaction.VcG01WorldLogicTests`，`4/4 PASS`、`failed=0`、`skipped=0`；同时完成变更脚本编译。
  - [XML 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-WORLD-LOGIC/VC_G01_WORLD_LOGIC_EDITMODE_R1.xml)
  - [Unity 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-WORLD-LOGIC/VC_G01_WORLD_LOGIC_EDITMODE_R1.log)
- 全量 Python 发现 1078 个测试中有 2 个既有范围外失败：D27 R03 角色审计结果为 `BLOCKED`、D35 材质审计结果为 `PARTIAL`。本轮没有修改对应模块；定向范围仍为通过。
- 静态输入审计：[D19_R03_A01_SCENE_COLLIDER_AUDIT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json) 仅作为问题发现依据，不作为真实运行时通过证据。

完整结果：[VC_G01_WORLD_LOGIC_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-WORLD-LOGIC/VC_G01_WORLD_LOGIC_RESULT.json)

## 未验证项与下一步

- 未验证真实 Player 首屏非空、W/A/S/D 连续移动、脚底接触稳定、左右无蛇形摆动、转向、船体/建筑镜头遮挡。
- 未验证真实 Player 中 R03 延迟加载与 D6 fallback 升级的完整时序。
- 下一步使用最新 RERUN48 或后续 Player 完成真实输入矩阵，读取路线探针 JSON 与截图；只有这些证据齐全后才能决定 `VC-G01` 是否收口。
