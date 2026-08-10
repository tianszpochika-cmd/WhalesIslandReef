using NUnit.Framework;
using Whalebone.Agent;

namespace D8.Tests
{
    public sealed class D8A04AgentReplyMockTests
    {
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
    }
}
