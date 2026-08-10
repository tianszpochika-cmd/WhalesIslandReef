# 船舶数据模型与存档

## 1. 模型分层

船舶数据分为：

1. ShipTypeDef：不可变船型定义；
2. HullTemplate：模型、碰撞、甲板和舱室模板；
3. ShipInstance：玩家或 NPC 的具体船；
4. ShipModuleDef：模块静态定义；
5. ShipModuleInstance：已安装模块；
6. ShipCompartmentState：舱室状态；
7. ShipFunctionalPointState：功能点状态；
8. ShipVoyageState：航行和锚泊；
9. ShipDamageState：损伤和灾害；
10. ShipHistory：建造、改装、战斗和维修历史。

## 2. ShipTypeDef

必须包含：

- id、name、display_name、historical_context；
- role_tags、model_ref、collision_ref；
- length、beam、base_draft、freeboard、deck_count；
- base_speed、base_turn、base_acceleration；
- windward_rating、seaworthiness、stealth_rating；
- cargo_volume_max、cargo_weight_max；
- crew_min、crew_standard、crew_max；
- cannon_slots_max、hull_max、flooding_capacity；
- tilt_sensitivity、maintenance_rating、purchase_price；
- base_materials、required_port_level、required_reputation；
- functional_point_slots；
- compatible_module_tags、incompatible_module_tags；
- available_boat_types；
- source_notes。

## 3. ShipInstance

必须记录：

- id、ship_type_id、name、owner_id、flag_id；
- home_port_id、build_port_id、build_year；
- current_hull、current_hull_max、current_draft；
- current_speed_modifier、current_turn_modifier；
- current_cargo_volume、current_cargo_weight；
- crew_ids、crew_assignments；
- installed_modules；
- functional_point_states；
- compartment_states；
- voyage_state；
- damage_state；
- anchor_state、mooring_state；
- cargo_state、supply_state；
- ship_history、ship_log_id。

## 4. ShipModuleDef

字段：

- module_id、category、display_name；
- compatible_ship_tags；
- required_functional_point；
- occupied_space、weight、draft_change；
- crew_requirement；
- stat_modifiers、risk_modifiers；
- level_max；
- cost_by_level、material_by_level、time_by_level；
- required_port_level、required_reputation；
- visual_variant；
- removable；
- mutually_exclusive_modules；
- source_notes。

## 5. ShipModuleInstance

字段：

- module_id；
- level；
- condition；
- installed_day；
- last_repair_day；
- current_wear；
- owner_ship_id；
- damaged；
- locked_by_quest；
- custom_name。

## 6. ShipCompartmentState

字段：

- compartment_id；
- capacity；
- occupied_volume；
- occupied_weight；
- water_level；
- fire_level；
- smoke_level；
- structural_damage；
- flooded；
- burning；
- sealed；
- accessible；
- crew_present；
- cargo_ids。

## 7. ShipFunctionalPointState

字段：

- point_id；
- point_type；
- enabled；
- condition；
- assigned_crew_id；
- blocked_reason；
- required_materials；
- current_task；
- interaction_anchor。

## 8. 航行和损伤状态

航行状态至少包含：

- 当前航向、速度、帆面、舵角；
- 风向、风速、海流、海深；
- 锚状态、漂移速度和锚泊半径；
- 泊位、缆绳和跳板状态。

损伤状态至少包含：

- 结构耐久；
- 分区进水；
- 倾斜角；
- 舵损；
- 桅杆、索具、帆具；
- 火灾、烟雾和火药风险；
- 货物受潮、滑动和落海。

## 9. 当前存档兼容

D1—D7 旧字段必须保留：

- shipTypeId；
- name；
- hull、hullMax；
- cargo；
- cannons、loadedCount；
- supplies；
- crew；
- upgrades；
- state；
- shipLog。

D4 的 shipyard_state 当前只表达当前船型、已解锁船型和六个改装槽等级。

新字段接入规则：

1. 先更新本目录设计；
2. 再更新 DATA_MODELS；
3. 再设计版本化 contracts；
4. 再同步 game、server、web；
5. 旧档读取时填充默认值；
6. SaveSystem 只序列化和恢复，不计算改装数值。

本次目录重构不修改 contracts 和代码。

## 10. 测试数据

最少准备以下 fixtures：

- 小渔船默认状态；
- 单桅纵帆船完整状态；
- 装有货物和三个模块的贸易船；
- 进水并倾斜的损伤船；
- 抛锚和走锚状态；
- 船坞施工中的船；
- 旧 D1—D7 存档；
- 具有未知新字段的旧版本存档。
