using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D10;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    /// <summary>
    /// D10-A02 的强逻辑验收：行为状态只由 CrewSystem/D10 入口裁决，
    /// 每个可观察变化有事实事件，且行为状态随既有船员存档回读。
    /// </summary>
    public class D10CrewBehaviorSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_behavior_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void ShiftAndDutyDetermineEfficiency()
        {
            var dayAgent = NewCrew();
            var dayLog = new EventLog();
            var daySystem = new D10CrewBehaviorSystem(dayLog);
            daySystem.EnsureState(dayAgent, 1, 1);
            Assert.IsTrue(daySystem.AssignShift(dayAgent, CrewShift.DayWatch, 1, 2).Accepted);
            var day = daySystem.Tick(dayAgent, CrewShift.DayWatch, 1, 1, 3,
                new[] { "event_shift_roster_day" });

            Assert.IsTrue(day.Accepted);
            Assert.AreEqual(CrewBehaviorStatus.Working, dayAgent.BehaviorState.Status);
            Assert.Greater(dayAgent.BehaviorState.CurrentEfficiencyPercent, 0f);
            Assert.AreEqual("CrewSystem", dayLog.Facts[dayLog.Count - 1].OwnerSystem);

            var nightAgent = NewCrew();
            var nightSystem = new D10CrewBehaviorSystem(new EventLog());
            nightSystem.EnsureState(nightAgent, 1, 1);
            nightSystem.AssignShift(nightAgent, CrewShift.NightWatch, 1, 2);
            nightSystem.Tick(nightAgent, CrewShift.NightWatch, 1, 1, 3);

            Assert.Less(nightAgent.BehaviorState.CurrentEfficiencyPercent,
                dayAgent.BehaviorState.CurrentEfficiencyPercent);
            Assert.AreEqual(CrewDuty.Deckhand, dayAgent.BehaviorState.CurrentDuty);
        }

        [Test]
        public void FatigueAndHealthProduceSlackingRefusalAndInjury()
        {
            var agent = NewCrew();
            var system = new D10CrewBehaviorSystem(new EventLog());
            system.EnsureState(agent, 1, 1);
            system.AssignShift(agent, CrewShift.DayWatch, 1, 2);

            agent.Stats.Fatigue = 70f;
            var slacking = system.Tick(agent, CrewShift.DayWatch, 1, 1, 3);
            Assert.AreEqual(CrewBehaviorStatus.Slacking, agent.BehaviorState.Status);
            Assert.AreEqual(D10CrewBehaviorEventType.Slacking, slacking.Event.Type);
            Assert.AreEqual(CrewBehaviorStatus.Resting, slacking.Event.PreviousStatus);

            agent.Stats.Fatigue = 89f;
            var refusing = system.Tick(agent, CrewShift.DayWatch, 1, 1, 4);
            Assert.AreEqual(CrewBehaviorStatus.Refusing, agent.BehaviorState.Status);
            Assert.AreEqual(D10CrewBehaviorEventType.Refusal, refusing.Event.Type);
            Assert.AreEqual(CrewBehaviorStatus.Slacking, refusing.Event.PreviousStatus);

            agent.Stats.Health = 10f;
            var injured = system.ApplyHealthChange(agent, 0f, 1, 5,
                new[] { "event_health_damage" });
            Assert.IsTrue(injured.Accepted);
            Assert.AreEqual(CrewBehaviorStatus.Injured, agent.BehaviorState.Status);
            Assert.AreEqual(D10CrewBehaviorEventType.Injured, injured.Event.Type);
            CollectionAssert.Contains(injured.Event.CauseEventIds, "event_health_damage");
        }

        [Test]
        public void LeaveTransferAndDepartureHaveExplicitStates()
        {
            var agent = NewCrew();
            var system = new D10CrewBehaviorSystem(new EventLog());
            system.EnsureState(agent, 1, 1);

            Assert.IsTrue(system.RequestLeave(agent, "家庭事务", 1, 2).Accepted);
            Assert.AreEqual(CrewBehaviorStatus.LeaveRequested, agent.BehaviorState.Status);
            Assert.IsTrue(system.ApproveLeave(agent, 2, 1, 3).Accepted);
            Assert.AreEqual(CrewBehaviorStatus.OnLeave, agent.BehaviorState.Status);
            system.Tick(agent, CrewShift.OffDuty, 24, 1, 4);
            Assert.AreEqual(CrewBehaviorStatus.OnLeave, agent.BehaviorState.Status);
            var returned = system.Tick(agent, CrewShift.OffDuty, 24, 2, 5);
            Assert.AreEqual(CrewBehaviorStatus.Resting, agent.BehaviorState.Status);
            Assert.AreEqual(D10CrewBehaviorEventType.LeaveEnded, returned.Event.Type);

            Assert.IsTrue(system.RequestDutyTransfer(agent, CrewDuty.Lookout, 2, 6).Accepted);
            var transferred = system.ApproveDutyTransfer(agent, 2, 7,
                new[] { "event_captain_transfer_approval" });
            Assert.IsTrue(transferred.Accepted);
            Assert.AreEqual(CrewDuty.Lookout, agent.BehaviorState.CurrentDuty);
            Assert.AreEqual(CrewBehaviorStatus.Resting, agent.BehaviorState.Status);

            Assert.IsTrue(system.RequestDeparture(agent, "不再续约", 2, 8).Accepted);
            var departed = system.ApproveDeparture(agent, 2, 9);
            Assert.AreEqual(CrewBehaviorStatus.Departed, agent.BehaviorState.Status);
            Assert.AreEqual(CrewDuty.None, agent.BehaviorState.CurrentDuty);
            Assert.AreEqual(CrewShift.OffDuty, agent.BehaviorState.AssignedShift);
            Assert.AreEqual(CrewBehaviorStatus.DepartureRequested, departed.Event.PreviousStatus);
        }

        [Test]
        public void MutinyForewarningIsDeterministicAndCausal()
        {
            var agent = NewCrew();
            agent.Stats.Loyalty = 20f;
            agent.Stats.Morale = 20f;
            agent.Stats.Fatigue = 80f;
            agent.Stats.Affinity = -60f;
            var log = new EventLog();
            var system = new D10CrewBehaviorSystem(log);
            system.EnsureState(agent, 1, 1);

            var result = system.EvaluateMutinyForewarning(agent, 2, 2,
                new[] { "event_d9_crew_report" });

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(CrewMutinyForewarning.Critical, agent.BehaviorState.Forewarning);
            Assert.AreEqual(D10CrewBehaviorEventType.MutinyForewarningChanged, result.Event.Type);
            Assert.IsNotNull(result.Fact);
            CollectionAssert.Contains(result.Fact.CauseIds, "event_d9_crew_report");
            Assert.IsTrue(result.Fact.EventId.StartsWith("event_d10_crew_behavior_"));
            Assert.AreEqual(2, log.Count);
        }

        [Test]
        public void BehaviorStateRoundTripsThroughCrewSave()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D10测试船长" });
            var agent = GameState.CrewAgents[0];
            var system = new D10CrewBehaviorSystem(new EventLog());
            system.EnsureState(agent, 3, 1);
            system.AssignShift(agent, CrewShift.NightWatch, 3, 2);
            agent.Stats.Fatigue = 71f;
            system.Tick(agent, CrewShift.NightWatch, 1, 3, 3);
            var sequence = agent.BehaviorState.TransitionSequence;

            SaveSystem.Save();
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());

            var restored = GameState.CrewAgents[0];
            Assert.AreEqual(CrewShift.NightWatch, restored.BehaviorState.AssignedShift);
            Assert.AreEqual(CrewBehaviorStatus.Slacking, restored.BehaviorState.Status);
            Assert.AreEqual(sequence, restored.BehaviorState.TransitionSequence);
            Assert.AreEqual(74f, restored.Stats.Fatigue, 0.001f);
        }

        private static CrewAgent NewCrew()
        {
            return D9CrewAgentVerticalSlice.CreateIsaacReed().Agent;
        }
    }
}
