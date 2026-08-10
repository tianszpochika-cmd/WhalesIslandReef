// D14-A02 接舷、俘虏与战后处理契约。
// 本模块只编排带来源的 authority 决定；伤亡、俘虏、库存、治疗和关系数值
// 仍由各所属系统写入，D14 只保存其结果投影和统一事实因果。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14AftercareIds
    {
        public const int SchemaVersion = 1;
        public const string OwnerSystem = "D14BoardingAftercareSystem";
        public const string CombatOwner = "CombatSystem";
        public const string CasualtyOwner = "CrewSystem";
        public const string PrisonerOwner = "PrisonerSystem";
        public const string LootOwner = "InventorySystem";
        public const string MedicalOwner = "MedicalSystem";
        public const string FuneralOwner = "CommunitySystem";
        public const string RelationOwner = "RelationSystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string RecoveryRoute = "route_d14_boarding_aftercare_review";
        public const string SensitiveRecoveryRoute = "route_d14_sensitive_content_review";
        public const string EventPrefix = "event_d14_aftercare_";
    }

    public enum D14BoardingCaseStatus
    {
        Open,
        Captured,
        Surrendered,
        Retreated,
        Rescued,
        Released,
        Closed,
    }

    public enum D14AftercareOutcome
    {
        Captured,
        Surrendered,
        Retreated,
        NonLethalDisengagement,
        Rescued,
        Released,
    }

    public enum D14ParticipantSide
    {
        Player,
        Enemy,
    }

    public enum D14ParticipantRole
    {
        Captain,
        Officer,
        Boarder,
        Gunner,
        Sailor,
        Medic,
        Civilian,
    }

    public enum D14BoardingWeapon
    {
        None,
        Cutlass,
        Pistol,
        BoardingAxe,
        Musket,
        Improvised,
    }

    public enum D14CasualtyStatus
    {
        Wounded,
        Dead,
        Missing,
        Recovered,
    }

    public enum D14PrisonerStatus
    {
        Held,
        Rescued,
        Released,
        Transferred,
        Escaped,
    }

    public enum D14LootStatus
    {
        Requested,
        Granted,
        Denied,
        SeizedForEvidence,
    }

    public enum D14TreatmentStatus
    {
        Treated,
        Stabilized,
        Rejected,
        Deferred,
    }

    public enum D14FuneralStatus
    {
        Held,
        Skipped,
        Deferred,
    }

    public enum D14SensitiveReviewStatus
    {
        Pending,
        Approved,
        ApprovedWithWarning,
        Rejected,
    }

    public enum D14RelationMetric
    {
        Affinity,
        Trust,
        Loyalty,
        Fear,
        Respect,
    }

    [Serializable]
    public sealed class D14SensitiveContentTag
    {
        public string ContentId;
        public string HistoricalTag = "inferred";
        public string SensitiveLabel;
        public string ContentWarning;
        public D14SensitiveReviewStatus ReviewStatus = D14SensitiveReviewStatus.Pending;
        public bool ReviewAcknowledged;
        public bool SkipSensitiveContent;
        public string SourceNote;
        public List<string> Tags = new List<string>();

        public D14SensitiveContentTag Clone()
        {
            var result = new D14SensitiveContentTag
            {
                ContentId = ContentId, HistoricalTag = HistoricalTag, SensitiveLabel = SensitiveLabel,
                ContentWarning = ContentWarning, ReviewStatus = ReviewStatus, ReviewAcknowledged = ReviewAcknowledged,
                SkipSensitiveContent = SkipSensitiveContent, SourceNote = SourceNote,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate(string field, bool requireDecision)
        {
            if (string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(HistoricalTag) || string.IsNullOrWhiteSpace(SourceNote))
                throw new InvalidOperationException(field + " requires content_id, historical_tag and source_note.");
            if (!Enum.IsDefined(typeof(D14SensitiveReviewStatus), ReviewStatus))
                throw new InvalidOperationException(field + ".review_status is invalid.");
            if (ReviewStatus == D14SensitiveReviewStatus.ApprovedWithWarning && string.IsNullOrWhiteSpace(ContentWarning))
                throw new InvalidOperationException(field + " approved_with_warning requires content_warning.");
            if (requireDecision && ReviewStatus == D14SensitiveReviewStatus.Pending)
                throw new InvalidOperationException(field + " requires a completed sensitive-content review.");
            if (ReviewStatus == D14SensitiveReviewStatus.ApprovedWithWarning && !ReviewAcknowledged && !SkipSensitiveContent)
                throw new InvalidOperationException(field + " warning must be acknowledged or skipped.");
        }
    }

    [Serializable]
    public sealed class D14BoardingParticipant
    {
        public string ParticipantId;
        public D14ParticipantSide Side;
        public D14ParticipantRole Role;
        public D14BoardingWeapon Weapon;
        public int Morale = 50;
        public int Health = 100;
        public bool WillingToSurrender;

        public D14BoardingParticipant Clone()
        {
            return new D14BoardingParticipant
            {
                ParticipantId = ParticipantId, Side = Side, Role = Role, Weapon = Weapon,
                Morale = Morale, Health = Health, WillingToSurrender = WillingToSurrender,
            };
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ParticipantId)) throw new InvalidOperationException(field + ".participant_id is required.");
            if (!Enum.IsDefined(typeof(D14ParticipantSide), Side) || !Enum.IsDefined(typeof(D14ParticipantRole), Role) || !Enum.IsDefined(typeof(D14BoardingWeapon), Weapon))
                throw new InvalidOperationException(field + " contains an invalid participant enum.");
            if (Morale < 0 || Morale > 100 || Health < 0 || Health > 100)
                throw new InvalidOperationException(field + " morale/health must be 0..100.");
        }
    }

    [Serializable]
    public sealed class D14CasualtyRecord
    {
        public string CasualtyId;
        public string CaseId;
        public string ParticipantId;
        public D14ParticipantSide Side;
        public D14CasualtyStatus Status;
        public int HealthLoss;
        public bool BurialRequested;
        public string SourceEventId;
        public string LastEventId;

        public D14CasualtyRecord Clone()
        {
            return new D14CasualtyRecord
            {
                CasualtyId = CasualtyId, CaseId = CaseId, ParticipantId = ParticipantId, Side = Side,
                Status = Status, HealthLoss = HealthLoss, BurialRequested = BurialRequested,
                SourceEventId = SourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14PrisonerRecord
    {
        public string PrisonerId;
        public string CaseId;
        public string ParticipantId;
        public D14PrisonerStatus Status = D14PrisonerStatus.Held;
        public string CustodyOwner = "player";
        public bool Rescued;
        public string SourceEventId;
        public string LastEventId;

        public D14PrisonerRecord Clone()
        {
            return new D14PrisonerRecord
            {
                PrisonerId = PrisonerId, CaseId = CaseId, ParticipantId = ParticipantId, Status = Status,
                CustodyOwner = CustodyOwner, Rescued = Rescued, SourceEventId = SourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14LootItem
    {
        public string ItemId;
        public int Quantity;
        public string LegalStatus = "legal";
        public string InventorySourceEventId;

        public D14LootItem Clone()
        {
            return new D14LootItem { ItemId = ItemId, Quantity = Quantity, LegalStatus = LegalStatus, InventorySourceEventId = InventorySourceEventId };
        }
    }

    [Serializable]
    public sealed class D14LootRecord
    {
        public string LootId;
        public string CaseId;
        public D14LootStatus Status;
        public string LegalSourceEventId;
        public string InventorySourceEventId;
        public string LastEventId;
        public List<D14LootItem> Items = new List<D14LootItem>();

        public D14LootRecord Clone()
        {
            var result = new D14LootRecord
            {
                LootId = LootId, CaseId = CaseId, Status = Status, LegalSourceEventId = LegalSourceEventId,
                InventorySourceEventId = InventorySourceEventId, LastEventId = LastEventId,
            };
            if (Items != null) foreach (var item in Items) if (item != null) result.Items.Add(item.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D14TreatmentRecord
    {
        public string TreatmentId;
        public string CaseId;
        public string PatientId;
        public D14TreatmentStatus Status;
        public int HealthBefore;
        public int HealthAfter;
        public string MedicalSourceEventId;
        public string LastEventId;

        public D14TreatmentRecord Clone()
        {
            return new D14TreatmentRecord
            {
                TreatmentId = TreatmentId, CaseId = CaseId, PatientId = PatientId, Status = Status,
                HealthBefore = HealthBefore, HealthAfter = HealthAfter, MedicalSourceEventId = MedicalSourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14RescueRecord
    {
        public string RescueId;
        public string CaseId;
        public string MedicalSourceEventId;
        public string LastEventId;
        public List<string> RescuedPrisonerIds = new List<string>();

        public D14RescueRecord Clone()
        {
            var result = new D14RescueRecord { RescueId = RescueId, CaseId = CaseId, MedicalSourceEventId = MedicalSourceEventId, LastEventId = LastEventId };
            if (RescuedPrisonerIds != null) result.RescuedPrisonerIds.AddRange(RescuedPrisonerIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D14InterrogationRecord
    {
        public string InterrogationId;
        public string CaseId;
        public string PrisonerId;
        public bool Skipped;
        public string InformationRef;
        public D14SensitiveContentTag Content;
        public string SourceEventId;
        public string LastEventId;

        public D14InterrogationRecord Clone()
        {
            return new D14InterrogationRecord
            {
                InterrogationId = InterrogationId, CaseId = CaseId, PrisonerId = PrisonerId, Skipped = Skipped,
                InformationRef = InformationRef, Content = Content == null ? null : Content.Clone(), SourceEventId = SourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14FuneralRecord
    {
        public string FuneralId;
        public string CaseId;
        public D14FuneralStatus Status;
        public bool Skipped;
        public D14SensitiveContentTag Content;
        public string CommunitySourceEventId;
        public string LastEventId;
        public List<string> CasualtyIds = new List<string>();

        public D14FuneralRecord Clone()
        {
            var result = new D14FuneralRecord
            {
                FuneralId = FuneralId, CaseId = CaseId, Status = Status, Skipped = Skipped,
                Content = Content == null ? null : Content.Clone(), CommunitySourceEventId = CommunitySourceEventId, LastEventId = LastEventId,
            };
            if (CasualtyIds != null) result.CasualtyIds.AddRange(CasualtyIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D14RelationConsequenceRecord
    {
        public string RelationId;
        public string SourceId;
        public string TargetId;
        public D14RelationMetric Metric;
        public float ProposedDelta;
        public float AppliedDelta;
        public string CauseEventId;
        public string RelationSourceEventId;
        public string LastEventId;

        public D14RelationConsequenceRecord Clone()
        {
            return new D14RelationConsequenceRecord
            {
                RelationId = RelationId, SourceId = SourceId, TargetId = TargetId, Metric = Metric,
                ProposedDelta = ProposedDelta, AppliedDelta = AppliedDelta, CauseEventId = CauseEventId,
                RelationSourceEventId = RelationSourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14BoardingCase
    {
        public string CaseId;
        public string EncounterId;
        public string PlayerShipId;
        public string EnemyShipId;
        public D14BoardingCaseStatus Status = D14BoardingCaseStatus.Open;
        public D14AftercareOutcome Outcome;
        public float Distance;
        public int PlayerMorale;
        public int EnemyMorale;
        public string CombatSourceEventId;
        public string LastEventId;
        public D14SensitiveContentTag Content;
        public List<D14BoardingParticipant> PlayerParty = new List<D14BoardingParticipant>();
        public List<D14BoardingParticipant> EnemyParty = new List<D14BoardingParticipant>();

        public D14BoardingCase Clone()
        {
            var result = new D14BoardingCase
            {
                CaseId = CaseId, EncounterId = EncounterId, PlayerShipId = PlayerShipId, EnemyShipId = EnemyShipId,
                Status = Status, Outcome = Outcome, Distance = Distance, PlayerMorale = PlayerMorale, EnemyMorale = EnemyMorale,
                CombatSourceEventId = CombatSourceEventId, LastEventId = LastEventId, Content = Content == null ? null : Content.Clone(),
            };
            if (PlayerParty != null) foreach (var participant in PlayerParty) if (participant != null) result.PlayerParty.Add(participant.Clone());
            if (EnemyParty != null) foreach (var participant in EnemyParty) if (participant != null) result.EnemyParty.Add(participant.Clone());
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CaseId) || string.IsNullOrWhiteSpace(EncounterId) || string.IsNullOrWhiteSpace(PlayerShipId) || string.IsNullOrWhiteSpace(EnemyShipId))
                throw new InvalidOperationException("D14 boarding case identifiers are required.");
            if (!Enum.IsDefined(typeof(D14BoardingCaseStatus), Status) || !Enum.IsDefined(typeof(D14AftercareOutcome), Outcome))
                throw new InvalidOperationException("D14 boarding case status/outcome is invalid.");
            if (Distance < 0f || float.IsNaN(Distance) || float.IsInfinity(Distance) || PlayerMorale < 0 || PlayerMorale > 100 || EnemyMorale < 0 || EnemyMorale > 100)
                throw new InvalidOperationException("D14 boarding case values are outside their ranges.");
            if (string.IsNullOrWhiteSpace(CombatSourceEventId) || PlayerParty == null || EnemyParty == null)
                throw new InvalidOperationException("D14 boarding case source and parties are required.");
            for (int i = 0; i < PlayerParty.Count; i++) PlayerParty[i].Validate("player_party[" + i + "]");
            for (int i = 0; i < EnemyParty.Count; i++) EnemyParty[i].Validate("enemy_party[" + i + "]");
            if (Content != null) Content.Validate("case.content", false);
        }
    }

    [Serializable]
    public sealed class D14AftercareRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14AftercareIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public List<D14BoardingCase> Cases = new List<D14BoardingCase>();
        public List<D14CasualtyRecord> Casualties = new List<D14CasualtyRecord>();
        public List<D14PrisonerRecord> Prisoners = new List<D14PrisonerRecord>();
        public List<D14LootRecord> Loot = new List<D14LootRecord>();
        public List<D14TreatmentRecord> Treatments = new List<D14TreatmentRecord>();
        public List<D14RescueRecord> Rescues = new List<D14RescueRecord>();
        public List<D14InterrogationRecord> Interrogations = new List<D14InterrogationRecord>();
        public List<D14FuneralRecord> Funerals = new List<D14FuneralRecord>();
        public List<D14RelationConsequenceRecord> Relations = new List<D14RelationConsequenceRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14AftercareRuntimeSnapshot Clone()
        {
            var result = new D14AftercareRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Cases != null) foreach (var item in Cases) if (item != null) result.Cases.Add(item.Clone());
            if (Casualties != null) foreach (var item in Casualties) if (item != null) result.Casualties.Add(item.Clone());
            if (Prisoners != null) foreach (var item in Prisoners) if (item != null) result.Prisoners.Add(item.Clone());
            if (Loot != null) foreach (var item in Loot) if (item != null) result.Loot.Add(item.Clone());
            if (Treatments != null) foreach (var item in Treatments) if (item != null) result.Treatments.Add(item.Clone());
            if (Rescues != null) foreach (var item in Rescues) if (item != null) result.Rescues.Add(item.Clone());
            if (Interrogations != null) foreach (var item in Interrogations) if (item != null) result.Interrogations.Add(item.Clone());
            if (Funerals != null) foreach (var item in Funerals) if (item != null) result.Funerals.Add(item.Clone());
            if (Relations != null) foreach (var item in Relations) if (item != null) result.Relations.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0)
                throw new InvalidOperationException("D14 aftercare schema/time values are invalid.");
            if (Cases == null || Casualties == null || Prisoners == null || Loot == null || Treatments == null || Rescues == null || Interrogations == null || Funerals == null || Relations == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14 aftercare collections are required.");
            foreach (var item in Cases) { if (item == null) throw new InvalidOperationException("D14 aftercare cases cannot contain null."); item.Validate(); }
            foreach (var item in Casualties) if (item == null) throw new InvalidOperationException("D14 casualties cannot contain null.");
            foreach (var item in Prisoners) if (item == null) throw new InvalidOperationException("D14 prisoners cannot contain null.");
            foreach (var item in Loot) if (item == null) throw new InvalidOperationException("D14 loot cannot contain null.");
            foreach (var item in Treatments) if (item == null) throw new InvalidOperationException("D14 treatments cannot contain null.");
            foreach (var item in Rescues) if (item == null) throw new InvalidOperationException("D14 rescues cannot contain null.");
            foreach (var item in Interrogations) if (item == null) throw new InvalidOperationException("D14 interrogations cannot contain null.");
            foreach (var item in Funerals) if (item == null) throw new InvalidOperationException("D14 funerals cannot contain null.");
            foreach (var item in Relations) if (item == null) throw new InvalidOperationException("D14 relations cannot contain null.");
        }
    }

    [Serializable]
    public class D14BoardingRequest
    {
        public string OperationId;
        public string CaseId;
        public D14NavalCombatState CombatSnapshot;
        public List<D14BoardingParticipant> PlayerParty = new List<D14BoardingParticipant>();
        public List<D14BoardingParticipant> EnemyParty = new List<D14BoardingParticipant>();
        public D14SensitiveContentTag Content;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14SurrenderAftercareRequest : D14BoardingRequest
    {
    }

    [Serializable]
    public sealed class D14BoardingResolutionRequest
    {
        public string OperationId;
        public string CaseId;
        public D14AftercareOutcome RequestedOutcome = D14AftercareOutcome.NonLethalDisengagement;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14RescueRequest
    {
        public string OperationId;
        public string CaseId;
        public List<string> PrisonerIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14TreatmentRequest
    {
        public string OperationId;
        public string CaseId;
        public string CasualtyId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14LootRequest
    {
        public string OperationId;
        public string CaseId;
        public string LegalSourceEventId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14InterrogationRequest
    {
        public string OperationId;
        public string CaseId;
        public string PrisonerId;
        public D14SensitiveContentTag Content;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14FuneralRequest
    {
        public string OperationId;
        public string CaseId;
        public List<string> CasualtyIds = new List<string>();
        public D14SensitiveContentTag Content;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14BoardingDecision
    {
        public bool Allowed;
        public string Code = "boarding_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.CombatOwner;
        public string SourceEventId;
        public D14AftercareOutcome Outcome = D14AftercareOutcome.NonLethalDisengagement;
        public int PlayerMorale;
        public int EnemyMorale;
        public List<D14RelationProposal> RelationProposals = new List<D14RelationProposal>();
    }

    [Serializable]
    public sealed class D14CasualtyDecision
    {
        public bool Allowed;
        public string Code = "casualty_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.CasualtyOwner;
        public string SourceEventId;
        public List<D14CasualtyRecord> Casualties = new List<D14CasualtyRecord>();
    }

    [Serializable]
    public sealed class D14PrisonerDecision
    {
        public bool Allowed;
        public string Code = "prisoner_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.PrisonerOwner;
        public string SourceEventId;
        public List<D14PrisonerRecord> Prisoners = new List<D14PrisonerRecord>();
    }

    [Serializable]
    public sealed class D14LootDecision
    {
        public bool Allowed;
        public string Code = "loot_denied";
        public string Message;
        public D14LootStatus Status = D14LootStatus.Denied;
        public string SourceOwnerSystem = D14AftercareIds.LootOwner;
        public string SourceEventId;
        public string LegalSourceEventId;
        public List<D14LootItem> Items = new List<D14LootItem>();
    }

    [Serializable]
    public sealed class D14MedicalDecision
    {
        public bool Allowed;
        public string Code = "medical_denied";
        public string Message;
        public D14TreatmentStatus Status = D14TreatmentStatus.Rejected;
        public string SourceOwnerSystem = D14AftercareIds.MedicalOwner;
        public string SourceEventId;
        public int HealthAfter;
    }

    [Serializable]
    public sealed class D14RescueDecision
    {
        public bool Allowed;
        public string Code = "rescue_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.MedicalOwner;
        public string SourceEventId;
        public List<string> RescuedPrisonerIds = new List<string>();
    }

    [Serializable]
    public sealed class D14InterrogationDecision
    {
        public bool Allowed;
        public string Code = "interrogation_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.LegalOwner;
        public string SourceEventId;
        public string InformationRef;
    }

    [Serializable]
    public sealed class D14FuneralDecision
    {
        public bool Allowed;
        public string Code = "funeral_denied";
        public string Message;
        public D14FuneralStatus Status = D14FuneralStatus.Deferred;
        public string SourceOwnerSystem = D14AftercareIds.FuneralOwner;
        public string SourceEventId;
    }

    [Serializable]
    public sealed class D14RelationProposal
    {
        public string SourceId;
        public string TargetId;
        public D14RelationMetric Metric;
        public float ProposedDelta;
        public string Basis;
    }

    [Serializable]
    public sealed class D14RelationDecision
    {
        public bool Allowed;
        public string Code = "relation_denied";
        public string Message;
        public string SourceOwnerSystem = D14AftercareIds.RelationOwner;
        public string SourceEventId;
        public float AppliedDelta;
    }

    public interface ID14CombatBoardingAuthority
    {
        D14BoardingDecision Evaluate(D14BoardingRequest request);
        D14BoardingDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase);
    }

    public interface ID14CasualtyAuthority
    {
        D14CasualtyDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, D14BoardingDecision boardingDecision);
    }

    public interface ID14PrisonerAuthority
    {
        D14PrisonerDecision Register(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, FactEvent aftercareFact);
    }

    public interface ID14LootAuthority
    {
        D14LootDecision Grant(D14LootRequest request, D14BoardingCase boardingCase, FactEvent aftercareFact);
    }

    public interface ID14MedicalAuthority
    {
        D14MedicalDecision Treat(D14TreatmentRequest request, D14CasualtyRecord casualty, FactEvent aftercareFact);
        D14RescueDecision Rescue(D14RescueRequest request, D14BoardingCase boardingCase, IReadOnlyList<D14PrisonerRecord> prisoners, FactEvent aftercareFact);
    }

    public interface ID14InterrogationAuthority
    {
        D14InterrogationDecision Interrogate(D14InterrogationRequest request, D14PrisonerRecord prisoner, FactEvent aftercareFact);
    }

    public interface ID14FuneralAuthority
    {
        D14FuneralDecision Hold(D14FuneralRequest request, IReadOnlyList<D14CasualtyRecord> casualties, FactEvent aftercareFact);
    }

    public interface ID14RelationAuthority
    {
        D14RelationDecision Apply(D14RelationProposal proposal, FactEvent aftercareFact);
    }

    [Serializable]
    public sealed class D14AftercareResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14AftercareRuntimeSnapshot State;
        public D14BoardingCase BoardingCase;
        public D14LootRecord Loot;
        public D14RescueRecord Rescue;
        public D14TreatmentRecord Treatment;
        public D14InterrogationRecord Interrogation;
        public D14FuneralRecord Funeral;
    }
}
