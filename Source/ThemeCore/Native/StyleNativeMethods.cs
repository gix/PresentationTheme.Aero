namespace ThemeCore.Native
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms.VisualStyles;

    public static class StyleNativeMethods
    {
        [DllImport("gdi32", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ExternalDeleteObject(HandleRef hObject);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern SafeThemeHandle OpenThemeData(
            HandleRef hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport("uxtheme", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeThemeHandle OpenThemeData(
            IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(IntPtr hTheme);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] IntPtr pClipRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] CRECT pClipRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] ref CRECT pBoundingRect,
            out CRECT pContentRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pBoundingRect,
            out CRECT pContentRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pBoundingRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pContentRect,
            CRECT pExtentRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pContentRect);

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public static extern int DrawThemeText(
            SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId,
            string pszText, int iCharCount, int dwTextFlags, int dwTextFlags2,
            [In] CRECT pRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out CRECT pRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId);

        [DllImport("uxtheme", CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern SIZE GetThemePartSize(
            SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId, CRECT prc, ThemeSize eSize);

        [DllImport("uxtheme", CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern MARGINS GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, CRECT prc);

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(
            IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(HandleRef hTheme);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(HandleRef hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] CRECT pClipRect);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int DrawThemeEdge(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pDestRect, int uEdge, int uFlags, [Out] CRECT pContentRect);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int DrawThemeParentBackground(HandleRef hwnd, HandleRef hdc, [In] CRECT prc);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int HitTestThemeBackground(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int dwOptions, [In] CRECT pRect, HandleRef hrgn, [In] POINT ptTest, ref int pwHitTestCode);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern void SetThemeAppProperties(int Flags);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern bool IsAppThemed();
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern bool IsThemeBackgroundPartiallyTransparent(SafeThemeHandle hTheme, int iPartId, int iStateId);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern bool IsThemePartDefined(HandleRef hTheme, int iPartId, int iStateId);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeAppProperties();
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pBoundingRect, [Out] CRECT pContentRect);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pContentRect, [Out] CRECT pExtentRect);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundRegion(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pRect, ref IntPtr pRegion);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeBool(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref bool pfVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeColor(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int pColor);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeDocumentationProperty([MarshalAs(UnmanagedType.LPWStr)] string pszThemeName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName, StringBuilder pszValueBuff, int cchMaxValChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeEnumValue(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeFilename(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeFilename(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeFont(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int iPropId, LOGFONT pFont);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeInt(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeMargins(HandleRef hTheme, HandleRef hDC, int iPartId, int iStateId, int iPropId, ref MARGINS margins);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemePartSize(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT prc, ThemeSize eSize, [Out] SIZE psz);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemePosition(HandleRef hTheme, int iPartId, int iStateId, int iPropId, [Out] out POINT pPoint);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeString(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern bool GetThemeSysBool(HandleRef hTheme, int iBoolId);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeSysInt(HandleRef hTheme, int iIntId, ref int piValue);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeTextExtent(SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlags, [In] CRECT pBoundingRect, [Out] out Rectangle pExtentRect);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeTextMetrics(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, ref TextMetrics ptm);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeBitmap(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId,
           [MarshalAs(UnmanagedType.U4)] GBF dwFlags, out IntPtr phBitmap);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeBool(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out bool pfVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeColor(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int pColor);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeEnumValue(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeInt(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeMetric(SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeIntList(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, INTLIST pIntList);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeFont(SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, [Out] LOGFONT pFont);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysFont(SafeThemeHandle hTheme, int iFontID, [Out] LOGFONT pFont);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId,
            int iPropId, CRECT prc, out MARGINS pMargins);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemePosition(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out POINT pPoint);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeRect(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out RECT pRect);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeStream(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId,
            out IntPtr ppvStream, out uint pcbStream, SafeModuleHandle hInst);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeString(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysString(SafeThemeHandle hTheme, int iStringID, StringBuilder pszStringBuff, int cchMaxStringChars);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemePartSize(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, CRECT prc, ThemeSize eSize, out Size psz);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemePartSize(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, CRECT prc, ThemeSize eSize, out SIZE psz);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern bool GetThemeSysBool(SafeThemeHandle hTheme, int iBoolId);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern int GetThemeSysColor(SafeThemeHandle hTheme, int iColorID);
        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysInt(SafeThemeHandle hTheme, int iIntId, out int piValue);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemeTransitionDuration(
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateIdFrom,
            int iStateIdTo,
            int iPropId,
            out uint  pdwDuration);

        [DllImport("uxtheme", CharSet = CharSet.Auto)]
        public static extern HResult GetThemePropertyOrigin(
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            [MarshalAs(UnmanagedType.U4)] out PropertyOrigin pOrigin);

        [DllImport("uxtheme", EntryPoint = "#65", CharSet = CharSet.Unicode)]
        public static extern int SetSystemVisualStyle(string pszFilename, string pszColor, string pszSize, int dwReserved);
    }

    [Flags]
    public enum PropertyOrigin
    {
        Unknown = 0,
        Global = 1,
        Class = 2,
        Part = 4,
        State = 8,
        NotFound = 16,
        Inherited = 32,
        Default = 64,
    }

    [StructLayout(LayoutKind.Sequential)]
    public class INTLIST
    {
        public int iValueCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 402)]
        public int[] iValues;
    }

    public enum GBF
    {
        GBF_DIRECT = 0x1,
        GBF_COPY = 0x2,
        GBF_VALIDBITS = GBF_DIRECT | GBF_COPY,
    }
}
