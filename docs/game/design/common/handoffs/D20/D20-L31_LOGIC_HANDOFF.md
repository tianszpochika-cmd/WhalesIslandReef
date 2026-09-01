# D20-L31 逻辑交接说明

- 任务：`D20-L31`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L31/D20_L31_LOGIC_RESULT.json`

## 已完成

- 新增 `D20ActionEventRouter`，统一角色、NPC、动物命令的验证、owner result、domain fact、D20 projection 和只读 presentation 事件。
- 事件序列和事件 ID 确定性生成；同命令 fingerprint 重放不重复触发副作用，domain fact 只在 owner 成功且非重复时产生。
- `D20ActionEventRouteResult.Validate()` 校验首尾阶段、序列连续性、ID/fingerprint 稳定性、domain fact 条件和 replay 标记。
- 未修改旧 A/R/S runtime、D10、D11、D19 所有权实现或指定状态/记忆文件。

## 公开接口

- `D20ActionEventRouter.Route(...)` 的角色/NPC/动物重载及便捷请求入口；
- `D20ActionEventPhase`、`D20ActionEvent`、`D20ActionEventRouteResult`；
- `D20ActionEventRouteResult.Validate()` 和 replay-safe clone。

## 验证与延期

- D20-L31 批量逻辑 runner 6 assertions PASS；编译 0 warnings、0 errors；生产文件禁用依赖扫描 0 hits。
- 未启动 Player/PlayMode，未验证真实 UI/presentation、跨端事件传输、owner 事务/回滚、SaveSystem、网络、LLM、设备或性能。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L31 LOGIC_PASS`；新增确定性 action event router，事件阶段、owner/domain fact 条件、D20 projection 与 presentation read-only 边界通过验证；6 assertions、编译和禁用依赖扫描通过；真实 Runtime 事件传输延期。
