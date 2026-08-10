// ============================================================
// D9PortShipPreparationContracts —— 港口整备的快照与结果契约
//
// 这些类型是 D9 港口页面的只读投影/意图结果，不新增 D8 实体。
// 领域状态仍由 D3 Economy/Inventory、D4 Shipyard、Ship/Crew owner 写入。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    public enum D9PortService
    {
        Berth,
        Market,
        Tavern,
        Shipyard,
        Supplies,
        Repair,
        Recruitment,
        Wages,
    }

    [Serializable]
    public sealed class D9PreparationResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string Service;
        public int Cost;
        public int Quantity;
        public int MoneyAfter;
        public FactEvent Fact;

        public static D9PreparationResult Fail(string service, string code, string message, int money)
        {
            return new D9PreparationResult
            {
                Accepted = false,
                Code = code ?? "rejected",
                Message = message ?? "请求被拒绝。",
                Service = service ?? string.Empty,
                MoneyAfter = Math.Max(0, money),
            };
        }

        public static D9PreparationResult Success(string service, string message, int cost, int quantity, int money)
        {
            return new D9PreparationResult
            {
                Accepted = true,
                Code = "ok",
                Message = message ?? "操作完成。",
                Service = service ?? string.Empty,
                Cost = Math.Max(0, cost),
                Quantity = Math.Max(0, quantity),
                MoneyAfter = Math.Max(0, money),
            };
        }
    }

    [Serializable]
    public sealed class D9ShipPreparationSnapshot
    {
        public string ShipId;
        public string ShipTypeId;
        public int Weight;
        public int CargoCapacity;
        public float Draft;
        public int CrewCount;
        public int CrewCapacity;
        public int Hull;
        public int HullMax;
        public int Damage;
        public int Water;
        public int Food;
        public bool Moored;
        public string BerthId;
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();
        public Dictionary<string, int> Upgrades = new Dictionary<string, int>();

        public D9ShipPreparationSnapshot Clone()
        {
            var result = new D9ShipPreparationSnapshot
            {
                ShipId = ShipId,
                ShipTypeId = ShipTypeId,
                Weight = Weight,
                CargoCapacity = CargoCapacity,
                Draft = Draft,
                CrewCount = CrewCount,
                CrewCapacity = CrewCapacity,
                Hull = Hull,
                HullMax = HullMax,
                Damage = Damage,
                Water = Water,
                Food = Food,
                Moored = Moored,
                BerthId = BerthId,
            };
            if (Cargo != null) foreach (var pair in Cargo) result.Cargo[pair.Key] = pair.Value;
            if (Upgrades != null) foreach (var pair in Upgrades) result.Upgrades[pair.Key] = pair.Value;
            return result;
        }
    }

    [Serializable]
    public sealed class D9PortShipPreparationSnapshot
    {
        public string PortId;
        public string PortDisplayName;
        public bool InPort;
        public bool Moored;
        public string BerthId;
        public int Money;
        public D9ShipPreparationSnapshot Ship = new D9ShipPreparationSnapshot();
        public Dictionary<string, bool> Services = new Dictionary<string, bool>();

        public D9PortShipPreparationSnapshot Clone()
        {
            var result = new D9PortShipPreparationSnapshot
            {
                PortId = PortId,
                PortDisplayName = PortDisplayName,
                InPort = InPort,
                Moored = Moored,
                BerthId = BerthId,
                Money = Money,
                Ship = Ship == null ? new D9ShipPreparationSnapshot() : Ship.Clone(),
            };
            if (Services != null) foreach (var pair in Services) result.Services[pair.Key] = pair.Value;
            return result;
        }
    }
}
