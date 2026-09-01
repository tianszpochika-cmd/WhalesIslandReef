# D23 纯逻辑场景目录

> 版本：D23-L25 / 2026-09-02
> 结论：L01—L38 的纯逻辑证据已由统一 runner 收口；Runtime/Release 维度仍延期。

固定夹具：`game/Assets/Tests/EditMode/D23/D23ScenarioFixtureFactory.cs`。固定输入为 seed `2301`、world day `23`、save revision `230100`、player `player_d23_001`、ship `ship_d23_sloop_001`、port `port_santa_margarita`、berth `berth_santa_margarita_dock_01`、commodity `sugar`。机器总报告：`game/tools/results/D23/D23-L31/D23_L31_RUNNER_REPORT.json`。

## 场景清单

| 任务 | 场景测试/产物 | 覆盖 | 当前状态 |
|---|---|---|---|
| D23-L01 | `D23_LOGIC_SCENARIO_BASELINE.md` | 验收边界、固定输入、owner 边界、依赖快照 | `LOGIC_PASS` |
| D23-L02 | `D23ScenarioFixtureFactory.cs` | 固定种子、身份/船/港口/泊位/天气/时间/save/UI 初始 projection | `LOGIC_PASS` |
| D23-L03 | `D23NewGameScenario.cs` | 初始 seed/day/save、空港口状态、稳定 ship save ID | `LOGIC_PASS` |
| D23-L04 | `D23CharacterSetupScenario.cs` | D20 presence 命令、snapshot restore、immutable identity | `LOGIC_PASS` |
| D23-L05 | `D23PortEntryScenario.cs` | D19 enter → dock → berth、泊位不变式、港口状态 | `LOGIC_PASS` |
| D23-L06 | `D23MarketScenario.cs` | D19→D3 买入、货币/货舱变化、资金不足失败 | `LOGIC_PASS` |
| D23-L07 | `D23WarehouseScenario.cs` | D19→D3 仓库存取、归属、重复/冲突命令幂等 | `LOGIC_PASS` |
| D23-L08 | `D23TavernScenario.cs` | 酒馆服务可用性输入与稳定时间段 | `LOGIC_PASS` |
| D23-L09 | `D23ShipyardScenario.cs` | D4 API 证据与 repair 未实现的显式 blocker | `LOGIC_PASS` |
| D23-L10 | `D23PreparationScenario.cs` | D21 hull/condition/weather 读数校验 | `LOGIC_PASS` |
| D23-L11 | `D23BoardingScenario.cs` | D19 enter → dock → berth → board 生命周期 | `LOGIC_PASS` |
| D23-L12 | `D23DepartureScenario.cs` | D19 boarded → departed 与 D21 departure adapter | `LOGIC_PASS` |
| D23-L13 | `D23VoyageScenario.cs` | D21 weather 确定性、event seed、固定 Tick | `LOGIC_PASS` |
| D23-L14 | `D23ReturnScenario.cs` | D19 departed → returned 与 D21 ReturnToPort | `LOGIC_PASS` |
| D23-L15 | `D23EventLogScenario.cs` | D19 event draft、D8 append duplicate、命令 replay | `LOGIC_PASS` |
| D23-L16 | `D23SavePointScenario.cs` | D20/D19/D21 projection clone/JSON round-trip | `LOGIC_PASS` |
| D23-L17 | `D23TimeAdvanceScenario.cs` | D21 固定 logical tick 与时钟投影 | `LOGIC_PASS` |
| D23-L18 | `D23FailureRecoveryScenario.cs` | 无效离港拒绝、港口状态无部分提交 | `LOGIC_PASS` |
| D23-L19 | `D23CurrencyInventoryInvariantTests.cs` | D3 货币/库存非负读数、D19 泊位初始不变式 | `LOGIC_PASS` |
| D23-L20 | `D23PetAnimalScenario.cs` | 角色 owner 引用与只读边界 | `LOGIC_PASS` |
| D23-L21 | `D23FishingCollectionScenario.cs` | D3 inventory、D8 event owner 边界存在 | `LOGIC_PASS` |
| D23-L22 | `D23FactionConsequenceScenario.cs` | 显式 ship/port authority claims | `LOGIC_PASS` |
| D23-L23 | `D23UiCommandScenario.cs` | D22 NewGame request/context validator | `LOGIC_PASS` |
| D23-L24 | `D23ContractCompatibilityTests.cs` | D20 identity、D21 weather/ship、D22 UI DTO shape | `LOGIC_PASS` |
| D23-L25 | 本目录 | 任务→文件→覆盖→状态完整映射 | `LOGIC_PASS` |
| D23-L26 | `D23FullLoopScenarioTests.cs` | 新游戏→港口→市场/仓库→出航→40 Tick→抵达→返港→projection restore | `LOGIC_PASS` |
| D23-L27 | `D23ReplayDeterminismTests.cs` | 相同固定命令序列重复 20 次，结果指纹一致 | `LOGIC_PASS` |
| D23-L28 | `D23InterruptionMatrixTests.cs` | 非法出航/非法 Tick/重放/snapshot restore | `LOGIC_PASS` |
| D23-L29 | `D23LongRunScenarioTests.cs` | 50 Tick 长跑、距离/补给/事件/save 稳定 | `LOGIC_PASS` |
| D23-L30 | `d23_cross_system_boundary_audit.py` | D23 C# source boundary audit 与 self-test | `LOGIC_PASS` |
| D23-L31 | `d23_scenario_runner.py` | Python、audit、upstream gate、C# build/run、结果报告 | `LOGIC_PASS` |
| D23-L32 | `D23_LOGIC_FULL_LOOP_INTERFACE.md` | 主剧本接口、owner 边界、断点与摘要 | `LOGIC_PASS` |
| D23-L33 | `D23_RUNTIME_DEFERRED_REGISTER.md` | Runtime/Release 延期范围登记 | `LOGIC_PASS` |
| D23-L34 | `D23_LOGIC_DEBT_REGISTER.md` | 已修复逻辑缺口、非 P0 债务、收口规则 | `LOGIC_PASS` |
| D23-L35 | `D23-L35` 结果与 handoff | 最终场景/回放/长跑/中断/边界收口 | `LOGIC_PASS` |
| D23-L36 | `D23_LOGIC_SAVE_AUDIT.md` | save projection、D19/D21/D8 snapshot 审计 | `LOGIC_PASS` |
| D23-L37 | `D23_LOGIC_STATUS_REVIEW.md` | 结果、协议、上游和延期状态复核 | `LOGIC_PASS` |
| D23-L38 | `D23_LOGIC_ACCEPTANCE.md` | 最终纯逻辑验收与交接 | `LOGIC_PASS` |

## 统一证据

统一 runner 的真实输出为 `D23_CASES=26`、`D23_ASSERTIONS=13925`、`D23_FAILURES=0`；26 个 case 名称均出现一次。上游 D18-L50、D19-L48、D20-L40、D21-L32、D22-L36 的结果 JSON 均为 `LOGIC_PASS`。D23-L01—L38 的结果 JSON 与 handoff 均使用九字段结果协议并互相引用。

## 边界与延期

目录中的 `LOGIC_PASS` 只表示纯逻辑证据链闭合。Unity Player/PlayMode、场景/Prefab/Canvas、真实输入/UI、设备/性能、画面/音频、网络、LLM、真实 SaveSystem 介质和 Steam/发行验收仍为 Runtime Deferred，详见 `D23_RUNTIME_DEFERRED_REGISTER.md`。
