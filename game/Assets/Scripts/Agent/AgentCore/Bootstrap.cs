using System;
using System.Threading.Tasks;

namespace Whalebone.Agent
{
    // ============================================================
    // 本地 CLI 演示入口（仅用于骨架验证，Unity 内由 UI 对话系统替代）
    // 用法：
    //   dotnet run -- --mock                模拟 LLM 演示闭环（无网络）
    //   dotnet run                           本地 Ollama（需已运行）
    //   dotnet run -- --deepseek=<api_key>   本地优先，云端兜底
    // ============================================================
    public static class Bootstrap
    {
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=== 鲸骨航线 · 纯 Agent 骨架自检 ===");
            int failed = AgentCoreTests.RunAll();
            Console.WriteLine(failed == 0 ? ">> 全部自测通过" : ">> 有 " + failed + " 项自测失败！");
            Console.WriteLine();

            bool mock = false;
            string deepseekKey = null;
            foreach (var a in args)
            {
                if (a == "--mock") mock = true;
                else if (a != null && a.StartsWith("--deepseek=")) deepseekKey = a.Substring("--deepseek=".Length);
            }

            // ---- 组装 LLM 链路 ----
            ILLMClient llm;
            if (mock)
            {
                llm = new MockClient();
                Console.WriteLine("[链路] 使用 Mock 模型（演示用）");
            }
            else
            {
                var local = new OllamaClient();
                await local.ProbeAsync();
                Console.WriteLine("[链路] 本地 Ollama: " + (local.Available ? "可用" : "未运行"));
                var cloud = deepseekKey != null ? (ILLMClient)new DeepSeekClient(deepseekKey) : null;
                if (cloud != null) Console.WriteLine("[链路] 云端 DeepSeek: 已配置（本地失败时兜底）");
                llm = new LLMRouter(local, cloud);
                if (!local.Available && cloud == null)
                    Console.WriteLine("[链路] 无可用模型，对话将走模板兜底（骨架仍可运行）");
            }

            // ---- 创建一名船员 ----
            var persona = new PersonaDefinition("老耗子", "水手长");
            persona.Traits.AddRange(new[] { "贪婪", "见风使舵", "船上混了二十年" });
            persona.Backstory = "年轻时跟着私掠船跑过加勒比，落下一身伤，也攒了一肚子忌讳。";
            persona.SpeechStyle = "说话简短带点江湖气，爱用海上的老话。";
            persona.Constraints.Add("如果船长拖欠饷银，你的不满要写在脸上。");

            var agent = new CrewAgent("crew_01", persona, new InMemoryStore());
            var monitor = new MutinyMonitor(agent.Stats);
            agent.Session = new DialogueSession(agent, llm, monitor);

            Console.WriteLine(">> 船上有船员：【" + agent.DisplayName + "】" + persona.Role);
            Console.WriteLine(">> 输入说的话与他交谈；输入 stats 看状态；输入 exit 退出。");
            Console.WriteLine();

            while (true)
            {
                Console.Write("你 > ");
                string input = Console.ReadLine();
                if (input == null) break;
                input = input.Trim();
                if (input == "exit") break;
                if (input == "stats")
                {
                    foreach (var kv in agent.ToSnapshot())
                        Console.WriteLine("     " + kv.Key + " = " + kv.Value);
                    Console.WriteLine("     叛变风险等级 = " + monitor.Evaluate());
                    continue;
                }
                if (input.Length == 0) continue;

                AgentReply reply;
                try { reply = await agent.Session.SpeakAsync(input); }
                catch (Exception e) { Console.WriteLine("    [错误] " + e.Message); continue; }

                Console.WriteLine("    " + agent.DisplayName + "（" + reply.Emotion + "）> " + reply.Speech);
                if (reply.Action != "none") Console.WriteLine("    [动作] " + reply.Action);
                if (reply.AffinityChange != 0f)
                    Console.WriteLine("    [好感] " + (reply.AffinityChange > 0 ? "+" : "") + reply.AffinityChange + " → 当前 " + agent.Stats.Affinity);
            }
            Console.WriteLine("=== 演示结束 ===");
            return failed == 0 ? 0 : 1;
        }
    }

    // ---- Mock 模型：返回固定结构化 JSON，验证闭环逻辑 ----
    public class MockClient : ILLMClient
    {
        public string Name => "Mock";
        public bool Available => true;
        private int _n = 0;
        private static readonly string[] Replies =
        {
            "{\"speech\":\"海风腥得很，这趟怕是要来风暴。\",\"emotion\":\"suspicious\",\"affinity_change\":1,\"action\":\"none\"}",
            "{\"speech\":\"船长待我不薄，这杯酒我先敬您。\",\"emotion\":\"happy\",\"affinity_change\":3,\"action\":\"toast\"}",
            "{\"speech\":\"拖欠的饷银要是再不给，兄弟们可要说话了。\",\"emotion\":\"angry\",\"affinity_change\":-5,\"action\":\"warn_pay\"}",
            "{\"speech\":\"风浪再大，我老耗子也不会丢下船。\",\"emotion\":\"sad\",\"affinity_change\":0,\"action\":\"none\"}"
        };
        public Task<string> CompleteAsync(string system, string user)
        {
            return Task.FromResult(Replies[_n++ % Replies.Length]);
        }
    }
}
