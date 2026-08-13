# D15-A07* 角色/宠物批次交接

日期：2026-08-11  
任务：D15-A07*  
前置：D15-S01 通过，crew_isaac_reed、pet_cabin_cat、animal_domestic_chicken 代表样例已存在。

## 交付

- 批次清单：game/Assets/Resources/ContentRegistry/batches/d15_a07/d15_a07_character_pet_batch.json
- 角色目录：game/Assets/Resources/ContentRegistry/entries/crew/crew_d15_a07_preset_001.json—crew_d15_a07_preset_060.json
- 新宠物/家禽目录：pet_cat_mooring、pet_cat_galley、pet_dog_watch、pet_dog_village、animal_domestic_duck、animal_domestic_guinea_fowl
- 目录/性能说明：docs/game/design/common/D15_CHARACTER_PET_BATCH_CATALOG.md
- 批次检查器：tools/d15_a07_batch_check.py
- 测试：tools/tests/test_d15_a07_batch.py
- Unity 抽样测试：game/Assets/Tests/EditMode/D15A07ContentBatchTests.cs

## 数量与字段

- 60 个船员预设，ID 范围 crew_d15_a07_preset_001—060；
- 500 个 NPC 实例，ID 范围 npc_d15_a07_instance_0001—0500；
- 猫、狗、家禽、牲畜各 3 个，共 12 个命名变体；
- 24 个文化/职业/服装/发型/职业装备组合；
- 发型维度覆盖 6 个已登记 outfit_ 内容 ID，并已同步写入 60 个船员目录引用；
- 所有角色/实例/变体均有 ID、文化/职业标签、来源、授权、外观组合、资源状态、行为入口、模型/材质/碰撞/LOD、历史审查、证据和测试字段；
- 资产只提供 Q2 占位内容；没有关系、工资、健康、奖励、战斗或其他运行时裁决字段。

## Owner 与加载边界

- 船员预设和 NPC：CrewSystem；
- 猫狗：AnimalCareSystem；
- 家禽和牲畜：EcologySystem；
- 外观组合：AppearanceSystem；
- 存档键与快照落盘：SaveSystem。

资产只引用 CrewSystem:LoadPreset、CrewSystem:LoadNpcInstance、AnimalCareSystem:LoadDefinition 和 EcologySystem:LoadDefinition。没有新增 Systems 逻辑，没有网络或 LLM 代码，没有第二套运行时状态。

## 授权/历史/性能

统一授权 ID：auth_d15_a07_internal_authored_20260811，项目自有、商业使用允许、可修改，证据 D15-A07-AUTH-001。占位模型沿用 PH_ASSET_SOURCES.md 的仓库资源。批次 asset_state=placeholder、quality_level=Q2，LOD0/1/2/3 上限为 30,000 / 12,000 / 4,000 / 1,500 三角面；NPC 可见代理上限为 32。

抽样门禁已登记：船员 6、NPC 20、四家族宠物/家畜 4。所有实例有独立 instance_id、位置、锚点、composition_key 和 save_key，不是简单复制 prefab。

## 测试结果

    python tools/d15_a07_batch_check.py game/Assets/Resources/ContentRegistry/batches/d15_a07/d15_a07_character_pet_batch.json
    => D15-A07 batch OK: 60 crew presets, 500 NPC instances, 12 named variants

    python -m unittest tools.tests.test_d15_a07_batch -v
    => 3/3 PASS

Unity EditMode 抽样测试已登记，覆盖 60 个 crew_d15_a07_ 条目、代表角色/宠物的 ContentRegistryReader 加载、来源、授权和 owner 字段。

D8 全库注册表检查通过：2,812 entries、13 prefixes。A07 新增条目、发型引用和代表内容均无孤儿或前缀错误。

## 后续

1. D15-S02 执行 100% 目录/授权/历史检查和 6/20/4 分层抽样；
2. 美术资源 owner 将 Q2 placeholder 替换为 Q3 角色/宠物资源，保持 ID、行为入口和保存键不变；
3. D15-S02 继续执行全库内容抽样、授权和资源状态复核。
