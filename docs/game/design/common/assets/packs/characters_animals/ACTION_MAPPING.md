# D18-A04 动作片段映射建议

本文件只记录候选资源的真实片段到游戏语义的映射建议，不创建 Animator、不修改运行时状态，也不改变 CharacterSystem/AnimalSystem 的裁决权。片段名称来自 `game/tools/results/D18/D18-A04/D18_A04_CLIP_NAMES.tsv`。

## 角色

| 语义 | Kenney `character-a.fbx` | Quaternius Animated Men | 说明 |
|---|---|---|---|
| Idle | `idle` | `HumanArmature\|Man_Idle` | 可作为默认待机候选 |
| Walk | `walk` | `HumanArmature\|Man_Walk` | 可作为移动候选 |
| Run | `sprint` | `HumanArmature\|Man_Run` | 仅为动作建议，速度由运动系统决定 |
| Interact | `interact-left` / `interact-right` | 无明确 Interact 片段；暂用 `Man_Standing` 作为占位 | 不得把占位动作当作交互完成事件 |
| Jump | 无明确 Jump 片段 | `HumanArmature\|Man_Jump` / `Man_RunningJump` | 角色动作候选 |
| Hit/Death | `die` | `HumanArmature\|Man_Death` | 伤病/死亡状态仍由角色系统裁决 |
| Social | `emote-yes` / `emote-no` / `sit` | `Man_Clapping` / `Man_Sitting` | 只用于表现，不裁决关系结果 |

Kenney 还提供攻击、持物、拾取、轮椅移动等片段。Quaternius 四个男性模型都是 Generic/NoAvatar，不能在没有人工重定向检查时当作 Humanoid retarget-ready；`Male_Suit` 不直接作为 1675 年服装使用。

## 陆生动物

| 语义 | 真实片段组 | 适用候选 |
|---|---|---|
| Idle | `AnimalArmature\|Idle`, `Idle_2`, `Idle_Headlow` 或 `Idle_2_HeadLow` | Alpaca、Bull、Cow、Deer、Donkey、Fox、Horse、Horse_White、Husky、ShibaInu、Stag、Wolf |
| Move | `Walk` | 全部陆生动物 |
| Fast move | `Gallop`, `Gallop_Jump` | 有该片段的马、鹿、牛科等；不可由资源包推断速度数值 |
| Feed | `Eating` | 全部陆生动物 |
| Startle/Hit | `Idle_HitReact_Left`, `Idle_HitReact_Right` | 全部陆生动物 |
| Attack | `Attack`, 或 `Attack_Headbutt` / `Attack_Kick` | 以每个模型的真实片段为准，战斗/行为裁决不在资产层 |
| Death | `Death` | 全部陆生动物 |
| Jump return | `Jump_ToIdle` 或 `Jump_toIdle` | 以大小写真实导出名为准 |

Husky 和 ShibaInu 只登记为狗宠物候选；包内没有猫。`Horse_White` 是颜色变体，保留文件和审计记录，但不计作额外独立模型。

## 水生动物

| 语义 | 真实片段 | 适用候选 | 缺口 |
|---|---|---|---|
| Swim | `Armature\|Swim` | Dolphin、Fish1、Fish3、Manta ray、Shark、Whale | 只有游泳片段 |
| Swim | `Armature\|Swim.001` | Fish2 | 只有游泳片段 |

水生候选没有命名的 Idle、Feeding、Startle 或 Death 片段，不得用随机文本或动作名推导行为。Fish1/2/3 的种属名称仍待内容审查；鱼群位置与生态行为仍由 EcologySystem/AnimalSystem 负责。

## 鸟类源文件

OpenGameArt Bird 的官方页面列出 `pose`、`attack`、`idle`、`flap`、`loony`、`sing1`、`sing2`，但本次仅获得 Blender 源文件，未转换为 FBX，也未导入 Unity。因此这些是“待转换动作映射”，不是已导入动画数量，不进入 24 个 FBX/228 个片段统计。

## 使用边界

- 本文件不生成控制器、不连接场景、不创建运行时消费者。
- 角色服饰/时代适配、文化与职业标签由内容审核和外观系统决定；动作片段不改变这些标签。
- Generic/NoAvatar、缺少动作或源文件未导入必须在 Prefab/Animator 交接时显式处理，禁止静默重定向。
