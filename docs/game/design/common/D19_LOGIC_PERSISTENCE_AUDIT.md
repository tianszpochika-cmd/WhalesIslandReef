# D19 港口领域逻辑持久化与所有权审计报告

> 归属：`docs/game/design/common/D19_LOGIC_PERSISTENCE_AUDIT.md`（D19-L44 唯一持久化审计产物）  
> 审计类型：只读静态与契约审计（基于 L20/L21/L38/L39/L40/L41 的真实执行与测试证据）

---

## 一、可变事实与所有权矩阵

| 业务可变事实 | 唯一写者系统（Single Writer） | 持久化承载位置 (`D19PortSaveProjection`) | 加载与迁移恢复路径 | 往返测试证据 | 误写外部字段审计 |
|---|---|---|---|---|---|
| **港口访问状态** (`VisitState`) | `D19PortVisitSystem` (L03) | `projection.VisitState` | `D19PortSaveProjectionMapper.BuildProjection` / `RestoreVisitSystem` | `D19PortSaveLoadTests.cs` (Node 1/2/5, PASS) | **零违规**（L40 静态审计验证） |
| **当前停靠港** (`PortId`) | `D19PortVisitSystem` (L03) | `projection.PortId` | 映射至 `PortVisitSystem._portId` | `D19PortSaveLoadTests.cs` (Node 1, PASS) | **零违规** |
| **当前占用泊位** (`BerthId`) | `D19PortVisitSystem` (L03) / `D19BerthSystem` (L06) | `projection.BerthId` | 映射至 `PortVisitSystem._berthId` | `D19PortSaveLoadTests.cs` (Node 2, PASS) | **零违规** |
| **泊位分配与租期表** (`Allocations`) | `D19BerthSystem` (L06) | `projection.BerthAllocations` (`D19BerthAllocationSaveEntry`) | `RestoreBerthAllocations` 还原至 `_byBerth` | `D19PortSaveLoadTests.cs` (Node 2, PASS) | **零违规** |
| **船员逻辑位置** (`CrewLocation`) | `D19BoardingWorkflow` (L08) | `projection.MemberLocations` (`D19MemberLocationSaveEntry`) | `RestoreMemberLocations` 还原为位置字典 | `D19PortSaveLoadTests.cs` (Node 3, PASS) | **零违规** |
| **港口服务状态** (`ServiceStates`) | `D19PortAvailabilitySystem` (L05) | `projection.ServiceStates` (`D19PortServiceSaveEntry`) | 深拷贝还原至服务可用性列表 | `D19PortSaveLoadTests.cs` (Node 2, PASS) | **零违规** |
| **命令幂等记录** (`Idempotency`) | `D19CommandIdempotency` (L22) | `projection.Idempotency` (`D19IdempotencySaveProjection`) | `idemp.RestoreFromProjection` | `D19PortSaveLoadTests.cs` (Node 1, PASS) | **零违规** |
| **挂起未完成命令** (`PendingCommands`) | `D19PortCommandRouter` (L33) | `projection.PendingCommands` (`D19PendingCommandSaveEntry`) | 深度克隆与重放对账 | `D19PortSaveLoadTests.cs` (Node 4, PASS) | **零违规** |
| **补偿草案记录** (`CompensationDrafts`) | `D19PortCompensation` (L34) | `projection.CompensationDrafts` (`D19CompensationDraftSaveEntry`) | 供中断恢复后逆序对账执行 | `D19PortSaveLoadTests.cs` (Node 4, PASS) | **零违规** |
| **事件游标与版本** (`Cursors/Version`) | `D19PortEventOrderAudit` (L35) | `SchemaVersion`, `SaveRevision`, `LastEventId`, `LastEventSequence` | 版本检查与单调性校验 | `D19LegacySaveTests.cs` (L39, PASS) | **零违规** |

---

## 二、非法状态与旧档迁移策略审计

1. **缺失 D19 数据块**：
   - 策略：由 `D19PortSaveMigration.Migrate` 安全调用 `CreateDefault`，生成合规 `SchemaVersion=1`，船员默认 `OnShore`；
   - 证据：`D19LegacySaveTests.LegacyFixture1_MissingD19Block_ReturnsSafeDefaultProjection` (PASS)。
2. **废弃锚点与旧键别名**：
   - 策略：自动规范化为 `port_*` / `berth_*` / `service_*`，保留 `MigrationHint`；
   - 证据：`D19LegacySaveTests.LegacyFixture2_DeprecatedAliases_MigratedToCanonicalWithHints` (PASS)。
3. **不可能状态与数据损失**：
   - 策略：处于 `Berthed` 状态缺失泊位或港口键时，判定为不可逆数据损失并拒绝，**绝不伪造泊位、成员位置或费用**；
   - 证据：`D19LegacySaveTests.LegacyFixture5_DataLossInBerthedState_RejectedWithoutForgingBerth` (PASS)。
4. **跨系统隔离与禁止双写**：
   - 策略：D19 零单例、零 static 业务状态；严禁私自直写 `GameState`、`PlayerState`、`D3TradeState`、`D4ShipyardSystem` 或 `D9PortShipPreparationSystem`；
   - 证据：`tools/d19_logic_boundary_audit.py` (Rule D19-AUDIT-004, 38 文件 0 违规)。

---

## 三、审计结论

- **无无主可变状态**：全部 10 项可变业务事实均明确归属于单一领域系统；
- **无不可保存全局状态**：全部持久化结构为可序列化纯 DTO；
- **无双写者与越权改动**：所有权清晰，跨系统完全解耦。
- **持久化审计结论**：**`PASS`**。
