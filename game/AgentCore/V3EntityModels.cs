using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Whalebone.Game.Config;

namespace Whalebone.Agent.V3
{
    // ============================================================
    // D8-A01 v3 data model
    //
    // 这是纯数据层：不引用 Unity、Systems、网络或 LLM。
    // 数值裁决仍由 DESIGN_BOUNDARIES 指定的领域系统负责；这些类型只
    // 提供稳定字段、明确单位、版本和可序列化形状。
    // ============================================================

    public static class V3ModelVersions
    {
        public const int EntitySchemaVersion = 1;
        public const int TargetSaveVersion = 3;
    }

    public enum EntityPhase
    {
        Unknown = 0,
        Draft = 1,
        Prototype = 2,
        Production = 3,
        Deprecated = 4
    }

    public enum V3EntityKind
    {
        Unknown = 0,
        Player = 1,
        Ship = 2,
        Crew = 3,
        Relation = 4,
        Island = 5,
        LandParcel = 6,
        Building = 7,
        Item = 8,
        Animal = 9,
        FishSchool = 10,
        Pet = 11,
        FinanceAccount = 12,
        Quest = 13,
        WorldEvent = 14,
        Reward = 15,
        AppearanceLoadout = 16
    }

    public enum LocationKind
    {
        Unknown = 0,
        Port = 1,
        OpenSea = 2,
        Island = 3,
        LandParcel = 4,
        Building = 5,
        Ship = 6
    }

    public enum PlayerStatus
    {
        Unknown = 0,
        Active = 1,
        Away = 2,
        Retired = 3,
        Dead = 4
    }

    public enum ShipStatus
    {
        Unknown = 0,
        InPort = 1,
        Sailing = 2,
        Anchored = 3,
        Damaged = 4,
        Lost = 5,
        Retired = 6
    }

    public enum CrewStatus
    {
        Unknown = 0,
        Active = 1,
        Sick = 2,
        Injured = 3,
        Resting = 4,
        Deserted = 5,
        Dead = 6
    }

    public enum RelationType
    {
        Unknown = 0,
        Affinity = 1,
        Trust = 2,
        Loyalty = 3,
        Fear = 4,
        Debt = 5,
        Rivalry = 6,
        Kinship = 7,
        Romance = 8,
        Faction = 9
    }

    public enum RelationVisibility
    {
        Unknown = 0,
        Public = 1,
        Private = 2,
        Rumored = 3,
        Hidden = 4
    }

    public enum IslandStatus
    {
        Unknown = 0,
        Discovered = 1,
        Visited = 2,
        Restricted = 3,
        Abandoned = 4
    }

    public enum ParcelPurpose
    {
        Unknown = 0,
        Residential = 1,
        Agricultural = 2,
        Pasture = 3,
        Fishery = 4,
        Industrial = 5,
        Commercial = 6,
        Conservation = 7,
        Sacred = 8
    }

    public enum BuildingFunction
    {
        Unknown = 0,
        Residence = 1,
        Warehouse = 2,
        Workshop = 3,
        Farm = 4,
        Fishery = 5,
        Shipyard = 6,
        Tavern = 7,
        Clinic = 8,
        Market = 9,
        Fortification = 10,
        Religious = 11,
        Civic = 12
    }

    public enum BuildingOpenState
    {
        Unknown = 0,
        Closed = 1,
        Open = 2,
        Damaged = 3,
        UnderConstruction = 4
    }

    public enum ItemCategory
    {
        Unknown = 0,
        Tool = 1,
        Weapon = 2,
        Material = 3,
        Food = 4,
        Cargo = 5,
        Clothing = 6,
        Medical = 7,
        Quest = 8,
        Collectible = 9,
        Decor = 10
    }

    public enum ItemStatus
    {
        Unknown = 0,
        Active = 1,
        Restricted = 2,
        Deprecated = 3,
        Draft = 4
    }

    public enum AnimalSex
    {
        Unknown = 0,
        Female = 1,
        Male = 2,
        Indeterminate = 3
    }

    public enum AnimalStatus
    {
        Unknown = 0,
        Wild = 1,
        Observed = 2,
        Rescued = 3,
        Captive = 4,
        Released = 5,
        Dead = 6
    }

    public enum AnimalBehavior
    {
        Unknown = 0,
        Foraging = 1,
        Alert = 2,
        Attacking = 3,
        BreedingHint = 4,
        Migrating = 5,
        Resting = 6
    }

    public enum FishMigration
    {
        Unknown = 0,
        Resident = 1,
        Seasonal = 2,
        Passing = 3,
        Recovering = 4
    }

    public enum PetCareState
    {
        Unknown = 0,
        Healthy = 1,
        Hungry = 2,
        Dirty = 3,
        Sick = 4,
        Lost = 5,
        Dead = 6
    }

    public enum FinanceAccountType
    {
        Unknown = 0,
        Cash = 1,
        Credit = 2,
        Loan = 3,
        Insurance = 4,
        Mortgage = 5,
        Ledger = 6
    }

    public enum FinanceDefaultState
    {
        Unknown = 0,
        Current = 1,
        Delinquent = 2,
        Defaulted = 3,
        Closed = 4
    }

    public enum QuestStatus
    {
        Unknown = 0,
        Locked = 1,
        Available = 2,
        Active = 3,
        Failed = 4,
        Completed = 5,
        Abandoned = 6,
        Recovering = 7
    }

    public enum EventOutcomeStatus
    {
        Unknown = 0,
        Proposed = 1,
        Succeeded = 2,
        Failed = 3,
        Cancelled = 4,
        Interrupted = 5,
        Recovered = 6,
        Duplicate = 7
    }

    public enum RewardStatus
    {
        Unknown = 0,
        Pending = 1,
        Claimed = 2,
        Reclaimed = 3,
        Rejected = 4,
        Expired = 5
    }

    public enum AnchorState
    {
        Unknown = 0,
        Stowed = 1,
        Deployed = 2,
        Dragging = 3,
        Stuck = 4
    }

    public enum RepairState
    {
        Unknown = 0,
        None = 1,
        Needed = 2,
        InProgress = 3,
        Complete = 4
    }

    /// <summary>整数金币；允许负值表示债务或账本净额，不负责经济裁决。</summary>
    public struct MoneyAmount
    {
        public long Value;

        public MoneyAmount(long value) { Value = NumericConfiguration.ClampMoney(value); }
        public static MoneyAmount Zero => new MoneyAmount(0L);
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>重量，单位为公吨；模型层不换算容量和载重。</summary>
    public struct WeightAmount
    {
        public decimal Value;

        public WeightAmount(decimal value) { Value = NumericConfiguration.ClampWeight(value); }
        public static WeightAmount Zero => new WeightAmount(0m);
    }

    /// <summary>体积，单位为立方米；模型层不裁决装载规则。</summary>
    public struct VolumeAmount
    {
        public decimal Value;

        public VolumeAmount(decimal value) { Value = NumericConfiguration.ClampVolume(value); }
        public static VolumeAmount Zero => new VolumeAmount(0m);
    }

    /// <summary>世界日，非负整数；时间推进由 Environment/Time 系统负责。</summary>
    public struct WorldDay
    {
        public int Value;

        public WorldDay(int value) { Value = NumericConfiguration.ClampWorldDay(value); }
        public static WorldDay Zero => new WorldDay(0);
    }

    /// <summary>一日内的归一化时间，范围 0..1。</summary>
    public struct TimeOfDay
    {
        public decimal Normalized;

        public TimeOfDay(decimal normalized)
        {
            Normalized = NumericConfiguration.ClampTimeOfDay(normalized);
        }

        public static TimeOfDay Start => new TimeOfDay((decimal)NumericConfiguration.Default.TimeOfDay.DefaultValue);
    }

    /// <summary>关系值，范围 -100..100；不执行关系变化。</summary>
    public struct RelationValue
    {
        public int Value;

        public RelationValue(int value)
        {
            Value = NumericConfiguration.ClampRelation(value);
        }

        public static RelationValue Neutral => new RelationValue(0);
    }

    /// <summary>百分比状态，范围 0..100；健康、疲劳、耐久等字段按语义解释。</summary>
    public struct PercentValue
    {
        public int Value;

        public PercentValue(int value)
        {
            Value = NumericConfiguration.ClampPercent(value);
        }

        public static PercentValue Zero => new PercentValue(0);
        public static PercentValue Full => new PercentValue((int)NumericConfiguration.Default.Health.DefaultValue);
    }

    public class V3EntityBase
    {
        [JsonPropertyName("id")]
        public string Id = "";

        [JsonPropertyName("schema_version")]
        public int SchemaVersion = V3ModelVersions.EntitySchemaVersion;

        [JsonPropertyName("display_name_key")]
        public string DisplayNameKey = "";

        [JsonPropertyName("source_note")]
        public string SourceNote = "";

        [JsonPropertyName("historical_tag")]
        public string HistoricalTag = "";

        [JsonPropertyName("phase")]
        public EntityPhase Phase = EntityPhase.Unknown;

        [JsonPropertyName("owner_system")]
        public string OwnerSystem = "";

        [JsonPropertyName("test_id")]
        public string TestId = "";

        [JsonPropertyName("created_day")]
        public WorldDay CreatedDay = WorldDay.Zero;

        [JsonPropertyName("updated_day")]
        public WorldDay UpdatedDay = WorldDay.Zero;

        [JsonPropertyName("location_ref")]
        public string LocationRef = "";

        [JsonPropertyName("state_flags")]
        public Dictionary<string, string> StateFlags = new Dictionary<string, string>();

        [JsonPropertyName("revision")]
        public long Revision;
    }

    public static class V3StableIds
    {
        public static string Prefix(V3EntityKind kind)
        {
            switch (kind)
            {
                case V3EntityKind.Player: return "player_";
                case V3EntityKind.Ship: return "ship_";
                case V3EntityKind.Crew: return "crew_";
                case V3EntityKind.Relation: return "relation_";
                case V3EntityKind.Island: return "island_";
                case V3EntityKind.LandParcel: return "parcel_";
                case V3EntityKind.Building: return "building_";
                case V3EntityKind.Item: return "item_";
                case V3EntityKind.Animal: return "animal_";
                case V3EntityKind.FishSchool: return "fish_";
                case V3EntityKind.Pet: return "pet_";
                case V3EntityKind.FinanceAccount: return "finance_";
                case V3EntityKind.Quest: return "quest_";
                case V3EntityKind.WorldEvent: return "event_";
                case V3EntityKind.Reward: return "reward_";
                case V3EntityKind.AppearanceLoadout: return "outfit_";
                default: return "";
            }
        }

        /// <summary>从稳定 slug 生成可重复的 ID；不使用随机数、时间或全局状态。</summary>
        public static string Create(V3EntityKind kind, string slug)
        {
            string prefix = Prefix(kind);
            if (prefix.Length == 0) return "";
            return prefix + NormalizeSlug(slug);
        }

        public static bool IsValid(V3EntityKind kind, string id)
        {
            string prefix = Prefix(kind);
            if (prefix.Length == 0 || string.IsNullOrEmpty(id) || !id.StartsWith(prefix, StringComparison.Ordinal))
                return false;
            if (id.Length == prefix.Length) return false;
            for (int i = prefix.Length; i < id.Length; i++)
            {
                char c = id[i];
                if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                    return false;
            }
            return true;
        }

        public static string NormalizeSlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return "unknown";
            var sb = new StringBuilder();
            bool separator = false;
            foreach (char source in slug.Trim().ToLowerInvariant())
            {
                bool asciiLetter = source >= 'a' && source <= 'z';
                bool digit = source >= '0' && source <= '9';
                if (asciiLetter || digit)
                {
                    sb.Append(source);
                    separator = false;
                }
                else if (!separator && sb.Length > 0)
                {
                    sb.Append('_');
                    separator = true;
                }
            }
            while (sb.Length > 0 && sb[sb.Length - 1] == '_') sb.Length--;
            return sb.Length == 0 ? "unknown" : sb.ToString();
        }
    }

    public sealed class LocationRefV3
    {
        [JsonPropertyName("kind")]
        public LocationKind Kind = LocationKind.Unknown;

        [JsonPropertyName("ref_id")]
        public string RefId = "";

        [JsonPropertyName("world_day")]
        public WorldDay WorldDay = WorldDay.Zero;

        [JsonPropertyName("time_of_day")]
        public TimeOfDay TimeOfDay = TimeOfDay.Start;
    }

    public sealed class ItemHolding
    {
        [JsonPropertyName("item_id")]
        public string ItemId = "";

        [JsonPropertyName("quantity")]
        public int Quantity;

        [JsonPropertyName("total_weight")]
        public WeightAmount TotalWeight = WeightAmount.Zero;

        [JsonPropertyName("total_volume")]
        public VolumeAmount TotalVolume = VolumeAmount.Zero;

        [JsonPropertyName("durability")]
        public PercentValue Durability = PercentValue.Full;
    }

    public sealed class LegacyD7LocationFields
    {
        [JsonPropertyName("type")]
        public string Type = "port";

        [JsonPropertyName("port_id")]
        public string PortId = "";

        [JsonPropertyName("x")]
        public decimal X;

        [JsonPropertyName("y")]
        public decimal Y;

        [JsonPropertyName("heading")]
        public decimal Heading;

        [JsonPropertyName("speed")]
        public decimal Speed;

        [JsonPropertyName("day_index")]
        public int DayIndex;

        [JsonPropertyName("normalized_time")]
        public decimal NormalizedTime = (decimal)NumericConfiguration.Default.TimeOfDay.DefaultValue;
    }

    /// <summary>PlayerState 的 D7 字段镜像；D3/D4/D5 快照保留为原始 JSON 边界。</summary>
    public sealed class LegacyD7PlayerFields
    {
        [JsonPropertyName("id")]
        public string Id = "";

        [JsonPropertyName("name")]
        public string Name = "";

        [JsonPropertyName("money")]
        public int Money;

        [JsonPropertyName("skills")]
        public Dictionary<string, int> Skills = new Dictionary<string, int>();

        [JsonPropertyName("traits")]
        public List<string> Traits = new List<string>();

        [JsonPropertyName("reputation")]
        public Dictionary<string, int> Reputation = new Dictionary<string, int>();

        [JsonPropertyName("inventory")]
        public Dictionary<string, int> Inventory = new Dictionary<string, int>();

        [JsonPropertyName("ship_id")]
        public string ShipId = "";

        [JsonPropertyName("location")]
        public LegacyD7LocationFields Location = new LegacyD7LocationFields();

        [JsonPropertyName("play_seconds")]
        public int PlaySeconds;

        [JsonPropertyName("d3_runtime_json")]
        public string D3RuntimeJson = "";

        [JsonPropertyName("d4_runtime_json")]
        public string D4RuntimeJson = "";

        [JsonPropertyName("d5_runtime_json")]
        public string D5RuntimeJson = "";
    }

    public sealed class LegacyD7ShipLogEntry
    {
        [JsonPropertyName("time")]
        public string Time = "";

        [JsonPropertyName("kind")]
        public string Kind = "auto";

        [JsonPropertyName("text")]
        public string Text = "";
    }

    /// <summary>ShipState 的 D7 字段镜像；HullMax 仍由船型定义回查。</summary>
    public sealed class LegacyD7ShipFields
    {
        [JsonPropertyName("id")]
        public string Id = "";

        [JsonPropertyName("ship_type_id")]
        public string ShipTypeId = "";

        [JsonPropertyName("name")]
        public string Name = "";

        [JsonPropertyName("hull")]
        public int Hull;

        [JsonPropertyName("cargo")]
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();

        [JsonPropertyName("cannons")]
        public int Cannons;

        [JsonPropertyName("loaded_count")]
        public int LoadedCount;

        [JsonPropertyName("supplies")]
        public Dictionary<string, int> Supplies = new Dictionary<string, int>();

        [JsonPropertyName("crew_capacity")]
        public int CrewCapacity;

        [JsonPropertyName("crew_ids")]
        public List<string> CrewIds = new List<string>();

        [JsonPropertyName("upgrades")]
        public Dictionary<string, int> Upgrades = new Dictionary<string, int>();

        [JsonPropertyName("state")]
        public Dictionary<string, decimal> State = new Dictionary<string, decimal>();

        [JsonPropertyName("ship_log")]
        public List<LegacyD7ShipLogEntry> ShipLog = new List<LegacyD7ShipLogEntry>();
    }

    /// <summary>CrewAgent/CrewAgentCodec 的 D7 字段镜像；JSON 占位不被误判为完整 v3 模型。</summary>
    public sealed class LegacyD7CrewFields
    {
        [JsonPropertyName("id")]
        public string Id = "";

        [JsonPropertyName("name")]
        public string Name = "";

        [JsonPropertyName("role")]
        public string Role = "";

        [JsonPropertyName("culture")]
        public string Culture = "";

        [JsonPropertyName("traits_json")]
        public string TraitsJson = "[]";

        [JsonPropertyName("stats_json")]
        public string StatsJson = "{}";

        [JsonPropertyName("persona_json")]
        public string PersonaJson = "{}";

        [JsonPropertyName("memory_json")]
        public string MemoryJson = "{\"summary\":\"\",\"events\":[]}";

        [JsonPropertyName("relations_json")]
        public string RelationsJson = "{}";
    }

    public sealed class PlayerEntity : V3EntityBase
    {
        [JsonPropertyName("legacy_d7")]
        public LegacyD7PlayerFields LegacyD7 = new LegacyD7PlayerFields();

        [JsonPropertyName("status")]
        public PlayerStatus Status = PlayerStatus.Unknown;

        [JsonPropertyName("pronouns")]
        public List<string> Pronouns = new List<string>();

        [JsonPropertyName("identity_history")]
        public List<string> IdentityHistory = new List<string>();

        [JsonPropertyName("appearance_loadout_id")]
        public string AppearanceLoadoutId = "";

        [JsonPropertyName("balance")]
        public MoneyAmount Balance = MoneyAmount.Zero;

        [JsonPropertyName("credit")]
        public MoneyAmount Credit = MoneyAmount.Zero;

        [JsonPropertyName("ship_ids")]
        public List<string> ShipIds = new List<string>();

        [JsonPropertyName("crew_ids")]
        public List<string> CrewIds = new List<string>();

        [JsonPropertyName("relation_ids")]
        public List<string> RelationIds = new List<string>();

        [JsonPropertyName("quest_ids")]
        public List<string> QuestIds = new List<string>();

        [JsonPropertyName("collection_ids")]
        public List<string> CollectionIds = new List<string>();
    }

    public sealed class ShipEntity : V3EntityBase
    {
        [JsonPropertyName("legacy_d7")]
        public LegacyD7ShipFields LegacyD7 = new LegacyD7ShipFields();

        [JsonPropertyName("status")]
        public ShipStatus Status = ShipStatus.Unknown;

        [JsonPropertyName("hull_integrity")]
        public PercentValue HullIntegrity = PercentValue.Full;

        [JsonPropertyName("mass")]
        public WeightAmount Mass = WeightAmount.Zero;

        [JsonPropertyName("cargo_capacity")]
        public VolumeAmount CargoCapacity = VolumeAmount.Zero;

        [JsonPropertyName("cabin_ids")]
        public List<string> CabinIds = new List<string>();

        [JsonPropertyName("function_point_ids")]
        public List<string> FunctionPointIds = new List<string>();

        [JsonPropertyName("anchor_state")]
        public AnchorState AnchorState = AnchorState.Unknown;

        [JsonPropertyName("repair_state")]
        public RepairState RepairState = RepairState.Unknown;

        [JsonPropertyName("ownership_ref")]
        public string OwnershipRef = "";

        [JsonPropertyName("appearance_loadout_id")]
        public string AppearanceLoadoutId = "";
    }

    public sealed class CrewEntity : V3EntityBase
    {
        [JsonPropertyName("legacy_d7")]
        public LegacyD7CrewFields LegacyD7 = new LegacyD7CrewFields();

        [JsonPropertyName("name")]
        public string Name = "";

        [JsonPropertyName("role_id")]
        public string RoleId = "";

        [JsonPropertyName("culture_id")]
        public string CultureId = "";

        [JsonPropertyName("status")]
        public CrewStatus Status = CrewStatus.Unknown;

        [JsonPropertyName("trait_ids")]
        public List<string> TraitIds = new List<string>();

        [JsonPropertyName("health")]
        public PercentValue Health = PercentValue.Full;

        [JsonPropertyName("fatigue")]
        public PercentValue Fatigue = PercentValue.Zero;

        [JsonPropertyName("relation_ids")]
        public List<string> RelationIds = new List<string>();

        [JsonPropertyName("goal_ids")]
        public List<string> GoalIds = new List<string>();

        [JsonPropertyName("commitment_ids")]
        public List<string> CommitmentIds = new List<string>();

        [JsonPropertyName("debt_account_ids")]
        public List<string> DebtAccountIds = new List<string>();

        [JsonPropertyName("task_ids")]
        public List<string> TaskIds = new List<string>();

        [JsonPropertyName("shift_ids")]
        public List<string> ShiftIds = new List<string>();

        [JsonPropertyName("appearance_loadout_id")]
        public string AppearanceLoadoutId = "";

        [JsonPropertyName("equipment_item_ids")]
        public List<string> EquipmentItemIds = new List<string>();

        [JsonPropertyName("pet_id")]
        public string PetId = "";

        [JsonPropertyName("personal_item_ids")]
        public List<string> PersonalItemIds = new List<string>();
    }

    public sealed class RelationEntity : V3EntityBase
    {
        [JsonPropertyName("source_id")]
        public string SourceId = "";

        [JsonPropertyName("target_id")]
        public string TargetId = "";

        [JsonPropertyName("relation_type")]
        public RelationType RelationType = RelationType.Unknown;

        [JsonPropertyName("value")]
        public RelationValue Value = RelationValue.Neutral;

        [JsonPropertyName("confidence")]
        public PercentValue Confidence = PercentValue.Zero;

        [JsonPropertyName("cause_event_ids")]
        public List<string> CauseEventIds = new List<string>();

        [JsonPropertyName("last_changed_day")]
        public WorldDay LastChangedDay = WorldDay.Zero;

        [JsonPropertyName("locked_by_story")]
        public bool LockedByStory;

        [JsonPropertyName("visibility")]
        public RelationVisibility Visibility = RelationVisibility.Unknown;
    }

    public sealed class IslandEntity : V3EntityBase
    {
        [JsonPropertyName("status")]
        public IslandStatus Status = IslandStatus.Unknown;

        [JsonPropertyName("terrain")]
        public string Terrain = "";

        [JsonPropertyName("geology")]
        public string Geology = "";

        [JsonPropertyName("climate")]
        public string Climate = "";

        [JsonPropertyName("water_source_ids")]
        public List<string> WaterSourceIds = new List<string>();

        [JsonPropertyName("habitat_ids")]
        public List<string> HabitatIds = new List<string>();

        [JsonPropertyName("population")]
        public int Population;

        [JsonPropertyName("ecology_metrics")]
        public Dictionary<string, PercentValue> EcologyMetrics = new Dictionary<string, PercentValue>();

        [JsonPropertyName("asset_ids")]
        public List<string> AssetIds = new List<string>();

        [JsonPropertyName("controller_ref")]
        public string ControllerRef = "";
    }

    public sealed class LandParcelEntity : V3EntityBase
    {
        [JsonPropertyName("purpose")]
        public ParcelPurpose Purpose = ParcelPurpose.Unknown;

        [JsonPropertyName("fertility")]
        public PercentValue Fertility = PercentValue.Zero;

        [JsonPropertyName("water_source_ref")]
        public string WaterSourceRef = "";

        [JsonPropertyName("lease_ref")]
        public string LeaseRef = "";

        [JsonPropertyName("owner_ref")]
        public string OwnerRef = "";

        [JsonPropertyName("risk")]
        public PercentValue Risk = PercentValue.Zero;
    }

    public sealed class BuildingEntity : V3EntityBase
    {
        [JsonPropertyName("function")]
        public BuildingFunction Function = BuildingFunction.Unknown;

        [JsonPropertyName("level")]
        public int Level;

        [JsonPropertyName("inventory")]
        public List<ItemHolding> Inventory = new List<ItemHolding>();

        [JsonPropertyName("worker_ids")]
        public List<string> WorkerIds = new List<string>();

        [JsonPropertyName("damage")]
        public PercentValue Damage = PercentValue.Zero;

        [JsonPropertyName("open_state")]
        public BuildingOpenState OpenState = BuildingOpenState.Unknown;

        [JsonPropertyName("ownership_ref")]
        public string OwnershipRef = "";
    }

    public sealed class ItemEntity : V3EntityBase
    {
        [JsonPropertyName("category")]
        public ItemCategory Category = ItemCategory.Unknown;

        [JsonPropertyName("tags")]
        public List<string> Tags = new List<string>();

        [JsonPropertyName("description")]
        public string Description = "";

        [JsonPropertyName("gameplay_role")]
        public string GameplayRole = "";

        [JsonPropertyName("dependencies")]
        public List<string> Dependencies = new List<string>();

        [JsonPropertyName("asset_refs")]
        public List<string> AssetRefs = new List<string>();

        [JsonPropertyName("source_refs")]
        public List<string> SourceRefs = new List<string>();

        [JsonPropertyName("license")]
        public string License = "";

        [JsonPropertyName("status")]
        public ItemStatus Status = ItemStatus.Unknown;

        [JsonPropertyName("unit_weight")]
        public WeightAmount UnitWeight = WeightAmount.Zero;

        [JsonPropertyName("unit_volume")]
        public VolumeAmount UnitVolume = VolumeAmount.Zero;

        [JsonPropertyName("default_durability")]
        public PercentValue DefaultDurability = PercentValue.Full;
    }

    public sealed class AnimalEntity : V3EntityBase
    {
        [JsonPropertyName("species_id")]
        public string SpeciesId = "";

        [JsonPropertyName("age_days")]
        public WorldDay AgeDays = WorldDay.Zero;

        [JsonPropertyName("sex")]
        public AnimalSex Sex = AnimalSex.Unknown;

        [JsonPropertyName("variant_id")]
        public string VariantId = "";

        [JsonPropertyName("status")]
        public AnimalStatus Status = AnimalStatus.Unknown;

        [JsonPropertyName("habitat_id")]
        public string HabitatId = "";

        [JsonPropertyName("behavior")]
        public AnimalBehavior Behavior = AnimalBehavior.Unknown;

        [JsonPropertyName("health")]
        public PercentValue Health = PercentValue.Full;

        [JsonPropertyName("legal_status")]
        public LegalStatus LegalStatus = LegalStatus.Unknown;

        [JsonPropertyName("ecological_pressure")]
        public PercentValue EcologicalPressure = PercentValue.Zero;
    }

    public sealed class FishSchoolEntity : V3EntityBase
    {
        [JsonPropertyName("fish_species")]
        public string FishSpecies = "";

        [JsonPropertyName("quantity")]
        public int Quantity;

        [JsonPropertyName("depth_meters")]
        public decimal DepthMeters;

        [JsonPropertyName("migration")]
        public FishMigration Migration = FishMigration.Unknown;

        [JsonPropertyName("fishing_pressure")]
        public PercentValue FishingPressure = PercentValue.Zero;

        [JsonPropertyName("recovery")]
        public PercentValue Recovery = PercentValue.Zero;
    }

    public sealed class PetEntity : V3EntityBase
    {
        [JsonPropertyName("breed")]
        public string Breed = "";

        [JsonPropertyName("health")]
        public PercentValue Health = PercentValue.Full;

        [JsonPropertyName("hunger")]
        public PercentValue Hunger = PercentValue.Zero;

        [JsonPropertyName("cleanliness")]
        public PercentValue Cleanliness = PercentValue.Full;

        [JsonPropertyName("attachment")]
        public PercentValue Attachment = PercentValue.Zero;

        [JsonPropertyName("location")]
        public LocationRefV3 Location = new LocationRefV3();

        [JsonPropertyName("care_state")]
        public PetCareState CareState = PetCareState.Unknown;
    }

    public sealed class FinanceAccountEntity : V3EntityBase
    {
        [JsonPropertyName("account_type")]
        public FinanceAccountType AccountType = FinanceAccountType.Unknown;

        [JsonPropertyName("owner_ref")]
        public string OwnerRef = "";

        [JsonPropertyName("principal")]
        public MoneyAmount Principal = MoneyAmount.Zero;

        [JsonPropertyName("interest")]
        public MoneyAmount Interest = MoneyAmount.Zero;

        [JsonPropertyName("guarantee")]
        public MoneyAmount Guarantee = MoneyAmount.Zero;

        [JsonPropertyName("mortgage")]
        public MoneyAmount Mortgage = MoneyAmount.Zero;

        [JsonPropertyName("due_day")]
        public WorldDay DueDay = WorldDay.Zero;

        [JsonPropertyName("default_state")]
        public FinanceDefaultState DefaultState = FinanceDefaultState.Unknown;

        [JsonPropertyName("ledger_refs")]
        public List<string> LedgerRefs = new List<string>();
    }

    public sealed class QuestEntity : V3EntityBase
    {
        [JsonPropertyName("status")]
        public QuestStatus Status = QuestStatus.Unknown;

        [JsonPropertyName("objective_ids")]
        public List<string> ObjectiveIds = new List<string>();

        [JsonPropertyName("prerequisite_ids")]
        public List<string> PrerequisiteIds = new List<string>();

        [JsonPropertyName("choice_ids")]
        public List<string> ChoiceIds = new List<string>();

        [JsonPropertyName("failure_state")]
        public string FailureState = "";

        [JsonPropertyName("recovery_route")]
        public string RecoveryRoute = "";

        [JsonPropertyName("reward_request_ids")]
        public List<string> RewardRequestIds = new List<string>();

        [JsonPropertyName("fact_event_ids")]
        public List<string> FactEventIds = new List<string>();
    }

    public sealed class WorldEventEntity : V3EntityBase
    {
        [JsonPropertyName("event_id")]
        public string EventId = "";

        [JsonPropertyName("world_day")]
        public WorldDay WorldDay = WorldDay.Zero;

        [JsonPropertyName("actor_ids")]
        public List<string> ActorIds = new List<string>();

        [JsonPropertyName("input_facts")]
        public Dictionary<string, string> InputFacts = new Dictionary<string, string>();

        [JsonPropertyName("result")]
        public Dictionary<string, string> Result = new Dictionary<string, string>();

        [JsonPropertyName("cause_ids")]
        public List<string> CauseIds = new List<string>();

        [JsonPropertyName("outcome_status")]
        public EventOutcomeStatus OutcomeStatus = EventOutcomeStatus.Unknown;

        [JsonPropertyName("recovery_route")]
        public string RecoveryRoute = "";

        [JsonPropertyName("save_revision")]
        public long SaveRevision;

        [JsonPropertyName("duplicate_key")]
        public string DuplicateKey = "";
    }

    public sealed class RewardEntity : V3EntityBase
    {
        [JsonPropertyName("reward_id")]
        public string RewardId = "";

        [JsonPropertyName("source")]
        public string Source = "";

        [JsonPropertyName("item_id")]
        public string ItemId = "";

        [JsonPropertyName("quantity")]
        public int Quantity;

        [JsonPropertyName("money")]
        public MoneyAmount Money = MoneyAmount.Zero;

        [JsonPropertyName("unique")]
        public bool Unique;

        [JsonPropertyName("claim_record")]
        public List<string> ClaimRecord = new List<string>();

        [JsonPropertyName("status")]
        public RewardStatus Status = RewardStatus.Unknown;

        [JsonPropertyName("reclaim")]
        public string Reclaim = "";

        [JsonPropertyName("recovery_route")]
        public string RecoveryRoute = "";
    }

    public sealed class AppearanceLoadoutEntity : V3EntityBase
    {
        [JsonPropertyName("loadout_id")]
        public string LoadoutId = "";

        [JsonPropertyName("body")]
        public string Body = "";

        [JsonPropertyName("hair")]
        public string Hair = "";

        [JsonPropertyName("clothing")]
        public string Clothing = "";

        [JsonPropertyName("accessories")]
        public List<string> Accessories = new List<string>();

        [JsonPropertyName("equipment")]
        public List<string> Equipment = new List<string>();

        [JsonPropertyName("durability")]
        public PercentValue Durability = PercentValue.Full;

        [JsonPropertyName("source")]
        public string Source = "";
    }

    /// <summary>存档级版本元数据；不拥有任何实体数值。</summary>
    public sealed class V3SaveMetadata
    {
        [JsonPropertyName("save_version")]
        public int SaveVersion = V3ModelVersions.TargetSaveVersion;

        [JsonPropertyName("world_revision")]
        public long WorldRevision;

        [JsonPropertyName("last_event_id")]
        public string LastEventId = "";

        [JsonPropertyName("migration_flags")]
        public List<string> MigrationFlags = new List<string>();
    }

    /// <summary>
    /// AgentCore 的唯一 v3 JSON 入口。
    /// 默认忽略未知对象字段以允许旧存档向前读取；未知枚举字符串严格拒绝，
    /// 不静默降级到另一个玩法状态。缺失枚举字段才使用 Unknown 默认值。
    /// </summary>
    public static class V3Json
    {
        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = false,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
            return options;
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, CreateOptions());
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentException("json is required", nameof(json));
            return JsonSerializer.Deserialize<T>(json, CreateOptions());
        }

        public static bool TryDeserialize<T>(string json, out T value, out string error)
        {
            value = default(T);
            error = "";
            if (string.IsNullOrEmpty(json))
            {
                error = "json_required";
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<T>(json, CreateOptions());
                if (ReferenceEquals(value, null))
                {
                    error = "json_null";
                    return false;
                }
                return true;
            }
            catch (JsonException ex)
            {
                error = ex.Path == null
                    ? "enum_or_value_rejected"
                    : "enum_or_value_rejected:" + ex.Path;
                return false;
            }
        }
    }
}
