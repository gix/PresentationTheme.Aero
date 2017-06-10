namespace PresentationTheme.AeroLite.Win10
{
    using System;
    using Aero;

    /// <summary>AeroLite Windows 10 Theme</summary>
    public static class AeroLiteWin10Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroLiteWin10Theme).Assembly.GetName(),
                "Themes/AeroLite.Win10.NormalColor.xaml");

        /// <summary>
        ///   Gets or sets a value determining whether animations are forcibly
        ///   enabled or disabled.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to forcibly enable animations.
        ///   <see langword="false"/> to disable animations.
        ///   Use <see langword="null"/> to automatically determine whether
        ///   animations should be used.
        /// </value>
        /// <seealso cref="SystemVisualStateManager.UseAnimationsOverride"/>
        public static bool? UseAnimationsOverride
        {
            get => SystemVisualStateManager.Instance.UseAnimationsOverride;
            set => SystemVisualStateManager.Instance.UseAnimationsOverride = value;
        }
    }
}
