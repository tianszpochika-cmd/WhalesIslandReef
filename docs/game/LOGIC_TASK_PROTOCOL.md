# D18—D24 单任务逻辑开发协议

本协议由所有 `D18-Lxx`—`D24-Lxx` 任务卡引用。将章节中的“单任务执行提示词”原样复制给 Agent，并填入任务编号；该提示词加上对应任务卡构成完整的可执行指令。

## 允许的工作

- 阅读并修改任务卡明确允许的 C#、契约、测试、数据、工具或文档；
- 运行不启动 Player 的构建、静态检查、Schema 校验、单元测试和确定性模拟；
- 修复由这些检查真实发现的逻辑缺陷；
- 为任务创建其唯一结果目录、交接报告和当日记忆日志。

## 禁止的工作

- 不启动 Unity Player、PlayMode、场景漫游、截图、录屏、性能采样或 Windows 防火墙操作；
- 不改 `InitialHarbor.unity`、正式 Prefab、Canvas、Player/Harbor Builder、Build Settings 或外部二进制资源；
- 不把静态编译通过写成真实可玩、视觉完成、ReleaseApproved 或 Runtime PASS；
- 不越过 `contracts/`、系统数据所有权、AgentCore 纯 C# 边界或单一写者边界。

## 固定的结果格式

每个任务在其唯一目录创建 `Dxx_Lnn_LOGIC_RESULT.json`：

```json
{
  "task_id": "Dxx-Lnn",
  "status": "LOGIC_PASS",
  "updated_at": "2026-08-16T12:00:00+08:00",
  "summary": "一句话说明真实完成内容",
  "changed_files": ["相对路径"],
  "verification": [
    {"command": "实际执行的命令", "result": "PASS 或失败摘要"}
  ],
  "runtime_deferred": ["必须留给 Runtime 阶段的风险"],
  "blockers": [],
  "handoff": "docs/game/design/common/handoffs/Dxx/Dxx-Lnn_LOGIC_HANDOFF.md"
}
```

只可使用 `PLANNED`、`IN_PROGRESS`、`LOGIC_PASS`、`LOGIC_PARTIAL`、`LOGIC_BLOCKED`、`RUNTIME_DEFERRED`。`LOGIC_PASS` 必须没有 P0 blocker；没有证据时必须写 `LOGIC_PARTIAL` 或 `LOGIC_BLOCKED`。

完成后运行：

```powershell
python tools/sync_logic_task_status.py
git diff --check
```

同步工具会更新 [逻辑任务状态总览](LOGIC_TASK_STATUS.md)。若并行任务同时完成，重新运行一次同步工具即可；每个任务自己的结果 JSON 才是事实源。

## 单任务执行提示词模板

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]，不要等待确认，不要改做相邻任务。

项目根目录：C:\Users\Administrator\Desktop\game\first01
开始前必须阅读：AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md，以及对应章节文件中 [TASK_ID] 的任务卡。

严格按任务卡的前置条件、唯一写入范围、实施内容和验收执行。当前是纯逻辑阶段：允许编译、单元测试、契约/静态检查和确定性模拟；禁止启动 Unity Player、PlayMode、性能/截图/路线实测、外部资源导入以及任何场景、Prefab、Canvas、Build Settings 修改。

只能修改本任务列出的模块；若发现相邻模块缺陷，记录 blocker，不要越权修复。跨端数据先更新 contracts 并遵守三端同步；Systems 内不得加入网络/LLM；AgentCore 必须保持无 Unity/第三方依赖。

完成时写入 game/tools/results/[DXX]/[TASK_ID]/[TASK_ID下划线]_LOGIC_RESULT.json、docs/game/design/common/handoffs/[DXX]/[TASK_ID]_LOGIC_HANDOFF.md 和当天 .workbuddy/memory/YYYY-MM-DD.md；运行 python tools/sync_logic_task_status.py 与 git diff --check。最终仅报告 LOGIC_PASS、LOGIC_PARTIAL 或 LOGIC_BLOCKED、实际验证命令、延期的 Runtime 风险和结果路径。
```
