// ============================================================
// D3InventorySystem —— 货舱容量与物品数量唯一写者
// ============================================================

using System;

namespace Whalebone.Systems.D3
{
    public static class D3InventorySystem
    {
        public static int UsedCargo(D3InventoryState state)
        {
            if (state == null || state.Cargo == null) return 0;
            int used = 0;
            foreach (var item in state.Cargo)
            {
                var definition = CommodityCatalog.Find(item.Key);
                if (definition == null || item.Value <= 0) continue;
                used += item.Value * definition.CargoVolume;
            }
            return Math.Max(0, used);
        }

        public static int FreeCargo(D3InventoryState state)
        {
            if (state == null) return 0;
            return Math.Max(0, state.CargoCapacity - UsedCargo(state));
        }

        public static int CountCargo(D3InventoryState state, string itemId)
        {
            if (state == null || itemId == null || state.Cargo == null) return 0;
            return state.Cargo.TryGetValue(itemId, out int count) ? Math.Max(0, count) : 0;
        }

        public static bool TryAddCargo(D3InventoryState state, string itemId, int quantity, out string error)
        {
            error = null;
            var definition = CommodityCatalog.Find(itemId);
            if (state == null || definition == null || quantity <= 0)
            {
                error = "invalid_cargo";
                return false;
            }

            long volume = (long)quantity * definition.CargoVolume;
            if (volume > FreeCargo(state))
            {
                error = "cargo_full";
                return false;
            }

            state.Cargo.TryGetValue(itemId, out int current);
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
    }
}
