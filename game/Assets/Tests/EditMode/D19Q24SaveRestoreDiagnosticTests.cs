#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Whalebone.Systems.D2;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    /// <summary>
    /// Q24 存档恢复诊断：验证陆地/船控位置的既有语义、港口安全出生锚点和
    /// 事务失败保护。游泳状态当前没有独立存档字段，测试明确记录这一设计边界。
    /// </summary>
    public sealed class D19Q24SaveRestoreDiagnosticTests
    {
        private string _dbPath;
        private GameObject _shipProbe;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_q24_save_test_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            SaveSystem.TestTransactionBoundaryHook = null;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.TestTransactionBoundaryHook = null;
            SaveSystem.DbPath = null;
            GameState.Reset();
            if (_shipProbe != null) UnityEngine.Object.DestroyImmediate(_shipProbe);
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void LandSave_RoundTripPreservesPortClockMoneyCargoAndCrew()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "Q24 陆地船长" });
            GameState.CurrentPlayer.Money = 725;
            GameState.CurrentShip.Cargo["salt"] = 8;
            GameState.CrewAgents[0].Stats.Morale = 91f;

            SaveSystem.SaveD2Runtime(null, new DayNightSnapshot(3, 1.35f));
            GameState.Reset();

            Assert.IsTrue(SaveSystem.Load());
            Assert.AreEqual("port", GameState.CurrentPlayer.Location.Type);
            Assert.AreEqual(PlayerState.StartingPortId, GameState.CurrentPlayer.Location.PortId);
            Assert.That(GameState.CurrentPlayer.Location.NormalizedTime, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.AreEqual(3, GameState.CurrentPlayer.Location.DayIndex);
            Assert.AreEqual(725, GameState.CurrentPlayer.Money);
            Assert.AreEqual(8, GameState.CurrentShip.Cargo["salt"]);
            Assert.AreEqual(91f, GameState.CrewAgents[0].Stats.Morale, 0.0001f);
        }

        [Test]
        public void OpenSeaSave_RoundTripPreservesShipLocationAndUsesHarborSpawnAnchorForPlayer()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "Q24 航行船长" });
            SaveSystem.SaveD2Runtime(
                new SailingSnapshot
                {
                    ShipId = GameState.CurrentShip.Id,
                    PositionX = 41.5f,
                    PositionZ = -17.25f,
                    Heading = 212f,
                    Speed = 2.25f,
                    MaxSpeed = 4f,
                    ControlMode = D2ControlMode.Ship,
                },
                new DayNightSnapshot(7, 0.62f));
            GameState.Reset();

            Assert.IsTrue(SaveSystem.Load());
            Assert.AreEqual("open_sea", GameState.CurrentPlayer.Location.Type);
            Assert.That(GameState.CurrentPlayer.Location.X, Is.EqualTo(41.5f).Within(0.0001f));
            Assert.That(GameState.CurrentPlayer.Location.Y, Is.EqualTo(-17.25f).Within(0.0001f));
            Assert.That(GameState.CurrentPlayer.Location.Heading, Is.EqualTo(212f).Within(0.0001f));
            Assert.That(GameState.CurrentPlayer.Location.Speed, Is.EqualTo(2.25f).Within(0.0001f));
            Assert.AreEqual(7, GameState.CurrentPlayer.Location.DayIndex);
            Assert.That(GameState.CurrentPlayer.Location.NormalizedTime, Is.EqualTo(0.62f).Within(0.0001f));

            _shipProbe = new GameObject("Q24_ShipSpawnProbe");
            _shipProbe.transform.position = new Vector3(10f, 0.6f, 20f);
            var anchor = new GameObject("D2_PlayerSpawnAnchor");
            anchor.transform.SetParent(_shipProbe.transform, false);
            anchor.transform.localPosition = new Vector3(1.25f, 1.1f, -2.5f);

            var expected = anchor.transform.position;
            var bootstrapType = Type.GetType("Whalebone.Gameplay.D2.D2RuntimeBootstrap, Assembly-CSharp");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未被 Unity 主程序集加载。");
            var resolveSpawn = bootstrapType.GetMethod(
                "ResolvePlayerSpawnPosition",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(resolveSpawn, "D2RuntimeBootstrap 缺少安全出生锚点解析方法。");
            var resolved = (Vector3)resolveSpawn.Invoke(null, new object[] { _shipProbe });
            Assert.That(resolved, Is.EqualTo(expected),
                "Open-sea save must restore the player to the authored harbor-safe spawn anchor, not into ocean coordinates.");
        }

        [Test]
        public void FailedSave_DoesNotOverwriteLastGoodPlayerShipOrCrewSnapshot()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "Q24 好档船长" });
            GameState.CurrentPlayer.Money = 812;
            GameState.CurrentShip.Hull = 47;
            GameState.CrewAgents[0].Stats.Morale = 86f;
            SaveSystem.Save();

            GameState.CurrentPlayer.Money = 9999;
            GameState.CurrentShip.Hull = 1;
            GameState.CrewAgents[0].Stats.Morale = 1f;
            SaveSystem.TestTransactionBoundaryHook = phase =>
            {
                if (phase == "after_player") throw new InvalidOperationException("Q24 simulated interrupted save");
            };

            Assert.Throws<InvalidOperationException>(() => SaveSystem.Save());
            SaveSystem.TestTransactionBoundaryHook = null;
            GameState.Reset();

            Assert.IsTrue(SaveSystem.Load());
            Assert.AreEqual(812, GameState.CurrentPlayer.Money);
            Assert.AreEqual(47, GameState.CurrentShip.Hull);
            Assert.AreEqual(86f, GameState.CrewAgents[0].Stats.Morale, 0.0001f);
            Assert.IsTrue(SaveSystem.HasSave());
        }
    }
}
#endif
