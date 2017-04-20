namespace ThemeBrowser
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Media;
    using StyleCore.Native;

    public class ThemeData : IThemeData
    {
        private readonly SafeThemeHandle theme;

        private ThemeData(SafeThemeHandle theme)
        {
            this.theme = theme;
        }

        public bool IsValid => !theme.IsInvalid;

        public static ThemeData Open(IntPtr hwnd, string classList)
        {
            return new ThemeData(StyleNativeMethods.OpenThemeData(hwnd, classList));
        }

        public void Dispose()
        {
            theme.Dispose();
        }

        public bool? GetThemeBool(int partId, int stateId, int propertyId)
        {
            bool value;
            HResult hr = StyleNativeMethods.GetThemeBool(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (bool?)null;
        }

        public bool GetThemeSysBool(int propertyId)
        {
            return StyleNativeMethods.GetThemeSysBool(theme, propertyId);
        }

        public Color? GetThemeColor(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = StyleNativeMethods.GetThemeColor(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? ThemeExtensions.ColorFromArgb(value) : (Color?)null;
        }

        public Color GetThemeSysColor(int propertyId)
        {
            int value = StyleNativeMethods.GetThemeSysColor(theme, propertyId);
            return ThemeExtensions.ColorFromArgb(value);
        }

        public Stream GetThemeStream(int partId, int stateId, int propertyId, SafeModuleHandle instance)
        {
            IntPtr stream;
            uint length;
            HResult hr = StyleNativeMethods.GetThemeStream(theme, partId, stateId, propertyId, out stream, out length, instance);

            if (!Found(hr))
                return null;

            var buffer = new byte[length];
            Marshal.Copy(stream, buffer, 0, buffer.Length);
            return new MemoryStream(buffer);
        }

        public int? GetThemeEnumValue(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = StyleNativeMethods.GetThemeEnumValue(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public string GetThemeFilename(int partId, int stateId, int propertyId)
        {
            var filename = new StringBuilder(512);
            HResult hr = StyleNativeMethods.GetThemeFilename(theme, partId, stateId, propertyId, filename, filename.Capacity);
            return Found(hr) ? filename.ToString() : null;
        }

        public LOGFONT GetThemeFont(int partId, int stateId, int propertyId)
        {
            var value = new LOGFONT();
            HResult hr = StyleNativeMethods.GetThemeFont(theme, IntPtr.Zero, partId, stateId, propertyId, value);
            return Found(hr) ? value : null;
        }

        public LOGFONT GetThemeSysFont(int propertyId)
        {
            var value = new LOGFONT();
            HResult hr = StyleNativeMethods.GetThemeSysFont(theme, propertyId, value);
            return Found(hr) ? value : null;
        }

        public IntPtr? GetThemeBitmap(int partId, int stateId, int propertyId)
        {
            IntPtr value;
            HResult hr = StyleNativeMethods.GetThemeBitmap(theme, partId, stateId, propertyId, GBF.GBF_DIRECT, out value);
            return hr.Succeeded() ? value : (IntPtr?)null;
        }

        public int? GetThemeInt(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = StyleNativeMethods.GetThemeInt(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public int? GetThemeSysInt(int propertyId)
        {
            int value;
            HResult hr = StyleNativeMethods.GetThemeSysInt(theme, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public int? GetThemeMetric(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = StyleNativeMethods.GetThemeMetric(theme, IntPtr.Zero, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public INTLIST GetThemeIntList(int partId, int stateId, int propertyId)
        {
            var value = new INTLIST();
            HResult hr = StyleNativeMethods.GetThemeIntList(theme, partId, stateId, propertyId, value);
            return Found(hr) ? value : null;
        }

        public MARGINS? GetThemeMargins(int partId, int stateId, int propertyId)
        {
            MARGINS value;
            HResult hr = StyleNativeMethods.GetThemeMargins(theme, IntPtr.Zero, partId, stateId, propertyId, null, out value);
            return Found(hr) ? value : (MARGINS?)null;
        }

        public POINT? GetThemePosition(int partId, int stateId, int propertyId)
        {
            POINT value;
            HResult hr = StyleNativeMethods.GetThemePosition(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (POINT?)null;
        }

        public RECT? GetThemeRect(int partId, int stateId, int propertyId)
        {
            RECT value;
            HResult hr = StyleNativeMethods.GetThemeRect(theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (RECT?)null;
        }

        public SIZE? GetThemePartSize(int partId, int stateId, ThemeSize themeSize)
        {
            SIZE value;
            HResult hr = StyleNativeMethods.GetThemePartSize(theme, IntPtr.Zero, partId, stateId, null, themeSize, out value);
            return Found(hr) ? value : (SIZE?)null;
        }

        public string GetThemeString(int partId, int stateId, int propertyId)
        {
            var value = new StringBuilder(512);
            HResult hr = StyleNativeMethods.GetThemeString(theme, partId, stateId, propertyId, value, value.Capacity);
            return Found(hr) ? value.ToString() : null;
        }

        public string GetThemeSysString(int propertyId)
        {
            var value = new StringBuilder(512);
            HResult hr = StyleNativeMethods.GetThemeSysString(theme, propertyId, value, value.Capacity);
            return Found(hr) ? value.ToString() : null;
        }

        public HResult GetThemeTransitionDuration(int partId, int stateFrom, int stateTo, int propertyId, out uint duration)
        {
            return StyleNativeMethods.GetThemeTransitionDuration(theme, partId, stateFrom, stateTo, propertyId, out duration);
        }

        public PropertyOrigin GetThemePropertyOrigin(int partId, int stateId, int propertyId)
        {
            PropertyOrigin origin;
            HResult hr = StyleNativeMethods.GetThemePropertyOrigin(
                theme, partId, stateId, propertyId, out origin);
            return hr.Succeeded() ? origin : PropertyOrigin.NotFound;
        }

        private static bool Found(HResult hr)
        {
            if (hr.Failed()) {
                if (hr == HResult.ElementNotFound || hr == HResult.InvalidArgument)
                    return false;

                Marshal.ThrowExceptionForHR((int)hr);
            }

            return true;
        }
    }
}
