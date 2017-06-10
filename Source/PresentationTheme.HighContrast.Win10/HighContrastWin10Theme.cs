namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Reflection;
    using System.Text;
    using Aero;

    /// <summary>High Contrast Windows 10 Theme</summary>
    public static class HighContrastWin10Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            MakePackUri(typeof(HighContrastWin10Theme).Assembly, "Themes/HighContrast.Win10.NormalColor.xaml");

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
        ///   Sets the current theme to AeroLite.
        /// </summary>
        public static void SetCurrentTheme()
        {
            ThemeHelper.SetPresentationFrameworkTheme(ResourceUri);
        }

        private static Uri MakePackUri(Assembly assembly, string path)
        {
            var name = FormatName(assembly.GetName());
            return new Uri(
                $"pack://application:,,,/{name};component/{path}",
                UriKind.Absolute);
        }

        private static string FormatName(AssemblyName name)
        {
            return $"{name.Name};v{name.Version}{GetPublicKeySegment(name)}";
        }

        private static string GetPublicKeySegment(AssemblyName name)
        {
            var bytes = name.GetPublicKeyToken();
            if (bytes.Length == 0)
                return string.Empty;

            var builder = new StringBuilder(1 + bytes.Length * 2);
            builder.Append(';');
            foreach (var b in bytes)
                builder.AppendFormat("{0:x2}", b);

            return builder.ToString();
        }
    }
}
