# D12-A07 交接报告

## 结论

D12-A07 已完成。图鉴状态由 `D12CollectionAtlasSystem` 唯一写入，所有条目必须引用共享 EventLog 中已存在的领域事实；重复获得不会重复解锁成就，保存/读档后状态和事实引用保持一致。

## 契约与版本

- `D12CollectionAtlasState.schema_version`: `1`
- `D12RuntimeSnapshot.schema_version`: `1`，新增 `collectionAtlas`
- `D12CollectionEntryDefinition`、`D12CollectionEvidenceRecord`、`D12CollectionRecord`、`D12CollectionAchievement`、`D12CollectionRecordRequest`、`D12CollectionResult`
- 未新增 server/web 私有协议；图鉴投影是 game Systems 本地状态，内容 ID 继续使用 D8 registry 作为跨端事实源。

## Action / 条目清单

本任务没有新增 P2A Action；外部调用入口是 `D12CollectionAtlasSystem.Record(D12CollectionRecordRequest)`，不是 UI 直接写状态。冻结条目如下：

1. `animal_green_turtle` — Animal
2. `plant_red_mangrove` — Plant
3. `fish_mackerel_school` — Fish
4. `ship_moonwake` — Ship
5. `outfit_career_fisher_handline` — Clothing
6. `reef_saint_marys_east` — Map
7. `asset_historical_saint_marys_log` — HistoricalFile，需 `archive_permit_saint_marys` 和授权事实
8. `reward_first_chart` — Treasure
9. `item_special_weapon_moonwake_cutlass` — SpecialWeapon，需 `armory_authorization_moonwake` 和授权事实

历史文件和特殊武器的实体目录 owner 仍分别为 `AssetRegistry` / `ItemCatalog`；图鉴只保存投影。动物/植物/鱼/服装/地图/任务事实仍由其既有 owner 产生。

## 拒绝逻辑

实现于 `game/Assets/Scripts/Systems/D12/D12CollectionSystem.cs`：

- `unknown_content_id`、`category_mismatch`、`acquisition_not_allowed`：内容/类别/方式不匹配；
- `fact_event_missing`、`evidence_ref_missing`、`location_ref_missing`：没有可追踪事实证据；
- `fact_context_mismatch`：地点或时间不是来源事实的地点/时间；
- `authorization_required`、`authorization_fact_missing`、`authorization_revision_invalid`：授权类条目缺授权或授权事实；
- `duplicate_evidence`：同一事实证据已经记录，不重复写成就。

系统不接受 Agent 或 UI 的直接解锁 API，不生成奖励、价格、钱、伤害、天气或关系数值。新增成就事件由 `CollectionAtlasSystem` 根据已记录证据写入 `event_d12_achievement_unlocked_<content_id>`。

## 跨端同步文件

- `game/Assets/Scripts/Systems/D12/D12CollectionContracts.cs`
- `game/Assets/Scripts/Systems/D12/D12CollectionCatalog.cs`
- `game/Assets/Scripts/Systems/D12/D12CollectionSystem.cs`
- `game/Assets/Scripts/Systems/D12/D12MapDiscoveryContracts.cs`（D12 runtime 包含图鉴快照）
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`（`location_json.d12.collectionAtlas` 编解码）
- `game/Assets/Resources/ContentRegistry/registry.json`
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_historical_saint_marys_log.json`
- `game/Assets/Resources/ContentRegistry/entries/item/item_special_weapon_moonwake_cutlass.json`
- `game/Assets/Tests/EditMode/D12CollectionAtlasTests.cs`
- `tools/d12_a07_runner/D12A07Runner.csproj`、`tools/d12_a07_runner/Program.cs`
- `docs/game/design/common/D12_COLLECTION_ATLAS.md`

## 测试命令与结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162 -v:minimal` — PASS，0 warning / 0 error
- `dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162 -v:minimal` — PASS，0 warning / 0 error
- `dotnet run --project tools/d12_a07_runner/D12A07Runner.csproj` — PASS，`D12-A07 tests PASS count=5`
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` — PASS，`79 entries, 13 prefixes`
- Systems 红线扫描：D12 新增系统无网络、LLM 或 Unity 依赖；只依赖共享 EventLog 和 DTO。
- `git diff --check` — PASS。

## 工作区集成修复记录

为使当前工作区的已有 D12 工程入口可编译，本任务同步了必要的工程包含项，移除了测试工程中不存在的 `D12CommunityTradeSystemTests.cs` 残留引用，并修正了已有 salvage 测试的多参数 helper 调用；未删除 D7/D12 领域代码，未复制领域裁决逻辑。

## 后续注意

D12-A07 不拥有 D10/D11/D12 领域实体；后续 UI 只应读取图鉴快照与 EventLog 事实。若新增图鉴条目，先更新 D8 registry 和目录定义，再补来源事实与幂等测试；不要让 Agent 或 UI 绕过 `Record`。
