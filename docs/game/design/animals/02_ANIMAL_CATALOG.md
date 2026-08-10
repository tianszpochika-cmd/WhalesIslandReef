# 动物目录与物种字段

## 1. AnimalDef 字段

- animal_id；
- display_name；
- scientific_or_historical_note；
- class；
- habitat_tags；
- activity_period；
- diet；
- sociality；
- danger；
- health；
- speed；
- senses；
- territory；
- migration_tags；
- nesting_or_spawning；
- predator_tags；
- prey_tags；
- resource_outputs；
- observation_value；
- collection_rule；
- conservation_status；
- model_refs；
- animation_refs；
- sound_refs；
- source_refs；
- license；
- phase；
- test_id。

## 2. ID 规则

格式：

animal_生境_名称_变体

示例：

- animal_ocean_whale_humpback；
- animal_reef_parrotfish_v1；
- animal_mangrove_crab_red；
- animal_rainforest_parrot_green；
- animal_dryland_iguana_v1。

## 3. 行为标签

- day_active；
- night_active；
- crepuscular；
- solitary；
- pair；
- herd；
- school；
- territorial；
- migratory；
- nesting；
- spawning；
- venomous；
- predator；
- scavenger；
- pollinator；
- indicator_species。

## 4. 条目状态

draft、designed、cataloged、prototyped、production、verified、deprecated。

每个条目必须标记当前状态，不能用 300 这个数字冒充 300 个已完成物种。

## 5. 目录验收

- 300 条目有唯一 ID；
- 每条目至少有一个生境；
- 每条目有行为层级；
- 每条目有模型或明确占位；
- 每个危险行为有伤害或逃跑规则；
- 每个捕捞或观察行为有结果；
- 物种分布与岛屿自然档案一致。
