# D20-S02 交接报告

- 任务编号：`D20-S02`
- 日期：`2026-08-15`
- 结论：**BLOCKED**
- 决策：未开始正式角色/NPC/动物集成；未修改正式场景、生成器、正式 Prefab 或共享 Animator。

## 阻断门禁

1. `D19-R01-S02` 的权威新门禁结果为 `PARTIAL`，而 D20-S02 明确要求 `PASS`。D19 真实路线约 254 秒，未闭合 NPC/市场/登船/出航/返港/保存重载；当前 Player 14 固定镜头未运行；低中高 1080p 性能均未达目标，PlayMode 预构建超时。
2. D20-A01 只有 `asset_kenney_blocky_character_a_v01` 候选，源为 Generic/NoAvatar，`rig_interface_ready=false`；D20-A02 因此 BLOCKED，不能建立玩家 Animator/Override。
3. D20-A03/A04/A05 为 PARTIAL；虽然 A03/A05 的 interface JSON 有 ready 布尔，但资源审核状态仍是 Candidate、缺主场景消费者和完整运行证据。D20-A06 仅隔离表现 PASS，未进入正式场景。
4. D18 当前 `LocalDevelopmentReady=0`、`ReleaseApproved=0`。`intake_eligible` 只允许受控工作/Prefab 消费，不能被写成正式运行就绪。

## 已读取和处理的结果

| 来源 | 真实结果 |
|---|---|
| D20-S01 | PASS，标准冻结 |
| D20-A01 | PARTIAL，rig 未就绪 |
| D20-A02 | BLOCKED，前置 rig 门禁失败 |
| D20-A03 | PARTIAL，12 Prefab/3 源模型，3/3 隔离测试 |
| D20-A04 | PARTIAL，7/7 隔离测试，无主场景路线 |
| D20-A05 | PARTIAL，8 wildlife Prefab，6/6 隔离测试；鸟/海龟等缺口保留 |
| D20-A06 | PASS（隔离表现），4 Prefab/6/6 测试，未正式接入 |
| D18-S02-R01 | PASS，准入目录刷新；不是运行时晋级 |
| D19-R01-S02 | PARTIAL，D20-S02 硬阻断 |

## 测试、截图和性能

- D20-A03 Unity EditMode：3/3 PASS。
- D20-A04 隔离 Unity EditMode：7/7 PASS。
- D20-A05 Unity EditMode：6/6 PASS。
- D20-A06 Unity EditMode：6/6 PASS。
- D19-R01-S02 EditMode：23/23 PASS；D19 PlayMode：超时，无有效 XML。
- D17 历史本机基线：60/60 PASS、工具 79/79 PASS，但本轮未重跑，且不能替代 D19/D20 正式门禁。
- D20-S02 正式玩家路线：NOT RUN（硬门禁未满足）。
- D20-S02 固定角色截图：0 张；D19 的 10 张有效路线帧只作为失败证据，不能冒充 D20 证据。
- D20-S02 1080p 性能：NOT RUN。D19 已有 Low/Medium/High 真实数据分别为平均 13.54/57.85/59.18 FPS，1% low 为 0.18/11.62/21.49，均未达标准。

## 资源与兼容回退

已核对的玩家 key 为 `asset_kenney_blocky_character_a_v01`；NPC key 共 12 个：`npc_d20_deckhand_01`、`npc_d20_rigger_01`、`npc_d20_shore_trader_01`、`npc_d20_shore_trader_02`、`npc_d20_dock_clerk_01`、`npc_d20_net_mender_01`、`npc_d20_fisher_01`、`npc_d20_tavern_cook_01`、`npc_d20_boatbuilder_01`、`npc_d20_sailmaker_01`、`npc_d20_harbor_watch_proxy`、`npc_d20_common_resident_01`。

野生/水生 key 共 8 个，覆盖鹰、狐、牛、两类鱼、鲨鱼、Manta 等价样本和鲸；宠物/观察 key 覆盖猫、Shiba、鹰和狐。Manta 不等于海龟；第二种鸟、真正海龟、女性/中性完整人形均继续作为真实缺口。

回退保持为 D17 `D2FallbackHumanoidBuilder/D2CharacterController`、NPC 中立姿态/FarProxy、动物 Idle/Swim/FarProxy、宠物 Unavailable/remote fallback。回退只写表现，不写逻辑、关系、生态、奖励或存档。

## 修改文件

本任务仅新增：

- `game/tools/results/D20/D20-S02/D20_S02_MACHINE_RESULT.json`
- `docs/game/design/common/D20_CHARACTER_CREATURE_ACCEPTANCE.md`
- `docs/game/design/common/handoffs/D20/D20-S02_HANDOFF.md`
- `.workbuddy/memory/2026-08-15.md` 中的 D20-S02 记忆段落

没有修改或删除已有 D20/D19 资源、场景、Prefab、脚本、契约、存档或资源状态。

## 责任任务与重新放行条件

| 顺序 | 责任 | 必须完成 |
|---:|---|---|
| 1 | D19 修复/复验 | `D19-R01-S02 PASS`：真实路线、保存恢复、14 镜头、三档性能、PlayMode/D17 |
| 2 | D20-A01/A02 | 真实 player rig/Avatar 接口、Animator/动作回归；或明确重新批准 D17 fallback |
| 3 | D20-A03/A04/A05/A06 | 主场景消费者、交互反馈、动物水陆层级、LOD、截图、性能、读档 |
| 4 | D18-S03 | 真实消费者证据后才决定 `LocalDevelopmentReady` 晋级 |
| 5 | D20-S02 | 仅在上述硬门禁全满足后重新执行；保存回滚点并串行集成 |

机器明细：`game/tools/results/D20/D20-S02/D20_S02_MACHINE_RESULT.json`。

## 交接结论

**BLOCKED**。不允许开始 D20-S02 的正式集成，也不允许把上游隔离测试计为角色/动物正式通过。保留旧回退与稳定 key，等待 D19 正式门禁和 D20-A01/A02 的真实 P0 证据。
