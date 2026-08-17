# D19-L23 交接报告

## 任务信息与结论

- 任务编号：`D19-L23`（波次 19.2，并行；前置 D19-L02/D19-L04/D19-L17/D19-L18=LOGIC_PASS 全部满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L23/D19_L23_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortAuthorityValidator.cs`（新建，唯一权限校验器）
- `game/tools/results/D19/D19-L23/D19_L23_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L23_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改 `D19PortLifecycleContracts.cs`（L02）、`D19PortAnchorCatalog.cs`（L04）、`D19PortFailureCatalog.cs`（L17）、`D19PortLocalizationKeys.cs`（L18）、`D19PortEventContracts.cs`（L16）或任何其他系统/UI/本地化文件。

## 公开接口（供 D19-L11—L15 适配器在调用所有者前使用；L03/L05/L08/L09 可消费）

- `D19AuthorityContext`：Kind（L02 D19PortCommandKind）/ ActorId / ShipId / PortId / BerthId / ServiceId / CargoShipId（货物归属船，非空时必须等于 ShipId）
- `D19AuthorityClaims`（调用方明确传入的声明，校验器绝不创建/修正）：ShipId / ActorOwnsShip / ActorIsCrewOfShip / ShipCurrentPortId（停靠港声明；校验器只做一致性比较，不读取登录/场景状态）
- `D19AuthorityResult`：Allowed / Cause（D19DenialCause）/ Failure（L17 条目，含 Code+LocKey）/ CanonicalPortId / CanonicalBerthId / CanonicalServiceId
- `D19PortAuthorityValidator.Validate(context, claims)` 与 `Validate(context, claims, catalog)`（显式目录重载供确定性测试）；`ServiceCommands`/`BerthCommands`/`PortlessCommands`；`CauseToEntry(cause)`；`VerifyMapping()`

## 校验矩阵（纯函数，顺序固定，可重放）

| 步骤 | 检查 | 拒绝原因（Cause） | L17 条目 |
|---|---|---|---|
| 1 | 输入完整性（actor/ship/port；Compensate 免 port） | EmptyActor / EmptyShip / EmptyPort / InvalidInput | permission_denied |
| 2 | 声明船 == 请求船；actor 与船有 owns 或 crew 关系 | CrossShip / CrossPlayer | permission_denied |
| 3 | port_id 锚点解析 | UnknownPort | **d19.port.unknown_port** |
| 4 | 声明停靠港（canonical 比较）== 请求港 | CrossPort | permission_denied |
| 5 | AssignBerth 必需泊位；携带的泊位必须属于请求港 | MissingBerth / UnknownBerth / CrossPortBerth | permission_denied |
| 6 | 仅 StartService/CompleteService 允许且必需服务；非服务命令带服务 = 伪造 | MissingService / UnknownService / CrossPortService / ForgedService | permission_denied |
| 7 | 货物归属船 == 请求船 | CrossShip | permission_denied |

- 只有 UnknownPort 拒绝返回 L17 `d19.port.unknown_port` 条目；其余全部返回 `d19.port.permission_denied` 条目（任务要求"权限校验只返回允许/拒绝与 L17 失败码/key"）。
- 历史别名：`santa_margarita`→`port_santa_margarita`、`building_saint_marys_dock`→`berth_santa_margarita_dock_01`（L04 目录解析，结果携带规范键）。

## 关键口径

1. **可重放**：相同 (context, claims, catalog) → 逐字段相同结果（断言验证）；无时间/随机/静态可变状态。
2. **零写入**：本文件无任何可变字段；不写 D3/D9/D4/Save 任何状态；不创建授权记录。
3. **不读取登录状态**：船舶停靠港、船主/成员关系全部由调用方作为声明传入；校验器只比较，不查询。
4. **拒绝四类越权**：跨船（声明船/货物船 ≠ 请求船）、跨港（请求港 ≠ 声明停靠港、泊位/服务归属他港）、跨玩家（无任何 owns/crew 关系）、伪造服务（非服务命令携带服务 ID、服务命令缺服务、未知/他港服务）。
5. **只返回 L17 code+key**：结果不含任何显示文案；LocKey 来自 L17 条目（permission_denied 的 key 属 L17 定义的待 L18 注册键）。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 前置核对（L02/L04/L17/L18 状态） | PASS：全部 LOGIC_PASS |
| 仓库外临时 dotnet 工程（真实 8 个源文件）`dotnet run` | PASS：编译 0 错误 0 警告；33 项确定性断言 0 失败 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L23=LOGIC_PASS |
| `git diff --check` | PASS：exit 0（仅既有 LF→CRLF 警告） |

## 明确未宣称

- 未授予/读取任何登录会话、玩家身份或运行时状态；
- 未创建授权、未修正任何归属、未调用 D3/D9/D4 所有者命令；
- 未修改任何既有 D19 文件或其它系统失败码；
- 本校验器不产生任何 RuntimeApproved/ReleaseApproved。

## 待办 / 后续注意事项

- L11—L15 适配器：调用 `Validate` 后再调用所有者（D3/D9/D4）；停靠港与归属声明由适配器从各自读模型构造传入。
- L18 扩展：`d19.failure.permission_denied` 等 7 个 L17 新 key 注册进 `D19PortLocalizationKeys.AllKeys`（本任务写范围禁止）。
- L45 一致性核对：VerifyMapping 与 L17 目录、L04 锚点目录最终一致性。
- 无 P0 blocker；Runtime 延期项见结果 JSON。

## 附录：只读消费的既有文件（本任务未修改）

- `game/Assets/Scripts/Systems/D19/D19PortLifecycleContracts.cs`（L02：D19PortCommandKind）
- `game/Assets/Scripts/Systems/D19/D19PortAnchorCatalog.cs`（L04：port/berth/service 稳定键解析与跨港拒绝）
- `game/Assets/Scripts/Systems/D19/D19PortFailureCatalog.cs`（L17：d19.port.permission_denied / d19.port.unknown_port 条目）
- `game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs`（L18：key 命名规则）
