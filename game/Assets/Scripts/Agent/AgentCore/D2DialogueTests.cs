using System;
using System.Threading.Tasks;

namespace Whalebone.Agent
{
    public static class D2DialogueTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("合法回复通过 D2 校验", ValidReply);
            failed += Check("缺少 speech 被拒绝", MissingSpeechRejected);
            failed += Check("越界好感被拒绝", AffinityRejected);
            failed += Check("非法情绪被拒绝", EmotionRejected);
            failed += Check("D2 动作被拒绝", ActionRejected);
            failed += Check("对话只返回好感提议，不重复修改数值", DialogueDoesNotApplyAffinity);
            failed += Check("模型失败走模板兜底", FailureFallsBack);
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

        private static bool ValidReply()
        {
            const string json = "{\"speech\":\"海风正好。\",\"emotion\":\"happy\",\"affinity_change\":2,\"action\":\"none\",\"action_args\":[]}";
            return AgentReplyValidator.TryParseD2(json, out var reply, out _)
                && reply.Speech == "海风正好。" && reply.AffinityChange == 2f;
        }

        private static bool MissingSpeechRejected()
        {
            const string json = "{\"emotion\":\"neutral\",\"affinity_change\":0,\"action\":\"none\"}";
            return !AgentReplyValidator.TryParseD2(json, out _, out var error) && error == "missing_speech";
        }

        private static bool AffinityRejected()
        {
            const string json = "{\"speech\":\"越界\",\"emotion\":\"angry\",\"affinity_change\":99,\"action\":\"none\"}";
            return !AgentReplyValidator.TryParseD2(json, out _, out _);
        }

        private static bool EmotionRejected()
        {
            const string json = "{\"speech\":\"非法\",\"emotion\":\"excited\",\"affinity_change\":0,\"action\":\"none\"}";
            return !AgentReplyValidator.TryParseD2(json, out _, out _);
        }

        private static bool ActionRejected()
        {
            const string json = "{\"speech\":\"动作\",\"emotion\":\"neutral\",\"affinity_change\":0,\"action\":\"fire\"}";
            return !AgentReplyValidator.TryParseD2(json, out _, out _);
        }

        private static bool DialogueDoesNotApplyAffinity()
        {
            var agent = new CrewAgent("crew", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var llm = new MockLLMClient("{\"speech\":\"船长。\",\"emotion\":\"neutral\",\"affinity_change\":3,\"action\":\"none\"}");
            var session = new DialogueSession(agent, llm, new MutinyMonitor(agent.Stats));
            var reply = session.SpeakAsync("你好").GetAwaiter().GetResult();
            return reply.AffinityChange == 3f && agent.Stats.Affinity == 0f;
        }

        private static bool FailureFallsBack()
        {
            var agent = new CrewAgent("crew", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var session = new DialogueSession(agent, new FailingClient(), new MutinyMonitor(agent.Stats));
            var reply = session.SpeakAsync("你好").GetAwaiter().GetResult();
            return reply != null && !string.IsNullOrWhiteSpace(reply.Speech) && reply.Action == "none";
        }

        private sealed class FailingClient : ILLMClient
        {
            public string Name => "Failing";
            public bool Available => false;
            public Task<string> CompleteAsync(string system, string user)
            {
                return Task.FromException<string>(new InvalidOperationException("offline"));
            }
        }
    }
}
