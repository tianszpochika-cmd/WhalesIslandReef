// ============================================================
// D21CrewRequirementRules —— D21-L06 船员最低配置与出航资格（纯 C#）
//
// 本文件只消费船员公开只读投影。船员身份、角色资格、疲劳和士气的写入
// 仍归属于船员 owner；D20 尚未提供已通过的公共投影时，本规则不能宣称
// 已完成跨系统集成。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>船员可承担的逻辑岗位。</summary>
    public enum D21CrewRole
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>掌舵岗位。</summary>
        Helmsman = 1,
        /// <summary>帆缆与航行操作岗位。</summary>
        Sailmaster = 2,
        /// <summary>瞭望岗位。</summary>
        Lookout = 3,
        /// <summary>航海计算岗位。</summary>
        Navigator = 4,
        /// <summary>木工与船体维护岗位。</summary>
        Carpenter = 5,
        /// <summary>厨务岗位。</summary>
        Cook = 6,
        /// <summary>货物管理岗位。</summary>
        CargoMaster = 7,
        /// <summary>小艇和缆泊岗位。</summary>
        Boatmaster = 8,
        /// <summary>火炮操作岗位。</summary>
        Gunner = 9,
        /// <summary>医疗照护岗位。</summary>
        Medic = 10,
        /// <summary>普通甲板岗位。</summary>
        Deckhand = 11,
    }

    /// <summary>船员配置对出航的资格等级。</summary>
    public enum D21CrewEligibilityState
    {
        /// <summary>无法完成评估。</summary>
        Unknown = 0,
        /// <summary>满足最低出航条件。</summary>
        Eligible = 1,
        /// <summary>可以出航但效率或余量受限。</summary>
        Restricted = 2,
        /// <summary>不允许出航。</summary>
        Refused = 3,
    }

    /// <summary>船员规则校验或评估失败的稳定原因码。</summary>
    public enum D21CrewRequirementFailureCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>请求或输入缺失。</summary>
        InvalidRequest = 1,
        /// <summary>配置档无效。</summary>
        InvalidProfile = 2,
        /// <summary>船员投影无效。</summary>
        InvalidRoster = 3,
        /// <summary>船员成员数据无效。</summary>
        InvalidCrewMember = 4,
        /// <summary>可用船员人数不足。</summary>
        InsufficientCrew = 5,
        /// <summary>缺少必需岗位。</summary>
        MissingRequiredRole = 6,
        /// <summary>平均士气低于最低值。</summary>
        MoraleTooLow = 7,
        /// <summary>平均疲劳超过最高值。</summary>
        FatigueTooHigh = 8,
        /// <summary>平均健康低于最低值。</summary>
        HealthTooLow = 9,
        /// <summary>输入保存版本无效。</summary>
        InvalidSaveRevision = 10,
    }

    /// <summary>一个船员的只读资格投影。</summary>
    public sealed class D21CrewMemberReadSnapshot
    {
        /// <summary>创建空船员投影。</summary>
        public D21CrewMemberReadSnapshot()
        {
            QualifiedRoles = new List<D21CrewRole>();
        }

        /// <summary>船员稳定 ID。</summary>
        public string CrewId { get; set; }

        /// <summary>该船员已取得的岗位资格。</summary>
        public List<D21CrewRole> QualifiedRoles { get; set; }

        /// <summary>船员是否可被排入本次值勤。</summary>
        public bool IsAvailable { get; set; }

        /// <summary>船员士气，范围为 0 到 100。</summary>
        public double MoraleScore { get; set; }

        /// <summary>船员疲劳，范围为 0 到 100。</summary>
        public double FatigueScore { get; set; }

        /// <summary>船员健康，范围为 0 到 100。</summary>
        public double HealthScore { get; set; }

        /// <summary>船员是否失去执行岗位的能力。</summary>
        public bool IsIncapacitated { get; set; }

        /// <summary>复制船员只读投影。</summary>
        public D21CrewMemberReadSnapshot Clone()
        {
            var clone = new D21CrewMemberReadSnapshot
            {
                CrewId = CrewId,
                IsAvailable = IsAvailable,
                MoraleScore = MoraleScore,
                FatigueScore = FatigueScore,
                HealthScore = HealthScore,
                IsIncapacitated = IsIncapacitated,
            };
            if (QualifiedRoles != null)
            {
                clone.QualifiedRoles.AddRange(QualifiedRoles);
            }
            return clone;
        }
    }

    /// <summary>一个岗位的最低人数要求。</summary>
    public sealed class D21CrewRoleRequirement
    {
        /// <summary>岗位类别。</summary>
        public D21CrewRole Role { get; set; }

        /// <summary>该岗位至少需要的人数。</summary>
        public int MinimumCount { get; set; }

        /// <summary>复制岗位要求。</summary>
        public D21CrewRoleRequirement Clone()
        {
            return new D21CrewRoleRequirement
            {
                Role = Role,
                MinimumCount = MinimumCount,
            };
        }
    }

    /// <summary>一条船的最低船员配置档。</summary>
    public sealed class D21CrewRequirementProfile
    {
        /// <summary>创建空配置档。</summary>
        public D21CrewRequirementProfile()
        {
            SchemaVersion = D21CrewRequirementRules.CurrentSchemaVersion;
            RequiredRoles = new List<D21CrewRoleRequirement>();
        }

        /// <summary>配置契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>允许出航的最低可用人数。</summary>
        public int MinimumCrew { get; set; }

        /// <summary>推荐标准人数。</summary>
        public int StandardCrew { get; set; }

        /// <summary>配置档允许的最大人数。</summary>
        public int MaximumCrew { get; set; }

        /// <summary>允许出航的最低平均士气。</summary>
        public double MinimumAverageMorale { get; set; }

        /// <summary>允许出航的最高平均疲劳。</summary>
        public double MaximumAverageFatigue { get; set; }

        /// <summary>允许出航的最低平均健康。</summary>
        public double MinimumAverageHealth { get; set; }

        /// <summary>必需岗位集合。</summary>
        public List<D21CrewRoleRequirement> RequiredRoles { get; set; }

        /// <summary>复制配置档。</summary>
        public D21CrewRequirementProfile Clone()
        {
            var clone = new D21CrewRequirementProfile
            {
                SchemaVersion = SchemaVersion,
                MinimumCrew = MinimumCrew,
                StandardCrew = StandardCrew,
                MaximumCrew = MaximumCrew,
                MinimumAverageMorale = MinimumAverageMorale,
                MaximumAverageFatigue = MaximumAverageFatigue,
                MinimumAverageHealth = MinimumAverageHealth,
            };
            if (RequiredRoles != null)
            {
                for (var index = 0; index < RequiredRoles.Count; index++)
                {
                    if (RequiredRoles[index] != null)
                    {
                        clone.RequiredRoles.Add(RequiredRoles[index].Clone());
                    }
                }
            }
            return clone;
        }
    }

    /// <summary>船员 owner 对 D21 暴露的只读船员名单。</summary>
    public sealed class D21CrewRosterSnapshot
    {
        /// <summary>创建空船员投影。</summary>
        public D21CrewRosterSnapshot()
        {
            SchemaVersion = D21CrewRequirementRules.CurrentSchemaVersion;
            CrewMembers = new List<D21CrewMemberReadSnapshot>();
        }

        /// <summary>投影契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>目标船只稳定 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>船员只读成员。</summary>
        public List<D21CrewMemberReadSnapshot> CrewMembers { get; set; }

        /// <summary>投影保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>上游投影标识，可用于证据追踪。</summary>
        public string SourceProjectionId { get; set; }

        /// <summary>复制船员投影。</summary>
        public D21CrewRosterSnapshot Clone()
        {
            var clone = new D21CrewRosterSnapshot
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId,
                SaveRevision = SaveRevision,
                SourceProjectionId = SourceProjectionId,
            };
            if (CrewMembers != null)
            {
                for (var index = 0; index < CrewMembers.Count; index++)
                {
                    if (CrewMembers[index] != null)
                    {
                        clone.CrewMembers.Add(CrewMembers[index].Clone());
                    }
                }
            }
            return clone;
        }
    }

    /// <summary>确定性岗位分配结果。</summary>
    public sealed class D21CrewRoleAssignment
    {
        /// <summary>岗位类别。</summary>
        public D21CrewRole Role { get; set; }

        /// <summary>被分配的船员 ID。</summary>
        public string CrewId { get; set; }

        /// <summary>复制岗位分配。</summary>
        public D21CrewRoleAssignment Clone()
        {
            return new D21CrewRoleAssignment
            {
                Role = Role,
                CrewId = CrewId,
            };
        }
    }

    /// <summary>船员最低配置评估结果。</summary>
    public sealed class D21CrewRequirementDecision
    {
        /// <summary>创建空评估结果。</summary>
        public D21CrewRequirementDecision()
        {
            Eligibility = D21CrewEligibilityState.Unknown;
            ReasonCode = D21CrewRequirementFailureCode.None;
            MissingRoles = new ReadOnlyCollection<D21CrewRole>(new List<D21CrewRole>());
            ReasonCodes = new ReadOnlyCollection<string>(new List<string>());
            Assignments = new ReadOnlyCollection<D21CrewRoleAssignment>(new List<D21CrewRoleAssignment>());
        }

        /// <summary>输入是否有效。</summary>
        public bool Valid { get; set; }

        /// <summary>船员配置资格等级。</summary>
        public D21CrewEligibilityState Eligibility { get; set; }

        /// <summary>是否允许出航。</summary>
        public bool CanDepart { get; set; }

        /// <summary>可用且未失能的船员人数。</summary>
        public int ActiveCrewCount { get; set; }

        /// <summary>当前平均士气。</summary>
        public double AverageMorale { get; set; }

        /// <summary>当前平均疲劳。</summary>
        public double AverageFatigue { get; set; }

        /// <summary>当前平均健康。</summary>
        public double AverageHealth { get; set; }

        /// <summary>最低人数要求。</summary>
        public int RequiredCrewCount { get; set; }

        /// <summary>船员状态对航行效率的确定性系数。</summary>
        public double NavigationEfficiencyMultiplier { get; set; }

        /// <summary>首个稳定原因码。</summary>
        public D21CrewRequirementFailureCode ReasonCode { get; set; }

        /// <summary>缺失的岗位。</summary>
        public ReadOnlyCollection<D21CrewRole> MissingRoles { get; set; }

        /// <summary>稳定诊断原因集合。</summary>
        public ReadOnlyCollection<string> ReasonCodes { get; set; }

        /// <summary>确定性岗位分配。</summary>
        public ReadOnlyCollection<D21CrewRoleAssignment> Assignments { get; set; }

        /// <summary>复制评估结果。</summary>
        public D21CrewRequirementDecision Clone()
        {
            var assignments = new List<D21CrewRoleAssignment>();
            if (Assignments != null)
            {
                for (var index = 0; index < Assignments.Count; index++)
                {
                    if (Assignments[index] != null)
                    {
                        assignments.Add(Assignments[index].Clone());
                    }
                }
            }
            return new D21CrewRequirementDecision
            {
                Valid = Valid,
                Eligibility = Eligibility,
                CanDepart = CanDepart,
                ActiveCrewCount = ActiveCrewCount,
                AverageMorale = AverageMorale,
                AverageFatigue = AverageFatigue,
                AverageHealth = AverageHealth,
                RequiredCrewCount = RequiredCrewCount,
                NavigationEfficiencyMultiplier = NavigationEfficiencyMultiplier,
                ReasonCode = ReasonCode,
                MissingRoles = new ReadOnlyCollection<D21CrewRole>(MissingRoles == null ? new List<D21CrewRole>() : new List<D21CrewRole>(MissingRoles)),
                ReasonCodes = new ReadOnlyCollection<string>(ReasonCodes == null ? new List<string>() : new List<string>(ReasonCodes)),
                Assignments = new ReadOnlyCollection<D21CrewRoleAssignment>(assignments),
            };
        }
    }

    /// <summary>船员最低配置和出航资格的只读规则。</summary>
    public static class D21CrewRequirementRules
    {
        /// <summary>当前 D21-L06 船员规则版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>船员规则的 owner 名称。</summary>
        public const string OwnerSystem = "D21CrewRequirementRules";

        /// <summary>校验船员配置档。</summary>
        public static D21CrewRequirementFailureCode ValidateProfile(D21CrewRequirementProfile profile, out ReadOnlyCollection<string> errors)
        {
            var list = new List<string>();
            if (profile == null)
            {
                list.Add("crew_profile_missing");
                errors = new ReadOnlyCollection<string>(list);
                return D21CrewRequirementFailureCode.InvalidProfile;
            }
            if (profile.SchemaVersion != CurrentSchemaVersion || profile.MinimumCrew <= 0 || profile.StandardCrew < profile.MinimumCrew || profile.MaximumCrew < profile.StandardCrew)
            {
                list.Add("crew_profile_count_range_invalid");
            }
            if (!IsScore(profile.MinimumAverageMorale) || !IsScore(profile.MaximumAverageFatigue) || !IsScore(profile.MinimumAverageHealth))
            {
                list.Add("crew_profile_threshold_invalid");
            }
            if (profile.RequiredRoles == null)
            {
                list.Add("crew_profile_roles_missing");
            }
            else
            {
                var roles = new HashSet<D21CrewRole>();
                for (var index = 0; index < profile.RequiredRoles.Count; index++)
                {
                    var requirement = profile.RequiredRoles[index];
                    if (requirement == null || !IsSupportedRole(requirement.Role) || requirement.MinimumCount <= 0 || !roles.Add(requirement.Role))
                    {
                        list.Add("crew_profile_role_invalid");
                    }
                }
            }
            errors = new ReadOnlyCollection<string>(list);
            return list.Count == 0 ? D21CrewRequirementFailureCode.None : D21CrewRequirementFailureCode.InvalidProfile;
        }

        /// <summary>校验船员只读投影。</summary>
        public static D21CrewRequirementFailureCode ValidateRoster(D21CrewRosterSnapshot roster, out ReadOnlyCollection<string> errors)
        {
            var list = new List<string>();
            if (roster == null)
            {
                list.Add("crew_roster_missing");
                errors = new ReadOnlyCollection<string>(list);
                return D21CrewRequirementFailureCode.InvalidRoster;
            }
            if (roster.SchemaVersion != CurrentSchemaVersion || !D21ShipIdentityContracts.IsStableId(roster.ShipId, 96) || roster.SaveRevision < 0)
            {
                list.Add("crew_roster_metadata_invalid");
            }
            if (roster.CrewMembers == null)
            {
                list.Add("crew_roster_members_missing");
            }
            else
            {
                var ids = new HashSet<string>(StringComparer.Ordinal);
                for (var index = 0; index < roster.CrewMembers.Count; index++)
                {
                    var member = roster.CrewMembers[index];
                    if (!ValidateCrewMember(member) || !ids.Add(member.CrewId))
                    {
                        list.Add("crew_roster_member_invalid");
                    }
                }
            }
            errors = new ReadOnlyCollection<string>(list);
            return list.Count == 0 ? D21CrewRequirementFailureCode.None : D21CrewRequirementFailureCode.InvalidRoster;
        }

        /// <summary>
        /// 校验历史调用方提交的已计算结果。该入口只用于保持 D21-L27
        /// 离线回放兼容；D20 公共投影和直接 roster 路径仍必须调用 Evaluate。
        /// </summary>
        public static D21CrewRequirementFailureCode ValidateDecision(
            D21CrewRequirementDecision decision,
            out ReadOnlyCollection<string> errors)
        {
            var list = new List<string>();
            if (decision == null)
            {
                list.Add("crew_decision_missing");
                errors = new ReadOnlyCollection<string>(list);
                return D21CrewRequirementFailureCode.InvalidRequest;
            }
            if (!decision.Valid)
                list.Add("crew_decision_not_valid");
            if (decision.Eligibility == D21CrewEligibilityState.Unknown ||
                !Enum.IsDefined(typeof(D21CrewEligibilityState), decision.Eligibility))
                list.Add("crew_decision_eligibility_invalid");
            if (decision.ActiveCrewCount < 0 || decision.RequiredCrewCount <= 0 ||
                decision.ActiveCrewCount < decision.RequiredCrewCount)
                list.Add("crew_decision_count_invalid");
            if (!IsScore(decision.AverageMorale) || !IsScore(decision.AverageFatigue) ||
                !IsScore(decision.AverageHealth) ||
                double.IsNaN(decision.NavigationEfficiencyMultiplier) ||
                double.IsInfinity(decision.NavigationEfficiencyMultiplier) ||
                decision.NavigationEfficiencyMultiplier < 0.25d ||
                decision.NavigationEfficiencyMultiplier > 1d)
                list.Add("crew_decision_metric_invalid");

            var assignedRoles = new HashSet<D21CrewRole>();
            var assignedCrew = new HashSet<string>(StringComparer.Ordinal);
            if (decision.Assignments == null)
            {
                list.Add("crew_decision_assignments_missing");
            }
            else
            {
                foreach (D21CrewRoleAssignment assignment in decision.Assignments)
                {
                    if (assignment == null || !IsSupportedRole(assignment.Role) ||
                        !D21ShipIdentityContracts.IsStableId(assignment.CrewId, 96) ||
                        !assignedRoles.Add(assignment.Role) ||
                        !assignedCrew.Add(assignment.CrewId))
                        list.Add("crew_decision_assignment_invalid");
                }
            }

            var missingRoles = new HashSet<D21CrewRole>();
            if (decision.MissingRoles == null)
            {
                list.Add("crew_decision_missing_roles_missing");
            }
            else
            {
                foreach (D21CrewRole role in decision.MissingRoles)
                {
                    if (!IsSupportedRole(role) || !missingRoles.Add(role))
                        list.Add("crew_decision_missing_role_invalid");
                }
            }

            if (decision.ReasonCodes == null)
                list.Add("crew_decision_reason_codes_missing");
            errors = new ReadOnlyCollection<string>(list);
            return list.Count == 0 ? D21CrewRequirementFailureCode.None :
                D21CrewRequirementFailureCode.InvalidRequest;
        }

        /// <summary>以公开只读投影评估最低船员配置。</summary>
        public static D21CrewRequirementDecision Evaluate(D21CrewRequirementProfile profile, D21CrewRosterSnapshot roster)
        {
            var decision = new D21CrewRequirementDecision();
            ReadOnlyCollection<string> profileErrors;
            var profileCode = ValidateProfile(profile, out profileErrors);
            if (profileCode != D21CrewRequirementFailureCode.None)
            {
                return Failure(decision, profileCode, profileErrors);
            }
            ReadOnlyCollection<string> rosterErrors;
            var rosterCode = ValidateRoster(roster, out rosterErrors);
            if (rosterCode != D21CrewRequirementFailureCode.None)
            {
                return Failure(decision, rosterCode, rosterErrors);
            }

            var active = new List<D21CrewMemberReadSnapshot>();
            for (var index = 0; index < roster.CrewMembers.Count; index++)
            {
                var member = roster.CrewMembers[index];
                if (member.IsAvailable && !member.IsIncapacitated && member.HealthScore > 0d)
                {
                    active.Add(member.Clone());
                }
            }
            active.Sort(CompareMembers);
            decision.Valid = true;
            decision.RequiredCrewCount = profile.MinimumCrew;
            decision.ActiveCrewCount = active.Count;
            decision.AverageMorale = Round(Average(active, ScoreKind.Morale));
            decision.AverageFatigue = Round(Average(active, ScoreKind.Fatigue));
            decision.AverageHealth = Round(Average(active, ScoreKind.Health));
            decision.NavigationEfficiencyMultiplier = ComputeEfficiency(decision.AverageMorale, decision.AverageFatigue, decision.AverageHealth);

            if (active.Count < profile.MinimumCrew)
            {
                decision.Eligibility = D21CrewEligibilityState.Refused;
                decision.CanDepart = false;
                decision.ReasonCode = D21CrewRequirementFailureCode.InsufficientCrew;
                decision.ReasonCodes = new ReadOnlyCollection<string>(new[] { "crew_minimum_not_met" });
                return decision;
            }

            var assignments = new List<D21CrewRoleAssignment>();
            var missingRoles = FindMissingRoles(profile, active, assignments);
            decision.MissingRoles = new ReadOnlyCollection<D21CrewRole>(missingRoles);
            decision.Assignments = new ReadOnlyCollection<D21CrewRoleAssignment>(assignments);
            if (missingRoles.Count > 0)
            {
                decision.Eligibility = D21CrewEligibilityState.Refused;
                decision.CanDepart = false;
                decision.ReasonCode = D21CrewRequirementFailureCode.MissingRequiredRole;
                decision.ReasonCodes = new ReadOnlyCollection<string>(new[] { "crew_required_role_missing" });
                return decision;
            }

            var reasons = new List<string>();
            if (decision.AverageMorale < profile.MinimumAverageMorale)
            {
                reasons.Add("crew_morale_below_threshold");
            }
            if (decision.AverageFatigue > profile.MaximumAverageFatigue)
            {
                reasons.Add("crew_fatigue_above_threshold");
            }
            if (decision.AverageHealth < profile.MinimumAverageHealth)
            {
                reasons.Add("crew_health_below_threshold");
            }
            if (active.Count < profile.StandardCrew)
            {
                reasons.Add("crew_below_standard_complement");
            }
            decision.ReasonCodes = new ReadOnlyCollection<string>(reasons);
            decision.CanDepart = decision.AverageMorale >= profile.MinimumAverageMorale && decision.AverageFatigue <= profile.MaximumAverageFatigue && decision.AverageHealth >= profile.MinimumAverageHealth;
            if (decision.CanDepart && active.Count >= profile.StandardCrew)
            {
                decision.Eligibility = D21CrewEligibilityState.Eligible;
                decision.ReasonCode = D21CrewRequirementFailureCode.None;
                return decision;
            }

            decision.Eligibility = IsSeverelyUnfit(decision, profile) ? D21CrewEligibilityState.Refused : D21CrewEligibilityState.Restricted;
            if (!decision.CanDepart)
            {
                decision.ReasonCode = decision.AverageMorale < profile.MinimumAverageMorale ? D21CrewRequirementFailureCode.MoraleTooLow : decision.AverageFatigue > profile.MaximumAverageFatigue ? D21CrewRequirementFailureCode.FatigueTooHigh : D21CrewRequirementFailureCode.HealthTooLow;
                decision.CanDepart = false;
            }
            else
            {
                decision.ReasonCode = D21CrewRequirementFailureCode.None;
            }
            return decision;
        }

        private sealed class RoleSlot
        {
            public D21CrewRole Role;
            public List<int> CandidateIndexes = new List<int>();
            public int Occurrence;
        }

        private enum ScoreKind
        {
            Morale,
            Fatigue,
            Health,
        }

        private static D21CrewRequirementDecision Failure(D21CrewRequirementDecision decision, D21CrewRequirementFailureCode code, IEnumerable<string> errors)
        {
            decision.Valid = false;
            decision.Eligibility = D21CrewEligibilityState.Unknown;
            decision.CanDepart = false;
            decision.ReasonCode = code;
            decision.ReasonCodes = new ReadOnlyCollection<string>(new List<string>(errors ?? new string[0]));
            return decision;
        }

        private static List<D21CrewRole> FindMissingRoles(D21CrewRequirementProfile profile, List<D21CrewMemberReadSnapshot> active, List<D21CrewRoleAssignment> assignments)
        {
            var slots = new List<RoleSlot>();
            for (var index = 0; index < profile.RequiredRoles.Count; index++)
            {
                var requirement = profile.RequiredRoles[index];
                for (var occurrence = 0; occurrence < requirement.MinimumCount; occurrence++)
                {
                    var slot = new RoleSlot { Role = requirement.Role, Occurrence = occurrence };
                    for (var memberIndex = 0; memberIndex < active.Count; memberIndex++)
                    {
                        if (active[memberIndex].QualifiedRoles.Contains(requirement.Role))
                        {
                            slot.CandidateIndexes.Add(memberIndex);
                        }
                    }
                    slots.Add(slot);
                }
            }
            slots.Sort(CompareSlots);
            var used = new bool[active.Count];
            var selected = new List<D21CrewRoleAssignment>();
            if (AssignSlots(slots, 0, active, used, selected))
            {
                selected.Sort(CompareAssignments);
                assignments.AddRange(selected);
                return new List<D21CrewRole>();
            }

            var missing = new List<D21CrewRole>();
            for (var index = 0; index < profile.RequiredRoles.Count; index++)
            {
                var requirement = profile.RequiredRoles[index];
                var count = 0;
                for (var memberIndex = 0; memberIndex < active.Count; memberIndex++)
                {
                    if (active[memberIndex].QualifiedRoles.Contains(requirement.Role))
                    {
                        count++;
                    }
                }
                if (count < requirement.MinimumCount)
                {
                    missing.Add(requirement.Role);
                }
            }
            if (missing.Count == 0)
            {
                for (var index = 0; index < profile.RequiredRoles.Count; index++)
                {
                    missing.Add(profile.RequiredRoles[index].Role);
                }
            }
            return missing;
        }

        private static bool AssignSlots(List<RoleSlot> slots, int slotIndex, List<D21CrewMemberReadSnapshot> active, bool[] used, List<D21CrewRoleAssignment> selected)
        {
            if (slotIndex >= slots.Count)
            {
                return true;
            }
            var slot = slots[slotIndex];
            for (var index = 0; index < slot.CandidateIndexes.Count; index++)
            {
                var memberIndex = slot.CandidateIndexes[index];
                if (used[memberIndex])
                {
                    continue;
                }
                used[memberIndex] = true;
                selected.Add(new D21CrewRoleAssignment { Role = slot.Role, CrewId = active[memberIndex].CrewId });
                if (AssignSlots(slots, slotIndex + 1, active, used, selected))
                {
                    return true;
                }
                selected.RemoveAt(selected.Count - 1);
                used[memberIndex] = false;
            }
            return false;
        }

        private static int CompareMembers(D21CrewMemberReadSnapshot left, D21CrewMemberReadSnapshot right)
        {
            return string.Compare(left.CrewId, right.CrewId, StringComparison.Ordinal);
        }

        private static int CompareSlots(RoleSlot left, RoleSlot right)
        {
            var result = left.CandidateIndexes.Count.CompareTo(right.CandidateIndexes.Count);
            return result != 0 ? result : left.Role.CompareTo(right.Role);
        }

        private static int CompareAssignments(D21CrewRoleAssignment left, D21CrewRoleAssignment right)
        {
            var result = left.Role.CompareTo(right.Role);
            return result != 0 ? result : string.Compare(left.CrewId, right.CrewId, StringComparison.Ordinal);
        }

        private static double Average(List<D21CrewMemberReadSnapshot> members, ScoreKind kind)
        {
            if (members.Count == 0)
            {
                return 0d;
            }
            var total = 0d;
            for (var index = 0; index < members.Count; index++)
            {
                total += kind == ScoreKind.Morale ? members[index].MoraleScore : kind == ScoreKind.Fatigue ? members[index].FatigueScore : members[index].HealthScore;
            }
            return total / members.Count;
        }

        private static double ComputeEfficiency(double morale, double fatigue, double health)
        {
            var moraleFactor = Math.Max(0d, Math.Min(1d, morale / 100d));
            var fatigueFactor = Math.Max(0d, Math.Min(1d, 1d - fatigue / 100d));
            var healthFactor = Math.Max(0d, Math.Min(1d, health / 100d));
            var result = 0.25d + moraleFactor * 0.45d + fatigueFactor * 0.2d + healthFactor * 0.1d;
            return Round(Math.Max(0.25d, Math.Min(1d, result)));
        }

        private static bool IsSeverelyUnfit(D21CrewRequirementDecision decision, D21CrewRequirementProfile profile)
        {
            return decision.AverageMorale < Math.Min(profile.MinimumAverageMorale, 20d) || decision.AverageFatigue > Math.Max(profile.MaximumAverageFatigue, 95d) || decision.AverageHealth < Math.Min(profile.MinimumAverageHealth, 25d);
        }

        private static bool ValidateCrewMember(D21CrewMemberReadSnapshot member)
        {
            if (member == null || !D21ShipIdentityContracts.IsStableId(member.CrewId, 96) || !IsScore(member.MoraleScore) || !IsScore(member.FatigueScore) || !IsScore(member.HealthScore) || member.QualifiedRoles == null)
            {
                return false;
            }
            var roles = new HashSet<D21CrewRole>();
            for (var index = 0; index < member.QualifiedRoles.Count; index++)
            {
                if (!IsSupportedRole(member.QualifiedRoles[index]) || !roles.Add(member.QualifiedRoles[index]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsSupportedRole(D21CrewRole role)
        {
            return role >= D21CrewRole.Helmsman && role <= D21CrewRole.Deckhand;
        }

        private static bool IsScore(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d && value <= 100d;
        }

        private static double Round(double value)
        {
            return Math.Round(value, 4, MidpointRounding.AwayFromZero);
        }
    }
}
