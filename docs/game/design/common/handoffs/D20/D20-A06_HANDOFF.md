# D20-A06 交接报告

任务：D20-A06（宠物与珍稀动物表现）  
日期：2026-08-15  
结论：PASS（隔离表现交付；不代表 D18-S03 正式晋级）

## 交付范围

单一写者目录内已交付：

- `game/Assets/Prefabs/Animals/D20/PetsRare/`：4 个 Prefab 及 `.meta`；
- `game/Assets/Animations/D20/PetsRare/`：4 个 Animator Controller 及 `.meta`；
- `game/Assets/Scripts/Presentation/D20/PetRarePresentation/`：只读契约、适配器、Editor 构建/预览脚本及 asmdef；
- `game/Assets/Tests/EditMode/D20/PetsRare/`：6 个 EditMode 测试；
- `game/tools/results/D20/D20-A06/`：机器报告、Unity 日志、XML 测试结果和 4 张预览图；
- 设计说明：`docs/game/design/common/D20_PETS_RARE_ANIMALS_PRESENTATION.md`。

没有修改正式场景、InitialHarbor、Pet/Ecology/Reward/Relationship/Save、Registry、contracts、UI 主框架或 D20-A05 接口。

## 前置与准入核对

- D20-S01：PASS，读取 `docs/game/design/common/handoffs/D20/D20-S01_HANDOFF.md`。
- D18-S02-R01：PASS，读取 `docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md` 和最新 intake catalog。
- A05 interface：`game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json` 存在且 `wildlife_interface_ready=true`；本任务只读，未回写。
- intake catalog 中 cat 1 条、dog 2 条 `intake_eligible=true`，实际使用 cat、Shiba Inu；未把 `d18_local_development_ready_catalog.json` 的 BLOCKED 改成可用。
- 使用的 A05 interface SHA-256：`CF94738984A03983CA57B759727F59D42BE78E5FC664A1B0A8600E6C7B55E1CE`。

## 真实模型与动作

| 资产 | content/species id | 真实动画数 | 真实动作/映射 | 用途 |
|---|---|---:|---|---|
| cat core | `animal_quaternius_cat_pack_vol2_v01` | 2 | Idle、Walking；Walk/Follow 用 Walking，其余状态明确 Idle fallback | 宠物核心 |
| dog Shiba | `animal_quaternius_shiba_inu_v01` | 12 | Idle、Walk、Gallop、Eating、Attack、Death、Jump、HitReact；Controller 映射 Walk/Run/Interact/Rest/Alert | 宠物核心 |
| eagle observation | `animal_quaternius_eagle_v01` | 3 | Idle、Flying；Travel/Alert/Startled/Flee 使用 Flying fallback | 珍稀观察 |
| fox observation | `animal_quaternius_fox_v01` | 12 | Idle、Walk、Gallop、Eating、Attack、Death、Jump、HitReact；观察移动映射 | 珍稀观察 |

每个 Controller 含 19 个状态：`Idle/Walk/Run/Follow/Stay/Rest/Sit/Interact/Play/Eat/Travel/Alert/Startled/Flee/Observe/Observed/Unavailable/DiscoverySuccess/DiscoveryFailure`。缺失动作不会伪造新片段，统一回退到真实 Idle 或同类真实片段。

## 结构与只读边界

每个 Prefab 的构建报告均确认：Animator、LODGroup、五个锚点、触发器碰撞存在；所有碰撞器均为 trigger，不含 Rigidbody、CharacterController、NavMeshAgent。根 scale 为 1，目标高度为 cat 0.28m、dog 0.55m、eagle 0.85m、fox 0.55m。

`D20PetRarePresentationAdapter` 仅写自己的 Transform、Renderer、Animator 和锚点。它处理 Follow/Stay/Interact/Startled/Unavailable、上船/下船、丢失后权威位置恢复、读档恢复、Far/Proxy 降级和发现成功/失败提示。它不找目标、不捕获、不发奖励、不改所有权、好感、健康、生态、库存或存档。

珍稀 Prefab 明确 `rareObservationOnly=true`。发现提示只使用状态 prompt key，不保存或显示未发现坐标；不存在新增捕猎玩法。

## 验证命令与结果

Unity 版本：6000.0.80f1。因主工程仍由用户的 D19 编辑器占用，本轮使用隔离 Unity 工程编译与导入，随后只回填 A06 允许目录。

1. `Unity.exe -batchmode -nographics -disable-assembly-updater -projectPath <A06 fixture> -executeMethod WhalesIslandReef.Editor.D20.PetRare.D20A06PrefabBuilder.BatchBuild` → exit 0，4/4 Prefab、4/4 Controller。
2. `Unity.exe -batchmode -nographics -disable-assembly-updater -projectPath <A06 fixture> -runTests -testPlatform editmode -assemblyNames WhalesIslandReef.D20.PetRare.Tests -testResults game/tools/results/D20/D20-A06/D20_A06_EDITMODE_TESTS_FINAL.xml` → 6/6 passed，0 failed，0 skipped，exit 0。
3. `Unity.exe -batchmode -disable-assembly-updater -projectPath <A06 fixture> -executeMethod WhalesIslandReef.Editor.D20.PetRare.D20A06PrefabBuilder.BatchPreview` → exit 0；生成 cat、dog、eagle、fox 四张 640×480 预览图。
4. `git diff --check` → 交接前执行；无新增 whitespace error。

机器证据：

- `game/tools/results/D20/D20-A06/D20_A06_PREFAB_BUILD.json`
- `game/tools/results/D20/D20-A06/D20_A06_EDITMODE_TESTS_FINAL.xml`
- `game/tools/results/D20/D20-A06/D20_A06_EDITMODE_TESTS_FINAL.log`
- `game/tools/results/D20/D20-A06/D20_A06_PREVIEW_EVIDENCE.json`
- `game/tools/results/D20/D20-A06/previews/`

隔离 Unity 日志含项目既有 Burst/Collections 包的 `System.Runtime.CompilerServices.Unsafe` 解析告警，但未阻断编译、导入、预览或测试；原始日志保留，未隐藏该告警。

## 交付哈希

| 文件 | SHA-256 |
|---|---|
| `D20_PetsRare_cat_core.prefab` | `25A6B67C425210E9403C421863F69BC40332716A8A9D72ABE0B7DD65F54C5337` |
| `D20_PetsRare_dog_shiba.prefab` | `F566A6A4FD60D3AF882F05DF05269E0303A06B1072EDF603641073BC73933096` |
| `D20_PetsRare_eagle_observation.prefab` | `5EB1F7FAB46359E4E7947F15DC48E2836547EF1EC9169851F5DA2F99248DC462` |
| `D20_PetsRare_fox_observation.prefab` | `58BE56D66BEFF077F92ABD9A534C42CFDEB5CF313778F3C30499E3B9232C0FFA` |
| `D20_PetsRare_cat_core.controller` | `97EEFC7E911E46DB4A2211B62EE8C3661BA0C91F45A34A540BBB7A083A6D82D3` |
| `D20_PetsRare_dog_shiba.controller` | `FC401187F5BC67F53B29311351138D67E12CC29AB367667EBC1CB642D0CF1593` |
| `D20_PetsRare_eagle_observation.controller` | `56E42B52155F8F3B4CD1A5FB380D5896F4EF060F42D6F69D271412938036F5EC` |
| `D20_PetsRare_fox_observation.controller` | `0C035587BC7022349C306ABEC8C06D3D4D1B83DA1C28FACD17F34A59511D9B21` |

## 后续边界

- 正式场景接入、生态刷新、领养/所有权、好感、奖励、存档和 UI 绑定由所属任务处理；A06 不复制这些领域逻辑。
- D18-S03 负责正式资源晋级；本交付没有标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。
- 预览使用临时纯色材质用于轮廓核验，不替换正式材质，也不接入运行时场景。

结论：D20-A06 **PASS**；可将本交付交给后续集成任务读取，但不能据此跳过 D18-S03 的正式准入门槛。
