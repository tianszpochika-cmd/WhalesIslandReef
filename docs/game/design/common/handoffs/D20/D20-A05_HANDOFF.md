# D20-A05 交接报告：野生动物与水生生物表现

任务：`D20-A05`  
日期：`2026-08-15`  
结论：**PARTIAL**

## 1. 交付摘要

- 真实完成：8 个表现 Prefab、8 个唯一 `species_id`、8 个 Animator Controller、1 个只读表现适配器、1 个远距 fallback 材质、6 项 EditMode 测试。
- 资源来源：D18-S02-R01 当前 `intake_eligible` 清单中的 Quaternius CC0-1.0 条目；未复制源 FBX，未新建第二套动物目录。
- 资源状态：未修改全局 intake catalog，未提升 `LocalDevelopmentReady` 或 `ReleaseApproved`。
- 单一写者：只写 D20-A05 允许的 Prefab、动物表现资源、动画、适配器、测试、结果和文档目录；未写 `AnimalSystem`、`EcologySystem`、正式场景、水体、Registry、存档或奖励逻辑。

主文档：`docs/game/design/common/D20_WILDLIFE_AQUATIC_PRESENTATION.md`  
机器接口：`game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json`  
机器结果：`game/tools/results/D20/D20-A05/D20_A05_MACHINE_RESULT.json`

## 2. Prefab 与 species_id 清单

| species_id | Prefab / 类别 | 动作与 fallback | LOD/放置 |
|---|---|---|---|
| `animal_quaternius_eagle_v01` | `D20_Wildlife_eagle_coastal_bird` / 海鸟 | Idle、Flying；Alert/Flee/Perch/Takeoff/Land/Feed fallback | 空中/海岸，0.85m，根偏移 2.0m |
| `animal_quaternius_fox_v01` | `D20_Wildlife_fox_small_land` / 陆生小动物 | Idle、Eating、Walk、HitReact、Gallop | 地面，0.75m |
| `animal_quaternius_cow_v01` | `D20_Wildlife_cow_livestock` / 家畜或野生代表 | Idle、Eating、Walk、HitReact、Gallop | 地面，1.8m |
| `fish_quaternius_fish_01_v01` | `D20_Wildlife_fish_shallow_school` / 浅水鱼群 | Swim；Turn/Dive/Surface/Flee fallback | 水深参考 0.8m，0.55m |
| `fish_quaternius_fish_02_v01` | `D20_Wildlife_fish_nearshore` / 近海鱼 | Swim；Turn/Dive/Surface/Flee fallback | 水深参考 1.5m，0.7m |
| `animal_quaternius_shark_v01` | `D20_Wildlife_shark_nearshore_predator` / 近海掠食者 | Swim；其它水生状态 fallback | 水深参考 4m，3m |
| `animal_quaternius_manta_ray_v01` | `D20_Wildlife_manta_large_aquatic_equivalent` / 大型水生等价样本 | Swim；其它水生状态 fallback | 水深参考 3m，2.5m；不是海龟 |
| `animal_quaternius_whale_v01` | `D20_Wildlife_whale_distant_aquatic` / 远景大型水生 | Swim；其它水生状态 fallback | 水深参考 10m，12m |

每项的实际模型文件、manifest、归档 SHA-256、许可和 D18 Unity 导入证据均在 `D20_A05_WILDLIFE_INTERFACE.json` 和 `D20_A05_PREFAB_AUDIT.json` 中逐项定位。

## 3. 交接接口与所有权

`D20WildlifePresentationAdapter` 只接收 `D20WildlifePresentationInput`：`speciesId`、行为状态、世界位置、速度、水深和生态可见性快照。它只更新视觉 Transform、Renderer、Animator、LOD 和远距替代。

以下事实不能从 Prefab、Animator、VFX 或 A06 宠物接口推导或写入：种群、刷新、位置事实、行为裁决、捕获、稀有度、采集压力、法律、健康、死亡、奖励、库存、任务、存档、宠物归属、收养、跟随目标、好感和喂养结果。Animal/Ecology/Pet 所属系统仍是唯一写者。

## 4. 性能与验证

- LOD 带：`0—12m / 12—32m / 32—80m / 80m+`。
- Animator：近/中 `CullUpdateTransforms`，远/代理 `CullCompletely`；Root Motion 关闭。
- 1080p 静态同屏预算：Low `12`、Medium `24`、High `32`。
- 8/8 Prefab 通过 GUID、模型引用、Controller、LOD、碰撞代理、fallback、根 scale 和源导入证据检查。
- Unity 构建/导入日志：`game/tools/results/D20/D20-A05/D20_A05_PREFAB_BUILD.log`，无本任务编译错误。
- EditMode：`6/6 PASS`，结果 `game/tools/results/D20/D20-A05/D20_A05_EDITMODE_TESTS.xml`。
- 静态性能结果：`game/tools/results/D20/D20-A05/D20_A05_PERFORMANCE_SAMPLE.json`。
- 真实 Player/目标设备 FPS、GC、GPU、1080p 同屏截图未运行，不能在本报告中宣称通过。

## 5. 缺口与后续责任

1. 岸鸟/小型鸟第二样本：当前只有 Eagle intake eligible；原 Blender-only bird 不计数。
2. 海龟：D11/D15 有内容 ID，但没有 intake eligible 海龟模型；Manta 只作为明确标注的等价样本。
3. Eagle 近镜动作及全部水生 Turn/Dive/Surface/Flee：使用安全 fallback，等待有证据的动作资源。
4. 运行时截图和目标设备性能：交给 D20-S02 工作/集成场景执行；不得为了截图修改 InitialHarbor 或正式场景。
5. 资源晋级：交给 D18-S03；本任务不改变资源准入状态。

## 6. 文件交接

新增或更新的任务文件：

- `game/Assets/Prefabs/Animals/D20/Wildlife/`：8 个 Prefab及其 `.meta`。
- `game/Assets/Animations/D20/Animals/`：8 个 Controller及其 `.meta`。
- `game/Assets/Art/Models/Animals/D20/D20_Wildlife_RemoteFallback.mat`。
- `game/Assets/Scripts/Presentation/D20/Wildlife/D20WildlifePresentationAdapter.cs` 及运行时 asmdef。
- `game/Assets/Tests/EditMode/D20/D20WildlifePresentationTests.cs` 及测试 asmdef。
- `docs/game/design/common/D20_WILDLIFE_AQUATIC_PRESENTATION.md`。
- `game/tools/results/D20/D20-A05/` 下接口、审计、测试和性能结果。

推荐下一步：D20-S02 读取 `D20_A05_WILDLIFE_INTERFACE.json`，只在授权工作/集成场景消费这些 Prefab，并补齐运行时截图、层级和目标设备性能证据；不要以当前 8 个表现样本替代未准入的鸟类/海龟内容。
