// ============================================================
// D10PlayerIdentitySystem —— 玩家身份表达与港口身份服务
//
// 唯一写者：Character / Identity。该系统只写玩家身份表达、身份历史
// 与证件记录；债务、法律、声望、关系和外观实体仍由各自 owner 写入。
// 外观修改通过 ID10AppearanceWriter 委托给 Appearance/Character owner，
// 本系统只在身份历史中保存外观 loadout 的引用。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D10
{
    public static class D10IdentityLimits
    {
        public const int NameMaxLength = 120;
        public const int KeyMaxLength = 160;
        public const int LongTextMaxLength = 65535;
        public const int DocumentIdMaxLength = 160;
    }

    [Serializable]
    public sealed class D10LocalizedText
    {
        public string Key = "";
        public string Zh = "";
        public string En = "";

        public D10LocalizedText() { }

        public D10LocalizedText(string key, string zh, string en)
        {
            Key = key ?? "";
            Zh = zh ?? "";
            En = en ?? "";
        }

        public string Resolve(string language)
        {
            if (language == "en" && !string.IsNullOrEmpty(En)) return En;
            if (!string.IsNullOrEmpty(Zh)) return Zh;
            return En ?? Key ?? "";
        }

        public D10LocalizedText Clone()
        {
            return new D10LocalizedText(Key, Zh, En);
        }

        public void Validate(string fieldName, bool requireKey = false)
        {
            if (requireKey && string.IsNullOrWhiteSpace(Key))
                throw new ArgumentException(fieldName + ".key is required.");
            if (Key != null && Key.Length > D10IdentityLimits.KeyMaxLength)
                throw new ArgumentException(fieldName + ".key is too long.");
            if (Zh != null && Zh.Length > D10IdentityLimits.LongTextMaxLength)
                throw new ArgumentException(fieldName + ".zh is too long.");
            if (En != null && En.Length > D10IdentityLimits.LongTextMaxLength)
                throw new ArgumentException(fieldName + ".en is too long.");
        }
    }

    [Serializable]
    public sealed class D10IdentityHistoryPreservationContext
    {
        // These are opaque references owned by Legal, Finance, Faction and Crew.
        // Identity never reads or rewrites their values.
        public List<string> LegalHistoryRefs = new List<string>();
        public List<string> DebtHistoryRefs = new List<string>();
        public List<string> ReputationHistoryRefs = new List<string>();
        public List<string> RelationHistoryRefs = new List<string>();

        public D10IdentityHistoryPreservationContext Clone()
        {
            var result = new D10IdentityHistoryPreservationContext();
            CopyUnique(result.LegalHistoryRefs, LegalHistoryRefs);
            CopyUnique(result.DebtHistoryRefs, DebtHistoryRefs);
            CopyUnique(result.ReputationHistoryRefs, ReputationHistoryRefs);
            CopyUnique(result.RelationHistoryRefs, RelationHistoryRefs);
            return result;
        }

        internal void MergeFrom(D10IdentityHistoryPreservationContext other)
        {
            if (other == null) return;
            CopyUnique(LegalHistoryRefs, other.LegalHistoryRefs);
            CopyUnique(DebtHistoryRefs, other.DebtHistoryRefs);
            CopyUnique(ReputationHistoryRefs, other.ReputationHistoryRefs);
            CopyUnique(RelationHistoryRefs, other.RelationHistoryRefs);
        }

        internal static void CopyUnique(List<string> target, IEnumerable<string> source)
        {
            if (target == null || source == null) return;
            foreach (var value in source)
                if (!string.IsNullOrEmpty(value) && !target.Contains(value)) target.Add(value);
        }
    }

    [Serializable]
    public sealed class D10IdentityDocumentRecord
    {
        public string DocumentId = "";
        public string DocumentTypeKey = "";
        public string IssuerRef = "";
        public string StatusKey = "active";
        public int IssuedWorldDay;
        public int ExpiresWorldDay;
        public D10LocalizedText RecordText = new D10LocalizedText();

        public D10IdentityDocumentRecord Clone()
        {
            return new D10IdentityDocumentRecord
            {
                DocumentId = DocumentId,
                DocumentTypeKey = DocumentTypeKey,
                IssuerRef = IssuerRef,
                StatusKey = StatusKey,
                IssuedWorldDay = IssuedWorldDay,
                ExpiresWorldDay = ExpiresWorldDay,
                RecordText = RecordText == null ? new D10LocalizedText() : RecordText.Clone(),
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DocumentTypeKey))
                throw new ArgumentException("document.document_type_key is required.");
            if (DocumentTypeKey.Length > D10IdentityLimits.KeyMaxLength)
                throw new ArgumentException("document.document_type_key is too long.");
            if (DocumentId != null && DocumentId.Length > D10IdentityLimits.DocumentIdMaxLength)
                throw new ArgumentException("document.document_id is too long.");
            if (IssuerRef != null && IssuerRef.Length > D10IdentityLimits.KeyMaxLength)
                throw new ArgumentException("document.issuer_ref is too long.");
            if (ExpiresWorldDay > 0 && ExpiresWorldDay < IssuedWorldDay)
                throw new ArgumentException("document expiration precedes issue day.");
            if (RecordText == null) RecordText = new D10LocalizedText();
            RecordText.Validate("document.record_text");
        }
    }

    [Serializable]
    public sealed class D10IdentityHistoryEntry
    {
        public string HistoryId = "";
        public int WorldDay;
        public string ChangeKind = "creation";
        public string ChangeReasonKey = "";
        public string PreviousName = "";
        public string NewName = "";
        public string PreviousGenderExpressionKey = "";
        public string NewGenderExpressionKey = "";
        public List<string> PreviousPronounKeys = new List<string>();
        public List<string> NewPronounKeys = new List<string>();
        public string PreviousTitleKey = "";
        public string NewTitleKey = "";
        public string PreviousAgeBandKey = "";
        public string NewAgeBandKey = "";
        public string PreviousCultureOriginKey = "";
        public string NewCultureOriginKey = "";
        public string PreviousAppearanceLoadoutId = "";
        public string NewAppearanceLoadoutId = "";
        public List<string> PreservedLegalHistoryRefs = new List<string>();
        public List<string> PreservedDebtHistoryRefs = new List<string>();
        public List<string> PreservedReputationHistoryRefs = new List<string>();
        public List<string> PreservedRelationHistoryRefs = new List<string>();
        public D10LocalizedText Note = new D10LocalizedText();

        public D10IdentityHistoryEntry Clone()
        {
            var result = new D10IdentityHistoryEntry
            {
                HistoryId = HistoryId,
                WorldDay = WorldDay,
                ChangeKind = ChangeKind,
                ChangeReasonKey = ChangeReasonKey,
                PreviousName = PreviousName,
                NewName = NewName,
                PreviousGenderExpressionKey = PreviousGenderExpressionKey,
                NewGenderExpressionKey = NewGenderExpressionKey,
                PreviousTitleKey = PreviousTitleKey,
                NewTitleKey = NewTitleKey,
                PreviousAgeBandKey = PreviousAgeBandKey,
                NewAgeBandKey = NewAgeBandKey,
                PreviousCultureOriginKey = PreviousCultureOriginKey,
                NewCultureOriginKey = NewCultureOriginKey,
                PreviousAppearanceLoadoutId = PreviousAppearanceLoadoutId,
                NewAppearanceLoadoutId = NewAppearanceLoadoutId,
                Note = Note == null ? new D10LocalizedText() : Note.Clone(),
            };
            D10IdentityHistoryPreservationContext.CopyUnique(result.PreviousPronounKeys, PreviousPronounKeys);
            D10IdentityHistoryPreservationContext.CopyUnique(result.NewPronounKeys, NewPronounKeys);
            D10IdentityHistoryPreservationContext.CopyUnique(result.PreservedLegalHistoryRefs, PreservedLegalHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(result.PreservedDebtHistoryRefs, PreservedDebtHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(result.PreservedReputationHistoryRefs, PreservedReputationHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(result.PreservedRelationHistoryRefs, PreservedRelationHistoryRefs);
            return result;
        }
    }

    [Serializable]
    public sealed class D10PlayerIdentityCreation
    {
        public string Name = "无名船长";
        public string GenderExpressionKey = "gender_expression_unspecified";
        public List<string> PronounKeys = new List<string>();
        public D10LocalizedText Title = new D10LocalizedText("title.captain", "船长", "Captain");
        public string AgeBandKey = "age_band_adult";
        public string CultureOriginKey = "culture_origin_caribbean_mixed";
        public D10LocalizedText IdentityNote = new D10LocalizedText();

        public D10PlayerIdentityCreation Clone()
        {
            var result = new D10PlayerIdentityCreation
            {
                Name = Name,
                GenderExpressionKey = GenderExpressionKey,
                Title = Title == null ? new D10LocalizedText() : Title.Clone(),
                AgeBandKey = AgeBandKey,
                CultureOriginKey = CultureOriginKey,
                IdentityNote = IdentityNote == null ? new D10LocalizedText() : IdentityNote.Clone(),
            };
            D10IdentityHistoryPreservationContext.CopyUnique(result.PronounKeys, PronounKeys);
            return result;
        }

        public static D10PlayerIdentityCreation Default(string name)
        {
            return new D10PlayerIdentityCreation { Name = string.IsNullOrWhiteSpace(name) ? "无名船长" : name };
        }
    }

    [Serializable]
    public sealed class D10PlayerIdentityState
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string PlayerId = "";
        public string Name = "无名船长";
        public string GenderExpressionKey = "gender_expression_unspecified";
        public List<string> PronounKeys = new List<string>();
        public D10LocalizedText Title = new D10LocalizedText("title.captain", "船长", "Captain");
        public string AgeBandKey = "age_band_adult";
        public string CultureOriginKey = "culture_origin_caribbean_mixed";
        public D10LocalizedText IdentityNote = new D10LocalizedText();
        public D10IdentityHistoryPreservationContext PreservedHistory = new D10IdentityHistoryPreservationContext();
        public List<D10IdentityDocumentRecord> Documents = new List<D10IdentityDocumentRecord>();
        public List<D10IdentityHistoryEntry> IdentityHistory = new List<D10IdentityHistoryEntry>();

        public static D10PlayerIdentityState CreateNew(string playerId, D10PlayerIdentityCreation creation, int worldDay = 0,
            D10IdentityHistoryPreservationContext preservedHistory = null)
        {
            var input = creation == null ? D10PlayerIdentityCreation.Default("无名船长") : creation.Clone();
            var result = new D10PlayerIdentityState
            {
                PlayerId = playerId ?? "",
                Name = NormalizeName(input.Name),
                GenderExpressionKey = NormalizeKey(input.GenderExpressionKey, "gender_expression_unspecified"),
                Title = input.Title == null ? new D10LocalizedText("title.captain", "船长", "Captain") : input.Title.Clone(),
                AgeBandKey = NormalizeKey(input.AgeBandKey, "age_band_adult"),
                CultureOriginKey = NormalizeKey(input.CultureOriginKey, "culture_origin_caribbean_mixed"),
                IdentityNote = input.IdentityNote == null ? new D10LocalizedText() : input.IdentityNote.Clone(),
                PreservedHistory = preservedHistory == null ? new D10IdentityHistoryPreservationContext() : preservedHistory.Clone(),
            };
            D10IdentityHistoryPreservationContext.CopyUnique(result.PronounKeys, input.PronounKeys);
            result.Title.Validate("title");
            result.IdentityNote.Validate("identity_note");
            var creationHistory = result.BuildHistoryEntry("identity_history_0001", worldDay, "creation", "identity.created", "", "");
            creationHistory.PreviousName = "";
            creationHistory.PreviousGenderExpressionKey = "";
            creationHistory.PreviousPronounKeys.Clear();
            creationHistory.PreviousTitleKey = "";
            creationHistory.PreviousAgeBandKey = "";
            creationHistory.PreviousCultureOriginKey = "";
            result.IdentityHistory.Add(creationHistory);
            return result;
        }

        public static D10PlayerIdentityState CreateDefault(string playerId, string legacyName)
        {
            return CreateNew(playerId, D10PlayerIdentityCreation.Default(legacyName), 0, null);
        }

        public D10PlayerIdentityState Clone()
        {
            var result = new D10PlayerIdentityState
            {
                SchemaVersion = SchemaVersion,
                PlayerId = PlayerId,
                Name = Name,
                GenderExpressionKey = GenderExpressionKey,
                Title = Title == null ? new D10LocalizedText() : Title.Clone(),
                AgeBandKey = AgeBandKey,
                CultureOriginKey = CultureOriginKey,
                IdentityNote = IdentityNote == null ? new D10LocalizedText() : IdentityNote.Clone(),
                PreservedHistory = PreservedHistory == null ? new D10IdentityHistoryPreservationContext() : PreservedHistory.Clone(),
                Documents = new List<D10IdentityDocumentRecord>(),
                IdentityHistory = new List<D10IdentityHistoryEntry>(),
            };
            D10IdentityHistoryPreservationContext.CopyUnique(result.PronounKeys, PronounKeys);
            if (Documents != null)
                foreach (var document in Documents)
                    if (document != null) result.Documents.Add(document.Clone());
            if (IdentityHistory != null)
                foreach (var entry in IdentityHistory)
                    if (entry != null) result.IdentityHistory.Add(entry.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D10 identity schema_version mismatch: expected " + CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (string.IsNullOrEmpty(PlayerId)) throw new InvalidOperationException("D10 identity player_id is required.");
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > D10IdentityLimits.NameMaxLength)
                throw new InvalidOperationException("D10 identity name is missing or too long.");
            if (Title == null) Title = new D10LocalizedText("title.captain", "船长", "Captain");
            Title.Validate("title");
            if (IdentityNote == null) IdentityNote = new D10LocalizedText();
            IdentityNote.Validate("identity_note");
            if (PreservedHistory == null) PreservedHistory = new D10IdentityHistoryPreservationContext();
            if (Documents == null) Documents = new List<D10IdentityDocumentRecord>();
            foreach (var document in Documents)
                if (document != null) document.Validate();
            if (IdentityHistory == null) IdentityHistory = new List<D10IdentityHistoryEntry>();
        }

        internal D10IdentityHistoryEntry BuildHistoryEntry(string historyId, int worldDay, string changeKind,
            string reasonKey, string previousAppearanceLoadoutId, string newAppearanceLoadoutId)
        {
            var entry = new D10IdentityHistoryEntry
            {
                HistoryId = historyId,
                WorldDay = Math.Max(0, worldDay),
                ChangeKind = changeKind ?? "identity_change",
                ChangeReasonKey = reasonKey ?? "identity.changed",
                PreviousName = Name,
                NewName = Name,
                PreviousGenderExpressionKey = GenderExpressionKey,
                NewGenderExpressionKey = GenderExpressionKey,
                PreviousTitleKey = Title == null ? "" : Title.Key,
                NewTitleKey = Title == null ? "" : Title.Key,
                PreviousAgeBandKey = AgeBandKey,
                NewAgeBandKey = AgeBandKey,
                PreviousCultureOriginKey = CultureOriginKey,
                NewCultureOriginKey = CultureOriginKey,
                PreviousAppearanceLoadoutId = previousAppearanceLoadoutId ?? "",
                NewAppearanceLoadoutId = newAppearanceLoadoutId ?? "",
                Note = IdentityNote == null ? new D10LocalizedText() : IdentityNote.Clone(),
            };
            D10IdentityHistoryPreservationContext.CopyUnique(entry.PreviousPronounKeys, PronounKeys);
            D10IdentityHistoryPreservationContext.CopyUnique(entry.NewPronounKeys, PronounKeys);
            CopyPreservedRefs(entry);
            return entry;
        }

        internal void CopyPreservedRefs(D10IdentityHistoryEntry entry)
        {
            if (entry == null || PreservedHistory == null) return;
            D10IdentityHistoryPreservationContext.CopyUnique(entry.PreservedLegalHistoryRefs, PreservedHistory.LegalHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(entry.PreservedDebtHistoryRefs, PreservedHistory.DebtHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(entry.PreservedReputationHistoryRefs, PreservedHistory.ReputationHistoryRefs);
            D10IdentityHistoryPreservationContext.CopyUnique(entry.PreservedRelationHistoryRefs, PreservedHistory.RelationHistoryRefs);
        }

        private static string NormalizeName(string value)
        {
            var result = string.IsNullOrWhiteSpace(value) ? "无名船长" : value.Trim();
            if (result.Length > D10IdentityLimits.NameMaxLength) throw new ArgumentException("identity.name is too long.");
            return result;
        }

        private static string NormalizeKey(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }

    public interface ID10AppearanceWriter
    {
        string CurrentAppearanceLoadoutId { get; }
        bool TryApplyAppearance(string appearanceLoadoutId);
    }

    /// <summary>把 D10 外观请求交给已有 AppearanceSystem owner，D10 不复制外观裁决。</summary>
    public sealed class D10AppearanceOwnerWriter : ID10AppearanceWriter
    {
        private readonly D10AppearanceSystem _appearanceSystem;

        public D10AppearanceOwnerWriter(D10AppearanceSystem appearanceSystem)
        {
            _appearanceSystem = appearanceSystem;
        }

        public string CurrentAppearanceLoadoutId
        {
            get
            {
                if (_appearanceSystem == null || _appearanceSystem.Snapshot == null || _appearanceSystem.Snapshot.ActiveLoadout == null)
                    return "";
                var loadout = _appearanceSystem.Snapshot.ActiveLoadout;
                var parts = new List<string>();
                foreach (D10AppearanceSlot slot in Enum.GetValues(typeof(D10AppearanceSlot)))
                    parts.Add(slot.ToString() + "=" + loadout.Get(slot));
                return string.Join("|", parts.ToArray());
            }
        }

        public bool TryApplyAppearance(string appearanceLoadoutId)
        {
            if (_appearanceSystem == null) return false;
            var result = _appearanceSystem.Equip(appearanceLoadoutId);
            return result != null && result.Success;
        }
    }

    public sealed class D10PortIdentityUpdateRequest
    {
        public D10LocalizedText NewTitle;
        public string AppearanceLoadoutId = "";
        public D10IdentityDocumentRecord Document;
        public string ChangeReasonKey = "identity.port_update";
        public int WorldDay;
        public D10LocalizedText Note;

        public bool HasAnyChange
        {
            get { return NewTitle != null || !string.IsNullOrEmpty(AppearanceLoadoutId) || Document != null; }
        }
    }

    public enum D10IdentityUpdateStatus
    {
        Applied,
        Rejected,
    }

    public sealed class D10PortIdentityUpdateResult
    {
        public D10IdentityUpdateStatus Status;
        public string ErrorCode = "";
        public D10IdentityDocumentRecord AddedDocument;
        public D10PlayerIdentityState Identity;
        public bool AppearanceChanged;

        public bool Succeeded { get { return Status == D10IdentityUpdateStatus.Applied; } }
    }

    public sealed class D10PortIdentityInspection
    {
        public bool IsAtPort;
        public string LocationRef = "";
        public bool CanEditTitle;
        public bool CanEditAppearance;
        public bool CanRecordDocument;
        public string CurrentAppearanceLoadoutId = "";
        public D10PlayerIdentityState Identity;
        public int IdentityHistoryCount;
        public int DocumentCount;
        public int PreservedLegalHistoryCount;
        public int PreservedDebtHistoryCount;
        public int PreservedReputationHistoryCount;
        public int PreservedRelationHistoryCount;
    }

    /// <summary>
    /// 玩家身份系统。状态结果只落在 D10PlayerIdentityState；调用方负责把
    /// Snapshot 写回 PlayerState/SaveSystem，避免 UI 直接改存档数值。
    /// </summary>
    public sealed class D10PlayerIdentitySystem
    {
        private readonly D10PlayerIdentityState _state;
        private readonly ID10AppearanceWriter _appearanceWriter;

        public D10PlayerIdentitySystem(D10PlayerIdentityState initial, ID10AppearanceWriter appearanceWriter = null)
        {
            _state = initial == null ? D10PlayerIdentityState.CreateDefault("player:unknown", "无名船长") : initial.Clone();
            _state.Validate();
            _appearanceWriter = appearanceWriter;
        }

        public D10PlayerIdentityState Snapshot { get { return _state.Clone(); } }

        public D10PortIdentityInspection InspectAtPort(bool isAtPort, string locationRef)
        {
            bool portContext = IsPortContext(isAtPort, locationRef);
            var preserved = _state.PreservedHistory ?? new D10IdentityHistoryPreservationContext();
            return new D10PortIdentityInspection
            {
                IsAtPort = portContext,
                LocationRef = locationRef ?? "",
                CanEditTitle = portContext,
                CanEditAppearance = portContext && _appearanceWriter != null,
                CanRecordDocument = portContext,
                CurrentAppearanceLoadoutId = _appearanceWriter == null ? "" : _appearanceWriter.CurrentAppearanceLoadoutId,
                Identity = _state.Clone(),
                IdentityHistoryCount = _state.IdentityHistory == null ? 0 : _state.IdentityHistory.Count,
                DocumentCount = _state.Documents == null ? 0 : _state.Documents.Count,
                PreservedLegalHistoryCount = preserved.LegalHistoryRefs.Count,
                PreservedDebtHistoryCount = preserved.DebtHistoryRefs.Count,
                PreservedReputationHistoryCount = preserved.ReputationHistoryRefs.Count,
                PreservedRelationHistoryCount = preserved.RelationHistoryRefs.Count,
            };
        }

        public D10PortIdentityUpdateResult ApplyPortUpdate(D10PortIdentityUpdateRequest request, bool isAtPort,
            string locationRef, D10IdentityHistoryPreservationContext latestHistory = null)
        {
            var rejected = new D10PortIdentityUpdateResult
            {
                Status = D10IdentityUpdateStatus.Rejected,
                Identity = _state.Clone(),
            };
            if (!IsPortContext(isAtPort, locationRef))
            {
                rejected.ErrorCode = "port_access_required";
                return rejected;
            }
            if (request == null || !request.HasAnyChange)
            {
                rejected.ErrorCode = "identity_update_empty";
                return rejected;
            }
            if (request.NewTitle != null)
            {
                try { request.NewTitle.Validate("new_title", true); }
                catch (ArgumentException) { rejected.ErrorCode = "invalid_title"; return rejected; }
            }
            if (request.Document != null)
            {
                try { request.Document.Validate(); }
                catch (ArgumentException) { rejected.ErrorCode = "invalid_document"; return rejected; }
            }
            if (!string.IsNullOrEmpty(request.AppearanceLoadoutId) && _appearanceWriter == null)
            {
                rejected.ErrorCode = "appearance_owner_unavailable";
                return rejected;
            }

            string previousTitleKey = _state.Title == null ? "" : _state.Title.Key;
            string previousName = _state.Name;
            string previousGenderExpressionKey = _state.GenderExpressionKey;
            string previousAgeBandKey = _state.AgeBandKey;
            string previousCultureOriginKey = _state.CultureOriginKey;
            var previousPronounKeys = _state.PronounKeys == null ? new List<string>() : new List<string>(_state.PronounKeys);
            string previousAppearance = _appearanceWriter == null ? "" : _appearanceWriter.CurrentAppearanceLoadoutId;
            bool appearanceChanged = false;
            if (!string.IsNullOrEmpty(request.AppearanceLoadoutId))
            {
                if (!_appearanceWriter.TryApplyAppearance(request.AppearanceLoadoutId))
                {
                    rejected.ErrorCode = "unknown_appearance_loadout";
                    return rejected;
                }
                appearanceChanged = previousAppearance != request.AppearanceLoadoutId;
            }

            // Merge-only: the identity system never clears domain-owned history references.
            _state.PreservedHistory.MergeFrom(latestHistory);
            if (request.NewTitle != null) _state.Title = request.NewTitle.Clone();
            if (request.Note != null) _state.IdentityNote = request.Note.Clone();

            D10IdentityDocumentRecord addedDocument = null;
            if (request.Document != null)
            {
                addedDocument = request.Document.Clone();
                if (string.IsNullOrEmpty(addedDocument.DocumentId))
                    addedDocument.DocumentId = "identity_document_" + (_state.Documents.Count + 1).ToString("D4");
                addedDocument.IssuedWorldDay = Math.Max(0, request.WorldDay);
                if (string.IsNullOrEmpty(addedDocument.StatusKey)) addedDocument.StatusKey = "active";
                _state.Documents.Add(addedDocument.Clone());
            }

            var history = _state.BuildHistoryEntry(
                "identity_history_" + (_state.IdentityHistory.Count + 1).ToString("D4"),
                request.WorldDay,
                "port_identity_update",
                string.IsNullOrEmpty(request.ChangeReasonKey) ? "identity.port_update" : request.ChangeReasonKey,
                previousAppearance,
                _appearanceWriter == null ? previousAppearance : _appearanceWriter.CurrentAppearanceLoadoutId);
            history.PreviousTitleKey = previousTitleKey;
            history.NewTitleKey = _state.Title == null ? "" : _state.Title.Key;
            history.PreviousName = previousName;
            history.PreviousGenderExpressionKey = previousGenderExpressionKey;
            history.PreviousPronounKeys = previousPronounKeys;
            history.PreviousAgeBandKey = previousAgeBandKey;
            history.PreviousCultureOriginKey = previousCultureOriginKey;
            _state.IdentityHistory.Add(history);
            _state.Validate();

            return new D10PortIdentityUpdateResult
            {
                Status = D10IdentityUpdateStatus.Applied,
                Identity = _state.Clone(),
                AddedDocument = addedDocument,
                AppearanceChanged = appearanceChanged,
            };
        }

        private static bool IsPortContext(bool isAtPort, string locationRef)
        {
            if (!isAtPort || string.IsNullOrWhiteSpace(locationRef)) return false;
            return !string.Equals(locationRef, "open_sea", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(locationRef, "sea", StringComparison.OrdinalIgnoreCase);
        }
    }
}
