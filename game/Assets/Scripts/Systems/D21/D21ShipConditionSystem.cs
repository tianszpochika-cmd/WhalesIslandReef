// ============================================================
// D21ShipConditionSystem —— D21-L04 船况、损伤与维修（纯 C#）
//
// 本系统只拥有船况数值、损伤事实的船侧投影和维修裁决。损伤必须由上游
// 事实引用触发；本文件不从碰撞、刚体、天气或场景对象读取结果。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>船况损伤的逻辑类别。</summary>
    public enum D21ShipDamageKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>船体结构耐久损失。</summary>
        Structural = 1,
        /// <summary>进水或舱室积水。</summary>
        Flooding = 2,
        /// <summary>船体倾斜角度变化。</summary>
        Tilt = 3,
        /// <summary>舵系损伤。</summary>
        Rudder = 4,
        /// <summary>桅杆损伤。</summary>
        Mast = 5,
        /// <summary>索具损伤。</summary>
        Rigging = 6,
        /// <summary>帆面损伤。</summary>
        Sail = 7,
        /// <summary>火灾损伤。</summary>
        Fire = 8,
        /// <summary>烟熏和能见度风险。</summary>
        Smoke = 9,
        /// <summary>火药或爆炸物安全风险。</summary>
        PowderSafety = 10,
    }

    /// <summary>损伤严重程度。</summary>
    public enum D21ShipDamageSeverity
    {
        /// <summary>没有损伤。</summary>
        None = 0,
        /// <summary>轻微损伤。</summary>
        Minor = 1,
        /// <summary>重大损伤。</summary>
        Major = 2,
        /// <summary>危急损伤。</summary>
        Critical = 3,
    }

    /// <summary>产生损伤事实的逻辑来源。</summary>
    public enum D21DamageSourceKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>战斗事实。</summary>
        CombatFact = 1,
        /// <summary>天气事实。</summary>
        WeatherFact = 2,
        /// <summary>搁浅事实。</summary>
        GroundingFact = 3,
        /// <summary>火灾事实。</summary>
        FireFact = 4,
        /// <summary>维护事实。</summary>
        MaintenanceFact = 5,
        /// <summary>其他经过验证的外部事实。</summary>
        ExternalFact = 6,
    }

    /// <summary>维修动作的逻辑类别。</summary>
    public enum D21RepairKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>临时封堵进水。</summary>
        TemporaryLeakSeal = 1,
        /// <summary>抽排积水。</summary>
        PumpOut = 2,
        /// <summary>修补帆面。</summary>
        SailRepair = 3,
        /// <summary>修理索具。</summary>
        RiggingRepair = 4,
        /// <summary>修理舵系。</summary>
        RudderRepair = 5,
        /// <summary>修理桅杆。</summary>
        MastRepair = 6,
        /// <summary>修理船体结构。</summary>
        StructuralRepair = 7,
        /// <summary>灭火和清理火灾风险。</summary>
        FireSuppression = 8,
        /// <summary>在船厂执行完整维修。</summary>
        FullDockRepair = 9,
    }

    /// <summary>维修发生位置。</summary>
    public enum D21RepairLocation
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>船上维修。</summary>
        Onboard = 1,
        /// <summary>船厂维修。</summary>
        Shipyard = 2,
    }

    /// <summary>根据船况计算的适航等级。</summary>
    public enum D21ShipSeaworthiness
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>可以按正常规则出航。</summary>
        Navigable = 1,
        /// <summary>可以移动但受到限制。</summary>
        Restricted = 2,
        /// <summary>不能安全执行正常航行。</summary>
        Disabled = 3,
        /// <summary>正在快速失去浮性或稳定性。</summary>
        Sinking = 4,
        /// <summary>船况已经失去可恢复的航行语义。</summary>
        Lost = 5,
    }

    /// <summary>船况校验或命令应用失败的稳定原因码。</summary>
    public enum D21ShipConditionFailureCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>缺少命令 ID。</summary>
        MissingCommandId = 1,
        /// <summary>引用不是稳定逻辑 ID。</summary>
        InvalidReference = 2,
        /// <summary>命令类型不受支持。</summary>
        UnsupportedCommand = 3,
        /// <summary>数值不在合法范围。</summary>
        InvalidAmount = 4,
        /// <summary>损伤类别或严重程度不受支持。</summary>
        UnsupportedDamage = 5,
        /// <summary>损伤缺少来源事实。</summary>
        MissingSourceFact = 6,
        /// <summary>船 ID 不匹配。</summary>
        ShipMismatch = 7,
        /// <summary>保存版本不匹配。</summary>
        SaveRevisionMismatch = 8,
        /// <summary>命令 ID 已经被不同命令使用。</summary>
        CommandIdReused = 9,
        /// <summary>状态结构无效。</summary>
        InvalidState = 10,
        /// <summary>维修上下文缺少权限或资源。</summary>
        RepairNotEligible = 11,
        /// <summary>没有可被该维修动作处理的损伤。</summary>
        NoDamageToRepair = 12,
        /// <summary>船况已丢失，不能继续维修。</summary>
        ShipLost = 13,
        /// <summary>事件事实 ID 无效。</summary>
        InvalidEventId = 14,
    }

    /// <summary>船况 owner 支持的命令类别。</summary>
    public enum D21ShipConditionCommandKind
    {
        /// <summary>未初始化。</summary>
        None = 0,
        /// <summary>追加一条事实驱动的损伤。</summary>
        ApplyDamage = 1,
        /// <summary>依据维修资格处理已有损伤。</summary>
        Repair = 2,
    }

    /// <summary>一条可追溯的船况损伤记录。</summary>
    public sealed class D21ShipDamageEntry
    {
        /// <summary>创建空损伤记录。</summary>
        public D21ShipDamageEntry()
        {
            Kind = D21ShipDamageKind.Unknown;
            Severity = D21ShipDamageSeverity.None;
            SourceKind = D21DamageSourceKind.Unknown;
        }

        /// <summary>损伤记录的稳定 ID。</summary>
        public string DamageId { get; set; }

        /// <summary>损伤类别。</summary>
        public D21ShipDamageKind Kind { get; set; }

        /// <summary>受影响组件的逻辑 ID，可为空。</summary>
        public string ComponentId { get; set; }

        /// <summary>严重程度。</summary>
        public D21ShipDamageSeverity Severity { get; set; }

        /// <summary>损伤量；单位由损伤类别解释。</summary>
        public double Amount { get; set; }

        /// <summary>记录是否仍然有效。</summary>
        public bool Active { get; set; }

        /// <summary>产生该损伤的事实 ID。</summary>
        public string SourceFactId { get; set; }

        /// <summary>产生事实时的逻辑世界日。</summary>
        public int WorldDay { get; set; }

        /// <summary>记录写入时的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>损伤来源类别。</summary>
        public D21DamageSourceKind SourceKind { get; set; }

        /// <summary>复制损伤记录及其值。</summary>
        public D21ShipDamageEntry Clone()
        {
            return new D21ShipDamageEntry
            {
                DamageId = DamageId,
                Kind = Kind,
                ComponentId = ComponentId,
                Severity = Severity,
                Amount = Amount,
                Active = Active,
                SourceFactId = SourceFactId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                SourceKind = SourceKind,
            };
        }
    }

    /// <summary>船况 owner 的完整逻辑状态。</summary>
    public sealed class D21ShipConditionState
    {
        /// <summary>创建空船况状态。</summary>
        public D21ShipConditionState()
        {
            SchemaVersion = D21ShipConditionSystem.CurrentSchemaVersion;
            DamageEntries = new List<D21ShipDamageEntry>();
            Seaworthiness = D21ShipSeaworthiness.Unknown;
        }

        /// <summary>船况契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>船只稳定 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>最大结构耐久。</summary>
        public double MaxDurability { get; set; }

        /// <summary>当前结构耐久。</summary>
        public double Durability { get; set; }

        /// <summary>最大允许进水量。</summary>
        public double MaxFlooding { get; set; }

        /// <summary>当前进水量。</summary>
        public double Flooding { get; set; }

        /// <summary>当前倾斜角度，单位为逻辑度。</summary>
        public double TiltDegrees { get; set; }

        /// <summary>按事实累积的损伤记录。</summary>
        public List<D21ShipDamageEntry> DamageEntries { get; set; }

        /// <summary>当前适航等级。</summary>
        public D21ShipSeaworthiness Seaworthiness { get; set; }

        /// <summary>状态保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>最近一次状态事件 ID。</summary>
        public string LastEventId { get; set; }

        /// <summary>深复制船况状态。</summary>
        public D21ShipConditionState Clone()
        {
            var clone = new D21ShipConditionState
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId,
                MaxDurability = MaxDurability,
                Durability = Durability,
                MaxFlooding = MaxFlooding,
                Flooding = Flooding,
                TiltDegrees = TiltDegrees,
                Seaworthiness = Seaworthiness,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId,
            };

            if (DamageEntries != null)
            {
                for (var index = 0; index < DamageEntries.Count; index++)
                {
                    if (DamageEntries[index] != null)
                    {
                        clone.DamageEntries.Add(DamageEntries[index].Clone());
                    }
                }
            }

            return clone;
        }
    }

    /// <summary>船上或船厂维修资格的只读输入。</summary>
    public sealed class D21ShipRepairContext
    {
        /// <summary>创建空维修上下文。</summary>
        public D21ShipRepairContext()
        {
            Location = D21RepairLocation.Unknown;
        }

        /// <summary>维修位置。</summary>
        public D21RepairLocation Location { get; set; }

        /// <summary>船是否处于港口语义。</summary>
        public bool IsAtPort { get; set; }

        /// <summary>船是否已系泊。</summary>
        public bool IsMoored { get; set; }

        /// <summary>是否有具备维修资格的人员。</summary>
        public bool HasQualifiedCrew { get; set; }

        /// <summary>是否有维修材料。</summary>
        public bool HasMaterials { get; set; }

        /// <summary>调用者是否拥有维修权限。</summary>
        public bool OwnerPermission { get; set; }

        /// <summary>船上是否允许执行有限维修。</summary>
        public bool CanRepairAtSea { get; set; }

        /// <summary>维修所依据的事实 ID，可为空。</summary>
        public string CauseFactId { get; set; }

        /// <summary>复制维修上下文。</summary>
        public D21ShipRepairContext Clone()
        {
            return new D21ShipRepairContext
            {
                Location = Location,
                IsAtPort = IsAtPort,
                IsMoored = IsMoored,
                HasQualifiedCrew = HasQualifiedCrew,
                HasMaterials = HasMaterials,
                OwnerPermission = OwnerPermission,
                CanRepairAtSea = CanRepairAtSea,
                CauseFactId = CauseFactId,
            };
        }
    }

    /// <summary>船况校验结果。</summary>
    public sealed class D21ShipConditionValidationResult
    {
        /// <summary>创建校验结果。</summary>
        public D21ShipConditionValidationResult(bool valid, D21ShipConditionFailureCode code, IEnumerable<string> errors)
        {
            Valid = valid;
            Code = code;
            Errors = new ReadOnlyCollection<string>(new List<string>(errors ?? new string[0]));
        }

        /// <summary>是否通过校验。</summary>
        public bool Valid { get; private set; }

        /// <summary>首个稳定原因码。</summary>
        public D21ShipConditionFailureCode Code { get; private set; }

        /// <summary>可供诊断的稳定错误码集合。</summary>
        public ReadOnlyCollection<string> Errors { get; private set; }

        /// <summary>创建成功结果。</summary>
        public static D21ShipConditionValidationResult Success()
        {
            return new D21ShipConditionValidationResult(true, D21ShipConditionFailureCode.None, new string[0]);
        }

        /// <summary>创建失败结果。</summary>
        public static D21ShipConditionValidationResult Failure(D21ShipConditionFailureCode code, string error)
        {
            return new D21ShipConditionValidationResult(false, code, new[] { error ?? code.ToString() });
        }

        /// <summary>复制校验结果。</summary>
        public D21ShipConditionValidationResult Clone()
        {
            return new D21ShipConditionValidationResult(Valid, Code, Errors);
        }
    }

    /// <summary>船况 owner 接收的确定性命令。</summary>
    public sealed class D21ShipConditionCommand
    {
        /// <summary>创建空船况命令。</summary>
        public D21ShipConditionCommand()
        {
            Kind = D21ShipConditionCommandKind.None;
        }

        /// <summary>命令类别。</summary>
        public D21ShipConditionCommandKind Kind { get; set; }

        /// <summary>幂等命令 ID。</summary>
        public string CommandId { get; set; }

        /// <summary>目标船只 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>损伤命令携带的损伤记录。</summary>
        public D21ShipDamageEntry Damage { get; set; }

        /// <summary>维修命令的类别。</summary>
        public D21RepairKind RepairKind { get; set; }

        /// <summary>维修量；完整船厂维修使用正值作为资格校验输入。</summary>
        public double RepairAmount { get; set; }

        /// <summary>维修资格输入。</summary>
        public D21ShipRepairContext RepairContext { get; set; }

        /// <summary>逻辑世界日。</summary>
        public int WorldDay { get; set; }

        /// <summary>调用方观察到的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>触发该命令的上游事件 ID。</summary>
        public string CauseEventId { get; set; }

        /// <summary>复制命令。</summary>
        public D21ShipConditionCommand Clone()
        {
            return new D21ShipConditionCommand
            {
                Kind = Kind,
                CommandId = CommandId,
                ShipId = ShipId,
                Damage = Damage == null ? null : Damage.Clone(),
                RepairKind = RepairKind,
                RepairAmount = RepairAmount,
                RepairContext = RepairContext == null ? null : RepairContext.Clone(),
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                CauseEventId = CauseEventId,
            };
        }

        /// <summary>生成用于命令幂等判断的确定性指纹。</summary>
        public string Fingerprint()
        {
            var damageFingerprint = string.Empty;
            if (Damage != null)
            {
                damageFingerprint = string.Join("|", new[]
                {
                    Damage.DamageId ?? string.Empty,
                    Damage.Kind.ToString(),
                    Damage.ComponentId ?? string.Empty,
                    Damage.Severity.ToString(),
                    Damage.Amount.ToString("R", CultureInfo.InvariantCulture),
                    Damage.Active.ToString(),
                    Damage.SourceFactId ?? string.Empty,
                    Damage.WorldDay.ToString(CultureInfo.InvariantCulture),
                    Damage.SourceKind.ToString(),
                });
            }

            var repairFingerprint = string.Empty;
            if (RepairContext != null)
            {
                repairFingerprint = string.Join("|", new[]
                {
                    RepairContext.Location.ToString(),
                    RepairContext.IsAtPort.ToString(),
                    RepairContext.IsMoored.ToString(),
                    RepairContext.HasQualifiedCrew.ToString(),
                    RepairContext.HasMaterials.ToString(),
                    RepairContext.OwnerPermission.ToString(),
                    RepairContext.CanRepairAtSea.ToString(),
                    RepairContext.CauseFactId ?? string.Empty,
                });
            }

            return string.Join("|", new[]
            {
                Kind.ToString(),
                CommandId ?? string.Empty,
                ShipId ?? string.Empty,
                damageFingerprint,
                RepairKind.ToString(),
                RepairAmount.ToString("R", CultureInfo.InvariantCulture),
                repairFingerprint,
                WorldDay.ToString(CultureInfo.InvariantCulture),
                SaveRevision.ToString(CultureInfo.InvariantCulture),
                CauseEventId ?? string.Empty,
            });
        }
    }

    /// <summary>船况命令应用结果。</summary>
    public sealed class D21ShipConditionResult
    {
        /// <summary>创建空结果。</summary>
        public D21ShipConditionResult()
        {
            ReasonCode = D21ShipConditionFailureCode.None;
        }

        /// <summary>命令是否被接受。</summary>
        public bool Accepted { get; set; }

        /// <summary>命令是否是相同指纹的重放。</summary>
        public bool Duplicate { get; set; }

        /// <summary>命令 ID。</summary>
        public string CommandId { get; set; }

        /// <summary>船只 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>失败或成功的稳定原因码。</summary>
        public D21ShipConditionFailureCode ReasonCode { get; set; }

        /// <summary>应用后的船况快照。</summary>
        public D21ShipConditionState State { get; set; }

        /// <summary>应用后的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>应用后生成的事件 ID。</summary>
        public string EventId { get; set; }

        /// <summary>应用后的适航等级。</summary>
        public D21ShipSeaworthiness Seaworthiness { get; set; }

        /// <summary>复制结果和内部状态。</summary>
        public D21ShipConditionResult Clone()
        {
            return new D21ShipConditionResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                CommandId = CommandId,
                ShipId = ShipId,
                ReasonCode = ReasonCode,
                State = State == null ? null : State.Clone(),
                SaveRevision = SaveRevision,
                EventId = EventId,
                Seaworthiness = Seaworthiness,
            };
        }
    }

    /// <summary>船况 owner；只在逻辑命令边界内修改自己的状态。</summary>
    public sealed class D21ShipConditionSystem
    {
        /// <summary>当前 D21-L04 船况契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>船况状态的唯一 owner 名称。</summary>
        public const string OwnerSystem = "D21ShipConditionSystem";

        private readonly D21ShipConditionState _state;
        private readonly Dictionary<string, string> _commandFingerprints;
        private readonly Dictionary<string, D21ShipConditionResult> _commandResults;

        /// <summary>以已有船况快照初始化 owner。</summary>
        public D21ShipConditionSystem(D21ShipConditionState initialState)
        {
            _state = initialState == null ? new D21ShipConditionState() : initialState.Clone();
            _commandFingerprints = new Dictionary<string, string>(StringComparer.Ordinal);
            _commandResults = new Dictionary<string, D21ShipConditionResult>(StringComparer.Ordinal);
        }

        /// <summary>创建健康船况的确定性起始状态。</summary>
        public static D21ShipConditionState CreateHealthyState(string shipId, double maxDurability, double maxFlooding)
        {
            var state = new D21ShipConditionState
            {
                SchemaVersion = CurrentSchemaVersion,
                ShipId = shipId,
                MaxDurability = Round(maxDurability),
                Durability = Round(maxDurability),
                MaxFlooding = Round(maxFlooding),
                Flooding = 0d,
                TiltDegrees = 0d,
                Seaworthiness = D21ShipSeaworthiness.Navigable,
                SaveRevision = 0,
            };
            state.Seaworthiness = EvaluateSeaworthinessOf(state);
            return state;
        }

        /// <summary>返回 owner 状态的深复制快照。</summary>
        public D21ShipConditionState Snapshot()
        {
            return _state.Clone();
        }

        /// <summary>校验指定船况快照，不改变任何状态。</summary>
        public D21ShipConditionValidationResult ValidateState(D21ShipConditionState state)
        {
            return ValidateStateInternal(state);
        }

        /// <summary>返回当前快照的适航等级。</summary>
        public D21ShipSeaworthiness EvaluateSeaworthiness()
        {
            return EvaluateSeaworthinessOf(_state);
        }

        /// <summary>应用损伤或维修命令，并提供命令级幂等语义。</summary>
        public D21ShipConditionResult Apply(D21ShipConditionCommand command)
        {
            if (command == null)
            {
                return Reject(null, D21ShipConditionFailureCode.InvalidState);
            }

            var commandId = command.CommandId == null ? string.Empty : command.CommandId.Trim();
            if (!D21ShipIdentityContracts.IsStableId(commandId, 96))
            {
                return Reject(command, D21ShipConditionFailureCode.MissingCommandId);
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

                return Reject(command, D21ShipConditionFailureCode.CommandIdReused);
            }

            var commandValidation = ValidateCommand(command);
            if (!commandValidation.Valid)
            {
                var rejected = Reject(command, commandValidation.Code);
                Remember(commandId, fingerprint, rejected);
                return rejected;
            }

            var candidate = _state.Clone();
            if (command.Kind == D21ShipConditionCommandKind.ApplyDamage)
            {
                ApplyDamageTo(candidate, command);
            }
            else
            {
                if (!TryRepairState(candidate, command))
                {
                    var noDamage = Reject(command, D21ShipConditionFailureCode.NoDamageToRepair);
                    Remember(commandId, fingerprint, noDamage);
                    return noDamage;
                }
            }

            candidate.SaveRevision = _state.SaveRevision + 1;
            candidate.LastEventId = "d21_condition_" + commandId + "_" + candidate.SaveRevision.ToString(CultureInfo.InvariantCulture);
            candidate.Seaworthiness = EvaluateSeaworthinessOf(candidate);
            var stateValidation = ValidateStateInternal(candidate);
            if (!stateValidation.Valid)
            {
                var invalidState = Reject(command, D21ShipConditionFailureCode.InvalidState);
                Remember(commandId, fingerprint, invalidState);
                return invalidState;
            }

            CopyState(candidate, _state);
            var accepted = new D21ShipConditionResult
            {
                Accepted = true,
                Duplicate = false,
                CommandId = commandId,
                ShipId = _state.ShipId,
                ReasonCode = D21ShipConditionFailureCode.None,
                State = _state.Clone(),
                SaveRevision = _state.SaveRevision,
                EventId = _state.LastEventId,
                Seaworthiness = _state.Seaworthiness,
            };
            Remember(commandId, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>使用损伤记录构造并应用一条损伤命令。</summary>
        public D21ShipConditionResult ApplyDamage(string commandId, string shipId, D21ShipDamageEntry damage, int saveRevision, string causeEventId)
        {
            return Apply(new D21ShipConditionCommand
            {
                Kind = D21ShipConditionCommandKind.ApplyDamage,
                CommandId = commandId,
                ShipId = shipId,
                Damage = damage == null ? null : damage.Clone(),
                WorldDay = damage == null ? 0 : damage.WorldDay,
                SaveRevision = saveRevision,
                CauseEventId = causeEventId,
            });
        }

        /// <summary>使用维修上下文构造并应用一条维修命令。</summary>
        public D21ShipConditionResult TryRepair(string commandId, string shipId, D21RepairKind repairKind, double repairAmount, D21ShipRepairContext context, int saveRevision, string causeEventId)
        {
            return Apply(new D21ShipConditionCommand
            {
                Kind = D21ShipConditionCommandKind.Repair,
                CommandId = commandId,
                ShipId = shipId,
                RepairKind = repairKind,
                RepairAmount = repairAmount,
                RepairContext = context == null ? null : context.Clone(),
                WorldDay = 0,
                SaveRevision = saveRevision,
                CauseEventId = causeEventId,
            });
        }

        private static D21ShipConditionValidationResult ValidateStateInternal(D21ShipConditionState state)
        {
            var errors = new List<string>();
            if (state == null)
            {
                errors.Add("condition_state_missing");
                return new D21ShipConditionValidationResult(false, D21ShipConditionFailureCode.InvalidState, errors);
            }

            if (state.SchemaVersion != CurrentSchemaVersion)
            {
                errors.Add("condition_schema_version_invalid");
            }
            if (!D21ShipIdentityContracts.IsStableId(state.ShipId, 96))
            {
                errors.Add("condition_ship_id_invalid");
            }
            if (!IsFinitePositive(state.MaxDurability) || !IsFiniteNonNegative(state.Durability) || state.Durability > state.MaxDurability)
            {
                errors.Add("condition_durability_invalid");
            }
            if (!IsFinitePositive(state.MaxFlooding) || !IsFiniteNonNegative(state.Flooding) || state.Flooding > state.MaxFlooding)
            {
                errors.Add("condition_flooding_invalid");
            }
            if (!IsFiniteNonNegative(state.TiltDegrees) || state.TiltDegrees > 90d)
            {
                errors.Add("condition_tilt_invalid");
            }
            if (state.SaveRevision < 0)
            {
                errors.Add("condition_save_revision_invalid");
            }
            if (!string.IsNullOrEmpty(state.LastEventId) && !D21ShipIdentityContracts.IsStableId(state.LastEventId, 128))
            {
                errors.Add("condition_event_id_invalid");
            }
            if (state.DamageEntries == null)
            {
                errors.Add("condition_damage_entries_missing");
            }
            else
            {
                var ids = new HashSet<string>(StringComparer.Ordinal);
                for (var index = 0; index < state.DamageEntries.Count; index++)
                {
                    var entry = state.DamageEntries[index];
                    if (entry == null || !D21ShipIdentityContracts.IsStableId(entry.DamageId, 128))
                    {
                        errors.Add("condition_damage_id_invalid");
                        continue;
                    }
                    if (!ids.Add(entry.DamageId))
                    {
                        errors.Add("condition_damage_id_duplicate");
                    }
                    if (!IsSupportedDamageKind(entry.Kind) || !IsSupportedSeverity(entry.Severity) || !IsSupportedSourceKind(entry.SourceKind))
                    {
                        errors.Add("condition_damage_kind_invalid");
                    }
                    if (!IsFinitePositive(entry.Amount))
                    {
                        errors.Add("condition_damage_amount_invalid");
                    }
                    if (!D21ShipIdentityContracts.IsStableId(entry.SourceFactId, 128))
                    {
                        errors.Add("condition_damage_source_invalid");
                    }
                    if (entry.WorldDay < 0 || entry.SaveRevision < 0)
                    {
                        errors.Add("condition_damage_metadata_invalid");
                    }
                    if (!string.IsNullOrEmpty(entry.ComponentId) && !D21ShipIdentityContracts.IsStableId(entry.ComponentId, 128))
                    {
                        errors.Add("condition_damage_component_invalid");
                    }
                }
            }

            if (errors.Count > 0)
            {
                return new D21ShipConditionValidationResult(false, D21ShipConditionFailureCode.InvalidState, errors);
            }

            return D21ShipConditionValidationResult.Success();
        }

        private D21ShipConditionValidationResult ValidateCommand(D21ShipConditionCommand command)
        {
            if (!D21ShipIdentityContracts.IsStableId(command.ShipId, 96) || !string.Equals(command.ShipId, _state.ShipId, StringComparison.Ordinal))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.ShipMismatch, "condition_ship_mismatch");
            }
            if (command.SaveRevision != _state.SaveRevision)
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.SaveRevisionMismatch, "condition_save_revision_mismatch");
            }
            if (command.WorldDay < 0 || !D21ShipIdentityContracts.IsStableId(command.CauseEventId, 128))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidEventId, "condition_cause_event_invalid");
            }

            if (command.Kind == D21ShipConditionCommandKind.ApplyDamage)
            {
                return ValidateDamageCommand(command);
            }
            if (command.Kind == D21ShipConditionCommandKind.Repair)
            {
                if (_state.Seaworthiness == D21ShipSeaworthiness.Lost)
                {
                    return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.ShipLost, "condition_ship_lost");
                }
                return ValidateRepairCommand(command);
            }
            return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.UnsupportedCommand, "condition_command_unsupported");
        }

        private static D21ShipConditionValidationResult ValidateDamageCommand(D21ShipConditionCommand command)
        {
            var damage = command.Damage;
            if (damage == null)
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.UnsupportedDamage, "condition_damage_missing");
            }
            if (!IsSupportedDamageKind(damage.Kind) || !IsSupportedSeverity(damage.Severity) || damage.Severity == D21ShipDamageSeverity.None || !IsSupportedSourceKind(damage.SourceKind))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.UnsupportedDamage, "condition_damage_kind_unsupported");
            }
            if (!IsFinitePositive(damage.Amount))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidAmount, "condition_damage_amount_invalid");
            }
            if (!D21ShipIdentityContracts.IsStableId(damage.SourceFactId, 128))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.MissingSourceFact, "condition_damage_source_missing");
            }
            if (damage.WorldDay < 0 || damage.SaveRevision < 0)
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidState, "condition_damage_metadata_invalid");
            }
            if (!string.IsNullOrEmpty(damage.DamageId) && !D21ShipIdentityContracts.IsStableId(damage.DamageId, 128))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidReference, "condition_damage_id_invalid");
            }
            if (!string.IsNullOrEmpty(damage.ComponentId) && !D21ShipIdentityContracts.IsStableId(damage.ComponentId, 128))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidReference, "condition_damage_component_invalid");
            }
            return D21ShipConditionValidationResult.Success();
        }

        private static D21ShipConditionValidationResult ValidateRepairCommand(D21ShipConditionCommand command)
        {
            if (!IsSupportedRepairKind(command.RepairKind))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.RepairNotEligible, "condition_repair_kind_unsupported");
            }
            if (!IsFinitePositive(command.RepairAmount))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidAmount, "condition_repair_amount_invalid");
            }
            if (command.RepairContext == null)
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.RepairNotEligible, "condition_repair_context_missing");
            }
            if (!command.RepairContext.OwnerPermission || !command.RepairContext.HasQualifiedCrew || !command.RepairContext.HasMaterials)
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.RepairNotEligible, "condition_repair_resources_missing");
            }
            if (!IsRepairLocationEligible(command.RepairKind, command.RepairContext))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.RepairNotEligible, "condition_repair_location_ineligible");
            }
            if (command.WorldDay < 0 || !string.IsNullOrEmpty(command.RepairContext.CauseFactId) && !D21ShipIdentityContracts.IsStableId(command.RepairContext.CauseFactId, 128))
            {
                return D21ShipConditionValidationResult.Failure(D21ShipConditionFailureCode.InvalidEventId, "condition_repair_fact_invalid");
            }
            return D21ShipConditionValidationResult.Success();
        }

        private static void ApplyDamageTo(D21ShipConditionState candidate, D21ShipConditionCommand command)
        {
            var damage = command.Damage.Clone();
            damage.DamageId = string.IsNullOrEmpty(damage.DamageId) ? "d21_damage_" + command.CommandId : damage.DamageId;
            damage.Active = true;
            damage.SaveRevision = candidate.SaveRevision;
            candidate.DamageEntries.Add(damage);

            if (damage.Kind == D21ShipDamageKind.Structural)
            {
                candidate.Durability = Round(Math.Max(0d, candidate.Durability - damage.Amount));
            }
            else if (damage.Kind == D21ShipDamageKind.Flooding)
            {
                candidate.Flooding = Round(Math.Min(candidate.MaxFlooding, candidate.Flooding + damage.Amount));
            }
            else if (damage.Kind == D21ShipDamageKind.Tilt)
            {
                candidate.TiltDegrees = Round(Math.Min(90d, candidate.TiltDegrees + damage.Amount));
            }
        }

        private static bool TryRepairState(D21ShipConditionState candidate, D21ShipConditionCommand command)
        {
            if (candidate.Seaworthiness == D21ShipSeaworthiness.Lost)
            {
                return false;
            }

            var context = command.RepairContext;
            if (!context.OwnerPermission || !context.HasQualifiedCrew || !context.HasMaterials)
            {
                return false;
            }
            if (!IsRepairLocationEligible(command.RepairKind, context))
            {
                return false;
            }

            var amount = command.RepairAmount;
            var changed = false;
            if (command.RepairKind == D21RepairKind.FullDockRepair)
            {
                changed = candidate.Durability < candidate.MaxDurability || candidate.Flooding > 0d || candidate.TiltDegrees > 0d || HasActiveDamage(candidate);
                candidate.Durability = candidate.MaxDurability;
                candidate.Flooding = 0d;
                candidate.TiltDegrees = 0d;
                DeactivateAllDamage(candidate);
            }
            else
            {
                var targetKind = TargetDamageKind(command.RepairKind);
                if (command.RepairKind == D21RepairKind.StructuralRepair)
                {
                    var nextDurability = Math.Min(candidate.MaxDurability, candidate.Durability + amount);
                    changed = changed || nextDurability > candidate.Durability;
                    candidate.Durability = Round(nextDurability);
                    candidate.TiltDegrees = Round(Math.Max(0d, candidate.TiltDegrees - amount / 10d));
                }
                if (command.RepairKind == D21RepairKind.PumpOut || command.RepairKind == D21RepairKind.TemporaryLeakSeal)
                {
                    var nextFlooding = Math.Max(0d, candidate.Flooding - amount);
                    changed = changed || nextFlooding < candidate.Flooding;
                    candidate.Flooding = Round(nextFlooding);
                }

                for (var index = 0; index < candidate.DamageEntries.Count; index++)
                {
                    var entry = candidate.DamageEntries[index];
                    if (entry == null || !entry.Active || entry.Kind != targetKind)
                    {
                        continue;
                    }

                    var remaining = entry.Amount - amount;
                    if (remaining <= 0d)
                    {
                        entry.Amount = Round(entry.Amount);
                        entry.Active = false;
                    }
                    else
                    {
                        entry.Amount = Round(remaining);
                    }
                    changed = true;
                }
            }

            return changed;
        }

        private static bool IsRepairLocationEligible(D21RepairKind repairKind, D21ShipRepairContext context)
        {
            if (context.Location == D21RepairLocation.Shipyard)
            {
                return context.IsAtPort && context.IsMoored;
            }
            if (context.Location != D21RepairLocation.Onboard)
            {
                return false;
            }
            if (repairKind == D21RepairKind.FullDockRepair || repairKind == D21RepairKind.StructuralRepair || repairKind == D21RepairKind.MastRepair)
            {
                return false;
            }
            return context.CanRepairAtSea || (context.IsAtPort && context.IsMoored);
        }

        private static D21ShipDamageKind TargetDamageKind(D21RepairKind repairKind)
        {
            switch (repairKind)
            {
                case D21RepairKind.TemporaryLeakSeal:
                case D21RepairKind.PumpOut:
                    return D21ShipDamageKind.Flooding;
                case D21RepairKind.SailRepair:
                    return D21ShipDamageKind.Sail;
                case D21RepairKind.RiggingRepair:
                    return D21ShipDamageKind.Rigging;
                case D21RepairKind.RudderRepair:
                    return D21ShipDamageKind.Rudder;
                case D21RepairKind.MastRepair:
                    return D21ShipDamageKind.Mast;
                case D21RepairKind.StructuralRepair:
                    return D21ShipDamageKind.Structural;
                case D21RepairKind.FireSuppression:
                    return D21ShipDamageKind.Fire;
                default:
                    return D21ShipDamageKind.Unknown;
            }
        }

        private static bool DeactivateAllDamage(D21ShipConditionState state)
        {
            var changed = false;
            for (var index = 0; index < state.DamageEntries.Count; index++)
            {
                if (state.DamageEntries[index] != null && state.DamageEntries[index].Active)
                {
                    state.DamageEntries[index].Active = false;
                    changed = true;
                }
            }
            return changed;
        }

        private static bool HasActiveDamage(D21ShipConditionState state)
        {
            for (var index = 0; index < state.DamageEntries.Count; index++)
            {
                if (state.DamageEntries[index] != null && state.DamageEntries[index].Active)
                {
                    return true;
                }
            }
            return false;
        }

        private static D21ShipSeaworthiness EvaluateSeaworthinessOf(D21ShipConditionState state)
        {
            if (state == null || !IsFinitePositive(state.MaxDurability) || !IsFinitePositive(state.MaxFlooding))
            {
                return D21ShipSeaworthiness.Unknown;
            }
            if (state.Durability <= 0d)
            {
                return D21ShipSeaworthiness.Lost;
            }
            if (state.Flooding >= state.MaxFlooding * 0.8d || state.TiltDegrees >= 45d)
            {
                return D21ShipSeaworthiness.Sinking;
            }

            var hasCritical = false;
            var hasMajor = false;
            var hasNavigationDamage = false;
            if (state.DamageEntries != null)
            {
                for (var index = 0; index < state.DamageEntries.Count; index++)
                {
                    var entry = state.DamageEntries[index];
                    if (entry == null || !entry.Active)
                    {
                        continue;
                    }
                    hasCritical = hasCritical || entry.Severity == D21ShipDamageSeverity.Critical;
                    hasMajor = hasMajor || entry.Severity == D21ShipDamageSeverity.Major;
                    hasNavigationDamage = hasNavigationDamage || entry.Kind == D21ShipDamageKind.Rudder || entry.Kind == D21ShipDamageKind.Mast;
                }
            }
            if (hasCritical || state.Durability <= state.MaxDurability * 0.25d)
            {
                return D21ShipSeaworthiness.Disabled;
            }
            if (hasMajor || hasNavigationDamage || state.Flooding > 0d || state.TiltDegrees > 25d)
            {
                return D21ShipSeaworthiness.Restricted;
            }
            return D21ShipSeaworthiness.Navigable;
        }

        private static bool IsSupportedDamageKind(D21ShipDamageKind kind)
        {
            return kind >= D21ShipDamageKind.Structural && kind <= D21ShipDamageKind.PowderSafety;
        }

        private static bool IsSupportedSeverity(D21ShipDamageSeverity severity)
        {
            return severity >= D21ShipDamageSeverity.None && severity <= D21ShipDamageSeverity.Critical;
        }

        private static bool IsSupportedSourceKind(D21DamageSourceKind sourceKind)
        {
            return sourceKind >= D21DamageSourceKind.CombatFact && sourceKind <= D21DamageSourceKind.ExternalFact;
        }

        private static bool IsSupportedRepairKind(D21RepairKind repairKind)
        {
            return repairKind >= D21RepairKind.TemporaryLeakSeal && repairKind <= D21RepairKind.FullDockRepair;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0d;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
        }

        private static double Round(double value)
        {
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }

        private static D21ShipConditionResult Reject(D21ShipConditionCommand command, D21ShipConditionFailureCode code)
        {
            return new D21ShipConditionResult
            {
                Accepted = false,
                Duplicate = false,
                CommandId = command == null ? null : command.CommandId,
                ShipId = command == null ? null : command.ShipId,
                ReasonCode = code,
                State = null,
                SaveRevision = -1,
                EventId = null,
                Seaworthiness = D21ShipSeaworthiness.Unknown,
            };
        }

        private void Remember(string commandId, string fingerprint, D21ShipConditionResult result)
        {
            _commandFingerprints[commandId] = fingerprint;
            _commandResults[commandId] = result.Clone();
        }

        private static void CopyState(D21ShipConditionState source, D21ShipConditionState target)
        {
            target.SchemaVersion = source.SchemaVersion;
            target.ShipId = source.ShipId;
            target.MaxDurability = source.MaxDurability;
            target.Durability = source.Durability;
            target.MaxFlooding = source.MaxFlooding;
            target.Flooding = source.Flooding;
            target.TiltDegrees = source.TiltDegrees;
            target.DamageEntries = source.DamageEntries;
            target.Seaworthiness = source.Seaworthiness;
            target.SaveRevision = source.SaveRevision;
            target.LastEventId = source.LastEventId;
        }
    }
}
