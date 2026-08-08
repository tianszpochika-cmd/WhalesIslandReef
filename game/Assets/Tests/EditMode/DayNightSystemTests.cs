using NUnit.Framework;
using Whalebone.Systems.D2;

namespace D2.Tests
{
    public class DayNightSystemTests
    {
        [Test]
        public void Advance_ProgressesTimeAndRollsDay()
        {
            var clock = new DayNightSystem(10f, new DayNightSnapshot(2, 0.9f));
            clock.Advance(2f);
            var state = clock.Snapshot;
            Assert.AreEqual(3, state.DayIndex);
            Assert.AreEqual(0.1f, state.NormalizedTime, 0.0001f);
        }

        [Test]
        public void PausedClock_DoesNotAdvance()
        {
            var clock = new DayNightSystem(10f, new DayNightSnapshot(1, 0.4f, true));
            clock.Advance(20f);
            Assert.AreEqual(1, clock.Snapshot.DayIndex);
            Assert.AreEqual(0.4f, clock.Snapshot.NormalizedTime, 0.0001f);
        }

        [Test]
        public void ApplySnapshot_NormalizesLegacyValues()
        {
            var clock = new DayNightSystem();
            clock.ApplySnapshot(new DayNightSnapshot(-1, -0.25f));
            Assert.AreEqual(0, clock.Snapshot.DayIndex);
            Assert.AreEqual(0.75f, clock.Snapshot.NormalizedTime, 0.0001f);
        }
    }
}
