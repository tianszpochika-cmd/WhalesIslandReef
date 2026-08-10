# D11-S01 交接报告

任务：`D11-S01` 生态与动物同步集成  
日期：2026-08-11  
前置：D11-A01、D11-A02、D11-A03、D11-A04、D11-A05、D11-A06、D11-A07

## 1. 交付结论

D11-S01 已完成，D11 集成验收 **通过**。8 项集成测试全部通过，D11-A06/A07 定向测试、D11-A01～A04 smoke 和 D9-S02 门禁均通过。D13 可使用 D11 的生态/采集快照、目录和事实，但不能取得 D11 状态写权限。

## 2. 新增文件

### 生产代码

- `game/Assets/Scripts/Systems/D11/D11EcologyVoyageIntegrationContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11EcologyVoyageIntegrationSystem.cs`

该层是跨 owner 事实编排层，不是新的生态、动物、库存或关系系统。它不保存第二份 canonical 数值。

### 测试与执行入口

- `game/Assets/Tests/EditMode/D11AcceptanceIntegrationTests.cs`：8 项集成验收；
- `tools/d11_s01_runner/D11S01Runner.csproj`；
- `tools/d11_s01_runner/Program.cs`。

`Whalebone.Systems.csproj` 和 `Save.Tests.csproj` 已登记集成代码/测试。

## 3. 新接口

```text
D11EcologyVoyageIntegrationSystem.RescueRareAnimal(D11RescueRequest)
D11EcologyVoyageIntegrationSystem.ApplyPetCrewRelations(D11PetActionResult)
```

### `RescueRareAnimal`

- 先调用 `D11RareAnimalCollectionSystem.Rescue`，图鉴、动物生命周期和救助生态事实仍由原 owner 写入；
- 读取共享 `EventLog` 的救助事实；
- 调用 `D4FactionSystem.ApplyReputation("england", +5, ...)`，只在对应事实不存在时发生；
- 追加 `FactionSystem` owner 的 `wildlife_rescue_recognized` 事实；
- 追加 `QuestSystem` owner 的 `wildlife_rescue_completed` 事实；
- 同一 `event_id + save_revision` 再次请求返回 `duplicate_settlement`，不重复声望/任务结算。

当前仓库没有独立 QuestSystem 实现，因此这里只发布任务 owner 事实，不维护第二套任务状态。后续任务系统应消费 `quest:wildlife_rescue_completed` 和 `quest_id=quest_wildlife_rescue_saint_marys`。

### `ApplyPetCrewRelations`

- 读取 `D11PetSystem.CrewReactionEventId` 已提交事实；
- 把结构化反应映射为确定性 `Attachment` 提议；
- 只能通过 `RelationSystem.ApplyFactEvent` 写关系；
- 关系系统负责角色引用、事实因果、值域边界和重复键；
- 不直接改 `RelationEdge`，也不复制 D10 关系公式。

## 4. 验收覆盖

| 测试 | 覆盖 |
|---|---|
| `WeatherAndTideChangeFishSchoolProjection` | 天气/潮汐改变鱼群水文投影和恢复 |
| `FishingChangesFishPressureAndEconomyPriceInputs` | 捕捞降低鱼群、增加压力，并通过 Economy 改变价格输入 |
| `LiveFishHoldRestoreDoesNotDuplicateSettlement` | 活鱼舱读档重放不重复加鱼获/压力 |
| `WildlifeRescueUpdatesCodexFactionAndQuestFact` | 救助更新图鉴、声望、任务事实，重复不加声望 |
| `PetFactsEnterCrewAttachmentRelations` | 宠物船员反应进入 D10 attachment 关系，重复不叠加 |
| `VegetationHarvestEntersCargoAndChangesEcology` | 植被产物进入 D3 Cargo，Ecology 数量下降 |
| `AnimalBehaviorIsDeterministicWithoutLanguageModel` | 动物行为同输入同结果，无 LLM |
| `EcologyAndAnimalSnapshotsRestoreWithFacts` | 生态/动物快照恢复，救助重放不重复 |

执行结果：

```text
D11-S01 integration tests PASS count=8
D11-A06 tests PASS count=7
D11-A07 tests PASS count=6
D11-A01/A02/A03/A04 smoke PASS
D9-S02 PASS: blocked=0
```

`Whalebone.Systems`、`Save.Tests` 和 S01 runner 均以 0 warning / 0 error 编译通过。

## 5. 所有权与 UI/Agent 边界

- `D3InventorySystem` 是货舱/库存唯一写者；
- `D4EcologySystem` 是生态数量、压力和恢复唯一写者；
- `D11LiveFishHoldSystem` / `D11IslandAquacultureSystem` 是活鱼舱/养殖状态 owner；
- `D11PetSystem` 是宠物状态 owner；
- `D11RareAnimalCollectionSystem` 是稀有动物收集/图鉴 owner；
- `D4FactionSystem` 是声望 owner；
- `RelationSystem` 是关系唯一写者；
- UI 只读结果 DTO、快照和事件；
- Agent 只能提议请求和表达，不能生成资源、改生态、改鱼获、改宠物、改关系或发奖励；
- D11 Systems 没有新增网络或 LLM。

## 6. 保存/读档交接

S01 没有新增第二套存档。它依赖：

- D3 Cargo 快照；
- D4 Ecology 与 `AppliedEcologyEventKeys`；
- D11 活鱼舱、宠物、鲸豚路线和稀有动物快照；
- 共享 `EventLogSnapshot`。

读档后使用原 owner 的 `Restore`，再消费相同事实会被 `event_id + save_revision` 或 RelationSystem 的 `AppliedEventKeys` 拒绝，不重复写库存、生态、声望、任务或关系。

## 7. 遗留问题

当前内容目录检查由并行 D12 新内容导致 6 项错误：`asset_historical_saint_marys_log` 与 `item_special_weapon_moonwake_cutlass` 尚未接入注册根，且 owner/test_id 校验不匹配。D11-S01 未修改 D12 文件；D12-A07 需单独修复。

这不阻断 D11-S01 的代码、事实链、保存/恢复和 8 项集成验收。D13 可以在上述边界下使用 D11 生态/采集数据。
