# D21-L09 逻辑交接

## 已完成

- 任务：`D21-L09`
- 状态：`LOGIC_PASS`，范围仅为纯 C# 出航适配逻辑。
- 新增 `D21PortDepartureRequest`、`D21PortDepartureResult` 和 `D21VoyageStartEffect`。
- `D21PortDepartureAdapter` 只构造公开 `D19RoutedPortCommand`，并通过注入的 `D19PortCommandRouter.Route` 请求 `Depart`。
- D19 的成功、拒绝和幂等结果均保留；成功结果只投影为 D21 只读航程启动效果，不复制或改写港口、泊位、船体、货物、船员或补给事实。
- 输入检查覆盖空引用、稳定 ID、SchemaVersion、世界日、存档修订和引用字符边界。

## 真实上游证据

- D19-L48 已通过；当前公开类型是 `D19RoutedPortCommand` / `D19RoutedCommandResult`，不是旧的 `D19PortCommandResult` 路由结果。
- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke 先用 D19 `EnterPort -> Dock -> Board` 建立合法状态，再经适配器出航；首次 `Success=true`，重复同一 `CommandId` 返回 `IsDuplicate=true`。

## 进行中 / 未宣称

- D21 航程 owner 尚未在本任务中接管 `VoyageStart` 并推进船体、船员、货物或补给事实。
- 没有修改 D19 路由器、港口访问状态机或共享状态总览。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. 上游编排必须继续把 D19 作为港口/泊位唯一写者；不得从适配器结果反推或重写 D19 状态。
2. `RouteId` 与 `DestinationPortId` 是 D21 后续 owner 的不透明引用，实际航线/抵达裁决必须由对应 owner 完成。
3. 任何船况、船员、D3 库存和航程补给的副作用都要通过各自公开入口完成，不能塞进本适配器。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21PortDepartureAdapter.cs`
- `game/Assets/Scripts/Systems/D21/D21PortDepartureAdapter.cs.meta`
- `game/tools/results/D21/D21-L09/D21_L09_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L09_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- D19 Route 出航幂等 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L09 已完成出航适配器；只构造 D19RoutedPortCommand 并调用 D19PortCommandRouter.Route，D19 返回结果映射为只读 VoyageStart，重复命令可重放且不重复写港口事实。`
