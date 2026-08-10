# 事件因果与动态任务

## 1. 事件字段

- event_id；
- trigger_type；
- trigger_conditions；
- probability；
- cooldown；
- region_scope；
- faction_scope；
- actor_scope；
- weather_scope；
- prerequisites；
- choices；
- consequences；
- recovery；
- log_text；
- seed；
- owner_system。

## 2. 触发条件

- 时间；
- 地点；
- 天气；
- 船体状态；
- 货物；
- 船员状态；
- 势力声望；
- 法律身份；
- 动物或植物生态；
- 任务旗标；
- 玩家过去行为。

## 3. 事件等级

| 等级 | 例子 | 失败 |
|---|---|---|
| 日常 | 小雨、鱼群、船员争论 | 时间或资源 |
| 区域 | 封锁、洪水、价格变化 | 路线和声望 |
| 重大 | 风暴、追捕、战斗 | 船体、船员和法律 |
| 主线 | 作者锚点、章节选择 | 结局方向 |

## 4. 事件裁决

事件系统只根据条件触发和分发输入，实际数值结果由对应系统裁决：

- 船体由 ShipState；
- 价格由 Economy；
- 声望由 Faction；
- 动物由 Ecology；
- 疾病由 Survival；
- 任务旗标由 Quest。

## 5. 事件恢复

事件失败必须有：

- 替代路线；
- 延迟后重试；
- 付费补救；
- 关系损失；
- 新任务；
- 日志说明。

## 6. 事实与文案

事件事实先写入日志，再由 Agent 生成摘要。摘要不得反向修改事件结果。
