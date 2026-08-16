# D19-R03-A02 交接

- 任务：D19-R03-A02
- 结论：**PARTIAL**
- 前置：D18-S02-R02 保持 PASS；D19 历史 R02 结果保持 PARTIAL。

## 修改范围

建立并接入 `D19_R03_HarborPresentation` Integration Prefab、R03 材质和运行时 HUD 压缩桥；没有修改 Systems、AgentCore、Registry、Save Schema、contracts、server/web 或 D19-A01—A06 工作目录。

## 真实证据

- Unity builder/build：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN5.log`，退出码 0。
- Editor audit：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`；R03 renderer 191、mesh vertices 3639、R03 collider 0；历史根渲染器 enabled=0。
- Player build：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_RERUN4.log`，退出码 0。
- 真实画面：`screenshots/r03_a02_spawn_rerun2.jpg`（对照）、`r03_a02_spawn_rerun3.jpg`、`r03_a02_spawn_rerun4.jpg`（最新）。

## RERUN6/7 增量

- 重新构建 R03 Prefab 与场景：`D19_R03_A02_UNITY_BUILD_RERUN7.log`、`D19_R03_A02_UNITY_AUDIT_RERUN7.log`，均退出码 0。
- 新 Editor audit 为 199 个 Renderer、198 个启用、3,639 顶点、0 个 R03 Collider；历史稳定根的 957 个 Renderer 仍保持禁用，未删除碰撞/锚点。
- 新 Player 构建：`D19_R03_A02_Player_RERUN6.exe`，构建退出码 0。真实 Player 再次观察到出生画面仍有平地、弱建筑轮廓和底部路线层；追加屋顶几何没有让出生固定镜头达到最终视觉门槛，因此结论仍为 PARTIAL。

## 结论与下一步

已消除旧前景代理遮挡，但尚未消除所有平地/块状建筑债务；14 镜头、天气、路线和保存恢复尚未在本包完成。D19-R03-A03 已另外完成真实 Player 1080p 性能取证，但 D19 最终门禁仍锁定。后续任务必须继续使用 R03 Integration 目录并保留旧节点回滚。

## RERUN13 增量（2026-08-16）

- 历史 RERUN5/6/7 证据保持不变并继续可追溯；本节只补充新证据。
- `D19_R03_A02_UNITY_BUILD_RERUN13.log` 与 `D19_R03_A02_UNITY_AUDIT_RERUN13.log` 均退出码 0。生成器现在把 297 个网格 `.asset` 写入 `game/Assets/Art/Materials/D19/R03/Meshes/`，并回读写入 Prefab，修复了 Player 中临时网格丢失的缺陷。
- 最新 Editor audit 为 R03 311 Renderer、310 enabled、5,996 顶点、0 R03 Collider；旧稳定根 957 个渲染器仍禁用，碰撞/锚点未删除。
- `D19_R03_A02_PLAYER_BUILD_RERUN13.log` 退出码 0；真实 1920×1080 Player 画面为 `screenshots/r03_a02_spawn_rerun13_player.jpg`。该画面可见屋顶、立面、门窗、市场入口、摊位、码头木板和道具，未观察到粉色材质或大面积白色代理平面。
- 仍存在近景屋顶重叠、局部平坦地表、远景细节不足；本包仍为 PARTIAL，不宣称 14 固定镜头、天气、连续路线、保存恢复或本次几何变化后的性能门禁通过。

## RERUN14 增量（2026-08-16）

- 新增并接入 `game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_ShipVisual.prefab`；只作为稳定 `ph_ship_fishing` 节点下的表现子 Prefab，保留 D2/D6 对根节点、船舶状态和碰撞所有权。
- 船体表现包含 Hull、Deck、Bow、Mast、Sail、HelmStand、HelmWheel、栏杆和显式 `D6_ShipHullCollision`；场景保留原稳定船节点，原占位渲染关闭但未删除。
- Unity 构建日志：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN14.log`；Editor 审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN14.log` 与 `D19_R03_A02_EDITOR_AUDIT.json`；Player 构建：`D19_R03_A02_PLAYER_BUILD_RERUN14.log`。
- 真实 Player 截图：`game/tools/results/D19/D19-R03-A02/screenshots/r03_a02_ship_visual_rerun14_player.jpg`。观察到船/码头模块进入 Player，无粉色材质和大面积白色代理面；仍存在近景屋顶/路面重叠、局部平坦地表和调试 HUD，A02 仍为 PARTIAL。
- 未完成：14 个固定镜头、天气变体、最终视觉债务清除、碰撞/路线/保存恢复和正式性能门禁；不可将本增量标为 PASS。

## RERUN15 增量（2026-08-16）

- 本轮结论：**PARTIAL**。
- 真实非调试 Player：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_Player_RERUN15.exe`，1920×1080、D3D11，ScreenCapture 产生 14/14 张固定镜头。
- 机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN15.json`；镜头结果：`D19_R03_A02_FIXED_SHOT_PLAYER_RERUN15.json`。
- 真实镜头目录：`game/tools/results/D19/D19-R03-A02/fixed_cameras_rerun15/`。已确认港口几何、市场/码头标识和角色可见；同时确认平面/代理柱/屋顶重叠、低细节与开发 HUD 仍在。
- 只观察到 Calm，Rain/Storm 仍为 0/3；本探针没有写 EnvironmentSystem，也没有改变正式场景或存档。

## RERUN17 交接增量（2026-08-16）

- 结论：**PARTIAL**。
- 实际修改：R03 受控材质调色、市场入口棚架尺寸、表现层系泊线、表现层巨大候选树实例，以及稳定遗留表现 Renderer 的关闭；稳定节点、碰撞、交互锚点和兼容入口保留。
- 真实构建：Unity 场景重建和非 Development Windows Player 构建退出码 0，日志位于 `game/tools/results/D19/D19-R03-A02/`。
- 真实 Player 证据：`D19_R03_A02_FIXED_SHOT_PLAYER_RERUN17.json`，14/14 张 1920×1080 backbuffer 截图，目录 `fixed_cameras_rerun17/`；逐镜头记录见 `D19_R03_A02_VISUAL_QA_RERUN17.json`。
- 通过子项：无粉色材质、大面积白色代理平面未观察到、截图文件与尺寸完整、旧三角树/浅色竖杆视觉债务降低。
- 未通过子项：平坦地表/码头、块状低细节建筑、近岸代理几何仍在；天气仅 Calm，0/3；本轮没有真实玩家路线、交互或保存恢复证据。
- 资源与边界：未提升 Candidate/ImportVerified 状态，没有写 LocalDevelopmentReady/ReleaseApproved；未修改 Systems、AgentCore、Registry、Save Schema、contracts/server/web 或 A01—A06 工作目录。
- 回退：回退 `InitialHarbor.unity`、R03 Integration Prefab 和 R03 材质生成器到本轮前快照即可；本轮未删除稳定兼容节点。

## RERUN28_ROUTE_GROUND（2026-08-16）

- 在不改变 D2/D6 所有权的前提下，R03 Integration Prefab 增加命名 `D19_TerrainRoot` 的受控路线 BoxCollider；Unity 静态审计从历史 RERUN26 的 R03 自有 Collider 0 更新为 Collider 1，8 个外部候选和 315 Renderer 未变。
- 新审计：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`；构建/审计日志：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN28_ROUTE_GROUND_ONLY.log`。
- 该变化只证明 D2 运行时可以识别路线接地面，不等于完整玩家路线、保存恢复、天气或视觉门禁通过；R03-A02 仍 **PARTIAL**。

下一责任入口是 D19 R03 A02 或新的受控视觉修复任务继续处理地表/建筑/近岸细节，并由 EnvironmentSystem 控制采集 Calm/Rain/Storm；不能用本交接放行 D19-R02-S02 或 D20 正式集成。

## RERUN35_VISUAL_PATCH（2026-08-16）

- 结论保持 **PARTIAL**。本轮只修正 R03 Integration 表现几何，不改变 D2/D6/D7 所有权、稳定交互、碰撞、存档、Systems 或资源状态。
- 屋顶从 `1.1m` 高脊和全深度门廊收敛为低脊屋面；前后门廊缩短；市场入口改为低矮 sign frame；远景岩体/棕榈缩小，减少第三人称近景的遮挡和锥形占位感。
- Unity 构建成功：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_BUILD_RERUN35_VISUAL_COLLAPSE.log`。
- Editor 审计成功：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`，315 Renderer、314 enabled、6457 网格顶点、1 个 R03 Integration 路线 BoxCollider、8 个已准入环境候选引用。
- 非开发 Player 构建成功：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN29_VISUALPATCH.log`；实际 Player 1920×1080 运行约 254.29 秒，生成 5 张探针截图，结果与探针归档在：
  - `game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN35_VISUAL_PATCH.json`
  - `game/tools/results/D19/D19-R03-A02/D19_R03_A02_RUNTIME_VISUALPATCH_PLAYER_RERUN35.json`
  - `game/tools/results/D19/D19-R03-A02/D19_R03_A02_RUNTIME_VISUALPATCH_PLAYER_RERUN35.log`
- 本轮实际画面确认屋顶近景遮挡比 RERUN29 收敛，但仍可见平坦地表/码头、低细节建筑和开发路线 HUD；本轮没有天气变体、完整路线、保存/读档或性能新证据。输入探针本轮 `moved_observed=false`、`save_input_observed=false`，因此不把短测写成玩家路线通过。
- 旧 RERUN29 长路线结果和其归档继续保留；本轮只将短测复制为 RERUN35 独立结果，不覆盖长路线机器事实。

下一责任入口仍为 D19-R03-A02 的受控视觉/天气补齐与 D19-R03-A01 的真实输入路线复验；D19-R03-S01 继续 BLOCKED，不能放行 D19-R02-S02 或 D20 正式集成。

## RERUN35_FIXED_SHOTS

- 在 RERUN35 几何变更后重新构建固定镜头 Player，并从真实 1920×1080 backbuffer 采集 14/14 张截图；结果仍只观察到 Calm，天气变体 0/3。
- 构建/运行日志：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_FIXED_RERUN35.log`、`D19_R03_A02_FIXED_SHOT_RERUN35_PLAYER.log`。
- 机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN35_FIXED.json`；截图目录仍为 `game/tools/results/D19/D19-R03-A02/fixed_cameras_rerun17/`，对应 JSON 已更新为本次 RERUN35 采集结果。
- 复核结论：粉色材质/大面积纯白代理面未观察到，屋顶遮挡有所收敛；平坦地表/码头、低细节建筑、三角/代理岩体和开发路线 HUD 仍在。固定镜头不能替代真实玩家路线、保存重载或天气门禁，A02 仍 **PARTIAL**。

## RERUN21/RERUN23 时序修复增量（2026-08-16）

- 新增修复：`D19R03HarborRuntimePresentation` 等待 `ShoreBoundary` 后再应用路线碰撞窗口；日志确认 `ShoreBoundary z=17.0` 且 D6 collider owner retained。
- 构建：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_BUILD_RERUN21_COLLISION_WAIT.log` 与 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_BUILD_PLAYER_RERUN21_UNITY.log`，均成功。
- RERUN23 真实 Player 探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN23.json`；持续 `160.012863 s`，玩家/控制器/路由器找到，交互输入边沿为 true，但移动为 false、保存输入为 false。
- 因此修复缩小了时序问题范围，但没有闭合真实玩家路线、保存恢复、天气和最终视觉门禁；A02 继续 **PARTIAL**。
- 本轮没有修改 Systems、AgentCore、Registry、Save Schema、contracts/server/web 或资源状态；历史 RERUN10/RERUN20 结果保留。

## RERUN26 候选接入与 Player 复验增量（2026-08-16）

- 受控修改：`D19R03HarborPresentationBuilder.AddCandidateDetails` 继续保留生成式 fallback，并加入 8 个已在 intake catalog 中 `intake_eligible=true`、允许 D19-A01/D19-A03 使用的 NatureKit 候选：`tree_default.fbx`、`tree_small_dark.fbx`、`rock_largeF.fbx`、`cliff_large_rock.fbx`、`grass_large.fbx`、`ground_pathBend.fbx`、`cliff_stepsCorner_stone.fbx`、`rock_tallF.fbx`。没有改变资源状态、content ID、Registry 或正式集成所有权。
- Unity 审计：构建/审计日志为 `game/tools/results/D19/D19-R03-A02/D19_R03_A02_UNITY_AUDIT_RERUN26_CANDIDATES_REFRESH.log`，退出码 0；最新 `D19_R03_A02_EDITOR_AUDIT.json` 为 315 Renderer、314 enabled、6457 网格顶点、R03 自身 0 Collider。审计 JSON 的外部候选列表已与实际 8 个引用一致。
- Player：RERUN26 使用非调试 Windows Player `game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN26.exe` 进入新游戏/港口，真实截图为 `game/tools/results/D19/D19-R03-A02/RERUN26_PLAYER_MAINMENU_AFTER_CANDIDATES.jpg`、`RERUN26_PLAYER_HARBOR_AFTER_CANDIDATES.jpg`、`RERUN26_PLAYER_PORT_AFTER_CANDIDATES.jpg`。港口可见树、岩石、草和路径候选，未观察到粉色缺材质；当前截图仍存在块状建筑、平坦地表和底部调试 HUD。
- RERUN26 只用于候选视觉取证，没有完成 14 镜头、三天气、完整玩家路线或存档恢复；A02 继续 **PARTIAL**。未把 Candidate/ImportVerified 晋级为 LocalDevelopmentReady 或 ReleaseApproved。
