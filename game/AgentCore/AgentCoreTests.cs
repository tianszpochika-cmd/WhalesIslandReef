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
            failed += Check("D8-A04 注册表包含 60 个动作", TestP2ARegistryCount);
            failed += Check("D8-A04 未授权对象和越权目标被拒绝", TestP2AAuthorizationBoundary);
            failed += Check("D8-A04 非法参数和最终结果字段被拒绝", TestP2AParameterBoundary);
            failed += Check("事实摘要只进入 Agent 记忆，不改变领域结果", TestFactSummaryRead);
            failed += D2DialogueTests.RunAll();
#if !UNITY_2021_1_OR_NEWER
            failed += Whalebone.Game.Config.NumericConfigurationTests.RunAll();
            failed += V3.V3EntityModelTests.RunAll();
#endif
            failed += D9CrewAgentVerticalSliceTests.RunAll();
            failed += D10CrewBehaviorStateTests.RunAll();
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
            string json = AgentReplyTestFixtures.ValidTrimSail;
            var r = AgentReply.FromJson(json);
            return r.IsValid
                && r.Speech == "起风了，收帆！"
                && r.Emotion == Emotion.Afraid
                && r.AffinityChange == -3f
                && r.Action == "trim_sail"
                && r.ActionIntent.TargetRef == "ship:active"
                && (string)r.ActionIntent.Parameters["setting"] == "full";
        }

        private static bool TestMiniJsonEscape()
        {
            string json = AgentReplyTestFixtures.ValidNone.Replace("海风正好。", "他说\\\"今天\\\"不走，\\\\反斜杠\\\\");
            var r = AgentReply.FromJson(json);
            return r.Speech == "他说\"今天\"不走，\\反斜杠\\" && r.Action == "none";
        }

        private static bool TestP2ARegistryCount()
        {
            return P2AActionRegistry.All.Count == 60 && P2AActionRegistry.TryGet("trim_sail", out _)
                && P2AActionRegistry.TryGet("tend_garden", out _);
        }

        private static bool TestP2AAuthorizationBoundary()
        {
            var context = new ActionValidationContext();
            context.AuthorizedActorRefs.Add("crew:deckhand-1");
            context.AuthorizedTargetRefs.Add("ship:active");
            context.Permissions.Add("ship.sail");
            context.SatisfiedPrerequisites.Add("ship.present");
            context.SatisfiedPrerequisites.Add("crew.on_duty");
            if (!AgentReplyValidator.TryValidate(AgentReplyTestFixtures.ValidTrimSail, context, out _, out _)) return false;
            context.AuthorizedTargetRefs.Clear();
            return !AgentReplyValidator.TryValidate(AgentReplyTestFixtures.ValidTrimSail, context, out _, out var error) && error == "unauthorized_target";
        }

        private static bool TestP2AParameterBoundary()
        {
            if (AgentReplyValidator.TryParse(AgentReplyTestFixtures.InvalidDuration, out _, out _)) return false;
            return !AgentReplyValidator.TryParse(AgentReplyTestFixtures.ForbiddenResult, out _, out var error) && error == "forbidden_result_field";
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

        private static bool TestFactSummaryRead()
        {
            var store = new InMemoryStore();
            var memory = new AgentMemory(store);
            memory.RecordFactSummary("event_fog_001", "雾中航行被迫中断，可沿灯塔路线恢复。", 0.8f);
            var facts = store.Top(3);
            return facts.Count == 1
                && facts[0].Kind == "fact:event_fog_001"
                && facts[0].Text.Contains("灯塔路线")
                && memory.BuildMemorySection().Contains("雾中航行");
        }
    }
}
