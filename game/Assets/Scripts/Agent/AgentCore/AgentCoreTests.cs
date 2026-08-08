using System;
using System.Collections.Generic;

namespace Whalebone.Agent
{
    // ============================================================
    // 纯自测（无网络、无第三方依赖）：
    // 覆盖 强逻辑数值 / JSON 解析 / Persona 组装 / 分支锁
    // 在 Unity 里可对应为 Edit Mode 单元测试
    // ============================================================
    public static class AgentCoreTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("JSON 解析字符串+数字+数组", TestMiniJsonParse);
            failed += Check("JSON 转义（引号/反斜杠）", TestMiniJsonEscape);
            failed += Check("叛变风险随忠诚下降而上升", TestMutinyRisk);
            failed += Check("好感度夹取 ±100", TestAffinityClamp);
            failed += Check("Persona prompt 包含 JSON 指令与约束", TestPersonaPrompt);
            failed += Check("分支锁：低好感锁求情分支", TestBranchLock);
            failed += D2DialogueTests.RunAll();
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            bool ok = false;
            try { ok = test(); }
            catch (Exception e) { Console.WriteLine("[EX] " + name + " : " + e.Message); return 1; }
            Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
            return ok ? 0 : 1;
        }

        private static bool TestMiniJsonParse()
        {
            string json = "{\"speech\":\"起风了，收帆！\",\"emotion\":\"afraid\",\"affinity_change\":-3,\"action\":\"trim_sail\",\"action_args\":[\"main\",\"fast\"]}";
            var r = AgentReply.FromJson(json);
            return r.IsValid
                && r.Speech == "起风了，收帆！"
                && r.Emotion == Emotion.Afraid
                && r.AffinityChange == -3f
                && r.Action == "trim_sail"
                && r.ActionArgs.Length == 2 && r.ActionArgs[1] == "fast";
        }

        private static bool TestMiniJsonEscape()
        {
            string json = "{\"speech\":\"他说\\\"今天\\\"不走，\\\\反斜杠\\\\\",\"action\":\"none\"}";
            var r = AgentReply.FromJson(json);
            return r.Speech == "他说\"今天\"不走，\\反斜杠\\" && r.Action == "none";
        }

        private static bool TestMutinyRisk()
        {
            var s = new CrewStats("t1") { Loyalty = 60f, Morale = 60f, Fatigue = 20f };
            float low = s.GetMutinyRisk();
            s.Loyalty = 15f; s.Morale = 20f; s.Fatigue = 95f;
            float high = s.GetMutinyRisk();
            // 高期望值：0.30(忠诚) + 0.12(士气) + 0.10(疲劳) = 0.52
            return high > low + 0.3f && high > 0.5f;
        }

        private static bool TestAffinityClamp()
        {
            var s = new CrewStats("t2");
            s.ApplyAffinityChange(200f);
            if (s.Affinity != 100f) return false;
            s.ApplyAffinityChange(-300f);
            return s.Affinity == -100f;
        }

        private static bool TestPersonaPrompt()
        {
            var p = new PersonaDefinition("老耗子", "水手长");
            p.Traits.Add("贪婪"); p.Traits.Add("见风使舵");
            string prompt = p.BuildSystemPrompt(new CrewStats("t3"), "记忆片段");
            return prompt.Contains("老耗子") && prompt.Contains("water") == false
                && prompt.Contains("affinity_change") && prompt.Contains("约束");
        }

        private static bool TestBranchLock()
        {
            var s = new CrewStats("t4") { Affinity = -80f, Loyalty = 30f };
            var m = new MutinyMonitor(s);
            if (!m.IsBranchLocked("plead")) return false;      // 好感过低 → 求情被锁
            if (m.IsBranchLocked("intimidate")) return false;  // 忠诚低 → 威胁未锁（可试）
            s.Affinity = 10f; s.Loyalty = 90f;
            return !m.IsBranchLocked("plead") && m.IsBranchLocked("intimidate");
        }
    }
}
