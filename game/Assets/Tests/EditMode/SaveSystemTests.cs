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
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
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
        public void D3Runtime_SaveLoad_RestoresTradeVoyageAndMarketState()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D3 船长" });
            var snapshot = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.GreenCayId,
                DestinationId = PortCatalog.DriftwoodIsleId,
                VoyageStatus = D3VoyageStatus.InPort,
                Weather = D3WeatherType.Rain,
                CurrentDay = 3,
                TotalDays = 4,
                VoyageSeed = 987,
                Water = 11,
                Food = 12,
                Hull = 55,
                HullMax = 60,
                Money = 432,
            };
            snapshot.Cargo["sugar"] = 7;
            snapshot.PersonalItems["compass"] = 1;
            snapshot.MarketStock["sugar"] = 43;
            snapshot.GatheredThisVisit["timber"] = 2;
            snapshot.EventIds.Add("rain_catch");

            SaveSystem.SaveD3Runtime(snapshot);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD3Runtime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(PortCatalog.GreenCayId, loaded.CurrentLocationId);
            Assert.AreEqual(PortCatalog.DriftwoodIsleId, loaded.DestinationId);
            Assert.AreEqual(D3WeatherType.Rain, loaded.Weather);
            Assert.AreEqual(7, loaded.Cargo["sugar"]);
            Assert.AreEqual(43, loaded.MarketStock["sugar"]);
            Assert.AreEqual("rain_catch", loaded.EventIds[0]);
        }

        [Test]
        public void D9FishingState_SaveLoad_RestoresFishCargoAndSchool()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D9 捕捞船长" });
            var d3 = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Money = 518,
            };
            d3.Cargo["item_fresh_mackerel"] = 2;
            d3.MarketStock["item_salted_mackerel"] = 48;
            var d4 = new D4RuntimeSnapshot();
            d4.Ecology.FishSchools.Add(new FishSchoolState
            {
                FishSchoolId = "fish_mackerel_school",
                FishingSpotId = "nearshore_saint_marys",
                WorldX = -4.25f,
                WorldZ = 8.75f,
                Biomass = 10,
                FishingPressure = 2,
                LastMovedDay = 1,
            });

            SaveSystem.SaveD3Runtime(d3);
            SaveSystem.SaveD4Runtime(d4);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());

            var loadedD3 = SaveSystem.LoadD3Runtime();
            var loadedD4 = SaveSystem.LoadD4Runtime();
            Assert.AreEqual(2, loadedD3.Cargo["item_fresh_mackerel"]);
            Assert.AreEqual(48, loadedD3.MarketStock["item_salted_mackerel"]);
            Assert.AreEqual(1, loadedD4.Ecology.FishSchools.Count);
            Assert.AreEqual("fish_mackerel_school", loadedD4.Ecology.FishSchools[0].FishSchoolId);
            Assert.AreEqual(10, loadedD4.Ecology.FishSchools[0].Biomass);
            Assert.AreEqual(2, loadedD4.Ecology.FishSchools[0].FishingPressure);
        }

        [Test]
        public void D4Runtime_SaveLoad_RestoresContentState()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D4 船长" });
            var snapshot = new D4RuntimeSnapshot();
            snapshot.Factions.Reputation["england"] = 42;
            snapshot.Factions.PrivateerPermits.Add("england");
            snapshot.Shipyard.Upgrades["Cannon"] = 2;
            snapshot.Combat.Status = D4CombatStatus.Victory;
            snapshot.Combat.EnemyHull = 0;
            snapshot.Ecology.HabitatId = "jungle";
            snapshot.Health.MedicalSupplies = 7;
            snapshot.Mainline.Flags["learn_combat"] = true;
            snapshot.Mainline.CompletedMissions.Add("ch2_cannon");
            snapshot.RouteLog.Add(new RouteLogEntry { Id = "log_0001", Day = 4, Kind = D4LogKind.Combat, Text = "炮战胜利" });

            SaveSystem.SaveD4Runtime(snapshot);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD4Runtime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(42, loaded.Factions.Reputation["england"]);
            Assert.IsTrue(loaded.Factions.PrivateerPermits.Contains("england"));
            Assert.AreEqual(2, loaded.Shipyard.Upgrades["Cannon"]);
            Assert.AreEqual(D4CombatStatus.Victory, loaded.Combat.Status);
            Assert.AreEqual(7, loaded.Health.MedicalSupplies);
            Assert.IsTrue(loaded.Mainline.Flags["learn_combat"]);
            Assert.AreEqual("炮战胜利", loaded.RouteLog[0].Text);
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

        [Test]
        public void D8A05_EmptyArchive_ReturnsEmptyWithoutCreatingFile()
        {
            var result = SaveSystem.MigrateToV3();

            Assert.AreEqual(SaveMigrationStatus.Empty, result.Status);
            Assert.IsTrue(result.OriginalPreserved);
            Assert.IsFalse(File.Exists(_dbPath), "空档迁移不得创建数据库文件");
        }

        [Test]
        public void D8A05_D7Archive_MigratesCoreEntitiesAndMetadata()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D7 船长" });
            string playerId = GameState.CurrentPlayer.Id;
            string shipId = GameState.CurrentShip.Id;

            var result = SaveSystem.MigrateToV3();

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            Assert.AreEqual(1, result.SourceUserVersion);
            Assert.AreEqual(3, result.SaveVersion);
            using (var db = new SqliteDb(_dbPath))
            {
                var metadata = db.Query("SELECT * FROM save_metadata WHERE id=1")[0];
                Assert.AreEqual(3, SaveJson.GetInt(metadata, "save_version"));
                Assert.AreEqual(0, SaveJson.GetInt(metadata, "world_revision"));
                Assert.AreEqual("", SaveJson.GetStr(metadata, "last_event_id", ""));
                Assert.IsNotEmpty(SaveJson.GetStr(metadata, "migration_flags", "[]"));
                Assert.AreEqual(1, SaveJson.GetInt(metadata, "source_user_version"));
                Assert.AreEqual(3, Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM v3_entities")[0]["c"]));

                var oldPlayer = db.Query("SELECT money, ship_id FROM players WHERE id=?", playerId)[0];
                Assert.AreEqual(500, SaveJson.GetInt(oldPlayer, "money"));
                Assert.AreEqual(shipId, SaveJson.GetStr(oldPlayer, "ship_id"));
                var player = ReadV3Entity(db, "Player", playerId);
                Assert.AreEqual(1, SaveJson.GetInt(player, "schema_version"));
                Assert.IsTrue(player.ContainsKey("legacy_d7"));
                Assert.IsTrue(player.ContainsKey("balance"));
            }
        }

        [Test]
        public void D8A05_SailingArchive_PreservesLocationAndEvent()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "航行中船长" });
            var d3 = new D3RuntimeSnapshot();
            d3.EventIds.Add("d7_event_0007");
            SaveSystem.SaveD3Runtime(d3);
            SaveSystem.SaveD2Runtime(
                new SailingSnapshot
                {
                    ShipId = GameState.CurrentShip.Id,
                    PositionX = 32.5f,
                    PositionZ = -4.25f,
                    Heading = 180f,
                    Speed = 2f,
                    MaxSpeed = 4f,
                    ControlMode = D2ControlMode.Character,
                },
                new DayNightSnapshot(9, 0.35f));

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            Assert.AreEqual("d7_event_0007", result.LastEventId);
            using (var db = new SqliteDb(_dbPath))
            {
                var player = ReadV3Entity(db, "Player", GameState.CurrentPlayer.Id);
                var legacy = (Dictionary<string, object>)player["legacy_d7"];
                var location = (Dictionary<string, object>)legacy["location"];
                Assert.AreEqual("open_sea", SaveJson.GetStr(location, "type"));
                Assert.AreEqual(9, SaveJson.GetInt(location, "day_index"));
                Assert.IsNotEmpty(SaveJson.GetStr(legacy, "d3_runtime_json", ""));
            }
        }

        [Test]
        public void D8A05_DamagedShip_PreservesHullAndAddsIntegrity()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "损伤船长" });
            GameState.CurrentShip.Hull = 42;
            SaveSystem.Save();

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            using (var db = new SqliteDb(_dbPath))
            {
                var oldShip = db.Query("SELECT hull FROM ships WHERE id=?", GameState.CurrentShip.Id)[0];
                Assert.AreEqual(42, SaveJson.GetInt(oldShip, "hull"));
                var ship = ReadV3Entity(db, "Ship", GameState.CurrentShip.Id);
                var legacy = (Dictionary<string, object>)ship["legacy_d7"];
                Assert.AreEqual(42, SaveJson.GetInt(legacy, "hull"));
                var integrity = (Dictionary<string, object>)ship["hull_integrity"];
                Assert.AreEqual(70, SaveJson.GetInt(integrity, "value"));
            }
        }

        [Test]
        public void D8A05_DebtArchive_UsesSafeFinanceDefaults()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "负债船长" });
            GameState.CurrentPlayer.Money = -250;
            SaveSystem.Save();

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            Assert.IsTrue(result.MigrationFlags.Contains("finance_account_defaulted"));
            using (var db = new SqliteDb(_dbPath))
            {
                var player = ReadV3Entity(db, "Player", GameState.CurrentPlayer.Id);
                Assert.AreEqual(-250, SaveJson.GetInt((Dictionary<string, object>)player["balance"], "value"));
                Assert.AreEqual(0, SaveJson.GetInt((Dictionary<string, object>)player["credit"], "value"));
            }
        }

        [Test]
        public void D8A05_InProgressQuest_LeavesLegacyRuntimeAndDefaultsQuest()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "任务中船长" });
            var d4 = new D4RuntimeSnapshot();
            d4.Mainline.Chapter = 2;
            d4.Mainline.MissionIndex = 1;
            d4.Mainline.Flags["mission_in_progress"] = true;
            SaveSystem.SaveD4Runtime(d4);

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            Assert.IsTrue(result.MigrationFlags.Contains("quest_defaulted"));
            using (var db = new SqliteDb(_dbPath))
            {
                var player = ReadV3Entity(db, "Player", GameState.CurrentPlayer.Id);
                Assert.IsNotEmpty(SaveJson.GetStr((Dictionary<string, object>)player["legacy_d7"], "d4_runtime_json", ""));
                Assert.AreEqual(0, ((List<object>)player["quest_ids"]).Count);
            }
        }

        [Test]
        public void D8A05_RepeatedMigration_IsNoOp()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "幂等船长" });
            var first = SaveMigration.Migrate(_dbPath);
            Assert.AreEqual(SaveMigrationStatus.Migrated, first.Status);
            string playerId = GameState.CurrentPlayer.Id;
            string beforePayload;
            long beforeCount;
            using (var db = new SqliteDb(_dbPath))
            {
                beforePayload = SaveJson.Stringify(ReadV3Entity(db, "Player", playerId));
                beforeCount = Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM v3_entities")[0]["c"]);
            }

            var second = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.AlreadyMigrated, second.Status);
            using (var db = new SqliteDb(_dbPath))
            {
                Assert.AreEqual(beforeCount, Convert.ToInt64(db.Query("SELECT COUNT(*) AS c FROM v3_entities")[0]["c"]));
                Assert.AreEqual(beforePayload, SaveJson.Stringify(ReadV3Entity(db, "Player", playerId)));
            }
        }

        [Test]
        public void D8A05_FailedMigration_RollsBackAndPreservesD7()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "中断船长" });
            GameState.CurrentShip.Hull = 42;
            SaveSystem.Save();
            string playerId = GameState.CurrentPlayer.Id;
            string shipId = GameState.CurrentShip.Id;
            var result = SaveMigration.Migrate(_dbPath, new SaveMigrationOptions { FaultPoint = SaveMigrationFaultPoint.BeforeCommit });
            Assert.AreEqual(SaveMigrationStatus.Failed, result.Status);
            Assert.AreEqual("migration_interrupted", result.ErrorCode);
            Assert.IsTrue(result.OriginalPreserved);
            using (var db = new SqliteDb(_dbPath))
                Assert.AreEqual(0, db.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='save_metadata'").Count);
            using (var db = new SqliteDb(_dbPath))
            {
                Assert.AreEqual(500, SaveJson.GetInt(db.Query("SELECT money FROM players WHERE id=?", playerId)[0], "money"));
                Assert.AreEqual(shipId, SaveJson.GetStr(db.Query("SELECT ship_id FROM players WHERE id=?", playerId)[0], "ship_id"));
                Assert.AreEqual(42, SaveJson.GetInt(db.Query("SELECT hull FROM ships WHERE id=?", shipId)[0], "hull"));
            }
            Assert.IsTrue(SaveSystem.Load(), "回滚后原始 D7 存档仍应能加载");
        }

        [Test]
        public void D8A05_UnknownVersion_IsRejectedWithoutWrites()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "未知版本船长" });
            using (var db = new SqliteDb(_dbPath)) db.Execute("PRAGMA user_version=99");

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.UnsupportedVersion, result.Status);
            Assert.AreEqual("unsupported_version", result.ErrorCode);
            using (var db = new SqliteDb(_dbPath))
            {
                Assert.AreEqual(0, db.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='save_metadata'").Count);
                Assert.AreEqual(500, SaveJson.GetInt(db.Query("SELECT money FROM players LIMIT 1")[0], "money"));
            }
            Assert.IsTrue(SaveSystem.Load(), "未知版本拒绝迁移不应破坏 D7 读取");
        }

        [Test]
        public void D8A05_PartialCorruption_DefaultsNewFieldsWithoutDroppingD7()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "部分损坏船长" });
            string playerId = GameState.CurrentPlayer.Id;
            using (var db = new SqliteDb(_dbPath)) db.Execute("UPDATE players SET inventory_json=? WHERE id=?", "{broken", playerId);

            var result = SaveMigration.Migrate(_dbPath);

            Assert.AreEqual(SaveMigrationStatus.Migrated, result.Status);
            Assert.IsTrue(result.MigrationFlags.Contains("partial_recovery:players.inventory_json"));
            using (var db = new SqliteDb(_dbPath))
            {
                var oldRow = db.Query("SELECT inventory_json FROM players WHERE id=?", playerId)[0];
                Assert.AreEqual("{broken", SaveJson.GetStr(oldRow, "inventory_json"));
                var player = ReadV3Entity(db, "Player", playerId);
                var legacy = (Dictionary<string, object>)player["legacy_d7"];
                Assert.AreEqual(0, ((Dictionary<string, object>)legacy["inventory"]).Count);
            }
        }

        private static Dictionary<string, object> ReadV3Entity(SqliteDb db, string entityType, string entityId)
        {
            var rows = db.Query("SELECT payload_json FROM v3_entities WHERE entity_type=? AND entity_id=?", entityType, entityId);
            Assert.AreEqual(1, rows.Count, "v3 实体应存在：" + entityType + "/" + entityId);
            return SaveJson.ParseDict(SaveJson.GetStr(rows[0], "payload_json", "{}"));
        }
    }
}
