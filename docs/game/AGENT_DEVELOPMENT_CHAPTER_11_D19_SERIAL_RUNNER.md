# D19-L33 → D19-L48 单 Agent 顺序执行总提示词

> 把下面代码块整段复制给一个 Agent。不要拆开，不要同时开第二个 Agent 写 L33—L48。
>
> 分项实施内容、写入范围、验收标准已经写在 [D19_PROMPTS](AGENT_DEVELOPMENT_CHAPTER_11_D19_PROMPTS.md)；本文件只负责顺序调度。
>
> 当前起点：用户确认 `D19-L01`—`L32` 均按 `LOGIC_PASS` 处理（含进行中的 L27，按已完成计）。从 **D19-L33** 开始。

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。从 D19-L33 起，按严格串行顺序一口气做完到 D19-L48。不要等待确认，不要回头重做 L01—L32，不要改做 D16/D17 门禁轨或 D20—D24。

项目根目录：C:\Users\Administrator\Desktop\game\first01

开始前必须阅读：
1. AGENTS.md
2. docs/CONTEXT.md
3. docs/DEVELOPMENT_STANDARDS.md
4. docs/game/LOGIC_TASK_PROTOCOL.md
5. docs/game/AGENT_DEVELOPMENT_TASKS.md
6. docs/game/AGENT_DEVELOPMENT_CHAPTER_11_D19.md
7. docs/game/AGENT_DEVELOPMENT_CHAPTER_11_D19_PROMPTS.md
8. docs/game/LOGIC_TASK_STATUS.md

然后核验：D19-L01—L32 按已完成处理。L27 即使看板仍显示进行中，也视为 LOGIC_PASS，不要重做、不要改它的结果。若 L33 的提示词要求“L02—L32 全部 LOGIC_PASS”，把 L27 记为已满足前置即可。L33—L48 必须仍是未完成；若发现其中某项已经 LOGIC_PASS，跳过该项，从第一个未 PASS 的编号继续，不要重写已通过结果。

================================================================
严格顺序（一次只做一项，做完并 LOGIC_PASS 再下一项）
================================================================
波次 19.7
D19-L33
D19-L34
D19-L35
D19-L36
D19-L37
D19-L38
D19-L39
D19-L40
D19-L41
D19-L42

波次 19.8
D19-L43
D19-L44
D19-L45
D19-L46
D19-L47
D19-L48

对每一项：
1. 打开 docs/game/AGENT_DEVELOPMENT_CHAPTER_11_D19_PROMPTS.md，定位该任务标题下的整段提示词，按那段执行。写入范围、精确前置、验收以那段为准，不要自己另拆子任务号。
2. 先读该项精确前置的结果 JSON。除 L27 按已完成计外，任一真实前置不是 LOGIC_PASS，本项只写 LOGIC_BLOCKED 并停止整条链。
3. 只改该项唯一写入范围。不要顺手修相邻任务或已通过的 L01—L32 生产代码。L43—L48 是只读汇合门禁：禁止改代码、测试、契约、场景或前置结果。
4. 完成后必须留下：
   - game/tools/results/D19/[TASK_ID]/[TASK_ID下划线]_LOGIC_RESULT.json
   - docs/game/design/common/handoffs/D19/[TASK_ID]_LOGIC_HANDOFF.md
   - 当天 .workbuddy/memory/YYYY-MM-DD.md
   - 运行 python tools/sync_logic_task_status.py
   - 运行 git diff --check
5. 用 sync 后的 LOGIC_TASK_STATUS.md 确认本项已是 LOGIC_PASS，再进入下一项。

================================================================
停止规则（碰到就停，不要跳号）
================================================================
- 本项只能写成 LOGIC_PARTIAL 或 LOGIC_BLOCKED：立即停止整条链，报告卡在哪个编号、缺什么、下一项是谁。不要开下一项。
- 禁止启动 Unity Player/PlayMode，禁止改场景、Prefab、Canvas、输入、碰撞、Builder、Build Settings、外部二进制资源。
- Systems/D19 保持纯 C# 领域逻辑：无 UnityEngine、无网络、无 LLM。不得私写 D2/D3/D4/D9/Save 拥有的货币、库存、船只、船员或存档字段。
- 禁止把本机画面、截图、FPS、旧 R03、D16 GATE_* 写成 D19 LOGIC_PASS。
- 禁止并行开下一项。L33 的前置是 L02—L32 全过；L34 等 L33；以此类推直到 L48。

================================================================
每项在链中的职责（细节仍以 D19_PROMPTS 原文为准）
================================================================
D19-L33  唯一港口命令路由
D19-L34  中途失败补偿
D19-L35  事件顺序审计
D19-L36  无场景确定性港口剧本测试
D19-L37  固定种子 ≥1000 命令不变式模糊测试
D19-L38  保存→加载→继续命令等价
D19-L39  旧存档迁移夹具
D19-L40  逻辑边界静态审计工具
D19-L41  统一逻辑测试 runner
D19-L42  给 D20—D23 的港口逻辑接口文档
D19-L43  只读复核 P0 证据
D19-L44  只读持久化/所有权审计
D19-L45  只读工作流与不变式审计
D19-L46  只读 Runtime Deferred 登记
D19-L47  只读章节状态审查
D19-L48  D19 唯一逻辑验收；决定对 D20/D23 的放行

================================================================
全部做完时
================================================================
再跑一次 python tools/sync_logic_task_status.py。
最终只报告：L33—L48 每项真实 LOGIC_*、卡点（如有）、LOGIC_TASK_STATUS.md 里 D19 的汇总数字、以及是否 48/48 LOGIC_PASS。
不要宣称港口画面/交互已完成，不要启动 D20，除非 L48 明确放行。
```
