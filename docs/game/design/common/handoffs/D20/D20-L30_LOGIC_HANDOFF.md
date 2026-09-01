# D20-L30 逻辑交接说明

- 任务：`D20-L30`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L30/D20_L30_LOGIC_RESULT.json`

## 已完成

- 新增 `D20AnimalBehaviorService`，提供动物注册、行为命令、read model、snapshot 和确定性回执。
- 动物状态变化只经 D20-L22 行为状态机；宠物跟随只调用 D20-L23 公开 eligibility；wildlife/aquatic 通过 adapter 读取，不复制或裁决生态 owner 事实。
- 覆盖 animal command duplicate/conflict、宠物所有权缺失安全返回、wildlife/aquatic 只读投影；未修改 D11 具体实现、旧 A/R/S runtime 或指定状态/记忆文件。

## 公开接口

- `D20AnimalBehaviorService.Register(...)`、`Execute(...)`、`Apply(...)`、`Read(...)`；
- `ReadWildlife(...)`、`ReadAquatic(...)`、`EvaluatePetFollow(...)`；
- `D20AnimalBehaviorServiceResult` 和 `D20AnimalBehaviorReadModel`。

## 验证与延期

- D20-L30 批量逻辑 runner 3 assertions PASS；编译 0 warnings、0 errors；生产文件禁用依赖扫描 0 hits。
- 未启动 Player/PlayMode，未验证真实模型/动画/场景、生态 owner 持久化、网络、LLM、设备、性能或 Runtime 组合。

## Coordinator memory entry

协调器在批次汇合时记录：`D20-L30 LOGIC_PASS`；新增纯逻辑动物行为服务，行为状态机写入、宠物 follow eligibility 与生态只读 adapter 边界清晰；3 assertions、编译和禁用依赖扫描通过；真实生态/Unity Runtime 延期。
