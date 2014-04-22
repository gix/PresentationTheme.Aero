namespace StyleInspector
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;
    using StyleCore;
    using StyleCore.Native;
    using Color = StyleCore.Color;

    public abstract class ThemePropertyContainer
    {
        public abstract IReadOnlyList<ThemePropertyViewModel> Properties { get; }
        public abstract IReadOnlyList<ThemePropertyViewModel> AllProperties { get; }
    }

    public abstract class ThemePropertyViewModel
    {
        public abstract string DisplayName { get; }
        public abstract string DisplayPrimitiveType { get; }
        public abstract TMT PropertyId { get; }
        public abstract TMT PrimitiveType { get; }
        public abstract PropertyOrigin Origin { get; }
        public abstract object Value { get; }

        public PropertyOrigin SimpleOrigin => Origin & ~PropertyOrigin.Inherited;
    }

    public class OwnedThemePropertyViewModel : ThemePropertyViewModel
    {
        private readonly ThemeProperty property;

        public OwnedThemePropertyViewModel(ThemeProperty property)
        {
            this.property = property;
            Value = ConvertValue(property);
        }

        private static object ConvertValue(ThemeProperty property)
        {
            var bitmap = property.Value as ThemeBitmap;
            if (bitmap != null)
                return new ThemeBitmapViewModel(bitmap);
            if (property.Value is Color)
                return ((Color)property.Value).ToWpfColor();
            if (property.Value is HIGHCONTRASTCOLOR)
                return new HighContrastColor(((HIGHCONTRASTCOLOR)property.Value));
            return property.Value;
        }

        public override TMT PropertyId => property.PropertyId;
        public override TMT PrimitiveType => property.PrimitiveType;
        public override PropertyOrigin Origin => property.Origin;
        public override object Value { get; }

        public override string DisplayName
        {
            get
            {
                if (!Enum.IsDefined(typeof(TMT), PropertyId))
                    return $"{PropertyId:D}";
                return $"{PropertyId} [{PropertyId:D}]";
            }
        }

        public override string DisplayPrimitiveType
        {
            get
            {
                if (!Enum.IsDefined(typeof(TMT), PrimitiveType))
                    return PrimitiveType.ToString("D");
                return PrimitiveType + " [" + PrimitiveType.ToString("D") + "]";
            }
        }
    }

    public class InheritedThemePropertyViewModel : ThemePropertyViewModel
    {
        private readonly ThemePropertyViewModel property;

        public InheritedThemePropertyViewModel(ThemePropertyViewModel property)
        {
            this.property = property;
            Origin = property.Origin | PropertyOrigin.Inherited;
        }

        public override string DisplayName => property.DisplayName;
        public override string DisplayPrimitiveType => property.DisplayPrimitiveType;
        public override TMT PropertyId => property.PropertyId;
        public override TMT PrimitiveType => property.PrimitiveType;
        public override PropertyOrigin Origin { get; }
        public override object Value => property.Value;
    }

    public class ThemeBitmapViewModel : ViewModel
    {
        private readonly ThemeBitmap themeBitmap;
        private readonly Lazy<BitmapImage> bitmap;

        public ThemeBitmapViewModel(ThemeBitmap themeBitmap)
        {
            this.themeBitmap = themeBitmap;
            bitmap = new Lazy<BitmapImage>(LoadBitmap);
        }

        public int ImageId => themeBitmap.ImageId;
        public BitmapImage Bitmap => bitmap.Value;

        public Stream OpenStream()
        {
            return themeBitmap.OpenStream();
        }

        public BitmapImage LoadBitmap()
        {
            using (var stream = OpenStream()) {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public Bitmap LoadDrawingBitmap()
        {
            {
                var src = LoadBitmap();
                var b = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var data = b.LockBits(new Rectangle(Point.Empty, b.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                src.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                b.UnlockBits(data);
                return b;
            }

            using (var stream = OpenStream())
                return new Bitmap(stream);
        }
    }
}
