using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Whalebone.Agent
{
    // ============================================================
    // LLM 统一接口：本地(默认) + 云端(增强) + 模板兜底，自动降级
    // ============================================================
    public interface ILLMClient
    {
        string Name { get; }
        bool Available { get; }
        Task<string> CompleteAsync(string system, string user);
    }

    // ---- 本地 Ollama（离线、零成本、默认优先）----
    public class OllamaClient : ILLMClient
    {
        public string Name => "Ollama(" + Model + ")";
        public bool Available { get; private set; }

        public string BaseUrl;
        public string Model;

        private readonly HttpClient _http;
        private const int TimeoutSeconds = 25;

        public OllamaClient(string baseUrl = "http://127.0.0.1:11434", string model = "qwen2.5:3b")
        {
            BaseUrl = baseUrl;
            Model = model;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
        }

        /// <summary>探测本地服务是否在运行（App 启动时调用一次即可）。</summary>
        public async Task<bool> ProbeAsync()
        {
            try
            {
                var resp = await _http.GetAsync(BaseUrl + "/api/tags");
                Available = resp.IsSuccessStatusCode;
            }
            catch { Available = false; }
            return Available;
        }

        public async Task<string> CompleteAsync(string system, string user)
        {
            var body = "{\"model\":\"" + Model + "\",\"stream\":false,\"messages\":[" +
                       "{\"role\":\"system\",\"content\":" + JsonEscape(system) + "}," +
                       "{\"role\":\"user\",\"content\":" + JsonEscape(user) + "}]}";
            var resp = await _http.PostAsync(BaseUrl + "/api/chat",
                new StringContent(body, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            string text = await resp.Content.ReadAsStringAsync();
            // 提取 message.content 字段
            string content = MiniJson.GetString(text, "content");
            return content ?? text;
        }

        private static string JsonEscape(string s)
        {
            if (s == null) return "\"\"";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }
    }

    // ---- 云端 DeepSeek（OpenAI 兼容接口；Key 为空则不可用）----
    public class DeepSeekClient : ILLMClient
    {
        public string Name => "DeepSeek";
        public bool Available => !string.IsNullOrEmpty(ApiKey);

        public string ApiKey;
        public string Model = "deepseek-chat";

        private readonly HttpClient _http;
        private const int TimeoutSeconds = 40;

        public DeepSeekClient(string apiKey)
        {
            ApiKey = apiKey;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
            _http.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);
        }

        public async Task<string> CompleteAsync(string system, string user)
        {
            var body = "{\"model\":\"" + Model + "\",\"messages\":[" +
                       "{\"role\":\"system\",\"content\":" + JsonEscape(system) + "}," +
                       "{\"role\":\"user\",\"content\":" + JsonEscape(user) + "}]}";
            var resp = await _http.PostAsync("https://api.deepseek.com/chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            string text = await resp.Content.ReadAsStringAsync();
            // choices[0].message.content
            int idx = text.IndexOf("\"content\"", StringComparison.Ordinal);
            if (idx < 0) return text;
            return MiniJson.GetString(text.Substring(idx), "content") ?? text;
        }

        private static string JsonEscape(string s)
        {
            if (s == null) return "\"\"";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }
    }

    // ---- 路由器：本地优先 → 云端兜底 → 抛异常（由上层模板兜底）----
    public class LLMRouter : ILLMClient
    {
        public string Name => "Router";
        public bool Available => true;

        private readonly ILLMClient _local;
        private readonly ILLMClient _cloud;

        public LLMRouter(ILLMClient local, ILLMClient cloud)
        {
            _local = local; _cloud = cloud;
        }

        public async Task<string> CompleteAsync(string system, string user)
        {
            if (_local != null && _local.Available)
            {
                try { return await _local.CompleteAsync(system, user); }
                catch { /* 本地失败，尝试云端 */ }
            }
            if (_cloud != null && _cloud.Available)
            {
                try { return await _cloud.CompleteAsync(system, user); }
                catch { /* 云端也失败 */ }
            }
            throw new InvalidOperationException("no_llm_available");
        }
    }

    // ---- 模板兜底：任何模型不可用时，保证游戏仍可玩 ----
    public static class TemplateFallback
    {
        private static readonly string[] Replies =
        {
            "…(嘟囔着摇了摇头，没有接话)",
            "这事改天再说吧，眼下还有活要干。",
            "海风太大，我啥也没听清。",
            "嗯…你说了算，船长。"
        };

        private static int _idx = 0;

        public static AgentReply Reply(PersonaDefinition persona, string playerInput)
        {
            string text = Replies[_idx % Replies.Length];
            _idx++;
            return new AgentReply
            {
                Speech = text,
                Emotion = Emotion.Neutral,
                AffinityChange = 0f,
                Action = "none",
                ActionArgs = new string[0]
            };
        }
    }
}
