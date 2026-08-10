// D14-A01 海战裁决验收：运动学、炮位/弹种、命中分区、火灾进水、撤退投降与读档幂等。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D14;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14NavalCombatTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_combat_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = _dbPath + suffix;
                if (File.Exists(path)) try { File.Delete(path); } catch { }
            }
        }

        [Test]
        public void KinematicsDamageFireFloodingAndSaveRestoreAreDeterministic()
        {
            var log = CreateLog("weather_source");
            var system = new D14NavalCombatSystem(log);
            var weather = D9WeatherSeaSnapshot.CreateDefault();
            weather.LocationRef = "combat_test_sea";
            weather.LastEventId = "weather_source";
            var started = system.StartEncounter(new D14CombatStartRequest
            {
                OperationId = "start", EncounterId = "encounter_test", PlayerShip = Profile("player_ship", 20f, 12), EnemyShip = Profile("enemy_ship", 4f, 0),
                Kinematics = new D14CombatKinematicsState { Distance = 150f, RelativeBearingDegrees = 90f, PlayerHeadingDegrees = 0f, EnemyHeadingDegrees = 180f, PlayerSpeed = 20f, EnemySpeed = 4f },
                Weather = weather, WeatherSourceEventId = "weather_source", WorldDay = 10, SaveRevision = 1, CauseEventIds = new List<string> { "weather_source" },
            });
            Assert.IsTrue(started.Accepted, started.Message);
            Assert.AreEqual(D14NavalCombatStatus.Engaged, system.Snapshot.Status);

            int hits = 0;
            for (int i = 0; i < 12; i++)
            {
                string gunId = "gun_" + i;
                var loaded = system.LoadGun(new D14CombatLoadGunRequest
                {
                    OperationId = "load_" + i, GunId = gunId, Ammo = D14NavalAmmoType.RoundShot,
                    WorldDay = 10, SaveRevision = 2 + i * 3, CauseEventIds = new List<string> { system.Snapshot.LastEventId },
                });
                Assert.IsTrue(loaded.Accepted, loaded.Message);
                var aimed = system.Aim(new D14CombatAimRequest
                {
                    OperationId = "aim_" + i, GunId = gunId, BearingDegrees = 90f, TargetZone = D14TargetZone.Hull,
                    WorldDay = 10, SaveRevision = 3 + i * 3, CauseEventIds = new List<string> { loaded.EventId },
                });
                Assert.IsTrue(aimed.Accepted, aimed.Message);
                var fired = system.Fire(new D14CombatFireRequest
                {
                    OperationId = "fire_" + i, GunId = gunId, WorldDay = 10, SaveRevision = 4 + i * 3,
                    CauseEventIds = new List<string> { aimed.EventId },
                });
                Assert.IsTrue(fired.Accepted, fired.Message);
                if (fired.Hit) hits++;
            }

            Assert.Greater(hits, 0);
            Assert.Less(system.Snapshot.EnemyDamage.CurrentHull, system.Snapshot.EnemyDamage.MaxHull);
            Assert.IsTrue(system.Snapshot.EnemyDamage.FireLevel > 0 || system.Snapshot.EnemyDamage.FloodingLevel > 0,
                "连续命中应能留下火灾或进水状态。");
            Assert.AreEqual(D14GunPosition.PortMidships, system.Snapshot.PlayerGuns[0].Position);
            Assert.AreEqual(D14NavalAmmoType.None, system.Snapshot.PlayerGuns[0].LoadedAmmo);
            Assert.Greater(system.Snapshot.PlayerGuns[0].ReloadRemainingTurns, 0);
            Assert.AreEqual(90f, system.Snapshot.Kinematics.RelativeBearingDegrees);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 海战测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14Runtime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD14Runtime();
            Assert.IsNotNull(restored);
            Assert.AreEqual(saved.State.EnemyDamage.CurrentHull, restored.State.EnemyDamage.CurrentHull);
            Assert.AreEqual(saved.State.EnemyDamage.FireLevel, restored.State.EnemyDamage.FireLevel);
            Assert.AreEqual(saved.State.EnemyDamage.FloodingLevel, restored.State.EnemyDamage.FloodingLevel);
            Assert.AreEqual(saved.CombatEventIds.Count, restored.CombatEventIds.Count);
            Assert.IsNotNull(restored.EventLog);
            Assert.AreEqual(saved.EventLog.Facts.Count, restored.EventLog.Facts.Count);

            var restoredSystem = new D14NavalCombatSystem(null, restored);
            var duplicate = restoredSystem.Fire(new D14CombatFireRequest
            {
                OperationId = "fire_11", GunId = "gun_11", WorldDay = 10, SaveRevision = 37,
            });
            Assert.IsTrue(duplicate.Accepted && duplicate.Duplicate);
            Assert.AreEqual(saved.EventLog.Facts.Count, restoredSystem.SharedEventLog.Count);
        }

        [Test]
        public void AmmoKindsAndRetreatOrSurrenderProduceTraceableOutcomes()
        {
            var log = CreateLog("weather_source");
            var weather = D9WeatherSeaSnapshot.CreateDefault();
            var system = new D14NavalCombatSystem(log);
            var started = system.StartEncounter(new D14CombatStartRequest
            {
                OperationId = "start_ammo", EncounterId = "encounter_ammo", PlayerShip = Profile("player", 8f, 3), EnemyShip = Profile("enemy", 5f, 0),
                Kinematics = new D14CombatKinematicsState { Distance = 260f, RelativeBearingDegrees = 45f, PlayerHeadingDegrees = 0f, EnemyHeadingDegrees = 180f, PlayerSpeed = 20f, EnemySpeed = 0f },
                Weather = weather, WeatherSourceEventId = "weather_source", WorldDay = 1, SaveRevision = 1, CauseEventIds = new List<string> { "weather_source" },
            });
            Assert.IsTrue(started.Accepted, started.Message);
            var chain = system.LoadGun(new D14CombatLoadGunRequest { OperationId = "load_chain", GunId = "gun_0", Ammo = D14NavalAmmoType.ChainShot, WorldDay = 1, SaveRevision = 2, CauseEventIds = new List<string> { started.EventId } });
            Assert.IsTrue(chain.Accepted, chain.Message);
            var chainAim = system.Aim(new D14CombatAimRequest { OperationId = "aim_chain", GunId = "gun_0", BearingDegrees = 45f, TargetZone = D14TargetZone.Auto, WorldDay = 1, SaveRevision = 3, CauseEventIds = new List<string> { chain.EventId } });
            Assert.IsTrue(chainAim.Accepted, chainAim.Message);
            var chainFire = system.Fire(new D14CombatFireRequest { OperationId = "fire_chain", GunId = "gun_0", WorldDay = 1, SaveRevision = 4, CauseEventIds = new List<string> { chainAim.EventId } });
            Assert.IsTrue(chainFire.Accepted, chainFire.Message);
            if (chainFire.Hit) Assert.AreEqual(D14HitKind.Sail, chainFire.HitKind);

            var retreat = system.AttemptRetreat(new D14CombatRetreatRequest { OperationId = "retreat", WorldDay = 1, SaveRevision = 5, CauseEventIds = new List<string> { chainFire.EventId } });
            Assert.IsTrue(retreat.Accepted, retreat.Message);
            Assert.AreEqual(D14NavalCombatStatus.Escaping, retreat.Status);
            Assert.IsNotNull(retreat.Fact);

            var surrenderSystem = new D14NavalCombatSystem(log);
            var surrenderStart = surrenderSystem.StartEncounter(new D14CombatStartRequest
            {
                OperationId = "start_surrender", EncounterId = "encounter_surrender", PlayerShip = Profile("player_surrender", 8f, 1), EnemyShip = Profile("enemy_surrender", 5f, 0),
                Kinematics = new D14CombatKinematicsState { Distance = 80f, RelativeBearingDegrees = 0f, PlayerHeadingDegrees = 0f, EnemyHeadingDegrees = 180f, PlayerSpeed = 8f, EnemySpeed = 5f },
                Weather = weather, WeatherSourceEventId = "weather_source", WorldDay = 1, SaveRevision = 10, CauseEventIds = new List<string> { "weather_source" },
            });
            Assert.IsTrue(surrenderStart.Accepted, surrenderStart.Message);
            var surrendered = surrenderSystem.Surrender(new D14CombatSurrenderRequest { OperationId = "surrender", WorldDay = 1, SaveRevision = 11, CauseEventIds = new List<string> { surrenderStart.EventId } });
            Assert.IsTrue(surrendered.Accepted, surrendered.Message);
            Assert.AreEqual(D14NavalCombatStatus.Surrendered, surrendered.Status);
        }

        private static D14CombatShipProfile Profile(string id, float speed, int gunCount)
        {
            var result = new D14CombatShipProfile
            {
                ShipId = id, ShipTypeId = "test_sloop", HullMax = 500, Crew = 50, Speed = speed,
                Heading = 0f, GunnerSkill = 95, ManeuverSkill = 90, ConditionPercent = 100,
            };
            for (int i = 0; i < gunCount; i++)
                result.Guns.Add(new D14GunState
                {
                    GunId = "gun_" + i, Position = D14GunPosition.PortMidships, RoundShot = 5, ChainShot = 5, GrapeShot = 5,
                });
            return result;
        }

        private static EventLog CreateLog(params string[] ids)
        {
            var log = new EventLog();
            foreach (var id in ids)
                log.Append(new WorldEvent(id, 0, "test_source", "EnvironmentSystem",
                    new EventResult { ResultCode = "source_ready", Summary = "测试来源事实。", FactIds = new List<string> { "source:" + id } },
                    new[] { "world" }, new[] { "test:d14_a01" }));
            return log;
        }
    }
}
