# D20-L08 逻辑交接说明

- 任务：`D20-L08`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L08/D20_L08_LOGIC_RESULT.json`

## 已完成

新增 `game/Assets/Scripts/Systems/D20/D20ActionReadModel.cs`：

- 将意图、优先级判定、L06 结果、L06 快照和可选 L07 表现事件汇总为只读动作读模型；
- 明确 `Uninitialized`、`NoAction`、`Proposed`、`Active`、`Paused`、`Completed`、`Cancelled`、`Unavailable`、`Fallback`、`Invalid` 状态；
- 暴露动作、目标、逻辑位置、owner、优先级、互斥组、presence、可用性、回退、事件 ID 和修订字段；
- 所有嵌套意图、结果、事件和集合通过克隆/只读视图隔离来源；
- 只作投影，不拥有或修改动作、位置、业务结算或表现事实。

## 公开 API

- `D20ActionReadModel.FromIntent(...)`、`FromInterruptResult(...)`、`FromSnapshot(...)`；
- `D20ActionReadModelBuilder.Build(...)` 三个重载；
- `D20ActionReadModel.Validate()`、`Clone()`；
- `D20ActionReadModelState`、`D20ActionIntentReadModel`、`D20ActionResultReadModel`。

## 验证与未完成项

- 精确 C# harness 编译：通过，0 warnings / 0 errors；
- 确定性 smoke：通过，覆盖意图、结果、快照入口和 active/proposed 状态；
- 五个目标生产文件禁用引用扫描：通过；`git diff --check`：通过；机器结果 JSON：通过解析；
- 未接入任何 Runtime UI、输入或场景。

## Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L08 LOGIC_PASS`；新增统一动作只读读模型与三入口 builder，汇总 L04/L05/L06/L07 的稳定逻辑字段并防御性复制；C# 编译、smoke、禁用引用扫描、JSON 解析和 diff check 通过；Runtime 消费延后。
