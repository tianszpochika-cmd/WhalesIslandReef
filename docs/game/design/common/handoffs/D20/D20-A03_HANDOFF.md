# D20-A03 交接报告：NPC 视觉阵容

任务：D20-A03
日期：2026-08-15
结论：`PARTIAL`

## 前置与范围

- D20-S01：`PASS`。
- D18-S02-R01：`PASS`；最新 `d18_prefab_candidate_intake_catalog.json` 的 `npc_visual_core` 有 4 条 `intake_eligible`，本批使用其中 3 条。
- 只写入 NPC Prefab、NPC 模型/材质、对应测试、D20 文档和 D20 机器结果。
- 未修改 NPCSystem、生成器、对话、身份/阵营/关系、存档、Registry、InitialHarbor 或运行时场景。
- `Male_Suit.fbx` 按 D18 审查拒绝，未使用。资源状态保持 `Candidate`，没有提前晋级。

## 交付物

| 类型 | 路径 | 真实数量/说明 |
| --- | --- | --- |
| NPC Prefab | `game/Assets/Prefabs/Characters/D20/NPC/` | 12 个 `.prefab`、12 个 `.meta` |
| NPC 材质 | `game/Assets/Art/Models/Characters/D20/NPC/Materials/` | 16 个 `.mat`、16 个 `.meta` |
| 只读接口 | `game/tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json` | D20-A04 唯一读取入口 |
| 构建报告 | `game/tools/results/D20/D20-A03/D20_A03_PREFAB_BUILD_REPORT.json` | Unity 6000.0.80f1，12 Prefab，3 独立源模型 |
| 主工程测试 | `game/tools/results/D20/D20-A03/D20_A03_UNITY_TEST_RESULTS.xml` | 3/3 通过 |
| 导入审计 | `game/tools/results/D20/D20-A03/D20_A03_MAIN_IMPORT_AUDIT.json` | 0 材质/Shader/碰撞/LOD/Animator/锚点失败 |
| 联系表 | `game/tools/results/D20/D20-A03/D20_A03_NPC_CONTACT_SHEET.md` | 12 个岗位视觉组合 |
| 设计/阵容文档 | `docs/game/design/common/D20_NPC_VISUAL_ROSTER.md` | 外观、复用、时代审查和边界 |

临时 Unity 构建工程及其 Library/缓存已从任务结果中清理；保留构建器日志和最终 JSON 证据。机器结果目录增量约 0.3 MB（不含主工程已有 Library）。

## NPC 字段与外观阵容

下表列出 `npc_id`、`resource_key`、外观和复用关系。颜色、头饰和道具只用于视觉识别，不是岗位、阵营、阶级、关系或法律数值写者。

| npc_id | resource_key | 岗位/轮廓 | 外观组合 | 源模型/复用 | 时代/审查 |
| --- | --- | --- | --- | --- | --- |
| `npc_d15_a07_instance_0001` | `npc_d20_deckhand_01` | deckhand | ochre、暖肤、草帽、绳圈 | Male_Shirt；与 rigger/fisher 复用 | 工作服候选 |
| `npc_d15_a07_instance_0002` | `npc_d20_rigger_01` | rigger | sea-green、中肤、头巾、绳圈 | Male_Shirt；与 deckhand/fisher 复用 | 工作服候选 |
| `npc_d15_a07_instance_0008` | `npc_d20_shore_trader_01` | shore trader | navy、暖肤、宽檐帽、账本 | Male_LongSleeve；与 trader-02/船坞工复用 | 商人轮廓需审查 |
| `npc_d15_a07_instance_0028` | `npc_d20_shore_trader_02` | shore trader | burgundy、深肤、宽檐帽、账本 | Male_LongSleeve；颜色变体 | 商人轮廓需审查 |
| `npc_d15_a07_instance_0006` | `npc_d20_dock_clerk_01` | dock clerk | russet、中肤、草帽、码头钩 | Male_Casual；与居民/酒馆复用 | 工作服候选 |
| `npc_d15_a07_instance_0020` | `npc_d20_net_mender_01` | net mender | cream、深肤、头巾、网针 | Male_Casual；与 dock/cook/resident 复用 | 渔业服装候选 |
| `npc_d15_a07_instance_0004` | `npc_d20_fisher_01` | fisher | sea-green、深肤、草帽、鱼竿 | Male_Shirt；与 deckhand/rigger 复用 | 工作服候选 |
| `npc_d15_a07_instance_0010` | `npc_d20_tavern_cook_01` | tavern cook | burgundy、暖肤、头巾、托盘 | Male_Casual；仅视觉道具 | 酒馆岗位候选 |
| `npc_d15_a07_instance_0005` | `npc_d20_boatbuilder_01` | boatbuilder | ochre、中肤、头巾、木槌 | Male_LongSleeve；与 sailmaker/watch 复用 | 船坞工作服候选 |
| `npc_d15_a07_instance_0014` | `npc_d20_sailmaker_01` | sailmaker | cream、深肤、草帽、帆针 | Male_LongSleeve；与 boatbuilder 复用 | 船坞工作服候选 |
| `npc_d15_a07_instance_0018` | `npc_d20_harbor_watch_proxy` | harbor watch/official proxy | indigo、深肤、宽檐帽、手杖 | Male_LongSleeve；代理缺口 | 身份必须人工历史审查 |
| `npc_d15_a07_instance_0009` | `npc_d20_common_resident_01` | common resident | russet、暖肤、无帽、篮子 | Male_Casual；与 dock/net/cook 复用 | 居民候选 |

复用统计：`crew_quaternius_male_casual_v01` 4 个、`crew_quaternius_male_long_sleeve_v01` 5 个、`crew_quaternius_male_shirt_v01` 3 个；独立网格源模型 3 个，Prefab 视觉组合 12 个。

## D20 对接边界

- 坐标遵守 D20：1 unit = 1m、Y-up、+Z forward、root scale = 1；`Visual` 子节点缩放到约 1.8m 目标高度。
- 根节点包含约 1.8m CapsuleCollider、Animator（Root Motion 关闭）、LODGroup，以及 `Head`、`Hand_R`、`Carry` 三个锚点。
- 源 FBX 为 `Generic/NoAvatar`。没有声称 Humanoid Avatar、骨骼重定向或正式动作兼容；D20-A04 必须把这一点视为接口缺口。
- D18 源动作确认有 Idle/Walk/Run/Jump/Death；没有把 Interact 或 `Man_Standing` 伪报为正式交互动作。缺动作时使用稳定姿态 fallback，视觉包不触发行为事件。
- LOD 参考为 0–12m 近距、12–32m 中距、32–80m FarProxy、80m 以上裁剪；FarProxy 无碰撞、无交互语义。
- 颜色、肤色、头饰、工具和轮廓不能由 UI 或 AgentReply 写入，也不能成为 faction/relation/identity 的强逻辑裁决。

## 测试与结果

测试命令（Unity 6000.0.80f1，主工程）：

```text
Unity -batchmode -nographics -disable-assembly-updater -projectPath game -runTests -testPlatform editmode -assemblyNames D20.Wildlife.Tests -testFilter D20A03 -testResults game/tools/results/D20/D20-A03/D20_A03_UNITY_TEST_RESULTS.xml
```

结果：`Passed`，3/3 通过：

1. 12 个稳定 Prefab key 存在；
2. 每个 Prefab 有 Visual、Renderer、根碰撞、LODGroup、Animator、Head/Hand_R/Carry 锚点、非空材质 shader 和非零 bounds；
3. 只读接口存在，包含 `intake_eligible` 边界、3 个允许源模型，并明确排除 `Male_Suit.fbx`。

主工程导入审计为 0 个丢材质、丢 Shader、粉色材质、非零 bounds、根碰撞、LOD、Animator 或锚点失败。Unity 日志中存在环境性的 Licensing access-token 警告和既有 Cinemachine partial-class 警告，但没有本任务新增脚本编译错误；详细记录见 `unity_d20_a03_main.log`。

## 已知缺口与后续责任

1. 三个源模型均为 Generic/NoAvatar，Humanoid 骨架、Avatar、重定向和正式动作必须由后续 D20 验收/资源晋级处理。
2. 12 个条目不是 12 个独立人物网格；当前是三套基础模型加材质、头饰和工具模块，不能夸大独立模型数量。
3. 没有专门的历史守望/官员模型；`npc_d20_harbor_watch_proxy` 只能作为待审视觉代理。
4. 没有女性或完成 1675 服装审查的人物基底；不得用缩放或改色冒充缺口。
5. 没有场景摆放、NPC 生成、行为、对话、身份、阵营、关系或保存接入；正式使用需等待 D18-S03 准入及 D20 后续任务。

最终交接结论：`PARTIAL`。可交付项是 12 个 Candidate 视觉 Prefab 与可复核接口；不可交付项是 Humanoid/历史定稿和运行时接入。
