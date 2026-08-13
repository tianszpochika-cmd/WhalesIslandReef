using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Whalebone.Content;
using Whalebone.Systems.D4;

namespace D15.Tests
{
    public sealed class D15AnimalBatchTests
    {
        private static string RegistryPath =>
            Path.Combine(Application.dataPath, "Resources", "ContentRegistry");

        [Test]
        public void BatchLoadsThreeHundredEntriesWithCompleteEcologyHandoff()
        {
            var registry = ContentRegistryReader.LoadFromDirectory(RegistryPath);
            var batch = registry.Entries
                .Where(entry => entry != null && entry.production_batch == "D15-A03*")
                .ToArray();

            Assert.That(batch, Has.Length.EqualTo(300));
            Assert.That(batch.Select(entry => entry.id).Distinct().Count(), Is.EqualTo(300));
            Assert.That(batch.All(entry => entry.owner_system == "EcologySystem"), Is.True);
            Assert.That(batch.All(entry => !string.IsNullOrEmpty(entry.behavior_profile)), Is.True);
            Assert.That(batch.All(entry => entry.habitat_profile != null && !string.IsNullOrEmpty(entry.habitat_profile.primary)), Is.True);
            Assert.That(batch.All(entry => entry.refresh_profile != null && entry.refresh_profile.interval_days > 0), Is.True);
            Assert.That(batch.All(entry => entry.food_chain_profile != null && !string.IsNullOrEmpty(entry.food_chain_profile.role)), Is.True);
            Assert.That(batch.All(entry => entry.danger_profile != null && !string.IsNullOrEmpty(entry.danger_profile.level)), Is.True);
            Assert.That(batch.All(entry => entry.legal_profile != null && !string.IsNullOrEmpty(entry.legal_profile.status)), Is.True);
            Assert.That(batch.All(entry => entry.performance_budget != null && entry.performance_budget.lod0_triangles > 0), Is.True);
        }

        [Test]
        public void RepresentativeSamplesExposeEcologyOwnerAndDeterministicLogicEntry()
        {
            var registry = ContentRegistryReader.LoadFromDirectory(RegistryPath);
            foreach (var id in new[] { "animal_green_turtle", "animal_domestic_chicken", "fish_mackerel_school" })
            {
                var entry = registry.Find(id);
                Assert.That(entry, Is.Not.Null, id);
                Assert.That(entry.owner_system, Is.EqualTo("EcologySystem"), id);
                Assert.That(entry.behavior_level, Does.Match("^L[0-4]$"), id);
                Assert.That(entry.test_cases, Does.Contain("D15-A03-ECOLOGY-TAKEOVER"), id);
            }

            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("mangrove", 3, 17);
            var first = ecology.Snapshot.Animals.Select(animal => animal.Behavior).ToArray();
            ecology.Tick(4, 17, false);
            var second = ecology.Snapshot.Animals.Select(animal => animal.Behavior).ToArray();
            Assert.That(first.Length, Is.GreaterThan(0));
            Assert.That(second.Length, Is.EqualTo(first.Length));
        }
    }
}
