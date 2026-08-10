# D11-A04 交接报告

## 任务编号

`D11-A04`，完成日期：2026-08-11。

## 修改文件

- `game/Assets/Scripts/Systems/D11/D11WhaleDolphinContracts.cs`
- `game/Assets/Scripts/Systems/D11/D11WhaleDolphinSystem.cs`
- `game/Assets/Scripts/Systems/D11/D11WhaleDolphinContracts.cs.meta`
- `game/Assets/Scripts/Systems/D11/D11WhaleDolphinSystem.cs.meta`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Assets/Scripts/Systems/D11/D11PetSystem.cs`（仅修复当前并行 D11 Pet 编译中的 `lostRevision` 变量作用域，未改变 Pet 逻辑）
- `game/Whalebone.Systems.csproj`
- `game/Assets/Tests/EditMode/D11WhaleDolphinRoutesTests.cs`
- `game/Assets/Tests/EditMode/D11WhaleDolphinRoutesTests.cs.meta`
- `game/Save.Tests.csproj`
- `tools/d11_a04_smoke/D11A04Smoke.csproj`
- `tools/d11_a04_smoke/Program.cs`
- `docs/game/design/common/D11_WHALE_DOLPHIN_ROUTES.md`
- `.workbuddy/memory/2026-08-11.md`

工作区原有的 D11-A03/A05/A06 和 D12 并行成果也在本次编译接线中出现；其领域逻辑未被本任务改写。为保持 Systems 可编译，只修复了 Pet 并行代码中 `lostRevision` 的变量作用域，并保留各 DTO 的独立存档键。D11-A04 自己的鲸豚快照使用 `D11Runtime`，活鱼舱使用 `D11AquacultureRuntime`，避免同名覆盖。

## 数据边界

`D11WhaleDolphinSystem` 唯一写 `D11WhaleDolphinRuntimeSnapshot` 的 encounters、route records、reward/consequence/quest requests 和幂等键。它读取 A01 `EcologySystem` 的生态/搁浅/保护压力快照、D9 `EnvironmentSystem` 海况快照、Faction/Legal 许可证政策、Ship/Crew 准备条件；不写这些系统的 canonical state。

Reward、Inventory/Cargo、Ecology、Faction/Legal、Ship/Crew 和 Quest 后续只消费 request/fact，并按各自所有权结算。

## D7/既有兼容性

- 未删除 Player、Ship、CrewAgent 或 D4 Ecology 的 D7 字段。
- 鲸豚数据写在现有 `location_json.d11`，不改变 D7 SQLite 三表结构。
- 旧档没有 `d11` 时读为 `null`/空运行时；当前 D11 代码构造时补齐空列表和 schema version=1。
- SaveSystem 仍保留并行 D11 水产、Pet、D12 字段，各自使用独立键。

## 捕鲸和敏感内容

固定标签：`history_1675_caribbean_whaling`。固定审查状态：`sensitive_whaling_non_glorifying_review_required`。捕鲸必须有许可证、法律政策、季节、船员、船型、设施、声望和生态条件；失败有船体/船员风险和恢复路线；保护种群/禁止政策/条件不足不发收益；放弃追踪提供观察或路线记录替代奖励。

## 测试命令与结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：通过，0 warning / 0 error。
- `dotnet run --project tools/d11_a04_smoke/D11A04Smoke.csproj`：通过，输出 `D11-A04 smoke PASS: migration/whaling/alternative-boundary/dedupe/snapshot`。
- Unity EditMode 测试入口已登记：`D11WhaleDolphinRoutesTests.cs`。
- `dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：被当前工作区既有 D12 测试错误阻断：`D12MapDiscoverySystemTests.cs(190)` 使用不存在的 `D12ToolResult.Fact`。该错误不来自 D11-A04；Systems 主工程和独立 D11 烟测已通过。

## 未完成事项

- RewardSystem、EcologySystem、Faction/Legal、Ship/Crew、QuestSystem 尚未在 D11-A04 中消费 request 队列；这属于后续跨系统接入，不在本任务内直接代写。
- Unity 编辑器实际运行 EditMode 套件仍需先修复/整理工作区既有 D12 测试基线。
- 未添加批量鲸豚内容、UI 面板或网络/LLM 逻辑。

## D11-A05 迁移注意事项

- A05 珍稀动物链路继续使用 D4 `EcologyState.RareAnimals` 和 `D4EcologySystem` 的 `animalId`/生境状态；不要把 `D11MarineEncounter` 误当成 `AnimalState`，也不要复制生态压力或死亡率公式。
- A05 若需要观测/救助事实，应引用本任务 `QuestFacts`/EventLog cause，而不是直接写 `D11WhaleDolphinRuntimeSnapshot` 或发放 `RewardRequests`。
- A05 的生态、收藏、势力法律后果仍由其既有 owner 写入；D11-A04 的 consequence request 只允许通过所属 owner 结算。

## 验收结论和异步任务许可

核心验收通过：迁徙、海豚伴航、观测、搁浅救助、捕鲸成功/失败、放弃追踪均有确定性路线或事实入口；捕鲸条件、生态/社会/法律后果、替代奖励和重复保护均可测试、可保存、可恢复。

允许开始后续异步集成任务（建议 D11-S01），前提是先读取本报告和主文档，并由各 canonical owner 消费待结算请求；不得把本系统的请求对象直接当作已经发放的收益或已经改变的生态/势力状态。
