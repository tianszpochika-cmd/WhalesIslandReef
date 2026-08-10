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
using Whalebone.Game.Config;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D5;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.D12;
using Whalebone.Systems.D11;
using Whalebone.Systems.D13;
using Whalebone.Systems.D14;

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
        public float NormalizedTime = (float)NumericConfiguration.Default.TimeOfDay.DefaultValue; // D2 一日内时间 0..1

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
        public static readonly int StartingMoney = (int)NumericConfiguration.Default.StartingMoney.DefaultValue;

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
        /// <summary>D5 打磨上架快照；只由 D5 系统裁决，Save 域负责序列化。</summary>
        public D5RuntimeSnapshot D5Runtime;
        /// <summary>D9 任务/流程快照；Save 域只序列化，不裁决任务结果。</summary>
        public D9RuntimeSnapshot D9Runtime;
        /// <summary>D10 关系/恋爱阶段快照；Save 域只序列化，不裁决关系结果。</summary>
        public D10RelationRuntimeSnapshot D10Runtime;
        /// <summary>D12 海图发现快照；Save 域只序列化，不裁决发现结果。</summary>
        public D12RuntimeSnapshot D12Runtime;
        /// <summary>D12 牌局事实/请求快照；MiniGameSystem 写入，Save 域只序列化。</summary>
        public D12PokerRuntimeSnapshot D12PokerRuntime;
        /// <summary>D11 活鱼舱/岛上水产快照；Save 域只序列化，不裁决养殖结果。</summary>
        public D11AquacultureRuntimeSnapshot D11AquacultureRuntime;
        /// <summary>D11 鲸豚路线投影；Save 域只序列化，不裁决生态、法律或奖励后果。</summary>
        public D11WhaleDolphinRuntimeSnapshot D11Runtime;
        /// <summary>D11 Pet 快照；AnimalCareSystem 写状态，Save 域只序列化。</summary>
        public D11PetRuntimeSnapshot D11PetRuntime;
        /// <summary>D13 产权快照；PropertySystem 写产权，Save 域只序列化。</summary>
        public D13PropertyRuntimeSnapshot D13Runtime;
        /// <summary>D13 金融快照；FinanceSystem 写账户/债务/保险，Save 域只序列化。</summary>
        public D13FinanceRuntimeSnapshot D13FinanceRuntime;
        public D13FarmingRuntimeSnapshot D13FarmingRuntime;
        public D13RanchRuntimeSnapshot D13RanchRuntime;
        /// <summary>D13-A05 岛屿治理快照；IslandGovernanceSystem 写状态，Save 域只序列化。</summary>
        public D13GovernanceRuntimeSnapshot D13GovernanceRuntime;
        /// <summary>D13-A06 建筑/仓库/船坞快照；BuildingSystem 写状态，Save 域只序列化。</summary>
        public D13BuildingRuntimeSnapshot D13BuildingRuntime;
        /// <summary>D13-A07 商品流/运输快照；市场价格、市场状态和货舱仍由所属系统写入，Save 只序列化。</summary>
        public D13DynamicMarketRuntimeSnapshot D13DynamicMarketRuntime;
        /// <summary>D14 海战快照；CombatSystem 写入，Save 域只序列化。</summary>
        public D14NavalCombatRuntimeSnapshot D14Runtime;
        /// <summary>D14-A02 接舷/俘虏/战后处理投影；AftercareSystem 写入，Save 域只序列化。</summary>
        public D14AftercareRuntimeSnapshot D14AftercareRuntime;
        /// <summary>D14-A03 特殊装备快照；装备/所属 authority 写入，Save 域只序列化。</summary>
        public D14SpecialEquipmentRuntimeSnapshot D14SpecialEquipmentRuntime;
        /// <summary>D14-A04 奖励快照；RewardSystem 写奖励状态，Save 域只序列化。</summary>
        public D14RewardRuntimeSnapshot D14RewardRuntime;
        /// <summary>D14-A05 势力/法律快照；FactionLegalSystem 写入，Save 域只序列化。</summary>
        public D14FactionLegalRuntimeSnapshot D14FactionLegalRuntime;
        /// <summary>D14-A06 宗教/社群/历史审查快照；ReligionCommunitySystem 写入，Save 域只序列化。</summary>
        public D14ReligionCommunityRuntimeSnapshot D14ReligionCommunityRuntime;
        /// <summary>D14-S01 跨域集成账本；各领域 owner 写最终状态，IntegrationSystem 只写因果投影。</summary>
        public D14S01IntegrationRuntimeSnapshot D14S01IntegrationRuntime;
        /// <summary>
        /// D10 玩家身份表达；Character/Identity 是唯一写者，Save 域只序列化。
        /// PlayerState.Name 保留为 D7/D8 兼容镜像，保存时以 Identity.Name 为准。
        /// </summary>
        public D10PlayerIdentityState Identity;

        /// <summary>创建全新玩家（技能默认 1、初始金钱 500、初始位置=圣玛格丽特港）。</summary>
        public static PlayerState CreateNew(string id, string name)
        {
            return CreateNew(id, name, null);
        }

        /// <summary>创建带有 D10 身份表达的玩家；旧调用保持默认身份兼容。</summary>
        public static PlayerState CreateNew(string id, string name, D10PlayerIdentityCreation identityCreation)
        {
            var p = new PlayerState
            {
                Id = id,
                Name = name,
                Money = StartingMoney,
                ShipId = null,
                Location = new LocationRef(StartingPortId),
                PlaySeconds = 0,
                Identity = D10PlayerIdentityState.CreateNew(id,
                    identityCreation == null ? D10PlayerIdentityCreation.Default(name) : identityCreation,
                    0,
                    null),
            };
            p.Name = p.Identity.Name;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                p.Skills[s] = 1; // 技能默认 1（需求文档约定）
            return p;
        }
    }
}
