// D13-A07 动态市场与运输契约。
// EconomySystem 写价格/市场，Inventory/Cargo 写库存，D13 系统只编排来源化商品流并写运输状态/损耗。

using System;
using System.Collections.Generic;
using System.Linq;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13MarketTransportIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "D13MarketTransportSystem";
        public const string TransportOwner = "TransportSystem";
        public const string EconomyOwner = "EconomySystem";
        public const string InventoryOwner = "D3InventorySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string EnvironmentOwner = "EnvironmentSystem";
        public const string EcologyOwner = "EcologySystem";
        public const string ShipOwner = "ShipSystem";
        public const string PropertyOwner = "PropertySystem";
        public const string Currency = "coin";
        public const string PlayerRef = "player";
        public const string MarketRecoveryRoute = "route_market_price_review";
        public const string InventoryRecoveryRoute = "route_market_inventory_review";
        public const string LegalRecoveryRoute = "route_market_legal_review";
        public const string ProductionRecoveryRoute = "route_market_production_review";
        public const string ConsumptionRecoveryRoute = "route_market_consumption_review";
        public const string TransportRecoveryRoute = "route_market_transport_review";
    }

    public enum D13MarketGoodCategory
    {
        FishCatch,
        FarmProduce,
        ProcessedProduct,
        ShipMaterial,
        Clothing,
        PetSupply,
        Tool,
        Medicine,
        Special,
        Toxin,
    }

    public enum D13MarketLegalStatus
    {
        Legal,
        Licensed,
        Restricted,
        Illegal,
    }

    public enum D13MarketFlowKind
    {
        Production,
        Consumption,
        Sale,
    }

    public enum D13TransportStatus
    {
        Loaded,
        InTransit,
        Arrived,
        Lost,
        Held,
        Cancelled,
    }

    [Serializable]
    public sealed class D13MarketGoodDefinition
    {
        public string GoodId;
        public string ItemId;
        public string DisplayName;
        public D13MarketGoodCategory Category;
        public int BasePrice;
        public int UnitWeight = 1;
        public int UnitVolume = 1;
        public int DemandTarget = 50;
        public int FragilityPercent;
        public D13MarketLegalStatus LegalStatus = D13MarketLegalStatus.Legal;
        public bool RequiresPermit;
        public string PermitId;
        public bool Transportable = true;
        public List<string> AllowedProducerOwners = new List<string>();
        public List<string> Tags = new List<string>();

        public D13MarketGoodDefinition Clone()
        {
            var result = (D13MarketGoodDefinition)MemberwiseClone();
            result.AllowedProducerOwners = AllowedProducerOwners == null ? new List<string>() : new List<string>(AllowedProducerOwners);
            result.Tags = Tags == null ? new List<string>() : new List<string>(Tags);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(GoodId) || string.IsNullOrWhiteSpace(ItemId) || string.IsNullOrWhiteSpace(DisplayName) ||
                !Enum.IsDefined(typeof(D13MarketGoodCategory), Category) || BasePrice <= 0 || UnitWeight <= 0 || UnitVolume <= 0 ||
                DemandTarget <= 0 || FragilityPercent < 0 || FragilityPercent > 100 || AllowedProducerOwners == null ||
                Tags == null || Tags.Count == 0 || RequiresPermit && string.IsNullOrWhiteSpace(PermitId))
                throw new InvalidOperationException("D13 market good definition is invalid: " + GoodId);
            if (!Enum.IsDefined(typeof(D13MarketLegalStatus), LegalStatus)) throw new InvalidOperationException("D13 market legal status is invalid: " + GoodId);
            if (AllowedProducerOwners.Count == 0) throw new InvalidOperationException("D13 market good needs a producer owner: " + GoodId);
        }
    }

    [Serializable]
    public sealed class D13MarketRouteDefinition
    {
        public string RouteId;
        public string OriginLocationId;
        public string DestinationLocationId;
        public int CapacityUnits = 20;
        public int BaseLossPercent;
        public bool RequiresShip = true;
        public List<string> Tags = new List<string>();

        public D13MarketRouteDefinition Clone()
        {
            var result = (D13MarketRouteDefinition)MemberwiseClone();
            result.Tags = Tags == null ? new List<string>() : new List<string>(Tags);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RouteId) || string.IsNullOrWhiteSpace(OriginLocationId) || string.IsNullOrWhiteSpace(DestinationLocationId) ||
                OriginLocationId == DestinationLocationId || CapacityUnits <= 0 || BaseLossPercent < 0 || BaseLossPercent > 50 || Tags == null || Tags.Count == 0)
                throw new InvalidOperationException("D13 market route definition is invalid: " + RouteId);
        }
    }

    [Serializable]
    public class D13MarketOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13MarketTransportIds.PlayerRef;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D13MarketQuoteRequest : D13MarketOperationRequest
    {
        public string MarketId;
        public string ItemId;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;
    }

    [Serializable]
    public sealed class D13MarketProductionRequest : D13MarketOperationRequest
    {
        public string ItemId;
        public int RequestedQuantity;
        public string SourceLocationId;
        public string InventoryHolderId;
        public string MarketId;
        public string PermitId;
    }

    [Serializable]
    public sealed class D13MarketConsumptionRequest : D13MarketOperationRequest
    {
        public string ItemId;
        public int Quantity;
        public string ConsumerId;
        public string LocationId;
        public string InventoryHolderId;
        public string MarketId;
        public string PermitId;
        public string ConsumptionReason;
    }

    [Serializable]
    public sealed class D13MarketSaleRequest : D13MarketOperationRequest
    {
        public string ItemId;
        public int Quantity;
        public string SellerId;
        public string LocationId;
        public string InventoryHolderId;
        public string MarketId;
        public string PermitId;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;
    }

    [Serializable]
    public sealed class D13MarketShipmentRequest : D13MarketOperationRequest
    {
        public string RouteId;
        public string ItemId;
        public int Quantity;
        public string InventoryHolderId;
        public string PermitId;
        public string ShipId;
    }

    [Serializable]
    public sealed class D13MarketShipmentAdvanceRequest : D13MarketOperationRequest
    {
        public string ShipmentId;
        public int Days = 1;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public int AssetConditionPercent = 100;
        public int EcologyPressurePercent;
        public string WeatherSourceEventId;
        public string AssetSourceEventId;
        public string EcologySourceEventId;
    }

    [Serializable]
    public sealed class D13MarketShipmentArrivalRequest : D13MarketOperationRequest
    {
        public string ShipmentId;
        public string InventoryHolderId;
    }

    [Serializable]
    public sealed class D13MarketInventoryMutationRequest
    {
        public string OperationId;
        public string HolderId;
        public string ItemId;
        public int Quantity;
        public bool Add;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13MarketFlowRequest : D13MarketOperationRequest
    {
        public D13MarketFlowKind Kind;
        public string ItemId;
        public int Quantity;
        public string MarketId;
        public D3WeatherType Weather = D3WeatherType.Calm;
        public string FactionPolicyId = "neutral";
        public float FactionPolicyCoefficient = 1f;
    }

    [Serializable]
    public sealed class D13MarketLegalDecision
    {
        public bool Allowed;
        public string Code = "legal_denied";
        public string Message;
        public D13MarketLegalStatus LegalStatus;
        public string PermitId;
        public string SourceEventId;
        public string SourceOwnerSystem = D13MarketTransportIds.LegalOwner;
    }

    [Serializable]
    public sealed class D13MarketProductionDecision
    {
        public bool Allowed;
        public string Code = "production_denied";
        public string Message;
        public int Quantity;
        public string SourceEventId;
        public string SourceOwnerSystem;
        public string EcologySourceEventId;
        public string AssetSourceEventId;
    }

    [Serializable]
    public sealed class D13MarketConsumptionDecision
    {
        public bool Allowed;
        public string Code = "consumption_denied";
        public string Message;
        public string SourceEventId;
        public string SourceOwnerSystem;
    }

    [Serializable]
    public sealed class D13MarketInventoryDecision
    {
        public bool Allowed;
        public string Code = "inventory_denied";
        public string Message;
        public string ItemId;
        public int Quantity;
        public string SourceEventId;
        public string SourceOwnerSystem = D13MarketTransportIds.InventoryOwner;
    }

    [Serializable]
    public sealed class D13MarketQuoteDecision
    {
        public bool Allowed;
        public string Code = "quote_denied";
        public string Message;
        public string MarketId;
        public string ItemId;
        public int UnitPrice;
        public string Currency = D13MarketTransportIds.Currency;
        public string SourceEventId;
        public string MarketSourceEventId;
        public string SourceOwnerSystem = D13MarketTransportIds.EconomyOwner;
        public List<PriceReason> Reasons = new List<PriceReason>();
    }

    [Serializable]
    public sealed class D13MarketFlowDecision
    {
        public bool Allowed;
        public string Code = "market_flow_denied";
        public string Message;
        public D13MarketFlowKind Kind;
        public string MarketId;
        public string ItemId;
        public int Quantity;
        public int UnitPrice;
        public long TotalValue;
        public string Currency = D13MarketTransportIds.Currency;
        public string LedgerRef;
        public string SourceEventId;
        public string MarketSourceEventId;
        public string SourceOwnerSystem = D13MarketTransportIds.EconomyOwner;
        public int MarketStockAfter;
        public int MarketDemandAfter;
    }

    [Serializable]
    public sealed class D13TransportAssetDecision
    {
        public bool Allowed;
        public string Code = "transport_asset_denied";
        public string Message;
        public int ConditionPercent = 100;
        public string SourceEventId;
        public string SourceOwnerSystem = D13MarketTransportIds.ShipOwner;
    }

    public interface ID13MarketLegalAuthority
    {
        D13MarketLegalDecision ValidateGood(D13MarketGoodDefinition definition, string locationId, string permitId, D13MarketOperationRequest request);
        D13MarketLegalDecision ValidateRoute(D13MarketRouteDefinition route, string permitId, D13MarketShipmentRequest request);
    }

    public interface ID13MarketProductionAuthority
    {
        D13MarketProductionDecision ValidateProduction(D13MarketGoodDefinition definition, D13MarketProductionRequest request);
    }

    public interface ID13MarketConsumptionAuthority
    {
        D13MarketConsumptionDecision ValidateConsumption(D13MarketGoodDefinition definition, D13MarketConsumptionRequest request);
    }

    public interface ID13MarketInventoryAuthority
    {
        D13MarketInventoryDecision Apply(D13MarketInventoryMutationRequest request);
    }

    public interface ID13MarketEconomyAuthority
    {
        D13MarketQuoteDecision Quote(D13MarketGoodDefinition definition, D13MarketQuoteRequest request);
        D13MarketFlowDecision ApplyFlow(D13MarketGoodDefinition definition, D13MarketFlowRequest request);
    }

    public interface ID13TransportAssetAuthority
    {
        D13TransportAssetDecision ValidateTransportAsset(D13MarketRouteDefinition route, D13MarketShipmentAdvanceRequest request);
    }

    [Serializable]
    public sealed class D13MarketFlowRecord
    {
        public string FlowId;
        public D13MarketFlowKind Kind;
        public string ItemId;
        public int Quantity;
        public string OriginLocationId;
        public string DestinationLocationId;
        public string MarketId;
        public int UnitPrice;
        public long TotalValue;
        public string Currency = D13MarketTransportIds.Currency;
        public string LedgerRef;
        public string ProducerSourceEventId;
        public string ConsumerSourceEventId;
        public string InventorySourceEventId;
        public string EconomySourceEventId;
        public string MarketSourceEventId;
        public string LegalSourceEventId;
        public string EcologySourceEventId;
        public string AssetSourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13MarketFlowRecord Clone() { return (D13MarketFlowRecord)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FlowId) || !Enum.IsDefined(typeof(D13MarketFlowKind), Kind) || string.IsNullOrWhiteSpace(ItemId) || Quantity <= 0 ||
                string.IsNullOrWhiteSpace(InventorySourceEventId) || string.IsNullOrWhiteSpace(EconomySourceEventId) || string.IsNullOrWhiteSpace(MarketSourceEventId) ||
                string.IsNullOrWhiteSpace(LegalSourceEventId) || UnitPrice < 0 || TotalValue < 0 || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException("D13 market flow record is invalid: " + FlowId);
        }
    }

    [Serializable]
    public sealed class D13TransportShipmentState
    {
        public string ShipmentId;
        public string RouteId;
        public string ItemId;
        public string OriginLocationId;
        public string DestinationLocationId;
        public string ShipId;
        public int RequestedQuantity;
        public int RemainingQuantity;
        public int LostQuantity;
        public int DeliveredQuantity;
        public D13TransportStatus Status = D13TransportStatus.Loaded;
        public int LastWeatherRiskPercent;
        public int LastAssetRiskPercent;
        public int LastEcologyRiskPercent;
        public string LegalSourceEventId;
        public string InventoryLoadSourceEventId;
        public string InventoryArrivalSourceEventId;
        public string AssetSourceEventId;
        public string LastEventId;
        public int CreatedDay;
        public int LastUpdatedDay;
        public long Revision;

        public D13TransportShipmentState Clone() { return (D13TransportShipmentState)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ShipmentId) || string.IsNullOrWhiteSpace(RouteId) || string.IsNullOrWhiteSpace(ItemId) ||
                string.IsNullOrWhiteSpace(OriginLocationId) || string.IsNullOrWhiteSpace(DestinationLocationId) || RequestedQuantity <= 0 ||
                RemainingQuantity < 0 || LostQuantity < 0 || DeliveredQuantity < 0 || RemainingQuantity + LostQuantity + DeliveredQuantity != RequestedQuantity ||
                !Enum.IsDefined(typeof(D13TransportStatus), Status) || LastWeatherRiskPercent < 0 || LastWeatherRiskPercent > 100 ||
                LastAssetRiskPercent < 0 || LastAssetRiskPercent > 100 || LastEcologyRiskPercent < 0 || LastEcologyRiskPercent > 100 ||
                string.IsNullOrWhiteSpace(LegalSourceEventId) || string.IsNullOrWhiteSpace(InventoryLoadSourceEventId) || CreatedDay < 0 || LastUpdatedDay < 0 || Revision < 0)
                throw new InvalidOperationException("D13 transport shipment is invalid: " + ShipmentId);
        }
    }

    [Serializable]
    public sealed class D13DynamicMarketRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13MarketTransportIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13MarketFlowRecord> Flows = new List<D13MarketFlowRecord>();
        public List<D13TransportShipmentState> Shipments = new List<D13TransportShipmentState>();
        public List<string> AppliedEventKeys = new List<string>();

        public D13DynamicMarketRuntimeSnapshot Clone()
        {
            var result = new D13DynamicMarketRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Flows != null) foreach (var item in Flows) if (item != null) result.Flows.Add(item.Clone());
            if (Shipments != null) foreach (var item in Shipments) if (item != null) result.Shipments.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Flows == null || Shipments == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D13 dynamic market snapshot is invalid.");
            var flowIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Flows) { if (item == null || !flowIds.Add(item.FlowId)) throw new InvalidOperationException("D13 dynamic market flow IDs are invalid."); item.Validate(); }
            var shipmentIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Shipments) { if (item == null || !shipmentIds.Add(item.ShipmentId)) throw new InvalidOperationException("D13 shipment IDs are invalid."); item.Validate(); }
        }
    }

    [Serializable]
    public sealed class D13MarketResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string RecoveryRouteId;
        public D13MarketFlowRecord Flow;
        public D13TransportShipmentState Shipment;
        public D13MarketQuoteDecision Quote;
        public FactEvent Fact;
    }

    [Serializable]
    public sealed class D13DynamicMarketReadModel
    {
        public long Revision;
        public List<D13MarketGoodDefinition> Goods = new List<D13MarketGoodDefinition>();
        public List<D13MarketRouteDefinition> Routes = new List<D13MarketRouteDefinition>();
        public List<D13MarketFlowRecord> Flows = new List<D13MarketFlowRecord>();
        public List<D13TransportShipmentState> Shipments = new List<D13TransportShipmentState>();
    }
}
