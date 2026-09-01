# D20-L12 逻辑交接说明

- 任务：`D20-L12`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L12/D20_L12_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20CharacterConditionAdapter.cs`：

- `Evaluate(CrewStats, D10CrewBehaviorState, D20ActionIntentKind)` 读取 D10/CrewSystem 的公开状态；
- 提供纯值 overload，便于契约 smoke 使用公开快照值，不创建 owner 状态副本；
- 复用 `D10CrewBehaviorConfiguration` 的健康/疲劳阈值，不在 D20 重建数值规则；
- 输出 `Ready`、`Fatigued`、`Refusing`、`Injured`、`Unavailable`、`Dead`、`Departed` 等条件等级；
- 对离队、死亡、不可用、受伤、拒绝和疲劳动作返回稳定失败码与反馈键；对安全的对话/交互保留明确的 safe-action 结果；
- 不钳制、不修改 `CrewStats` 或 `D10CrewBehaviorState`，不调用行为 owner 的写入口。

## 2. 后续可消费公开 API

- `D20CharacterConditionAdapter.Evaluate(CrewStats, D10CrewBehaviorState, D20ActionIntentKind)`；
- `D20CharacterConditionAdapter.Evaluate(float, float, CrewBehaviorStatus, D20ActionIntentKind)`；
- `D20CharacterConditionEvaluation`；
- `D20CharacterConditionLevel`、`D20CharacterConditionFailureCode`。

阈值和行为状态的事实仍由 D10/CrewSystem 所有；聚合层只消费本结果，不复制阈值或写入状态。

## 3. 验证与未完成项

- AgentCore 独立 build：exit 0、0 警告、0 错误；
- D20-L11—L15 合并纯 C# 编译：Roslyn `/warnaserror+`，exit 0、0 警告；
- smoke 覆盖正常、拒绝和非法数值边界；
- 目标源文件静态边界检查：无 Unity/network/LLM/time/random/owner mutation 引用，尾随空白为 0；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实状态推进、动作路由和 UI 反馈延期。

## 4. Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L12 LOGIC_PASS`；记录 D10 公开健康/疲劳/行为状态只读适配、稳定限制码、非法值拒绝和未启动 Runtime。
