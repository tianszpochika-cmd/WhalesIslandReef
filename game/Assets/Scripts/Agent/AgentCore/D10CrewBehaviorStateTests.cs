using System;
using System.Collections.Generic;

namespace Whalebone.Agent
{
    /// <summary>D10 状态载体和规则参数的纯 C# 测试；不启动 Unity/网络。</summary>
    public static class D10CrewBehaviorStateTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("D10 行为状态可序列化往返", StateSnapshotRoundTrip);
            failed += Check("D10 非法状态可归一化", InvalidStateNormalizes);
            failed += Check("D10 行为参数入口稳定", BehaviorRulesAreDeterministic);
            failed += Check("D10 行为参数合法性检查", BehaviorConfigurationIsValid);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("[EX] " + name + " : " + e.Message);
                return 1;
            }
        }

        private static bool StateSnapshotRoundTrip()
        {
            var source = new D10CrewBehaviorState
            {
                CurrentDuty = CrewDuty.Lookout,
                RequestedDuty = CrewDuty.Navigator,
                AssignedShift = CrewShift.NightWatch,
                Status = CrewBehaviorStatus.TransferRequested,
                Forewarning = CrewMutinyForewarning.Warning,
                LeaveDaysRemaining = 2,
                LeaveHoursElapsed = 7,
                RefusalCount = 3,
                SlackingCount = 4,
                TransferCount = 1,
                DepartureRequestCount = 0,
                TransitionSequence = 7,
                LastWorldDay = 12,
                CurrentEfficiencyPercent = 42.5f,
                LastEventId = "event_d10_crew_behavior_test",
                LastCommandKey = "transfer|12|7",
            };
            var restored = new D10CrewBehaviorState();
            restored.Restore(source.ToSnapshot());
            return restored.CurrentDuty == source.CurrentDuty
                && restored.RequestedDuty == source.RequestedDuty
                && restored.AssignedShift == source.AssignedShift
                && restored.Status == source.Status
                && restored.Forewarning == source.Forewarning
                && restored.LeaveDaysRemaining == 2
                && restored.LeaveHoursElapsed == 7
                && restored.RefusalCount == 3
                && restored.SlackingCount == 4
                && restored.TransitionSequence == 7
                && Math.Abs(restored.CurrentEfficiencyPercent - 42.5f) < 0.001f
                && restored.LastEventId == source.LastEventId;
        }

        private static bool InvalidStateNormalizes()
        {
            var values = new Dictionary<string, object>
            {
                ["current_duty"] = "UnknownDuty",
                ["assigned_shift"] = "UnknownShift",
                ["status"] = "UnknownStatus",
                ["forewarning"] = "UnknownForewarning",
                ["leave_days_remaining"] = -5,
                ["leave_hours_elapsed"] = 30,
                ["current_efficiency_percent"] = 150f,
            };
            var state = new D10CrewBehaviorState();
            state.Restore(values);
            return state.CurrentDuty == CrewDuty.Deckhand
                && state.AssignedShift == CrewShift.DayWatch
                && state.Status == CrewBehaviorStatus.Unassigned
                && state.Forewarning == CrewMutinyForewarning.None
                && state.LeaveDaysRemaining == 0
                && state.LeaveHoursElapsed == 23
                && state.CurrentEfficiencyPercent == 100f;
        }

        private static bool BehaviorRulesAreDeterministic()
        {
            return D10CrewBehaviorConfiguration.DutyEfficiencyPercent(CrewDuty.Lookout) == 95f
                && D10CrewBehaviorConfiguration.DutyEfficiencyPercent(CrewDuty.None) == 0f
                && D10CrewBehaviorConfiguration.FatigueFactor(20f) == 100f
                && D10CrewBehaviorConfiguration.FatigueFactor(95f) == 40f
                && D10CrewBehaviorConfiguration.HealthFactor(10f) == 30f
                && D10CrewBehaviorConfiguration.ForewarningForRisk(0.50f) == CrewMutinyForewarning.Critical;
        }

        private static bool BehaviorConfigurationIsValid()
        {
            return D10CrewBehaviorConfiguration.Validate().Count == 0;
        }
    }
}
