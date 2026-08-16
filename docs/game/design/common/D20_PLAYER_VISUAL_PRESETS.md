# D20 玩家视觉预设与接口

任务：D20-A01-R01  
日期：2026-08-15  
结论：**PARTIAL**

## 1. 本轮边界

本轮只建立可供 D20-A02-R01 读取的玩家视觉工作 Prefab、派生 Humanoid Avatar、锚点/LOD 契约和真实 Unity 编辑器证据。没有修改正式玩家生成器、D17 物理根、CharacterController、Animator、InitialHarbor、Systems、Registry、存档、contracts 或资源状态。`intake_eligible` 只表示受控工作范围；`LocalDevelopmentReady=0`、`ReleaseApproved=0` 保持不变，正式运行时消费者仍由 D20-S02-R01 与 D18-S03 负责。

历史 D20-A01 的 PARTIAL 结果已保存在 `game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE_HISTORICAL_PARTIAL.json` 和既有 `docs/game/design/common/handoffs/D20/D20-A01_HANDOFF.md`；本文件记录 R01 的新证据，不覆盖历史结论。

## 2. 真实来源和准入边界

| 项目 | 值 |
|---|---|
| manifest | `pack_quaternius_universal_base_characters_standard_v01` |
| intake | `intake_a99050649c3cc24cb8a7` |
| content / ResourceKey | `candidate_superhero_female_fullbody_r03p` |
| 源文件 | `game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/Models/Base/Superhero_Female_FullBody.fbx` |
| 源 SHA-256 | 由 D18-R02 准入记录和本轮源导入审计引用，未修改候选文件 |
| 许可 | Quaternius 官方 CC0 证据；不因本轮工作 Prefab 而升级发布许可 |
| 工作 Prefab | `game/Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab` |
| 派生 Avatar | `game/Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset` |

主工程对源 FBX 的当前导入器事实为 `Generic/NoAvatar`、Avatar 列表为空，证据见 `game/tools/results/D20/D20-A01-R01/D20_A01_R01_SOURCE_IMPORT_AUDIT.json`。本轮没有修改源 FBX 的 `.meta` 或把源文件伪写为 Humanoid；Unity 编辑器使用同一准入骨架通过 `AvatarBuilder` 生成了项目自有的候选 Avatar，证据见 `D20_A01_R01_EDITOR_AUDIT.json`。这是一项可追溯的工作层派生，不是源资源状态晋级。

## 3. 工作 Prefab 技术契约

| 项目 | 真实结果 |
|---|---:|
| 视觉高度 | `1.79999995 m`，目标 `1.80 m ± 0.03 m` |
| 根缩放 | `[1,1,1]` |
| Renderer | 3 |
| 材质数 | 1 个已绑定的项目自有中性皮肤材质 |
| Humanoid Avatar | 存在、`isValid=true`、`isHuman=true` |
| Animator / CharacterController / MonoBehaviour | 0 / 0 / 0 |
| LODGroup | 1；工作层近/中/远裁剪配置已审计 |
| 纯 Transform sockets | 10：Head、Hand_L、Hand_R、Foot_L、Foot_R、Carry、Fish、Boarding、Disembark、Helm |
| 独立身体方向 | 1/2；不把换色计作第二身体 |
| 独立发型几何 | 0/6；发色材质不计作发型 |
| 1675 时代服装组合 | 0/8；没有用同一身体或换色凑数 |

Prefab 仅承载 Visual 层。D17/D2 继续拥有 `D2_PlayerAvatar`、物理根、CharacterController、CameraTarget 和 gameplay 锚点；D20-A02 只可读取本轮接口，不能向本任务目录回写逻辑状态。

## 4. 资源键和外观回退

当前可消费的工作层键为：

- `candidate_superhero_female_fullbody_r03p`：唯一真实 Humanoid 玩家基体；
- `asset_d20_player_humanoid_avatar_female_r01`：由准入骨架派生的候选 Avatar；
- `asset_d20_player_skin_neutral_v01`：已绑定中性肤色材质；
- `asset_d20_player_skin_warm_v01`、`asset_d20_player_hair_brown_v01`、`asset_d20_player_hair_black_v01`：候选材质，因没有对应独立几何，不得冒充第二身体或发型。

D10 仍是 Body/Hair/Hat/Top/Bottom/Coat/Shoes/Gloves/Accessory/CareerEquipment/Injury 外观槽的事实写者。缺失资源必须保留具体 key，经过 MissingResourcePlaceholder 后使用 D17 稳定程序回退；外观请求不得重置 D2 gameplay 或存档事实。

## 5. 验证证据

- Unity 6000.0.80f1 编辑器构建与审计：`game/tools/results/D20/D20-A01-R01/D20_A01_R01_BUILD_V3.log`、`D20_A01_R01_EDITOR_AUDIT.json`。
- 源导入事实：`D20_A01_R01_SOURCE_IMPORT_AUDIT.json`。
- 预览：`game/tools/results/D20/D20-A01-R01/previews/`，front/back/three-quarter T-pose 共 3 张，512×768；预览是工作层 Camera.Render 取证，不是正式 Player 验收。
- 聚焦 EditMode：`D20_A01_R01_EDITMODE_TESTS_FINAL2.xml`，6 tests，6 passed，0 failed，0 skipped；日志为同名 `.log`。
- 接口：`game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json`，`rig_interface_ready=true`，但 `formal_scene_consumers=0`、`local_development_ready=false`、`release_approved=false`。
- JSON 结构校验与 `git diff --check` 在本轮交付前执行；行尾转换提示不计为 whitespace error。

## 6. 结论和下一步

`rig_interface_ready=true` 只表示 D20-A02-R01 可以在隔离工作层建立动作接口；它不表示玩家已正式接入，也不表示资源已 LocalDevelopmentReady。A01 仍为 PARTIAL，原因是第二独立身体、6 个独立发型、8 套时代服装和正式运行时消费者尚未证明。D20-A02-R01 可继续，但必须保持 Root Motion=false、只写表现层，并以 D17 fallback 作为动作缺失回退。D19-R02-S02 仍是 D20-S02-R01 的硬门禁。
