// D14-A04 奖励：五档稀有度、任务请求、唯一性、来源审计、丢失恢复和读档幂等。

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D14;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14RewardSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_reward_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = _dbPath + suffix;
                if (File.Exists(path)) try { File.Delete(path); } catch { }
            }
        }

        [Test]
        public void FiveRaritiesRequestGrantClaimAndSaveRestoreAreIdempotent()
        {
            var log = CreateSources();
            var authority = new AuthorityFixture();
            var system = new D14RewardSystem(log, null, authority, authority);
            var catalog = new D14RewardCatalog();
            var rewardIds = new[]
            {
                "reward_d14_ordinary_supply", "reward_d14_professional_rescue", "reward_d14_rare_indigo_dye",
                "reward_d14_unique_moonwake_armory", "reward_d14_worldclass_reinforced_keel",
            };
            var sourceIds = new[] { "source_ordinary", "source_professional", "source_rare", "source_unique", "source_world" };
            var grants = new List<D14RewardGrantRecord>();

            for (int i = 0; i < rewardIds.Length; i++)
            {
                var request = system.Request(new D14RewardRequest
                {
                    RequestId = "task_reward_" + i, RewardId = rewardIds[i], RecipientRef = "player",
                    RequestedBySystem = "QuestSystem", TaskId = "quest_d14_vertical_slice", SourceEventId = sourceIds[i],
                    SourceSaveRevision = 0, WorldDay = i + 1, SaveRevision = 10 + i,
                    CauseEventIds = new List<string> { sourceIds[i] },
                });
                Assert.IsTrue(request.Accepted, request.Message);
                Assert.IsNotNull(request.Request);
                Assert.AreEqual(D14RewardRequestStatus.Pending, request.Request.Status);
                Assert.IsNull(request.Grant, "Request must not carry a final reward result.");

                var granted = system.Grant(new D14RewardGrantRequest { RequestId = "task_reward_" + i, WorldDay = i + 1, SaveRevision = 20 + i });
                Assert.IsTrue(granted.Accepted, granted.Message);
                Assert.IsNotNull(granted.Grant);
                Assert.AreEqual((D14RewardRarity)i, granted.Grant.Rarity);
                grants.Add(granted.Grant);

                var claimed = system.Claim(new D14RewardClaimRequest
                {
                    OperationId = "claim_" + i, GrantId = granted.Grant.GrantId, RecipientRef = "player", WorldDay = i + 1, SaveRevision = 30 + i,
                });
                Assert.IsTrue(claimed.Accepted, claimed.Message);
                Assert.AreEqual(D14RewardStatus.Claimed, claimed.Grant.Status);
            }

            var duplicateUniqueRequest = system.Request(new D14RewardRequest
            {
                RequestId = "task_unique_again", RewardId = "reward_d14_unique_moonwake_armory", RecipientRef = "player",
                RequestedBySystem = "QuestSystem", TaskId = "quest_d14_vertical_slice", SourceEventId = "source_unique_again",
                SourceSaveRevision = 0, WorldDay = 9, SaveRevision = 40,
            });
            Assert.IsTrue(duplicateUniqueRequest.Accepted, duplicateUniqueRequest.Message);
            var duplicateUnique = system.Grant(new D14RewardGrantRequest { RequestId = "task_unique_again", WorldDay = 9, SaveRevision = 41 });
            Assert.IsFalse(duplicateUnique.Accepted);
            Assert.AreEqual("unique_already_granted", duplicateUnique.Code);

            var professional = grants[1];
            var lost = system.MarkLost(new D14RewardLossRequest
            {
                OperationId = "lose_professional", GrantId = professional.GrantId, Reason = D14RewardLossReason.InventoryLoss,
                SourceEventId = "inventory_loss", SourceSaveRevision = 0, WorldDay = 8, SaveRevision = 42,
            });
            Assert.IsTrue(lost.Accepted, lost.Message);
            Assert.AreEqual(D14RewardStatus.Lost, lost.Grant.Status);
            var recovered = system.Recover(new D14RewardRecoveryRequest
            {
                OperationId = "recover_professional", GrantId = professional.GrantId, RecoverySourceEventId = "recovery_source",
                RecoverySourceSaveRevision = 0, RecipientRef = "player", WorldDay = 9, SaveRevision = 43,
            });
            Assert.IsTrue(recovered.Accepted, recovered.Message);
            Assert.AreEqual(D14RewardStatus.Recovered, recovered.Grant.Status);
            Assert.AreEqual(professional.GrantId, recovered.Grant.GrantId);
            Assert.IsTrue(system.ReadAudits().Count >= 8, "请求、发放、领取、丢失、恢复均应留审计。");

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 奖励测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14RewardRuntime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14RewardRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(saved.Requests.Count, loaded.Requests.Count);
            Assert.AreEqual(saved.Grants.Count, loaded.Grants.Count);
            Assert.AreEqual(saved.Audits.Count, loaded.Audits.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = new D14RewardSystem(null, catalog, authority, authority, loaded);
            var duplicateClaim = restored.Claim(new D14RewardClaimRequest
            {
                OperationId = "claim_0", GrantId = grants[0].GrantId, RecipientRef = "player", WorldDay = 1, SaveRevision = 30,
            });
            Assert.IsTrue(duplicateClaim.Accepted && duplicateClaim.Duplicate);
            var duplicateRecovery = restored.Recover(new D14RewardRecoveryRequest
            {
                OperationId = "recover_professional", GrantId = professional.GrantId, RecoverySourceEventId = "recovery_source",
                RecoverySourceSaveRevision = 0, RecipientRef = "player", WorldDay = 9, SaveRevision = 43,
            });
            Assert.IsTrue(duplicateRecovery.Accepted && duplicateRecovery.Duplicate);
            Assert.AreEqual(saved.Grants.Count, restored.Snapshot.Grants.Count);
            Assert.AreEqual(saved.Audits.Count, restored.Snapshot.Audits.Count);
        }

        [Test]
        public void TaskRequestDoesNotGrantAndAgentReplyCannotSupplyFinalReward()
        {
            var system = new D14RewardSystem(CreateSources(), null, new AuthorityFixture(), new AuthorityFixture());
            var pending = system.RequestTaskReward(new D14TaskRewardRequest
            {
                RequestId = "task_pending_only", RewardId = "reward_d14_ordinary_supply", TaskId = "quest_test",
                RecipientRef = "player", SourceEventId = "source_ordinary", SourceSaveRevision = 0, WorldDay = 1, SaveRevision = 50,
            });
            Assert.IsTrue(pending.Accepted, pending.Message);
            Assert.AreEqual(0, pending.State.Grants.Count);
            Assert.AreEqual(D14RewardRequestStatus.Pending, pending.Request.Status);

            var rejected = system.Request(new D14RewardRequest
            {
                RequestId = "agent_final_reward", RewardId = "reward_d14_ordinary_supply", RecipientRef = "player",
                RequestedBySystem = "AgentReply", SourceEventId = "source_ordinary", SourceSaveRevision = 0, WorldDay = 1, SaveRevision = 51,
            });
            Assert.IsFalse(rejected.Accepted);
            Assert.AreEqual("invalid_request", rejected.Code);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "source_ordinary", "QuestSystem"); AddSource(log, "source_professional", "CombatSystem");
            AddSource(log, "source_rare", "CollectionAtlasSystem"); AddSource(log, "source_unique", "CollectionAtlasSystem");
            AddSource(log, "source_world", "AchievementSystem"); AddSource(log, "source_unique_again", "QuestSystem");
            AddSource(log, "inventory_settlement", D14RewardIds.InventoryOwner); AddSource(log, "inventory_loss", D14RewardIds.InventoryOwner);
            AddSource(log, "recovery_source", D14RewardIds.RecoveryOwner);
            return log;
        }

        private static void AddSource(EventLog log, string id, string owner)
        {
            log.Append(new WorldEvent(id, 0, "test_source", owner,
                new EventResult { ResultCode = "source_ready", Summary = "D14-A04 测试来源事实。", FactIds = new List<string> { "source:" + id } },
                new[] { "player" }, new[] { "test:d14_a04" }));
        }

        private sealed class AuthorityFixture : ID14RewardSettlementAuthority, ID14RewardRecoveryAuthority
        {
            public D14RewardSettlementDecision Settle(D14RewardSettlementRequest request)
            {
                return new D14RewardSettlementDecision
                {
                    Allowed = true, Code = "settled", Message = "fixture settlement",
                    SourceOwnerSystem = D14RewardIds.OwnerSystem, SourceEventId = request.SourceFact.EventId,
                    InventorySourceEventId = "inventory_settlement",
                };
            }

            public D14RewardRecoveryDecision Recover(D14RewardRecoveryRequest request, D14RewardGrantRecord grant, D14RewardDefinition definition, FactEvent sourceFact)
            {
                return new D14RewardRecoveryDecision
                {
                    Allowed = true, Code = "recovered", Message = "fixture recovery",
                    SourceOwnerSystem = D14RewardIds.RecoveryOwner, SourceEventId = sourceFact.EventId,
                    InventorySourceEventId = "inventory_settlement",
                };
            }
        }
    }
}
