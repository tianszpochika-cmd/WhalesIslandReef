# 建筑与港口总览

## 1. 数量目标

- 80 类功能建筑；
- 180 个建筑变体；
- 600—1,200 个建筑实例；
- 每个大型港口至少 15 个可交互功能建筑；
- 每个村落至少 6 个功能建筑；
- 荒岛和遗迹以资源、危险和历史地点为主。

建筑实例必须有位置、所有者、功能、状态、所属势力、开放条件和交互入口。

## 2. 建筑分类

| 类别 | 例子 | 主要系统 |
|---|---|---|
| 港口 | 码头、泊位、灯塔、海关 | 船舶、势力、航行 |
| 船坞 | 修船棚、帆具坊、炮匠坊 | 船舶、经济 |
| 贸易 | 市场、仓库、交易所、商会 | 经济、任务 |
| 生活 | 住宅、酒馆、旅店、诊所 | NPC、船员、生存 |
| 社会 | 教堂、祈祷所、会所、学校 | 宗教、文化、关系 |
| 防御 | 炮台、城墙、军营、检查站 | 势力、战斗、法律 |
| 生产 | 农场、磨坊、木材场、渔场 | 经济、生态 |
| 探索 | 瞭望塔、洞穴入口、遗迹 | 岛屿、任务 |
| 管理 | 市政厅、监狱、档案室 | 法律、势力 |

## 3. BuildingDef 字段

- building_id；
- category；
- display_name；
- variant；
- location_requirements；
- owner_faction；
- owner_npc；
- function_tags；
- capacity；
- worker_slots；
- storage_capacity；
- service_price；
- input_resources；
- output_resources；
- open_hours；
- access_rules；
- damage_state；
- fire_state；
- occupation_state；
- model_refs；
- audio_profile；
- source_notes；
- phase；
- test_id。

## 4. 建筑不是静态菜单

建筑可以：

- 开门、关门、排队；
- 缺货、涨价、停工；
- 被封锁、检查、占领或损坏；
- 受天气、战争、火灾和污染影响；
- 由 NPC 使用；
- 产生订单、工作和任务；
- 影响附近资源与社会关系。

## 5. 位置规则

建筑必须符合：

- 地形；
- 水深；
- 道路；
- 港口防御；
- 风暴风险；
- 生境保护；
- 历史时期；
- 所属势力。
