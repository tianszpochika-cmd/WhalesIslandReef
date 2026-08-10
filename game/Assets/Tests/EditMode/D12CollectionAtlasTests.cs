using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D12CollectionAtlasTests
    {
        [Test]
        public void VerticalSliceCatalog_CoversAllNineCategories()
        {
            var definitions = D12CollectionCatalog.CreateVerticalSlice();
            var categories = new HashSet<D12CollectionCategory>();
            foreach (var definition in definitions)
            {
                definition.Validate();
                categories.Add(definition.Category);
            }
            Assert.AreEqual(9, definitions.Count);
            Assert.AreEqual(9, categories.Count);
        }

        [Test]
        public void EvidenceMustReferenceFact_AndAuthorizedEntriesNeedAuthorizationFact()
        {
            var log = new EventLog();
            var system = new D12CollectionAtlasSystem(eventLog: log);
            var noFact = system.Record(new D12CollectionRecordRequest
            {
                ContentId = "animal_green_turtle",
                Category = D12CollectionCategory.Animal,
                FactEventId = "agent_text_only",
                FactSaveRevision = 1,
                EvidenceRef = "agent:claimed_turtle",
                LocationRef = "nearshore_saint_marys",
                WorldDay = 1,
                AcquisitionMethod = D12CollectionAcquisitionMethod.Observation,
            });
            Assert.IsFalse(noFact.Accepted);
            Assert.AreEqual("fact_event_missing", noFact.Code);

            var source = AppendFact(log, "event_archive_log_found", 2, "santa_margarita", 20, "ArchiveSystem");
            var missingAuthorization = system.Record(new D12CollectionRecordRequest
            {
                ContentId = D12CollectionCatalog.HistoricalFileId,
                Category = D12CollectionCategory.HistoricalFile,
                FactEventId = source.EventId,
                FactSaveRevision = source.SaveRevision,
                EvidenceRef = "archive:log:page_01",
                LocationRef = source.LocationRef,
                WorldDay = source.WorldDay,
                AcquisitionMethod = D12CollectionAcquisitionMethod.AuthorizedArchive,
                AuthorizationRef = "archive_permit_saint_marys",
                AuthorizationFactEventId = "missing_permit",
                AuthorizationSaveRevision = 21,
            });
            Assert.IsFalse(missingAuthorization.Accepted);
            Assert.AreEqual("authorization_fact_missing", missingAuthorization.Code);

            var permit = AppendFact(log, "event_archive_permit_saint_marys", 2, "santa_margarita", 21, "HarborAuthority");
            var accepted = system.Record(new D12CollectionRecordRequest
            {
                ContentId = D12CollectionCatalog.HistoricalFileId,
                Category = D12CollectionCategory.HistoricalFile,
                FactEventId = source.EventId,
                FactSaveRevision = source.SaveRevision,
                EvidenceRef = "archive:log:page_01",
                LocationRef = source.LocationRef,
                WorldDay = source.WorldDay,
                AcquisitionMethod = D12CollectionAcquisitionMethod.AuthorizedArchive,
                AuthorizationRef = "archive_permit_saint_marys",
                AuthorizationFactEventId = permit.EventId,
                AuthorizationSaveRevision = permit.SaveRevision,
            });
            Assert.IsTrue(accepted.Accepted, accepted.Message);
            Assert.IsTrue(accepted.AchievementUnlocked);
            Assert.IsNotNull(accepted.Evidence.FactEventId);
            Assert.IsNotNull(accepted.AchievementFact);
        }

        [Test]
        public void EveryVerticalSliceCategory_CanRecordTraceableEvidence()
        {
            var log = new EventLog();
            var system = new D12CollectionAtlasSystem(eventLog: log);
            int index = 0;
            foreach (var definition in D12CollectionCatalog.CreateVerticalSlice())
            {
                index++;
                var location = "test_location_" + index;
                var source = AppendFact(log, "event_d12_a07_source_" + index, index, location, 100 + index, "FixtureOwner");
                FactEvent authorization = null;
                if (definition.RequiresAuthorization)
                    authorization = AppendFact(log, "event_d12_a07_authorization_" + index, index, location, 200 + index, "AuthorityOwner");
                var result = system.Record(new D12CollectionRecordRequest
                {
                    ContentId = definition.ContentId,
                    Category = definition.Category,
                    FactEventId = source.EventId,
                    FactSaveRevision = source.SaveRevision,
                    EvidenceRef = "fixture:evidence:" + definition.ContentId,
                    LocationRef = source.LocationRef,
                    WorldDay = source.WorldDay,
                    AcquisitionMethod = definition.AllowedAcquisitionMethods[0],
                    AuthorizationRef = definition.RequiresAuthorization ? definition.RequiredAuthorizationRef : "",
                    AuthorizationFactEventId = authorization == null ? "" : authorization.EventId,
                    AuthorizationSaveRevision = authorization == null ? 0 : authorization.SaveRevision,
                });
                Assert.IsTrue(result.Accepted, definition.ContentId + ": " + result.Message);
                Assert.AreEqual(definition.ContentId, result.Evidence.ContentId);
                Assert.IsNotEmpty(result.Evidence.EvidenceRef);
                Assert.IsNotEmpty(result.Evidence.LocationRef);
                Assert.GreaterOrEqual(result.Evidence.WorldDay, 0);
                Assert.IsNotEmpty(result.Evidence.FactEventId);
            }
            Assert.AreEqual(9, system.Snapshot.Records.Count);
            Assert.AreEqual(9, system.Snapshot.Evidence.Count);
            Assert.AreEqual(9, system.Snapshot.Achievements.Count);
        }

        [Test]
        public void DuplicateEvidence_DoesNotUnlockAchievementTwice()
        {
            var log = new EventLog();
            var system = new D12CollectionAtlasSystem(eventLog: log);
            var source = AppendFact(log, "event_fish_caught_01", 4, "nearshore_saint_marys", 40, "D11FishingHarvestSystem");
            var request = new D12CollectionRecordRequest
            {
                ContentId = "fish_mackerel_school",
                Category = D12CollectionCategory.Fish,
                FactEventId = source.EventId,
                FactSaveRevision = source.SaveRevision,
                EvidenceRef = "harvest:fish:mackerel:01",
                LocationRef = source.LocationRef,
                WorldDay = source.WorldDay,
                AcquisitionMethod = D12CollectionAcquisitionMethod.Catch,
            };
            var first = system.Record(request);
            var duplicate = system.Record(request);
            Assert.IsTrue(first.AchievementUnlocked);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.IsFalse(duplicate.AchievementUnlocked);
            Assert.AreEqual(1, system.Snapshot.Records.Count);
            Assert.AreEqual(1, system.Snapshot.Evidence.Count);
            Assert.AreEqual(1, system.Snapshot.Achievements.Count);

            var secondSource = AppendFact(log, "event_fish_caught_02", 5, "nearshore_saint_marys", 50, "D11FishingHarvestSystem");
            request.FactEventId = secondSource.EventId;
            request.FactSaveRevision = secondSource.SaveRevision;
            request.EvidenceRef = "harvest:fish:mackerel:02";
            request.WorldDay = secondSource.WorldDay;
            var second = system.Record(request);
            Assert.IsTrue(second.Accepted);
            Assert.IsFalse(second.AchievementUnlocked);
            Assert.AreEqual(2, system.FindRecord("fish_mackerel_school").EvidenceIds.Count);
            Assert.AreEqual(1, system.Snapshot.Achievements.Count);
        }

        [Test]
        public void SaveLoad_PreservesEvidenceAndCollectionState()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_collection_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D12 图鉴测试船长" });
                var log = new EventLog();
                var source = AppendFact(log, "event_treasure_found_01", 6, "landmark:saint_marys_chart_cove", 60, "D9TreasureQuestSystem");
                var system = new D12CollectionAtlasSystem(eventLog: log);
                var result = system.Record(new D12CollectionRecordRequest
                {
                    ContentId = "reward_first_chart",
                    Category = D12CollectionCategory.Treasure,
                    FactEventId = source.EventId,
                    FactSaveRevision = source.SaveRevision,
                    EvidenceRef = "quest:first_chart:reward:01",
                    LocationRef = source.LocationRef,
                    WorldDay = source.WorldDay,
                    AcquisitionMethod = D12CollectionAcquisitionMethod.QuestReward,
                });
                Assert.IsTrue(result.Accepted, result.Message);

                SaveSystem.SaveD12Runtime(new D12RuntimeSnapshot
                {
                    MapDiscovery = new D12MapDiscoverySnapshot(),
                    CollectionAtlas = system.Snapshot,
                });
                SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
                {
                    CurrentLocationId = source.LocationRef,
                    EventLog = log.CaptureSnapshot(),
                });
                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loaded = SaveSystem.LoadD12Runtime();
                var loadedD3 = SaveSystem.LoadD3Runtime();
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.CollectionAtlas);
                Assert.AreEqual(1, loaded.CollectionAtlas.Records.Count);
                Assert.AreEqual(1, loaded.CollectionAtlas.Evidence.Count);
                Assert.AreEqual(1, loaded.CollectionAtlas.Achievements.Count);
                Assert.AreEqual("reward_first_chart", loaded.CollectionAtlas.Records[0].ContentId);
                var restoredLog = EventLog.FromSnapshot(loadedD3.EventLog);
                var restored = new D12CollectionAtlasSystem(eventLog: restoredLog, initialState: loaded.CollectionAtlas);
                var duplicate = restored.Record(new D12CollectionRecordRequest
                {
                    ContentId = "reward_first_chart",
                    Category = D12CollectionCategory.Treasure,
                    FactEventId = source.EventId,
                    FactSaveRevision = source.SaveRevision,
                    EvidenceRef = "quest:first_chart:reward:01",
                    LocationRef = source.LocationRef,
                    WorldDay = source.WorldDay,
                    AcquisitionMethod = D12CollectionAcquisitionMethod.QuestReward,
                });
                Assert.IsTrue(duplicate.Duplicate);
                Assert.AreEqual(3, restoredLog.Count);
            }
            finally
            {
                SaveSystem.DbPath = null;
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    var path = dbPath + suffix;
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
        }

        private static FactEvent AppendFact(EventLog log, string eventId, int worldDay,
            string locationRef, long saveRevision, string owner)
        {
            return log.Append(new WorldEvent(
                eventId,
                worldDay,
                locationRef,
                owner,
                new EventResult
                {
                    ResultCode = "source_fact_confirmed",
                    Summary = "测试用所属系统事实。",
                    FactIds = new List<string> { "content:source" },
                },
                new[] { "player" },
                new[] { "input:source" },
                new string[0],
                saveRevision)).Fact;
        }
    }
}
