namespace ThemeBrowser
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    public class ResizingAdorner : Adorner
    {
        private readonly Thumb topLeft;
        private readonly Thumb topRight;
        private readonly Thumb bottomLeft;
        private readonly Thumb bottomRight;

        private readonly VisualCollection visualChildren;

        public ResizingAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);

            CreateThumb(ref topLeft, Cursors.SizeNWSE, ResizeDirection.TopLeft);
            CreateThumb(ref topRight, Cursors.SizeNESW, ResizeDirection.TopRight);
            CreateThumb(ref bottomLeft, Cursors.SizeNESW, ResizeDirection.BottomLeft);
            CreateThumb(ref bottomRight, Cursors.SizeNWSE, ResizeDirection.BottomRight);

            bottomLeft.DragDelta += HandleBottomLeft;
            bottomRight.DragDelta += HandleBottomRight;
            topLeft.DragDelta += HandleTopLeft;
            topRight.DragDelta += HandleTopRight;
        }

        protected override int VisualChildrenCount => visualChildren.Count;
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

        private void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            if (!(AdornedElement is FrameworkElement adornedElement) ||
                !(sender is Thumb hitThumb)) return;

            EnforceSize(adornedElement);

            adornedElement.Width = Math.Max(
                adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(
                args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
        }

        private void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            if (!(AdornedElement is FrameworkElement adornedElement) ||
                !(sender is Thumb hitThumb)) return;

            EnforceSize(adornedElement);

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
            if (!(AdornedElement is FrameworkElement adornedElement) ||
                !(sender is Thumb hitThumb)) return;

            EnforceSize(adornedElement);

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
            if (!(AdornedElement is FrameworkElement adornedElement) ||
                !(sender is Thumb hitThumb)) return;

            EnforceSize(adornedElement);

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
            // desiredWidth and desiredHeight are the width and height of the adorned element.
            // These will be used to place the ResizingAdorner at the corners of the adorned element.
            var desiredWidth = AdornedElement.DesiredSize.Width;
            var desiredHeight = AdornedElement.DesiredSize.Height;
            var adornerWidth = DesiredSize.Width;
            var adornerHeight = DesiredSize.Height;

            var left = -adornerWidth / 2;
            var right = desiredWidth - adornerWidth / 2;
            var top = -adornerHeight / 2;
            var bottom = desiredHeight - adornerHeight / 2;
            topLeft.Arrange(new Rect(left, top, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(right, top, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(left, bottom, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(right, bottom, adornerWidth, adornerHeight));

            return finalSize;
        }

        private void CreateThumb(
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

        /// <devdoc>
        ///   Ensures that Width and Height are initialized. Sizing to content
        ///   produces Width and Height values of Double.NaN. Because this adorner
        ///   explicitly resizes, the Width and Height need to be set first. It
        ///   also sets the maximum size of the adorned element.
        /// </devdoc>
        private static void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            if (adornedElement.Parent is FrameworkElement parent) {
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
            var newGeo = new StreamGeometry();

            double w = ActualWidth;
            double h = ActualHeight;

            using (var ctx = newGeo.Open()) {
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

            return newGeo;
        }
    }
}
