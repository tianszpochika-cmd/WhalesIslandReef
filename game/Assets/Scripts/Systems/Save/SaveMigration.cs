// ============================================================
// SaveMigration —— D7 SQLite 存档到 D8/v3 统一模型迁移
//
// 迁移只扩展存档数据库：D7 三张表保持原样，v3 投影写入独立表。
// 迁移是本地同步操作，不包含玩法裁决、网络或 LLM 依赖。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using Whalebone.Game.Config;

namespace Whalebone.Systems.Save
{
    public enum SaveMigrationStatus
    {
        Empty,
        Migrated,
        AlreadyMigrated,
        Failed,
        UnsupportedVersion,
    }

    public sealed class SaveMigrationResult
    {
        public SaveMigrationStatus Status { get; internal set; }
        public int SourceUserVersion { get; internal set; }
        public int SaveVersion { get; internal set; }
        public int WorldRevision { get; internal set; }
        public string LastEventId { get; internal set; }
        public List<string> MigrationFlags { get; internal set; }
        public string ErrorCode { get; internal set; }
        public string Message { get; internal set; }
        public string BackupPath { get; internal set; }
        public bool OriginalPreserved { get; internal set; }

        public bool Succeeded
        {
            get { return Status == SaveMigrationStatus.Empty || Status == SaveMigrationStatus.Migrated || Status == SaveMigrationStatus.AlreadyMigrated; }
        }

        internal static SaveMigrationResult Make(SaveMigrationStatus status, int sourceVersion, string message)
        {
            return new SaveMigrationResult
            {
                Status = status,
                SourceUserVersion = sourceVersion,
                SaveVersion = status == SaveMigrationStatus.Migrated || status == SaveMigrationStatus.AlreadyMigrated ? SaveMigration.TargetSaveVersion : 0,
                WorldRevision = 0,
                LastEventId = "",
                MigrationFlags = new List<string>(),
                Message = message,
                OriginalPreserved = true,
            };
        }
    }

    internal enum SaveMigrationFaultPoint
    {
        None,
        BeforeCommit,
        ProcessAbortBeforeCommit,
    }

    internal sealed class SaveMigrationOptions
    {
        public SaveMigrationFaultPoint FaultPoint = SaveMigrationFaultPoint.None;
    }

    public static class SaveMigration
    {
        public const int LegacyUserVersion = 1;
        public const int TargetSaveVersion = 3;

        private const string MetadataTable = "save_metadata";
        private const string EntityTable = "v3_entities";

        // 仅供 Save.Tests 的子进程故障注入使用；普通迁移永远保持 null。
        internal static Action TestProcessAbortHook { get; set; }

        public static SaveMigrationResult Migrate(string dbPath)
        {
            return MigrateCore(dbPath, null);
        }

        internal static SaveMigrationResult Migrate(string dbPath, SaveMigrationOptions options)
        {
            return MigrateCore(dbPath, options);
        }

        private static SaveMigrationResult MigrateCore(string dbPath, SaveMigrationOptions options)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                var missingPath = SaveMigrationResult.Make(SaveMigrationStatus.Failed, 0, "迁移失败：存档路径为空。");
                missingPath.ErrorCode = "path_required";
                missingPath.OriginalPreserved = true;
                return missingPath;
            }

            if (!File.Exists(dbPath) || new FileInfo(dbPath).Length == 0)
            {
                var empty = SaveMigrationResult.Make(SaveMigrationStatus.Empty, 0, "没有可迁移的存档，未创建新文件。");
                empty.OriginalPreserved = true;
                return empty;
            }

            int sourceVersion = 0;
            try
            {
                using (var db = new SqliteDb(dbPath))
                {
                    sourceVersion = ReadUserVersion(db);

                    if (TableExists(db, MetadataTable))
                        return ReadExistingMigration(db, sourceVersion);

                    // 只有 v3 实体表而没有元数据时，说明上次写入是不完整的。
                    if (TableExists(db, EntityTable))
                        return Failed(sourceVersion, "metadata_missing", "迁移失败：发现 v3 实体表但缺少 save_metadata，原始存档未改变。");

                    if (sourceVersion != LegacyUserVersion)
                        return Unsupported(sourceVersion, "不支持的存档版本：user_version=" + sourceVersion + "，未写入任何迁移数据。");

                    if (!HasD7Tables(db))
                    {
                        var noD7 = SaveMigrationResult.Make(SaveMigrationStatus.Empty, sourceVersion, "没有发现可迁移的 D7 存档表，未写入迁移数据。");
                        noD7.OriginalPreserved = true;
                        return noD7;
                    }

                    var playerRows = db.Query("SELECT * FROM players LIMIT 1");
                    if (playerRows.Count == 0)
                    {
                        var noPlayer = SaveMigrationResult.Make(SaveMigrationStatus.Empty, sourceVersion, "D7 存档为空，没有玩家行，未写入迁移数据。");
                        noPlayer.OriginalPreserved = true;
                        return noPlayer;
                    }

                    var snapshot = BuildSnapshot(db, playerRows[0], sourceVersion);
                    if (!string.IsNullOrEmpty(snapshot.ErrorCode))
                        return Failed(sourceVersion, snapshot.ErrorCode, snapshot.ErrorMessage);

                    bool transactionStarted = false;
                    try
                    {
                        db.Execute("BEGIN IMMEDIATE TRANSACTION");
                        transactionStarted = true;
                        CreateV3Tables(db);

                        foreach (var entity in snapshot.Entities)
                        {
                            db.Execute(
                                "INSERT INTO v3_entities(entity_type, entity_id, schema_version, save_version, payload_json) VALUES(?, ?, ?, ?, ?)",
                                entity.Type, entity.Id, 1, TargetSaveVersion, entity.Payload);
                        }

                        db.Execute(
                            "INSERT INTO save_metadata(id, save_version, world_revision, last_event_id, migration_flags, migration_status, source_user_version) VALUES(1, ?, ?, ?, ?, ?, ?)",
                            TargetSaveVersion,
                            snapshot.WorldRevision,
                            snapshot.LastEventId,
                            SaveJson.Stringify(snapshot.Flags),
                            "migrated",
                            sourceVersion);

                        if (options != null && options.FaultPoint == SaveMigrationFaultPoint.BeforeCommit)
                            throw new SaveMigrationException("测试注入：提交前中断迁移。");

                        if (options != null && options.FaultPoint == SaveMigrationFaultPoint.ProcessAbortBeforeCommit)
                        {
                            if (TestProcessAbortHook != null) TestProcessAbortHook();
                            throw new SaveMigrationException("测试注入：进程在提交前中止迁移。");
                        }

                        db.Execute("COMMIT");
                        transactionStarted = false;

                        var result = SaveMigrationResult.Make(SaveMigrationStatus.Migrated, sourceVersion, "D7 存档已迁移到 save_version=3。");
                        result.SaveVersion = TargetSaveVersion;
                        result.WorldRevision = snapshot.WorldRevision;
                        result.LastEventId = snapshot.LastEventId;
                        result.MigrationFlags = snapshot.Flags;
                        result.OriginalPreserved = true;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        if (transactionStarted)
                        {
                            try { db.Execute("ROLLBACK"); } catch { /* 原始错误更有助于恢复提示 */ }
                        }

                        var failed = Failed(sourceVersion,
                            ex is SaveMigrationException ? "migration_interrupted" : "migration_write_failed",
                            "迁移未完成，原始 D7 存档未覆盖。请保留原文件并重试；原因：" + ex.Message);
                        failed.OriginalPreserved = true;
                        return failed;
                    }
                }
            }
            catch (Exception ex)
            {
                var failed = Failed(sourceVersion, "migration_read_failed", "无法读取存档并完成迁移，原始文件未覆盖。请检查文件完整性后重试；原因：" + ex.Message);
                failed.OriginalPreserved = true;
                return failed;
            }
        }

        private static SaveMigrationResult ReadExistingMigration(SqliteDb db, int sourceVersion)
        {
            var rows = db.Query("SELECT * FROM save_metadata WHERE id=1");
            if (rows.Count == 0)
                return Failed(sourceVersion, "metadata_incomplete", "迁移状态不完整：save_metadata 没有主记录，原始存档未改变。");

            var row = rows[0];
            int saveVersion = SaveJson.GetInt(row, "save_version", 0);
            if (saveVersion != TargetSaveVersion)
                return Unsupported(saveVersion, "不支持的目标存档版本：save_version=" + saveVersion + "。");

            if (SaveJson.GetStr(row, "migration_status", "") != "migrated")
                return Failed(sourceVersion, "metadata_status_incomplete", "迁移状态不完整：save_metadata 未标记为 migrated，原始存档未改变。");

            if (!TableExists(db, EntityTable))
                return Failed(sourceVersion, "entity_table_missing", "迁移状态不完整：save_metadata 存在但 v3_entities 缺失。");

            long entityCount = Count(db, EntityTable);
            long playerCount = Count(db, "players");
            if (playerCount > 0 && entityCount == 0)
                return Failed(sourceVersion, "entity_rows_missing", "迁移状态不完整：v3_entities 没有实体行，原始存档未改变。");

            if (playerCount > 0)
            {
                var playerRows = db.Query("SELECT id, ship_id FROM players LIMIT 1");
                string playerId = playerRows.Count == 0 ? "" : SaveJson.GetStr(playerRows[0], "id", "");
                string shipId = playerRows.Count == 0 ? "" : SaveJson.GetStr(playerRows[0], "ship_id", "");
                if (!EntityExists(db, "Player", playerId) || !EntityExists(db, "Ship", shipId))
                    return Failed(sourceVersion, "entity_rows_missing", "迁移状态不完整：Player 或 Ship v3 实体缺失，原始存档未改变。");
                foreach (var crew in db.Query("SELECT id FROM crew_agents"))
                {
                    string crewId = SaveJson.GetStr(crew, "id", "");
                    if (!string.IsNullOrEmpty(crewId) && !EntityExists(db, "Crew", crewId))
                        return Failed(sourceVersion, "entity_rows_missing", "迁移状态不完整：Crew v3 实体缺失，原始存档未改变。");
                }
            }

            var result = SaveMigrationResult.Make(SaveMigrationStatus.AlreadyMigrated, sourceVersion, "存档已经是 save_version=3，未重复写入实体或世界状态。");
            result.SaveVersion = saveVersion;
            result.WorldRevision = SaveJson.GetInt(row, "world_revision", 0);
            result.LastEventId = SaveJson.GetStr(row, "last_event_id", "");
            result.MigrationFlags = ReadFlags(SaveJson.GetStr(row, "migration_flags", SaveJson.GetStr(row, "migration_flags_json", "[]")));
            result.OriginalPreserved = true;
            return result;
        }

        private static SaveMigrationResult Unsupported(int version, string message)
        {
            var result = SaveMigrationResult.Make(SaveMigrationStatus.UnsupportedVersion, version, message);
            result.ErrorCode = "unsupported_version";
            result.OriginalPreserved = true;
            return result;
        }

        private static SaveMigrationResult Failed(int sourceVersion, string code, string message)
        {
            var result = SaveMigrationResult.Make(SaveMigrationStatus.Failed, sourceVersion, message);
            result.ErrorCode = code;
            result.OriginalPreserved = true;
            return result;
        }

        private static int ReadUserVersion(SqliteDb db)
        {
            var rows = db.Query("PRAGMA user_version");
            return rows.Count == 0 ? 0 : SaveJson.GetInt(rows[0], "user_version", 0);
        }

        private static bool HasD7Tables(SqliteDb db)
        {
            return TableExists(db, "players") && TableExists(db, "ships") && TableExists(db, "crew_agents");
        }

        private static bool TableExists(SqliteDb db, string table)
        {
            return db.Query("SELECT name FROM sqlite_master WHERE type='table' AND name=?", table).Count > 0;
        }

        private static long Count(SqliteDb db, string table)
        {
            var rows = db.Query("SELECT COUNT(*) AS c FROM " + table);
            return rows.Count == 0 ? 0 : Convert.ToInt64(rows[0]["c"]);
        }

        private static bool EntityExists(SqliteDb db, string entityType, string entityId)
        {
            return !string.IsNullOrEmpty(entityId) && db.Query(
                "SELECT entity_id FROM v3_entities WHERE entity_type=? AND entity_id=?",
                entityType, entityId).Count > 0;
        }

        private static void CreateV3Tables(SqliteDb db)
        {
            db.Execute(@"
CREATE TABLE IF NOT EXISTS save_metadata(
    id INTEGER PRIMARY KEY CHECK(id=1),
    save_version INTEGER NOT NULL,
    world_revision INTEGER NOT NULL DEFAULT 0,
    last_event_id TEXT NOT NULL DEFAULT '',
    migration_flags TEXT NOT NULL DEFAULT '[]',
    migration_status TEXT NOT NULL,
    source_user_version INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS v3_entities(
    entity_type TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    schema_version INTEGER NOT NULL,
    save_version INTEGER NOT NULL,
    payload_json TEXT NOT NULL,
    PRIMARY KEY(entity_type, entity_id)
);");
        }

        private sealed class Snapshot
        {
            public readonly List<EntityRow> Entities = new List<EntityRow>();
            public readonly List<string> Flags = new List<string>();
            public int WorldRevision = 0;
            public string LastEventId = "";
            public string ErrorCode;
            public string ErrorMessage;
        }

        private sealed class EntityRow
        {
            public string Type;
            public string Id;
            public string Payload;
        }

        private sealed class SaveMigrationException : Exception
        {
            public SaveMigrationException(string message) : base(message) { }
        }

        private static Snapshot BuildSnapshot(SqliteDb db, Dictionary<string, object> playerRow, int sourceVersion)
        {
            var snapshot = new Snapshot();
            AddFlag(snapshot.Flags, "d7_source_user_version_" + sourceVersion);
            AddFlag(snapshot.Flags, "new_fields_defaulted");
            AddFlag(snapshot.Flags, "world_revision_defaulted");
            AddFlag(snapshot.Flags, "finance_account_defaulted");
            AddFlag(snapshot.Flags, "quest_defaulted");
            AddFlag(snapshot.Flags, "world_entities_defaulted");
            AddFlag(snapshot.Flags, "rewards_defaulted");
            AddFlag(snapshot.Flags, "appearance_loadout_defaulted");
            AddFlag(snapshot.Flags, "identity_expression_defaulted");

            string playerId = SaveJson.GetStr(playerRow, "id", "");
            string shipId = SaveJson.GetStr(playerRow, "ship_id", "");
            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(shipId))
            {
                snapshot.ErrorCode = "source_required_field_missing";
                snapshot.ErrorMessage = "迁移失败：D7 players.id 或 ship_id 缺失，未覆盖原始存档。";
                return snapshot;
            }

            var shipRows = db.Query("SELECT * FROM ships WHERE id=?", shipId);
            if (shipRows.Count == 0)
            {
                snapshot.ErrorCode = "source_ship_missing";
                snapshot.ErrorMessage = "迁移失败：D7 Player 指向的 Ship 不存在，未覆盖原始存档。";
                return snapshot;
            }

            var location = ReadDict(playerRow, "location_json", new Dictionary<string, object>(), "players.location_json", snapshot.Flags);
            int dayIndex = Math.Max(0, SaveJson.GetInt(location, "dayIndex", 0));
            string locationRef = SaveJson.GetStr(location, "portId", null);
            if (string.IsNullOrEmpty(locationRef)) locationRef = SaveJson.GetStr(location, "type", "unknown");
            snapshot.LastEventId = FindLastEventId(playerRow, location, snapshot.Flags);

            snapshot.Entities.Add(new EntityRow { Type = "Player", Id = playerId, Payload = SaveJson.Stringify(BuildPlayer(playerRow, location, playerId, shipId, locationRef, dayIndex, snapshot.Flags)) });

            var ship = shipRows[0];
            var crewIds = ReadList(ship, "crew_json", new List<object>(), "ships.crew_json", snapshot.Flags);
            var shipPayload = BuildShip(db, ship, playerId, locationRef, dayIndex, crewIds, snapshot.Flags);
            snapshot.Entities.Add(new EntityRow { Type = "Ship", Id = shipId, Payload = SaveJson.Stringify(shipPayload) });

            var crewRows = db.Query("SELECT * FROM crew_agents ORDER BY id");
            foreach (var crewRow in crewRows)
            {
                string crewId = SaveJson.GetStr(crewRow, "id", "");
                if (string.IsNullOrEmpty(crewId))
                {
                    AddFlag(snapshot.Flags, "partial_recovery:crew_agents.id");
                    continue;
                }
                snapshot.Entities.Add(new EntityRow { Type = "Crew", Id = crewId, Payload = SaveJson.Stringify(BuildCrew(crewRow, locationRef, dayIndex, snapshot.Flags)) });
            }

            return snapshot;
        }

        private static Dictionary<string, object> BuildPlayer(Dictionary<string, object> row, Dictionary<string, object> location, string playerId, string shipId, string locationRef, int dayIndex, List<string> flags)
        {
            int money = SaveJson.GetInt(row, "money", 0);
            string name = SaveJson.GetStr(row, "name", "");
            var skills = ReadDict(row, "skills_json", new Dictionary<string, object>(), "players.skills_json", flags);
            var traits = ReadList(row, "traits_json", new List<object>(), "players.traits_json", flags);
            var reputation = ReadDict(row, "reputation_json", new Dictionary<string, object>(), "players.reputation_json", flags);
            var inventory = ReadDict(row, "inventory_json", new Dictionary<string, object>(), "players.inventory_json", flags);
            string identityRaw = ReadRawSnapshot(location, "identity");
            var identity = string.IsNullOrEmpty(identityRaw) ? null : SaveJson.ParseDict(identityRaw);
            if (identity != null)
            {
                // D10 identity is an additive game-only projection. Keep the complete
                // object and expose only the D8-compatible projections at top level.
                flags.Remove("identity_expression_defaulted");
            }
            var legacy = new Dictionary<string, object>
            {
                ["id"] = playerId,
                ["name"] = name,
                ["money"] = money,
                ["skills"] = skills,
                ["traits"] = traits,
                ["reputation"] = reputation,
                ["inventory"] = inventory,
                ["ship_id"] = shipId,
                ["location"] = BuildLegacyLocation(location, flags),
                ["play_seconds"] = SaveJson.GetInt(row, "play_seconds", 0),
                ["d3_runtime_json"] = ReadRawSnapshot(location, "d3"),
                ["d4_runtime_json"] = ReadRawSnapshot(location, "d4"),
                ["d5_runtime_json"] = ReadRawSnapshot(location, "d5"),
                ["identity_json"] = identityRaw,
            };

            var payload = Base("Player", playerId, name, locationRef, dayIndex);
            payload["legacy_d7"] = legacy;
            payload["status"] = "unknown";
            payload["pronouns"] = identity != null && identity.TryGetValue("pronounKeys", out object pronouns)
                ? pronouns : new List<object>();
            payload["identity_history"] = identity == null
                ? new List<object>() : ReadIdentityHistoryIds(identity);
            payload["appearance_loadout_id"] = "";
            if (identity != null) payload["identity"] = identity;
            payload["balance"] = Value(money);
            payload["credit"] = Value(0);
            payload["ship_ids"] = new List<object> { shipId };
            payload["crew_ids"] = new List<object>();
            payload["relation_ids"] = new List<object>();
            payload["quest_ids"] = new List<object>();
            payload["collection_ids"] = new List<object>();
            return payload;
        }

        private static Dictionary<string, object> BuildShip(SqliteDb db, Dictionary<string, object> row, string playerId, string locationRef, int dayIndex, List<object> crewIds, List<string> flags)
        {
            string shipId = SaveJson.GetStr(row, "id", "");
            string shipTypeId = SaveJson.GetStr(row, "ship_type_id", "");
            string name = SaveJson.GetStr(row, "name", "");
            int hull = SaveJson.GetInt(row, "hull", 0);
            int hullMax = 0;
            var typeRows = db.Query("SELECT hull_max FROM ship_type_defs WHERE id=?", shipTypeId);
            if (typeRows.Count > 0) hullMax = SaveJson.GetInt(typeRows[0], "hull_max", 0);
            int integrity = hullMax > 0 ? Math.Max(0, Math.Min(100, (int)Math.Round((double)hull * 100d / hullMax))) : 0;
            if (hullMax <= 0) AddFlag(flags, "hull_integrity_defaulted");
            else AddFlag(flags, "hull_integrity_derived_from_d7_hull");
            AddFlag(flags, "ownership_ref_inferred_from_player_ship_id");

            var cargo = ReadDict(row, "cargo_json", new Dictionary<string, object>(), "ships.cargo_json", flags);
            var supplies = ReadDict(row, "supplies_json", new Dictionary<string, object>(), "ships.supplies_json", flags);
            var upgrades = ReadDict(row, "upgrades_json", new Dictionary<string, object>(), "ships.upgrades_json", flags);
            var state = ReadDict(row, "state_json", new Dictionary<string, object>(), "ships.state_json", flags);
            var shipLog = ReadList(row, "ship_log_json", new List<object>(), "ships.ship_log_json", flags);
            var legacy = new Dictionary<string, object>
            {
                ["id"] = shipId,
                ["ship_type_id"] = shipTypeId,
                ["name"] = name,
                ["hull"] = hull,
                ["cargo"] = cargo,
                ["cannons"] = SaveJson.GetInt(row, "cannons", 0),
                ["loaded_count"] = SaveJson.GetInt(row, "loaded_count", 0),
                ["supplies"] = supplies,
                ["crew_capacity"] = SaveJson.GetInt(row, "crew_capacity", 0),
                ["crew_ids"] = crewIds,
                ["upgrades"] = upgrades,
                ["state"] = state,
                ["ship_log"] = shipLog,
            };

            var payload = Base("Ship", shipId, name, locationRef, dayIndex);
            payload["legacy_d7"] = legacy;
            payload["ship_type_id"] = shipTypeId;
            payload["status"] = locationRef == "open_sea" ? "sailing" : "unknown";
            payload["hull_integrity"] = Value(integrity);
            payload["mass"] = Value(0);
            payload["cargo_capacity"] = Value(0);
            payload["ownership_ref"] = playerId;
            payload["crew_ids"] = crewIds;
            payload["cabin_ids"] = new List<object>();
            payload["function_point_ids"] = new List<object>();
            payload["anchor_state"] = "unknown";
            payload["repair_state"] = "unknown";
            return payload;
        }

        private static Dictionary<string, object> BuildCrew(Dictionary<string, object> row, string locationRef, int dayIndex, List<string> flags)
        {
            string id = SaveJson.GetStr(row, "id", "");
            string name = SaveJson.GetStr(row, "name", "");
            var traits = ReadList(row, "traits_json", new List<object>(), "crew_agents.traits_json", flags);
            var stats = ReadDict(row, "stats_json", new Dictionary<string, object>(), "crew_agents.stats_json", flags);
            int health = SaveJson.GetInt(stats, "health", (int)NumericConfiguration.Default.Health.DefaultValue);
            int fatigue = SaveJson.GetInt(stats, "fatigue", (int)NumericConfiguration.Default.Fatigue.DefaultValue);
            if (!stats.ContainsKey("health")) AddFlag(flags, "crew_health_defaulted:" + id);
            if (!stats.ContainsKey("fatigue")) AddFlag(flags, "crew_fatigue_defaulted:" + id);

            var legacy = new Dictionary<string, object>
            {
                ["id"] = id,
                ["name"] = name,
                ["role"] = SaveJson.GetStr(row, "role", ""),
                ["culture"] = SaveJson.GetStr(row, "culture", ""),
                ["traits_json"] = SaveJson.GetStr(row, "traits_json", "[]"),
                ["stats_json"] = SaveJson.GetStr(row, "stats_json", "{}"),
                ["persona_json"] = SaveJson.GetStr(row, "persona_json", "{}"),
                ["memory_json"] = SaveJson.GetStr(row, "memory_json", "{}"),
                ["relations_json"] = SaveJson.GetStr(row, "relations_json", "{}"),
            };

            var payload = Base("Crew", id, name, locationRef, dayIndex);
            payload["legacy_d7"] = legacy;
            payload["role_id"] = SaveJson.GetStr(row, "role", "");
            payload["culture_id"] = SaveJson.GetStr(row, "culture", "");
            payload["status"] = "unknown";
            payload["trait_ids"] = traits;
            payload["health"] = Value((int)NumericConfiguration.ClampHealth(health));
            payload["fatigue"] = Value((int)NumericConfiguration.ClampFatigue(fatigue));
            payload["relation_ids"] = new List<object>();
            payload["goal_ids"] = new List<object>();
            payload["commitment"] = "unknown";
            payload["debt"] = Value(0);
            payload["task_id"] = "";
            payload["shift_id"] = "";
            return payload;
        }

        private static Dictionary<string, object> Base(string type, string id, string displayName, string locationRef, int dayIndex)
        {
            return new Dictionary<string, object>
            {
                ["id"] = id,
                ["schema_version"] = 1,
                ["display_name_key"] = displayName ?? "",
                ["source_note"] = "migrated:d7",
                ["historical_tag"] = "d7",
                ["phase"] = "unknown",
                ["owner_system"] = "SaveMigration/D8-A05",
                ["test_id"] = "",
                ["created_day"] = Value(dayIndex),
                ["updated_day"] = Value(dayIndex),
                ["location_ref"] = locationRef ?? "unknown",
                ["state_flags"] = new Dictionary<string, object> { ["legacy_source"] = "d7" },
                ["revision"] = 0,
            };
        }

        private static Dictionary<string, object> Value(int value)
        {
            return new Dictionary<string, object> { ["value"] = value };
        }

        private static Dictionary<string, object> BuildLegacyLocation(Dictionary<string, object> location, List<string> flags)
        {
            if (location == null)
            {
                AddFlag(flags, "default:players.location_json");
                return new Dictionary<string, object>
                {
                    ["type"] = "port",
                    ["port_id"] = "",
                    ["x"] = 0,
                    ["y"] = 0,
                    ["heading"] = 0,
                    ["speed"] = 0,
                    ["day_index"] = 0,
                    ["normalized_time"] = 0,
                };
            }
            return new Dictionary<string, object>
            {
                ["type"] = SaveJson.GetStr(location, "type", "port"),
                ["port_id"] = SaveJson.GetStr(location, "port_id", SaveJson.GetStr(location, "portId", "")),
                ["x"] = SaveJson.GetFloat(location, "x", 0f),
                ["y"] = SaveJson.GetFloat(location, "y", 0f),
                ["heading"] = SaveJson.GetFloat(location, "heading", 0f),
                ["speed"] = SaveJson.GetFloat(location, "speed", 0f),
                ["day_index"] = Math.Max(0, SaveJson.GetInt(location, "day_index", SaveJson.GetInt(location, "dayIndex", 0))),
                ["normalized_time"] = SaveJson.GetFloat(location, "normalized_time", SaveJson.GetFloat(location, "normalizedTime", 0f)),
            };
        }

        private static Dictionary<string, object> ReadDict(Dictionary<string, object> row, string column, Dictionary<string, object> fallback, string flag, List<string> flags)
        {
            string raw = SaveJson.GetStr(row, column, null);
            if (string.IsNullOrEmpty(raw))
            {
                AddFlag(flags, "default:" + flag);
                return fallback;
            }
            try
            {
                var parsed = SaveJson.ParseDict(raw);
                if (parsed != null) return parsed;
            }
            catch { /* 部分损坏字段进入安全默认值，D7 原字符串仍保留在原表 */ }
            AddFlag(flags, "partial_recovery:" + flag);
            return fallback;
        }

        private static List<object> ReadList(Dictionary<string, object> row, string column, List<object> fallback, string flag, List<string> flags)
        {
            string raw = SaveJson.GetStr(row, column, null);
            if (string.IsNullOrEmpty(raw))
            {
                AddFlag(flags, "default:" + flag);
                return fallback;
            }
            try
            {
                var parsed = SaveJson.ParseArray(raw);
                if (parsed != null) return parsed;
            }
            catch { /* 部分损坏字段进入安全默认值，D7 原字符串仍保留在原表 */ }
            AddFlag(flags, "partial_recovery:" + flag);
            return fallback;
        }

        private static string ReadRawSnapshot(Dictionary<string, object> location, string key)
        {
            if (location == null || !location.TryGetValue(key, out object raw) || raw == null) return "";
            return raw is string ? (string)raw : SaveJson.Stringify(raw);
        }

        private static List<object> ReadIdentityHistoryIds(Dictionary<string, object> identity)
        {
            var result = new List<object>();
            if (identity == null || !identity.TryGetValue("identityHistory", out object raw) || !(raw is List<object> entries))
                return result;
            foreach (var entry in entries)
            {
                if (entry is Dictionary<string, object> map)
                {
                    string historyId = SaveJson.GetStr(map, "historyId", "");
                    if (!string.IsNullOrEmpty(historyId)) result.Add(historyId);
                }
            }
            return result;
        }

        private static string FindLastEventId(Dictionary<string, object> playerRow, Dictionary<string, object> location, List<string> flags)
        {
            var d3 = ReadEmbeddedDict(location, "d3", flags, "players.location_json.d3");
            var ids = ReadEmbeddedList(d3, "eventIds");
            if (ids.Count > 0)
            {
                string id = Convert.ToString(ids[ids.Count - 1]);
                if (!string.IsNullOrEmpty(id)) return id;
            }

            var d4 = ReadEmbeddedDict(location, "d4", flags, "players.location_json.d4");
            var routeLog = ReadEmbeddedList(d4, "routeLog");
            for (int i = routeLog.Count - 1; i >= 0; i--)
            {
                var entry = routeLog[i] as Dictionary<string, object>;
                string id = SaveJson.GetStr(entry, "id", "");
                if (!string.IsNullOrEmpty(id)) return id;
            }

            AddFlag(flags, "last_event_id_defaulted");
            return "";
        }

        private static Dictionary<string, object> ReadEmbeddedDict(Dictionary<string, object> parent, string key, List<string> flags, string flag)
        {
            if (parent == null || !parent.TryGetValue(key, out object value) || value == null) return null;
            if (value is Dictionary<string, object> dict) return dict;
            if (value is string text)
            {
                try { return SaveJson.ParseDict(text); } catch { AddFlag(flags, "partial_recovery:" + flag); }
            }
            return null;
        }

        private static List<object> ReadEmbeddedList(Dictionary<string, object> parent, string key)
        {
            if (parent == null || !parent.TryGetValue(key, out object value) || value == null) return new List<object>();
            if (value is List<object> list) return list;
            if (value is string text)
            {
                try { return SaveJson.ParseArray(text) ?? new List<object>(); } catch { return new List<object>(); }
            }
            return new List<object>();
        }

        private static List<string> ReadFlags(string raw)
        {
            var result = new List<string>();
            try
            {
                var list = SaveJson.ParseArray(raw) ?? new List<object>();
                foreach (var item in list)
                {
                    string flag = item as string;
                    if (!string.IsNullOrEmpty(flag)) AddFlag(result, flag);
                }
            }
            catch { AddFlag(result, "partial_recovery:save_metadata.migration_flags"); }
            return result;
        }

        private static void AddFlag(List<string> flags, string flag)
        {
            if (flags != null && !flags.Contains(flag)) flags.Add(flag);
        }
    }
}
