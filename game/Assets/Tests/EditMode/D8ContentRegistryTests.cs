using System.Linq;
using NUnit.Framework;
using Whalebone.Content;

namespace D8.Tests
{
    public class D8ContentRegistryTests
    {
        [Test]
        public void Resources_ReadRepresentativeContentRegistry()
        {
            var registry = ContentRegistryReader.LoadFromResources();

            Assert.AreEqual(13, registry.Definition.prefixes.Length);
            Assert.GreaterOrEqual(registry.Entries.Count, 20);
            Assert.GreaterOrEqual(registry.Entries.Count(entry => entry != null && entry.id.StartsWith("plant_")), 1000);
            Assert.IsNotNull(registry.Find("ship_moonwake"));
            Assert.IsNotNull(registry.Find("crew_isaac_reed"));
            Assert.IsNotNull(registry.Find("island_saint_marys"));
            Assert.IsNotNull(registry.Find("building_saint_marys_dock"));
            Assert.IsNotNull(registry.Find("item_salted_cod"));
            Assert.IsNotNull(registry.Find("item_gear_handline_basic"));
            Assert.IsNotNull(registry.Find("item_fresh_mackerel"));
            Assert.IsNotNull(registry.Find("item_salted_mackerel"));
            Assert.IsNotNull(registry.Find("item_cooked_mackerel"));
            Assert.AreEqual("gear_handline_basic", registry.Find("item_gear_handline_basic").logic_ref);
            Assert.IsNotNull(registry.Find("animal_green_turtle"));
            Assert.IsNotNull(registry.Find("plant_red_mangrove"));
            Assert.IsNotNull(registry.Find("fish_mackerel_school"));
            Assert.IsNotNull(registry.Find("quest_first_chart"));
            Assert.IsNotNull(registry.Find("reward_first_chart"));
            Assert.IsNotNull(registry.Find("outfit_weathered_deckhand"));
            Assert.IsNotNull(registry.Find("pet_cabin_cat"));
            Assert.IsNotNull(registry.Find("asset_ph_ship_fishing"));
        }

        [Test]
        public void Entries_ExposeLocalizationOwnershipSourceAndLicense()
        {
            var registry = ContentRegistryReader.LoadFromResources();
            foreach (var entry in registry.Entries)
            {
                Assert.IsNotEmpty(entry.name, entry.id);
                Assert.IsTrue(entry.display_name_key.StartsWith("content."), entry.id);
                Assert.IsNotEmpty(entry.source_note, entry.id);
                Assert.IsNotNull(entry.source, entry.id);
                Assert.IsNotEmpty(entry.source.reference, entry.id);
                Assert.IsNotNull(entry.license, entry.id);
                Assert.IsTrue(entry.license.commercial_use, entry.id);
                Assert.IsNotEmpty(entry.owner_system, entry.id);
                Assert.IsNotEmpty(entry.test_id, entry.id);
            }
        }
    }
}
