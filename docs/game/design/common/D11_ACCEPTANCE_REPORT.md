# D11 生态与动物同步集成验收报告

任务：`D11-S01`  
日期：2026-08-11  
范围：D11-A01～D11-A07 的跨系统事实接入、保存/恢复和重复结算保护

## 1. 最终结论

**D11 通过。**

D11-S01 的 8 项集成验收全部通过，D11 生态、捕捞、活鱼舱、鲸豚路线、珍稀动物、宠物和植被采集可以通过既有 owner、快照和共享 `EventLog` 进行同步。D13 **允许使用生态/采集数据**，但只能读取 D11 的快照、目录和事实；地块、种子、灌溉、种植周期及 D13 自有奖励仍须由 D13 的唯一 owner 写入。

本报告不把工作区并行 D12 的目录错误冒充为 D11 失败：当前内容目录检查发现 D12 新增的两个条目未接入注册根，产生 6 项 D12 错误，详见第 6 节遗留问题。

## 2. 集成验收矩阵

| 验收项 | 验证内容 | 结果 |
|---|---|---|
| 天气/潮汐改变鱼群 | 同一生态种子在不同潮汐/天气下得到不同鱼群水文投影、浑浊度和恢复结果 | PASS |
| 捕捞影响鱼群和价格 | 捕捞降低鱼群资源并增加压力；销售通过 `TradeEconomySystem` 改变市场供给、玩家交易量和价格原因 | PASS |
| 活鱼舱读档不重复结算 | 恢复货舱、活鱼舱、生态与 `EventLog` 后重放同一捕捞事实，不重复结算鱼获或生态压力 | PASS |
| 救助珍稀动物改变图鉴/声望/任务 | `D11RareAnimalCollectionSystem` 写救助和图鉴；集成层消费同一事实，由 `FactionSystem` 写声望，并发出 `QuestSystem` owner 的完成事实 | PASS |
| 宠物状态进入船员关系 | `AnimalCareSystem` 产生结构化船员反应事实；集成层只将已存在事实交给 `RelationSystem` 唯一关系写入口 | PASS |
| 动物逻辑无 LLM | 双实例相同种子、天气、日期和输入得到相同生命周期、行为、健康和压力；Systems 红线检查无网络/LLM 依赖 | PASS |
| 植被采集进入库存并改变资源量 | 果实/蔬果/药材/纤维/木材/树脂/染料/毒物共用 D3 货舱写入口，采集事实由 Ecology 写入，植物数量下降 | PASS |
| 生态和动物状态保存/恢复 | D4 Ecology、D11 稀有动物图鉴、D11 宠物/活鱼舱快照和共享事实恢复后状态一致且不重复结算 | PASS |

对应独立测试：`game/Assets/Tests/EditMode/D11AcceptanceIntegrationTests.cs`，运行结果：`D11-S01 integration tests PASS count=8`。

## 3. 新增集成入口

新增：

- `game/Assets/Scripts/Systems/D11/D11EcologyVoyageIntegrationContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11EcologyVoyageIntegrationSystem.cs`
- `game/Assets/Tests/EditMode/D11AcceptanceIntegrationTests.cs`
- `tools/d11_s01_runner/`

### 3.1 珍稀动物救助事实链

`RescueRareAnimal(request)` 的顺序固定为：

1. 调用 `D11RareAnimalCollectionSystem.Rescue`，由其裁决救助、图鉴和生态状态；
2. 从共享 `EventLog` 读取已提交的救助事实；
3. 若不存在对应声望事实，调用 `D4FactionSystem.ApplyReputation`，再记录 `FactionSystem` owner 的 `wildlife_rescue_recognized` 事实；
4. 若不存在对应任务事实，记录 `QuestSystem` owner 的 `wildlife_rescue_completed` 事实，包含任务 ID、物种、声望事实和救助事实因果链；
5. 使用 `event_id + save_revision` 保护重复请求，不重复增加声望或追加任务完成事实。

当前代码库没有独立的任务数值系统，因此集成层只发布任务 owner 可消费的事实，不维护第二份任务状态，也不直接发奖励。

### 3.2 宠物关系事实链

`ApplyPetCrewRelations(petAction)` 只读取 `D11PetSystem` 已生成的 `CrewReactionEventId` 事实，然后将确定性关系意图提交给 `RelationSystem.ApplyFactEvent`。反应到依恋的增量规则固定在集成契约中：欢迎/缓解为 `+3`，照料支持/检疫恢复为 `+2`，预警/捕鼠认可/走失担忧为 `+1`，哀悼为 `-2`，未知反应默认 `+1`。最终关系值、边界、事实校验和重复保护仍由 `RelationSystem` 裁决。

## 4. 唯一写者与边界复核

| 数据 | 唯一写者 | D11-S01 行为 |
|---|---|---|
| 天气、潮汐 | `EnvironmentSystem` / D9 天气系统 | 只读输入；不在 D11 集成层重算天气 |
| 鱼群、植物、生态压力和恢复 | `D4EcologySystem` | 捕捞/采集只提交领域入口；恢复仍由 Ecology 裁决 |
| 货舱和库存 | `D3InventorySystem` | 只调用 `TryAddCargo`、`TryRemoveCargo`；不维护缓存库存 |
| 市场价格、市场库存、金币 | `TradeEconomySystem` | 捕捞销售只委托经济系统 |
| 活鱼舱/鱼塘 | D11-A03 对应系统 | 只读/转交已有结果；不复制鱼舱状态 |
| 鲸豚路线 | `D11WhaleDolphinSystem` | 只消费其事实；不复制迁徙、捕鲸或生态压力逻辑 |
| 珍稀动物与图鉴 | `D11RareAnimalCollectionSystem` | 先由原系统裁决救助，再消费救助事实 |
| 宠物 | `D11PetSystem` / `AnimalCareSystem` | 只消费宠物及船员反应事实 |
| 船员关系 | `RelationSystem` | 唯一关系写入口，负责边界、事实引用和去重 |
| UI | UI/Presentation | 只读取结果 DTO、快照和 `EventLog`，无写入口 |
| Agent | Agent/AgentReply | 只能提出请求或表达，不能裁决生态、鱼获、宠物、库存、关系或奖励 |

`game/Assets/Scripts/Systems/` 未加入网络、HTTP 或 LLM；D9-S02 的 `systems_redline` 检查通过。

## 5. 保存、恢复和幂等

- 生态资源数量、压力、恢复日和 `AppliedEcologyEventKeys` 由 D4 快照保存；
- 货舱由 D3 快照保存；活鱼舱/鱼塘、宠物和鲸豚路线沿用各自 D11 快照；
- 珍稀动物图鉴与救助状态沿用 Ecology/D11 rare-animal 快照；
- 事实链统一保存在共享 `EventLogSnapshot`，集成层不新增第二份结算账本；
- 恢复后，捕捞、采集、救助、声望、任务和船员关系均按事实键拒绝重复结算；
- 测试覆盖活鱼舱重复、植被重复、救助声望/任务重复、宠物关系重复和生态/动物快照恢复。

## 6. 验证记录与遗留问题

已通过：

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162   # 0 warning / 0 error
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162          # 0 warning / 0 error
dotnet build tools/d11_s01_runner/D11S01Runner.csproj --no-restore                   # 0 warning / 0 error
dotnet run --project tools/d11_s01_runner/D11S01Runner.csproj --no-build             # 8/8 PASS
dotnet run --project tools/d11_a06_runner/D11A06Runner.csproj --no-build             # 7/7 PASS
dotnet run --project tools/d11_a07_runner/D11A07Runner.csproj --no-build             # 6/6 PASS
dotnet run --project tools/d11_a01_smoke/D11A01Smoke.csproj --no-build                # PASS
dotnet run --project tools/d11_a02_smoke/D11A02Smoke.csproj --no-build                # PASS
dotnet run --project tools/d11_a03_smoke/D11A03Smoke.csproj --no-build                # PASS
dotnet run --project tools/d11_a04_smoke/D11A04Smoke.csproj --no-build                # PASS
python tools/d9_s02_acceptance_check.py                                               # D9-S02 PASS
git diff --check                                                                       # 无差异错误
```

当前工作区非 D11 遗留：`python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` 报告 D12 新增的 `asset_historical_saint_marys_log` 和 `item_special_weapon_moonwake_cutlass` 未接入注册根，合计 6 项 owner/test/orphan 错误。该内容与本任务无关，本任务未修改或删除 D12 文件；D12-A07 需补注册根/owner/test 绑定后再恢复全目录绿色。

敏感内容审查未跳过：D11-A04 的捕鲸分支继续保留 `history_1675_caribbean_whaling` 与 `sensitive_whaling_non_glorifying_review_required` 标签，D11-S01 未新增捕鲸收益或弱化审查的路径。

## 7. D13 使用许可

**允许 D13 使用生态/采集数据。** 推荐读取：

- D4 Ecology 的植物/鱼群数量、成熟度、压力和恢复快照；
- D11 目录中的资源、工具和产物 ID；
- `plant_harvested`、保护区拒绝、毒物误采和恢复事实；
- D11 采集结果 DTO 与共享 `EventLog` 读模型。

D13 不得直接改写 D4 生态数量、D3 Cargo、D11 采集压力、宠物/鱼舱状态或 D10 关系；D13 的种植地块、种子、灌溉、农作周期和自身奖励必须建立在 D13 的 owner 与契约下。
