# D18-A07 Placeholder Replacement Map

状态：**PARTIAL（映射审计 PASS，尚未解锁资源接入）**  
核对日期：2026-08-14  
任务：D18-A07

## 1. 目的与边界

本成果把 D16-A05 的权威 placeholder 队列与 D18 A01—A06 候选 manifest 连接起来，生成稳定、可增量更新的替换计划。输入的唯一事实源是：

- Registry：`game/Assets/Resources/ContentRegistry/entries/` 中 `asset_state == placeholder` 的记录；
- 候选：`docs/game/design/common/assets/**/*.manifest.json` 的 `candidate_files`；
- 既有 Prefab/场景只做只读消费者扫描。

工具没有修改 Registry、`asset_state`、content ID、Prefab、场景、正式资源或 Systems，也没有把候选资源写入运行时。`planned_candidate` 仅表示计划中的候选关系，不能解释为 `integrated`、`LocalDevelopmentReady` 或 `ReleaseApproved`。

主映射文件：[`d18_placeholder_replacement_map.json`](assets/replacement/d18_placeholder_replacement_map.json)。

## 2. 真实输入与机器统计

| 项目 | 结果 |
|---|---:|
| Registry placeholder 输入 | 2,264 |
| 映射输出记录 | 2,264 |
| 有语义候选的记录 | 1,107 |
| 保持未映射的记录 | 1,157 |
| 候选 manifest | 25 |
| 候选文件记录 | 1,857 |
| 允许的一对多组合 | 45 个岛屿组合记录，审计通过 |
| 合法多对一复用组 | 35 |
| 重复 content ID | 0 |
| 文件断链 | 0 |
| 许可证证据断链 | 0 |
| 非法状态 | 0 |
| 重复写者 | 0 |
| 运行时消费者缺口 | 303 |
| LocalDevelopmentReady 候选 | 0 |
| ReleaseApproved 候选 | 0 |

机器审计的结构结论为 PASS；任务结论为 PARTIAL，因为候选仍停留在 Candidate/ImportVerified，且运行时消费者、Prefab、碰撞、锚点、LOD、动画和部分语义候选尚未由后续责任任务完成。

机器结果：[`D18_A07_MACHINE_RESULT.json`](../../../../game/tools/results/D18/D18-A07/D18_A07_MACHINE_RESULT.json)。候选扁平目录快照：[`D18_A07_CANDIDATE_CATALOG.json`](../../../../game/tools/results/D18/D18-A07/D18_A07_CANDIDATE_CATALOG.json)。

## 3. 输出记录契约

每条记录均包含以下字段，便于 A07 之后按 `content_id` 增量重算，不需要复制 2,264 行手工清单：

`content_id`、`placeholder_asset`、`domain`、`gameplay_priority`、`visibility_frequency`、`target_candidates`、`status`、`mapping_type`、`license_evidence`、`prefab_scene_consumers`、`collision_requirements`、`anchor_requirements`、`lod_requirements`、`animation_requirements`、`responsible_phase`、`write_owner`、`registry_asset_state`、`blockers`。

状态含义：

- `planned_candidate`：已有实际候选文件、许可证证据和 ImportVerified 证据，但尚未接入；
- `unmapped`：当前候选目录没有足够语义依据，保持待处理；
- `blocked_candidate_evidence`：候选关系存在，但文件或许可证证据不完整；当前全量结果为 0。

`mapping_type` 中 `many_to_one_reuse` 是明确复用同一文件，不是复制模型；`one_to_many_composition` 仅用于岛屿由沙滩、岩石、棕榈等模块组成。当前 A07 审计对其余一对多关系不放行。

## 4. P0 样板闭环

### 已进入 2,264 条映射的 P0

| content_id | 候选 | 类型 | 当前状态 | 主要阻断 |
|---|---|---|---|---|
| `asset_ph_crew_male` | `asset_ph_crew_male` / Kenney Blocky Characters vertical slice | 多对一复用 | planned_candidate | LocalDevelopmentReady、运行时消费者、锚点 |
| `crew_d15_a07_preset_001` | `crew_quaternius_male_shirt_v01` / Quaternius Animated Men | 多对一复用 | planned_candidate | Prefab、碰撞、LOD、动画绑定、历史/角色审核 |
| `building_saint_marys_dock` | `building_saint_marys_dock` / Pirate Kit vertical slice | 多对一复用 | planned_candidate | 碰撞、锚点、LOD、运行时消费者 |
| `building_small_boat_pier` | `building_small_boat_pier` / Pirate Kit vertical slice | 多对一复用 | planned_candidate | 碰撞、锚点、LOD、运行时消费者 |
| `island_saint_marys` | `patch-sand.fbx`、`rocks-sand-a.fbx`、`palm-detailed-straight.fbx` | 一对多组合 | planned_candidate | 地形组合、碰撞、锚点、LOD、运行时消费者 |

### P0 控制点（不伪造 Registry 映射）

- 玩家：当前 2,264 条 placeholder 中没有独立玩家视觉 ID，保留既有玩家视觉契约，由 D19 布局/场景任务补齐控制点；
- 起始船：`asset_ph_ship_fishing` 已是 integrated 且不在 placeholder 输入，`ship-pirate-small.fbx` 仅作为 D21 候选控制点，不能自动重映射；
- 海水/天气：A03—A06 没有可证明的海面或天气运行时候选，保持 unmapped control point；
- 核心 UI：当前 2,264 条 placeholder 没有核心 UI content ID，使用 Kenney Input Prompts 的真实候选作为 D22 控制点，但不写入 Registry。

因此 P0 的“覆盖”表示候选和接口入口已被明确；不表示 P0 已可运行或已完成场景替换。

## 5. 阶段统计与责任入口

| 阶段 | 总数 | 有候选 | 未映射 | 下一责任 |
|---|---:|---:|---:|---|
| P0 | 5 | 5 | 0 | D19 港口/岛屿接口、D20 关键 NPC、D21 船海、D22 UI 控制点 |
| P1 | 548 | 535 | 13 | D19 港口/建筑/植被，D20 角色动物宠物，D21 鱼群/船海，D22 表现接口 |
| P2 | 500 | 32 | 468 | D23 道具/服装语义候选和外观接口 |
| P3 | 1,211 | 535 | 676 | D19 植被长尾和 D24 全量 QA/释放门禁 |

阶段与单一写者约束：

- D19 只接收本文件中环境、岛屿、建筑和 P0 地形记录；不绕过 manifest 直接下载或替换；
- D20 只接收 Crew、Animal、Pet 记录，角色/动物系统仍拥有强逻辑数据；
- D21 只接收 Fish 与船海控制点；起始船的 integrated 事实不得被 A07 覆盖；
- D22 只接收 UI、材质、音频和海面/天气缺口的候选审计入口；
- D23 负责 Item/Outfit 的语义候选补齐，不能把通用道具模型冒充服装；
- D24 负责全量消费者、Prefabs、碰撞、锚点、LOD、动画、许可证和人工 ReleaseApproved 门禁。

## 6. 未映射和阻断原则

未映射的主要原因是：

1. 服装 165 条没有服装专用候选，角色基础模型不能替代服装；
2. 鱼获处理、食物、药材、工具等 item 没有同语义模型，不能用炮、桶或通用道具强行代替；
3. 植物长尾包含 `arrowleaf`、`burdock` 等当前候选没有的物种/状态；
4. 资产类数据目录、植被 profile 等是数据或运行时配置，不应被错误地映射成音频/视觉文件；
5. 海面、天气运行时表现没有当前 manifest 可证明的候选。

所有已映射记录还保留以下通用阻断：候选未确认 LocalDevelopmentReady、运行时消费者未注册，以及按实体类型需要的 Prefab/碰撞/锚点/LOD/动画验证。工具不会根据字段自动批准这些状态。

## 7. 审计规则

只读工具 `tools/d18_placeholder_replacement_map.py` 检查：

- 权威输入 ID 与输出 ID 一致；
- content ID 唯一；
- 候选文件和许可证证据存在；
- Candidate → LicenseEvidenceComplete → ImportVerified → LocalDevelopmentReady → ReleaseApproved 的状态不被工具跨级推断；
- 合法的多对一复用和仅限岛屿模块组合的一对多；
- owner_system 是否出现重复写者；
- Registry/Prefab/场景声明的消费者是否存在，以及候选运行时消费者是否仍为空；
- Registry 快照摘要，证明 A07 未改动 Registry。

重复运行摘要：两次全量运行均得到 digest `e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`，`stable=True`。重跑记录：[`D18_A07_DETERMINISM_RECHECK.md`](../../../../game/tools/results/D18/D18-A07/D18_A07_DETERMINISM_RECHECK.md)。

## 8. 使用方式与后续门禁

```powershell
python tools/d18_placeholder_replacement_map.py `
  --project-root . `
  --output-map docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json `
  --result-dir game/tools/results/D18/D18-A07
python -m unittest tools.tests.test_d18_placeholder_replacement_map -v
```

后续候选 manifest 增量更新后，应重新运行工具并比较 `deterministic_digest`、输入 ID 集合、P0 控制点和阻断原因。只有责任人完成 Prefab/场景消费者、碰撞、锚点、LOD、动画及 LocalDevelopmentReady 审核后，后续同步任务才可把记录推进到实际接入计划；ReleaseApproved 始终需要有责任的人类审核者确认，工具和 Agent 不得代签。

最终重跑摘要（包含玩家 P0 控制点）：digest `e908eef063e1019fa19e273cb5234c955913dd781855277408821ac2b5d68c75`，`stable=True`。
