// ============================================================
// ShipState —— 船只存档状态（内存形态，对应 DATA_MODELS §2）
//
// 归属：5 船只系统（Save 域只负责序列化落库与还原，不做任何数值修改）
// 对应表：ships（DATA_MODELS §7）
// 说明：hullMax 不落库（数据单一事实源在 ship_type_defs.hull_max），
//       Load 时按 shipTypeId 回查填充。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.Save
{
    /// <summary>航线日志条目（自动记录 + AI 摘要，DATA_MODELS §2 shipLog）。</summary>
    [Serializable]
    public class ShipLogEntry
    {
        public string Time;   // ISO8601 文本
        public string Kind;   // "auto" | "ai"
        public string Text;
    }

    /// <summary>
    /// 船只存档状态。
    /// </summary>
    [Serializable]
    public class ShipState
    {
        public string Id;
        public string ShipTypeId;
        public string Name;
        public int Hull;
        public int HullMax;                 // 运行时由 ShipTypeDef 填充，不落库
        public Dictionary<string, int> Cargo = new Dictionary<string, int>();        // ItemId→count
        public int Cannons;
        public int LoadedCount;             // 已装填炮数
        public Dictionary<string, int> Supplies = new Dictionary<string, int>();    // water/food
        public int CrewCapacity;
        public List<string> CrewIds = new List<string>();                           // CrewAgentId[]
        public Dictionary<string, int> Upgrades = new Dictionary<string, int>();    // M2 启用，M0 全 0
        public Dictionary<string, float> State = new Dictionary<string, float>();   // tilt/flooding，M2 启用
        public List<ShipLogEntry> ShipLog = new List<ShipLogEntry>();

        /// <summary>初始补给（生存系统消耗；小渔船起始值）。</summary>
        public const int StartingWater = 20;
        public const int StartingFood = 20;

        /// <summary>创建全新船只：满耐久、空货舱、默认补给。</summary>
        public static ShipState CreateNew(string id, ShipTypeDef shipType, string ownerId)
        {
            var s = new ShipState
            {
                Id = id,
                ShipTypeId = shipType.Id,
                Name = shipType.Name,          // 默认船名=船型名（玩家可改）
                Hull = shipType.HullMax,       // 满耐久
                HullMax = shipType.HullMax,
                Cannons = 0,
                LoadedCount = 0,
                CrewCapacity = shipType.CrewMax,
            };
            s.Supplies["water"] = StartingWater;
            s.Supplies["food"] = StartingFood;
            foreach (var slot in UpgradeSlots)
                s.Upgrades[slot] = 0;          // M0 留默认 0
            s.State["tilt"] = 0f;
            s.State["flooding"] = 0f;
            return s;
        }

        /// <summary>改装槽位（M2 启用，M0 占位默认 0；与 DATA_MODELS §2 upgrades 对应）。</summary>
        public static readonly string[] UpgradeSlots =
            { "hull", "sail", "cannon", "medBay", "galley", "cabin" };
    }
}
