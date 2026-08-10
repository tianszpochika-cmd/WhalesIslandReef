using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Whalebone.Game.Config;

namespace Whalebone.Agent
{
    /// <summary>
    /// AgentReply v1.1 的唯一进入边界。TryParse 做契约/注册表校验；TryValidate 还做权限、目标和前置校验。
    /// </summary>
    public static class AgentReplyValidator
    {
        private static readonly string[] AllowedKeys =
        {
            "schema_version", "speech", "dialogue_intent", "emotion", "affinity_change",
            "relation_proposal", "negotiation_tone", "refusal_reason", "action_intent"
        };

        private static readonly string[] DialogueIntents = { "greet", "inform", "ask", "answer", "warn", "suggest", "negotiate", "refuse", "apologize", "report", "confirm", "farewell" };
        private static readonly string[] Emotions = { "neutral", "suspicious", "happy", "angry", "sad", "afraid", "calm", "concerned", "determined", "relieved", "disgusted", "hopeful" };
        private static readonly string[] RelationKinds = { "none", "trust", "respect", "affinity", "loyalty", "fear", "rivalry", "romance", "professional" };
        private static readonly string[] RelationStances = { "none", "build", "maintain", "repair", "challenge", "avoid", "request_mediation", "offer_cooperation", "accept", "decline" };
        private static readonly string[] NegotiationTones = { "neutral", "cooperative", "firm", "conciliatory", "guarded", "competitive", "formal", "urgent" };
        private static readonly string[] RefusalReasons = { "none", "unknown_request", "insufficient_authority", "unsafe_target", "missing_prerequisite", "conflicting_order", "personal_boundary", "legal_restriction", "resource_unavailable", "out_of_scope", "not_in_character", "ambiguous_intent" };
        private static readonly HashSet<string> ForbiddenResultFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "money", "price", "damage", "reward", "weather", "relation_result", "relation_value", "affinity_result", "final_money", "final_price", "final_damage", "final_reward", "final_weather", "final_relation"
        };

        public static bool TryParse(string json, out AgentReply reply, out string error)
        {
            return TryParseInternal(json, ActionValidationContext.SchemaOnly(), true, out reply, out error);
        }

        /// <summary>D2 兼容入口：契约仍是 v1.1，但对话阶段不允许执行 P2A。</summary>
        public static bool TryParseD2(string json, out AgentReply reply, out string error)
        {
            return TryParseInternal(json, ActionValidationContext.SchemaOnly(), false, out reply, out error);
        }

        /// <summary>运行时入口：在契约通过后检查当前 Agent 是否有权操作指定对象和目标。</summary>
        public static bool TryValidate(string json, ActionValidationContext context, out AgentReply reply, out string error)
        {
            return TryParseInternal(json, context ?? new ActionValidationContext(), true, out reply, out error);
        }

        private static bool TryParseInternal(string json, ActionValidationContext context, bool allowActions, out AgentReply reply, out string error)
        {
            reply = null;
            error = null;
            Dictionary<string, object> root;
            string parseError = null;
            if (string.IsNullOrWhiteSpace(json) || !MiniJson.TryParseObject(json, out root, out parseError)) return Fail(parseError ?? "invalid_object", out error);
            if (!HasNoUnknownKeys(root, AllowedKeys)) return Fail("invalid_object", out error);

            string schemaVersion;
            if (!RequiredString(root, "schema_version", out schemaVersion) || schemaVersion != AgentReply.CurrentSchemaVersion) return Fail("invalid_schema_version", out error);
            string speech;
            if (!RequiredString(root, "speech", out speech)) return Fail("missing_speech", out error);
            if (speech.Trim().Length == 0) return Fail("empty_speech", out error);
            if (speech.Length > 240) return Fail("speech_too_long", out error);

            string dialogueIntent;
            if (!RequiredEnum(root, "dialogue_intent", DialogueIntents, out dialogueIntent)) return Fail("invalid_dialogue_intent", out error);
            string emotion;
            if (!RequiredEnum(root, "emotion", Emotions, out emotion)) return Fail("invalid_emotion", out error);
            double affinity;
            if (!RequiredNumber(root, "affinity_change", out affinity)) return Fail("invalid_affinity_change", out error);
            var proposalBounds = NumericConfiguration.Default.RelationProposalDelta;
            if (Math.Abs(affinity - Math.Round(affinity)) > 0.000001
                || affinity < (double)proposalBounds.Minimum || affinity > (double)proposalBounds.Maximum)
                return Fail("invalid_affinity_change", out error);

            Dictionary<string, object> relation;
            if (!RequiredObject(root, "relation_proposal", out relation) || !ValidateRelationProposal(relation, context, out error)) return false;
            string tone;
            if (!RequiredEnum(root, "negotiation_tone", NegotiationTones, out tone)) return Fail("invalid_negotiation_tone", out error);
            string refusal;
            if (!RequiredEnum(root, "refusal_reason", RefusalReasons, out refusal)) return Fail("invalid_refusal_reason", out error);

            Dictionary<string, object> action;
            if (!RequiredObject(root, "action_intent", out action)) return Fail("missing_action_intent", out error);
            string actionName;
            if (!RequiredString(action, "name", out actionName)) return Fail("invalid_action_intent", out error);
            if (!allowActions && actionName != "none") return Fail("action_not_allowed", out error);
            if (!ValidateAction(action, context, out error)) return false;
            if (refusal != "none" && actionName != "none") return Fail("refusal_with_action", out error);

            reply = AgentReply.FromMap(root);
            return true;
        }

        private static bool ValidateRelationProposal(Dictionary<string, object> relation, ActionValidationContext context, out string error)
        {
            error = null;
            if (!HasOnlyKeys(relation, new[] { "kind", "target_ref", "stance", "basis" })) return Fail("invalid_relation_proposal", out error);
            string kind;
            string target;
            string stance;
            string basis;
            if (!RequiredEnum(relation, "kind", RelationKinds, out kind) || !RequiredString(relation, "target_ref", out target) || !RequiredEnum(relation, "stance", RelationStances, out stance) || !RequiredString(relation, "basis", out basis)) return Fail("invalid_relation_proposal", out error);
            if (basis.Length == 0 || basis.Length > 160 || !IsReference(target)) return Fail("invalid_relation_proposal", out error);
            if (kind == "none" && (target != "none" || stance != "none")) return Fail("invalid_relation_proposal", out error);
            if (context.EnforceAuthorization && target != "none" && !context.HasTarget(target)) return Fail("unauthorized_target", out error);
            return true;
        }

        private static bool ValidateAction(Dictionary<string, object> action, ActionValidationContext context, out string error)
        {
            error = null;
            if (!HasOnlyKeys(action, new[] { "name", "actor_ref", "target_ref", "parameters" })) return Fail("invalid_action_intent", out error);
            string name;
            string actor;
            string target;
            Dictionary<string, object> parameters;
            if (!RequiredString(action, "name", out name) || !RequiredString(action, "actor_ref", out actor) || !RequiredString(action, "target_ref", out target) || !RequiredObject(action, "parameters", out parameters)) return Fail("invalid_action_intent", out error);
            if (!IsReference(actor) || !IsReference(target)) return Fail("invalid_action_reference", out error);

            if (name == "none")
            {
                if (actor != "none" || target != "none" || parameters.Count != 0) return Fail("invalid_noop_action", out error);
                return true;
            }

            P2AActionDefinition definition;
            if (!P2AActionRegistry.TryGet(name, out definition)) return Fail("unknown_action", out error);
            if (!MatchesKind(actor, definition.AllowedActorKinds) || !MatchesKind(target, definition.AllowedTargetKinds)) return Fail("invalid_target_kind", out error);
            if (context.EnforceAuthorization)
            {
                if (!context.HasActor(actor)) return Fail("unauthorized_actor", out error);
                if (target != "none" && !context.HasTarget(target)) return Fail("unauthorized_target", out error);
                foreach (var permission in definition.Permissions) if (!context.HasPermission(permission)) return Fail("permission_denied", out error);
                foreach (var prerequisite in definition.Prerequisites) if (!context.HasPrerequisite(prerequisite)) return Fail("missing_prerequisite", out error);
            }

            foreach (var pair in parameters)
            {
                if (ForbiddenResultFields.Contains(pair.Key)) return Fail("forbidden_result_field", out error);
                P2AParameterDefinition parameter;
                if (!definition.Parameters.TryGetValue(pair.Key, out parameter)) return Fail("invalid_action_parameters", out error);
                string parameterError;
                if (!parameter.Validate(pair.Value, out parameterError)) return Fail("invalid_action_parameters", out error);
            }
            foreach (var pair in definition.Parameters)
                if (pair.Value.Required && !parameters.ContainsKey(pair.Key)) return Fail("missing_action_parameter", out error);
            if (ContainsForbiddenResultField(parameters)) return Fail("forbidden_result_field", out error);
            return true;
        }

        private static bool ContainsForbiddenResultField(object value)
        {
            var map = value as Dictionary<string, object>;
            if (map != null)
            {
                foreach (var pair in map)
                {
                    if (ForbiddenResultFields.Contains(pair.Key) || ContainsForbiddenResultField(pair.Value)) return true;
                }
                return false;
            }
            var list = value as List<object>;
            if (list != null) foreach (var item in list) if (ContainsForbiddenResultField(item)) return true;
            return false;
        }

        private static bool MatchesKind(string reference, string[] allowedKinds)
        {
            string kind = P2AActionRegistry.GetReferenceKind(reference);
            return Array.IndexOf(allowedKinds, kind) >= 0;
        }

        private static bool IsReference(string value)
        {
            return value == "none" || (!string.IsNullOrEmpty(value) && Regex.IsMatch(value, "^[a-z][a-z0-9_]*:[A-Za-z0-9._:-]+$"));
        }

        private static bool HasOnlyKeys(Dictionary<string, object> values, string[] allowedKeys)
        {
            if (values == null || allowedKeys == null || values.Count != allowedKeys.Length) return false;
            var allowed = new HashSet<string>(allowedKeys, StringComparer.Ordinal);
            foreach (var key in values.Keys) if (!allowed.Contains(key)) return false;
            return true;
        }

        private static bool HasNoUnknownKeys(Dictionary<string, object> values, string[] allowedKeys)
        {
            if (values == null || allowedKeys == null) return false;
            var allowed = new HashSet<string>(allowedKeys, StringComparer.Ordinal);
            foreach (var key in values.Keys) if (!allowed.Contains(key)) return false;
            return true;
        }

        private static bool RequiredString(Dictionary<string, object> values, string key, out string result)
        {
            object value;
            result = null;
            return values != null && values.TryGetValue(key, out value) && (result = value as string) != null;
        }

        private static bool RequiredNumber(Dictionary<string, object> values, string key, out double result)
        {
            object value;
            result = 0;
            return values != null && values.TryGetValue(key, out value) && value is double && (result = (double)value) == (double)value;
        }

        private static bool RequiredObject(Dictionary<string, object> values, string key, out Dictionary<string, object> result)
        {
            object value;
            result = null;
            return values != null && values.TryGetValue(key, out value) && (result = value as Dictionary<string, object>) != null;
        }

        private static bool RequiredEnum(Dictionary<string, object> values, string key, string[] allowed, out string result)
        {
            if (!RequiredString(values, key, out result)) return false;
            return Array.IndexOf(allowed, result) >= 0;
        }

        private static bool Fail(string code, out string error)
        {
            error = code;
            return false;
        }
    }
}
