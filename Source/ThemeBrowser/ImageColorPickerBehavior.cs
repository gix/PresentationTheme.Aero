namespace StyleInspector
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class ImageColorPickerBehavior : Behavior<Image>
    {
        private static readonly DependencyPropertyKey PixelColorPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PixelColor),
                typeof(Color?),
                typeof(ImageColorPickerBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PixelColorProperty =
            PixelColorPropertyKey.DependencyProperty;

        public Color? PixelColor
        {
            get => (Color?)GetValue(PixelColorProperty);
            private set => SetValue(PixelColorPropertyKey, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeave -= OnMouseLeave;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseEnter -= OnMouseEnter;
            base.OnDetaching();
        }

        private void OnMouseEnter(object sender, MouseEventArgs args)
        {
            var window = Window.GetWindow(AssociatedObject);
            if (window == null)
                return;

            window.AddHandler(
                Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnKeyDown), true);
            window.AddHandler(
                Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp), true);
        }

        private void OnMouseLeave(object sender, MouseEventArgs args)
        {
            var window = Window.GetWindow(AssociatedObject);
            if (window == null)
                return;

            window.RemoveHandler(
                Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnKeyDown));
            window.RemoveHandler(
                Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp));
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.LeftCtrl && args.Key != Key.RightCtrl)
                return;

            AssociatedObject.Cursor = Cursors.Cross;
            Point point = Mouse.GetPosition(AssociatedObject);
            PixelColor = GetPixelColor(AssociatedObject, point);
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            AssociatedObject.Cursor = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            Point point = args.GetPosition(AssociatedObject);
            PixelColor = GetPixelColor(AssociatedObject, point);
        }

        private static bool IsIdentityOrNull(Transform transform)
        {
            return transform == null || Equals(transform, Transform.Identity);
        }

        private static Color? GetPixelColor(Image image, Point point)
        {
            if (IsIdentityOrNull(image.RenderTransform) &&
                IsIdentityOrNull(image.LayoutTransform) &&
                image.Source is BitmapSource) {

                var bitmap = (BitmapSource)image.Source;

                var scaleX = bitmap.PixelWidth / image.ActualWidth;
                var scaleY = bitmap.PixelHeight / image.ActualHeight;

                point.X *= scaleX;
                point.Y *= scaleY;

                return GetPixelColor(bitmap, (int)point.X, (int)point.Y);
            } else {
                // Use RenderTargetBitmap to get the visual, in case the image has
                // been transformed.
                var bitmap = new RenderTargetBitmap((int)image.ActualWidth,
                    (int)image.ActualHeight,
                    96, 96, PixelFormats.Default);
                bitmap.Render(image);
                return GetPixelColor(bitmap, (int)point.X, (int)point.Y);
            }
        }

        private static Color? GetPixelColor(BitmapSource bitmap, int x, int y)
        {
            if (x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
                return null;

            var cropped = new CroppedBitmap(bitmap, new Int32Rect(x, y, 1, 1));

            var pixels = new byte[4];
            cropped.CopyPixels(pixels, 4, 0);

            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }
    }
}
