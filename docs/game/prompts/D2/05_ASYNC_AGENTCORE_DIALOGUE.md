# D2 异步任务 05：AgentCore 对话、校验与 Mock

你负责对话领域核心。前置条件：同步任务 01 已完成，并且已阅读 `docs/game/design/D2_SCOPE.md`。

## 负责范围

- `game/AgentCore/`。
- Unity 镜像目录 `game/Assets/Scripts/Agent/AgentCore/` 只有在确认现有同步方式后才更新，必须保持可编译一致。
- 相关纯 C# 测试和 README。

## 实现目标

复用并完善现有 `DialogueSession`、`AgentReply`、`LLMClient` 体系：

- 本地 Ollama 优先；不可用时自动 Mock/模板回复。
- 原始回复必须经过 `contracts/agent-reply.schema.json` 对应的校验。
- 非法回复最多重试一次，然后使用模板兜底；非法数据不得进入逻辑层。
- `affinity_change` 只作为经过校验的提议返回，由后续系统决定是否落库。
- 记录对话上下文，但不要在本任务加入游戏 UI 或 SaveSystem 写入。

## 硬约束

- AgentCore 零 Unity 依赖、零第三方依赖，必须可独立构建。
- 不把对话结果直接写入船体、经济、任务、奖励等数值系统。
- Schema 如需变更，必须先报告并同步 game/server/web；不能私自改一份。

## 验证

- 测试正常 JSON、缺字段、越界好感、非法 emotion、空 speech、Ollama 超时和模板兜底。
- `dotnet build game/AgentCore/AgentCore.csproj --nologo` 必须 0 警告/0 错误。
- 运行现有 AgentCore 测试并更新 README/当日记忆日志。

返回接口变化、测试结果、Mock 使用方法和给 06/08/09 任务的集成说明。
