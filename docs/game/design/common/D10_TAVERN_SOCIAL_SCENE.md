# D10-A06 酒馆社会场景

状态：已实现，等待 D10-S01 做全局关系/存档汇总。  
前置：D9-S02 已通过（blocked_count=0，冻结事件链通过）。  
实现边界：本任务只提供酒馆请求、目录、敏感内容门禁和 owner 编排，不调整最终经济、奖励或战斗平衡。

## 1. 入口与所有权

运行时入口是 game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs，系统入口是：

- D10TavernSocialSystem：酒馆场景目录、请求事实、警告、重复保护和 owner 编排。
- D10TavernConfiguration：本场景的请求参数和效果档案入口；价格字段是 Economy 的服务报价，不是酒馆直接扣款。
- D10CrewBehaviorSystem：健康、疲劳、行为状态和酒精事件的唯一写者。
- TradeEconomySystem：金钱扣款的唯一写者。
- D9PortShipPreparationSystem：招募/船员名册和靠泊条件的既有 owner。
- D10RelationSystem：舞会、夜话和酒后冲突关系边的唯一写者。
- MiniGameSystem：牌局规则、牌面、下注、胜负、债务和结算的后续 owner；酒馆不复制牌局逻辑。
- SaveSystem：运行时快照和共享 EventLog 的正式保存入口。

UI 只调用 Request*，再把已记录的 D10TavernRequest 交给 Resolve。UI、Agent 和 TavernSystem 都不能直接修改 money、health、fatigue、relation、reward 或 job outcome。

旧 D9 港口页的“酒馆”按钮现在只跳转到 D10 社会页；旧 VisitTavern API 保留给 D9 兼容测试，不作为 D10 运行时入口。

## 2. 可用行为

| 行为 | 请求入口 | 裁决 owner | 结果 |
|---|---|---|---|
| 进入酒馆 | EnterTavern | Tavern/EventLog | 记录进入事实，不写领域数值 |
| 招募 | RequestRecruitment | D9 Recruitment/Economy/Ship | 候选人、价格、容量和名册由 D9 owner 判断 |
| 休息 | RequestRest | D10CrewBehaviorSystem | 按小时降低疲劳并记录 CrewSystem 事实 |
| 吃饭 | RequestMeal | Economy + CrewSystem | Economy 扣餐费，CrewSystem 应用健康变化 |
| 饮酒 | RequestAlcohol | Economy + CrewSystem + Relation | 应用健康/疲劳，条件满足时触发可追溯冲突 |
| 传闻 | HearRumor | Tavern/EventLog | 保存文本、来源和可靠度，不把传闻当成事实结论 |
| 牌局入口 | RequestPokerEntry | MiniGameSystem | 只返回 D10PokerEntryIntent，不产生牌局结算 |
| 舞会/夜话 | RequestDance / RequestNightTalk | D10RelationSystem | 关系系统按事实事件裁决变化 |
| 副业公告板 | OpenSideJobBoard / RequestSideJobOffer | Tavern/后续 Job + Economy | 只展示和发起副业，不直接发收入或完成任务 |

运行时在 InitialHarbor 自动创建；按 V 打开/关闭，D9 港口页按 T 只负责跳转。快捷键为 Q/R/E/A/H/P/D/N/J/K/X，同样的按钮也可通过 UI 点击。

## 3. 酒精后果

酒精使用 D10TavernConfiguration.AlcoholEffect 返回的效果档案。当前档案只冻结入口形状，数值是现有垂直切片的服务报价/后果占位，不代表最终平衡：

| 档案 | Economy 报价 | 健康变化 | 疲劳变化 | 冲突风险 |
|---|---:|---:|---:|---:|
| alcohol_light | 2 (>=0) | -1 [-100,100] | +4 [0,100] | 10% [0,1] |
| alcohol_strong | 5 (>=0) | -3 [-100,100] | +8 [0,100] | 25% [0,1] |
| alcohol_binge | 8 (>=0) | -8 [-100,100] | +16 [0,100] | 50% [0,1] |

运行时默认按钮使用 DefaultRestHours=4 小时和 DefaultAlcoholStrength=Strong；两者也经过同一配置合法性检查。

流程为：

1. Tavern 先写 event_d10_tavern_request_drinkalcohol_* 请求事实。
2. D10CrewBehaviorSystem.ApplyAlcoholEffect 通过 CrewSystem clamp 健康/疲劳，写 AlcoholConsumed 事件，并记录 alcohol_profile、conflict_risk_percent、前后数值和 cause_event_ids。
3. 风险至少 25%，且船员士气低于 50 或已经消极怠工/拒绝执行时，向 D10RelationSystem 提交 Argument。关系事实的 cause_event_ids 同时包含酒馆请求事实和酒精后果事实。
4. D10TavernSocialSystem 只写带 owner 结果的酒馆 resolution fact；它不直接改 CrewAgent 或关系边。

因此酒精不是消费动画：健康、疲劳、行为状态和冲突均能从共享 EventLog 反查。

## 4. 传闻、牌局和副业

每条 D10TavernRumor 必须包含：

- RumorId、文本和 Tags；
- SourceRef、SourceKind；
- 0—1 的 Reliability；
- ReliabilityBand/ReliabilityLabel（较可靠、半可信、低可信）。

当前示例来源为码头装卸工、瞭望手和酒馆老板，可靠度分别为 0.72、0.56、0.34。来源和可靠度随请求事实保存，UI 不把传闻直接提升为任务事实。

牌局入口返回 TableId、RulesetId、StakeBand 和固定 SettlementOwner = MiniGameSystem。酒馆不调用牌局结算、不扣赌注、不生成胜负或债务。

公告板只返回发行人、类型、前置标签、风险标签、奖励标签和过期日。护卫副业标记 RequiresWarning，必须先确认警告；跳过通过 SkipSensitiveContent 完成，酒馆不发放副业收入。

## 5. 敏感内容

Alcohol、Gambling、Intimacy、ExploitativeWork 四类请求必须先收到 WarningRequired；用户可二次确认，也可调用 SkipSensitiveContent。护卫副业的警告明确说明暴力、伤病和法律后果。舞会/夜话只提供非露骨社交表达，不绕过同意和关系 owner。

## 6. 存档、幂等和测试入口

请求事实使用 event_d10_tavern_request_* + save_revision，结算事实使用 event_d10_tavern_resolution_* + save_revision。重复请求或重复结算只返回已有事实，不重复扣款、恢复疲劳、招募、改变关系或进入牌局。

Unity 运行时结算后调用 D3RuntimeBootstrap.CommitPreparationState() 保存 D3/Economy/船只状态；发生关系变化时把 D10RelationSystem 返回的快照交给 SaveSystem.SaveD10Runtime。CrewAgent 的 BehaviorState 随既有 crew save 编解码保存。

纯 C# 测试入口：

- game/Assets/Tests/EditMode/D10TavernSocialSystemTests.cs
- RumorsHaveSourceAndReliability_PokerOnlyCreatesEntryIntent
- StrongAlcoholRoutesHealthFatigueAndConflictThroughOwners
- RestMealRecruitNightTalkAndSensitiveSideJobUseExplicitEntrances

测试覆盖来源/可靠度、牌局无结算副作用、酒精健康/疲劳/冲突因果、休息、吃饭、招募、夜话、敏感副业警告和配置合法性检查。

## 7. 边界自查

- game/Assets/Scripts/Systems/D10/ 无 Unity、网络或 LLM 依赖。
- 没有新增牌局结算、奖励发放或关系直接写入。
- 船员关系变化绑定 cause_event_ids；关系 world event 的输入事实与因果事实参数已校正，避免把 cause ID 错放到 input_facts。
- 未修改最终经济或战斗公式；旧 D9 酒馆按钮不再绕过 D10 社会请求链。
