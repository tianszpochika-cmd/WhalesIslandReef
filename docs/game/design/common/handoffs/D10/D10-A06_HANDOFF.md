# D10-A06 交接报告

任务：D10-A06 酒馆社会场景  
前置：D9-S02 已通过（复核仍为 blocked_count=0，冻结事件链通过）  
状态：完成，交给 D10-S01 做全局关系/存档汇总。

## 主成果

- 文档：docs/game/design/common/D10_TAVERN_SOCIAL_SCENE.md
- 酒馆请求/数据契约：game/Assets/Scripts/Systems/D10/D10TavernContracts.cs
- 酒馆 owner 编排：game/Assets/Scripts/Systems/D10/D10TavernSocialSystem.cs
- InitialHarbor 运行时/UI：game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs
- 定向测试：game/Assets/Tests/EditMode/D10TavernSocialSystemTests.cs

## 新增配置

配置入口为 D10TavernConfiguration，当前入口值保持垂直切片既有行为，不是最终经济平衡：

| 配置 | 默认值/边界 | 单位 | owner | 测试入口 |
|---|---:|---|---|---|
| RestMinHours / RestMaxHours | 1 / 12，必须为正且 Max 不超过 24 | 小时 | CrewSystem | D10TavernConfiguration.Validate |
| DefaultRestHours | 4，必须在 RestMinHours—RestMaxHours 内 | 小时 | Tavern 请求入口 | D10TavernConfiguration.Validate |
| DefaultAlcoholStrength | Strong，必须是 D10AlcoholStrength 枚举 | 枚举 | Tavern 请求入口 | D10TavernConfiguration.Validate |
| MealPriceQuote | 3，必须 >= 0 | 金币报价 | EconomySystem | RestMealRecruitNightTalkAndSensitiveSideJobUseExplicitEntrances |
| MealHealthDelta | +2 | 健康点 | CrewSystem | 同上 |
| alcohol_light | 2 / -1 / +4 / 0.10；报价 >=0，健康 [-100,100]，疲劳 [0,100]，风险 [0,1] | 金币报价 / 健康 / 疲劳 / 风险 | Economy + Crew + Relation | StrongAlcoholRoutesHealthFatigueAndConflictThroughOwners |
| alcohol_strong | 5 / -3 / +8 / 0.25；报价 >=0，健康 [-100,100]，疲劳 [0,100]，风险 [0,1] | 金币报价 / 健康 / 疲劳 / 风险 | Economy + Crew + Relation | 同上 |
| alcohol_binge | 8 / -8 / +16 / 0.50；报价 >=0，健康 [-100,100]，疲劳 [0,100]，风险 [0,1] | 金币报价 / 健康 / 疲劳 / 风险 | Economy + Crew + Relation | D10TavernConfiguration.Validate |
| rumor reliability | 0.34 / 0.56 / 0.72，范围 0—1 | 可靠度 | Tavern/EventLog | RumorsHaveSourceAndReliability_PokerOnlyCreatesEntryIntent |
| sensitivity | Alcohol/Gambling/Intimacy/ExploitativeWork | 枚举 | Tavern/UI 门禁 | 三项酒馆测试 |
| poker settlement owner | MiniGameSystem | owner ID | MiniGameSystem | 牌局入口测试 |

每个传闻还登记了来源引用、来源类别、可靠度等级和标签；每个副业登记了发布者、前置、风险、奖励标签和过期日。

## 替换的硬编码/入口

- 新增酒馆价格与酒精后果的统一入口，D10 酒馆系统不再在行为方法中散落价格、健康、疲劳或冲突常量。
- D10 UI 的所有行为改为 Request → EventLog fact → Resolve → owner 结果链；UI 不直接写领域字段。
- D9 港口页的旧“酒馆/T”按钮改为跳转 D10 社会页，不再从活动 UI 调用旧的直接改士气/疲劳路径；旧 D9 VisitTavern API 保留用于兼容。
- 修正 D10RelationSystem 构造关系 WorldEvent 的参数顺序：请求 cause_event_ids 现在进入 FactEvent.CauseIds，input_facts 保留关系输入事实。该修正保证舞会、夜话和酒后冲突的关系变化都可追溯到事件。
- D10CrewBehaviorSystem 增加 AlcoholConsumed 事件、酒精档案、风险百分比和统一 ApplyAlcoholEffect 入口；关系系统仍是关系变化唯一写者。

本任务没有调整 D9 最终价格、奖励、工资、航速、战斗伤害或牌局结算公式；酒馆报价和后果只是已批准入口档案。

## 行为与边界

- 招募委托 D9 Recruitment/Economy/Ship owner。
- 休息、吃饭和饮酒的健康/疲劳委托 CrewSystem；金钱委托 EconomySystem。
- 饮酒风险达到门槛且船员低士气/怠工/拒绝时，向 D10RelationSystem 提交 Argument；关系变化的 cause_event_ids 同时含酒馆请求和 AlcoholConsumed 事实。
- 牌局入口只返回 MiniGameSystem intent，不复制结算。
- 副业公告板只发起 offer，不直接发收入或任务结果。
- 敏感行为先返回 WarningRequired；可二次确认或 SkipSensitiveContent。
- Systems/D10 无 Unity、网络或 LLM；Runtime/UI 是请求适配层，不能成为数值裁决者。

## 配置检查命令

在项目根目录执行：

    dotnet build game/Whalebone.Systems.csproj --no-restore
    dotnet build game/Save.Tests.csproj --no-restore
    dotnet build game/Assembly-CSharp.csproj --no-restore
    dotnet run --project tools/d10_a06_runner/D10A06Runner.csproj

其中最后一条是本轮临时 NUnit 定向执行器，执行结束后已移除，不作为交付文件；正式测试仍在 Unity EditMode 测试程序集内。

配置合法性检查由 D10TavernConfiguration.Validate 执行，定向测试最后断言返回空错误列表；其检查包括 schema、休息小时、默认枚举、非负报价、健康 [-100,100]、疲劳 [0,100] 和 0—1 冲突风险。

## 测试结果

- Whalebone.Systems：0 warning / 0 error。
- Save.Tests：0 warning / 0 error。
- Assembly-CSharp：0 warning / 0 error。
- D10-A06 定向测试：3/3 PASS。
  - RumorsHaveSourceAndReliability_PokerOnlyCreatesEntryIntent
  - StrongAlcoholRoutesHealthFatigueAndConflictThroughOwners
  - RestMealRecruitNightTalkAndSensitiveSideJobUseExplicitEntrances
- D9-S02 前置门禁：PASS，blocked_count=0。
- git diff --check：通过（仅显示工作区既有换行风格提示，无新增空白错误）。

## D10-S01 接线注意

1. 将 D10TavernSocialSystem、D10CrewBehaviorSystem、D10RelationSystem 绑定到同一个恢复后的 EventLog。
2. 关系保存继续使用 SaveSystem.SaveD10Runtime；不要在 Tavern/UI 侧创建第二份关系存档。
3. 牌局入口消费 D10PokerEntryIntent，结算仍由 MiniGameSystem 生产事实和经济结果。
4. 副业公告板要接入 Job owner 的 offer/accept/complete 流程；TavernSystem 不能临时发钱。
5. 若调整酒馆报价或酒精后果，修改 D10TavernConfiguration 并同步参数说明/测试，不要在 UI 或 owner 编排中新增常量。
