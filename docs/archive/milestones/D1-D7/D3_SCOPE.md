# D3 可玩环范围冻结

> 状态：D3-01 已冻结，D3-01 至 D3-10 已完成｜日期：2026-08-09

## 目标

D3 的完成标准是“一次完整航程可以从港口整备开始，经过航行事件、登岛采集、交易和结算，再回到可继续游戏的存档状态”。本阶段交付的是可验证的玩法底座，不是最终美术内容量。

## 本阶段交付范围

| 领域 | D3 交付基线 | 数据所有者 |
|---|---|---|
| 货物 | 10 种货物：盐、糖、烟草、朗姆酒、咖啡、可可、香料、染料、木材、药材 | `D3InventorySystem` / `TradeEconomySystem` |
| 地点 | 圣玛格丽特港（城镇）、绿湾村（村落）、漂木荒岛（荒岛） | `PortCatalog` |
| 价格 | 基础价 × 供需系数 × 港口关税 × 事件扰动，并包含港口专长修正 | `TradeEconomySystem` |
| 采集 | 村落和荒岛可以登岛采集；单次访问有资源上限和货舱容量约束 | `GatheringSystem` |
| 航行 | 起点→目的地的离散航程日推进，消耗水/食物并记录船体变化 | `VoyageSettlementSystem` |
| 事件 | 8 个确定性事件；事件只由强逻辑系统裁决，Agent 不能改数值 | `VoyageEventSystem` |
| 天气 | 晴朗、信风、降雨、风暴、飓风 5 类 | `WeatherCatalog` |
| 持久化 | 贸易货物、玩家库存、金钱、港口位置、航程状态和航线日志可保存/读取 | `SaveSystem`（序列化）+ D3 系统（数值裁决） |
| 表现层 | 港口信息、贸易面板、航程推进、采集/靠港/结算按钮和键盘快捷键 | `Gameplay/D3` |
| 服务端 | 与本地裁决字段一致的报价、下单、航程结算开发态接口 | `server/modules/trade` |

## 明确不在 D3

- 不接入真实 LLM、联网贸易或服务端裁决；服务端接口只做契约和开发态镜像。
- 不扩展到需求文档的完整 14 种货物、12 座岛屿和 12-15 个事件；这些属于后续内容量，不阻塞 D3 底座验收。
- 不实现最终港口建筑、角色动画、Crest 海洋和完整生态；运行时占位物只负责证明玩法链路。
- 不引入复杂时间连续模拟；D3 采用可存档的离散航程日，D2 的实时操船继续保持不变。

## 数据所有权

1. `PortCatalog`、`CommodityCatalog`、`WeatherCatalog`、`VoyageEventCatalog` 只读提供静态定义。
2. `D3InventorySystem` 唯一修改货舱容量和货物数量。
3. `TradeEconomySystem` 唯一修改贸易金钱与市场库存，并通过库存系统执行货舱变更。
4. `GatheringSystem` 唯一裁决采集数量，通过库存系统入舱。
5. `VoyageEventSystem` / `VoyageSettlementSystem` 唯一修改航程日、补给、船体和事件记录。
6. `SaveSystem` 只负责序列化，不在读取或写入时进行经济裁决。

## 验收门

- 纯 C#：10 种货物、3 个地点、价格公式、买卖边界、货舱边界、采集上限、5 种天气、8 个事件和航程成功/失败路径均有 EditMode 测试。
- Unity：新游戏进入初始港后能看到 D3 面板；至少能执行“规划航程→推进航程→登岛/采集→靠港→买卖→结算→保存”。
- Server：`npm run build` 通过，`/trade/quote`、`/trade/order`、`/voyage/settle` 与 OpenAPI/JSON Schema 字段一致。
- 存档：D3 状态写入旧 SQLite 表的 JSON 字段，D2 存档仍可加载；旧存档缺少 D3 字段时按默认值兼容。
- 留痕：本文件、D3 任务提示词、验收报告、`docs/CONTEXT.md`、`docs/README.md` 和当日 memory 同步更新。
