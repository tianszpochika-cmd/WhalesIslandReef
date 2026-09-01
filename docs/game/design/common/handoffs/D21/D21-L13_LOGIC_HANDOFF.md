# D21-L13 逻辑交接

## 已完成

- 任务：`D21-L13`
- 状态：`LOGIC_PASS`，范围仅为天气影响的只读规则。
- `D21WeatherImpactRules` 读取天气输入和匹配的海况结果，产出船体风险、航速倍率、补给/淡水/食物/燃料倍率、船员疲劳/士气/健康风险、捕鱼活动/收获倍率和港口可用性。
- 规则先校验输入和海况指纹，错配、非法或被拒绝的海况不会被计算；所有数值采用固定公式与 clamp。
- `D21WeatherImpactResult` 通过只读属性和防御性 Clone 隔离输入/海况；本系统不写船体、D3 库存、船员、D11 捕鱼或 D19 港口事实。

## 真实上游证据

- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke 完成 `WeatherInput -> SeaState -> WeatherImpact`，有效结果 `Success=true`；相同输入重复结果稳定，改变风力会改变稳定种子/派生结果。
- 未引用 Unity、网络、LLM、随机或系统时钟。

## 进行中 / 未宣称

- 各 owner 消费影响结果并在自身系统内写事实的编排不在本任务实现。
- 不修改 D9、D3、D11、D19 或船员/船体系统。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. 影响结果是资格/倍率输入，不是跨系统写命令；船况、补给、船员、捕鱼和港口 owner 必须自行裁决与写入。
2. L14 可以消费 `FoodConsumptionMultiplier` 和 `WaterConsumptionMultiplier`，但仍只写自己的航程补给 ledger，不得改 D3 库存。
3. 港口可用性是天气条件派生结果，D19 是否接受实际港口命令仍由 D19 Router/owner 决定。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21WeatherImpactRules.cs`
- `game/Assets/Scripts/Systems/D21/D21WeatherImpactRules.cs.meta`
- `game/tools/results/D21/D21-L13/D21_L13_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L13_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- 天气影响纯逻辑 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L13 已完成天气影响只读规则；输出覆盖速度、补给、船员、捕鱼和港口可用性，结果与输入/海况均防御性复制，不写任何 owner 事实。`
