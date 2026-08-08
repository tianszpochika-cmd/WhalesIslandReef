using NUnit.Framework;
using Whalebone.Systems.D2;

namespace D2.Tests
{
    public class SailingSystemTests
    {
        [Test]
        public void CharacterMode_DoesNotMoveShip()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.Tick(new SailingInput(1f, 0f), 1f);
            Assert.AreEqual(0f, sailing.Snapshot.Speed);
            Assert.AreEqual(0f, sailing.Snapshot.PositionZ);
        }

        [Test]
        public void ShipMode_AcceleratesAndMovesForward()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.SetControlMode(D2ControlMode.Ship);
            sailing.Tick(new SailingInput(1f, 0f), 1f);
            var snapshot = sailing.Snapshot;
            Assert.AreEqual(2.5f, snapshot.Speed, 0.0001f);
            Assert.Greater(snapshot.PositionZ, 0f);
        }

        [Test]
        public void ShipMode_ClampsSpeedAndBrakeStops()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.SetControlMode(D2ControlMode.Ship);
            sailing.Tick(new SailingInput(1f, 0f), 10f);
            Assert.AreEqual(4f, sailing.Snapshot.Speed, 0.0001f);
            sailing.Tick(new SailingInput(0f, 0f, true), 2f);
            Assert.AreEqual(0f, sailing.Snapshot.Speed, 0.0001f);
        }

        [Test]
        public void ShipMode_RudderChangesHeading()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.SetControlMode(D2ControlMode.Ship);
            sailing.Tick(new SailingInput(1f, 1f), 1f);
            Assert.Greater(sailing.Snapshot.Heading, 0f);
        }

        [Test]
        public void LockedMode_StopsAndIgnoresInput()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.SetControlMode(D2ControlMode.Ship);
            sailing.Tick(new SailingInput(1f, 0f), 1f);
            sailing.SetControlMode(D2ControlMode.Locked);
            var before = sailing.Snapshot;
            sailing.Tick(new SailingInput(1f, 1f), 10f);
            var after = sailing.Snapshot;
            Assert.AreEqual(0f, after.Speed);
            Assert.AreEqual(before.PositionZ, after.PositionZ, 0.0001f);
        }

        [Test]
        public void ApplySnapshot_RestoresClampedRuntimeState()
        {
            var sailing = new SailingSystem("ship", 4f, 6f);
            sailing.ApplySnapshot(new SailingSnapshot
            {
                PositionX = 3f,
                PositionZ = -4f,
                Heading = 450f,
                Speed = 99f,
                ControlMode = D2ControlMode.Ship,
            });
            var snapshot = sailing.Snapshot;
            Assert.AreEqual(3f, snapshot.PositionX);
            Assert.AreEqual(-4f, snapshot.PositionZ);
            Assert.AreEqual(90f, snapshot.Heading, 0.0001f);
            Assert.AreEqual(4f, snapshot.Speed, 0.0001f);
        }
    }
}
