using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D5;

namespace D5.Tests
{
    public class D5ContentTests
    {
        [Test]
        public void Catalogs_HaveReleaseBaselineContent()
        {
            Assert.AreEqual(12, D5IslandCatalog.All.Count);
            Assert.AreEqual(50, D5TraitCatalog.All.Count);
            Assert.AreEqual(15, D5TraitCatalog.Count(D5TraitCategory.Body));
            Assert.AreEqual(20, D5TraitCatalog.Count(D5TraitCategory.Personality));
            Assert.AreEqual(10, D5TraitCatalog.Count(D5TraitCategory.Background));
            Assert.AreEqual(5, D5TraitCatalog.Count(D5TraitCategory.Disease));
            Assert.AreEqual(8, D5CharacterCatalog.All.Count);
            Assert.AreEqual(8, D5AchievementCatalog.All.Count);
        }

        [Test]
        public void CatalogIds_AreUnique_AndLocalized()
        {
            var ids = new HashSet<string>();
            foreach (var island in D5IslandCatalog.All)
            {
                Assert.IsTrue(ids.Add(island.Id));
                Assert.IsNotEmpty(D5LocalizationSystem.Get("island_" + island.Id, "zh"));
                Assert.IsNotEmpty(D5LocalizationSystem.Get("island_" + island.Id, "en"));
            }
            ids.Clear();
            foreach (var trait in D5TraitCatalog.All)
            {
                Assert.IsTrue(ids.Add(trait.Id));
                Assert.IsNotEmpty(D5LocalizationSystem.Get("trait_" + trait.Id, "zh"));
                Assert.IsNotEmpty(D5LocalizationSystem.Get("trait_" + trait.Id, "en"));
            }
        }

        [Test]
        public void TraitGeneration_IsDeterministicAndBounded()
        {
            var a = D5TraitSystem.GenerateTraitIds("player_01", 4);
            var b = D5TraitSystem.GenerateTraitIds("player_01", 4);
            CollectionAssert.AreEqual(a, b);
            Assert.AreEqual(4, a.Count);
            Assert.IsTrue(D5TraitSystem.IsValidTraitSet(a, 3, 4));
            Assert.IsFalse(D5TraitSystem.IsValidTraitSet(new[] { "strong", "strong" }, 0, 4));
        }

        [Test]
        public void World_MapsAndVisitsAllIslands()
        {
            var world = new D5WorldSystem();
            Assert.AreEqual(3, world.Snapshot.DiscoveredIslands.Count);
            while (world.Snapshot.DiscoveredIslands.Count < 12)
                Assert.IsTrue(world.DiscoverNext().Accepted);
            Assert.AreEqual(12, world.Snapshot.DiscoveredIslands.Count);
            Assert.IsTrue(world.VisitIsland("jaguar_isle").Accepted);
            Assert.AreEqual("jaguar_isle", world.Snapshot.CurrentIslandId);
        }

        [Test]
        public void Character_PerformanceAndSteamSystemsUpdateTheirOwnState()
        {
            var character = new D5CharacterSystem();
            Assert.IsTrue(character.ApplyPreset("privateer"));
            Assert.AreEqual("privateer", character.Snapshot.PresetId);

            var performance = new D5PerformanceSystem();
            performance.SetQuality(D5QualityLevel.High);
            for (int i = 0; i < 5; i++) performance.RecordFrame(0.016f);
            Assert.IsTrue(performance.Report().MeetsTarget);
            Assert.AreEqual(D5QualityLevel.High, performance.Snapshot.Quality);

            var steam = new D5SteamSystem();
            Assert.IsTrue(steam.UnlockAchievement("customized_captain"));
            Assert.IsFalse(steam.UnlockAchievement("not_real"));
            int revision = steam.QueueCloudUpload();
            Assert.Greater(revision, 0);
            Assert.IsTrue(steam.MarkCloudSynced(revision));
            Assert.AreEqual(0, steam.Snapshot.PendingCloudRevision);
        }

        [Test]
        public void Steam_DefaultsToOfflineAndKeepsCloudWorkLocal()
        {
            var steam = new D5SteamSystem();
            var initial = steam.Snapshot;

            Assert.AreEqual("UNCONFIGURED", initial.AppId);
            Assert.IsTrue(initial.OfflineMode);
            Assert.IsTrue(initial.CloudEnabled);
            Assert.AreEqual("never", initial.LastSync);
            Assert.AreEqual("Steam Bridge 离线安全模式", steam.StatusText());

            int revision = steam.QueueCloudUpload();
            var queued = steam.Snapshot;
            Assert.AreEqual(1, revision);
            Assert.AreEqual(1, queued.PendingCloudRevision);
            Assert.AreEqual("pending", queued.LastSync);
            Assert.IsTrue(queued.OfflineMode);
            Assert.AreEqual("UNCONFIGURED", queued.AppId);
            Assert.AreEqual("Steam Bridge 离线安全模式", steam.StatusText());

            Assert.IsTrue(steam.UnlockAchievement("first_voyage"));
            Assert.IsTrue(steam.HasAchievement("first_voyage"));
            Assert.IsTrue(steam.MarkCloudSynced(revision));
            Assert.AreEqual(0, steam.Snapshot.PendingCloudRevision);
            Assert.AreEqual("synced:1", steam.Snapshot.LastSync);
        }

        [Test]
        public void Steam_DisabledCloudDoesNotCreateAnUploadRevision()
        {
            var steam = new D5SteamSystem(new D5SteamState
            {
                OfflineMode = true,
                CloudEnabled = false,
                PendingCloudRevision = 4,
                LastSync = "never"
            });

            Assert.AreEqual(4, steam.QueueCloudUpload());
            Assert.AreEqual(4, steam.Snapshot.PendingCloudRevision);
            Assert.AreEqual("never", steam.Snapshot.LastSync);
            Assert.AreEqual("Steam Bridge 离线安全模式", steam.StatusText());
        }

        [Test]
        public void Snapshot_CloneIsIndependent()
        {
            var snapshot = new D5RuntimeSnapshot();
            snapshot.World.DiscoveredIslands.Add("havana");
            snapshot.PlayerTraitIds.Add("strong");
            snapshot.Steam.UnlockedAchievements.Add("first_voyage");
            var clone = snapshot.Clone();
            clone.World.DiscoveredIslands.Add("tortuga");
            clone.PlayerTraitIds[0] = "weak";
            clone.Steam.UnlockedAchievements.Clear();
            Assert.IsFalse(snapshot.World.DiscoveredIslands.Contains("tortuga"));
            Assert.AreEqual("strong", snapshot.PlayerTraitIds[0]);
            Assert.AreEqual(1, snapshot.Steam.UnlockedAchievements.Count);
        }
    }
}
