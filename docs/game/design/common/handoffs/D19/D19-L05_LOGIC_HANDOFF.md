# D19-L05 交接报告

## 任务信息与结论

- 任务编号：`D19-L05`（波次 1，并行；精确前置 D19-L04/D19-L17/D19-L18 均 LOGIC_PASS 已核验）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L05/D19_L05_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortAvailabilitySystem.cs`（新建，纯函数裁决器）
- `game/tools/results/D19/D19-L05/D19_L05_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L05_LOGIC_HANDOFF.md`（本文件）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改场景、Prefab、D19Lighting、资源 catalog、其它 D19 系统文件或 D2—D9 文件；未启动 Player；未读取系统时钟/场景天气/NPC GameObject/资源。

## 冻结语义

### 输入（全部显式传入，纯逻辑）

`portId`（L04 锚点目录，别名可解析）、`serviceId`（L18 注册的 5 项：market/warehouse/tavern/shipyard/supply）、`D19ServiceTimePeriod`（Morning/Afternoon/Evening/Night）、`D19FactionPermission`（Allowed/Denied）、`D19WeatherLabel`（Clear/Rain/Storm）、`D19FacilityState`（Open/Closed/OutOfStock/UnderRepair/Blockaded）、`staffed`（NPC 仅以此逻辑标记表示，无任何对象引用）。

### 输出

`D19ServiceAvailabilityResult`：Verdict（Available/Unavailable/Rejected）、ReasonCode（**必须能被 D19PortFailureCatalog.Find 解析，来自 L17**）、LocKey（**来自 L18 注册表**：可用→`d19.readmodel.service_open`，不可用/拒绝→L17 条目 `d19.failure.service_unavailable` 或 `d19.failure.unknown_port`）、RetryPolicy（来自 L17 条目）、Detail（稳定细目枚举）、RetryHint（稳定逻辑提示）。

### 确定性优先级链（冻结）

设施状态（Closed/UnderRepair/Blockaded/OutOfStock）→ 势力拒绝 → 天气（Storm 关 market/supply，Rain 关 market）→ 时段（Night 关 market/warehouse/shipyard；tavern 夜间开放）→ 未配人（NotStaffed）→ 可用。

### 保守拒绝（未知/冲突）

未知港口 → Rejected + L17 `d19.port.unknown_port`；未知服务 ID、越界枚举值（cast 值）、空/null 输入 → Rejected(UnknownInput)；`OutOfStock` 声明于非补货服务（如 tavern）→ Rejected(ConflictingInput)。服务未登记于该港（L04 目录无 port+service 键，如荒岛全部服务）→ Unavailable(NotRegistered) 而非 Rejected。

### 零副作用

纯静态函数，无写路径；500 轮裁决后目录键数不变（harness 验证）。

## 跨任务键差处理（真实发现并修复）

L18 冻结 `ServiceIdSupply = "supply"`（单数），L04 锚点目录按 D9PortService 冻结 `supplies`（复数）——若直接拼接键会把 supply 误判为 NotRegistered。在 **L05 唯一写入范围**内增加只读 `CatalogServiceSuffix` 映射（supply→supplies 等 5 项）并纳入 `Validate()` 自检；**未修改 L04/L18 文件**。此差异已在 handoff 登记，供 L45 一致性审计核对。

## 验证结果（全部真实执行）

| 验证 | 结果 |
|---|---|
| 临时 dotnet 工程编译（链接真实 D19 五文件 + Event 契约） | 0 warning 0 error |
| 57 项确定性断言 | 57/57 PASS（含 L17/L18 自检、3000 次确定性、优先级链逐项、保守拒绝、别名解析、零副作用） |
| `git diff --check` | PASS（仅既有 LF→CRLF 警告） |
| `python tools/sync_logic_task_status.py` | PASS：D19-L05=LOGIC_PASS |

## 明确未宣称

- 不裁决资金/库存/关系/场景：本系统无任何写路径，服务关闭不影响任何状态。
- 不读取系统时钟、场景天气或 NPC 对象；staffed 只是逻辑标记。
- 服务排队/费用/工期/真实开放时间联动全部延期（见 runtime_deferred）。

## 待办 / 后续注意事项

- D19-L19 读模型、L33 命令路由消费本裁决结果；L05/L06（泊位）可用同一 Result 形状扩展。
- L45 兼容性审计核对 supply/supplies 键差；若 L04 后续对齐 L18 单数，本系统映射表需同步更新。
- 真实开放时间/天气/势力/设施状态来源接入由后续整合任务完成。
