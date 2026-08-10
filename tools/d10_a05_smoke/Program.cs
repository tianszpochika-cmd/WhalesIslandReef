using System;
using Whalebone.Systems.D3;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var system = new D10AppearanceSystem();
        Check(system.Snapshot.ActiveLoadout.Get(D10AppearanceSlot.Body) == "outfit_body_sun_bronzed", "starter composition");
        var preview = system.TryOn("outfit_coat_rain_oilskin");
        Check(preview.Success && preview.Code == D10AppearanceResultCode.MissingResourcePlaceholder && preview.UsesPlaceholder,
            "missing resource placeholder");
        Check(!system.TryOn("outfit_accessory_compass_badge").Success, "career restriction");

        var log = new EventLog();
        var economy = new TradeEconomySystem(log);
        var trade = new D3TradeState(500, 100);
        var ready = new D10AppearanceSystem(null, economy, trade, id => true);
        Check(ready.Purchase("outfit_coat_rain_oilskin").Success, "purchase");
        Check(ready.Equip("outfit_coat_rain_oilskin").Success, "equip");
        Check(ready.Dye("outfit_coat_rain_oilskin", "item_dye_indigo", "indigo").Success, "dye");
        var damaged = ready.Snapshot;
        damaged.Owned["outfit_coat_rain_oilskin"].Durability = 15;
        ready = new D10AppearanceSystem(damaged, economy, trade, id => true);
        Check(ready.Repair("outfit_coat_rain_oilskin").Success, "repair");
        ready.ApplyInjuryAppearance(new[] { "injured_leg" });
        Check(ready.Snapshot.InjuryHistoryIds.Contains("injured_leg"), "injury history");
        Check(trade.Money == 430 && log.Count == 3, "economy/event ownership");
        Console.WriteLine("D10-A05 smoke PASS: composition/try-on/purchase/equip/dye/repair/restriction/injury");
    }
}
