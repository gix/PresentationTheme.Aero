namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Microsoft.Win32;
    using StyleCore.Native;

    /// <summary>Interaction logic for Colors.xaml</summary>
    internal partial class ColorList : INotifyPropertyChanged
    {
        /// <summary>
        ///   Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                "Label",
                typeof(string),
                typeof(ColorList),
                new PropertyMetadata(null));

        public ColorList()
        {
            InitializeComponent();
            DataContext = this;

            Label = "Colors";

            SystemEvents.UserPreferenceChanged += (s, e) => Refresh();
            Refresh();
        }

        private void Refresh()
        {
            var colors = new List<ColorSpec>();
            foreach (var property in typeof(SystemColors).GetProperties(BindingFlags.Public | BindingFlags.Static)) {
                if (property.PropertyType != typeof(Color))
                    continue;
                colors.Add(new ColorSpec($"System → {property.Name}", (Color)property.GetValue(null)));
            }

            var theme = new ThemeColors(new WindowInteropHelper(Application.Current.MainWindow).Handle, "LISTVIEW");
            foreach (var property in typeof(ThemeColors).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (property.PropertyType != typeof(Color))
                    continue;
                colors.Add(new ColorSpec($"ListView → {property.Name}", (Color)property.GetValue(theme)));
            }

            colors.Sort((l, r) => string.Compare(l.Name, r.Name, StringComparison.Ordinal));

            Colors.Clear();
            foreach (var color in colors)
                Colors.Add(color);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public ObservableCollection<ColorSpec> Colors { get; } =
            new ObservableCollection<ColorSpec>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal sealed class ColorSpec
    {
        private readonly Color color;
        private Brush brush;

        public ColorSpec(string name, Color color)
        {
            Name = name;
            this.color = color;
        }

        public string Name { get; }

        public string StringValue => color.ToString();

        public Brush Brush => brush ?? (brush = new SolidColorBrush(color));
    }

    internal sealed class ThemeColors : IDisposable
    {
        private readonly SolidColorBrush[] BrushCache = new SolidColorBrush[30];
        private readonly BitArray BrushCacheValid = new BitArray(30);
        private readonly Color[] ColorCache = new Color[30];
        private readonly BitArray ColorCacheValid = new BitArray(30);
        private readonly SafeThemeHandle theme;

        private ThemeResourceKey _cacheActiveBorderBrush;
        private ThemeResourceKey _cacheActiveBorderColor;
        private ThemeResourceKey _cacheActiveCaptionBrush;
        private ThemeResourceKey _cacheActiveCaptionColor;
        private ThemeResourceKey _cacheActiveCaptionTextBrush;
        private ThemeResourceKey _cacheActiveCaptionTextColor;
        private ThemeResourceKey _cacheAppWorkspaceBrush;
        private ThemeResourceKey _cacheAppWorkspaceColor;
        private ThemeResourceKey _cacheControlBrush;
        private ThemeResourceKey _cacheControlColor;
        private ThemeResourceKey _cacheControlDarkBrush;
        private ThemeResourceKey _cacheControlDarkColor;
        private ThemeResourceKey _cacheControlDarkDarkBrush;
        private ThemeResourceKey _cacheControlDarkDarkColor;
        private ThemeResourceKey _cacheControlLightBrush;
        private ThemeResourceKey _cacheControlLightColor;
        private ThemeResourceKey _cacheControlLightLightBrush;
        private ThemeResourceKey _cacheControlLightLightColor;
        private ThemeResourceKey _cacheControlTextBrush;
        private ThemeResourceKey _cacheControlTextColor;
        private ThemeResourceKey _cacheDesktopBrush;
        private ThemeResourceKey _cacheDesktopColor;
        private ThemeResourceKey _cacheGradientActiveCaptionBrush;
        private ThemeResourceKey _cacheGradientActiveCaptionColor;
        private ThemeResourceKey _cacheGradientInactiveCaptionBrush;
        private ThemeResourceKey _cacheGradientInactiveCaptionColor;
        private ThemeResourceKey _cacheGrayTextBrush;
        private ThemeResourceKey _cacheGrayTextColor;
        private ThemeResourceKey _cacheHighlightBrush;
        private ThemeResourceKey _cacheHighlightColor;
        private ThemeResourceKey _cacheHighlightTextBrush;
        private ThemeResourceKey _cacheHighlightTextColor;
        private ThemeResourceKey _cacheHotTrackBrush;
        private ThemeResourceKey _cacheHotTrackColor;
        private ThemeResourceKey _cacheInactiveBorderBrush;
        private ThemeResourceKey _cacheInactiveBorderColor;
        private ThemeResourceKey _cacheInactiveCaptionBrush;
        private ThemeResourceKey _cacheInactiveCaptionColor;
        private ThemeResourceKey _cacheInactiveCaptionTextBrush;
        private ThemeResourceKey _cacheInactiveCaptionTextColor;
        private ThemeResourceKey _cacheInactiveSelectionHighlightBrush;
        private ThemeResourceKey _cacheInactiveSelectionHighlightTextBrush;
        private ThemeResourceKey _cacheInfoBrush;
        private ThemeResourceKey _cacheInfoColor;
        private ThemeResourceKey _cacheInfoTextBrush;
        private ThemeResourceKey _cacheInfoTextColor;
        private ThemeResourceKey _cacheMenuBarBrush;
        private ThemeResourceKey _cacheMenuBarColor;
        private ThemeResourceKey _cacheMenuBrush;
        private ThemeResourceKey _cacheMenuColor;
        private ThemeResourceKey _cacheMenuHighlightBrush;
        private ThemeResourceKey _cacheMenuHighlightColor;
        private ThemeResourceKey _cacheMenuTextBrush;
        private ThemeResourceKey _cacheMenuTextColor;
        private ThemeResourceKey _cacheScrollBarBrush;
        private ThemeResourceKey _cacheScrollBarColor;
        private ThemeResourceKey _cacheWindowBrush;
        private ThemeResourceKey _cacheWindowColor;
        private ThemeResourceKey _cacheWindowFrameBrush;
        private ThemeResourceKey _cacheWindowFrameColor;
        private ThemeResourceKey _cacheWindowTextBrush;
        private ThemeResourceKey _cacheWindowTextColor;
        private const int AlphaShift = 24;
        private const int BlueShift = 0;
        private const int GreenShift = 8;
        private const int RedShift = 16;
        private const int Win32BlueShift = 16;
        private const int Win32GreenShift = 8;
        private const int Win32RedShift = 0;

        public ThemeColors(IntPtr hwnd, string classList)
        {
            theme = StyleNativeMethods.OpenThemeData(hwnd, classList);
        }

        public void Dispose()
        {
            theme.Dispose();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ThemeResourceKey CreateInstance(ThemeResourceKeyId keyId)
        {
            return new ThemeResourceKey(keyId);
        }

        private static int EncodeBgra(int alpha, int red, int green, int blue)
        {
            return (alpha << AlphaShift) | (red << RedShift) | (green << GreenShift) | (blue << BlueShift);
        }

        private static int FromWin32Value(int rgb)
        {
            return EncodeBgra(
                0xFF,
                (rgb >> Win32RedShift) & 0xFF,
                (rgb >> Win32GreenShift) & 0xFF,
                (rgb >> Win32BlueShift) & 0xFF);
        }

        private Color GetThemeColor(CacheSlot slot)
        {
            lock (ColorCacheValid) {
                if (!ColorCacheValid[(int)slot]) {
                    uint bgra = (uint)FromWin32Value(StyleNativeMethods.GetThemeSysColor(theme, SlotToFlag(slot)));
                    Color color = Color.FromArgb(
                        (byte)((bgra & 0xFF000000) >> 24),
                        (byte)((bgra & 0xFF0000) >> 16),
                        (byte)((bgra & 0xFF00) >> 8),
                        (byte)(bgra & 0xFF));
                    ColorCache[(int)slot] = color;
                    ColorCacheValid[(int)slot] = true;
                    return color;
                }
                return ColorCache[(int)slot];
            }
        }

        internal bool InvalidateCache()
        {
            bool colorsInvalidated = ClearBitArray(ColorCacheValid);
            bool brushesInvalidated = ClearBitArray(BrushCacheValid);
            return colorsInvalidated || brushesInvalidated;
        }

        internal static bool ClearBitArray(BitArray cacheValid)
        {
            bool invalidated = false;
            for (int i = 0; i < cacheValid.Count; ++i) {
                if (ClearSlot(cacheValid, i))
                    invalidated = true;
            }
            return invalidated;
        }

        internal static bool ClearSlot(BitArray cacheValid, int slot)
        {
            if (cacheValid[slot]) {
                cacheValid[slot] = false;
                return true;
            }
            return false;
        }

        private SolidColorBrush MakeBrush(CacheSlot slot)
        {
            lock (BrushCacheValid) {
                if (!BrushCacheValid[(int)slot]) {
                    var brush = new SolidColorBrush(GetThemeColor(slot));
                    brush.Freeze();
                    BrushCache[(int)slot] = brush;
                    BrushCacheValid[(int)slot] = true;
                    return brush;
                }
                return BrushCache[(int)slot];
            }
        }

        private static int SlotToFlag(CacheSlot slot)
        {
            switch (slot) {
                case CacheSlot.ActiveBorder:
                    return 10;

                case CacheSlot.ActiveCaption:
                    return 2;

                case CacheSlot.ActiveCaptionText:
                    return 9;

                case CacheSlot.AppWorkspace:
                    return 12;

                case CacheSlot.Control:
                    return 15;

                case CacheSlot.ControlDark:
                    return 0x10;

                case CacheSlot.ControlDarkDark:
                    return 0x15;

                case CacheSlot.ControlLight:
                    return 0x16;

                case CacheSlot.ControlLightLight:
                    return 20;

                case CacheSlot.ControlText:
                    return 0x12;

                case CacheSlot.Desktop:
                    return 1;

                case CacheSlot.GradientActiveCaption:
                    return 0x1b;

                case CacheSlot.GradientInactiveCaption:
                    return 0x1c;

                case CacheSlot.GrayText:
                    return 0x11;

                case CacheSlot.Highlight:
                    return 13;

                case CacheSlot.HighlightText:
                    return 14;

                case CacheSlot.HotTrack:
                    return 0x1a;

                case CacheSlot.InactiveBorder:
                    return 11;

                case CacheSlot.InactiveCaption:
                    return 3;

                case CacheSlot.InactiveCaptionText:
                    return 0x13;

                case CacheSlot.Info:
                    return 0x18;

                case CacheSlot.InfoText:
                    return 0x17;

                case CacheSlot.Menu:
                    return 4;

                case CacheSlot.MenuBar:
                    return 30;

                case CacheSlot.MenuHighlight:
                    return 0x1d;

                case CacheSlot.MenuText:
                    return 7;

                case CacheSlot.ScrollBar:
                    return 0;

                case CacheSlot.Window:
                    return 5;

                case CacheSlot.WindowFrame:
                    return 6;

                case CacheSlot.WindowText:
                    return 8;
            }
            return 0;
        }

        public SolidColorBrush ActiveBorderBrush => MakeBrush(CacheSlot.ActiveBorder);

        public ResourceKey ActiveBorderBrushKey
        {
            get
            {
                if (_cacheActiveBorderBrush == null) {
                    _cacheActiveBorderBrush = CreateInstance(ThemeResourceKeyId.ActiveBorderBrush);
                }
                return _cacheActiveBorderBrush;
            }
        }

        public Color ActiveBorderColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ActiveBorder);
            }
        }

        public ResourceKey ActiveBorderColorKey
        {
            get
            {
                if (_cacheActiveBorderColor == null) {
                    _cacheActiveBorderColor = CreateInstance(ThemeResourceKeyId.ActiveBorderColor);
                }
                return _cacheActiveBorderColor;
            }
        }

        public SolidColorBrush ActiveCaptionBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ActiveCaption);
            }
        }

        public ResourceKey ActiveCaptionBrushKey
        {
            get
            {
                if (_cacheActiveCaptionBrush == null) {
                    _cacheActiveCaptionBrush = CreateInstance(ThemeResourceKeyId.ActiveCaptionBrush);
                }
                return _cacheActiveCaptionBrush;
            }
        }

        public Color ActiveCaptionColor => GetThemeColor(CacheSlot.ActiveCaption);

        public ResourceKey ActiveCaptionColorKey
        {
            get
            {
                if (_cacheActiveCaptionColor == null) {
                    _cacheActiveCaptionColor = CreateInstance(ThemeResourceKeyId.ActiveCaptionColor);
                }
                return _cacheActiveCaptionColor;
            }
        }

        public SolidColorBrush ActiveCaptionTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ActiveCaptionText);
            }
        }

        public ResourceKey ActiveCaptionTextBrushKey
        {
            get
            {
                if (_cacheActiveCaptionTextBrush == null) {
                    _cacheActiveCaptionTextBrush = CreateInstance(ThemeResourceKeyId.ActiveCaptionTextBrush);
                }
                return _cacheActiveCaptionTextBrush;
            }
        }

        public Color ActiveCaptionTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ActiveCaptionText);
            }
        }

        public ResourceKey ActiveCaptionTextColorKey
        {
            get
            {
                if (_cacheActiveCaptionTextColor == null) {
                    _cacheActiveCaptionTextColor = CreateInstance(ThemeResourceKeyId.ActiveCaptionTextColor);
                }
                return _cacheActiveCaptionTextColor;
            }
        }

        public SolidColorBrush AppWorkspaceBrush
        {
            get
            {
                return MakeBrush(CacheSlot.AppWorkspace);
            }
        }

        public ResourceKey AppWorkspaceBrushKey
        {
            get
            {
                if (_cacheAppWorkspaceBrush == null) {
                    _cacheAppWorkspaceBrush = CreateInstance(ThemeResourceKeyId.AppWorkspaceBrush);
                }
                return _cacheAppWorkspaceBrush;
            }
        }

        public Color AppWorkspaceColor
        {
            get
            {
                return GetThemeColor(CacheSlot.AppWorkspace);
            }
        }

        public ResourceKey AppWorkspaceColorKey
        {
            get
            {
                if (_cacheAppWorkspaceColor == null) {
                    _cacheAppWorkspaceColor = CreateInstance(ThemeResourceKeyId.AppWorkspaceColor);
                }
                return _cacheAppWorkspaceColor;
            }
        }

        public SolidColorBrush ControlBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Control);
            }
        }

        public ResourceKey ControlBrushKey
        {
            get
            {
                if (_cacheControlBrush == null) {
                    _cacheControlBrush = CreateInstance(ThemeResourceKeyId.ControlBrush);
                }
                return _cacheControlBrush;
            }
        }

        public Color ControlColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Control);
            }
        }

        public ResourceKey ControlColorKey
        {
            get
            {
                if (_cacheControlColor == null) {
                    _cacheControlColor = CreateInstance(ThemeResourceKeyId.ControlColor);
                }
                return _cacheControlColor;
            }
        }

        public SolidColorBrush ControlDarkBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ControlDark);
            }
        }

        public ResourceKey ControlDarkBrushKey
        {
            get
            {
                if (_cacheControlDarkBrush == null) {
                    _cacheControlDarkBrush = CreateInstance(ThemeResourceKeyId.ControlDarkBrush);
                }
                return _cacheControlDarkBrush;
            }
        }

        public Color ControlDarkColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ControlDark);
            }
        }

        public ResourceKey ControlDarkColorKey
        {
            get
            {
                if (_cacheControlDarkColor == null) {
                    _cacheControlDarkColor = CreateInstance(ThemeResourceKeyId.ControlDarkColor);
                }
                return _cacheControlDarkColor;
            }
        }

        public SolidColorBrush ControlDarkDarkBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ControlDarkDark);
            }
        }

        public ResourceKey ControlDarkDarkBrushKey
        {
            get
            {
                if (_cacheControlDarkDarkBrush == null) {
                    _cacheControlDarkDarkBrush = CreateInstance(ThemeResourceKeyId.ControlDarkDarkBrush);
                }
                return _cacheControlDarkDarkBrush;
            }
        }

        public Color ControlDarkDarkColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ControlDarkDark);
            }
        }

        public ResourceKey ControlDarkDarkColorKey
        {
            get
            {
                if (_cacheControlDarkDarkColor == null) {
                    _cacheControlDarkDarkColor = CreateInstance(ThemeResourceKeyId.ControlDarkDarkColor);
                }
                return _cacheControlDarkDarkColor;
            }
        }

        public SolidColorBrush ControlLightBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ControlLight);
            }
        }

        public ResourceKey ControlLightBrushKey
        {
            get
            {
                if (_cacheControlLightBrush == null) {
                    _cacheControlLightBrush = CreateInstance(ThemeResourceKeyId.ControlLightBrush);
                }
                return _cacheControlLightBrush;
            }
        }

        public Color ControlLightColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ControlLight);
            }
        }

        public ResourceKey ControlLightColorKey
        {
            get
            {
                if (_cacheControlLightColor == null) {
                    _cacheControlLightColor = CreateInstance(ThemeResourceKeyId.ControlLightColor);
                }
                return _cacheControlLightColor;
            }
        }

        public SolidColorBrush ControlLightLightBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ControlLightLight);
            }
        }

        public ResourceKey ControlLightLightBrushKey
        {
            get
            {
                if (_cacheControlLightLightBrush == null) {
                    _cacheControlLightLightBrush = CreateInstance(ThemeResourceKeyId.ControlLightLightBrush);
                }
                return _cacheControlLightLightBrush;
            }
        }

        public Color ControlLightLightColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ControlLightLight);
            }
        }

        public ResourceKey ControlLightLightColorKey
        {
            get
            {
                if (_cacheControlLightLightColor == null) {
                    _cacheControlLightLightColor = CreateInstance(ThemeResourceKeyId.ControlLightLightColor);
                }
                return _cacheControlLightLightColor;
            }
        }

        public SolidColorBrush ControlTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ControlText);
            }
        }

        public ResourceKey ControlTextBrushKey
        {
            get
            {
                if (_cacheControlTextBrush == null) {
                    _cacheControlTextBrush = CreateInstance(ThemeResourceKeyId.ControlTextBrush);
                }
                return _cacheControlTextBrush;
            }
        }

        public Color ControlTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ControlText);
            }
        }

        public ResourceKey ControlTextColorKey
        {
            get
            {
                if (_cacheControlTextColor == null) {
                    _cacheControlTextColor = CreateInstance(ThemeResourceKeyId.ControlTextColor);
                }
                return _cacheControlTextColor;
            }
        }

        public SolidColorBrush DesktopBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Desktop);
            }
        }

        public ResourceKey DesktopBrushKey
        {
            get
            {
                if (_cacheDesktopBrush == null) {
                    _cacheDesktopBrush = CreateInstance(ThemeResourceKeyId.DesktopBrush);
                }
                return _cacheDesktopBrush;
            }
        }

        public Color DesktopColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Desktop);
            }
        }

        public ResourceKey DesktopColorKey
        {
            get
            {
                if (_cacheDesktopColor == null) {
                    _cacheDesktopColor = CreateInstance(ThemeResourceKeyId.DesktopColor);
                }
                return _cacheDesktopColor;
            }
        }

        public SolidColorBrush GradientActiveCaptionBrush
        {
            get
            {
                return MakeBrush(CacheSlot.GradientActiveCaption);
            }
        }

        public ResourceKey GradientActiveCaptionBrushKey
        {
            get
            {
                if (_cacheGradientActiveCaptionBrush == null) {
                    _cacheGradientActiveCaptionBrush = CreateInstance(ThemeResourceKeyId.GradientActiveCaptionBrush);
                }
                return _cacheGradientActiveCaptionBrush;
            }
        }

        public Color GradientActiveCaptionColor
        {
            get
            {
                return GetThemeColor(CacheSlot.GradientActiveCaption);
            }
        }

        public ResourceKey GradientActiveCaptionColorKey
        {
            get
            {
                if (_cacheGradientActiveCaptionColor == null) {
                    _cacheGradientActiveCaptionColor = CreateInstance(ThemeResourceKeyId.GradientActiveCaptionColor);
                }
                return _cacheGradientActiveCaptionColor;
            }
        }

        public SolidColorBrush GradientInactiveCaptionBrush
        {
            get
            {
                return MakeBrush(CacheSlot.GradientInactiveCaption);
            }
        }

        public ResourceKey GradientInactiveCaptionBrushKey
        {
            get
            {
                if (_cacheGradientInactiveCaptionBrush == null) {
                    _cacheGradientInactiveCaptionBrush = CreateInstance(ThemeResourceKeyId.GradientInactiveCaptionBrush);
                }
                return _cacheGradientInactiveCaptionBrush;
            }
        }

        public Color GradientInactiveCaptionColor
        {
            get
            {
                return GetThemeColor(CacheSlot.GradientInactiveCaption);
            }
        }

        public ResourceKey GradientInactiveCaptionColorKey
        {
            get
            {
                if (_cacheGradientInactiveCaptionColor == null) {
                    _cacheGradientInactiveCaptionColor = CreateInstance(ThemeResourceKeyId.GradientInactiveCaptionColor);
                }
                return _cacheGradientInactiveCaptionColor;
            }
        }

        public SolidColorBrush GrayTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.GrayText);
            }
        }

        public ResourceKey GrayTextBrushKey
        {
            get
            {
                if (_cacheGrayTextBrush == null) {
                    _cacheGrayTextBrush = CreateInstance(ThemeResourceKeyId.GrayTextBrush);
                }
                return _cacheGrayTextBrush;
            }
        }

        public Color GrayTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.GrayText);
            }
        }

        public ResourceKey GrayTextColorKey
        {
            get
            {
                if (_cacheGrayTextColor == null) {
                    _cacheGrayTextColor = CreateInstance(ThemeResourceKeyId.GrayTextColor);
                }
                return _cacheGrayTextColor;
            }
        }

        public SolidColorBrush HighlightBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Highlight);
            }
        }

        public ResourceKey HighlightBrushKey
        {
            get
            {
                if (_cacheHighlightBrush == null) {
                    _cacheHighlightBrush = CreateInstance(ThemeResourceKeyId.HighlightBrush);
                }
                return _cacheHighlightBrush;
            }
        }

        public Color HighlightColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Highlight);
            }
        }

        public ResourceKey HighlightColorKey
        {
            get
            {
                if (_cacheHighlightColor == null) {
                    _cacheHighlightColor = CreateInstance(ThemeResourceKeyId.HighlightColor);
                }
                return _cacheHighlightColor;
            }
        }

        public SolidColorBrush HighlightTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.HighlightText);
            }
        }

        public ResourceKey HighlightTextBrushKey
        {
            get
            {
                if (_cacheHighlightTextBrush == null) {
                    _cacheHighlightTextBrush = CreateInstance(ThemeResourceKeyId.HighlightTextBrush);
                }
                return _cacheHighlightTextBrush;
            }
        }

        public Color HighlightTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.HighlightText);
            }
        }

        public ResourceKey HighlightTextColorKey
        {
            get
            {
                if (_cacheHighlightTextColor == null) {
                    _cacheHighlightTextColor = CreateInstance(ThemeResourceKeyId.HighlightTextColor);
                }
                return _cacheHighlightTextColor;
            }
        }

        public SolidColorBrush HotTrackBrush
        {
            get
            {
                return MakeBrush(CacheSlot.HotTrack);
            }
        }

        public ResourceKey HotTrackBrushKey
        {
            get
            {
                if (_cacheHotTrackBrush == null) {
                    _cacheHotTrackBrush = CreateInstance(ThemeResourceKeyId.HotTrackBrush);
                }
                return _cacheHotTrackBrush;
            }
        }

        public Color HotTrackColor
        {
            get
            {
                return GetThemeColor(CacheSlot.HotTrack);
            }
        }

        public ResourceKey HotTrackColorKey
        {
            get
            {
                if (_cacheHotTrackColor == null) {
                    _cacheHotTrackColor = CreateInstance(ThemeResourceKeyId.HotTrackColor);
                }
                return _cacheHotTrackColor;
            }
        }

        public SolidColorBrush InactiveBorderBrush
        {
            get
            {
                return MakeBrush(CacheSlot.InactiveBorder);
            }
        }

        public ResourceKey InactiveBorderBrushKey
        {
            get
            {
                if (_cacheInactiveBorderBrush == null) {
                    _cacheInactiveBorderBrush = CreateInstance(ThemeResourceKeyId.InactiveBorderBrush);
                }
                return _cacheInactiveBorderBrush;
            }
        }

        public Color InactiveBorderColor
        {
            get
            {
                return GetThemeColor(CacheSlot.InactiveBorder);
            }
        }

        public ResourceKey InactiveBorderColorKey
        {
            get
            {
                if (_cacheInactiveBorderColor == null) {
                    _cacheInactiveBorderColor = CreateInstance(ThemeResourceKeyId.InactiveBorderColor);
                }
                return _cacheInactiveBorderColor;
            }
        }

        public SolidColorBrush InactiveCaptionBrush
        {
            get
            {
                return MakeBrush(CacheSlot.InactiveCaption);
            }
        }

        public ResourceKey InactiveCaptionBrushKey
        {
            get
            {
                if (_cacheInactiveCaptionBrush == null) {
                    _cacheInactiveCaptionBrush = CreateInstance(ThemeResourceKeyId.InactiveCaptionBrush);
                }
                return _cacheInactiveCaptionBrush;
            }
        }

        public Color InactiveCaptionColor
        {
            get
            {
                return GetThemeColor(CacheSlot.InactiveCaption);
            }
        }

        public ResourceKey InactiveCaptionColorKey
        {
            get
            {
                if (_cacheInactiveCaptionColor == null) {
                    _cacheInactiveCaptionColor = CreateInstance(ThemeResourceKeyId.InactiveCaptionColor);
                }
                return _cacheInactiveCaptionColor;
            }
        }

        public SolidColorBrush InactiveCaptionTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.InactiveCaptionText);
            }
        }

        public ResourceKey InactiveCaptionTextBrushKey
        {
            get
            {
                if (_cacheInactiveCaptionTextBrush == null) {
                    _cacheInactiveCaptionTextBrush = CreateInstance(ThemeResourceKeyId.InactiveCaptionTextBrush);
                }
                return _cacheInactiveCaptionTextBrush;
            }
        }

        public Color InactiveCaptionTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.InactiveCaptionText);
            }
        }

        public ResourceKey InactiveCaptionTextColorKey
        {
            get
            {
                if (_cacheInactiveCaptionTextColor == null) {
                    _cacheInactiveCaptionTextColor = CreateInstance(ThemeResourceKeyId.InactiveCaptionTextColor);
                }
                return _cacheInactiveCaptionTextColor;
            }
        }

        public SolidColorBrush InactiveSelectionHighlightBrush
        {
            get
            {
                if (SystemParameters.HighContrast) {
                    return HighlightBrush;
                }
                return ControlBrush;
            }
        }

        public ResourceKey InactiveSelectionHighlightBrushKey
        {
            get
            {
                //if (!FrameworkCompatibilityPreferences.GetAreInactiveSelectionHighlightBrushKeysSupported())
                //{
                //    return ControlBrushKey;
                //}
                if (_cacheInactiveSelectionHighlightBrush == null) {
                    _cacheInactiveSelectionHighlightBrush = CreateInstance(ThemeResourceKeyId.InactiveSelectionHighlightBrush);
                }
                return _cacheInactiveSelectionHighlightBrush;
            }
        }

        public SolidColorBrush InactiveSelectionHighlightTextBrush
        {
            get
            {
                if (SystemParameters.HighContrast) {
                    return HighlightTextBrush;
                }
                return ControlTextBrush;
            }
        }

        public ResourceKey InactiveSelectionHighlightTextBrushKey
        {
            get
            {
                //if (!FrameworkCompatibilityPreferences.GetAreInactiveSelectionHighlightBrushKeysSupported())
                //{
                //    return ControlTextBrushKey;
                //}
                if (_cacheInactiveSelectionHighlightTextBrush == null) {
                    _cacheInactiveSelectionHighlightTextBrush = CreateInstance(ThemeResourceKeyId.InactiveSelectionHighlightTextBrush);
                }
                return _cacheInactiveSelectionHighlightTextBrush;
            }
        }

        public SolidColorBrush InfoBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Info);
            }
        }

        public ResourceKey InfoBrushKey
        {
            get
            {
                if (_cacheInfoBrush == null) {
                    _cacheInfoBrush = CreateInstance(ThemeResourceKeyId.InfoBrush);
                }
                return _cacheInfoBrush;
            }
        }

        public Color InfoColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Info);
            }
        }

        public ResourceKey InfoColorKey
        {
            get
            {
                if (_cacheInfoColor == null) {
                    _cacheInfoColor = CreateInstance(ThemeResourceKeyId.InfoColor);
                }
                return _cacheInfoColor;
            }
        }

        public SolidColorBrush InfoTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.InfoText);
            }
        }

        public ResourceKey InfoTextBrushKey
        {
            get
            {
                if (_cacheInfoTextBrush == null) {
                    _cacheInfoTextBrush = CreateInstance(ThemeResourceKeyId.InfoTextBrush);
                }
                return _cacheInfoTextBrush;
            }
        }

        public Color InfoTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.InfoText);
            }
        }

        public ResourceKey InfoTextColorKey
        {
            get
            {
                if (_cacheInfoTextColor == null) {
                    _cacheInfoTextColor = CreateInstance(ThemeResourceKeyId.InfoTextColor);
                }
                return _cacheInfoTextColor;
            }
        }

        public SolidColorBrush MenuBarBrush
        {
            get
            {
                return MakeBrush(CacheSlot.MenuBar);
            }
        }

        public ResourceKey MenuBarBrushKey
        {
            get
            {
                if (_cacheMenuBarBrush == null) {
                    _cacheMenuBarBrush = CreateInstance(ThemeResourceKeyId.MenuBarBrush);
                }
                return _cacheMenuBarBrush;
            }
        }

        public Color MenuBarColor
        {
            get
            {
                return GetThemeColor(CacheSlot.MenuBar);
            }
        }

        public ResourceKey MenuBarColorKey
        {
            get
            {
                if (_cacheMenuBarColor == null) {
                    _cacheMenuBarColor = CreateInstance(ThemeResourceKeyId.MenuBarColor);
                }
                return _cacheMenuBarColor;
            }
        }

        public SolidColorBrush MenuBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Menu);
            }
        }

        public ResourceKey MenuBrushKey
        {
            get
            {
                if (_cacheMenuBrush == null) {
                    _cacheMenuBrush = CreateInstance(ThemeResourceKeyId.MenuBrush);
                }
                return _cacheMenuBrush;
            }
        }

        public Color MenuColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Menu);
            }
        }

        public ResourceKey MenuColorKey
        {
            get
            {
                if (_cacheMenuColor == null) {
                    _cacheMenuColor = CreateInstance(ThemeResourceKeyId.MenuColor);
                }
                return _cacheMenuColor;
            }
        }

        public SolidColorBrush MenuHighlightBrush
        {
            get
            {
                return MakeBrush(CacheSlot.MenuHighlight);
            }
        }

        public ResourceKey MenuHighlightBrushKey
        {
            get
            {
                if (_cacheMenuHighlightBrush == null) {
                    _cacheMenuHighlightBrush = CreateInstance(ThemeResourceKeyId.MenuHighlightBrush);
                }
                return _cacheMenuHighlightBrush;
            }
        }

        public Color MenuHighlightColor
        {
            get
            {
                return GetThemeColor(CacheSlot.MenuHighlight);
            }
        }

        public ResourceKey MenuHighlightColorKey
        {
            get
            {
                if (_cacheMenuHighlightColor == null) {
                    _cacheMenuHighlightColor = CreateInstance(ThemeResourceKeyId.MenuHighlightColor);
                }
                return _cacheMenuHighlightColor;
            }
        }

        public SolidColorBrush MenuTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.MenuText);
            }
        }

        public ResourceKey MenuTextBrushKey
        {
            get
            {
                if (_cacheMenuTextBrush == null) {
                    _cacheMenuTextBrush = CreateInstance(ThemeResourceKeyId.MenuTextBrush);
                }
                return _cacheMenuTextBrush;
            }
        }

        public Color MenuTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.MenuText);
            }
        }

        public ResourceKey MenuTextColorKey
        {
            get
            {
                if (_cacheMenuTextColor == null) {
                    _cacheMenuTextColor = CreateInstance(ThemeResourceKeyId.MenuTextColor);
                }
                return _cacheMenuTextColor;
            }
        }

        public SolidColorBrush ScrollBarBrush
        {
            get
            {
                return MakeBrush(CacheSlot.ScrollBar);
            }
        }

        public ResourceKey ScrollBarBrushKey
        {
            get
            {
                if (_cacheScrollBarBrush == null) {
                    _cacheScrollBarBrush = CreateInstance(ThemeResourceKeyId.ScrollBarBrush);
                }
                return _cacheScrollBarBrush;
            }
        }

        public Color ScrollBarColor
        {
            get
            {
                return GetThemeColor(CacheSlot.ScrollBar);
            }
        }

        public ResourceKey ScrollBarColorKey
        {
            get
            {
                if (_cacheScrollBarColor == null) {
                    _cacheScrollBarColor = CreateInstance(ThemeResourceKeyId.ScrollBarColor);
                }
                return _cacheScrollBarColor;
            }
        }

        public SolidColorBrush WindowBrush
        {
            get
            {
                return MakeBrush(CacheSlot.Window);
            }
        }

        public ResourceKey WindowBrushKey
        {
            get
            {
                if (_cacheWindowBrush == null) {
                    _cacheWindowBrush = CreateInstance(ThemeResourceKeyId.WindowBrush);
                }
                return _cacheWindowBrush;
            }
        }

        public Color WindowColor
        {
            get
            {
                return GetThemeColor(CacheSlot.Window);
            }
        }

        public ResourceKey WindowColorKey
        {
            get
            {
                if (_cacheWindowColor == null) {
                    _cacheWindowColor = CreateInstance(ThemeResourceKeyId.WindowColor);
                }
                return _cacheWindowColor;
            }
        }

        public SolidColorBrush WindowFrameBrush
        {
            get
            {
                return MakeBrush(CacheSlot.WindowFrame);
            }
        }

        public ResourceKey WindowFrameBrushKey
        {
            get
            {
                if (_cacheWindowFrameBrush == null) {
                    _cacheWindowFrameBrush = CreateInstance(ThemeResourceKeyId.WindowFrameBrush);
                }
                return _cacheWindowFrameBrush;
            }
        }

        public Color WindowFrameColor
        {
            get
            {
                return GetThemeColor(CacheSlot.WindowFrame);
            }
        }

        public ResourceKey WindowFrameColorKey
        {
            get
            {
                if (_cacheWindowFrameColor == null) {
                    _cacheWindowFrameColor = CreateInstance(ThemeResourceKeyId.WindowFrameColor);
                }
                return _cacheWindowFrameColor;
            }
        }

        public SolidColorBrush WindowTextBrush
        {
            get
            {
                return MakeBrush(CacheSlot.WindowText);
            }
        }

        public ResourceKey WindowTextBrushKey
        {
            get
            {
                if (_cacheWindowTextBrush == null) {
                    _cacheWindowTextBrush = CreateInstance(ThemeResourceKeyId.WindowTextBrush);
                }
                return _cacheWindowTextBrush;
            }
        }

        public Color WindowTextColor
        {
            get
            {
                return GetThemeColor(CacheSlot.WindowText);
            }
        }

        public ResourceKey WindowTextColorKey
        {
            get
            {
                if (_cacheWindowTextColor == null) {
                    _cacheWindowTextColor = CreateInstance(ThemeResourceKeyId.WindowTextColor);
                }
                return _cacheWindowTextColor;
            }
        }

        // Nested Types
        private enum CacheSlot
        {
            ActiveBorder,
            ActiveCaption,
            ActiveCaptionText,
            AppWorkspace,
            Control,
            ControlDark,
            ControlDarkDark,
            ControlLight,
            ControlLightLight,
            ControlText,
            Desktop,
            GradientActiveCaption,
            GradientInactiveCaption,
            GrayText,
            Highlight,
            HighlightText,
            HotTrack,
            InactiveBorder,
            InactiveCaption,
            InactiveCaptionText,
            Info,
            InfoText,
            Menu,
            MenuBar,
            MenuHighlight,
            MenuText,
            ScrollBar,
            Window,
            WindowFrame,
            WindowText,
            NumSlots
        }
    }

    internal enum ThemeResourceKeyId
    {
        InternalSystemColorsStart,
        ActiveBorderBrush,
        ActiveCaptionBrush,
        ActiveCaptionTextBrush,
        AppWorkspaceBrush,
        ControlBrush,
        ControlDarkBrush,
        ControlDarkDarkBrush,
        ControlLightBrush,
        ControlLightLightBrush,
        ControlTextBrush,
        DesktopBrush,
        GradientActiveCaptionBrush,
        GradientInactiveCaptionBrush,
        GrayTextBrush,
        HighlightBrush,
        HighlightTextBrush,
        HotTrackBrush,
        InactiveBorderBrush,
        InactiveCaptionBrush,
        InactiveCaptionTextBrush,
        InfoBrush,
        InfoTextBrush,
        MenuBrush,
        MenuBarBrush,
        MenuHighlightBrush,
        MenuTextBrush,
        ScrollBarBrush,
        WindowBrush,
        WindowFrameBrush,
        WindowTextBrush,
        ActiveBorderColor,
        ActiveCaptionColor,
        ActiveCaptionTextColor,
        AppWorkspaceColor,
        ControlColor,
        ControlDarkColor,
        ControlDarkDarkColor,
        ControlLightColor,
        ControlLightLightColor,
        ControlTextColor,
        DesktopColor,
        GradientActiveCaptionColor,
        GradientInactiveCaptionColor,
        GrayTextColor,
        HighlightColor,
        HighlightTextColor,
        HotTrackColor,
        InactiveBorderColor,
        InactiveCaptionColor,
        InactiveCaptionTextColor,
        InfoColor,
        InfoTextColor,
        MenuColor,
        MenuBarColor,
        MenuHighlightColor,
        MenuTextColor,
        ScrollBarColor,
        WindowColor,
        WindowFrameColor,
        WindowTextColor,
        InternalSystemColorsEnd,
        InternalSystemFontsStart,
        CaptionFontSize,
        CaptionFontFamily,
        CaptionFontStyle,
        CaptionFontWeight,
        CaptionFontTextDecorations,
        SmallCaptionFontSize,
        SmallCaptionFontFamily,
        SmallCaptionFontStyle,
        SmallCaptionFontWeight,
        SmallCaptionFontTextDecorations,
        MenuFontSize,
        MenuFontFamily,
        MenuFontStyle,
        MenuFontWeight,
        MenuFontTextDecorations,
        StatusFontSize,
        StatusFontFamily,
        StatusFontStyle,
        StatusFontWeight,
        StatusFontTextDecorations,
        MessageFontSize,
        MessageFontFamily,
        MessageFontStyle,
        MessageFontWeight,
        MessageFontTextDecorations,
        IconFontSize,
        IconFontFamily,
        IconFontStyle,
        IconFontWeight,
        IconFontTextDecorations,
        InternalSystemFontsEnd,
        InternalSystemParametersStart,
        ThinHorizontalBorderHeight,
        ThinVerticalBorderWidth,
        CursorWidth,
        CursorHeight,
        ThickHorizontalBorderHeight,
        ThickVerticalBorderWidth,
        FixedFrameHorizontalBorderHeight,
        FixedFrameVerticalBorderWidth,
        FocusHorizontalBorderHeight,
        FocusVerticalBorderWidth,
        FullPrimaryScreenWidth,
        FullPrimaryScreenHeight,
        HorizontalScrollBarButtonWidth,
        HorizontalScrollBarHeight,
        HorizontalScrollBarThumbWidth,
        IconWidth,
        IconHeight,
        IconGridWidth,
        IconGridHeight,
        MaximizedPrimaryScreenWidth,
        MaximizedPrimaryScreenHeight,
        MaximumWindowTrackWidth,
        MaximumWindowTrackHeight,
        MenuCheckmarkWidth,
        MenuCheckmarkHeight,
        MenuButtonWidth,
        MenuButtonHeight,
        MinimumWindowWidth,
        MinimumWindowHeight,
        MinimizedWindowWidth,
        MinimizedWindowHeight,
        MinimizedGridWidth,
        MinimizedGridHeight,
        MinimumWindowTrackWidth,
        MinimumWindowTrackHeight,
        PrimaryScreenWidth,
        PrimaryScreenHeight,
        WindowCaptionButtonWidth,
        WindowCaptionButtonHeight,
        ResizeFrameHorizontalBorderHeight,
        ResizeFrameVerticalBorderWidth,
        SmallIconWidth,
        SmallIconHeight,
        SmallWindowCaptionButtonWidth,
        SmallWindowCaptionButtonHeight,
        VirtualScreenWidth,
        VirtualScreenHeight,
        VerticalScrollBarWidth,
        VerticalScrollBarButtonHeight,
        WindowCaptionHeight,
        KanjiWindowHeight,
        MenuBarHeight,
        SmallCaptionHeight,
        VerticalScrollBarThumbHeight,
        IsImmEnabled,
        IsMediaCenter,
        IsMenuDropRightAligned,
        IsMiddleEastEnabled,
        IsMousePresent,
        IsMouseWheelPresent,
        IsPenWindows,
        IsRemotelyControlled,
        IsRemoteSession,
        ShowSounds,
        IsSlowMachine,
        SwapButtons,
        IsTabletPC,
        VirtualScreenLeft,
        VirtualScreenTop,
        FocusBorderWidth,
        FocusBorderHeight,
        HighContrast,
        DropShadow,
        FlatMenu,
        WorkArea,
        IconHorizontalSpacing,
        IconVerticalSpacing,
        IconTitleWrap,
        KeyboardCues,
        KeyboardDelay,
        KeyboardPreference,
        KeyboardSpeed,
        SnapToDefaultButton,
        WheelScrollLines,
        MouseHoverTime,
        MouseHoverHeight,
        MouseHoverWidth,
        MenuDropAlignment,
        MenuFade,
        MenuShowDelay,
        ComboBoxAnimation,
        ClientAreaAnimation,
        CursorShadow,
        GradientCaptions,
        HotTracking,
        ListBoxSmoothScrolling,
        MenuAnimation,
        SelectionFade,
        StylusHotTracking,
        ToolTipAnimation,
        ToolTipFade,
        UIEffects,
        MinimizeAnimation,
        Border,
        CaretWidth,
        ForegroundFlashCount,
        DragFullWindows,
        BorderWidth,
        ScrollWidth,
        ScrollHeight,
        CaptionWidth,
        CaptionHeight,
        SmallCaptionWidth,
        MenuWidth,
        MenuHeight,
        ComboBoxPopupAnimation,
        MenuPopupAnimation,
        ToolTipPopupAnimation,
        PowerLineStatus,
        InternalSystemThemeStylesStart,
        FocusVisualStyle,
        NavigationChromeDownLevelStyle,
        NavigationChromeStyle,
        InternalSystemParametersEnd,
        MenuItemSeparatorStyle,
        GridViewScrollViewerStyle,
        GridViewStyle,
        GridViewItemContainerStyle,
        StatusBarSeparatorStyle,
        ToolBarButtonStyle,
        ToolBarToggleButtonStyle,
        ToolBarSeparatorStyle,
        ToolBarCheckBoxStyle,
        ToolBarRadioButtonStyle,
        ToolBarComboBoxStyle,
        ToolBarTextBoxStyle,
        ToolBarMenuStyle,
        InternalSystemThemeStylesEnd,
        InternalSystemColorsExtendedStart,
        InactiveSelectionHighlightBrush,
        InactiveSelectionHighlightTextBrush,
        InternalSystemColorsExtendedEnd
    }

    internal sealed class ThemeResourceKey : ResourceKey
    {
        private readonly ThemeResourceKeyId id;

        public ThemeResourceKey(ThemeResourceKeyId id)
        {
            this.id = id;
        }

        public override Assembly Assembly
        {
            get { return null; }
        }

        public override bool Equals(object other)
        {
            var key = other as ThemeResourceKey;
            return key != null && key.id == id;
        }

        public override int GetHashCode()
        {
            return (int)id;
        }

        public override string ToString()
        {
            return id.ToString();
        }
    }
}
