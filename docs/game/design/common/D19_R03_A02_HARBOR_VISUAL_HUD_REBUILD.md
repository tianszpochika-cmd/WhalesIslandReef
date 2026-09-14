# D19-R03-A02 港口实景、画面与 HUD 重建记录

结论：**PARTIAL**。

本工作包建立了 R03 专属表现层并在真实 Windows Player 中检查。它不改变玩法系统、交互锚点、存档、Registry 或外部资源状态；旧的 `D19_SaintMargaret_Root` 渲染器被关闭，但其碰撞、稳定节点和兼容入口保留。

## 实际修改

- `game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab`
- `game/Assets/Art/Materials/D19/R03/`
- `game/Assets/Scenes/InitialHarbor.unity`：仅挂载 R03 Integration Prefab，并关闭历史视觉代理渲染器。
- `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`
- `game/Assets/Scripts/Gameplay/D19/R03/D19R03HarborRuntimePresentation.cs`

R03 表现层包含项目自有的起伏地表、沙/土/草/岩过渡、岸线、浅水/泡沫、道路、屋顶/门/窗/招牌/货箱的建筑模块、码头木板/桩/绳索/坡道、棕榈/灌木/岩石和四个 intake-eligible Kenney Nature Kit 模型。R03 Audit 记录 **191 个 Renderer、3,639 个网格顶点、R03 自身 0 Collider**；旧碰撞仍由原稳定节点承担。

## 真实 Player 对照

旧基线：`game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_spawn_rerun2.jpg`。该图仍可见旧前景代理堆叠。

最新：`game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_spawn_rerun4.jpg`。旧前景代理已不再遮挡，玩家 Humanoid、道路、地表和植被可见；但宽阔平坦色块、远处块状建筑轮廓和底部路线 HUD 仍可见，房屋可读性不足。故不能把该图写成最终视觉通过。

## 未闭合门禁

1. 尚无 14 个固定 Player 镜头和 3 个真实天气变体。
2. 尚无 5—10 分钟完整玩家路线、交互、登船、出航、返港和保存恢复证据。
3. 画面仍有低细节建筑/平坦地表债务；需要新的 R03 视觉修复轮次或合法资源补齐，不能靠截图裁切或雾化隐藏。
4. 性能取证独立记录在 D19-R03-A03，不由本报告代替。

机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT.json`、`D19_R03_A02_VISUAL_QA.json`、`D19_R03_A02_EDITOR_AUDIT.json`。

## RERUN13 补充（2026-08-16）

RERUN13 修复了生成网格仅存在于编辑器临时对象、Prefab `MeshFilter` 为空导致 Player 建筑丢失的问题。生成器现在将 297 个网格资产写入 `game/Assets/Art/Materials/D19/R03/Meshes/` 并写入 Prefab 引用。新的 Editor audit 为 311 个 Renderer、310 个启用 Renderer、5,996 个网格顶点；R03 自有 Collider 仍为 0，旧稳定根的碰撞和锚点保持不变。

真实 Player `game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_spawn_rerun13_player.jpg` 显示屋顶、立面、门窗、市场入口、摊位、码头木板和道具，未观察到粉色材质或大面积白色代理平面。局部平坦地表、近景屋顶重叠和远景细节不足仍存在，14 个固定镜头、天气、路线和保存恢复继续留给后续门禁，结论不变为 PARTIAL。

## RERUN14 补充（2026-08-16）

本轮在不改 D2/D6/D7 船舶和碰撞逻辑的前提下，为稳定 `ph_ship_fishing` 节点安装了独立的 `D19_R03_ShipVisual.prefab`。该 Prefab 仅包含表现几何和显式 `D6_ShipHullCollision`，原占位船根节点和名称保留，旧渲染未删除。Player 构建、Editor 审计和真实画面路径分别为：

- `game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN14.log`
- `game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN14.log`
- `game/tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_RERUN14.log`
- `game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_ship_visual_rerun14_player.jpg`

真实 Player 画面已观察到船/码头模块、船体栏杆与港口建筑共同出现，未见粉色材质或大面积白色代理面；同时仍能看到近景屋顶/路面重叠、局部平坦地表和底部调试 HUD。因此本增量改善了船体表现，但不能关闭最终视觉、固定镜头、天气、路线、保存恢复和性能门禁，结论仍为 PARTIAL。

## RERUN15 固定 Player 镜头增量（2026-08-16）

本轮新建的是只读运行时取证探针和非调试验证 Player，不修改 InitialHarbor、Prefab、天气状态或存档。真实 Player 在 1920×1080 回读并写出 14 张固定镜头（MainMenu 1 张 + InitialHarbor 13 张）：

- 机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN15.json`
- 镜头结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_FIXED_SHOT_PLAYER_RERUN15.json`
- 截图目录：`game/tools/results/D19/D19-R03-A02/fixed_cameras_rerun15/`
- 构建/运行日志：`D19_R03_A02_PLAYER_BUILD_RERUN15.log`、`D19_R03_A02_FIXED_SHOT_PLAYER_RERUN15.log`

14/14 文件真实存在且为 1920×1080；所有港口镜头只观察到 `Calm`，因此天气变体为 0/3。人工检查 `spawn`、`market`、`dock_overview`、`coastline` 等镜头时仍发现大面积平坦色块、灰色柱/三角代理、屋顶/道路近景重叠、低细节建筑和底部开发路线 HUD。未观察到粉色缺材质，但这不足以通过视觉门禁。本轮结论保持 **PARTIAL**。

## RERUN17 目标修复与真实 Player 固定镜头（2026-08-16）

本轮针对 RERUN15 的人工问题做了范围内修复：降低 R03 自有材质的高饱和度，移除表现层中的巨大三角树实例，关闭旧稳定节点的遗留表现渲染器但保留节点/碰撞/兼容入口，将浅色竖直绳代理改为有角度的棕色系泊线，并缩小市场入口棚架。没有修改 Systems、Registry、Save Schema、资源状态或 D19-A01—A06 工作成果。

- 构建日志：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN17.log`
- Editor 审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT_RERUN17.json`
- 独立 Player：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_Player_RERUN17.exe`
- 固定镜头机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_FIXED_SHOT_PLAYER_RERUN17.json`
- 逐镜头 QA：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_VISUAL_QA_RERUN17.json`
- 截图：`game/tools/results/D19/D19-R03-A02/fixed_cameras_rerun17/`

真实 Player 在 1920×1080 下输出 14/14 张截图，尺寸检查 14/14 通过；未观察到粉色缺材质或大面积纯白代理平面，旧三角树与浅色竖杆债务有所降低。但人工检查仍确认平坦地表/码头、低细节块状建筑、近岸代理几何和底部路线 HUD，且天气只有 Calm（0/3）。因此本轮是 **PARTIAL**，不能作为最终港口视觉通过。

## RERUN21/RERUN23 时序修复与输入复验（2026-08-16）

本轮修正了表现桥接在 `ShoreBoundary` 尚未创建时提前结束的问题：运行时最多等待 180 帧，确认 `ShoreBoundary` 后才应用路线碰撞窗口；现有节点移动到 `z=17.0`，D6 碰撞所有权保持不变。Unity 表现重建和 Player 构建均退出码 0，详见 `game/tools/results/D19/D19-R03-A02/D19_R03_A02_BUILD_RERUN21_COLLISION_WAIT.log` 与 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_BUILD_PLAYER_RERUN21_UNITY.log`。

RERUN21/RERUN23 的真实 Player 日志确认桥接确实执行，但 RERUN23 仍观察到 `moved_observed=false`、`save_input_observed=false`；因此这次修复不能被解释为连续路线或保存恢复通过。独立机器结果为 `game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN23_INPUT_RECHECK.json`，输入复验报告为 `docs/game/design/common/D19_R03_S01_RERUN23_INPUT_RECHECK.md`。本包结论仍为 **PARTIAL**。

## RERUN26 候选视觉增量（2026-08-16）

RERUN26 在现有 R03 表现层中使用 8 个已通过 intake 证据、且允许 D19-A01/D19-A03 的 NatureKit 候选。生成式地表和建筑 fallback 保留，新增候选仅作为树、岩、草、路径和岩岸层次的表现引用，不承担碰撞或玩法所有权。候选路径和准入证据由 `D19_R03_A02_EDITOR_AUDIT.json` 与 D18 intake catalog 共同追溯。

真实 Player 画面 `game/tools/results/D19/D19-R03-A02/RERUN26_PLAYER_PORT_AFTER_CANDIDATES.jpg` 显示港口、市场入口、木码头、树/岩/草和路径候选已进入 Player，材质未呈粉色；同时仍可见块状建筑、平坦地表和底部开发路线 HUD。故视觉债务仍未闭合，未产生 14 镜头或天气矩阵的新通过证据，A02 保持 **PARTIAL**。

机器审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN26_CANDIDATES_REFRESH.log`，退出码 0；历史 RERUN26 统计为 315 Renderer、314 enabled、6457 顶点、R03 自有 Collider 0。R03 自有 Collider 为 0 是既有表现层边界，稳定 D2/D6 节点继续负责碰撞，不能被解释为碰撞门禁通过。

## RERUN28_ROUTE_GROUND 增量

R03 Integration 新增受控 `D19_TerrainRoot` BoxCollider 以覆盖 D19 R03 视觉路线的基础接地缺口；它没有 Renderer，不承担内容资源或玩法逻辑所有权。Unity 只读审计确认 Collider 数从历史 RERUN26 的 0 变为 1，Renderer 315、enabled 314、网格顶点 6457、8 个 intake-eligible 外部候选保持不变。该修复随后由 RERUN29 真实 Player 读取并在日志中记录接地恢复，但完整路线仍未闭合，A02 结论继续 **PARTIAL**。

## RERUN35 视觉几何收敛（2026-08-16）

本轮将近景遮挡作为表现债务修复：屋面脊高、前后门廊深度、市场入口框架高度和远景岩体/棕榈尺度均在 R03 生成器内收敛，未改固定坐标、交互锚点或任何强逻辑。真实 Player 回读确认港口建筑轮廓和市场入口更清晰，但仍有平坦地表、低细节建筑、开发路线 HUD 和缺少天气矩阵等未闭合问题。

- 机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN35_VISUAL_PATCH.json`
- Unity 构建/审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN35_VISUAL_COLLAPSE.log`、`D19_R03_A02_EDITOR_AUDIT.json`
- Player/探针归档：`D19_R03_A02_RUNTIME_VISUALPATCH_PLAYER_RERUN35.json`、`D19_R03_A02_RUNTIME_VISUALPATCH_PLAYER_RERUN35.log`
- 实测：1920×1080，254.29 秒；5 张探针截图；本轮移动和保存输入没有形成可复核证据，故不宣称路线通过。

本报告结论仍为 **PARTIAL**。

## R50 建筑标牌可读性修复（2026-09-03）

复核 R29 Player 固定镜头时发现，四栋程序化建筑同时生成前后两块同名标牌；从码头斜视角观察时，后立面标牌会从建筑侧面露出，造成重复文字和场景噪声。R50 删除 `BackSignBoard`/`BackSign`，保留后立面门、窗、棚架和建筑几何，不改变交互锚点、碰撞、业务 owner、存档或天气逻辑。

- 生成器：`game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`
- 重建日志：`game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R50.log`
- 审计日志：`game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R50.log`
- Editor 审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`

重建结果为 `307 Renderer / 22004 mesh vertices / 1 Collider`，Prefab 中后立面标牌标记为 0；D28—D31 相关回归 47/47 PASS（其中 D19 R03 builder 测试 5/5）。随后在真实独立 Windows x64 Player 中重新采集 14/14 固定镜头和 3/3 Calm/Rain/Storm 天气镜头，分辨率为 1920×1080，真实 backbuffer 截图已归档到 `game/tools/results/D31/D31-P04/visual_evidence_R50/`。截图确认重复标牌已清理，但低模/程序化建筑、平坦地表、杂乱竖向几何、粗糙环境道具和高密度 HUD 仍存在，因此本报告和 D31 视觉门禁仍为 `PARTIAL`。

Player 复验结果：[D31_P04_R50_REVALIDATION_RESULT.json](../../../../game/tools/results/D31/D31-P04/D31_P04_R50_REVALIDATION_RESULT.json)。
