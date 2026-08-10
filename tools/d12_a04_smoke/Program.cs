using System;
using Whalebone.Systems.D3;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var log = new EventLog();
        var system = new D12CommunityTradeSystem(
            D12CommunityTradeCatalog.CreateVerticalSlice(),
            D12CommunityTradeCatalog.CreateBoundaries(), log);
        var inventory = new D3InventoryState(20, 40);
        Check(D3InventorySystem.TryAddCargo(inventory, "salt", 2, out _), "seed salt");

        var proposal = system.ProposeTrade(new D12CommunityTradeIntent
        {
            RequestId = "smoke-trade",
            CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
            ActorRef = "player",
            LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
            GiveItemId = "salt", GiveQuantity = 1,
            WantItemId = "cocoa", WantQuantity = 1,
            LanguageId = "green_cay_trade_tongue", LanguageLevel = 2,
            KnowledgeLevel = 0, Etiquette = D12CommunityEtiquette.GiftBeforeTrade,
            WorldDay = 1, SaveRevision = 1,
        });
        Check(proposal != null && log.Count == 0, "proposal boundary");
        var help = system.RequestHelp(new D12CommunityHelpRequest
        {
            RequestId = "smoke-help", CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
            ActorRef = "player", LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
            KnowledgeLevel = 0, WorldDay = 1, SaveRevision = 2,
        });
        Check(help.Succeeded && help.Code == "community_help_offered", "help");
        var knowledgeGate = proposal.Clone();
        knowledgeGate.RequestId = "smoke-knowledge-gate";
        knowledgeGate.SaveRevision = 3;
        var knowledgeResult = system.SettleTrade(knowledgeGate, inventory);
        Check(!knowledgeResult.Succeeded && knowledgeResult.Code == "knowledge_required", "knowledge gate");
        var etiquetteGate = proposal.Clone();
        etiquetteGate.RequestId = "smoke-etiquette-gate";
        etiquetteGate.KnowledgeLevel = help.CulturalKnowledgeAfter;
        etiquetteGate.Etiquette = D12CommunityEtiquette.DirectBargain;
        etiquetteGate.SaveRevision = 4;
        var etiquetteResult = system.SettleTrade(etiquetteGate, inventory);
        Check(!etiquetteResult.Succeeded && etiquetteResult.Code == "etiquette_mismatch", "etiquette gate");
        proposal.KnowledgeLevel = help.CulturalKnowledgeAfter;
        proposal.SaveRevision = 5;
        proposal.CauseEventIds.Add(help.EventId);
        var trade = system.SettleTrade(proposal, inventory);
        Check(trade.Succeeded && trade.Code == "community_trade_completed", "barter");
        Check(D3InventorySystem.CountCargo(inventory, "cocoa") == 1, "cargo output");
        Check(trade.Fact.CauseIds.Contains(help.EventId), "cause chain");
        var outOfStock = proposal.Clone();
        outOfStock.RequestId = "smoke-out-of-stock";
        outOfStock.WantItemId = "medicine";
        outOfStock.WantQuantity = 99;
        outOfStock.SaveRevision = 6;
        var outOfStockResult = system.SettleTrade(outOfStock, inventory);
        Check(!outOfStockResult.Succeeded && outOfStockResult.Code == "community_out_of_stock", "community stock");

        var snapshot = system.Snapshot;
        var restoredLog = EventLog.FromSnapshot(snapshot.EventLog);
        var restored = new D12CommunityTradeSystem(
            D12CommunityTradeCatalog.CreateVerticalSlice(),
            D12CommunityTradeCatalog.CreateBoundaries(), restoredLog, snapshot);
        var duplicate = restored.SettleTrade(proposal, inventory);
        Check(duplicate.Duplicate, "duplicate save protection");
        var protectedResult = restored.SettleTrade(new D12CommunityTradeIntent
        {
            RequestId = "smoke-protected", CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
            ActorRef = "player", LocationRef = D12CommunityTradeIds.GreenCayMangroveLocation,
            GiveItemId = "salt", GiveQuantity = 1, WantItemId = "cocoa", WantQuantity = 1,
            LanguageId = "green_cay_trade_tongue", LanguageLevel = 2, KnowledgeLevel = 3,
            Etiquette = D12CommunityEtiquette.GiftBeforeTrade, WorldDay = 2, SaveRevision = 4,
        }, inventory);
        Check(!protectedResult.Succeeded && protectedResult.Code == "protected_area_refused", "protected area");
        Console.WriteLine("D12-A04 smoke PASS: proposal/help/barter/boundary/save");
    }
}
