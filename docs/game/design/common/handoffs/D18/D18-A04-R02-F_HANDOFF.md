# D18-A04-R02-F 交接报告

任务：D18-A04-R02-F（D18 女性/中性人形资源补齐）  
日期：2026-08-15  
前置：D18-S02-v2 PASS；D18-A04 与 D18-A04-R01 只读核对，未发现 A04-R01 文件。  
结论：**PARTIAL**

## 交付范围

本任务只写入以下任务边界：

- `game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/`
- `docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/`
- `game/tools/results/D18/D18-A04-R02-F/`
- 本交接和当日记忆日志。

未修改其它 Candidate 子目录、全局准入目录、D20 文件、Prefab、场景、Systems、Registry、存档、contracts/server/web 或 Git 历史。

## 来源与许可证

| 项目 | 证据 |
|---|---|
| 官方资源页 | [Quaternius Animated Women Pack](https://quaternius.com/packs/animatedwomen.html) |
| 官方下载入口 | [Quaternius Google Drive folder](https://drive.google.com/drive/folders/1c13R--fMqdR6r2MRlcKKsbPky0__T-yJ?usp=sharing) |
| 实际字节下载 | [Poly Pizza direct archive](https://static.poly.pizza/list/HHSKxnk1mY-zip--1745939261.zip)，页面归属 Quaternius 并列出同一四个模型 |
| 许可证 | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)；[Quaternius FAQ](https://quaternius.com/faq.html) |
| 下载日期 | 2026-08-15 |
| 原始文件名 | `Animated-Women-Pack-HHSKxnk1mY-zip--1745939261.zip` |
| 原包大小 | 1,783,003 bytes |
| 原包 SHA-256 | `BCC6170D0175388FDF9AC7A3FC1E59B9250178E913B94B0D374B4795A1311C9F` |
| 许可证快照 | `game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/LICENSE_EVIDENCE.md` |

原包存放于 `.gitignore` 覆盖的 `artifacts/external_sources/` 根缓存，不进入普通 Git 历史。

## 模型与动作数量

- 独立女性人形源模型：4；没有按换色重复计数。
- 候选 FBX：4；源文件分别为 `Female_Alternative.fbx`、`Female_Casual.fbx`、`Female_Dress.fbx`、`Female_TankTop.fbx`。
- 每个 FBX 静态证据：1 Geometry、1 Mesh model、42 LimbNode、32 Deformer、11 AnimationStack、11 AnimationLayer、3,168 AnimationCurve。
- 源动画栈合计：44（11 × 4），不是 Unity `AnimationClip` 数量。
- 动作标签：Clapping、Death、Idle、Jump、Punch、Run、RunningJump、Sitting、Standing、SwordSlash、Walk。
- 材质节点：每文件 5–11 个；存在 Skin/Eyes/Hair 或 HairBase 与若干服装材质名。
- FBX 静态导入解析错误：0/4。
- Unity import verified 文件：0/4；不是模型数量为 0，而是本机没有 Unity 导入环境。

逐文件哈希和结构结果见：

- `game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_FBX_STATIC_AUDIT.json`
- `docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/D18_A04_R02_F_PACKET.md`

## Manifest 与准入状态

机器 manifest：`docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/d18_a04_r02_f.manifest.json`。

- `review_status=Candidate`
- `import_status=NotImported`
- `runtime_consumers=[]`
- 没有 `intake_eligible`、`LocalDevelopmentReady` 或 `ReleaseApproved`
- `file_count=4`
- `unity_object_count=0`：表示本机未测得 Unity 对象数量，不是对源 FBX 为空的判断。

## 验证结果

| 验证 | 实际结果 |
|---|---|
| Manifest schema/审计 | PASS；0 issue |
| 原包 SHA-256 | PASS；与记录一致 |
| FBX 静态结构审计 | PASS；4/4 文件可读取几何、骨架、材质节点和动画栈 |
| Unity 导入与 Console | NOT RUN；未发现 Unity Editor executable |
| D20 Prefab/Animator/场景引用 | NOT RUN；按任务边界禁止 |
| `git diff --check` | 交接前执行；结果记录在最终回复和验证文件 |

已执行命令和无 Unity 环境的可复现命令见 `game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_VALIDATION.md`。

## 适配边界与未完成项

已补足：D18-A04 的“没有女性人形基体”缺口；可作为 D20-A01 第二外观方向的 Candidate 评估输入。

未补足：

1. 未证明 1675 年加勒比服装/文化适配；Casual、TankTop、Dress、Alternative 均需时代审查和可能的改装。
2. 未证明六个独立发型槽；当前只有 Hair/HairBase 材质名，单网格结构没有独立发型网格证据。
3. 未提供帽子、外套、鞋、手套、饰品、职业装备的 D10 outfit/asset 组合数据。
4. 未验证 D20 精确骨架映射、Humanoid Avatar、Y-up/单位、脚底、bounds、Read/Write、材质提取与动画片段设置。
5. 没有 D20 玩家 Prefab、正式 Animator、场景引用或运行时替换；后续必须由 D20 任务和 D18-S02-R01 处理。

## 责任模块与后续建议

- D18-A04-R02-F：已交付候选来源、许可证证据、哈希、隔离 manifest 和静态审计；Unity 导入未完成是环境阻塞，不升级状态。
- D18-S02-R01：复核官方来源链、许可证和隔离 manifest，决定是否准入；不得直接引用本包为 ReleaseApproved。
- D20-A01：在准入后完成 Avatar/骨架映射、尺度和第二外观方向验证；重新做时代服装、发型和材质组合评估。

最终状态：**PARTIAL**，原因是 Unity Editor 不可用导致导入验收未完成；来源、许可证、哈希、真实模型数和静态结构均有证据。
