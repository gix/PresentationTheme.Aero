namespace PresentationTheme.Aero
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;

    /// <summary>
    ///   Provides the default Aero theme policy which chooses the appropriate
    ///   theme resources depending on the current system theme and Windows version.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Use the policy by passing <see cref="GetCurrentThemeUri"/> to
    ///     <see cref="ThemeManager.SetPresentationFrameworkTheme(IThemePolicy)"/>
    ///     or <see cref="ThemeManager.SetTheme(Assembly,IThemePolicy)"/>.
    ///   </para>
    ///   <para>
    ///     The policy chooses the following theme resource assemblies:
    ///     <list type="table">
    ///       <item>
    ///         <term>Windows 8 with Aero theme</term>
    ///         <description><c>PresentationTheme.Aero.Win8.dll</c></description>
    ///       </item>
    ///       <item>
    ///         <term>Windows 10 with Aero theme</term>
    ///         <description><c>PresentationTheme.Aero.Win10.dll</c></description>
    ///       </item>
    ///       <item>
    ///         <term>Windows 10 with AeroLite theme</term>
    ///         <description><c>PresentationTheme.AeroLite.Win10.dll</c></description>
    ///       </item>
    ///       <item>
    ///         <term>Windows 10 in high contrast mode</term>
    ///         <description>
    ///           <c>PresentationTheme.HighContrast.Win10.dll</c>
    ///         </description>
    ///       </item>
    ///       <item>
    ///         <term>Other Windows versions</term>
    ///         <description>Fallback to default theme</description>
    ///       </item>
    ///     </list>
    ///   </para>
    /// </remarks>
    public class AeroThemePolicy : IThemePolicy
    {
        bool IThemePolicy.MergeWithBaseResources => true;

        /// <summary>
        ///   Builds the pack <see cref="Uri"/> for the theme resources matching
        ///   the current system theme and Windows version.
        /// </summary>
        /// <returns>
        ///   An absolute pack <see cref="Uri"/> to a <see cref="ResourceDictionary"/>
        ///   with the theme resources. Returns <see langword="null"/> if the
        ///   current system theme or Windows version are not supported.
        /// </returns>
        public virtual Uri GetCurrentThemeUri()
        {
            var osVersion = GetRealWindowsVersion();
            string uxThemeName = SystemParameters.UxThemeName;
            string uxThemeColor = SystemParameters.UxThemeColor;
            bool highContrast = SystemParameters.HighContrast;

            string resourceName = GetThemeResourceName(osVersion, uxThemeName, uxThemeColor, highContrast);
            if (resourceName == null)
                return null;

            string partialName = $"PresentationTheme.{resourceName}";

            var asmName = GetFullAssemblyNameFromPartialName(typeof(AeroTheme).Assembly, partialName);
            string themedResourceName =
                "themes/" + resourceName.ToLowerInvariant() + "." + uxThemeColor.ToLowerInvariant() + ".xaml";

            return PackUriUtils.MakeContentPackUri(asmName, themedResourceName);
        }

        /// <summary>
        ///   Gets theme resource name matching the current system theme and
        ///   Windows version.
        /// </summary>
        /// <param name="osVersion">The Windows version.</param>
        /// <param name="themeName">The system theme name.</param>
        /// <param name="themeColor">The system theme color.</param>
        /// <param name="highContrast">
        ///   Indicates whether Windows is in high contrast mode.
        /// </param>
        /// <returns>
        ///   An absolute pack <see cref="Uri"/> to a <see cref="ResourceDictionary"/>
        ///   with the theme resources.
        /// </returns>
        /// <returns>
        ///   The theme resource name. Returns <see langword="null"/> if the
        ///   current system theme or Windows version are not supported.
        /// </returns>
        protected virtual string GetThemeResourceName(
            Version osVersion, string themeName, string themeColor, bool highContrast)
        {
            var win7 = new Version(6, 1);
            var win8 = new Version(6, 2);
            var win10 = new Version(10, 0);

            if (string.Equals(themeName, "Aero", StringComparison.OrdinalIgnoreCase)) {
                if (osVersion >= win10)
                    return "Aero.Win10";
                if(osVersion >= win8)
                    return "Aero.Win8";
            } else if (string.Equals(themeName, "AeroLite", StringComparison.OrdinalIgnoreCase)) {
                if (highContrast) {
                    if (osVersion >= win10)
                        return "HighContrast.Win10";
                } else {
                    if (osVersion >= win10)
                        return "AeroLite.Win10";
                }
            }

            return null;
        }

        private static AssemblyName GetFullAssemblyNameFromPartialName(
            Assembly protoAssembly, string partialName)
        {
            return new AssemblyName(protoAssembly.FullName) {
                Name = partialName
            };
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

        internal static Version GetRealWindowsVersion()
        {
            var version = new RTL_OSVERSIONINFOEXW {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RTL_OSVERSIONINFOEXW))
            };

            // RtlGetVersion does not require a manifest to detect Windows 10+.
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
    }
}
