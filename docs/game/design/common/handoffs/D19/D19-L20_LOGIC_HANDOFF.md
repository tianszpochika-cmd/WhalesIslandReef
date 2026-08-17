# D19-L20 交接报告

## 任务信息与结论

- 任务编号：`D19-L20`（波次 19.3，保存投影）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L20/D19_L20_LOGIC_RESULT.json`

## 前置核验

精确前置检查通过：
- `D19-L02`：生命周期状态机契约（`LOGIC_PASS`）
- `D19-L03`：访问状态机（`LOGIC_PASS`）
- `D19-L06`：泊位领域状态系统（`LOGIC_PASS`）
- `D19-L08`：登船/下船工作流（`LOGIC_PASS`）
- `D19-L16`：事件契约与 ID（`LOGIC_PASS`）
- `D19-L17`：失败码目录（`LOGIC_PASS`）
- `D19-L22`：命令幂等控制（`LOGIC_PASS`）
- 从 `D19-L01` 明确了既有 Save 系统的公开扩展点与边界规则。

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19PortSaveProjection.cs`（新建，纯 C# 港口保存/恢复纯投影模型）
- `game/tools/results/D19/D19-L20/D19_L20_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L20_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- 状态总览由 `python tools/sync_logic_task_status.py` 刷新（唯一共享写者）

未修改任何 Save 根模型（`GameState`/`PlayerState`）、未触碰数据库/JSON 磁盘直接操作、未改动 contracts/Unity 内容。

## 纯投影模型设计（`D19PortSaveProjection`）

涵盖七大可保存/可恢复纯事实维度：

1. **访问状态（Visit State）**：
   - 字段：`ShipId`, `PortId`, `VisitState`（字符串形式，映射自 `D19PortVisitState`）, `BerthId`, `WorldDay`, `SaveRevision`。
   - 辅助方法：`D19PortSaveProjectionMapper.ParseVisitState`。
2. **泊位租期与分配（Berth Allocations）**：
   - 条目：`D19BerthAllocationSaveEntry`（`BerthId`, `State`, `ShipId`, `CommandId`, `ReservedWorldDay`, `ExpiresWorldDay`, `ReleaseReason`）。
   - 恢复：`D19PortSaveProjectionMapper.RestoreBerthAllocations` 转化为 `D19BerthAllocation` 列表。
3. **港口服务状态（Service States）**：
   - 条目：`D19PortServiceSaveEntry`（`ServiceId`, `PortId`, `FacilityState`, `Staffed`, `LastUpdatedWorldDay`）。
4. **成员逻辑位置（Member Locations）**：
   - 条目：`D19MemberLocationSaveEntry`（`MemberId`, `Location`："OnShore" / "OnBoard"）。
   - 恢复：`D19PortSaveProjectionMapper.RestoreMemberLocations` 转化为 `Dictionary<string, D19CrewLocation>`。
5. **命令幂等记录（Idempotency）**：
   - 结构：直接内嵌消费 `D19IdempotencySaveProjection`（D19-L22 提供的最小可保存幂等投影）。
6. **未完成命令与补偿草案（Pending Commands & Compensation Drafts）**：
   - 未完成命令条目：`D19PendingCommandSaveEntry`（`CommandId`, `Kind`, `Fingerprint`, `PortId`, `BerthId`, `ActorId`, `WorldDay`, `SaveRevision`, `Payload`）。
   - 补偿草案条目：`D19CompensationDraftSaveEntry`（`DraftId`, `OriginalCommandId`, `Domain`, `Reason`, `Status`, `WorldDay`, `SaveRevision`, `Parameters`）。
7. **事件游标与版本（Event Cursors & Versioning）**：
   - 字段：`SchemaVersion`（1）、`LastEventId`、`LastEventSequence`。
   - 校验：`Validate()` 严格检查版本不匹配（非 1 抛出 `InvalidOperationException`）、负数逻辑时间/修订号、子条目有效性。

## 铁律与边界遵循

- **数据所有权唯一**：每个 D19 可变事实有唯一投影位置；
- **零全局静态状态**：类均为纯 DTO 与纯函数静态映射器，无可变 static 状态；
- **零 Unity 依赖**：无 `UnityEngine` 引用，不持久化任何 `Transform`、`GameObject`、`Canvas` 或 UI 视图；
- **稳定与版本化**：标量与列表结构化组织，无绝对路径、场景引用；
- **旧 Save 缺字段处理**：安全默认值 `CreateDefault` 与版本标识已建立，旧档缺字段安全回退留给 `D19-L21` 实现。

## 验证证据

| 检查项 | 命令 / 工具 | 结果 |
|---|---|---|
| 纯 C# 编译 | `dotnet run`（依赖 13 个关联 C# 文件） | PASS（0 错误 0 警告） |
| 18 项确定性断言 | 包含默认值、建造者、克隆完整性、恢复映射、SchemaVersion 校验、负数时间拒绝等 | ALL 18 ASSERTIONS PASSED |
| 边界与静态检查 | 代码审计 | PASS（无 Unity/时钟/路径依赖） |
| 状态总览同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L20 = LOGIC_PASS） |

## 后续任务提示

- `D19-L21`（旧存档缺 D19 字段迁移）：消费 `D19PortSaveProjection` 的安全默认值（`CreateDefault`）与 `SchemaVersion`，实现平滑迁移与兼容。
