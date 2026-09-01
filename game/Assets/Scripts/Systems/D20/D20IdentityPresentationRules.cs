// ============================================================
// D20IdentityPresentationRules —— D20-L10 身份只读表现映射（纯 C#）
//
// 本文件只消费 D10 的身份公开快照，把姓名、称谓、代词、本地化文本和
// 历史表达映射为防御性读模型。它不写 D10 身份、法律、关系或外观事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D10;

namespace Whalebone.Systems.D20
{
    /// <summary>身份表现使用的稳定语言键。</summary>
    public enum D20IdentityPresentationLanguage
    {
        /// <summary>简体中文。</summary>
        ZhCn = 0,
        /// <summary>英文（美国）。</summary>
        EnUs = 1,
    }

    /// <summary>L10 身份表现映射失败码。</summary>
    public enum D20IdentityPresentationCode
    {
        /// <summary>映射成功。</summary>
        None = 0,
        /// <summary>请求为空。</summary>
        MissingRequest = 1,
        /// <summary>版本不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>身份快照为空。</summary>
        MissingIdentity = 3,
        /// <summary>玩家 ID 缺失或不稳定。</summary>
        MissingPlayerId = 4,
        /// <summary>姓名缺失或过长。</summary>
        InvalidName = 5,
        /// <summary>性别/年龄/文化稳定键不合法。</summary>
        InvalidIdentityKey = 6,
        /// <summary>称谓或备注本地化载体不合法。</summary>
        InvalidLocalizedText = 7,
        /// <summary>代词键不稳定。</summary>
        InvalidPronounKey = 8,
        /// <summary>历史条目包含不稳定引用。</summary>
        InvalidHistoryEntry = 9,
        /// <summary>语言键不支持。</summary>
        UnsupportedLanguage = 10,
    }

    /// <summary>身份表现映射请求；D10 输入在使用前会被复制。</summary>
    [Serializable]
    public sealed class D20IdentityPresentationRequest
    {
        /// <summary>请求版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>请求版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>D10 身份公开快照。</summary>
        public D10PlayerIdentityState Identity;

        /// <summary>语言键，如 zh-CN 或 en-US。</summary>
        public string Language = "zh-CN";

        /// <summary>是否明确允许展示敏感身份历史细节。</summary>
        public bool RevealSensitiveHistory;

        /// <summary>返回不共享身份集合的请求副本。</summary>
        public D20IdentityPresentationRequest Clone()
        {
            return new D20IdentityPresentationRequest
            {
                SchemaVersion = SchemaVersion,
                Identity = Identity == null ? null : Identity.Clone(),
                Language = Language ?? string.Empty,
                RevealSensitiveHistory = RevealSensitiveHistory,
            };
        }
    }

    /// <summary>本地化文本的只读映射，记录是否使用了安全回退。</summary>
    [Serializable]
    public sealed class D20LocalizedPresentationText
    {
        private readonly string _key;
        private readonly string _text;
        private readonly D20IdentityPresentationLanguage _language;
        private readonly bool _usedFallback;

        internal D20LocalizedPresentationText(string key, string text,
            D20IdentityPresentationLanguage language, bool usedFallback)
        {
            _key = key ?? string.Empty;
            _text = text ?? string.Empty;
            _language = language;
            _usedFallback = usedFallback;
        }

        /// <summary>稳定本地化 key。</summary>
        public string Key { get { return _key; } }
        /// <summary>按请求语言选择的文本；缺失时为空，不伪造文案。</summary>
        public string Text { get { return _text; } }
        /// <summary>选择语言。</summary>
        public D20IdentityPresentationLanguage Language { get { return _language; } }
        /// <summary>是否没有目标语言而使用了其它语言或 key 回退。</summary>
        public bool UsedFallback { get { return _usedFallback; } }

        /// <summary>返回值语义相同的副本。</summary>
        public D20LocalizedPresentationText Clone()
        {
            return new D20LocalizedPresentationText(_key, _text, _language, _usedFallback);
        }
    }

    /// <summary>身份历史的安全只读映射。</summary>
    [Serializable]
    public sealed class D20IdentityHistoryPresentation
    {
        private readonly string _historyId;
        private readonly int _worldDay;
        private readonly string _changeKind;
        private readonly string _changeReasonKey;
        private readonly bool _isSensitive;
        private readonly bool _isRedacted;
        private readonly string _redactionKey;
        private readonly string _previousName;
        private readonly string _newName;
        private readonly string _previousGenderExpressionKey;
        private readonly string _newGenderExpressionKey;
        private readonly ReadOnlyCollection<string> _previousPronounKeys;
        private readonly ReadOnlyCollection<string> _newPronounKeys;
        private readonly string _previousTitleKey;
        private readonly string _newTitleKey;
        private readonly string _previousAgeBandKey;
        private readonly string _newAgeBandKey;
        private readonly string _previousCultureOriginKey;
        private readonly string _newCultureOriginKey;
        private readonly string _previousAppearanceLoadoutId;
        private readonly string _newAppearanceLoadoutId;
        private readonly D20LocalizedPresentationText _note;

        internal D20IdentityHistoryPresentation(
            string historyId,
            int worldDay,
            string changeKind,
            string changeReasonKey,
            bool isSensitive,
            bool isRedacted,
            string redactionKey,
            string previousName,
            string newName,
            string previousGenderExpressionKey,
            string newGenderExpressionKey,
            IEnumerable<string> previousPronounKeys,
            IEnumerable<string> newPronounKeys,
            string previousTitleKey,
            string newTitleKey,
            string previousAgeBandKey,
            string newAgeBandKey,
            string previousCultureOriginKey,
            string newCultureOriginKey,
            string previousAppearanceLoadoutId,
            string newAppearanceLoadoutId,
            D20LocalizedPresentationText note)
        {
            _historyId = historyId ?? string.Empty;
            _worldDay = worldDay;
            _changeKind = changeKind ?? string.Empty;
            _changeReasonKey = changeReasonKey ?? string.Empty;
            _isSensitive = isSensitive;
            _isRedacted = isRedacted;
            _redactionKey = redactionKey ?? string.Empty;
            _previousName = previousName ?? string.Empty;
            _newName = newName ?? string.Empty;
            _previousGenderExpressionKey = previousGenderExpressionKey ?? string.Empty;
            _newGenderExpressionKey = newGenderExpressionKey ?? string.Empty;
            _previousPronounKeys = ToReadOnly(previousPronounKeys);
            _newPronounKeys = ToReadOnly(newPronounKeys);
            _previousTitleKey = previousTitleKey ?? string.Empty;
            _newTitleKey = newTitleKey ?? string.Empty;
            _previousAgeBandKey = previousAgeBandKey ?? string.Empty;
            _newAgeBandKey = newAgeBandKey ?? string.Empty;
            _previousCultureOriginKey = previousCultureOriginKey ?? string.Empty;
            _newCultureOriginKey = newCultureOriginKey ?? string.Empty;
            _previousAppearanceLoadoutId = previousAppearanceLoadoutId ?? string.Empty;
            _newAppearanceLoadoutId = newAppearanceLoadoutId ?? string.Empty;
            _note = note == null ? null : note.Clone();
        }

        /// <summary>历史稳定 ID。</summary>
        public string HistoryId { get { return _historyId; } }
        /// <summary>历史发生世界日。</summary>
        public int WorldDay { get { return _worldDay; } }
        /// <summary>变更类别。</summary>
        public string ChangeKind { get { return _changeKind; } }
        /// <summary>变更原因 key。</summary>
        public string ChangeReasonKey { get { return _changeReasonKey; } }
        /// <summary>是否包含敏感身份表达。</summary>
        public bool IsSensitive { get { return _isSensitive; } }
        /// <summary>是否按默认策略脱敏。</summary>
        public bool IsRedacted { get { return _isRedacted; } }
        /// <summary>稳定脱敏提示 key。</summary>
        public string RedactionKey { get { return _redactionKey; } }
        /// <summary>历史前姓名；脱敏时为空。</summary>
        public string PreviousName { get { return _previousName; } }
        /// <summary>历史后姓名；脱敏时为空。</summary>
        public string NewName { get { return _newName; } }
        /// <summary>历史前性别表达 key。</summary>
        public string PreviousGenderExpressionKey { get { return _previousGenderExpressionKey; } }
        /// <summary>历史后性别表达 key。</summary>
        public string NewGenderExpressionKey { get { return _newGenderExpressionKey; } }
        /// <summary>历史前代词 key。</summary>
        public IReadOnlyList<string> PreviousPronounKeys { get { return _previousPronounKeys; } }
        /// <summary>历史后代词 key。</summary>
        public IReadOnlyList<string> NewPronounKeys { get { return _newPronounKeys; } }
        /// <summary>历史前称谓 key。</summary>
        public string PreviousTitleKey { get { return _previousTitleKey; } }
        /// <summary>历史后称谓 key。</summary>
        public string NewTitleKey { get { return _newTitleKey; } }
        /// <summary>历史前年龄段 key。</summary>
        public string PreviousAgeBandKey { get { return _previousAgeBandKey; } }
        /// <summary>历史后年龄段 key。</summary>
        public string NewAgeBandKey { get { return _newAgeBandKey; } }
        /// <summary>历史前文化出身 key。</summary>
        public string PreviousCultureOriginKey { get { return _previousCultureOriginKey; } }
        /// <summary>历史后文化出身 key。</summary>
        public string NewCultureOriginKey { get { return _newCultureOriginKey; } }
        /// <summary>历史前外观引用。</summary>
        public string PreviousAppearanceLoadoutId { get { return _previousAppearanceLoadoutId; } }
        /// <summary>历史后外观引用。</summary>
        public string NewAppearanceLoadoutId { get { return _newAppearanceLoadoutId; } }
        /// <summary>历史备注的只读本地化映射。</summary>
        public D20LocalizedPresentationText Note { get { return _note == null ? null : _note.Clone(); } }

        /// <summary>返回不共享列表的副本。</summary>
        public D20IdentityHistoryPresentation Clone()
        {
            return new D20IdentityHistoryPresentation(
                _historyId, _worldDay, _changeKind, _changeReasonKey, _isSensitive,
                _isRedacted, _redactionKey, _previousName, _newName,
                _previousGenderExpressionKey, _newGenderExpressionKey,
                _previousPronounKeys, _newPronounKeys, _previousTitleKey, _newTitleKey,
                _previousAgeBandKey, _newAgeBandKey, _previousCultureOriginKey,
                _newCultureOriginKey, _previousAppearanceLoadoutId, _newAppearanceLoadoutId,
                _note);
        }

        private static ReadOnlyCollection<string> ToReadOnly(IEnumerable<string> values)
        {
            var copy = new List<string>();
            if (values != null)
                foreach (var value in values)
                    if (value != null) copy.Add(value);
            return new ReadOnlyCollection<string>(copy);
        }
    }

    /// <summary>身份完整表现读模型；只包含 D10 的防御性投影。</summary>
    [Serializable]
    public sealed class D20IdentityPresentationModel
    {
        private readonly int _schemaVersion;
        private readonly string _playerId;
        private readonly string _displayName;
        private readonly string _genderExpressionKey;
        private readonly string _ageBandKey;
        private readonly string _cultureOriginKey;
        private readonly D20IdentityPresentationLanguage _language;
        private readonly D20LocalizedPresentationText _title;
        private readonly D20LocalizedPresentationText _identityNote;
        private readonly ReadOnlyCollection<string> _pronounKeys;
        private readonly ReadOnlyCollection<D20IdentityHistoryPresentation> _identityHistory;
        private readonly int _documentCount;
        private readonly int _preservedLegalHistoryCount;
        private readonly int _preservedDebtHistoryCount;
        private readonly int _preservedReputationHistoryCount;
        private readonly int _preservedRelationHistoryCount;
        private readonly bool _hasLocalizationFallback;
        private readonly bool _sensitiveHistoryRedacted;

        internal D20IdentityPresentationModel(
            int schemaVersion,
            string playerId,
            string displayName,
            string genderExpressionKey,
            string ageBandKey,
            string cultureOriginKey,
            D20IdentityPresentationLanguage language,
            D20LocalizedPresentationText title,
            D20LocalizedPresentationText identityNote,
            IEnumerable<string> pronounKeys,
            IEnumerable<D20IdentityHistoryPresentation> identityHistory,
            int documentCount,
            int preservedLegalHistoryCount,
            int preservedDebtHistoryCount,
            int preservedReputationHistoryCount,
            int preservedRelationHistoryCount,
            bool hasLocalizationFallback,
            bool sensitiveHistoryRedacted)
        {
            _schemaVersion = schemaVersion;
            _playerId = playerId ?? string.Empty;
            _displayName = displayName ?? string.Empty;
            _genderExpressionKey = genderExpressionKey ?? string.Empty;
            _ageBandKey = ageBandKey ?? string.Empty;
            _cultureOriginKey = cultureOriginKey ?? string.Empty;
            _language = language;
            _title = title == null ? null : title.Clone();
            _identityNote = identityNote == null ? null : identityNote.Clone();
            _pronounKeys = ToReadOnly(pronounKeys);
            var history = new List<D20IdentityHistoryPresentation>();
            if (identityHistory != null)
                foreach (var entry in identityHistory)
                    if (entry != null) history.Add(entry.Clone());
            _identityHistory = new ReadOnlyCollection<D20IdentityHistoryPresentation>(history);
            _documentCount = Math.Max(0, documentCount);
            _preservedLegalHistoryCount = Math.Max(0, preservedLegalHistoryCount);
            _preservedDebtHistoryCount = Math.Max(0, preservedDebtHistoryCount);
            _preservedReputationHistoryCount = Math.Max(0, preservedReputationHistoryCount);
            _preservedRelationHistoryCount = Math.Max(0, preservedRelationHistoryCount);
            _hasLocalizationFallback = hasLocalizationFallback;
            _sensitiveHistoryRedacted = sensitiveHistoryRedacted;
        }

        /// <summary>读模型版本。</summary>
        public int SchemaVersion { get { return _schemaVersion; } }
        /// <summary>玩家 ID。</summary>
        public string PlayerId { get { return _playerId; } }
        /// <summary>当前姓名显示值。</summary>
        public string DisplayName { get { return _displayName; } }
        /// <summary>性别表达稳定 key。</summary>
        public string GenderExpressionKey { get { return _genderExpressionKey; } }
        /// <summary>年龄段稳定 key。</summary>
        public string AgeBandKey { get { return _ageBandKey; } }
        /// <summary>文化出身稳定 key。</summary>
        public string CultureOriginKey { get { return _cultureOriginKey; } }
        /// <summary>请求语言。</summary>
        public D20IdentityPresentationLanguage Language { get { return _language; } }
        /// <summary>称谓本地化映射。</summary>
        public D20LocalizedPresentationText Title { get { return _title == null ? null : _title.Clone(); } }
        /// <summary>身份备注本地化映射。</summary>
        public D20LocalizedPresentationText IdentityNote
        {
            get { return _identityNote == null ? null : _identityNote.Clone(); }
        }
        /// <summary>代词稳定 key 列表。</summary>
        public IReadOnlyList<string> PronounKeys { get { return _pronounKeys; } }
        /// <summary>身份历史安全投影。</summary>
        public IReadOnlyList<D20IdentityHistoryPresentation> IdentityHistory
        {
            get { return _identityHistory; }
        }
        /// <summary>公开证件条目数量，不复制证件事实。</summary>
        public int DocumentCount { get { return _documentCount; } }
        /// <summary>保留法律历史引用数量。</summary>
        public int PreservedLegalHistoryCount { get { return _preservedLegalHistoryCount; } }
        /// <summary>保留债务历史引用数量。</summary>
        public int PreservedDebtHistoryCount { get { return _preservedDebtHistoryCount; } }
        /// <summary>保留声望历史引用数量。</summary>
        public int PreservedReputationHistoryCount { get { return _preservedReputationHistoryCount; } }
        /// <summary>保留关系历史引用数量。</summary>
        public int PreservedRelationHistoryCount { get { return _preservedRelationHistoryCount; } }
        /// <summary>是否有本地化回退。</summary>
        public bool HasLocalizationFallback { get { return _hasLocalizationFallback; } }
        /// <summary>是否对敏感历史默认脱敏。</summary>
        public bool SensitiveHistoryRedacted { get { return _sensitiveHistoryRedacted; } }

        /// <summary>返回深拷贝。</summary>
        public D20IdentityPresentationModel Clone()
        {
            return new D20IdentityPresentationModel(
                _schemaVersion, _playerId, _displayName, _genderExpressionKey, _ageBandKey,
                _cultureOriginKey, _language, _title, _identityNote, _pronounKeys,
                _identityHistory, _documentCount, _preservedLegalHistoryCount,
                _preservedDebtHistoryCount, _preservedReputationHistoryCount,
                _preservedRelationHistoryCount, _hasLocalizationFallback,
                _sensitiveHistoryRedacted);
        }

        private static ReadOnlyCollection<string> ToReadOnly(IEnumerable<string> values)
        {
            var copy = new List<string>();
            if (values != null)
                foreach (var value in values)
                    if (value != null) copy.Add(value);
            return new ReadOnlyCollection<string>(copy);
        }
    }

    /// <summary>L10 映射结果。</summary>
    [Serializable]
    public sealed class D20IdentityPresentationResult
    {
        /// <summary>是否生成了有效读模型。</summary>
        public bool Valid;

        /// <summary>稳定结果码。</summary>
        public D20IdentityPresentationCode Code;

        /// <summary>机器可读原因码；成功且无回退时为空。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>只读身份表现模型。</summary>
        public D20IdentityPresentationModel Model;

        /// <summary>返回深拷贝。</summary>
        public D20IdentityPresentationResult Clone()
        {
            return new D20IdentityPresentationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
                Model = Model == null ? null : Model.Clone(),
            };
        }
    }

    /// <summary>D20-L10 身份表现只读映射规则。</summary>
    public static class D20IdentityPresentationRules
    {
        /// <summary>当前规则版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>规则 owner 标识；不等于 D10 身份事实 owner。</summary>
        public const string OwnerSystem = "D20IdentityPresentationRules";

        /// <summary>默认敏感历史脱敏 key。</summary>
        public const string SensitiveHistoryRedactionKey = "identity.history.redacted";

        /// <summary>校验 D10 快照并生成只读表现映射。</summary>
        public static D20IdentityPresentationResult Build(
            D20IdentityPresentationRequest request)
        {
            if (request == null)
                return Failure(D20IdentityPresentationCode.MissingRequest,
                    "identity_presentation_request_missing");
            if (request.SchemaVersion != D20IdentityPresentationRequest.CurrentSchemaVersion)
                return Failure(D20IdentityPresentationCode.InvalidSchemaVersion,
                    "schema_version_mismatch");
            D20IdentityPresentationLanguage language;
            if (!TryParseLanguage(request.Language, out language))
                return Failure(D20IdentityPresentationCode.UnsupportedLanguage,
                    "identity_language_unsupported");
            if (request.Identity == null)
                return Failure(D20IdentityPresentationCode.MissingIdentity,
                    "identity_snapshot_missing");

            D10PlayerIdentityState identity = request.Identity.Clone();
            if (!D20CharacterIdentityContracts.IsStableCharacterId(identity.PlayerId))
                return Failure(D20IdentityPresentationCode.MissingPlayerId, "missing_player_id");
            try
            {
                identity.Validate();
            }
            catch (Exception)
            {
                return Failure(D20IdentityPresentationCode.InvalidLocalizedText,
                    "identity_snapshot_invalid");
            }
            if (string.IsNullOrWhiteSpace(identity.Name) ||
                identity.Name.Length > D10IdentityLimits.NameMaxLength)
                return Failure(D20IdentityPresentationCode.InvalidName, "identity_name_invalid");
            if (!IsKey(identity.GenderExpressionKey, "gender_expression_"))
                return Failure(D20IdentityPresentationCode.InvalidIdentityKey,
                    "identity_gender_expression_key_invalid");
            if (!IsKey(identity.AgeBandKey, "age_band_"))
                return Failure(D20IdentityPresentationCode.InvalidIdentityKey,
                    "identity_age_band_key_invalid");
            if (!IsKey(identity.CultureOriginKey, "culture_origin_"))
                return Failure(D20IdentityPresentationCode.InvalidIdentityKey,
                    "identity_culture_origin_key_invalid");

            if (identity.PronounKeys != null)
            {
                foreach (var pronounKey in identity.PronounKeys)
                {
                    if (!IsKey(pronounKey, "pronoun_key_"))
                        return Failure(D20IdentityPresentationCode.InvalidPronounKey,
                            "identity_pronoun_key_invalid");
                }
            }

            bool localizationFallback;
            D20LocalizedPresentationText title;
            D20LocalizedPresentationText note;
            if (!TryMapText(identity.Title, language, "identity.title.unspecified",
                true, out title, out localizationFallback))
                return Failure(D20IdentityPresentationCode.InvalidLocalizedText,
                    "identity_title_invalid");
            bool noteFallback;
            if (!TryMapText(identity.IdentityNote, language, "identity.note.unspecified",
                false, out note, out noteFallback))
                return Failure(D20IdentityPresentationCode.InvalidLocalizedText,
                    "identity_note_invalid");
            localizationFallback = localizationFallback || noteFallback;

            var history = new List<D20IdentityHistoryPresentation>();
            bool redacted = false;
            if (identity.IdentityHistory != null)
            {
                foreach (var entry in identity.IdentityHistory)
                {
                    if (entry == null) continue;
                    D20IdentityHistoryPresentation mapped;
                    string historyReason;
                    if (!TryMapHistory(entry, language, request.RevealSensitiveHistory,
                        out mapped, out historyReason))
                        return Failure(D20IdentityPresentationCode.InvalidHistoryEntry,
                            historyReason);
                    history.Add(mapped);
                    redacted = redacted || mapped.IsRedacted;
                }
            }

            var model = new D20IdentityPresentationModel(
                CurrentSchemaVersion,
                identity.PlayerId,
                identity.Name.Trim(),
                identity.GenderExpressionKey,
                identity.AgeBandKey,
                identity.CultureOriginKey,
                language,
                title,
                note,
                identity.PronounKeys,
                history,
                identity.Documents == null ? 0 : identity.Documents.Count,
                identity.PreservedHistory == null || identity.PreservedHistory.LegalHistoryRefs == null
                    ? 0 : identity.PreservedHistory.LegalHistoryRefs.Count,
                identity.PreservedHistory == null || identity.PreservedHistory.DebtHistoryRefs == null
                    ? 0 : identity.PreservedHistory.DebtHistoryRefs.Count,
                identity.PreservedHistory == null || identity.PreservedHistory.ReputationHistoryRefs == null
                    ? 0 : identity.PreservedHistory.ReputationHistoryRefs.Count,
                identity.PreservedHistory == null || identity.PreservedHistory.RelationHistoryRefs == null
                    ? 0 : identity.PreservedHistory.RelationHistoryRefs.Count,
                localizationFallback,
                redacted);
            return new D20IdentityPresentationResult
            {
                Valid = true,
                Code = D20IdentityPresentationCode.None,
                ReasonCode = localizationFallback ? "identity_localization_fallback" : string.Empty,
                Model = model,
            };
        }

        /// <summary>从 D10 快照直接构建映射的便捷入口。</summary>
        public static D20IdentityPresentationResult Build(D10PlayerIdentityState identity,
            string language = "zh-CN", bool revealSensitiveHistory = false)
        {
            return Build(new D20IdentityPresentationRequest
            {
                Identity = identity,
                Language = language ?? string.Empty,
                RevealSensitiveHistory = revealSensitiveHistory,
            });
        }

        /// <summary>解析支持的语言键。</summary>
        public static bool TryParseLanguage(string value,
            out D20IdentityPresentationLanguage language)
        {
            if (string.Equals(value, "zh-CN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "zh_CN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "zh", StringComparison.OrdinalIgnoreCase))
            {
                language = D20IdentityPresentationLanguage.ZhCn;
                return true;
            }
            if (string.Equals(value, "en-US", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "en_US", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "en", StringComparison.OrdinalIgnoreCase))
            {
                language = D20IdentityPresentationLanguage.EnUs;
                return true;
            }
            language = D20IdentityPresentationLanguage.ZhCn;
            return false;
        }

        /// <summary>校验身份稳定 key 的前缀和值字符。</summary>
        public static bool IsKey(string value, string prefix)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix) ||
                !value.StartsWith(prefix, StringComparison.Ordinal) || value.Length > 160 ||
                value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }

        private static bool TryMapText(D10LocalizedText source,
            D20IdentityPresentationLanguage language, string fallbackKey,
            bool requireKey, out D20LocalizedPresentationText mapped,
            out bool usedFallback)
        {
            mapped = null;
            usedFallback = false;
            if (source == null)
            {
                if (requireKey) return false;
                mapped = new D20LocalizedPresentationText(fallbackKey, string.Empty,
                    language, true);
                usedFallback = true;
                return true;
            }
            if ((source.Key != null && source.Key.Length > D10IdentityLimits.KeyMaxLength) ||
                (source.Zh != null && source.Zh.Length > D10IdentityLimits.LongTextMaxLength) ||
                (source.En != null && source.En.Length > D10IdentityLimits.LongTextMaxLength))
                return false;
            string key = string.IsNullOrWhiteSpace(source.Key) ? fallbackKey : source.Key.Trim();
            if (!IsGenericStableKey(key)) return false;
            if (requireKey && string.IsNullOrWhiteSpace(source.Key)) usedFallback = true;
            string text;
            if (language == D20IdentityPresentationLanguage.EnUs &&
                !string.IsNullOrEmpty(source.En))
                text = source.En;
            else if (language == D20IdentityPresentationLanguage.ZhCn &&
                !string.IsNullOrEmpty(source.Zh))
                text = source.Zh;
            else if (!string.IsNullOrEmpty(source.Zh))
            {
                text = source.Zh;
                usedFallback = true;
            }
            else if (!string.IsNullOrEmpty(source.En))
            {
                text = source.En;
                usedFallback = true;
            }
            else
            {
                text = string.Empty;
                usedFallback = true;
            }
            mapped = new D20LocalizedPresentationText(key, text, language, usedFallback);
            return true;
        }

        private static bool TryMapHistory(D10IdentityHistoryEntry entry,
            D20IdentityPresentationLanguage language, bool revealSensitiveHistory,
            out D20IdentityHistoryPresentation mapped, out string reasonCode)
        {
            mapped = null;
            reasonCode = string.Empty;
            if (entry.WorldDay < 0 || !IsGenericStableKey(entry.HistoryId) ||
                !IsGenericStableKey(entry.ChangeKind) ||
                !IsGenericStableKey(entry.ChangeReasonKey))
            {
                reasonCode = "identity_history_key_invalid";
                return false;
            }
            if (!ValidateOptionalKey(entry.PreviousGenderExpressionKey) ||
                !ValidateOptionalKey(entry.NewGenderExpressionKey) ||
                !ValidateOptionalKey(entry.PreviousTitleKey) ||
                !ValidateOptionalKey(entry.NewTitleKey) ||
                !ValidateOptionalKey(entry.PreviousAgeBandKey) ||
                !ValidateOptionalKey(entry.NewAgeBandKey) ||
                !ValidateOptionalKey(entry.PreviousCultureOriginKey) ||
                !ValidateOptionalKey(entry.NewCultureOriginKey) ||
                !ValidateOptionalKey(entry.PreviousAppearanceLoadoutId) ||
                !ValidateOptionalKey(entry.NewAppearanceLoadoutId) ||
                !ValidatePronouns(entry.PreviousPronounKeys) ||
                !ValidatePronouns(entry.NewPronounKeys))
            {
                reasonCode = "identity_history_expression_invalid";
                return false;
            }
            D20LocalizedPresentationText note;
            bool noteFallback;
            if (!TryMapText(entry.Note, language, "identity.history.note.unspecified",
                false, out note, out noteFallback))
            {
                reasonCode = "identity_history_note_invalid";
                return false;
            }
            bool sensitive = HasSensitiveHistory(entry);
            if (sensitive && !revealSensitiveHistory)
            {
                mapped = new D20IdentityHistoryPresentation(
                    entry.HistoryId,
                    entry.WorldDay,
                    entry.ChangeKind,
                    entry.ChangeReasonKey,
                    true,
                    true,
                    SensitiveHistoryRedactionKey,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    null,
                    null,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    null);
                return true;
            }
            mapped = new D20IdentityHistoryPresentation(
                entry.HistoryId,
                entry.WorldDay,
                entry.ChangeKind,
                entry.ChangeReasonKey,
                sensitive,
                false,
                string.Empty,
                entry.PreviousName,
                entry.NewName,
                entry.PreviousGenderExpressionKey,
                entry.NewGenderExpressionKey,
                entry.PreviousPronounKeys,
                entry.NewPronounKeys,
                entry.PreviousTitleKey,
                entry.NewTitleKey,
                entry.PreviousAgeBandKey,
                entry.NewAgeBandKey,
                entry.PreviousCultureOriginKey,
                entry.NewCultureOriginKey,
                entry.PreviousAppearanceLoadoutId,
                entry.NewAppearanceLoadoutId,
                note);
            return true;
        }

        private static bool HasSensitiveHistory(D10IdentityHistoryEntry entry)
        {
            return !string.IsNullOrEmpty(entry.PreviousName) ||
                !string.IsNullOrEmpty(entry.NewName) ||
                !string.IsNullOrEmpty(entry.PreviousGenderExpressionKey) ||
                !string.IsNullOrEmpty(entry.NewGenderExpressionKey) ||
                !string.IsNullOrEmpty(entry.PreviousTitleKey) ||
                !string.IsNullOrEmpty(entry.NewTitleKey) ||
                !string.IsNullOrEmpty(entry.PreviousAgeBandKey) ||
                !string.IsNullOrEmpty(entry.NewAgeBandKey) ||
                !string.IsNullOrEmpty(entry.PreviousCultureOriginKey) ||
                !string.IsNullOrEmpty(entry.NewCultureOriginKey) ||
                (entry.PreviousPronounKeys != null && entry.PreviousPronounKeys.Count > 0) ||
                (entry.NewPronounKeys != null && entry.NewPronounKeys.Count > 0) ||
                !string.IsNullOrEmpty(entry.PreviousAppearanceLoadoutId) ||
                !string.IsNullOrEmpty(entry.NewAppearanceLoadoutId) ||
                entry.Note != null && (!string.IsNullOrEmpty(entry.Note.Zh) ||
                    !string.IsNullOrEmpty(entry.Note.En));
        }

        private static bool ValidatePronouns(IEnumerable<string> values)
        {
            if (values == null) return true;
            foreach (var value in values)
                if (!IsKey(value, "pronoun_key_")) return false;
            return true;
        }

        private static bool ValidateOptionalKey(string value)
        {
            return string.IsNullOrEmpty(value) || IsGenericStableKey(value);
        }

        private static bool IsGenericStableKey(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 256 || value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == '_' || c == '-' || c == '.' || c == ':';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }

        private static D20IdentityPresentationResult Failure(
            D20IdentityPresentationCode code, string reasonCode)
        {
            return new D20IdentityPresentationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
                Model = null,
            };
        }

        private static ReadOnlyCollection<string> ToReadOnly(IEnumerable<string> values)
        {
            var copy = new List<string>();
            if (values != null)
                foreach (var value in values)
                    if (value != null) copy.Add(value);
            return new ReadOnlyCollection<string>(copy);
        }
    }
}
