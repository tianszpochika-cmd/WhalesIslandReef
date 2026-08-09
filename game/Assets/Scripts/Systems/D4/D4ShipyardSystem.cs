using System;

namespace Whalebone.Systems.D4
{
    public sealed class ShipyardResult
    {
        public bool Accepted;
        public int Cost;
        public int NewLevel;
        public string ShipTypeId;
        public string Message;
    }

    public sealed class ShipModifiers
    {
        public int HullBonus;
        public int CannonBonus;
        public int CargoBonus;
        public float SpeedBonus;
        public float TurnBonus;
        public int MedicalBonus;
        public int MoraleBonus;
    }

    /// <summary>船型解锁与六类数值改装的唯一裁决者。</summary>
    public sealed class D4ShipyardSystem
    {
        private readonly ShipyardState _state;

        public D4ShipyardSystem(ShipyardState initial = null)
        {
            _state = initial == null ? new ShipyardState() : initial.Clone();
            foreach (var slot in Enum.GetNames(typeof(D4UpgradeSlot)))
                if (!_state.Upgrades.ContainsKey(slot)) _state.Upgrades[slot] = 0;
        }

        public ShipyardState Snapshot => _state.Clone();

        public ShipyardResult TryUnlockShip(string shipTypeId, int reputation, ref int money)
        {
            var definition = D4ShipCatalog.Find(shipTypeId);
            if (definition == null) return new ShipyardResult { Message = "未知船型。" };
            if (_state.UnlockedShipTypes.Contains(shipTypeId))
                return new ShipyardResult { Accepted = true, ShipTypeId = shipTypeId, Message = "船型已经解锁。" };
            if (reputation < definition.UnlockReputation)
                return new ShipyardResult { Message = "势力声望不足，船坞不接受这笔订单。" };
            if (money < definition.Price)
                return new ShipyardResult { Message = "资金不足。" };
            money -= definition.Price;
            _state.UnlockedShipTypes.Add(shipTypeId);
            return new ShipyardResult { Accepted = true, Cost = definition.Price, ShipTypeId = shipTypeId, Message = "新船型已解锁。" };
        }

        public ShipyardResult TryUpgrade(D4UpgradeSlot slot, ref int money, int reputation)
        {
            string key = slot.ToString();
            int level = _state.Upgrades.ContainsKey(key) ? _state.Upgrades[key] : 0;
            if (level >= 3) return new ShipyardResult { Message = "该改装已达到最高等级。" };
            int cost = UpgradeCost(slot, level + 1);
            if (money < cost) return new ShipyardResult { Message = "资金不足。" };
            if (reputation < (level + 1) * 5) return new ShipyardResult { Message = "港口声望不足，船坞拒绝改装。" };
            money -= cost;
            _state.Upgrades[key] = level + 1;
            return new ShipyardResult { Accepted = true, Cost = cost, NewLevel = level + 1, Message = "船只改装完成。" };
        }

        public ShipModifiers GetModifiers()
        {
            return new ShipModifiers
            {
                HullBonus = Level(D4UpgradeSlot.Hull) * 15,
                SpeedBonus = Level(D4UpgradeSlot.Sail) * 0.35f,
                TurnBonus = Level(D4UpgradeSlot.Sail) * 0.25f,
                CannonBonus = Level(D4UpgradeSlot.Cannon) * 1,
                MedicalBonus = Level(D4UpgradeSlot.MedBay) * 10,
                MoraleBonus = Level(D4UpgradeSlot.Galley) * 4 + Level(D4UpgradeSlot.Cabin) * 3,
                CargoBonus = Level(D4UpgradeSlot.Cabin) * 8,
            };
        }

        public int Level(D4UpgradeSlot slot)
        {
            int level;
            return _state.Upgrades.TryGetValue(slot.ToString(), out level) ? Math.Max(0, Math.Min(3, level)) : 0;
        }

        private static int UpgradeCost(D4UpgradeSlot slot, int level)
        {
            int baseCost;
            switch (slot)
            {
                case D4UpgradeSlot.Hull: baseCost = 180; break;
                case D4UpgradeSlot.Sail: baseCost = 150; break;
                case D4UpgradeSlot.Cannon: baseCost = 220; break;
                case D4UpgradeSlot.MedBay: baseCost = 240; break;
                case D4UpgradeSlot.Galley: baseCost = 140; break;
                default: baseCost = 160; break;
            }
            return baseCost * level;
        }
    }
}
