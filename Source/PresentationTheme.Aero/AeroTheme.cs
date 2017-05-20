namespace PresentationTheme.Aero
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>Aero theme</summary>
    public static class AeroTheme
    {
        private static Lazy<Uri> resourceUriCache = new Lazy<Uri>(BuildCurrentResourceUri);

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
        ///   Sets the current theme to Aero. The theme will update automatically
        ///   if the system theme changes.
        /// </summary>
        public static void SetAsCurrentTheme()
        {
            ThemeHelper.SetPresentationFrameworkTheme(BuildCurrentResourceUri);
        }

        /// <summary>
        ///   Removes the Aero theme, falling back to the default theme.
        /// </summary>
        public static bool RemoveAsCurrentTheme()
        {
            return ThemeHelper.ClearPresentationFrameworkTheme();
        }

        private static void OnUserPreferenceChanged(
            object sender, UserPreferenceChangedEventArgs args)
        {
            if (args.Category == UserPreferenceCategory.VisualStyle)
                resourceUriCache = new Lazy<Uri>(BuildCurrentResourceUri);
        }

        private static Uri BuildCurrentResourceUri()
        {
            var osVersion = GetRealWindowsVersion();
            string uxThemeName = SystemParameters.UxThemeName;
            string uxThemeColor = SystemParameters.UxThemeColor;
            bool highContrast = SystemParameters.HighContrast;

            var win7 = new Version(6, 1);
            var win8 = new Version(6, 2);
            var win10 = new Version(10, 0);

            string partialName = null;
            string resourceName = null;
            if (string.Equals(uxThemeName, "Aero", StringComparison.OrdinalIgnoreCase)) {
                if (osVersion >= win10) {
                    partialName = "PresentationTheme.Aero.Win10";
                    resourceName = "AeroWin10";
                } else if (osVersion >= win8) {
                    partialName = "PresentationTheme.Aero.Win8";
                    resourceName = "AeroWin8";
                } else if (osVersion >= win7) {
                    partialName = "PresentationTheme.Aero.Win7";
                    resourceName = "AeroWin7";
                }
            } else if (string.Equals(uxThemeName, "AeroLite", StringComparison.OrdinalIgnoreCase)) {
                if (highContrast) {
                    if (osVersion >= win10) {
                        partialName = "PresentationTheme.HighContrast.Win10";
                        resourceName = "HighContrastWin10";
                    }
                } else {
                    if (osVersion >= win10) {
                        partialName = "PresentationTheme.AeroLite.Win10";
                        resourceName = "AeroLiteWin10";
                    }
                }
            }

            if (partialName == null)
                return null;

            var asmName = GetFullAssemblyNameFromPartialName(typeof(AeroTheme).Assembly, partialName);
            string themedResourceName =
                "themes/" + resourceName.ToLowerInvariant() + "." + uxThemeColor.ToLowerInvariant();

            return MakePackUri(asmName, themedResourceName);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RTL_OSVERSIONINFOEXW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        private static Version GetRealWindowsVersion()
        {
            var version = new RTL_OSVERSIONINFOEXW {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RTL_OSVERSIONINFOEXW))
            };

            int st = RtlGetVersion(ref version);
            if (st != 0)
                throw Marshal.GetExceptionForHR(st);

            return new Version(
                (int)version.dwMajorVersion,
                (int)version.dwMinorVersion,
                (int)version.dwBuildNumber,
                (version.wServicePackMajor << 16) | version.wServicePackMinor);
        }

        [DllImport("ntdll", CharSet = CharSet.Auto)]
        private static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEXW lpVersionInformation);

        private static AssemblyName GetFullAssemblyNameFromPartialName(
            Assembly protoAssembly, string partialName)
        {
            return new AssemblyName(protoAssembly.FullName) {
                Name = partialName
            };
        }

        private static Uri MakePackUri(AssemblyName assemblyName, string path)
        {
            var name = FormatName(assemblyName);
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
