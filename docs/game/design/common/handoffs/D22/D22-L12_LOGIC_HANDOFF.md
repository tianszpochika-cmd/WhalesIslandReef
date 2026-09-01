# D22-L12 Logic Handoff

| Field | Value |
|---|---|
| Task | `D22-L12` |
| Status | `LOGIC_PASS` |
| Date | `2026-09-01` |
| Source | `game/Assets/Scripts/Systems/D22/D22PointerModeReadModel.cs` |
| Result | `game/tools/results/D22/D22-L12/D22_L12_LOGIC_RESULT.json` |

## 已完成

- 定义 `Default`、`Free`、`Locked` 逻辑模式和 pointer 可见性需求。
- 模态/文本输入/触控/暂停优先使用 Free；明确请求且允许捕获时使用 Locked；捕获不可用时返回 Free + `LockUnavailable`。
- 缺少、失效或非活动上下文稳定降级为安全 Default/Visible 读模型。
- Context、读模型和 projector 均不持有或控制设备对象。

## 边界与交接

`D22PointerModeReadModel` 是 Runtime 的需求输入，不是 cursor 或输入 API。Runtime 必须自行决定设备能否满足需求，并把失败留在 Runtime Deferred，不得把可见性结果写成领域事实。

## 证据

- D22-L02-L16 纯 C# 文件及 D16 accessibility contract 内存编译通过。
- 确定性 smoke 覆盖三种模式、modal/free、锁定降级和缺少上下文。
- 注释剥离后的依赖扫描、结果 JSON 解析和 `git diff --check` 通过。

## Runtime Deferred

真实 cursor/pointer API、设备输入、UGUI/EventSystem、焦点、Canvas、布局、动画、Player/PlayMode、设备、性能和发布验证未执行。

## Blockers

本任务无逻辑 blocker；真实 pointer 行为需要 Runtime 适配器和设备证据。

## Coordinator memory entry

```text
2026-09-01 D22-L12：已完成纯 C# pointer mode read model；Default/Free/Locked、Visible/Hidden、modal/text/touch/paused 与 lock unavailable fallback 均有确定性投影，不控制设备。未修改 LOGIC_TASK_STATUS.md、.workbuddy/memory/2026-09-01.md，未执行 status sync；cursor/Input/UGUI/Player/PlayMode 保持 Deferred。
```
