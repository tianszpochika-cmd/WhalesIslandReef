// ============================================================
// SaveSystemTests —— 存档系统单元测试（Unity Test Framework, Edit Mode）
//
// 覆盖（DEV_ROADMAP D1-P1B DoD）：
//   1. NewGame → HasSave()==true
//   2. Save → Load 字段还原一致（player/ship/crew）
//   3. 种子数据条数正确（trait_defs 20 + ship_type_defs 2，且分类 8/8/4）
//   4. 种子幂等（重复 EnsureSchemaAndSeed 不产生脏数据）
//
// 隔离：每用例使用独立临时 .db 路径（SaveSystem.DbPath 覆盖），
//       不触碰真实 persistentDataPath 存档。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D2;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public class SaveSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_save_test_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath; // internal，经 InternalsVisibleTo
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string f = _dbPath + suffix;
                if (File.Exists(f))
                {
                    try { File.Delete(f); } catch { /* 清理失败不阻塞测试 */ }
                }
            }
        }

        [Test]
        public void HasSave_WithoutNewGame_IsFalse()
        {
            Assert.IsFalse(SaveSystem.HasSave());
        }

        [Test]
        public void NewGame_CreatesPersistentSave()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "杰克船长" });
            Assert.IsTrue(SaveSystem.HasSave(), "NewGame 落库后 HasSave 应为 true");
            Assert.IsTrue(File.Exists(_dbPath), "数据库文件应存在");
        }

        [Test]
        public void NewGame_GeneratesExpectedInitialState()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "杰克船长" });

            // ---- Player ----
            var player = GameState.CurrentPlayer;
            Assert.IsNotNull(player, "应生成 Player");
            Assert.AreEqual("杰克船长", player.Name);
            Assert.AreEqual(PlayerState.StartingMoney, player.Money);   // 500
            Assert.AreEqual(PlayerState.StartingPortId, player.Location.PortId); // 圣玛格丽特港
            Assert.AreEqual("port", player.Location.Type);
            Assert.AreEqual(Enum.GetValues(typeof(SkillType)).Length, player.Skills.Count, "8 项技能均应有默认值");
            foreach (var kv in player.Skills)
                Assert.AreEqual(1, kv.Value, "技能默认 1");
            Assert.GreaterOrEqual(player.Traits.Count, 3);
            Assert.LessOrEqual(player.Traits.Count, 4);

            // ---- Ship ----
            var ship = GameState.CurrentShip;
            Assert.IsNotNull(ship);
            Assert.AreEqual("small_fishing", ship.ShipTypeId, "默认船型 small_fishing");
            Assert.AreEqual(ship.HullMax, ship.Hull, "初始满耐久");
            Assert.AreEqual(60, ship.HullMax);
            Assert.AreEqual(1, ship.CrewIds.Count, "初始船员 1 人");
            Assert.Greater(ship.Supplies["water"], 0);
            Assert.Greater(ship.Supplies["food"], 0);

            // ---- CrewAgent 老水手 ----
            Assert.AreEqual(1, GameState.CrewAgents.Count);
            var sailor = GameState.CrewAgents[0];
            Assert.AreEqual("老水手", sailor.Persona.Name);
            Assert.AreEqual("eng", CrewAgentCodec.CultureEng);
            var traitIds = GameState.CrewAgentTraitIds[sailor.Id];
            Assert.Contains("trait_old_sailor", traitIds, "老水手必须含老水手经历特质");
            Assert.GreaterOrEqual(traitIds.Count, 3);
            Assert.LessOrEqual(traitIds.Count, 4);
        }

        [Test]
        public void Save_Then_Load_RestoresAllFields()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "杰克船长" });

            // 修改若干字段后保存（模拟运行中的状态）
            GameState.CurrentPlayer.Money = 777;
            GameState.CurrentPlayer.PlaySeconds = 123;
            GameState.CurrentPlayer.Skills[SkillType.Trading] = 3;
            GameState.CurrentShip.Hull = 42;
            GameState.CurrentShip.Cargo["salt"] = 15;
            GameState.CurrentShip.Supplies["water"] = 9;
            GameState.CurrentShip.ShipLog.Add(new ShipLogEntry { Time = "1675-01-01T08:00:00", Kind = "auto", Text = "离港" });
            GameState.CrewAgents[0].Stats.Morale = 88f;

            SaveSystem.Save();

            // 清空内存，模拟下次启动
            GameState.Reset();
            Assert.IsNull(GameState.CurrentPlayer, "Reset 后应无内存状态");

            Assert.IsTrue(SaveSystem.Load(), "Load 应成功返回 true");
            Assert.IsNotNull(GameState.CurrentPlayer);

            // ---- Player 还原 ----
            Assert.AreEqual("杰克船长", GameState.CurrentPlayer.Name);
            Assert.AreEqual(777, GameState.CurrentPlayer.Money);
            Assert.AreEqual(123, GameState.CurrentPlayer.PlaySeconds);
            Assert.AreEqual(3, GameState.CurrentPlayer.Skills[SkillType.Trading]);
            Assert.AreEqual(1, GameState.CurrentPlayer.Skills[SkillType.Sailing], "未修改技能保持默认");
            Assert.AreEqual(PlayerState.StartingPortId, GameState.CurrentPlayer.Location.PortId);

            // ---- Ship 还原 ----
            Assert.AreEqual(42, GameState.CurrentShip.Hull);
            Assert.AreEqual(60, GameState.CurrentShip.HullMax, "hullMax 应从 ship_type_defs 回查");
            Assert.AreEqual(15, GameState.CurrentShip.Cargo["salt"]);
            Assert.AreEqual(9, GameState.CurrentShip.Supplies["water"]);
            Assert.AreEqual(1, GameState.CurrentShip.ShipLog.Count);
            Assert.AreEqual("离港", GameState.CurrentShip.ShipLog[0].Text);
            Assert.AreEqual("auto", GameState.CurrentShip.ShipLog[0].Kind);
            Assert.AreEqual(1, GameState.CurrentShip.CrewIds.Count);

            // ---- CrewAgent 还原 ----
            Assert.AreEqual(1, GameState.CrewAgents.Count);
            var sailor = GameState.CrewAgents[0];
            Assert.AreEqual("老水手", sailor.Persona.Name);
            Assert.AreEqual(88f, sailor.Stats.Morale, 0.01f, "士气数值应还原");
            Assert.AreEqual(1, GameState.CrewAgents[0].Stats.Skills[SkillType.Sailing]);
            var traitIds = GameState.CrewAgentTraitIds[sailor.Id];
            Assert.Contains("trait_old_sailor", traitIds);
        }

        [Test]
        public void D2Runtime_SaveLoad_RestoresLocationClockAndAffinity()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D2 船长" });
            var sailor = GameState.CrewAgents[0];
            var proposal = new AgentReply { Speech = "记住这片海。", AffinityChange = 5f, Action = "none" };
            Assert.IsTrue(CrewRelationSystem.ApplyAgentProposal(sailor, proposal));

            SaveSystem.SaveD2Runtime(
                new SailingSnapshot
                {
                    ShipId = GameState.CurrentShip.Id,
                    PositionX = 12.5f,
                    PositionZ = -8.25f,
                    Heading = 135f,
                    Speed = 2f,
                    MaxSpeed = 4f,
                    ControlMode = D2ControlMode.Character,
                },
                new DayNightSnapshot(4, 0.72f));

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            Assert.AreEqual("open_sea", GameState.CurrentPlayer.Location.Type);
            Assert.AreEqual(12.5f, GameState.CurrentPlayer.Location.X, 0.0001f);
            Assert.AreEqual(-8.25f, GameState.CurrentPlayer.Location.Y, 0.0001f);
            Assert.AreEqual(135f, GameState.CurrentPlayer.Location.Heading, 0.0001f);
            Assert.AreEqual(4, GameState.CurrentPlayer.Location.DayIndex);
            Assert.AreEqual(0.72f, GameState.CurrentPlayer.Location.NormalizedTime, 0.0001f);
            Assert.AreEqual(5f, GameState.CrewAgents[0].Stats.Affinity, 0.0001f);
        }

        [Test]
        public void SeedData_Counts_AreCorrect()
        {
            SaveSystem.EnsureSchemaAndSeed(); // internal
            using (var db = new SqliteDb(_dbPath))
            {
                long traits = Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM trait_defs")[0]["c"]);
                long ships = Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM ship_type_defs")[0]["c"]);
                Assert.AreEqual(20, traits, "trait_defs 应 20 条");
                Assert.AreEqual(2, ships, "ship_type_defs 应 2 条");
            }
        }

        [Test]
        public void SeedData_Category_Distribution_Is_8_8_4()
        {
            SaveSystem.EnsureSchemaAndSeed();
            using (var db = new SqliteDb(_dbPath))
            {
                var rows = db.Query("SELECT category, COUNT(*) AS c FROM trait_defs GROUP BY category");
                var map = new Dictionary<string, long>();
                foreach (var r in rows)
                    map[SaveJson.GetStr(r, "category", "")] = Convert.ToInt64(r["c"]);
                Assert.AreEqual(8, map["Physical"], "身体 8");
                Assert.AreEqual(8, map["Personality"], "性格 8");
                Assert.AreEqual(4, map["Experience"], "经历 4");
            }
        }

        [Test]
        public void SeedData_Is_Idempotent()
        {
            SaveSystem.EnsureSchemaAndSeed();
            SaveSystem.EnsureSchemaAndSeed(); // 重复执行不应产生脏数据
            using (var db = new SqliteDb(_dbPath))
            {
                long traits = Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM trait_defs")[0]["c"]);
                long ships = Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM ship_type_defs")[0]["c"]);
                Assert.AreEqual(20, traits);
                Assert.AreEqual(2, ships);
            }
        }
    }
}
