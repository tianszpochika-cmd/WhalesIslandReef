# 道具与资源总览

## 1. 数量目标

- 200 个功能道具；
- 300 个装饰道具模型或变体；
- 商品、材料、工具、武器、补给、收藏品和任务物品有不同数据规则；
- 人不能作为货物、库存或道具。

## 2. 分类

| 类别 | 用途 |
|---|---|
| 工具 | 采集、测深、生火、修理、观察 |
| 船材 | 木板、树脂、绳索、帆布、金属 |
| 武器 | 炮、火药、刀、短剑、火枪 |
| 补给 | 水、食物、药品、燃料 |
| 商品 | 糖、烟草、棉、香料、酒、布料 |
| 任务 | 信件、许可证、图纸、证据 |
| 收藏 | 标本、遗物、地图、徽章 |
| 装饰 | 桶、箱、灯、旗帜、家具 |

## 3. ItemDef 字段

- item_id；
- display_name；
- category；
- tags；
- weight；
- volume；
- stack_limit；
- durability；
- condition；
- base_value；
- legal_status；
- perishability；
- fire_risk；
- water_damage；
- resource_inputs；
- resource_outputs；
- compatible_systems；
- source_region；
- owner_faction；
- asset_refs；
- source_refs；
- license；
- phase；
- test_id。

## 4. 道具状态

- 完好；
- 磨损；
- 损坏；
- 受潮；
- 过期；
- 污染；
- 被没收；
- 被标记；
- 任务锁定；
- 不可交易。

## 5. 重量与体积

道具和货物同时有重量与体积。重量影响船吃水，体积影响货舱，位置影响倾斜和防潮。
