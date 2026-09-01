// ============================================================
// D21AnchorMooringSystem —— D21-L07 锚泊与船侧系泊（纯 C#）
//
// 本系统只拥有船侧锚泊/系泊状态和确定性拖锚风险。港口、泊位和许可事实
// 由 D19 提供公开结果；本文件只接收 opaque ID 与 permission，不写港口状态。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>船侧锚状态。</summary>
    public enum D21AnchorState
    {
        /// <summary>尚未下锚。</summary>
        Unanchored = 0,
        /// <summary>正在下锚。</summary>
        Deploying = 1,
        /// <summary>锚已抓底并保持。</summary>
        Holding = 2,
        /// <summary>锚正在走锚。</summary>
        Dragging = 3,
        /// <summary>正在收锚。</summary>
        Recovering = 4,
        /// <summary>已完成收锚。</summary>
        Released = 5,
        /// <summary>收锚失败且锚已丢失。</summary>
        Lost = 6,
        /// <summary>锚不可用。</summary>
        Disabled = 7,
    }

    /// <summary>船侧系泊状态。</summary>
    public enum D21MooringState
    {
        /// <summary>没有系泊。</summary>
        Unmoored = 0,
        /// <summary>正在驶向泊位。</summary>
        Approaching = 1,
        /// <summary>正在收紧缆绳。</summary>
        Securing = 2,
        /// <summary>已完成系泊。</summary>
        Moored = 3,
        /// <summary>系泊正在断开。</summary>
        Breaking = 4,
        /// <summary>已释放系泊。</summary>
        Released = 5,
    }

    /// <summary>逻辑海床类别。</summary>
    public enum D21SeabedKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>泥底。</summary>
        Mud = 1,
        /// <summary>沙底。</summary>
        Sand = 2,
        /// <summary>砾石底。</summary>
        Gravel = 3,
        /// <summary>珊瑚底。</summary>
        Coral = 4,
        /// <summary>岩石底。</summary>
        Rock = 5,
    }

    /// <summary>锚泊和系泊命令类别。</summary>
    public enum D21AnchorMooringCommandKind
    {
        /// <summary>未初始化。</summary>
        None = 0,
        /// <summary>开始下锚。</summary>
        Deploy = 1,
        /// <summary>完成下锚并判定是否走锚。</summary>
        CompleteDeployment = 2,
        /// <summary>重新评估当前锚的抓持。</summary>
        ReevaluateHolding = 3,
        /// <summary>开始收锚。</summary>
        BeginRecovery = 4,
        /// <summary>完成收锚。</summary>
        CompleteRecovery = 5,
        /// <summary>开始驶向系泊点。</summary>
        BeginMooring = 6,
        /// <summary>完成系泊。</summary>
        CompleteMooring = 7,
        /// <summary>释放系泊。</summary>
        ReleaseMooring = 8,
    }

    /// <summary>锚泊或系泊命令的稳定失败原因码。</summary>
    public enum D21AnchorMooringFailureCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>命令无效。</summary>
        InvalidCommand = 1,
        /// <summary>引用无效。</summary>
        InvalidReference = 2,
        /// <summary>环境快照无效。</summary>
        InvalidEnvironment = 3,
        /// <summary>命令类型不受支持。</summary>
        UnsupportedCommand = 4,
        /// <summary>锚地不允许锚泊。</summary>
        AnchorageForbidden = 5,
        /// <summary>没有可用船员。</summary>
        CrewUnavailable = 6,
        /// <summary>锚链长度不足。</summary>
        ChainTooShort = 7,
        /// <summary>当前状态不能执行该转换。</summary>
        InvalidStateTransition = 8,
        /// <summary>锚或系泊状态不允许该动作。</summary>
        AnchorUnavailable = 9,
        /// <summary>缺少 D19 泊位许可。</summary>
        BerthPermissionRequired = 10,
        /// <summary>港口或泊位引用不匹配。</summary>
        PortOrBerthMismatch = 11,
        /// <summary>保存版本不匹配。</summary>
        SaveRevisionMismatch = 12,
        /// <summary>命令 ID 已被不同命令使用。</summary>
        CommandIdReused = 13,
        /// <summary>命令 ID 已经成功或失败地处理过。</summary>
        CommandAlreadyApplied = 14,
    }

    /// <summary>计算锚泊风险所需的确定性环境输入。</summary>
    public sealed class D21AnchorEnvironmentSnapshot
    {
        /// <summary>水深，单位为米。</summary>
        public double WaterDepthMeters { get; set; }

        /// <summary>海床类别。</summary>
        public D21SeabedKind Seabed { get; set; }

        /// <summary>逻辑风速输入。</summary>
        public double WindSpeed { get; set; }

        /// <summary>逻辑流速输入。</summary>
        public double CurrentSpeed { get; set; }

        /// <summary>逻辑波浪严重度，范围为 0 到 100。</summary>
        public double WaveSeverity { get; set; }

        /// <summary>可用锚链长度，单位为米。</summary>
        public double ChainLengthMeters { get; set; }

        /// <summary>锚具的逻辑抓持能力。</summary>
        public double AnchorHoldingCapacity { get; set; }

        /// <summary>船舶逻辑质量。</summary>
        public double ShipMass { get; set; }

        /// <summary>船舶吃水，单位为米。</summary>
        public double DraftMeters { get; set; }

        /// <summary>是否有执行锚泊动作的船员。</summary>
        public bool CrewAvailable { get; set; }

        /// <summary>当前锚地是否允许锚泊。</summary>
        public bool AnchorageAllowed { get; set; }

        /// <summary>D19 是否授予了系泊许可。</summary>
        public bool BerthPermissionGranted { get; set; }

        /// <summary>港口稳定 ID，系泊时必填。</summary>
        public string PortId { get; set; }

        /// <summary>泊位稳定 ID，系泊时必填。</summary>
        public string BerthId { get; set; }

        /// <summary>复制环境快照。</summary>
        public D21AnchorEnvironmentSnapshot Clone()
        {
            return new D21AnchorEnvironmentSnapshot
            {
                WaterDepthMeters = WaterDepthMeters,
                Seabed = Seabed,
                WindSpeed = WindSpeed,
                CurrentSpeed = CurrentSpeed,
                WaveSeverity = WaveSeverity,
                ChainLengthMeters = ChainLengthMeters,
                AnchorHoldingCapacity = AnchorHoldingCapacity,
                ShipMass = ShipMass,
                DraftMeters = DraftMeters,
                CrewAvailable = CrewAvailable,
                AnchorageAllowed = AnchorageAllowed,
                BerthPermissionGranted = BerthPermissionGranted,
                PortId = PortId,
                BerthId = BerthId,
            };
        }
    }

    /// <summary>锚泊和系泊 owner 的完整船侧状态。</summary>
    public sealed class D21AnchorMooringState
    {
        /// <summary>创建空锚泊状态。</summary>
        public D21AnchorMooringState()
        {
            SchemaVersion = D21AnchorMooringSystem.CurrentSchemaVersion;
            AnchorState = D21AnchorState.Unanchored;
            MooringState = D21MooringState.Unmoored;
        }

        /// <summary>状态契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>船只稳定 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>当前锚状态。</summary>
        public D21AnchorState AnchorState { get; set; }

        /// <summary>当前系泊状态。</summary>
        public D21MooringState MooringState { get; set; }

        /// <summary>当前使用的锚 ID。</summary>
        public string AnchorId { get; set; }

        /// <summary>当前系泊港口 ID。</summary>
        public string PortId { get; set; }

        /// <summary>当前系泊泊位 ID。</summary>
        public string BerthId { get; set; }

        /// <summary>已经放出的锚链长度。</summary>
        public double ChainDeployedMeters { get; set; }

        /// <summary>当前确定性拖锚风险，范围为 0 到 1。</summary>
        public double DragRisk { get; set; }

        /// <summary>保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>最近一次状态事件 ID。</summary>
        public string LastEventId { get; set; }

        /// <summary>复制锚泊状态。</summary>
        public D21AnchorMooringState Clone()
        {
            return new D21AnchorMooringState
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId,
                AnchorState = AnchorState,
                MooringState = MooringState,
                AnchorId = AnchorId,
                PortId = PortId,
                BerthId = BerthId,
                ChainDeployedMeters = ChainDeployedMeters,
                DragRisk = DragRisk,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId,
            };
        }
    }

    /// <summary>锚泊和系泊 owner 接收的确定性命令。</summary>
    public sealed class D21AnchorMooringCommand
    {
        /// <summary>创建空命令。</summary>
        public D21AnchorMooringCommand()
        {
            Kind = D21AnchorMooringCommandKind.None;
        }

        /// <summary>命令类别。</summary>
        public D21AnchorMooringCommandKind Kind { get; set; }

        /// <summary>幂等命令 ID。</summary>
        public string CommandId { get; set; }

        /// <summary>目标船只 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>锚泊环境和 D19 许可输入。</summary>
        public D21AnchorEnvironmentSnapshot Environment { get; set; }

        /// <summary>逻辑世界日。</summary>
        public int WorldDay { get; set; }

        /// <summary>调用方观察到的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>上游触发事件 ID。</summary>
        public string CauseEventId { get; set; }

        /// <summary>收锚是否成功；仅完成收锚命令读取。</summary>
        public bool RecoverySucceeded { get; set; }

        /// <summary>复制命令。</summary>
        public D21AnchorMooringCommand Clone()
        {
            return new D21AnchorMooringCommand
            {
                Kind = Kind,
                CommandId = CommandId,
                ShipId = ShipId,
                Environment = Environment == null ? null : Environment.Clone(),
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                CauseEventId = CauseEventId,
                RecoverySucceeded = RecoverySucceeded,
            };
        }

        /// <summary>生成用于幂等判断的确定性指纹。</summary>
        public string Fingerprint()
        {
            var environment = Environment == null ? string.Empty : string.Join("|", new[]
            {
                Environment.WaterDepthMeters.ToString("R", CultureInfo.InvariantCulture),
                Environment.Seabed.ToString(),
                Environment.WindSpeed.ToString("R", CultureInfo.InvariantCulture),
                Environment.CurrentSpeed.ToString("R", CultureInfo.InvariantCulture),
                Environment.WaveSeverity.ToString("R", CultureInfo.InvariantCulture),
                Environment.ChainLengthMeters.ToString("R", CultureInfo.InvariantCulture),
                Environment.AnchorHoldingCapacity.ToString("R", CultureInfo.InvariantCulture),
                Environment.ShipMass.ToString("R", CultureInfo.InvariantCulture),
                Environment.DraftMeters.ToString("R", CultureInfo.InvariantCulture),
                Environment.CrewAvailable.ToString(),
                Environment.AnchorageAllowed.ToString(),
                Environment.BerthPermissionGranted.ToString(),
                Environment.PortId ?? string.Empty,
                Environment.BerthId ?? string.Empty,
            });
            return string.Join("|", new[]
            {
                Kind.ToString(), CommandId ?? string.Empty, ShipId ?? string.Empty, environment,
                WorldDay.ToString(CultureInfo.InvariantCulture), SaveRevision.ToString(CultureInfo.InvariantCulture),
                CauseEventId ?? string.Empty, RecoverySucceeded.ToString(),
            });
        }
    }

    /// <summary>锚泊和系泊命令应用结果。</summary>
    public sealed class D21AnchorMooringResult
    {
        /// <summary>命令是否被接受。</summary>
        public bool Accepted { get; set; }

        /// <summary>命令是否是相同指纹的重放。</summary>
        public bool Duplicate { get; set; }

        /// <summary>命令 ID。</summary>
        public string CommandId { get; set; }

        /// <summary>船只 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>稳定原因码。</summary>
        public D21AnchorMooringFailureCode ReasonCode { get; set; }

        /// <summary>应用后的状态快照。</summary>
        public D21AnchorMooringState State { get; set; }

        /// <summary>应用后的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>应用后的拖锚风险。</summary>
        public double DragRisk { get; set; }

        /// <summary>复制结果。</summary>
        public D21AnchorMooringResult Clone()
        {
            return new D21AnchorMooringResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                CommandId = CommandId,
                ShipId = ShipId,
                ReasonCode = ReasonCode,
                State = State == null ? null : State.Clone(),
                SaveRevision = SaveRevision,
                DragRisk = DragRisk,
            };
        }
    }

    /// <summary>锚泊和系泊船侧 owner。</summary>
    public sealed class D21AnchorMooringSystem
    {
        /// <summary>当前 D21-L07 锚泊契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>锚泊状态的唯一 owner 名称。</summary>
        public const string OwnerSystem = "D21AnchorMooringSystem";

        private readonly D21AnchorMooringState _state;
        private readonly Dictionary<string, string> _commandFingerprints;
        private readonly Dictionary<string, D21AnchorMooringResult> _commandResults;

        /// <summary>以已有状态初始化锚泊 owner。</summary>
        public D21AnchorMooringSystem(D21AnchorMooringState initialState)
        {
            _state = initialState == null ? new D21AnchorMooringState() : initialState.Clone();
            _commandFingerprints = new Dictionary<string, string>(StringComparer.Ordinal);
            _commandResults = new Dictionary<string, D21AnchorMooringResult>(StringComparer.Ordinal);
        }

        /// <summary>返回状态深复制快照。</summary>
        public D21AnchorMooringState Snapshot()
        {
            return _state.Clone();
        }

        /// <summary>校验指定状态。</summary>
        public bool ValidateState(D21AnchorMooringState state, out string error)
        {
            error = null;
            if (state == null || state.SchemaVersion != CurrentSchemaVersion || !D21ShipIdentityContracts.IsStableId(state.ShipId, 96))
            {
                error = "anchor_state_identity_invalid";
                return false;
            }
            if (!IsSupportedAnchorState(state.AnchorState) || !IsSupportedMooringState(state.MooringState) || !IsFiniteNonNegative(state.ChainDeployedMeters) || !IsUnitInterval(state.DragRisk) || state.SaveRevision < 0)
            {
                error = "anchor_state_value_invalid";
                return false;
            }
            if (!string.IsNullOrEmpty(state.AnchorId) && !D21ShipIdentityContracts.IsStableId(state.AnchorId, 96))
            {
                error = "anchor_state_anchor_id_invalid";
                return false;
            }
            if (!string.IsNullOrEmpty(state.PortId) && !D21ShipIdentityContracts.IsStableId(state.PortId, 96) || !string.IsNullOrEmpty(state.BerthId) && !D21ShipIdentityContracts.IsStableId(state.BerthId, 96))
            {
                error = "anchor_state_port_id_invalid";
                return false;
            }
            if (state.MooringState == D21MooringState.Moored && (state.AnchorState == D21AnchorState.Holding || state.AnchorState == D21AnchorState.Dragging))
            {
                error = "anchor_moored_while_anchor_holding";
                return false;
            }
            if (state.MooringState == D21MooringState.Moored && (!D21ShipIdentityContracts.IsStableId(state.PortId, 96) || !D21ShipIdentityContracts.IsStableId(state.BerthId, 96)))
            {
                error = "anchor_moored_reference_missing";
                return false;
            }
            return true;
        }

        /// <summary>根据环境输入计算确定性拖锚风险。</summary>
        public static double ComputeDragRisk(D21AnchorEnvironmentSnapshot environment)
        {
            string environmentError;
            if (!ValidateEnvironment(environment, out environmentError))
            {
                return 1d;
            }
            var grip = GripFactor(environment.Seabed);
            var chainRatio = Clamp(environment.ChainLengthMeters / Math.Max(environment.WaterDepthMeters * 1.5d, 1d), 0.25d, 1d);
            var holdingCapacity = environment.AnchorHoldingCapacity * grip * chainRatio / Math.Max(environment.ShipMass, 1d);
            var demand = environment.WindSpeed * 0.04d + environment.CurrentSpeed * 0.25d + environment.WaveSeverity * 0.01d + environment.DraftMeters * 0.1d;
            if (demand <= 0d)
            {
                return 0d;
            }
            return Round(Clamp((demand - holdingCapacity) / demand, 0d, 1d));
        }

        /// <summary>应用锚泊或系泊命令，并提供命令级幂等语义。</summary>
        public D21AnchorMooringResult Apply(D21AnchorMooringCommand command)
        {
            if (command == null)
            {
                return Reject(null, D21AnchorMooringFailureCode.InvalidCommand);
            }
            var commandId = command.CommandId == null ? string.Empty : command.CommandId.Trim();
            if (!D21ShipIdentityContracts.IsStableId(commandId, 96))
            {
                return Reject(command, D21AnchorMooringFailureCode.InvalidCommand);
            }
            var fingerprint = command.Fingerprint();
            string previousFingerprint;
            if (_commandFingerprints.TryGetValue(commandId, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    var replay = _commandResults[commandId].Clone();
                    replay.Duplicate = true;
                    return replay;
                }
                return Reject(command, D21AnchorMooringFailureCode.CommandIdReused);
            }

            D21AnchorMooringFailureCode validationCode;
            if (!ValidateCommand(command, out validationCode))
            {
                var rejected = Reject(command, validationCode);
                Remember(commandId, fingerprint, rejected);
                return rejected;
            }
            var candidate = _state.Clone();
            D21AnchorMooringFailureCode transitionCode;
            if (!ApplyTransition(candidate, command, out transitionCode))
            {
                var transitionRejected = Reject(command, transitionCode);
                Remember(commandId, fingerprint, transitionRejected);
                return transitionRejected;
            }
            candidate.SaveRevision = _state.SaveRevision + 1;
            candidate.LastEventId = "d21_anchor_mooring_" + commandId + "_" + candidate.SaveRevision.ToString(CultureInfo.InvariantCulture);
            var stateError = string.Empty;
            if (!ValidateState(candidate, out stateError))
            {
                var invalidState = Reject(command, D21AnchorMooringFailureCode.InvalidCommand);
                Remember(commandId, fingerprint, invalidState);
                return invalidState;
            }
            CopyState(candidate, _state);
            var accepted = new D21AnchorMooringResult
            {
                Accepted = true,
                Duplicate = false,
                CommandId = commandId,
                ShipId = _state.ShipId,
                ReasonCode = D21AnchorMooringFailureCode.None,
                State = _state.Clone(),
                SaveRevision = _state.SaveRevision,
                DragRisk = _state.DragRisk,
            };
            Remember(commandId, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>根据命令校验通用输入。</summary>
        private bool ValidateCommand(D21AnchorMooringCommand command, out D21AnchorMooringFailureCode code)
        {
            code = D21AnchorMooringFailureCode.None;
            if (!D21ShipIdentityContracts.IsStableId(command.ShipId, 96) || !string.Equals(command.ShipId, _state.ShipId, StringComparison.Ordinal))
            {
                code = D21AnchorMooringFailureCode.InvalidReference;
                return false;
            }
            if (command.SaveRevision != _state.SaveRevision)
            {
                code = D21AnchorMooringFailureCode.SaveRevisionMismatch;
                return false;
            }
            if (command.WorldDay < 0 || !D21ShipIdentityContracts.IsStableId(command.CauseEventId, 128))
            {
                code = D21AnchorMooringFailureCode.InvalidReference;
                return false;
            }
            string environmentError;
            if (!ValidateEnvironment(command.Environment, out environmentError))
            {
                code = D21AnchorMooringFailureCode.InvalidEnvironment;
                return false;
            }
            if (command.Kind < D21AnchorMooringCommandKind.Deploy || command.Kind > D21AnchorMooringCommandKind.ReleaseMooring)
            {
                code = D21AnchorMooringFailureCode.UnsupportedCommand;
                return false;
            }
            return true;
        }

        private static bool ApplyTransition(D21AnchorMooringState state, D21AnchorMooringCommand command, out D21AnchorMooringFailureCode code)
        {
            code = D21AnchorMooringFailureCode.None;
            var environment = command.Environment;
            switch (command.Kind)
            {
                case D21AnchorMooringCommandKind.Deploy:
                    if (!environment.AnchorageAllowed) return Fail(out code, D21AnchorMooringFailureCode.AnchorageForbidden);
                    if (!environment.CrewAvailable) return Fail(out code, D21AnchorMooringFailureCode.CrewUnavailable);
                    if (environment.ChainLengthMeters < environment.WaterDepthMeters * 1.2d) return Fail(out code, D21AnchorMooringFailureCode.ChainTooShort);
                    if (state.MooringState != D21MooringState.Unmoored && state.MooringState != D21MooringState.Released) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    if (state.AnchorState != D21AnchorState.Unanchored && state.AnchorState != D21AnchorState.Released && state.AnchorState != D21AnchorState.Dragging) return Fail(out code, D21AnchorMooringFailureCode.AnchorUnavailable);
                    state.AnchorState = D21AnchorState.Deploying;
                    state.MooringState = D21MooringState.Unmoored;
                    state.ChainDeployedMeters = 0d;
                    state.DragRisk = 0d;
                    return true;
                case D21AnchorMooringCommandKind.CompleteDeployment:
                    if (state.AnchorState != D21AnchorState.Deploying) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    if (!environment.AnchorageAllowed) return Fail(out code, D21AnchorMooringFailureCode.AnchorageForbidden);
                    if (!environment.CrewAvailable) return Fail(out code, D21AnchorMooringFailureCode.CrewUnavailable);
                    if (environment.ChainLengthMeters < environment.WaterDepthMeters * 1.2d) return Fail(out code, D21AnchorMooringFailureCode.ChainTooShort);
                    state.ChainDeployedMeters = Round(environment.ChainLengthMeters);
                    state.DragRisk = ComputeDragRisk(environment);
                    state.AnchorState = state.DragRisk >= 0.65d ? D21AnchorState.Dragging : D21AnchorState.Holding;
                    return true;
                case D21AnchorMooringCommandKind.ReevaluateHolding:
                    if (state.AnchorState != D21AnchorState.Holding && state.AnchorState != D21AnchorState.Dragging) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    if (!environment.AnchorageAllowed) return Fail(out code, D21AnchorMooringFailureCode.AnchorageForbidden);
                    state.DragRisk = ComputeDragRisk(environment);
                    state.AnchorState = state.DragRisk >= 0.65d ? D21AnchorState.Dragging : D21AnchorState.Holding;
                    state.ChainDeployedMeters = Round(environment.ChainLengthMeters);
                    return true;
                case D21AnchorMooringCommandKind.BeginRecovery:
                    if (!environment.CrewAvailable) return Fail(out code, D21AnchorMooringFailureCode.CrewUnavailable);
                    if (state.AnchorState != D21AnchorState.Holding && state.AnchorState != D21AnchorState.Dragging) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    state.AnchorState = D21AnchorState.Recovering;
                    return true;
                case D21AnchorMooringCommandKind.CompleteRecovery:
                    if (state.AnchorState != D21AnchorState.Recovering) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    state.AnchorState = command.RecoverySucceeded ? D21AnchorState.Released : D21AnchorState.Lost;
                    state.ChainDeployedMeters = 0d;
                    state.DragRisk = command.RecoverySucceeded ? 0d : 1d;
                    return true;
                case D21AnchorMooringCommandKind.BeginMooring:
                    if (!HasBerthPermission(environment)) return Fail(out code, D21AnchorMooringFailureCode.BerthPermissionRequired);
                    if (state.AnchorState == D21AnchorState.Holding || state.AnchorState == D21AnchorState.Dragging || state.AnchorState == D21AnchorState.Deploying || state.AnchorState == D21AnchorState.Recovering) return Fail(out code, D21AnchorMooringFailureCode.AnchorUnavailable);
                    if (state.MooringState != D21MooringState.Unmoored && state.MooringState != D21MooringState.Released) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    state.MooringState = D21MooringState.Approaching;
                    state.PortId = environment.PortId;
                    state.BerthId = environment.BerthId;
                    state.AnchorState = D21AnchorState.Released;
                    return true;
                case D21AnchorMooringCommandKind.CompleteMooring:
                    if (!HasBerthPermission(environment)) return Fail(out code, D21AnchorMooringFailureCode.BerthPermissionRequired);
                    if (state.MooringState != D21MooringState.Approaching && state.MooringState != D21MooringState.Securing) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    if (!string.Equals(state.PortId, environment.PortId, StringComparison.Ordinal) || !string.Equals(state.BerthId, environment.BerthId, StringComparison.Ordinal)) return Fail(out code, D21AnchorMooringFailureCode.PortOrBerthMismatch);
                    state.MooringState = D21MooringState.Moored;
                    state.AnchorState = D21AnchorState.Released;
                    state.ChainDeployedMeters = 0d;
                    state.DragRisk = 0d;
                    return true;
                case D21AnchorMooringCommandKind.ReleaseMooring:
                    if (state.MooringState != D21MooringState.Moored && state.MooringState != D21MooringState.Breaking) return Fail(out code, D21AnchorMooringFailureCode.InvalidStateTransition);
                    state.MooringState = D21MooringState.Released;
                    state.PortId = null;
                    state.BerthId = null;
                    return true;
                default:
                    return Fail(out code, D21AnchorMooringFailureCode.UnsupportedCommand);
            }
        }

        private static bool HasBerthPermission(D21AnchorEnvironmentSnapshot environment)
        {
            return environment.BerthPermissionGranted && D21ShipIdentityContracts.IsStableId(environment.PortId, 96) && D21ShipIdentityContracts.IsStableId(environment.BerthId, 96);
        }

        private static bool ValidateEnvironment(D21AnchorEnvironmentSnapshot environment, out string error)
        {
            error = null;
            if (environment == null || !IsFinitePositive(environment.WaterDepthMeters) || !IsSupportedSeabed(environment.Seabed) || !IsFiniteNonNegative(environment.WindSpeed) || !IsFiniteNonNegative(environment.CurrentSpeed) || !IsFiniteNonNegative(environment.WaveSeverity) || environment.WaveSeverity > 100d || !IsFinitePositive(environment.ChainLengthMeters) || !IsFinitePositive(environment.AnchorHoldingCapacity) || !IsFinitePositive(environment.ShipMass) || !IsFiniteNonNegative(environment.DraftMeters))
            {
                error = "anchor_environment_value_invalid";
                return false;
            }
            if (environment.BerthPermissionGranted && (!D21ShipIdentityContracts.IsStableId(environment.PortId, 96) || !D21ShipIdentityContracts.IsStableId(environment.BerthId, 96)))
            {
                error = "anchor_environment_berth_reference_invalid";
                return false;
            }
            return true;
        }

        private static bool Fail(out D21AnchorMooringFailureCode code, D21AnchorMooringFailureCode value)
        {
            code = value;
            return false;
        }

        private static double GripFactor(D21SeabedKind seabed)
        {
            switch (seabed)
            {
                case D21SeabedKind.Mud: return 0.9d;
                case D21SeabedKind.Sand: return 1d;
                case D21SeabedKind.Gravel: return 0.65d;
                case D21SeabedKind.Coral: return 0.45d;
                case D21SeabedKind.Rock: return 0.3d;
                default: return 0d;
            }
        }

        private static bool IsSupportedAnchorState(D21AnchorState value)
        {
            return value >= D21AnchorState.Unanchored && value <= D21AnchorState.Disabled;
        }

        private static bool IsSupportedMooringState(D21MooringState value)
        {
            return value >= D21MooringState.Unmoored && value <= D21MooringState.Released;
        }

        private static bool IsSupportedSeabed(D21SeabedKind value)
        {
            return value >= D21SeabedKind.Mud && value <= D21SeabedKind.Rock;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0d;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
        }

        private static bool IsUnitInterval(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d && value <= 1d;
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static double Round(double value)
        {
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }

        private static D21AnchorMooringResult Reject(D21AnchorMooringCommand command, D21AnchorMooringFailureCode code)
        {
            return new D21AnchorMooringResult
            {
                Accepted = false,
                Duplicate = false,
                CommandId = command == null ? null : command.CommandId,
                ShipId = command == null ? null : command.ShipId,
                ReasonCode = code,
                State = null,
                SaveRevision = -1,
                DragRisk = 1d,
            };
        }

        private void Remember(string commandId, string fingerprint, D21AnchorMooringResult result)
        {
            _commandFingerprints[commandId] = fingerprint;
            _commandResults[commandId] = result.Clone();
        }

        private static void CopyState(D21AnchorMooringState source, D21AnchorMooringState target)
        {
            target.SchemaVersion = source.SchemaVersion;
            target.ShipId = source.ShipId;
            target.AnchorState = source.AnchorState;
            target.MooringState = source.MooringState;
            target.AnchorId = source.AnchorId;
            target.PortId = source.PortId;
            target.BerthId = source.BerthId;
            target.ChainDeployedMeters = source.ChainDeployedMeters;
            target.DragRisk = source.DragRisk;
            target.SaveRevision = source.SaveRevision;
            target.LastEventId = source.LastEventId;
        }
    }
}
