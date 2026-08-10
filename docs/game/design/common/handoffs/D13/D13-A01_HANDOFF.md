# D13-A01 交接报告：产权与资产交易

任务：`D13-A01`  
日期：2026-08-11  
状态：完成

## 已完成

- 新增 `D13PropertyAssetContracts.cs`：九类资产定义、产权状态、调查/估值/交易请求、法律与经济 authority 接口、快照与只读模型。
- 新增 `D13PropertyAssetSystem.cs`：调查、估值、购买、租赁、共同治理、抵押、出售、转让、继承、争议开立/解决、失败恢复、来源事件和幂等保护。
- PropertySystem 是产权状态唯一写者；没有 authority 时安全拒绝，不在本模块默认生成价格、金钱或法律归属。
- 新增 `D13PropertyAssetSystemTests.cs` 与 `tools/d13_a01_runner/`。
- SaveSystem/GameState/PlayerState 增加 `D13PropertyRuntimeSnapshot` 的 `location_json.d13` 保存和读取入口。
- 新增设计文档并登记到 `docs/README.md`。

## 对外接口

```text
D13PropertyAssetSystem.Investigate(D13PropertySurveyRequest)
D13PropertyAssetSystem.RequestValuation(D13PropertyValuationRequest)
D13PropertyAssetSystem.Purchase / Lease / CoGovern / Mortgage
D13PropertyAssetSystem.Sell / Transfer / Inherit
D13PropertyAssetSystem.OpenDispute / ResolveDispute
D13PropertyAssetSystem.GetReadModel()
D13PropertyAssetSystem.CaptureSnapshot() / Restore(snapshot)
```

`ID13PropertyLegalAuthority` 只返回法律/势力决定；`ID13PropertyEconomyAuthority` 只返回估值、账本和金额决定。PropertySystem 只在两者批准后更新产权状态。

## 运行验证

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162  # 0 warning / 0 error
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162       # 0 warning / 0 error
dotnet run --project tools/d13_a01_runner/D13A01Runner.csproj                 # 4/4 PASS
```

覆盖：九类资产、全操作生命周期、法律/经济拒绝、报价不可被 `OfferedAmount` 覆盖、来源事件、共享 EventLog 幂等和 D13 快照存档恢复。

## 交接边界与待办

1. D13-A02 接入真实 Finance/Economy authority，负责现金、抵押本金、利息、债务和账本；不得让 PropertySystem 改钱。
2. D13-A05 接入居民、社区协议和治理政策；`CoGovernanceRefs` 只是产权权利投影，不是人口状态。
3. D13-A06 接入建筑/仓库/船坞状态；资产权利只能作为读取条件。
4. D13-A03/A04/A07 分别接入农业、养殖、动态市场；生产、库存、价格和生态状态仍由对应 owner 写入。
5. D13-S01 集成时必须同时保存 `D13PropertyRuntimeSnapshot` 与共享 `EventLogSnapshot`，并验证贷款、生产、税费、维护、保险和生态后果不重复结算。

## 红线复核

- `game/Assets/Scripts/Systems/D13/` 无 LLM、HTTP、网络或 UnityWebRequest。
- UI/Agent 没有产权写入口；只读入口是 `GetReadModel()`。
- 产权、估值、经济、法律、库存和建筑状态没有在本模块复制。
- 事实事件保留法律/经济来源和因果引用；拒绝结果保留恢复路线。
