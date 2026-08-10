# AGENTS.md — 自动加载的开发约定（所有 AI agent 必读，无需跳转）

> 本文件是项目根约定文件：主流的 AI 编码工具（Claude Code / Cursor / Codex 等）在进入本项目时会自动读取。
> 完整上下文请读 `docs/CONTEXT.md`（10 分钟对齐页）；完整规范见 `docs/DEVELOPMENT_STANDARDS.md`。

## 项目一句话

1675 年加勒比海开放世界航海沙盒 RPG（纯 Agent 游戏）：AI 船员社会 × 航海贸易 × 真实主义，Steam 目标，三端一体（Unity 客户端 + NestJS 服务端 + Next.js 官网，contracts/ 契约桥接）。

## 铁律 5 条（违反 = 产出不合格，无条件退回）

1. **契约先行**：跨端数据一律走 `contracts/`（JSON Schema / OpenAPI）。改契约必须三端同步（game/server/web），禁止单端私有协议。
2. **强逻辑红线**：`game/Assets/Scripts/Systems/` 禁止出现 LLM / HttpClient / 网络代码；任何数值修改只允许发生在所属系统类内部（裁决权见 DESIGN_BOUNDARIES 第 6 节）。
3. **数据所有权唯一**：每份数据只有一个系统能写。Agent（LLM）只提议、不裁决；输出必须过 `AgentReply` Schema 校验才能进逻辑层。
4. **AgentCore 纯 C#**：`game/AgentCore/` 零 Unity 依赖、零第三方依赖，可独立 `dotnet build` 验证。
5. **产出必须留痕**：代码 + 对应文档同步更新 + 写当日记忆日志（`.workbuddy/memory/YYYY-MM-DD.md`）。

## 开工流程（每个任务必走）

1. 读 `docs/CONTEXT.md`（必读文档顺序在列，10 分钟内对齐）；
2. **单一写者原则**：本任务只动一个模块（所有权表见 DEVELOPMENT_STANDARDS 第 1 节），跨模块改动拆子任务；
3. 按规范开发（模块化/依赖单向/低耦合高内聚）；
4. 验证：编译（AgentCore `dotnet build` / server `npm run build`）+ 测试 + 规范自查；
5. 过 DoD 验收清单（DEVELOPMENT_STANDARDS 第 8 节，8 项全勾才算完成）；
6. 留痕：更新文档 + 写记忆日志 + 交接说明（改了什么/新接口/待办）。

## 文档索引（按需读，别全读）

| 文档 | 用途 |
|---|---|
| `docs/ARCHITECTURE.md` | 技术架构、文件层级、命名与资源位置 |
| `docs/game/DESIGN_PILLARS.md` | 设计宪法（方向，6 支柱） |
| `docs/game/design/DESIGN_BOUNDARIES.md` | 边界总纲（玩家/船员/AI/NPC/动物/系统间权限） |
| `docs/DEVELOPMENT_STANDARDS.md` | ★ 本规范的完整版（所有权/拆分/耦合/DoD） |
| `docs/game/REQUIREMENTS.md` | 统一需求 v3.0（完整玩法、内容目标、完成定义） |
| `docs/game/IDEA_BANK.md` | 创意入口（新想法先登记，不直接进代码） |
| `docs/game/design/gameplay/README.md` | 玩法总目录（玩家循环和跨系统后果） |
| `docs/game/design/DATA_MODELS.md` | 统一数据模型（实体、事件与存档） |
| `docs/game/DEV_ROADMAP.md` | D7 后唯一开发路线 |
| `docs/game/AGENT_DEVELOPMENT_TASKS.md` | Agent 开发任务总索引；实际提示词在 D8—D16 独立章节文件中 |
| `contracts/` | 跨端契约（单一事实源） |

## 快速否决（看到即停）

- 在 Systems/ 里写网络/LLM 代码 → 停
- 绕过 contracts/ 直接对接两端 → 停
- 复制粘贴修改而非理解后修改 → 停
- 不写测试/不留痕的"完成" → 停
