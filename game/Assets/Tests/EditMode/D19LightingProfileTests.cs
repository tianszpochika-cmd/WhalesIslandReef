using System.IO;
using NUnit.Framework;
using UnityEngine;
using Whalebone.Game.D19;

namespace Whalebone.Tests.EditMode
{
    public sealed class D19LightingProfileTests
    {
        private static string CatalogPath
        {
            get { return Path.Combine(Application.dataPath, "Settings/D19/Lighting/D19LightingProfileCatalog.json"); }
        }

        private static D19LightingProfileCatalog LoadCatalog()
        {
            Assert.That(File.Exists(CatalogPath), Is.True, "D19-A05 profile catalog is missing.");
            var catalog = JsonUtility.FromJson<D19LightingProfileCatalog>(File.ReadAllText(CatalogPath));
            Assert.That(catalog, Is.Not.Null);
            return catalog;
        }

        [Test]
        public void CatalogHasFiveProfilesAndFrozenSafetyBounds()
        {
            var catalog = LoadCatalog();
            var errors = D19LightingProfileValidator.Validate(catalog);
            Assert.That(errors, Is.Empty, string.Join(";", errors));
            Assert.That(catalog.profiles, Has.Length.EqualTo(5));
            Assert.That(catalog.mappings, Has.Length.GreaterThanOrEqualTo(5));
        }

        [Test]
        public void ReadOnlyWeatherAndTimeMapToAllRepresentativeProfiles()
        {
            var catalog = LoadCatalog();
            Assert.That(D19LightingProfileSelector.SelectProfile(new D19LightingReadOnlyState("Calm", 0.35f, 0.95f), catalog), Is.EqualTo("clear_day"));
            Assert.That(D19LightingProfileSelector.SelectProfile(new D19LightingReadOnlyState("Rain", 0.30f, 0.65f), catalog), Is.EqualTo("fog"));
            Assert.That(D19LightingProfileSelector.SelectProfile(new D19LightingReadOnlyState("Rain", 0.55f, 0.65f), catalog), Is.EqualTo("overcast_rain"));
            Assert.That(D19LightingProfileSelector.SelectProfile(new D19LightingReadOnlyState("Calm", 0.90f, 0.95f), catalog), Is.EqualTo("twilight_night"));
            Assert.That(D19LightingProfileSelector.SelectProfile(new D19LightingReadOnlyState("Storm", 0.35f, 0.25f), catalog), Is.EqualTo("storm"));
        }

        [Test]
        public void UnsafeProfileIsRejectedByValidator()
        {
            var catalog = LoadCatalog();
            var night = D19LightingProfileValidator.Find(catalog, "twilight_night");
            night.motion_blur = true;
            night.exposure_ev = 9f;
            var errors = D19LightingProfileValidator.Validate(catalog);
            Assert.That(errors, Does.Contain("unsafe_motion_effect:twilight_night"));
            Assert.That(errors, Does.Contain("exposure_out_of_range:twilight_night"));
        }

        [Test]
        public void IllegalMappingAndColorBoundsAreRejectedByValidator()
        {
            var catalog = LoadCatalog();
            catalog.mappings[0].time_start = -0.1f;
            catalog.profiles[0].sky_color.r = 2f;
            var errors = D19LightingProfileValidator.Validate(catalog);
            Assert.That(errors, Does.Contain("mapping_time_out_of_range:0"));
            Assert.That(errors, Does.Contain("color_out_of_range:sky:clear_day"));
        }

        [Test]
        public void TimeWindowSupportsDuskNightWrapAround()
        {
            Assert.That(D19LightingProfileSelector.TimeInWindow(0.02f, 0.75f, 0.20f), Is.True);
            Assert.That(D19LightingProfileSelector.TimeInWindow(0.88f, 0.75f, 0.20f), Is.True);
            Assert.That(D19LightingProfileSelector.TimeInWindow(0.50f, 0.75f, 0.20f), Is.False);
        }
    }
}
