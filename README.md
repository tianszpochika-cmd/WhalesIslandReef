# WhalesIslandReef（鲸骨航线）— 纯 Agent 3D 航海游戏

> 1675 年加勒比海 · 开放世界航海沙盒 RPG ｜ 大陆个人开发者 · Steam 目标 ｜ 强逻辑 × 重交互

## 这是什么

你是一艘小渔船的主人，在英西法葡列强夹缝的加勒比海上经营船与船员、探索岛屿、贸易、寻宝、生存。
**核心卖点：每一名 AI 船员都记得你做过什么，并会为此背叛你或追随你。**

工程为三端一体：**Unity 游戏客户端 + AI 服务端（账号/token 配额/LLM 网关）+ 官网**，`contracts/` 契约桥接。

## 开发约束（自动生效，开工必守）

> 根目录 `AGENTS.md` 会被 AI 编码工具自动加载（Claude Code/Cursor/Codex 等），无需手动调用。
> 铁律 5 条：**契约先行 / Systems 禁 LLM / 数据所有权唯一 / AgentCore 纯 C# / 产出留痕**。
> 完整规范：`docs/DEVELOPMENT_STANDARDS.md` ｜ 对齐页：`docs/CONTEXT.md`

## 快速入口

| 想看什么 | 去哪 |
|---|---|
| 技术架构 / 文件层级 / 命名与资源位置 | `docs/ARCHITECTURE.md` |
| 设计宪法（6 支柱 + 1675 时代） | `docs/game/DESIGN_PILLARS.md` |
| 边界总纲（玩家/船员/AI/NPC/动物/系统间） | `docs/game/design/DESIGN_BOUNDARIES.md` |
| 统一需求 v3.0（完整玩法、内容目标、验收） | `docs/game/REQUIREMENTS.md` |
| 创意池（新想法登记） | `docs/game/IDEA_BANK.md` |
| D7 后统一开发路线 | `docs/game/DEV_ROADMAP.md` |
| Agent 开发任务总索引 | `docs/game/AGENT_DEVELOPMENT_TASKS.md`；实际任务提示词位于同目录 D8—D16 章节文件 |
| 玩法总目录 | `docs/game/design/gameplay/README.md` |
| 环境搭建 / 本地测试 / 素材 | `docs/game/SETUP.md` |
| Agent 核心库（纯 C#，可跑） | `game/AgentCore/`（`dotnet run -- --mock`） |
| AI 服务端（可运行骨架） | `server/`（`npm run dev`） |
| 跨端契约（单一事实源） | `contracts/` |

## 顶层结构

```
docs/        文档（目标/边界/工程/契约/验收 五层）
contracts/   跨端契约（JSON Schema + OpenAPI）
game/        Unity 客户端（Assets 分层 + AgentCore 纯 C# 核心库）
server/      AI 服务端：账号注册 / token 配额计量 / LLM 网关（NestJS）
web/         官网 + 用户中心 + 运营后台（Next.js，待建）
ops/         部署 / 监控（海外机房）
tools/       内部工具（素材管线/数据生成）
assets/      共享素材（git-lfs，占位 + 已购授权）
```

## 技术栈

| 端 | 技术 | 状态 |
|---|---|---|
| 游戏 | Unity 6 + URP + Crest Ocean（C#） | ✅ D7 基线已完成，D8 扩展中 |
| Agent 核心 | 纯 C#（Ollama 本地优先 + DeepSeek 云端兜底） | ✅ 骨架可用 |
| 服务端 | NestJS 11 + TS + Prisma/PostgreSQL + Redis | ✅ 骨架可用 |
| 官网 | Next.js | ⬜ 待建 |
| 契约 | JSON Schema + OpenAPI | ✅ 基础落地 |

## 当前进度

- ✅ 上架合规分析（个人可上架、无版号、W-8BEN）｜ 需求 v1.0 过审｜设计边界 v1.0｜AI 开发规范｜服务器骨架可运行｜AgentCore 自测通过
- ✅ D7 技术基线已完成；D7 后进入统一需求、设计和路线重构
- 📌 关键决策：1675 年 / 沙盒 / 无奇幻 / 强逻辑 / AI 船员社会 / 完整玩法按 D8—D16 推进

## 开发原则

- **强逻辑**：确定性系统裁决一切，AI 只提议、不直接改状态
- **重交互**：自由对话 / 决策会议 / 语言喊话指挥（P2A）/ AI 叙事 / 篝火夜话
- **契约先行**：改 `contracts/` → 三端同步
- **灰盒开发**：占位素材推逻辑，后期统一替换（`Art/Placeholder/`）
- **分级推进**：D7→D16（分级=执行顺序，设计内容不因阶段被删除）
