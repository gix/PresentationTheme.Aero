namespace ThemeBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Extensions;
    using ThemeCore;
    using ThemeCore.Native;
    using Color = ThemeCore.Color;

    public enum ComparisonResult
    {
        None,
        Removed,
        Added,
        Unchanged
    }

    public static class ComparisonUtils
    {
        public static ComparisonResult GetResult(bool oldPresent, bool newPresent)
        {
            if (oldPresent && newPresent)
                return ComparisonResult.Unchanged;
            if (oldPresent)
                return ComparisonResult.Removed;
            return ComparisonResult.Added;
        }
    }

    public class ThemeFileComparisonBuilder
    {
        public ThemeFileComparisonViewModel CompareThemes(
                ThemeFileViewModel oldThemeFile, ThemeFileViewModel newThemeFile)
        {
            var cmpModel = new ThemeFileComparisonViewModel(oldThemeFile, newThemeFile);

            var items = MatchItems(oldThemeFile.Classes, newThemeFile.Classes,
                (l, r) => string.Compare(l.Name, r.Name, StringComparison.OrdinalIgnoreCase));

            foreach (var tuple in items) {
                var cmp = Compare(tuple.Item1, tuple.Item2);
                if (cmp != null)
                    cmpModel.Classes.Add(cmp);
            }

            cmpModel.Properties = Compare(oldThemeFile.Properties, newThemeFile.Properties);

            return cmpModel;
        }

        public ThemeClassComparisonViewModel Compare(
            ThemeClassViewModel oldClass, ThemeClassViewModel newClass)
        {
            var cmpModel = new ThemeClassComparisonViewModel(oldClass, newClass);

            if (oldClass != null && newClass != null) {
                var items = MatchItems(oldClass.Parts, newClass.Parts, (l, r) => l.Id.CompareTo(r.Id));
                foreach (var tuple in items) {
                    var cmp = Compare(tuple.Item1, tuple.Item2);
                    if (cmp != null)
                        cmpModel.Parts.Add(cmp);
                }

                cmpModel.Properties = Compare(oldClass.Properties, newClass.Properties);

                if (cmpModel.Parts.Count == 0 && cmpModel.Properties.Count == 0)
                    return null;
            }

            return cmpModel;
        }

        public ThemePartComparisonViewModel Compare(
            ThemePartViewModel oldPart, ThemePartViewModel newPart)
        {
            var cmpModel = new ThemePartComparisonViewModel(oldPart, newPart);

            if (oldPart != null && newPart != null) {
                var items = MatchItems(oldPart.States, newPart.States, (l, r) => l.Id.CompareTo(r.Id));
                foreach (var tuple in items) {
                    var cmp = Compare(tuple.Item1, tuple.Item2);
                    if (cmp != null)
                        cmpModel.States.Add(cmp);
                }

                cmpModel.Properties = Compare(oldPart.Properties, newPart.Properties);

                if (cmpModel.States.Count == 0 && cmpModel.Properties.Count == 0)
                    return null;
            }

            return cmpModel;
        }

        public ThemeStateComparisonViewModel Compare(
            ThemeStateViewModel oldState, ThemeStateViewModel newState)
        {
            var cmpModel = new ThemeStateComparisonViewModel(oldState, newState);
            if (oldState != null && newState != null) {
                cmpModel.Properties = Compare(oldState.Properties, newState.Properties);
                if (cmpModel.Properties.Count == 0)
                    return null;
            }

            return cmpModel;
        }

        public List<ThemePropertyComparisonViewModel> Compare(
            IReadOnlyList<ThemePropertyViewModel> oldProperties,
            IReadOnlyList<ThemePropertyViewModel> newProperties)
        {
            var result = new List<ThemePropertyComparisonViewModel>();

            var items = MatchItems(oldProperties, newProperties, (l, r) => l.PropertyId.CompareTo(r.PropertyId));
            foreach (var tuple in items) {
                var cmp = Compare(tuple.Item1, tuple.Item2);
                if (cmp != null)
                    result.Add(cmp);
            }

            return result;
        }

        public ThemePropertyComparisonViewModel Compare(
            ThemePropertyViewModel oldProperty,
            ThemePropertyViewModel newProperty)
        {
            if (oldProperty != null && newProperty != null) {
                int cmp = Compare(oldProperty.Value, newProperty.Value);
                if (cmp == 0)
                    return null;
            }

            return new ThemePropertyComparisonViewModel(oldProperty, newProperty);
        }

        private static int Compare(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
                return 0;
            if (oldValue == null)
                return -1;
            if (newValue == null)
                return 1;

            if (oldValue.GetType() != newValue.GetType())
                return 1;

            switch (oldValue) {
                case string value: return CompareValue(value, (string)newValue);
                case int value: return CompareValue(value, (int)newValue);
                case MARGINS value: return CompareValue(value, (MARGINS)newValue);
                case POINT value: return CompareValue(value, (POINT)newValue);
                case RECT value: return CompareValue(value, (RECT)newValue);
                case byte[] value: return CompareValue(value, (byte[])newValue);
                case bool value: return CompareValue(value, (bool)newValue);
                case IntList value: return CompareValue(value, (IntList)newValue);
                case FontInfo value: return CompareValue(value, (FontInfo)newValue);
                case Color value: return CompareValue(value, (Color)newValue);
                case HIGHCONTRASTCOLOR value: return CompareValue(value, (HIGHCONTRASTCOLOR)newValue);
                case ThemeBitmap value: return CompareValue(value, (ThemeBitmap)newValue);

                case System.Windows.Media.Color value: return CompareValue(value, (System.Windows.Media.Color)newValue);
                case HighContrastColor value: return CompareValue(value, (HighContrastColor)newValue);
                case ThemeBitmapViewModel value: return CompareValue(value, (ThemeBitmapViewModel)newValue);
                case SimplifiedImageGroup value: return CompareValue(value, (SimplifiedImageGroup)newValue);
                case HighContrastSimplifiedImageGroup value: return CompareValue(value, (HighContrastSimplifiedImageGroup)newValue);

                default:
                    Debug.Assert(false, $"Comparison missing for value of type '{oldValue?.GetType()}'");
                    return 1;
            }
        }

        private static int CompareValue(string oldValue, string newValue)
        {
            return string.CompareOrdinal(oldValue, newValue);
        }

        private static int CompareValue(int oldValue, int newValue)
        {
            return oldValue.CompareTo(newValue);
        }

        private static int CompareValue(bool oldValue, bool newValue)
        {
            return oldValue.CompareTo(newValue);
        }

        private static int CompareValue(HIGHCONTRASTCOLOR oldValue, HIGHCONTRASTCOLOR newValue)
        {
            return oldValue.CompareTo(newValue);
        }

        private static int CompareValue(HighContrastColor oldValue, HighContrastColor newValue)
        {
            return oldValue.Index.CompareTo(newValue.Index);
        }

        private static int CompareValue(SimplifiedImageGroup oldValue, SimplifiedImageGroup newValue)
        {
            return CompareSequence(oldValue.Images, newValue.Images, CompareValue);
        }

        private static int CompareValue(SimplifiedImage oldValue, SimplifiedImage newValue)
        {
            int cmp = CompareValue(oldValue.BackgroundColor, newValue.BackgroundColor);
            if (cmp == 0) cmp = CompareValue(oldValue.BorderColor, newValue.BorderColor);
            return cmp;
        }

        private static int CompareValue(HighContrastSimplifiedImageGroup oldValue, HighContrastSimplifiedImageGroup newValue)
        {
            return CompareSequence(oldValue.Images, newValue.Images, CompareValue);
        }

        private static int CompareValue(HighContrastSimplifiedImage oldValue, HighContrastSimplifiedImage newValue)
        {
            int cmp = CompareValue(oldValue.BackgroundColor, newValue.BackgroundColor);
            if (cmp == 0) cmp = CompareValue(oldValue.BorderColor, newValue.BorderColor);
            return cmp;
        }

        private static int CompareValue(ThemeBitmap oldValue, ThemeBitmap newValue)
        {
            BitmapImage oldImage = LoadImage(oldValue);
            BitmapImage newImage = LoadImage(newValue);

            int cmp = oldImage.PixelWidth.CompareTo(newImage.PixelWidth);
            if (cmp == 0) cmp = oldImage.PixelHeight.CompareTo(newImage.PixelHeight);
            if (cmp == 0) cmp = oldImage.Format.BitsPerPixel.CompareTo(newImage.Format.BitsPerPixel);
            if (cmp == 0) {
                int pixelWidth = oldImage.PixelWidth;
                int pixelHeight = oldImage.PixelHeight;
                int stride = pixelWidth * (oldImage.Format.BitsPerPixel + 7 / 8);
                int size = stride * pixelHeight;

                var oldPixels = new byte[size];
                var newPixels = new byte[size];
                oldImage.CopyPixels(Int32Rect.Empty, oldPixels, stride, 0);
                newImage.CopyPixels(Int32Rect.Empty, newPixels, stride, 0);
                cmp = CompareSequence(oldPixels, newPixels);
            }

            return cmp;
        }

        private static BitmapImage LoadImage(ThemeBitmap oldValue)
        {
            using (var stream = oldValue.OpenStream())
                return ImagingUtils.LoadBitmapImageFromStream(stream);
        }

        private static int CompareValue(ThemeBitmapViewModel oldValue, ThemeBitmapViewModel newValue)
        {
            return CompareValue(oldValue.ThemeBitmap, newValue.ThemeBitmap);
        }

        private static int CompareValue(Color oldValue, Color newValue)
        {
            return oldValue.ToArgb().CompareTo(newValue.ToArgb());
        }

        private static int CompareValue(FontInfo oldValue, FontInfo newValue)
        {
            int cmp = oldValue.PointSize.CompareTo(newValue.PointSize);
            if (cmp == 0) cmp = string.Compare(oldValue.FontFamily, newValue.FontFamily, StringComparison.OrdinalIgnoreCase);
            if (cmp == 0) cmp = string.Compare(oldValue.Options, newValue.Options, StringComparison.OrdinalIgnoreCase);
            return cmp;
        }

        private static int CompareValue(IntList oldValue, IntList newValue)
        {
            return CompareSequence(oldValue, newValue);
        }

        private static int CompareValue(byte[] oldValue, byte[] newValue)
        {
            return CompareSequence(oldValue, newValue);
        }

        private static int CompareValue(System.Windows.Media.Color oldValue, System.Windows.Media.Color newValue)
        {
            return oldValue.ToArgb().CompareTo(newValue.ToArgb());
        }

        private static int CompareSequence<T>(IReadOnlyList<T> lhs, IReadOnlyList<T> rhs)
            where T : IComparable<T>
        {
            return CompareSequence(lhs, rhs, (l, r) => l.CompareTo(r));
        }

        private static int CompareSequence<T>(
            IReadOnlyList<T> lhs, IReadOnlyList<T> rhs, Func<T, T, int> compare)
        {
            int cmp = lhs.Count.CompareTo(rhs.Count);
            if (cmp != 0)
                return cmp;

            for (int i = 0; i < lhs.Count; ++i) {
                cmp = compare(lhs[i], rhs[i]);
                if (cmp != 0)
                    return cmp;
            }

            return 0;
        }

        private static int CompareValue(MARGINS oldValue, MARGINS newValue)
        {
            int cmp = oldValue.cxLeftWidth.CompareTo(newValue.cxLeftWidth);
            if (cmp == 0) cmp = oldValue.cxRightWidth.CompareTo(newValue.cxRightWidth);
            if (cmp == 0) cmp = oldValue.cyTopHeight.CompareTo(newValue.cyTopHeight);
            if (cmp == 0) cmp = oldValue.cyBottomHeight.CompareTo(newValue.cyBottomHeight);
            return cmp;
        }

        private static int CompareValue(RECT oldValue, RECT newValue)
        {
            int cmp = oldValue.left.CompareTo(newValue.left);
            if (cmp == 0) cmp = oldValue.top.CompareTo(newValue.top);
            if (cmp == 0) cmp = oldValue.right.CompareTo(newValue.right);
            if (cmp == 0) cmp = oldValue.bottom.CompareTo(newValue.bottom);
            return cmp;
        }

        private static int CompareValue(POINT oldValue, POINT newValue)
        {
            int cmp = oldValue.x.CompareTo(newValue.x);
            if (cmp == 0) cmp = oldValue.y.CompareTo(newValue.y);
            return cmp;
        }

        private static IEnumerable<Tuple<T, T>> MatchItems<T>(
            IEnumerable<T> lhs, IEnumerable<T> rhs,
            Comparison<T> comparison)
            where T : class
        {
            var lhsItems = lhs.ToList();
            var rhsItems = rhs.ToList();

            lhsItems.Sort(comparison);
            rhsItems.Sort(comparison);

            int left = 0;
            int right = 0;
            while (left < lhsItems.Count && right < rhsItems.Count) {
                int cmp = comparison(lhsItems[left], rhsItems[right]);
                if (cmp < 0)
                    yield return Tuple.Create(lhsItems[left++], (T)null);
                else if (cmp > 0)
                    yield return Tuple.Create((T)null, rhsItems[right++]);
                else
                    yield return Tuple.Create(lhsItems[left++], rhsItems[right++]);
            }

            while (left < lhsItems.Count)
                yield return Tuple.Create(lhsItems[left++], (T)null);
            while (right < rhsItems.Count)
                yield return Tuple.Create((T)null, rhsItems[right++]);
        }
    }

    public class ThemeFileComparisonViewModel : ThemeFileBase
    {
        public ThemeFileComparisonViewModel(
            ThemeFileViewModel oldThemeFile, ThemeFileViewModel newThemeFile)
        {
            OldThemeFile = oldThemeFile;
            NewThemeFile = newThemeFile;
        }

        public ThemeFileViewModel OldThemeFile { get; }
        public ThemeFileViewModel NewThemeFile { get; }

        public ObservableCollection<ThemeClassComparisonViewModel> Classes { get; }
            = new ObservableCollection<ThemeClassComparisonViewModel>();
        public List<ThemePropertyComparisonViewModel> Properties { get; set; }

        public override void Dispose()
        {
            OldThemeFile?.Dispose();
            NewThemeFile?.Dispose();
        }
    }

    public abstract class ThemePropertyComparisonContainer : ViewModel
    {
        public IReadOnlyList<ThemePropertyComparisonViewModel> Properties { get; set; }
    }

    public class ThemeClassComparisonViewModel : ThemePropertyComparisonContainer
    {
        public ThemeClassComparisonViewModel(
            ThemeClassViewModel oldClass, ThemeClassViewModel newClass)
        {
            OldClass = oldClass;
            NewClass = newClass;
            Comparison = ComparisonUtils.GetResult(oldClass != null, newClass != null);
            DisplayName = OldClass?.Name ?? NewClass?.Name;
        }

        public string DisplayName { get; }
        public ComparisonResult Comparison { get; }
        public ThemeClassViewModel OldClass { get; }
        public ThemeClassViewModel NewClass { get; }

        public ObservableCollection<ThemePartComparisonViewModel> Parts { get; }
            = new ObservableCollection<ThemePartComparisonViewModel>();
    }

    public class ThemePartComparisonViewModel : ThemePropertyComparisonContainer
    {
        public ThemePartComparisonViewModel(
            ThemePartViewModel oldPart, ThemePartViewModel newPart)
        {
            OldPart = oldPart;
            NewPart = newPart;
            Comparison = ComparisonUtils.GetResult(oldPart != null, newPart != null);
            DisplayName = NewPart?.DisplayName ?? OldPart?.DisplayName;
        }

        public string DisplayName { get; }
        public ComparisonResult Comparison { get; }
        public ThemePartViewModel OldPart { get; }
        public ThemePartViewModel NewPart { get; }

        public ObservableCollection<ThemeStateComparisonViewModel> States { get; }
            = new ObservableCollection<ThemeStateComparisonViewModel>();
    }

    public class ThemeStateComparisonViewModel : ThemePropertyComparisonContainer
    {
        public ThemeStateComparisonViewModel(
            ThemeStateViewModel oldState, ThemeStateViewModel newState)
        {
            OldState = oldState;
            NewState = newState;
            Comparison = ComparisonUtils.GetResult(oldState != null, newState != null);
            DisplayName = NewState?.DisplayName ?? OldState?.DisplayName;
        }

        public string DisplayName { get; }
        public ComparisonResult Comparison { get; }
        public ThemeStateViewModel OldState { get; }
        public ThemeStateViewModel NewState { get; }
    }

    public class ThemePropertyComparisonViewModel : ViewModel
    {
        public ThemePropertyComparisonViewModel(
            ThemePropertyViewModel oldProperty, ThemePropertyViewModel newProperty)
        {
            if (oldProperty == null && newProperty == null)
                throw new ArgumentException();

            OldProperty = oldProperty;
            NewProperty = newProperty;
            Comparison = ComparisonUtils.GetResult(oldProperty != null, newProperty != null);

            if (newProperty != null) {
                DisplayName = newProperty.DisplayName;
                PropertyId = newProperty.PropertyId;
                PrimitiveType = newProperty.DisplayPrimitiveType;
            } else {
                DisplayName = oldProperty.DisplayName;
                PropertyId = oldProperty.PropertyId;
                PrimitiveType = oldProperty.DisplayPrimitiveType;
            }
        }

        public string DisplayName { get; }
        public TMT PropertyId { get; }
        public string PrimitiveType { get; }
        public ComparisonResult Comparison { get; }
        public ThemePropertyViewModel OldProperty { get; }
        public ThemePropertyViewModel NewProperty { get; }
    }

    public class ComparisonResultToBrushConverter : IValueConverter
    {
        public Brush UnchangedBrush { get; set; } = SystemColors.ControlTextBrush;
        public Brush AddedBrush { get; set; } = Brushes.Green;
        public Brush RemovedBrush { get; set; } = Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ComparisonResult))
                return Binding.DoNothing;

            switch ((ComparisonResult)value) {
                case ComparisonResult.Added: return AddedBrush;
                case ComparisonResult.Removed: return RemovedBrush;
                default: return UnchangedBrush;
            }
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
