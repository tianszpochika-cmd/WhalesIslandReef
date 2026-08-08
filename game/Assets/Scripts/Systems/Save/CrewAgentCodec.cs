// ============================================================
// CrewAgentCodec —— 船员 Agent <-> crew_agents 表行 的编解码
//
// 归属：2 船员系统（存档域负责序列化；数值裁决仍在船员系统内）
// 对应表：crew_agents（DATA_MODELS §7）
// M0 已知限制（记录在案，P2 解除）：
//   - AgentMemory 内部状态（短期上下文/摘要/事件）为私有，M0 不做还原，
//     Load 后以空记忆重建（memory_json 仅存骨架占位）；
//   - traits_json 存 TraitId[]；AgentCore 的 Persona.Traits 存对应中文名（展示/LLM 用），
//     两者由 GameState.CrewAgentTraitIds 桥接（见 GameState.cs 注释）。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// CrewAgent 存档编解码（纯逻辑，无 Unity 依赖）。
    /// </summary>
    internal static class CrewAgentCodec
    {
        /// <summary>角色 id（DATA_MODELS §6 RoleId）。</summary>
        public const string RoleDeckhand = "deckhand";

        /// <summary>文化 id（DATA_MODELS §6 CultureId）。</summary>
        public const string CultureEng = "eng";

        /// <summary>老水手（初始船员）固定名与岗位。</summary>
        public const string OldSailorName = "老水手";
        public const string OldSailorPersonaRole = "普通水手";

        /// <summary>把船员序列化为表行参数字典（与 crew_agents 列一一对应）。</summary>
        public static Dictionary<string, object> ToRow(CrewAgent agent, List<string> traitIds)
        {
            var persona = agent.Persona ?? new PersonaDefinition(agent.DisplayName, "");
            var stats = agent.Stats ?? new CrewStats(agent.Id);

            var statsObj = new Dictionary<string, object>
            {
                ["morale"] = stats.Morale,
                ["loyalty"] = stats.Loyalty,
                ["health"] = stats.Health,
                ["fatigue"] = stats.Fatigue,
                ["affinity"] = stats.Affinity,
            };
            var skillsObj = new Dictionary<string, object>();
            foreach (var kv in stats.Skills) skillsObj[kv.Key.ToString()] = kv.Value;
            statsObj["skills"] = skillsObj;

            var personaObj = new Dictionary<string, object>
            {
                ["name"] = persona.Name,
                ["role"] = persona.Role,
                ["traits"] = ToList(persona.Traits),
                ["constraints"] = ToList(persona.Constraints),
                ["backstory"] = persona.Backstory ?? "",
                ["speech_style"] = persona.SpeechStyle ?? "",
            };

            return new Dictionary<string, object>
            {
                ["id"] = agent.Id,
                ["name"] = persona.Name,
                ["role"] = RoleFor(),   // RoleId 存档列
                ["culture"] = CultureFor(),
                ["traits_json"] = SaveJson.Stringify(traitIds ?? new List<string>()),
                ["stats_json"] = SaveJson.Stringify(statsObj),
                ["persona_json"] = SaveJson.Stringify(personaObj),
                ["memory_json"] = SaveJson.Stringify(new Dictionary<string, object>
                {
                    ["summary"] = "",
                    ["events"] = new List<object>(),
                }),
                ["relations_json"] = SaveJson.Stringify(new Dictionary<string, object>()),
            };
        }

        /// <summary>从表行还原 CrewAgent 与特质 id 引用。</summary>
        public static CrewAgent FromRow(Dictionary<string, object> row)
        {
            var id = SaveJson.GetStr(row, "id", "");
            var personaJson = SaveJson.ParseDict(SaveJson.GetStr(row, "persona_json", "{}"));
            var statsJson = SaveJson.ParseDict(SaveJson.GetStr(row, "stats_json", "{}"));

            var persona = new PersonaDefinition(
                SaveJson.GetStr(personaJson, "name", "船员"),
                SaveJson.GetStr(personaJson, "role", ""));
            persona.Constraints.Clear();
            persona.Constraints.AddRange(ReadStringList(personaJson, "constraints"));
            persona.Traits.Clear();
            persona.Traits.AddRange(ReadStringList(personaJson, "traits"));
            persona.Backstory = SaveJson.GetStr(personaJson, "backstory", "");
            persona.SpeechStyle = SaveJson.GetStr(personaJson, "speech_style", "");

            var agent = new CrewAgent(id, persona, new InMemoryStore());
            RestoreStats(agent.Stats, statsJson);
            return agent;
        }

        /// <summary>从表行还原"特质 id 引用"（独立于 Agent 对象，由调用方存入 GameState）。</summary>
        public static List<string> ReadTraitIds(Dictionary<string, object> row)
        {
            var list = new List<string>();
            var parsed = SaveJson.ParseArray(SaveJson.GetStr(row, "traits_json", "[]"));
            if (parsed != null)
                foreach (var o in parsed) list.Add(Convert.ToString(o));
            return list;
        }

        private static void RestoreStats(CrewStats stats, Dictionary<string, object> json)
        {
            if (json == null || stats == null) return;
            stats.Morale = SaveJson.GetFloat(json, "morale", stats.Morale);
            stats.Loyalty = SaveJson.GetFloat(json, "loyalty", stats.Loyalty);
            stats.Health = SaveJson.GetFloat(json, "health", stats.Health);
            stats.Fatigue = SaveJson.GetFloat(json, "fatigue", stats.Fatigue);
            stats.Affinity = SaveJson.GetFloat(json, "affinity", stats.Affinity);
            if (json.TryGetValue("skills", out object s) && s is Dictionary<string, object> skills)
            {
                foreach (var kv in skills)
                {
                    if (Enum.TryParse(kv.Key, true, out SkillType st) && kv.Value is long lv)
                        stats.Skills[st] = (int)lv;
                }
            }
        }

        /// <summary>RoleId（DATA_MODELS §6）；M0 初始船员统一 deckhand，P2 由船员系统接管维护。</summary>
        private static string RoleFor()
        {
            return RoleDeckhand;
        }

        /// <summary>CultureId（DATA_MODELS §6）；M0 初始船员固定 eng，P2 由船员系统接管维护。</summary>
        private static string CultureFor()
        {
            return CultureEng;
        }

        private static List<object> ToList(List<string> source)
        {
            var list = new List<object>();
            if (source != null)
                foreach (var s in source) list.Add(s);
            return list;
        }

        private static List<string> ReadStringList(Dictionary<string, object> json, string key)
        {
            var result = new List<string>();
            if (json == null || !json.TryGetValue(key, out object v) || !(v is List<object> list)) return result;
            foreach (var o in list) result.Add(Convert.ToString(o));
            return result;
        }
    }
}
