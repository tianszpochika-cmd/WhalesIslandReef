// ============================================================
// D11WhaleDolphinContracts —— 鲸豚路线、事实与结算请求 DTO
//
// 本文件只保存确定性逻辑所需的数据。它不引用 Unity，不持有船体、
// 船员、金钱、势力或生态系统的写引用；这些领域的最终写者仍由
// EcologySystem / FactionSystem / ShipSystem / CrewSystem / RewardSystem
// 负责。D11 路线系统只写自己的路线投影和可追溯请求。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D11
{
    public static class D11WhaleDolphinSchema
    {
        public const int CurrentVersion = 1;
        public const string OwnerSystem = "EcologySystem";
        public const string HistoricalReviewTag = "history_1675_caribbean_whaling";
        public const string SensitiveContentReview = "sensitive_whaling_non_glorifying_review_required";
    }

    public enum D11MarineSpeciesKind
    {
        Unknown,
        Whale,
        Dolphin,
    }

    public enum D11MarineRouteAction
    {
        Unknown,
        Migration,
        Observation,
        Escort,
        StrandingRescue,
        Whaling,
        AbandonTracking,
    }

    public enum D11RouteStatus
    {
        Unknown,
        Recorded,
        Completed,
        Rejected,
        Failed,
        Abandoned,
    }

    public enum D11WhalingLicenseStatus
    {
        Unknown,
        None,
        Expired,
        Valid,
        Suspended,
    }

    public enum D11WhalingLawPolicy
    {
        Unknown,
        Prohibited,
        ResearchOnly,
        SubsistenceOnly,
        CommercialAllowed,
    }

    [Serializable]
    public sealed class D11MarineEnvironmentReadSnapshot
    {
        public string WeatherStateId = "Calm";
        public float Visibility = 1f;
        public float CurrentSpeed;
        public float CurrentDirectionDegrees;

        public D11MarineEnvironmentReadSnapshot Clone()
        {
            return new D11MarineEnvironmentReadSnapshot
            {
                WeatherStateId = WeatherStateId,
                Visibility = Visibility,
                CurrentSpeed = CurrentSpeed,
                CurrentDirectionDegrees = CurrentDirectionDegrees,
            };
        }
    }

    /// <summary>A01 EcologySystem 提供的只读动物/生境输入。</summary>
    [Serializable]
    public sealed class D11MarineEcologyReadSnapshot
    {
        public string HabitatId = "open_sea";
        public float EcologyStress;
        public bool ProtectedPopulation;
        public bool IsStranded;
        public bool ObservationBlocked;
        public bool DolphinEscortAvailable = true;

        public D11MarineEcologyReadSnapshot Clone()
        {
            return new D11MarineEcologyReadSnapshot
            {
                HabitatId = HabitatId,
                EcologyStress = EcologyStress,
                ProtectedPopulation = ProtectedPopulation,
                IsStranded = IsStranded,
                ObservationBlocked = ObservationBlocked,
                DolphinEscortAvailable = DolphinEscortAvailable,
            };
        }
    }

    /// <summary>
    /// Faction/Legal、Ship 和 Crew 的只读裁决输入。D11 不修改其中任何字段。
    /// </summary>
    [Serializable]
    public sealed class D11WhalingPolicySnapshot
    {
        public D11WhalingLicenseStatus LicenseStatus = D11WhalingLicenseStatus.None;
        public string LicenseId = "";
        public D11WhalingLawPolicy LawPolicy = D11WhalingLawPolicy.Prohibited;
        public string FactionId = "";
        public int SocialReputation;
        public int MinimumSocialReputation = 0;
        public string SeasonId = "unknown";
        public string RequiredSeasonId = "unknown";
        public int CrewCount;
        public int RequiredCrewCount = 1;
        public string ShipClassId = "";
        public string RequiredShipClassId = "whaling_vessel";
        public bool HasProcessingFacility;
        public bool EquipmentReady;
        public int ShipConditionPercent = 100;
        public int CrewReadinessPercent = 100;
        public bool ProtectedPopulation;
        public float EcologyStress;

        public D11WhalingPolicySnapshot Clone()
        {
            return new D11WhalingPolicySnapshot
            {
                LicenseStatus = LicenseStatus,
                LicenseId = LicenseId,
                LawPolicy = LawPolicy,
                FactionId = FactionId,
                SocialReputation = SocialReputation,
                MinimumSocialReputation = MinimumSocialReputation,
                SeasonId = SeasonId,
                RequiredSeasonId = RequiredSeasonId,
                CrewCount = CrewCount,
                RequiredCrewCount = RequiredCrewCount,
                ShipClassId = ShipClassId,
                RequiredShipClassId = RequiredShipClassId,
                HasProcessingFacility = HasProcessingFacility,
                EquipmentReady = EquipmentReady,
                ShipConditionPercent = ShipConditionPercent,
                CrewReadinessPercent = CrewReadinessPercent,
                ProtectedPopulation = ProtectedPopulation,
                EcologyStress = EcologyStress,
            };
        }
    }

    [Serializable]
    public sealed class D11MarineEncounter
    {
        public int SchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public string EncounterId = "";
        public string SpeciesId = "";
        public D11MarineSpeciesKind SpeciesKind = D11MarineSpeciesKind.Unknown;
        public string RouteRef = "";
        public string HabitatId = "open_sea";
        public float WorldX;
        public float WorldZ;
        public int MigrationDirectionDegrees;
        public int LastMigratedDay;
        public float EcologyStress;
        public bool ProtectedPopulation;
        public bool IsStranded;
        public bool Observed;
        public bool Escorted;
        public bool Rescued;
        public bool WhalingAttempted;
        public bool WhalingResolved;
        public bool TrackingAbandoned;
        public string LastEventId = "";
        public List<string> CauseEventIds = new List<string>();

        public D11MarineEncounter Clone()
        {
            var result = new D11MarineEncounter
            {
                SchemaVersion = SchemaVersion,
                EncounterId = EncounterId,
                SpeciesId = SpeciesId,
                SpeciesKind = SpeciesKind,
                RouteRef = RouteRef,
                HabitatId = HabitatId,
                WorldX = WorldX,
                WorldZ = WorldZ,
                MigrationDirectionDegrees = MigrationDirectionDegrees,
                LastMigratedDay = LastMigratedDay,
                EcologyStress = EcologyStress,
                ProtectedPopulation = ProtectedPopulation,
                IsStranded = IsStranded,
                Observed = Observed,
                Escorted = Escorted,
                Rescued = Rescued,
                WhalingAttempted = WhalingAttempted,
                WhalingResolved = WhalingResolved,
                TrackingAbandoned = TrackingAbandoned,
                LastEventId = LastEventId,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D11WhaleDolphinActionRequest
    {
        public D11MarineRouteAction Action = D11MarineRouteAction.Unknown;
        public string EncounterId = "";
        public string ActorId = "player";
        public string LocationRef = "open_sea";
        public int WorldDay;
        public int Seed;
        public long SaveRevision;
        public string EventId = "";
        public D11MarineEnvironmentReadSnapshot Environment = new D11MarineEnvironmentReadSnapshot();
        public D11WhalingPolicySnapshot WhalingPolicy = new D11WhalingPolicySnapshot();
        public int RescueCrewCount;
        public bool HasRescueKit;

        public D11WhaleDolphinActionRequest Clone()
        {
            return new D11WhaleDolphinActionRequest
            {
                Action = Action,
                EncounterId = EncounterId,
                ActorId = ActorId,
                LocationRef = LocationRef,
                WorldDay = WorldDay,
                Seed = Seed,
                SaveRevision = SaveRevision,
                EventId = EventId,
                Environment = Environment == null ? new D11MarineEnvironmentReadSnapshot() : Environment.Clone(),
                WhalingPolicy = WhalingPolicy == null ? new D11WhalingPolicySnapshot() : WhalingPolicy.Clone(),
                RescueCrewCount = RescueCrewCount,
                HasRescueKit = HasRescueKit,
            };
        }
    }

    [Serializable]
    public sealed class D11RewardRequest
    {
        public int SchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public string RequestId = "";
        public string RewardKind = "";
        public string OwnerSystem = "RewardSystem";
        public string SourceEventId = "";
        public string Status = "Pending";
        public int Quantity = 1;
        public string Description = "";

        public D11RewardRequest Clone()
        {
            return new D11RewardRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                RewardKind = RewardKind,
                OwnerSystem = OwnerSystem,
                SourceEventId = SourceEventId,
                Status = Status,
                Quantity = Quantity,
                Description = Description,
            };
        }
    }

    [Serializable]
    public sealed class D11ConsequenceRequest
    {
        public int SchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public string RequestId = "";
        public string OwnerSystem = "";
        public string SourceEventId = "";
        public string ConsequenceKind = "";
        public float EcologyStressDelta;
        public float ShipRiskDelta;
        public int CrewRiskDelta;
        public int SocialReputationDelta;
        public string LegalConsequenceCode = "none";
        public string Status = "Pending";

        public D11ConsequenceRequest Clone()
        {
            return new D11ConsequenceRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                OwnerSystem = OwnerSystem,
                SourceEventId = SourceEventId,
                ConsequenceKind = ConsequenceKind,
                EcologyStressDelta = EcologyStressDelta,
                ShipRiskDelta = ShipRiskDelta,
                CrewRiskDelta = CrewRiskDelta,
                SocialReputationDelta = SocialReputationDelta,
                LegalConsequenceCode = LegalConsequenceCode,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D11QuestFactRequest
    {
        public int SchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public string FactId = "";
        public string SourceEventId = "";
        public string FactKind = "";
        public string Status = "Pending";

        public D11QuestFactRequest Clone()
        {
            return new D11QuestFactRequest
            {
                SchemaVersion = SchemaVersion,
                FactId = FactId,
                SourceEventId = SourceEventId,
                FactKind = FactKind,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D11RouteRecord
    {
        public int SchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public string RecordId = "";
        public string EncounterId = "";
        public D11MarineRouteAction Action = D11MarineRouteAction.Unknown;
        public D11RouteStatus Status = D11RouteStatus.Unknown;
        public string Code = "";
        public string Message = "";
        public string EventId = "";
        public int WorldDay;
        public long SaveRevision;
        public int RiskRoll;
        public int RiskThreshold;
        public float EcologyStressDelta;
        public int SocialReputationDelta;
        public string LegalConsequenceCode = "none";
        public string RewardRequestId = "";
        public string RecoveryRouteId = "";
        public string HistoricalReviewTag = D11WhaleDolphinSchema.HistoricalReviewTag;
        public string SensitiveContentReview = D11WhaleDolphinSchema.SensitiveContentReview;
        public List<string> CauseEventIds = new List<string>();

        public D11RouteRecord Clone()
        {
            var result = new D11RouteRecord
            {
                SchemaVersion = SchemaVersion,
                RecordId = RecordId,
                EncounterId = EncounterId,
                Action = Action,
                Status = Status,
                Code = Code,
                Message = Message,
                EventId = EventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                RiskRoll = RiskRoll,
                RiskThreshold = RiskThreshold,
                EcologyStressDelta = EcologyStressDelta,
                SocialReputationDelta = SocialReputationDelta,
                LegalConsequenceCode = LegalConsequenceCode,
                RewardRequestId = RewardRequestId,
                RecoveryRouteId = RecoveryRouteId,
                HistoricalReviewTag = HistoricalReviewTag,
                SensitiveContentReview = SensitiveContentReview,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D11WhaleDolphinRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D11WhaleDolphinSchema.CurrentVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public int WorldDay;
        public long Revision;
        public string LastEventId = "";
        public List<string> AppliedEventKeys = new List<string>();
        public List<D11MarineEncounter> Encounters = new List<D11MarineEncounter>();
        public List<D11RouteRecord> RouteRecords = new List<D11RouteRecord>();
        public List<D11RewardRequest> RewardRequests = new List<D11RewardRequest>();
        public List<D11ConsequenceRequest> ConsequenceRequests = new List<D11ConsequenceRequest>();
        public List<D11QuestFactRequest> QuestFacts = new List<D11QuestFactRequest>();

        public D11WhaleDolphinRuntimeSnapshot Clone()
        {
            var result = new D11WhaleDolphinRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                WorldDay = WorldDay,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            if (Encounters != null) foreach (var item in Encounters)
                if (item != null) result.Encounters.Add(item.Clone());
            if (RouteRecords != null) foreach (var item in RouteRecords)
                if (item != null) result.RouteRecords.Add(item.Clone());
            if (RewardRequests != null) foreach (var item in RewardRequests)
                if (item != null) result.RewardRequests.Add(item.Clone());
            if (ConsequenceRequests != null) foreach (var item in ConsequenceRequests)
                if (item != null) result.ConsequenceRequests.Add(item.Clone());
            if (QuestFacts != null) foreach (var item in QuestFacts)
                if (item != null) result.QuestFacts.Add(item.Clone());
            return result;
        }

        public void EnsureDefaults()
        {
            if (AppliedEventKeys == null) AppliedEventKeys = new List<string>();
            if (Encounters == null) Encounters = new List<D11MarineEncounter>();
            if (RouteRecords == null) RouteRecords = new List<D11RouteRecord>();
            if (RewardRequests == null) RewardRequests = new List<D11RewardRequest>();
            if (ConsequenceRequests == null) ConsequenceRequests = new List<D11ConsequenceRequest>();
            if (QuestFacts == null) QuestFacts = new List<D11QuestFactRequest>();
            if (SchemaVersion == 0) SchemaVersion = CurrentSchemaVersion;
        }

        public void Validate()
        {
            EnsureDefaults();
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D11 whale/dolphin schema_version mismatch: " + SchemaVersion);
            if (WorldDay < 0 || Revision < 0) throw new InvalidOperationException("D11 snapshot counters cannot be negative.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var encounter in Encounters)
            {
                if (encounter == null || string.IsNullOrWhiteSpace(encounter.EncounterId))
                    throw new InvalidOperationException("D11 encounter id is required.");
                if (!ids.Add(encounter.EncounterId)) throw new InvalidOperationException("Duplicate D11 encounter id: " + encounter.EncounterId);
            }
            ids.Clear();
            foreach (var record in RouteRecords)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.RecordId))
                    throw new InvalidOperationException("D11 route record id is required.");
                if (!ids.Add(record.RecordId)) throw new InvalidOperationException("Duplicate D11 route record id: " + record.RecordId);
            }
        }
    }

    [Serializable]
    public sealed class D11WhaleDolphinRouteResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool Duplicate;
        public string Code = "";
        public string Message = "";
        public string EventId = "";
        public D11RouteRecord Record;
        public List<D11RewardRequest> RewardRequests = new List<D11RewardRequest>();
        public List<D11ConsequenceRequest> ConsequenceRequests = new List<D11ConsequenceRequest>();
        public List<D11QuestFactRequest> QuestFacts = new List<D11QuestFactRequest>();
    }
}
