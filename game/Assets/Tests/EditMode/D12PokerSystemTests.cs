using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public class D12PokerSystemTests
    {
        [Test]
        public void EnterBetResolve_UsesFixedDeck_AndRoutesMoneyRewardRelationAndInformation()
        {
            var definition = CreateTable(25, 20, 0);
            var firstLog = new EventLog();
            var first = new D12PokerSystem(new[] { definition }, firstLog);
            var enter = first.Apply(Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 100));
            Assert.IsTrue(enter.Accepted, enter.Message);
            Assert.AreEqual(D12PokerTableStatus.InHand, enter.Status);
            Assert.AreEqual(5, enter.Session.PlayerHand.Count);
            Assert.AreEqual(5, enter.Session.HouseHand.Count);
            Assert.IsNotNull(enter.EconomyRequest);
            Assert.AreEqual(-25, enter.EconomyRequest.Amount);

            var bet = first.Apply(Request(D12PokerAction.PlaceBet, definition.TableId, 2, 1, 5, 100));
            Assert.IsTrue(bet.Accepted, bet.Message);
            Assert.AreEqual(5, bet.Session.CurrentBet);
            Assert.AreEqual(-5, bet.EconomyRequest.Amount);

            var resolved = first.Apply(Request(D12PokerAction.Resolve, definition.TableId, 3, 1, 0, 100));
            Assert.IsTrue(resolved.Accepted, resolved.Message);
            Assert.AreEqual(D12PokerTableStatus.Settled, resolved.Status);
            Assert.IsNotNull(resolved.Record);
            Assert.AreNotEqual(D12PokerOutcome.Unknown, resolved.Outcome);
            Assert.AreEqual(5, resolved.Record.PlayerCardIds.Count);
            Assert.AreEqual(5, resolved.Record.HouseCardIds.Count);
            Assert.IsNotNull(resolved.InformationRequest);
            Assert.IsNotNull(resolved.RelationRequest);
            if (resolved.Outcome == D12PokerOutcome.Win || resolved.Outcome == D12PokerOutcome.Push)
                Assert.IsNotNull(resolved.RewardRequest);
            Assert.AreEqual(3, firstLog.Facts.Count);
            foreach (var fact in firstLog.Facts) Assert.AreEqual(D12PokerIds.OwnerSystem, fact.OwnerSystem);

            var second = new D12PokerSystem(new[] { definition }, new EventLog());
            var secondEnter = second.Apply(Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 100));
            Assert.AreEqual(resolved.Session.PlayerHand[0].CardId, secondEnter.Session.PlayerHand[0].CardId);
            var secondBet = second.Apply(Request(D12PokerAction.PlaceBet, definition.TableId, 2, 1, 5, 100));
            var secondResolved = second.Apply(Request(D12PokerAction.Resolve, definition.TableId, 3, 1, 0, 100));
            Assert.AreEqual(resolved.Outcome, secondResolved.Outcome);
            Assert.AreEqual(resolved.Record.Payout, secondResolved.Record.Payout);
        }

        [Test]
        public void BetCapAndUnknownAction_AreRejectedWithoutFacts()
        {
            var definition = CreateTable(10, 3, 0);
            var log = new EventLog();
            var system = new D12PokerSystem(new[] { definition }, log);
            var unknown = system.Apply(Request(D12PokerAction.Unknown, definition.TableId, 0, 0, 0, 100));
            Assert.IsFalse(unknown.Accepted);
            Assert.AreEqual("unknown_action", unknown.Code);

            Assert.IsTrue(system.Apply(Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 100)).Accepted);
            var overCap = system.Apply(Request(D12PokerAction.PlaceBet, definition.TableId, 2, 1, 4, 100));
            Assert.IsFalse(overCap.Accepted);
            Assert.AreEqual("bet_cap_exceeded", overCap.Code);
            Assert.AreEqual(1, log.Facts.Count);
        }

        [Test]
        public void CheatRisk_ExposedEndsTheHand_AndProducesTraceableConsequences()
        {
            var definition = CreateTable(100, 20, 100);
            var system = new D12PokerSystem(new[] { definition }, new EventLog());
            Assert.IsTrue(system.Apply(Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 100)).Accepted);
            var cheated = system.Apply(Request(D12PokerAction.AttemptCheat, definition.TableId, 2, 1, 0, 100));
            Assert.IsTrue(cheated.Accepted, cheated.Message);
            Assert.AreEqual("cheat_exposed", cheated.Code);
            Assert.AreEqual(D12PokerTableStatus.Cheated, cheated.Status);
            Assert.AreEqual(D12PokerOutcome.Cheated, cheated.Outcome);
            Assert.AreEqual(D12PokerCheatRisk.Exposed, cheated.CheatRisk);
            Assert.AreEqual(-8, cheated.RelationRequest.ProposedDelta);
            Assert.AreEqual("cheat_exposed", cheated.InformationRequest.InformationCode);
            Assert.IsNotNull(cheated.Record);
            Assert.IsFalse(system.Apply(Request(D12PokerAction.Resolve, definition.TableId, 3, 1, 0, 100)).Accepted);
        }

        [Test]
        public void ExitAndDebt_CreateOwnerRequests_WithoutRefundOrDirectMoneyMutation()
        {
            var definition = CreateTable(5, 5, 20);
            var system = new D12PokerSystem(new[] { definition }, new EventLog());
            var enterRequest = Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 0);
            enterRequest.Economy.CanBorrow = true;
            enterRequest.Economy.DebtLimit = 20;
            var enter = system.Apply(enterRequest);
            Assert.IsTrue(enter.Accepted, enter.Message);
            Assert.IsNotNull(enter.DebtRequest);
            Assert.AreEqual(5, enter.DebtRequest.Principal);

            var betRequest = Request(D12PokerAction.PlaceBet, definition.TableId, 2, 1, 3, 0);
            betRequest.Economy.CanBorrow = true;
            betRequest.Economy.DebtLimit = 20;
            var bet = system.Apply(betRequest);
            Assert.IsTrue(bet.Accepted, bet.Message);
            Assert.IsNotNull(bet.DebtRequest);
            Assert.AreEqual(3, bet.DebtRequest.Principal);

            var exit = system.Apply(Request(D12PokerAction.Exit, definition.TableId, 3, 1, 0, 0));
            Assert.IsTrue(exit.Accepted, exit.Message);
            Assert.AreEqual(D12PokerOutcome.Fold, exit.Outcome);
            Assert.AreEqual(0, exit.Record.Payout);
            Assert.IsNull(exit.RewardRequest);
            Assert.AreEqual(2, system.Snapshot.DebtRequests.Count);
            Assert.AreEqual(-5, system.Snapshot.EconomyRequests[0].Amount);
            Assert.AreEqual(-3, system.Snapshot.EconomyRequests[1].Amount);
        }

        [Test]
        public void SaveLoad_PreservesCardsAndClaimGate_AndDoesNotPayTwice()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_poker_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "牌局测试船长" });
                var definition = CreateTable(0, 20, 0);
                var log = new EventLog();
                var system = new D12PokerSystem(new[] { definition }, log);
                system.Apply(Request(D12PokerAction.Enter, definition.TableId, 1, 1, 0, 100));
                system.Apply(Request(D12PokerAction.PlaceBet, definition.TableId, 2, 1, 4, 100));
                var resolved = system.Apply(Request(D12PokerAction.Resolve, definition.TableId, 3, 1, 0, 100));
                Assert.IsTrue(resolved.Accepted);
                var claim = system.ClaimSettlement(resolved.Record.GameId, 3);
                Assert.IsTrue(claim.Accepted, claim.Message);

                SaveSystem.SaveD12PokerRuntime(system.Snapshot);
                SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot { EventLog = log.CaptureSnapshot() });
                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loaded = SaveSystem.LoadD12PokerRuntime();
                Assert.IsNotNull(loaded);
                Assert.AreEqual(1, loaded.Records.Count);
                Assert.IsTrue(loaded.Records[0].SettlementClaimed);
                Assert.AreEqual(resolved.Record.PlayerCardIds[0], loaded.Sessions[0].PlayerHand[0].CardId);

                var restoredLog = EventLog.FromSnapshot(SaveSystem.LoadD3Runtime().EventLog);
                var restored = new D12PokerSystem(new[] { definition }, restoredLog, loaded);
                var duplicateClaim = restored.ClaimSettlement(resolved.Record.GameId, 3);
                Assert.IsFalse(duplicateClaim.Accepted);
                Assert.AreEqual("duplicate_settlement", duplicateClaim.Code);
                Assert.AreEqual(1, restored.Snapshot.RewardRequests.Count);
            }
            finally
            {
                SaveSystem.DbPath = null;
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    string path = dbPath + suffix;
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
        }

        private static D12PokerTableDefinition CreateTable(long entryFee, long betCap, int cheatRisk)
        {
            return new D12PokerTableDefinition
            {
                TableId = "test_poker_table",
                RulesetId = D12PokerIds.RulesetId,
                LocationRef = "test_tavern",
                RelationTargetId = "npc_test_keeper",
                InformationPoolId = "test_tells",
                FixedSeed = 7799,
                EntryFee = entryFee,
                BetCap = betCap,
                DebtLimit = 20,
                CheatRiskPercent = cheatRisk,
            };
        }

        private static D12PokerActionRequest Request(D12PokerAction action, string tableId,
            long revision, int day, long amount, long availableMoney)
        {
            return new D12PokerActionRequest
            {
                Action = action,
                TableId = tableId,
                PlayerId = "player_test",
                SaveRevision = revision,
                WorldDay = day,
                Amount = amount,
                LocationRef = "test_tavern",
                Economy = new D12PokerEconomyReadSnapshot
                {
                    AvailableMoney = availableMoney,
                    CurrentDebt = 0,
                    CanBorrow = false,
                    DebtLimit = 0,
                },
            };
        }
    }
}
