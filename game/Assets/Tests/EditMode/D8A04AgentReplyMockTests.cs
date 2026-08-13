using NUnit.Framework;
using Whalebone.Agent;

namespace D8.Tests
{
    public sealed class D8A04AgentReplyMockTests
    {
        [Test]
        public void TemplateFallback_ProducesExplicitV11NoOpProposal()
        {
            var reply = TemplateFallback.Reply(
                new PersonaDefinition("老水手", "水手"),
                "报告海况");

            Assert.That(reply, Is.Not.Null);
            Assert.That(reply.IsValid, Is.True);
            Assert.That(reply.SchemaVersion, Is.EqualTo(AgentReply.CurrentSchemaVersion));
            Assert.That(reply.AffinityChange, Is.EqualTo(0f));
            Assert.That(reply.RelationProposal.Kind, Is.EqualTo(RelationKind.None));
            Assert.That(reply.ActionIntent.Name, Is.EqualTo("none"));
            Assert.That(reply.ActionIntent.ActorRef, Is.EqualTo("none"));
            Assert.That(reply.ActionIntent.TargetRef, Is.EqualTo("none"));
            Assert.That(reply.Action, Is.EqualTo("none"));
        }

        [Test]
        public void TemplateFallback_DoesNotMutateCrewStatsWhenDialogueProviderFails()
        {
            var agent = new CrewAgent("crew-fallback", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var session = new DialogueSession(agent, new FailingClient(), new MutinyMonitor(agent.Stats));

            var reply = session.Speak("报告海况");

            Assert.That(reply, Is.Not.Null);
            Assert.That(reply.ActionIntent.Name, Is.EqualTo("none"));
            Assert.That(reply.AffinityChange, Is.EqualTo(0f));
            Assert.That(agent.Stats.Affinity, Is.EqualTo(0f));
        }

        [Test]
        public void MockDialogue_ExecutesOnlyAfterV11Boundary()
        {
            var agent = new CrewAgent("crew-1", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            var session = new DialogueSession(agent, new MockLLMClient(AgentReplyTestFixtures.ValidNone), new MutinyMonitor(agent.Stats));
            var reply = session.Speak("报告海况");

            Assert.That(reply, Is.Not.Null);
            Assert.That(reply.SchemaVersion, Is.EqualTo("agent_reply.v1.1"));
            Assert.That(reply.ActionIntent.Name, Is.EqualTo("none"));
            Assert.That(reply.Action, Is.EqualTo("none"));
        }

        [Test]
        public void MockAction_RejectsTargetOutsideRuntimeAuthorization()
        {
            var context = new ActionValidationContext();
            context.AuthorizedActorRefs.Add("crew:deckhand-1");
            context.AuthorizedTargetRefs.Add("ship:other");
            context.Permissions.Add("ship.sail");
            context.SatisfiedPrerequisites.Add("ship.present");
            context.SatisfiedPrerequisites.Add("crew.on_duty");

            Assert.That(AgentReplyValidator.TryValidate(AgentReplyTestFixtures.ValidTrimSail, context, out _, out var error), Is.False);
            Assert.That(error, Is.EqualTo("unauthorized_target"));
        }

        private sealed class FailingClient : ILLMClient
        {
            public string Name => "OfflineFailingProvider";
            public bool Available => false;

            public System.Threading.Tasks.Task<string> CompleteAsync(string system, string user)
            {
                return System.Threading.Tasks.Task.FromException<string>(
                    new System.InvalidOperationException("offline"));
            }
        }
    }
}
