namespace StyleCore.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class UxThemeExNativeMethods
    {
        private const string UxthemeLib = "uxtheme.dll";

        [DllImport("UxThemeEx")]
        public static extern HResult UxOpenThemeFile(
            [MarshalAs(UnmanagedType.LPWStr)] string themeFileName,
            out SafeThemeFileHandle hThemeFile);

        [DllImport("UxThemeEx")]
        public static extern HResult UxCloseThemeFile(IntPtr hThemeFile);

        [DllImport("UxThemeEx", SetLastError = true)]
        public static extern SafeThemeHandle UxOpenThemeData(
            SafeThemeFileHandle themeFile, IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport("UxThemeEx")]
        public static extern HResult UxCloseThemeData(IntPtr hThemeFile, IntPtr hTheme);

        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeBitmap(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, [MarshalAs(UnmanagedType.U4)] GBF dwFlags,
            out IntPtr phBitmap);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeBool(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out bool pfVal);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeColor(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out int pColor);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeEnumValue(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out int piVal);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeFilename(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, StringBuilder pszThemeFilename,
            int cchMaxBuffChars);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeInt(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out int piVal);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeMetric(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, IntPtr hdc,
            int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeIntList(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, INTLIST pIntList);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeFont(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, IntPtr hdc,
            int iPartId, int iStateId, int iPropId, [Out] LOGFONT pFont);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeMargins(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, IntPtr hdc,
            int iPartId, int iStateId, int iPropId, CRECT prc, out MARGINS pMargins);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemePosition(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out POINT pPoint);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeRect(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out RECT pRect);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeStream(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, out IntPtr ppvStream, out uint pcbStream,
            SafeModuleHandle hInst);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeString(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, int iPartId,
            int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemePartSize(
            SafeThemeFileHandle hThemeFile, SafeThemeHandle hTheme, IntPtr hdc,
            int iPartId, int iStateId, CRECT prc, ThemeSize eSize, out SIZE psz);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemeTransitionDuration(
            SafeThemeFileHandle hThemeFile,
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateIdFrom,
            int iStateIdTo,
            int iPropId,
            out uint pdwDuration);
        [DllImport("UxThemeEx")]
        public static extern HResult UxGetThemePropertyOrigin(
            SafeThemeFileHandle hThemeFile,
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            [MarshalAs(UnmanagedType.U4)] out PropertyOrigin pOrigin);

        [DllImport(UxthemeLib)]
        public static extern HResult GetThemeSysFont(SafeThemeHandle hTheme, int iFontID, [Out] LOGFONT pFont);
        [DllImport(UxthemeLib)]
        public static extern HResult GetThemeSysString(SafeThemeHandle hTheme, int iStringID, StringBuilder pszStringBuff, int cchMaxStringChars);
        [DllImport(UxthemeLib)]
        public static extern bool GetThemeSysBool(SafeThemeHandle hTheme, int iBoolId);
        [DllImport(UxthemeLib)]
        public static extern int GetThemeSysColor(SafeThemeHandle hTheme, int iColorID);
        [DllImport(UxthemeLib)]
        public static extern HResult GetThemeSysInt(SafeThemeHandle hTheme, int iIntId, out int piValue);

        [DllImport("UxThemeEx")]
        public static extern HResult UxOverrideTheme(SafeThemeFileHandle themeFile);

        [DllImport("UxThemeEx")]
        public static extern HResult UxHook();

        [DllImport("UxThemeEx")]
        public static extern HResult UxUnhook();

        [DllImport("UxThemeEx")]
        public static extern void UxBroadcastThemeChange();
    }
}