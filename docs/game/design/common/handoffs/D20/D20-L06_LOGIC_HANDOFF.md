# D20-L06 逻辑交接说明

- 任务：`D20-L06`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L06/D20_L06_LOGIC_RESULT.json`

## 已完成

新增 `game/Assets/Scripts/Systems/D20/D20ActionInterruptSystem.cs`：

- 以 D20 自有投影作为唯一写入范围，管理活动意图、暂停/恢复、待恢复意图和安全回退；不写健康、身份、presence、位置或其它业务事实；
- 消费 L04 `D20ActionIntent` 与 L05 `D20ActionPriorityRules`，并仅接受 L03 已裁决的 `D20PlayerPresenceResult`；
- 支持暂停、受击、失能、presence 变化、可用性变化、命令取消、显式恢复和 owner 完成；
- 为 idle、paused、unavailable、presence fallback 提供稳定 ID；
- 以命令 ID + 确定性输入指纹保留成功/拒绝回执，支持重复命令返回历史结果和冲突拒绝；快照可验证、可恢复并防御性复制。

## 公开 API

- `D20ActionInterruptSystem.RegisterActiveAction(...)`、`Apply(...)`、`Complete(...)`、`CaptureSnapshot()`、`Restore(...)`；
- `D20ActionInterruptRequest.Pause(...)`、`Hit(...)`、`Presence(...)`、`AvailabilityChanged(...)`、`Cancel(...)`、`Resume(...)`；
- `D20ActionInterruptResult`、`D20ActionInterruptSnapshot`、`D20ActionInterruptReceipt`；
- `D20ActionInterruptReason`、`D20ActionFallbackKind` 和 `D20ActionInterruptFailureCode`。

## 验证与未完成项

- 精确 C# harness 编译：通过，0 warnings / 0 errors；
- 确定性 smoke：通过，覆盖正常注册、暂停/恢复、重复命令幂等、受击清理和 idle 回退；
- 五个目标生产文件禁用引用扫描：通过；`git diff --check`：通过；机器结果 JSON：通过解析；
- 未启动 Unity Runtime，未实现真实 owner 路由、动画、输入或动作结算。

## Coordinator memory entry

请协调器在本批次汇合后统一写入当天日志：`D20-L06 LOGIC_PASS`；新增动作打断唯一写者，消费 L04/L05 与已裁决 L03 presence 结果，覆盖暂停/恢复、受击、失能、presence 变化、取消、完成和确定性回退；命令指纹回执支持幂等与冲突拒绝；C# 编译、smoke、禁用引用扫描、JSON 解析和 diff check 通过；Runtime 延后。
