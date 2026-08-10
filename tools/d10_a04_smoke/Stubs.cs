using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D10
{
    public enum D10AppearanceSlot { Body, Hair, Hat, Top, Bottom, Coat, Shoes, Gloves, Accessory, CareerEquipment, Injury }

    public sealed class D10AppearanceLoadout
    {
        private readonly Dictionary<D10AppearanceSlot, string> _slots = new Dictionary<D10AppearanceSlot, string>();
        public string Get(D10AppearanceSlot slot)
        {
            string value;
            return _slots.TryGetValue(slot, out value) ? value : "";
        }
        public void Set(D10AppearanceSlot slot, string value) { _slots[slot] = value ?? ""; }
    }

    public sealed class D10AppearanceSnapshot
    {
        public D10AppearanceLoadout ActiveLoadout = new D10AppearanceLoadout();
    }

    public sealed class D10AppearanceOperationResult
    {
        public bool Success;
    }

    public sealed class D10AppearanceSystem
    {
        public D10AppearanceSnapshot Snapshot = new D10AppearanceSnapshot();
        public D10AppearanceOperationResult Equip(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || itemId == "unknown") return new D10AppearanceOperationResult();
            Snapshot.ActiveLoadout.Set(D10AppearanceSlot.Hat, itemId);
            return new D10AppearanceOperationResult { Success = true };
        }
    }
}
