# D8-A04 P2A Action 注册表

- 任务：`D8-A04`
- 契约版本：`agent_reply.v1.1`；动作注册版本：`p2a.v1`
- 机器可读单一事实源：[`contracts/p2a-action-registry.json`](../../../../contracts/p2a-action-registry.json)
- AgentReply 的 `action_intent.name` 允许 `none`；`none` 不是 60 个可执行 P2A Action 之一。
- 本表的参数是可读摘要；类型、枚举、正则、必填项以 contracts registry 为准。所有动作都必须同时通过 Schema、AgentCore 注册表和 server gateway 边界校验。

## 注册字段约定

| 字段 | 含义 |
|---|---|
| 注册名 | `action_intent.name` 的稳定值；未知值拒绝 |
| 参数 Schema | 结构化 `parameters` 的必填键、类型/枚举/引用格式/范围；不是最终数值结果 |
| 权限 | 所属系统授予的提议/执行权限；Agent 不拥有写权限 |
| 前置 | 所属系统快照必须证明的条件；未满足返回 `missing_prerequisite` |
| 失败码 | 执行者可以返回的拒绝分类；失败不生成隐式结果 |
| 执行者 | 最终动作请求的强逻辑系统/控制器；AgentCore 和 server 不执行世界结果 |

## 60 个 Action

| # | 注册名 | 参数 Schema | 权限 | 前置 | 失败码 | 执行者 |
|---:|---|---|---|---|---|---|
| 1 | `trim_sail` | `sail∈main,fore,jib,all`; `setting∈furled,reefed,half,full` | `ship.sail` | `ship.present`; `crew.on_duty` | `ship_unavailable`; `crew_unavailable`; `invalid_sail_setting` | `ShipSystem.SailController` |
| 2 | `steer` | `direction∈port,starboard,center`; `intensity∈微调,常规,满舵` | `ship.steer` | `ship.present`; `helmsman.assigned` | `ship_unavailable`; `helmsman_unavailable`; `steering_locked` | `ShipSystem.HelmController` |
| 3 | `set_course` | `destination_ref`：`island|port|landmark:<id>` | `ship.course` | `chart.available`; `ship.present` | `unknown_destination`; `chart_unavailable`; `route_blocked` | `ShipSystem.CoursePlanner` |
| 4 | `drop_anchor` | 无参数 | `ship.anchor` | `ship.present`; `water_depth_known` | `ship_unavailable`; `unsafe_depth`; `anchor_damaged` | `ShipSystem.AnchorController` |
| 5 | `weigh_anchor` | 无参数 | `ship.anchor` | `ship.present`; `anchor_deployed` | `ship_unavailable`; `anchor_not_deployed`; `anchor_stuck` | `ShipSystem.AnchorController` |
| 6 | `stop_ship` | 无参数 | `ship.stop` | `ship.present` | `ship_unavailable`; `wind_too_strong`; `stop_refused` | `ShipSystem.ManeuverController` |
| 7 | `set_watch` | `duty∈helm,lookout,gunner,repair`; `crew_ref=crew:<id>` | `crew.assign_duty` | `ship.present`; `crew.onboard` | `crew_not_found`; `duty_unavailable`; `duplicate_watch` | `ShipSystem.WatchScheduler` |
| 8 | `report` | `topic∈island,vessel,hazard,weather_signs,soundings` | `ship.observe` | `crew.on_duty` | `observer_unavailable`; `view_obstructed`; `report_unreliable` | `Environment.ObservationSystem` |
| 9 | `heave_to` | 无参数 | `ship.maneuver` | `ship.present`; `sails_set` | `ship_unavailable`; `sail_failure`; `maneuver_refused` | `ShipSystem.ManeuverController` |
| 10 | `change_speed` | `setting∈slow,cruise,fast` | `ship.sail` | `ship.present` | `ship_unavailable`; `sail_failure`; `speed_limit` | `ShipSystem.SailController` |
| 11 | `fire_cannon` | `cannon_group∈port,starboard,bow,stern`; `ammunition∈roundshot,chainshot,grapeshot` | `combat.fire` | `combat.engaged`; `cannon_loaded` | `no_target`; `cannon_unavailable`; `ammunition_unavailable`; `unsafe_fire_order` | `CombatSystem.CannonController` |
| 12 | `repair` | `section∈hull,sails,mast,rudder,pump,cannon`; `urgency∈inspect,patch,emergency` | `ship.repair` | `ship.present`; `repair_tools` | `ship_unavailable`; `tools_unavailable`; `section_unreachable` | `ShipSystem.RepairController` |
| 13 | `bilge_pump` | `mode∈manual,crew_shift,emergency` | `ship.pump` | `ship.present`; `pump_operable` | `pump_unavailable`; `crew_unavailable`; `water_level_critical` | `ShipSystem.BilgeController` |
| 14 | `ration` | `plan∈lean,standard,generous` | `crew.ration` | `crew.onboard`; `provisions.available` | `provisions_unavailable`; `crew_conflict`; `plan_not_allowed` | `CrewSystem.RationController` |
| 15 | `hold_below` | `crew_ref=crew:<id>`; `duration_hours` 整数 `1..72` | `legal.detain` | `ship.secure`; `legal.authority` | `crew_not_found`; `authority_missing`; `detention_unsafe` | `FactionLegalSystem.DetentionController` |
| 16 | `reward_crew` | `crew_ref=crew:<id>`; `recognition∈thanks,leave,public_praise,duty_preference` | `crew.recognize` | `crew.onboard` | `crew_not_found`; `recognition_conflict`; `crew_unavailable` | `CrewSystem.RecognitionController` |
| 17 | `launch_boat` | `boat_purpose∈shore_party,salvage,scouting,rescue` | `ship.launch_boat` | `boat_available`; `sea_state_tolerable` | `boat_unavailable`; `sea_state_unsafe`; `crew_unavailable` | `ShipSystem.BoatController` |
| 18 | `board` | `stance∈hail,grapple,withdraw` | `combat.board` | `combat.engaged`; `boarding_distance` | `no_target`; `distance_too_far`; `boarding_refused`; `crew_unready` | `CombatSystem.BoardingController` |
| 19 | `extinguish_fire` | `section∈galley,hold,deck,cabin,mast` | `ship.fire_response` | `fire_detected`; `water_or_blanket` | `fire_not_found`; `equipment_unavailable`; `crew_unavailable` | `ShipSystem.FireController` |
| 20 | `secure_cargo` | `hold_ref=hold|cargo_hold:<id>` | `cargo.secure` | `ship.present`; `cargo_loaded` | `hold_not_found`; `cargo_unavailable`; `crew_unavailable` | `InventoryCargoSystem.SecuringController` |
| 21 | `assemble_crew` | `team_ref=team:<id>`; `equipment_plan∈light,survey,medical,defensive` | `crew.assemble` | `crew.onboard`; `shore_access` | `team_not_found`; `shore_access_missing`; `equipment_unavailable` | `CrewSystem.LandingPartyController` |
| 22 | `gather` | `resource_type∈water,food,medicine,timber` | `ecology.gather` | `shore_access`; `valid_resource_zone` | `shore_access_missing`; `resource_unavailable`; `legal_restriction` | `EcologySystem.GatherController` |
| 23 | `scout` | `area_ref=island|port|sector:<id>` | `exploration.scout` | `shore_access`; `scout_available` | `area_unknown`; `scout_unavailable`; `legal_restriction` | `QuestExplorationSystem.ScoutController` |
| 24 | `treat` | `crew_ref=crew:<id>`; `treatment∈first_aid,rest,herbal,surgery_request` | `crew.medicine` | `medical_kit`; `crew_onboard` | `crew_not_found`; `medical_unavailable`; `treatment_unsafe` | `CrewSystem.MedicalController` |
| 25 | `trade` | `counterparty_ref=npc|faction|port:<id>`; `commodity_ref=item:<id>`; `intent∈inquire,offer,request_exchange` | `economy.negotiate` | `counterparty_present`; `market_open` | `counterparty_unavailable`; `market_closed`; `legal_restriction` | `EconomySystem.TradeController` |
| 26 | `cook_meal` | `meal_plan∈stew,bread,fish,preserve` | `crew.cook` | `galley_operable`; `ingredients_available` | `galley_unavailable`; `ingredients_unavailable`; `cook_unavailable` | `CrewSystem.GalleyController` |
| 27 | `distribute_rations` | `recipient_scope∈all_crew,watch_team,landing_party,medical_case` | `crew.ration` | `provisions.available`; `crew.onboard` | `provisions_unavailable`; `recipient_unavailable`; `distribution_conflict` | `CrewSystem.RationController` |
| 28 | `assign_duty` | `crew_ref=crew:<id>`; `duty∈helm,lookout,gunner,repair,cook,medic,deckhand` | `crew.assign_duty` | `crew.onboard`; `duty_roster_open` | `crew_not_found`; `duty_unavailable`; `roster_locked` | `CrewSystem.DutyRosterController` |
| 29 | `call_meeting` | `topic∈route,ration,discipline,danger,shore_leave` | `crew.meeting` | `crew.onboard`; `safe_meeting_place` | `crew_unavailable`; `meeting_unsafe`; `topic_restricted` | `CrewSystem.MeetingController` |
| 30 | `request_leave` | `crew_ref=crew:<id>`; `reason∈family,health,faith,grief,personal` | `crew.leave` | `port_access`; `crew.onboard` | `crew_not_found`; `port_access_missing`; `leave_conflict` | `CrewSystem.LeaveController` |
| 31 | `greet_contact` | `protocol∈formal,shipboard,community,religious,cautious` | `social.speak` | `contact_present` | `contact_unavailable`; `protocol_unknown`; `legal_restriction` | `FactionLegalSystem.DiplomacyController` |
| 32 | `ask_information` | `topic∈route,market,law,rumor,shelter,danger` | `social.inquire` | `contact_present` | `contact_unavailable`; `topic_restricted`; `information_refused` | `QuestExplorationSystem.InformationController` |
| 33 | `propose_trade` | `trade_intent∈inquire,barter,consignment,exchange_service` | `economy.negotiate` | `contact_present`; `market_open` | `contact_unavailable`; `market_closed`; `terms_rejected` | `EconomySystem.NegotiationController` |
| 34 | `propose_alliance` | `scope∈mutual_aid,safe_passage,information,escort` | `faction.diplomacy` | `contact_present`; `faction_access` | `contact_unavailable`; `faction_access_missing`; `terms_rejected` | `FactionLegalSystem.DiplomacyController` |
| 35 | `request_passage` | `route_ref=route|port|sector:<id>`; `protocol∈petition,escort,safe_conduct` | `faction.passage` | `route_known`; `contact_present` | `route_unknown`; `contact_unavailable`; `permit_required` | `FactionLegalSystem.PassageController` |
| 36 | `request_mediation` | `dispute_ref=dispute:<id>`; `urgency∈routine,urgent,emergency` | `faction.mediation` | `mediator_present`; `dispute_known` | `mediator_unavailable`; `dispute_unknown`; `legal_restriction` | `FactionLegalSystem.MediationController` |
| 37 | `offer_apology` | `recipient_ref=crew|npc|faction:<id>`; `form∈private,public,restitution_request,ritual` | `social.repair` | `recipient_present` | `recipient_unavailable`; `apology_rejected`; `legal_restriction` | `CrewSystem.RelationController` |
| 38 | `negotiate_terms` | `term_category∈access,service,protection,labor,exchange`; `posture∈ask,firm,concede,pause` | `social.negotiate` | `contact_present`; `terms_open` | `contact_unavailable`; `terms_closed`; `authority_missing` | `FactionLegalSystem.TermsController` |
| 39 | `accept_offer` | `offer_ref=offer:<id>` | `social.negotiate` | `offer_open`; `terms_reviewed` | `offer_unknown`; `offer_expired`; `authority_missing` | `FactionLegalSystem.TermsController` |
| 40 | `end_talk` | `reason∈agreement,pause,insult,danger,no_progress` | `social.speak` | `conversation_open` | `conversation_not_open`; `contact_unavailable`; `unsafe_exit` | `FactionLegalSystem.DiplomacyController` |
| 41 | `survey_coast` | `sector_ref=sector:<id>`; `survey_method∈shoreline,soundings,wildlife,shelter` | `exploration.survey` | `survey_tools`; `safe_position` | `sector_unknown`; `tools_unavailable`; `sea_state_unsafe` | `Environment.ObservationSystem` |
| 42 | `inspect_wreck` | `wreck_ref=wreck:<id>`; `purpose∈identify,rescue,salvage,evidence` | `exploration.wreck` | `wreck_known`; `safe_access` | `wreck_unknown`; `access_unsafe`; `legal_restriction` | `QuestExplorationSystem.WreckController` |
| 43 | `map_landmark` | `landmark_ref=landmark:<id>` | `exploration.map` | `map_available`; `landmark_visible` | `landmark_unknown`; `map_unavailable`; `visibility_blocked` | `QuestExplorationSystem.MappingController` |
| 44 | `observe_animal` | `observation_method∈lookout,quiet_approach,track,record` | `ecology.observe` | `animal_present`; `observation_tools` | `animal_unavailable`; `approach_unsafe`; `observation_blocked` | `EcologySystem.ObservationController` |
| 45 | `rescue_animal` | `animal_ref=animal:<id>`; `care_plan∈stabilize,transport,release_site,contact_caregiver` | `ecology.rescue` | `animal_in_distress`; `care_tools` | `animal_unknown`; `care_tools_unavailable`; `legal_restriction` | `AnimalCareSystem.RescueController` |
| 46 | `release_animal` | `animal_ref=animal:<id>`; `release_plan∈home_habitat,protected_area,caregiver,defer` | `ecology.release` | `animal_in_care`; `release_site_known` | `animal_unknown`; `release_site_missing`; `legal_restriction` | `AnimalCareSystem.ReleaseController` |
| 47 | `fish` | `gear∈line,net,trap,handline`; `zone_ref=sector:<id>` | `ecology.fish` | `fishing_gear`; `fish_zone_known` | `gear_unavailable`; `zone_unknown`; `legal_restriction` | `EcologySystem.FishingController` |
| 48 | `collect_water` | `source_ref=water_source|island|port:<id>`; `method∈fill_cask,filter,boil` | `ecology.collect` | `source_known`; `containers_available` | `source_unknown`; `containers_unavailable`; `water_unsafe` | `EcologySystem.GatherController` |
| 49 | `collect_firewood` | `area_ref=island|sector:<id>`; `method∈fallen_only,managed_cut,salvage` | `ecology.collect` | `shore_access`; `tool_axe_or_saw` | `area_unknown`; `tools_unavailable`; `legal_restriction` | `EcologySystem.GatherController` |
| 50 | `enter_cave` | `cave_ref=cave:<id>`; `formation∈survey,passage,shelter,rescue` | `exploration.cave` | `cave_known`; `lamp_or_torch`; `safe_access` | `cave_unknown`; `light_unavailable`; `access_unsafe` | `QuestExplorationSystem.CaveController` |
| 51 | `buy_supplies` | `vendor_ref=npc|faction|port:<id>`; `supply_category∈food,water,medicine,ammunition,repair_material` | `economy.purchase` | `vendor_present`; `market_open` | `vendor_unavailable`; `market_closed`; `credit_rejected`; `supply_unavailable` | `EconomySystem.PurchaseController` |
| 52 | `sell_cargo` | `counterparty_ref=npc|faction|port:<id>`; `cargo_ref=cargo:<id>` | `economy.sell` | `vendor_present`; `cargo_loaded`; `market_open` | `counterparty_unavailable`; `cargo_unknown`; `market_closed`; `legal_restriction` | `EconomySystem.SaleController` |
| 53 | `visit_shipyard` | `service∈inspect,repair_request,refit_request,dock_request` | `shipyard.request` | `shipyard_open`; `ship_docked` | `shipyard_unavailable`; `ship_not_docked`; `service_unavailable` | `BuildingShipyardSystem.ShipyardController` |
| 54 | `request_upgrade` | `upgrade_ref=upgrade:<id>`; `priority∈inspect,planned,urgent` | `shipyard.upgrade` | `shipyard_open`; `ship_docked`; `upgrade_known` | `shipyard_unavailable`; `ship_not_docked`; `upgrade_unknown`; `materials_unavailable` | `BuildingShipyardSystem.UpgradeController` |
| 55 | `hire_crew` | `role∈sailor,navigator,gunner,carpenter,medic,cook,trader`; `contract_intent∈inquire,trial,offer_position` | `crew.hire` | `port_access`; `candidate_present`; `berth_available` | `candidate_unavailable`; `berth_unavailable`; `legal_restriction`; `terms_rejected` | `CrewSystem.RecruitmentController` |
| 56 | `dismiss_crew` | `crew_ref=crew:<id>`; `notice∈warning,release_at_port,mutual_agreement,emergency` | `crew.dismiss` | `crew.onboard`; `port_access` | `crew_not_found`; `port_access_missing`; `dismissal_blocked` | `CrewSystem.RecruitmentController` |
| 57 | `store_cargo` | `hold_ref=hold|warehouse:<id>`; `cargo_ref=cargo:<id>` | `cargo.store` | `storage_access`; `cargo_loaded` | `storage_unavailable`; `cargo_unknown`; `capacity_unavailable` | `InventoryCargoSystem.StorageController` |
| 58 | `retrieve_cargo` | `hold_ref=hold|warehouse:<id>`; `cargo_ref=cargo:<id>` | `cargo.retrieve` | `storage_access`; `cargo_stored` | `storage_unavailable`; `cargo_unknown`; `capacity_unavailable` | `InventoryCargoSystem.StorageController` |
| 59 | `submit_quest` | `quest_ref=quest:<id>` | `quest.submit` | `quest_open`; `objective_ready` | `quest_unknown`; `objective_incomplete`; `submission_closed` | `QuestSystem.SubmissionController` |
| 60 | `tend_garden` | `plot_ref=land:<id>`; `task∈inspect,water,weed,plant,harvest_request` | `farming.tend` | `plot_access`; `tool_available` | `plot_unknown`; `plot_access_missing`; `tool_unavailable`; `season_closed` | `FarmingSystem.GardenController` |

## 边界与拒绝规则

1. 顶层未知字段、缺字段、非法枚举、超范围 `affinity_change`、过长对白和错误对象形状拒绝。
2. `action_intent.name` 不在 60 个注册名或 `none` 中拒绝；参数键、类型、枚举、引用格式、整数范围和必填项由 registry 校验。
3. `actor_ref` 必须来自当前 Agent 的授权对象集合；动作权限不足返回 `permission_denied`；执行者不是 Agent。
4. `target_ref` 必须匹配动作允许的对象类型并属于授权目标集合；否则返回 `invalid_target_kind` 或 `unauthorized_target`。
5. 所有注册前置条件必须在当前快照中满足；否则返回 `missing_prerequisite`。失败码只描述拒绝，不代表已经结算。
6. 关系提议只能表达 `kind/stance/basis` 和有界的 `affinity_change` 意图，不能包含 relation 数值结果；Crew/RelationSystem 是唯一裁决者。
7. AgentReply 和动作参数禁止最终 `money`、`price`、`damage`、`reward`、`weather` 或关系结果字段；经济、战斗、环境、奖励和关系系统分别裁决结果。
8. Action 的执行链固定为：`LLM → agent-reply.schema.json → AgentCore/server gateway 边界 → 所属 executor → 事实事件/结果反馈`。
