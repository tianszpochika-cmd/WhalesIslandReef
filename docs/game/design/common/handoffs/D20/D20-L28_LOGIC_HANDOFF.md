# D20-L28 逻辑交接说明

- 任务：`D20-L28`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L28/D20_L28_LOGIC_RESULT.json`

## 已完成

- 新增 `D20CharacterCommandService`，承接角色 presence、动作启动/中断/完成、外观选择、NPC 交互可用性和 D19 港口命令。
- 命令拥有稳定 `CommandId`、确定性 fingerprint、同 fingerprint 重放回执和冲突拒绝；不重复写入动作、presence 或港口 owner 事实。
- presence、动作 gate、外观和 NPC interaction 均调用 D20 已有公开逻辑；港口命令严格调用注入的 `D19PortCommandRouter.Route(D19RoutedPortCommand)`。
- 未修改旧 A/R/S runtime、D10、D11、D19 所有权实现或指定状态/记忆文件。

## 公开接口

- `D20CharacterCommandService.Execute(D20CharacterCommand)` / `Handle(...)`；
- `D20CharacterCommand` 及 `ForPresence`、`ForAction`、`ForPort` 工厂；
- `D20CharacterCommandResult`：`Accepted`、`Succeeded`、`IsDuplicate`、失败原因和 owner result。

## 验证与延期

- `Whalebone.Systems` 基线编译、D20 通配纯 C# 编译和 L28-L35 批量 runner 编译均为 0 warnings、0 errors。
- 批量 runner：L28 5 assertions PASS；L29-L35 同批全部 PASS；生产 D20 文件禁用依赖扫描 0 hits。
- 未启动 Player/PlayMode，未验证真实场景、UI/Input/Animator/Canvas、网络、LLM、设备、性能、SaveSystem 或跨域 Runtime 组合；这些保持 `Runtime Deferred`。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L28 LOGIC_PASS`；新增确定性 D20 角色命令服务，统一角色命令入口、幂等回执和 D19 port router 适配；纯 C# 编译、批量逻辑 smoke 与禁用依赖扫描通过；Unity/真实 Runtime 组合延期。
