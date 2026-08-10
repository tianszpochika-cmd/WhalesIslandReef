// ============================================================
// D9CrewEventSystem —— CrewSystem 的 D9 事实接线
//
// AgentCore 只返回经过边界校验的对白/typed proposal；本类只把已有事实摘要
// 投影给 Agent，并由 CrewSystem 记录 event_d9_crew_report。它不执行航线、岗位
// 或关系提议，也不复制任何强逻辑数值公式。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    [Serializable]
    public sealed class D9CrewDiscussionResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public AgentReply Reply;
        public CrewAgentProposalSet Proposals;
        public FactEvent Fact;
    }

    /// <summary>只做 CrewSystem/EventLog 的事实接线，不执行 Agent proposal。</summary>
    public sealed class D9CrewEventSystem
    {
        public const string CrewReportEventId = "event_d9_crew_report";
        private readonly EventLog _eventLog;
        private long _nextRevision;

        public D9CrewEventSystem(EventLog eventLog)
        {
            _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");
            _nextRevision = _eventLog.NextSequence;
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D9CrewDiscussionResult Discuss(D9CrewAgentVerticalSlice crew, string prompt,
            int worldDay, long saveRevision = 0)
        {
            if (crew == null || crew.Agent == null)
                return Reject("crew_missing", "核心船员尚未进入讨论状态。");

            var causes = new List<string>();
            foreach (var fact in _eventLog.ReadFactsForUi(8))
            {
                if (fact == null) continue;
                D9CrewAgentIdsForIntegration.AddUnique(causes, fact.EventId);
                crew.RecordFactSummary(new CrewFactSummary(
                    fact.EventId,
                    fact.WorldDay,
                    fact.LocationRef,
                    fact.OwnerSystem,
                    fact.Result == null ? "fact" : fact.Result.ResultCode,
                    fact.Result == null ? fact.EventId : fact.Result.Summary,
                    fact.CauseIds));
            }

            AgentReply reply = crew.Speak(prompt ?? string.Empty);
            var proposals = crew.ProjectReply(reply);
            long revision = saveRevision > 0 ? saveRevision : ++_nextRevision;
            var existing = _eventLog.Find(CrewReportEventId, revision);
            if (existing != null)
            {
                return new D9CrewDiscussionResult
                {
                    Succeeded = true,
                    Code = "duplicate_settlement",
                    Message = "船员报告事实已经记录，未重复追加。",
                    Reply = reply,
                    Proposals = proposals,
                    Fact = existing,
                };
            }

            var values = new Dictionary<string, string>
            {
                ["crew_id"] = crew.Agent.Id ?? string.Empty,
                ["speech"] = reply == null ? string.Empty : (reply.Speech ?? string.Empty),
                ["action_name"] = "none",
                ["destination_ref"] = string.Empty,
                ["proposed_duty"] = string.Empty,
                ["proposal_count"] = proposals == null ? "0" : proposals.Proposals.Count.ToString(),
            };
            if (reply != null && reply.ActionIntent != null)
            {
                values["action_name"] = reply.ActionIntent.Name ?? string.Empty;
                values["target_ref"] = reply.ActionIntent.TargetRef ?? string.Empty;
                values["destination_ref"] = StringParameter(reply.ActionIntent, "destination_ref");
                values["duty"] = StringParameter(reply.ActionIntent, "duty");
            }

            var append = _eventLog.Append(new WorldEvent(
                CrewReportEventId,
                Math.Max(0, worldDay),
                "nearshore_saint_marys",
                "CrewSystem",
                new EventResult
                {
                    ResultCode = "crew_report_recorded",
                    Summary = "艾萨克已基于航程事实提出航线/岗位建议；提议等待对应系统裁决。",
                    FactIds = new List<string> { crew.Agent.Id ?? string.Empty },
                    Values = values,
                },
                new[] { crew.Agent.Id ?? "crew_isaac_reed" },
                causes,
                new string[0],
                Math.Max(0, revision)));
            var factResult = append.Fact;
            if (factResult != null)
            {
                crew.RecordFactSummary(new CrewFactSummary(
                    factResult.EventId,
                    factResult.WorldDay,
                    factResult.LocationRef,
                    factResult.OwnerSystem,
                    factResult.Result.ResultCode,
                    factResult.Result.Summary,
                    factResult.CauseIds));
            }
            return new D9CrewDiscussionResult
            {
                Succeeded = factResult != null,
                Code = factResult == null ? "append_failed" : "ok",
                Message = factResult == null ? "船员报告未能进入事实日志。" : "船员报告已记录，提议未被直接执行。",
                Reply = reply,
                Proposals = proposals,
                Fact = factResult,
            };
        }

        private static string StringParameter(ActionIntent action, string key)
        {
            object value;
            return action != null && action.Parameters != null && action.Parameters.TryGetValue(key, out value)
                ? value as string ?? string.Empty : string.Empty;
        }

        private static D9CrewDiscussionResult Reject(string code, string message)
        {
            return new D9CrewDiscussionResult { Succeeded = false, Code = code, Message = message };
        }
    }

    internal static class D9CrewAgentIdsForIntegration
    {
        public static void AddUnique(List<string> values, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !values.Contains(value)) values.Add(value);
        }
    }
}
