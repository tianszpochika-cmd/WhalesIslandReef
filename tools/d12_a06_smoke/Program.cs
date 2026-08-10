using System;
using System.Linq;
using Whalebone.Systems.D9;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Main()
    {
        var log = new EventLog();
        var system = new D12TarotSystem(log);
        var request = new D12TarotSessionRequest
        {
            DivinerId = "diviner_saint_marys_keeper", QueryTag = "weather", ActorRef = "player", LocationRef = "island_saint_marys",
            WorldDay = 12, Seed = 4242, SaveRevision = 7, WarningAcknowledged = true, TrustSourceRef = "test:d12-a06",
            TrustEvidenceRef = "test:evidence:session", ActionId = "smoke-session",
        };
        var session = system.StartSession(request);
        Require(session.Succeeded && session.Cards.Count == 3 && session.SoftClues.Count == 3, "session");
        Require(session.ExplanationContext.IsSoftOnly && session.ExplanationContext.NoWorldMutation, "soft context");
        Require(log.Find(session.FactEventId, 7).Result.Values["supernatural"] == "false", "no supernatural");

        var before = system.Snapshot.Sessions[0].Cards.Select(card => card.CardId).ToArray();
        var explanation = system.AttachExplanation(new D12TarotExplanationInput
        {
            SessionId = session.SessionId, Provider = "llm_text_only", ExplanationText = "这只是待验证的观察提示。",
            SaveRevision = 7, ActionId = "smoke-explanation",
        });
        Require(explanation.Succeeded && system.Snapshot.Sessions[0].Cards.Select(card => card.CardId).SequenceEqual(before), "explanation isolation");

        var clue = session.SoftClues.First(clueItem => clueItem.TargetRef.IndexOf("weather:", StringComparison.Ordinal) < 0);
        var verification = system.Verify(new D12TarotVerificationRequest
        {
            SessionId = session.SessionId, ClueId = clue.ClueId, Method = D12TarotVerificationMethod.WeatherObservation,
            Weather = D9WeatherSeaSnapshot.CreateDefault(), SourceRef = "test:d9-weather", EvidenceRef = "test:evidence:weather",
            WorldDay = 12, SaveRevision = 7, ActionId = "smoke-verification",
        });
        Require(verification.Succeeded && verification.Outcome != D12TarotClueStatus.Unverified, "verification");
        var consequence = system.RecordConsequence(new D12TarotConsequenceRequest
        {
            SessionId = session.SessionId, ClueId = clue.ClueId, VerificationId = verification.VerificationId,
            FollowedAdvice = true, WorldDay = 12, SaveRevision = 7, ActionId = "smoke-consequence",
        });
        var expectedCost = verification.Outcome == D12TarotClueStatus.Contradicted ? "recheck_time" : verification.Outcome == D12TarotClueStatus.Unresolved ? "evidence_delay" : "none";
        Require(consequence.Succeeded && consequence.Consequence.CostBand == expectedCost, "consequence");
        Require(system.ReadDivinerTrust("diviner_saint_marys_keeper").LastEventId == verification.FactEventId, "trust event");

        var restoredLog = EventLog.FromSnapshot(log.CaptureSnapshot());
        var restoredSystem = new D12TarotSystem(restoredLog, system.Snapshot);
        var duplicate = restoredSystem.StartSession(request);
        Require(duplicate.Succeeded && duplicate.Duplicate && duplicate.Cards.Count == 3, "restore duplicate");
        Console.WriteLine("D12-A06 smoke passed: cards, warning boundary, explanation isolation, verification, consequence, trust and restore idempotence.");
    }

    private static void Require(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("D12-A06 smoke failed: " + name);
    }
}
