# D18-A04-R02-F-R01 女性/中性人形 Humanoid/Avatar 复审包

任务编号：`D18-A04-R02-F-R01`  
日期：2026-08-15  
结论：**PARTIAL**

本轮使用用户已授权的电脑独占窗口，在 Unity `6000.0.80f1` 隔离 fixture 中真实导入历史 D18-A04-R02-F 的四个 FBX。没有改写原 Candidate、全局 intake catalog、D20 Prefab、Animator、正式场景、Registry、存档或 Systems。

## 真实结果

| 检查 | 结果 |
|---|---:|
| 本地候选文件 | 4/4 存在、SHA-256 与历史包一致 |
| Unity 导入 | 4/4 有网格和 Renderer |
| Avatar 对象 | 4/4 存在、`isValid=true` |
| `isHuman=true` | 0/4 |
| Humanoid + valid human Avatar | 0/4 |
| Unity 导入错误 | 0 个脚本错误；FBX Rig 日志报告缺少/无效 `LeftFoot` |

四个候选的逐项哈希、Animation Type、Avatar、骨骼、网格、材质、bounds 和失败字段见：

`D18_A04_R02_F_R01_HUMANOID_AVATAR_PACKET.json` 以及 `game/tools/results/D18/D18-A04-R02-F-R01/D18_A04_R02_F_R01_ISOLATED_UNITY_IMPORT.json`。

## 失败原因与边界

- Importer 可创建 Avatar 对象，但 Unity 判定它们不是人形 Avatar；历史 FBX 骨架有 25 个 HumanBone/45 个 SkeletonBone，但没有有效的左右脚映射。
- `Animation Type=Human` 不能单独证明 Humanoid；本任务按 `avatar_is_human` 实测字段裁决，不把“有 Avatar”误报成“玩家可用”。
- 源文件 bounds 约 0.047 m 是该 FBX 的局部单位表现，不是玩家高度；不能据此校准 D17 的 1.8 m。
- 结论不升级任何资源状态，`LocalDevelopmentReady=0`、`ReleaseApproved=0` 继续保持。

## 后续责任

1. D18-S02-R02 必须把四项逐一登记为“已导入但非人形 Avatar”，不得将其作为 D20 玩家核心。
2. D18-A04-R03-P 已补充了一个真实 `Humanoid + valid Avatar` 的女性外观候选；D18-S02-R02 应将该证据与 R03 包一并审计，但仍要保留 D17 高度、时代服装和 D20 工作 Prefab 缺口。
3. D20-A01-R01 只能在 D18 R02 消费门禁读取到合法 Humanoid+valid Avatar 后开始；不得使用本包的四个文件绕过门禁。

## 证据路径

- 历史来源/许可证：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/LICENSE_EVIDENCE.md`
- 历史 manifest：`docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/d18_a04_r02_f.manifest.json`
- 原始 Unity 导入 JSON：`game/tools/results/D18/D18-A04-R02-F-R01/D18_A04_R02_F_R01_ISOLATED_UNITY_IMPORT.json`
- 原始 Unity 导入日志：`game/tools/results/D18/D18-A04-R02-F-R01/D18_A04_R02_F_R01_ISOLATED_UNITY_IMPORT.log`
- 初次主工程编译失败日志（保留）：`game/tools/results/D18/D18-A04-R02-F-R01/D18_A04_R02_F_R01_UNITY_IMPORT.log`
