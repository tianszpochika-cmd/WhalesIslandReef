# VC-A16 首屏岸线与道路几何修复交接

日期：2026-09-05  
状态：`PARTIAL`（本任务限定范围已完成；D25—D31 正式运行时/性能门禁仍保持原状态）

## 结论

本任务关闭了一个真实的 R03 表现层回归：旧版 `HideLegacyVisuals` 按名称遍历全部根节点，把当前 R03 的 `Beach` 也隐藏了。修复后沙滩重新出现在 Player 中；同时把沙色地表从世界 Z 阈值改成岛屿边缘环带，把沙滩和道路改为按 `Height` 贴地的点列带，把泡沫压回水线并缩短，减少悬空青色条和矩形地板感。

## 实际修改

- `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`
  - `HideLegacyVisuals` 跳过 `D19_R03_HarborPresentation`，只清理历史表现根节点。
  - `SurfaceMaterial` 使用椭圆边缘半径生成两级沙色海岸环带，移除南侧世界 Z 沙带和路线内意外沙条。
  - 新增 `MakePathMesh`，为沙滩、主路和仓库路生成贴合 `Height` 的折线带。
  - 泡沫改为固定水线高度、向外偏移、短尺寸的 24 个装饰条。
- `game/Assets/Scripts/Gameplay/D19/R03/D19R03FixedShotProbe.cs`
  - 增加 RERUN20/RERUN21 的证据目录选择，避免覆盖旧固定镜头结果。
- `tools/tests/test_d19_r03_visual_builder.py`
  - 新增旧版清理隔离、贴地道路/沙滩和泡沫水线约束测试；同步修正已被删除的近景棕榈位置断言。

## 验证证据

- Python 定向回归：65/65 PASS。
- R03 Prefab/InitialHarbor 重建：`game/tools/results/VISUAL-CLEAN/VC-A09_R03_BUILD_R02.log`，包含 `Built and placed`，未发现 `error CS` 或构建异常。
- 角色/动物表现层重建：`game/tools/results/VISUAL-CLEAN/VC-A09_D27_BUILD_R02.log`，记录 `npc=12, wildlife=8, pets=2`；末尾的 Curl 42 是编辑器服务回调告警，不影响已记录的 attach 结果。
- Player 重建：`game/tools/results/VISUAL-CLEAN/VC-A09_PLAYER_BUILD_R02.log`。
- 真实 Player 固定镜头：`game/tools/results/D19/D19-R03-A01/D19_R03_A02_FIXED_SHOT_PLAYER_RERUN21.json`，1920×1080、14/14 张 backbuffer 截图；本次只证明 Calm，不证明天气矩阵。
- 场景渲染器诊断：`game/tools/results/VISUAL-CLEAN/VC-A09_SCENE_RENDERER_DIAGNOSTICS.json`。当前 `Beach` 为 enabled，泡沫渲染器水线高度约 0.02，R03 可玩区域没有近景 `Palm_*`。

## 未完成与边界

- R03 仍是低模风格；最终美术品质不是本任务的 PASS 条件。
- D19/D31 的天气变体、完整路线、D29/D30/D31 负向交互/恢复和性能 1% low 仍需后续正式验证。
- 未修改任何玩法系统、数值裁决、D6/D19 碰撞所有权或 D25—D31 正式结果文件。
- `game/tools/results/D31/D31-S05/D31_S05_RESULT.json` 继续保持 `PARTIAL/R74`。

## 下一任务

进入 `VC-S01`：执行 Unity 编译、定向 EditMode、静态场景/Prefab 审计，统一核对本轮结果和任务台账；通过后再进入 `VC-G01` 当前 Player 路线与交互回归。不要把本次 14/14 Calm 截图直接升级为最终视觉或 D31 PASS。

