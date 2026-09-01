# D21-L12 逻辑交接

## 已完成

- 任务：`D21-L12`
- 状态：`LOGIC_PASS`，范围仅为天气到海况的纯确定性裁决。
- `D21SeaStateSystem` 消费已校验 `D21WeatherInput`，产出 `D21SeaStateResult`：海况等级、严重度、风险、事件概率、可航行性、输入指纹和稳定事件种子。
- 风力为主导因素，降水/能见度/低气压/湿季等只通过固定规则增加压力；极端、不可航行和输入失败均显式表达。
- 结果中的天气输入通过防御性 Clone 返回；稳定种子使用固定 FNV-1a，不使用随机源、系统时钟、网络或 Unity。

## 真实上游证据

- 独立 Roslyn 编译以当前构建的 `Whalebone.Systems.dll` 为引用通过。
- 纯 C# smoke 对同一有效输入重复计算，海况/指纹/事件种子一致；修改 WindForce 后稳定种子变化。
- 输入校验失败不会进入部分海况计算。

## 进行中 / 未宣称

- D9 天气快照接入和 D21 事件/船况 owner 的真实消费编排未在本任务实现。
- 本系统不写 D9 天气、船况、路线、船员或事件日志。
- `LOGIC_PASS` 不表示 Player/PlayMode、可玩性、性能、真机或发行通过。

## 风险与下一步

1. 后续 owner 必须把 `D21SeaStateResult` 视为派生只读资格，不得把它反写成 D9 天气事实。
2. `EventProbability` 只表示事件触发资格/概率输入，事件事实和后果仍由事件或对应 owner 裁决。
3. 船速、补给、船况、船员和港口影响应继续经过 L13 或各自 owner，不要在海况系统内追加副作用。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21SeaStateSystem.cs`
- `game/Assets/Scripts/Systems/D21/D21SeaStateSystem.cs.meta`
- `game/tools/results/D21/D21-L12/D21_L12_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L12_LOGIC_HANDOFF.md`

## 验证与范围保护

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：通过，0 warnings，0 errors；因生成 csproj 未纳入新增文件，另行完成六文件 Roslyn 编译。
- 六个本批次 D21 `.cs` 文件的独立 Roslyn 编译：通过，无错误、无警告。
- 海况确定性/输入敏感性纯逻辑 smoke：通过。
- 不运行 Player/PlayMode、截图、UI、输入、物理、Shader、场景、性能、真机或发行验证。
- 按委派要求不运行 `tools/sync_logic_task_status.py`，不修改 `docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## Coordinator memory entry

`2026-09-01：D21-L12 已实现天气到海况的纯确定性计算；同输入重复结果一致，FNV-1a 派生事件种子，不使用随机/系统时钟，结果通过副本隔离输入。`
