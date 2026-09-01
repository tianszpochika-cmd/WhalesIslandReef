// D22ReadModelAdapterTests —— D22-L17—L25 纯逻辑定向断言。
// 该测试文件只使用 BCL/NUnit 与公开契约，不启动 Unity、Canvas、输入或网络。

using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.D14;
using Whalebone.Systems.D19;
using Whalebone.Systems.D20;
using Whalebone.Systems.D21;
using Whalebone.Systems.D22;
using Whalebone.Systems.D3;

namespace WhalesIslandReef.Tests.EditMode.D22
{
    public sealed class D22ReadModelAdapterTests
    {
        [Test]
        public void MarketProjection_UsesStableOrderAndCloneIsolation()
        {
            var market = new D3MarketState();
            market.Stock["sugar"] = 8;
            market.Demand["sugar"] = 4;
            market.PlayerTradeVolume["sugar"] = 1;
            var quote = new MarketQuote
            {
                PortId = "port.test",
                CommodityId = "sugar",
                BuyUnitPrice = 10,
                SellUnitPrice = 8,
                Stock = 8,
                Demand = 4,
                PlayerTradeVolume = 1,
            };
            var tradeState = new D3TradeState(100, 20) { Market = market };
            var source = new D22MarketOwnerSnapshot
            {
                WorldDay = 3,
                SaveRevision = 7,
                PortId = "port.test",
                Market = market,
                TradeState = tradeState,
            };
            source.Quotes.Add(quote);

            var model = D22MarketReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Ready));
            Assert.That(model.Items.Count, Is.EqualTo(1));
            Assert.That(model.Items[0].CommodityId, Is.EqualTo("sugar"));
            Assert.That(model.Items[0].BuyUnitPrice, Is.EqualTo(10));
            Assert.That(model.IsReadOnly, Is.True);
            Assert.That(model.LocalizationKey, Does.StartWith("ui.d22.adapter.market."));

            market.Stock["sugar"] = 1;
            quote.BuyUnitPrice = 999;
            source.Quotes.Clear();

            Assert.That(model.Items[0].Stock, Is.EqualTo(8));
            Assert.That(model.Items[0].BuyUnitPrice, Is.EqualTo(10));
            Assert.That(model.Clone().Items, Is.Not.SameAs(model.Items));
        }

        [Test]
        public void MarketProjection_FailsClosedOnMissingQuoteAndVersion()
        {
            var market = new D3MarketState();
            market.Stock["salt"] = 4;
            var source = new D22MarketOwnerSnapshot
            {
                WorldDay = 1,
                SaveRevision = 1,
                PortId = "port.test",
                Market = market,
                TradeState = new D3TradeState(10, 10),
            };

            var missingQuote = D22MarketReadModel.Project(source);
            Assert.That(missingQuote.Status, Is.EqualTo(D22ReadModelAdapterStatus.Partial));
            Assert.That(missingQuote.Items[0].IsEnabled, Is.False);
            Assert.That(missingQuote.Items[0].DisabledReasonCode, Is.EqualTo("d22.market.quote_missing"));
            Assert.That(HasIssue(missingQuote, "missing_market_quote"), Is.True);

            source.SchemaVersion = 99;
            var invalidVersion = D22MarketReadModel.Project(source);
            Assert.That(invalidVersion.Status, Is.EqualTo(D22ReadModelAdapterStatus.Invalid));
        }

        [Test]
        public void PortProjection_CopiesPublicReadModelAndStableServices()
        {
            var sourcePort = D19PortReadModel.Build(
                4,
                9,
                D19PortVisitState.PortVisit,
                "port.test",
                string.Empty,
                "ship.test",
                null,
                null,
                null,
                null,
                null);
            var source = new D22PortOwnerSnapshot { Port = sourcePort };

            var model = D22PortReadModel.Project(source);

            Assert.That(model.PortId, Is.EqualTo("port.test"));
            Assert.That(model.Visit.IsInPort, Is.True);
            Assert.That(model.Services.Count, Is.GreaterThan(0));
            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Partial));
            Assert.That(model.Metadata.MissingModuleIds, Does.Contain("d19.port.departure_checklist"));
            Assert.That(model.Clone().Visit, Is.Not.SameAs(model.Visit));
            Assert.That(model.Services[0].ServiceId, Is.LessThanOrEqualTo(model.Services[model.Services.Count - 1].ServiceId));
        }

        [Test]
        public void InventoryProjection_UsesOwnerReadHelpersWithoutMutation()
        {
            var inventory = new D3InventoryState(10, 100);
            inventory.Cargo["timber"] = 2;
            inventory.PersonalItems["chart"] = 1;
            var source = new D22InventoryOwnerSnapshot
            {
                WorldDay = 2,
                SaveRevision = 8,
                Inventory = inventory,
                TradeState = new D3TradeState(123, 10),
            };

            var model = D22InventoryReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Ready));
            Assert.That(model.UsedCargo, Is.EqualTo(4));
            Assert.That(model.UsedWeight, Is.EqualTo(4));
            Assert.That(model.Money, Is.EqualTo(123));
            Assert.That(model.Items[0].ItemId, Is.EqualTo("timber"));
            Assert.That(model.Items[1].ItemId, Is.EqualTo("chart"));

            inventory.Cargo["timber"] = 99;
            Assert.That(model.UsedCargo, Is.EqualTo(4));
            Assert.That(model.Items[0].Quantity, Is.EqualTo(2));
        }

        [Test]
        public void NpcProjection_MapsRoleAndDialogueQualification()
        {
            var identity = new D20NpcIdentity
            {
                NpcId = "npc.test",
                RoleId = "merchant",
                FactionId = "faction.test",
                ContentId = "content.test",
                Availability = D20CharacterAvailability.Available,
                MigrationStatus = D20NpcIdentityMigrationStatus.UnknownPreserved,
            };
            var role = D20NpcRoleCatalog.Find("merchant");
            var request = D20NpcInteractionAvailabilityRequest.Create(
                "npc.test",
                "merchant",
                D20NpcInteractionKind.Talk,
                D20NpcBehaviorState.Idle,
                D20CharacterAvailability.Available,
                true,
                false,
                false,
                false,
                0,
                false,
                D20NpcDangerLevel.None,
                "location.test",
                2,
                8);
            var interaction = D20NpcInteractionAvailability.Evaluate(request);
            var source = new D22NpcOwnerSnapshot
            {
                WorldDay = 2,
                SaveRevision = 8,
            };
            source.Npcs.Add(new D22NpcOwnerEntry { Identity = identity, Role = role, Interaction = interaction });

            var model = D22NpcReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Ready));
            Assert.That(model.Npcs[0].NpcId, Is.EqualTo("npc.test"));
            Assert.That(model.Npcs[0].DialogueQualified, Is.True);
            Assert.That(model.Npcs[0].InteractionAvailable, Is.True);
            Assert.That(model.Npcs[0].SourceLocalizationKey, Is.EqualTo("npc.interaction.available"));
            Assert.That(model.Npcs[0].InteractionOutcome.Feedback, Is.Not.Null);
        }

        [Test]
        public void QuestProjection_MapsObjectiveFailureAndRewardStatus()
        {
            var state = new D12DynamicQuestState
            {
                QuestId = "quest.alpha",
                Status = D12DynamicQuestStatus.Active,
                AcceptedWorldDay = 1,
                LastUpdatedWorldDay = 2,
                LastFailureCode = string.Empty,
                RewardRequest = new D12QuestRewardRequest
                {
                    RequestId = "request.alpha",
                    RewardId = "reward.alpha",
                    QuestId = "quest.alpha",
                    SourceEventId = "event.alpha",
                    RewardType = D12QuestRewardType.Item,
                },
            };
            state.Objectives.Add(new D12QuestObjectiveState { ObjectiveId = "objective.alpha", Progress = 1 });
            var definition = new D12DynamicQuestDefinition
            {
                QuestId = "quest.alpha",
                DisplayNameKey = "quest.title.alpha",
                Title = "source title",
                Description = "source description",
                RewardId = "reward.alpha",
                RewardType = D12QuestRewardType.Item,
            };
            definition.Objectives.Add(new D12QuestObjectiveDefinition
            {
                ObjectiveId = "objective.alpha",
                Description = "source objective",
                TargetOwnerSystem = "D3",
                RequiredCount = 1,
            });
            var reward = new D14RewardRuntimeSnapshot();
            reward.Grants.Add(new D14RewardGrantRecord
            {
                GrantId = "grant.alpha",
                RequestId = "request.alpha",
                RewardId = "reward.alpha",
                RecipientRef = "player",
                Status = D14RewardStatus.Granted,
            });
            var source = new D22QuestOwnerSnapshot { WorldDay = 2, SaveRevision = 5, QuestLog = new D12DynamicQuestLogSnapshot() };
            source.QuestLog.Quests.Add(state);
            source.Definitions.Add(definition);
            source.RewardSnapshot = reward;
            source.Results.Add(new D12QuestResult { Accepted = true, Applied = true, QuestId = "quest.alpha", Status = D12DynamicQuestStatus.Active });

            var model = D22QuestReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Ready));
            Assert.That(model.Quests.Count, Is.EqualTo(1));
            Assert.That(model.Quests[0].Status, Is.EqualTo("Active"));
            Assert.That(model.Quests[0].Objectives[0].Progress, Is.EqualTo(1));
            Assert.That(model.Quests[0].Reward.Status, Is.EqualTo("Granted"));
            Assert.That(model.Quests[0].LastResult.Succeeded, Is.True);
        }

        [Test]
        public void SaveProjection_MapsLoadStateSlotsAndMissingModules()
        {
            var load = new D22LoadStateSnapshot
            {
                Operation = D22LoadOperationKind.Load,
                State = D22LoadLifecycleState.Loading,
                SlotId = "slot.alpha",
                SaveRevision = 3,
                SourceRevision = 2,
                TargetRevision = 3,
                ProgressPercent = 50,
            };
            load.Modules.Add(new D22LoadModuleState { ModuleId = "d3.inventory", Status = D22LoadModuleStatus.Present, Required = true, ModuleSchemaVersion = 1 });
            var source = new D22SaveOwnerSnapshot
            {
                WorldDay = 2,
                SaveRevision = 3,
                LoadState = load,
            };
            source.Slots.Add(new D22SaveSlotOwnerSnapshot
            {
                SlotId = "slot.alpha",
                Exists = true,
                IsReadable = true,
                IntegrityOk = true,
                SaveRevision = 3,
                WorldDay = 2,
            });

            var model = D22SaveReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Ready));
            Assert.That(model.LoadState.IsBusy, Is.True);
            Assert.That(model.Slots[0].IntegrityOk, Is.True);
            Assert.That(model.Slots[0].SlotId, Is.EqualTo("slot.alpha"));
            Assert.That(model.Clone().LoadState, Is.Not.SameAs(model.LoadState));

            load.MissingModuleIds.Add("d21.ship");
            Assert.That(model.LoadState.MissingModuleIds, Does.Not.Contain("d21.ship"));
        }

        [Test]
        public void ShipProjection_ConsumesPublicD21SnapshotsWithoutInventingAggregateState()
        {
            var source = new D22ShipOwnerSnapshot
            {
                WorldDay = 2,
                SaveRevision = 5,
                HasWorldDay = true,
                HasSaveRevision = true,
                VoyageId = "voyage.alpha",
                Identity = new D21ShipIdentity
                {
                    ShipId = "ship.alpha",
                    ShipTypeId = "sloop",
                    ShipName = "source name",
                },
                ConditionState = new D21ShipConditionState
                {
                    ShipId = "ship.alpha",
                    MaxDurability = 100,
                    Durability = 90,
                    MaxFlooding = 10,
                    Flooding = 0,
                    Seaworthiness = D21ShipSeaworthiness.Navigable,
                },
                Route = new D21NavigationRouteDefinition
                {
                    RouteId = "route.alpha",
                    OriginId = "port.alpha",
                    DestinationId = "port.beta",
                    TotalDistanceNm = 10,
                    Waypoints = new List<D21RouteWaypoint>(),
                    Hazards = new List<D21HazardZoneDefinition>(),
                },
                Time = D21VoyageTimeState.CreateInitial("voyage.alpha", "ship.alpha", 10000, 2, 60, 5),
            };

            var model = D22ShipReadModel.Project(source);

            Assert.That(model.Status, Is.EqualTo(D22ReadModelAdapterStatus.Partial));
            Assert.That(model.IsBlocked, Is.False);
            Assert.That(HasIssue(model, "upstream_acceptance_missing"), Is.False);
            Assert.That(model.ShipId, Is.EqualTo("ship.alpha"));
            Assert.That(model.RouteId, Is.EqualTo("route.alpha"));
            Assert.That(model.RouteDistanceMilliNm, Is.EqualTo(10000));
        }

        [Test]
        public void FocusPanelAndCommandRules_ArePureAndDeterministic()
        {
            var focus = new D22FocusStateMachine();
            var activation = focus.ActivateScreen(
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                D22FocusControlIds.MainMenuContinue,
                D22FocusControlIds.MainMenuNewGame,
                D22InputModeIntent.Navigation);
            Assert.That(activation.Accepted, Is.True);

            var panelDecision = D22PanelExclusivityRules.Evaluate(new D22PanelOpenRequest
            {
                PanelId = D22PanelIds.MainMenuSettings,
                ScreenId = D22ScreenIds.MainMenu,
                OwnerId = "test",
            });
            Assert.That(panelDecision.Allowed, Is.True);

            var request = D22UiCommandRequest.Create(
                D22UiCommandKind.OpenPanel,
                "command.open",
                "correlation.open",
                "actor.test",
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenuSettings,
                D22PanelIds.MainMenuSettings);
            Assert.That(request.ValidateShape(), Is.EqualTo(D22UiCommandValidationCode.None));
            var rejected = D22FeedbackCatalog.Create(D22FeedbackCodes.OwnerRejected, "command.open", "test", "owner_rejected");
            Assert.That(rejected.LocalizationKey, Does.StartWith("ui.d22.feedback."));
            Assert.That(rejected.Code, Is.EqualTo(D22FeedbackCodes.OwnerRejected));
        }

        [Test]
        public void MissingSnapshotsNeverClaimReady()
        {
            Assert.That(D22MarketReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22PortReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22ShipReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22InventoryReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22NpcReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22QuestReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
            Assert.That(D22SaveReadModel.Project(null).Status, Is.EqualTo(D22ReadModelAdapterStatus.Blocked));
        }

        private static bool HasIssue(D22ReadModelAdapterModel model, string code)
        {
            foreach (var issue in model.Issues)
                if (issue != null && issue.Code == code) return true;
            return false;
        }
    }
}
