# D15-A07* 角色与宠物批次目录

状态：批次目标完成，等待 D15-S02 分层抽样与最终资源复核  
日期：2026-08-11  
前置：D15-S01 已通过；角色/宠物代表样例为 crew_isaac_reed、pet_cabin_cat、animal_domestic_chicken。

## 1. 产出与入口

批次唯一清单：

game/Assets/Resources/ContentRegistry/batches/d15_a07/d15_a07_character_pet_batch.json

目录级船员预设位于 game/Assets/Resources/ContentRegistry/entries/crew/，新增 60 个稳定 crew_ ID。四个新宠物和两个新家禽物种同时登记到 entries/pet/ 与 entries/animal/；其余变体复用 D8/D11 已登记的代表 ID。批次清单包含完整的 12 个变体记录和 500 个独立 NPC 实例记录，不依赖运行时生成第二套实体目录。

本批次实际数量：

| 内容 | 实际数量 | 冻结目标 | 质量 |
|---|---:|---:|---|
| 船员预设 | 60 | ≥60 | Q2，占位资源，production_stage=integrated |
| NPC 实例组合 | 500 | 500—1,000 | Q2，占位资源，独立 instance/save/spawn ID |
| 猫 | 3 | 3 | Q2 |
| 狗 | 3 | 3 | Q2 |
| 家禽 | 3 | 3 | Q2 |
| 牲畜 | 3 | 3 | Q2 |

Q2 记录明确标记 asset_state=placeholder，没有把占位模型宣称为最终表现；后续美术替换只更新资源引用，不改变内容 ID、行为 owner 或存档键。

## 2. 船员预设

船员 ID 连续范围：

crew_d15_a07_preset_001 — crew_d15_a07_preset_060

每条记录都有统一 D8 字段，以及 culture_id、career_id、source_ref、authorization_id、appearance_combo_id、behavior_profile、asset_state、model_ref、material_ref、collision_ref、lod_profile、test_cases、historical_review、review_evidence_refs、content_warning_ids 和 skip_allowed。

文化标签覆盖 island_saint_marys、green_cay、free_cove、santa_margarita_coast、port_royal_quay、havana_coast、martinique_coast、curacao_freeport、maroon_community、coastal_indigenous_community、merchant_diaspora 和 island_farmstead。职业标签覆盖甲板、渔业、领航、烹饪、木工、医护、瞭望、索具、造船、岸上贸易、引航、帆匠、桶匠、补网、口译、码头文书、农务、畜牧照料、海岸向导和信号守望。

24 个 appearance_combo_d15_a07_* 组合明确记录文化、职业、发型、上装、下装、外套、鞋、手套、饰品、职业装备、材质/碰撞和测试 ID。服装/发型使用已有 outfit_ 内容 ID，角色模型使用已有 asset_ph_crew_male 占位资源；没有新增私有外观协议。

发型维度实际覆盖 outfit_hair_short_brown、outfit_long_tied_hair、outfit_loose_wave_hair、outfit_low_ponytail_hair、outfit_short_curl_hair 和 outfit_side_braid_hair 六个已登记内容 ID。

行为接管入口固定为 CrewSystem:LoadPreset。资产记录不包含关系、工资、健康、奖励、战斗或其他运行时结果字段。

## 3. NPC 实例组合

NPC ID 为 npc_d15_a07_instance_0001 — npc_d15_a07_instance_0500。

每个实例均有独立 instance_id、save_key、spawn_anchor_id、test_id；preset_id、文化、职业和外观组合来源；location_ref 与数值位置数组；composition_key；CrewSystem:LoadNpcInstance 行为入口；共享占位模型、材质、胶囊碰撞和角色 LOD3 引用。

实例按 10 个港口、村落、市场、渔场、船坞和船上锚点分层分布；实例只声明静态内容组合和加载/保存入口，不写居民状态或任何强逻辑结果。可见对象预算由 npc_visible_proxy_cap=32 和 D9/D16 更严格的场景预算共同约束。

## 4. 猫、狗、家禽和牲畜

| 家族 | 内容 ID |
|---|---|
| 猫 | pet_cabin_cat、pet_cat_mooring、pet_cat_galley |
| 狗 | pet_harbor_dog、pet_dog_watch、pet_dog_village |
| 家禽 | animal_domestic_chicken、animal_domestic_duck、animal_domestic_guinea_fowl |
| 牲畜 | animal_domestic_cattle、animal_domestic_goat、animal_domestic_pig |

每条变体都有文化/来源标签、照料/生态 owner、来源和授权记录、pet_appearance_d15_a07_* 外观组合、占位模型/材质/碰撞/LOD、历史审查、测试字段和清晰的行为入口。猫狗入口为 AnimalCareSystem:LoadDefinition；家禽和牲畜入口为 EcologySystem:LoadDefinition。宠物不是战斗单位，家畜也不由内容目录直接生成状态、产物或结果。

## 5. 来源、授权与历史审查

本批次统一授权记录为 auth_d15_a07_internal_authored_20260811：

- 权利主体：Whalebone project；
- 类型：项目自有内容；
- 商业使用：允许；
- 修改：允许；
- 证据：D15-A07-AUTH-001；
- 记录日期：2026-08-11；
- 适用范围：游戏、测试和营销构建。

角色、外观和宠物内容分别引用 D10/D11 设计文档、D13 家畜目录以及本冻结文件。现实社群、殖民身份、宗教、疾病、伤残和动物来源不在本批次凭空扩写；生成预设使用 historical_tag=fictional、historical_review=Approved，敏感警告数组仍显式存在。占位模型沿用仓库已有授权清单 game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md，不新增网络素材依赖。

## 6. 性能与抽样

角色/宠物固定使用 D15-A07 LOD3 预算：

| 档位 | 三角面上限 |
|---|---:|
| LOD0 | 30,000 |
| LOD1 | 12,000 |
| LOD2 | 4,000 |
| LOD3 | 1,500 |

船员抽样 10% 且不少于 6 个；NPC 抽样 2% 且不少于 20 个；宠物/家畜抽样 25% 且最少覆盖猫、狗、家禽、牲畜各 1 个。批次清单记录的样本数分别为 6、20 和 4。

## 7. 测试与责任边界

已执行：

    python tools/d15_a07_batch_check.py game/Assets/Resources/ContentRegistry/batches/d15_a07/d15_a07_character_pet_batch.json
    python -m unittest tools.tests.test_d15_a07_batch -v

结果：批次检查通过（60 / 500 / 12）；Python 测试 3/3 通过；代表 ID JSON 加载、来源/商业授权、独立 NPC 存档键、锚点和位置字段均通过。Unity EditMode 抽样入口已登记在 game/Assets/Tests/EditMode/D15A07ContentBatchTests.cs，覆盖 60 个 crew_d15_a07_ 条目计数和代表角色/宠物资源加载。

Unity/逻辑 owner 接口不由资产目录复制：CrewSystem 写船员/NPC 运行时状态，AnimalCareSystem 写猫狗状态，EcologySystem 写家禽/牲畜生态状态，AppearanceSystem 读取外观组合，SaveSystem 负责快照落盘。UI、Agent 和资产文件没有数值写入口。

全库 D8 registry checker 已通过：2,812 entries、13 prefixes；A07 新增的 60 个 crew 条目、6 个新宠物/家禽条目和外观引用均已进入可达注册图。本任务没有修改建筑/船舶批次逻辑。
