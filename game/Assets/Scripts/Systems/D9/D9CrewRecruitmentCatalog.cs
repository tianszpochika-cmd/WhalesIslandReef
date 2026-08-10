// ============================================================
// D9CrewRecruitmentCatalog —— 垂直切片唯一招募样例
// ============================================================

using Whalebone.Agent;

namespace Whalebone.Systems.D9
{
    public static class D9CrewRecruitmentCatalog
    {
        public const string IsaacReedId = "crew_isaac_reed";

        public static CrewAgent CreateIsaacReed()
        {
            return D9CrewAgentVerticalSlice.CreateIsaacReed().Agent;
        }
    }
}
