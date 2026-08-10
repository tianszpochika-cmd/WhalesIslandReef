# D11-A06 交接报告

## 结果

D11-A06 已实现并通过 7 项独立宠物测试。宠物是非战斗单位；状态、位置和生命周期只有 `AnimalCareSystem`/`D11PetSystem` 写入。喂养只通过 `D3InventorySystem` 扣除饲料，船员反应只记录结构化表达事实。

## 契约版本与内容

- `D11PetRuntimeSnapshot.schema_version = 1`。
- `D11PetState`、`D11PetDefinition`、`D11PetCrewReaction`、`D11PetActionResult` 位于 `game/Assets/Scripts/Systems/D11/D11PetContracts.cs`。
- 内容 ID：`pet_cabin_cat`、`pet_harbor_dog`、`item_pet_feed`。
- 存档路径：现有 `location_json.d11Pet`；没有新增 server/web 协议。

## Action/事件清单

| 入口 | 事件 | 关键拒绝/恢复 |
|---|---|---|
| `Adopt` | `event_d11_pet_adopted` | `pet_unknown`、`source_missing`、`pet_already_owned`、`pet_dead_no_respawn` |
| `Feed` / `Clean` / `Comfort` | `event_d11_pet_care` | `pet_not_careable`、`invalid_quantity`、`not_enough_cargo`；重复结算退回饲料 |
| `MoveTo` | `event_d11_pet_location` | 只允许 Harbor/ShipCabin/ShipDeck |
| `Quarantine` / `ReleaseFromQuarantine` | `event_d11_pet_quarantine` | `pet_not_quarantineable`、`quarantine_incomplete` |
| `Treat` | `event_d11_pet_care` | 只有 Quarantined 可治疗 |
| `MarkLost` | `event_d11_pet_lost` | `Interrupted`，恢复路线 `pet_search_or_shelter` |
| `Search` | `event_d11_pet_search` | 工具/地点缺失拒绝；未找到保留原实例，找到用 `AppendRecovery` |
| `CatchMouse` | `event_d11_pet_mouse_caught` | 仅猫；不写战斗结果 |
| `Warn` | `event_d11_pet_warning` | 只写 `observation_only=true`，不写天气/航行结果 |
| 船员反应 | `event_d11_pet_crew_reaction` | 读取 Persona，只写表达投影，不改 Crew 状态 |
| `AdvanceDay` | `event_d11_pet_daily_tick` | 日期不可倒退；健康归零为 Dead，拒绝重生 |

## 跨端同步文件

- 运行时契约/裁决：`game/Assets/Scripts/Systems/D11/D11PetContracts.cs`、`D11PetCatalog.cs`、`D11PetSystem.cs`。
- 存档接线：`game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`、`GameState.cs`、`SaveSystem.cs`。
- 内容注册：三个 D11 Pet/饲料 JSON 入口。
- 测试：`game/Assets/Tests/EditMode/D11PetSystemTests.cs`、`tools/d11_a06_runner/`。
- 文档：`docs/game/design/common/D11_PETS_CATS_DOGS.md`。

## 验证命令与结果

1. `dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162 -v:minimal` —— PASS。
2. `dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162 -v:minimal` —— PASS。
3. `dotnet run --project tools/d11_a06_runner/D11A06Runner.csproj --no-restore` —— `D11-A06 tests PASS count=7`。
4. `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` —— `Content registry OK: 77 entries, 13 prefixes`。

## 基线修复记录

工作区原有 D11-A02/A03/A04 与 D12 测试/存档接线存在最小编译兼容缺口；本任务只补齐了已有 `D11FishingHarvestTests` 的配置命名空间、`D11AquacultureRuntimeSnapshot` 项目入口、保留其独立字段，并补齐已有 D12 DTO 的 Save 编解码/`D12ToolResult.Fact` 兼容字段。没有复制 D12/D11 领域逻辑，也没有修改网络、LLM 或 UI 裁决边界。
