// ============================================================
// D20CharacterSaveMigration —— D20-L27 角色保存迁移（纯 C#）
//
// 迁移只处理 D20CharacterSaveProjection。缺失的动作/NPC/动物块被解释为
// “没有可恢复的 D20 投影”，不会生成已发生的动作、角色、动物或 owner 事实；
// 未知/未来版本和不可能状态保留原始副本并拒绝交给 SaveSystem。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>D20 角色存档迁移的结果状态。</summary>
    public enum D20CharacterSaveMigrationStatus
    {
        Rejected = 0,
        Migrated = 1,
        AlreadyUpToDate = 2,
    }

    /// <summary>D20 角色存档迁移结果；拒绝时保留 OriginalProjection。</summary>
    [Serializable]
    public sealed class D20CharacterSaveMigrationResult
    {
        public D20CharacterSaveMigrationStatus Status;
        public int SourceSchemaVersion;
        public int TargetSchemaVersion = D20CharacterSaveProjection.CurrentSchemaVersion;
        public string ReasonCode = string.Empty;
        public string FailureMessage = string.Empty;
        public D20CharacterSaveProjection MigratedProjection;
        public D20CharacterSaveProjection OriginalProjection;
        public List<string> MigrationFlags = new List<string>();

        public bool Succeeded
        {
            get
            {
                return Status == D20CharacterSaveMigrationStatus.Migrated ||
                    Status == D20CharacterSaveMigrationStatus.AlreadyUpToDate;
            }
        }

        public D20CharacterSaveMigrationResult Clone()
        {
            var result = new D20CharacterSaveMigrationResult
            {
                Status = Status,
                SourceSchemaVersion = SourceSchemaVersion,
                TargetSchemaVersion = TargetSchemaVersion,
                ReasonCode = ReasonCode ?? string.Empty,
                FailureMessage = FailureMessage ?? string.Empty,
                MigratedProjection = MigratedProjection == null ? null : MigratedProjection.Clone(),
                OriginalProjection = OriginalProjection == null ? null : OriginalProjection.Clone(),
            };
            if (MigrationFlags != null) result.MigrationFlags.AddRange(MigrationFlags);
            return result;
        }

        internal static D20CharacterSaveMigrationResult Success(
            D20CharacterSaveProjection projection,
            int sourceSchemaVersion,
            bool changed,
            List<string> flags)
        {
            var result = new D20CharacterSaveMigrationResult
            {
                Status = changed
                    ? D20CharacterSaveMigrationStatus.Migrated
                    : D20CharacterSaveMigrationStatus.AlreadyUpToDate,
                SourceSchemaVersion = sourceSchemaVersion,
                TargetSchemaVersion = D20CharacterSaveProjection.CurrentSchemaVersion,
                ReasonCode = changed ? "d20_save_migrated" : "d20_save_already_current",
                MigratedProjection = projection == null ? null : projection.Clone(),
            };
            if (flags != null) result.MigrationFlags.AddRange(flags);
            return result;
        }

        internal static D20CharacterSaveMigrationResult Reject(
            D20CharacterSaveProjection original,
            int sourceSchemaVersion,
            string reasonCode,
            string failureMessage)
        {
            return new D20CharacterSaveMigrationResult
            {
                Status = D20CharacterSaveMigrationStatus.Rejected,
                SourceSchemaVersion = sourceSchemaVersion,
                TargetSchemaVersion = D20CharacterSaveProjection.CurrentSchemaVersion,
                ReasonCode = reasonCode ?? "d20_save_migration_rejected",
                FailureMessage = failureMessage ?? string.Empty,
                OriginalProjection = original == null ? null : original.Clone(),
            };
        }
    }

    /// <summary>
    /// D20-L27 确定性、幂等的保存迁移器。当前支持根/嵌套 v0 → v1，
    /// 当前版本再次迁移只返回等价副本；未来版本和非法数据不被降级覆盖。
    /// </summary>
    public static class D20CharacterSaveMigration
    {
        public const int SupportedSchemaVersion = D20CharacterSaveProjection.CurrentSchemaVersion;

        /// <summary>
        /// 迁移 D20 角色保存投影。defaultPlayerId 只可来自既有 Save 根的稳定 ID，
        /// 不会据此创建身份、动作、NPC 或动物事实。
        /// </summary>
        public static D20CharacterSaveMigrationResult Migrate(
            D20CharacterSaveProjection source,
            string defaultPlayerId = "")
        {
            if (!IsValidOptionalPlayerId(defaultPlayerId))
            {
                return D20CharacterSaveMigrationResult.Reject(
                    source, source == null ? 0 : source.SchemaVersion,
                    "d20_save.invalid_default_player_id",
                    "default_player_id is not a stable character id.");
            }

            if (source == null)
            {
                var empty = D20CharacterSaveProjection.CreateDefault(defaultPlayerId);
                var emptyFlags = new List<string> { "missing_root_defaulted_to_empty_projection" };
                if (!string.IsNullOrEmpty(defaultPlayerId))
                    emptyFlags.Add("default_player_id_applied_without_identity_fact");
                empty.Validate();
                return D20CharacterSaveMigrationResult.Success(empty, 0, true, emptyFlags);
            }

            int sourceVersion = source.SchemaVersion;
            if (sourceVersion < 0 || sourceVersion > SupportedSchemaVersion)
            {
                return D20CharacterSaveMigrationResult.Reject(
                    source, sourceVersion,
                    "d20_save.unsupported_version",
                    "save schema version is outside the supported range.");
            }

            var flags = new List<string>();
            bool changed = false;
            bool missingAction = source.PlayerAction == null;
            bool missingNpcIdentities = source.NpcIdentities == null;
            bool missingNpcBehaviors = source.NpcBehaviors == null;
            bool missingAnimalIdentities = source.AnimalIdentities == null;
            bool missingAnimalBehaviors = source.AnimalBehaviors == null;

            D20CharacterSaveProjection target = source.Clone();
            try
            {
                if (sourceVersion == 0)
                {
                    target.SchemaVersion = SupportedSchemaVersion;
                    flags.Add("root_schema_v0_upgraded_to_v1");
                    changed = true;
                }

                if (source.PlayerId == null)
                {
                    flags.Add("missing_player_id_defaulted_empty_until_resolved");
                    changed = true;
                }
                string derivedPlayerId = ResolvePlayerId(target, defaultPlayerId);
                if (string.IsNullOrEmpty(target.PlayerId) && !string.IsNullOrEmpty(derivedPlayerId))
                {
                    target.PlayerId = derivedPlayerId;
                    flags.Add("player_id_recovered_from_existing_projection");
                    changed = true;
                }
                else if (!string.IsNullOrEmpty(defaultPlayerId) && string.IsNullOrEmpty(target.PlayerId))
                {
                    target.PlayerId = defaultPlayerId;
                    flags.Add("default_player_id_applied_without_identity_fact");
                    changed = true;
                }

                if (missingAction && sourceVersion == 0)
                {
                    flags.Add("missing_player_action_defaulted_to_absent");
                    changed = true;
                }
                if (missingNpcIdentities)
                {
                    flags.Add("missing_npc_identities_defaulted_empty");
                    changed = true;
                }
                if (missingNpcBehaviors)
                {
                    flags.Add("missing_npc_behaviors_defaulted_empty");
                    changed = true;
                }
                if (missingAnimalIdentities)
                {
                    flags.Add("missing_animal_identities_defaulted_empty");
                    changed = true;
                }
                if (missingAnimalBehaviors)
                {
                    flags.Add("missing_animal_behaviors_defaulted_empty");
                    changed = true;
                }

                if (source.PetOwnership == null)
                {
                    flags.Add("missing_pet_ownership_defaulted_empty");
                    changed = true;
                }
                if (source.Events == null)
                {
                    flags.Add("missing_event_cursor_defaulted_empty");
                    changed = true;
                }
                if (source.UpstreamSnapshots == null)
                {
                    flags.Add("missing_upstream_references_defaulted_empty");
                    changed = true;
                }
                if (source.CauseEventIds == null)
                {
                    flags.Add("missing_cause_event_ids_defaulted_empty");
                    changed = true;
                }

                NormalizePlayerIdentity(target, flags, ref changed);
                NormalizePresence(target, flags, ref changed);
                NormalizeAction(target, flags, ref changed);
                NormalizeIdentityPresentation(target, flags, ref changed);
                NormalizeNpcIdentities(target, flags, ref changed);
                NormalizeNpcBehaviors(target, flags, ref changed);
                NormalizeAnimalIdentities(target, flags, ref changed);
                NormalizeAnimalBehaviors(target, flags, ref changed);
                NormalizePetOwnership(target, flags, ref changed);
                NormalizeEvents(target, flags, ref changed);
                NormalizeUpstreamReferences(target, flags, ref changed);

                if (target.WorldDay < 0 || target.SaveRevision < 0)
                    throw new InvalidOperationException("root world_day/save_revision is negative.");
                target.Validate();
            }
            catch (InvalidOperationException exception)
            {
                return D20CharacterSaveMigrationResult.Reject(
                    source, sourceVersion,
                    "d20_save.invalid_projection",
                    exception.Message);
            }

            return D20CharacterSaveMigrationResult.Success(target, sourceVersion, changed, flags);
        }

        private static void NormalizePlayerIdentity(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.PlayerIdentity == null) return;
            EnsureSupportedVersion(target.PlayerIdentity.SchemaVersion,
                D20CharacterIdentityContracts.CurrentSchemaVersion, "player_identity");
            if (target.PlayerIdentity.SchemaVersion == 0)
            {
                target.PlayerIdentity.SchemaVersion = D20CharacterIdentityContracts.CurrentSchemaVersion;
                flags.Add("player_identity_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (target.PlayerIdentity.ImmutableFields == null)
                throw new InvalidOperationException("player_identity immutable_fields are missing.");
        }

        private static void NormalizePresence(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.PlayerPresence == null) return;
            EnsureSupportedVersion(target.PlayerPresence.SchemaVersion,
                D20PlayerPresenceSnapshot.CurrentSchemaVersion, "player_presence");
            if (target.PlayerPresence.SchemaVersion == 0)
            {
                target.PlayerPresence.SchemaVersion = D20PlayerPresenceSnapshot.CurrentSchemaVersion;
                flags.Add("player_presence_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (target.PlayerPresence.CommandReceipts == null)
            {
                target.PlayerPresence.CommandReceipts = new List<D20PlayerPresenceResult>();
                flags.Add("missing_presence_command_receipts_defaulted_empty");
                changed = true;
            }
        }

        private static void NormalizeAction(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.PlayerAction == null) return;
            EnsureSupportedVersion(target.PlayerAction.SchemaVersion,
                D20ActionInterruptSnapshot.CurrentSchemaVersion, "player_action");
            if (target.PlayerAction.SchemaVersion == 0)
            {
                target.PlayerAction.SchemaVersion = D20ActionInterruptSnapshot.CurrentSchemaVersion;
                flags.Add("player_action_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (target.PlayerAction.CommandReceipts == null)
            {
                target.PlayerAction.CommandReceipts = new List<D20ActionInterruptReceipt>();
                flags.Add("missing_action_command_receipts_defaulted_empty");
                changed = true;
            }
        }

        private static void NormalizeIdentityPresentation(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            D20IdentityPresentationSaveProjection presentation = target.PlayerIdentityPresentation;
            if (presentation == null) return;
            EnsureSupportedVersion(presentation.SchemaVersion,
                D20IdentityPresentationSaveProjection.CurrentSchemaVersion, "identity_presentation");
            if (presentation.SchemaVersion == 0)
            {
                presentation.SchemaVersion = D20IdentityPresentationSaveProjection.CurrentSchemaVersion;
                flags.Add("identity_presentation_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (presentation.PronounKeys == null)
            {
                presentation.PronounKeys = new List<string>();
                flags.Add("missing_identity_pronouns_defaulted_empty");
                changed = true;
            }
            if (presentation.IdentityHistory == null)
            {
                presentation.IdentityHistory = new List<D20IdentityHistorySaveProjection>();
                flags.Add("missing_identity_history_defaulted_empty");
                changed = true;
            }
            if (presentation.Title != null) EnsurePresentationText(presentation.Title, "identity_title");
            if (presentation.IdentityNote != null) EnsurePresentationText(presentation.IdentityNote, "identity_note");
            foreach (D20IdentityHistorySaveProjection history in presentation.IdentityHistory)
            {
                if (history == null) throw new InvalidOperationException("identity history entry is missing.");
                if (history.PreviousPronounKeys == null)
                {
                    history.PreviousPronounKeys = new List<string>();
                    flags.Add("missing_identity_history_previous_pronouns_defaulted_empty");
                    changed = true;
                }
                if (history.NewPronounKeys == null)
                {
                    history.NewPronounKeys = new List<string>();
                    flags.Add("missing_identity_history_new_pronouns_defaulted_empty");
                    changed = true;
                }
                if (history.Note != null) EnsurePresentationText(history.Note, "identity_history_note");
            }
        }

        private static void NormalizeNpcIdentities(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            foreach (D20NpcIdentity identity in target.NpcIdentities)
            {
                if (identity == null) throw new InvalidOperationException("NPC identity entry is missing.");
                EnsureSupportedVersion(identity.SchemaVersion,
                    D20NpcIdentityContracts.CurrentSchemaVersion, "npc_identity");
                if (identity.SchemaVersion == 0)
                {
                    identity.SchemaVersion = D20NpcIdentityContracts.CurrentSchemaVersion;
                    flags.Add("npc_identity_schema_v0_upgraded_to_v1");
                    changed = true;
                }
                if (identity.MigrationStatus == D20NpcIdentityMigrationStatus.Unknown)
                {
                    if (!D20NpcIdentityContracts.IsStableNpcId(identity.NpcId))
                        throw new InvalidOperationException("NPC identity cannot default an unstable id.");
                    identity.MigrationStatus = D20NpcIdentityMigrationStatus.UnknownPreserved;
                    flags.Add("npc_identity_unknown_status_preserved");
                    changed = true;
                }
            }
        }

        private static void NormalizeNpcBehaviors(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            foreach (D20NpcBehaviorSaveProjection wrapper in target.NpcBehaviors)
            {
                if (wrapper == null) throw new InvalidOperationException("NPC behavior entry is missing.");
                EnsureSupportedVersion(wrapper.SchemaVersion,
                    D20NpcBehaviorSaveProjection.CurrentSchemaVersion, "npc_behavior_wrapper");
                if (wrapper.SchemaVersion == 0)
                {
                    wrapper.SchemaVersion = D20NpcBehaviorSaveProjection.CurrentSchemaVersion;
                    flags.Add("npc_behavior_wrapper_schema_v0_upgraded_to_v1");
                    changed = true;
                }
                if (wrapper.Behavior == null)
                    throw new InvalidOperationException("NPC behavior snapshot is missing.");
                EnsureSupportedVersion(wrapper.Behavior.SchemaVersion,
                    D20NpcBehaviorSnapshot.CurrentSchemaVersion, "npc_behavior");
                if (wrapper.Behavior.SchemaVersion == 0)
                {
                    wrapper.Behavior.SchemaVersion = D20NpcBehaviorSnapshot.CurrentSchemaVersion;
                    flags.Add("npc_behavior_schema_v0_upgraded_to_v1");
                    changed = true;
                }
                if (wrapper.CauseEventIds == null)
                {
                    wrapper.CauseEventIds = new List<string>();
                    flags.Add("missing_npc_behavior_causes_defaulted_empty");
                    changed = true;
                }
            }
        }

        private static void NormalizeAnimalIdentities(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            foreach (D20AnimalIdentity identity in target.AnimalIdentities)
            {
                if (identity == null) throw new InvalidOperationException("animal identity entry is missing.");
                EnsureSupportedVersion(identity.SchemaVersion,
                    D20AnimalIdentityContracts.CurrentSchemaVersion, "animal_identity");
                if (identity.SchemaVersion == 0)
                {
                    identity.SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;
                    flags.Add("animal_identity_schema_v0_upgraded_to_v1");
                    changed = true;
                }
            }
        }

        private static void NormalizeAnimalBehaviors(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            foreach (D20AnimalBehaviorSnapshot behavior in target.AnimalBehaviors)
            {
                if (behavior == null) throw new InvalidOperationException("animal behavior entry is missing.");
                EnsureSupportedVersion(behavior.SchemaVersion,
                    D20AnimalIdentityContracts.CurrentSchemaVersion, "animal_behavior");
                if (behavior.SchemaVersion == 0)
                {
                    behavior.SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;
                    flags.Add("animal_behavior_schema_v0_upgraded_to_v1");
                    changed = true;
                }
                if (behavior.Receipts == null)
                {
                    behavior.Receipts = new List<D20AnimalBehaviorReceipt>();
                    flags.Add("missing_animal_behavior_receipts_defaulted_empty");
                    changed = true;
                }
                foreach (D20AnimalBehaviorReceipt receipt in behavior.Receipts)
                {
                    if (receipt == null || receipt.Result == null)
                        throw new InvalidOperationException("animal behavior receipt is incomplete.");
                    EnsureSupportedVersion(receipt.Result.SchemaVersion,
                        D20AnimalIdentityContracts.CurrentSchemaVersion, "animal_behavior_result");
                    if (receipt.Result.SchemaVersion == 0)
                    {
                        receipt.Result.SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;
                        flags.Add("animal_behavior_result_schema_v0_upgraded_to_v1");
                        changed = true;
                    }
                }
            }
        }

        private static void NormalizePetOwnership(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.PetOwnership == null)
            {
                target.PetOwnership = new D20PetOwnershipSnapshot();
                return;
            }
            EnsureSupportedVersion(target.PetOwnership.SchemaVersion,
                D20PetOwnershipSystem.CurrentSchemaVersion, "pet_ownership");
            if (target.PetOwnership.SchemaVersion == 0)
            {
                target.PetOwnership.SchemaVersion = D20PetOwnershipSystem.CurrentSchemaVersion;
                flags.Add("pet_ownership_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (target.PetOwnership.Records == null)
            {
                target.PetOwnership.Records = new List<D20PetOwnershipRecord>();
                flags.Add("missing_pet_ownership_records_defaulted_empty");
                changed = true;
            }
            foreach (D20PetOwnershipRecord record in target.PetOwnership.Records)
            {
                if (record == null) throw new InvalidOperationException("pet ownership record is missing.");
                EnsureSupportedVersion(record.SchemaVersion,
                    D20PetOwnershipSystem.CurrentSchemaVersion, "pet_ownership_record");
                if (record.SchemaVersion == 0)
                {
                    record.SchemaVersion = D20PetOwnershipSystem.CurrentSchemaVersion;
                    flags.Add("pet_ownership_record_schema_v0_upgraded_to_v1");
                    changed = true;
                }
            }
        }

        private static void NormalizeEvents(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.Events == null)
            {
                target.Events = new D20CharacterSaveEventCursor();
                return;
            }
            EnsureSupportedVersion(target.Events.SchemaVersion,
                D20CharacterSaveEventCursor.CurrentSchemaVersion, "event_cursor");
            if (target.Events.SchemaVersion == 0)
            {
                target.Events.SchemaVersion = D20CharacterSaveEventCursor.CurrentSchemaVersion;
                flags.Add("event_cursor_schema_v0_upgraded_to_v1");
                changed = true;
            }
            if (target.Events.AppliedEventKeys == null)
            {
                target.Events.AppliedEventKeys = new List<string>();
                flags.Add("missing_applied_event_keys_defaulted_empty");
                changed = true;
            }
            if (target.Events.CauseEventIds == null)
            {
                target.Events.CauseEventIds = new List<string>();
                flags.Add("missing_event_cursor_causes_defaulted_empty");
                changed = true;
            }
        }

        private static void NormalizeUpstreamReferences(
            D20CharacterSaveProjection target, List<string> flags, ref bool changed)
        {
            if (target.UpstreamSnapshots == null)
            {
                target.UpstreamSnapshots = new List<D20SaveOwnerSnapshotReference>();
                return;
            }
            foreach (D20SaveOwnerSnapshotReference reference in target.UpstreamSnapshots)
            {
                if (reference == null) throw new InvalidOperationException("upstream reference is missing.");
                EnsureSupportedVersion(reference.SchemaVersion,
                    D20SaveOwnerSnapshotReference.CurrentSchemaVersion, "upstream_reference");
                if (reference.SchemaVersion == 0)
                {
                    reference.SchemaVersion = D20SaveOwnerSnapshotReference.CurrentSchemaVersion;
                    flags.Add("upstream_reference_schema_v0_upgraded_to_v1");
                    changed = true;
                }
                if (reference.CauseEventIds == null)
                {
                    reference.CauseEventIds = new List<string>();
                    flags.Add("missing_upstream_causes_defaulted_empty");
                    changed = true;
                }
            }
        }

        private static void EnsurePresentationText(
            D20IdentityPresentationTextSaveProjection text, string fieldName)
        {
            if (text == null) return;
            text.Validate(fieldName);
        }

        private static string ResolvePlayerId(
            D20CharacterSaveProjection target, string defaultPlayerId)
        {
            string candidate = target.PlayerId ?? string.Empty;
            candidate = ResolveCandidate(candidate,
                target.PlayerIdentity == null ? string.Empty : target.PlayerIdentity.CharacterId);
            candidate = ResolveCandidate(candidate,
                target.PlayerPresence == null ? string.Empty : target.PlayerPresence.PlayerId);
            candidate = ResolveCandidate(candidate,
                target.PlayerAction == null ? string.Empty : target.PlayerAction.ActorId);
            candidate = ResolveCandidate(candidate,
                target.PlayerIdentityPresentation == null ? string.Empty : target.PlayerIdentityPresentation.PlayerId);
            candidate = ResolveCandidate(candidate, defaultPlayerId ?? string.Empty);
            return candidate;
        }

        private static string ResolveCandidate(string current, string next)
        {
            string safeNext = next ?? string.Empty;
            if (string.IsNullOrEmpty(safeNext)) return current ?? string.Empty;
            if (!D20CharacterIdentityContracts.IsStableCharacterId(safeNext))
                throw new InvalidOperationException("player id cannot be recovered from an unstable reference.");
            if (!string.IsNullOrEmpty(current) &&
                !string.Equals(current, safeNext, StringComparison.Ordinal))
                throw new InvalidOperationException("player projection ids conflict during migration.");
            return safeNext;
        }

        private static bool IsValidOptionalPlayerId(string playerId)
        {
            return string.IsNullOrEmpty(playerId) ||
                D20CharacterIdentityContracts.IsStableCharacterId(playerId);
        }

        private static void EnsureSupportedVersion(int version, int current, string fieldName)
        {
            if (version < 0 || version > current)
                throw new InvalidOperationException(fieldName + " schema version is unsupported.");
        }
    }
}
