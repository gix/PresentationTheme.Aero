namespace ThemeBrowser
{
    using StyleCore;
    using Color = System.Windows.Media.Color;

    public class HighContrastColor
    {
        public HighContrastColor(HIGHCONTRASTCOLOR index)
        {
            Index = index;
        }

        public HIGHCONTRASTCOLOR Index { get; }
        public Color Color => ThemeExtensions.ColorFromCOLORREF(MapEnumToSysColor(Index));

        private static int MapEnumToSysColor(HIGHCONTRASTCOLOR hcColor)
        {
            switch (hcColor) {
                case HIGHCONTRASTCOLOR.ACTIVECAPTION: return NativeMethods.GetSysColor(SysColor.COLOR_ACTIVECAPTION);
                case HIGHCONTRASTCOLOR.CAPTIONTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_CAPTIONTEXT);
                case HIGHCONTRASTCOLOR.BTNFACE: return NativeMethods.GetSysColor(SysColor.COLOR_BTNFACE);
                case HIGHCONTRASTCOLOR.BTNTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_BTNTEXT);
                case HIGHCONTRASTCOLOR.DESKTOP: return NativeMethods.GetSysColor(SysColor.COLOR_BACKGROUND);
                case HIGHCONTRASTCOLOR.GRAYTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_GRAYTEXT);
                case HIGHCONTRASTCOLOR.HOTLIGHT: return NativeMethods.GetSysColor(SysColor.COLOR_HOTLIGHT);
                case HIGHCONTRASTCOLOR.INACTIVECAPTION: return NativeMethods.GetSysColor(SysColor.COLOR_INACTIVECAPTION);
                case HIGHCONTRASTCOLOR.INACTIVECAPTIONTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_INACTIVECAPTIONTEXT);
                case HIGHCONTRASTCOLOR.HIGHLIGHT: return NativeMethods.GetSysColor(SysColor.COLOR_HIGHLIGHT);
                case HIGHCONTRASTCOLOR.HIGHLIGHTTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_HIGHLIGHTTEXT);
                case HIGHCONTRASTCOLOR.WINDOW: return NativeMethods.GetSysColor(SysColor.COLOR_WINDOW);
                case HIGHCONTRASTCOLOR.WINDOWTEXT: return NativeMethods.GetSysColor(SysColor.COLOR_WINDOWTEXT);
                default: return 0;
            }
        }

        public override string ToString() => $"{Index} ({Color})";
    }
}
