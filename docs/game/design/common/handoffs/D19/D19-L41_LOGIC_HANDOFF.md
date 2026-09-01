# D19-L41 逻辑交接文档：D19 统一逻辑测试运行器

## 1. 任务元数据
- 任务编号：`D19-L41`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：
  - `tools/d19_logic_test_runner.py`
  - `tools/tests/test_d19_logic_test_runner.py`
  - `tools/d19_logic_result_integrity_audit.py`
  - `tools/tests/test_d19_logic_result_integrity_audit.py`
- 交付物：
  - `tools/d19_logic_test_runner.py`
  - `tools/tests/test_d19_logic_test_runner.py`
  - `game/tools/results/D19/D19-L41/D19_L41_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L41_LOGIC_HANDOFF.md`

## 2. 运行器架构与能力
`tools/d19_logic_test_runner.py` 提供了面向 D19 逻辑全套件的统一离线自动化执行与度量汇总能力：
1. **确定性发现与执行**：固定顺序执行 D19 逻辑边界静态审计（L40）、审计工具自测、48 个结果协议完整性审计和状态总览同步；
2. **严密失败传播**：任何一个测试或子套件退出码非 0 时，运行器总汇总 `all_passed` 恒为 `false` 并以 exit 1 退出；
3. **机器可读报告**：输出格式化的 JSON 报告，包含各子项命令、耗时（ms）、退出码、标准输出/错误；结果完整性审计还逐项核对 task_id、状态、时间、verification、延期字段和 handoff 文件，供下游 L43 汇合门禁直接审查。

## 3. 验证结果
- `python -m unittest tools/tests/test_d19_logic_test_runner.py tools/tests/test_d19_logic_result_integrity_audit.py tools/tests/test_d19_logic_boundary_audit.py`：13/13 全部通过；
- `python tools/d19_logic_test_runner.py`：4/4 子套件通过，D19-L01..L48 结果协议完整性 48/48 通过；
- Unity EditMode 程序集实测不由本 Python 运行器伪装代替，已由 D19-L48 单独记录 117/117 通过。
