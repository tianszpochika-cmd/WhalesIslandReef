# D15-A05* 交接报告

- 任务编号：D15-A05*
- 日期：2026-08-11
- 前置：D15-S01 通过；代表道具/服装样例已完成；D14-S01 通过
- 结论：本批 200 个功能道具和 300 个装饰对象已进入统一 Content Registry，目录/授权/owner/测试/资源接入检查通过

## 1. 交付文件

### 目录与资产

- `game/Assets/Resources/ContentRegistry/entries/item/`：本批新增 335 个 `item_` 条目，覆盖功能道具、家具、船舱装饰和收藏品
- `game/Assets/Resources/ContentRegistry/entries/outfit/`：本批新增 165 个 `outfit_` 条目，覆盖服装、发型和饰品
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_d15_item_prop_placeholder.json`
- `game/Assets/Resources/ContentRegistry/entries/asset/asset_d15_outfit_placeholder.json`
- `game/Assets/Prefabs/D15/d15_item_prop_placeholder.prefab`
- `game/Assets/Prefabs/D15/d15_item_prop_placeholder.prefab.meta`
- `game/Assets/Resources/ContentRegistry/registry.json`：保留 13 个冻结前缀并登记本批内容 root

### 读取、测试与工具

- `game/Assets/Content/Registry/ContentRegistryReader.cs`：读取 D15 扩展字段；不写运行时状态
- `game/Assets/Tests/EditMode/D15ItemOutfitDecorTests.cs`
- `game/Save.Tests.csproj`：登记 D15 批次 EditMode 测试
- `tools/d15_a05_content_check.py`

### 文档与留痕

- `docs/game/design/common/D15_ITEM_OUTFIT_DECOR_BATCH_CATALOG.md`
- `docs/game/design/common/handoffs/D15/D15-A05_HANDOFF.md`
- `.workbuddy/memory/2026-08-11.md`

## 2. 数量和内容覆盖

专用批次过滤 `production_batch=D15-A05*` 后：

| 类型 | 数量 | 细分 |
|---|---:|---|
| 功能 | 200 | 工具 30、武器 25、渔具 25、种子 25、食物 25、鱼获处理 25、服装 15、发型 15、饰品 15 |
| 装饰 | 300 | 家具 90、船舱 60、收藏品 30、装饰服装 50、装饰发型 30、装饰饰品 40 |
| 合计 | **500** | 335 个 item_ + 165 个 outfit_ |

覆盖分类：工具、武器、渔具、种子、食物、鱼获处理、服装、发型、饰品、家具、船舱、收藏品。

## 3. 数据边界和功能接入

- `ItemCatalog` 负责 item 定义入口；Inventory/Cargo、Fishing/Processing、Farming、Economy、Reward、Combat 等既有系统继续写运行时状态。
- `AppearanceSystem` 负责 outfit 定义入口；服装、发型、饰品不直接写价格、耐久、身份法律结果或关系。
- D14 特殊武器/武器物资仅登记 `D14SpecialEquipmentSystem/definition_or_item_catalog` 入口；伤害、强度、耐久、装配和法律结果仍由 D14/Combat/Ship/Legal owner 裁决。
- 家具、船舱和收藏品只登记 `D3InventorySystem`/`D12CollectionAtlasSystem` 的交互入口；不创建产权、成就或摆放实例状态。
- 每条本批内容都有 `function_entry`（功能对象）或 `interaction_entry`（装饰对象）、`logic_ref`、`integration.system` 和 `canonical_write_owner`。
- `ContentRegistryReader` 读取批次、资源状态、模型/材质/碰撞、LOD、历史审查和接管字段；它不成为任何运行时实体的写者。

未修改 `game/Assets/Scripts/Systems/`，没有加入网络、HTTP 或 LLM；未修改 contracts，因本批内容是 game 内容定义，跨端状态仍由现有契约/领域 owner 处理。

## 4. 资产质量和授权

- 新增 `d15_item_prop_placeholder.prefab` 使用 Unity 内置 Cube、项目 `ph_mat_dark_wood.mat` 和明确的 `BoxCollider`，用于 item/家具/船舱的可加载和交互抽样。
- outfit 复用已登记的 `ph_crew_male.prefab` 和 `ph_mat_crew_cloth.mat`；穿戴层明确使用角色身体碰撞，不伪造服装独立碰撞。
- 所有条目 `quality_level=Q2`、`asset_state=placeholder`、`production_stage=integrated`；占位资源可替换，不能被误报为 Q3 最终美术。
- 每条条目含来源、授权、商业使用许可、历史标签、审查状态和 test_id。武器采用 `ApprovedWithWarning` 并带 `content_warning_historical_weapon_context`；其他内容为 Approved。
- 目录条目没有价格、伤害、奖励、耐久、重量、体积、强度或掉落率字段。

## 5. 测试命令与结果

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python tools/d15_a05_content_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools/tests/test_d15_a05_content_check.py
dotnet build game/Whalebone.Content.Runtime.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
```

结果：

- Content Registry：`2,812 entries / 13 prefixes`，通过；当前工作区含其他并行 D15 批次，不能用全量数替代本批数量；
- D15-A05 专用检查：`functional=200 decorative=300 total=500`，12 类覆盖，50 条加载/交互抽样通过；
- Python 单元测试：`1/1` 通过；
- Content Runtime 构建：通过（0 警告 / 0 错误）；
- Save.Tests 构建：被既有 `CrewAgent.cs` 缺少 `D10CrewBehaviorState` 阻断；本任务新增测试已登记，未改变领域逻辑；
- Unity EditMode 测试入口：`D15ItemOutfitDecorTests.D15A05Batch_Loads_AndHasOwnerInteractionAndAssets`。

## 6. 冲突和保留事项

1. 工作区已有 D15-A01/A02/A03/A04/A06/A07 的并行内容和 `registry.json` 根列表。D15-A05 保留现有 13 个前缀与其他批次条目，只加入本批 ID；全量扫描以当前工作区为准，本批数量使用 `production_batch` 过滤。
2. `ContentRegistryReader` 已存在 D15-A03 扩展字段，本任务在其基础上补充 `content_category`、`batch_role`、`interaction_entry`、`function_entry` 和 `integration`，没有删除已有动物/建筑/船体字段。
3. 代表样例和最终 Q3 美术不在本批重复生产；共享 Q2 placeholder 已具备模型、材质、碰撞/无碰撞理由和用途，正式美术替换由后续资产 QA/美术任务负责。
4. 本批没有新增价格、伤害、奖励、耐久、装备效果或运行时数值；若后续系统需要这些值，必须由所属系统通过既有配置/裁决任务补充。

## 7. 后续注意事项

- D15-S02 应按本批 `production_batch` 检查重复模型、材质、碰撞、LOD、本地化和抽样交互；placeholder 不能被标记为 Q3/verified。
- Item/Appearance/Inventory/Building/Collection owner 接入时使用稳定 ID，不要复制目录到 UI、Agent 或第二套 Catalog。
- 新增真实模型/材质前必须新增 `asset_` 授权记录，并保持 `asset_refs` 可追溯；禁止把外部下载资源直接写进 item/outfit JSON。
- 武器、宗教/身份服饰、悼念与社群收藏等历史/敏感条目必须保留警告/跳过信息；不要在批量 QA 中为了数量删除审查字段。
- 允许 D15-S02 开始批量 QA 和不改变 owner 的异步资源替换；禁止在 A05 后续修订中自行实现 Item/Appearance/Combat/Economy 逻辑。
