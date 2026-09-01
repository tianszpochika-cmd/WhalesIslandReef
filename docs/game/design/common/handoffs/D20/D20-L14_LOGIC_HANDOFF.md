# D20-L14 逻辑交接说明

- 任务：`D20-L14`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L14/D20_L14_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcIdentityContracts.cs`：

- `D20NpcIdentity` 提供 `NpcId`、`RoleId`、`FactionId`、`ContentId`、`Availability`、来源 ID 和迁移状态字段；
- 复用 D20-L02 的稳定角色 ID 与可用性契约，身份契约只校验格式/版本，不把视觉资源当作身份事实；
- `DeprecatedNpcIdMappings` 为只读映射，包含已登记的历史 NPC 键到 canonical ID 的迁移关系；
- `Migrate(...)` 保留 `SourceNpcId`，区分 `Canonical`、`DeprecatedAlias`、`UnknownPreserved` 和 `Unresolved`；未知但格式正确的 ID 不会被猜测归并；
- `Validate()` 与 `IsReadyForRuntime(...)` 为未解析/版本不匹配/非法可用性提供明确门禁；
- 契约不注册、创建或修改 NPC owner 状态。

## 2. 后续可消费公开 API

- `D20NpcIdentityContracts.Migrate(...)`；
- `D20NpcIdentityContracts.TryResolveNpcId(...)`、`IsDeprecatedNpcId(...)`、`IsReadyForRuntime(...)`；
- `D20NpcIdentity`；
- `D20NpcIdentityMigrationStatus`；
- `D20NpcIdentityContracts.DeprecatedNpcIdMappings`。

未知值应继续沿用 `SourceNpcId` 与 `UnknownPreserved/Unresolved` 语义，不能在聚合层静默替换。

## 3. 验证与未完成项

- D20-L14/L15 与 D20-L02 的纯 C# 编译：Roslyn `/warnaserror+`，exit 0、0 警告；
- smoke 覆盖已登记弃用别名、未知 ID 保留和非法 ID runtime-ready 门禁；
- 目标源文件静态边界检查：无 Unity/network/LLM/time/random/owner mutation 引用，尾随空白为 0；
- 未启动 Player、PlayMode、场景、输入、动画或设备；真实 NPC owner 注册、保存和迁移执行延期。

## 4. Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L14 LOGIC_PASS`；记录 NPC 身份字段、只读弃用映射、未知值保留和未启动 Runtime。
