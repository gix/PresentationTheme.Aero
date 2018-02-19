namespace PresentationTheme.Aero.Win7
{
    using System;

    /// <summary>Aero Windows 7 Theme</summary>
    public static class AeroWin7Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroWin7Theme).Assembly.GetName(),
                "Themes/Aero.Win7.NormalColor.xaml");
    }
}
