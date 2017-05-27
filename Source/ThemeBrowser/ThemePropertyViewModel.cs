namespace ThemeBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using StyleCore;
    using StyleCore.Native;
    using Color = StyleCore.Color;

    public abstract class ThemePropertyContainer
    {
        public abstract IReadOnlyList<ThemePropertyViewModel> Properties { get; }
        public abstract void AddDefaultProperty(TMT propertyId, TMT primitiveType, object value);

        public bool HasProperty(TMT propertyId)
        {
            return Properties.Any(x => x.PropertyId == propertyId);
        }

        public bool Find<T>(TMT propertyId, out T value)
        {
            var entry = Properties.FirstOrDefault(x => x.PropertyId == propertyId);
            if (entry == null) {
                value = default(T);
                return false;
            }

            value = (T)entry.RawValue;
            return true;
        }

        public T EnsurePropertyValue<T>(TMT propertyId, TMT primitiveType, T defaultValue)
        {
            T value;
            if (Find(propertyId, out value))
                return value;

            AddDefaultProperty(propertyId, primitiveType, defaultValue);
            return defaultValue;
        }

        public bool EnsureBoolValue(TMT propertyId, bool defaultValue)
        {
            return EnsurePropertyValue(propertyId, TMT.BOOL, defaultValue);
        }

        public int EnsureIntValue(TMT propertyId, int defaultValue)
        {
            return EnsurePropertyValue(propertyId, TMT.INT, defaultValue);
        }

        public Color EnsureColorValue(TMT propertyId, Color defaultValue)
        {
            return EnsurePropertyValue(propertyId, TMT.COLOR, defaultValue);
        }

        public MARGINS EnsureMarginsValue(TMT propertyId, MARGINS defaultValue)
        {
            return EnsurePropertyValue(propertyId, TMT.MARGINS, defaultValue);
        }

        public T EnsureEnumValue<T>(TMT propertyId, T defaultValue)
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Type must be enum");
            return EnsurePropertyValue(propertyId, TMT.ENUM, defaultValue);
        }
    }

    public abstract class ThemePropertyViewModel
    {
        public abstract string DisplayName { get; }
        public abstract string DisplayPrimitiveType { get; }
        public abstract TMT PropertyId { get; }
        public abstract TMT PrimitiveType { get; }
        public abstract PropertyOrigin Origin { get; }
        public abstract object Value { get; }
        public abstract object RawValue { get; }
        public abstract string StringValue { get; }

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
            switch (property.Value) {
                case ThemeBitmap bitmap:
                    return new ThemeBitmapViewModel(bitmap);
                case Color color:
                    return color.ToWpfColor();
                case HIGHCONTRASTCOLOR hcc:
                    return new HighContrastColor(hcc);
                case IMAGEPROPERTIES[] images:
                    return new SimplifiedImageGroup(images);
                case HCIMAGEPROPERTIES[] images:
                    return new HighContrastSimplifiedImageGroup(images);
            }

            if (property.Value != null && property.Value.GetType().IsEnum)
                return $"{property.Value} [{(int)property.Value}]";

            return property.Value;
        }

        public override TMT PropertyId => property.PropertyId;
        public override TMT PrimitiveType => property.PrimitiveType;
        public override PropertyOrigin Origin => property.Origin;
        public override object Value { get; }
        public override object RawValue => property.Value;
        public override string StringValue => Value?.ToString();

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
        public override object RawValue => property.RawValue;
        public override string StringValue => property.StringValue;
    }

    public class SimplifiedImage
    {
        public SimplifiedImage(IMAGEPROPERTIES imageProperties)
        {
            BorderColor = ThemeExt.ColorFromArgb(imageProperties.BorderColor).ToWpfColor();
            BackgroundColor = ThemeExt.ColorFromArgb(imageProperties.BackgroundColor).ToWpfColor();
        }

        public System.Windows.Media.Color BorderColor { get; }
        public System.Windows.Media.Color BackgroundColor { get; }

        public override string ToString() => $"{BorderColor}, {BackgroundColor}";
    }

    public class HighContrastSimplifiedImage
    {
        public HighContrastSimplifiedImage(HCIMAGEPROPERTIES imageProperties)
        {
            BorderColor = new HighContrastColor((HIGHCONTRASTCOLOR)imageProperties.BorderColor);
            BackgroundColor = new HighContrastColor((HIGHCONTRASTCOLOR)imageProperties.BackgroundColor);
        }

        public HighContrastColor BorderColor { get; }
        public HighContrastColor BackgroundColor { get; }

        public override string ToString() => $"{BorderColor}, {BackgroundColor}";
    }

    public class SimplifiedImageGroup
    {
        public SimplifiedImageGroup(IEnumerable<IMAGEPROPERTIES> images)
        {
            Images = images.Select(x => new SimplifiedImage(x)).ToList();
        }

        public IReadOnlyList<SimplifiedImage> Images { get; }
    }

    public class HighContrastSimplifiedImageGroup
    {
        public HighContrastSimplifiedImageGroup(IEnumerable<HCIMAGEPROPERTIES> images)
        {
            Images = images.Select(x => new HighContrastSimplifiedImage(x)).ToList();
        }

        public IReadOnlyList<HighContrastSimplifiedImage> Images { get; }
    }

    public class ThemeBitmapViewModel : ViewModel
    {
        private readonly Lazy<BitmapImage> bitmap;

        public ThemeBitmapViewModel(ThemeBitmap themeBitmap)
        {
            ThemeBitmap = themeBitmap;
            bitmap = new Lazy<BitmapImage>(LoadBitmap);
        }

        public ThemeBitmap ThemeBitmap { get; }
        public int ImageId => ThemeBitmap.ImageId;
        public BitmapImage Bitmap => bitmap.Value;

        public Stream OpenStream()
        {
            return ThemeBitmap.OpenStream();
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
            using (var stream = OpenStream())
                return new Bitmap(stream);
        }
    }
}
