namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class ListViewItemChrome : Decorator
    {
        private static readonly Lazy<Pen> NormalBorderPen;
        private static readonly Lazy<Pen> SelectedBorderPen;
        private static readonly Lazy<Brush> SelectedBackgroundBrush;
        private static readonly Lazy<Pen> HoveredBorderPen;
        private static readonly Lazy<Brush> HoveredBackgroundBrush;
        private static readonly Lazy<Pen> FocusedBorderPen;
        private static readonly Lazy<Brush> FocusedBackgroundBrush;
        private static readonly Lazy<Pen> InactiveBorderPen;
        private static readonly Lazy<Brush> InactiveBackgroundBrush;

        /// <summary>
        ///   Identifies the <see cref="BackgroundProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty =
            Control.BackgroundProperty.AddOwner(
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="BorderBrushProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="BorderThicknessProperty"/> dependency
        ///   property.
        /// </summary>
        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    new Thickness(1.0),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderFocused"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderFocusedProperty =
            DependencyProperty.Register(
                nameof(RenderFocused),
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderMouseOver"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderMouseOverProperty =
            DependencyProperty.Register(
                nameof(RenderMouseOver),
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderSelectedProperty =
            DependencyProperty.Register(
                nameof(RenderSelected),
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderInactive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderInactiveProperty =
            DependencyProperty.Register(
                nameof(RenderInactive),
                typeof(bool),
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        static ListViewItemChrome()
        {
            IsEnabledProperty.OverrideMetadata(
                typeof(ListViewItemChrome),
                new FrameworkPropertyMetadata(
                    true, FrameworkPropertyMetadataOptions.AffectsRender));

            NormalBorderPen = new Lazy<Pen>(
                () => {
                    var pen = new Pen {
                        Thickness = 1.0,
                        Brush = Brushes.Transparent
                    };
                    pen.Freeze();
                    return pen;
                });

            SelectedBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x26, 0xA0, 0xDA));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            SelectedBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x3D, 0x26, 0x9F, 0xD9));
                    brush.Freeze();
                    return brush;
                });

            HoveredBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0xA8, 0x26, 0x9F, 0xDB));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            HoveredBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x1F, 0x29, 0x9C, 0xDE));
                    brush.Freeze();
                    return brush;
                });

            FocusedBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x6C, 0xD9));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            FocusedBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x2E, 0x00, 0x80, 0xFF));
                    brush.Freeze();
                    return brush;
                });

            InactiveBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x21, 0x00, 0x00, 0x00));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            InactiveBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(Color.FromArgb(0x08, 0x00, 0x00, 0x00));
                    brush.Freeze();
                    return brush;
                });
        }

        /// <summary>
        ///   Gets or sets the RenderFocused of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderFocused
        {
            get => (bool)GetValue(RenderFocusedProperty);
            set => SetValue(RenderFocusedProperty, value);
        }

        /// <summary>
        ///   Gets or sets the RenderMouseOver of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderMouseOver
        {
            get => (bool)GetValue(RenderMouseOverProperty);
            set => SetValue(RenderMouseOverProperty, value);
        }

        /// <summary>
        ///   Gets or sets the RenderSelected of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderSelected
        {
            get => (bool)GetValue(RenderSelectedProperty);
            set => SetValue(RenderSelectedProperty, value);
        }

        /// <summary>
        ///   Gets or sets the RenderInactive of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderInactive
        {
            get => (bool)GetValue(RenderInactiveProperty);
            set => SetValue(RenderInactiveProperty, value);
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

        protected override Size MeasureOverride(Size constraint)
        {
            const double MinW = 4.0;
            const double MinH = 4.0;

            UIElement child = Child;
            if (child == null)
                return new Size(
                    Math.Min(MinW, constraint.Width),
                    Math.Min(MinH, constraint.Height));

            var size = new Size();
            bool widthTooSmall = constraint.Width < MinW;
            bool heightTooSmall = constraint.Height < MinH;
            if (!widthTooSmall)
                size.Width = constraint.Width - 0;
            if (!heightTooSmall)
                size.Height = constraint.Height - 4;

            child.Measure(size);

            Size desiredSize = child.DesiredSize;
            if (!widthTooSmall)
                desiredSize.Width += 0;
            if (!heightTooSmall)
                desiredSize.Height += 4;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child == null)
                return arrangeSize;

            var bounds = new Rect(
                0,
                2,
                Math.Max(0, arrangeSize.Width - 0),
                Math.Max(0, arrangeSize.Height - 4));
            child.Arrange(bounds);

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bounds = new Rect(RenderSize);

            bounds.Inflate(-1, -1);
            if (bounds.IsEmpty)
                return;

            dc.DrawRectangle(BackgroundBrush, null, bounds);

            bounds.Inflate(0.5, 0.5);
            dc.DrawRectangle(null, BorderPen, bounds);
        }
    }

    internal sealed class GridViewHeaderRowPresenterEx : GridViewHeaderRowPresenter
    {
        #region public FrameworkElement ScrollViewer { get; set; }

        /// <summary>
        ///   Identifies the <see cref="ParentScope"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ParentScopeProperty =
            DependencyProperty.Register(
                nameof(ParentScope),
                typeof(FrameworkElement),
                typeof(GridViewHeaderRowPresenterEx),
                new PropertyMetadata(null, OnParentScopeChanged));

        private static void OnParentScopeChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (GridViewHeaderRowPresenterEx)d;
            source.InvalidateVisual();
        }

        /// <summary>
        ///   Gets or sets the scrollViewer.
        /// </summary>
        public FrameworkElement ParentScope
        {
            get => (FrameworkElement)GetValue(ParentScopeProperty);
            set => SetValue(ParentScopeProperty, value);
        }

        #endregion


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }
    }
}
