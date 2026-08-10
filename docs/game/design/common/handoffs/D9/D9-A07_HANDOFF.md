# D9-A07 交接报告

## 任务结论

D9-A07 表现层接线已完成：航行 HUD、船舶状态、市场库存、近海鱼获、船员对话、任务日志、天气提示、战况、存档反馈和关键音效均有代码入口。所有入口只消费状态快照或 D8 统一事实，不拥有领域裁决权。

## 契约与数据来源

- AgentReply：沿用当前 AgentCore/`contracts/agent-reply.schema.json`，A07 只读 D2 已校验 `LastReply`，不显示或计算最终关系结果。
- WorldEvent / FactEvent：沿用 D8 EventLog v1，UI/音频读取 `EventLog.ReadFactsForUi` 的副本。
- 事件事实唯一去重键：`event_id + save_revision`；表现层新增消费游标使用 `LogSequence`，不替代领域幂等门。
- 状态快照：D3 `D3RuntimeSnapshot`、D4 `D4RuntimeSnapshot`、D9 `D9PortShipPreparationSnapshot`。

## 代码清单

- `game/Assets/Scripts/Gameplay/D9/D9VoyageHudController.cs`
  - D9 航行 HUD，读取 D3/D4/D9 快照、D2 对话和 EventLog。
- `game/Assets/Scripts/Gameplay/D9/D9PresentationReadModel.cs`
  - 只读投影、统一事实桥接和读档去重游标。
- `game/Assets/Scripts/Gameplay/D9/D9AudioPresentation.cs`
  - 14 个冻结 D9 事件的音效路由，以及 Storm/Hurricane 状态迁移提示音。
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`
  - 增加只读 D9 整备快照出口。
- `game/Assets/Scripts/Gameplay/D4/D4RuntimeBootstrap.cs`
  - 增加只读 D4 内容快照出口。
- `game/Assets/Scripts/Gameplay/UI/Dialogue/D2DialogueController.cs`
  - 增加只读最近回复出口。
- `game/Assets/Editor/D9A07UiAudioLogTests.cs`
  - A07 Editor / Unity Mock 测试入口。

## Action / 事件覆盖清单

| 流程 | 读取/提示 |
|---|---|
| 港口整备与市场 | D9 ship preparation snapshot、D3 market stock、`event_d9_trade_settled` |
| 离港与航行 | D3 location/destination/status、`event_d9_departure_prepared`、`event_d9_departure` |
| 近海鱼场 | `event_d9_fishing_catch` 的事实摘要 |
| 风暴 | D3 Weather snapshot、`event_d9_weather_storm` |
| 核心船员 | D2 `LastReply`、`event_d9_crew_report` |
| 可撤退海战 | D4 Combat snapshot、`event_d9_nearshore_patrol`、`event_d9_nearshore_retreat` |
| 寻宝 | `event_d9_treasure_clue`、`event_d9_treasure_resolved` |
| 返港与工资/修船 | D9/D3 snapshot、`event_d9_returned_to_harbor`、`event_d9_wage_settled`、`event_d9_ship_repaired` |
| 读档 | `event_d9_save_checkpoint` + `D9EventFactCursor` 历史跳过 |

## 拒绝/越权边界

- HUD 没有修改 money、price、damage、reward、weather、relation、cargo 或 crew 的代码路径。
- 日志和音频没有 `EventLog.Append`、`ClaimSettlement` 或任何私有事实构造。
- 未知事件没有音效映射；未知状态只显示枚举文本，不推导数值结果。
- Systems 目录未新增网络/LLM 代码；A07 所有 Unity 代码位于 Gameplay/UI/Editor。

## 测试命令与结果

```text
dotnet build game/Assembly-CSharp.csproj --no-restore       PASS (0 warning, 0 error)
dotnet build game/AgentCore/AgentCore.csproj --no-restore  PASS (0 warning, 0 error)
dotnet test game/AgentCore/AgentCore.csproj --no-build     PASS
npm run build                                               PASS (server/)
python tools/d9_vertical_slice_check.py                    PASS
python -m unittest tools.tests.test_d9_vertical_slice_freeze -v  PASS (1/1)
git diff --check                                            PASS
```

Unity 2022.3 隔离工程已完成 A07 运行时代码编译并退出码 0；由于旧 package lock 与当前 TextMeshPro/编辑器版本不匹配，命令行 Test Runner 未落盘 XML。可复跑测试类型和断言已保存在 `Assets/Editor/D9A07UiAudioLogTests.cs`，应在项目当前 Unity 编辑器中补跑并归档 XML。

## 接入注意

D8 事件生产者恢复/创建当前 EventLog 后，需要调用 `D9EventLogBridge.Bind(eventLog)`。A07 特意不在 UI 内 new EventLog 或生成 fallback 事实，以保持“日志只读统一事实”的契约。

## 后续责任

- D8 事件运行时接线任务：负责把当前/恢复后的统一 EventLog 注入 `D9EventLogBridge`，不由 A07 复制领域写入。
- 工具/CI 维护：在当前 Unity 版本补跑 `D9.EditorTests.D9A07UiAudioLogTests` 并保存 XML；不需要改变 A07 的裁决边界。
