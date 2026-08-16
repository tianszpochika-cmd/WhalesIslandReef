# D18-A04 角色与动物候选资产包

任务：D18-A04。结论：**PARTIAL（候选包、许可证据和 Unity 导入/动作审计完成；Prefab 级生产和编辑器截图未完成）**。

本成果只写角色/动物候选目录、该目录的 D18-A01 manifest、D18-A04 结果和交接。没有修改 CharacterSystem、AnimalSystem、存档、Registry、InitialHarbor、正式 Prefab、正式 Animator 或运行时 Systems。所有候选保持 `Candidate`，没有登记 `ReleaseApproved`。

## 1. 交付范围与真实数量

Unity 6 `6000.0.80f1` 在 D18-A04 结果夹具中实际导入并预检 24 个 FBX 文件：30 个 Mesh、228 个动画片段、121 个材质，0 个错误、25 个命名警告。`Horse_White.fbx` 是 Horse 的颜色变体，保留来源和审计行但不计作额外独立模型，所以按模型身份计为 23 个；按真实导入文件计为 24 个。OpenGameArt Bird 只有 Blender 源文件，未计入 Unity 模型或动画数量。

| 来源包 | FBX 文件 | Mesh | 动画 | 材质 | 结果 |
|---|---:|---:|---:|---:|---|
| Kenney Blocky Characters | 1 | 6 | 27 | 1 | Candidate；中性/玩家基体候选 |
| Quaternius Animated Men | 4 | 4 | 44 | 27 | Candidate；现代/时代不符项需改造 |
| Quaternius Ultimate Animated Animals | 12 | 13 | 150 | 76 | Candidate；含狗宠物、陆生动物 |
| Quaternius Animated Fish | 7 | 7 | 7 | 17 | Candidate；水生表现只有 Swim |
| OpenGameArt Bird | 0（Blender 源文件 1） | 0 | 0 | 0 | NotImported；不进入运行时 |
| **合计** | **24** | **30** | **228** | **121** | Unity 审计 PASS |

逐资产机器表：`game/tools/results/D18/D18-A04/D18_A04_ASSET_ASSESSMENT.tsv`。完整片段名：`game/tools/results/D18/D18-A04/D18_A04_CLIP_NAMES.tsv`。

## 2. 来源、授权和哈希

所有正式候选都保留包内许可证文本或来源页许可证据。五个 manifest 复用 D18-A01 的 `tools/schemas/d18_asset_pack_manifest.schema.json`，没有创建第二套清单。许可均登记为 `CC0-1.0`；以下哈希是本次保留的下载快照/原始归档哈希，不是发布批准。

| 包 | 官方来源 | 本地候选/证据 | 快照或归档 SHA-256 |
|---|---|---|---|
| Kenney Blocky Characters | [kenney.nl/assets/blocky-characters](https://kenney.nl/assets/blocky-characters) | `game/Assets/Art/Candidates/CharactersAnimals/KenneyBlockyCharacters/License.txt` | `72CAA887BBA1B301B0BDB82B4FBC2702058C72314F3A1680D2558C6DA6E98338` |
| Quaternius Animated Men | [quaternius.com/packs/animatedmen.html](https://quaternius.com/packs/animatedmen.html) | `.../QuaterniusAnimatedMen/License.txt` | `6A843D2498C488AA466BEE6D7FD11980D840AB61336E10A1475D0AD6BEE5EFF0` |
| Quaternius Ultimate Animated Animals | [quaternius.com/packs/ultimateanimatedanimals.html](https://quaternius.com/packs/ultimateanimatedanimals.html) | `.../QuaterniusAnimatedAnimals/License.txt` | `A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827` |
| Quaternius Animated Fish | [quaternius.com/packs/animatedfish.html](https://quaternius.com/packs/animatedfish.html) | `.../QuaterniusAnimatedFish/License.txt` | `D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9` |
| OpenGameArt Bird | [opengameart.org/content/bird-1](https://opengameart.org/content/bird-1) | `.../OpenGameArtBird/Source/LICENSE` | `BE008AE2DAB3C25D2F61632E2E77060AFD4CAA8AB699FDFB0DF6054824EEA567`（原始 `bird.tar.gz`） |

CC0 证据入口：[Creative Commons Zero 1.0](https://creativecommons.org/publicdomain/zero/1.0/)。来源包快照也复制到 `game/tools/results/D18/D18-A04/` 的 `source_evidence/` 和 zip 证据中，便于复核。

## 3. 覆盖与时代适配

| 目标覆盖 | 实际结果 | 结论 |
|---|---|---|
| 玩家男女/中性基体 | Kenney 一个中性/未定性基体候选；Quaternius 只有四个男性外观 | 部分完成；没有已导入女性基体 |
| 港口平民/商人/水手/军人外观模块 | 四个 Quaternius 男性模型按角色语义登记 | 仅视觉候选；服饰需人工时代适配 |
| 猫/狗宠物 | Husky、ShibaInu 两个狗候选 | 狗完成；没有猫 |
| 鸟类 | OpenGameArt Bird Blender 源文件 | 来源已留证，未导入，不能运行时使用 |
| 陆生动物 | Alpaca、Bull、Cow、Deer、Donkey、Fox、Horse、Stag、Wolf，另含 Horse 颜色变体 | 候选完成，比例和栖息地仍由后续 Prefab/生态审查 |
| 代表性水生动物 | Dolphin、Fish1/2/3、Manta ray、Shark、Whale | 候选完成；Fish1/2/3 的种属名待内容审查 |

时代适配结论：Quaternius 四个人类模型是现代或时代含义不明确的服装，`Male_Suit` 明显不适合直接作为 1675 年角色，已标为 `Rejected`；其余三项标为 `NeedsModification`，不能直接宣称港口平民、商人、水手或军人已完成。Kenney Blocky Characters 仅作为中性基体/动作参考，不含已批准的 1675 年服装。动物和鱼没有人类文化标签，但其低多边形风格、比例和行为仍需正式内容审查。

## 4. Unity 导入与逐项预检

Unity 结果：`game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json` 和 `.md`。

- 24 个 FBX 均成功解析；所有文件 `globalScale=1`，报告原始 bounds 和模型尺度，未统一缩放。
- 所有审计模型均为 `Generic`、`NoAvatar`；有骨骼的动物/角色并不等于 Humanoid Avatar 已就绪，不能直接承诺跨骨架重定向。
- 角色有骨骼和材质槽；Quaternius 角色为蒙皮网格，Kenney 是 6 个网格的角色基体。没有把身体、眼睛、头发或服装拆分臆造为独立资产；当前只报告导入层网格/材质事实。
- 动物骨骼数量为 56–70（按文件），材质槽为 4–8；全部 bounds 有效。鱼类骨骼数量为 11–20，材质槽为 2–3；全部 bounds 有效。
- 原始 FBX 的 `Collider=0`、`LODGroup=0`、`InteractionAnchor=0`；这是原始导入事实，不是缺失错误。交互锚点、碰撞体和 LOD 必须由后续 Prefab/角色动物资源任务添加。
- 缺失脚本和丢失引用均为 0。25 个问题全部为原始文件名不符合 D18 snake_case 建议的 warning，不自动重命名源文件，避免改变来源路径和 GUID。
- Quaternius 动物 bounds 最大尺度约 0.039–0.117 Unity 单位；这是审计报告，不由本任务自动放大或统一，避免把模型自身尺度和目标 Prefab 尺度混为一谈。
- A02 的路径分类启发式将部分水生动物和四个人类模型报告为 `Props`；D18-A04 的 manifest、角色/动物角色字段和本表保留真实分类。该偏差是工具分类局限，不被静默改写成通过。

## 5. 动作与行为证据

动作映射建议见 `docs/game/design/common/assets/packs/characters_animals/ACTION_MAPPING.md`。

- Kenney：真实具备 `idle`、`walk`、`sprint`、`interact-left/right`、`pick-up`、`die`，另有持物、攻击、坐下和情绪片段。
- Quaternius 人类：每个 11 个真实片段，含 `Man_Idle`、`Man_Walk`、`Man_Run`、`Man_Jump`、`Man_Death`；没有明确命名的 Interact，不能把 `Man_Standing` 当作交互完成事件。
- 陆生动物：真实片段覆盖 Idle、Walk、Gallop、Eating、HitReact、Attack、Death；不同文件的攻击和大小写存在差异，映射表按真实名称记录。
- 鱼类：每个只有一个 Swim 片段（Fish2 为 `Swim.001`）；没有命名的进食、受惊、死亡或待机动作，不用随机文本或资产名称推导这些行为。
- 鸟类官方页面列出 pose/attack/idle/flap/loony/sing1/sing2，但因为 Blender 未转换，不能计为已导入动作。

## 6. 预览证据

结果目录的 `previews/` 保存五组官方来源静态预览，索引见 `game/tools/results/D18/D18-A04/D18_A04_PREVIEW_EVIDENCE.md`。本任务没有生成 Unity 编辑器截图、预览场景或正式 Animator；这是有意遵守“不接入运行时场景”和单一写者边界，故任务整体保持 PARTIAL。

## 7. 验证命令和结果

### Manifest 审计

```powershell
python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs/characters_animals --output game/tools/results/D18/D18-A04/D18_A04_MANIFEST_AUDIT.json
```

实际结果：5 manifests，0 errors，0 issues，`PASS`。

### Unity 6 隔离夹具导入审计

本次没有用主项目创建场景或正式 Prefab，而是在任务结果内的 `_UnityAuditFixtureProject` 复制夹具中运行 D18-A02 Editor-only 审计。可复现命令：

```powershell
& 'C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Resolve-Path 'game/tools/results/D18/D18-A04/_UnityAuditFixtureProject').Path -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18ArtPipelineBatch.RunKenneyAudit -logFile game/tools/results/D18/D18-A04/unity_a04_import_audit.log
```

实际结果：`status=PASS files=24 meshes=30 animations=228 materials=121 issues=25`，0 errors；Unity 启动日志另有夹具缺少 Burst 依赖的非审计警告，未阻止资产审计。

### 动作片段导出

```powershell
& 'C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Resolve-Path 'game/tools/results/D18/D18-A04/_UnityAuditFixtureProject').Path -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18A04ClipDump.Dump -logFile game/tools/results/D18/D18-A04/unity_a04_clip_dump.log
```

实际结果：`D18_A04_CLIP_NAMES.tsv`，228 个真实片段。最后执行：

```powershell
git diff --check
```

结果和限制见交接报告；本任务没有提交或推送 Git。

## 8. 未完成项与后续入口

1. 没有女性或已批准的中性时代服装基体；没有猫模型。
2. 鸟类仍为 Blender-only；需要独立、可复现的 Blender/FBX 转换工具和重新审计，不得把源文件直接接入 Unity。
3. 没有创建 Prefab、正式 Animator、LOD、碰撞和交互锚点；由后续角色/动物 Prefab 任务消费本包，并在不改原始 GUID 的前提下建立实例数据。
4. Generic/NoAvatar 的 Humanoid 重定向、骨架层级、身高基准和身体/眼睛/头发/服装拆分尚未达到发布门槛。
5. Human pack 的现代服饰、Fish1/2/3 种属语义和动物时代/生态语境需要人工审核；本任务不自动生成审核结论。

下游入口：五个 manifest、`D18_A04_ASSET_ASSESSMENT.tsv`、`D18_A04_CLIP_NAMES.tsv`、动作映射文档、来源许可证和 Unity 审计 JSON。任何后续消费者必须先核对 `review_status=Candidate`、`release_approved_asserted=false`，再由自己的系统/Prefab 任务裁决。
