using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Whalebone.Content;

namespace Save.Tests
{
    public class D15ItemOutfitDecorTests
    {
        [Test]
        public void D15A05Batch_Loads_AndHasOwnerInteractionAndAssets()
        {
            string root = Path.Combine(Application.dataPath, "Resources", "ContentRegistry");
            var registry = ContentRegistryReader.LoadFromDirectory(root);
            var categories = new HashSet<string>(StringComparer.Ordinal);
            int functional = 0;
            int decorative = 0;
            int sampled = 0;
            foreach (var entry in registry.Entries)
            {
                if (entry == null || entry.production_batch != "D15-A05*") continue;
                Assert.IsNotNull(entry.integration, entry.id);
                Assert.AreEqual(entry.owner_system, entry.integration.canonical_write_owner, entry.id);
                Assert.IsNotEmpty(entry.asset_refs, entry.id);
                Assert.IsNotEmpty(entry.model_ref, entry.id);
                Assert.IsNotEmpty(entry.material_ref, entry.id);
                Assert.IsNotEmpty(entry.collision_ref, entry.id);
                Assert.IsNotEmpty(entry.interaction_entry, entry.id);
                Assert.IsNotEmpty(entry.test_cases, entry.id);
                categories.Add(entry.content_category);
                if (entry.batch_role == "functional")
                {
                    functional++;
                    Assert.IsNotEmpty(entry.function_entry, entry.id);
                }
                else if (entry.batch_role == "decorative") decorative++;
                if ((functional + decorative) % 10 == 0) sampled++;
            }
            Assert.AreEqual(200, functional);
            Assert.AreEqual(300, decorative);
            foreach (var category in new[] { "tool", "weapon", "fishing_gear", "seed", "food", "fish_processing", "clothing", "hair", "accessory", "furniture", "ship_cabin", "collectible" })
                Assert.IsTrue(categories.Contains(category), category);
            Assert.GreaterOrEqual(sampled, 50);
            Assert.IsTrue(File.Exists(Path.Combine(Application.dataPath, "Prefabs", "D15", "d15_item_prop_placeholder.prefab")));
        }
    }
}
