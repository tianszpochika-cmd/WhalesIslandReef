# D18-S02-R01 免费资源准入增量刷新报告

日期：2026-08-15  
结论：**PASS**（仅表示本轮 `PrefabCandidateIntake` 刷新真实、完整、可复核；不表示 `LocalDevelopmentReady`、正式场景接入或发布就绪）

## 1. 本轮边界与历史保留

本轮是 D18-S02-v2 之后的唯一全局准入目录刷新。它读取五份隔离复审/补齐结果，更新唯一目录 `d18_prefab_candidate_intake_catalog.json`，不修改源 manifest 的资源状态，不创建 Prefab，不写 Registry，不接入正式场景，也不做运行时晋级。

D18-S02-v2 的历史快照保持可复核：

- 旧 v2 catalog SHA-256：`A25F07D4ED8A9AC5820126F9DD6A2D76DCA680F5A39B2767C12B3F645533EAB6`；
- 旧 v2 统计：25 manifests、1,857 候选文件、1,674 个准入文件、16 个准入 manifest、9 个排除 manifest、0 errors；
- 旧 v2 报告：`docs/game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md`；
- 旧 v2 交接：`docs/game/design/common/handoffs/D18/D18-S02_V2_HANDOFF.md`；
- 旧版 D18-S02 的 BLOCKED 报告、handoff 和机器结果没有删除或覆盖；其含义仍是“旧版运行时就绪验收发现循环依赖”。

本轮 catalog 内保存 `previous_v2_catalog_snapshot`，机器快照另存于：
`game/tools/results/D18/D18-S02-R01/D18_S02_R01_PREVIOUS_CATALOG_SNAPSHOT.json`。

## 2. 五份上游结果处理

| 上游任务 | 原结论 | 处理记录 | 本轮准入 | 排除/限制 |
|---|---|---:|---:|---|
| D18-A04-R01 | PARTIAL | 25 条角色/动物记录 | 24 | Blender-only 原鸟 1 条无 Unity 导入证据 |
| D18-A04-R02-F | PARTIAL | 4 个女性 FBX | 0 | 只有静态 FBX 审计；Unity ImportVerified 未运行 |
| D18-A04-R02-C | PARTIAL | 1 个猫 FBX | 1 | 只有 2 个动作；无原始 Collider/LOD/Avatar |
| D18-A04-R02-B | PARTIAL | 1 个 Eagle FBX | 1 | 仅适合受控中远景候选；无 Animator/LOD/Collider |
| D18-A05-R01 | PARTIAL | 151 个船港模型 | 133 | 18 个 `out_of_scope_coastal_environment` 按上游责任分给 D19-A01/A03，不冒充港口准入 |

A05 的 133 个准入文件由 96 个 `port_structure_core`、6 个 `dock_and_shipyard_core`、16 个 `market_prop_core` 和 15 个 `ship_visual_candidate` 组成。A05 的 18 个草、棕榈、沙地和岩石条目仍可由其真实复审记录追溯，但不进入本轮港口消费清单。

## 3. 刷新后的真实统计

| 指标 | v2 | R01 | 变化 |
|---|---:|---:|---:|
| 逻辑 manifest | 25 | 28 | +3（女性、猫、Eagle 隔离 manifest） |
| 候选文件总数 | 1,857 | 1,863 | +6 |
| `intake_eligible` 文件 | 1,674 | 1,833 | +159 |
| `intake_eligible` manifest | 16 | 24 | +8 |
| 排除 manifest | 9 | 4 | 目录刷新后按当前证据重新汇总 |
| 重复 manifest-file 引用 | 0 | 0 | — |
| 路径逃逸/引用错误 | 0 | 0 | — |
| LocalDevelopmentReady | 0 | 0 | 未晋级 |
| ReleaseApproved | 0 | 0 | 未晋级 |

刷新目录中的 1,833 条仍是文件级受控工作入口，不等于 1,833 个独立模型，也不等于正式内容 ID 已接入。所有新增记录均带有来源复审任务、候选路径、manifest、许可证证据、归档/候选文件哈希、ImportVerified 证据、允许任务、禁止范围、已知缺口和责任任务。

## 4. 资源状态边界

`intake_eligible` 继续是资源状态机之外的 `PrefabCandidateIntake` 字段。对于本轮新增的 159 条记录，源 manifest 的 `review_status=Candidate` 保持不变，`import_status=ImportVerified` 由上游本地 Unity 证据支持。该事实只允许指定任务在受控 Work 场景或 Prefab 制作中使用，绝不表示：

- `LocalDevelopmentReady`；
- 正式 InitialHarbor 或其他正式场景已接入；
- 已有运行时消费者、交互、碰撞、锚点、LOD、Animator、保存/恢复或性能通过证据；
- `ReleaseApproved`、Steam Ready 或任何外部发布批准。

海水、天气、Shader、灯光和后处理没有被伪造为外部候选；它们仍是 D19-A04/D19-A05 的项目自有表现资产责任。起始船 `asset_ph_ship_fishing` 的稳定 ID 和存档语义没有被替换。

## 5. 消费类别与真实缺口

| 消费类别 | 当前准入数 | 状态 | 允许表达 |
|---|---:|---|---|
| `player_visual_core` | 1 | INTAKE_AVAILABLE_PARTIAL | Kenney 中性基体工作复核；Humanoid Avatar、比例、碰撞仍缺 |
| `npc_visual_core` | 4 | INTAKE_AVAILABLE_PARTIAL | 4 个男性候选工作复核；1675 年服饰/时代适配仍缺 |
| `wildlife_core` | 18 | INTAKE_AVAILABLE_PARTIAL | 现有陆生/水生候选和 Eagle 受控复核；生态语义和 Prefab 仍缺 |
| `pet_core` | 3 | INTAKE_AVAILABLE_PARTIAL | Husky、ShibaInu、Cat 工作复核；宠物动作/跟随/恢复仍缺 |
| `animation_core` | 26 | INTAKE_AVAILABLE_PARTIAL | 原始 Generic 动作/片段复核；无正式 Animator/Avatar 绑定 |
| `port_structure_core` | 96 | INTAKE_AVAILABLE_PARTIAL | 港口/建筑模块 Work 场景复核；无消费者碰撞/锚点/LOD |
| `dock_and_shipyard_core` | 6 | INTAKE_AVAILABLE_PARTIAL | 码头/船坞模块 Work 场景复核；泊位/吃水/系泊仍缺 |
| `market_prop_core` | 16 | INTAKE_AVAILABLE_PARTIAL | 市场货箱、桶、旗帜等密度候选；Pivot/碰撞/批处理仍缺 |

仍需显式保留的缺口：

- `female_or_neutral_human_gap=0`：女性包的 4 个 FBX 尚未有 Unity ImportVerified；已有中性基体不能冒充完整 Humanoid 解决方案；责任为 D18-A04-R02-F、D20-A01、D20-A03。
- `cat_gap=1`：猫已有真实 ImportVerified 工作入口，但只有 Idle/Walking，不能宣称宠物完成；责任为 D20-A06、D18-S03。
- `bird_gap=1`：Eagle 可作中远景受控候选；原 OpenGameArt `bird.blend` 仍是 Blender-only 并被排除，不能互相冒充；责任为 D20-A05、D18-S03。
- `sail_rig_gap=0`、`rudder_anchor_gap=0`：没有独立帆、舵、锚或消费者交互锚点证据；责任为 D21-A01—A03。
- 18 个 A05 海岸环境条目不进入本轮港口消费；D19-A01/A03 必须使用自己的准入/项目自有证据，不得从本包类别推断。

## 6. D19/D20 消费门禁

| 任务/方向 | 本轮可用入口 | 门禁结论 |
|---|---|---|
| D19-R01-B / D19-A03 | 96 港口结构、6 码头船坞、16 市场道具；另有 v2 环境/材质准入 | 可立即开始受控 Work 场景/Prefab；不得绕过目录进入正式场景；碰撞、锚点、LOD 由消费者补齐 |
| D20-A01 | 1 个中性基体 | 可 PARTIAL 开始工作复核；Humanoid Avatar、脚底、D17 1.80m 和正式玩家 Prefab 未通过前不能完成 |
| D20-A03 | 4 个男性 NPC 候选 | 可 PARTIAL 开始；时代服饰、身份变体、Animator/碰撞未完成；女性缺口保持 OPEN |
| D20-A05 | 18 个 wildlife（含 1 个 Eagle） | 可 PARTIAL 开始；Eagle 只承诺中远景工作入口，近景、栖停、起降和生态消费者仍阻断 |
| D20-A06 | 2 只狗 + 1 只猫 | 可 PARTIAL 开始；Cat 的动作覆盖、比例、跟随、碰撞、LOD 和存档恢复仍待任务验证 |
| D20-A02 | 26 个动画核心候选 | 可作为原始片段复核；Generic/NoAvatar 不能静默当作 Humanoid retargeting |

所有任务的正式集成、运行时消费者和 `LocalDevelopmentReady` 晋级仍等待 D18-S03；不允许把本目录当成正式场景白名单以外的下载入口。

## 7. 验证与机器结果

机器结果目录：`game/tools/results/D18/D18-S02-R01/`。

- `python tools/d18_s02_r01_intake_refresh.py ... --write-catalog`：PASS；28 manifests、1,863 文件、1,833 intake、0 catalog errors。
- `python tools/d18_asset_manifest_audit.py` 对实际 28 个 manifest（含文件名不带标准后缀的 Cat manifest）：PASS，28/28，0 errors、0 issues。
- `python tools/d18_prefab_candidate_intake_audit.py` 历史 v2 规则只读复核：PASS，27 个标准后缀 manifest、1,862 文件、1,674 v2 intake；不写当前 R01 catalog。
- `python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake tools.tests.test_d18_s02_r01_intake_refresh`：PASS，18/18。
- JSON 校验：catalog、ready catalog、R01 machine/upstream/category、28-manifest audit、legacy audit 均 PASS。
- R01 工具重复执行：catalog SHA-256 前后均为 `F2D040DE15AFBE5DE2D22B863630484672E909EB352DD5E66838FD853DCDE530`，1833 条、0 重复、0 validation errors。
- `d18_local_development_ready_catalog.json` SHA-256 保持 `F218651902B39A989AD0E557DC62B576577C7AED0CD633D9F5FEF4C43F2F71A3`，状态仍为 BLOCKED、LocalDevelopmentReady=0、ReleaseApproved=0。
- 上游 Unity 证据被逐项读取：A04-R01 24 条、A04-R02-C 1 条、A04-R02-B 1 条、A05-R01 151 条；女性包 Unity 未运行，故没有准入。
- 未改 source manifest、Registry、asset_state、Prefab、场景、Systems、存档、contracts、server 或 web。

完整机器统计、逐项排除和类别门禁见：

- `D18_S02_R01_MACHINE_RESULT.json`
- `D18_S02_R01_UPSTREAM_EVIDENCE_AUDIT.json`
- `D18_S02_R01_CATEGORY_GATE.json`
- `D18_S02_R01_SCOPE_PROTECTION.json`
- `D18_S02_R01_REPEATABILITY.json`

## 8. 下一门禁

D18-S02-R01 PASS 只解锁真实证据范围内的受控工作场景/Prefab 工作。D19/D20/D21/D22 完成消费者侧 Prefab、碰撞、锚点、LOD、Animator、交互、保存/恢复、截图和性能证据后，统一交给 D18-S03。只有 D18-S03 才能决定哪些资源晋级 `LocalDevelopmentReady`；`ReleaseApproved` 仍只能由责任人类审核者确认。
