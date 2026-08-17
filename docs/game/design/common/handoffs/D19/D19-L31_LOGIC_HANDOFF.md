# D19-L31 交接报告

## 任务信息与结论

- 任务编号：`D19-L31`（波次 19.5，返港与再靠泊领域测试）
- 日期：2026-08-18
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L31/D19_L31_LOGIC_RESULT.json`

## 前置核验

精确前置检查通过：
- `D19-L06`：泊位原子领域系统（`LOGIC_PASS`）
- `D19-L07`：靠泊前置规则纯校验（`LOGIC_PASS`）
- `D19-L10`：返港工作流编排（`LOGIC_PASS`）
- `D19-L16`：港口事件契约（`LOGIC_PASS`）
- `D19-L17`：失败码目录（`LOGIC_PASS`）
- `D19-L21`：旧存档迁移（`LOGIC_PASS`）
- `D19-L22`：命令幂等控制（`LOGIC_PASS`）
- `D19-L24`：集中式不变式/状态守卫（`LOGIC_PASS`）

## 实际修改文件（唯一写入范围）

- `game/Assets/Tests/EditMode/D19/D19ArrivalReturnTests.cs`（新建，EditMode 纯确定性单元测试）
- `game/tools/results/D19/D19-L31/D19_L31_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L31_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-18.md`（追加当日记忆）
- 状态总览由 `python tools/sync_logic_task_status.py` 刷新（唯一共享写者）

未修改任何生产代码、未修改 Save 根模型（`GameState`/`PlayerState`）、未触碰 GameObject/Player/NavMesh/真实坐标与物理碰撞。

## 测试覆盖范围（7 项纯确定性测试）

1. **合法返港与泊位分配（`Arrival_Success_FromDeparted_AllocatesBerth_MovesToBerthed_GeneratesVoyageEndIntent`）**：
   - 验证在航（`Departed`）状态成功返港并分配泊位，推进至 `Berthed`，泊位状态转为 `Occupied`，生成纯数据航程结束意图 `D19VoyageEndIntent`（`Executable=true`/`SettlementReady=true`），恢复目标港可用服务列表（稳定字典序）。
2. **无空泊位直接返港（`Arrival_Success_NoTargetBerth_MovesToReturned_NoBerthAllocated`）**：
   - 验证未指定目标泊位时返港成功推进至 `Returned`，不分配任何泊位，产出有效航程结束意图。
3. **他人占用泊位拒绝（`Arrival_BerthOccupiedByOther_Rejects_KeepsDepartedState_NoSideEffect`）**：
   - 验证目标泊位被他人占用时拒绝返港靠泊（`BerthOccupied`），在航状态保持为 `Departed`，绝不把在航状态伪写为返港，原占用事实不变。
4. **船型能力不匹配拒绝（`Arrival_ShipTierIncompatible_Rejects_NoBerthAllocation`）**：
   - 验证船只尺寸超出泊位上限时拦截，在航状态不变，无泊位分配。
5. **非法前态拒绝（`Arrival_InvalidPrestate_BoardedOrInService_Rejects`）**：
   - 验证在港服务中（`InService`）等非在航前态执行返港时被拦截（`InvalidTransition`），状态保持原态。
6. **重复返港命令幂等控制（`Arrival_DuplicateCommand_SameParams_ReplaysResultWithoutSecondSideEffect`）**：
   - 验证相同 CommandId 重复投递时返回既有结果（`DuplicateSubmission=true`），绝无第二次分配或事件副作用。
7. **中断与状态机拒绝自动补偿（`Arrival_VisitSystemRejects_TriggersCompensation_ReleasesBerth_NoOrphanBerth`）**：
   - 验证底层状态机迁移被拒绝时自动触发确定性补偿，释放已分配泊位回 `Available`，回滚在航状态，生成带 `cause_ids` 的补偿事件草案，消除孤儿泊位与双重航程状态。

## 验证执行与断言结果

| 检查项 | 工具 / 命令 | 结果 |
|---|---|---|
| 纯 C# / NUnit 编译与执行 | .NET 9.0 运行套件 | PASS（7 项测试全部 PASS，0 失败） |
| 边界与红线自检 | 源码扫描 | PASS（零 Unity/GameObject/Player/坐标/NavMesh依赖，零生产文件修改） |
| 状态总览同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L31 = LOGIC_PASS） |
| Git 差异检查 | `git diff --check` | PASS（exit 0） |

## 后续任务提示

- 返港与再靠泊测试的完备覆盖为后续 `D19-L33`（命令路由）与 `D19-L36`（确定性港口剧本：到港→服务→登船→离港→返港）提供了闭环测试依据。
