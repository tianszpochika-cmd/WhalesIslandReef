# D21-L11 逻辑交接

## 已完成

- 任务：`D21-L11`
- 状态：`LOGIC_PASS`，范围仅为天气输入契约和校验。
- `D21WeatherInputContracts` 冻结季节、风向、风力、降水（毫米/小时）、能见度比例、气压（hPa）、世界日、存档修订、事件/来源/位置引用和种子字段。
- 明确拒绝未知季节/风向、负数、NaN、Infinity、越界数值，以及 `Calm + 非零 WindForce` 组合；稳定失败码可被上游审计。
- `D21WeatherInputCommand` 只携带输入和 `D9.EnvironmentSystem` owner 标识，输入/命令均不提供天气快照写入口。

## 真实上游证据

- D9 `EnvironmentSystem` 仍是天气事实唯一写者；本任务不改 D9 类型或保存字段。
- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke 验证有效 Wet/West 输入通过，重复指纹稳定；Unknown season、Calm+positive wind、NaN visibility 均显式拒绝。

## 进行中 / 未宣称

- D9 公开天气快照到 `D21WeatherInput` 的具体编排/订阅接线未在本任务新增；不能把本 DTO 当成第二份天气事实。
- 不修改 `EnvironmentSystem`、共享状态总览或当天记忆。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. 后续适配必须从 D9 的公开结果建立输入，并保留来源/修订/世界日等审计字段。
2. 上游不得用默认值吞掉未知枚举、非有限浮点或负数；应把本契约的失败码传播给命令/事件 owner。
3. `D21WeatherInputCommand` 只能作为输入包，不得新增修改 D9 内部状态的旁路。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21WeatherInputContracts.cs`
- `game/Assets/Scripts/Systems/D21/D21WeatherInputContracts.cs.meta`
- `game/tools/results/D21/D21-L11/D21_L11_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L11_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- 有效/非法天气输入纯逻辑 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L11 已冻结季节/风向/风力/降水/能见度/气压等确定性天气输入与稳定失败码；D9.EnvironmentSystem 保持唯一天气写者，D21 输入包不提供写入方法。`
