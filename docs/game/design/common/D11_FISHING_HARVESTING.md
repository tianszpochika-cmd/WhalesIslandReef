# D11-A02 钓鱼与海上采集

状态：完成（2026-08-11）  
任务：D11-A02  
前置：D9-S02、D11-A01

## 1. 交付范围

本任务扩展了 D9 的单鱼群切片，但没有复制第二套生态、库存或经济系统：

- 捕捞方式：基础手线、岸钓竿、撒网、近岸鱼笼；
- 浅水采集：贝类耙、海草钩，使用 EcologySystem 的资源压力/恢复表；
- 生态输入：鱼种、深度、时间段、潮汐、天气海况和渔具；
- 鱼获：鲜鱼品质、捕捞压力和可追溯的保护区阻断；
- 处理：腌制、鱼干、鱼油、腌贝肉和海草干；
- 市场：所有可出售物品注册给 TradeEconomySystem，出售结果返回价格原因/市场反馈；
- 幂等：捕捞、浅水资源和处理事实均使用 `event_id + save_revision`，读档后不会重复增加鱼获。

本任务不改变既有 D9 鲭鱼价格（`item_fresh_mackerel` 11、`item_salted_mackerel` 17），也不调整战斗或最终经济平衡。新增物品的市场定义只是让 D11 处理品具备可出售的接入基线，仍由 EconomySystem 在运行时生成价格和反馈。

## 2. 单一写者与调用流

| 事实 | 唯一写者 | D11-A02 接口 | UI/Agent 权限 |
|---|---|---|---|
| 鱼群位置、生物量、捕捞压力、恢复、鱼获质量预览 | `EcologySystem` (`D4EcologySystem`) | `SeedFishSchool`、`PreviewFishHarvest`、`CommitFishCatch` | 只能提交采集请求、读取快照 |
| 海草/贝类数量和采集压力 | `EcologySystem` | `EnsureD11ShallowResources`、`HarvestShallowResource` | 只能提交采集请求 |
| 鱼获、采集物、处理品库存 | `D3InventorySystem` | `CanAddCargo`、`TryAddCargo`、`TryRemoveCargo` | 不能直接改货舱字典 |
| 价格、港口市场库存、金币和价格原因 | `TradeEconomySystem` | `RegisterMarketItem`、`Sell` | 只能读取 `TradeResult` |
| 捕捞/采集/处理事实 | 所属 owner 写入后追加 `EventLog` | `event_d11_fishing_harvest`、`event_d11_shallow_resource_harvest`、`event_d11_fish_processing` | 只能读取事件 |
| 保护区规则 | Ecology 边界的 D11 采集入口 | `protected_turtle_cove_no_take` | 不能绕过许可检查 |

`D11FishingHarvestSystem` 只是跨 owner 编排层，不保存第二份资源数量、库存数量或价格。Systems 目录没有网络、LLM 或 Unity 依赖。

## 3. 正式内容 ID

运行时目录在 `game/Assets/Scripts/Systems/D11/D11FishingHarvestCatalog.cs`，内容来源在 `game/Assets/Resources/ContentRegistry/entries/`：

| 类型 | ID |
|---|---|
| 鱼群/鱼种 | `fish_mackerel_school` / `mackerel`、`fish_snapper_school` / `snapper`、`fish_sardine_school` / `sardine` |
| 渔具 | `gear_handline_basic`、`gear_shore_rod`、`gear_cast_net`、`gear_fish_trap`、`gear_shellfish_rake`、`gear_seaweed_hook` |
| 捕捞点 | `nearshore_saint_marys`、`reef_edge_coral_bay`、`sardine_bay` |
| 浅水资源 | `plant_conch_bed`（`shellfish`）、`plant_shallow_seaweed`（`seaweed`） |
| 鲜鱼/采集物 | `item_fresh_mackerel`、`item_fresh_snapper`、`item_fresh_sardine`、`item_conch`、`item_seaweed` |
| 处理品 | `item_salted_*`、`item_dried_*`、`item_fish_oil`、`item_cured_shellfish`、`item_dried_seaweed` |

每个新增内容条目登记 `owner_system`、来源、许可、阶段和 D8 测试 ID；内容门禁当前为 77 entries / 13 prefixes。

## 4. 确定性差异

`D4EcologySystem.PreviewFishHarvest` 只读当前鱼群和天气快照，按以下输入确定性计算预览：

```text
base = stable_hash(fish_school + location + world_day + seed + gear)
quantity = floor(base * pressure * weather * depth * time * tide * gear)
```

实际实现会将数量限制在鱼群当前生物量，并将质量分数限制在 `[0,100]`，投影到统一 `Whalebone.Game.Config.QualityGrade`：`Poor`、`Standard`、`Good`、`Excellent`、`Masterwork`。因此：

- 深度偏离鱼种偏好深度会降低 `DepthMultiplier`；
- 鲭鱼偏日间、笛鲷偏黎明、沙丁鱼偏黄昏；
- 潮汐带按世界日的潮汐相位计算 `TideMultiplier`；
- 撒网、鱼笼、岸钓和手线有明确的容量/深度/渔具系数；
- 既有捕捞压力降低下一次预览，`AdvanceEcology` 按海况恢复鱼群并衰减压力。

数量、质量和压力先由 EcologySystem 预览/提交；库存容量不足时不提交鱼群，防止出现无货可入库却扣除生物量的半结算。

## 5. 处理与市场反馈

`D11FishingHarvestSystem.Process(inputItemId, quantity, method, saveRevision)` 只使用目录中的处理定义：

| 方法 | 输入 | 输出 | 所属写者 |
|---|---|---|---|
| `Salt` | 鲜鲭/笛鲷/沙丁鱼 | 对应盐渍鱼 | `D3InventorySystem` |
| `Dry` | 鲜鲭/笛鲷/沙丁鱼、海草 | 对应鱼干/海草干 | `D3InventorySystem` |
| `RenderOil` | 鲜鱼 | `item_fish_oil`（定义产率） | `D3InventorySystem` |
| `CureShellfish` | `item_conch` | `item_cured_shellfish` | `D3InventorySystem` |

成功处理先扣输入、再加输出；输出空间不足会回滚输入。`Sell` 只把物品、港口、数量和天气交给 `TradeEconomySystem.Sell`，调用方不能自行计算价格或金币。返回的 `TradeResult.PriceReasons` 是市场反馈入口。

## 6. 保护区、事件和读档

`protected_turtle_cove` 使用 `protected_turtle_cove_no_take`。没有许可时不调用资源写入，追加 `event_d11_harvest_rejected_protected`，事件 owner 为 `EcologySystem`，并记录位置、渔具、许可布尔值和规则 ID。

成功鱼获追加 `event_d11_fishing_harvest`，值包括鱼种、物品、数量、深度、时段、潮汐、品质、渔具和 `fishing_pressure_after`；浅水采集同时保留 EcologySystem 的 `event_d11_shallow_resource_harvest`（剩余量/压力）与最终入库事件。处理事实追加 `event_d11_fish_processing`，owner 为 `InventorySystem`。

`D11FishingHarvestRuntimeSnapshot` 保存下一修订号、最后结果和共享 EventLogSnapshot；生态资源仍由 `D4RuntimeSnapshot.Ecology` 保存。恢复同一生态快照和事件日志后，重复请求只返回 `Duplicate=true`，不再增加货舱数量或扣除鱼群生物量。

## 7. 参数登记和合法性检查

捕捞专项配置入口是 `D11FishingHarvestConfiguration`；金额、重量、体积、时间和品质枚举继续使用 D8 `NumericConfiguration`/`QualityGrade`，不在 D11 复制枚举。

| 参数 | 单位 | 默认 | 最小/最大 | owner | 来源 | 测试入口 |
|---|---|---:|---:|---|---|---|
| `d11_harvest_depth_meters` | m | 2 | 0/30 | EcologySystem | D11-A02 深度预览 | `D11A02_FishingHarvestTests` |
| `d11_gear_capacity` | 无量纲 | 2 | 1/20 | EcologySystem | D11-A02 渔具定义 | `D11A02_FishingHarvestTests` |
| `d11_fish_quality_score` | % | 50 | 0/100 | EcologySystem | D11-A02 品质预览 | `D11A02_FishingHarvestTests` |
| `d11_processing_yield_percent` | % | 100 | 1/100 | InventorySystem | D11-A02 处理定义 | `D11A02_FishingHarvestTests` |
| `d11_shallow_resource_capacity` | 无量纲 | 8 | 1/100 | EcologySystem | D11-A02 浅水资源 | `D11A02_FishingHarvestTests` |
| `d11_fish_school_capacity` | 无量纲 | 12 | 1/100 | EcologySystem | D11-A02 鱼群初始化 | `D11A02_FishingHarvestTests` |

启动时 `D11FishingHarvestConfiguration.ThrowIfInvalid()` 检查每项的范围、默认值、owner、来源和测试入口；非法配置会抛出异常。

## 8. 验证入口

```text
dotnet run --project tools/d11_a02_smoke/D11A02Smoke.csproj
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

本任务定向 smoke：

```text
D11-A02 smoke PASS: species/depth/time/tide/gear/shallow/processing/market/protection/idempotency
```

内容目录检查：`Content registry OK: 77 entries, 13 prefixes`。主 Systems 与 Save.Tests 工程均已通过 0 warning / 0 error；代码红线检查确认新增 Systems 没有网络或 LLM 调用。
