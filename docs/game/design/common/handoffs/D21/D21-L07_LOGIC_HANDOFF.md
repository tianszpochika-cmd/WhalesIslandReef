# D21-L07 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：船侧锚泊、拖锚风险和系泊状态机；港口/泊位 owner 不在本任务内。

## 已完成

- `D21AnchorMooringSystem` 支持下锚、完成下锚、重新评估抓持、走锚、收锚和船侧系泊转换。
- 根据水深、海床、风流、波浪、锚链、抓持能力、船重和吃水计算确定性 `DragRisk`。
- 系泊只接受 D19 提供的 `PortId`、`BerthId` 和 `BerthPermissionGranted`，不写 D19 港口或泊位状态。
- 命令带保存版本、事件 ID 和 fingerprint，支持重放幂等并保护非法转换。

## Public API / owner 边界

- 主要入口：`ComputeDragRisk`、`ValidateState`、`Apply`、`Snapshot`。
- 主要 DTO：`D21AnchorEnvironmentSnapshot`、`D21AnchorMooringState`、`D21AnchorMooringCommand`、`D21AnchorMooringResult`。
- D19 只提供公开许可/引用输入；真实缆绳、锚链、泊位空间和物理位姿属于 Runtime/适配层。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 5 项：下锚、稳定抓持、走锚风险、收锚、D19 许可系泊与幂等重放。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L07/D21_L07_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 真实锚链/缆绳、靠岸、碰撞、浮力、漂移、驾驶、场景/Prefab、输入、UI、VFX、音频、性能、真机和发行门禁均未执行。

## Blockers / 下一步

- 本项无纯逻辑 blocker。接入 D19 时必须传递其公开 permission/result，并由运行时适配层负责空间和表现反馈。

## Coordinator memory entry

`2026-09-01：D21-L07 已建立船侧锚泊/系泊状态机与确定性拖锚风险；D19 仅通过公开港口/泊位 ID 和许可接入，D21 不写港口 owner。`
