using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;
using Whalebone.Systems.D5;

namespace D10.Tests
{
    public class D10AppearanceOutfitTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_appearance_" + Guid.NewGuid().ToString("N") + ".db");
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
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void StarterLoadout_ComposesBodyHairAndClothingLayers()
        {
            var system = new D10AppearanceSystem();
            var snapshot = system.Snapshot;

            Assert.AreEqual("outfit_body_sun_bronzed", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Body));
            Assert.AreEqual("outfit_hair_short_brown", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Hair));
            Assert.AreEqual("outfit_hat_straw_deck", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Hat));
            Assert.AreEqual("outfit_top_linen_deckhand", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Top));
            Assert.AreEqual("outfit_bottom_canvas_trousers", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Bottom));
            Assert.AreEqual("outfit_shoes_deck_boots", snapshot.ActiveLoadout.Get(D10AppearanceSlot.Shoes));
            Assert.IsEmpty(snapshot.ActiveLoadout.Get(D10AppearanceSlot.Injury));
            Assert.GreaterOrEqual(D10AppearanceCatalog.All.Count, 14);
        }

        [Test]
        public void TryOn_DoesNotGrantOwnershipOrChangeActiveLoadout()
        {
            var system = new D10AppearanceSystem();
            var before = system.Snapshot;
            var result = system.TryOn("outfit_coat_rain_oilskin");
            var after = system.Snapshot;

            Assert.IsTrue(result.Success);
            Assert.AreEqual(D10AppearanceResultCode.MissingResourcePlaceholder, result.Code,
                "资源缺失必须明确暴露占位状态；试穿仍可生成结构预览。");
            Assert.IsTrue(result.UsesPlaceholder);
            Assert.IsFalse(after.Owned.ContainsKey("outfit_coat_rain_oilskin"));
            Assert.AreEqual(before.ActiveLoadout.Get(D10AppearanceSlot.Coat),
                after.ActiveLoadout.Get(D10AppearanceSlot.Coat));
            Assert.AreEqual(1, result.Fact == null ? 0 : 1);
        }

        [Test]
        public void Tailor_PurchaseDyeRepairAndEquip_UseEconomyForMoney()
        {
            var eventLog = new EventLog();
            var economy = new TradeEconomySystem(eventLog);
            var trade = new D3TradeState(500, 100);
            var system = new D10AppearanceSystem(null, economy, trade, delegate { return true; });

            var purchase = system.Purchase("outfit_coat_rain_oilskin");
            Assert.IsTrue(purchase.Success);
            Assert.AreEqual(446, trade.Money);
            Assert.IsTrue(system.Snapshot.Owned.ContainsKey("outfit_coat_rain_oilskin"));
            Assert.IsTrue(system.Equip("outfit_coat_rain_oilskin").Success);
            Assert.IsTrue(system.Dye("outfit_coat_rain_oilskin", "item_dye_indigo", "indigo").Success);

            var damaged = system.Snapshot;
            damaged.Owned["outfit_coat_rain_oilskin"].Durability = 15;
            system = new D10AppearanceSystem(damaged, economy, trade, delegate { return true; });
            var repair = system.Repair("outfit_coat_rain_oilskin");

            Assert.IsTrue(repair.Success);
            Assert.AreEqual(100, system.Snapshot.Owned["outfit_coat_rain_oilskin"].Durability);
            Assert.AreEqual(430, trade.Money);
            Assert.AreEqual(1, CountFacts(eventLog, D10AppearanceSystem.PurchaseEventId));
            Assert.AreEqual(1, CountFacts(eventLog, D10AppearanceSystem.DyeEventId));
            Assert.AreEqual(1, CountFacts(eventLog, D10AppearanceSystem.RepairEventId));
        }

        [Test]
        public void RestrictionsAndInjuryHistory_AreExplicitAndPersistent()
        {
            var system = new D10AppearanceSystem();
            var career = system.TryOn("outfit_accessory_compass_badge");
            Assert.IsFalse(career.Success);
            Assert.AreEqual(D10AppearanceResultCode.CareerRestricted, career.Code);
            Assert.That(career.Message, Does.Contain("outfit_accessory_compass_badge"));

            system.ApplyInjuryAppearance(new[] { "injured_leg" });
            Assert.AreEqual("outfit_injury_old_leg", system.Snapshot.ActiveLoadout.Get(D10AppearanceSlot.Injury));
            Assert.IsTrue(system.Equip("outfit_hat_straw_deck").Success);
            var snapshot = system.Snapshot;
            Assert.Contains("injured_leg", snapshot.ActiveInjuryIds);
            Assert.Contains("injured_leg", snapshot.InjuryHistoryIds);

            var blocked = system.Equip("outfit_injury_bandaged_arm");
            Assert.IsFalse(blocked.Success);
            Assert.AreEqual(D10AppearanceResultCode.InjuryProtected, blocked.Code);

            var combination = system.Snapshot;
            combination.Context.CareerId = "navigator";
            combination.ActiveLoadout.Set(D10AppearanceSlot.Coat, "outfit_coat_rain_oilskin");
            combination.Owned["outfit_coat_rain_oilskin"] = new D10OwnedAppearance
            {
                ItemId = "outfit_coat_rain_oilskin", Durability = 100,
            };
            combination.Owned["outfit_ceremonial_silk_coat"] = new D10OwnedAppearance
            {
                ItemId = "outfit_ceremonial_silk_coat", Durability = 100,
            };
            var combinationSystem = new D10AppearanceSystem(combination);
            var conflict = combinationSystem.Equip("outfit_ceremonial_silk_coat");
            Assert.IsFalse(conflict.Success);
            Assert.AreEqual(D10AppearanceResultCode.SlotConflict, conflict.Code);
            Assert.That(conflict.Message, Does.Contain("outfit_ceremonial_silk_coat"));
        }

        [Test]
        public void AppearanceSnapshot_SaveAndLoad_RestoresOwnershipDyeAndInjuryHistory()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D10 Appearance Captain" });
            var economy = new TradeEconomySystem();
            var trade = new D3TradeState(500, 100);
            var system = new D10AppearanceSystem(null, economy, trade, delegate { return true; });
            Assert.IsTrue(system.Purchase("outfit_coat_rain_oilskin").Success);
            Assert.IsTrue(system.Dye("outfit_hat_straw_deck", "item_dye_indigo", "indigo").Success);
            system.ApplyInjuryAppearance(new[] { "scarred_arm" });

            var d5 = new D5RuntimeSnapshot { Appearance = system.Snapshot };
            SaveSystem.SaveD5Runtime(d5);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());

            var restored = SaveSystem.LoadD5Runtime();
            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.Appearance.Owned.ContainsKey("outfit_coat_rain_oilskin"));
            Assert.AreEqual("item_dye_indigo", restored.Appearance.Owned["outfit_hat_straw_deck"].DyeId);
            Assert.Contains("scarred_arm", restored.Appearance.InjuryHistoryIds);
            Assert.AreEqual("outfit_injury_bandaged_arm",
                restored.Appearance.ActiveLoadout.Get(D10AppearanceSlot.Injury));
        }

        private static int CountFacts(EventLog log, string eventId)
        {
            var count = 0;
            foreach (var fact in log.Facts) if (fact.EventId == eventId) count++;
            return count;
        }
    }
}
