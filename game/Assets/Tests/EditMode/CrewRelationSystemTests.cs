using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D2;

namespace D2.Tests
{
    public class CrewRelationSystemTests
    {
        [Test]
        public void AgentProposal_IsAppliedOnceAndClampedByStats()
        {
            var agent = new CrewAgent("crew", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var reply = new AgentReply { Speech = "好", AffinityChange = 4f, Action = "none" };
            Assert.IsTrue(CrewRelationSystem.ApplyAgentProposal(agent, reply));
            Assert.AreEqual(4f, agent.Stats.Affinity, 0.0001f);
            Assert.IsTrue(CrewRelationSystem.ApplyAgentProposal(agent, reply));
            Assert.AreEqual(8f, agent.Stats.Affinity, 0.0001f);
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
