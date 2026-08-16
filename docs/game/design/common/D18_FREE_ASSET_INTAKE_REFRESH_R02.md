# D18-S02-R02 免费资源库准入刷新报告

任务：`D18-S02-R02`　日期：2026-08-15　结论：**PASS**

## 结论边界

本轮是对受控工作场景的 `PrefabCandidateIntake` 刷新，不是运行时晋级。旧版 `D18-S02-v2`、`D18-S02-R01` 的历史报告和结果保留；本轮没有覆盖或重写它们。`LocalDevelopmentReady=0`、`ReleaseApproved=0` 仍为真实数量，D18-S03 仍是运行时晋级门禁。

准入字段只证明：本地候选文件、逐文件 SHA-256、CC0 许可证证据、来源归档 SHA-256、manifest、隔离 Unity `ImportVerified` 证据和受控消费方向齐全。它不证明 Prefab、碰撞、锚点、LOD、Animator、运行时消费者、正式 InitialHarbor、保存/重载或 Steam Ready。

## 输入与真实统计

| 项目 | 结果 |
|---|---:|
| 本轮输入 manifest | 30 |
| 候选文件总数 | 1,895 |
| 既有 R01 intake 条目 | 1,833 |
| 本轮新增真实 intake 条目 | 32 |
| R02 输出 intake 条目 | 1,865 |
| intake manifest 数 | 26 |
| 排除 manifest | 4 |
| 重复 manifest/文件引用 | 0 |
| 审计错误 | 0 |
| LocalDevelopmentReady | 0 |
| ReleaseApproved | 0 |

新增 32 条全部来自 Quaternius R03-P 两个包：1 个真实 `Humanoid + valid Avatar` 玩家基体，5 个独立发型，1 个胡须相关几何，眉毛饰面，以及女性/男性模块和整套服装。R03-P 本身仍是 `PARTIAL`：只发现 5 个独立发型而非目标 6 个，且 1675 年加勒比时代服装组合未被 Fantasy 包证明。

D18-A04-R02-F-R01 的 4 个历史女性 FBX 均实际导入生成 Avatar 对象，但 `isHuman=0/4`，缺 `LeftFoot`/`RightFoot`，故 0 条进入玩家 Humanoid intake。

## 类别消费门禁

| 类别 | 当前受控 intake | 允许工作 | 未解决 |
|---|---:|---|---|
| `player_visual_core` | 33（含旧 1 条与 R03-P 32 条） | D20-A01-R01、D20-A03 工作 Prefab | D17 尺度校准、Animator、碰撞、LOD、正式消费者 |
| `player_appearance_module` | 32 | D20-A01-R01、D20-A03 | 5 发型不足目标 6；服装时代审查、骨骼挂接、穿插、LOD |
| `female_or_neutral_human_gap` | 1 个有效女性 Humanoid 候选 | D20-A01-R01 受控工作 Prefab | 4 个历史女性文件不可用；1675 服装和 1.8m 视觉基线未闭合 |
| `npc_visual_core` | 保持既有 | D20-A03/A04 | 专门女性/历史角色基底仍未证明 |
| `wildlife_core` | 保持既有 | D20-A05 | 运行时/LOD/碰撞仍待 D20 与 D18-S03 |
| `pet_core` | 保持既有 | D20-A06 | 宠物运行时和正式场景仍未证明 |
| `port_structure_core` | 保持既有 | D19-R01-B | 正式集成和路线/性能仍待 D19 |
| `dock_and_shipyard_core` | 保持既有 | D19-R01-B | 船体交互锚点和正式场景仍待后续任务 |
| `market_prop_core` | 保持既有 | D19-R01-B | 正式消费者和性能仍待后续任务 |

海水、天气 Shader、灯光和后处理没有外部模型准入；由 D19-A04/D19-A05 创建项目自有表现资产，不伪造为外部资源已就绪。

## 玩家消费门禁

机器门禁：`game/tools/results/D18/D18-S02-R02/D18_R02_PLAYER_CONSUMPTION_GATE.json`。

- `player_humanoid_candidate_available=true`，有效数量 1：`candidate_superhero_female_fullbody_r03p`。
- D20-A01-R01 可以开始受控 Work Prefab；不得直接修改正式生成入口。
- D20-A02-R01 仍不允许开始，必须等待 D20-A01-R01 真实 `rig_interface_ready=true`。
- 该候选高度为 1.8537368m，D20 必须按 D17 1.8m 基线重新校准脚底、胶囊体、相机和上下船。

## 验证与可复核结果

工具命令：

```text
python -m tools.d18_s02_r02_refresh --project-root . --generated-on 2026-08-15 --write
python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs --output game/tools/results/D18/D18-S02-R02/D18_S02_R02_MANIFEST_AUDIT_PACKS.json
```

刷新工具实际输出 `PASS`，manifest index、证据、catalog validation 和重复引用均为 0 错误。针对 `docs/game/design/common/assets/packs` 的只读 manifest audit 输入 28 个 JSON、错误 0、状态 `PASS`；R03 两个新增 manifest 也分别单独运行并为 `PASS`。D18 R02 的 machine result、玩家门禁、输入/输出统计和历史 R01 SHA 快照保存在 `game/tools/results/D18/D18-S02-R02/`。

没有写入 `d18_local_development_ready_catalog.json`，没有修改 Registry、asset_state、场景、Prefab 或 Unity 正式运行时。所有新条目的 `source_state_snapshot` 仍为 Candidate，准入不等于资源状态晋级。
