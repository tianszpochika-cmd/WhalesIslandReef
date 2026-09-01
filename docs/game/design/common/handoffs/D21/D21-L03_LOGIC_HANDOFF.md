# D21-L03 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：船体组件安装/移除/启用状态、动力组合和组合不变式；未宣称 Unity 运行时组件完成。

## 已完成

- `D21HullComponentSystem` 作为组件状态唯一写者，支持 install、remove 和 operational-state 命令。
- 冻结 Hull、Mast、Sail、Rigging、Rudder、Anchor、CargoHold、OarDrive 等逻辑类别。
- 校验船体/舵/锚等必需组件、动力模式、重复 ID、数量边界、依赖、冲突和船型兼容性。
- 命令带保存版本、事件 ID 和确定性 fingerprint；相同命令重放返回 duplicate，失败不改变 owner 状态。

## Public API / owner 边界

- 主要入口：`CreateStarterConfiguration`、`ValidateConfiguration`、`Apply`、`Install`、`Remove`、`Snapshot`。
- 主要 DTO：`D21HullComponentConfiguration`、`D21HullComponentDefinition`、`D21HullComponentState`、`D21HullComponentCommand`、`D21HullComponentResult`。
- 船型内容、船况耐久、D3 货物数量、港口泊位和物理组件不由本 owner 写入。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 5 项：starter 校验、显式安装、命令重放幂等、锚缺失拒绝、快照隔离。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L03/D21_L03_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 模型、Prefab、Collider、场景、Transform、Rigidbody、浮力、实际航行、输入、相机、UI、VFX、音频、性能、真机和发行门禁均未执行。

## Blockers / 下一步

- 本项无纯逻辑 blocker。后续必须由适配层把外部组件事实转换为公开命令，不得让 D4 或表现层直接改本状态。

## Coordinator memory entry

`2026-09-01：D21-L03 已建立船体组件唯一写者和确定性组合校验；组件安装/移除/启用均经版本化命令，物理组件、船况和货物数量仍归其他 owner。`
