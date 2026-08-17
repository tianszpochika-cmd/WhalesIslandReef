# D19-L16 交接报告

## 任务信息与结论

- 任务编号：`D19-L16`（波次 1，并行）
- 前置：`D19-L01 = LOGIC_PASS`（2026-08-17T01:09:26）已满足
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L16/D19_L16_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortEventContracts.cs`（新建，契约本体，纯 C#、零 Unity 依赖）
- `game/tools/results/D19/D19-L16/D19_L16_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L16_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆；如当天文件不存在则新建）

未修改任何事件生产者、Save、其它 D19 文件、contracts、Unity 或表现层；未启动 Player；未运行场景。注意：`D19PortEventContracts.cs.meta` 由 Unity 在下次导入时生成，不属于本任务写入范围（与仓库内其它新建 .cs 文件一致）。

## 契约内容（`D19PortEventContracts.cs`）

命名空间 `Whalebone.Systems.D19`，依赖既有 `Whalebone.Systems.Events`（EventLog/WorldEvent/EventResult/EventValidationException/EventSchemaVersionException）。

### 1. 事件类型（D19PortEventType，冻结十类）

`Unknown`（安全拒绝哨兵）、`Arrival`（到港）、`BerthReserved`（泊位预留）、`BerthReleased`（泊位释放）、`ServiceResult`（服务结果）、`Boarded`（登船）、`Disembarked`（下船）、`Departure`（离港）、`Return`（返港）、`Compensation`（补偿）、`Rejection`（拒绝）。

### 2. 稳定 event_id

- 格式：`d19pe_<type_token>_<commandId>_<occurrence>`（全部小写，如 `d19pe_arrival_cmd_001_0`）；
- `TryParseEventId` 可解析回 command/type/occurrence（供 L35 顺序审计与上层重放）；
- 同一命令的多个事件用 occurrence 区分，同一 (event_id, save_revision) 由 EventLog 去重。

### 3. schema/version 与未知事件安全拒绝

- `D19PortEventSchema.CurrentVersion = 1`；`EnsureSupportedVersion` 对未知版本抛 `EventSchemaVersionException`；
- `TryGetPortEventType`：非 `D19PortSystem` owner 或未知类型 → `Unknown`/false（上层安全拒绝，不崩溃）；
- `D19PortEventGuard.Validate`：append 前结构校验（command:/port:/ship: 关联必在、result_code 必须 `port.` 前缀、summary 必须本地化 key、补偿必须 cause_ids、拒绝必须 port.reject_reason）。

### 4. 关联 ID 与序列化边界

| 关联 | 表达位置 | 规则 |
|---|---|---|
| command | `input_facts` 的 `command:<id>` | 必填；事件可唯一关联命令 |
| port / ship | `input_facts` 的 `port:<id>` / `ship:<id>` | 必填 |
| berth / service | `input_facts` + `result.fact_ids` 的 `berth:<id>` / `service:<id>` | 随事件类型 |
| actor | `actor_ids` | 可空列表，非空项保留 |
| 因果 | `cause_ids` | 补偿/拒绝必须引用原事件 ID |
| 逻辑时间 | `WorldDay` + `SaveRevision` | 无 DateTime/真实时钟/场景引用 |
| payload | `result.values`（string→string） | 仅安全可序列化字符串（含控制字符拒绝）；无 Unity 对象、无可变系统引用 |
| 文案 | `result.summary` | 空 或 `d19.port.*` 本地化 key；禁止 UI 文案 |
| 结果码 | `result.result_code` | 必须 `port.` 前缀的稳定机器码 |

### 5. 便捷工厂

`CreateArrival / CreateBerthReserved / CreateBerthReleased / CreateServiceResult / CreateBoarded / CreateDisembarked / CreateDeparture / CreateReturn / CreateCompensation / CreateRejection` + 通用 `Create`。工厂只构造事实包（WorldEvent），不执行任何状态变更；追加与去重由 EventLog 完成（基线 1.6/2 节：不另起事件总线）。

## 验证（全部真实执行）

| 检查 | 结果 |
|---|---|
| 临时 dotnet 工程编译（引用真实 EventContracts.cs + EventLog.cs + 本文件，netstandard2.1、C# 9、0 依赖） | 0 警告 / 0 错误 |
| 临时控制台 33 项确定性断言（事件 ID 构造/解析往返、未知类型/畸形 ID/未知版本/外来 owner 拒绝、到港→泊位→服务→登船→离港→返港→补偿→拒绝 8 事件 EventLog 追加序列 1—8、重复投递去重、快照重放顺序保持、逐事件守卫、非法 payload/UI 文案/无 cause 补偿拒绝） | ALL CHECKS PASS |
| `git diff --check` | exit 0（仅既有 LF→CRLF 警告） |
| `python tools/sync_logic_task_status.py` | 总览重新生成，D19-L16=LOGIC_PASS |

## 明确未宣称

- 未宣称任何事件已接入真实端口流程（编排由 D19-L17—L35 实现）；
- 未修改事件生产者、Save、其它 D19 文件、contracts、Unity 或表现层；
- 未知事件类型/版本只被安全拒绝，不猜测语义。

## 待办 / 后续注意事项

- D19-L22（命令幂等）应使用 `command:<id>` input_fact 与 event_id 构造规则；L34（补偿）应使用 `CreateCompensation` 的 cause_ids 强制引用语义；
- D19-L35（事件顺序审计）可直接复用 `TryParseEventId`/`D19PortEventGuard`；
- D19-L17（失败目录）的失败码应使用 `port.rejected` + `port.reject_reason` payload 约定；
- .meta 文件待 Unity 导入时生成（非本任务范围）。
