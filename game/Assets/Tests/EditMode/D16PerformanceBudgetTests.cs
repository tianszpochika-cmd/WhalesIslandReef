using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D16;
using Whalebone.Systems.Events;

namespace Save.Tests
{
    public sealed class D16PerformanceBudgetTests
    {
        [Test]
        public void Configuration_RejectsInvalidDistanceOrder()
        {
            var configuration = new D16PerformanceBudgetConfiguration
            {
                FullSimulationDistance = 100f,
                ReducedSimulationDistance = 50f,
            };

            CollectionAssert.Contains(configuration.Validate(), "reduced_distance_must_exceed_full_distance");
        }

        [Test]
        public void ProtectedInteractionRemainsFullEvenUnderPressure()
        {
            var system = new D16PerformanceBudgetSystem();
            var snapshot = system.Evaluate(new[]
            {
                new D16PerformanceCandidate { Id = "far_interaction", Distance = 500f, HasActiveInteraction = true },
                new D16PerformanceCandidate { Id = "far_background", Distance = 500f },
            }, true);

            Assert.AreEqual(D16SimulationTier.Full, snapshot.Decisions[0].SimulationTier);
            Assert.AreEqual(D16SimulationTier.Frozen, snapshot.Decisions[1].SimulationTier);
        }

        [Test]
        public void AgentAndAnimalBudgetsAreStableAndCapped()
        {
            var configuration = new D16PerformanceBudgetConfiguration
            {
                MaxActiveAgents = 2,
                MaxActiveAnimals = 2,
            };
            var system = new D16PerformanceBudgetSystem(configuration);
            var candidates = new List<D16PerformanceCandidate>
            {
                new D16PerformanceCandidate { Id = "agent_b", IsAgent = true, Distance = 10f },
                new D16PerformanceCandidate { Id = "agent_a", IsAgent = true, Distance = 10f },
                new D16PerformanceCandidate { Id = "agent_c", IsAgent = true, Distance = 10f },
                new D16PerformanceCandidate { Id = "animal_c", IsAnimal = true, Distance = 10f },
                new D16PerformanceCandidate { Id = "animal_b", IsAnimal = true, Distance = 10f },
                new D16PerformanceCandidate { Id = "animal_a", IsAnimal = true, Distance = 10f },
            };

            var first = system.Evaluate(candidates);
            var second = system.Evaluate(candidates);

            Assert.AreEqual(2, first.ActiveAgentCount);
            Assert.AreEqual(2, first.ActiveAnimalCount);
            Assert.IsTrue(first.BudgetSatisfied);
            CollectionAssert.AreEqual(
                new[] { "agent_a", "agent_b", "agent_c", "animal_a", "animal_b", "animal_c" },
                first.Decisions.ConvertAll(item => item.Id));
            CollectionAssert.AreEqual(
                first.Decisions.ConvertAll(item => item.SimulationTier),
                second.Decisions.ConvertAll(item => item.SimulationTier));
        }

        [Test]
        public void AudioBudgetDisablesOnlyUnprotectedSources()
        {
            var system = new D16PerformanceBudgetSystem(new D16PerformanceBudgetConfiguration { MaxAudibleSources = 1 });
            var snapshot = system.Evaluate(new[]
            {
                new D16PerformanceCandidate { Id = "protected_audio", UsesAudio = true, HasActiveInteraction = true },
                new D16PerformanceCandidate { Id = "background_audio", UsesAudio = true, Distance = 20f },
            });

            Assert.IsTrue(snapshot.Decisions[0].AudioEnabled);
            Assert.IsFalse(snapshot.Decisions[1].AudioEnabled);
        }

        [Test]
        public void InvalidAndDuplicateCandidatesAreNotAppliedTwice()
        {
            var system = new D16PerformanceBudgetSystem();
            var snapshot = system.Evaluate(new[]
            {
                new D16PerformanceCandidate { Id = "same", Distance = 1f },
                new D16PerformanceCandidate { Id = "same", Distance = 2f },
                new D16PerformanceCandidate { Id = "", Distance = 3f },
                null,
                new D16PerformanceCandidate { Id = "nan", Distance = float.NaN },
            });

            Assert.AreEqual(2, snapshot.CandidateCount);
            Assert.AreEqual(1, snapshot.DuplicateCandidateCount);
            Assert.AreEqual(2, snapshot.SkippedInvalidCandidateCount);
            Assert.AreEqual("same", snapshot.Decisions[0].Id);
            Assert.AreEqual(D16SimulationTier.Frozen, snapshot.Decisions[1].SimulationTier);
        }

        [Test]
        public void HighPressureEvaluationDoesNotMutateEventFactsOrSettlementClaims()
        {
            var eventLog = new EventLog();
            var worldEvent = new WorldEvent(
                "d16_budget_fact_guard",
                33,
                "saint_marys_port",
                "D16PerformanceBudgetTest",
                new EventResult { ResultCode = "fact_recorded", Summary = "预算保护事实" },
                saveRevision: 3301);
            Assert.IsFalse(eventLog.Append(worldEvent).IsDuplicate);
            Assert.IsFalse(eventLog.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision).IsDuplicate);

            var before = eventLog.CaptureSnapshot();
            var system = new D16PerformanceBudgetSystem(new D16PerformanceBudgetConfiguration
            {
                MaxActiveAgents = 1,
                MaxActiveAnimals = 1,
                MaxAudibleSources = 1,
            });
            var snapshot = system.Evaluate(new[]
            {
                new D16PerformanceCandidate { Id = "protected_agent", IsAgent = true, HasActiveInteraction = true },
                new D16PerformanceCandidate { Id = "background_agent", IsAgent = true, Distance = 500f, UsesAudio = true },
                new D16PerformanceCandidate { Id = "background_animal", IsAnimal = true, Distance = 500f },
            }, highPressure: true);
            var after = eventLog.CaptureSnapshot();

            Assert.IsTrue(snapshot.BudgetSatisfied);
            Assert.AreEqual(before.NextSequence, after.NextSequence);
            CollectionAssert.AreEqual(before.SettledKeys, after.SettledKeys);
            Assert.AreEqual(before.Facts.Count, after.Facts.Count);
            Assert.AreEqual(before.Facts[0].EventId, after.Facts[0].EventId);
            Assert.AreEqual(before.Facts[0].SaveRevision, after.Facts[0].SaveRevision);
            Assert.AreEqual(before.Facts[0].Result.ResultCode, after.Facts[0].Result.ResultCode);
        }
    }
}
