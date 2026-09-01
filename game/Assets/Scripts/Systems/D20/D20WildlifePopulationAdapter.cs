// ============================================================
// D20WildlifePopulationAdapter —— D20-L24 野生动物/种群只读适配（纯 C#）
//
// D4EcologySystem 是生境、野生动物和鱼群数量/压力的唯一裁决者。本文件
// 只读取它返回的 EcologyState clone，把已有状态投影为 D20 可消费的
// 资格、风险桶和终止/枯竭状态；不推进、不钳制、不写回生态数值，也不
// 依赖 D20-L20/L21/L22 尚未落地的类型或资源存在性。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D4;

namespace Whalebone.Systems.D20
{
    /// <summary>野生动物读模型的来源类别。</summary>
    public enum D20WildlifePopulationSourceKind
    {
        Unknown = 0,
        Animal = 1,
        FishSchool = 2,
    }

    /// <summary>从 EcologyState 读取的种群状态桶，不是生态系统写入的数值。</summary>
    public enum D20WildlifePopulationStatus
    {
        Unknown = 0,
        Present = 1,
        Depleted = 2,
        Dead = 3,
        Collected = 4,
        Invalid = 5,
    }

    /// <summary>只读风险解释桶；风险桶不改变 EcologyState。</summary>
    public enum D20WildlifeRiskLevel
    {
        Unknown = 0,
        None = 1,
        Low = 2,
        Moderate = 3,
        High = 4,
        Critical = 5,
    }

    /// <summary>生境条件的 D20 只读投影。</summary>
    [Serializable]
    public sealed class D20HabitatReadModel
    {
        public string HabitatId = string.Empty;
        public string BiomeId = string.Empty;
        public float TemperatureC;
        public float Turbidity;
        public float Pollution;
        public float TidePhase;
        public float CurrentSpeed;
        public float CurrentDirectionDegrees;
        public int LastUpdatedDay;
        public long Revision;

        /// <summary>返回防御性副本。</summary>
        public D20HabitatReadModel Clone()
        {
            return new D20HabitatReadModel
            {
                HabitatId = HabitatId ?? string.Empty,
                BiomeId = BiomeId ?? string.Empty,
                TemperatureC = TemperatureC,
                Turbidity = Turbidity,
                Pollution = Pollution,
                TidePhase = TidePhase,
                CurrentSpeed = CurrentSpeed,
                CurrentDirectionDegrees = CurrentDirectionDegrees,
                LastUpdatedDay = LastUpdatedDay,
                Revision = Revision,
            };
        }
    }

    /// <summary>
    /// 单个野生动物或鱼群的只读资格/风险/状态投影。Biome 与数量字段
    /// 来自 EcologySystem 的公开 clone；D20 不拥有这些字段。
    /// </summary>
    [Serializable]
    public sealed class D20WildlifePopulationReadModel
    {
        public int SchemaVersion = D20WildlifePopulationAdapter.CurrentSchemaVersion;
        public bool InputAvailable;
        public bool ObservationEligible;
        public bool InteractionEligible;
        public D20WildlifePopulationSourceKind SourceKind;
        public D20WildlifePopulationStatus PopulationStatus;
        public D20WildlifeRiskLevel RiskLevel;
        public string EntityId = string.Empty;
        public string SpeciesId = string.Empty;
        public string HabitatId = string.Empty;
        public string LocationRef = string.Empty;
        public D4AnimalBehavior Behavior;
        public D11AnimalLifecycleState Lifecycle;
        public int Health;
        public int Biomass;
        public int Capacity;
        public int FishingPressure;
        public float EcologyStress;
        public float Pollution;
        public float Turbidity;
        public long EcologyRevision;
        public string SourceOwnerSystem = D20WildlifePopulationAdapter.EcologyOwnerSystem;
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;

        /// <summary>返回防御性副本，不把状态列表暴露给消费者。</summary>
        public D20WildlifePopulationReadModel Clone()
        {
            return new D20WildlifePopulationReadModel
            {
                SchemaVersion = SchemaVersion,
                InputAvailable = InputAvailable,
                ObservationEligible = ObservationEligible,
                InteractionEligible = InteractionEligible,
                SourceKind = SourceKind,
                PopulationStatus = PopulationStatus,
                RiskLevel = RiskLevel,
                EntityId = EntityId ?? string.Empty,
                SpeciesId = SpeciesId ?? string.Empty,
                HabitatId = HabitatId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                Behavior = Behavior,
                Lifecycle = Lifecycle,
                Health = Health,
                Biomass = Biomass,
                Capacity = Capacity,
                FishingPressure = FishingPressure,
                EcologyStress = EcologyStress,
                Pollution = Pollution,
                Turbidity = Turbidity,
                EcologyRevision = EcologyRevision,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackKey = FeedbackKey ?? string.Empty,
            };
        }
    }

    /// <summary>完整生境种群只读快照，来源仍是 D4EcologySystem。</summary>
    [Serializable]
    public sealed class D20WildlifePopulationSnapshot
    {
        public int SchemaVersion = D20WildlifePopulationAdapter.CurrentSchemaVersion;
        public bool InputAvailable;
        public string HabitatId = string.Empty;
        public int WorldDay;
        public long EcologyRevision;
        public D20HabitatReadModel Habitat = new D20HabitatReadModel();
        public List<D20WildlifePopulationReadModel> Records =
            new List<D20WildlifePopulationReadModel>();

        /// <summary>返回深拷贝。</summary>
        public D20WildlifePopulationSnapshot Clone()
        {
            var result = new D20WildlifePopulationSnapshot
            {
                SchemaVersion = SchemaVersion,
                InputAvailable = InputAvailable,
                HabitatId = HabitatId ?? string.Empty,
                WorldDay = WorldDay,
                EcologyRevision = EcologyRevision,
                Habitat = Habitat == null ? new D20HabitatReadModel() : Habitat.Clone(),
            };
            if (Records != null)
                foreach (var record in Records)
                    if (record != null) result.Records.Add(record.Clone());
            return result;
        }

        /// <summary>验证投影结构，不修改任何 EcologyState。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20WildlifePopulationAdapter.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 wildlife population schema_version mismatch.");
            if (WorldDay < 0 || EcologyRevision < 0)
                throw new InvalidOperationException("D20 wildlife population counters cannot be negative.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (Records == null) return;
            foreach (var record in Records)
            {
                if (record == null || string.IsNullOrEmpty(record.EntityId))
                    throw new InvalidOperationException("D20 wildlife population entity_id is required.");
                if (!ids.Add(record.EntityId))
                    throw new InvalidOperationException("D20 wildlife population entity_id is duplicated: " + record.EntityId);
            }
        }
    }

    /// <summary>
    /// D20-L24 生态只读适配器。它接受 D4EcologySystem 的公开 Snapshot，
    /// 不保留 EcologyState 别名，也没有推进/写入生态的操作。
    /// </summary>
    public sealed class D20WildlifePopulationAdapter
    {
        /// <summary>适配器投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>D20 适配器自身的 owner 标识。</summary>
        public const string OwnerSystem = "D20WildlifePopulationAdapter";

        /// <summary>生态事实的真实 owner。</summary>
        public const string EcologyOwnerSystem = "EcologySystem";

        private readonly D4EcologySystem _ecologySystem;

        /// <summary>创建只读适配器；null 表示公开生态输入尚不可用。</summary>
        public D20WildlifePopulationAdapter(D4EcologySystem ecologySystem = null)
        {
            _ecologySystem = ecologySystem;
        }

        /// <summary>返回 D4 公开生态快照的 clone，不暴露其内部引用。</summary>
        public EcologyState EcologySnapshot
        {
            get { return _ecologySystem == null ? null : _ecologySystem.Snapshot; }
        }

        /// <summary>构建当前生境的完整只读投影。</summary>
        public D20WildlifePopulationSnapshot Snapshot
        {
            get { return BuildSnapshot(EcologySnapshot); }
        }

        /// <summary>捕获当前只读投影。</summary>
        public D20WildlifePopulationSnapshot CaptureSnapshot()
        {
            return Snapshot;
        }

        /// <summary>按稳定 animal_id 读取生态动物状态。</summary>
        public D20WildlifePopulationReadModel ReadAnimal(string animalId)
        {
            return ReadAnimal(EcologySnapshot, animalId, string.Empty);
        }

        /// <summary>按 animal_id 和生境引用读取生态动物状态。</summary>
        public D20WildlifePopulationReadModel ReadAnimal(string animalId, string habitatId)
        {
            return ReadAnimal(EcologySnapshot, animalId, habitatId);
        }

        /// <summary>按稳定 fish_school_id 读取生态鱼群状态。</summary>
        public D20WildlifePopulationReadModel ReadFishSchool(string fishSchoolId)
        {
            return ReadFishSchool(EcologySnapshot, fishSchoolId, string.Empty);
        }

        /// <summary>按 fish_school_id 和生境引用读取生态鱼群状态。</summary>
        public D20WildlifePopulationReadModel ReadFishSchool(string fishSchoolId, string habitatId)
        {
            return ReadFishSchool(EcologySnapshot, fishSchoolId, habitatId);
        }

        /// <summary>读取动物或鱼群；同名实体按动物优先保持稳定。</summary>
        public D20WildlifePopulationReadModel Read(string entityId)
        {
            var animal = ReadAnimal(entityId);
            if (animal.InputAvailable)
                return animal;
            return ReadFishSchool(entityId);
        }

        /// <summary>按实体和生境读取动物或鱼群。</summary>
        public D20WildlifePopulationReadModel Read(string entityId, string habitatId)
        {
            var animal = ReadAnimal(EcologySnapshot, entityId, habitatId);
            if (animal.InputAvailable)
                return animal;
            return ReadFishSchool(EcologySnapshot, entityId, habitatId);
        }

        /// <summary>直接消费已由 EcologySystem clone 的公开快照。</summary>
        public static D20WildlifePopulationReadModel ReadAnimal(EcologyState snapshot,
            string animalId, string habitatId = "")
        {
            if (snapshot == null) return Missing("animal", animalId, "ecology_snapshot_missing");
            if (string.IsNullOrWhiteSpace(animalId)) return Missing("animal", animalId, "animal_id_missing");

            var expectedHabitat = Safe(habitatId);
            var actualHabitat = HabitatId(snapshot);
            if (!string.IsNullOrEmpty(expectedHabitat) &&
                !string.Equals(expectedHabitat, actualHabitat, StringComparison.Ordinal))
                return Missing("animal", animalId, "habitat_mismatch");

            if (snapshot.Animals == null)
                return Missing("animal", animalId, "animal_records_missing");
            foreach (var animal in snapshot.Animals)
            {
                if (animal == null || !string.Equals(animal.AnimalId, animalId,
                    StringComparison.Ordinal)) continue;
                return FromAnimal(snapshot, animal, actualHabitat);
            }
            return Missing("animal", animalId, "animal_not_found");
        }

        /// <summary>直接消费已由 EcologySystem clone 的公开鱼群快照。</summary>
        public static D20WildlifePopulationReadModel ReadFishSchool(EcologyState snapshot,
            string fishSchoolId, string habitatId = "")
        {
            if (snapshot == null) return Missing("fish_school", fishSchoolId, "ecology_snapshot_missing");
            if (string.IsNullOrWhiteSpace(fishSchoolId))
                return Missing("fish_school", fishSchoolId, "fish_school_id_missing");

            var expectedHabitat = Safe(habitatId);
            var actualHabitat = HabitatId(snapshot);
            if (!string.IsNullOrEmpty(expectedHabitat) &&
                !string.Equals(expectedHabitat, actualHabitat, StringComparison.Ordinal))
                return Missing("fish_school", fishSchoolId, "habitat_mismatch");

            if (snapshot.FishSchools == null)
                return Missing("fish_school", fishSchoolId, "fish_school_records_missing");
            foreach (var school in snapshot.FishSchools)
            {
                if (school == null || !string.Equals(school.FishSchoolId, fishSchoolId,
                    StringComparison.Ordinal)) continue;
                return FromFishSchool(snapshot, school, actualHabitat);
            }
            return Missing("fish_school", fishSchoolId, "fish_school_not_found");
        }

        /// <summary>用公开 EcologyState 构建完整生境投影。</summary>
        public static D20WildlifePopulationSnapshot BuildSnapshot(EcologyState snapshot)
        {
            var result = new D20WildlifePopulationSnapshot();
            if (snapshot == null) return result;

            result.InputAvailable = true;
            result.HabitatId = HabitatId(snapshot);
            result.WorldDay = Math.Max(0, snapshot.LastEcologyDay);
            result.EcologyRevision = Math.Max(0L, snapshot.EcologyRevision);
            result.Habitat = FromHabitat(snapshot);
            if (snapshot.Animals != null)
                foreach (var animal in snapshot.Animals)
                    if (animal != null)
                        result.Records.Add(FromAnimal(snapshot, animal, result.HabitatId));
            if (snapshot.FishSchools != null)
                foreach (var school in snapshot.FishSchools)
                    if (school != null)
                        result.Records.Add(FromFishSchool(snapshot, school, result.HabitatId));
            return result;
        }

        private static D20WildlifePopulationReadModel FromAnimal(EcologyState snapshot,
            AnimalState animal, string habitatId)
        {
            var result = Base(snapshot, D20WildlifePopulationSourceKind.Animal,
                animal.AnimalId, animal.AnimalId, habitatId);
            result.Behavior = animal.Behavior;
            result.Lifecycle = animal.Lifecycle;
            result.Health = animal.Health;
            result.EcologyStress = animal.EcologyStress;

            if (animal.Health < 0 || animal.Health > 100 ||
                float.IsNaN(animal.EcologyStress) || float.IsInfinity(animal.EcologyStress) ||
                animal.EcologyStress < 0f || animal.EcologyStress > 1f ||
                !Enum.IsDefined(typeof(D4AnimalBehavior), animal.Behavior) ||
                !Enum.IsDefined(typeof(D11AnimalLifecycleState), animal.Lifecycle))
            {
                result.PopulationStatus = D20WildlifePopulationStatus.Invalid;
                result.RiskLevel = D20WildlifeRiskLevel.Unknown;
                result.ReasonCode = "animal_state_invalid";
                result.FeedbackKey = "wildlife.feedback.unavailable";
                return result;
            }

            if (animal.Lifecycle == D11AnimalLifecycleState.Dead)
            {
                result.PopulationStatus = D20WildlifePopulationStatus.Dead;
                result.RiskLevel = D20WildlifeRiskLevel.None;
                result.ReasonCode = "animal_dead";
                result.FeedbackKey = "wildlife.feedback.terminal";
                return result;
            }
            if (animal.Lifecycle == D11AnimalLifecycleState.Collected)
            {
                result.PopulationStatus = D20WildlifePopulationStatus.Collected;
                result.RiskLevel = D20WildlifeRiskLevel.None;
                result.ReasonCode = "animal_collected";
                result.FeedbackKey = "wildlife.feedback.collected";
                return result;
            }

            result.PopulationStatus = D20WildlifePopulationStatus.Present;
            result.ObservationEligible = true;
            result.InteractionEligible = animal.Health > 0 &&
                animal.Behavior != D4AnimalBehavior.Attacking;
            result.RiskLevel = RiskForAnimal(animal);
            result.ReasonCode = result.InteractionEligible
                ? "animal_observation_and_interaction_available"
                : animal.Behavior == D4AnimalBehavior.Attacking
                    ? "animal_interaction_risk_high"
                    : "animal_observation_only";
            result.FeedbackKey = result.InteractionEligible
                ? "wildlife.feedback.interact"
                : "wildlife.feedback.observe_only";
            return result;
        }

        private static D20WildlifePopulationReadModel FromFishSchool(EcologyState snapshot,
            FishSchoolState school, string habitatId)
        {
            var result = Base(snapshot, D20WildlifePopulationSourceKind.FishSchool,
                school.FishSchoolId, school.FishSpeciesId, habitatId);
            result.LocationRef = school.FishingSpotId ?? string.Empty;
            result.Biomass = school.Biomass;
            result.Capacity = school.Capacity;
            result.FishingPressure = school.FishingPressure;

            if (school.Biomass < 0 || school.Capacity <= 0 || school.FishingPressure < 0)
            {
                result.PopulationStatus = D20WildlifePopulationStatus.Invalid;
                result.RiskLevel = D20WildlifeRiskLevel.Unknown;
                result.ReasonCode = "fish_school_state_invalid";
                result.FeedbackKey = "wildlife.feedback.unavailable";
                return result;
            }
            if (school.Biomass == 0)
            {
                result.PopulationStatus = D20WildlifePopulationStatus.Depleted;
                result.RiskLevel = D20WildlifeRiskLevel.None;
                result.ReasonCode = "fish_school_depleted";
                result.FeedbackKey = "wildlife.feedback.depleted";
                return result;
            }

            result.PopulationStatus = D20WildlifePopulationStatus.Present;
            result.ObservationEligible = true;
            result.InteractionEligible = !string.IsNullOrEmpty(result.LocationRef);
            result.RiskLevel = RiskForFishSchool(school);
            result.ReasonCode = result.InteractionEligible
                ? "fish_school_observation_and_interaction_available"
                : "fish_school_observation_only";
            result.FeedbackKey = result.InteractionEligible
                ? "wildlife.feedback.interact"
                : "wildlife.feedback.observe_only";
            return result;
        }

        private static D20WildlifePopulationReadModel Base(EcologyState snapshot,
            D20WildlifePopulationSourceKind sourceKind, string entityId, string speciesId,
            string habitatId)
        {
            var habitat = snapshot.Habitat;
            return new D20WildlifePopulationReadModel
            {
                InputAvailable = true,
                SourceKind = sourceKind,
                EntityId = entityId ?? string.Empty,
                SpeciesId = speciesId ?? string.Empty,
                HabitatId = habitatId ?? string.Empty,
                EcologyStress = 0f,
                Pollution = habitat == null ? 0f : habitat.Pollution,
                Turbidity = habitat == null ? 0f : habitat.Turbidity,
                EcologyRevision = Math.Max(0L, snapshot.EcologyRevision),
                SourceOwnerSystem = EcologyOwnerSystem,
            };
        }

        private static D20WildlifePopulationReadModel Missing(string sourceKind,
            string entityId, string reasonCode)
        {
            D20WildlifePopulationSourceKind kind = sourceKind == "animal"
                ? D20WildlifePopulationSourceKind.Animal
                : sourceKind == "fish_school"
                    ? D20WildlifePopulationSourceKind.FishSchool
                    : D20WildlifePopulationSourceKind.Unknown;
            return new D20WildlifePopulationReadModel
            {
                InputAvailable = false,
                SourceKind = kind,
                PopulationStatus = D20WildlifePopulationStatus.Unknown,
                RiskLevel = D20WildlifeRiskLevel.Unknown,
                EntityId = entityId ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackKey = "wildlife.feedback.unavailable",
                SourceOwnerSystem = EcologyOwnerSystem,
            };
        }

        private static D20HabitatReadModel FromHabitat(EcologyState snapshot)
        {
            var habitat = snapshot.Habitat;
            if (habitat == null) return new D20HabitatReadModel { HabitatId = HabitatId(snapshot) };
            return new D20HabitatReadModel
            {
                HabitatId = habitat.HabitatId ?? HabitatId(snapshot),
                BiomeId = habitat.BiomeId ?? string.Empty,
                TemperatureC = habitat.TemperatureC,
                Turbidity = habitat.Turbidity,
                Pollution = habitat.Pollution,
                TidePhase = habitat.TidePhase,
                CurrentSpeed = habitat.CurrentSpeed,
                CurrentDirectionDegrees = habitat.CurrentDirectionDegrees,
                LastUpdatedDay = Math.Max(0, habitat.LastUpdatedDay),
                Revision = Math.Max(0L, habitat.Revision),
            };
        }

        private static string HabitatId(EcologyState snapshot)
        {
            if (snapshot == null) return string.Empty;
            if (snapshot.Habitat != null && !string.IsNullOrEmpty(snapshot.Habitat.HabitatId))
                return snapshot.Habitat.HabitatId;
            return snapshot.HabitatId ?? string.Empty;
        }

        private static D20WildlifeRiskLevel RiskForAnimal(AnimalState animal)
        {
            if (animal.Health <= 0) return D20WildlifeRiskLevel.Critical;
            if (animal.EcologyStress >= 0.75f || animal.Health <= 25)
                return D20WildlifeRiskLevel.Critical;
            if (animal.EcologyStress >= 0.50f || animal.Health <= 50 ||
                animal.Behavior == D4AnimalBehavior.Attacking)
                return D20WildlifeRiskLevel.High;
            if (animal.EcologyStress >= 0.25f || animal.Behavior == D4AnimalBehavior.Alert)
                return D20WildlifeRiskLevel.Moderate;
            return D20WildlifeRiskLevel.Low;
        }

        private static D20WildlifeRiskLevel RiskForFishSchool(FishSchoolState school)
        {
            if (school.Biomass <= 0) return D20WildlifeRiskLevel.None;
            float pressure = school.FishingPressure / (float)Math.Max(1, school.Capacity);
            if (pressure >= 1.0f || school.Biomass <= Math.Max(1, school.Capacity / 4))
                return D20WildlifeRiskLevel.Critical;
            if (pressure >= 0.66f || school.Biomass <= Math.Max(1, school.Capacity / 2))
                return D20WildlifeRiskLevel.High;
            if (pressure >= 0.33f) return D20WildlifeRiskLevel.Moderate;
            return D20WildlifeRiskLevel.Low;
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
