// D21VoyageSaveLoadTests —— D21-L23 航程节点保存/加载/继续纯逻辑测试

using NUnit.Framework;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21VoyageSaveLoadTests
    {
        [Test]
        public void VoyageNode_SaveLoadContinue_IsLogicallyEquivalentToContinuousRun()
        {
            const string voyageId = "voyage:l23:main";
            const string shipId = "ship_l23_01";
            const string routeId = "route:l23:main";
            const long voyageSeed = 90210L;

            D21VoyageTimeState initial = D21VoyageTimeState.CreateInitial(
                voyageId, shipId, 5000L, 4, 350, 0L);
            var input = new D21VoyageTimeInput
            {
                RouteDistanceMilliNm = 5000L,
                BaseSpeedMilliKnots = 1000L,
                SpeedPermille = 1000,
                CurrentPermille = 1000,
            };
            D21TickContext firstContext = D21VoyageTimeSystem.CreateTickContext(
                4, 350, 0, 0L, voyageSeed, voyageId, shipId);
            D21VoyageTimeResult first = D21VoyageTimeSystem.Advance(initial, input, firstContext);
            Assert.That(first.Accepted, Is.True);
            Assert.That(first.State.ElapsedTicks, Is.EqualTo(1));

            D21ShipSaveProjection saved = D21ShipSaveProjection.CreateDefault(shipId);
            saved.WorldDay = first.ProjectedWorldDay;
            saved.SaveRevision = 1L;
            saved.Voyage.VoyageId = voyageId;
            saved.Voyage.ShipId = shipId;
            saved.Voyage.RouteId = routeId;
            saved.Voyage.OriginId = "port:l23:origin";
            saved.Voyage.DestinationId = "port:l23:destination";
            saved.Voyage.CurrentWaypointId = "waypoint:l23:mid";
            saved.Voyage.Status = "Sailing";
            saved.Voyage.LastTickIndex = first.State.ElapsedTicks;
            saved.Voyage.Time = first.State.Clone();
            saved.ShipSnapshot.Revision = 1L;
            saved.Validate();

            string json = saved.ToJson();
            D21ShipSaveProjection loaded = D21ShipSaveProjection.FromJson(json);
            Assert.That(loaded.ComputeFingerprint(), Is.EqualTo(saved.ComputeFingerprint()));
            Assert.That(loaded.Voyage.CurrentWaypointId, Is.EqualTo("waypoint:l23:mid"));
            Assert.That(loaded.Voyage.Time.Fingerprint, Is.EqualTo(saved.Voyage.Time.Fingerprint));

            D21TickContext secondContext = D21VoyageTimeSystem.CreateTickContext(
                first.ProjectedWorldDay,
                first.ProjectedMinuteOfDay,
                1,
                1L,
                voyageSeed,
                voyageId,
                shipId);
            D21VoyageTimeResult continuous = D21VoyageTimeSystem.Advance(
                first.State, input, secondContext);
            D21VoyageTimeResult resumed = D21VoyageTimeSystem.Advance(
                loaded.Voyage.Time, input, secondContext);

            Assert.That(continuous.Accepted, Is.True);
            Assert.That(resumed.Accepted, Is.True);
            Assert.That(resumed.Code, Is.EqualTo(continuous.Code));
            Assert.That(resumed.DistanceAdvancedMilliNm, Is.EqualTo(continuous.DistanceAdvancedMilliNm));
            Assert.That(resumed.RemainingDistanceMilliNm, Is.EqualTo(continuous.RemainingDistanceMilliNm));
            Assert.That(resumed.ProjectedWorldDay, Is.EqualTo(continuous.ProjectedWorldDay));
            Assert.That(resumed.ProjectedMinuteOfDay, Is.EqualTo(continuous.ProjectedMinuteOfDay));
            Assert.That(resumed.ProjectedPhase, Is.EqualTo(continuous.ProjectedPhase));
            Assert.That(resumed.State.Fingerprint, Is.EqualTo(continuous.State.Fingerprint));
            Assert.That(resumed.State.LastSaveRevision, Is.EqualTo(1L));
        }

        [Test]
        public void VoyageTime_ExplicitTickContextRejectsSkippedNodeWithoutMutatingState()
        {
            D21VoyageTimeState state = D21VoyageTimeState.CreateInitial(
                "voyage:l23:guard", "ship_l23_guard", 1000L, 2, 600, 3L);
            D21VoyageTimeInput input = new D21VoyageTimeInput
            {
                RouteDistanceMilliNm = 1000L,
                BaseSpeedMilliKnots = 1000L,
            };
            D21TickContext skipped = D21VoyageTimeSystem.CreateTickContext(
                2, 630, 2, 3L, 4L, "voyage:l23:guard", "ship_l23_guard");
            D21VoyageTimeResult rejected = D21VoyageTimeSystem.Advance(state, input, skipped);

            Assert.That(rejected.Accepted, Is.False);
            Assert.That(rejected.Code, Is.EqualTo("tick_index_mismatch"));
            Assert.That(rejected.State.Fingerprint, Is.EqualTo(state.Fingerprint));
            Assert.That(state.ElapsedTicks, Is.EqualTo(0));
            Assert.That(state.DistanceTravelledMilliNm, Is.EqualTo(0L));
        }
    }
}
