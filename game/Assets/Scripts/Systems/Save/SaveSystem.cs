// ============================================================
// SaveSystem —— 存档系统（P1B 真实实现：SQLite 存档 + 新游戏数据流）
//
// ★★★ 本文件是 P1A（调用方）与 P1B（实现方）之间的【唯一契约】 ★★★
// P0 定稿的 4 个公共方法签名（HasSave / NewGame / Save / Load）保持不变；
// 新增扩展（GameState、DbPath 测试覆盖）经 DEV_ROADMAP D1 统筹授权。
//
// 归属：Core（存档所有权，见 DEVELOPMENT_STANDARDS §1 与 DESIGN_BOUNDARIES §6）
// 红线：Systems/ 目录禁止 LLM / HttpClient / 网络代码 —— 本文件零网络依赖，
//       数据访问仅走本地 SQLite（sqlite.org 官方 sqlite3.dll，见 Db/ 目录）。
// 存储：Application.persistentDataPath/save.db（测试可通过 DbPath 覆盖到临时目录）
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Whalebone.Agent;

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// 新游戏初始化参数（P0 定稿，未修改）。
    /// </summary>
    public struct NewGameParams
    {
        /// <summary>船长名（由玩家输入/创建角色界面提供）。</summary>
        public string CaptainName;

        /// <summary>初始船型 ID；调用方未指定时由实现层回退为 "small_fishing"（DATA_MODELS §5）。</summary>
        public string ShipTypeId;
    }

    /// <summary>
    /// 存档系统静态入口。
    /// </summary>
    public static class SaveSystem
    {
        private const string DbFileName = "save.db";
        private const int SchemaVersion = 1;

        /// <summary>
        /// 数据库路径覆盖（null = 使用默认 persistentDataPath）。仅测试 / 工具使用。
        /// </summary>
        internal static string DbPath { get; set; }

        internal static string ResolveDbPath()
        {
            if (!string.IsNullOrEmpty(DbPath)) return DbPath;
            return Path.Combine(Application.persistentDataPath, DbFileName);
        }

        // ============================================================
        // 公共 API（P0 定稿签名）
        // ============================================================

        /// <summary>是否存在可读取的存档（players 表存在且有玩家行）。</summary>
        public static bool HasSave()
        {
            if (!File.Exists(ResolveDbPath())) return false;
            try
            {
                using (var db = OpenDb())
                {
                    var rows = db.Query("SELECT COUNT(*) AS c FROM players");
                    return rows.Count > 0 && Convert.ToInt64(rows[0]["c"]) > 0;
                }
            }
            catch
            {
                return false; // 数据库损坏/不可读时视为无存档，不抛出（保证主菜单可用）
            }
        }

        /// <summary>
        /// 初始化新游戏：生成 Player（船长/金钱 500/技能默认 1/圣玛格丽特港）
        /// + Ship（small_fishing，满耐久）+ 初始船员老水手，全部落库。
        /// </summary>
        public static void NewGame(NewGameParams p)
        {
            string captainName = string.IsNullOrEmpty(p.CaptainName) ? "无名船长" : p.CaptainName;
            string shipTypeId = string.IsNullOrEmpty(p.ShipTypeId) ? "small_fishing" : p.ShipTypeId;
            var rng = new System.Random();

            EnsureSchemaAndSeed();

            // 新游戏 = 覆盖旧档：清空三张存档表（trait_defs / ship_type_defs 种子表保留）
            using (var db = OpenDb())
            {
                db.Execute("DELETE FROM players");
                db.Execute("DELETE FROM ships");
                db.Execute("DELETE FROM crew_agents");
            }

            // --- 生成实体（全部数值初始化为默认值，后续由各系统裁决修改）---
            var player = PlayerState.CreateNew(NewId("player"), captainName);
            var shipType = ShipTypeSeed.Find(shipTypeId) ?? ShipTypeSeed.Find("small_fishing");
            var ship = ShipState.CreateNew(NewId("ship"), shipType, player.Id);

            // 玩家特质：随机 3-4 条（DATA_MODELS §1 约定 3-4 条）
            player.Traits = RollRandomTraits(rng, TraitSeed.All, 3, 4);

            // 初始船员：老水手（固定含"老水手"经历特质 + 随机 2-3 条身体/性格特质 → 共 3-4 条）
            var (sailor, sailorTraitIds) = CreateOldSailor(rng);
            ship.CrewIds.Add(sailor.Id);

            player.ShipId = ship.Id;

            // --- 入内存（GameState 单一写者入口）---
            GameState.Reset();
            GameState.CurrentPlayer = player;
            GameState.CurrentShip = ship;
            GameState.CrewAgents.Add(sailor);
            GameState.CrewAgentTraitIds[sailor.Id] = sailorTraitIds; // 见 CrewAgentCodec 注释（桥接占位）

            // --- 落库 ---
            WritePlayer(player);
            WriteShip(ship);
            WriteCrew(sailor);
        }

        /// <summary>把内存状态（Player/Ship/CrewAgent）序列化回三表。</summary>
        public static void Save()
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存（请先 NewGame 或 Load）。");

            EnsureSchemaAndSeed();
            WritePlayer(GameState.CurrentPlayer);
            if (GameState.CurrentShip != null) WriteShip(GameState.CurrentShip);
            foreach (var c in GameState.CrewAgents) WriteCrew(c);
        }

        /// <summary>从 SQLite 恢复 Player/Ship/CrewAgent 到 GameState，成功返回 true。</summary>
        public static bool Load()
        {
            if (!HasSave()) return false;
            try
            {
                using (var db = OpenDb())
                {
                    var pRows = db.Query("SELECT * FROM players LIMIT 1");
                    if (pRows.Count == 0) return false;
                    var player = ReadPlayer(pRows[0]);

                    var sRows = db.Query("SELECT * FROM ships WHERE id = ?", player.ShipId);
                    if (sRows.Count == 0) return false;
                    var ship = ReadShip(sRows[0], db);

                    // 先收集，后 Reset：Reset 会清空 CrewAgentTraitIds
                    var crew = new List<CrewAgent>();
                    var crewTraitIds = new List<KeyValuePair<string, List<string>>>();
                    foreach (var r in db.Query("SELECT * FROM crew_agents"))
                    {
                        var agent = CrewAgentCodec.FromRow(r);
                        crew.Add(agent);
                        crewTraitIds.Add(new KeyValuePair<string, List<string>>(agent.Id, CrewAgentCodec.ReadTraitIds(r)));
                    }

                    GameState.Reset();
                    GameState.CurrentPlayer = player;
                    GameState.CurrentShip = ship;
                    GameState.CrewAgents.AddRange(crew);
                    foreach (var kv in crewTraitIds)
                        GameState.CrewAgentTraitIds[kv.Key] = kv.Value;

                    return true;
                }
            }
            catch
            {
                return false; // 存档损坏时静默失败，由上层决定（可提示重新开始）
            }
        }

        // ============================================================
        // 建表与种子
        // ============================================================

        private static readonly string SchemaSql =
@"CREATE TABLE IF NOT EXISTS players(
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  money INTEGER NOT NULL DEFAULT 0,
  skills_json TEXT NOT NULL,
  traits_json TEXT NOT NULL,
  reputation_json TEXT NOT NULL,
  inventory_json TEXT NOT NULL,
  ship_id TEXT,
  location_json TEXT NOT NULL,
  play_seconds INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS ships(
  id TEXT PRIMARY KEY,
  ship_type_id TEXT NOT NULL,
  name TEXT NOT NULL,
  hull INTEGER NOT NULL DEFAULT 0,
  cargo_json TEXT NOT NULL,
  cannons INTEGER NOT NULL DEFAULT 0,
  loaded_count INTEGER NOT NULL DEFAULT 0,
  supplies_json TEXT NOT NULL,
  crew_json TEXT NOT NULL,
  upgrades_json TEXT NOT NULL,
  state_json TEXT NOT NULL,
  ship_log_json TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS crew_agents(
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  role TEXT NOT NULL,
  culture TEXT NOT NULL,
  traits_json TEXT NOT NULL,
  stats_json TEXT NOT NULL,
  persona_json TEXT NOT NULL,
  memory_json TEXT NOT NULL,
  relations_json TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS trait_defs(
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  category TEXT NOT NULL,
  desc TEXT NOT NULL,
  stat_mods_json TEXT NOT NULL,
  persona_note TEXT NOT NULL,
  event_mods_json TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS ship_type_defs(
  id TEXT PRIMARY KEY,
  name TEXT NOT NULL,
  speed REAL NOT NULL,
  turn REAL NOT NULL,
  cargo_max INTEGER NOT NULL,
  crew_max INTEGER NOT NULL,
  cannon_max INTEGER NOT NULL,
  hull_max INTEGER NOT NULL,
  draft REAL NOT NULL,
  price INTEGER NOT NULL
);
PRAGMA user_version = " + SchemaVersion + @";
";

        /// <summary>建表（幂等）+ 写入种子数据（按 id INSERT OR IGNORE，幂等）。</summary>
        internal static void EnsureSchemaAndSeed()
        {
            using (var db = OpenDb())
            {
                db.Execute(SchemaSql);

                foreach (var t in TraitSeed.All)
                {
                    db.Execute(
                        "INSERT OR IGNORE INTO trait_defs(id,name,category,desc,stat_mods_json,persona_note,event_mods_json) VALUES(?,?,?,?,?,?,?)",
                        t.Id, t.Name, t.Category.ToString(), t.Desc,
                        SaveJson.Stringify(t.StatMods), t.PersonaNote,
                        SaveJson.Stringify(t.EventMods));
                }

                foreach (var s in ShipTypeSeed.All)
                {
                    db.Execute(
                        "INSERT OR IGNORE INTO ship_type_defs(id,name,speed,turn,cargo_max,crew_max,cannon_max,hull_max,draft,price) VALUES(?,?,?,?,?,?,?,?,?,?)",
                        s.Id, s.Name, (double)s.Speed, (double)s.Turn, s.CargoMax, s.CrewMax,
                        s.CannonMax, s.HullMax, (double)s.Draft, s.Price);
                }
            }
        }

        private static SqliteDb OpenDb()
        {
            return new SqliteDb(ResolveDbPath());
        }

        // ============================================================
        // Player 行读写
        // ============================================================

        private static void WritePlayer(PlayerState p)
        {
            using (var db = OpenDb())
            {
                db.Execute(
                    "INSERT OR REPLACE INTO players(id,name,money,skills_json,traits_json,reputation_json,inventory_json,ship_id,location_json,play_seconds) VALUES(?,?,?,?,?,?,?,?,?,?)",
                    p.Id, p.Name, p.Money,
                    SaveJson.Stringify(EnumDictToStrings(p.Skills)),
                    SaveJson.Stringify(p.Traits),
                    SaveJson.Stringify(p.Reputation),
                    SaveJson.Stringify(p.Inventory),
                    p.ShipId,
                    SaveJson.Stringify(LocationToObj(p.Location)),
                    p.PlaySeconds);
            }
        }

        private static PlayerState ReadPlayer(Dictionary<string, object> row)
        {
            var p = new PlayerState
            {
                Id = SaveJson.GetStr(row, "id", ""),
                Name = SaveJson.GetStr(row, "name", ""),
                Money = SaveJson.GetInt(row, "money"),
                ShipId = SaveJson.GetStr(row, "ship_id", null),
                PlaySeconds = SaveJson.GetInt(row, "play_seconds"),
            };

            // skills：JSON key = 枚举名
            var skillsJson = SaveJson.ParseDict(SaveJson.GetStr(row, "skills_json", "{}"));
            if (skillsJson != null)
            {
                foreach (var kv in skillsJson)
                {
                    if (Enum.TryParse(kv.Key, true, out SkillType st) && kv.Value is long lv)
                        p.Skills[st] = (int)lv;
                }
            }

            p.Traits.AddRange(ReadStringArray(SaveJson.GetStr(row, "traits_json", "[]")));

            var rep = SaveJson.ParseDict(SaveJson.GetStr(row, "reputation_json", "{}"));
            if (rep != null) foreach (var kv in rep) p.Reputation[kv.Key] = (int)ToLong(kv.Value);

            var inv = SaveJson.ParseDict(SaveJson.GetStr(row, "inventory_json", "{}"));
            if (inv != null) foreach (var kv in inv) p.Inventory[kv.Key] = (int)ToLong(kv.Value);

            var loc = SaveJson.ParseDict(SaveJson.GetStr(row, "location_json", "{}"));
            if (loc != null)
            {
                p.Location.Type = SaveJson.GetStr(loc, "type", "port");
                p.Location.PortId = SaveJson.GetStr(loc, "portId", null);
                p.Location.X = SaveJson.GetFloat(loc, "x");
                p.Location.Y = SaveJson.GetFloat(loc, "y");
            }

            return p;
        }

        // ============================================================
        // Ship 行读写
        // ============================================================

        private static void WriteShip(ShipState s)
        {
            using (var db = OpenDb())
            {
                var logList = new List<object>();
                foreach (var e in s.ShipLog)
                    logList.Add(new Dictionary<string, object> { ["time"] = e.Time, ["kind"] = e.Kind, ["text"] = e.Text });

                db.Execute(
                    "INSERT OR REPLACE INTO ships(id,ship_type_id,name,hull,cargo_json,cannons,loaded_count,supplies_json,crew_json,upgrades_json,state_json,ship_log_json) VALUES(?,?,?,?,?,?,?,?,?,?,?,?)",
                    s.Id, s.ShipTypeId, s.Name, s.Hull,
                    SaveJson.Stringify(s.Cargo),
                    s.Cannons, s.LoadedCount,
                    SaveJson.Stringify(s.Supplies),
                    SaveJson.Stringify(s.CrewIds),
                    SaveJson.Stringify(s.Upgrades),
                    SaveJson.Stringify(s.State),
                    SaveJson.Stringify(logList));
            }
        }

        private static ShipState ReadShip(Dictionary<string, object> row, SqliteDb db)
        {
            var s = new ShipState
            {
                Id = SaveJson.GetStr(row, "id", ""),
                ShipTypeId = SaveJson.GetStr(row, "ship_type_id", ""),
                Name = SaveJson.GetStr(row, "name", ""),
                Hull = SaveJson.GetInt(row, "hull"),
                Cannons = SaveJson.GetInt(row, "cannons"),
                LoadedCount = SaveJson.GetInt(row, "loaded_count"),
            };

            // hullMax 不落库：回查 ship_type_defs（数据单一事实源）
            var typeRows = db.Query("SELECT hull_max FROM ship_type_defs WHERE id = ?", s.ShipTypeId);
            s.HullMax = typeRows.Count > 0 ? SaveJson.GetInt(typeRows[0], "hull_max", s.Hull) : s.Hull;

            FillIntDict(s.Cargo, SaveJson.GetStr(row, "cargo_json", "{}"));
            FillIntDict(s.Supplies, SaveJson.GetStr(row, "supplies_json", "{}"));
            FillIntDict(s.Upgrades, SaveJson.GetStr(row, "upgrades_json", "{}"));
            FillFloatDict(s.State, SaveJson.GetStr(row, "state_json", "{}"));

            foreach (var id in ReadStringArray(SaveJson.GetStr(row, "crew_json", "[]")))
                s.CrewIds.Add(id);

            var logJson = SaveJson.ParseArray(SaveJson.GetStr(row, "ship_log_json", "[]"));
            if (logJson != null)
            {
                foreach (var o in logJson)
                {
                    if (o is Dictionary<string, object> e)
                        s.ShipLog.Add(new ShipLogEntry
                        {
                            Time = SaveJson.GetStr(e, "time", ""),
                            Kind = SaveJson.GetStr(e, "kind", ""),
                            Text = SaveJson.GetStr(e, "text", ""),
                        });
                }
            }

            return s;
        }

        // ============================================================
        // CrewAgent 行读写
        // ============================================================

        private static void WriteCrew(CrewAgent agent)
        {
            GameState.CrewAgentTraitIds.TryGetValue(agent.Id, out List<string> traitIds);
            using (var db = OpenDb())
            {
                var row = CrewAgentCodec.ToRow(agent, traitIds ?? new List<string>());
                db.Execute(
                    "INSERT OR REPLACE INTO crew_agents(id,name,role,culture,traits_json,stats_json,persona_json,memory_json,relations_json) VALUES(?,?,?,?,?,?,?,?,?)",
                    row["id"], row["name"], row["role"], row["culture"],
                    row["traits_json"], row["stats_json"], row["persona_json"],
                    row["memory_json"], row["relations_json"]);
            }
        }

        // ============================================================
        // 新游戏生成辅助
        // ============================================================

        /// <summary>创建初始船员"老水手"：固定含 trait_old_sailor + 随机 2-3 条（共 3-4 条）。</summary>
        private static (CrewAgent agent, List<string> traitIds) CreateOldSailor(System.Random rng)
        {
            string id = NewId("crew");

            // 特质引用：固定"老水手"经历 + 从身体/性格池随机补足
            var traitIds = new List<string> { "trait_old_sailor" };
            var pool = new List<TraitDef>();
            foreach (var t in TraitSeed.All)
                if (t.Category != TraitCategory.Experience && t.Id != "trait_old_sailor")
                    pool.Add(t);
            Shuffle(pool, rng);
            int extra = rng.Next(2, 4); // 2-3 条 → 总计 3-4 条
            for (int i = 0; i < extra && i < pool.Count; i++)
                traitIds.Add(pool[i].Id);

            var persona = new PersonaDefinition(CrewAgentCodec.OldSailorName, CrewAgentCodec.OldSailorPersonaRole);
            foreach (var traitId in traitIds)
            {
                var t = TraitSeed.Find(traitId);
                if (t != null) persona.Traits.Add(t.Name); // Persona 用中文名（展示/LLM 用）
            }
            persona.Backstory = "在加勒比海域漂泊了三十年的老水手，见过飓风、海盗和沉船，什么风浪都认得出。";
            persona.SpeechStyle = "低沉沙哑，说话带海风味的俚语。";

            return (new CrewAgent(id, persona, new InMemoryStore()), traitIds);
        }

        /// <summary>从给定池随机抽取 min~max 条特质（去重），返回 id 列表。</summary>
        private static List<string> RollRandomTraits(System.Random rng, IReadOnlyList<TraitDef> pool, int min, int max)
        {
            var shuffled = new List<TraitDef>(pool);
            Shuffle(shuffled, rng);
            int count = rng.Next(min, max + 1);
            if (count > shuffled.Count) count = shuffled.Count;
            var result = new List<string>();
            for (int i = 0; i < count; i++) result.Add(shuffled[i].Id);
            return result;
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static string NewId(string prefix)
        {
            return prefix + "_" + Guid.NewGuid().ToString("N");
        }

        // ============================================================
        // 序列化辅助
        // ============================================================

        private static Dictionary<string, object> EnumDictToStrings(Dictionary<SkillType, int> source)
        {
            var result = new Dictionary<string, object>();
            foreach (var kv in source) result[kv.Key.ToString()] = kv.Value;
            return result;
        }

        private static Dictionary<string, object> LocationToObj(LocationRef loc)
        {
            return new Dictionary<string, object>
            {
                ["type"] = loc.Type,
                ["portId"] = loc.PortId ?? "",
                ["x"] = loc.X,
                ["y"] = loc.Y,
            };
        }

        private static List<string> ReadStringArray(string json)
        {
            var result = new List<string>();
            var parsed = SaveJson.ParseArray(json);
            if (parsed != null)
                foreach (var o in parsed) result.Add(Convert.ToString(o));
            return result;
        }

        private static void FillIntDict(Dictionary<string, int> target, string json)
        {
            var parsed = SaveJson.ParseDict(json);
            if (parsed == null) return;
            foreach (var kv in parsed) target[kv.Key] = (int)ToLong(kv.Value);
        }

        private static void FillFloatDict(Dictionary<string, float> target, string json)
        {
            var parsed = SaveJson.ParseDict(json);
            if (parsed == null) return;
            foreach (var kv in parsed) target[kv.Key] = (float)ToDouble(kv.Value);
        }

        private static long ToLong(object v)
        {
            if (v is long l) return l;
            if (v is double d) return (long)d;
            return 0;
        }

        private static double ToDouble(object v)
        {
            if (v is double d) return d;
            if (v is long l) return l;
            return 0;
        }
    }
}
