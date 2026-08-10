# D9-S01 任务交接报告

任务：D9-S01 冻结垂直切片范围
日期：2026-08-10
前置：D8-S02 已通过
状态：完成，允许 D9-A01～D9-A07 按冻结边界并行实施

## 交付内容

主成果：`docs/game/design/common/D9_VERTICAL_SLICE_FREEZE.md`

验收脚本：

- `tools/d9_vertical_slice_check.py`
- `tools/tests/test_d9_vertical_slice_freeze.py`

记忆日志：`.workbuddy/memory/2026-08-10.md`

本任务只冻结边界和集成入口，没有新增 Systems 领域逻辑、没有删除 D7 行为、没有新增跨端契约或单端协议。

## 固定垂直切片

| 范围 | 冻结样例/入口 |
|---|---|
| 港口 | `santa_margarita`，内容样例 `island_saint_marys`、`building_saint_marys_dock`，入口 `InitialHarbor.unity` |
| 村落 | `green_cay`，`D3LocationType.Village`，唯一村落逻辑节点 |
| 近海鱼场 | `nearshore_saint_marys` + `fish_mackerel_school`，预留工具 `gear_handline_basic` |
| 小船 | 内容 `ship_moonwake`，D7 兼容运行时类型 `small_fishing`，Prefab `ph_ship_fishing.prefab` |
| 商品 | `salt`、`sugar`、`tobacco`、`rum`、`coffee`、`cocoa`、`spices`、`dyes`、`timber`、`medicine` |
| 核心船员 Agent | `crew_isaac_reed`，使用 `DialogueSession`、`AgentReply v1.1`、`D2DialogueController` |
| 天气事件 | `event_d9_weather_storm`，`D3WeatherType.Storm`，owner `EnvironmentSystem` |
| 小型寻宝 | `quest_first_chart` → `reward_first_chart`，登陆点 `landmark:saint_marys_chart_cove`，无硬谜题门槛 |
| 可撤退海战 | `event_d9_nearshore_patrol`，敌方 `ph_spanish_patrol`，撤退结果 `D4CombatStatus.Escaped` |

`gear_handline_basic` 与 `landmark:saint_marys_chart_cove` 是预留逻辑引用，不是已创建的目录条目；D9-A02/D9-A06 实现前必须先按 D8 内容准入补齐来源、授权、owner 和 test_id。

## 冻结入口

- 场景只保留 D7 的 `MainMenu.unity`、`Bootstrap.unity`、`InitialHarbor.unity`；D9 不新增 Unity 场景。D3 的 `driftwood_isle` 仅保留兼容回归，不进入 D9 航线/UI。
- 视觉/碰撞入口只允许复用 `ph_ship_fishing`、`ph_crew_male`、`ph_palm_tree`、`ph_rock`、`ph_grass`。
- 运行时锚点固定为 `D2_PlayerAvatar`、`ph_ship_fishing`、`D7_HelmPoint`、`D7StatusCanvas`、`D3Canvas`、`D4Canvas`。
- D7 `D7ControlCoordinator`、`D7ShipDeckController`、`D7SaveCoordinator` 继续负责输入、上下船和保存编排；不复制这些职责。
- D3 入口为 `D3RuntimeBootstrap`、`CommodityCatalog`、`TradeEconomySystem`、`D3InventorySystem`、`WorldCatalog`。
- D4 战斗入口为 `D4RuntimeBootstrap` 和 `D4CombatSystem`；天气现有基线入口为 `WeatherCatalog`/`VoyageSettlementSystem`，后续天气状态写权归 EnvironmentSystem。
- Agent 入口为纯 C# `game/AgentCore/DialogueSession.cs`、`AgentModels.cs`、`AgentReplyValidator.cs`，Unity 仅通过 D2 对话表现接线。

## 事件和 UI 边界

冻结文件登记了 14 个唯一事件 ID，去重键固定为 `event_id + save_revision`。事件 owner 依次覆盖 ShipSystem、EcologySystem、EnvironmentSystem、CrewSystem、CombatSystem、QuestSystem、EconomySystem、ShipyardSystem 和 SaveSystem；UI、AgentCore、server gateway 不得成为事件 owner。

冻结 UI 页面共 6 个：`D9_VoyageHud`、`D9_PortServicePage`、`D9_CrewDialoguePage`、`D9_TreasureJournalPage`、`D9_CombatPage`、`D9_SaveRestoreFeedback`。所有页面 `writes=[]`，只能读取快照和统一事实事件。

## 性能预算

基线：1920×1080、60 FPS、单场景 `InitialHarbor`。

- 单帧 16.6 ms；领域/环境 4.0 ms；输入/UI 2.0 ms；物理 3.0 ms；渲染提交 6.0 ms；余量至少 1.6 ms。
- 活动动态对象 ≤32；可见占位环境对象 ≤128；单帧物理接触 ≤64。
- 无模型 Agent 模板回复 ≤250 ms；保存/读档反馈 ≤500 ms。

以上是 D9-A01～A07 必须测量并报告的上限，不是 S01 对尚未实现逻辑的性能宣称。

## 验收命令和结果

```text
python tools/d9_vertical_slice_check.py
  PASS: scenes=3, prefabs=5, commodities=10, events=14, ui_pages=6, agents=7

python -m unittest tools.tests.test_d9_vertical_slice_freeze -v
  PASS: 1/1

git diff --check
  PASS: 无 whitespace error
```

验收脚本还检查：D8 报告明确允许进入 D9、D8 内容 ID 存在、D3 10 商品与村落/风暴入口未漂移、D4 海战拥有撤退入口、事件 owner/ID 完整、UI 不拥有写权限、所有 D9 Agent 都有入口/样例/测试边界/性能预算，以及 `game/Assets/Scripts/Systems/` 没有可执行网络或 LLM 引用。

## 后续责任边界

- D9-A01～A07 只能在冻结文件规定的范围内实现；超出范围先更新冻结文件并重新验收。
- 新增鱼具、寻宝登陆点或其他内容必须经过 `contracts/`/ContentRegistry 及 D8 所有权准入；不得把预留引用直接当成已存在状态。
- Agent 只能提出对白、情绪、关系姿态和 P2A 意图；金钱、价格、伤害、天气、鱼获、任务和奖励继续由所属 Systems 裁决。
- D9-S02 的完整集成顺序固定为：整备 → 装货 → 配员 → 出航 → 钓鱼 → 遇雨/风暴 → 船员讨论 → 登陆寻宝 → 返回交易 → 发工资 → 修船 → 保存 → 读档。
