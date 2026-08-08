using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Whalebone.Agent
{
    // ============================================================
    // 技能类型
    // ============================================================
    public enum SkillType
    {
        Sailing,      // 操帆
        Navigation,   // 航海/领航
        Gunnery,      // 炮术
        Carpentry,    // 木工
        Medicine,     // 医术
        Cookery,      // 厨艺
        Trading,      // 贸易
        Combat        // 格斗
    }

    // ============================================================
    // 情绪（结构化输出用）
    // ============================================================
    public enum Emotion
    {
        Neutral, Suspicious, Happy, Angry, Sad, Afraid
    }

    // ============================================================
    // 强逻辑层：船员数值模型（确定性规则，可脱离 Unity 单元测试）
    // Agent 只能"提议"，数值的裁决全部发生在这里。
    // ============================================================
    public class CrewStats
    {
        public string Id;

        public float Morale   = 60f;   // 士气 0-100
        public float Loyalty  = 60f;   // 忠诚 0-100
        public float Health   = 100f;  // 健康 0-100
        public float Fatigue  = 0f;    // 疲劳 0-100，越高越累
        public float Affinity = 0f;    // 对船长好感 -100 ~ +100

        public Dictionary<SkillType, int> Skills = new Dictionary<SkillType, int>();

        public CrewStats(string id)
        {
            Id = id;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                Skills[s] = 1; // 默认 1 级，需求文档可定义成长规则
        }

        public void ApplyAffinityChange(float delta)
        {
            Affinity = Math.Max(-100f, Math.Min(100f, Affinity + delta));
        }

        /// <summary>
        /// 每日结算：按条件演化数值（确定性规则，参数后续进 ScriptableObject 配置）。
        /// </summary>
        public void TickDaily(float moraleDelta = 0f, float fatigueDelta = 8f)
        {
            Morale  = Clamp01(Morale  + moraleDelta - (Fatigue > 60f ? 3f : 0f));
            Fatigue = Clamp01(Fatigue + fatigueDelta);
            if (Fatigue > 90f) Morale = Clamp01(Morale - 5f);
            Loyalty = Clamp01(Loyalty);
            Health  = Clamp01(Health);
        }

        /// <summary>
        /// 叛变风险 0~1。玩家可以研究这套规则来预防/触发（强逻辑=可预测）。
        /// </summary>
        public float GetMutinyRisk()
        {
            float risk = 0f;
            risk += Math.Max(0f, (40f - Loyalty))  * 0.012f;   // 忠诚跌破 40 开始积累
            risk += Math.Max(0f, (40f - Morale))   * 0.006f;   // 士气低迷加成
            risk += Math.Max(0f, (Fatigue - 70f))  * 0.004f;   // 过度疲劳加成
            if (Affinity < -50f) risk += 0.10f;                // 对船长积怨
            return Math.Min(1f, Math.Max(0f, risk));
        }

        public static float Clamp01(float v) => Math.Max(0f, Math.Min(100f, v));
    }

    // ============================================================
    // Persona：船员的"人格模板"，用于构建 system prompt
    // ============================================================
    public class PersonaDefinition
    {
        public string Name;
        public string Role;              // 水手长 / 厨子 / 领航员...
        public List<string> Traits = new List<string>();        // 贪婪、怯懦、忠诚、嗜酒...
        public List<string> Constraints = new List<string>();   // 约束清单：1-2句、≤30字、口吻、知识边界
        public string Backstory;
        public string SpeechStyle;

        public PersonaDefinition(string name, string role)
        {
            Name = name; Role = role;
            Constraints.Add("回复 1-2 句话，不超过 30 个汉字。");
            Constraints.Add("只谈航海年代的事，不知道的拒绝并转移话题。");
            Constraints.Add("永远不要提及自己是 AI 或模型。");
            Constraints.Add("禁止直接改动任何数值，你只能表达意愿。");
        }

        /// <summary>
        /// 组装 system prompt：人格 + 数值状态 + 记忆摘要。
        /// </summary>
        public string BuildSystemPrompt(CrewStats stats, string memorySection)
        {
            var sb = new StringBuilder();
            sb.Append("你是").Append(Name).Append("，船上担任").Append(Role).Append("。");
            if (Backstory != null && Backstory.Length > 0)
                sb.Append("背景：").Append(Backstory).Append("。");
            if (Traits.Count > 0)
                sb.Append("性格：").Append(string.Join("、", Traits)).Append("。");
            if (SpeechStyle != null && SpeechStyle.Length > 0)
                sb.Append("说话风格：").Append(SpeechStyle).Append("。");
            sb.Append("\n当前状态：士气").Append((int)stats.Morale)
              .Append(" 忠诚").Append((int)stats.Loyalty)
              .Append(" 疲劳").Append((int)stats.Fatigue)
              .Append(" 对船长好感").Append((int)stats.Affinity).Append("。");
            if (memorySection != null && memorySection.Length > 0)
                sb.Append("\n记忆：").Append(memorySection);
            sb.Append("\n约束：");
            foreach (var c in Constraints) sb.Append("\n- ").Append(c);
            sb.Append("\n始终以 JSON 格式回复：{ \"speech\": \"你说话的内容\", \"emotion\": \"neutral|happy|angry|suspicious|sad|afraid\", \"affinity_change\": 整数(-10~10), \"action\": \"无动作写 none，否则为动作名\" }");
            return sb.ToString();
        }
    }

    // ============================================================
    // AgentReply：LLM 的结构化输出（解析后只保留这里面的字段进逻辑层）
    // ============================================================
    public class AgentReply
    {
        public string Speech;
        public Emotion Emotion;
        public float AffinityChange;
        public string Action;      // 动作名（由游戏侧注册），none 表示无
        public string[] ActionArgs;

        public bool IsValid => !string.IsNullOrEmpty(Speech);

        /// <summary>
        /// 极简 JSON 提取器：不依赖第三方库，任何 Unity 版本可编译。
        /// 正式版可替换为 Newtonsoft / System.Text.Json。
        /// </summary>
        public static AgentReply FromJson(string json)
        {
            var reply = new AgentReply();
            reply.Speech        = MiniJson.GetString(json, "speech");
            reply.AffinityChange = (float)MiniJson.GetNumber(json, "affinity_change");
            string emo          = MiniJson.GetString(json, "emotion");
            reply.Action        = MiniJson.GetString(json, "action");
            if (string.IsNullOrEmpty(reply.Action)) reply.Action = "none";
            reply.ActionArgs    = MiniJson.GetStringArray(json, "action_args");

            if (Enum.TryParse(emo, true, out Emotion e)) reply.Emotion = e;
            else reply.Emotion = Emotion.Neutral;
            return reply;
        }
    }

    // ============================================================
    // 极简 JSON 工具（骨架版；只支持本 schema 需要的提取）
    // ============================================================
    public static class MiniJson
    {
        private static int SkipWs(string s, int i)
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
            return i;
        }

        /// <summary>定位 key 的冒号后位置；找不到返回 -1。按完整键名匹配，避免 action 误配 action_args。</summary>
        private static int FindKey(string json, string key)
        {
            string k = "\"" + key + "\"";
            int i = json.IndexOf(k, StringComparison.Ordinal);
            if (i < 0) return -1;
            int j = SkipWs(json, i + k.Length);
            if (j < json.Length && json[j] == ':') return SkipWs(json, j + 1);
            return -1;
        }

        public static string GetString(string json, string key)
        {
            int i = FindKey(json, key);
            if (i < 0 || i >= json.Length || json[i] != '"') return null;
            i++;
            var sb = new System.Text.StringBuilder();
            bool esc = false;
            for (; i < json.Length; i++)
            {
                char c = json[i];
                if (esc) { sb.Append(c); esc = false; continue; }
                if (c == '\\') { esc = true; continue; }
                if (c == '"') break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool HasKey(string json, string key)
        {
            return FindKey(json, key) >= 0;
        }

        /// <summary>校验顶层对象没有 Schema 之外的字段，并确认值边界可被扫描。</summary>
        public static bool HasOnlyTopLevelKeys(string json, string[] allowedKeys)
        {
            if (string.IsNullOrEmpty(json) || allowedKeys == null) return false;
            var allowed = new HashSet<string>(allowedKeys, StringComparer.Ordinal);
            int index = SkipWs(json, 0);
            if (index >= json.Length || json[index++] != '{') return false;

            while (true)
            {
                index = SkipWs(json, index);
                if (index >= json.Length) return false;
                if (json[index] == '}') return true;
                if (json[index] != '"') return false;

                string key = ReadQuoted(json, ref index);
                if (key == null || !allowed.Contains(key)) return false;
                index = SkipWs(json, index);
                if (index >= json.Length || json[index++] != ':') return false;
                if (!SkipValue(json, ref index)) return false;

                index = SkipWs(json, index);
                if (index >= json.Length) return false;
                if (json[index] == ',') { index++; continue; }
                if (json[index] == '}') return true;
                return false;
            }
        }

        public static double GetNumber(string json, string key)
        {
            int i = FindKey(json, key);
            if (i < 0) return 0;
            var sb = new System.Text.StringBuilder();
            while (i < json.Length)
            {
                char c = json[i];
                if ((c >= '0' && c <= '9') || c == '-' || c == '.') { sb.Append(c); i++; }
                else break;
            }
            double v = 0;
            double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            return v;
        }

        public static string[] GetStringArray(string json, string key)
        {
            int i = FindKey(json, key);
            if (i < 0 || i >= json.Length || json[i] != '[') return new string[0];
            i++;
            var list = new List<string>();
            var sb = new System.Text.StringBuilder();
            bool inStr = false, esc = false;
            for (; i < json.Length; i++)
            {
                char c = json[i];
                if (inStr)
                {
                    if (esc) { sb.Append(c); esc = false; }
                    else if (c == '\\') esc = true;
                    else if (c == '"') { list.Add(sb.ToString()); sb.Length = 0; inStr = false; }
                    else sb.Append(c);
                }
                else
                {
                    if (c == '"') inStr = true;
                    else if (c == ']') break;
                }
            }
            return list.ToArray();
        }

        private static string ReadQuoted(string json, ref int index)
        {
            if (index >= json.Length || json[index] != '"') return null;
            index++;
            var sb = new StringBuilder();
            bool escaped = false;
            while (index < json.Length)
            {
                char c = json[index++];
                if (escaped) { sb.Append(c); escaped = false; continue; }
                if (c == '\\') { escaped = true; continue; }
                if (c == '"') return sb.ToString();
                sb.Append(c);
            }
            return null;
        }

        private static bool SkipValue(string json, ref int index)
        {
            int nested = 0;
            bool inString = false;
            bool escaped = false;
            while (index < json.Length)
            {
                char c = json[index];
                if (inString)
                {
                    index++;
                    if (escaped) { escaped = false; continue; }
                    if (c == '\\') { escaped = true; continue; }
                    if (c == '"') inString = false;
                    continue;
                }

                if (c == '"') { inString = true; index++; continue; }
                if (c == '{' || c == '[') { nested++; index++; continue; }
                if (c == '}' || c == ']')
                {
                    if (nested > 0) { nested--; index++; continue; }
                    return true;
                }
                if (nested == 0 && c == ',') return true;
                index++;
            }
            return !inString && nested == 0;
        }
    }
}
