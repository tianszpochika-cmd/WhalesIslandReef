// ============================================================
// GatheringSystem —— 登岛采集数量与访问上限裁决
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D3
{
    public sealed class GatheringSystem
    {
        public GatheringResult Gather(D3InventoryState inventory, IDictionary<string, int> gatheredThisVisit,
            string locationId, string itemId, int quantity)
        {
            var location = PortCatalog.Find(locationId);
            var commodity = CommodityCatalog.Find(itemId);
            if (location == null || commodity == null)
                return Fail("invalid_reference", "采集地点或资源不存在。", itemId, inventory);
            if (location.Type == D3LocationType.Town)
                return Fail("not_gatherable", "城镇港没有可直接采集的野外资源。", itemId, inventory);
            if (quantity <= 0)
                return Fail("invalid_quantity", "采集数量必须大于零。", itemId, inventory);

            int limit = location.GatheringLimits.TryGetValue(itemId, out int configured)
                ? Math.Max(0, configured) : 0;
            if (limit <= 0)
                return Fail("resource_unavailable", "这里没有这种可采集资源。", itemId, inventory);

            int gathered = gatheredThisVisit != null && gatheredThisVisit.TryGetValue(itemId, out int current)
                ? Math.Max(0, current) : 0;
            if (gathered + quantity > limit)
                return Fail("gathering_limit", "本次登岛的资源已经接近枯竭。", itemId, inventory);

            if (!D3InventorySystem.TryAddCargo(inventory, itemId, quantity, out string error))
                return Fail(error, error == "cargo_full" ? "货舱容量不足。" : "无法装入采集物。", itemId, inventory);

            if (gatheredThisVisit != null)
                gatheredThisVisit[itemId] = gathered + quantity;
            return new GatheringResult
            {
                Succeeded = true,
                Code = "ok",
                Message = "采集完成。",
                ItemId = itemId,
                Quantity = quantity,
                CargoUsed = D3InventorySystem.UsedCargo(inventory),
            };
        }

        private static GatheringResult Fail(string code, string message, string itemId, D3InventoryState inventory)
        {
            return new GatheringResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                ItemId = itemId,
                CargoUsed = D3InventorySystem.UsedCargo(inventory),
            };
        }
    }
}
