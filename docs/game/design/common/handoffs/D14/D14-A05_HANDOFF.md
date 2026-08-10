# D14-A05 交接报告

## 任务

- 任务编号：D14-A05
- 目标：实现声望、战争、停战、私掠证、检查、通缉、护航、港口禁入、扣押、罚金、赎金、封锁和外交后果。
- 状态：完成。

## 修改文件

- `game/Assets/Scripts/Systems/D14/D14FactionLegalContracts.cs`
- `game/Assets/Scripts/Systems/D14/D14FactionLegalCatalog.cs`
- `game/Assets/Scripts/Systems/D14/D14FactionLegalSystem.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Assets/Tests/EditMode/D14FactionLegalSystemTests.cs`
- `game/Whalebone.Systems.csproj`
- `game/Save.Tests.csproj`
- `tools/d14_a05_runner/`
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_privateer_letter_of_marque.json`
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_blockade_order.json`
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_cargo_seizure_writ.json`
- `game/Assets/Resources/ContentRegistry/registry.json`
- `docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md`

## 新接口和边界

- `D14FactionLegalRuntimeSnapshot`：法律域完整保存快照。
- `D14FactionLegalOperationRequest` 及各领域请求：必须有来源事实、世界日、存档版本、证据和因果引用。
- `ID14FactionLegalCargoAuthority`：Inventory/Cargo 返回检查与扣押决定。
- `ID14FactionLegalEconomyAuthority`：Economy 返回罚金/赎金评估和结算确认。
- `D14FactionLegalSystem`：唯一写声望、关系、法律案件、封锁和港口状态；Agent/UI 无最终裁决接口。

## 验收结果

- `D14-A05 tests PASS count=2`；
- `Whalebone.Systems.csproj`：0 warning / 0 error；
- `Save.Tests.csproj`：0 warning / 0 error；
- 保存/读档保留关系、声望、私掠证、检查、通缉、扣押、罚金、赎金、护航、封锁、外交意图和共享事实日志；
- 同一操作键在恢复后返回 duplicate，不重复改变声望、法律或港口状态；
- 内容注册表通过；D14-A05 Systems 无网络、HTTP 或 LLM。

## 后续接线

1. D14-S01 可消费 A05 的只读法律快照和事实，不能复制声望、通缉、港口许可或金融案件。
2. 战斗/接舷结果应先追加 Combat/Aftercare 事实，再由 A05 的护航、扣押或外交入口裁决法律后果。
3. D15 任务或奖励只能提交法律来源请求；现金、库存、奖励、关系和战斗状态继续由各自 owner 写入。
