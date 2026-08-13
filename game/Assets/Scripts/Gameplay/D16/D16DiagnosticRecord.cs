using System;
using System.Text;
using UnityEngine;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// D16 本地诊断记录的最小字段构造器。
    /// 原始错误文本和堆栈永远不写入记录，避免路径、账号、存档正文或对话内容泄漏。
    /// </summary>
    [Serializable]
    public sealed class D16DiagnosticRecord
    {
        public string schema = "d16_local_diagnostic_v1";
        public string utc;
        public string severity;
        public string category;
        public string scene;
        public string unity_version;
        public string platform;
        public int condition_length;
        public int stack_line_count;
        public bool raw_message_omitted = true;
        public bool raw_stack_omitted = true;

        public static string CreateJson(string utc, string severity, string scene, string unityVersion,
            string platform, string condition, string stackTrace)
        {
            return CreateJsonFromMetadata(
                utc,
                severity,
                Classify(severity, condition),
                scene,
                unityVersion,
                platform,
                condition == null ? 0 : condition.Length,
                CountLines(stackTrace));
        }

        /// <summary>
        /// 使用已经脱敏的元数据构造记录。
        /// 该入口供后台线程日志队列在主线程落盘时使用，避免把原始文本跨线程保存。
        /// </summary>
        public static string CreateJsonFromMetadata(string utc, string severity, string category,
            string scene, string unityVersion, string platform, int conditionLength, int stackLineCount)
        {
            var record = new D16DiagnosticRecord
            {
                utc = SanitizeToken(utc, "unknown-time"),
                severity = SanitizeToken(severity, "unknown"),
                category = SanitizeToken(category, "runtime_diagnostic"),
                scene = SanitizeToken(scene, "unknown-scene"),
                unity_version = SanitizeToken(unityVersion, "unknown-unity"),
                platform = SanitizeToken(platform, "unknown-platform"),
                condition_length = Math.Max(0, Math.Min(conditionLength, 4096)),
                stack_line_count = Math.Max(0, Math.Min(stackLineCount, 4096)),
            };
            return JsonUtility.ToJson(record, false);
        }

        public static string Classify(string severity, string condition)
        {
            string normalizedSeverity = (severity ?? string.Empty).Trim().ToLowerInvariant();
            string normalizedCondition = (condition ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedCondition.Contains("nullreferenceexception")) return "null_reference";
            if (normalizedCondition.Contains("missingreferenceexception")) return "missing_reference";
            if (normalizedCondition.Contains("no cameras rendering")) return "camera";
            if (normalizedCondition.Contains("input") && normalizedCondition.Contains("not normalized")) return "input";
            if (normalizedSeverity.Contains("exception")) return "exception";
            if (normalizedSeverity.Contains("assert")) return "assert";
            if (normalizedSeverity.Contains("error")) return "error";
            return "runtime_diagnostic";
        }

        private static int CountLines(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            int count = 1;
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\n') count++;
            return Math.Min(count, 4096);
        }

        private static string SanitizeToken(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            var builder = new StringBuilder(Math.Min(value.Length, 96));
            for (int i = 0; i < value.Length && builder.Length < 96; i++)
            {
                char character = value[i];
                bool allowed = (character >= 'a' && character <= 'z') ||
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9') ||
                    character == '_' || character == '-' || character == '.' || character == ':';
                builder.Append(allowed ? character : '_');
            }
            return builder.Length == 0 ? fallback : builder.ToString();
        }
    }
}
