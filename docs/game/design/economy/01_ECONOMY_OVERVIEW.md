# 经济系统总览

## 1. 经济循环

生产 → 运输 → 市场 → 消费 → 税费 → 工资 → 维护 → 再投资。

玩家可以选择：

- 小规模高频贸易；
- 大宗货运；
- 高风险违禁品；
- 护航和私掠；
- 船坞和工匠服务；
- 资源采集；
- 金融和债务；
- 势力特许。

## 2. 经济数据所有权

| 数据 | 写者 |
|---|---|
| 商品基础价 | EconomySystem |
| 港口库存 | MarketSystem |
| 供需变化 | MarketSystem |
| 关税 | FactionSystem |
| 钱和交易 | EconomySystem |
| 船员工资 | Crew/EconomySystem |
| 船坞费用 | ShipyardSystem |
| 债务和信用 | FinanceSystem |
| 任务奖励 | EventSystem 提议，EconomySystem 裁决 |

## 3. 商品字段

- commodity_id；
- base_price；
- volume；
- weight；
- perishability；
- legal_status；
- origin_regions；
- destination_tags；
- production_cycle；
- demand_tags；
- price_floor；
- price_ceiling；
- tariff_profile；
- risk_profile。

## 4. 经济风险

- 价格波动；
- 船体损伤；
- 货物受潮；
- 海盗和检查；
- 关税；
- 债务；
- 保险；
- 港口封锁；
- 资源枯竭；
- 船员工资；
- 船坞停航。

## 5. 透明度

玩家要能看到：

- 价格变化原因；
- 港口库存；
- 预计利润；
- 运输风险；
- 税费；
- 维护费；
- 债务和利息；
- 交易后果。
