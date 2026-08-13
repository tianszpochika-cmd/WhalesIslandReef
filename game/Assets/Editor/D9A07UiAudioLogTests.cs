using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Whalebone.Gameplay.D9;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace D9.EditorTests
{
    public class D9A07UiAudioLogTests
    {
        [SetUp]
        public void SetUp()
        {
            D9EventLogBridge.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            D9EventLogBridge.Clear();
        }

        [Test]
        public void ReadModelUsesSnapshotsAndUnifiedFactsOnly()
        {
            var log = new EventLog();
            log.Append(NewEvent("event_d9_fishing_catch", "近海鱼获事实已记录。", 4));
            var d3 = new D3RuntimeSnapshot
            {
                CurrentLocationId = "nearshore_saint_marys",
                DestinationId = "green_cay",
                VoyageStatus = D3VoyageStatus.Sailing,
                Weather = D3WeatherType.Storm,
                MarketStock = new Dictionary<string, int> { { "salt", 6 } },
            };
            var preparation = new D9PortShipPreparationSnapshot
            {
                PortId = "santa_margarita",
                Money = 321,
                Ship = new D9ShipPreparationSnapshot
                {
                    Weight = 12,
                    CargoCapacity = 40,
                    Draft = 0.8f,
                    CrewCount = 1,
                    CrewCapacity = 4,
                    Damage = 3,
                    Water = 18,
                    Food = 20,
                    Moored = false,
                },
            };

            var view = D9PresentationReadModel.Build(d3, new D4RuntimeSnapshot(), preparation, log, null);

            StringAssert.Contains("圣玛丽近海鱼场", view.Location);
            StringAssert.Contains("航行中", view.VoyageStatus);
            StringAssert.Contains("风暴", view.Weather);
            StringAssert.Contains("近海鱼获事实", view.Catch);
            StringAssert.Contains("salt 库存 6", view.Market);
            StringAssert.Contains("重量 12/40", view.Ship);
            Assert.AreEqual(1, view.Facts.Count);
            Assert.AreEqual(1, log.Count);
        }

        [Test]
        public void CursorSkipsRestoredHistoryAndConsumesEachNewFactOnce()
        {
            var log = new EventLog();
            log.Append(NewEvent("event_d9_departure", "离港事实已记录。", 1));
            var cursor = new D9EventFactCursor();
            cursor.Bind(log);
            Assert.AreEqual(0, cursor.ConsumeNewFacts().Count);

            log.Append(NewEvent("event_d9_weather_storm", "风暴事实已记录。", 2));
            Assert.AreEqual(1, cursor.ConsumeNewFacts().Count);
            Assert.AreEqual(0, cursor.ConsumeNewFacts().Count);

            var restored = EventLog.FromSnapshot(log.CaptureSnapshot());
            var afterReload = new D9EventFactCursor();
            afterReload.Bind(restored);
            Assert.AreEqual(0, afterReload.ConsumeNewFacts().Count);
            restored.Append(NewEvent("event_d9_returned_to_harbor", "返港事实已记录。", 3));
            Assert.AreEqual(1, afterReload.ConsumeNewFacts().Count);
        }

        [Test]
        public void CueRouterCoversFrozenD9EventChainWithoutUnknownCues()
        {
            var ids = new[]
            {
                "event_d9_departure_prepared", "event_d9_departure", "event_d9_fishing_catch",
                "event_d9_weather_storm", "event_d9_crew_report", "event_d9_nearshore_patrol",
                "event_d9_nearshore_retreat", "event_d9_treasure_clue", "event_d9_treasure_resolved",
                "event_d9_returned_to_harbor", "event_d9_trade_settled", "event_d9_wage_settled",
                "event_d9_ship_repaired", "event_d9_save_checkpoint",
            };
            foreach (var id in ids) Assert.IsNotEmpty(D9AudioCueRouter.CueForEvent(id), id);
            Assert.IsEmpty(D9AudioCueRouter.CueForEvent("event_private_ui_fact"));
        }

        [Test]
        public void UnityMockCanMountReadOnlyHudAndAudioControllers()
        {
            var root = new GameObject("D9A07MockRoot");
            try
            {
                var hud = root.AddComponent<D9VoyageHudController>();
                var audio = root.AddComponent<D9AudioPresentationController>();
                // Unity 6 EditMode 下，临时 AddComponent 不保证自动派发私有 Awake；
                // 这个测试验证的是组件能否挂载并完成只读表现初始化，因此显式补齐生命周期。
                if (!hud.IsReady) InvokeAwakeForEditMode(hud);
                if (!audio.IsReady) InvokeAwakeForEditMode(audio);
                Assert.IsTrue(hud.IsReady);
                Assert.IsTrue(audio.IsReady);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void InvokeAwakeForEditMode(MonoBehaviour component)
        {
            var method = component.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, component.GetType().Name + " must provide Awake for EditMode mounting.");
            method.Invoke(component, null);
        }

        private static WorldEvent NewEvent(string id, string summary, long revision)
        {
            return new WorldEvent
            {
                EventId = id,
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = (int)revision,
                LocationRef = "nearshore_saint_marys",
                ActorIds = new List<string> { "player_001" },
                InputFacts = new List<string> { "d9_sample" },
                OwnerSystem = "D9TestOwner",
                Result = new EventResult
                {
                    ResultCode = "d9_test_fact",
                    Summary = summary,
                    FactIds = new List<string> { "fact_d9_test" },
                },
                CauseIds = new List<string>(),
                SaveRevision = revision,
            };
        }
    }
}
