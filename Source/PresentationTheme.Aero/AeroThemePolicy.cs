namespace PresentationTheme.Aero
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;

    public class AeroThemePolicy
    {
        public virtual Uri BuildCurrentResourceUri()
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
                "themes/" + resourceName.ToLowerInvariant() + "." + uxThemeColor.ToLowerInvariant();

            return PackUriUtils.MakeContentPackUri(asmName, themedResourceName);
        }

        protected virtual string GetThemeResourceName(
            Version osVersion, string themeName, string themeColor, bool highContrast)
        {
            var win7 = new Version(6, 1);
            var win8 = new Version(6, 2);
            var win10 = new Version(10, 0);

            string resourceName = null;
            if (string.Equals(themeName, "Aero", StringComparison.OrdinalIgnoreCase)) {
                if (osVersion >= win10)
                    resourceName = "Aero.Win10";
                else if (osVersion >= win8)
                    resourceName = "Aero.Win8";
                else if (osVersion >= win7)
                    resourceName = "Aero.Win7";
            } else if (string.Equals(themeName, "AeroLite", StringComparison.OrdinalIgnoreCase)) {
                if (highContrast) {
                    if (osVersion >= win10)
                        resourceName = "HighContrast.Win10";
                } else {
                    if (osVersion >= win10)
                        resourceName = "AeroLite.Win10";
                }
            }

            return resourceName;
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

        private static Version GetRealWindowsVersion()
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
