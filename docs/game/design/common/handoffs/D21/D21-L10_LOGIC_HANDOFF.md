# D21-L10 逻辑交接

## 已完成

- 任务：`D21-L10`
- 状态：`LOGIC_PASS`，范围仅为纯 C# 返港/请求靠泊适配逻辑。
- 新增 `D21PortReturnRequest`、`D21PortReturnResult` 和 `D21VoyageEndEffect`。
- `D21PortReturnAdapter` 先通过公开 `D19PortCommandRouter.Route` 路由 `Return`；有 `TargetBerthId` 时，再以 `CommandId + ":berth"` 派生稳定命令路由 `AssignBerth`。
- 返回和靠泊结果分开暴露：`VoyageEnded` 可以在靠泊失败时为真，`Success` 只有所有声明步骤都成功时才为真，避免中途失败被伪装成整体成功。
- 适配器不访问 D19 `VisitSystem`、`BerthSystem` 的内部状态，也不保存泊位事实。

## 真实上游证据

- D19-L48 已通过；使用当前公开 `D19RoutedPortCommand` / `D19RoutedCommandResult`。
- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke 先建立 `Departed` 状态，再调用返港并请求真实目录中的泊位；`ReturnResult.Success=true`、`BerthResult.Success=true`，最终状态为 `Berthed`。

## 进行中 / 未宣称

- 返港后重新进入 D19 访问循环、船体/船员/货物/补给接线不在本任务实现范围。
- 不修改 D19 路由器、生命周期表、泊位系统或共享状态总览。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. `Return` 与可选 `AssignBerth` 是两个独立 D19 命令；靠泊失败时上游必须按两个结果分别处理，不能回滚或假造 D19 事实。
2. D19 继续拥有港口/泊位事实；D21 只消费 `VoyageEnd` 和公开路由结果。
3. 如需靠泊重试，应复用稳定目标和命令语义，让 D19 幂等层决定重放或继续处理。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21PortReturnAdapter.cs`
- `game/Assets/Scripts/Systems/D21/D21PortReturnAdapter.cs.meta`
- `game/tools/results/D21/D21-L10/D21_L10_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L10_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- D19 Return + AssignBerth 纯逻辑 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L10 已完成返港适配器；Return 与可选 AssignBerth 分别通过 D19PortCommandRouter.Route，结果保留返港/靠泊边界，最终泊位事实只认 D19 结果。`
