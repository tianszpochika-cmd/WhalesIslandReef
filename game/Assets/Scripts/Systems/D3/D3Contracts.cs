// ============================================================
// D3Contracts —— 贸易、地点、航程与存档快照的纯 C# 数据契约
//
// 归属：Whalebone.Systems.D3
// 说明：这里只定义跨系统的状态形状；数值写入必须经过同目录的裁决系统。
// 不引用 Unity、网络或 LLM，表现层只能读取快照并提交意图。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

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
        public int UnitWeight;
        public int CargoVolume;
        public int DemandTarget;

        public int Weight { get { return UnitWeight; } }
        public int Volume { get { return CargoVolume; } }

        public CommodityDefinition(string id, string displayName, int basePrice, int cargoVolume)
            : this(id, displayName, basePrice, cargoVolume, cargoVolume, 50)
        {
        }

        public CommodityDefinition(string id, string displayName, int basePrice,
            int unitWeight, int cargoVolume, int demandTarget)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            BasePrice = Math.Max(1, basePrice);
            UnitWeight = Math.Max(1, unitWeight);
            CargoVolume = NumericConfiguration.ClampItemUnitVolume(cargoVolume);
            DemandTarget = Math.Max(1, demandTarget);
        }
    }

    /// <summary>
    /// 不扩张 D3 冻结的十种基础商品目录的可交易货物定义。
    /// D9 鱼获使用此形态接入 EconomySystem，价格仍由 TradeEconomySystem 裁决。
    /// </summary>
    [Serializable]
    public sealed class D3MarketItemDefinition
    {
        public string Id;
        public string DisplayName;
        public int BasePrice;
        public int UnitWeight;
        public int CargoVolume;
        public int DemandTarget;

        public D3MarketItemDefinition(string id, string displayName, int basePrice,
            int unitWeight, int cargoVolume, int demandTarget = 50)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            BasePrice = Math.Max(1, basePrice);
            UnitWeight = Math.Max(1, unitWeight);
            CargoVolume = NumericConfiguration.ClampItemUnitVolume(cargoVolume);
            DemandTarget = Math.Max(1, demandTarget);
        }
    }

    public enum PriceReasonKind
    {
        Inventory,
        Demand,
        Weather,
        PlayerTradeVolume,
        FactionPolicy,
        Tax,
    }

    /// <summary>EconomySystem 生成的可解释价格因子；表现层只能读取。</summary>
    [Serializable]
    public sealed class PriceReason
    {
        public PriceReasonKind Kind;
        public string Code;
        public string Explanation;
        public float Coefficient;

        public PriceReason Clone()
        {
            return new PriceReason
            {
                Kind = Kind,
                Code = Code,
                Explanation = Explanation,
                Coefficient = Coefficient,
            };
        }
    }

    /// <summary>只读传入 EconomySystem 的市场环境；不承载任何领域写操作。</summary>
    [Serializable]
    public sealed class TradePriceContext
    {
        public int WorldDay;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;

        public TradePriceContext Clone()
        {
            return new TradePriceContext
            {
                WorldDay = WorldDay,
                Weather = Weather,
                FactionPolicyId = FactionPolicyId,
                FactionPolicyCoefficient = FactionPolicyCoefficient,
            };
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
        public int CargoWeightCapacity;
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();
        public Dictionary<string, int> PersonalItems = new Dictionary<string, int>();

        public D3InventoryState(int cargoCapacity)
            : this(cargoCapacity, int.MaxValue)
        {
        }

        public D3InventoryState(int cargoCapacity, int cargoWeightCapacity)
        {
            CargoCapacity = Math.Max(0, cargoCapacity);
            CargoWeightCapacity = cargoWeightCapacity <= 0 ? int.MaxValue : cargoWeightCapacity;
        }

        public D3InventoryState Clone()
        {
            var copy = new D3InventoryState(CargoCapacity, CargoWeightCapacity);
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
        public Dictionary<string, int> Demand = new Dictionary<string, int>();
        public Dictionary<string, int> PlayerTradeVolume = new Dictionary<string, int>();
        public int WorldDay;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;

        public D3MarketState Clone()
        {
            var copy = new D3MarketState();
            foreach (var item in Stock) copy.Stock[item.Key] = item.Value;
            foreach (var item in Demand) copy.Demand[item.Key] = item.Value;
            foreach (var item in PlayerTradeVolume) copy.PlayerTradeVolume[item.Key] = item.Value;
            copy.WorldDay = WorldDay;
            copy.Weather = Weather;
            copy.FactionPolicyId = FactionPolicyId;
            copy.FactionPolicyCoefficient = FactionPolicyCoefficient;
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
            Money = NumericConfiguration.ClampCash(money);
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
        public int Stock;
        public int Demand;
        public int PlayerTradeVolume;
        public float SupplyCoefficient;
        public float DemandCoefficient;
        public float WeatherCoefficient;
        public float PlayerTradeVolumeCoefficient;
        public float FactionPolicyCoefficient;
        public float TariffCoefficient;
        public float EventCoefficient;
        public float BuyTaxRate;
        public float SellTaxRate;
        public List<PriceReason> PriceReasons = new List<PriceReason>();
        public bool IsOutOfStock { get { return Stock <= 0; } }

        public MarketQuote Clone()
        {
            var copy = new MarketQuote
            {
                PortId = PortId,
                CommodityId = CommodityId,
                BuyUnitPrice = BuyUnitPrice,
                SellUnitPrice = SellUnitPrice,
                Stock = Stock,
                Demand = Demand,
                PlayerTradeVolume = PlayerTradeVolume,
                SupplyCoefficient = SupplyCoefficient,
                DemandCoefficient = DemandCoefficient,
                WeatherCoefficient = WeatherCoefficient,
                PlayerTradeVolumeCoefficient = PlayerTradeVolumeCoefficient,
                FactionPolicyCoefficient = FactionPolicyCoefficient,
                TariffCoefficient = TariffCoefficient,
                EventCoefficient = EventCoefficient,
                BuyTaxRate = BuyTaxRate,
                SellTaxRate = SellTaxRate,
            };
            foreach (var reason in PriceReasons)
                if (reason != null) copy.PriceReasons.Add(reason.Clone());
            return copy;
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
        public int TaxAmount;
        public int WeightAfter;
        public int VolumeAfter;
        public string EventId;
        public string CommodityId;
        public long SaveRevision;
        public bool FactLogged;
        public bool DuplicateSettlement;
        public List<PriceReason> PriceReasons = new List<PriceReason>();

        public static TradeResult Fail(string code, string message, int money)
        {
            return new TradeResult
            {
                Succeeded = false,
                Code = code ?? "trade_failed",
                Message = message ?? string.Empty,
                MoneyAfter = NumericConfiguration.ClampCash(money),
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
        public D3WeatherType Weather = (D3WeatherType)NumericConfiguration.LegacyD3WeatherIndex(
            NumericConfiguration.Default.DefaultWeather);
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
        public D3WeatherType Weather = (D3WeatherType)NumericConfiguration.LegacyD3WeatherIndex(
            NumericConfiguration.Default.DefaultWeather);
        /// <summary>D9 天气/海况扩展；旧 D3 存档没有此字段时保持 null。</summary>
        public D9WeatherSeaSnapshot WeatherSea;
        public int CurrentDay;
        public int TotalDays;
        public int VoyageSeed;
        public int Water;
        public int Food;
        public int Hull;
        public int HullMax;
        public int Money;
        public long SaveRevision;
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();
        public Dictionary<string, int> PersonalItems = new Dictionary<string, int>();
        public Dictionary<string, int> MarketStock = new Dictionary<string, int>();
        public Dictionary<string, int> MarketDemand = new Dictionary<string, int>();
        public Dictionary<string, int> PlayerTradeVolume = new Dictionary<string, int>();
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;
        public Dictionary<string, int> GatheredThisVisit = new Dictionary<string, int>();
        public List<string> EventIds = new List<string>();
        public EventLogSnapshot EventLog;

        public D3RuntimeSnapshot Clone()
        {
            var copy = new D3RuntimeSnapshot
            {
                CurrentLocationId = CurrentLocationId,
                DestinationId = DestinationId,
                VoyageStatus = VoyageStatus,
                Weather = Weather,
                WeatherSea = WeatherSea == null ? null : WeatherSea.Clone(),
                CurrentDay = CurrentDay,
                TotalDays = TotalDays,
                VoyageSeed = VoyageSeed,
                Water = Water,
                Food = Food,
                Hull = Hull,
                HullMax = HullMax,
                Money = Money,
                SaveRevision = SaveRevision,
            };
            foreach (var item in Cargo) copy.Cargo[item.Key] = item.Value;
            foreach (var item in PersonalItems) copy.PersonalItems[item.Key] = item.Value;
            foreach (var item in MarketStock) copy.MarketStock[item.Key] = item.Value;
            foreach (var item in MarketDemand) copy.MarketDemand[item.Key] = item.Value;
            foreach (var item in PlayerTradeVolume) copy.PlayerTradeVolume[item.Key] = item.Value;
            copy.FactionPolicyId = FactionPolicyId;
            copy.FactionPolicyCoefficient = FactionPolicyCoefficient;
            foreach (var item in GatheredThisVisit) copy.GatheredThisVisit[item.Key] = item.Value;
            copy.EventIds.AddRange(EventIds);
            copy.EventLog = EventLog == null ? null : EventLog.Clone();
            return copy;
        }
    }
}
