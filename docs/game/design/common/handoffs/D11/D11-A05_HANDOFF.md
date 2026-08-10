# D11-A05 交接报告：珍稀动物、收藏和图鉴

日期：2026-08-11  
任务：D11-A05  
前置：D9-S02 PASS、D11-A01 完成

## 交付结论

D11-A05 已完成代表性珍稀动物闭环：发现、识别、观察、记录、救助、合法转移、运输安全/死亡、保护区释放、合法收藏和非法交易后果均可保存、读档和追踪。代表样例为 D8 正式登记的 `animal_green_turtle`，没有批量生成动物、稀有奖励或无来源外部资源。

## ID 与目录

| 类型 | ID | 目录/状态字段 | 写者 |
|---|---|---|---|
| 物种 | `animal_green_turtle` | `game/Assets/Resources/ContentRegistry/entries/animal/animal_green_turtle.json` | EcologySystem（运行时动物） |
| 运行时动物映射 | `sea_turtle` | `D4RuntimeSnapshot.Ecology.Animals[]` | EcologySystem |
| 图鉴 | `codex_animal_green_turtle` | `RareAnimals.Book[]` | CollectionSystem |
| 证据 | `evidence_animal_green_turtle_<kind>_<day>_<index>` | `RareAnimals.Evidence[]` | CollectionSystem |
| 保护区 | `protected_area_saint_marys_coast` | `RareAnimals.ProtectedAreas[]` | CollectionSystem |
| 收藏家 | `collector_saint_marys_researcher` | `RareAnimals.Collectors[]` | CollectionSystem |
| 运输 | `transport_animal_green_turtle_<day>_<revision>` | `RareAnimals.Transports[]` | CollectionSystem 记录；死亡率由 EcologySystem 判定 |
| 法律势力 | `england` | `D4RuntimeSnapshot.Factions` | FactionSystem |

目录和运行时字段：

- 内容目录沿用 D8：`id`、`schema_version`、`display_name_key`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`owner_system`、`test_id`；
- 图鉴：首次地点、世界日、季节、行为、处理方式、证据 ID、当前状态和 `dead`；
- 证据：物种/动物、地点、世界日、季节、行为、处理方式、正式来源、证据引用、事实事件 ID、验证标记；
- 运输：起点/目的地、起始日、天数、已用天数、种子、死亡率、状态和最后更新日；
- 生态动物：生命周期、福利、运输天数、最后处理日、当前地点和正式来源。

## 修改文件和入口

- `game/Assets/Scripts/Systems/D4/D4Contracts.cs`：D11-A05 动物生命周期、图鉴、证据、保护区、收藏家、运输和保存契约；`D4RuntimeSnapshot.RareAnimals`。
- `game/Assets/Scripts/Systems/D4/D4EcologyD11RareAnimal.cs`：EcologySystem 唯一动物决策入口；不生成动物，确定性决定行为上下文、福利、生命周期和运输死亡。
- `game/Assets/Scripts/Systems/D4/D4FactionSystem.cs`：`RecordWildlifeOffense`，非法交易声望/通缉唯一写入口。
- `game/Assets/Scripts/Systems/D11/D11RareAnimalContracts.cs`：UI/Agent 可提交的请求 DTO 与结果 DTO。
- `game/Assets/Scripts/Systems/D11/D11RareAnimalCatalog.cs`：项目自有单个代表定义及正式来源引用。
- `game/Assets/Scripts/Systems/D11/D11RareAnimalCollectionSystem.cs`：图鉴/证据/收藏/保护区/运输记录唯一写者，统一事件日志入口。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：D4 动物扩展字段与 `RareAnimals` 的 JSON 保存/还原。
- `game/Assets/Tests/EditMode/D11RareAnimalCollectionTests.cs`：Unity EditMode 定向测试。
- `tools/d11_a01_smoke/D11A01Smoke.csproj`、`Program.cs`：D11-A01 + D11-A05 纯 C# smoke。
- `docs/game/design/common/D11_RARE_ANIMAL_COLLECTION.md`：主设计、字段、边界和后续入口。

## 检查命令与测试结果

```text
dotnet run --project tools/d11_a01_smoke/D11A01Smoke.csproj --no-restore
```

结果：

```text
D11-A01 smoke PASS: habitat/plant/animal/fish/pressure/weather/pollution/recovery/events
D11-A05 smoke PASS: discovery/identification/observation/rescue/legal-transfer/collection/release/illegal-trade/save-restore
```

Smoke 覆盖：

1. 发现 → 识别 → 观察，首条图鉴证据保存地点、季节和行为；
2. 救助 → 合法转移 → 运输 → 合法收藏；
3. 保护区释放路线与收藏路线结果不同；
4. 运输快照 Clone/恢复后，使用相同世界日、天气和种子得到相同结果，重复事件不再推进；
5. 非法交易不生成动物/奖励，但进入 `FactionSystem` 的声望和通缉后果。

已加入 Unity EditMode 测试程序集：`D11.Tests.D11RareAnimalCollectionTests`。当前工作区全量 `Whalebone.Systems/Save.Tests` 仍被既有 D12 改动阻断：`SaveSystem.cs` 的 `D12ToObj/D12FromObj` 重复定义，以及 `Whalebone.Systems.csproj` 的 D12 MapDiscovery 源重复包含；本任务专用 smoke 已通过，未修改 D12 或其他系统来掩盖该阻断。

阻断责任与建议：责任模块为 D12 MapDiscovery/Save 集成；由 D12 负责人删除生成项目中的重复源引用，并在 `SaveSystem` 只保留一组 `D12ToObj/D12FromObj` 后，重新执行 Systems、Save.Tests 和 Unity 定向测试。本任务没有跨边界修复该问题。

建议红线/内容检查：

```text
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
rg -n "HttpClient|System.Net.Http|LLM|OpenAI|UnityWebRequest|WebRequest" game/Assets/Scripts/Systems -g "*.cs"
```

## 写者边界与后续入口

- EcologySystem：`AnimalState` 的野外状态、行为、福利、运输死亡率；
- CollectionSystem：`RareAnimals` 图鉴、证据、合法转移、运输记录、收藏家和保护区；
- FactionSystem：非法交易声望和通缉；
- SaveSystem：只做快照序列化与还原；
- UI、Quest、Agent：只读结果/事件或提交请求，不得直接写图鉴、收藏、动物和法律结果。

D11 后续可从以下入口读取：

- 图鉴/证据/运输：`D11RareAnimalCollectionSystem.Snapshot`；
- 生态动物事实：`D4EcologySystem.Snapshot.Animals`；
- 事件因果：共享 `EventLog`；
- 内容正式来源：D8 Content Registry 的 `animal_green_turtle` 条目。

后续任务不得复制动物死亡率、图鉴写入或 Faction 法律裁决，也不得把未登记内容或未授权素材加入正式目录。
