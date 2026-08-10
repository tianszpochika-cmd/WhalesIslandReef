// ============================================================
// D4Contracts —— D4 内容扩充的纯 C# 数据契约
//
// 强逻辑层只保存可回溯状态和输入/结果，不引用 Unity、网络或 LLM。
// 表现层、服务端镜像和 SaveSystem 都只能通过这些字段交换数据。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D4
{
    public enum D4FactionId { England, Spain, France, Portugal, Pirates, Tribes, Smugglers }
    public enum D4WeaponId { RoundShot, ChainShot, GrapeShot }
    public enum D4CombatStatus { Idle, Engaged, Boarded, Victory, Escaped, Defeat }
    public enum D4AnimalBehavior { Foraging, Alert, Attacking, BreedingHint }
    public enum D4AnimalId
    {
        Whale, Dolphin, Shark, SeaTurtle, Stingray, FishSchool,
        Iguana, Parrot, HermitCrab, Crab, VenomousSnake, MosquitoSwarm,
        Crocodile, Jaguar, GoldenParrot
    }
    public enum D4DiseaseId { Scurvy, Malaria, YellowFever, Dysentery, Consumption }
    public enum D4LogKind { Voyage, Combat, Faction, Animal, Medical, Mainline, CrewDiary }
    public enum D4ShipTypeId { SmallFishing, Sloop, Brigantine, MerchantBrig, LightGalleon }
    public enum D4UpgradeSlot { Hull, Sail, Cannon, MedBay, Galley, Cabin }
    public enum D11AnimalLifecycleState { Wild, Discovered, Identified, Observed, Rescued, InTransit, Released, Collected, Dead }
    public enum D11RareAnimalAction { Discover, Identify, Observe, Rescue, LegalTransfer, Transport, Release, Collect, IllegalTrade }
    public enum D11TransportStatus { InTransit, Arrived, Died, Released, Collected }
    public enum D11EvidenceKind { Observation, Identification, Rescue, Transfer, Release, Collection, IllegalTrade }

    [Serializable]
    public sealed class FactionDefinition
    {
        public string Id;
        public string Name;
        public string Culture;
        public int BaseTariff;
        public string[] RivalIds = new string[0];
        public string[] HomePorts = new string[0];
    }

    [Serializable]
    public sealed class FactionState
    {
        public Dictionary<string, int> Reputation = new Dictionary<string, int>();
        public List<string> AtWar = new List<string>();
        public List<string> EmbargoedPorts = new List<string>();
        public List<string> PrivateerPermits = new List<string>();
        public int WantedLevel;

        public FactionState Clone()
        {
            var result = new FactionState { WantedLevel = NumericConfiguration.ClampWantedLevel(WantedLevel) };
            Copy(Reputation, result.Reputation);
            result.AtWar.AddRange(AtWar ?? new List<string>());
            result.EmbargoedPorts.AddRange(EmbargoedPorts ?? new List<string>());
            result.PrivateerPermits.AddRange(PrivateerPermits ?? new List<string>());
            return result;
        }

        private static void Copy(Dictionary<string, int> source, Dictionary<string, int> target)
        {
            if (source == null) return;
            foreach (var pair in source) target[pair.Key] = pair.Value;
        }

    }

    [Serializable]
    public sealed class ShipDefinition
    {
        public string Id;
        public string Name;
        public float Speed;
        public float Turn;
        public int CargoMax;
        public int CrewMax;
        public int CannonMax;
        public int HullMax;
        public int Price;
        public int UnlockReputation;
    }

    [Serializable]
    public sealed class ShipyardState
    {
        public string ShipTypeId = "small_fishing";
        public List<string> UnlockedShipTypes = new List<string> { "small_fishing" };
        public Dictionary<string, int> Upgrades = new Dictionary<string, int>();

        public ShipyardState Clone()
        {
            var result = new ShipyardState { ShipTypeId = ShipTypeId };
            if (UnlockedShipTypes != null) result.UnlockedShipTypes.AddRange(UnlockedShipTypes);
            result.UnlockedShipTypes.Remove("small_fishing");
            result.UnlockedShipTypes.Insert(0, "small_fishing");
            if (Upgrades != null)
                foreach (var pair in Upgrades) result.Upgrades[pair.Key] = pair.Value;
            return result;
        }
    }

    [Serializable]
    public sealed class CombatState
    {
        public D4CombatStatus Status = D4CombatStatus.Idle;
        public string EnemyId;
        public int PlayerHull;
        public int PlayerHullMax;
        public int EnemyHull;
        public int EnemyHullMax;
        public float Distance;
        public float Wind;
        public int PlayerCannons;
        public int LoadedCount;
        public D4WeaponId Weapon = D4WeaponId.RoundShot;
        public int EnemyCannons;
        public int EnemyLoadedCount;
        public int EnemyCrew;
        public int PlayerCrew;
        public int Turn;
        public int BoardingScore;

        public CombatState Clone()
        {
            return new CombatState
            {
                Status = Status,
                EnemyId = EnemyId,
                PlayerHull = PlayerHull,
                PlayerHullMax = PlayerHullMax,
                EnemyHull = EnemyHull,
                EnemyHullMax = EnemyHullMax,
                Distance = Distance,
                Wind = Wind,
                PlayerCannons = PlayerCannons,
                LoadedCount = LoadedCount,
                Weapon = Weapon,
                EnemyCannons = EnemyCannons,
                EnemyLoadedCount = EnemyLoadedCount,
                EnemyCrew = EnemyCrew,
                PlayerCrew = PlayerCrew,
                Turn = Turn,
                BoardingScore = BoardingScore,
            };
        }
    }

    [Serializable]
    public sealed class CombatResult
    {
        public bool Accepted;
        public bool Hit;
        public bool Critical;
        public int Damage;
        public int EnemyCrewLoss;
        public int PlayerDamage;
        public D4CombatStatus Status;
        public string Message;
    }

    [Serializable]
    public sealed class AnimalDefinition
    {
        public string Id;
        public string Name;
        public string Habitat;
        public int Danger;
        public int CollectionValue;
        public bool Marine;
        public Whalebone.Game.Config.Rarity Rarity = Whalebone.Game.Config.Rarity.Common;

        /// <summary>D4 旧字段兼容适配；canonical 值是 Rarity。</summary>
        public bool Rare
        {
            get { return Rarity >= Whalebone.Game.Config.Rarity.Rare; }
            set { Rarity = value ? Whalebone.Game.Config.Rarity.Rare : Whalebone.Game.Config.Rarity.Common; }
        }
    }

    [Serializable]
    public sealed class AnimalState
    {
        public string AnimalId;
        public string HabitatId;
        public D4AnimalBehavior Behavior;
        public int Health = (int)NumericConfiguration.Default.Health.DefaultValue;
        public float EcologyStress;
        public int LastUpdatedDay;
        public bool Observed;
        public int CollectionValue;
        public D11AnimalLifecycleState Lifecycle = D11AnimalLifecycleState.Wild;
        public int Welfare = 100;
        public int TransportDays;
        public int LastHandledDay;
        public string CurrentLocationRef;
        public string SourceRef;

        public AnimalState Clone()
        {
            return new AnimalState
            {
                AnimalId = AnimalId,
                HabitatId = HabitatId,
                Behavior = Behavior,
                Health = Health,
                EcologyStress = EcologyStress,
                LastUpdatedDay = LastUpdatedDay,
                Observed = Observed,
                CollectionValue = CollectionValue,
                Lifecycle = Lifecycle,
                Welfare = Welfare,
                TransportDays = TransportDays,
                LastHandledDay = LastHandledDay,
                CurrentLocationRef = CurrentLocationRef,
                SourceRef = SourceRef,
            };
        }
    }

    [Serializable]
    public sealed class D11RareAnimalDefinition
    {
        public string SpeciesId;
        public string RuntimeAnimalId;
        public string HabitatId;
        public string RegionId;
        public string Season;
        public string BehaviorNote;
        public string RarityReason;
        public string CapturePolicy;
        public string TransportNeed;
        public string BuyerType;
        public string ReleaseRegion;
        public string SourceNote;
        public string SourceRef;
        public string TestId;
        public bool Protected;
        public int ObservationValue;
        public int CollectionValue;
    }

    [Serializable]
    public sealed class D11AnimalEcologyDecision
    {
        public bool Accepted;
        public bool Applied;
        public bool Duplicate;
        public bool Died;
        public string Code;
        public string Message;
        public string AnimalId;
        public string SpeciesId;
        public string LocationRef;
        public string Behavior;
        public int Welfare;
        public float MortalityRisk;
        public D11AnimalLifecycleState Lifecycle;
        public int WorldDay;

        public D11AnimalEcologyDecision Clone()
        {
            return new D11AnimalEcologyDecision
            {
                Accepted = Accepted,
                Applied = Applied,
                Duplicate = Duplicate,
                Died = Died,
                Code = Code,
                Message = Message,
                AnimalId = AnimalId,
                SpeciesId = SpeciesId,
                LocationRef = LocationRef,
                Behavior = Behavior,
                Welfare = Welfare,
                MortalityRisk = MortalityRisk,
                Lifecycle = Lifecycle,
                WorldDay = WorldDay,
            };
        }
    }

    [Serializable]
    public sealed class D11AnimalEvidenceRecord
    {
        public string EvidenceId;
        public D11EvidenceKind Kind;
        public string AnimalId;
        public string SpeciesId;
        public string LocationRef;
        public int WorldDay;
        public string Season;
        public string Behavior;
        public string Handling;
        public string SourceRef;
        public string EvidenceRef;
        public string FactEventId;
        public bool Verified;

        public D11AnimalEvidenceRecord Clone()
        {
            return new D11AnimalEvidenceRecord
            {
                EvidenceId = EvidenceId,
                Kind = Kind,
                AnimalId = AnimalId,
                SpeciesId = SpeciesId,
                LocationRef = LocationRef,
                WorldDay = WorldDay,
                Season = Season,
                Behavior = Behavior,
                Handling = Handling,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                FactEventId = FactEventId,
                Verified = Verified,
            };
        }
    }

    [Serializable]
    public sealed class D11CollectionBookEntry
    {
        public string EntryId;
        public string SpeciesId;
        public string AnimalId;
        public string FirstLocationRef;
        public int FirstWorldDay = -1;
        public string FirstSeason;
        public string FirstBehavior;
        public string Handling;
        public List<string> EvidenceIds = new List<string>();
        public bool Observed;
        public bool Identified;
        public bool Rescued;
        public bool Released;
        public bool Collected;
        public bool Dead;
        public string CurrentStatus;
        public long Revision;

        public D11CollectionBookEntry Clone()
        {
            var result = new D11CollectionBookEntry
            {
                EntryId = EntryId,
                SpeciesId = SpeciesId,
                AnimalId = AnimalId,
                FirstLocationRef = FirstLocationRef,
                FirstWorldDay = FirstWorldDay,
                FirstSeason = FirstSeason,
                FirstBehavior = FirstBehavior,
                Handling = Handling,
                Observed = Observed,
                Identified = Identified,
                Rescued = Rescued,
                Released = Released,
                Collected = Collected,
                Dead = Dead,
                CurrentStatus = CurrentStatus,
                Revision = Revision,
            };
            if (EvidenceIds != null) result.EvidenceIds.AddRange(EvidenceIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D11CollectorState
    {
        public string CollectorId;
        public string CollectorType;
        public string BuyerType;
        public string[] AllowedSpeciesIds = new string[0];
        public string ProtectedAreaRef;
        public int ReputationGate;
        public bool EvidenceRequired = true;

        public D11CollectorState Clone()
        {
            return new D11CollectorState
            {
                CollectorId = CollectorId,
                CollectorType = CollectorType,
                BuyerType = BuyerType,
                AllowedSpeciesIds = AllowedSpeciesIds == null ? new string[0] : (string[])AllowedSpeciesIds.Clone(),
                ProtectedAreaRef = ProtectedAreaRef,
                ReputationGate = ReputationGate,
                EvidenceRequired = EvidenceRequired,
            };
        }
    }

    [Serializable]
    public sealed class D11ProtectedAreaState
    {
        public string AreaId;
        public string LocationRef;
        public bool ObservationAllowed = true;
        public bool RescueAllowed = true;
        public bool ReleaseAllowed = true;
        public bool TransferAllowed;
        public bool Active = true;
        public long Revision;

        public D11ProtectedAreaState Clone()
        {
            return new D11ProtectedAreaState
            {
                AreaId = AreaId,
                LocationRef = LocationRef,
                ObservationAllowed = ObservationAllowed,
                RescueAllowed = RescueAllowed,
                ReleaseAllowed = ReleaseAllowed,
                TransferAllowed = TransferAllowed,
                Active = Active,
                Revision = Revision,
            };
        }
    }

    [Serializable]
    public sealed class D11AnimalTransportState
    {
        public string TransportId;
        public string AnimalId;
        public string SpeciesId;
        public string FromLocationRef;
        public string ToLocationRef;
        public int StartWorldDay;
        public int DaysInTransit;
        public int DaysElapsed;
        public int Seed;
        public float MortalityRisk;
        public D11TransportStatus Status = D11TransportStatus.InTransit;
        public List<string> EvidenceIds = new List<string>();
        public int LastUpdatedDay;

        public D11AnimalTransportState Clone()
        {
            var result = new D11AnimalTransportState
            {
                TransportId = TransportId,
                AnimalId = AnimalId,
                SpeciesId = SpeciesId,
                FromLocationRef = FromLocationRef,
                ToLocationRef = ToLocationRef,
                StartWorldDay = StartWorldDay,
                DaysInTransit = DaysInTransit,
                DaysElapsed = DaysElapsed,
                Seed = Seed,
                MortalityRisk = MortalityRisk,
                Status = Status,
                LastUpdatedDay = LastUpdatedDay,
            };
            if (EvidenceIds != null) result.EvidenceIds.AddRange(EvidenceIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D11RareAnimalCollectionState
    {
        public int SchemaVersion = 1;
        public List<D11CollectionBookEntry> Book = new List<D11CollectionBookEntry>();
        public List<D11AnimalEvidenceRecord> Evidence = new List<D11AnimalEvidenceRecord>();
        public List<D11AnimalTransportState> Transports = new List<D11AnimalTransportState>();
        public List<D11CollectorState> Collectors = new List<D11CollectorState>();
        public List<D11ProtectedAreaState> ProtectedAreas = new List<D11ProtectedAreaState>();
        public long Revision;
        public string LastEventId = "";
        public List<string> AppliedEventKeys = new List<string>();

        public D11RareAnimalCollectionState Clone()
        {
            var result = new D11RareAnimalCollectionState
            {
                SchemaVersion = SchemaVersion,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (Book != null) foreach (var entry in Book) if (entry != null) result.Book.Add(entry.Clone());
            if (Evidence != null) foreach (var evidence in Evidence) if (evidence != null) result.Evidence.Add(evidence.Clone());
            if (Transports != null) foreach (var transport in Transports) if (transport != null) result.Transports.Add(transport.Clone());
            if (Collectors != null) foreach (var collector in Collectors) if (collector != null) result.Collectors.Add(collector.Clone());
            if (ProtectedAreas != null) foreach (var area in ProtectedAreas) if (area != null) result.ProtectedAreas.Add(area.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }
    }

    /// <summary>鱼群运行时状态；位置、生物量和捕捞压力只能由 EcologySystem 写入。</summary>
    [Serializable]
    public sealed class FishSchoolState
    {
        public string FishSchoolId;
        public string FishSpeciesId;
        public string FishingSpotId;
        public float WorldX;
        public float WorldZ;
        public int Biomass = 12;
        public int Capacity = 12;
        public float Depth = 2f;
        public string Migration = "nearshore";
        public int FishingPressure;
        public float Recovery;
        public int RecoveryRatePerDay = 1;
        public int LastMovedDay;

        public FishSchoolState Clone()
        {
            return new FishSchoolState
            {
                FishSchoolId = FishSchoolId,
                FishSpeciesId = FishSpeciesId,
                FishingSpotId = FishingSpotId,
                WorldX = WorldX,
                WorldZ = WorldZ,
                Biomass = Biomass,
                Capacity = Capacity,
                Depth = Depth,
                Migration = Migration,
                FishingPressure = FishingPressure,
                Recovery = Recovery,
                RecoveryRatePerDay = RecoveryRatePerDay,
                LastMovedDay = LastMovedDay,
            };
        }
    }

    [Serializable]
    public sealed class FishCatchPreview
    {
        public bool Accepted;
        public string Code;
        public string Message;
        public string FishSchoolId;
        public string FishingSpotId;
        public string FishItemId;
        public int Quantity;
        public int Day;
        public float CatchMultiplier = 1f;
        public float PressureMultiplier = 1f;
        public float TideMultiplier = 1f;
        public string FishSpeciesId;
        public string GearId;
        public float DepthMeters;
        public string TimeOfDayBucket;
        public string TideBand;
        public float DepthMultiplier = 1f;
        public float TimeMultiplier = 1f;
        public float GearMultiplier = 1f;
        public QualityGrade Quality = QualityGrade.Standard;
        public int QualityScore;
        public string ProtectionRuleId;
        public int FishingPressureAfter;
        public long EcologyRevision;

        public FishCatchPreview Clone()
        {
            return new FishCatchPreview
            {
                Accepted = Accepted,
                Code = Code,
                Message = Message,
                FishSchoolId = FishSchoolId,
                FishingSpotId = FishingSpotId,
                FishItemId = FishItemId,
                Quantity = Quantity,
                Day = Day,
                CatchMultiplier = CatchMultiplier,
                PressureMultiplier = PressureMultiplier,
                TideMultiplier = TideMultiplier,
                FishSpeciesId = FishSpeciesId,
                GearId = GearId,
                DepthMeters = DepthMeters,
                TimeOfDayBucket = TimeOfDayBucket,
                TideBand = TideBand,
                DepthMultiplier = DepthMultiplier,
                TimeMultiplier = TimeMultiplier,
                GearMultiplier = GearMultiplier,
                Quality = Quality,
                QualityScore = QualityScore,
                ProtectionRuleId = ProtectionRuleId,
                FishingPressureAfter = FishingPressureAfter,
                EcologyRevision = EcologyRevision,
            };
        }
    }

    [Serializable]
    public sealed class PlantResourceDefinition
    {
        public string Id;
        public string HabitatId;
        /// <summary>兼容 D11 资源行；ResourceKind 可为植物、浅水资源或植被采集类别。</summary>
        public string ResourceKind = "plant";
        public int Capacity = 8;
        public int RecoveryRatePerDay = 1;
        public int HarvestLimitPerAction = 3;
    }

    [Serializable]
    public sealed class PlantResourceState
    {
        public string PlantId;
        public string HabitatId;
        public string ResourceKind = "plant";
        public int Quantity;
        public int Capacity;
        public int HarvestPressure;
        public int RecoveryRatePerDay = 1;
        public int LastUpdatedDay;

        public PlantResourceState Clone()
        {
            return new PlantResourceState
            {
                PlantId = PlantId,
                HabitatId = HabitatId,
                ResourceKind = ResourceKind,
                Quantity = Quantity,
                Capacity = Capacity,
                HarvestPressure = HarvestPressure,
                RecoveryRatePerDay = RecoveryRatePerDay,
                LastUpdatedDay = LastUpdatedDay,
            };
        }
    }

    /// <summary>EcologySystem 保存的生境条件；天气仍由 EnvironmentSystem 唯一写入。</summary>
    [Serializable]
    public sealed class HabitatConditionState
    {
        public string HabitatId = "nearshore_reef_edge";
        public string BiomeId = "reef";
        public float TemperatureC = 27f;
        public float Turbidity = 0.10f;
        public float Pollution;
        public float TidePhase;
        public float CurrentSpeed = 0.20f;
        public float CurrentDirectionDegrees = 90f;
        public int LastUpdatedDay;
        public long Revision;

        public HabitatConditionState Clone()
        {
            return new HabitatConditionState
            {
                HabitatId = HabitatId,
                BiomeId = BiomeId,
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
    /// 植被采集的只读上下文。它描述采集系统已经裁决的工具、成熟度和
    /// 识别结果，资源数量仍只由 EcologySystem 写入。
    /// </summary>
    [Serializable]
    public sealed class D11PlantHarvestContext
    {
        public string ItemId;
        public string ResourceKind;
        public string ToolId;
        public string MaturityStage;
        public string Quality;
        public string RecognizedPlantId;
        public float IdentificationConfidence;
        public bool Misidentified;
        public bool Toxic;
        public int OutputQuantity;
        public string ConsequenceId;
        public string ProtectionAreaId;

        public D11PlantHarvestContext Clone()
        {
            return new D11PlantHarvestContext
            {
                ItemId = ItemId,
                ResourceKind = ResourceKind,
                ToolId = ToolId,
                MaturityStage = MaturityStage,
                Quality = Quality,
                RecognizedPlantId = RecognizedPlantId,
                IdentificationConfidence = IdentificationConfidence,
                Misidentified = Misidentified,
                Toxic = Toxic,
                OutputQuantity = OutputQuantity,
                ConsequenceId = ConsequenceId,
                ProtectionAreaId = ProtectionAreaId,
            };
        }
    }

    /// <summary>由天气快照和生态状态计算出的只读条件，不持有写权限。</summary>
    [Serializable]
    public sealed class D11EcologyConditions
    {
        public string WeatherStateId;
        public int WorldDay;
        public float TidePhase;
        public float CurrentSpeed;
        public float CurrentDirectionDegrees;
        public float TemperatureC;
        public float Turbidity;
        public float Pollution;
        public float FishActivityMultiplier;
        public float FishCatchMultiplier;
        public float FishRecoveryMultiplier;
        public float PlantGrowthMultiplier;
        public float AnimalHealthMultiplier;
        public bool ObservationBlocked;

        public D11EcologyConditions Clone()
        {
            return new D11EcologyConditions
            {
                WeatherStateId = WeatherStateId,
                WorldDay = WorldDay,
                TidePhase = TidePhase,
                CurrentSpeed = CurrentSpeed,
                CurrentDirectionDegrees = CurrentDirectionDegrees,
                TemperatureC = TemperatureC,
                Turbidity = Turbidity,
                Pollution = Pollution,
                FishActivityMultiplier = FishActivityMultiplier,
                FishCatchMultiplier = FishCatchMultiplier,
                FishRecoveryMultiplier = FishRecoveryMultiplier,
                PlantGrowthMultiplier = PlantGrowthMultiplier,
                AnimalHealthMultiplier = AnimalHealthMultiplier,
                ObservationBlocked = ObservationBlocked,
            };
        }
    }

    [Serializable]
    public sealed class D11EcologyOperationResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public int WorldDay;
        public int DaysApplied;
        public int Quantity;
        public string ResourceId;
        public string ResourceKind;
        public int RemainingQuantity;
        public int HarvestPressureAfter;
        public float PollutionAfter;
        public D11EcologyConditions Conditions;
    }

    [Serializable]
    public sealed class EcologyState
    {
        public string HabitatId;
        public List<AnimalState> Animals = new List<AnimalState>();
        public List<FishSchoolState> FishSchools = new List<FishSchoolState>();
        public HabitatConditionState Habitat = new HabitatConditionState();
        public List<PlantResourceState> Plants = new List<PlantResourceState>();
        public int CollectionCount;
        public int LastEcologyDay;
        public long EcologyRevision;
        public string LastEcologyEventId = "";
        public List<string> AppliedEcologyEventKeys = new List<string>();

        public EcologyState Clone()
        {
            var result = new EcologyState
            {
                HabitatId = HabitatId,
                Habitat = Habitat == null ? new HabitatConditionState() : Habitat.Clone(),
                CollectionCount = CollectionCount,
                LastEcologyDay = LastEcologyDay,
                EcologyRevision = EcologyRevision,
                LastEcologyEventId = LastEcologyEventId,
            };
            if (Animals != null)
                foreach (var animal in Animals) if (animal != null) result.Animals.Add(animal.Clone());
            if (FishSchools != null)
                foreach (var school in FishSchools) if (school != null) result.FishSchools.Add(school.Clone());
            if (Plants != null)
                foreach (var plant in Plants) if (plant != null) result.Plants.Add(plant.Clone());
            if (AppliedEcologyEventKeys != null)
                result.AppliedEcologyEventKeys.AddRange(AppliedEcologyEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class DiseaseDefinition
    {
        public string Id;
        public string Name;
        public int DailyRisk;
        public int DailyDamage;
        public int TreatmentCost;
        public int TreatmentPower;
        public string Source;
    }

    [Serializable]
    public sealed class CrewHealthState
    {
        public string CrewId;
        public int Health = (int)NumericConfiguration.Default.Health.DefaultValue;
        public int Fatigue = (int)NumericConfiguration.Default.Fatigue.DefaultValue;
        public List<string> Diseases = new List<string>();

        public CrewHealthState Clone()
        {
            var result = new CrewHealthState { CrewId = CrewId, Health = Health, Fatigue = Fatigue };
            if (Diseases != null) result.Diseases.AddRange(Diseases);
            return result;
        }
    }

    [Serializable]
    public sealed class HealthState
    {
        public List<CrewHealthState> Crew = new List<CrewHealthState>();
        public int MedicalSupplies;

        public HealthState Clone()
        {
            var result = new HealthState { MedicalSupplies = MedicalSupplies };
            if (Crew != null)
                foreach (var member in Crew) if (member != null) result.Crew.Add(member.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class MainlineMission
    {
        public string Id;
        public int Chapter;
        public string Title;
        public string Description;
        public string RequiredFlag;
        public string GrantsFlag;
        public string RequiredFaction;
        public int RequiredReputation;
    }

    [Serializable]
    public sealed class MainlineState
    {
        public int Chapter = 1;
        public int MissionIndex;
        public Dictionary<string, bool> Flags = new Dictionary<string, bool>();
        public List<string> CompletedMissions = new List<string>();

        public MainlineState Clone()
        {
            var result = new MainlineState { Chapter = Chapter, MissionIndex = MissionIndex };
            if (Flags != null) foreach (var pair in Flags) result.Flags[pair.Key] = pair.Value;
            if (CompletedMissions != null) result.CompletedMissions.AddRange(CompletedMissions);
            return result;
        }
    }

    [Serializable]
    public sealed class MainlineResult
    {
        public bool Advanced;
        public bool Completed;
        public MainlineMission Mission;
        public string Message;
    }

    [Serializable]
    public sealed class RouteLogEntry
    {
        public string Id;
        public int Day;
        public D4LogKind Kind;
        public string OriginId;
        public string DestinationId;
        public string Text;

        public RouteLogEntry Clone()
        {
            return new RouteLogEntry
            {
                Id = Id,
                Day = Day,
                Kind = Kind,
                OriginId = OriginId,
                DestinationId = DestinationId,
                Text = Text,
            };
        }
    }

    [Serializable]
    public sealed class D4RuntimeSnapshot
    {
        public FactionState Factions = new FactionState();
        public ShipyardState Shipyard = new ShipyardState();
        public CombatState Combat = new CombatState();
        public EcologyState Ecology = new EcologyState();
        public D11RareAnimalCollectionState RareAnimals = new D11RareAnimalCollectionState();
        public HealthState Health = new HealthState();
        public MainlineState Mainline = new MainlineState();
        public List<RouteLogEntry> RouteLog = new List<RouteLogEntry>();

        public D4RuntimeSnapshot Clone()
        {
            var result = new D4RuntimeSnapshot
            {
                Factions = Factions == null ? new FactionState() : Factions.Clone(),
                Shipyard = Shipyard == null ? new ShipyardState() : Shipyard.Clone(),
                Combat = Combat == null ? new CombatState() : Combat.Clone(),
                Ecology = Ecology == null ? new EcologyState() : Ecology.Clone(),
                RareAnimals = RareAnimals == null ? new D11RareAnimalCollectionState() : RareAnimals.Clone(),
                Health = Health == null ? new HealthState() : Health.Clone(),
                Mainline = Mainline == null ? new MainlineState() : Mainline.Clone(),
            };
            if (RouteLog != null)
                foreach (var entry in RouteLog) if (entry != null) result.RouteLog.Add(entry.Clone());
            return result;
        }
    }
}
