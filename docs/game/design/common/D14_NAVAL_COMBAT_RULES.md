# D14-A01 海战裁决规则

状态：已实现，D14-A01。所有规则在 `CombatSystem`（`D14NavalCombatSystem`）内确定性裁决；UI 和 Agent 只能提交 request、读取快照与事实。

## 1. 权限与数据所有权

| 数据 | 唯一写者 | D14 的使用方式 |
|---|---|---|
| 命中概率、命中类型、分区损伤、火灾、进水、战斗状态 | `CombatSystem` / `D14NavalCombatSystem` | 本模块唯一写入 D14 战斗快照 |
| 船型/船体/船员/炮位初始读模型 | `ShipSystem` 提供的带来源输入 | `D14CombatShipProfile` 只读拷贝，不修改 D4 `ShipState` |
| 风向、风速、能见度、海况风险 | `EnvironmentSystem` | 读取 `D9WeatherSeaSnapshot`，要求存在 `WeatherSourceEventId` |
| 库存、现金、资产、法律和奖励 | 各自所属系统 | D14 只记录来源事实，后续系统再裁决后果 |
| 战斗事实日志 | 共享 `EventLog` | `CombatSystem` 通过统一 `WorldEvent` 追加，SaveSystem 只复制/恢复 |

这一层保留 D4 简化战斗兼容，不覆盖旧 `D4CombatSystem`，也不建立第二份 Inventory、Economy、Property 或 ShipState 写者。D14 快照中的 `D14HullDamageState` 是 CombatSystem-owned 的战斗投影；若后续要把结果映射到航行中的实体船体，应由 ShipSystem 通过带来源 authority 接收战斗事实。

## 2. 战斗输入与状态

接战必须同时提供：

- 双方船舶 `ShipId/HullMax/Crew/Speed/Heading/GunnerSkill/ManeuverSkill` 和炮位；
- `Distance`、`RelativeBearingDegrees`、双方航向和航速；
- D9 天气快照及已存在的 `EnvironmentSystem` 来源事件；
- `OperationId + SaveRevision`，用于事实去重。

每个炮位保存炮位位置（艏/艉/左舷/右舷及中段）、三类弹药数量、当前装填弹种、剩余装填回合、炮况、瞄准角度和目标分区。每艘船保存：

- 船体耐久、船员；
- 桅帆完整度、舵完整度、可用炮数；
- 火灾等级、进水等级、火药风险和倾斜角；
- 当前战斗状态、回合、天气来源、最后事实事件。

## 3. 固定弹种与分区

| 弹种 | 基准距离 | 装填 | 默认目标 | 主要后果 |
|---|---:|---:|---|---|
| RoundShot | 150 | 3 回合 | Hull | 船体 18 点，可能引发火灾或进水 |
| ChainShot | 100 | 2 回合 | Mast | 桅帆完整度下降并降低航速 |
| GrapeShot | 35 | 1 回合 | Crew | 近距离杀伤船员；超过 80 距离有固定惩罚 |

玩家可以在瞄准请求中指定 `Hull/Gun/Mast/Rudder/Crew/Powder`；`Auto` 按弹种使用默认分区。命中类型会记录为 `Miss/Graze/Hull/Gun/Sail/Rudder/Crew/Fire/Flooding/PowderRisk`。炮位被禁用、没有弹药、尚未完成装填或未完成瞄准时，开火请求拒绝且不改状态。

## 4. 确定性命中公式

先计算：

```text
alignment = 1 - clamp(abs(signed_angle(aim_bearing, relative_bearing)) / 90, 0, 1)
range     = 1 - clamp(abs(distance - ideal_distance) / ideal_distance, 0, 1)
skill     = gunner_skill / 100 * 0.16
wind_pen  = min(0.24, crosswind * wind_speed / 24 * 0.28)
speed_pen = clamp(abs(player_speed - enemy_speed) / 14, 0, 1) * 0.10

chance = clamp(
  0.12 + alignment * 0.34 + range * 0.25 + visibility * 0.12
  + skill + (1 - speed_pen) * 0.07 - wind_pen - grape_long_range_penalty,
  0.03, 0.94)
```

`crosswind` 是风向相对玩家航向的正弦绝对值；`grape_long_range_penalty` 在霰弹距离大于 80 时固定为 0.25，否则为 0。命中比较值不是运行时随机数，而是 FNV-1a 稳定值：

```text
hash(encounter_id | turn | gun_id | ammo | target_zone) % 10000 / 10000
```

因此同一存档、同一操作号和同一战斗输入必得同一结果。命中值低于 `chance * 0.16` 时为重击，基准伤害乘 1.6；船体命中还用独立稳定值判断火灾/进水。炮位和弹药的变化在命中前后均由 CombatSystem 写入，不能由 UI 或 Agent 预填最终结果。

## 5. 火灾、进水和状态推进

- 船体命中后，稳定值小于 `0.35`（重击小于 `0.60`）增加火灾等级；稳定值在 `[0.52, 0.84)` 增加进水等级。
- 每推进一回合，火灾按 `max(1, fire / 15)` 扣船体，并降低 4 点火灾；进水按 `max(1, flooding / 20)` 扣船体，并增加 2 点进水，直至由后续灭火/损管系统处理。
- 船体归零或进水达到 100 为 `Sinking`；敌船船体归零、进水达到 100 或船员归零为 `Victory`。
- 回合推进同时减少每个炮位的装填回合，不会自动生成弹药，也不会自动恢复船体。

## 6. 撤退、投降与恢复

撤退机会由距离、相对航速、舵完整度和天气风险固定计算：

```text
chance = clamp(0.22 + distance/260*0.32
               + clamp((player_speed-enemy_speed+4)/12,0,1)*0.28
               + rudder_integrity/100*0.18 - hull_risk*0.18, 0, 1)
```

将该机会与 `hash(encounter_id | retreat | turn)` 比较；机会达到 0.90 时保证脱离。成功进入 `Escaping`，失败扣 5 点船体、增加 4 点进水，并写入 `Failed + recovery_route` 事实。恢复路线为修复/拉开距离后使用新操作号重新接战。

投降请求由 CombatSystem 接受后进入 `Surrendered`，只记录对手和当前船体事实；俘虏、物资、声望、法律和奖励留给 D14 后续所属系统，Agent 不裁决。

## 7. 事件与存档

公开入口为：

- `StartEncounter`
- `LoadGun`
- `Aim`
- `Fire`
- `AdvanceRound`
- `AttemptRetreat`
- `Surrender`

每个成功操作写入 `event_d14_naval_<operation_id>`，`owner_system=CombatSystem`，包含距离、角度、风向、风速、命中、伤害、弹种、分区、回合和来源事实。所有操作按 `event_id + save_revision` 幂等；读档重放已有操作只返回 `duplicate`，不再次扣弹、改船体或追加日志。

`D14NavalCombatRuntimeSnapshot` 通过 `location_json.d14` 保存：战斗状态、炮位、分区损伤、天气快照、应用键、战斗事件 ID 和共享 `EventLogSnapshot`。`PlayerState`/`GameState` 只保存快照引用，`SaveSystem` 不计算命中或损伤。

## 8. 验收覆盖

`D14NavalCombatTests` 验证：

1. 距离/风向/角度/航速/炮位/装填/圆弹命中，船体、火灾、进水和读档恢复；
2. 读档后重复开火只返回 duplicate；
3. 链弹自动选择桅帆分区；
4. 高速远距撤退进入 `Escaping`；
5. 投降进入 `Surrendered` 并写入事实。

Systems、Save.Tests 和 D14 独立 runner 均无编译错误，D14-A01 runner 2/2 通过。
