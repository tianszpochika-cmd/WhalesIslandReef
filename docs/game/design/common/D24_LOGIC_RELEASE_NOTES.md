# D24 逻辑候选版说明

> 对应任务：`D24-L31`。这是纯逻辑候选版说明，不是可玩候选版、Runtime candidate 或 Release candidate。

## 1. 本次交付

- 冻结 D18—D23 逻辑接受快照、契约版本、D23 固定 seed/ID 和候选版边界。
- 建立 D24-L25 可重复总 runner：15 项 D24 Python 审计、D24-L17—L22 纯 C#、AgentCore 离线 build/`--d9-check`、D23 26-case 场景以及 D18—D23 结果/handoff 矩阵。
- 建立 D24-L26 不自动改写的固定 baseline，阻止回归数量下降、D23 failure 增加、migration/error marker 删除。
- 增加 D24-L27 纯 C# 跨章节回放、中断和 50 Tick 长跑测试。
- 发布迁移兼容、逻辑覆盖、Runtime backlog、边界审计、状态复核和 Runtime 阶段有限交接文档。

## 2. 逻辑证据摘要

| 检查 | 结果 |
|---|---|
| D18—D23 任务证据 | 247/247 results、247/247 handoffs、247/247 `LOGIC_PASS` |
| D24-L25 总回归 | 5/5 suites PASS |
| D24 Python audits | 15/15 PASS |
| D24-L17—L22 | 6/6 pure C# PASS |
| AgentCore | build + offline `--d9-check` 2/2 PASS |
| D23 场景 | 26 cases、13925 assertions、0 failures |
| D24-L26 baseline | 8/8 comparison checks PASS |
| D24-L27 | build/run 0 warning/0 error、7003 assertions、0 failures |

权威机器报告位于：

- `game/tools/results/D24/D24-L25/D24_L25_RUNNER_REPORT.json`
- `game/tools/results/D24/D24-L26/D24_L26_COMPARE.json`
- `game/tools/results/D24/D24-L27/D24_L27_RUNNER_REPORT.json`

## 3. 已知限制与风险

- 旧章节结果中存在少量历史协议形状差异；D24-L25 将其作为兼容 finding 记录，并以任务状态、结果可解析性和文件完整性作门禁。
- D18 部分资源哈希仍 Pending；Candidate、Pending、Editor-only 或来源证据不能在本说明中升级为 RuntimeApproved。
- 全仓库历史 D9/D15/D16 内容字段/review-packet 基线另有待清理项，不由 D24 runner 隐式删除、跳过或改写。
- 真实 SaveSystem、网络、Agent runtime、设备、性能、视觉、UI、输入、物理和发行证据均未执行。

## 4. 禁止误读

本说明中的“候选版”只表示代码/数据/契约/静态检查/纯 C# 和确定性模拟可重复。它不表示“游戏已经可玩”“画面已经完成”“实机通过”“性能达标”“Steam ready”，也不授予 `RuntimeApproved` 或 `ReleaseApproved`。后续 Runtime 只可按 [D24_RUNTIME_VALIDATION_BACKLOG.md](D24_RUNTIME_VALIDATION_BACKLOG.md) 和 D24-L36 handoff 建立新门禁。
