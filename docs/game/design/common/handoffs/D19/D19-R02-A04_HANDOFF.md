# D19-R02-A04 交接

- 任务：`D19-R02-A04`
- 结论：**PARTIAL**
- 日期：2026-08-15

## 已交付

- `game/Assets/Scenes/InitialHarbor.unity` 新增 R02 Integration Prefab 实例。
- `game/Assets/Prefabs/Environment/D19/Integration/R02/D19_R02_HarborPresentation.prefab`。
- `game/Assets/Art/Materials/D19/R02/` 下 13 个项目自有表现材质。
- `game/Assets/Editor/D19R02/D19R02HarborPresentationBuilder.cs`。
- `game/Assets/Tests/EditMode/D19R02Scene/D19R02A04SceneTests.cs`。
- 结果、截图、回滚快照和本交接文件。

## 可复核证据

- 场景修改前 SHA-256：`4C45779C7B5A020583E9F11F9F8F61642C12F1EC3920E8001F4752EF626D7E9D`。
- 场景修改后 SHA-256：`051830CED5E2DA9C85019991F76A361AF0DAF1015ECCCB7A4D6A2F3B4520015B`。
- 回滚快照：`game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_150433.unity`。
- R02 Prefab SHA-256：`B0FB811CF957DF895DE5FD68FFEAAFA2460E1D277F77A8BCD27DE19811A49E8E`。
- 编辑器审计：80 Renderers、1 Collider、1936 地形顶点；见 `D19_R02_A04_EDITOR_AUDIT.json`。
- EditMode：3/3 PASS；XML：`D19_R02_A04_EDITMODE_TESTS_RERUN.xml`。
- Player 构建：`D19_R02_A04_PLAYER_BUILD_V3_RERUN.log`。
- Player 实帧：14 张，位于 `screenshots/`；报告分辨率 2194×1234，精确 1920×1080 backbuffer 未证明。

## 未通过与责任边界

1. 旧红色码头、方块化临时构件和宽阔低细节地表仍在玩家视野中：D19 后续场景视觉集成责任，不能用本轮 14 张帧标记通过。
2. 棕榈叶/树干遮挡镜头：D19 Integration 表现层后续修复。
3. R02 地表落地后被 D2 恢复链切回 `BeachGround`：记录为 D2/场景接地接口冲突，本任务未修改 D2。
4. 真实路线、14 个固定机位、天气变体和保存恢复：留给 D19-R02-S02。
5. Low/Medium/High 1080p 性能：本轮 `NOT_RUN`，由 D19-R02-A05 建立真实采样证据。

## 状态边界

本任务没有新增外部下载，没有修改 intake catalog、asset_state、LocalDevelopmentReady、ReleaseApproved、Systems、Registry、Save Schema 或正式 D20 入口。`D19-R02-A04` PARTIAL，不放行最终 D19 门禁，也不放行 D20-S02-R01。

## RERUN4 追加交接（2026-08-15）

- 保留上一版场景、机器结果、14 帧和回滚快照；本轮新增回滚快照 `game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_180000_pre_rerun2.unity`，并在 Integration 层隐藏两个旧表现子树。未修改 D19-A01—A06 工作目录、Systems、Registry、存档、资源状态或 D20 正式入口。
- 最新 Builder/Audit：旧子树隐藏计数 16；`D19_R02_A04_EDITOR_AUDIT.json` 为 101 Renderer、1 Collider、1936 地形顶点；RERUN4 EditMode 3/3 PASS。
- 真实 Player 重新启动并经真实键鼠输入进入港口；日志记录 `surface=D19_TerrainRoot`、`surfaceY=0.53`、`playerY=0.68`。截图 `screenshots/rerun4_spawn_after_subtree_hide_medium_request.jpg` 仍显示低多边形/临时感码头组合、红色线性构件和低细节地表，故没有宣称视觉通过。
- 真实性能探针：RERUN3 495 帧/平均 59.83 FPS/最低 17.07 FPS；RERUN4 Medium 请求 1235 帧/平均 59.94 FPS/最低 19.42 FPS。两次均为 1920×1080 启动参数，但运行时质量档位和精确 backbuffer 未独立证明；Low/Medium/High、天气和完整路线仍 NOT_RUN。
- 新机器结果：`game/tools/results/D19/D19-R02-A04/D19_R02_A04_RERUN4_MACHINE_RESULT.json`；结论仍 **PARTIAL**。下一硬门禁为 D19-R02-S02，需重新完成真实菜单路线、14+ Player 镜头、天气、保存恢复和三档性能。
- 记录性校验：RERUN4 完成场景文本空值尾随空格清理后，当前 `InitialHarbor.unity` SHA-256 为 `53861A681D9C59C4635CC3D037868C37F1D06CB54A9F22B6DE88F12B2026DCEB`；`git diff --check` 退出码 0。

## RERUN5E 追加交接（2026-08-15）

- 本轮继续保持 **PARTIAL**，不覆盖或删除原始 A04/RERUN4 结果。新场景 SHA-256 为 `B22A005F58EC19C8B3A95BB8D6BAE935C04B9A0FF849E0703E24293C815981EC`；R02 Prefab SHA-256 为 `D0A21A05F4800A0D83971637C40FDDF971BBB5088D8FB13D4520F06046D6BCE3`；回滚快照为 `game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_2015_pre_rerun5_intake_detail.unity`。
- 只修改 A04 Integration Builder：加入四个准入目录中已有真实证据的环境模型作为细节层，并把源材质槽映射至 D19 项目材质；桥模型仅审计不使用。外部资源 ID、manifest、路径和 ImportVerified 证据均记录在 `game/tools/results/D19/D19-R02-A04/D19_R02_A04_INTAKE_MODEL_AUDIT_RERUN5.json` 与机器结果中；没有资源状态晋级。
- 旧 `D19_R01_B_HarborVisualRepair` 视觉子树本轮关闭 324 个仍启用 Renderer；场景 dump 显示其 351 个 Renderer 最终均为 `enabled=false`。兼容节点、稳定交互 ID、碰撞和存档 key 保留；旧 R01 Prefab 源文件没有改写。
- 真实 x64 Player 构建 `D19_R02_A04_Player_RERUN5E.exe` 从主菜单完成新游戏、港口出生落地和 E 上船观察；三张证据为 `D19_R02_A04_MENU_RERUN5D.jpg`、`D19_R02_A04_SPAWN_RERUN5E.jpg`、`D19_R02_A04_BOARD_RERUN5E.jpg`，输入日志为 `D19_R02_A04_PLAYER_RERUN5E_INPUT.log`。旧红色绳线在本轮观察中不再出现，粉色材质未观察到；地表仍低细节/块状，不能升为视觉通过。
- A04 EditMode `D19_R02_A04_EDITMODE_TESTS_RERUN5E.xml` 为 3/3 PASS，Builder/Audit 完成。Low/Medium/High、14 固定镜头、天气、完整路线和保存重载仍未在本任务闭合；D19-R02-S02 仍是最终门禁。机器结果：`game/tools/results/D19/D19-R02-A04/D19_R02_A04_RERUN5E_MACHINE_RESULT.json`。
- 构建注意：x86 入口失败日志 `D19_R02_A04_PLAYER_BUILD_RERUN5B.log` 保留，原因是输出缺少 `sqlite3.dll`；后续 Player 证据必须使用 x64 构建入口。`git diff --check` 在本轮清理两处场景修改块尾随空格后通过。

## RERUN6_AUTHORED3 追加交接（2026-08-15）

- 在 A04 Integration Builder 唯一写者范围内加入项目自有 facade/building visual 层并重新生成 x64 Player；最新 Builder hash 为 `BA16FC3762F08C330CB66926508DC3145CEA8A9D7681EFEC67A83F608BF388A7`，Prefab hash 为 `5051EBAC9703E215F52AFBF7ACCEE3CA6F51BD0EFC1A7B4B7DA1477DF181AF14`。
- 最新 Editor Audit：224 Renderers、45 Colliders、1,936 地形顶点；A04 Build/Player Build 日志退出码 0。该静态证据不等于玩家视角通过。
- 最新 x64 Player 帧 `D19_R02_A04_FACADE_RERUN6_AUTHORED3_SPAWN.jpg`、`D19_R02_A04_FACADE_RERUN6_AUTHORED3_DISEMBARK.jpg` 仍显示平坦棕/橙地表、简化植被和不可读建筑轮廓；A04 结论继续 **PARTIAL**，未标记任何资源为 LocalDevelopmentReady/ReleaseApproved。
- D19-R02-S02 RERUN8 已独立保存 300 秒路线、Continue、Low/Medium/High 探针和静态回归证据；A04 不把这些证据越权写成自身 PASS。D19 门禁仍未通过，D20-S02-R01 不启动。
