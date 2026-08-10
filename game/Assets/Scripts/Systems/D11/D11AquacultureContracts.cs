// ============================================================
// D11AquacultureContracts —— D11-A03 活鱼舱与岛上水产 DTO
//
// 活鱼舱状态由 LiveFishHoldSystem 写入；岛上鱼塘/网箱状态由
// BuildingAquacultureSystem 写入。数量、重量和体积都在 DTO 中公开，
// 不藏在 UI、任务脚本或 AgentReply 中。生态压力不在本文件裁决，
// 由 EcologySystem 接收并写入自己的 EcologyState。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public static class D11AquacultureIds
    {
        public const int SchemaVersion = 1;
        public const string LiveFishHoldId = "room_live_fish_hold";
        public const string FishPondBuildingId = "building_fish_pond_basic";
        public const string NearshoreNetCageBuildingId = "building_nearshore_net_cage_basic";
        public const string FishSpeciesId = "mackerel";
        public const string FishSchoolId = "fish_mackerel_school";
        public const string FryItemId = "item_fish_fry_mackerel";
        public const string FeedItemId = "item_fish_feed_basic";
        public const string FishCargoItemId = "item_fresh_mackerel";
        public const string DefaultHabitatId = "nearshore_reef_edge";
    }

    public enum D11AquacultureFacilityKind
    {
        LiveFishHold,
        FishPond,
        NearshoreNetCage,
    }

    [Serializable]
    public sealed class D11FishDefinition
    {
        public string SpeciesId;
        public string FryItemId;
        public string FeedItemId;
        public string CargoItemId;
        public int UnitWeight = 1;
        public int UnitVolume = 1;
        public int FeedPerFishPerDay = 1;
        public float BaseMortalityRate = 0.01f;

        public D11FishDefinition Clone()
        {
            return new D11FishDefinition
            {
                SpeciesId = SpeciesId,
                FryItemId = FryItemId,
                FeedItemId = FeedItemId,
                CargoItemId = CargoItemId,
                UnitWeight = UnitWeight,
                UnitVolume = UnitVolume,
                FeedPerFishPerDay = FeedPerFishPerDay,
                BaseMortalityRate = BaseMortalityRate,
            };
        }
    }

    [Serializable]
    public sealed class D11AquacultureFacilityDefinition
    {
        public string Id;
        public D11AquacultureFacilityKind Kind;
        public string LocationRef;
        public string HabitatId;
        public int CapacityVolume;
        public int UnitFishVolume;
        public int UnitFishWeight;
        public int WaterExchangePerDay;
        public int MaintenanceLaborPerDay;
        public int MaxFish;

        public D11AquacultureFacilityDefinition Clone()
        {
            return new D11AquacultureFacilityDefinition
            {
                Id = Id,
                Kind = Kind,
                LocationRef = LocationRef,
                HabitatId = HabitatId,
                CapacityVolume = CapacityVolume,
                UnitFishVolume = UnitFishVolume,
                UnitFishWeight = UnitFishWeight,
                WaterExchangePerDay = WaterExchangePerDay,
                MaintenanceLaborPerDay = MaintenanceLaborPerDay,
                MaxFish = MaxFish,
            };
        }
    }

    [Serializable]
    public sealed class D11FishStockState
    {
        public string SpeciesId;
        public string CargoItemId;
        public int Quantity;
        public int UnitWeight = 1;
        public int UnitVolume = 1;

        public int TotalWeight { get { return Math.Max(0, Quantity) * Math.Max(1, UnitWeight); } }
        public int TotalVolume { get { return Math.Max(0, Quantity) * Math.Max(1, UnitVolume); } }

        public D11FishStockState Clone()
        {
            return new D11FishStockState
            {
                SpeciesId = SpeciesId,
                CargoItemId = CargoItemId,
                Quantity = Quantity,
                UnitWeight = UnitWeight,
                UnitVolume = UnitVolume,
            };
        }
    }

    /// <summary>一座鱼塘/网箱或活鱼舱的运行时状态；只有所属系统可写。</summary>
    [Serializable]
    public class D11AquacultureFacilityState
    {
        public int SchemaVersion = D11AquacultureIds.SchemaVersion;
        public string FacilityId;
        public D11AquacultureFacilityKind Kind;
        public string LocationRef;
        public string HabitatId;
        public int CapacityVolume;
        public int UnitFishVolume = 1;
        public int UnitFishWeight = 1;
        public int WaterExchangePerDay = 1;
        public int MaintenanceLaborPerDay = 1;
        public int MaxFish;
        public float Oxygen = 1f;
        public float WaterQuality = 1f;
        public float Crowding;
        public float Maintenance = 1f;
        public float MaintenanceDebt;
        public int FeedStock;
        public int LaborStock;
        public int LastWorldDay;
        public string LastEventId = string.Empty;
        public List<D11FishStockState> Fish = new List<D11FishStockState>();
        public List<string> AppliedEventKeys = new List<string>();

        public int FishQuantity
        {
            get
            {
                int total = 0;
                if (Fish != null)
                    foreach (var stock in Fish)
                        if (stock != null) total += Math.Max(0, stock.Quantity);
                return total;
            }
        }

        public int UsedVolume
        {
            get
            {
                int total = 0;
                if (Fish != null)
                    foreach (var stock in Fish)
                        if (stock != null) total += stock.TotalVolume;
                return total;
            }
        }

        public int TotalWeight
        {
            get
            {
                int total = 0;
                if (Fish != null)
                    foreach (var stock in Fish)
                        if (stock != null) total += stock.TotalWeight;
                return total;
            }
        }

        public int FreeVolume { get { return Math.Max(0, CapacityVolume - UsedVolume); } }

        public D11AquacultureFacilityState Clone()
        {
            var result = new D11AquacultureFacilityState
            {
                SchemaVersion = SchemaVersion,
                FacilityId = FacilityId,
                Kind = Kind,
                LocationRef = LocationRef,
                HabitatId = HabitatId,
                CapacityVolume = CapacityVolume,
                UnitFishVolume = UnitFishVolume,
                UnitFishWeight = UnitFishWeight,
                WaterExchangePerDay = WaterExchangePerDay,
                MaintenanceLaborPerDay = MaintenanceLaborPerDay,
                MaxFish = MaxFish,
                Oxygen = Oxygen,
                WaterQuality = WaterQuality,
                Crowding = Crowding,
                Maintenance = Maintenance,
                MaintenanceDebt = MaintenanceDebt,
                FeedStock = FeedStock,
                LaborStock = LaborStock,
                LastWorldDay = LastWorldDay,
                LastEventId = LastEventId ?? string.Empty,
            };
            if (Fish != null)
                foreach (var stock in Fish) if (stock != null) result.Fish.Add(stock.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void CopyFrom(D11AquacultureFacilityState source)
        {
            if (source == null) return;
            var copy = source.Clone();
            SchemaVersion = copy.SchemaVersion;
            FacilityId = copy.FacilityId;
            Kind = copy.Kind;
            LocationRef = copy.LocationRef;
            HabitatId = copy.HabitatId;
            CapacityVolume = copy.CapacityVolume;
            UnitFishVolume = copy.UnitFishVolume;
            UnitFishWeight = copy.UnitFishWeight;
            WaterExchangePerDay = copy.WaterExchangePerDay;
            MaintenanceLaborPerDay = copy.MaintenanceLaborPerDay;
            MaxFish = copy.MaxFish;
            Oxygen = copy.Oxygen;
            WaterQuality = copy.WaterQuality;
            Crowding = copy.Crowding;
            Maintenance = copy.Maintenance;
            MaintenanceDebt = copy.MaintenanceDebt;
            FeedStock = copy.FeedStock;
            LaborStock = copy.LaborStock;
            LastWorldDay = copy.LastWorldDay;
            LastEventId = copy.LastEventId;
            Fish = copy.Fish;
            AppliedEventKeys = copy.AppliedEventKeys;
        }
    }

    [Serializable]
    public sealed class D11LiveFishHoldState : D11AquacultureFacilityState
    {
        public D11LiveFishHoldState()
        {
            FacilityId = D11AquacultureIds.LiveFishHoldId;
            Kind = D11AquacultureFacilityKind.LiveFishHold;
            LocationRef = "ship_live_fish_hold";
            HabitatId = D11AquacultureIds.DefaultHabitatId;
            CapacityVolume = 12;
            UnitFishVolume = 1;
            UnitFishWeight = 1;
            WaterExchangePerDay = 4;
            MaintenanceLaborPerDay = 1;
            MaxFish = 12;
        }

        public new D11LiveFishHoldState Clone()
        {
            var result = new D11LiveFishHoldState();
            result.CopyFrom(this);
            return result;
        }
    }

    [Serializable]
    public sealed class D11IslandAquacultureState
    {
        public int SchemaVersion = D11AquacultureIds.SchemaVersion;
        public List<D11AquacultureFacilityState> Facilities = new List<D11AquacultureFacilityState>();

        public D11IslandAquacultureState Clone()
        {
            var result = new D11IslandAquacultureState { SchemaVersion = SchemaVersion };
            if (Facilities != null)
                foreach (var facility in Facilities)
                    if (facility != null) result.Facilities.Add(facility.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D11AquacultureRuntimeSnapshot
    {
        public int SchemaVersion = D11AquacultureIds.SchemaVersion;
        public D11LiveFishHoldState LiveFishHold = new D11LiveFishHoldState();
        public D11IslandAquacultureState IslandAquaculture = new D11IslandAquacultureState();
        public EventLogSnapshot EventLog;

        public D11AquacultureRuntimeSnapshot Clone()
        {
            return new D11AquacultureRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                LiveFishHold = LiveFishHold == null ? new D11LiveFishHoldState() : LiveFishHold.Clone(),
                IslandAquaculture = IslandAquaculture == null ? new D11IslandAquacultureState() : IslandAquaculture.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != D11AquacultureIds.SchemaVersion)
                throw new InvalidOperationException("D11 aquaculture schema_version mismatch: expected " +
                    D11AquacultureIds.SchemaVersion + ", received " + SchemaVersion + ".");
            ValidateFacility(LiveFishHold, "live_fish_hold");
            if (IslandAquaculture == null) throw new InvalidOperationException("D11 island aquaculture state is required.");
            if (IslandAquaculture.SchemaVersion != D11AquacultureIds.SchemaVersion)
                throw new InvalidOperationException("D11 island aquaculture schema_version mismatch.");
            if (IslandAquaculture.Facilities != null)
                foreach (var facility in IslandAquaculture.Facilities) ValidateFacility(facility, "island_facility");
        }

        private static void ValidateFacility(D11AquacultureFacilityState facility, string label)
        {
            if (facility == null) throw new InvalidOperationException("D11 " + label + " state is required.");
            if (facility.SchemaVersion != D11AquacultureIds.SchemaVersion)
                throw new InvalidOperationException("D11 " + label + " schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(facility.FacilityId))
                throw new InvalidOperationException("D11 " + label + " facility_id is required.");
            if (facility.CapacityVolume < 0 || facility.FishQuantity < 0 || facility.UsedVolume > facility.CapacityVolume)
                throw new InvalidOperationException("D11 " + label + " capacity state is invalid.");
        }
    }

    [Serializable]
    public sealed class D11LiveFishStockRequest
    {
        public string FishSpeciesId = D11AquacultureIds.FishSpeciesId;
        public int Quantity;
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11LiveFishMaintenanceRequest
    {
        public int WorldDay;
        public int WaterExchangeUnits;
        public int LaborUnits;
        public int Seed;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11AquacultureBuildRequest
    {
        public D11AquacultureFacilityKind Kind;
        public string FacilityId;
        public string LocationRef;
        public string HabitatId = D11AquacultureIds.DefaultHabitatId;
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11AquacultureInputRequest
    {
        public string FacilityId;
        public string FishSpeciesId = D11AquacultureIds.FishSpeciesId;
        public int FryQuantity;
        public int FeedQuantity;
        public int LaborUnits;
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11AquacultureAdvanceRequest
    {
        public string FacilityId;
        public int WorldDay;
        public int WaterExchangeUnits;
        public int Seed;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11AquacultureHarvestRequest
    {
        public string FacilityId;
        public string FishSpeciesId = D11AquacultureIds.FishSpeciesId;
        public int Quantity;
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D11AquacultureResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string DeathEventId;
        public string EcologyEventId;
        public int WorldDay;
        public int Quantity;
        public int DeadQuantity;
        public int FeedConsumed;
        public int LaborConsumed;
        public int OutputQuantity;
        public int TotalWeight;
        public int UsedVolume;
        public float OxygenAfter;
        public float WaterQualityAfter;
        public float CrowdingAfter;
        public float MaintenanceAfter;
        public FactEvent Fact;

        public D11AquacultureResult Clone()
        {
            return new D11AquacultureResult
            {
                Succeeded = Succeeded,
                Applied = Applied,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                EventId = EventId,
                DeathEventId = DeathEventId,
                EcologyEventId = EcologyEventId,
                WorldDay = WorldDay,
                Quantity = Quantity,
                DeadQuantity = DeadQuantity,
                FeedConsumed = FeedConsumed,
                LaborConsumed = LaborConsumed,
                OutputQuantity = OutputQuantity,
                TotalWeight = TotalWeight,
                UsedVolume = UsedVolume,
                OxygenAfter = OxygenAfter,
                WaterQualityAfter = WaterQualityAfter,
                CrowdingAfter = CrowdingAfter,
                MaintenanceAfter = MaintenanceAfter,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    public static class D11AquacultureCatalog
    {
        public static readonly D11FishDefinition Mackerel = new D11FishDefinition
        {
            SpeciesId = D11AquacultureIds.FishSpeciesId,
            FryItemId = D11AquacultureIds.FryItemId,
            FeedItemId = D11AquacultureIds.FeedItemId,
            CargoItemId = D11AquacultureIds.FishCargoItemId,
            UnitWeight = 1,
            UnitVolume = 1,
            FeedPerFishPerDay = 1,
            BaseMortalityRate = 0.01f,
        };

        public static readonly IReadOnlyList<D11AquacultureFacilityDefinition> Facilities =
            new List<D11AquacultureFacilityDefinition>
            {
                new D11AquacultureFacilityDefinition
                {
                    Id = D11AquacultureIds.LiveFishHoldId,
                    Kind = D11AquacultureFacilityKind.LiveFishHold,
                    LocationRef = "ship_live_fish_hold",
                    HabitatId = D11AquacultureIds.DefaultHabitatId,
                    CapacityVolume = 12,
                    UnitFishVolume = 1,
                    UnitFishWeight = 1,
                    WaterExchangePerDay = 4,
                    MaintenanceLaborPerDay = 1,
                    MaxFish = 12,
                },
                new D11AquacultureFacilityDefinition
                {
                    Id = D11AquacultureIds.FishPondBuildingId,
                    Kind = D11AquacultureFacilityKind.FishPond,
                    LocationRef = "island_fish_pond",
                    HabitatId = "freshwater_pond",
                    CapacityVolume = 24,
                    UnitFishVolume = 1,
                    UnitFishWeight = 1,
                    WaterExchangePerDay = 3,
                    MaintenanceLaborPerDay = 1,
                    MaxFish = 24,
                },
                new D11AquacultureFacilityDefinition
                {
                    Id = D11AquacultureIds.NearshoreNetCageBuildingId,
                    Kind = D11AquacultureFacilityKind.NearshoreNetCage,
                    LocationRef = "nearshore_net_cage",
                    HabitatId = D11AquacultureIds.DefaultHabitatId,
                    CapacityVolume = 32,
                    UnitFishVolume = 1,
                    UnitFishWeight = 1,
                    WaterExchangePerDay = 5,
                    MaintenanceLaborPerDay = 1,
                    MaxFish = 32,
                },
            };

        public static D11FishDefinition FindFish(string speciesId)
        {
            return string.Equals(speciesId, Mackerel.SpeciesId, StringComparison.Ordinal)
                ? Mackerel.Clone() : null;
        }

        public static D11AquacultureFacilityDefinition FindFacility(D11AquacultureFacilityKind kind)
        {
            foreach (var definition in Facilities)
                if (definition != null && definition.Kind == kind) return definition.Clone();
            return null;
        }
    }
}
