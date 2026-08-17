# D16-C06 → D17-C04 单 Agent 顺序执行总提示词

> 把下面代码块整段复制给一个 Agent。不要拆开，不要同时开第二个 Agent 写同一条轨。
>
> 分项实施内容、写入范围、验收标准已经写在 [GATE_PROMPTS](AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATE_PROMPTS.md)；本文件只负责顺序调度。
>
> 当前起点（2026-08-18）：`D16-C01`—`C05` 均为 `GATE_PASS`。从 **D16-C06** 开始。

```text
你是 WhalesIslandReef 的发布门禁 Agent。从 D16-C06 起，按严格串行顺序一口气做完到 D17-C04。不要等待确认，不要回头重做 C01—C05，不要改做 D18—D24 或 D19 逻辑任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01

开始前必须阅读：
1. AGENTS.md
2. docs/CONTEXT.md
3. docs/game/GATE_TASK_PROTOCOL.md
4. docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md
5. docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATE_PROMPTS.md
6. docs/game/GATE_TASK_STATUS.md
7. docs/game/design/common/D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md

然后立刻核验 game/tools/results/D16/D16-C0{1,2,3,4,5}/ 的 GATE_RESULT.json 都是 GATE_PASS。不是就停，写清缺哪项，不要开 C06。

================================================================
严格顺序（一次只做一项，做完并 GATE_PASS 再下一项）
================================================================
D16-C06
D16-C07
D16-C08
D16-C09
D16-C10
D16-C11
D16-C12
D17-C01
D17-C02
D17-C03
D17-C04

对每一项：
1. 打开 GATE_PROMPTS.md，定位该任务标题下的整段提示词，按那段执行。写入范围、前置、验收以那段为准，不要自己另拆子任务号。
2. 先读该项精确前置的结果 JSON。前置不是 GATE_PASS，本项只写 GATE_BLOCKED 并停止整条链。
3. 只改该项唯一写入范围。不要顺手修相邻任务，不要改 C01—C05 已提交结果，不要改 D18—D24 的 *_LOGIC_RESULT.json 或 LOGIC_TASK_STATUS.md。
4. 完成后必须留下：
   - game/tools/results/[DXX]/[TASK_ID]/[TASK_ID下划线]_GATE_RESULT.json
   - docs/game/design/common/handoffs/[DXX]/[TASK_ID]_GATE_HANDOFF.md
   - 当天 .workbuddy/memory/YYYY-MM-DD.md
   - 运行 python tools/sync_gate_task_status.py
   - 运行 git diff --check
5. 用 sync 后的 GATE_TASK_STATUS.md 确认本项已是 GATE_PASS，再进入下一项。

================================================================
停止规则（碰到就停，不要跳号）
================================================================
- 本项只能写成 GATE_PARTIAL 或 GATE_BLOCKED：立即停止整条链，报告卡在哪个编号、缺什么证据、下一项是谁。不要开下一项。
- 缺真实设备、人工审查责任人、最终授权资源、断电/跨平台材料时，写真实 GATE_BLOCKED。禁止用本机 PlayMode、合成手柄、Kenney 候选包、低模人物、D18/D19 LOGIC_PASS 冒充通过。
- 禁止 Steam 上传。禁止脚本批量批准 A04。禁止把 placeholder 改状态清零。
- 禁止执行旧号 D16-C13—C31、D17-C05—C16。
- 禁止并行开下一项。C12 也按顺序做，做完再开 D17-C01（D17-C01 的硬前置仍是 D16-C11=GATE_PASS；C12 失败则停在 C12，不要跳去 D17）。

================================================================
每项在链中的职责（细节仍以 GATE_PROMPTS 原文为准）
================================================================
D16-C06  A04 剩余资产审查，批次 005—008，86 条。不要碰 001—004。
D16-C07  A04 历史内容 009—012 + 关闭 envelope，pending_count=0。必须 C02—C06 全过。
D16-C08  方案 B：自行检索可商用免费资源，只替换切片 4 项并先保证能运行；不要把 Kenney 方块人标最终角色。若当前结果仍是上一轮 BLOCKED，按 GATE_PROMPTS 修订段重做后再继续 C09。
D16-C09  A01/A02/A03/A06 四份外部真实证据一次收齐。缺真机就 BLOCKED。
D16-C10  六门审计 → S01 就绪 → 长线故事 → 复核。中途失败就停。
D16-C11  S02 交接 + D16 章节放行。这是 D17-C01 的硬前置。
D16-C12  旁路三项一次做完：本机安装器回归、S01 夹具、确认 Steam 未上传。
D17-C01  切片冻结 + 最终角色/环境资源登记。不改运行时。
D17-C02  物理 → 上下船 → 镜头 → 动作 → 失败恢复，一条运行时切片做完。
D17-C03  真机输入 + 目标性能 + 资源场景回归。
D17-C04  正式连续 10 次故事 + 重启复核 + 状态审计 + 章节验收。禁止复用 2026-08-12 本机十循环 XML。

================================================================
全部做完时
================================================================
再跑一次 python tools/sync_gate_task_status.py。
最终只报告：每项真实 GATE_*、卡点（如有）、GATE_TASK_STATUS.md 汇总数字、以及 D16-C06 到 D17-C04 是否全部 GATE_PASS。
不要宣称 Steam 已发布，不要宣称 D18—D24 因此关闭。
```
