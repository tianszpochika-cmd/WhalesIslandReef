using NUnit.Framework;
using Whalebone.Gameplay.D29;

namespace Whalebone.Tests.D31
{
    /// <summary>
    /// Q18 的代码级分辨率矩阵：D25/D29 使用 1920x1080、matchWidthOrHeight=0.5
    /// 的 CanvasScaler；本测试验证同一 16:9 比例下缩放后的安全矩形仍在屏内，
    /// 且不把参考布局的无重叠关系变成重叠。它不替代原生 Player 截图。
    /// </summary>
    public sealed class D31Q18HudResolutionTests
    {
        private struct Surface
        {
            public string Name;
            public D29InteractionLayoutPolicy.ScreenRect Rect;

            public Surface(string name, D29InteractionLayoutPolicy.ScreenRect rect)
            {
                Name = name;
                Rect = rect;
            }
        }

        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        public void RequiredSixteenByNineResolutionKeepsHudSurfacesInsideViewport(int width, int height)
        {
            Assert.That(D29InteractionLayoutPolicy.ValidateReferenceLayout(), Is.Empty,
                "参考布局本身必须先通过。");

            var widthScale = width / (float)D29InteractionLayoutPolicy.ReferenceWidth;
            var heightScale = height / (float)D29InteractionLayoutPolicy.ReferenceHeight;
            Assert.That(widthScale, Is.EqualTo(heightScale).Within(0.0001f),
                "该矩阵只覆盖 D25/D29 当前约定的 16:9 比例。");

            var surfaces = new[]
            {
                new Surface("D25PlayerTitle", D29InteractionLayoutPolicy.PlayerTitleRect),
                new Surface("D25PlayerFeedback", D29InteractionLayoutPolicy.PlayerFeedbackRect),
                new Surface("D25PlayerHint", D29InteractionLayoutPolicy.PlayerHintRect),
                new Surface("D29QuickActions", D29InteractionLayoutPolicy.QuickActionsRect),
                new Surface("D29Warehouse", D29InteractionLayoutPolicy.WarehouseRect),
            };

            for (var i = 0; i < surfaces.Length; i++)
            {
                var surface = surfaces[i];
                var left = surface.Rect.Left * widthScale;
                var top = surface.Rect.Top * heightScale;
                var right = surface.Rect.Right * widthScale;
                var bottom = surface.Rect.Bottom * heightScale;
                Assert.That(left, Is.GreaterThanOrEqualTo(-0.01f), surface.Name + " left outside viewport");
                Assert.That(top, Is.GreaterThanOrEqualTo(-0.01f), surface.Name + " top outside viewport");
                Assert.That(right, Is.LessThanOrEqualTo(width + 0.01f), surface.Name + " right outside viewport");
                Assert.That(bottom, Is.LessThanOrEqualTo(height + 0.01f), surface.Name + " bottom outside viewport");

                for (var j = i + 1; j < surfaces.Length; j++)
                {
                    var other = surfaces[j];
                    var otherLeft = other.Rect.Left * widthScale;
                    var otherTop = other.Rect.Top * heightScale;
                    var otherRight = other.Rect.Right * widthScale;
                    var otherBottom = other.Rect.Bottom * heightScale;
                    var overlaps = left < otherRight && right > otherLeft &&
                        top < otherBottom && bottom > otherTop;
                    Assert.IsFalse(overlaps, surface.Name + " overlaps " + other.Name);
                }
            }

            var buttonStack = D29InteractionLayoutPolicy.QuickActionButtonStackRect;
            Assert.That(buttonStack.Left * widthScale, Is.GreaterThanOrEqualTo(-0.01f));
            Assert.That(buttonStack.Top * heightScale, Is.GreaterThanOrEqualTo(-0.01f));
            Assert.That(buttonStack.Right * widthScale, Is.LessThanOrEqualTo(width + 0.01f));
            Assert.That(buttonStack.Bottom * heightScale, Is.LessThanOrEqualTo(height + 0.01f));
        }
    }
}
