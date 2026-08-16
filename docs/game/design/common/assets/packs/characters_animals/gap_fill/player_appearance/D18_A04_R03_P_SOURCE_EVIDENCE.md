# D18-A04-R03-P 来源与许可证证据

核对日期：2026-08-15（Asia/Shanghai）  
证据范围：本任务隔离 Candidate；不等于 `PrefabCandidateIntake`、`LocalDevelopmentReady` 或 `ReleaseApproved`。

## 官方来源

| 包 | 官方资源页 | 官方免费分发页 | 包内许可证 | 本地归档 SHA-256 |
|---|---|---|---|---|
| Universal Base Characters | <https://quaternius.com/packs/universalbasecharacters.html> | <https://quaternius.itch.io/universal-base-characters> | `License_Standard.txt`，CC0 1.0 | `FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40` |
| Modular Character Outfits - Fantasy | <https://quaternius.com/packs/modularcharacteroutfitsfantasy.html> | <https://quaternius.itch.io/modular-character-outfits-fantasy> | `License_Standard.txt`，CC0 1.0 | `C3468B18871CC8C8F05AB14DF7712BAF22CB9F389CBD870BABF130E595187F70` |

许可证原文链接：<https://creativecommons.org/publicdomain/zero/1.0/>。两份官方页面均在本次核对时列出 CC0、商业使用和修改许可；免费标准包与付费 Source 包分开，本任务只下载标准包，没有绕过登录、付款或限流。

## 下载与缓存

- `D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip`：128,968,391 bytes；下载时间 `2026-08-15T05:04:45.6554860Z`。
- `D18-A04-R03-P_Quaternius_ModularCharacterOutfitsFantasy_Standard.zip`：294,347,394 bytes；下载时间 `2026-08-15T05:06:05.1786878Z`。
- 原始归档只保留在 `artifacts/external_sources/` 本地缓存；项目 `.gitignore` 已覆盖该目录中的压缩归档，未把 402 MB 压缩包写入普通 Git 历史。
- 包内许可证分别复制到 Candidate 目录：
  - `game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt`
  - `game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusModularCharacterOutfitsFantasy/LICENSE_QUATERNIUS_CC0_STANDARD.txt`

## 实际 Candidate 内容

- 32 个真实不同 FBX：1 个女性完整基体、7 个头发/眉毛相关网格、12 个女性模块/完整服装、12 个男性参考模块/完整服装；没有按颜色变体重复计数。
- 47 张随标准包提供的 PNG 贴图被保留并登记；未保留 Godot/Unreal 示例工程、付费 Source 版本或可执行文件。
- 许可证文件和 README 均保留；原始文件名、包内相对路径、候选路径和 SHA-256 在机器 JSON 中逐项记录。

## 证据边界

- 隔离 Unity 6000.0.80f1 导入：32/32 有网格和 Renderer，1/32（`Superhero_Female_FullBody.fbx`）实际为 `Animation Type=Human`、Avatar 存在、`isValid=true`、`isHuman=true`；其隔离 bounds 高度为 1.8537368 m，D20 仍需按 D17 的 1.80 m ± 0.03 m 标准校准。
- 头发与服装 FBX 是 `Generic/NoAvatar`，可作为外观模块候选，不可单独作为 Humanoid。头发相关几何 6 个中，5 个是头发造型，1 个是面部胡须；眉毛不计为发型。
- 没有建立 D20 Prefab、Animator、骨骼挂接、碰撞、LOD、正式场景、运行时消费者、Registry、存档或资源状态更新。贴图绑定、遮挡/穿插和 1675 年加勒比服装适配留给 D20 与人工历史审查。
- 原 D18-A04-R02-F 的四个女性 FBX 仍保持原 Candidate/NotImported 历史，不被本包覆盖或改写。

逐项证据见：`D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json`、`game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json`。
