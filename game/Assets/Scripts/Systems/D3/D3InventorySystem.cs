// ============================================================
// D3InventorySystem —— 货舱容量与物品数量唯一写者
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D3
{
    public static class D3InventorySystem
    {
        private sealed class CargoDefinition
        {
            public int UnitWeight;
            public int CargoVolume;
        }

        private static readonly Dictionary<string, CargoDefinition> AdditionalCargoDefinitions =
            new Dictionary<string, CargoDefinition>(StringComparer.Ordinal);

        /// <summary>
        /// 注册非 CommodityCatalog 货物的容量元数据。只注册不可变定义；数量仍只能由本类写入。
        /// </summary>
        public static void RegisterCargoDefinition(string itemId, int unitWeight, int cargoVolume)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            AdditionalCargoDefinitions[itemId] = new CargoDefinition
            {
                UnitWeight = Math.Max(1, unitWeight),
                CargoVolume = NumericConfiguration.ClampItemUnitVolume(cargoVolume),
            };
        }

        public static int UsedCargo(D3InventoryState state)
        {
            return UsedVolume(state);
        }

        public static int UsedVolume(D3InventoryState state)
        {
            if (state == null || state.Cargo == null) return 0;
            long used = 0;
            foreach (var item in state.Cargo)
            {
                if (!TryGetDefinition(item.Key, out int unitWeight, out int volume) || item.Value <= 0) continue;
                used += item.Value * volume;
            }
            return used > int.MaxValue ? int.MaxValue : Math.Max(0, (int)used);
        }

        public static int UsedWeight(D3InventoryState state)
        {
            if (state == null || state.Cargo == null) return 0;
            long used = 0;
            foreach (var item in state.Cargo)
            {
                if (!TryGetDefinition(item.Key, out int unitWeight, out int volume) || item.Value <= 0) continue;
                used += (long)item.Value * unitWeight;
            }
            return used > int.MaxValue ? int.MaxValue : Math.Max(0, (int)used);
        }

        public static int FreeCargo(D3InventoryState state)
        {
            if (state == null) return 0;
            return Math.Max(0, state.CargoCapacity - UsedVolume(state));
        }

        public static int FreeWeight(D3InventoryState state)
        {
            if (state == null || state.CargoWeightCapacity == int.MaxValue) return int.MaxValue;
            return Math.Max(0, state.CargoWeightCapacity - UsedWeight(state));
        }

        public static bool CanAddCargo(D3InventoryState state, string itemId, int quantity, out string error)
        {
            error = null;
            if (state == null || !TryGetDefinition(itemId, out int unitWeight, out int cargoVolume) || quantity <= 0)
            {
                error = "invalid_cargo";
                return false;
            }

            long volume = (long)quantity * cargoVolume;
            if (volume > FreeCargo(state))
            {
                error = "cargo_full";
                return false;
            }

            long weight = (long)quantity * unitWeight;
            if (weight > FreeWeight(state))
            {
                error = "weight_full";
                return false;
            }
            return true;
        }

        public static int CountCargo(D3InventoryState state, string itemId)
        {
            if (state == null || itemId == null || state.Cargo == null) return 0;
            return state.Cargo.TryGetValue(itemId, out int count) ? Math.Max(0, count) : 0;
        }

        public static bool TryAddCargo(D3InventoryState state, string itemId, int quantity, out string error)
        {
            error = null;
            if (!CanAddCargo(state, itemId, quantity, out error)) return false;

            state.Cargo.TryGetValue(itemId, out int current);
            if ((long)current + quantity > int.MaxValue)
            {
                error = "cargo_overflow";
                return false;
            }
            state.Cargo[itemId] = current + quantity;
            return true;
        }

        public static bool TryRemoveCargo(D3InventoryState state, string itemId, int quantity, out string error)
        {
            error = null;
            if (state == null || itemId == null || quantity <= 0 || CountCargo(state, itemId) < quantity)
            {
                error = "not_enough_cargo";
                return false;
            }

            int remaining = CountCargo(state, itemId) - quantity;
            if (remaining == 0) state.Cargo.Remove(itemId);
            else state.Cargo[itemId] = remaining;
            return true;
        }

        public static int CountPersonal(D3InventoryState state, string itemId)
        {
            if (state == null || itemId == null || state.PersonalItems == null) return 0;
            return state.PersonalItems.TryGetValue(itemId, out int count) ? Math.Max(0, count) : 0;
        }

        public static bool TryAddPersonal(D3InventoryState state, string itemId, int quantity, out string error)
        {
            error = null;
            if (state == null || string.IsNullOrEmpty(itemId) || quantity <= 0)
            {
                error = "invalid_item";
                return false;
            }

            state.PersonalItems.TryGetValue(itemId, out int current);
            state.PersonalItems[itemId] = current + quantity;
            return true;
        }

        private static bool TryGetDefinition(string itemId, out int unitWeight, out int cargoVolume)
        {
            var commodity = CommodityCatalog.Find(itemId);
            if (commodity != null)
            {
                unitWeight = commodity.UnitWeight;
                cargoVolume = commodity.CargoVolume;
                return true;
            }

            CargoDefinition additional;
            if (itemId != null && AdditionalCargoDefinitions.TryGetValue(itemId, out additional))
            {
                unitWeight = additional.UnitWeight;
                cargoVolume = additional.CargoVolume;
                return true;
            }

            unitWeight = 0;
            cargoVolume = 0;
            return false;
        }
    }
}
