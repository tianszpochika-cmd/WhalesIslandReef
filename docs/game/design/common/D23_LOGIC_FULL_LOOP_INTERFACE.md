# D23 纯逻辑全链路接口

> 对应任务：D23-L32
> 固定输入：seed `2301`、world day `23`、save revision `230100`
> 真实证据：`game/tools/results/D23/D23-L31/D23_L31_RUNNER_REPORT.json`

## 1. 目的与边界

本接口文档定义 D23 主剧本如何在一个离线纯 C# 进程中组合已有 owner API。它验证命令顺序、owner 回执、失败不变式、事件幂等、确定性 Tick 和 projection round-trip；不定义 Unity 场景、输入、UI、网络、LLM 或真实介质存档接口。

主实现为 `game/Assets/Tests/EditMode/D23/D23FullLoopScenarioSupport.cs`，统一入口是 `D23FullLoopScenarioSupport.Run("full-loop")`。D23 runner 同时执行该入口、20 次回放、50 Tick 长跑和中断矩阵。

## 2. 固定上下文

| 字段 | 值 |
|---|---|
| player | `player_d23_001` |
| ship | `ship_d23_sloop_001` |
| port / berth | `port_santa_margarita` / `berth_santa_margarita_dock_01` |
| commodity | `sugar` |
| voyage | `voyage_d23_001`；主剧本 route `120` nm |
| start time | day `23`、minute `480` |
| main ticks | `40`；base speed `12000` milli-knots |
| long-run ticks | `50`；long route `5000000` nm |

夹具由 `D23ScenarioFixtureFactory.Create()` 新建。业务事实只能由其所属 owner 的公开命令改变；夹具中的直接赋值仅用于构造初始输入。

## 3. 主剧本顺序

| 阶段 | 调用/事实 | 必须成立的断言 |
|---|---|---|
| 初始世界 | D18/D19/D20/D21/D22 gate + D20 Port presence | 五项上游结果均为 `LOGIC_PASS`，身份/船/港口/时间 projection 可读 |
| 港口与登船 | D19 `EnterPort` → `Dock` → `AssignBerth` → `Board` | 港口状态到 `Boarded`，每次成功产生唯一 draft |
| 市场与仓库 | `D19MarketPortAdapter.Buy`；`D19WarehousePortAdapter.Store` | D3 money/cargo 变化由 owner 反映；相同 command replay 不二次写入；冲突保持货舱不变 |
| 出航 | D21 `Start` + `D21PortDepartureAdapter`；D20 presence `Voyage` | D19 到 `Departed`，D21 到 `Sailing`，departure draft 已追加到 D8 |
| 航行 | D21 `ResolveTick` × 40；每次把 `D21VoyageEvent.ToWorldEvent()` 追加到 D8 | 每个 Tick 只接受一次，elapsed 单调递增，距离前进，food/water 非负，save projection 可读 |
| 抵达 | 第 40 个 Tick | D21 状态为 `Arrived`；中途 save fingerprint 与最终 fingerprint 可区分 |
| 返港 | D21 `ReturnToPort` + `D21PortReturnAdapter`；D20 presence `Port` | D19 到 `Returned`，D21 到 `Returned`；Arrived→Return 的 Recovered 路由使用窄因果门 |
| 恢复与摘要 | D21 event log restore；D19 port projection validate/migrate；D8 `CaptureSnapshot`/`FromSnapshot` | 事件数、sequence、fact read model 和稳定摘要保持一致 |

## 4. owner 与写权限

| Owner | D23 只允许的入口 | D23 不允许 |
|---|---|---|
| D3 economy/inventory | D19 market/warehouse adapter 或 D3 公共 API | 在编排器直接修改 money/cargo |
| D19 port/berth | `D19PortCommandRouter`、port adapter、只读 projection | 直接推进港口状态或伪造服务回执 |
| D20 presence/identity | `D20PlayerPresenceSystem.Apply`、identity clone | 直接写 presence 的领域迁移事实 |
| D21 ship/voyage | `D21VoyageCommandService.Route` 和其公开 projection | 在 D23 编排器修改航程/天气/补给/船况数值 |
| D8 event log | `EventLog.Append` owner 事件与快照恢复 | 直接构造跨 owner 事实替代命令回执 |

## 5. 断点与可比摘要

主剧本在以下断点保留可检查结果：

1. 港口 `Boarded`：D19 lifecycle、D20 presence 和 D3 market/warehouse 结果；
2. Tick `0`：D21 save projection JSON round-trip；
3. Tick `20`：再次 round-trip，并记录 `midSaveFingerprint`；
4. Tick `39`：`Arrived`、最终 D21 projection 和 D21 event log；
5. `Returned`：D19 port save migration、D21 event log restore、D8 snapshot restore 和 `FinalFingerprint`。

回放使用相同 tag/seed/命令序列运行 20 次，并比较 save/event/shared-fact/final fingerprints。长跑使用同一 owner service 连续执行 50 个 Tick，检查距离、补给、事件 key 和每 10 Tick 的 save round-trip。

## 6. 证据与延期映射

统一命令为：

```text
python tools/d23_scenario_runner.py --write-results
```

报告必须显示 `D23_CASES=26`、`D23_FAILURES=0`，并由上游磁盘结果、D23 边界审计、build 和 C# run 共同决定 `pass_eligible`。Unity Player/PlayMode、真实输入、场景/Prefab/Canvas、设备/性能/发行、网络/LLM 和真实 SaveSystem 介质不属于本接口，见 `D23_RUNTIME_DEFERRED_REGISTER.md`。
