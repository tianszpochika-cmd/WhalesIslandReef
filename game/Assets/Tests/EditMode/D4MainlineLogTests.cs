using NUnit.Framework;
using Whalebone.Systems.D4;

namespace D4.Tests
{
    public class D4MainlineLogTests
    {
        [Test]
        public void Mainline_AdvancesThreeChaptersWithTeachingFlags()
        {
            var factions = new D4FactionSystem();
            var mainline = new D4MainlineSystem();
            for (int i = 0; i < 5; i++)
            {
                var result = mainline.TryAdvance(factions.Snapshot);
                Assert.IsTrue(result.Advanced, result.Message);
            }
            factions.ApplyReputation("england", 20, "test");
            while (mainline.CurrentMission != null)
            {
                var result = mainline.TryAdvance(factions.Snapshot);
                Assert.IsTrue(result.Advanced, result.Message);
            }
            Assert.IsTrue(mainline.GetFlag("mainline_complete"));
            Assert.AreEqual(9, mainline.Snapshot.CompletedMissions.Count);
        }

        [Test]
        public void RouteLog_AppendsFactsAndBuildsSummary()
        {
            var log = new D4LogSystem();
            log.Append(1, D4LogKind.Voyage, "离开港口", "santa_margarita", "green_cay");
            log.Append(2, D4LogKind.Combat, "炮战胜利");
            log.Append(3, D4LogKind.Medical, "老水手接受治疗");
            Assert.AreEqual(3, log.Entries.Count);
            StringAssert.Contains("炮战胜利", log.BuildSummary(2));
            Assert.AreEqual("log_0001", log.Entries[0].Id);
        }
    }
}
