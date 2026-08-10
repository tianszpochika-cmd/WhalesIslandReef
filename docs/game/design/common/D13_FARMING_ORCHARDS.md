# D13-A03 农业、果园与蔬菜

任务：`D13-A03`  
日期：2026-08-11  
状态：已实现并通过自动化测试

## 1. 范围

本模块提供地块农业的确定性纵切片：地块/产权资格、种子、土壤、水源、季节、播种、浇水、劳力、病虫害、风暴、成熟度、收获、轮作、加工和销售。

样例地块为 `plot_saint_marys_orchard` 与 `plot_green_cay_cassava_field`，均引用 D13-A01 的 `property_farmland_green_cay_cassava`。作物定义保存在 `D13FarmingCatalog`；运行时 `crop_*` 是农业系统内部内容 ID，货物 ID 继续由 Inventory/Cargo 注册和写入。

## 2. 唯一写者边界

| 数据 | 唯一写者 | 农业系统行为 |
|---|---|---|
| 作物状态、成长点、成熟度、健康、病虫害/风暴损伤、收获次数、轮作索引 | `FarmingSystem` | 唯一写入地块作物状态 |
| 土壤肥力、含水量、水源库存、污染、生态压力 | `EcologySystem` | 只通过 `ID13FarmingEcologyAuthority.Apply` 提交环境操作 |
| 种子、收成、加工品、药剂 | `D3InventorySystem` | 只调用 Inventory/Cargo 的加减货接口，农业系统不直接操作字典 |
| 产权资格、保护区、争议、租赁/共同治理 | `PropertySystem` / 法律势力 authority | `D13FarmingPropertyAuthority` 只读资格决定 |
| 销售单价、总价、货币、经济账本 | `EconomySystem` | `ID13FarmingEconomyAuthority` 返回带来源报价；农业系统不写钱 |
| UI/Agent | UI / Agent | UI 读取 `D13FarmingReadModel`，Agent 只能提出操作请求或解释风险 |

任何缺少产权、法律、生态、库存或经济 authority 的操作都会拒绝并留下失败事实，不使用隐藏默认裁决。

## 3. 作物标签与样例

| 类型 | Crop ID | 种子 | 收成 | 加工品 | 标签示例 |
|---|---|---|---|---|---|
| 果实 | `crop_orchard_guava` | `item_seed_guava` | `item_fruit_guava` | `item_guava_preserve` | `orchard`, `fruit`, `perennial` |
| 蔬菜 | `crop_vegetable_cassava` | `item_seed_cassava` | `item_vegetable_cassava` | `item_cassava_flour` | `vegetable`, `root_crop`, `short_cycle` |
| 甘蔗 | `crop_sugarcane` | `item_seed_sugarcane` | `sugar` | `rum` | `sugarcane`, `plantation`, `stalk` |
| 烟草 | `crop_tobacco` | `item_seed_tobacco` | `tobacco` | `item_tobacco_cured` | `tobacco`, `cash_crop`, `leaf` |
| 咖啡 | `crop_coffee` | `item_seed_coffee` | `coffee` | `item_coffee_roasted` | `coffee`, `shade_crop`, `bean` |
| 可可 | `crop_cocoa` | `item_seed_cocoa` | `cocoa` | `item_cocoa_cured` | `cocoa`, `shade_crop`, `pod` |
| 香料 | `crop_spice` | `item_seed_spice` | `spices` | `item_spices_dried` | `spice`, `drying`, `aromatic` |
| 药草 | `crop_herb` | `item_seed_herb` | `medicine` | `item_herb_prepared` | `herb`, `medicinal`, `small_plot` |

所有作物都要求 `loam` 土壤；样例水源分别是 `water_cistern_saint_marys` 和 `water_well_green_cay`。不同作物拥有独立的成长天数、基础产量、劳力需求、用水、病虫害风险和风暴抗性，标签会写入农业事实和只读结果。

## 4. 确定性规则

1. 世界日以 120 日为一个农业季节周期：`0—59 Dry`、`60—99 Wet`、`100—119 Hurricane`。样例作物只允许 Dry/Wet 播种，Hurricane 播种返回 `season_invalid`。
2. 播种先检查地块为空、产权/法律来源、季节、种子 ID、种子库存和土壤；成功后由 Inventory 扣 1 枚种子，由 Ecology 写入播种的肥力、湿度和生态压力。
3. 浇水每次 1—100 水单位，默认单次配置值为 20；实际用水不能超过登记水源可用量。水量、湿度和水源库存由 Ecology 裁决。
4. 劳力每次最多 10 单位；达到作物劳力需求后，在日推进中增加固定成长点。劳力只进入 FarmingSystem 的作物状态。
5. 每次日推进按固定成长点、湿度、劳力、季节和稳定哈希风险计算。基础为每天 10 点，湿度达到 30 时加 4 点，满足劳力需求时加 3 点；Hurricane 成长减半。成长点达到 `GrowthDays × 10` 后为 `Ready`。
6. 病虫害风险由作物基础风险、生态病虫害压力和稳定哈希（作物 ID、地块 ID、世界日、请求种子）决定；不使用随机服务。病虫害会降低健康并进入 `Infested`，处理剂 `item_farm_pest_treatment` 每单位固定降低 25 压力，过量处理的污染由 Ecology 记录。
7. 风暴损伤为 `max(0, intensity - storm_resistance)`，健康至少下降 1，损伤与肥力后果由 Ecology 同步写入。健康降到 0 或枯萎后不得生成收成。
8. 收获数量由基础产量、地块面积、作物健康和 Ecology 肥力确定；先由 Inventory/Cargo 预检容量并写入收成，之后由 Ecology 扣减肥力/湿度并增加生态压力。任一前置失败都不改变作物和库存。
9. 收获后地块为空；轮作只能在空地进行，Ecology 固定增加肥力、降低部分病虫害压力并记录轮作家族。加工按每作物固定批次转换，例如 2 份木薯收成变 1 份木薯粉；输入/输出均经 Inventory/Cargo。
10. 销售只能销售农业收成或加工品。Economy 必须返回 `EconomySystem` owner、`SourceEventId`、合法单价和 `TotalPrice = UnitPrice × Quantity`；农业系统只扣货、记录销售和账本引用，不写玩家金钱。

## 5. 事实、失败恢复与读档

所有农业结果都经过共享 `EventLog`，事件 owner 为 `FarmingSystem`，生态事件 owner 为 `EcologySystem`。事实至少带地块、作物、标签、输入/输出、环境结果、来源事件和恢复路线。失败包括：

- `route_farming_land_review`：产权资格、保护区或争议问题；
- `route_farming_water_review`：水源为空或浇水参数越界；
- `route_farming_crop_recovery`：季节、成熟度、病虫害或枯萎问题；
- `route_farming_inventory_capacity`：种子、收成、加工品或药剂库存问题；
- `route_farming_ecology_review`：土壤/水源/生态 authority 不可用；
- `route_farming_market_review`：经济报价、来源或销售裁决无效。

`D13FarmingRuntimeSnapshot` 保存地块作物状态、轮作历史、加工记录、销售记录、生态环境、生态应用键和农业事实幂等键，并通过 `SaveSystem.SaveD13FarmingRuntime` 接入现有 `location_json`。读档恢复后，即使共享事实日志尚未重建，保存的 `event_id + save_revision` 键也会拦截重复播种、收获、加工、销售和环境结算。

## 6. 代码与测试

- `game/Assets/Scripts/Systems/D13/D13FarmingContracts.cs`
- `game/Assets/Scripts/Systems/D13/D13FarmingConfiguration.cs`
- `game/Assets/Scripts/Systems/D13/D13FarmingCatalog.cs`
- `game/Assets/Scripts/Systems/D13/D13FarmingSystem.cs`
- `game/Assets/Scripts/Systems/D13/D13FarmingPropertyAuthority.cs`
- `game/Assets/Scripts/Systems/D4/D4EcologyD13Farming.cs`
- `game/Assets/Tests/EditMode/D13FarmingSystemTests.cs`

测试覆盖完整农业循环、8 类作物标签和季节、保护/经济拒绝、库存不变、风暴与病虫害、生态后果、加工销售和 SaveSystem 读档幂等。Systems 代码未加入网络或 LLM。
