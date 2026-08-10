using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D2;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using System.Collections.Generic;

namespace D2.Tests
{
    public class CrewRelationSystemTests
    {
        [Test]
        public void AgentProposal_CannotWriteWithoutFactEvent()
        {
            var agent = new CrewAgent("crew", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var reply = new AgentReply { Speech = "好", AffinityChange = 4f, Action = "none" };
            Assert.IsFalse(CrewRelationSystem.ApplyAgentProposal(agent, reply));
            Assert.AreEqual(0f, agent.Stats.Affinity, 0.0001f);

            var log = new EventLog();
            var fact = log.Append(new WorldEvent
            {
                EventId = "event_d10_affinity_test",
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = 1,
                LocationRef = "nearshore_saint_marys",
                ActorIds = new List<string> { "crew", "player" },
                InputFacts = new List<string> { "dialogue" },
                OwnerSystem = "CrewSystem",
                Result = new EventResult { ResultCode = "dialogue_fact", Summary = "船员听见船长兑现承诺。", FactIds = new List<string> { "crew", "player" } },
                CauseIds = new List<string>(),
                SaveRevision = 1,
            }).Fact;
            var relationSystem = new RelationSystem(log, new[] { "crew", "player" });
            var result = relationSystem.ApplyFactEvent(fact, new RelationChangeIntent("crew", "player", RelationType.Affinity, reply.AffinityChange));
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(4f, result.After, 0.0001f);
            Assert.AreEqual(0f, agent.Stats.Affinity, 0.0001f);
        }

        [Test]
        public void InvalidReply_IsIgnored()
        {
            var agent = new CrewAgent("crew", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var reply = new AgentReply { Speech = "", AffinityChange = 10f, Action = "none" };
            Assert.IsFalse(CrewRelationSystem.ApplyAgentProposal(agent, reply));
            Assert.AreEqual(0f, agent.Stats.Affinity);
        }
    }
}
