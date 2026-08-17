# D19-L08 交接报告

## 任务信息与结论

- 任务编号：`D19-L08`（波次 1；精确前置 L02/L03/L16/L17/L23/L24 均 LOGIC_PASS 已核验）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L08/D19_L08_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19BoardingWorkflow.cs`（新建，纯业务登船/下船工作流）
- `game/tools/results/D19/D19-L08/D19_L08_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L08_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加）

未改 L02/L03/L16/L17/L23/L24、D2/D3/D4/D9/Save 或 Unity 内容；未复制 lifecycle/event/failure/guard 类型（只读引用）。

## 实现内容

| 类型 | 内容 |
|---|---|
| `D19BoardingRequestType` | Board / Disembark |
| `D19BoardingRequest` | CommandId/ActorId/ShipId/PortId/Type/MemberIds（稳定实体 ID） |
| `D19MemberPosition` | MemberId + TargetLocation（`D19CrewLocation` 逻辑枚举）+ Moved + L24 `D19GuardVerdict` |
| `D19BoardingResult` | Success/CommandId/Members/ProposedLocations/Events/Failure/Rejection |

**确定性校验链**：命令 ID → 授权（L23 `Validate`）→ 生命周期状态（Board 经 L24 `CheckCommand`；Disembark 要求船在港口且不迁移生命周期）→ 船一致 → 每成员（未知成员 `unknown_member`；L24 `CheckCrewLocation` 已在目标位置拒绝）。

**原子性与幂等**：
- 部分成员失败 → **原子拒绝**：每成员保留各自 verdict（不把首个失败分发给其它成员）、零移动（无 ProposedLocations，杜绝"船上/岸上"双重状态）、1 个 L16 拒绝事件草案；
- 成功 → 全部通过后输出 `ProposedLocations`（调用方应用；本工作流不写 D20/船员/Save）+ 1 个 Boarded/Disembarked 事件；
- 同一 CommandId 重复 → 返回既有稳定结果（字段复制、Events 置空、零副作用）。

## 验证

| 命令 | 结果 |
|---|---|
| Roslyn csc.dll `/target:library`（本任务依赖链 9 文件 + EventContracts） | exit 0（旧 .NET Framework csc 不支持 L04 的 C#6 插值，改用 dotnet SDK Roslyn） |
| 临时冒烟程序（dotnet 运行） | **RUN exit 0**：登船/下船成功各 1 事件 1 移动；幂等 0 事件；已在目标位置拒绝（原子 prop=0）；未知成员拒绝；非法生命周期拒绝；无权拒绝；部分失败原子拒绝且成员各自 verdict |
| 静态自检（无 Transform/动画/碰撞/输入；不直接改 D20/船员/Save） | PASS |
| `python tools/sync_logic_task_status.py` | D19-L08 = LOGIC_PASS |
| `git diff --check` | exit 0，仅既有 LF→CRLF 警告 |

**冒烟中发现并修复 2 处缺陷**：①原子拒绝曾把首个失败 verdict 分发给所有成员（已改为保留每成员各自校验结果）；②幂等命中曾返回含缓存事件的对象（已改为复制字段且 Events 置空）。

## Runtime Deferred

成员位置实际应用与 Transform/动画/碰撞验证（D20/船员系统）、事件落日志/存档投影运行时行为、真实命令来源（L33 路由）、`RuntimeApproved`/`ReleaseApproved`（均为 0）。

## 后续

- `D19-L09`（离港检查）、`D19-L10`（返港）等可并行；L08 结果供 L30 领域测试与 L33 路由直接消费。
