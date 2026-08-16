# D18-A04-R01 角色、动物与动作候选证据复审

任务：`D18-A04-R01`  
复审日期：2026-08-15  
结论：**PARTIAL**

本复审只读取 D18-A04 已有候选、五个角色/动物 manifest、D18-A04 Unity 隔离导入结果、动作导出、D18-S02-v2 准入目录和 D20-S01 标准。没有修改全局准入目录、原始 manifest、Registry、asset_state、Prefab、场景、Systems、存档或 contracts。

`D18-A04-R01_CHARACTER_ANIMAL_REVIEW.json` 是逐候选机器证据源：它保存 25 条候选记录的本地路径、候选文件哈希、归档哈希、许可证证据哈希、Unity 审计字段、动作列表、缺口和消费类别；同时保存本轮输入文件哈希。本文只给出可读摘要。

## 1. 复审结论

- 角色/动物 manifest：5 个，manifest 审计 `PASS`，0 error、0 issue。
- 候选记录：25 条；其中 24 个 FBX 在 D18-A04 的 Unity 6 隔离夹具中导入，1 个鸟类是 Blender 源文件，未导入 Unity。
- 独立模型身份：23 个；`Horse_White.fbx` 是 Horse 颜色变体，保留审计记录但不计为第二个独立模型。
- Unity 隔离导入事实：30 Mesh、228 动画片段、121 材质、0 error、25 个命名 warning。
- 25/25 候选文件存在，25/25 有 `.meta`；5/5 包内许可证证据存在，5/5 本地保留归档的 SHA-256 与 manifest 一致。
- 当前全局 `PrefabCandidateIntake` 对这 5 个 manifest 是 **0 条**：它们的 `review_status` 仍为 `Candidate`；鸟类另有 `import_status=NotImported`。本复审没有把任何记录写入准入目录。
- `LocalDevelopmentReady=0`，`ReleaseApproved=0`，均保持不变。

因此，本轮只能证明“若经 D18-S02-R01 最终裁决，哪些源文件值得进入后续 D20 工作场景复核”，不能证明 D20 Prefab、Animator、碰撞、LOD、锚点、运行时消费者或正式集成已经完成。

## 2. 包级来源与证据

| 包 | 官方页 | 许可证据 | 归档 SHA-256（与 manifest 比对） | 候选/Unity 导入 | 当前状态 |
|---|---|---|---|---:|---|
| Kenney Blocky Characters | https://kenney.nl/assets/blocky-characters | `game/Assets/Art/Candidates/CharactersAnimals/KenneyBlockyCharacters/License.txt`；CC0-1.0 | `72CAA887BBA1B301B0BDB82B4FBC2702058C72314F3A1680D2558C6DA6E98338`；匹配 | 1 / 1 FBX | Candidate；中性视觉基体候选 |
| Quaternius Animated Men | https://quaternius.com/packs/animatedmen.html | `.../QuaterniusAnimatedMen/License.txt`；CC0-1.0 | `6A843D2498C488AA466BEE6D7FD11980D840AB61336E10A1475D0AD6BEE5EFF0`；匹配 | 4 / 4 FBX | Candidate；服饰/Avatar 需复核 |
| Quaternius Ultimate Animated Animals | https://quaternius.com/packs/ultimateanimatedanimals.html | `.../QuaterniusAnimatedAnimals/License.txt`；CC0-1.0 | `A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827`；匹配 | 12 / 12 FBX | Candidate；含狗、陆生动物 |
| Quaternius Animated Fish | https://quaternius.com/packs/animatedfish.html | `.../QuaterniusAnimatedFish/License.txt`；CC0-1.0 | `D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9`；匹配 | 7 / 7 FBX | Candidate；只有 Swim |
| OpenGameArt Bird | https://opengameart.org/content/bird-1 | `.../OpenGameArtBird/Source/LICENSE`；文件内容为 public domain | `BE008AE2DAB3C25D2F61632E2E77060AFD4CAA8AB699FDFB0DF6054824EEA567`；原始 tar.gz 匹配 | 1 / 0 Unity 模型 | Candidate + NotImported；build_distribution=pending |

OpenGameArt Bird 的归档在 D18-A04 的 `_source_downloads/opengameart_bird.tar.gz` 中，候选是 `bird.blend`。本包有来源、归档哈希和本地许可证文本，但没有 FBX、Unity Mesh、Unity Animation 或导入错误/成功证据；不能按已导入鸟类消费。

## 3. D18 状态与准入边界

D18-S02-v2 当前准入目录对角色/动物五包的排除理由如下：

| manifest | 候选文件数 | 排除理由 |
|---|---:|---|
| `pack_kenney_blocky_characters_d18_a04_v1` | 1 | `review_status=Candidate` |
| `pack_quaternius_animated_men_d18_a04_v1` | 4 | `review_status=Candidate` |
| `pack_quaternius_ultimate_animated_animals_d18_a04_v1` | 12 | `review_status=Candidate` |
| `pack_quaternius_animated_fish_d18_a04_v1` | 7 | `review_status=Candidate` |
| `pack_opengameart_bird_d18_a04_v1` | 1 | `review_status=Candidate`；`import_status=NotImported` |

这不是本复审的失败写入，也不是允许绕过状态机的例外。`PrefabCandidateIntake`、`LocalDevelopmentReady` 和 `ReleaseApproved` 仍然分别表示不同证据门；推荐下一步复核不等于准入。

## 4. 逐候选机器复核摘要

下表的 `Mesh/Clip/Mat/Bones/MaxDim` 来自 `D18_A04_UNITY_IMPORT_AUDIT.json` 的隔离夹具结果；`MaxDim` 是原始导入 bounds 最大维度，不能直接当作最终世界高度。所有已导入记录的轴向参考为 Unity Y-up/Z-forward、global scale=1、animation type=Generic、avatar setup=NoAvatar；原始导入均没有 Prefab 级 Collider、LODGroup 或 InteractionAnchor。

| candidate_id | 文件 | 消费类别 | 隔离导入 | Mesh | Clip | Mat | Bones | MaxDim | 复审结论 |
|---|---|---|---:|---:|---:|---:|---:|---:|---|
| `asset_kenney_blocky_character_a_v01` | `character-a.fbx` | player_visual_core, animation_core | 是 | 6 | 27 | 1 | 8 | 9.000000 | 推荐进入受控工作 Prefab 复核；不等于人形 Avatar 就绪 |
| `animal_opengameart_bird_v01` | `bird.blend` | bird_gap | 否 | 0 | 0 | 0 | 0 | — | 缺 Unity 导入证据 |
| `animal_quaternius_dolphin_v01` | `Dolphin.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 2 | 20 | 0.092689 | 推荐进入受控工作复核；只有 Swim |
| `fish_quaternius_fish_01_v01` | `Fish1.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 3 | 14 | 0.079835 | 推荐进入受控工作复核；种属语义待内容审核 |
| `fish_quaternius_fish_02_v01` | `Fish2.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 3 | 14 | 0.098742 | 推荐进入受控工作复核；动作名为 Swim.001 |
| `fish_quaternius_fish_03_v01` | `Fish3.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 3 | 11 | 0.078771 | 推荐进入受控工作复核；种属语义待内容审核 |
| `animal_quaternius_manta_ray_v01` | `Manta ray.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 2 | 20 | 0.117259 | 推荐进入受控工作复核；只有 Swim |
| `animal_quaternius_shark_v01` | `Shark.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 2 | 15 | 0.099306 | 推荐进入受控工作复核；只有 Swim |
| `animal_quaternius_whale_v01` | `Whale.fbx` | wildlife_core, animation_core | 是 | 1 | 1 | 2 | 20 | 0.101943 | 推荐进入受控工作复核；只有 Swim |
| `crew_quaternius_male_casual_v01` | `Male_Casual.fbx` | npc_visual_core, animation_core | 是 | 1 | 11 | 6 | 45 | 0.051972 | 需时代服饰与 Humanoid 复核 |
| `crew_quaternius_male_long_sleeve_v01` | `Male_LongSleeve.fbx` | npc_visual_core, animation_core | 是 | 1 | 11 | 5 | 45 | 0.051972 | 需时代服饰与 Humanoid 复核 |
| `crew_quaternius_male_shirt_v01` | `Male_Shirt.fbx` | npc_visual_core, animation_core | 是 | 1 | 11 | 9 | 45 | 0.051972 | 需时代服饰与 Humanoid 复核 |
| `crew_quaternius_male_suit_v01` | `Male_Suit.fbx` | npc_visual_core, animation_core | 是 | 1 | 11 | 7 | 45 | 0.051972 | 不适合直接作为 1675 服饰 |
| `animal_quaternius_alpaca_v01` | `Alpaca.fbx` | wildlife_core, animation_core | 是 | 1 | 13 | 7 | 64 | 0.053976 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_bull_v01` | `Bull.fbx` | wildlife_core, animation_core | 是 | 1 | 12 | 7 | 59 | 0.080670 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_cow_v01` | `Cow.fbx` | wildlife_core, animation_core | 是 | 1 | 12 | 7 | 59 | 0.080670 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_deer_v01` | `Deer.fbx` | wildlife_core, animation_core | 是 | 1 | 13 | 7 | 64 | 0.043973 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_donkey_v01` | `Donkey.fbx` | wildlife_core, animation_core | 是 | 1 | 13 | 8 | 68 | 0.047080 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_fox_v01` | `Fox.fbx` | wildlife_core, animation_core | 是 | 1 | 12 | 5 | 70 | 0.058831 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_horse_v01` | `Horse.fbx` | wildlife_core, animation_core | 是 | 1 | 13 | 8 | 68 | 0.056761 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_horse_white_v01` | `Horse_White.fbx` | wildlife_core, animation_core | 是 | 1 | 13 | 7 | 68 | 0.056761 | 只作 Horse 颜色变体，不计独立模型 |
| `animal_quaternius_husky_v01` | `Husky.fbx` | pet_core, animation_core | 是 | 1 | 12 | 5 | 68 | 0.038892 | 推荐作为狗宠物工作候选；不是猫 |
| `animal_quaternius_shiba_inu_v01` | `ShibaInu.fbx` | pet_core, animation_core | 是 | 1 | 12 | 6 | 65 | 0.039484 | 推荐作为狗宠物工作候选；不是猫 |
| `animal_quaternius_stag_v01` | `Stag.fbx` | wildlife_core, animation_core | 是 | 2 | 13 | 5 | 56 | 0.048432 | 推荐进入受控工作 Prefab 复核 |
| `animal_quaternius_wolf_v01` | `Wolf.fbx` | wildlife_core, animation_core | 是 | 1 | 12 | 4 | 70 | 0.055531 | 推荐进入受控工作 Prefab 复核 |

### 4.1 动作事实

- Kenney 基体的真实片段包括 `idle`、`walk`、`sprint`、`interact-left/right`、`pick-up`、`die` 等；这些只是片段存在性，不是交互事实写入。
- Quaternius 四个人物各有 11 个片段，包含 `Man_Idle`、`Man_Walk`、`Man_Run`、`Man_Jump`、`Man_Death`；没有明确命名的 Interact。`Man_Standing` 不被当成交互完成动作。
- 陆生动物真实片段覆盖 `Idle`、`Walk`、`Gallop`、`Eating`、`HitReact`、`Attack`/`Attack_Headbutt`/`Attack_Kick`、`Death`；具体大小写以 `D18_A04_CLIP_NAMES.tsv` 为准。
- 水生候选各只有一个 `Swim`，Fish2 是 `Swim.001`；没有用动作名推导进食、受惊、死亡或奖励。
- 鸟类网页/manifest 记录的 `pose/attack/idle/flap/loony/sing1/sing2` 是来源描述，因 Blender 未导入，不计入 228 个 Unity 动画片段。

## 5. 五类 D20 工作支持结论

| 消费类别 | 真实支持 | 能否开始 D20 工作场景 | 仍缺什么 |
|---|---|---|---|
| `player_visual_core` | 1 个 Kenney 中性视觉基体 | **PARTIAL：可作为受控工作复核输入，不能直接接入** | Humanoid Avatar/T 或 A pose、D17 1.80m 目标、脚底、材质槽/外观拆分、Prefab/碰撞/LOD/锚点 |
| `npc_visual_core` | 4 个男性视觉候选；3 个需时代审核，Suit 不适合直接 1675 | **PARTIAL：仅视觉原型方向** | 人形重定向、时代服饰/身份审查、Prefab/Animator/LOD/锚点；不能由服饰裁决身份或关系 |
| `wildlife_core` | 9 个独立陆生候选 + 7 个水生候选 | **PARTIAL：可做 Generic 工作场景候选** | 物种/生境语义、真实尺度与水深、Prefab 碰撞/LOD/观察锚点、D20 最低动作集和生态只读接入 |
| `pet_core` | Husky、ShibaInu 两个狗候选 | **PARTIAL：可做狗宠物工作候选** | 猫缺口；跟随/停留/上下船/恢复表现由 D20-A06 读取系统事实，不能由资源决定 |
| `animation_core` | 24 个 FBX、228 个真实片段 | **PARTIAL：只有原始 Generic 片段证据** | Animator 参数绑定、Humanoid Avatar 或 Generic 状态机、Root Motion=false 验证、fallback、运行时消费者 |

## 6. 明确缺口

1. `female_or_neutral_human_gap`：没有女性模型。Kenney `character-a` 是中性视觉基体，但原始 bounds 为 9m、Generic/NoAvatar、无 1675 服装审查，不能宣称“女性/中性人形已完成”。责任：`D18-A04-R02-F`、后续 `D20-A01`。
2. `cat_gap`：五个已审计包中没有猫。Husky/ShibaInu 不能缩放、换色或改名为猫。责任：`D18-A04-R02-C`、后续 `D20-A06`。
3. `bird_gap`：有 1 个 Blender 源文件、来源页、许可证和归档 SHA，但没有 Unity 导入模型/动画；不能进入 D20-A05 运行时或当作 0 缺口。责任：`D18-A04-R02-B`、后续 `D20-A05`。
4. `humanoid_avatar_gap`：24 个导入记录均为 `Generic/NoAvatar`，不满足 D20 玩家/NPC 的 Humanoid 唯一重定向入口。
5. `prefab_runtime_evidence_gap`：D18-A04 没有创建 Prefab、正式 Animator、LODGroup、碰撞体、交互锚点、运行时消费者、保存/恢复或 Unity 编辑器预览截图。
6. Fish1/2/3 的物种名称尚未由内容/生态审查裁决；不能用模型颜色或文件名代替物种事实。
7. Quaternius 人物服饰现代或语义不明确；`Male_Suit` 不能直接作为 1675 服饰，另外三项只能在人工时代审核和改造后再决定。

## 7. 推荐分组（推荐不等于准入）

### 推荐进入后续受控工作 Prefab 复核

19 条模型身份/候选方向：Kenney 中性基体 1 条；非狗陆生与水生 16 条；Husky/ShibaInu 狗宠物 2 条。它们都有本地候选文件、包级许可证/归档哈希和隔离 Unity 导入证据（鸟除外），但仍保持 `Candidate`，需要 D20 任务补齐 Prefab 级证据。

### 需补证据或人工审核

- `Male_Casual`、`Male_LongSleeve`、`Male_Shirt`：需 1675 时代服饰审查、Humanoid Avatar/骨架重定向和 D20 视觉工作 Prefab 证据。
- 所有动物：需按 D20-S01 记录物种尺度、脚底/水深、Generic 骨架、碰撞、LOD、观察锚点和最低动作 fallback。
- 所有水生候选：需由 Ecology/Animal 所属系统提供水深/鱼群事实；资源不能发放捕获、观察或奖励。

### 不适合直接使用

- `Male_Suit`：机器导入存在且来源/哈希完整，但 D18-A04 已将其标为 `Rejected`，不能直接表达 1675 军人/官员。
- `Horse_White`：可作为 Horse 颜色变体的来源记录，不得计为独立模型或用来凑覆盖数量。

### 明确缺口

女性或时代中性完整基体、猫、已导入鸟类、Humanoid Avatar/重定向、Prefab/LOD/碰撞/锚点/运行时消费者均保持 OPEN。最终 `intake_eligible` 和 `LocalDevelopmentReady` 裁决只能由 D18-S02-R01/D18-S03 及其责任人完成，本复审不代签。

## 8. 验证命令与结果

```text
python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs/characters_animals --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_MANIFEST_AUDIT.json
  -> PASS; 5 manifests; 0 errors; 0 issues

python tools/d18_prefab_candidate_intake_audit.py --project-root . --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_INTAKE_CATALOG_AUDIT.json
  -> PASS; 25 manifests; 1,857 candidates; 1,674 global intake entries; current resource state unchanged

python tools/d18_a04_r01_review_audit.py --project-root . --output docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json
  -> PARTIAL; 25 candidate records; 24 isolated FBX imports; 1 source-only Blender record; 0 global intake records

python -m pytest ...
  -> NOT AVAILABLE: environment reports `No module named pytest`

python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake
  -> 11/11 PASS

python -m json.tool <the two catalogs and R01 JSON>
  -> PASS

git diff --check
  -> PASS（以最终工作区复验结果为准）
```

Unity 证据不是本轮重新改造或导入：本轮复核的事实范围是 D18-A04 已保存的 Unity 6 `6000.0.80f1` 隔离夹具审计 `D18_A04_UNITY_IMPORT_AUDIT.json`。它不等于主项目已导入、Prefab 已创建或运行时可用。

## 9. 证据入口

- 机器复审：`docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json`
- 逐资产原始评估：`game/tools/results/D18/D18-A04/D18_A04_ASSET_ASSESSMENT.tsv`
- 真实动作片段：`game/tools/results/D18/D18-A04/D18_A04_CLIP_NAMES.tsv`
- Unity 隔离导入：`game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json`
- 包级 manifest 审计：`game/tools/results/D18/D18-A04-R01/D18_A04_R01_MANIFEST_AUDIT.json`
- 全局准入审计：`game/tools/results/D18/D18-A04-R01/D18_A04_R01_INTAKE_CATALOG_AUDIT.json`
- 历史 D18-A04 主成果：`docs/game/design/common/D18_CHARACTER_ANIMAL_ASSET_PACKET.md`
- 历史 D18-A04 交接：`docs/game/design/common/handoffs/D18/D18-A04_HANDOFF.md`
- D20 适配基准：`docs/game/design/common/D20_CHARACTER_RIG_ANIMATION_STANDARD.md`

本文件和 JSON 均不修改六种资源状态，不授权正式场景，不创建 D20 Prefab；结论保持 **PARTIAL**，等待缺口补齐和 D18-S02-R01 的最终准入裁决。
