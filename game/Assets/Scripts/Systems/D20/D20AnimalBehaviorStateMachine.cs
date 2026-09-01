// ============================================================
// D20AnimalBehaviorStateMachine —— D20-L22 动物行为状态机（纯 C#）
//
// 本状态机只拥有 D20 行为投影。它接收明确的逻辑输入并返回确定性结果，
// 不计算健康、生态压力、种群数量或空间移动，也不引用 Unity、网络、LLM、
// 系统时钟或不可控随机。D11/D4 owner 的事实必须通过公开结果输入。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>D20 动物行为状态；枚举别名保留阅读方常用的进行时写法。</summary>
    public enum D20AnimalBehaviorState
    {
        /// <summary>未初始化值。</summary>
        Unknown = 0,
        /// <summary>休息。</summary>
        Rest = 1,
        /// <summary>Rest 的语义别名。</summary>
        Resting = Rest,
        /// <summary>觅食。</summary>
        Forage = 2,
        /// <summary>Forage 的语义别名。</summary>
        Foraging = Forage,
        /// <summary>警觉。</summary>
        Alert = 3,
        /// <summary>逃离威胁。</summary>
        Flee = 4,
        /// <summary>Flee 的语义别名。</summary>
        Fleeing = Flee,
        /// <summary>跟随稳定目标。</summary>
        Follow = 5,
        /// <summary>Follow 的语义别名。</summary>
        Following = Follow,
        /// <summary>受伤，需要 owner 提供恢复事实。</summary>
        Injured = 6,
        /// <summary>已死亡，行为状态终止。</summary>
        Dead = 7,
    }

    /// <summary>驱动动物状态机的确定性输入类别。</summary>
    public enum D20AnimalBehaviorInputKind
    {
        /// <summary>无效输入。</summary>
        None = 0,
        /// <summary>请求休息。</summary>
        Rest = 1,
        /// <summary>请求觅食。</summary>
        Forage = 2,
        /// <summary>确认发现威胁并进入警觉。</summary>
        ThreatDetected = 3,
        /// <summary>请求逃离。</summary>
        Flee = 4,
        /// <summary>请求跟随稳定目标。</summary>
        Follow = 5,
        /// <summary>owner 报告受伤，不携带伤害数值。</summary>
        Injury = 6,
        /// <summary>owner 确认可以从受伤状态恢复。</summary>
        Recovery = 7,
        /// <summary>owner 确认生命周期死亡。</summary>
        Death = 8,
        /// <summary>威胁解除，警觉/逃离可回到休息。</summary>
        ThreatCleared = 9,
        /// <summary>觅食意图完成，回到休息。</summary>
        ForageCompleted = 10,
        /// <summary>跟随目标丢失，回到休息。</summary>
        FollowTargetLost = 11,
        /// <summary>取消可取消的普通行为。</summary>
        Cancel = 12,

        // 常用语义别名；数值相同不会产生第二种协议。
        /// <summary>Rest 的语义别名。</summary>
        RestRequested = Rest,
        /// <summary>Forage 的语义别名。</summary>
        ForageRequested = Forage,
        /// <summary>ThreatDetected 的语义别名。</summary>
        Alert = ThreatDetected,
        /// <summary>Flee 的语义别名。</summary>
        FleeRequested = Flee,
        /// <summary>Follow 的语义别名。</summary>
        FollowRequested = Follow,
        /// <summary>Injury 的语义别名。</summary>
        InjuryReported = Injury,
        /// <summary>Recovery 的语义别名。</summary>
        RecoveryCompleted = Recovery,
        /// <summary>Death 的语义别名。</summary>
        DeathConfirmed = Death,
        /// <summary>ForageCompleted 的语义别名。</summary>
        ForageComplete = ForageCompleted,
    }

    /// <summary>状态机输入校验码。</summary>
    public enum D20AnimalBehaviorInputValidationCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>输入为空。</summary>
        MissingInput = 1,
        /// <summary>版本不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>命令 ID 缺失或不稳定。</summary>
        InvalidCommandId = 3,
        /// <summary>动物 ID 缺失或不稳定。</summary>
        InvalidAnimalId = 4,
        /// <summary>输入动物 ID 与状态机不一致。</summary>
        AnimalIdMismatch = 5,
        /// <summary>输入类别未知。</summary>
        InvalidInputKind = 6,
        /// <summary>跟随输入缺少目标。</summary>
        MissingTargetId = 7,
        /// <summary>跟随目标引用非法或指向自身。</summary>
        InvalidTargetId = 8,
        /// <summary>因果事件引用非法。</summary>
        InvalidCauseEventId = 9,
        /// <summary>世界日非法。</summary>
        InvalidWorldDay = 10,
        /// <summary>存档修订非法。</summary>
        InvalidSaveRevision = 11,
        /// <summary>恢复输入未得到 owner 确认。</summary>
        RecoveryNotConfirmed = 12,
    }

    /// <summary>状态机结果状态。</summary>
    public enum D20AnimalBehaviorResultStatus
    {
        /// <summary>输入已接受并应用。</summary>
        Applied = 0,
        /// <summary>输入被状态或前置拒绝。</summary>
        Rejected = 1,
        /// <summary>命令已处理，返回历史回执。</summary>
        Duplicate = 2,
        /// <summary>输入有效但当前能力不可用。</summary>
        Unavailable = 3,
    }

    /// <summary>不改变输入的状态机输入 DTO。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorInput
    {
        /// <summary>行为输入契约版本。</summary>
        public int SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;

        /// <summary>跨重试不变的命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>目标动物的稳定实例 ID。</summary>
        public string AnimalId = string.Empty;

        /// <summary>确定性输入类别。</summary>
        public D20AnimalBehaviorInputKind Kind = D20AnimalBehaviorInputKind.None;

        /// <summary>Follow 使用的稳定目标引用。</summary>
        public string TargetId = string.Empty;

        /// <summary>owner 事实或上游事件的稳定因果引用。</summary>
        public string CauseEventId = string.Empty;

        /// <summary>调用方显式提供的世界日。</summary>
        public int WorldDay;

        /// <summary>调用方显式提供的存档修订。</summary>
        public long SaveRevision;

        /// <summary>Recovery 输入是否已得到事实 owner 的恢复确认。</summary>
        public bool RecoveryConfirmed;

        /// <summary>无参构造器供序列化器创建 DTO。</summary>
        public D20AnimalBehaviorInput() { }

        /// <summary>创建最小确定性行为输入。</summary>
        public static D20AnimalBehaviorInput Create(
            string commandId,
            string animalId,
            D20AnimalBehaviorInputKind kind,
            string targetId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return new D20AnimalBehaviorInput
            {
                CommandId = commandId ?? string.Empty,
                AnimalId = animalId ?? string.Empty,
                Kind = kind,
                TargetId = targetId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D20AnimalBehaviorInput Clone()
        {
            return new D20AnimalBehaviorInput
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                AnimalId = AnimalId ?? string.Empty,
                Kind = Kind,
                TargetId = TargetId ?? string.Empty,
                CauseEventId = CauseEventId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                RecoveryConfirmed = RecoveryConfirmed,
            };
        }

        /// <summary>返回用于幂等判断的确定性请求指纹。</summary>
        public string Fingerprint()
        {
            return (AnimalId ?? string.Empty) + "|" +
                   (int)Kind + "|" +
                   (TargetId ?? string.Empty) + "|" +
                   (CauseEventId ?? string.Empty) + "|" +
                   WorldDay + "|" +
                   SaveRevision + "|" +
                   (RecoveryConfirmed ? "1" : "0");
        }

        /// <summary>验证输入的稳定字段，不改变输入。</summary>
        public D20AnimalBehaviorInputValidationCode Validate(string expectedAnimalId)
        {
            if (SchemaVersion != D20AnimalIdentityContracts.CurrentSchemaVersion)
                return D20AnimalBehaviorInputValidationCode.InvalidSchemaVersion;
            if (!D20AnimalIdentityContracts.IsStableSpeciesId(CommandId))
                return D20AnimalBehaviorInputValidationCode.InvalidCommandId;
            if (!D20AnimalIdentityContracts.IsStableAnimalId(AnimalId))
                return D20AnimalBehaviorInputValidationCode.InvalidAnimalId;
            if (!string.Equals(AnimalId, expectedAnimalId, StringComparison.Ordinal))
                return D20AnimalBehaviorInputValidationCode.AnimalIdMismatch;
            if (!D20AnimalBehaviorStateMachine.IsSupportedInput(Kind))
                return D20AnimalBehaviorInputValidationCode.InvalidInputKind;
            if (Kind == D20AnimalBehaviorInputKind.Follow)
            {
                if (string.IsNullOrEmpty(TargetId))
                    return D20AnimalBehaviorInputValidationCode.MissingTargetId;
                if (!D20AnimalIdentityContracts.IsStableSpeciesId(TargetId))
                    return D20AnimalBehaviorInputValidationCode.InvalidTargetId;
                if (string.Equals(TargetId, AnimalId, StringComparison.Ordinal))
                    return D20AnimalBehaviorInputValidationCode.InvalidTargetId;
            }
            else if (!string.IsNullOrEmpty(TargetId) &&
                     !D20AnimalIdentityContracts.IsStableSpeciesId(TargetId))
            {
                return D20AnimalBehaviorInputValidationCode.InvalidTargetId;
            }
            if (!string.IsNullOrEmpty(CauseEventId) &&
                !D20AnimalIdentityContracts.IsStableSpeciesId(CauseEventId))
                return D20AnimalBehaviorInputValidationCode.InvalidCauseEventId;
            if (WorldDay < 0)
                return D20AnimalBehaviorInputValidationCode.InvalidWorldDay;
            if (SaveRevision < 0)
                return D20AnimalBehaviorInputValidationCode.InvalidSaveRevision;
            if (Kind == D20AnimalBehaviorInputKind.Recovery && !RecoveryConfirmed)
                return D20AnimalBehaviorInputValidationCode.RecoveryNotConfirmed;
            return D20AnimalBehaviorInputValidationCode.None;
        }
    }

    /// <summary>状态机应用结果；只包含 D20 行为投影和稳定失败信息。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorResult
    {
        /// <summary>契约版本。</summary>
        public int SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;

        /// <summary>原始命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>动物稳定 ID。</summary>
        public string AnimalId = string.Empty;

        /// <summary>输入类别。</summary>
        public D20AnimalBehaviorInputKind InputKind;

        /// <summary>应用前状态。</summary>
        public D20AnimalBehaviorState StateBefore;

        /// <summary>应用后状态。</summary>
        public D20AnimalBehaviorState StateAfter;

        /// <summary>结果状态。</summary>
        public D20AnimalBehaviorResultStatus Status;

        /// <summary>是否被成功应用；重复回放为 false。</summary>
        public bool Applied;

        /// <summary>是否是已有命令的回放或冲突。</summary>
        public bool Duplicate;

        /// <summary>状态是否实际改变。</summary>
        public bool StateChanged;

        /// <summary>稳定结果码。</summary>
        public string ResultCode = string.Empty;

        /// <summary>稳定失败/结果原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>成功状态投影事件的确定性 ID；拒绝时为空。</summary>
        public string EventId = string.Empty;

        /// <summary>请求携带的世界日。</summary>
        public int WorldDay;

        /// <summary>请求携带的存档修订。</summary>
        public long SaveRevision;

        /// <summary>状态机本地投影修订，单调递增且不依赖系统时钟。</summary>
        public long ProjectionRevision;

        /// <summary>成功后当前跟随目标引用。</summary>
        public string FollowTargetId = string.Empty;

        /// <summary>受伤状态是否存在显式恢复路径。</summary>
        public bool CanRecover;

        /// <summary>返回深拷贝。</summary>
        public D20AnimalBehaviorResult Clone()
        {
            return new D20AnimalBehaviorResult
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                AnimalId = AnimalId ?? string.Empty,
                InputKind = InputKind,
                StateBefore = StateBefore,
                StateAfter = StateAfter,
                Status = Status,
                Applied = Applied,
                Duplicate = Duplicate,
                StateChanged = StateChanged,
                ResultCode = ResultCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                EventId = EventId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                ProjectionRevision = ProjectionRevision,
                FollowTargetId = FollowTargetId ?? string.Empty,
                CanRecover = CanRecover,
            };
        }
    }

    /// <summary>行为状态机的可序列化快照。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorSnapshot
    {
        /// <summary>快照版本。</summary>
        public int SchemaVersion = D20AnimalIdentityContracts.CurrentSchemaVersion;

        /// <summary>状态机动物 ID。</summary>
        public string AnimalId = string.Empty;

        /// <summary>目录内容引用，仅为只读元数据。</summary>
        public string ContentId = string.Empty;

        /// <summary>当前行为状态。</summary>
        public D20AnimalBehaviorState State = D20AnimalBehaviorState.Unknown;

        /// <summary>当前跟随目标。</summary>
        public string FollowTargetId = string.Empty;

        /// <summary>状态机投影修订。</summary>
        public long ProjectionRevision;

        /// <summary>已处理命令回执，供重放保护使用。</summary>
        public List<D20AnimalBehaviorReceipt> Receipts = new List<D20AnimalBehaviorReceipt>();

        /// <summary>返回深拷贝。</summary>
        public D20AnimalBehaviorSnapshot Clone()
        {
            var result = new D20AnimalBehaviorSnapshot
            {
                SchemaVersion = SchemaVersion,
                AnimalId = AnimalId ?? string.Empty,
                ContentId = ContentId ?? string.Empty,
                State = State,
                FollowTargetId = FollowTargetId ?? string.Empty,
                ProjectionRevision = ProjectionRevision,
            };
            if (Receipts != null)
                foreach (var receipt in Receipts)
                    if (receipt != null) result.Receipts.Add(receipt.Clone());
            return result;
        }

        /// <summary>校验快照自身的稳定边界。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20AnimalIdentityContracts.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 animal behavior snapshot schema_version mismatch.");
            if (!D20AnimalIdentityContracts.IsStableAnimalId(AnimalId))
                throw new InvalidOperationException("D20 animal behavior snapshot animal_id is invalid.");
            if (!D20AnimalBehaviorStateMachine.IsSupportedState(State))
                throw new InvalidOperationException("D20 animal behavior snapshot state is invalid.");
            if (State == D20AnimalBehaviorState.Follow &&
                !D20AnimalIdentityContracts.IsStableSpeciesId(FollowTargetId))
                throw new InvalidOperationException("D20 animal behavior snapshot follow_target_id is required.");
            if (State != D20AnimalBehaviorState.Follow && !string.IsNullOrEmpty(FollowTargetId))
                throw new InvalidOperationException("D20 animal behavior snapshot has a stale follow target.");
            if (ProjectionRevision < 0)
                throw new InvalidOperationException("D20 animal behavior snapshot projection_revision is invalid.");
            if (Receipts != null)
            {
                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (var receipt in Receipts)
                {
                    if (receipt == null || !D20AnimalIdentityContracts.IsStableSpeciesId(receipt.CommandId) ||
                        !ids.Add(receipt.CommandId) || receipt.Result == null)
                        throw new InvalidOperationException("D20 animal behavior snapshot receipt is invalid.");
                }
            }
        }
    }

    /// <summary>命令回执的内部可序列化投影。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorReceipt
    {
        /// <summary>命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>首次接收时的输入指纹。</summary>
        public string Fingerprint = string.Empty;

        /// <summary>首次处理结果。</summary>
        public D20AnimalBehaviorResult Result;

        /// <summary>返回深拷贝。</summary>
        public D20AnimalBehaviorReceipt Clone()
        {
            return new D20AnimalBehaviorReceipt
            {
                CommandId = CommandId ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
                Result = Result == null ? null : Result.Clone(),
            };
        }
    }

    /// <summary>
    /// D20-L22 纯逻辑行为状态机。所有迁移都由显式输入决定，不能由动画、
    /// 距离、碰撞、Transform 或环境数值隐式触发。
    /// </summary>
    public sealed class D20AnimalBehaviorStateMachine
    {
        /// <summary>状态机行为 owner 标识。</summary>
        public const string OwnerSystem = "D20AnimalBehaviorStateMachine";

        private readonly string _animalId;
        private readonly D20AnimalSpeciesDefinition _species;
        private readonly Dictionary<string, D20AnimalBehaviorReceipt> _receipts =
            new Dictionary<string, D20AnimalBehaviorReceipt>(StringComparer.Ordinal);
        private D20AnimalBehaviorState _state;
        private string _followTargetId = string.Empty;
        private long _projectionRevision;

        /// <summary>使用安全占位动物 ID 的无参构造器，便于纯逻辑测试。</summary>
        public D20AnimalBehaviorStateMachine()
            : this("animal:unassigned", null, D20AnimalBehaviorState.Rest, "")
        {
        }

        /// <summary>创建一个不依赖外部运行时对象的行为状态机。</summary>
        public D20AnimalBehaviorStateMachine(
            string animalId,
            D20AnimalSpeciesDefinition species = null,
            D20AnimalBehaviorState initialState = D20AnimalBehaviorState.Rest,
            string followTargetId = "")
        {
            if (!D20AnimalIdentityContracts.IsStableAnimalId(animalId))
                throw new ArgumentException("D20 animal_id is invalid.", "animalId");
            if (!IsSupportedState(initialState) || initialState == D20AnimalBehaviorState.Unknown)
                throw new ArgumentException("D20 animal initial state is invalid.", "initialState");
            if (species != null)
            {
                species.Validate();
                _species = species.Clone();
                if (!string.Equals(_species.ContentId, string.Empty, StringComparison.Ordinal) &&
                    !D20AnimalIdentityContracts.IsStableAnimalContentId(_species.ContentId))
                    throw new ArgumentException("D20 animal species content_id is invalid.", "species");
            }
            _animalId = animalId;
            _state = initialState;
            if (initialState == D20AnimalBehaviorState.Follow)
            {
                if (!D20AnimalIdentityContracts.IsStableSpeciesId(followTargetId) ||
                    string.Equals(animalId, followTargetId, StringComparison.Ordinal))
                    throw new ArgumentException("D20 animal follow target is invalid.", "followTargetId");
                _followTargetId = followTargetId;
            }
            else if (!string.IsNullOrEmpty(followTargetId))
            {
                throw new ArgumentException("A non-following animal cannot have a follow target.", "followTargetId");
            }
        }

        /// <summary>状态机动物 ID。</summary>
        public string AnimalId { get { return _animalId; } }

        /// <summary>当前 D20 行为状态。</summary>
        public D20AnimalBehaviorState CurrentState { get { return _state; } }

        /// <summary>CurrentState 的兼容别名。</summary>
        public D20AnimalBehaviorState State { get { return _state; } }

        /// <summary>当前跟随目标，只能在 Follow 状态存在。</summary>
        public string FollowTargetId { get { return _followTargetId; } }

        /// <summary>当前本地投影修订。</summary>
        public long ProjectionRevision { get { return _projectionRevision; } }

        /// <summary>构造时提供的只读物种元数据副本。</summary>
        public D20AnimalSpeciesDefinition Species
        {
            get { return _species == null ? null : _species.Clone(); }
        }

        /// <summary>当前受伤状态是否有能力标签支持的恢复路径。</summary>
        public bool CanRecover
        {
            get
            {
                return _state == D20AnimalBehaviorState.Injured && HasAbility(D20AnimalBehaviorAbility.Recover);
            }
        }

        /// <summary>判断状态是否是终止状态。</summary>
        public static bool IsTerminalState(D20AnimalBehaviorState state)
        {
            return state == D20AnimalBehaviorState.Dead;
        }

        /// <summary>判断状态枚举是否受支持。</summary>
        public static bool IsSupportedState(D20AnimalBehaviorState state)
        {
            return state >= D20AnimalBehaviorState.Rest && state <= D20AnimalBehaviorState.Dead;
        }

        /// <summary>判断输入枚举是否受支持。</summary>
        public static bool IsSupportedInput(D20AnimalBehaviorInputKind kind)
        {
            return kind >= D20AnimalBehaviorInputKind.Rest &&
                   kind <= D20AnimalBehaviorInputKind.Cancel;
        }

        /// <summary>
        /// 查询不考虑物种能力时的合法迁移。Follow 需要由调用方另行提供目标。
        /// </summary>
        public static bool CanTransition(
            D20AnimalBehaviorState from,
            D20AnimalBehaviorInputKind inputKind)
        {
            D20AnimalBehaviorState ignored;
            string ignoredReason;
            return TryGetTransition(from, inputKind, true, out ignored, out ignoredReason);
        }

        /// <summary>查询带恢复确认的合法迁移。</summary>
        public static bool CanTransition(
            D20AnimalBehaviorState from,
            D20AnimalBehaviorInputKind inputKind,
            bool recoveryConfirmed)
        {
            D20AnimalBehaviorState ignored;
            string ignoredReason;
            return TryGetTransition(from, inputKind, recoveryConfirmed, out ignored, out ignoredReason);
        }

        /// <summary>应用行为输入，重复命令只回放已有结果。</summary>
        public D20AnimalBehaviorResult Apply(D20AnimalBehaviorInput input)
        {
            if (input == null)
            {
                return Rejected(null, D20AnimalBehaviorResultStatus.Rejected,
                    "behavior_input_missing", D20AnimalBehaviorInputValidationCode.MissingInput);
            }

            D20AnimalBehaviorInputValidationCode validation = input.Validate(_animalId);
            if (validation != D20AnimalBehaviorInputValidationCode.None)
            {
                return Rejected(input, validation == D20AnimalBehaviorInputValidationCode.RecoveryNotConfirmed
                        ? D20AnimalBehaviorResultStatus.Unavailable
                        : D20AnimalBehaviorResultStatus.Rejected,
                    ReasonFor(validation), validation);
            }

            D20AnimalBehaviorReceipt receipt;
            if (_receipts.TryGetValue(input.CommandId, out receipt))
            {
                if (string.Equals(receipt.Fingerprint, input.Fingerprint(), StringComparison.Ordinal))
                {
                    var duplicate = receipt.Result.Clone();
                    duplicate.Duplicate = true;
                    duplicate.Applied = false;
                    duplicate.Status = D20AnimalBehaviorResultStatus.Duplicate;
                    duplicate.ResultCode = "duplicate_replay";
                    duplicate.ReasonCode = "behavior_command_replayed";
                    return duplicate;
                }

                var conflict = BaseResult(input);
                conflict.Duplicate = true;
                conflict.Status = D20AnimalBehaviorResultStatus.Duplicate;
                conflict.ResultCode = "duplicate_command_conflict";
                conflict.ReasonCode = "behavior_command_id_reused_with_different_input";
                conflict.StateAfter = _state;
                conflict.FollowTargetId = _followTargetId;
                conflict.CanRecover = CanRecover;
                return conflict;
            }

            if (!HasRequiredAbility(input.Kind))
            {
                var unavailable = Rejected(input, D20AnimalBehaviorResultStatus.Unavailable,
                    "behavior_ability_unavailable", D20AnimalBehaviorInputValidationCode.None);
                StoreReceipt(input, unavailable);
                return unavailable;
            }

            D20AnimalBehaviorState nextState;
            string transitionReason;
            if (!TryGetTransition(_state, input.Kind, input.RecoveryConfirmed,
                    out nextState, out transitionReason))
            {
                var rejected = Rejected(input, D20AnimalBehaviorResultStatus.Rejected,
                    transitionReason, D20AnimalBehaviorInputValidationCode.None);
                StoreReceipt(input, rejected);
                return rejected;
            }

            D20AnimalBehaviorState before = _state;
            string previousFollowTarget = _followTargetId;
            string nextFollowTarget = _followTargetId;
            if (input.Kind == D20AnimalBehaviorInputKind.Follow)
                nextFollowTarget = input.TargetId;
            else if (nextState != D20AnimalBehaviorState.Follow)
                nextFollowTarget = string.Empty;

            bool stateChanged = before != nextState ||
                                !string.Equals(previousFollowTarget, nextFollowTarget, StringComparison.Ordinal);
            _state = nextState;
            _followTargetId = nextFollowTarget;
            _projectionRevision++;

            var result = new D20AnimalBehaviorResult
            {
                CommandId = input.CommandId ?? string.Empty,
                AnimalId = _animalId,
                InputKind = input.Kind,
                StateBefore = before,
                StateAfter = _state,
                Status = D20AnimalBehaviorResultStatus.Applied,
                Applied = true,
                Duplicate = false,
                StateChanged = stateChanged,
                ResultCode = stateChanged ? "behavior_state_changed" : "behavior_state_unchanged",
                ReasonCode = transitionReason,
                EventId = BuildEventId(_animalId, input.CommandId),
                WorldDay = input.WorldDay,
                SaveRevision = input.SaveRevision,
                ProjectionRevision = _projectionRevision,
                FollowTargetId = _followTargetId,
                CanRecover = CanRecover,
            };

            StoreReceipt(input, result);
            return result.Clone();
        }

        /// <summary>Transition 的语义别名，保持唯一状态迁移入口。</summary>
        public D20AnimalBehaviorResult Transition(D20AnimalBehaviorInput input)
        {
            return Apply(input);
        }

        /// <summary>按参数创建并应用输入，方便纯逻辑 smoke。</summary>
        public D20AnimalBehaviorResult Apply(
            D20AnimalBehaviorInputKind kind,
            string commandId,
            string targetId = "",
            int worldDay = 0,
            long saveRevision = 0,
            bool recoveryConfirmed = false,
            string causeEventId = "")
        {
            var input = D20AnimalBehaviorInput.Create(commandId, _animalId, kind,
                targetId, worldDay, saveRevision);
            input.RecoveryConfirmed = recoveryConfirmed;
            input.CauseEventId = causeEventId ?? string.Empty;
            return Apply(input);
        }

        /// <summary>捕获可重放的行为快照。</summary>
        public D20AnimalBehaviorSnapshot CaptureSnapshot()
        {
            var result = new D20AnimalBehaviorSnapshot
            {
                AnimalId = _animalId,
                ContentId = _species == null ? string.Empty : _species.ContentId,
                State = _state,
                FollowTargetId = _followTargetId,
                ProjectionRevision = _projectionRevision,
            };
            foreach (var pair in _receipts)
                result.Receipts.Add(pair.Value.Clone());
            result.Receipts.Sort(delegate(D20AnimalBehaviorReceipt left, D20AnimalBehaviorReceipt right)
            {
                return string.Compare(left.CommandId, right.CommandId, StringComparison.Ordinal);
            });
            return result;
        }

        /// <summary>
        /// 恢复 D20 自有快照；不从 Presentation、Prefab 或外部 owner 私有字段推断状态。
        /// </summary>
        public void Restore(D20AnimalBehaviorSnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Validate();
            if (!string.Equals(snapshot.AnimalId, _animalId, StringComparison.Ordinal))
                throw new InvalidOperationException("D20 animal behavior snapshot animal_id mismatch.");
            if (_species != null && !string.IsNullOrEmpty(snapshot.ContentId) &&
                !string.Equals(snapshot.ContentId, _species.ContentId, StringComparison.Ordinal))
                throw new InvalidOperationException("D20 animal behavior snapshot content_id mismatch.");

            _state = snapshot.State;
            _followTargetId = snapshot.FollowTargetId ?? string.Empty;
            _projectionRevision = snapshot.ProjectionRevision;
            _receipts.Clear();
            if (snapshot.Receipts != null)
            {
                foreach (var receipt in snapshot.Receipts)
                {
                    if (receipt == null || receipt.Result == null ||
                        !string.Equals(receipt.Result.AnimalId, _animalId, StringComparison.Ordinal))
                        throw new InvalidOperationException("D20 animal behavior snapshot receipt animal_id mismatch.");
                    _receipts.Add(receipt.CommandId, receipt.Clone());
                }
            }
        }

        private bool HasAbility(D20AnimalBehaviorAbility ability)
        {
            return _species == null || (_species.Capabilities & ability) != 0;
        }

        private bool HasRequiredAbility(D20AnimalBehaviorInputKind kind)
        {
            switch (kind)
            {
                case D20AnimalBehaviorInputKind.Rest:
                    return HasAbility(D20AnimalBehaviorAbility.Rest);
                case D20AnimalBehaviorInputKind.Forage:
                    return HasAbility(D20AnimalBehaviorAbility.Forage);
                case D20AnimalBehaviorInputKind.ThreatDetected:
                    return HasAbility(D20AnimalBehaviorAbility.Alert);
                case D20AnimalBehaviorInputKind.Flee:
                    return HasAbility(D20AnimalBehaviorAbility.Flee);
                case D20AnimalBehaviorInputKind.Follow:
                    return HasAbility(D20AnimalBehaviorAbility.Follow);
                case D20AnimalBehaviorInputKind.Recovery:
                    return HasAbility(D20AnimalBehaviorAbility.Recover);
                default:
                    return true;
            }
        }

        private static bool TryGetTransition(
            D20AnimalBehaviorState from,
            D20AnimalBehaviorInputKind kind,
            bool recoveryConfirmed,
            out D20AnimalBehaviorState next,
            out string reason)
        {
            next = from;
            reason = "behavior_transition_rejected";
            if (!IsSupportedState(from) || !IsSupportedInput(kind)) return false;
            if (from == D20AnimalBehaviorState.Dead) return false;

            switch (kind)
            {
                case D20AnimalBehaviorInputKind.Rest:
                    if (from == D20AnimalBehaviorState.Rest ||
                        from == D20AnimalBehaviorState.Forage ||
                        from == D20AnimalBehaviorState.Follow)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = from == next ? "already_resting" : "behavior_rest_requested";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Forage:
                    if (from == D20AnimalBehaviorState.Rest || from == D20AnimalBehaviorState.Forage)
                    {
                        next = D20AnimalBehaviorState.Forage;
                        reason = from == next ? "already_foraging" : "behavior_forage_requested";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.ThreatDetected:
                    if (from == D20AnimalBehaviorState.Rest ||
                        from == D20AnimalBehaviorState.Forage ||
                        from == D20AnimalBehaviorState.Follow ||
                        from == D20AnimalBehaviorState.Alert)
                    {
                        next = D20AnimalBehaviorState.Alert;
                        reason = from == next ? "already_alert" : "behavior_threat_detected";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Flee:
                    if (from == D20AnimalBehaviorState.Rest ||
                        from == D20AnimalBehaviorState.Forage ||
                        from == D20AnimalBehaviorState.Alert ||
                        from == D20AnimalBehaviorState.Follow ||
                        from == D20AnimalBehaviorState.Flee)
                    {
                        next = D20AnimalBehaviorState.Flee;
                        reason = from == next ? "already_fleeing" : "behavior_flee_requested";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Follow:
                    if (from == D20AnimalBehaviorState.Rest ||
                        from == D20AnimalBehaviorState.Forage ||
                        from == D20AnimalBehaviorState.Follow)
                    {
                        next = D20AnimalBehaviorState.Follow;
                        reason = from == next ? "follow_target_updated" : "behavior_follow_requested";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Injury:
                    if (from != D20AnimalBehaviorState.Dead)
                    {
                        next = D20AnimalBehaviorState.Injured;
                        reason = from == next ? "already_injured" : "behavior_injury_reported";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Recovery:
                    if (from == D20AnimalBehaviorState.Injured && recoveryConfirmed)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = "behavior_recovery_confirmed";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Death:
                    if (from != D20AnimalBehaviorState.Dead)
                    {
                        next = D20AnimalBehaviorState.Dead;
                        reason = "behavior_death_confirmed";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.ThreatCleared:
                    if (from == D20AnimalBehaviorState.Alert || from == D20AnimalBehaviorState.Flee)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = "behavior_threat_cleared";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.ForageCompleted:
                    if (from == D20AnimalBehaviorState.Forage)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = "behavior_forage_completed";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.FollowTargetLost:
                    if (from == D20AnimalBehaviorState.Follow)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = "behavior_follow_target_lost";
                        return true;
                    }
                    return false;

                case D20AnimalBehaviorInputKind.Cancel:
                    if (from == D20AnimalBehaviorState.Rest ||
                        from == D20AnimalBehaviorState.Forage ||
                        from == D20AnimalBehaviorState.Follow)
                    {
                        next = D20AnimalBehaviorState.Rest;
                        reason = from == next ? "behavior_cancel_noop" : "behavior_cancelled";
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        private D20AnimalBehaviorResult BaseResult(D20AnimalBehaviorInput input)
        {
            return new D20AnimalBehaviorResult
            {
                CommandId = input == null ? string.Empty : input.CommandId ?? string.Empty,
                AnimalId = _animalId,
                InputKind = input == null ? D20AnimalBehaviorInputKind.None : input.Kind,
                StateBefore = _state,
                StateAfter = _state,
                WorldDay = input == null ? 0 : input.WorldDay,
                SaveRevision = input == null ? 0 : input.SaveRevision,
                ProjectionRevision = _projectionRevision,
                FollowTargetId = _followTargetId,
                CanRecover = CanRecover,
            };
        }

        private D20AnimalBehaviorResult Rejected(
            D20AnimalBehaviorInput input,
            D20AnimalBehaviorResultStatus status,
            string reasonCode,
            D20AnimalBehaviorInputValidationCode validation)
        {
            var result = BaseResult(input);
            result.Status = status;
            result.Applied = false;
            result.Duplicate = false;
            result.StateChanged = false;
            result.ResultCode = validation == D20AnimalBehaviorInputValidationCode.None
                ? "behavior_input_rejected" : "behavior_input_invalid";
            result.ReasonCode = reasonCode ?? string.Empty;
            return result;
        }

        private void StoreReceipt(D20AnimalBehaviorInput input, D20AnimalBehaviorResult result)
        {
            if (input == null || result == null || string.IsNullOrEmpty(input.CommandId)) return;
            _receipts[input.CommandId] = new D20AnimalBehaviorReceipt
            {
                CommandId = input.CommandId,
                Fingerprint = input.Fingerprint(),
                Result = result.Clone(),
            };
        }

        private static string BuildEventId(string animalId, string commandId)
        {
            return "event_d20_animal_behavior:" + (animalId ?? string.Empty) + ":" + (commandId ?? string.Empty);
        }

        private static string ReasonFor(D20AnimalBehaviorInputValidationCode code)
        {
            switch (code)
            {
                case D20AnimalBehaviorInputValidationCode.MissingInput: return "behavior_input_missing";
                case D20AnimalBehaviorInputValidationCode.InvalidSchemaVersion: return "behavior_schema_version_mismatch";
                case D20AnimalBehaviorInputValidationCode.InvalidCommandId: return "behavior_command_id_invalid";
                case D20AnimalBehaviorInputValidationCode.InvalidAnimalId: return "behavior_animal_id_invalid";
                case D20AnimalBehaviorInputValidationCode.AnimalIdMismatch: return "behavior_animal_id_mismatch";
                case D20AnimalBehaviorInputValidationCode.InvalidInputKind: return "behavior_input_kind_invalid";
                case D20AnimalBehaviorInputValidationCode.MissingTargetId: return "behavior_follow_target_missing";
                case D20AnimalBehaviorInputValidationCode.InvalidTargetId: return "behavior_follow_target_invalid";
                case D20AnimalBehaviorInputValidationCode.InvalidCauseEventId: return "behavior_cause_event_invalid";
                case D20AnimalBehaviorInputValidationCode.InvalidWorldDay: return "behavior_world_day_invalid";
                case D20AnimalBehaviorInputValidationCode.InvalidSaveRevision: return "behavior_save_revision_invalid";
                case D20AnimalBehaviorInputValidationCode.RecoveryNotConfirmed: return "behavior_recovery_not_confirmed";
                default: return "behavior_input_invalid";
            }
        }
    }
}
