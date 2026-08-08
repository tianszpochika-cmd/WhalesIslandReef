using System;

namespace Whalebone.Agent
{
    /// <summary>
    /// AgentReply 的边界校验器。只有通过这里的回复才能进入游戏逻辑。
    /// </summary>
    public static class AgentReplyValidator
    {
        private static readonly string[] AllowedKeys =
            { "speech", "emotion", "affinity_change", "action", "action_args" };

        public static bool TryParse(string json, out AgentReply reply, out string error)
        {
            return TryParse(json, true, out reply, out error);
        }

        /// <summary>D2 使用 allowActions=false，避免 LLM 直接触发未实现动作。</summary>
        public static bool TryParseD2(string json, out AgentReply reply, out string error)
        {
            return TryParse(json, false, out reply, out error);
        }

        private static bool TryParse(string json, bool allowActions, out AgentReply reply, out string error)
        {
            reply = null;
            error = null;
            if (string.IsNullOrWhiteSpace(json)) return Fail("empty_json", out error);
            if (!MiniJson.HasOnlyTopLevelKeys(json, AllowedKeys)) return Fail("invalid_object", out error);
            if (!MiniJson.HasKey(json, "speech")) return Fail("missing_speech", out error);
            if (!MiniJson.HasKey(json, "emotion")) return Fail("missing_emotion", out error);
            if (!MiniJson.HasKey(json, "affinity_change")) return Fail("missing_affinity_change", out error);
            if (!MiniJson.HasKey(json, "action")) return Fail("missing_action", out error);

            string speech = MiniJson.GetString(json, "speech");
            if (string.IsNullOrWhiteSpace(speech)) return Fail("empty_speech", out error);

            string emotionText = MiniJson.GetString(json, "emotion");
            if (!Enum.TryParse(emotionText, true, out Emotion emotion))
                return Fail("invalid_emotion", out error);

            double affinity = MiniJson.GetNumber(json, "affinity_change");
            if (Math.Abs(affinity - Math.Round(affinity)) > 0.000001 || affinity < -10 || affinity > 10)
                return Fail("invalid_affinity_change", out error);

            string action = MiniJson.GetString(json, "action");
            if (string.IsNullOrWhiteSpace(action)) return Fail("empty_action", out error);
            if (!allowActions && !string.Equals(action, "none", StringComparison.OrdinalIgnoreCase))
                return Fail("action_not_allowed", out error);

            reply = AgentReply.FromJson(json);
            reply.Emotion = emotion;
            reply.AffinityChange = (float)affinity;
            return true;
        }

        private static bool Fail(string code, out string error)
        {
            error = code;
            return false;
        }
    }
}
