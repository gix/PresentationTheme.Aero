namespace ThemeTestApp
{
    using System;
    using System.Drawing;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms.VisualStyles;
    using ThemeTestApp.Samples;

    internal static class NativeMethods
    {
        public const int WM_CREATE = 0x1;
        public const int WM_PAINT = 0x000F;
        public const int WM_NOTIFY = 0x004E;

        public const int CDRF_DODEFAULT = 0x00000000;

        public const int TVM_SETEXTENDEDSTYLE = (0x1100 + 44);
        public const int TVM_GETEXTENDEDSTYLE = (0x1100 + 45);

        public const int TVS_EX_MULTISELECT = 0x0002;
        public const int TVS_EX_DOUBLEBUFFER = 0x0004;
        public const int TVS_EX_NOINDENTSTATE = 0x0008;
        public const int TVS_EX_RICHTOOLTIP = 0x0010;
        public const int TVS_EX_AUTOHSCROLL = 0x0020;
        public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
        public const int TVS_EX_PARTIALCHECKBOXES = 0x0080;
        public const int TVS_EX_EXCLUSIONCHECKBOXES = 0x0100;
        public const int TVS_EX_DIMMEDCHECKBOXES = 0x0200;
        public const int TVS_EX_DRAWIMAGEASYNC = 0x0400;


        public const int LVS_EX_GRIDLINES = 0x00000001;
        public const int LVS_EX_SUBITEMIMAGES = 0x00000002;
        public const int LVS_EX_CHECKBOXES = 0x00000004;
        public const int LVS_EX_TRACKSELECT = 0x00000008;
        public const int LVS_EX_HEADERDRAGDROP = 0x00000010;
        public const int LVS_EX_FULLROWSELECT = 0x00000020; // applies to report mode only
        public const int LVS_EX_ONECLICKACTIVATE = 0x00000040;
        public const int LVS_EX_TWOCLICKACTIVATE = 0x00000080;
        public const int LVS_EX_FLATSB = 0x00000100;
        public const int LVS_EX_REGIONAL = 0x00000200;
        public const int LVS_EX_INFOTIP = 0x00000400; // listview does InfoTips for you
        public const int LVS_EX_UNDERLINEHOT = 0x00000800;
        public const int LVS_EX_UNDERLINECOLD = 0x00001000;
        public const int LVS_EX_MULTIWORKAREAS = 0x00002000;
        public const int LVS_EX_LABELTIP = 0x00004000; // listview unfolds partly hidden labels if it does not have infotip text
        public const int LVS_EX_BORDERSELECT = 0x00008000; // border selection style instead of highlight
        public const int LVS_EX_DOUBLEBUFFER = 0x00010000;
        public const int LVS_EX_HIDELABELS = 0x00020000;
        public const int LVS_EX_SINGLEROW = 0x00040000;
        public const int LVS_EX_SNAPTOGRID = 0x00080000; // Icons automatically snap to grid.
        public const int LVS_EX_SIMPLESELECT = 0x00100000; // Also changes overlay rendering to top right for icon mode.
        public const int LVS_EX_JUSTIFYCOLUMNS = 0x00200000; // Icons are lined up in columns that use up the whole view area.
        public const int LVS_EX_TRANSPARENTBKGND = 0x00400000; // Background is painted by the parent via WM_PRINTCLIENT
        public const int LVS_EX_TRANSPARENTSHADOWTEXT = 0x00800000; // Enable shadow text on transparent backgrounds only (useful with bitmaps)
        public const int LVS_EX_AUTOAUTOARRANGE = 0x01000000; // Icons automatically arrange if no icon positions have been set
        public const int LVS_EX_HEADERINALLVIEWS = 0x02000000; // Display column header in all view modes
        public const int LVS_EX_AUTOCHECKSELECT = 0x08000000;
        public const int LVS_EX_AUTOSIZECOLUMNS = 0x10000000;
        public const int LVS_EX_COLUMNSNAPPOINTS = 0x40000000;
        public const int LVS_EX_COLUMNOVERFLOW = unchecked((int)0x80000000);

        public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        public static int ColorToCOLORREF(Color color)
        {
            return ((color.R | (color.G << 8)) | (color.B << 0x10));
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkMode(HandleRef hDC, [MarshalAs(UnmanagedType.I4)] BkMode nBkMode);
    }

    enum BkMode
    {
        Opaque = 0,
        Transparent = 1,
    }

    internal static class VisualStylesNativeMethods
    {
        private const string UxthemeLib = "uxtheme.dll";

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ExternalDeleteObject(HandleRef hObject);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern SafeThemeHandle OpenThemeData(
            HandleRef hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern SafeThemeHandle OpenThemeData(
            IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(IntPtr hTheme);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeSysColor(
            SafeThemeHandle hTheme, int iColorID);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] RECT pRect, [In] IntPtr pClipRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(
            SafeThemeHandle hTheme, HandleRef hdc, int partId, int stateId, [In] RECT pRect, [In] RECT pClipRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] ref RECT pBoundingRect,
            out RECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] RECT pBoundingRect,
            out RECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern RECT GetThemeBackgroundContentRect(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] RECT pBoundingRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] RECT pContentRect,
            out RECT pExtentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern RECT GetThemeBackgroundExtent(
            SafeThemeHandle hTheme,
            HandleRef hdc,
            int iPartId,
            int iStateId,
            [In] RECT pContentRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int DrawThemeText(
            HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId,
            [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount,
            int dwTextFlags, int dwTextFlags2, [In] RECT pRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId, out RECT pRect);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern RECT GetThemeRect(
            SafeThemeHandle hTheme, int iPartId, int iStateId, int iPropId);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemePartSize(
            SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId, ref RECT prc, ThemeSize eSize, out  SIZE psz);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern SIZE GetThemePartSize(
            SafeThemeHandle hTheme, HandleRef hdc, int iPartId, int iStateId, ref RECT prc, ThemeSize eSize);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto)]
        public static extern int GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, RECT prc, out MARGINS pMargins);

        [DllImport(UxthemeLib, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern MARGINS GetThemeMargins(
            SafeThemeHandle hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, RECT prc);

        [DllImport(UxthemeLib, CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(
            IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int CloseThemeData(HandleRef hTheme);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(HandleRef hTheme, HandleRef hdc, int partId, int stateId, [In] RECT pRect, [In] RECT pClipRect);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int DrawThemeEdge(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] RECT pDestRect, int uEdge, int uFlags, [Out] RECT pContentRect);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int DrawThemeParentBackground(HandleRef hwnd, HandleRef hdc, [In] RECT prc);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int HitTestThemeBackground(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int dwOptions, [In] RECT pRect, HandleRef hrgn, [In] POINTSTRUCT ptTest, ref int pwHitTestCode);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern void SetThemeAppProperties(int Flags);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool IsAppThemed();
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool IsThemeBackgroundPartiallyTransparent(HandleRef hTheme, int iPartId, int iStateId);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool IsThemePartDefined(HandleRef hTheme, int iPartId, int iStateId);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeAppProperties();
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] RECT pBoundingRect, [Out] RECT pContentRect);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] RECT pContentRect, [Out] RECT pExtentRect);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeBackgroundRegion(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] RECT pRect, ref IntPtr pRegion);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeBool(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref bool pfVal);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeColor(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int pColor);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeDocumentationProperty([MarshalAs(UnmanagedType.LPWStr)] string pszThemeName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName, StringBuilder pszValueBuff, int cchMaxValChars);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeEnumValue(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeFilename(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeFont(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int iPropId, LOGFONT pFont);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeInt(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeMargins(HandleRef hTheme, HandleRef hDC, int iPartId, int iStateId, int iPropId, ref MARGINS margins);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemePartSize(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] RECT prc, ThemeSizeType eSize, [Out] SIZE psz);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemePosition(HandleRef hTheme, int iPartId, int iStateId, int iPropId, [Out] POINT pPoint);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeString(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool GetThemeSysBool(HandleRef hTheme, int iBoolId);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeSysInt(HandleRef hTheme, int iIntId, ref int piValue);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeTextExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlags, [In] RECT pBoundingRect, [Out] RECT pExtentRect);
        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int GetThemeTextMetrics(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, ref TextMetrics ptm);
    }

    internal sealed class SafeThemeHandle : SafeHandleZeroIsInvalid
    {
        public SafeThemeHandle()
            : base(true)
        {
        }

        public static SafeThemeHandle Zero
        {
            get { return new SafeThemeHandle(); }
        }

        protected override bool ReleaseHandle()
        {
            return VisualStylesNativeMethods.CloseThemeData(handle) >= 0;
        }
    }

    [SecurityCritical]
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    public abstract class SafeHandleZeroIsInvalid : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected SafeHandleZeroIsInvalid(bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get { return handle == new IntPtr(); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public enum ThemeSize
    {
        Min,
        True,
        Draw
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public override string ToString()
        {
            return string.Concat("Left = ", left, " Top ", top, " Right = ", right, " Bottom = ", bottom);
        }

        public RECT()
        {
        }

        public RECT(Rectangle r)
        {
            left = r.X;
            top = r.Y;
            right = r.Right;
            bottom = r.Bottom;
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string lfFaceName;

        public LOGFONT()
        {
        }

        public LOGFONT(LOGFONT lf)
        {
            lfHeight = lf.lfHeight;
            lfWidth = lf.lfWidth;
            lfEscapement = lf.lfEscapement;
            lfOrientation = lf.lfOrientation;
            lfWeight = lf.lfWeight;
            lfItalic = lf.lfItalic;
            lfUnderline = lf.lfUnderline;
            lfStrikeOut = lf.lfStrikeOut;
            lfCharSet = lf.lfCharSet;
            lfOutPrecision = lf.lfOutPrecision;
            lfClipPrecision = lf.lfClipPrecision;
            lfQuality = lf.lfQuality;
            lfPitchAndFamily = lf.lfPitchAndFamily;
            lfFaceName = lf.lfFaceName;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
        public int x;
        public int y;
        public POINT()
        {
        }

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
