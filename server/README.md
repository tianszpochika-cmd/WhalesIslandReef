# Whalebone Server — AI 服务端

账号注册 / token 配额计量 / LLM 网关（游戏内对话的唯一入口）。

## 技术栈（已定案，见 docs/server/ARCHITECTURE.md）

NestJS 11 + TypeScript 7 + Express ｜ Prisma(待装) + PostgreSQL ｜ Redis(ioredis, 待装)

## 当前状态（2026-08-07）

✅ 骨架可启动，已验证：`/api/v1/health`、`/api/v1/wallet`、`/api/v1/auth/steam`
⬜ 待需求文档（玩法关键词）确认后：auth 流程、billing 定价、gateway 上游接入、DB 迁移

## 运行

```bash
npm install
npm run dev      # 开发（tsx watch）
npm run build    # 编译
npm run start    # 运行（需先 build）
```

## 模块

| 模块 | 路径 | 状态 |
|---|---|---|
| health | src/modules/health | ✅ 可用 |
| auth（Steam/邮箱/JWT） | src/modules/auth | 🟡 占位 |
| billing（钱包/token 双轨） | src/modules/billing | 🟡 占位 |
| gateway（LLM 网关） | src/modules/gateway | 🟡 占位（核心待做） |
| worker（定时任务） | src/modules/worker | ⬜ 占位 |
| ws（对话流式推送） | src/modules/ws | ⬜ 待建（需 @nestjs/websockets） |

## D2 开发态对话入口

`POST /api/v1/agent/chat` 已提供开发态 Mock 返回，严格输出 `contracts/agent-reply.schema.json` 规定的字段。请求示例：

```json
{"crew_id":"crew-old-sailor","player_input":"天气如何？"}
```

真实 JWT、点数、Ollama/云端路由和流式推送仍在网关模块边界内，D2 本地游戏不依赖它们才能完成对话。

## 环境变量

见 `.env.example`（复制为 `.env`，本地可不配，默认值可用；生产必须全量）。
