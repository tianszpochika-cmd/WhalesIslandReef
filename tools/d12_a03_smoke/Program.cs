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

    private static D9TreasureLandingRequest Landing(string clueId, bool verified, bool window)
    {
        return new D9TreasureLandingRequest
        {
            ActorRef = "player:captain",
            LocationRef = D9TreasureQuestCatalog.SiteRef,
            ClueId = clueId,
            RouteChoice = D9TreasureRouteChoice.WaitForTide,
            TideWindowOpen = true,
            RequireCrossValidation = verified,
            EnforceClueWindow = window,
            EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
        };
    }

    public static void Main()
    {
        var log = new EventLog();
        var system = new D9TreasureQuestSystem(log);
        Check(system.HearRumor("player:captain", 1, 1).Accepted, "rumor");
        Check(system.AcceptRumor().Accepted, "accept");
        Check(system.RecordCatalogClue("clue:first_chart_salvaged_chart", "player:captain", 2, 2).Accepted, "chart clue");
        Check(system.CrossValidateClues(new D9TreasureClueVerificationRequest
        {
            ActorRef = "player:captain",
            ClueId = "clue:first_chart_tide_mark",
            OtherClueId = "clue:first_chart_salvaged_chart",
        }, 3, 3).Accepted, "cross validation");

        Check(system.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 4, 4).RewardRequest != null, "reward request");
        Check(system.Snapshot.RewardRequest.RewardType == D9TreasureRewardType.Item, "reward type");
        Check(system.Snapshot.RewardRequest.CauseEventIds.Count > 0, "reward cause");

        var restored = D9TreasureQuestSystem.RestoreFromSave(system.CaptureSaveEnvelope());
        var before = restored.EventLog.Count;
        Check(!restored.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 5, 5).Accepted, "duplicate guard");
        Check(restored.EventLog.Count == before, "duplicate event");
        Check(restored.Snapshot.Clues.Count == 2, "clue restore");
        Check(restored.Snapshot.VerifiedClueIds.Count == 2, "verification restore");
        Console.WriteLine("D12-A03 smoke PASS: multi-source clues/cross-validation/reward trace/save restore");
    }
}
