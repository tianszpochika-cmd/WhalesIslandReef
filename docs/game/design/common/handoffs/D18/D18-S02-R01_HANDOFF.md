# D18-S02-R01 资源准入刷新交接

任务编号：`D18-S02-R01`  
日期：2026-08-15  
结论：**PASS**（本轮全局 `PrefabCandidateIntake` 刷新真实、完整、可复核）

## 1. 实际修改文件

- `docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`：唯一全局准入目录；保留 v2 快照并新增 159 个证据完整的受控工作入口。
- `docs/game/design/common/D18_FREE_ASSET_INTAKE_REFRESH_R01.md`：本交接对应的主报告。
- `tools/d18_s02_r01_intake_refresh.py`：只读上游证据审计、稳定 ID、目录刷新和语义校验工具；唯一写入 catalog 的 R01 工具。
- `tools/tests/test_d18_s02_r01_intake_refresh.py`：7 个 R01 自动测试。
- `game/tools/results/D18/D18-S02-R01/`：机器结果、上游审计、类别门禁、作用域保护、重复运行和 28 个 manifest 审计结果。
- `docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md`：本文件。
- `.workbuddy/memory/2026-08-15.md`：本任务记忆追加。

历史 v2 报告、v2 handoff、旧 BLOCKED 报告和旧机器结果没有删除或重写。

## 2. 五份上游处理结论

| 上游 | 状态 | 处理数量 | 进入 R01 intake | 保留缺口 |
|---|---|---:|---:|---|
| D18-A04-R01 | PARTIAL | 25 | 24 | 原 Blender-only 鸟 1 条排除 |
| D18-A04-R02-F | PARTIAL | 4 | 0 | Unity 导入未运行，女性缺口 OPEN |
| D18-A04-R02-C | PARTIAL | 1 | 1 | 猫只有 2 clips；无原始 Collider/LOD/Avatar |
| D18-A04-R02-B | PARTIAL | 1 | 1 | Eagle 仅中远景工作候选；无 Animator/LOD/Collider |
| D18-A05-R01 | PARTIAL | 151 | 133 | 18 条海岸环境按上游责任排除；船部件/锚点仍缺 |

A05 进入的 133 条为 96 `port_structure_core`、6 `dock_and_shipyard_core`、16 `market_prop_core`、15 `ship_visual_candidate`。没有把 A05 的“证据推荐”写成正式运行时消费。

## 3. 目录与状态边界

R01 catalog 统计：28 manifests、1,863 候选文件、1,833 `intake_eligible` 文件、24 个含准入记录的 manifest、4 个排除 manifest、0 duplicate、0 errors。旧 v2 基线为 25/1,857/1,674/16/9/0。

新增记录可以保留源 manifest `review_status=Candidate`，前提是隔离复审同时提供本地文件、候选文件 SHA-256、归档 SHA-256、CC0 许可证证据、Unity `ImportVerified` 证据和稳定映射。该规则只作用于非资源状态字段 `PrefabCandidateIntake`，不改变源 manifest 状态。

`LocalDevelopmentReady=0`、`ReleaseApproved=0`，ready catalog 未修改。所有新增 entry 都禁止正式 InitialHarbor、Registry/asset_state 修改、LocalDevelopmentReady/ReleaseApproved 晋级和发布审批。D18-S03 仍是运行时消费者与 LocalDevelopmentReady 的唯一后续门禁。

## 4. 类别与消费门禁

| 类别 | 真实 intake 数 | 责任任务 | 当前可做 | 当前不可宣称 |
|---|---:|---|---|---|
| player_visual_core | 1 | D20-A01/A02 | 受控玩家视觉基体复核 | Humanoid/Avatar/正式玩家 Prefab |
| npc_visual_core | 4 | D20-A03 | 男性 NPC 工作复核 | 女性覆盖、时代服饰完成、运行时消费者 |
| wildlife_core | 18 | D20-A05 | 陆生/水生/Eagle 受控复核 | 完整生态行为、近景鸟、保存恢复 |
| pet_core | 3 | D20-A06 | 2 狗 + 1 猫工作复核 | 宠物动作/跟随/碰撞/LOD 完成 |
| animation_core | 26 | D20-A02/A05/A06 | 原始 Generic 片段检查 | Humanoid retargeting/正式 Animator |
| port_structure_core | 96 | D19-R01-B/D19-A03 | 港口结构 Work 场景 | 正式港口、碰撞/锚点/LOD |
| dock_and_shipyard_core | 6 | D19-R01-B/D21-A01 | 码头/船坞 Work 场景 | 泊位、吃水、系泊和出航链路 |
| market_prop_core | 16 | D19-R01-B | 市场密度 Work 场景 | 交易事实、正式场景和运行时交互 |

明确阻断：女性/中性完整人形 0、独立帆 0、独立舵/锚/交互锚点 0；海水/天气/Shader/灯光由 D19 项目自有资产任务负责。猫和 Eagle 是“准入可用但仍 PARTIAL”，不等于缺口完全关闭。

## 5. 验收与命令结果

- R01 refresh tool：PASS；0 manifest index errors、0 catalog validation errors、0 duplicate new refs、0 path escape、0 status shortcut。
- 28 个实际 manifest 的 `d18_asset_manifest_audit.py`：PASS，28/28，0 errors、0 issues；Cat 文件名不符合普通后缀但已通过显式 manifest 参数审计。
- 历史 v2 intake audit（只读、不写当前 catalog）：PASS，27 个标准后缀 manifest、1,862 candidates、1,674 v2 intake、0 errors。
- `python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake tools.tests.test_d18_s02_r01_intake_refresh`：PASS，18/18。
- JSON `json.tool` 校验：PASS；catalog、ready catalog、R01 machine、upstream、category、scope、manifest audits 和 legacy audit 均有效。
- 重复运行：catalog SHA-256 `F2D040DE15AFBE5DE2D22B863630484672E909EB352DD5E66838FD853DCDE530` 前后相同，1833 条、0 duplicates、0 validation errors。
- `git diff --check`：PASS，exit code 0；仅有 Git 对既有工作区文件的 LF/CRLF 提示；不提交、不推送 Git。

## 6. 后续注意事项

1. D19-R01-B/D19-A03 只能使用当前目录中准入的 96/6/16 港口类别或自己的项目自有表现资产；不得使用 A05 被排除的 18 个海岸环境条目绕过 D19-A01/A03。
2. D20-A01/A03 不得把 1 个中性基体或 4 个男性候选冒充女性/时代中性完整角色；Generic/NoAvatar 不得静默当 Humanoid。
3. D20-A05/D20-A06 可以分别对 Eagle/Cat 做受控 Work 场景复核，但必须保留动作、比例、碰撞、LOD、Animator、保存和生态/宠物所有权缺口。
4. D21 只能把 15 个 `ship_visual_candidate` 当作船体/远景工作输入；独立帆、舵、锚和交互锚点仍是阻断项。
5. 任何正式集成和运行时消费者证据必须交给 D18-S03；本 R01 不改变 `LocalDevelopmentReady` 或 `ReleaseApproved`。

## 7. 结果路径

- 主目录：`docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json`
- 主报告：`docs/game/design/common/D18_FREE_ASSET_INTAKE_REFRESH_R01.md`
- 机器结果：`game/tools/results/D18/D18-S02-R01/`
- 旧 v2 交接：`docs/game/design/common/handoffs/D18/D18-S02_V2_HANDOFF.md`
- 本交接：`docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md`

最终交接结论：**PASS**，仅放行证据完整的受控开发准入，不放行运行时晋级或发布。
