# D9-A04 任务交接报告

任务：`D9-A04` 船员 Agent 垂直切片  
日期：2026-08-10  
前置：`D9-S01` 已完成  
状态：实现完成，交由 D9-S02 集成验收

## 主成果

- `game/Assets/Scripts/Agent/AgentCore/D9CrewAgentVerticalSlice.cs`
  - 新增 `CrewDuty`、事实摘要、typed proposal、关系裁决结果；
  - 新增 `D9CrewAgentVerticalSlice.CreateIsaacReed()`，固定 `crew_isaac_reed` Persona、岗位和 D9 提议上下文；
  - 新增 `D9OfflineTemplateClient` 和航线/岗位本地模板；
  - 新增 `CrewRelationArbiter`，缺少 `cause_event_ids` 时拒绝关系提议；不直接写关系。
- `game/Assets/Scripts/Agent/AgentCore/D9CrewAgentVerticalSliceTests.cs`：D9-A04 纯 C# 6 项专项测试；`game/Assets/Tests/EditMode/D9CrewRelationSystemTests.cs`：强逻辑关系写入口测试。
- `game/AgentCore/AgentCore.csproj`：将 D9 运行时代码和测试以 linked compile 收录，保证 Unity/AgentCore 共用源文件。
- `game/AgentCore/AgentModels.cs`、`game/Assets/Scripts/Agent/AgentCore/AgentModels.cs`：Persona 增加主岗位/可胜任岗位，并写入快照。
- `game/AgentCore/AgentMemory.cs`、`game/Assets/Scripts/Agent/AgentCore/AgentMemory.cs`：prompt 包含最近 8 条短期上下文，并提供只读读取入口。
- `game/AgentCore/DialogueSession.cs`、`game/Assets/Scripts/Agent/AgentCore/DialogueSession.cs`：D9 可在授权快照下校验动作提议，模型失败转 D9 模板；D2 默认构造行为保持不变。
- `game/Assets/Scripts/Systems/D9/D9CrewRecruitmentCatalog.cs`：改为复用 D9 canonical Isaac Reed 工厂，清理重复 Persona 定义。
- `game/Assets/Scripts/Systems/D2/CrewRelationSystem.cs`：新增接收 `CrewAgentProposal` 的严格关系入口；预检委托 `CrewRelationArbiter`，实际 `CrewStats.Affinity` 写入留在 CrewRelationSystem。
- `docs/game/design/common/D9_CREW_AGENT_VERTICAL_SLICE.md`：设计说明、所有权、模板、提议和测试入口。

## 新增配置/参数与来源登记

本任务没有新增经济、奖励、战斗或航速平衡参数。新增的 D9 统一入口值为：

| 名称 | 类型/单位 | 默认或固定值 | 所有者/来源 | 测试入口 |
|---|---|---|---|---|
| `CrewDuty` | C# 枚举/岗位 | `Deckhand` 主岗位；`Navigator`、`Lookout`、`Repair` 可胜任 | CrewSystem 写编制；D9-S01 `crew_isaac_reed` 冻结样例 | `PersonaAndDutyProfile`、`OfflineDutyTemplate` |
| `AgentMemory.MaxShortContext` | `int`/对话条目 | 8 | AgentMemory | `FactSummaryFeedsMemory` |
| `IsaacReedId` | 稳定 ID | `crew_isaac_reed` | D8 内容注册表、D9-S01 | `PersonaAndDutyProfile` |
| `ShipMoonwakeRef` | 稳定引用 | `ship:ship_moonwake` | D9-S01 | `OfflineRouteTemplate` |
| `RelationTargetRef` | 稳定引用 | `player:captain` | D9 Agent 关系提议入口 | `RelationRequiresCauseEvent` |

上述值是垂直切片身份/接口入口，不是经济平衡参数；没有改变现有价格、工资、奖励、战斗或航行公式。

## 替换的硬编码

- 招募目录原先直接组装 Isaac Reed 的 Persona；现在由 `D9CrewAgentVerticalSlice.CreateIsaacReed()` 单一入口组装，避免招募路径和对话路径出现两份岗位/特质定义。
- 原有 `PersonaDefinition` 没有岗位类型；现在增加 `PrimaryDuty` 与 `QualifiedDuties`，Agent 只读取，CrewSystem 仍负责实际编制写入。
- 原有 D9 路线/岗位提示没有 typed 投影；现在模板和模型输出统一经过 `AgentReplyValidator`/`ActionValidationContext`（模型路径）和 `CrewAgentProposalSet`（两条路径），不再让 UI 或 Agent 执行命令。
- 原有记忆 prompt 只拼接长期 Top 3；现在同时拼接最近 8 条短期对话，事实摘要仍保留 `event_id` 与 `cause_event_ids`。

没有把最终数值、最终奖励或最终任务结果复制到 AgentCore。

## 关系变化与事件追溯

`CrewAgentProposalSet` 从已记录且合法的 `CrewFactSummary.EventId` 生成 `CauseEventIds`。`CrewRelationArbiter` 的关系 proposal 没有原因时返回 `MissingCauseEvent`，不写 `CrewStats.Affinity`；有原因且通过边界后，返回包含 cause IDs 的 `CrewRelationDecision`，再由 `CrewRelationSystem` 执行唯一写入。D9 正式运行时应由 CrewSystem/EventLog 先记录事实（包括 `event_d9_crew_report`），再将事实摘要交给 Agent 并调用严格关系入口。

D2 旧 `AgentReply` 入口为兼容已有 D2 EditMode/Save 测试而保留；D9 不使用它。D9-S02 应继续推动所有 D9 关系写入切换到 cause-bound proposal overload。

## 配置检查和测试结果

执行命令：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --d9-check
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --mock
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check
python tools/d9_vertical_slice_check.py
python -m unittest tools.tests.test_d9_vertical_slice_freeze -v
git diff --check
```

本次 AgentCore/D9 已执行结果：

- AgentCore build：0 warning / 0 error；
- D9 专项 `--d9-check`：6/6 通过；
- 全量 AgentCore `--mock`：全部既有自测与 D9 测试通过；无网络 Mock 链路正常退出；
- 数值配置 `--config-check`：5/5 通过；
- `python tools/d9_vertical_slice_check.py`：通过（3 scenes / 5 prefabs / 10 commodities / 14 events / 6 UI / 7 agents）；
- `python -m unittest tools.tests.test_d9_vertical_slice_freeze -v`：1/1 通过；
- `git diff --check`：通过；输出仅为工作区既有的 LF→CRLF 提示，无空白错误。

## 集成注意

- AgentCore 没有 Unity、网络或第三方依赖；`Systems` 中未加入网络/LLM。
- `DialogueSession` 的 D9 构造只允许经授权的 `set_course`/`assign_duty` 提议，不执行动作。
- UI/Agent 不得直接写航速、钱、关系、岗位、任务或奖励；接线必须把 typed proposal 交给对应 System。
- 无模型模板是离线默认路径，模型不可用时不阻断 D9 对话。
- 未修改 contracts；D9 事件仍需 D9-S02 接入 D8 EventLog 和 SaveSystem 的正式 owner 链。
