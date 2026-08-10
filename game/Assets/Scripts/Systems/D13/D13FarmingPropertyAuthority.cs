// D13-A03 对 D13-A01 PropertySystem 的只读农业资格适配器。
// 它不写产权，只读取已裁决的所有权、租约、共同治理和法律状态。

using System;

namespace Whalebone.Systems.D13
{
    public sealed class D13FarmingPropertyAuthority : ID13FarmingLandAuthority
    {
        private readonly D13PropertyAssetSystem _propertySystem;

        public D13FarmingPropertyAuthority(D13PropertyAssetSystem propertySystem)
        {
            _propertySystem = propertySystem;
        }

        public D13FarmingLandDecision ValidatePlot(D13FarmingPlotDefinition definition, string actorRef, int worldDay)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.PropertyId))
                return Deny("property_required", "农业地块缺少产权引用。");
            if (_propertySystem == null) return Deny("property_authority_required", "必须由 PropertySystem 提供农业产权资格。");
            var propertyDefinition = _propertySystem.FindDefinition(definition.PropertyId);
            var state = _propertySystem.FindState(definition.PropertyId);
            if (propertyDefinition == null || state == null) return Deny("property_missing", "农业地块引用的产权不存在。");
            if (propertyDefinition.Kind != D13PropertyKind.Farmland && propertyDefinition.Kind != D13PropertyKind.LandParcel)
                return Deny("property_not_farmable", "当前产权类型不是可耕地或地块。");
            if (definition.ProtectedArea || propertyDefinition.ProtectedArea || state.LegalStatus == D13PropertyLegalStatus.Protected)
                return Deny("protected_farm_area", "该地块受保护区规则约束，当前不能进行农业操作。");
            if (state.DisputeStatus == D13PropertyDisputeStatus.Open || state.Ownership == D13OwnershipKind.Disputed)
                return Deny("property_disputed", "产权争议未解决，不能在该地块经营。");

            string actor = string.IsNullOrWhiteSpace(actorRef) ? D13FarmingIds.PlayerRef : actorRef;
            bool hasRight = string.Equals(actor, state.OwnerRef, StringComparison.Ordinal) ||
                string.Equals(actor, state.ControllerRef, StringComparison.Ordinal) ||
                string.Equals(actor, state.LeaseHolderRef, StringComparison.Ordinal) ||
                (state.CoGovernanceRefs != null && state.CoGovernanceRefs.Contains(actor));
            if (!hasRight) return Deny("farming_right_required", "只有产权所有者、租户、控制者或共同治理成员可以经营地块。");
            if (string.IsNullOrWhiteSpace(state.LastEventId) && string.IsNullOrWhiteSpace(state.LegalSourceEventId))
                return Deny("property_source_missing", "农业资格缺少产权/法律来源事实。");
            return new D13FarmingLandDecision
            {
                Allowed = true,
                Code = "farming_right_approved",
                Message = "PropertySystem 已确认农业经营资格。",
                ControllerRef = state.ControllerRef,
                PropertySourceEventId = state.LastEventId,
                LegalSourceEventId = state.LegalSourceEventId,
            };
        }

        private static D13FarmingLandDecision Deny(string code, string message)
        {
            return new D13FarmingLandDecision { Allowed = false, Code = code, Message = message };
        }
    }
}
