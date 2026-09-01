// D20RelationActionAdapter —— D10 关系图到对话/合作资格的只读投影。
//
// RelationSystem 是关系值的唯一写者。本文件只使用 KnowsCharacter 与
// ReadEdge 的公开副本，不调用关系变更/角色注册入口，也不把资格判断结果
// 写回关系图。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D10;

namespace Whalebone.Systems.D20
{
    public enum D20RelationActionKind
    {
        Dialogue = 1,
        Cooperation = 2,
    }

    public enum D20RelationActionFailureCode
    {
        None = 0,
        MissingRelationSystem = 1,
        MissingCharacterId = 2,
        UnknownCharacter = 3,
        UnsupportedAction = 4,
        NoRelationEvidence = 5,
        FearTooHigh = 6,
        TrustTooLow = 7,
        AffinityTooLow = 8,
        RelationDataInvalid = 9,
    }

    [Serializable]
    public sealed class D20RelationActionEvaluation
    {
        public string SourceId = string.Empty;
        public string TargetId = string.Empty;
        public D20RelationActionKind ActionKind;
        public bool Eligible;
        public bool RelationDataAvailable;
        public bool HasRelationEvidence;
        public D20RelationActionFailureCode FailureCode;
        public float Affinity;
        public float Trust;
        public float Respect;
        public float Fear;
        public List<RelationType> EvidenceTypes = new List<RelationType>();
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;

        public D20RelationActionEvaluation Clone()
        {
            var result = new D20RelationActionEvaluation
            {
                SourceId = SourceId,
                TargetId = TargetId,
                ActionKind = ActionKind,
                Eligible = Eligible,
                RelationDataAvailable = RelationDataAvailable,
                HasRelationEvidence = HasRelationEvidence,
                FailureCode = FailureCode,
                Affinity = Affinity,
                Trust = Trust,
                Respect = Respect,
                Fear = Fear,
                EvidenceTypes = new List<RelationType>(),
                ReasonCode = ReasonCode,
                FeedbackKey = FeedbackKey,
            };

            if (EvidenceTypes != null) result.EvidenceTypes.AddRange(EvidenceTypes);
            return result;
        }
    }

    public static class D20RelationActionAdapter
    {
        public const string OwnerSystem = "D20RelationActionAdapter";
        public const float DialogueFearBlockThreshold = 90f;
        public const float CooperationFearBlockThreshold = 80f;
        public const float CooperationMinimumTrust = 40f;
        public const float CooperationMinimumRespect = 30f;
        public const float CooperationMinimumAffinity = 0f;
        public const float CooperationAffinityBlockThreshold = -20f;

        /// <summary>将通用 D20 动作映射到关系动作，不执行关系写入。</summary>
        public static D20RelationActionEvaluation Evaluate(
            RelationSystem relationSystem,
            string sourceId,
            string targetId,
            D20ActionIntentKind actionKind)
        {
            if (actionKind == D20ActionIntentKind.Talk)
                return Evaluate(relationSystem, sourceId, targetId, D20RelationActionKind.Dialogue);
            if (actionKind == D20ActionIntentKind.Interact)
                return Evaluate(relationSystem, sourceId, targetId, D20RelationActionKind.Cooperation);

            var unsupported = CreateBase(sourceId, targetId, D20RelationActionKind.Dialogue);
            unsupported.Eligible = false;
            unsupported.FailureCode = D20RelationActionFailureCode.UnsupportedAction;
            unsupported.ReasonCode = "relation_action_unsupported";
            unsupported.FeedbackKey = "relation.feedback.unsupported_action";
            return unsupported;
        }

        /// <summary>
        /// 从 RelationSystem 的公开关系边副本评估对话或合作资格。
        /// 缺少关系边时对话可安全继续，合作则明确返回无证据失败。
        /// </summary>
        public static D20RelationActionEvaluation Evaluate(
            RelationSystem relationSystem,
            string sourceId,
            string targetId,
            D20RelationActionKind actionKind)
        {
            var result = CreateBase(sourceId, targetId, actionKind);

            if (!Enum.IsDefined(typeof(D20RelationActionKind), actionKind))
            {
                result.Eligible = false;
                result.FailureCode = D20RelationActionFailureCode.UnsupportedAction;
                result.ReasonCode = "relation_action_unsupported";
                result.FeedbackKey = "relation.feedback.unsupported_action";
                return result;
            }

            if (relationSystem == null)
            {
                result.Eligible = false;
                result.FailureCode = D20RelationActionFailureCode.MissingRelationSystem;
                result.ReasonCode = "relation_system_missing";
                result.FeedbackKey = "relation.feedback.unavailable";
                return result;
            }

            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(targetId))
            {
                result.Eligible = false;
                result.FailureCode = D20RelationActionFailureCode.MissingCharacterId;
                result.ReasonCode = "relation_character_missing";
                result.FeedbackKey = "relation.feedback.invalid";
                return result;
            }

            if (!relationSystem.KnowsCharacter(sourceId) || !relationSystem.KnowsCharacter(targetId))
            {
                result.Eligible = false;
                result.FailureCode = D20RelationActionFailureCode.UnknownCharacter;
                result.ReasonCode = "relation_character_unknown";
                result.FeedbackKey = "relation.feedback.unknown_character";
                return result;
            }

            result.RelationDataAvailable = true;
            bool hasAffinity;
            bool hasTrust;
            bool hasRespect;
            bool hasFear;

            if (!ReadEdge(relationSystem, sourceId, targetId, RelationType.Affinity,
                    result, out hasAffinity)
                || !ReadEdge(relationSystem, sourceId, targetId, RelationType.Trust,
                    result, out hasTrust)
                || !ReadEdge(relationSystem, sourceId, targetId, RelationType.Respect,
                    result, out hasRespect)
                || !ReadEdge(relationSystem, sourceId, targetId, RelationType.Fear,
                    result, out hasFear))
            {
                result.Eligible = false;
                result.FailureCode = D20RelationActionFailureCode.RelationDataInvalid;
                result.ReasonCode = "relation_data_invalid";
                result.FeedbackKey = "relation.feedback.invalid";
                return result;
            }

            if (actionKind == D20RelationActionKind.Dialogue)
            {
                if (hasFear && result.Fear >= DialogueFearBlockThreshold)
                {
                    return Restrict(result, D20RelationActionFailureCode.FearTooHigh,
                        "relation_dialogue_blocked_by_fear", "relation.feedback.fear_high");
                }

                result.ReasonCode = hasFear && result.Fear > 0f
                    ? "relation_dialogue_guarded" : "relation_dialogue_available";
                result.FeedbackKey = hasFear && result.Fear > 0f
                    ? "relation.feedback.guarded" : "relation.feedback.dialogue";
                return result;
            }

            if (!result.HasRelationEvidence)
            {
                return Restrict(result, D20RelationActionFailureCode.NoRelationEvidence,
                    "relation_cooperation_evidence_missing", "relation.feedback.no_evidence");
            }

            if (hasFear && result.Fear >= CooperationFearBlockThreshold)
            {
                return Restrict(result, D20RelationActionFailureCode.FearTooHigh,
                    "relation_cooperation_blocked_by_fear", "relation.feedback.fear_high");
            }

            if (hasAffinity && result.Affinity < CooperationAffinityBlockThreshold)
            {
                return Restrict(result, D20RelationActionFailureCode.AffinityTooLow,
                    "relation_cooperation_affinity_low", "relation.feedback.affinity_low");
            }

            bool trustSupports = hasTrust && result.Trust >= CooperationMinimumTrust;
            bool respectSupports = hasRespect && result.Respect >= CooperationMinimumRespect;
            bool affinitySupports = hasAffinity && result.Affinity >= CooperationMinimumAffinity;
            if (!trustSupports && !respectSupports && !affinitySupports)
            {
                return Restrict(result, hasTrust
                        ? D20RelationActionFailureCode.TrustTooLow
                        : D20RelationActionFailureCode.AffinityTooLow,
                    hasTrust ? "relation_cooperation_trust_low" : "relation_cooperation_support_low",
                    hasTrust ? "relation.feedback.trust_low" : "relation.feedback.affinity_low");
            }

            result.ReasonCode = "relation_cooperation_qualified";
            result.FeedbackKey = "relation.feedback.cooperation";
            return result;
        }

        private static D20RelationActionEvaluation CreateBase(
            string sourceId,
            string targetId,
            D20RelationActionKind actionKind)
        {
            return new D20RelationActionEvaluation
            {
                SourceId = sourceId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
                ActionKind = actionKind,
                Eligible = true,
                FailureCode = D20RelationActionFailureCode.None,
            };
        }

        private static D20RelationActionEvaluation Restrict(
            D20RelationActionEvaluation result,
            D20RelationActionFailureCode failureCode,
            string reasonCode,
            string feedbackKey)
        {
            result.Eligible = false;
            result.FailureCode = failureCode;
            result.ReasonCode = reasonCode;
            result.FeedbackKey = feedbackKey;
            return result;
        }

        private static bool ReadEdge(
            RelationSystem relationSystem,
            string sourceId,
            string targetId,
            RelationType relationType,
            D20RelationActionEvaluation result,
            out bool present)
        {
            present = false;
            var edge = relationSystem.ReadEdge(sourceId, targetId, relationType);
            if (edge == null) return true;

            if (edge.RelationType != relationType
                || edge.SchemaVersion != D10RelationSchema.CurrentVersion
                || !string.Equals(edge.SourceId, sourceId, StringComparison.Ordinal)
                || !string.Equals(edge.TargetId, targetId, StringComparison.Ordinal)
                || float.IsNaN(edge.Value) || float.IsInfinity(edge.Value)
                || float.IsNaN(edge.Confidence) || float.IsInfinity(edge.Confidence)
                || edge.Confidence < 0f || edge.Confidence > 1f
                || RelationValueRules.ClampValue(relationType, edge.Value) != edge.Value)
                return false;

            present = true;
            result.HasRelationEvidence = true;
            result.EvidenceTypes.Add(relationType);
            switch (relationType)
            {
                case RelationType.Affinity:
                    result.Affinity = edge.Value;
                    break;
                case RelationType.Trust:
                    result.Trust = edge.Value;
                    break;
                case RelationType.Respect:
                    result.Respect = edge.Value;
                    break;
                case RelationType.Fear:
                    result.Fear = edge.Value;
                    break;
            }
            return true;
        }
    }
}
