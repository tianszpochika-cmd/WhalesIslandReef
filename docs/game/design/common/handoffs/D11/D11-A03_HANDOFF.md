# D11-A03 任务交接报告

日期：2026-08-11  
任务：D11-A03 活鱼舱与岛上水产

## 交付结论

已完成代表性活鱼舱、鱼塘和近岸网箱逻辑。活鱼舱与岛上养殖使用两个独立唯一写者；鱼苗、饲料和鱼货均通过 InventorySystem；养殖生态压力由 EcologySystem 接收和裁决；事件先写入 D11 EventLog，再提交所属状态。状态、事实日志和重复键可通过 D11 水产存档快照恢复。

## 事件字段与所有权

所有事件使用 D8 `WorldEvent/FactEvent` 字段：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`save_revision`；失败/中断事件如由上层扩展，继续使用统一 `recovery_route` 规则。

| 字段/状态 | 唯一写者 |
|---|---|
| 活鱼舱鱼群、容量、重量、体积、氧气、水质、拥挤、维护、死亡 | `D11LiveFishHoldSystem` |
| 鱼塘/网箱实例、鱼群、饲料、劳力、水质、维护、死亡 | `D11IslandAquacultureSystem` / Building-Aquaculture owner |
| 鱼货库存与货舱容量 | `D3InventorySystem` |
| 生态压力、污染、浑浊度、近岸鱼群压力 | `D4EcologySystem` / `EcologySystem` |
| 事实日志与保存投影 | `EventLog` / `SaveSystem`；只追加/序列化，不裁决领域结果 |

## 重复结算保护

每个系统在状态中保存 `applied_event_keys`，键为 `event_id + save_revision`；同时查询 `EventLog.Find`。维护、死亡、鱼苗入舱、建筑输入和收获在事实已存在时返回 `duplicate_settlement`，不会再次扣鱼苗/饲料、增加鱼货、扣鱼或增加生态压力。读档通过 `EventLog.FromSnapshot` 恢复事实日志和 settlement claims，再构造系统即可继续保护。

## 新接口

- `D11LiveFishHoldSystem.StockFry / Maintain / Harvest / CaptureRuntime`；
- `D11IslandAquacultureSystem.Build / SupplyInputs / Advance / Harvest / CaptureRuntime`；
- `D4EcologySystem.PreviewAquaculturePressure / ApplyAquaculturePressure`；
- `SaveSystem.SaveD11AquacultureRuntime / LoadD11AquacultureRuntime`；
- `D11AquacultureRuntimeSnapshot`：`LiveFishHold`、`IslandAquaculture`、`EventLog`。

## 测试命令与结果

```powershell
dotnet run --project tools/d11_a03_smoke/D11A03Smoke.csproj
```

结果：`D11-A03 smoke PASS: live-hold/pond/cargo/pressure/events`。

```powershell
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

A03 接入后的定向 Systems 源编译结果为 `0 warning / 0 error`；D11 A03 定向源编译和 smoke 仍通过。

截至交接前的当前重跑结果：全量 `Whalebone.Systems` 与 `Save.Tests` 被工作区并行的 D11-A05 变更阻断，错误为 `D11PetSystem.cs(330)` 在同一范围重复声明 `lostRevision`；A03 未修改该文件。D11 A03 smoke、AgentCore 构建和 D9-S02 门禁均已通过。

Unity EditMode 测试已登记：`D11.Tests.D11AquacultureTests`。本轮未启动 Unity EditMode 全量运行，避免打断工作区中已有 Unity 进程。

## 已知边界

- 仅交付 mackerel、活鱼舱、基础淡水鱼塘和基础近岸网箱样例；完整内容目录和正式建筑产权/税费由后续内容/经营任务接入。
- 船舶总重量汇总、Economy 价格和 UI 面板只可读取本任务快照，尚未在本任务中扩展无关系统。
- EcologySystem 的生态压力结果保存在 D4 Ecology 快照；保存完整水产状态时必须同时保存 D11 水产快照和 D4 快照。
- 当前全量构建受并行 D11-A05 的 `D11PetSystem.cs(330)` 重复局部变量阻断，未在本任务中修改；修复该基线错误后应重新运行 `Whalebone.Systems` 和 `Save.Tests`。

## 变更文件

- `game/Assets/Scripts/Systems/D11/D11AquacultureContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11AquacultureMath.cs`
- `game/Assets/Scripts/Systems/D11/D11EcologyAquaculturePressure.cs`
- `game/Assets/Scripts/Systems/D11/D11LiveFishHoldSystem.cs`
- `game/Assets/Scripts/Systems/D11/D11IslandAquacultureSystem.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Whalebone.Systems.csproj`、`game/Save.Tests.csproj`
- `game/Assets/Tests/EditMode/D11AquacultureTests.cs`
- `docs/game/design/common/D11_LIVE_FISH_AQUACULTURE.md`
