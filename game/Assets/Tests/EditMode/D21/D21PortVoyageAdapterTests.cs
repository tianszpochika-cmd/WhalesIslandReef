// D21PortVoyageAdapterTests —— D21-L22 港口出航/返港适配器纯逻辑测试
//
// D21 只走 D19PortCommandRouter 的公开入口；泊位占用、访问状态与补偿草案
// 均由 D19 owner 返回并断言，测试不读写其私有集合。

using NUnit.Framework;
using Whalebone.Systems.D19;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21PortVoyageAdapterTests
    {
        private const string PortId = "port_santa_margarita";
        private const string BerthId = "berth_santa_margarita_dock_01";
        private const string ShipId = "ship_l22_01";
        private const string ActorId = "actor_l22_01";

        [Test]
        public void Departure_RoutesThroughD19AndSameCommandIsIdempotent()
        {
            D19PortCommandRouter router = CreateBoardedRouter();
            var adapter = new D21PortDepartureAdapter(router);
            var request = new D21PortDepartureRequest(
                "l22-departure",
                ActorId,
                ShipId,
                PortId,
                1,
                1,
                Claims(),
                "route:l22:main",
                "port:green_cay",
                BerthId);

            D21PortDepartureResult first = adapter.Depart(request);
            Assert.That(first.Success, Is.True);
            Assert.That(first.VoyageStarted, Is.True);
            Assert.That(first.Applied, Is.True);
            Assert.That(first.IsDuplicate, Is.False);
            Assert.That(first.PortStateAfter, Is.EqualTo(D19PortVisitState.Departed));
            Assert.That(router.VisitSystem.State, Is.EqualTo(D19PortVisitState.Departed));
            Assert.That(first.VoyageStart, Is.Not.Null);
            Assert.That(first.VoyageStart.RouteId, Is.EqualTo("route:l22:main"));

            D21PortDepartureResult replay = adapter.Depart(request.Clone());
            Assert.That(replay.Success, Is.True);
            Assert.That(replay.IsDuplicate, Is.True);
            Assert.That(replay.Applied, Is.False);
            Assert.That(router.VisitSystem.State, Is.EqualTo(D19PortVisitState.Departed));
        }

        [Test]
        public void Departure_InvalidRequestDoesNotTouchD19State()
        {
            D19PortCommandRouter router = CreateBoardedRouter();
            var adapter = new D21PortDepartureAdapter(router);

            D21PortDepartureResult rejected = adapter.Depart(new D21PortDepartureRequest
            {
                ActorId = ActorId,
                ShipId = ShipId,
                PortId = PortId,
                WorldDay = 1,
                SaveRevision = 1,
                Claims = Claims(),
            });
            Assert.That(rejected.Success, Is.False);
            Assert.That(rejected.FailureCode, Is.EqualTo(D21PortDepartureFailureCodes.CommandIdRequired));
            Assert.That(router.VisitSystem.State, Is.EqualTo(D19PortVisitState.Boarded));
        }

        [Test]
        public void Return_WithAvailableBerthEndsVoyageAndUpdatesD19Visit()
        {
            D19PortCommandRouter router = CreateDepartedRouter();
            var adapter = new D21PortReturnAdapter(router);
            var request = new D21PortReturnRequest(
                "l22-return",
                ActorId,
                ShipId,
                PortId,
                2,
                2,
                Claims(),
                BerthId,
                "route:l22:main");

            D21PortReturnResult result = adapter.Return(request);
            Assert.That(result.Success, Is.True);
            Assert.That(result.VoyageEnded, Is.True);
            Assert.That(result.ReturnApplied, Is.True);
            Assert.That(result.BerthRequested, Is.True);
            Assert.That(result.BerthApplied, Is.True);
            Assert.That(result.BerthIsDuplicate, Is.False);
            Assert.That(result.ReturnResult.Success, Is.True);
            Assert.That(result.BerthResult.Success, Is.True);
            Assert.That(result.PortStateAfter, Is.EqualTo(D19PortVisitState.Berthed));
            Assert.That(router.VisitSystem.State, Is.EqualTo(D19PortVisitState.Berthed));
            Assert.That(router.VisitSystem.BerthId, Is.EqualTo(BerthId));
            Assert.That(router.BerthSystem.GetAllocation(BerthId).ShipId, Is.EqualTo(ShipId));
        }

        [Test]
        public void Return_BerthFailurePreservesReturnAndExposesCompensationDraft()
        {
            D19PortAnchorCatalog catalog = D19PortAnchorCatalog.Instance;
            var berthSystem = new D19BerthSystem(catalog);
            D19BerthResult reserved = berthSystem.Reserve(
                "l22-block-reserve",
                PortId,
                BerthId,
                new D19BerthShipProfile("ship_l22_blocker"),
                1,
                4,
                1);
            Assert.That(reserved.Success, Is.True);
            D19BerthResult occupied = berthSystem.ConfirmOccupancy(
                "l22-block-occupy", PortId, BerthId, "ship_l22_blocker", 1, 1);
            Assert.That(occupied.Success, Is.True);

            D19PortCommandRouter router = CreateDepartedRouter(berthSystem);
            var adapter = new D21PortReturnAdapter(router);
            D21PortReturnResult result = adapter.Return(new D21PortReturnRequest(
                "l22-return-berth-fail",
                ActorId,
                ShipId,
                PortId,
                2,
                2,
                Claims(),
                BerthId,
                "route:l22:main"));

            Assert.That(result.Success, Is.False);
            Assert.That(result.VoyageEnded, Is.True);
            Assert.That(result.ReturnApplied, Is.True);
            Assert.That(result.BerthRequested, Is.True);
            Assert.That(result.BerthApplied, Is.False);
            Assert.That(result.ReturnResult.Success, Is.True);
            Assert.That(result.BerthResult.Success, Is.False);
            Assert.That(result.BerthResult.CompensationDraft, Is.Not.Null);
            Assert.That(result.BerthResult.CompensationDraft.OriginalCommandId,
                Is.EqualTo("l22-return-berth-fail:berth"));
            Assert.That(result.PortStateAfter, Is.EqualTo(D19PortVisitState.Returned));
            Assert.That(router.VisitSystem.State, Is.EqualTo(D19PortVisitState.Returned));
            Assert.That(berthSystem.GetAllocation(BerthId).ShipId, Is.EqualTo("ship_l22_blocker"));
        }

        private static D19PortCommandRouter CreateBoardedRouter()
        {
            D19PortAnchorCatalog catalog = D19PortAnchorCatalog.Instance;
            var visitSystem = new D19PortVisitSystem(ShipId);
            var berthSystem = new D19BerthSystem(catalog);
            var router = new D19PortCommandRouter(
                catalog, visitSystem, berthSystem, new D19CommandIdempotency(ShipId));

            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-enter", D19PortCommandKind.EnterPort, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-dock", D19PortCommandKind.Dock, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-board", D19PortCommandKind.Board, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            return router;
        }

        private static D19PortCommandRouter CreateDepartedRouter(D19BerthSystem berthSystem = null)
        {
            D19PortCommandRouter router = CreateBoardedRouter(berthSystem);
            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-depart-prep", D19PortCommandKind.Depart, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            return router;
        }

        private static D19PortCommandRouter CreateBoardedRouter(D19BerthSystem berthSystem)
        {
            D19PortAnchorCatalog catalog = D19PortAnchorCatalog.Instance;
            var visitSystem = new D19PortVisitSystem(ShipId);
            var router = new D19PortCommandRouter(
                catalog,
                visitSystem,
                berthSystem ?? new D19BerthSystem(catalog),
                new D19CommandIdempotency(ShipId));

            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-enter", D19PortCommandKind.EnterPort, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-dock", D19PortCommandKind.Dock, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            Assert.That(router.Route(new D19RoutedPortCommand(
                "l22-board", D19PortCommandKind.Board, ActorId, ShipId, PortId,
                worldDay: 1, saveRevision: 1, claims: Claims())).Success, Is.True);
            return router;
        }

        private static D19AuthorityClaims Claims()
        {
            return new D19AuthorityClaims(ShipId, true, true, PortId);
        }
    }
}
