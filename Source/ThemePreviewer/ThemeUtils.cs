namespace ThemePreviewer
{
    using System;
    using StyleCore.Native;

    public static class ThemeUtils
    {
        public static void SendThemeChangedGlobal()
        {
            NativeMethods.PostMessage(
                NativeMethods.HWND_BROADCAST, NativeMethods.WM_THEMECHANGED,
                IntPtr.Zero, IntPtr.Zero);
        }

        public static void SendThemeChangedProcessLocal()
        {
            foreach (var hwnd in NativeMethods.EnumerateProcessWindows()) {
                NativeMethods.PostMessage(
                    hwnd, NativeMethods.WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);

                NativeMethods.EnumChildWindows(hwnd, (hwndChild, o) => {
                    NativeMethods.PostMessage(
                        hwndChild, NativeMethods.WM_THEMECHANGED, IntPtr.Zero, IntPtr.Zero);
                    return true;
                }, IntPtr.Zero);
            }
        }
    }
}
