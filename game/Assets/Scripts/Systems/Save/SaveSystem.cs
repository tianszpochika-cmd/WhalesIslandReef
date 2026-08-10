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
using Whalebone.Game.Config;
using Whalebone.Systems.D2;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D5;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.D12;
using Whalebone.Systems.D11;
using Whalebone.Systems.D13;
using Whalebone.Systems.D14;
using Whalebone.Systems.Events;

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

        /// <summary>D10 创建时身份表达；为空时由 CaptainName 生成默认身份。</summary>
        public D10PlayerIdentityCreation Identity;
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

        /// <summary>
        /// 将当前 D7 SQLite 存档迁移到 D8 save_version=3。迁移器只扩展存档，
        /// 不删除或重写 D7 三张兼容表；失败时由迁移器回滚新增写入。
        /// </summary>
        public static SaveMigrationResult MigrateToV3()
        {
            return SaveMigration.Migrate(ResolveDbPath());
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
            var identityCreation = p.Identity == null ? D10PlayerIdentityCreation.Default(captainName) : p.Identity.Clone();
            if (string.IsNullOrWhiteSpace(identityCreation.Name)) identityCreation.Name = captainName;
            captainName = identityCreation.Name;
            var rng = new System.Random();

            EnsureSchemaAndSeed();

            // 新游戏 = 覆盖旧档：清空三张存档表（trait_defs / ship_type_defs 种子表保留）
            using (var db = OpenDb())
            {
                db.Execute("DELETE FROM players");
                db.Execute("DELETE FROM ships");
                db.Execute("DELETE FROM crew_agents");

                // D8-A05 迁移表属于同一 save.db；“新游戏 = 覆盖旧档”时不能保留旧存档的 v3 投影。
                // 旧 D7 数据库没有这些表，因此只在表存在时清理，保持 D7 首次启动兼容。
                if (db.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='v3_entities'").Count > 0)
                    db.Execute("DELETE FROM v3_entities");
                if (db.Query("SELECT name FROM sqlite_master WHERE type='table' AND name='save_metadata'").Count > 0)
                    db.Execute("DELETE FROM save_metadata");
            }

            // --- 生成实体（全部数值初始化为默认值，后续由各系统裁决修改）---
            var player = PlayerState.CreateNew(NewId("player"), captainName, identityCreation);
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

        /// <summary>
        /// D5 快照的唯一落库入口。快照作为 location_json.d5 保存，旧数据库无需迁移；
        /// Save 域只复制内容，不计算世界、特质、性能或成就结果。
        /// </summary>
        public static void SaveD5Runtime(D5RuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D5 快照。");
            GameState.CurrentD5Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D5Runtime = GameState.CurrentD5Runtime == null
                ? null : GameState.CurrentD5Runtime.Clone();
            Save();
        }

        /// <summary>D9 任务快照的唯一落库入口；D3 EventLog 仍随 D3 快照保存。</summary>
        public static void SaveD9Runtime(D9RuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D9 快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD9Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D9Runtime = GameState.CurrentD9Runtime == null
                ? null : GameState.CurrentD9Runtime.Clone();
            Save();
        }

        /// <summary>D10 关系快照的唯一落库入口；关系事实日志仍由共享 EventLog 保存。</summary>
        public static void SaveD10Runtime(D10RelationRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D10 关系快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD10Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D10Runtime = GameState.CurrentD10Runtime == null
                ? null : GameState.CurrentD10Runtime.Clone();
            Save();
        }

        /// <summary>D12 海图发现快照的唯一落库入口；探索系统先裁决，Save 域只复制快照。</summary>
        public static void SaveD12Runtime(D12RuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D12 海图快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD12Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D12Runtime = GameState.CurrentD12Runtime == null
                ? null : GameState.CurrentD12Runtime.Clone();
            Save();
        }

        /// <summary>D12 牌局快照的唯一落库入口；牌局系统先裁决，Save 域只复制事实与 owner 请求。</summary>
        public static void SaveD12PokerRuntime(D12PokerRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D12 牌局快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD12PokerRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D12PokerRuntime = GameState.CurrentD12PokerRuntime == null
                ? null : GameState.CurrentD12PokerRuntime.Clone();
            Save();
        }

        /// <summary>D11 鲸豚路线快照的唯一落库入口；不结算生态、法律、船员或奖励。</summary>
        public static void SaveD11Runtime(D11WhaleDolphinRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D11 鲸豚路线快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD11Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D11Runtime = GameState.CurrentD11Runtime == null
                ? null : GameState.CurrentD11Runtime.Clone();
            Save();
        }

        /// <summary>D11 Pet 快照的唯一落库入口；不计算宠物健康、照料或走失结果。</summary>
        public static void SaveD11PetRuntime(D11PetRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D11 Pet 快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD11PetRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D11PetRuntime = GameState.CurrentD11PetRuntime == null
                ? null : GameState.CurrentD11PetRuntime.Clone();
            Save();
        }

        /// <summary>D13 产权快照的唯一落库入口；PropertySystem 先裁决，Save 域只复制快照。</summary>
        public static void SaveD13Runtime(D13PropertyRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 产权快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13Runtime = GameState.CurrentD13Runtime == null
                ? null : GameState.CurrentD13Runtime.Clone();
            Save();
        }

        /// <summary>D13-A02 金融快照的唯一落库入口；FinanceSystem 先裁决，Save 域只复制快照。</summary>
        public static void SaveD13FinanceRuntime(D13FinanceRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 金融快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13FinanceRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13FinanceRuntime = GameState.CurrentD13FinanceRuntime == null
                ? null : GameState.CurrentD13FinanceRuntime.Clone();
            Save();
        }

        /// <summary>D13-A03 农业快照的唯一落库入口；Farming/Ecology/Inventory 先裁决，Save 只复制快照。</summary>
        public static void SaveD13FarmingRuntime(D13FarmingRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 农业快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13FarmingRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13FarmingRuntime = GameState.CurrentD13FarmingRuntime == null
                ? null : GameState.CurrentD13FarmingRuntime.Clone();
            Save();
        }

        /// <summary>D13-A04 牧场快照的唯一落库入口；Ranch/Ecology/Inventory 先裁决，Save 只复制快照。</summary>
        public static void SaveD13RanchRuntime(D13RanchRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 牧场快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13RanchRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13RanchRuntime = GameState.CurrentD13RanchRuntime == null
                ? null : GameState.CurrentD13RanchRuntime.Clone();
            Save();
        }

        /// <summary>D13-A05 治理快照的唯一落库入口；治理 authority 先裁决，Save 域只复制快照。</summary>
        public static void SaveD13GovernanceRuntime(D13GovernanceRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 岛屿治理快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13GovernanceRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13GovernanceRuntime = GameState.CurrentD13GovernanceRuntime == null
                ? null : GameState.CurrentD13GovernanceRuntime.Clone();
            Save();
        }

        /// <summary>D13-A06 建筑快照的唯一落库入口；Building/Inventory/Economy authority 先裁决，Save 只复制快照。</summary>
        public static void SaveD13BuildingRuntime(D13BuildingRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 建筑快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13BuildingRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13BuildingRuntime = GameState.CurrentD13BuildingRuntime == null
                ? null : GameState.CurrentD13BuildingRuntime.Clone();
            Save();
        }

        /// <summary>D13-A07 商品流/运输快照的唯一落库入口；价格、市场和库存仍由所属系统写入。</summary>
        public static void SaveD13DynamicMarketRuntime(D13DynamicMarketRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D13 动态市场快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD13DynamicMarketRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D13DynamicMarketRuntime = GameState.CurrentD13DynamicMarketRuntime == null
                ? null : GameState.CurrentD13DynamicMarketRuntime.Clone();
            Save();
        }

        /// <summary>D14-A01 海战快照的唯一落库入口；CombatSystem 先裁决，Save 域只复制状态和共享事件快照。</summary>
        public static void SaveD14Runtime(D14NavalCombatRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 海战快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14Runtime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14Runtime = GameState.CurrentD14Runtime == null
                ? null : GameState.CurrentD14Runtime.Clone();
            Save();
        }

        /// <summary>D14-A02 战后快照的唯一落库入口；Aftercare/所属 authority 先裁决，Save 只复制结果。</summary>
        public static void SaveD14AftercareRuntime(D14AftercareRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 战后快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14AftercareRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14AftercareRuntime = GameState.CurrentD14AftercareRuntime == null
                ? null : GameState.CurrentD14AftercareRuntime.Clone();
            Save();
        }

        /// <summary>D14-A03 特殊装备快照的唯一落库入口；Save 域只复制 authority 结果。</summary>
        public static void SaveD14SpecialEquipmentRuntime(D14SpecialEquipmentRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 特殊装备快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14SpecialEquipmentRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14SpecialEquipmentRuntime = GameState.CurrentD14SpecialEquipmentRuntime == null
                ? null : GameState.CurrentD14SpecialEquipmentRuntime.Clone();
            Save();
        }

        /// <summary>D14-A04 奖励快照的唯一落库入口；RewardSystem 先裁决，Save 只复制结果。</summary>
        public static void SaveD14RewardRuntime(D14RewardRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 奖励快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14RewardRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14RewardRuntime = GameState.CurrentD14RewardRuntime == null
                ? null : GameState.CurrentD14RewardRuntime.Clone();
            Save();
        }

        /// <summary>D14-A05 势力/法律快照的唯一落库入口；FactionLegalSystem 先裁决，Save 只复制快照。</summary>
        public static void SaveD14FactionLegalRuntime(D14FactionLegalRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 势力/法律快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14FactionLegalRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14FactionLegalRuntime = GameState.CurrentD14FactionLegalRuntime == null
                ? null : GameState.CurrentD14FactionLegalRuntime.Clone();
            Save();
        }

        /// <summary>D14-A06 宗教/社群/历史审查快照的唯一落库入口；Save 只复制所属系统结果。</summary>
        public static void SaveD14ReligionCommunityRuntime(D14ReligionCommunityRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14 宗教/社群快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14ReligionCommunityRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14ReligionCommunityRuntime = GameState.CurrentD14ReligionCommunityRuntime == null
                ? null : GameState.CurrentD14ReligionCommunityRuntime.Clone();
            Save();
        }

        /// <summary>D14-S01 集成账本的唯一落库入口；不裁决任何领域结果。</summary>
        public static void SaveD14S01IntegrationRuntime(D14S01IntegrationRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D14-S01 集成快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD14S01IntegrationRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D14S01IntegrationRuntime = GameState.CurrentD14S01IntegrationRuntime == null
                ? null : GameState.CurrentD14S01IntegrationRuntime.Clone();
            Save();
        }

        /// <summary>D11-A03 水产快照的唯一落库入口；不裁决鱼货、死亡或生态压力。</summary>
        public static void SaveD11AquacultureRuntime(D11AquacultureRuntimeSnapshot snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D11 水产快照。");
            if (snapshot != null) snapshot.Validate();
            GameState.CurrentD11AquacultureRuntime = snapshot == null ? null : snapshot.Clone();
            GameState.CurrentPlayer.D11AquacultureRuntime = GameState.CurrentD11AquacultureRuntime == null
                ? null : GameState.CurrentD11AquacultureRuntime.Clone();
            Save();
        }

        /// <summary>
        /// D10-A01 canonical 关系图的唯一落库接线。数值仍由 RelationSystem 先裁决，
        /// 本方法只把图快照放入现有 D10 运行时容器。
        /// </summary>
        public static void SaveD10RelationGraph(RelationGraphSnapshot graph)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D10 关系图。");
            if (graph != null) graph.Validate();
            var runtime = GameState.CurrentD10Runtime == null
                ? (GameState.CurrentPlayer.D10Runtime == null ? new D10RelationRuntimeSnapshot() : GameState.CurrentPlayer.D10Runtime.Clone())
                : GameState.CurrentD10Runtime.Clone();
            runtime.CanonicalGraph = graph == null ? null : graph.Clone();
            SaveD10Runtime(runtime);
        }

        /// <summary>读取 D10-A01 canonical 关系图副本；旧存档没有时返回 null。</summary>
        public static RelationGraphSnapshot LoadD10RelationGraph()
        {
            var runtime = LoadD10Runtime();
            return runtime == null || runtime.CanonicalGraph == null ? null : runtime.CanonicalGraph.Clone();
        }

        /// <summary>D10 玩家身份快照的唯一落库入口；不修改法律、债务、声望或关系结果。</summary>
        public static void SaveD10Identity(D10PlayerIdentityState snapshot)
        {
            if (GameState.CurrentPlayer == null)
                throw new InvalidOperationException("[SaveSystem] 无进行中的游戏，无法保存 D10 玩家身份。");
            if (snapshot == null)
                throw new InvalidOperationException("[SaveSystem] D10 玩家身份快照不能为空。");
            if (!string.Equals(snapshot.PlayerId, GameState.CurrentPlayer.Id, StringComparison.Ordinal))
                throw new InvalidOperationException("[SaveSystem] D10 玩家身份不属于当前玩家，拒绝写入。");
            snapshot.Validate();
            GameState.CurrentPlayer.Identity = snapshot.Clone();
            GameState.CurrentPlayer.Name = GameState.CurrentPlayer.Identity.Name;
            Save();
        }

        /// <summary>读取当前已加载的 D3 快照副本；旧 D2 存档没有快照时返回 null。</summary>
        public static D3RuntimeSnapshot LoadD3Runtime()
        {
            var snapshot = GameState.CurrentD3Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D3Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>
        /// 从已加载的 D3 存档恢复交易事实日志；恢复过程会校验 EventLog 版本、序号、
        /// 重复键和 settlement claim，调用方再把同一只日志绑定给领域 owner/表现层。
        /// </summary>
        public static EventLog RestoreD3TradeEventLog()
        {
            var snapshot = LoadD3Runtime();
            return snapshot == null || snapshot.EventLog == null
                ? new EventLog()
                : EventLog.FromSnapshot(snapshot.EventLog);
        }

        /// <summary>保存前复制交易 owner 提供的事实日志快照；不取得领域状态写权限。</summary>
        public static EventLogSnapshot CaptureD3TradeEventLog(EventLog eventLog)
        {
            return eventLog == null ? null : eventLog.CaptureSnapshot();
        }

        /// <summary>
        /// SaveSystem 唯一写入 D9 保存事实的入口。先追加 checkpoint，再由调用方把
        /// 同一 EventLogSnapshot 随 D3 快照落库，读档时不会凭空生成第二份日志。
        /// </summary>
        public static FactEvent AppendD9SaveCheckpoint(EventLog eventLog, int worldDay,
            string locationRef, long saveRevision)
        {
            if (eventLog == null) return null;
            var result = eventLog.Append(new WorldEvent(
                "event_d9_save_checkpoint",
                Math.Max(0, worldDay),
                string.IsNullOrEmpty(locationRef) ? PortCatalog.StartingPortId : locationRef,
                "SaveSystem",
                new EventResult
                {
                    ResultCode = "save_checkpoint_written",
                    Summary = "D9 航程关键状态已写入统一本地存档。",
                    FactIds = new List<string> { "save:checkpoint" },
                    Values = new Dictionary<string, string>
                    {
                        ["save_revision"] = Math.Max(0L, saveRevision).ToString(),
                        ["event_count"] = eventLog.Count.ToString(),
                    },
                },
                new[] { "player" },
                new[] { "save:runtime" },
                new string[0],
                Math.Max(0L, saveRevision)));
            return result.Fact;
        }

        /// <summary>读取当前已加载的 D4 快照副本；D1-D3 旧存档没有该字段时返回 null。</summary>
        public static D4RuntimeSnapshot LoadD4Runtime()
        {
            var snapshot = GameState.CurrentD4Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D4Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D5 快照副本；D1-D4 旧存档没有该字段时返回 null。</summary>
        public static D5RuntimeSnapshot LoadD5Runtime()
        {
            var snapshot = GameState.CurrentD5Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D5Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D9 任务快照；旧存档没有时返回 null。</summary>
        public static D9RuntimeSnapshot LoadD9Runtime()
        {
            var snapshot = GameState.CurrentD9Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D9Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D10 关系快照；旧存档没有时返回 null。</summary>
        public static D10RelationRuntimeSnapshot LoadD10Runtime()
        {
            var snapshot = GameState.CurrentD10Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D10Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D12 海图发现快照；旧存档没有时返回 null。</summary>
        public static D12RuntimeSnapshot LoadD12Runtime()
        {
            var snapshot = GameState.CurrentD12Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D12Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D12 牌局快照；旧存档没有时返回 null。</summary>
        public static D12PokerRuntimeSnapshot LoadD12PokerRuntime()
        {
            var snapshot = GameState.CurrentD12PokerRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D12PokerRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D11 鲸豚路线副本；旧存档没有时返回 null。</summary>
        public static D11WhaleDolphinRuntimeSnapshot LoadD11Runtime()
        {
            var snapshot = GameState.CurrentD11Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D11Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D11 Pet 快照；旧存档没有时返回 null。</summary>
        public static D11PetRuntimeSnapshot LoadD11PetRuntime()
        {
            var snapshot = GameState.CurrentD11PetRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D11PetRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D11-A03 水产快照；旧存档没有时返回 null。</summary>
        public static D11AquacultureRuntimeSnapshot LoadD11AquacultureRuntime()
        {
            var snapshot = GameState.CurrentD11AquacultureRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D11AquacultureRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13 产权快照；旧存档没有时返回 null。</summary>
        public static D13PropertyRuntimeSnapshot LoadD13Runtime()
        {
            var snapshot = GameState.CurrentD13Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13 金融快照；旧存档没有时返回 null。</summary>
        public static D13FinanceRuntimeSnapshot LoadD13FinanceRuntime()
        {
            var snapshot = GameState.CurrentD13FinanceRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13FinanceRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13 农业快照；旧存档没有时返回 null。</summary>
        public static D13FarmingRuntimeSnapshot LoadD13FarmingRuntime()
        {
            var snapshot = GameState.CurrentD13FarmingRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13FarmingRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13 牧场快照；旧存档没有时返回 null。</summary>
        public static D13RanchRuntimeSnapshot LoadD13RanchRuntime()
        {
            var snapshot = GameState.CurrentD13RanchRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13RanchRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13-A05 治理快照；旧存档没有时返回 null。</summary>
        public static D13GovernanceRuntimeSnapshot LoadD13GovernanceRuntime()
        {
            var snapshot = GameState.CurrentD13GovernanceRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13GovernanceRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13-A06 建筑快照；旧存档没有时返回 null。</summary>
        public static D13BuildingRuntimeSnapshot LoadD13BuildingRuntime()
        {
            var snapshot = GameState.CurrentD13BuildingRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13BuildingRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D13-A07 商品流/运输快照；旧存档没有时返回 null。</summary>
        public static D13DynamicMarketRuntimeSnapshot LoadD13DynamicMarketRuntime()
        {
            var snapshot = GameState.CurrentD13DynamicMarketRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D13DynamicMarketRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14 海战快照；旧存档没有时返回 null。</summary>
        public static D14NavalCombatRuntimeSnapshot LoadD14Runtime()
        {
            var snapshot = GameState.CurrentD14Runtime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14Runtime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-A02 战后快照；旧存档没有时返回 null。</summary>
        public static D14AftercareRuntimeSnapshot LoadD14AftercareRuntime()
        {
            var snapshot = GameState.CurrentD14AftercareRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14AftercareRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-A03 特殊装备快照；旧存档没有时返回 null。</summary>
        public static D14SpecialEquipmentRuntimeSnapshot LoadD14SpecialEquipmentRuntime()
        {
            var snapshot = GameState.CurrentD14SpecialEquipmentRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14SpecialEquipmentRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-A04 奖励快照；旧存档没有时返回 null。</summary>
        public static D14RewardRuntimeSnapshot LoadD14RewardRuntime()
        {
            var snapshot = GameState.CurrentD14RewardRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14RewardRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-A05 势力/法律快照；旧存档没有时返回 null。</summary>
        public static D14FactionLegalRuntimeSnapshot LoadD14FactionLegalRuntime()
        {
            var snapshot = GameState.CurrentD14FactionLegalRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14FactionLegalRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-A06 宗教/社群/历史审查快照；旧存档没有时返回 null。</summary>
        public static D14ReligionCommunityRuntimeSnapshot LoadD14ReligionCommunityRuntime()
        {
            var snapshot = GameState.CurrentD14ReligionCommunityRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14ReligionCommunityRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D14-S01 集成账本；旧存档没有时返回 null。</summary>
        public static D14S01IntegrationRuntimeSnapshot LoadD14S01IntegrationRuntime()
        {
            var snapshot = GameState.CurrentD14S01IntegrationRuntime ??
                (GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.D14S01IntegrationRuntime);
            return snapshot == null ? null : snapshot.Clone();
        }

        /// <summary>读取当前已加载的 D10 玩家身份；D7/D8 旧档按姓名生成安全默认值。</summary>
        public static D10PlayerIdentityState LoadD10Identity()
        {
            if (GameState.CurrentPlayer == null) return null;
            var identity = GameState.CurrentPlayer.Identity;
            return identity == null
                ? D10PlayerIdentityState.CreateDefault(GameState.CurrentPlayer.Id, GameState.CurrentPlayer.Name)
                : identity.Clone();
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
                    GameState.CurrentD5Runtime = player.D5Runtime == null ? null : player.D5Runtime.Clone();
                    GameState.CurrentD9Runtime = player.D9Runtime == null ? null : player.D9Runtime.Clone();
                    GameState.CurrentD10Runtime = player.D10Runtime == null ? null : player.D10Runtime.Clone();
                    GameState.CurrentD12Runtime = player.D12Runtime == null ? null : player.D12Runtime.Clone();
                    GameState.CurrentD12PokerRuntime = player.D12PokerRuntime == null ? null : player.D12PokerRuntime.Clone();
                    GameState.CurrentD11Runtime = player.D11Runtime == null ? null : player.D11Runtime.Clone();
                    GameState.CurrentD11AquacultureRuntime = player.D11AquacultureRuntime == null
                        ? null : player.D11AquacultureRuntime.Clone();
                    GameState.CurrentD11PetRuntime = player.D11PetRuntime == null ? null : player.D11PetRuntime.Clone();
                    GameState.CurrentD13Runtime = player.D13Runtime == null ? null : player.D13Runtime.Clone();
                    GameState.CurrentD13FinanceRuntime = player.D13FinanceRuntime == null ? null : player.D13FinanceRuntime.Clone();
                    GameState.CurrentD13FarmingRuntime = player.D13FarmingRuntime == null ? null : player.D13FarmingRuntime.Clone();
                    GameState.CurrentD13RanchRuntime = player.D13RanchRuntime == null ? null : player.D13RanchRuntime.Clone();
                    GameState.CurrentD13GovernanceRuntime = player.D13GovernanceRuntime == null ? null : player.D13GovernanceRuntime.Clone();
                    GameState.CurrentD13BuildingRuntime = player.D13BuildingRuntime == null ? null : player.D13BuildingRuntime.Clone();
                    GameState.CurrentD13DynamicMarketRuntime = player.D13DynamicMarketRuntime == null ? null : player.D13DynamicMarketRuntime.Clone();
                    GameState.CurrentD14Runtime = player.D14Runtime == null ? null : player.D14Runtime.Clone();
                    GameState.CurrentD14AftercareRuntime = player.D14AftercareRuntime == null ? null : player.D14AftercareRuntime.Clone();
                    GameState.CurrentD14SpecialEquipmentRuntime = player.D14SpecialEquipmentRuntime == null ? null : player.D14SpecialEquipmentRuntime.Clone();
                    GameState.CurrentD14RewardRuntime = player.D14RewardRuntime == null ? null : player.D14RewardRuntime.Clone();
                    GameState.CurrentD14FactionLegalRuntime = player.D14FactionLegalRuntime == null ? null : player.D14FactionLegalRuntime.Clone();
                    GameState.CurrentD14ReligionCommunityRuntime = player.D14ReligionCommunityRuntime == null ? null : player.D14ReligionCommunityRuntime.Clone();
                    GameState.CurrentD14S01IntegrationRuntime = player.D14S01IntegrationRuntime == null ? null : player.D14S01IntegrationRuntime.Clone();
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
            if (p.Identity == null)
                p.Identity = D10PlayerIdentityState.CreateDefault(p.Id, p.Name);
            if (!string.Equals(p.Identity.PlayerId, p.Id, StringComparison.Ordinal))
                throw new InvalidOperationException("[SaveSystem] Player identity owner mismatch, refusing to save.");
            p.Identity.Validate();
            p.Name = p.Identity.Name;
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
                    SaveJson.Stringify(LocationToObj(p.Location, p.D3Runtime, p.D4Runtime, p.D5Runtime, p.D9Runtime, p.D10Runtime, p.D11Runtime, p.Identity, p.D11PetRuntime, p.D11AquacultureRuntime, p.D12Runtime, p.D12PokerRuntime, p.D13Runtime, p.D13FinanceRuntime, p.D13FarmingRuntime, p.D13RanchRuntime, p.D13GovernanceRuntime, p.D13BuildingRuntime, p.D13DynamicMarketRuntime, p.D14Runtime, p.D14AftercareRuntime, p.D14SpecialEquipmentRuntime, p.D14RewardRuntime, p.D14FactionLegalRuntime, p.D14ReligionCommunityRuntime, p.D14S01IntegrationRuntime)),
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
                p.Location.NormalizedTime = SaveJson.GetFloat(
                    loc, "normalizedTime", (float)NumericConfiguration.Default.TimeOfDay.DefaultValue);
                if (p.Location.NormalizedTime < 0f || p.Location.NormalizedTime >= 1f)
                {
                    p.Location.NormalizedTime %= 1f;
                    if (p.Location.NormalizedTime < 0f) p.Location.NormalizedTime += 1f;
                }
                if (loc.TryGetValue("d3", out object d3Value) && d3Value is Dictionary<string, object> d3Object)
                    p.D3Runtime = D3FromObj(d3Object);
                if (loc.TryGetValue("d4", out object d4Value) && d4Value is Dictionary<string, object> d4Object)
                    p.D4Runtime = D4FromObj(d4Object);
                if (loc.TryGetValue("d5", out object d5Value) && d5Value is Dictionary<string, object> d5Object)
                    p.D5Runtime = D5FromObj(d5Object);
                if (loc.TryGetValue("d9", out object d9Value) && d9Value is Dictionary<string, object> d9Object)
                    p.D9Runtime = D9FromObj(d9Object);
                if (loc.TryGetValue("d10", out object d10Value) && d10Value is Dictionary<string, object> d10Object)
                    p.D10Runtime = D10FromObj(d10Object);
                if (loc.TryGetValue("d12", out object d12Value) && d12Value is Dictionary<string, object> d12Object)
                    p.D12Runtime = D12FromObj(d12Object);
                if (loc.TryGetValue("d12Poker", out object d12PokerValue) && d12PokerValue is Dictionary<string, object> d12PokerObject)
                    p.D12PokerRuntime = D12PokerFromObj(d12PokerObject);
                if (loc.TryGetValue("d13", out object d13Value) && d13Value is Dictionary<string, object> d13Object)
                    p.D13Runtime = D13FromObj(d13Object);
                if (loc.TryGetValue("d13Finance", out object d13FinanceValue) && d13FinanceValue is Dictionary<string, object> d13FinanceObject)
                    p.D13FinanceRuntime = D13FinanceFromObj(d13FinanceObject);
                if (loc.TryGetValue("d13Farming", out object d13FarmingValue) && d13FarmingValue is Dictionary<string, object> d13FarmingObject)
                    p.D13FarmingRuntime = D13FarmingFromObj(d13FarmingObject);
                if (loc.TryGetValue("d13Ranch", out object d13RanchValue) && d13RanchValue is Dictionary<string, object> d13RanchObject)
                    p.D13RanchRuntime = D13RanchFromObj(d13RanchObject);
                if (loc.TryGetValue("d13Governance", out object d13GovernanceValue) && d13GovernanceValue is Dictionary<string, object> d13GovernanceObject)
                    p.D13GovernanceRuntime = D13GovernanceFromObj(d13GovernanceObject);
                if (loc.TryGetValue("d13Buildings", out object d13BuildingsValue) && d13BuildingsValue is Dictionary<string, object> d13BuildingsObject)
                    p.D13BuildingRuntime = D13BuildingFromObj(d13BuildingsObject);
                if (loc.TryGetValue("d13DynamicMarket", out object d13DynamicMarketValue) && d13DynamicMarketValue is Dictionary<string, object> d13DynamicMarketObject)
                    p.D13DynamicMarketRuntime = D13DynamicMarketFromObj(d13DynamicMarketObject);
                if (loc.TryGetValue("d14", out object d14Value) && d14Value is Dictionary<string, object> d14Object)
                    p.D14Runtime = D14FromObj(d14Object);
                if (loc.TryGetValue("d14Aftercare", out object d14AftercareValue) && d14AftercareValue is Dictionary<string, object> d14AftercareObject)
                    p.D14AftercareRuntime = D14AftercareFromObj(d14AftercareObject);
                if (loc.TryGetValue("d14SpecialEquipment", out object d14SpecialEquipmentValue) && d14SpecialEquipmentValue is Dictionary<string, object> d14SpecialEquipmentObject)
                    p.D14SpecialEquipmentRuntime = D14SpecialEquipmentFromObj(d14SpecialEquipmentObject);
                if (loc.TryGetValue("d14Reward", out object d14RewardValue) && d14RewardValue is Dictionary<string, object> d14RewardObject)
                    p.D14RewardRuntime = D14RewardFromObj(d14RewardObject);
                if (loc.TryGetValue("d14FactionLegal", out object d14FactionLegalValue) && d14FactionLegalValue is Dictionary<string, object> d14FactionLegalObject)
                    p.D14FactionLegalRuntime = D14FactionLegalFromObj(d14FactionLegalObject);
                if (loc.TryGetValue("d14ReligionCommunity", out object d14ReligionCommunityValue) && d14ReligionCommunityValue is Dictionary<string, object> d14ReligionCommunityObject)
                    p.D14ReligionCommunityRuntime = D14ReligionCommunityFromObj(d14ReligionCommunityObject);
                if (loc.TryGetValue("d14S01Integration", out object d14S01IntegrationValue) && d14S01IntegrationValue is Dictionary<string, object> d14S01IntegrationObject)
                    p.D14S01IntegrationRuntime = D14S01IntegrationFromObj(d14S01IntegrationObject);
                if (loc.TryGetValue("d11", out object d11Value) && d11Value is Dictionary<string, object> d11Object)
                    p.D11Runtime = D11FromObj(d11Object);
                if (loc.TryGetValue("d11Aquaculture", out object d11AquacultureValue) && d11AquacultureValue is Dictionary<string, object> d11AquacultureObject)
                    p.D11AquacultureRuntime = D11AquacultureFromObj(d11AquacultureObject);
                if (loc.TryGetValue("d11Pet", out object d11PetValue) && d11PetValue is Dictionary<string, object> d11PetObject)
                    p.D11PetRuntime = D11PetFromObj(d11PetObject);
                if (loc.TryGetValue("identity", out object identityValue) && identityValue is Dictionary<string, object> identityObject)
                    p.Identity = D10IdentityFromObj(identityObject);
            }

            if (p.Identity == null)
                p.Identity = D10PlayerIdentityState.CreateDefault(p.Id, p.Name);
            p.Name = p.Identity.Name;

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

        private static Dictionary<string, object> LocationToObj(LocationRef loc, D3RuntimeSnapshot d3 = null,
            D4RuntimeSnapshot d4 = null, D5RuntimeSnapshot d5 = null, D9RuntimeSnapshot d9 = null,
            D10RelationRuntimeSnapshot d10 = null, D11WhaleDolphinRuntimeSnapshot d11 = null,
            D10PlayerIdentityState identity = null, D11PetRuntimeSnapshot d11Pet = null,
            D11AquacultureRuntimeSnapshot d11Aquaculture = null, D12RuntimeSnapshot d12 = null,
            D12PokerRuntimeSnapshot d12Poker = null, D13PropertyRuntimeSnapshot d13 = null,
            D13FinanceRuntimeSnapshot d13Finance = null, D13FarmingRuntimeSnapshot d13Farming = null,
            D13RanchRuntimeSnapshot d13Ranch = null, D13GovernanceRuntimeSnapshot d13Governance = null, D13BuildingRuntimeSnapshot d13Buildings = null, D13DynamicMarketRuntimeSnapshot d13DynamicMarket = null, D14NavalCombatRuntimeSnapshot d14 = null, D14AftercareRuntimeSnapshot d14Aftercare = null, D14SpecialEquipmentRuntimeSnapshot d14SpecialEquipment = null, D14RewardRuntimeSnapshot d14Reward = null, D14FactionLegalRuntimeSnapshot d14FactionLegal = null, D14ReligionCommunityRuntimeSnapshot d14ReligionCommunity = null, D14S01IntegrationRuntimeSnapshot d14S01Integration = null)
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
            if (d5 != null) result["d5"] = D5ToObj(d5);
            if (d9 != null) result["d9"] = D9ToObj(d9);
            if (d10 != null) result["d10"] = D10ToObj(d10);
            if (d11 != null) result["d11"] = D11ToObj(d11);
            if (identity != null) result["identity"] = D10IdentityToObj(identity);
            if (d11Pet != null) result["d11Pet"] = D11PetToObj(d11Pet);
            if (d11Aquaculture != null) result["d11Aquaculture"] = D11AquacultureToObj(d11Aquaculture);
            if (d12 != null) result["d12"] = D12ToObj(d12);
            if (d12Poker != null) result["d12Poker"] = D12PokerToObj(d12Poker);
            if (d13 != null) result["d13"] = D13ToObj(d13);
            if (d13Finance != null) result["d13Finance"] = D13FinanceToObj(d13Finance);
            if (d13Farming != null) result["d13Farming"] = D13FarmingToObj(d13Farming);
            if (d13Ranch != null) result["d13Ranch"] = D13RanchToObj(d13Ranch);
            if (d13Governance != null) result["d13Governance"] = D13GovernanceToObj(d13Governance);
            if (d13Buildings != null) result["d13Buildings"] = D13BuildingToObj(d13Buildings);
            if (d13DynamicMarket != null) result["d13DynamicMarket"] = D13DynamicMarketToObj(d13DynamicMarket);
            if (d14 != null) result["d14"] = D14ToObj(d14);
            if (d14Aftercare != null) result["d14Aftercare"] = D14AftercareToObj(d14Aftercare);
            if (d14SpecialEquipment != null) result["d14SpecialEquipment"] = D14SpecialEquipmentToObj(d14SpecialEquipment);
            if (d14Reward != null) result["d14Reward"] = D14RewardToObj(d14Reward);
            if (d14FactionLegal != null) result["d14FactionLegal"] = D14FactionLegalToObj(d14FactionLegal);
            if (d14ReligionCommunity != null) result["d14ReligionCommunity"] = D14ReligionCommunityToObj(d14ReligionCommunity);
            if (d14S01Integration != null) result["d14S01Integration"] = D14S01IntegrationToObj(d14S01Integration);
            return result;
        }

        private static Dictionary<string, object> D3ToObj(D3RuntimeSnapshot d3)
        {
            var result = new Dictionary<string, object>
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
                ["saveRevision"] = d3.SaveRevision,
                ["cargo"] = d3.Cargo,
                ["personalItems"] = d3.PersonalItems,
                ["marketStock"] = d3.MarketStock,
                ["marketDemand"] = d3.MarketDemand,
                ["playerTradeVolume"] = d3.PlayerTradeVolume,
                ["factionPolicyId"] = d3.FactionPolicyId ?? "neutral",
                ["factionPolicyCoefficient"] = d3.FactionPolicyCoefficient,
                ["gatheredThisVisit"] = d3.GatheredThisVisit,
                ["eventIds"] = d3.EventIds,
            };
            if (d3.WeatherSea != null) result["weatherSea"] = WeatherSeaToObj(d3.WeatherSea);
            if (d3.EventLog != null) result["eventLog"] = EventLogToObj(d3.EventLog);
            return result;
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
                SaveRevision = ReadLong(obj, "saveRevision"),
                FactionPolicyId = SaveJson.GetStr(obj, "factionPolicyId", "neutral"),
                FactionPolicyCoefficient = SaveJson.GetFloat(obj, "factionPolicyCoefficient", 1f),
            };
            if (Enum.TryParse(SaveJson.GetStr(obj, "voyageStatus", "InPort"), true, out D3VoyageStatus status))
                result.VoyageStatus = status;
            if (Enum.TryParse(SaveJson.GetStr(obj, "weather", "Calm"), true, out D3WeatherType weather))
                result.Weather = weather;
            if (obj.TryGetValue("weatherSea", out object weatherSeaValue) && weatherSeaValue is Dictionary<string, object> weatherSeaObject)
                result.WeatherSea = WeatherSeaFromObj(weatherSeaObject);
            ReadIntDictObject(result.Cargo, obj, "cargo");
            ReadIntDictObject(result.PersonalItems, obj, "personalItems");
            ReadIntDictObject(result.MarketStock, obj, "marketStock");
            ReadIntDictObject(result.MarketDemand, obj, "marketDemand");
            ReadIntDictObject(result.PlayerTradeVolume, obj, "playerTradeVolume");
            ReadIntDictObject(result.GatheredThisVisit, obj, "gatheredThisVisit");
            ReadStringListObject(result.EventIds, obj, "eventIds");
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            return result;
        }

        private static Dictionary<string, object> WeatherSeaToObj(D9WeatherSeaSnapshot weather)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = weather.SchemaVersion,
                ["worldDay"] = weather.WorldDay,
                ["revision"] = weather.Revision,
                ["weather"] = weather.Weather.ToString(),
                ["windSpeed"] = weather.WindSpeed,
                ["windDirectionDegrees"] = weather.WindDirectionDegrees,
                ["visibility"] = weather.Visibility,
                ["currentSpeed"] = weather.CurrentSpeed,
                ["currentDirectionDegrees"] = weather.CurrentDirectionDegrees,
                ["crewFatiguePerDay"] = weather.CrewFatiguePerDay,
                ["hullRisk"] = weather.HullRisk,
                ["fishActivityMultiplier"] = weather.FishActivityMultiplier,
                ["fishCatchMultiplier"] = weather.FishCatchMultiplier,
                ["locationRef"] = weather.LocationRef ?? "nearshore_saint_marys",
                ["lastEventId"] = weather.LastEventId ?? "",
                ["appliedEventIds"] = weather.AppliedEventIds ?? new List<string>(),
            };
        }

        private static D9WeatherSeaSnapshot WeatherSeaFromObj(Dictionary<string, object> obj)
        {
            var result = D9WeatherSeaSnapshot.CreateDefault();
            result.SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D9WeatherSeaSnapshot.CurrentSchemaVersion);
            result.WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay", 0));
            result.Revision = Math.Max(0, SaveJson.GetInt(obj, "revision", 0));
            string weatherText = SaveJson.GetStr(obj, "weather", "Calm");
            if (!Enum.TryParse(weatherText, true, out D9WeatherState weather))
                throw new InvalidOperationException("未知 D9 天气状态：" + weatherText + "。");
            var profile = D9WeatherProfiles.Get(weather);
            result.Weather = weather;
            result.WindSpeed = SaveJson.GetFloat(obj, "windSpeed", profile.WindSpeed);
            result.WindDirectionDegrees = SaveJson.GetFloat(obj, "windDirectionDegrees", profile.WindDirectionDegrees);
            result.Visibility = SaveJson.GetFloat(obj, "visibility", profile.Visibility);
            result.CurrentSpeed = SaveJson.GetFloat(obj, "currentSpeed", profile.CurrentSpeed);
            result.CurrentDirectionDegrees = SaveJson.GetFloat(obj, "currentDirectionDegrees", profile.CurrentDirectionDegrees);
            result.CrewFatiguePerDay = SaveJson.GetInt(obj, "crewFatiguePerDay", profile.CrewFatiguePerDay);
            result.HullRisk = SaveJson.GetFloat(obj, "hullRisk", profile.HullRisk);
            result.FishActivityMultiplier = SaveJson.GetFloat(obj, "fishActivityMultiplier", profile.FishActivityMultiplier);
            result.FishCatchMultiplier = SaveJson.GetFloat(obj, "fishCatchMultiplier", profile.FishCatchMultiplier);
            result.LocationRef = SaveJson.GetStr(obj, "locationRef", "nearshore_saint_marys");
            result.LastEventId = SaveJson.GetStr(obj, "lastEventId", "");
            ReadStringListObject(result.AppliedEventIds, obj, "appliedEventIds");
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> EventLogToObj(EventLogSnapshot snapshot)
        {
            var facts = new List<object>();
            if (snapshot.Facts != null)
                foreach (var fact in snapshot.Facts)
                    if (fact != null) facts.Add(FactToObj(fact));
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = snapshot.SchemaVersion,
                ["nextSequence"] = snapshot.NextSequence,
                ["settledKeys"] = StringListToObjects(snapshot.SettledKeys),
                ["facts"] = facts,
            };
        }

        private static Dictionary<string, object> FactToObj(FactEvent fact)
        {
            var result = new Dictionary<string, object>
            {
                ["eventId"] = fact.EventId ?? "",
                ["schemaVersion"] = fact.SchemaVersion,
                ["worldDay"] = fact.WorldDay,
                ["locationRef"] = fact.LocationRef ?? "",
                ["actorIds"] = StringListToObjects(fact.ActorIds),
                ["inputFacts"] = StringListToObjects(fact.InputFacts),
                ["ownerSystem"] = fact.OwnerSystem ?? "",
                ["result"] = ResultToObj(fact.Result),
                ["causeIds"] = StringListToObjects(fact.CauseIds),
                ["saveRevision"] = fact.SaveRevision,
                ["outcomeStatus"] = fact.OutcomeStatus.ToString(),
                ["logSequence"] = fact.LogSequence,
            };
            if (fact.RecoveryRoute != null)
                result["recoveryRoute"] = new Dictionary<string, object>
                {
                    ["routeId"] = fact.RecoveryRoute.RouteId ?? "",
                    ["action"] = fact.RecoveryRoute.Action ?? "",
                    ["resumeFromEventId"] = fact.RecoveryRoute.ResumeFromEventId ?? "",
                    ["description"] = fact.RecoveryRoute.Description ?? "",
                };
            return result;
        }

        private static Dictionary<string, object> ResultToObj(EventResult result)
        {
            var values = new Dictionary<string, object>();
            if (result != null && result.Values != null)
                foreach (var pair in result.Values) values[pair.Key] = pair.Value ?? "";
            return new Dictionary<string, object>
            {
                ["resultCode"] = result == null ? "completed" : result.ResultCode ?? "completed",
                ["summary"] = result == null ? "" : result.Summary ?? "",
                ["factIds"] = StringListToObjects(result == null ? null : result.FactIds),
                ["values"] = values,
            };
        }

        private static EventLogSnapshot EventLogFromObj(Dictionary<string, object> obj)
        {
            var snapshot = new EventLogSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", EventSchema.EventLogVersion),
                NextSequence = ReadLong(obj, "nextSequence"),
            };
            ReadStringListObject(snapshot.SettledKeys, obj, "settledKeys");
            foreach (var factObj in ObjectList(obj, "facts")) snapshot.Facts.Add(FactFromObj(factObj));
            return snapshot;
        }

        private static FactEvent FactFromObj(Dictionary<string, object> obj)
        {
            var fact = new FactEvent
            {
                EventId = SaveJson.GetStr(obj, "eventId", ""),
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", EventSchema.CurrentVersion),
                WorldDay = SaveJson.GetInt(obj, "worldDay"),
                LocationRef = SaveJson.GetStr(obj, "locationRef", ""),
                OwnerSystem = SaveJson.GetStr(obj, "ownerSystem", ""),
                SaveRevision = ReadLong(obj, "saveRevision"),
                LogSequence = ReadLong(obj, "logSequence"),
                Result = ResultFromObj(Nested(obj, "result")),
            };
            ReadStringListObject(fact.ActorIds, obj, "actorIds");
            ReadStringListObject(fact.InputFacts, obj, "inputFacts");
            ReadStringListObject(fact.CauseIds, obj, "causeIds");
            if (Enum.TryParse(SaveJson.GetStr(obj, "outcomeStatus", "Normal"), true, out WorldEventStatus status))
                fact.OutcomeStatus = status;
            var recovery = Nested(obj, "recoveryRoute");
            if (recovery != null)
                fact.RecoveryRoute = new RecoveryRoute
                {
                    RouteId = SaveJson.GetStr(recovery, "routeId", ""),
                    Action = SaveJson.GetStr(recovery, "action", ""),
                    ResumeFromEventId = SaveJson.GetStr(recovery, "resumeFromEventId", ""),
                    Description = SaveJson.GetStr(recovery, "description", ""),
                };
            return fact;
        }

        private static EventResult ResultFromObj(Dictionary<string, object> obj)
        {
            var result = new EventResult
            {
                ResultCode = SaveJson.GetStr(obj, "resultCode", "completed"),
                Summary = SaveJson.GetStr(obj, "summary", ""),
            };
            ReadStringListObject(result.FactIds, obj, "factIds");
            var values = Nested(obj, "values");
            if (values != null)
                foreach (var pair in values) result.Values[pair.Key] = Convert.ToString(pair.Value);
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
                    ["animalId"] = animal.AnimalId ?? "", ["habitatId"] = animal.HabitatId ?? "",
                    ["behavior"] = animal.Behavior.ToString(), ["health"] = animal.Health,
                    ["ecologyStress"] = animal.EcologyStress, ["lastUpdatedDay"] = animal.LastUpdatedDay,
                    ["observed"] = animal.Observed, ["collectionValue"] = animal.CollectionValue,
                    ["lifecycle"] = animal.Lifecycle.ToString(), ["welfare"] = animal.Welfare,
                    ["transportDays"] = animal.TransportDays, ["lastHandledDay"] = animal.LastHandledDay,
                    ["currentLocationRef"] = animal.CurrentLocationRef ?? "", ["sourceRef"] = animal.SourceRef ?? "",
                });
            var ecology = new Dictionary<string, object>
            {
                ["habitatId"] = d4.Ecology.HabitatId ?? "", ["animals"] = animals,
                ["collectionCount"] = d4.Ecology.CollectionCount,
                ["lastEcologyDay"] = d4.Ecology.LastEcologyDay,
                ["ecologyRevision"] = d4.Ecology.EcologyRevision,
                ["lastEcologyEventId"] = d4.Ecology.LastEcologyEventId ?? "",
                ["appliedEcologyEventKeys"] = StringListToObjects(d4.Ecology.AppliedEcologyEventKeys),
            };
            var habitat = d4.Ecology.Habitat ?? new HabitatConditionState();
            ecology["habitatCondition"] = new Dictionary<string, object>
            {
                ["habitatId"] = habitat.HabitatId ?? "nearshore_reef_edge",
                ["biomeId"] = habitat.BiomeId ?? "reef",
                ["temperatureC"] = habitat.TemperatureC,
                ["turbidity"] = habitat.Turbidity,
                ["pollution"] = habitat.Pollution,
                ["tidePhase"] = habitat.TidePhase,
                ["currentSpeed"] = habitat.CurrentSpeed,
                ["currentDirectionDegrees"] = habitat.CurrentDirectionDegrees,
                ["lastUpdatedDay"] = habitat.LastUpdatedDay,
                ["revision"] = habitat.Revision,
            };
            var plants = new List<object>();
            foreach (var plant in d4.Ecology.Plants)
                plants.Add(new Dictionary<string, object>
                {
                    ["plantId"] = plant.PlantId ?? "", ["habitatId"] = plant.HabitatId ?? "",
                    ["resourceKind"] = plant.ResourceKind ?? "plant",
                    ["quantity"] = plant.Quantity, ["capacity"] = plant.Capacity,
                    ["harvestPressure"] = plant.HarvestPressure,
                    ["recoveryRatePerDay"] = plant.RecoveryRatePerDay,
                    ["lastUpdatedDay"] = plant.LastUpdatedDay,
                });
            ecology["plants"] = plants;
            var fishSchools = new List<object>();
            foreach (var school in d4.Ecology.FishSchools)
                fishSchools.Add(new Dictionary<string, object>
                {
                    ["fishSchoolId"] = school.FishSchoolId ?? "",
                    ["fishSpeciesId"] = school.FishSpeciesId ?? "",
                    ["fishingSpotId"] = school.FishingSpotId ?? "",
                    ["worldX"] = school.WorldX,
                    ["worldZ"] = school.WorldZ,
                    ["biomass"] = school.Biomass,
                    ["capacity"] = school.Capacity,
                    ["depth"] = school.Depth,
                    ["migration"] = school.Migration ?? "nearshore",
                    ["fishingPressure"] = school.FishingPressure,
                    ["recovery"] = school.Recovery,
                    ["recoveryRatePerDay"] = school.RecoveryRatePerDay,
                    ["lastMovedDay"] = school.LastMovedDay,
                });
            ecology["fishSchools"] = fishSchools;

            var rareAnimals = d4.RareAnimals ?? new D11RareAnimalCollectionState();

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
                ["ecology"] = ecology, ["rareAnimals"] = D11RareAnimalsToObj(rareAnimals),
                ["health"] = health, ["mainline"] = mainline, ["routeLog"] = logs,
            };
        }

        private static Dictionary<string, object> D11RareAnimalsToObj(D11RareAnimalCollectionState state)
        {
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = state.SchemaVersion,
                ["revision"] = state.Revision,
                ["lastEventId"] = state.LastEventId ?? "",
                ["appliedEventKeys"] = StringListToObjects(state.AppliedEventKeys),
            };
            var book = new List<object>();
            foreach (var entry in state.Book ?? new List<D11CollectionBookEntry>())
                if (entry != null)
                    book.Add(new Dictionary<string, object>
                    {
                        ["entryId"] = entry.EntryId ?? "", ["speciesId"] = entry.SpeciesId ?? "",
                        ["animalId"] = entry.AnimalId ?? "", ["firstLocationRef"] = entry.FirstLocationRef ?? "",
                        ["firstWorldDay"] = entry.FirstWorldDay, ["firstSeason"] = entry.FirstSeason ?? "",
                        ["firstBehavior"] = entry.FirstBehavior ?? "", ["handling"] = entry.Handling ?? "",
                        ["evidenceIds"] = StringListToObjects(entry.EvidenceIds), ["observed"] = entry.Observed,
                        ["identified"] = entry.Identified, ["rescued"] = entry.Rescued,
                        ["released"] = entry.Released, ["collected"] = entry.Collected,
                        ["dead"] = entry.Dead, ["currentStatus"] = entry.CurrentStatus ?? "",
                        ["revision"] = entry.Revision,
                    });
            result["book"] = book;
            var evidence = new List<object>();
            foreach (var item in state.Evidence ?? new List<D11AnimalEvidenceRecord>())
                if (item != null)
                    evidence.Add(new Dictionary<string, object>
                    {
                        ["evidenceId"] = item.EvidenceId ?? "", ["kind"] = item.Kind.ToString(),
                        ["animalId"] = item.AnimalId ?? "", ["speciesId"] = item.SpeciesId ?? "",
                        ["locationRef"] = item.LocationRef ?? "", ["worldDay"] = item.WorldDay,
                        ["season"] = item.Season ?? "", ["behavior"] = item.Behavior ?? "",
                        ["handling"] = item.Handling ?? "", ["sourceRef"] = item.SourceRef ?? "",
                        ["evidenceRef"] = item.EvidenceRef ?? "", ["factEventId"] = item.FactEventId ?? "",
                        ["verified"] = item.Verified,
                    });
            result["evidence"] = evidence;
            var transports = new List<object>();
            foreach (var item in state.Transports ?? new List<D11AnimalTransportState>())
                if (item != null)
                    transports.Add(new Dictionary<string, object>
                    {
                        ["transportId"] = item.TransportId ?? "", ["animalId"] = item.AnimalId ?? "",
                        ["speciesId"] = item.SpeciesId ?? "", ["fromLocationRef"] = item.FromLocationRef ?? "",
                        ["toLocationRef"] = item.ToLocationRef ?? "", ["startWorldDay"] = item.StartWorldDay,
                        ["daysInTransit"] = item.DaysInTransit, ["daysElapsed"] = item.DaysElapsed,
                        ["seed"] = item.Seed, ["mortalityRisk"] = item.MortalityRisk,
                        ["status"] = item.Status.ToString(), ["evidenceIds"] = StringListToObjects(item.EvidenceIds),
                        ["lastUpdatedDay"] = item.LastUpdatedDay,
                    });
            result["transports"] = transports;
            var collectors = new List<object>();
            foreach (var item in state.Collectors ?? new List<D11CollectorState>())
                if (item != null)
                    collectors.Add(new Dictionary<string, object>
                    {
                        ["collectorId"] = item.CollectorId ?? "", ["collectorType"] = item.CollectorType ?? "",
                        ["buyerType"] = item.BuyerType ?? "", ["allowedSpeciesIds"] = StringListToObjects(item.AllowedSpeciesIds),
                        ["protectedAreaRef"] = item.ProtectedAreaRef ?? "", ["reputationGate"] = item.ReputationGate,
                        ["evidenceRequired"] = item.EvidenceRequired,
                    });
            result["collectors"] = collectors;
            var areas = new List<object>();
            foreach (var item in state.ProtectedAreas ?? new List<D11ProtectedAreaState>())
                if (item != null)
                    areas.Add(new Dictionary<string, object>
                    {
                        ["areaId"] = item.AreaId ?? "", ["locationRef"] = item.LocationRef ?? "",
                        ["observationAllowed"] = item.ObservationAllowed, ["rescueAllowed"] = item.RescueAllowed,
                        ["releaseAllowed"] = item.ReleaseAllowed, ["transferAllowed"] = item.TransferAllowed,
                        ["active"] = item.Active, ["revision"] = item.Revision,
                    });
            result["protectedAreas"] = areas;
            return result;
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
                result.Ecology.LastEcologyDay = Math.Max(0, SaveJson.GetInt(ecology, "lastEcologyDay"));
                result.Ecology.EcologyRevision = Math.Max(0L, ReadLong(ecology, "ecologyRevision"));
                result.Ecology.LastEcologyEventId = SaveJson.GetStr(ecology, "lastEcologyEventId", "");
                ReadStringListObject(result.Ecology.AppliedEcologyEventKeys, ecology, "appliedEcologyEventKeys");
                var habitatObj = Nested(ecology, "habitatCondition");
                if (habitatObj != null)
                {
                    result.Ecology.Habitat.HabitatId = SaveJson.GetStr(habitatObj, "habitatId", "nearshore_reef_edge");
                    result.Ecology.Habitat.BiomeId = SaveJson.GetStr(habitatObj, "biomeId", "reef");
                    result.Ecology.Habitat.TemperatureC = SaveJson.GetFloat(habitatObj, "temperatureC", 27f);
                    result.Ecology.Habitat.Turbidity = SaveJson.GetFloat(habitatObj, "turbidity", 0.10f);
                    result.Ecology.Habitat.Pollution = SaveJson.GetFloat(habitatObj, "pollution");
                    result.Ecology.Habitat.TidePhase = SaveJson.GetFloat(habitatObj, "tidePhase");
                    result.Ecology.Habitat.CurrentSpeed = SaveJson.GetFloat(habitatObj, "currentSpeed", 0.20f);
                    result.Ecology.Habitat.CurrentDirectionDegrees = SaveJson.GetFloat(habitatObj, "currentDirectionDegrees", 90f);
                    result.Ecology.Habitat.LastUpdatedDay = Math.Max(0, SaveJson.GetInt(habitatObj, "lastUpdatedDay"));
                    result.Ecology.Habitat.Revision = Math.Max(0L, ReadLong(habitatObj, "revision"));
                }
                foreach (var animalObj in ObjectList(ecology, "animals"))
                {
                    var animal = new AnimalState
                    {
                        AnimalId = SaveJson.GetStr(animalObj, "animalId", ""),
                        HabitatId = SaveJson.GetStr(animalObj, "habitatId", result.Ecology.Habitat.HabitatId),
                        Health = (int)NumericConfiguration.ClampHealth(SaveJson.GetInt(animalObj, "health", (int)NumericConfiguration.Default.Health.DefaultValue)),
                        EcologyStress = SaveJson.GetFloat(animalObj, "ecologyStress"),
                        LastUpdatedDay = Math.Max(0, SaveJson.GetInt(animalObj, "lastUpdatedDay")),
                        Observed = Convert.ToBoolean(animalObj.ContainsKey("observed") ? animalObj["observed"] : false),
                        CollectionValue = SaveJson.GetInt(animalObj, "collectionValue"),
                        Welfare = Math.Max(0, Math.Min(100, SaveJson.GetInt(animalObj, "welfare", 100))),
                        TransportDays = Math.Max(0, SaveJson.GetInt(animalObj, "transportDays")),
                        LastHandledDay = Math.Max(0, SaveJson.GetInt(animalObj, "lastHandledDay")),
                        CurrentLocationRef = SaveJson.GetStr(animalObj, "currentLocationRef", null),
                        SourceRef = SaveJson.GetStr(animalObj, "sourceRef", null),
                    };
                    Enum.TryParse(SaveJson.GetStr(animalObj, "behavior", "Foraging"), true, out animal.Behavior);
                    Enum.TryParse(SaveJson.GetStr(animalObj, "lifecycle", "Wild"), true, out animal.Lifecycle);
                    result.Ecology.Animals.Add(animal);
                }
                foreach (var schoolObj in ObjectList(ecology, "fishSchools"))
                    result.Ecology.FishSchools.Add(new FishSchoolState
                    {
                        FishSchoolId = SaveJson.GetStr(schoolObj, "fishSchoolId", ""),
                        FishSpeciesId = SaveJson.GetStr(schoolObj, "fishSpeciesId", ""),
                        FishingSpotId = SaveJson.GetStr(schoolObj, "fishingSpotId", ""),
                        WorldX = SaveJson.GetFloat(schoolObj, "worldX"),
                        WorldZ = SaveJson.GetFloat(schoolObj, "worldZ"),
                        Biomass = Math.Max(0, SaveJson.GetInt(schoolObj, "biomass", 12)),
                        Capacity = Math.Max(1, SaveJson.GetInt(schoolObj, "capacity", 12)),
                        Depth = Math.Max(0f, SaveJson.GetFloat(schoolObj, "depth", 2f)),
                        Migration = SaveJson.GetStr(schoolObj, "migration", "nearshore"),
                        FishingPressure = Math.Max(0, SaveJson.GetInt(schoolObj, "fishingPressure")),
                        Recovery = Math.Max(0f, SaveJson.GetFloat(schoolObj, "recovery")),
                        RecoveryRatePerDay = Math.Max(1, SaveJson.GetInt(schoolObj, "recoveryRatePerDay", 1)),
                        LastMovedDay = Math.Max(0, SaveJson.GetInt(schoolObj, "lastMovedDay")),
                    });
                foreach (var plantObj in ObjectList(ecology, "plants"))
                    result.Ecology.Plants.Add(new PlantResourceState
                    {
                        PlantId = SaveJson.GetStr(plantObj, "plantId", ""),
                        HabitatId = SaveJson.GetStr(plantObj, "habitatId", result.Ecology.Habitat.HabitatId),
                        ResourceKind = SaveJson.GetStr(plantObj, "resourceKind", "plant"),
                        Quantity = Math.Max(0, SaveJson.GetInt(plantObj, "quantity")),
                        Capacity = Math.Max(1, SaveJson.GetInt(plantObj, "capacity", 8)),
                        HarvestPressure = Math.Max(0, SaveJson.GetInt(plantObj, "harvestPressure")),
                        RecoveryRatePerDay = Math.Max(1, SaveJson.GetInt(plantObj, "recoveryRatePerDay", 1)),
                        LastUpdatedDay = Math.Max(0, SaveJson.GetInt(plantObj, "lastUpdatedDay")),
                    });
            }

            var rareAnimals = Nested(obj, "rareAnimals");
            if (rareAnimals != null) result.RareAnimals = D11RareAnimalsFromObj(rareAnimals);

            var health = Nested(obj, "health");
            if (health != null)
            {
                result.Health.MedicalSupplies = SaveJson.GetInt(health, "medicalSupplies");
                foreach (var crewObj in ObjectList(health, "crew"))
                {
                    var crew = new CrewHealthState
                    {
                        CrewId = SaveJson.GetStr(crewObj, "crewId", ""),
                        Health = (int)NumericConfiguration.ClampHealth(SaveJson.GetInt(crewObj, "health", (int)NumericConfiguration.Default.Health.DefaultValue)),
                        Fatigue = (int)NumericConfiguration.ClampFatigue(SaveJson.GetInt(crewObj, "fatigue", (int)NumericConfiguration.Default.Fatigue.DefaultValue)),
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

        private static D11RareAnimalCollectionState D11RareAnimalsFromObj(Dictionary<string, object> obj)
        {
            var result = new D11RareAnimalCollectionState
            {
                SchemaVersion = Math.Max(1, SaveJson.GetInt(obj, "schemaVersion", 1)),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = SaveJson.GetStr(obj, "lastEventId", ""),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "book"))
            {
                var entry = new D11CollectionBookEntry
                {
                    EntryId = SaveJson.GetStr(item, "entryId", ""), SpeciesId = SaveJson.GetStr(item, "speciesId", ""),
                    AnimalId = SaveJson.GetStr(item, "animalId", ""), FirstLocationRef = SaveJson.GetStr(item, "firstLocationRef", ""),
                    FirstWorldDay = SaveJson.GetInt(item, "firstWorldDay", -1), FirstSeason = SaveJson.GetStr(item, "firstSeason", ""),
                    FirstBehavior = SaveJson.GetStr(item, "firstBehavior", ""), Handling = SaveJson.GetStr(item, "handling", ""),
                    Observed = Bool(item, "observed"), Identified = Bool(item, "identified"), Rescued = Bool(item, "rescued"),
                    Released = Bool(item, "released"), Collected = Bool(item, "collected"), Dead = Bool(item, "dead"),
                    CurrentStatus = SaveJson.GetStr(item, "currentStatus", ""), Revision = ReadLong(item, "revision"),
                };
                ReadStringListObject(entry.EvidenceIds, item, "evidenceIds");
                result.Book.Add(entry);
            }
            foreach (var item in ObjectList(obj, "evidence"))
            {
                var evidence = new D11AnimalEvidenceRecord
                {
                    EvidenceId = SaveJson.GetStr(item, "evidenceId", ""), AnimalId = SaveJson.GetStr(item, "animalId", ""),
                    SpeciesId = SaveJson.GetStr(item, "speciesId", ""), LocationRef = SaveJson.GetStr(item, "locationRef", ""),
                    WorldDay = SaveJson.GetInt(item, "worldDay"), Season = SaveJson.GetStr(item, "season", ""),
                    Behavior = SaveJson.GetStr(item, "behavior", ""), Handling = SaveJson.GetStr(item, "handling", ""),
                    SourceRef = SaveJson.GetStr(item, "sourceRef", ""), EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""),
                    FactEventId = SaveJson.GetStr(item, "factEventId", ""), Verified = !item.ContainsKey("verified") || Bool(item, "verified"),
                };
                Enum.TryParse(SaveJson.GetStr(item, "kind", "Observation"), true, out evidence.Kind);
                result.Evidence.Add(evidence);
            }
            foreach (var item in ObjectList(obj, "transports"))
            {
                var transport = new D11AnimalTransportState
                {
                    TransportId = SaveJson.GetStr(item, "transportId", ""), AnimalId = SaveJson.GetStr(item, "animalId", ""),
                    SpeciesId = SaveJson.GetStr(item, "speciesId", ""), FromLocationRef = SaveJson.GetStr(item, "fromLocationRef", ""),
                    ToLocationRef = SaveJson.GetStr(item, "toLocationRef", ""), StartWorldDay = SaveJson.GetInt(item, "startWorldDay"),
                    DaysInTransit = Math.Max(1, SaveJson.GetInt(item, "daysInTransit", 1)), DaysElapsed = Math.Max(0, SaveJson.GetInt(item, "daysElapsed")),
                    Seed = SaveJson.GetInt(item, "seed"), MortalityRisk = SaveJson.GetFloat(item, "mortalityRisk"),
                    LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "status", "InTransit"), true, out transport.Status);
                ReadStringListObject(transport.EvidenceIds, item, "evidenceIds");
                result.Transports.Add(transport);
            }
            foreach (var item in ObjectList(obj, "collectors"))
            {
                var collector = new D11CollectorState
                {
                    CollectorId = SaveJson.GetStr(item, "collectorId", ""), CollectorType = SaveJson.GetStr(item, "collectorType", ""),
                    BuyerType = SaveJson.GetStr(item, "buyerType", ""), ProtectedAreaRef = SaveJson.GetStr(item, "protectedAreaRef", ""),
                    ReputationGate = SaveJson.GetInt(item, "reputationGate"), EvidenceRequired = !item.ContainsKey("evidenceRequired") || Bool(item, "evidenceRequired"),
                };
                var allowed = new List<string>();
                ReadStringListObject(allowed, item, "allowedSpeciesIds");
                collector.AllowedSpeciesIds = allowed.ToArray();
                result.Collectors.Add(collector);
            }
            foreach (var item in ObjectList(obj, "protectedAreas"))
                result.ProtectedAreas.Add(new D11ProtectedAreaState
                {
                    AreaId = SaveJson.GetStr(item, "areaId", ""), LocationRef = SaveJson.GetStr(item, "locationRef", ""),
                    ObservationAllowed = !item.ContainsKey("observationAllowed") || Bool(item, "observationAllowed"),
                    RescueAllowed = !item.ContainsKey("rescueAllowed") || Bool(item, "rescueAllowed"),
                    ReleaseAllowed = !item.ContainsKey("releaseAllowed") || Bool(item, "releaseAllowed"),
                    TransferAllowed = Bool(item, "transferAllowed"), Active = !item.ContainsKey("active") || Bool(item, "active"),
                    Revision = ReadLong(item, "revision"),
                });
            return result;
        }

        private static bool Bool(Dictionary<string, object> obj, string key)
        {
            return obj != null && obj.ContainsKey(key) && Convert.ToBoolean(obj[key]);
        }

        private static Dictionary<string, object> D9ToObj(D9RuntimeSnapshot d9)
        {
            d9.Validate();
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = d9.SchemaVersion,
            };
            if (d9.TreasureQuest != null)
            {
                var quest = d9.TreasureQuest;
                var questObj = new Dictionary<string, object>
                {
                    ["schemaVersion"] = quest.SchemaVersion,
                    ["questId"] = quest.QuestId ?? string.Empty,
                    ["status"] = quest.Status.ToString(),
                    ["rumorHeard"] = quest.RumorHeard,
                    ["equipmentReady"] = quest.EquipmentReady,
                    ["clueReliabilityPercent"] = quest.ClueReliabilityPercent,
                    ["lastFailureCode"] = quest.LastFailureCode ?? string.Empty,
                    ["recoveryRouteId"] = quest.RecoveryRouteId ?? string.Empty,
                    ["lastResolutionEventRef"] = quest.LastResolutionEventRef ?? string.Empty,
                    ["lastRecoveryEventRef"] = quest.LastRecoveryEventRef ?? string.Empty,
                    ["verifiedClueIds"] = StringListToObjects(quest.VerifiedClueIds),
                    ["crossValidationCount"] = quest.CrossValidationCount,
                    ["dangerTags"] = StringListToObjects(quest.DangerTags),
                    ["factEventRefs"] = StringListToObjects(quest.FactEventRefs),
                };
                var clues = new List<object>();
                foreach (var clue in quest.Clues ?? new List<D9TreasureClueRecord>())
                    clues.Add(new Dictionary<string, object>
                    {
                        ["clueId"] = clue.ClueId ?? "",
                        ["questId"] = clue.QuestId ?? "",
                        ["sourceKind"] = clue.SourceKind.ToString(),
                        ["sourceRef"] = clue.SourceRef ?? "",
                        ["evidenceRef"] = clue.EvidenceRef ?? "",
                        ["text"] = clue.Text ?? "",
                        ["reliabilityPercent"] = clue.ReliabilityPercent,
                        ["regionRef"] = clue.RegionRef ?? "",
                        ["siteRef"] = clue.SiteRef ?? "",
                        ["validFromWorldDay"] = clue.ValidFromWorldDay,
                        ["validUntilWorldDay"] = clue.ValidUntilWorldDay,
                        ["requiredEquipmentIds"] = StringListToObjects(clue.RequiredEquipmentIds),
                        ["alternativeRouteId"] = clue.AlternativeRouteId ?? "",
                        ["rewardType"] = clue.RewardType.ToString(),
                        ["causeEventIds"] = StringListToObjects(clue.CauseEventIds),
                        ["crossValidatedByClueIds"] = StringListToObjects(clue.CrossValidatedByClueIds),
                        ["factEventRef"] = clue.FactEventRef ?? "",
                        ["isCrossValidated"] = clue.IsCrossValidated,
                    });
                questObj["clues"] = clues;
                if (quest.RewardRequest != null)
                {
                    questObj["rewardRequest"] = new Dictionary<string, object>
                    {
                        ["requestId"] = quest.RewardRequest.RequestId ?? string.Empty,
                        ["rewardId"] = quest.RewardRequest.RewardId ?? string.Empty,
                        ["questId"] = quest.RewardRequest.QuestId ?? string.Empty,
                        ["sourceEventRef"] = quest.RewardRequest.SourceEventRef ?? string.Empty,
                        ["rewardType"] = quest.RewardRequest.RewardType.ToString(),
                        ["causeEventIds"] = StringListToObjects(quest.RewardRequest.CauseEventIds),
                        ["clueIds"] = StringListToObjects(quest.RewardRequest.ClueIds),
                        ["status"] = quest.RewardRequest.Status ?? "pending",
                    };
                }
                result["treasureQuest"] = questObj;
            }
            return result;
        }

        private static D9RuntimeSnapshot D9FromObj(Dictionary<string, object> obj)
        {
            var result = new D9RuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D9RuntimeSnapshot.CurrentSchemaVersion),
            };
            var quest = Nested(obj, "treasureQuest");
            if (quest != null)
            {
                var state = new D9TreasureQuestSnapshot
                {
                    SchemaVersion = SaveJson.GetInt(quest, "schemaVersion", 1),
                    QuestId = SaveJson.GetStr(quest, "questId", D9TreasureQuestCatalog.QuestId),
                    RumorHeard = ReadBool(quest, "rumorHeard"),
                    EquipmentReady = ReadBool(quest, "equipmentReady"),
                    ClueReliabilityPercent = SaveJson.GetInt(quest, "clueReliabilityPercent", 72),
                    LastFailureCode = SaveJson.GetStr(quest, "lastFailureCode", D9TreasureFailureCodes.None),
                    RecoveryRouteId = SaveJson.GetStr(quest, "recoveryRouteId", null),
                    LastResolutionEventRef = SaveJson.GetStr(quest, "lastResolutionEventRef", null),
                    LastRecoveryEventRef = SaveJson.GetStr(quest, "lastRecoveryEventRef", null),
                    CrossValidationCount = SaveJson.GetInt(quest, "crossValidationCount"),
                };
                Enum.TryParse(SaveJson.GetStr(quest, "status", "RumorAvailable"), true, out state.Status);
                ReadStringListObject(state.DangerTags, quest, "dangerTags");
                ReadStringListObject(state.FactEventRefs, quest, "factEventRefs");
                ReadStringListObject(state.VerifiedClueIds, quest, "verifiedClueIds");
                foreach (var clueObj in ObjectList(quest, "clues"))
                {
                    var clue = new D9TreasureClueRecord
                    {
                        ClueId = SaveJson.GetStr(clueObj, "clueId", ""),
                        QuestId = SaveJson.GetStr(clueObj, "questId", D9TreasureQuestCatalog.QuestId),
                        SourceRef = SaveJson.GetStr(clueObj, "sourceRef", ""),
                        EvidenceRef = SaveJson.GetStr(clueObj, "evidenceRef", ""),
                        Text = SaveJson.GetStr(clueObj, "text", ""),
                        ReliabilityPercent = SaveJson.GetInt(clueObj, "reliabilityPercent"),
                        RegionRef = SaveJson.GetStr(clueObj, "regionRef", ""),
                        SiteRef = SaveJson.GetStr(clueObj, "siteRef", D9TreasureQuestCatalog.SiteRef),
                        ValidFromWorldDay = SaveJson.GetInt(clueObj, "validFromWorldDay", -1),
                        ValidUntilWorldDay = SaveJson.GetInt(clueObj, "validUntilWorldDay", -1),
                        AlternativeRouteId = SaveJson.GetStr(clueObj, "alternativeRouteId", null),
                        FactEventRef = SaveJson.GetStr(clueObj, "factEventRef", null),
                        IsCrossValidated = Bool(clueObj, "isCrossValidated"),
                    };
                    if (!Enum.TryParse(SaveJson.GetStr(clueObj, "sourceKind", "TavernRumor"), true, out clue.SourceKind))
                        throw new InvalidOperationException("未知 D9 寻宝线索来源：" + clue.ClueId);
                    if (!Enum.TryParse(SaveJson.GetStr(clueObj, "rewardType", "MapKnowledge"), true, out clue.RewardType))
                        throw new InvalidOperationException("未知 D9 寻宝奖励类型：" + clue.ClueId);
                    ReadStringListObject(clue.RequiredEquipmentIds, clueObj, "requiredEquipmentIds");
                    ReadStringListObject(clue.CauseEventIds, clueObj, "causeEventIds");
                    ReadStringListObject(clue.CrossValidatedByClueIds, clueObj, "crossValidatedByClueIds");
                    state.Clues.Add(clue);
                }
                var reward = Nested(quest, "rewardRequest");
                if (reward != null)
                    state.RewardRequest = new D9TreasureRewardRequest
                    {
                        RequestId = SaveJson.GetStr(reward, "requestId", ""),
                        RewardId = SaveJson.GetStr(reward, "rewardId", ""),
                        QuestId = SaveJson.GetStr(reward, "questId", D9TreasureQuestCatalog.QuestId),
                        SourceEventRef = SaveJson.GetStr(reward, "sourceEventRef", ""),
                        Status = SaveJson.GetStr(reward, "status", "pending"),
                    };
                if (state.RewardRequest != null)
                {
                    if (!Enum.TryParse(SaveJson.GetStr(reward, "rewardType", "MapKnowledge"), true, out state.RewardRequest.RewardType))
                        throw new InvalidOperationException("未知 D9 奖励类型：" + state.RewardRequest.RequestId);
                    ReadStringListObject(state.RewardRequest.CauseEventIds, reward, "causeEventIds");
                    ReadStringListObject(state.RewardRequest.ClueIds, reward, "clueIds");
                }
                result.TreasureQuest = state;
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12CompatToObj2(D12RuntimeSnapshot d12)
        {
            d12.Validate();
            var features = new List<object>();
            foreach (var feature in d12.MapDiscovery.Features)
            {
                var evidence = new List<object>();
                if (feature.Evidence != null)
                    foreach (var item in feature.Evidence)
                        evidence.Add(new Dictionary<string, object>
                        {
                            ["evidenceId"] = item.EvidenceId ?? "",
                            ["source"] = item.Source.ToString(),
                            ["kind"] = item.Kind.ToString(),
                            ["sourceRef"] = item.SourceRef ?? "",
                            ["evidenceRef"] = item.EvidenceRef ?? "",
                            ["summary"] = item.Summary ?? "",
                            ["locationRef"] = item.LocationRef ?? "",
                            ["toolId"] = item.ToolId ?? "",
                            ["eventId"] = item.EventId ?? "",
                            ["worldDay"] = item.WorldDay,
                            ["saveRevision"] = item.SaveRevision,
                            ["strength"] = item.Strength,
                        });
                features.Add(new Dictionary<string, object>
                {
                    ["featureId"] = feature.FeatureId ?? "",
                    ["stage"] = feature.Stage.ToString(),
                    ["credibility"] = feature.Credibility,
                    ["lastDiscoveredDay"] = feature.LastDiscoveredDay,
                    ["lastSource"] = feature.LastSource ?? "",
                    ["lastEvidenceId"] = feature.LastEvidenceId ?? "",
                    ["hasMeasuredDepth"] = feature.HasMeasuredDepth,
                    ["measuredDepthMeters"] = feature.MeasuredDepthMeters,
                    ["evidenceIds"] = StringListToObjects(feature.EvidenceIds),
                    ["evidence"] = evidence,
                });
            }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = d12.SchemaVersion,
                ["mapDiscovery"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = d12.MapDiscovery.SchemaVersion,
                    ["lastWorldDay"] = d12.MapDiscovery.LastWorldDay,
                    ["revision"] = d12.MapDiscovery.Revision,
                    ["lastEventId"] = d12.MapDiscovery.LastEventId ?? "",
                    ["features"] = features,
                },
            };
        }

        private static D12RuntimeSnapshot D12CompatFromObj2(Dictionary<string, object> obj)
        {
            var result = new D12RuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12RuntimeSnapshot.CurrentSchemaVersion),
            };
            var map = Nested(obj, "mapDiscovery");
            if (map != null)
            {
                result.MapDiscovery = new D12MapDiscoverySnapshot
                {
                    SchemaVersion = SaveJson.GetInt(map, "schemaVersion", D12MapDiscoverySnapshot.CurrentSchemaVersion),
                    LastWorldDay = Math.Max(0, SaveJson.GetInt(map, "lastWorldDay", 0)),
                    Revision = Math.Max(0L, ReadLong(map, "revision")),
                    LastEventId = SaveJson.GetStr(map, "lastEventId", ""),
                };
                foreach (var featureObj in ObjectList(map, "features"))
                {
                    var feature = new D12MapFeatureState
                    {
                        FeatureId = SaveJson.GetStr(featureObj, "featureId", ""),
                        Credibility = SaveJson.GetFloat(featureObj, "credibility"),
                        LastDiscoveredDay = Math.Max(0, SaveJson.GetInt(featureObj, "lastDiscoveredDay", 0)),
                        LastSource = SaveJson.GetStr(featureObj, "lastSource", ""),
                        LastEvidenceId = SaveJson.GetStr(featureObj, "lastEvidenceId", ""),
                        HasMeasuredDepth = ReadBool(featureObj, "hasMeasuredDepth"),
                        MeasuredDepthMeters = Math.Max(0f, SaveJson.GetFloat(featureObj, "measuredDepthMeters")),
                    };
                    if (!Enum.TryParse(SaveJson.GetStr(featureObj, "stage", "Unknown"), true, out feature.Stage))
                        throw new InvalidOperationException("未知 D12 海图阶段：" + feature.FeatureId);
                    ReadStringListObject(feature.EvidenceIds, featureObj, "evidenceIds");
                    foreach (var evidenceObj in ObjectList(featureObj, "evidence"))
                    {
                        var evidence = new D12ChartEvidence
                        {
                            EvidenceId = SaveJson.GetStr(evidenceObj, "evidenceId", ""),
                            SourceRef = SaveJson.GetStr(evidenceObj, "sourceRef", ""),
                            EvidenceRef = SaveJson.GetStr(evidenceObj, "evidenceRef", ""),
                            Summary = SaveJson.GetStr(evidenceObj, "summary", ""),
                            LocationRef = SaveJson.GetStr(evidenceObj, "locationRef", ""),
                            ToolId = SaveJson.GetStr(evidenceObj, "toolId", ""),
                            EventId = SaveJson.GetStr(evidenceObj, "eventId", ""),
                            WorldDay = Math.Max(0, SaveJson.GetInt(evidenceObj, "worldDay", 0)),
                            SaveRevision = Math.Max(0L, ReadLong(evidenceObj, "saveRevision")),
                            Strength = SaveJson.GetFloat(evidenceObj, "strength"),
                        };
                        if (!Enum.TryParse(SaveJson.GetStr(evidenceObj, "source", "Exploration"), true, out evidence.Source))
                            throw new InvalidOperationException("未知 D12 海图证据来源：" + evidence.EvidenceId);
                        if (!Enum.TryParse(SaveJson.GetStr(evidenceObj, "kind", "DirectSight"), true, out evidence.Kind))
                            throw new InvalidOperationException("未知 D12 海图证据类型：" + evidence.EvidenceId);
                        feature.Evidence.Add(evidence);
                    }
                    result.MapDiscovery.Features.Add(feature);
                }
            }
            result.Validate();
            return result;
        }

        // D12 的领域裁决仍在 ExplorationSystem；这些方法只补齐既有
        // SaveSystem 入口的 DTO 编解码，避免存档层复制发现逻辑。
        private static Dictionary<string, object> D12DynamicQuestLogToObj(D12DynamicQuestLogSnapshot log)
        {
            if (log == null) log = new D12DynamicQuestLogSnapshot();
            log.Validate();
            var quests = new List<object>();
            foreach (var state in log.Quests)
            {
                var objectives = new List<object>();
                foreach (var objective in state.Objectives)
                {
                    var appliedFacts = new List<object>();
                    foreach (var fact in objective.AppliedFacts)
                        appliedFacts.Add(D12QuestFactRefToObj(fact));
                    objectives.Add(new Dictionary<string, object>
                    {
                        ["objectiveId"] = objective.ObjectiveId ?? "",
                        ["progress"] = objective.Progress,
                        ["appliedFacts"] = appliedFacts,
                    });
                }
                var factEvents = new List<object>();
                foreach (var fact in state.FactEvents) factEvents.Add(D12QuestFactRefToObj(fact));
                quests.Add(new Dictionary<string, object>
                {
                    ["questId"] = state.QuestId ?? "",
                    ["status"] = state.Status.ToString(),
                    ["acceptedWorldDay"] = state.AcceptedWorldDay,
                    ["lastUpdatedWorldDay"] = state.LastUpdatedWorldDay,
                    ["lastFailureCode"] = state.LastFailureCode ?? "",
                    ["lastFailureFact"] = D12QuestFactRefToObj(state.LastFailureFact),
                    ["lastRecoveryFact"] = D12QuestFactRefToObj(state.LastRecoveryFact),
                    ["recoveryRouteId"] = state.RecoveryRouteId ?? "",
                    ["alternativeRouteId"] = state.AlternativeRouteId ?? "",
                    ["completedFact"] = D12QuestFactRefToObj(state.CompletedFact),
                    ["objectives"] = objectives,
                    ["factEvents"] = factEvents,
                    ["rewardRequest"] = D12QuestRewardRequestToObj(state.RewardRequest),
                });
            }
            var journalFacts = new List<object>();
            foreach (var fact in log.JournalFacts) journalFacts.Add(D12QuestFactRefToObj(fact));
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = log.SchemaVersion,
                ["quests"] = quests,
                ["journalFacts"] = journalFacts,
            };
        }

        private static D12DynamicQuestLogSnapshot D12DynamicQuestLogFromObj(Dictionary<string, object> obj)
        {
            var result = new D12DynamicQuestLogSnapshot
            {
                SchemaVersion = obj == null ? D12DynamicQuestLogSnapshot.CurrentSchemaVersion :
                    SaveJson.GetInt(obj, "schemaVersion", D12DynamicQuestLogSnapshot.CurrentSchemaVersion),
            };
            if (obj == null)
            {
                result.Validate();
                return result;
            }

            foreach (var questObj in ObjectList(obj, "quests"))
            {
                var state = new D12DynamicQuestState
                {
                    QuestId = SaveJson.GetStr(questObj, "questId", ""),
                    AcceptedWorldDay = SaveJson.GetInt(questObj, "acceptedWorldDay", -1),
                    LastUpdatedWorldDay = Math.Max(0, SaveJson.GetInt(questObj, "lastUpdatedWorldDay")),
                    LastFailureCode = SaveJson.GetStr(questObj, "lastFailureCode", ""),
                    LastFailureFact = D12QuestFactRefFromObj(Nested(questObj, "lastFailureFact")),
                    LastRecoveryFact = D12QuestFactRefFromObj(Nested(questObj, "lastRecoveryFact")),
                    RecoveryRouteId = EmptyToNull(SaveJson.GetStr(questObj, "recoveryRouteId", "")),
                    AlternativeRouteId = EmptyToNull(SaveJson.GetStr(questObj, "alternativeRouteId", "")),
                    CompletedFact = D12QuestFactRefFromObj(Nested(questObj, "completedFact")),
                    RewardRequest = D12QuestRewardRequestFromObj(Nested(questObj, "rewardRequest")),
                };
                if (!Enum.TryParse(SaveJson.GetStr(questObj, "status", "Available"), true, out state.Status))
                    throw new InvalidOperationException("未知 D12 动态任务状态：" + state.QuestId);
                foreach (var objectiveObj in ObjectList(questObj, "objectives"))
                {
                    var objective = new D12QuestObjectiveState
                    {
                        ObjectiveId = SaveJson.GetStr(objectiveObj, "objectiveId", ""),
                        Progress = Math.Max(0, SaveJson.GetInt(objectiveObj, "progress")),
                    };
                    foreach (var factObj in ObjectList(objectiveObj, "appliedFacts"))
                    {
                        var fact = D12QuestFactRefFromObj(factObj);
                        if (fact != null) objective.AppliedFacts.Add(fact);
                    }
                    state.Objectives.Add(objective);
                }
                foreach (var factObj in ObjectList(questObj, "factEvents"))
                {
                    var fact = D12QuestFactRefFromObj(factObj);
                    if (fact != null) state.FactEvents.Add(fact);
                }
                result.Quests.Add(state);
            }
            foreach (var factObj in ObjectList(obj, "journalFacts"))
            {
                var fact = D12QuestFactRefFromObj(factObj);
                if (fact != null) result.JournalFacts.Add(fact);
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12QuestFactRefToObj(D12QuestFactRef fact)
        {
            if (fact == null) return null;
            fact.Validate("quest_fact_ref");
            return new Dictionary<string, object>
            {
                ["eventId"] = fact.EventId ?? "",
                ["saveRevision"] = fact.SaveRevision,
            };
        }

        private static D12QuestFactRef D12QuestFactRefFromObj(Dictionary<string, object> obj)
        {
            if (obj == null) return null;
            return new D12QuestFactRef
            {
                EventId = SaveJson.GetStr(obj, "eventId", ""),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
        }

        private static Dictionary<string, object> D12QuestRewardRequestToObj(D12QuestRewardRequest request)
        {
            if (request == null) return null;
            request.Validate();
            return new Dictionary<string, object>
            {
                ["requestId"] = request.RequestId ?? "",
                ["rewardId"] = request.RewardId ?? "",
                ["questId"] = request.QuestId ?? "",
                ["sourceEventId"] = request.SourceEventId ?? "",
                ["sourceSaveRevision"] = request.SourceSaveRevision,
                ["rewardType"] = request.RewardType.ToString(),
                ["ownerSystem"] = request.OwnerSystem ?? "RewardSystem",
                ["status"] = request.Status ?? "pending",
                ["causeEventIds"] = StringListToObjects(request.CauseEventIds),
            };
        }

        private static D12QuestRewardRequest D12QuestRewardRequestFromObj(Dictionary<string, object> obj)
        {
            if (obj == null) return null;
            var result = new D12QuestRewardRequest
            {
                RequestId = SaveJson.GetStr(obj, "requestId", ""),
                RewardId = SaveJson.GetStr(obj, "rewardId", ""),
                QuestId = SaveJson.GetStr(obj, "questId", ""),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                SourceSaveRevision = Math.Max(0L, ReadLong(obj, "sourceSaveRevision")),
                OwnerSystem = SaveJson.GetStr(obj, "ownerSystem", "RewardSystem"),
                Status = SaveJson.GetStr(obj, "status", "pending"),
            };
            if (!Enum.TryParse(SaveJson.GetStr(obj, "rewardType", "MapKnowledge"), true, out result.RewardType))
                throw new InvalidOperationException("未知 D12 动态任务奖励类型：" + result.RequestId);
            ReadStringListObject(result.CauseEventIds, obj, "causeEventIds");
            result.Validate();
            return result;
        }

        private static string EmptyToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static Dictionary<string, object> D12ToObj(D12RuntimeSnapshot runtime)
        {
            runtime.Validate();
            var features = new List<object>();
            if (runtime.MapDiscovery != null && runtime.MapDiscovery.Features != null)
                foreach (var feature in runtime.MapDiscovery.Features)
                {
                    var evidence = new List<object>();
                    if (feature.Evidence != null)
                        foreach (var item in feature.Evidence)
                            if (item != null) evidence.Add(new Dictionary<string, object>
                            {
                                ["evidenceId"] = item.EvidenceId ?? "",
                                ["source"] = item.Source.ToString(),
                                ["kind"] = item.Kind.ToString(),
                                ["sourceRef"] = item.SourceRef ?? "",
                                ["evidenceRef"] = item.EvidenceRef ?? "",
                                ["summary"] = item.Summary ?? "",
                                ["locationRef"] = item.LocationRef ?? "",
                                ["toolId"] = item.ToolId ?? "",
                                ["eventId"] = item.EventId ?? "",
                                ["worldDay"] = item.WorldDay,
                                ["saveRevision"] = item.SaveRevision,
                                ["strength"] = item.Strength,
                            });
                    features.Add(new Dictionary<string, object>
                    {
                        ["featureId"] = feature.FeatureId ?? "",
                        ["stage"] = feature.Stage.ToString(),
                        ["credibility"] = feature.Credibility,
                        ["lastDiscoveredDay"] = feature.LastDiscoveredDay,
                        ["lastSource"] = feature.LastSource ?? "",
                        ["lastEvidenceId"] = feature.LastEvidenceId ?? "",
                        ["hasMeasuredDepth"] = feature.HasMeasuredDepth,
                        ["measuredDepthMeters"] = feature.MeasuredDepthMeters,
                        ["evidenceIds"] = StringListToObjects(feature.EvidenceIds),
                        ["evidence"] = evidence,
                    });
                }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["mapDiscovery"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = runtime.MapDiscovery.SchemaVersion,
                    ["lastWorldDay"] = runtime.MapDiscovery.LastWorldDay,
                    ["revision"] = runtime.MapDiscovery.Revision,
                    ["lastEventId"] = runtime.MapDiscovery.LastEventId ?? "",
                    ["features"] = features,
                },
                ["tarot"] = D12TarotLegacyToObj(runtime.Tarot),
                ["collectionAtlas"] = D12CollectionAtlasToObj(runtime.CollectionAtlas),
                ["communityTrade"] = D12CommunityTradeToObj(runtime.CommunityTrade),
                ["salvage"] = D12SalvageToObj(runtime.Salvage),
                ["dynamicQuestLog"] = D12DynamicQuestLogToObj(runtime.DynamicQuestLog),
            };
        }

        private static D12RuntimeSnapshot D12FromObj(Dictionary<string, object> obj)
        {
            var map = Nested(obj, "mapDiscovery");
            var result = new D12RuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12RuntimeSnapshot.CurrentSchemaVersion),
                MapDiscovery = new D12MapDiscoverySnapshot
                {
                    SchemaVersion = map == null ? D12MapDiscoverySnapshot.CurrentSchemaVersion :
                        SaveJson.GetInt(map, "schemaVersion", D12MapDiscoverySnapshot.CurrentSchemaVersion),
                    LastWorldDay = map == null ? 0 : Math.Max(0, SaveJson.GetInt(map, "lastWorldDay")),
                    Revision = map == null ? 0L : Math.Max(0L, ReadLong(map, "revision")),
                    LastEventId = map == null ? "" : SaveJson.GetStr(map, "lastEventId", ""),
                },
                Tarot = D12TarotLegacyFromObj(Nested(obj, "tarot")),
                CollectionAtlas = D12CollectionAtlasFromObj(Nested(obj, "collectionAtlas")),
                CommunityTrade = D12CommunityTradeFromObj(Nested(obj, "communityTrade")),
                Salvage = D12SalvageFromObj(Nested(obj, "salvage")),
                DynamicQuestLog = D12DynamicQuestLogFromObj(Nested(obj, "dynamicQuestLog")),
            };
            if (map != null)
                foreach (var featureObj in ObjectList(map, "features"))
                {
                    var feature = new D12MapFeatureState
                    {
                        FeatureId = SaveJson.GetStr(featureObj, "featureId", ""),
                        Credibility = SaveJson.GetFloat(featureObj, "credibility"),
                        LastDiscoveredDay = Math.Max(0, SaveJson.GetInt(featureObj, "lastDiscoveredDay")),
                        LastSource = SaveJson.GetStr(featureObj, "lastSource", ""),
                        LastEvidenceId = SaveJson.GetStr(featureObj, "lastEvidenceId", ""),
                        HasMeasuredDepth = ReadBool(featureObj, "hasMeasuredDepth"),
                        MeasuredDepthMeters = Math.Max(0f, SaveJson.GetFloat(featureObj, "measuredDepthMeters")),
                    };
                    Enum.TryParse(SaveJson.GetStr(featureObj, "stage", "Unknown"), true, out feature.Stage);
                    ReadStringListObject(feature.EvidenceIds, featureObj, "evidenceIds");
                    foreach (var evidenceObj in ObjectList(featureObj, "evidence"))
                    {
                        var evidence = new D12ChartEvidence
                        {
                            EvidenceId = SaveJson.GetStr(evidenceObj, "evidenceId", ""),
                            SourceRef = SaveJson.GetStr(evidenceObj, "sourceRef", ""),
                            EvidenceRef = SaveJson.GetStr(evidenceObj, "evidenceRef", ""),
                            Summary = SaveJson.GetStr(evidenceObj, "summary", ""),
                            LocationRef = SaveJson.GetStr(evidenceObj, "locationRef", ""),
                            ToolId = SaveJson.GetStr(evidenceObj, "toolId", ""),
                            EventId = SaveJson.GetStr(evidenceObj, "eventId", ""),
                            WorldDay = Math.Max(0, SaveJson.GetInt(evidenceObj, "worldDay")),
                            SaveRevision = Math.Max(0L, ReadLong(evidenceObj, "saveRevision")),
                            Strength = Math.Max(0f, Math.Min(1f, SaveJson.GetFloat(evidenceObj, "strength"))),
                        };
                        Enum.TryParse(SaveJson.GetStr(evidenceObj, "source", "Exploration"), true, out evidence.Source);
                        Enum.TryParse(SaveJson.GetStr(evidenceObj, "kind", "DirectSight"), true, out evidence.Kind);
                        feature.Evidence.Add(evidence);
                    }
                    result.MapDiscovery.Features.Add(feature);
                }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12SalvageToObj(D12SalvageSnapshot salvage)
        {
            if (salvage == null) salvage = new D12SalvageSnapshot();
            salvage.Validate();
            var sites = new List<object>();
            if (salvage.Sites != null)
                foreach (var site in salvage.Sites)
                {
                    var losses = new List<object>();
                    if (site.LossRecords != null)
                        foreach (var loss in site.LossRecords)
                            if (loss != null) losses.Add(new Dictionary<string, object>
                            {
                                ["lossId"] = loss.LossId ?? "",
                                ["kind"] = loss.Kind.ToString(),
                                ["summary"] = loss.Summary ?? "",
                                ["severity"] = loss.Severity,
                                ["recoveryRouteId"] = loss.RecoveryRouteId ?? "",
                                ["eventId"] = loss.EventId ?? "",
                                ["worldDay"] = loss.WorldDay,
                            });
                    Dictionary<string, object> reward = null;
                    if (site.RewardRequest != null)
                        reward = new Dictionary<string, object>
                        {
                            ["requestId"] = site.RewardRequest.RequestId ?? "",
                            ["rewardId"] = site.RewardRequest.RewardId ?? "",
                            ["siteId"] = site.RewardRequest.SiteId ?? "",
                            ["sourceEventRef"] = site.RewardRequest.SourceEventRef ?? "",
                            ["status"] = site.RewardRequest.Status ?? "pending",
                        };
                    sites.Add(new Dictionary<string, object>
                    {
                        ["siteId"] = site.SiteId ?? "",
                        ["status"] = site.Status.ToString(),
                        ["attemptCount"] = site.AttemptCount,
                        ["lastFailureCode"] = site.LastFailureCode ?? "",
                        ["lastRecoveryRouteId"] = site.LastRecoveryRouteId ?? "",
                        ["lastResolutionEventRef"] = site.LastResolutionEventRef ?? "",
                        ["lastFailureEventRef"] = site.LastFailureEventRef ?? "",
                        ["lastRecoveryEventRef"] = site.LastRecoveryEventRef ?? "",
                        ["lastCompetitorRef"] = site.LastCompetitorRef ?? "",
                        ["lastCompetitorPressurePercent"] = site.LastCompetitorPressurePercent,
                        ["rewardRequest"] = reward,
                        ["lossRecords"] = losses,
                        ["factEventRefs"] = StringListToObjects(site.FactEventRefs),
                    });
                }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = salvage.SchemaVersion,
                ["lastWorldDay"] = salvage.LastWorldDay,
                ["revision"] = salvage.Revision,
                ["lastEventId"] = salvage.LastEventId ?? "",
                ["sites"] = sites,
            };
        }

        private static D12SalvageSnapshot D12SalvageFromObj(Dictionary<string, object> obj)
        {
            var result = new D12SalvageSnapshot
            {
                SchemaVersion = obj == null ? D12SalvageSnapshot.CurrentSchemaVersion :
                    SaveJson.GetInt(obj, "schemaVersion", D12SalvageSnapshot.CurrentSchemaVersion),
                LastWorldDay = obj == null ? 0 : Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")),
                Revision = obj == null ? 0L : Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = obj == null ? "" : SaveJson.GetStr(obj, "lastEventId", ""),
            };
            if (obj != null)
                foreach (var siteObj in ObjectList(obj, "sites"))
                {
                    var site = new D12SalvageSiteState
                    {
                        SiteId = SaveJson.GetStr(siteObj, "siteId", ""),
                        AttemptCount = Math.Max(0, SaveJson.GetInt(siteObj, "attemptCount")),
                        LastFailureCode = SaveJson.GetStr(siteObj, "lastFailureCode", D12SalvageFailureCode.None.ToString()),
                        LastRecoveryRouteId = SaveJson.GetStr(siteObj, "lastRecoveryRouteId", ""),
                        LastResolutionEventRef = SaveJson.GetStr(siteObj, "lastResolutionEventRef", ""),
                        LastFailureEventRef = SaveJson.GetStr(siteObj, "lastFailureEventRef", ""),
                        LastRecoveryEventRef = SaveJson.GetStr(siteObj, "lastRecoveryEventRef", ""),
                        LastCompetitorRef = SaveJson.GetStr(siteObj, "lastCompetitorRef", ""),
                        LastCompetitorPressurePercent = Math.Max(0, Math.Min(100, SaveJson.GetInt(siteObj, "lastCompetitorPressurePercent"))),
                    };
                    Enum.TryParse(SaveJson.GetStr(siteObj, "status", "Available"), true, out site.Status);
                    ReadStringListObject(site.FactEventRefs, siteObj, "factEventRefs");
                    foreach (var lossObj in ObjectList(siteObj, "lossRecords"))
                    {
                        var loss = new D12SalvageLossRecord
                        {
                            LossId = SaveJson.GetStr(lossObj, "lossId", ""),
                            Summary = SaveJson.GetStr(lossObj, "summary", ""),
                            Severity = Math.Max(0, Math.Min(100, SaveJson.GetInt(lossObj, "severity"))),
                            RecoveryRouteId = SaveJson.GetStr(lossObj, "recoveryRouteId", ""),
                            EventId = SaveJson.GetStr(lossObj, "eventId", ""),
                            WorldDay = Math.Max(0, SaveJson.GetInt(lossObj, "worldDay")),
                        };
                        Enum.TryParse(SaveJson.GetStr(lossObj, "kind", "None"), true, out loss.Kind);
                        site.LossRecords.Add(loss);
                    }
                    var rewardObj = Nested(siteObj, "rewardRequest");
                    if (rewardObj != null)
                        site.RewardRequest = new D12SalvageRewardRequest
                        {
                            RequestId = SaveJson.GetStr(rewardObj, "requestId", ""),
                            RewardId = SaveJson.GetStr(rewardObj, "rewardId", ""),
                            SiteId = SaveJson.GetStr(rewardObj, "siteId", site.SiteId),
                            SourceEventRef = SaveJson.GetStr(rewardObj, "sourceEventRef", ""),
                            Status = SaveJson.GetStr(rewardObj, "status", "pending"),
                        };
                    result.Sites.Add(site);
                }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12TarotA07WrapperToObj(D12TarotRuntimeSnapshot snapshot)
        {
            return D12TarotLegacyToObj(snapshot);
        }

        private static D12TarotRuntimeSnapshot D12TarotA07WrapperFromObj(Dictionary<string, object> obj)
        {
            return D12TarotLegacyFromObj(obj);
        }

        private static Dictionary<string, object> D12PokerA07WrapperToObj(D12PokerRuntimeSnapshot poker)
        {
            return D12PokerA07UnusedToObj(poker);
        }

        private static D12PokerRuntimeSnapshot D12PokerA07WrapperFromObj(Dictionary<string, object> obj)
        {
            return D12PokerA07UnusedFromObj(obj);
        }

        private static Dictionary<string, object> D12TarotLegacyToObj(D12TarotRuntimeSnapshot snapshot)
        {
            if (snapshot == null) snapshot = new D12TarotRuntimeSnapshot();
            snapshot.Validate();
            var sessions = new List<object>();
            foreach (var session in snapshot.Sessions)
            {
                var cards = new List<object>();
                foreach (var card in session.Cards)
                    cards.Add(new Dictionary<string, object>
                    {
                        ["drawId"] = card.DrawId ?? "", ["cardId"] = card.CardId ?? "", ["orientation"] = card.Orientation.ToString(),
                        ["position"] = card.Position, ["meaningTag"] = card.MeaningTag ?? "", ["softSignal"] = card.SoftSignal ?? "",
                    });
                var clues = new List<object>();
                foreach (var clue in session.Clues)
                    clues.Add(new Dictionary<string, object>
                    {
                        ["clueId"] = clue.ClueId ?? "", ["sessionId"] = clue.SessionId ?? "", ["kind"] = clue.Kind ?? "",
                        ["targetRef"] = clue.TargetRef ?? "", ["signal"] = clue.Signal ?? "", ["reliability"] = clue.Reliability,
                        ["biasCode"] = clue.BiasCode ?? "", ["status"] = clue.Status.ToString(), ["evidenceEventId"] = clue.EvidenceEventId ?? "",
                        ["sourceRef"] = clue.SourceRef ?? "", ["evidenceRef"] = clue.EvidenceRef ?? "", ["worldDay"] = clue.WorldDay,
                        ["saveRevision"] = clue.SaveRevision, ["verified"] = clue.Verified,
                    });
                sessions.Add(new Dictionary<string, object>
                {
                    ["sessionId"] = session.SessionId ?? "", ["divinerId"] = session.DivinerId ?? "", ["queryTag"] = session.QueryTag ?? "",
                    ["actorRef"] = session.ActorRef ?? "", ["locationRef"] = session.LocationRef ?? "", ["worldDay"] = session.WorldDay,
                    ["seed"] = session.Seed, ["sensitivity"] = session.Sensitivity.ToString(), ["warningAcknowledged"] = session.WarningAcknowledged,
                    ["skippedSensitiveContent"] = session.SkippedSensitiveContent, ["explanationText"] = session.ExplanationText ?? "",
                    ["explanationProvider"] = session.ExplanationProvider ?? "", ["lastEventId"] = session.LastEventId ?? "",
                    ["revision"] = session.Revision, ["appliedEventKeys"] = StringListToObjects(session.AppliedEventKeys),
                    ["cards"] = cards, ["clues"] = clues,
                });
            }
            var trusts = new List<object>();
            foreach (var trust in snapshot.TrustStates)
                trusts.Add(new Dictionary<string, object>
                {
                    ["divinerId"] = trust.DivinerId ?? "", ["cultureId"] = trust.CultureId ?? "", ["religionId"] = trust.ReligionId ?? "",
                    ["trust"] = trust.Trust, ["sourceRef"] = trust.SourceRef ?? "", ["evidenceRef"] = trust.EvidenceRef ?? "",
                    ["lastEventId"] = trust.LastEventId ?? "", ["lastWorldDay"] = trust.LastWorldDay, ["lastSaveRevision"] = trust.LastSaveRevision,
                    ["appliedEventKeys"] = StringListToObjects(trust.AppliedEventKeys),
                });
            var verifications = new List<object>();
            foreach (var item in snapshot.Verifications)
                verifications.Add(new Dictionary<string, object>
                {
                    ["verificationId"] = item.VerificationId ?? "", ["sessionId"] = item.SessionId ?? "", ["clueId"] = item.ClueId ?? "",
                    ["method"] = item.Method.ToString(), ["outcome"] = item.Outcome.ToString(), ["evidenceRef"] = item.EvidenceRef ?? "",
                    ["sourceRef"] = item.SourceRef ?? "", ["factEventId"] = item.FactEventId ?? "", ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision, ["lastEventId"] = item.LastEventId ?? "",
                });
            var consequences = new List<object>();
            foreach (var item in snapshot.Consequences)
                consequences.Add(new Dictionary<string, object>
                {
                    ["consequenceId"] = item.ConsequenceId ?? "", ["sessionId"] = item.SessionId ?? "", ["clueId"] = item.ClueId ?? "",
                    ["verificationId"] = item.VerificationId ?? "", ["impactTag"] = item.ImpactTag ?? "", ["followedAdvice"] = item.FollowedAdvice,
                    ["costBand"] = item.CostBand ?? "", ["causeEventIds"] = StringListToObjects(item.CauseEventIds), ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision, ["eventId"] = item.EventId ?? "", ["downstreamOwner"] = item.DownstreamOwner ?? "",
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = snapshot.SchemaVersion, ["revision"] = snapshot.Revision, ["lastEventId"] = snapshot.LastEventId ?? "",
                ["appliedEventKeys"] = StringListToObjects(snapshot.AppliedEventKeys), ["sessions"] = sessions, ["trustStates"] = trusts,
                ["verifications"] = verifications, ["consequences"] = consequences,
            };
        }

        private static D12TarotRuntimeSnapshot D12TarotLegacyFromObj(Dictionary<string, object> obj)
        {
            var result = new D12TarotRuntimeSnapshot();
            if (obj == null) return result;
            result.SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12TarotRuntimeSnapshot.CurrentSchemaVersion);
            result.Revision = Math.Max(0L, ReadLong(obj, "revision"));
            result.LastEventId = SaveJson.GetStr(obj, "lastEventId", "");
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "sessions"))
            {
                var session = new D12TarotSessionState
                {
                    SessionId = SaveJson.GetStr(item, "sessionId", ""), DivinerId = SaveJson.GetStr(item, "divinerId", ""), QueryTag = SaveJson.GetStr(item, "queryTag", ""),
                    ActorRef = SaveJson.GetStr(item, "actorRef", "player"), LocationRef = SaveJson.GetStr(item, "locationRef", "island_saint_marys"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    Seed = ReadLong(item, "seed"), WarningAcknowledged = ReadBool(item, "warningAcknowledged"), SkippedSensitiveContent = ReadBool(item, "skippedSensitiveContent"),
                    ExplanationText = SaveJson.GetStr(item, "explanationText", ""), ExplanationProvider = SaveJson.GetStr(item, "explanationProvider", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                    Revision = Math.Max(0L, ReadLong(item, "revision")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "sensitivity", "None"), true, out session.Sensitivity);
                ReadStringListObject(session.AppliedEventKeys, item, "appliedEventKeys");
                foreach (var cardObj in ObjectList(item, "cards"))
                {
                    var card = new D12TarotCardDraw
                    {
                        DrawId = SaveJson.GetStr(cardObj, "drawId", ""), CardId = SaveJson.GetStr(cardObj, "cardId", ""), Position = SaveJson.GetInt(cardObj, "position"),
                        MeaningTag = SaveJson.GetStr(cardObj, "meaningTag", ""), SoftSignal = SaveJson.GetStr(cardObj, "softSignal", ""),
                    };
                    Enum.TryParse(SaveJson.GetStr(cardObj, "orientation", "Upright"), true, out card.Orientation);
                    session.Cards.Add(card);
                }
                foreach (var clueObj in ObjectList(item, "clues"))
                {
                    var clue = new D12SoftClue
                    {
                        ClueId = SaveJson.GetStr(clueObj, "clueId", ""), SessionId = SaveJson.GetStr(clueObj, "sessionId", session.SessionId), Kind = SaveJson.GetStr(clueObj, "kind", ""),
                        TargetRef = SaveJson.GetStr(clueObj, "targetRef", ""), Signal = SaveJson.GetStr(clueObj, "signal", ""), Reliability = SaveJson.GetFloat(clueObj, "reliability"),
                        BiasCode = SaveJson.GetStr(clueObj, "biasCode", ""), EvidenceEventId = SaveJson.GetStr(clueObj, "evidenceEventId", ""), SourceRef = SaveJson.GetStr(clueObj, "sourceRef", ""),
                        EvidenceRef = SaveJson.GetStr(clueObj, "evidenceRef", ""), WorldDay = Math.Max(0, SaveJson.GetInt(clueObj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(clueObj, "saveRevision")), Verified = ReadBool(clueObj, "verified"),
                    };
                    Enum.TryParse(SaveJson.GetStr(clueObj, "status", "Unverified"), true, out clue.Status);
                    session.Clues.Add(clue);
                }
                result.Sessions.Add(session);
            }
            foreach (var item in ObjectList(obj, "trustStates"))
            {
                var trust = new D12DivinationTrustState
                {
                    DivinerId = SaveJson.GetStr(item, "divinerId", ""), CultureId = SaveJson.GetStr(item, "cultureId", ""), ReligionId = SaveJson.GetStr(item, "religionId", ""),
                    Trust = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "trust"))), SourceRef = SaveJson.GetStr(item, "sourceRef", ""), EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""),
                    LastEventId = SaveJson.GetStr(item, "lastEventId", ""), LastWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastWorldDay")), LastSaveRevision = Math.Max(0L, ReadLong(item, "lastSaveRevision")),
                };
                ReadStringListObject(trust.AppliedEventKeys, item, "appliedEventKeys");
                result.TrustStates.Add(trust);
            }
            foreach (var item in ObjectList(obj, "verifications"))
            {
                var verification = new D12TarotVerificationState
                {
                    VerificationId = SaveJson.GetStr(item, "verificationId", ""), SessionId = SaveJson.GetStr(item, "sessionId", ""), ClueId = SaveJson.GetStr(item, "clueId", ""),
                    EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""), SourceRef = SaveJson.GetStr(item, "sourceRef", ""), FactEventId = SaveJson.GetStr(item, "factEventId", ""),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                };
                Enum.TryParse(SaveJson.GetStr(item, "method", "EventRecord"), true, out verification.Method);
                Enum.TryParse(SaveJson.GetStr(item, "outcome", "Unresolved"), true, out verification.Outcome);
                result.Verifications.Add(verification);
            }
            foreach (var item in ObjectList(obj, "consequences"))
            {
                var consequence = new D12TarotErrorConsequenceRecord
                {
                    ConsequenceId = SaveJson.GetStr(item, "consequenceId", ""), SessionId = SaveJson.GetStr(item, "sessionId", ""), ClueId = SaveJson.GetStr(item, "clueId", ""),
                    VerificationId = SaveJson.GetStr(item, "verificationId", ""), ImpactTag = SaveJson.GetStr(item, "impactTag", ""), FollowedAdvice = ReadBool(item, "followedAdvice"), CostBand = SaveJson.GetStr(item, "costBand", ""),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), EventId = SaveJson.GetStr(item, "eventId", ""), DownstreamOwner = SaveJson.GetStr(item, "downstreamOwner", "caller_domain_system"),
                };
                ReadStringListObject(consequence.CauseEventIds, item, "causeEventIds");
                result.Consequences.Add(consequence);
            }
            return result;
        }

        private static Dictionary<string, object> D12CommunityTradeToObj(D12CommunityTradeSnapshot snapshot)
        {
            if (snapshot == null) snapshot = new D12CommunityTradeSnapshot();
            snapshot.Validate();
            var communities = new List<object>();
            foreach (var community in snapshot.Communities)
                communities.Add(new Dictionary<string, object>
                {
                    ["communityId"] = community.CommunityId ?? "",
                    ["stock"] = community.Stock,
                    ["revision"] = community.Revision,
                    ["lastEventId"] = community.LastEventId ?? "",
                    ["appliedEventKeys"] = StringListToObjects(community.AppliedEventKeys),
                });
            var reputations = new List<object>();
            foreach (var reputation in snapshot.Reputations)
            {
                var history = new List<object>();
                foreach (var change in reputation.History)
                    history.Add(new Dictionary<string, object>
                    {
                        ["eventId"] = change.EventId ?? "",
                        ["causeEventIds"] = StringListToObjects(change.CauseEventIds),
                        ["worldDay"] = change.WorldDay,
                        ["reasonCode"] = change.ReasonCode ?? "",
                        ["reputationDelta"] = change.ReputationDelta,
                        ["trustDelta"] = change.TrustDelta,
                        ["reputationAfter"] = change.ReputationAfter,
                        ["trustAfter"] = change.TrustAfter,
                        ["explanation"] = change.Explanation ?? "",
                    });
                reputations.Add(new Dictionary<string, object>
                {
                    ["actorRef"] = reputation.ActorRef ?? "",
                    ["communityId"] = reputation.CommunityId ?? "",
                    ["longTermReputation"] = reputation.LongTermReputation,
                    ["trust"] = reputation.Trust,
                    ["culturalKnowledge"] = reputation.CulturalKnowledge,
                    ["etiquetteFamiliarity"] = reputation.EtiquetteFamiliarity,
                    ["misunderstandingCount"] = reputation.MisunderstandingCount,
                    ["helpReceivedCount"] = reputation.HelpReceivedCount,
                    ["refusalCount"] = reputation.RefusalCount,
                    ["successfulTradeCount"] = reputation.SuccessfulTradeCount,
                    ["revision"] = reputation.Revision,
                    ["history"] = history,
                });
            }
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = snapshot.SchemaVersion,
                ["lastWorldDay"] = snapshot.LastWorldDay,
                ["revision"] = snapshot.Revision,
                ["lastEventId"] = snapshot.LastEventId ?? "",
                ["communities"] = communities,
                ["reputations"] = reputations,
            };
            if (snapshot.EventLog != null) result["eventLog"] = EventLogToObj(snapshot.EventLog);
            return result;
        }

        private static D12CommunityTradeSnapshot D12CommunityTradeFromObj(Dictionary<string, object> obj)
        {
            var result = new D12CommunityTradeSnapshot
            {
                SchemaVersion = obj == null ? D12CommunityTradeIds.SchemaVersion :
                    SaveJson.GetInt(obj, "schemaVersion", D12CommunityTradeIds.SchemaVersion),
                LastWorldDay = obj == null ? 0 : Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")),
                Revision = obj == null ? 0L : Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = obj == null ? "" : SaveJson.GetStr(obj, "lastEventId", ""),
            };
            if (obj != null)
            {
                foreach (var communityObj in ObjectList(obj, "communities"))
                {
                    var community = new D12CommunityInventoryState
                    {
                        CommunityId = SaveJson.GetStr(communityObj, "communityId", ""),
                        Revision = Math.Max(0L, ReadLong(communityObj, "revision")),
                        LastEventId = SaveJson.GetStr(communityObj, "lastEventId", ""),
                    };
                    ReadIntDictObject(community.Stock, communityObj, "stock");
                    ReadStringListObject(community.AppliedEventKeys, communityObj, "appliedEventKeys");
                    result.Communities.Add(community);
                }
                foreach (var reputationObj in ObjectList(obj, "reputations"))
                {
                    var reputation = new D12CommunityReputationState
                    {
                        ActorRef = SaveJson.GetStr(reputationObj, "actorRef", "player"),
                        CommunityId = SaveJson.GetStr(reputationObj, "communityId", ""),
                        LongTermReputation = Math.Max(-100, Math.Min(100, SaveJson.GetInt(reputationObj, "longTermReputation"))),
                        Trust = Math.Max(0, Math.Min(100, SaveJson.GetInt(reputationObj, "trust"))),
                        CulturalKnowledge = Math.Max(0, Math.Min(100, SaveJson.GetInt(reputationObj, "culturalKnowledge"))),
                        EtiquetteFamiliarity = Math.Max(0, Math.Min(100, SaveJson.GetInt(reputationObj, "etiquetteFamiliarity"))),
                        MisunderstandingCount = Math.Max(0, SaveJson.GetInt(reputationObj, "misunderstandingCount")),
                        HelpReceivedCount = Math.Max(0, SaveJson.GetInt(reputationObj, "helpReceivedCount")),
                        RefusalCount = Math.Max(0, SaveJson.GetInt(reputationObj, "refusalCount")),
                        SuccessfulTradeCount = Math.Max(0, SaveJson.GetInt(reputationObj, "successfulTradeCount")),
                        Revision = Math.Max(0L, ReadLong(reputationObj, "revision")),
                    };
                    foreach (var historyObj in ObjectList(reputationObj, "history"))
                    {
                        var change = new D12CommunityReputationChange
                        {
                            EventId = SaveJson.GetStr(historyObj, "eventId", ""),
                            WorldDay = Math.Max(0, SaveJson.GetInt(historyObj, "worldDay")),
                            ReasonCode = SaveJson.GetStr(historyObj, "reasonCode", ""),
                            ReputationDelta = SaveJson.GetInt(historyObj, "reputationDelta"),
                            TrustDelta = SaveJson.GetInt(historyObj, "trustDelta"),
                            ReputationAfter = Math.Max(-100, Math.Min(100, SaveJson.GetInt(historyObj, "reputationAfter"))),
                            TrustAfter = Math.Max(0, Math.Min(100, SaveJson.GetInt(historyObj, "trustAfter"))),
                            Explanation = SaveJson.GetStr(historyObj, "explanation", ""),
                        };
                        ReadStringListObject(change.CauseEventIds, historyObj, "causeEventIds");
                        reputation.History.Add(change);
                    }
                    result.Reputations.Add(reputation);
                }
                var eventLog = Nested(obj, "eventLog");
                if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12CollectionAtlasToObj(D12CollectionAtlasState atlas)
        {
            if (atlas == null) atlas = new D12CollectionAtlasState();
            atlas.Validate();
            var records = new List<object>();
            foreach (var record in atlas.Records)
            {
                var evidenceIds = StringListToObjects(record.EvidenceIds);
                records.Add(new Dictionary<string, object>
                {
                    ["collectionId"] = record.CollectionId ?? "",
                    ["contentId"] = record.ContentId ?? "",
                    ["category"] = record.Category.ToString(),
                    ["unlocked"] = record.Unlocked,
                    ["firstEvidenceId"] = record.FirstEvidenceId ?? "",
                    ["firstLocationRef"] = record.FirstLocationRef ?? "",
                    ["firstWorldDay"] = record.FirstWorldDay,
                    ["firstAcquisitionMethod"] = record.FirstAcquisitionMethod.ToString(),
                    ["evidenceIds"] = evidenceIds,
                    ["achievementId"] = record.AchievementId ?? "",
                    ["achievementUnlocked"] = record.AchievementUnlocked,
                    ["achievementEventId"] = record.AchievementEventId ?? "",
                    ["revision"] = record.Revision,
                });
            }
            var evidence = new List<object>();
            foreach (var item in atlas.Evidence)
                evidence.Add(new Dictionary<string, object>
                {
                    ["evidenceId"] = item.EvidenceId ?? "",
                    ["contentId"] = item.ContentId ?? "",
                    ["category"] = item.Category.ToString(),
                    ["factEventId"] = item.FactEventId ?? "",
                    ["factSaveRevision"] = item.FactSaveRevision,
                    ["sourceRef"] = item.SourceRef ?? "",
                    ["evidenceRef"] = item.EvidenceRef ?? "",
                    ["locationRef"] = item.LocationRef ?? "",
                    ["worldDay"] = item.WorldDay,
                    ["acquisitionMethod"] = item.AcquisitionMethod.ToString(),
                    ["authorizationRef"] = item.AuthorizationRef ?? "",
                    ["authorizationFactEventId"] = item.AuthorizationFactEventId ?? "",
                    ["authorizationSaveRevision"] = item.AuthorizationSaveRevision,
                    ["recordedEventId"] = item.RecordedEventId ?? "",
                    ["recordedSaveRevision"] = item.RecordedSaveRevision,
                });
            var achievements = new List<object>();
            foreach (var item in atlas.Achievements)
                achievements.Add(new Dictionary<string, object>
                {
                    ["achievementId"] = item.AchievementId ?? "",
                    ["contentId"] = item.ContentId ?? "",
                    ["unlocked"] = item.Unlocked,
                    ["evidenceId"] = item.EvidenceId ?? "",
                    ["eventId"] = item.EventId ?? "",
                    ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision,
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = atlas.SchemaVersion,
                ["revision"] = atlas.Revision,
                ["records"] = records,
                ["evidence"] = evidence,
                ["achievements"] = achievements,
                ["appliedEventKeys"] = StringListToObjects(atlas.AppliedEventKeys),
            };
        }

        private static D12CollectionAtlasState D12CollectionAtlasFromObj(Dictionary<string, object> obj)
        {
            var result = new D12CollectionAtlasState();
            if (obj == null)
            {
                result.Validate();
                return result;
            }
            result.SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12CollectionAtlasState.CurrentSchemaVersion);
            result.Revision = Math.Max(0L, ReadLong(obj, "revision"));
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "records"))
            {
                var record = new D12CollectionRecord
                {
                    CollectionId = SaveJson.GetStr(item, "collectionId", ""),
                    ContentId = SaveJson.GetStr(item, "contentId", ""),
                    Unlocked = ReadBool(item, "unlocked"),
                    FirstEvidenceId = SaveJson.GetStr(item, "firstEvidenceId", ""),
                    FirstLocationRef = SaveJson.GetStr(item, "firstLocationRef", ""),
                    FirstWorldDay = Math.Max(0, SaveJson.GetInt(item, "firstWorldDay")),
                    AchievementId = SaveJson.GetStr(item, "achievementId", ""),
                    AchievementUnlocked = ReadBool(item, "achievementUnlocked"),
                    AchievementEventId = SaveJson.GetStr(item, "achievementEventId", ""),
                    Revision = Math.Max(0L, ReadLong(item, "revision")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "category", "Animal"), true, out record.Category);
                Enum.TryParse(SaveJson.GetStr(item, "firstAcquisitionMethod", "Observation"), true, out record.FirstAcquisitionMethod);
                ReadStringListObject(record.EvidenceIds, item, "evidenceIds");
                result.Records.Add(record);
            }
            foreach (var item in ObjectList(obj, "evidence"))
            {
                var evidence = new D12CollectionEvidenceRecord
                {
                    EvidenceId = SaveJson.GetStr(item, "evidenceId", ""),
                    ContentId = SaveJson.GetStr(item, "contentId", ""),
                    FactEventId = SaveJson.GetStr(item, "factEventId", ""),
                    FactSaveRevision = Math.Max(0L, ReadLong(item, "factSaveRevision")),
                    SourceRef = SaveJson.GetStr(item, "sourceRef", ""),
                    EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""),
                    LocationRef = SaveJson.GetStr(item, "locationRef", ""),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    AuthorizationRef = SaveJson.GetStr(item, "authorizationRef", ""),
                    AuthorizationFactEventId = SaveJson.GetStr(item, "authorizationFactEventId", ""),
                    AuthorizationSaveRevision = Math.Max(0L, ReadLong(item, "authorizationSaveRevision")),
                    RecordedEventId = SaveJson.GetStr(item, "recordedEventId", ""),
                    RecordedSaveRevision = Math.Max(0L, ReadLong(item, "recordedSaveRevision")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "category", "Animal"), true, out evidence.Category);
                Enum.TryParse(SaveJson.GetStr(item, "acquisitionMethod", "Observation"), true, out evidence.AcquisitionMethod);
                result.Evidence.Add(evidence);
            }
            foreach (var item in ObjectList(obj, "achievements"))
            {
                result.Achievements.Add(new D12CollectionAchievement
                {
                    AchievementId = SaveJson.GetStr(item, "achievementId", ""),
                    ContentId = SaveJson.GetStr(item, "contentId", ""),
                    Unlocked = ReadBool(item, "unlocked"),
                    EvidenceId = SaveJson.GetStr(item, "evidenceId", ""),
                    EventId = SaveJson.GetStr(item, "eventId", ""),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                });
            }
            result.Validate();
            return result;
        }

        // D12 Poker 的保存入口在 D12-A03 已冻结；这里仅补齐 DTO 编解码，
        // 不在存档层重算牌面、资金、关系或奖励。
        private static Dictionary<string, object> D12PokerA07UnusedToObj(D12PokerRuntimeSnapshot poker)
        {
            if (poker == null) return null;
            poker.Validate();
            var sessions = new List<object>();
            foreach (var item in poker.Sessions)
                sessions.Add(new Dictionary<string, object>
                {
                    ["schemaVersion"] = item.SchemaVersion, ["tableId"] = item.TableId ?? "", ["playerId"] = item.PlayerId ?? "",
                    ["gameId"] = item.GameId ?? "", ["rulesetId"] = item.RulesetId ?? "", ["fixedSeed"] = item.FixedSeed,
                    ["entryFee"] = item.EntryFee, ["betCap"] = item.BetCap, ["currentBet"] = item.CurrentBet,
                    ["proposedDebt"] = item.ProposedDebt, ["status"] = item.Status.ToString(), ["outcome"] = item.Outcome.ToString(),
                    ["cheatRisk"] = item.CheatRisk.ToString(), ["cheatAttempted"] = item.CheatAttempted,
                    ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision, ["lastEventId"] = item.LastEventId ?? "",
                    ["playerHand"] = D12PokerCardsToObj(item.PlayerHand), ["houseHand"] = D12PokerCardsToObj(item.HouseHand),
                });
            var records = new List<object>();
            foreach (var item in poker.Records)
                records.Add(new Dictionary<string, object>
                {
                    ["schemaVersion"] = item.SchemaVersion, ["recordId"] = item.RecordId ?? "", ["gameId"] = item.GameId ?? "",
                    ["tableId"] = item.TableId ?? "", ["playerId"] = item.PlayerId ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["saveRevision"] = item.SaveRevision, ["worldDay"] = item.WorldDay, ["fixedSeed"] = item.FixedSeed,
                    ["entryFee"] = item.EntryFee, ["bet"] = item.Bet, ["payout"] = item.Payout, ["netProposedDelta"] = item.NetProposedDelta,
                    ["outcome"] = item.Outcome.ToString(), ["playerHandCategory"] = item.PlayerHandCategory.ToString(),
                    ["houseHandCategory"] = item.HouseHandCategory.ToString(), ["cheatRisk"] = item.CheatRisk.ToString(),
                    ["playerCardIds"] = StringListToObjects(item.PlayerCardIds), ["houseCardIds"] = StringListToObjects(item.HouseCardIds),
                    ["settlementClaimed"] = item.SettlementClaimed,
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = poker.SchemaVersion, ["revision"] = poker.Revision, ["lastWorldDay"] = poker.LastWorldDay,
                ["lastEventId"] = poker.LastEventId ?? "", ["nextGameSequence"] = poker.NextGameSequence,
                ["appliedEventKeys"] = StringListToObjects(poker.AppliedEventKeys), ["sessions"] = sessions, ["records"] = records,
                ["economyRequests"] = D12PokerEconomyRequestsToObj(poker.EconomyRequests),
                ["debtRequests"] = D12PokerDebtRequestsToObj(poker.DebtRequests),
                ["rewardRequests"] = D12PokerRewardRequestsToObj(poker.RewardRequests),
                ["relationImpactRequests"] = D12PokerRelationRequestsToObj(poker.RelationImpactRequests),
                ["informationRequests"] = D12PokerInformationRequestsToObj(poker.InformationRequests),
            };
        }

        private static D12PokerRuntimeSnapshot D12PokerA07UnusedFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerRuntimeSnapshot();
            if (obj == null) { result.Validate(); return result; }
            result.SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerRuntimeSnapshot.CurrentSchemaVersion);
            result.Revision = Math.Max(0L, ReadLong(obj, "revision"));
            result.LastWorldDay = Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay"));
            result.LastEventId = SaveJson.GetStr(obj, "lastEventId", "");
            result.NextGameSequence = Math.Max(0L, ReadLong(obj, "nextGameSequence"));
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "sessions"))
            {
                var session = new D12PokerSessionState
                {
                    SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D12PokerIds.CurrentSchemaVersion),
                    TableId = SaveJson.GetStr(item, "tableId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""),
                    GameId = SaveJson.GetStr(item, "gameId", ""), RulesetId = SaveJson.GetStr(item, "rulesetId", D12PokerIds.RulesetId),
                    FixedSeed = ReadLong(item, "fixedSeed"), EntryFee = ReadLong(item, "entryFee"), BetCap = ReadLong(item, "betCap"),
                    CurrentBet = ReadLong(item, "currentBet"), ProposedDebt = ReadLong(item, "proposedDebt"),
                    CheatAttempted = ReadBool(item, "cheatAttempted"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                };
                Enum.TryParse(SaveJson.GetStr(item, "status", "Unknown"), true, out session.Status);
                Enum.TryParse(SaveJson.GetStr(item, "outcome", "Unknown"), true, out session.Outcome);
                Enum.TryParse(SaveJson.GetStr(item, "cheatRisk", "None"), true, out session.CheatRisk);
                D12PokerCardsFromObj(session.PlayerHand, item, "playerHand");
                D12PokerCardsFromObj(session.HouseHand, item, "houseHand");
                result.Sessions.Add(session);
            }
            foreach (var item in ObjectList(obj, "records"))
            {
                var record = new D12PokerGameRecord
                {
                    SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D12PokerIds.CurrentSchemaVersion),
                    RecordId = SaveJson.GetStr(item, "recordId", ""), GameId = SaveJson.GetStr(item, "gameId", ""),
                    TableId = SaveJson.GetStr(item, "tableId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""),
                    SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), FixedSeed = ReadLong(item, "fixedSeed"),
                    EntryFee = ReadLong(item, "entryFee"), Bet = ReadLong(item, "bet"), Payout = ReadLong(item, "payout"),
                    NetProposedDelta = ReadLong(item, "netProposedDelta"), SettlementClaimed = ReadBool(item, "settlementClaimed"),
                };
                Enum.TryParse(SaveJson.GetStr(item, "outcome", "Unknown"), true, out record.Outcome);
                Enum.TryParse(SaveJson.GetStr(item, "playerHandCategory", "Unknown"), true, out record.PlayerHandCategory);
                Enum.TryParse(SaveJson.GetStr(item, "houseHandCategory", "Unknown"), true, out record.HouseHandCategory);
                Enum.TryParse(SaveJson.GetStr(item, "cheatRisk", "None"), true, out record.CheatRisk);
                ReadStringListObject(record.PlayerCardIds, item, "playerCardIds");
                ReadStringListObject(record.HouseCardIds, item, "houseCardIds");
                result.Records.Add(record);
            }
            D12PokerRequestsFromObj(result, obj);
            result.Validate();
            return result;
        }

        private static List<object> D12PokerCardsToObj(List<D12PokerCard> cards)
        {
            var result = new List<object>();
            if (cards != null) foreach (var card in cards) if (card != null)
                result.Add(new Dictionary<string, object> { ["cardId"] = card.CardId ?? "", ["suit"] = card.Suit.ToString(), ["rank"] = card.Rank.ToString() });
            return result;
        }

        private static void D12PokerCardsFromObj(List<D12PokerCard> target, Dictionary<string, object> obj, string key)
        {
            foreach (var item in ObjectList(obj, key))
            {
                var card = new D12PokerCard { CardId = SaveJson.GetStr(item, "cardId", "") };
                Enum.TryParse(SaveJson.GetStr(item, "suit", "Unknown"), true, out card.Suit);
                Enum.TryParse(SaveJson.GetStr(item, "rank", "Unknown"), true, out card.Rank);
                target.Add(card);
            }
        }

        private static List<object> D12PokerEconomyRequestsToObj(List<D12PokerEconomyRequest> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var item in values) if (item != null)
                result.Add(new Dictionary<string, object> { ["schemaVersion"] = item.SchemaVersion, ["requestId"] = item.RequestId ?? "", ["gameId"] = item.GameId ?? "", ["sourceEventId"] = item.SourceEventId ?? "", ["playerId"] = item.PlayerId ?? "", ["currency"] = item.Currency ?? "", ["kind"] = item.Kind ?? "", ["amount"] = item.Amount, ["status"] = item.Status.ToString() });
            return result;
        }

        private static List<object> D12PokerDebtRequestsToObj(List<D12PokerDebtRequest> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var item in values) if (item != null)
                result.Add(new Dictionary<string, object> { ["schemaVersion"] = item.SchemaVersion, ["requestId"] = item.RequestId ?? "", ["gameId"] = item.GameId ?? "", ["sourceEventId"] = item.SourceEventId ?? "", ["playerId"] = item.PlayerId ?? "", ["principal"] = item.Principal, ["currentDebt"] = item.CurrentDebt, ["debtLimit"] = item.DebtLimit, ["status"] = item.Status.ToString() });
            return result;
        }

        private static List<object> D12PokerRewardRequestsToObj(List<D12PokerRewardRequest> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var item in values) if (item != null)
                result.Add(new Dictionary<string, object> { ["schemaVersion"] = item.SchemaVersion, ["requestId"] = item.RequestId ?? "", ["rewardId"] = item.RewardId ?? "", ["gameId"] = item.GameId ?? "", ["sourceEventId"] = item.SourceEventId ?? "", ["playerId"] = item.PlayerId ?? "", ["rewardTag"] = item.RewardTag ?? "", ["currency"] = item.Currency ?? "", ["currencyAmount"] = item.CurrencyAmount, ["status"] = item.Status.ToString() });
            return result;
        }

        private static List<object> D12PokerRelationRequestsToObj(List<D12PokerRelationImpactRequest> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var item in values) if (item != null)
                result.Add(new Dictionary<string, object> { ["schemaVersion"] = item.SchemaVersion, ["requestId"] = item.RequestId ?? "", ["sourceEventId"] = item.SourceEventId ?? "", ["sourceId"] = item.SourceId ?? "", ["targetId"] = item.TargetId ?? "", ["relationType"] = item.RelationType ?? "", ["proposedDelta"] = item.ProposedDelta, ["reason"] = item.Reason ?? "", ["status"] = item.Status.ToString() });
            return result;
        }

        private static List<object> D12PokerInformationRequestsToObj(List<D12PokerInformationRequest> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var item in values) if (item != null)
                result.Add(new Dictionary<string, object> { ["schemaVersion"] = item.SchemaVersion, ["requestId"] = item.RequestId ?? "", ["sourceEventId"] = item.SourceEventId ?? "", ["playerId"] = item.PlayerId ?? "", ["informationPoolId"] = item.InformationPoolId ?? "", ["informationCode"] = item.InformationCode ?? "", ["reliability"] = item.Reliability, ["status"] = item.Status.ToString() });
            return result;
        }

        private static void D12PokerRequestsFromObj(D12PokerRuntimeSnapshot result, Dictionary<string, object> obj)
        {
            foreach (var item in ObjectList(obj, "economyRequests")) result.EconomyRequests.Add(new D12PokerEconomyRequest { SchemaVersion = SaveJson.GetInt(item, "schemaVersion", 1), RequestId = SaveJson.GetStr(item, "requestId", ""), GameId = SaveJson.GetStr(item, "gameId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""), Currency = SaveJson.GetStr(item, "currency", D12PokerIds.Currency), Kind = SaveJson.GetStr(item, "kind", ""), Amount = ReadLong(item, "amount"), Status = ParsePokerStatus(item, "status") });
            foreach (var item in ObjectList(obj, "debtRequests")) result.DebtRequests.Add(new D12PokerDebtRequest { SchemaVersion = SaveJson.GetInt(item, "schemaVersion", 1), RequestId = SaveJson.GetStr(item, "requestId", ""), GameId = SaveJson.GetStr(item, "gameId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""), Principal = ReadLong(item, "principal"), CurrentDebt = ReadLong(item, "currentDebt"), DebtLimit = ReadLong(item, "debtLimit"), Status = ParsePokerStatus(item, "status") });
            foreach (var item in ObjectList(obj, "rewardRequests")) result.RewardRequests.Add(new D12PokerRewardRequest { SchemaVersion = SaveJson.GetInt(item, "schemaVersion", 1), RequestId = SaveJson.GetStr(item, "requestId", ""), RewardId = SaveJson.GetStr(item, "rewardId", ""), GameId = SaveJson.GetStr(item, "gameId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""), RewardTag = SaveJson.GetStr(item, "rewardTag", ""), Currency = SaveJson.GetStr(item, "currency", D12PokerIds.Currency), CurrencyAmount = ReadLong(item, "currencyAmount"), Status = ParsePokerStatus(item, "status") });
            foreach (var item in ObjectList(obj, "relationImpactRequests")) result.RelationImpactRequests.Add(new D12PokerRelationImpactRequest { SchemaVersion = SaveJson.GetInt(item, "schemaVersion", 1), RequestId = SaveJson.GetStr(item, "requestId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), SourceId = SaveJson.GetStr(item, "sourceId", ""), TargetId = SaveJson.GetStr(item, "targetId", ""), RelationType = SaveJson.GetStr(item, "relationType", "trust"), ProposedDelta = SaveJson.GetInt(item, "proposedDelta"), Reason = SaveJson.GetStr(item, "reason", ""), Status = ParsePokerStatus(item, "status") });
            foreach (var item in ObjectList(obj, "informationRequests")) result.InformationRequests.Add(new D12PokerInformationRequest { SchemaVersion = SaveJson.GetInt(item, "schemaVersion", 1), RequestId = SaveJson.GetStr(item, "requestId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), PlayerId = SaveJson.GetStr(item, "playerId", ""), InformationPoolId = SaveJson.GetStr(item, "informationPoolId", ""), InformationCode = SaveJson.GetStr(item, "informationCode", ""), Reliability = SaveJson.GetFloat(item, "reliability"), Status = ParsePokerStatus(item, "status") });
        }

        private static D12PokerRequestStatus ParsePokerStatus(Dictionary<string, object> obj, string key)
        {
            D12PokerRequestStatus status;
            if (!Enum.TryParse(SaveJson.GetStr(obj, key, "Unknown"), true, out status)) status = D12PokerRequestStatus.Unknown;
            return status;
        }

        private static Dictionary<string, object> D11AquacultureToObj(D11AquacultureRuntimeSnapshot runtime)
        {
            var facilities = new List<object>();
            if (runtime.IslandAquaculture != null && runtime.IslandAquaculture.Facilities != null)
                foreach (var facility in runtime.IslandAquaculture.Facilities)
                    if (facility != null) facilities.Add(D11AquacultureFacilityToObj(facility));
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["liveFishHold"] = D11AquacultureFacilityToObj(runtime.LiveFishHold),
                ["islandSchemaVersion"] = runtime.IslandAquaculture == null ? D11AquacultureIds.SchemaVersion : runtime.IslandAquaculture.SchemaVersion,
                ["facilities"] = facilities,
            };
            if (runtime.EventLog != null) result["eventLog"] = EventLogToObj(runtime.EventLog);
            return result;
        }

        private static Dictionary<string, object> D11AquacultureFacilityToObj(D11AquacultureFacilityState facility)
        {
            var fish = new List<object>();
            if (facility != null && facility.Fish != null)
                foreach (var stock in facility.Fish)
                    if (stock != null)
                        fish.Add(new Dictionary<string, object>
                        {
                            ["speciesId"] = stock.SpeciesId ?? "",
                            ["cargoItemId"] = stock.CargoItemId ?? "",
                            ["quantity"] = stock.Quantity,
                            ["unitWeight"] = stock.UnitWeight,
                            ["unitVolume"] = stock.UnitVolume,
                        });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = facility == null ? D11AquacultureIds.SchemaVersion : facility.SchemaVersion,
                ["facilityId"] = facility == null ? "" : facility.FacilityId ?? "",
                ["kind"] = facility == null ? D11AquacultureFacilityKind.LiveFishHold.ToString() : facility.Kind.ToString(),
                ["locationRef"] = facility == null ? "" : facility.LocationRef ?? "",
                ["habitatId"] = facility == null ? "" : facility.HabitatId ?? "",
                ["capacityVolume"] = facility == null ? 0 : facility.CapacityVolume,
                ["unitFishVolume"] = facility == null ? 1 : facility.UnitFishVolume,
                ["unitFishWeight"] = facility == null ? 1 : facility.UnitFishWeight,
                ["waterExchangePerDay"] = facility == null ? 1 : facility.WaterExchangePerDay,
                ["maintenanceLaborPerDay"] = facility == null ? 1 : facility.MaintenanceLaborPerDay,
                ["maxFish"] = facility == null ? 0 : facility.MaxFish,
                ["oxygen"] = facility == null ? 1f : facility.Oxygen,
                ["waterQuality"] = facility == null ? 1f : facility.WaterQuality,
                ["crowding"] = facility == null ? 0f : facility.Crowding,
                ["maintenance"] = facility == null ? 1f : facility.Maintenance,
                ["maintenanceDebt"] = facility == null ? 0f : facility.MaintenanceDebt,
                ["feedStock"] = facility == null ? 0 : facility.FeedStock,
                ["laborStock"] = facility == null ? 0 : facility.LaborStock,
                ["lastWorldDay"] = facility == null ? 0 : facility.LastWorldDay,
                ["lastEventId"] = facility == null ? "" : facility.LastEventId ?? "",
                ["fish"] = fish,
                ["appliedEventKeys"] = StringListToObjects(facility == null ? null : facility.AppliedEventKeys),
            };
        }

        private static D11AquacultureRuntimeSnapshot D11AquacultureFromObj(Dictionary<string, object> obj)
        {
            var result = new D11AquacultureRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D11AquacultureIds.SchemaVersion),
                IslandAquaculture = new D11IslandAquacultureState
                {
                    SchemaVersion = SaveJson.GetInt(obj, "islandSchemaVersion", D11AquacultureIds.SchemaVersion),
                },
            };
            var live = Nested(obj, "liveFishHold");
            if (live != null) result.LiveFishHold.CopyFrom(D11AquacultureFacilityFromObj(live));
            foreach (var facilityObj in ObjectList(obj, "facilities"))
                result.IslandAquaculture.Facilities.Add(D11AquacultureFacilityFromObj(facilityObj));
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            return result;
        }

        private static D11AquacultureFacilityState D11AquacultureFacilityFromObj(Dictionary<string, object> obj)
        {
            var facility = new D11AquacultureFacilityState
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D11AquacultureIds.SchemaVersion),
                FacilityId = SaveJson.GetStr(obj, "facilityId", ""),
                LocationRef = SaveJson.GetStr(obj, "locationRef", ""),
                HabitatId = SaveJson.GetStr(obj, "habitatId", D11AquacultureIds.DefaultHabitatId),
                CapacityVolume = Math.Max(0, SaveJson.GetInt(obj, "capacityVolume")),
                UnitFishVolume = Math.Max(1, SaveJson.GetInt(obj, "unitFishVolume", 1)),
                UnitFishWeight = Math.Max(1, SaveJson.GetInt(obj, "unitFishWeight", 1)),
                WaterExchangePerDay = Math.Max(1, SaveJson.GetInt(obj, "waterExchangePerDay", 1)),
                MaintenanceLaborPerDay = Math.Max(1, SaveJson.GetInt(obj, "maintenanceLaborPerDay", 1)),
                MaxFish = Math.Max(0, SaveJson.GetInt(obj, "maxFish")),
                Oxygen = SaveJson.GetFloat(obj, "oxygen", 1f),
                WaterQuality = SaveJson.GetFloat(obj, "waterQuality", 1f),
                Crowding = SaveJson.GetFloat(obj, "crowding"),
                Maintenance = SaveJson.GetFloat(obj, "maintenance", 1f),
                MaintenanceDebt = SaveJson.GetFloat(obj, "maintenanceDebt"),
                FeedStock = Math.Max(0, SaveJson.GetInt(obj, "feedStock")),
                LaborStock = Math.Max(0, SaveJson.GetInt(obj, "laborStock")),
                LastWorldDay = Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")),
                LastEventId = SaveJson.GetStr(obj, "lastEventId", ""),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "kind", "LiveFishHold"), true, out facility.Kind);
            foreach (var stockObj in ObjectList(obj, "fish"))
                facility.Fish.Add(new D11FishStockState
                {
                    SpeciesId = SaveJson.GetStr(stockObj, "speciesId", D11AquacultureIds.FishSpeciesId),
                    CargoItemId = SaveJson.GetStr(stockObj, "cargoItemId", D11AquacultureIds.FishCargoItemId),
                    Quantity = Math.Max(0, SaveJson.GetInt(stockObj, "quantity")),
                    UnitWeight = Math.Max(1, SaveJson.GetInt(stockObj, "unitWeight", 1)),
                    UnitVolume = Math.Max(1, SaveJson.GetInt(stockObj, "unitVolume", 1)),
                });
            ReadStringListObject(facility.AppliedEventKeys, obj, "appliedEventKeys");
            return facility;
        }

        private static Dictionary<string, object> D12CompatToObj3(D12RuntimeSnapshot d12)
        {
            d12.Validate();
            var map = d12.MapDiscovery;
            var features = new List<object>();
            foreach (var feature in map.Features)
            {
                var evidence = new List<object>();
                foreach (var item in feature.Evidence)
                    evidence.Add(new Dictionary<string, object>
                    {
                        ["evidenceId"] = item.EvidenceId ?? "", ["source"] = item.Source.ToString(),
                        ["kind"] = item.Kind.ToString(), ["sourceRef"] = item.SourceRef ?? "",
                        ["evidenceRef"] = item.EvidenceRef ?? "", ["summary"] = item.Summary ?? "",
                        ["locationRef"] = item.LocationRef ?? "", ["toolId"] = item.ToolId ?? "",
                        ["eventId"] = item.EventId ?? "", ["worldDay"] = item.WorldDay,
                        ["saveRevision"] = item.SaveRevision, ["strength"] = item.Strength,
                    });
                features.Add(new Dictionary<string, object>
                {
                    ["featureId"] = feature.FeatureId ?? "", ["stage"] = feature.Stage.ToString(),
                    ["credibility"] = feature.Credibility, ["lastDiscoveredDay"] = feature.LastDiscoveredDay,
                    ["lastSource"] = feature.LastSource ?? "", ["lastEvidenceId"] = feature.LastEvidenceId ?? "",
                    ["hasMeasuredDepth"] = feature.HasMeasuredDepth, ["measuredDepthMeters"] = feature.MeasuredDepthMeters,
                    ["evidenceIds"] = StringListToObjects(feature.EvidenceIds), ["evidence"] = evidence,
                });
            }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = d12.SchemaVersion,
                ["mapDiscovery"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = map.SchemaVersion, ["lastWorldDay"] = map.LastWorldDay,
                    ["revision"] = map.Revision, ["lastEventId"] = map.LastEventId ?? "", ["features"] = features,
                },
            };
        }

        private static D12RuntimeSnapshot D12CompatFromObj3(Dictionary<string, object> obj)
        {
            var result = new D12RuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12RuntimeSnapshot.CurrentSchemaVersion) };
            var mapObj = Nested(obj, "mapDiscovery");
            if (mapObj != null)
            {
                result.MapDiscovery = new D12MapDiscoverySnapshot
                {
                    SchemaVersion = SaveJson.GetInt(mapObj, "schemaVersion", D12MapDiscoverySnapshot.CurrentSchemaVersion),
                    LastWorldDay = Math.Max(0, SaveJson.GetInt(mapObj, "lastWorldDay")),
                    Revision = Math.Max(0L, ReadLong(mapObj, "revision")),
                    LastEventId = SaveJson.GetStr(mapObj, "lastEventId", ""),
                };
                foreach (var item in ObjectList(mapObj, "features"))
                {
                    var feature = new D12MapFeatureState
                    {
                        FeatureId = SaveJson.GetStr(item, "featureId", ""),
                        Credibility = SaveJson.GetFloat(item, "credibility"),
                        LastDiscoveredDay = Math.Max(0, SaveJson.GetInt(item, "lastDiscoveredDay")),
                        LastSource = SaveJson.GetStr(item, "lastSource", ""),
                        LastEvidenceId = SaveJson.GetStr(item, "lastEvidenceId", ""),
                        HasMeasuredDepth = ReadBool(item, "hasMeasuredDepth"),
                        MeasuredDepthMeters = Math.Max(0f, SaveJson.GetFloat(item, "measuredDepthMeters")),
                    };
                    Enum.TryParse(SaveJson.GetStr(item, "stage", "Unknown"), true, out feature.Stage);
                    ReadStringListObject(feature.EvidenceIds, item, "evidenceIds");
                    foreach (var evidenceObj in ObjectList(item, "evidence"))
                    {
                        var evidence = new D12ChartEvidence
                        {
                            EvidenceId = SaveJson.GetStr(evidenceObj, "evidenceId", ""),
                            SourceRef = SaveJson.GetStr(evidenceObj, "sourceRef", "legacy"),
                            EvidenceRef = SaveJson.GetStr(evidenceObj, "evidenceRef", "legacy"),
                            Summary = SaveJson.GetStr(evidenceObj, "summary", "legacy evidence"),
                            LocationRef = SaveJson.GetStr(evidenceObj, "locationRef", "unknown"),
                            ToolId = SaveJson.GetStr(evidenceObj, "toolId", ""), EventId = SaveJson.GetStr(evidenceObj, "eventId", ""),
                            WorldDay = Math.Max(0, SaveJson.GetInt(evidenceObj, "worldDay")),
                            SaveRevision = Math.Max(0L, ReadLong(evidenceObj, "saveRevision")),
                            Strength = Math.Max(0f, Math.Min(1f, SaveJson.GetFloat(evidenceObj, "strength"))),
                        };
                        Enum.TryParse(SaveJson.GetStr(evidenceObj, "source", "Exploration"), true, out evidence.Source);
                        Enum.TryParse(SaveJson.GetStr(evidenceObj, "kind", "DirectSight"), true, out evidence.Kind);
                        feature.Evidence.Add(evidence);
                    }
                    result.MapDiscovery.Features.Add(feature);
                }
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D11ToObj(D11WhaleDolphinRuntimeSnapshot d11)
        {
            d11.Validate();
            var encounters = new List<object>();
            foreach (var encounter in d11.Encounters)
                encounters.Add(new Dictionary<string, object>
                {
                    ["schemaVersion"] = encounter.SchemaVersion,
                    ["encounterId"] = encounter.EncounterId ?? "",
                    ["speciesId"] = encounter.SpeciesId ?? "",
                    ["speciesKind"] = encounter.SpeciesKind.ToString(),
                    ["routeRef"] = encounter.RouteRef ?? "",
                    ["habitatId"] = encounter.HabitatId ?? "open_sea",
                    ["worldX"] = encounter.WorldX,
                    ["worldZ"] = encounter.WorldZ,
                    ["migrationDirectionDegrees"] = encounter.MigrationDirectionDegrees,
                    ["lastMigratedDay"] = encounter.LastMigratedDay,
                    ["ecologyStress"] = encounter.EcologyStress,
                    ["protectedPopulation"] = encounter.ProtectedPopulation,
                    ["isStranded"] = encounter.IsStranded,
                    ["observed"] = encounter.Observed,
                    ["escorted"] = encounter.Escorted,
                    ["rescued"] = encounter.Rescued,
                    ["whalingAttempted"] = encounter.WhalingAttempted,
                    ["whalingResolved"] = encounter.WhalingResolved,
                    ["trackingAbandoned"] = encounter.TrackingAbandoned,
                    ["lastEventId"] = encounter.LastEventId ?? "",
                    ["causeEventIds"] = StringListToObjects(encounter.CauseEventIds),
                });
            var records = new List<object>();
            foreach (var record in d11.RouteRecords) records.Add(D11RecordToObj(record));
            var rewards = new List<object>();
            foreach (var reward in d11.RewardRequests) rewards.Add(new Dictionary<string, object>
            {
                ["schemaVersion"] = reward.SchemaVersion, ["requestId"] = reward.RequestId ?? "",
                ["rewardKind"] = reward.RewardKind ?? "", ["ownerSystem"] = reward.OwnerSystem ?? "RewardSystem",
                ["sourceEventId"] = reward.SourceEventId ?? "", ["status"] = reward.Status ?? "Pending",
                ["quantity"] = reward.Quantity, ["description"] = reward.Description ?? "",
            });
            var consequences = new List<object>();
            foreach (var consequence in d11.ConsequenceRequests) consequences.Add(new Dictionary<string, object>
            {
                ["schemaVersion"] = consequence.SchemaVersion, ["requestId"] = consequence.RequestId ?? "",
                ["ownerSystem"] = consequence.OwnerSystem ?? "", ["sourceEventId"] = consequence.SourceEventId ?? "",
                ["consequenceKind"] = consequence.ConsequenceKind ?? "", ["ecologyStressDelta"] = consequence.EcologyStressDelta,
                ["shipRiskDelta"] = consequence.ShipRiskDelta, ["crewRiskDelta"] = consequence.CrewRiskDelta,
                ["socialReputationDelta"] = consequence.SocialReputationDelta,
                ["legalConsequenceCode"] = consequence.LegalConsequenceCode ?? "none", ["status"] = consequence.Status ?? "Pending",
            });
            var questFacts = new List<object>();
            foreach (var fact in d11.QuestFacts) questFacts.Add(new Dictionary<string, object>
            {
                ["schemaVersion"] = fact.SchemaVersion, ["factId"] = fact.FactId ?? "",
                ["sourceEventId"] = fact.SourceEventId ?? "", ["factKind"] = fact.FactKind ?? "",
                ["status"] = fact.Status ?? "Pending",
            });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = d11.SchemaVersion,
                ["worldDay"] = d11.WorldDay,
                ["revision"] = d11.Revision,
                ["lastEventId"] = d11.LastEventId ?? "",
                ["appliedEventKeys"] = StringListToObjects(d11.AppliedEventKeys),
                ["encounters"] = encounters,
                ["routeRecords"] = records,
                ["rewardRequests"] = rewards,
                ["consequenceRequests"] = consequences,
                ["questFacts"] = questFacts,
            };
        }

        private static Dictionary<string, object> D11RecordToObj(D11RouteRecord record)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = record.SchemaVersion, ["recordId"] = record.RecordId ?? "",
                ["encounterId"] = record.EncounterId ?? "", ["action"] = record.Action.ToString(),
                ["status"] = record.Status.ToString(), ["code"] = record.Code ?? "",
                ["message"] = record.Message ?? "", ["eventId"] = record.EventId ?? "",
                ["worldDay"] = record.WorldDay, ["saveRevision"] = record.SaveRevision, ["riskRoll"] = record.RiskRoll,
                ["riskThreshold"] = record.RiskThreshold, ["ecologyStressDelta"] = record.EcologyStressDelta,
                ["socialReputationDelta"] = record.SocialReputationDelta,
                ["legalConsequenceCode"] = record.LegalConsequenceCode ?? "none",
                ["rewardRequestId"] = record.RewardRequestId ?? "", ["recoveryRouteId"] = record.RecoveryRouteId ?? "",
                ["historicalReviewTag"] = record.HistoricalReviewTag ?? D11WhaleDolphinSchema.HistoricalReviewTag,
                ["sensitiveContentReview"] = record.SensitiveContentReview ?? D11WhaleDolphinSchema.SensitiveContentReview,
                ["causeEventIds"] = StringListToObjects(record.CauseEventIds),
            };
        }

        private static D11WhaleDolphinRuntimeSnapshot D11FromObj(Dictionary<string, object> obj)
        {
            var result = new D11WhaleDolphinRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D11WhaleDolphinRuntimeSnapshot.CurrentSchemaVersion),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = SaveJson.GetStr(obj, "lastEventId", ""),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "encounters"))
            {
                var encounter = new D11MarineEncounter
                {
                    SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D11WhaleDolphinSchema.CurrentVersion),
                    EncounterId = SaveJson.GetStr(item, "encounterId", ""),
                    SpeciesId = SaveJson.GetStr(item, "speciesId", ""),
                    RouteRef = SaveJson.GetStr(item, "routeRef", "open_sea_migration"),
                    HabitatId = SaveJson.GetStr(item, "habitatId", "open_sea"),
                    WorldX = SaveJson.GetFloat(item, "worldX"), WorldZ = SaveJson.GetFloat(item, "worldZ"),
                    MigrationDirectionDegrees = SaveJson.GetInt(item, "migrationDirectionDegrees"),
                    LastMigratedDay = Math.Max(0, SaveJson.GetInt(item, "lastMigratedDay")),
                    EcologyStress = SaveJson.GetFloat(item, "ecologyStress"),
                    ProtectedPopulation = ReadBool(item, "protectedPopulation"), IsStranded = ReadBool(item, "isStranded"),
                    Observed = ReadBool(item, "observed"), Escorted = ReadBool(item, "escorted"),
                    Rescued = ReadBool(item, "rescued"), WhalingAttempted = ReadBool(item, "whalingAttempted"),
                    WhalingResolved = ReadBool(item, "whalingResolved"), TrackingAbandoned = ReadBool(item, "trackingAbandoned"),
                    LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                };
                Enum.TryParse(SaveJson.GetStr(item, "speciesKind", "Unknown"), true, out encounter.SpeciesKind);
                ReadStringListObject(encounter.CauseEventIds, item, "causeEventIds");
                result.Encounters.Add(encounter);
            }
            foreach (var item in ObjectList(obj, "routeRecords")) result.RouteRecords.Add(D11RecordFromObj(item));
            foreach (var item in ObjectList(obj, "rewardRequests")) result.RewardRequests.Add(new D11RewardRequest
            {
                SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D11WhaleDolphinSchema.CurrentVersion),
                RequestId = SaveJson.GetStr(item, "requestId", ""), RewardKind = SaveJson.GetStr(item, "rewardKind", ""),
                OwnerSystem = SaveJson.GetStr(item, "ownerSystem", "RewardSystem"), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""),
                Status = SaveJson.GetStr(item, "status", "Pending"), Quantity = Math.Max(0, SaveJson.GetInt(item, "quantity", 1)),
                Description = SaveJson.GetStr(item, "description", ""),
            });
            foreach (var item in ObjectList(obj, "consequenceRequests")) result.ConsequenceRequests.Add(new D11ConsequenceRequest
            {
                SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D11WhaleDolphinSchema.CurrentVersion),
                RequestId = SaveJson.GetStr(item, "requestId", ""), OwnerSystem = SaveJson.GetStr(item, "ownerSystem", ""),
                SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), ConsequenceKind = SaveJson.GetStr(item, "consequenceKind", ""),
                EcologyStressDelta = SaveJson.GetFloat(item, "ecologyStressDelta"),
                ShipRiskDelta = SaveJson.GetFloat(item, "shipRiskDelta"),
                CrewRiskDelta = SaveJson.GetInt(item, "crewRiskDelta"),
                SocialReputationDelta = SaveJson.GetInt(item, "socialReputationDelta"),
                LegalConsequenceCode = SaveJson.GetStr(item, "legalConsequenceCode", "none"), Status = SaveJson.GetStr(item, "status", "Pending"),
            });
            foreach (var item in ObjectList(obj, "questFacts")) result.QuestFacts.Add(new D11QuestFactRequest
            {
                SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D11WhaleDolphinSchema.CurrentVersion),
                FactId = SaveJson.GetStr(item, "factId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""),
                FactKind = SaveJson.GetStr(item, "factKind", ""), Status = SaveJson.GetStr(item, "status", "Pending"),
            });
            result.Validate();
            return result;
        }

        private static D11RouteRecord D11RecordFromObj(Dictionary<string, object> item)
        {
            var record = new D11RouteRecord
            {
                SchemaVersion = SaveJson.GetInt(item, "schemaVersion", D11WhaleDolphinSchema.CurrentVersion),
                RecordId = SaveJson.GetStr(item, "recordId", ""), EncounterId = SaveJson.GetStr(item, "encounterId", ""),
                Code = SaveJson.GetStr(item, "code", ""), Message = SaveJson.GetStr(item, "message", ""),
                EventId = SaveJson.GetStr(item, "eventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                RiskRoll = SaveJson.GetInt(item, "riskRoll"), RiskThreshold = SaveJson.GetInt(item, "riskThreshold"),
                EcologyStressDelta = SaveJson.GetFloat(item, "ecologyStressDelta"), SocialReputationDelta = SaveJson.GetInt(item, "socialReputationDelta"),
                LegalConsequenceCode = SaveJson.GetStr(item, "legalConsequenceCode", "none"), RewardRequestId = SaveJson.GetStr(item, "rewardRequestId", ""),
                RecoveryRouteId = SaveJson.GetStr(item, "recoveryRouteId", ""),
                HistoricalReviewTag = SaveJson.GetStr(item, "historicalReviewTag", D11WhaleDolphinSchema.HistoricalReviewTag),
                SensitiveContentReview = SaveJson.GetStr(item, "sensitiveContentReview", D11WhaleDolphinSchema.SensitiveContentReview),
            };
            Enum.TryParse(SaveJson.GetStr(item, "action", "Unknown"), true, out record.Action);
            Enum.TryParse(SaveJson.GetStr(item, "status", "Unknown"), true, out record.Status);
            ReadStringListObject(record.CauseEventIds, item, "causeEventIds");
            return record;
        }

        private static Dictionary<string, object> D11PetToObj(D11PetRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["nextRevision"] = runtime.NextRevision,
                ["crewReactions"] = new List<object>(),
            };
            if (runtime.Pet != null)
            {
                var pet = runtime.Pet;
                result["pet"] = new Dictionary<string, object>
                {
                    ["petId"] = pet.PetId ?? "", ["definitionId"] = pet.DefinitionId ?? "",
                    ["kind"] = pet.Kind.ToString(), ["breed"] = pet.Breed ?? "",
                    ["sourceRef"] = pet.SourceRef ?? "", ["sourceTag"] = pet.SourceTag ?? "",
                    ["adoptionDay"] = pet.AdoptionDay, ["lastUpdatedDay"] = pet.LastUpdatedDay,
                    ["lostSinceDay"] = pet.LostSinceDay, ["lostSaveRevision"] = pet.LostSaveRevision,
                    ["quarantineStartedDay"] = pet.QuarantineStartedDay,
                    ["quarantineDaysRemaining"] = pet.QuarantineDaysRemaining,
                    ["health"] = pet.Health, ["hunger"] = pet.Hunger,
                    ["cleanliness"] = pet.Cleanliness, ["fear"] = pet.Fear,
                    ["attachment"] = pet.Attachment, ["lifeState"] = pet.LifeState.ToString(),
                    ["location"] = pet.Location.ToString(), ["careState"] = pet.CareState.ToString(),
                    ["mouseCaughtCount"] = pet.MouseCaughtCount, ["warningCount"] = pet.WarningCount,
                    ["lastCauseEventId"] = pet.LastCauseEventId ?? "",
                    ["lastSaveRevision"] = pet.LastSaveRevision,
                };
            }
            var reactions = (List<object>)result["crewReactions"];
            if (runtime.CrewReactions != null)
                foreach (var reaction in runtime.CrewReactions)
                    if (reaction != null) reactions.Add(new Dictionary<string, object>
                    {
                        ["crewId"] = reaction.CrewId ?? "", ["crewRole"] = reaction.CrewRole ?? "",
                        ["petId"] = reaction.PetId ?? "", ["reactionId"] = reaction.ReactionId ?? "",
                        ["expressionKey"] = reaction.ExpressionKey ?? "",
                        ["causeEventId"] = reaction.CauseEventId ?? "", ["worldDay"] = reaction.WorldDay,
                        ["saveRevision"] = reaction.SaveRevision,
                    });
            if (runtime.EventLog != null) result["eventLog"] = EventLogToObj(runtime.EventLog);
            return result;
        }

        private static D11PetRuntimeSnapshot D11PetFromObj(Dictionary<string, object> obj)
        {
            var result = new D11PetRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", 1),
                NextRevision = Math.Max(0L, ReadLong(obj, "nextRevision")),
            };
            var petObj = Nested(obj, "pet");
            if (petObj != null)
            {
                var pet = new D11PetState
                {
                    PetId = SaveJson.GetStr(petObj, "petId", ""),
                    DefinitionId = SaveJson.GetStr(petObj, "definitionId", ""),
                    Breed = SaveJson.GetStr(petObj, "breed", ""),
                    SourceRef = SaveJson.GetStr(petObj, "sourceRef", ""),
                    SourceTag = SaveJson.GetStr(petObj, "sourceTag", ""),
                    AdoptionDay = Math.Max(0, SaveJson.GetInt(petObj, "adoptionDay")),
                    LastUpdatedDay = Math.Max(0, SaveJson.GetInt(petObj, "lastUpdatedDay")),
                    LostSinceDay = SaveJson.GetInt(petObj, "lostSinceDay", -1),
                    LostSaveRevision = Math.Max(0L, ReadLong(petObj, "lostSaveRevision")),
                    QuarantineStartedDay = SaveJson.GetInt(petObj, "quarantineStartedDay", -1),
                    QuarantineDaysRemaining = Math.Max(0, SaveJson.GetInt(petObj, "quarantineDaysRemaining")),
                    Health = (int)NumericConfiguration.ClampHealth(SaveJson.GetInt(petObj, "health", 100)),
                    Hunger = ClampPetPercent(SaveJson.GetInt(petObj, "hunger")),
                    Cleanliness = ClampPetPercent(SaveJson.GetInt(petObj, "cleanliness", 100)),
                    Fear = ClampPetPercent(SaveJson.GetInt(petObj, "fear")),
                    Attachment = ClampPetPercent(SaveJson.GetInt(petObj, "attachment")),
                    MouseCaughtCount = Math.Max(0, SaveJson.GetInt(petObj, "mouseCaughtCount")),
                    WarningCount = Math.Max(0, SaveJson.GetInt(petObj, "warningCount")),
                    LastCauseEventId = SaveJson.GetStr(petObj, "lastCauseEventId", ""),
                    LastSaveRevision = Math.Max(0L, ReadLong(petObj, "lastSaveRevision")),
                };
                Enum.TryParse(SaveJson.GetStr(petObj, "kind", "Cat"), true, out pet.Kind);
                Enum.TryParse(SaveJson.GetStr(petObj, "lifeState", "Unadopted"), true, out pet.LifeState);
                Enum.TryParse(SaveJson.GetStr(petObj, "location", "Unknown"), true, out pet.Location);
                Enum.TryParse(SaveJson.GetStr(petObj, "careState", "Content"), true, out pet.CareState);
                result.Pet = pet;
            }
            foreach (var item in ObjectList(obj, "crewReactions"))
                result.CrewReactions.Add(new D11PetCrewReaction
                {
                    CrewId = SaveJson.GetStr(item, "crewId", ""),
                    CrewRole = SaveJson.GetStr(item, "crewRole", ""),
                    PetId = SaveJson.GetStr(item, "petId", ""),
                    ReactionId = SaveJson.GetStr(item, "reactionId", ""),
                    ExpressionKey = SaveJson.GetStr(item, "expressionKey", ""),
                    CauseEventId = SaveJson.GetStr(item, "causeEventId", ""),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                });
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate();
            return result;
        }

        private static int ClampPetPercent(int value)
        {
            return Math.Max(0, Math.Min(100, value));
        }

        private static Dictionary<string, object> D10ToObj(D10RelationRuntimeSnapshot d10)
        {
            d10.Validate();
            var relations = new List<object>();
            foreach (var relation in d10.Relations)
            {
                relations.Add(new Dictionary<string, object>
                {
                    ["schemaVersion"] = relation.SchemaVersion,
                    ["id"] = relation.Id ?? string.Empty,
                    ["sourceId"] = relation.SourceId ?? string.Empty,
                    ["targetId"] = relation.TargetId ?? string.Empty,
                    ["phase"] = relation.Phase.ToString(),
                    ["romanceConsent"] = relation.RomanceConsent.ToString(),
                    ["affinity"] = relation.Affinity,
                    ["trust"] = relation.Trust,
                    ["attachment"] = relation.Attachment,
                    ["respect"] = relation.Respect,
                    ["jealousy"] = relation.Jealousy,
                    ["conflictScore"] = relation.ConflictScore,
                    ["commitment"] = relation.Commitment,
                    ["conflictActive"] = relation.ConflictActive,
                    ["conflictHistory"] = relation.ConflictHistory,
                    ["lockedByStory"] = relation.LockedByStory,
                    ["visibility"] = relation.Visibility.ToString(),
                    ["lastChangedDay"] = relation.LastChangedDay,
                    ["causeEventIds"] = StringListToObjects(relation.CauseEventIds),
                });
            }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = d10.SchemaVersion,
                ["worldDay"] = d10.WorldDay,
                ["revision"] = d10.Revision,
                ["lastEventId"] = d10.LastEventId ?? string.Empty,
                ["appliedEventKeys"] = StringListToObjects(d10.AppliedEventKeys),
                ["relations"] = relations,
                ["canonicalGraph"] = RelationGraphToObj(d10.CanonicalGraph),
            };
        }

        private static Dictionary<string, object> D10IdentityToObj(D10PlayerIdentityState identity)
        {
            identity.Validate();
            var documents = new List<object>();
            foreach (var document in identity.Documents)
                if (document != null) documents.Add(D10IdentityDocumentToObj(document));
            var history = new List<object>();
            foreach (var entry in identity.IdentityHistory)
                if (entry != null) history.Add(D10IdentityHistoryToObj(entry));
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = identity.SchemaVersion,
                ["playerId"] = identity.PlayerId ?? "",
                ["name"] = identity.Name ?? "",
                ["genderExpressionKey"] = identity.GenderExpressionKey ?? "",
                ["pronounKeys"] = StringListToObjects(identity.PronounKeys),
                ["title"] = D10LocalizedTextToObj(identity.Title),
                ["ageBandKey"] = identity.AgeBandKey ?? "",
                ["cultureOriginKey"] = identity.CultureOriginKey ?? "",
                ["identityNote"] = D10LocalizedTextToObj(identity.IdentityNote),
                ["preservedHistory"] = D10PreservedHistoryToObj(identity.PreservedHistory),
                ["documents"] = documents,
                ["identityHistory"] = history,
            };
        }

        private static Dictionary<string, object> D10LocalizedTextToObj(D10LocalizedText text)
        {
            var value = text ?? new D10LocalizedText();
            value.Validate("localized_text");
            return new Dictionary<string, object>
            {
                ["key"] = value.Key ?? "",
                ["zh"] = value.Zh ?? "",
                ["en"] = value.En ?? "",
            };
        }

        private static Dictionary<string, object> D10PreservedHistoryToObj(D10IdentityHistoryPreservationContext history)
        {
            var value = history ?? new D10IdentityHistoryPreservationContext();
            return new Dictionary<string, object>
            {
                ["legalHistoryRefs"] = StringListToObjects(value.LegalHistoryRefs),
                ["debtHistoryRefs"] = StringListToObjects(value.DebtHistoryRefs),
                ["reputationHistoryRefs"] = StringListToObjects(value.ReputationHistoryRefs),
                ["relationHistoryRefs"] = StringListToObjects(value.RelationHistoryRefs),
            };
        }

        private static Dictionary<string, object> D10IdentityDocumentToObj(D10IdentityDocumentRecord document)
        {
            document.Validate();
            return new Dictionary<string, object>
            {
                ["documentId"] = document.DocumentId ?? "",
                ["documentTypeKey"] = document.DocumentTypeKey ?? "",
                ["issuerRef"] = document.IssuerRef ?? "",
                ["statusKey"] = document.StatusKey ?? "",
                ["issuedWorldDay"] = document.IssuedWorldDay,
                ["expiresWorldDay"] = document.ExpiresWorldDay,
                ["recordText"] = D10LocalizedTextToObj(document.RecordText),
            };
        }

        private static Dictionary<string, object> D10IdentityHistoryToObj(D10IdentityHistoryEntry entry)
        {
            return new Dictionary<string, object>
            {
                ["historyId"] = entry.HistoryId ?? "",
                ["worldDay"] = entry.WorldDay,
                ["changeKind"] = entry.ChangeKind ?? "",
                ["changeReasonKey"] = entry.ChangeReasonKey ?? "",
                ["previousName"] = entry.PreviousName ?? "",
                ["newName"] = entry.NewName ?? "",
                ["previousGenderExpressionKey"] = entry.PreviousGenderExpressionKey ?? "",
                ["newGenderExpressionKey"] = entry.NewGenderExpressionKey ?? "",
                ["previousPronounKeys"] = StringListToObjects(entry.PreviousPronounKeys),
                ["newPronounKeys"] = StringListToObjects(entry.NewPronounKeys),
                ["previousTitleKey"] = entry.PreviousTitleKey ?? "",
                ["newTitleKey"] = entry.NewTitleKey ?? "",
                ["previousAgeBandKey"] = entry.PreviousAgeBandKey ?? "",
                ["newAgeBandKey"] = entry.NewAgeBandKey ?? "",
                ["previousCultureOriginKey"] = entry.PreviousCultureOriginKey ?? "",
                ["newCultureOriginKey"] = entry.NewCultureOriginKey ?? "",
                ["previousAppearanceLoadoutId"] = entry.PreviousAppearanceLoadoutId ?? "",
                ["newAppearanceLoadoutId"] = entry.NewAppearanceLoadoutId ?? "",
                ["preservedLegalHistoryRefs"] = StringListToObjects(entry.PreservedLegalHistoryRefs),
                ["preservedDebtHistoryRefs"] = StringListToObjects(entry.PreservedDebtHistoryRefs),
                ["preservedReputationHistoryRefs"] = StringListToObjects(entry.PreservedReputationHistoryRefs),
                ["preservedRelationHistoryRefs"] = StringListToObjects(entry.PreservedRelationHistoryRefs),
                ["note"] = D10LocalizedTextToObj(entry.Note),
            };
        }

        private static D10PlayerIdentityState D10IdentityFromObj(Dictionary<string, object> obj)
        {
            var result = new D10PlayerIdentityState
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D10PlayerIdentityState.CurrentSchemaVersion),
                PlayerId = SaveJson.GetStr(obj, "playerId", ""),
                Name = SaveJson.GetStr(obj, "name", "无名船长"),
                GenderExpressionKey = SaveJson.GetStr(obj, "genderExpressionKey", "gender_expression_unspecified"),
                Title = D10LocalizedTextFromObj(Nested(obj, "title")),
                AgeBandKey = SaveJson.GetStr(obj, "ageBandKey", "age_band_adult"),
                CultureOriginKey = SaveJson.GetStr(obj, "cultureOriginKey", "culture_origin_caribbean_mixed"),
                IdentityNote = D10LocalizedTextFromObj(Nested(obj, "identityNote")),
                PreservedHistory = D10PreservedHistoryFromObj(Nested(obj, "preservedHistory")),
                Documents = new List<D10IdentityDocumentRecord>(),
                IdentityHistory = new List<D10IdentityHistoryEntry>(),
            };
            ReadStringListObject(result.PronounKeys, obj, "pronounKeys");
            foreach (var documentObj in ObjectList(obj, "documents"))
                result.Documents.Add(D10IdentityDocumentFromObj(documentObj));
            foreach (var historyObj in ObjectList(obj, "identityHistory"))
                result.IdentityHistory.Add(D10IdentityHistoryFromObj(historyObj));
            if (string.IsNullOrEmpty(result.PlayerId)) result.PlayerId = "player:unknown";
            result.Validate();
            return result;
        }

        private static D10LocalizedText D10LocalizedTextFromObj(Dictionary<string, object> obj)
        {
            if (obj == null) return new D10LocalizedText();
            return new D10LocalizedText(
                SaveJson.GetStr(obj, "key", ""),
                SaveJson.GetStr(obj, "zh", ""),
                SaveJson.GetStr(obj, "en", ""));
        }

        private static D10IdentityHistoryPreservationContext D10PreservedHistoryFromObj(Dictionary<string, object> obj)
        {
            var result = new D10IdentityHistoryPreservationContext();
            if (obj == null) return result;
            ReadStringListObject(result.LegalHistoryRefs, obj, "legalHistoryRefs");
            ReadStringListObject(result.DebtHistoryRefs, obj, "debtHistoryRefs");
            ReadStringListObject(result.ReputationHistoryRefs, obj, "reputationHistoryRefs");
            ReadStringListObject(result.RelationHistoryRefs, obj, "relationHistoryRefs");
            return result;
        }

        private static D10IdentityDocumentRecord D10IdentityDocumentFromObj(Dictionary<string, object> obj)
        {
            var result = new D10IdentityDocumentRecord
            {
                DocumentId = SaveJson.GetStr(obj, "documentId", ""),
                DocumentTypeKey = SaveJson.GetStr(obj, "documentTypeKey", "legacy.identity_record"),
                IssuerRef = SaveJson.GetStr(obj, "issuerRef", ""),
                StatusKey = SaveJson.GetStr(obj, "statusKey", "active"),
                IssuedWorldDay = Math.Max(0, SaveJson.GetInt(obj, "issuedWorldDay")),
                ExpiresWorldDay = Math.Max(0, SaveJson.GetInt(obj, "expiresWorldDay")),
                RecordText = D10LocalizedTextFromObj(Nested(obj, "recordText")),
            };
            result.Validate();
            return result;
        }

        private static D10IdentityHistoryEntry D10IdentityHistoryFromObj(Dictionary<string, object> obj)
        {
            var result = new D10IdentityHistoryEntry
            {
                HistoryId = SaveJson.GetStr(obj, "historyId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                ChangeKind = SaveJson.GetStr(obj, "changeKind", "identity_change"),
                ChangeReasonKey = SaveJson.GetStr(obj, "changeReasonKey", "identity.changed"),
                PreviousName = SaveJson.GetStr(obj, "previousName", ""),
                NewName = SaveJson.GetStr(obj, "newName", ""),
                PreviousGenderExpressionKey = SaveJson.GetStr(obj, "previousGenderExpressionKey", ""),
                NewGenderExpressionKey = SaveJson.GetStr(obj, "newGenderExpressionKey", ""),
                PreviousTitleKey = SaveJson.GetStr(obj, "previousTitleKey", ""),
                NewTitleKey = SaveJson.GetStr(obj, "newTitleKey", ""),
                PreviousAgeBandKey = SaveJson.GetStr(obj, "previousAgeBandKey", ""),
                NewAgeBandKey = SaveJson.GetStr(obj, "newAgeBandKey", ""),
                PreviousCultureOriginKey = SaveJson.GetStr(obj, "previousCultureOriginKey", ""),
                NewCultureOriginKey = SaveJson.GetStr(obj, "newCultureOriginKey", ""),
                PreviousAppearanceLoadoutId = SaveJson.GetStr(obj, "previousAppearanceLoadoutId", ""),
                NewAppearanceLoadoutId = SaveJson.GetStr(obj, "newAppearanceLoadoutId", ""),
                Note = D10LocalizedTextFromObj(Nested(obj, "note")),
            };
            ReadStringListObject(result.PreviousPronounKeys, obj, "previousPronounKeys");
            ReadStringListObject(result.NewPronounKeys, obj, "newPronounKeys");
            ReadStringListObject(result.PreservedLegalHistoryRefs, obj, "preservedLegalHistoryRefs");
            ReadStringListObject(result.PreservedDebtHistoryRefs, obj, "preservedDebtHistoryRefs");
            ReadStringListObject(result.PreservedReputationHistoryRefs, obj, "preservedReputationHistoryRefs");
            ReadStringListObject(result.PreservedRelationHistoryRefs, obj, "preservedRelationHistoryRefs");
            return result;
        }

        private static D10RelationRuntimeSnapshot D10FromObj(Dictionary<string, object> obj)
        {
            var result = new D10RelationRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D10RelationRuntimeSnapshot.CurrentSchemaVersion),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = SaveJson.GetStr(obj, "lastEventId", string.Empty),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var relationObj in ObjectList(obj, "relations"))
            {
                var relation = new D10RelationEdge
                {
                    SchemaVersion = SaveJson.GetInt(relationObj, "schemaVersion", D10RelationEdge.CurrentSchemaVersion),
                    Id = SaveJson.GetStr(relationObj, "id", string.Empty),
                    SourceId = SaveJson.GetStr(relationObj, "sourceId", string.Empty),
                    TargetId = SaveJson.GetStr(relationObj, "targetId", string.Empty),
                    Affinity = SaveJson.GetInt(relationObj, "affinity"),
                    Trust = SaveJson.GetInt(relationObj, "trust"),
                    Attachment = SaveJson.GetInt(relationObj, "attachment"),
                    Respect = SaveJson.GetInt(relationObj, "respect"),
                    Jealousy = SaveJson.GetInt(relationObj, "jealousy"),
                    ConflictScore = SaveJson.GetInt(relationObj, "conflictScore"),
                    Commitment = SaveJson.GetInt(relationObj, "commitment"),
                    ConflictActive = ReadBool(relationObj, "conflictActive"),
                    ConflictHistory = ReadBool(relationObj, "conflictHistory"),
                    LockedByStory = ReadBool(relationObj, "lockedByStory"),
                    LastChangedDay = SaveJson.GetInt(relationObj, "lastChangedDay"),
                };
                Enum.TryParse(SaveJson.GetStr(relationObj, "phase", "Acquaintance"), true, out relation.Phase);
                Enum.TryParse(SaveJson.GetStr(relationObj, "romanceConsent", "Undecided"), true, out relation.RomanceConsent);
                Enum.TryParse(SaveJson.GetStr(relationObj, "visibility", "Private"), true, out relation.Visibility);
                ReadStringListObject(relation.CauseEventIds, relationObj, "causeEventIds");
                relation.Validate();
                result.Relations.Add(relation);
            }
            var canonical = Nested(obj, "canonicalGraph");
            if (canonical != null) result.CanonicalGraph = RelationGraphFromObj(canonical);
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> RelationGraphToObj(RelationGraphSnapshot graph)
        {
            if (graph == null) return null;
            graph.Validate();
            var edges = new List<object>();
            foreach (var edge in graph.Edges)
            {
                edges.Add(new Dictionary<string, object>
                {
                    ["schemaVersion"] = edge.SchemaVersion,
                    ["sourceId"] = edge.SourceId ?? string.Empty,
                    ["targetId"] = edge.TargetId ?? string.Empty,
                    ["relationType"] = edge.RelationType.ToString(),
                    ["value"] = edge.Value,
                    ["confidence"] = edge.Confidence,
                    ["causeEventIds"] = StringListToObjects(edge.CauseEventIds),
                    ["lastChangedDay"] = edge.LastChangedDay,
                    ["lastChangeSaveRevision"] = edge.LastChangeSaveRevision,
                    ["lockedByStory"] = edge.LockedByStory,
                    ["visibility"] = edge.Visibility.ToString(),
                    ["appliedEventKeys"] = StringListToObjects(edge.AppliedEventKeys),
                });
            }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = graph.SchemaVersion,
                ["edges"] = edges,
            };
        }

        private static RelationGraphSnapshot RelationGraphFromObj(Dictionary<string, object> obj)
        {
            var graph = new RelationGraphSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D10RelationSchema.CurrentVersion),
            };
            foreach (var edgeObj in ObjectList(obj, "edges"))
            {
                var edge = new RelationEdge
                {
                    SchemaVersion = SaveJson.GetInt(edgeObj, "schemaVersion", D10RelationSchema.CurrentVersion),
                    SourceId = SaveJson.GetStr(edgeObj, "sourceId", ""),
                    TargetId = SaveJson.GetStr(edgeObj, "targetId", ""),
                    Value = SaveJson.GetFloat(edgeObj, "value"),
                    Confidence = SaveJson.GetFloat(edgeObj, "confidence", 1f),
                    LastChangedDay = SaveJson.GetInt(edgeObj, "lastChangedDay"),
                    LastChangeSaveRevision = ReadLong(edgeObj, "lastChangeSaveRevision"),
                    LockedByStory = ReadBool(edgeObj, "lockedByStory"),
                };
                Enum.TryParse(SaveJson.GetStr(edgeObj, "relationType", "Affinity"), true, out edge.RelationType);
                Enum.TryParse(SaveJson.GetStr(edgeObj, "visibility", "Private"), true, out edge.Visibility);
                ReadStringListObject(edge.CauseEventIds, edgeObj, "causeEventIds");
                ReadStringListObject(edge.AppliedEventKeys, edgeObj, "appliedEventKeys");
                graph.Edges.Add(edge);
            }
            graph.Validate();
            return graph;
        }

        private static bool ReadBool(Dictionary<string, object> obj, string key, bool fallback = false)
        {
            if (obj == null || !obj.ContainsKey(key) || obj[key] == null)
                return fallback;
            if (obj[key] is bool value)
                return value;
            return bool.TryParse(obj[key].ToString(), out var parsed) ? parsed : fallback;
        }

        private static Dictionary<string, object> D5ToObj(D5RuntimeSnapshot d5)
        {
            var world = new Dictionary<string, object>
            {
                ["currentIslandId"] = d5.World.CurrentIslandId ?? "santa_margarita",
                ["discoveredIslands"] = StringListToObjects(d5.World.DiscoveredIslands),
                ["visitedIslands"] = StringListToObjects(d5.World.VisitedIslands),
                ["mapRevision"] = d5.World.MapRevision,
            };
            var character = new Dictionary<string, object>
            {
                ["presetId"] = d5.Character.PresetId ?? "dockhand_male",
                ["skinTone"] = d5.Character.SkinTone ?? "sun_bronzed",
                ["hairStyle"] = d5.Character.HairStyle ?? "short",
                ["hairColor"] = d5.Character.HairColor ?? "brown",
                ["outfit"] = d5.Character.Outfit ?? "deckhand",
                ["faceScale"] = d5.Character.FaceScale,
                ["bodyScale"] = d5.Character.BodyScale,
            };
            var performance = new Dictionary<string, object>
            {
                ["quality"] = d5.Performance.Quality.ToString(),
                ["targetWidth"] = d5.Performance.TargetWidth, ["targetHeight"] = d5.Performance.TargetHeight,
                ["targetFps"] = d5.Performance.TargetFps, ["lastFps"] = d5.Performance.LastFps,
                ["minFps"] = d5.Performance.MinFps, ["averageFps"] = d5.Performance.AverageFps,
                ["sampleCount"] = d5.Performance.SampleCount, ["meetsTarget"] = d5.Performance.MeetsTarget,
            };
            var steam = new Dictionary<string, object>
            {
                ["appId"] = d5.Steam.AppId ?? "UNCONFIGURED", ["offlineMode"] = d5.Steam.OfflineMode,
                ["cloudEnabled"] = d5.Steam.CloudEnabled, ["pendingCloudRevision"] = d5.Steam.PendingCloudRevision,
                ["lastSync"] = d5.Steam.LastSync ?? "never",
                ["unlockedAchievements"] = StringListToObjects(d5.Steam.UnlockedAchievements),
            };
            var crewTraits = new Dictionary<string, object>();
            foreach (var pair in d5.CrewTraitIds)
                crewTraits[pair.Key] = StringListToObjects(pair.Value);
            return new Dictionary<string, object>
            {
                ["world"] = world, ["character"] = character, ["performance"] = performance,
                ["steam"] = steam, ["language"] = d5.Language == "en" ? "en" : "zh",
                ["playerTraitIds"] = StringListToObjects(d5.PlayerTraitIds), ["crewTraitIds"] = crewTraits,
                ["appearance"] = D10AppearanceToObj(d5.Appearance),
            };
        }

        private static D5RuntimeSnapshot D5FromObj(Dictionary<string, object> obj)
        {
            var result = new D5RuntimeSnapshot();
            var world = Nested(obj, "world");
            if (world != null)
            {
                result.World.CurrentIslandId = SaveJson.GetStr(world, "currentIslandId", "santa_margarita");
                result.World.MapRevision = SaveJson.GetInt(world, "mapRevision");
                result.World.DiscoveredIslands.Clear();
                result.World.VisitedIslands.Clear();
                ReadStringListObject(result.World.DiscoveredIslands, world, "discoveredIslands");
                ReadStringListObject(result.World.VisitedIslands, world, "visitedIslands");
            }

            var character = Nested(obj, "character");
            if (character != null)
            {
                result.Character.PresetId = SaveJson.GetStr(character, "presetId", "dockhand_male");
                result.Character.SkinTone = SaveJson.GetStr(character, "skinTone", "sun_bronzed");
                result.Character.HairStyle = SaveJson.GetStr(character, "hairStyle", "short");
                result.Character.HairColor = SaveJson.GetStr(character, "hairColor", "brown");
                result.Character.Outfit = SaveJson.GetStr(character, "outfit", "deckhand");
                result.Character.FaceScale = SaveJson.GetFloat(character, "faceScale", 1f);
                result.Character.BodyScale = SaveJson.GetFloat(character, "bodyScale", 1f);
            }

            var performance = Nested(obj, "performance");
            if (performance != null)
            {
                result.Performance.TargetWidth = SaveJson.GetInt(performance, "targetWidth", 1920);
                result.Performance.TargetHeight = SaveJson.GetInt(performance, "targetHeight", 1080);
                result.Performance.TargetFps = SaveJson.GetInt(performance, "targetFps", 60);
                result.Performance.LastFps = SaveJson.GetFloat(performance, "lastFps");
                result.Performance.MinFps = SaveJson.GetFloat(performance, "minFps");
                result.Performance.AverageFps = SaveJson.GetFloat(performance, "averageFps");
                result.Performance.SampleCount = SaveJson.GetInt(performance, "sampleCount");
                result.Performance.MeetsTarget = performance.ContainsKey("meetsTarget") && Convert.ToBoolean(performance["meetsTarget"]);
                Enum.TryParse(SaveJson.GetStr(performance, "quality", "Medium"), true, out result.Performance.Quality);
            }

            var steam = Nested(obj, "steam");
            if (steam != null)
            {
                result.Steam.AppId = SaveJson.GetStr(steam, "appId", "UNCONFIGURED");
                result.Steam.OfflineMode = !steam.ContainsKey("offlineMode") || Convert.ToBoolean(steam["offlineMode"]);
                result.Steam.CloudEnabled = !steam.ContainsKey("cloudEnabled") || Convert.ToBoolean(steam["cloudEnabled"]);
                result.Steam.PendingCloudRevision = SaveJson.GetInt(steam, "pendingCloudRevision");
                result.Steam.LastSync = SaveJson.GetStr(steam, "lastSync", "never");
                ReadStringListObject(result.Steam.UnlockedAchievements, steam, "unlockedAchievements");
            }
            result.Language = SaveJson.GetStr(obj, "language", "zh") == "en" ? "en" : "zh";
            ReadStringListObject(result.PlayerTraitIds, obj, "playerTraitIds");
            var appearance = Nested(obj, "appearance");
            if (appearance != null) result.Appearance = D10AppearanceFromObj(appearance);
            var crewTraits = Nested(obj, "crewTraitIds");
            if (crewTraits != null)
                foreach (var pair in crewTraits)
                {
                    var values = new List<string>();
                    if (pair.Value is List<object> list)
                        foreach (var value in list) values.Add(Convert.ToString(value));
                    result.CrewTraitIds[pair.Key] = values;
                }
            return result;
        }

        private static Dictionary<string, object> D10AppearanceToObj(D10AppearanceSnapshot appearance)
        {
            if (appearance == null) appearance = new D10AppearanceSnapshot();
            var context = appearance.Context == null ? new D10AppearanceContext() : appearance.Context;
            var slots = new Dictionary<string, object>();
            if (appearance.ActiveLoadout != null && appearance.ActiveLoadout.Slots != null)
                foreach (var pair in appearance.ActiveLoadout.Slots) slots[pair.Key] = pair.Value ?? "";

            var owned = new List<object>();
            if (appearance.Owned != null)
                foreach (var pair in appearance.Owned)
                {
                    var value = pair.Value ?? new D10OwnedAppearance { ItemId = pair.Key };
                    owned.Add(new Dictionary<string, object>
                    {
                        ["itemId"] = value.ItemId ?? pair.Key,
                        ["durability"] = value.Durability,
                        ["dyeId"] = value.DyeId ?? "",
                        ["dyeColor"] = value.DyeColor ?? "",
                        ["purchaseCount"] = value.PurchaseCount,
                    });
                }
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = appearance.SchemaVersion,
                ["context"] = new Dictionary<string, object>
                {
                    ["cultureId"] = context.CultureId ?? "island_saint_marys",
                    ["careerId"] = context.CareerId ?? "deckhand",
                    ["weatherId"] = context.WeatherId ?? "clear",
                    ["legalIdentityId"] = context.LegalIdentityId ?? "player",
                },
                ["slots"] = slots,
                ["owned"] = owned,
                ["activeInjuryIds"] = StringListToObjects(appearance.ActiveInjuryIds),
                ["injuryHistoryIds"] = StringListToObjects(appearance.InjuryHistoryIds),
                ["saveRevision"] = appearance.SaveRevision,
            };
        }

        private static D10AppearanceSnapshot D10AppearanceFromObj(Dictionary<string, object> obj)
        {
            var result = new D10AppearanceSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", 1),
                SaveRevision = ReadLong(obj, "saveRevision"),
            };
            var context = Nested(obj, "context");
            if (context != null)
            {
                result.Context.CultureId = SaveJson.GetStr(context, "cultureId", "island_saint_marys");
                result.Context.CareerId = SaveJson.GetStr(context, "careerId", "deckhand");
                result.Context.WeatherId = SaveJson.GetStr(context, "weatherId", "clear");
                result.Context.LegalIdentityId = SaveJson.GetStr(context, "legalIdentityId", "player");
            }
            var slots = Nested(obj, "slots");
            if (slots != null)
                foreach (var pair in slots) result.ActiveLoadout.Slots[pair.Key] = Convert.ToString(pair.Value);
            foreach (var ownedObj in ObjectList(obj, "owned"))
            {
                var itemId = SaveJson.GetStr(ownedObj, "itemId", "");
                if (string.IsNullOrEmpty(itemId)) continue;
                result.Owned[itemId] = new D10OwnedAppearance
                {
                    ItemId = itemId,
                    Durability = SaveJson.GetInt(ownedObj, "durability", 100),
                    DyeId = SaveJson.GetStr(ownedObj, "dyeId", ""),
                    DyeColor = SaveJson.GetStr(ownedObj, "dyeColor", ""),
                    PurchaseCount = SaveJson.GetInt(ownedObj, "purchaseCount", 1),
                };
            }
            ReadStringListObject(result.ActiveInjuryIds, obj, "activeInjuryIds");
            ReadStringListObject(result.InjuryHistoryIds, obj, "injuryHistoryIds");
            return result;
        }

        // D12-A05 兼容审查副本；正式 D12 Tarot 编解码位于前面的共享 D12 容器区。
        private static Dictionary<string, object> D12TarotAuditToObj(D12TarotRuntimeSnapshot runtime)
        {
            if (runtime == null) return null;
            runtime.Validate();
            var sessions = new List<object>();
            foreach (var session in runtime.Sessions)
            {
                var cards = new List<object>();
                foreach (var card in session.Cards) cards.Add(new Dictionary<string, object>
                {
                    ["drawId"] = card.DrawId ?? "", ["cardId"] = card.CardId ?? "", ["orientation"] = card.Orientation.ToString(),
                    ["position"] = card.Position, ["meaningTag"] = card.MeaningTag ?? "", ["softSignal"] = card.SoftSignal ?? "",
                });
                var clues = new List<object>();
                foreach (var clue in session.Clues) clues.Add(new Dictionary<string, object>
                {
                    ["clueId"] = clue.ClueId ?? "", ["sessionId"] = clue.SessionId ?? "", ["kind"] = clue.Kind ?? "", ["targetRef"] = clue.TargetRef ?? "",
                    ["signal"] = clue.Signal ?? "", ["reliability"] = clue.Reliability, ["biasCode"] = clue.BiasCode ?? "", ["status"] = clue.Status.ToString(),
                    ["evidenceEventId"] = clue.EvidenceEventId ?? "", ["sourceRef"] = clue.SourceRef ?? "", ["evidenceRef"] = clue.EvidenceRef ?? "",
                    ["worldDay"] = clue.WorldDay, ["saveRevision"] = clue.SaveRevision, ["verified"] = clue.Verified,
                });
                sessions.Add(new Dictionary<string, object>
                {
                    ["sessionId"] = session.SessionId ?? "", ["divinerId"] = session.DivinerId ?? "", ["queryTag"] = session.QueryTag ?? "", ["actorRef"] = session.ActorRef ?? "",
                    ["locationRef"] = session.LocationRef ?? "", ["worldDay"] = session.WorldDay, ["seed"] = session.Seed, ["sensitivity"] = session.Sensitivity.ToString(),
                    ["warningAcknowledged"] = session.WarningAcknowledged, ["skippedSensitiveContent"] = session.SkippedSensitiveContent,
                    ["cards"] = cards, ["clues"] = clues, ["explanationText"] = session.ExplanationText ?? "", ["explanationProvider"] = session.ExplanationProvider ?? "",
                    ["lastEventId"] = session.LastEventId ?? "", ["revision"] = session.Revision, ["appliedEventKeys"] = StringListToObjects(session.AppliedEventKeys),
                });
            }
            var trusts = new List<object>();
            foreach (var trust in runtime.TrustStates) trusts.Add(new Dictionary<string, object>
            {
                ["divinerId"] = trust.DivinerId ?? "", ["cultureId"] = trust.CultureId ?? "", ["religionId"] = trust.ReligionId ?? "", ["trust"] = trust.Trust,
                ["sourceRef"] = trust.SourceRef ?? "", ["evidenceRef"] = trust.EvidenceRef ?? "", ["lastEventId"] = trust.LastEventId ?? "", ["lastWorldDay"] = trust.LastWorldDay,
                ["lastSaveRevision"] = trust.LastSaveRevision, ["appliedEventKeys"] = StringListToObjects(trust.AppliedEventKeys),
            });
            var verifications = new List<object>();
            foreach (var item in runtime.Verifications) verifications.Add(new Dictionary<string, object>
            {
                ["verificationId"] = item.VerificationId ?? "", ["sessionId"] = item.SessionId ?? "", ["clueId"] = item.ClueId ?? "", ["method"] = item.Method.ToString(),
                ["outcome"] = item.Outcome.ToString(), ["evidenceRef"] = item.EvidenceRef ?? "", ["sourceRef"] = item.SourceRef ?? "", ["factEventId"] = item.FactEventId ?? "",
                ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision, ["lastEventId"] = item.LastEventId ?? "",
            });
            var consequences = new List<object>();
            foreach (var item in runtime.Consequences) consequences.Add(new Dictionary<string, object>
            {
                ["consequenceId"] = item.ConsequenceId ?? "", ["sessionId"] = item.SessionId ?? "", ["clueId"] = item.ClueId ?? "", ["verificationId"] = item.VerificationId ?? "",
                ["impactTag"] = item.ImpactTag ?? "", ["followedAdvice"] = item.FollowedAdvice, ["costBand"] = item.CostBand ?? "", ["causeEventIds"] = StringListToObjects(item.CauseEventIds),
                ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision, ["eventId"] = item.EventId ?? "", ["downstreamOwner"] = item.DownstreamOwner ?? "",
            });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["lastEventId"] = runtime.LastEventId ?? "",
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys), ["sessions"] = sessions, ["trustStates"] = trusts,
                ["verifications"] = verifications, ["consequences"] = consequences,
            };
        }

        private static D12TarotRuntimeSnapshot D12TarotAuditFromObj(Dictionary<string, object> obj)
        {
            var result = new D12TarotRuntimeSnapshot
            {
                SchemaVersion = obj == null ? D12TarotRuntimeSnapshot.CurrentSchemaVersion : SaveJson.GetInt(obj, "schemaVersion", D12TarotRuntimeSnapshot.CurrentSchemaVersion),
                Revision = obj == null ? 0L : Math.Max(0L, ReadLong(obj, "revision")), LastEventId = obj == null ? "" : SaveJson.GetStr(obj, "lastEventId", ""),
            };
            if (obj == null) return result;
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "sessions"))
            {
                var session = new D12TarotSessionState
                {
                    SessionId = SaveJson.GetStr(item, "sessionId", ""), DivinerId = SaveJson.GetStr(item, "divinerId", ""), QueryTag = SaveJson.GetStr(item, "queryTag", ""),
                    ActorRef = SaveJson.GetStr(item, "actorRef", ""), LocationRef = SaveJson.GetStr(item, "locationRef", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    Seed = ReadLong(item, "seed"), WarningAcknowledged = ReadBool(item, "warningAcknowledged"), SkippedSensitiveContent = ReadBool(item, "skippedSensitiveContent"),
                    ExplanationText = SaveJson.GetStr(item, "explanationText", ""), ExplanationProvider = SaveJson.GetStr(item, "explanationProvider", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                    Revision = Math.Max(0L, ReadLong(item, "revision")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "sensitivity", "None"), true, out session.Sensitivity);
                ReadStringListObject(session.AppliedEventKeys, item, "appliedEventKeys");
                foreach (var cardObj in ObjectList(item, "cards"))
                {
                    var card = new D12TarotCardDraw
                    {
                        DrawId = SaveJson.GetStr(cardObj, "drawId", ""), CardId = SaveJson.GetStr(cardObj, "cardId", ""), Position = SaveJson.GetInt(cardObj, "position"),
                        MeaningTag = SaveJson.GetStr(cardObj, "meaningTag", ""), SoftSignal = SaveJson.GetStr(cardObj, "softSignal", ""),
                    };
                    Enum.TryParse(SaveJson.GetStr(cardObj, "orientation", "Upright"), true, out card.Orientation);
                    session.Cards.Add(card);
                }
                foreach (var clueObj in ObjectList(item, "clues"))
                {
                    var clue = new D12SoftClue
                    {
                        ClueId = SaveJson.GetStr(clueObj, "clueId", ""), SessionId = SaveJson.GetStr(clueObj, "sessionId", ""), Kind = SaveJson.GetStr(clueObj, "kind", ""),
                        TargetRef = SaveJson.GetStr(clueObj, "targetRef", ""), Signal = SaveJson.GetStr(clueObj, "signal", ""), Reliability = Math.Max(0f, Math.Min(1f, SaveJson.GetFloat(clueObj, "reliability"))),
                        BiasCode = SaveJson.GetStr(clueObj, "biasCode", ""), EvidenceEventId = SaveJson.GetStr(clueObj, "evidenceEventId", ""), SourceRef = SaveJson.GetStr(clueObj, "sourceRef", ""), EvidenceRef = SaveJson.GetStr(clueObj, "evidenceRef", ""),
                        WorldDay = Math.Max(0, SaveJson.GetInt(clueObj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(clueObj, "saveRevision")), Verified = ReadBool(clueObj, "verified"),
                    };
                    Enum.TryParse(SaveJson.GetStr(clueObj, "status", "Unverified"), true, out clue.Status);
                    session.Clues.Add(clue);
                }
                result.Sessions.Add(session);
            }
            foreach (var item in ObjectList(obj, "trustStates"))
            {
                var trust = new D12DivinationTrustState
                {
                    DivinerId = SaveJson.GetStr(item, "divinerId", ""), CultureId = SaveJson.GetStr(item, "cultureId", ""), ReligionId = SaveJson.GetStr(item, "religionId", ""), Trust = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "trust"))),
                    SourceRef = SaveJson.GetStr(item, "sourceRef", ""), EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", ""), LastWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastWorldDay")), LastSaveRevision = Math.Max(0L, ReadLong(item, "lastSaveRevision")),
                };
                ReadStringListObject(trust.AppliedEventKeys, item, "appliedEventKeys"); result.TrustStates.Add(trust);
            }
            foreach (var item in ObjectList(obj, "verifications"))
            {
                var verification = new D12TarotVerificationState
                {
                    VerificationId = SaveJson.GetStr(item, "verificationId", ""), SessionId = SaveJson.GetStr(item, "sessionId", ""), ClueId = SaveJson.GetStr(item, "clueId", ""), EvidenceRef = SaveJson.GetStr(item, "evidenceRef", ""), SourceRef = SaveJson.GetStr(item, "sourceRef", ""), FactEventId = SaveJson.GetStr(item, "factEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                };
                Enum.TryParse(SaveJson.GetStr(item, "method", "EventRecord"), true, out verification.Method); Enum.TryParse(SaveJson.GetStr(item, "outcome", "Unverified"), true, out verification.Outcome); result.Verifications.Add(verification);
            }
            foreach (var item in ObjectList(obj, "consequences"))
            {
                var consequence = new D12TarotErrorConsequenceRecord
                {
                    ConsequenceId = SaveJson.GetStr(item, "consequenceId", ""), SessionId = SaveJson.GetStr(item, "sessionId", ""), ClueId = SaveJson.GetStr(item, "clueId", ""), VerificationId = SaveJson.GetStr(item, "verificationId", ""), ImpactTag = SaveJson.GetStr(item, "impactTag", ""), FollowedAdvice = ReadBool(item, "followedAdvice"), CostBand = SaveJson.GetStr(item, "costBand", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), EventId = SaveJson.GetStr(item, "eventId", ""), DownstreamOwner = SaveJson.GetStr(item, "downstreamOwner", "caller_domain_system"),
                };
                ReadStringListObject(consequence.CauseEventIds, item, "causeEventIds"); result.Consequences.Add(consequence);
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D12PokerToObj(D12PokerRuntimeSnapshot runtime)
        {
            if (runtime == null) return null;
            runtime.Validate();
            var sessions = new List<object>();
            foreach (var session in runtime.Sessions)
                if (session != null) sessions.Add(D12PokerSessionToObj(session));
            var records = new List<object>();
            foreach (var record in runtime.Records)
                if (record != null) records.Add(D12PokerRecordToObj(record));
            var economy = new List<object>();
            foreach (var request in runtime.EconomyRequests)
                if (request != null) economy.Add(D12PokerEconomyRequestToObj(request));
            var debt = new List<object>();
            foreach (var request in runtime.DebtRequests)
                if (request != null) debt.Add(D12PokerDebtRequestToObj(request));
            var rewards = new List<object>();
            foreach (var request in runtime.RewardRequests)
                if (request != null) rewards.Add(D12PokerRewardRequestToObj(request));
            var relations = new List<object>();
            foreach (var request in runtime.RelationImpactRequests)
                if (request != null) relations.Add(D12PokerRelationRequestToObj(request));
            var information = new List<object>();
            foreach (var request in runtime.InformationRequests)
                if (request != null) information.Add(D12PokerInformationRequestToObj(request));
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["revision"] = runtime.Revision,
                ["lastWorldDay"] = runtime.LastWorldDay,
                ["lastEventId"] = runtime.LastEventId ?? "",
                ["nextGameSequence"] = runtime.NextGameSequence,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
                ["sessions"] = sessions,
                ["records"] = records,
                ["economyRequests"] = economy,
                ["debtRequests"] = debt,
                ["rewardRequests"] = rewards,
                ["relationImpactRequests"] = relations,
                ["informationRequests"] = information,
            };
        }

        private static Dictionary<string, object> D12PokerSessionToObj(D12PokerSessionState value)
        {
            var playerCards = new List<object>();
            foreach (var card in value.PlayerHand) if (card != null) playerCards.Add(D12PokerCardToObj(card));
            var houseCards = new List<object>();
            foreach (var card in value.HouseHand) if (card != null) houseCards.Add(D12PokerCardToObj(card));
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["tableId"] = value.TableId ?? "", ["playerId"] = value.PlayerId ?? "",
                ["gameId"] = value.GameId ?? "", ["rulesetId"] = value.RulesetId ?? "", ["fixedSeed"] = value.FixedSeed,
                ["entryFee"] = value.EntryFee, ["betCap"] = value.BetCap, ["currentBet"] = value.CurrentBet,
                ["proposedDebt"] = value.ProposedDebt, ["status"] = value.Status.ToString(), ["outcome"] = value.Outcome.ToString(),
                ["cheatRisk"] = value.CheatRisk.ToString(), ["cheatAttempted"] = value.CheatAttempted,
                ["worldDay"] = value.WorldDay, ["saveRevision"] = value.SaveRevision, ["lastEventId"] = value.LastEventId ?? "",
                ["playerHand"] = playerCards, ["houseHand"] = houseCards,
            };
        }

        private static Dictionary<string, object> D12PokerCardToObj(D12PokerCard value)
        {
            return new Dictionary<string, object>
            {
                ["cardId"] = value.CardId ?? "", ["suit"] = value.Suit.ToString(), ["rank"] = value.Rank.ToString(),
            };
        }

        private static Dictionary<string, object> D12PokerRecordToObj(D12PokerGameRecord value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["recordId"] = value.RecordId ?? "", ["gameId"] = value.GameId ?? "",
                ["tableId"] = value.TableId ?? "", ["playerId"] = value.PlayerId ?? "", ["sourceEventId"] = value.SourceEventId ?? "",
                ["saveRevision"] = value.SaveRevision, ["worldDay"] = value.WorldDay, ["fixedSeed"] = value.FixedSeed,
                ["entryFee"] = value.EntryFee, ["bet"] = value.Bet, ["payout"] = value.Payout, ["netProposedDelta"] = value.NetProposedDelta,
                ["outcome"] = value.Outcome.ToString(), ["playerHandCategory"] = value.PlayerHandCategory.ToString(),
                ["houseHandCategory"] = value.HouseHandCategory.ToString(), ["cheatRisk"] = value.CheatRisk.ToString(),
                ["playerCardIds"] = StringListToObjects(value.PlayerCardIds), ["houseCardIds"] = StringListToObjects(value.HouseCardIds),
                ["settlementClaimed"] = value.SettlementClaimed,
            };
        }

        private static Dictionary<string, object> D12PokerEconomyRequestToObj(D12PokerEconomyRequest value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["requestId"] = value.RequestId ?? "", ["gameId"] = value.GameId ?? "",
                ["sourceEventId"] = value.SourceEventId ?? "", ["playerId"] = value.PlayerId ?? "", ["currency"] = value.Currency ?? "",
                ["kind"] = value.Kind ?? "", ["amount"] = value.Amount, ["status"] = value.Status.ToString(),
            };
        }

        private static Dictionary<string, object> D12PokerDebtRequestToObj(D12PokerDebtRequest value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["requestId"] = value.RequestId ?? "", ["gameId"] = value.GameId ?? "",
                ["sourceEventId"] = value.SourceEventId ?? "", ["playerId"] = value.PlayerId ?? "", ["principal"] = value.Principal,
                ["currentDebt"] = value.CurrentDebt, ["debtLimit"] = value.DebtLimit, ["status"] = value.Status.ToString(),
            };
        }

        private static Dictionary<string, object> D12PokerRewardRequestToObj(D12PokerRewardRequest value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["requestId"] = value.RequestId ?? "", ["rewardId"] = value.RewardId ?? "",
                ["gameId"] = value.GameId ?? "", ["sourceEventId"] = value.SourceEventId ?? "", ["playerId"] = value.PlayerId ?? "",
                ["rewardTag"] = value.RewardTag ?? "", ["currency"] = value.Currency ?? "", ["currencyAmount"] = value.CurrencyAmount,
                ["status"] = value.Status.ToString(),
            };
        }

        private static Dictionary<string, object> D12PokerRelationRequestToObj(D12PokerRelationImpactRequest value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["requestId"] = value.RequestId ?? "", ["sourceEventId"] = value.SourceEventId ?? "",
                ["sourceId"] = value.SourceId ?? "", ["targetId"] = value.TargetId ?? "", ["relationType"] = value.RelationType ?? "",
                ["proposedDelta"] = value.ProposedDelta, ["reason"] = value.Reason ?? "", ["status"] = value.Status.ToString(),
            };
        }

        private static Dictionary<string, object> D12PokerInformationRequestToObj(D12PokerInformationRequest value)
        {
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = value.SchemaVersion, ["requestId"] = value.RequestId ?? "", ["sourceEventId"] = value.SourceEventId ?? "",
                ["playerId"] = value.PlayerId ?? "", ["informationPoolId"] = value.InformationPoolId ?? "",
                ["informationCode"] = value.InformationCode ?? "", ["reliability"] = value.Reliability, ["status"] = value.Status.ToString(),
            };
        }

        private static D12PokerRuntimeSnapshot D12PokerFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastWorldDay = Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")),
                LastEventId = SaveJson.GetStr(obj, "lastEventId", ""),
                NextGameSequence = Math.Max(0L, ReadLong(obj, "nextGameSequence")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "sessions")) result.Sessions.Add(D12PokerSessionFromObj(item));
            foreach (var item in ObjectList(obj, "records")) result.Records.Add(D12PokerRecordFromObj(item));
            foreach (var item in ObjectList(obj, "economyRequests")) result.EconomyRequests.Add(D12PokerEconomyRequestFromObj(item));
            foreach (var item in ObjectList(obj, "debtRequests")) result.DebtRequests.Add(D12PokerDebtRequestFromObj(item));
            foreach (var item in ObjectList(obj, "rewardRequests")) result.RewardRequests.Add(D12PokerRewardRequestFromObj(item));
            foreach (var item in ObjectList(obj, "relationImpactRequests")) result.RelationImpactRequests.Add(D12PokerRelationRequestFromObj(item));
            foreach (var item in ObjectList(obj, "informationRequests")) result.InformationRequests.Add(D12PokerInformationRequestFromObj(item));
            result.Validate();
            return result;
        }

        private static D12PokerSessionState D12PokerSessionFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerSessionState
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), TableId = SaveJson.GetStr(obj, "tableId", ""),
                PlayerId = SaveJson.GetStr(obj, "playerId", ""), GameId = SaveJson.GetStr(obj, "gameId", ""), RulesetId = SaveJson.GetStr(obj, "rulesetId", D12PokerIds.RulesetId),
                FixedSeed = ReadLong(obj, "fixedSeed"), EntryFee = Math.Max(0L, ReadLong(obj, "entryFee")), BetCap = Math.Max(0L, ReadLong(obj, "betCap")),
                CurrentBet = Math.Max(0L, ReadLong(obj, "currentBet")), ProposedDebt = Math.Max(0L, ReadLong(obj, "proposedDebt")),
                CheatAttempted = ReadBool(obj, "cheatAttempted"), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")), LastEventId = SaveJson.GetStr(obj, "lastEventId", ""),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Unknown"), true, out result.Status);
            Enum.TryParse(SaveJson.GetStr(obj, "outcome", "Unknown"), true, out result.Outcome);
            Enum.TryParse(SaveJson.GetStr(obj, "cheatRisk", "Unknown"), true, out result.CheatRisk);
            foreach (var item in ObjectList(obj, "playerHand")) result.PlayerHand.Add(D12PokerCardFromObj(item));
            foreach (var item in ObjectList(obj, "houseHand")) result.HouseHand.Add(D12PokerCardFromObj(item));
            return result;
        }

        private static D12PokerCard D12PokerCardFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerCard { CardId = SaveJson.GetStr(obj, "cardId", "") };
            Enum.TryParse(SaveJson.GetStr(obj, "suit", "Unknown"), true, out result.Suit);
            Enum.TryParse(SaveJson.GetStr(obj, "rank", "Unknown"), true, out result.Rank);
            return result;
        }

        private static D12PokerGameRecord D12PokerRecordFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerGameRecord
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RecordId = SaveJson.GetStr(obj, "recordId", ""),
                GameId = SaveJson.GetStr(obj, "gameId", ""), TableId = SaveJson.GetStr(obj, "tableId", ""), PlayerId = SaveJson.GetStr(obj, "playerId", ""),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""), SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), FixedSeed = ReadLong(obj, "fixedSeed"), EntryFee = Math.Max(0L, ReadLong(obj, "entryFee")),
                Bet = Math.Max(0L, ReadLong(obj, "bet")), Payout = Math.Max(0L, ReadLong(obj, "payout")), NetProposedDelta = ReadLong(obj, "netProposedDelta"),
                SettlementClaimed = ReadBool(obj, "settlementClaimed"),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "outcome", "Unknown"), true, out result.Outcome);
            Enum.TryParse(SaveJson.GetStr(obj, "playerHandCategory", "Unknown"), true, out result.PlayerHandCategory);
            Enum.TryParse(SaveJson.GetStr(obj, "houseHandCategory", "Unknown"), true, out result.HouseHandCategory);
            Enum.TryParse(SaveJson.GetStr(obj, "cheatRisk", "Unknown"), true, out result.CheatRisk);
            ReadStringListObject(result.PlayerCardIds, obj, "playerCardIds");
            ReadStringListObject(result.HouseCardIds, obj, "houseCardIds");
            return result;
        }

        private static D12PokerEconomyRequest D12PokerEconomyRequestFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerEconomyRequest
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RequestId = SaveJson.GetStr(obj, "requestId", ""),
                GameId = SaveJson.GetStr(obj, "gameId", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""), PlayerId = SaveJson.GetStr(obj, "playerId", ""),
                Currency = SaveJson.GetStr(obj, "currency", D12PokerIds.Currency), Kind = SaveJson.GetStr(obj, "kind", ""), Amount = ReadLong(obj, "amount"),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Pending"), true, out result.Status);
            return result;
        }

        private static D12PokerDebtRequest D12PokerDebtRequestFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerDebtRequest
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RequestId = SaveJson.GetStr(obj, "requestId", ""),
                GameId = SaveJson.GetStr(obj, "gameId", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""), PlayerId = SaveJson.GetStr(obj, "playerId", ""),
                Principal = Math.Max(0L, ReadLong(obj, "principal")), CurrentDebt = Math.Max(0L, ReadLong(obj, "currentDebt")), DebtLimit = Math.Max(0L, ReadLong(obj, "debtLimit")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Pending"), true, out result.Status);
            return result;
        }

        private static D12PokerRewardRequest D12PokerRewardRequestFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerRewardRequest
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RequestId = SaveJson.GetStr(obj, "requestId", ""), RewardId = SaveJson.GetStr(obj, "rewardId", ""),
                GameId = SaveJson.GetStr(obj, "gameId", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""), PlayerId = SaveJson.GetStr(obj, "playerId", ""),
                RewardTag = SaveJson.GetStr(obj, "rewardTag", ""), Currency = SaveJson.GetStr(obj, "currency", D12PokerIds.Currency), CurrencyAmount = Math.Max(0L, ReadLong(obj, "currencyAmount")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Pending"), true, out result.Status);
            return result;
        }

        private static D12PokerRelationImpactRequest D12PokerRelationRequestFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerRelationImpactRequest
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RequestId = SaveJson.GetStr(obj, "requestId", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                SourceId = SaveJson.GetStr(obj, "sourceId", ""), TargetId = SaveJson.GetStr(obj, "targetId", ""), RelationType = SaveJson.GetStr(obj, "relationType", "trust"),
                ProposedDelta = SaveJson.GetInt(obj, "proposedDelta"), Reason = SaveJson.GetStr(obj, "reason", ""),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Pending"), true, out result.Status);
            return result;
        }

        private static D12PokerInformationRequest D12PokerInformationRequestFromObj(Dictionary<string, object> obj)
        {
            var result = new D12PokerInformationRequest
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D12PokerIds.CurrentSchemaVersion), RequestId = SaveJson.GetStr(obj, "requestId", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                PlayerId = SaveJson.GetStr(obj, "playerId", ""), InformationPoolId = SaveJson.GetStr(obj, "informationPoolId", ""), InformationCode = SaveJson.GetStr(obj, "informationCode", ""),
                Reliability = Math.Max(0f, Math.Min(1f, SaveJson.GetFloat(obj, "reliability"))),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "status", "Pending"), true, out result.Status);
            return result;
        }

        private static Dictionary<string, object> D13ToObj(D13PropertyRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var properties = new List<object>();
            foreach (var state in runtime.Properties)
                properties.Add(new Dictionary<string, object>
                {
                    ["propertyId"] = state.PropertyId ?? "",
                    ["ownership"] = state.Ownership.ToString(),
                    ["ownerRef"] = state.OwnerRef ?? "",
                    ["controllerRef"] = state.ControllerRef ?? "",
                    ["leaseHolderRef"] = state.LeaseHolderRef ?? "",
                    ["leaseEndDay"] = state.LeaseEndDay,
                    ["coGovernanceRefs"] = StringListToObjects(state.CoGovernanceRefs),
                    ["mortgageRef"] = state.MortgageRef ?? "",
                    ["legalStatus"] = state.LegalStatus.ToString(),
                    ["legalSourceEventId"] = state.LegalSourceEventId ?? "",
                    ["disputeStatus"] = state.DisputeStatus.ToString(),
                    ["disputeId"] = state.DisputeId ?? "",
                    ["latestValuationAmount"] = state.LatestValuationAmount,
                    ["latestValuationSourceEventId"] = state.LatestValuationSourceEventId ?? "",
                    ["latestValuationDay"] = state.LatestValuationDay,
                    ["revision"] = state.Revision,
                    ["lastUpdatedDay"] = state.LastUpdatedDay,
                    ["lastEventId"] = state.LastEventId ?? "",
                });
            var surveys = new List<object>();
            foreach (var survey in runtime.Surveys)
                surveys.Add(new Dictionary<string, object>
                {
                    ["surveyId"] = survey.SurveyId ?? "",
                    ["propertyId"] = survey.PropertyId ?? "",
                    ["investigatorRef"] = survey.InvestigatorRef ?? "",
                    ["approved"] = survey.Approved,
                    ["decisionCode"] = survey.DecisionCode ?? "",
                    ["titleHolderRef"] = survey.TitleHolderRef ?? "",
                    ["legalStatus"] = survey.LegalStatus.ToString(),
                    ["sourceAuthority"] = survey.SourceAuthority ?? "",
                    ["sourceEventId"] = survey.SourceEventId ?? "",
                    ["evidenceRefs"] = StringListToObjects(survey.EvidenceRefs),
                    ["worldDay"] = survey.WorldDay,
                    ["saveRevision"] = survey.SaveRevision,
                });
            var valuations = new List<object>();
            foreach (var quote in runtime.Valuations)
                valuations.Add(new Dictionary<string, object>
                {
                    ["propertyId"] = quote.PropertyId ?? "",
                    ["amount"] = quote.Amount,
                    ["currency"] = quote.Currency ?? "coin",
                    ["valuationBasis"] = quote.ValuationBasis ?? "",
                    ["sourceOwnerSystem"] = quote.SourceOwnerSystem ?? "",
                    ["sourceEventId"] = quote.SourceEventId ?? "",
                    ["worldDay"] = quote.WorldDay,
                    ["saveRevision"] = quote.SaveRevision,
                    ["reasons"] = StringListToObjects(quote.Reasons),
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["revision"] = runtime.Revision,
                ["properties"] = properties,
                ["surveys"] = surveys,
                ["valuations"] = valuations,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
            };
        }

        private static D13PropertyRuntimeSnapshot D13FromObj(Dictionary<string, object> obj)
        {
            var result = new D13PropertyRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13PropertyRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "properties")) result.Properties.Add(D13PropertyStateFromObj(item));
            foreach (var item in ObjectList(obj, "surveys")) result.Surveys.Add(D13PropertySurveyFromObj(item));
            foreach (var item in ObjectList(obj, "valuations")) result.Valuations.Add(D13ValuationFromObj(item));
            result.Validate();
            return result;
        }

        private static D13PropertyState D13PropertyStateFromObj(Dictionary<string, object> obj)
        {
            var result = new D13PropertyState
            {
                PropertyId = SaveJson.GetStr(obj, "propertyId", ""),
                OwnerRef = EmptyToNull(SaveJson.GetStr(obj, "ownerRef", "")),
                ControllerRef = EmptyToNull(SaveJson.GetStr(obj, "controllerRef", "")),
                LeaseHolderRef = EmptyToNull(SaveJson.GetStr(obj, "leaseHolderRef", "")),
                LeaseEndDay = Math.Max(0, SaveJson.GetInt(obj, "leaseEndDay")),
                MortgageRef = EmptyToNull(SaveJson.GetStr(obj, "mortgageRef", "")),
                LegalSourceEventId = EmptyToNull(SaveJson.GetStr(obj, "legalSourceEventId", "")),
                DisputeId = EmptyToNull(SaveJson.GetStr(obj, "disputeId", "")),
                LatestValuationAmount = Math.Max(0, SaveJson.GetInt(obj, "latestValuationAmount")),
                LatestValuationSourceEventId = EmptyToNull(SaveJson.GetStr(obj, "latestValuationSourceEventId", "")),
                LatestValuationDay = Math.Max(0, SaveJson.GetInt(obj, "latestValuationDay")),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "ownership", "PrivateOwnership"), true, out result.Ownership);
            Enum.TryParse(SaveJson.GetStr(obj, "legalStatus", "Unknown"), true, out result.LegalStatus);
            Enum.TryParse(SaveJson.GetStr(obj, "disputeStatus", "None"), true, out result.DisputeStatus);
            ReadStringListObject(result.CoGovernanceRefs, obj, "coGovernanceRefs");
            return result;
        }

        private static D13PropertySurveyRecord D13PropertySurveyFromObj(Dictionary<string, object> obj)
        {
            var result = new D13PropertySurveyRecord
            {
                SurveyId = SaveJson.GetStr(obj, "surveyId", ""),
                PropertyId = SaveJson.GetStr(obj, "propertyId", ""),
                InvestigatorRef = SaveJson.GetStr(obj, "investigatorRef", ""),
                Approved = ReadBool(obj, "approved"),
                DecisionCode = SaveJson.GetStr(obj, "decisionCode", "legal_denied"),
                TitleHolderRef = SaveJson.GetStr(obj, "titleHolderRef", ""),
                SourceAuthority = SaveJson.GetStr(obj, "sourceAuthority", D13PropertyIds.LegalOwner),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "legalStatus", "Unknown"), true, out result.LegalStatus);
            ReadStringListObject(result.EvidenceRefs, obj, "evidenceRefs");
            return result;
        }

        private static D13ValuationQuote D13ValuationFromObj(Dictionary<string, object> obj)
        {
            var result = new D13ValuationQuote
            {
                PropertyId = SaveJson.GetStr(obj, "propertyId", ""),
                Amount = Math.Max(0, SaveJson.GetInt(obj, "amount")),
                Currency = SaveJson.GetStr(obj, "currency", "coin"),
                ValuationBasis = SaveJson.GetStr(obj, "valuationBasis", "unknown"),
                SourceOwnerSystem = SaveJson.GetStr(obj, "sourceOwnerSystem", D13PropertyIds.EconomyOwner),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
            ReadStringListObject(result.Reasons, obj, "reasons");
            return result;
        }

        private static Dictionary<string, object> D13FinanceToObj(D13FinanceRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var accounts = new List<object>();
            foreach (var item in runtime.Accounts)
                accounts.Add(new Dictionary<string, object>
                {
                    ["accountId"] = item.AccountId ?? "",
                    ["ownerRef"] = item.OwnerRef ?? "",
                    ["accountType"] = item.AccountType.ToString(),
                    ["currency"] = item.Currency ?? D13FinanceIds.Currency,
                    ["cashBalance"] = item.CashBalance,
                    ["creditLimit"] = item.CreditLimit,
                    ["creditUsed"] = item.CreditUsed,
                    ["creditScore"] = item.CreditScore,
                    ["creditReasons"] = StringListToObjects(item.CreditReasons),
                    ["debtIds"] = StringListToObjects(item.DebtIds),
                    ["policyIds"] = StringListToObjects(item.PolicyIds),
                    ["bankruptcyProtectionUntilDay"] = item.BankruptcyProtectionUntilDay,
                    ["bankruptcyProtectionCount"] = item.BankruptcyProtectionCount,
                    ["revision"] = item.Revision,
                    ["lastUpdatedDay"] = item.LastUpdatedDay,
                    ["lastEventId"] = item.LastEventId ?? "",
                });
            var debts = new List<object>();
            foreach (var item in runtime.Debts)
            {
                var collateral = new List<object>();
                foreach (var value in item.Collateral)
                    collateral.Add(new Dictionary<string, object>
                    {
                        ["assetRef"] = value.AssetRef ?? "",
                        ["assetKind"] = value.AssetKind ?? "property",
                        ["claimValue"] = value.ClaimValue,
                        ["valuationSourceOwnerSystem"] = value.ValuationSourceOwnerSystem ?? "",
                        ["valuationSourceEventId"] = value.ValuationSourceEventId ?? "",
                        ["legalSourceOwnerSystem"] = value.LegalSourceOwnerSystem ?? "",
                        ["legalSourceEventId"] = value.LegalSourceEventId ?? "",
                        ["sourceEventId"] = value.SourceEventId ?? "",
                    });
                debts.Add(new Dictionary<string, object>
                {
                    ["debtId"] = item.DebtId ?? "",
                    ["accountId"] = item.AccountId ?? "",
                    ["debtorRef"] = item.DebtorRef ?? "",
                    ["creditorRef"] = item.CreditorRef ?? "",
                    ["kind"] = item.Kind.ToString(),
                    ["state"] = item.State.ToString(),
                    ["stateBeforeProtection"] = item.StateBeforeProtection.ToString(),
                    ["collectionState"] = item.CollectionState.ToString(),
                    ["principal"] = item.Principal,
                    ["interestAccrued"] = item.InterestAccrued,
                    ["latePenalty"] = item.LatePenalty,
                    ["interestRatePercent"] = item.InterestRatePercent,
                    ["latePenaltyPercent"] = item.LatePenaltyPercent,
                    ["dueDay"] = item.DueDay,
                    ["graceUntilDay"] = item.GraceUntilDay,
                    ["lastAccrualDay"] = item.LastAccrualDay,
                    ["lastPaymentDay"] = item.LastPaymentDay,
                    ["renegotiationCount"] = item.RenegotiationCount,
                    ["ledgerRef"] = item.LedgerRef ?? "",
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["collateral"] = collateral,
                    ["guaranteeIds"] = StringListToObjects(item.GuaranteeIds),
                    ["riskExplanation"] = item.RiskExplanation ?? "",
                    ["worstCaseOutcome"] = item.WorstCaseOutcome ?? "",
                    ["revision"] = item.Revision,
                    ["lastUpdatedDay"] = item.LastUpdatedDay,
                    ["lastEventId"] = item.LastEventId ?? "",
                });
            }
            var guarantees = new List<object>();
            foreach (var item in runtime.Guarantees)
                guarantees.Add(new Dictionary<string, object>
                {
                    ["guaranteeId"] = item.GuaranteeId ?? "",
                    ["debtId"] = item.DebtId ?? "",
                    ["guarantorRef"] = item.GuarantorRef ?? "",
                    ["principalCap"] = item.PrincipalCap,
                    ["state"] = item.State.ToString(),
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["revision"] = item.Revision,
                    ["lastUpdatedDay"] = item.LastUpdatedDay,
                });
            var evidence = new List<object>();
            foreach (var item in runtime.Evidence)
                evidence.Add(new Dictionary<string, object>
                {
                    ["evidenceId"] = item.EvidenceId ?? "",
                    ["ownerRef"] = item.OwnerRef ?? "",
                    ["kind"] = item.Kind.ToString(),
                    ["factEventId"] = item.FactEventId ?? "",
                    ["factSaveRevision"] = item.FactSaveRevision,
                    ["description"] = item.Description ?? "",
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision,
                });
            var policies = new List<object>();
            foreach (var item in runtime.Policies)
                policies.Add(new Dictionary<string, object>
                {
                    ["policyId"] = item.PolicyId ?? "",
                    ["accountId"] = item.AccountId ?? "",
                    ["insuredRef"] = item.InsuredRef ?? "",
                    ["kind"] = item.Kind.ToString(),
                    ["assetRef"] = item.AssetRef ?? "",
                    ["coverageAmount"] = item.CoverageAmount,
                    ["premiumPaid"] = item.PremiumPaid,
                    ["deductibleAmount"] = item.DeductibleAmount,
                    ["maxPayout"] = item.MaxPayout,
                    ["paidOutAmount"] = item.PaidOutAmount,
                    ["remainingCoverage"] = item.RemainingCoverage,
                    ["startDay"] = item.StartDay,
                    ["endDay"] = item.EndDay,
                    ["state"] = item.State.ToString(),
                    ["exclusions"] = StringListToObjects(item.Exclusions),
                    ["requiredEvidenceKinds"] = EnumListToObjects(item.RequiredEvidenceKinds),
                    ["claimIds"] = StringListToObjects(item.ClaimIds),
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["revision"] = item.Revision,
                    ["lastUpdatedDay"] = item.LastUpdatedDay,
                    ["lastEventId"] = item.LastEventId ?? "",
                });
            var claims = new List<object>();
            foreach (var item in runtime.Claims)
                claims.Add(new Dictionary<string, object>
                {
                    ["claimId"] = item.ClaimId ?? "",
                    ["policyId"] = item.PolicyId ?? "",
                    ["incidentEventId"] = item.IncidentEventId ?? "",
                    ["incidentTag"] = item.IncidentTag ?? "",
                    ["lossAmount"] = item.LossAmount,
                    ["approvedPayout"] = item.ApprovedPayout,
                    ["deniedAmount"] = item.DeniedAmount,
                    ["state"] = item.State.ToString(),
                    ["decisionCode"] = item.DecisionCode ?? "",
                    ["decisionExplanation"] = item.DecisionExplanation ?? "",
                    ["evidenceIds"] = StringListToObjects(item.EvidenceIds),
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision,
                });
            var seizures = new List<object>();
            foreach (var item in runtime.Seizures)
                seizures.Add(new Dictionary<string, object>
                {
                    ["seizureId"] = item.SeizureId ?? "",
                    ["debtId"] = item.DebtId ?? "",
                    ["assetRef"] = item.AssetRef ?? "",
                    ["status"] = item.Status ?? "",
                    ["legalSourceEventId"] = item.LegalSourceEventId ?? "",
                    ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay,
                    ["saveRevision"] = item.SaveRevision,
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion,
                ["revision"] = runtime.Revision,
                ["accounts"] = accounts,
                ["debts"] = debts,
                ["guarantees"] = guarantees,
                ["evidence"] = evidence,
                ["policies"] = policies,
                ["claims"] = claims,
                ["seizures"] = seizures,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
            };
        }

        private static D13FinanceRuntimeSnapshot D13FinanceFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13FinanceRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "accounts")) result.Accounts.Add(D13FinanceAccountFromObj(item));
            foreach (var item in ObjectList(obj, "debts")) result.Debts.Add(D13FinanceDebtFromObj(item));
            foreach (var item in ObjectList(obj, "guarantees")) result.Guarantees.Add(D13FinanceGuaranteeFromObj(item));
            foreach (var item in ObjectList(obj, "evidence")) result.Evidence.Add(D13FinanceEvidenceFromObj(item));
            foreach (var item in ObjectList(obj, "policies")) result.Policies.Add(D13FinancePolicyFromObj(item));
            foreach (var item in ObjectList(obj, "claims")) result.Claims.Add(D13FinanceClaimFromObj(item));
            foreach (var item in ObjectList(obj, "seizures")) result.Seizures.Add(D13FinanceSeizureFromObj(item));
            result.Validate();
            return result;
        }

        private static D13FinanceAccountState D13FinanceAccountFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceAccountState
            {
                AccountId = SaveJson.GetStr(obj, "accountId", ""),
                OwnerRef = SaveJson.GetStr(obj, "ownerRef", ""),
                Currency = SaveJson.GetStr(obj, "currency", D13FinanceIds.Currency),
                CashBalance = Math.Max(0L, ReadLong(obj, "cashBalance")),
                CreditLimit = Math.Max(0L, ReadLong(obj, "creditLimit")),
                CreditUsed = Math.Max(0L, ReadLong(obj, "creditUsed")),
                CreditScore = Math.Max(0, Math.Min(100, SaveJson.GetInt(obj, "creditScore"))),
                BankruptcyProtectionUntilDay = Math.Max(0, SaveJson.GetInt(obj, "bankruptcyProtectionUntilDay")),
                BankruptcyProtectionCount = Math.Max(0, SaveJson.GetInt(obj, "bankruptcyProtectionCount")),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "accountType", "PlayerCashAndCredit"), true, out result.AccountType);
            ReadStringListObject(result.CreditReasons, obj, "creditReasons");
            ReadStringListObject(result.DebtIds, obj, "debtIds");
            ReadStringListObject(result.PolicyIds, obj, "policyIds");
            return result;
        }

        private static D13FinanceDebtState D13FinanceDebtFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceDebtState
            {
                DebtId = SaveJson.GetStr(obj, "debtId", ""),
                AccountId = SaveJson.GetStr(obj, "accountId", ""),
                DebtorRef = SaveJson.GetStr(obj, "debtorRef", ""),
                CreditorRef = SaveJson.GetStr(obj, "creditorRef", ""),
                Principal = Math.Max(0L, ReadLong(obj, "principal")),
                InterestAccrued = Math.Max(0L, ReadLong(obj, "interestAccrued")),
                LatePenalty = Math.Max(0L, ReadLong(obj, "latePenalty")),
                InterestRatePercent = Math.Max(0, SaveJson.GetInt(obj, "interestRatePercent")),
                LatePenaltyPercent = Math.Max(0, SaveJson.GetInt(obj, "latePenaltyPercent")),
                DueDay = Math.Max(0, SaveJson.GetInt(obj, "dueDay")),
                GraceUntilDay = Math.Max(0, SaveJson.GetInt(obj, "graceUntilDay")),
                LastAccrualDay = Math.Max(0, SaveJson.GetInt(obj, "lastAccrualDay")),
                LastPaymentDay = Math.Max(0, SaveJson.GetInt(obj, "lastPaymentDay")),
                RenegotiationCount = Math.Max(0, SaveJson.GetInt(obj, "renegotiationCount")),
                LedgerRef = EmptyToNull(SaveJson.GetStr(obj, "ledgerRef", "")),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                RiskExplanation = SaveJson.GetStr(obj, "riskExplanation", ""),
                WorstCaseOutcome = SaveJson.GetStr(obj, "worstCaseOutcome", ""),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "kind", "Loan"), true, out result.Kind);
            Enum.TryParse(SaveJson.GetStr(obj, "state", "Active"), true, out result.State);
            Enum.TryParse(SaveJson.GetStr(obj, "stateBeforeProtection", "Active"), true, out result.StateBeforeProtection);
            Enum.TryParse(SaveJson.GetStr(obj, "collectionState", "None"), true, out result.CollectionState);
            ReadStringListObject(result.GuaranteeIds, obj, "guaranteeIds");
            foreach (var item in ObjectList(obj, "collateral"))
                result.Collateral.Add(new D13FinanceCollateralState
                {
                    AssetRef = SaveJson.GetStr(item, "assetRef", ""),
                    AssetKind = SaveJson.GetStr(item, "assetKind", "property"),
                    ClaimValue = Math.Max(0L, ReadLong(item, "claimValue")),
                    ValuationSourceOwnerSystem = SaveJson.GetStr(item, "valuationSourceOwnerSystem", ""),
                    ValuationSourceEventId = SaveJson.GetStr(item, "valuationSourceEventId", ""),
                    LegalSourceOwnerSystem = SaveJson.GetStr(item, "legalSourceOwnerSystem", ""),
                    LegalSourceEventId = SaveJson.GetStr(item, "legalSourceEventId", ""),
                    SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""),
                });
            return result;
        }

        private static D13FinanceGuaranteeState D13FinanceGuaranteeFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceGuaranteeState
            {
                GuaranteeId = SaveJson.GetStr(obj, "guaranteeId", ""),
                DebtId = SaveJson.GetStr(obj, "debtId", ""),
                GuarantorRef = SaveJson.GetStr(obj, "guarantorRef", ""),
                PrincipalCap = Math.Max(0L, ReadLong(obj, "principalCap")),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "state", "Active"), true, out result.State);
            return result;
        }

        private static D13FinanceEvidenceRecord D13FinanceEvidenceFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceEvidenceRecord
            {
                EvidenceId = SaveJson.GetStr(obj, "evidenceId", ""),
                OwnerRef = SaveJson.GetStr(obj, "ownerRef", ""),
                FactEventId = SaveJson.GetStr(obj, "factEventId", ""),
                FactSaveRevision = Math.Max(0L, ReadLong(obj, "factSaveRevision")),
                Description = SaveJson.GetStr(obj, "description", ""),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "kind", "Incident"), true, out result.Kind);
            return result;
        }

        private static D13FinanceInsurancePolicyState D13FinancePolicyFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceInsurancePolicyState
            {
                PolicyId = SaveJson.GetStr(obj, "policyId", ""),
                AccountId = SaveJson.GetStr(obj, "accountId", ""),
                InsuredRef = SaveJson.GetStr(obj, "insuredRef", ""),
                AssetRef = EmptyToNull(SaveJson.GetStr(obj, "assetRef", "")),
                CoverageAmount = Math.Max(0L, ReadLong(obj, "coverageAmount")),
                PremiumPaid = Math.Max(0L, ReadLong(obj, "premiumPaid")),
                DeductibleAmount = Math.Max(0L, ReadLong(obj, "deductibleAmount")),
                MaxPayout = Math.Max(0L, ReadLong(obj, "maxPayout")),
                PaidOutAmount = Math.Max(0L, ReadLong(obj, "paidOutAmount")),
                RemainingCoverage = Math.Max(0L, ReadLong(obj, "remainingCoverage")),
                StartDay = Math.Max(0, SaveJson.GetInt(obj, "startDay")),
                EndDay = Math.Max(0, SaveJson.GetInt(obj, "endDay")),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "kind", "Cargo"), true, out result.Kind);
            Enum.TryParse(SaveJson.GetStr(obj, "state", "Active"), true, out result.State);
            ReadStringListObject(result.Exclusions, obj, "exclusions");
            ReadEnumList(result.RequiredEvidenceKinds, obj, "requiredEvidenceKinds");
            ReadStringListObject(result.ClaimIds, obj, "claimIds");
            return result;
        }

        private static D13FinanceInsuranceClaimState D13FinanceClaimFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FinanceInsuranceClaimState
            {
                ClaimId = SaveJson.GetStr(obj, "claimId", ""),
                PolicyId = SaveJson.GetStr(obj, "policyId", ""),
                IncidentEventId = SaveJson.GetStr(obj, "incidentEventId", ""),
                IncidentTag = SaveJson.GetStr(obj, "incidentTag", ""),
                LossAmount = Math.Max(1L, ReadLong(obj, "lossAmount")),
                ApprovedPayout = Math.Max(0L, ReadLong(obj, "approvedPayout")),
                DeniedAmount = Math.Max(0L, ReadLong(obj, "deniedAmount")),
                DecisionCode = SaveJson.GetStr(obj, "decisionCode", "claim_denied"),
                DecisionExplanation = SaveJson.GetStr(obj, "decisionExplanation", ""),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "state", "Submitted"), true, out result.State);
            ReadStringListObject(result.EvidenceIds, obj, "evidenceIds");
            return result;
        }

        private static D13FinanceSeizureState D13FinanceSeizureFromObj(Dictionary<string, object> obj)
        {
            return new D13FinanceSeizureState
            {
                SeizureId = SaveJson.GetStr(obj, "seizureId", ""),
                DebtId = SaveJson.GetStr(obj, "debtId", ""),
                AssetRef = SaveJson.GetStr(obj, "assetRef", ""),
                Status = SaveJson.GetStr(obj, "status", ""),
                LegalSourceEventId = SaveJson.GetStr(obj, "legalSourceEventId", ""),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
        }

        private static List<object> EnumListToObjects<T>(IEnumerable<T> values)
        {
            var result = new List<object>();
            if (values != null) foreach (var value in values) result.Add(value.ToString());
            return result;
        }

        private static void ReadEnumList<T>(List<T> target, Dictionary<string, object> source, string key)
            where T : struct
        {
            if (target == null || source == null || !source.TryGetValue(key, out object value) || !(value is List<object> list)) return;
            foreach (var item in list)
            {
                T parsed;
                if (Enum.TryParse(Convert.ToString(item), true, out parsed)) target.Add(parsed);
            }
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

        private static Dictionary<string, object> D13FarmingToObj(D13FarmingRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var plots = new List<object>();
            foreach (var item in runtime.Plots)
                plots.Add(new Dictionary<string, object>
                {
                    ["plotId"] = item.PlotId ?? "", ["propertyId"] = item.PropertyId ?? "", ["plantingId"] = item.PlantingId ?? "",
                    ["cropId"] = item.CropId ?? "", ["seedItemId"] = item.SeedItemId ?? "", ["cropKind"] = item.CropKind.ToString(),
                    ["state"] = item.State.ToString(), ["plantedSeason"] = item.PlantedSeason.ToString(), ["plantDay"] = item.PlantDay,
                    ["lastUpdatedDay"] = item.LastUpdatedDay, ["growthPoints"] = item.GrowthPoints, ["growthPercent"] = item.GrowthPercent,
                    ["waterApplied"] = item.WaterApplied, ["laborApplied"] = item.LaborApplied, ["pestPressure"] = item.PestPressure,
                    ["stormDamage"] = item.StormDamage, ["health"] = item.Health, ["harvestCount"] = item.HarvestCount,
                    ["rotationIndex"] = item.RotationIndex, ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "",
                    ["rotationHistory"] = StringListToObjects(item.RotationHistory),
                });
            var processes = new List<object>();
            foreach (var item in runtime.Processes)
                processes.Add(new Dictionary<string, object>
                {
                    ["processId"] = item.ProcessId ?? "", ["plotId"] = item.PlotId ?? "", ["inputItemId"] = item.InputItemId ?? "",
                    ["outputItemId"] = item.OutputItemId ?? "", ["inputQuantity"] = item.InputQuantity, ["outputQuantity"] = item.OutputQuantity,
                    ["sourceEventId"] = item.SourceEventId ?? "", ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var sales = new List<object>();
            foreach (var item in runtime.Sales)
                sales.Add(new Dictionary<string, object>
                {
                    ["saleId"] = item.SaleId ?? "", ["itemId"] = item.ItemId ?? "", ["quantity"] = item.Quantity,
                    ["totalPrice"] = item.TotalPrice, ["unitPrice"] = item.UnitPrice, ["currency"] = item.Currency ?? D13FarmingIds.Currency,
                    ["marketRef"] = item.MarketRef ?? "", ["ledgerRef"] = item.LedgerRef ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var environments = new List<object>();
            foreach (var item in runtime.Ecology.Environments)
                environments.Add(new Dictionary<string, object>
                {
                    ["plotId"] = item.PlotId ?? "", ["soilType"] = item.SoilType ?? "", ["fertility"] = item.Fertility,
                    ["moisture"] = item.Moisture, ["waterAvailable"] = item.WaterAvailable, ["waterCapacity"] = item.WaterCapacity,
                    ["waterSourceId"] = item.WaterSourceId ?? "", ["pollution"] = item.Pollution, ["ecologyPressure"] = item.EcologyPressure,
                    ["pestPressure"] = item.PestPressure, ["stormDamage"] = item.StormDamage, ["lastUpdatedDay"] = item.LastUpdatedDay,
                    ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "",
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["plots"] = plots,
                ["processes"] = processes, ["sales"] = sales,
                ["ecology"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = runtime.Ecology.SchemaVersion, ["revision"] = runtime.Ecology.Revision,
                    ["environments"] = environments, ["appliedEventKeys"] = StringListToObjects(runtime.Ecology.AppliedEventKeys),
                },
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
            };
        }

        private static D13FarmingRuntimeSnapshot D13FarmingFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FarmingRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13FarmingRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "plots")) result.Plots.Add(D13FarmingPlotFromObj(item));
            foreach (var item in ObjectList(obj, "processes")) result.Processes.Add(D13FarmingProcessFromObj(item));
            foreach (var item in ObjectList(obj, "sales")) result.Sales.Add(D13FarmingSaleFromObj(item));
            if (obj.TryGetValue("ecology", out object ecologyValue) && ecologyValue is Dictionary<string, object> ecologyObject)
                result.Ecology = D13FarmingEcologyFromObj(ecologyObject);
            result.Validate();
            return result;
        }

        private static D13FarmingPlotState D13FarmingPlotFromObj(Dictionary<string, object> obj)
        {
            Enum.TryParse(SaveJson.GetStr(obj, "cropKind", D13FarmingCropKind.Fruit.ToString()), true, out D13FarmingCropKind cropKind);
            Enum.TryParse(SaveJson.GetStr(obj, "state", D13FarmingCropStateKind.Empty.ToString()), true, out D13FarmingCropStateKind state);
            Enum.TryParse(SaveJson.GetStr(obj, "plantedSeason", D13FarmingSeason.Dry.ToString()), true, out D13FarmingSeason season);
            var result = new D13FarmingPlotState
            {
                PlotId = SaveJson.GetStr(obj, "plotId", ""), PropertyId = SaveJson.GetStr(obj, "propertyId", ""),
                PlantingId = EmptyToNull(SaveJson.GetStr(obj, "plantingId", "")), CropId = EmptyToNull(SaveJson.GetStr(obj, "cropId", "")),
                SeedItemId = EmptyToNull(SaveJson.GetStr(obj, "seedItemId", "")), CropKind = cropKind, State = state, PlantedSeason = season,
                PlantDay = Math.Max(0, SaveJson.GetInt(obj, "plantDay")), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                GrowthPoints = Math.Max(0, SaveJson.GetInt(obj, "growthPoints")), GrowthPercent = Math.Max(0, SaveJson.GetInt(obj, "growthPercent")),
                WaterApplied = Math.Max(0, SaveJson.GetInt(obj, "waterApplied")), LaborApplied = Math.Max(0, SaveJson.GetInt(obj, "laborApplied")),
                PestPressure = Math.Max(0, SaveJson.GetInt(obj, "pestPressure")), StormDamage = Math.Max(0, SaveJson.GetInt(obj, "stormDamage")),
                Health = Math.Max(0, SaveJson.GetInt(obj, "health", 100)), HarvestCount = Math.Max(0, SaveJson.GetInt(obj, "harvestCount")),
                RotationIndex = Math.Max(0, SaveJson.GetInt(obj, "rotationIndex")), Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            ReadStringListObject(result.RotationHistory, obj, "rotationHistory");
            return result;
        }

        private static D13FarmingProcessRecord D13FarmingProcessFromObj(Dictionary<string, object> obj)
        {
            return new D13FarmingProcessRecord
            {
                ProcessId = SaveJson.GetStr(obj, "processId", ""), PlotId = SaveJson.GetStr(obj, "plotId", ""),
                InputItemId = SaveJson.GetStr(obj, "inputItemId", ""), OutputItemId = SaveJson.GetStr(obj, "outputItemId", ""),
                InputQuantity = Math.Max(0, SaveJson.GetInt(obj, "inputQuantity")), OutputQuantity = Math.Max(0, SaveJson.GetInt(obj, "outputQuantity")),
                SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
        }

        private static D13FarmingSaleRecord D13FarmingSaleFromObj(Dictionary<string, object> obj)
        {
            return new D13FarmingSaleRecord
            {
                SaleId = SaveJson.GetStr(obj, "saleId", ""), ItemId = SaveJson.GetStr(obj, "itemId", ""), Quantity = Math.Max(0, SaveJson.GetInt(obj, "quantity")),
                TotalPrice = Math.Max(0L, ReadLong(obj, "totalPrice")), UnitPrice = Math.Max(0L, ReadLong(obj, "unitPrice")), Currency = SaveJson.GetStr(obj, "currency", D13FarmingIds.Currency),
                MarketRef = SaveJson.GetStr(obj, "marketRef", ""), LedgerRef = EmptyToNull(SaveJson.GetStr(obj, "ledgerRef", "")), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
        }

        private static D13FarmingEcologyRuntimeSnapshot D13FarmingEcologyFromObj(Dictionary<string, object> obj)
        {
            var result = new D13FarmingEcologyRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13FarmingEcologyRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "environments"))
                result.Environments.Add(new D13FarmingEcologyEnvironmentState
                {
                    PlotId = SaveJson.GetStr(item, "plotId", ""), SoilType = SaveJson.GetStr(item, "soilType", "loam"),
                    Fertility = SaveJson.GetInt(item, "fertility", 70), Moisture = SaveJson.GetInt(item, "moisture", 50),
                    WaterAvailable = SaveJson.GetInt(item, "waterAvailable", 100), WaterCapacity = SaveJson.GetInt(item, "waterCapacity", 100),
                    WaterSourceId = SaveJson.GetStr(item, "waterSourceId", ""), Pollution = SaveJson.GetInt(item, "pollution"), EcologyPressure = SaveJson.GetInt(item, "ecologyPressure"),
                    PestPressure = SaveJson.GetInt(item, "pestPressure"), StormDamage = SaveJson.GetInt(item, "stormDamage"), LastUpdatedDay = SaveJson.GetInt(item, "lastUpdatedDay"),
                    Revision = ReadLong(item, "revision"), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")),
                });
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D13BuildingToObj(D13BuildingRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var buildings = new List<object>();
            foreach (var item in runtime.Buildings) buildings.Add(new Dictionary<string, object>
            {
                ["buildingId"] = item.BuildingId ?? "", ["propertyId"] = item.PropertyId ?? "", ["islandId"] = item.IslandId ?? "", ["kind"] = item.Kind.ToString(),
                ["lifecycle"] = item.Lifecycle.ToString(), ["damageState"] = item.DamageState.ToString(), ["damagePoints"] = item.DamagePoints,
                ["buildDaysRequired"] = item.BuildDaysRequired, ["buildDaysElapsed"] = item.BuildDaysElapsed, ["workerCount"] = item.WorkerCount,
                ["occupationState"] = item.OccupationState.ToString(), ["occupyingFactionId"] = item.OccupyingFactionId ?? "", ["occupationSourceEventId"] = item.OccupationSourceEventId ?? "",
                ["mapState"] = item.MapState.ToString(), ["lastUpdatedDay"] = item.LastUpdatedDay, ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "",
            });
            var orders = new List<object>();
            foreach (var item in runtime.Orders)
            {
                var materials = new List<object>(); foreach (var material in item.Materials) materials.Add(new Dictionary<string, object> { ["itemId"] = material.ItemId ?? "", ["quantity"] = material.Quantity });
                orders.Add(new Dictionary<string, object>
                {
                    ["orderId"] = item.OrderId ?? "", ["buildingId"] = item.BuildingId ?? "", ["propertyId"] = item.PropertyId ?? "", ["state"] = item.State.ToString(),
                    ["startDay"] = item.StartDay, ["dueDay"] = item.DueDay, ["workerCount"] = item.WorkerCount, ["daysElapsed"] = item.DaysElapsed, ["materials"] = materials,
                    ["propertySourceEventId"] = item.PropertySourceEventId ?? "", ["inventorySourceEventId"] = item.InventorySourceEventId ?? "", ["workforceSourceEventId"] = item.WorkforceSourceEventId ?? "",
                    ["lastUpdatedDay"] = item.LastUpdatedDay, ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "",
                });
            }
            var services = new List<object>();
            foreach (var item in runtime.Services) services.Add(new Dictionary<string, object>
            {
                ["serviceRecordId"] = item.ServiceRecordId ?? "", ["buildingId"] = item.BuildingId ?? "", ["serviceId"] = item.ServiceId ?? "", ["actorRef"] = item.ActorRef ?? "",
                ["priceAmount"] = item.PriceAmount, ["currency"] = item.Currency ?? D13BuildingIds.Currency, ["ledgerRef"] = item.LedgerRef ?? "", ["economySourceEventId"] = item.EconomySourceEventId ?? "",
                ["inventorySourceEventId"] = item.InventorySourceEventId ?? "", ["shipSourceEventId"] = item.ShipSourceEventId ?? "", ["outputSummary"] = item.OutputSummary ?? "", ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
            });
            var fires = new List<object>();
            foreach (var item in runtime.Fires) fires.Add(new Dictionary<string, object>
            {
                ["fireId"] = item.FireId ?? "", ["buildingId"] = item.BuildingId ?? "", ["severity"] = item.Severity, ["damagePoints"] = item.DamagePoints,
                ["extinguished"] = item.Extinguished, ["causeEventId"] = item.CauseEventId ?? "", ["recoveryRouteId"] = item.RecoveryRouteId ?? "", ["worldDay"] = item.WorldDay, ["revision"] = item.Revision,
            });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["buildings"] = buildings, ["orders"] = orders, ["services"] = services, ["fires"] = fires,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys), ["lastWorldDay"] = runtime.LastWorldDay, ["lastEventId"] = runtime.LastEventId ?? "",
            };
        }

        private static D13BuildingRuntimeSnapshot D13BuildingFromObj(Dictionary<string, object> obj)
        {
            var result = new D13BuildingRuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13BuildingRuntimeSnapshot.CurrentSchemaVersion), Revision = Math.Max(0L, ReadLong(obj, "revision")), LastWorldDay = Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")), LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")) };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "buildings"))
            {
                Enum.TryParse(SaveJson.GetStr(item, "kind", D13BuildingKind.Dock.ToString()), true, out D13BuildingKind kind);
                Enum.TryParse(SaveJson.GetStr(item, "lifecycle", D13BuildingLifecycle.Planned.ToString()), true, out D13BuildingLifecycle lifecycle);
                Enum.TryParse(SaveJson.GetStr(item, "damageState", D13BuildingDamageState.None.ToString()), true, out D13BuildingDamageState damage);
                Enum.TryParse(SaveJson.GetStr(item, "occupationState", D13BuildingOccupationState.Free.ToString()), true, out D13BuildingOccupationState occupation);
                Enum.TryParse(SaveJson.GetStr(item, "mapState", D13BuildingMapState.Hidden.ToString()), true, out D13BuildingMapState map);
                result.Buildings.Add(new D13BuildingState { BuildingId = SaveJson.GetStr(item, "buildingId", ""), PropertyId = SaveJson.GetStr(item, "propertyId", ""), IslandId = SaveJson.GetStr(item, "islandId", ""), Kind = kind, Lifecycle = lifecycle, DamageState = damage, DamagePoints = Math.Max(0, SaveJson.GetInt(item, "damagePoints")), BuildDaysRequired = Math.Max(1, SaveJson.GetInt(item, "buildDaysRequired", 1)), BuildDaysElapsed = Math.Max(0, SaveJson.GetInt(item, "buildDaysElapsed")), WorkerCount = Math.Max(0, SaveJson.GetInt(item, "workerCount")), OccupationState = occupation, OccupyingFactionId = EmptyToNull(SaveJson.GetStr(item, "occupyingFactionId", "")), OccupationSourceEventId = EmptyToNull(SaveJson.GetStr(item, "occupationSourceEventId", "")), MapState = map, LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), Revision = Math.Max(0L, ReadLong(item, "revision")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) });
            }
            foreach (var item in ObjectList(obj, "orders"))
            {
                Enum.TryParse(SaveJson.GetStr(item, "state", D13ConstructionOrderStatus.InProgress.ToString()), true, out D13ConstructionOrderStatus state);
                var order = new D13ConstructionOrderState { OrderId = SaveJson.GetStr(item, "orderId", ""), BuildingId = SaveJson.GetStr(item, "buildingId", ""), PropertyId = SaveJson.GetStr(item, "propertyId", ""), State = state, StartDay = Math.Max(0, SaveJson.GetInt(item, "startDay")), DueDay = Math.Max(0, SaveJson.GetInt(item, "dueDay")), WorkerCount = Math.Max(1, SaveJson.GetInt(item, "workerCount", 1)), DaysElapsed = Math.Max(0, SaveJson.GetInt(item, "daysElapsed")), PropertySourceEventId = SaveJson.GetStr(item, "propertySourceEventId", ""), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""), WorkforceSourceEventId = SaveJson.GetStr(item, "workforceSourceEventId", ""), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), Revision = Math.Max(0L, ReadLong(item, "revision")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) };
                foreach (var material in ObjectList(item, "materials")) order.Materials.Add(new D13BuildingMaterialRequirement { ItemId = SaveJson.GetStr(material, "itemId", ""), Quantity = Math.Max(1, SaveJson.GetInt(material, "quantity", 1)) });
                result.Orders.Add(order);
            }
            foreach (var item in ObjectList(obj, "services")) result.Services.Add(new D13BuildingServiceRecord { ServiceRecordId = SaveJson.GetStr(item, "serviceRecordId", ""), BuildingId = SaveJson.GetStr(item, "buildingId", ""), ServiceId = SaveJson.GetStr(item, "serviceId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", D13BuildingIds.PlayerRef), PriceAmount = Math.Max(0, SaveJson.GetInt(item, "priceAmount")), Currency = SaveJson.GetStr(item, "currency", D13BuildingIds.Currency), LedgerRef = SaveJson.GetStr(item, "ledgerRef", ""), EconomySourceEventId = SaveJson.GetStr(item, "economySourceEventId", ""), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""), ShipSourceEventId = EmptyToNull(SaveJson.GetStr(item, "shipSourceEventId", "")), OutputSummary = EmptyToNull(SaveJson.GetStr(item, "outputSummary", "")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")) });
            foreach (var item in ObjectList(obj, "fires")) result.Fires.Add(new D13BuildingFireIncident { FireId = SaveJson.GetStr(item, "fireId", ""), BuildingId = SaveJson.GetStr(item, "buildingId", ""), Severity = SaveJson.GetInt(item, "severity", 1), DamagePoints = Math.Max(1, SaveJson.GetInt(item, "damagePoints", 25)), Extinguished = ReadBool(item, "extinguished", false), CauseEventId = SaveJson.GetStr(item, "causeEventId", ""), RecoveryRouteId = SaveJson.GetStr(item, "recoveryRouteId", D13BuildingIds.FireRecoveryRoute), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), Revision = Math.Max(0L, ReadLong(item, "revision")) });
            result.Validate(); return result;
        }

        private static Dictionary<string, object> D13DynamicMarketToObj(D13DynamicMarketRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var flows = new List<object>();
            foreach (var item in runtime.Flows)
                flows.Add(new Dictionary<string, object>
                {
                    ["flowId"] = item.FlowId ?? "", ["kind"] = item.Kind.ToString(), ["itemId"] = item.ItemId ?? "", ["quantity"] = item.Quantity,
                    ["originLocationId"] = item.OriginLocationId ?? "", ["destinationLocationId"] = item.DestinationLocationId ?? "", ["marketId"] = item.MarketId ?? "",
                    ["unitPrice"] = item.UnitPrice, ["totalValue"] = item.TotalValue, ["currency"] = item.Currency ?? D13MarketTransportIds.Currency, ["ledgerRef"] = item.LedgerRef ?? "",
                    ["producerSourceEventId"] = item.ProducerSourceEventId ?? "", ["consumerSourceEventId"] = item.ConsumerSourceEventId ?? "", ["inventorySourceEventId"] = item.InventorySourceEventId ?? "",
                    ["economySourceEventId"] = item.EconomySourceEventId ?? "", ["marketSourceEventId"] = item.MarketSourceEventId ?? "", ["legalSourceEventId"] = item.LegalSourceEventId ?? "",
                    ["ecologySourceEventId"] = item.EcologySourceEventId ?? "", ["assetSourceEventId"] = item.AssetSourceEventId ?? "", ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var shipments = new List<object>();
            foreach (var item in runtime.Shipments)
                shipments.Add(new Dictionary<string, object>
                {
                    ["shipmentId"] = item.ShipmentId ?? "", ["routeId"] = item.RouteId ?? "", ["itemId"] = item.ItemId ?? "", ["originLocationId"] = item.OriginLocationId ?? "", ["destinationLocationId"] = item.DestinationLocationId ?? "", ["shipId"] = item.ShipId ?? "",
                    ["requestedQuantity"] = item.RequestedQuantity, ["remainingQuantity"] = item.RemainingQuantity, ["lostQuantity"] = item.LostQuantity, ["deliveredQuantity"] = item.DeliveredQuantity,
                    ["status"] = item.Status.ToString(), ["lastWeatherRiskPercent"] = item.LastWeatherRiskPercent, ["lastAssetRiskPercent"] = item.LastAssetRiskPercent, ["lastEcologyRiskPercent"] = item.LastEcologyRiskPercent,
                    ["legalSourceEventId"] = item.LegalSourceEventId ?? "", ["inventoryLoadSourceEventId"] = item.InventoryLoadSourceEventId ?? "", ["inventoryArrivalSourceEventId"] = item.InventoryArrivalSourceEventId ?? "", ["assetSourceEventId"] = item.AssetSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["createdDay"] = item.CreatedDay, ["lastUpdatedDay"] = item.LastUpdatedDay, ["revision"] = item.Revision,
                });
            return new Dictionary<string, object> { ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["flows"] = flows, ["shipments"] = shipments, ["appliedEventKeys"] = runtime.AppliedEventKeys };
        }

        private static D13DynamicMarketRuntimeSnapshot D13DynamicMarketFromObj(Dictionary<string, object> obj)
        {
            var result = new D13DynamicMarketRuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13DynamicMarketRuntimeSnapshot.CurrentSchemaVersion), Revision = Math.Max(0L, ReadLong(obj, "revision")) };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "flows"))
            {
                Enum.TryParse(SaveJson.GetStr(item, "kind", D13MarketFlowKind.Production.ToString()), true, out D13MarketFlowKind kind);
                result.Flows.Add(new D13MarketFlowRecord
                {
                    FlowId = SaveJson.GetStr(item, "flowId", ""), Kind = kind, ItemId = SaveJson.GetStr(item, "itemId", ""), Quantity = Math.Max(0, SaveJson.GetInt(item, "quantity")), OriginLocationId = EmptyToNull(SaveJson.GetStr(item, "originLocationId", "")), DestinationLocationId = EmptyToNull(SaveJson.GetStr(item, "destinationLocationId", "")), MarketId = EmptyToNull(SaveJson.GetStr(item, "marketId", "")), UnitPrice = Math.Max(0, SaveJson.GetInt(item, "unitPrice")), TotalValue = Math.Max(0L, ReadLong(item, "totalValue")), Currency = SaveJson.GetStr(item, "currency", D13MarketTransportIds.Currency), LedgerRef = EmptyToNull(SaveJson.GetStr(item, "ledgerRef", "")), ProducerSourceEventId = EmptyToNull(SaveJson.GetStr(item, "producerSourceEventId", "")), ConsumerSourceEventId = EmptyToNull(SaveJson.GetStr(item, "consumerSourceEventId", "")), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""), EconomySourceEventId = SaveJson.GetStr(item, "economySourceEventId", ""), MarketSourceEventId = SaveJson.GetStr(item, "marketSourceEventId", ""), LegalSourceEventId = SaveJson.GetStr(item, "legalSourceEventId", ""), EcologySourceEventId = EmptyToNull(SaveJson.GetStr(item, "ecologySourceEventId", "")), AssetSourceEventId = EmptyToNull(SaveJson.GetStr(item, "assetSourceEventId", "")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                });
            }
            foreach (var item in ObjectList(obj, "shipments"))
            {
                Enum.TryParse(SaveJson.GetStr(item, "status", D13TransportStatus.Loaded.ToString()), true, out D13TransportStatus status);
                result.Shipments.Add(new D13TransportShipmentState
                {
                    ShipmentId = SaveJson.GetStr(item, "shipmentId", ""), RouteId = SaveJson.GetStr(item, "routeId", ""), ItemId = SaveJson.GetStr(item, "itemId", ""), OriginLocationId = SaveJson.GetStr(item, "originLocationId", ""), DestinationLocationId = SaveJson.GetStr(item, "destinationLocationId", ""), ShipId = EmptyToNull(SaveJson.GetStr(item, "shipId", "")), RequestedQuantity = Math.Max(0, SaveJson.GetInt(item, "requestedQuantity")), RemainingQuantity = Math.Max(0, SaveJson.GetInt(item, "remainingQuantity")), LostQuantity = Math.Max(0, SaveJson.GetInt(item, "lostQuantity")), DeliveredQuantity = Math.Max(0, SaveJson.GetInt(item, "deliveredQuantity")), Status = status, LastWeatherRiskPercent = Math.Max(0, SaveJson.GetInt(item, "lastWeatherRiskPercent")), LastAssetRiskPercent = Math.Max(0, SaveJson.GetInt(item, "lastAssetRiskPercent")), LastEcologyRiskPercent = Math.Max(0, SaveJson.GetInt(item, "lastEcologyRiskPercent")), LegalSourceEventId = SaveJson.GetStr(item, "legalSourceEventId", ""), InventoryLoadSourceEventId = SaveJson.GetStr(item, "inventoryLoadSourceEventId", ""), InventoryArrivalSourceEventId = EmptyToNull(SaveJson.GetStr(item, "inventoryArrivalSourceEventId", "")), AssetSourceEventId = EmptyToNull(SaveJson.GetStr(item, "assetSourceEventId", "")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")), CreatedDay = Math.Max(0, SaveJson.GetInt(item, "createdDay")), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), Revision = Math.Max(0L, ReadLong(item, "revision")),
                });
            }
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D14ToObj(D14NavalCombatRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var state = runtime.State;
            var kinematics = new Dictionary<string, object>
            {
                ["distance"] = state.Kinematics.Distance, ["relativeBearingDegrees"] = state.Kinematics.RelativeBearingDegrees,
                ["playerHeadingDegrees"] = state.Kinematics.PlayerHeadingDegrees, ["enemyHeadingDegrees"] = state.Kinematics.EnemyHeadingDegrees,
                ["playerSpeed"] = state.Kinematics.PlayerSpeed, ["enemySpeed"] = state.Kinematics.EnemySpeed,
            };
            var playerDamage = D14DamageToObj(state.PlayerDamage);
            var enemyDamage = D14DamageToObj(state.EnemyDamage);
            var playerGuns = new List<object>();
            foreach (var gun in state.PlayerGuns) playerGuns.Add(D14GunToObj(gun));
            var enemyGuns = new List<object>();
            foreach (var gun in state.EnemyGuns) enemyGuns.Add(D14GunToObj(gun));
            var stateObj = new Dictionary<string, object>
            {
                ["encounterId"] = state.EncounterId ?? "", ["playerShipId"] = state.PlayerShipId ?? "", ["enemyShipId"] = state.EnemyShipId ?? "",
                ["status"] = state.Status.ToString(), ["kinematics"] = kinematics, ["playerDamage"] = playerDamage, ["enemyDamage"] = enemyDamage,
                ["playerGuns"] = playerGuns, ["enemyGuns"] = enemyGuns, ["playerGunnerSkill"] = state.PlayerGunnerSkill,
                ["playerManeuverSkill"] = state.PlayerManeuverSkill, ["enemyGunnerSkill"] = state.EnemyGunnerSkill,
                ["enemyManeuverSkill"] = state.EnemyManeuverSkill, ["turn"] = state.Turn, ["worldDay"] = state.WorldDay,
                ["saveRevision"] = state.SaveRevision, ["weatherSourceEventId"] = state.WeatherSourceEventId ?? "", ["lastEventId"] = state.LastEventId ?? "",
            };
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["state"] = stateObj,
                ["appliedEventKeys"] = runtime.AppliedEventKeys, ["combatEventIds"] = runtime.CombatEventIds,
            };
            if (runtime.Weather != null) result["weather"] = WeatherSeaToObj(runtime.Weather);
            if (runtime.EventLog != null) result["eventLog"] = EventLogToObj(runtime.EventLog);
            return result;
        }

        private static Dictionary<string, object> D14DamageToObj(D14HullDamageState damage)
        {
            return new Dictionary<string, object>
            {
                ["currentHull"] = damage.CurrentHull, ["maxHull"] = damage.MaxHull, ["crew"] = damage.Crew, ["maxCrew"] = damage.MaxCrew,
                ["mastIntegrity"] = damage.MastIntegrity, ["rudderIntegrity"] = damage.RudderIntegrity, ["gunOperationalCount"] = damage.GunOperationalCount,
                ["fireLevel"] = damage.FireLevel, ["floodingLevel"] = damage.FloodingLevel, ["powderRisk"] = damage.PowderRisk, ["tiltDegrees"] = damage.TiltDegrees,
            };
        }

        private static Dictionary<string, object> D14GunToObj(D14GunState gun)
        {
            return new Dictionary<string, object>
            {
                ["gunId"] = gun.GunId ?? "", ["position"] = gun.Position.ToString(), ["roundShot"] = gun.RoundShot,
                ["chainShot"] = gun.ChainShot, ["grapeShot"] = gun.GrapeShot, ["loadedAmmo"] = gun.LoadedAmmo.ToString(),
                ["reloadRemainingTurns"] = gun.ReloadRemainingTurns, ["conditionPercent"] = gun.ConditionPercent, ["disabled"] = gun.Disabled,
                ["hasAim"] = gun.HasAim, ["aimBearingDegrees"] = gun.AimBearingDegrees, ["aimTargetZone"] = gun.AimTargetZone.ToString(),
            };
        }

        private static D14NavalCombatRuntimeSnapshot D14FromObj(Dictionary<string, object> obj)
        {
            var result = new D14NavalCombatRuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14NavalCombatRuntimeSnapshot.CurrentSchemaVersion) };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            ReadStringListObject(result.CombatEventIds, obj, "combatEventIds");
            var stateObj = Nested(obj, "state");
            if (stateObj != null)
            {
                var state = new D14NavalCombatState
                {
                    EncounterId = SaveJson.GetStr(stateObj, "encounterId", ""), PlayerShipId = SaveJson.GetStr(stateObj, "playerShipId", ""), EnemyShipId = SaveJson.GetStr(stateObj, "enemyShipId", ""),
                    PlayerGunnerSkill = SaveJson.GetInt(stateObj, "playerGunnerSkill"), PlayerManeuverSkill = SaveJson.GetInt(stateObj, "playerManeuverSkill"),
                    EnemyGunnerSkill = SaveJson.GetInt(stateObj, "enemyGunnerSkill"), EnemyManeuverSkill = SaveJson.GetInt(stateObj, "enemyManeuverSkill"),
                    Turn = Math.Max(0, SaveJson.GetInt(stateObj, "turn")), WorldDay = Math.Max(0, SaveJson.GetInt(stateObj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(stateObj, "saveRevision")),
                    WeatherSourceEventId = EmptyToNull(SaveJson.GetStr(stateObj, "weatherSourceEventId", "")), LastEventId = EmptyToNull(SaveJson.GetStr(stateObj, "lastEventId", "")),
                };
                if (!Enum.TryParse(SaveJson.GetStr(stateObj, "status", D14NavalCombatStatus.Idle.ToString()), true, out D14NavalCombatStatus status)) status = D14NavalCombatStatus.Idle;
                state.Status = status;
                var kinematics = Nested(stateObj, "kinematics");
                if (kinematics != null)
                    state.Kinematics = new D14CombatKinematicsState
                    {
                        Distance = SaveJson.GetFloat(kinematics, "distance"), RelativeBearingDegrees = SaveJson.GetFloat(kinematics, "relativeBearingDegrees"),
                        PlayerHeadingDegrees = SaveJson.GetFloat(kinematics, "playerHeadingDegrees"), EnemyHeadingDegrees = SaveJson.GetFloat(kinematics, "enemyHeadingDegrees"),
                        PlayerSpeed = SaveJson.GetFloat(kinematics, "playerSpeed"), EnemySpeed = SaveJson.GetFloat(kinematics, "enemySpeed"),
                    };
                state.PlayerDamage = D14DamageFromObj(Nested(stateObj, "playerDamage"));
                state.EnemyDamage = D14DamageFromObj(Nested(stateObj, "enemyDamage"));
                foreach (var gun in ObjectList(stateObj, "playerGuns")) state.PlayerGuns.Add(D14GunFromObj(gun));
                foreach (var gun in ObjectList(stateObj, "enemyGuns")) state.EnemyGuns.Add(D14GunFromObj(gun));
                result.State = state;
            }
            var weather = Nested(obj, "weather");
            if (weather != null) result.Weather = WeatherSeaFromObj(weather);
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate();
            return result;
        }

        private static D14HullDamageState D14DamageFromObj(Dictionary<string, object> obj)
        {
            if (obj == null) return new D14HullDamageState();
            return new D14HullDamageState
            {
                CurrentHull = SaveJson.GetInt(obj, "currentHull"), MaxHull = SaveJson.GetInt(obj, "maxHull"), Crew = SaveJson.GetInt(obj, "crew"), MaxCrew = SaveJson.GetInt(obj, "maxCrew"),
                MastIntegrity = SaveJson.GetInt(obj, "mastIntegrity", 100), RudderIntegrity = SaveJson.GetInt(obj, "rudderIntegrity", 100), GunOperationalCount = SaveJson.GetInt(obj, "gunOperationalCount"),
                FireLevel = SaveJson.GetInt(obj, "fireLevel"), FloodingLevel = SaveJson.GetInt(obj, "floodingLevel"), PowderRisk = SaveJson.GetInt(obj, "powderRisk"), TiltDegrees = SaveJson.GetFloat(obj, "tiltDegrees"),
            };
        }

        private static D14GunState D14GunFromObj(Dictionary<string, object> obj)
        {
            var result = new D14GunState
            {
                GunId = SaveJson.GetStr(obj, "gunId", ""), RoundShot = Math.Max(0, SaveJson.GetInt(obj, "roundShot")), ChainShot = Math.Max(0, SaveJson.GetInt(obj, "chainShot")),
                GrapeShot = Math.Max(0, SaveJson.GetInt(obj, "grapeShot")), ReloadRemainingTurns = Math.Max(0, SaveJson.GetInt(obj, "reloadRemainingTurns")),
                ConditionPercent = SaveJson.GetInt(obj, "conditionPercent", 100), Disabled = ReadBool(obj, "disabled"), HasAim = ReadBool(obj, "hasAim"),
                AimBearingDegrees = SaveJson.GetFloat(obj, "aimBearingDegrees"),
            };
            if (!Enum.TryParse(SaveJson.GetStr(obj, "position", D14GunPosition.PortMidships.ToString()), true, out D14GunPosition position)) position = D14GunPosition.PortMidships;
            if (!Enum.TryParse(SaveJson.GetStr(obj, "loadedAmmo", D14NavalAmmoType.None.ToString()), true, out D14NavalAmmoType ammo)) ammo = D14NavalAmmoType.None;
            if (!Enum.TryParse(SaveJson.GetStr(obj, "aimTargetZone", D14TargetZone.Auto.ToString()), true, out D14TargetZone zone)) zone = D14TargetZone.Auto;
            result.Position = position; result.LoadedAmmo = ammo; result.AimTargetZone = zone;
            return result;
        }

        private static Dictionary<string, object> D14AftercareToObj(D14AftercareRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var cases = new List<object>();
            foreach (var item in runtime.Cases)
            {
                var players = new List<object>(); foreach (var participant in item.PlayerParty) players.Add(D14ParticipantToObj(participant));
                var enemies = new List<object>(); foreach (var participant in item.EnemyParty) enemies.Add(D14ParticipantToObj(participant));
                cases.Add(new Dictionary<string, object>
                {
                    ["caseId"] = item.CaseId ?? "", ["encounterId"] = item.EncounterId ?? "", ["playerShipId"] = item.PlayerShipId ?? "", ["enemyShipId"] = item.EnemyShipId ?? "",
                    ["status"] = item.Status.ToString(), ["outcome"] = item.Outcome.ToString(), ["distance"] = item.Distance, ["playerMorale"] = item.PlayerMorale, ["enemyMorale"] = item.EnemyMorale,
                    ["combatSourceEventId"] = item.CombatSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["content"] = D14ContentToObj(item.Content), ["playerParty"] = players, ["enemyParty"] = enemies,
                });
            }
            var casualties = new List<object>(); foreach (var item in runtime.Casualties) casualties.Add(new Dictionary<string, object> { ["casualtyId"] = item.CasualtyId ?? "", ["caseId"] = item.CaseId ?? "", ["participantId"] = item.ParticipantId ?? "", ["side"] = item.Side.ToString(), ["status"] = item.Status.ToString(), ["healthLoss"] = item.HealthLoss, ["burialRequested"] = item.BurialRequested, ["sourceEventId"] = item.SourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "" });
            var prisoners = new List<object>(); foreach (var item in runtime.Prisoners) prisoners.Add(new Dictionary<string, object> { ["prisonerId"] = item.PrisonerId ?? "", ["caseId"] = item.CaseId ?? "", ["participantId"] = item.ParticipantId ?? "", ["status"] = item.Status.ToString(), ["custodyOwner"] = item.CustodyOwner ?? "", ["rescued"] = item.Rescued, ["sourceEventId"] = item.SourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "" });
            var loot = new List<object>(); foreach (var item in runtime.Loot) { var items = new List<object>(); foreach (var good in item.Items) items.Add(new Dictionary<string, object> { ["itemId"] = good.ItemId ?? "", ["quantity"] = good.Quantity, ["legalStatus"] = good.LegalStatus ?? "", ["inventorySourceEventId"] = good.InventorySourceEventId ?? "" }); loot.Add(new Dictionary<string, object> { ["lootId"] = item.LootId ?? "", ["caseId"] = item.CaseId ?? "", ["status"] = item.Status.ToString(), ["legalSourceEventId"] = item.LegalSourceEventId ?? "", ["inventorySourceEventId"] = item.InventorySourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["items"] = items }); }
            var treatments = new List<object>(); foreach (var item in runtime.Treatments) treatments.Add(new Dictionary<string, object> { ["treatmentId"] = item.TreatmentId ?? "", ["caseId"] = item.CaseId ?? "", ["patientId"] = item.PatientId ?? "", ["status"] = item.Status.ToString(), ["healthBefore"] = item.HealthBefore, ["healthAfter"] = item.HealthAfter, ["medicalSourceEventId"] = item.MedicalSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "" });
            var rescues = new List<object>(); foreach (var item in runtime.Rescues) rescues.Add(new Dictionary<string, object> { ["rescueId"] = item.RescueId ?? "", ["caseId"] = item.CaseId ?? "", ["medicalSourceEventId"] = item.MedicalSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["rescuedPrisonerIds"] = StringListToObjects(item.RescuedPrisonerIds) });
            var interrogations = new List<object>(); foreach (var item in runtime.Interrogations) interrogations.Add(new Dictionary<string, object> { ["interrogationId"] = item.InterrogationId ?? "", ["caseId"] = item.CaseId ?? "", ["prisonerId"] = item.PrisonerId ?? "", ["skipped"] = item.Skipped, ["informationRef"] = item.InformationRef ?? "", ["content"] = D14ContentToObj(item.Content), ["sourceEventId"] = item.SourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "" });
            var funerals = new List<object>(); foreach (var item in runtime.Funerals) funerals.Add(new Dictionary<string, object> { ["funeralId"] = item.FuneralId ?? "", ["caseId"] = item.CaseId ?? "", ["status"] = item.Status.ToString(), ["skipped"] = item.Skipped, ["content"] = D14ContentToObj(item.Content), ["communitySourceEventId"] = item.CommunitySourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["casualtyIds"] = StringListToObjects(item.CasualtyIds) });
            var relations = new List<object>(); foreach (var item in runtime.Relations) relations.Add(new Dictionary<string, object> { ["relationId"] = item.RelationId ?? "", ["sourceId"] = item.SourceId ?? "", ["targetId"] = item.TargetId ?? "", ["metric"] = item.Metric.ToString(), ["proposedDelta"] = item.ProposedDelta, ["appliedDelta"] = item.AppliedDelta, ["causeEventId"] = item.CauseEventId ?? "", ["relationSourceEventId"] = item.RelationSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "" });
            var result = new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay, ["lastEventId"] = runtime.LastEventId ?? "",
                ["cases"] = cases, ["casualties"] = casualties, ["prisoners"] = prisoners, ["loot"] = loot, ["treatments"] = treatments, ["rescues"] = rescues, ["interrogations"] = interrogations, ["funerals"] = funerals, ["relations"] = relations, ["appliedEventKeys"] = runtime.AppliedEventKeys,
            };
            if (runtime.EventLog != null) result["eventLog"] = EventLogToObj(runtime.EventLog);
            return result;
        }

        private static Dictionary<string, object> D14ParticipantToObj(D14BoardingParticipant participant)
        {
            return new Dictionary<string, object> { ["participantId"] = participant.ParticipantId ?? "", ["side"] = participant.Side.ToString(), ["role"] = participant.Role.ToString(), ["weapon"] = participant.Weapon.ToString(), ["morale"] = participant.Morale, ["health"] = participant.Health, ["willingToSurrender"] = participant.WillingToSurrender };
        }

        private static Dictionary<string, object> D14ContentToObj(D14SensitiveContentTag content)
        {
            if (content == null) return null;
            return new Dictionary<string, object> { ["contentId"] = content.ContentId ?? "", ["historicalTag"] = content.HistoricalTag ?? "", ["sensitiveLabel"] = content.SensitiveLabel ?? "", ["contentWarning"] = content.ContentWarning ?? "", ["reviewStatus"] = content.ReviewStatus.ToString(), ["reviewAcknowledged"] = content.ReviewAcknowledged, ["skipSensitiveContent"] = content.SkipSensitiveContent, ["sourceNote"] = content.SourceNote ?? "", ["tags"] = StringListToObjects(content.Tags) };
        }

        private static D14AftercareRuntimeSnapshot D14AftercareFromObj(Dictionary<string, object> obj)
        {
            var result = new D14AftercareRuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14AftercareRuntimeSnapshot.CurrentSchemaVersion), Revision = Math.Max(0L, ReadLong(obj, "revision")), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")) };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "cases"))
            {
                var value = new D14BoardingCase { CaseId = SaveJson.GetStr(item, "caseId", ""), EncounterId = SaveJson.GetStr(item, "encounterId", ""), PlayerShipId = SaveJson.GetStr(item, "playerShipId", ""), EnemyShipId = SaveJson.GetStr(item, "enemyShipId", ""), Distance = SaveJson.GetFloat(item, "distance"), PlayerMorale = SaveJson.GetInt(item, "playerMorale"), EnemyMorale = SaveJson.GetInt(item, "enemyMorale"), CombatSourceEventId = SaveJson.GetStr(item, "combatSourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")), Content = D14ContentFromObj(Nested(item, "content")) };
                Enum.TryParse(SaveJson.GetStr(item, "status", D14BoardingCaseStatus.Open.ToString()), true, out D14BoardingCaseStatus status); value.Status = status;
                Enum.TryParse(SaveJson.GetStr(item, "outcome", D14AftercareOutcome.NonLethalDisengagement.ToString()), true, out D14AftercareOutcome outcome); value.Outcome = outcome;
                foreach (var participant in ObjectList(item, "playerParty")) value.PlayerParty.Add(D14ParticipantFromObj(participant, D14ParticipantSide.Player));
                foreach (var participant in ObjectList(item, "enemyParty")) value.EnemyParty.Add(D14ParticipantFromObj(participant, D14ParticipantSide.Enemy));
                result.Cases.Add(value);
            }
            foreach (var item in ObjectList(obj, "casualties")) { var value = new D14CasualtyRecord { CasualtyId = SaveJson.GetStr(item, "casualtyId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), ParticipantId = SaveJson.GetStr(item, "participantId", ""), HealthLoss = SaveJson.GetInt(item, "healthLoss"), BurialRequested = ReadBool(item, "burialRequested"), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; Enum.TryParse(SaveJson.GetStr(item, "side", D14ParticipantSide.Player.ToString()), true, out value.Side); Enum.TryParse(SaveJson.GetStr(item, "status", D14CasualtyStatus.Wounded.ToString()), true, out value.Status); result.Casualties.Add(value); }
            foreach (var item in ObjectList(obj, "prisoners")) { var value = new D14PrisonerRecord { PrisonerId = SaveJson.GetStr(item, "prisonerId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), ParticipantId = SaveJson.GetStr(item, "participantId", ""), CustodyOwner = SaveJson.GetStr(item, "custodyOwner", "player"), Rescued = ReadBool(item, "rescued"), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; Enum.TryParse(SaveJson.GetStr(item, "status", D14PrisonerStatus.Held.ToString()), true, out value.Status); result.Prisoners.Add(value); }
            foreach (var item in ObjectList(obj, "loot")) { var value = new D14LootRecord { LootId = SaveJson.GetStr(item, "lootId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), LegalSourceEventId = SaveJson.GetStr(item, "legalSourceEventId", ""), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; Enum.TryParse(SaveJson.GetStr(item, "status", D14LootStatus.Denied.ToString()), true, out value.Status); foreach (var good in ObjectList(item, "items")) { var entry = new D14LootItem { ItemId = SaveJson.GetStr(good, "itemId", ""), Quantity = Math.Max(0, SaveJson.GetInt(good, "quantity")), LegalStatus = SaveJson.GetStr(good, "legalStatus", "legal"), InventorySourceEventId = SaveJson.GetStr(good, "inventorySourceEventId", "") }; value.Items.Add(entry); } result.Loot.Add(value); }
            foreach (var item in ObjectList(obj, "treatments")) { var value = new D14TreatmentRecord { TreatmentId = SaveJson.GetStr(item, "treatmentId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), PatientId = SaveJson.GetStr(item, "patientId", ""), HealthBefore = SaveJson.GetInt(item, "healthBefore"), HealthAfter = SaveJson.GetInt(item, "healthAfter"), MedicalSourceEventId = SaveJson.GetStr(item, "medicalSourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; Enum.TryParse(SaveJson.GetStr(item, "status", D14TreatmentStatus.Rejected.ToString()), true, out value.Status); result.Treatments.Add(value); }
            foreach (var item in ObjectList(obj, "rescues")) { var value = new D14RescueRecord { RescueId = SaveJson.GetStr(item, "rescueId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), MedicalSourceEventId = SaveJson.GetStr(item, "medicalSourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; ReadStringListObject(value.RescuedPrisonerIds, item, "rescuedPrisonerIds"); result.Rescues.Add(value); }
            foreach (var item in ObjectList(obj, "interrogations")) { var value = new D14InterrogationRecord { InterrogationId = SaveJson.GetStr(item, "interrogationId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), PrisonerId = SaveJson.GetStr(item, "prisonerId", ""), Skipped = ReadBool(item, "skipped"), InformationRef = SaveJson.GetStr(item, "informationRef", ""), Content = D14ContentFromObj(Nested(item, "content")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; result.Interrogations.Add(value); }
            foreach (var item in ObjectList(obj, "funerals")) { var value = new D14FuneralRecord { FuneralId = SaveJson.GetStr(item, "funeralId", ""), CaseId = SaveJson.GetStr(item, "caseId", ""), Skipped = ReadBool(item, "skipped"), Content = D14ContentFromObj(Nested(item, "content")), CommunitySourceEventId = SaveJson.GetStr(item, "communitySourceEventId", ""), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; Enum.TryParse(SaveJson.GetStr(item, "status", D14FuneralStatus.Deferred.ToString()), true, out value.Status); ReadStringListObject(value.CasualtyIds, item, "casualtyIds"); result.Funerals.Add(value); }
            foreach (var item in ObjectList(obj, "relations")) { var value = new D14RelationConsequenceRecord { RelationId = SaveJson.GetStr(item, "relationId", ""), SourceId = SaveJson.GetStr(item, "sourceId", ""), TargetId = SaveJson.GetStr(item, "targetId", ""), ProposedDelta = SaveJson.GetFloat(item, "proposedDelta"), AppliedDelta = SaveJson.GetFloat(item, "appliedDelta"), CauseEventId = SaveJson.GetStr(item, "causeEventId", ""), RelationSourceEventId = SaveJson.GetStr(item, "relationSourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "metric", D14RelationMetric.Affinity.ToString()), true, out value.Metric); result.Relations.Add(value); }
            var eventLog = Nested(obj, "eventLog"); if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate(); return result;
        }

        private static D14BoardingParticipant D14ParticipantFromObj(Dictionary<string, object> obj, D14ParticipantSide fallbackSide)
        {
            var value = new D14BoardingParticipant { ParticipantId = SaveJson.GetStr(obj, "participantId", ""), Morale = SaveJson.GetInt(obj, "morale"), Health = SaveJson.GetInt(obj, "health", 100), WillingToSurrender = ReadBool(obj, "willingToSurrender"), Side = fallbackSide };
            Enum.TryParse(SaveJson.GetStr(obj, "side", fallbackSide.ToString()), true, out value.Side); Enum.TryParse(SaveJson.GetStr(obj, "role", D14ParticipantRole.Sailor.ToString()), true, out value.Role); Enum.TryParse(SaveJson.GetStr(obj, "weapon", D14BoardingWeapon.None.ToString()), true, out value.Weapon); return value;
        }

        private static D14SensitiveContentTag D14ContentFromObj(Dictionary<string, object> obj)
        {
            if (obj == null) return null;
            var value = new D14SensitiveContentTag { ContentId = SaveJson.GetStr(obj, "contentId", ""), HistoricalTag = SaveJson.GetStr(obj, "historicalTag", "inferred"), SensitiveLabel = EmptyToNull(SaveJson.GetStr(obj, "sensitiveLabel", "")), ContentWarning = EmptyToNull(SaveJson.GetStr(obj, "contentWarning", "")), ReviewAcknowledged = ReadBool(obj, "reviewAcknowledged"), SkipSensitiveContent = ReadBool(obj, "skipSensitiveContent"), SourceNote = SaveJson.GetStr(obj, "sourceNote", "save_restore") };
            Enum.TryParse(SaveJson.GetStr(obj, "reviewStatus", D14SensitiveReviewStatus.Pending.ToString()), true, out value.ReviewStatus); ReadStringListObject(value.Tags, obj, "tags"); return value;
        }

        private static Dictionary<string, object> D13GovernanceContentToObj(D13GovernanceContentTag content)
        {
            if (content == null) return null;
            content.Validate("governance_content");
            return new Dictionary<string, object>
            {
                ["contentId"] = content.ContentId ?? "",
                ["historicalTag"] = content.HistoricalTag ?? "",
                ["sensitiveContentLabel"] = content.SensitiveContentLabel ?? "",
                ["contentWarning"] = content.ContentWarning ?? "",
                ["reviewStatus"] = content.ReviewStatus.ToString(),
                ["sourceNote"] = content.SourceNote ?? "",
                ["tags"] = StringListToObjects(content.Tags),
            };
        }

        private static D13GovernanceContentTag D13GovernanceContentFromObj(Dictionary<string, object> obj)
        {
            var result = new D13GovernanceContentTag
            {
                ContentId = SaveJson.GetStr(obj, "contentId", ""),
                HistoricalTag = SaveJson.GetStr(obj, "historicalTag", "inferred"),
                SensitiveContentLabel = EmptyToNull(SaveJson.GetStr(obj, "sensitiveContentLabel", "")),
                ContentWarning = EmptyToNull(SaveJson.GetStr(obj, "contentWarning", "")),
                SourceNote = SaveJson.GetStr(obj, "sourceNote", "save_restore"),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "reviewStatus", D13GovernanceContentReviewStatus.PendingReview.ToString()), true, out D13GovernanceContentReviewStatus status);
            result.ReviewStatus = status;
            ReadStringListObject(result.Tags, obj, "tags");
            return result;
        }

        private static Dictionary<string, object> D13GovernanceToObj(D13GovernanceRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var households = new List<object>();
            foreach (var item in runtime.Households)
                households.Add(new Dictionary<string, object>
                {
                    ["householdId"] = item.HouseholdId ?? "", ["islandId"] = item.IslandId ?? "", ["familyName"] = item.FamilyName ?? "",
                    ["housingUnits"] = item.HousingUnits, ["memberIds"] = StringListToObjects(item.MemberIds), ["revision"] = item.Revision,
                    ["lastUpdatedDay"] = item.LastUpdatedDay, ["lastEventId"] = item.LastEventId ?? "",
                });
            var residents = new List<object>();
            foreach (var item in runtime.Residents)
                residents.Add(new Dictionary<string, object>
                {
                    ["residentId"] = item.ResidentId ?? "", ["householdId"] = item.HouseholdId ?? "", ["islandId"] = item.IslandId ?? "",
                    ["role"] = item.Role.ToString(), ["lifecycle"] = item.Lifecycle.ToString(), ["health"] = item.Health,
                    ["satisfaction"] = item.Satisfaction, ["educationLevel"] = item.EducationLevel, ["occupationId"] = item.OccupationId ?? "",
                    ["employmentStatus"] = item.EmploymentStatus.ToString(), ["wagePerDay"] = item.WagePerDay, ["wageDue"] = item.WageDue,
                    ["religionId"] = item.ReligionId ?? "", ["religionContentId"] = item.ReligionContentId ?? "", ["faithPracticeCount"] = item.FaithPracticeCount,
                    ["lastWorldDay"] = item.LastWorldDay, ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "",
                });
            var agreements = new List<object>();
            foreach (var item in runtime.Agreements)
                agreements.Add(new Dictionary<string, object>
                {
                    ["agreementId"] = item.AgreementId ?? "", ["communityId"] = item.CommunityId ?? "", ["status"] = item.Status ?? "",
                    ["content"] = D13GovernanceContentToObj(item.Content), ["sourceEventId"] = item.SourceEventId ?? "",
                    ["lastUpdatedDay"] = item.LastUpdatedDay, ["revision"] = item.Revision,
                });
            var provisions = new List<object>();
            foreach (var item in runtime.ProvisionRecords)
                provisions.Add(new Dictionary<string, object>
                {
                    ["recordId"] = item.RecordId ?? "", ["islandId"] = item.IslandId ?? "", ["residentCount"] = item.ResidentCount,
                    ["foodRequired"] = item.FoodRequired, ["foodGranted"] = item.FoodGranted, ["waterRequired"] = item.WaterRequired,
                    ["waterGranted"] = item.WaterGranted, ["foodSourceEventId"] = item.FoodSourceEventId ?? "", ["waterSourceEventId"] = item.WaterSourceEventId ?? "",
                    ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var taxes = new List<object>();
            foreach (var item in runtime.Taxes)
                taxes.Add(new Dictionary<string, object>
                {
                    ["assessmentId"] = item.AssessmentId ?? "", ["householdId"] = item.HouseholdId ?? "", ["taxBase"] = item.TaxBase,
                    ["taxRateBps"] = item.TaxRateBps, ["amountDue"] = item.AmountDue, ["amountCollected"] = item.AmountCollected,
                    ["sourceEventId"] = item.SourceEventId ?? "", ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var wages = new List<object>();
            foreach (var item in runtime.Wages)
                wages.Add(new Dictionary<string, object>
                {
                    ["paymentId"] = item.PaymentId ?? "", ["residentId"] = item.ResidentId ?? "", ["amount"] = item.Amount,
                    ["currency"] = item.Currency ?? D13GovernanceIds.Currency, ["sourceEventId"] = item.SourceEventId ?? "", ["ledgerRef"] = item.LedgerRef ?? "",
                    ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var incidents = new List<object>();
            foreach (var item in runtime.Incidents)
                incidents.Add(new Dictionary<string, object>
                {
                    ["incidentId"] = item.IncidentId ?? "", ["islandId"] = item.IslandId ?? "", ["severity"] = item.Severity,
                    ["resolved"] = item.Resolved, ["resolutionId"] = item.ResolutionId ?? "", ["worldDay"] = item.WorldDay, ["revision"] = item.Revision,
                });
            var agents = new List<object>();
            foreach (var item in runtime.AgentAssignments)
                agents.Add(new Dictionary<string, object>
                {
                    ["assignmentId"] = item.AssignmentId ?? "", ["agentRef"] = item.AgentRef ?? "", ["islandId"] = item.IslandId ?? "",
                    ["residentId"] = item.ResidentId ?? "", ["managementRole"] = item.ManagementRole ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay, ["revision"] = item.Revision,
                });
            var policy = runtime.Policy;
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["households"] = households, ["residents"] = residents,
                ["policy"] = new Dictionary<string, object>
                {
                    ["taxRateBps"] = policy.TaxRateBps, ["waterRationingLevel"] = policy.WaterRationingLevel, ["foodRationingLevel"] = policy.FoodRationingLevel,
                    ["educationOpen"] = policy.EducationOpen, ["religionArrangementId"] = policy.ReligionArrangementId ?? "", ["securityPostureId"] = policy.SecurityPostureId ?? "",
                    ["sourceEventId"] = policy.SourceEventId ?? "", ["revision"] = policy.Revision, ["lastUpdatedDay"] = policy.LastUpdatedDay,
                },
                ["agreements"] = agreements, ["provisions"] = provisions, ["taxes"] = taxes, ["wages"] = wages, ["incidents"] = incidents, ["agents"] = agents,
                ["securityStatus"] = runtime.SecurityStatus.ToString(), ["securityScore"] = runtime.SecurityScore, ["lastWorldDay"] = runtime.LastWorldDay,
                ["lastEventId"] = runtime.LastEventId ?? "", ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
            };
        }

        private static D13GovernanceRuntimeSnapshot D13GovernanceFromObj(Dictionary<string, object> obj)
        {
            var result = new D13GovernanceRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13GovernanceRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                SecurityScore = SaveJson.GetInt(obj, "securityScore", 70),
                LastWorldDay = Math.Max(0, SaveJson.GetInt(obj, "lastWorldDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            Enum.TryParse(SaveJson.GetStr(obj, "securityStatus", D13SecurityStatus.Stable.ToString()), true, out D13SecurityStatus securityStatus);
            result.SecurityStatus = securityStatus;
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "households"))
            {
                var household = new D13HouseholdState
                {
                    HouseholdId = SaveJson.GetStr(item, "householdId", ""), IslandId = SaveJson.GetStr(item, "islandId", ""), FamilyName = EmptyToNull(SaveJson.GetStr(item, "familyName", "")),
                    HousingUnits = Math.Max(1, SaveJson.GetInt(item, "housingUnits", 1)), Revision = Math.Max(0L, ReadLong(item, "revision")),
                    LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")),
                };
                ReadStringListObject(household.MemberIds, item, "memberIds"); result.Households.Add(household);
            }
            foreach (var item in ObjectList(obj, "residents"))
            {
                Enum.TryParse(SaveJson.GetStr(item, "role", D13ResidentRole.Adult.ToString()), true, out D13ResidentRole role);
                Enum.TryParse(SaveJson.GetStr(item, "lifecycle", D13ResidentLifecycle.Resident.ToString()), true, out D13ResidentLifecycle lifecycle);
                Enum.TryParse(SaveJson.GetStr(item, "employmentStatus", D13EmploymentStatus.Unemployed.ToString()), true, out D13EmploymentStatus employment);
                result.Residents.Add(new D13ResidentState
                {
                    ResidentId = SaveJson.GetStr(item, "residentId", ""), HouseholdId = SaveJson.GetStr(item, "householdId", ""), IslandId = SaveJson.GetStr(item, "islandId", ""),
                    Role = role, Lifecycle = lifecycle, Health = SaveJson.GetInt(item, "health", 100), Satisfaction = SaveJson.GetInt(item, "satisfaction", 70),
                    EducationLevel = Math.Max(0, SaveJson.GetInt(item, "educationLevel")), OccupationId = EmptyToNull(SaveJson.GetStr(item, "occupationId", "")), EmploymentStatus = employment,
                    WagePerDay = Math.Max(0, SaveJson.GetInt(item, "wagePerDay")), WageDue = Math.Max(0, SaveJson.GetInt(item, "wageDue")), ReligionId = EmptyToNull(SaveJson.GetStr(item, "religionId", "")),
                    ReligionContentId = EmptyToNull(SaveJson.GetStr(item, "religionContentId", "")), FaithPracticeCount = Math.Max(0, SaveJson.GetInt(item, "faithPracticeCount")),
                    LastWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastWorldDay")), Revision = Math.Max(0L, ReadLong(item, "revision")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")),
                });
            }
            if (obj.TryGetValue("policy", out object policyValue) && policyValue is Dictionary<string, object> policyObject)
            {
                result.Policy = new D13GovernancePolicyState
                {
                    TaxRateBps = SaveJson.GetInt(policyObject, "taxRateBps", 800), WaterRationingLevel = SaveJson.GetInt(policyObject, "waterRationingLevel"), FoodRationingLevel = SaveJson.GetInt(policyObject, "foodRationingLevel"),
                    EducationOpen = ReadBool(policyObject, "educationOpen", true), ReligionArrangementId = SaveJson.GetStr(policyObject, "religionArrangementId", "religion_catholic_parish"), SecurityPostureId = SaveJson.GetStr(policyObject, "securityPostureId", "watch_regular"),
                    SourceEventId = EmptyToNull(SaveJson.GetStr(policyObject, "sourceEventId", "")), Revision = Math.Max(0L, ReadLong(policyObject, "revision")), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(policyObject, "lastUpdatedDay")),
                };
            }
            foreach (var item in ObjectList(obj, "agreements"))
            {
                var agreement = new D13GovernanceAgreementState
                {
                    AgreementId = SaveJson.GetStr(item, "agreementId", ""), CommunityId = SaveJson.GetStr(item, "communityId", ""), Status = SaveJson.GetStr(item, "status", ""),
                    SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), Revision = Math.Max(0L, ReadLong(item, "revision")),
                };
                if (item.TryGetValue("content", out object contentValue) && contentValue is Dictionary<string, object> contentObject) agreement.Content = D13GovernanceContentFromObj(contentObject);
                result.Agreements.Add(agreement);
            }
            foreach (var item in ObjectList(obj, "provisions")) result.ProvisionRecords.Add(new D13GovernanceProvisionRecord { RecordId = SaveJson.GetStr(item, "recordId", ""), IslandId = SaveJson.GetStr(item, "islandId", ""), ResidentCount = Math.Max(0, SaveJson.GetInt(item, "residentCount")), FoodRequired = Math.Max(0, SaveJson.GetInt(item, "foodRequired")), FoodGranted = Math.Max(0, SaveJson.GetInt(item, "foodGranted")), WaterRequired = Math.Max(0, SaveJson.GetInt(item, "waterRequired")), WaterGranted = Math.Max(0, SaveJson.GetInt(item, "waterGranted")), FoodSourceEventId = SaveJson.GetStr(item, "foodSourceEventId", ""), WaterSourceEventId = SaveJson.GetStr(item, "waterSourceEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")) });
            foreach (var item in ObjectList(obj, "taxes")) result.Taxes.Add(new D13GovernanceTaxRecord { AssessmentId = SaveJson.GetStr(item, "assessmentId", ""), HouseholdId = SaveJson.GetStr(item, "householdId", ""), TaxBase = Math.Max(0, SaveJson.GetInt(item, "taxBase")), TaxRateBps = Math.Max(0, SaveJson.GetInt(item, "taxRateBps")), AmountDue = Math.Max(0, SaveJson.GetInt(item, "amountDue")), AmountCollected = Math.Max(0, SaveJson.GetInt(item, "amountCollected")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")) });
            foreach (var item in ObjectList(obj, "wages")) result.Wages.Add(new D13GovernanceWageRecord { PaymentId = SaveJson.GetStr(item, "paymentId", ""), ResidentId = SaveJson.GetStr(item, "residentId", ""), Amount = Math.Max(0, SaveJson.GetInt(item, "amount")), Currency = SaveJson.GetStr(item, "currency", D13GovernanceIds.Currency), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LedgerRef = SaveJson.GetStr(item, "ledgerRef", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")) });
            foreach (var item in ObjectList(obj, "incidents")) result.Incidents.Add(new D13GovernanceIncidentState { IncidentId = SaveJson.GetStr(item, "incidentId", ""), IslandId = SaveJson.GetStr(item, "islandId", ""), Severity = SaveJson.GetInt(item, "severity", 1), Resolved = ReadBool(item, "resolved", false), ResolutionId = EmptyToNull(SaveJson.GetStr(item, "resolutionId", "")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), Revision = Math.Max(0L, ReadLong(item, "revision")) });
            foreach (var item in ObjectList(obj, "agents")) result.AgentAssignments.Add(new D13GovernanceAgentAssignment { AssignmentId = SaveJson.GetStr(item, "assignmentId", ""), AgentRef = SaveJson.GetStr(item, "agentRef", ""), IslandId = SaveJson.GetStr(item, "islandId", ""), ResidentId = EmptyToNull(SaveJson.GetStr(item, "residentId", "")), ManagementRole = SaveJson.GetStr(item, "managementRole", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), Revision = Math.Max(0L, ReadLong(item, "revision")) });
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D13RanchToObj(D13RanchRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var animals = new List<object>();
            foreach (var item in runtime.Animals)
                animals.Add(new Dictionary<string, object>
                {
                    ["animalId"] = item.AnimalId ?? "", ["ranchId"] = item.RanchId ?? "", ["definitionId"] = item.DefinitionId ?? "",
                    ["sourceContentId"] = item.SourceContentId ?? "", ["sourceRef"] = item.SourceRef ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["inspectionEventId"] = item.InspectionEventId ?? "", ["kind"] = item.Kind.ToString(), ["lifecycle"] = item.Lifecycle.ToString(),
                    ["disease"] = item.Disease.ToString(), ["health"] = item.Health, ["quarantineUntilDay"] = item.QuarantineUntilDay,
                    ["cycleStartDay"] = item.CycleStartDay, ["lastUpdatedDay"] = item.LastUpdatedDay, ["feedUnits"] = item.FeedUnits,
                    ["waterUnits"] = item.WaterUnits, ["laborUnits"] = item.LaborUnits, ["cycleIndex"] = item.CycleIndex,
                    ["productReadyQuantity"] = item.ProductReadyQuantity, ["productItemId"] = item.ProductItemId ?? "", ["deathDay"] = item.DeathDay,
                    ["revision"] = item.Revision, ["lastEventId"] = item.LastEventId ?? "", ["lastDiseaseEventId"] = item.LastDiseaseEventId ?? "",
                });
            var sales = new List<object>();
            foreach (var item in runtime.Sales)
                sales.Add(new Dictionary<string, object>
                {
                    ["saleId"] = item.SaleId ?? "", ["itemId"] = item.ItemId ?? "", ["quantity"] = item.Quantity,
                    ["unitPrice"] = item.UnitPrice, ["totalPrice"] = item.TotalPrice, ["currency"] = item.Currency ?? D13RanchIds.Currency,
                    ["marketRef"] = item.MarketRef ?? "", ["ledgerRef"] = item.LedgerRef ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                });
            var waterSources = new List<object>();
            foreach (var item in runtime.Water.Sources)
                waterSources.Add(new Dictionary<string, object>
                {
                    ["ranchId"] = item.RanchId ?? "", ["sourceId"] = item.SourceId ?? "", ["available"] = item.Available,
                    ["capacity"] = item.Capacity, ["lastUpdatedDay"] = item.LastUpdatedDay, ["revision"] = item.Revision,
                    ["lastEventId"] = item.LastEventId ?? "",
                });
            var water = new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.Water.SchemaVersion, ["revision"] = runtime.Water.Revision,
                ["sources"] = waterSources, ["appliedEventKeys"] = StringListToObjects(runtime.Water.AppliedEventKeys),
            };
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision,
                ["animals"] = animals, ["sales"] = sales, ["water"] = water,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
            };
        }

        private static D13RanchRuntimeSnapshot D13RanchFromObj(Dictionary<string, object> obj)
        {
            var result = new D13RanchRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13RanchRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "animals")) result.Animals.Add(D13RanchAnimalFromObj(item));
            foreach (var item in ObjectList(obj, "sales")) result.Sales.Add(D13RanchSaleFromObj(item));
            if (obj.TryGetValue("water", out object waterValue) && waterValue is Dictionary<string, object> waterObject)
                result.Water = D13RanchWaterFromObj(waterObject);
            result.Validate();
            return result;
        }

        private static D13RanchAnimalState D13RanchAnimalFromObj(Dictionary<string, object> obj)
        {
            Enum.TryParse(SaveJson.GetStr(obj, "kind", D13RanchAnimalKind.Poultry.ToString()), true, out D13RanchAnimalKind kind);
            Enum.TryParse(SaveJson.GetStr(obj, "lifecycle", D13RanchAnimalLifecycle.Quarantined.ToString()), true, out D13RanchAnimalLifecycle lifecycle);
            Enum.TryParse(SaveJson.GetStr(obj, "disease", D13RanchDisease.None.ToString()), true, out D13RanchDisease disease);
            return new D13RanchAnimalState
            {
                AnimalId = SaveJson.GetStr(obj, "animalId", ""), RanchId = SaveJson.GetStr(obj, "ranchId", ""), DefinitionId = SaveJson.GetStr(obj, "definitionId", ""),
                SourceContentId = SaveJson.GetStr(obj, "sourceContentId", ""), SourceRef = SaveJson.GetStr(obj, "sourceRef", ""), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                InspectionEventId = SaveJson.GetStr(obj, "inspectionEventId", ""), Kind = kind, Lifecycle = lifecycle, Disease = disease,
                Health = SaveJson.GetInt(obj, "health", 100), QuarantineUntilDay = Math.Max(0, SaveJson.GetInt(obj, "quarantineUntilDay")),
                CycleStartDay = Math.Max(0, SaveJson.GetInt(obj, "cycleStartDay")), LastUpdatedDay = Math.Max(0, SaveJson.GetInt(obj, "lastUpdatedDay")),
                FeedUnits = Math.Max(0, SaveJson.GetInt(obj, "feedUnits")), WaterUnits = Math.Max(0, SaveJson.GetInt(obj, "waterUnits")),
                LaborUnits = Math.Max(0, SaveJson.GetInt(obj, "laborUnits")), CycleIndex = Math.Max(0, SaveJson.GetInt(obj, "cycleIndex")),
                ProductReadyQuantity = Math.Max(0, SaveJson.GetInt(obj, "productReadyQuantity")), ProductItemId = EmptyToNull(SaveJson.GetStr(obj, "productItemId", "")),
                DeathDay = SaveJson.GetInt(obj, "deathDay", -1), Revision = Math.Max(0L, ReadLong(obj, "revision")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")), LastDiseaseEventId = EmptyToNull(SaveJson.GetStr(obj, "lastDiseaseEventId", "")),
            };
        }

        private static D13RanchSaleRecord D13RanchSaleFromObj(Dictionary<string, object> obj)
        {
            return new D13RanchSaleRecord
            {
                SaleId = SaveJson.GetStr(obj, "saleId", ""), ItemId = SaveJson.GetStr(obj, "itemId", ""), Quantity = Math.Max(0, SaveJson.GetInt(obj, "quantity")),
                UnitPrice = Math.Max(0L, ReadLong(obj, "unitPrice")), TotalPrice = Math.Max(0L, ReadLong(obj, "totalPrice")), Currency = SaveJson.GetStr(obj, "currency", D13RanchIds.Currency),
                MarketRef = SaveJson.GetStr(obj, "marketRef", ""), LedgerRef = EmptyToNull(SaveJson.GetStr(obj, "ledgerRef", "")), SourceEventId = SaveJson.GetStr(obj, "sourceEventId", ""),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), SaveRevision = Math.Max(0L, ReadLong(obj, "saveRevision")),
            };
        }

        private static D13RanchWaterRuntimeSnapshot D13RanchWaterFromObj(Dictionary<string, object> obj)
        {
            var result = new D13RanchWaterRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D13RanchIds.SchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "sources"))
                result.Sources.Add(new D13RanchWaterState
                {
                    RanchId = SaveJson.GetStr(item, "ranchId", ""), SourceId = SaveJson.GetStr(item, "sourceId", ""),
                    Available = Math.Max(0, SaveJson.GetInt(item, "available", 100)), Capacity = Math.Max(1, SaveJson.GetInt(item, "capacity", 100)),
                    LastUpdatedDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedDay")), Revision = Math.Max(0L, ReadLong(item, "revision")),
                    LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")),
                });
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D14SpecialEquipmentToObj(D14SpecialEquipmentRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var equipment = new List<object>();
            foreach (var item in runtime.Equipment)
                equipment.Add(new Dictionary<string, object>
                {
                    ["instanceId"] = item.InstanceId ?? "", ["definitionId"] = item.DefinitionId ?? "", ["ownerRef"] = item.OwnerRef ?? "",
                    ["shipId"] = item.ShipId ?? "", ["slot"] = item.Slot.ToString(), ["status"] = item.Status.ToString(), ["legalStatus"] = item.LegalStatus.ToString(),
                    ["strength"] = item.Strength, ["durability"] = item.Durability, ["acquisitionSourceEventId"] = item.AcquisitionSourceEventId ?? "",
                    ["authorizationEventId"] = item.AuthorizationEventId ?? "", ["historicalReviewEventId"] = item.HistoricalReviewEventId ?? "",
                    ["inventorySourceEventId"] = item.InventorySourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "", ["recoveryRoute"] = item.RecoveryRoute ?? "",
                });
            var operations = new List<object>();
            foreach (var item in runtime.Operations)
                operations.Add(new Dictionary<string, object>
                {
                    ["operationId"] = item.OperationId ?? "", ["kind"] = item.Kind.ToString(), ["instanceId"] = item.InstanceId ?? "", ["definitionId"] = item.DefinitionId ?? "",
                    ["status"] = item.Status.ToString(), ["strengthBefore"] = item.StrengthBefore, ["strengthAfter"] = item.StrengthAfter,
                    ["durabilityBefore"] = item.DurabilityBefore, ["durabilityAfter"] = item.DurabilityAfter, ["saleValue"] = item.SaleValue,
                    ["ownerRef"] = item.OwnerRef ?? "", ["shipId"] = item.ShipId ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["authorizationEventId"] = item.AuthorizationEventId ?? "", ["inventorySourceEventId"] = item.InventorySourceEventId ?? "",
                    ["shipSourceEventId"] = item.ShipSourceEventId ?? "", ["economySourceEventId"] = item.EconomySourceEventId ?? "",
                    ["legalSourceEventId"] = item.LegalSourceEventId ?? "", ["lastEventId"] = item.LastEventId ?? "",
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay,
                ["lastEventId"] = runtime.LastEventId ?? "", ["equipment"] = equipment, ["operations"] = operations,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
                ["eventLog"] = runtime.EventLog == null ? null : EventLogToObj(runtime.EventLog),
            };
        }

        private static D14SpecialEquipmentRuntimeSnapshot D14SpecialEquipmentFromObj(Dictionary<string, object> obj)
        {
            var result = new D14SpecialEquipmentRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14SpecialEquipmentRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "equipment"))
            {
                var value = new D14EquipmentInstance
                {
                    InstanceId = SaveJson.GetStr(item, "instanceId", ""), DefinitionId = SaveJson.GetStr(item, "definitionId", ""), OwnerRef = SaveJson.GetStr(item, "ownerRef", ""),
                    ShipId = EmptyToNull(SaveJson.GetStr(item, "shipId", "")), Strength = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "strength"))), Durability = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "durability"))),
                    AcquisitionSourceEventId = SaveJson.GetStr(item, "acquisitionSourceEventId", ""), AuthorizationEventId = EmptyToNull(SaveJson.GetStr(item, "authorizationEventId", "")),
                    HistoricalReviewEventId = EmptyToNull(SaveJson.GetStr(item, "historicalReviewEventId", "")), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""),
                    LastEventId = SaveJson.GetStr(item, "lastEventId", ""), RecoveryRoute = EmptyToNull(SaveJson.GetStr(item, "recoveryRoute", "")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "slot", D14EquipmentSlot.None.ToString()), true, out value.Slot);
                Enum.TryParse(SaveJson.GetStr(item, "status", D14EquipmentStatus.Acquired.ToString()), true, out value.Status);
                Enum.TryParse(SaveJson.GetStr(item, "legalStatus", D14EquipmentLegalStatus.Legal.ToString()), true, out value.LegalStatus);
                result.Equipment.Add(value);
            }
            foreach (var item in ObjectList(obj, "operations"))
            {
                var value = new D14EquipmentOperationRecord
                {
                    OperationId = SaveJson.GetStr(item, "operationId", ""), InstanceId = SaveJson.GetStr(item, "instanceId", ""), DefinitionId = SaveJson.GetStr(item, "definitionId", ""),
                    StrengthBefore = SaveJson.GetInt(item, "strengthBefore"), StrengthAfter = SaveJson.GetInt(item, "strengthAfter"), DurabilityBefore = SaveJson.GetInt(item, "durabilityBefore"),
                    DurabilityAfter = SaveJson.GetInt(item, "durabilityAfter"), SaleValue = Math.Max(0, SaveJson.GetInt(item, "saleValue")), OwnerRef = SaveJson.GetStr(item, "ownerRef", ""),
                    ShipId = EmptyToNull(SaveJson.GetStr(item, "shipId", "")), SourceEventId = EmptyToNull(SaveJson.GetStr(item, "sourceEventId", "")),
                    AuthorizationEventId = EmptyToNull(SaveJson.GetStr(item, "authorizationEventId", "")), InventorySourceEventId = EmptyToNull(SaveJson.GetStr(item, "inventorySourceEventId", "")),
                    ShipSourceEventId = EmptyToNull(SaveJson.GetStr(item, "shipSourceEventId", "")), EconomySourceEventId = EmptyToNull(SaveJson.GetStr(item, "economySourceEventId", "")),
                    LegalSourceEventId = EmptyToNull(SaveJson.GetStr(item, "legalSourceEventId", "")), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                };
                Enum.TryParse(SaveJson.GetStr(item, "kind", D14EquipmentOperationKind.Acquisition.ToString()), true, out value.Kind);
                Enum.TryParse(SaveJson.GetStr(item, "status", D14EquipmentStatus.Acquired.ToString()), true, out value.Status);
                result.Operations.Add(value);
            }
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D14RewardToObj(D14RewardRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var requests = new List<object>();
            foreach (var item in runtime.Requests)
                requests.Add(new Dictionary<string, object>
                {
                    ["requestId"] = item.RequestId ?? "", ["rewardId"] = item.RewardId ?? "", ["recipientRef"] = item.RecipientRef ?? "",
                    ["requestedBySystem"] = item.RequestedBySystem ?? "", ["taskId"] = item.TaskId ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["sourceSaveRevision"] = item.SourceSaveRevision, ["worldDay"] = item.WorldDay, ["saveRevision"] = item.SaveRevision,
                    ["status"] = item.Status.ToString(), ["causeEventIds"] = StringListToObjects(item.CauseEventIds),
                });
            var grants = new List<object>();
            foreach (var item in runtime.Grants)
                grants.Add(new Dictionary<string, object>
                {
                    ["grantId"] = item.GrantId ?? "", ["requestId"] = item.RequestId ?? "", ["rewardId"] = item.RewardId ?? "",
                    ["contentId"] = item.ContentId ?? "", ["rarity"] = item.Rarity.ToString(), ["kind"] = item.Kind.ToString(),
                    ["unique"] = item.Unique, ["recipientRef"] = item.RecipientRef ?? "", ["status"] = item.Status.ToString(),
                    ["sourceEventId"] = item.SourceEventId ?? "", ["sourceSaveRevision"] = item.SourceSaveRevision,
                    ["grantEventId"] = item.GrantEventId ?? "", ["grantSaveRevision"] = item.GrantSaveRevision,
                    ["claimEventId"] = item.ClaimEventId ?? "", ["lossEventId"] = item.LossEventId ?? "", ["recoveryEventId"] = item.RecoveryEventId ?? "",
                    ["inventorySourceEventId"] = item.InventorySourceEventId ?? "", ["economySourceEventId"] = item.EconomySourceEventId ?? "",
                    ["collectionSourceEventId"] = item.CollectionSourceEventId ?? "", ["recoveryRoute"] = item.RecoveryRoute ?? "",
                    ["lossReason"] = item.LossReason.ToString(), ["lostWorldDay"] = item.LostWorldDay, ["lastEventId"] = item.LastEventId ?? "",
                });
            var audits = new List<object>();
            foreach (var item in runtime.Audits)
                audits.Add(new Dictionary<string, object>
                {
                    ["auditId"] = item.AuditId ?? "", ["grantId"] = item.GrantId ?? "", ["requestId"] = item.RequestId ?? "",
                    ["action"] = item.Action ?? "", ["eventId"] = item.EventId ?? "", ["saveRevision"] = item.SaveRevision,
                    ["worldDay"] = item.WorldDay, ["ownerSystem"] = item.OwnerSystem ?? "", ["sourceEventId"] = item.SourceEventId ?? "",
                    ["reason"] = item.Reason ?? "", ["recoveryRoute"] = item.RecoveryRoute ?? "",
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay,
                ["lastEventId"] = runtime.LastEventId ?? "", ["requests"] = requests, ["grants"] = grants, ["audits"] = audits,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys),
                ["eventLog"] = runtime.EventLog == null ? null : EventLogToObj(runtime.EventLog),
            };
        }

        private static D14RewardRuntimeSnapshot D14RewardFromObj(Dictionary<string, object> obj)
        {
            var result = new D14RewardRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14RewardRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "requests"))
            {
                var request = new D14RewardRequest
                {
                    RequestId = SaveJson.GetStr(item, "requestId", ""), RewardId = SaveJson.GetStr(item, "rewardId", ""),
                    RecipientRef = SaveJson.GetStr(item, "recipientRef", ""), RequestedBySystem = SaveJson.GetStr(item, "requestedBySystem", ""),
                    TaskId = EmptyToNull(SaveJson.GetStr(item, "taskId", "")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""),
                    SourceSaveRevision = Math.Max(0L, ReadLong(item, "sourceSaveRevision")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "status", D14RewardRequestStatus.Pending.ToString()), true, out request.Status);
                ReadStringListObject(request.CauseEventIds, item, "causeEventIds");
                result.Requests.Add(request);
            }
            foreach (var item in ObjectList(obj, "grants"))
            {
                var grant = new D14RewardGrantRecord
                {
                    GrantId = SaveJson.GetStr(item, "grantId", ""), RequestId = SaveJson.GetStr(item, "requestId", ""), RewardId = SaveJson.GetStr(item, "rewardId", ""),
                    ContentId = EmptyToNull(SaveJson.GetStr(item, "contentId", "")), RecipientRef = SaveJson.GetStr(item, "recipientRef", ""),
                    SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), SourceSaveRevision = Math.Max(0L, ReadLong(item, "sourceSaveRevision")),
                    GrantEventId = SaveJson.GetStr(item, "grantEventId", ""), GrantSaveRevision = Math.Max(0L, ReadLong(item, "grantSaveRevision")),
                    ClaimEventId = EmptyToNull(SaveJson.GetStr(item, "claimEventId", "")), LossEventId = EmptyToNull(SaveJson.GetStr(item, "lossEventId", "")),
                    RecoveryEventId = EmptyToNull(SaveJson.GetStr(item, "recoveryEventId", "")), InventorySourceEventId = EmptyToNull(SaveJson.GetStr(item, "inventorySourceEventId", "")),
                    EconomySourceEventId = EmptyToNull(SaveJson.GetStr(item, "economySourceEventId", "")), CollectionSourceEventId = EmptyToNull(SaveJson.GetStr(item, "collectionSourceEventId", "")),
                    RecoveryRoute = EmptyToNull(SaveJson.GetStr(item, "recoveryRoute", "")), LostWorldDay = SaveJson.GetInt(item, "lostWorldDay", -1), LastEventId = SaveJson.GetStr(item, "lastEventId", ""),
                    Unique = item.TryGetValue("unique", out object uniqueValue) && uniqueValue is bool && (bool)uniqueValue,
                };
                Enum.TryParse(SaveJson.GetStr(item, "rarity", D14RewardRarity.Ordinary.ToString()), true, out grant.Rarity);
                Enum.TryParse(SaveJson.GetStr(item, "kind", D14RewardKind.Item.ToString()), true, out grant.Kind);
                Enum.TryParse(SaveJson.GetStr(item, "status", D14RewardStatus.Pending.ToString()), true, out grant.Status);
                Enum.TryParse(SaveJson.GetStr(item, "lossReason", D14RewardLossReason.OwnerCorrection.ToString()), true, out grant.LossReason);
                result.Grants.Add(grant);
            }
            foreach (var item in ObjectList(obj, "audits"))
                result.Audits.Add(new D14RewardAuditRecord
                {
                    AuditId = SaveJson.GetStr(item, "auditId", ""), GrantId = EmptyToNull(SaveJson.GetStr(item, "grantId", "")),
                    RequestId = EmptyToNull(SaveJson.GetStr(item, "requestId", "")), Action = SaveJson.GetStr(item, "action", ""),
                    EventId = SaveJson.GetStr(item, "eventId", ""), SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")),
                    WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), OwnerSystem = SaveJson.GetStr(item, "ownerSystem", ""),
                    SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), Reason = SaveJson.GetStr(item, "reason", ""),
                    RecoveryRoute = EmptyToNull(SaveJson.GetStr(item, "recoveryRoute", "")),
                });
            var eventLog = Nested(obj, "eventLog");
            if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate();
            return result;
        }

        private static Dictionary<string, object> D14FactionLegalToObj(D14FactionLegalRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var relations = new List<object>();
            foreach (var v in runtime.Relations) relations.Add(new Dictionary<string, object>
            {
                ["relationKey"] = v.RelationKey ?? "", ["factionA"] = v.FactionA ?? "", ["factionB"] = v.FactionB ?? "", ["status"] = v.Status.ToString(),
                ["startedWorldDay"] = v.StartedWorldDay, ["lastUpdatedWorldDay"] = v.LastUpdatedWorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventId"] = v.EvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "",
            });
            var permits = new List<object>();
            foreach (var v in runtime.Permits) permits.Add(new Dictionary<string, object>
            {
                ["permitId"] = v.PermitId ?? "", ["issuerFactionId"] = v.IssuerFactionId ?? "", ["holderRef"] = v.HolderRef ?? "", ["targetFactionId"] = v.TargetFactionId ?? "", ["status"] = v.Status.ToString(),
                ["grantedWorldDay"] = v.GrantedWorldDay, ["expiresWorldDay"] = v.ExpiresWorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventId"] = v.EvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "",
            });
            var ports = new List<object>();
            foreach (var v in runtime.Ports) ports.Add(new Dictionary<string, object>
            {
                ["portId"] = v.PortId ?? "", ["controllingFactionId"] = v.ControllingFactionId ?? "", ["accessStatus"] = v.AccessStatus.ToString(), ["reason"] = v.Reason ?? "", ["lastUpdatedWorldDay"] = v.LastUpdatedWorldDay,
                ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventId"] = v.EvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "",
            });
            var wanted = new List<object>();
            foreach (var v in runtime.WantedCases) wanted.Add(new Dictionary<string, object>
            {
                ["caseId"] = v.CaseId ?? "", ["subjectRef"] = v.SubjectRef ?? "", ["factionId"] = v.FactionId ?? "", ["severity"] = v.Severity, ["status"] = v.Status.ToString(), ["issuedWorldDay"] = v.IssuedWorldDay, ["resolvedWorldDay"] = v.ResolvedWorldDay,
                ["reason"] = v.Reason ?? "", ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var inspections = new List<object>();
            foreach (var v in runtime.Inspections) inspections.Add(new Dictionary<string, object>
            {
                ["inspectionId"] = v.InspectionId ?? "", ["subjectRef"] = v.SubjectRef ?? "", ["portId"] = v.PortId ?? "", ["controllingFactionId"] = v.ControllingFactionId ?? "", ["allowed"] = v.Allowed, ["cargoLegal"] = v.CargoLegal,
                ["prohibitedCargoRefs"] = StringListToObjects(v.ProhibitedCargoRefs), ["wantedCaseId"] = v.WantedCaseId ?? "", ["seizureId"] = v.SeizureId ?? "", ["fineCaseId"] = v.FineCaseId ?? "", ["worldDay"] = v.WorldDay,
                ["sourceEventId"] = v.SourceEventId ?? "", ["cargoManifestEventId"] = v.CargoManifestEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var seizures = new List<object>();
            foreach (var v in runtime.Seizures) seizures.Add(new Dictionary<string, object>
            {
                ["seizureId"] = v.SeizureId ?? "", ["subjectRef"] = v.SubjectRef ?? "", ["factionId"] = v.FactionId ?? "", ["status"] = v.Status.ToString(), ["cargoRefs"] = StringListToObjects(v.CargoRefs), ["cargoManifestEventId"] = v.CargoManifestEventId ?? "", ["inventorySourceEventId"] = v.InventorySourceEventId ?? "",
                ["seizedWorldDay"] = v.SeizedWorldDay, ["releasedWorldDay"] = v.ReleasedWorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var financial = new List<object>();
            foreach (var v in runtime.FinancialCases) financial.Add(new Dictionary<string, object>
            {
                ["caseId"] = v.CaseId ?? "", ["kind"] = v.Kind.ToString(), ["subjectRef"] = v.SubjectRef ?? "", ["factionId"] = v.FactionId ?? "", ["prisonerRef"] = v.PrisonerRef ?? "", ["status"] = v.Status.ToString(), ["requestedAmount"] = v.RequestedAmount,
                ["issuedWorldDay"] = v.IssuedWorldDay, ["resolvedWorldDay"] = v.ResolvedWorldDay, ["reason"] = v.Reason ?? "", ["sourceEventId"] = v.SourceEventId ?? "", ["economySourceEventId"] = v.EconomySourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var escorts = new List<object>();
            foreach (var v in runtime.Escorts) escorts.Add(new Dictionary<string, object>
            {
                ["contractId"] = v.ContractId ?? "", ["escorterRef"] = v.EscorterRef ?? "", ["issuerFactionId"] = v.IssuerFactionId ?? "", ["routeId"] = v.RouteId ?? "", ["protectedCargoManifestEventId"] = v.ProtectedCargoManifestEventId ?? "", ["success"] = v.Success, ["reputationDelta"] = v.ReputationDelta,
                ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var blockades = new List<object>();
            foreach (var v in runtime.Blockades) blockades.Add(new Dictionary<string, object>
            {
                ["blockadeId"] = v.BlockadeId ?? "", ["issuerFactionId"] = v.IssuerFactionId ?? "", ["targetFactionId"] = v.TargetFactionId ?? "", ["targetPortId"] = v.TargetPortId ?? "", ["status"] = v.Status.ToString(), ["startedWorldDay"] = v.StartedWorldDay, ["lastUpdatedWorldDay"] = v.LastUpdatedWorldDay,
                ["reason"] = v.Reason ?? "", ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["lastEventId"] = v.LastEventId ?? "",
            });
            var intents = new List<object>();
            foreach (var v in runtime.DiplomaticIntents) intents.Add(new Dictionary<string, object>
            {
                ["proposalId"] = v.ProposalId ?? "", ["actorRef"] = v.ActorRef ?? "", ["requestedBySystem"] = v.RequestedBySystem ?? "", ["targetFactionId"] = v.TargetFactionId ?? "", ["intentKind"] = v.IntentKind.ToString(), ["summary"] = v.Summary ?? "", ["status"] = v.Status ?? "pending", ["worldDay"] = v.WorldDay, ["lastEventId"] = v.LastEventId ?? "",
            });
            var audits = new List<object>();
            foreach (var v in runtime.Audits) audits.Add(new Dictionary<string, object>
            {
                ["auditId"] = v.AuditId ?? "", ["action"] = v.Action ?? "", ["eventId"] = v.EventId ?? "", ["sourceEventId"] = v.SourceEventId ?? "", ["evidenceEventIds"] = StringListToObjects(v.EvidenceEventIds), ["subjectRef"] = v.SubjectRef ?? "", ["factionId"] = v.FactionId ?? "", ["worldDay"] = v.WorldDay, ["reason"] = v.Reason ?? "", ["recoveryRoute"] = v.RecoveryRoute ?? "",
            });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay, ["lastEventId"] = runtime.LastEventId ?? "",
                ["reputation"] = IntDictToObject(runtime.Reputation), ["wantedLevels"] = IntDictToObject(runtime.WantedLevels), ["relations"] = relations, ["permits"] = permits, ["ports"] = ports, ["wantedCases"] = wanted, ["inspections"] = inspections, ["seizures"] = seizures, ["financialCases"] = financial, ["escorts"] = escorts, ["blockades"] = blockades, ["diplomaticIntents"] = intents, ["audits"] = audits,
                ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys), ["eventLog"] = runtime.EventLog == null ? null : EventLogToObj(runtime.EventLog),
            };
        }

        private static D14FactionLegalRuntimeSnapshot D14FactionLegalFromObj(Dictionary<string, object> obj)
        {
            var result = new D14FactionLegalRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14FactionLegalRuntimeSnapshot.CurrentSchemaVersion), Revision = Math.Max(0L, ReadLong(obj, "revision")), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            ReadIntDictObject(result.Reputation, obj, "reputation"); ReadIntDictObject(result.WantedLevels, obj, "wantedLevels"); ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "relations")) { var v = new D14FactionRelationState { RelationKey = SaveJson.GetStr(item, "relationKey", ""), FactionA = SaveJson.GetStr(item, "factionA", ""), FactionB = SaveJson.GetStr(item, "factionB", ""), StartedWorldDay = Math.Max(0, SaveJson.GetInt(item, "startedWorldDay")), LastUpdatedWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedWorldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), EvidenceEventId = EmptyToNull(SaveJson.GetStr(item, "evidenceEventId", "")), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14FactionRelationStatus.Neutral.ToString()), true, out v.Status); result.Relations.Add(v); }
            foreach (var item in ObjectList(obj, "permits")) { var v = new D14PrivateerPermitState { PermitId = SaveJson.GetStr(item, "permitId", ""), IssuerFactionId = SaveJson.GetStr(item, "issuerFactionId", ""), HolderRef = SaveJson.GetStr(item, "holderRef", ""), TargetFactionId = EmptyToNull(SaveJson.GetStr(item, "targetFactionId", "")), GrantedWorldDay = Math.Max(0, SaveJson.GetInt(item, "grantedWorldDay")), ExpiresWorldDay = Math.Max(0, SaveJson.GetInt(item, "expiresWorldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), EvidenceEventId = SaveJson.GetStr(item, "evidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14PrivateerPermitStatus.Active.ToString()), true, out v.Status); result.Permits.Add(v); }
            foreach (var item in ObjectList(obj, "ports")) { var v = new D14PortLegalState { PortId = SaveJson.GetStr(item, "portId", ""), ControllingFactionId = SaveJson.GetStr(item, "controllingFactionId", ""), Reason = EmptyToNull(SaveJson.GetStr(item, "reason", "")), LastUpdatedWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedWorldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), EvidenceEventId = EmptyToNull(SaveJson.GetStr(item, "evidenceEventId", "")), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "accessStatus", D14PortAccessStatus.Open.ToString()), true, out v.AccessStatus); result.Ports.Add(v); }
            foreach (var item in ObjectList(obj, "wantedCases")) { var v = new D14WantedCaseState { CaseId = SaveJson.GetStr(item, "caseId", ""), SubjectRef = SaveJson.GetStr(item, "subjectRef", ""), FactionId = SaveJson.GetStr(item, "factionId", ""), Severity = SaveJson.GetInt(item, "severity", 1), IssuedWorldDay = Math.Max(0, SaveJson.GetInt(item, "issuedWorldDay")), ResolvedWorldDay = SaveJson.GetInt(item, "resolvedWorldDay", -1), Reason = SaveJson.GetStr(item, "reason", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14WantedStatus.Open.ToString()), true, out v.Status); ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.WantedCases.Add(v); }
            foreach (var item in ObjectList(obj, "inspections")) { var v = new D14InspectionRecord { InspectionId = SaveJson.GetStr(item, "inspectionId", ""), SubjectRef = SaveJson.GetStr(item, "subjectRef", ""), PortId = SaveJson.GetStr(item, "portId", ""), ControllingFactionId = SaveJson.GetStr(item, "controllingFactionId", ""), Allowed = ReadBool(item, "allowed"), CargoLegal = ReadBool(item, "cargoLegal"), WantedCaseId = EmptyToNull(SaveJson.GetStr(item, "wantedCaseId", "")), SeizureId = EmptyToNull(SaveJson.GetStr(item, "seizureId", "")), FineCaseId = EmptyToNull(SaveJson.GetStr(item, "fineCaseId", "")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), CargoManifestEventId = SaveJson.GetStr(item, "cargoManifestEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; ReadStringListObject(v.ProhibitedCargoRefs, item, "prohibitedCargoRefs"); ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.Inspections.Add(v); }
            foreach (var item in ObjectList(obj, "seizures")) { var v = new D14SeizureRecord { SeizureId = SaveJson.GetStr(item, "seizureId", ""), SubjectRef = SaveJson.GetStr(item, "subjectRef", ""), FactionId = SaveJson.GetStr(item, "factionId", ""), CargoManifestEventId = SaveJson.GetStr(item, "cargoManifestEventId", ""), InventorySourceEventId = SaveJson.GetStr(item, "inventorySourceEventId", ""), SeizedWorldDay = Math.Max(0, SaveJson.GetInt(item, "seizedWorldDay")), ReleasedWorldDay = SaveJson.GetInt(item, "releasedWorldDay", -1), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14LegalCaseStatus.Open.ToString()), true, out v.Status); ReadStringListObject(v.CargoRefs, item, "cargoRefs"); ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.Seizures.Add(v); }
            foreach (var item in ObjectList(obj, "financialCases")) { var v = new D14FinancialCaseState { CaseId = SaveJson.GetStr(item, "caseId", ""), SubjectRef = SaveJson.GetStr(item, "subjectRef", ""), FactionId = SaveJson.GetStr(item, "factionId", ""), PrisonerRef = EmptyToNull(SaveJson.GetStr(item, "prisonerRef", "")), RequestedAmount = Math.Max(0, SaveJson.GetInt(item, "requestedAmount")), IssuedWorldDay = Math.Max(0, SaveJson.GetInt(item, "issuedWorldDay")), ResolvedWorldDay = SaveJson.GetInt(item, "resolvedWorldDay", -1), Reason = SaveJson.GetStr(item, "reason", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), EconomySourceEventId = SaveJson.GetStr(item, "economySourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "kind", D14FinancialCaseKind.Fine.ToString()), true, out v.Kind); Enum.TryParse(SaveJson.GetStr(item, "status", D14LegalCaseStatus.Open.ToString()), true, out v.Status); ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.FinancialCases.Add(v); }
            foreach (var item in ObjectList(obj, "escorts")) { var v = new D14EscortRecord { ContractId = SaveJson.GetStr(item, "contractId", ""), EscorterRef = SaveJson.GetStr(item, "escorterRef", ""), IssuerFactionId = SaveJson.GetStr(item, "issuerFactionId", ""), RouteId = SaveJson.GetStr(item, "routeId", ""), ProtectedCargoManifestEventId = SaveJson.GetStr(item, "protectedCargoManifestEventId", ""), Success = ReadBool(item, "success"), ReputationDelta = SaveJson.GetInt(item, "reputationDelta"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.Escorts.Add(v); }
            foreach (var item in ObjectList(obj, "blockades")) { var v = new D14BlockadeState { BlockadeId = SaveJson.GetStr(item, "blockadeId", ""), IssuerFactionId = SaveJson.GetStr(item, "issuerFactionId", ""), TargetFactionId = EmptyToNull(SaveJson.GetStr(item, "targetFactionId", "")), TargetPortId = EmptyToNull(SaveJson.GetStr(item, "targetPortId", "")), StartedWorldDay = Math.Max(0, SaveJson.GetInt(item, "startedWorldDay")), LastUpdatedWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastUpdatedWorldDay")), Reason = SaveJson.GetStr(item, "reason", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14BlockadeStatus.Declared.ToString()), true, out v.Status); ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.Blockades.Add(v); }
            foreach (var item in ObjectList(obj, "diplomaticIntents")) { var v = new D14DiplomaticIntentRecord { ProposalId = SaveJson.GetStr(item, "proposalId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), RequestedBySystem = SaveJson.GetStr(item, "requestedBySystem", ""), TargetFactionId = SaveJson.GetStr(item, "targetFactionId", ""), Summary = SaveJson.GetStr(item, "summary", ""), Status = SaveJson.GetStr(item, "status", "pending"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "intentKind", D14DiplomaticIntentKind.RequestTruce.ToString()), true, out v.IntentKind); result.DiplomaticIntents.Add(v); }
            foreach (var item in ObjectList(obj, "audits")) { var v = new D14FactionLegalAuditRecord { AuditId = SaveJson.GetStr(item, "auditId", ""), Action = SaveJson.GetStr(item, "action", ""), EventId = SaveJson.GetStr(item, "eventId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), SubjectRef = EmptyToNull(SaveJson.GetStr(item, "subjectRef", "")), FactionId = EmptyToNull(SaveJson.GetStr(item, "factionId", "")), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), Reason = SaveJson.GetStr(item, "reason", ""), RecoveryRoute = EmptyToNull(SaveJson.GetStr(item, "recoveryRoute", "")) }; ReadStringListObject(v.EvidenceEventIds, item, "evidenceEventIds"); result.Audits.Add(v); }
            var eventLog = Nested(obj, "eventLog"); if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate(); return result;
        }

        private static Dictionary<string, object> IntDictToObject(Dictionary<string, int> source)
        {
            var result = new Dictionary<string, object>();
            if (source != null) foreach (var pair in source) result[pair.Key] = pair.Value;
            return result;
        }

        private static Dictionary<string, object> D14ReligionCommunityToObj(D14ReligionCommunityRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var states = new List<object>();
            foreach (var v in runtime.ReligionStates) states.Add(new Dictionary<string, object> { ["actorRef"] = v.ActorRef ?? "", ["religionId"] = v.ReligionId ?? "", ["festivalParticipationCount"] = v.FestivalParticipationCount, ["dietaryComplianceScore"] = v.DietaryComplianceScore, ["lastFestivalId"] = v.LastFestivalId ?? "", ["lastDietaryRuleId"] = v.LastDietaryRuleId ?? "", ["lastWorldDay"] = v.LastWorldDay, ["revision"] = v.Revision, ["lastEventId"] = v.LastEventId ?? "", ["appliedEventKeys"] = StringListToObjects(v.AppliedEventKeys) });
            var reviews = new List<object>();
            foreach (var v in runtime.Reviews) reviews.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["contentId"] = v.ContentId ?? "", ["reviewStatus"] = v.ReviewStatus.ToString(), ["performanceLevel"] = v.PerformanceLevel.ToString(), ["warningAcknowledged"] = v.WarningAcknowledged, ["skipped"] = v.Skipped, ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["worldDay"] = v.WorldDay, ["lastEventId"] = v.LastEventId ?? "" });
            var festivals = new List<object>();
            foreach (var v in runtime.Festivals) festivals.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["actorRef"] = v.ActorRef ?? "", ["festivalId"] = v.FestivalId ?? "", ["religionId"] = v.ReligionId ?? "", ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            var dietary = new List<object>();
            foreach (var v in runtime.DietaryRecords) dietary.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["actorRef"] = v.ActorRef ?? "", ["ruleId"] = v.RuleId ?? "", ["foodTag"] = v.FoodTag ?? "", ["religionId"] = v.ReligionId ?? "", ["compliant"] = v.Compliant, ["complianceAfter"] = v.ComplianceAfter, ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            var buildings = new List<object>();
            foreach (var v in runtime.BuildingAccess) buildings.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["actorRef"] = v.ActorRef ?? "", ["riteId"] = v.RiteId ?? "", ["buildingId"] = v.BuildingId ?? "", ["allowed"] = v.Allowed, ["buildingStatus"] = v.BuildingStatus ?? "", ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["buildingSourceEventId"] = v.BuildingSourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            var funerals = new List<object>();
            foreach (var v in runtime.FuneralRites) funerals.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["funeralId"] = v.FuneralId ?? "", ["riteId"] = v.RiteId ?? "", ["status"] = v.Status ?? "", ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["communitySourceEventId"] = v.CommunitySourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            var oaths = new List<object>();
            foreach (var v in runtime.Oaths) oaths.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["oathId"] = v.OathId ?? "", ["actorRef"] = v.ActorRef ?? "", ["targetRef"] = v.TargetRef ?? "", ["religionId"] = v.ReligionId ?? "", ["communityId"] = v.CommunityId ?? "", ["status"] = v.Status.ToString(), ["action"] = v.Action.ToString(), ["trustDomain"] = v.TrustDomain.ToString(), ["proposedTrustDelta"] = v.ProposedTrustDelta, ["appliedTrustDelta"] = v.AppliedTrustDelta, ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["trustSourceEventId"] = v.TrustSourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            var interactions = new List<object>();
            foreach (var v in runtime.CommunityInteractions) interactions.Add(new Dictionary<string, object> { ["actionId"] = v.ActionId ?? "", ["actorRef"] = v.ActorRef ?? "", ["communityId"] = v.CommunityId ?? "", ["etiquetteId"] = v.EtiquetteId ?? "", ["festivalId"] = v.FestivalId ?? "", ["dietaryRuleId"] = v.DietaryRuleId ?? "", ["allowed"] = v.Allowed, ["outcomeCode"] = v.OutcomeCode ?? "", ["proposedTrustDelta"] = v.ProposedTrustDelta, ["appliedTrustDelta"] = v.AppliedTrustDelta, ["trustAfter"] = v.TrustAfter, ["performanceLevel"] = v.PerformanceLevel.ToString(), ["skipped"] = v.Skipped, ["worldDay"] = v.WorldDay, ["sourceEventId"] = v.SourceEventId ?? "", ["communitySourceEventId"] = v.CommunitySourceEventId ?? "", ["historicalEvidenceEventId"] = v.HistoricalEvidenceEventId ?? "", ["lastEventId"] = v.LastEventId ?? "" });
            return new Dictionary<string, object> { ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay, ["lastEventId"] = runtime.LastEventId ?? "", ["religionStates"] = states, ["reviews"] = reviews, ["festivals"] = festivals, ["dietaryRecords"] = dietary, ["buildingAccess"] = buildings, ["funeralRites"] = funerals, ["oaths"] = oaths, ["communityInteractions"] = interactions, ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys), ["eventLog"] = runtime.EventLog == null ? null : EventLogToObj(runtime.EventLog) };
        }

        private static D14ReligionCommunityRuntimeSnapshot D14ReligionCommunityFromObj(Dictionary<string, object> obj)
        {
            var result = new D14ReligionCommunityRuntimeSnapshot { SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14ReligionCommunityRuntimeSnapshot.CurrentSchemaVersion), Revision = Math.Max(0L, ReadLong(obj, "revision")), WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")), LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")) };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "religionStates")) { var v = new D14ReligionPracticeState { ActorRef = SaveJson.GetStr(item, "actorRef", ""), ReligionId = SaveJson.GetStr(item, "religionId", ""), FestivalParticipationCount = Math.Max(0, SaveJson.GetInt(item, "festivalParticipationCount")), DietaryComplianceScore = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "dietaryComplianceScore"))), LastFestivalId = EmptyToNull(SaveJson.GetStr(item, "lastFestivalId", "")), LastDietaryRuleId = EmptyToNull(SaveJson.GetStr(item, "lastDietaryRuleId", "")), LastWorldDay = Math.Max(0, SaveJson.GetInt(item, "lastWorldDay")), Revision = Math.Max(0L, ReadLong(item, "revision")), LastEventId = EmptyToNull(SaveJson.GetStr(item, "lastEventId", "")) }; ReadStringListObject(v.AppliedEventKeys, item, "appliedEventKeys"); result.ReligionStates.Add(v); }
            foreach (var item in ObjectList(obj, "reviews")) { var v = new D14ContentReviewRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), ContentId = SaveJson.GetStr(item, "contentId", ""), WarningAcknowledged = ReadBool(item, "warningAcknowledged"), Skipped = ReadBool(item, "skipped"), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "reviewStatus", D14SensitiveReviewStatus.Pending.ToString()), true, out v.ReviewStatus); Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.Reviews.Add(v); }
            foreach (var item in ObjectList(obj, "festivals")) { var v = new D14FestivalRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), FestivalId = SaveJson.GetStr(item, "festivalId", ""), ReligionId = SaveJson.GetStr(item, "religionId", ""), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.Festivals.Add(v); }
            foreach (var item in ObjectList(obj, "dietaryRecords")) { var v = new D14DietaryRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), RuleId = SaveJson.GetStr(item, "ruleId", ""), FoodTag = SaveJson.GetStr(item, "foodTag", ""), ReligionId = SaveJson.GetStr(item, "religionId", ""), Compliant = ReadBool(item, "compliant"), ComplianceAfter = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "complianceAfter"))), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.DietaryRecords.Add(v); }
            foreach (var item in ObjectList(obj, "buildingAccess")) { var v = new D14BuildingAccessRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), RiteId = SaveJson.GetStr(item, "riteId", ""), BuildingId = SaveJson.GetStr(item, "buildingId", ""), Allowed = ReadBool(item, "allowed"), BuildingStatus = SaveJson.GetStr(item, "buildingStatus", ""), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), BuildingSourceEventId = EmptyToNull(SaveJson.GetStr(item, "buildingSourceEventId", "")), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.BuildingAccess.Add(v); }
            foreach (var item in ObjectList(obj, "funeralRites")) { var v = new D14FuneralRiteRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), FuneralId = SaveJson.GetStr(item, "funeralId", ""), RiteId = SaveJson.GetStr(item, "riteId", ""), Status = SaveJson.GetStr(item, "status", ""), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), CommunitySourceEventId = EmptyToNull(SaveJson.GetStr(item, "communitySourceEventId", "")), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.FuneralRites.Add(v); }
            foreach (var item in ObjectList(obj, "oaths")) { var v = new D14OathRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), OathId = SaveJson.GetStr(item, "oathId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), TargetRef = SaveJson.GetStr(item, "targetRef", ""), ReligionId = EmptyToNull(SaveJson.GetStr(item, "religionId", "")), CommunityId = EmptyToNull(SaveJson.GetStr(item, "communityId", "")), ProposedTrustDelta = SaveJson.GetInt(item, "proposedTrustDelta"), AppliedTrustDelta = SaveJson.GetInt(item, "appliedTrustDelta"), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), TrustSourceEventId = EmptyToNull(SaveJson.GetStr(item, "trustSourceEventId", "")), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "status", D14OathStatus.Released.ToString()), true, out v.Status); Enum.TryParse(SaveJson.GetStr(item, "action", D14OathAction.Swear.ToString()), true, out v.Action); Enum.TryParse(SaveJson.GetStr(item, "trustDomain", D14TrustDomain.CrewRelation.ToString()), true, out v.TrustDomain); Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.Oaths.Add(v); }
            foreach (var item in ObjectList(obj, "communityInteractions")) { var v = new D14CommunityInteractionRecord { ActionId = SaveJson.GetStr(item, "actionId", ""), ActorRef = SaveJson.GetStr(item, "actorRef", ""), CommunityId = SaveJson.GetStr(item, "communityId", ""), EtiquetteId = SaveJson.GetStr(item, "etiquetteId", ""), FestivalId = EmptyToNull(SaveJson.GetStr(item, "festivalId", "")), DietaryRuleId = EmptyToNull(SaveJson.GetStr(item, "dietaryRuleId", "")), Allowed = ReadBool(item, "allowed"), OutcomeCode = SaveJson.GetStr(item, "outcomeCode", ""), ProposedTrustDelta = SaveJson.GetInt(item, "proposedTrustDelta"), AppliedTrustDelta = SaveJson.GetInt(item, "appliedTrustDelta"), TrustAfter = Math.Max(0, Math.Min(100, SaveJson.GetInt(item, "trustAfter"))), Skipped = ReadBool(item, "skipped"), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""), CommunitySourceEventId = EmptyToNull(SaveJson.GetStr(item, "communitySourceEventId", "")), HistoricalEvidenceEventId = SaveJson.GetStr(item, "historicalEvidenceEventId", ""), LastEventId = SaveJson.GetStr(item, "lastEventId", "") }; Enum.TryParse(SaveJson.GetStr(item, "performanceLevel", D14PerformanceLevel.Contextual.ToString()), true, out v.PerformanceLevel); result.CommunityInteractions.Add(v); }
            var eventLog = Nested(obj, "eventLog"); if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate(); return result;
        }

        private static Dictionary<string, object> D14S01IntegrationToObj(D14S01IntegrationRuntimeSnapshot runtime)
        {
            runtime.Validate();
            var ledger = new List<object>();
            foreach (var v in runtime.Ledger)
                ledger.Add(new Dictionary<string, object>
                {
                    ["operationId"] = v.OperationId ?? "", ["stage"] = v.Stage.ToString(), ["sourceEventId"] = v.SourceEventId ?? "",
                    ["integrationEventId"] = v.IntegrationEventId ?? "", ["worldDay"] = v.WorldDay, ["saveRevision"] = v.SaveRevision,
                    ["authorityEventIds"] = StringListToObjects(v.AuthorityEventIds), ["rewardGrantId"] = v.RewardGrantId ?? "", ["legalActionKind"] = v.LegalActionKind ?? "",
                });
            return new Dictionary<string, object>
            {
                ["schemaVersion"] = runtime.SchemaVersion, ["revision"] = runtime.Revision, ["worldDay"] = runtime.WorldDay, ["lastEventId"] = runtime.LastEventId ?? "",
                ["ledger"] = ledger, ["appliedEventKeys"] = StringListToObjects(runtime.AppliedEventKeys), ["eventLog"] = runtime.EventLog == null ? null : EventLogToObj(runtime.EventLog),
            };
        }

        private static D14S01IntegrationRuntimeSnapshot D14S01IntegrationFromObj(Dictionary<string, object> obj)
        {
            var result = new D14S01IntegrationRuntimeSnapshot
            {
                SchemaVersion = SaveJson.GetInt(obj, "schemaVersion", D14S01IntegrationRuntimeSnapshot.CurrentSchemaVersion),
                Revision = Math.Max(0L, ReadLong(obj, "revision")),
                WorldDay = Math.Max(0, SaveJson.GetInt(obj, "worldDay")),
                LastEventId = EmptyToNull(SaveJson.GetStr(obj, "lastEventId", "")),
            };
            ReadStringListObject(result.AppliedEventKeys, obj, "appliedEventKeys");
            foreach (var item in ObjectList(obj, "ledger"))
            {
                var value = new D14S01IntegrationLedgerRecord
                {
                    OperationId = SaveJson.GetStr(item, "operationId", ""), SourceEventId = SaveJson.GetStr(item, "sourceEventId", ""),
                    IntegrationEventId = SaveJson.GetStr(item, "integrationEventId", ""), WorldDay = Math.Max(0, SaveJson.GetInt(item, "worldDay")),
                    SaveRevision = Math.Max(0L, ReadLong(item, "saveRevision")), RewardGrantId = EmptyToNull(SaveJson.GetStr(item, "rewardGrantId", "")),
                    LegalActionKind = EmptyToNull(SaveJson.GetStr(item, "legalActionKind", "")),
                };
                Enum.TryParse(SaveJson.GetStr(item, "stage", D14S01IntegrationStage.Combat.ToString()), true, out value.Stage);
                ReadStringListObject(value.AuthorityEventIds, item, "authorityEventIds");
                result.Ledger.Add(value);
            }
            var eventLog = Nested(obj, "eventLog"); if (eventLog != null) result.EventLog = EventLogFromObj(eventLog);
            result.Validate(); return result;
        }

        private static List<object> StringListToObjects(IEnumerable<string> values)
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

        private static long ReadLong(Dictionary<string, object> source, string key, long defaultValue = 0L)
        {
            if (source == null || !source.TryGetValue(key, out object value)) return defaultValue;
            if (value is long l) return l;
            if (value is double d) return (long)d;
            return defaultValue;
        }

        private static double ToDouble(object v)
        {
            if (v is double d) return d;
            if (v is long l) return l;
            return 0;
        }
    }
}
