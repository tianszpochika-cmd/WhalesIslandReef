# 服务端架构与选型（server/）

> 状态：**已定案**（2026-08-07）｜ 对应 `docs/ARCHITECTURE.md` 第 4 节
> 决策原则：个人项目优先"一套语言、最少运维、AI 生态够用"，长期演进能力其次。

---

## 1. 选型结论

| 项 | 选择 | 理由 |
|---|---|---|
| 语言/运行时 | **Node.js 22 LTS + TypeScript** | 与官网（Next.js）同一语言栈，一套 TS 心智；WebSocket 原生支持；个人项目开发效率最高 |
| 框架 | **NestJS** | 模块化（auth/billing/gateway 天然分模块）、DI、测试友好；长期要演化出运营后台/官网 API，收益大 |
| ORM | **Prisma** | 类型安全、迁移（migrations）开箱即用，适合个人项目的 PostgreSQL 建模 |
| 数据库 | **PostgreSQL** | 用户/钱包/流水强一致（事务），Postgres 是标配 |
| 缓存/限流 | **Redis（ioredis）** | 限流计数、会话缓存、热数据 |
| LLM 上游接入 | 直接 HTTP 调各上游（Ollama/DeepSeek/OpenAI） | 不引 LiteLLM 等重框架，自写 router/metering 更可控（核心逻辑自己掌握） |
| WebSocket | `@nestjs/websockets` + `socket.io` | 对话流式推送 |

**为什么不选 Python(FastAPI)**：AI 库生态强，但与官网（Next.js）是两套语言，个人维护成本翻倍；本项目的 LLM 网关本质是 HTTP 转发+计量，Node 完全够。**为什么不选 Go**：开发速度与生态对个人项目不划算。

## 2. 目录职责（与根架构一致）

```
src/
├── main.ts                 # 启动：端口、CORS、全局前缀 /api/v1、Swagger
├── app.module.ts           # 根模块：装配 auth/billing/gateway/ws/worker
├── config/
│   └── env.ts              # 环境变量读取与校验（.env 不入库）
├── modules/
│   ├── health/             # GET /health 存活探针（部署用）
│   ├── auth/               # Steam OpenID 关联 / 邮箱注册 / JWT 签发刷新
│   ├── billing/            # 钱包余额 / 点数-上游 token 换算 / 流水 / webhooks
│   ├── gateway/            # ★ LLM 网关：上游注册、路由(本地优先→云端)、metering 计量、quota 配额
│   ├── ws/                 # WebSocket 网关：对话流式推送、游戏内实时通道
│   └── worker/             # 定时任务：日结算、用量统计、告警
```

## 3. 配置规范（.env.example 已就位）

| 变量 | 示例 | 说明 |
|---|---|---|
| `PORT` | 3000 | 服务端口 |
| `DATABASE_URL` | postgres://user:pass@localhost:5432/whalebone | Prisma 连接串 |
| `REDIS_URL` | redis://localhost:6379 | 限流/缓存 |
| `JWT_SECRET` | 随机长串 | 会话签名 |
| `DEEPSEEK_API_KEY` | sk-xxx | 云端 LLM（可空） |
| `OLLAMA_URL` | http://127.0.0.1:11434 | 本地 LLM |
| `STEAM_WEB_API_KEY` | 由 Steamworks 后台获取 | Steam 关联登录 |

## 4. 运行方式

```bash
cd server
npm install        # 已装
npm run dev        # tsx watch 热重载（开发）
npm run build      # tsc 编译
npm run start      # node dist/main.js（生产）
```

## 5. 本阶段边界（等需求文档/玩法关键词确认后再扩展）

- [x] 骨架可启动（/health 通）
- [ ] auth 具体流程（Steam 回调细节，需 Steamworks 账号就绪）
- [ ] billing 定价与免费额度（依赖 token 商业模型，见步骤③）
- [ ] gateway 上游真实接入与 metering 计费字段（依赖玩法定的对话频度）
- [ ] DB 迁移落地（依赖 DB_SCHEMA 定稿）
