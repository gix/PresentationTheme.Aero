namespace ThemeBrowser
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Extensions;

    public partial class TraceImageDialog
    {
        private readonly Brush pathBorderBrush =
            new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)).EnsureFrozen();

        public TraceImageDialog(BitmapSource bitmap)
        {
            InitializeComponent();
            DataContext = this;

            BackgroundPatterns.Add(new Pattern("Checkered", (Brush)FindResource("CheckeredBrush")));
            BackgroundPatterns.Add(new Pattern("White", Brushes.White));
            BackgroundPatterns.Add(new Pattern("Black", Brushes.Black));
            BackgroundPatterns.Add(new Pattern("Magenta", Brushes.Magenta));
            SelectedBackgroundPattern = BackgroundPatterns[0];

            Bitmap = bitmap;
            SourceRect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);

            OnPathVisualChanged();
        }

        public static readonly DependencyProperty BitmapProperty =
            DependencyProperty.Register(
                nameof(Bitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(null));

        public BitmapSource Bitmap
        {
            get => (BitmapSource)GetValue(BitmapProperty);
            set => SetValue(BitmapProperty, value);
        }

        public static readonly DependencyProperty SourceBitmapProperty =
            DependencyProperty.Register(
                nameof(SourceBitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null,
                    (d, e) => ((TraceImageDialog)d).OnSourceBitmapChanged(),
                    (d, b) => ((TraceImageDialog)d).CoerceSourceBitmap(b)));

        public BitmapSource SourceBitmap
        {
            get => (BitmapSource)GetValue(SourceBitmapProperty);
            set => SetValue(SourceBitmapProperty, value);
        }

        private object CoerceSourceBitmap(object baseValue)
        {
            if (Bitmap == null)
                return null;

            return new CroppedBitmap(Bitmap, SourceRect);
        }

        private static readonly DependencyPropertyKey DiffBitmapPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(DiffBitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null, null, (d, b) => ((TraceImageDialog)d).CoerceDiffBitmap(b)));

        public static readonly DependencyProperty DiffBitmapProperty =
            DiffBitmapPropertyKey.DependencyProperty;

        public BitmapSource DiffBitmap => (BitmapSource)GetValue(DiffBitmapProperty);

        private static readonly DependencyPropertyKey TracedBitmapPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TracedBitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null,
                    (d, e) => ((TraceImageDialog)d).OnTracedBitmapChanged(),
                    (d, b) => ((TraceImageDialog)d).CoerceTracedBitmap(b)));

        public static readonly DependencyProperty TracedBitmapProperty =
            TracedBitmapPropertyKey.DependencyProperty;

        public BitmapSource TracedBitmap => (BitmapSource)GetValue(TracedBitmapProperty);

        public static readonly DependencyProperty SourceRectProperty =
            DependencyProperty.Register(
                nameof(SourceRect),
                typeof(Int32Rect),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    Int32Rect.Empty,
                    (d, e) => ((TraceImageDialog)d).OnSourceRectChanged(),
                    (d, v) => ((TraceImageDialog)d).CoerceSourceRect((Int32Rect)v)));

        public Int32Rect SourceRect
        {
            get => (Int32Rect)GetValue(SourceRectProperty);
            set => SetValue(SourceRectProperty, value);
        }

        public sealed class Pattern
        {
            public Pattern(string name, Brush brush)
            {
                Name = name;
                Brush = brush;
            }

            public string Name { get; }
            public Brush Brush { get; }
        }

        public ObservableCollection<Pattern> BackgroundPatterns { get; } =
            new ObservableCollection<Pattern>();

        public static readonly DependencyProperty SelectedBackgroundPatternProperty =
            DependencyProperty.Register(
                nameof(SelectedBackgroundPattern),
                typeof(Pattern),
                typeof(TraceImageDialog),
                new PropertyMetadata(null));

        public Pattern SelectedBackgroundPattern
        {
            get => (Pattern)GetValue(SelectedBackgroundPatternProperty);
            set => SetValue(SelectedBackgroundPatternProperty, value);
        }

        public static readonly DependencyProperty ShowDiffFlagProperty =
            DependencyProperty.Register(
                nameof(ShowDiffFlag),
                typeof(bool),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    false,
                    (d, e) => ((TraceImageDialog)d).OnShowDiffFlagChanged()));

        public bool ShowDiffFlag
        {
            get => (bool)GetValue(ShowDiffFlagProperty);
            set => SetValue(ShowDiffFlagProperty, value);
        }

        public static readonly DependencyProperty PathGeometryProperty =
            DependencyProperty.Register(
                nameof(PathGeometry),
                typeof(Geometry),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public Geometry PathGeometry
        {
            get => (Geometry)GetValue(PathGeometryProperty);
            set => SetValue(PathGeometryProperty, value);
        }

        public static readonly DependencyProperty PathMarginProperty =
            DependencyProperty.Register(
                nameof(PathMargin),
                typeof(Thickness),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    new Thickness(),
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public Thickness PathMargin
        {
            get => (Thickness)GetValue(PathMarginProperty);
            set => SetValue(PathMarginProperty, value);
        }

        public static readonly DependencyProperty PathDataProperty =
            DependencyProperty.Register(
                nameof(PathData),
                typeof(string),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null, (d, e) => ((TraceImageDialog)d).OnPathDataChanged()));

        public string PathData
        {
            get => (string)GetValue(PathDataProperty);
            set => SetValue(PathDataProperty, value);
        }

        public static readonly DependencyProperty PathFillProperty =
            DependencyProperty.Register(
                nameof(PathFill),
                typeof(Brush),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    Brushes.Black,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public Brush PathFill
        {
            get => (Brush)GetValue(PathFillProperty);
            set => SetValue(PathFillProperty, value);
        }

        public static readonly DependencyProperty PathStrokeProperty =
            DependencyProperty.Register(
                nameof(PathStroke),
                typeof(Brush),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public Brush PathStroke
        {
            get => (Brush)GetValue(PathStrokeProperty);
            set => SetValue(PathStrokeProperty, value);
        }

        public static readonly DependencyProperty PathStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(PathStrokeThickness),
                typeof(double),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    0.0,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public double PathStrokeThickness
        {
            get => (double)GetValue(PathStrokeProperty);
            set => SetValue(PathStrokeProperty, value);
        }

        public static readonly DependencyProperty PathStrokeLineJoinProperty =
            DependencyProperty.Register(
                nameof(PathStrokeLineJoin),
                typeof(PenLineJoin),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    PenLineJoin.Miter,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public PenLineJoin PathStrokeLineJoin
        {
            get => (PenLineJoin)GetValue(PathStrokeLineJoinProperty);
            set => SetValue(PathStrokeLineJoinProperty, value);
        }

        public static readonly DependencyProperty PathStrokeLineCapProperty =
            DependencyProperty.Register(
                nameof(PathStrokeLineCap),
                typeof(PenLineCap),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    PenLineCap.Flat,
                    (d, e) => ((TraceImageDialog)d).OnPathVisualChanged()));

        public PenLineCap PathStrokeLineCap
        {
            get => (PenLineCap)GetValue(PathStrokeLineCapProperty);
            set => SetValue(PathStrokeLineCapProperty, value);
        }

        public static readonly DependencyProperty PathRenderedDataProperty =
            DependencyProperty.Register(
                nameof(PathRenderedData),
                typeof(string),
                typeof(TraceImageDialog),
                new PropertyMetadata(null));

        public string PathRenderedData
        {
            get => (string)GetValue(PathRenderedDataProperty);
            set => SetValue(PathRenderedDataProperty, value);
        }

        public static readonly DependencyProperty PathWidenProperty =
            DependencyProperty.Register(
                nameof(PathWiden),
                typeof(double),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    0.0,
                    (d, e) => ((TraceImageDialog)d).OnPathDataChanged()));

        public double PathWiden
        {
            get => (double)GetValue(PathWidenProperty);
            set => SetValue(PathWidenProperty, value);
        }

        public static readonly DependencyProperty RenderAliasedProperty =
            DependencyProperty.Register(
                nameof(RenderAliased),
                typeof(bool),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    false,
                    (d, e) => ((TraceImageDialog)d).OnRenderAliasedChanged((bool)e.NewValue)));

        public bool RenderAliased
        {
            get => (bool)GetValue(RenderAliasedProperty);
            set => SetValue(RenderAliasedProperty, value);
        }

        private void OnRenderAliasedChanged(bool newValue)
        {
            RenderOptions.SetEdgeMode(
                path, newValue ? EdgeMode.Aliased : EdgeMode.Unspecified);
            OnPathVisualChanged();
        }

        private Int32Rect CoerceSourceRect(Int32Rect baseValue)
        {
            if (baseValue.IsEmpty)
                return new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight);

            var x = Clamp(baseValue.X, 0, Bitmap.PixelWidth);
            var y = Clamp(baseValue.Y, 0, Bitmap.PixelHeight);
            var w = Clamp(baseValue.Width, 0, Bitmap.PixelWidth - x);
            var h = Clamp(baseValue.Height, 0, Bitmap.PixelHeight - y);
            return new Int32Rect(x, y, w, h);
        }

        private void OnSourceRectChanged()
        {
            pathBorder.Width = SourceRect.Width;
            pathBorder.Height = SourceRect.Height;
            InvalidateProperty(SourceBitmapProperty);
        }

        private void OnPathDataChanged()
        {
            try {
                var geometry = Geometry.Parse(PathData);
                if (PathWiden > 0)
                    geometry = geometry.GetWidenedPathGeometry(new Pen(Brushes.Black, PathWiden)).Round(3);

                PathGeometry = geometry;
                PathRenderedData = geometry.ToString(CultureInfo.InvariantCulture);

                pathBorder.BorderBrush = pathBorderBrush;
            } catch {
                PathGeometry = new RectangleGeometry(new Rect(new Size(10, 10)));
                pathBorder.BorderBrush = Brushes.Red;
            }
        }

        private void OnPathVisualChanged()
        {
            Dispatcher.InvokeAsync(
                () => InvalidateProperty(TracedBitmapProperty),
                DispatcherPriority.ContextIdle);
        }

        private void OnSourceBitmapChanged()
        {
            InvalidateProperty(TracedBitmapProperty);
            InvalidateProperty(DiffBitmapProperty);
        }

        private void OnTracedBitmapChanged()
        {
            InvalidateProperty(DiffBitmapProperty);
        }

        private void OnShowDiffFlagChanged()
        {
            InvalidateProperty(DiffBitmapProperty);
        }

        private object CoerceTracedBitmap(object baseValue)
        {
            return ImagingUtils.RenderSnapshot(pathBorder);
        }

        private object CoerceDiffBitmap(object baseValue)
        {
            if (!ShowDiffFlag)
                return TracedBitmap;
            if (TracedBitmap != null)
                return ImagingUtils.Difference(SourceBitmap, TracedBitmap, 0xFFFFFFFF);
            return baseValue;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value <= max)
                return value;
            return max;
        }
    }
}
