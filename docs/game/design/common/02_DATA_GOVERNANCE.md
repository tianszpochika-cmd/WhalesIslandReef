# 数据治理与系统所有权

## 1. 数据生命周期

数据从设计到运行时经过：

设计条目 → Schema → 种子数据 → 系统读取 → 运行时实例 → 快照 → 日志 → QA 证据。

任何一步缺失，都不能把内容称为“完成”。

## 2. 数据类型

| 类型 | 示例 | 是否可运行时修改 |
|---|---|---:|
| 定义数据 | ShipTypeDef、TraitDef、PlantDef | 否 |
| 目录数据 | 50 座岛、300 动物、200 道具 | 否 |
| 实例数据 | 玩家船、某个 NPC、某片红树林 | 是 |
| 状态数据 | 健康、库存、关系、天气、进水 | 是 |
| 事件数据 | 战斗、交易、疾病、风暴、对话结果 | 只追加 |
| 表现数据 | 模型、动画、音频、材质 | 由资源管线管理 |

## 3. 所有权矩阵

| 领域 | 唯一裁决者 | 不能写入的领域 |
|---|---|---|
| 玩家与存档 | Core / SaveSystem | 不能计算经济或战斗 |
| 船舶 | Ship Systems | 不能由 UI 或 LLM 修改 |
| 角色数值 | CrewSystem | 不能由对白直接修改 |
| 角色语言 | Agent 层 | 不能裁决数值 |
| NPC 日程 | NPCSystem | 不能直接改势力声望 |
| 植被与资源 | VegetationSystem | 不能由采集 UI 直接改库存 |
| 动物 | EcologySystem | 不能使用 LLM 决策 |
| 天气与时间 | WeatherSystem / WorldClock | 不能由任务文本直接改 |
| 建筑与港口 | BuildingSystem | 不能由装饰系统改功能规则 |
| 经济 | EconomySystem | 不能由商人对白突破价格边界 |
| 势力与法律 | FactionSystem | 不能由任务文案直接改声望 |
| 战斗 | CombatSystem | 不能由动画结果反向改伤害 |
| 任务事件 | EventSystem | 不能越权写所有系统 |

## 4. Agent 边界

Agent 只输出：

- 命令意图；
- 台词；
- 情绪；
- 叙事摘要；
- 建议；
- 观察报告。

Agent 不输出最终：

- 金钱；
- 价格；
- 耐久；
- 伤害；
- 位置；
- 物种数量；
- 任务完成；
- 声望；
- 法律身份；
- 存档字段。

## 5. 契约规则

跨端字段必须：

1. 先写 contracts；
2. 更新客户端模型；
3. 更新服务端校验；
4. 更新官网或工具端类型；
5. 更新测试样例；
6. 更新设计文档。

只改设计文档时，不得声称已经完成跨端实现。

## 6. 版本迁移

每个可保存对象需要：

- schema_version；
- created_at；
- updated_at；
- source_version；
- default migration；
- unknown field policy；
- corruption recovery。

与 `DATA_MODELS.md` 对齐：游戏实体的时间字段统一使用世界时间 `created_day` / `updated_day`；`created_at` / `updated_at` 仅可作为存储审计元数据，不得成为第二套游戏状态字段。

旧档读取时填充默认值，不通过删除旧字段来“解决兼容”。

## 7. 事件日志

事实日志至少记录：

- event_id；
- world_day；
- actor_id；
- target_id；
- input_intent；
- resolved_by；
- old_state；
- new_state；
- consequence；
- display_text；
- source_version。

日志系统只追加事实，不反向修改状态。
