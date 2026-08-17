# D19-L15 交接报告

## 任务信息与结论

- 任务编号：`D19-L15`（出航准备适配器）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L15/D19_L15_LOGIC_RESULT.json`

## 前置确认（全部满足）

- D19-L17（失败码目录）、D19-L18（本地化 key 注册表）、D19-L23（权限校验器）均为 **LOGIC_PASS**。
- D9/D2/D4 出航准备公开读模型/命令盘点（2026-08-17 源码事实）：
  - **D9** `D9PortShipPreparationSystem.PrepareDeparture(ship, portId, money)` 是出航准备的权威公开命令；原始失败码集 `{not_moored, no_crew, insufficient_supplies, invalid_port}`，`Depart(...)` 增加 `{not_in_port, invalid_destination}`；公开读模型 `BuildSnapshot(...)` → `D9PortShipPreparationSnapshot`（Moored/BerthId/InPort/CrewCount/Water/Food/Hull/HullMax/Weight/CargoCapacity/Cargo）。
  - **D3** `D3RuntimeSnapshot`（VoyageStatus/CurrentLocationId/DestinationId，Voyage owner）与 `D3InventorySystem.UsedCargo`（货舱容量/数量唯一写者公开读数）。
  - **D4/Save** `ShipState`（Hull/HullMax/CrewCapacity/Supplies/Cargo 事实来源）。
  - **D2** `SailingSnapshot` 属航行表现层（位置/航向），出航准备不引用。
- 事实边界：`D9.PrepareDeparture` 成功时有副作用（`SetMooring(false)`），`Depart` 会启动航程——因此本适配器**只读**消费 `BuildSnapshot` 读模型，**从不调用**这两个入口。

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PreparationPortAdapter.cs`（新建，纯 C#）
- `game/tools/results/D19/D19-L15/D19_L15_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L15_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改任何既有 D19/D3/D9/D4/Save/Event 文件。

## 交付内容

### 1. 只读检查（固定顺序，7 项必检 + 1 项可选）

| 检查 ID | 域 | 原所有者 | 原始失败码（D9 真实码） | 失败时 D19 码/key |
|---|---|---|---|---|
| `departure.port` | 港口 | D9.PrepareDeparture | `invalid_port` | NotReadyToDepart / `d19.departure.failed` / L17 LocKey |
| `departure.moored` | 泊位 | D9.PrepareDeparture | `not_moored` | 同上 |
| `departure.crew` | 船员 | D9.PrepareDeparture | `no_crew` | 同上 |
| `departure.supplies` | 补给 | D9.PrepareDeparture | `insufficient_supplies` | 同上 |
| `departure.cargo` | 货舱 | D3.D3InventorySystem（读数）/D9 投影 | （无所有者门槛，D19 一致性护栏） | 同上 |
| `departure.ship` | 船只 | D4 ShipSystem / ShipState | （无 D9 门槛，D19 完整性护栏） | 同上 |
| `departure.route` | 航线 | D9.Depart | `not_in_port` / `invalid_destination` | 同上 |
| `departure.lifecycle` | 生命周期 | D19PortLifecycleContracts（L02 迁移表） | （D19 侧门槛，允许前态 Boarded/PortVisit） | 同上 |

- 状态三值：`Ready` / `NotReady`（明确不满足，保留原始码）/ `Unknown`（输入缺失/冲突/无法判定，**保守拒绝**）。
- 每条保留：`Owner`、`OriginalCode`、`FailureCode`（L02 枚举）、`FailureCodeId`（L17 目录）、`LocKey`（L17/L18）、`Detail`（稳定事实数字，无 UI 文案）。
- 货舱一致性 = 无负数量 + 重量 ≤ 容量 +（trade 可用时）D9 投影重量与 `D3InventorySystem.UsedCargo` 交叉核对，读数冲突 → Unknown。
- 船只完整性 = hull>0（损毁禁止离港）；hull>hullMax → 冲突 Unknown。

### 2. 确定性输出（L09 离港工作流直接消费）

- `D19DepartureReadinessSummary`：`CanDepart`（全部 Ready 且无 Unknown）、`HasUnknown`、`FirstBlocker`、`BlockingCheckIds[]`、`BlockingOriginalCodes[]`（与阻断项一一对应，固定顺序）、`Items[]`（固定顺序）、`Clone()`。
- `D19DepartureIntent`：纯数据（CommandId/ShipId/PortId/DestinationId/WorldDay/Summary/Executable）；本适配器**只生成不执行**。
- 成功标准印证：汇总可完整说明可否离港而**不拥有任一数值**（数字全在原所有者读模型）；缺接口/冲突/未知 → 保守拒绝；无 UI/场景/运行时依赖。

### 3. 红线（与任务卡一致）

- 不存在任何对 `PrepareDeparture(`/`.Depart(`/`BuySupply`/`Recruit(` 的调用；不自动补货、不扣钱、不修改船员、不替航海系统启动航程（60 项断言中的静态扫描项 + 副作用不变式断言双重验证）。
- 无 UnityEngine/Transform/Canvas/Input/DateTime/HttpClient；无 EventLog 追加、无事件创建。
- 纯函数式构造：相同输入 → 逐字段相同输出（确定性断言）。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 前置状态核对 + D9/D2/D4 读模型/命令盘点 | PASS：三前置 LOGIC_PASS；公开入口与原始码集与 L17 目录条目 #11 完全一致 |
| 仓库外临时 dotnet 工程（net9.0 / LangVersion 9.0 / Nullable disable，15 个真实源文件 + 本文件 + 60 项断言 Program.cs）`dotnet run` | PASS：编译 0 错误 0 警告；60/60 断言通过（场景清单见结果 JSON verification 第 2 条） |
| `python tools/sync_logic_task_status.py` | PASS：D19-L15=LOGIC_PASS，总览重新生成 |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 明确未宣称

- 未替任何系统执行离港/补给/扣款/船员变更；唯一产出是只读汇总与纯数据意图。
- 未把本汇总写成"已离港/可运行时离港"；真实离港执行与航程启动验证属 Runtime 阶段。
- 未修改 L17/L18/L23 或任何既有系统文件；未引用旧 D19-A/S/R 结论。

## 待办 / 后续注意事项

- **L09 离港工作流**：消费 `BuildSummary`（`CanDepart`/`BlockingCheckIds`/`BlockingOriginalCodes`）做离港前置验证，消费 `D19DepartureIntent`（`Executable`）后经 D9 `Depart` 发出航程启动意图；D19 仍不得直接写 SailingSystem。
- **L19 港口只读模型**：如需把本汇总并入读模型投影，只读消费本适配器；本文件不改。
- **D2 SailingSnapshot**：表现层引用由 Runtime 阶段场景任务处理，本适配器不接。
- 若 D9 后续扩展 `PrepareDeparture` 门槛（如船体/货舱门槛），本适配器需同步其原始码口径（当前 cargo/ship 条目明确标注为 D19 一致性护栏、无所有者门槛，不冒充 D9 码）。

## 测试覆盖清单（60 项断言分组）

全通过（含 Boarded 生命周期，8 项）/ 无生命周期输入（7 项）/ Docked 阻断 + FailureCodeId/LocKey/无原始码 / water=0→insufficient_supplies / crew=0→no_crew / moored=false→not_moored / 目的地空→invalid_destination / 目的地==当前港→invalid_destination / 航行中→not_in_port / portId 空→invalid_port / D9 端口冲突→Unknown / d9=null 六域 Unknown+HasUnknown / D3↔D9 货舱冲突→Unknown / hull=0→NotReady / hull>hullMax→Unknown / 负货舱→NotReady / 阻断顺序+原始码对应（moored→crew→supplies→lifecycle）/ 两次构建逐字段一致 / 意图 Executable 一致性 + 无副作用（VoyageStatus 与 DestinationId 不变）/ Validate() 空 / 去注释静态扫描禁词 0 命中。
