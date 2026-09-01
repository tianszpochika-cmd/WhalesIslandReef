// D20-L32 纯逻辑 content_id/resource_key 集成测试。
// 该测试不读取 Unity 资产、候选路径或二进制资源，只验证公开逻辑目录。

using System;
using Whalebone.Systems.D10;
using Whalebone.Systems.D20;

namespace WhalesIslandReef.Tests.D20
{
    public static class D20ContentIdIntegrationTests
    {
        /// <summary>由离线纯 C# runner 调用；返回实际断言数。</summary>
        public static int Run()
        {
            int assertions = 0;

            var character = D20CharacterIdentityContracts.CreateDefault(
                "player_l32", D20CharacterKind.Player,
                "character_captain_001", "origin:character:l32");
            character.Validate();
            Check(ref assertions, character.CharacterId == "player_l32",
                "character_id must remain stable");
            Check(ref assertions, character.ImmutableFields.ContentId == "character_captain_001",
                "character content_id must be retained");
            Check(ref assertions, D20CharacterIdentityContracts.IsStableCharacterId(
                character.ImmutableFields.IdentityKey), "character identity key must be stable");

            string canonicalNpcId;
            Check(ref assertions, D20NpcIdentityContracts.TryResolveNpcId(
                "npc_harbor_clerk", out canonicalNpcId),
                "deprecated NPC id must resolve");
            Check(ref assertions, D20NpcIdentityContracts.IsDeprecatedNpcId(
                "npc_harbor_clerk"), "deprecated NPC mapping must be declared");
            var migratedNpc = D20NpcIdentityContracts.Migrate(
                "npc_harbor_clerk", "role:harbor_clerk", "faction:port",
                D20CharacterAvailability.Available, "character_npc_harbor_clerk");
            migratedNpc.Validate();
            Check(ref assertions, migratedNpc.NpcId == canonicalNpcId,
                "deprecated NPC mapping must use canonical id");
            Check(ref assertions, migratedNpc.MigrationStatus ==
                D20NpcIdentityMigrationStatus.DeprecatedAlias,
                "deprecated NPC mapping status must be explicit");
            Check(ref assertions, migratedNpc.ContentId == "character_npc_harbor_clerk",
                "NPC content_id must be retained");

            var unknownNpc = D20NpcIdentityContracts.Migrate(
                "npc_unknown_l32", "role:merchant", "faction:port",
                D20CharacterAvailability.Available, "character_npc_unknown");
            unknownNpc.Validate();
            Check(ref assertions, unknownNpc.NpcId == "npc_unknown_l32",
                "unknown stable NPC id must be preserved");
            Check(ref assertions, unknownNpc.MigrationStatus ==
                D20NpcIdentityMigrationStatus.UnknownPreserved,
                "unknown stable NPC status must be explicit");

            Check(ref assertions, D20AnimalSpeciesCatalog.ValidateCatalog().Count == 0,
                "animal content catalog must validate without resources");
            foreach (var species in D20AnimalSpeciesCatalog.All)
            {
                Check(ref assertions, D20AnimalIdentityContracts.IsStableAnimalContentId(
                    species.ContentId), "animal content_id must be stable: " + species.ContentId);
                Check(ref assertions, !species.ContentId.Contains("/"),
                    "animal content_id must not be a path: " + species.ContentId);
                Check(ref assertions, species.TryValidate().Valid,
                    "animal species definition must validate: " + species.ContentId);
            }
            var animal = D20AnimalIdentityContracts.CreateDefault(
                "animal_l32_001", D20AnimalSpeciesCatalog.AgoutiForestContentId,
                D20AnimalSpeciesCatalog.AgoutiForestContentId,
                D20AnimalClassification.Terrestrial);
            animal.Validate();
            Check(ref assertions, animal.ContentId == D20AnimalSpeciesCatalog.AgoutiForestContentId,
                "animal identity content_id must match the catalog");

            Check(ref assertions, D20AppearanceSelectionRules.ValidateCatalog().Count == 0,
                "appearance resource catalog must validate without asset lookup");
            foreach (var appearance in D10AppearanceCatalog.All)
            {
                Check(ref assertions, D20AppearanceSelectionRules.IsStableContentId(
                    appearance.Id, "outfit_"), "appearance content_id must be stable: " + appearance.Id);
                Check(ref assertions, D20AppearanceSelectionRules.IsStableResourceKey(
                    appearance.ResourceKey), "appearance resource_key must be stable: " + appearance.Id);
                Check(ref assertions, !appearance.ResourceKey.Contains("candidate") &&
                    !appearance.ResourceKey.Contains("Assets/"),
                    "appearance resource_key must not depend on candidate paths: " + appearance.Id);
            }

            return assertions;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
