# D19-R02-A04 港口视觉与场景表现修复记录

## 结论

本任务结论为 **PARTIAL**。本轮建立并编译了 D19 R02 的正式场景 Integration 表现层，真实 Player 能看到新的起伏地形、沙/草/岩色带、道路、岸线、浅水/泡沫、码头与植被层；但旧 R01/灰盒临时构件仍在玩家可见范围内，部分植被会遮挡镜头，D2 的旧接地恢复逻辑会把玩家从 R02 地表恢复到 `BeachGround`。因此不能把这轮写成“港口视觉已通过”。

本任务没有修改 Systems、D2、D3、存档、Registry、资源状态、D20 或主 UI。没有把任何 Candidate、ImportVerified 或 `PrefabCandidateIntake` 条目标记为 `LocalDevelopmentReady`/`ReleaseApproved`。

## 实际修改

- 正式场景新增一个 `D19_R02_HarborPresentation` Prefab 实例；R01 兼容实例和旧交互节点保留。
- 新增 R02 项目自有材质：草地、浅草、沙地、浅沙、石材、浅石、木材、浅木、屋顶、灰泥、水体、泡沫、叶片。
- R02 Prefab 包含地形表面、道路、市场/街道轮廓、码头/船坞表现、岸线岩石、水体/泡沫和植被带。编辑器审计记录 80 个 Renderer、1 个 MeshCollider、1936 个地形网格顶点，包围盒约 `39.6 × 4.66 × 39.6` 米。
- 建立可回滚快照：`game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_150433.unity`。原始场景 SHA-256 为 `4C45779C7B5A020583E9F11F9F8F61642C12F1EC3920E8001F4752EF626D7E9D`，修改后为 `051830CED5E2DA9C85019991F76A361AF0DAF1015ECCCB7A4D6A2F3B4520015B`。

## 真实 Player 证据

使用 `D19_R02_A04_Player_V3.exe` 真实启动并通过真实键鼠输入观察场景，保留 14 张帧图于 `game/tools/results/D19/D19-R02-A04/screenshots/`。采集窗口报告为 `2194 × 1234`；命令目标为 `1920 × 1080`，但本轮没有独立证明 Player backbuffer 已精确锁定到目标值，故不能将其当作 1080p 验收证据。

人工检查结论：

- 粉色/丢失材质：在观察到的帧中未见，记为 `PASS_FOR_OBSERVED_FRAMES`。
- 新 R02 地形：确实渲染，具有高度差和色带层次，记为部分通过。
- 旧红色码头、方块化临时物体和宽阔的橙/绿低多边形表面：仍可见，记为失败。
- 大型棕榈叶/树干会遮挡摄像机，记为失败。
- 14 张帧不是已验证的 D19-S01 固定机位集；固定机位验证为 `0/14`，不能用截图数量掩盖这一点。

Player 日志还记录：

```text
[D2] 玩家已落地: surface=D19_R02_TerrainSurface, surfaceY=0.61, playerY=0.76
[D2] 玩家已恢复可行走接地: surface=BeachGround, surfaceY=0.28
```

这证明 R02 地表有真实碰撞并先完成落地，但旧接地恢复链随后选择了 `BeachGround`。该跨模块冲突属于后续路线/集成修复范围，本任务没有越权修改 D2。

## 自动化验证

`D19_R02_A04_EDITMODE_TESTS_RERUN.xml`：3/3 PASS，覆盖 R02 Prefab 的地形 MeshCollider/LOD、材质引用和 InitialHarbor 的 R02/稳定锚点 Prefab 引用。

`D19_R02_A04_PLAYER_BUILD_V3_RERUN.log`：Player 构建命令成功。没有把构建成功解释为路线或性能通过。

## 性能与天气

本任务的 `D19_R02_A04_PERFORMANCE_1080P.json` 明确为 `NOT_RUN`：没有获得经批准的 Player 内 CPU/GPU/GC/内存帧时间采样路径，也没有把编辑器或静态网格数据冒充 FPS。Low/Medium/High 的 30 秒 warm-up + 60 秒采样和天气变体均留给后续任务。

## RERUN6_AUTHORED3 追加记录（2026-08-15）

本轮继续保留 A04 原始 PARTIAL、RERUN4、RERUN5E 及所有回滚证据。只在 A04 Integration Builder 范围内加入项目自有 facade/building visual 层，并生成 `D19_R02_A04_Player_RERUN6_AUTHORED3.exe`；没有改工作场景源 Prefab、玩法 Systems、Registry、存档或资源状态。当前 Prefab SHA-256 为 `5051EBAC9703E215F52AFBF7ACCEE3CA6F51BD0EFC1A7B4B7DA1477DF181AF14`，当前场景文本卫生清理后的 SHA-256 为 `CE12DC16EBF33960C24F18208C821FCA4A357F147D8501007D719B40712865DC`。

最新 Editor Audit 记录 224 Renderers、45 Colliders、1,936 地形顶点，且 Builder/Player 构建退出码为 0。静态数量只能证明对象进入构建数据，不能证明玩家镜头可读性。

真实 Player 观察仍显示平坦棕/橙地表、简化水带、块状植被和不可读建筑轮廓；新增 facade 在当前镜头中没有形成可接受的港口层次。因此 A04 仍为 **PARTIAL**。Low/Medium/High 真实性能和 300 秒路线由 D19-R02-S02 的 RERUN8 结果单独记录，不能回写为 A04 PASS。RERUN8 的视觉/路线/性能结果见 `game/tools/results/D19/D19-R02-S02/` 对应 JSON；D20-S02-R01 仍受 D19 门禁阻止。

## 回滚与后续

需要撤回本轮场景表现时，使用独立快照恢复 `InitialHarbor.unity`；不要删除或重写旧 R01 结果，也不要改变稳定交互 ID。后续应先由 D19-R02-A05 建立真实性能/渲染采样，再由 D19-R02-S02 重新完成真实玩家路线、固定机位、天气、保存恢复和最终门禁。旧构件的替换必须保留兼容节点，并由场景集成写者处理。

## 机器结果

- `game/tools/results/D19/D19-R02-A04/D19_R02_A04_MACHINE_RESULT.json`
- `game/tools/results/D19/D19-R02-A04/D19_R02_A04_VISUAL_QA.json`
- `game/tools/results/D19/D19-R02-A04/D19_R02_A04_PERFORMANCE_1080P.json`
- `game/tools/results/D19/D19-R02-A04/D19_R02_A04_EDITOR_AUDIT.json`
- `game/tools/results/D19/D19-R02-A04/D19_R02_A04_EDITMODE_TESTS_RERUN.xml`

## RERUN4 追加证据（2026-08-15）

RERUN4 是针对旧表现子树的最小、可回滚修复，不覆盖原始 PARTIAL 结论。构建日志记录 `D19_R01_B_DockMarketProps_Approved` 与 `D19_R01_B_PortModules_Approved` 共 16 个旧表现 Renderer 被隐藏；稳定交互锚点和兼容节点保留。最新场景审计为 101 个 Renderer、1 个 Collider、1936 个地形顶点，详见 `D19_R02_A04_EDITOR_AUDIT.json` 与 `D19_R02_A04_SCENE_RENDERER_DUMP_RERUN4.txt`。

真实 Player `D19_R02_A04_Player_RERUN4.exe` 按 1920×1080 启动参数进入港口，日志确认角色落在 `D19_TerrainRoot`（`surfaceY=0.53`），没有观察到本轮 A02 修复前的键盘枚举异常。RERUN4 画面仍能看到低多边形码头组合、红色绳线/构件、低细节地表和未完成的固定镜头质量，因此视觉仍为 PARTIAL；不能把隐藏旧子树等同于“灰盒已消除”。截图位于 `screenshots/rerun4_spawn_after_subtree_hide_medium_request.jpg`。

性能探针在真实 Player 退出时写出两次可复核样本：RERUN3 495 帧，平均 59.83 FPS、最低 17.07 FPS、平均主线程 4.30 ms、GPU 0.77 ms、峰值保留内存约 202.2 MB；RERUN4 的 Medium 请求样本 1235 帧，平均 59.94 FPS、最低 19.42 FPS、平均主线程 4.89 ms、GPU 0.73 ms、峰值保留内存约 206.4 MB。由于运行时质量档位没有独立可靠证据，不能把 RERUN4 计为 Medium 通过，也没有 Low/High 或天气变体完整数据。

本轮自动验证：RERUN4 场景 EditMode 3/3 PASS，Builder 和 Editor Audit 完成；完整玩家路线、14 个固定机位、天气、保存恢复以及 Low/Medium/High 质量门禁仍交由 D19-R02-S02。机器摘要见 `D19_R02_A04_RERUN4_MACHINE_RESULT.json`。最终结论保持 **PARTIAL**。

## RERUN5E 追加证据（2026-08-15）

本轮保留 RERUN4 及更早结果，使用独立回滚快照 `game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_2015_pre_rerun5_intake_detail.unity`。场景由 `D19R02HarborPresentationBuilder` 重新生成，最新 SHA-256 为 `B22A005F58EC19C8B3A95BB8D6BAE935C04B9A0FF849E0703E24293C815981EC`，R02 Prefab SHA-256 为 `D0A21A05F4800A0D83971637C40FDDF971BBB5088D8FB13D4520F06046D6BCE3`。

针对真实 Player 观察到的准入模型嵌入材质不协调问题，Integration 层现在只实例化四个已经在 `d18_prefab_candidate_intake_catalog.json` 中 `intake_eligible=true` 且有本地 ImportVerified 证据的 Kenney Nature Kit 模型：`env_kenney_nature_tree_default`、`env_kenney_nature_tree_detailed_dark`、`env_kenney_nature_cliff_large_rock`、`env_kenney_nature_rock_large_f`。材质槽由 A04 Builder 映射到项目自有 D19_R02 材质；`env_kenney_nature_bridge_wood` 仅保留在只读模型审计，不再进入当前镜头。没有改变 intake catalog、Candidate/ImportVerified、LocalDevelopmentReady 或 ReleaseApproved 状态。

旧 R01 视觉兼容子树 `D19_R01_B_HarborVisualRepair` 的 324 个本轮仍启用的 Renderer 被关闭；场景 dump 中该兼容树的 351 个 Renderer 均为 `enabled=false`，稳定锚点、旧 key 和碰撞/交互节点仍保留。R02 Prefab 编辑器审计为 105 Renderer、1 Collider、1936 个地形顶点、包围盒约 `39.6 × 4.884 × 39.6 m`。这只移除误导性旧视觉，不改变玩法事实。

真实 Windows x64 Player `D19_R02_A04_Player_RERUN5E.exe` 从主菜单通过键鼠新建 `RerunA04E`，进入港口并观察到角色落地；再次按 E 观察到“已上船”与船控状态变化。三张新帧为 `D19_R02_A04_MENU_RERUN5D.jpg`、`D19_R02_A04_SPAWN_RERUN5E.jpg`、`D19_R02_A04_BOARD_RERUN5E.jpg`，输入日志为 `D19_R02_A04_PLAYER_RERUN5E_INPUT.log`。画面中未见粉色材质或旧红色绳线，但地表仍宽阔、低细节且有简单块状轮廓，因此视觉结论仍为 **PARTIAL**，不能用三帧替代 14 个固定机位。

当前 A04 专属 EditMode 为 3/3 PASS；x64 Player 构建 PASS。Low/Medium/High、14 固定镜头、天气变体、5—10 分钟路线和保存重载没有在 A04 重新完成，仍由 D19-R02-S02 负责。误用 x86 构建入口导致的 `sqlite3.dll` 缺失日志 `D19_R02_A04_PLAYER_BUILD_RERUN5B.log` 保留；后续真实 Player 必须使用 `-buildWindows64Player`。机器结果见 `game/tools/results/D19/D19-R02-A04/D19_R02_A04_RERUN5E_MACHINE_RESULT.json`。
