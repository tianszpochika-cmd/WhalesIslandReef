using System;
using Whalebone.Systems.D10;

internal static class Program
{
    private sealed class FakeAppearanceWriter : ID10AppearanceWriter
    {
        public string CurrentAppearanceLoadoutId { get; private set; } = "loadout:default";
        public bool TryApplyAppearance(string appearanceLoadoutId)
        {
            if (appearanceLoadoutId == "unknown") return false;
            CurrentAppearanceLoadoutId = appearanceLoadoutId;
            return true;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var preserved = new D10IdentityHistoryPreservationContext();
        preserved.LegalHistoryRefs.Add("legal:old");
        preserved.DebtHistoryRefs.Add("debt:old");
        preserved.ReputationHistoryRefs.Add("reputation:old");
        preserved.RelationHistoryRefs.Add("relation:old");
        var creation = new D10PlayerIdentityCreation
        {
            Name = "Amina",
            GenderExpressionKey = "gender_expression_feminine",
            Title = new D10LocalizedText("title.captain", "船长", "Captain"),
            AgeBandKey = "age_band_adult",
            CultureOriginKey = "culture_origin_afro_caribbean",
            IdentityNote = new D10LocalizedText("note.long", new string('海', 1000), new string('s', 1000)),
        };
        var state = D10PlayerIdentityState.CreateNew("player:smoke", creation, 1, preserved);
        Check(state.IdentityHistory.Count == 1 && state.IdentityHistory[0].PreviousName == "", "creation history");

        var writer = new FakeAppearanceWriter();
        var system = new D10PlayerIdentitySystem(state, writer);
        Check(!system.ApplyPortUpdate(new D10PortIdentityUpdateRequest { NewTitle = new D10LocalizedText("no", "不应", "No") }, false, "open_sea").Succeeded, "sea gate");
        var applied = system.ApplyPortUpdate(new D10PortIdentityUpdateRequest
        {
            NewTitle = new D10LocalizedText("title:witness", "见证人", "Witness"),
            AppearanceLoadoutId = "loadout:port",
            Document = new D10IdentityDocumentRecord { DocumentTypeKey = "document.port_register" },
            WorldDay = 8,
        }, true, "santa_margarita", new D10IdentityHistoryPreservationContext { LegalHistoryRefs = { "legal:new" } });
        Check(applied.Succeeded, "port update");
        Check(applied.Identity.IdentityHistory.Count == 2, "history append");
        Check(applied.Identity.Documents.Count == 1, "document");
        Check(applied.Identity.PreservedHistory.LegalHistoryRefs.Count == 2, "legal refs preserved");
        Check(applied.Identity.PreservedHistory.DebtHistoryRefs.Contains("debt:old"), "debt ref preserved");
        Check(!system.ApplyPortUpdate(new D10PortIdentityUpdateRequest { AppearanceLoadoutId = "unknown" }, true, "santa_margarita").Succeeded, "unknown appearance");
        Check(system.Snapshot.Title.Key == "title:witness", "failed update must not partially write");
        Console.WriteLine("D10-A04 smoke PASS: creation/port-gate/update/history/localized-long-text/rejection");
    }
}
