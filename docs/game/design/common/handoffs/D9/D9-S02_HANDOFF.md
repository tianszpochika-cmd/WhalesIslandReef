# D9-S02 交接报告

任务：`D9-S02`  
日期：2026-08-10  
状态：**PASS**  
放行：**允许 D10、D11、D12 并行开始**

## 主成果

- 验收报告：[D9_ACCEPTANCE_REPORT.md](../D9_ACCEPTANCE_REPORT.md)
- 验收脚本：`tools/d9_s02_acceptance_check.py`
- 验收测试：`tools/tests/test_d9_s02_acceptance.py`
- 集成测试：`game/Assets/Tests/EditMode/D9S02VerticalSliceIntegrationTests.cs`
- 运行时协调入口：`game/Assets/Scripts/Gameplay/D9/D9VerticalSliceRuntimeBootstrap.cs`

## ID 前缀与内容入口

D9 沿用 D8-A02 的 13 个前缀：

`ship_`、`crew_`、`island_`、`building_`、`item_`、`animal_`、`plant_`、`fish_`、`quest_`、`reward_`、`outfit_`、`pet_`、`asset_`

本切片入口：

- `ship_moonwake` / `small_fishing`
- `crew_isaac_reed`
- `fish_mackerel_school`
- `nearshore_saint_marys`
- `item_gear_handline_basic`，兼容逻辑引用 `gear_handline_basic`
- `item_fresh_mackerel`、`item_salted_mackerel`、`item_cooked_mackerel`
- `quest_first_chart`、`reward_first_chart`
- `landmark:saint_marys_chart_cove`
- `ph_spanish_patrol`

正式目录仍位于 `game/Assets/Resources/ContentRegistry/`，UI 不维护内容目录。

## 事件 owner 链

| 事件 | owner |
|---|---|
| `event_d9_departure_prepared` / `event_d9_departure` | ShipSystem |
| `event_d9_fishing_catch` | EcologySystem |
| `event_d9_weather_storm` | EnvironmentSystem |
| `event_d9_crew_report` | CrewSystem |
| `event_d9_nearshore_patrol` / `event_d9_nearshore_retreat` | CombatSystem |
| `event_d9_treasure_clue` / `event_d9_treasure_resolved` | QuestSystem |
| `event_d9_returned_to_harbor` | ShipSystem |
| `event_d9_trade_settled` / `event_d9_wage_settled` | EconomySystem |
| `event_d9_ship_repaired` | ShipyardSystem |
| `event_d9_save_checkpoint` | SaveSystem |

所有者通过同一个 `EventLog` 写事实，幂等键固定为 `event_id + save_revision`。UI、音频、AgentCore、server gateway 均无写权。

## 已完成同步

1. 整备、装货、配员、出航：D9-A01 与 D3 共用 Economy/EventLog；D3 成功进入 `Sailing` 后记录离港事实。
2. 钓鱼：EcologySystem 写鱼群位置、生物量和捕捞压力；Inventory 写鱼获；Economy 写鱼获价格与出售结算。
3. 天气：EnvironmentSystem 写风暴状态，并投影至 D3 `WeatherSea`。
4. 船员：AgentCore 读取事实摘要，只产生对白/typed proposal；CrewSystem 记录 `event_d9_crew_report`，不直接执行提议。
5. 寻宝：QuestSystem 记录线索/结算，成功只生成 RewardRequest，不直接发奖励。
6. 返回、工资、修船：各自 owner 追加事实并有重复结算门。
7. 保存、读档：D3 快照携带 EventLog；D9 快照写入 `location_json.d9`；运行时入口加载 D9 任务和 D3 WeatherSea，恢复后绑定 `D9EventLogBridge`。

## 检查命令与结果

```text
python tools/d9_s02_acceptance_check.py --json
# PASS, blocked_count=0, frozen_event_count=14

python tools/d9_vertical_slice_check.py
# PASS: scenes=3, prefabs=5, commodities=10, events=14, ui_pages=6, agents=7

python -m unittest tools.tests.test_d9_s02_acceptance \
  tools.tests.test_d9_vertical_slice_freeze \
  tools.tests.test_content_registry_check -v
# 10/10 passed

dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
dotnet build game/Save.Tests.csproj --no-restore -v:minimal
dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal
# 均 0 warning / 0 error
```

额外的 D9 集成执行结果：`D9-S02_TESTS_PASS count=2`。覆盖完整流程、共享 EventLog、鱼获/价格/天气/船员/任务状态、重复捕捞/交易/工资/修船、SQLite 保存读档和读档后重复寻宝保护。

主 Unity 编辑器因已有实例占用未强杀；释放锁后补跑 EditMode XML 归档即可，不改变 D9 代码验收结论。

## 后续 D9/D10-D12 可用入口

- D10 任务/奖励：`D9VerticalSliceRuntimeBootstrap.TreasureQuestSystem`、`D9TreasureQuestSnapshot.RewardRequest`。
- D11 生态/经济：`D9FishingSystem`、`D4EcologySystem`、`TradeEconomySystem`，不得复制鱼群位置或价格公式。
- D12 表现/日志：`D9EventLogBridge.Current`、`D9EventFactCursor`、`D9PresentationReadModel`，只能读取当前或恢复后的统一 EventLog。
- 保存：`D9VerticalSliceRuntimeBootstrap.SaveProgress()` 与 `SaveSystem.SaveD9Runtime/LoadD9Runtime`。

## 后续注意事项

- D10-D12 不得新建第二个 EventLog、第二套经济/生态/奖励逻辑或 UI 内容目录。
- 主 Unity 工程重新导入脚本后，应确认生成工程收录新增 `D9VerticalSliceRuntimeBootstrap.cs`，然后归档 EditMode 测试 XML。
- 任何新增结算入口必须先检查 `event_id + save_revision`，再检查“已完成”状态，避免重放返回普通业务失败码。
