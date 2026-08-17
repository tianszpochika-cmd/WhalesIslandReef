# D19-L18 交接报告

## 任务信息与结论

- 任务编号：`D19-L18`（波次 19.1，并行；前置 D19-L01=LOGIC_PASS 已满足）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L18/D19_L18_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortLocalizationKeys.cs`（新建，纯 C#、零 Unity 依赖）
- `game/tools/results/D19/D19-L18/D19_L18_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L18_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- `docs/game/LOGIC_TASK_STATUS.md`（仅由 `python tools/sync_logic_task_status.py` 写入）

未修改任何实际 UI、字符串表、Canvas、D19PortLifecycleContracts.cs、D19PortEventContracts.cs、其它 D19 源文件或旧本地化内容；未启动 Player。

## 公开接口（`Whalebone.Systems.D19.D19PortLocalizationKeys`）

**key 常量（61 个，6 域 + 事件摘要域）**

| 域 | 数量 | 示例 | 对应关系 |
|---|---|---|---|
| 生命周期-命令 | 9 | `d19.lifecycle.enter_port`、`d19.lifecycle.depart` | D19PortCommandKind 一一对应 |
| 生命周期-迁移原因 | 9 | `d19.lifecycle.berth_assigned`、`d19.lifecycle.departed` | D19PortTransitionReason 一一对应 |
| 服务 | 5 | `d19.service.market`、`d19.service.supply` | 稳定服务 ID market/warehouse/tavern/shipyard/supply |
| 失败 | 13 | `d19.failure.unknown_port`、`d19.failure.berth_occupied` | **D19PortFailureCode 非 None 值一一对应（L17 目录消费目标）** |
| 补偿 | 3 | `d19.compensation.rolled_back`、`partial`、`failed` | 补偿反馈 |
| 迁移提示 | 3 | `d19.migration.save_unsupported/repaired/conflict` | 旧存档/保存兼容反馈 |
| 读模型反馈 | 8 | `d19.readmodel.port_open`、`berth_taken`、`command_rejected` | D19-L19 读模型输出 |
| 港口事件摘要 | 10 | `d19.port.event.arrival`、`d19.port.event.rejection` | D19PortEventType 十类一一对应，**命中 L16 冻结正则 `^d19\.port\.[a-z0-9_.]+$`** |

**纯检查 API**

- `IsValidKey(string)`：命名规则 `d19.<domain>.<name>`，每段 `[a-z0-9_]+`，无大小写/空白/分隔符变体。
- `TryGetFailureKey(D19PortFailureCode, out string)` / `FailureKeyOrMissing(code)`：失败码 → 唯一 key；未注册码返回 false / `MissingKey` 哨兵（`d19.key.missing`），**绝不静默回退为成功**。
- `TryGetTransitionReasonKey` / `TryGetServiceKey` / `TryGetEventSummaryKey`：迁移原因/服务 ID/事件类型 → key；未注册（None/Unknown/未知 ID）显式 false。
- `ValidateRegistry()` → `D19KeyValidationResult`：重复检测、命名校验、**缺 key 报告（经 `Enum.GetValues` 反射遍历 D19PortFailureCode，L17 扩展枚举时自动报告）**；`HasErrors=true` 时调用方必须显式处理。

## 关键口径

- 本文件**只定义逻辑键**，不含任何中英文显示文案（静态扫描：代码字符串字面量全部为 key/前缀/服务 ID）；不引用 UnityEngine / UnityEngine.Localization / TMPro / Canvas。
- `D19PortFailureCode.None` 与 `D19PortEventType.Unknown` 是成功/未知哨兵，显式不注册 key（Try* 返回 false）。
- L16 事件契约要求 `result.summary` 必须匹配 `d19.port.*`：本清单的 `d19.port.event.*` 摘要 key 与 L16 冻结正则逐条实测兼容（75 项断言中包含逐类型匹配检查）。

## 验证命令与结果（与结果 JSON 一致）

| 命令 | 结果 |
|---|---|
| `dotnet build`（仓库外临时工程，链接 4 个真实 D19/Event 契约文件，net9.0/C#9） | PASS：0 警告 / 0 错误 |
| 临时控制台断言 75 项 | PASS：75/75（注册表 61 唯一 key 0 问题；13 失败码 1:1；缺 key 哨兵；命名正反例；L16 正则逐条命中） |
| 静态扫描（去注释代码） | PASS：0 处 Unity 本地化 API、0 处显示文案 |
| `python tools/sync_logic_task_status.py` | PASS：D19-L18=LOGIC_PASS |
| `git diff --check` | PASS：无空白错误，仅既有 LF→CRLF 警告 |

## 明确未宣称

- 未添加或翻译任何显示文案；未接入任何 UI/Canvas/字符串表。
- 未声称任何 D19 系统已消费这些 key（消费任务按各自前置落地）。
- D19-L17 尚未落地（PLANNED）：13 个 key 按 L02 冻结枚举注册；L17 落地后若扩展失败码，`ValidateRegistry()` 会自动报告缺 key，届时需重跑检查。

## 待办 / 后续注意事项

- L17 落地后：重跑注册表检查确认无缺 key；L17 目录的"本地化 key"字段应引用本文件常量。
- D19-L05（服务可用性）、L11—L15（适配器）、L19（读模型）将消费本清单；L19 读模型 feedback 使用 `d19.readmodel.*`，事件 summary 使用 `d19.port.event.*`。
- 显示文案（中英）由未来本地化任务按本清单 key 补充，本任务不写字符串表。
