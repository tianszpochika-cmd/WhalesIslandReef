using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whalebone.Game.Config;

namespace Whalebone.Agent
{
    public enum CrewProposalKind
    {
        None,
        Route,
        Duty,
        Relation
    }

    public enum CrewRelationDecisionStatus
    {
        Applied,
        NoChange,
        MissingCauseEvent,
        InvalidProposal,
        WrongAgent
    }

    /// <summary>
    /// 从 D8 FactEvent 投影给 Agent 的只读摘要。AgentCore 不引用 Systems/EventLog，
    /// 因此由事件 owner 传入已校验的字段；Agent 只保存摘要，不写事实。
    /// </summary>
    public sealed class CrewFactSummary
    {
        public string EventId { get; private set; }
        public int WorldDay { get; private set; }
        public string LocationRef { get; private set; }
        public string OwnerSystem { get; private set; }
        public string ResultCode { get; private set; }
        public string Summary { get; private set; }
        public IReadOnlyList<string> CauseEventIds { get; private set; }

        public CrewFactSummary(string eventId, int worldDay, string locationRef,
            string ownerSystem, string resultCode, string summary,
            IEnumerable<string> causeEventIds = null)
        {
            EventId = eventId ?? string.Empty;
            WorldDay = worldDay;
            LocationRef = locationRef ?? string.Empty;
            OwnerSystem = ownerSystem ?? string.Empty;
            ResultCode = resultCode ?? string.Empty;
            Summary = summary ?? string.Empty;
            CauseEventIds = D9CrewAgentIds.Copy(causeEventIds);
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(EventId)
            && WorldDay >= 0
            && !string.IsNullOrWhiteSpace(LocationRef)
            && !string.IsNullOrWhiteSpace(OwnerSystem)
            && !string.IsNullOrWhiteSpace(ResultCode)
            && !string.IsNullOrWhiteSpace(Summary);

        public string ToMemoryText()
        {
            string causes = CauseEventIds.Count == 0
                ? string.Empty
                : " cause_event_ids=" + string.Join(",", D9CrewAgentIds.ToArray(CauseEventIds));
            return "事实 " + EventId + " [" + OwnerSystem + "] " + Summary + causes;
        }
    }

    /// <summary>AgentReply 经边界校验后的 typed 提议；没有执行方法。</summary>
    public sealed class CrewAgentProposal
    {
        public string AgentId { get; private set; }
        public CrewProposalKind Kind { get; private set; }
        public string ActionName { get; private set; }
        public string TargetRef { get; private set; }
        public string DestinationRef { get; private set; }
        public CrewDuty ProposedDuty { get; private set; }
        public float AffinityDelta { get; private set; }
        public RelationKind RelationKind { get; private set; }
        public RelationStance RelationStance { get; private set; }
        public string Basis { get; private set; }
        public IReadOnlyList<string> CauseEventIds { get; private set; }

        public CrewAgentProposal(string agentId, CrewProposalKind kind, string actionName,
            string targetRef, string destinationRef, CrewDuty proposedDuty,
            float affinityDelta, RelationKind relationKind, RelationStance relationStance,
            string basis, IEnumerable<string> causeEventIds)
        {
            AgentId = agentId ?? string.Empty;
            Kind = kind;
            ActionName = actionName ?? string.Empty;
            TargetRef = targetRef ?? string.Empty;
            DestinationRef = destinationRef ?? string.Empty;
            ProposedDuty = proposedDuty;
            AffinityDelta = affinityDelta;
            RelationKind = relationKind;
            RelationStance = relationStance;
            Basis = basis ?? string.Empty;
            CauseEventIds = D9CrewAgentIds.Copy(causeEventIds);
        }

        public bool IsActionProposal => Kind == CrewProposalKind.Route || Kind == CrewProposalKind.Duty;
        public bool IsRelationProposal => Kind == CrewProposalKind.Relation;
    }

    public sealed class CrewAgentProposalSet
    {
        public IReadOnlyList<CrewAgentProposal> Proposals { get; private set; }

        private CrewAgentProposalSet(List<CrewAgentProposal> proposals)
        {
            Proposals = proposals.AsReadOnly();
        }

        public static CrewAgentProposalSet FromReply(CrewAgent agent, AgentReply reply,
            IEnumerable<CrewFactSummary> facts, string relationTargetRef)
        {
            var proposals = new List<CrewAgentProposal>();
            if (agent == null || reply == null) return new CrewAgentProposalSet(proposals);

            var causes = new List<string>();
            if (facts != null)
            {
                foreach (var fact in facts)
                {
                    if (fact == null || !fact.IsValid) continue;
                    D9CrewAgentIds.AddUnique(causes, fact.EventId);
                }
            }

            var action = reply.ActionIntent;
            if (action != null && action.Name == "set_course")
            {
                string destination = StringParameter(action, "destination_ref");
                if (!string.IsNullOrWhiteSpace(destination))
                {
                    proposals.Add(new CrewAgentProposal(
                        agent.Id, CrewProposalKind.Route, action.Name, action.TargetRef,
                        destination, CrewDuty.None, 0f, RelationKind.None, RelationStance.None,
                        "船员提出航线建议。", causes));
                }
            }
            else if (action != null && action.Name == "assign_duty")
            {
                CrewDuty duty = ParseDuty(StringParameter(action, "duty"));
                if (duty != CrewDuty.None)
                {
                    proposals.Add(new CrewAgentProposal(
                        agent.Id, CrewProposalKind.Duty, action.Name, action.TargetRef,
                        string.Empty, duty, 0f, RelationKind.None, RelationStance.None,
                        "船员提出岗位安排建议。", causes));
                }
            }

            if (Math.Abs(reply.AffinityChange) > 0.0001f)
            {
                var relation = reply.RelationProposal ?? RelationProposal.Empty();
                RelationKind kind = relation.Kind == RelationKind.None
                    ? RelationKind.Affinity : relation.Kind;
                string target = string.IsNullOrWhiteSpace(relation.TargetRef)
                    || relation.TargetRef == "none" ? relationTargetRef : relation.TargetRef;
                proposals.Add(new CrewAgentProposal(
                    agent.Id, CrewProposalKind.Relation, "relation_proposal", target,
                    string.Empty, CrewDuty.None, reply.AffinityChange, kind,
                    relation.Stance, relation.Basis, causes));
            }

            return new CrewAgentProposalSet(proposals);
        }

        private static string StringParameter(ActionIntent action, string key)
        {
            object value;
            return action.Parameters != null && action.Parameters.TryGetValue(key, out value)
                ? value as string : null;
        }

        private static CrewDuty ParseDuty(string value)
        {
            switch (value)
            {
                case "helm": return CrewDuty.Helm;
                case "lookout": return CrewDuty.Lookout;
                case "gunner": return CrewDuty.Gunner;
                case "repair": return CrewDuty.Repair;
                case "cook": return CrewDuty.Cook;
                case "medic": return CrewDuty.Medic;
                case "deckhand": return CrewDuty.Deckhand;
                default: return CrewDuty.None;
            }
        }
    }

    public sealed class CrewRelationDecision
    {
        public CrewRelationDecisionStatus Status { get; private set; }
        public string Reason { get; private set; }
        public float Before { get; private set; }
        public float AppliedDelta { get; private set; }
        public float After { get; private set; }
        public IReadOnlyList<string> CauseEventIds { get; private set; }

        public bool Accepted => Status == CrewRelationDecisionStatus.Applied;

        internal CrewRelationDecision(CrewRelationDecisionStatus status, string reason,
            float before, float appliedDelta, float after, IEnumerable<string> causeEventIds)
        {
            Status = status;
            Reason = reason ?? string.Empty;
            Before = before;
            AppliedDelta = appliedDelta;
            After = after;
            CauseEventIds = D9CrewAgentIds.Copy(causeEventIds);
        }
    }

    /// <summary>
    /// D9 关系预检门。只有带事实事件 ID 的提议才会被批准进入 CrewRelationSystem；
    /// 本类不写 CrewStats，航线/岗位提议也没有执行入口。
    /// </summary>
    public static class CrewRelationArbiter
    {
        public static CrewRelationDecision Evaluate(CrewAgent agent, CrewAgentProposal proposal)
        {
            if (agent == null || proposal == null)
                return new CrewRelationDecision(CrewRelationDecisionStatus.InvalidProposal,
                    "缺少船员或提议。", 0f, 0f, 0f, null);
            if (proposal.AgentId != agent.Id)
                return new CrewRelationDecision(CrewRelationDecisionStatus.WrongAgent,
                    "提议不属于当前船员。", agent.Stats.Affinity, 0f, agent.Stats.Affinity,
                    proposal.CauseEventIds);
            if (!proposal.IsRelationProposal || proposal.CauseEventIds.Count == 0)
                return new CrewRelationDecision(CrewRelationDecisionStatus.MissingCauseEvent,
                    "关系变化必须绑定 cause_event_ids。", agent.Stats.Affinity, 0f,
                    agent.Stats.Affinity, proposal.CauseEventIds);
            if (float.IsNaN(proposal.AffinityDelta) || float.IsInfinity(proposal.AffinityDelta))
                return new CrewRelationDecision(CrewRelationDecisionStatus.InvalidProposal,
                    "关系提议数值非法。", agent.Stats.Affinity, 0f, agent.Stats.Affinity,
                    proposal.CauseEventIds);

            float delta = NumericConfiguration.ClampRelationProposal(proposal.AffinityDelta);
            float before = agent.Stats.Affinity;
            if (Math.Abs(delta) < 0.0001f)
                return new CrewRelationDecision(CrewRelationDecisionStatus.NoChange,
                    "关系提议没有变化。", before, 0f, before, proposal.CauseEventIds);

            float after = NumericConfiguration.ClampRelation(before + delta);
            return new CrewRelationDecision(CrewRelationDecisionStatus.Applied,
                "关系提议已通过 cause_event_ids 预检，等待 CrewRelationSystem 写入。", before,
                delta, after, proposal.CauseEventIds);
        }
    }

    /// <summary>D9 无模型/断网客户端；所有表达仍由本地模板生成。</summary>
    public sealed class D9OfflineTemplateClient : ILLMClient
    {
        public string Name => "D9.TemplateOffline";
        public bool Available => false;

        public Task<string> CompleteAsync(string system, string user)
        {
            return Task.FromException<string>(new InvalidOperationException("d9_model_unavailable"));
        }
    }

    /// <summary>
    /// D9 一个核心船员的纯 C# 纵切片。它只产出 AgentReply/typed proposal，领域执行器在外部接管。
    /// </summary>
    public sealed class D9CrewAgentVerticalSlice
    {
        public const string IsaacReedId = "crew_isaac_reed";
        public const string ShipMoonwakeRef = "ship:ship_moonwake";
        public const string RelationTargetRef = "player:captain";

        private readonly List<CrewFactSummary> _facts = new List<CrewFactSummary>();

        public CrewAgent Agent { get; private set; }
        public IReadOnlyList<CrewFactSummary> FactSummaries => _facts.AsReadOnly();

        private D9CrewAgentVerticalSlice(CrewAgent agent)
        {
            Agent = agent;
        }

        public static D9CrewAgentVerticalSlice CreateIsaacReed(ILLMClient llm = null)
        {
            var persona = new PersonaDefinition("艾萨克·里德", "水手长");
            persona.PrimaryDuty = CrewDuty.Deckhand;
            persona.QualifiedDuties.Clear();
            persona.QualifiedDuties.Add(CrewDuty.Deckhand);
            persona.QualifiedDuties.Add(CrewDuty.Navigator);
            persona.QualifiedDuties.Add(CrewDuty.Lookout);
            persona.QualifiedDuties.Add(CrewDuty.Repair);
            persona.Traits.Add("谨慎");
            persona.Traits.Add("识潮");
            persona.Backstory = "在圣玛丽近海跑了十年的水手长，熟悉浅滩、雨云和渔场。";
            persona.SpeechStyle = "短句、克制，先报风险再提建议。";

            var agent = new CrewAgent(IsaacReedId, persona, new InMemoryStore());
            var slice = new D9CrewAgentVerticalSlice(agent);
            var context = BuildProposalContext(agent.Id);
            var client = llm ?? new D9OfflineTemplateClient();
            agent.Session = new DialogueSession(agent, client, new MutinyMonitor(agent.Stats),
                context, input => TemplateReply(agent, input));
            return slice;
        }

        public AgentReply Speak(string playerInput)
        {
            return Agent.Session == null ? TemplateReply(Agent, playerInput) : Agent.Session.Speak(playerInput);
        }

        public bool RecordFactSummary(CrewFactSummary summary, float importance = 0.85f)
        {
            if (summary == null || !summary.IsValid) return false;
            foreach (var existing in _facts)
                if (existing.EventId == summary.EventId) return false;
            _facts.Add(summary);
            Agent.Memory.RecordFactSummary(summary.EventId, summary.ToMemoryText(), importance);
            return true;
        }

        public CrewAgentProposalSet ProjectReply(AgentReply reply)
        {
            return CrewAgentProposalSet.FromReply(Agent, reply, _facts, RelationTargetRef);
        }

        public CrewRelationDecision EvaluateRelationProposal(CrewAgentProposal proposal)
        {
            return CrewRelationArbiter.Evaluate(Agent, proposal);
        }

        private static ActionValidationContext BuildProposalContext(string agentId)
        {
            var context = new ActionValidationContext();
            context.AuthorizedActorRefs.Add("crew:" + agentId);
            context.AuthorizedTargetRefs.Add(ShipMoonwakeRef);
            context.AuthorizedTargetRefs.Add("crew:" + agentId);
            context.Permissions.Add("ship.course");
            context.Permissions.Add("crew.assign_duty");
            context.SatisfiedPrerequisites.Add("ship.present");
            context.SatisfiedPrerequisites.Add("chart.available");
            context.SatisfiedPrerequisites.Add("crew.on_duty");
            context.SatisfiedPrerequisites.Add("crew.onboard");
            context.SatisfiedPrerequisites.Add("duty_roster_open");
            return context;
        }

        private static AgentReply TemplateReply(CrewAgent agent, string input)
        {
            string text = input ?? string.Empty;
            if (ContainsAny(text, "航线", "路线", "绿湾", "村落"))
            {
                return ActionReply(agent, "走绿湾，先避开风口。", "set_course", ShipMoonwakeRef,
                    new Dictionary<string, object> { ["destination_ref"] = "port:green_cay" });
            }
            if (ContainsAny(text, "岗位", "值班", "瞭望", "安排"))
            {
                string crewRef = "crew:" + agent.Id;
                return ActionReply(agent, "让我去瞭望，风浪有变我先报。", "assign_duty", crewRef,
                    new Dictionary<string, object> { ["crew_ref"] = crewRef, ["duty"] = "lookout" });
            }
            return TemplateFallback.Reply(agent.Persona, input);
        }

        private static AgentReply ActionReply(CrewAgent agent, string speech, string actionName,
            string targetRef, Dictionary<string, object> parameters)
        {
            string actorRef = "crew:" + agent.Id;
            return new AgentReply
            {
                SchemaVersion = AgentReply.CurrentSchemaVersion,
                Speech = speech,
                DialogueIntent = DialogueIntent.Suggest,
                Emotion = Emotion.Concerned,
                AffinityChange = 0f,
                RelationProposal = RelationProposal.Empty(),
                NegotiationTone = NegotiationTone.Firm,
                RefusalReason = RefusalReason.None,
                Action = actionName,
                ActionIntent = new ActionIntent
                {
                    Name = actionName,
                    ActorRef = actorRef,
                    TargetRef = targetRef,
                    Parameters = parameters,
                },
            };
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            foreach (var candidate in candidates)
                if (value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
    }

    internal static class D9CrewAgentIds
    {
        public static List<string> Copy(IEnumerable<string> values)
        {
            var result = new List<string>();
            if (values == null) return result;
            foreach (var value in values) AddUnique(result, value);
            return result;
        }

        public static void AddUnique(List<string> values, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || values.Contains(value)) return;
            values.Add(value);
        }

        public static string[] ToArray(IReadOnlyList<string> values)
        {
            var result = new string[values == null ? 0 : values.Count];
            if (values != null)
                for (int i = 0; i < values.Count; i++) result[i] = values[i];
            return result;
        }
    }
}
