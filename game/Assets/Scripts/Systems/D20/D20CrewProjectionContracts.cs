// ============================================================
// D20CrewProjectionContracts —— D20 船员公开只读投影（纯 C#）
//
// 船员身份、岗位资格、可用性和状态数值仍由 D10/Crew owner 写入。本文件
// 只定义 owner 可以发布给 D20 的最小快照，并通过防御性拷贝形成稳定的
// 公共投影；它不持有 CrewAgent 引用，也不提供任何 owner 写入入口。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D20
{
    /// <summary>D20 船员公开投影校验的稳定结果码。</summary>
    public enum D20CrewProjectionFailureCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>投影或 owner 输入缺失。</summary>
        MissingProjection = 1,
        /// <summary>投影版本不受支持。</summary>
        SchemaVersionMismatch = 2,
        /// <summary>船只、来源或修订元数据无效。</summary>
        MetadataInvalid = 3,
        /// <summary>成员集合缺失。</summary>
        MembersMissing = 4,
        /// <summary>成员字段或数值无效。</summary>
        MemberInvalid = 5,
        /// <summary>成员 ID 重复。</summary>
        DuplicateCrewId = 6,
        /// <summary>同一成员的岗位 ID 重复。</summary>
        DuplicateRoleId = 7,
    }

    /// <summary>
    /// D10/Crew owner 发布给 D20 的只读成员输入。该载体不提供任何写回
    /// owner 的方法；D20 适配器只会读取并复制这些字段。
    /// </summary>
    [Serializable]
    public sealed class D20CrewMemberOwnerReadSnapshot
    {
        /// <summary>稳定船员 ID。</summary>
        public string CrewId = string.Empty;

        /// <summary>owner 已确认的岗位字符串；未知岗位由下游适配器拒绝。</summary>
        public List<string> QualifiedRoleIds = new List<string>();

        /// <summary>当前是否可以被安排值勤。</summary>
        public bool IsAvailable;

        /// <summary>当前是否失去执行岗位的能力。</summary>
        public bool IsIncapacitated;

        /// <summary>owner 发布的士气，范围 0 到 100。</summary>
        public double MoraleScore;

        /// <summary>owner 发布的疲劳，范围 0 到 100。</summary>
        public double FatigueScore;

        /// <summary>owner 发布的健康，范围 0 到 100。</summary>
        public double HealthScore;

        /// <summary>返回不改变输入且按岗位字符串排序的副本。</summary>
        public D20CrewMemberOwnerReadSnapshot Clone()
        {
            var result = new D20CrewMemberOwnerReadSnapshot
            {
                CrewId = CrewId ?? string.Empty,
                QualifiedRoleIds = QualifiedRoleIds == null
                    ? null : new List<string>(QualifiedRoleIds),
                IsAvailable = IsAvailable,
                IsIncapacitated = IsIncapacitated,
                MoraleScore = MoraleScore,
                FatigueScore = FatigueScore,
                HealthScore = HealthScore,
            };
            if (result.QualifiedRoleIds != null)
                result.QualifiedRoleIds.Sort(D20CrewProjectionContracts.CompareStableReferences);
            return result;
        }
    }

    /// <summary>
    /// D10/Crew owner 的公开只读名单输入。它是适配边界的 read model，D20
    /// 不会把该对象注册成第二个船员事实 owner。
    /// </summary>
    [Serializable]
    public sealed class D20CrewOwnerRosterReadSnapshot
    {
        /// <summary>D20 owner-read 输入版本。</summary>
        public int SchemaVersion = D20CrewProjectionContracts.CurrentSchemaVersion;

        /// <summary>该名单所属船只的稳定 ID。</summary>
        public string ShipId = string.Empty;

        /// <summary>生成名单时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>owner 生成的稳定公开投影 ID。</summary>
        public string SourceProjectionId = string.Empty;

        /// <summary>真实事实 owner 的稳定系统标识。</summary>
        public string SourceOwnerSystem = string.Empty;

        /// <summary>船员成员输入；null 表示 owner 没有发布名单，不能猜成空名单。</summary>
        public List<D20CrewMemberOwnerReadSnapshot> CrewMembers =
            new List<D20CrewMemberOwnerReadSnapshot>();

        /// <summary>返回防御性且按稳定 crew_id 排序的副本。</summary>
        public D20CrewOwnerRosterReadSnapshot Clone()
        {
            var result = new D20CrewOwnerRosterReadSnapshot
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId ?? string.Empty,
                SaveRevision = SaveRevision,
                SourceProjectionId = SourceProjectionId ?? string.Empty,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
                CrewMembers = CrewMembers == null
                    ? null : new List<D20CrewMemberOwnerReadSnapshot>(),
            };
            if (CrewMembers != null)
            {
                foreach (D20CrewMemberOwnerReadSnapshot member in CrewMembers)
                    result.CrewMembers.Add(member == null ? null : member.Clone());
                result.CrewMembers.Sort(D20CrewProjectionContracts.CompareOwnerMembers);
            }
            return result;
        }
    }

    /// <summary>D20 发布给 D21/D22/D23 的最小船员成员只读投影。</summary>
    [Serializable]
    public sealed class D20CrewMemberProjection
    {
        /// <summary>稳定船员 ID。</summary>
        public string CrewId = string.Empty;

        /// <summary>owner 确认的岗位字符串，不在 D20 内猜测岗位语义。</summary>
        public List<string> QualifiedRoleIds = new List<string>();

        /// <summary>当前是否可被排入值勤。</summary>
        public bool IsAvailable;

        /// <summary>当前是否失能。</summary>
        public bool IsIncapacitated;

        /// <summary>只读士气值，范围 0 到 100。</summary>
        public double MoraleScore;

        /// <summary>只读疲劳值，范围 0 到 100。</summary>
        public double FatigueScore;

        /// <summary>只读健康值，范围 0 到 100。</summary>
        public double HealthScore;

        /// <summary>返回不改变投影且按岗位字符串排序的副本。</summary>
        public D20CrewMemberProjection Clone()
        {
            var result = new D20CrewMemberProjection
            {
                CrewId = CrewId ?? string.Empty,
                QualifiedRoleIds = QualifiedRoleIds == null
                    ? null : new List<string>(QualifiedRoleIds),
                IsAvailable = IsAvailable,
                IsIncapacitated = IsIncapacitated,
                MoraleScore = MoraleScore,
                FatigueScore = FatigueScore,
                HealthScore = HealthScore,
            };
            if (result.QualifiedRoleIds != null)
                result.QualifiedRoleIds.Sort(D20CrewProjectionContracts.CompareStableReferences);
            return result;
        }
    }

    /// <summary>D20 对下游公开的完整船员名单只读投影。</summary>
    [Serializable]
    public sealed class D20CrewRosterProjection
    {
        /// <summary>D20 船员投影契约版本。</summary>
        public int SchemaVersion = D20CrewProjectionContracts.CurrentSchemaVersion;

        /// <summary>该名单所属船只的稳定 ID。</summary>
        public string ShipId = string.Empty;

        /// <summary>生成该投影时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>来源 owner 生成的稳定投影 ID。</summary>
        public string SourceProjectionId = string.Empty;

        /// <summary>产生事实的真实 owner 系统。</summary>
        public string SourceOwnerSystem = string.Empty;

        /// <summary>按稳定 crew_id 排序的船员成员。</summary>
        public List<D20CrewMemberProjection> CrewMembers =
            new List<D20CrewMemberProjection>();

        /// <summary>返回防御性且确定性排序的深拷贝。</summary>
        public D20CrewRosterProjection Clone()
        {
            var result = new D20CrewRosterProjection
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId ?? string.Empty,
                SaveRevision = SaveRevision,
                SourceProjectionId = SourceProjectionId ?? string.Empty,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
                CrewMembers = CrewMembers == null
                    ? null : new List<D20CrewMemberProjection>(),
            };
            if (CrewMembers != null)
            {
                foreach (D20CrewMemberProjection member in CrewMembers)
                    result.CrewMembers.Add(member == null ? null : member.Clone());
                result.CrewMembers.Sort(D20CrewProjectionContracts.CompareProjectionMembers);
            }
            return result;
        }

        /// <summary>验证公开投影；失败抛出稳定的逻辑异常。</summary>
        public void Validate()
        {
            ReadOnlyCollection<string> errors;
            if (D20CrewProjectionContracts.Validate(this, out errors) !=
                D20CrewProjectionFailureCode.None)
            {
                throw new InvalidOperationException(string.Join(";", errors));
            }
        }

        /// <summary>生成不依赖运行时哈希的确定性指纹。</summary>
        public string Fingerprint()
        {
            var sorted = Clone();
            var values = new List<string>
            {
                SchemaVersion.ToString(CultureInfo.InvariantCulture),
                ShipId ?? string.Empty,
                SaveRevision.ToString(CultureInfo.InvariantCulture),
                SourceProjectionId ?? string.Empty,
                SourceOwnerSystem ?? string.Empty,
            };
            if (sorted.CrewMembers != null)
            {
                foreach (D20CrewMemberProjection member in sorted.CrewMembers)
                {
                    if (member == null)
                    {
                        values.Add("<null>");
                        continue;
                    }
                    values.Add(member.CrewId ?? string.Empty);
                    values.Add(member.IsAvailable ? "1" : "0");
                    values.Add(member.IsIncapacitated ? "1" : "0");
                    values.Add(member.MoraleScore.ToString("R", CultureInfo.InvariantCulture));
                    values.Add(member.FatigueScore.ToString("R", CultureInfo.InvariantCulture));
                    values.Add(member.HealthScore.ToString("R", CultureInfo.InvariantCulture));
                    if (member.QualifiedRoleIds != null)
                        values.AddRange(member.QualifiedRoleIds);
                    values.Add(";");
                }
            }
            return string.Join("|", values);
        }
    }

    /// <summary>D20 船员投影校验结果。</summary>
    [Serializable]
    public sealed class D20CrewProjectionValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;

        /// <summary>稳定校验结果码。</summary>
        public D20CrewProjectionFailureCode Code;

        /// <summary>机器可读诊断码集合。</summary>
        public ReadOnlyCollection<string> Errors =
            new ReadOnlyCollection<string>(new List<string>());

        /// <summary>复制校验结果。</summary>
        public D20CrewProjectionValidationResult Clone()
        {
            return new D20CrewProjectionValidationResult
            {
                Valid = Valid,
                Code = Code,
                Errors = new ReadOnlyCollection<string>(
                    Errors == null ? new List<string>() : new List<string>(Errors)),
            };
        }
    }

    /// <summary>D20 owner-read 到公开船员投影的适配结果。</summary>
    [Serializable]
    public sealed class D20CrewProjectionResult
    {
        /// <summary>是否成功生成公开投影。</summary>
        public bool Valid;

        /// <summary>稳定生成结果码。</summary>
        public D20CrewProjectionFailureCode Code;

        /// <summary>机器可读诊断码集合。</summary>
        public ReadOnlyCollection<string> Errors =
            new ReadOnlyCollection<string>(new List<string>());

        /// <summary>成功时的深拷贝公开投影。</summary>
        public D20CrewRosterProjection Projection;

        /// <summary>成功时保留的真实 owner 标识。</summary>
        public string SourceOwnerSystem = string.Empty;

        /// <summary>复制适配结果。</summary>
        public D20CrewProjectionResult Clone()
        {
            return new D20CrewProjectionResult
            {
                Valid = Valid,
                Code = Code,
                Errors = new ReadOnlyCollection<string>(
                    Errors == null ? new List<string>() : new List<string>(Errors)),
                Projection = Projection == null ? null : Projection.Clone(),
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
            };
        }
    }

    /// <summary>D20 船员公开投影的 schema、ID 和数值边界。</summary>
    public static class D20CrewProjectionContracts
    {
        /// <summary>当前 D20 船员公开投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>D20 适配器的 owner 标识；不是船员事实 owner。</summary>
        public const string OwnerSystem = "D20CrewProjectionAdapter";

        /// <summary>公开投影中稳定 ID 的最大长度。</summary>
        public const int StableIdMaxLength = 128;

        /// <summary>来源引用的最大长度。</summary>
        public const int ReferenceMaxLength = 160;

        /// <summary>岗位字符串的最大长度。</summary>
        public const int RoleIdMaxLength = 64;

        /// <summary>校验公开投影，永不修改输入集合。</summary>
        public static D20CrewProjectionFailureCode Validate(
            D20CrewRosterProjection projection,
            out ReadOnlyCollection<string> errors)
        {
            var list = new List<string>();
            if (projection == null)
            {
                list.Add("crew_projection_missing");
                errors = ReadOnly(list);
                return D20CrewProjectionFailureCode.MissingProjection;
            }

            ValidateMetadata(projection.SchemaVersion, projection.ShipId,
                projection.SaveRevision, projection.SourceProjectionId,
                projection.SourceOwnerSystem, list);
            ValidateMembers(projection.CrewMembers, list, false);
            errors = ReadOnly(list);
            return FailureCode(list);
        }

        /// <summary>校验 owner 输入，适配器必须在复制前完成此检查。</summary>
        public static D20CrewProjectionFailureCode ValidateOwnerSnapshot(
            D20CrewOwnerRosterReadSnapshot ownerSnapshot,
            out ReadOnlyCollection<string> errors)
        {
            var list = new List<string>();
            if (ownerSnapshot == null)
            {
                list.Add("crew_owner_snapshot_missing");
                errors = ReadOnly(list);
                return D20CrewProjectionFailureCode.MissingProjection;
            }

            ValidateMetadata(ownerSnapshot.SchemaVersion, ownerSnapshot.ShipId,
                ownerSnapshot.SaveRevision, ownerSnapshot.SourceProjectionId,
                ownerSnapshot.SourceOwnerSystem, list);
            ValidateMembers(ownerSnapshot.CrewMembers, list, true);
            errors = ReadOnly(list);
            return FailureCode(list);
        }

        /// <summary>判断稳定 ID 是否只含可跨存档使用的 ASCII 字符。</summary>
        public static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength ||
                !string.Equals(value.Trim(), value, StringComparison.Ordinal))
                return false;
            for (var index = 0; index < value.Length; index++)
            {
                char current = value[index];
                bool letter = (current >= 'a' && current <= 'z') ||
                    (current >= 'A' && current <= 'Z');
                bool digit = current >= '0' && current <= '9';
                bool separator = current == ':' || current == '_' ||
                    current == '-' || current == '.';
                if (!letter && !digit && !separator)
                    return false;
            }
            return true;
        }

        /// <summary>判断公开状态数值是否有限且在 0 到 100 内。</summary>
        public static bool IsScore(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) &&
                value >= 0d && value <= 100d;
        }

        /// <summary>按稳定字符串排序，大小写差异仍保持确定性。</summary>
        public static int CompareStableReferences(string left, string right)
        {
            var result = StringComparer.OrdinalIgnoreCase.Compare(left, right);
            return result != 0 ? result : StringComparer.Ordinal.Compare(left, right);
        }

        internal static int CompareOwnerMembers(
            D20CrewMemberOwnerReadSnapshot left,
            D20CrewMemberOwnerReadSnapshot right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            return CompareStableReferences(left.CrewId, right.CrewId);
        }

        internal static int CompareProjectionMembers(
            D20CrewMemberProjection left,
            D20CrewMemberProjection right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            return CompareStableReferences(left.CrewId, right.CrewId);
        }

        private static void ValidateMetadata(int schemaVersion, string shipId,
            long saveRevision, string sourceProjectionId, string sourceOwnerSystem,
            List<string> errors)
        {
            if (schemaVersion != CurrentSchemaVersion)
                errors.Add("crew_projection_schema_version_invalid");
            if (!IsStableReference(shipId, StableIdMaxLength))
                errors.Add("crew_projection_ship_id_invalid");
            if (saveRevision < 0)
                errors.Add("crew_projection_save_revision_invalid");
            if (!IsStableReference(sourceProjectionId, ReferenceMaxLength))
                errors.Add("crew_projection_source_projection_id_invalid");
            if (!IsStableReference(sourceOwnerSystem, ReferenceMaxLength))
                errors.Add("crew_projection_source_owner_invalid");
        }

        private static void ValidateMembers<T>(List<T> members, List<string> errors,
            bool ownerInput)
        {
            if (members == null)
            {
                errors.Add("crew_projection_members_missing");
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (T value in members)
            {
                string crewId;
                List<string> roles;
                double morale;
                double fatigue;
                double health;
                if (ownerInput)
                {
                    var member = value as D20CrewMemberOwnerReadSnapshot;
                    crewId = member == null ? string.Empty : member.CrewId;
                    roles = member == null ? null : member.QualifiedRoleIds;
                    morale = member == null ? double.NaN : member.MoraleScore;
                    fatigue = member == null ? double.NaN : member.FatigueScore;
                    health = member == null ? double.NaN : member.HealthScore;
                }
                else
                {
                    var member = value as D20CrewMemberProjection;
                    crewId = member == null ? string.Empty : member.CrewId;
                    roles = member == null ? null : member.QualifiedRoleIds;
                    morale = member == null ? double.NaN : member.MoraleScore;
                    fatigue = member == null ? double.NaN : member.FatigueScore;
                    health = member == null ? double.NaN : member.HealthScore;
                }

                if (!IsStableReference(crewId, StableIdMaxLength))
                    errors.Add("crew_projection_member_id_invalid");
                else if (!ids.Add(crewId))
                    errors.Add("crew_projection_member_id_duplicate:" + crewId);

                if (roles == null)
                {
                    errors.Add("crew_projection_member_roles_missing");
                }
                else
                {
                    var roleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string roleId in roles)
                    {
                        if (!IsStableReference(roleId, RoleIdMaxLength))
                            errors.Add("crew_projection_member_role_invalid");
                        else if (!roleIds.Add(roleId))
                            errors.Add("crew_projection_member_role_duplicate:" + roleId);
                    }
                }

                if (!IsScore(morale) || !IsScore(fatigue) || !IsScore(health))
                    errors.Add("crew_projection_member_score_invalid");
            }
        }

        private static D20CrewProjectionFailureCode FailureCode(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
                return D20CrewProjectionFailureCode.None;
            foreach (string error in errors)
            {
                if (error != null && error.StartsWith("crew_projection_schema_version_invalid", StringComparison.Ordinal))
                    return D20CrewProjectionFailureCode.SchemaVersionMismatch;
            }
            foreach (string error in errors)
            {
                if (error != null && error.StartsWith("crew_projection_member_id_duplicate", StringComparison.Ordinal))
                    return D20CrewProjectionFailureCode.DuplicateCrewId;
                if (error != null && error.StartsWith("crew_projection_member_role_duplicate", StringComparison.Ordinal))
                    return D20CrewProjectionFailureCode.DuplicateRoleId;
            }
            foreach (string error in errors)
            {
                if (error == "crew_projection_members_missing")
                    return D20CrewProjectionFailureCode.MembersMissing;
                if (error != null && error.StartsWith("crew_projection_", StringComparison.Ordinal))
                    return error.Contains("member_")
                        ? D20CrewProjectionFailureCode.MemberInvalid
                        : D20CrewProjectionFailureCode.MetadataInvalid;
            }
            return D20CrewProjectionFailureCode.MemberInvalid;
        }

        private static ReadOnlyCollection<string> ReadOnly(List<string> errors)
        {
            return new ReadOnlyCollection<string>(errors ?? new List<string>());
        }
    }

    /// <summary>
    /// D20 船员只读投影适配器。输入必须是 owner 已发布的 read model，输出
    /// 只包含副本；任何缺失或非法字段都 fail-closed，不填充猜测值。
    /// </summary>
    public static class D20CrewProjectionAdapter
    {
        /// <summary>适配器 owner 标识，事实 owner 仍是输入声明的系统。</summary>
        public const string OwnerSystem = D20CrewProjectionContracts.OwnerSystem;

        /// <summary>将 owner 公开输入复制为可审计的 D20 投影。</summary>
        public static D20CrewProjectionResult Project(
            D20CrewOwnerRosterReadSnapshot ownerSnapshot)
        {
            ReadOnlyCollection<string> ownerErrors;
            var ownerCode = D20CrewProjectionContracts.ValidateOwnerSnapshot(
                ownerSnapshot, out ownerErrors);
            if (ownerCode != D20CrewProjectionFailureCode.None)
            {
                return Failure(ownerCode, ownerErrors);
            }

            var projection = new D20CrewRosterProjection
            {
                SchemaVersion = D20CrewProjectionContracts.CurrentSchemaVersion,
                ShipId = ownerSnapshot.ShipId,
                SaveRevision = ownerSnapshot.SaveRevision,
                SourceProjectionId = ownerSnapshot.SourceProjectionId,
                SourceOwnerSystem = ownerSnapshot.SourceOwnerSystem,
                CrewMembers = new List<D20CrewMemberProjection>(),
            };
            foreach (D20CrewMemberOwnerReadSnapshot member in ownerSnapshot.CrewMembers)
            {
                projection.CrewMembers.Add(member == null ? null : new D20CrewMemberProjection
                {
                    CrewId = member.CrewId,
                    QualifiedRoleIds = member.QualifiedRoleIds == null
                        ? null : new List<string>(member.QualifiedRoleIds),
                    IsAvailable = member.IsAvailable,
                    IsIncapacitated = member.IsIncapacitated,
                    MoraleScore = member.MoraleScore,
                    FatigueScore = member.FatigueScore,
                    HealthScore = member.HealthScore,
                });
            }

            projection = projection.Clone();
            ReadOnlyCollection<string> projectionErrors;
            var projectionCode = D20CrewProjectionContracts.Validate(
                projection, out projectionErrors);
            if (projectionCode != D20CrewProjectionFailureCode.None)
                return Failure(projectionCode, projectionErrors);

            return new D20CrewProjectionResult
            {
                Valid = true,
                Code = D20CrewProjectionFailureCode.None,
                Errors = new ReadOnlyCollection<string>(new List<string>()),
                Projection = projection,
                SourceOwnerSystem = projection.SourceOwnerSystem,
            };
        }

        /// <summary>直接校验已发布的 D20 投影，不返回输入引用。</summary>
        public static D20CrewProjectionValidationResult Validate(
            D20CrewRosterProjection projection)
        {
            ReadOnlyCollection<string> errors;
            var code = D20CrewProjectionContracts.Validate(projection, out errors);
            return new D20CrewProjectionValidationResult
            {
                Valid = code == D20CrewProjectionFailureCode.None,
                Code = code,
                Errors = errors,
            };
        }

        private static D20CrewProjectionResult Failure(
            D20CrewProjectionFailureCode code,
            ReadOnlyCollection<string> errors)
        {
            return new D20CrewProjectionResult
            {
                Valid = false,
                Code = code,
                Errors = errors ?? new ReadOnlyCollection<string>(new List<string>()),
                Projection = null,
                SourceOwnerSystem = string.Empty,
            };
        }
    }
}
