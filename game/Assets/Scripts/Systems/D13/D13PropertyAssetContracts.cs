// D13-A01 产权与资产交易契约。
// PropertySystem 只写产权状态与交易事实投影；估值/金额由 Economy authority，
// 法律归属/争议由 FactionLegal authority 裁决。请求对象不等于结果。

using System;
using System.Collections.Generic;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13PropertyIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "PropertySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string PlayerRef = "player";
        public const string AuthorityOwner = "harbor_authority";
        public const string PropertySurveyRecoveryRoute = "route_property_title_review";
        public const string PropertyValuationRecoveryRoute = "route_property_valuation_review";
        public const string PropertyTransactionRecoveryRoute = "route_property_transaction_review";
        public const string PropertyDisputeRecoveryRoute = "route_property_legal_appeal";
    }

    public enum D13PropertyKind
    {
        IslandEstate,
        LandParcel,
        House,
        Warehouse,
        Workshop,
        Tavern,
        Shipyard,
        Farmland,
        Fishery,
    }

    public enum D13OwnershipKind
    {
        Unassigned,
        PrivateOwnership,
        Leasehold,
        CoGovernance,
        Mortgaged,
        Disputed,
        Transferred,
        Inherited,
    }

    public enum D13PropertyTransactionKind
    {
        Purchase,
        Lease,
        CoGovernance,
        Mortgage,
        Sale,
        Transfer,
        Inheritance,
        OpenDispute,
        ResolveDispute,
    }

    public enum D13PropertyLegalStatus
    {
        Unknown,
        Registered,
        Leaseable,
        Protected,
        Disputed,
        Seized,
    }

    public enum D13PropertyDisputeStatus
    {
        None,
        Open,
        ResolvedForOwner,
        ResolvedForClaimant,
        Escalated,
    }

    [Serializable]
    public sealed class D13PropertyDefinition
    {
        public string Id;
        public string DisplayNameKey;
        public D13PropertyKind Kind;
        public string IslandId;
        public string LocationRef;
        public string ControllingFactionId;
        public string ResidentCommunityRef;
        public string DefaultOwnerRef = D13PropertyIds.AuthorityOwner;
        public string LegalRegistryRef;
        public bool RequiresCommunityAgreement;
        public bool ProtectedArea;
        public int AreaUnits = 1;
        public string SourceNote;
        public string HistoricalTag = "inferred";
        public string Phase = "D13";
        public string TestId;

        public D13PropertyDefinition Clone()
        {
            return new D13PropertyDefinition
            {
                Id = Id,
                DisplayNameKey = DisplayNameKey,
                Kind = Kind,
                IslandId = IslandId,
                LocationRef = LocationRef,
                ControllingFactionId = ControllingFactionId,
                ResidentCommunityRef = ResidentCommunityRef,
                DefaultOwnerRef = DefaultOwnerRef,
                LegalRegistryRef = LegalRegistryRef,
                RequiresCommunityAgreement = RequiresCommunityAgreement,
                ProtectedArea = ProtectedArea,
                AreaUnits = AreaUnits,
                SourceNote = SourceNote,
                HistoricalTag = HistoricalTag,
                Phase = Phase,
                TestId = TestId,
            };
        }

        public void Validate()
        {
            Require(Id, "property definition id");
            Require(DisplayNameKey, "property definition display_name_key");
            Require(IslandId, "property definition island_id");
            Require(LocationRef, "property definition location_ref");
            Require(ControllingFactionId, "property definition controlling_faction_id");
            Require(DefaultOwnerRef, "property definition default_owner_ref");
            Require(LegalRegistryRef, "property definition legal_registry_ref");
            Require(SourceNote, "property definition source_note");
            Require(HistoricalTag, "property definition historical_tag");
            Require(Phase, "property definition phase");
            Require(TestId, "property definition test_id");
            if (!Enum.IsDefined(typeof(D13PropertyKind), Kind))
                throw new InvalidOperationException("Property kind is invalid: " + Id);
            if (AreaUnits <= 0) throw new InvalidOperationException("Property area must be positive: " + Id);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13ValuationQuote
    {
        public string PropertyId;
        public int Amount;
        public string Currency = "coin";
        public string ValuationBasis;
        public string SourceOwnerSystem = D13PropertyIds.EconomyOwner;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> Reasons = new List<string>();

        public D13ValuationQuote Clone()
        {
            var result = new D13ValuationQuote
            {
                PropertyId = PropertyId,
                Amount = Amount,
                Currency = Currency,
                ValuationBasis = ValuationBasis,
                SourceOwnerSystem = SourceOwnerSystem,
                SourceEventId = SourceEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (Reasons != null) result.Reasons.AddRange(Reasons);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PropertyId)) throw new InvalidOperationException("Valuation property_id is required.");
            if (Amount < 0) throw new InvalidOperationException("Valuation amount must be non-negative.");
            if (string.IsNullOrWhiteSpace(Currency)) throw new InvalidOperationException("Valuation currency is required.");
            if (string.IsNullOrWhiteSpace(ValuationBasis)) throw new InvalidOperationException("Valuation basis is required.");
            if (SourceOwnerSystem != D13PropertyIds.EconomyOwner)
                throw new InvalidOperationException("Property valuation must come from EconomySystem.");
            if (string.IsNullOrWhiteSpace(SourceEventId)) throw new InvalidOperationException("Valuation source_event_id is required.");
            if (WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Valuation time is invalid.");
        }
    }

    [Serializable]
    public sealed class D13PropertyState
    {
        public string PropertyId;
        public D13OwnershipKind Ownership = D13OwnershipKind.PrivateOwnership;
        public string OwnerRef;
        public string ControllerRef;
        public string LeaseHolderRef;
        public int LeaseEndDay;
        public List<string> CoGovernanceRefs = new List<string>();
        public string MortgageRef;
        public D13PropertyLegalStatus LegalStatus = D13PropertyLegalStatus.Unknown;
        public string LegalSourceEventId;
        public D13PropertyDisputeStatus DisputeStatus = D13PropertyDisputeStatus.None;
        public string DisputeId;
        public int LatestValuationAmount;
        public string LatestValuationSourceEventId;
        public int LatestValuationDay;
        public long Revision;
        public int LastUpdatedDay;
        public string LastEventId;

        public D13PropertyState Clone()
        {
            var result = new D13PropertyState
            {
                PropertyId = PropertyId,
                Ownership = Ownership,
                OwnerRef = OwnerRef,
                ControllerRef = ControllerRef,
                LeaseHolderRef = LeaseHolderRef,
                LeaseEndDay = LeaseEndDay,
                MortgageRef = MortgageRef,
                LegalStatus = LegalStatus,
                LegalSourceEventId = LegalSourceEventId,
                DisputeStatus = DisputeStatus,
                DisputeId = DisputeId,
                LatestValuationAmount = LatestValuationAmount,
                LatestValuationSourceEventId = LatestValuationSourceEventId,
                LatestValuationDay = LatestValuationDay,
                Revision = Revision,
                LastUpdatedDay = LastUpdatedDay,
                LastEventId = LastEventId,
            };
            if (CoGovernanceRefs != null) result.CoGovernanceRefs.AddRange(CoGovernanceRefs);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PropertyId)) throw new InvalidOperationException("Property state property_id is required.");
            if (!Enum.IsDefined(typeof(D13OwnershipKind), Ownership)) throw new InvalidOperationException("Property ownership is invalid.");
            if (!Enum.IsDefined(typeof(D13PropertyLegalStatus), LegalStatus)) throw new InvalidOperationException("Property legal status is invalid.");
            if (!Enum.IsDefined(typeof(D13PropertyDisputeStatus), DisputeStatus)) throw new InvalidOperationException("Property dispute status is invalid.");
            if (Revision < 0 || LastUpdatedDay < 0 || LatestValuationDay < 0)
                throw new InvalidOperationException("Property state time is invalid.");
            if (CoGovernanceRefs == null) throw new InvalidOperationException("Property governance refs are required.");
        }
    }

    [Serializable]
    public sealed class D13PropertySurveyRecord
    {
        public string SurveyId;
        public string PropertyId;
        public string InvestigatorRef;
        public bool Approved;
        public string DecisionCode;
        public string TitleHolderRef;
        public D13PropertyLegalStatus LegalStatus;
        public string SourceAuthority = D13PropertyIds.LegalOwner;
        public string SourceEventId;
        public List<string> EvidenceRefs = new List<string>();
        public int WorldDay;
        public long SaveRevision;

        public D13PropertySurveyRecord Clone()
        {
            var result = new D13PropertySurveyRecord
            {
                SurveyId = SurveyId,
                PropertyId = PropertyId,
                InvestigatorRef = InvestigatorRef,
                Approved = Approved,
                DecisionCode = DecisionCode,
                TitleHolderRef = TitleHolderRef,
                LegalStatus = LegalStatus,
                SourceAuthority = SourceAuthority,
                SourceEventId = SourceEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (EvidenceRefs != null) result.EvidenceRefs.AddRange(EvidenceRefs);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SurveyId) || string.IsNullOrWhiteSpace(PropertyId))
                throw new InvalidOperationException("Property survey ids are required.");
            if (string.IsNullOrWhiteSpace(InvestigatorRef) || string.IsNullOrWhiteSpace(DecisionCode))
                throw new InvalidOperationException("Property survey investigator and decision are required.");
            if (SourceAuthority != D13PropertyIds.LegalOwner || string.IsNullOrWhiteSpace(SourceEventId))
                throw new InvalidOperationException("Property survey must cite the legal authority source.");
            if (!Enum.IsDefined(typeof(D13PropertyLegalStatus), LegalStatus))
                throw new InvalidOperationException("Property survey legal status is invalid.");
            if (WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Property survey time is invalid.");
            if (EvidenceRefs == null) throw new InvalidOperationException("Property survey evidence refs are required.");
        }
    }

    [Serializable]
    public sealed class D13PropertyReadModel
    {
        public D13PropertyDefinition Definition;
        public D13PropertyState State;
        public D13PropertySurveyRecord LatestSurvey;
        public D13ValuationQuote LatestValuation;

        public D13PropertyReadModel Clone()
        {
            return new D13PropertyReadModel
            {
                Definition = Definition == null ? null : Definition.Clone(),
                State = State == null ? null : State.Clone(),
                LatestSurvey = LatestSurvey == null ? null : LatestSurvey.Clone(),
                LatestValuation = LatestValuation == null ? null : LatestValuation.Clone(),
            };
        }
    }

    [Serializable]
    public sealed class D13PropertyRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13PropertyIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13PropertyState> Properties = new List<D13PropertyState>();
        public List<D13PropertySurveyRecord> Surveys = new List<D13PropertySurveyRecord>();
        public List<D13ValuationQuote> Valuations = new List<D13ValuationQuote>();
        public List<string> AppliedEventKeys = new List<string>();

        public D13PropertyRuntimeSnapshot Clone()
        {
            var result = new D13PropertyRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Properties != null) foreach (var item in Properties) if (item != null) result.Properties.Add(item.Clone());
            if (Surveys != null) foreach (var item in Surveys) if (item != null) result.Surveys.Add(item.Clone());
            if (Valuations != null) foreach (var item in Valuations) if (item != null) result.Valuations.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D13 property schema_version mismatch.");
            if (Revision < 0) throw new InvalidOperationException("D13 property revision must be non-negative.");
            if (Properties == null || Surveys == null || Valuations == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D13 property snapshot collections are required.");
            var propertyIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in Properties)
            {
                if (property == null) throw new InvalidOperationException("D13 property snapshot contains null state.");
                property.Validate();
                if (!propertyIds.Add(property.PropertyId)) throw new InvalidOperationException("D13 property duplicated: " + property.PropertyId);
            }
            var surveyIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var survey in Surveys)
            {
                if (survey == null) throw new InvalidOperationException("D13 property snapshot contains null survey.");
                survey.Validate();
                if (!surveyIds.Add(survey.SurveyId)) throw new InvalidOperationException("D13 survey duplicated: " + survey.SurveyId);
            }
            var valuationKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var quote in Valuations)
            {
                if (quote == null) throw new InvalidOperationException("D13 property snapshot contains null valuation.");
                quote.Validate();
                string key = quote.PropertyId + "\u001f" + quote.SaveRevision;
                if (!valuationKeys.Add(key)) throw new InvalidOperationException("D13 valuation duplicated: " + key);
            }
        }
    }

    [Serializable]
    public sealed class D13PropertySurveyRequest
    {
        public string PropertyId;
        public string InvestigatorRef = D13PropertyIds.PlayerRef;
        public string OperationId;
        public List<string> EvidenceRefs = new List<string>();
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13PropertyValuationRequest
    {
        public string PropertyId;
        public string ApplicantRef = D13PropertyIds.PlayerRef;
        public string OperationId;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13PropertyTransactionRequest
    {
        public string PropertyId;
        public D13PropertyTransactionKind Kind;
        public string ApplicantRef = D13PropertyIds.PlayerRef;
        public string CounterpartyRef;
        public string DisputeId;
        public int LeaseEndDay;
        public string LegalInstrumentRef;
        public string OperationId;
        public List<string> CoGovernanceRefs = new List<string>();
        public List<string> EvidenceRefs = new List<string>();
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
        public int OfferedAmount;
    }

    [Serializable]
    public sealed class D13LegalDecision
    {
        public bool Allowed;
        public string Code = "legal_denied";
        public string Message;
        public D13PropertyLegalStatus LegalStatus = D13PropertyLegalStatus.Unknown;
        public D13PropertyDisputeStatus DisputeStatus = D13PropertyDisputeStatus.None;
        public string TitleHolderRef;
        public string ResolvedOwnerRef;
        public string SourceOwnerSystem = D13PropertyIds.LegalOwner;
        public string SourceEventId;
        public List<string> EvidenceRefs = new List<string>();
    }

    [Serializable]
    public sealed class D13EconomyValuationDecision
    {
        public bool Approved;
        public string Code = "valuation_denied";
        public string Message;
        public D13ValuationQuote Quote;
    }

    [Serializable]
    public sealed class D13EconomyTransactionDecision
    {
        public bool Approved;
        public string Code = "economy_denied";
        public string Message;
        public int Amount;
        public string LedgerRef;
        public string SourceOwnerSystem = D13PropertyIds.EconomyOwner;
        public string SourceEventId;
    }

    public interface ID13PropertyLegalAuthority
    {
        D13LegalDecision Investigate(D13PropertyDefinition definition, D13PropertyState state,
            D13PropertySurveyRequest request);

        D13LegalDecision Authorize(D13PropertyDefinition definition, D13PropertyState state,
            D13PropertyTransactionRequest request, D13PropertySurveyRecord survey);
    }

    public interface ID13PropertyEconomyAuthority
    {
        D13EconomyValuationDecision Quote(D13PropertyDefinition definition, D13PropertyState state,
            D13PropertyValuationRequest request);

        D13EconomyTransactionDecision Approve(D13PropertyDefinition definition, D13PropertyState state,
            D13PropertyTransactionRequest request, D13ValuationQuote valuation);
    }

    [Serializable]
    public sealed class D13PropertyResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public D13PropertyTransactionKind TransactionKind;
        public D13PropertyState Property;
        public D13PropertySurveyRecord Survey;
        public D13ValuationQuote Valuation;
        public FactEvent Fact;
        public string LegalSourceEventId;
        public string EconomySourceEventId;
        public int Amount;
        public string RecoveryRouteId;
    }

    public static class D13PropertyCatalog
    {
        public static readonly IReadOnlyList<D13PropertyDefinition> All = CreateVerticalSlice();

        public static D13PropertyDefinition Find(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId)) return null;
            foreach (var item in All) if (item.Id == propertyId) return item.Clone();
            return null;
        }

        public static IReadOnlyList<D13PropertyDefinition> CreateVerticalSlice()
        {
            return new List<D13PropertyDefinition>
            {
                Property("property_estate_saint_marys", D13PropertyKind.IslandEstate, "island_saint_marys", "santa_margarita_estate", "spain", "crown_spain", false, false, 18, "岛屿经营权样例；购买不等于抹除居民与势力法律。", "D13-A01-PROPERTY-001"),
                Property("property_parcel_green_cay_mangrove_edge", D13PropertyKind.LandParcel, "island_green_cay", "green_cay_mangrove_edge", "tribes", "community_green_cay", true, true, 6, "红树林边缘地块样例；必须尊重保护区和共同治理协议。", "D13-A01-PROPERTY-002"),
                Property("property_house_santa_marys_quayside", D13PropertyKind.House, "island_saint_marys", "santa_margarita_quay_house", "spain", "crown_spain", false, false, 2, "港区住宅样例；居住者与登记权利由法律调查返回。", "D13-A01-PROPERTY-003"),
                Property("property_warehouse_santa_marys_north_store", D13PropertyKind.Warehouse, "island_saint_marys", "santa_margarita_north_store", "spain", "merchant_guild_saint_marys", false, false, 4, "港区仓库样例；货物不属于产权状态，仍由 Inventory/Cargo 写入。", "D13-A01-PROPERTY-004"),
                Property("property_workshop_driftwood_ropewalk", D13PropertyKind.Workshop, "island_driftwood_isle", "driftwood_ropewalk", "smugglers", "dockworkers_driftwood", true, false, 5, "漂木岛绳作工坊样例；劳动与居民协议由后续治理系统裁决。", "D13-A01-PROPERTY-005"),
                Property("property_tavern_santa_marys_anchor", D13PropertyKind.Tavern, "island_saint_marys", "santa_margarita_anchor_tavern", "spain", "tavern_family_anchor", true, false, 3, "酒馆经营权样例；不复制 D10 酒馆社交状态。", "D13-A01-PROPERTY-006"),
                Property("property_shipyard_santa_marys_dock", D13PropertyKind.Shipyard, "island_saint_marys", "santa_margarita_dockyard", "spain", "crown_spain", false, false, 8, "船坞产权样例；船型服务仍由 Building/Shipyard owner 裁决。", "D13-A01-PROPERTY-007"),
                Property("property_farmland_green_cay_cassava", D13PropertyKind.Farmland, "island_green_cay", "green_cay_cassava_field", "tribes", "community_green_cay", true, false, 7, "农地样例；作物与生态状态由后续农业/Ecology owner 写入。", "D13-A01-PROPERTY-008"),
                Property("property_fishery_green_cay_shallow_reef", D13PropertyKind.Fishery, "island_green_cay", "green_cay_shallow_reef", "tribes", "community_green_cay", true, true, 9, "浅礁渔场样例；鱼群数量与压力由 Ecology owner 写入。", "D13-A01-PROPERTY-009"),
            };
        }

        private static D13PropertyDefinition Property(string id, D13PropertyKind kind, string islandId,
            string locationRef, string factionId, string defaultOwner, bool communityAgreement,
            bool protectedArea, int area, string sourceNote, string testId)
        {
            return new D13PropertyDefinition
            {
                Id = id,
                DisplayNameKey = "content.property." + id + ".name",
                Kind = kind,
                IslandId = islandId,
                LocationRef = locationRef,
                ControllingFactionId = factionId,
                ResidentCommunityRef = communityAgreement ? "community_green_cay" : string.Empty,
                DefaultOwnerRef = defaultOwner,
                LegalRegistryRef = "title_registry_" + id,
                RequiresCommunityAgreement = communityAgreement,
                ProtectedArea = protectedArea,
                AreaUnits = area,
                SourceNote = sourceNote,
                HistoricalTag = "inferred",
                Phase = "D13",
                TestId = testId,
            };
        }
    }
}
