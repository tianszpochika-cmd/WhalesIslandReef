using System;
using System.Collections.Generic;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static D9TreasureLandingRequest Landing(D9TreasureRouteChoice route)
    {
        return new D9TreasureLandingRequest
        {
            ActorRef = "player:captain",
            LocationRef = D9TreasureQuestCatalog.SiteRef,
            RouteChoice = route,
            EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            TideWindowOpen = route == D9TreasureRouteChoice.WaitForTide,
            VillageGuideAvailable = route == D9TreasureRouteChoice.VillageGuide,
        };
    }

    public static void Main()
    {
        var log = new EventLog();
        var system = new D9TreasureQuestSystem(log);
        Check(system.BuildClueExpressionProposal("crew:isaac_reed", "表达").ReliabilityPercent == 72, "proposal");
        Check(system.HearRumor("player:captain", 1, 1).Accepted, "rumor");
        Check(system.AcceptRumor().Accepted, "accept");

        var failed = system.TryLand(new D9TreasureLandingRequest
        {
            ActorRef = "player:captain",
            LocationRef = D9TreasureQuestCatalog.SiteRef,
            RouteChoice = D9TreasureRouteChoice.DirectLanding,
            EquipmentIds = new List<string>(),
        }, 2, 2);
        Check(failed.Fact.OutcomeStatus == WorldEventStatus.Failed, "equipment failure");
        Check(system.Recover(new D9TreasureRecoveryRequest
        {
            ActorRef = "player:captain",
            RouteId = "route_buy_chart_kit",
            EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
        }, 3, 3).Accepted, "equipment recovery");
        Check(system.TryLand(Landing(D9TreasureRouteChoice.WaitForTide), 4, 4).RewardRequest != null, "discovery");

        var envelope = system.CaptureSaveEnvelope();
        var restored = D9TreasureQuestSystem.RestoreFromSave(envelope);
        Check(restored.EventLog.Count == 4, "event restore");
        Check(restored.Snapshot.Status == D9TreasureQuestStatus.Discovered, "quest restore");
        Check(!restored.TryLand(Landing(D9TreasureRouteChoice.WaitForTide), 5, 4).Accepted, "duplicate discovery");
        Console.WriteLine("D9-A06 smoke PASS: success/failure/recovery/save-restore/duplicate guard");
    }
}
