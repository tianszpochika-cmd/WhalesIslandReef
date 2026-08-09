using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D4;

namespace D4.Tests
{
    public class D4ContentTests
    {
        [Test]
        public void Catalogs_HaveD4BaselineContent()
        {
            Assert.AreEqual(7, D4FactionCatalog.All.Count);
            Assert.AreEqual(5, D4ShipCatalog.All.Count);
            Assert.AreEqual(3, System.Enum.GetValues(typeof(D4WeaponId)).Length);
            Assert.AreEqual(15, D4AnimalCatalog.All.Count);
            Assert.AreEqual(5, D4DiseaseCatalog.All.Count);
            Assert.AreEqual(9, D4MainlineSystem.Missions.Count);
        }

        [Test]
        public void CatalogIds_AreUnique()
        {
            var ids = new HashSet<string>();
            foreach (var faction in D4FactionCatalog.All) Assert.IsTrue(ids.Add(faction.Id));
            ids.Clear();
            foreach (var ship in D4ShipCatalog.All) Assert.IsTrue(ids.Add(ship.Id));
            ids.Clear();
            foreach (var animal in D4AnimalCatalog.All) Assert.IsTrue(ids.Add(animal.Id));
            ids.Clear();
            foreach (var disease in D4DiseaseCatalog.All) Assert.IsTrue(ids.Add(disease.Id));
        }

        [Test]
        public void Snapshot_CloneIsIndependent()
        {
            var snapshot = new D4RuntimeSnapshot();
            snapshot.Factions.Reputation["england"] = 20;
            snapshot.Mainline.Flags["learn_sailing"] = true;
            snapshot.RouteLog.Add(new RouteLogEntry { Id = "log_1", Text = "记录" });
            var clone = snapshot.Clone();
            clone.Factions.Reputation["england"] = -10;
            clone.Mainline.Flags["learn_sailing"] = false;
            clone.RouteLog[0].Text = "改写";
            Assert.AreEqual(20, snapshot.Factions.Reputation["england"]);
            Assert.IsTrue(snapshot.Mainline.Flags["learn_sailing"]);
            Assert.AreEqual("记录", snapshot.RouteLog[0].Text);
        }
    }
}
