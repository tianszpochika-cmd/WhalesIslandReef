# D20-L16 逻辑交接说明

- 任务：`D20-L16`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L16/D20_L16_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcScheduleInput.cs`：

- `D20NpcScheduleInput` 以稳定 `NpcId`、可选 `RoleId`/`LocationRef`、非负 `WorldDay`/`SaveRevision` 和枚举化时段、活动、天气、危险、可用性承载日程输入；
- `IsBusy`、`IsRestRequired`、`IsInjured` 和 `ShelterAvailable` 只是上游 owner 提供的只读事实，不在 D20 中修改岗位、健康或环境；
- `D20NpcScheduleInputContracts.Validate` 拒绝版本不匹配、非法稳定引用、负日/修订、Unknown 枚举和工作窗口缺失活动；
- `IsWorkDuty`、`IsRestDuty`、`IsSevereWeather` 与 `IsAlarmDanger` 提供确定性策略查询；
- `Clone` 和稳定校验结果保证状态机不会持有调用方可变输入引用。

## 2. 后续可消费公开 API

- `D20NpcScheduleInput`、`D20NpcTimeSlot`、`D20NpcDutyKind`、`D20NpcWeatherKind`、`D20NpcDangerLevel`；
- `D20NpcScheduleInputContracts.Validate(...)`、`IsSupportedTimeSlot(...)`、`IsSupportedDuty(...)`、`IsWorkDuty(...)`、`IsRestDuty(...)`、`IsSevereWeather(...)`、`IsAlarmDanger(...)`；
- `D20NpcScheduleInputValidationResult` 与 `D20NpcScheduleInputValidationCode`。

L17 只消费该输入的副本；岗位真实分配、疲劳/健康数值、天气事实和任何业务副作用必须回到既有 owner。未知但格式正确的角色/地点引用不由本契约猜测或替换。

## 3. 验证与未完成项

- D20-L14/L15、D20-L02 与 D20-L16—L19 依赖链使用现代 Roslyn 纯 C# library 编译，`CSC_EXIT=0`、无警告；
- 确定性 smoke 覆盖有效日程、负世界日、未知时段和工作窗口缺少活动，汇合命令共 25 项检查通过；
- 四个目标源文件的禁用引用和尾随空白扫描均为 0；`git diff --check` exit 0（Git 仅提示工作区既有换行转换）；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实日程时钟、寻路、坐标和运行时表现延期。

## Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L16 LOGIC_PASS`；记录 NPC 日程输入的时段/活动/天气/危险边界、只读 owner 事实和未启动 Runtime。
