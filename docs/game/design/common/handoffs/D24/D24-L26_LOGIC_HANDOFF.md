# D24-L26 逻辑交接

- 状态：`LOGIC_PASS`。
- 固定基线：`game/tools/results/D24/D24-L26/D24_L26_BASELINE.json`。
- 当前比对：`game/tools/results/D24/D24-L26/D24_L26_COMPARE.json`。
- 真实命令：`python tools/d24_regression_baseline_compare.py --root . --baseline game/tools/results/D24/D24-L26/D24_L26_BASELINE.json --current game/tools/results/D24/D24-L25/D24_L25_RUNNER_REPORT.json --json-out game/tools/results/D24/D24-L26/D24_L26_COMPARE.json`。
- 结果：8/8 checks 通过；固定下限覆盖 D18—D23 结果/交接数量、D24 audit/C#、AgentCore、D23 marker，以及迁移器/错误码源码存在性。
- 规则：baseline 不由工具隐式改写；任一数量下降、failure 上升、关键 marker 删除或当前 runner 非 PASS 都会使比对失败。
- 延期：这是纯逻辑防回归门禁，不是 Unity Runtime、可玩候选版或 Release 门禁。
