using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D5;
using Whalebone.Systems.Save;

namespace D5.Tests
{
    public class D5SaveSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d5_save_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void D5Snapshot_SaveAndLoadRestoresReleaseState()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D5 Captain" });
            var snapshot = new D5RuntimeSnapshot();
            var world = new D5WorldSystem(snapshot.World);
            world.DiscoverAll();
            world.VisitIsland("havana");
            snapshot.World = world.Snapshot;
            snapshot.Character = new D5CharacterSystem().Snapshot;
            snapshot.PlayerTraitIds = D5TraitSystem.GenerateTraitIds("save-test", 4);
            snapshot.Performance = new D5PerformanceSystem().Snapshot;
            var steam = new D5SteamSystem();
            steam.UnlockAchievement("discover_all_islands");
            steam.QueueCloudUpload();
            snapshot.Steam = steam.Snapshot;
            snapshot.Language = "en";

            SaveSystem.SaveD5Runtime(snapshot);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD5Runtime();
            Assert.IsNotNull(restored);
            Assert.AreEqual("havana", restored.World.CurrentIslandId);
            Assert.AreEqual(12, restored.World.DiscoveredIslands.Count);
            Assert.AreEqual("en", restored.Language);
            Assert.Contains("discover_all_islands", restored.Steam.UnlockedAchievements);
            Assert.AreEqual(4, restored.PlayerTraitIds.Count);
        }

        [Test]
        public void OldSaveWithoutD5Snapshot_RemainsCompatible()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "Legacy" });
            Assert.IsNull(SaveSystem.LoadD5Runtime());
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            Assert.IsNull(GameState.CurrentD5Runtime);
        }
    }
}
