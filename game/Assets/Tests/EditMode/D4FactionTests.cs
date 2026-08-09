using NUnit.Framework;
using Whalebone.Systems.D4;

namespace D4.Tests
{
    public class D4FactionTests
    {
        [Test]
        public void Reputation_ClampsAndUnlocksPrivateerPermit()
        {
            var factions = new D4FactionSystem();
            factions.ApplyReputation("england", 200, "test");
            Assert.AreEqual(100, factions.GetReputation("england"));
            var permit = factions.IssuePrivateerPermit("england");
            Assert.IsTrue(permit.Accepted);
            Assert.IsTrue(factions.HasPrivateerPermit("england"));
        }

        [Test]
        public void Embargo_AndInspection_ProtectPortRules()
        {
            var factions = new D4FactionSystem();
            factions.SetEmbargo("green_cay", true);
            Assert.IsFalse(factions.CanEnterPort("green_cay", "england"));
            var inspection = factions.Inspect("green_cay", "england", new[] { "tobacco", "salt" });
            Assert.IsFalse(inspection.Allowed);
            Assert.IsTrue(inspection.CargoSeized);
            Assert.Greater(inspection.Fine, 0);
        }

        [Test]
        public void TariffAndEscort_ReflectReputation()
        {
            var factions = new D4FactionSystem();
            Assert.AreEqual(1f, factions.GetTariffMultiplier("france"), 0.0001f);
            factions.ApplyReputation("france", 70, "护航");
            Assert.Less(factions.GetTariffMultiplier("france"), 1f);
            factions.CompleteEscort("france", true);
            Assert.Greater(factions.GetReputation("france"), 70);
        }
    }
}
