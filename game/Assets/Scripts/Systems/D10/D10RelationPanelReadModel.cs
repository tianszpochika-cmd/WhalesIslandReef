// ============================================================
// D10RelationPanelReadModel —— 关系面板只读投影
//
// 只读取 RelationSystem 的副本和 EventLog 的事实副本，不提供任何关系写入口。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    [Serializable]
    public sealed class RelationPanelCause
    {
        public string EventId;
        public bool Found;
        public string Text;
        public string OwnerSystem;
        public int WorldDay;
        public long SaveRevision;
        public WorldEventStatus Status;
    }

    [Serializable]
    public sealed class RelationPanelRow
    {
        public RelationEdge Edge;
        public string Explanation;
        public List<RelationPanelCause> Causes = new List<RelationPanelCause>();
    }

    [Serializable]
    public sealed class RelationPanelSnapshot
    {
        public string Status;
        public string Message;
        public List<RelationPanelRow> Rows = new List<RelationPanelRow>();

        public bool IsAvailable { get { return Status == "ok"; } }
    }

    public static class D10RelationPanelReadModel
    {
        public static RelationPanelSnapshot Build(RelationSystem relationSystem, EventLog eventLog,
            string sourceId = null, string targetId = null)
        {
            var result = new RelationPanelSnapshot { Status = "ok", Message = "关系快照已读取。" };
            if (relationSystem == null)
                return Missing("relation_system_missing", "关系系统尚未接入。");
            if (!string.IsNullOrWhiteSpace(sourceId) && !relationSystem.KnowsCharacter(sourceId))
                return Missing("unknown_character", "未知角色：" + sourceId + "。");
            if (!string.IsNullOrWhiteSpace(targetId) && !relationSystem.KnowsCharacter(targetId))
                return Missing("unknown_character", "未知角色：" + targetId + "。");

            foreach (var edge in relationSystem.ReadEdges())
            {
                if (!string.IsNullOrWhiteSpace(sourceId) && edge.SourceId != sourceId) continue;
                if (!string.IsNullOrWhiteSpace(targetId) && edge.TargetId != targetId) continue;
                var row = new RelationPanelRow { Edge = edge };
                if (edge.CauseEventIds != null)
                    foreach (var eventId in edge.CauseEventIds)
                        row.Causes.Add(ExplainCause(eventId, edge.LastChangeSaveRevision, eventLog));
                row.Explanation = BuildExplanation(row.Causes);
                result.Rows.Add(row);
            }
            return result;
        }

        private static RelationPanelCause ExplainCause(string eventId, long revision, EventLog eventLog)
        {
            if (eventLog != null)
            {
                var exact = eventLog.Find(eventId, revision);
                if (exact != null) return FromFact(exact);
                foreach (var fact in eventLog.Facts)
                    if (fact != null && fact.EventId == eventId) return FromFact(fact);
            }
            return new RelationPanelCause
            {
                EventId = eventId ?? string.Empty,
                Found = false,
                Text = "原因事实缺失：" + (eventId ?? string.Empty),
            };
        }

        private static RelationPanelCause FromFact(FactEvent fact)
        {
            return new RelationPanelCause
            {
                EventId = fact.EventId,
                Found = true,
                Text = fact.Result == null || string.IsNullOrWhiteSpace(fact.Result.Summary)
                    ? fact.EventId : fact.Result.Summary,
                OwnerSystem = fact.OwnerSystem,
                WorldDay = fact.WorldDay,
                SaveRevision = fact.SaveRevision,
                Status = fact.OutcomeStatus,
            };
        }

        private static string BuildExplanation(List<RelationPanelCause> causes)
        {
            if (causes == null || causes.Count == 0) return "暂无原因事实。";
            var lines = new List<string>();
            foreach (var cause in causes)
                lines.Add(cause.Found ? "原因事实 " + cause.EventId + "：" + cause.Text : cause.Text);
            return string.Join("；", lines.ToArray());
        }

        private static RelationPanelSnapshot Missing(string status, string message)
        {
            return new RelationPanelSnapshot { Status = status, Message = message };
        }
    }
}
