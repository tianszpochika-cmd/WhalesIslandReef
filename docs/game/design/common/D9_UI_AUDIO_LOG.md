# D9-A07 UI / 音频 / 日志接线

## 范围

本成果覆盖 D9 垂直切片中的航行 HUD、船舶状态、市场库存、近海鱼获、核心船员对话、任务日志、天气提示、可撤退海战状态、存档反馈和关键音效。

表现层只读以下来源：

| 表现 | 唯一读取来源 | 本任务写入 |
|---|---|---|
| 航行 HUD / 船舶状态 | D3RuntimeSnapshot、D9PortShipPreparationSnapshot | 无 |
| 市场 | D3RuntimeSnapshot.MarketStock | 无 |
| 鱼获 | D8 EventLog 的 `event_d9_fishing_catch` 事实 | 无 |
| 船员对话 | D2DialogueController.LastReply / `ReplyReceived` | 无 |
| 任务 | D4RuntimeSnapshot.Mainline | 无 |
| 天气提示 | D3RuntimeSnapshot.Weather | 无 |
| 战况 | D4RuntimeSnapshot.Combat | 无 |
| 存档反馈 | D8 EventLog 的 `event_d9_save_checkpoint` 事实 | 无 |
| 音效 | D8 EventLog 事实、天气状态迁移 | 无 |

## 运行时入口

- `game/Assets/Scripts/Gameplay/D9/D9VoyageHudController.cs`
  - InitialHarbor 自动创建 `D9VoyageHud`。
  - 每 0.12 秒刷新一次只读文字快照；不接收鼠标射线，不调用领域裁决器。
  - 显示重量、吃水、船员、货物、补给、损伤、锚泊、改装、市场库存、鱼获事实、对话回复、任务、天气和战况。
- `game/Assets/Scripts/Gameplay/D9/D9PresentationReadModel.cs`
  - 将 D3/D4/D9 快照和 EventLog 副本投影为 UI 文本。
  - `D9EventLogBridge` 是统一事实日志的只读注入点；A07 不创建 EventLog，也不提供私有日志回退。
- `game/Assets/Scripts/Gameplay/D9/D9AudioPresentation.cs`
  - InitialHarbor 自动创建 `D9AudioPresentationController`。
  - 缺少正式音频资源时仅生成表现层 fallback clip；不改变游戏状态。
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`
  - 增加只读 `Snapshot` 出口，继续由 D9 整备系统生成快照。
- `game/Assets/Scripts/Gameplay/D4/D4RuntimeBootstrap.cs`
  - 增加只读 `Snapshot` 出口，未复制 D4 主线/战斗/船坞逻辑。
- `game/Assets/Scripts/Gameplay/UI/Dialogue/D2DialogueController.cs`
  - 增加只读 `LastReply` 出口；关系提议仍由 D2 逻辑入口裁决。

## 统一事实与读档去重

日志 UI 只调用 D8 `EventLog.ReadFactsForUi`，展示 `FactEvent.EventId`、`LogSequence`、`Result.Summary` 和 `OwnerSystem`。它不调用 `Append`，不调用 `ClaimSettlement`，不从 UI 文字反推价格、伤害、奖励、天气或关系结果。

音频和 UI 事件游标均使用 `D9EventFactCursor`：

1. 首次绑定已有 EventLog 时跳过当前 `NextSequence` 之前的历史事实；历史日志仍可以展示。
2. 只消费后来新增的 `LogSequence`，同一事实不会重复触发提示音。
3. 读档绑定恢复后的 EventLog 时再次跳过已有序列，因此不会重复 UI 结算或音频结算。
4. EventLog 本身继续按 `event_id + save_revision` 去重；A07 不增加第二套去重键或第二份事实。

领域事件生产者在接入 A07 时调用 `D9EventLogBridge.Bind(restoredOrCurrentEventLog)`。没有统一事实源时页面明确显示“尚未接收到 D8 事实”，不会伪造 D9 私有日志。

## 冻结事件音效路由

| Event ID | Cue |
|---|---|
| `event_d9_departure_prepared` | departure_prepared |
| `event_d9_departure` | departure |
| `event_d9_fishing_catch` | fishing_catch |
| `event_d9_weather_storm` | weather_storm |
| `event_d9_crew_report` | crew_report |
| `event_d9_nearshore_patrol` | combat_warning |
| `event_d9_nearshore_retreat` | combat_retreat |
| `event_d9_treasure_clue` | treasure_clue |
| `event_d9_treasure_resolved` | treasure_resolved |
| `event_d9_returned_to_harbor` | returned_to_harbor |
| `event_d9_trade_settled` | trade_settled |
| `event_d9_wage_settled` | wage_settled |
| `event_d9_ship_repaired` | ship_repaired |
| `event_d9_save_checkpoint` | save_checkpoint |

未知事件不会发声。天气从平静/其他状态迁移到 Storm 或 Hurricane 时，音频只播放天气提示，不修改天气状态。

## 测试与结果

已执行：

- `dotnet build game/Assembly-CSharp.csproj --no-restore`：通过，0 警告，0 错误。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore`：通过，0 警告，0 错误。
- `dotnet test game/AgentCore/AgentCore.csproj --no-build`：通过。
- `npm run build`（工作目录 `server/`）：通过。
- `python tools/d9_vertical_slice_check.py`：通过，14 事件 / 6 UI 页面 / 10 商品冻结检查通过。
- `python -m unittest tools.tests.test_d9_vertical_slice_freeze -v`：1/1 通过。
- `git diff --check`：通过；仅有仓库既有换行风格提示。
- Systems 红线扫描：无网络或 LLM 代码命中；`SaveSystem.cs` 的注释命中已人工确认不是实现。

Unity Editor 测试入口：

- `game/Assets/Editor/D9A07UiAudioLogTests.cs`
- 覆盖快照投影、统一事实读取、恢复后游标去重、14 个冻结事件音效路由和 MonoBehaviour Unity Mock 挂载。

在隔离 Unity 2022.3 工程中，A07 脚本编译和 Editor 测试入口以退出码 0 完成；由于该隔离工程使用的旧 package lock 与当前 TextMeshPro/Unity 编辑器版本不一致，命令行 Test Runner 未生成 XML 结果文件。此项不是代码失败，但需要在项目当前 Unity 编辑器完成一次 Test Runner 结果落盘后作为 CI 证据补归档。

## 性能边界

- UI refresh：0.12 秒一次，约 8.3Hz；与 D9 冻结的 UI 2ms 预算相容。
- HUD 使用一个 overlay Canvas、四个只读 Text，无每帧物理射线和无网络调用。
- EventLog 每次只读最近 20 条事实副本。
- 音频使用一个 one-shot AudioSource；正式资源缺失时 fallback clip 只为听觉反馈，不参与强逻辑。
