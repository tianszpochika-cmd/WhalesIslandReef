# D21-L18 逻辑交接

## 已完成

- `D21VoyageEventContracts.cs` 定义航程事件、结果、输入事实、因果 ID、恢复路由和日志序列。
- 失败/中断事件必须带恢复路由，恢复事件必须有因果来源；事件可适配既有 `WorldEvent`。
- 稳定 JSON 编解码、事件日志追加、`event_id + save_revision` 去重和 snapshot restore 已实现。

## 验证证据

- 纯 C# 编译：`CSC_EXIT=0`。
- 冒烟：`L18_EVENT_PASS`。
- 边界/因果：`L18_CAUSAL_ID_PASS`。
- 禁用能力扫描：`FORBIDDEN_API_SCAN=PASS`。

## 依赖与未宣称

- 后续 owner 必须保留 `input_facts`、`cause_ids`、结果和恢复路由，不得在表现层拼接事件 JSON。
- D20-L40 与 D21-L24-L29 仍是跨模块聚合门；真实事件总线、Save 介质、恢复执行和 UI 均未验证。
- Player/PlayMode、场景/Prefab、输入、物理、Shader、性能、真机和发行验证均 Runtime Deferred。

## Coordinator memory entry

`2026-09-01：D21-L18 已落地航程事件/结果/因果 ID/恢复路由、稳定 JSON 和 event_id+save_revision 日志去重；事件总线与跨 owner 效果仍待 D21 聚合任务接线。`
