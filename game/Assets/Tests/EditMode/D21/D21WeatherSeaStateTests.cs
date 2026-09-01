// D21WeatherSeaStateTests —— D21-L21 天气、海况与风险资格纯逻辑测试

using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21WeatherSeaStateTests
    {
        [Test]
        public void SameSeedAndWeather_ProducesIdenticalSeaStateImpactAndRiskQualification()
        {
            D21WeatherInput input = Weather(6.5f, 12f, 0.72f, 1002f, 777L);
            D21SeaStateResult first = D21SeaStateSystem.EvaluateDeterministic(input);
            D21SeaStateResult second = D21SeaStateSystem.EvaluateDeterministic(input.Clone());

            Assert.That(first.Success, Is.True);
            Assert.That(second.Success, Is.True);
            Assert.That(first.InputFingerprint, Is.EqualTo(second.InputFingerprint));
            Assert.That(first.Level, Is.EqualTo(second.Level));
            Assert.That(first.SeverityIndex, Is.EqualTo(second.SeverityIndex));
            Assert.That(first.Risk, Is.EqualTo(second.Risk));
            Assert.That(first.EventProbability, Is.EqualTo(second.EventProbability));
            Assert.That(first.DeterministicEventSeed, Is.EqualTo(second.DeterministicEventSeed));

            D21WeatherImpactResult impactA = D21WeatherImpactRules.Evaluate(input, first);
            D21WeatherImpactResult impactB = D21WeatherImpactRules.Evaluate(input.Clone(), second);
            Assert.That(impactA.Success, Is.True);
            Assert.That(impactB.Success, Is.True);
            Assert.That(impactA.SpeedMultiplier, Is.EqualTo(impactB.SpeedMultiplier));
            Assert.That(impactA.ProvisionConsumptionMultiplier, Is.EqualTo(impactB.ProvisionConsumptionMultiplier));
            Assert.That(impactA.PortAvailable, Is.EqualTo(impactB.PortAvailable));

            D21VoyageRiskInput riskInput = new D21VoyageRiskInput
            {
                VoyageId = "voyage:l21:seed",
                ShipId = "ship_l21_01",
                RouteId = "route:l21:main",
                LocationRef = "sea:l21:cell",
                VoyageSeed = 777L,
                TickContext = D21VoyageTimeSystem.CreateTickContext(
                    8, 360, 0, 4, 777L, "voyage:l21:seed", "ship_l21_01"),
                StormActive = true,
                StormRiskPermille = 500,
                SeaStateRiskPermille = 250,
                VisibilityPermille = 720,
                ShipConditionPermille = 900,
                CrewHealthPermille = 850,
                InputFactIds = new List<string> { "fact:l21:weather", "fact:l21:sea" },
            };
            D21VoyageRiskResult riskA = D21VoyageRiskSystem.Evaluate(riskInput);
            D21VoyageRiskResult riskB = D21VoyageRiskSystem.Evaluate(riskInput.Clone());
            Assert.That(riskA.Accepted, Is.True);
            Assert.That(riskB.Accepted, Is.True);
            Assert.That(riskA.Qualifications, Has.Count.EqualTo(riskB.Qualifications.Count));
            Assert.That(riskA.Qualifications[1].Seed.Seed, Is.EqualTo(riskB.Qualifications[1].Seed.Seed));
            Assert.That(riskA.Qualifications[1].RollPermille, Is.EqualTo(riskB.Qualifications[1].RollPermille));
            Assert.That(D21VoyageRiskSystem.Qualify(riskInput, D21VoyageRiskType.Storm).Eligible, Is.True);
        }

        [Test]
        public void WeatherBoundaries_ValidateEdgesAndRejectInvalidCalmWind()
        {
            D21WeatherInput calm = Weather(0f, 0f, 1f, 1013.25f, 0L);
            string failure;
            Assert.That(calm.TryValidate(out failure), Is.True);
            Assert.That(failure, Is.Empty);
            Assert.That(D21WeatherInputContracts.ClassifyPrecipitation(0f),
                Is.EqualTo(D21PrecipitationBand.None));
            Assert.That(D21WeatherInputContracts.ClassifyPrecipitation(2.5f),
                Is.EqualTo(D21PrecipitationBand.Moderate));
            Assert.That(D21WeatherInputContracts.ClassifyPrecipitation(10f),
                Is.EqualTo(D21PrecipitationBand.Heavy));
            Assert.That(D21WeatherInputContracts.ClassifyPrecipitation(50f),
                Is.EqualTo(D21PrecipitationBand.Torrential));

            D21WeatherInput maxWind = Weather(D21WeatherInputContracts.MaxWindForce,
                D21WeatherInputContracts.MaxPrecipitationMmPerHour,
                0f, D21WeatherInputContracts.MinPressureHpa, 1L);
            maxWind.WindDirection = D21WindDirection.North;
            Assert.That(maxWind.TryValidate(out failure), Is.True);
            D21SeaStateResult extreme = D21SeaStateSystem.EvaluateDeterministic(maxWind);
            Assert.That(extreme.Success, Is.True);
            Assert.That(extreme.Level, Is.EqualTo(D21SeaStateLevel.Extreme));
            Assert.That(extreme.IsNavigable, Is.False);

            D21WeatherInput calmWithWind = calm.Clone();
            calmWithWind.WindForce = 0.1f;
            Assert.That(calmWithWind.TryValidate(out failure), Is.False);
            Assert.That(failure, Is.EqualTo("d21.weather.calm_direction_with_wind"));

            D21WeatherInput overWind = maxWind.Clone();
            overWind.WindForce = D21WeatherInputContracts.MaxWindForce + 0.01f;
            Assert.That(overWind.TryValidate(out failure), Is.False);
            Assert.That(failure, Is.EqualTo("d21.weather.wind_force_out_of_range"));
        }

        [Test]
        public void WeatherImpact_RejectsSeaStateFromDifferentInputFingerprint()
        {
            D21WeatherInput original = Weather(3f, 4f, 0.8f, 1008f, 44L);
            D21SeaStateResult seaState = D21SeaStateSystem.EvaluateDeterministic(original);
            D21WeatherInput changed = original.Clone();
            changed.WindForce = 3.5f;

            D21WeatherImpactResult result = D21WeatherImpactRules.Evaluate(changed, seaState);
            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureCode, Is.EqualTo(D21WeatherImpactFailureCodes.InputMismatch));
        }

        private static D21WeatherInput Weather(
            float windForce,
            float precipitation,
            float visibility,
            float pressure,
            long seed)
        {
            return new D21WeatherInput
            {
                EventId = "event:l21:weather",
                SourceId = "d9:environment",
                LocationRef = "sea:l21",
                WorldDay = 8,
                SaveRevision = 4,
                Seed = seed,
                Season = D21WeatherSeason.Wet,
                WindDirection = windForce == 0f ? D21WindDirection.Calm : D21WindDirection.West,
                WindForce = windForce,
                PrecipitationMmPerHour = precipitation,
                VisibilityFraction = visibility,
                PressureHpa = pressure,
            };
        }
    }
}
