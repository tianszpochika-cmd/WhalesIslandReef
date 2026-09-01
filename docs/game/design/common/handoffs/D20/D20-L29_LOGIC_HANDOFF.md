# D20-L29 逻辑交接说明

- 任务：`D20-L29`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L29/D20_L29_LOGIC_RESULT.json`

## 已完成

- 新增 `D20NpcBehaviorService`，提供 NPC 注册、命令 apply、确定性 tick、交互 availability、read model 和 snapshot capture。
- 行为状态只由 D20-L20 状态机裁决；身份、日程和 owner projection 作为显式输入，owner 经济/关系/任务等事实不在本服务写入。
- 统一 receipt/fingerprint 幂等规则；通过 `D20NpcBehaviorAuthorityGuard` 区分行为 projection 写入、owner 提案和非法 CoreAgent/外部事实写入。
- 未修改旧 A/R/S runtime、D10、D11、D19 所有权实现或指定状态/记忆文件。

## 公开接口

- `D20NpcBehaviorService.Register(...)`、`Execute(...)`、`Apply(...)`、`Tick(...)`；
- `EvaluateInteraction(...)`、`Read(...)`、`CaptureBehaviorSnapshots()`；
- `D20NpcBehaviorServiceResult` 和 `D20NpcBehaviorReadModel`。

## 验证与延期

- D20-L29 批量逻辑 runner 4 assertions PASS；编译 0 warnings、0 errors；生产文件禁用依赖扫描 0 hits。
- 已覆盖命令幂等、日程派生行为、交互可用性、owner read-only projection 和 authority guard；未启动 Player/PlayMode。
- 真实 NPC 场景/视觉/导航、owner service 组合、SaveSystem、网络、LLM、设备、性能与 Runtime 迁移保持延期。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L29 LOGIC_PASS`；新增纯逻辑 NPC 行为服务，行为由 D20 状态机裁决，owner 事实只读且 authority guard 覆盖非法写入；4 assertions、编译和禁用依赖扫描通过；真实 Runtime/owner 组合延期。
