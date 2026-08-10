# D14-A03 特殊武器与船体模块

状态：已实现代表性纵切片。A03 只编排装备生命周期，不成为 Inventory、Reward、Ship、Combat、Economy 或 Faction/Legal 的第二个写者。

## 1. 内容目录与唯一 ID

D8 没有独立的 `module_` 前缀，因此特殊炮架、火枪、信号器具、捕缆工具和船体模块统一登记为 `item_` 内容 ID。不可变定义位于 `D14SpecialEquipmentCatalog`，内容注册表同步登记来源、授权、历史标签、审查状态和 `test_id`。

代表条目：

| ID | 类型 | 槽位 | 强度上限 | 耐久上限 | 历史/审查 |
|---|---|---|---:|---:|---|
| `item_special_weapon_moonwake_cutlass` | 历史武器 | `DeckArmament` | 72 | 100 | fictional / Approved |
| `item_special_weapon_royal_gun_carriage` | 特殊炮架 | `GunMount` | 58 | 100 | inferred / Approved |
| `item_firearm_matchlock_carbine` | 火枪 | `DeckArmament` | 44 | 80 | historical / ApprovedWithWarning |
| `item_signal_lantern_amber` | 信号器具 | `Signal` | 25 | 90 | inferred / Approved |
| `item_tool_rescue_grapnel` | 捕缆/救援工具 | `Rescue` | 30 | 100 | inferred / Approved |
| `item_ship_module_reinforced_keel` | 独特船体模块 | `Hull` | 35 | 100 | inferred / Approved |
| `item_ship_module_quick_release_rigging` | 独特索具模块 | `Rigging` | 30 | 100 | inferred / Approved |

实例 ID 由 `definition_id + "@" + stable(source_event_id)` 生成。相同定义和来源不能再次生成实例；恢复沿用原实例 ID，不创建副本。

## 2. 所有权和 authority

| 结果 | 唯一裁决/写入者 | A03 行为 |
|---|---|---|
| 获取/奖励 | `RewardSystem`、`CollectionAtlasSystem` 或所属奖励系统 | 要求来源事实、授权事实、Inventory 来源和稳定实例 ID；只保存装备投影 |
| 物品持有/出入库 | `InventorySystem` / `ItemCatalog` | 只接受带 `InventorySourceEventId` 的决定，不改 Cargo 或 PersonalItems |
| 装配/强度/船型兼容 | `ShipSystem` / `CombatSystem` | 只接受所属系统返回的槽位、强度和耐久；A03 二次验证不能越过目录上限 |
| 损坏/遗失 | `CombatSystem` / `ShipSystem` | 只接受战斗或船舶事实；耐久只能下降，`Lost` 由 authority 返回 |
| 维护/维修 | `ShipSystem` / `ShipyardSystem` + Inventory 材料 | 要求维修材料事实，A03 不自行恢复耐久 |
| 出售/价格/账本 | `EconomySystem` + `InventorySystem` | 只接受价格、账本和出库事实，不写金钱或库存 |
| 收缴/证据/合法性 | `FactionLegalSystem` | 要求法律事实，保留 `Seized` 和 `Evidence` 状态 |
| 遗失恢复 | `RewardSystem` / `ItemCatalog` + Inventory | 只允许已记录的 `Lost` 实例，要求恢复来源和授权，保持唯一 ID |

未注入 authority 时，系统明确拒绝需要外部裁决的操作，不生成默认物品、奖励、伤害或耐久。

## 3. 生命周期和确定性边界

公开请求入口为：

- `Acquire`：来源、授权和历史审查通过后获取；
- `Install`：验证船型和槽位，最终强度由 Ship/Combat authority 返回；
- `Maintain`：验证 Inventory 维修材料来源，最终耐久由 Ship/Shipyard authority 返回；
- `Damage`：引用 CombatSystem 事实，接受受损或遗失结果；
- `Sell`：引用法律、Economy 和 Inventory 事实，已装配物品还必须有 ShipSystem 拆卸事实；
- `Seize`：由 FactionLegalSystem 决定，并保留证据状态；
- `Recover`：只恢复 `Lost` 实例，不能绕过收缴或法律释放。

强度与耐久规则是确定性的：

1. 定义给出 `StrengthCap`、`MaxDurability` 和槽位；
2. authority 返回最终数值；
3. A03 拒绝任何越过目录上限、降低维修后耐久、改变唯一实例 ID 或跨船型装配的结果；
4. Agent/UI 提交的 `RequestedStrength`、`RequestedDurability` 只是意图，不会直接成为最终结果。

历史武器至少要求 `historical_tag`、已完成的 `ReviewStatus` 和历史审查事实。`ApprovedWithWarning` 还要求玩家明确确认；未确认时不能获取。

## 4. 事实和存档

成功操作写入 `event_d14_equipment_<operation_id>`，结果值包含实例 ID、定义 ID、操作前后强度/耐久、Inventory/Ship/Combat/Economy/Legal 来源事件和恢复路线。相同 `operation_id + save_revision` 读档后返回 `Duplicate`，不再次调用 authority。

`D14SpecialEquipmentRuntimeSnapshot` 保存：

- 装备实例的定义、所有者、船、槽位、状态、合法性、强度、耐久；
- 获取、授权、历史审查、Inventory 和最近事实引用；
- 全部操作记录、来源事件、账本值、应用键和共享 EventLog；
- `Lost` 的恢复路线和恢复后的同一实例 ID。

SaveSystem 只序列化 `location_json.d14SpecialEquipment`，不裁决装备结果、不写货舱、不改船体或价格。

## 5. 内容与后续接线

内容注册表新增 6 个 D14-A03 代表条目，保留 D12 的 `item_special_weapon_moonwake_cutlass`；检查器结果为 116 entries / 13 prefixes。D14-A04 可消费带来源的奖励获取请求，D14-A05 可消费收缴和许可证事实，D14-S01 可将装备投影与海战、经济和法律快照串联，但不得复制本系统或所属系统状态。

## 6. 验收

`D14SpecialEquipmentTests` 覆盖：

1. 历史武器获取、授权、历史警告确认、稳定唯一 ID；
2. 装配、强度上限、战斗损坏、维修和出售；
3. 收缴后的 `Evidence` 法律状态；
4. 遗失恢复和唯一 ID 保持；
5. SaveSystem 读档和重复出售保护；
6. Agent/UI 不能以请求值越过强度边界。

验证命令和结果：

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error；
- `dotnet run --project tools/d14_a03_runner/D14A03Runner.csproj --no-restore`：`D14-A03 tests PASS count=2`；
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：116 entries / 13 prefixes PASS；
- D14 Systems 红线检查：无网络、HttpClient、UnityWebRequest、LLM。
