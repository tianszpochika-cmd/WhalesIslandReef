using System.Collections.Generic;
using Whalebone.Systems.D4;

namespace Whalebone.Systems.D11
{
    internal static class D11RareAnimalCatalog
    {
        internal const string CollectorId = "collector_saint_marys_researcher";
        internal const string ProtectedAreaId = "protected_area_saint_marys_coast";

        internal static readonly IReadOnlyList<D11RareAnimalDefinition> All =
            new List<D11RareAnimalDefinition>
            {
                // This is the single formal representative; it maps to the existing EcologySystem sea_turtle state.
                new D11RareAnimalDefinition
                {
                    SpeciesId = "animal_green_turtle",
                    RuntimeAnimalId = "sea_turtle",
                    HabitatId = "nearshore",
                    RegionId = "island_saint_marys",
                    Season = "wet",
                    BehaviorNote = "breeding_hint",
                    RarityReason = "protected coastal nesting",
                    CapturePolicy = "protected",
                    TransportNeed = "shallow_saltwater_crate",
                    BuyerType = "research",
                    ReleaseRegion = "island_saint_marys",
                    SourceNote = "项目自有代表物种定义；只映射已存在的 EcologySystem 动物状态。",
                    SourceRef = "content:animal_green_turtle",
                    TestId = "D11-A05-ANIMAL-001",
                    Protected = true,
                    ObservationValue = 10,
                    CollectionValue = 30,
                },
            };

        internal static D11RareAnimalDefinition Find(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId)) return null;
            foreach (var definition in All)
                if (definition != null && definition.SpeciesId == speciesId) return definition;
            return null;
        }

        internal static D11RareAnimalDefinition FindByAnimal(string animalId)
        {
            if (string.IsNullOrEmpty(animalId)) return null;
            foreach (var definition in All)
                if (definition != null && definition.RuntimeAnimalId == animalId) return definition;
            return null;
        }
    }
}
