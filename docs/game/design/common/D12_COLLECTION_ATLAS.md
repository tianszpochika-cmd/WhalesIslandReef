# D12-A07 收藏与图鉴

## 边界与版本

本任务冻结 `D12CollectionAtlasSystem` 为 D12 图鉴状态的唯一写者，图鉴快照版本为 `D12CollectionAtlasState.schema_version=1`，随 `D12RuntimeSnapshot.schema_version=1` 的 `collectionAtlas` 字段保存。D10 外观、D11 生态/鱼获、D12 海图、D9 任务和船舶系统仍分别拥有自己的实体状态；图鉴系统只追加自己的记录，不回写这些领域状态，也不授予金钱、价格、伤害、奖励、天气或关系结果。

图鉴跨系统引用使用 D8 内容 ID。地图条目使用 D12 已冻结的地图内容 ID；历史文件和特殊武器已补入 D8 registry，并保持 `AssetRegistry` / `ItemCatalog` 为实体目录 owner，`CollectionAtlasSystem` 只拥有图鉴投影。

## 垂直切片条目

| 类别 | content_id | 事实来源 owner | 获得方式 | 授权 |
|---|---|---|---|---|
| 动物 | `animal_green_turtle` | `CollectionSystem` | observation / salvage | 否 |
| 植物 | `plant_red_mangrove` | `EcologySystem` | observation / harvest | 否 |
| 鱼类 | `fish_mackerel_school` | `EcologySystem` | observation / catch | 否 |
| 船 | `ship_moonwake` | `ShipSystem` | observation / purchase / transfer | 否 |
| 服装 | `outfit_career_fisher_handline` | `AppearanceSystem` | purchase / gift / transfer | 否 |
| 地图 | `reef_saint_marys_east` | `ExplorationSystem` | map_discovery | 否 |
| 历史文件 | `asset_historical_saint_marys_log` | `ArchiveSystem` | authorized_archive | `archive_permit_saint_marys` |
| 宝藏 | `reward_first_chart` | `QuestSystem` | quest_reward / salvage | 否 |
| 特殊武器 | `item_special_weapon_moonwake_cutlass` | `ArmorySystem` | authorized_armory | `armory_authorization_moonwake` |

每个定义同时冻结展示键、来源引用、成就 ID 和允许的获得方式。`D12CollectionCatalog.CreateVerticalSlice()` 是代码侧的可执行目录；D8 Content Registry 中的 JSON 是内容 ID、来源和许可证事实，不是已解锁状态。

## 证据链

`D12CollectionRecordRequest` 必须携带：`content_id`、类别、来源 `fact_event_id + fact_save_revision`、`evidence_ref`、`location_ref`、`world_day`、获得方式和 actor。系统只接受共享 `EventLog` 中已经存在的事实，并要求地点和时间与来源事实一致；Agent 或 UI 不能用一段文本伪造证据。

系统为每条实际证据写入 `D12CollectionEvidenceRecord`，至少保留：

- 统一 `content_id` 与类别；
- 来源 `fact_event_id` / `fact_save_revision`；
- `source_ref`、`evidence_ref`；
- `location_ref`、`world_day`、获得方式；
- 图鉴记录事件和授权引用。

历史文件和特殊武器还必须提供目录要求的 `authorization_ref`，并引用共享 EventLog 中的 `authorization_fact_event_id + authorization_save_revision`。授权引用不等于授权事实；没有事实的请求会被拒绝。

首次证据创建一条 `event_d12_collection_record_<content_id>_<stable_token>`，再由同一个系统创建 `event_d12_achievement_unlocked_<content_id>`。成就状态只由该系统写入，UI 只能消费 `Snapshot` / read model。后续新证据可以追加到同一记录，但不会再创建同一成就。

## 拒绝与幂等

系统拒绝以下情况：

- 未注册 content ID、类别不匹配、未允许的获得方式；
- 缺少来源事实、证据引用、地点或时间；
- 来源事实不存在，或请求地点/时间与事实不一致；
- 历史文件/特殊武器缺少正确授权引用或授权事实；
- 相同来源事实和证据重复提交。

重复提交返回 `Duplicate=true` / `duplicate_evidence`，不会追加 evidence、record 或 achievement。不同来源事实再次获得同一内容时只追加新的证据，`Achievements` 保持单条。

## 存档

`SaveSystem` 将 `D12RuntimeSnapshot.CollectionAtlas` 编解码到 `location_json.d12.collectionAtlas`，保存记录、证据、成就、revision 和 applied event keys。来源事实仍由 D3 共享 `EventLogSnapshot` 保存；读档顺序是先恢复共享事实，再把图鉴快照交还给 `D12CollectionAtlasSystem`。旧 D7/D8/D9 存档缺少 `collectionAtlas` 时按空 v1 快照读取，不改变已有实体或事件。

## 测试边界

`D12CollectionAtlasTests` 覆盖九类目录完整性、文本不能替代事实、授权事实、重复解锁和 Save/Load 后的重复请求；`D12A07Runner` 以 Unity NUnit DLL 直接执行四项测试。Content Registry 检查同时验证两个新增内容 ID 的 owner、测试 ID、根可达性和来源字段。
