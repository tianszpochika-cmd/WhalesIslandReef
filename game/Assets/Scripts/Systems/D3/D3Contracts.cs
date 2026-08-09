// ============================================================
// D3Contracts —— 贸易、地点、航程与存档快照的纯 C# 数据契约
//
// 归属：Whalebone.Systems.D3
// 说明：这里只定义跨系统的状态形状；数值写入必须经过同目录的裁决系统。
// 不引用 Unity、网络或 LLM，表现层只能读取快照并提交意图。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D3
{
    public enum D3LocationType
    {
        Town,
        Village,
        DesertedIsland
    }

    public enum D3WeatherType
    {
        Calm,
        TradeWind,
        Rain,
        Storm,
        Hurricane
    }

    public enum D3MarketAction
    {
        Buy,
        Sell
    }

    public enum D3VoyageStatus
    {
        InPort,
        Sailing,
        Landed,
        Arrived,
        Failed
    }

    [Serializable]
    public sealed class CommodityDefinition
    {
        public string Id;
        public string DisplayName;
        public int BasePrice;
        public int CargoVolume;

        public CommodityDefinition(string id, string displayName, int basePrice, int cargoVolume)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            BasePrice = Math.Max(1, basePrice);
            CargoVolume = Math.Max(1, cargoVolume);
        }
    }

    [Serializable]
    public sealed class PortDefinition
    {
        public string Id;
        public string DisplayName;
        public D3LocationType Type;
        public float WorldX;
        public float WorldZ;
        public float TariffRate;
        public Dictionary<string, float> SpecialtyCoefficients = new Dictionary<string, float>();
        public Dictionary<string, int> GatheringLimits = new Dictionary<string, int>();

        public PortDefinition(string id, string displayName, D3LocationType type,
            float worldX, float worldZ, float tariffRate)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Type = type;
            WorldX = Sanitize(worldX);
            WorldZ = Sanitize(worldZ);
            TariffRate = Clamp(tariffRate, 0f, 0.5f);
        }

        public float Specialty(string commodityId)
        {
            if (commodityId != null && SpecialtyCoefficients.TryGetValue(commodityId, out float value))
                return Clamp(value, 0.5f, 1.5f);
            return 1f;
        }

        private static float Sanitize(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    [Serializable]
    public sealed class WeatherDefinition
    {
        public D3WeatherType Type;
        public string DisplayName;
        public float EventMultiplier;

        public WeatherDefinition(D3WeatherType type, string displayName, float eventMultiplier)
        {
            Type = type;
            DisplayName = displayName ?? string.Empty;
            EventMultiplier = Clamp(eventMultiplier, 0.5f, 1.5f);
        }

        private static float Clamp(float value, float min, float max)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 1f : Math.Max(min, Math.Min(max, value));
        }
    }

    [Serializable]
    public sealed class VoyageEventDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int WaterDelta;
        public int FoodDelta;
        public int HullDelta;
        public int MoneyDelta;

        public VoyageEventDefinition(string id, string displayName, string description,
            int waterDelta = 0, int foodDelta = 0, int hullDelta = 0, int moneyDelta = 0)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Description = description ?? string.Empty;
            WaterDelta = waterDelta;
            FoodDelta = foodDelta;
            HullDelta = hullDelta;
            MoneyDelta = moneyDelta;
        }
    }

    /// <summary>货舱与玩家随身物品的内存状态。货物数量只能由库存系统变更。</summary>
    [Serializable]
    public sealed class D3InventoryState
    {
        public int CargoCapacity;
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();
        public Dictionary<string, int> PersonalItems = new Dictionary<string, int>();

        public D3InventoryState(int cargoCapacity)
        {
            CargoCapacity = Math.Max(0, cargoCapacity);
        }

        public D3InventoryState Clone()
        {
            var copy = new D3InventoryState(CargoCapacity);
            foreach (var item in Cargo) copy.Cargo[item.Key] = item.Value;
            foreach (var item in PersonalItems) copy.PersonalItems[item.Key] = item.Value;
            return copy;
        }
    }

    /// <summary>港口市场的供给库存。市场数量由贸易系统唯一修改。</summary>
    [Serializable]
    public sealed class D3MarketState
    {
        public Dictionary<string, int> Stock = new Dictionary<string, int>();

        public D3MarketState Clone()
        {
            var copy = new D3MarketState();
            foreach (var item in Stock) copy.Stock[item.Key] = item.Value;
            return copy;
        }
    }

    [Serializable]
    public sealed class D3TradeState
    {
        public int Money;
        public D3InventoryState Inventory;
        public D3MarketState Market;

        public D3TradeState(int money, int cargoCapacity)
        {
            Money = Math.Max(0, money);
            Inventory = new D3InventoryState(cargoCapacity);
            Market = new D3MarketState();
        }
    }

    [Serializable]
    public sealed class MarketQuote
    {
        public string PortId;
        public string CommodityId;
        public int BuyUnitPrice;
        public int SellUnitPrice;
        public float SupplyCoefficient;
        public float TariffCoefficient;
        public float EventCoefficient;

        public MarketQuote Clone()
        {
            return new MarketQuote
            {
                PortId = PortId,
                CommodityId = CommodityId,
                BuyUnitPrice = BuyUnitPrice,
                SellUnitPrice = SellUnitPrice,
                SupplyCoefficient = SupplyCoefficient,
                TariffCoefficient = TariffCoefficient,
                EventCoefficient = EventCoefficient,
            };
        }
    }

    [Serializable]
    public sealed class TradeResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public int Quantity;
        public int UnitPrice;
        public int TotalPrice;
        public int MoneyAfter;

        public static TradeResult Fail(string code, string message, int money)
        {
            return new TradeResult
            {
                Succeeded = false,
                Code = code ?? "trade_failed",
                Message = message ?? string.Empty,
                MoneyAfter = Math.Max(0, money),
            };
        }
    }

    [Serializable]
    public sealed class VoyagePlan
    {
        public string OriginId;
        public string DestinationId;
        public int TotalDays;
        public int Seed;

        public VoyagePlan Clone()
        {
            return new VoyagePlan
            {
                OriginId = OriginId,
                DestinationId = DestinationId,
                TotalDays = TotalDays,
                Seed = Seed,
            };
        }
    }

    [Serializable]
    public sealed class VoyageState
    {
        public string OriginId;
        public string DestinationId;
        public D3VoyageStatus Status = D3VoyageStatus.InPort;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public int CurrentDay;
        public int TotalDays;
        public int Seed;
        public int Water;
        public int Food;
        public int Hull;
        public int HullMax;
        public int MoneyDelta;
        public List<string> EventIds = new List<string>();

        public VoyageState Clone()
        {
            var copy = new VoyageState
            {
                OriginId = OriginId,
                DestinationId = DestinationId,
                Status = Status,
                Weather = Weather,
                CurrentDay = CurrentDay,
                TotalDays = TotalDays,
                Seed = Seed,
                Water = Water,
                Food = Food,
                Hull = Hull,
                HullMax = HullMax,
                MoneyDelta = MoneyDelta,
            };
            copy.EventIds.AddRange(EventIds);
            return copy;
        }
    }

    [Serializable]
    public sealed class VoyageDayResult
    {
        public bool Succeeded;
        public bool Arrived;
        public string ErrorCode;
        public string EventId;
        public string Message;
        public int WaterAfter;
        public int FoodAfter;
        public int HullAfter;
        public int MoneyDelta;
    }

    [Serializable]
    public sealed class VoyageSettlement
    {
        public bool Succeeded;
        public string OriginId;
        public string DestinationId;
        public int DaysElapsed;
        public List<string> Events = new List<string>();
        public int WaterRemaining;
        public int FoodRemaining;
        public int HullRemaining;
        public int MoneyDelta;
        public string Message;
    }

    [Serializable]
    public sealed class GatheringResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public string ItemId;
        public int Quantity;
        public int CargoUsed;
    }

    [Serializable]
    public sealed class D3RuntimeSnapshot
    {
        public string CurrentLocationId;
        public string DestinationId;
        public D3VoyageStatus VoyageStatus = D3VoyageStatus.InPort;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public int CurrentDay;
        public int TotalDays;
        public int VoyageSeed;
        public int Water;
        public int Food;
        public int Hull;
        public int HullMax;
        public int Money;
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();
        public Dictionary<string, int> PersonalItems = new Dictionary<string, int>();
        public Dictionary<string, int> MarketStock = new Dictionary<string, int>();
        public Dictionary<string, int> GatheredThisVisit = new Dictionary<string, int>();
        public List<string> EventIds = new List<string>();

        public D3RuntimeSnapshot Clone()
        {
            var copy = new D3RuntimeSnapshot
            {
                CurrentLocationId = CurrentLocationId,
                DestinationId = DestinationId,
                VoyageStatus = VoyageStatus,
                Weather = Weather,
                CurrentDay = CurrentDay,
                TotalDays = TotalDays,
                VoyageSeed = VoyageSeed,
                Water = Water,
                Food = Food,
                Hull = Hull,
                HullMax = HullMax,
                Money = Money,
            };
            foreach (var item in Cargo) copy.Cargo[item.Key] = item.Value;
            foreach (var item in PersonalItems) copy.PersonalItems[item.Key] = item.Value;
            foreach (var item in MarketStock) copy.MarketStock[item.Key] = item.Value;
            foreach (var item in GatheredThisVisit) copy.GatheredThisVisit[item.Key] = item.Value;
            copy.EventIds.AddRange(EventIds);
            return copy;
        }
    }
}
