// ============================================================
// D11EcologyVoyageIntegrationContracts —— D11-S01 集成结果契约
//
// 本文件只描述跨 owner 的消费结果。生态、图鉴、势力、任务和关系状态
// 仍由各自系统写入；集成层不保存第二份 canonical 数值。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D4;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    [Serializable]
    public sealed class D11S01RescueIntegrationResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public D11RareAnimalResult Rescue;
        public FactionActionResult Faction;
        public FactEvent FactionFact;
        public FactEvent QuestFact;

        public D11S01RescueIntegrationResult Clone()
        {
            return new D11S01RescueIntegrationResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                Rescue = Rescue == null ? null : Rescue.Clone(),
                Faction = Faction == null ? null : new FactionActionResult
                {
                    Accepted = Faction.Accepted,
                    ReputationDelta = Faction.ReputationDelta,
                    WantedDelta = Faction.WantedDelta,
                    Message = Faction.Message,
                },
                FactionFact = FactionFact == null ? null : FactionFact.CloneFact(),
                QuestFact = QuestFact == null ? null : QuestFact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D11S01PetRelationResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public string PetId;
        public int AppliedCount;
        public int DuplicateCount;
        public List<RelationApplyResult> Relations = new List<RelationApplyResult>();

        public D11S01PetRelationResult Clone()
        {
            var result = new D11S01PetRelationResult
            {
                Succeeded = Succeeded,
                Code = Code,
                Message = Message,
                PetId = PetId,
                AppliedCount = AppliedCount,
                DuplicateCount = DuplicateCount,
            };
            if (Relations != null)
                foreach (var relation in Relations)
                    if (relation != null) result.Relations.Add(relation);
            return result;
        }
    }
}
