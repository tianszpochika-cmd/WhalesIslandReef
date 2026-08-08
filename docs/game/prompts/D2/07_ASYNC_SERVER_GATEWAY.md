# D2 异步任务 07：服务端 Agent Gateway（非本地闭环阻塞项）

你负责服务端对话网关。前置条件：同步任务 01 已完成，并且已阅读 `docs/game/design/D2_SCOPE.md`。这是可并行任务，不能阻塞本地 Ollama/Mock 游戏闭环。

## 负责范围

- `contracts/`（只有确实需要时才改）。
- `server/src/modules/gateway/` 及服务端测试、README/配置说明。

## 实现目标

- 按 `contracts/openapi/v1.yaml` 实现 `/api/v1/agent/chat`，请求和返回严格符合 AgentReply Schema。
- 明确认证、超时、上游不可用、限流和错误响应；不得把原始非法 LLM 输出直接返回游戏。
- 本地开发提供可重复的 Mock 模式，方便 Unity 或接口测试。
- Ollama/DeepSeek 等上游适配放在 gateway 边界，不能进入游戏 `Systems/`。

## 硬约束

- 改契约时必须说明 game/server/web 三端影响，禁止单端私有字段。
- 不修改 Unity 场景、UI、SaveSystem 和航行逻辑。
- 不提交密钥、真实 Token 或 `.env` 私密值。

## 验证

- `npm run build` 通过；补充正常、非法回复、未授权、超时和限流测试。
- 用 Mock 请求验证返回能通过 `agent-reply.schema.json`。
- 更新服务端 README 和当日记忆日志，报告 API 示例、配置和已知风险。
