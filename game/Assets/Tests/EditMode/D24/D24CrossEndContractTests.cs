// D24-L22：共享 AgentReply 契约的 game/server/web 逻辑消费证据。

using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Whalebone.Tests.D24
{
    public static class D24CrossEndContractTests
    {
        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            try
            {
                string root = FindRepositoryRoot();
                string contractPath = Path.Combine(root, "contracts", "agent-reply.schema.json");
                if (!File.Exists(contractPath)) return Blocked("agent-reply contract is missing");
                string contractText = File.ReadAllText(contractPath);
                using (JsonDocument document = JsonDocument.Parse(contractText))
                {
                    if (!document.RootElement.TryGetProperty("$id", out JsonElement id) || string.IsNullOrWhiteSpace(id.GetString()))
                        return Fail("agent-reply schema has no $id");
                }
                string gameRoot = Path.Combine(root, "game", "Assets", "Scripts", "Agent", "AgentCore");
                string[] gameFiles = Directory.Exists(gameRoot) ? Directory.GetFiles(gameRoot, "*.cs", SearchOption.AllDirectories) : Array.Empty<string>();
                bool gameMarker = gameFiles.Any(file => File.ReadAllText(file).IndexOf("agent_reply.v1.1", StringComparison.Ordinal) >= 0);
                string gateway = Path.Combine(root, "server", "src", "modules", "gateway", "gateway.service.ts");
                bool serverMarker = File.Exists(gateway) && File.ReadAllText(gateway).IndexOf("agent_reply.v1.1", StringComparison.Ordinal) >= 0;
                string webRoot = Path.Combine(root, "web");
                int webFiles = Directory.Exists(webRoot)
                    ? Directory.GetFiles(webRoot, "*.*", SearchOption.AllDirectories).Count(file => file.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    : 0;
                string webConsumerPath = Path.Combine(webRoot, "src", "contracts", "agent-reply.ts");
                string webConsumer = File.Exists(webConsumerPath) ? File.ReadAllText(webConsumerPath) : string.Empty;
                string[] requiredWebConsumerMarkers =
                {
                    "AGENT_REPLY_SCHEMA_VERSION",
                    "parseAgentReply",
                    "FORBIDDEN_RESULT_KEYS",
                    "hasExactKeys",
                    "relation_proposal",
                    "action_intent",
                    "invalid_agent_reply_noop_action"
                };
                bool webConsumerValid = requiredWebConsumerMarkers.All(marker => webConsumer.IndexOf(marker, StringComparison.Ordinal) >= 0);
                Console.WriteLine("    contract_id=present; game_marker=" + gameMarker + "; server_marker=" + serverMarker + "; web_consumer_files=" + webFiles + "; web_consumer_valid=" + webConsumerValid);
                if (!gameMarker || !serverMarker) return Blocked("shared contract marker is missing in game or server");
                if (webFiles == 0) return Blocked("web has no source consumer for the shared contract");
                if (!webConsumerValid) return Blocked("web AgentReply consumer is missing required validation rules");
                return 0;
            }
            catch (Exception error)
            {
                Console.WriteLine("[EX] D24-L22 " + error.Message);
                return 1;
            }
        }

        private static int Fail(string message)
        {
            Console.WriteLine("[FAIL] D24-L22 " + message);
            return 1;
        }

        private static int Blocked(string message)
        {
            Console.WriteLine("[BLOCKED] D24-L22 " + message);
            return 1;
        }

        private static string FindRepositoryRoot()
        {
            string current = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "AGENTS.md"))) return current;
                current = Directory.GetParent(current)?.FullName;
            }
            throw new DirectoryNotFoundException("repository root with AGENTS.md was not found");
        }
    }
}
