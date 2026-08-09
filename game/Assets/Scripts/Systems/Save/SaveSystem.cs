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
using Whalebone.Systems.D2;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;

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

        /// <summary>
        /// D2 运行时快照的唯一落库入口。新增字段写入已有 location_json，旧数据库无需迁移。
        /// </summary>
        public static void SaveD2Runtime(SailingSnapshot sailing, DayNightSnapshot dayNight)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D2 快照。");

            var location = GameState.CurrentPlayer.Location ?? new LocationRef();
            if (sailing != null)
            {
                location.Type = "open_sea";
                location.PortId = null;
                location.X = sailing.PositionX;
                location.Y = sailing.PositionZ;
                location.Heading = sailing.Heading;
                location.Speed = sailing.Speed;
            }
            location.DayIndex = Math.Max(0, dayNight.DayIndex);
            location.NormalizedTime = dayNight.NormalizedTime;
            GameState.CurrentPlayer.Location = location;
            Save();
        }

        /// <summary>
        /// D3 快照的唯一落库入口。快照作为 location_json.d3 保存，旧数据库无需迁移；
        /// Save 域不计算金钱、价格、补给或事件，只复制可序列化状态。
        /// </summary>
        public static void SaveD3Runtime(D3RuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D3 快照。");
            GameState.CurrentD3Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D3Runtime = GameState.CurrentD3Runtime == null
                ? null : GameState.CurrentD3Runtime.Clone();
            Save();
        }

        /// <summary>
        /// D4 快照的唯一落库入口。快照作为 location_json.d4 保存，旧数据库无需迁移；
        /// Save 域只复制内容，不计算战斗、声望、疾病或任务结果。
        /// </summary>
        public static void SaveD4Runtime(D4RuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D4 快照。");
            GameState.CurrentD4Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D4Runtime = GameState.CurrentD4Runtime == null
                ? null : GameState.CurrentD4Runtime.Clone();
            Save();
        }

        /// <summary>读取当前已加载的 D3 快照副本；旧 D2 存档没有快照时返回 null。</summary>
        public static D3RuntimeSnapshot LoadD3Runtime()
        {
            var snapshot = GameState.CurrentD3Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D3Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D4 快照副本；D1-D3 旧存档没有该字段时返回 null。</summary>
        public static D4RuntimeSnapshot LoadD4Runtime()
        {
            var snapshot = GameState.CurrentD4Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D4Runtime);
            return snapshot == null ? null : snapshot.Clone();
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
                    GameState.CurrentD3Runtime = player.D3Runtime == null ? null : player.D3Runtime.Clone();
                    GameState.CurrentD4Runtime = player.D4Runtime == null ? null : player.D4Runtime.Clone();
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
                    SaveJson.Stringify(LocationToObj(p.Location, p.D3Runtime, p.D4Runtime)),
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
                p.Location.Heading = SaveJson.GetFloat(loc, "heading");
                p.Location.Speed = SaveJson.GetFloat(loc, "speed");
                p.Location.DayIndex = SaveJson.GetInt(loc, "dayIndex");
                p.Location.NormalizedTime = SaveJson.GetFloat(loc, "normalizedTime", 0.25f);
                if (p.Location.NormalizedTime < 0f || p.Location.NormalizedTime >= 1f)
                {
                    p.Location.NormalizedTime %= 1f;
                    if (p.Location.NormalizedTime < 0f) p.Location.NormalizedTime += 1f;
                }
                if (loc.TryGetValue("d3", out object d3Value) && d3Value is Dictionary<string, object> d3Object)
                    p.D3Runtime = D3FromObj(d3Object);
                if (loc.TryGetValue("d4", out object d4Value) && d4Value is Dictionary<string, object> d4Object)
                    p.D4Runtime = D4FromObj(d4Object);
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

        private static Dictionary<string, object> LocationToObj(LocationRef loc, D3RuntimeSnapshot d3 = null, D4RuntimeSnapshot d4 = null)
        {
            var result = new Dictionary<string, object>
            {
                ["type"] = loc.Type,
                ["portId"] = loc.PortId ?? "",
                ["x"] = loc.X,
                ["y"] = loc.Y,
                ["heading"] = loc.Heading,
                ["speed"] = loc.Speed,
                ["dayIndex"] = loc.DayIndex,
                ["normalizedTime"] = loc.NormalizedTime,
            };
            if (d3 != null) result["d3"] = D3ToObj(d3);
            if (d4 != null) result["d4"] = D4ToObj(d4);
            return result;
        }

        private static Dictionary<string, object> D3ToObj(D3RuntimeSnapshot d3)
        {
            return new Dictionary<string, object>
            {
                ["currentLocationId"] = d3.CurrentLocationId ?? "",
                ["destinationId"] = d3.DestinationId ?? "",
                ["voyageStatus"] = d3.VoyageStatus.ToString(),
                ["weather"] = d3.Weather.ToString(),
                ["currentDay"] = d3.CurrentDay,
                ["totalDays"] = d3.TotalDays,
                ["voyageSeed"] = d3.VoyageSeed,
                ["water"] = d3.Water,
                ["food"] = d3.Food,
                ["hull"] = d3.Hull,
                ["hullMax"] = d3.HullMax,
                ["money"] = d3.Money,
                ["cargo"] = d3.Cargo,
                ["personalItems"] = d3.PersonalItems,
                ["marketStock"] = d3.MarketStock,
                ["gatheredThisVisit"] = d3.GatheredThisVisit,
                ["eventIds"] = d3.EventIds,
            };
        }

        private static D3RuntimeSnapshot D3FromObj(Dictionary<string, object> obj)
        {
            var result = new D3RuntimeSnapshot
            {
                CurrentLocationId = SaveJson.GetStr(obj, "currentLocationId", null),
                DestinationId = SaveJson.GetStr(obj, "destinationId", null),
                CurrentDay = SaveJson.GetInt(obj, "currentDay"),
                TotalDays = SaveJson.GetInt(obj, "totalDays"),
                VoyageSeed = SaveJson.GetInt(obj, "voyageSeed"),
                Water = SaveJson.GetInt(obj, "water"),
                Food = SaveJson.GetInt(obj, "food"),
                Hull = SaveJson.GetInt(obj, "hull"),
                HullMax = SaveJson.GetInt(obj, "hullMax"),
                Money = SaveJson.GetInt(obj, "money"),
            };
            if (Enum.TryParse(SaveJson.GetStr(obj, "voyageStatus", "InPort"), true, out D3VoyageStatus status))
                result.VoyageStatus = status;
            if (Enum.TryParse(SaveJson.GetStr(obj, "weather", "Calm"), true, out D3WeatherType weather))
                result.Weather = weather;
            ReadIntDictObject(result.Cargo, obj, "cargo");
            ReadIntDictObject(result.PersonalItems, obj, "personalItems");
            ReadIntDictObject(result.MarketStock, obj, "marketStock");
            ReadIntDictObject(result.GatheredThisVisit, obj, "gatheredThisVisit");
            ReadStringListObject(result.EventIds, obj, "eventIds");
            return result;
        }

        private static Dictionary<string, object> D4ToObj(D4RuntimeSnapshot d4)
        {
            var reputation = new Dictionary<string, object>();
            foreach (var pair in d4.Factions.Reputation) reputation[pair.Key] = pair.Value;
            var faction = new Dictionary<string, object>
            {
                ["reputation"] = reputation,
                ["atWar"] = StringListToObjects(d4.Factions.AtWar),
                ["embargoedPorts"] = StringListToObjects(d4.Factions.EmbargoedPorts),
                ["privateerPermits"] = StringListToObjects(d4.Factions.PrivateerPermits),
                ["wantedLevel"] = d4.Factions.WantedLevel,
            };

            var upgrades = new Dictionary<string, object>();
            foreach (var pair in d4.Shipyard.Upgrades) upgrades[pair.Key] = pair.Value;
            var shipyard = new Dictionary<string, object>
            {
                ["shipTypeId"] = d4.Shipyard.ShipTypeId ?? "small_fishing",
                ["unlockedShipTypes"] = StringListToObjects(d4.Shipyard.UnlockedShipTypes),
                ["upgrades"] = upgrades,
            };

            var combat = new Dictionary<string, object>
            {
                ["status"] = d4.Combat.Status.ToString(), ["enemyId"] = d4.Combat.EnemyId ?? "",
                ["playerHull"] = d4.Combat.PlayerHull, ["playerHullMax"] = d4.Combat.PlayerHullMax,
                ["enemyHull"] = d4.Combat.EnemyHull, ["enemyHullMax"] = d4.Combat.EnemyHullMax,
                ["distance"] = d4.Combat.Distance, ["wind"] = d4.Combat.Wind,
                ["playerCannons"] = d4.Combat.PlayerCannons, ["loadedCount"] = d4.Combat.LoadedCount,
                ["weapon"] = d4.Combat.Weapon.ToString(), ["enemyCannons"] = d4.Combat.EnemyCannons,
                ["enemyLoadedCount"] = d4.Combat.EnemyLoadedCount, ["enemyCrew"] = d4.Combat.EnemyCrew,
                ["playerCrew"] = d4.Combat.PlayerCrew, ["turn"] = d4.Combat.Turn,
                ["boardingScore"] = d4.Combat.BoardingScore,
            };

            var animals = new List<object>();
            foreach (var animal in d4.Ecology.Animals)
                animals.Add(new Dictionary<string, object>
                {
                    ["animalId"] = animal.AnimalId ?? "", ["behavior"] = animal.Behavior.ToString(),
                    ["health"] = animal.Health, ["observed"] = animal.Observed,
                    ["collectionValue"] = animal.CollectionValue,
                });
            var ecology = new Dictionary<string, object>
            {
                ["habitatId"] = d4.Ecology.HabitatId ?? "", ["animals"] = animals,
                ["collectionCount"] = d4.Ecology.CollectionCount,
            };

            var crewHealth = new List<object>();
            foreach (var crew in d4.Health.Crew)
                crewHealth.Add(new Dictionary<string, object>
                {
                    ["crewId"] = crew.CrewId ?? "", ["health"] = crew.Health, ["fatigue"] = crew.Fatigue,
                    ["diseases"] = StringListToObjects(crew.Diseases),
                });
            var health = new Dictionary<string, object>
            {
                ["crew"] = crewHealth, ["medicalSupplies"] = d4.Health.MedicalSupplies,
            };

            var flags = new Dictionary<string, object>();
            foreach (var pair in d4.Mainline.Flags) flags[pair.Key] = pair.Value;
            var mainline = new Dictionary<string, object>
            {
                ["chapter"] = d4.Mainline.Chapter, ["missionIndex"] = d4.Mainline.MissionIndex,
                ["flags"] = flags, ["completedMissions"] = StringListToObjects(d4.Mainline.CompletedMissions),
            };

            var logs = new List<object>();
            foreach (var entry in d4.RouteLog)
                logs.Add(new Dictionary<string, object>
                {
                    ["id"] = entry.Id ?? "", ["day"] = entry.Day, ["kind"] = entry.Kind.ToString(),
                    ["originId"] = entry.OriginId ?? "", ["destinationId"] = entry.DestinationId ?? "",
                    ["text"] = entry.Text ?? "",
                });

            return new Dictionary<string, object>
            {
                ["factions"] = faction, ["shipyard"] = shipyard, ["combat"] = combat,
                ["ecology"] = ecology, ["health"] = health, ["mainline"] = mainline, ["routeLog"] = logs,
            };
        }

        private static D4RuntimeSnapshot D4FromObj(Dictionary<string, object> obj)
        {
            var result = new D4RuntimeSnapshot();
            var faction = Nested(obj, "factions");
            if (faction != null)
            {
                ReadIntDictObject(result.Factions.Reputation, faction, "reputation");
                ReadStringListObject(result.Factions.AtWar, faction, "atWar");
                ReadStringListObject(result.Factions.EmbargoedPorts, faction, "embargoedPorts");
                ReadStringListObject(result.Factions.PrivateerPermits, faction, "privateerPermits");
                result.Factions.WantedLevel = SaveJson.GetInt(faction, "wantedLevel");
            }

            var shipyard = Nested(obj, "shipyard");
            if (shipyard != null)
            {
                result.Shipyard.ShipTypeId = SaveJson.GetStr(shipyard, "shipTypeId", "small_fishing");
                result.Shipyard.UnlockedShipTypes.Clear();
                ReadStringListObject(result.Shipyard.UnlockedShipTypes, shipyard, "unlockedShipTypes");
                ReadIntDictObject(result.Shipyard.Upgrades, shipyard, "upgrades");
            }

            var combat = Nested(obj, "combat");
            if (combat != null)
            {
                result.Combat.EnemyId = SaveJson.GetStr(combat, "enemyId", null);
                result.Combat.PlayerHull = SaveJson.GetInt(combat, "playerHull");
                result.Combat.PlayerHullMax = SaveJson.GetInt(combat, "playerHullMax");
                result.Combat.EnemyHull = SaveJson.GetInt(combat, "enemyHull");
                result.Combat.EnemyHullMax = SaveJson.GetInt(combat, "enemyHullMax");
                result.Combat.Distance = SaveJson.GetFloat(combat, "distance");
                result.Combat.Wind = SaveJson.GetFloat(combat, "wind");
                result.Combat.PlayerCannons = SaveJson.GetInt(combat, "playerCannons");
                result.Combat.LoadedCount = SaveJson.GetInt(combat, "loadedCount");
                result.Combat.EnemyCannons = SaveJson.GetInt(combat, "enemyCannons");
                result.Combat.EnemyLoadedCount = SaveJson.GetInt(combat, "enemyLoadedCount");
                result.Combat.EnemyCrew = SaveJson.GetInt(combat, "enemyCrew");
                result.Combat.PlayerCrew = SaveJson.GetInt(combat, "playerCrew");
                result.Combat.Turn = SaveJson.GetInt(combat, "turn");
                result.Combat.BoardingScore = SaveJson.GetInt(combat, "boardingScore");
                Enum.TryParse(SaveJson.GetStr(combat, "status", "Idle"), true, out result.Combat.Status);
                Enum.TryParse(SaveJson.GetStr(combat, "weapon", "RoundShot"), true, out result.Combat.Weapon);
            }

            var ecology = Nested(obj, "ecology");
            if (ecology != null)
            {
                result.Ecology.HabitatId = SaveJson.GetStr(ecology, "habitatId", "");
                result.Ecology.CollectionCount = SaveJson.GetInt(ecology, "collectionCount");
                foreach (var animalObj in ObjectList(ecology, "animals"))
                {
                    var animal = new AnimalState
                    {
                        AnimalId = SaveJson.GetStr(animalObj, "animalId", ""),
                        Health = SaveJson.GetInt(animalObj, "health", 100),
                        Observed = Convert.ToBoolean(animalObj.ContainsKey("observed") ? animalObj["observed"] : false),
                        CollectionValue = SaveJson.GetInt(animalObj, "collectionValue"),
                    };
                    Enum.TryParse(SaveJson.GetStr(animalObj, "behavior", "Foraging"), true, out animal.Behavior);
                    result.Ecology.Animals.Add(animal);
                }
            }

            var health = Nested(obj, "health");
            if (health != null)
            {
                result.Health.MedicalSupplies = SaveJson.GetInt(health, "medicalSupplies");
                foreach (var crewObj in ObjectList(health, "crew"))
                {
                    var crew = new CrewHealthState
                    {
                        CrewId = SaveJson.GetStr(crewObj, "crewId", ""),
                        Health = SaveJson.GetInt(crewObj, "health", 100),
                        Fatigue = SaveJson.GetInt(crewObj, "fatigue"),
                    };
                    ReadStringListObject(crew.Diseases, crewObj, "diseases");
                    result.Health.Crew.Add(crew);
                }
            }

            var mainline = Nested(obj, "mainline");
            if (mainline != null)
            {
                result.Mainline.Chapter = SaveJson.GetInt(mainline, "chapter", 1);
                result.Mainline.MissionIndex = SaveJson.GetInt(mainline, "missionIndex");
                ReadStringListObject(result.Mainline.CompletedMissions, mainline, "completedMissions");
                var flags = Nested(mainline, "flags");
                if (flags != null)
                    foreach (var pair in flags) result.Mainline.Flags[pair.Key] = Convert.ToBoolean(pair.Value);
            }

            foreach (var logObj in ObjectList(obj, "routeLog"))
            {
                var entry = new RouteLogEntry
                {
                    Id = SaveJson.GetStr(logObj, "id", ""), Day = SaveJson.GetInt(logObj, "day"),
                    OriginId = SaveJson.GetStr(logObj, "originId", null), DestinationId = SaveJson.GetStr(logObj, "destinationId", null),
                    Text = SaveJson.GetStr(logObj, "text", ""),
                };
                Enum.TryParse(SaveJson.GetStr(logObj, "kind", "Voyage"), true, out entry.Kind);
                result.RouteLog.Add(entry);
            }
            return result;
        }

        private static Dictionary<string, object> Nested(Dictionary<string, object> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out object value)) return null;
            return value as Dictionary<string, object>;
        }

        private static List<Dictionary<string, object>> ObjectList(Dictionary<string, object> source, string key)
        {
            var result = new List<Dictionary<string, object>>();
            if (source == null || !source.TryGetValue(key, out object value) || !(value is List<object> list)) return result;
            foreach (var item in list)
                if (item is Dictionary<string, object> dict) result.Add(dict);
            return result;
        }

        private static List<object> StringListToObjects(List<string> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var value in values) result.Add(value ?? "");
            return result;
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

        private static void ReadIntDictObject(Dictionary<string, int> target,
            Dictionary<string, object> source, string key)
        {
            if (target == null || source == null || !source.TryGetValue(key, out object value)) return;
            if (!(value is Dictionary<string, object> dict)) return;
            foreach (var item in dict) target[item.Key] = (int)ToLong(item.Value);
        }

        private static void ReadStringListObject(List<string> target,
            Dictionary<string, object> source, string key)
        {
            if (target == null || source == null || !source.TryGetValue(key, out object value)) return;
            if (!(value is List<object> list)) return;
            foreach (var item in list) target.Add(Convert.ToString(item));
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
