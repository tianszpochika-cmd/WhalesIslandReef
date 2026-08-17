# D19 逻辑边界基线（D19-L01）

> 任务：`D19-L01`（波次 0，D19 唯一冻结任务）
> 冻结日期：2026-08-17
> 前置：`D18-L53 LOGIC_PASS`（含 D18-L50 重核 LOGIC_PASS）已满足
> 本文件是 D19 全章唯一权威边界基线。**后续 D19-L02—L48 必须从本文件出发**；不阅读任何旧 D19-Rxx/RERUN 文档也能独立工作。
> 生效依据：[D18—D24 纯逻辑完成路线](../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md)、[单任务逻辑开发协议](../../LOGIC_TASK_PROTOCOL.md)、[D19 章节任务卡](../../AGENT_DEVELOPMENT_CHAPTER_11_D19.md)。

## 0. 读取顺序与本文用途

1. 先读 [D19 章节任务卡](../../AGENT_DEVELOPMENT_CHAPTER_11_D19.md)（任务卡与波次）→ 本文 → [D18 只读交接接口](D18_LOGIC_HANDOFF_INTERFACE.md)（L46，可消费键与红线）。
2. 本文冻结五项内容：
   - D2/D3/D4/D9/Save 现有公开接口盘点（命令、读模型、事件、ID、拥有者、禁止直写字段）；
   - D19 可编排范围、唯一裁决者、禁止写入红线；
   - PortVisit/泊位/港口服务/登船/离港/返港/命令幂等/事件/保存投影的边界；
   - 可消费的 D18 稳定键与全部 Runtime Deferred 项；
   - 旧 D19-Rxx/RERUN 的 SUPERSEDED 隔离清单。

## 1. 现状盘点（2026-08-17 磁盘事实，只读）

### 1.1 D2 航海（`game/Assets/Scripts/Systems/D2/`）

| 公开入口 | 类型 | 说明 |
|---|---|---|
| `SailingSystem.ApplySnapshot` / `SetControlMode` / `Tick` | 命令 | 航行状态机入口；`D2ControlMode`（Character/Ship/Locked） |
| `D2Contracts`：`SailingInput`/`SailingSnapshot`/`DayNightSnapshot`/`D2RuntimeSnapshot` | 读模型/契约 | 输入与快照语义；表现层只传递输入意图 |

- **拥有者**：`SailingSystem`（航行状态、控制模式）；`D2Contracts` 定义快照字段。
- **禁止直写字段**：`SailingSnapshot` 内部数值（速度/位置/方向等）、`D2RuntimeSnapshot` 任何字段——D19 不得直接修改。

### 1.2 D3 贸易/库存（`game/Assets/Scripts/Systems/D3/`）

| 公开入口 | 类型 | 说明 |
|---|---|---|
| `TradeEconomySystem.Buy / Sell / Quote / CanSpend / TrySpend / ApplyExternalMarketFlow / EnsureMarketItem / RegisterMarketItem / CaptureEventLogSnapshot / RestoreEventLog` | 命令/读模型 | 市场买卖与货币裁决；报价与价格原因（`MarketQuote`/`PriceReason`） |
| `D3InventorySystem.CanAddCargo / TryAddCargo / TryRemoveCargo / TryAddPersonal / TryRemovePersonal / CountCargo / CountPersonal / FreeCargo / FreeWeight / UsedCargo / UsedVolume / UsedWeight / RegisterCargoDefinition` | 命令/读模型 | 货舱/个人物品容量与归属 |
| `D3Contracts`：`D3TradeState`（Money/Inventory/Market）、`PortDefinition`（Id/DisplayName/Type/WorldX/WorldZ/TariffRate）、`CommodityDefinition`、`MarketQuote`、`TradeResult`、`VoyageState`/`VoyageSettlement` 等 | 契约 | 唯一字段事实源 |

- **拥有者**：`TradeEconomySystem`（货币、价格、市场流）；`D3InventorySystem`（货舱/个人物品容量、数量、归属）。
- **禁止直写字段**：`D3TradeState.Money`、`Inventory`、`Market` 任何字段——D19 只能通过 `Buy/Sell/TrySpend` 等命令改变，不得 `new D3TradeState` 覆盖或直接改字段。

### 1.3 D4 船舶/船员（`game/Assets/Scripts/Systems/D4/`）

| 公开入口 | 类型 | 说明 |
|---|---|---|
| `D4ShipyardSystem.TryUpgrade / TryUnlockShip / Level / GetModifiers` | 命令/读模型 | 船舶升级/解锁/改装；`ShipModifiers`/`ShipyardResult` |
| `D4Catalogs`：`D4ShipCatalog`/`D4FactionCatalog`/`D4AnimalCatalog`/`D4DiseaseCatalog`（`Find`/`CreateDefault`） | 只读目录 | 船舶/势力/动物/疾病定义 |
| `D4Contracts`：`ShipState`、`CombatState`、`CrewHealthState`、势力/生态/医疗契约 | 契约 | 唯一字段事实源 |

- **拥有者**：`D4ShipyardSystem`（船舶数值/改装/解锁）；船员状态由其领域系统（D10/D4 船员契约）裁决。
- **禁止直写字段**：`ShipState` 结构/数值（耐久/改装/武器等）——D19 只能经 `TryUpgrade` 等公开命令，不得越权修改船舶数值。

### 1.4 D9 港口准备（`game/Assets/Scripts/Systems/D9/`）

| 公开入口 | 类型 | 说明 |
|---|---|---|
| `D9PortShipPreparationSystem.EnterPort / AssignBerth / Buy / BuySupply / Repair / Recruit / VisitTavern / SettleWages / Upgrade / PrepareDeparture / Depart / RecordDeparture / BuildSnapshot` | 命令/读模型 | 港口服务准备的全部现有公开命令 |
| `D9PortShipPreparationContracts`：`D9PortShipPreparationSnapshot`（PortId/InPort/Moored/BerthId/Money/Services）、`D9PreparationResult`、`D9ShipPreparationSnapshot` | 契约 | 港口准备读模型与结果 |
| `D9RuntimeContracts`：`D9RuntimeSnapshot`（`Validate`/`Clone`） | 契约 | 运行时快照 |

- **拥有者**：`D9PortShipPreparationSystem`（港口准备事实：进港/泊位/补给/修理/招募/工资/离港准备）。
- **禁止直写字段**：`D9PortShipPreparationSnapshot` 字段——D19 适配器只能调用命令并读取快照，不得改写。

### 1.5 Save（`game/Assets/Scripts/Systems/Save/`）

| 公开入口 | 类型 | 说明 |
|---|---|---|
| `SaveSystem.Save / Load / NewGame / HasSave / MigrateToV3` | 命令 | 存档主入口（磁盘 JSON，`GameState` 根模型） |
| `SaveSystem.LoadD3Runtime / LoadD4Runtime / LoadD9Runtime / LoadD2… / CaptureD3TradeEventLog / RestoreD3TradeEventLog / AppendD9SaveCheckpoint / SaveD10…/LoadD10…/…` | 投影接口 | 各领域存档投影；D19 使用既有 `LoadD9Runtime` 等投影，不新建私有存档格式 |
| `SaveFileSafety` / `SaveMigration` / `GameState` / `SeedData` / `CrewAgentCodec` | 基础设施 | 安全写盘、迁移、根模型 |

- **拥有者**：`SaveSystem`（存档文件、迁移、投影注册）；各领域快照由各自系统生产（`BuildSnapshot`）。
- **禁止直写字段**：`GameState` 根模型与磁盘 JSON 结构——D19 只通过 `SaveSystem` 公开投影接口落盘。

### 1.6 事件基础设施

- `Systems/Event/EventLog`：D19 港口事件（L16）必须接入既有事件日志（命令 ID 关联、可重放），不另起事件总线。

## 2. 冻结边界（D19 编排矩阵）

| D19 主题 | D19 可编排什么（调用公开契约） | 唯一裁决者（保持不变） | D19 禁止写 |
|---|---|---|---|
| PortVisit（到港） | `D9PortShipPreparationSystem.EnterPort`；`D3 VoyageState` 只读 | D9 港口准备（访问事实）；D3 航程状态 | `D3TradeState`/`D9PortShipPreparationSnapshot` 字段；航程数值 |
| 泊位（Berth） | `D9.AssignBerth`；D19-L06 纯逻辑预留/占用/冲突/租期（新 D19 领域，锚点 ID 来自 L04） | D9 泊位分配命令；D19-L06 泊位逻辑（新） | Unity 坐标、`Transform`、场景锚点；泊位数量/租期数值越权 |
| 港口服务（市场/仓库/酒馆/船坞/补给） | `D3.Buy/Sell/Quote`、`D3Inventory.*`、`D9.BuySupply/Repair/Recruit/VisitTavern/SettleWages/Upgrade`（经 D19-L11—L15 适配器封装） | D3 货币/库存/价格；D4 船舶数值；D9 服务准备 | 货币、库存容量/归属、船舶耐久/改装数值的直接写入 |
| 登船/下船（Boarding） | D19-L08 成员位置语义（新），只读船员 ID/位置 | 成员位置事实的裁决者（D20 领域，登船状态机属 D19 编排） | 角色/动物外观、碰撞、NavMesh、`Transform` |
| 离港（Departure） | `D9.PrepareDeparture/Depart/RecordDeparture`；D19-L09 检查/补偿 | D9 离港事实；D2 航行启动（经公开命令） | `SailingSnapshot` 数值；`SailingSystem` 内部状态 |
| 返港（Return/Arrival） | D19-L10 确定性流程（返港→泊位→服务恢复→航程结束） | D9 泊位/服务；D3 航程结算 | 航程结算数值、服务恢复越权 |
| 命令幂等（Idempotency） | D19-L22 重复投递稳定结果（基于命令 ID + 事件重放） | 事件日志（EventLog）追加顺序 | 跳过事件、重复副作用 |
| 事件（Events） | D19-L16 港口事件（ID/顺序/关联命令 ID），接入 `EventLog` | 既有事件日志追加唯一性 | 绕过事件日志直写状态 |
| 保存投影（Save Projection） | D19-L20 投影到既有 Save 模型（`SaveSystem.Save`/`LoadD9Runtime` 等） | `SaveSystem`（磁盘/迁移/根模型） | 私有存档格式、`GameState` 直写 |

**通用规则**：
- D19 只能**调用公开契约、读取快照、编排流程**；任何数值修改只发生在所属系统类内部（裁决权表，见 DEVELOPMENT_STANDARDS §4）；
- 找不到公开契约 → `LOGIC_BLOCKED` 并记录，**不得复制类型或抢改前置文件**；
- 不触碰 `Transform`、`NavMesh`、`Input`、`Canvas`、`PlayerConnection`、网络/LLM（D19-L40 边界审计强制执行）。

## 3. D19 禁止写什么（红线汇总）

1. `D3TradeState.Money` / `Inventory` / `Market` 字段（D3 裁决）；
2. `D3InventoryState` 容量、数量、归属（D3 裁决）；
3. `ShipState` 船舶数值、改装、耐久（D4 裁决）；
4. `SailingSnapshot`/`D2RuntimeSnapshot` 航行数值与控制模式（D2 裁决）；
5. `D9PortShipPreparationSnapshot` 字段（D9 裁决；D19 只读）；
6. `GameState` 根模型与磁盘 JSON 结构（SaveSystem 裁决）；
7. `InitialHarbor`、正式 Prefab、Canvas、Build Settings、场景（Runtime 阶段）；
8. 任何 `RuntimeApproved`/`ReleaseApproved`/`IntakeEligible` 授予（D18 状态机边界）。

## 4. 可消费的 D18 稳定键（只读）

- L02 `category`（11 值大类）+ `taxonomy_category`（8 细分类，映射 `tools/schemas/d18_taxonomy_category_map.json`）——D19 资源引用必须用稳定键；
- 正式 content_id：`asset_hdri_green_point_park` / `asset_hdri_joburg_central_sunset` / `asset_hdri_kloppenheim_06` / `asset_hdri_venice_sunset` 等（L52 已修复引用）；
- ResourceKey 规则（L16）：`d10:<segment>:<tail>` / `d20:<segment>:<tail>`，`candidate_` 前缀只作 deprecated alias；
- 详见 [D18 只读交接接口](D18_LOGIC_HANDOFF_INTERFACE.md)（L46，§2—§4）。

## 5. Runtime Deferred 清单（D19 本阶段全部延期）

- 港口场景、碰撞、路径、`Transform` 锚点实测、HUD/Canvas/UI 可读性、输入手感；
- 真实航行、天气视觉、性能/FPS/加载、截图与 Player 路线；
- 模型导入、Prefab 装配、画面质量（D18_L53 放行范围之外的全部视觉/运行项）；
- 4 条 HDRI 逐文件哈希（Pending，不补猜）；`RuntimeApproved`/`ReleaseApproved`（均为 0）；
- D19-L46 将登记完整清单（锚点、碰撞、输入、HUD、路线、画面、性能）。

## 6. SUPERSEDED 隔离清单（旧 D19 历史证据）

以下材料**只作历史证据**：可被只读引用（诊断），**禁止作为 D19-Lxx 通过条件或前置**，禁止重跑 RERUN：

- 结果目录：`game/tools/results/D19/D19-R01-A / R01-B / R01-S02 / R02-A01—A05 / R02-S02 / R03-A01—A03 / R03-S01`（含全部 RERUN* 机器结果）；
- 文档：`docs/game/design/common/D19_R01_*`、`D19_R02_*`、`D19_R03_*`（含 RERUN* 报告与预检）；
- 场景/Prefab 工作成果：`D19_PORT_ARCHITECTURE.md`（D19-A02 工作场景）、`game/Assets/Scenes/D19/Work/`、`Prefabs/Buildings/D19/`（历史 A/S 系列，SUPERSEDED）；
- handoff：`docs/game/design/common/handoffs/D19/D19-R0*_HANDOFF.md`、`D19-S0*_HANDOFF.md`（历史）；
- 旧 D19 截图、Player 日志、性能样本（R03）——历史诊断用，不作门禁。

规则：D19-Rxx 的 Player 路线/FPS/截图结论**不阻塞也不通过**任何 D19-Lxx；新 L 编号全部从 `PLANNED` 开始，以纯逻辑证据取得 `LOGIC_PASS`。

## 7. 链接索引（本文件引用的既有文件）

- 章节：`../../AGENT_DEVELOPMENT_CHAPTER_11_D19.md`、`../../AGENT_DEVELOPMENT_TASKS.md`、`../../D18_D24_VISUAL_PLAYABLE_ROADMAP.md`、`../../LOGIC_TASK_PROTOCOL.md`
- D18：`D18_LOGIC_HANDOFF_INTERFACE.md`、`../D18_LOGIC_ACCEPTANCE.md`、`../D18_BINARY_AND_IMPORT_DEFERRAL.md`
- 源码：`game/Assets/Scripts/Systems/D2/SailingSystem.cs`、`D3/TradeEconomySystem.cs`、`D3/D3InventorySystem.cs`、`D4/D4ShipyardSystem.cs`、`D9/D9PortShipPreparationSystem.cs`、`Save/SaveSystem.cs`
