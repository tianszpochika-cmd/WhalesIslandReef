# D11-A01 生态确定性状态机

状态：完成（2026-08-10）  
任务：D11-A01  
前置：D9-S02 已通过

本任务把 D11 的第一段生态闭环接入现有 `D4EcologySystem`。没有新建第二个生态系统，也没有把 UI、任务脚本或 Agent 变成资源裁决者。

## 1. 代表样例与唯一写者

| 类型 | 代表 ID | 用途 | 唯一写者 |
|---|---|---|---|
| 生境 | `mangrove` / `nearshore_reef_edge` | D4 兼容生境与 D11 canonical 生境条件 | `EcologySystem` |
| 植物 | `plant_red_mangrove` | 有正式 D8 内容登记的植被资源 | `EcologySystem` |
| 动物 | `animal_green_turtle`（内容登记样例）及 D4 生境动物目录 | 动物行为、健康和生态压力 | `EcologySystem` |
| 鱼群 | `fish_mackerel_school` | 生物量、位置、深度、迁徙和捕捞压力 | `EcologySystem` |
| 捕捞点 | `nearshore_saint_marys` | 鱼群位置引用 | `EcologySystem` 写位置，捕捞系统只提交请求 |
| 天气/海况 | D9 `Calm`、`Rain`、`Storm` | 生态输入快照 | `EnvironmentSystem` |

`plant_red_mangrove`、`fish_mackerel_school` 和 `animal_green_turtle` 均来自 D8 内容目录；本任务没有新增无来源或无授权外部资源，也没有批量伪造植被、动物或岛屿数量。

## 2. 运行时状态

`D4RuntimeSnapshot.Ecology` 新增以下 D11 字段，仍由 `EcologySystem` 负责写入，`SaveSystem` 只负责序列化和还原：

```text
habitatCondition:
  habitatId, biomeId,
  temperatureC, turbidity, pollution,
  tidePhase, currentSpeed, currentDirectionDegrees,
  lastUpdatedDay, revision
plants[]:
  plantId, habitatId, quantity, capacity,
  harvestPressure, recoveryRatePerDay, lastUpdatedDay
fishSchools[]:
  fishSchoolId, fishSpeciesId, fishingSpotId,
  biomass, capacity, depth, migration,
  fishingPressure, recovery, recoveryRatePerDay, lastMovedDay
animals[]:
  animalId, habitatId, behavior, health,
  ecologyStress, lastUpdatedDay, observed, collectionValue
lastEcologyDay, ecologyRevision, lastEcologyEventId,
appliedEcologyEventKeys[]
```

旧 D4 字段 `habitatId`、`animals[].animalId/health/observed/collectionValue` 和鱼群旧字段全部保留。D8 的 `fish_school` marker 仍不是本任务的 FishSchool 实例；真实鱼群使用 `fish_mackerel_school`。

## 3. 确定性关系

生态只读取 `D9WeatherSeaSnapshot` 的副本。天气状态及风/海流/能见度由 `D9EcologyWeatherReader` 和 D9 snapshot 提供，生态不写天气字段。

### 3.1 条件计算

对世界日 `d`：

```text
tidePhase = (d mod 4) / 4
tideWave = abs(sin(tidePhase * 2π))
tidalCurrent = 0.12 + 0.28 * tideWave
currentSpeed = weather.currentSpeed + tidalCurrent
```

温度以 27°C 为近海基线；雨天偏移 -0.4°C，风暴偏移 -1.0°C，再叠加潮汐波动。浑浊度由能见度、海流和污染确定性得到，并限制在 `[0,1]`：

```text
turbidity = clamp(
  0.08 + (1 - visibility) * 0.45 + currentSpeed * 0.10 + pollution * 0.35,
  0, 1)
```

`D9EcologyWeatherReader` 的鱼群活跃系数和鱼获系数不在 D11 重复定义。D11 只把它们与温度适宜度、潮汐、浑浊度和污染合成为：

```text
fishRecovery = clamp(
  0.45 + activity * 0.35 + temperatureSuitability * 0.20
  + tideMultiplier * 0.20 - turbidity * 0.30 - pollution * 0.55,
  0.05, 1.50)
plantGrowth = clamp(
  0.35 + temperatureSuitability * 0.45 + (1 - turbidity) * 0.25
  + tideMultiplier * 0.15 - pollution * 0.60,
  0.05, 1.50)
```

### 3.2 捕捞压力、恢复和污染

- 鱼获预览的基础数量由 `fishSchoolId + fishingSpotId + day + seed` 的稳定哈希确定；不使用运行时随机数。
- 捕捞压力系数为 `clamp(1 - fishingPressure / 12, 0.25, 1)`；天气鱼获系数和潮汐系数共同影响预览数量。
- 捕捞提交后只由 EcologySystem 扣除 `biomass`、增加 `fishingPressure`，并记录生态修订号；库存仍由 Inventory/Cargo 写入。
- `AdvanceEcology` 按世界日恢复植物和鱼群，按恢复系数衰减捕捞压力；鱼群位置也在此由 EcologySystem 重写，海流只改变定位输入。
- 污染事件增加 `pollution` 和 `turbidity`，确定性增加资源压力并减少植物数量；污染不会由 Agent 或 UI 直接写入。
- `RestoreHabitat` 提供显式恢复入口；恢复量由当前天气、潮汐、浑浊度和污染共同计算，不接受外部传入的最终恢复数值。
- 动物不使用 LLM。推进时根据生态压力确定健康损耗或恢复，并在压力较高时转为 `Alert`；行为仍使用已有 D4 确定性状态机。

## 4. 系统入口

所有入口位于现有 `D4EcologySystem`：

```csharp
SeedHabitat(habitatId, day, seed)
SeedFishSchool(fishSchoolId, fishingSpotId, day, seed)
ReadConditions(weatherSnapshot, worldDay)                 // 只读
PreviewFishCatch(..., weatherSnapshot)                    // 只读预览
CommitFishCatch(preview)                                  // 鱼群唯一写入
AdvanceEcology(worldDay, weatherSnapshot, seed, log, revision, eventId)
HarvestPlant(plantId, requestedQuantity, day, seed, log, revision, eventId)
ApplyPollution(sourceId, amount, day, log, revision, eventId)
RestoreHabitat(day, weatherSnapshot, seed, log, revision, eventId)
```

UI 只读 `Snapshot`、`FindHabitatConditionSnapshot`、`FindPlantSnapshot` 和事件日志副本；UI 不持有 `_state` 写引用。任务脚本和 Agent 不携带鱼获数量、污染结果或恢复结果。

## 5. 事件与重复结算

生态事实使用共享 `Whalebone.Systems.Events.EventLog`，`owner_system` 固定为 `EcologySystem`。事件去重键遵守 D8 规则 `event_id + save_revision`，同时将键保存在 `appliedEcologyEventKeys`，因此 Ecology 快照单独读档时也不会重复改变资源。

使用的事件 ID：

| 事件 | 结果码 | 事实内容 |
|---|---|---|
| `event_d11_ecology_advance_day_<day>` 或调用方指定 ID | `ecology_advanced` | 温度、浑浊度、污染、潮汐、海流、推进天数 |
| `event_d11_plant_harvest_<plant>_<day>` 或调用方指定 ID | `plant_harvested` | 植物 ID、采集量、剩余数量 |
| `event_d11_pollution_<source>_<day>` 或调用方指定 ID | `pollution_applied` | 来源、污染增量、污染和浑浊度结果 |
| `event_d11_ecology_restore_day_<day>` 或调用方指定 ID | `ecology_restored` | 恢复系数、污染结果、恢复种子 |

事件日志只追加已经由 EcologySystem 写入后的事实；不会替代生态系统裁决数值。事件中的 `input_facts`、`result.fact_ids` 和 `cause_ids` 可供 D9/D11 UI、任务和 Agent 做只读追踪。

## 6. 保存、读档和版本兼容

`SaveSystem.D4ToObj/D4FromObj` 已扩展 `location_json.d4.ecology`：

- 保存 D11 生境条件、植被资源、鱼群新字段、动物压力、生态修订号和应用事件键；
- 旧档缺少 D11 字段时使用安全默认值，不生成虚假的鱼群数量；
- 读档后用恢复的 `appliedEcologyEventKeys` 阻止同一污染、恢复、采集或推进事件再次写入；
- 共享 `EventLogSnapshot` 仍由 D3 runtime 负责保存，生态状态不会复制另一份事件日志。

## 7. 验证

已执行：

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet run --project tools/d11_a01_smoke/D11A01Smoke.csproj
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
rg -n "HttpClient|System.Net.Http|LLM|OpenAI|UnityWebRequest|WebRequest|Uri\(" game/Assets/Scripts/Systems -g "*.cs"
```

结果：

- `Whalebone.Systems`：0 warning / 0 error；
- D11-A01 smoke：PASS，覆盖生境、植物、动物、鱼群、捕捞压力、天气/潮汐/海流、污染、恢复和事件去重；
- 内容目录检查：`35 entries / 13 prefixes`，D11 使用的代表 ID 均有正式登记；
- Systems 红线扫描：未发现新增可执行网络或 LLM 代码，命中的均为既有注释/边界说明；
- `Save.Tests.csproj`：未完成全量编译，工作区既有 `D10S01AcceptanceIntegrationTests.cs(195)` 缺失 `D5RuntimeSnapshot` using 阻断，与 D11 文件无关；D11 测试已加入 Unity EditMode 测试程序集，待该既有基线恢复后定向执行。

Unity EditMode 测试入口：`D11.Tests.D11EcologyStateMachineTests`，覆盖天气与污染、压力与恢复、植被采集、事件追踪、SQLite 保存读档和读档后重复保护。

## 8. 后续入口

D11-A02 可复用 `ReadConditions`、`PreviewFishCatch`、`HarvestPlant` 和 `AdvanceEcology` 扩展捕捞/采集，而不复制资源量公式。D11-A03 可复用 `FishSchoolState.recovery/depth/migration` 接入活鱼舱/养殖；D11-A04/D11-A05 可复用动物 `EcologyStress` 与 `HabitatId`。后续任务不得把 UI、Agent、EnvironmentSystem 或 SaveSystem 变成生态事实写者。
