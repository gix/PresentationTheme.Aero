namespace ThemeBrowser
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Media;
    using ThemeCore.Native;

    public class UxThemeExData : IThemeData
    {
        private readonly SafeThemeFileHandle themeFile;
        private readonly SafeThemeHandle theme;

        private UxThemeExData(SafeThemeFileHandle themeFile, SafeThemeHandle theme)
        {
            this.themeFile = themeFile;
            this.theme = theme;
        }

        public bool IsValid => !theme.IsInvalid;

        public static UxThemeExData Open(
            SafeThemeFileHandle themeFile, IntPtr hwnd, string classList)
        {
            return new UxThemeExData(
                themeFile, UxThemeExNativeMethods.UxOpenThemeData(themeFile, hwnd, classList));
        }

        public void Dispose()
        {
            theme.Dispose();
        }

        public bool? GetThemeBool(int partId, int stateId, int propertyId)
        {
            bool value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeBool(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (bool?)null;
        }

        public Color? GetThemeColor(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeColor(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? ThemeExtensions.ColorFromArgb(value) : (Color?)null;
        }

        public unsafe Stream GetThemeStream(
            int partId, int stateId, int propertyId, SafeModuleHandle instance)
        {
            IntPtr stream;
            uint length;
            HResult hr = UxThemeExNativeMethods.UxGetThemeStream(
                themeFile, theme, partId, stateId, propertyId, out stream,
                out length, instance);

            if (!Found(hr) || stream == IntPtr.Zero)
                return null;

            var buffer = new byte[length];
            if (length > 0)
                Marshal.Copy(stream, buffer, 0, buffer.Length);
            return new UnmanagedMemoryStream(
                (byte*)stream.ToPointer(), 0, length, FileAccess.Read);
        }

        public int? GetThemeEnumValue(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeEnumValue(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public string GetThemeFilename(int partId, int stateId, int propertyId)
        {
            var filename = new StringBuilder(512);
            HResult hr = UxThemeExNativeMethods.UxGetThemeFilename(
                themeFile, theme, partId, stateId, propertyId, filename, filename.Capacity);
            return Found(hr) ? filename.ToString() : null;
        }

        public LOGFONT GetThemeFont(int partId, int stateId, int propertyId)
        {
            var value = new LOGFONT();
            HResult hr = UxThemeExNativeMethods.UxGetThemeFont(
                themeFile, theme, IntPtr.Zero, partId, stateId, propertyId, value);
            return Found(hr) ? value : null;
        }

        public IntPtr? GetThemeBitmap(int partId, int stateId, int propertyId)
        {
            IntPtr value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeBitmap(
                themeFile, theme, partId, stateId, propertyId, GBF.GBF_DIRECT, out value);
            return hr.Succeeded() ? value : (IntPtr?)null;
        }

        public int? GetThemeInt(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeInt(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public int? GetThemeMetric(int partId, int stateId, int propertyId)
        {
            int value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeMetric(
                themeFile, theme, IntPtr.Zero, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public INTLIST GetThemeIntList(int partId, int stateId, int propertyId)
        {
            var value = new INTLIST();
            HResult hr = UxThemeExNativeMethods.UxGetThemeIntList(
                themeFile, theme, partId, stateId, propertyId, value);
            return Found(hr) ? value : null;
        }

        public MARGINS? GetThemeMargins(int partId, int stateId, int propertyId)
        {
            MARGINS value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeMargins(
                themeFile, theme, IntPtr.Zero, partId, stateId, propertyId, null, out value);
            return Found(hr) ? value : (MARGINS?)null;
        }

        public POINT? GetThemePosition(int partId, int stateId, int propertyId)
        {
            POINT value;
            HResult hr = UxThemeExNativeMethods.UxGetThemePosition(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (POINT?)null;
        }

        public RECT? GetThemeRect(int partId, int stateId, int propertyId)
        {
            RECT value;
            HResult hr = UxThemeExNativeMethods.UxGetThemeRect(
                themeFile, theme, partId, stateId, propertyId, out value);
            return Found(hr) ? value : (RECT?)null;
        }

        public SIZE? GetThemePartSize(int partId, int stateId, ThemeSize themeSize)
        {
            SIZE value;
            HResult hr = UxThemeExNativeMethods.UxGetThemePartSize(
                themeFile, theme, IntPtr.Zero, partId, stateId, null, themeSize, out value);
            return Found(hr) ? value : (SIZE?)null;
        }

        public string GetThemeString(int partId, int stateId, int propertyId)
        {
            var value = new StringBuilder(512);
            HResult hr = UxThemeExNativeMethods.UxGetThemeString(
                themeFile, theme, partId, stateId, propertyId, value, value.Capacity);
            return Found(hr) ? value.ToString() : null;
        }

        public HResult GetThemeTransitionDuration(
            int partId, int stateFrom, int stateTo, int propertyId, out uint duration)
        {
            return UxThemeExNativeMethods.UxGetThemeTransitionDuration(
                themeFile, theme, partId, stateFrom, stateTo, propertyId, out duration);
        }

        public PropertyOrigin GetThemePropertyOrigin(
            int partId, int stateId, int propertyId)
        {
            PropertyOrigin origin;
            HResult hr = UxThemeExNativeMethods.UxGetThemePropertyOrigin(
                themeFile, theme, partId, stateId, propertyId, out origin);
            return hr.Succeeded() ? origin : PropertyOrigin.NotFound;
        }

        public bool GetThemeSysBool(int propertyId)
        {
            return UxThemeExNativeMethods.GetThemeSysBool(theme, propertyId);
        }

        public Color GetThemeSysColor(int propertyId)
        {
            int value = UxThemeExNativeMethods.GetThemeSysColor(theme, propertyId);
            return ThemeExtensions.ColorFromArgb(value);
        }

        public LOGFONT GetThemeSysFont(int propertyId)
        {
            var value = new LOGFONT();
            HResult hr = UxThemeExNativeMethods.GetThemeSysFont(theme, propertyId, value);
            return Found(hr) ? value : null;
        }

        public int? GetThemeSysInt(int propertyId)
        {
            int value;
            HResult hr = UxThemeExNativeMethods.GetThemeSysInt(
                theme, propertyId, out value);
            return Found(hr) ? value : (int?)null;
        }

        public string GetThemeSysString(int propertyId)
        {
            var value = new StringBuilder(512);
            HResult hr = UxThemeExNativeMethods.GetThemeSysString(
                theme, propertyId, value, value.Capacity);
            return Found(hr) ? value.ToString() : null;
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
