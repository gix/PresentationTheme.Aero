namespace StyleCore.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms.VisualStyles;

    public static class StyleNativeMethods
    {
        private const string UxthemeLib = "uxtheme.dll";

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ExternalDeleteObject(HandleRef hObject);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern SafeThemeHandle OpenThemeData(
            HandleRef hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeThemeHandle OpenThemeData(
            IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(IntPtr hTheme);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] IntPtr pClipRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] CRECT pClipRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] ref CRECT pBoundingRect,
            out CRECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pBoundingRect,
            out CRECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pBoundingRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pContentRect,
            CRECT pExtentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] CRECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeText(
            HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId,
            [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount,
            int dwTextFlags, int dwTextFlags2, [In] CRECT pRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out CRECT pRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern CRECT GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern SIZE GetThemePartSize(
            SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId, CRECT prc, ThemeSize eSize);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern MARGINS GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, CRECT prc);

        [DllImport(UxthemeLib, CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(
            IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(HandleRef hTheme);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(HandleRef hTheme, HandleRef hdc, int partId, int stateId, [In] CRECT pRect, [In] CRECT pClipRect);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeEdge(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pDestRect, int uEdge, int uFlags, [Out] CRECT pContentRect);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeParentBackground(HandleRef hwnd, HandleRef hdc, [In] CRECT prc);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int HitTestThemeBackground(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int dwOptions, [In] CRECT pRect, HandleRef hrgn, [In] POINT ptTest, ref int pwHitTestCode);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern void SetThemeAppProperties(int Flags);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern bool IsAppThemed();
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern bool IsThemeBackgroundPartiallyTransparent(HandleRef hTheme, int iPartId, int iStateId);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern bool IsThemePartDefined(HandleRef hTheme, int iPartId, int iStateId);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeAppProperties();
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pBoundingRect, [Out] CRECT pContentRect);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pContentRect, [Out] CRECT pExtentRect);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundRegion(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT pRect, ref IntPtr pRegion);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBool(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref bool pfVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeColor(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int pColor);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeDocumentationProperty([MarshalAs(UnmanagedType.LPWStr)] string pszThemeName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName, StringBuilder pszValueBuff, int cchMaxValChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeEnumValue(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeFilename(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeFilename(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeFont(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int iPropId, LOGFONT pFont);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeInt(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeMargins(HandleRef hTheme, HandleRef hDC, int iPartId, int iStateId, int iPropId, ref MARGINS margins);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemePartSize(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] CRECT prc, ThemeSizeType eSize, [Out] SIZE psz);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemePosition(HandleRef hTheme, int iPartId, int iStateId, int iPropId, [Out] out POINT pPoint);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeString(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern bool GetThemeSysBool(HandleRef hTheme, int iBoolId);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeSysInt(HandleRef hTheme, int iIntId, ref int piValue);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeTextExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlags, [In] CRECT pBoundingRect, [Out] CRECT pExtentRect);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeTextMetrics(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, ref TextMetrics ptm);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeBitmap(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId,
           [MarshalAs(UnmanagedType.U4)] GBF dwFlags, out IntPtr phBitmap);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeBool(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out bool pfVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeColor(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int pColor);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeEnumValue(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeInt(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeMetric(SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out int piVal);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeIntList(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, INTLIST pIntList);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeFont(SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, [Out] LOGFONT pFont);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysFont(SafeThemeHandle hTheme, int iFontID, [Out] LOGFONT pFont);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId,
            int iPropId, CRECT prc, out MARGINS pMargins);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemePosition(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out POINT pPoint);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeRect(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out RECT pRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeStream(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId,
            out IntPtr ppvStream, out uint pcbStream, SafeModuleHandle hInst);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeString(SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysString(SafeThemeHandle hTheme, int iStringID, StringBuilder pszStringBuff, int cchMaxStringChars);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemePartSize(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, CRECT prc, ThemeSize eSize, out SIZE psz);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern bool GetThemeSysBool(SafeThemeHandle hTheme, int iBoolId);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeSysColor(SafeThemeHandle hTheme, int iColorID);
        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeSysInt(SafeThemeHandle hTheme, int iIntId, out int piValue);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemeTransitionDuration(
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateIdFrom,
            int iStateIdTo,
            int iPropId,
            out uint  pdwDuration);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern HResult GetThemePropertyOrigin(
            SafeThemeHandle hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            [MarshalAs(UnmanagedType.U4)] out PropertyOrigin pOrigin);

        [DllImport(UxthemeLib, EntryPoint = "#65", CharSet = CharSet.Unicode)]
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
