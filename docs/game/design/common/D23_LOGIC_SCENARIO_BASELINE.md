# D23 纯逻辑跨系统场景基线

> 版本：D23-L01 / 2026-09-02
> 范围：D23-L01—D23-L38 的纯逻辑场景、固定种子夹具、串行 runner 与收口证据。
> 当前章节结论：`LOGIC_PASS`（仅限纯逻辑门禁；Runtime/Release 仍延期）。

## 1. 验收边界

D23 只验证纯 C# 逻辑契约、owner API 编排、固定输入下的状态结果、失败不变式、事件幂等、确定性回放、长跑和 save projection/snapshot round-trip。场景不启动 Unity Player/PlayMode，不读取场景、Prefab、Canvas、Transform、Animator、物理、输入、真实时钟、网络或 LLM，也不把编译成功或就绪状态单独当作运行时通过。

完整证据由 `tools/d23_scenario_runner.py --write-results` 生成，必须同时满足上游结果门禁、D23 边界审计、纯 C# build、26-case run、marker 和协议检查。

## 2. 上游门禁快照

每项跨系统场景先验证 D18-L50、D19-L48、D20-L40、D21-L32、D22-L36 五个真实 `LOGIC_PASS` 结果：

| 依赖 | 结果文件 | 当前状态 | 证据说明 |
|---|---|---|---|
| D18-L50 | `game/tools/results/D18/D18-L50/D18_L50_LOGIC_RESULT.json` | `LOGIC_PASS` | 结果 JSON 已核对 |
| D19-L48 | `game/tools/results/D19/D19-L48/D19_L48_LOGIC_RESULT.json` | `LOGIC_PASS` | 结果 JSON 已核对 |
| D20-L40 | `game/tools/results/D20/D20-L40/D20_L40_LOGIC_RESULT.json` | `LOGIC_PASS` | 结果 JSON 已核对 |
| D21-L32 | `game/tools/results/D21/D21-L32/D21_L32_LOGIC_RESULT.json` | `LOGIC_PASS` | 结果 JSON 已核对 |
| D22-L36 | `game/tools/results/D22/D22-L36/D22_L36_LOGIC_RESULT.json` | `LOGIC_PASS` | 结果 JSON 已核对 |

五项门禁已闭合；D23 不再依赖缺失结果或实现文件名推断通过。

## 3. 固定种子与初始世界

夹具入口为 `game/Assets/Tests/EditMode/D23/D23ScenarioFixtureFactory.cs`，所有场景通过 `D23ScenarioFixtureFactory.Create()` 获取新实例。初始输入冻结为：

| 字段 | 值 |
|---|---|
| seed | `2301` |
| world_day | `23` |
| save_revision | `230100` |
| player_id | `player_d23_001` |
| ship_id | `ship_d23_sloop_001` |
| port_id | `port_santa_margarita` |
| berth_id | `berth_santa_margarita_dock_01` |
| commodity_id | `sugar` |
| voyage_id | `voyage_d23_001` |
| voyage route | `120000` milli-nautical-miles |
| voyage start | day 23, minute 480 |

市场、货币、库存、市场/港口访问、泊位、玩家 presence、船身份、船体组件、船况、天气、航程时间、save projection 和 UI 命令上下文均通过各自公开 owner API 构造。场景不得直接写入这些 owner 的业务字段；可变事实只能由所属 owner 的公开命令改变。

## 4. Owner 边界

| 事实 | 唯一 owner | D23 允许的消费方式 |
|---|---|---|
| 金钱、市场、货舱 | D3 `TradeEconomySystem` / `D3InventorySystem` | 透传 `D19MarketPortAdapter`、`D19WarehousePortAdapter` 或 D3 公共 API |
| 港口访问、泊位、港口命令幂等 | D19 `D19PortVisitSystem` / `D19BerthSystem` / `D19PortCommandRouter` | 透传 D19 命令与只读快照 |
| 玩家角色 identity / presence | D20 公共 identity/presence contracts | 提交显式 presence request，读取 clone |
| 船身份、组件、船况、天气、航程时钟 | D21 对应 owner | 提交 D21 公共命令或读取验证后的 projection |
| 事件事实 | D8 `EventLog` | 只追加 owner 产生且已验证的 `WorldEvent`；重复键必须返回 duplicate |
| UI 命令包络 | D22 contracts/validator | 只校验 request/context，不让 UI 直接写领域事实 |

D23 不创建第二份货币、库存、关系、船、船员、生态或存档根模型。D20 Agent/NPC/animal 仍只提供提议或只读 projection，不能裁决 D3/D10/D11/D19/D21 事实。D4 没有公开耐久维修 API，D23-L09 保留“repair API 不存在”的显式边界断言。

## 5. 场景执行策略

L03—L24 的场景文件由统一 C# 程序执行；上游 gate 已闭合后，局部断言与跨系统 guard 均在真实运行中通过。L26—L29 复用同一夹具和 owner API，分别验证 40 Tick 主剧本、20 次回放、50 Tick 长跑和中断/恢复矩阵；L30—L34 提供边界审计、runner、接口、延期和债务证据。

当前真实 run 的机器摘要为：`D23_CASES=26`、`D23_ASSERTIONS=13925`、`D23_FAILURES=0`，各 case 名称唯一且完整。最终机器报告见 `game/tools/results/D23/D23-L31/D23_L31_RUNNER_REPORT.json`。

## 6. 验证命令与延期项

```text
python tools/d23_cross_system_boundary_audit.py --self-test
python tools/d23_cross_system_boundary_audit.py --json
dotnet build .codex-temp/d23-compile/D23IntegrationTests.csproj --no-restore --verbosity:minimal
dotnet run --project .codex-temp/d23-compile/D23IntegrationTests.csproj --no-build --no-restore
python tools/d23_scenario_runner.py --write-results
```

上述命令在本次 D23 收口中真实执行并退出 0；runner 还把结果协议、上游 gate 和 26-case markers 写入机器报告。结果 JSON 与 handoff 使用九字段协议，L01—L38 的最终收口见 `D23_LOGIC_ACCEPTANCE.md`。

以下内容明确延期：Unity Player/PlayMode、场景/Prefab/UI/输入/物理/动画、真实存档介质、设备/性能、网络、LLM 和发行验收。详见 `D23_RUNTIME_DEFERRED_REGISTER.md`。本章节的 `LOGIC_PASS` 不等于 `RuntimeApproved` 或 `ReleaseApproved`。
