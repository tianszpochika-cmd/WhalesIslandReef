# D10-A05 外观与服装组合

> 任务：`D10-A05`　> 版本：`d10.appearance.outfit.v1`　> 日期：2026-08-10
>
> 前置放行：`D9-S02` 已通过；D8 内容 ID、存档和边界冻结继续有效。

本切片把角色外观拆成可保存、可校验的组合层。它只登记少量代表样例，不批量伪造服装资源；Unity 资源缺失时保留结构状态并返回明确的占位错误。

## 1. 入口与写者

| 领域 | 唯一写者 | 其他模块可做的事 |
|---|---|---|
| 外观槽位、外观所有权、耐久、染色、伤病表现历史 | `D10AppearanceSystem`（`AppearanceSystem`） | UI/Agent 只能提交请求；医疗入口可提交伤病表现 ID |
| 购买、染色、维修的金币扣除 | `TradeEconomySystem`（`EconomySystem`） | AppearanceSystem 只能调用 `TrySpend`，不直接改 `Money` |
| Unity 模型/材质资源是否可加载 | `D10AppearanceRuntimeBootstrap` 的资源适配 | 系统只接收 `asset_` 是否存在的布尔结果，不保存 Unity 对象 |
| 旧 D5 角色预设 | `D5CharacterSystem` | D5 `Character.Outfit` 保留兼容投影，不覆盖 D10 `Appearance` |
| 目录 | `ContentRegistryReader` / `D8` 注册表 | UI 不维护内容目录 |

`game/Assets/Scripts/Systems/` 中的 D10 文件是纯本地强逻辑，没有网络、HTTP 或 LLM 代码。

## 2. ID 与目录

D8-S01 的 13 个前缀不变：

```text
ship_ crew_ island_ building_ item_ animal_ plant_ fish_
quest_ reward_ outfit_ pet_ asset_
```

D10-A05 外观层使用：

- 外观条目：`outfit_`，例如 `outfit_hat_straw_deck`、`outfit_coat_rain_oilskin`；
- Unity/占位资源：`asset_`，例如已有的 `asset_ph_crew_male`；
- 染料和服装所依赖的物品：继续使用 `item_`，例如已登记的 `item_dye_indigo`、`item_gear_handline_basic`；
- 不新增 `body_`、`hair_`、`gear_` 等第二套前缀。

正式目录入口：

```text
game/Assets/Resources/ContentRegistry/
├── registry.json
└── entries/outfit/
    ├── outfit_body_sun_bronzed.json
    ├── outfit_hair_short_brown.json
    ├── outfit_hat_straw_deck.json
    ├── outfit_top_linen_deckhand.json
    ├── outfit_bottom_canvas_trousers.json
    ├── outfit_coat_rain_oilskin.json
    ├── outfit_shoes_deck_boots.json
    ├── outfit_gloves_ropework.json
    ├── outfit_accessory_compass_badge.json
    ├── outfit_career_fisher_handline.json
    ├── outfit_injury_bandaged_arm.json
    ├── outfit_injury_old_leg.json
    ├── outfit_ceremonial_silk_coat.json
    └── outfit_missing_tailor_placeholder.json
```

每条正式目录内容继续填写 D8 全字段：`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。D10 样例的 `owner_system` 为 `AppearanceSystem`，`phase` 为 `D10`，且 `test_id` 保持 D8 注册表的 `D8-A02-*` 机器校验格式。

## 3. 外观组合模型

`D10AppearanceSlot` 固定 11 层：

| 槽位 | 示例 | 说明 |
|---|---|---|
| `Body` | `outfit_body_sun_bronzed` | 身体/肤色基础表现 |
| `Hair` | `outfit_hair_short_brown` | 发型和发色表现 |
| `Hat` | `outfit_hat_straw_deck` | 帽子 |
| `Top` | `outfit_top_linen_deckhand` | 上衣 |
| `Bottom` | `outfit_bottom_canvas_trousers` | 下装 |
| `Coat` | `outfit_coat_rain_oilskin` | 外套、天气标签 |
| `Shoes` | `outfit_shoes_deck_boots` | 鞋 |
| `Gloves` | `outfit_gloves_ropework` | 手套 |
| `Accessory` | `outfit_accessory_compass_badge` | 饰品、职业/文化标签 |
| `CareerEquipment` | `outfit_career_fisher_handline` | 职业装备外观层；依赖 `item_gear_handline_basic` |
| `Injury` | `outfit_injury_bandaged_arm` / `outfit_injury_old_leg` | 伤病叠加表现，不由普通换装清除 |

目录定义与运行时状态分离：定义保存 ID、`asset_id`、本地化 key、价格、维修费、最大耐久、可染色属性以及文化/职业/天气/历史标签；`D10AppearanceSnapshot` 保存当前上下文、活动槽位、拥有状态、耐久、染料和伤病历史。运行时不把名称或随机文本当成鱼获/服装结果。

## 4. 裁缝服务与失败码

`D10AppearanceRuntimeBootstrap` 的 `Request*` 方法只是请求入口；它把结果交给 `D10AppearanceSystem`，成功的持久化操作再写入 D5 快照中的 `appearance` 节点。

| 请求 | 系统行为 | 典型结果 |
|---|---|---|
| `RequestTryOn(outfitId)` | 生成预览组合，不授予所有权、不改活动槽位 | `Success` 或 `MissingResourcePlaceholder` |
| `RequestPurchase(outfitId)` | 校验标签，使用 `TradeEconomySystem.TrySpend`，写入所有权 | `Success`、`InsufficientFunds`、`AlreadyOwned` |
| `RequestEquip(outfitId)` | 只允许已拥有条目进入其声明槽位 | `Success`、`NotOwned`、`SlotConflict` |
| `RequestDye(outfitId, dyeId, color)` | 检查已登记的 `item_dye_` 条目并收取固定服务费 `DyeServiceCost=6` | `Success`、`InvalidDye`、`NotDyeable` |
| `RequestRepair(outfitId)` | 使用条目 `RepairCost`，恢复到 `MaxDurability` | `Success`、`AlreadyRepaired`、`InsufficientFunds` |
| `ApplyInjuryPresentation(injuryIds)` | 更新当前伤病表现，同时追加历史 ID | `Success`；普通换装不会调用清除逻辑 |

价格字段只来自项目登记的内容定义和 AppearanceSystem 的裁缝服务规则；金币实际扣除只发生在 EconomySystem。Agent 不得提交最终价格、奖励或文化限制。

文化、职业和天气不作为 UI 文本建议，而是系统校验：例如默认 `deckhand` 身份试穿 `outfit_accessory_compass_badge` 会返回 `CareerRestricted`；职业装备的天气标签不匹配会返回 `WeatherRestricted`。错误信息总是带具体 `outfit_` ID。

## 5. 缺失资源与历史表现

每个外观定义都保存 `AssetId` 和 `ResourceKey`。运行时通过 `Resources.Load<GameObject>("D10/Appearance/" + assetId)` 查询可用资源：

- 找到资源：`ResourceStatus=ready`；
- 找不到资源：操作结果仍可保存结构化状态，但 `Code=MissingResourcePlaceholder`、`ResourceStatus=placeholder_missing_resource`、`UsesPlaceholder=true`，消息包含具体 `asset_` ID；
- 不允许静默跳过、临时下载或把未授权外部资源放入正式目录。

伤病表现使用 `ActiveInjuryIds` 和不可清除的 `InjuryHistoryIds` 两组字段。恢复/换装只可以改变当前叠加层，不会删除曾经发生的伤病历史。

## 6. 保存与读档

D5 `D5RuntimeSnapshot` 新增独立 `Appearance` 字段，旧的 `Character` 预设字段仍保留兼容性。`SaveSystem` 将其序列化到 `location_json.d5.appearance`：

```text
appearance
├── schemaVersion
├── context: cultureId / careerId / weatherId / legalIdentityId
├── slots: slot -> outfit_ ID
├── owned: itemId / durability / dyeId / dyeColor / purchaseCount
├── activeInjuryIds
├── injuryHistoryIds
└── saveRevision
```

旧 D5 存档没有 `appearance` 时使用 D10 starter 快照，未知的旧字段不被删除。D10 保存入口先调用 D3 Bootstrap 保存同一个 EconomySystem 的金币，再调用 `SaveSystem.SaveD5Runtime` 保存外观状态；读档后 `D10AppearanceRuntimeBootstrap` 从同一快照重建系统。

## 7. 检查与测试

内容目录检查：

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

系统编译：

```powershell
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild
```

无 Unity 依赖的行为烟测：

```powershell
dotnet run --project tools/d10_a05_smoke/D10A05Smoke.csproj --no-restore
```

定向 Unity EditMode 测试类：

```text
D10.Tests.D10AppearanceOutfitTests
```

覆盖内容：11 层 starter 组合、试穿不授予所有权、购买/装备/染色/维修走 EconomySystem、资源缺失占位、文化/职业限制、伤病历史保留和 SQLite 保存/读档。

## 8. 非目标

本切片不批量生成服装网格、材质或外部资源，不新增第二套内容目录，不把 D5 预设复制成第二套角色逻辑，不改变 D9 航行/天气/钓鱼/交易公式，也不把外观数值交给 Agent 或 UI。
