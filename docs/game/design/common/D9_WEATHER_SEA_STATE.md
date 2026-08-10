# D9 天气与海况状态冻结

任务：D9-A05  
冻结日期：2026-08-10  
状态：已实现最小可测试垂直切片；D9-S02 的完整事件日志接线仍由其 owner 负责。

## 1. 边界结论

本文件是 D9-A05 天气/海况影响的单一设计入口。D9-S01 已冻结的 D3 五态天气仍保留为兼容投影；D9 垂直切片在 `EnvironmentSystem` 内使用三态：`Calm`、`Rain`、`Storm`。

| 数据或行为 | 唯一写者 | 读取者 | 写入方式 | 禁止事项 |
|---|---|---|---|---|
| D9 天气状态与海况快照 | `EnvironmentSystem` | 船舶适配器、生态适配器、存档适配器、UI 只读投影 | `SetWeather`、`ApplyWeatherEvent`、`AdvanceWorldDay`、`Restore` | Ship/Ecology/UI/AgentReply 直接改天气 |
| 风、能见度、海流、疲劳、船体风险、鱼群系数 | `EnvironmentSystem` 随天气状态一次性发布 | `D9ShipWeatherReader`、`D9EcologyWeatherReader` | profile → `D9WeatherSeaSnapshot` | 读取者回写快照或重复结算 |
| 天气事实事件 | `EnvironmentSystem` 生成，`EventLog.Append` 记录 | Quest、UI、Agent 上下文、存档 | `event_d9_weather_storm` + `event_id/save_revision` 去重 | 事件层自行改变天气或效果 |
| 存档天气字段 | `SaveSystem` 负责序列化/反序列化，环境 owner 负责快照内容 | `EnvironmentSystem.Restore`、读档测试 | `location_json.d3.weatherSea` | 删除旧 D3 `weather` 字段 |

当前仓库中尚未存在可接入的 canonical `ShipSystem` 或 D9 鱼群天气入口。因此本任务只创建纯数据的 Environment owner 与两个只读适配器；D3 航程结算公式和 D4 动物逻辑不被复制或改写。后续系统应直接消费适配器，不应再建立第二套天气公式。

## 2. 三种状态与冻结值

所有比例为无量纲值，范围为 `0..1`；角度为 `0..360` 度归一化；风速和海流速度为非负数；疲劳为每世界日的整数点数。

| 状态 | 风速 | 风向 | 能见度 | 海流速度 | 海流方向 | 船员疲劳/日 | 船体风险 | 鱼群活跃 | 鱼获系数 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `Calm` | 4 | 90° | 0.95 | 0.20 | 90° | 1 | 0.05 | 1.00 | 1.00 |
| `Rain` | 8 | 110° | 0.65 | 0.45 | 110° | 3 | 0.15 | 0.90 | 0.80 |
| `Storm` | 16 | 135° | 0.25 | 0.80 | 135° | 6 | 0.45 | 0.55 | 0.50 |

`D9WeatherProfile` 会在构造时执行安全归一化：负速度/疲劳归零，比例裁剪到 `0..1`，非法角度归一化到 `0..360`。快照包含 `schema_version=1`；未知状态拒绝恢复，不静默写入未知值。

已落盘快照的影响字段必须通过范围校验；能见度/船体风险必须在 `0..1`，速度/倍率/疲劳不能为负，方向必须是归一化角度。损坏快照在恢复前拒绝，不覆盖当前有效环境状态。

## 3. 影响计算

### 船舶读取

`D9ShipWeatherReader.Read` 只读快照，并计算以下投影：

```text
航速系数 = clamp(1 - 风速/40 - (1-能见度)*0.25 - 海流速度*0.10, 0.35, 1.00)
危险标记 = 船体风险 >= 0.30 或 能见度 < 0.40
```

`ResolveExposure(weather, days)` 返回预览/结算所需的疲劳增量和累计风险：

```text
疲劳增量 = 每日疲劳 × max(days, 0)
累计船体风险 = clamp(单日风险 × max(days, 1), 0, 1)
```

该适配器不写 Ship、Crew、Hull 或疲劳状态；实际数值提交仍必须由相应领域 owner 裁决。

### 生态读取

`D9EcologyWeatherReader.ReadFishConditions` 只读快照，提供鱼群活跃系数、鱼获系数和观测是否被阻断。`ResolveCatch` 仅返回确定性的结果预览：

```text
鱼获数量 = floor(max(base_quantity, 0) × max(fish_catch_multiplier, 0))
观测阻断 = 能见度 < 0.30
```

它不生成 FishSchool、FishCatch 事实，也不写库存。D9-A06 或生态 owner 接入时必须继续保持这个写入边界。

## 4. 事件、幂等与时间

- D9 风暴事件固定 ID：`event_d9_weather_storm`。
- 事件 owner：`EnvironmentSystem`。
- 事件层使用 `event_id + save_revision` 作为日志去重键；天气快照同时保存 `last_event_id` 与 `applied_event_ids`，防止只有快照被恢复时再次改变海况。
- `ApplyStormEvent` 第二次调用返回 `duplicate_weather_event`，不追加日志、不增加 revision、不重复生成影响。
- `AdvanceWorldDay` 只推进 `world_day/revision`，不自动再次触发天气事件；事件调度必须由 EnvironmentSystem 的明确入口完成。
- 读档后用保存的快照 `Restore`，再提交同一风暴事件时返回重复结果，船体风险、疲劳和鱼群系数保持不变。

## 5. 存档形状与 D7 兼容

天气海况嵌入既有 D3 runtime snapshot：

```text
location_json.d3.weatherSea = {
  schemaVersion, worldDay, revision, weather,
  windSpeed, windDirectionDegrees, visibility,
  currentSpeed, currentDirectionDegrees,
  crewFatiguePerDay, hullRisk,
  fishActivityMultiplier, fishCatchMultiplier,
  locationRef, lastEventId, appliedEventIds
}
```

旧字段 `location_json.d3.weather`（`Calm/TradeWind/Rain/Storm/Hurricane`）保持不变并继续写入。保存 D9 快照时，`EnvironmentSystem.PublishTo` 同步更新旧字段作为 D3/UI 兼容投影；旧档缺少 `weatherSea` 时，读档不会失败，调用 `RestoreLegacy` 将旧天气映射为安全的 D9 默认快照：

| D3 旧天气 | D9 恢复状态 |
|---|---|
| `Calm`、`TradeWind` | `Calm` |
| `Rain` | `Rain` |
| `Storm`、`Hurricane` | `Storm` |

旧档恢复不会伪造 `AppliedEventIds`，因此不会把历史天气误判为新的 D9 事件。D7 航行、船体、食物、金钱等字段不被删除或重解释。

## 6. 测试入口

测试文件：`game/Assets/Tests/EditMode/D9WeatherSeaStateTests.cs`，测试边界：`D9-A05-WEATHER`。

覆盖内容：

- Calm/Rain/Storm 的海况差异，以及航速、疲劳、船体风险、鱼获结果差异；
- 风暴事实事件只追加一次；
- `D3RuntimeSnapshot` 的 D9 快照保存、恢复和旧 `weather` 投影；
- 旧 D3 存档缺少 D9 快照时的兼容恢复；
- Ship/Ecology 读取器不改变 Environment 快照。

纯 C# 语法/依赖检查可使用：

```text
dotnet build game/.codex-d9-a05-compile.csproj
```

该临时编译入口不属于交付文件，验证后应删除；Unity EditMode 测试入口为 `D9.Tests.D9WeatherSeaStateTests`。完整 Unity 测试应由打开的 Unity 工程执行。

## 7. 跨模块冲突与后续约束

1. D3 当前仍持有旧 `Weather` 字段，且 D3 航程结算使用旧五态目录；本任务采用 D9→D3 兼容投影，不在 D9 重写 D3 航程结算。后续若统一天气枚举，必须先更新 D3 航程、存档和 UI 的共同契约。
2. D8-S02 负责跨领域 EventLog 的完整持久化接线；本任务保存天气快照内的事件去重字段，并在有 EventLog 时追加事实，但不替 D8-S02 决定事件日志的总装载时序。
3. 当前没有 canonical ShipSystem/EcologySystem 的天气消费入口；后续 A 任务应把实体字段写入交还给 Ship/Ecology owner，继续只读取本文件定义的快照与适配器。
4. 本任务不新增 server/web/contracts 字段；天气三态和影响仍是 game 内部垂直切片数据，跨端展示如有需要必须另开契约变更并同步三端。
