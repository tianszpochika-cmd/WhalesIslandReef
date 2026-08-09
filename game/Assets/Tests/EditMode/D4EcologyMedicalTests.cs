using NUnit.Framework;
using Whalebone.Systems.D4;

namespace D4.Tests
{
    public class D4EcologyMedicalTests
    {
        [Test]
        public void Ecology_SeedsObservesAndCollectsRareAnimals()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("jungle", 1, 3);
            Assert.GreaterOrEqual(ecology.Snapshot.Animals.Count, 4);
            var observed = ecology.Observe("golden_parrot");
            Assert.IsTrue(observed.Accepted);
            var collected = ecology.Collect("golden_parrot");
            Assert.IsTrue(collected.Accepted);
            Assert.Greater(collected.CollectionValue, 0);
        }

        [Test]
        public void Ecology_DangerCanProduceDamageAndDisease()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("mangrove", 1, 1);
            var danger = ecology.ResolveDanger("mosquito_swarm", 0, 1);
            Assert.IsTrue(danger.Accepted);
            Assert.IsTrue(danger.Dangerous);
        }

        [Test]
        public void Medical_InfectsDamagesAndTreatsCrew()
        {
            var medical = new D4SurvivalSystem();
            medical.CreateInitial(new[] { "crew_1" }, 5);
            Assert.IsTrue(medical.Infect("crew_1", "scurvy").Accepted);
            var before = medical.Snapshot.Crew[0].Health;
            medical.TickDay(false, false, false, 0);
            Assert.Less(medical.Snapshot.Crew[0].Health, before);
            var treated = medical.Treat("crew_1", "scurvy", 2);
            Assert.IsTrue(treated.Accepted);
            Assert.IsFalse(medical.Snapshot.Crew[0].Diseases.Contains("scurvy"));
        }
    }
}
