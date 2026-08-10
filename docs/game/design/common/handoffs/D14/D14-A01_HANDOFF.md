# D14-A01 交接报告：海战裁决

日期：2026-08-11  
任务：D14-A01  
前置：D13-S01 已通过

## 已交付

- `game/Assets/Scripts/Systems/D14/D14NavalCombatContracts.cs`
  - 战斗状态、运动学、炮位、弹种、分区、损伤、请求、结果和存档快照契约。
- `game/Assets/Scripts/Systems/D14/D14NavalCombatSystem.cs`
  - 距离、相对角度、风向/风速、航速差、能见度和炮手技能的确定性命中公式；
  - Round/Chain/Grape 三种弹种，炮位装填、瞄准、命中和分区损伤；
  - 火灾、进水、火药风险、桅帆/舵/炮位/船员/船体状态；
  - 回合推进、撤退失败恢复路线、撤退成功、投降和共享 EventLog 事实。
- `game/Assets/Scripts/Systems/Save/GameState.cs`
  - 增加 `CurrentD14Runtime` 运行时投影。
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
  - 增加 `D14Runtime` 玩家存档投影。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
  - 增加 `SaveD14Runtime`/`LoadD14Runtime`，接入 `location_json.d14`，序列化天气、炮位、损伤和共享事件快照。
- `game/Assets/Tests/EditMode/D14NavalCombatTests.cs`
  - 2 项 NUnit 验收测试。
- `tools/d14_a01_runner/`
  - 独立执行入口，加载项目内 SQLite 原生库后运行 2 项测试。
- `docs/game/design/common/D14_NAVAL_COMBAT_RULES.md`
  - 规则、所有权、公式、事件和存档说明。

## 新接口/使用约定

1. 由 ShipSystem/EnvironmentSystem 提供带来源的只读 `D14CombatShipProfile` 与 `D9WeatherSeaSnapshot`。
2. 先在共享 `EventLog` 中存在天气事实，再调用 `StartEncounter`；缺少 `WeatherSourceEventId` 或来源事实会拒绝。
3. UI/Agent 只构造 `D14Combat*Request`；不要写 `D14NavalCombatState`，不要把命中率、伤害、投降结果塞入 AgentReply。
4. 每个操作使用新的 `OperationId` 与单调 `SaveRevision`；同一组合读档后重放只返回 `Duplicate`。
5. `SaveSystem.SaveD14Runtime(system.CaptureSnapshot())` 是落库入口；存档域只复制快照和事件日志，不执行公式。

## 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error。
- `dotnet run tools/d14_a01_runner/D14A01Runner.csproj --no-restore`：`D14-A01 tests PASS count=2`。
- 既有 D4 简化战斗源码未被替换；D14 没有网络、HttpClient、LLM 或 Agent 裁决代码。

## 边界与后续

- D14-A01 保存的是 CombatSystem-owned 战斗投影和事实；实际航行实体的 ShipState 仍由 ShipSystem 持有。后续 D14 集成若要把战斗结果映射到航行船体，应由 ShipSystem 接收本模块事实/authority，不得在 D14 或 UI 直接改 `ShipState`。
- 俘虏、战后物资、奖励、私掠/法律和历史审查不在 A01 裁决，等待 D14-A02—A06 的所属系统。
- `EventLogSnapshot` 是共享日志的序列化副本，运行时始终只有传入的一个 `EventLog` 写者；与 D3 统一存档时应由集成层复用同一日志快照，避免建立第二日志写者。
