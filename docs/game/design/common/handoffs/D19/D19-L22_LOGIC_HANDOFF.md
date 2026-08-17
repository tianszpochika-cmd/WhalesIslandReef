# D19-L22 交接报告

## 任务信息与结论

- 任务编号：`D19-L22`（波次并行）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L22/D19_L22_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19CommandIdempotency.cs`（新建，港口命令幂等控制，纯 C#）
- `game/tools/results/D19/D19-L22/D19_L22_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L22_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改任何其他 D19 文件、契约、测试、Unity 场景/Prefab、UI、字符串表、contracts、server、web。

## 前置门禁（已核实）

D19-L02（契约）、L03（访问状态机）、L06（泊位）、L16（事件契约）、L17（失败目录）、L24（非法状态守卫）均 `LOGIC_PASS`。

## 交付内容

### 1. 幂等语义（核心 API）

| 方法 | 语义 |
|---|---|
| `Begin(domain, commandId, fingerprint, worldDay)` | 登记处理中标记；New（首次，授权副作用）/ Completed（原结果命中）/ InProgress（中断残留，可恢复）/ Conflict / Aborted / MissingCommandId |
| `Complete(domain, commandId, fingerprint, resultCode, summaryKey, occurrence, worldDay)` | 登记原结果 → Completed；未登记 → UnknownCommand |
| `Abort(domain, commandId, fingerprint, resultCode, summaryKey, worldDay)` | 异常/中断的安全结果 → Aborted（终态；重试须新命令 ID） |
| `EvictCompletedBefore(worldDay)` | 过期策略：按世界日驱逐终态记录（处理中残留永不自动清除） |
| `BuildSaveProjection() / RestoreFromProjection()` | 可保存最小投影（L20 消费）；字段稳定、无 Unity 依赖 |
| `BuildFingerprint(params string[])` | 确定性命令指纹：规范化+转义 `|`，防拼接歧义 |

### 2. 成功标准逐条落实

- **重复投递不二次扣款/二次占泊/二次发事件**：适配器只在 `Outcome==New`（`SideEffectAuthorized==true`）时执行副作用；重复投递返回 `Completed`（带原 resultCode/occurrence），冒烟程序验证市场 3 次投递仅 1 次扣款、泊位 3 次投递仅 1 次占泊；
- **相同 ID+相同指纹返回原结果**：`Completed` 命中返回原结果与事件数（occurrence），适配器据此补发 0 个新事件；
- **同 ID+不同指纹明确冲突**：`Conflict`（reasonId=`d19.idem.conflict`），状态不变，绝不静默返回旧结果；
- **不得用全局静态缓存**：每船实例（构造绑定 shipId），无 static 状态；
- **不得用真实时钟**：时间一律以 worldDay 参数传入，无 DateTime/Time/Environment 引用；
- **所有状态可由 L20 保存**：`D19IdempotencySaveProjection`（SchemaVersion/ShipId/RetentionWorldDays/MaxRetainedEntries/Entries）完整覆盖记录状态；投影往返后重复投递行为与恢复前一致；
- **异常/中断可审计安全结果**：InProgress 残留可枚举（InProgressCount）、可恢复（Recoverable=true）且绝不伪造完成结果；Abort 产生 Aborted 终态安全结果；Aborted 命中返回既有安全结果。

### 3. 命令域覆盖（六类共用语义）

`D19IdempotencyDomain`：`Market`（L11）、`Warehouse`（L12）、`Berth`（L06/L10）、`Boarding`（L08）、`Departure`（L09）、`Save`（L20）、`Service`（L13—L15）。本系统不替任何适配器执行副作用——只做判定与登记。

## 明确未宣称

- 没有执行任何市场扣款、泊位占用或事件追加副作用（服务只判定，副作用由适配器按 New 授权执行）；
- 没有写入任何存档（L20 投影接口消费本系统投影，本任务不落盘）；
- 未修改 L02/L03/L06/L16/L17/L24 及其文件；未触碰 Unity/场景/Prefab/UI/字符串表/contracts。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| 前置门禁检查（六项前置状态） | PASS：全部 LOGIC_PASS |
| `csc /target:library` 编译 D19CommandIdempotency.cs | PASS：exit 0，自包含 |
| 临时冒烟程序（10 组 28 项断言）编译运行 | PASS：RUN exit 0，全部通过 |
| 静态自检（Unity/时钟/全局静态扫描） | PASS：0 处真实引用 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L22=LOGIC_PASS |
| `git diff --check` | PASS：exit 0，仅既有 LF→CRLF 警告 |

## 待办 / 后续注意事项

- **L20（保存投影）接口**：`D19CommandIdempotency.BuildSaveProjection()` 返回 `D19IdempotencySaveProjection`（字段见第 1 节），L20 经既有 `SaveSystem` 投影接口落盘，禁止写 `GameState` 根模型；
- **适配器接入**：D19-L11—L15/L09/L10 按"Begin → 仅 New 时执行副作用 → Complete/Abort"模式接入；冲突/中断结果由各适配器映射到其失败载体；
- **补偿**：中断残留的最终裁决归 D19-L34 补偿任务（本系统只提供可审计标记与 Aborted 安全结果）；
- **事件**：重复投递补发事件数为 0（occurrence 用于审计对账），事件落日志由 L16/EventLog 负责。
