# D11-A02 交接报告

日期：2026-08-11  
任务：D11-A02 钓鱼与海上采集  
前置：D9-S02、D11-A01

## 交付结论

D11-A02 的纯 C# 捕捞/浅水采集闭环已完成。手线、岸钓、撒网、鱼笼、贝类耙、海草钩均可提交请求；鱼种、深度、时间、潮汐、渔具、天气和捕捞压力会形成可解释差异；处理品会进入 D3 货舱并通过 EconomySystem 出售。保护区阻断、资源压力、品质和读档幂等均有事件或快照证据。

## 新增配置

| 配置入口 | 新增登记 |
|---|---|
| `game/Assets/Scripts/Systems/D11/D11FishingHarvestConfiguration.cs` | `d11_harvest_depth_meters`：m，默认 2，边界 0—30，owner EcologySystem；`d11_gear_capacity`：无量纲，默认 2，边界 1—20；`d11_fish_quality_score`：%，默认 50，边界 0—100；`d11_processing_yield_percent`：%，默认 100，边界 1—100，owner InventorySystem；`d11_shallow_resource_capacity`：无量纲，默认 8，边界 1—100；`d11_fish_school_capacity`：无量纲，默认 12，边界 1—100。每项均登记 source 和 `D11A02_FishingHarvestTests`。 |
| 统一入口复用 | 金额、重量、体积、世界日、时间段和品质枚举继续使用 D8 `NumericConfiguration` / `QualityGrade`，没有在 D11 复制另一套枚举。 |

非法配置入口：`D11FishingHarvestConfiguration.Validate()` / `ThrowIfInvalid()`。

## 新增代码与数据

- `D4Contracts.cs`：为捕捞预览增加鱼种、深度、时段、潮汐、渔具、品质和保护区追踪字段；浅水资源定义/状态增加 `ResourceKind`；操作结果增加资源剩余量与压力字段。
- `D4EcologySystem.cs`：增加带鱼种/深度/时间/潮汐/渔具/保护区输入的只读 `PreviewFishHarvest`，以及带鱼种深度/容量的 `SeedFishSchool` 重载；仍只有 `CommitFishCatch` 写鱼群。
- `D4EcologyD11StateMachine.cs`：增加海草/贝类资源目录、资源补齐入口 `EnsureD11ShallowResources` 和显式 `HarvestShallowResource`；状态仍由 EcologySystem 写入。
- `SaveSystem.cs`：保存/读档 `PlantResourceState.ResourceKind`，旧档默认回退为 `plant`。
- `D11FishingHarvestContracts.cs`：捕捞、处理、装备、地点、结果和快照契约。
- `D11FishingHarvestCatalog.cs`：六类渔具、三种鱼群/鱼种、三个鱼点、两个浅水资源、市场物品和腌制/鱼干/鱼油/腌贝肉/海草干映射。
- `D11FishingHarvestSystem.cs`：跨 Ecology/Inventory/Economy 的请求编排、回滚、出售、事件和 `event_id + save_revision` 幂等；无 Unity、网络或 LLM。
- `D11FishingHarvestTests.cs`：配置、鱼种差异、浅水资源、处理品、市场反馈、保护区和读档不重复测试入口。
- `tools/d11_a02_smoke/`：独立纯 C# smoke，覆盖验收主链，便于在 Unity 之外验证。
- 内容目录新增/补齐 D11 鱼群、鱼具、贝类、海草、鲜鱼和处理品 ID；所有条目已登记来源、owner、阶段、许可和唯一 D8 test_id。

## 替换的硬编码

- 原 D4 鱼群初始化只支持固定鲭鱼字段；改为保留 D9 旧重载，并新增由 EcologySystem 写入鱼种、偏好深度、迁徙和容量的重载。
- 原 D4 仅按稳定哈希和容量预览鱼获；D11 入口改为在同一 EcologySystem 内叠加捕捞压力、天气、深度、时间、潮汐和渔具因素，并把质量/压力写入预览和事实。
- 原 D11-A01 植物目录只有 `plant_red_mangrove`；增加 `plant_shallow_seaweed` 与 `plant_conch_bed`，通过 `ResourceKind` 区分海草/贝类，数量和压力仍复用原 EcologySystem 状态表。
- 原 D9 处理入口只处理鲭鱼盐渍；D11 处理映射统一收口到 `D11FishingHarvestCatalog.Processing`，避免在系统方法中散落输入/输出物品硬编码。
- 原 D9 市场注册只覆盖鲜鲭/盐渍鲭；D11 将新增鲜鱼、采集物和处理品通过 `TradeEconomySystem.RegisterMarketItem` 注册，价格仍由 EconomySystem 生成，未改动既有 D9 鲭鱼价格。

## 事件与存档

- `event_d11_fishing_harvest`：鱼种、物品、数量、品质、深度、时段、潮汐、渔具和 `fishing_pressure_after`。
- `event_d11_shallow_resource_harvest`：EcologySystem 写资源剩余量和采集压力。
- `event_d11_fish_processing`：InventorySystem 写输入/输出物品与数量。
- `event_d11_harvest_rejected_protected`：保护区规则 ID、位置、渔具、是否有许可。
- `D11FishingHarvestRuntimeSnapshot` 保存修订号/最后结果/共享 EventLog；生态量仍保存在 `D4RuntimeSnapshot.Ecology`。恢复快照后同一请求不会再次增加库存或扣鱼群生物量。

## 检查命令与结果

```text
dotnet run --project tools/d11_a02_smoke/D11A02Smoke.csproj
```

结果：

```text
D11-A02 smoke PASS: species/depth/time/tide/gear/shallow/processing/market/protection/idempotency
```

```text
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

结果：`Content registry OK: 77 entries, 13 prefixes`。

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

结果：0 warning / 0 error。

```text
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

结果：0 warning / 0 error；`D11FishingHarvestTests` 已登记并编译。

红线检查：

```text
rg -n "HttpClient|System.Net.Http|LLM|OpenAI|UnityWebRequest|WebRequest|Uri\(" game/Assets/Scripts/Systems -g "*.cs"
```

新增 D11 捕捞代码无网络或 LLM 代码；命中项仅为工作区既有边界说明/其他任务内容。

## 待办与后续使用

- 在 Unity Editor 中执行 `D11.Tests.D11FishingHarvestTests` EditMode 全套；纯 C# smoke 和测试程序集编译已通过。
- D11-A03 可直接读取 `FishSchoolState.Depth/Migration` 与捕捞压力，但不得复制捕捞数量公式。
- D13 可以使用鱼获、处理品、市场反馈和生态事实；价格、库存、资源量仍必须通过各自 owner 入口读取/写入。
