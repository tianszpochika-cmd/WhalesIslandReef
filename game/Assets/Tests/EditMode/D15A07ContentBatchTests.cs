using NUnit.Framework;
using Whalebone.Content;

namespace D15.Tests
{
    public class D15A07ContentBatchTests
    {
        [Test]
        public void Resources_LoadD15A07RepresentativeAndBatchEntries()
        {
            var registry = ContentRegistryReader.LoadFromResources();

            var representativeIds = new[]
            {
                "crew_isaac_reed",
                "crew_d15_a07_preset_001",
                "crew_d15_a07_preset_060",
                "pet_cabin_cat",
                "pet_cat_mooring",
                "pet_harbor_dog",
                "pet_dog_watch",
                "animal_domestic_chicken",
                "animal_domestic_duck",
                "animal_domestic_cattle"
            };

            foreach (var id in representativeIds)
            {
                var entry = registry.Find(id);
                Assert.IsNotNull(entry, id);
                Assert.IsNotEmpty(entry.display_name_key, id);
                Assert.IsNotEmpty(entry.owner_system, id);
                Assert.IsNotNull(entry.source, id);
                Assert.IsNotNull(entry.license, id);
                Assert.IsTrue(entry.license.commercial_use, id);
            }

            var crewCount = 0;
            foreach (var entry in registry.Entries)
            {
                if (entry != null && entry.id.StartsWith("crew_d15_a07_"))
                    crewCount++;
            }

            Assert.AreEqual(60, crewCount);
        }
    }
}
