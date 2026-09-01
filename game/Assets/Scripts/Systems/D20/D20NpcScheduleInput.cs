// D20NpcScheduleInput —— D20-L16 NPC 日程输入的纯 C# 契约。
//
// 日程只携带由时间、天气、危险和既有 owner 提供的只读事实。它是行为
// 状态机的输入，不是岗位、健康、经济或位置事实的第二个写者；位置只允许
// 使用稳定的语义引用，因此本文件不需要引擎对象、坐标或寻路组件。

using System;

namespace Whalebone.Systems.D20
{
    /// <summary>NPC 日程使用的六个世界时段。</summary>
    public enum D20NpcTimeSlot
    {
        Unknown = 0,
        Dawn = 1,
        Morning = 2,
        Midday = 3,
        Afternoon = 4,
        Dusk = 5,
        Night = 6,
    }

    /// <summary>日程层需要的天气类别；不包含渲染或海况对象。</summary>
    public enum D20NpcWeatherKind
    {
        Unknown = 0,
        Clear = 1,
        Cloudy = 2,
        Rain = 3,
        Wind = 4,
        Storm = 5,
        Fog = 6,
    }

    /// <summary>日程层的危险等级；危险后果仍由所属系统裁决。</summary>
    public enum D20NpcDangerLevel
    {
        None = 0,
        Low = 1,
        High = 2,
        Critical = 3,
    }

    /// <summary>NPC 当前时段的语义工作/生活活动。</summary>
    public enum D20NpcDutyKind
    {
        None = 0,
        Pray = 1,
        PrepareTools = 2,
        Produce = 3,
        Trade = 4,
        Load = 5,
        Patrol = 6,
        Meal = 7,
        Rest = 8,
        Market = 9,
        Shipyard = 10,
        Tavern = 11,
        Guard = 12,
        Sleep = 13,
        SecretTrade = 14,
        Ceremony = 15,
        Fishing = 16,
    }

    /// <summary>日程输入的稳定校验码。</summary>
    public enum D20NpcScheduleInputValidationCode
    {
        None = 0,
        MissingInput = 1,
        InvalidSchemaVersion = 2,
        MissingNpcId = 3,
        InvalidRoleId = 4,
        InvalidLocationRef = 5,
        NegativeWorldDay = 6,
        UnknownTimeSlot = 7,
        UnknownWeather = 8,
        UnknownDangerLevel = 9,
        UnknownDuty = 10,
        UnknownAvailability = 11,
        NegativeSaveRevision = 12,
        WorkDutyMissing = 13,
    }

    /// <summary>日程输入校验结果；失败不会修正或写回原输入。</summary>
    [Serializable]
    public sealed class D20NpcScheduleInputValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20NpcScheduleInputValidationCode Code;

        /// <summary>机器可读原因。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20NpcScheduleInputValidationResult Success()
        {
            return new D20NpcScheduleInputValidationResult
            {
                Valid = true,
                Code = D20NpcScheduleInputValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20NpcScheduleInputValidationResult Failure(
            D20NpcScheduleInputValidationCode code,
            string reasonCode)
        {
            return new D20NpcScheduleInputValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D20NpcScheduleInputValidationResult Clone()
        {
            return new D20NpcScheduleInputValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// 供 D20 NPC 行为使用的只读日程输入。该对象不保存日程历史，也不拥有
    /// 岗位分配；调用方需要在每次逻辑推进前提交新的完整输入。
    /// </summary>
    [Serializable]
    public sealed class D20NpcScheduleInput
    {
        /// <summary>当前日程输入契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>日程输入契约的 owner 标识。</summary>
        public const string OwnerSystem = "D20NpcScheduleInput";

        /// <summary>稳定引用的最大长度。</summary>
        public const int ReferenceMaxLength = D20CharacterIdentityContracts.ReferenceMaxLength;

        /// <summary>输入契约版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>需要消费该输入的 NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>岗位引用；未知但格式正确的值由 L15 语义保留。</summary>
        public string RoleId = string.Empty;

        /// <summary>世界日，必须是非负逻辑值，不读取系统时钟。</summary>
        public int WorldDay;

        /// <summary>当前世界时段。</summary>
        public D20NpcTimeSlot TimeSlot = D20NpcTimeSlot.Unknown;

        /// <summary>当前时段的语义活动。</summary>
        public D20NpcDutyKind ScheduledDuty = D20NpcDutyKind.None;

        /// <summary>owner 提供的天气类别。</summary>
        public D20NpcWeatherKind Weather = D20NpcWeatherKind.Unknown;

        /// <summary>owner 提供的危险等级。</summary>
        public D20NpcDangerLevel DangerLevel = D20NpcDangerLevel.None;

        /// <summary>角色动作层的公开可用性快照。</summary>
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;

        /// <summary>当前输入是否处在岗位工作窗口。</summary>
        public bool IsWorkPeriod;

        /// <summary>既有行为 owner 是否报告 NPC 正被不可覆盖的工作占用。</summary>
        public bool IsBusy;

        /// <summary>既有健康/疲劳 owner 是否要求先休息。</summary>
        public bool IsRestRequired;

        /// <summary>既有健康 owner 是否报告受伤；D20 不修改该事实。</summary>
        public bool IsInjured;

        /// <summary>危险或天气发生时是否存在语义避难处。</summary>
        public bool ShelterAvailable;

        /// <summary>可选的稳定地点引用，不是坐标、场景或空间对象。</summary>
        public string LocationRef = string.Empty;

        /// <summary>输入对应的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>是否存在危险，便于下游只读消费。</summary>
        public bool IsDangerPresent
        {
            get { return DangerLevel != D20NpcDangerLevel.None; }
        }

        /// <summary>是否存在工作活动。</summary>
        public bool HasScheduledDuty
        {
            get { return ScheduledDuty != D20NpcDutyKind.None; }
        }

        /// <summary>返回不共享可变字段引用的副本。</summary>
        public D20NpcScheduleInput Clone()
        {
            return new D20NpcScheduleInput
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId ?? string.Empty,
                RoleId = RoleId ?? string.Empty,
                WorldDay = WorldDay,
                TimeSlot = TimeSlot,
                ScheduledDuty = ScheduledDuty,
                Weather = Weather,
                DangerLevel = DangerLevel,
                Availability = Availability,
                IsWorkPeriod = IsWorkPeriod,
                IsBusy = IsBusy,
                IsRestRequired = IsRestRequired,
                IsInjured = IsInjured,
                ShelterAvailable = ShelterAvailable,
                LocationRef = LocationRef ?? string.Empty,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>验证输入；失败时不会自动规范化字段。</summary>
        public void Validate()
        {
            D20NpcScheduleInputValidationResult result =
                D20NpcScheduleInputContracts.Validate(this);
            if (!result.Valid)
                throw new InvalidOperationException(result.ReasonCode);
        }
    }

    /// <summary>D20-L16 日程字段、枚举和工作活动的单一校验入口。</summary>
    public static class D20NpcScheduleInputContracts
    {
        /// <summary>日程输入契约版本。</summary>
        public const int CurrentSchemaVersion = D20NpcScheduleInput.CurrentSchemaVersion;

        /// <summary>日程输入的逻辑 owner。</summary>
        public const string OwnerSystem = D20NpcScheduleInput.OwnerSystem;

        /// <summary>验证完整日程输入。</summary>
        public static D20NpcScheduleInputValidationResult Validate(
            D20NpcScheduleInput input)
        {
            if (input == null)
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.MissingInput,
                    "npc_schedule_input_missing");
            if (input.SchemaVersion != CurrentSchemaVersion)
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.InvalidSchemaVersion,
                    "npc_schedule_schema_version_mismatch");
            if (!D20NpcIdentityContracts.IsStableNpcId(input.NpcId))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.MissingNpcId,
                    "npc_schedule_npc_id_missing");
            if (!D20NpcIdentityContracts.IsStableReference(input.RoleId, true))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.InvalidRoleId,
                    "npc_schedule_role_id_invalid");
            if (!D20NpcIdentityContracts.IsStableReference(input.LocationRef, true))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.InvalidLocationRef,
                    "npc_schedule_location_ref_invalid");
            if (input.WorldDay < 0)
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.NegativeWorldDay,
                    "npc_schedule_world_day_negative");
            if (!IsSupportedTimeSlot(input.TimeSlot))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.UnknownTimeSlot,
                    "npc_schedule_time_slot_unknown");
            if (!IsSupportedWeather(input.Weather))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.UnknownWeather,
                    "npc_schedule_weather_unknown");
            if (!IsSupportedDangerLevel(input.DangerLevel))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.UnknownDangerLevel,
                    "npc_schedule_danger_level_unknown");
            if (!IsSupportedDuty(input.ScheduledDuty))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.UnknownDuty,
                    "npc_schedule_duty_unknown");
            if (!D20CharacterIdentityContracts.IsSupportedAvailability(input.Availability))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.UnknownAvailability,
                    "npc_schedule_availability_unknown");
            if (input.SaveRevision < 0)
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.NegativeSaveRevision,
                    "npc_schedule_save_revision_negative");
            if (input.IsWorkPeriod && !IsWorkDuty(input.ScheduledDuty))
                return D20NpcScheduleInputValidationResult.Failure(
                    D20NpcScheduleInputValidationCode.WorkDutyMissing,
                    "npc_schedule_work_duty_missing");

            return D20NpcScheduleInputValidationResult.Success();
        }

        /// <summary>验证时段是否为当前版本支持值。</summary>
        public static bool IsSupportedTimeSlot(D20NpcTimeSlot timeSlot)
        {
            return timeSlot >= D20NpcTimeSlot.Dawn &&
                   timeSlot <= D20NpcTimeSlot.Night;
        }

        /// <summary>验证天气是否为当前版本支持值。</summary>
        public static bool IsSupportedWeather(D20NpcWeatherKind weather)
        {
            return weather >= D20NpcWeatherKind.Clear &&
                   weather <= D20NpcWeatherKind.Fog;
        }

        /// <summary>验证危险等级是否为当前版本支持值。</summary>
        public static bool IsSupportedDangerLevel(D20NpcDangerLevel dangerLevel)
        {
            return dangerLevel >= D20NpcDangerLevel.None &&
                   dangerLevel <= D20NpcDangerLevel.Critical;
        }

        /// <summary>验证活动枚举是否为当前版本支持值。</summary>
        public static bool IsSupportedDuty(D20NpcDutyKind duty)
        {
            return duty >= D20NpcDutyKind.None &&
                   duty <= D20NpcDutyKind.Fishing;
        }

        /// <summary>判断活动是否属于工作而不是生活/休息活动。</summary>
        public static bool IsWorkDuty(D20NpcDutyKind duty)
        {
            switch (duty)
            {
                case D20NpcDutyKind.PrepareTools:
                case D20NpcDutyKind.Produce:
                case D20NpcDutyKind.Trade:
                case D20NpcDutyKind.Load:
                case D20NpcDutyKind.Patrol:
                case D20NpcDutyKind.Market:
                case D20NpcDutyKind.Shipyard:
                case D20NpcDutyKind.Tavern:
                case D20NpcDutyKind.Guard:
                case D20NpcDutyKind.SecretTrade:
                case D20NpcDutyKind.Fishing:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>判断活动是否明确要求休息或睡眠。</summary>
        public static bool IsRestDuty(D20NpcDutyKind duty)
        {
            return duty == D20NpcDutyKind.Meal ||
                   duty == D20NpcDutyKind.Rest ||
                   duty == D20NpcDutyKind.Sleep;
        }

        /// <summary>判断天气是否足以阻断普通室外工作。</summary>
        public static bool IsSevereWeather(D20NpcWeatherKind weather)
        {
            return weather == D20NpcWeatherKind.Storm;
        }

        /// <summary>判断危险是否达到警戒状态。</summary>
        public static bool IsAlarmDanger(D20NpcDangerLevel dangerLevel)
        {
            return dangerLevel >= D20NpcDangerLevel.High;
        }
    }
}
