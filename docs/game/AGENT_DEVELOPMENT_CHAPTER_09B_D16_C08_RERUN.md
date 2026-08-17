# D16-C08 方案 B 重跑提示词

> 下面整段直接复制给 Agent。只重做 C08，不要开 C09。

```text
你是 WhalesIslandReef 的发布门禁 Agent。用户已选方案 B，现在重做 D16-C08。不要等待确认，不要开 D16-C09 及以后，不要改 D16-C01—C07，不要改 D18—D24 / D19 逻辑任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01

上一轮 C08 是 GATE_BLOCKED，原因是只有 Kenney 候选、没有最终文件。本轮必须自己检索并下载可商用免费资源，做成 4 个可运行的切片最终资源后再预检接入。先保证垂直切片能跑，不追求 2264 个 placeholder 全换完。

开始前阅读：
1. AGENTS.md
2. docs/CONTEXT.md
3. docs/game/GATE_TASK_PROTOCOL.md
4. docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md
5. docs/game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATE_PROMPTS.md 里「D16-C08」方案 B 修订全文
6. docs/game/design/common/D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md 的 A05 预检规则
7. tools/d16_a05_replacement_preflight.py
8. 上一轮 game/tools/results/D16/D16-C08/D16_C08_GATE_RESULT.json（只读，不要沿用 BLOCKED 当最终结论）

精确前置：D16-C07=GATE_PASS。不是就停。

本轮只替换这 4 个：
- asset_ph_crew_male
- building_saint_marys_dock
- building_small_boat_pier
- island_saint_marys

实施、写入范围、许可红线和预检命令，严格按 GATE_PROMPTS.md 中 D16-C08 方案 B 修订段执行。

完成后必须留下结果 JSON、handoff、当天记忆，并运行：
python tools/sync_gate_task_status.py
git diff --check

最终只报告：本轮 GATE_*、4 项各自来源/许可、preflight can_apply、替换了几条、2264 是否未被清零。只有 4 项本地最终文件+Prefab+预检通过才能 GATE_PASS。找不到合规资源就继续 GATE_BLOCKED，不要把 Kenney 方块人标最终角色。
```
