# D12 海图、测深与发现系统

任务：`D12-A01`  
状态：已实现，可供 D12-A02/A03 读取  
前置：`D9-S02 = PASS`

## 1. 目标与边界

D12 的海图不是开局全亮的静态导航图，而是玩家通过航行观察、交易获得的旧图、任务线索和地方社群知识逐步形成的证据集合。地图信息只在 `Whalebone.Systems.D12.D12MapDiscoverySystem` 内写入；`EventLog` 记录已经裁决的发现事实，`SaveSystem` 只序列化 D12 快照。

本任务不修改 D9 的鱼群数量、价格、奖励或任务状态。D9 的 `nearshore_saint_marys` / `fish_mackerel_school` 通过 `D12MapDiscoveryCatalog` 作为鱼场的内容引用，捕捞仍由 `D9FishingSystem` 和 `EcologySystem` 裁决。

## 2. 海图分层与纵切片内容

| 分层 | 代表内容 | 未知时 | 已获得后 |
|---|---|---|---|
| `Coastline` | `coast_unknown_green_cay` 未知海岸 | 不进入 UI ReadModel | 传闻先显示区域，定位后显示海岸位置 |
| `NavigationHazards` | `reef_saint_marys_east` 暗礁 | 不显示 | 显示危险类型、可信度和证据来源 |
| `Bathymetry` | `shoal_saint_marys_bar` 浅滩、`channel_saint_marys_inner` 安全航道 | 不显示 | 测深后显示实测深度；真实配置水深不直接泄露 |
| `Ecology` | `nearshore_saint_marys` 鱼场 | 不显示 | 望远镜/观察或其他合法来源确认后显示鱼场 |
| `CommunityKnowledge` | 社群传闻作为证据来源 | 不直接解锁 | 社区确认后只能先形成 `Rumor` |
| `ExplorationSites` | `wreck_saint_marys_chart_cove` 沉船、`cave_green_cay_tide_cave` 洞穴入口 | 不显示 | 先获得位置证据，再交给 D12-A02 做潮汐/装备/法律检查 |

目录定义 (`D12MapFeatureDefinition`) 只描述世界内容和真实水深；玩家进度保存于 `D12MapFeatureState`，不能通过读取目录定义绕过发现。

## 3. 发现阶段与可信度

发现阶段按证据强度与来源交叉验证推进：

`Unknown → Rumor → Approximate → Located → Reliable → Verified`

证据必须同时包含 `SourceRef`、`EvidenceRef`、解释摘要、来源类型、证据类型、世界日和 `save_revision`。系统以最高证据强度为基础，并给不同来源的交叉验证提供有限加成；单条传闻不会因为 Agent 说得肯定而直接变成可靠地图。

默认证据强度：

- 社群知识：`0.20—0.49`，社区确认后最多为 `Rumor`；
- 交易旧图：`0.55`，可形成 `Approximate`；
- 任务线索：`0.78`，可形成 `Located`；
- 直接探索：默认 `0.68`；
- 望远镜观察：`0.74`，受能见度和距离限制；
- 测深绳触底：`0.96`，记录精确测深并推进到 `Verified`。

可信度展示为 `Uncharted / Rumored / Approximate / Reliable / Verified` 五档。每档都可追溯到 `D12ChartEvidence` 及对应的 `FactEvent`，不存在无来源的地图可信度字段。

## 4. 来源与写入入口

| 入口 | 能做什么 | 是否立即改变地图 |
|---|---|---|
| `Discover(D12DiscoveryRequest)` | 玩家/航海观察记录直接探索事实 | 是，来源固定为 `Exploration` |
| `RecordTradeKnowledge(...)` | 交易中获得旧图或航线知识 | 是，来源为 `Trade` |
| `RecordQuestKnowledge(...)` | 任务日志/线索提供位置证据 | 是，来源为 `Quest` |
| `ProposeCommunityKnowledge(...)` | Agent/NPC 提出传闻/知识 | 否，只返回提案 |
| `AcceptCommunityKnowledge(...)` | 带社区确认回执的提案进入事实链 | 是，但最多形成传闻级知识 |

社区入口要求 `CommunityConfirmed=true` 和非空 `CommunityReceiptId`。因此 Agent 可以提供表达和传闻内容，但没有直接解锁海图的入口。

每次有效发现都追加 `owner_system=ExplorationSystem` 的 `event_d12_map_discovery_*` 事实；事件去重仍使用统一 `event_id + save_revision` 机制。重复证据不会重复追加状态或事件。

## 5. 测深绳与望远镜

### 测深绳

`UseSoundingLine(D12SoundingLineRequest)` 只对暗礁、浅滩、鱼场和安全航道有效。绳长不足时返回 `line_too_short`，只说明“水深超过当前绳长”，不生成精确发现标记；触底后保存 `HasMeasuredDepth`、`MeasuredDepthMeters` 和 `SoundingLine` 证据。

### 望远镜

`UseTelescope(D12TelescopeRequest)` 可观察未知海岸、暗礁、浅滩和鱼场，但必须满足：距离不超过 20 海里、能见度至少 `0.35`。安全航道不能靠远望确认，必须使用测深或任务/航线资料。低能见度和超距分别返回 `visibility_too_low` / `out_of_range`，不会写入地图状态。

## 6. UI、Agent 与存档边界

- UI 只能读取 `GetMapReadModel()`；未知特征不会出现在 `VisibleFeatures`，传闻阶段不输出精确 `LocationRef`，真实目录水深不输出。
- Agent 只能消费事实摘要并调用 `ProposeCommunityKnowledge` 产生提案；它不能写 `D12MapDiscoverySnapshot` 或 `EventLog`。
- `D12RuntimeSnapshot.MapDiscovery` 通过 `GameState.CurrentD12Runtime`、`PlayerState.D12Runtime` 和 `SaveSystem.SaveD12Runtime/LoadD12Runtime` 保存。共享 `EventLogSnapshot` 仍由 D3 快照承载，地图状态和事实可以在读档后重新绑定。
- `D12MapDiscoverySnapshot.Validate()` 校验 schema 版本、重复特征/证据、可信度范围和证据完整性；存档恢复时未知特征不会被静默接纳。

## 7. 代码与测试入口

- `game/Assets/Scripts/Systems/D12/D12MapDiscoveryContracts.cs`
- `game/Assets/Scripts/Systems/D12/D12MapDiscoverySystem.cs`
- `game/Assets/Scripts/Systems/D12/D12MapDiscoveryCatalog.cs`
- `game/Assets/Tests/EditMode/D12MapDiscoverySystemTests.cs`

测试覆盖：未知信息不进入 UI、Agent 提案不直接解锁、社群/交易/任务/望远镜/测深的渐进发现、工具失败解释、唯一 owner 事实、D12 快照与共享 EventLog 读档一致性。
