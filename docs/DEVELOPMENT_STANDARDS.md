# AI 开发规范（Development Standards）v1.0

> 定位：**工程层约束**，约束所有开发者（人类 + AI agent）的产出一致性。
> 规范分层：目标层（DESIGN_PILLARS）→ 边界层（DESIGN_BOUNDARIES）→ **本规范（工程层）** → 契约层（contracts/）→ 验收层（第 9 节 DoD）。
> 任何产出不符合本规范 → 不合格，退回重做。

---

## 1. 仓库结构与文件所有权（谁可以写什么）

| 区域 | 内容 | 写者约束 |
|---|---|---|
| `docs/` | 全部文档 | 任何人可写；**新增文档必须登记进 `docs/README.md` 地图** |
| `contracts/` | 跨端契约 | 改契约 = 触发三端任务（game/server/web 必须同步），禁止单端私自改 |
| `game/AgentCore/` | 纯 C# 核心库 | 独立可测；**禁止引用 Unity API、禁止引用网络以外的第三方库** |
| `game/Assets/Scripts/Systems/` | 强逻辑层 | **禁止出现 LLM/HttpClient/网络代码**（编译层红线，见第 4 节） |
| `game/Assets/Scripts/Agent/` | LLM 接入层 | 唯一的客户端 LLM 入口；只依赖 Systems 的公开接口 |
| `game/Assets/Scripts/Gameplay/` | 表现层 | 依赖 Systems/Agent，禁止被它们反向依赖 |
| `game/Assets/Art|Audio|Prefabs|UI` | 资源 | 按素材规范；占位统一 `ph_` 前缀 |
| `server/src/modules/*` | 服务端模块 | 一个任务/agent 同时只写**一个模块** |
| `web/*` | 官网 | 遵循 contracts/openapi 生成类型 |
| `ops/ tools/ assets/` | 运维/工具/素材 | 按各目录 README |

**单一写者原则**：一个任务同时只写一个模块；跨模块改动必须拆分成独立子任务（防止并发冲突）。

## 2. 模块化与拆分约束

### 2.1 模块定义
模块 = 目录 + 明确公开 API 面（统一出口：`index.ts` / 命名空间 / 接口类）。
模块内部实现细节**不允许**被模块外引用（跨模块只能走公开面）。

### 2.2 拆分原则
1. **单一职责**：一个模块只做一件事（auth 不做计费，gateway 不做账号）；
2. **依赖单向**：依赖方向从上到下，禁止反向：
   - game：`Gameplay → Agent → Systems`（Systems 最底层，不依赖任何上层）
   - server：`api → gateway/auth/billing`，`worker → billing`；禁止 gateway 依赖 api
3. **禁止循环依赖**（编译/构建阶段即检查）；
4. **分层铁律**：强逻辑层（纯 C#/纯函数）永远独立于表现层与网络层。

### 2.3 复杂项目拆分标准流程（每个大功能必走）
```
识别职责（这个功能做几件事？）→ 按 2.2 定模块边界 → 定模块间接口（契约先行）
→ 定数据所有权（每份数据只有一个模块能写）→ 再实现
```
禁止"先写一大坨再拆"。

### 2.4 反过度设计
能一个文件解决的**不拆目录**；能一个模块解决的**不拆服务**；只有出现第二处使用场景才抽象。

## 3. 低耦合高内聚约束

### 3.1 高内聚（模块内部）
- 数据与行为绑定：`CrewStats` 的字段与修改它的规则放在一起；
- 类单一职责：一个类一个理由被修改；
- 内部私有化：不暴露不必要 public。

### 3.2 低耦合（模块之间）
- **只依赖接口/抽象**，具体实现通过构造注入（DI），禁止 `new` 对方核心类；
- 跨模块通信走**事件总线**（game: Core 事件总线；server: NestJS 事件/模块导出），禁止直接改对方状态；
- **禁止全局可变状态**（Unity 滥用 static、server 全局变量）——状态必须可存档、可回溯；
- 数据所有权唯一（见 DESIGN_BOUNDARIES 第 6 节裁决权表）。

### 3.3 契约耦合
跨端数据（游戏↔服务端↔官网）**一律走 `contracts/`** 定义（JSON Schema / OpenAPI），
禁止两端各自定义私有协议后手工对齐（必然漂移）。

## 4. 强逻辑与 Agent 边界（编码红线）

| 红线 | 说明 |
|---|---|
| LLM 代码位置唯一 | 客户端只在 `game/Assets/Scripts/Agent/`；服务端只在 `server/src/modules/gateway/`（+auth 无关） |
| Systems 无 LLM | `Systems/` 内禁止出现 LLM/HttpClient/网络调用——编译层约束：Systems 不引用网络库 |
| 数据修改唯一 | 任何数值修改只允许发生在所属系统类内部（裁决权表） |
| P2A 动作单一事实源 | 命令动作以 `docs/game/ACTIONS.md` 为准，新增动作先登记再实现 |
| 输出契约 | LLM 输出必须过 `AgentReply` Schema 校验才能进逻辑层（兜底链：校验→重试→模板） |

## 5. 命名与风格（.editorconfig 已统一缩进/换行）

| 语言 | 目录 | 文件/类型 | 函数/变量 | 常量 |
|---|---|---|---|---|
| C# | PascalCase | PascalCase | camelCase | PascalCase |
| TypeScript | kebab-case | kebab-case 文件 / PascalCase 类型 | camelCase | UPPER_SNAKE |
| Python | snake_case | snake_case | snake_case | UPPER_SNAKE |
| 数据库 | snake_case 复数 | — | — | — |
| 素材 | 类型_名称_变体；占位前缀 `ph_` | — | — | — |

- 注释：中文；写**为什么**（决策依据），不写是什么；公共 API 必须有 doc 注释；
- 提交信息：`<模块>: <一句话做了什么> (<关联文档/任务>)`，如 `gateway: 接入 Ollama 上游路由 (ARCHITECTURE#4)`。

## 6. 跨端同步规则

1. 改 `contracts/agent-reply.schema.json` → 必须同步：`game/AgentCore/AgentModels.cs`（C# 实现）+ `server` 校验逻辑 + 文档；
2. 改 `contracts/openapi/v1.yaml` → server 路由 + web 生成类型 + 客户端 API 封装 三端联动；
3. 任何契约变更：先更新契约，再改实现（**契约先行**）。

## 7. AI 开发工作流（每个 agent 开工标准流程）

```
① 必读上下文：docs/CONTEXT.md（5 分钟对齐）+ 所属模块文档
② 任务拆分：一个任务只动一个模块；跨模块拆子任务
③ 开发：按模块规范；强逻辑写成可脱离引擎/可测试的纯逻辑
④ 验证：编译 + 测试（AgentCore 自测 / server tsc / 单测）+ 规范自查（第 5/8 节）
⑤ 留痕：更新对应文档（设计文档 / 契约 / IDEA_BANK）+ 写 .workbuddy/memory/ 当日日志
⑥ 交接：产出说明（改了什么 / 新接口 / 待办 / 依赖）
```

## 8. 验收清单（Definition of Done —— 不满足即未完成）

- [ ] 编译通过（AgentCore: dotnet build；server: tsc；Unity 工程无报错）
- [ ] 测试通过（自测/单元测试新增或未破坏）
- [ ] 边界合规（第 4 节红线无违反；Systems 无 LLM）
- [ ] 契约同步（contracts/ 无漂移，AgentCore ↔ schema 一一对应）
- [ ] 文档更新（对应设计文档 + 当日记忆日志）
- [ ] 命名/风格合规（第 5 节 + .editorconfig）
- [ ] 无重复代码 / 无死代码 / 无调试残留

## 9. 冲突与错误处理

| 情况 | 处理 |
|---|---|
| 编译失败 | 回读模块规范与契约，修复根因；禁止注释掉代码"绕过" |
| 契约冲突 | 以 `contracts/` 为准；先改契约、三端同步，再改实现 |
| 边界冲突 | 按 `DESIGN_BOUNDARIES` 第 7 节仲裁（强逻辑>LLM、服务端>客户端、数值>文案） |
| 规范冲突 | 回读本文件；仍不清 → 记入文档"待定"，不在代码里临时决策 |
| 多 agent 冲突 | 单一写者原则；同一文件出现双写 → 后写者先读再合并，禁止覆盖他人改动 |
