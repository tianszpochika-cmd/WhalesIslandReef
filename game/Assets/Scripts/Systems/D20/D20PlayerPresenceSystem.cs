// ============================================================
// D20PlayerPresenceSystem —— D20-L03 玩家语义 presence（纯 C#）
//
// 本系统只拥有玩家“在哪里/能否行动”的逻辑语义快照：港口、船上、航行、
// 不可行动。空间位置、碰撞、输入和表现均不属于本系统；所有迁移由显式
// 命令、前态和原因裁决，并保留可重放的命令回执。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>玩家在 D20 角色层的互斥语义 presence 状态。</summary>
    public enum D20PlayerPresenceState
    {
        /// <summary>尚未恢复或初始化的载体状态。</summary>
        Unknown = 0,
        /// <summary>玩家在港口侧可行动。</summary>
        Port = 1,
        /// <summary>玩家在船上可行动。</summary>
        Ship = 2,
        /// <summary>玩家随船处于航行中。</summary>
        Voyage = 3,
        /// <summary>玩家暂时不可接受普通动作，但可恢复到先前语义状态。</summary>
        Unavailable = 4,
    }

    /// <summary>玩家 presence 迁移的稳定原因。</summary>
    public enum D20PlayerPresenceTransitionReason
    {
        /// <summary>无原因，始终拒绝。</summary>
        None = 0,
        /// <summary>新建或返港后进入港口。</summary>
        EnterPort = 1,
        /// <summary>从港口登上船。</summary>
        BoardShip = 2,
        /// <summary>从船上下到港口侧。</summary>
        Disembark = 3,
        /// <summary>从船上开始航行。</summary>
        StartVoyage = 4,
        /// <summary>航行结束并返回港口。</summary>
        ReturnToPort = 5,
        /// <summary>记录不可行动，同时保留恢复前状态。</summary>
        BecomeUnavailable = 6,
        /// <summary>从不可行动恢复到保存的前态。</summary>
        Recover = 7,
        /// <summary>同一语义状态的合法刷新。</summary>
        Refresh = 8,
    }

    /// <summary>玩家 presence 命令的稳定拒绝码。</summary>
    public enum D20PlayerPresenceFailureCode
    {
        /// <summary>没有失败。</summary>
        None = 0,
        /// <summary>缺少命令 ID。</summary>
        MissingCommandId = 1,
        /// <summary>缺少或格式非法的玩家 ID。</summary>
        MissingPlayerId = 2,
        /// <summary>命令玩家 ID 与系统 owner 不一致。</summary>
        PlayerMismatch = 3,
        /// <summary>契约版本不支持。</summary>
        InvalidSchemaVersion = 4,
        /// <summary>目标状态或迁移原因不支持。</summary>
        InvalidState = 5,
        /// <summary>当前前态不允许该迁移。</summary>
        InvalidTransition = 6,
        /// <summary>进入港口语义时缺少港口稳定键。</summary>
        MissingPortId = 7,
        /// <summary>船上或航行语义时缺少船稳定键。</summary>
        MissingShipId = 8,
        /// <summary>港口键、船键和状态组合互相冲突。</summary>
        LocationConflict = 9,
        /// <summary>恢复不可行动状态时缺少可恢复前态。</summary>
        MissingResumeState = 10,
        /// <summary>命令已处理，调用方应使用原回执。</summary>
        DuplicateCommand = 11,
        /// <summary>相同命令 ID 携带了不同请求内容。</summary>
        DuplicateCommandConflict = 12,
        /// <summary>输入快照违反内部不变式。</summary>
        InvariantViolation = 13,
    }

    /// <summary>玩家 presence 命令请求，所有字段均为逻辑稳定数据。</summary>
    [Serializable]
    public sealed class D20PlayerPresenceRequest
    {
        /// <summary>跨重试不变的命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>发出命令的玩家稳定 ID。</summary>
        public string PlayerId = string.Empty;

        /// <summary>希望进入的语义状态。</summary>
        public D20PlayerPresenceState DesiredState = D20PlayerPresenceState.Unknown;

        /// <summary>状态迁移的稳定原因。</summary>
        public D20PlayerPresenceTransitionReason Reason = D20PlayerPresenceTransitionReason.None;

        /// <summary>港口稳定键；仅港口语义或从港口恢复时使用。</summary>
        public string PortId = string.Empty;

        /// <summary>船稳定键；船上、航行或对应恢复语义时使用。</summary>
        public string ShipId = string.Empty;

        /// <summary>可供下游 owner 关联的逻辑位置引用，不是空间坐标。</summary>
        public string LocationRef = string.Empty;

        /// <summary>请求发生的世界日。</summary>
        public int WorldDay;

        /// <summary>请求发生时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>造成该请求的既有事实事件引用。</summary>
        public List<string> CauseEventIds = new List<string>();

        /// <summary>返回防御性请求副本。</summary>
        public D20PlayerPresenceRequest Clone()
        {
            var result = new D20PlayerPresenceRequest
            {
                CommandId = CommandId ?? string.Empty,
                PlayerId = PlayerId ?? string.Empty,
                DesiredState = DesiredState,
                Reason = Reason,
                PortId = PortId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    /// <summary>已裁决的 presence 变化事实事件。</summary>
    [Serializable]
    public sealed class D20PlayerPresenceEvent
    {
        /// <summary>由命令 ID 确定性派生的事件 ID。</summary>
        public string EventId = string.Empty;

        /// <summary>产生该事件的命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>事件 owner。</summary>
        public string OwnerSystem = D20PlayerPresenceSystem.OwnerSystem;

        /// <summary>变化前状态。</summary>
        public D20PlayerPresenceState PreviousState;

        /// <summary>变化后状态。</summary>
        public D20PlayerPresenceState CurrentState;

        /// <summary>事件关联的玩家 ID。</summary>
        public string PlayerId = string.Empty;

        /// <summary>状态关联的港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>状态关联的船稳定键。</summary>
        public string ShipId = string.Empty;

        /// <summary>事件发生的世界日。</summary>
        public int WorldDay;

        /// <summary>事件发生时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>上游事实事件引用。</summary>
        public List<string> CauseEventIds = new List<string>();

        /// <summary>返回深拷贝。</summary>
        public D20PlayerPresenceEvent Clone()
        {
            var result = new D20PlayerPresenceEvent
            {
                EventId = EventId ?? string.Empty,
                CommandId = CommandId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                PreviousState = PreviousState,
                CurrentState = CurrentState,
                PlayerId = PlayerId ?? string.Empty,
                PortId = PortId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    /// <summary>presence 系统输出的可序列化命令回执。</summary>
    [Serializable]
    public sealed class D20PlayerPresenceResult
    {
        /// <summary>命令是否被接受并应用。</summary>
        public bool Accepted;

        /// <summary>命令是否命中已有回执。</summary>
        public bool Duplicate;

        /// <summary>与请求一致的命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>请求内容指纹，用于读档后的重复命令冲突检测。</summary>
        public string RequestFingerprint = string.Empty;

        /// <summary>稳定拒绝码；成功时为 None。</summary>
        public D20PlayerPresenceFailureCode FailureCode;

        /// <summary>机器可读原因键，不承载 UI 文案。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>命令执行后的状态快照。</summary>
        public D20PlayerPresenceSnapshot State;

        /// <summary>成功状态变化时产生的事实事件。</summary>
        public D20PlayerPresenceEvent Event;

        /// <summary>返回深拷贝，供幂等回放使用。</summary>
        public D20PlayerPresenceResult Clone()
        {
            return new D20PlayerPresenceResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                CommandId = CommandId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                State = State == null ? null : State.Clone(),
                Event = Event == null ? null : Event.Clone(),
            };
        }

        /// <summary>创建拒绝回执。</summary>
        public static D20PlayerPresenceResult Rejected(
            string commandId,
            D20PlayerPresenceFailureCode code,
            string reasonCode,
            D20PlayerPresenceSnapshot state)
        {
            return new D20PlayerPresenceResult
            {
                Accepted = false,
                Duplicate = false,
                CommandId = commandId ?? string.Empty,
                RequestFingerprint = string.Empty,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                State = state == null ? null : state.Clone(),
            };
        }
    }

    /// <summary>玩家 presence 的版本化只读投影和幂等回执保存载体。</summary>
    [Serializable]
    public sealed class D20PlayerPresenceSnapshot
    {
        /// <summary>当前 presence 投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>投影版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>拥有该 presence 的玩家稳定 ID。</summary>
        public string PlayerId = string.Empty;

        /// <summary>当前互斥语义状态。</summary>
        public D20PlayerPresenceState State = D20PlayerPresenceState.Unknown;

        /// <summary>进入不可行动前的状态，用于安全恢复。</summary>
        public D20PlayerPresenceState ResumeState = D20PlayerPresenceState.Unknown;

        /// <summary>当前状态关联的港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>当前状态关联的船稳定键。</summary>
        public string ShipId = string.Empty;

        /// <summary>逻辑位置引用。</summary>
        public string LocationRef = string.Empty;

        /// <summary>最近一次成功命令 ID。</summary>
        public string LastCommandId = string.Empty;

        /// <summary>最近一次成功事件 ID。</summary>
        public string LastEventId = string.Empty;

        /// <summary>最近一次成功命令的世界日。</summary>
        public int WorldDay;

        /// <summary>最近一次成功命令的存档修订。</summary>
        public long SaveRevision;

        /// <summary>可重放命令回执；用于读档后继续幂等保护。</summary>
        public List<D20PlayerPresenceResult> CommandReceipts = new List<D20PlayerPresenceResult>();

        /// <summary>返回深拷贝。</summary>
        public D20PlayerPresenceSnapshot Clone()
        {
            var result = new D20PlayerPresenceSnapshot
            {
                SchemaVersion = SchemaVersion,
                PlayerId = PlayerId ?? string.Empty,
                State = State,
                ResumeState = ResumeState,
                PortId = PortId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                LastCommandId = LastCommandId ?? string.Empty,
                LastEventId = LastEventId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CommandReceipts != null)
                foreach (var receipt in CommandReceipts)
                    if (receipt != null) result.CommandReceipts.Add(receipt.Clone());
            return result;
        }

        /// <summary>验证投影版本、ID、状态和位置语义。</summary>
        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 player presence schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(PlayerId))
                throw new InvalidOperationException("D20 player presence player_id is missing or unstable.");
            if (!D20PlayerPresenceSystem.IsSupportedState(State))
                throw new InvalidOperationException("D20 player presence state is unknown.");
            if (!D20PlayerPresenceSystem.IsSupportedState(ResumeState) &&
                ResumeState != D20PlayerPresenceState.Unknown)
                throw new InvalidOperationException("D20 player presence resume_state is unknown.");
            if (WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException("D20 player presence world_day/save_revision must be non-negative.");
            if (CommandReceipts == null)
                throw new InvalidOperationException("D20 player presence command_receipts are required.");
            D20PlayerPresenceSystem.ValidateLocation(State, PortId, ShipId);
            if (State == D20PlayerPresenceState.Unavailable &&
                !D20PlayerPresenceSystem.IsActiveState(ResumeState))
                throw new InvalidOperationException("D20 unavailable presence must retain an active resume_state.");
        }
    }

    /// <summary>
    /// 玩家语义 presence 的唯一写者。该系统不读取或写入空间/表现状态。
    /// </summary>
    public sealed class D20PlayerPresenceSystem
    {
        /// <summary>D20 presence 的唯一 owner 标识。</summary>
        public const string OwnerSystem = "D20PlayerPresenceSystem";

        private readonly D20PlayerPresenceSnapshot _state;
        private readonly Dictionary<string, D20PlayerPresenceResult> _receipts =
            new Dictionary<string, D20PlayerPresenceResult>(StringComparer.Ordinal);

        /// <summary>创建未初始化的 presence owner；首次合法命令绑定玩家 ID。</summary>
        public D20PlayerPresenceSystem()
        {
            _state = new D20PlayerPresenceSnapshot();
        }

        /// <summary>从已有投影恢复 presence owner 和命令回执。</summary>
        public D20PlayerPresenceSystem(D20PlayerPresenceSnapshot initialSnapshot)
        {
            if (initialSnapshot == null)
                throw new ArgumentNullException("initialSnapshot");
            _state = initialSnapshot.Clone();
            _state.Validate();
            RebuildReceiptIndex();
        }

        /// <summary>返回防御性只读快照。</summary>
        public D20PlayerPresenceSnapshot Snapshot
        {
            get { return _state.Clone(); }
        }

        /// <summary>应用一条 presence 命令，成功后只改变本系统自有语义投影。</summary>
        public D20PlayerPresenceResult Apply(D20PlayerPresenceRequest request)
        {
            if (request == null)
                return D20PlayerPresenceResult.Rejected(string.Empty,
                    D20PlayerPresenceFailureCode.InvariantViolation,
                    "request_missing", Snapshot);

            string commandId = request.CommandId ?? string.Empty;
            string fingerprint = Fingerprint(request);
            D20PlayerPresenceResult previous;
            if (!string.IsNullOrEmpty(commandId) && _receipts.TryGetValue(commandId, out previous))
            {
                if (!string.Equals(fingerprint, FingerprintForReceipt(previous), StringComparison.Ordinal))
                {
                    return D20PlayerPresenceResult.Rejected(commandId,
                        D20PlayerPresenceFailureCode.DuplicateCommandConflict,
                        "duplicate_command_conflict", Snapshot);
                }

                var duplicate = previous.Clone();
                duplicate.Duplicate = true;
                duplicate.FailureCode = D20PlayerPresenceFailureCode.DuplicateCommand;
                duplicate.ReasonCode = "duplicate_command";
                return duplicate;
            }

            D20PlayerPresenceFailureCode code;
            string reasonCode;
            if (!ValidateRequest(request, out code, out reasonCode))
                return D20PlayerPresenceResult.Rejected(commandId, code, reasonCode, Snapshot);

            D20PlayerPresenceState nextState = request.DesiredState;
            D20PlayerPresenceState previousState = _state.State;
            D20PlayerPresenceState nextResumeState = _state.ResumeState;
            if (nextState == D20PlayerPresenceState.Unavailable)
                nextResumeState = previousState;
            else if (previousState == D20PlayerPresenceState.Unavailable)
                nextResumeState = D20PlayerPresenceState.Unknown;

            _state.State = nextState;
            _state.ResumeState = nextResumeState;
            _state.PlayerId = request.PlayerId;
            _state.PortId = request.PortId ?? string.Empty;
            _state.ShipId = request.ShipId ?? string.Empty;
            _state.LocationRef = request.LocationRef ?? string.Empty;
            _state.LastCommandId = commandId;
            _state.LastEventId = EventIdFor(commandId);
            _state.WorldDay = request.WorldDay;
            _state.SaveRevision = request.SaveRevision;

            var changeEvent = new D20PlayerPresenceEvent
            {
                EventId = _state.LastEventId,
                CommandId = commandId,
                OwnerSystem = OwnerSystem,
                PreviousState = previousState,
                CurrentState = nextState,
                PlayerId = _state.PlayerId,
                PortId = _state.PortId,
                ShipId = _state.ShipId,
                WorldDay = _state.WorldDay,
                SaveRevision = _state.SaveRevision,
            };
            if (request.CauseEventIds != null)
                changeEvent.CauseEventIds.AddRange(request.CauseEventIds);

            var result = new D20PlayerPresenceResult
            {
                Accepted = true,
                Duplicate = false,
                CommandId = commandId,
                RequestFingerprint = fingerprint,
                FailureCode = D20PlayerPresenceFailureCode.None,
                ReasonCode = ReasonCodeFor(request.Reason),
                State = Snapshot,
                Event = changeEvent,
            };
            _receipts[commandId] = result.Clone();
            ReplaceReceipt(_state, result);
            return result;
        }

        /// <summary>Apply 的语义别名，供命令编排层使用。</summary>
        public D20PlayerPresenceResult Transition(D20PlayerPresenceRequest request)
        {
            return Apply(request);
        }

        /// <summary>捕获可交给 SaveSystem 组合器的 D20 presence 投影。</summary>
        public D20PlayerPresenceSnapshot CaptureSnapshot()
        {
            return Snapshot;
        }

        /// <summary>恢复 D20 自有 presence 投影，不推断任何外部实体或空间事实。</summary>
        public void Restore(D20PlayerPresenceSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            CopyInto(_state, snapshot);
            _receipts.Clear();
            RebuildReceiptIndex();
        }

        /// <summary>判断状态枚举是否属于当前版本。</summary>
        public static bool IsSupportedState(D20PlayerPresenceState state)
        {
            return state >= D20PlayerPresenceState.Port &&
                   state <= D20PlayerPresenceState.Unavailable;
        }

        /// <summary>判断状态是否是可恢复的活动 presence。</summary>
        public static bool IsActiveState(D20PlayerPresenceState state)
        {
            return state == D20PlayerPresenceState.Port ||
                   state == D20PlayerPresenceState.Ship ||
                   state == D20PlayerPresenceState.Voyage;
        }

        /// <summary>验证状态与稳定港口/船引用的互斥组合。</summary>
        public static void ValidateLocation(D20PlayerPresenceState state, string portId, string shipId)
        {
            string safePortId = portId ?? string.Empty;
            string safeShipId = shipId ?? string.Empty;
            if (state == D20PlayerPresenceState.Port)
            {
                if (string.IsNullOrWhiteSpace(safePortId) || !string.IsNullOrEmpty(safeShipId))
                    throw new InvalidOperationException("D20 port presence requires port_id and no ship_id.");
            }
            else if (state == D20PlayerPresenceState.Ship || state == D20PlayerPresenceState.Voyage)
            {
                if (string.IsNullOrWhiteSpace(safeShipId) || !string.IsNullOrEmpty(safePortId))
                    throw new InvalidOperationException("D20 ship/voyage presence requires ship_id and no port_id.");
            }
            else if (state == D20PlayerPresenceState.Unavailable)
            {
                bool hasPort = !string.IsNullOrWhiteSpace(safePortId);
                bool hasShip = !string.IsNullOrWhiteSpace(safeShipId);
                if (hasPort == hasShip)
                    throw new InvalidOperationException("D20 unavailable presence must retain exactly one location owner.");
            }
            else
            {
                if (!string.IsNullOrEmpty(safePortId) || !string.IsNullOrEmpty(safeShipId))
                    throw new InvalidOperationException("D20 unknown presence cannot carry a location owner.");
            }
        }

        private bool ValidateRequest(D20PlayerPresenceRequest request,
            out D20PlayerPresenceFailureCode code, out string reasonCode)
        {
            code = D20PlayerPresenceFailureCode.None;
            reasonCode = string.Empty;
            if (string.IsNullOrWhiteSpace(request.CommandId))
            {
                code = D20PlayerPresenceFailureCode.MissingCommandId;
                reasonCode = "missing_command_id";
                return false;
            }
            if (!D20CharacterIdentityContracts.IsStableCharacterId(request.PlayerId))
            {
                code = D20PlayerPresenceFailureCode.MissingPlayerId;
                reasonCode = "missing_player_id";
                return false;
            }
            if (!string.IsNullOrEmpty(_state.PlayerId) &&
                !string.Equals(_state.PlayerId, request.PlayerId, StringComparison.Ordinal))
            {
                code = D20PlayerPresenceFailureCode.PlayerMismatch;
                reasonCode = "player_mismatch";
                return false;
            }
            if (!IsSupportedState(request.DesiredState) ||
                !Enum.IsDefined(typeof(D20PlayerPresenceTransitionReason), request.Reason) ||
                request.Reason == D20PlayerPresenceTransitionReason.None)
            {
                code = D20PlayerPresenceFailureCode.InvalidState;
                reasonCode = "invalid_presence_request";
                return false;
            }
            if (request.WorldDay < 0 || request.SaveRevision < 0)
            {
                code = D20PlayerPresenceFailureCode.InvariantViolation;
                reasonCode = "negative_revision";
                return false;
            }
            try
            {
                ValidateLocation(request.DesiredState, request.PortId, request.ShipId);
            }
            catch (InvalidOperationException)
            {
                code = request.DesiredState == D20PlayerPresenceState.Port
                    ? D20PlayerPresenceFailureCode.MissingPortId
                    : request.DesiredState == D20PlayerPresenceState.Ship ||
                      request.DesiredState == D20PlayerPresenceState.Voyage
                        ? D20PlayerPresenceFailureCode.MissingShipId
                        : D20PlayerPresenceFailureCode.LocationConflict;
                reasonCode = "presence_location_conflict";
                return false;
            }

            if (!CanTransition(_state.State, request.DesiredState, request.Reason,
                _state.ResumeState))
            {
                code = request.DesiredState == D20PlayerPresenceState.Unavailable &&
                       !IsActiveState(_state.State)
                    ? D20PlayerPresenceFailureCode.MissingResumeState
                    : D20PlayerPresenceFailureCode.InvalidTransition;
                reasonCode = "invalid_presence_transition";
                return false;
            }
            return true;
        }

        /// <summary>表驱动判断 presence 是否允许按原因迁移。</summary>
        public static bool CanTransition(D20PlayerPresenceState from,
            D20PlayerPresenceState to,
            D20PlayerPresenceTransitionReason reason,
            D20PlayerPresenceState resumeState = D20PlayerPresenceState.Unknown)
        {
            if (reason == D20PlayerPresenceTransitionReason.Refresh)
                return from == to && IsSupportedState(to);
            if (reason == D20PlayerPresenceTransitionReason.EnterPort)
                return (from == D20PlayerPresenceState.Unknown ||
                        from == D20PlayerPresenceState.Voyage) &&
                       to == D20PlayerPresenceState.Port;
            if (reason == D20PlayerPresenceTransitionReason.BoardShip)
                return from == D20PlayerPresenceState.Port && to == D20PlayerPresenceState.Ship;
            if (reason == D20PlayerPresenceTransitionReason.Disembark)
                return from == D20PlayerPresenceState.Ship && to == D20PlayerPresenceState.Port;
            if (reason == D20PlayerPresenceTransitionReason.StartVoyage)
                return from == D20PlayerPresenceState.Ship && to == D20PlayerPresenceState.Voyage;
            if (reason == D20PlayerPresenceTransitionReason.ReturnToPort)
                return from == D20PlayerPresenceState.Voyage && to == D20PlayerPresenceState.Port;
            if (reason == D20PlayerPresenceTransitionReason.BecomeUnavailable)
                return IsActiveState(from) && to == D20PlayerPresenceState.Unavailable;
            if (reason == D20PlayerPresenceTransitionReason.Recover)
                return from == D20PlayerPresenceState.Unavailable &&
                       IsActiveState(resumeState) && to == resumeState;
            return false;
        }

        private static string ReasonCodeFor(D20PlayerPresenceTransitionReason reason)
        {
            return "presence_" + reason.ToString().ToLowerInvariant();
        }

        private static string EventIdFor(string commandId)
        {
            return "d20_presence_" + (commandId ?? string.Empty);
        }

        private static string Fingerprint(D20PlayerPresenceRequest request)
        {
            return (request.PlayerId ?? string.Empty) + "|" +
                   request.DesiredState + "|" + request.Reason + "|" +
                   (request.PortId ?? string.Empty) + "|" +
                   (request.ShipId ?? string.Empty) + "|" +
                   (request.LocationRef ?? string.Empty) + "|" +
                   request.WorldDay + "|" + request.SaveRevision;
        }

        private static string FingerprintForReceipt(D20PlayerPresenceResult result)
        {
            return result == null ? string.Empty : result.RequestFingerprint ?? string.Empty;
        }

        private static void ReplaceReceipt(D20PlayerPresenceSnapshot state,
            D20PlayerPresenceResult result)
        {
            if (state.CommandReceipts == null)
                state.CommandReceipts = new List<D20PlayerPresenceResult>();
            for (int i = 0; i < state.CommandReceipts.Count; i++)
            {
                if (state.CommandReceipts[i] != null &&
                    string.Equals(state.CommandReceipts[i].CommandId, result.CommandId,
                        StringComparison.Ordinal))
                {
                    state.CommandReceipts[i] = result.Clone();
                    return;
                }
            }
            state.CommandReceipts.Add(result.Clone());
        }

        private void RebuildReceiptIndex()
        {
            _receipts.Clear();
            if (_state.CommandReceipts == null) return;
            foreach (var receipt in _state.CommandReceipts)
            {
                if (receipt != null && !string.IsNullOrEmpty(receipt.CommandId))
                    _receipts[receipt.CommandId] = receipt.Clone();
            }
        }

        private static void CopyInto(D20PlayerPresenceSnapshot target,
            D20PlayerPresenceSnapshot source)
        {
            target.SchemaVersion = source.SchemaVersion;
            target.PlayerId = source.PlayerId ?? string.Empty;
            target.State = source.State;
            target.ResumeState = source.ResumeState;
            target.PortId = source.PortId ?? string.Empty;
            target.ShipId = source.ShipId ?? string.Empty;
            target.LocationRef = source.LocationRef ?? string.Empty;
            target.LastCommandId = source.LastCommandId ?? string.Empty;
            target.LastEventId = source.LastEventId ?? string.Empty;
            target.WorldDay = source.WorldDay;
            target.SaveRevision = source.SaveRevision;
            target.CommandReceipts = new List<D20PlayerPresenceResult>();
            foreach (var receipt in source.CommandReceipts)
                if (receipt != null) target.CommandReceipts.Add(receipt.Clone());
        }
    }
}
