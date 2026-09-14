using System;
using System.Collections.Generic;

namespace Whalebone.Gameplay.D29
{
    /// <summary>
    /// D29 主 HUD 的纯逻辑布局契约。
    ///
    /// 这里使用屏幕左上角为原点的整数矩形，和 Unity UGUI 的锚点实现解耦。
    /// 表现层可以据此建立 Canvas；测试则可以在没有 Unity 场景的情况下验证
    /// 1920x1080 安全区、按钮堆叠和模态页之间不会发生意外覆盖。
    /// 本类不裁决任何业务状态，也不负责显示/隐藏 Canvas。
    /// </summary>
    public static class D29InteractionLayoutPolicy
    {
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;

        public const int QuickActionsRightInset = 28;
        public const int QuickActionsTopInset = 116;
        public const int QuickActionsWidth = 332;
        public const int QuickActionsHeight = 590;

        public const int QuickActionButtonWidth = 286;
        public const int QuickActionButtonHeight = 42;
        public const int QuickActionButtonTop = 94;
        public const int QuickActionButtonStep = 50;

        public const int WarehouseWidth = 620;
        public const int WarehouseHeight = 520;

        public const int PlayerTitleLeft = 24;
        public const int PlayerTitleTop = 24;
        public const int PlayerTitleWidth = 390;
        public const int PlayerTitleHeight = 78;

        public const int PlayerHintWidth = 820;
        public const int PlayerHintHeight = 66;
        public const int PlayerHintBottomInset = 30;

        public const int PlayerFeedbackLeft = 24;
        public const int PlayerFeedbackWidth = 650;
        public const int PlayerFeedbackHeight = 54;
        public const int PlayerFeedbackBottomInset = 118;

        public const string WarehouseObjectName = "WarehousePanel";
        public const string WarehouseSurfaceName = "D29WarehousePanel";

        public readonly struct ScreenRect
        {
            public ScreenRect(int left, int top, int width, int height)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }

            public int Left { get; }
            public int Top { get; }
            public int Width { get; }
            public int Height { get; }
            public int Right => Left + Width;
            public int Bottom => Top + Height;

            public bool IsInside(int viewportWidth, int viewportHeight)
            {
                return Left >= 0 && Top >= 0 && Width > 0 && Height > 0 &&
                    Right <= viewportWidth && Bottom <= viewportHeight;
            }

            public bool Overlaps(ScreenRect other)
            {
                return Left < other.Right && Right > other.Left &&
                    Top < other.Bottom && Bottom > other.Top;
            }
        }

        public static ScreenRect QuickActionsRect => new ScreenRect(
            ReferenceWidth - QuickActionsRightInset - QuickActionsWidth,
            QuickActionsTopInset,
            QuickActionsWidth,
            QuickActionsHeight);

        public static ScreenRect WarehouseRect => new ScreenRect(
            (ReferenceWidth - WarehouseWidth) / 2,
            (ReferenceHeight - WarehouseHeight) / 2,
            WarehouseWidth,
            WarehouseHeight);

        public static ScreenRect PlayerTitleRect => new ScreenRect(
            PlayerTitleLeft, PlayerTitleTop, PlayerTitleWidth, PlayerTitleHeight);

        public static ScreenRect PlayerHintRect => new ScreenRect(
            (ReferenceWidth - PlayerHintWidth) / 2,
            ReferenceHeight - PlayerHintBottomInset - PlayerHintHeight,
            PlayerHintWidth,
            PlayerHintHeight);

        public static ScreenRect PlayerFeedbackRect => new ScreenRect(
            PlayerFeedbackLeft,
            ReferenceHeight - PlayerFeedbackBottomInset - PlayerFeedbackHeight,
            PlayerFeedbackWidth,
            PlayerFeedbackHeight);

        public static ScreenRect QuickActionButtonStackRect
        {
            get
            {
                var panel = QuickActionsRect;
                var left = panel.Left + (QuickActionsWidth - QuickActionButtonWidth) / 2;
                var top = panel.Top + QuickActionButtonTop;
                var height = QuickActionButtonTop +
                    (QuickActionButtonCount - 1) * QuickActionButtonStep +
                    QuickActionButtonHeight;
                return new ScreenRect(left, top, QuickActionButtonWidth, height);
            }
        }

        public const int QuickActionButtonCount = 8;

        /// <summary>
        /// 返回空数组表示参考分辨率下布局合法。返回的错误是稳定文本，便于
        /// 结果 JSON 和交接报告直接记录，不把编辑器截图当作布局证明。
        /// </summary>
        public static string[] ValidateReferenceLayout()
        {
            var errors = new List<string>();
            var viewportWidth = ReferenceWidth;
            var viewportHeight = ReferenceHeight;

            var primary = new[]
            {
                Tuple.Create("D25PlayerTitle", PlayerTitleRect),
                Tuple.Create("D25PlayerFeedback", PlayerFeedbackRect),
                Tuple.Create("D25PlayerHint", PlayerHintRect),
                Tuple.Create("D29QuickActions", QuickActionsRect),
                Tuple.Create("D29Warehouse", WarehouseRect),
            };

            foreach (var surface in primary)
                if (!surface.Item2.IsInside(viewportWidth, viewportHeight))
                    errors.Add(surface.Item1 + " is outside reference viewport");

            for (var i = 0; i < primary.Length; i++)
            {
                for (var j = i + 1; j < primary.Length; j++)
                {
                    if (primary[i].Item2.Overlaps(primary[j].Item2))
                        errors.Add(primary[i].Item1 + " overlaps " + primary[j].Item1);
                }
            }

            if (!QuickActionButtonStackRect.IsInside(viewportWidth, viewportHeight))
                errors.Add("D29QuickActionButtonStack is outside reference viewport");

            return errors.ToArray();
        }
    }
}
