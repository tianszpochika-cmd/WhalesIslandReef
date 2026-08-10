# D9 垂直切片同步验收报告

任务：`D9-S02`  
日期：2026-08-10  
项目：鲸骨航线

## 最终结论

**D9 通过，允许 D10、D11、D12 并行开始。**

D9-S02 已完成冻结流程的真实系统串联：

`整备 → 装货 → 配员 → 出航 → 钓鱼 → 遇雨/风暴 → 船员讨论 → 登陆寻宝 → 返回交易 → 发工资 → 修船 → 保存 → 读档`

通过依据是：静态验收门禁 14/14 事件生产者通过；纯 C# 全流程集成测试通过；SQLite D9 任务快照与共享 EventLog 读写通过；重复捕捞、交易、工资、修船和寻宝发现均验证了幂等；Systems 网络/LLM 红线通过。

主 Unity 工程当前仍被已有 Unity 实例占用，因此本轮没有强杀实例或伪造 Editor Test Runner XML。Unity 源码和生成程序集编译通过，主工程释放锁后只需补跑已有 EditMode XML 归档，不改变本次 D9 通过结论。

## 1. 同步实现

- `D9PortShipPreparationSystem` 统一整备、离港、工资、修船事实，并将离港事实接到 D3 成功出航之后。
- `D9FishingSystem` 使用 D4 `EcologySystem` 的鱼群位置/生物量预览与提交；使用 D3 `D3InventorySystem` 写鱼获；使用 `TradeEconomySystem` 写价格、市场和金币。
- `D4CombatSystem` 写近海巡逻与撤退事实；`EnvironmentSystem` 写风暴与海况快照；`D9CrewEventSystem` 读取已落盘事实后调用 AgentCore，并由 CrewSystem 写船员报告。
- `D9TreasureQuestSystem` 写线索和寻宝结算，只生成 `RewardRequest`，不直接发放奖励。
- `VoyageSettlementSystem` 写返港事实；`SaveSystem` 写保存检查点并序列化 `EventLogSnapshot`、D9 任务快照；`D9VerticalSliceRuntimeBootstrap` 负责运行时共享 EventLog、恢复任务/天气和提交 D9 保存。
- `D9EventLogBridge` 仍是只读表现层入口；UI、音频、AgentCore 和 gateway 没有事实写权限。

## 2. 冻结事件与唯一写者

所有事实去重键均为 `event_id + save_revision`，由 `EventLog.Append` 统一拦截重复键。

| event_id | 唯一写者 |
|---|---|
| `event_d9_departure_prepared` | ShipSystem / `D9PortShipPreparationSystem` |
| `event_d9_departure` | ShipSystem / `D9PortShipPreparationSystem` |
| `event_d9_fishing_catch` | EcologySystem / `D9FishingSystem` |
| `event_d9_weather_storm` | EnvironmentSystem |
| `event_d9_crew_report` | CrewSystem / `D9CrewEventSystem` |
| `event_d9_nearshore_patrol` | CombatSystem / `D4CombatSystem` |
| `event_d9_nearshore_retreat` | CombatSystem / `D4CombatSystem` |
| `event_d9_treasure_clue` | QuestSystem / `D9TreasureQuestSystem` |
| `event_d9_treasure_resolved` | QuestSystem / `D9TreasureQuestSystem` |
| `event_d9_returned_to_harbor` | ShipSystem / `VoyageSettlementSystem` |
| `event_d9_trade_settled` | EconomySystem / `TradeEconomySystem` |
| `event_d9_wage_settled` | EconomySystem / `D9PortShipPreparationSystem` |
| `event_d9_ship_repaired` | ShipyardSystem / `D9PortShipPreparationSystem` |
| `event_d9_save_checkpoint` | SaveSystem |

## 3. 验收结果

| 验收项 | 结果 | 证据 |
|---|---|---|
| 14 个冻结事件均有生产写者 | PASS | `tools/d9_s02_acceptance_check.py --json`，`blocked_count=0` |
| 全流程 13 阶段 | PASS | 静态流程门禁与 `D9S02VerticalSliceIntegrationTests` |
| 鱼群位置/生物量唯一写者 | PASS | `D4EcologySystem.SeedFishSchool/MoveFishSchool/CommitFishCatch` |
| 价格/市场/金币唯一写者 | PASS | `TradeEconomySystem`；捕捞系统不计算售价 |
| 捕捞重复结算 | PASS | 同一捕捞请求第二次返回 `duplicate_settlement`，货舱数量不变 |
| 交易重复结算 | PASS | 同一交易修订号第二次返回 `DuplicateSettlement`，金币不变 |
| 工资重复结算 | PASS | 同一工资修订号第二次返回 `Duplicate`，金币不变 |
| 修船重复结算 | PASS | 同一修船修订号第二次返回 `Duplicate`；修复门前置避免误报 `already_repaired` |
| 寻宝重复发现 | PASS | 读档后再次登陆返回 `AlreadyDiscovered`，事实数不增加 |
| 保存/读档 | PASS | D9 任务快照写入 `location_json.d9`；D3 共享 EventLog 保存并恢复 |
| UI/Agent 无写权 | PASS | 表现层只读快照/事实；Agent 只写记忆摘要和 typed proposal |
| Systems 网络/LLM 红线 | PASS | `game/Assets/Scripts/Systems/` 扫描无网络或 LLM 代码 |
| D7 行为兼容 | PASS | 未删除 D7；D3/D7 保存入口继续有效 |

## 4. 问题、责任模块与修复建议

| 问题 | 责任模块 | 本轮处理/建议 |
|---|---|---|
| Unity Runtime Agent 镜像缺少 `RecordFactSummary` | AgentCore Unity 镜像 | 已补齐与纯 C# AgentCore 对齐的记忆摘要接口；后续修改 AgentCore 时继续保持两份镜像同步 |
| 修船重复请求先返回“已完整” | D9-A01 / Shipyard 整备入口 | 已将 `event_d9_ship_repaired + save_revision` 幂等门前移；后续所有有状态结算入口都应先查重复键 |
| D9 运行时缺少统一组装入口 | D9-S02 | 已新增 `D9VerticalSliceRuntimeBootstrap`；主 Unity 工程释放锁后复跑场景级测试 |
| 主 Unity 工程被外部实例占用 | 环境/工具链，不归因 D9 代码 | 释放锁后补跑 EditMode XML；不得强杀用户实例或以静态结果冒充场景测试 |

## 5. 检查命令与结果

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

python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
# PASS: 20 entries / 13 prefixes

git diff --check
# PASS
```

D9 集成测试文件：`game/Assets/Tests/EditMode/D9S02VerticalSliceIntegrationTests.cs`，包含 2 项测试：全流程共享 EventLog/重复结算，以及 D9 任务快照与 EventLog SQLite 读档。独立运行结果为 `D9-S02_TESTS_PASS count=2`。

## 6. D10-D12 入口

D10、D11、D12 可以并行使用现有入口，不得复制领域逻辑：

- D10：从 `D9VerticalSliceRuntimeBootstrap.TreasureQuestSystem` 和 `RewardRequest` 读取任务结果；奖励仍由 RewardSystem 裁决。
- D11：从 `D9FishingSystem`、`D4EcologySystem` 和 `TradeEconomySystem` 读取鱼群/鱼获/价格；新增内容继续走 D8 内容注册表。
- D12：从 `D9EventLogBridge`、`EventLogSnapshot` 和 `D9EventFactCursor` 读取已恢复事实；UI 继续只读。
