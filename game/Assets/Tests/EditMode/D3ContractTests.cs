using NUnit.Framework;
using Whalebone.Systems.D3;

namespace D3.Tests
{
    public class D3ContractTests
    {
        [Test]
        public void M1Catalog_HasTenCommoditiesThreePortsFiveWeatherEightEvents()
        {
            Assert.AreEqual(10, CommodityCatalog.All.Count);
            Assert.AreEqual(3, PortCatalog.All.Count);
            Assert.AreEqual(5, WeatherCatalog.All.Count);
            Assert.AreEqual(8, VoyageEventCatalog.All.Count);
            Assert.IsNotNull(PortCatalog.Find(PortCatalog.StartingPortId));
            Assert.AreEqual(D3LocationType.DesertedIsland,
                PortCatalog.Find(PortCatalog.DriftwoodIsleId).Type);
        }

        [Test]
        public void CatalogIds_AreUniqueAndResolvable()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var commodity in CommodityCatalog.All)
            {
                Assert.IsTrue(seen.Add(commodity.Id));
                Assert.AreSame(commodity, CommodityCatalog.Find(commodity.Id));
                Assert.Greater(commodity.BasePrice, 0);
                Assert.Greater(commodity.CargoVolume, 0);
            }
        }

        [Test]
        public void RouteDays_ArePositiveForDifferentLocations()
        {
            int days = PortCatalog.CalculateTravelDays(
                PortCatalog.StartingPortId, PortCatalog.GreenCayId, 4f);
            Assert.Greater(days, 0);
            Assert.AreEqual(0, PortCatalog.CalculateTravelDays(
                PortCatalog.StartingPortId, PortCatalog.StartingPortId, 4f));
        }
    }
}
