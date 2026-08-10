using System;
using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Agent.V3
{
    /// <summary>
    /// D8-A01 纯 C# 自测：空实体、完整实体、稳定 ID、单位类型、旧字段和未知枚举。
    /// 不依赖 Unity、网络、LLM 或第三方测试框架。
    /// </summary>
    public static class V3EntityModelTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("v3 空实体默认值与 16 实体构造", TestEmptyEntities);
            failed += Check("v3 稳定 ID 可重复且前缀固定", TestStableIds);
            failed += Check("金额/重量/体积/时间/关系/百分比类型边界", TestTypedValues);
            failed += Check("v3 完整实体序列化往返", TestCompleteEntityRoundTrips);
            failed += Check("D7 旧字段和未知新增字段兼容", TestLegacyFieldsCompatible);
            failed += Check("未知枚举严格拒绝并返回错误", TestUnknownEnumRejected);
            failed += Check("存档级版本默认值", TestSaveMetadataDefaults);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[EX] " + name + " : " + ex.Message);
                return 1;
            }
        }

        private static bool TestEmptyEntities()
        {
            var player = new PlayerEntity();
            var ship = new ShipEntity();
            var crew = new CrewEntity();
            var relation = new RelationEntity();
            var island = new IslandEntity();
            var parcel = new LandParcelEntity();
            var building = new BuildingEntity();
            var item = new ItemEntity();
            var animal = new AnimalEntity();
            var fish = new FishSchoolEntity();
            var pet = new PetEntity();
            var finance = new FinanceAccountEntity();
            var quest = new QuestEntity();
            var worldEvent = new WorldEventEntity();
            var reward = new RewardEntity();
            var outfit = new AppearanceLoadoutEntity();

            return player.SchemaVersion == 1
                && ship.LegacyD7 != null
                && crew.LegacyD7 != null
                && relation.CauseEventIds.Count == 0
                && island.EcologyMetrics.Count == 0
                && parcel.Purpose == ParcelPurpose.Unknown
                && building.Inventory.Count == 0
                && item.Tags.Count == 0
                && animal.Status == AnimalStatus.Unknown
                && fish.Migration == FishMigration.Unknown
                && pet.Location != null
                && finance.DefaultState == FinanceDefaultState.Unknown
                && quest.Status == QuestStatus.Unknown
                && worldEvent.OutcomeStatus == EventOutcomeStatus.Unknown
                && reward.Status == RewardStatus.Unknown
                && outfit.Accessories.Count == 0;
        }

        private static bool TestStableIds()
        {
            V3EntityKind[] kinds =
            {
                V3EntityKind.Player, V3EntityKind.Ship, V3EntityKind.Crew,
                V3EntityKind.Relation, V3EntityKind.Island, V3EntityKind.LandParcel,
                V3EntityKind.Building, V3EntityKind.Item, V3EntityKind.Animal,
                V3EntityKind.FishSchool, V3EntityKind.Pet, V3EntityKind.FinanceAccount,
                V3EntityKind.Quest, V3EntityKind.WorldEvent, V3EntityKind.Reward,
                V3EntityKind.AppearanceLoadout
            };

            foreach (V3EntityKind kind in kinds)
            {
                string first = V3StableIds.Create(kind, "Captain One/Legacy");
                string second = V3StableIds.Create(kind, "Captain One/Legacy");
                if (first != second || !V3StableIds.IsValid(kind, first)) return false;
            }

            return V3StableIds.Create(V3EntityKind.Unknown, "x") == ""
                && !V3StableIds.IsValid(V3EntityKind.Player, "ship_wrong")
                && V3StableIds.Create(V3EntityKind.Player, "Captain One/Legacy") == "player_captain_one_legacy";
        }

        private static bool TestTypedValues()
        {
            var money = new MoneyAmount(-25);
            var weight = new WeightAmount(-1m);
            var volume = new VolumeAmount(2.5m);
            var day = new WorldDay(-2);
            var time = new TimeOfDay(1.5m);
            var relation = new RelationValue(-200);
            var health = new PercentValue(150);

            return money.Value == -25L
                && weight.Value == 0m
                && volume.Value == 2.5m
                && day.Value == 0
                && time.Normalized == 1m
                && relation.Value == -100
                && health.Value == 100;
        }

        private static bool TestCompleteEntityRoundTrips()
        {
            var player = new PlayerEntity
            {
                Id = V3StableIds.Create(V3EntityKind.Player, "captain_jack"),
                DisplayNameKey = "player.jack",
                Phase = EntityPhase.Production,
                Status = PlayerStatus.Active,
                Balance = new MoneyAmount(500),
                Credit = new MoneyAmount(1000),
                AppearanceLoadoutId = "outfit_default",
                ShipIds = new List<string> { "ship_small_fishing" },
                CrewIds = new List<string> { "crew_old_sailor" },
                RelationIds = new List<string> { "relation_jack_sailor" },
                QuestIds = new List<string> { "quest_first_voyage" },
                CollectionIds = new List<string> { "collection_first_map" }
            };
            player.LegacyD7.Money = 500;
            player.LegacyD7.ShipId = "ship_small_fishing";
            player.StateFlags["ready"] = "true";

            var ship = new ShipEntity
            {
                Id = "ship_small_fishing",
                Status = ShipStatus.InPort,
                HullIntegrity = new PercentValue(80),
                Mass = new WeightAmount(12.5m),
                CargoCapacity = new VolumeAmount(60m),
                AnchorState = AnchorState.Deployed,
                RepairState = RepairState.Needed,
                OwnershipRef = "player_captain_jack",
                CabinIds = new List<string> { "cabin_main" },
                FunctionPointIds = new List<string> { "function_helm", "function_anchor" }
            };
            ship.LegacyD7.Hull = 48;
            ship.LegacyD7.CrewIds.Add("crew_old_sailor");

            var crew = new CrewEntity
            {
                Id = "crew_old_sailor",
                Name = "老水手",
                RoleId = "deckhand",
                CultureId = "eng",
                Status = CrewStatus.Active,
                TraitIds = new List<string> { "trait_old_sailor" },
                Health = new PercentValue(90),
                Fatigue = new PercentValue(20),
                RelationIds = new List<string> { "relation_jack_sailor" },
                EquipmentItemIds = new List<string> { "item_compass" }
            };
            crew.LegacyD7.StatsJson = "{\"morale\":88,\"health\":90}";

            var relation = new RelationEntity
            {
                Id = "relation_jack_sailor",
                SourceId = "player_captain_jack",
                TargetId = "crew_old_sailor",
                RelationType = RelationType.Trust,
                Value = new RelationValue(25),
                Confidence = new PercentValue(75),
                Visibility = RelationVisibility.Private,
                CauseEventIds = new List<string> { "event_saved_at_sea" }
            };

            var island = new IslandEntity
            {
                Id = "island_santa_margarita",
                Status = IslandStatus.Visited,
                Terrain = "coast",
                WaterSourceIds = new List<string> { "water_spring_01" },
                HabitatIds = new List<string> { "habitat_mangrove" },
                Population = 120,
                AssetIds = new List<string> { "asset_port_santa_margarita" }
            };
            island.EcologyMetrics["health"] = new PercentValue(80);

            var parcel = new LandParcelEntity
            {
                Id = "parcel_santa_farm_01",
                Purpose = ParcelPurpose.Agricultural,
                Fertility = new PercentValue(70),
                WaterSourceRef = "water_spring_01",
                OwnerRef = "player_captain_jack",
                Risk = new PercentValue(10)
            };

            var building = new BuildingEntity
            {
                Id = "building_santa_warehouse_01",
                Function = BuildingFunction.Warehouse,
                Level = 1,
                OpenState = BuildingOpenState.Open,
                OwnershipRef = "player_captain_jack",
                WorkerIds = new List<string> { "crew_old_sailor" },
                Inventory = new List<ItemHolding>
                {
                    new ItemHolding
                    {
                        ItemId = "item_salt",
                        Quantity = 4,
                        TotalWeight = new WeightAmount(0.4m),
                        TotalVolume = new VolumeAmount(0.8m)
                    }
                }
            };

            var item = new ItemEntity
            {
                Id = "item_compass",
                Category = ItemCategory.Tool,
                Tags = new List<string> { "navigation" },
                GameplayRole = "navigation_tool",
                UnitWeight = new WeightAmount(0.2m),
                UnitVolume = new VolumeAmount(0.1m),
                Status = ItemStatus.Active
            };

            var animal = new AnimalEntity
            {
                Id = "animal_green_turtle_01",
                SpeciesId = "animal_green_turtle",
                AgeDays = new WorldDay(120),
                Sex = AnimalSex.Indeterminate,
                Status = AnimalStatus.Observed,
                HabitatId = "habitat_reef",
                Behavior = AnimalBehavior.Foraging,
                Health = new PercentValue(95),
                LegalStatus = LegalStatus.Licensed
            };

            var fish = new FishSchoolEntity
            {
                Id = "fish_reef_01",
                FishSpecies = "fish_snapper",
                Quantity = 40,
                DepthMeters = 12.5m,
                Migration = FishMigration.Resident,
                FishingPressure = new PercentValue(20),
                Recovery = new PercentValue(80)
            };

            var pet = new PetEntity
            {
                Id = "pet_dock_dog_01",
                Breed = "dock_dog",
                Health = new PercentValue(100),
                Hunger = new PercentValue(10),
                Cleanliness = new PercentValue(90),
                Attachment = new PercentValue(40),
                CareState = PetCareState.Healthy,
                Location = new LocationRefV3 { Kind = LocationKind.Island, RefId = island.Id }
            };

            var finance = new FinanceAccountEntity
            {
                Id = "finance_jack_credit",
                AccountType = FinanceAccountType.Credit,
                OwnerRef = player.Id,
                Principal = new MoneyAmount(1000),
                Interest = new MoneyAmount(25),
                DueDay = new WorldDay(30),
                DefaultState = FinanceDefaultState.Current,
                LedgerRefs = new List<string> { "ledger_jack_0001" }
            };

            var quest = new QuestEntity
            {
                Id = "quest_first_voyage",
                Status = QuestStatus.Active,
                ObjectiveIds = new List<string> { "objective_reach_green_cay" },
                PrerequisiteIds = new List<string> { "quest_start" },
                ChoiceIds = new List<string> { "choice_sail_day" },
                RewardRequestIds = new List<string> { "reward_first_voyage" },
                FactEventIds = new List<string> { "event_departed_port" }
            };

            var worldEvent = new WorldEventEntity
            {
                Id = "event_departed_port",
                EventId = "event_departed_port",
                WorldDay = new WorldDay(1),
                LocationRef = island.Id,
                ActorIds = new List<string> { player.Id, ship.Id },
                InputFacts = new Dictionary<string, string> { ["weather"] = "calm" },
                Result = new Dictionary<string, string> { ["status"] = "departed" },
                OutcomeStatus = EventOutcomeStatus.Succeeded,
                SaveRevision = 3,
                DuplicateKey = "event_departed_port:3"
            };

            var reward = new RewardEntity
            {
                Id = "reward_first_voyage",
                RewardId = "reward_first_voyage",
                Source = quest.Id,
                ItemId = item.Id,
                Quantity = 1,
                Money = new MoneyAmount(25),
                Unique = true,
                ClaimRecord = new List<string> { "player_captain_jack" },
                Status = RewardStatus.Claimed
            };

            var outfit = new AppearanceLoadoutEntity
            {
                Id = "outfit_default",
                LoadoutId = "outfit_default",
                Body = "body_default",
                Hair = "hair_short",
                Clothing = "outfit_deckhand",
                Accessories = new List<string> { "accessory_bandana" },
                Equipment = new List<string> { item.Id },
                Durability = new PercentValue(100),
                Source = "d5_legacy_preset"
            };

            return RoundTrip(player, p => p.Balance.Value == 500 && p.LegacyD7.Money == 500)
                && RoundTrip(ship, s => s.HullIntegrity.Value == 80 && s.LegacyD7.Hull == 48)
                && RoundTrip(crew, c => c.Health.Value == 90 && c.LegacyD7.StatsJson.Contains("morale"))
                && RoundTrip(relation, r => r.Value.Value == 25 && r.CauseEventIds.Count == 1)
                && RoundTrip(island, i => i.Population == 120 && i.EcologyMetrics["health"].Value == 80)
                && RoundTrip(parcel, p => p.Fertility.Value == 70)
                && RoundTrip(building, b => b.Inventory.Count == 1 && b.Inventory[0].Quantity == 4)
                && RoundTrip(item, i => i.UnitVolume.Value == 0.1m && i.Category == ItemCategory.Tool)
                && RoundTrip(animal, a => a.SpeciesId == "animal_green_turtle" && a.Health.Value == 95)
                && RoundTrip(fish, f => f.Quantity == 40 && f.DepthMeters == 12.5m)
                && RoundTrip(pet, p => p.Location.RefId == island.Id && p.CareState == PetCareState.Healthy)
                && RoundTrip(finance, f => f.Principal.Value == 1000 && f.DueDay.Value == 30)
                && RoundTrip(quest, q => q.Status == QuestStatus.Active && q.FactEventIds.Count == 1)
                && RoundTrip(worldEvent, e => e.EventId == e.Id && e.WorldDay.Value == 1 && e.SaveRevision == 3)
                && RoundTrip(reward, r => r.RewardId == reward.Id && r.Money.Value == 25 && r.Unique)
                && RoundTrip(outfit, o => o.LoadoutId == outfit.Id && o.Accessories.Count == 1);
        }

        private static bool TestLegacyFieldsCompatible()
        {
            string json = "{"
                + "\"id\":\"player_legacy\","
                + "\"legacy_d7\":{"
                + "\"id\":\"player_legacy\","
                + "\"name\":\"旧档船长\","
                + "\"money\":500,"
                + "\"skills\":{\"Sailing\":1},"
                + "\"traits\":[\"trait_old_sailor\"],"
                + "\"ship_id\":\"ship_legacy\","
                + "\"location\":{\"type\":\"port\",\"port_id\":\"santa_margarita\"},"
                + "\"play_seconds\":42"
                + "},"
                + "\"old_unknown_field\":\"ignored_for_forward_read\""
                + "}";

            bool ok = V3Json.TryDeserialize(json, out PlayerEntity player, out string error);
            return ok
                && error == ""
                && player.SchemaVersion == V3ModelVersions.EntitySchemaVersion
                && player.Status == PlayerStatus.Unknown
                && player.LegacyD7.Name == "旧档船长"
                && player.LegacyD7.Money == 500
                && player.LegacyD7.Location.PortId == "santa_margarita"
                && player.LegacyD7.PlaySeconds == 42
                && player.StateFlags != null;
        }

        private static bool TestUnknownEnumRejected()
        {
            bool ok = V3Json.TryDeserialize(
                "{\"id\":\"quest_unknown\",\"status\":\"future_status\"}",
                out QuestEntity ignored,
                out string error);
            return !ok && error.StartsWith("enum_or_value_rejected", StringComparison.Ordinal);
        }

        private static bool TestSaveMetadataDefaults()
        {
            var metadata = new V3SaveMetadata();
            string json = V3Json.Serialize(metadata);
            var restored = V3Json.Deserialize<V3SaveMetadata>(json);
            return metadata.SaveVersion == V3ModelVersions.TargetSaveVersion
                && restored.SaveVersion == 3
                && restored.MigrationFlags.Count == 0;
        }

        private static bool RoundTrip<T>(T value, Func<T, bool> check)
        {
            string json = V3Json.Serialize(value);
            if (string.IsNullOrEmpty(json) || !json.Contains("schema_version")) return false;
            T restored = V3Json.Deserialize<T>(json);
            return restored != null && check(restored);
        }
    }
}
