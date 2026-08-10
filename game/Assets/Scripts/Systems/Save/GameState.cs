// ============================================================
// GameState —— 当前游戏状态（静态持有，供 P2 与后续阶段使用）
//
// 归属：Core（存档域）
// 注意：与 Whalebone.Core.GameState（枚举，GameManager 状态机）同名不同命名空间；
//       引用处请使用完整名 Whalebone.Systems.Save.GameState 或按 using 区分。
// 单一写者：Save 域（NewGame/Load/Save）是唯一修改入口；各系统读取后自行裁决数值，
//           但"写回存档"必须经由 SaveSystem.Save()。
// ============================================================

using System.Collections.Generic;
using Whalebone.Agent;
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
    /// <summary>
    /// 当前游戏内存状态（玩家 / 船 / 船员）。
    /// </summary>
    public static class GameState
    {
        public static PlayerState CurrentPlayer;
        public static ShipState CurrentShip;
        /// <summary>当前 D3 运行时快照；由 D3 表现层持有并通过 SaveSystem 落库。</summary>
        public static D3RuntimeSnapshot CurrentD3Runtime;
        /// <summary>当前 D4 内容扩充快照；由 D4 表现层持有并通过 SaveSystem 落库。</summary>
        public static D4RuntimeSnapshot CurrentD4Runtime;
        /// <summary>当前 D5 打磨上架快照；由 D5 表现层持有并通过 SaveSystem 落库。</summary>
        public static D5RuntimeSnapshot CurrentD5Runtime;
        /// <summary>D9 任务/流程快照；D3/D4 领域状态仍保存在各自快照。</summary>
        public static D9RuntimeSnapshot CurrentD9Runtime;
        /// <summary>D10 关系/恋爱阶段快照；由 RelationSystem 写入，Save 域负责持久化。</summary>
        public static D10RelationRuntimeSnapshot CurrentD10Runtime;
        /// <summary>D12 海图发现快照；探索系统裁决，Save 域只负责序列化。</summary>
        public static D12RuntimeSnapshot CurrentD12Runtime;
        /// <summary>D12 牌局快照；MiniGameSystem 裁决，Save 域只负责序列化。</summary>
        public static D12PokerRuntimeSnapshot CurrentD12PokerRuntime;
        /// <summary>D11 活鱼舱/岛上水产快照；领域系统写入，SaveSystem 只负责序列化。</summary>
        public static D11AquacultureRuntimeSnapshot CurrentD11AquacultureRuntime;
        /// <summary>D11 鲸豚路线/事实快照；路线系统写入，Save 域负责持久化。</summary>
        public static D11WhaleDolphinRuntimeSnapshot CurrentD11Runtime;
        /// <summary>D11 Pet 运行时快照；AnimalCareSystem 写入，Save 域负责持久化。</summary>
        public static D11PetRuntimeSnapshot CurrentD11PetRuntime;
        /// <summary>D13 产权运行时快照；PropertySystem 写入，Save 域负责持久化。</summary>
        public static D13PropertyRuntimeSnapshot CurrentD13Runtime;
        /// <summary>D13 金融运行时快照；FinanceSystem 写入，Save 域负责持久化。</summary>
        public static D13FinanceRuntimeSnapshot CurrentD13FinanceRuntime;
        public static D13FarmingRuntimeSnapshot CurrentD13FarmingRuntime;
        public static D13RanchRuntimeSnapshot CurrentD13RanchRuntime;
        /// <summary>D13-A05 岛屿治理运行时快照；治理系统写入，SaveSystem 负责持久化。</summary>
        public static D13GovernanceRuntimeSnapshot CurrentD13GovernanceRuntime;
        /// <summary>D13-A06 建筑运行时快照；BuildingSystem 写入，SaveSystem 负责持久化。</summary>
        public static D13BuildingRuntimeSnapshot CurrentD13BuildingRuntime;
        /// <summary>D13-A07 商品流/运输运行时快照；D13 系统写运输状态，SaveSystem 只负责持久化。</summary>
        public static D13DynamicMarketRuntimeSnapshot CurrentD13DynamicMarketRuntime;
        /// <summary>D14 海战运行时快照；CombatSystem 写战斗，Save 域只负责持久化。</summary>
        public static D14NavalCombatRuntimeSnapshot CurrentD14Runtime;
        /// <summary>D14 接舷/战后快照；AftercareSystem 写入，Save 域只负责持久化。</summary>
        public static D14AftercareRuntimeSnapshot CurrentD14AftercareRuntime;
        /// <summary>D14-A03 特殊装备快照；装备与所属 authority 写入，Save 域只负责持久化。</summary>
        public static D14SpecialEquipmentRuntimeSnapshot CurrentD14SpecialEquipmentRuntime;
        /// <summary>D14-A04 奖励快照；RewardSystem 写入，Save 域只负责持久化。</summary>
        public static D14RewardRuntimeSnapshot CurrentD14RewardRuntime;
        /// <summary>D14-A05 势力/法律运行时快照；FactionLegalSystem 写入，SaveSystem 负责持久化。</summary>
        public static D14FactionLegalRuntimeSnapshot CurrentD14FactionLegalRuntime;
        /// <summary>D14-A06 宗教/社群/历史审查运行时快照；ReligionCommunitySystem 写入。</summary>
        public static D14ReligionCommunityRuntimeSnapshot CurrentD14ReligionCommunityRuntime;
        /// <summary>D14-S01 跨域集成账本；IntegrationSystem 只写来源与因果投影。</summary>
        public static D14S01IntegrationRuntimeSnapshot CurrentD14S01IntegrationRuntime;

        /// <summary>当前船上的船员 Agent 列表（AgentCore.CrewAgent）。</summary>
        public static readonly List<CrewAgent> CrewAgents = new List<CrewAgent>();

        /// <summary>
        /// 船员特质引用表：crewId → TraitId[]。
        /// AgentCore 的 PersonaDefinition.Traits 目前是"显示名"列表（List&lt;string&gt;），
        /// 而 DATA_MODELS §3 要求 crew_agents.traits_json 存 TraitId 引用。
        /// 在 AgentCore 扩展完成前，由存档域在此维护 id 映射（P2 特质系统接手时迁移）。
        /// </summary>
        public static readonly Dictionary<string, List<string>> CrewAgentTraitIds =
            new Dictionary<string, List<string>>();

        /// <summary>清空内存状态（NewGame / Load 前调用；不触碰数据库）。</summary>
        public static void Reset()
        {
            CurrentPlayer = null;
            CurrentShip = null;
            CurrentD3Runtime = null;
            CurrentD4Runtime = null;
            CurrentD5Runtime = null;
            CurrentD9Runtime = null;
            CurrentD10Runtime = null;
            CurrentD12Runtime = null;
            CurrentD12PokerRuntime = null;
            CurrentD11Runtime = null;
            CurrentD11AquacultureRuntime = null;
            CurrentD11PetRuntime = null;
            CurrentD13Runtime = null;
            CurrentD13FinanceRuntime = null;
            CurrentD13FarmingRuntime = null;
            CurrentD13RanchRuntime = null;
            CurrentD13GovernanceRuntime = null;
            CurrentD13BuildingRuntime = null;
            CurrentD13DynamicMarketRuntime = null;
            CurrentD14Runtime = null;
            CurrentD14AftercareRuntime = null;
            CurrentD14SpecialEquipmentRuntime = null;
            CurrentD14RewardRuntime = null;
            CurrentD14FactionLegalRuntime = null;
            CurrentD14ReligionCommunityRuntime = null;
            CurrentD14S01IntegrationRuntime = null;
            CrewAgents.Clear();
            CrewAgentTraitIds.Clear();
        }
    }
}
