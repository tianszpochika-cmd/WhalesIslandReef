# D19-L10 交接报告

## 任务信息与结论

- 任务编号：`D19-L10`（波次 19.4；精确前置 `D19-L03`、`D19-L06`、`D19-L07`、`D19-L16`、`D19-L17`、`D19-L22`、`D19-L24` 均 LOGIC_PASS 已核验）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L10/D19_L10_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19ArrivalWorkflow.cs`（新建，纯业务返港/到港工作流编排）
- `game/tools/results/D19/D19-L10/D19_L10_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L10_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加）
- `docs/game/LOGIC_TASK_STATUS.md`（由 sync 脚本同步更新）

未修改既有系统核心数值、未直接结束 D2 航程或直接写 D3 货币/库存/船员数值；未引用 Unity 坐标、Transform、碰撞、场景或 UI 字符串。

## 实现内容

| 类型 | 内容 |
|---|---|
| `D19VoyageEndIntent` | 纯数据航程结束意图（CommandId/ShipId/PortId/WorldDay/Executable/SettlementReady/Summary/Note），供 D3/D9 消费，不直接结算 |
| `D19ArrivalWorkflowRequest` | CommandId/ActorId/ShipId/PortId/TargetBerthId/CurrentVisitState/VoyageStatus/ShipProfile/AuthorityClaims/FacilityAvailability/ReservationTermDays/ConfirmOccupancy/WorldDay/SaveRevision |
| `D19ArrivalWorkflowResult` | CommandId/Success/DuplicateSubmission/StateAfter/AllocatedBerthId/BerthAllocated/BerthConfirmed/VoyageEndIntent/AvailableServices/Compensated/Failure/FailureVerdict/Events/Note |
| `D19ArrivalWorkflow` | 确定性编排器（幂等→安全校验→港口/权限校验→在航前态校验→泊位分配与占用确认→状态机推进至 Returned/Berthed→航程结束意图→可用服务恢复→确定性补偿与释放） |

### 确定性执行管道

1. **幂等判定（L22）**：相同 CommandId+相同指纹返回既有稳定结果，Events 置空（零二次副作用）；同 CommandId 冲突指纹返回 `DuplicateCommand` 拒绝；
2. **安全与目标港校验（L24 / L04）**：校验命令 ID 合法性与目标港口存在性，获取规范港口键 `canonicalPortId`；
3. **权限校验（L23）**：通过 `D19PortAuthorityValidator.Validate` 校验操作权限，拦截跨玩家、跨船、跨港操作；
4. **在航/返港前态校验（L02 / L24）**：合法前置状态仅限 `Departed`（航程返港）、`None`（初次到港）、`Returned`（已返港重新对接泊位/服务），其它状态（如 `InService`/`Boarded`）拒绝并保持原状态；
5. **泊位规则裁决与分配（L07 / L06）**：
   - 若传入 `TargetBerthId`，先经 `D19DockingRules.Adjudicate` 执行靠泊前置纯规则裁决（尺寸、吃水、占用、跨港等）；
   - 裁决通过后调用 `D19BerthSystem.Reserve` 分配预留；若请求 `ConfirmOccupancy`，调用 `ConfirmOccupancy` 转为占用；若占用确认失败，自动回滚释放预留；
6. **访问状态机推进（L03）与失败补偿保护**：
   - 调用 `D19PortVisitSystem.Execute` 推进状态至 `Returned`（若有泊位则进一步分配至 `Berthed`）；
   - 若状态机拒绝，自动触发**确定性可审计补偿**：释放已分配泊位（杜绝孤儿泊位），发出携带 `cause_ids` 的 L16 补偿事件草案（`PortEventCompensation`），设置 `Compensated=true`；
7. **航程结束意图生成**：构造 `D19VoyageEndIntent`（`Executable=true`、`SettlementReady=true`），交由 D3 `VoyageSettlementSystem` / D9 消费；
8. **港口服务恢复**：遍历锚点目录解析目标港口下全部有效服务键并稳定排序（`AvailableServices`）；
9. **L16 领域事件草案生成**：生成规范 `Return`、`BerthReserved`、`Rejection` 或 `Compensation` 事件草案，全部通过 `D19PortEventGuard` 校验。

## 验证

| 命令 | 结果 |
|---|---|
| `dotnet run --project temp_test_build/TestApp.csproj`（真实 D19 依赖链 + EventContracts + D3Contracts） | **PASS**：6 组测试场景全通过：<br>1. 正常返港含泊位预留与占用确认；<br>2. 无泊位返港返回 Returned 状态；<br>3. 幂等重复投递 0 事件与冲突参数 DuplicateCommand 拒绝；<br>4. 泊位尺寸/吃水/占用/跨港规则拒绝；<br>5. 非法前态拒绝；<br>6. 状态机拒绝时的确定性补偿、释放泊位与孤儿泊位消除 |
| 静态自检（纯逻辑输入驱动，无真实位置/碰撞/场景判断，不直接修改 D2/D3/D4 内部数值） | **PASS** |
| `python tools/sync_logic_task_status.py` | **PASS**：D19-L10 状态更新为 LOGIC_PASS |
| `git diff --check` | **PASS**：exit 0，无空白错误 |

## Runtime Deferred

- 航程实际结算（D19VoyageEndIntent 由 D3/D9 系统在运行时结算消费）；
- 港口服务恢复的运行时 UI/HUD 展示刷新；
- 真实到港场景加载、锚地渲染与物理停泊（属于表现层/Runtime 阶段）；
- `RuntimeApproved`/`ReleaseApproved`（均为 0）。

## 后续

- `D19-L31`（返港/到港领域测试）：可直接测试本工作流与各异常边界；
- `D19-L33`（命令路由总装）：将返港/到港命令接入 `D19ArrivalWorkflow`。
