# D20-A05 野生动物与水生生物表现交付

任务编号：`D20-A05`  
日期：`2026-08-15`  
结论：**PARTIAL**

## 1. 交付边界

本任务只交付动物/水生生物的表现层 Prefab、Animator、LOD、远距替代和只读适配器。`AnimalSystem` 与 `EcologySystem` 继续拥有物种、种群、位置事实、行为裁决、捕捞/捕获、稀有度、压力和事件；本任务没有修改这些系统，也没有修改正式场景、Registry、存档或水体。

外部模型全部来自当前 D18 `intake_eligible` 清单中的 CC0 条目。Prefab 保留源模型 GUID，不把源 FBX 复制到第二套动物目录，也没有把资源状态提升到 `LocalDevelopmentReady` 或 `ReleaseApproved`。

机器接口以 [D20_A05_WILDLIFE_INTERFACE.json](../../../game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json) 为唯一消费入口；D20-A06 只能读取该文件，不能回写 D20-A05 目录。

## 2. 实际样板覆盖

实际完成 8 个 Prefab、8 个唯一 `species_id`，覆盖 8 个表现类别；其中海岸/海鸟只有 1 个真实准入样本，海龟没有可用模型，故不把等价物冒充海龟。

| 类别 | species_id | Prefab | 来源/许可 | 代表动作 | 放置 |
|---|---|---|---|---|---|
| 海鸟 | `animal_quaternius_eagle_v01` | `D20_Wildlife_eagle_coastal_bird` | Quaternius Animal Pack Vol.2 Eagle，CC0-1.0，归档 SHA-256 `6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248` | Idle、Flying；Alert/Flee/Perch/Takeoff/Land/Feed 走明确 fallback | 空中/海岸，根偏移 2.0m，目标长度 0.85m |
| 陆生小动物 | `animal_quaternius_fox_v01` | `D20_Wildlife_fox_small_land` | Quaternius Ultimate Animated Animals，CC0-1.0，归档 SHA-256 `A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827` | Idle、Eating、Walk、HitReact、Gallop | 地面，目标长度 0.75m |
| 家畜/野生代表 | `animal_quaternius_cow_v01` | `D20_Wildlife_cow_livestock` | 同上 | Idle、Eating、Walk、HitReact、Gallop | 地面，目标长度 1.8m |
| 浅水鱼群 | `fish_quaternius_fish_01_v01` | `D20_Wildlife_fish_shallow_school` | Quaternius Animated Fish，CC0-1.0，归档 SHA-256 `D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9` | Swim；Turn/Dive/Surface/Flee 走 Swim fallback | 浅水层，参考深度 0.8m，目标长度 0.55m |
| 近海鱼 | `fish_quaternius_fish_02_v01` | `D20_Wildlife_fish_nearshore` | 同上 | Swim；Turn/Dive/Surface/Flee 走 Swim fallback | 近岸水层，参考深度 1.5m，目标长度 0.7m |
| 近海掠食者 | `animal_quaternius_shark_v01` | `D20_Wildlife_shark_nearshore_predator` | 同上 | Swim；Turn/Dive/Surface/Flee 走 Swim fallback | 近岸水层，参考深度 4m，目标长度 3m |
| 大型水生等价样本 | `animal_quaternius_manta_ray_v01` | `D20_Wildlife_manta_large_aquatic_equivalent` | 同上 | Swim；其余水生状态走 Swim fallback | 近岸水层，参考深度 3m，目标长度 2.5m；**不是海龟** |
| 远景大型水生 | `animal_quaternius_whale_v01` | `D20_Wildlife_whale_distant_aquatic` | 同上 | Swim；其余水生状态走 Swim fallback | 远景海水层，参考深度 10m，目标长度 12m |

来源文件与 Unity 导入证据均登记在接口 JSON；对应原始证据为：

- Eagle：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/Models/eagle.fbx`，`game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_UNITY_IMPORT_AUDIT.json`。
- Fox/Cow：`game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedAnimals/Models/`，`game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json`。
- Fish/Fish2/Shark/Manta ray/Whale：`game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/`，同一 D18-A04 Unity 导入证据。

## 3. Prefab、动画和表现适配

每个样板具有以下结构：`Visual/LOD0`、`Visual/LOD1`、`Visual/LOD2`、`Rig/Generic`、`Sockets/Observation`，以及 `CollisionProxy` 触发器；水生样本另有 `WaterCenter`，陆生样本有 `Ground`/接地参考。根节点 scale 保持 `(1,1,1)`，轴向为 `Y-up/Z-forward`，Animator Root Motion 关闭。碰撞代理只用于表现/观察触发，不写入生态事实。

只读适配器位于 `game/Assets/Scripts/Presentation/D20/Wildlife/D20WildlifePresentationAdapter.cs`，读取 `species_id`、行为状态、位置、速度、水深和生态可见性快照，仅写入 Transform、Renderer、Animator 和 LOD 可见性。它不写种群、刷新、捕获、稀有度、压力、法律、奖励、库存、任务、存档或宠物所有权，也不包含网络/LLM。

状态映射保持 D20-S01 的规范：

- 陆生：`Idle`→Idle，`Forage`→Eating/Feed，`Travel`→Walk，`Alert`→HitReact/Idle，`Flee`→Gallop，`Rest/Injured/Observed`→Idle。
- 水生：`Swim`→Swim；Turn、Dive、Surface、Flee 在源片段只有 Swim 时使用 Swim fallback，不制造伪动作。
- 海鸟：`Idle`→Idle，`Travel`→Flying；Alert、Flee、Perch、Takeoff、Land、Feed 使用 Flying 或 Idle fallback。

缺失动作不生成 T-pose，不通过 AnimationEvent 改变游戏事实；只使用稳定的 neutral 动画或远距替代。

## 4. LOD、群体显示和性能预算

统一距离带为 `0—12m / 12—32m / 32—80m / 80m+`。LOD0/LOD1 保留源 SkinnedMeshRenderer 以便工作场景检查；LOD2 使用无阴影的抽象轮廓 quad；80m+ 继续保留生态快照，只关闭 Animator 或显示远距替代。近/中距离 Animator 使用 Unity 6 对应的 `CullUpdateTransforms`，远距/代理使用 `CullCompletely`。这避免所有鱼/动物持续运行完整 Animator。

1080p 同屏预算按 D20-S01 固定为：Low `12`、Medium `24`、High `32` 个动物表现实体。静态 Prefab 结构和预算检查通过，但没有在 Player/目标设备上声称 FPS、GC、GPU 或同屏截图通过；实际运行时测量留给 D20-S02。

## 5. 真实缺口

| 缺口 | 结果 | 说明 |
|---|---|---|
| 岸鸟/小型鸟第二样本 | OPEN | 当前只准入 Eagle；原 Blender-only bird 不是 Unity 模型，不能计数。 |
| 海龟 | OPEN | D11/D15 有海龟内容 ID，但当前没有 `intake_eligible` 海龟模型；Manta 明确标作等价样本。 |
| 鸟类近镜动作 | FALLBACK | Eagle 只有 Idle/Flying，栖息、起飞、着陆、进食不宣称专用片段。 |
| 水生 Turn/Dive/Surface/Flee | FALLBACK | Fish、鲨、Manta、Whale 源资源只确认 Swim。 |
| 运行时截图/性能 | OPEN | 未在正式场景或 Player 中生成截图，也未使用静态结果冒充目标设备帧率。D20-A05 不能修改正式场景，故记录为后续门禁。 |
| `LocalDevelopmentReady` | WAITING | 资源晋级仍由 D18-S03 负责。 |

## 6. 验证结果

- Unity 6000.0.80f1 批处理构建和导入：PASS；日志 `game/tools/results/D20/D20-A05/D20_A05_PREFAB_BUILD.log`。
- Prefab/源 GUID/结构/Animator/LOD/碰撞代理/缺材质检查：8/8 PASS；`game/tools/results/D20/D20-A05/D20_A05_PREFAB_AUDIT.json`。
- EditMode：6/6 PASS；`game/tools/results/D20/D20-A05/D20_A05_EDITMODE_TESTS.xml`。
- 性能：静态 Low/Medium/High 预算和 culling 结构 PASS；真实 Player/设备帧率未测，`D20_A05_PERFORMANCE_SAMPLE.json` 标记 `STATIC_BUDGET_VERIFIED_NEEDS_PLAYER_CAPTURE`。
- 接口：`D20_A05_WILDLIFE_INTERFACE.json` 可被 JSON 解析，`wildlife_interface_ready=true`，但整体任务状态为 PARTIAL。
- 截图：本任务没有生成正式场景截图；不以占位图或源包预览冒充 Prefab/Player 视觉证据。
- 资源状态：没有修改 D18 intake catalog，没有标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。

后续消费顺序：先由 D18-S03 完成真实资源消费者与晋级门禁，再由 D20-S02 在允许的工作/集成场景中执行 8 类样本、层级、动作、LOD、截图和目标设备性能验收；D20-A06 只读取本任务的接口，不从视觉表现推导宠物所有权或宠物行为裁决。
