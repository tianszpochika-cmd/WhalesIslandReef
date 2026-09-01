# D21-L02 逻辑交接

## 任务状态

- 状态：`LOGIC_PASS`。
- 范围：船只身份、所有权、旗帜、注册状态、稳定 ID 和旧值迁移；未宣称运行时、视觉或可玩完成。

## 已完成

- `D21ShipIdentityContracts` 冻结身份契约版本、owner 名称、长度约束和稳定 ID 校验。
- 冻结 20 个目标船型 ID，并保留 `small_fishing -> fishing_boat` 的显式 deprecated mapping。
- `ship_moonwake` 作为 content ID 处理，不创建第二个运行时船型。
- 提供默认身份、迁移结果、校验结果、深复制和确定性 fingerprint；不读取或写入旧 `ShipState` 私有字段。

## Public API / owner 边界

- 主要入口：`D21ShipIdentityContracts.CreateDefault`、`Validate`、`MigrateReference`、`TryMapDeprecatedShipTypeId`、`IsStableId`。
- 主要 DTO：`D21ShipIdentity`、`D21ShipDeprecatedIdMapping`、`D21ShipIdentityMigrationResult`、`D21ShipIdentityValidationResult`。
- owner 仅为身份投影；船况、组件、货物、船员、港口和天气不在此写入。

## 验证

- `/warnaserror+` 独立 C# library 编译通过，XML docs 生成成功。
- 冒烟通过 5 项：默认身份、旧船型迁移、不安全 ID 拒绝、未知引用保持、clone 隔离。
- D21-L02—L08 最终合编通过；可执行行禁用依赖扫描命中 0。
- 结果文件：`game/tools/results/D21/D21-L02/D21_L02_LOGIC_RESULT.json`。

## 未完成 / Runtime Deferred

- 船型模型、Prefab、场景、Transform、实际驾驶/物理、输入、相机、UI、VFX、音频、性能、真机和发行门禁均未执行。
- 与 D20 owner 的运行时身份适配不在本任务范围；不得用兼容 `ShipState` 作为 D21 权威身份。

## Blockers / 下一步

- 本项无纯逻辑 blocker。后续接入时仍须由协调者确认 D20-L40 公开投影和旧模型适配，不得复制 owner。

## Coordinator memory entry

`2026-09-01：D21-L02 已冻结船只身份/所有权/旗帜/注册契约、20 个目标船型 ID 与 small_fishing 显式迁移；ship_moonwake 仅为 content ID，旧 ShipState 只能作为迁移输入。`
