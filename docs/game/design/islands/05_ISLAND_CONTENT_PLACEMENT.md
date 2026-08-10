# 岛屿内容布置

## 1. 放置顺序

岛屿制作必须按：

地质 → 高度 → 水文 → 生境 → 植被 → 动物 → 道路 → 建筑 → 资源 → 任务 → 表现

反向制作会导致建筑挡河、动物出现在错误生境和资源没有来源。

## 2. 自然区域字段

- region_id；
- island_id；
- parent_region；
- geology；
- habitat；
- elevation；
- water_sources；
- coast_access；
- walkability；
- hazard_tags；
- plant_tags；
- animal_tags；
- resource_tags；
- building_tags；
- faction_tags；
- quest_tags；
- streaming_cell；
- lighting_profile；
- audio_profile。

## 3. 资源点

资源点分为：

- 普通刷新点；
- 稀有观察点；
- 任务锁定点；
- 社群管理点；
- 危险环境点；
- 生态关键点。

资源点必须有刷新、耗尽、恢复、采集工具和玩家反馈。

## 4. 多路线

重点地点至少提供两条路线：

- 海岸路线；
- 内陆路线；
- 山地或洞穴路线；
- 潮汐路线；
- 小艇路线；
- 社群许可路线。

玩家不应只能沿单一任务箭头行走。

## 5. 岛屿档案验收

- 50 座岛都有主地貌和自然档案；
- 每座重点岛有不同的登陆、淡水、危险和资源逻辑；
- 生态、建筑和势力布置有来源；
- 海图能表达海岸、浅滩、礁口和港口；
- 任务不会把玩家送到不可达或无恢复的区域。
