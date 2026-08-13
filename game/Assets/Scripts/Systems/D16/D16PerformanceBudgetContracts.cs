using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D16
{
    /// <summary>
    /// 表现层/低频模拟层的降级等级。它只描述预算建议，不写入领域状态、事实事件或存档。
    /// </summary>
    public enum D16SimulationTier
    {
        Full,
        Reduced,
        Background,
        Frozen,
    }

    [Serializable]
    public sealed class D16PerformanceBudgetConfiguration
    {
        public int TargetFps = 60;
        public int MaxActiveAgents = 32;
        public int MaxActiveAnimals = 64;
        public int MaxAudibleSources = 24;
        public float FullSimulationDistance = 40f;
        public float ReducedSimulationDistance = 100f;
        public float BackgroundSimulationDistance = 240f;

        public D16PerformanceBudgetConfiguration Clone()
        {
            return new D16PerformanceBudgetConfiguration
            {
                TargetFps = TargetFps,
                MaxActiveAgents = MaxActiveAgents,
                MaxActiveAnimals = MaxActiveAnimals,
                MaxAudibleSources = MaxAudibleSources,
                FullSimulationDistance = FullSimulationDistance,
                ReducedSimulationDistance = ReducedSimulationDistance,
                BackgroundSimulationDistance = BackgroundSimulationDistance,
            };
        }

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (TargetFps <= 0) errors.Add("target_fps_must_be_positive");
            if (MaxActiveAgents <= 0) errors.Add("max_active_agents_must_be_positive");
            if (MaxActiveAnimals <= 0) errors.Add("max_active_animals_must_be_positive");
            if (MaxAudibleSources <= 0) errors.Add("max_audible_sources_must_be_positive");
            if (FullSimulationDistance < 0f) errors.Add("full_distance_must_be_non_negative");
            if (ReducedSimulationDistance <= FullSimulationDistance) errors.Add("reduced_distance_must_exceed_full_distance");
            if (BackgroundSimulationDistance <= ReducedSimulationDistance) errors.Add("background_distance_must_exceed_reduced_distance");
            return errors;
        }
    }

    [Serializable]
    public sealed class D16PerformanceCandidate
    {
        public string Id;
        public float Distance;
        public bool IsVisible;
        public bool IsPlayerRelevant;
        public bool HasActiveInteraction;
        public bool IsAgent;
        public bool IsAnimal;
        public bool UsesAudio;

        public bool IsProtected => IsPlayerRelevant || HasActiveInteraction;
    }

    [Serializable]
    public sealed class D16PerformanceDecision
    {
        public string Id;
        public D16SimulationTier SimulationTier;
        public bool AudioEnabled;
        public bool IsProtected;
        public bool IsAgent;
        public bool IsAnimal;
        public bool UsesAudio;
    }

    [Serializable]
    public sealed class D16PerformanceBudgetSnapshot
    {
        public int CandidateCount;
        public int SkippedInvalidCandidateCount;
        public int DuplicateCandidateCount;
        public int ActiveAgentCount;
        public int ActiveAnimalCount;
        public int AudibleSourceCount;
        public int ProtectedOverBudgetCount;
        public bool BudgetSatisfied;
        public bool HighPressure;
        public List<D16PerformanceDecision> Decisions = new List<D16PerformanceDecision>();
    }
}
