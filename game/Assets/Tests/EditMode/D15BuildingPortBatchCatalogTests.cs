using NUnit.Framework;
using Whalebone.Content.D15;

namespace Whalebone.Tests.EditMode
{
    [TestFixture]
    public sealed class D15BuildingPortBatchCatalogTests
    {
        [Test]
        public void BatchHasRequiredCountsAndUniqueReferences()
        {
            var data = D15BuildingPortBatchCatalog.Create();
            Assert.AreEqual(80, data.Definitions.Count);
            Assert.AreEqual(180, data.Variants.Count);
            Assert.That(data.Instances.Count, Is.InRange(600, 1200));
            data.Validate();
        }

        [Test]
        public void EveryInstanceHasOwnerStateBindingsInteractionAudioAndSaveFields()
        {
            var data = D15BuildingPortBatchCatalog.Create();
            foreach (var instance in data.Instances)
            {
                Assert.That(instance.InstanceId, Does.StartWith("instance_"));
                Assert.That(instance.LocationRef, Does.StartWith("island_"));
                Assert.IsNotEmpty(instance.OwnerRef);
                Assert.IsNotEmpty(instance.InventoryProfileId);
                Assert.IsNotEmpty(instance.DamageProfileId);
                Assert.IsNotEmpty(instance.OpenRuleId);
                Assert.That(instance.InteractionPoints.Count, Is.GreaterThanOrEqualTo(3));
                Assert.IsNotEmpty(instance.AudioProfile);
                Assert.IsNotEmpty(instance.SaveStateKey);
                Assert.That(instance.AssetState, Is.EqualTo("placeholder"));
            }
        }

        [Test]
        public void SensitiveBuildingsCarryWarningAndSkipPresentation()
        {
            var data = D15BuildingPortBatchCatalog.Create();
            var warningCount = 0;
            foreach (var definition in data.Definitions)
            {
                if (definition.HistoricalReview == "ApprovedWithWarning")
                {
                    warningCount++;
                    Assert.That(definition.ContentWarningIds, Is.Not.Empty);
                    Assert.IsTrue(definition.SkipAllowed);
                    Assert.That(definition.PresentationLevels, Does.Contain("Omitted"));
                }
            }
            Assert.That(warningCount, Is.GreaterThanOrEqualTo(10));
        }
    }
}
