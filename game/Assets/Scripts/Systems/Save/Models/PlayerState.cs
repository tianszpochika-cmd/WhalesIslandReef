// ============================================================
// PlayerState —— 玩家存档状态（内存形态，对应 DATA_MODELS §1）
//
// 归属：Core（存档所有权；经济/技能等数值的【裁决权】在各自系统类，
//       Save 域只负责序列化落库与还原，不做任何数值修改）
// 对应表：players（DATA_MODELS §7）
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;

namespace Whalebone.Systems.Save
{
    /// <summary>位置引用：港口 id 或海域坐标（DATA_MODELS §6 Location）。</summary>
    [Serializable]
    public class LocationRef
    {
        public string Type = "port";   // "port" | "open_sea"
        public string PortId;          // Type == port 时有效
        public float X;                // Type == open_sea 时有效
        public float Y;                // D2 世界 Z 坐标兼容映射
        public float Heading;          // D2 船只朝向（度）
        public float Speed;             // D2 船只速度
        public int DayIndex;            // D2 昼夜日期
        public float NormalizedTime = 0.25f; // D2 一日内时间 0..1

        public LocationRef() { }
        public LocationRef(string portId)
        {
            Type = "port";
            PortId = portId;
        }
    }

    /// <summary>
    /// 玩家存档状态。
    /// </summary>
    [Serializable]
    public class PlayerState
    {
        /// <summary>初始金钱（金币，经济系统唯一写者；此处仅为初始值）。</summary>
        public const int StartingMoney = 500;

        /// <summary>初始港口（DATA_MODELS §1 location；M0 固定为"圣玛格丽特港"）。</summary>
        public const string StartingPortId = "santa_margarita";

        public string Id;
        public string Name;
        public int Money;
        public Dictionary<SkillType, int> Skills = new Dictionary<SkillType, int>();
        public List<string> Traits = new List<string>();          // TraitId[]（引用 trait_defs）
        public Dictionary<string, int> Reputation = new Dictionary<string, int>(); // M1 启用，M0 空
        public Dictionary<string, int> Inventory = new Dictionary<string, int>();  // ItemId→count
        public string ShipId;
        public LocationRef Location = new LocationRef();
        public int PlaySeconds;
        /// <summary>D3 快照序列化载体；数值仍由 D3 系统裁决，Save 域只保存/还原。</summary>
        public D3RuntimeSnapshot D3Runtime;
        /// <summary>D4 内容扩充快照；只由 D4 系统裁决，Save 域负责序列化。</summary>
        public D4RuntimeSnapshot D4Runtime;

        /// <summary>创建全新玩家（技能默认 1、初始金钱 500、初始位置=圣玛格丽特港）。</summary>
        public static PlayerState CreateNew(string id, string name)
        {
            var p = new PlayerState
            {
                Id = id,
                Name = name,
                Money = StartingMoney,
                ShipId = null,
                Location = new LocationRef(StartingPortId),
                PlaySeconds = 0
            };
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                p.Skills[s] = 1; // 技能默认 1（需求文档约定）
            return p;
        }
    }
}
