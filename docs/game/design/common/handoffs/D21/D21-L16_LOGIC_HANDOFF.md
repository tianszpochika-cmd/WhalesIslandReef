# D21-L16 逻辑交接

## 已完成

- `D21CourseCommandSystem.cs` 覆盖设定航向、改航、停船和避险命令。
- 命令使用固定 centidegree 量化，并校验航次/船只、Tick/修订、航次激活、适航、船员准备和锚泊前置。
- 成功命令记录冷却和 `command_id` 指纹；重复命令返回 duplicate，不重复写状态；冲突命令被拒绝。

## 验证证据

- 纯 C# 编译：`CSC_EXIT=0`。
- 冒烟：`L16_COMMAND_PASS`。
- 边界：`L16_MOORED_REJECT_PASS`。
- 禁用能力扫描：`FORBIDDEN_API_SCAN=PASS`。

## 依赖与未宣称

- 授权快照必须由后续编排以显式输入提供；本 owner 不读取输入、物理或船况私有状态。
- D20-L40 和 D21-L24-L29 仍是后续跨模块接线门。
- 驾驶输入、相机、物理、Transform、NavMesh、UI、性能、真机和发行验证均 Runtime Deferred。

## Coordinator memory entry

`2026-09-01：D21-L16 已落地设定航向/改航/停船/避险的 owner 裁决、量化、冷却和 command_id 幂等；锚泊、船况、船员和航次授权边界仍由后续聚合显式传入。`
