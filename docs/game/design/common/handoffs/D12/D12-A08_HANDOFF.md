# D12-A08 交接报告：动态任务与航海日志

日期：2026-08-11  
状态：完成，验收通过

## 交付内容

- 动态任务与日志契约：`game/Assets/Scripts/Systems/D12/D12DynamicQuestLogContracts.cs`
- 垂直切片任务目录：`game/Assets/Scripts/Systems/D12/D12DynamicQuestCatalog.cs`
- 任务/日志系统：`game/Assets/Scripts/Systems/D12/D12DynamicQuestLogSystem.cs`
- D12 运行时快照与存档接线：`game/Assets/Scripts/Systems/D12/D12MapDiscoveryContracts.cs`、`game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- EditMode 测试：`game/Assets/Tests/EditMode/D12DynamicQuestLogTests.cs`
- 独立运行器：`tools/d12_a08_runner/`
- 设计主成果：`docs/game/design/common/D12_DYNAMIC_QUEST_LOG.md`

## 新接口与责任

`D12DynamicQuestLogSystem` 对外提供 `Accept`、`ObserveFact`、`SelectAlternativeRoute`、`ReadQuest`、`ReadJournal`、`BuildAgentSummary` 和 `Restore`。输入事实必须已经由统一 `EventLog` 接收；系统不会接受一段文本来替代事实。

`QuestSystem` 只写任务状态、目标进度、事实引用、失败/恢复/替代路线和 pending `D12QuestRewardRequest`。`EventLog` 是日志事实唯一写者；`RewardSystem` 和经济、库存、关系、图鉴等 owner 负责最终奖励或领域状态。UI 只能读取结果/快照，Agent 只能读取事实摘要并提议。

## 关键流程

1. `Accept` 追加 `quest_accepted` 事实并将任务置为 `Active`。
2. `ObserveFact` 按 owner、结果码、值上下文和幂等键匹配目标；领域事实不被复制。
3. 失败结果置为 `Failed`，保留失败事实和默认恢复路线；恢复事实或替代路线选择置为 `Recovered`。
4. 所有目标完成后追加 `quest_completed` 事实，生成唯一 pending 奖励请求。
5. `ReadJournal` 从 EventLog 解析事实引用；Agent 摘要固定标记为仅事实、不可创建事实。

主线样例为 `quest_d12_saint_marys_chart_recovery`，包含海图发现→打捞成功和社区见证替代路线；支线样例为 `quest_d12_green_cay_reciprocity`。这套系统不复制 D9TreasureQuestSystem 的寻宝线索、经济、库存或关系状态。

## 保存与恢复

动态日志位于 `D12RuntimeSnapshot.DynamicQuestLog`，保存路径为 `location_json.d12.dynamicQuestLog`。缺少字段的旧存档得到空 schema v1 快照。恢复时校验定义 ID、目标/路线 ID、事实引用和奖励请求；相同 `event_id + save_revision` 再次观察返回 Duplicate，不会重复追加事实或请求奖励。

## 验证记录

- `dotnet run --project tools/d12_a08_runner/D12A08Runner.csproj --no-build`：D12-A08 tests PASS count=4。
- `dotnet run --project tools/d12_a02_runner/D12A02Runner.csproj`：4/4 PASS。
- `dotnet run --project tools/d12_a03_smoke/D12A03Smoke.csproj`：PASS。
- `dotnet run --project tools/d12_a04_smoke/D12A04Smoke.csproj`：PASS。
- `dotnet run --project tools/d12_a06_smoke/D12A06Smoke.csproj`：PASS。
- `dotnet run --project tools/d12_a07_runner/D12A07Runner.csproj`：5/5 PASS。
- `dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：0 warning / 0 error。
- `python tools/d9_s02_acceptance_check.py`：D9-S02 PASS，`blocked=0`，14 frozen events。
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：79 entries / 13 prefixes PASS。

## 下游注意

- 下游奖励消费者应按 `RequestId` 或 `SourceEventId + SourceSaveRevision` 幂等领取，并在各自 owner 内写最终结果。
- 下游日志/UI 不应直接读取任务定义推断完成；应读取任务结果或 EventLog 事实。
- D12-S01 可消费本任务输出的任务快照、日志事实引用和 pending 奖励请求，但不能把任务系统变成经济、库存、关系或奖励的第二写者。
