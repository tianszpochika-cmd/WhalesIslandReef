# D19-L24 交接报告

## 任务信息与结论

- 任务编号：`D19-L24`（精确前置：D19-L02、D19-L17、D19-L18 均 LOGIC_PASS，已满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L24/D19_L24_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19InvalidStateGuard.cs`（新建，纯 C#、零 Unity 依赖）
- `game/tools/results/D19/D19-L24/D19_L24_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L24_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- `docs/game/LOGIC_TASK_STATUS.md`（仅由 sync 脚本写入）

未修改 L02/L04/L16/L17/L18 文件、任何实际 UI、字符串表、Canvas、其它 D19 源文件、contracts、Unity 或保存内容；未启动 Player。

## 公开接口（`Whalebone.Systems.D19.D19InvalidStateGuard`）

| 检查 | API | 说明 |
|---|---|---|
| 生命周期迁移 | `CanTransition(from, reason, out to, out verdict)` | 封装 L02 迁移表；拒绝时原状态不变（out to = from），拒绝码 → L17 业务码（按 L17.ForLifecycleCode 优先，再按原因域：Boarded→boarding、Departed→departure、BerthAssigned→berth、ServiceStarted→service，兜底 lifecycle） |
| 港口归属 | `CheckPort(catalog, portId)` | 格式非法/未知 → `d19.port.unknown_port` + `d19.failure.unknown_port` |
| 泊位归属 | `CheckBerth(catalog, berthId, portId)` | 跨港/未知/格式非法 → `d19.berth.occupied` + 精确子原因（cross_port_berth / unknown_berth / invalid_berth_key） |
| 服务归属 | `CheckService(catalog, serviceId, portId)` | 跨港/未知 → `d19.service.unavailable` + 子原因 |
| 服务前置 | `CheckServicePrerequisite(catalog, serviceId, portId, state)` | 归属 + 必须 Berthed（否则 not_berthed） |
| 命令 ID | `CheckCommandId(commandId)` | 空/空白/控制符/超长(>64) → `d19.lifecycle.invalid_transition` + missing/invalid_command_id |
| 船员位置语义 | `CheckCrewLocation(current, requested)` | OnShore→OnBoard / OnBoard→OnShore 允许；重复登船（already_on_board）/重复下船（not_on_board）/Unknown（invariant_violation，不猜测）拒绝 |
| 状态组合 | `CheckStateCombination(catalog, state, portId, berthId, crew)` | None 不得带 ID；Berthed/InService 必须持本港泊位；Boarded 可选泊位但需归属；Boarded/Departed 要求船员 OnBoard |
| 不变式 | `CheckInvariant(state, berthId)` | 封装 L02 ValidateInvariant（Berthed 必须非空泊位） |
| 命令级组合 | `CheckCommand(catalog, command, currentState, currentBerthId, crew)` | ID→港口→泊位（AssignBerth 校验新泊位，其余用当前泊位）→迁移（KindToReason 映射）→状态组合；Compensate 不做正向迁移校验 |

每次拒绝返回 `D19GuardVerdict`：`BusinessCode`（L17）、`LocKey`（L18 绑定）、`Retry`（L17 条目）、`LifecycleCode`（L02 子原因）、`ReasonId`（精确稳定蛇形）、`Params`（**只含条目 ParamWhitelist 内稳定标量**）。

## 关键口径

- **不吞异常**：业务输入非法 → 显式 Verdict；`catalog=null` 等基础设施错误与不可解析业务码 → 异常向上传播（`InvalidOperationException`），绝无 try/catch 吞掉或 bool 默认值"自动修复"（实测 2 项断言覆盖）。
- **只校验不拥有**：文件无任何市场/库存/船舶/保存系统引用；不写任何状态；不改 L02/L04/L17/L18。
- 泊位占用/租期事实由 D19-L06 裁决，本防护器只校验键归属；船员位置是纯逻辑输入（`D19CrewLocation`），不读取 D4 内部状态。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `dotnet build`（临时工程，链接真实 D19 6 文件 + EventContracts.cs） | PASS：0 警告 / 0 错误 |
| 临时控制台断言 56 项 | PASS：56/56（含原状态不变、跨港/未知/格式拒绝、船员语义、命令级组合、异常传播、拒绝必带 L17 码+L18 key、参数全在白名单） |
| 静态扫描 | PASS：0 处 Unity API 引用（仅注释提及）；无 try/catch |
| `python tools/sync_logic_task_status.py` | PASS：D19-L24=LOGIC_PASS |
| `git diff --check` | PASS：无空白错误，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未接入任何真实命令路由/工作流（L03/L05—L10 等按各自前置落地后调用本防护器复用同一语义）。
- 未裁决泊位占用/租期、资金、库存或船员事实本身。

## 待办 / 后续注意事项

- L03（PortVisitSystem）应通过 `CanTransition`/`CheckCommand` 消费迁移语义；L05（服务可用性）用 `CheckServicePrerequisite`；L07—L10 工作流用 `CheckCommand`。
- 若 L06 泊位系统落地后引入占用/租期新失败事实，需先在 L17 目录登记条目（本防护器拒绝时若业务码未登记会显式抛出，不会静默成功）。
- 显示文案（中英）由本地化任务按 verdict.LocKey 补充。
