# D23 纯逻辑债务登记

> 对应任务：D23-L34
> 当前 P0 blocker：无

## 1. 已解决的纯逻辑缺口

| 项目 | 处理 | 证据 |
|---|---|---|
| D21 Arrived→Return 事件路由 | `CompleteReturn` 在合法返港后生成 `Recovered` 事件；事件路由器新增窄因果门：上一事件必须为 `Arrived`、关联该事件 ID，且因果以 `port:return:` 开头。原有 Failed/Interrupted recovery 约束保持不变。 | `game/Assets/Scripts/Systems/D21/D21VoyageEventRouter.cs`；D23 FullLoop/InterruptionMatrix 实跑 |
| 仓库重复/冲突副作用 | 相同 command replay 返回原结果且不二次修改货舱；相同 command 的不同 quantity 被拒绝且货舱不变。 | `D23WarehouseScenario.cs`、D23 FullLoop/InterruptionMatrix |

## 2. 非 P0 逻辑债务

| 优先级 | 债务 | 影响/后续 |
|---|---|---|
| P1 | D23 编排器在测试进程中桥接 D19/D21 事件到 D8；尚未有一个生产级跨系统 application service 作为唯一编排入口。 | D23 证明 owner API 与事件契约可组合；后续应在正式应用层落地同样的因果/幂等边界，再做集成回归。 |
| P1 | D23 验证的是 D19 port、D21 ship/voyage、D8 snapshot 与 projection 的组合，不是 root SaveSystem 真实介质。 | 真实介质、崩溃恢复、云同步和版本迁移继续由 Runtime/Release 任务负责。 |
| P2 | 统一 runner 的结果协议和 handoff 由脚本生成，场景明细仍分布在各 L03–L29 测试文件。 | 修改 runner 或 owner API 后必须重跑 `python tools/d23_scenario_runner.py --write-results`，不得手改计数冒充证据。 |

## 3. 不属于逻辑债务的延期

Unity/Player/PlayMode、真实输入/UI、设备/性能/画面/音频、网络、LLM、Steam 和真实 SaveSystem 介质统一登记在 `D23_RUNTIME_DEFERRED_REGISTER.md`，不在本登记中转化为逻辑 blocker。

## 4. 收口规则

只有真实 runner 报告、上游五项 `LOGIC_PASS`、边界审计和 L01–L34 协议证据全部通过时，才允许 L35–L38 写入 `LOGIC_PASS`。任何未来债务修复都必须重新编译、运行并更新本登记与结果 handoff。
