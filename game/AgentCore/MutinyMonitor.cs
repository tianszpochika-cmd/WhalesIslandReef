using Whalebone.Game.Config;

namespace Whalebone.Agent
{
    // ============================================================
    // 叛变监视器：强逻辑"数值边界"的示例实现
    // 关键设计：危险分支由数值锁死，LLM 根本没有机会触发它们——
    // 玩家无法靠"说服"绕过忠诚度阈值，这正是"数值即边界"原则。
    // ============================================================
    public class MutinyMonitor
    {
        public enum RiskLevel { Low, Warning, Critical }

        private readonly CrewStats _stats;

        public MutinyMonitor(CrewStats stats) { _stats = stats; }

        public RiskLevel Evaluate()
        {
            float r = _stats.GetMutinyRisk();
            if (r >= 0.35f) return RiskLevel.Critical;
            if (r >= 0.18f) return RiskLevel.Warning;
            return RiskLevel.Low;
        }

        /// <summary>
        /// 生成追加到 system prompt 的边界提示；并把被锁的分支从语境中剔除。
        /// </summary>
        public string BuildBoundaryNote()
        {
            switch (Evaluate())
            {
                case RiskLevel.Critical:
                    return "你现在极度不满，对船长的信任接近崩溃；不要假装无事发生，也不要说" +
                           "‘我没事’——但不要在对话中直接宣告叛变（那是事件系统的裁决）。";
                case RiskLevel.Warning:
                    return "你对船长的某些决定心怀不满，可以抱怨，但还不到翻脸的地步。";
                default:
                    return "";
            }
        }

        /// <summary>分支锁：判断某个对话选项当前是否被数值锁定（不进 Prompt）。</summary>
        public bool IsBranchLocked(string branchKey)
        {
            switch (branchKey)
            {
                case "plead":      // 求情/挽回：好感过低时无效
                    return _stats.Affinity < (float)NumericConfiguration.Default.MutinyAffinityThreshold.DefaultValue;
                case "intimidate": // 威胁：忠诚高于 70 才可能奏效
                    return _stats.Loyalty > 70f;
                default:
                    return false;
            }
        }
    }
}
