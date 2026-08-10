// D13-A04 对 D13-A01 PropertySystem 的只读牧场资格适配。
// 产权规则仍由既有 D13FarmingPropertyAuthority/PropertySystem 裁决，不在 RanchSystem 复制。

using Whalebone.Systems.D13;

namespace Whalebone.Systems.D13
{
    public sealed class D13RanchPropertyAuthority : ID13RanchLandAuthority
    {
        private readonly D13FarmingPropertyAuthority _delegate;

        public D13RanchPropertyAuthority(D13PropertyAssetSystem propertySystem)
        {
            _delegate = new D13FarmingPropertyAuthority(propertySystem);
        }

        public D13RanchLandDecision ValidateFacility(D13RanchFacilityDefinition definition, string actorRef, int worldDay)
        {
            if (definition == null) return new D13RanchLandDecision { Allowed = false, Code = "property_required", Message = "牧场缺少产权定义。" };
            var farmingDecision = _delegate.ValidatePlot(new D13FarmingPlotDefinition
            {
                PlotId = definition.RanchId,
                PropertyId = definition.PropertyId,
                DisplayName = definition.DisplayName,
                SoilType = "loam",
                WaterSourceId = definition.WaterSourceId,
                AreaUnits = 1,
                Tags = definition.Tags,
            }, actorRef, worldDay);
            return new D13RanchLandDecision
            {
                Allowed = farmingDecision.Allowed,
                Code = farmingDecision.Code,
                Message = farmingDecision.Message,
                PropertySourceEventId = farmingDecision.PropertySourceEventId,
                LegalSourceEventId = farmingDecision.LegalSourceEventId,
            };
        }
    }
}
