#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Whalebone.Gameplay.D30;
using Whalebone.Tests.D23;

namespace Save.Tests
{
    /// <summary>
    /// Q25 首航/循环诊断：D30 只投影成功、失败和重复事件，实际货币、库存、
    /// 航程、返港和保存投影仍由 D23 的领域 owner 链裁决。
    /// </summary>
    public sealed class D19Q25FirstVoyageLoopDiagnosticTests
    {
        [Test]
        public void D30FirstVoyageProjection_FailureAndDuplicateNeverAdvanceOrAward()
        {
            int stage = D30VisualStagePolicy.HarborStage;
            string lastEvent = string.Empty;
            float lastEventTime = -1f;
            float now = 10f;
            var successfulEvents = new[]
            {
                "preparation_opened",
                "market_opened",
                "tavern_opened",
                "shipyard_opened",
                "boarded",
                "departed",
                "returned",
            };

            foreach (var eventId in successfulEvents)
            {
                var decision = D30VisualStagePolicy.Evaluate(
                    stage, eventId, lastEvent, now, lastEventTime, 0.35f);
                Assert.That(decision.IsStageAdvance, Is.True,
                    "D30 success event must advance or validly re-affirm its stage: " + eventId);
                Assert.That(decision.IsFailure, Is.False, eventId);
                Assert.That(decision.IsDuplicate, Is.False, eventId);
                stage = decision.NextStage;
                lastEvent = decision.NormalizedEventId;
                lastEventTime = now;
                now += 1f;
            }

            Assert.That(stage, Is.EqualTo(D30VisualStagePolicy.ReturnStage));

            var failure = D30VisualStagePolicy.Evaluate(
                stage, "save_failed", lastEvent, now, lastEventTime, 0.35f);
            Assert.That(failure.IsFailure, Is.True);
            Assert.That(failure.NextStage, Is.EqualTo(stage),
                "A failed save must not advance the visual route or award a new stage.");

            var duplicate = D30VisualStagePolicy.Evaluate(
                stage, "returned", "returned", now + 0.1f, now, 0.35f);
            Assert.That(duplicate.IsDuplicate, Is.True);
            Assert.That(duplicate.NextStage, Is.EqualTo(stage),
                "A repeated return event inside the guard window must not advance twice.");

            Debug.Log("[Q25/D30] projection_sequence=preparation>market>tavern>shipyard>boarded>departed>returned, " +
                "failure_stable=" + failure.IsFailure + ", duplicate_stable=" + duplicate.IsDuplicate);
        }

        [Test]
        public void D23OwnerFirstVoyageLoop_TradesSailsReturnsAndRestoresSaveProjection()
        {
            D23FullLoopReport report = D23FullLoopScenarioSupport.Run("q25-first-voyage");

            Assert.That(report.TickCount, Is.EqualTo(40));
            Assert.That(report.Arrived, Is.True);
            Assert.That(report.Returned, Is.True);
            Assert.That(report.D19FactCount, Is.EqualTo(report.SharedFactCount));
            Assert.That(report.SaveFingerprint, Is.Not.Empty);
            Assert.That(report.EventFingerprint, Is.Not.Empty);
            Assert.That(report.FinalFingerprint, Is.Not.Empty);
            Assert.That(report.Assertions, Is.GreaterThan(100));

            Debug.Log("[Q25/D23] first_voyage_loop=true, ticks=" + report.TickCount +
                ", arrived=" + report.Arrived + ", returned=" + report.Returned +
                ", d19_facts=" + report.D19FactCount + ", d21_events=" + report.D21EventCount +
                ", save_restored=true, duplicate_owner_checks=true");
        }
    }
}
#endif
