# D20-S02 角色与动物表现集成验收

日期：2026-08-15
任务：`D20-S02`
结论：**BLOCKED**

## 1. 当前门禁结论

本轮没有开始正式角色/动物集成。任务规定 `D19-R01-S02 PASS` 是硬门禁，而当前权威结果仍是 `PARTIAL`；继续写 InitialHarbor、正式生成引用或共享 Animator 连接会绕过门禁。因此本轮只完成前置证据核对，并把未通过事实保存为新的 D20-S02 结果。旧 D19/D20 报告和失败证据均保留，没有被改写为通过。

`D20-S01` 的 PASS 只冻结技术标准，不等于资源或运行时集成通过。`D18-S02-R01` 的 PASS 只表示准入目录真实可复核，也不等于 `LocalDevelopmentReady`、正式场景接入或 `ReleaseApproved`。

## 2. 硬门禁核对

| 门禁 | 实际状态 | 影响 |
|---|---|---|
| D20-S01 | PASS | 标准可用，但不能单独放行集成 |
| D19-R01-S02 | **PARTIAL** | **硬阻断；D20-S02 不得启动** |
| D18-S02-R01 | PASS | 仅允许受控工作/Prefab 候选消费 |
| D18 LocalDevelopmentReady | 0 | 没有资源晋级运行时就绪 |
| D18 ReleaseApproved | 0 | 没有发布批准 |
| D20-A01 | PARTIAL；`rig_interface_ready=false` | 玩家 Humanoid/Avatar 接口未就绪 |
| D20-A02 | BLOCKED | 玩家 Animator 层不能安全制作/接入 |
| D20-A03 | PARTIAL；接口布尔为 true，但状态为 Candidate | 12 个 NPC Prefab 还不是正式运行消费者 |
| D20-A04 | PARTIAL | 只有隔离表现适配器和测试，无主场景路线 |
| D20-A05 | PARTIAL；接口布尔为 true，但状态为 Candidate | 8 个动物样例没有正式 Player/目标设备证据 |
| D20-A06 | PASS（隔离表现） | 4 个宠物/珍稀观察 Prefab 未进入正式场景 |

### D19 失败证据摘要

`D19-R01-S02` 的真实 Player 补充路线约 254 秒，未完成 NPC/市场/登船/出航/返港/保存重载闭环；当前 Player 的 14 个固定镜头未运行，画面仍有灰盒/调试叠字债务。D19 EditMode 为 23/23，但 D19 PlayMode 在预构建阶段超时，没有可计为通过的 XML。1920×1080 真实 Player 的 30 秒 warm-up + 60 秒采样为：Low 平均 13.54 FPS、1% low 0.18；Medium 平均 57.85、1% low 11.62；High 平均 59.18、1% low 21.49。GPU 时间和有效 GC/工作集遥测不完整，三档均未达到既有标准。

本轮没有生成 D20-S02 的角色截图或性能采样，因为它们必须在 D19 门禁通过后进行。D19 已有的 10 张有效路线帧、性能档案和超时记录仅作为阻断证据，不能冒充 D20-S02 证据：

- D19 报告：`docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE_R01_S02.md`
- D19 机器结果：`game/tools/results/D19/D19-R01-S02/D19_R01_S02_MACHINE_RESULT.json`
- D19 路线/性能/视觉结果：`game/tools/results/D19/D19-R01-S02/`
- D19 现有截图目录：`game/tools/results/D19/D19-R01-S02/screenshots/`
- D20-S02 截图数量：`0`（NOT RUN）
- D20-S02 性能数据：`NOT RUN`（正式集成未开始）

## 3. 已核对的 D20 上游结果

| 任务 | 实物/接口事实 | 测试事实 | 本轮解释 |
|---|---|---:|---|
| D20-A01 | 1 个玩家候选；`asset_kenney_blocky_character_a_v01`；Generic/NoAvatar；8 bones、6 meshes、27 source clips；无 Avatar；`rig_interface_ready=false` | 静态检查 PASS | 仍保留 D17 回退，不能开始玩家动作正式接入 |
| D20-A02 | 没有创建正式 Animator/Override；只读 preflight 识别 A01 阻断 | preflight BLOCKED | 依赖真实 RigReady |
| D20-A03 | 12 个 NPC Prefab、3 个独立源模型；12 个是岗位复用，不是 12 个独立网格；全部 Candidate/Generic/NoAvatar | 3/3 PASS（隔离） | 不能视为主场景 12 NPC 已放置 |
| D20-A04 | 只读 NPC snapshot/Animator/LOD/反馈适配器；无 NPCSystem/关系/工资/存档写入 | 7/7 PASS（隔离） | 没有主场景 PlayMode 路线 |
| D20-A05 | 8 个野生/水生 Prefab；Eagle、Fox、Cow、两种鱼、Shark、Manta 等价样本、Whale；Manta 不是 Turtle | 6/6 PASS（隔离） | 没有正式 Player/目标设备性能或截图；鸟/海龟缺口仍存在 |
| D20-A06 | Cat、Shiba、Eagle observation、Fox observation 共 4 个 Prefab | 6/6 PASS（隔离） | 未修改正式场景；仍等待 D18-S03 |

D20 上游证据位置：

- `game/tools/results/D20/D20-A01/`
- `game/tools/results/D20/D20-A02/`
- `game/tools/results/D20/D20-A03/`
- `game/tools/results/D20/D20-A04/`
- `game/tools/results/D20/D20-A05/`
- `game/tools/results/D20/D20-A06/`
- `docs/game/design/common/handoffs/D20/`

## 4. 资源 ID 与边界

本轮只核对了已存在的稳定 key，没有把任何 key 写入正式生成器或场景。

### 玩家

- `asset_kenney_blocky_character_a_v01`
- manifest：`pack_kenney_blocky_characters_d18_a04_v1`
- intake：`intake_3469c541b7a7968018ba`
- 状态：可做候选工作 Prefab；Generic/NoAvatar，不可宣称最终玩家 rig。

### NPC

`npc_d20_deckhand_01`、`npc_d20_rigger_01`、`npc_d20_shore_trader_01`、`npc_d20_shore_trader_02`、`npc_d20_dock_clerk_01`、`npc_d20_net_mender_01`、`npc_d20_fisher_01`、`npc_d20_tavern_cook_01`、`npc_d20_boatbuilder_01`、`npc_d20_sailmaker_01`、`npc_d20_harbor_watch_proxy`、`npc_d20_common_resident_01`。这些 key 映射到 D15 稳定 NPC ID，但没有被本轮挂到正式港口；通用 NPC 源仍是 3 个，身份/岗位事实仍由 NPC/任务/关系系统拥有。

### 野生动物与宠物

- 野生/水生：`wildlife_animal_quaternius_eagle_v01_visual`、`wildlife_animal_quaternius_fox_v01_visual`、`wildlife_animal_quaternius_cow_v01_visual`、`wildlife_fish_quaternius_fish_01_v01_visual`、`wildlife_fish_quaternius_fish_02_v01_visual`、`wildlife_animal_quaternius_shark_v01_visual`、`wildlife_animal_quaternius_manta_ray_v01_visual`、`wildlife_animal_quaternius_whale_v01_visual`。
- 宠物/观察：`pet_animal_quaternius_cat_pack_vol2_v01_visual`、`pet_animal_quaternius_shiba_inu_v01_visual`，以及鹰/狐观察复用 key。
- 明确缺口：当前没有可准入的第二种鸟、真正的海龟模型、女性/中性完整人形基底。Manta 不能代替海龟，颜色/换装不能代替独立身体或骨架。

### ResourceKey / 生成边界核对

当前 D10 外观入口通过 `Resources.Load<GameObject>("D10/Appearance/" + assetId)` 查询外观资源；D20 NPC、wildlife、pet 适配器只消费上游快照中的 `resource_key`，只写自己的 Transform、Renderer、Animator、LOD、锚点和反馈。未发现可在本轮安全接入的第二套角色/动物数据路径，也没有修改现有 ResourceKey 语义、生成器、Registry 或存档。

## 5. 兼容回退

| 表现类别 | 已保留回退 | 不能替代的证据 |
|---|---|---|
| Player | `D2FallbackHumanoidBuilder` / `D2CharacterController`；保留 `D2_PlayerAvatar`、物理根、外观历史和稳定 ID | 最终玩家模型、Avatar、动作层、真人/目标设备验收 |
| NPC | D20 中立姿态、缺动作回退、FarProxy | 主场景生成、岗位行为、对话/拒绝/受伤路线 |
| Wildlife | Idle/Swim 映射、远距 fallback、`CullCompletely` | 生态事实、鱼群/动物行为、正式水陆层级和性能 |
| Pet/rare | Unavailable/remote fallback、只读位置恢复、观察反馈 | 宠物所有权、关系/奖励/存档以及正式路线 |

回退由表现层使用，不写强逻辑、经济、关系、生态、奖励、任务或存档；不能通过回退掩盖 D19 的路线、性能和视觉门禁失败。

## 6. 本轮没有修改的范围

本轮未修改 InitialHarbor、正式玩家/NPC/动物生成引用、正式 Prefab、共享 Animator、Systems、AgentCore、Registry、Save Schema、contracts、server 或 web，也未提升任何资源状态。工作区中已有的 InitialHarbor 修改、D20 Prefab 和 Presentation 未被覆盖或清理；这些是前序工作区事实，不作为本轮完成证据。

## 7. 后续放行顺序

1. 由 D19 修复责任任务解决真实 Player 路线、交互反馈、返港/保存恢复、14 个 Player 镜头、低中高 1080p 性能、PlayMode 和 D17 回归，然后重新运行 `D19-R01-S02` 至 PASS。
2. D20-A01 补齐真实玩家 rig/Avatar 接口；完成后重跑 D20-A02 preflight 和动作回归。若继续使用 D17 回退，必须由新的任务明确批准，而不能默认为完成。
3. D20-A03/A04/A05 补齐主场景运行消费者、对话/拒绝/受伤、动物地面/水体、LOD、截图、性能和读档证据；D20-A06 保留其隔离 PASS，待正式集成消费。
4. 真实消费者存在后运行 D18-S03，由它决定哪些资源可以晋级 `LocalDevelopmentReady`；本任务不提前晋级。
5. 所有硬门禁满足后，重新打开 D20-S02，保存回滚点，再做玩家/NPC/动物正式串行集成和 12+ NPC/动物样例路线。

## 8. 机器结果

机器结果、逐项门禁、资源 key、测试计数、截图/性能未运行声明和阻断原因位于：

`game/tools/results/D20/D20-S02/D20_S02_MACHINE_RESULT.json`

综合结论：**BLOCKED**。这是对硬门禁事实的如实判断，不是把上游隔离测试或历史本机基线降级，也不是删除失败流程。
