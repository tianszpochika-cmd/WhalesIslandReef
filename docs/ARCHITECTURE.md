# 项目总体架构与文件层级设计

> 项目：纯 Agent 3D 航海游戏《鲸骨航线》（暂定）｜ 更新：2026-08-07
> 定位：**游戏客户端 + AI 服务端 + 官网** 三端一体的完整工程。
> 关键决策：游戏涉及 AI（LLM）就必须有自有服务器，负责 **账号注册、token 配额计量、LLM 网关**——LLM Key 绝不能进客户端。

---

## 1. 顶层目录（总览）

```
first01/                                  # 项目根（本工作区）
├── README.md                             # 项目总览：是什么、怎么跑、文档索引
├── .gitignore                            # Unity + .NET + Node 混合忽略规则
├── docs/                                 # ★ 全项目文档（单一入口，先看这里）
├── contracts/                            # ★ 跨端契约（单一事实源，语言无关）
├── game/                                 # Unity 游戏客户端
├── server/                               # ★ AI 服务端（账号 / token / LLM 网关）
├── web/                                  # 官网 + 用户中心 + 运营后台
├── ops/                                  # 部署 / 监控 / 脚本
├── tools/                                # 内部开发工具
└── assets/                               # 全局共享素材（非 Unity 内，git-lfs）
```

---

## 2. docs/ —— 文档层（先文档后代码）

```
docs/
├── README.md                 # 文档地图：各文档一句话定位
├── game/
│   ├── GAME_PLAN.md          # ★ 已就位：上架合规 + 游戏计划（原根目录移入）
│   ├── SETUP.md              # ★ 已就位：环境搭建 + 测试 + 素材
│   ├── REQUIREMENTS.md       # ◌ 待用户编写：需求文档（见 README 第 6 节契约约定）
│   ├── SYSTEMS.md            # ◌ 6 大强逻辑系统详细设计（经济/状态机/事件/声望/船只/天气）
│   ├── PERSONAS.md           # ◌ 船员人格库（角色/性格 Traits/背景/口吻）
│   └── ACTIONS.md            # ◌ 动作清单（结构化输出 action 字段的注册表）
├── server/
│   ├── ARCHITECTURE.md       # ◌ 服务端架构与选型
│   ├── API.md                # ◌ REST/WS 接口定义（与 contracts/ 同步）
│   ├── DB_SCHEMA.md          # ◌ 数据库表结构
│   ├── TOKEN_SYSTEM.md       # ◌ token 双轨与配额策略（见本文第 6 节）
│   └── SECURITY.md           # ◌ 鉴权/限流/防滥用
├── web/
│   ├── PAGES.md              # ◌ 官网页面清单与文案大纲
│   └── COPY.md               # ◌ 营销文案
└── ops/
    ├── DEPLOY.md             # ◌ 部署拓扑（开发/测试/生产）
    └── MONITORING.md         # ◌ 日志与指标
```

> ★ = 已存在　◌ = 待创建。文档先行：**写代码前先写对应文档**。

---

## 3. game/ —— Unity 客户端

```
game/
├── Assets/
│   ├── Scenes/               # 主场景 / 港口场景 / 测试场景（每个场景配 .unitymeta）
│   ├── Scripts/
│   │   ├── Core/             # 事件总线、存档(SQLite)、配置(ScriptableObject)、GameManager
│   │   ├── Gameplay/         # 表现层：船、天气、航行、港口、UI 控制器
│   │   ├── Systems/          # ★ 强逻辑层：经济/船员状态机/事件因果/声望（纯 C#，可单测）
│   │   ├── Agent/            # ★ Agent 集成：LLM 客户端、记忆、对话 UI（对接 AgentCore）
│   │   └── Networking/       # 预留：服务器 API 客户端 / 多人（本阶段只留接口）
│   ├── Art/
│   │   ├── Placeholder/      # ★ 占位素材集中地（灰盒开发，后期整目录替换）
│   │   ├── Models/  Textures/  Materials/
│   ├── Audio/  Music/  SFX/  Voice/
│   ├── Prefabs/              # 船/船员/港口/岛屿 Prefab（场景只引用这里）
│   ├── UI/                   # 字体、图标、对话界面
│   ├── Settings/             # ScriptableObject 配置（数值参数全部可调）
│   └── ThirdParty/           # Crest 等第三方包导入位置
├── Packages/                 # manifest.json（Unity 生成 + 手动编辑）
├── ProjectSettings/          # Unity 项目设置（Unity 生成）
├── AgentCore/                # ★ 已就位：纯 C# Agent 核心库（独立可测，见下）
└── (Unity 生成: Library/ Temp/ 等，已被 .gitignore 忽略)
```

**AgentCore 与 Unity 的关系**：`game/AgentCore/` 是纯 C#、零依赖的核心库（已通过 dotnet 自测），
负责"强逻辑 × 重交互"的闭环逻辑；Unity 侧 `Assets/Scripts/Agent/` 负责接入（复制源码或用
asmdef 引用）。**逻辑永远不依赖 Unity API**，这是"换素材/换 UI/换引擎都不影响逻辑"的根基。

---

## 4. server/ —— ★ AI 服务端（本项目的核心壁垒）

### 4.1 为什么必须有服务器

| 理由 | 说明 |
|---|---|
| Key 安全 | LLM API Key 进客户端 = 白送（可被扒包/抓流量），必须只存服务端 |
| 计量计费 | 每个玩家消耗多少 token 必须可计量，才能做免费额度/点数售卖 |
| 限流防滥用 | 按账号 RPM/TPM + 并发限制，防刷接口 |
| 合规 | 玩家数据（对话内容）可留存审计；未成年人保护；隐私政策落地 |
| 官网/账号 | 买断玩家≠永久免费 AI，需要账号体系支撑后续商业模型 |

### 4.2 目录设计

```
server/
├── src/
│   ├── gateway/              # ★ LLM 网关：统一代理上游（Ollama 集群/vLLM/DeepSeek/OpenAI）
│   │   ├── upstreams.ts      #   上游注册表（按成本/质量路由）
│   │   ├── router.ts         #   路由：本地优先→云端兜底→熔断重试
│   │   ├── metering.ts       #   计量：本次请求消耗的 token（入+出）
│   │   └── quota.ts          #   配额校验：余额不足→拒绝/降级
│   ├── auth/                 # 账号：注册/登录/会话
│   │   ├── steam.ts          #   Steam OpenID 关联登录（主账号）
│   │   ├── email.ts          #   邮箱注册（官网用户）
│   │   ├── jwt.ts            #   会话签发/刷新
│   │   └── middleware.ts     #   请求鉴权
│   ├── billing/              # ★ token 双轨：钱包/交易/兑换
│   │   ├── wallet.ts         #   余额（玩家点数）
│   │   ├── exchange.ts       #   点数 → 上游 token 换算
│   │   ├── transactions.ts   #   流水（充值/消耗/赠送/退款）
│   │   └── webhooks.ts       #   支付回调（Steam 钱包/官网支付）
│   ├── api/                  # REST 路由 v1
│   │   ├── auth.routes.ts    #   POST /api/v1/auth/*
│   │   ├── agent.routes.ts   #   POST /api/v1/agent/chat  ★游戏内对话唯一入口
│   │   └── billing.routes.ts #   GET  /api/v1/wallet 等
│   ├── ws/                   # WebSocket：游戏内实时通道（对话流式推送/状态同步）
│   └── worker/               # 定时任务：日结算、会话摘要、用量统计、告警
├── db/
│   ├── migrations/           # 数据库迁移（版本化）
│   └── seed/                 # 种子数据（默认配额策略等）
├── config/                   # 环境配置（.env 不入库）
├── tests/                    # 单元 + 集成测试
└── Dockerfile / docker-compose.yml
```

### 4.3 请求链路（游戏内一次对话）

```
Unity 客户端 → POST /api/v1/agent/chat (JWT 鉴权)
  → auth 中间件（验会话）→ billing 校验配额
  → gateway 路由（本地 Ollama 优先 → 云端兜底）
  → 上游 LLM 返回 → metering 计量 → 扣减余额 → 写 usage_records
  → 流式返回客户端（WS 或 SSE）
```

### 4.4 Token 双轨设计（商业模型核心）

| 轨 | 单位 | 说明 |
|---|---|---|
| **上游 token** | LLM 计费单位（成本侧） | 服务端真实消耗，按模型计费（如 DeepSeek 每百万 token ¥X） |
| **玩家点数** | 售卖单位（收入侧） | 玩家充值/买断赠送的配额，1 点数 = 固定上游 token（可调） |

**策略草案**（待需求文档定稿）：
- 买断玩家：赠送基础点数（如 100 万上游 token 等价额度）；
- 超额：官网购买点数（Steam 钱包/微信/支付宝均可，走 webhooks）；
- 免费玩家（demo）：极低每日额度，仅限本地模型对话；
- 降级：余额不足时自动回退本地模型/模板对话，游戏永远可玩；
- 限流：每账号 RPM ≤ 20、TPM ≤ 100k、并发 ≤ 1（防刷）。

### 4.5 数据库表（草案）

```
users            id, email, nickname, status, created_at
steam_links      user_id, steam_id(唯一), display_name
sessions         id, user_id, token_hash, expires_at
wallets          user_id(唯一), points_balance, version(乐观锁)
transactions     id, user_id, type(充值/消耗/赠送), points, upstream_tokens, ref_id, created_at
usage_records    id, user_id, model, prompt_tokens, completion_tokens, latency_ms, created_at
quotas           user_tier, rpm_limit, tpm_limit, daily_points, monthly_points
```

### 4.6 技术选型（草案，需求文档确认）

- 服务端：**Node.js (NestJS)** 或 **Python (FastAPI)** —— 个人项目二选一，推荐 Node；
- 数据库：**PostgreSQL**（用户/流水）+ **Redis**（限流/会话缓存）；
- 部署：Docker Compose → 海外 VPS（合规前提，见 GAME_PLAN Part H）。

---

## 5. web/ —— 官网 + 用户中心

```
web/
├── landing/                  # 官网落地页：介绍/玩法/截图/定价/下载/新闻
│   ├── app/  components/  public/
│   └── package.json
├── console/                  # ★ 用户中心（登录后）：
│   ├── pages/                #   仪表盘（余额/用量图表）、购买点数、兑换码
│   └── ...
└── admin/                    # 运营后台（后置）：用户查询、点数发放、用量统计
```

- 前端：**Next.js**（个人项目主流，SEO 好 + 全栈省事）；
- 与 `server/` 共享 `contracts/openapi`，接口类型自动生成。

---

## 6. contracts/ —— 跨端契约（单一事实源）

```
contracts/
├── openapi/                  # OpenAPI 定义（server 与 web 共用，自动生成类型）
│   └── v1.yaml
├── agent-reply.schema.json   # ★ LLM 结构化输出 Schema（客户端解析与服务端校验共用）
└── README.md                 # 契约变更流程（改契约→三端同步）
```

> 已落地事实：`game/AgentCore/AgentModels.cs` 的 `AgentReply` 即 `agent-reply.schema.json`
> 的 C# 实现。**契约先行**：改 schema → 三端（游戏/服务端/官网）同步改。

---

## 7. ops/ / tools/ / assets/

```
ops/
├── docker/                   # compose、nginx、证书
├── scripts/                  # 部署、备份、迁移脚本
└── monitoring/               # 日志采集、指标面板（Grafana 等）

tools/
├── asset-pipeline/           # 素材处理（DDS/图集/音频压缩批量脚本）
├── data-gen/                 # 数据生成（港口/岛屿/事件模板）
└── dev/                      # 本地开发辅助（Ollama 拉起、seed 脚本）

assets/                       # 全局共享素材库（git-lfs 跟踪）
├── placeholder/              # 占位素材（Kenney 等 CC0）
└── licensed/                 # 已购商业素材 + 授权记录表（★合规必查）
```

---

## 8. 命名规范（全项目统一，完整版见 DEVELOPMENT_STANDARDS 第 5 节）

| 项 | 规则 |
|---|---|
| 目录 | 小写，`kebab-case`（`asset-pipeline`）；Unity Assets 内保留 PascalCase 子目录（引擎约定） |
| C# 类/文件 | `PascalCase`，一个类一个文件 |
| TypeScript/Python | `camelCase` 函数 / `PascalCase` 类；文件 `kebab-case` |
| 数据库 | 表 `snake_case` 复数；主键 `id`；时间戳 `created_at/updated_at` |
| 契约 | JSON Schema 严格版本化，改动必须三端同步 |
| 素材 | `类型_名称_变体`（`ship_brig_v1`），占位素材统一前缀 `ph_`（`ph_crew_male`） |

## 8.1 资源位置规范（素材放哪，不许乱放）

| 资源类型 | 位置 | 约定 |
|---|---|---|
| 占位素材（灰盒期） | `game/Assets/Art/Placeholder/` | 只用 CC0/CC-BY（Kenney/Poly Haven 等）；`ph_` 前缀；**替换时整目录换，不污染正式目录** |
| 3D 模型 | `game/Assets/Art/Models/` | 子目录按类别（Ships/Crew/Island/Props）；低多边形 Synty 风 |
| 贴图/材质 | `game/Assets/Art/Textures/` `Materials/` | 贴图命名带用途后缀（`_albedo/_normal/_mask`） |
| 音频 | `game/Assets/Audio/` 下 `Music/ SFX/ Voice/` | 音乐带 BPM/氛围标签（`music_harbor_tavern_80bpm`） |
| 预制体 | `game/Assets/Prefabs/` | 按类别子目录；**场景只引用 Prefab，不直接挂资源**（替换隔离） |
| UI | `game/Assets/UI/` | 字体/图标/对话界面；字体用思源黑体/阿里巴巴普惠体（免费商用） |
| 配置 | `game/Assets/Settings/` | ScriptableObject 参数全部在此，改参不改代码 |
| 第三方包 | `game/Assets/ThirdParty/` | Crest 等导入位置，勿与自有代码混放 |
| 全局共享素材 | `assets/`（仓库根） | git-lfs 跟踪；`placeholder/` 与 `licensed/`（授权记录表必留） |

## 8.2 代码位置规范（谁在哪个目录，见 DEVELOPMENT_STANDARDS 第 1 节所有权表）

- 强逻辑（纯 C# 可测）→ `game/Assets/Scripts/Systems/` + `game/AgentCore/`
- LLM 接入 → 客户端 `game/Assets/Scripts/Agent/`；服务端 `server/src/modules/gateway/`
- 表现层 → `game/Assets/Scripts/Gameplay/`（依赖 Systems/Agent，禁止被反向依赖）
- 服务端业务模块 → `server/src/modules/{auth,billing,gateway,api,ws,worker}/`

---

## 9. 现有资产归位对照（本次已执行）

| 原位置 | 新位置 | 说明 |
|---|---|---|
| `GAME_PLAN.md`（根） | `docs/game/GAME_PLAN.md` | 游戏计划 + 上架合规 |
| `SETUP.md`（根） | `docs/game/SETUP.md` | 环境/测试/素材 |
| `AgentCore/`（根） | `game/AgentCore/` | Agent 核心库归入游戏模块 |

---

## 10. 技术架构总览（v1.0 定案）

```
┌─ 游戏客户端 game/（Unity 6 + URP + Crest Ocean，C#）
│   ├─ AgentCore（纯 C# 核心库：强逻辑×重交互闭环，独立可测）
│   └─ Assets/Scripts/{Systems(强逻辑,无LLM) · Agent(LLM接入) · Gameplay(表现) · Networking(预留)}
│            │ 对话请求（JWT）
│            ▼
┌─ AI 服务端 server/（NestJS 11 + TS + Prisma/PostgreSQL + Redis）
│   ├─ auth（Steam 关联/邮箱/JWT） → billing（token 双轨：点数钱包/兑换/流水）
│   ├─ gateway（LLM 网关：本地 Ollama 优先→云端 DeepSeek 兜底→metering 计量→quota 配额）
│   └─ api/ws/worker（REST / WebSocket 流式 / 定时任务）
│            ▲ 契约 contracts/（agent-reply.schema.json + openapi/v1.yaml）
│            │
└─ 官网 web/（Next.js：landing + console 用户中心 + admin 后台）
```

依赖方向：`game/Gameplay → Agent → Systems`；`server/api → gateway/auth/billing`；两端只通过 `contracts/` 耦合。

## 11. 建议的下一步（按序，当前状态）

1. ✅ 需求文档 v1.0（过审）→ `docs/game/REQUIREMENTS.md`
2. ✅ 服务器选型定案 → `docs/server/ARCHITECTURE.md`（NestJS + TS）
3. ✅ **token 商业模型定案**（DeepSeek 双轨）→ `docs/server/TOKEN_SYSTEM.md`
4. ✅ 契约基础（agent-reply.schema.json + openapi/v1.yaml）
5. ✅ **M0 数据模型定稿**（Player/Ship/CrewAgent/TraitDef/ShipTypeDef）→ `docs/game/design/DATA_MODELS.md`
6. ⬜ Unity 工程初始化 → 按 SETUP.md 开工（M0 验证段；AgentCore 扩展 traits/culture/role 随工程进行）

---

> 一句话总结：**这个工程 = 游戏（C#/Unity）+ 服务（Node/DB）+ 官网（Next.js）+ 契约（JSON Schema）**，
> 全部从 `docs/` 开始，`contracts/` 是连接三端的桥。
