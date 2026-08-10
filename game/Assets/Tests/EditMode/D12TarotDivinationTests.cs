using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D12TarotDivinationTests
    {
        private static D12TarotSessionRequest SessionRequest(string actionId = "session")
        {
            return new D12TarotSessionRequest
            {
                DivinerId = "diviner_saint_marys_keeper",
                QueryTag = "weather",
                ActorRef = "player",
                LocationRef = "island_saint_marys",
                WorldDay = 12,
                Seed = 4242,
                SaveRevision = 7,
                WarningAcknowledged = true,
                TrustSourceRef = "test:d12-a06",
                TrustEvidenceRef = "test:evidence:session",
                ActionId = actionId,
            };
        }

        [Test]
        public void CardsAreDeterministicAndExplicitlyNonSupernatural()
        {
            var firstLog = new EventLog();
            var secondLog = new EventLog();
            var first = new D12TarotSystem(firstLog).StartSession(SessionRequest());
            var second = new D12TarotSystem(secondLog).StartSession(SessionRequest());

            Assert.IsTrue(first.Succeeded, first.Message);
            Assert.IsTrue(second.Succeeded, second.Message);
            CollectionAssert.AreEqual(first.Cards.Select(card => card.CardId).ToArray(), second.Cards.Select(card => card.CardId).ToArray());
            CollectionAssert.AreEqual(first.Cards.Select(card => card.Orientation).ToArray(), second.Cards.Select(card => card.Orientation).ToArray());
            Assert.IsTrue(first.SoftClues.All(clue => clue.Status == D12TarotClueStatus.Unverified));
            Assert.AreEqual("false", firstLog.Find(first.FactEventId, 7).Result.Values["supernatural"]);
            Assert.AreEqual("true", firstLog.Find(first.FactEventId, 7).Result.Values["world_mutation"] == "false" ? "true" : "false");
        }

        [Test]
        public void SensitiveReligiousContentRequiresWarningAndSupportsSkip()
        {
            var system = new D12TarotSystem(new EventLog());
            var request = SessionRequest("warning");
            request.WarningAcknowledged = false;
            var warning = system.StartSession(request);
            Assert.IsTrue(warning.WarningRequired);
            Assert.AreEqual("warning_required", warning.Code);
            Assert.AreEqual(0, system.Snapshot.Sessions.Count);
            request.SkipSensitiveContent = true;
            var skipped = system.StartSession(request);
            Assert.IsTrue(skipped.Succeeded);
            Assert.IsTrue(skipped.Skipped);
            Assert.AreEqual(0, skipped.Cards.Count);
        }

        [Test]
        public void ExplanationIsTextOnlyAndTrustHasSourceAndEvent()
        {
            var log = new EventLog();
            var system = new D12TarotSystem(log);
            var session = system.StartSession(SessionRequest("explanation"));
            var cardsBefore = session.Cards.Select(card => card.CardId).ToArray();
            var attached = system.AttachExplanation(new D12TarotExplanationInput
            {
                SessionId = session.SessionId,
                Provider = "llm_text_only",
                ExplanationText = "只描述观察方向，不裁决天气或奖励。",
                SaveRevision = 7,
                ActionId = "explanation-text",
            });

            Assert.IsTrue(attached.Succeeded, attached.Message);
            Assert.AreEqual("llm_text_only", system.Snapshot.Sessions[0].ExplanationProvider);
            CollectionAssert.AreEqual(cardsBefore, system.Snapshot.Sessions[0].Cards.Select(card => card.CardId).ToArray());
            var explanationFact = log.Find(attached.FactEventId, 7);
            Assert.AreEqual("false", explanationFact.Result.Values["affects_state"]);
            Assert.AreEqual("true", explanationFact.Result.Values["llm_text_only"]);

            var trustEvent = system.RecordTrustEvent(new D12TarotTrustEventRequest
            {
                DivinerId = "diviner_saint_marys_keeper",
                Kind = D12TarotTrustEventKind.SessionRespect,
                SourceRef = "test:conversation",
                EvidenceRef = "event:test:conversation:01",
                WorldDay = 12,
                SaveRevision = 7,
                ActionId = "trust-source-event",
            });
            Assert.IsTrue(trustEvent.Succeeded, trustEvent.Message);
            Assert.AreEqual("test:conversation", trustEvent.Trust.SourceRef);
            Assert.AreEqual("event:test:conversation:01", trustEvent.Trust.EvidenceRef);
            Assert.IsNotNull(log.Find(trustEvent.FactEventId, 7));
        }

        [Test]
        public void VerificationAndErrorConsequenceAreTraceable()
        {
            var log = new EventLog();
            var system = new D12TarotSystem(log);
            var session = system.StartSession(SessionRequest("verification"));
            var confirmedClue = session.SoftClues[0];
            var confirmed = system.Verify(new D12TarotVerificationRequest
            {
                SessionId = session.SessionId, ClueId = confirmedClue.ClueId, Method = D12TarotVerificationMethod.EventRecord,
                FactEventId = session.FactEventId, SourceRef = "test:event-record", EvidenceRef = "event:evidence:confirmed",
                WorldDay = 12, SaveRevision = 7, ActionId = "verify-confirmed",
            });
            Assert.IsTrue(confirmed.Succeeded, confirmed.Message);
            Assert.AreEqual(D12TarotClueStatus.Confirmed, confirmed.Outcome);
            var noError = system.RecordConsequence(new D12TarotConsequenceRequest
            {
                SessionId = session.SessionId, ClueId = confirmedClue.ClueId, VerificationId = confirmed.VerificationId,
                FollowedAdvice = true, WorldDay = 12, SaveRevision = 7, ActionId = "consequence-confirmed",
            });
            Assert.IsTrue(noError.Succeeded, noError.Message);
            Assert.AreEqual("none", noError.Consequence.CostBand);

            var unresolvedClue = session.SoftClues[1];
            var unresolved = system.Verify(new D12TarotVerificationRequest
            {
                SessionId = session.SessionId, ClueId = unresolvedClue.ClueId, Method = D12TarotVerificationMethod.EventRecord,
                FactEventId = "event:not-in-log", SourceRef = "test:missing-fact", EvidenceRef = "evidence:missing-fact",
                WorldDay = 12, SaveRevision = 7, ActionId = "verify-unresolved",
            });
            Assert.AreEqual(D12TarotClueStatus.Unresolved, unresolved.Outcome);
            var error = system.RecordConsequence(new D12TarotConsequenceRequest
            {
                SessionId = session.SessionId, ClueId = unresolvedClue.ClueId, VerificationId = unresolved.VerificationId,
                FollowedAdvice = true, WorldDay = 12, SaveRevision = 7, ActionId = "consequence-unresolved",
            });
            Assert.IsTrue(error.Succeeded, error.Message);
            Assert.AreEqual("uncertainty_cost_recorded", error.Consequence.ImpactTag);
            Assert.AreEqual("evidence_delay", error.Consequence.CostBand);
            Assert.AreEqual("caller_domain_system", error.Consequence.DownstreamOwner);
            Assert.IsNotNull(log.Find(error.Consequence.EventId, 7));
        }

        [Test]
        public void SaveRestoreDoesNotRedrawOrDuplicateVerification()
        {
            var log = new EventLog();
            var system = new D12TarotSystem(log);
            var request = SessionRequest("restore");
            var session = system.StartSession(request);
            var snapshot = system.Snapshot;
            var restored = new D12TarotSystem(EventLog.FromSnapshot(log.CaptureSnapshot()), snapshot);

            var duplicate = restored.StartSession(request);
            Assert.IsTrue(duplicate.Succeeded);
            Assert.IsTrue(duplicate.Duplicate);
            CollectionAssert.AreEqual(session.Cards.Select(card => card.CardId).ToArray(), duplicate.Cards.Select(card => card.CardId).ToArray());
            var clue = duplicate.SoftClues[0];
            var verified = restored.Verify(new D12TarotVerificationRequest
            {
                SessionId = duplicate.SessionId, ClueId = clue.ClueId, Method = D12TarotVerificationMethod.EventRecord,
                FactEventId = duplicate.FactEventId, SourceRef = "test:restore", EvidenceRef = "event:evidence:restore",
                WorldDay = 12, SaveRevision = 7, ActionId = "verify-restore",
            });
            var duplicateVerification = restored.Verify(new D12TarotVerificationRequest
            {
                SessionId = duplicate.SessionId, ClueId = clue.ClueId, Method = D12TarotVerificationMethod.EventRecord,
                FactEventId = duplicate.FactEventId, SourceRef = "test:restore", EvidenceRef = "event:evidence:restore",
                WorldDay = 12, SaveRevision = 7, ActionId = "verify-restore",
            });
            Assert.IsTrue(verified.Succeeded);
            Assert.IsTrue(duplicateVerification.Duplicate);
            Assert.AreEqual(1, restored.Snapshot.Verifications.Count);
        }

        [Test]
        public void SaveSystemRoundTripPreservesTarotSnapshot()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_tarot_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D12 塔罗存档测试" });
                var system = new D12TarotSystem(new EventLog());
                var session = system.StartSession(SessionRequest("save-roundtrip"));
                Assert.IsTrue(session.Succeeded, session.Message);
                SaveSystem.SaveD12Runtime(new D12RuntimeSnapshot { MapDiscovery = new D12MapDiscoverySnapshot(), Tarot = system.Snapshot });
                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loaded = SaveSystem.LoadD12Runtime();
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.Tarot);
                Assert.AreEqual(1, loaded.Tarot.Sessions.Count);
                Assert.AreEqual(3, loaded.Tarot.Sessions[0].Cards.Count);
                var restored = new D12TarotSystem(new EventLog(), loaded.Tarot);
                var duplicate = restored.StartSession(SessionRequest("save-roundtrip"));
                Assert.IsTrue(duplicate.Duplicate);
            }
            finally
            {
                SaveSystem.DbPath = null;
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
