# D21-L29 逻辑交接

## 已完成

- `tools/d21_logic_boundary_audit.py` 的 `_code_lines` 已改为逐字符状态机：跳过行注释、块注释、普通字符串、字符字面量、C# verbatim 字符串及转义内容，只把可执行代码交给规则匹配。
- 自测夹具覆盖普通字符串、verbatim 字符串、块/行注释和执行代码；审计规则仍覆盖 Unity/runtime、network/LLM、系统时钟/不可控随机、跨 owner 私写和二进制运行时资源。
- 新增 `tools/tests/test_d21_logic_boundary_audit.py`，覆盖注释/字符串排除、执行违规检出、内置自测和真实 D21 目录扫描。

## 真实验证证据

- `python -m py_compile tools/d21_logic_boundary_audit.py tools/tests/test_d21_logic_boundary_audit.py`：EXIT=0。
- `python tools/d21_logic_boundary_audit.py --self-test`：EXIT=0；good fixture 无违规，bad fixture 检出 D21-AUDIT-001、002、003。
- `python -m unittest tools/tests/test_d21_logic_boundary_audit.py`：EXIT=0；3 tests，全部通过。
- `python tools/d21_logic_boundary_audit.py`：EXIT=0；扫描 29 个 D21 C# 文件，D21-AUDIT-001—005 violations=0，未发现二进制运行时资源。
- 当前 D21 统一离线逻辑入口也通过并输出 11 cases、16 crew assertions、0 failures；该命令服务长跑证据由 D21-L27 负责，不把 L29 静态审计扩大为运行时验收。

## 未宣称与后续边界

- 本 handoff 只证明静态纯逻辑边界和工具测试，不证明 Unity Player/PlayMode、场景/Prefab/UI/Input/Transform/Animator/物理、平台、真机、SaveSystem 或发行就绪。
- D20-L40 公开 crew projection 真实 owner 接入仍 deferred；本审计及相关 D21 测试不关闭 D20 门禁。
- 不运行状态同步脚本，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## 交付物

- `tools/d21_logic_boundary_audit.py`
- `tools/tests/test_d21_logic_boundary_audit.py`
- `game/tools/results/D21/D21-L29/D21_L29_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L29_LOGIC_HANDOFF.md`

## Coordinator memory entry

`2026-09-01：D21-L29 审计扫描器已修复注释/字符串状态处理；自测、3 个单元测试和 29 个 D21 C# 文件的 D21-AUDIT-001—005 全量扫描通过，0 violations。共享当天记忆文件按范围未修改。`
