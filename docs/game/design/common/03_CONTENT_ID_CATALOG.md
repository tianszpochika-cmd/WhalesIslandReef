# 内容 ID 与目录规范

## 1. ID 原则

ID 必须稳定、唯一、可读、可迁移。禁止使用展示名称作为主键。

推荐格式：

领域_对象_名称_变体

示例：

- ship_sloop_base；
- plant_mangrove_red_v1；
- animal_sea_turtle_caribbean；
- npc_harbor_clerk_001；
- building_shipyard_town_v1；
- item_pitch_barrel；
- quest_main_old_debt_01。

## 2. 内容条目字段

所有目录条目至少包含：

- id；
- display_name；
- category；
- tags；
- description；
- gameplay_role；
- owner_system；
- valid_regions；
- valid_habitats；
- valid_weather；
- dependencies；
- asset_refs；
- source_refs；
- license；
- phase；
- status；
- test_id；
- reviewer；

## 3. 状态值

| 状态 | 含义 |
|---|---|
| draft | 只有想法或字段草稿 |
| designed | 规则和字段已经完成 |
| cataloged | 已有唯一 ID 和目录记录 |
| prototyped | 有可运行样例 |
| production | 正在批量制作 |
| verified | 通过功能和表现验收 |
| deprecated | 不再使用，但保留迁移记录 |

## 4. 版本规则

- 改展示名称不改变 ID；
- 改规则字段要升级内容版本；
- 删除条目先标 deprecated；
- 运行时已经保存的条目不能直接复用旧 ID 表示新对象；
- 资源替换不改变逻辑 ID；
- 同一条目不能同时属于两个唯一写者。

## 5. 资源来源

资产来源必须记录：

- 作者或供应商；
- 许可证；
- 下载或购买日期；
- 原始链接；
- 是否允许商业发行；
- 是否需要署名；
- 是否允许修改；
- 导入和压缩记录。

## 6. 目录验收

- 没有重复 ID；
- 没有缺少 owner_system 的条目；
- 没有缺少 license 的外部资产；
- 所有已实现条目至少有一个测试入口；
- 所有 deprecated 条目有替代 ID 或迁移说明。
