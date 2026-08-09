# 项目上下文（CONTEXT）— 每个开发任务开工前必读

> 给所有开发者（人类 + AI agent）的 5 分钟对齐页。**先读这个，再动手。**

## 项目是什么

1675 年加勒比海的开放世界航海沙盒 RPG（纯 Agent 游戏）：
**AI 船员社会模拟 × 航海贸易 × 真实主义**。Steam 目标、个人开发、三端一体。

## 必读文档（按序，缺一不可）

1. `docs/ARCHITECTURE.md` — 三端架构与目录层级（先懂全局）
2. `docs/game/DESIGN_PILLARS.md` — 设计宪法（6 根支柱，方向）
3. `docs/game/design/DESIGN_BOUNDARIES.md` — 边界总纲（谁有权做什么）
4. `docs/DEVELOPMENT_STANDARDS.md` — ★ 本规范（工程约束，怎么写）
5. 所属模块文档（game / server / web / design 下对应文件）

## 项目速览

| 端 | 技术 | 位置 |
|---|---|---|
| 游戏客户端 | Unity 6 + C#（AgentCore 纯 C# 核心库） | `game/` |
| AI 服务端 | NestJS + TypeScript（账号/token/LLM 网关） | `server/` |
| 官网 | Next.js（待建） | `web/` |
| 契约 | JSON Schema + OpenAPI | `contracts/` |
| 文档 | 设计宪法/边界/规范/创意池 | `docs/` |

## 铁律 5 条（违反即不合格）

1. **契约先行**：改 `contracts/` 必须三端同步，禁止单端私有协议；
2. **强逻辑红线**：`Systems/` 禁止 LLM/网络代码；数值修改只在所属系统类内；
3. **数据所有权唯一**：每份数据只有一个系统能写（裁决权表）；
4. **AgentCore 纯 C#**：零 Unity 依赖、零第三方依赖，可独立编译测试；
5. **产出必须留痕**：代码 + 文档同步 + 写当日记忆日志（.workbuddy/memory/）。

## 当前进度指针

- ✅ 需求 v1.0 过审｜设计边界总纲 v1.0｜服务器骨架可运行｜契约基础落地
- ✅ D1 工程底座与 InitialHarbor 已通过用户验收
- ✅ D2 航行与对话闭环已完成：Unity 6 编译通过，EditMode 23/23 通过，AgentCore/server 检查通过
- ✅ D3 贸易与完整航程可玩环已完成：10 商品、3 地点、5 天气、8 事件；Unity 40/40 测试与运行时冒烟通过，D2 接口保持稳定

## 开工自检

- [ ] 我读了 4 份必读文档？
- [ ] 我知道我的任务只动哪个模块（单一写者）？
- [ ] 我知道产出要过哪个验收清单（DoD）？
