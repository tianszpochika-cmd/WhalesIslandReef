# D20-L19 逻辑交接说明

- 任务：`D20-L19`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L19/D20_L19_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcBehaviorAuthorityGuard.cs`：

- `D20NpcBehaviorAuthorityDomain` 区分 D20 行为投影、日程/交互/表现只读面与既有外部域；
- `D20NpcBehaviorAuthorityGuard.Evaluate` 只允许 `D20NpcBehaviorStateMachine` 写入行为投影；经济、关系、库存、任务、AgentCore、身份、健康和港口事实写入均拒绝；
- 外部 owner 的操作只能使用 `SubmitOwnerRequest + IsProposal`，守卫不会执行请求或改变外部事实；
- `D20NpcBehaviorAuthorityClaim` 与 `Audit(...)` 对权限声明集合做可独立运行的静态式审计，发现缺失 proposal、错误 owner、读模型写入或外部事实写入即返回稳定违规原因；
- 所有结果带 `Allowed`、`IsReadOnly`、域/操作、owner 和稳定失败码，便于后续聚合层保留边界证据。

## 2. 后续可消费公开 API

- `D20NpcBehaviorAuthorityGuard.Evaluate(...)`、`CanWrite(...)`、`CanSubmitOwnerRequest(...)`、`Audit(...)`；
- `D20NpcBehaviorAuthorityRequest`、`D20NpcBehaviorAuthorityResult`；
- `D20NpcBehaviorAuthorityClaim`、`D20NpcBehaviorAuthorityAuditResult`；
- `D20NpcBehaviorAuthorityDomain`、`D20NpcBehaviorAuthorityOperation` 与权限校验/失败码。

守卫通过不代表任何外部业务已经成功；后续 L28/L29 必须继续调用对应 owner 的公开命令入口。

## 3. 验证与未完成项

- 依赖链纯 C# 编译：现代 Roslyn `/warnaserror+`，`CSC_EXIT=0`、无警告；
- 确定性 smoke 覆盖允许的行为投影写入、外部域拒绝、proposal 语义和批量声明审计，共 25 项汇合检查通过；
- 禁用引用/尾随空白扫描均为 0；`git diff --check` exit 0（Git 仅提示既有换行转换）；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实程序集扫描接入、owner 路由和运行时集成延期。

## Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L19 LOGIC_PASS`；记录 NPC 行为投影唯一写者、外部域 proposal-only 守卫、静态声明审计和未启动 Runtime。
