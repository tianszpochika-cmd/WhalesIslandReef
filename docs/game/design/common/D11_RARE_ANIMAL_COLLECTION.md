# D11-A05 珍稀动物、收藏和图鉴闭环

状态：完成（2026-08-11）  
任务：D11-A05  
前置：D9-S02、D11-A01

本切片实现一条可追溯的野生动物路径：发现 → 识别 → 观察/记录 → 救助 → 合法转移 → 运输存活或死亡 → 释放/合法收藏；另有独立的非法交易尝试路径。样例只使用已有的正式内容 `animal_green_turtle`，不生成动物、不发放 Agent 奖励，也不批量伪造内容。

## 1. 代表样例与唯一写者

| 内容/状态 | ID | 唯一写者 | 说明 |
|---|---|---|---|
| 正式物种 | `animal_green_turtle` | `EcologySystem`（野外状态） / `CollectionSystem`（图鉴记录） | 映射现有生态运行时动物 `sea_turtle` |
| 代表地点 | `island_saint_marys` | 岛屿/世界数据 | 发现、救助、释放证据地点 |
| 保护区 | `protected_area_saint_marys_coast` | `CollectionSystem` | 允许观察、救助和释放；不允许无证转移 |
| 收藏家 | `collector_saint_marys_researcher` | `CollectionSystem` | `research` 接收方，只接受 `animal_green_turtle` |
| 生态动物状态 | `sea_turtle` | `EcologySystem` | 生命周期、福利、行为和运输死亡判定 |
| 法律/声望 | `england` 的声望与通缉 | `FactionSystem` | 非法交易统一调用 `RecordWildlifeOffense` |

正式内容来源是 `game/Assets/Resources/ContentRegistry/entries/animal/animal_green_turtle.json`，其 `source`、`license`、`historical_tag`、`phase`、`owner_system` 和 `test_id` 均由 D8 内容注册表扫描；本切片没有新增外部素材。

## 2. 运行时目录字段

`D4RuntimeSnapshot.RareAnimals` 是 D11-A05 的保存根，`CollectionSystem` 只写该子状态；`D4RuntimeSnapshot.Ecology.Animals` 仍由 `EcologySystem` 唯一写入。

```text
RareAnimals:
  schemaVersion, revision, lastEventId, appliedEventKeys[]
  book[]:
    entryId, speciesId, animalId,
    firstLocationRef, firstWorldDay, firstSeason, firstBehavior,
    handling, evidenceIds[], observed, identified, rescued,
    released, collected, dead, currentStatus, revision
  evidence[]:
    evidenceId, kind, speciesId, animalId,
    locationRef, worldDay, season, behavior, handling,
    sourceRef, evidenceRef, factEventId, verified
  transports[]:
    transportId, speciesId, animalId, fromLocationRef, toLocationRef,
    startWorldDay, daysInTransit, daysElapsed, seed, mortalityRisk,
    status, evidenceIds[], lastUpdatedDay
  collectors[]:
    collectorId, collectorType, buyerType, allowedSpeciesIds[],
    protectedAreaRef, reputationGate, evidenceRequired
  protectedAreas[]:
    areaId, locationRef, observationAllowed, rescueAllowed,
    releaseAllowed, transferAllowed, active, revision
```

每条图鉴或合法转移证据都必须包含：正式 `sourceRef`、地点、世界日、季节和 `evidenceRef`。`factEventId` 连接到共享 EventLog，`verified=true` 只由 CollectionSystem 在输入字段完整且生态决策已成功后写入。首次地点/季节/行为只在首条图鉴记录建立时写入，后续救助、释放、收藏不会抹除历史表现。

## 3. 系统入口与裁决边界

入口位于：

- `game/Assets/Scripts/Systems/D4/D4EcologyD11RareAnimal.cs`：野生动物的发现可用性、识别、观察、救助、转移开始、运输死亡、释放和收藏生命周期；福利与死亡率全部由 EcologySystem 决定。
- `game/Assets/Scripts/Systems/D11/D11RareAnimalCollectionSystem.cs`：证据完整性、图鉴首次记录、保护区、收藏家资格、合法运输记录、事件事实和非法交易请求。
- `game/Assets/Scripts/Systems/D11/D11RareAnimalCatalog.cs`：一个项目自有正式代表定义，不负责生成运行时动物。
- `game/Assets/Scripts/Systems/D4/D4Contracts.cs`：保存契约、动物生命周期、图鉴、证据、运输、收藏家和保护区字段。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：仅序列化/还原 `RareAnimals` 与 Ecology 动物扩展字段，不计算结果。

边界如下：

- UI/Agent 只能提交请求和证据引用，不能直接添加图鉴、收藏、动物或奖励；
- EcologySystem 是动物状态、行为、福利和运输死亡率的唯一写者；
- CollectionSystem 是图鉴、证据、收藏家、保护区和运输记录的唯一写者；
- FactionSystem 是非法交易声望和通缉后果的唯一写者；
- EconomySystem 没有被本任务复制或接管，非法交易不在本切片发放价格/奖励；
- Systems 新增代码没有网络、LLM、HttpClient 或 WebRequest。

## 4. 确定性运输死亡率

运输开始后，`D11AnimalTransportState` 保存 `seed`、运输天数和已推进天数。每个世界日由 EcologySystem 根据动物福利、生态压力、天气、能见度、海流和天数计算：

```text
risk = clamp(
  0.02 + (100 - welfare) * 0.004
  + ecologyStress * 0.25
  + storm ? 0.28 : rain ? 0.08 : 0
  + (1 - visibility) * 0.18
  + max(0, currentSpeed - 0.35) * 0.08
  + elapsedDays * 0.015,
  0.01, 0.95)
```

死亡判定使用 `animalId + worldDay + saved seed + elapsedDays` 的稳定哈希，不使用运行时随机数。死亡时生态动物进入 `Dead`、福利变为 0，CollectionSystem 将运输记录置为 `Died`，图鉴保留 `transport_mortality` 和原有证据。读档后用相同快照和天气继续推进，不会重复死亡或追加第二条事实。

## 5. 路径结果

| 路径 | 成功结果 | 可追溯状态 |
|---|---|---|
| 发现 | `animal_discovered` | 发现证据、首次地点/季节 |
| 识别 | `animal_identified` | 识别证据、物种身份 |
| 观察 | `animal_observed` | 生态行为、观察证据 |
| 救助 | `animal_rescued` | 福利、救助地点、处理方式 |
| 合法转移 | `animal_transfer_started` | 收藏家、目的地、许可证据、运输记录 |
| 运输安全 | `transport_survived`/`transport_arrived` | 每日运输事实、风险、状态 |
| 运输死亡 | `transport_mortality` | 死亡状态、风险、死亡日和证据链 |
| 释放 | `animal_released` | 保护区、释放地点和历史处理方式 |
| 收藏 | `animal_collected` | 合法接收方、收藏收据和图鉴状态 |
| 非法交易 | `illegal_wildlife_trade` | 拒绝生成动物/奖励；Faction 声望 -12、通缉 +15 |

事件均使用共享 EventLog 的 `event_id + save_revision` 去重键，并在 `RareAnimals.appliedEventKeys` 中留存。具体事件 ID 由 `event_d11_animal_<action>_<species>_<actionId|worldDay>` 或运输记录/世界日构成，可由错误信息和 ID 定位内容。

## 6. 验证

代表闭环 smoke：

```text
dotnet run --project tools/d11_a01_smoke/D11A01Smoke.csproj --no-restore
```

结果：

```text
D11-A01 smoke PASS: habitat/plant/animal/fish/pressure/weather/pollution/recovery/events
D11-A05 smoke PASS: discovery/identification/observation/rescue/legal-transfer/collection/release/illegal-trade/save-restore
```

Unity EditMode 定向测试：`D11.Tests.D11RareAnimalCollectionTests`，覆盖首次证据、合法收藏/释放、运输快照恢复与重复保护、非法交易法律后果。测试文件为 `game/Assets/Tests/EditMode/D11RareAnimalCollectionTests.cs`。

建议检查：

```text
dotnet build game/Whalebone.Systems.csproj --no-restore
rg -n "HttpClient|System.Net.Http|LLM|OpenAI|UnityWebRequest|WebRequest" game/Assets/Scripts/Systems -g "*.cs"
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

当前工作区全量 `Whalebone.Systems/Save.Tests` 仍受既有 D12 改动阻断：`SaveSystem.cs` 存在重复 `D12ToObj/D12FromObj`，项目文件也重复包含 D12 MapDiscovery 源文件；该基线错误不属于本任务，本任务代码已由专用 smoke 编译并通过。Unity 测试已登记，待基线恢复后可定向执行。

## 7. 后续入口

D11 后续任务可从 `D11RareAnimalCollectionSystem` 的请求/结果和 `RareAnimals` 快照读取图鉴、证据、收藏家和运输状态；从 `D4EcologySystem` 的 `AnimalState.Lifecycle/Welfare/EcologyStress` 读取动物事实。不得在 UI、Quest、Agent 或新系统中复制动物死亡率、图鉴写入或法律后果。
