using System;
using System.Collections.Generic;

namespace Whalebone.Game.Config
{
    /// <summary>统一配置中的计量单位。数值字段不负责领域结算。</summary>
    public enum ConfigurationUnit
    {
        None,
        SilverCoin,
        MetricTonne,
        CubicMeter,
        Meter,
        WorldDay,
        Second,
        NormalizedDay,
        RelationPoint,
        Percent
    }

    public enum ConfigurationValueType
    {
        Decimal,
        Integer,
        Enum
    }

    /// <summary>天气状态 canonical 枚举；D3WeatherType 只作为 D7 兼容投影。</summary>
    public enum WeatherState
    {
        Unknown = 0,
        Calm = 1,
        TradeWind = 2,
        Rain = 3,
        Storm = 4,
        Hurricane = 5
    }

    /// <summary>内容稀有度 canonical 枚举；不携带价格或奖励强度。</summary>
    public enum Rarity
    {
        Unknown = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        VeryRare = 4,
        Unique = 5
    }

    /// <summary>法律状态 canonical 枚举；合法性由 Faction/Legal 系统裁决。</summary>
    public enum LegalStatus
    {
        Unknown = 0,
        Legal = 1,
        Licensed = 2,
        Restricted = 3,
        Illegal = 4
    }

    /// <summary>物品、捕获物和收藏记录的品质枚举；不直接映射经济奖励。</summary>
    public enum QualityGrade
    {
        Unknown = 0,
        Poor = 1,
        Standard = 2,
        Good = 3,
        Excellent = 4,
        Masterwork = 5
    }

    /// <summary>只允许非 NaN 的 decimal 配置范围，领域系统仍拥有实际状态。</summary>
    public sealed class NumericRange
    {
        public decimal DefaultValue { get; private set; }
        public decimal Minimum { get; private set; }
        public decimal Maximum { get; private set; }

        public NumericRange(decimal defaultValue, decimal minimum, decimal maximum)
        {
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
        }

        public static NumericRange From(NumericParameterDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return new NumericRange(definition.DefaultValue, definition.Minimum, definition.Maximum);
        }
    }

    public sealed class NumericParameterDefinition
    {
        public string Id { get; private set; }
        public ConfigurationValueType ValueType { get; private set; }
        public ConfigurationUnit Unit { get; private set; }
        public decimal DefaultValue { get; private set; }
        public decimal Minimum { get; private set; }
        public decimal Maximum { get; private set; }
        public string OwnerSystem { get; private set; }
        public string Source { get; private set; }
        public string TestEntry { get; private set; }

        public NumericParameterDefinition(string id, ConfigurationUnit unit, decimal defaultValue,
            decimal minimum, decimal maximum, string ownerSystem, string source, string testEntry,
            ConfigurationValueType valueType = ConfigurationValueType.Decimal)
        {
            Id = id ?? string.Empty;
            ValueType = valueType;
            Unit = unit;
            DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
            OwnerSystem = ownerSystem ?? string.Empty;
            Source = source ?? string.Empty;
            TestEntry = testEntry ?? string.Empty;
        }
    }

    public sealed class EnumParameterDefinition
    {
        public string Id { get; private set; }
        public string DefaultValue { get; private set; }
        public IReadOnlyList<string> AllowedValues { get; private set; }
        public string OwnerSystem { get; private set; }
        public string Source { get; private set; }
        public string TestEntry { get; private set; }

        public EnumParameterDefinition(string id, string defaultValue, string[] allowedValues,
            string ownerSystem, string source, string testEntry)
        {
            Id = id ?? string.Empty;
            DefaultValue = defaultValue ?? string.Empty;
            AllowedValues = allowedValues ?? new string[0];
            OwnerSystem = ownerSystem ?? string.Empty;
            Source = source ?? string.Empty;
            TestEntry = testEntry ?? string.Empty;
        }
    }

    /// <summary>可被存档/测试替换的配置快照；默认入口本身只读。</summary>
    public sealed class NumericConfigurationProfile
    {
        public NumericRange MoneyAmount { get; private set; }
        public NumericRange StartingMoney { get; private set; }
        public NumericRange CashBalance { get; private set; }
        public NumericRange Weight { get; private set; }
        public NumericRange Volume { get; private set; }
        public NumericRange ItemUnitVolume { get; private set; }
        public NumericRange Distance { get; private set; }
        public NumericRange CombatDistance { get; private set; }
        public NumericRange BoardingDistance { get; private set; }
        public NumericRange WorldDay { get; private set; }
        public NumericRange SecondsPerWorldDay { get; private set; }
        public NumericRange TimeOfDay { get; private set; }
        public NumericRange Relation { get; private set; }
        public NumericRange RelationProposalDelta { get; private set; }
        public NumericRange MutinyAffinityThreshold { get; private set; }
        public NumericRange Health { get; private set; }
        public NumericRange Fatigue { get; private set; }
        public NumericRange Morale { get; private set; }
        public NumericRange Loyalty { get; private set; }
        public NumericRange WantedLevel { get; private set; }
        public WeatherState DefaultWeather { get; private set; }
        public Rarity DefaultRarity { get; private set; }
        public LegalStatus DefaultLegalStatus { get; private set; }
        public QualityGrade DefaultQuality { get; private set; }

        public NumericConfigurationProfile(
            NumericRange moneyAmount,
            NumericRange startingMoney,
            NumericRange cashBalance,
            NumericRange weight,
            NumericRange volume,
            NumericRange itemUnitVolume,
            NumericRange distance,
            NumericRange combatDistance,
            NumericRange boardingDistance,
            NumericRange worldDay,
            NumericRange secondsPerWorldDay,
            NumericRange timeOfDay,
            NumericRange relation,
            NumericRange relationProposalDelta,
            NumericRange mutinyAffinityThreshold,
            NumericRange health,
            NumericRange fatigue,
            NumericRange morale,
            NumericRange loyalty,
            NumericRange wantedLevel,
            WeatherState defaultWeather,
            Rarity defaultRarity,
            LegalStatus defaultLegalStatus,
            QualityGrade defaultQuality)
        {
            MoneyAmount = moneyAmount;
            StartingMoney = startingMoney;
            CashBalance = cashBalance;
            Weight = weight;
            Volume = volume;
            ItemUnitVolume = itemUnitVolume;
            Distance = distance;
            CombatDistance = combatDistance;
            BoardingDistance = boardingDistance;
            WorldDay = worldDay;
            SecondsPerWorldDay = secondsPerWorldDay;
            TimeOfDay = timeOfDay;
            Relation = relation;
            RelationProposalDelta = relationProposalDelta;
            MutinyAffinityThreshold = mutinyAffinityThreshold;
            Health = health;
            Fatigue = fatigue;
            Morale = morale;
            Loyalty = loyalty;
            WantedLevel = wantedLevel;
            DefaultWeather = defaultWeather;
            DefaultRarity = defaultRarity;
            DefaultLegalStatus = defaultLegalStatus;
            DefaultQuality = defaultQuality;
        }

        /// <summary>只替换测试指定的字段，用于配置加载器和非法配置测试。</summary>
        public NumericConfigurationProfile With(
            NumericRange moneyAmount = null,
            NumericRange startingMoney = null,
            NumericRange cashBalance = null,
            NumericRange weight = null,
            NumericRange volume = null,
            NumericRange itemUnitVolume = null,
            NumericRange distance = null,
            NumericRange combatDistance = null,
            NumericRange boardingDistance = null,
            NumericRange worldDay = null,
            NumericRange secondsPerWorldDay = null,
            NumericRange timeOfDay = null,
            NumericRange relation = null,
            NumericRange relationProposalDelta = null,
            NumericRange mutinyAffinityThreshold = null,
            NumericRange health = null,
            NumericRange fatigue = null,
            NumericRange morale = null,
            NumericRange loyalty = null,
            NumericRange wantedLevel = null,
            WeatherState? defaultWeather = null,
            Rarity? defaultRarity = null,
            LegalStatus? defaultLegalStatus = null,
            QualityGrade? defaultQuality = null)
        {
            return new NumericConfigurationProfile(
                moneyAmount ?? MoneyAmount,
                startingMoney ?? StartingMoney,
                cashBalance ?? CashBalance,
                weight ?? Weight,
                volume ?? Volume,
                itemUnitVolume ?? ItemUnitVolume,
                distance ?? Distance,
                combatDistance ?? CombatDistance,
                boardingDistance ?? BoardingDistance,
                worldDay ?? WorldDay,
                secondsPerWorldDay ?? SecondsPerWorldDay,
                timeOfDay ?? TimeOfDay,
                relation ?? Relation,
                relationProposalDelta ?? RelationProposalDelta,
                mutinyAffinityThreshold ?? MutinyAffinityThreshold,
                health ?? Health,
                fatigue ?? Fatigue,
                morale ?? Morale,
                loyalty ?? Loyalty,
                wantedLevel ?? WantedLevel,
                defaultWeather ?? DefaultWeather,
                defaultRarity ?? DefaultRarity,
                defaultLegalStatus ?? DefaultLegalStatus,
                defaultQuality ?? DefaultQuality);
        }
    }

    public sealed class ConfigurationIssue
    {
        public string ParameterId { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }

        public ConfigurationIssue(string parameterId, string code, string message)
        {
            ParameterId = parameterId ?? string.Empty;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public override string ToString()
        {
            return ParameterId + ":" + Code + ":" + Message;
        }
    }

    /// <summary>
    /// D8-A06 唯一配置入口。定义值是 D7 当前行为的登记值；后续调参只能通过领域 owner 的配置变更审查。
    /// </summary>
    public static class NumericConfiguration
    {
        public const int SchemaVersion = 1;

        public static readonly NumericParameterDefinition MoneyAmount = Numeric(
            "money_amount", ConfigurationUnit.SilverCoin, 0m, -1000000000m, 1000000000m,
            "Economy/Finance", "DATA_MODELS §2.1；D8 freeze §4", "D8A06_Config_Money", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition StartingMoney = Numeric(
            "starting_money", ConfigurationUnit.SilverCoin, 500m, 0m, 1000000000m,
            "Economy", "D7 PlayerState.StartingMoney；D8 freeze §3", "D8A06_Config_StartingMoney", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition CashBalance = Numeric(
            "cash_balance", ConfigurationUnit.SilverCoin, 0m, 0m, 1000000000m,
            "Economy/Finance", "D3 D3TradeState.Money；D8 freeze §7 money mirror", "D8A06_Config_CashBalance", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition Weight = Numeric(
            "weight", ConfigurationUnit.MetricTonne, 0m, 0m, 100000m,
            "Inventory/Cargo", "DATA_MODELS §2.6 CatchRecord；D8-A01 WeightAmount", "D8A06_Config_Weight");
        public static readonly NumericParameterDefinition Volume = Numeric(
            "volume", ConfigurationUnit.CubicMeter, 0m, 0m, 100000m,
            "Inventory/Cargo", "DATA_MODELS §2.2/§2.6；D8-A01 VolumeAmount", "D8A06_Config_Volume");
        public static readonly NumericParameterDefinition ItemUnitVolume = Numeric(
            "item_unit_volume", ConfigurationUnit.CubicMeter, 1m, 1m, 100000m,
            "ItemCatalog/Inventory", "D7 D3 CommodityDefinition.cargoVolume", "D8A06_Config_ItemVolume", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition Distance = Numeric(
            "distance", ConfigurationUnit.Meter, 0m, 0m, 1000000m,
            "Environment/Ship", "DATA_MODELS §2.2；D8-A01 location/distance types", "D8A06_Config_Distance");
        public static readonly NumericParameterDefinition CombatDistance = Numeric(
            "combat_distance", ConfigurationUnit.Meter, 35m, 10m, 100m,
            "Combat", "D4CombatSystem.BeginEncounter/Fire", "D8A06_Config_CombatDistance");
        public static readonly NumericParameterDefinition BoardingDistance = Numeric(
            "boarding_distance", ConfigurationUnit.Meter, 28m, 0m, 100m,
            "Combat", "D4CombatSystem.Board", "D8A06_Config_BoardingDistance");
        public static readonly NumericParameterDefinition WorldDay = Numeric(
            "world_day", ConfigurationUnit.WorldDay, 0m, 0m, 1000000m,
            "Environment/Time", "DATA_MODELS §1；D8 freeze §2.1", "D8A06_Config_WorldDay", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition SecondsPerWorldDay = Numeric(
            "seconds_per_world_day", ConfigurationUnit.Second, 600m, 1m, 86400m,
            "Environment/Time", "D2 DayNightSystem/DayNightController", "D8A06_Config_DayLength");
        public static readonly NumericParameterDefinition TimeOfDay = Numeric(
            "time_of_day", ConfigurationUnit.NormalizedDay, 0.25m, 0m, 1m,
            "Environment/Time", "D2Contracts.DayNightSnapshot；D8 freeze §2.1", "D8A06_Config_TimeOfDay");
        public static readonly NumericParameterDefinition Relation = Numeric(
            "relation_value", ConfigurationUnit.RelationPoint, 0m, -100m, 100m,
            "Character/Crew", "DATA_MODELS §2.3/§2.4；D2 AgentModels", "D8A06_Config_Relation");
        public static readonly NumericParameterDefinition RelationProposalDelta = Numeric(
            "relation_proposal_delta", ConfigurationUnit.RelationPoint, 0m, -10m, 10m,
            "Character/Crew (proposal boundary)", "D2 AgentReplyValidator/CrewRelationSystem", "D8A06_Config_RelationProposal", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MutinyAffinityThreshold = Numeric(
            "mutiny_affinity_threshold", ConfigurationUnit.RelationPoint, -50m, -100m, 100m,
            "Character/Crew", "D2 MutinyMonitor branch lock", "D8A06_Config_MutinyThreshold");
        public static readonly NumericParameterDefinition Health = Numeric(
            "health_percent", ConfigurationUnit.Percent, 100m, 0m, 100m,
            "Character/Crew; Ecology reads range", "DATA_MODELS §2.3/§2.6；D4 health projection", "D8A06_Config_Health", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition Fatigue = Numeric(
            "fatigue_percent", ConfigurationUnit.Percent, 0m, 0m, 100m,
            "Character/Crew", "DATA_MODELS §2.3；D7 CrewStats", "D8A06_Config_Fatigue", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition Morale = Numeric(
            "morale_percent", ConfigurationUnit.Percent, 60m, 0m, 100m,
            "Character/Crew", "D7 CrewStats", "D8A06_Config_Morale", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition Loyalty = Numeric(
            "loyalty_percent", ConfigurationUnit.Percent, 60m, 0m, 100m,
            "Character/Crew", "D7 CrewStats", "D8A06_Config_Loyalty", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition WantedLevel = Numeric(
            "wanted_level_percent", ConfigurationUnit.Percent, 0m, 0m, 100m,
            "Faction/Legal", "D4 FactionState.WantedLevel", "D8A06_Config_WantedLevel", ConfigurationValueType.Integer);

        public static readonly EnumParameterDefinition Weather = Enum(
            "weather_state", "Calm", new[] { "Unknown", "Calm", "TradeWind", "Rain", "Storm", "Hurricane" },
            "Environment", "D3WeatherType compatibility；REQUIREMENTS REQ-WORLD", "D8A06_Config_Weather");
        public static readonly EnumParameterDefinition Rarity = Enum(
            "rarity", "Common", new[] { "Unknown", "Common", "Uncommon", "Rare", "VeryRare", "Unique" },
            "Item/Ecology/Collection", "DATA_MODELS §2.6/§2.7；D4 Rare compatibility", "D8A06_Config_Rarity");
        public static readonly EnumParameterDefinition Legal = Enum(
            "legal_status", "Legal", new[] { "Unknown", "Legal", "Licensed", "Restricted", "Illegal" },
            "Faction/Legal", "DESIGN_BOUNDARIES §4；D8-A01 LegalStatus compatibility", "D8A06_Config_Legal");
        public static readonly EnumParameterDefinition Quality = Enum(
            "quality_grade", "Standard", new[] { "Unknown", "Poor", "Standard", "Good", "Excellent", "Masterwork" },
            "Inventory/Ecology/Collection", "DATA_MODELS §2.6 CatchRecord.quality", "D8A06_Config_Quality");

        public static readonly IReadOnlyList<NumericParameterDefinition> NumericParameters = new[] 
        {
            MoneyAmount, StartingMoney, CashBalance, Weight, Volume, ItemUnitVolume, Distance, CombatDistance,
            BoardingDistance, WorldDay, SecondsPerWorldDay, TimeOfDay, Relation, RelationProposalDelta,
            MutinyAffinityThreshold, Health, Fatigue, Morale, Loyalty, WantedLevel
        };

        public static readonly IReadOnlyList<EnumParameterDefinition> EnumParameters = new[] 
        {
            Weather, Rarity, Legal, Quality
        };

        public static readonly NumericConfigurationProfile Default = CreateDefault();

        static NumericConfiguration()
        {
            ThrowIfInvalid(Default);
        }

        public static IReadOnlyList<ConfigurationIssue> Validate(NumericConfigurationProfile profile)
        {
            var issues = new List<ConfigurationIssue>();
            if (profile == null)
            {
                issues.Add(new ConfigurationIssue("profile", "null", "配置快照不能为空。"));
                return issues;
            }

            ValidateRange(issues, MoneyAmount, profile.MoneyAmount);
            ValidateRange(issues, StartingMoney, profile.StartingMoney);
            ValidateRange(issues, CashBalance, profile.CashBalance);
            ValidateRange(issues, Weight, profile.Weight);
            ValidateRange(issues, Volume, profile.Volume);
            ValidateRange(issues, ItemUnitVolume, profile.ItemUnitVolume);
            ValidateRange(issues, Distance, profile.Distance);
            ValidateRange(issues, CombatDistance, profile.CombatDistance);
            ValidateRange(issues, BoardingDistance, profile.BoardingDistance);
            ValidateRange(issues, WorldDay, profile.WorldDay);
            ValidateRange(issues, SecondsPerWorldDay, profile.SecondsPerWorldDay);
            ValidateRange(issues, TimeOfDay, profile.TimeOfDay);
            ValidateRange(issues, Relation, profile.Relation);
            ValidateRange(issues, RelationProposalDelta, profile.RelationProposalDelta);
            ValidateRange(issues, MutinyAffinityThreshold, profile.MutinyAffinityThreshold);
            ValidateRange(issues, Health, profile.Health);
            ValidateRange(issues, Fatigue, profile.Fatigue);
            ValidateRange(issues, Morale, profile.Morale);
            ValidateRange(issues, Loyalty, profile.Loyalty);
            ValidateRange(issues, WantedLevel, profile.WantedLevel);

            ValidateEnum(issues, Weather, profile.DefaultWeather.ToString());
            ValidateEnum(issues, Rarity, profile.DefaultRarity.ToString());
            ValidateEnum(issues, Legal, profile.DefaultLegalStatus.ToString());
            ValidateEnum(issues, Quality, profile.DefaultQuality.ToString());
            return issues;
        }

        public static void ThrowIfInvalid(NumericConfigurationProfile profile)
        {
            var issues = Validate(profile);
            if (issues.Count == 0) return;
            throw new InvalidOperationException("非法统一配置：" + string.Join("; ", issues));
        }

        public static float ClampRelation(float value)
        {
            return ClampFloat(value, Default.Relation);
        }

        public static int ClampRelation(int value)
        {
            return ClampInteger(value, Default.Relation);
        }

        public static long ClampMoney(long value)
        {
            return (long)Math.Max(Default.MoneyAmount.Minimum,
                Math.Min(Default.MoneyAmount.Maximum, value));
        }

        public static int ClampWantedLevel(int value)
        {
            return ClampInteger(value, Default.WantedLevel);
        }

        public static decimal ClampWeight(decimal value)
        {
            return ClampDecimal(value, Default.Weight);
        }

        public static decimal ClampVolume(decimal value)
        {
            return ClampDecimal(value, Default.Volume);
        }

        public static int ClampWorldDay(int value)
        {
            return ClampInteger(value, Default.WorldDay);
        }

        public static decimal ClampTimeOfDay(decimal value)
        {
            return ClampDecimal(value, Default.TimeOfDay);
        }

        public static int ClampPercent(int value)
        {
            return ClampInteger(value, Default.Health);
        }

        public static int ClampCash(int value)
        {
            return ClampInteger(value, Default.CashBalance);
        }

        public static int ClampItemUnitVolume(int value)
        {
            return ClampInteger(value, Default.ItemUnitVolume);
        }

        public static float ClampRelationProposal(float value)
        {
            return ClampFloat(value, Default.RelationProposalDelta);
        }

        public static float ClampHealth(float value)
        {
            return ClampFloat(value, Default.Health);
        }

        public static float ClampFatigue(float value)
        {
            return ClampFloat(value, Default.Fatigue);
        }

        public static float ClampMorale(float value)
        {
            return ClampFloat(value, Default.Morale);
        }

        public static float ClampLoyalty(float value)
        {
            return ClampFloat(value, Default.Loyalty);
        }

        public static float ClampTimeOfDay(float value)
        {
            return ClampFloat(value, Default.TimeOfDay);
        }

        public static float ClampSecondsPerWorldDay(float value)
        {
            return ClampFloat(value, Default.SecondsPerWorldDay);
        }

        public static float ClampCombatDistance(float value)
        {
            return ClampFloat(value, Default.CombatDistance);
        }

        public static float ClampBoardingDistance(float value)
        {
            return ClampFloat(value, Default.BoardingDistance);
        }

        /// <summary>D3 的旧天气枚举没有 Unknown，转换只用于兼容快照，不改变天气状态定义。</summary>
        public static int LegacyD3WeatherIndex(WeatherState state)
        {
            int index = (int)state - 1;
            return Math.Max(0, Math.Min(4, index));
        }

        private static NumericConfigurationProfile CreateDefault()
        {
            return new NumericConfigurationProfile(
                NumericRange.From(MoneyAmount), NumericRange.From(StartingMoney), NumericRange.From(CashBalance),
                NumericRange.From(Weight),
                NumericRange.From(Volume), NumericRange.From(ItemUnitVolume), NumericRange.From(Distance),
                NumericRange.From(CombatDistance), NumericRange.From(BoardingDistance), NumericRange.From(WorldDay),
                NumericRange.From(SecondsPerWorldDay), NumericRange.From(TimeOfDay), NumericRange.From(Relation),
                NumericRange.From(RelationProposalDelta), NumericRange.From(MutinyAffinityThreshold),
                NumericRange.From(Health), NumericRange.From(Fatigue), NumericRange.From(Morale), NumericRange.From(Loyalty),
                NumericRange.From(WantedLevel),
                WeatherState.Calm, global::Whalebone.Game.Config.Rarity.Common,
                global::Whalebone.Game.Config.LegalStatus.Legal, QualityGrade.Standard);
        }

        private static NumericParameterDefinition Numeric(string id, ConfigurationUnit unit, decimal defaultValue,
            decimal minimum, decimal maximum, string owner, string source, string testEntry,
            ConfigurationValueType valueType = ConfigurationValueType.Decimal)
        {
            return new NumericParameterDefinition(id, unit, defaultValue, minimum, maximum, owner, source, testEntry, valueType);
        }

        private static EnumParameterDefinition Enum(string id, string defaultValue, string[] allowedValues,
            string owner, string source, string testEntry)
        {
            return new EnumParameterDefinition(id, defaultValue, allowedValues, owner, source, testEntry);
        }

        private static void ValidateRange(List<ConfigurationIssue> issues, NumericParameterDefinition definition,
            NumericRange range)
        {
            if (range == null)
            {
                issues.Add(new ConfigurationIssue(definition.Id, "null_range", "数值范围不能为空。"));
                return;
            }
            if (range.Minimum > range.Maximum)
                issues.Add(new ConfigurationIssue(definition.Id, "range_order", "最小值不能大于最大值。"));
            if (range.Minimum < definition.Minimum || range.Maximum > definition.Maximum)
                issues.Add(new ConfigurationIssue(definition.Id, "outside_schema", "配置边界超出 D8 安全边界。"));
            if (range.DefaultValue < range.Minimum || range.DefaultValue > range.Maximum)
                issues.Add(new ConfigurationIssue(definition.Id, "default_outside_range", "默认值必须位于配置范围内。"));
        }

        private static void ValidateEnum(List<ConfigurationIssue> issues, EnumParameterDefinition definition,
            string value)
        {
            bool allowed = false;
            foreach (var candidate in definition.AllowedValues)
                if (string.Equals(candidate, value, StringComparison.Ordinal)) { allowed = true; break; }
            if (!allowed)
                issues.Add(new ConfigurationIssue(definition.Id, "unknown_enum", "默认枚举值不在登记表内。"));
        }

        private static float ClampFloat(float value, NumericRange range)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return (float)range.DefaultValue;
            return Math.Max((float)range.Minimum, Math.Min((float)range.Maximum, value));
        }

        private static int ClampInteger(int value, NumericRange range)
        {
            return (int)Math.Max(range.Minimum, Math.Min(range.Maximum, value));
        }

        private static decimal ClampDecimal(decimal value, NumericRange range)
        {
            return Math.Max(range.Minimum, Math.Min(range.Maximum, value));
        }
    }
}
