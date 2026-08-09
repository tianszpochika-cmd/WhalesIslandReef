// ============================================================
// D4Contracts —— D4 内容扩充的纯 C# 数据契约
//
// 强逻辑层只保存可回溯状态和输入/结果，不引用 Unity、网络或 LLM。
// 表现层、服务端镜像和 SaveSystem 都只能通过这些字段交换数据。
// ============================================================

using System;
using System.Collections.Generic;

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
            var result = new FactionState { WantedLevel = Clamp(WantedLevel, 0, 100) };
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

        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
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
        public bool Rare;
    }

    [Serializable]
    public sealed class AnimalState
    {
        public string AnimalId;
        public D4AnimalBehavior Behavior;
        public int Health = 100;
        public bool Observed;
        public int CollectionValue;

        public AnimalState Clone()
        {
            return new AnimalState
            {
                AnimalId = AnimalId,
                Behavior = Behavior,
                Health = Health,
                Observed = Observed,
                CollectionValue = CollectionValue,
            };
        }
    }

    [Serializable]
    public sealed class EcologyState
    {
        public string HabitatId;
        public List<AnimalState> Animals = new List<AnimalState>();
        public int CollectionCount;

        public EcologyState Clone()
        {
            var result = new EcologyState { HabitatId = HabitatId, CollectionCount = CollectionCount };
            if (Animals != null)
                foreach (var animal in Animals) if (animal != null) result.Animals.Add(animal.Clone());
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
        public int Health = 100;
        public int Fatigue;
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
                Health = Health == null ? new HealthState() : Health.Clone(),
                Mainline = Mainline == null ? new MainlineState() : Mainline.Clone(),
            };
            if (RouteLog != null)
                foreach (var entry in RouteLog) if (entry != null) result.RouteLog.Add(entry.Clone());
            return result;
        }
    }
}
