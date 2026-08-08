// ============================================================
// SaveJson —— 极简 JSON 编解码（存档专用）
//
// 为什么自写：Unity 的 JsonUtility 不支持 Dictionary；MiniJson（AgentCore）
//   只提供只读提取；引入第三方 JSON 库会带来版本/兼容包袱。
//   存档字段均为简单组合（对象/数组/标量），一个递归下降解析器足够，
//   且保持 Systems 层"零第三方托管依赖"。
//
// 中间表示约定：
//   Parse → object（Dictionary<string,object> | List<object> | string |
//                    long | double | bool | null）
//   Stringify(object) → 对应 JSON 文本（整数写为 long/int，浮点写为 double/float）
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Whalebone.Systems.Save
{
    internal static class SaveJson
    {
        // ---------------- 解码 ----------------

        public static object Parse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            var p = new Parser(json);
            object v = p.ParseValue();
            p.SkipWs();
            if (!p.AtEnd) throw new FormatException("JSON 尾部存在多余内容: " + p.Peek());
            return v;
        }

        public static Dictionary<string, object> ParseDict(string json)
        {
            object v = Parse(json);
            return v as Dictionary<string, object>;
        }

        public static List<object> ParseArray(string json)
        {
            object v = Parse(json);
            return v as List<object>;
        }

        private sealed class Parser
        {
            private readonly string _s;
            private int _i;

            public Parser(string s) { _s = s; }
            public bool AtEnd => _i >= _s.Length;
            public char Peek() => _s[_i];

            public void SkipWs()
            {
                while (_i < _s.Length && (_s[_i] == ' ' || _s[_i] == '\t' || _s[_i] == '\n' || _s[_i] == '\r')) _i++;
            }

            public object ParseValue()
            {
                SkipWs();
                if (AtEnd) throw new FormatException("JSON 意外结束");
                char c = _s[_i];
                switch (c)
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default: return ParseNumber();
                }
            }

            private void Expect(string word)
            {
                if (_i + word.Length > _s.Length || _s.Substring(_i, word.Length) != word)
                    throw new FormatException("非法 JSON 字面量");
                _i += word.Length;
            }

            private Dictionary<string, object> ParseObject()
            {
                var obj = new Dictionary<string, object>(StringComparer.Ordinal);
                _i++; // '{'
                SkipWs();
                if (!AtEnd && _s[_i] == '}') { _i++; return obj; }
                while (true)
                {
                    SkipWs();
                    if (AtEnd || _s[_i] != '"') throw new FormatException("对象键必须是字符串");
                    string key = ParseString();
                    SkipWs();
                    if (AtEnd || _s[_i] != ':') throw new FormatException("对象缺冒号");
                    _i++;
                    obj[key] = ParseValue();
                    SkipWs();
                    if (AtEnd) throw new FormatException("对象未闭合");
                    if (_s[_i] == ',') { _i++; continue; }
                    if (_s[_i] == '}') { _i++; return obj; }
                    throw new FormatException("对象分隔符非法");
                }
            }

            private List<object> ParseArray()
            {
                var list = new List<object>();
                _i++; // '['
                SkipWs();
                if (!AtEnd && _s[_i] == ']') { _i++; return list; }
                while (true)
                {
                    list.Add(ParseValue());
                    SkipWs();
                    if (AtEnd) throw new FormatException("数组未闭合");
                    if (_s[_i] == ',') { _i++; continue; }
                    if (_s[_i] == ']') { _i++; return list; }
                    throw new FormatException("数组分隔符非法");
                }
            }

            private string ParseString()
            {
                _i++; // '"'
                var sb = new StringBuilder();
                while (true)
                {
                    if (AtEnd) throw new FormatException("字符串未闭合");
                    char c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (AtEnd) throw new FormatException("转义未完成");
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw new FormatException("\\u 转义非法");
                            string hex = _s.Substring(_i, 4);
                            _i += 4;
                            sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            break;
                        default: throw new FormatException("非法转义 \\" + e);
                    }
                }
            }

            private object ParseNumber()
            {
                int start = _i;
                while (_i < _s.Length && "-+0123456789.eE".IndexOf(_s[_i]) >= 0) _i++;
                string num = _s.Substring(start, _i - start);
                if (num.Length == 0 || num == "-" || num == ".") throw new FormatException("非法数字");
                if (num.IndexOf('.') >= 0 || num.IndexOf('e') >= 0 || num.IndexOf('E') >= 0)
                    return double.Parse(num, NumberStyles.Float, CultureInfo.InvariantCulture);
                return long.Parse(num, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
        }

        // ---------------- 编码 ----------------

        public static string Stringify(object value)
        {
            var sb = new StringBuilder(256);
            WriteValue(sb, value);
            return sb.ToString();
        }

        private static void WriteValue(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            if (v is string s) { WriteString(sb, s); return; }
            if (v is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (v is long l) { sb.Append(l.ToString(CultureInfo.InvariantCulture)); return; }
            if (v is int i) { sb.Append(i.ToString(CultureInfo.InvariantCulture)); return; }
            if (v is float f) { WriteNumber(sb, (double)f); return; }
            if (v is double d) { WriteNumber(sb, d); return; }
            if (v is IDictionary dict)
            {
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry entry in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteString(sb, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                    sb.Append(':');
                    WriteValue(sb, entry.Value);
                }
                sb.Append('}');
                return;
            }
            if (v is IList list)
            {
                sb.Append('[');
                bool first = true;
                foreach (object item in list)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WriteValue(sb, item);
                }
                sb.Append(']');
                return;
            }
            if (v is Enum e) { WriteString(sb, e.ToString()); return; }
            // 未知类型兜底：按字符串处理（存档字段不应出现）
            WriteString(sb, v.ToString());
        }

        private static void WriteNumber(StringBuilder sb, double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) { sb.Append("0"); return; }
            sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20)
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        // ---------------- 便捷读取 ----------------

        public static string GetStr(Dictionary<string, object> d, string key, string def = null)
        {
            return d != null && d.TryGetValue(key, out object v) && v is string s ? s : def;
        }

        public static int GetInt(Dictionary<string, object> d, string key, int def = 0)
        {
            if (d == null || !d.TryGetValue(key, out object v)) return def;
            if (v is long l) return (int)l;
            if (v is double dbl) return (int)dbl;
            return def;
        }

        public static float GetFloat(Dictionary<string, object> d, string key, float def = 0f)
        {
            if (d == null || !d.TryGetValue(key, out object v)) return def;
            if (v is double dbl) return (float)dbl;
            if (v is long l) return l;
            return def;
        }
    }
}
