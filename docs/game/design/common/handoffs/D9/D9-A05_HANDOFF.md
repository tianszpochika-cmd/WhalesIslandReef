# D9-A05 交接报告：天气与海况状态

- 任务编号：D9-A05
- 完成日期：2026-08-10
- 前置任务：D9-S01 已完成；D9-A05 补齐 D9-S02 指出的 Environment producer 缺口
- 主成果：D9 三天气、海况影响、存档恢复和幂等测试

## 修改文件

### 代码与测试

- `game/Assets/Scripts/Systems/D9/D9WeatherSeaState.cs`
- `game/Assets/Scripts/Systems/D9/EnvironmentSystem.cs`
- `game/Assets/Scripts/Systems/D9/D9WeatherImpactReaders.cs`
- `game/Assets/Scripts/Systems/D3/D3Contracts.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Assets/Tests/EditMode/D9WeatherSeaStateTests.cs`

### 文档

- `docs/game/design/common/D9_WEATHER_SEA_STATE.md`
- `docs/game/design/common/handoffs/D9/D9-A05_HANDOFF.md`
- `.workbuddy/memory/2026-08-10.md`

## 数据边界

`EnvironmentSystem` 是天气、风、能见度、海流、每日疲劳、船体风险和鱼群系数的唯一写者。`D9ShipWeatherReader` 和 `D9EcologyWeatherReader` 只接受快照副本，返回影响投影或确定性预览，不写天气、船、船员、鱼群或库存。UI 只能读取 `D3RuntimeSnapshot.Weather` / `WeatherSea`；AgentReply 没有天气写入口。

事件事实由 EnvironmentSystem 构造并通过 D8 `EventLog.Append` 记录。固定风暴事件是 `event_d9_weather_storm`，日志使用 `event_id + save_revision` 去重，快照另外记录 `last_event_id` 和 `applied_event_ids`。

## D7/v3 差异与存档

保留 D7/D3 的 `D3RuntimeSnapshot.Weather` 五态字段及所有原航行字段。新增 D9 `WeatherSea` 快照，schema version 为 `1`，含：

`worldDay`、`revision`、三态 `weather`、风速/风向、能见度、海流速度/方向、每日船员疲劳、船体风险、鱼群活跃/鱼获系数、地点、最后事件 ID、已应用事件 ID 列表。

保存位置为 `location_json.d3.weatherSea`；旧 `location_json.d3.weather` 同步保留为兼容投影。旧档没有 `weatherSea` 时，`RestoreLegacy` 以 Calm/Rain/Storm 安全默认 profile 恢复，不删除、不重解释旧字段，也不伪造历史事件。

## 冲突与未完成事项

1. D3 现有航程仍使用五态天气目录，D9 三态只通过 `PublishTo` 映射到 D3。统一五态/三态需要 D3、存档和 UI 联合变更，未在本任务擅自决定。
2. 当前仓库没有 canonical ShipSystem 或 D9 鱼群天气消费系统，因此 A05 提供最小纯数据读者适配器；后续 A 任务不得在 Ship/Ecology 中复制天气公式。
3. D8-S02 仍负责 EventLog 总体存档/装载接线。本任务已让天气快照独立保存去重状态，但不改变 D8-S02 的跨领域装载顺序。
4. 未新增 server/web/contracts 字段。若官网或服务端要展示天气，必须另开契约变更并同步三端。

## 验收结论

- [x] Calm、Rain、Storm 均有可保存的海况快照。
- [x] 风、能见度、海流、疲劳、船体风险和鱼群系数由 profile 明确提供。
- [x] 船舶/生态只读天气；环境系统是唯一写者。
- [x] 风暴事件可通过 EventLog 和快照双重去重，读档后重复提交不重复改变影响。
- [x] D7 旧天气字段保持兼容；旧档缺少 D9 字段时安全恢复。
- [x] 未知天气枚举和损坏影响快照在恢复前拒绝，不静默修改环境状态。
- [x] 新增纯 C# D9 代码通过临时独立编译检查。
- [ ] Unity EditMode 全量运行：当前工程存在并行 Unity 编辑器/任务状态，需在 Unity 工程可用时执行 `D9.Tests.D9WeatherSeaStateTests`。

已执行的编译命令：

```text
dotnet build game/.codex-d9-a05-compile.csproj
dotnet build game/AgentCore/AgentCore.csproj --no-restore
python tools/d9_vertical_slice_check.py
python -m unittest tools.tests.test_d9_vertical_slice_freeze -v
git diff --check
```

结果：D9 纯 C# 临时编译通过（0 warning、0 error，入口已删除）；AgentCore build 通过（0 warning、0 error）；D9 冻结脚本通过；冻结 unittest 1/1 通过；`git diff --check` 无新增空白错误（仅报告仓库既有换行转换提示）。Unity 定向命令因项目已被其他 Unity 实例打开而未生成结果 XML。

完整生成 Systems 工程的尝试仍被工作区既有 AgentCore `AgentModels.cs` 中缺少 `CrewDuty` 类型阻断；这不是本任务新增的 D9 编译错误，需由并行 AgentCore 基线修复后复跑。

## D9-S02 与后续 A 任务注意事项

- D9-S02 应把 WeatherSea 接入它的天气事件总装载和事件日志恢复流程，保留 `event_id + save_revision` 与 `applied_event_ids` 双重保护。
- Ship 侧接入时只消费 `D9ShipWeatherReader.Read/ResolveExposure`，实际疲劳和船体写入仍由 Ship/Crew owner 完成。
- Ecology 侧接入时只消费 `D9EcologyWeatherReader`，实际 FishSchool/FishCatch 写入仍由 Ecology owner 完成。
- UI 只展示快照和影响摘要；任何按钮意图必须经过 EnvironmentSystem 的明确命令入口。
- 在 Unity EditMode 测试通过前，不建议把 D9-A05 标记为可发布的跨端天气功能；允许继续进行不依赖 Unity 运行时的文档和纯数据异步工作。
