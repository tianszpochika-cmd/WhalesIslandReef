using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Whalebone.Agent
{
    public enum P2AParameterKind
    {
        String,
        Integer,
        Boolean
    }

    public sealed class P2AParameterDefinition
    {
        public string Name;
        public P2AParameterKind Kind;
        public bool Required;
        public string[] EnumValues;
        public int Minimum;
        public int Maximum;
        public string Pattern;

        public bool Validate(object value, out string error)
        {
            error = null;
            if (Kind == P2AParameterKind.String)
            {
                string text = value as string;
                if (text == null) { error = "expected_string"; return false; }
                if (EnumValues != null && Array.IndexOf(EnumValues, text) < 0) { error = "invalid_enum"; return false; }
                if (!string.IsNullOrEmpty(Pattern) && !Regex.IsMatch(text, Pattern)) { error = "invalid_reference"; return false; }
                return true;
            }

            if (Kind == P2AParameterKind.Integer)
            {
                if (!(value is double)) { error = "expected_integer"; return false; }
                double number = (double)value;
                if (Math.Abs(number - Math.Round(number)) > 0.000001 || number < Minimum || number > Maximum)
                {
                    error = "integer_out_of_range";
                    return false;
                }
                return true;
            }

            if (!(value is bool)) { error = "expected_boolean"; return false; }
            return true;
        }
    }

    public sealed class P2AActionDefinition
    {
        public string Name;
        public string Category;
        public Dictionary<string, P2AParameterDefinition> Parameters = new Dictionary<string, P2AParameterDefinition>(StringComparer.Ordinal);
        public string[] Permissions;
        public string[] Prerequisites;
        public string[] FailureCodes;
        public string Executor;
        public string[] AllowedActorKinds;
        public string[] AllowedTargetKinds;
    }

    /// <summary>
    /// Runtime authorization snapshot. It is supplied by the owning game systems;
    /// it never grants an Agent permission to write a result.
    /// </summary>
    public sealed class ActionValidationContext
    {
        public readonly HashSet<string> AuthorizedActorRefs = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> AuthorizedTargetRefs = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> Permissions = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> SatisfiedPrerequisites = new HashSet<string>(StringComparer.Ordinal);
        public bool EnforceAuthorization = true;

        public static ActionValidationContext SchemaOnly()
        {
            return new ActionValidationContext { EnforceAuthorization = false };
        }

        public bool HasPermission(string permission) => !EnforceAuthorization || Permissions.Contains(permission);
        public bool HasPrerequisite(string prerequisite) => !EnforceAuthorization || SatisfiedPrerequisites.Contains(prerequisite);
        public bool HasActor(string actorRef) => !EnforceAuthorization || AuthorizedActorRefs.Contains(actorRef);
        public bool HasTarget(string targetRef) => !EnforceAuthorization || AuthorizedTargetRefs.Contains(targetRef);
    }

    /// <summary>
    /// D8-A04 的 60 个 P2A 动作注册表。参数、权限、前置、失败码和执行者均为提议边界元数据。
    /// 强逻辑系统仍负责执行和最终结果。
    /// </summary>
    public static class P2AActionRegistry
    {
        private static readonly Dictionary<string, P2AActionDefinition> Definitions =
            new Dictionary<string, P2AActionDefinition>(StringComparer.Ordinal);

        public static IReadOnlyDictionary<string, P2AActionDefinition> All => Definitions;

        static P2AActionRegistry()
        {
            Add("trim_sail", "navigation", "ship.sail", "ShipSystem.SailController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "crew.on_duty" }, new[] { "ship_unavailable", "crew_unavailable", "invalid_sail_setting" }, P("sail", "main", "fore", "jib", "all"), P("setting", "furled", "reefed", "half", "full"));
            Add("steer", "navigation", "ship.steer", "ShipSystem.HelmController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "helmsman.assigned" }, new[] { "ship_unavailable", "helmsman_unavailable", "steering_locked" }, P("direction", "port", "starboard", "center"), P("intensity", "微调", "常规", "满舵"));
            Add("set_course", "navigation", "ship.course", "ShipSystem.CoursePlanner", new[] { "crew", "player" }, new[] { "ship" }, new[] { "chart.available", "ship.present" }, new[] { "unknown_destination", "chart_unavailable", "route_blocked" }, Ref("destination_ref", "island", "port", "landmark"));
            Add("drop_anchor", "navigation", "ship.anchor", "ShipSystem.AnchorController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "water_depth_known" }, new[] { "ship_unavailable", "unsafe_depth", "anchor_damaged" });
            Add("weigh_anchor", "navigation", "ship.anchor", "ShipSystem.AnchorController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "anchor_deployed" }, new[] { "ship_unavailable", "anchor_not_deployed", "anchor_stuck" });
            Add("stop_ship", "navigation", "ship.stop", "ShipSystem.ManeuverController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present" }, new[] { "ship_unavailable", "wind_too_strong", "stop_refused" });
            Add("set_watch", "navigation", "crew.assign_duty", "ShipSystem.WatchScheduler", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "crew.onboard" }, new[] { "crew_not_found", "duty_unavailable", "duplicate_watch" }, P("duty", "helm", "lookout", "gunner", "repair"), Ref("crew_ref", "crew"));
            Add("report", "navigation", "ship.observe", "Environment.ObservationSystem", new[] { "crew", "player" }, new[] { "ship", "none" }, new[] { "crew.on_duty" }, new[] { "observer_unavailable", "view_obstructed", "report_unreliable" }, P("topic", "island", "vessel", "hazard", "weather_signs", "soundings"));
            Add("heave_to", "navigation", "ship.maneuver", "ShipSystem.ManeuverController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "sails_set" }, new[] { "ship_unavailable", "sail_failure", "maneuver_refused" });
            Add("change_speed", "navigation", "ship.sail", "ShipSystem.SailController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present" }, new[] { "ship_unavailable", "sail_failure", "speed_limit" }, P("setting", "slow", "cruise", "fast"));

            Add("fire_cannon", "shipboard", "combat.fire", "CombatSystem.CannonController", new[] { "crew", "player" }, new[] { "ship", "npc" }, new[] { "combat.engaged", "cannon_loaded" }, new[] { "no_target", "cannon_unavailable", "ammunition_unavailable", "unsafe_fire_order" }, P("cannon_group", "port", "starboard", "bow", "stern"), P("ammunition", "roundshot", "chainshot", "grapeshot"));
            Add("repair", "shipboard", "ship.repair", "ShipSystem.RepairController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "repair_tools" }, new[] { "ship_unavailable", "tools_unavailable", "section_unreachable" }, P("section", "hull", "sails", "mast", "rudder", "pump", "cannon"), P("urgency", "inspect", "patch", "emergency"));
            Add("bilge_pump", "shipboard", "ship.pump", "ShipSystem.BilgeController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "pump_operable" }, new[] { "pump_unavailable", "crew_unavailable", "water_level_critical" }, P("mode", "manual", "crew_shift", "emergency"));
            Add("ration", "shipboard", "crew.ration", "CrewSystem.RationController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "crew.onboard", "provisions.available" }, new[] { "provisions_unavailable", "crew_conflict", "plan_not_allowed" }, P("plan", "lean", "standard", "generous"));
            Add("hold_below", "shipboard", "legal.detain", "FactionLegalSystem.DetentionController", new[] { "player", "crew" }, new[] { "ship", "crew" }, new[] { "ship.secure", "legal.authority" }, new[] { "crew_not_found", "authority_missing", "detention_unsafe" }, Ref("crew_ref", "crew"), I("duration_hours", 1, 72));
            Add("reward_crew", "shipboard", "crew.recognize", "CrewSystem.RecognitionController", new[] { "player", "crew" }, new[] { "ship", "crew" }, new[] { "crew.onboard" }, new[] { "crew_not_found", "recognition_conflict", "crew_unavailable" }, Ref("crew_ref", "crew"), P("recognition", "thanks", "leave", "public_praise", "duty_preference"));
            Add("launch_boat", "shipboard", "ship.launch_boat", "ShipSystem.BoatController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "boat_available", "sea_state_tolerable" }, new[] { "boat_unavailable", "sea_state_unsafe", "crew_unavailable" }, P("boat_purpose", "shore_party", "salvage", "scouting", "rescue"));
            Add("board", "shipboard", "combat.board", "CombatSystem.BoardingController", new[] { "crew", "player" }, new[] { "ship", "npc" }, new[] { "combat.engaged", "boarding_distance" }, new[] { "no_target", "distance_too_far", "boarding_refused", "crew_unready" }, P("stance", "hail", "grapple", "withdraw"));
            Add("extinguish_fire", "shipboard", "ship.fire_response", "ShipSystem.FireController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "fire_detected", "water_or_blanket" }, new[] { "fire_not_found", "equipment_unavailable", "crew_unavailable" }, P("section", "galley", "hold", "deck", "cabin", "mast"));
            Add("secure_cargo", "shipboard", "cargo.secure", "InventoryCargoSystem.SecuringController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "ship.present", "cargo_loaded" }, new[] { "hold_not_found", "cargo_unavailable", "crew_unavailable" }, Ref("hold_ref", "hold", "cargo_hold"));

            Add("assemble_crew", "crew", "crew.assemble", "CrewSystem.LandingPartyController", new[] { "player", "crew" }, new[] { "island", "port" }, new[] { "crew.onboard", "shore_access" }, new[] { "team_not_found", "shore_access_missing", "equipment_unavailable" }, Ref("team_ref", "team"), P("equipment_plan", "light", "survey", "medical", "defensive"));
            Add("gather", "crew", "ecology.gather", "EcologySystem.GatherController", new[] { "crew", "player" }, new[] { "island", "port" }, new[] { "shore_access", "valid_resource_zone" }, new[] { "shore_access_missing", "resource_unavailable", "legal_restriction" }, P("resource_type", "water", "food", "medicine", "timber"));
            Add("scout", "crew", "exploration.scout", "QuestExplorationSystem.ScoutController", new[] { "crew", "player" }, new[] { "island", "port", "sector" }, new[] { "shore_access", "scout_available" }, new[] { "area_unknown", "scout_unavailable", "legal_restriction" }, Ref("area_ref", "island", "port", "sector"));
            Add("treat", "crew", "crew.medicine", "CrewSystem.MedicalController", new[] { "crew", "player" }, new[] { "crew", "ship" }, new[] { "medical_kit", "crew_onboard" }, new[] { "crew_not_found", "medical_unavailable", "treatment_unsafe" }, Ref("crew_ref", "crew"), P("treatment", "first_aid", "rest", "herbal", "surgery_request"));
            Add("trade", "crew", "economy.negotiate", "EconomySystem.TradeController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "counterparty_present", "market_open" }, new[] { "counterparty_unavailable", "market_closed", "legal_restriction" }, Ref("counterparty_ref", "npc", "faction", "port"), Ref("commodity_ref", "item"), P("intent", "inquire", "offer", "request_exchange"));
            Add("cook_meal", "crew", "crew.cook", "CrewSystem.GalleyController", new[] { "crew", "player" }, new[] { "ship" }, new[] { "galley_operable", "ingredients_available" }, new[] { "galley_unavailable", "ingredients_unavailable", "cook_unavailable" }, P("meal_plan", "stew", "bread", "fish", "preserve"));
            Add("distribute_rations", "crew", "crew.ration", "CrewSystem.RationController", new[] { "crew", "player" }, new[] { "ship", "crew" }, new[] { "provisions.available", "crew.onboard" }, new[] { "provisions_unavailable", "recipient_unavailable", "distribution_conflict" }, P("recipient_scope", "all_crew", "watch_team", "landing_party", "medical_case"));
            Add("assign_duty", "crew", "crew.assign_duty", "CrewSystem.DutyRosterController", new[] { "player", "crew" }, new[] { "crew", "ship" }, new[] { "crew.onboard", "duty_roster_open" }, new[] { "crew_not_found", "duty_unavailable", "roster_locked" }, Ref("crew_ref", "crew"), P("duty", "helm", "lookout", "gunner", "repair", "cook", "medic", "deckhand"));
            Add("call_meeting", "crew", "crew.meeting", "CrewSystem.MeetingController", new[] { "player", "crew" }, new[] { "ship" }, new[] { "crew.onboard", "safe_meeting_place" }, new[] { "crew_unavailable", "meeting_unsafe", "topic_restricted" }, P("topic", "route", "ration", "discipline", "danger", "shore_leave"));
            Add("request_leave", "crew", "crew.leave", "CrewSystem.LeaveController", new[] { "player", "crew" }, new[] { "crew", "port" }, new[] { "port_access", "crew.onboard" }, new[] { "crew_not_found", "port_access_missing", "leave_conflict" }, Ref("crew_ref", "crew"), P("reason", "family", "health", "faith", "grief", "personal"));

            Add("greet_contact", "negotiation", "social.speak", "FactionLegalSystem.DiplomacyController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "contact_present" }, new[] { "contact_unavailable", "protocol_unknown", "legal_restriction" }, P("protocol", "formal", "shipboard", "community", "religious", "cautious"));
            Add("ask_information", "negotiation", "social.inquire", "QuestExplorationSystem.InformationController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "contact_present" }, new[] { "contact_unavailable", "topic_restricted", "information_refused" }, P("topic", "route", "market", "law", "rumor", "shelter", "danger"));
            Add("propose_trade", "negotiation", "economy.negotiate", "EconomySystem.NegotiationController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "contact_present", "market_open" }, new[] { "contact_unavailable", "market_closed", "terms_rejected" }, P("trade_intent", "inquire", "barter", "consignment", "exchange_service"));
            Add("propose_alliance", "negotiation", "faction.diplomacy", "FactionLegalSystem.DiplomacyController", new[] { "player", "crew" }, new[] { "faction", "npc" }, new[] { "contact_present", "faction_access" }, new[] { "contact_unavailable", "faction_access_missing", "terms_rejected" }, P("scope", "mutual_aid", "safe_passage", "information", "escort"));
            Add("request_passage", "negotiation", "faction.passage", "FactionLegalSystem.PassageController", new[] { "player", "crew" }, new[] { "faction", "port", "sector" }, new[] { "route_known", "contact_present" }, new[] { "route_unknown", "contact_unavailable", "permit_required" }, Ref("route_ref", "route", "port", "sector"), P("protocol", "petition", "escort", "safe_conduct"));
            Add("request_mediation", "negotiation", "faction.mediation", "FactionLegalSystem.MediationController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "mediator_present", "dispute_known" }, new[] { "mediator_unavailable", "dispute_unknown", "legal_restriction" }, Ref("dispute_ref", "dispute"), P("urgency", "routine", "urgent", "emergency"));
            Add("offer_apology", "negotiation", "social.repair", "CrewSystem.RelationController", new[] { "player", "crew" }, new[] { "crew", "npc", "faction" }, new[] { "recipient_present" }, new[] { "recipient_unavailable", "apology_rejected", "legal_restriction" }, Ref("recipient_ref", "crew", "npc", "faction"), P("form", "private", "public", "restitution_request", "ritual"));
            Add("negotiate_terms", "negotiation", "social.negotiate", "FactionLegalSystem.TermsController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "contact_present", "terms_open" }, new[] { "contact_unavailable", "terms_closed", "authority_missing" }, P("term_category", "access", "service", "protection", "labor", "exchange"), P("posture", "ask", "firm", "concede", "pause"));
            Add("accept_offer", "negotiation", "social.negotiate", "FactionLegalSystem.TermsController", new[] { "player", "crew" }, new[] { "offer", "npc", "faction" }, new[] { "offer_open", "terms_reviewed" }, new[] { "offer_unknown", "offer_expired", "authority_missing" }, Ref("offer_ref", "offer"));
            Add("end_talk", "negotiation", "social.speak", "FactionLegalSystem.DiplomacyController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "conversation_open" }, new[] { "conversation_not_open", "contact_unavailable", "unsafe_exit" }, P("reason", "agreement", "pause", "insult", "danger", "no_progress"));

            Add("survey_coast", "exploration", "exploration.survey", "Environment.ObservationSystem", new[] { "crew", "player" }, new[] { "island", "sector" }, new[] { "survey_tools", "safe_position" }, new[] { "sector_unknown", "tools_unavailable", "sea_state_unsafe" }, Ref("sector_ref", "sector"), P("survey_method", "shoreline", "soundings", "wildlife", "shelter"));
            Add("inspect_wreck", "exploration", "exploration.wreck", "QuestExplorationSystem.WreckController", new[] { "crew", "player" }, new[] { "wreck", "ship", "sector" }, new[] { "wreck_known", "safe_access" }, new[] { "wreck_unknown", "access_unsafe", "legal_restriction" }, Ref("wreck_ref", "wreck"), P("purpose", "identify", "rescue", "salvage", "evidence"));
            Add("map_landmark", "exploration", "exploration.map", "QuestExplorationSystem.MappingController", new[] { "crew", "player" }, new[] { "landmark", "island", "sector" }, new[] { "map_available", "landmark_visible" }, new[] { "landmark_unknown", "map_unavailable", "visibility_blocked" }, Ref("landmark_ref", "landmark"));
            Add("observe_animal", "exploration", "ecology.observe", "EcologySystem.ObservationController", new[] { "crew", "player" }, new[] { "animal", "fish_school", "island" }, new[] { "animal_present", "observation_tools" }, new[] { "animal_unavailable", "approach_unsafe", "observation_blocked" }, P("observation_method", "lookout", "quiet_approach", "track", "record"));
            Add("rescue_animal", "exploration", "ecology.rescue", "AnimalCareSystem.RescueController", new[] { "crew", "player" }, new[] { "animal", "island" }, new[] { "animal_in_distress", "care_tools" }, new[] { "animal_unknown", "care_tools_unavailable", "legal_restriction" }, Ref("animal_ref", "animal"), P("care_plan", "stabilize", "transport", "release_site", "contact_caregiver"));
            Add("release_animal", "exploration", "ecology.release", "AnimalCareSystem.ReleaseController", new[] { "crew", "player" }, new[] { "animal", "island" }, new[] { "animal_in_care", "release_site_known" }, new[] { "animal_unknown", "release_site_missing", "legal_restriction" }, Ref("animal_ref", "animal"), P("release_plan", "home_habitat", "protected_area", "caregiver", "defer"));
            Add("fish", "exploration", "ecology.fish", "EcologySystem.FishingController", new[] { "crew", "player" }, new[] { "fish_school", "sector", "ship" }, new[] { "fishing_gear", "fish_zone_known" }, new[] { "gear_unavailable", "zone_unknown", "legal_restriction" }, P("gear", "line", "net", "trap", "handline"), Ref("zone_ref", "sector"));
            Add("collect_water", "exploration", "ecology.collect", "EcologySystem.GatherController", new[] { "crew", "player" }, new[] { "island", "port", "ship" }, new[] { "source_known", "containers_available" }, new[] { "source_unknown", "containers_unavailable", "water_unsafe" }, Ref("source_ref", "water_source", "island", "port"), P("method", "fill_cask", "filter", "boil"));
            Add("collect_firewood", "exploration", "ecology.collect", "EcologySystem.GatherController", new[] { "crew", "player" }, new[] { "island", "sector" }, new[] { "shore_access", "tool_axe_or_saw" }, new[] { "area_unknown", "tools_unavailable", "legal_restriction" }, Ref("area_ref", "island", "sector"), P("method", "fallen_only", "managed_cut", "salvage"));
            Add("enter_cave", "exploration", "exploration.cave", "QuestExplorationSystem.CaveController", new[] { "crew", "player" }, new[] { "cave", "island" }, new[] { "cave_known", "lamp_or_torch", "safe_access" }, new[] { "cave_unknown", "light_unavailable", "access_unsafe" }, Ref("cave_ref", "cave"), P("formation", "survey", "passage", "shelter", "rescue"));

            Add("buy_supplies", "economy", "economy.purchase", "EconomySystem.PurchaseController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "vendor_present", "market_open" }, new[] { "vendor_unavailable", "market_closed", "credit_rejected", "supply_unavailable" }, Ref("vendor_ref", "npc", "faction", "port"), P("supply_category", "food", "water", "medicine", "ammunition", "repair_material"));
            Add("sell_cargo", "economy", "economy.sell", "EconomySystem.SaleController", new[] { "player", "crew" }, new[] { "npc", "faction", "port" }, new[] { "vendor_present", "cargo_loaded", "market_open" }, new[] { "counterparty_unavailable", "cargo_unknown", "market_closed", "legal_restriction" }, Ref("counterparty_ref", "npc", "faction", "port"), Ref("cargo_ref", "cargo"));
            Add("visit_shipyard", "economy", "shipyard.request", "BuildingShipyardSystem.ShipyardController", new[] { "player", "crew" }, new[] { "building", "port", "ship" }, new[] { "shipyard_open", "ship_docked" }, new[] { "shipyard_unavailable", "ship_not_docked", "service_unavailable" }, P("service", "inspect", "repair_request", "refit_request", "dock_request"));
            Add("request_upgrade", "economy", "shipyard.upgrade", "BuildingShipyardSystem.UpgradeController", new[] { "player", "crew" }, new[] { "ship", "building" }, new[] { "shipyard_open", "ship_docked", "upgrade_known" }, new[] { "shipyard_unavailable", "ship_not_docked", "upgrade_unknown", "materials_unavailable" }, Ref("upgrade_ref", "upgrade"), P("priority", "inspect", "planned", "urgent"));
            Add("hire_crew", "economy", "crew.hire", "CrewSystem.RecruitmentController", new[] { "player", "crew" }, new[] { "npc", "port" }, new[] { "port_access", "candidate_present", "berth_available" }, new[] { "candidate_unavailable", "berth_unavailable", "legal_restriction", "terms_rejected" }, P("role", "sailor", "navigator", "gunner", "carpenter", "medic", "cook", "trader"), P("contract_intent", "inquire", "trial", "offer_position"));
            Add("dismiss_crew", "economy", "crew.dismiss", "CrewSystem.RecruitmentController", new[] { "player" }, new[] { "crew", "port" }, new[] { "crew.onboard", "port_access" }, new[] { "crew_not_found", "port_access_missing", "dismissal_blocked" }, Ref("crew_ref", "crew"), P("notice", "warning", "release_at_port", "mutual_agreement", "emergency"));
            Add("store_cargo", "economy", "cargo.store", "InventoryCargoSystem.StorageController", new[] { "player", "crew" }, new[] { "ship", "building", "port" }, new[] { "storage_access", "cargo_loaded" }, new[] { "storage_unavailable", "cargo_unknown", "capacity_unavailable" }, Ref("hold_ref", "hold", "warehouse"), Ref("cargo_ref", "cargo"));
            Add("retrieve_cargo", "economy", "cargo.retrieve", "InventoryCargoSystem.StorageController", new[] { "player", "crew" }, new[] { "ship", "building", "port" }, new[] { "storage_access", "cargo_stored" }, new[] { "storage_unavailable", "cargo_unknown", "capacity_unavailable" }, Ref("hold_ref", "hold", "warehouse"), Ref("cargo_ref", "cargo"));
            Add("submit_quest", "economy", "quest.submit", "QuestSystem.SubmissionController", new[] { "player", "crew" }, new[] { "quest", "npc", "faction", "port" }, new[] { "quest_open", "objective_ready" }, new[] { "quest_unknown", "objective_incomplete", "submission_closed" }, Ref("quest_ref", "quest"));
            Add("tend_garden", "economy", "farming.tend", "FarmingSystem.GardenController", new[] { "player", "crew" }, new[] { "land", "island", "building" }, new[] { "plot_access", "tool_available" }, new[] { "plot_unknown", "plot_access_missing", "tool_unavailable", "season_closed" }, Ref("plot_ref", "land"), P("task", "inspect", "water", "weed", "plant", "harvest_request"));
        }

        private static void Add(string name, string category, string permission, string executor, string[] actorKinds, string[] targetKinds, string[] prerequisites, string[] failureCodes, params P2AParameterDefinition[] parameters)
        {
            var definition = new P2AActionDefinition
            {
                Name = name,
                Category = category,
                Permissions = new[] { permission },
                Prerequisites = prerequisites,
                FailureCodes = failureCodes,
                Executor = executor,
                AllowedActorKinds = actorKinds,
                AllowedTargetKinds = targetKinds
            };
            if (parameters != null)
                foreach (var parameter in parameters) definition.Parameters.Add(parameter.Name, parameter);
            Definitions.Add(name, definition);
        }

        private static P2AParameterDefinition P(string name, params string[] enumValues)
        {
            return new P2AParameterDefinition { Name = name, Kind = P2AParameterKind.String, Required = true, EnumValues = enumValues };
        }

        private static P2AParameterDefinition Ref(string name, params string[] kinds)
        {
            return new P2AParameterDefinition
            {
                Name = name,
                Kind = P2AParameterKind.String,
                Required = true,
                Pattern = "^(" + string.Join("|", kinds) + "):[A-Za-z0-9._:-]+$"
            };
        }

        private static P2AParameterDefinition I(string name, int minimum, int maximum)
        {
            return new P2AParameterDefinition { Name = name, Kind = P2AParameterKind.Integer, Required = true, Minimum = minimum, Maximum = maximum };
        }

        public static bool TryGet(string name, out P2AActionDefinition definition)
        {
            return Definitions.TryGetValue(name ?? string.Empty, out definition);
        }

        public static string GetReferenceKind(string reference)
        {
            if (string.Equals(reference, "none", StringComparison.Ordinal)) return "none";
            if (string.IsNullOrEmpty(reference)) return string.Empty;
            int separator = reference.IndexOf(':');
            return separator > 0 ? reference.Substring(0, separator) : string.Empty;
        }
    }
}
