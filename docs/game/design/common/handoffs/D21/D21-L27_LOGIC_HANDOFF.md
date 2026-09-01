# D21-L27 逻辑交接

## 已完成

- `D21ProvisionConsumptionSystem` 已将每个航程的已应用 `ApplicationId` 从逐状态深复制列表改为不可变持久链。新 tick 只追加一个节点；`AppliedApplicationIds` 仍按原有旧到新顺序返回防御性列表，仅在调用方显式读取时物化。
- ApplicationId 重放缓存只保存标量结果和共享的不可变账本快照，不再把每个 tick 之前的完整历史嵌入缓存结果；原子提交、相同指纹幂等重放、冲突/重复 tick/短缺无突变语义保持不变。
- `D21VoyageEventLog` 增加内部 dedupe 索引，保持事件顺序、重复事件返回和保存/恢复语义不变，避免长跑时对全部历史逐条扫描。
- 新增真实命令服务长跑：通过 `D21VoyageCommandService.Route` 先 Start，再连续 10,000 个 ResolveTick；每 tick 补给和解析均提交，最终状态仍为 `Sailing`，补给 food/water 均为 10,000，事件日志 40,001 条且 dedupe key 唯一。

## 真实验证证据

- `dotnet build .codex-temp/d21-tests/D21IntegrationTests.csproj --no-restore --verbosity:minimal`：通过，0 warning / 0 error。
- `dotnet run --project .codex-temp/d21-tests/D21IntegrationTests.csproj --no-build --no-restore`：退出码 0；输出 `D21_INTEGRATION_BASE_CASES=8`、`D21_CREW_PROJECTION_ASSERTIONS=16`、`D21_INTEGRATION_CASES=11`、`D21_INTEGRATION_FAILURES=0`。
- 10,000 Tick 测试断言：`elapsed_ticks=10,000`、`LastTickIndex=9,999`、状态 `Sailing`、food/water 非负且最终为 10,000、`AppliedApplicationIds.Count=10,000`、事件数 `1 + 4 * 10,000 = 40,001`、dedupe key 全部唯一。
- L14 历史语义测试覆盖相同 ApplicationId 指纹重放、冲突、重复 tick、历史顺序、返回列表防御性和食物/淡水同时短缺原子性。
- `Measure-Command` 观测本机离线 harness 墙钟约 29,336 ms；这不是产品性能指标或运行时承诺。

## L29 审计证据

- `python tools/d21_logic_boundary_audit.py --self-test`：通过；注释、普通字符串和 verbatim 字符串中的伪违规不再误报，执行代码中的违规可检出。
- `python -m unittest tools/tests/test_d21_logic_boundary_audit.py`：3 tests，全部通过。
- `python tools/d21_logic_boundary_audit.py`：扫描 29 个 D21 C# 文件，D21-AUDIT-001—005 violations=0。

## 未宣称与后续边界

- 本 handoff 只证明离线纯 C# 逻辑和命令服务压力路径，不证明 Unity Player/PlayMode、场景/Prefab/UI/Input/Transform/Animator/物理、平台、真机、SaveSystem 磁盘/云端/崩溃恢复或发行就绪。
- 10k 命令服务测试使用显式已校验 `CrewDecision`。当前 D20 crew projection 的真实 owner 接入仍是 deferred；本次相关 D21 断言不关闭 D20-L40 门禁。
- 不修改 D21-L06/L24、D20 源码、`docs/game/LOGIC_TASK_STATUS.md` 或 `.workbuddy/memory/2026-09-01.md`。

## 交付物

- `game/Assets/Scripts/Systems/D21/D21ProvisionConsumptionSystem.cs`
- `game/Assets/Scripts/Systems/D21/D21VoyageEventContracts.cs`
- `game/Assets/Tests/EditMode/D21/D21LongVoyageSimulationTests.cs`
- `.codex-temp/d21-compile/D21IntegrationCompile.csproj`
- `.codex-temp/d21-tests/D21IntegrationTestsProgram.cs`
- `game/tools/results/D21/D21-L27/D21_L27_LOGIC_RESULT.json`
- `docs/game/design/common/handoffs/D21/D21-L27_LOGIC_HANDOFF.md`

## Coordinator memory entry

`2026-09-01：D21-L27 已完成不可变补给历史链/轻量重放记录和事件 dedupe 索引，真实 D21VoyageCommandService Start + 10,000 ResolveTick 通过；当前 runner 输出 11 cases、16 crew assertions、0 failures。共享当天记忆文件按范围未修改。`
