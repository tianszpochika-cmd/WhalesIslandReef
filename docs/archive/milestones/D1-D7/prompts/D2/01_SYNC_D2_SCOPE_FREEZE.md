# D2 同步任务 01：范围、接口与所有权冻结

你是 D2 的技术负责人。请在 `C:\Users\Administrator\Desktop\game\first01` 工作，只做 D2 的范围和接口冻结，不提前实现具体功能。

## 必读

`AGENTS.md`、`docs/CONTEXT.md`、`docs/ARCHITECTURE.md`、`docs/DEVELOPMENT_STANDARDS.md`、`docs/game/REQUIREMENTS.md`、`docs/game/design/DESIGN_BOUNDARIES.md`、`docs/game/design/DATA_MODELS.md`、`docs/game/DEV_ROADMAP.md`、`docs/game/prompts/D2/README.md`。

## 任务

1. 盘点现有 D1 的场景、存档、相机、暂停菜单、AgentCore 和 `AgentReply` Schema。
2. 创建 `docs/game/design/D2_SCOPE.md`，冻结 D2 M0 的场景流、输入、状态、数据所有权、异常兜底、接口和验收条件。
3. 明确以下最小接口：航行状态/输入、人物移动或控制权切换、第三人称相机、昼夜时钟、对话服务、好感变化、存档快照。
4. 判定哪些数据必须进入 `contracts/`。若确实需要修改契约，必须在同一任务中给出 game/server/web 的同步影响清单；不要只改单端 Schema。
5. 更新 `docs/game/DEV_ROADMAP.md` 的 D2 状态和 D2 入口链接。

## 硬约束

- 不实现船的物理、UI、网络或 LLM。
- 不引入贸易、炮战、多个岛屿、完整天气和 M1 内容。
- `game/Assets/Scripts/Systems/` 不得出现 LLM、HttpClient、网络代码。
- Agent 只提出对话回复；数值结算、好感落库和状态变化必须由明确的逻辑系统裁决。

## 验收

- 任何后续 Agent 只看 `D2_SCOPE.md` 就能知道自己能改什么、不能改什么。
- 接口能支持本地 Mock 先跑通，Ollama/服务端失败时有模板兜底。
- 文档明确列出 D2 DoD 和不属于 D2 的需求。
- `git diff --check` 通过；只提交范围/文档变更和必要的契约说明。

## 回报格式

请返回：范围结论、接口表、数据所有权表、依赖变更、修改文件、验证结果、给 03-10 任务的注意事项。不要自行开始后续任务。
