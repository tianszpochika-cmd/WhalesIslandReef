# D12-A06 塔罗与占卜垂直切片

状态：完成（2026-08-11）  
前置：D9-S02 必须通过；本切片复用 D8 EventLog、SaveSystem、D9 天气快照和 D10 关系边界。

## 目标与非目标

本切片把牌面、占卜语境、解释偏差、软线索、求证和错误后果做成可保存、可追溯的确定性闭环。塔罗在游戏中是历史民俗、宗教语境和心理安慰的表现，不产生真实超自然结果。

解释文本可以由模板、玩家输入或 LLM 文本适配器提供，但只能写入 `ExplanationText`。解释不会改变牌面、软线索、天气、奖励、金钱、关系、任务、鱼群或生态状态。

## 代表目录

这些是 D12 系统目录 ID，不新增或改写 D8-S01 的正式内容前缀；每个目录对象仍带来源、授权、许可、历史标签、阶段、所属系统和 `test_id`，供后续内容注册表接入：

| ID | 类型 | 本地化 key / 文化 | 来源与历史标签 | test_id |
|---|---|---|---|---|
| `tarot_major_sun` | 牌面 | `tarot.card.sun` | `content:d12_tarot_major_arcana` / `historical_tarot_major_arcana` | `D12-A06-CARD-001` |
| `tarot_major_moon` | 牌面 | `tarot.card.moon` | 同上 | `D12-A06-CARD-002` |
| `tarot_major_tower` | 牌面 | `tarot.card.tower` | 同上 | `D12-A06-CARD-003` |
| `tarot_major_star` | 牌面 | `tarot.card.star` | 同上 | `D12-A06-CARD-004` |
| `tarot_major_hermit` | 牌面 | `tarot.card.hermit` | 同上 | `D12-A06-CARD-005` |
| `tarot_major_chariot` | 牌面 | `tarot.card.chariot` | 同上 | `D12-A06-CARD-006` |
| `diviner_saint_marys_keeper` | 占卜者 | `content:d12_diviner_saint_marys_keeper` | `caribbean_port_folk_practice_1675` | `D12-A06-DIVINER-001` |

牌面定义同时有 `CardId`、`DisplayName`、`DisplayNameKey`、`Arcana`、`MeaningTags`、正/逆位观察提示、`SourceRef`、`AuthorizationRef`、`LicenseRef`、`HistoricalTag`、`Phase`、`OwnerSystem` 和 `TestId`。代表牌面全部为 `project-authored`，不引入无来源或无授权外部资源；牌面提示是问题方向，不是预言。

## 唯一写者与边界

| 数据 | 唯一写者 | 读取者/下游 |
|---|---|---|
| 抽牌、正逆位、牌阵、软线索 | `D12TarotSystem` | UI、解释适配器、日志/存档 |
| D12 占卜语境信任 | `D12TarotSystem` | 偏差计算、UI；不是 D10 `RelationEdge` |
| 事实事件与因果引用 | `EventLog` 接收 `D12TarotSystem` 已裁决的 `WorldEvent` | UI、审计、存档 |
| 天气/海况 | `EnvironmentSystem`；求证只读 `D9WeatherSeaSnapshot` | `D12TarotSystem` 只判断可验证条件 |
| 关系、奖励、价格、任务、生态 | 各自 canonical owner | 占卜只提供软线索或后果记录 |
| 解释文本 | 模板/LLM 文本适配器 | UI；不能进入裁决路径 |

UI 只能提交 `D12TarotSessionRequest`、`D12TarotExplanationInput`、`D12TarotVerificationRequest` 和后果记录请求，并展示返回的牌面、警告、解释和状态。UI、任务脚本和 Agent 不能直接添加牌面、修改信任或裁决求证结果。

## 确定性牌面和偏差

一次同样的 `diviner_id + query_tag + world_day + seed + action_id` 产生相同的 `session_id` 和三张不重复牌面。系统使用稳定 FNV-1a 风格哈希，不使用随机源、网络或 LLM。每张牌生成一个 `D12SoftClue`，包括：

- `clue_id`、`session_id`、`kind`、`target_ref`、`signal`；
- `reliability`（0..1）、`bias_code`、`status`；
- `source_ref`、`evidence_ref`、`world_day`、`save_revision` 和求证事件引用。

占卜信任为 0..100 的 D12 语境值，初始值和文化/宗教来源来自占卜者目录。它只决定解释偏差：

- `<35`：`cautious_distance`，可靠度修正 `0.55`；
- `35..70`：`cultural_lens`，可靠度修正 `0.70`；
- `>70`：`trusted_rapport`，可靠度修正 `0.82`。

这些修正只影响软线索的可读性；系统不会把信任值写入 D10 关系，也不会由解释直接改变天气、关系、奖励或任务。

## 敏感内容警告与跳过

`diviner_saint_marys_keeper` 标记 `ReligiousPractice`，拥有 `warning_id`、警告文本和 `SkipAllowed=true`。未确认警告时返回 `warning_required`，不创建会话、不抽牌、不写事件；选择跳过时记录 `sensitive_content_skipped`，会话为空牌面并保留来源/证据引用。确认后才进入普通牌面流程。警告明确说明牌面不是可验证的超自然结果。

## 求证与错误后果

求证必须提交 `source_ref` 和 `evidence_ref`，可选方式为：

- `WeatherObservation`：只读取 D9 天气/海况快照；能见度、风暴和船体风险等关系由固定规则判断；
- `EventRecord`：只能确认已经在 `EventLog` 存在的事实；缺失事实返回 `Unresolved`，不会补造事实；
- `WitnessRecord`：同样需要事实事件引用，不能把解释文本冒充证词。

结果为 `Confirmed`、`Contradicted` 或 `Unresolved`，写入 `D12TarotVerificationState` 和 `tarot_clue_verified` 事件。求证结果可以影响 D12 占卜语境信任（确认 `+2`、矛盾 `-4`），每次变化都带来源、证据和事件 ID。

错误后果写入 `D12TarotErrorConsequenceRecord`，包括 `impact_tag`、`cost_band`、是否跟随建议、因果事件、时间、存档版本和 `downstream_owner`：

- 已求证提示：`verified_guidance_recorded` / `none`；
- 未求证而跟随：`uncertainty_cost_recorded` / `evidence_delay`；
- 被证伪而跟随：`misinterpretation_recorded` / `recheck_time`；
- 拒绝建议：`advice_declined_no_domain_write` / `none`。

这些是可追溯的后果标签，不直接扣钱、消耗时间、改关系、改任务、改天气或改奖励。未来若需要实际后果，必须由对应 canonical owner 消费该事实并自行裁决。

## 事件、幂等与存档

事件 owner 固定为 `D12TarotSystem`，事件前缀为 `event_d12_tarot_`。会话、解释、求证、错误后果和信任事件都使用 `event_id + save_revision` 去重，并同时写入快照的 `AppliedEventKeys`。重复请求返回 `Duplicate`，不会重新抽牌、重复改变信任或追加后果。

`D12RuntimeSnapshot.Tarot` 保存：

- 会话、牌面、正逆位、软线索、解释文本和解释来源；
- 占卜者文化/宗教、信任、来源、证据、最后事件和应用键；
- 求证方式/结果/证据事件；
- 错误后果、代价标签、因果事件和下游 owner。

SaveSystem 只序列化/恢复快照，EventLog 仍保存事实。恢复后用保存的快照和事件日志重新创建 `D12TarotSystem`，重复请求仍返回 Duplicate。

## 代码与验证入口

- 契约：`game/Assets/Scripts/Systems/D12/D12TarotContracts.cs`
- 目录：`game/Assets/Scripts/Systems/D12/D12TarotCatalog.cs`
- 裁决：`game/Assets/Scripts/Systems/D12/D12TarotSystem.cs`
- EditMode 测试：`game/Assets/Tests/EditMode/D12TarotDivinationTests.cs`
- 独立冒烟：`tools/d12_a06_smoke/`

验证命令：

```text
dotnet run --project tools/d12_a06_smoke/D12A06Smoke.csproj --configuration Release
dotnet build game/Whalebone.Systems.csproj --configuration Release
dotnet build game/Save.Tests.csproj --configuration Release
```

本次结果：D12-A06 smoke 通过；`Whalebone.Systems` 和 `Save.Tests` 均 0 warning / 0 error。`dotnet test` 未列出测试执行器（现有 `Save.Tests.csproj` 是 Unity/NUnit 编译项目），因此 EditMode 测试以源码登记并通过 `Save.Tests` 编译，核心行为由独立 smoke 覆盖。

Systems 目录新增代码不包含网络、HTTP 客户端或 LLM 调用；LLM 仅作为外部解释文本 provider 标识，不能接触牌面或裁决。
