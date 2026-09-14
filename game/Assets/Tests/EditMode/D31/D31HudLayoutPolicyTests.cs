using NUnit.Framework;
using Whalebone.Gameplay.D29;

namespace Whalebone.Tests.D31
{
    /// <summary>
    /// D25/D29 HUD 的纯逻辑布局测试；不启动场景、不读取存档、不替代 Player 证据。
    /// </summary>
    public sealed class D31HudLayoutPolicyTests
    {
        [Test]
        public void ReferenceLayoutFitsViewportWithoutUnexpectedOverlap()
        {
            Assert.That(D29InteractionLayoutPolicy.ValidateReferenceLayout(), Is.Empty);
        }

        [Test]
        public void QuickActionStackFitsInsideQuickActionsPanel()
        {
            var panel = D29InteractionLayoutPolicy.QuickActionsRect;
            var buttons = D29InteractionLayoutPolicy.QuickActionButtonStackRect;

            Assert.That(buttons.Left, Is.GreaterThanOrEqualTo(panel.Left));
            Assert.That(buttons.Top, Is.GreaterThanOrEqualTo(panel.Top));
            Assert.That(buttons.Right, Is.LessThanOrEqualTo(panel.Right));
            Assert.That(buttons.Bottom, Is.LessThanOrEqualTo(panel.Bottom));
            Assert.That(D29InteractionLayoutPolicy.QuickActionButtonCount, Is.EqualTo(8));
            Assert.That(D29InteractionLayoutPolicy.QuickActionButtonStep,
                Is.GreaterThanOrEqualTo(D29InteractionLayoutPolicy.QuickActionButtonHeight));
        }

        [Test]
        public void WarehouseModalUsesDistinctObjectAndCoordinatorSurfaceNames()
        {
            Assert.That(D29InteractionLayoutPolicy.WarehouseObjectName,
                Is.EqualTo("WarehousePanel"));
            Assert.That(D29InteractionLayoutPolicy.WarehouseSurfaceName,
                Is.EqualTo("D29WarehousePanel"));
            Assert.That(D29InteractionLayoutPolicy.WarehouseObjectName,
                Is.Not.EqualTo(D29InteractionLayoutPolicy.WarehouseSurfaceName));
        }
    }
}
