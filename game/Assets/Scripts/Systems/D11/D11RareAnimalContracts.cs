using System;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;

namespace Whalebone.Systems.D11
{
    [Serializable]
    public class D11RareAnimalEvidenceInput
    {
        public string SpeciesId;
        public string AnimalId;
        public string LocationRef;
        public int WorldDay;
        public string Season;
        public string EvidenceRef;
        public string SourceRef;
        public string ActorRef = "player";
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D11DiscoveryRequest : D11RareAnimalEvidenceInput { }
    [Serializable]
    public sealed class D11IdentificationRequest : D11RareAnimalEvidenceInput { }

    [Serializable]
    public sealed class D11ObservationRequest : D11RareAnimalEvidenceInput
    {
        public D9WeatherSeaSnapshot Weather;
    }

    [Serializable]
    public sealed class D11RescueRequest : D11RareAnimalEvidenceInput { }

    [Serializable]
    public sealed class D11LegalTransferRequest : D11RareAnimalEvidenceInput
    {
        public string CollectorId;
        public string DestinationRef;
        public int DaysInTransit = 2;
        public int Seed;
    }

    [Serializable]
    public sealed class D11TransportAdvanceRequest
    {
        public string TransportId;
        public int WorldDay;
        public D9WeatherSeaSnapshot Weather;
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D11ReleaseRequest : D11RareAnimalEvidenceInput
    {
        public string ProtectedAreaId;
        public string TransportId;
    }

    [Serializable]
    public sealed class D11CollectionRequest : D11RareAnimalEvidenceInput
    {
        public string CollectorId;
        public string TransportId;
    }

    [Serializable]
    public sealed class D11IllegalTradeRequest : D11RareAnimalEvidenceInput
    {
        public string ControllingFactionId = "england";
    }

    [Serializable]
    public sealed class D11RareAnimalResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool Duplicate;
        public bool Mortality;
        public string Code;
        public string Message;
        public string SpeciesId;
        public string AnimalId;
        public string EvidenceId;
        public string TransportId;
        public string EventId;
        public int WorldDay;
        public float MortalityRisk;
        public D11AnimalLifecycleState Lifecycle;
        public D11AnimalEvidenceRecord Evidence;
        public D11CollectionBookEntry BookEntry;
        public FactionActionResult LegalConsequence;

        public D11RareAnimalResult Clone()
        {
            return new D11RareAnimalResult
            {
                Succeeded = Succeeded,
                Applied = Applied,
                Duplicate = Duplicate,
                Mortality = Mortality,
                Code = Code,
                Message = Message,
                SpeciesId = SpeciesId,
                AnimalId = AnimalId,
                EvidenceId = EvidenceId,
                TransportId = TransportId,
                EventId = EventId,
                WorldDay = WorldDay,
                MortalityRisk = MortalityRisk,
                Lifecycle = Lifecycle,
                Evidence = Evidence == null ? null : Evidence.Clone(),
                BookEntry = BookEntry == null ? null : BookEntry.Clone(),
                LegalConsequence = LegalConsequence == null ? null : new FactionActionResult
                {
                    Accepted = LegalConsequence.Accepted,
                    ReputationDelta = LegalConsequence.ReputationDelta,
                    WantedDelta = LegalConsequence.WantedDelta,
                    Message = LegalConsequence.Message,
                },
            };
        }
    }
}
