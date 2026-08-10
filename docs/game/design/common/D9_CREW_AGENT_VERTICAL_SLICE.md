# D9 船员 Agent 垂直切片

任务：`D9-A04`  
状态：已完成（2026-08-10）  
前置：`D9-S01` 垂直切片冻结已完成

## 目标与边界

本切片只实现一个核心船员 `crew_isaac_reed` 的 Persona、岗位能力、短期记忆、D8 事实摘要、对话和提议投影。Agent 只能生成表达和提议，不能直接写入航速、金钱、关系、岗位、任务结果、奖励或战斗结果；最终执行仍由对应强逻辑系统裁决。

实现不调整经济价格、奖励、战斗数值或玩法公式，也没有新增跨端契约。

## 权威实现入口

`game/Assets/Scripts/Agent/AgentCore/D9CrewAgentVerticalSlice.cs` 是 Unity 与纯 C# AgentCore 共用的源文件，`game/AgentCore/AgentCore.csproj` 通过 linked compile 收录它。相关通用入口为：

- `PersonaDefinition`：保存 Persona、主岗位和可胜任岗位；Persona 不写领域状态；
- `AgentMemory`：保留最近 8 条短期上下文，事实摘要以 `fact:<event_id>` 进入只读 Agent 记忆；
- `DialogueSession`：输入 → Persona prompt/记忆 → 模型或模板 → AgentReply 校验 → 返回提议；
- `CrewAgentProposalSet`：把合法 `AgentReply` 投影为路线、岗位或关系 typed proposal；
- `CrewRelationArbiter`：关系提议的纯确定性预检门；最终写入由 `CrewRelationSystem` 完成；
- `D9CrewRecruitmentCatalog`：D9-A01 招募入口复用本切片的 Isaac Reed 工厂，避免再次复制 Persona。

## 核心船员与岗位

| 字段 | 冻结值 |
|---|---|
| ID | `crew_isaac_reed` |
| 显示名 | 艾萨克·里德 |
| Persona 角色 | 水手长 |
| 主岗位 | `CrewDuty.Deckhand` |
| 可胜任岗位 | `Deckhand`、`Navigator`、`Lookout`、`Repair` |
| 特质 | 谨慎、识潮 |
| 说话风格 | 短句、克制，先报风险再提建议 |

`PrimaryDuty` 和 `QualifiedDuties` 只是 Persona/编制建议的输入快照。岗位真正变更必须由 `CrewSystem` 在通过前置条件后写入；Agent 提出的 `assign_duty` 不会改动 Persona 或 Crew 编制。

## 事实摘要与记忆

`CrewFactSummary` 只接受事件摘要字段：`event_id`、`world_day`、`location_ref`、`owner_system`、`result_code`、`summary` 和 `cause_event_ids`。摘要必须先具备完整的事件身份和 owner；重复 `event_id` 不会再次进入该 Agent 的摘要列表。

摘要进入 `AgentMemory` 后只用于 prompt 和记忆检索，AgentCore 不引用、不追加 `EventLog`，也不把摘要当成领域事实写入。D9 风暴等正式事实仍由 `EnvironmentSystem`/`CrewSystem` 按 D8 EventLog 规则写入，船员 Agent 只读取投影。

对话会话将玩家输入和船员表达写入最近 8 条短期上下文，确保下一轮对话能够看到当前交互；长期事实保留 D8 事件 ID 和因果 ID，便于关系提议追溯。

## 对话与无模型模板

`D9OfflineTemplateClient` 明确报告模型不可用并触发本地模板，不访问网络。D9 模板在预算内提供固定可验收表达：

- 输入含“航线/路线/绿湾/村落”：提出 `set_course`，目标 `ship:ship_moonwake`，参数 `destination_ref=port:green_cay`；
- 输入含“岗位/值班/瞭望/安排”：提出 `assign_duty`，目标为 Isaac Reed，岗位建议 `lookout`；
- 其他输入：使用通用本地模板回复，不携带可执行动作。

模型返回的 D9 动作必须通过 `ActionValidationContext`（actor、target、权限和前置条件）与 `AgentReplyValidator`；模板回复是同一结构化 `AgentReply`，仍由 `CrewAgentProposalSet` 投影，绝不在对话层执行。

## 提议与强逻辑裁决

路线和岗位提议只包含 `action_name`、目标引用、目的地/岗位和事实依据文本，不包含最终速度、钱、价格、奖励、伤害、天气或任务结果。实际执行路径如下：

`DialogueSession` → `AgentReply` → `CrewAgentProposalSet` → `ShipSystem`/`CrewSystem` 验证和执行。

关系提议必须带至少一个 `cause_event_ids`。`CrewRelationArbiter` 在缺少原因、船员 ID 不匹配或数值非法时拒绝；通过后返回 `CrewRelationDecision`，其中同时保留 `Before`、待应用的 `AppliedDelta`、预期 `After` 和 `CauseEventIds`。只有 `CrewRelationSystem` 接受该 decision 后才写 `CrewStats.Affinity`，因此关系变化可以追溯到已记录事实，Agent/Persona/UI 都不是关系唯一写者。

D2 既有 `CrewRelationSystem.ApplyAgentProposal(CrewAgent, AgentReply)` 兼容入口仍保留给旧 D2 测试和旧运行时；D9 垂直切片不调用该无事件上下文入口，而使用 `CrewAgentProposal` 的 cause-bound overload。后续 D9-S02 接线时应把正式 `event_d9_crew_report` 和对应事实事件 ID 交给 CrewSystem，再调用严格入口。

## 测试入口

新增 `game/Assets/Scripts/Agent/AgentCore/D9CrewAgentVerticalSliceTests.cs` 作为 AgentCore 纯 C# 自测，并新增 Unity EditMode 的 `D9CrewRelationSystemTests`，覆盖：

1. Persona、主岗位和可胜任岗位；
2. 断网模板提出航线建议；
3. 断网模板提出岗位建议；
4. 事件摘要、因果 ID 和短期对话记忆；
5. 路线/岗位提议不直接修改关系；
6. 无 `cause_event_ids` 的关系提议被拒绝，有事件 ID 的提议才可裁决。

本任务检查命令：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --d9-check
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --mock
git diff --check
```

`--d9-check` 为 6 项 D9 专项测试；`--mock` 继续执行既有 AgentCore 全量自测并使用无网络 Mock 链路。
