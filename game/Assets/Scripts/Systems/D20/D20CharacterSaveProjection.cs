// ============================================================
// D20CharacterSaveProjection —— D20-L26 角色保存边界（纯 C#）
//
// 本文件只组合 D20 自有的身份语义、玩家 presence/action、NPC 行为、
// 动物行为/身份和宠物所有权投影。D10/D11/D19 的事实只以公开 owner
// 快照引用保存；SaveSystem 仍是存档根的唯一组合/写入者。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.D20
{
    internal static class D20CharacterSaveValidation
    {
        internal const int ReferenceMaxLength = D20CharacterIdentityContracts.ReferenceMaxLength;
        internal const int TextMaxLength = 2048;

        internal static bool IsStableReference(string value, bool allowEmpty)
        {
            if (string.IsNullOrEmpty(value)) return allowEmpty;
            if (value.Length > ReferenceMaxLength || value.Trim() != value)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.' || c == '/';
                if (!asciiLetter && !digit && !separator) return false;
            }

            return true;
        }

        internal static void ValidateReference(string value, string fieldName, bool allowEmpty)
        {
            if (!IsStableReference(value, allowEmpty))
                throw new InvalidOperationException("D20 save " + fieldName + " is not a stable reference.");
        }

        internal static void ValidateText(string value, string fieldName, bool allowEmpty)
        {
            string safe = value ?? string.Empty;
            if (!allowEmpty && string.IsNullOrEmpty(safe))
                throw new InvalidOperationException("D20 save " + fieldName + " is required.");
            if (safe.Length > TextMaxLength)
                throw new InvalidOperationException("D20 save " + fieldName + " is too long.");
            for (int i = 0; i < safe.Length; i++)
            {
                if (char.IsControl(safe[i]))
                    throw new InvalidOperationException("D20 save " + fieldName + " contains a control character.");
            }
        }

        internal static void ValidateStringList(
            IEnumerable<string> values, string fieldName, bool allowEmptyItems)
        {
            if (values == null)
                throw new InvalidOperationException("D20 save " + fieldName + " is required.");

            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
            {
                if (!IsStableReference(value, allowEmptyItems) || !unique.Add(value ?? string.Empty))
                    throw new InvalidOperationException("D20 save " + fieldName + " contains an invalid or duplicate reference.");
            }
        }

        internal static string Invariant(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// 对 D10/D11/D19 等既有 owner 公开快照的稳定引用。
    /// 这里只保存 owner、快照 ID、Schema 和修订，不复制 owner 事实。
    /// </summary>
    [Serializable]
    public sealed class D20SaveOwnerSnapshotReference
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string OwnerSystem = string.Empty;
        public string SnapshotId = string.Empty;
        public int SnapshotSchemaVersion;
        public long Revision;
        public string LastEventId = string.Empty;
        public List<string> CauseEventIds = new List<string>();

        public string ReferenceKey
        {
            get
            {
                return (OwnerSystem ?? string.Empty) + "|" + (SnapshotId ?? string.Empty) + "|" +
                    SnapshotSchemaVersion.ToString(CultureInfo.InvariantCulture) + "|" +
                    Revision.ToString(CultureInfo.InvariantCulture);
            }
        }

        public D20SaveOwnerSnapshotReference Clone()
        {
            var result = new D20SaveOwnerSnapshotReference
            {
                SchemaVersion = SchemaVersion,
                OwnerSystem = OwnerSystem ?? string.Empty,
                SnapshotId = SnapshotId ?? string.Empty,
                SnapshotSchemaVersion = SnapshotSchemaVersion,
                Revision = Revision,
                LastEventId = LastEventId ?? string.Empty,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 owner reference schema_version mismatch.");
            D20CharacterSaveValidation.ValidateReference(OwnerSystem, "owner_system", false);
            D20CharacterSaveValidation.ValidateReference(SnapshotId, "snapshot_id", false);
            if (SnapshotSchemaVersion <= 0 || Revision < 0)
                throw new InvalidOperationException("D20 owner reference schema/revision is invalid.");
            D20CharacterSaveValidation.ValidateReference(LastEventId, "owner_last_event_id", true);
            D20CharacterSaveValidation.ValidateStringList(CauseEventIds, "owner_cause_event_ids", false);
        }
    }

    /// <summary>D20 事件游标和已应用事件键；事件事实仍由对应 owner 产生。</summary>
    [Serializable]
    public sealed class D20CharacterSaveEventCursor
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string LastEventId = string.Empty;
        public long LastEventSequence;
        public long LastEventSaveRevision;
        public List<string> AppliedEventKeys = new List<string>();
        public List<string> CauseEventIds = new List<string>();

        public D20CharacterSaveEventCursor Clone()
        {
            var result = new D20CharacterSaveEventCursor
            {
                SchemaVersion = SchemaVersion,
                LastEventId = LastEventId ?? string.Empty,
                LastEventSequence = LastEventSequence,
                LastEventSaveRevision = LastEventSaveRevision,
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || LastEventSequence < 0 ||
                LastEventSaveRevision < 0)
                throw new InvalidOperationException("D20 save event cursor is outside its supported range.");
            D20CharacterSaveValidation.ValidateReference(LastEventId, "last_event_id", true);
            D20CharacterSaveValidation.ValidateStringList(AppliedEventKeys, "applied_event_keys", false);
            D20CharacterSaveValidation.ValidateStringList(CauseEventIds, "event_cause_event_ids", false);
            if (string.IsNullOrEmpty(LastEventId) && LastEventSequence != 0)
                throw new InvalidOperationException("D20 empty event cursor cannot carry a sequence.");
            if (LastEventSequence == 0 && LastEventSaveRevision != 0)
                throw new InvalidOperationException("D20 empty event cursor cannot carry a save revision.");
        }
    }

    /// <summary>身份表现模型中的本地化文本保存副本；它是可重建的只读投影。</summary>
    [Serializable]
    public sealed class D20IdentityPresentationTextSaveProjection
    {
        public string Key = string.Empty;
        public string Text = string.Empty;
        public D20IdentityPresentationLanguage Language = D20IdentityPresentationLanguage.ZhCn;
        public bool UsedFallback;

        public static D20IdentityPresentationTextSaveProjection FromModel(
            D20LocalizedPresentationText source)
        {
            if (source == null) return null;
            return new D20IdentityPresentationTextSaveProjection
            {
                Key = source.Key ?? string.Empty,
                Text = source.Text ?? string.Empty,
                Language = source.Language,
                UsedFallback = source.UsedFallback,
            };
        }

        public D20IdentityPresentationTextSaveProjection Clone()
        {
            return new D20IdentityPresentationTextSaveProjection
            {
                Key = Key ?? string.Empty,
                Text = Text ?? string.Empty,
                Language = Language,
                UsedFallback = UsedFallback,
            };
        }

        public void Validate(string fieldName)
        {
            if (!Enum.IsDefined(typeof(D20IdentityPresentationLanguage), Language))
                throw new InvalidOperationException("D20 save " + fieldName + " language is invalid.");
            D20CharacterSaveValidation.ValidateReference(Key, fieldName + ".key", true);
            D20CharacterSaveValidation.ValidateText(Text, fieldName + ".text", true);
        }
    }

    /// <summary>身份历史安全表现的可保存只读投影。</summary>
    [Serializable]
    public sealed class D20IdentityHistorySaveProjection
    {
        public string HistoryId = string.Empty;
        public int WorldDay;
        public string ChangeKind = string.Empty;
        public string ChangeReasonKey = string.Empty;
        public bool IsSensitive;
        public bool IsRedacted;
        public string RedactionKey = string.Empty;
        public string PreviousName = string.Empty;
        public string NewName = string.Empty;
        public string PreviousGenderExpressionKey = string.Empty;
        public string NewGenderExpressionKey = string.Empty;
        public List<string> PreviousPronounKeys = new List<string>();
        public List<string> NewPronounKeys = new List<string>();
        public string PreviousTitleKey = string.Empty;
        public string NewTitleKey = string.Empty;
        public string PreviousAgeBandKey = string.Empty;
        public string NewAgeBandKey = string.Empty;
        public string PreviousCultureOriginKey = string.Empty;
        public string NewCultureOriginKey = string.Empty;
        public string PreviousAppearanceLoadoutId = string.Empty;
        public string NewAppearanceLoadoutId = string.Empty;
        public D20IdentityPresentationTextSaveProjection Note;

        public static D20IdentityHistorySaveProjection FromModel(
            D20IdentityHistoryPresentation source)
        {
            if (source == null) return null;
            var result = new D20IdentityHistorySaveProjection
            {
                HistoryId = source.HistoryId ?? string.Empty,
                WorldDay = source.WorldDay,
                ChangeKind = source.ChangeKind ?? string.Empty,
                ChangeReasonKey = source.ChangeReasonKey ?? string.Empty,
                IsSensitive = source.IsSensitive,
                IsRedacted = source.IsRedacted,
                RedactionKey = source.RedactionKey ?? string.Empty,
                PreviousName = source.PreviousName ?? string.Empty,
                NewName = source.NewName ?? string.Empty,
                PreviousGenderExpressionKey = source.PreviousGenderExpressionKey ?? string.Empty,
                NewGenderExpressionKey = source.NewGenderExpressionKey ?? string.Empty,
                PreviousTitleKey = source.PreviousTitleKey ?? string.Empty,
                NewTitleKey = source.NewTitleKey ?? string.Empty,
                PreviousAgeBandKey = source.PreviousAgeBandKey ?? string.Empty,
                NewAgeBandKey = source.NewAgeBandKey ?? string.Empty,
                PreviousCultureOriginKey = source.PreviousCultureOriginKey ?? string.Empty,
                NewCultureOriginKey = source.NewCultureOriginKey ?? string.Empty,
                PreviousAppearanceLoadoutId = source.PreviousAppearanceLoadoutId ?? string.Empty,
                NewAppearanceLoadoutId = source.NewAppearanceLoadoutId ?? string.Empty,
                Note = D20IdentityPresentationTextSaveProjection.FromModel(source.Note),
            };
            if (source.PreviousPronounKeys != null)
                result.PreviousPronounKeys.AddRange(source.PreviousPronounKeys);
            if (source.NewPronounKeys != null)
                result.NewPronounKeys.AddRange(source.NewPronounKeys);
            return result;
        }

        public D20IdentityHistorySaveProjection Clone()
        {
            var result = new D20IdentityHistorySaveProjection
            {
                HistoryId = HistoryId ?? string.Empty,
                WorldDay = WorldDay,
                ChangeKind = ChangeKind ?? string.Empty,
                ChangeReasonKey = ChangeReasonKey ?? string.Empty,
                IsSensitive = IsSensitive,
                IsRedacted = IsRedacted,
                RedactionKey = RedactionKey ?? string.Empty,
                PreviousName = PreviousName ?? string.Empty,
                NewName = NewName ?? string.Empty,
                PreviousGenderExpressionKey = PreviousGenderExpressionKey ?? string.Empty,
                NewGenderExpressionKey = NewGenderExpressionKey ?? string.Empty,
                PreviousTitleKey = PreviousTitleKey ?? string.Empty,
                NewTitleKey = NewTitleKey ?? string.Empty,
                PreviousAgeBandKey = PreviousAgeBandKey ?? string.Empty,
                NewAgeBandKey = NewAgeBandKey ?? string.Empty,
                PreviousCultureOriginKey = PreviousCultureOriginKey ?? string.Empty,
                NewCultureOriginKey = NewCultureOriginKey ?? string.Empty,
                PreviousAppearanceLoadoutId = PreviousAppearanceLoadoutId ?? string.Empty,
                NewAppearanceLoadoutId = NewAppearanceLoadoutId ?? string.Empty,
                Note = Note == null ? null : Note.Clone(),
            };
            if (PreviousPronounKeys != null) result.PreviousPronounKeys.AddRange(PreviousPronounKeys);
            if (NewPronounKeys != null) result.NewPronounKeys.AddRange(NewPronounKeys);
            return result;
        }

        public void Validate()
        {
            D20CharacterSaveValidation.ValidateReference(HistoryId, "identity_history.history_id", false);
            if (WorldDay < 0)
                throw new InvalidOperationException("D20 identity history world_day is invalid.");
            D20CharacterSaveValidation.ValidateReference(ChangeKind, "identity_history.change_kind", true);
            D20CharacterSaveValidation.ValidateReference(ChangeReasonKey, "identity_history.change_reason_key", true);
            D20CharacterSaveValidation.ValidateReference(RedactionKey, "identity_history.redaction_key", true);
            D20CharacterSaveValidation.ValidateText(PreviousName, "identity_history.previous_name", true);
            D20CharacterSaveValidation.ValidateText(NewName, "identity_history.new_name", true);
            D20CharacterSaveValidation.ValidateReference(
                PreviousGenderExpressionKey, "identity_history.previous_gender_expression_key", true);
            D20CharacterSaveValidation.ValidateReference(
                NewGenderExpressionKey, "identity_history.new_gender_expression_key", true);
            D20CharacterSaveValidation.ValidateStringList(
                PreviousPronounKeys, "identity_history.previous_pronoun_keys", false);
            D20CharacterSaveValidation.ValidateStringList(
                NewPronounKeys, "identity_history.new_pronoun_keys", false);
            D20CharacterSaveValidation.ValidateReference(PreviousTitleKey, "identity_history.previous_title_key", true);
            D20CharacterSaveValidation.ValidateReference(NewTitleKey, "identity_history.new_title_key", true);
            D20CharacterSaveValidation.ValidateReference(PreviousAgeBandKey, "identity_history.previous_age_band_key", true);
            D20CharacterSaveValidation.ValidateReference(NewAgeBandKey, "identity_history.new_age_band_key", true);
            D20CharacterSaveValidation.ValidateReference(
                PreviousCultureOriginKey, "identity_history.previous_culture_origin_key", true);
            D20CharacterSaveValidation.ValidateReference(
                NewCultureOriginKey, "identity_history.new_culture_origin_key", true);
            D20CharacterSaveValidation.ValidateReference(
                PreviousAppearanceLoadoutId, "identity_history.previous_appearance_loadout_id", true);
            D20CharacterSaveValidation.ValidateReference(
                NewAppearanceLoadoutId, "identity_history.new_appearance_loadout_id", true);
            if (Note != null) Note.Validate("identity_history.note");
            if (IsRedacted && string.IsNullOrEmpty(RedactionKey))
                throw new InvalidOperationException("D20 redacted identity history requires a redaction key.");
        }
    }

    /// <summary>
    /// L10 身份表现的保存投影。姓名/文案来自已生成的只读表现模型，
    /// 不成为 D10 身份、法律或关系事实的第二写者。
    /// </summary>
    [Serializable]
    public sealed class D20IdentityPresentationSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string PlayerId = string.Empty;
        public string DisplayName = string.Empty;
        public string GenderExpressionKey = string.Empty;
        public string AgeBandKey = string.Empty;
        public string CultureOriginKey = string.Empty;
        public D20IdentityPresentationLanguage Language = D20IdentityPresentationLanguage.ZhCn;
        public D20IdentityPresentationTextSaveProjection Title;
        public D20IdentityPresentationTextSaveProjection IdentityNote;
        public List<string> PronounKeys = new List<string>();
        public List<D20IdentityHistorySaveProjection> IdentityHistory =
            new List<D20IdentityHistorySaveProjection>();
        public int DocumentCount;
        public int PreservedLegalHistoryCount;
        public int PreservedDebtHistoryCount;
        public int PreservedReputationHistoryCount;
        public int PreservedRelationHistoryCount;
        public bool HasLocalizationFallback;
        public bool SensitiveHistoryRedacted;

        public static D20IdentityPresentationSaveProjection FromModel(
            D20IdentityPresentationModel source)
        {
            if (source == null) return null;
            var result = new D20IdentityPresentationSaveProjection
            {
                SchemaVersion = source.SchemaVersion,
                PlayerId = source.PlayerId ?? string.Empty,
                DisplayName = source.DisplayName ?? string.Empty,
                GenderExpressionKey = source.GenderExpressionKey ?? string.Empty,
                AgeBandKey = source.AgeBandKey ?? string.Empty,
                CultureOriginKey = source.CultureOriginKey ?? string.Empty,
                Language = source.Language,
                Title = D20IdentityPresentationTextSaveProjection.FromModel(source.Title),
                IdentityNote = D20IdentityPresentationTextSaveProjection.FromModel(source.IdentityNote),
                DocumentCount = source.DocumentCount,
                PreservedLegalHistoryCount = source.PreservedLegalHistoryCount,
                PreservedDebtHistoryCount = source.PreservedDebtHistoryCount,
                PreservedReputationHistoryCount = source.PreservedReputationHistoryCount,
                PreservedRelationHistoryCount = source.PreservedRelationHistoryCount,
                HasLocalizationFallback = source.HasLocalizationFallback,
                SensitiveHistoryRedacted = source.SensitiveHistoryRedacted,
            };
            if (source.PronounKeys != null) result.PronounKeys.AddRange(source.PronounKeys);
            if (source.IdentityHistory != null)
            {
                foreach (D20IdentityHistoryPresentation entry in source.IdentityHistory)
                {
                    if (entry != null) result.IdentityHistory.Add(D20IdentityHistorySaveProjection.FromModel(entry));
                }
            }
            return result;
        }

        public D20IdentityPresentationSaveProjection Clone()
        {
            var result = new D20IdentityPresentationSaveProjection
            {
                SchemaVersion = SchemaVersion,
                PlayerId = PlayerId ?? string.Empty,
                DisplayName = DisplayName ?? string.Empty,
                GenderExpressionKey = GenderExpressionKey ?? string.Empty,
                AgeBandKey = AgeBandKey ?? string.Empty,
                CultureOriginKey = CultureOriginKey ?? string.Empty,
                Language = Language,
                Title = Title == null ? null : Title.Clone(),
                IdentityNote = IdentityNote == null ? null : IdentityNote.Clone(),
                DocumentCount = DocumentCount,
                PreservedLegalHistoryCount = PreservedLegalHistoryCount,
                PreservedDebtHistoryCount = PreservedDebtHistoryCount,
                PreservedReputationHistoryCount = PreservedReputationHistoryCount,
                PreservedRelationHistoryCount = PreservedRelationHistoryCount,
                HasLocalizationFallback = HasLocalizationFallback,
                SensitiveHistoryRedacted = SensitiveHistoryRedacted,
            };
            if (PronounKeys != null) result.PronounKeys.AddRange(PronounKeys);
            if (IdentityHistory != null)
            {
                foreach (D20IdentityHistorySaveProjection entry in IdentityHistory)
                    if (entry != null) result.IdentityHistory.Add(entry.Clone());
            }
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 identity presentation schema_version mismatch.");
            D20CharacterSaveValidation.ValidateReference(PlayerId, "identity_presentation.player_id", false);
            D20CharacterSaveValidation.ValidateText(DisplayName, "identity_presentation.display_name", true);
            D20CharacterSaveValidation.ValidateReference(
                GenderExpressionKey, "identity_presentation.gender_expression_key", true);
            D20CharacterSaveValidation.ValidateReference(
                AgeBandKey, "identity_presentation.age_band_key", true);
            D20CharacterSaveValidation.ValidateReference(
                CultureOriginKey, "identity_presentation.culture_origin_key", true);
            if (!Enum.IsDefined(typeof(D20IdentityPresentationLanguage), Language))
                throw new InvalidOperationException("D20 identity presentation language is invalid.");
            if (Title != null) Title.Validate("identity_presentation.title");
            if (IdentityNote != null) IdentityNote.Validate("identity_presentation.identity_note");
            D20CharacterSaveValidation.ValidateStringList(
                PronounKeys, "identity_presentation.pronoun_keys", false);
            if (IdentityHistory == null)
                throw new InvalidOperationException("D20 identity presentation history is required.");
            var historyIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20IdentityHistorySaveProjection entry in IdentityHistory)
            {
                if (entry == null || !historyIds.Add(entry.HistoryId ?? string.Empty))
                    throw new InvalidOperationException("D20 identity presentation history contains a duplicate or null entry.");
                entry.Validate();
            }
            if (DocumentCount < 0 || PreservedLegalHistoryCount < 0 ||
                PreservedDebtHistoryCount < 0 || PreservedReputationHistoryCount < 0 ||
                PreservedRelationHistoryCount < 0)
                throw new InvalidOperationException("D20 identity presentation counters cannot be negative.");
        }
    }

    /// <summary>
    /// NPC 行为投影的保存包装。L17 当前快照没有独立回执表，
    /// 因此命令/事件字段可由上层编排器提供；未提供时保持为空，不伪造事件。
    /// </summary>
    [Serializable]
    public sealed class D20NpcBehaviorSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D20NpcBehaviorSnapshot Behavior;
        public string LastCommandId = string.Empty;
        public string LastEventId = string.Empty;
        public List<string> CauseEventIds = new List<string>();

        public static D20NpcBehaviorSaveProjection FromSnapshot(
            D20NpcBehaviorSnapshot source,
            string lastCommandId = "",
            string lastEventId = "",
            IEnumerable<string> causeEventIds = null)
        {
            if (source == null) return null;
            var result = new D20NpcBehaviorSaveProjection
            {
                Behavior = source.Clone(),
                LastCommandId = lastCommandId ?? string.Empty,
                LastEventId = lastEventId ?? string.Empty,
            };
            if (causeEventIds != null) result.CauseEventIds.AddRange(causeEventIds);
            return result;
        }

        public D20NpcBehaviorSaveProjection Clone()
        {
            var result = new D20NpcBehaviorSaveProjection
            {
                SchemaVersion = SchemaVersion,
                Behavior = Behavior == null ? null : Behavior.Clone(),
                LastCommandId = LastCommandId ?? string.Empty,
                LastEventId = LastEventId ?? string.Empty,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Behavior == null)
                throw new InvalidOperationException("D20 NPC behavior save projection is incomplete.");
            Behavior.Validate();
            D20CharacterSaveValidation.ValidateReference(LastCommandId, "npc_behavior.last_command_id", true);
            D20CharacterSaveValidation.ValidateReference(LastEventId, "npc_behavior.last_event_id", true);
            D20CharacterSaveValidation.ValidateStringList(
                CauseEventIds, "npc_behavior.cause_event_ids", false);
        }
    }

    /// <summary>D20 角色/动物存档的纯投影；不替换 SaveSystem 根模型。</summary>
    [Serializable]
    public sealed class D20CharacterSaveProjection
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string PlayerId = string.Empty;
        public int WorldDay;
        public long SaveRevision;

        public D20CharacterIdentity PlayerIdentity;
        public D20PlayerPresenceSnapshot PlayerPresence;
        public D20ActionInterruptSnapshot PlayerAction;
        public D20IdentityPresentationSaveProjection PlayerIdentityPresentation;

        public List<D20NpcIdentity> NpcIdentities = new List<D20NpcIdentity>();
        public List<D20NpcBehaviorSaveProjection> NpcBehaviors =
            new List<D20NpcBehaviorSaveProjection>();
        public List<D20AnimalIdentity> AnimalIdentities = new List<D20AnimalIdentity>();
        public List<D20AnimalBehaviorSnapshot> AnimalBehaviors =
            new List<D20AnimalBehaviorSnapshot>();
        public D20PetOwnershipSnapshot PetOwnership = new D20PetOwnershipSnapshot();

        public D20CharacterSaveEventCursor Events = new D20CharacterSaveEventCursor();
        public List<D20SaveOwnerSnapshotReference> UpstreamSnapshots =
            new List<D20SaveOwnerSnapshotReference>();
        public List<string> CauseEventIds = new List<string>();

        /// <summary>兼容下游命名的身份别名；不产生第二个序列化字段。</summary>
        public D20CharacterIdentity Identity
        {
            get { return PlayerIdentity; }
            set { PlayerIdentity = value; }
        }

        /// <summary>兼容下游命名的 presence 别名；不产生第二个序列化字段。</summary>
        public D20PlayerPresenceSnapshot Presence
        {
            get { return PlayerPresence; }
            set { PlayerPresence = value; }
        }

        /// <summary>兼容下游命名的动作别名；不产生第二个序列化字段。</summary>
        public D20ActionInterruptSnapshot Action
        {
            get { return PlayerAction; }
            set { PlayerAction = value; }
        }

        /// <summary>创建不带角色事实的安全空投影。</summary>
        public static D20CharacterSaveProjection CreateDefault(string playerId = "")
        {
            return new D20CharacterSaveProjection
            {
                SchemaVersion = CurrentSchemaVersion,
                PlayerId = playerId ?? string.Empty,
                WorldDay = 0,
                SaveRevision = 0,
                PlayerIdentity = null,
                PlayerPresence = null,
                PlayerAction = null,
                PlayerIdentityPresentation = null,
                NpcIdentities = new List<D20NpcIdentity>(),
                NpcBehaviors = new List<D20NpcBehaviorSaveProjection>(),
                AnimalIdentities = new List<D20AnimalIdentity>(),
                AnimalBehaviors = new List<D20AnimalBehaviorSnapshot>(),
                PetOwnership = new D20PetOwnershipSnapshot(),
                Events = new D20CharacterSaveEventCursor(),
                UpstreamSnapshots = new List<D20SaveOwnerSnapshotReference>(),
                CauseEventIds = new List<string>(),
            };
        }

        /// <summary>返回包含所有嵌套投影副本的保存快照。</summary>
        public D20CharacterSaveProjection Clone()
        {
            var result = new D20CharacterSaveProjection
            {
                SchemaVersion = SchemaVersion,
                PlayerId = PlayerId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                PlayerIdentity = PlayerIdentity == null ? null : PlayerIdentity.Clone(),
                PlayerPresence = PlayerPresence == null ? null : PlayerPresence.Clone(),
                PlayerAction = PlayerAction == null ? null : PlayerAction.Clone(),
                PlayerIdentityPresentation = PlayerIdentityPresentation == null
                    ? null : PlayerIdentityPresentation.Clone(),
                PetOwnership = PetOwnership == null ? null : PetOwnership.Clone(),
                Events = Events == null ? null : Events.Clone(),
                NpcIdentities = new List<D20NpcIdentity>(),
                NpcBehaviors = new List<D20NpcBehaviorSaveProjection>(),
                AnimalIdentities = new List<D20AnimalIdentity>(),
                AnimalBehaviors = new List<D20AnimalBehaviorSnapshot>(),
                UpstreamSnapshots = new List<D20SaveOwnerSnapshotReference>(),
                CauseEventIds = new List<string>(),
            };
            if (NpcIdentities != null)
                foreach (D20NpcIdentity identity in NpcIdentities)
                    if (identity != null) result.NpcIdentities.Add(identity.Clone());
            if (NpcBehaviors != null)
                foreach (D20NpcBehaviorSaveProjection behavior in NpcBehaviors)
                    if (behavior != null) result.NpcBehaviors.Add(behavior.Clone());
            if (AnimalIdentities != null)
                foreach (D20AnimalIdentity identity in AnimalIdentities)
                    if (identity != null) result.AnimalIdentities.Add(identity.Clone());
            if (AnimalBehaviors != null)
                foreach (D20AnimalBehaviorSnapshot behavior in AnimalBehaviors)
                    if (behavior != null) result.AnimalBehaviors.Add(behavior.Clone());
            if (UpstreamSnapshots != null)
                foreach (D20SaveOwnerSnapshotReference reference in UpstreamSnapshots)
                    if (reference != null) result.UpstreamSnapshots.Add(reference.Clone());
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        /// <summary>验证版本、唯一 ID、D20 自有事实边界和 owner 引用。</summary>
        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 character save schema_version mismatch.");
            D20CharacterSaveValidation.ValidateReference(PlayerId, "player_id", true);
            if (WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException("D20 character save world_day/save_revision is invalid.");

            bool hasPlayerProjection = PlayerIdentity != null || PlayerPresence != null ||
                PlayerAction != null || PlayerIdentityPresentation != null;
            if (hasPlayerProjection && string.IsNullOrEmpty(PlayerId))
                throw new InvalidOperationException("D20 player projections require player_id.");

            if (PlayerIdentity != null)
            {
                PlayerIdentity.Validate();
                if (PlayerIdentity.CharacterKind != D20CharacterKind.Player ||
                    !string.Equals(PlayerIdentity.CharacterId, PlayerId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 player identity does not match player_id.");
            }
            if (PlayerPresence != null)
            {
                PlayerPresence.Validate();
                if (!string.Equals(PlayerPresence.PlayerId, PlayerId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 player presence does not match player_id.");
            }
            if (PlayerAction != null)
            {
                PlayerAction.Validate();
                if (!string.Equals(PlayerAction.ActorId, PlayerId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 player action does not match player_id.");
            }
            if (PlayerIdentityPresentation != null)
            {
                PlayerIdentityPresentation.Validate();
                if (!string.Equals(PlayerIdentityPresentation.PlayerId, PlayerId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 identity presentation does not match player_id.");
            }

            ValidateNpcIdentities();
            ValidateNpcBehaviors();
            ValidateAnimalIdentities();
            ValidateAnimalBehaviors();

            if (PetOwnership == null)
                throw new InvalidOperationException("D20 pet ownership projection is required.");
            PetOwnership.Validate();
            if (Events == null)
                throw new InvalidOperationException("D20 character event cursor is required.");
            Events.Validate();
            D20CharacterSaveValidation.ValidateStringList(CauseEventIds, "cause_event_ids", false);
            ValidateUpstreamSnapshots();
        }

        private void ValidateNpcIdentities()
        {
            if (NpcIdentities == null)
                throw new InvalidOperationException("D20 NPC identities are required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20NpcIdentity identity in NpcIdentities)
            {
                if (identity == null) throw new InvalidOperationException("D20 NPC identity is required.");
                identity.Validate();
                if (!ids.Add(identity.NpcId))
                    throw new InvalidOperationException("D20 NPC identities contain a duplicate id: " + identity.NpcId);
            }
        }

        private void ValidateNpcBehaviors()
        {
            if (NpcBehaviors == null)
                throw new InvalidOperationException("D20 NPC behaviors are required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20NpcBehaviorSaveProjection behavior in NpcBehaviors)
            {
                if (behavior == null) throw new InvalidOperationException("D20 NPC behavior is required.");
                behavior.Validate();
                string id = behavior.Behavior.NpcId;
                if (!ids.Add(id))
                    throw new InvalidOperationException("D20 NPC behaviors contain a duplicate id: " + id);
            }
        }

        private void ValidateAnimalIdentities()
        {
            if (AnimalIdentities == null)
                throw new InvalidOperationException("D20 animal identities are required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20AnimalIdentity identity in AnimalIdentities)
            {
                if (identity == null) throw new InvalidOperationException("D20 animal identity is required.");
                identity.Validate();
                if (!ids.Add(identity.AnimalId))
                    throw new InvalidOperationException("D20 animal identities contain a duplicate id: " + identity.AnimalId);
            }
        }

        private void ValidateAnimalBehaviors()
        {
            if (AnimalBehaviors == null)
                throw new InvalidOperationException("D20 animal behaviors are required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20AnimalBehaviorSnapshot behavior in AnimalBehaviors)
            {
                if (behavior == null) throw new InvalidOperationException("D20 animal behavior is required.");
                behavior.Validate();
                if (!ids.Add(behavior.AnimalId))
                    throw new InvalidOperationException("D20 animal behaviors contain a duplicate id: " + behavior.AnimalId);
            }
        }

        private void ValidateUpstreamSnapshots()
        {
            if (UpstreamSnapshots == null)
                throw new InvalidOperationException("D20 upstream snapshot references are required.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (D20SaveOwnerSnapshotReference reference in UpstreamSnapshots)
            {
                if (reference == null) throw new InvalidOperationException("D20 upstream snapshot reference is required.");
                reference.Validate();
                if (!keys.Add(reference.ReferenceKey))
                    throw new InvalidOperationException("D20 upstream snapshot references contain a duplicate key.");
            }
        }
    }

    /// <summary>D20-L26 从已公开快照建立角色保存投影的纯映射器。</summary>
    public static class D20CharacterSaveProjectionMapper
    {
        /// <summary>
        /// 组合 D20 自有快照。NPC 行为快照只复制到 D20 包装；
        /// D10/D11/D19 事实通过 upstreamSnapshots 传入引用，不在此复制。
        /// </summary>
        public static D20CharacterSaveProjection BuildProjection(
            string playerId,
            int worldDay,
            long saveRevision,
            D20CharacterIdentity playerIdentity = null,
            D20PlayerPresenceSnapshot playerPresence = null,
            D20ActionInterruptSnapshot playerAction = null,
            D20IdentityPresentationModel playerIdentityPresentation = null,
            IEnumerable<D20NpcIdentity> npcIdentities = null,
            IEnumerable<D20NpcBehaviorSnapshot> npcBehaviors = null,
            IEnumerable<D20AnimalIdentity> animalIdentities = null,
            IEnumerable<D20AnimalBehaviorSnapshot> animalBehaviors = null,
            D20PetOwnershipSnapshot petOwnership = null,
            D20CharacterSaveEventCursor events = null,
            IEnumerable<D20SaveOwnerSnapshotReference> upstreamSnapshots = null,
            IEnumerable<string> causeEventIds = null)
        {
            var result = D20CharacterSaveProjection.CreateDefault(playerId);
            result.WorldDay = worldDay;
            result.SaveRevision = saveRevision;
            result.PlayerIdentity = playerIdentity == null ? null : playerIdentity.Clone();
            result.PlayerPresence = playerPresence == null ? null : playerPresence.Clone();
            result.PlayerAction = playerAction == null ? null : playerAction.Clone();
            result.PlayerIdentityPresentation = playerIdentityPresentation == null
                ? null : D20IdentityPresentationSaveProjection.FromModel(playerIdentityPresentation);
            result.PetOwnership = petOwnership == null ? new D20PetOwnershipSnapshot() : petOwnership.Clone();
            result.Events = events == null ? new D20CharacterSaveEventCursor() : events.Clone();
            if (npcIdentities != null)
                foreach (D20NpcIdentity identity in npcIdentities)
                    if (identity != null) result.NpcIdentities.Add(identity.Clone());
            if (npcBehaviors != null)
            {
                foreach (D20NpcBehaviorSnapshot behavior in npcBehaviors)
                {
                    if (behavior != null)
                        result.NpcBehaviors.Add(D20NpcBehaviorSaveProjection.FromSnapshot(behavior));
                }
            }
            if (animalIdentities != null)
                foreach (D20AnimalIdentity identity in animalIdentities)
                    if (identity != null) result.AnimalIdentities.Add(identity.Clone());
            if (animalBehaviors != null)
                foreach (D20AnimalBehaviorSnapshot behavior in animalBehaviors)
                    if (behavior != null) result.AnimalBehaviors.Add(behavior.Clone());
            if (upstreamSnapshots != null)
                foreach (D20SaveOwnerSnapshotReference reference in upstreamSnapshots)
                    if (reference != null) result.UpstreamSnapshots.Add(reference.Clone());
            if (causeEventIds != null) result.CauseEventIds.AddRange(causeEventIds);

            if (string.IsNullOrEmpty(result.PlayerId))
                result.PlayerId = ResolvePlayerId(result);
            result.Validate();
            return result;
        }

        /// <summary>兼容下游调用的简短别名。</summary>
        public static D20CharacterSaveProjection Build(
            string playerId,
            int worldDay,
            long saveRevision,
            D20CharacterIdentity playerIdentity = null,
            D20PlayerPresenceSnapshot playerPresence = null,
            D20ActionInterruptSnapshot playerAction = null,
            D20IdentityPresentationModel playerIdentityPresentation = null,
            IEnumerable<D20NpcIdentity> npcIdentities = null,
            IEnumerable<D20NpcBehaviorSnapshot> npcBehaviors = null,
            IEnumerable<D20AnimalIdentity> animalIdentities = null,
            IEnumerable<D20AnimalBehaviorSnapshot> animalBehaviors = null,
            D20PetOwnershipSnapshot petOwnership = null,
            D20CharacterSaveEventCursor events = null,
            IEnumerable<D20SaveOwnerSnapshotReference> upstreamSnapshots = null,
            IEnumerable<string> causeEventIds = null)
        {
            return BuildProjection(playerId, worldDay, saveRevision, playerIdentity,
                playerPresence, playerAction, playerIdentityPresentation, npcIdentities,
                npcBehaviors, animalIdentities, animalBehaviors, petOwnership, events,
                upstreamSnapshots, causeEventIds);
        }

        private static string ResolvePlayerId(D20CharacterSaveProjection projection)
        {
            string candidate = string.Empty;
            if (projection.PlayerIdentity != null) candidate = projection.PlayerIdentity.CharacterId;
            candidate = ResolveCandidate(candidate,
                projection.PlayerPresence == null ? string.Empty : projection.PlayerPresence.PlayerId);
            candidate = ResolveCandidate(candidate,
                projection.PlayerAction == null ? string.Empty : projection.PlayerAction.ActorId);
            candidate = ResolveCandidate(candidate,
                projection.PlayerIdentityPresentation == null ? string.Empty : projection.PlayerIdentityPresentation.PlayerId);
            return candidate;
        }

        private static string ResolveCandidate(string current, string next)
        {
            string safeNext = next ?? string.Empty;
            if (string.IsNullOrEmpty(safeNext)) return current ?? string.Empty;
            if (!D20CharacterIdentityContracts.IsStableCharacterId(safeNext))
                throw new InvalidOperationException("D20 player id cannot be derived from an unstable reference.");
            if (!string.IsNullOrEmpty(current) &&
                !string.Equals(current, safeNext, StringComparison.Ordinal))
                throw new InvalidOperationException("D20 player projection ids conflict.");
            return safeNext;
        }
    }
}
