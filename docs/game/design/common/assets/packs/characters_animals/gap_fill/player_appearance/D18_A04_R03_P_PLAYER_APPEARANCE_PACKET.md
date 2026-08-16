# D18-A04-R03-P 玩家发型与 1675 服装候选包

任务编号：`D18-A04-R03-P`  
日期：2026-08-15  
结论：**PARTIAL（隔离候选来源、哈希与 Unity 导入证据完整，但目标覆盖存在真实缺口）**

本结论表示候选包的来源、哈希和隔离导入证据完整，但“至少六个独立发型槽位”和“至少八个时代合理服装组合”未被本批真实资源证明；不表示已进入全局准入目录、D20 Prefab、正式场景或运行时。`LocalDevelopmentReady=0`、`ReleaseApproved=0` 保持不变。

## 实际交付

| 项目 | 真实数量 | 证据 |
|---|---:|---|
| Candidate FBX | 32 | `D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json` |
| Candidate PNG | 47 | Candidate 目录扫描与 packet JSON |
| Unity 隔离导入网格/Renderer | 32/32 | Unity 6000.0.80f1 日志和 JSON |
| Humanoid + valid Avatar | 1 | `Superhero_Female_FullBody.fbx`，Human、Avatar valid、isHuman |
| 独立头发相关几何 | 6 | 5 个独立头发造型 + 1 个胡须；眉毛单列，不计入发型 |
| 女性服装模块/完整服装 | 12 | Female Peasant/Ranger 的模块与整套 FBX |
| 女性完整服装 | 2 | `Female_Peasant.fbx`、`Female_Ranger.fbx` |
| D20 Prefab/正式场景/运行时消费者 | 0 | 本任务禁止创建 |

## 来源与许可

- Universal Base Characters：<https://quaternius.com/packs/universalbasecharacters.html>；免费标准包：<https://quaternius.itch.io/universal-base-characters>。
- Modular Character Outfits - Fantasy：<https://quaternius.com/packs/modularcharacteroutfitsfantasy.html>；免费标准包：<https://quaternius.itch.io/modular-character-outfits-fantasy>。
- 两包包内 `License_Standard.txt` 为 CC0 证据；许可证原文：<https://creativecommons.org/publicdomain/zero/1.0/>。
- 归档名称、下载日期、归档大小、SHA-256、包内路径和 Candidate 文件哈希逐项见 `D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json`。
- 完整来源说明和许可证快照：`D18_A04_R03_P_SOURCE_EVIDENCE.md`。

## 玩家与外观适配结论

### 玩家核心人形

`Superhero_Female_FullBody.fbx` 是目前 D18 R02 线第一次真实取得的 `Humanoid + valid Avatar` 候选：

- `Animation Type=Human`；`Avatar Setup=CreateFromThisModel`；Avatar 存在，`isValid=true`，`isHuman=true`。
- 隔离 bounds 高度 `1.8537368 m`，高于 D17 的 `1.80 m ± 0.03 m` 视觉基线；D20-A01-R01 必须在工作 Prefab 中校准根节点/Visual scale，并重新验证脚底、胶囊体、落地、镜头和上下船。
- Unity 导入没有生成正式 Animator、Prefab 或场景消费者；本任务不能把它标为 `LocalDevelopmentReady`。

### 发型与饰面

可见的独立网格包括 `Hair_Buns`、`Hair_BuzzedFemale`、`Hair_Long`、`Hair_SimpleParted`、`Hair_Buzzed` 和 `Hair_Beard`。前五项才计为发型造型，第六项只计面部胡须；`Eyebrows_Female` 是眉毛饰面。它们在 Unity 中以 `Generic/NoAvatar` 导入，仍需 D20 使用 Head 锚点完成挂接、遮挡和动画抖动检查。

### 服装

女性 Peasant/Ranger 提供 12 个真实模块/整套 FBX，包含身体、手臂、腿、脚、兜帽/肩甲等。它们是 Medieval/Fantasy 风格候选，不是已经通过历史审查的 1675 年加勒比服装。当前只证明了模块文件可导入和有 Renderer；没有证明八套历史服装组合、骨骼权重、穿插、碰撞、LOD 或 D10 `AppearanceLoadout` 接口。

## 必须保留的缺口

1. 独立女性头发造型只有 5 个，仍缺第 6 个独立发型槽；不能把胡须或眉毛冒充发型。
2. 八套时代合理服装组合尚未由 D20 工作 Prefab 组装和审查；Fantasy/Medieval 资产不能直接宣称 1675 适配。
3. 女性基体高度需按 D17 1.8 m 视觉标准校准；不能直接替换玩家生成入口。
4. 头发/服装模块均无独立 Humanoid Avatar，挂接和 Animator 仍需 D20-A01/A02。
5. 没有碰撞、LOD、运行时消费者、保存/重载、真实 Player 截图或性能证据。

## 验证

- Unity 隔离导入命令使用 Unity 6000.0.80f1 batchmode；结果：32 FBX，32/32 网格/Renderer，1 个 Humanoid+valid Avatar，0 个来源候选文件被修改。
- 原始 Unity 日志：`game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.log`。
- 机器结果：`game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_MACHINE_RESULT.json`。
- `git diff --check` 在 D18 R02 阶段统一执行；本任务交接前再次执行并记录在 handoff。
