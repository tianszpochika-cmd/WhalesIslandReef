# D21-L17 逻辑交接

## 已完成

- `D21VoyageRiskSystem.cs` 只读消费 D9 天气、D11 生态和 D4 生存公开快照，形成带来源事实 ID 的 D21 风险输入。
- 暗礁、风暴、敌对和疾病按固定顺序计算资格、permille 阈值、确定性 seed/roll。
- 风险 owner 不写天气、生态、船员或船况事实，也不使用系统时钟、随机 API、网络或 LLM。

## 验证证据

- 纯 C# 编译：`CSC_EXIT=0`。
- 冒烟：`L17_RISK_PASS`。
- 边界：`L17_INVALID_INPUT_REJECT_PASS`。
- 禁用能力扫描：`FORBIDDEN_API_SCAN=PASS`。

## 依赖与未宣称

- 上游快照只作为显式公开证据；D20-L40 未验收的身份/路线投影不得被当作权威风险事实。
- 风险解决、补给扣除、船况变化、事件表现和 UI 反馈留给后续 owner/聚合任务。
- 天气/生态真实运行时、物理、场景、性能、真机和发行验证均 Runtime Deferred。

## Coordinator memory entry

`2026-09-01：D21-L17 已落地 D9/D11/D4 公开快照到风险输入的只读适配，按暗礁/风暴/敌对/疾病固定顺序输出资格与确定性 seed/roll；风险效果仍由后续 owner 聚合。`
