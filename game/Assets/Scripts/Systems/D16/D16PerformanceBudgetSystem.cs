using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D16
{
    /// <summary>
    /// D16 性能预算的唯一裁决入口。
    ///
    /// 该类只为表现和低频模拟层生成确定性预算建议：
    /// - 不修改 Agent、动物、任务、事件或存档状态；
    /// - 不删除实体，只把远景对象降为低频或冻结表现；
    /// - 受玩家当前交互保护的对象不会因预算被冻结；
    /// - 相同输入按稳定 ID 得到相同结果，便于回归和性能采样。
    /// </summary>
    public sealed class D16PerformanceBudgetSystem
    {
        private readonly D16PerformanceBudgetConfiguration _configuration;

        public D16PerformanceBudgetSystem(D16PerformanceBudgetConfiguration configuration = null)
        {
            _configuration = configuration == null
                ? new D16PerformanceBudgetConfiguration()
                : configuration.Clone();
            var errors = _configuration.Validate();
            if (errors.Count > 0)
                throw new ArgumentException("Invalid D16 performance budget: " + string.Join(",", errors));
        }

        public D16PerformanceBudgetConfiguration Configuration => _configuration.Clone();

        public D16PerformanceBudgetSnapshot Evaluate(IEnumerable<D16PerformanceCandidate> source, bool highPressure = false)
        {
            var candidates = new List<D16PerformanceCandidate>();
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            var skippedInvalid = 0;
            var duplicateIds = 0;
            if (source != null)
            {
                foreach (var candidate in source)
                {
                    if (candidate == null || string.IsNullOrWhiteSpace(candidate.Id))
                    {
                        skippedInvalid++;
                        continue;
                    }
                    if (!seenIds.Add(candidate.Id))
                    {
                        duplicateIds++;
                        continue;
                    }
                    candidates.Add(candidate);
                }
            }

            candidates.Sort(ComparePriority);
            var snapshot = new D16PerformanceBudgetSnapshot
            {
                CandidateCount = candidates.Count,
                SkippedInvalidCandidateCount = skippedInvalid,
                DuplicateCandidateCount = duplicateIds,
                HighPressure = highPressure,
            };
            var activeAgents = 0;
            var activeAnimals = 0;
            var audibleSources = 0;
            var protectedOverBudget = 0;

            foreach (var candidate in candidates)
            {
                var decision = new D16PerformanceDecision
                {
                    Id = candidate.Id,
                    SimulationTier = TierFor(candidate, highPressure),
                    AudioEnabled = candidate.UsesAudio,
                    IsProtected = candidate.IsProtected,
                    IsAgent = candidate.IsAgent,
                    IsAnimal = candidate.IsAnimal,
                    UsesAudio = candidate.UsesAudio,
                };

                if (candidate.IsAgent && decision.SimulationTier != D16SimulationTier.Frozen)
                {
                    if (activeAgents >= _configuration.MaxActiveAgents && !candidate.IsProtected)
                        decision.SimulationTier = D16SimulationTier.Frozen;
                    else
                        activeAgents++;
                }

                if (candidate.IsAnimal && decision.SimulationTier != D16SimulationTier.Frozen)
                {
                    if (activeAnimals >= _configuration.MaxActiveAnimals && !candidate.IsProtected)
                        decision.SimulationTier = D16SimulationTier.Frozen;
                    else
                        activeAnimals++;
                }

                if (candidate.UsesAudio && decision.AudioEnabled)
                {
                    if (audibleSources >= _configuration.MaxAudibleSources && !candidate.IsProtected)
                        decision.AudioEnabled = false;
                    else
                        audibleSources++;
                }

                if (candidate.IsProtected &&
                    ((candidate.IsAgent && activeAgents > _configuration.MaxActiveAgents) ||
                     (candidate.IsAnimal && activeAnimals > _configuration.MaxActiveAnimals) ||
                     (candidate.UsesAudio && audibleSources > _configuration.MaxAudibleSources)))
                {
                    protectedOverBudget++;
                }

                snapshot.Decisions.Add(decision);
            }

            snapshot.ActiveAgentCount = activeAgents;
            snapshot.ActiveAnimalCount = activeAnimals;
            snapshot.AudibleSourceCount = audibleSources;
            snapshot.ProtectedOverBudgetCount = protectedOverBudget;
            snapshot.BudgetSatisfied = activeAgents <= _configuration.MaxActiveAgents &&
                                       activeAnimals <= _configuration.MaxActiveAnimals &&
                                       audibleSources <= _configuration.MaxAudibleSources;
            return snapshot;
        }

        private static int ComparePriority(D16PerformanceCandidate left, D16PerformanceCandidate right)
        {
            int leftPriority = Priority(left);
            int rightPriority = Priority(right);
            int result = rightPriority.CompareTo(leftPriority);
            if (result != 0) return result;
            result = SafeDistance(left.Distance).CompareTo(SafeDistance(right.Distance));
            if (result != 0) return result;
            return string.CompareOrdinal(left.Id, right.Id);
        }

        private static int Priority(D16PerformanceCandidate candidate)
        {
            int priority = 0;
            if (candidate.IsProtected) priority += 10000;
            if (candidate.IsVisible) priority += 1000;
            if (candidate.IsAgent || candidate.IsAnimal) priority += 100;
            if (candidate.UsesAudio) priority += 10;
            return priority;
        }

        private static float SafeDistance(float distance)
        {
            if (float.IsNaN(distance) || float.IsInfinity(distance)) return float.MaxValue;
            return Math.Max(0f, distance);
        }

        private D16SimulationTier TierFor(D16PerformanceCandidate candidate, bool highPressure)
        {
            if (candidate.IsProtected) return D16SimulationTier.Full;
            float distance = SafeDistance(candidate.Distance);
            if (distance <= _configuration.FullSimulationDistance)
                return highPressure ? D16SimulationTier.Reduced : D16SimulationTier.Full;
            if (distance <= _configuration.ReducedSimulationDistance)
                return D16SimulationTier.Reduced;
            if (distance <= _configuration.BackgroundSimulationDistance)
                return D16SimulationTier.Background;
            return D16SimulationTier.Frozen;
        }
    }
}
