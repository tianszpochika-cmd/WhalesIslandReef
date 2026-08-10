using System;
using Whalebone.Game.Config;
using Whalebone.Systems.D9;

namespace Whalebone.Systems.D4
{
    public sealed class WildlifeResult
    {
        public bool Accepted;
        public bool Dangerous;
        public int Damage;
        public int CollectionValue;
        public string DiseaseId;
        public string Message;
    }

    /// <summary>动物栖息地与可预判行为状态机的唯一裁决者。</summary>
    public sealed partial class D4EcologySystem
    {
        private readonly EcologyState _state;

        public D4EcologySystem(EcologyState initial = null)
        {
            _state = initial == null ? new EcologyState() : initial.Clone();
            InitializeD11State();
        }

        public EcologyState Snapshot => _state.Clone();

        public void SeedHabitat(string habitatId, int day, int seed)
        {
            _state.HabitatId = string.IsNullOrEmpty(habitatId) ? "jungle" : habitatId;
            _state.Animals.Clear();
            foreach (var definition in D4AnimalCatalog.All)
            {
                if (definition.Habitat != _state.HabitatId && !(definition.Marine && _state.HabitatId == "open_sea")) continue;
                _state.Animals.Add(new AnimalState
                {
                    AnimalId = definition.Id,
                    HabitatId = _state.HabitatId,
                    Behavior = BehaviorFor(definition, day, seed),
                    Health = (int)NumericConfiguration.Default.Health.DefaultValue,
                    LastUpdatedDay = Math.Max(0, day),
                    CollectionValue = definition.CollectionValue,
                });
            }
            InitializeD11State(day, seed);
        }

        public void Tick(int day, int seed, bool bloodInWater)
        {
            foreach (var animal in _state.Animals)
            {
                if (animal.Lifecycle == D11AnimalLifecycleState.Dead ||
                    animal.Lifecycle == D11AnimalLifecycleState.Collected) continue;
                var definition = D4AnimalCatalog.Find(animal.AnimalId);
                if (definition == null) continue;
                animal.Behavior = BehaviorFor(definition, day, seed + _state.CollectionCount);
                if (bloodInWater && definition.Id == "shark") animal.Behavior = D4AnimalBehavior.Attacking;
                if (animal.Behavior == D4AnimalBehavior.Attacking)
                    animal.Health = (int)NumericConfiguration.ClampHealth(animal.Health - 1);
            }

            foreach (var school in _state.FishSchools)
                MoveFishSchool(school.FishSchoolId, day, seed + school.FishingPressure);
        }

        /// <summary>建立 D9 唯一近海鱼群；初始位置由 EcologySystem 统一写入。</summary>
        public FishSchoolState SeedFishSchool(string fishSchoolId, string fishingSpotId, int day, int seed)
        {
            if (string.IsNullOrEmpty(fishSchoolId)) fishSchoolId = "fish_mackerel_school";
            if (string.IsNullOrEmpty(fishingSpotId)) fishingSpotId = "nearshore_saint_marys";
            string species = fishSchoolId == "fish_mackerel_school" ? "mackerel" : fishSchoolId;
            return SeedFishSchool(fishSchoolId, species, fishingSpotId, 2f, "nearshore", 12, day, seed);
        }

        /// <summary>
        /// D11 捕捞目录的鱼群初始化入口。仍由 EcologySystem 写入鱼种、深度、容量和位置，
        /// 调用方只能提供内容引用与环境意图，不能直接写入运行时生物量。
        /// </summary>
        public FishSchoolState SeedFishSchool(string fishSchoolId, string fishSpeciesId,
            string fishingSpotId, float depthMeters, string migration, int capacity, int day, int seed)
        {
            if (string.IsNullOrEmpty(fishSchoolId)) fishSchoolId = "fish_mackerel_school";
            if (string.IsNullOrEmpty(fishSpeciesId)) fishSpeciesId = fishSchoolId == "fish_mackerel_school" ? "mackerel" : fishSchoolId;
            if (string.IsNullOrEmpty(fishingSpotId)) fishingSpotId = "nearshore_saint_marys";
            var existing = FindFishSchool(fishSchoolId);
            if (existing != null) return existing.Clone();

            var school = new FishSchoolState
            {
                FishSchoolId = fishSchoolId,
                FishSpeciesId = fishSpeciesId,
                FishingSpotId = fishingSpotId,
                Biomass = Math.Max(1, capacity),
                Capacity = Math.Max(1, capacity),
                Depth = Math.Max(0f, depthMeters),
                Migration = string.IsNullOrEmpty(migration) ? "nearshore" : migration,
                FishingPressure = 0,
                Recovery = 0f,
                RecoveryRatePerDay = 1,
                LastMovedDay = Math.Max(0, day),
            };
            PositionFor(school, day, seed);
            _state.FishSchools.Add(school);
            return school.Clone();
        }

        /// <summary>只允许 EcologySystem 推进鱼群位置，不由 UI/任务脚本写坐标。</summary>
        public bool MoveFishSchool(string fishSchoolId, int day, int seed)
        {
            var school = FindFishSchool(fishSchoolId);
            if (school == null) return false;
            PositionFor(school, day, seed + school.FishingPressure);
            return true;
        }

        public FishSchoolState FindFishSchoolSnapshot(string fishSchoolId)
        {
            var school = FindFishSchool(fishSchoolId);
            return school == null ? null : school.Clone();
        }

        /// <summary>
        /// 计算确定性鱼获预览但不改变生态状态。提交由 CommitFishCatch 完成，
        /// 让库存容量不足时不会先消耗鱼群生物量。
        /// </summary>
        public FishCatchPreview PreviewFishCatch(string fishSchoolId, string fishingSpotId,
            string fishItemId, int gearCapacity, int day, int seed)
        {
            var school = FindFishSchool(fishSchoolId);
            if (school == null)
                return RejectCatch("fish_school_missing", "目标鱼群不存在。");
            if (!string.Equals(school.FishingSpotId, fishingSpotId, StringComparison.Ordinal))
                return RejectCatch("wrong_fishing_spot", "鱼群不在当前捕捞点。");
            if (school.Biomass <= 0)
                return RejectCatch("fish_school_depleted", "这片鱼群暂时离开或已经枯竭。");
            if (gearCapacity <= 0)
                return RejectCatch("gear_invalid", "渔具无法承载鱼获。");

            int deterministic = Stable(fishSchoolId + ":" + fishingSpotId + ":" + day + ":" + seed);
            int quantity = 1 + deterministic % Math.Max(1, Math.Min(gearCapacity, 3));
            quantity = Math.Min(quantity, school.Biomass);
            return new FishCatchPreview
            {
                Accepted = quantity > 0,
                Code = quantity > 0 ? "preview" : "fish_school_depleted",
                Message = quantity > 0 ? "生态系统已计算本次鱼获。" : "鱼群没有可结算的生物量。",
                FishSchoolId = fishSchoolId,
                FishingSpotId = fishingSpotId,
                FishItemId = fishItemId,
                Quantity = quantity,
                Day = Math.Max(0, day),
            };
        }

        /// <summary>
        /// D11 捕捞预览。深度、时段、潮汐、渔具和保护区都在 EcologySystem 内确定性裁决，
        /// 只返回预览；只有 CommitFishCatch 才能改变鱼群生物量和捕捞压力。
        /// </summary>
        public FishCatchPreview PreviewFishHarvest(string fishSchoolId, string fishingSpotId,
            string fishItemId, int gearCapacity, int day, int seed, D9WeatherSeaSnapshot weather,
            string fishSpeciesId, string gearId, float depthMeters, string timeOfDayBucket,
            string tideBand, bool protectedArea, bool hasConservationPermit, string protectionRuleId)
        {
            var school = FindFishSchool(fishSchoolId);
            if (school == null) return RejectHarvest("fish_school_missing", "目标鱼群不存在。", protectionRuleId);
            if (!string.Equals(school.FishingSpotId, fishingSpotId, StringComparison.Ordinal))
                return RejectHarvest("wrong_fishing_spot", "鱼群不在当前捕捞点。", protectionRuleId);
            if (!string.IsNullOrEmpty(fishSpeciesId) && !string.Equals(school.FishSpeciesId,
                    fishSpeciesId, StringComparison.Ordinal))
                return RejectHarvest("fish_species_mismatch", "鱼群鱼种与捕捞目录不一致。", protectionRuleId);
            if (protectedArea && !hasConservationPermit)
                return RejectHarvest("protected_area_no_permit", "保护区没有有效采集许可。", protectionRuleId);
            if (school.Biomass <= 0) return RejectHarvest("fish_school_depleted", "这片鱼群暂时离开或已经枯竭。", protectionRuleId);
            if (gearCapacity <= 0) return RejectHarvest("gear_invalid", "渔具无法承载鱼获。", protectionRuleId);

            var conditions = ReadConditions(weather, day);
            float depthMultiplier = DepthSuitability(school.Depth, depthMeters);
            float timeMultiplier = TimeSuitability(school.Migration, timeOfDayBucket, school.FishSpeciesId);
            float tideMultiplier = TideSuitability(conditions.TidePhase, tideBand);
            float gearMultiplier = GearSuitability(gearId);
            int deterministic = Stable(fishSchoolId + ":" + fishingSpotId + ":" + day + ":" + seed + ":" + gearId);
            int baseQuantity = 1 + deterministic % Math.Max(1, Math.Min(gearCapacity, 5));
            float pressureMultiplier = Clamp(1f - school.FishingPressure / 12f, 0.25f, 1f);
            float adjusted = baseQuantity * pressureMultiplier * conditions.FishCatchMultiplier *
                depthMultiplier * timeMultiplier * tideMultiplier * gearMultiplier;
            int quantity = Math.Min(school.Biomass, Math.Max(1, (int)Math.Floor(adjusted)));
            int qualityScore = Math.Max(0, Math.Min(100, 50 + (int)Math.Round(
                (depthMultiplier + timeMultiplier + tideMultiplier + gearMultiplier - 4f) * 18f) +
                (int)Math.Round((conditions.FishCatchMultiplier - 1f) * 20f) - school.FishingPressure * 2));
            return new FishCatchPreview
            {
                Accepted = quantity > 0,
                Code = quantity > 0 ? "preview" : "fish_school_depleted",
                Message = quantity > 0 ? "生态系统已按鱼种、深度、时段、潮汐、渔具和压力计算鱼获。" : "鱼群没有可结算的生物量。",
                FishSchoolId = fishSchoolId,
                FishSpeciesId = school.FishSpeciesId,
                FishingSpotId = fishingSpotId,
                FishItemId = fishItemId,
                GearId = gearId,
                Quantity = quantity,
                Day = Math.Max(0, day),
                DepthMeters = Math.Max(0f, depthMeters),
                TimeOfDayBucket = CanonicalBucket(timeOfDayBucket, "day"),
                TideBand = CanonicalBucket(tideBand, "mid"),
                CatchMultiplier = conditions.FishCatchMultiplier,
                PressureMultiplier = pressureMultiplier,
                TideMultiplier = tideMultiplier,
                DepthMultiplier = depthMultiplier,
                TimeMultiplier = timeMultiplier,
                GearMultiplier = gearMultiplier,
                QualityScore = qualityScore,
                Quality = QualityFor(qualityScore),
                ProtectionRuleId = protectionRuleId ?? string.Empty,
                FishingPressureAfter = school.FishingPressure + quantity,
                EcologyRevision = _state.EcologyRevision,
            };
        }

        /// <summary>生态系统唯一的鱼群生物量/捕捞压力写入入口。</summary>
        public bool CommitFishCatch(FishCatchPreview preview)
        {
            if (preview == null || !preview.Accepted || preview.Quantity <= 0) return false;
            var school = FindFishSchool(preview.FishSchoolId);
            if (school == null || school.FishingSpotId != preview.FishingSpotId ||
                school.Biomass < preview.Quantity) return false;
            school.Biomass -= preview.Quantity;
            school.FishingPressure = Math.Max(0, school.FishingPressure + preview.Quantity);
            school.Capacity = Math.Max(1, school.Capacity <= 0 ? 12 : school.Capacity);
            school.Recovery = Math.Max(0f, school.Recovery - preview.Quantity * 0.25f);
            _state.EcologyRevision++;
            return true;
        }

        public WildlifeResult Observe(string animalId)
        {
            var animal = Find(animalId);
            if (animal == null) return new WildlifeResult { Message = "没有发现这种动物。" };
            animal.Observed = true;
            var definition = D4AnimalCatalog.Find(animalId);
            return new WildlifeResult { Accepted = true, CollectionValue = definition == null ? 0 : definition.CollectionValue, Message = "已记录动物观察。" };
        }

        public WildlifeResult Collect(string animalId)
        {
            var animal = Find(animalId);
            var definition = D4AnimalCatalog.Find(animalId);
            if (animal == null || definition == null) return new WildlifeResult { Message = "没有可收集的动物记录。" };
            if (!animal.Observed) return new WildlifeResult { Message = "请先观察并确认物种。" };
            if (definition.CollectionValue <= 0) return new WildlifeResult { Message = "这种动物只能观察，不能收集。" };
            if (_state.CollectionCount >= 3) return new WildlifeResult { Message = "本次地点的收藏额度已满。" };
            _state.CollectionCount++;
            return new WildlifeResult { Accepted = true, CollectionValue = definition.CollectionValue, Message = "已获得动物标本/收藏记录。" };
        }

        public WildlifeResult ResolveDanger(string animalId, int protection, int seed)
        {
            var definition = D4AnimalCatalog.Find(animalId);
            if (definition == null) return new WildlifeResult { Message = "未知动物。" };
            if (definition.Danger <= 0) return new WildlifeResult { Accepted = true, Message = "动物没有主动攻击。" };
            int dangerRoll = Stable(definition.Id + ":" + seed) % 100;
            if (dangerRoll + protection >= definition.Danger + 20)
                return new WildlifeResult { Accepted = true, Dangerous = true, Message = "装备和队形让你安全避开了危险。" };
            int damage = Math.Max(1, definition.Danger / 12);
            string disease = definition.Id == "mosquito_swarm" ? "malaria" : definition.Id == "venomous_snake" ? "dysentery" : null;
            return new WildlifeResult { Accepted = true, Dangerous = true, Damage = damage, DiseaseId = disease, Message = "遭遇危险动物，队伍受到伤害。" };
        }

        private AnimalState Find(string animalId)
        {
            if (animalId == null) return null;
            foreach (var animal in _state.Animals) if (animal.AnimalId == animalId) return animal;
            return null;
        }

        private FishSchoolState FindFishSchool(string fishSchoolId)
        {
            if (fishSchoolId == null) return null;
            foreach (var school in _state.FishSchools)
                if (school != null && school.FishSchoolId == fishSchoolId) return school;
            return null;
        }

        private static FishCatchPreview RejectCatch(string code, string message)
        {
            return new FishCatchPreview { Accepted = false, Code = code, Message = message };
        }

        private static FishCatchPreview RejectHarvest(string code, string message, string protectionRuleId)
        {
            return new FishCatchPreview
            {
                Accepted = false,
                Code = code,
                Message = message,
                ProtectionRuleId = protectionRuleId ?? string.Empty,
            };
        }

        private static float DepthSuitability(float preferredDepth, float requestedDepth)
        {
            float difference = Math.Abs(Math.Max(0f, preferredDepth) - Math.Max(0f, requestedDepth));
            return Clamp(1f - difference / Math.Max(1f, preferredDepth + 1f), 0.25f, 1f);
        }

        private static float TimeSuitability(string migration, string bucket, string species)
        {
            string value = CanonicalBucket(bucket, "day");
            string movement = (migration ?? string.Empty).ToLowerInvariant();
            if (species == "sardine" && value == "dusk") return 1.25f;
            if (species == "snapper" && value == "dawn") return 1.25f;
            if (movement == "intertidal" && (value == "dawn" || value == "dusk")) return 1.15f;
            return value == "night" ? 0.80f : 1.0f;
        }

        private static float TideSuitability(float tidePhase, string tideBand)
        {
            string band = CanonicalBucket(tideBand, "mid");
            float phase = tidePhase - (float)Math.Floor(tidePhase);
            float target = band == "flood" ? 0.25f : band == "ebb" ? 0.75f : 0.50f;
            return Clamp(1.20f - Math.Abs(phase - target) * 0.80f, 0.65f, 1.20f);
        }

        private static float GearSuitability(string gearId)
        {
            if (gearId == "gear_cast_net") return 1.20f;
            if (gearId == "gear_fish_trap") return 1.05f;
            if (gearId == "gear_shore_rod") return 1.10f;
            return 1.0f;
        }

        private static QualityGrade QualityFor(int score)
        {
            if (score >= 90) return QualityGrade.Masterwork;
            if (score >= 75) return QualityGrade.Excellent;
            if (score >= 60) return QualityGrade.Good;
            if (score >= 35) return QualityGrade.Standard;
            return QualityGrade.Poor;
        }

        private static string CanonicalBucket(string value, string fallback)
        {
            string result = (value ?? string.Empty).Trim().ToLowerInvariant();
            return result == "dawn" || result == "day" || result == "dusk" || result == "night" ||
                result == "flood" || result == "ebb" || result == "mid" ? result : fallback;
        }

        private static void PositionFor(FishSchoolState school, int day, int seed)
        {
            int x = Stable(school.FishSchoolId + ":x:" + day + ":" + seed) % 500;
            int z = Stable(school.FishSchoolId + ":z:" + day + ":" + seed) % 500;
            school.WorldX = -8f + x / 100f;
            school.WorldZ = 5f + z / 100f;
            school.LastMovedDay = Math.Max(0, day);
        }

        private static D4AnimalBehavior BehaviorFor(AnimalDefinition definition, int day, int seed)
        {
            int roll = Stable(definition.Id + ":" + day + ":" + seed) % 100;
            if (definition.Danger >= 60 && roll > 72) return D4AnimalBehavior.Attacking;
            if (definition.Rare && roll > 82) return D4AnimalBehavior.BreedingHint;
            if (roll > 55) return D4AnimalBehavior.Alert;
            return D4AnimalBehavior.Foraging;
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
