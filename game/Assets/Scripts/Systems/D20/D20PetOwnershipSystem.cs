// ============================================================
// D20PetOwnershipSystem —— D20-L23 宠物所有权与资格适配（纯 C#）
//
// D11PetSystem（AnimalCareSystem）仍是宠物生命、照料、位置和走失事实的
// 唯一写者。本文件只保存 D20 自有的 pet_id -> owner_id 语义映射，并把
// 领养、照料、走失和找回意图转发给 D11；跟随结果是只读资格投影。
// 这样 UI/Presentation 不会因为按钮或动画直接制造宠物事实，也不会把
// 饲料、船员关系或生态数值复制成第二个事实源。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D11;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L23 通过本适配器提出的宠物操作。</summary>
    public enum D20PetOwnershipOperation
    {
        Unknown = 0,
        Adopt = 1,
        Feed = 2,
        Clean = 3,
        Comfort = 4,
        MarkLost = 5,
        Search = 6,
        FollowEligibility = 7,
    }

    /// <summary>所有权或资格校验的稳定失败码。</summary>
    public enum D20PetOwnershipFailureCode
    {
        None = 0,
        MissingOwnerId = 1,
        InvalidWorldDay = 2,
        InvalidSaveRevision = 3,
        PetSystemMissing = 4,
        PetStateMissing = 5,
        PetReferenceMissing = 6,
        PetReferenceMismatch = 7,
        OwnershipRecordMissing = 8,
        OwnershipConflict = 9,
        FollowUnavailable = 10,
        PetLocationUnavailable = 11,
    }

    /// <summary>跟随资格的只读状态；不等同于 D11 的宠物生命状态。</summary>
    public enum D20PetFollowState
    {
        Unknown = 0,
        Eligible = 1,
        OwnerIdInvalid = 2,
        OwnershipRecordMissing = 3,
        OwnerMismatch = 4,
        PetMissing = 5,
        PetReferenceMismatch = 6,
        Unadopted = 7,
        Quarantined = 8,
        Lost = 9,
        Sheltered = 10,
        Dead = 11,
        LocationUnavailable = 12,
    }

    /// <summary>
    /// D20 自有的所有权映射。它不复制 D11PetState 的生命、照料、位置或
    /// 走失字段；这些字段每次都从 D11PetSystem 的公开快照读取。
    /// </summary>
    [Serializable]
    public sealed class D20PetOwnershipRecord
    {
        /// <summary>投影版本。</summary>
        public int SchemaVersion = D20PetOwnershipSystem.CurrentSchemaVersion;

        /// <summary>D11 返回的宠物实例稳定 ID。</summary>
        public string PetId = string.Empty;

        /// <summary>拥有该宠物的角色稳定 ID。</summary>
        public string OwnerId = string.Empty;

        /// <summary>建立映射时的逻辑存档修订。</summary>
        public long AcquiredSaveRevision;

        /// <summary>最近一次所有权投影事件引用，不承载 D11 生命事实。</summary>
        public string LastOwnershipEventId = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D20PetOwnershipRecord Clone()
        {
            return new D20PetOwnershipRecord
            {
                SchemaVersion = SchemaVersion,
                PetId = PetId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                AcquiredSaveRevision = AcquiredSaveRevision,
                LastOwnershipEventId = LastOwnershipEventId ?? string.Empty,
            };
        }

        /// <summary>验证稳定引用和投影修订，不访问或修改 D11 状态。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20PetOwnershipSystem.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 pet ownership schema_version mismatch.");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(PetId))
                throw new InvalidOperationException("D20 pet ownership pet_id is missing or unstable.");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(OwnerId))
                throw new InvalidOperationException("D20 pet ownership owner_id is missing or unstable.");
            if (AcquiredSaveRevision < 0)
                throw new InvalidOperationException("D20 pet ownership acquired_save_revision must be non-negative.");
            if (LastOwnershipEventId != null && LastOwnershipEventId.Length >
                D20CharacterIdentityContracts.ReferenceMaxLength)
                throw new InvalidOperationException("D20 pet ownership event id is too long.");
        }
    }

    /// <summary>
    /// D20-L23 的可保存投影。D11PetRuntimeSnapshot 必须由 D11 自己保存和
    /// 恢复；本对象只保存所有权映射及其 D20 版本/修订。
    /// </summary>
    [Serializable]
    public sealed class D20PetOwnershipSnapshot
    {
        /// <summary>当前投影版本。</summary>
        public int SchemaVersion = D20PetOwnershipSystem.CurrentSchemaVersion;

        /// <summary>D20 所有权投影的内部修订游标。</summary>
        public long NextRevision;

        /// <summary>宠物实例到拥有者的唯一映射。</summary>
        public List<D20PetOwnershipRecord> Records = new List<D20PetOwnershipRecord>();

        /// <summary>返回深拷贝。</summary>
        public D20PetOwnershipSnapshot Clone()
        {
            var result = new D20PetOwnershipSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextRevision = NextRevision,
            };
            if (Records != null)
                foreach (var record in Records)
                    if (record != null) result.Records.Add(record.Clone());
            return result;
        }

        /// <summary>检查版本、修订和一宠一主不变式。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20PetOwnershipSystem.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 pet ownership snapshot schema_version mismatch.");
            if (NextRevision < 0) throw new InvalidOperationException("D20 pet ownership revision cannot be negative.");

            var petIds = new HashSet<string>(StringComparer.Ordinal);
            if (Records == null) return;
            foreach (var record in Records)
            {
                if (record == null) throw new InvalidOperationException("D20 pet ownership record is required.");
                record.Validate();
                if (!petIds.Add(record.PetId))
                    throw new InvalidOperationException("D20 pet ownership contains duplicate pet_id: " + record.PetId);
            }
        }
    }

    /// <summary>跟随/交互资格的只读投影。</summary>
    [Serializable]
    public sealed class D20PetFollowEligibility
    {
        /// <summary>是否允许 D20 行为层提出跟随意图。</summary>
        public bool Eligible;

        /// <summary>所有权数据是否存在且可验证。</summary>
        public bool OwnershipDataAvailable;

        /// <summary>D11 宠物公开状态是否可读。</summary>
        public bool PetDataAvailable;

        /// <summary>稳定资格状态。</summary>
        public D20PetFollowState State;

        /// <summary>稳定失败/解释原因。</summary>
        public D20PetOwnershipFailureCode FailureCode;

        /// <summary>机器可读原因键。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>表现层可选择的本地化键。</summary>
        public string FeedbackKey = string.Empty;

        /// <summary>请求的拥有者 ID。</summary>
        public string OwnerId = string.Empty;

        /// <summary>请求的宠物 ID。</summary>
        public string PetId = string.Empty;

        /// <summary>D11 公开生命状态，仅供读取。</summary>
        public D11PetLifeState PetLifeState = D11PetLifeState.Unadopted;

        /// <summary>D11 公开位置，仅供读取。</summary>
        public D11PetLocation PetLocation = D11PetLocation.Unknown;

        /// <summary>返回防御性副本。</summary>
        public D20PetFollowEligibility Clone()
        {
            return new D20PetFollowEligibility
            {
                Eligible = Eligible,
                OwnershipDataAvailable = OwnershipDataAvailable,
                PetDataAvailable = PetDataAvailable,
                State = State,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackKey = FeedbackKey ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                PetId = PetId ?? string.Empty,
                PetLifeState = PetLifeState,
                PetLocation = PetLocation,
            };
        }
    }

    /// <summary>D20-L23 操作结果；D11 owner 的状态以 clone 形式只读返回。</summary>
    [Serializable]
    public sealed class D20PetOwnershipResult
    {
        /// <summary>结果版本。</summary>
        public int SchemaVersion = D20PetOwnershipSystem.CurrentSchemaVersion;

        /// <summary>执行的 D20 操作。</summary>
        public D20PetOwnershipOperation Operation;

        /// <summary>操作是否被 owner 接受并成功。</summary>
        public bool Succeeded;

        /// <summary>本次是否真的应用了 D11 owner 结果。</summary>
        public bool Applied;

        /// <summary>是否为相同修订的幂等重放。</summary>
        public bool Duplicate;

        /// <summary>是否拿到了 D11 的公开操作结果。</summary>
        public bool OwnerResultAvailable;

        /// <summary>稳定结果码，保留 owner 返回码或 D20 前置拒绝码。</summary>
        public string Code = string.Empty;

        /// <summary>稳定原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>D20 前置校验失败码；owner 拒绝时保持 None。</summary>
        public D20PetOwnershipFailureCode FailureCode;

        /// <summary>非契约文案只作为诊断信息，不是事实来源。</summary>
        public string Message = string.Empty;

        /// <summary>D11 事实事件引用。</summary>
        public string EventId = string.Empty;

        /// <summary>实际使用的逻辑修订。</summary>
        public long SaveRevision;

        /// <summary>操作行为者。</summary>
        public string OwnerId = string.Empty;

        /// <summary>关联宠物实例。</summary>
        public string PetId = string.Empty;

        /// <summary>宠物 owner 标识，始终是 D11 的公开 owner 名称。</summary>
        public string PetOwnerSystem = D11PetSystem.OwnerSystem;

        /// <summary>D11 宠物状态的防御性副本。</summary>
        public D11PetState PetState;

        /// <summary>D20 所有权映射的防御性副本。</summary>
        public D20PetOwnershipRecord Ownership;

        /// <summary>操作完成后的跟随资格投影。</summary>
        public D20PetFollowEligibility FollowEligibility;

        /// <summary>返回深拷贝。</summary>
        public D20PetOwnershipResult Clone()
        {
            return new D20PetOwnershipResult
            {
                SchemaVersion = SchemaVersion,
                Operation = Operation,
                Succeeded = Succeeded,
                Applied = Applied,
                Duplicate = Duplicate,
                OwnerResultAvailable = OwnerResultAvailable,
                Code = Code ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                FailureCode = FailureCode,
                Message = Message ?? string.Empty,
                EventId = EventId ?? string.Empty,
                SaveRevision = SaveRevision,
                OwnerId = OwnerId ?? string.Empty,
                PetId = PetId ?? string.Empty,
                PetOwnerSystem = PetOwnerSystem ?? string.Empty,
                PetState = PetState == null ? null : PetState.Clone(),
                Ownership = Ownership == null ? null : Ownership.Clone(),
                FollowEligibility = FollowEligibility == null ? null : FollowEligibility.Clone(),
            };
        }
    }

    /// <summary>
    /// 宠物所有权的 D20 逻辑边界。除 D20 自有所有权映射外，本类不持有
    /// 可变宠物生命/照料/位置数据；所有操作都必须经 D11PetSystem 公开面。
    /// </summary>
    public sealed class D20PetOwnershipSystem
    {
        /// <summary>D20-L23 投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>D20 投影 owner。</summary>
        public const string OwnerSystem = "D20PetOwnershipSystem";

        private readonly D11PetSystem _petSystem;
        private readonly List<D20PetOwnershipRecord> _records =
            new List<D20PetOwnershipRecord>();
        private long _nextRevision;

        /// <summary>创建只依赖公开 D11PetSystem 的所有权适配器。</summary>
        public D20PetOwnershipSystem(D11PetSystem petSystem,
            D20PetOwnershipSnapshot initialSnapshot = null)
        {
            _petSystem = petSystem;
            if (initialSnapshot != null)
            {
                var snapshot = initialSnapshot.Clone();
                snapshot.Validate();
                _nextRevision = snapshot.NextRevision;
                foreach (var record in snapshot.Records)
                    _records.Add(record.Clone());
            }
        }

        /// <summary>当前 D11 宠物快照的防御性副本。</summary>
        public D11PetState PetSnapshot
        {
            get { return _petSystem == null ? null : _petSystem.Snapshot; }
        }

        /// <summary>当前 D20 所有权投影的防御性副本。</summary>
        public D20PetOwnershipSnapshot Snapshot
        {
            get
            {
                var result = new D20PetOwnershipSnapshot { NextRevision = _nextRevision };
                foreach (var record in _records)
                    if (record != null) result.Records.Add(record.Clone());
                return result;
            }
        }

        /// <summary>捕获与 Snapshot 等价的 D20 投影。</summary>
        public D20PetOwnershipSnapshot CaptureSnapshot()
        {
            return Snapshot;
        }

        /// <summary>
        /// 恢复 D20 自有映射。D11 宠物快照应由 D11 owner 在此前恢复；本方法
        /// 不会从旧映射创建宠物，也不会修改 D11 状态。
        /// </summary>
        public void Restore(D20PetOwnershipSnapshot snapshot)
        {
            if (snapshot == null) return;
            var copy = snapshot.Clone();
            copy.Validate();
            _records.Clear();
            foreach (var record in copy.Records)
                _records.Add(record.Clone());
            _nextRevision = copy.NextRevision;
        }

        /// <summary>通过 D11 owner 领养宠物，并在成功后建立唯一所有权映射。</summary>
        public D20PetOwnershipResult Adopt(string ownerId, string definitionId,
            string sourceRef, int worldDay, long saveRevision = 0)
        {
            var validation = ValidateCommon(D20PetOwnershipOperation.Adopt, ownerId,
                string.Empty, worldDay, saveRevision, true);
            if (validation != null) return validation;

            var existingState = PetSnapshot;
            if (existingState != null && existingState.HasInstance)
            {
                var existing = FindRecord(existingState.PetId);
                if (existing != null)
                    return Reject(D20PetOwnershipOperation.Adopt, ownerId, existingState.PetId,
                        D20PetOwnershipFailureCode.OwnershipConflict,
                        existing.OwnerId == ownerId ? "pet_already_owned" : "pet_owned_by_other_actor",
                        existing.OwnerId == ownerId
                            ? "该拥有者已经登记了宠物，不能重复生成实例。"
                            : "该宠物已有其他拥有者，不能覆盖所有权映射。");
                return Reject(D20PetOwnershipOperation.Adopt, ownerId, existingState.PetId,
                    D20PetOwnershipFailureCode.OwnershipConflict, "ownership_reference_missing",
                    "D11 已存在宠物实例但 D20 没有所有权引用，必须先恢复一致的存档投影。");
            }

            long revision = ResolveRevision(saveRevision);
            var ownerResult = _petSystem.Adopt(definitionId, sourceRef, worldDay, revision);
            var result = FromOwnerResult(D20PetOwnershipOperation.Adopt, ownerId,
                string.Empty, ownerResult, revision);
            if (ownerResult == null || !ownerResult.Succeeded || ownerResult.State == null ||
                !ownerResult.State.HasInstance)
                return result;

            var state = ownerResult.State;
            var record = FindRecord(state.PetId);
            if (record == null)
            {
                record = new D20PetOwnershipRecord
                {
                    PetId = state.PetId,
                    OwnerId = ownerId,
                    AcquiredSaveRevision = ownerResult.SaveRevision > 0
                        ? ownerResult.SaveRevision : revision,
                    LastOwnershipEventId = ownerResult.EventId ?? string.Empty,
                };
                _records.Add(record);
            }
            result.PetId = state.PetId;
            result.Ownership = record.Clone();
            result.PetState = state.Clone();
            result.FollowEligibility = EvaluateFollowEligibility(ownerId, state.PetId, record, state);
            return result;
        }

        /// <summary>向 D11 owner 提出喂食意图，饲料扣除仍由 Inventory/Cargo 完成。</summary>
        public D20PetOwnershipResult Feed(string ownerId, int quantity, int worldDay,
            long saveRevision = 0)
        {
            return Feed(ownerId, string.Empty, quantity, worldDay, saveRevision);
        }

        /// <summary>带目标宠物 ID 的喂食意图。</summary>
        public D20PetOwnershipResult Feed(string ownerId, string petId, int quantity,
            int worldDay, long saveRevision = 0)
        {
            var validation = ValidateOwnedRequest(D20PetOwnershipOperation.Feed, ownerId,
                petId, worldDay, saveRevision);
            if (validation != null) return validation;
            long revision = ResolveRevision(saveRevision);
            return CompleteOwnerOperation(D20PetOwnershipOperation.Feed, ownerId, petId,
                _petSystem.Feed(quantity, worldDay, revision), revision);
        }

        /// <summary>向 D11 owner 提出清洁意图。</summary>
        public D20PetOwnershipResult Clean(string ownerId, int worldDay,
            long saveRevision = 0)
        {
            return Clean(ownerId, string.Empty, worldDay, saveRevision);
        }

        /// <summary>带目标宠物 ID 的清洁意图。</summary>
        public D20PetOwnershipResult Clean(string ownerId, string petId, int worldDay,
            long saveRevision = 0)
        {
            var validation = ValidateOwnedRequest(D20PetOwnershipOperation.Clean, ownerId,
                petId, worldDay, saveRevision);
            if (validation != null) return validation;
            long revision = ResolveRevision(saveRevision);
            return CompleteOwnerOperation(D20PetOwnershipOperation.Clean, ownerId, petId,
                _petSystem.Clean(worldDay, revision), revision);
        }

        /// <summary>向 D11 owner 提出安抚意图。</summary>
        public D20PetOwnershipResult Comfort(string ownerId, int worldDay,
            long saveRevision = 0)
        {
            return Comfort(ownerId, string.Empty, worldDay, saveRevision);
        }

        /// <summary>带目标宠物 ID 的安抚意图。</summary>
        public D20PetOwnershipResult Comfort(string ownerId, string petId, int worldDay,
            long saveRevision = 0)
        {
            var validation = ValidateOwnedRequest(D20PetOwnershipOperation.Comfort, ownerId,
                petId, worldDay, saveRevision);
            if (validation != null) return validation;
            long revision = ResolveRevision(saveRevision);
            return CompleteOwnerOperation(D20PetOwnershipOperation.Comfort, ownerId, petId,
                _petSystem.Comfort(worldDay, revision), revision);
        }

        /// <summary>向 D11 owner 提交走失事实；D20 不直接改走失状态。</summary>
        public D20PetOwnershipResult MarkLost(string ownerId, string reason, int worldDay,
            long saveRevision = 0)
        {
            return MarkLost(ownerId, string.Empty, reason, worldDay, saveRevision);
        }

        /// <summary>带目标宠物 ID 的走失意图。</summary>
        public D20PetOwnershipResult MarkLost(string ownerId, string petId, string reason,
            int worldDay, long saveRevision = 0)
        {
            var validation = ValidateOwnedRequest(D20PetOwnershipOperation.MarkLost, ownerId,
                petId, worldDay, saveRevision);
            if (validation != null) return validation;
            long revision = ResolveRevision(saveRevision);
            return CompleteOwnerOperation(D20PetOwnershipOperation.MarkLost, ownerId, petId,
                _petSystem.MarkLost(reason, worldDay, revision), revision);
        }

        /// <summary>向 D11 owner 提交确定性搜索/找回意图。</summary>
        public D20PetOwnershipResult Search(string ownerId, string searchLocationRef,
            bool hasSearchKit, int worldDay, int seed, long saveRevision = 0)
        {
            return Search(ownerId, string.Empty, searchLocationRef, hasSearchKit,
                worldDay, seed, saveRevision);
        }

        /// <summary>带目标宠物 ID 的搜索/找回意图。</summary>
        public D20PetOwnershipResult Search(string ownerId, string petId,
            string searchLocationRef, bool hasSearchKit, int worldDay, int seed,
            long saveRevision = 0)
        {
            var validation = ValidateOwnedRequest(D20PetOwnershipOperation.Search, ownerId,
                petId, worldDay, saveRevision);
            if (validation != null) return validation;
            long revision = ResolveRevision(saveRevision);
            return CompleteOwnerOperation(D20PetOwnershipOperation.Search, ownerId, petId,
                _petSystem.Search(searchLocationRef, hasSearchKit, worldDay, seed, revision), revision);
        }

        /// <summary>读取当前 D11 状态并计算跟随资格，不产生任何写入。</summary>
        public D20PetFollowEligibility EvaluateFollowEligibility(string ownerId,
            string petId = "")
        {
            var state = PetSnapshot;
            if (state == null)
                return FollowRejected(ownerId, petId, D20PetFollowState.PetMissing,
                    D20PetOwnershipFailureCode.PetSystemMissing, "pet_system_missing",
                    "pet.follow.unavailable", false, false);
            var record = FindRecord(state.PetId);
            return EvaluateFollowEligibility(ownerId, petId, record, state);
        }

        /// <summary>跟随资格的布尔便捷读取面。</summary>
        public bool CanFollow(string ownerId, string petId = "")
        {
            return EvaluateFollowEligibility(ownerId, petId).Eligible;
        }

        /// <summary>
        /// 纯值跟随资格入口，供无 D11 实例的契约测试和读模型消费。状态和
        /// 所有权记录都必须由调用方从各自公开投影提供，本方法不生成事实。
        /// </summary>
        public static D20PetFollowEligibility EvaluateFollowEligibility(string ownerId,
            string petId, D20PetOwnershipRecord ownership, D11PetState petState)
        {
            if (ownership == null)
                return EvaluateFollowEligibility(ownerId, petId, string.Empty, petState);
            if (!string.IsNullOrEmpty(petId) && !string.Equals(petId, ownership.PetId,
                StringComparison.Ordinal))
            {
                var mismatch = new D20PetFollowEligibility
                {
                    OwnerId = ownerId ?? string.Empty,
                    PetId = petId ?? string.Empty,
                };
                return FollowRejected(mismatch, D20PetFollowState.PetReferenceMismatch,
                    D20PetOwnershipFailureCode.PetReferenceMismatch, "pet_reference_mismatch",
                    "pet.follow.unavailable", true, petState != null);
            }
            var result = EvaluateFollowEligibility(ownerId,
                string.IsNullOrEmpty(petId) ? ownership.PetId : petId,
                ownership.OwnerId, petState);
            if (string.IsNullOrEmpty(result.PetId)) result.PetId = ownership.PetId;
            return result;
        }

        /// <summary>使用显式 owner ID 的纯值跟随资格入口。</summary>
        public static D20PetFollowEligibility EvaluateFollowEligibility(string ownerId,
            string petId, string registeredOwnerId, D11PetState petState)
        {
            var result = new D20PetFollowEligibility
            {
                OwnerId = ownerId ?? string.Empty,
                PetId = petId ?? string.Empty,
                State = D20PetFollowState.Unknown,
            };

            if (!D20CharacterIdentityContracts.IsStableCharacterId(ownerId))
                return FollowRejected(result, D20PetFollowState.OwnerIdInvalid,
                    D20PetOwnershipFailureCode.MissingOwnerId, "owner_id_invalid",
                    "pet.follow.invalid_owner", false, petState != null);

            if (string.IsNullOrEmpty(registeredOwnerId))
                return FollowRejected(result, D20PetFollowState.OwnershipRecordMissing,
                    D20PetOwnershipFailureCode.OwnershipRecordMissing,
                    "ownership_record_missing", "pet.follow.unavailable", false, petState != null);

            result.OwnershipDataAvailable = true;
            if (!string.Equals(ownerId, registeredOwnerId, StringComparison.Ordinal))
                return FollowRejected(result, D20PetFollowState.OwnerMismatch,
                    D20PetOwnershipFailureCode.OwnershipConflict, "owner_mismatch",
                    "pet.follow.owner_mismatch", true, petState != null);

            if (petState == null || !petState.HasInstance)
                return FollowRejected(result, D20PetFollowState.PetMissing,
                    D20PetOwnershipFailureCode.PetStateMissing, "pet_state_missing",
                    "pet.follow.unavailable", true, false);

            result.PetDataAvailable = true;
            if (string.IsNullOrEmpty(petId)) result.PetId = petState.PetId;
            if (!string.Equals(result.PetId, petState.PetId, StringComparison.Ordinal))
                return FollowRejected(result, D20PetFollowState.PetReferenceMismatch,
                    D20PetOwnershipFailureCode.PetReferenceMismatch, "pet_reference_mismatch",
                    "pet.follow.unavailable", true, true);

            result.PetLifeState = petState.LifeState;
            result.PetLocation = petState.Location;
            if (petState.Health <= 0 || petState.LifeState == D11PetLifeState.Dead)
                return FollowRejected(result, D20PetFollowState.Dead,
                    D20PetOwnershipFailureCode.FollowUnavailable, "pet_dead",
                    "pet.follow.dead", true, true);

            switch (petState.LifeState)
            {
                case D11PetLifeState.Adopted:
                    if (!IsFollowLocation(petState.Location))
                        return FollowRejected(result, D20PetFollowState.LocationUnavailable,
                            D20PetOwnershipFailureCode.PetLocationUnavailable,
                            "pet_location_unavailable", "pet.follow.location_unavailable", true, true);
                    result.Eligible = true;
                    result.State = D20PetFollowState.Eligible;
                    result.FailureCode = D20PetOwnershipFailureCode.None;
                    result.ReasonCode = "pet_follow_eligible";
                    result.FeedbackKey = "pet.follow.eligible";
                    return result;
                case D11PetLifeState.Quarantined:
                    return FollowRejected(result, D20PetFollowState.Quarantined,
                        D20PetOwnershipFailureCode.FollowUnavailable, "pet_quarantined",
                        "pet.follow.quarantined", true, true);
                case D11PetLifeState.Lost:
                    return FollowRejected(result, D20PetFollowState.Lost,
                        D20PetOwnershipFailureCode.FollowUnavailable, "pet_lost",
                        "pet.follow.lost", true, true);
                case D11PetLifeState.Sheltered:
                    return FollowRejected(result, D20PetFollowState.Sheltered,
                        D20PetOwnershipFailureCode.FollowUnavailable, "pet_sheltered",
                        "pet.follow.sheltered", true, true);
                case D11PetLifeState.Unadopted:
                    return FollowRejected(result, D20PetFollowState.Unadopted,
                        D20PetOwnershipFailureCode.FollowUnavailable, "pet_unadopted",
                        "pet.follow.unadopted", true, true);
                default:
                    return FollowRejected(result, D20PetFollowState.Unknown,
                        D20PetOwnershipFailureCode.FollowUnavailable, "pet_state_unknown",
                        "pet.follow.unavailable", true, true);
            }
        }

        /// <summary>纯值布尔资格入口。</summary>
        public static bool CanFollow(string ownerId, string petId,
            D20PetOwnershipRecord ownership, D11PetState petState)
        {
            return EvaluateFollowEligibility(ownerId, petId, ownership, petState).Eligible;
        }

        private D20PetOwnershipResult CompleteOwnerOperation(
            D20PetOwnershipOperation operation, string ownerId, string petId,
            D11PetActionResult ownerResult, long revision)
        {
            var result = FromOwnerResult(operation, ownerId, petId, ownerResult, revision);
            var state = ownerResult == null || ownerResult.State == null
                ? PetSnapshot : ownerResult.State;
            if (state != null && state.HasInstance)
            {
                result.PetId = state.PetId;
                result.PetState = state.Clone();
                var record = FindRecord(state.PetId);
                result.Ownership = record == null ? null : record.Clone();
                result.FollowEligibility = EvaluateFollowEligibility(ownerId, state.PetId, record, state);
            }
            return result;
        }

        private D20PetOwnershipResult ValidateOwnedRequest(
            D20PetOwnershipOperation operation, string ownerId, string petId,
            int worldDay, long saveRevision)
        {
            var common = ValidateCommon(operation, ownerId, petId, worldDay, saveRevision, false);
            if (common != null) return common;

            var state = PetSnapshot;
            if (state == null || !state.HasInstance)
                return Reject(operation, ownerId, petId, D20PetOwnershipFailureCode.PetStateMissing,
                    "pet_state_missing", "D11 没有可供所有权命令引用的宠物实例。");

            string requestedPetId = string.IsNullOrEmpty(petId) ? state.PetId : petId;
            if (!string.Equals(requestedPetId, state.PetId, StringComparison.Ordinal))
                return Reject(operation, ownerId, requestedPetId,
                    D20PetOwnershipFailureCode.PetReferenceMismatch, "pet_reference_mismatch",
                    "请求的宠物 ID 与 D11 当前公开实例不一致。");

            var record = FindRecord(state.PetId);
            if (record == null)
                return Reject(operation, ownerId, state.PetId,
                    D20PetOwnershipFailureCode.OwnershipRecordMissing,
                    "ownership_record_missing", "没有可验证的 D20 所有权映射，不能越权操作宠物。");
            if (!string.Equals(record.OwnerId, ownerId, StringComparison.Ordinal))
                return Reject(operation, ownerId, state.PetId,
                    D20PetOwnershipFailureCode.OwnershipConflict, "owner_mismatch",
                    "当前行为者不是 D20 所有权映射中的拥有者。");
            return null;
        }

        private D20PetOwnershipResult ValidateCommon(
            D20PetOwnershipOperation operation, string ownerId, string petId,
            int worldDay, long saveRevision, bool adoption)
        {
            if (!D20CharacterIdentityContracts.IsStableCharacterId(ownerId))
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.MissingOwnerId, "owner_id_missing",
                    "宠物所有权操作需要稳定的 owner_id。");
            if (worldDay < 0)
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.InvalidWorldDay, "world_day_invalid",
                    "世界日不能为负数。");
            if (saveRevision < 0)
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.InvalidSaveRevision, "save_revision_invalid",
                    "存档修订不能为负数。");
            if (_petSystem == null)
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.PetSystemMissing, "pet_system_missing",
                    "D11 宠物公开 owner 不可用，未生成本地宠物事实。");
            if (!adoption && !string.IsNullOrEmpty(petId) &&
                !D20CharacterIdentityContracts.IsStableCharacterId(petId))
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.PetReferenceMissing, "pet_reference_invalid",
                    "宠物 ID 不是可跨存档的稳定引用。");
            return null;
        }

        private D20PetOwnershipResult FromOwnerResult(
            D20PetOwnershipOperation operation, string ownerId, string petId,
            D11PetActionResult ownerResult, long revision)
        {
            if (ownerResult == null)
                return Reject(operation, ownerId, petId,
                    D20PetOwnershipFailureCode.PetSystemMissing, "owner_result_missing",
                    "D11 宠物 owner 没有返回公开结果。");

            var state = ownerResult.State == null ? PetSnapshot : ownerResult.State;
            var result = new D20PetOwnershipResult
            {
                Operation = operation,
                Succeeded = ownerResult.Succeeded,
                Applied = ownerResult.Applied,
                Duplicate = ownerResult.Duplicate,
                OwnerResultAvailable = true,
                Code = ownerResult.Code ?? string.Empty,
                ReasonCode = ownerResult.Code ?? string.Empty,
                FailureCode = D20PetOwnershipFailureCode.None,
                Message = ownerResult.Message ?? string.Empty,
                EventId = ownerResult.EventId ?? string.Empty,
                SaveRevision = ownerResult.SaveRevision > 0 ? ownerResult.SaveRevision : revision,
                OwnerId = ownerId ?? string.Empty,
                PetId = state == null ? (petId ?? string.Empty) : state.PetId ?? string.Empty,
                PetState = state == null ? null : state.Clone(),
            };
            if (state != null && state.HasInstance)
            {
                var record = FindRecord(state.PetId);
                result.Ownership = record == null ? null : record.Clone();
                result.FollowEligibility = EvaluateFollowEligibility(ownerId, state.PetId, record, state);
            }
            _nextRevision = Math.Max(_nextRevision, result.SaveRevision);
            return result;
        }

        private D20PetOwnershipResult Reject(D20PetOwnershipOperation operation,
            string ownerId, string petId, D20PetOwnershipFailureCode failureCode,
            string reasonCode, string message)
        {
            return new D20PetOwnershipResult
            {
                Operation = operation,
                Succeeded = false,
                Applied = false,
                Duplicate = false,
                OwnerResultAvailable = false,
                Code = reasonCode ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                FailureCode = failureCode,
                Message = message ?? string.Empty,
                OwnerId = ownerId ?? string.Empty,
                PetId = petId ?? string.Empty,
                SaveRevision = _nextRevision,
            };
        }

        private D20PetOwnershipRecord FindRecord(string petId)
        {
            if (string.IsNullOrEmpty(petId)) return null;
            foreach (var record in _records)
                if (record != null && string.Equals(record.PetId, petId, StringComparison.Ordinal))
                    return record;
            return null;
        }

        private long ResolveRevision(long requested)
        {
            if (requested > 0)
            {
                _nextRevision = Math.Max(_nextRevision, requested);
                return requested;
            }
            return ++_nextRevision;
        }

        private static bool IsFollowLocation(D11PetLocation location)
        {
            return location == D11PetLocation.Harbor ||
                location == D11PetLocation.ShipCabin ||
                location == D11PetLocation.ShipDeck;
        }

        private static D20PetFollowEligibility FollowRejected(string ownerId, string petId,
            D20PetFollowState state, D20PetOwnershipFailureCode code, string reasonCode,
            string feedbackKey, bool ownershipAvailable, bool petAvailable)
        {
            var result = new D20PetFollowEligibility
            {
                OwnerId = ownerId ?? string.Empty,
                PetId = petId ?? string.Empty,
            };
            return FollowRejected(result, state, code, reasonCode, feedbackKey,
                ownershipAvailable, petAvailable);
        }

        private static D20PetFollowEligibility FollowRejected(
            D20PetFollowEligibility result, D20PetFollowState state,
            D20PetOwnershipFailureCode code, string reasonCode, string feedbackKey,
            bool ownershipAvailable, bool petAvailable)
        {
            result.Eligible = false;
            result.OwnershipDataAvailable = ownershipAvailable;
            result.PetDataAvailable = petAvailable;
            result.State = state;
            result.FailureCode = code;
            result.ReasonCode = reasonCode ?? string.Empty;
            result.FeedbackKey = feedbackKey ?? string.Empty;
            return result;
        }

    }
}
