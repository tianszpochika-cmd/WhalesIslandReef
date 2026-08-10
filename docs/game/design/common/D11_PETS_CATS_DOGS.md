# D11-A06 宠物：猫与犬

## 交付范围

本切片提供两条非战斗宠物内容：`pet_cabin_cat` 与 `pet_harbor_dog`，以及统一饲料 `item_pet_feed`。宠物实例是 D8 冻结的 game-only 数据，不新增 server/web 私有协议。

| 能力 | 裁决系统 | 结果事实 |
|---|---|---|
| 领养、来源与实例 ID | `AnimalCareSystem`（代码入口 `D11PetSystem.Adopt`） | `event_d11_pet_adopted` |
| 健康、饥饿、清洁、恐惧、依恋 | `AnimalCareSystem` | `event_d11_pet_care`、`event_d11_pet_daily_tick` |
| 港口/船舱/甲板位置 | `AnimalCareSystem`（`MoveTo`） | `event_d11_pet_location` |
| 喂养 | Pet 提交扣除请求；`D3InventorySystem` 写饲料数量；Pet 写自身状态 | `event_d11_pet_care` |
| 检疫、治疗、释放 | `AnimalCareSystem` | `event_d11_pet_quarantine`、`event_d11_pet_care` |
| 走失、搜索、恢复 | `AnimalCareSystem` | `event_d11_pet_lost`（Interrupted）、`event_d11_pet_search`（Recovered/未找到） |
| 捕鼠 | `AnimalCareSystem`，仅猫可用 | `event_d11_pet_mouse_caught` |
| 预警 | `AnimalCareSystem`，只记录观察事实 | `event_d11_pet_warning` |
| 船员反应 | 读取船员 Persona，生成结构化表达投影 | `event_d11_pet_crew_reaction` |

## 状态与所有权

`D11PetState` 的唯一写者是 `D11PetSystem`，字段包括：`PetId`、`DefinitionId`、`Kind`、`Breed`、`SourceRef`、`SourceTag`、`AdoptionDay`、`LastUpdatedDay`、`Health`、`Hunger`、`Cleanliness`、`Fear`、`Attachment`、`LifeState`、`Location`、`CareState`、`MouseCaughtCount`、`WarningCount` 以及因果/保存修订号。

货物数量不写入 Pet 状态：喂食前由 `D3InventorySystem.TryRemoveCargo` 扣除 `item_pet_feed`，重复结算时原量退回。Pet 不写钱、价格、奖励、天气、伤害、战斗或船员数值。

UI 只应提交 `Adopt`、`Feed`、`Clean`、`Comfort`、`MoveTo`、`Quarantine`、`Treat`、`ReleaseFromQuarantine`、`MarkLost`、`Search`、`CatchMouse`、`Warn` 请求，并读取 `Snapshot`/`D11PetActionResult`；Agent 只能生成 `ExpressionKey` 对应的表达，不得调用状态写入以外的裁决入口。

## 生命周期与失败恢复

- 领养必须带 `sourceRef`，实例 ID 由定义和来源稳定生成；已有实例拒绝重复领养。
- 宠物不是战斗单位。捕鼠只增加 `MouseCaughtCount`；预警只增加 `WarningCount` 并写入 `observation_only=true`，天气、战斗和航行系统不被修改。
- 走失是 `Interrupted` 事实，搜索未找到时保留原 `PetId` 和 `Lost` 状态；搜索成功通过 `AppendRecovery` 产生 `Recovered` 事实，清除走失标记并回到港口，不生成新实例。
- 检疫至少经过一个游戏日且健康恢复到 70 才能释放；死亡保留 `Dead` 状态和死亡事实，`Adopt` 返回 `pet_dead_no_respawn`，不允许无条件重生。
- 每日推进按确定性规则消耗照料状态；日期倒退、非法位置、无饲料、错误宠物类型和不可用生命周期都会拒绝。

## 存档

`D11PetRuntimeSnapshot` 版本为 `schema_version=1`，写入现有 `location_json.d11Pet`。快照包含宠物、船员结构化反应和共享 `EventLogSnapshot`；`SaveSystem` 只复制/序列化，不计算宠物结果。D7/D8 旧档没有 `d11Pet` 时按空快照兼容读取。

## 内容入口

- `game/Assets/Resources/ContentRegistry/entries/pet/pet_cabin_cat.json`
- `game/Assets/Resources/ContentRegistry/entries/pet/pet_harbor_dog.json`
- `game/Assets/Resources/ContentRegistry/entries/item/item_pet_feed.json`
- 运行时目录：`game/Assets/Scripts/Systems/D11/D11PetContracts.cs`、`D11PetCatalog.cs`、`D11PetSystem.cs`

## 测试边界

`game/Assets/Tests/EditMode/D11PetSystemTests.cs` 覆盖来源与领养、库存扣料及幂等、位置、检疫治疗释放、走失搜索恢复、捕鼠/预警观察事实、存档读回、死亡后不可重生。独立执行入口为 `tools/d11_a06_runner`。

