namespace ThemeBrowser
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Extensions;
    using ThemeCore.Native;
    using Brushes = System.Windows.Media.Brushes;
    using Point = System.Windows.Point;
    using Size = System.Windows.Size;

    public partial class PreviewImageDialog
    {
        public PreviewImageDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;

            var layer = AdornerLayer.GetAdornerLayer(border);
            layer.Add(new ResizingAdorner(border));
        }
    }

    public static class DataPiping
    {
        public static readonly DependencyProperty DataPipesProperty =
            DependencyProperty.RegisterAttached("DataPipes",
            typeof(DataPipeCollection),
            typeof(DataPiping),
            new UIPropertyMetadata(null));

        public static void SetDataPipes(DependencyObject o, DataPipeCollection value)
        {
            o.SetValue(DataPipesProperty, value);
        }

        public static DataPipeCollection GetDataPipes(DependencyObject o)
        {
            return (DataPipeCollection)o.GetValue(DataPipesProperty);
        }
    }

    public class DataPipeCollection : FreezableCollection<DataPipe>
    {
    }

    public class DataPipe : Freezable
    {
        #region Source (DependencyProperty)

        public object Source
        {
            get => (object)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(DataPipe),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged)));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataPipe)d).OnSourceChanged(e);
        }

        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Target = e.NewValue;
        }

        #endregion

        #region Target (DependencyProperty)

        public object Target
        {
            get => (object)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(object), typeof(DataPipe),
            new FrameworkPropertyMetadata(null));

        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new DataPipe();
        }
    }

    public class PreviewImageDialogViewModel : ViewModel
    {
        private string message;
        private readonly Bitmap sourceBitmap;
        private readonly SafeBitmapHandle sourceBitmapGdi;
        private ImageSource imageSource;
        private double imageWidth;
        private double imageHeight;

        private Int32Rect sourceRect;
        private Int32Rect destRect;
        private Int32Rect sizingMargins;
        private int drawOption;

        public PreviewImageDialogViewModel(Bitmap sourceBitmap)
        {
            this.sourceBitmap = sourceBitmap;
            sourceBitmapGdi = sourceBitmap.GetHbitmapHandle();

            imageWidth = 100;
            imageHeight = 100;
            sourceRect = new Int32Rect(0, 0, sourceBitmap.Width, sourceBitmap.Height / 6);
            destRect = new Int32Rect(0, 0, (int)ImageWidth, (int)ImageHeight);
            sizingMargins = new Int32Rect(6, 5, 6, 5);
            drawOption = 4;

            RenderImage();
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public Int32Rect SourceRect
        {
            get { return sourceRect; }
            set
            {
                if (SetProperty(ref sourceRect, value))
                    RenderImage();
            }
        }

        public Int32Rect DestRect
        {
            get { return destRect; }
            set
            {
                if (SetProperty(ref destRect, value))
                    RenderImage();
            }
        }

        public Int32Rect SizingMargins
        {
            get { return sizingMargins; }
            set
            {
                if (SetProperty(ref sizingMargins, value))
                    RenderImage();
            }
        }

        private Int32Rect? clippingRect;

        public Int32Rect? ClippingRect
        {
            get { return clippingRect; }
            set
            {
                if (SetProperty(ref clippingRect, value))
                    RenderImage();
            }
        }


        public int DrawOption
        {
            get { return drawOption; }
            set
            {
                if (SetProperty(ref drawOption, value))
                    RenderImage();
            }
        }

        public ImageSource ImageSource
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }

        public double ImageWidth
        {
            get { return imageWidth; }
            set
            {
                if (SetProperty(ref imageWidth, value))
                    RenderImage();
            }
        }

        public double ImageHeight
        {
            get { return imageHeight; }
            set
            {
                if (SetProperty(ref imageHeight, value))
                    RenderImage();
            }
        }

        private void RenderImage()
        {
            message = null;

            var width = (int)ImageWidth;
            var height = (int)ImageHeight;
            if (width == 0 || height == 0)
                return;

            var srcRect = Convert(SourceRect);
            var dstRect = Convert(DestRect);
            dstRect = new RECT(0, 0, width, height);
            var clipRect = ClippingRect != null ? Convert(ClippingRect.Value) : dstRect;
            var margins = new MARGINS {
                cxLeftWidth = SizingMargins.X,
                cyTopHeight = SizingMargins.Y,
                cxRightWidth = SizingMargins.Width,
                cyBottomHeight = SizingMargins.Height
            };

            uint transparentColor = 0x000000;

            using (var bitmap = new Bitmap(width, height)) {
                using (var gfx = Graphics.FromImage(bitmap)) {
                    var hdc = gfx.GetHdc();

                    var gds = new GDIDRAWSTREAM();
                    gds.signature = 0x44727753;
                    gds.hDC = (uint)hdc.ToInt64();
                    gds.hImage = (uint)sourceBitmapGdi.DangerousGetHandle().ToInt64();
                    gds.one = 1;
                    gds.nine = 9;
                    gds.rcSrc = srcRect;
                    gds.rcClip = clipRect;
                    gds.rcDest = dstRect;
                    gds.drawOption = (uint)DrawOption;
                    gds.leftArcValue = (uint)margins.cxLeftWidth;
                    gds.rightArcValue = (uint)margins.cxRightWidth;
                    gds.topArcValue = (uint)margins.cyTopHeight;
                    gds.bottomArcValue = (uint)margins.cyBottomHeight;
                    gds.crTransparent = transparentColor;

                    var success = GdiNativeMethods.GdiDrawStream(
                        hdc, (uint)Marshal.SizeOf<GDIDRAWSTREAM>(), ref gds);

                    gfx.ReleaseHdc(hdc);

                    if (!success) {
                        var ec = Marshal.GetLastWin32Error();
                        message = string.Format("Draw failed: ec={0} ({1})", ec, new Win32Exception(ec).Message);
                        return;
                    }
                }

                ImageSource = ImagingUtils.ConvertBitmap(bitmap).EnsureFrozen();
            }
        }

        private static RECT Convert(Int32Rect rect)
        {
            return new RECT(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GDIDRAWSTREAM
    {
        public uint signature;      // = 0x44727753;//"Swrd"
        public uint reserved;       // Zero value.
        public uint hDC;            // handle to the device object of window to draw.
        public RECT rcDest;         // desination rect of window to draw.
        public uint one;            // must be 1.
        public uint hImage;         // handle to the specia bitmap image.
        public uint nine;           // must be 9.
        public RECT rcClip;         // desination rect of window to draw.
        public RECT rcSrc;          // source rect of bitmap to draw.
        public uint drawOption;     // option flag for drawing image.
        public uint leftArcValue;   // arc value of left side.
        public uint rightArcValue;  // arc value of right side.
        public uint topArcValue;    // arc value of top side.
        public uint bottomArcValue; // arc value of bottom side.
        public uint crTransparent;  // transparent color.
    }

    public static class GdiNativeMethods
    {
        [DllImport("gdi32")]
        public static extern bool GdiDrawStream(
            IntPtr hDC, uint dwStructSize, ref GDIDRAWSTREAM pStream);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int iBkMode);

        public static SafeBitmapHandle GetHbitmapHandle(this Bitmap bitmap)
        {
            return new SafeBitmapHandle(bitmap.GetHbitmap(), true);
        }
    }

    public class SafeBitmapHandle : SafeHandleZeroIsInvalid
    {
        private SafeBitmapHandle()
            : base(true)
        {
        }

        public SafeBitmapHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return GdiNativeMethods.DeleteObject(handle);
        }
    }

    public class ResizingAdorner : Adorner
    {
        // Resizing adorner uses Thumbs for visual elements.
        // The Thumbs have built-in mouse input handling.
        private readonly Thumb topLeft;
        private readonly Thumb topRight;
        private readonly Thumb bottomLeft;
        private readonly Thumb bottomRight;

        private readonly VisualCollection visualChildren;

        public ResizingAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);

            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE, ResizeDirection.TopLeft);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW, ResizeDirection.TopRight);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW, ResizeDirection.BottomLeft);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE, ResizeDirection.BottomRight);

            bottomLeft.DragDelta += HandleBottomLeft;
            bottomRight.DragDelta += HandleBottomRight;
            topLeft.DragDelta += HandleTopLeft;
            topRight.DragDelta += HandleTopRight;
        }

        protected override int VisualChildrenCount => visualChildren.Count;
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

        private void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            var parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
        }

        private void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            var parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            var heightOld = adornedElement.Height;
            var heightNew = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            var topOld = Canvas.GetTop(adornedElement);
            adornedElement.Height = heightNew;
            Canvas.SetTop(adornedElement, topOld - (heightNew - heightOld));
        }

        private void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            var widthOld = adornedElement.Width;
            var widthNew = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            var leftOld = Canvas.GetLeft(adornedElement);
            adornedElement.Width = widthNew;
            Canvas.SetLeft(adornedElement, leftOld - (widthNew - widthOld));

            var heightOld = adornedElement.Height;
            var heightNew = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            var topOld = Canvas.GetTop(adornedElement);
            adornedElement.Height = heightNew;
            Canvas.SetTop(adornedElement, topOld - (heightNew - heightOld));
        }

        private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            var widthOld = adornedElement.Width;
            var widthNew = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            var leftOld = Canvas.GetLeft(adornedElement);
            adornedElement.Width = widthNew;
            Canvas.SetLeft(adornedElement, leftOld - (widthNew - widthOld));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.
            // These will be used to place the ResizingAdorner at the corners of the adorned element.
            var desiredWidth = AdornedElement.DesiredSize.Width;
            var desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            var adornerWidth = DesiredSize.Width;
            var adornerHeight = DesiredSize.Height;

            topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            // Return the final size.
            return finalSize;
        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property,
        // set some appearance properties, and add the elements to the visual tree.
        private void BuildAdornerCorner(
            ref Thumb resizeThumb, Cursor customizedCursor, ResizeDirection direction)
        {
            if (resizeThumb != null)
                return;

            var thumb = new ResizeThumb {
                Direction = direction,
                Cursor = customizedCursor,
                Width = 10,
                Height = 10,
                Background = Brushes.Black
            };

            visualChildren.Add(thumb);
            resizeThumb = thumb;
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        private void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            var parent = adornedElement.Parent as FrameworkElement;
            if (parent != null) {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }
    }

    public enum ResizeDirection
    {
        None = 0,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }

    public class ResizeThumb : Thumb
    {
        private Geometry geometry;

        public ResizeThumb()
        {
            Template = null;
            SizeChanged += (s, e) => { geometry = null; };
        }

        #region public ResizeDirection Direction { get; set; }

        /// <summary>
        ///   Identifies the <see cref="Direction"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register(
                nameof(Direction),
                typeof(ResizeDirection),
                typeof(ResizeThumb),
                new FrameworkPropertyMetadata(
                    ResizeDirection.None,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnDirectionChanged));

        /// <summary>
        ///   Gets or sets the direction.
        /// </summary>
        public ResizeDirection Direction
        {
            get => (ResizeDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        private static void OnDirectionChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ResizeThumb)d;
            source.geometry = null;
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (geometry == null)
                geometry = BuildGeometry();

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            dc.DrawGeometry(Background, null, geometry);
        }

        private Geometry BuildGeometry()
        {
            var geometry = new StreamGeometry();

            double w = ActualWidth;
            double h = ActualHeight;

            using (var ctx = geometry.Open()) {
                switch (Direction) {
                    case ResizeDirection.TopLeft:
                        ctx.BeginFigure(new Point(0, 0), true, true);
                        ctx.LineTo(new Point(0, h), false, false);
                        ctx.LineTo(new Point(2, h), false, false);
                        ctx.LineTo(new Point(2, 2), false, false);
                        ctx.LineTo(new Point(w, 2), false, false);
                        ctx.LineTo(new Point(w, 0), false, false);
                        break;
                    case ResizeDirection.BottomRight:
                        ctx.BeginFigure(new Point(0, h), true, true);
                        ctx.LineTo(new Point(w, h), false, false);
                        ctx.LineTo(new Point(w, 0), false, false);
                        ctx.LineTo(new Point(w - 2, 0), false, false);
                        ctx.LineTo(new Point(w - 2, h - 2), false, false);
                        ctx.LineTo(new Point(0, h - 2), false, false);
                        break;
                    case ResizeDirection.BottomLeft:
                        ctx.BeginFigure(new Point(0, h), true, true);
                        ctx.LineTo(new Point(w, h), false, false);
                        ctx.LineTo(new Point(w, h - 2), false, false);
                        ctx.LineTo(new Point(2, h - 2), false, false);
                        ctx.LineTo(new Point(2, 0), false, false);
                        ctx.LineTo(new Point(0, 0), false, false);
                        break;
                    case ResizeDirection.TopRight:
                        ctx.BeginFigure(new Point(0, 0), true, true);
                        ctx.LineTo(new Point(0, 2), false, false);
                        ctx.LineTo(new Point(w - 2, 2), false, false);
                        ctx.LineTo(new Point(w - 2, h), false, false);
                        ctx.LineTo(new Point(w, h), false, false);
                        ctx.LineTo(new Point(w, 0), false, false);
                        break;
                }
            }

            return geometry;
        }
    }
}
