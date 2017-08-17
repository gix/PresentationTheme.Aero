namespace PresentationTheme.Aero
{
    using System;

    /// <summary>
    ///   Provides convenience helpers for the default <see cref="AeroThemePolicy"/>.
    ///   The theme will update automatically if the system theme changes.
    /// </summary>
    /// <remarks>
    ///   To use the theme, call <see cref="SetAsCurrentTheme"/> and revert it
    ///   using <see cref="RemoveAsCurrentTheme"/>.
    /// </remarks>
    /// <seealso cref="AeroThemePolicy"/>
    /// <seealso cref="SetAsCurrentTheme"/>
    public static class AeroTheme
    {
        private static readonly AeroThemePolicy Policy = new AeroThemePolicy();
        private static Lazy<Uri> resourceUriCache = new Lazy<Uri>(new AeroThemePolicy().GetCurrentThemeUri);

        static AeroTheme()
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        /// <summary>
        ///   Occurs when the <see cref="ResourceUri"/> may have changed after a
        ///   <see cref="ThemeManager.ThemeChanged"/> event.
        /// </summary>
        public static event EventHandler ResourceUriChanged;

        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for current theme resources. The
        ///   resource URI will change if the system theme changes.
        /// </summary>
        public static Uri ResourceUri => resourceUriCache.Value;

        /// <summary>
        ///   Sets the current theme to Aero using the default
        ///   <see cref="AeroThemePolicy"/>. The theme will update automatically
        ///   if the system theme changes.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <seealso cref="AeroThemePolicy"/>
        public static bool SetAsCurrentTheme()
        {
            return ThemeManager.SetPresentationFrameworkTheme(Policy);
        }

        /// <summary>
        ///   Removes the Aero theme, falling back to the default theme.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        public static bool RemoveAsCurrentTheme()
        {
            return ThemeManager.ClearPresentationFrameworkTheme();
        }

        private static void OnThemeChanged(object sender, EventArgs args)
        {
            resourceUriCache = new Lazy<Uri>(Policy.GetCurrentThemeUri);
            ResourceUriChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
