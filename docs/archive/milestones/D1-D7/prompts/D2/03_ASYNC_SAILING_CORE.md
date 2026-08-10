# D2 异步任务 03：航行核心与可测试状态机

你负责 D2 的航行强逻辑。前置条件：同步任务 01 已完成，并且你已阅读 `docs/game/design/D2_SCOPE.md`。

## 负责范围

- `game/Assets/Scripts/Systems/Sailing/` 及其测试。
- 如 D2_SCOPE 明确要求，才新增对应的纯 C# 类型到 `game/AgentCore/`；不要改 UI、场景或服务端。

## 实现目标

实现一个可测试的最小航行状态机：前进、减速/停止、转向、速度上限、基础耐久/补给消耗接口和状态快照。数值必须由航行系统单一写入，Unity 输入只传入意图，不直接改船的数值。人物步行如果不属于本强逻辑模块，只定义控制权/状态接口，不实现 Unity 表现。

## 硬约束

- `Systems/` 内禁止 LLM、HttpClient、网络请求和 Unity UI。
- 不把 Crest API、Transform、InputAction 或 MonoBehaviour 放进强逻辑核心。
- 不修改 `contracts/`、MainMenu、对话 UI 和 SaveSystem 的公共行为；需要接口时在报告中提出。
- 速度、转向、耐久等数值只能由本系统裁决，Agent 不得参与。

## 测试与交付

- 覆盖零输入、前进、反向/停止、转向、速度上限、边界值和快照恢复。
- 按项目现有测试方式验证；至少运行相关 C# 测试，并执行 `dotnet build game/AgentCore/AgentCore.csproj --nologo`。
- 更新当日 `.workbuddy/memory/YYYY-MM-DD.md`。

返回修改文件、公开接口、测试结果、未解决依赖和合并说明。不要修改主分支。
