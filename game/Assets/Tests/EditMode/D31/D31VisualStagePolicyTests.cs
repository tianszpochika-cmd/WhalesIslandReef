using NUnit.Framework;
using Whalebone.Gameplay.D29;
using Whalebone.Gameplay.D30;

namespace Whalebone.Tests.D31
{
    /// <summary>
    /// D31 交互收口的纯逻辑回归。
    /// 测试不启动场景、不写存档，只验证 D29/D30 事件不会把失败或重复请求误投影为成功阶段。
    /// </summary>
    public sealed class D31VisualStagePolicyTests
    {
        [Test]
        public void FirstOneShotRequestIsAllowedAndNormalized()
        {
            var decision = D29OneShotPolicy.Evaluate(
                "  market  ", string.Empty, 10f, -1f, 0.35f);

            Assert.That(decision.Allowed, Is.True);
            Assert.That(decision.IsDuplicate, Is.False);
            Assert.That(decision.NormalizedActionId, Is.EqualTo("market"));
        }

        [Test]
        public void SameOneShotRequestWithinGuardWindowIsBlocked()
        {
            var decision = D29OneShotPolicy.Evaluate(
                "market", "market", 10.20f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.False);
            Assert.That(decision.IsDuplicate, Is.True);
        }

        [Test]
        public void DifferentOneShotRequestDoesNotShareThePreviousGuard()
        {
            var decision = D29OneShotPolicy.Evaluate(
                "tavern", "market", 10.20f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.True);
            Assert.That(decision.IsDuplicate, Is.False);
        }

        [Test]
        public void SameOneShotRequestAfterGuardWindowIsAllowed()
        {
            var decision = D29OneShotPolicy.Evaluate(
                "market", "market", 10.36f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.True);
            Assert.That(decision.IsDuplicate, Is.False);
        }

        [Test]
        public void OneShotGuardNormalizesPreviousActionId()
        {
            var decision = D29OneShotPolicy.Evaluate(
                " MARKET ", " market ", 10.20f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.False);
            Assert.That(decision.IsDuplicate, Is.True);
            Assert.That(decision.NormalizedActionId, Is.EqualTo("market"));
        }

        [Test]
        public void OneShotGuardDoesNotStickWhenClockMovesBackwards()
        {
            var decision = D29OneShotPolicy.Evaluate(
                "market", "market", 9.90f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.True);
            Assert.That(decision.IsDuplicate, Is.False);
        }

        [Test]
        public void EmptyOneShotRequestIsNotMistakenForADuplicate()
        {
            var decision = D29OneShotPolicy.Evaluate(
                null, string.Empty, 10.20f, 10f, 0.35f);

            Assert.That(decision.Allowed, Is.True);
            Assert.That(decision.NormalizedActionId, Is.Empty);
        }

        [TestCase("market_opened", D30VisualStagePolicy.MarketStage)]
        [TestCase("tavern_opened", D30VisualStagePolicy.TavernStage)]
        [TestCase("shipyard_opened", D30VisualStagePolicy.ShipyardStage)]
        [TestCase("boarded", D30VisualStagePolicy.ShipStage)]
        [TestCase("departed", D30VisualStagePolicy.VoyageStage)]
        [TestCase("returned", D30VisualStagePolicy.ReturnStage)]
        public void SuccessfulEventsProjectToTheirExpectedStage(string eventId, int expectedStage)
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.HarborStage, eventId, string.Empty, 10f, -1f, 0.35f);

            Assert.That(decision.NextStage, Is.EqualTo(expectedStage));
            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Advanced));
            Assert.That(decision.IsStageAdvance, Is.True);
            Assert.That(decision.NormalizedEventId, Is.EqualTo(eventId));
        }

        [Test]
        public void PreparationSuccessIsAValidEventEvenWhenItKeepsHarborStage()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.HarborStage, " Preparation_Opened ", string.Empty,
                10f, -1f, 0.35f);

            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.HarborStage));
            Assert.That(decision.IsStageAdvance, Is.True);
            Assert.That(decision.NormalizedEventId, Is.EqualTo("preparation_opened"));
        }

        [TestCase("market_failed")]
        [TestCase("tavern_failed")]
        [TestCase("shipyard_failed")]
        [TestCase("preparation_failed")]
        [TestCase("board_failed")]
        [TestCase("save_failed")]
        [TestCase("weather_failed")]
        [TestCase("quality_failed")]
        public void FailedEventsKeepTheCurrentStage(string eventId)
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.VoyageStage, eventId, string.Empty, 10f, -1f, 0.35f);

            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.VoyageStage));
            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Failed));
            Assert.That(decision.IsFailure, Is.True);
            Assert.That(decision.IsStageAdvance, Is.False);
        }

        [Test]
        public void EveryFailureEventKeepsEveryKnownVisualStage()
        {
            var stages = new[]
            {
                D30VisualStagePolicy.HarborStage,
                D30VisualStagePolicy.MarketStage,
                D30VisualStagePolicy.TavernStage,
                D30VisualStagePolicy.ShipyardStage,
                D30VisualStagePolicy.ShipStage,
                D30VisualStagePolicy.VoyageStage,
                D30VisualStagePolicy.ReturnStage,
            };
            var failures = new[]
            {
                "market_failed", "tavern_failed", "shipyard_failed",
                "preparation_failed", "board_failed", "save_failed",
                "weather_failed", "quality_failed",
            };

            foreach (var failure in failures)
            {
                Assert.That(D30VisualStagePolicy.IsFailureEvent(failure), Is.True, failure);
                foreach (var stage in stages)
                {
                    var decision = D30VisualStagePolicy.Evaluate(
                        stage, failure, string.Empty, 10f, -1f, 0.35f);
                    Assert.That(decision.NextStage, Is.EqualTo(stage), failure + " stage=" + stage);
                    Assert.That(decision.Outcome,
                        Is.EqualTo(D30VisualStagePolicy.EventOutcome.Failed),
                        failure + " stage=" + stage);
                }
            }
        }

        [Test]
        public void FailureClassifierRejectsSuccessUnknownAndEmptyEvents()
        {
            Assert.That(D30VisualStagePolicy.IsFailureEvent("market_opened"), Is.False);
            Assert.That(D30VisualStagePolicy.IsFailureEvent("not_a_route_event"), Is.False);
            Assert.That(D30VisualStagePolicy.IsFailureEvent(null), Is.False);
        }

        [Test]
        public void DuplicateEventKeepsStageAndIsDistinctFromFailure()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.MarketStage, " MARKET_OPENED ", "market_opened",
                10.20f, 10f, 0.35f);

            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.MarketStage));
            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Duplicate));
            Assert.That(decision.IsDuplicate, Is.True);
            Assert.That(decision.IsFailure, Is.False);
        }

        [Test]
        public void VisualStageGuardNormalizesPreviousEventId()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.MarketStage, " MARKET_OPENED ", " market_opened ",
                10.20f, 10f, 0.35f);

            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Duplicate));
            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.MarketStage));
        }

        [Test]
        public void VisualStageGuardDoesNotStickWhenClockMovesBackwards()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.HarborStage, "market_opened", "market_opened",
                9.90f, 10f, 0.35f);

            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Advanced));
            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.MarketStage));
        }

        [Test]
        public void SameEventAfterGuardWindowCanAdvanceAgain()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.HarborStage, "market_opened", "market_opened",
                10.36f, 10f, 0.35f);

            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Advanced));
            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.MarketStage));
        }

        [Test]
        public void UnknownOrEmptyEventKeepsStage()
        {
            var unknown = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.ReturnStage, "not_a_route_event", string.Empty,
                10f, -1f, 0.35f);
            var empty = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.ReturnStage, null, string.Empty,
                10f, -1f, 0.35f);

            Assert.That(unknown.NextStage, Is.EqualTo(D30VisualStagePolicy.ReturnStage));
            Assert.That(unknown.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Unknown));
            Assert.That(empty.NextStage, Is.EqualTo(D30VisualStagePolicy.ReturnStage));
            Assert.That(empty.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Unknown));
        }

        [Test]
        public void UnknownEventDoesNotEnterRepeatGuard()
        {
            var decision = D30VisualStagePolicy.Evaluate(
                D30VisualStagePolicy.ReturnStage, "not_a_route_event", "not_a_route_event",
                10.20f, 10f, 0.35f);

            Assert.That(decision.NextStage, Is.EqualTo(D30VisualStagePolicy.ReturnStage));
            Assert.That(decision.Outcome, Is.EqualTo(D30VisualStagePolicy.EventOutcome.Unknown));
            Assert.That(decision.IsDuplicate, Is.False);
        }
    }
}
