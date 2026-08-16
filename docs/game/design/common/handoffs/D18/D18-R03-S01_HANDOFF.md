# D18-R03-S01 交接报告

## 结论

- 任务：`D18-R03-S01`
- 结论：`PARTIAL`
- 含义：本轮只读复审证据真实、完整且可追溯；角色资源目标仍有缺口，不能晋级 LocalDevelopmentReady，也不能宣称运行时可用。

## 复审范围与输入

- A01 packet：`docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json`
- Unity 隔离导入：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json`
- A01 机器结果：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT.json`
- 官方来源：`https://quaternius.com/packs/universalbasecharacters.html`
- 许可证证据：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt`
- 归档：`artifacts/external_sources/D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip`

## 真实审计结果

- 复审候选 33 个；本地文件存在 33/33；SHA-256 匹配 33/33；Unity 导入记录 33/33；导入错误 0。
- 有效 Humanoid 基础 2 个（女性和男性各 1 个）；两者均为有效 human Avatar。
- 独立发型 5 个；眉毛和胡须未计入发型。
- 完整服装候选文件 4 个，服装模块文件 24 个；没有完成 8 套 1675 年加勒比时代组合的历史审查、装配和 Player 证据，已装配组合计 0/8。
- 独立动作片段候选计 0；NPC、野生动物和宠物不在本 packet 范围内，不能用玩家模型冒充这些类别。
- 正式 Prefab/场景运行时消费者 0；LocalDevelopmentReady 0；ReleaseApproved 0。

逐项审计包：

- `docs/game/design/common/assets/review_packets/D18-R03-S01/D18_R03_S01_CHARACTER_APPEARANCE_REVIEW.json`
- `docs/game/design/common/assets/review_packets/D18-R03-S01/D18_R03_S01_CHARACTER_APPEARANCE_REVIEW.md`
- `game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT.json`

## 消费结论

| 类别 | 结论 | 说明 |
|---|---|---|
| `player_visual_core` | PARTIAL / 可推荐受控工作 Prefab | 2 个 Humanoid 基础、5 个发型和 4 个完整服装候选文件有文件/哈希/许可证/导入证据，但仍是 Candidate。 |
| `animation_core` | OPEN | 本 packet 没有独立动作片段证据；沿用 D20-A02 的项目自有动作层，需单独复核。 |
| `npc_visual_core` | OUT_OF_SCOPE | 本轮没有审计 NPC 候选。 |
| `wildlife_core` | OUT_OF_SCOPE | 本轮没有审计野生动物候选。 |
| `pet_core` | OUT_OF_SCOPE | 本轮没有审计宠物候选。 |
| 中性人形 | GAP | 男/女基础存在，但没有中性身体方向证据。 |
| 第 6 个独立发型 | GAP | 未发现可复核的第 6 个发型；不以胡须/眉毛凑数。 |
| 8 套时代服装 | GAP | Fantasy/Medieval 候选不能自动证明 1675 年加勒比适配。 |

## 边界与后续

- 本复审未修改 `d18_prefab_candidate_intake_catalog.json`、manifest、Registry、Prefab、场景、Systems、存档或 contracts。
- 所有资源仍保持 Candidate/隔离证据边界；不写入 LocalDevelopmentReady 或 ReleaseApproved。
- `D20-A01-R02` 可以在其授权的工作 Prefab范围内消费这份复审中推荐的真实候选，但必须补齐装配和 Player 证据。
- `D18-S03` 仍是正式运行时消费者和 LocalDevelopmentReady 晋级门禁，当前不能提前执行。

## 验证命令

```text
python tools/d18_r03_s01_review.py
python -m json.tool game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT.json
python -m json.tool docs/game/design/common/assets/review_packets/D18-R03-S01/D18_R03_S01_CHARACTER_APPEARANCE_REVIEW.json
git diff --check
```

`D18-R03-S01` 不能写 PASS，因为发型、时代服装组合和运行时消费者的硬条件仍未满足；复审本身没有发现文件、哈希、许可证或隔离导入证据错误。

## RERUN1 追加交接（2026-08-16）

上面的 `PARTIAL` 是历史复审结果，保留不覆盖。本轮重新处理了隔离工作台证据，结论为 **PASS（仅限隔离资源消费门禁）**：

- 源候选 33/33 文件存在，33/33 SHA-256 匹配，33/33 Unity 导入记录存在，导入错误 0；许可证证据和归档 SHA-256 均匹配。
- 新 Unity 工作台审计：2 个有效 Human Avatar 基体、6 个独立发型、8 个几何服装组合，12 个生成资产全部存在；`D18_R03_A01_EDITOR_AUDIT_RERUN1.json` 的错误数为 0。
- `player_visual_core` 允许 D20-A01-R02 在授权隔离工作 Prefab 范围内消费；`npc_visual_core`、`wildlife_core`、`pet_core` 保持 OUT_OF_SCOPE；`animation_core` 保持 OPEN。
- 中性人形、NPC/动物/宠物、独立动作片段仍没有本包证据；服装历史适配保持 `PROVISIONAL_LOCAL_VISUAL`。
- 本轮不修改全局 catalog、候选状态、正式场景或运行时消费者，`LocalPlayableReady=0`、`LocalDevelopmentReady=0`、`ReleaseApproved=0`。

RERUN1 机器结果：`game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT_RERUN1.json`。逐项复审：`docs/game/design/common/assets/review_packets/D18-R03-S01/D18_R03_S01_CHARACTER_APPEARANCE_REVIEW_RERUN1.json`。本结论允许继续 D20-A01-R02 的隔离外观工作，不等于 D18-S03 运行时晋级。
