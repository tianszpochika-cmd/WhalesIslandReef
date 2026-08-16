# D18-R03-S01 RERUN1 角色外观资源消费门禁

结论：**PASS**。本轮只读核对历史 A01 源证据和新建隔离工作台，不修改全局准入目录或资源状态。

## 真实输入

- 历史 A01 packet：`docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json`
- 隔离 Unity 导入：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json`
- RERUN1 工作台审计：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_EDITOR_AUDIT_RERUN1.json`
- 许可证证据：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt`；归档 SHA 记录/实际：`FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40` / `FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40`

## 计数

- 源候选记录：33；文件存在：33；哈希通过：33。
- Unity 导入记录：33；有效 Humanoid：2；独立发型：6；装配组合：8。
- 新生成工作台资产存在：12；运行时消费者：0；LocalPlayableReady：0；LocalDevelopmentReady：0；ReleaseApproved：0。

## 门禁结论

- `player_visual_core`：2 个全身方向、6 个发型、8 个装配变体可用于指定 D20 隔离工作 Prefab；这不是正式运行时准入。
- `npc_visual_core`、`wildlife_core`、`pet_core`：本包不覆盖，保持空，不用玩家资源冒充。
- `animation_core`：保持 OPEN，由 D20 动作任务负责。
- `female_or_neutral_human_gap`：女性已验证；中性方向无证据，不宣称覆盖。
- 服装的 1675 年历史审查保持 `PROVISIONAL_LOCAL_VISUAL`。

## 后续边界

D20-A01-R02 可开始隔离工作 Prefab；D20-A02-R02 需等待外观 Prefab 的真实动作检查；D18-S03 仍是运行时消费者、碰撞、LOD、保存恢复和 LocalDevelopmentReady 的唯一晋级门禁。

错误数：0。机器结果：`game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT_RERUN1.json`。
