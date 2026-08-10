# D13-A02 交接报告：金融、信用与保险

任务：`D13-A02`  
日期：2026-08-11  
状态：完成

## 已完成

- 新增 `D13FinanceConfiguration.cs`：统一利率、滞纳金、展期、保险、宽限、催收、破产保护和信用配置，并执行配置校验。
- 新增 `D13FinanceContracts.cs`：账户、现金、债务、担保、抵押、保险、证据、理赔、催收、查封、破产保护、风险说明、读模型和快照契约。
- 新增 `D13FinanceSystem.cs`：FinanceSystem 是账户、债务、利息、保险状态唯一写者；实现贷款/赊账、还款、利息、到期、展期、保险、证据、拒赔/部分赔付、违约、催收、查封请求和破产保护。
- 接入 `SaveSystem`、`GameState`、`PlayerState` 的 `location_json.d13Finance` 存档路径，保存恢复账户、债务、担保、证据、保单、理赔、查封和已应用事实键。
- 新增 `D13FinanceSystemTests.cs` 与 `tools/d13_a02_runner/`，覆盖成功、失败恢复、来源验证、风险解释和读档幂等。
- 新增主设计文档 `D13_FINANCE_CREDIT_INSURANCE.md` 并登记 README。

## 对外接口

```text
D13FinanceSystem.OpenAccount / Deposit / Withdraw / Spend / Receive
D13FinanceSystem.RequestLoan / RequestCredit / AddGuarantee
D13FinanceSystem.AccrueInterest / EvaluateDue / Repay / Renew
D13FinanceSystem.QuoteInsurance / BuyInsurance / RegisterEvidence / SubmitClaim
D13FinanceSystem.Collect / RequestAssetSeizure
D13FinanceSystem.RequestBankruptcyProtection / ReleaseBankruptcyProtection
D13FinanceSystem.GetReadModel()
D13FinanceSystem.CaptureSnapshot() / Restore(snapshot)
```

`ID13FinanceCollateralAuthority` 只返回 Property/Economy/Legal authority 的抵押估值和查封决定；实际资产所有权仍由 `PropertySystem` 写入。`FinanceSystem` 不写 D3 现金镜像、库存、产权、声望、关系或奖励。

## 运行验证

```text
dotnet build game/Whalebone.Agent.Runtime.csproj --no-restore -t:Rebuild -p:NoWarn=0162  # 0/0
dotnet build game/Whalebone.Content.Runtime.csproj --no-restore -t:Rebuild -p:NoWarn=0162 # 0/0
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162         # 0/0
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162               # 0/0
dotnet build game/AgentCore/AgentCore.csproj --no-restore                                # 0/0
dotnet run --project tools/d13_a02_runner/D13A02Runner.csproj --no-restore                # 3/3 PASS
```

覆盖：贷款本金和现金、赊账不发现金、利息、展期费用、担保/抵押、破产保护、到期/违约/信用变化、催收通知费、法律查封来源、保险报价、证据不足拒赔、证据满足后的部分赔付、保存恢复和相同 `event_id + save_revision` 去重。

## 接入边界与待办

1. D13-A01/后续 PropertySystem 只消费 `approved_pending_property_owner` 查封请求并裁决实际产权，不能让 FinanceSystem 直接改所有权。
2. EconomySystem 应消费金融事件或提供正式 `EconomySystem` 来源事实，统一承担外部资金、估值和价格裁决；本模块不写 D3 旧现金镜像。
3. Inventory/Cargo、RewardSystem、RelationSystem 和 QuestSystem 只消费带 `source_event_id` 的金融事实，不从 Finance 快照复制自己的状态。
4. UI 以 `D13FinanceReadModel` 展示本金、利息、费用、风险、证据门槛和最坏结果；不能把结果对象转成直接写操作。

## 红线复核

- `game/Assets/Scripts/Systems/D13/` 未加入 LLM、HTTP、网络或 `UnityWebRequest`。
- FinanceSystem 未写 PropertySystem 产权状态、D3 现金兼容镜像、库存、奖励或关系。
- 外部金钱/估值/法律决定必须有 owner 和共享 EventLog 来源事实；失败保留 recovery route。
- 所有可结算金融操作使用 `event_id + save_revision` 幂等保护；读档不会重复添加现金、利息或赔付。
