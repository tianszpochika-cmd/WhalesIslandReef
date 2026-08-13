# D15-A03* 动物批次目录

任务：`D15-A03*`  
日期：2026-08-11  
状态：**批次内容生产完成；已通过 300 条目数据门禁，待 D15-S02 做全批次跨场景抽样**  
前置：`D15-S01`、代表动物样例已完成

本文件是 A03 的内容目录与接管说明。实际不可变定义位于
`game/Assets/Resources/ContentRegistry/entries/animal/` 和
`game/Assets/Resources/ContentRegistry/entries/fish/`；运行时数量、刷新结果、
行为状态、法律结果、生态压力和保存状态仍由 `EcologySystem` 写入。

## 1. 批次统计

| 口径 | 数量 |
|---|---:|
| D15-A03 批次条目 | 300 |
| `animal_` 条目 | 166 |
| `fish_` 条目 | 134 |
| 行为等级覆盖 | L0 / L1 / L2 / L3 / L4 |
| 视觉质量 | Q2；共享占位模型/材质，明确记录 `asset_state=placeholder` |
| 生产阶段 | `integrated` |
| owner | `EcologySystem` |

| 子类 | 数量 | 主前缀 |
|---|---:|---|
| `terrestrial` | 40 | `animal_` |
| `reptiles` | 25 | `animal_` |
| `coastal_birds` | 30 | `animal_` |
| `birds` | 25 | `animal_` |
| `insects` | 20 | `animal_` |
| `freshwater_estuary` | 25 | `animal_` |
| `sea_turtles` | 15 | `animal_` |
| `crustaceans_softbody` | 35 | `fish_` |
| `nearshore_fish` | 50 | `fish_` |
| `whales_dolphins` | 15 | `fish_` |
| `sharks_rays` | 20 | `fish_` |

## 2. 目录位置与 ID 规则

- 每条记录使用稳定小写 ASCII snake_case，文件名与 `id` 完全一致。
- 物种/陆生/鸟类/爬行/昆虫/淡水河口/海岸鸟使用 `animal_`；鱼类、鲨鳐、海龟、鲸豚、甲壳类/软体使用 `fish_`，均登记为 `EcologySystem` owner。
- 300 条目已加入 D8 `registry.json` 的可达 roots，没有创建第二套内容主键。
- 本批次没有新增外部二进制资源；模型、材质和碰撞均显式登记为可替换占位引用。

## 3. 每条记录的冻结字段

| 字段 | A03 口径 |
|---|---|
| `d15_category` | 上表 11 个生产子类之一 |
| `behavior_level` / `behavior_profile` | L0–L4；只引用 `ecology.d15.*` 逻辑接管配置，不存 LLM prompt 或公式 |
| `habitat_profile` | 主生境、次生境、水深范围（米）、生态分区；与 `valid_habitats` 一致 |
| `refresh_profile` | 刷新模式、确定性间隔（日）、seed domain、时间窗口；不写运行时种群量 |
| `food_chain_profile` | 生态角色、食物标签、捕食者标签；不写奖励或经济价格 |
| `danger_profile` | danger level、危险标签和可预警 telegraph；最终危险结果由 EcologySystem 裁决 |
| `legal_profile` | `legal/regulated/protected/licensed`、规则 ID、许可要求、来源；不直接改变法律状态 |
| `performance_budget` | LOD0/1/2/3 三角面上限、活动距离（米）、更新频率（Hz）、最大活动对象数 |
| 来源/授权 | D8 `source/source_refs/source_note` + A03 `authorization`，商业使用和修改权限均为 true |
| 审查/跳过 | `historical_review`、`review_evidence_refs`、`content_warning_ids`、`skip_allowed`、表现级别 |
| 加载/接管 | `test_cases` 固定覆盖 LOAD、ECOLOGY-TAKEOVER、SAVE-RESTORE |

所有条目统一使用 D15 动物 LOD2 档位：LOD0 ≤ 12,000、LOD1 ≤ 5,000、LOD2 ≤ 1,500、
LOD3 ≤ 500；实际值按动物规模下调。大型鲸豚使用冻结上限，昆虫使用更低预算和更高
更新频率。

## 4. 代表样例

| ID | 子类 | 行为 | 法律 | 审查 | owner |
|---|---|---|---|---|---|
| `animal_green_turtle` | sea_turtles | L3 | protected | ApprovedWithWarning，可跳过 | EcologySystem |
| `animal_domestic_chicken` | terrestrial | L1 | regulated | Approved | EcologySystem |
| `fish_mackerel_school` | nearshore_fish | L2 | legal | Approved | EcologySystem |

三个样例已补齐 A03 扩展字段，且与现有 D11 动物/鱼群状态机入口保持一致；样例加载
只验证内容定义和 owner，不在内容读取器中写入动物、鱼群或库存状态。

## 5. 逻辑与资产边界

- `EcologySystem` 唯一写鱼群/动物的数量、位置、刷新结果、行为状态、生态压力和法律采集结果。
- 内容目录只提供生境、行为等级、刷新配置引用、食物链标签、危险预警标签、法律规则引用和表现预算。
- UI/Agent 只能读取目录、快照和事件；不得由目录或 UI 直接决定捕捉、伤害、奖励、价格、库存或保护区结果。
- 动物行为不使用 LLM；Systems 没有新增网络、HTTP 或外部生成服务。
- 受保护内容统一使用 wildlife protection warning；玩家可选择低强度表现或跳过，跳过只记录事实，不伪造奖励。

## 6. 测试入口

~~~powershell
python tools/d15_a03_animal_batch_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools/tests/test_d15_a03_animal_batch_check.py
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
~~~

Unity EditMode 入口：`D15.Tests.D15AnimalBatchTests`，覆盖批次 300 条目/唯一 ID、
代表样例字段、Ecology owner、加载和确定性 D4 行为入口。

## 7. 条目索引

### terrestrial（40）

- `animal_agouti_forest`
- `animal_agouti_lowland`
- `animal_anteater_northern`
- `animal_anteater_southern`
- `animal_armadillo_giant`
- `animal_armadillo_nine_banded`
- `animal_brocket_deer_forest`
- `animal_brocket_deer_lowland`
- `animal_cane_rat`
- `animal_capuchin_monkey_brown`
- `animal_capuchin_monkey_white`
- `animal_coati_ring_tailed`
- `animal_coati_white_nosed`
- `animal_domestic_chicken`
- `animal_donkey_island`
- `animal_feral_cat_harbor`
- `animal_feral_dog_village`
- `animal_fruit_bat_greater`
- `animal_fruit_bat_lesser`
- `animal_goat_hill`
- `animal_howler_monkey_black`
- `animal_howler_monkey_red`
- `animal_hutia_forest`
- `animal_hutia_lowland`
- `animal_leaf_nosed_bat`
- `animal_mongoose_small`
- `animal_opossum_common`
- `animal_opossum_water`
- `animal_paca_forest`
- `animal_paca_lowland`
- `animal_peccary_forest`
- `animal_peccary_lowland`
- `animal_pig_feral`
- `animal_raccoon_crab_eating`
- `animal_raccoon_mountain`
- `animal_sloth_three_toed`
- `animal_sloth_two_toed`
- `animal_spider_monkey_black`
- `animal_spider_monkey_brown`
- `animal_tree_rat`

### reptiles（25）

- `animal_alligator_lizard`
- `animal_anole_brown`
- `animal_anole_green`
- `animal_basilisk_brown`
- `animal_basilisk_green`
- `animal_blind_snake`
- `animal_boa_constrictor`
- `animal_box_turtle_caribbean`
- `animal_caiman_spectacled`
- `animal_crocodile_american`
- `animal_curly_tailed_lizard`
- `animal_gecko_giant`
- `animal_gecko_house`
- `animal_ground_snake`
- `animal_iguana_green`
- `animal_iguana_lesser`
- `animal_iguana_rhinoceros`
- `animal_monitor_lizard`
- `animal_pond_turtle_spotted`
- `animal_skink_blue_tailed`
- `animal_skink_ground`
- `animal_tortoise_giant`
- `animal_tortoise_woodland`
- `animal_vine_snake`
- `animal_water_snake`

### coastal_birds（30）

- `animal_anhinga`
- `animal_avocet`
- `animal_black_crowned_night_heron`
- `animal_brown_booby`
- `animal_brown_pelican`
- `animal_cormorant`
- `animal_curlew`
- `animal_frigatebird`
- `animal_gannet`
- `animal_great_blue_heron`
- `animal_great_egret`
- `animal_green_heron`
- `animal_gull`
- `animal_least_tern`
- `animal_little_blue_heron`
- `animal_magnificent_frigatebird`
- `animal_noddy`
- `animal_osprey`
- `animal_plover`
- `animal_royal_tern`
- `animal_sanderling`
- `animal_sandpiper`
- `animal_sandwich_tern`
- `animal_skimmer`
- `animal_snowy_egret`
- `animal_tern`
- `animal_tricolored_heron`
- `animal_turnstone`
- `animal_willet`
- `animal_yellow_crowned_night_heron`

### birds（25）

- `animal_bananaquit`
- `animal_caribbean_parrot`
- `animal_cuckoo`
- `animal_dove`
- `animal_falcon`
- `animal_finch`
- `animal_flycatcher`
- `animal_green_parrot`
- `animal_hawk`
- `animal_hummingbird`
- `animal_macaw`
- `animal_martin`
- `animal_nightjar`
- `animal_oriole`
- `animal_owl`
- `animal_pigeon`
- `animal_quail_dove`
- `animal_rail`
- `animal_scarlet_ibis`
- `animal_starling`
- `animal_swift`
- `animal_tanager`
- `animal_trogon`
- `animal_waxwing`
- `animal_woodpecker`

### insects（20）

- `animal_beetle`
- `animal_butterfly`
- `animal_cicada`
- `animal_cricket`
- `animal_damselfly`
- `animal_dragonfly`
- `animal_fire_ant`
- `animal_grasshopper`
- `animal_honeybee`
- `animal_ladybird`
- `animal_leafcutter_ant`
- `animal_mantis`
- `animal_mosquito`
- `animal_moth`
- `animal_rhinoceros_beetle`
- `animal_sandfly`
- `animal_stick_insect`
- `animal_stingless_bee`
- `animal_termite`
- `animal_wasp`

### freshwater_estuary（25）

- `animal_capybara_wetland`
- `animal_catfish_channel`
- `animal_crab_freshwater`
- `animal_eel_swamp`
- `animal_frog_bull`
- `animal_frog_tree`
- `animal_gar_longnose`
- `animal_goby_mangrove`
- `animal_killifish_marsh`
- `animal_manatee_estuary`
- `animal_mojarra_silver`
- `animal_mudskipper_spotted`
- `animal_mullet_mangrove`
- `animal_mullet_river`
- `animal_mussel_river`
- `animal_needlefish_estuary`
- `animal_otter_river`
- `animal_oyster_estuary`
- `animal_prawn_river`
- `animal_salamander_wetland`
- `animal_shrimp_freshwater`
- `animal_snapper_mangrove`
- `animal_snook_common`
- `animal_tarpon_estuary`
- `animal_turtle_river`

### sea_turtles（15）

- `animal_green_turtle`
- `fish_box_turtle`
- `fish_flatback_turtle`
- `fish_green_turtle_hatchling`
- `fish_hawksbill_hatchling`
- `fish_hawksbill_turtle`
- `fish_leatherback_hatchling`
- `fish_leatherback_turtle`
- `fish_loggerhead_nesting`
- `fish_loggerhead_turtle`
- `fish_mud_turtle`
- `fish_olive_ridley_turtle`
- `fish_river_turtle`
- `fish_turtle_feeding_juvenile`
- `fish_turtle_migration_adult`

### crustaceans_softbody（35）

- `fish_abalone_shallow`
- `fish_anemone_reef`
- `fish_barnacle_rock`
- `fish_blue_crab`
- `fish_clam_shallow`
- `fish_conch_queen`
- `fish_coral_polyp`
- `fish_cuttlefish`
- `fish_fiddler_crab`
- `fish_ghost_crab`
- `fish_hermit_crab`
- `fish_horseshoe_crab`
- `fish_jellyfish`
- `fish_krill_school`
- `fish_limpet_rock`
- `fish_lobster_caribbean`
- `fish_lobster_spiny`
- `fish_mangrove_crab`
- `fish_mussel_reef`
- `fish_nudibranch`
- `fish_octopus_reef`
- `fish_oyster_reef`
- `fish_plankton_bloom`
- `fish_prawn_mangrove`
- `fish_scallop_bay`
- `fish_sea_cucumber`
- `fish_sea_louse`
- `fish_sea_sponge`
- `fish_sea_urchin`
- `fish_shrimp_reef`
- `fish_snail_reef`
- `fish_squid_shallow`
- `fish_starfish`
- `fish_stone_crab`
- `fish_whelk_reef`

### nearshore_fish（50）

- `fish_amberjack_school`
- `fish_amberjack_shoal`
- `fish_anchovy_school`
- `fish_anchovy_shoal`
- `fish_barracuda_school`
- `fish_barracuda_shoal`
- `fish_bonito_school`
- `fish_bonito_shoal`
- `fish_damselfish_school`
- `fish_damselfish_shoal`
- `fish_filefish_school`
- `fish_filefish_shoal`
- `fish_flying_fish_school`
- `fish_flying_fish_shoal`
- `fish_goatfish_school`
- `fish_goatfish_shoal`
- `fish_grouper_school`
- `fish_grouper_shoal`
- `fish_grunt_school`
- `fish_grunt_shoal`
- `fish_herring_school`
- `fish_herring_shoal`
- `fish_jack_school`
- `fish_jack_shoal`
- `fish_kingfish_school`
- `fish_kingfish_shoal`
- `fish_lionfish_school`
- `fish_lionfish_shoal`
- `fish_mackerel_juvenile`
- `fish_mackerel_school`
- `fish_mullet_school`
- `fish_mullet_shoal`
- `fish_needlefish_school`
- `fish_needlefish_shoal`
- `fish_parrotfish_school`
- `fish_parrotfish_shoal`
- `fish_pompano_school`
- `fish_pompano_shoal`
- `fish_pufferfish_school`
- `fish_pufferfish_shoal`
- `fish_sardine_school`
- `fish_sardine_shoal`
- `fish_snapper_school`
- `fish_snapper_shoal`
- `fish_triggerfish_school`
- `fish_triggerfish_shoal`
- `fish_tuna_school`
- `fish_tuna_shoal`
- `fish_wrasse_school`
- `fish_wrasse_shoal`

### whales_dolphins（15）

- `fish_beaked_whale`
- `fish_bottlenose_dolphin`
- `fish_common_dolphin`
- `fish_fin_whale`
- `fish_humpback_whale`
- `fish_manatee_offshore`
- `fish_minke_whale`
- `fish_pilot_whale`
- `fish_porpoise`
- `fish_right_whale`
- `fish_rough_toothed_dolphin`
- `fish_sei_whale`
- `fish_sperm_whale`
- `fish_spinner_dolphin`
- `fish_spotted_dolphin`

### sharks_rays（20）

- `fish_blacktip_shark`
- `fish_bull_shark`
- `fish_cownose_ray`
- `fish_deep_ray`
- `fish_devil_ray`
- `fish_eagle_ray`
- `fish_electric_ray`
- `fish_guitarfish`
- `fish_hammerhead_shark`
- `fish_lemon_shark`
- `fish_manta_ray`
- `fish_nurse_shark`
- `fish_ray_nursery`
- `fish_reef_shark`
- `fish_sawfish`
- `fish_shark_nursery`
- `fish_skate_reef`
- `fish_stingray_spotted`
- `fish_tiger_shark`
- `fish_whitetip_shark`

