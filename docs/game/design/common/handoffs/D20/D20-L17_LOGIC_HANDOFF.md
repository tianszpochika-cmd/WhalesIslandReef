# D20-L17 逻辑交接说明

- 任务：`D20-L17`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L17/D20_L17_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcBehaviorStateMachine.cs`：

- `D20NpcBehaviorState` 覆盖 `Idle`、`Work`、`Trade`、`Talk`、`Rest`、`Alarm`、`Refuse`、`Injured`；
- `D20NpcBehaviorRequest` 绑定稳定命令 ID、NPC ID 和完整日程输入副本，校验失败不会改变状态；
- `D20NpcBehaviorStateMachine.Tick/Apply/Evaluate` 按固定优先级处理终止、受伤、危险、恶劣天气、休息、工作活动和空闲；
- `RequestWork/RequestTrade/RequestTalk/RequestRest/RaiseAlarm/ReportInjury/Recover/Refuse/Cancel` 提供明确请求入口，Trade/Talk 只检查 L15 岗位能力，不执行交易或关系结算；
- `CanTransition` 拒绝 Unknown→Work 等非法迁移，并允许由新的 owner 输入从 Alarm/Refuse/Injured 回到正常日程；
- `D20NpcBehaviorSnapshot` 保存当前/前一状态、恢复状态、原因码、逻辑日程位置、恢复策略和存档修订，未复制外部岗位、健康、经济或任务事实。

## 2. 后续可消费公开 API

- `D20NpcBehaviorStateMachine`：`Evaluate`、`Apply`、`Tick`、`CaptureSnapshot`、各请求便捷方法、`CanTransition`；
- `D20NpcBehaviorState`、`D20NpcBehaviorRequestKind`、`D20NpcBehaviorRecoveryPolicy`；
- `D20NpcBehaviorRequest`、`D20NpcBehaviorSnapshot`、`D20NpcBehaviorTransitionResult`；
- `D20NpcBehaviorFailureCode` 与 `D20NpcBehaviorRequestValidationResult`。

`Succeeded/Accepted` 仅表示 D20 行为投影发生了合法转换；不表示交易、对话、库存、关系、健康、港口或任务 owner 已成功。

## 3. 验证与未完成项

- 依赖链纯 C# 编译：现代 Roslyn `/warnaserror+`，`CSC_EXIT=0`、无警告；
- 确定性 smoke 覆盖日程 Trade、手动 Talk、危险 Alarm、受伤 Injured、owner 清除后的 Recover、休息优先和非法状态迁移，共 25 项汇合检查通过；
- 禁用引用/尾随空白扫描均为 0；`git diff --check` exit 0（Git 仅提示既有换行转换）；
- 未启动 Player、PlayMode、场景、输入、动画或设备；寻路、动画、真实时钟和正式 owner 集成延期。

## Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L17 LOGIC_PASS`；记录 NPC 八态确定性状态机、非法迁移拒绝、受伤/危险/拒绝恢复策略和未启动 Runtime。
