# D23 逻辑存档审计

> 对应任务：D23-L36
> 审计时间：2026-09-02（Asia/Shanghai）
> 当前结论：`LOGIC_PASS`（纯逻辑 projection/snapshot 范围）
> P0 blocker：无

## 1. 审计范围与结论

本文件审计 D23 当前可复现的离线 save projection、事件日志 snapshot 和跨阶段 round-trip。它证明 owner projection 能在固定输入下序列化、恢复、校验和生成稳定 fingerprint；不把这些证据扩大为真实 root SaveSystem、文件/云介质或崩溃恢复验收。

主剧本在 Tick `0`、Tick `20` 和最终返港阶段读取 D21 `ShipSave`；长跑每 10 Tick 做 JSON round-trip；返港后读取 D19 port projection、D21 event log snapshot 和 D8 shared `EventLog` snapshot。所有恢复结果都由 C# 断言检查。

## 2. 固定输入

| 字段 | 固定值 |
|---|---|
| seed | `2301` |
| world_day | `23` |
| save_revision | `230100` |
| player_id / ship_id | `player_d23_001` / `ship_d23_sloop_001` |
| port_id / voyage_id | `port_santa_margarita` / `voyage_d23_001` |
| voyage start | day `23`、minute `480` |

夹具入口为 `game/Assets/Tests/EditMode/D23/D23ScenarioFixtureFactory.cs`；D23 场景只消费 owner API 和公开 projection，不另建第二份存档根模型。

## 3. 真实命令与输出

| 检查 | 实际命令 | 实际结果 |
|---|---|---|
| 编译 | `dotnet build .codex-temp/d23-compile/D23IntegrationTests.csproj --no-restore --verbosity:minimal` | exit `0`；0 warning、0 error |
| 统一 run | `python tools/d23_scenario_runner.py --write-results` | exit `0`；26 cases、13925 assertions、0 failures |
| C# run marker | `dotnet run --project .codex-temp/d23-compile/D23IntegrationTests.csproj --no-build --no-restore` | exit `0`；`D23_CASES=26`、`D23_FAILURES=0` |
| 边界审计 | `python tools/d23_cross_system_boundary_audit.py --json` | exit `0`；28 files、0 issues |

统一报告：`game/tools/results/D23/D23-L31/D23_L31_RUNNER_REPORT.json`。

## 4. 存档证据矩阵

| 检查点 | 真实证据 | 结论 |
|---|---|---|
| D23-L16 SavePoint | `D23SavePointScenario` 的 D21 ship projection JSON round-trip、D19 port clone/Validate、D20 identity clone | `LOGIC_PASS` |
| 主剧本中途断点 | FullLoop 在 Tick `0` 和 `20` 调用 `ToJson/FromJson`，并检查中途与最终 save fingerprint 不相同 | `LOGIC_PASS` |
| 主剧本返港恢复 | D21 event log `Validate/Restore`、D19 port `BuildProjection/Validate/Migrate`、D8 `CaptureSnapshot/FromSnapshot` 检查 event count、sequence 和 facts | `LOGIC_PASS` |
| 长跑保存稳定性 | LongRun 50 Tick；每 10 Tick 做 D21 JSON round-trip，并检查距离/补给/事件 key | `LOGIC_PASS` |
| 任意断点失败恢复 | InterruptionMatrix 检查非法 D21 Tick 不改变 state/save，D21/D8 snapshot restore 不丢历史 | `LOGIC_PASS`（纯逻辑边界） |
| 真实 SaveSystem 介质 | 本章没有文件/云介质写入、读取或崩溃恢复调用 | Runtime Deferred |

## 5. 限定结论与交接

L36 的 `LOGIC_PASS` 只授予上表中的 owner projection/snapshot 证据。真实 SaveSystem 介质、云同步、版本升级、进程崩溃恢复、Unity 生命周期接线和设备验证继续登记在 `D23_RUNTIME_DEFERRED_REGISTER.md`，不在本章伪造为通过。

若未来修改 D19/D21/D8 projection 或事件 schema，必须使用同一固定输入重新运行统一 runner，并重新检查 L16/L26/L28/L29 与本审计。
