using NUnit.Framework;
using Whalebone.Systems.D2;

namespace D2.Tests
{
    public class D2ContractTests
    {
        [Test]
        public void SailingInput_ClampsInvalidValues()
        {
            var input = new SailingInput(4f, -3f, true);
            Assert.AreEqual(1f, input.Throttle);
            Assert.AreEqual(-1f, input.Rudder);
            Assert.IsTrue(input.Brake);
        }

        [Test]
        public void SailingSnapshot_CloneIsIndependent()
        {
            var source = new SailingSnapshot
            {
                ShipId = "ship-1",
                PositionX = 3f,
                PositionZ = -2f,
                Heading = 90f,
                Speed = 1.5f,
                MaxSpeed = 5f,
                ControlMode = D2ControlMode.Ship,
            };

            var copy = source.Clone();
            copy.PositionX = 99f;
            copy.ControlMode = D2ControlMode.Locked;

            Assert.AreEqual(3f, source.PositionX);
            Assert.AreEqual(D2ControlMode.Ship, source.ControlMode);
            Assert.AreEqual("ship-1", copy.ShipId);
        }

        [Test]
        public void DayNightSnapshot_NormalizesTimeAndDay()
        {
            var state = new DayNightSnapshot(-2, 2.25f);
            Assert.AreEqual(0, state.DayIndex);
            Assert.AreEqual(0.25f, state.NormalizedTime, 0.0001f);
        }

        [Test]
        public void RuntimeSnapshot_CloneKeepsBothDomains()
        {
            var state = new D2RuntimeSnapshot
            {
                Sailing = new SailingSnapshot { ShipId = "s", PositionZ = 7f },
                DayNight = new DayNightSnapshot(3, 0.5f, true),
            };

            var copy = state.Clone();
            copy.Sailing.PositionZ = 11f;
            copy.DayNight = new DayNightSnapshot(4, 0.75f);

            Assert.AreEqual(7f, state.Sailing.PositionZ);
            Assert.AreEqual(3, state.DayNight.DayIndex);
            Assert.IsTrue(state.DayNight.Paused);
        }
    }
}
