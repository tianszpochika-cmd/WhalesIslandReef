# D18-R03-A01 交接报告

## 任务与结论

- 任务编号：`D18-R03-A01`
- 结论：`PARTIAL`
- 范围：角色外观候选补充与隔离 Unity 导入证据；未创建正式 Prefab、场景消费者或运行时接入。
- 历史结果：R02/R03-P 报告和机器结果均保留，本报告不覆盖任何历史证据。

## 实际修改

1. 从已审计的 Quaternius Universal Base Characters Standard CC0 归档中提取男性基础模型：
   `game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/Models/Base/Superhero_Male_FullBody.fbx`
2. 新建隔离导入 fixture：
   `game/tools/results/D18/D18-R03-A01/UnityAppearanceFixture/`
3. 新增只读审计工具：
   `tools/d18_r03_a01_resource_audit.py`
4. 未修改全局 intake catalog、Registry、Prefab、场景、Systems、存档、contracts、server 或 web。

## 真实证据

- 官方来源页：`https://quaternius.com/packs/universalbasecharacters.html`
- 下载页：`https://quaternius.itch.io/universal-base-characters`
- 许可证：CC0-1.0；证据文件：
  `game/Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/QuaterniusUniversalBaseCharacters/LICENSE_QUATERNIUS_CC0_STANDARD.txt`
- 使用的已审计归档：
  `artifacts/external_sources/D18-A04-R03-P_Quaternius_UniversalBaseCharacters_Standard.zip`
- 归档 SHA-256：`FDBF1804C90DFC1EA03E992BFF7DA2DFD1A79318E13270A660180F9308455F40`
- 隔离导入结果：
  `game/tools/results/D18/D18-R03-A01/D18_R03_A01_ISOLATED_UNITY_IMPORT.json`
- 审计机器结果：
  `game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT.json`
- 资源包：
  `docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/D18_R03_A01_PLAYER_APPEARANCE_PACKET.json`

实际统计：

- 33 个 FBX 被隔离导入；33/33 有网格和 Renderer；无导入错误。
- 2 个有效 `Humanoid + human Avatar`：女性 `1.8537367582m`，男性 `1.9155801535m`。
- 5 个独立发型网格：`Hair_Buns`、`Hair_Buzzed`、`Hair_BuzzedFemale`、`Hair_Long`、`Hair_SimpleParted`。
- 4 个完整服装候选文件：Female/Male Peasant、Female/Male Ranger；均仍为 Generic/NoAvatar 模块候选。
- 0 个正式 Prefab、0 个运行时消费者、0 个 LocalPlayableReady、0 个 LocalDevelopmentReady、0 个 ReleaseApproved。

## 未完成与责任

- 第 6 个独立发型尚未有真实本地/导入证据；眉毛和胡须不计为发型。
- 8 套 1675 年加勒比时代服装组合尚未完成历史审查和真实装配证据；Fantasy/Medieval 名称不能自动视为时代适配。
- 没有正式运行时消费者，因此不得推进 D18-S03 或把候选写成 LocalDevelopmentReady。
- 下一责任：`D18-R03-S01` 进行只读复审与消费门禁；外观正式装配属于 `D20-A01-R02`，不得在本任务越权完成。

## 执行命令

```text
Unity.exe -batchmode -nographics -quit -projectPath game/tools/results/D18/D18-R03-A01/UnityAppearanceFixture -executeMethod D18R03A01IsolatedImportRunner.Run -logFile game/tools/results/D18/D18-R03-A01/D18_R03_A01_UNITY_IMPORT.log
python tools/d18_r03_a01_resource_audit.py
git diff --check
```

Unity 隔离导入真实完成；Python 审计结果写入机器结果和 packet；本任务没有正式 Player 路线或性能验收，故不能写 PASS。

## RERUN1 追加交接（2026-08-16）

针对上面的历史 `PARTIAL` 结果，本轮只在 D18 R03 隔离目录完成了资源/工作台补齐与重新审计。历史段落、原始机器结果和原始 packet 均保留，不被覆盖。

- 本轮结论：`PASS`，限定为“隔离资源完成 / 工作台证据”。
- 2 个独立全身方向均有已审计源 FBX，并由 Unity `AvatarBuilder` 生成真实有效的项目自有 Human Avatar 资产；`isValid=true`、`isHuman=true`。
- 5 个已审计发型加 1 个项目自有 `Hair_TwistPonytail` 几何模块，共 6 个；项目自有模块没有外部许可证冒充。
- 8 个真实几何服装组合 Prefab 完成导入审计，均有网格/材质且没有运行时 MonoBehaviour。
- 工作台、发型 Prefab、8 个组合 Prefab 和两个 Avatar 的路径、SHA-256 与审计结果见：
  `game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT_RERUN1.json`。
- `LocalPlayableReady=0`、`LocalDevelopmentReady=0`、`ReleaseApproved=0`、正式场景消费者 `0`、运行时消费者 `0`；本轮不执行资源晋级，也不宣称正式 Player 可用。
- 服装历史适配仍为 `PROVISIONAL_LOCAL_VISUAL`；D20-A01-R02 负责真实 Player 外观校准，D18-R03-S01 负责只读消费门禁。

本轮 Unity 命令：

```text
Unity.exe -batchmode -quit -projectPath game -executeMethod WhalesIslandReef.Editor.D18R03A01.D18R03A01AppearanceBuilder.BuildFromMenu -logFile game/tools/results/D18/D18-R03-A01/D18_R03_A01_BUILD_RERUN1D.log
git diff --check
```

命令以 Unity batchmode exit code 0 完成，机器审计为 PASS；这不替代后续真实 Player、动作、保存或 D18-S03 验证。
