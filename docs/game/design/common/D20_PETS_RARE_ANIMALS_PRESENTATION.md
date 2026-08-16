# D20-A06 宠物与珍稀动物表现

状态：PASS（仅限 D20-A06 隔离表现资源；未晋级 LocalDevelopmentReady）  
任务：D20-A06  2026-08-15

## 范围与前置证据

本任务只写入 `game/Assets/Prefabs/Animals/D20/PetsRare/`、`game/Assets/Animations/D20/PetsRare/`、`game/Assets/Scripts/Presentation/D20/PetRarePresentation/`、专属 EditMode 测试和 D20-A06 结果目录。没有修改正式场景、InitialHarbor、Pet/Ecology/Reward/Relationship/Save、Registry、contracts、UI 主框架或 A05 接口。

只读前置：

- D20-A05 接口：`game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json`，`wildlife_interface_ready=true`；SHA-256 `CF94738984A03983CA57B759727F59D42BE78E5FC664A1B0A8600E6C7B55E1CE`。
- D18-S02-R01 intake catalog：`docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`，cat 1 条、dog 2 条均 `intake_eligible=true`；未读取或提升 `d18_local_development_ready_catalog.json` 的 BLOCKED 状态。
- A05 只读边界：表现层只消费 `species_id/resource_key/behavior_state/position/ecology_result/lod_band`；本任务没有假设或创建 ownership、adoption、affection、feeding、health、death、reward、inventory 或 save writer 字段。

## 已交付 Prefab

| Prefab | 真实来源模型 / content_id | 类型 | 高度目标 | 来源动画 | 表现边界 |
|---|---|---|---:|---|---|
| `D20_PetsRare_cat_core.prefab` | `GapFill/Cat/Source/Cat.fbx` / `animal_quaternius_cat_pack_vol2_v01` | pet cat | 0.28m | `CatArmature\|Idle`, `CatArmature\|Walking` | 可领养宠物表现，不写领养或好感 |
| `D20_PetsRare_dog_shiba.prefab` | `QuaterniusAnimatedAnimals/Models/ShibaInu.fbx` / `animal_quaternius_shiba_inu_v01` | pet dog | 0.55m | 12 条，含 Idle、Walk、Gallop、Eating、HitReact | 可领养宠物表现，不写领养或健康 |
| `D20_PetsRare_eagle_observation.prefab` | `GapFill/Bird/Models/eagle.fbx` / `animal_quaternius_eagle_v01` | rare observation | 0.85m | `EagleArmature\|Flying`, `EagleArmature\|Idle`, `Eagle\|Idle` | 仅发现/观察，不能捕获、持有或领奖 |
| `D20_PetsRare_fox_observation.prefab` | `QuaterniusAnimatedAnimals/Models/Fox.fbx` / `animal_quaternius_fox_v01` | rare observation | 0.55m | 12 条，含 Idle、Walk、Gallop、Eating、HitReact | 仅发现/观察，不能捕获、持有或领奖 |

每个 Prefab 都有：真实源网格、Animator、独立 Controller、LODGroup、`Visual/LOD0/LOD1/LOD2`、`RemoteFallback`、Follow/Interaction/Boarding/Rest/DiscoveryHint 五个锚点，以及仅用于避免阻塞的 `CollisionProxy` 触发器。没有 Rigidbody、CharacterController 或 NavMeshAgent。根节点保持 scale 1，模型子节点承载测量后的目标高度。

机器明细见 `game/tools/results/D20/D20-A06/D20_A06_PREFAB_BUILD.json`；其中记录了每个模型的 Renderer、材质数、真实动画数、Controller 状态和锚点完整性。

## 动作映射

Controller 为每个候选建立 19 个表现状态：`Idle/Walk/Run/Follow/Stay/Rest/Sit/Interact/Play/Eat/Travel/Alert/Startled/Flee/Observe/Observed/Unavailable/DiscoverySuccess/DiscoveryFailure`。

- cat：Idle 与 Walking 为真实片段；Walk/Follow 使用 Walking，其余未找到的动作明确回退到 Idle。
- dog Shiba、fox：Walk/Follow 使用 Walk，Run/Flee 使用 Gallop，Interact/Play/Eat 使用 Eating 或 Attack，Rest/Sit 使用 Idle_2_HeadLow/Idle_2，Alert/Startled 使用 HitReact；找不到时回退 Idle。
- eagle：Idle/Observed/Observe 使用 Idle；Travel/Alert/Startled/Flee 使用真实 Flying 作为飞行 fallback；其余宠物专用状态不向珍稀动物开放。
- 任何 fallback 都是视觉层动作复用，不生成新的事实、奖励、捕获或生态结果。

## 只读表现适配器

`D20PetRarePresentationAdapter` 只接受上层已裁决的 `D20PetRarePresentationInput`，写入自身 Transform、Renderer 显隐、Animator 播放和自身锚点：

- Follow、Stay、Interact、Startled、Unavailable；
- Boarding、Disembarking、Lost/不可用、远距降级和 `RecoverAtAuthoritativePosition`；
- `DiscoveryFeedback` 的观察提示、发现成功、发现失败；提示 key 只表达状态，不携带坐标；
- Near/Mid 使用完整模型，Far 使用远距 fallback，Proxy/不可用隐藏表现并停止 Animator。

位置始终由输入快照提供。适配器不寻找目标、不写宠物状态、不改变所有权/好感/健康/生态/奖励、不访问保存写者或网络/LLM。珍稀动物 Prefab 设置 `rareObservationOnly=true`，不会提供捕猎入口。

## 关键流程与死锁规避

1. 领养后：PetSystem 提供物种和位置快照，表现层进入 Idle/Follow；UI 不能直接改所有权。
2. 跟随/停留/互动：系统给出 Follow/Stay/Interact 状态和锚点，适配器不计算路径。
3. 上下船：系统先给 Boarding/Disembarking 与权威位置，适配器移动到该位置；触发器碰撞不阻挡门、梯、舵位或登船点。
4. 跟随丢失/传送/读档：系统重新提供 `authoritativeWorldPosition`，调用 `RecoverAtAuthoritativePosition`；没有本地搜索、重生或所有权推断。
5. 远距：切换 Far/Proxy 时只显示 fallback，不继续播放完整 Animator；回到 Near/Mid 再恢复完整模型。
6. 珍稀观察：发现成功/失败仅显示 observation/codex 状态。未发现位置不会进入提示 key 或 Prefab 数据，摄影/观察反馈不自动捕获或发奖。

## 验证

Unity 6000.0.80f1 在隔离 Unity 工程中完成：

- Prefab 构建：`-executeMethod WhalesIslandReef.Editor.D20.PetRare.D20A06PrefabBuilder.BatchBuild`，退出码 0；4/4 Prefab 生成。
- EditMode：`-runTests -testPlatform editmode -assemblyNames WhalesIslandReef.D20.PetRare.Tests`，6/6 passed、0 failed、0 skipped；结果 `game/tools/results/D20/D20-A06/D20_A06_EDITMODE_TESTS_FINAL.xml`。
- 覆盖：Prefab/Animator/Controller/LOD/锚点完整性、触发器非阻塞碰撞、四个稳定 ID、只读 Follow/Stay/Interact/Boarding/Far/Recovery、发现提示不泄露坐标、表现源无网络/LLM/强逻辑接入。
- 预览：4 张 640×480 预览图位于 `game/tools/results/D20/D20-A06/previews/`；为避免隔离工程材质管线差异，预览脚本临时使用 `Sprites/Default` 纯色材质，仅用于检查轮廓、比例、相机可见性，不改变正式 Prefab 材质。
- Unity 日志存在隔离工程既有 Burst/Collections 包的非阻断 `System.Runtime.CompilerServices.Unsafe` 解析告警；测试运行仍为 Passed，未归因于 A06 代码，完整日志保留在 `D20_A06_EDITMODE_TESTS_FINAL.log` 与 `D20_A06_PREVIEW_FIXTURE.log`。
- `git diff --check` 在交接前执行；未创建提交或推送。

正式运行时晋级仍等待 D18-S03；本任务没有标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。
