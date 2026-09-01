// D24-L27 —— D18—D23 纯逻辑跨章节确定性回归
//
// 只复用 D23 已公开的固定种子夹具和 owner API。这里不启动 Unity、
// 不读取场景/Prefab/UI/Input/物理/真实时钟，也不接触网络或 LLM。
// ============================================================

using System;
using Whalebone.Tests.D23;

namespace Whalebone.Tests.D24
{
    /// <summary>
    /// 对完整跨章节逻辑链做重复回放，并覆盖中断与长跑边界。
    /// D23 已证明各 case 的局部证据；D24-L27 再验证同一入口的跨章节
    /// 结果摘要不会因进程内重复构造而漂移。
    /// </summary>
    public static class D24FullLogicRegressionTests
    {
        public const int ReplayCount = 10;

        public static int Run()
        {
            return RunAll();
        }

        public static int RunAll()
        {
            D23FullLoopReport baseline = D23FullLoopScenarioSupport.Run("d24-regression");
            int assertions = baseline.Assertions;
            D23ScenarioAssertions.Check(ref assertions,
                baseline.TickCount == 40 && baseline.Arrived && baseline.Returned,
                "D24 cross-chapter baseline must complete the 40-tick return path.");
            D23ScenarioAssertions.Check(ref assertions,
                baseline.D19FactCount == baseline.SharedFactCount &&
                baseline.SaveFingerprint.Length > 0 &&
                baseline.EventFingerprint.Length > 0 &&
                baseline.FinalFingerprint.Length > 0,
                "D24 cross-chapter baseline must expose restored owner fingerprints.");

            for (int index = 1; index < ReplayCount; index++)
            {
                D23FullLoopReport replay = D23FullLoopScenarioSupport.Run("d24-regression");
                assertions += replay.Assertions;
                D23ScenarioAssertions.Check(ref assertions,
                    replay.TickCount == baseline.TickCount &&
                    replay.Arrived == baseline.Arrived &&
                    replay.Returned == baseline.Returned &&
                    replay.D19FactCount == baseline.D19FactCount &&
                    replay.D21EventCount == baseline.D21EventCount &&
                    replay.Money == baseline.Money &&
                    replay.CargoCount == baseline.CargoCount &&
                    replay.SaveFingerprint == baseline.SaveFingerprint &&
                    replay.EventFingerprint == baseline.EventFingerprint &&
                    replay.FinalFingerprint == baseline.FinalFingerprint,
                    "D24 cross-chapter replay " + index + " diverged from the fixed baseline.");
            }

            int interruptionAssertions = D23FullLoopScenarioSupport.RunInterruptionMatrix("d24-regression-interrupt");
            D23ScenarioAssertions.Check(ref assertions,
                interruptionAssertions > 0,
                "D24 cross-chapter interruption regression must execute assertions.");
            assertions += interruptionAssertions;

            int longRunAssertions = D23FullLoopScenarioSupport.RunLongRun("d24-regression-long-run");
            D23ScenarioAssertions.Check(ref assertions,
                longRunAssertions > 50,
                "D24 cross-chapter long-run regression must execute more than 50 assertions.");
            assertions += longRunAssertions;

            return assertions;
        }
    }
}
