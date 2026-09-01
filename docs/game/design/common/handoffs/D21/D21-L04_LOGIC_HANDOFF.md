# D21-L04 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：船况数值、事实驱动损伤、适航等级和维修资格；物理/天气/火灾采样不在本任务内。

## 已完成

- `D21ShipConditionSystem` 只拥有耐久、进水、倾斜、损伤记录、适航状态和维修后的船况投影。
- 损伤命令必须带稳定 `SourceFactId`、来源类别、严重度和量；结构/进水/倾斜有确定性数值裁决。
- 适航等级覆盖 Navigable、Restricted、Disabled、Sinking、Lost；维修区分船上有限维修和船厂完整维修，并校验权限、船员、材料、港口/系泊条件。
- 命令使用保存版本和 fingerprint 做幂等，所有拒绝都保留原 owner 状态。

## Public API / owner 边界

- 主要入口：`CreateHealthyState`、`ValidateState`、`EvaluateSeaworthiness`、`Apply`、`ApplyDamage`、`TryRepair`、`Snapshot`。
- 主要 DTO：`D21ShipConditionState`、`D21ShipDamageEntry`、`D21ShipRepairContext`、`D21ShipConditionCommand`、`D21ShipConditionResult`。
- D4/天气/搁浅等上游只能提供已裁决事实；本系统不读取碰撞或场景对象，也不写组件、港口、货物或船员 owner。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 5 项：损伤应用、重放幂等、船厂维修、缺少来源事实拒绝、完整维修。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L04/D21_L04_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 真实碰撞/搁浅/火灾/天气采样、Rigidbody、浮力、沉没、维修动画、模型、场景、输入、UI、VFX、音频、性能、真机和发行门禁均未执行。

## Blockers / 下一步

- 本项无纯逻辑 blocker。后续适配需把 D4/D9 的公开事实映射到 `ApplyDamage`，不能绕过来源事实直接改耐久。

## Coordinator memory entry

`2026-09-01：D21-L04 已建立事实驱动船况 owner，覆盖耐久/进水/倾斜/适航和维修资格；损伤必须带稳定来源事实，物理与天气采样保持 Runtime/上游适配边界。`
