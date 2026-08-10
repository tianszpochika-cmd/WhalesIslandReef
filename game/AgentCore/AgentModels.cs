using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Whalebone.Game.Config;

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

    /// <summary>D9 船员岗位的 canonical 类型；实际编制仍由 CrewSystem 写入。</summary>
    public enum CrewDuty
    {
        None,
        Helm,
        Navigator,
        Lookout,
        Gunner,
        Repair,
        Cook,
        Medic,
        Deckhand
    }

    // ============================================================
    // 情绪（结构化输出用）
    // ============================================================
    public enum Emotion
    {
        Neutral, Suspicious, Happy, Angry, Sad, Afraid, Calm, Concerned, Determined, Relieved, Disgusted, Hopeful
    }

    public enum DialogueIntent
    {
        Greet, Inform, Ask, Answer, Warn, Suggest, Negotiate, Refuse, Apologize, Report, Confirm, Farewell
    }

    public enum RelationKind
    {
        None, Trust, Respect, Affinity, Loyalty, Fear, Rivalry, Romance, Professional
    }

    public enum RelationStance
    {
        None, Build, Maintain, Repair, Challenge, Avoid, RequestMediation, OfferCooperation, Accept, Decline
    }

    public enum NegotiationTone
    {
        Neutral, Cooperative, Firm, Conciliatory, Guarded, Competitive, Formal, Urgent
    }

    public enum RefusalReason
    {
        None, UnknownRequest, InsufficientAuthority, UnsafeTarget, MissingPrerequisite,
        ConflictingOrder, PersonalBoundary, LegalRestriction, ResourceUnavailable,
        OutOfScope, NotInCharacter, AmbiguousIntent
    }

    // ============================================================
    // 强逻辑层：船员数值模型（确定性规则，可脱离 Unity 单元测试）
    // Agent 只能"提议"，数值的裁决全部发生在这里。
    // ============================================================
    public class CrewStats
    {
        public string Id;

        public float Morale   = (float)NumericConfiguration.Default.Morale.DefaultValue;   // 士气 0-100
        public float Loyalty  = (float)NumericConfiguration.Default.Loyalty.DefaultValue;  // 忠诚 0-100
        public float Health   = (float)NumericConfiguration.Default.Health.DefaultValue;   // 健康 0-100
        public float Fatigue  = (float)NumericConfiguration.Default.Fatigue.DefaultValue;  // 疲劳 0-100，越高越累
        public float Affinity = (float)NumericConfiguration.Default.Relation.DefaultValue; // 对船长好感 -100 ~ +100

        public Dictionary<SkillType, int> Skills = new Dictionary<SkillType, int>();

        public CrewStats(string id)
        {
            Id = id;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                Skills[s] = 1; // 默认 1 级，需求文档可定义成长规则
        }

        public void ApplyAffinityChange(float delta)
        {
            Affinity = NumericConfiguration.ClampRelation(Affinity + delta);
        }

        /// <summary>
        /// 每日结算：按条件演化数值（确定性规则，参数后续进 ScriptableObject 配置）。
        /// </summary>
        public void TickDaily(float moraleDelta = 0f, float fatigueDelta = 8f)
        {
            Morale  = NumericConfiguration.ClampMorale(Morale  + moraleDelta - (Fatigue > 60f ? 3f : 0f));
            Fatigue = NumericConfiguration.ClampFatigue(Fatigue + fatigueDelta);
            if (Fatigue > 90f) Morale = NumericConfiguration.ClampMorale(Morale - 5f);
            Loyalty = NumericConfiguration.ClampLoyalty(Loyalty);
            Health  = NumericConfiguration.ClampHealth(Health);
        }

        /// <summary>
        /// 叛变风险 0~1。玩家可以研究这套规则来预防/触发（强逻辑=可预测）。
        /// </summary>
        public float GetMutinyRisk()
        {
            float risk = 0f;
            risk += Math.Max(0f, (D10CrewBehaviorConfiguration.MutinyLoyaltyRiskStart - Loyalty))
                * D10CrewBehaviorConfiguration.MutinyLoyaltyRiskPerPoint;
            risk += Math.Max(0f, (D10CrewBehaviorConfiguration.MutinyMoraleRiskStart - Morale))
                * D10CrewBehaviorConfiguration.MutinyMoraleRiskPerPoint;
            risk += Math.Max(0f, (Fatigue - D10CrewBehaviorConfiguration.MutinyFatigueRiskStart))
                * D10CrewBehaviorConfiguration.MutinyFatigueRiskPerPoint;
            if (Affinity < (float)NumericConfiguration.Default.MutinyAffinityThreshold.DefaultValue)
                risk += D10CrewBehaviorConfiguration.MutinyAffinityRiskIncrement;
            return Math.Min(1f, Math.Max(0f, risk));
        }

        public static float Clamp01(float v) => NumericConfiguration.ClampHealth(v);
    }

    // ============================================================
    // Persona：船员的"人格模板"，用于构建 system prompt
    // ============================================================
    public class PersonaDefinition
    {
        public string Name;
        public string Role;              // 水手长 / 厨子 / 领航员...
        public CrewDuty PrimaryDuty = CrewDuty.Deckhand;
        public List<CrewDuty> QualifiedDuties = new List<CrewDuty> { CrewDuty.Deckhand };
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
            sb.Append("岗位：").Append(PrimaryDuty);
            if (QualifiedDuties.Count > 0)
                sb.Append("；可胜任：").Append(string.Join("、", QualifiedDuties));
            sb.Append("\n当前状态：士气").Append((int)stats.Morale)
              .Append(" 忠诚").Append((int)stats.Loyalty)
              .Append(" 疲劳").Append((int)stats.Fatigue)
              .Append(" 对船长好感").Append((int)stats.Affinity).Append("。");
            if (memorySection != null && memorySection.Length > 0)
                sb.Append("\n记忆：").Append(memorySection);
            sb.Append("\n约束：");
            foreach (var c in Constraints) sb.Append("\n- ").Append(c);
            sb.Append("\n始终以 JSON 格式回复：{ \"schema_version\": \"agent_reply.v1.1\", \"speech\": \"对白\", \"dialogue_intent\": \"greet|inform|ask|answer|warn|suggest|negotiate|refuse|apologize|report|confirm|farewell\", \"emotion\": \"neutral|suspicious|happy|angry|sad|afraid|calm|concerned|determined|relieved|disgusted|hopeful\", \"affinity_change\": 整数(")
              .Append((int)NumericConfiguration.Default.RelationProposalDelta.Minimum).Append("~")
              .Append((int)NumericConfiguration.Default.RelationProposalDelta.Maximum)
              .Append("), \"relation_proposal\": { \"kind\": \"none|trust|respect|affinity|loyalty|fear|rivalry|romance|professional\", \"target_ref\": \"none 或授权对象\", \"stance\": \"none|build|maintain|repair|challenge|avoid|request_mediation|offer_cooperation|accept|decline\", \"basis\": \"理由\" }, \"negotiation_tone\": \"neutral|cooperative|firm|conciliatory|guarded|competitive|formal|urgent\", \"refusal_reason\": \"none 或拒绝原因\", \"action_intent\": { \"name\": \"none 或注册动作名\", \"actor_ref\": \"none 或授权执行者\", \"target_ref\": \"none 或授权目标\", \"parameters\": {} } }");
            sb.Append("\n严禁输出最终 money、price、damage、reward、weather 或 relation 数值结果；动作只是提议，结果由所属强逻辑系统裁决。");
            return sb.ToString();
        }
    }

    // ============================================================
    // AgentReply：LLM 的结构化提议（不携带任何最终经济、战斗、天气或关系结果）
    // ============================================================
    public class AgentReply
    {
        public const string CurrentSchemaVersion = "agent_reply.v1.1";

        public string SchemaVersion = CurrentSchemaVersion;
        public string Speech;
        public DialogueIntent DialogueIntent = DialogueIntent.Inform;
        public Emotion Emotion = Emotion.Neutral;
        public float AffinityChange;
        public RelationProposal RelationProposal = RelationProposal.Empty();
        public NegotiationTone NegotiationTone = NegotiationTone.Neutral;
        public RefusalReason RefusalReason = RefusalReason.None;
        public ActionIntent ActionIntent = ActionIntent.Empty();

        // D2 source compatibility projections. They are never serialized by the v1.1 contract.
        public string Action;
        public string[] ActionArgs = new string[0];

        public bool IsValid => !string.IsNullOrEmpty(Speech);

        /// <summary>
        /// 极简 JSON 提取器：不依赖第三方库，任何 Unity 版本可编译。
        /// 正式版可替换为 Newtonsoft / System.Text.Json。
        /// </summary>
        public static AgentReply FromJson(string json)
        {
            Dictionary<string, object> values;
            string error;
            return MiniJson.TryParseObject(json, out values, out error)
                ? FromMap(values)
                : new AgentReply { Speech = null, Action = "none", ActionIntent = ActionIntent.Empty() };
        }

        internal static AgentReply FromMap(Dictionary<string, object> values)
        {
            var reply = new AgentReply();
            reply.SchemaVersion = StringValue(values, "schema_version") ?? CurrentSchemaVersion;
            reply.Speech = StringValue(values, "speech");
            reply.DialogueIntent = ParseDialogueIntent(StringValue(values, "dialogue_intent"));
            reply.Emotion = ParseEmotion(StringValue(values, "emotion"));
            double affinity;
            if (NumberValue(values, "affinity_change", out affinity)) reply.AffinityChange = (float)affinity;

            Dictionary<string, object> relation = ObjectValue(values, "relation_proposal");
            if (relation != null)
            {
                reply.RelationProposal = new RelationProposal
                {
                    Kind = ParseRelationKind(StringValue(relation, "kind")),
                    TargetRef = StringValue(relation, "target_ref") ?? "none",
                    Stance = ParseRelationStance(StringValue(relation, "stance")),
                    Basis = StringValue(relation, "basis") ?? "无关系提议"
                };
            }

            reply.NegotiationTone = ParseNegotiationTone(StringValue(values, "negotiation_tone"));
            reply.RefusalReason = ParseRefusalReason(StringValue(values, "refusal_reason"));
            Dictionary<string, object> action = ObjectValue(values, "action_intent");
            if (action != null)
            {
                reply.ActionIntent = new ActionIntent
                {
                    Name = StringValue(action, "name") ?? "none",
                    ActorRef = StringValue(action, "actor_ref") ?? "none",
                    TargetRef = StringValue(action, "target_ref") ?? "none"
                };
                Dictionary<string, object> parameters = ObjectValue(action, "parameters");
                if (parameters != null)
                    foreach (var pair in parameters) reply.ActionIntent.Parameters[pair.Key] = pair.Value;
            }
            reply.Action = reply.ActionIntent.Name;
            reply.ActionArgs = new string[0];
            return reply;
        }

        private static string StringValue(Dictionary<string, object> values, string key)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value as string : null;
        }

        private static Dictionary<string, object> ObjectValue(Dictionary<string, object> values, string key)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value as Dictionary<string, object> : null;
        }

        private static bool NumberValue(Dictionary<string, object> values, string key, out double number)
        {
            object value;
            if (values != null && values.TryGetValue(key, out value) && value is double)
            {
                number = (double)value;
                return true;
            }
            number = 0;
            return false;
        }

        private static Emotion ParseEmotion(string value)
        {
            Emotion parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : Emotion.Neutral;
        }

        private static DialogueIntent ParseDialogueIntent(string value)
        {
            switch (value)
            {
                case "greet": return DialogueIntent.Greet; case "ask": return DialogueIntent.Ask;
                case "answer": return DialogueIntent.Answer; case "warn": return DialogueIntent.Warn;
                case "suggest": return DialogueIntent.Suggest; case "negotiate": return DialogueIntent.Negotiate;
                case "refuse": return DialogueIntent.Refuse; case "apologize": return DialogueIntent.Apologize;
                case "report": return DialogueIntent.Report; case "confirm": return DialogueIntent.Confirm;
                case "farewell": return DialogueIntent.Farewell; default: return DialogueIntent.Inform;
            }
        }

        private static RelationKind ParseRelationKind(string value)
        {
            RelationKind parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : RelationKind.None;
        }

        private static RelationStance ParseRelationStance(string value)
        {
            if (value == "request_mediation") return RelationStance.RequestMediation;
            if (value == "offer_cooperation") return RelationStance.OfferCooperation;
            RelationStance parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : RelationStance.None;
        }

        private static NegotiationTone ParseNegotiationTone(string value)
        {
            NegotiationTone parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : NegotiationTone.Neutral;
        }

        private static RefusalReason ParseRefusalReason(string value)
        {
            if (value == "unknown_request") return RefusalReason.UnknownRequest;
            if (value == "insufficient_authority") return RefusalReason.InsufficientAuthority;
            if (value == "unsafe_target") return RefusalReason.UnsafeTarget;
            if (value == "missing_prerequisite") return RefusalReason.MissingPrerequisite;
            if (value == "conflicting_order") return RefusalReason.ConflictingOrder;
            if (value == "personal_boundary") return RefusalReason.PersonalBoundary;
            if (value == "legal_restriction") return RefusalReason.LegalRestriction;
            if (value == "resource_unavailable") return RefusalReason.ResourceUnavailable;
            if (value == "out_of_scope") return RefusalReason.OutOfScope;
            if (value == "not_in_character") return RefusalReason.NotInCharacter;
            if (value == "ambiguous_intent") return RefusalReason.AmbiguousIntent;
            return RefusalReason.None;
        }
    }

    public sealed class RelationProposal
    {
        public RelationKind Kind;
        public string TargetRef;
        public RelationStance Stance;
        public string Basis;

        public static RelationProposal Empty() => new RelationProposal { Kind = RelationKind.None, TargetRef = "none", Stance = RelationStance.None, Basis = "无关系提议" };
    }

    public sealed class ActionIntent
    {
        public string Name;
        public string ActorRef;
        public string TargetRef;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>(StringComparer.Ordinal);

        public static ActionIntent Empty() => new ActionIntent { Name = "none", ActorRef = "none", TargetRef = "none" };
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

        public static bool TryParseObject(string json, out Dictionary<string, object> value, out string error)
        {
            value = null;
            var reader = new JsonReader(json);
            object parsed;
            if (!reader.TryReadValue(out parsed) || !(parsed is Dictionary<string, object>) || !reader.IsAtEnd)
            {
                error = reader.Error ?? "invalid_json";
                return false;
            }
            value = (Dictionary<string, object>)parsed;
            error = null;
            return true;
        }

        private sealed class JsonReader
        {
            private readonly string _json;
            private int _index;
            public string Error;
            public bool IsAtEnd
            {
                get { SkipWhitespace(); return _index == (_json == null ? 0 : _json.Length); }
            }

            public JsonReader(string json) { _json = json ?? string.Empty; }

            public bool TryReadValue(out object value)
            {
                value = null;
                SkipWhitespace();
                if (_index >= _json.Length) return Fail("unexpected_end");
                char next = _json[_index];
                if (next == '{') return TryReadObject(out value);
                if (next == '[') return TryReadArray(out value);
                if (next == '"') return TryReadString(out value);
                if (next == 't' && Consume("true")) { value = true; return true; }
                if (next == 'f' && Consume("false")) { value = false; return true; }
                if (next == 'n' && Consume("null")) { value = null; return true; }
                return TryReadNumber(out value);
            }

            private bool TryReadObject(out object value)
            {
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                value = result;
                _index++;
                SkipWhitespace();
                if (ConsumeChar('}')) return true;
                while (_index < _json.Length)
                {
                    object keyValue;
                    if (!TryReadString(out keyValue)) return false;
                    string key = keyValue as string;
                    if (key == null) return Fail("object_key_not_string");
                    if (result.ContainsKey(key)) return Fail("duplicate_key");
                    SkipWhitespace();
                    if (!ConsumeChar(':')) return Fail("missing_colon");
                    object child;
                    if (!TryReadValue(out child)) return false;
                    result.Add(key, child);
                    SkipWhitespace();
                    if (ConsumeChar('}')) return true;
                    if (!ConsumeChar(',')) return Fail("missing_comma");
                    SkipWhitespace();
                }
                return Fail("unterminated_object");
            }

            private bool TryReadArray(out object value)
            {
                var result = new List<object>();
                value = result;
                _index++;
                SkipWhitespace();
                if (ConsumeChar(']')) return true;
                while (_index < _json.Length)
                {
                    object child;
                    if (!TryReadValue(out child)) return false;
                    result.Add(child);
                    SkipWhitespace();
                    if (ConsumeChar(']')) return true;
                    if (!ConsumeChar(',')) return Fail("missing_comma");
                    SkipWhitespace();
                }
                return Fail("unterminated_array");
            }

            private bool TryReadString(out object value)
            {
                value = null;
                if (!ConsumeChar('"')) return Fail("expected_string");
                var builder = new StringBuilder();
                while (_index < _json.Length)
                {
                    char c = _json[_index++];
                    if (c == '"') { value = builder.ToString(); return true; }
                    if (c == '\\')
                    {
                        if (_index >= _json.Length) return Fail("unterminated_escape");
                        char escaped = _json[_index++];
                        switch (escaped)
                        {
                            case '"': builder.Append('"'); break; case '\\': builder.Append('\\'); break;
                            case '/': builder.Append('/'); break; case 'b': builder.Append('\b'); break;
                            case 'f': builder.Append('\f'); break; case 'n': builder.Append('\n'); break;
                            case 'r': builder.Append('\r'); break; case 't': builder.Append('\t'); break;
                            case 'u':
                                if (_index + 4 > _json.Length) return Fail("invalid_unicode_escape");
                                string hex = _json.Substring(_index, 4);
                                int code;
                                if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code)) return Fail("invalid_unicode_escape");
                                builder.Append((char)code); _index += 4; break;
                            default: return Fail("invalid_escape");
                        }
                    }
                    else
                    {
                        if (c < 0x20) return Fail("control_character_in_string");
                        builder.Append(c);
                    }
                }
                return Fail("unterminated_string");
            }

            private bool TryReadNumber(out object value)
            {
                value = null;
                int start = _index;
                if (_json[_index] == '-') _index++;
                while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
                if (_index < _json.Length && _json[_index] == '.')
                {
                    _index++;
                    while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
                }
                if (_index < _json.Length && (_json[_index] == 'e' || _json[_index] == 'E'))
                {
                    _index++;
                    if (_index < _json.Length && (_json[_index] == '+' || _json[_index] == '-')) _index++;
                    while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
                }
                string numberText = _json.Substring(start, _index - start);
                double number;
                if (numberText.Length == 0 || !double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return Fail("invalid_number");
                value = number;
                return true;
            }

            private bool Consume(string text)
            {
                if (_index + text.Length > _json.Length || !string.Equals(_json.Substring(_index, text.Length), text, StringComparison.Ordinal)) return false;
                _index += text.Length;
                return true;
            }

            private bool ConsumeChar(char expected)
            {
                if (_index >= _json.Length || _json[_index] != expected) return false;
                _index++;
                return true;
            }

            private void SkipWhitespace()
            {
                while (_index < _json.Length && char.IsWhiteSpace(_json[_index])) _index++;
            }

            private bool Fail(string error)
            {
                Error = error;
                return false;
            }
        }
    }
}
