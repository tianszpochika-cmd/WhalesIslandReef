using System;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var ecology = new D4EcologySystem();
        ecology.SeedHabitat("mangrove", 0, 17);
        ecology.SeedFishSchool("fish_mackerel_school", "nearshore_saint_marys", 0, 17);
        var calm = new EnvironmentSystem().Snapshot;
        var stormEnvironment = new EnvironmentSystem();
        stormEnvironment.ApplyStormEvent(2, "nearshore_saint_marys", 2);
        var storm = stormEnvironment.Snapshot;
        var calmConditions = ecology.ReadConditions(calm, 0);
        var stormConditions = ecology.ReadConditions(storm, 2);
        Check(stormConditions.Turbidity > calmConditions.Turbidity, "storm turbidity");
        Check(stormConditions.CurrentSpeed > calmConditions.CurrentSpeed, "tidal/current relation");

        var preview = ecology.PreviewFishCatch("fish_mackerel_school", "nearshore_saint_marys",
            "item_fresh_mackerel", 3, 0, 17, calm);
        Check(preview.Accepted && ecology.CommitFishCatch(preview), "catch commit");
        int biomassAfterCatch = ecology.Snapshot.FishSchools[0].Biomass;
        var log = new EventLog();
        Check(ecology.ApplyPollution("dock", 0.4f, 1, log, 3, "event_d11_pollution_smoke").Applied,
            "pollution");
        Check(ecology.AdvanceEcology(12, calm, 17, log, 4, "event_d11_advance_smoke").Applied,
            "advance");
        Check(ecology.Snapshot.FishSchools[0].Biomass >= biomassAfterCatch, "fish recovery");
        Check(ecology.HarvestPlant("plant_red_mangrove", 2, 12, 17, log, 5,
            "event_d11_harvest_smoke").Applied, "plant harvest");
        Check(ecology.RestoreHabitat(13, calm, 17, log, 6,
            "event_d11_restore_smoke").Applied, "habitat restore");
        Check(log.Count == 4, "event trace count");
        Check(ecology.ApplyPollution("dock", 0.4f, 1, log, 3,
            "event_d11_pollution_smoke").Duplicate, "duplicate pollution");

        var rareEcology = new D4EcologySystem();
        rareEcology.SeedHabitat("beach", 0, 21);
        var rareLog = new EventLog();
        var factions = new D4FactionSystem();
        var rare = new D11RareAnimalCollectionSystem(rareEcology, rareLog, factions);
        Check(rare.Discover(new D11DiscoveryRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "photo:green_turtle_discovery_01",
        }).Succeeded, "rare discovery");
        Check(rare.Identify(new D11IdentificationRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "field_note:green_turtle_id_01",
        }).Succeeded, "rare identification");
        var observation = rare.Observe(new D11ObservationRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "photo:green_turtle_behavior_01", Weather = calm,
        });
        Check(observation.Succeeded && observation.Evidence.Verified, "rare observation evidence");
        Check(rare.Rescue(new D11RescueRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
            Season = "wet", EvidenceRef = "rescue_report:green_turtle_01",
        }).Succeeded, "rare rescue");
        var transfer = rare.StartLegalTransfer(new D11LegalTransferRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 3,
            Season = "wet", EvidenceRef = "permit:green_turtle_transfer_01",
            CollectorId = "collector_saint_marys_researcher", DestinationRef = "island_saint_marys_research_center",
            DaysInTransit = 2, Seed = 11,
        });
        Check(transfer.Succeeded && !string.IsNullOrEmpty(transfer.TransportId), "legal transfer");
        var saved = new D4RuntimeSnapshot { Ecology = rareEcology.Snapshot, RareAnimals = rare.Snapshot };
        var restoredLog = EventLog.FromSnapshot(rareLog.CaptureSnapshot());
        var restoredEcology = new D4EcologySystem(saved.Ecology);
        var restored = new D11RareAnimalCollectionSystem(restoredEcology, restoredLog, factions, saved.RareAnimals);
        var originalAdvance = rare.AdvanceTransport(new D11TransportAdvanceRequest
        {
            TransportId = transfer.TransportId, WorldDay = 4, Weather = calm,
        });
        var restoredAdvance = restored.AdvanceTransport(new D11TransportAdvanceRequest
        {
            TransportId = transfer.TransportId, WorldDay = 4, Weather = calm,
        });
        Check(originalAdvance.Succeeded && restoredAdvance.Succeeded, "transport save/restore");
        Check(originalAdvance.Code == restoredAdvance.Code &&
            rare.Snapshot.Transports[0].Status == restored.Snapshot.Transports[0].Status,
            "transport deterministic restore");
        if (rare.Snapshot.Transports[0].Status == D11TransportStatus.InTransit)
            Check(rare.AdvanceTransport(new D11TransportAdvanceRequest
            {
                TransportId = transfer.TransportId, WorldDay = 5, Weather = calm,
            }).Succeeded, "transport arrival");
        Check(rare.Snapshot.Transports[0].Status == D11TransportStatus.Arrived, "transport arrived");
        Check(rare.Collect(new D11CollectionRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys_research_center",
            WorldDay = 5, Season = "wet", EvidenceRef = "collection_receipt:green_turtle_01",
            CollectorId = "collector_saint_marys_researcher", TransportId = transfer.TransportId,
        }).Succeeded, "legal collection");

        var releaseEcology = new D4EcologySystem();
        releaseEcology.SeedHabitat("beach", 0, 21);
        var release = new D11RareAnimalCollectionSystem(releaseEcology, new EventLog(), new D4FactionSystem());
        PrepareRare(release);
        Check(release.Release(new D11ReleaseRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
            Season = "wet", EvidenceRef = "release_report:green_turtle_01",
            ProtectedAreaId = "protected_area_saint_marys_coast",
        }).Succeeded, "protected release");

        var illegalEcology = new D4EcologySystem();
        illegalEcology.SeedHabitat("beach", 0, 21);
        var illegalFactions = new D4FactionSystem();
        var illegal = new D11RareAnimalCollectionSystem(illegalEcology, new EventLog(), illegalFactions);
        var illegalResult = illegal.IllegalTrade(new D11IllegalTradeRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
            Season = "wet", EvidenceRef = "law_report:green_turtle_illegal_01", ControllingFactionId = "england",
        });
        Check(illegalResult.Succeeded && illegalResult.LegalConsequence.WantedDelta > 0 &&
            illegalFactions.GetReputation("england") < 0, "illegal trade legal consequence");

        Console.WriteLine("D11-A01 smoke PASS: habitat/plant/animal/fish/pressure/weather/pollution/recovery/events");
        Console.WriteLine("D11-A05 smoke PASS: discovery/identification/observation/rescue/legal-transfer/collection/release/illegal-trade/save-restore");
    }

    private static void PrepareRare(D11RareAnimalCollectionSystem rare)
    {
        Check(rare.Discover(new D11DiscoveryRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "photo:release_discovery_01",
        }).Succeeded, "release discovery");
        Check(rare.Identify(new D11IdentificationRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "field_note:release_id_01",
        }).Succeeded, "release identification");
        Check(rare.Observe(new D11ObservationRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
            Season = "wet", EvidenceRef = "photo:release_observation_01", Weather = D9WeatherSeaSnapshot.CreateDefault(),
        }).Succeeded, "release observation");
        Check(rare.Rescue(new D11RescueRequest
        {
            SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
            Season = "wet", EvidenceRef = "rescue_report:release_01",
        }).Succeeded, "release rescue");
    }
}
