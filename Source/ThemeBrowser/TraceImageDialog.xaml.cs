namespace ThemeBrowser
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Extensions;

    public partial class TraceImageDialog
    {
        public TraceImageDialog(BitmapSource bitmap)
        {
            InitializeComponent();
            DataContext = this;

            Bitmap = bitmap;
            SourceRect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
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
                new PropertyMetadata(null));

        public BitmapSource SourceBitmap
        {
            get => (BitmapSource)GetValue(SourceBitmapProperty);
            set => SetValue(SourceBitmapProperty, value);
        }

        public static readonly DependencyProperty DiffBitmapProperty =
            DependencyProperty.Register(
                nameof(DiffBitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(null));

        public BitmapSource DiffBitmap
        {
            get => (BitmapSource)GetValue(DiffBitmapProperty);
            set => SetValue(DiffBitmapProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="TracedBitmap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TracedBitmapProperty =
            DependencyProperty.Register(
                nameof(TracedBitmap),
                typeof(BitmapSource),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    null,
                    (d, e) => ((TraceImageDialog)d).OnTracedBitmapChanged()));

        public BitmapSource TracedBitmap
        {
            get => (BitmapSource)GetValue(TracedBitmapProperty);
            set => SetValue(TracedBitmapProperty, value);
        }

        public static readonly DependencyProperty SourceRectProperty =
            DependencyProperty.Register(
                nameof(SourceRect),
                typeof(Int32Rect),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    Int32Rect.Empty,
                    (d, e) => ((TraceImageDialog)d).OnSourceRectChanged(),
                    (d, v) => ((TraceImageDialog)d).OnCoerceSourceRect((Int32Rect)v)));

        public Int32Rect SourceRect
        {
            get => (Int32Rect)GetValue(SourceRectProperty);
            set => SetValue(SourceRectProperty, value);
        }

        public static readonly DependencyProperty ShowTracedFlagProperty =
            DependencyProperty.Register(
                nameof(ShowTracedFlag),
                typeof(bool),
                typeof(TraceImageDialog),
                new PropertyMetadata(
                    false,
                    (d, e) => ((TraceImageDialog)d).OnShowTracedFlagChanged()));

        public bool ShowTracedFlag
        {
            get => (bool)GetValue(ShowTracedFlagProperty);
            set => SetValue(ShowTracedFlagProperty, value);
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

        private readonly Brush pathBorderBrush =
            new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)).EnsureFrozen();

        private Int32Rect OnCoerceSourceRect(Int32Rect baseValue)
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
            SourceBitmap = Bitmap != null ? new CroppedBitmap(Bitmap, SourceRect) : null;
            pathBorder.Width = SourceRect.Width;
            pathBorder.Height = SourceRect.Height;

            Dispatcher.BeginInvoke(new Action(UpdateDiff), DispatcherPriority.ContextIdle);
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
            Dispatcher.BeginInvoke(new Action(() => {
                TracedBitmap = ImagingUtils.RenderSnapshot(pathBorder);
            }), DispatcherPriority.ContextIdle);
        }

        private void OnTracedBitmapChanged()
        {
            Dispatcher.BeginInvoke(new Action(UpdateDiff), DispatcherPriority.ContextIdle);
        }

        private void OnShowTracedFlagChanged()
        {
            UpdateDiff();
        }

        private void UpdateDiff()
        {
            if (ShowTracedFlag)
                DiffBitmap = TracedBitmap;
            else if (TracedBitmap != null)
                DiffBitmap = ImagingUtils.Difference(SourceBitmap, TracedBitmap, 0xFFFFFFFF);
            else
                DiffBitmap = null;
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
