# D9 垂直切片冻结

任务：D9-S01
版本：`d9.vertical_slice.v1`
日期：2026-08-10
前置：`docs/game/design/common/D8_ACCEPTANCE_REPORT.md` 已确认 D8 通过并允许进入 D9。
状态：冻结，可供 D9-A01～D9-A07 直接引用。

## 1. 冻结结论

D9 只做一条可保存、可恢复的最小航程：

```text
圣玛格丽特港整备
→ 装载 10 个商品、配员、出航
→ 近海鲭鱼鱼场捕捞
→ 遇到一次风暴天气事件
→ 与核心船员 Agent 讨论航线/岗位建议
→ 可选遭遇一场可撤退海战
→ 登陆绿湾村完成一次小型寻宝
→ 返回港口交易、发工资、修船
→ 保存、读档并验证事实不重复结算
```

固定范围只有：一座港口、一个村落、一片近海鱼场、一艘小船、10 个商品、1 个核心船员 Agent、1 个天气事件、1 个小型寻宝、1 场可撤退海战。D9-S01 不实现上述领域逻辑，只冻结入口、样例、状态边界和验收口径。

### 1.1 机器可读冻结清单

下列 JSON 是本文件的唯一机器可读摘要；后续 D9 Agent 修改范围、入口、样例或预算时，必须先更新本文件并重新运行验收脚本。

<!-- D9_FREEZE_MANIFEST_BEGIN -->
```json
{
  "task_id": "D9-S01",
  "freeze_version": "d9.vertical_slice.v1",
  "scene_boundary": {
    "entry_scene": "game/Assets/Scenes/InitialHarbor.unity",
    "preserved_scenes": [
      "game/Assets/Scenes/MainMenu.unity",
      "game/Assets/Scenes/Bootstrap.unity",
      "game/Assets/Scenes/InitialHarbor.unity"
    ],
    "logical_nodes": [
      {"id": "santa_margarita", "kind": "harbor", "entry": "PortCatalog.StartingPortId"},
      {"id": "green_cay", "kind": "village", "entry": "PortCatalog.GreenCayId"},
      {"id": "nearshore_saint_marys", "kind": "nearshore_fishing_ground", "content_ref": "fish_mackerel_school"},
      {"id": "saint_marys_chart_cove", "kind": "treasure_landing_site", "status": "reserved_logical_ref"}
    ],
    "excluded_legacy_nodes": ["driftwood_isle"],
    "rule": "D9-S01 不新增 Unity 场景；港口、村落、鱼场、寻宝登陆点是 InitialHarbor 下的逻辑节点或航程 location_ref。D7 三个场景入口和已有行为保持不变。"
  },
  "prefab_entries": [
    {"content_id": "asset_ph_ship_fishing", "path": "game/Assets/Prefabs/Placeholder/ph_ship_fishing.prefab", "scene_object": "ph_ship_fishing", "owner": "ShipSystem"},
    {"content_id": "asset_ph_crew_male", "path": "game/Assets/Prefabs/Placeholder/ph_crew_male.prefab", "scene_object": "D2_PlayerAvatar/crew_visual", "owner": "CrewSystem"},
    {"content_id": "asset_ph_palm_leaves", "path": "game/Assets/Prefabs/Placeholder/ph_palm_tree.prefab", "scene_object": "D9_HarborVegetation", "owner": "AssetRegistry"},
    {"content_id": "asset_ph_rock", "path": "game/Assets/Prefabs/Placeholder/ph_rock.prefab", "scene_object": "D9_ShoreMarkers", "owner": "AssetRegistry"},
    {"content_id": "asset_ph_grass", "path": "game/Assets/Prefabs/Placeholder/ph_grass.prefab", "scene_object": "D9_ShoreMarkers", "owner": "AssetRegistry"}
  ],
  "data_samples": {
    "harbor": {"runtime_id": "santa_margarita", "content_id": "island_saint_marys", "building_id": "building_saint_marys_dock", "entry": "D3RuntimeBootstrap + ContentRegistryReader"},
    "village": {"runtime_id": "green_cay", "type": "D3LocationType.Village", "entry": "D3RuntimeBootstrap destination marker", "single_instance": true},
    "nearshore_fishing_ground": {"runtime_id": "nearshore_saint_marys", "content_id": "fish_mackerel_school", "habitat": "nearshore/reef_edge", "gear_ref": "gear_handline_basic", "gear_registration_owner": "D9-A02"},
    "ship": {"content_id": "ship_moonwake", "legacy_runtime_type": "small_fishing", "prefab_content_id": "asset_ph_ship_fishing", "entry": "D2ShipController + D7ShipDeckController"},
    "commodities": [
      {"id": "salt", "name": "盐"},
      {"id": "sugar", "name": "糖"},
      {"id": "tobacco", "name": "烟草"},
      {"id": "rum", "name": "朗姆酒"},
      {"id": "coffee", "name": "咖啡"},
      {"id": "cocoa", "name": "可可"},
      {"id": "spices", "name": "香料"},
      {"id": "dyes", "name": "染料"},
      {"id": "timber", "name": "木材"},
      {"id": "medicine", "name": "药材"}
    ],
    "core_crew_agent": {"content_id": "crew_isaac_reed", "prefab_content_id": "asset_ph_crew_male", "entry": "AgentCore.DialogueSession + D2DialogueController", "template_reply_required": true},
    "weather_event": {"event_id": "event_d9_weather_storm", "weather": "D3WeatherType.Storm", "owner_system": "EnvironmentSystem", "entry": "WeatherCatalog + WorldEvent schema"},
    "treasure": {"quest_id": "quest_first_chart", "reward_request_id": "reward_first_chart", "site_ref": "landmark:saint_marys_chart_cove", "entry": "QuestSystem + EventLog", "puzzle_gate": false},
    "combat": {"event_id": "event_d9_nearshore_patrol", "enemy_id": "ph_spanish_patrol", "owner_system": "CombatSystem", "entry": "D4CombatSystem.BeginEncounter / Escape", "retreat_status": "D4CombatStatus.Escaped"}
  },
  "event_chain": [
    {"event_id": "event_d9_departure_prepared", "owner_system": "ShipSystem", "test_id": "D9-S01-EVENT-001"},
    {"event_id": "event_d9_departure", "owner_system": "ShipSystem", "test_id": "D9-S01-EVENT-002"},
    {"event_id": "event_d9_fishing_catch", "owner_system": "EcologySystem", "test_id": "D9-S01-EVENT-003"},
    {"event_id": "event_d9_weather_storm", "owner_system": "EnvironmentSystem", "test_id": "D9-S01-EVENT-004"},
    {"event_id": "event_d9_crew_report", "owner_system": "CrewSystem", "test_id": "D9-S01-EVENT-005"},
    {"event_id": "event_d9_nearshore_patrol", "owner_system": "CombatSystem", "test_id": "D9-S01-EVENT-006"},
    {"event_id": "event_d9_nearshore_retreat", "owner_system": "CombatSystem", "test_id": "D9-S01-EVENT-007"},
    {"event_id": "event_d9_treasure_clue", "owner_system": "QuestSystem", "test_id": "D9-S01-EVENT-008"},
    {"event_id": "event_d9_treasure_resolved", "owner_system": "QuestSystem", "test_id": "D9-S01-EVENT-009"},
    {"event_id": "event_d9_returned_to_harbor", "owner_system": "ShipSystem", "test_id": "D9-S01-EVENT-010"},
    {"event_id": "event_d9_trade_settled", "owner_system": "EconomySystem", "test_id": "D9-S01-EVENT-011"},
    {"event_id": "event_d9_wage_settled", "owner_system": "EconomySystem", "test_id": "D9-S01-EVENT-012"},
    {"event_id": "event_d9_ship_repaired", "owner_system": "ShipyardSystem", "test_id": "D9-S01-EVENT-013"},
    {"event_id": "event_d9_save_checkpoint", "owner_system": "SaveSystem", "test_id": "D9-S01-EVENT-014"}
  ],
  "ui_pages": [
    {"id": "D9_VoyageHud", "entry": "D7RuntimeStatusHud + D3RuntimeBootstrap", "writes": [], "test_id": "D9-S01-UI-001"},
    {"id": "D9_PortServicePage", "entry": "D3Canvas / D3RuntimeBootstrap", "writes": [], "test_id": "D9-S01-UI-002"},
    {"id": "D9_CrewDialoguePage", "entry": "D2DialogueController / DialogueSession", "writes": [], "test_id": "D9-S01-UI-003"},
    {"id": "D9_TreasureJournalPage", "entry": "EventLog + QuestSystem snapshot", "writes": [], "test_id": "D9-S01-UI-004"},
    {"id": "D9_CombatPage", "entry": "D4Canvas / D4RuntimeBootstrap", "writes": [], "test_id": "D9-S01-UI-005"},
    {"id": "D9_SaveRestoreFeedback", "entry": "D7SaveCoordinator + HarborPauseController", "writes": [], "test_id": "D9-S01-UI-006"}
  ],
  "performance_budget": {
    "resolution": "1920x1080",
    "target_fps": 60,
    "frame_ms": 16.6,
    "cpu_ms": {"domain_and_environment": 4.0, "input_and_ui": 2.0, "physics": 3.0, "render_submission": 6.0, "headroom": 1.6},
    "active_dynamic_objects": 32,
    "visible_placeholder_objects": 128,
    "physics_contacts_per_frame": 64,
    "template_agent_reply_ms": 250,
    "save_restore_ms": 500,
    "single_loaded_d9_scene": "InitialHarbor"
  },
  "agent_boundaries": {
    "D9-A01": {"entry_points": ["game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs", "game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs", "game/Assets/Scripts/Systems/D4/D4ShipyardSystem.cs", "game/Assets/Scripts/Gameplay/D7/D7SaveCoordinator.cs"], "samples": ["santa_margarita", "building_saint_marys_dock", "ship_moonwake"], "test_boundary": "D9-A01-PORT-SHIP-PREP", "budget": "performance_budget.cpu_ms and active_dynamic_objects"},
    "D9-A02": {"entry_points": ["game/Assets/Scripts/Systems/D3/D3InventorySystem.cs", "game/Assets/Scripts/Systems/D4/D4EcologySystem.cs", "game/Assets/Resources/ContentRegistry/entries/fish/fish_mackerel_school.json"], "samples": ["fish_mackerel_school", "nearshore_saint_marys", "gear_handline_basic"], "test_boundary": "D9-A02-FISHING", "budget": "performance_budget.physics_contacts_per_frame"},
    "D9-A03": {"entry_points": ["game/Assets/Scripts/Systems/D3/CommodityCatalog.cs", "game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs", "game/Assets/Scripts/Systems/D3/D3InventorySystem.cs"], "samples": ["commodities[0..9]", "santa_margarita", "green_cay"], "test_boundary": "D9-A03-TRADE-10-COMMODITIES", "budget": "performance_budget.cpu_ms.domain_and_environment"},
    "D9-A04": {"entry_points": ["game/AgentCore/DialogueSession.cs", "game/AgentCore/AgentModels.cs", "game/AgentCore/AgentReplyValidator.cs", "game/Assets/Scripts/Gameplay/UI/Dialogue/D2DialogueController.cs"], "samples": ["crew_isaac_reed", "agent_reply.v1.1", "event_d9_crew_report"], "test_boundary": "D9-A04-CREW-AGENT", "budget": "performance_budget.template_agent_reply_ms"},
    "D9-A05": {"entry_points": ["game/Assets/Scripts/Systems/D3/WorldCatalog.cs", "game/Assets/Scripts/Systems/D3/VoyageSettlementSystem.cs", "game/Assets/Scripts/Systems/D3/D3Contracts.cs"], "samples": ["event_d9_weather_storm", "D3WeatherType.Storm", "InitialHarbor"], "test_boundary": "D9-A05-WEATHER", "budget": "performance_budget.cpu_ms.domain_and_environment"},
    "D9-A06": {"entry_points": ["game/Assets/Scripts/Systems/Event/EventLog.cs", "game/Assets/Scripts/Systems/D4/D4MainlineSystem.cs", "game/Assets/Scripts/Systems/D9/D9TreasureQuestSystem.cs", "game/Assets/Resources/ContentRegistry/entries/quest/quest_first_chart.json"], "samples": ["quest_first_chart", "reward_first_chart", "item_chart_compass", "landmark:saint_marys_chart_cove"], "test_boundary": "D9-A06-TREASURE", "budget": "performance_budget.save_restore_ms"},
    "D9-A07": {"entry_points": ["game/Assets/Scripts/Gameplay/D7/D7RuntimeStatusHud.cs", "game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs", "game/Assets/Scripts/Gameplay/D4/D4RuntimeBootstrap.cs", "game/Assets/Scripts/Gameplay/UI/Dialogue/D2DialogueController.cs"], "samples": ["D9_VoyageHud", "D9_PortServicePage", "D9_CombatPage"], "test_boundary": "D9-A07-UI-AUDIO-LOG", "budget": "performance_budget.frame_ms"}
  },
  "dedupe_key": "event_id + save_revision",
  "forbidden_owners": ["UI", "AgentCore", "server gateway"]
}
```
<!-- D9_FREEZE_MANIFEST_END -->

## 2. 场景边界冻结

### 2.1 Unity 场景

| 场景 | D9 用途 | 处理规则 |
|---|---|---|
| `MainMenu.unity` | D7 进入入口 | 保留现有主菜单行为，不在 D9 添加领域状态 |
| `Bootstrap.unity` | D7 全局启动壳 | 保留 `GameManager` 和现有启动顺序 |
| `InitialHarbor.unity` | D9 唯一玩法入口 | 承载港口、船、玩家、UI 接线和逻辑 location_ref |

D9-S01 不新增 Unity 场景、不拆出第二套港口、不复制船员或船的实体。村落 `green_cay`、近海鱼场 `nearshore_saint_marys` 和寻宝登陆点 `saint_marys_chart_cove` 是 `InitialHarbor` 之下的逻辑节点；如果后续 Agent 需要空间标记，只能作为该场景内的子节点或由现有 D3 marker 生成，不得建立平行场景入口。

### 2.2 逻辑边界

- 起点和回程港：D3 `santa_margarita`，内容样例用 `island_saint_marys` 与 `building_saint_marys_dock`。
- 唯一村落：D3 `green_cay`，类型必须是 `D3LocationType.Village`。
- 唯一鱼场：`nearshore_saint_marys`，只绑定 `fish_mackerel_school`，不把 D4 legacy `fish_school` marker 当成鱼群数量。
- 唯一小船：内容 ID `ship_moonwake`；D7 存档/运行时的 `small_fishing` 是兼容别名，不是第二艘船。
- D3 现有 `driftwood_isle` 继续保留给 D7/D3 兼容回归，但明确排除在 D9 航线和 UI 入口之外；它不是 D9 的第二个村落、鱼场或寻宝点。
- D3 现有 Calm/TradeWind/Rain/Hurricane 状态继续保留为 D7/D3 基线；D9 只冻结 `Storm` 这一个天气事件，不扩展天气内容范围。
- 海战和寻宝均为可选分支，但成功、失败、撤退和恢复都必须回到同一航程事实链。

## 3. Prefab 入口冻结

只允许复用 D8 注册且 D7 已加载的占位入口：

- `ph_ship_fishing.prefab`：小船视觉与碰撞入口；`D2ShipController`、`D7ShipDeckController` 负责表现接线，ShipSystem 负责航行/船体裁决。
- `ph_crew_male.prefab`：核心船员视觉占位；`crew_isaac_reed` 是数据 ID，Persona/关系/健康不写入 Prefab。
- `ph_palm_tree.prefab`、`ph_rock.prefab`、`ph_grass.prefab`：港口/村落/近岸环境占位，只能作为 AssetRegistry 内容使用。

固定运行时锚点：`D2_PlayerAvatar`、`ph_ship_fishing`、`D7_HelmPoint`、`D7StatusCanvas`、`D3Canvas`、`D4Canvas`。Prefab 和 UI 不能写钱、价格、伤害、天气、鱼群、任务、奖励或关系结果。

## 4. 数据样例冻结

### 4.1 内容注册表样例

必须优先读取以下 D8 内容 ID，不在 UI、Agent 或任务脚本内重新定义：

| 切片对象 | canonical ID | D7 兼容/逻辑 ID | 唯一写者 |
|---|---|---|---|
| 港口岛屿 | `island_saint_marys` | `santa_margarita` | IslandSystem / D3 位置兼容投影 |
| 港口码头 | `building_saint_marys_dock` | D3 起始港服务 | BuildingSystem / ShipyardSystem |
| 小船 | `ship_moonwake` | `small_fishing` | ShipSystem |
| 核心船员 | `crew_isaac_reed` | D7 老水手兼容投影 | CrewSystem |
| 鱼群 | `fish_mackerel_school` | `fish_school` 只作 legacy marker | EcologySystem |
| 寻宝任务 | `quest_first_chart` | D3/D4 任务兼容入口 | QuestSystem |
| 奖励请求 | `reward_first_chart` | 不直接发放 | RewardSystem |

`gear_handline_basic` 和 `landmark:saint_marys_chart_cove` 是 D9 预留逻辑引用：D9-A02/D9-A06 在实现时必须先按 D8 内容准入补齐来源、授权、owner、test_id 和契约/目录入口；D9-S01 不伪造条目，也不把预留引用当成已存在的奖励或鱼获结果。

### 4.2 商品样例

D9 只允许使用现有 `CommodityCatalog.All` 的 10 个商品：

`salt` 盐、`sugar` 糖、`tobacco` 烟草、`rum` 朗姆酒、`coffee` 咖啡、`cocoa` 可可、`spices` 香料、`dyes` 染料、`timber` 木材、`medicine` 药材。

商品定义由 `CommodityCatalog` 提供，货物数量/重量/体积由 Inventory/Cargo 写，价格/需求/税由 Economy/Market 写；本冻结不添加第 11 个商品，也不在任务或 AgentReply 内保存价格副本。

## 5. 事件链冻结

所有事实事件遵循 D8 `WorldEvent` 契约，去重键固定为 `event_id + save_revision`。AgentReply 只能作为 `event_d9_crew_report` 的提议输入，不能直接追加事实或修改结果数值。

| 顺序 | 事件 | owner_system | 必须覆盖的边界 |
|---:|---|---|---|
| 1 | `event_d9_departure_prepared` | ShipSystem | 船、货物、补给、船员岗位均来自现有快照 |
| 2 | `event_d9_departure` | ShipSystem | 从 `santa_margarita` 出航，保留 D7 控制权与保存入口 |
| 3 | `event_d9_fishing_catch` | EcologySystem | 只结算一片 `fish_mackerel_school` 鱼场；捕捞结果进入 Inventory/Cargo |
| 4 | `event_d9_weather_storm` | EnvironmentSystem | 只触发一次 D9 风暴；Ship/Ecology 读取天气，不写天气 |
| 5 | `event_d9_crew_report` | CrewSystem | Agent 提议航线/岗位/关系姿态；事实摘要可追溯，不含最终数值结果 |
| 6 | `event_d9_nearshore_patrol` | CombatSystem | 只使用 `ph_spanish_patrol`；战斗可进入 `Engaged` |
| 7 | `event_d9_nearshore_retreat` | CombatSystem | `D4CombatStatus.Escaped` 是合法恢复结果，不视为胜利或失败奖励 |
| 8 | `event_d9_treasure_clue` | QuestSystem | `quest_first_chart` 的传闻/线索可靠度，不使用必须猜中的谜题 |
| 9 | `event_d9_treasure_resolved` | QuestSystem | 成功或失败都写事实；奖励只能生成 `RewardRequest` |
| 10 | `event_d9_returned_to_harbor` | ShipSystem | 回到 `santa_margarita`，不复制航行结果 |
| 11 | `event_d9_trade_settled` | EconomySystem | 10 商品交易、库存和价格均由所属系统裁决 |
| 12 | `event_d9_wage_settled` | EconomySystem | 发工资是经济事实，不由 UI 或 Agent 计算 |
| 13 | `event_d9_ship_repaired` | ShipyardSystem | 修船消耗和船体恢复由船坞/船舶系统裁决 |
| 14 | `event_d9_save_checkpoint` | SaveSystem | 写入 D8 save metadata；读档不得重复结算上述事实 |

分支规则：海战可撤退后继续寻宝或返港；寻宝失败必须保留线索/危险事实并提供返港恢复；任何中断都通过 D8 `interrupted → recovered` 因果链恢复，不创建私有任务日志。

## 6. UI 页面清单冻结

| 页面/Overlay ID | 现有入口 | 允许读取 | 禁止写入 |
|---|---|---|---|
| `D9_VoyageHud` | `D7RuntimeStatusHud`、`D3RuntimeBootstrap` | 位置、船体、补给、货舱、天气、事实摘要 | 所有领域结果 |
| `D9_PortServicePage` | `D3Canvas`、`D3RuntimeBootstrap` | 市场快照、10 商品、港口服务状态 | 钱、价格、库存、修船结果 |
| `D9_CrewDialoguePage` | `D2DialogueController`、`DialogueSession` | 角色快照、AgentReply、事件摘要 | 关系、岗位、航速、任务结果 |
| `D9_TreasureJournalPage` | `EventLog`、QuestSystem snapshot | 线索、可靠度、前置、失败恢复和奖励请求 | 任务完成、奖励发放 |
| `D9_CombatPage` | `D4Canvas`、`D4RuntimeBootstrap` | 战斗状态、可撤退状态、事实日志 | 伤害、船体、战利品、法律结果 |
| `D9_SaveRestoreFeedback` | `D7SaveCoordinator`、`HarborPauseController` | 保存/读档状态和迁移错误 | 任何状态裁决 |

页面、音频和日志只消费状态快照与统一事件事实；不存在 D9 私有事实表。D7 的暂停、ESC 模态优先级、角色/船控制权继续由 `D7ControlCoordinator` 负责。

## 7. 性能预算冻结

验收基线为 1920×1080、目标 60 FPS、单个 `InitialHarbor` 场景加载。预算是 D9 的上限，不是本任务宣称已经测得的结果；D9-A01～A07 必须在自己的测试中报告实际值。

| 项目 | 预算 |
|---|---:|
| 单帧总预算 | 16.6 ms |
| 领域与环境裁决 | ≤ 4.0 ms |
| 输入与 UI 更新 | ≤ 2.0 ms |
| 物理与碰撞 | ≤ 3.0 ms |
| 渲染提交 | ≤ 6.0 ms |
| 预留余量 | ≥ 1.6 ms |
| 同时活动动态对象 | ≤ 32 |
| 可见占位环境对象 | ≤ 128 |
| 单帧物理接触数 | ≤ 64 |
| 无模型 Agent 模板回复 | ≤ 250 ms |
| 保存/读档反馈 | ≤ 500 ms |

超出预算时优先减少同时活动对象、刷新频率和表现层开销，不降低事件去重、存档一致性或强逻辑裁决质量。D9 不进行全世界模拟、不加载第二个玩法场景、不在 Systems 增加网络/LLM。

## 8. D9 Agent 入口矩阵

| Agent | 入口 | 样例 | 测试边界 | 性能预算 |
|---|---|---|---|---|
| D9-A01 | D3RuntimeBootstrap、TradeEconomySystem、D4ShipyardSystem、D7SaveCoordinator | 港口/码头/小船 | `D9-A01-PORT-SHIP-PREP` | CPU、活动对象 |
| D9-A02 | D3InventorySystem、D4EcologySystem、鱼群 ContentRegistry | 鲭鱼鱼场、手线 | `D9-A02-FISHING` | 物理接触 |
| D9-A03 | CommodityCatalog、TradeEconomySystem、D3InventorySystem | 10 商品、港口/村落 | `D9-A03-TRADE-10-COMMODITIES` | 领域 CPU |
| D9-A04 | AgentCore DialogueSession/Validator、D2DialogueController | `crew_isaac_reed`、AgentReply v1.1 | `D9-A04-CREW-AGENT` | 模板回复 |
| D9-A05 | WorldCatalog、D3Contracts、VoyageSettlementSystem | `event_d9_weather_storm` | `D9-A05-WEATHER` | 环境 CPU |
| D9-A06 | EventLog、D4MainlineSystem、Quest ContentRegistry | `quest_first_chart`、`reward_first_chart` | `D9-A06-TREASURE` | 保存/读档 |
| D9-A07 | D7RuntimeStatusHud、D3/D4 RuntimeBootstrap、D2DialogueController | 6 个 D9 页面 | `D9-A07-UI-AUDIO-LOG` | 单帧预算 |

所有后续 Agent 必须遵守：跨端数据先进入 `contracts/`；UI、Agent、server gateway 不拥有强逻辑写权；新增实体/事件只能使用本冻结中的 ID 或先走 D8 内容/契约准入；不得删除 D7 行为。

## 9. 验收脚本

入口：`tools/d9_vertical_slice_check.py`

运行：

```text
python tools/d9_vertical_slice_check.py
python -m unittest tools.tests.test_d9_vertical_slice_freeze -v
```

脚本检查：D8 前置、机器可读冻结清单、场景和 Prefab 入口、D8 内容 ID、D3 10 商品、D3 村落/天气、D4 可撤退战斗、事件唯一性与 owner、UI 入口、D9 Agent 矩阵、性能预算，以及 Systems 网络/LLM 红线。它不运行 Unity，不声称替代后续 D9 领域实现测试。
