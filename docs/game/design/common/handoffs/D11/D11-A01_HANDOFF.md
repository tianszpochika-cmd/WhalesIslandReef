# D11-A01 交接报告

日期：2026-08-10  
任务：D11-A01 生态状态机  
前置：D9-S02 PASS

## 交付结论

D11-A01 已完成代表性生态状态机：生境、植物、动物、鱼群、捕捞压力、恢复、天气、潮汐、海流、温度、浑浊度和污染均有确定性关系；状态通过现有 D4 Ecology 快照保存，事件通过共享 D8 EventLog 追踪。没有复制第二套生态逻辑。

## 修改入口

| 文件 | 内容 |
|---|---|
| `game/Assets/Scripts/Systems/D4/D4Contracts.cs` | 扩展 Ecology/Fish/Animal 状态、植物状态、生境条件、只读生态条件和操作结果契约 |
| `game/Assets/Scripts/Systems/D4/D4EcologySystem.cs` | 保持原 D4 入口并初始化 D11 字段；鱼群提交增加生态修订和恢复余量 |
| `game/Assets/Scripts/Systems/D4/D4EcologyD11StateMachine.cs` | D11-A01 生态确定性关系、恢复、污染、植物采集、天气读取和事件写入 |
| `game/Assets/Scripts/Systems/Save/SaveSystem.cs` | D4 Ecology 的 D11 字段序列化/读档；保留旧 D4 字段 |
| `game/Assets/Tests/EditMode/D11EcologyStateMachineTests.cs` | Unity EditMode 定向测试 |
| `tools/d11_a01_smoke/` | 不依赖 Unity 场景的纯 C# 代表闭环 smoke |
| `docs/game/design/common/D11_ECOLOGY_STATE_MACHINE.md` | 主设计/数据/所有权/验证文档 |

## 代表 ID

- 生境：`mangrove`，D11 canonical 映射为 `mangrove`；近海兼容映射为 `nearshore_reef_edge`；
- 植物：`plant_red_mangrove`；
- 动物：`animal_green_turtle` 内容登记样例，动物行为仍由现有 D4 动物目录和 EcologySystem 计算；
- 鱼群：`fish_mackerel_school`；
- 捕捞点：`nearshore_saint_marys`；
- 天气：D9 `Calm`、`Rain`、`Storm`；天气唯一写者是 `EnvironmentSystem`。

## 目录字段

D11 使用 D8 内容目录已有字段：`id`、`schema_version`、`display_name_key`、`source_note`、`license`、`historical_tag`、`phase`、`owner_system`、`test_id`。本任务没有新增外部资源或批量内容；`plant_red_mangrove`、`fish_mackerel_school`、`animal_green_turtle` 均可由内容目录扫描。

运行时生态状态字段见主文档，核心为：

- 生境：温度、浑浊度、污染、潮汐相位、海流速度/方向、修订号；
- 植物：数量、容量、采集压力、日恢复率、最后更新日；
- 鱼群：鱼种、位置、数量/容量、深度、迁徙、捕捞压力、恢复余量、最后移动日；
- 动物：生境、行为、健康、生态压力、观察和收藏值；
- 幂等：`lastEcologyEventId`、`appliedEcologyEventKeys`。

## 检查命令与结果

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

结果：`0 warning / 0 error`。

```text
dotnet run --project tools/d11_a01_smoke/D11A01Smoke.csproj
```

结果：

```text
D11-A01 smoke PASS: habitat/plant/animal/fish/pressure/weather/pollution/recovery/events
```

红线检查：

```text
rg -n "HttpClient|System.Net.Http|LLM|OpenAI|UnityWebRequest|WebRequest|Uri\(" game/Assets/Scripts/Systems -g "*.cs"
```

未发现新增可执行网络或 LLM 代码；命中均为既有注释/边界说明。

内容目录检查：

```text
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

结果：`Content registry OK: 35 entries, 13 prefixes`。

`git diff --check` 退出码为 0；工作区存在既有 CRLF 提示，不是 D11 内容错误。

全量测试程序集检查：`dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162` 仍被工作区既有 `D10S01AcceptanceIntegrationTests.cs(195)` 缺少 `D5RuntimeSnapshot` using 阻断；该错误不在 D11 修改范围内，没有通过修改其他系统掩盖。D11 Unity 测试已登记，基线恢复后执行 `D11.Tests.D11EcologyStateMachineTests`。

## 写者边界

- `EcologySystem`：鱼群位置、生物量、捕捞压力、恢复、植被数量/压力、动物生态压力和生境条件；
- `EnvironmentSystem`：天气、风、能见度、海况快照；D11 只读；
- `Inventory/Cargo`：鱼获和采集物入库；
- `EconomySystem`：交易价格和市场结算；
- `SaveSystem`：只复制快照，不计算生态结果；
- UI/Agent/任务：只能读取快照或提交意图，不能携带最终资源数量、污染、价格或恢复值。

## 后续 D11 入口

- D11-A02：复用 `ReadConditions`、`PreviewFishCatch`、`HarvestPlant`、`AdvanceEcology` 扩展捕捞/采集，不复制公式；
- D11-A03：复用 `FishSchoolState.depth/migration/recovery` 接入活鱼舱/养殖；
- D11-A04/A05：复用动物 `HabitatId` 和 `EcologyStress` 接入鲸豚/动物收集；
- 所有后续扩展必须继续让 EcologySystem 成为资源状态唯一写者，并使用共享 EventLog 的 `event_id + save_revision` 幂等键。
