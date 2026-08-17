# D19-L17 交接报告

## 任务信息与结论

- 任务编号：`D19-L17`（波次 19.1，并行；前置 D19-L01=LOGIC_PASS 已满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L17/D19_L17_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortFailureCatalog.cs`（新建，唯一失败码目录）
- `game/tools/results/D19/D19-L17/D19_L17_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L17_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改 `D19PortLifecycleContracts.cs`（L02）、`D19PortLocalizationKeys.cs`（L18）、`D19PortEventContracts.cs`（L16）、`D19PortAnchorCatalog.cs`（L04）、任何 UI 文案、Localization 资源或其它 D19 源文件。

## 公开接口（供 L03/L05—L10/L19—L22/L25—L32 消费，无 UI 依赖）

- `D19FailureDomain`：十域枚举（Port/Permission/Lifecycle/Berth/Service/Trade/Inventory/Boarding/Departure/Save）
- `D19FailureRetryPolicy`：NoRetry / RetrySafe / RetryAfterCorrection
- `D19FailureEntry`：Code / LocKey / Domain / Retry / ParamWhitelist / LifecycleCode / OriginalSource / OriginalFailureFact / Note
- `D19PortFailureCatalog`：
  - `Entries`（冻结 13 条）、`AllCodes`、`CatalogDefinedKeys`（7 个新 key）
  - `Find(code)` / `TryResolve(code, out entry)` / `ForLifecycleCode(D19PortFailureCode)`（只读引用 L02 枚举；无对应业务条目的生命周期内部码返回 null）
  - `Validate()`（确定性自检：重复码/重复或非法 key/非法参数名/未登记 key/缺域/非法策略；空列表=健全）

## 目录条目（13 条 / 10 域，业务码全局唯一）

| 业务码 | 域 | LocKey | 重试 | 参数白名单 | 生命周期码 | 原始失败事实（真实来源） |
|---|---|---|---|---|---|---|
| d19.port.unknown_port | 未知港口 | L18 FailureUnknownPort | 修正后重试 | port_id | UnknownPort | D9 PrepareDeparture Code=invalid_port；相邻 D3 invalid_reference 保留 |
| d19.port.permission_denied | 权限不足 | 新 key permission_denied | 修正后重试 | port_id, service, actor_id | 无 | L23 计划；D4 声望门槛仅 Accepted=false（无码不映射） |
| d19.lifecycle.invalid_transition | 非法生命周期 | L18 FailureInvalidTransition | 不可重试 | from_state, reason_id | InvalidTransition | L02 CanTransition=false，原状态不变 |
| d19.berth.occupied | 泊位冲突 | L18 FailureBerthOccupied | 修正后重试 | berth_id, port_id | BerthOccupied | L06 计划；相邻 D9 unknown_berth 保留 |
| d19.berth.expired | 泊位过期 | 新 key berth_expired | 修正后重试 | berth_id, port_id, reserved_world_day | 无 | L06 计划；无既有码不编造 |
| d19.service.unavailable | 服务关闭 | L18 FailureServiceUnavailable | 安全重试 | port_id, service | ServiceUnavailable | D9 CheckService Code ∈ {invalid_state, invalid_port, not_moored} |
| d19.trade.funds_insufficient | 资金不足 | 新 key funds_insufficient | 修正后重试 | commodity_id, required_amount, available_amount | 无 | D3 not_enough_money；D9 not_enough_money；D4 Accepted=false（无码） |
| d19.inventory.operation_failed | 库存失败 | 新 key inventory_operation_failed | 修正后重试 | item_id, quantity | 无 | D3InventorySystem Try* false + out error（无码，error 为显示文本） |
| d19.inventory.capacity_exceeded | 容量不足 | 新 key capacity_exceeded | 修正后重试 | item_id, quantity, capacity, used | 无 | D3InventorySystem CanAddCargo=false / Free*/Used* 读数；不与 market_empty 合并 |
| d19.boarding.failed | 登船失败 | 新 key boarding_failed | 修正后重试 | member_id, state | AlreadyBoarded | L08 计划；生命周期子原因 ∈ {NotMoored, NotBerthed, AlreadyBoarded} |
| d19.departure.failed | 离港失败 | 新 key departure_failed | 修正后重试 | ship_id, port_id, destination_id | NotReadyToDepart | D9 PrepareDeparture/Depart Code ∈ {not_moored, no_crew, insufficient_supplies, invalid_port, not_in_port, invalid_destination} |
| d19.save.conflict | 保存冲突 | L18 MigrationSaveConflict | 安全重试 | save_revision, command_id | 无 | SaveSystem.AppendD9SaveCheckpoint revision / D3 SaveRevision / D9 Duplicate(duplicate_settlement) 机制真实存在；无独立 conflict 码不编造 |
| d19.save.unsupported_version | 不支持旧存档 | L18 MigrationSaveUnsupported | 不可重试 | save_version, supported_version | 无 | SaveMigrationStatus.UnsupportedVersion（真实枚举） |

## 关键口径

1. **唯一性**：同一业务失败只有一个 code（Validate() 拒绝重复码/key）；生命周期枚举码只作为子原因细节（LifecycleCode），不作为独立业务消息。
2. **不吞掉既有事实**：每条目记录真实来源 API 与真实稳定码；D4/D3Inventory 无稳定码时只保留 Accepted=false/bool+error 事实并在 Note 标注"不编造映射"。
3. **不泄露内部对象**：参数白名单仅稳定标量（ID/数量/版本/状态名），ForbiddenParamTokens 含 transform/unity/gameobject/component/snapshot/canvas/input/pointer/object/ref。
4. **无 UI 文案**：本文件不含任何中英文显示字符串；LocKey 只引用 D19-L18 既有常量或本目录定义键（命名规则与 L18 一致）。
5. **与 L18 关系**：本任务未扩展 D19PortFailureCode 枚举（写范围禁止），因此 L18 ValidateRegistry 零缺 key；7 个新业务 key 列入 CatalogDefinedKeys 供 L18 后续扩展注册。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 源码盘点（D3/D4/D9/Save/D19 真实失败码与事实提取） | PASS（真实码清单见结果 JSON verification[0]） |
| 临时 dotnet 工程（仓库外，引用真实 EventContracts/EventLog/D19PortLifecycleContracts/D19PortEventContracts/D19PortLocalizationKeys/D19PortFailureCatalog） | PASS：编译 0 错误 0 警告；382 项确定性断言 0 失败 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L17=LOGIC_PASS |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 明确未宣称

- 未创建/修改任何 UI 文案、字符串表、Canvas 或 Unity Localization 资源；
- 未修改 D19-L02/L04/L16/L18 交付文件或任何其他系统失败码；
- 未编造 D2—D4 失败码映射（D4/D3Inventory 无码事实如实保留）；
- 本目录不授予任何 RuntimeApproved/ReleaseApproved 状态。

## 待办 / 后续注意事项

- L18 扩展：7 个新 key（CatalogDefinedKeys）注册进 D19PortLocalizationKeys.AllKeys/常量（本任务写范围禁止）。
- L45 一致性核对：验证本目录 code/key 与 L18 注册表、L16 事件、L02 枚举的最终一致。
- 消费任务：L03 状态机/L05 可用性/L06 泊位/L23 权限/L08 登船/L09 离港/L20 保存投影落地时以 `Find`/`TryResolve` 消费本目录，不自行发明失败字符串。
- 无 P0 blocker；Runtime 延期项见结果 JSON。

## 附录：只读消费的既有文件（本任务未修改）

- `game/Assets/Scripts/Systems/D19/D19PortLifecycleContracts.cs`（D19-L02：D19PortFailureCode 枚举）
- `game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs`（D19-L18：key 常量与 ValidateRegistry）
- 既有系统失败事实来源：`D3/TradeEconomySystem.cs`、`D3/D3InventorySystem.cs`、`D4/D4ShipyardSystem.cs`、`D9/D9PortShipPreparationSystem.cs`、`Save/SaveMigration.cs`、`Save/SaveSystem.cs`（仅只读盘点）
