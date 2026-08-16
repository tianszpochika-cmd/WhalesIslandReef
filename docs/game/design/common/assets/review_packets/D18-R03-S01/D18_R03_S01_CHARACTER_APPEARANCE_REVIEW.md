# D18-R03-S01 角色外观资源复审

结论：**PARTIAL**。本报告只审计 D18-R03-A01 已存在的本地候选和隔离 Unity 导入证据，不修改全局准入目录或资源状态。

## 证据范围

- A01 packet：`docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json`
- Unity 导入：`game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json`
- 官方来源：https://quaternius.com/packs/universalbasecharacters.html
- 许可证：`CC0-1.0`，证据：`game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt`
- 归档 SHA-256：记录 `FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40`，实际 `FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40`

## 实际计数

- 复审候选：33；本地文件存在：33；哈希匹配：33。
- Unity 导入记录：33；有效 Humanoid：2；独立发型：5；服装模块文件：24（完整服装候选文件：4）。
- 已装配且完成时代审查的 1675 年服装组合：0/8；正式运行时消费者：0；LocalDevelopmentReady：0；ReleaseApproved：0。

## 消费结论

- `player_visual_core`：可推荐用于受控工作 Prefab 的 2 个 Humanoid 基础和 5 个发型/4 个服装模块，但仍是 Candidate 证据；不能写成 LocalDevelopmentReady。
- `animation_core`：本包没有独立动作片段证据；D20-A02 的项目自有动作层需单独复核。
- `npc_visual_core`、`wildlife_core`、`pet_core`：本包不覆盖，明确 OUT_OF_SCOPE，不用玩家资源冒充。
- 女性/中性人形：女性与男性各一个有效基础已证实；中性方向没有证据，不宣称已覆盖。
- 第 6 个独立发型：缺证据；眉毛和胡须未计入发型。
- 8 套时代服装：缺真实历史审查/装配证据；Fantasy/Medieval 资产名称不能作为年代证明。

## 状态边界

本复审没有修改全局 `d18_prefab_candidate_intake_catalog.json`、Candidate 状态、Prefab、场景、Registry、Systems、存档或 contracts；所有候选的 `local_development_ready=false`、`release_approved=false` 保持不变。

机器结果：`game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT.json`。完整逐项记录见同目录 JSON。
