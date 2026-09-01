# D24 逻辑迁移与兼容性

> 对应任务：`D24-L28`。本文只描述当前纯逻辑迁移器、Save projection 和 JSON 兼容边界；不代表真实磁盘/云端 Save 已验证。

## 1. 当前支持版本

| 领域 | 支持事实 | 迁移/读取入口 | 拒绝策略 |
|---|---|---|---|
| AgentReply | `agent_reply.v1.1` | `contracts/agent-reply.schema.json` 与 game/server/web validator | Schema 不匹配、未知顶层结果键或越权结果不进入逻辑裁决 |
| WorldEvent | `schema_version=1` | `contracts/world-event.schema.json`、D8 event log | 缺 event/cause/owner/revision 或非法 outcome 时拒绝；事件层不代替 owner 裁决 |
| D19 港口 | `D19PortSaveProjection.CurrentSchemaVersion=1`；源 v0→v1 或当前 v1 | `D19PortSaveMigration.Migrate` | 未来/负版本、不可能世界日/修订、冲突锚点或数据损失状态返回失败；保留原始输入 |
| D20 角色 | 迁移器从 `D20CharacterSaveProjection.CurrentSchemaVersion` 读取当前版本；根/嵌套 v0→v1 | `D20CharacterSaveMigration.Migrate` | 未来/负版本、非法默认 player ID 或不一致投影拒绝；拒绝结果保留 `OriginalProjection` |
| D21 船舶/航程 | `D21ShipSaveProjection.CurrentSchemaVersion=1` | `D21ShipSaveProjection` serializer/validator | 版本、ID、Tick、owner snapshot、seed 或事件游标非法时不生成可继续状态 |
| D22 UI projection | 只保存 UI 逻辑投影（焦点/通知/幂等等），不保存领域事实 | D22 projection/serializer | 缺失或版本不匹配显式 degraded/blocked，不把旧 Canvas 状态当业务事实 |
| D23/D24 回归 | 固定 seed `2301`、world day `23`、save revision `230100` | D23 fixture、D24 replay runner | 任何摘要漂移、事件重复、快照不一致或 owner 越权使回归失败 |

当前契约版本和投影版本以源码/契约为准；不在文档中复制第二份可变 schema 常量。D24-L26 的 baseline 只冻结不会无意下降的关键指标和 marker。

## 2. 迁移策略

1. 先识别根对象是否缺失、版本是否为当前支持范围；不对未来版本做猜测性降级。
2. 对可安全补齐的字段创建防御性 clone，再写入稳定默认值/规范键；原对象不被污染，迁移标记保留在结果中。
3. 对别名、废弃键和旧 projection 只使用已有 canonical catalog 或既有 Save 根提供的稳定 ID；不凭字符串相似度生成新实体。
4. 完成迁移后重新 Validate、序列化和 fingerprint/round-trip；失败返回结构化 reason/error code 与人工恢复提示。
5. 相同输入重复迁移必须得到相同结果；已是当前版本的投影不得产生第二次业务副作用。

D24-L19 的 schema round-trip 测试验证代表性的 D19/D21/D22 DTO、Save 和事件 JSON 往返，以及未知字段不会改变已知字段结果。它是纯 serializer 证据，不是旧版本真实存档批量升级证据。

## 3. 兼容与数据丢失风险

| 风险 | 处理 | 仍需 Runtime/运维证据 |
|---|---|---|
| 未来 schema/version | fail closed，保留原始 projection，提示人工恢复 | 真实 SaveSystem 读取/备份/回滚 |
| 缺失可选块 | 只创建“空 projection/未发生”默认，不创建动作、NPC、动物或奖励事实 | 真实旧档样本和产品迁移 UX |
| 锚点/资源别名 | 仅按 canonical catalog 映射并留下 MigrationHint | 内容发布包与旧资源目录兼容 |
| 重复迁移/重放 | 版本单调、幂等 fingerprint、原始输入防御性保留 | 崩溃恢复、并发写入和跨设备同步 |
| 冲突/不可逆状态 | 返回失败码和恢复路由，不删除或覆盖原始数据 | 备份恢复、客服处理和真实数据演练 |
| 未知 JSON 字段 | 代表性 DTO 测试隔离未知字段；不可识别的必需字段仍拒绝 | 真实生产 payload 兼容策略和版本治理 |

禁止把默认值当作“数据已恢复”，禁止把 `Migrated` 当作 Runtime 可加载成功，也禁止在迁移器内写 D3/D19/D20/D21 之外的 owner 事实。

## 4. 真实证据

- `python tools/d24_save_migration_audit.py --root .`：D19 migration、D20 migration、D21 Save projection 三组输入文件和关键模式通过。
- `python tools/d24_logic_test_runner.py --root . --task D24-L19 --configuration Release`：schema round-trip 纯 C# 测试通过。
- `python tools/d24_regression_baseline_compare.py ...`：D19/D20/D21 migration 与 D19/D22 错误目录 marker 保留。

以上均为逻辑证据。真实 SaveSystem 介质、云同步、设备差异、崩溃恢复和发布升级仍登记为 Runtime Deferred。
