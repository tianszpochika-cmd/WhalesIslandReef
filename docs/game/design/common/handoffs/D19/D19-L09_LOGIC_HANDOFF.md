# D19-L09 交接报告

## 任务信息与结论

- 任务编号：`D19-L09`（波次 19.4，离港工作流确定性编排）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L09/D19_L09_LOGIC_RESULT.json`

## 前置核验（7 项精确前置已全部 LOGIC_PASS）

| 前置 | 状态 | 成果文件 / 接口 |
|---|---|---|
| D19-L07 靠泊规则 | LOGIC_PASS | `D19DockingRules.cs` |
| D19-L08 登船/下船工作流 | LOGIC_PASS | `D19BoardingWorkflow.cs` |
| D19-L15 出航准备适配器 | LOGIC_PASS | `D19PreparationPortAdapter.cs` |
| D19-L16 港口事件契约 | LOGIC_PASS | `D19PortEventContracts.cs` |
| D19-L17 失败码目录 | LOGIC_PASS | `D19PortFailureCatalog.cs` |
| D19-L22 命令幂等控制 | LOGIC_PASS | `D19CommandIdempotency.cs` |
| D19-L24 集中式不变式/非法状态防护 | LOGIC_PASS | `D19InvalidStateGuard.cs` |

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19DepartureWorkflow.cs`（新建，纯 C# 离港工作流确定性编排器）
- `game/tools/results/D19/D19-L09/D19_L09_LOGIC_RESULT.json`（新建，LOGIC_PASS）
- `docs/game/design/common/handoffs/D19/D19-L09_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- 状态总览由 `python tools/sync_logic_task_status.py` 刷新（唯一共享写者）

未直接修改 SailingSystem、船只、库存或船员内部数值；未修改 D2/D4/D9、UI、contracts。

## 核心实现与执行管线

1. **幂等拦截（L22）**：
   - 使用 `D19CommandIdempotency.Begin(D19IdempotencyDomain.Departure, ...)` 与确定性指纹；
   - 拦截重复 `CommandId`，重复提交直接重放已存结果且补发 0 个新事件；异参冲突直接阻断。
2. **格式与安全前置（L24）**：
   - 命令 ID 校验、出发港口合法性校验、目的地有效性与别名解析（`DestinationId != PortId`）。
3. **权限校验（L23）**：
   - 校验 `D19AuthorityClaims`（跨船、跨玩家越权拒绝）。
4. **生命周期校验（L02 / L24）**：
   - 验证允许的前态（`Boarded` 或 `PortVisit` 直达），通过 `D19InvalidStateGuard.CanTransition` 判定。
5. **船员位置核验（L08）**：
   - 检查 `CrewLocations`，申报船员必须全员处于 `OnBoard` 状态，存在 `OnShore` 立即拒绝。
6. **出航准备清单核验（L15）**：
   - 消费 `D19DepartureReadinessSummary`，`CanDepart=false` 或 `HasUnknown=true` 时立即阻断并保留首个 blocker。
7. **离泊释放编排（L06）**：
   - 若持有泊位，调用 `D19BerthSystem.Release` 释放泊位并获取释放事件草案；离泊失败立即终止。
8. **状态机推进与补偿保护（L03 / L16 / L34）**：
   - 驱动 `D19PortVisitSystem.Execute`（`Depart` 命令）推进访问状态至 `Departed`；
   - 若状态机推进失败，生成带有原始事件 ID 的 `D19PortEventFactory.CreateCompensation` 补偿事件草案，原状态恢复，绝不留下“已离泊未离港”的半状态。
9. **成功事件与幂等登记**：
   - 构造 `D19PortEventType.Departure` 事件草案；
   - 调用 `_idempotency.Complete` 登记完成记录。

## 验证证据

| 验证项 | 命令 / 方式 | 结果 |
|---|---|---|
| 前置任务核验 | 7 项前置 JSON 与源码核验 | PASS（7 项全部 LOGIC_PASS） |
| 纯 C# 独立编译 | dotnet 9.0 build | PASS（0 警告 0 错误） |
| 5 组确定性断言 | 单元测试全覆盖执行 | PASS（全流程成功释放泊位+离港、幂等重放零新事件、船员未登船拒绝、出航准备不满足阻断列表透传、同港目的地拒绝） |
| 规范审计 | 源码静态扫描 | PASS（零 UI/Transform 引用、零直接改写数值、全事件通过 L16 契约） |
| Git 差异检查 | `git diff --check` | PASS（exit 0，仅既有 LF/CRLF 警告） |
| 状态同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L09=LOGIC_PASS 刷新完成） |

## 明确未宣称

- 未宣称任何实际航行画面、水体渲染、玩家控制或 HUD 按钮绑定（属于后续 Presentation / Runtime 阶段）。
- 未宣称 `RuntimeApproved/ReleaseApproved`（授予仍为 0）。
