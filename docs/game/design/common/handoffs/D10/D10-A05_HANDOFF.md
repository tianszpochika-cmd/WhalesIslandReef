# D10-A05 交接报告

任务：`D10-A05`  
日期：2026-08-10  
状态：**已完成实现，待主 Unity 编辑器释放后归档定向 EditMode XML**

## 主成果

- 规范：[D10_APPEARANCE_OUTFIT.md](../D10_APPEARANCE_OUTFIT.md)
- 外观契约、目录和系统：`game/Assets/Scripts/Systems/D10/D10AppearanceContracts.cs`、`D10AppearanceCatalog.cs`、`D10AppearanceSystem.cs`
- Unity 请求入口：`game/Assets/Scripts/Gameplay/D10/D10AppearanceRuntimeBootstrap.cs`
- 代表内容：`game/Assets/Resources/ContentRegistry/entries/outfit/`
- 测试：`game/Assets/Tests/EditMode/D10AppearanceOutfitTests.cs`
- 纯 C# 行为烟测：`tools/d10_a05_smoke/`

## ID 前缀

沿用 D8-S01 的 13 个前缀，不新增前缀：

`ship_`、`crew_`、`island_`、`building_`、`item_`、`animal_`、`plant_`、`fish_`、`quest_`、`reward_`、`outfit_`、`pet_`、`asset_`。

本任务的外观条目全部使用 `outfit_`，资源引用使用 `asset_`，染料/职业装备依赖继续使用 `item_`。

## 目录字段

所有 14 条 D10 代表样例均具备 D8 强制字段：

`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。

代表样例覆盖 `Body`、`Hair`、`Hat`、`Top`、`Bottom`、`Coat`、`Shoes`、`Gloves`、`Accessory`、`CareerEquipment`、`Injury`，并包含一个缺失资源占位条目。

## 检查命令与结果

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
# 结果：Content registry OK（35 entries / 13 prefixes）

dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild
# D10/Systems 代码无错误；依赖的既有 D10CrewBehaviorState.cs 发出 8 个 CS0162 警告，0 error，未由本任务改动

dotnet run --project tools/d10_a05_smoke/D10A05Smoke.csproj --no-restore
# 预期：D10-A05 smoke PASS
```

`game/Save.Tests.csproj` 当前工作区仍被既有的 `D10CrewBehaviorSystemTests.cs` 缺失 `D10CrewBehaviorSystem` 类型阻断；该错误不来自本任务。D10 外观测试已加入同一 EditMode 测试程序集，需主 Unity 编辑器释放后执行 `D10.Tests.D10AppearanceOutfitTests` 并归档结果。

## 代表样例与运行入口

- 默认组合：身体、短发、草帽、亚麻上衣、帆布长裤、甲板短靴；外套和职业装备按上下文购买/装备。
- 裁缝：`RequestTryOn`、`RequestPurchase`、`RequestEquip`、`RequestDye`、`RequestRepair`。
- 伤病：`ApplyInjuryPresentation`；普通装备请求遇到伤病层返回 `InjuryProtected`，不会抹除历史。
- 保存：`D10AppearanceRuntimeBootstrap.SaveProgress()`；D10 外观保存在 `location_json.d5.appearance`，与 D3 EconomySystem 的金币保存分开但属于同一存档。

## 边界与风险

- `D10AppearanceSystem` 是外观状态唯一写者；UI 不持有内容目录或私有金钱字段。
- EconomySystem 是金币扣除唯一写者；AppearanceSystem 不直接写 `D3TradeState.Money`。
- 资源缺失返回 `MissingResourcePlaceholder` 和具体 `asset_` ID，不下载、不静默失败。
- 旧 D5 角色预设仍可读取；D10 外观作为新节点兼容旧存档。

## D11 可用入口

D11 可直接读取：

1. `D10AppearanceSystem.Snapshot` 的槽位、所有权、耐久、染色和伤病历史；
2. `D10AppearanceCatalog.Find(outfitId)` 的内容标签和 `assetId`；
3. `D10AppearanceRuntimeBootstrap.LastResult` 的资源缺失/限制提示；
4. `SaveSystem.LoadD5Runtime().Appearance` 的读档快照。

D11 不应复制服装目录、直接改外观快照、重算价格或清除 `InjuryHistoryIds`。
