# D10-A03 关系阶段、恋爱与冲突事件

任务：D10-A03  
日期：2026-08-10  
状态：已实现关系动作的纯 C# 强逻辑切片，待主 Unity 工程释放锁后执行定向 EditMode 测试。

## 1. 前置与兼容说明

D9-S02 的现有验收报告和交接报告均标记为 PASS，并放行 D10。D10-A01 的 `D10RelationContracts.cs`/`RelationSystem.cs` 与 `D10_RELATION_MODEL.md` 已提供 canonical 九类关系边；本任务只接入这些公共类型，不重新定义 A01 的关系边，也不把阶段投影冒充成 canonical 数值。D10 仍兼容 D8 `RelationEntity` 结构（`source_id`、`target_id`、`cause_event_ids`、`last_changed_day`、`locked_by_story`、`visibility`）以及 D2/D9 的关系提议边界。

`D10RelationEdge` 保持这些稳定字段，并在 D10 增加阶段、恋爱同意、依恋、尊敬、嫉妒、冲突和承诺字段。A01 的 `RelationGraphSnapshot` 作为 canonical 多维关系图保存在同一 `location_json.d10.canonicalGraph` 容器中；A03 的 `RefreshFromCanonicalGraph` 只读取 A01 的好感、信任、依恋、尊敬和嫉妒边刷新阶段投影，不覆盖或替代 A01 的九类关系边。

## 2. 唯一写者与读取边界

| 数据 | 唯一写者 | 读取者 | 备注 |
|---|---|---|---|
| A01 canonical 九类关系边（`canonicalGraph`） | A01 `RelationSystem` | Crew、Quest/Event、D10 阶段系统、UI、Agent 摘要 | A01 关系数值唯一写者；A03 不直接改写这些边 |
| A03 阶段投影、恋爱同意、承诺和冲突状态 | `D10RelationSystem`（CrewSystem 的关系子系统） | Crew、Quest/Event、UI、Agent 摘要 | 只有 `Apply` 成功后写入；UI/Agent 不持有写引用 |
| 关系事实事件 | `D10RelationSystem.Apply` 通过共享 `EventLog.Append` | UI、Agent、Quest、存档 | 共享 D9 EventLog，不创建第二份日志 |
| 礼物成本/物品 | Inventory/Economy owner | RelationSystem 只接收已完成事实或请求 | 本任务不裁决金钱、库存或物品数量 |
| 个人任务状态 | QuestSystem | RelationSystem 读取 `ContextRef`/cause IDs | 本任务只记录 personal task 关系后果，不写 Quest 完成 |

玩家/Agent 的表达必须先转换为经过边界校验的 `D10RelationActionRequest`。请求不是最终结果，不能携带关系数值。系统先追加事实事件，追加成功后才提交关系快照。

## 3. 关系阶段

阶段不是对白标签，而是由关系边上的确定性字段计算：

| 阶段 | 进入条件 | 典型离开条件 |
|---|---|---|
| `Acquaintance` 认识 | 默认阶段；信任不足 30 | 信任达到 30 |
| `Trust` 信任 | 信任 ≥ 30 | 依恋 ≥ 45 且信任 ≥ 45，或进入冲突 |
| `Intimacy` 亲密 | 依恋 ≥ 45 且信任 ≥ 45 | 恋爱承诺阈值达成，或冲突 |
| `Commitment` 承诺 | 恋爱同意为 Accepted、承诺 ≥ 60、信任 ≥ 60、依恋 ≥ 55 | 冲突或主动分离 |
| `Conflict` 冲突 | `conflict_active=true` | 冲突分数降至 20 以下且完成调解 |
| `Stable` 稳定 | 有冲突历史、冲突已解除、信任 ≥ 35、好感 ≥ -10 | 新争执/嫉妒，或主动分离 |
| `Separation` 分离 | 显式 `Separate`，或已承诺路线被拒绝/结束 | D10 本切片不自动复合 |

阶段推进只能由关系事实触发，不能由一句对白直接跳跃。

## 4. 关系动作与因果事件

| 动作 | 事件 ID | 主要确定性影响 |
|---|---|---|
| 礼物 `Gift` | `event_d10_relation_gift` | 好感 +6、信任 +2、依恋 +1 |
| 守约 `PromiseKept` | `event_d10_promise_kept` | 好感 +3、信任 +10、承诺 +8、冲突下降 |
| 救援 `Rescue` | `event_d10_rescue` | 好感 +5、信任 +14、依恋 +8、尊敬 +4 |
| 公开奖励/偏袒 `PublicFavoritism` | `event_d10_public_favoritism` | 目标好感 +4、信任 +2；旁观者增加嫉妒关系 |
| 嫉妒 `Jealousy` | `event_d10_jealousy` | 好感 -5、信任 -7、嫉妒 +18、冲突 +25 |
| 争执 `Argument` | `event_d10_argument` | 好感 -7、信任 -10、冲突 +35，进入 Conflict |
| 夜话 `NightTalk` | `event_d10_night_talk` | 好感 +4、信任 +6、依恋 +8、冲突下降 |
| 个人任务 `PersonalTask` | `event_d10_personal_task` | 信任 +8、尊敬 +6、依恋 +4、承诺 +6 |
| 接受恋爱 `RomanceAccept` | `event_d10_romance_accepted` | 需要 Intimacy、好感 ≥20、信任 ≥45、依恋 ≥30；设置 Accepted |
| 拒绝恋爱 `RomanceDecline` | `event_d10_romance_declined` | 设置 Declined、承诺归零；不会自动进入恋爱 |
| 冲突调解 | `event_d10_conflict_stabilized` | 信任 +5、尊敬 +2、冲突 -40；满足条件后进入 Stable |
| 主动分离 `Separate` | `event_d10_relation_separated` | 设置 Separated、承诺归零、阶段 Separation |

所有数值均裁剪到明确范围：好感 `-100..100`，其余关系指标 `0..100`。同一动作的存档去重键为 `event_id + save_revision`；不同关系动作应使用不同 `save_revision`。

## 5. 玩家—船员与船员—船员

关系边是有向的，`source_id -> target_id` 生成稳定 ID `relation:{source_id}->{target_id}`。同一系统同时支持：

- `player_captain -> crew_isaac_reed`：玩家与船员关系；
- `crew_isaac_reed -> crew_second`：船员与船员关系；
- `PublicFavoritism` 的 `AffectedSourceIds`：船员看到玩家偏袒目标后生成旁观者→目标的嫉妒边。

本任务不将关系边强行变成双向值；如玩法需要互相承诺，必须分别提交两条有向关系事实。

## 6. 恋爱拒绝和内容边界

- `RomanceAccept` 必须先满足亲密、信任、好感和依恋条件，不能由对白直接跳过。
- 任一方向的关系边一旦是 `Declined` 或 `Separated`，后续恋爱接受会被拒绝。
- 玩家拒绝路线只改变恋爱同意和必要的承诺状态，不强制把普通关系变成恋爱或失败。
- 已经进入承诺后再拒绝会记录分离事实；本切片不自动复合。
- 没有露骨内容；礼物、守约、救援、偏袒、嫉妒、争执、夜话和个人任务只通过事实、对白和关系状态表达。
- A01 canonical 图刷新：`RefreshFromCanonicalGraph` 只把已裁决的好感、信任、依恋、尊敬和嫉妒读入 A03 阶段投影；不反向修改 canonical `RelationEdge.Value`，也不从 `romance` 倾向推断恋爱同意。

## 7. 存档与恢复

关系运行时快照保存在现有 `location_json.d10`：

```text
d10 = {
  schemaVersion, worldDay, revision, lastEventId,
  appliedEventKeys,
  relations: [
    { schemaVersion, id, sourceId, targetId, phase,
      romanceConsent, affinity, trust, attachment, respect,
      jealousy, conflictScore, commitment, conflictActive,
      conflictHistory, lockedByStory, visibility,
      lastChangedDay, causeEventIds }
  ],
  canonicalGraph: { schemaVersion, edges: [...] }
}
```

D10 关系事件事实不复制到第二个日志，而是随 D9/D3 的共享 `EventLogSnapshot` 保存。`SaveSystem.SaveD10Runtime` 只序列化关系快照；`SaveSystem.SaveD3Runtime` 负责同一共享 EventLog。读档后恢复快照和 EventLog，再提交同一动作时，快照去重键和 EventLog 去重键均阻止重复关系变化。

旧 D7/D8/D9 存档没有 `d10` 时，关系系统从空快照开始，不伪造恋爱或冲突事实；旧 `affinity` 仍由 D2/CrewAgent 兼容字段保存。缺少 `canonicalGraph` 时只恢复空 A01 关系图，不影响 A03 阶段投影的安全默认值。

## 8. 测试入口

测试文件：`game/Assets/Tests/EditMode/D10RelationRomanceTests.cs`。

覆盖：

- 认识→信任→亲密→承诺阈值推进；
- 争执→冲突→调解→稳定→分离回退/分支；
- 玩家/船员关系和船员/船员关系；
- 礼物、守约、救援、偏袒、嫉妒、夜话和个人任务事件；
- 恋爱前置条件与玩家拒绝后的不可强制重入；
- 事件 owner、因果字段、重复提交、D10 快照保存/恢复和共享 EventLog 恢复。
- A01 canonical 图读取刷新及不修改 canonical 值。
