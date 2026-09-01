// D20-L40 船员公共只读投影纯逻辑测试。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D20;

namespace WhalesIslandReef.Tests.D20
{
    /// <summary>D20 owner-read 投影的 schema、拷贝、排序和 fail-closed 测试。</summary>
    public static class D20CrewProjectionTests
    {
        /// <summary>运行全部断言并返回断言数量。</summary>
        public static int Run()
        {
            int assertions = 0;
            D20CrewOwnerRosterReadSnapshot owner = CreateOwnerSnapshot();
            D20CrewOwnerRosterReadSnapshot ownerBefore = owner.Clone();

            D20CrewProjectionResult projected = D20CrewProjectionAdapter.Project(owner);
            Check(ref assertions, projected.Valid && projected.Projection != null,
                "valid owner snapshot must create a D20 projection");
            Check(ref assertions,
                projected.Projection.CrewMembers[0].CrewId == "crew:projection:a" &&
                projected.Projection.CrewMembers[1].CrewId == "crew:projection:z",
                "projection members must be sorted by stable crew_id");
            Check(ref assertions,
                projected.Projection.CrewMembers[0].QualifiedRoleIds[0] == "helm",
                "member role strings must have deterministic order");
            Check(ref assertions,
                projected.Projection.SourceOwnerSystem == "D10CrewBehaviorSystem" &&
                projected.Projection.SourceProjectionId == "d10:crew:projection:1",
                "projection must retain auditable owner metadata");
            Check(ref assertions,
                owner.CrewMembers[0].CrewId == "crew:projection:z" &&
                owner.CrewMembers[0].CrewId == ownerBefore.CrewMembers[1].CrewId &&
                owner.CrewMembers[0].QualifiedRoleIds[0] ==
                    ownerBefore.CrewMembers[1].QualifiedRoleIds[0],
                "projection must not reorder or write the owner input");

            D20CrewRosterProjection clone = projected.Projection.Clone();
            clone.CrewMembers[0].CrewId = "crew:changed";
            clone.CrewMembers[0].QualifiedRoleIds[0] = "changed";
            Check(ref assertions,
                projected.Projection.CrewMembers[0].CrewId == "crew:projection:a" &&
                projected.Projection.CrewMembers[0].QualifiedRoleIds[0] == "helm",
                "projection Clone must isolate nested members and role lists");
            Check(ref assertions,
                D20CrewProjectionAdapter.Validate(projected.Projection).Valid,
                "published projection must pass its own schema validation");
            Check(ref assertions,
                projected.Projection.Fingerprint() == projected.Projection.Clone().Fingerprint(),
                "projection fingerprint must be deterministic across clones");

            D20CrewOwnerRosterReadSnapshot duplicateCrew = owner.Clone();
            duplicateCrew.CrewMembers.Add(duplicateCrew.CrewMembers[0].Clone());
            D20CrewProjectionResult duplicateCrewResult =
                D20CrewProjectionAdapter.Project(duplicateCrew);
            Check(ref assertions,
                !duplicateCrewResult.Valid &&
                duplicateCrewResult.Code == D20CrewProjectionFailureCode.DuplicateCrewId,
                "duplicate crew ids must fail closed");

            D20CrewOwnerRosterReadSnapshot duplicateRole = owner.Clone();
            duplicateRole.CrewMembers[0].QualifiedRoleIds.Add("HELM");
            D20CrewProjectionResult duplicateRoleResult =
                D20CrewProjectionAdapter.Project(duplicateRole);
            Check(ref assertions,
                !duplicateRoleResult.Valid &&
                duplicateRoleResult.Code == D20CrewProjectionFailureCode.DuplicateRoleId,
                "case-insensitive duplicate role ids must fail closed");

            D20CrewOwnerRosterReadSnapshot missingMembers = owner.Clone();
            missingMembers.CrewMembers = null;
            D20CrewProjectionResult missingMembersResult =
                D20CrewProjectionAdapter.Project(missingMembers);
            Check(ref assertions,
                !missingMembersResult.Valid &&
                missingMembersResult.Code == D20CrewProjectionFailureCode.MembersMissing,
                "missing member data must not be guessed as an empty roster");

            D20CrewOwnerRosterReadSnapshot invalidScore = owner.Clone();
            invalidScore.CrewMembers[0].MoraleScore = double.NaN;
            D20CrewProjectionResult invalidScoreResult =
                D20CrewProjectionAdapter.Project(invalidScore);
            Check(ref assertions,
                !invalidScoreResult.Valid &&
                invalidScoreResult.Code == D20CrewProjectionFailureCode.MemberInvalid,
                "non-finite owner scores must fail closed");

            D20CrewOwnerRosterReadSnapshot missingOwner = owner.Clone();
            missingOwner.SourceOwnerSystem = string.Empty;
            D20CrewProjectionResult missingOwnerResult =
                D20CrewProjectionAdapter.Project(missingOwner);
            Check(ref assertions,
                !missingOwnerResult.Valid &&
                missingOwnerResult.Code == D20CrewProjectionFailureCode.MetadataInvalid,
                "missing source owner metadata must fail closed");

            Check(ref assertions,
                !D20CrewProjectionAdapter.Project(null).Valid,
                "missing owner projection must fail closed");
            return assertions;
        }

        private static D20CrewOwnerRosterReadSnapshot CreateOwnerSnapshot()
        {
            return new D20CrewOwnerRosterReadSnapshot
            {
                SchemaVersion = D20CrewProjectionContracts.CurrentSchemaVersion,
                ShipId = "ship:projection:01",
                SaveRevision = 7,
                SourceProjectionId = "d10:crew:projection:1",
                SourceOwnerSystem = "D10CrewBehaviorSystem",
                CrewMembers = new List<D20CrewMemberOwnerReadSnapshot>
                {
                    new D20CrewMemberOwnerReadSnapshot
                    {
                        CrewId = "crew:projection:z",
                        QualifiedRoleIds = new List<string> { "navigator" },
                        IsAvailable = true,
                        IsIncapacitated = false,
                        MoraleScore = 80,
                        FatigueScore = 20,
                        HealthScore = 95,
                    },
                    new D20CrewMemberOwnerReadSnapshot
                    {
                        CrewId = "crew:projection:a",
                        QualifiedRoleIds = new List<string> { "navigator", "helm" },
                        IsAvailable = true,
                        IsIncapacitated = false,
                        MoraleScore = 90,
                        FatigueScore = 10,
                        HealthScore = 100,
                    },
                },
            };
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
