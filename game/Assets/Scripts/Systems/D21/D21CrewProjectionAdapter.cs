// ============================================================
// D21CrewProjectionAdapter —— D21-L06/L24 D20 船员公开投影适配（纯 C#）
//
// D20CrewRosterProjection 是唯一跨 D20/D21 的船员公开输入。本文件只读、
// 校验并映射该投影为既有 D21CrewRosterSnapshot，再交给
// D21CrewRequirementRules 裁决；不写 D10/Crew owner，也不把未知岗位降级
// 成 Deckhand 或其它合法岗位。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D20;

namespace Whalebone.Systems.D21
{
    /// <summary>D20 船员投影到 D21 映射的稳定结果码。</summary>
    public enum D21CrewProjectionAdapterFailureCode
    {
        /// <summary>映射通过。</summary>
        None = 0,
        /// <summary>输入投影缺失。</summary>
        MissingProjection = 1,
        /// <summary>D20 投影 schema 或字段无效。</summary>
        InvalidProjection = 2,
        /// <summary>投影船只与命令船只不一致。</summary>
        ShipMismatch = 3,
        /// <summary>投影修订与命令修订不一致。</summary>
        SaveRevisionMismatch = 4,
        /// <summary>修订无法安全映射到 D21 int。</summary>
        SaveRevisionOutOfRange = 5,
        /// <summary>岗位字符串没有显式 D21 映射。</summary>
        UnknownRole = 6,
        /// <summary>多个字符串映射到了同一个 D21 岗位。</summary>
        DuplicateMappedRole = 7,
    }

    /// <summary>D20 船员公开投影到 D21 roster 的只读结果。</summary>
    [Serializable]
    public sealed class D21CrewProjectionMappingResult
    {
        /// <summary>是否成功映射。</summary>
        public bool Valid;

        /// <summary>稳定映射结果码。</summary>
        public D21CrewProjectionAdapterFailureCode Code;

        /// <summary>机器可读诊断码集合。</summary>
        public ReadOnlyCollection<string> Errors =
            new ReadOnlyCollection<string>(new List<string>());

        /// <summary>成功时生成的防御性 D21 roster。</summary>
        public D21CrewRosterSnapshot Roster;

        /// <summary>映射保留的 D20 来源投影 ID。</summary>
        public string SourceProjectionId = string.Empty;

        /// <summary>映射保留的真实 owner 系统标识。</summary>
        public string SourceOwnerSystem = string.Empty;

        /// <summary>返回不暴露内部引用的副本。</summary>
        public D21CrewProjectionMappingResult Clone()
        {
            return new D21CrewProjectionMappingResult
            {
                Valid = Valid,
                Code = Code,
                Errors = new ReadOnlyCollection<string>(
                    Errors == null ? new List<string>() : new List<string>(Errors)),
                Roster = Roster == null ? null : Roster.Clone(),
                SourceProjectionId = SourceProjectionId ?? string.Empty,
                SourceOwnerSystem = SourceOwnerSystem ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// D20 船员公共投影的 D21 只读适配器。expectedSaveRevision 为负数时
    /// 只做投影自身校验；D21 Start 会传入命令修订以阻止陈旧快照混入航程。
    /// </summary>
    public static class D21CrewProjectionAdapter
    {
        /// <summary>适配器自身 owner 标识；不代表船员事实写者。</summary>
        public const string OwnerSystem = "D21CrewProjectionAdapter";

        /// <summary>将 D20 公共投影映射为 D21 roster。</summary>
        public static D21CrewProjectionMappingResult Map(
            D20CrewRosterProjection projection,
            string expectedShipId = "",
            long expectedSaveRevision = -1)
        {
            if (projection == null)
                return Failure(D21CrewProjectionAdapterFailureCode.MissingProjection,
                    "d21.crew.projection_missing");

            D20CrewProjectionValidationResult validation =
                D20CrewProjectionAdapter.Validate(projection);
            if (validation == null || !validation.Valid)
            {
                var errors = new List<string> { "d21.crew.d20_projection_invalid" };
                if (validation != null && validation.Errors != null)
                    foreach (string error in validation.Errors)
                        errors.Add("d21.crew.d20:" + (error ?? string.Empty));
                return Failure(D21CrewProjectionAdapterFailureCode.InvalidProjection, errors);
            }

            if (!string.IsNullOrEmpty(expectedShipId) &&
                !string.Equals(projection.ShipId, expectedShipId, StringComparison.Ordinal))
            {
                return Failure(D21CrewProjectionAdapterFailureCode.ShipMismatch,
                    "d21.crew.ship_id_mismatch");
            }
            if (expectedSaveRevision >= 0 && projection.SaveRevision != expectedSaveRevision)
            {
                return Failure(D21CrewProjectionAdapterFailureCode.SaveRevisionMismatch,
                    "d21.crew.save_revision_mismatch");
            }
            if (projection.SaveRevision > int.MaxValue)
            {
                return Failure(D21CrewProjectionAdapterFailureCode.SaveRevisionOutOfRange,
                    "d21.crew.save_revision_out_of_range");
            }

            D20CrewRosterProjection source = projection.Clone();
            var roster = new D21CrewRosterSnapshot
            {
                SchemaVersion = D21CrewRequirementRules.CurrentSchemaVersion,
                ShipId = source.ShipId,
                SaveRevision = (int)source.SaveRevision,
                SourceProjectionId = source.SourceProjectionId,
                CrewMembers = new List<D21CrewMemberReadSnapshot>(),
            };
            var errorsForRoles = new List<string>();
            foreach (D20CrewMemberProjection member in source.CrewMembers)
            {
                var mapped = new D21CrewMemberReadSnapshot
                {
                    CrewId = member.CrewId,
                    IsAvailable = member.IsAvailable,
                    IsIncapacitated = member.IsIncapacitated,
                    MoraleScore = member.MoraleScore,
                    FatigueScore = member.FatigueScore,
                    HealthScore = member.HealthScore,
                    QualifiedRoles = new List<D21CrewRole>(),
                };
                var mappedRoles = new HashSet<D21CrewRole>();
                foreach (string roleId in member.QualifiedRoleIds)
                {
                    D21CrewRole role;
                    if (!TryMapRole(roleId, out role))
                    {
                        errorsForRoles.Add("d21.crew.role_unknown:" + (roleId ?? string.Empty));
                        continue;
                    }
                    if (!mappedRoles.Add(role))
                    {
                        errorsForRoles.Add("d21.crew.role_duplicate_after_mapping:" +
                            (roleId ?? string.Empty));
                        continue;
                    }
                    mapped.QualifiedRoles.Add(role);
                }
                mapped.QualifiedRoles.Sort();
                roster.CrewMembers.Add(mapped);
            }

            if (errorsForRoles.Count > 0)
            {
                var code = ContainsPrefix(errorsForRoles,
                    "d21.crew.role_duplicate_after_mapping")
                    ? D21CrewProjectionAdapterFailureCode.DuplicateMappedRole
                    : D21CrewProjectionAdapterFailureCode.UnknownRole;
                return Failure(code, errorsForRoles);
            }

            roster.CrewMembers.Sort(CompareMembers);
            ReadOnlyCollection<string> rosterErrors;
            D21CrewRequirementFailureCode rosterCode =
                D21CrewRequirementRules.ValidateRoster(roster, out rosterErrors);
            if (rosterCode != D21CrewRequirementFailureCode.None)
            {
                var errors = new List<string> { "d21.crew.d21_roster_invalid" };
                if (rosterErrors != null)
                    foreach (string error in rosterErrors)
                        errors.Add("d21.crew.d21:" + (error ?? string.Empty));
                return Failure(D21CrewProjectionAdapterFailureCode.InvalidProjection, errors);
            }

            return new D21CrewProjectionMappingResult
            {
                Valid = true,
                Code = D21CrewProjectionAdapterFailureCode.None,
                Errors = new ReadOnlyCollection<string>(new List<string>()),
                Roster = roster,
                SourceProjectionId = source.SourceProjectionId,
                SourceOwnerSystem = source.SourceOwnerSystem,
            };
        }

        /// <summary>把一个 owner 岗位字符串映射到唯一 D21 逻辑岗位。</summary>
        public static bool TryMapRole(string roleId, out D21CrewRole role)
        {
            role = D21CrewRole.Unknown;
            if (string.IsNullOrEmpty(roleId)) return false;
            switch (roleId.ToLowerInvariant())
            {
                case "helm":
                case "helmsman":
                    role = D21CrewRole.Helmsman;
                    return true;
                case "sailmaster":
                case "sail_master":
                case "sail-master":
                    role = D21CrewRole.Sailmaster;
                    return true;
                case "lookout":
                    role = D21CrewRole.Lookout;
                    return true;
                case "navigator":
                    role = D21CrewRole.Navigator;
                    return true;
                case "repair":
                case "carpenter":
                    role = D21CrewRole.Carpenter;
                    return true;
                case "cook":
                    role = D21CrewRole.Cook;
                    return true;
                case "cargo_master":
                case "cargomaster":
                    role = D21CrewRole.CargoMaster;
                    return true;
                case "boatmaster":
                case "boat_master":
                    role = D21CrewRole.Boatmaster;
                    return true;
                case "gunner":
                    role = D21CrewRole.Gunner;
                    return true;
                case "medic":
                    role = D21CrewRole.Medic;
                    return true;
                case "deckhand":
                    role = D21CrewRole.Deckhand;
                    return true;
                default:
                    return false;
            }
        }

        private static int CompareMembers(
            D21CrewMemberReadSnapshot left,
            D21CrewMemberReadSnapshot right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            return StringComparer.Ordinal.Compare(left.CrewId, right.CrewId);
        }

        private static bool ContainsPrefix(List<string> errors, string prefix)
        {
            foreach (string error in errors)
                if (error != null && error.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static D21CrewProjectionMappingResult Failure(
            D21CrewProjectionAdapterFailureCode code,
            string error)
        {
            return Failure(code, new[] { error ?? string.Empty });
        }

        private static D21CrewProjectionMappingResult Failure(
            D21CrewProjectionAdapterFailureCode code,
            IEnumerable<string> errors)
        {
            return new D21CrewProjectionMappingResult
            {
                Valid = false,
                Code = code,
                Errors = new ReadOnlyCollection<string>(
                    new List<string>(errors ?? new string[0])),
                Roster = null,
                SourceProjectionId = string.Empty,
                SourceOwnerSystem = string.Empty,
            };
        }
    }
}
