namespace PresentationTheme.Aero.Win10
{
    using System;

    /// <summary>Windows 10 Aero Theme</summary>
    public static class AeroWin10Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroWin10Theme).Assembly.GetName(),
                "Themes/Aero.Win10.NormalColor.xaml");
    }
}
