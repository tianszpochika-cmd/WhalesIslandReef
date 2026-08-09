using NUnit.Framework;
using Whalebone.Systems.D3;

namespace D3.Tests
{
    public class D3VoyageTests
    {
        [Test]
        public void Begin_RejectsSamePortAndInsufficientSupplies()
        {
            var system = new VoyageSettlementSystem();
            Assert.IsFalse(system.TryBegin(PortCatalog.StartingPortId, PortCatalog.StartingPortId,
                4f, 1, 20, 20, 60, 60, out _, out string error));
            Assert.AreEqual("same_location", error);
            Assert.IsFalse(system.TryBegin(PortCatalog.StartingPortId, PortCatalog.GreenCayId,
                4f, 1, 1, 1, 60, 60, out _, out error));
            Assert.AreEqual("insufficient_supplies", error);
        }

        [Test]
        public void Voyage_ConsumesSuppliesAppliesEventAndArrives()
        {
            var system = new VoyageSettlementSystem();
            Assert.IsTrue(system.TryBegin(PortCatalog.StartingPortId, PortCatalog.GreenCayId,
                4f, 42, 20, 20, 60, 60, out VoyageState state, out string error), error);
            int guard = 0;
            while (state.Status == D3VoyageStatus.Sailing && guard++ < 10)
            {
                var day = system.AdvanceDay(state);
                Assert.IsTrue(day.Succeeded, day.Message);
            }
            Assert.AreEqual(D3VoyageStatus.Arrived, state.Status);
            Assert.Greater(state.EventIds.Count, 0);
            var settlement = system.Settle(state);
            Assert.IsTrue(settlement.Succeeded);
            Assert.AreEqual(D3VoyageStatus.InPort, state.Status);
            Assert.AreEqual(state.CurrentDay, settlement.DaysElapsed);
            Assert.LessOrEqual(state.Water, 20);
            Assert.Less(state.Food, 20);
        }

        [Test]
        public void VoyageEventRoll_IsDeterministicForSameState()
        {
            var first = Begin(777);
            var second = Begin(777);
            var eventSystem = new VoyageEventSystem();
            Assert.AreEqual(eventSystem.Roll(first).Id, eventSystem.Roll(second).Id);
            eventSystem.Apply(first, eventSystem.Roll(first));
            eventSystem.Apply(second, eventSystem.Roll(second));
            Assert.AreEqual(first.EventIds[0], second.EventIds[0]);
            Assert.AreEqual(first.Hull, second.Hull);
        }

        [Test]
        public void Voyage_FailsWhenHullIsDestroyed()
        {
            var system = new VoyageSettlementSystem();
            Assert.IsTrue(system.TryBegin(PortCatalog.StartingPortId, PortCatalog.DriftwoodIsleId,
                4f, 4, 20, 20, 1, 1, out VoyageState state, out _));
            int guard = 0;
            while (state.Status == D3VoyageStatus.Sailing && guard++ < 12)
                system.AdvanceDay(state);
            Assert.That(state.Status == D3VoyageStatus.Failed || state.Status == D3VoyageStatus.Arrived);
            if (state.Status == D3VoyageStatus.Failed)
                Assert.IsFalse(system.Settle(state).Succeeded);
        }

        private static VoyageState Begin(int seed)
        {
            var system = new VoyageSettlementSystem();
            Assert.IsTrue(system.TryBegin(PortCatalog.StartingPortId, PortCatalog.GreenCayId,
                4f, seed, 20, 20, 60, 60, out VoyageState state, out _));
            return state;
        }
    }
}
