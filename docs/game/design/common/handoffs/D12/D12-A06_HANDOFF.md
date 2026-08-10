# D12-A06 交接报告

日期：2026-08-11  
任务：D12-A06 塔罗、占卜、信任、软线索与求证  
前置：D9-S02（按任务前置要求）

## 已交付

- `D12TarotCatalog`：6 张代表牌面和 1 名敏感宗教语境占卜者；所有条目带来源、授权、许可、历史标签、阶段、所属系统、test_id 和本地化 key。
- `D12TarotSystem`：确定性三牌抽取、正逆位、文化/宗教解释偏差、D12 占卜语境信任、软线索、敏感内容警告/跳过、解释文本隔离、求证和错误后果记录。
- `D12TarotRuntimeSnapshot`：接入 `D12RuntimeSnapshot.Tarot`，覆盖会话、牌、线索、解释、信任、求证、后果和幂等键。
- SaveSystem：D12 Tarot 快照进入现有 D12 存档编解码；旧存档缺少 `tarot` 时使用空快照。另修复了 SaveSystem 对已有 `D12PokerA07Wrapper` 的两个过时调用名，未修改牌局逻辑。
- 测试：EditMode 测试 6 个场景已登记并通过 `Save.Tests` 编译；独立 smoke 覆盖核心闭环。

## ID 前缀和代表样例

本任务没有修改 D8-S01 的正式内容 ID 前缀。D12 系统目录使用可定位的内部 ID：

| 类型 | 代表 ID | 本地化/来源 | test_id |
|---|---|---|---|
| 牌面 | `tarot_major_sun` | `tarot.card.sun` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-001` |
| 牌面 | `tarot_major_moon` | `tarot.card.moon` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-002` |
| 牌面 | `tarot_major_tower` | `tarot.card.tower` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-003` |
| 牌面 | `tarot_major_star` | `tarot.card.star` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-004` |
| 牌面 | `tarot_major_hermit` | `tarot.card.hermit` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-005` |
| 牌面 | `tarot_major_chariot` | `tarot.card.chariot` / `content:d12_tarot_major_arcana` | `D12-A06-CARD-006` |
| 占卜者 | `diviner_saint_marys_keeper` | `content:d12_diviner_saint_marys_keeper` / `caribbean_port_folk_practice_1675` | `D12-A06-DIVINER-001` |

## 目录字段

牌面字段：`CardId`、`DisplayName`、`DisplayNameKey`、`Arcana`、`MeaningTags`、`UprightHint`、`ReversedHint`、`SourceRef`、`AuthorizationRef`、`LicenseRef`、`HistoricalTag`、`Phase`、`OwnerSystem`、`TestId`。

占卜者字段：`DivinerId`、`DisplayName`、`CultureId`、`ReligionId`、`SourceRef`、`AuthorizationRef`、`LicenseRef`、`HistoricalTag`、`WarningId`、`WarningText`、`Sensitivity`、`BaseTrust`、`TrustSourceRef`、`TrustEvidenceRef`、`TestId`、`SkipAllowed`。

运行时字段：会话 `SessionId/DivinerId/QueryTag/Seed/WorldDay/SaveRevision`；牌面 `CardId/Orientation/Position`；软线索 `ClueId/TargetRef/Reliability/BiasCode/Status/SourceRef/EvidenceRef`；求证 `VerificationId/Method/Outcome/FactEventId`；后果 `ConsequenceId/ImpactTag/CostBand/CauseEventIds/DownstreamOwner`。

## 关键边界

- `D12TarotSystem` 是牌面、偏差、D12 占卜信任、软线索、求证和后果记录的唯一写者。
- `D10RelationSystem` 仍是关系值唯一写者；本任务的信任不是 `RelationEdge`，不会直接改关系。
- 天气求证只读 `D9WeatherSeaSnapshot`；没有把牌面或解释写回天气。
- 解释 provider 的 `llm_text_only` 只保存文本，事件载荷有 `affects_state=false`、`soft_only=true`、`supernatural=false`。
- 错误后果是带因果的标签记录，不直接扣钱、改奖励、改关系、改任务或改天气。
- 敏感宗教内容未确认警告不会抽牌；跳过会留下 `sensitive_content_skipped` 事件和来源/证据。

## 检查命令与结果

```text
dotnet run --project tools/d12_a06_smoke/D12A06Smoke.csproj --configuration Release
dotnet build game/Whalebone.Systems.csproj --configuration Release
dotnet build game/Save.Tests.csproj --configuration Release
dotnet test game/Save.Tests.csproj --configuration Release --list-tests
```

结果：

- D12-A06 smoke：PASS（牌面、敏感警告/跳过、LLM 解释隔离、天气/事实求证、错误后果、信任来源事件、恢复后幂等）。
- `Whalebone.Systems`：PASS，0 warning / 0 error。
- `Save.Tests`：PASS，0 warning / 0 error；D12 EditMode 测试源码已登记并编译。
- `dotnet test --list-tests`：当前 Unity/NUnit 编译项目没有配置可被 dotnet test 发现的测试执行器；不代表测试失败。运行行为由 smoke 覆盖，实际 EditMode 需由 Unity Test Runner 执行。
- Systems 红线：D12 新增 Systems 文件没有网络、HTTP 客户端或 LLM 调用。

## D13 可使用入口

D13 可以读取 `D12TarotSystem.BuildExplanationContext(sessionId)` 展示牌面与软线索；可以读取 `D12TarotVerificationState` 和 `D12TarotErrorConsequenceRecord` 的事实事件、来源、证据、结果和下游 owner。D13 不应把软线索当作确定事实，也不应绕过 `D12TarotSystem` 修改牌面或信任。若要产生实际经济、关系、任务或天气后果，必须由相应 canonical owner 消费 `EventLog` 事实并自行裁决。
