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

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// 当前游戏内存状态（玩家 / 船 / 船员）。
    /// </summary>
    public static class GameState
    {
        public static PlayerState CurrentPlayer;
        public static ShipState CurrentShip;

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
            CrewAgents.Clear();
            CrewAgentTraitIds.Clear();
        }
    }
}
