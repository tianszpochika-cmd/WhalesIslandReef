# D19-S02 圣玛格丽特正式场景集成与验收

状态：`PARTIAL`

本文件记录 D19-S02 对正式 `InitialHarbor` 的串行 Prefab 集成和可复核验收证据。`PARTIAL` 只表示本次自动化执行没有替代人工 5—10 分钟路线和目标设备性能采样；已通过的场景、材质、稳定锚点和 PlayMode 证据保持有效。

## 前置和边界

- D19-S01 Attempt 2：`PASS`，冻结原点、海平面 Y=0、+Z 内陆、-Z 海面和 14 个固定镜头契约。
- D18-S02-v2：`PASS`，集成只读取准入目录；没有改写外部资源状态。
- D18-S03：按任务要求不是前置，本次没有执行，也没有把任何候选标记为 `LocalDevelopmentReady` 或 `ReleaseApproved`。
- 本任务没有修改 Systems 强逻辑、经济公式、contracts、server/web、Registry 或存档 Schema。

## 备份与回滚

集成前先保存：

- 回滚点：`d19-s02-preintegration-baseline`；
- 原场景 SHA-256：`D3D76848BDA2E00F3313F111D7E0DBD2B59E8D306C1A681765B67FD5A591932B`；
- 快照：`game/tools/results/D19/D19-S02/PreintegrationBaseline/InitialHarbor.unity` 及同目录 `.meta`；
- 原始规模：24 GameObjects、25 PrefabInstances、7 个场景 MonoBehaviour、NavMeshData 为 `fileID: 0`。

回滚时只需禁用或移除正式场景中的 `D19_SaintMargaret_Root` Prefab 实例；若需恢复原场景，则从上述快照恢复场景和 `.meta`。A01—A06 工作成果、测试、日志和截图不删除。

## 串行集成结果

集成顺序严格为 Terrain → Port → Biomes → Water/Sea → Lighting/Weather → Ambience。

| 独立根节点 | 来源 | 结果 |
|---|---|---|
| `D19_EnvironmentRoot` | D19-A01 TerrainRoot、Collision、Anchors、SurfaceSet、CaveFissure、NavMesh | 正式 Prefab 实例 |
| `D19_PortBuildingsRoot` | D19-A02 港口入口、市场、仓库、酒馆、船坞、码头和行走面 | 正式 Prefab 实例 |
| `D19_BiomeRoot` | D19-A03 六类生境 BiomeRoot | 正式嵌套 Prefab |
| `D19_ShipsSeaRoot` | D19-A04 WaterRoot、海岸、暗礁和水下表现 | 正式嵌套 Prefab |
| `D19_PresentationRoot` | D19-A05 LightingRoot、五类天气配置、禁用的截图相机 | 正式嵌套 Prefab |
| `D19_AmbienceRoot` | D19-A06 道具/VFX 代理、5 个区域触发器和事件图音景入口 | 正式嵌套 Prefab |
| `D19_CharactersAnimalsRoot` | D19 后续运行时消费者预留 | 空根，不抢写运行时实体 |
| `ExistingGameplayAnchors` | 20 个稳定交互锚点 | 稳定 ID 兼容层 |

统计：594 个嵌套 Prefab/Prefab 变换实例、436 个 Renderer、145 个 Collider、66 个 LODGroup、20 个稳定锚点。集成 Prefab 只引用 Prefab 和集成层材质；A02/A06 的旧 Standard Shader 在集成层转为 URP 材质，没有改写其源 Prefab。

## 稳定交互 ID

以下 ID 由 `D19_StableInteractionAnchors.prefab` 保存，玩法系统仍是各自事实写者：

`D2_PlayerAvatar`、`santa_margarita`、`building_market_santa_margarita`、`market_queue_santa_margarita`、`building_warehouse_santa_margarita`、`warehouse_queue_santa_margarita`、`building_tavern_santa_margarita`、`building_shipyard_santa_margarita`、`shipyard_repair_santa_margarita`、`building_saint_marys_dock`、`DockGround`、`berth_santa_margarita`、`GangplankGround`、`D7_HelmPoint`、`departure_channel_santa_margarita`、`return_channel_santa_margarita`、`save_point_santa_margarita`、`npc:santa_margarita_dockhand`、`npc:santa_margarita_lookout`、`npc:santa_margarita_tavern_keeper`。

旧兼容节点 `Dock`、`DockDeck`、`Gangplank`、`Beach`、`Sea`、`Main Camera`、`Harbor UI`、`HarborControls` 仍存在。未删除仍可能被 D2/D7/D9/D10 测试引用的旧对象；旧表现只保留为兼容回退，正式 D19 根负责新表现。最终构建结果记录旧节点在重跑时已处于隐藏状态，没有新增删除动作。

## 验收证据

- 材质/缺失引用审计：436/436 Renderer 有材质和 Shader；0 missing material、0 missing shader、0 `Hidden/InternalErrorShader`、0 legacy `Standard`。
- 固定镜头：16 张 1920×1080 PNG，覆盖 14 个 D19 镜头契约，天气另采集 `clear_day`、`overcast_rain`、`storm` 三种 profile。图像像素审计未发现粉色缺材质块。
- D19 EditMode：`D19_S02_EDITMODE_TESTS_FINAL.xml` 为 4/4；D19 全部相关 EditMode：`D19_ALL_EDITMODE_TESTS_FINAL.xml` 为 25/25。
- D19 PlayMode：`D19_S02_PLAYMODE_TESTS_FINAL3.xml` 为 1/1；6/6 陆上锚点有碰撞，6/6 可采样 NavMesh，水面 Y=0，开发叠字默认关闭。
- D16/D17 完整 PlayMode：`D16_D17_PLAYMODE_REGRESSION_FINAL.xml` 为 61 passed、0 failed、1 skipped。唯一跳过是 D19-A01 仅针对工作场景探针的测试，正式 InitialHarbor 不加载该工作场景探针，属于测试设计中的预期 Ignore。
- Unity 图形批处理和编译：`UNITY_D19_S02_BUILD_GRAPHICS_FINAL.log` 退出码 0；Console 中没有 D19 Shader 编译错误。D19 自有 Shader 增加了 `_MainTex` 兼容槽位，避免 D7 只读润色路径报错。

## 1080p 性能与路线边界

`D19_S02_PERFORMANCE_1080P.json` 记录的是编辑器 `Camera.Render` 采样：平均 20.182 ms、P95 295.704 ms。它不是目标机器或 PlayMode 帧率预算，因此目标设备 CPU/GPU/GC/峰值内存门仍为未完成项。

本次运行执行了正式场景 PlayMode 锚点/碰撞/NavMesh 烟测和 D16/D17 全量回归，但没有声称完成真人控制的 5—10 分钟出生→市场→仓库→酒馆→码头→登船→出航→返港→保存路线。人工路线应在下一次集成验收中补采，并记录每个交互 ID 的响应、卡死、穿模和保存恢复结果。

## 已知边界和责任

1. A02 建筑模块仍是项目自有低多边形表现代理，虽然不再是白色平面/粉色缺材质，也未达到最终美术资产精度；后续由建筑资产批次继续替换。
2. A06 的合法环境音候选仍通过事件图入口，不在场景里直接放 AudioSource；最终运行时音景由对应系统和 D18-S03 消费者闭环。
3. D18-S03 尚未执行；本交接只提供可定位的 Prefab、材质、测试、截图和回滚消费者证据。
4. 因人工路线和目标设备性能未完成，D19-S02 最终结论为 `PARTIAL`，不能把本结果解释为 D19 全部完成。
