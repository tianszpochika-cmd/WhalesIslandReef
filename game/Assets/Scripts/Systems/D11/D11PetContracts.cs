// ============================================================
// D11PetContracts —— D11-A06 Pet game-only contract
//
// Pet is not an AnimalState alias. AnimalCareSystem owns this state;
// Crew, UI and AgentCore may only read a clone or submit an intent.
// The complete Pet instance remains local game data per D8-S01.
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public enum D11PetKind
    {
        Cat,
        Dog,
    }

    public enum D11PetLifeState
    {
        Unadopted,
        Adopted,
        Quarantined,
        Lost,
        Sheltered,
        Dead,
    }

    public enum D11PetLocation
    {
        Unknown,
        Harbor,
        ShipCabin,
        ShipDeck,
        Quarantine,
        Shelter,
        Lost,
    }

    public enum D11PetCareState
    {
        Content,
        Hungry,
        Dirty,
        Afraid,
        Sick,
        Recovering,
    }

    [Serializable]
    public sealed class D11PetDefinition
    {
        public string Id;
        public D11PetKind Kind;
        public string Breed;
        public string DisplayName;
        public string FeedItemId;
        public bool CanCatchMice;
        public bool CanWarn;
        public string SourceTag;

        public D11PetDefinition Clone()
        {
            return new D11PetDefinition
            {
                Id = Id,
                Kind = Kind,
                Breed = Breed,
                DisplayName = DisplayName,
                FeedItemId = FeedItemId,
                CanCatchMice = CanCatchMice,
                CanWarn = CanWarn,
                SourceTag = SourceTag,
            };
        }
    }

    /// <summary>唯一由 AnimalCareSystem 写入的 Pet 实例状态。</summary>
    [Serializable]
    public sealed class D11PetState
    {
        public string PetId;
        public string DefinitionId;
        public D11PetKind Kind;
        public string Breed;
        public string SourceRef;
        public string SourceTag;
        public int AdoptionDay;
        public int LastUpdatedDay;
        public int LostSinceDay = -1;
        public long LostSaveRevision;
        public int QuarantineStartedDay = -1;
        public int QuarantineDaysRemaining;
        public int Health = (int)NumericConfiguration.Default.Health.DefaultValue;
        public int Hunger;
        public int Cleanliness = 100;
        public int Fear;
        public int Attachment;
        public D11PetLifeState LifeState = D11PetLifeState.Unadopted;
        public D11PetLocation Location = D11PetLocation.Unknown;
        public D11PetCareState CareState = D11PetCareState.Content;
        public int MouseCaughtCount;
        public int WarningCount;
        public string LastCauseEventId = string.Empty;
        public long LastSaveRevision;

        public bool HasInstance
        {
            get { return !string.IsNullOrEmpty(PetId); }
        }

        public D11PetState Clone()
        {
            return new D11PetState
            {
                PetId = PetId,
                DefinitionId = DefinitionId,
                Kind = Kind,
                Breed = Breed,
                SourceRef = SourceRef,
                SourceTag = SourceTag,
                AdoptionDay = AdoptionDay,
                LastUpdatedDay = LastUpdatedDay,
                LostSinceDay = LostSinceDay,
                LostSaveRevision = LostSaveRevision,
                QuarantineStartedDay = QuarantineStartedDay,
                QuarantineDaysRemaining = QuarantineDaysRemaining,
                Health = Health,
                Hunger = Hunger,
                Cleanliness = Cleanliness,
                Fear = Fear,
                Attachment = Attachment,
                LifeState = LifeState,
                Location = Location,
                CareState = CareState,
                MouseCaughtCount = MouseCaughtCount,
                WarningCount = WarningCount,
                LastCauseEventId = LastCauseEventId,
                LastSaveRevision = LastSaveRevision,
            };
        }
    }

    [Serializable]
    public sealed class D11PetCrewReaction
    {
        public string CrewId;
        public string CrewRole;
        public string PetId;
        public string ReactionId;
        public string ExpressionKey;
        public string CauseEventId;
        public int WorldDay;
        public long SaveRevision;

        public D11PetCrewReaction Clone()
        {
            return new D11PetCrewReaction
            {
                CrewId = CrewId,
                CrewRole = CrewRole,
                PetId = PetId,
                ReactionId = ReactionId,
                ExpressionKey = ExpressionKey,
                CauseEventId = CauseEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    [Serializable]
    public sealed class D11PetActionResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public long SaveRevision;
        public bool WarningRaised;
        public bool MouseCaught;
        public bool SearchFound;
        public D11PetState State;
        public FactEvent Fact;
        public List<D11PetCrewReaction> CrewReactions = new List<D11PetCrewReaction>();

        public static D11PetActionResult Reject(string code, string message, D11PetState state = null)
        {
            return new D11PetActionResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                State = state == null ? null : state.Clone(),
            };
        }
    }

    [Serializable]
    public sealed class D11PetRuntimeSnapshot
    {
        public int SchemaVersion = 1;
        public long NextRevision;
        public D11PetState Pet;
        public List<D11PetCrewReaction> CrewReactions = new List<D11PetCrewReaction>();
        public EventLogSnapshot EventLog;

        public D11PetRuntimeSnapshot Clone()
        {
            var result = new D11PetRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextRevision = NextRevision,
                Pet = Pet == null ? null : Pet.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (CrewReactions != null)
                foreach (var reaction in CrewReactions)
                    if (reaction != null) result.CrewReactions.Add(reaction.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != 1) throw new InvalidOperationException("D11 Pet schema_version must be 1.");
            if (NextRevision < 0) throw new InvalidOperationException("D11 Pet revision must be non-negative.");
            if (Pet == null) return;
            if (Pet.Health < 0 || Pet.Health > 100 || Pet.Hunger < 0 || Pet.Hunger > 100 ||
                Pet.Cleanliness < 0 || Pet.Cleanliness > 100 || Pet.Fear < 0 || Pet.Fear > 100 ||
                Pet.Attachment < 0 || Pet.Attachment > 100)
                throw new InvalidOperationException("D11 Pet bounded state is invalid.");
        }
    }
}
