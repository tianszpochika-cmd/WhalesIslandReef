# D20-L15 逻辑交接说明

- 任务：`D20-L15`
- 日期：2026-09-01
- 结论：`LOGIC_PASS`
- 结果：`game/tools/results/D20/D20-L15/D20_L15_LOGIC_RESULT.json`

## 1. 改动

新增 `game/Assets/Scripts/Systems/D20/D20NpcRoleCatalog.cs`：

- 建立 merchant、dockhand、tavern、shipyard、guard、harbor_clerk、lookout、fisher 岗位键；
- 每个岗位只包含本岗位的显示键、语义 role variant 引用、owner 可解释的建议 duty 引用和能力标签；
- `All`、`Find`、`TryGetRole` 均返回防御性副本，`ValidateCatalog()` 检查稳定键、重复项、能力位和引用格式；
- `Resolve(...)`/`TryResolveRoleId(...)` 保留未知岗位，显式区分 canonical、deprecated alias 和 unknown preserved；
- 目录不保存 NPC 当前可用性、岗位分配、日程、工资、关系、库存或视觉对象，也不改变任何 owner 状态。

## 2. 后续可消费公开 API

- `D20NpcRoleCatalog.All`、`Find(...)`、`TryGetRole(...)`、`IsKnownRole(...)`；
- `D20NpcRoleCatalog.Resolve(...)`、`TryResolveRoleId(...)`、`ValidateCatalog()`；
- `D20NpcRoleDefinition`、`D20NpcRoleResolution`、`D20NpcRoleCapability`；
- 角色键常量：`MerchantRoleId`、`DockhandRoleId`、`TavernRoleId`、`ShipyardRoleId`、`GuardRoleId` 等。

岗位目录结果只作为 D20/NPC 聚合层的逻辑元数据；岗位分配和行为事实必须回到对应 owner。

## 3. 验证与未完成项

- D20-L14/L15 与 D20-L02 的纯 C# 编译：Roslyn `/warnaserror+`，exit 0、0 警告；
- smoke 覆盖目录完整性、商人能力、弃用岗位迁移、未知岗位保留和防御性副本；
- 目标源文件静态边界检查：无 Unity/network/LLM/time/random/owner mutation 引用，尾随空白为 0；
- 未启动 Player、PlayMode、场景、输入、动画或设备；岗位真实分配、行为推进和表现消费延期。

## 4. Coordinator memory entry

协调器在批次汇合时统一更新共享状态和当日日志：`D20-L15 LOGIC_PASS`；记录 NPC 岗位目录覆盖、只读能力标签、弃用/未知岗位迁移语义和未启动 Runtime。
