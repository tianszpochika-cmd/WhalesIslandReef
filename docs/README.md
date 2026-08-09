# 项目文档索引

本目录保存项目的设计、开发规范、阶段路线图、验收记录与 Agent 任务提示词。

## 当前阶段

- D1：工程底座与初始港口，已由用户完成验收。
- D2：航行与对话闭环，D2-01 至 D2-10 已完成，自动化验收 23/23 通过。
- D3：贸易与完整航程可玩环，已完成；范围与验收见 [D3 范围冻结](game/design/D3_SCOPE.md) 和 [D3 最终验收报告](game/design/D3_ACCEPTANCE_REPORT.md)。
- D4：内容扩充，已完成；范围与验收见 [D4 范围冻结](game/design/D4_SCOPE.md) 和 [D4 最终验收报告](game/design/D4_ACCEPTANCE_REPORT.md)。

## D2 入口

- [D2 任务总览与依赖图](game/prompts/D2/README.md)
- [D2 同步任务 01：范围与接口冻结](game/prompts/D2/01_SYNC_D2_SCOPE_FREEZE.md)
- [D2 同步任务 02：契约与测试门](game/prompts/D2/02_SYNC_CONTRACT_TEST_GATE.md)
- [D2 异步任务 03：航行核心](game/prompts/D2/03_ASYNC_SAILING_CORE.md)
- [D2 异步任务 04：海洋与昼夜](game/prompts/D2/04_ASYNC_OCEAN_DAY_NIGHT.md)
- [D2 异步任务 05：AgentCore 对话](game/prompts/D2/05_ASYNC_AGENTCORE_DIALOGUE.md)
- [D2 异步任务 06：对话 UI](game/prompts/D2/06_ASYNC_DIALOGUE_UI.md)
- [D2 异步任务 07：服务端网关](game/prompts/D2/07_ASYNC_SERVER_GATEWAY.md)
- [D2 同步任务 08：运行时集成](game/prompts/D2/08_SYNC_RUNTIME_INTEGRATION.md)
- [D2 同步任务 09：好感与存档闭环](game/prompts/D2/09_SYNC_AFFINITY_SAVE_E2E.md)
- [D2 同步任务 10：最终验收](game/prompts/D2/10_SYNC_D2_ACCEPTANCE.md)
- [D2 最终验收报告](game/design/D2_ACCEPTANCE_REPORT.md)

## D3 入口

- [D3 任务总览与依赖图](game/prompts/D3/README.md)
- [D3 范围冻结与验收门](game/design/D3_SCOPE.md)
- [D3 贸易/航程契约](../contracts/d3-trade.schema.json)
- [D3 最终验收报告](game/design/D3_ACCEPTANCE_REPORT.md)

## D4 入口

- [D4 任务总览与依赖图](game/prompts/D4/README.md)
- [D4 范围冻结与验收门](game/design/D4_SCOPE.md)
- [D4 内容契约](../contracts/d4-content.schema.json)
- [D4 最终验收报告](game/design/D4_ACCEPTANCE_REPORT.md)
