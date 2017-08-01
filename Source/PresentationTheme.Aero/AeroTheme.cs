namespace PresentationTheme.Aero
{
    using System;
    using Microsoft.Win32;

    /// <summary>Aero theme</summary>
    public static class AeroTheme
    {
        private static readonly AeroThemePolicy policy = new AeroThemePolicy();
        private static Lazy<Uri> resourceUriCache = new Lazy<Uri>(policy.GetCurrentThemeUri);

        static AeroTheme()
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for current theme resources. The
        ///   resource URI will change if the system theme changes.
        /// </summary>
        public static Uri ResourceUri => resourceUriCache.Value;

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

        /// <summary>
        ///   Sets the current theme to Aero using the default
        ///   <see cref="AeroThemePolicy"/>. The theme will update automatically
        ///   if the system theme changes.
        /// </summary>
        /// <seealso cref="AeroThemePolicy"/>
        public static void SetAsCurrentTheme()
        {
            ThemeManager.SetPresentationFrameworkTheme(policy);
        }

        /// <summary>
        ///   Removes the Aero theme, falling back to the default theme.
        /// </summary>
        public static bool RemoveAsCurrentTheme()
        {
            return ThemeManager.ClearPresentationFrameworkTheme();
        }

        private static void OnUserPreferenceChanged(
            object sender, UserPreferenceChangedEventArgs args)
        {
            if (args.Category == UserPreferenceCategory.VisualStyle)
                resourceUriCache = new Lazy<Uri>(policy.GetCurrentThemeUri);
        }
    }
}
