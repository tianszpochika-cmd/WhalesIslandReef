# D22 UI 命令、焦点与读模型逻辑验收

> 归属：`D22-L36`。本文件只验收 D22 纯逻辑交接，不把 Unity UI、设备、视觉、性能或发行状态写成通过。
>
> 验收日期：2026-09-01（Asia/Shanghai）。机器事实：`game/tools/results/D22/D22-L33/D22_L33_RUNNER_REPORT.json`。

## 1. 验收门槛

D22-L33—L36 采用 fail-closed 汇合规则。只有以下条件全部满足，章节逻辑结论才可为 `LOGIC_PASS`：

- D21 acceptance、D21-L01—L32 结果与 handoff 完整且均为 `LOGIC_PASS`；
- D22-L01—L32 的 32 个结果 JSON 与 32 个 handoff 存在、可解析、协议字段完整、状态均为 `LOGIC_PASS`；
- D22 源与测试入口真实完成离线编译，确定性测试和断言标记通过；
- D22 边界审计没有 Canvas、EventSystem、Input、Unity UI、网络、LLM、墙钟、不可控随机或业务私写违规；
- 未启动 Unity Player/PlayMode、场景、Prefab、真实输入、性能或发布流程。

缺少结果、handoff、上游 acceptance、命令退出码或断言标记时，不得将本章写成 `LOGIC_PASS`。

本次真实验收结果：D22 runner 在 `2026-09-01T23:13:53+08:00` 完成 8/8 suites，D21 上游与 D22-L01—L32 均为 32/32 结果和 handoff 完整，`pass_eligible=true`，因此 D22-L33—L36 当前结论为 `LOGIC_PASS`。

## 2. 当前逻辑交付

| 领域 | 已验收的公开能力 |
|---|---|
| 屏幕/面板/模态 | 稳定逻辑 ID、互斥规则、打开/关闭/返回/替换与不可恢复错误状态转换 |
| 焦点与暂停 | 逻辑焦点 token、modal 恢复栈、确定性失焦回退、pause/escape/pointer intent/read model；不读取设备、不操作 `Time.timeScale` |
| 命令与反馈 | 结构化命令信封、上下文/权限/焦点/修订校验、公开 owner handler、关联 ID、owner 回执、失败/重试/幂等/禁用 reason 与 localization key |
| 通知与可访问性 | 去重、优先级、确认、过期、保存投影，以及缩放/对比度/字幕/减少动态效果等纯读模型 |
| 业务只读适配 | 市场、港口、船舶、库存、NPC、任务和 Save 的防御性投影；owner 仍是唯一事实写者 |
| ViewModel | 固定 `market → port → ship → inventory → crew → quest → save` 模块顺序，缺失/失败/版本不匹配显式降级 |
| Save/Load UI 投影 | 只保存 D22 UI 幂等/通知/焦点等投影，不写领域快照、PlayerPrefs 或 owner 私有状态 |

## 3. 机器证据

权威机器报告列出每条真实命令、退出码、状态、断言标记和证据审计结果：

`game/tools/results/D22/D22-L33/D22_L33_RUNNER_REPORT.json`

当前定向测试应至少包含：D22 adapter NUnit 10 cases、focus/panel 13 assertions、command/feedback 8 assertions、确定性 UI 场景 44 assertions、固定种子 fuzz 77,817 assertions、Save/Load 21 assertions，且 `D22_TEST_FAILURES=0`。如果这些标记或源文件数量发生变化，必须重新运行 runner，不能手改本验收文档或结果数字。

## 4. D23 可消费边界

D23 只能使用 `D22UiCommandRequest`、`D22UiCommandResult/D22UiCommandRouteResult`、`D22FeedbackRecord`、`D22ViewModel` 和公开的 read-model source registration。D23 不得直接写 D3/D12/D14/D19/D20/D21/Save 事实，不得把 `Blocked`、`Rejected`、`Pending`、`Duplicate` 或 `Cancelled` 显示成业务成功，也不得隐藏 Composer 的缺失/版本 reason。

## 5. 未放行范围

详见 [`D22_RUNTIME_DEFERRED_REGISTER.md`](D22_RUNTIME_DEFERRED_REGISTER.md)。真实 Canvas/UGUI/EventSystem、键鼠手柄/触摸、点击和焦点设备行为、分辨率布局、字幕/通知视觉、动画/音频、场景/Prefab/Player/PlayMode、性能、目标设备、真实 Save 介质和发行验证均保持 `Runtime Deferred`；本文件不授予 `RuntimeApproved` 或 `ReleaseApproved`。
