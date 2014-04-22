namespace ThemePreviewer
{
    using System;
    using System.Diagnostics;

    public static class ThemeUtils
    {
        public static void SendThemeChangedProcessLocal()
        {
            foreach (var hwnd in NativeMethods.EnumerateProcessWindows()) {
                Debug.WriteLine(
                    "ThemeChanged: 0x{0:X8} ({1})",
                    hwnd.ToInt64(), NativeMethods.GetClassName(hwnd));

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
