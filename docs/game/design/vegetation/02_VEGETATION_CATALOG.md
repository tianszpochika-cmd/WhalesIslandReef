# 植被目录与唯一 ID

## 1. 目录字段

每个 PlantDef 包含：

- plant_id；
- display_name；
- scientific_or_historical_note；
- category；
- habitat_tags；
- soil_tags；
- salt_tolerance；
- drought_tolerance；
- water_need；
- altitude_range；
- season_tags；
- growth_stage；
- harvest_parts；
- resource_outputs；
- toxicity；
- fire_fuel；
- animal_relationships；
- human_cultivation；
- introduced_status；
- model_refs；
- material_refs；
- source_refs；
- license；
- phase；
- test_id；
- status。

## 2. 分类配额

| 类别 | 目标条目 | 例子 |
|---|---:|---|
| 高大乔木 | 150—250 | 红树、棕榈、热带硬木 |
| 果树和食用植物 | 120—180 | 香蕉、椰子、番石榴、木薯 |
| 农作物 | 100—160 | 甘蔗、烟草、棉、可可、香料 |
| 灌木和荆棘 | 150—250 | 干旱灌丛、海滨灌木 |
| 草本与地被 | 180—300 | 草、蕨、药草、花卉 |
| 藤本和附生 | 100—180 | 藤蔓、兰科、苔藓 |
| 湿地和水生植物 | 120—200 | 芦苇、海草、浮萍、藻类 |
| 枯木和演替阶段 | 80—140 | 树桩、倒木、幼苗、病株 |
| **合计** | **1,000—2,000** | 目录目标 |

## 3. ID 规则

格式：

plant_生境_名称_变体

示例：

- plant_mangrove_red_v1；
- plant_rainforest_mahogany_v1；
- plant_dry_cactus_tall_v2；
- plant_farm_sugarcane_mature；
- plant_cave_moss_wet。

## 4. 成长阶段

植物实例按：

- 种子；
- 幼苗；
- 生长；
- 成熟；
- 采集后；
- 枯萎；
- 病害；
- 风暴损伤；
- 恢复；
- 枯木或次生演替。

重要剧情植物可以锁定剧情状态，但普通植物必须遵守季节和恢复规则。

## 5. 目录验收

- 不允许同一 ID 对应两个不同植物；
- 每条目有生境和资源标签；
- 每条目有模型、占位模型或明确待制作状态；
- 每条目有来源和许可证；
- 每条目有采集或观察测试；
- 目录目标不等于当前已完成资产数量。
