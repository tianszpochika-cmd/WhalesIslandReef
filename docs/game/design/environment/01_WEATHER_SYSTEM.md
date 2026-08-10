# 天气系统

## 1. 天气状态

首版至少支持：

- 晴朗微风；
- 多云；
- 阵雨；
- 持续降雨；
- 强风；
- 雷暴；
- 雾；
- 热湿闷热；
- 干旱高温；
- 风暴前兆；
- 飓风影响；
- 风暴恢复期。

## 2. 状态字段

- weather_id；
- region_id；
- start_time；
- expected_duration；
- wind_direction；
- wind_speed；
- gust；
- rainfall；
- cloud；
- visibility；
- temperature；
- wave_height；
- current_strength；
- storm_level；
- warning_level；
- affected_systems；
- recovery_profile；
- seed。

## 3. 预警链

常态 → 迹象 → 预警 → 影响 → 恢复。

迹象来自：

- 云层、光照和风向；
- 海面和浪型；
- 鸟群、鱼群和动物躲藏；
- 船员经验；
- 港口旗语；
- 雷声、雨幕和闪电；
- 海图或势力情报。

## 4. 系统影响

天气可以影响：

- 船速、转向、帆具、锚泊和进水；
- 角色体温、疲劳、疾病和火把；
- 植被摆动、掉枝、泥泞和火灾；
- 动物觅食、躲藏、迁徙和鱼群；
- NPC 日程、港口工作和市场供应；
- 洞穴、河流、瀑布、滑坡和洪水。

## 5. 灾害原则

大型灾害必须可预警、可绕行、可避难、可恢复。不能用无提示随机事件永久删除主线。
