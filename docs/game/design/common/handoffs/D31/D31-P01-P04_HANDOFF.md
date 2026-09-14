# D31-P01—P04 交接

## 结论

代码层已完成 D31 的质量控制入口；其中性能数据和固定镜头保持 `PARTIAL`，没有虚构硬件验收。

## 证据

- `game/Assets/Scripts/Gameplay/D31/D31VisualQualityRuntime.cs`
- `game/Assets/Scripts/Gameplay/D25/D25VisualTheme.cs`
- `game/Assets/Scripts/Gameplay/D28/D28WaterWeatherPresentationRuntime.cs`
- `tools/tests/test_d28_d30_d31_presentation.py`

Low/Medium/High 只改变 Unity 表现质量参数。1920×1080 帧时间、1% low、GC、14 个固定镜头和三天气截图已由一次性受控 Unity Player 采样补充，但性能 1% low 未达到 50 FPS 目标，固定镜头虽齐全仍存在低模/块状几何，Rain/Storm 雨丝不够清晰，所以 P03/P04 继续为 `PARTIAL`。

## 2026-09-03 R14 证据补充

- P03：Low/Medium/High 实际采样已归档到 `game/tools/results/D31/D31-P03/`；平均 FPS 约为 `59.62/59.66/59.80`，1% low 约为 `44.29/43.65/48.77`，长帧超过 50ms 为 0。
- P04：Player 真实捕获 `MainMenu + InitialHarbor` 的 14 个固定镜头和 Calm/Rain/Storm 三天气变体，共 17 张非空 PNG，归档到 `game/tools/results/D31/D31-P04/`。
- P03/P04 的结果 JSON 已写入具体数据和缺口；不把采样存在误报成性能达标，也不把镜头数量达标误报成最终画质达标。

## 2026-09-03 R27 表现生成器补充

R27 修复 `D19R03HarborPresentationBuilder.MakeBevelBoxMesh` 的真实实现：建筑、门窗、箱子、码头边缘和船体使用的倒角接口不再退化成普通硬盒体。重新生成后的 Editor 审计为 `315 Renderer / 15,609 mesh vertices / 1 R03 collider`；Unity 短编译成功，生成器静态测试 `3/3 PASS`。

本补充没有重新捕获 Player 固定镜头，也没有重新测量 Low/Medium/High 性能；低模材质、平坦地表和最终画质门禁仍保持 `PARTIAL`。

## 2026-09-12 Q26-A14：R03 岸线碰撞 owner 修复

连接 Unity Pipeline 的 PlayMode 采样复现了一个真实 D31-P04 几何/物理缺陷：进入 PlayMode 后，Unity 静态合批将 `D19_R03_IslandSurface.MeshFilter.sharedMesh` 变成 `Combined Mesh (root: scene) 4`；D6 若继续把该视觉网格复制到 `D19_TerrainRoot`，最外侧 `ShoreRock_0` 的碰撞命中点会比可见岩石底部高 `0.290m`。该第一次 Q26A14 失败已留在结果说明中，没有用固定 token 覆盖。

最小修复在 `game/Assets/Scripts/Gameplay/Harbor/HarborCollisionController.cs`：保留场景作者已保存的 `D19_TerrainRoot` `MeshCollider.sharedMesh` 作为唯一运行时地形碰撞源，只同步 owner Transform，不再从静态合批后的视觉 MeshFilter 重建碰撞。R03 Builder 和 `Q26A14ShorelineRocksHaveOwnedTerrainContact` 则负责确保九个岸线岩石仍按自有地形接地。

证据：Fresh PlayMode 中 terrain mesh=`D19_R03_IslandSurfaceMesh`，9/9 岩石接地误差=`0.018–0.022m`；Q26A14=`1/1 PASS`；完整 Q26=`14/14 PASS`；recompile=`failed=false/errors=[]`；Player build `build_0cc514c48042`=`StandaloneWindows64 / 0 errors / 8 warnings / 175181851 bytes`，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；Editor=`ready/stopped`、error console=`0`。

这只关闭了一个可复现的碰撞/接地缺陷。没有在本卡采集 native Player 截图或前台输入，因此 D31-P04 仍为 `PARTIAL`；低模/程序化环境、视觉质量、目标设备性能和完整 D31 门禁仍开放。权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_PIPELINE_20260912_RESULT.json`、`game/tools/results/D31/D31-P04/D31_P04_RESULT.json`。下一步仍按 `VC-G01 → VC-G02 → VC-G03` 依赖推进，不提前升级后续状态。
