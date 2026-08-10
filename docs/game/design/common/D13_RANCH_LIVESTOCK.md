# D13-A04 牧场、家禽与牲畜

任务：`D13-A04`  
日期：2026-08-11  
状态：已实现并通过自动化测试

## 1. 边界与唯一写者

`D13RanchSystem` 是家禽/牲畜实例的唯一写者，负责来源登记后的健康、疾病、检疫生命周期、劳力投入、单周期状态和待领取产物。它不写金钱、产权、水源、饲料或货舱数量。

| 状态 | 唯一写者 | 牧场如何接入 |
|---|---|---|
| 动物健康、疾病、检疫、周期、死亡、待产物 | `D13RanchSystem` | 只通过明确的操作请求结算 |
| 饲料、药剂、蛋/奶/肉/皮 | `D3InventorySystem` | Feed/Treat/Collect 先由货舱扣除或写入 |
| 水源剩余量与恢复 | `D4EcologySystem` | Ranch 只提交 `ConsumeWater` |
| 地块/设施经营资格、产权和法律来源 | `PropertySystem` / 法律 authority | 通过 `ID13RanchLandAuthority` 只读裁决 |
| 产品价格、市场许可和账本引用 | `EconomySystem` | 通过 `ID13RanchEconomyAuthority` 只读裁决 |
| 来源和检疫事实 | `AnimalCareSystem`/来源 authority | 来源事件和检疫事件必须先进入共享 `EventLog` |

`D13RanchPropertyAuthority` 复用 D13-A01 的农业产权适配器，不复制产权状态或交易逻辑。UI 只读取 `D13RanchReadModel` 和事实事件；Agent 没有动物、疾病、繁殖或销售裁决入口。

## 2. 来源、检疫与设施

四个目录定义分别是 `ranch_poultry_chicken`、`ranch_livestock_goat`、`ranch_livestock_pig`、`ranch_livestock_cattle`，对应内容 ID `animal_domestic_chicken`、`animal_domestic_goat`、`animal_domestic_pig`、`animal_domestic_cattle`。实例只能由 `Acquire` 创建，必须同时满足：

1. 产权/法律 authority 允许设施经营，并能在共享事件日志找到来源事实；
2. 来源 authority 返回允许、检疫通过、正确 `SourceContentId`、`AnimalCareSystem` owner 和检疫天数；
3. 来源事件、检疫事件都存在，且不能由错误 owner 或 UI/Agent 伪造；
4. 设施有足够空间和 Ecology 水源状态。

新实例进入 `Quarantined`，默认健康为 100；未到 `QuarantineUntilDay` 不能释放。释放前疾病必须为 `None` 且健康不低于 70。相同 `SourceRef` 产生稳定实例 ID，重复来源不会生成第二只动物。设施包括家禽棚和小牧场，目录固定空间、水源、市场和标签。

## 3. 照料与单周期产出

每种动物有独立的饲料量、饮水量、空间、劳力、疾病风险、周期产量和产品标签：

| 定义 | 周期产物 | 产量 | 饲料 | 饮水 | 空间 | 劳力 | 疾病风险 |
|---|---|---:|---:|---:|---:|---:|---:|
| 家鸡 | `item_chicken_egg` / `Egg` | 2 | 1 | 2 | 1 | 1 | 12% |
| 山羊 | `item_goat_milk` / `Milk` | 2 | 2 | 4 | 2 | 2 | 18% |
| 家猪 | `item_pig_meat` / `Meat` | 1 | 4 | 4 | 2 | 2 | 20% |
| 牛 | `item_cattle_leather` / `Leather` | 1 | 5 | 6 | 3 | 3 | 16% |

`Feed` 从 `item_livestock_feed` 扣货，`TreatDisease` 从 `item_livestock_medicine` 扣货；`Water` 由 Ecology 扣登记水源，RanchSystem 只增加该动物的照料饮水计数；`AssignLabor` 记录本周期劳力。

`AdvanceCycle` 必须通过饲料、饮水、劳力和空间门槛。所有输入满足时，使用定义 ID、实例 ID、世界日和固定 seed 做 FNV-1a 稳定疾病检查；疾病风险、病种、健康损失均可复算。通过后只进入 `ProductReady`，产品不会自动进入货舱；`CollectProduct` 才由 Inventory/Cargo 写入货舱并清空待领取状态。货舱满时待产物保留，不丢失也不重复生成。

缺少每类输入会按配置减少健康并写失败事实；健康降为 0 时永久保留 `Dead` 记录，不治疗、不重生、不生成产物。患病必须使用登记治疗剂恢复后再推进周期。产品销售只消费 EconomySystem 的单价、总价、市场和账本裁决，RanchSystem 不直接写玩家金钱。

## 4. 繁殖、交易与事件

本版本 `BreedingMode=Disabled`，`RequestBreeding` 只写 `breeding_deferred` 失败事实和恢复路线，不创建子代、不复制动物状态，控制繁殖复杂度并保留后续专门设计入口。

所有成功和失败操作都写入 `EventLog`，带 `owner_system=RanchSystem`、结果码、来源/因果引用和恢复路线。销售记录只保存产品、数量、价格、货币、市场、账本引用、来源事件、日期和存档版本；实际账务由 EconomySystem 负责。

## 5. 保存与幂等

`D13RanchRuntimeSnapshot` 保存：

- 动物来源、检疫、健康、疾病、周期、劳力、待产物、死亡和最后事件；
- 销售记录；
- Ecology 牧场水源和 Ecology 自己的应用键；
- Ranch 操作应用键。

SaveSystem 通过 `location_json.d13Ranch` 编解码；旧存档缺少该字段时按空快照兼容。所有操作使用 `event_id + save_revision`，读档后即使共享事实尚未重建，快照应用键也会拦截重复来源、饮水、产物和销售，避免重复添加库存、重复扣水或重复结算。

## 6. 验证

自动化测试位于 `game/Assets/Tests/EditMode/D13RanchSystemTests.cs`，独立运行器为 `tools/d13_a04_runner`，共 3 项：

- 家鸡来源→隔离→照料→单周期产蛋→库存→销售→保存/读档/重复销售；
- 山羊确定性患病、药剂治疗、恢复产出，以及缺少输入导致可追踪死亡；
- 设施空间上限、产权拒绝、市场拒绝和繁殖延后边界。

动物行为无 LLM、无网络；Systems 红线继续适用。
