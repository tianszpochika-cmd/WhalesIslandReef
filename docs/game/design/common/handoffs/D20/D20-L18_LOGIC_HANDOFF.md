# D20-L18 逻辑交接说明

- 任务：`D20-L18`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L18/D20_L18_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcInteractionAvailability.cs`：

- `D20NpcInteractionKind` 覆盖 Talk、Trade、Service、Inspect、Rumor，并由 L15 目录能力映射；
- `D20NpcInteractionAvailabilityRequest` 只携带稳定 ID、行为/可用性快照、可达布尔值、冷却 tick 和语义地点引用，不携带坐标或引擎对象；
- `D20NpcInteractionAvailability.Evaluate` 以终止/受伤/警戒、不可达、冷却、拒绝、休息、忙碌、不可用、岗位未知/能力缺失的固定顺序返回结果；
- `D20NpcInteractionAvailabilityResult.IsReadOnly` 始终为 true，`Available` 只表示可以向业务 owner 提交意图，不代表交易、关系、库存或对话事实成功；
- 每个阻断结果带稳定 `FailureCode`、`ReasonCode`、`LocalizationKey`、`RetryPolicy`，`CanInteract` 仅提供布尔便捷查询。

## 2. 后续可消费公开 API

- `D20NpcInteractionAvailability.Validate(...)`、`Evaluate(...)`、`CanInteract(...)`；
- `D20NpcInteractionAvailability.RequiredCapabilityFor(...)` 与 `IsSupportedInteraction(...)`；
- `D20NpcInteractionAvailabilityRequest`、`D20NpcInteractionAvailabilityResult`；
- `D20NpcInteractionFailureCode`、`D20NpcInteractionRetryPolicy` 和请求校验结果类型。

岗位未知不被猜测为可交互；资源或表现缺失也不能绕过这些逻辑原因。

## 3. 验证与未完成项

- 依赖链纯 C# 编译：现代 Roslyn `/warnaserror+`，`CSC_EXIT=0`、无警告；
- 确定性 smoke 覆盖 Available 与七类正负资格/能力分支，共 25 项汇合检查通过；
- 禁用引用/尾随空白扫描均为 0；`git diff --check` exit 0（Git 仅提示既有换行转换）；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实导航/可达性、UI 和业务 owner 结果延期。

## Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L18 LOGIC_PASS`；记录 NPC 交互资格的稳定忙碌/不可达/拒绝/冷却/受伤原因、只读结果和未启动 Runtime。
