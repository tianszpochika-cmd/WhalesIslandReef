# D14-A06 宗教、社群与历史审查接入

状态：已实现，2026-08-11 验证通过。

## 目标与边界

本成果把节期、饮食、建筑开放、葬礼、誓言、信任投影和社群礼仪接入统一事件与存档。它只写 `ReligionCommunitySystem` 自己拥有的宗教实践、仪式记录、誓言记录、审查记录和交互投影；最终裁决仍归属原系统：

| 结果 | 唯一裁决/写入者 | A06 的职责 |
|---|---|---|
| 宗教实践、节期、饮食、仪式表现和历史审查记录 | `ReligionCommunitySystem` | 校验来源、审查、写入事实和运行时状态 |
| 建筑开放、状态、开放时间 | `BuildingSystem` | 发起 authority 请求并保存开放结果投影 |
| 船员信任/关系 | D10 `RelationSystem` | 提议誓言信任变化，保存所属系统的决定来源 |
| 社群信任、礼仪结果 | D12 `CommunityTradeSystem` | 提议礼仪和信任变化，保存所属系统的决定来源 |
| 葬礼/善后最终结果 | `CommunitySystem`/`AftercareSystem` | 请求所属 authority，保存状态投影 |
| Agent/UI 表达 | Agent/UI | 只能生成表达或提交请求，不能写最终状态 |

代码位于 `game/Assets/Scripts/Systems/D14/`：

- `D14ReligionCommunityContracts.cs`：契约、请求、审查记录、运行时快照和 authority 接口。
- `D14ReligionCommunityCatalog.cs`：可审查内容、宗教、节期、饮食规则、建筑仪式和礼仪样例。
- `D14ReligionCommunitySystem.cs`：确定性规则、统一事件、唯一写者边界和防重复应用。

## 可用内容与确定性规则

样例内容均有稳定 Content ID、`historicalTag`、内容警告、来源说明、审查状态和跳过能力。当前样例以“港口堂区语境”作为项目抽象，不凭空声称历史事实；任何扩展必须先加入历史来源/审查事实。

- 节期 `festival_harbor_lantern`：30 日周期，第 4—6 日开放，完成后可作为礼仪语境。
- 饮食规则 `dietary_fast_day`：要求 `plant_staple` 食物标签；匹配时合规分数增加 10，并限制在 0—100。
- 建筑仪式 `building_rite_chapel_opening`：只在 06:00—18:00 的确定性窗口内请求 `building_green_cay_chapel` 开放裁决；窗口外返回 `building_rite_closed`，窗口内仍由 BuildingSystem 决定，A06 不改建筑状态。
- 葬礼 `funeral_rite_remembrance`：必须绑定善后案件/事实；最终是否举行由 Community/Aftercare authority 返回。
- 誓言 `oath_mutual_aid`：`Swear`、`Fulfill`、`Break`、`Release` 依次受状态机约束；提议信任变化分别为 `+5`、`+8`、`-10`、`+1`，实际应用值只接受 D10/D12 authority 的结果。
- 礼仪 `etiquette_formal_greeting`：要求同一角色近 7 日内完成非跳过节期和合规饮食实践，再请求 D12 社群裁决。

这些规则是玩法规则，不是对现实宗教的价值判断。事件中保留 `sourceEventId`、`historicalEvidenceEventId`、`evidenceEventIds`、`causeEventIds` 和所属 authority 的来源事件。

## 警告、表现等级和跳过

所有 A06 内容样例使用 `ApprovedWithWarning`，允许 `Full`、`Contextual`、`LowIntensity`、`Omitted`。请求必须引用 `HistoricalReviewSystem` 的审查事实：

1. 未确认警告时拒绝执行，返回 `warning_required`，不改变状态。
2. 玩家选择低强度时保留语境和事实，但降低表现等级。
3. 玩家选择跳过且内容允许跳过时，只写“已跳过”的审查/事实记录，不增加节期参与、饮食合规、信任、建筑开放或葬礼结果。
4. `Pending`/`Rejected` 内容拒绝进入逻辑；未知 Content ID 不能生成历史细节。
5. Agent 只能调用 `ProposeExpression` 生成表达草案；`agent`/`AgentReply` 作为写入角色会返回 `agent_cannot_write`。

## 事件与存档

成功操作写入 `d14_religion_community` 事件，事件 ID 使用 `d14rc_v1_*` 前缀；事件输入保留操作来源和审查证据，因果列表保留所属系统决定来源。所有操作以 `OperationId` 和事件去重键进入 `AppliedEventKeys`，重复读档或重放返回 `Duplicate`，不重复累加实践或信任。

运行时快照通过 `PlayerState.D14ReligionCommunityRuntime` 保存到 `location_json.d14ReligionCommunity`，包括宗教实践、审查、节期、饮食、建筑、葬礼、誓言、社群交互、去重键和事件日志。读取后重新构造系统会继续使用同一去重状态。

## 验证

`D14ReligionCommunitySystemTests` 覆盖：完整节期→饮食→建筑→社群礼仪→誓言→葬礼链路、D10/D12 信任来源、低强度/跳过、Agent 只表达不写入、保存/读取和重复操作。A06 独立执行器结果为 `2/2 PASS`。
