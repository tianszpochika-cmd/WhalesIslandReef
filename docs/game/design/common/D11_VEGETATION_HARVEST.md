# D11-A07 植被采集与水果资源

状态：已实现代表性闭环（2026-08-11）  
前置：D9-S02 PASS、D11-A01 完成

本任务把植被节点、识别、成熟度、工具、保护区和库存接入同一条确定性采集链。它扩展现有 D4 `D4EcologySystem`，没有创建第二套生态系统，也没有让 UI 或 Agent 成为资源裁决者。

## 1. 唯一写者和调用边界

| 数据 | 唯一写者 | D11-A07 入口 | 说明 |
|---|---|---|---|
| 植被数量、采集压力、恢复 | `EcologySystem`（现有 `D4EcologySystem`） | `HarvestPlant`、`AdvanceEcology`、`RestoreHabitat` | 数量由生态状态写入；采集系统不能提交最终资源量 |
| 货舱物品数量 | `D3InventorySystem` | `CanAddCargo`、`TryAddCargo`、`TryRemoveCargo` | 产物使用 D3 的容量/重量规则 |
| 工具、识别、成熟度、保护区规则 | `D11VegetationHarvestSystem` | `Identify`、`Harvest` | 规则是确定性计算，不持有库存/生态可写引用 |
| 事实事件 | 领域 owner 的共享 `EventLog` | Ecology `plant_harvested` | 事件载荷记录工具、成熟度、识别和后果；不替代状态写入 |
| UI / Agent | 只读或提议 | 提交请求、读取结果/事实 | 不得携带最终数量、毒性结算或资源修改 |

核心代码：

- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestCatalog.cs`
- `game/Assets/Scripts/Systems/D11/D11VegetationHarvestSystem.cs`
- `game/Assets/Scripts/Systems/D4/D4EcologyD11StateMachine.cs`
- `game/Assets/Scripts/Systems/D3/D3InventorySystem.cs`

## 2. 代表资源和产物

| 类别 | 植物 ID | 生境 | 工具 | 产物 | 特殊规则 |
|---|---|---|---|---|---|
| 果实 | `plant_orchard_guava_v1` | coast | `tool_vegetation_basket` | `item_fruit_guava` | 成熟/过熟影响品质和产量 |
| 蔬果 | `plant_farm_cassava_mature` | farm | `tool_vegetation_harvesting_knife` | `item_vegetable_cassava` | 生长期拒绝采集 |
| 药材 | `plant_dry_aloe_medicinal` | dry_shrub | `tool_vegetation_herb_knife` | `medicine` | 识别门槛高于普通果实 |
| 纤维 | `plant_coast_agave_fiber` | coast | `tool_vegetation_machete` | `item_fiber_agave` | 采集压力进入生态恢复 |
| 木材 | `plant_rainforest_mahogany_v1` | rainforest | `tool_vegetation_axe` | `timber` | 恢复慢；沿用 D9/D3 木材兼容商品 |
| 树脂 | `plant_rainforest_pine_resin_v1` | rainforest | `tool_vegetation_resin_scraper` | `item_resin_pine` | 刮刀与容器视为一个工具定义 |
| 染料 | `plant_farm_indigo_mature` | farm | `tool_vegetation_harvesting_knife` | `dyes` | 沿用现有经济商品链 |
| 毒物 | `plant_dry_datura_toxin_v1` | dry_shrub | `tool_vegetation_herb_knife` | `item_toxin_datura` | 误采记录 `plant_poisoning` |
| 保护样例 | `plant_red_mangrove` | mangrove | `tool_vegetation_axe` | `timber` | 物种保护；保护区内一律拒绝 |

新增/补齐的 D8 内容条目位于 `game/Assets/Resources/ContentRegistry/entries/plant/` 和 `entries/item/`。`plant_red_mangrove`、浅水海草和贝床等既有 D11 条目继续保留，目录检查通过：77 entries / 13 prefixes。

## 3. 确定性规则

### 3.1 识别

`Identify` 使用：

```text
knowledge = max(botany_skill, 0) + field_guide?1:0 + crew_advice?1:0
```

当 `knowledge >= minimum_identification_skill` 时确认实际植物；否则依据 `plant_id + day + seed` 的稳定哈希选择同生境的形态近似物，返回 `misidentified`。不使用运行时随机数。

`Harvest` 默认拒绝未确认物种；只有请求明确 `AllowUnidentifiedHarvest=true` 才能继续，这个选择会进入结果和 Ecology 事实。

### 3.2 成熟度和品质

每个资源定义自己的周期、成熟起点和成熟持续天数。成熟阶段由：

```text
phase = (max(day, 0) + stable(plant_id + seed)) mod cycle_days
```

映射为 `Seedling → Growing → Mature → Overripe`。幼苗/生长期拒绝采集；成熟期正常产出；过熟期至少可收取 1 单位但品质降为 `Poor`。误采也降为 `Poor`。

基础产量由资源定义的 `BaseYield` 和 Ecology 本次实际扣除的资源数量计算，不能由 UI、Agent 或结果文本传入。

### 3.3 工具

工具 ID 必须与资源定义的 `RequiredToolId` 完全匹配。错误工具在任何资源数量扣减和库存写入前返回 `tool_required`。当前工具覆盖采集篮、采集小刀、药草小刀、砍刀、短柄斧、树脂刮刀与容器。

### 3.4 保护区和误采

保护区定义是静态内容规则：

- `protected_mangrove_saint_marys`：红树林生境禁止全部采集；
- `protected_community_grove_saint_marys`：雨林生境禁止木材采集；
- 红树样例本身标记为受保护物种，无合法许可时拒绝采集。

误采允许后，仍只产出实际节点对应的物品，不会把错误识别文本变成另一种资源。毒物会产出 `item_toxin_datura`，结果和事实同时带有：`misidentified=true`、`toxic=true`、`consequence_id=plant_poisoning`。后续医疗/船员系统可读取这一事实处理健康后果；D11-A07 不越权写健康数值。

## 4. 结算顺序和失败恢复

1. 检查请求、目录 ID、生境节点和 `event_id + save_revision` 去重；
2. 识别、保护区、物种保护、工具和成熟度检查；
3. 用 `D3InventorySystem.CanAddCargo` 检查容量和重量；
4. 由 `D3InventorySystem.TryAddCargo` 写入产物；
5. 调用 `D4EcologySystem.HarvestPlant` 扣资源、增加压力并追加 `plant_harvested` 事实；
6. Ecology 提交失败时移除第 4 步已写入的库存，返回失败，不留下半个结算；
7. 结果只读取事实和库存快照，供 UI 显示。

库存写入前的容量检查、提交后的生态冲突回滚和共享事件去重，保证失败不会凭空生成资源。保护区、工具、未成熟和未识别失败均不改资源量。

## 5. 恢复、保存和读档

资源恢复不在采集系统中复制：`D4EcologySystem.AdvanceEcology` 按已有 `PlantResourceState.RecoveryRatePerDay`、天气/海况和压力恢复资源；`RestoreHabitat` 提供显式恢复入口。

存档继续使用已有 D3/D4 快照：

- D3 `Cargo` 保存采集产物；
- D4 `Ecology.Plants` 保存数量、容量、压力、恢复率和最后更新日；
- D4 `AppliedEcologyEventKeys` 与共享 `EventLogSnapshot` 保存 `event_id + save_revision`；
- 读档后新建采集系统并恢复 Ecology/EventLog，再次提交同一请求只返回 `duplicate_settlement`，不会重复增加货舱。

本任务没有新增第二个采集存档或 UI 状态写者。

## 6. 测试和验收

Unity EditMode：`game/Assets/Tests/EditMode/D11VegetationHarvestTests.cs`，覆盖：

- 8 类资源和工具目录；
- 识别确定性；
- 错误工具无资源/库存副作用；
- 各类资源的成熟度和工作工具路径；
- 毒物误采、库存产物和事实后果；
- 保护区拒绝与资源不变；
- Ecology/EventLog 快照恢复后的重复保护。

已执行：

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162   # 0 warning / 0 error
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162          # 0 warning / 0 error
dotnet run --project tools/d11_a07_smoke/D11A07Smoke.csproj --no-build             # PASS
dotnet run --project tools/d11_a07_runner/D11A07Runner.csproj --no-build           # D11-A07 tests PASS count=6
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry       # 77 entries / 13 prefixes
```

独立烟测不依赖 Unity 场景，专门验证确定性识别、成熟采集、毒物/保护区和恢复后的重复保护；Unity 测试程序集已完成编译并登记 EditMode 入口。
