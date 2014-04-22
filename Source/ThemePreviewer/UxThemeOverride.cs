namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    //using EasyHook;
    using StyleCore;
    using StyleCore.Native;

    public static class UxThemeOverrideExtensions
    {
        public static bool OverrideTheme(this Control control)
        {
            return control.SetProp("OverrideTheme", new IntPtr(1));
        }
    }

    public unsafe class UxThemeOverride
    {
        private readonly List<SafeThemeHandle> themeHandles =
            new List<SafeThemeHandle>();
        private readonly Dictionary<IntPtr, string> classNameMap =
            new Dictionary<IntPtr, string>();
        private readonly Dictionary<IntPtr, CRenderObj> renderEntries =
            new Dictionary<IntPtr, CRenderObj>();

        private ThemeFile theme;

        //private LocalHook hookDrawThemeBackground;
        //private LocalHook hookBeginPaint;
        //private LocalHook hookEndPaint;

        public void SetTheme(ThemeFile newTheme)
        {
            theme = newTheme;
            renderEntries.Clear();
            themeHandles.Clear();
            classNameMap.Clear();

            if (theme != null) {
                UpdateMapping();
                UxOverrideTheme(newTheme.FilePath).ThrowIfFailed();
                UxHook().ThrowIfFailed();
            } else {
                UxUnhook().ThrowIfFailed();
            }
        }

        private void UpdateMapping()
        {
            foreach (var handle in themeHandles)
                handle.Dispose();

            themeHandles.Clear();
            classNameMap.Clear();

            var classNames = new[] {
                "Edit",
                "Button",
                "ListBox",
                "ListView",
                "TreeView",
                "Combobox",
                "Progress",
                "Indeterminate::Progress",
                "Tab",
                "Menu",
                "ScrollBar",
                "Trackbar",
            };
            foreach (var className in classNames) {
                var handle = StyleNativeMethods.OpenThemeData(IntPtr.Zero, className);
                themeHandles.Add(handle);
                classNameMap[handle.DangerousGetHandle()] = className;
            }
        }

        public void Hook(int threadId = 0)
        {
            //hookDrawThemeBackground = LocalHook.Create(
            //    LocalHook.GetProcAddress("uxtheme.dll", "DrawThemeBackground"),
            //    new DrawThemeBackgroundDelegate(DrawThemeBackgroundHook),
            //    null);
            //hookBeginPaint = LocalHook.Create(
            //    LocalHook.GetProcAddress("win32u.dll", "NtUserBeginPaint"),
            //    new BeginPaintDelegate(BeginPaintHook),
            //    null);
            //hookEndPaint = LocalHook.Create(
            //    LocalHook.GetProcAddress("win32u.dll", "NtUserEndPaint"),
            //    new EndPaintDelegate(EndPaintHook),
            //    null);

            //var acl = new[] { threadId };
            //hookDrawThemeBackground.ThreadACL.SetInclusiveACL(acl);
            //hookBeginPaint.ThreadACL.SetInclusiveACL(acl);
            //hookEndPaint.ThreadACL.SetInclusiveACL(acl);
        }

        public void Unhook()
        {
            //hookDrawThemeBackground.Dispose();
            //hookBeginPaint.Dispose();
            //hookEndPaint.Dispose();
            //hookDrawThemeBackground = null;
            //hookBeginPaint = null;
            //hookEndPaint = null;
        }

        [DllImport("uxtheme")]
        private static extern HResult DrawThemeBackground(
            IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            RECT* pRect,
            RECT* pClipRect);

        [DllImport("win32u", EntryPoint = "NtUserBeginPaint")]
        private static extern IntPtr BeginPaint(IntPtr hwnd, PAINTSTRUCT* lpPaint);

        [DllImport("win32u", EntryPoint = "NtUserEndPaint")]
        private static extern IntPtr EndPaint(IntPtr hwnd, IntPtr lpPaint);

        [DllImport("UxThemeEx")]
        private static extern HResult UxHook();

        [DllImport("UxThemeEx")]
        private static extern HResult UxUnhook();

        [DllImport("UxThemeEx", CharSet = CharSet.Unicode)]
        private static extern HResult UxOverrideTheme(string themeFileName);

        private delegate HResult DrawThemeBackgroundDelegate(
            IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            RECT* pRect,
            RECT* pClipRect);
        private delegate IntPtr BeginPaintDelegate(IntPtr hwnd, PAINTSTRUCT* lpPaint);
        private delegate IntPtr EndPaintDelegate(IntPtr hwnd, IntPtr lpPaint);

        private string overrideTheme;

        [StructLayout(LayoutKind.Sequential)]
        struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public int fErase;
            public RECT rcPaint;
            public int fRestore;
            public int fIncUpdate;
            public fixed byte rgbReserved[32];
        }

        private IntPtr BeginPaintHook(IntPtr hwnd, PAINTSTRUCT* lpPaint)
        {
            if (NativeMethods.GetProp(hwnd, "OverrideTheme") == new IntPtr(1)) {
                overrideTheme = NativeMethods.GetRealClassName(hwnd);
                return BeginPaint(hwnd, lpPaint);
            }

            return BeginPaint(hwnd, lpPaint);
        }

        private IntPtr EndPaintHook(IntPtr hwnd, IntPtr lpPaint)
        {
            overrideTheme = null;
            return EndPaint(hwnd, lpPaint);
        }

        private HResult DrawThemeBackgroundHook(
            IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            RECT* pRect,
            RECT* pClipRect)
        {

            string className = GetClassNameFromThemeData(hTheme);
            if (className != null) {
                // && overrideHdc == hdc
                CRenderObj renderObj;
                if (!renderEntries.TryGetValue(hTheme, out renderObj)) {
                    var cls = theme.FindClass(className);
                    if (cls != null)
                        renderEntries[hTheme] = renderObj = new CRenderObj(cls);
                }

                if (renderObj != null)
                    return renderObj.Draw(hdc, iPartId, iStateId, pRect, pClipRect);
            }

            return DrawThemeBackground(hTheme, hdc, iPartId, iStateId, pRect, pClipRect);
        }

        private string GetClassNameFromThemeData(IntPtr hTheme)
        {
            string className;
            classNameMap.TryGetValue(hTheme, out className);
            return className;
        }
    }

    [Flags]
    internal enum DTBGF : uint
    {
        DTBG_CLIPRECT = 1, // rcClip has been specified
        DTBG_DRAWSOLID = 2, // DEPRECATED: draw transparent/alpha images as solid
        DTBG_OMITBORDER = 4, // don't draw border of part
        DTBG_OMITCONTENT = 8, // don't draw content area of part
        DTBG_COMPUTINGREGION = 16, // TRUE if calling to compute region
        DTBG_MIRRORDC = 32, // assume the hdc is mirrorred and
        DTBG_NOMIRROR = 64, // don't mirror the output, overrides everything else 
        DTBG_VALIDBITS = (
            DTBG_CLIPRECT |
            DTBG_DRAWSOLID |
            DTBG_OMITBORDER |
            DTBG_OMITCONTENT |
            DTBG_COMPUTINGREGION |
            DTBG_MIRRORDC |
            DTBG_NOMIRROR)
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DTBGOPTS
    {
        public uint dwSize;
        [MarshalAs(UnmanagedType.U4)]
        public DTBGF dwFlags;
        public RECT rcClip;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HBITMAP__
    {
        int unused;
    };

    [StructLayout(LayoutKind.Explicit)]
    unsafe struct HBITMAP64
    {
        [FieldOffset(0)]
        public HBITMAP__* hBitmap;
        [FieldOffset(0)]
        public void* hBitmap64;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DIBINFO
    {
        public HBITMAP64 uhbm;
        public int iDibOffset;
        public int iSingleWidth;
        public int iSingleHeight;
        public int iRgnListOffset;
        public SIZINGTYPE eSizingType;
        public bool fBorderOnly;
        public bool fPartiallyTransparent;
        public int iAlphaThreshold;
        public int iMinDpi;
        public SIZE szMinSize;
    };

    internal class CStateIdObjectCache
    {
        public List<CDrawBase> DrawObjs = new List<CDrawBase>();
        public List<CDrawBase> TextObjs = new List<CDrawBase>();

        public void Expand(int cStates)
        {
            while (DrawObjs.Count <= cStates)
                DrawObjs.Add(null);
        }
    }

    internal abstract class CDrawBase
    {
        public BGTYPE _eBgType;
        protected int _iUnused;
    }

    internal unsafe class CBorderFill : CDrawBase
    {
        private bool _fNoDraw;
        private BORDERTYPE _eBorderType;
        private uint _crBorder;
        private int _iBorderSize;
        private int _iRoundCornerWidth;
        private int _iRoundCornerHeight;
        private FILLTYPE _eFillType;
        private uint _crFill;
        private int _iDibOffset;
        private MARGINS _ContentMargins;
        private int _iGradientPartCount;
        private uint[] _crGradientColors = new uint[5];
        private int[] _iGradientRatios = new int[5];
        private int _iSourcePartId;
        private int _iSourceStateId;

        public HResult DrawBackground(
            CRenderObj pRender, IntPtr hdcOrig, RECT* pRect, DTBGOPTS* pOptions)
        {
            RECT* clipRect = null;
            bool drawContent = true;
            bool drawBorder = true;
            bool computeRegion = false;
            if (pOptions != null) {
                var flags = pOptions->dwFlags;
                if ((flags & DTBGF.DTBG_CLIPRECT) != 0)
                    clipRect = &pOptions->rcClip;
                if ((flags & DTBGF.DTBG_OMITBORDER) != 0)
                    drawBorder = false;
                if ((flags & DTBGF.DTBG_OMITCONTENT) != 0)
                    drawContent = false;
                if ((flags & DTBGF.DTBG_COMPUTINGREGION) != 0)
                    computeRegion = true;
            }

            if (_fNoDraw)
                return HResult.OK;

            var rcDst = new RECT();
            var hr = HResult.OK;

            uint prevBkColor;
            if (_eFillType != 0 || _eBorderType != 0)
                return hr;
                //return DrawComplexBackground(
                //    pRender,
                //    hdcOrig,
                //    pRect,
                //    (HRGN__*)clipRect,
                //    computeRegion,
                //    drawBorder,
                //    drawContent,
                //    clipRect);

            if (_iBorderSize != 0) {
                var v19 = NativeMethods.GetBkColor(hdcOrig);
                if (drawBorder) {
                    NativeMethods.SetBkColor(hdcOrig, _crBorder);
                    rcDst.SetRect(pRect->left, pRect->top, pRect->left + _iBorderSize, pRect->bottom);
                    if (clipRect != null)
                        NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                    NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2u, ref rcDst, null, 0, null);
                    rcDst.SetRect(pRect->right - _iBorderSize, pRect->top, pRect->right, pRect->bottom);
                    if (clipRect != null)
                        NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                    NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2u, ref rcDst, null, 0, null);
                    rcDst.SetRect(pRect->left, pRect->top, pRect->right, pRect->top + _iBorderSize);
                    if (clipRect != null)
                        NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                    NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2u, ref rcDst, null, 0, null);
                    rcDst.SetRect(pRect->left, pRect->bottom - _iBorderSize, pRect->right, pRect->bottom);
                    if (clipRect != null)
                        NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                    NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2u, ref rcDst, null, 0, null);
                }

                if (drawContent) {
                    rcDst = *pRect;
                    var v17 = rcDst;
                    rcDst.right -= _iBorderSize;
                    rcDst.top += _iBorderSize;
                    rcDst.bottom -= _iBorderSize;
                    rcDst.left = _iBorderSize + v17.left;
                    if (clipRect != null)
                        NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                    NativeMethods.SetBkColor(hdcOrig, _crFill);
                    NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2, ref rcDst, null, 0, null);
                }
                prevBkColor = v19;
            } else {
                if (!drawContent)
                    return hr;
                rcDst = *pRect;
                if (clipRect != null)
                    NativeMethods.IntersectRect(out rcDst, &rcDst, clipRect);
                prevBkColor = NativeMethods.SetBkColor(hdcOrig, _crFill);
                NativeMethods.ExtTextOut(hdcOrig, 0, 0, 2u, ref rcDst, null, 0, null);
            }

            NativeMethods.SetBkColor(hdcOrig, prevBkColor);
            return hr;
        }

        public HResult PackProperties(CRenderObj pRender, bool fNoDraw, int iPartId, int iStateId)
        {
            _iSourcePartId = iPartId;
            _eBgType = BGTYPE.BT_BORDERFILL;
            _iSourceStateId = iStateId;

            if (fNoDraw) {
                _fNoDraw = true;
                return HResult.OK;
            }

            if (pRender.ExternalGetEnumValue(iPartId, iStateId, TMT.BORDERTYPE, out _eBorderType) < 0)
                _eBorderType = 0;
            if (pRender.ExternalGetInt(iPartId, iStateId, TMT.BORDERCOLOR, out _crBorder) < 0)
                _crBorder = 0;
            if (pRender.ExternalGetInt(iPartId, iStateId, TMT.BORDERSIZE, out _iBorderSize) < 0)
                _iBorderSize = 1;

            if (_eBorderType == BORDERTYPE.BT_ROUNDRECT) {
                if (pRender.ExternalGetInt(iPartId, iStateId, TMT.ROUNDCORNERWIDTH, out _iRoundCornerWidth) < 0)
                    _iRoundCornerWidth = 80;
                if (pRender.ExternalGetInt(iPartId, iStateId, TMT.ROUNDCORNERHEIGHT, out _iRoundCornerHeight) < 0)
                    _iRoundCornerHeight = 80;
            }

            if (pRender.ExternalGetEnumValue(iPartId, iStateId, TMT.FILLTYPE, out _eFillType) < 0)
                _eFillType = 0;

            switch (_eFillType) {
                case FILLTYPE.FT_SOLID:
                    if (pRender.ExternalGetInt(iPartId, iStateId, TMT.FILLCOLOR, out _crFill) < 0)
                        _crFill = 0xFFFFFF;
                    break;
                case FILLTYPE.FT_TILEIMAGE:
                    _iDibOffset = pRender.GetValueIndex(iPartId, iStateId, 2);
                    if (_iDibOffset == -1)
                        _iDibOffset = 0;
                    break;
                default:
                    _iGradientPartCount = 0;
                    for (TMT prop = TMT.GRADIENTRATIO1; prop < TMT.GRADIENTRATIO5; ++prop) {
                        uint color;
                        if (pRender.ExternalGetInt(iPartId, iStateId, prop + 1404, out color) < 0)
                            break;
                        int ratio;
                        if (pRender.ExternalGetInt(iPartId, iStateId, prop, out ratio) < 0)
                            ratio = 0;
                        _crGradientColors[_iGradientPartCount] = color;
                        _iGradientRatios[_iGradientPartCount++] = ratio;
                        ++prop;
                    }
                    break;
            }

            if (pRender.ExternalGetMargins(iPartId, iStateId, TMT.CONTENTMARGINS, out _ContentMargins) < 0) {
                _ContentMargins.cxLeftWidth = _iBorderSize;
                _ContentMargins.cxRightWidth = _iBorderSize;
                _ContentMargins.cyTopHeight = _iBorderSize;
                _ContentMargins.cyBottomHeight = _iBorderSize;
            }

            return HResult.OK;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TRUESTRETCHINFO
    {
        public int fForceStretch;
        public int fFullStretch;
        public SIZE szDrawSize;
    };

    internal unsafe class CImageFile : CDrawBase
    {
        private DIBINFO _ImageInfo;
        private DIBINFO _ScaledImageInfo;
        private int _iMultiImageCount;
        private IMAGESELECTTYPE _eImageSelectType;
        private int _iImageCount;
        private IMAGELAYOUT _eImageLayout;
        private bool _fMirrorImage;
        private TRUESIZESCALINGTYPE _eTrueSizeScalingType;
        private HALIGN _eHAlign;
        private VALIGN _eVAlign;
        private bool _fBgFill;
        private uint _crFill;
        private int _iTrueSizeStretchMark;
        private bool _fUniformSizing;
        private bool _fIntegralSizing;
        private MARGINS _SizingMargins;
        private MARGINS _ContentMargins;
        private bool _fSourceGrow;
        private bool _fSourceShrink;
        private bool _fGlyphOnly;
        private GLYPHTYPE _eGlyphType;
        private uint _crGlyphTextColor;
        private ushort _iGlyphFontIndex;
        private int _iGlyphIndex;
        private DIBINFO _GlyphInfo;
        private int _iSourcePartId;
        private int _iSourceStateId;
        private Bitmap bitmap;
        private ThemeBitmap imageFile;

        public HResult DrawBackground(
            CRenderObj pRender, IntPtr hdc, int iStateId, RECT* pRect, DTBGOPTS* pOptions)
        {
            if (bitmap == null || iStateId < 0 || iStateId > _iImageCount)
                return HResult.Failed;

            using (var graphics = Graphics.FromHdc(hdc)) {
                // A---B----------C---D
                // |   |          |   |
                // E---F----------G---H
                // |   |          |   |
                // |   |          |   |
                // |   |          |   |
                // I---J----------K---L
                // |   |          |   |
                // M---N----------O---P
                var ml = _SizingMargins.cxLeftWidth;
                var mr = _SizingMargins.cxRightWidth;
                var mt = _SizingMargins.cyTopHeight;
                var mb = _SizingMargins.cyBottomHeight;

                var srcW = bitmap.Width;
                var srcH = bitmap.Height / _iImageCount;
                var srcY = (iStateId - 1) * srcH;
                var srca = new Point(0, srcY + 0);
                var srcb = new Point(ml, srcY + 0);
                var srcc = new Point(srcW - mr, srcY + 0);
                var srcd = new Point(srcW, srcY + 0);
                var srce = new Point(0, srcY + mt);
                var srcf = new Point(ml, srcY + mt);
                var srcg = new Point(srcW - mr, srcY + mt);
                var srch = new Point(srcW, srcY + mt);
                var srci = new Point(0, srcY + srcH - mb);
                var srcj = new Point(ml, srcY + srcH - mb);
                var srck = new Point(srcW - mr, srcY + srcH - mb);
                var srcl = new Point(srcW, srcY + srcH - mb);
                var srcm = new Point(0, srcY + srcH);
                var srcn = new Point(ml, srcY + srcH);
                var srco = new Point(srcW - mr, srcY + srcH);
                var srcp = new Point(srcW, srcY + srcH);

                var dstW = pRect->right - pRect->left;
                var dstH = pRect->bottom - pRect->top;
                var dstX = pRect->left;
                var dstY = pRect->top;
                var dsta = new Point(dstX + 0, dstY + 0);
                var dstb = new Point(dstX + ml, dstY + 0);
                var dstc = new Point(dstX + dstW - mr, dstY + 0);
                var dstd = new Point(dstX + dstW, dstY + 0);
                var dste = new Point(dstX + 0, dstY + mt);
                var dstf = new Point(dstX + ml, dstY + mt);
                var dstg = new Point(dstX + dstW - mr, dstY + mt);
                var dsth = new Point(dstX + dstW, dstY + mt);
                var dsti = new Point(dstX + 0, dstY + dstH - mb);
                var dstj = new Point(dstX + ml, dstY + dstH - mb);
                var dstk = new Point(dstX + dstW - mr, dstY + dstH - mb);
                var dstl = new Point(dstX + dstW, dstY + dstH - mb);
                var dstm = new Point(dstX + 0, dstY + dstH);
                var dstn = new Point(dstX + ml, dstY + dstH);
                var dsto = new Point(dstX + dstW - mr, dstY + dstH);
                var dstp = new Point(dstX + dstW, dstY + dstH);

                var srccw = srcW - mr - ml;
                var srcch = srcH - mb - mt;

                var srcSizeTL = new Size(ml, mt);
                var srcSizeTR = new Size(mr, mt);
                var srcSizeBL = new Size(ml, mb);
                var srcSizeBR = new Size(mr, mb);
                var srcSizeL = new Size(ml, srcch);
                var srcSizeR = new Size(mr, srcch);
                var srcSizeT = new Size(srccw, mt);
                var srcSizeB = new Size(srccw, mb);
                var srcSizeC = new Size(srccw, srcch);

                var srcTL = new Rectangle(srca, srcSizeTL);
                var srcTR = new Rectangle(srcc, srcSizeTR);
                var srcBL = new Rectangle(srci, srcSizeBL);
                var srcBR = new Rectangle(srck, srcSizeBR);
                var srcL = new Rectangle(srce, srcSizeL);
                var srcR = new Rectangle(srcg, srcSizeR);
                var srcT = new Rectangle(srcb, srcSizeT);
                var srcB = new Rectangle(srcj, srcSizeB);
                var srcC = new Rectangle(srcf, srcSizeC);

                var dstcw = Math.Max(dstW - mr - ml, 0);
                var dstch = Math.Max(dstH - mb - mt, 0);

                var dstSizeTL = new Size(ml, mt);
                var dstSizeTR = new Size(mr, mt);
                var dstSizeBL = new Size(ml, mb);
                var dstSizeBR = new Size(mr, mb);
                var dstSizeL = new Size(ml, dstch);
                var dstSizeR = new Size(mr, dstch);
                var dstSizeT = new Size(dstcw, mt);
                var dstSizeB = new Size(dstcw, mb);
                var dstSizeC = new Size(dstcw, dstch);

                var dstTL = new Rectangle(dsta, dstSizeTL);
                var dstTR = new Rectangle(dstc, dstSizeTR);
                var dstBL = new Rectangle(dsti, dstSizeBL);
                var dstBR = new Rectangle(dstk, dstSizeBR);
                var dstL = new Rectangle(dste, dstSizeL);
                var dstR = new Rectangle(dstg, dstSizeR);
                var dstT = new Rectangle(dstb, dstSizeT);
                var dstB = new Rectangle(dstj, dstSizeB);
                var dstC = new Rectangle(dstf, dstSizeC);

                // TopLeft
                graphics.DrawImage(bitmap, dstTL, srcTL, GraphicsUnit.Pixel);

                // TopRight
                graphics.DrawImage(bitmap, dstTR, srcTR, GraphicsUnit.Pixel);

                // BottomLeft
                graphics.DrawImage(bitmap, dstBL, srcBL, GraphicsUnit.Pixel);

                // BottomRight
                graphics.DrawImage(bitmap, dstBR, srcBR, GraphicsUnit.Pixel);

                // Left
                graphics.DrawImage(bitmap, dstL, srcL, GraphicsUnit.Pixel);

                // Right
                graphics.DrawImage(bitmap, dstR, srcR, GraphicsUnit.Pixel);

                // Top
                graphics.DrawImage(bitmap, dstT, srcT, GraphicsUnit.Pixel);

                // Bottom
                graphics.DrawImage(bitmap, dstB, srcB, GraphicsUnit.Pixel);

                // Center
                graphics.DrawImage(bitmap, dstC, srcC, GraphicsUnit.Pixel);
            }

            return 0;
        }

        public void PackProperties(CRenderObj pRender, int iPartId, int iStateId)
        {
            if (pRender.ExternalGetBitmap(iPartId, iStateId, TMT.IMAGEFILE, out imageFile) >= 0)
                using (var stream = imageFile.OpenStream())
                    bitmap = new Bitmap(stream);

            _eBgType = 0;
            _iSourcePartId = iPartId;
            _iSourceStateId = iStateId;
            _ImageInfo.iMinDpi = 96;
            _ImageInfo.iDibOffset = pRender.GetValueIndex(iPartId, iStateId, 2);
            if (_ImageInfo.iDibOffset == -1)
                _ImageInfo.iDibOffset = 0;

            if (pRender.ExternalGetInt(iPartId, iStateId, TMT.IMAGECOUNT, out _iImageCount) < 0)
                _iImageCount = 1;
            if (_iImageCount < 1)
                _iImageCount = 1;

            if (pRender.ExternalGetEnumValue(
                iPartId, iStateId, TMT.IMAGELAYOUT, out _eImageLayout) < 0)
                _eImageLayout = IMAGELAYOUT.IL_HORIZONTAL;

            if (_ImageInfo.iDibOffset != 0
                && SetImageInfo(ref _ImageInfo, pRender, iPartId, iStateId) < 0)
                return;

            if (pRender.ExternalGetPosition(
                    iPartId,
                    iStateId,
                    TMT.MINSIZE,
                    out _ImageInfo.szMinSize) >= 0) {
                CRenderObj.AdjustSizeMin(ref _ImageInfo.szMinSize, 1, 1);
            } else {
                _ImageInfo.szMinSize.cx = _ImageInfo.iSingleWidth;
                _ImageInfo.szMinSize.cy = _ImageInfo.iSingleHeight;
            }

            if (pRender.ExternalGetEnumValue(
                    iPartId,
                    iStateId,
                    TMT.TRUESIZESCALINGTYPE,
                    out _eTrueSizeScalingType) < 0)
                _eTrueSizeScalingType = 0;

            if (pRender.ExternalGetEnumValue(
                    iPartId,
                    iStateId,
                    TMT.SIZINGTYPE,
                    out _ImageInfo.eSizingType) < 0)
                _ImageInfo.eSizingType = SIZINGTYPE.ST_STRETCH;

            if (pRender.ExternalGetBool(
                    iPartId,
                    iStateId,
                    TMT.BORDERONLY,
                    out _ImageInfo.fBorderOnly) < 0)
                _ImageInfo.fBorderOnly = false;

            if (pRender.ExternalGetInt(
                    iPartId,
                    iStateId,
                    TMT.TRUESIZESTRETCHMARK,
                    out _iTrueSizeStretchMark) < 0)
                _iTrueSizeStretchMark = 0;

            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.UNIFORMSIZING, out _fUniformSizing) < 0)
                _fUniformSizing = false;

            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.INTEGRALSIZING, out _fIntegralSizing) < 0)
                _fIntegralSizing = false;

            pRender.ExternalGetBool(iPartId, iStateId, TMT.TRANSPARENT, out _ImageInfo.fPartiallyTransparent);

            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.MIRRORIMAGE, out _fMirrorImage) < 0)
                _fMirrorImage = true;

            if (pRender.ExternalGetEnumValue(iPartId, iStateId, TMT.HALIGN, out _eHAlign) < 0)
                _eHAlign = HALIGN.HA_CENTER;

            if (pRender.ExternalGetEnumValue(iPartId, iStateId, TMT.VALIGN, out _eVAlign) < 0)
                _eVAlign = VALIGN.VA_CENTER;

            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.BGFILL, out _fBgFill) >= 0
                && pRender.ExternalGetInt(iPartId, iStateId, TMT.FILLCOLOR, out _crFill) < 0) {
                _crFill = 0xFFFFFF;
            }

            if (pRender.ExternalGetMargins(iPartId, iStateId, TMT.SIZINGMARGINS, out _SizingMargins) < 0)
                _SizingMargins = new MARGINS();

            if (pRender.ExternalGetMargins(
                    iPartId,
                    iStateId,
                    TMT.CONTENTMARGINS,
                    out _ContentMargins) < 0)
                _ContentMargins = _SizingMargins;

            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.SOURCEGROW, out _fSourceGrow) < 0)
                _fSourceGrow = false;
            if (pRender.ExternalGetBool(iPartId, iStateId, TMT.SOURCESHRINK, out _fSourceShrink) < 0)
                _fSourceShrink = false;

            if (pRender.ExternalGetEnumValue(
                    iPartId,
                    iStateId,
                    TMT.GLYPHTYPE,
                    out _eGlyphType) < 0)
                _eGlyphType = 0;

            switch (_eGlyphType) {
                case GLYPHTYPE.GT_IMAGEGLYPH:
                    _GlyphInfo.iMinDpi = 96;
                    _GlyphInfo.iDibOffset = pRender.GetValueIndex(iPartId, iStateId, 8);
                    if (_GlyphInfo.iDibOffset == -1)
                        _GlyphInfo.iDibOffset = 0;
                    if (_GlyphInfo.iDibOffset > 0) {
                        if (SetImageInfo(ref _GlyphInfo, pRender, iPartId, iStateId) < 0)
                            return;
                    }

                    pRender.ExternalGetBool(
                        iPartId,
                        iStateId,
                        TMT.GLYPHTRANSPARENT,
                        out _GlyphInfo.fPartiallyTransparent);
                    _GlyphInfo.eSizingType = 0;
                    _GlyphInfo.fBorderOnly = false;
                    break;
                case GLYPHTYPE.GT_FONTGLYPH:
                    if (pRender.GetFontTableIndex(iPartId, iStateId, TMT.GLYPHFONT, out _iGlyphFontIndex) != 0)
                        return;

                    if (pRender.ExternalGetInt(
                            iPartId,
                            iStateId,
                            TMT.GLYPHTEXTCOLOR,
                            out _crGlyphTextColor) < 0)
                        _crGlyphTextColor = 0;
                    if (pRender.ExternalGetInt(iPartId, iStateId, TMT.GLYPHINDEX, out _iGlyphIndex) < 0)
                        _iGlyphIndex = 1;
                    break;
            }

            if (_eGlyphType != 0 && pRender.ExternalGetBool(
                    iPartId, iStateId, TMT.GLYPHONLY, out _fGlyphOnly) < 0)
                _fGlyphOnly = false;

            if (pRender.ExternalGetEnumValue(
                    iPartId,
                    iStateId,
                    TMT.IMAGESELECTTYPE,
                    out _eImageSelectType) < 0)
                _eImageSelectType = 0;

            //if (_eImageSelectType != 0) {
            //    if (_eGlyphType == GLYPHTYPE.GT_IMAGEGLYPH)
            //        pImageInfo = &_GlyphInfo;

            //    v19 = 0;
            //    do {
            //        if (v19 != 0) {
            //            switch (v19) {
            //                case 1:
            //                    v20 = 4;
            //                    break;
            //                case 2:
            //                    v20 = 5;
            //                    break;
            //                case 3:
            //                    v20 = 6;
            //                    break;
            //                case 4:
            //                    v20 = 7;
            //                    break;
            //                case 5:
            //                    v20 = 22;
            //                    break;
            //                default:
            //                    if (v19 != 6)
            //                        NT_ASSERT("FRE: FALSE");
            //                    v20 = 23;
            //                    break;
            //            }
            //        } else {
            //            v20 = 3;
            //        }
            //        if ((uint)pRender.GetValueIndex(iPartId, iStateId, v20) == -1)
            //            break;
            //        ++_iMultiImageCount;
            //        v21 = CMaxImageFile::MultiDibPtr(this, v19);
            //        *(_OWORD*)&v21->uhbm.hBitmap = *(_OWORD*)&pImageInfo->uhbm.hBitmap;
            //        *(_OWORD*)&v21->iSingleHeight = *(_OWORD*)&pImageInfo->iSingleHeight;
            //        *(_OWORD*)&v21->fPartiallyTransparent = *(_OWORD*)&pImageInfo->fPartiallyTransparent;
            //        *(_QWORD*)&v21->szMinSize.cy = *(_QWORD*)&pImageInfo->szMinSize.cy;
            //        v21->iDibOffset = v22;
            //        v32 = CImageFile::SetImageInfo(v23, v21, pRender, iPartId, iStateId);
            //        if ((v32 & 0x80000000) != 0)
            //            return v32;
            //        v24 = &v21->iMinDpi;
            //        TMT v25 = Map_Ordinal_To_MINDPI(v19);
            //        if (pRender.ExternalGetInt(iPartId, iStateId, v25, &v21->iMinDpi) >= 0) {
            //            if (*v24 < 1)
            //                *v24 = 1;
            //        } else {
            //            *v24 = 96;
            //        }
            //        v26 = Map_Ordinal_To_MINSIZE(v19);
            //        if (pRender.ExternalGetPosition(iPartId, iStateId, v26, (tagPOINT*)&v21->szMinSize) >= 0) {
            //            AdjustSizeMin(&v21->szMinSize, v27, v28);
            //        } else {
            //            v21->szMinSize.cx = v21->iSingleWidth;
            //            v21->szMinSize.cy = v21->iSingleHeight;
            //        }
            //        ++v19;
            //    } while (v19 < 7);

            //    if (_iMultiImageCount > 0) {
            //        *(_OWORD*)&pImageInfo->uhbm.hBitmap = *(_OWORD*)&this.MultiDibs[0].uhbm.hBitmap;
            //        *(_OWORD*)&pImageInfo->iSingleHeight = *(_OWORD*)&this.MultiDibs[0].iSingleHeight;
            //        *(_OWORD*)&pImageInfo->fPartiallyTransparent = *(_OWORD*)&this.MultiDibs[0].fPartiallyTransparent;
            //        *(_QWORD*)&pImageInfo->szMinSize.cy = *(_QWORD*)&this.MultiDibs[0].szMinSize.cy;
            //    }
            //}
        }

        private int SetImageInfo(
            ref DIBINFO pdi, CRenderObj pRender, int iPartId, int iStateId)
        {
            return 0;
        }
    }

    internal unsafe class CRenderObj
    {
        private readonly ThemeClass cls;
        private List<CStateIdObjectCache> parts = new List<CStateIdObjectCache>();

        public CRenderObj(ThemeClass cls)
        {
            this.cls = cls;
            Initialize();
        }

        public int GetFontTableIndex(int iPartId, int iStateId, TMT iPropId, out ushort pFontIndex)
        {
            pFontIndex = 0;
            return -1;
        }

        public int GetValueIndex(int iPartId, int iStateId, int p2)
        {
            return 0;
        }

        public static void AdjustSizeMin(ref SIZE psz, int ixMin, int iyMin)
        {
            if (psz.cx < ixMin)
                psz.cx = ixMin;
            if (psz.cy < iyMin)
                psz.cy = iyMin;
        }

        public static TMT Map_Ordinal_To_MINDPI(int i)
        {
            switch (i) {
                case 0:
                    return TMT.MINDPI1;
                case 1:
                    return TMT.MINDPI2;
                case 2:
                    return TMT.MINDPI3;
                case 3:
                    return TMT.MINDPI4;
                case 4:
                    return TMT.MINDPI5;
                case 5:
                    return TMT.MINDPI6;
                case 6:
                    return TMT.MINDPI7;
                default:
                    Debug.Assert(false, "FRE: FALSE");
                    return TMT.MINDPI1;
            }
        }

        public int ExternalGetMargins(int partId, int stateId, TMT propId, out MARGINS value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is MARGINS)) {
                value = new MARGINS();
                return -1;
            }

            value = (MARGINS)entry.Value;
            return 0;
        }

        public int ExternalGetInt(int partId, int stateId, TMT propId, out int value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is int)) {
                value = 0;
                return -1;
            }

            value = (int)entry.Value;
            return 0;
        }

        public int ExternalGetInt(int partId, int stateId, TMT propId, out uint value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is int)) {
                value = 0;
                return -1;
            }

            value = (uint)(int)entry.Value;
            return 0;
        }

        public int ExternalGetBool(int partId, int stateId, TMT propId, out bool value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is bool)) {
                value = false;
                return -1;
            }

            value = (bool)entry.Value;
            return 0;
        }

        public int ExternalGetPosition(int partId, int stateId, TMT propId, out SIZE value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is POINT)) {
                value = new SIZE();
                return -1;
            }

            var pt = (POINT)entry.Value;
            value = new SIZE { cx = pt.x, cy = pt.y };
            return 0;
        }

        public int ExternalGetEnumValue<T>(int partId, int stateId, TMT propId, out T value)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException();

            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is T)) {
                value = default(T);
                return -1;
            }

            value = (T)entry.Value;
            return 0;
        }

        public int ExternalGetBitmap(int partId, int stateId, TMT propId, out ThemeBitmap value)
        {
            var entry = EnumProperties(partId, stateId).FirstOrDefault(x => x.PropertyId == propId);
            if (!(entry?.Value is ThemeBitmap)) {
                value = default(ThemeBitmap);
                return -1;
            }

            value = (ThemeBitmap)entry.Value;
            return 0;
        }

        public IEnumerable<ThemeProperty> EnumProperties(int partId, int stateId)
        {
            var part = cls.FindPart(partId);
            var state = part?.FindState(stateId);

            var allProperties = Enumerable.Empty<ThemeProperty>();
            if (state != null)
                allProperties = allProperties.Concat(state.Properties);
            if (part != null)
                allProperties = allProperties.Concat(part.Properties);

            return allProperties.Concat(cls.Properties);
        }

        private void ExpandPartObjectCache(int cParts)
        {
            while (parts.Count <= cParts)
                parts.Add(new CStateIdObjectCache());
        }

        private HResult GetPartObject(int partId, int stateId, out CDrawBase obj)
        {
            if (partId < 0)
                partId = 0;
            if (stateId < 0)
                stateId = 0;

            obj = null;
            if (partId < parts.Count) {
                var state = parts[partId];
                if (stateId < state.DrawObjs.Count)
                    obj = state.DrawObjs[stateId];
            }

            if (obj != null)
                return 0;

            obj = FixupPartObjectCache(partId, stateId);
            if (obj != null)
                return HResult.OK;

            return HResult.Failed;
        }

        private CDrawBase FixupPartObjectCache(int partId, int stateId)
        {
            ExpandPartObjectCache(partId);
            var state = parts[partId];
            state.Expand(stateId);

            var obj = FindClassPartObject(partId, stateId);
            if (obj == null)
                obj = FindBaseClassPartObject(partId, stateId);
            state.DrawObjs[stateId] = obj;
            return obj;
        }

        private CDrawBase FindClassPartObject(int partId, int stateId)
        {
            var part = cls.FindPart(partId);
            if (part == null)
                return null;

            var state = part.FindState(stateId);
            if (state == null)
                return null;

            return null;
        }

        private CDrawBase FindBaseClassPartObject(int partId, int stateId)
        {
            return null;
        }

        private void Initialize()
        {
            ExpandPartObjectCache(cls.Parts.Count);
            foreach (var part in cls.Parts) {
                var stateCache = new CStateIdObjectCache();
                parts[part.Id] = stateCache;

                BGTYPE bgType = part.FindProperty(TMT.BGTYPE, BGTYPE.BT_BORDERFILL);

                stateCache.Expand(part.States.Count);
                foreach (var state in part.States) {
                    switch (bgType) {
                        case BGTYPE.BT_NONE:
                            var fill = new CBorderFill();
                            fill.PackProperties(this, true, part.Id, state.Id);
                            stateCache.DrawObjs[state.Id] = fill;
                            break;
                        case BGTYPE.BT_BORDERFILL:
                            var borderFill = new CBorderFill();
                            borderFill.PackProperties(this, false, part.Id, state.Id);
                            stateCache.DrawObjs[state.Id] = borderFill;
                            break;
                        case BGTYPE.BT_IMAGEFILE:
                            var imageFile = new CImageFile();
                            imageFile.PackProperties(this, part.Id, state.Id);
                            stateCache.DrawObjs[state.Id] = imageFile;
                            break;
                    }
                }
            }
        }

        public HResult Draw(IntPtr hdc, int partId, int stateId, RECT* rect, RECT* clipRect)
        {
            CDrawBase drawBase;
            var hr = GetPartObject(partId, stateId, out drawBase);
            if (hr < 0)
                return hr;

            var opts = new DTBGOPTS();
            DTBGOPTS* pOpts = null;
            if (clipRect != null) {
                opts.dwSize = (uint)Marshal.SizeOf<DTBGOPTS>();
                opts.dwFlags |= DTBGF.DTBG_CLIPRECT;
                opts.rcClip = *clipRect;
                pOpts = &opts;
            }

            if (drawBase._eBgType == BGTYPE.BT_BORDERFILL)
                return ((CBorderFill)drawBase).DrawBackground(this, hdc, rect, pOpts);
            else
                return ((CImageFile)drawBase).DrawBackground(this, hdc, stateId, rect, pOpts);
        }
    }
}