namespace ThemeBrowser
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ThemeBrowser.Extensions;

    public class ImageColorPickerBehavior : Behavior<Image>
    {
        private readonly ToolTip toolTip = new ToolTip {
            Placement = PlacementMode.Absolute
        };
        private Optional<object> oldToolTip = new Optional<object>();
        private Optional<Cursor> oldCursor = new Optional<Cursor>();
        private bool suppressMouseUp;

        private struct Optional<T>
        {
            public T Value { get; private set; }
            public bool HasValue { get; private set; }

            public T Clear()
            {
                var ret = Value;
                Value = default(T);
                HasValue = false;
                return ret;
            }

            public void Set(T value)
            {
                Value = value;
                HasValue = true;
            }

            public void SetIfEmpty(T value)
            {
                if (!HasValue) {
                    Value = value;
                    HasValue = true;
                }
            }
        }

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

        private static readonly DependencyPropertyKey PixelColorPremultipliedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PixelColorPremultiplied),
                typeof(Color?),
                typeof(ImageColorPickerBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PixelColorPremultipliedProperty =
            PixelColorPremultipliedPropertyKey.DependencyProperty;

        public Color? PixelColorPremultiplied
        {
            get => (Color?)GetValue(PixelColorPremultipliedProperty);
            private set => SetValue(PixelColorPremultipliedPropertyKey, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeave += OnMouseLeave;
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            HidePicker();
            AssociatedObject.MouseUp -= OnMouseUp;
            AssociatedObject.MouseDown -= OnMouseDown;
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
            HidePicker();

            var window = Window.GetWindow(AssociatedObject);
            if (window == null)
                return;

            window.RemoveHandler(
                Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnKeyDown));
            window.RemoveHandler(
                Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift)
                return;

            Color? color = null;
            switch (args.ChangedButton) {
                case MouseButton.Left:
                    color = PixelColor;
                    break;
                case MouseButton.Right:
                    color = PixelColorPremultiplied;
                    break;
            }

            if (color != null) {
                Clipboard.SetText(color.Value.ToString());
                args.Handled = true;
                suppressMouseUp = true;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (suppressMouseUp) {
                args.Handled = true;
                suppressMouseUp = false;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.LeftShift || args.Key == Key.LeftShift)
                DoPick();
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.LeftShift || args.Key == Key.LeftShift)
                HidePicker();
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                DoPick();
        }

        private void DoPick()
        {
            if (!toolTip.IsOpen)
                ShowPicker();

            var localPos = Mouse.GetPosition(AssociatedObject);
            var color = GetPixelColor(AssociatedObject, localPos, out int x, out int y);
            if (color == null) {
                PixelColor = null;
                PixelColorPremultiplied = null;
            } else {
                PixelColor = color.Value;
                PixelColorPremultiplied = color.Value.Premultiply();
            }

            Point screenPos = AssociatedObject.PointToScreen(localPos);
            toolTip.HorizontalOffset = screenPos.X + 10;
            toolTip.VerticalOffset = screenPos.Y + 10;
            toolTip.Content =
                $"X: {x}, Y: {y}\n" +
                $"Color: {PixelColor}\n" +
                $"Color (Premultiplied): {PixelColorPremultiplied}";
        }

        private void ShowPicker()
        {
            oldCursor.SetIfEmpty(AssociatedObject.Cursor);
            oldToolTip.SetIfEmpty(AssociatedObject.ToolTip);

            AssociatedObject.Cursor = Cursors.Cross;
            AssociatedObject.ToolTip = toolTip;
            toolTip.IsOpen = true;
        }

        private void HidePicker()
        {
            toolTip.IsOpen = false;
            if (oldToolTip.HasValue)
                AssociatedObject.Cursor = oldCursor.Clear();
            if (oldToolTip.HasValue)
                AssociatedObject.ToolTip = oldToolTip.Clear();
        }

        private static bool IsIdentityOrNull(Transform transform)
        {
            return transform == null || Equals(transform, Transform.Identity);
        }

        private static Color? GetPixelColor(Image image, Point point, out int x, out int y)
        {
            BitmapSource bitmap;
            if (IsIdentityOrNull(image.RenderTransform) &&
                IsIdentityOrNull(image.LayoutTransform) &&
                image.Source is BitmapSource) {

                bitmap = (BitmapSource)image.Source;

                var scaleX = bitmap.PixelWidth / image.ActualWidth;
                var scaleY = bitmap.PixelHeight / image.ActualHeight;

                point.X *= scaleX;
                point.Y *= scaleY;
            } else {
                // Use RenderTargetBitmap to get the visual, in case the image has
                // been transformed.
                var renderBitmap = new RenderTargetBitmap(
                    (int)image.ActualWidth,
                    (int)image.ActualHeight,
                    96, 96, PixelFormats.Default);
                renderBitmap.Render(image);
                bitmap = renderBitmap;
            }

            x = (int)point.X;
            y = (int)point.Y;
            return GetPixelColor(bitmap, x, y);
        }

        private static unsafe Color? GetPixelColor(BitmapSource bitmap, int x, int y)
        {
            if (x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
                return null;

            var pixels = stackalloc byte[4];
            int stride = (bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), (IntPtr)pixels, 4, stride);

            var color = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            if (bitmap.Format == PixelFormats.Pbgra32)
                return color.Unpremultiply();
            return color;
        }
    }
}
