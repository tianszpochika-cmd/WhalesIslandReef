// ============================================================
// D11PetCatalog —— D11-A06 immutable pet definitions
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D11
{
    public static class D11PetIds
    {
        public const string CatDefinitionId = "pet_cabin_cat";
        public const string DogDefinitionId = "pet_harbor_dog";
        public const string FeedItemId = "item_pet_feed";
    }

    public static class D11PetCatalog
    {
        private static readonly D11PetDefinition Cat = new D11PetDefinition
        {
            Id = D11PetIds.CatDefinitionId,
            Kind = D11PetKind.Cat,
            Breed = "Caribbean short-haired cat",
            DisplayName = "船舱猫",
            FeedItemId = D11PetIds.FeedItemId,
            CanCatchMice = true,
            CanWarn = true,
            SourceTag = "harbor_shelter",
        };

        private static readonly D11PetDefinition Dog = new D11PetDefinition
        {
            Id = D11PetIds.DogDefinitionId,
            Kind = D11PetKind.Dog,
            Breed = "Caribbean harbor dog",
            DisplayName = "港口犬",
            FeedItemId = D11PetIds.FeedItemId,
            CanCatchMice = false,
            CanWarn = true,
            SourceTag = "harbor_shelter",
        };

        public static IReadOnlyList<D11PetDefinition> All
        {
            get { return new[] { Cat, Dog }; }
        }

        public static D11PetDefinition Find(string id)
        {
            if (string.Equals(Cat.Id, id, StringComparison.Ordinal)) return Cat;
            if (string.Equals(Dog.Id, id, StringComparison.Ordinal)) return Dog;
            return null;
        }
    }
}
