# D18-A04 角色与动物资源交接

日期：2026-08-14  
任务：D18-A04（A* 异步资产任务）  
结论：**PARTIAL**

## 结论摘要

本任务已完成官方 CC0 候选核对、许可证/哈希留证、候选导入和动作片段审计、逐资产机器评估与动作映射。Unity 6 隔离夹具实际导入 24 个 FBX，得到 30 Mesh、228 个真实动画片段、121 个材质，0 个导入错误、25 个非阻断命名 warning；manifest 审计 5/5 通过。

整体不能标 PASS 的原因是：没有女性/已时代适配的中性角色基体、没有猫、鸟仅 Blender 源文件未导入、没有 Prefab/正式 Animator/LOD/碰撞/交互锚点，也没有生成 Unity 编辑器预览截图。未把这些缺口伪装成已完成。

## 包、许可、哈希

| 包 | 许可 | 证据/官方页 | 快照或归档 SHA-256 | 文件结果 |
|---|---|---|---|---:|
| Kenney Blocky Characters | CC0-1.0 | `KenneyBlockyCharacters/License.txt`；`https://kenney.nl/assets/blocky-characters` | `72CAA887BBA1B301B0BDB82B4FBC2702058C72314F3A1680D2558C6DA6E98338` | 1 FBX |
| Quaternius Animated Men | CC0-1.0 | `QuaterniusAnimatedMen/License.txt`；`https://quaternius.com/packs/animatedmen.html` | `6A843D2498C488AA466BEE6D7FD11980D840AB61336E10A1475D0AD6BEE5EFF0` | 4 FBX |
| Quaternius Ultimate Animated Animals | CC0-1.0 | `QuaterniusAnimatedAnimals/License.txt`；`https://quaternius.com/packs/ultimateanimatedanimals.html` | `A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827` | 12 FBX |
| Quaternius Animated Fish | CC0-1.0 | `QuaterniusAnimatedFish/License.txt`；`https://quaternius.com/packs/animatedfish.html` | `D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9` | 7 FBX |
| OpenGameArt Bird | CC0-1.0 | `OpenGameArtBird/Source/LICENSE`；`https://opengameart.org/content/bird-1` | `BE008AE2DAB3C25D2F61632E2E77060AFD4CAA8AB699FDFB0DF6054824EEA567`（tar.gz） | 1 Blender 源文件 |

所有来源均保持 `Candidate`，未写人工 `ReleaseApproved`。许可证入口为 `https://creativecommons.org/publicdomain/zero/1.0/`。

## 模型、动作和覆盖

- 实际导入文件：24 FBX；去除 Horse_White 颜色变体后的独立模型身份：23。
- Unity Mesh：30；材质：121；动画片段：228。
- 角色：Kenney 基体 1 个，Quaternius 人类 4 个；人类动作均 Generic/NoAvatar，四个服装不是已批准 1675 年服饰。
- 动物：陆生 12 个源文件（含 1 个 Horse 颜色变体），狗宠物 2 个；猫缺失。
- 水生：Dolphin、Fish1/2/3、Manta ray、Shark、Whale 共 7 个，真实动作各 1 个 Swim（Fish2 为 Swim.001）。
- 鸟类：官方页面动作证据有 pose/attack/idle/flap/loony/sing1/sing2，但 Blender 源文件没有转换，故 Unity 模型/动作数量均为 0。

逐资产检查表：`game/tools/results/D18/D18-A04/D18_A04_ASSET_ASSESSMENT.tsv`。动作映射：`docs/game/design/common/assets/packs/characters_animals/ACTION_MAPPING.md`。原始片段导出：`game/tools/results/D18/D18-A04/D18_A04_CLIP_NAMES.tsv`。

## 导入问题和时代适配

1. 24 个 FBX 均为 Generic/NoAvatar；骨骼存在不代表 Humanoid Avatar 或重定向可用。
2. 所有原始 FBX bounds 有效、材质可解析、缺失脚本/引用为 0；但原始层没有 Collider、LODGroup 或 InteractionAnchor。
3. Quaternius 动物源尺寸在 Unity 审计中约 0.039–0.117；没有自动统一 scale，后续 Prefab 需按物种比例和场景目标尺⼨裁决。
4. 25 个问题全部是源文件名不符合 D18 snake_case 的 warning；没有自动改名，避免改变来源和 GUID。
5. Quaternius 人类服装现代或含义不明：Casual/LongSleeve/Shirt 为 `NeedsModification`，Suit 为 `Rejected`，不直接列为 1675 年可用。
6. Fish1/2/3 的物种语义待人工内容审核；鱼只有 Swim，不能由资产名推断进食、受惊或死亡行为。
7. A02 的路径分类启发式把部分鱼和全部 Animated Men 报为 Props；D18-A04 以 manifest role 和真实资产用途保留分类，同时在审计结果中保留工具原始输出。

## 验证命令与实际结果

Manifest：

```powershell
python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs/characters_animals --output game/tools/results/D18/D18-A04/D18_A04_MANIFEST_AUDIT.json
```

结果：`5 manifests / 0 errors / 0 issues / PASS`。

Unity 6 隔离夹具：

```powershell
& 'C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Resolve-Path 'game/tools/results/D18/D18-A04/_UnityAuditFixtureProject').Path -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18ArtPipelineBatch.RunKenneyAudit -logFile game/tools/results/D18/D18-A04/unity_a04_import_audit.log
```

结果：`status=PASS files=24 meshes=30 animations=228 materials=121 issues=25`，0 errors。夹具启动日志有 Burst 依赖 warning，但没有影响资产审计。

动作导出：

```powershell
& 'C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Resolve-Path 'game/tools/results/D18/D18-A04/_UnityAuditFixtureProject').Path -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18A04ClipDump.Dump -logFile game/tools/results/D18/D18-A04/unity_a04_clip_dump.log
```

结果：228 个真实片段写入 `D18_A04_CLIP_NAMES.tsv`。

预览证据：`game/tools/results/D18/D18-A04/previews/` 为官方来源静态预览；Unity 编辑器截图为 0，原因和后续入口记录在 `D18_A04_PREVIEW_EVIDENCE.md`。  
规范检查：已执行 `git diff --check`；未提交或推送 Git。

## 仓库增量与边界

新增/写入范围仅为：

- `game/Assets/Art/Candidates/CharactersAnimals/`：候选 FBX、Blender 源文件、许可证和 Unity `.meta`；24 个 FBX 均存在对应 `.meta`，无正式 Prefab。
- `docs/game/design/common/assets/packs/characters_animals/`：5 个复用 D18-A01 schema 的 manifest 和动作映射。
- `docs/game/design/common/D18_CHARACTER_ANIMAL_ASSET_PACKET.md`。
- `game/tools/results/D18/D18-A04/`：来源证据、快照、Unity 审计、动作导出、逐资产评估、机器摘要和预览证据。
- `docs/game/design/common/handoffs/D18/D18-A04_HANDOFF.md`。
- `.workbuddy/memory/2026-08-14.md`。

没有修改主场景、正式 Animator、正式 Prefab、CharacterSystem、AnimalSystem、存档、Registry、其他资源分类或 Systems；没有下载来历不明的 Mixamo 转存、商业生成器导出物或购买资产。

## 后续可用入口和未完成项

后续角色/动物 Prefab 任务可从五个 manifest、`D18_A04_ASSET_ASSESSMENT.tsv`、`D18_A04_CLIP_NAMES.tsv`、`ACTION_MAPPING.md` 和 `D18_A04_UNITY_IMPORT_AUDIT.json` 开始。必须先保留 `Candidate` 状态，再为每个实例补 Prefab、碰撞、LOD、交互锚点、时代服装适配和必要的 Avatar 重定向验证。

未完成项：女性或时代中性基体、猫、鸟的 Unity 导入、Prefab/Animator/LOD/碰撞/锚点、Unity Editor 截图、男性服装时代审核、鱼类种属审核。

**最终状态：PARTIAL。**
