# D10-A01 多维关系模型

## 目标与边界

D10-A01 将关系实现为有向 `RelationEdge`。玩家—船员、船员—船员和角色—NPC 都使用同一套数据结构；关系变化必须先由所属系统把事实追加到 D8 `EventLog`，再交给 `RelationSystem` 裁决。

`RelationSystem` 是 `RelationEdge.Value` 的唯一写者。Agent 只能产生 `RelationChangeIntent`，其中的 `ProposedDelta` 不是最终结果；UI、Quest、Agent 摘要和 SaveSystem 只能读取副本或传递快照。已有 D10 恋爱/阶段系统的 `D10RelationEdge` 是阶段表现投影，与 A01 canonical graph 共用 D10 存档容器，但不得替代或私藏 A01 数值边。

## RelationEdge 字段

| 字段 | 含义 | 写入者 | 保存策略 |
|---|---|---|---|
| `schema_version` | 关系边版本，当前为 `1` | RelationSystem | `location_json.d10.canonicalGraph` |
| `source_id` | 关系源角色 ID | RelationSystem 校验后写入 | 与内容/角色 ID 对齐 |
| `target_id` | 关系目标角色 ID | RelationSystem 校验后写入 | 与内容/角色 ID 对齐 |
| `relation_type` | 九类关系之一 | RelationSystem | 枚举字符串 |
| `value` | 当前关系值 | RelationSystem | 按类型 clamp |
| `confidence` | 事实关系置信度，当前事实写入默认为 `1` | RelationSystem | `0..1` |
| `cause_event_ids` | 直接事实事件及其上游因果 ID | RelationSystem | 非空；不伪造事件 |
| `last_changed_day` | 最近一次事实变化的世界日 | RelationSystem | 来自 FactEvent |
| `last_change_save_revision` | 最近一次变化的存档修订号 | RelationSystem | 用于精确解释/去重 |
| `locked_by_story` | 剧情锁标记 | 未来剧情 owner；A01 只保存 | 不由 UI 改写 |
| `visibility` | `public/private/rumored/hidden` | 关系域/剧情域 | UI 只读 |
| `applied_event_keys` | `event_id + save_revision` 已应用键 | RelationSystem | 读档后重复保护 |

旧 D7 `CrewStats.Affinity` 只作为兼容镜像；A01 canonical 结果不通过 `relations_json` 私有 JSON 保存。D8 旧迁移若缺原因，只能标记 legacy 来源，不能编造事实事件 ID。

## 九类关系与范围

| 类型 | 范围 | 说明 |
|---|---:|---|
| `affinity` | `-100..100` | 私人好感，可正可负 |
| `trust` | `0..100` | 对承诺和事实的信任 |
| `loyalty` | `0..100` | 留船、执行命令和长期承诺倾向 |
| `fear` | `0..100` | 服从压力，不等于好感 |
| `respect` | `0..100` | 对能力、判断和身份的敬意 |
| `attachment` | `0..100` | 亲密和依恋强度 |
| `jealousy` | `0..100` | 对第三方偏袒或关系竞争的反应 |
| `debt` | `0..1,000,000` | 欠债数值，非负；金额结算仍由 Finance/Economy owner 裁决 |
| `romance` | `0..100` | 恋爱倾向/投入度；同意与阶段由对应阶段投影裁决 |

Agent 提议单次变化最多为 `10` 个关系点；RelationSystem 再按类型边界裁决。Debt 关系边只保存关系域的欠债事实，不裁决金币转移。

## 事实写入链

```text
Agent / 行为系统提出 RelationChangeIntent
        ↓
所属领域系统裁决行为并 Append(WorldEvent)
        ↓
EventLog 返回 FactEvent（事实已经落日志）
        ↓
RelationSystem.ApplyFactEvent(FactEvent, intent)
        ↓
RelationEdge.Value + cause_event_ids + applied_event_keys
        ↓
SaveSystem 保存副本；D10RelationPanelReadModel 读取解释
```

`ApplyFactEvent` 拒绝未进日志的事实、版本不匹配的事实、未知源/目标角色和未引用关系双方的事实。关系系统不调用 EventLog.Append，避免把关系系统变成事实来源或绕过其他领域的所有权。

## 因果解释与缺失处理

关系面板通过 `D10RelationPanelReadModel` 读取 `RelationSystem.ReadEdges()` 的副本，再按 `cause_event_ids` 查询统一 EventLog：

- 找到事件：显示事件 ID、事实摘要、owner、世界日和状态；
- 事件缺失：显示“原因事实缺失：`event_id`”，不补造解释；
- source/target 未注册：返回 `unknown_character` 和明确角色 ID；
- 关系快照 schema 不匹配：`Restore` 抛出 `RelationSchemaVersionException`；事实事件 schema 不匹配则返回 `event_schema_version_mismatch`。

## 保存与重复保护

关系图作为 `D10RelationRuntimeSnapshot.CanonicalGraph` 写入现有 `location_json.d10` 容器，由 `SaveSystem.SaveD10RelationGraph` 接线。SaveSystem 不修改关系值。每条边保存 `applied_event_keys`；同一 `event_id + save_revision` 在首次读档前后都只能应用一次，重复请求返回 `duplicate_relation_event`，不会再次叠加关系值。

## 测试入口

`D10RelationModelTests` 覆盖九类关系、因果链、重复保护、读档后恢复、未知角色、缺失事实、事件版本错误、面板解释和 SQLite 保存/读档；旧 D2/D9 关系测试同时确认 AgentReply 不再直接写 Affinity。
