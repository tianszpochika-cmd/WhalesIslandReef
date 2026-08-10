# D12-A01 交接报告：海图、测深与发现系统

任务：`D12-A01`  
日期：2026-08-11  
状态：**PASS（代码/纯 C# 构建与 D9 前置门禁通过）**

## 主成果

- 设计文档：`docs/game/design/common/D12_MAP_DISCOVERY.md`
- 海图契约与快照：`game/Assets/Scripts/Systems/D12/D12MapDiscoveryContracts.cs`
- 海图唯一写者：`game/Assets/Scripts/Systems/D12/D12MapDiscoverySystem.cs`
- D12 纵切片目录：`game/Assets/Scripts/Systems/D12/D12MapDiscoveryCatalog.cs`
- EditMode 测试：`game/Assets/Tests/EditMode/D12MapDiscoverySystemTests.cs`
- 存档接线：`GameState.CurrentD12Runtime`、`PlayerState.D12Runtime`、`SaveSystem.SaveD12Runtime/LoadD12Runtime`，快照写入既有 `location_json.d12`；共享事实仍随 D3 `EventLogSnapshot` 保存。

## 已实现接口

| 接口 | 责任 |
|---|---|
| `Discover` | 直接探索观察，写 `ExplorationSystem` 事实 |
| `RecordTradeKnowledge` | 交易旧图/航线知识 |
| `RecordQuestKnowledge` | 任务线索知识 |
| `ProposeCommunityKnowledge` | 只生成 Agent/NPC 传闻提案，不改状态 |
| `AcceptCommunityKnowledge` | 需要社区确认回执，最多产生 Rumor |
| `UseSoundingLine` | 触底才记录精确水深；绳短返回可解释失败 |
| `UseTelescope` | 受能见度/距离约束，确认海岸、暗礁、浅滩或鱼场轮廓 |
| `GetMapReadModel` | UI 唯一读取面，只输出已获得信息 |

## 写者与事实边界

- `D12MapDiscoverySystem` 是地图发现状态、可信度、证据和测深读数的唯一写者。
- 每条有效证据追加 `event_d12_map_discovery_*`，`owner_system=ExplorationSystem`；重复证据和重复事件不增加状态。
- Agent 没有地图状态写入接口；传闻必须有 `CommunityConfirmed` 与 `CommunityReceiptId` 才能进入事实链。
- UI 不读真实目录水深；传闻阶段不读精确位置；UI 不能写快照、事件或内容目录。
- D9 鱼群/生态/经济/奖励所有权未复制，D12 仅引用 `nearshore_saint_marys` 与 `fish_mackerel_school`。

## 验证结果

```text
python tools/d9_s02_acceptance_check.py --json
PASS, blocked_count=0, frozen_event_count=14

python -m unittest tools.tests.test_d9_s02_acceptance tools.tests.test_d9_vertical_slice_freeze -v
PASS, 3/3

dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
PASS, 0 warning / 0 error

dotnet build game/Save.Tests.csproj --no-restore -v:minimal
PASS, 0 warning / 0 error

tools/D9S02TestRunner/bin/Debug/net9.0/D9S02TestRunner.exe
D9-S02_TESTS_PASS count=2
```

D12 EditMode 测试已登记并成功编译进 `Save.Tests`；本轮未伪造 Unity Editor Test Runner 结果。Unity 主工程若仍被已有实例占用，释放后运行 `D12MapDiscoverySystemTests` 的三项测试即可补齐编辑器归档。

## 下游使用说明

1. D12-A02 沉船/洞穴使用 `GetMapReadModel` 和 `RecordQuestKnowledge`，不得复制地图状态或自行追加发现事实。
2. D12-A03 线索系统使用 `D12ChartEvidence` 的来源、证据和可信度，奖励仍由 RewardSystem 裁决。
3. 未来 UI 只绑定 `GetMapReadModel`；不要绑定 `D12MapFeatureDefinition` 读取真实位置/水深。
4. 若扩展跨端地图协议，先新增 `contracts/` Schema，再同步 game/server/web；本任务没有新增跨端私有协议。
