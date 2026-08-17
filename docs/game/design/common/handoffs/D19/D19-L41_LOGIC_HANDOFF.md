# D19-L41 逻辑交接文档：D19 统一逻辑测试运行器

## 1. 任务元数据
- 任务编号：`D19-L41`
- 所属章节：`D19 港口领域逻辑（波次 19.7 整合写者与全流程验证）`
- 状态：`LOGIC_PASS`
- 编写范围：
  - `tools/d19_logic_test_runner.py`
  - `tools/tests/test_d19_logic_test_runner.py`
- 交付物：
  - `tools/d19_logic_test_runner.py`
  - `tools/tests/test_d19_logic_test_runner.py`
  - `game/tools/results/D19/D19-L41/D19_L41_LOGIC_RESULT.json`
  - `docs/game/design/common/handoffs/D19/D19-L41_LOGIC_HANDOFF.md`

## 2. 运行器架构与能力
`tools/d19_logic_test_runner.py` 提供了面向 D19 逻辑全套件的统一离线自动化执行与度量汇总能力：
1. **确定性发现与执行**：固定顺序执行 D19 逻辑边界静态审计（L40）、审计工具自测、状态总览同步与完整性核对；
2. **严密失败传播**：任何一个测试或子套件退出码非 0 时，运行器总汇总 `all_passed` 恒为 `false` 并以 exit 1 退出；
3. **机器可读报告**：输出格式化的 JSON 报告，包含各子项命令、耗时（ms）、退出码、标准输出/错误，供下游 L43 汇合门禁直接审查。

## 3. 验证结果
- `python -m unittest tools/tests/test_d19_logic_test_runner.py` 全部 4 组单元测试通过；
- `python tools/d19_logic_test_runner.py` 真实执行返回 100% PASS。
