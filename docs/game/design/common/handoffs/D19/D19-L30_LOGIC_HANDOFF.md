# D19-L30 交接报告

## 任务信息与结论

- 任务编号：`D19-L30`（波次 19.5，登船与离港测试）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L30/D19_L30_LOGIC_RESULT.json`

## 前置核验

精确前置检查通过：
- `D19-L08`：登船/下船工作流（`LOGIC_PASS`）
- `D19-L09`：离港工作流（`LOGIC_PASS`）
- `D19-L15`：出航准备适配器（`LOGIC_PASS`）
- `D19-L16`：港口事件契约（`LOGIC_PASS`）
- `D19-L17`：失败码目录（`LOGIC_PASS`）
- `D19-L21`：旧存档迁移（`LOGIC_PASS`）
- `D19-L22`：命令幂等控制（`LOGIC_PASS`）
- `D19-L23`：港口权限校验器（`LOGIC_PASS`）
- `D19-L24`：集中式不变式/状态守卫（`LOGIC_PASS`）

## 实际修改文件（唯一写入范围）

- `game/Assets/Tests/EditMode/D19/D19BoardingDepartureTests.cs`（新建，EditMode 纯确定性单元测试）
- `game/tools/results/D19/D19-L30/D19_L30_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L30_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）
- 状态总览由 `python tools/sync_logic_task_status.py` 刷新（唯一共享写者）

未修改任何生产代码、未修改 Save 根模型（`GameState`/`PlayerState`）、未触碰 GameObject/Player/真实输入/D20表现层。

## 测试覆盖范围（10 项纯确定性测试）

1. **登船成功（`Boarding_Success_MembersMoveToOnBoard_GeneratesBoardedEvent`）**：
   - 验证申报成员从 `OnShore` 迁移至 `OnBoard`，产出 `port.boarding.applied` 事件草案，携带稳定 input_facts（command/port/ship）。
2. **下船成功（`Disembark_Success_MembersMoveToOnShore_GeneratesDisembarkedEvent`）**：
   - 验证成员从 `OnBoard` 迁移至 `OnShore`，产出 `port.boarding.disembarked` 事件草案。
3. **成员已在目标位置拒绝（`Boarding_AlreadyOnBoard_AtomicRejection_NoLocationsChanged`）**：
   - 验证部分成员已在船上时，请求被原子拒绝（`AlreadyBoarded`），`ProposedLocations` 为空，零位置变动，产出 1 个 `port.rejected` 事件草案。
4. **跨实体声明权限拒绝（`Boarding_CrossShipClaims_RejectedByAuthority`）**：
   - 验证 `ShipId != claims.ShipId` 跨船声明时权限拦截（`d19.port.permission_denied`）。
5. **未知成员原子拒绝（`Boarding_UnknownMember_AtomicRejection`）**：
   - 验证未在快照中的成员请求被拦截，无任何位置变动。
6. **离港全流程成功（`Departure_Success_FullWorkflow_ReleasesBerth_MovesToDeparted`）**：
   - 验证离港流程依次完成权限校验、船员位置检查、出航准备检查、泊位自动释放（变回 `Available`）、生命周期推进到 `Departed`，按发生顺序产出离泊释放事件草案与离港事件草案。
7. **出航准备未就绪拒绝（`Departure_PreparationNotReady_Rejection_StateAndBerthUnchanged`）**：
   - 验证 `CanDepart=false` 时拦截离港，状态与泊位均保持原态。
8. **船员未全部登船拒绝（`Departure_CrewNotOnBoard_Rejection`）**：
   - 验证存在船员处于 `OnShore` 时离港被拒绝（`NotReadyToDepart`）。
9. **前置状态不满足拒绝与泊位保护（`Departure_VisitSystemRejects_StateAndBerthProtected_NoOrphanState`）**：
   - 验证前置不满足时拒绝离港，泊位保持占用，绝无“已离泊未起航”的孤儿状态。
10. **重复离港命令幂等控制（`Departure_DuplicateCommand_SameParams_ReplaysResultWithoutSecondSideEffect`）**：
    - 验证相同 CommandId 重复提交时返回既有结果（`DuplicateSubmission=true`），绝无二次离泊或事件副作用。

## 验证执行与断言结果

| 检查项 | 工具 / 命令 | 结果 |
|---|---|---|
| 纯 C# / NUnit 编译与执行 | .NET 9.0 运行套件 | PASS（10 项测试全部 PASS，0 失败） |
| 边界与红线自检 | 源码扫描 | PASS（零 Unity/GameObject/Player/输入/物理依赖，零生产文件修改） |
| 状态总览同步 | `python tools/sync_logic_task_status.py` | PASS（D19-L30 = LOGIC_PASS） |
| Git 差异检查 | `git diff --check` | PASS（exit 0） |

## 后续任务提示

- 完整的登船与离港确定性测试为后续 `D19-L33`（命令路由）与 `D19-L36`（确定性港口剧本）提供了可靠的集成验收基线。
