# D20-L11 逻辑交接说明

- 任务：`D20-L11`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L11/D20_L11_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20TraitActionAdapter.cs`：

- `D20TraitActionAdapter.Evaluate(...)` 只接收动作意图枚举和已有 D5 trait ID；
- 复用 `D5TraitSystem.IsValidTraitSet(...)`，再通过 `D5TraitCatalog.Find(...)` 与 `D5TraitSystem.Modifier(...)` 读取 owner 定义；
- 对 `move_speed`、`boarding`、`sailing`、`night_watch` 等直接动作关联修正提供资格/限制投影，其余已知动作关联修正提供反馈匹配；
- `null` 特质输入使用安全中性反馈，非法集合返回稳定的 `InvalidTraitSet`；
- 结果通过 `D20TraitActionEvaluation` 返回匹配特质 ID、资格、决策、失败码、原因码和反馈键，并提供防御性 `Clone()`；
- 未写入 D5 目录、未改变 trait 数值、未调用任何动作 owner 或运行时表现接口。

## 2. 后续可消费公开 API

- `D20TraitActionAdapter.Evaluate(D20ActionIntentKind, IEnumerable<string>)`；
- `D20TraitActionEvaluation`；
- `D20TraitActionDecision`、`D20TraitActionFailureCode`。

调用方应把该结果交给 D20 后续动作前置聚合，再由对应 owner 处理真实业务结算；不要把 `MatchedTraitIds` 当作第二份特质状态。

## 3. 验证与未完成项

- D20-L11—L15 合并纯 C# 编译：Roslyn `/warnaserror+`，exit 0、0 警告；
- smoke 覆盖正向资格、负向硬限制和非法 trait 集合拒绝；
- 目标源文件静态边界检查：无 Unity/network/LLM/time/random/owner mutation 引用，尾随空白为 0；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实 D5 运行时输入、动作路由和表现反馈延期。

## 4. Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L11 LOGIC_PASS`；记录 D5 特质只读投影、稳定资格/反馈码、非法集合拒绝和未启动 Runtime。后续聚合层不得复制 D5 校验或修改特质事实。
