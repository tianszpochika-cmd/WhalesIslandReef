using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D11.Tests
{
    public sealed class D11EcologyStateMachineTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d11_ecology_" + Guid.NewGuid().ToString("N") + ".db");
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
                if (!File.Exists(file)) continue;
                try { File.Delete(file); } catch { }
            }
        }

        [Test]
        public void WeatherTideCurrentTemperatureTurbidityAndPollutionAreDeterministic()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("mangrove", 0, 11);
            var calm = new EnvironmentSystem().Snapshot;
            var stormEnvironment = new EnvironmentSystem();
            stormEnvironment.ApplyStormEvent(2, "nearshore_saint_marys", 3);
            var storm = stormEnvironment.Snapshot;

            var calmConditions = ecology.ReadConditions(calm, 0);
            var stormConditions = ecology.ReadConditions(storm, 2);
            Assert.Greater(stormConditions.CurrentSpeed, calmConditions.CurrentSpeed);
            Assert.Greater(stormConditions.Turbidity, calmConditions.Turbidity);
            Assert.Less(stormConditions.FishCatchMultiplier, calmConditions.FishCatchMultiplier);
            Assert.AreNotEqual(calmConditions.TidePhase, stormConditions.TidePhase);

            var log = new EventLog();
            var pollution = ecology.ApplyPollution("harbor_runoff", 0.5f, 2, log, 7,
                "event_d11_pollution_sample");
            Assert.IsTrue(pollution.Succeeded, pollution.Message);
            var polluted = ecology.ReadConditions(calm, 2);
            Assert.Greater(polluted.Pollution, calmConditions.Pollution);
            Assert.Greater(polluted.Turbidity, calmConditions.Turbidity);
            Assert.AreEqual("EcologySystem", log.Find("event_d11_pollution_sample", 7).OwnerSystem);
        }

        [Test]
        public void FishingPressureReducesPreviewAndDeterministicRecoveryRestoresResources()
        {
            var fresh = new D4EcologySystem();
            fresh.SeedFishSchool("fish_mackerel_school", "nearshore_saint_marys", 0, 17);
            var baseline = fresh.PreviewFishCatch("fish_mackerel_school", "nearshore_saint_marys",
                "item_fresh_mackerel", 3, 0, 17, D9WeatherSeaSnapshot.CreateDefault());

            var pressured = new D4EcologySystem();
            pressured.SeedFishSchool("fish_mackerel_school", "nearshore_saint_marys", 0, 17);
            var first = pressured.PreviewFishCatch("fish_mackerel_school", "nearshore_saint_marys",
                "item_fresh_mackerel", 3, 0, 17, D9WeatherSeaSnapshot.CreateDefault());
            Assert.IsTrue(first.Accepted);
            Assert.IsTrue(pressured.CommitFishCatch(first));
            var pressuredPreview = pressured.PreviewFishCatch("fish_mackerel_school", "nearshore_saint_marys",
                "item_fresh_mackerel", 3, 0, 17, D9WeatherSeaSnapshot.CreateDefault());
            Assert.Less(pressuredPreview.PressureMultiplier, baseline.PressureMultiplier);
            Assert.Greater(pressured.Snapshot.FishSchools[0].FishingPressure, 0);

            int biomassAfterCatch = pressured.Snapshot.FishSchools[0].Biomass;
            var advance = pressured.AdvanceEcology(20, D9WeatherSeaSnapshot.CreateDefault(), 17,
                new EventLog(), 8, "event_d11_ecology_advance_sample");
            var recovered = pressured.Snapshot.FishSchools[0];
            Assert.IsTrue(advance.Succeeded, advance.Message);
            Assert.GreaterOrEqual(recovered.Biomass, biomassAfterCatch);
            Assert.Less(recovered.FishingPressure, first.FishingPressureAfter);
            Assert.AreEqual(20, recovered.LastMovedDay);
        }

        [Test]
        public void PlantHarvestPollutionRecoveryAndEventTraceAreIdempotent()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("mangrove", 1, 5);
            var before = ecology.FindPlantSnapshot("plant_red_mangrove");
            Assert.IsNotNull(before);
            Assert.Greater(before.Quantity, 0);
            var log = new EventLog();

            var harvest = ecology.HarvestPlant("plant_red_mangrove", 2, 1, 5, log, 10,
                "event_d11_plant_harvest_sample");
            var afterHarvest = ecology.FindPlantSnapshot("plant_red_mangrove");
            var duplicateHarvest = ecology.HarvestPlant("plant_red_mangrove", 2, 1, 5, log, 10,
                "event_d11_plant_harvest_sample");
            Assert.IsTrue(harvest.Succeeded, harvest.Message);
            Assert.IsTrue(duplicateHarvest.Duplicate);
            Assert.AreEqual(afterHarvest.Quantity, ecology.FindPlantSnapshot("plant_red_mangrove").Quantity);

            var polluted = ecology.ApplyPollution("dock", 0.6f, 2, log, 11,
                "event_d11_pollution_trace_sample");
            Assert.IsTrue(polluted.Succeeded, polluted.Message);
            float pollutionBeforeRestore = ecology.FindHabitatConditionSnapshot().Pollution;
            var restored = ecology.RestoreHabitat(3, D9WeatherSeaSnapshot.CreateDefault(), 5, log, 12,
                "event_d11_ecology_restore_sample");
            Assert.IsTrue(restored.Succeeded, restored.Message);
            Assert.Less(ecology.FindHabitatConditionSnapshot().Pollution, pollutionBeforeRestore);
            Assert.IsNotNull(log.Find("event_d11_ecology_restore_sample", 12));
            Assert.AreEqual(3, log.Count);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "生态存档测试" });
            SaveSystem.SaveD4Runtime(new D4RuntimeSnapshot { Ecology = ecology.Snapshot });
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var saved = SaveSystem.LoadD4Runtime();
            Assert.IsNotNull(saved);
            Assert.AreEqual(ecology.Snapshot.Habitat.Pollution, saved.Ecology.Habitat.Pollution, 0.0001f);
            var resumed = new D4EcologySystem(saved.Ecology);
            var duplicateAfterLoad = resumed.ApplyPollution("dock", 0.6f, 2, null, 11,
                "event_d11_pollution_trace_sample");
            Assert.IsTrue(duplicateAfterLoad.Duplicate);
        }
    }
}
