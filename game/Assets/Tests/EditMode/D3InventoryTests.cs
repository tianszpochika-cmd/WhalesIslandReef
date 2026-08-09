using NUnit.Framework;
using Whalebone.Systems.D3;

namespace D3.Tests
{
    public class D3InventoryTests
    {
        [Test]
        public void Cargo_UsesCommodityVolumeAndRespectsCapacity()
        {
            var inventory = new D3InventoryState(4);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "timber", 2, out string error));
            Assert.IsNull(error);
            Assert.AreEqual(4, D3InventorySystem.UsedCargo(inventory));
            Assert.IsFalse(D3InventorySystem.TryAddCargo(inventory, "salt", 1, out error));
            Assert.AreEqual("cargo_full", error);
        }

        [Test]
        public void Cargo_Remove_CleansEmptyStacks()
        {
            var inventory = new D3InventoryState(10);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "salt", 3, out _));
            Assert.IsTrue(D3InventorySystem.TryRemoveCargo(inventory, "salt", 3, out _));
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "salt"));
            Assert.IsFalse(inventory.Cargo.ContainsKey("salt"));
        }

        [Test]
        public void Cargo_RejectsUnknownOrNonPositiveInput()
        {
            var inventory = new D3InventoryState(10);
            Assert.IsFalse(D3InventorySystem.TryAddCargo(inventory, "unknown", 1, out string error));
            Assert.AreEqual("invalid_cargo", error);
            Assert.IsFalse(D3InventorySystem.TryAddCargo(inventory, "salt", 0, out error));
            Assert.AreEqual("invalid_cargo", error);
        }

        [Test]
        public void PersonalItems_DoNotConsumeCargo()
        {
            var inventory = new D3InventoryState(1);
            Assert.IsTrue(D3InventorySystem.TryAddPersonal(inventory, "compass", 1, out _));
            Assert.AreEqual(0, D3InventorySystem.UsedCargo(inventory));
            Assert.AreEqual(1, D3InventorySystem.CountPersonal(inventory, "compass"));
        }
    }
}
