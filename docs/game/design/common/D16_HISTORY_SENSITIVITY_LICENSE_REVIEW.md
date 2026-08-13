# D16-A04 历史、敏感内容与授权审查报告

## 最终结论

状态：**IN_PROGRESS，机器结构/授权子门禁已通过，人工审查未完成**。

D15-S02 已通过；当前完整 game 目录的结构错误为 0、2918/2918 条可视资源授权记录闭合，但仍有 267 条 Pending 历史/敏感表现复核。因此本报告不把机器审计结果扩大为人工历史批准，也不把审查意见写成游戏数值裁决。

## 门禁证据

复核命令：

python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json

历史结果：D15_S02_QA_EXIT=1；当前顺序复验结果：D15-S02 PASS（0 errors、0 warnings）。

与本任务直接相关的阻断：

- D15-A06 的 166 个资源登记缺 historical_review、review_evidence_refs、content_warning_ids、skip_allowed；asset_ph_ship_fishing 另缺 lod_profile，asset_d15_a06_collision_profiles 缺 skip_allowed 且审查状态为空；
- D15-A02 有 2 条生境字段不一致；
- D15-A07 有 18 次引用指向 12 个未定义宠物外观 ID；
- D15-A08 任务/音频存在测试、LOD-N/A 和资源字段缺口；
- A01—A07 共 2536 个 display_name_key 没有可验证的中英本地化包证明；
- 24 个抽样中 9 个缺少明确的保存/恢复标记。

## 审查状态约定

本次沿用 D15/D14 冻结枚举：

- historical_tag：historical、inferred、fictional；
- historical_review：Pending、Approved、ApprovedWithWarning、Rejected；
- presentation_levels：Full、Contextual、LowIntensity、Omitted；
- content_warning_ids：无警告也必须是空数组，不得省略；
- skip_allowed：由审查数据决定；
- review_evidence_refs：来源、历史依据、敏感内容审查和授权证据的稳定引用。

Pending 或 Rejected 不得进入正式逻辑；ApprovedWithWarning 必须先显示警告并提供允许的低强度/跳过路径。跳过只记录事实，不生成奖励、关系、声望、土地、经济或生态结果。

## 范围审查矩阵

| 范围 | 已有成果 | 当前状态 | 待核查/责任 |
|---|---|---|---|
| 1675 年时间线 | REQUIREMENTS、D14 历史审查、D15 各批次 historical_tag/source_refs | BLOCKED | 逐条核对年份、地点、制度和来源；不把 inferred/fictional 当作史实。历史审查任务负责证据，内容/逻辑任务负责实现 |
| 势力与殖民网络 | D14FactionLegalSystem、D14_FACTION_PRIVATEERING_LAW、D4 势力目录复用 | PARTIAL | 逐条核对势力出现时间、法律语境和资产证据；法律状态仍由 FactionLegalSystem 写入 |
| 宗教与仪式 | D14ReligionCommunitySystem、D14_RELIGION_HISTORY_REVIEW | PARTIAL/PASS BY DESIGN | 样例有 ApprovedWithWarning、四级表现和跳过；批量新增内容仍须逐条补 evidence，不由审查意见修改宗教/关系数值 |
| 社群与礼仪 | D12CommunityTrade、D10 Tavern/Social、D14 社群审查 | PARTIAL | 核对社群来源、称谓、礼仪和表达范围；社区信任由 CommunityTradeSystem 裁决 |
| 奴役/强迫劳动 | REQUIREMENTS 的历史敏感规则、D15 审查字段门禁 | PENDING REVIEW | 当前没有以“可套利商品”通过的依据；需逐条列出史实锚点、表现等级、警告、跳过和不商品化说明，不能擅自下历史结论 |
| 捕鲸 | D11_WHALE_DOLPHIN_ROUTES 固定标签 history_1675_caribbean_whaling 和 non-glorifying 审查要求 | PARTIAL | 路线条件/生态/法律已有 owner；仍需核对每个内容资产和本地化表现的审查证据，捕鲸收益由 Reward/Economy/Inventory 写入 |
| 珍稀动物 | D11 生态状态机和鲸豚路线、D15-A03 动物目录 | PARTIAL | 保护状态、栖息地和法律来源需逐条核对；EcologySystem 写生态事实，审查不直接写数量或奖励 |
| 赌场/牌局 | D12 Poker、D10 Tavern 敏感类型 Gambling | PARTIAL | 必须保留赌博警告、退出/债务/作弊风险上下文；MiniGameSystem 写结算，审查不裁决下注、奖金或债务 |
| 土地经营 | D13 Island Governance、Property/Legal authority、D15 岛屿/建筑目录 | PARTIAL | 核对土地、产权、社群和殖民语境来源；Property/Legal/IslandGovernance 写结果，审查只提供许可门禁和语境 |
| 角色身份表达 | D10 Player Identity、D10 Appearance Outfit、D15-A07 角色/宠物目录 | PARTIAL | 核对文化来源、身份表达、服装历史标签和授权；Identity/AppearanceSystem 写状态，不把身份表达自动推导为法律或关系结果 |
| 外部资产 | D15 license/source 字段、Placeholder 许可证文件、D15-A01—A08 资产目录 | BLOCKED | D15 基础 license 字段检查通过，但 A06 166 个资源缺历史/审查元数据，不能完成“每个外部资产证据链”验收；AssetRegistry/资产任务补授权，审查任务复核证据 |

## 已确认的审查能力

- D14 宗教/社群系统在内容为 Pending/Rejected 时拒绝执行；ApprovedWithWarning 未确认警告时返回 warning_required；允许跳过时只写 skipped 事实。
- D14 法律系统保存势力、私掠、港口、检查和证据来源，但不把审查意见转换为罚金、声望或奖励数值。
- D11 捕鲸路线不是普通捕鱼按钮，要求许可证、季节、法律政策、船型、船员、设施、声望和生态条件；审查标签明确要求非美化表现。
- D10 身份/外观成果把身份表达、外观所有权、法律身份、关系和经济分开；外观与身份不会自动改变法律或关系。
- D15 冻结要求所有敏感内容包含审查状态、警告、跳过、表现等级和证据；当前批量 QA 尚未全部满足。

## 待核查清单

1. 建立 1675 时间线核查表：事件/势力/地点/制度、历史标签、证据引用、确定性或不确定性说明。
2. 对 D15-A01—A08 每条带 historical_tag 或 content_warning 的内容补齐审查字段，尤其是 A06 资源。
3. 对奴役/强迫劳动、殖民土地、宗教冲突、捕鲸、珍稀动物交易和牌局内容逐条确认是否允许 Full、Contextual、LowIntensity 或 Omitted。
4. 对 D10 身份/服装和 D15-A07 角色文化标签区分“表达/来源”与“法律身份/关系结果”。
5. 对所有外部或占位资产建立 source、rights holder、license reference、commercial use、modification permission 和 evidence reference 的逐条闭包。
6. 对事实不确定的条目保留 Pending 或 inferred，并记录核查问题，不自行补写历史事实。

## 责任边界

| 责任 | 允许做 | 禁止做 |
|---|---|---|
| 历史/内容审查 | 标签、证据、表现等级、警告、跳过和待核查记录 | 写价格、伤害、奖励、声望、生态数量、关系或法律裁决 |
| 资产 Agent | 来源、授权、模型/材质/音频引用和资源状态 | 删除敏感内容、伪造史实、写玩法数值 |
| 逻辑 Agent/所属系统 | 按已审查内容和事实裁决领域状态 | 把审查意见当作数值公式或让 UI/Agent 直接写结果 |
| UI/Agent | 展示警告、提供确认/低强度/跳过意图 | 隐藏警告、默认跳过、修改审查状态 |

## 本次未做的工作

- 未修改历史内容、敏感内容、授权、Systems 或游戏逻辑；
- 未删除任何 D15 条目或外部资源引用；
- 未把不确定事实写成确定历史结论；
- 未将审查状态改写为经济、奖励、法律、关系或生态数值；
- 未加入网络或 LLM。

## 重新开工门槛

1. D15-S02 统一 QA 错误归零并重新出具通过报告；
2. A06 资源审查字段和全部外部资产授权证据闭合；
3. 所有敏感范围逐条拥有审查状态、表现等级、警告、跳过和证据，或明确 Pending 待核查；
4. 对不确定事实完成证据核查或保留待核查标记；
5. 重新执行时间线、势力、宗教、社群、奴役/强迫劳动、捕鲸、珍稀动物、赌场、土地和身份范围审查。

## 2026-08-11 门禁复验更新

D15-S02 已通过；D15-A06 的 166 个 asset 资源现在具备 `historical_review`、真实项目文档 `review_evidence_refs`、`content_warning_ids`、`skip_allowed` 和 LOD/测试元数据。D16-A04 当前状态更新为 **PENDING**：这只关闭了目录字段完整性，不等于外部授权、1675 年史实逐条核验或敏感内容表现审查完成。

## 2026-08-11 结构审计进度更新

当前状态：**IN_PROGRESS，人工历史/授权审查未完成**。

## 2026-08-11 复审审计更新

重新执行 `tools/d16_a04_review_audit.py`：2812 条记录、91 条敏感记录、2590 条可视资源、2550 条授权记录；审查计数为 Approved 2609、ApprovedWithWarning 91、None 112；Pending/Rejected 为 0，但仍有 **488 条结构审查错误**，其中可视资源授权未闭合。审计器单测保持 3/3 PASS。该结果只说明缺口被重新量化，不代表人工历史事实、外部授权或敏感内容表现审查已通过。

新增 `tools/d16_a04_review_audit.py`，只检查目录结构，不替代历史研究或授权决定。当前扫描结果：2812 条注册记录、91 条敏感内容记录、0 条 Pending/Rejected review；仍有 488 个结构性缺口，且 40 条可视资源记录缺少完整 rights/authorization 记录，故仍阻断 A04。

复核输出：`tools/results/d16_a04_review_audit.json`。审计器单测通过，并明确保留 `ApprovedWithWarning` 的 warning/skip 要求；没有修改历史事实、法律、经济、关系或生态数值。

## 2026-08-11 全项目源记录复核

审计范围扩展到整个 `game` 目录后，发现 D15-A06 源目录副本也属于需要留痕的内容记录。已为其中 166 条缺字段记录补齐 `Pending` 结构元数据，并为源目录的 Quaternius 船占位登记补齐显式 authorization；没有把候选来源当作历史批准或外部授权批准。

最新 `tools/results/d16_a04_review_audit.json`：3139 条记录、91 条敏感记录、2916 条可视资源、2916/2916 权利记录闭合、结构错误 0、Pending 278、Rejected 0。当前唯一阻断为 `pending_historical_reviews`；人工逐条核验仍未完成。

## 2026-08-11 A04 当前审计复验

植被生成器重新生成的 13 个 `asset_vegetation_profile_*` 记录保留了 `historical_tag=inferred` 和项目来源引用，但缺少 D16-A04 所需的审查结构字段。已使用 `tools/d16_a04_fill_pending_metadata.py game/Assets/Resources/ContentRegistry --apply` 仅补齐 `historical_review=Pending`、候选 `review_evidence_refs`、空 `content_warning_ids` 和 `skip_allowed=false`；没有把它们改成 Approved，也没有新增或伪造来源。

随后以完整 `game` 目录重新审计：`entry_count=3139`、`visual_asset_count=2916`、`authorization_record_count=2916`、`structural_error_count=0`、`pending_review_count=278`、`rejected_review_count=0`。A04 机器结构/权利子门禁保持关闭，唯一剩余阻断是人工历史与敏感表现复核 `pending_historical_reviews`。

A02 生成器已同步加入相同的 Pending 字段输出；后续重生成不会再次擦除这组 D16-A04 结构元数据。

## 2026-08-12 Pending 人工复核队列

新增 `tools/d16_a04_review_queue.py`，只读取完整 `game` 目录中 `historical_review=Pending` 的记录，生成：

- `tools/results/d16_a04_review_queue.json`
- `tools/results/d16_a04_review_queue.md`

初始队列证据：

```text
status=BLOCKED
 pending_count=278
asset_history_and_authorization=187
historical_content=91
```

队列为每条记录保留文件路径、历史标签、来源/证据引用、资产状态、警告/跳过字段、LOD 信息和必须由人工决定的事项。

## 2026-08-12 明确虚构内容安全子门禁

新增 `tools/d16_a04_approve_explicit_fictional.py`。它默认只预览，只有显式 `--apply` 才能写回，并且同时要求：

- `historical_tag=fictional`；
- `source.kind=project-authored` 且许可证为内部自产；
- 所有 `review_evidence_refs` 在项目中真实存在；
- 无敏感标签、无内容警告，原状态为 `Pending`。

预览候选为 11 条，数量校验通过后才执行写回。写回记录保留 `review_decision_basis.not_a_historical_fact_claim=true`，并没有修改任何 `asset_state`。复验结果：

```text
content_registry_check: PASS (2812 entries)
D15-S02: PASS (batches=8 samples=24 errors=0 warnings=0)
D16-A04: IN_PROGRESS, Approved=2780, ApprovedWithWarning=91, Pending=267, structural_errors=0
```

这只关闭“明确虚构内容不构成历史事实主张”的安全子门禁；剩余 267 条仍需历史、敏感表现或外部资产责任审核，A04 不得标记 PASS。

## 2026-08-12 资源登记授权闭包复验

为 `asset_ph_crew_male` 与 `asset_ph_palm_leaves` 补齐了可追踪的源资源路径、运行时 Prefab 路径、`asset_state=placeholder`、LOD/测试元数据，以及各自独立的 Quaternius CC0 authorization。两项仍是占位资源，未被批准为最终表现，也未改变运行时逻辑。

复验结果：`content_registry_check` 2812 entries PASS；D16-A04 为 3139 条记录、2918/2918 可视资源权利记录闭合、结构错误 0、Approved 2780、ApprovedWithWarning 91、Pending 267、Rejected 0。A04 的唯一机器阻断仍是 Pending 人工历史/敏感表现复核。

## 2026-08-12 人工复核分批包

为便于不同责任人并行处理 Pending 队列，`tools/d16_a04_review_queue.py` 新增稳定的分批输出：默认每批最多 25 条，并且按 `scope` 分组，不把历史内容、敏感内容和资产权利决定混在同一批。每条记录现在带 `batch_id`/`batch_number`，总输出带批次 ID、范围、数量和记录 ID 清单。

当前重新生成结果：

- `status=BLOCKED`；
- `pending_count=267`；
- `scope_counts={asset_history_and_authorization: 186, historical_content: 81}`；
- `batch_size=25`，`batch_count=12`；
- `tools/results/d16_a04_review_queue.json` 与 `.md` 已更新。

新增测试覆盖批次稳定性、范围不混合和人工批准策略；专项审计 **14/14 PASS**，工具全量回归 **51/51 PASS**。该工具只读整理证据入口，不写回 registry、不批准任何记录；267 条 Pending 仍是 A04 的真实阻断。

## 2026-08-12 完整项目根目录审计口径复验

为避免把 D15 的 Registry 范围误当成 D16 的完整项目范围，本轮以 `game` 作为 A04 审计根目录，并保留 `game/Assets/Resources/ContentRegistry` 作为 D15-S02 的独立范围。权威命令与结果如下：

```text
python tools/d16_a04_review_audit.py game --json-out tools/results/d16_a04_review_audit.json
  entry_count=3139
  sensitive_entry_count=91
  visual_asset_count=2918
  authorization_record_count=2918
  structural_error_count=0
  Approved=2780, ApprovedWithWarning=91, Pending=267, Rejected=0
  blocking_reasons=pending_historical_reviews
```

审计结果与 `tools/results/d16_a04_review_audit.json` 一致。此前较小范围的探索性调用不作为 D16-A04 结论；完整 `game` 根目录结果才是当前验收口径。A04 仍为 **IN_PROGRESS**，267 条 Pending 不得自动批准，也不得进入 D16-S01/S02。

## 2026-08-12 人工复核队列当前文件复验

重新运行 `python tools/d16_a04_review_queue.py game --json-out tools/results/d16_a04_review_queue.json --markdown-out tools/results/d16_a04_review_queue.md`，确认队列与当前完整 `game` 根目录审计一致：

- `status=BLOCKED`，`pending_count=267`，`batch_count=12`，默认每批 25 条；
- `asset_history_and_authorization=186`，`historical_content=81`；
- JSON 与 Markdown 队列均已刷新；工具只整理来源、证据、警告/跳过字段和人工决定项，不写回 Registry、不自动批准。

A04 继续 **IN_PROGRESS**；267 条仍需具备责任权限的审核者逐条附真实证据后处理。

## 2026-08-12 权威审计再次复验

`d16_a04_review_audit.py` 在完整 `game` 根目录再次输出：3,139 条记录、2,918 条授权记录、结构错误 0、Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0。机器阻断仍为 `pending_historical_reviews`；本轮没有自动批准或改写授权/历史证据，A04 继续 **IN_PROGRESS**。

## 2026-08-12 单条人工决定落盘工具

新增 `tools/d16_a04_apply_review_decision.py`，为 Pending 队列提供唯一的逐条写入入口。工具默认 dry-run，且每次只能处理一个 ID；只有同时提供真实存在的本地证据文件、责任审核者、明确决定和不少于 20 个字符的决定依据，才允许使用 `--apply` 写回。

安全约束：

- 不接受缺失证据、远程 URL 或隐式决定；
- 敏感条目不能直接 Approved，必须是 ApprovedWithWarning 或 Rejected；ApprovedWithWarning 必须明确 warning、skip 和表现等级；
- 只写 `historical_review`、审核人、证据引用和决定依据，不改 source/license/authorization、asset_state、玩法数值或其他条目；
- 使用临时文件 + flush/fsync + replace 写入，验证失败不产生任何修改；
- 不会批量批准，也不会将 `inferred` 或 `historical` 自动改成 `fictional`。

专项测试 `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**19/19 PASS**，覆盖 dry-run、单条正常写入、敏感内容约束和缺证据无副作用。随后执行 `python -m unittest discover -s tools/tests -p "test_*.py" -q`：**58/58 PASS**。当前真实项目没有提交任何新的人工决定，完整审计仍为 Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0；A04 继续 **IN_PROGRESS**。

## 2026-08-12 A04/A05 队列刷新

- 重新运行 A04 审计和人工队列：3,139 条记录、2,918 条授权记录、结构错误 0、Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0；队列为 12 批，按资产权利/历史内容分组，仍不自动批准。
- A04/A06 审计专项与 A05 替换队列测试合计 **22/22 PASS**；测试通过只证明工具的安全边界，不证明人工历史或授权决定已经完成。
- 本轮没有写回 Registry、历史审核、授权、表现状态或玩法数据；A04 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前人工复核包

- 当前机器审计仍为 3,139 条记录、Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0、结构错误 0。
- 最新只读队列：`tools/results/d16_a04_review_queue_current.json` 与 `docs/game/design/common/D16_A04_REVIEW_QUEUE_CURRENT.md`；267 条分为 12 批，每批最多 25 条，资产历史/授权 186 条、历史内容 81 条。
- 队列只整理来源、证据、警告/跳过字段和人工决定项；没有自动批准、没有改写历史标签/授权，也没有修改玩法数据。A04 继续 **IN_PROGRESS**，必须由责任审核者逐条提交真实证据后再使用单条原子写入工具。

## 2026-08-12 人工决定日期校验补强

`tools/d16_a04_apply_review_decision.py` 的单条写入入口现在要求 `review_date` 为有效的 ISO 日期（`YYYY-MM-DD`），并拒绝晚于本机当前日期的决定。非法或未来日期会在写入前返回 `INVALID`，不会改变条目的 `historical_review` 或任何其他字段。

验证：

- A04/A05/A06 专项：**22/22 PASS**；
- 工具全量：**71/71 PASS**；
- 临时样本覆盖非法日期、未来日期和无副作用；
- 当前真实项目仍为 Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0；没有新增人工决定。

该增量只强化审计时间线安全，不替代责任审核者、真实历史/敏感内容证据或授权决定；A04 仍 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 明确虚构安全子集当前复验

执行 `python tools/d16_a04_approve_explicit_fictional.py game/Assets/Resources/ContentRegistry --review-date 2026-08-12 --json-out tools/results/d16_a04_safe_fictional_preview_current.json`，结果为：

- `eligible_count=0`、`changed_count=0`；本轮没有写回任何 Registry 条目；
- 当前 Registry 范围内的 Pending 条目均没有同时满足“显式 `historical_tag=fictional` + 项目自创来源 + 内部许可证 + 本地证据文件 + 无敏感/警告”的完整条件；
- 完整 `game` 根目录 A04 审计仍为 Pending 267、Approved 2,780、ApprovedWithWarning 91、Rejected 0，A04 继续 **IN_PROGRESS**。

该结果只说明当前没有可由机器安全子门禁关闭的条目，不替代责任审核者的历史、敏感表现和外部资产决定，也不改变 D16-S01/S02 的启动条件。

## 2026-08-12 明确虚构安全子集干跑复核（当前任务）

再次执行 `python tools/d16_a04_approve_explicit_fictional.py game --json-out tools/results/d16_a04_explicit_fictional_dry_run_current_20260812.json`，使用默认 dry-run，不带 `--apply`：

- `eligible_count=0`、`changed_count=0`、`ineligible_pending_count=267`；
- 267 条 Pending 全部因缺少显式 `historical_tag=fictional` 依据而不具备自动处理资格；
- 没有写回 Registry、历史标签、授权、敏感内容字段或 `asset_state`。

结果文件：`tools/results/d16_a04_explicit_fictional_dry_run_current_20260812.json`。A04 仍为 **IN_PROGRESS**；该干跑不能替代责任审核者的逐条历史、敏感内容和权利证据。
## 2026-08-12 总纲顺序下的 A04 当前证据

完整 `game` 根目录审计：3,139 条记录、2,918 条授权记录、结构错误 0、Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0。当前人工队列为 12 批，资产历史/授权 186 条、历史内容 81 条；没有自动批准任何条目。A04 保持 **IN_PROGRESS**，D16-S01/S02 不启动。
