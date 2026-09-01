# D21-L14 逻辑交接

## 已完成

- 任务：`D21-L14`
- 状态：`LOGIC_PASS`，范围仅为 D21 航程补给 ledger 和固定步消耗。
- `D21ProvisionConsumptionSystem` 独立持有每个航程的食物/淡水账本；D3/ShipState 库存不作为本系统的第二份可写事实。
- 每个请求先完整校验，再确定性计算船员基础消耗、天气倍率和显式损耗；短缺、库存匹配冲突、应用冲突和重复 tick 均原子拒绝。
- 记录饥饿/口渴阈值；食物或淡水耗尽时以 `VoyageFailed`、`Applied=true` 和稳定 depletion failure code 表达“本 tick 已提交但航程失败”。
- `ApplicationId` 相同且载荷指纹相同会重放原结果并返回 `IsDuplicate=true`；结果/账本通过 Clone 防止外部改写。

## 真实上游证据

- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke：首次消耗成功；同 ApplicationId 重放为 Duplicate；EnforceStockMatch 冲突返回 `d21.provision.stock_mismatch`；超额消耗返回 `d21.provision.insufficient_food_and_water`，账本不留下部分扣减。
- 无 Unity、网络、LLM、随机、系统时钟或外部库存写入。

## 进行中 / 未宣称

- D3 的装载/货物库存写入，以及 D3/ShipState 到本 ledger 的上游同步编排不在本任务实现。
- 本系统不会自动从 D3 读取或修改 `ShipState.Supplies`；需要上游明确提供初始快照和稳定 ApplicationId/TickIndex。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. 航程 owner 必须在首个 tick 提供可信的 `FoodUnits`/`WaterUnits` 初始快照，并在重试时复用同一 ApplicationId 和载荷。
2. D3 继续负责货物/库存事实；D21 只负责航程期间的补给账本，跨 owner 同步必须走后续公开编排。
3. 耗尽结果不是“无变化拒绝”：它已原子提交最后一批消耗并标记航程失败；上游应消费 `VoyageFailed`/`FailureCode`，不可继续提交普通 tick。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21ProvisionConsumptionSystem.cs`
- `game/Assets/Scripts/Systems/D21/D21ProvisionConsumptionSystem.cs.meta`
- `game/tools/results/D21/D21-L14/D21_L14_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L14_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- 补给幂等/冲突/短缺纯逻辑 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L14 已建立独立航程补给账本；固定步食物/淡水消耗与损耗支持幂等重放、冲突/短缺原子拒绝、饥饿/口渴警告和耗尽失败，D3 库存仍是外部 owner。`
