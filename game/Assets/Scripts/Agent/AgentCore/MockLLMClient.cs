using System.Threading.Tasks;

namespace Whalebone.Agent
{
    /// <summary>D2 离线测试用确定性客户端，不访问网络。</summary>
    public sealed class MockLLMClient : ILLMClient
    {
        private readonly string[] _responses;
        private int _index;

        public MockLLMClient(params string[] responses)
        {
            _responses = responses == null || responses.Length == 0
                ? new[] { AgentReplyTestFixtures.ValidNone.Replace("海风正好。", "海风正好，船长。") }
                : responses;
        }

        public string Name => "Mock";
        public bool Available => true;

        public Task<string> CompleteAsync(string system, string user)
        {
            string response = _responses[_index % _responses.Length];
            _index++;
            return Task.FromResult(response);
        }
    }
}
