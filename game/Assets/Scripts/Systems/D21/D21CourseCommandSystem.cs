// ============================================================
// D21CourseCommandSystem —— D21-L16 航向命令裁决
//
// 本文件只处理航向命令的引用、权限、幂等和冷却。船况、锚泊、港口、
// 船员和天气都以公开只读快照/资格输入提供；命令失败时返回原状态 clone，
// 不写任何上游事实。航向使用 centidegree 定点整数，避免浮点角度在重放
// 时产生跨平台漂移。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>D21 航向命令种类；输入层只负责转换为这些逻辑命令。</summary>
    public enum D21CourseCommandType
    {
        None = 0,
        SetCourse = 1,
        ChangeCourse = 2,
        Stop = 3,
        Evade = 4,
    }

    /// <summary>船侧航向的互斥逻辑模式，不映射到驾驶动画。</summary>
    public enum D21CourseMode
    {
        None = 0,
        Course = 1,
        Stopped = 2,
        Evading = 3,
    }

    /// <summary>
    /// 船况/锚泊/港口/船员 owner 提交给航向系统的只读资格快照。
    /// D21 不保存或修改这些字段。
    /// </summary>
    [Serializable]
    public sealed class D21CourseAuthoritySnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool VoyageActive;
        public bool IsAnchored;
        public bool IsBerthed;
        public bool IsSeaworthy = true;
        public bool CrewReady = true;
        public int CurrentTickIndex = -1;
        public long ExpectedSaveRevision = -1;

        public D21CourseAuthoritySnapshot Clone()
        {
            return new D21CourseAuthoritySnapshot
            {
                SchemaVersion = SchemaVersion,
                VoyageActive = VoyageActive,
                IsAnchored = IsAnchored,
                IsBerthed = IsBerthed,
                IsSeaworthy = IsSeaworthy,
                CrewReady = CrewReady,
                CurrentTickIndex = CurrentTickIndex,
                ExpectedSaveRevision = ExpectedSaveRevision,
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 course authority schema_version mismatch.");
            if (CurrentTickIndex < -1 || ExpectedSaveRevision < -1)
                throw new InvalidOperationException("D21 course authority counters are outside their ranges.");
        }
    }

    /// <summary>航向命令；TickContext 是调用方显式提供的逻辑时钟。</summary>
    [Serializable]
    public sealed class D21CourseCommand
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public D21CourseCommandType Type;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public bool HasHeading;
        public int HeadingCentidegrees;
        public int DesiredSpeedPermille = 1000;
        public bool HazardDetected;
        public D21TickContext TickContext;
        public D21CourseAuthoritySnapshot Authority;

        /// <summary>面向调用方的角度属性；写入时立即量化为 centidegree。</summary>
        public float HeadingDegrees
        {
            get { return HeadingCentidegrees / 100f; }
            set
            {
                HasHeading = true;
                HeadingCentidegrees = NormalizeHeading(value);
            }
        }

        public D21CourseCommand Clone()
        {
            return new D21CourseCommand
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                Type = Type,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                HasHeading = HasHeading,
                HeadingCentidegrees = HeadingCentidegrees,
                DesiredSpeedPermille = DesiredSpeedPermille,
                HazardDetected = HazardDetected,
                TickContext = TickContext == null ? null : TickContext.Clone(),
                Authority = Authority == null ? null : Authority.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 course command schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(CommandId) || string.IsNullOrWhiteSpace(VoyageId) ||
                string.IsNullOrWhiteSpace(ShipId))
                throw new InvalidOperationException("D21 course command ids are required.");
            if (!Enum.IsDefined(typeof(D21CourseCommandType), Type) || Type == D21CourseCommandType.None)
                throw new InvalidOperationException("D21 course command type is unknown.");
            if (HeadingCentidegrees < 0 || HeadingCentidegrees >= 36000)
                throw new InvalidOperationException("D21 course heading is outside [0, 360) degrees.");
            if (DesiredSpeedPermille < 0 || DesiredSpeedPermille > 2000)
                throw new InvalidOperationException("D21 desired speed multiplier is outside [0, 2000].");
            if (TickContext == null) throw new InvalidOperationException("D21 course tick context is required.");
            TickContext.Validate();
            if (Authority != null) Authority.Validate();
        }

        internal string Fingerprint
        {
            get
            {
                return Type.ToString() + "|" + (VoyageId ?? string.Empty) + "|" +
                    (ShipId ?? string.Empty) + "|" + (RouteId ?? string.Empty) + "|" +
                    HeadingCentidegrees.ToString(CultureInfo.InvariantCulture) + "|" +
                    DesiredSpeedPermille.ToString(CultureInfo.InvariantCulture) + "|" +
                    (HazardDetected ? "1" : "0");
            }
        }

        internal static int NormalizeHeading(float degrees)
        {
            if (float.IsNaN(degrees) || float.IsInfinity(degrees)) return 0;
            double normalized = degrees % 360d;
            if (normalized < 0d) normalized += 360d;
            int result = (int)Math.Round(normalized * 100d, MidpointRounding.AwayFromZero);
            result %= 36000;
            return result < 0 ? result + 36000 : result;
        }
    }

    /// <summary>成功命令的最小幂等记录；不复制任何船况或港口事实。</summary>
    [Serializable]
    public sealed class D21CourseCommandReceipt
    {
        public string CommandId = string.Empty;
        public string Fingerprint = string.Empty;
        public int AcceptedTick;
        public string ResultCode = string.Empty;

        public D21CourseCommandReceipt Clone()
        {
            return new D21CourseCommandReceipt
            {
                CommandId = CommandId ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
                AcceptedTick = AcceptedTick,
                ResultCode = ResultCode ?? string.Empty,
            };
        }
    }

    /// <summary>航向 owner 保存的纯逻辑状态。</summary>
    [Serializable]
    public sealed class D21CourseState
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public D21CourseMode Mode = D21CourseMode.None;
        public bool HasCourse;
        public bool IsStopped;
        public int HeadingCentidegrees;
        public int SpeedPermille;
        public int LastAcceptedTick = -1;
        public int CooldownUntilTick;
        public string LastCommandId = string.Empty;
        public List<D21CourseCommandReceipt> AppliedCommands = new List<D21CourseCommandReceipt>();

        public static D21CourseState CreateInitial(string voyageId, string shipId)
        {
            return new D21CourseState
            {
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                Mode = D21CourseMode.None,
                HasCourse = false,
                IsStopped = false,
                HeadingCentidegrees = 0,
                SpeedPermille = 0,
            };
        }

        public D21CourseState Clone()
        {
            var result = new D21CourseState
            {
                SchemaVersion = SchemaVersion,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                Mode = Mode,
                HasCourse = HasCourse,
                IsStopped = IsStopped,
                HeadingCentidegrees = HeadingCentidegrees,
                SpeedPermille = SpeedPermille,
                LastAcceptedTick = LastAcceptedTick,
                CooldownUntilTick = CooldownUntilTick,
                LastCommandId = LastCommandId ?? string.Empty,
            };
            if (AppliedCommands != null)
                foreach (D21CourseCommandReceipt receipt in AppliedCommands)
                    if (receipt != null) result.AppliedCommands.Add(receipt.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 course state schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(VoyageId) || string.IsNullOrWhiteSpace(ShipId))
                throw new InvalidOperationException("D21 course state ids are required.");
            if (!Enum.IsDefined(typeof(D21CourseMode), Mode))
                throw new InvalidOperationException("D21 course state mode is unknown.");
            if (HeadingCentidegrees < 0 || HeadingCentidegrees >= 36000 ||
                SpeedPermille < 0 || SpeedPermille > 2000 || LastAcceptedTick < -1 ||
                CooldownUntilTick < 0)
                throw new InvalidOperationException("D21 course state is outside its saved ranges.");
            if (!HasCourse && Mode == D21CourseMode.Course)
                throw new InvalidOperationException("D21 course state has Course mode without a course.");
            if (IsStopped && Mode != D21CourseMode.Stopped)
                throw new InvalidOperationException("D21 course stopped flag disagrees with mode.");
            if (AppliedCommands == null) throw new InvalidOperationException("D21 course receipts are required.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (D21CourseCommandReceipt receipt in AppliedCommands)
            {
                if (receipt == null || string.IsNullOrWhiteSpace(receipt.CommandId) ||
                    !ids.Add(receipt.CommandId))
                    throw new InvalidOperationException("D21 course command receipts contain a duplicate or empty id.");
            }
        }

        public string Fingerprint
        {
            get
            {
                return "d21course_v" + SchemaVersion.ToString(CultureInfo.InvariantCulture) + "_" +
                    (VoyageId ?? string.Empty) + "_" + Mode + "_" +
                    HeadingCentidegrees.ToString(CultureInfo.InvariantCulture) + "_" +
                    SpeedPermille.ToString(CultureInfo.InvariantCulture) + "_" +
                    LastAcceptedTick.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>命令裁决结果；Rejected/Duplicate 均不修改传入状态。</summary>
    [Serializable]
    public sealed class D21CourseCommandResult
    {
        public bool Accepted;
        public bool IsDuplicate;
        public string Code = string.Empty;
        public string CommandId = string.Empty;
        public int CooldownUntilTick;
        public D21CourseState State;
    }

    /// <summary>纯函数航向命令 owner。</summary>
    public static class D21CourseCommandSystem
    {
        public const int SetCourseCooldownTicks = 2;
        public const int ChangeCourseCooldownTicks = 2;
        public const int StopCooldownTicks = 1;
        public const int EvadeCooldownTicks = 4;

        public static D21CourseCommandResult Apply(
            D21CourseState current,
            D21CourseCommand command,
            D21CourseAuthoritySnapshot authority = null)
        {
            if (current == null) return Rejected(null, command, "missing_state");
            D21CourseState original = current.Clone();
            try
            {
                original.Validate();
                if (command == null) return Rejected(original, null, "missing_command");
                command.Validate();
                if (authority == null) authority = command.Authority;
                if (authority == null) return Rejected(original, command, "missing_authority_snapshot");
                authority.Validate();

                if (!string.Equals(original.VoyageId, command.VoyageId, StringComparison.Ordinal) ||
                    !string.Equals(original.ShipId, command.ShipId, StringComparison.Ordinal))
                    return Rejected(original, command, "owner_id_mismatch");
                if (authority.CurrentTickIndex >= 0 &&
                    authority.CurrentTickIndex != command.TickContext.TickIndex)
                    return Rejected(original, command, "authority_tick_mismatch");
                if (authority.ExpectedSaveRevision >= 0 &&
                    authority.ExpectedSaveRevision != command.TickContext.SaveRevision)
                    return Rejected(original, command, "authority_revision_mismatch");

                D21CourseCommandReceipt previous = FindReceipt(original, command.CommandId);
                if (previous != null)
                {
                    if (!string.Equals(previous.Fingerprint, command.Fingerprint, StringComparison.Ordinal))
                        return Rejected(original, command, "command_id_conflict");
                    return new D21CourseCommandResult
                    {
                        Accepted = true,
                        IsDuplicate = true,
                        Code = "duplicate_command",
                        CommandId = command.CommandId,
                        CooldownUntilTick = original.CooldownUntilTick,
                        State = original.Clone(),
                    };
                }

                int tick = command.TickContext.TickIndex;
                if (tick < original.CooldownUntilTick)
                    return Rejected(original, command, "course_cooldown");
                if (!authority.VoyageActive)
                    return Rejected(original, command, "voyage_inactive");
                if (!authority.IsSeaworthy)
                    return Rejected(original, command, "ship_not_seaworthy");
                if (!authority.CrewReady)
                    return Rejected(original, command, "crew_not_ready");

                if (command.Type != D21CourseCommandType.Stop &&
                    (authority.IsAnchored || authority.IsBerthed))
                    return Rejected(original, command, "ship_is_moored");

                D21CourseState next = original.Clone();
                int cooldown;
                switch (command.Type)
                {
                    case D21CourseCommandType.SetCourse:
                        if (!command.HasHeading) return Rejected(original, command, "heading_required");
                        if (command.DesiredSpeedPermille <= 0) return Rejected(original, command, "speed_required");
                        next.Mode = D21CourseMode.Course;
                        next.HasCourse = true;
                        next.IsStopped = false;
                        next.HeadingCentidegrees = command.HeadingCentidegrees;
                        next.SpeedPermille = command.DesiredSpeedPermille;
                        cooldown = SetCourseCooldownTicks;
                        break;

                    case D21CourseCommandType.ChangeCourse:
                        if (!next.HasCourse || next.IsStopped)
                            return Rejected(original, command, "no_active_course");
                        if (!command.HasHeading) return Rejected(original, command, "heading_required");
                        if (command.DesiredSpeedPermille <= 0) return Rejected(original, command, "speed_required");
                        next.Mode = D21CourseMode.Course;
                        next.IsStopped = false;
                        next.HeadingCentidegrees = command.HeadingCentidegrees;
                        next.SpeedPermille = command.DesiredSpeedPermille;
                        cooldown = ChangeCourseCooldownTicks;
                        break;

                    case D21CourseCommandType.Stop:
                        if (!next.HasCourse || next.IsStopped)
                            return Rejected(original, command, "no_active_course");
                        next.Mode = D21CourseMode.Stopped;
                        next.HasCourse = false;
                        next.IsStopped = true;
                        next.SpeedPermille = 0;
                        cooldown = StopCooldownTicks;
                        break;

                    case D21CourseCommandType.Evade:
                        if (!command.HazardDetected) return Rejected(original, command, "hazard_not_declared");
                        if (!next.HasCourse && !command.HasHeading)
                            return Rejected(original, command, "heading_required");
                        next.Mode = D21CourseMode.Evading;
                        next.HasCourse = true;
                        next.IsStopped = false;
                        next.HeadingCentidegrees = command.HasHeading
                            ? command.HeadingCentidegrees
                            : (next.HeadingCentidegrees + 4500) % 36000;
                        next.SpeedPermille = command.DesiredSpeedPermille <= 0 ? 1000 : command.DesiredSpeedPermille;
                        cooldown = EvadeCooldownTicks;
                        break;

                    default:
                        return Rejected(original, command, "unknown_command_type");
                }

                next.LastAcceptedTick = tick;
                next.CooldownUntilTick = checked(tick + cooldown);
                next.LastCommandId = command.CommandId;
                next.AppliedCommands.Add(new D21CourseCommandReceipt
                {
                    CommandId = command.CommandId,
                    Fingerprint = command.Fingerprint,
                    AcceptedTick = tick,
                    ResultCode = "course_command_applied",
                });
                next.Validate();
                return new D21CourseCommandResult
                {
                    Accepted = true,
                    IsDuplicate = false,
                    Code = "course_command_applied",
                    CommandId = command.CommandId,
                    CooldownUntilTick = next.CooldownUntilTick,
                    State = next,
                };
            }
            catch (OverflowException)
            {
                return Rejected(original, command, "counter_overflow");
            }
            catch (InvalidOperationException)
            {
                return Rejected(original, command, "invalid_course_input");
            }
        }

        public static D21CourseCommandResult Apply(D21CourseState current, D21CourseCommand command)
        {
            return Apply(current, command, command == null ? null : command.Authority);
        }

        public static bool CanApply(
            D21CourseState current,
            D21CourseCommand command,
            D21CourseAuthoritySnapshot authority = null)
        {
            return Apply(current, command, authority).Accepted;
        }

        private static D21CourseCommandReceipt FindReceipt(D21CourseState state, string commandId)
        {
            if (state.AppliedCommands == null) return null;
            foreach (D21CourseCommandReceipt receipt in state.AppliedCommands)
                if (receipt != null && string.Equals(receipt.CommandId, commandId, StringComparison.Ordinal))
                    return receipt;
            return null;
        }

        private static D21CourseCommandResult Rejected(
            D21CourseState state,
            D21CourseCommand command,
            string code)
        {
            return new D21CourseCommandResult
            {
                Accepted = false,
                IsDuplicate = false,
                Code = code ?? "rejected",
                CommandId = command == null ? string.Empty : command.CommandId ?? string.Empty,
                CooldownUntilTick = state == null ? 0 : state.CooldownUntilTick,
                State = state == null ? null : state.Clone(),
            };
        }
    }
}
