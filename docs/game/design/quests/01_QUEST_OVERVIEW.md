# 任务系统总览

## 1. 任务类型

目标包含：

- 5 章主线；
- 120 个支线模板；
- 世界事件；
- 港口任务；
- 船员个人任务；
- 势力任务；
- 生态观察任务；
- 船坞和贸易任务；
- 救援、调查、护航、追捕和寻宝。

## 2. 任务字段

- quest_id；
- type；
- chapter；
- giver；
- location；
- required_flags；
- required_reputation；
- required_ship_tags；
- required_items；
- required_crew；
- objectives；
- optional_objectives；
- failure_conditions；
- rewards；
- consequences；
- recovery_routes；
- historical_tags；
- sensitivity_tags；
- authored_lines；
- generated_ambient_lines；
- test_id。

## 3. 任务结构

发现 → 接受 → 准备 → 出航或行动 → 事件 → 选择 → 结算 → 日志 → 后果。

## 4. 任务原则

- 任务是引导，不是唯一玩法；
- 失败改变关系和奖励，不轻易删除世界结构；
- 任务目标必须在海图、日志或 NPC 话语中可理解；
- 主线关键节点由作者控制；
- LLM 只能扩展外围对白和摘要。

## 5. 五章方向

1. 港口与旧债；
2. 公司与旗帜；
3. 土地与自由；
4. 封锁与私掠；
5. 自己的航线。
