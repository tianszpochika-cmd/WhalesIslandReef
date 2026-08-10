using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D9.Tests
{
    public class D9WeatherSeaStateTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d9_weather_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string file = _dbPath + suffix;
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        [Test]
        public void CalmRainStorm_ChangeSeaShipAndFishEffects()
        {
            var environment = new EnvironmentSystem();
            var calm = environment.Snapshot;
            var rain = environment.SetWeather(D9WeatherState.Rain, 1, "nearshore_saint_marys").Snapshot;
            var storm = environment.ApplyStormEvent(2, "nearshore_saint_marys", 2).Snapshot;

            var calmShip = D9ShipWeatherReader.Read(calm);
            var rainShip = D9ShipWeatherReader.Read(rain);
            var stormShip = D9ShipWeatherReader.Read(storm);
            var calmFish = D9EcologyWeatherReader.ResolveCatch(calm, 10);
            var rainFish = D9EcologyWeatherReader.ResolveCatch(rain, 10);
            var stormFish = D9EcologyWeatherReader.ResolveCatch(storm, 10);

            Assert.AreEqual(D9WeatherState.Calm, calm.Weather);
            Assert.AreEqual(D9WeatherState.Rain, rain.Weather);
            Assert.AreEqual(D9WeatherState.Storm, storm.Weather);
            Assert.Less(calm.Visibility, 1.0f);
            Assert.Less(storm.Visibility, rain.Visibility);
            Assert.Greater(storm.WindSpeed, rain.WindSpeed);
            Assert.Greater(rain.WindSpeed, calm.WindSpeed);
            Assert.Less(calmShip.CrewFatiguePerDay, rainShip.CrewFatiguePerDay);
            Assert.Less(rainShip.CrewFatiguePerDay, stormShip.CrewFatiguePerDay);
            Assert.Less(calmShip.HullRisk, stormShip.HullRisk);
            Assert.Less(calmFish.Quantity, 11);
            Assert.Less(rainFish.Quantity, calmFish.Quantity);
            Assert.Less(stormFish.Quantity, rainFish.Quantity);
            Assert.IsTrue(stormShip.Hazardous);
        }

        [Test]
        public void EnvironmentSystem_StormEventIsWrittenOnce()
        {
            var log = new EventLog();
            var environment = new EnvironmentSystem();
            var first = environment.ApplyStormEvent(4, "nearshore_saint_marys", 11, log);
            var second = environment.ApplyStormEvent(4, "nearshore_saint_marys", 11, log);

            Assert.IsTrue(first.Succeeded);
            Assert.IsTrue(first.Applied);
            Assert.IsFalse(first.IsDuplicate);
            Assert.IsTrue(second.Succeeded);
            Assert.IsFalse(second.Applied);
            Assert.IsTrue(second.IsDuplicate);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(EnvironmentSystem.StormEventId, log.Facts[0].EventId);
            Assert.AreEqual("EnvironmentSystem", log.Facts[0].OwnerSystem);
            Assert.AreEqual(1, environment.Snapshot.AppliedEventIds.Count);
        }

        [Test]
        public void WeatherSnapshot_SavesRestoresAndBlocksRepeatedImpact()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "天气测试船长" });
            var environment = new EnvironmentSystem();
            var log = new EventLog();
            environment.ApplyStormEvent(6, "nearshore_saint_marys", 17, log);
            var runtime = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                DestinationId = PortCatalog.GreenCayId,
                VoyageStatus = D3VoyageStatus.Sailing,
                CurrentDay = 6,
                SaveRevision = 17,
            };
            environment.PublishTo(runtime);
            SaveSystem.SaveD3Runtime(runtime);

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            Assert.IsNotNull(GameState.CurrentD3Runtime);
            Assert.IsNotNull(GameState.CurrentD3Runtime.WeatherSea);
            var restored = GameState.CurrentD3Runtime.WeatherSea;
            Assert.AreEqual(D9WeatherState.Storm, restored.Weather);
            Assert.AreEqual(6, restored.WorldDay);
            Assert.AreEqual(EnvironmentSystem.StormEventId, restored.LastEventId);
            Assert.AreEqual(1, restored.AppliedEventIds.Count);
            Assert.AreEqual(D3WeatherType.Storm, GameState.CurrentD3Runtime.Weather);
            Assert.AreEqual(16f, restored.WindSpeed, 0.0001f);
            Assert.AreEqual(0.25f, restored.Visibility, 0.0001f);
            Assert.AreEqual(0.80f, restored.CurrentSpeed, 0.0001f);
            Assert.AreEqual(6, restored.CrewFatiguePerDay);
            Assert.AreEqual(0.45f, restored.HullRisk, 0.0001f);
            Assert.AreEqual(0.55f, restored.FishActivityMultiplier, 0.0001f);
            Assert.AreEqual(0.50f, restored.FishCatchMultiplier, 0.0001f);

            var resumedEnvironment = new EnvironmentSystem();
            resumedEnvironment.Restore(restored);
            var exposureBeforeDuplicate = D9ShipWeatherReader.ResolveExposure(restored, 1);
            var duplicate = resumedEnvironment.ApplyStormEvent(6, "nearshore_saint_marys", 17, new EventLog());
            Assert.IsTrue(duplicate.IsDuplicate);
            Assert.AreEqual(restored.HullRisk, resumedEnvironment.Snapshot.HullRisk, 0.0001f);
            var exposureAfterDuplicate = D9ShipWeatherReader.ResolveExposure(resumedEnvironment.Snapshot, 1);
            Assert.AreEqual(exposureBeforeDuplicate.CrewFatigueDelta, exposureAfterDuplicate.CrewFatigueDelta);
            Assert.AreEqual(exposureBeforeDuplicate.HullRisk, exposureAfterDuplicate.HullRisk, 0.0001f);
            Assert.AreEqual(exposureBeforeDuplicate.SpeedMultiplier, exposureAfterDuplicate.SpeedMultiplier, 0.0001f);
        }

        [Test]
        public void LegacyD3WeatherWithoutD9Snapshot_RestoresSafeCompatibilityState()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "旧天气船长" });
            SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                Weather = D3WeatherType.Rain,
                VoyageStatus = D3VoyageStatus.Sailing,
                CurrentDay = 3,
            });

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            Assert.IsNull(GameState.CurrentD3Runtime.WeatherSea);
            var environment = new EnvironmentSystem();
            environment.RestoreLegacy(GameState.CurrentD3Runtime.Weather, 3, "nearshore_saint_marys");

            Assert.AreEqual(D9WeatherState.Rain, environment.Snapshot.Weather);
            Assert.AreEqual(3, environment.Snapshot.WorldDay);
            Assert.AreEqual(0, environment.Snapshot.AppliedEventIds.Count);
        }

        [Test]
        public void ReadersDoNotMutateEnvironmentSnapshot()
        {
            var environment = new EnvironmentSystem();
            environment.ApplyStormEvent(2, "nearshore_saint_marys", 1);
            var before = environment.Snapshot;

            D9ShipWeatherReader.ResolveExposure(before, 2);
            D9EcologyWeatherReader.ResolveCatch(before, 12);

            var after = environment.Snapshot;
            Assert.AreEqual(before.Revision, after.Revision);
            Assert.AreEqual(before.Weather, after.Weather);
            Assert.AreEqual(before.AppliedEventIds.Count, after.AppliedEventIds.Count);
            Assert.AreEqual(before.HullRisk, after.HullRisk, 0.0001f);
        }

        [Test]
        public void UnknownWeatherIsRejectedWithoutMutatingSnapshot()
        {
            var environment = new EnvironmentSystem();
            var before = environment.Snapshot;
            var result = environment.SetWeather((D9WeatherState)999, 1, "nearshore_saint_marys");

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("unknown_weather", result.Code);
            Assert.AreEqual(before.Weather, environment.Snapshot.Weather);
            Assert.AreEqual(before.Revision, environment.Snapshot.Revision);
            Assert.AreEqual(0, environment.Snapshot.AppliedEventIds.Count);
        }

        [Test]
        public void CorruptImpactSnapshotIsRejectedBeforeRestore()
        {
            var corrupt = D9WeatherSeaSnapshot.CreateDefault();
            corrupt.Visibility = 2f;

            Assert.Throws<InvalidOperationException>(() => new EnvironmentSystem(corrupt));
        }
    }
}
