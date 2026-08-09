using NUnit.Framework;
using Whalebone.Systems.D4;

namespace D4.Tests
{
    public class D4CombatTests
    {
        [Test]
        public void Combat_RequiresReloadAndTracksHull()
        {
            var combat = new D4CombatSystem();
            combat.BeginEncounter("enemy_test", 100, 100, 4, 8, 80, 80, 2, 3, 30f, 0.2f);
            var rejected = combat.Fire(3);
            Assert.IsFalse(rejected.Accepted);
            combat.SelectWeapon(D4WeaponId.RoundShot);
            Assert.AreEqual(4, combat.Reload(4));
            var result = combat.Fire(4, true);
            Assert.IsTrue(result.Accepted);
            Assert.LessOrEqual(combat.Snapshot.EnemyHull, 80);
            Assert.AreEqual(0, combat.Snapshot.LoadedCount);
        }

        [Test]
        public void Boarding_WinsWithCrewAdvantage()
        {
            var combat = new D4CombatSystem();
            combat.BeginEncounter("enemy_crew", 100, 100, 2, 20, 100, 100, 2, 1, 20f, 0f);
            var result = combat.Board();
            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(D4CombatStatus.Victory, combat.Snapshot.Status);
        }

        [Test]
        public void Escape_LeavesCombatInTerminalState()
        {
            var combat = new D4CombatSystem();
            combat.BeginEncounter("enemy_escape", 100, 100, 2, 4, 100, 100, 2, 4, 30f, 1f);
            var result = combat.Escape();
            Assert.IsTrue(result.Accepted);
            Assert.IsTrue(combat.Snapshot.Status == D4CombatStatus.Escaped || combat.Snapshot.Status == D4CombatStatus.Defeat);
        }
    }
}
