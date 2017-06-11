namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using ThemeCore.Native;
    using Control = System.Windows.Forms.Control;

    internal static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WM_CREATE = 0x1;
        public const int WM_PAINT = 0x000F;
        public const int WM_NOTIFY = 0x004E;
        public const int WM_USER = 0x400;
        public const int WM_THEMECHANGED = 0x31A;

        public const int WS_OVERLAPPED = 0x00000000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_CHILD = 0x40000000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_DISABLED = 0x08000000;
        public const int WS_CLIPSIBLINGS = 0x04000000;
        public const int WS_CLIPCHILDREN = 0x02000000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_CAPTION = 0x00C00000;
        public const int WS_BORDER = 0x00800000;
        public const int WS_DLGFRAME = 0x00400000;
        public const int WS_VSCROLL = 0x00200000;
        public const int WS_HSCROLL = 0x00100000;
        public const int WS_SYSMENU = 0x00080000;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_GROUP = 0x00020000;
        public const int WS_TABSTOP = 0x00010000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_MAXIMIZEBOX = 0x00010000;

        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_ACCEPTFILES = 0x00000010;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_MDICHILD = 0x00000040;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_WINDOWEDGE = 0x00000100;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_CONTEXTHELP = 0x00000400;
        public const int WS_EX_RIGHT = 0x00001000;
        public const int WS_EX_LEFT = 0x00000000;
        public const int WS_EX_RTLREADING = 0x00002000;
        public const int WS_EX_LTRREADING = 0x00000000;
        public const int WS_EX_LEFTSCROLLBAR = 0x00004000;
        public const int WS_EX_RIGHTSCROLLBAR = 0x00000000;
        public const int WS_EX_CONTROLPARENT = 0x00010000;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_APPWINDOW = 0x00040000;

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

        public const int LBS_NOTIFY = 0x0001;
        public const int LBS_SORT = 0x0002;
        public const int LBS_NOREDRAW = 0x0004;
        public const int LBS_MULTIPLESEL = 0x0008;
        public const int LBS_OWNERDRAWFIXED = 0x0010;
        public const int LBS_OWNERDRAWVARIABLE = 0x0020;
        public const int LBS_HASSTRINGS = 0x0040;
        public const int LBS_USETABSTOPS = 0x0080;
        public const int LBS_NOINTEGRALHEIGHT = 0x0100;
        public const int LBS_MULTICOLUMN = 0x0200;
        public const int LBS_WANTKEYBOARDINPUT = 0x0400;
        public const int LBS_EXTENDEDSEL = 0x0800;
        public const int LBS_DISABLENOSCROLL = 0x1000;
        public const int LBS_NODATA = 0x2000;
        public const int LBS_NOSEL = 0x4000;
        public const int LBS_COMBOBOX = 0x8000;
        public const int LBS_STANDARD = LBS_NOTIFY | LBS_SORT | WS_VSCROLL | WS_BORDER;

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

        public const int SBS_HORZ = 0x0000;
        public const int SBS_VERT = 0x0001;
        public const int SBS_TOPALIGN = 0x0002;
        public const int SBS_LEFTALIGN = 0x0002;
        public const int SBS_BOTTOMALIGN = 0x0004;
        public const int SBS_RIGHTALIGN = 0x0004;
        public const int SBS_SIZEBOXTOPLEFTALIGN = 0x0002;
        public const int SBS_SIZEBOXBOTTOMRIGHTALIGN = 0x0004;
        public const int SBS_SIZEBOX = 0x0008;
        public const int SBS_SIZEGRIP = 0x0010;

        public const int TCS_SCROLLOPPOSITE = 0x0001;   // assumes multiline tab
        public const int TCS_BOTTOM = 0x0002;
        public const int TCS_RIGHT = 0x0002;
        public const int TCS_MULTISELECT = 0x0004;  // allow multi-select in button mode
        public const int TCS_FLATBUTTONS = 0x0008;
        public const int TCS_FORCEICONLEFT = 0x0010;
        public const int TCS_FORCELABELLEFT = 0x0020;
        public const int TCS_HOTTRACK = 0x0040;
        public const int TCS_VERTICAL = 0x0080;
        public const int TCS_TABS = 0x0000;
        public const int TCS_BUTTONS = 0x0100;
        public const int TCS_SINGLELINE = 0x0000;
        public const int TCS_MULTILINE = 0x0200;
        public const int TCS_RIGHTJUSTIFY = 0x0000;
        public const int TCS_FIXEDWIDTH = 0x0400;
        public const int TCS_RAGGEDRIGHT = 0x0800;
        public const int TCS_FOCUSONBUTTONDOWN = 0x1000;
        public const int TCS_OWNERDRAWFIXED = 0x2000;
        public const int TCS_TOOLTIPS = 0x4000;
        public const int TCS_FOCUSNEVER = 0x8000;

        public const int TCS_EX_FLATSEPARATORS = 0x00000001;
        public const int TCS_EX_REGISTERDROP = 0x00000002;

        public const int TBS_AUTOTICKS = 0x0001;
        public const int TBS_VERT = 0x0002;
        public const int TBS_HORZ = 0x0000;
        public const int TBS_TOP = 0x0004;
        public const int TBS_BOTTOM = 0x0000;
        public const int TBS_LEFT = 0x0004;
        public const int TBS_RIGHT = 0x0000;
        public const int TBS_BOTH = 0x0008;
        public const int TBS_NOTICKS = 0x0010;
        public const int TBS_ENABLESELRANGE = 0x0020;
        public const int TBS_FIXEDLENGTH = 0x0040;
        public const int TBS_NOTHUMB = 0x0080;
        public const int TBS_TOOLTIPS = 0x0100;
        public const int TBS_REVERSED = 0x0200;

        public const int TBM_SETSELSTART = WM_USER + 11;
        public const int TBM_SETSELEND = WM_USER + 12;

        public const int PBS_VERTICAL = 0x4;
        public const int PBM_SETSTATE = WM_USER + 16;
        public const int PBM_GETSTATE = WM_USER + 17;

        public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public static IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        public static int ColorToCOLORREF(Color color)
        {
            return ((color.R | (color.G << 8)) | (color.B << 0x10));
        }

        public static void SetWindowStyle(Control control, int flag, bool value)
        {
            int styleFlags = unchecked((int)(long)GetWindowLong(
                new HandleRef(control, control.Handle), GWL_STYLE));

            styleFlags = value ? styleFlags | flag : styleFlags & ~flag;

            SetWindowLong(
                new HandleRef(control, control.Handle), GWL_STYLE, (IntPtr)styleFlags);
        }

        public static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr(GetWindowLongPtr32(hWnd, nIndex));
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong.ToInt32());
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLongPtr32(HandleRef hWnd, int nIndex);

        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);

        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern uint GetBkColor(IntPtr hDC);

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern uint SetBkColor(IntPtr hDC, uint clr);

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkMode(HandleRef hDC, [MarshalAs(UnmanagedType.I4)] BkMode nBkMode);

        [DllImport("user32")]
        public static extern IntPtr WindowFromDC(IntPtr hdc);

        [DllImport("user32", SetLastError = true)]
        public static extern int GetClassName(
            IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public static string GetClassName(IntPtr hWnd)
        {
            var buffer = new StringBuilder(256);
            if (GetClassName(hWnd, buffer, buffer.Capacity) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return buffer.ToString();
        }

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr data);

        public delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool EnumChildWindows(
            IntPtr hWnd, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool EnumThreadWindows(
            int dwThreadId, EnumChildProc lpfn, IntPtr lParam);

        public static IEnumerable<IntPtr> EnumerateProcessWindows()
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
                EnumThreadWindows(thread.Id, (hWnd, lParam) => {
                    handles.Add(hWnd);
                    return true;
                }, IntPtr.Zero);

            return handles;
        }

        public static IntPtr GetProp(this Control control, string str)
        {
            return GetProp(control.Handle, str);
        }

        public static bool SetProp(this Control control, string str, IntPtr data)
        {
            return SetProp(control.Handle, str, data);
        }

        public static string GetRealClassName(IntPtr hwnd)
        {
            var name = GetClassName(hwnd);
            if (name != null && name.StartsWith("WindowsForms")) {
                var parts = name.Split('.');
                if (parts.Length >= 5)
                    return parts[1];
            }

            return name;
        }

        [DllImport("user32")]
        public static extern unsafe bool IntersectRect(
            out RECT lprcDst, RECT* lprcSrc1, RECT* lprcSrc2);

        [DllImport("gdi")]
        public static extern unsafe bool ExtTextOut(
            IntPtr hdc, int X, int Y, uint fuOptions, ref RECT lprc,
            string lpString, uint cbCount, int* lpDx);
    }

    public enum ProgressBarState
    {
        Normal = 1,
        Error = 2,
        Paused = 3,
    }

    enum BkMode
    {
        Opaque = 0,
        Transparent = 1,
    }
}
