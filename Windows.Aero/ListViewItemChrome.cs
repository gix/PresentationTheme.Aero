namespace Windows.Aero
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class ListViewItemChrome : Decorator
    {
        private static readonly Lazy<Pen> NormalBorderPen;
        private static readonly Lazy<Pen> InnerBorderPen;
        private static readonly Lazy<Pen> SelectedBorderPen;
        private static readonly Lazy<Brush> SelectedBackgroundBrush;
        private static readonly Lazy<Pen> HoveredBorderPen;
        private static readonly Lazy<Brush> HoveredBackgroundBrush;
        private static readonly Lazy<Pen> FocusedBorderPen;
        private static readonly Lazy<Brush> FocusedBackgroundBrush;
        private static readonly Lazy<Pen> InactiveBorderPen;
        private static readonly Lazy<Brush> InactiveBackgroundBrush;

        /// <summary>
        ///   Identifies the <see cref="RenderFocused"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderFocusedProperty =
            DependencyProperty.Register(
                "RenderFocused",
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderMouseOver"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderMouseOverProperty =
            DependencyProperty.Register(
                "RenderMouseOver",
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderSelectedProperty =
            DependencyProperty.Register(
                "RenderSelected",
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderInactive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderInactiveProperty =
            DependencyProperty.Register(
                "RenderInactive",
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        static ListViewItemChrome()
        {
            NormalBorderPen = new Lazy<Pen>(
                () => {
                    var pen = new Pen { Thickness = 1.0, Brush = Brushes.Transparent };
                    pen.Freeze();
                    return pen;
                });

            InnerBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });

            SelectedBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x84, 0xAC, 0xDD));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            SelectedBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xEC, 0xF5, 0xFF),
                        Color.FromArgb(0xFF, 0xD0, 0xE5, 0xFF),
                        90);
                    brush.Freeze();
                    return brush;
                });

            HoveredBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xB9, 0xD7, 0xFC));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            HoveredBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xFC, 0xFD, 0xFF),
                        Color.FromArgb(0xFF, 0xEC, 0xF5, 0xFF),
                        90);
                    brush.Freeze();
                    return brush;
                });

            FocusedBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x7D, 0xA2, 0xCE));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            FocusedBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xDD, 0xEC, 0xFD),
                        Color.FromArgb(0xFF, 0xC2, 0xDC, 0xFD),
                        90);
                    brush.Freeze();
                    return brush;
                });

            InactiveBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0xDA, 0xDA));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            InactiveBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xF9, 0xF9, 0xF9),
                        Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6),
                        90);
                    brush.Freeze();
                    return brush;
                });
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            if (child == null)
                return new Size(Math.Min(4.0, constraint.Width), Math.Min(4.0, constraint.Height));

            var size = new Size();
            bool widthTooSmall = constraint.Width < 4.0;
            bool heightTooSmall = constraint.Height < 4.0;
            if (!widthTooSmall)
                size.Width = constraint.Width - 6;
            if (!heightTooSmall)
                size.Height = constraint.Height - 7;

            child.Measure(size);

            Size desiredSize = child.DesiredSize;
            if (!widthTooSmall)
                desiredSize.Width += 6;
            if (!heightTooSmall)
                desiredSize.Height += 7;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child == null)
                return arrangeSize;

            var bounds = new Rect(
                3, 3, Math.Max(0, arrangeSize.Width - 6), Math.Max(0, arrangeSize.Height - 7));
            child.Arrange(bounds);

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bounds = new Rect(RenderSize);
            bounds.Inflate(-1, -1);
            dc.DrawRectangle(BackgroundBrush, null, bounds);

            //rect = new Rect(bounds.Left + 1.5, bounds.Top + 1.5, bounds.Width - 3.0, bounds.Height - 3.0);
            bounds.Inflate(-0.5, -0.5);
            dc.DrawRoundedRectangle(null, InnerBorderPen.Value, bounds, 1.75, 1.75);

            //bounds.Inflate(-0.5, -0.5);
            bounds.Inflate(1, 1);
            dc.DrawRoundedRectangle(null, BorderPen, bounds, 1.75, 1.75);
        }

        /// <summary>
        ///   Gets or sets the RenderFocused of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderFocused
        {
            get { return (bool)GetValue(RenderFocusedProperty); }
            set { SetValue(RenderFocusedProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the RenderMouseOver of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the RenderSelected of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderSelected
        {
            get { return (bool)GetValue(RenderSelectedProperty); }
            set { SetValue(RenderSelectedProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the RenderInactive of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderInactive
        {
            get { return (bool)GetValue(RenderInactiveProperty); }
            set { SetValue(RenderInactiveProperty, value); }
        }

        private Pen BorderPen
        {
            get
            {
                if (RenderSelected && RenderMouseOver)
                    return FocusedBorderPen.Value;
                if (RenderInactive)
                    return InactiveBorderPen.Value;
                if (RenderMouseOver)
                    return HoveredBorderPen.Value;
                if (RenderSelected)
                    return SelectedBorderPen.Value;
                return NormalBorderPen.Value;
            }
        }

        private Brush BackgroundBrush
        {
            get
            {
                if (RenderSelected && RenderMouseOver)
                    return FocusedBackgroundBrush.Value;
                if (RenderInactive)
                    return InactiveBackgroundBrush.Value;
                if (RenderMouseOver)
                    return HoveredBackgroundBrush.Value;
                if (RenderSelected)
                    return SelectedBackgroundBrush.Value;
                return Brushes.Transparent;
            }
        }
    }
}