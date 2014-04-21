namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using Microsoft.Win32;

    public sealed class VisualStyleRendererEx
    {
        private readonly IntPtr controlHandle;
        private string _class;

        private const TextFormatFlags AllGraphicsProperties =
            (TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping);

        internal const int EdgeAdjust = 0x2000;
        private static long globalCacheVersion = 0L;
        private int lastHResult;
        private static int numberOfPossibleClasses = 25; //VisualStyleElement.Count
        private int part;
        private int state;

        [ThreadStatic]
        private static Dictionary<object, SafeThemeHandle> themeHandles = null;

        [ThreadStatic]
        private static long threadCacheVersion = 0L;

        static VisualStyleRendererEx()
        {
            SystemEvents.UserPreferenceChanging += OnUserPreferenceChanging;
        }

        public VisualStyleRendererEx(IntPtr controlHandle, VisualStyleElement element)
            : this(controlHandle, element.ClassName, element.Part, element.State)
        {
        }

        public VisualStyleRendererEx(IntPtr controlHandle, string className, int part, int state)
        {
            //if (!IsCombinationDefined(className, part))
            //    throw new ArgumentException("VisualStylesInvalidCombination");
            this.controlHandle = controlHandle;
            this._class = className;
            this.part = part;
            this.state = state;
        }

        private static void CreateThemeHandleHashtable()
        {
            themeHandles = new Dictionary<object, SafeThemeHandle>(numberOfPossibleClasses);
        }

        public void DrawBackground(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if ((bounds.Width >= 0) && (bounds.Height >= 0)) {
                using (
                    WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                        dc,
                        TextFormatFlags.PreserveGraphicsTranslateTransform |
                        TextFormatFlags.PreserveGraphicsClipping)) {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult =
                        VisualStylesNativeMethods.DrawThemeBackground(
                            new HandleRef(this, this.Handle),
                            hdc,
                            this.part,
                            this.state,
                            new RECT(bounds),
                            null);
                }
            }
        }

        public void DrawBackground(IDeviceContext dc, Rectangle bounds, Rectangle clipRectangle)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (((bounds.Width >= 0) && (bounds.Height >= 0)) &&
                ((clipRectangle.Width >= 0) && (clipRectangle.Height >= 0))) {
                using (
                    WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                        dc,
                        TextFormatFlags.PreserveGraphicsTranslateTransform |
                        TextFormatFlags.PreserveGraphicsClipping)) {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult =
                        VisualStylesNativeMethods.DrawThemeBackground(
                            new HandleRef(this, this.Handle),
                            hdc,
                            this.part,
                            this.state,
                            new RECT(bounds),
                            new RECT(clipRectangle));
                }
            }
        }

        public Rectangle DrawEdge(
            IDeviceContext dc,
            Rectangle bounds,
            Edges edges,
            EdgeStyle style,
            EdgeEffects effects)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (!ClientUtils.IsEnumValid_Masked(edges, (int)edges, 0x1f))
                throw new InvalidEnumArgumentException("edges", (int)edges, typeof(Edges));
            if (!ClientUtils.IsEnumValid_NotSequential(style, (int)style, new int[] { 5, 10, 6, 9 }))
                throw new InvalidEnumArgumentException("style", (int)style, typeof(EdgeStyle));
            if (!ClientUtils.IsEnumValid_Masked(effects, (int)effects, 0xd800))
                throw new InvalidEnumArgumentException("effects", (int)effects, typeof(EdgeEffects));
            RECT pContentRect = new RECT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = VisualStylesNativeMethods.DrawThemeEdge(
                    new HandleRef(this, this.Handle),
                    hdc,
                    this.part,
                    this.state,
                    new RECT(bounds),
                    (int)style,
                    ((int)(edges | ((Edges)((int)effects)))) | 0x2000,
                    pContentRect);
            }
            return Rectangle.FromLTRB(
                pContentRect.left,
                pContentRect.top,
                pContentRect.right,
                pContentRect.bottom);
        }

        public void DrawImage(Graphics g, Rectangle bounds, Image image)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            if (image == null)
                throw new ArgumentNullException("image");
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
                g.DrawImage(image, bounds);
        }

        public void DrawImage(Graphics g, Rectangle bounds, ImageList imageList, int imageIndex)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            if (imageList == null)
                throw new ArgumentNullException("imageList");
            if ((imageIndex < 0) || (imageIndex >= imageList.Images.Count)) {
                throw new ArgumentOutOfRangeException(
                    "imageIndex",
                    string.Format(
                        "InvalidArgument",
                        new object[] { "imageIndex", imageIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
                g.DrawImage(imageList.Images[imageIndex], bounds);
        }

        public void DrawParentBackground(IDeviceContext dc, Rectangle bounds, Control childControl)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (childControl == null)
                throw new ArgumentNullException("childControl");
            if (((bounds.Width >= 0) && (bounds.Height >= 0)) && (childControl.Handle != IntPtr.Zero)) {
                using (
                    WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                        dc,
                        TextFormatFlags.PreserveGraphicsTranslateTransform |
                        TextFormatFlags.PreserveGraphicsClipping)) {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult =
                        VisualStylesNativeMethods.DrawThemeParentBackground(
                            new HandleRef(this, childControl.Handle),
                            hdc,
                            new RECT(bounds));
                }
            }
        }

        public void DrawText(IDeviceContext dc, Rectangle bounds, string textToDraw)
        {
            this.DrawText(dc, bounds, textToDraw, false);
        }

        public void DrawText(IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled)
        {
            this.DrawText(dc, bounds, textToDraw, drawDisabled, TextFormatFlags.HorizontalCenter);
        }

        public void DrawText(
            IDeviceContext dc,
            Rectangle bounds,
            string textToDraw,
            bool drawDisabled,
            TextFormatFlags flags)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if ((bounds.Width >= 0) && (bounds.Height >= 0)) {
                int num = drawDisabled ? 1 : 0;
                if (!string.IsNullOrEmpty(textToDraw)) {
                    using (
                        WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                            dc,
                            TextFormatFlags.PreserveGraphicsTranslateTransform |
                            TextFormatFlags.PreserveGraphicsClipping)) {
                        var hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                        this.lastHResult = VisualStylesNativeMethods.DrawThemeText(
                            new HandleRef(this, Handle),
                            hdc,
                            this.part,
                            this.state,
                            textToDraw,
                            textToDraw.Length,
                            (int)flags,
                            num,
                            new RECT(bounds));
                    }
                }
            }
        }

        public Rectangle GetBackgroundContentRectangle(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if ((bounds.Width < 0) || (bounds.Height < 0))
                return Rectangle.Empty;
            RECT pContentRect = new RECT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeBackgroundContentRect(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        new RECT(bounds),
                        pContentRect);
            }
            return Rectangle.FromLTRB(
                pContentRect.left,
                pContentRect.top,
                pContentRect.right,
                pContentRect.bottom);
        }

        public Rectangle GetBackgroundExtent(IDeviceContext dc, Rectangle contentBounds)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if ((contentBounds.Width < 0) || (contentBounds.Height < 0))
                return Rectangle.Empty;
            RECT pExtentRect = new RECT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeBackgroundExtent(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        new RECT(contentBounds),
                        pExtentRect);
            }
            return Rectangle.FromLTRB(
                pExtentRect.left,
                pExtentRect.top,
                pExtentRect.right,
                pExtentRect.bottom);
        }

        [SuppressUnmanagedCodeSecurity]
        public Region GetBackgroundRegion(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if ((bounds.Width < 0) || (bounds.Height < 0))
                return null;
            IntPtr zero = IntPtr.Zero;
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeBackgroundRegion(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        new RECT(bounds),
                        ref zero);
            }
            if (zero == IntPtr.Zero)
                return null;
            Region region = Region.FromHrgn(zero);
            VisualStylesNativeMethods.ExternalDeleteObject(new HandleRef(null, zero));
            return region;
        }

        public bool GetBoolean(BooleanProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0x899, 0x8a5))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(BooleanProperty));
            bool pfVal = false;
            this.lastHResult = VisualStylesNativeMethods.GetThemeBool(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                ref pfVal);
            return pfVal;
        }

        public Color GetColor(ColorProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xed9, 0xeef))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(ColorProperty));
            int pColor = 0;
            this.lastHResult = VisualStylesNativeMethods.GetThemeColor(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                ref pColor);
            return ColorTranslator.FromWin32(pColor);
        }

        public int GetEnumValue(EnumProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xfa1, 0xfaf))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(EnumProperty));
            int piVal = 0;
            this.lastHResult = VisualStylesNativeMethods.GetThemeEnumValue(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                ref piVal);
            return piVal;
        }

        public string GetFilename(FilenameProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xbb9, 0xbc0))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(FilenameProperty));
            var pszThemeFilename = new StringBuilder(0x200);
            this.lastHResult = VisualStylesNativeMethods.GetThemeFilename(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                pszThemeFilename,
                pszThemeFilename.Capacity);
            return pszThemeFilename.ToString();
        }

        public Font GetFont(IDeviceContext dc, FontProperty prop)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xa29, 0xa29))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(FontProperty));
            LOGFONT pFont = new LOGFONT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = VisualStylesNativeMethods.GetThemeFont(
                    new HandleRef(this, this.Handle),
                    hdc,
                    this.part,
                    this.state,
                    (int)prop,
                    pFont);
            }
            Font font = null;
            if (!NativeMethods.Succeeded(this.lastHResult))
                return font;
            ////System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
            try {
                return Font.FromLogFont(pFont);
            } catch (Exception exception) {
                if (ClientUtils.IsSecurityOrCriticalException(exception))
                    throw;
                return null;
            }
        }

        private SafeThemeHandle GetHandle(string className)
        {
            return GetHandle(className, true);
        }

        private SafeThemeHandle GetHandle(string className, bool throwExceptionOnFail)
        {
            SafeThemeHandle handle;
            if (themeHandles == null)
                CreateThemeHandleHashtable();
            if (threadCacheVersion != globalCacheVersion) {
                RefreshCache();
                threadCacheVersion = globalCacheVersion;
            }
            if (!themeHandles.ContainsKey(className)) {
                handle = ThemeHandle.Create(controlHandle, className, throwExceptionOnFail);
                if (handle == null)
                    return SafeThemeHandle.Zero;
                themeHandles.Add(className, handle);
            } else
                handle = (SafeThemeHandle)themeHandles[className];
            return handle;
        }

        public int GetInteger(IntegerProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0x961, 0x978))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(IntegerProperty));
            int piVal = 0;
            this.lastHResult = VisualStylesNativeMethods.GetThemeInt(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                ref piVal);
            return piVal;
        }

        public Padding GetMargins(IDeviceContext dc, MarginProperty prop)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xe11, 0xe13))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(MarginProperty));
            MARGINS margins = new MARGINS();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hDC = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = VisualStylesNativeMethods.GetThemeMargins(
                    new HandleRef(this, this.Handle),
                    hDC,
                    this.part,
                    this.state,
                    (int)prop,
                    ref margins);
            }
            return new Padding(
                margins.cxLeftWidth,
                margins.cyTopHeight,
                margins.cxRightWidth,
                margins.cyBottomHeight);
        }

        public Size GetPartSize(IDeviceContext dc, ThemeSizeType type)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (!ClientUtils.IsEnumValid(type, (int)type, 0, 2))
                throw new InvalidEnumArgumentException("type", (int)type, typeof(ThemeSizeType));
            SIZE psz = new SIZE();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = VisualStylesNativeMethods.GetThemePartSize(
                    new HandleRef(this, this.Handle),
                    hdc,
                    this.part,
                    this.state,
                    null,
                    type,
                    psz);
            }
            return new Size(psz.cx, psz.cy);
        }

        public Size GetPartSize(IDeviceContext dc, Rectangle bounds, ThemeSizeType type)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (!ClientUtils.IsEnumValid(type, (int)type, 0, 2))
                throw new InvalidEnumArgumentException("type", (int)type, typeof(ThemeSizeType));
            SIZE psz = new SIZE();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = VisualStylesNativeMethods.GetThemePartSize(
                    new HandleRef(this, this.Handle),
                    hdc,
                    this.part,
                    this.state,
                    new RECT(bounds),
                    type,
                    psz);
            }
            return new Size(psz.cx, psz.cy);
        }

        public Point GetPoint(PointProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xd49, 0xd50))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(PointProperty));
            POINT pPoint = new POINT();
            this.lastHResult = VisualStylesNativeMethods.GetThemePosition(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                pPoint);
            return new Point(pPoint.x, pPoint.y);
        }

        public string GetString(StringProperty prop)
        {
            if (!ClientUtils.IsEnumValid(prop, (int)prop, 0xc81, 0xc81))
                throw new InvalidEnumArgumentException("prop", (int)prop, typeof(StringProperty));
            StringBuilder pszBuff = new StringBuilder(0x200);
            this.lastHResult = VisualStylesNativeMethods.GetThemeString(
                new HandleRef(this, this.Handle),
                this.part,
                this.state,
                (int)prop,
                pszBuff,
                pszBuff.Capacity);
            return pszBuff.ToString();
        }

        public Rectangle GetTextExtent(IDeviceContext dc, string textToDraw, TextFormatFlags flags)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (string.IsNullOrEmpty(textToDraw))
                throw new ArgumentNullException("textToDraw");
            RECT pExtentRect = new RECT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeTextExtent(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        textToDraw,
                        textToDraw.Length,
                        (int)flags,
                        null,
                        pExtentRect);
            }
            return Rectangle.FromLTRB(
                pExtentRect.left,
                pExtentRect.top,
                pExtentRect.right,
                pExtentRect.bottom);
        }

        public Rectangle GetTextExtent(
            IDeviceContext dc,
            Rectangle bounds,
            string textToDraw,
            TextFormatFlags flags)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            if (string.IsNullOrEmpty(textToDraw))
                throw new ArgumentNullException("textToDraw");
            RECT pExtentRect = new RECT();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeTextExtent(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        textToDraw,
                        textToDraw.Length,
                        (int)flags,
                        new RECT(bounds),
                        pExtentRect);
            }
            return Rectangle.FromLTRB(
                pExtentRect.left,
                pExtentRect.top,
                pExtentRect.right,
                pExtentRect.bottom);
        }

        public TextMetrics GetTextMetrics(IDeviceContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            TextMetrics ptm = new TextMetrics();
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.GetThemeTextMetrics(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        ref ptm);
            }
            return ptm;
        }

        public HitTestCode HitTestBackground(
            IDeviceContext dc,
            Rectangle backgroundRectangle,
            Point pt,
            HitTestOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            int pwHitTestCode = 0;
            POINTSTRUCT ptTest = new POINTSTRUCT(pt.X, pt.Y);
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.HitTestThemeBackground(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        (int)options,
                        new RECT(backgroundRectangle),
                        NativeMethods.NullHandleRef,
                        ptTest,
                        ref pwHitTestCode);
            }
            return (HitTestCode)pwHitTestCode;
        }

        public HitTestCode HitTestBackground(
            Graphics g,
            Rectangle backgroundRectangle,
            Region region,
            Point pt,
            HitTestOptions options)
        {
            if (g == null)
                throw new ArgumentNullException("g");
            IntPtr hrgn = region.GetHrgn(g);
            return this.HitTestBackground(g, backgroundRectangle, hrgn, pt, options);
        }

        public HitTestCode HitTestBackground(
            IDeviceContext dc,
            Rectangle backgroundRectangle,
            IntPtr hRgn,
            Point pt,
            HitTestOptions options)
        {
            if (dc == null)
                throw new ArgumentNullException("dc");
            int pwHitTestCode = 0;
            POINTSTRUCT ptTest = new POINTSTRUCT(pt.X, pt.Y);
            using (
                WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(
                    dc,
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping)) {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult =
                    VisualStylesNativeMethods.HitTestThemeBackground(
                        new HandleRef(this, this.Handle),
                        hdc,
                        this.part,
                        this.state,
                        (int)options,
                        new RECT(backgroundRectangle),
                        new HandleRef(this, hRgn),
                        ptTest,
                        ref pwHitTestCode);
            }
            return (HitTestCode)pwHitTestCode;
        }

        public bool IsBackgroundPartiallyTransparent()
        {
            return
                VisualStylesNativeMethods.IsThemeBackgroundPartiallyTransparent(
                    new HandleRef(this, this.Handle),
                    this.part,
                    this.state);
        }

        private bool IsCombinationDefined(string className, int part)
        {
            bool flag = false;
            if (!IsSupported) {
                if (!VisualStyleInformation.IsEnabledByUser)
                    throw new InvalidOperationException("VisualStyleNotActive");
                throw new InvalidOperationException("VisualStylesDisabledInClientArea");
            }
            if (className == null)
                throw new ArgumentNullException("className");
            SafeThemeHandle ptr = GetHandle(className, false);
            if (!ptr.IsInvalid) {
                if (part == 0)
                    flag = true;
                else {
                    flag =
                        VisualStylesNativeMethods.IsThemePartDefined(
                            new HandleRef(null, ptr.DangerousGetHandle()),
                            part,
                            0);
                }
            }
            if (!flag) {
                using (SafeThemeHandle handle = ThemeHandle.Create(controlHandle, className, false)) {
                    if (handle != null) {
                        flag =
                            VisualStylesNativeMethods.IsThemePartDefined(
                                new HandleRef(null, handle.DangerousGetHandle()),
                                part,
                                0);
                    }
                    if (flag)
                        RefreshCache();
                }
            }
            return flag;
        }

        public bool IsElementDefined(VisualStyleElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return IsCombinationDefined(element.ClassName, element.Part);
        }

        private static void OnUserPreferenceChanging(object sender, UserPreferenceChangingEventArgs ea)
        {
            if (ea.Category == UserPreferenceCategory.VisualStyle)
                globalCacheVersion += 1L;
        }

        private void RefreshCache()
        {
            SafeThemeHandle handle = null;
            if (themeHandles != null) {
                string[] array = new string[themeHandles.Keys.Count];
                themeHandles.Keys.CopyTo(array, 0);
                bool flag = VisualStyleInformation.IsEnabledByUser &&
                            ((Application.VisualStyleState == VisualStyleState.ClientAreaEnabled) ||
                             (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled));
                foreach (string str in array) {
                    handle = themeHandles[str];
                    if (handle != null)
                        handle.Dispose();
                    if (flag) {
                        handle = ThemeHandle.Create(controlHandle, str, false);
                        if (handle != null)
                            themeHandles[str] = handle;
                    }
                }
            }
        }

        public void SetParameters(VisualStyleElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            this.SetParameters(element.ClassName, element.Part, element.State);
        }

        public void SetParameters(string className, int part, int state)
        {
            if (!IsCombinationDefined(className, part))
                throw new ArgumentException("VisualStylesInvalidCombination");
            this._class = className;
            this.part = part;
            this.state = state;
        }

        public string Class
        {
            get { return this._class; }
        }

        public IntPtr Handle
        {
            get
            {
                if (IsSupported)
                    return GetHandle(this._class).DangerousGetHandle();
                if (!VisualStyleInformation.IsEnabledByUser)
                    throw new InvalidOperationException("VisualStyleNotActive");
                throw new InvalidOperationException("VisualStylesDisabledInClientArea");
            }
        }

        public bool IsSupported
        {
            get
            {
                bool flag = VisualStyleInformation.IsEnabledByUser &&
                            ((Application.VisualStyleState == VisualStyleState.ClientAreaEnabled) ||
                             (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled));
                if (flag)
                    flag = !GetHandle("BUTTON", false).IsInvalid;
                return flag;
            }
        }

        public int LastHResult
        {
            get { return this.lastHResult; }
        }

        public int Part
        {
            get { return this.part; }
        }

        public int State
        {
            get { return this.state; }
        }

        private static class ThemeHandle
        {
            public static SafeThemeHandle Create(IntPtr handle, string className, bool throwExceptionOnFail)
            {
                SafeThemeHandle zero = new SafeThemeHandle();
                try {
                    zero = VisualStylesNativeMethods.OpenThemeData(
                        new HandleRef(null, handle),
                        className);
                } catch (Exception exception) {
                    if (ClientUtils.IsSecurityOrCriticalException(exception))
                        throw;
                    if (throwExceptionOnFail)
                        throw new InvalidOperationException(("VisualStyleHandleCreationFailed"), exception);
                    return null;
                }
                if (!zero.IsInvalid)
                    return zero;
                if (throwExceptionOnFail)
                    throw new InvalidOperationException("VisualStyleHandleCreationFailed");
                return null;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTSTRUCT
    {
        public int x;
        public int y;

        public POINTSTRUCT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Flags]
    internal enum ApplyGraphicsProperties
    {
        None,
        Clipping,
        TranslateTransform,
        All
    }

    internal enum TextPaddingOptions
    {
        GlyphOverhangPadding,
        NoPadding,
        LeftAndRightPadding
    }

    internal sealed class WindowsGraphicsWrapper : IDisposable
    {
        private IDeviceContext idc;
        private WindowsGraphics wg;

        public WindowsGraphicsWrapper(IDeviceContext idc, TextFormatFlags flags)
        {
            if (idc is Graphics) {
                ApplyGraphicsProperties none = ApplyGraphicsProperties.None;
                if ((flags & TextFormatFlags.PreserveGraphicsClipping) != TextFormatFlags.Default)
                    none |= ApplyGraphicsProperties.Clipping;
                if ((flags & TextFormatFlags.PreserveGraphicsTranslateTransform) != TextFormatFlags.Default)
                    none |= ApplyGraphicsProperties.TranslateTransform;
                if (none != ApplyGraphicsProperties.None)
                    this.wg = WindowsGraphics.FromGraphics(idc as Graphics, none);
            } else {
                this.wg = idc as WindowsGraphics;
                if (this.wg != null)
                    this.idc = idc;
            }
            if (this.wg == null) {
                this.idc = idc;
                this.wg = WindowsGraphics.FromHdc(idc.GetHdc());
            }
            if ((flags & TextFormatFlags.LeftAndRightPadding) != TextFormatFlags.Default)
                this.wg.TextPadding = TextPaddingOptions.LeftAndRightPadding;
            else if ((flags & TextFormatFlags.NoPadding) != TextFormatFlags.Default)
                this.wg.TextPadding = TextPaddingOptions.NoPadding;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.wg != null) {
                if (this.wg != this.idc) {
                    this.wg.Dispose();
                    if (this.idc != null)
                        this.idc.ReleaseHdc();
                }
                this.idc = null;
                this.wg = null;
            }
        }

        ~WindowsGraphicsWrapper()
        {
            this.Dispose(false);
        }

        public WindowsGraphics WindowsGraphics
        {
            get { return this.wg; }
        }
    }

    internal enum DeviceContextType
    {
        Unknown,
        Display,
        NCWindow,
        NamedDevice,
        Information,
        Memory,
        Metafile
    }

    internal enum WindowsFontQuality
    {
        Default,
        Draft,
        Proof,
        NonAntiAliased,
        AntiAliased,
        ClearType,
        ClearTypeNatural
    }

    internal sealed class WindowsFont : MarshalByRefObject, ICloneable, IDisposable
    {
        private const string defaultFaceName = "Microsoft Sans Serif";
        private const int defaultFontHeight = 13;
        private const float defaultFontSize = 8.25f;
        private bool everOwnedByCacheManager;
        private float fontSize;
        private IntPtr hFont;
        private int lineSpacing;
        private readonly IntNativeMethods.LOGFONT logFont;
        private const int LogFontNameOffset = 0x1c;
        private bool ownedByCacheManager;
        private bool ownHandle;
        private readonly FontStyle style;

        public WindowsFont(string faceName)
            : this(faceName, 8.25f, FontStyle.Regular, 1, WindowsFontQuality.Default)
        {
        }

        public WindowsFont(string faceName, float size)
            : this(faceName, size, FontStyle.Regular, 1, WindowsFontQuality.Default)
        {
        }

        private WindowsFont(IntNativeMethods.LOGFONT lf, bool createHandle)
        {
            this.fontSize = -1f;
            this.logFont = lf;
            if (this.logFont.lfFaceName == null)
                this.logFont.lfFaceName = "Microsoft Sans Serif";
            this.style = FontStyle.Regular;
            if (lf.lfWeight == 700)
                this.style |= FontStyle.Bold;
            if (lf.lfItalic == 1)
                this.style |= FontStyle.Italic;
            if (lf.lfUnderline == 1)
                this.style |= FontStyle.Underline;
            if (lf.lfStrikeOut == 1)
                this.style |= FontStyle.Strikeout;
            if (createHandle)
                this.CreateFont();
        }

        public WindowsFont(string faceName, float size, FontStyle style)
            : this(faceName, size, style, 1, WindowsFontQuality.Default)
        {
        }

        public WindowsFont(
            string faceName,
            float size,
            FontStyle style,
            byte charSet,
            WindowsFontQuality fontQuality)
        {
            this.fontSize = -1f;
            this.logFont = new IntNativeMethods.LOGFONT();
            int num =
                (int)
                    Math.Ceiling(
                        (double)
                            ((WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.DpiY * size) / 72f));
            this.logFont.lfHeight = -num;
            this.logFont.lfFaceName = (faceName != null) ? faceName : "Microsoft Sans Serif";
            this.logFont.lfCharSet = charSet;
            this.logFont.lfOutPrecision = 4;
            this.logFont.lfQuality = (byte)fontQuality;
            this.logFont.lfWeight = ((style & FontStyle.Bold) == FontStyle.Bold) ? 700 : 400;
            this.logFont.lfItalic = ((style & FontStyle.Italic) == FontStyle.Italic) ? ((byte)1) : ((byte)0);
            this.logFont.lfUnderline = ((style & FontStyle.Underline) == FontStyle.Underline)
                ? ((byte)1)
                : ((byte)0);
            this.logFont.lfStrikeOut = ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
                ? ((byte)1)
                : ((byte)0);
            this.style = style;
            this.CreateFont();
        }

        public object Clone()
        {
            return new WindowsFont(this.logFont, true);
        }

        private void CreateFont()
        {
            this.hFont = IntUnsafeNativeMethods.CreateFontIndirect(this.logFont);
            if (this.hFont == IntPtr.Zero) {
                this.logFont.lfFaceName = "Microsoft Sans Serif";
                this.logFont.lfOutPrecision = 7;
                this.hFont = IntUnsafeNativeMethods.CreateFontIndirect(this.logFont);
            }
            IntUnsafeNativeMethods.GetObject(new HandleRef(this, this.hFont), this.logFont);
            this.ownHandle = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        internal void Dispose(bool disposing)
        {
            bool flag = false;
            if ((this.ownHandle && (!this.ownedByCacheManager || !disposing)) &&
                ((this.everOwnedByCacheManager || !disposing) || !DeviceContexts.IsFontInUse(this))) {
                IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, this.hFont));
                this.hFont = IntPtr.Zero;
                this.ownHandle = false;
                flag = true;
            }
            if (disposing && (flag || !this.ownHandle))
                GC.SuppressFinalize(this);
        }

        public override bool Equals(object font)
        {
            WindowsFont font2 = font as WindowsFont;
            if (font2 == null)
                return false;
            return ((font2 == this) ||
                    ((((this.Name == font2.Name) && (this.LogFontHeight == font2.LogFontHeight)) &&
                      ((this.Style == font2.Style) && (this.CharSet == font2.CharSet))) &&
                     (this.Quality == font2.Quality)));
        }

        ~WindowsFont()
        {
            this.Dispose(false);
        }

        public static WindowsFont FromFont(Font font)
        {
            return FromFont(font, WindowsFontQuality.Default);
        }

        public static WindowsFont FromFont(Font font, WindowsFontQuality fontQuality)
        {
            string name = font.FontFamily.Name;
            if (((name != null) && (name.Length > 1)) && (name[0] == '@'))
                name = name.Substring(1);
            return new WindowsFont(name, font.SizeInPoints, font.Style, font.GdiCharSet, fontQuality);
        }

        public static WindowsFont FromHdc(IntPtr hdc)
        {
            return FromHfont(IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(null, hdc), 6));
        }

        public static WindowsFont FromHfont(IntPtr hFont)
        {
            return FromHfont(hFont, false);
        }

        public static WindowsFont FromHfont(IntPtr hFont, bool takeOwnership)
        {
            IntNativeMethods.LOGFONT lp = new IntNativeMethods.LOGFONT();
            IntUnsafeNativeMethods.GetObject(new HandleRef(null, hFont), lp);
            return new WindowsFont(lp, false) { hFont = hFont, ownHandle = takeOwnership };
        }

        public override int GetHashCode()
        {
            return ((((((int)this.Style) << 13) | (((int)this.Style) >> 0x13)) ^
                     ((this.CharSet << 0x1a) | (this.CharSet >> 6))) ^
                    ((int)((((uint)this.Size) << 7) | (((uint)this.Size) >> 0x19))));
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "[{0}: Name={1}, Size={2} points, Height={3} pixels, Sytle={4}]",
                new object[]
                { base.GetType().Name, this.logFont.lfFaceName, this.Size, this.Height, this.Style });
        }

        public static WindowsFontQuality WindowsFontQualityFromTextRenderingHint(Graphics g)
        {
            if (g != null) {
                switch (g.TextRenderingHint) {
                    case TextRenderingHint.SingleBitPerPixelGridFit:
                        return WindowsFontQuality.Proof;

                    case TextRenderingHint.SingleBitPerPixel:
                        return WindowsFontQuality.Draft;

                    case TextRenderingHint.AntiAliasGridFit:
                        return WindowsFontQuality.AntiAliased;

                    case TextRenderingHint.AntiAlias:
                        return WindowsFontQuality.AntiAliased;

                    case TextRenderingHint.ClearTypeGridFit:
                        if ((Environment.OSVersion.Version.Major != 5) ||
                            (Environment.OSVersion.Version.Minor < 1))
                            return WindowsFontQuality.ClearType;
                        return WindowsFontQuality.ClearTypeNatural;
                }
            }
            return WindowsFontQuality.Default;
        }

        public byte CharSet
        {
            get { return this.logFont.lfCharSet; }
        }

        public int Height
        {
            get
            {
                if (this.lineSpacing == 0) {
                    WindowsGraphics measurementGraphics = WindowsGraphicsCacheManager.MeasurementGraphics;
                    measurementGraphics.DeviceContext.SelectFont(this);
                    IntNativeMethods.TEXTMETRIC textMetrics = measurementGraphics.GetTextMetrics();
                    this.lineSpacing = textMetrics.tmHeight;
                }
                return this.lineSpacing;
            }
        }

        public IntPtr Hfont
        {
            get { return this.hFont; }
        }

        public bool Italic
        {
            get { return (this.logFont.lfItalic == 1); }
        }

        public int LogFontHeight
        {
            get { return this.logFont.lfHeight; }
        }

        public string Name
        {
            get { return this.logFont.lfFaceName; }
        }

        public bool OwnedByCacheManager
        {
            get { return this.ownedByCacheManager; }
            set
            {
                if (value)
                    this.everOwnedByCacheManager = true;
                this.ownedByCacheManager = value;
            }
        }

        public WindowsFontQuality Quality
        {
            get { return (WindowsFontQuality)this.logFont.lfQuality; }
        }

        public float Size
        {
            get
            {
                if (this.fontSize < 0f) {
                    WindowsGraphics measurementGraphics = WindowsGraphicsCacheManager.MeasurementGraphics;
                    measurementGraphics.DeviceContext.SelectFont(this);
                    IntNativeMethods.TEXTMETRIC textMetrics = measurementGraphics.GetTextMetrics();
                    int num = (this.logFont.lfHeight > 0)
                        ? textMetrics.tmHeight
                        : (textMetrics.tmHeight - textMetrics.tmInternalLeading);
                    this.fontSize = (num * 72f) / ((float)measurementGraphics.DeviceContext.DpiY);
                }
                return this.fontSize;
            }
        }

        public FontStyle Style
        {
            get { return this.style; }
        }
    }

    internal static class DeviceContexts
    {
        [ThreadStatic]
        private static ClientUtils.WeakRefCollection activeDeviceContexts;

        internal static void AddDeviceContext(DeviceContext dc)
        {
            if (activeDeviceContexts == null) {
                activeDeviceContexts = new ClientUtils.WeakRefCollection();
                activeDeviceContexts.RefCheckThreshold = 20;
            }
            if (!activeDeviceContexts.Contains(dc)) {
                dc.Disposing += new EventHandler(OnDcDisposing);
                activeDeviceContexts.Add(dc);
            }
        }

        internal static bool IsFontInUse(WindowsFont wf)
        {
            if (wf != null) {
                for (int i = 0; i < activeDeviceContexts.Count; i++) {
                    DeviceContext context = activeDeviceContexts[i] as DeviceContext;
                    if ((context != null) && ((context.ActiveFont == wf) || context.IsFontOnContextStack(wf)))
                        return true;
                }
            }
            return false;
        }

        private static void OnDcDisposing(object sender, EventArgs e)
        {
            DeviceContext dc = sender as DeviceContext;
            if (dc != null) {
                dc.Disposing -= new EventHandler(OnDcDisposing);
                RemoveDeviceContext(dc);
            }
        }

        internal static void RemoveDeviceContext(DeviceContext dc)
        {
            if (activeDeviceContexts != null)
                activeDeviceContexts.RemoveByHashCode(dc);
        }
    }

    internal enum DeviceCapabilities
    {
        BitsPerPixel = 12,
        DriverVersion = 0,
        HorizontalResolution = 8,
        HorizontalSize = 4,
        LogicalPixelsX = 0x58,
        LogicalPixelsY = 90,
        PhysicalHeight = 0x6f,
        PhysicalOffsetX = 0x70,
        PhysicalOffsetY = 0x71,
        PhysicalWidth = 110,
        ScalingFactorX = 0x72,
        ScalingFactorY = 0x73,
        Technology = 2,
        VerticalResolution = 10,
        VerticalSize = 6
    }

    internal enum DeviceContextBackgroundMode
    {
        Opaque = 2,
        Transparent = 1
    }

    internal enum GdiObjectType
    {
        Bitmap = 7,
        Brush = 2,
        ColorSpace = 14,
        DisplayDC = 3,
        EnhancedMetafileDC = 12,
        EnhMetafile = 13,
        ExtendedPen = 11,
        Font = 6,
        MemoryDC = 10,
        Metafile = 9,
        MetafileDC = 4,
        Palette = 5,
        Pen = 1,
        Region = 8
    }

    internal enum DeviceContextMapMode
    {
        Anisotropic = 8,
        HiEnglish = 5,
        HiMetric = 3,
        Isotropic = 7,
        LoEnglish = 4,
        LoMetric = 2,
        Text = 1,
        Twips = 6
    }

    [Flags]
    internal enum DeviceContextBinaryRasterOperationFlags
    {
        Black = 1,
        CopyPen = 13,
        MaskNotPen = 3,
        MaskPen = 9,
        MaskPenNot = 5,
        MergeNotPen = 12,
        MergePen = 15,
        MergePenNot = 14,
        Nop = 11,
        Not = 6,
        NotCopyPen = 4,
        NotMaskPen = 8,
        NotMergePen = 2,
        NotXorPen = 10,
        White = 0x10,
        XorPen = 7
    }

    internal class WindowsGraphicsCacheManager
    {
        private const int CacheSize = 10;

        [ThreadStatic]
        private static int currentIndex;

        [ThreadStatic]
        private static WindowsGraphics measurementGraphics;

        [ThreadStatic]
        private static List<KeyValuePair<Font, WindowsFont>> windowsFontCache;

        private WindowsGraphicsCacheManager()
        {
        }

        internal static WindowsGraphics GetCurrentMeasurementGraphics()
        {
            return measurementGraphics;
        }

        public static WindowsFont GetWindowsFont(Font font)
        {
            return GetWindowsFont(font, WindowsFontQuality.Default);
        }

        public static WindowsFont GetWindowsFont(Font font, WindowsFontQuality fontQuality)
        {
            if (font == null)
                return null;
            int num = 0;
            int currentIndex = WindowsGraphicsCacheManager.currentIndex;
            while (num < WindowsFontCache.Count) {
                KeyValuePair<Font, WindowsFont> pair2 = WindowsFontCache[currentIndex];
                if (pair2.Key.Equals(font)) {
                    KeyValuePair<Font, WindowsFont> pair3 = WindowsFontCache[currentIndex];
                    WindowsFont font2 = pair3.Value;
                    if (font2.Quality == fontQuality)
                        return font2;
                }
                currentIndex--;
                num++;
                if (currentIndex < 0)
                    currentIndex = 9;
            }
            WindowsFont font3 = WindowsFont.FromFont(font, fontQuality);
            KeyValuePair<Font, WindowsFont> item = new KeyValuePair<Font, WindowsFont>(font, font3);
            WindowsGraphicsCacheManager.currentIndex++;
            if (WindowsGraphicsCacheManager.currentIndex == 10)
                WindowsGraphicsCacheManager.currentIndex = 0;
            if (WindowsFontCache.Count != 10) {
                font3.OwnedByCacheManager = true;
                WindowsFontCache.Add(item);
                return font3;
            }
            WindowsFont wf = null;
            bool flag = false;
            int num3 = WindowsGraphicsCacheManager.currentIndex;
            int num4 = num3 + 1;
            while (!flag) {
                if (num4 >= 10)
                    num4 = 0;
                if (num4 == num3)
                    flag = true;
                KeyValuePair<Font, WindowsFont> pair4 = WindowsFontCache[num4];
                wf = pair4.Value;
                if (!DeviceContexts.IsFontInUse(wf)) {
                    WindowsGraphicsCacheManager.currentIndex = num4;
                    flag = true;
                    break;
                }
                num4++;
                wf = null;
            }
            if (wf != null) {
                WindowsFontCache[WindowsGraphicsCacheManager.currentIndex] = item;
                font3.OwnedByCacheManager = true;
                wf.OwnedByCacheManager = false;
                wf.Dispose();
                return font3;
            }
            font3.OwnedByCacheManager = false;
            return font3;
        }

        public static WindowsGraphics MeasurementGraphics
        {
            get
            {
                if ((measurementGraphics == null) || (measurementGraphics.DeviceContext == null))
                    measurementGraphics = WindowsGraphics.CreateMeasurementWindowsGraphics();
                return measurementGraphics;
            }
        }

        private static List<KeyValuePair<Font, WindowsFont>> WindowsFontCache
        {
            get
            {
                if (windowsFontCache == null) {
                    currentIndex = -1;
                    windowsFontCache = new List<KeyValuePair<Font, WindowsFont>>(10);
                }
                return windowsFontCache;
            }
        }
    }

    internal static class MeasurementDCInfo
    {
        [ThreadStatic]
        private static CachedInfo cachedMeasurementDCInfo;

        internal static IntNativeMethods.DRAWTEXTPARAMS GetTextMargins(WindowsGraphics wg, WindowsFont font)
        {
            CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
            if (((cachedMeasurementDCInfo == null) || (cachedMeasurementDCInfo.LeftTextMargin <= 0)) ||
                ((cachedMeasurementDCInfo.RightTextMargin <= 0) ||
                 (font != cachedMeasurementDCInfo.LastUsedFont))) {
                if (cachedMeasurementDCInfo == null) {
                    cachedMeasurementDCInfo = new CachedInfo();
                    MeasurementDCInfo.cachedMeasurementDCInfo = cachedMeasurementDCInfo;
                }
                IntNativeMethods.DRAWTEXTPARAMS textMargins = wg.GetTextMargins(font);
                cachedMeasurementDCInfo.LeftTextMargin = textMargins.iLeftMargin;
                cachedMeasurementDCInfo.RightTextMargin = textMargins.iRightMargin;
            }
            return new IntNativeMethods.DRAWTEXTPARAMS(
                cachedMeasurementDCInfo.LeftTextMargin,
                cachedMeasurementDCInfo.RightTextMargin);
        }

        internal static bool IsMeasurementDC(DeviceContext dc)
        {
            WindowsGraphics currentMeasurementGraphics =
                WindowsGraphicsCacheManager.GetCurrentMeasurementGraphics();
            return (((currentMeasurementGraphics != null) &&
                     (currentMeasurementGraphics.DeviceContext != null)) &&
                    (currentMeasurementGraphics.DeviceContext.Hdc == dc.Hdc));
        }

        internal static void Reset()
        {
            CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
            if (cachedMeasurementDCInfo != null)
                cachedMeasurementDCInfo.UpdateFont(null);
        }

        internal static void ResetIfIsMeasurementDC(IntPtr hdc)
        {
            WindowsGraphics currentMeasurementGraphics =
                WindowsGraphicsCacheManager.GetCurrentMeasurementGraphics();
            if (((currentMeasurementGraphics != null) && (currentMeasurementGraphics.DeviceContext != null)) &&
                (currentMeasurementGraphics.DeviceContext.Hdc == hdc)) {
                CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
                if (cachedMeasurementDCInfo != null)
                    cachedMeasurementDCInfo.UpdateFont(null);
            }
        }

        internal static WindowsFont LastUsedFont
        {
            get
            {
                if (cachedMeasurementDCInfo != null)
                    return cachedMeasurementDCInfo.LastUsedFont;
                return null;
            }
            set
            {
                if (cachedMeasurementDCInfo == null)
                    cachedMeasurementDCInfo = new CachedInfo();
                cachedMeasurementDCInfo.UpdateFont(value);
            }
        }

        private sealed class CachedInfo
        {
            public WindowsFont LastUsedFont;
            public int LeftTextMargin;
            public int RightTextMargin;

            internal void UpdateFont(WindowsFont font)
            {
                if (this.LastUsedFont != font) {
                    this.LastUsedFont = font;
                    this.LeftTextMargin = -1;
                    this.RightTextMargin = -1;
                }
            }
        }
    }

    internal sealed class DeviceContext : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private Stack contextStack;
        private readonly DeviceContextType dcType;
        private bool disposed;
        private IntPtr hCurrentBmp;
        private IntPtr hCurrentBrush;
        private IntPtr hCurrentFont;
        private IntPtr hCurrentPen;
        private IntPtr hDC;
        private IntPtr hInitialBmp;
        private IntPtr hInitialBrush;
        private IntPtr hInitialFont;
        private IntPtr hInitialPen;
        private readonly IntPtr hWnd;
        private WindowsFont selectedFont;

        public event EventHandler Disposing;

        private DeviceContext(IntPtr hWnd)
        {
            this.hWnd = (IntPtr)(-1);
            this.hWnd = hWnd;
            this.dcType = DeviceContextType.Display;
            DeviceContexts.AddDeviceContext(this);
        }

        private DeviceContext(IntPtr hDC, DeviceContextType dcType)
        {
            this.hWnd = (IntPtr)(-1);
            this.hDC = hDC;
            this.dcType = dcType;
            this.CacheInitialState();
            DeviceContexts.AddDeviceContext(this);
            if (dcType == DeviceContextType.Display)
                this.hWnd = IntUnsafeNativeMethods.WindowFromDC(new HandleRef(this, this.hDC));
        }

        private void CacheInitialState()
        {
            this.hCurrentPen =
                this.hInitialPen = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 1);
            this.hCurrentBrush =
                this.hInitialBrush = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 2);
            this.hCurrentBmp =
                this.hInitialBmp = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 7);
            this.hCurrentFont =
                this.hInitialFont = IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 6);
        }

        public static DeviceContext CreateDC(
            string driverName,
            string deviceName,
            string fileName,
            HandleRef devMode)
        {
            return
                new DeviceContext(
                    IntUnsafeNativeMethods.CreateDC(driverName, deviceName, fileName, devMode),
                    DeviceContextType.NamedDevice);
        }

        public static DeviceContext CreateIC(
            string driverName,
            string deviceName,
            string fileName,
            HandleRef devMode)
        {
            return
                new DeviceContext(
                    IntUnsafeNativeMethods.CreateIC(driverName, deviceName, fileName, devMode),
                    DeviceContextType.Information);
        }

        public void DeleteObject(IntPtr handle, GdiObjectType type)
        {
            IntPtr zero = IntPtr.Zero;
            switch (type) {
                case GdiObjectType.Pen:
                    if (handle == this.hCurrentPen) {
                        IntUnsafeNativeMethods.SelectObject(
                            new HandleRef(this, this.Hdc),
                            new HandleRef(this, this.hInitialPen));
                        this.hCurrentPen = IntPtr.Zero;
                    }
                    zero = handle;
                    break;

                case GdiObjectType.Brush:
                    if (handle == this.hCurrentBrush) {
                        IntUnsafeNativeMethods.SelectObject(
                            new HandleRef(this, this.Hdc),
                            new HandleRef(this, this.hInitialBrush));
                        this.hCurrentBrush = IntPtr.Zero;
                    }
                    zero = handle;
                    break;

                case GdiObjectType.Bitmap:
                    if (handle == this.hCurrentBmp) {
                        IntUnsafeNativeMethods.SelectObject(
                            new HandleRef(this, this.Hdc),
                            new HandleRef(this, this.hInitialBmp));
                        this.hCurrentBmp = IntPtr.Zero;
                    }
                    zero = handle;
                    break;
            }
            IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, zero));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (this.Disposing != null)
                    this.Disposing(this, EventArgs.Empty);
                this.disposed = true;
                this.DisposeFont(disposing);
                switch (this.dcType) {
                    case DeviceContextType.Unknown:
                    case DeviceContextType.NCWindow:
                        return;

                    case DeviceContextType.Display:
                        ((IDeviceContext)this).ReleaseHdc();
                        return;

                    case DeviceContextType.NamedDevice:
                    case DeviceContextType.Information:
                        IntUnsafeNativeMethods.DeleteHDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;

                    case DeviceContextType.Memory:
                        IntUnsafeNativeMethods.DeleteDC(new HandleRef(this, this.hDC));
                        this.hDC = IntPtr.Zero;
                        return;
                }
            }
        }

        internal void DisposeFont(bool disposing)
        {
            if (disposing)
                DeviceContexts.RemoveDeviceContext(this);
            if ((this.selectedFont != null) && (this.selectedFont.Hfont != IntPtr.Zero)) {
                if (IntUnsafeNativeMethods.GetCurrentObject(new HandleRef(this, this.hDC), 6) ==
                    this.selectedFont.Hfont) {
                    IntUnsafeNativeMethods.SelectObject(
                        new HandleRef(this, this.Hdc),
                        new HandleRef(null, this.hInitialFont));
                    IntPtr hInitialFont = this.hInitialFont;
                }
                this.selectedFont.Dispose(disposing);
                this.selectedFont = null;
            }
        }

        public override bool Equals(object obj)
        {
            DeviceContext context = obj as DeviceContext;
            if (context == this)
                return true;
            if (context == null)
                return false;
            return (context.Hdc == this.Hdc);
        }

        ~DeviceContext()
        {
            this.Dispose(false);
        }

        public static DeviceContext FromCompatibleDC(IntPtr hdc)
        {
            return new DeviceContext(
                IntUnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, hdc)),
                DeviceContextType.Memory);
        }

        public static DeviceContext FromHdc(IntPtr hdc)
        {
            return new DeviceContext(hdc, DeviceContextType.Unknown);
        }

        public static DeviceContext FromHwnd(IntPtr hwnd)
        {
            return new DeviceContext(hwnd);
        }

        public int GetDeviceCapabilities(DeviceCapabilities capabilityIndex)
        {
            return IntUnsafeNativeMethods.GetDeviceCaps(new HandleRef(this, this.Hdc), (int)capabilityIndex);
        }

        public override int GetHashCode()
        {
            return this.Hdc.GetHashCode();
        }

        public void IntersectClip(WindowsRegion wr)
        {
            if (wr.HRegion != IntPtr.Zero) {
                using (WindowsRegion region = new WindowsRegion(0, 0, 0, 0)) {
                    if (
                        IntUnsafeNativeMethods.GetClipRgn(
                            new HandleRef(this, this.Hdc),
                            new HandleRef(region, region.HRegion)) == 1)
                        wr.CombineRegion(region, wr, RegionCombineMode.AND);
                    this.SetClip(wr);
                }
            }
        }

        public bool IsFontOnContextStack(WindowsFont wf)
        {
            if (this.contextStack != null) {
                foreach (GraphicsState state in this.contextStack) {
                    if (state.hFont == wf.Hfont)
                        return true;
                }
            }
            return false;
        }

        public void ResetFont()
        {
            MeasurementDCInfo.ResetIfIsMeasurementDC(this.Hdc);
            IntUnsafeNativeMethods.SelectObject(
                new HandleRef(this, this.Hdc),
                new HandleRef(null, this.hInitialFont));
            this.selectedFont = null;
            this.hCurrentFont = this.hInitialFont;
        }

        public void RestoreHdc()
        {
            IntUnsafeNativeMethods.RestoreDC(new HandleRef(this, this.hDC), -1);
            if (this.contextStack != null) {
                GraphicsState state = (GraphicsState)this.contextStack.Pop();
                this.hCurrentBmp = state.hBitmap;
                this.hCurrentBrush = state.hBrush;
                this.hCurrentPen = state.hPen;
                this.hCurrentFont = state.hFont;
                if ((state.font != null) && state.font.IsAlive)
                    this.selectedFont = state.font.Target as WindowsFont;
                else {
                    WindowsFont selectedFont = this.selectedFont;
                    this.selectedFont = null;
                    if ((selectedFont != null) && MeasurementDCInfo.IsMeasurementDC(this))
                        selectedFont.Dispose();
                }
            }
            MeasurementDCInfo.ResetIfIsMeasurementDC(this.hDC);
        }

        public int SaveHdc()
        {
            HandleRef hDC = new HandleRef(this, this.Hdc);
            int num = IntUnsafeNativeMethods.SaveDC(hDC);
            if (this.contextStack == null)
                this.contextStack = new Stack();
            GraphicsState state = new GraphicsState {
                hBitmap = this.hCurrentBmp,
                hBrush = this.hCurrentBrush,
                hPen = this.hCurrentPen,
                hFont = this.hCurrentFont,
                font = new WeakReference(this.selectedFont)
            };
            this.contextStack.Push(state);
            return num;
        }

        public IntPtr SelectFont(WindowsFont font)
        {
            if (font.Equals(this.Font))
                return IntPtr.Zero;
            IntPtr ptr = this.SelectObject(font.Hfont, GdiObjectType.Font);
            WindowsFont selectedFont = this.selectedFont;
            this.selectedFont = font;
            this.hCurrentFont = font.Hfont;
            if ((selectedFont != null) && MeasurementDCInfo.IsMeasurementDC(this))
                selectedFont.Dispose();
            if (MeasurementDCInfo.IsMeasurementDC(this)) {
                if (ptr != IntPtr.Zero) {
                    MeasurementDCInfo.LastUsedFont = font;
                    return ptr;
                }
                MeasurementDCInfo.Reset();
            }
            return ptr;
        }

        public IntPtr SelectObject(IntPtr hObj, GdiObjectType type)
        {
            switch (type) {
                case GdiObjectType.Pen:
                    this.hCurrentPen = hObj;
                    break;

                case GdiObjectType.Brush:
                    this.hCurrentBrush = hObj;
                    break;

                case GdiObjectType.Bitmap:
                    this.hCurrentBmp = hObj;
                    break;
            }
            return IntUnsafeNativeMethods.SelectObject(
                new HandleRef(this, this.Hdc),
                new HandleRef(null, hObj));
        }

        public Color SetBackgroundColor(Color newColor)
        {
            return
                ColorTranslator.FromWin32(
                    IntUnsafeNativeMethods.SetBkColor(
                        new HandleRef(this, this.Hdc),
                        ColorTranslator.ToWin32(newColor)));
        }

        public DeviceContextBackgroundMode SetBackgroundMode(DeviceContextBackgroundMode newMode)
        {
            return
                (DeviceContextBackgroundMode)
                    IntUnsafeNativeMethods.SetBkMode(new HandleRef(this, this.Hdc), (int)newMode);
        }

        public void SetClip(WindowsRegion region)
        {
            HandleRef hDC = new HandleRef(this, this.Hdc);
            HandleRef hRgn = new HandleRef(region, region.HRegion);
            IntUnsafeNativeMethods.SelectClipRgn(hDC, hRgn);
        }

        public DeviceContextGraphicsMode SetGraphicsMode(DeviceContextGraphicsMode newMode)
        {
            return
                (DeviceContextGraphicsMode)
                    IntUnsafeNativeMethods.SetGraphicsMode(new HandleRef(this, this.Hdc), (int)newMode);
        }

        public DeviceContextMapMode SetMapMode(DeviceContextMapMode newMode)
        {
            return
                (DeviceContextMapMode)
                    IntUnsafeNativeMethods.SetMapMode(new HandleRef(this, this.Hdc), (int)newMode);
        }

        public DeviceContextBinaryRasterOperationFlags SetRasterOperation(
            DeviceContextBinaryRasterOperationFlags rasterOperation)
        {
            return
                (DeviceContextBinaryRasterOperationFlags)
                    IntUnsafeNativeMethods.SetROP2(new HandleRef(this, this.Hdc), (int)rasterOperation);
        }

        public DeviceContextTextAlignment SetTextAlignment(DeviceContextTextAlignment newAligment)
        {
            return
                (DeviceContextTextAlignment)
                    IntUnsafeNativeMethods.SetTextAlign(new HandleRef(this, this.Hdc), (int)newAligment);
        }

        public Color SetTextColor(Color newColor)
        {
            return
                ColorTranslator.FromWin32(
                    IntUnsafeNativeMethods.SetTextColor(
                        new HandleRef(this, this.Hdc),
                        ColorTranslator.ToWin32(newColor)));
        }

        public Size SetViewportExtent(Size newExtent)
        {
            IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
            IntUnsafeNativeMethods.SetViewportExtEx(
                new HandleRef(this, this.Hdc),
                newExtent.Width,
                newExtent.Height,
                size);
            return size.ToSize();
        }

        public Point SetViewportOrigin(Point newOrigin)
        {
            IntNativeMethods.POINT point = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.SetViewportOrgEx(
                new HandleRef(this, this.Hdc),
                newOrigin.X,
                newOrigin.Y,
                point);
            return point.ToPoint();
        }

        IntPtr IDeviceContext.GetHdc()
        {
            if (this.hDC == IntPtr.Zero)
                this.hDC = IntUnsafeNativeMethods.GetDC(new HandleRef(this, this.hWnd));
            return this.hDC;
        }

        void IDeviceContext.ReleaseHdc()
        {
            if ((this.hDC != IntPtr.Zero) && (this.dcType == DeviceContextType.Display)) {
                IntUnsafeNativeMethods.ReleaseDC(
                    new HandleRef(this, this.hWnd),
                    new HandleRef(this, this.hDC));
                this.hDC = IntPtr.Zero;
            }
        }

        public void TranslateTransform(int dx, int dy)
        {
            IntNativeMethods.POINT point = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.OffsetViewportOrgEx(new HandleRef(this, this.Hdc), dx, dy, point);
        }

        public WindowsFont ActiveFont
        {
            get { return this.selectedFont; }
        }

        public Color BackgroundColor
        {
            get
            {
                return
                    ColorTranslator.FromWin32(
                        IntUnsafeNativeMethods.GetBkColor(new HandleRef(this, this.Hdc)));
            }
        }

        public DeviceContextBackgroundMode BackgroundMode
        {
            get
            {
                return
                    (DeviceContextBackgroundMode)
                        IntUnsafeNativeMethods.GetBkMode(new HandleRef(this, this.Hdc));
            }
        }

        public DeviceContextBinaryRasterOperationFlags BinaryRasterOperation
        {
            get
            {
                return
                    (DeviceContextBinaryRasterOperationFlags)
                        IntUnsafeNativeMethods.GetROP2(new HandleRef(this, this.Hdc));
            }
        }

        public DeviceContextType DeviceContextType
        {
            get { return this.dcType; }
        }

        public Size Dpi
        {
            get
            {
                return new Size(
                    this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsX),
                    this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsY));
            }
        }

        public int DpiX
        {
            get { return this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsX); }
        }

        public int DpiY
        {
            get { return this.GetDeviceCapabilities(DeviceCapabilities.LogicalPixelsY); }
        }

        public WindowsFont Font
        {
            get
            {
                if (MeasurementDCInfo.IsMeasurementDC(this)) {
                    WindowsFont lastUsedFont = MeasurementDCInfo.LastUsedFont;
                    if ((lastUsedFont != null) && (lastUsedFont.Hfont != IntPtr.Zero))
                        return lastUsedFont;
                }
                return WindowsFont.FromHdc(this.Hdc);
            }
        }

        public DeviceContextGraphicsMode GraphicsMode
        {
            get
            {
                return
                    (DeviceContextGraphicsMode)
                        IntUnsafeNativeMethods.GetGraphicsMode(new HandleRef(this, this.Hdc));
            }
        }

        public IntPtr Hdc
        {
            get
            {
                if ((this.hDC == IntPtr.Zero) && (this.dcType == DeviceContextType.Display)) {
                    this.hDC = ((IDeviceContext)this).GetHdc();
                    this.CacheInitialState();
                }
                return this.hDC;
            }
        }

        public DeviceContextMapMode MapMode
        {
            get
            {
                return (DeviceContextMapMode)IntUnsafeNativeMethods.GetMapMode(new HandleRef(this, this.Hdc));
            }
        }

        public static DeviceContext ScreenDC
        {
            get { return FromHwnd(IntPtr.Zero); }
        }

        public DeviceContextTextAlignment TextAlignment
        {
            get
            {
                return
                    (DeviceContextTextAlignment)
                        IntUnsafeNativeMethods.GetTextAlign(new HandleRef(this, this.Hdc));
            }
        }

        public Color TextColor
        {
            get
            {
                return
                    ColorTranslator.FromWin32(
                        IntUnsafeNativeMethods.GetTextColor(new HandleRef(this, this.Hdc)));
            }
        }

        public Size ViewportExtent
        {
            get
            {
                IntNativeMethods.SIZE lpSize = new IntNativeMethods.SIZE();
                IntUnsafeNativeMethods.GetViewportExtEx(new HandleRef(this, this.Hdc), lpSize);
                return lpSize.ToSize();
            }
            set { this.SetViewportExtent(value); }
        }

        public Point ViewportOrigin
        {
            get
            {
                IntNativeMethods.POINT lpPoint = new IntNativeMethods.POINT();
                IntUnsafeNativeMethods.GetViewportOrgEx(new HandleRef(this, this.Hdc), lpPoint);
                return lpPoint.ToPoint();
            }
            set { this.SetViewportOrigin(value); }
        }

        internal class GraphicsState
        {
            internal WeakReference font;
            internal IntPtr hBitmap;
            internal IntPtr hBrush;
            internal IntPtr hFont;
            internal IntPtr hPen;
        }
    }

    internal enum DeviceContextTextAlignment
    {
        BaseLine = 0x18,
        Bottom = 8,
        Center = 6,
        Default = 0,
        Left = 0,
        NoUpdateCP = 0,
        Right = 2,
        RtlReading = 0x100,
        Top = 0,
        UpdateCP = 1,
        VerticalBaseLine = 2,
        VerticalCenter = 3
    }

    [Flags]
    internal enum WindowsPenStyle
    {
        Alternate = 8,
        Cosmetic = 0,
        Dash = 1,
        DashDot = 3,
        DashDotDot = 4,
        Default = 0,
        Dot = 2,
        EndcapFlat = 0x200,
        EndcapRound = 0,
        EndcapSquare = 0x100,
        Geometric = 0x10000,
        InsideFrame = 6,
        JoinBevel = 0x1000,
        JoinMiter = 0x2000,
        JoinRound = 0,
        Null = 5,
        Solid = 0,
        UserStyle = 7
    }

    internal abstract class WindowsBrush : MarshalByRefObject, ICloneable, IDisposable
    {
        private readonly Color color;
        private readonly DeviceContext dc;
        private IntPtr nativeHandle;

        public WindowsBrush(DeviceContext dc)
        {
            this.color = Color.White;
            this.dc = dc;
        }

        public WindowsBrush(DeviceContext dc, Color color)
        {
            this.color = Color.White;
            this.dc = dc;
            this.color = color;
        }

        public abstract object Clone();

        protected abstract void CreateBrush();

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((this.dc != null) && (this.nativeHandle != IntPtr.Zero)) {
                this.dc.DeleteObject(this.nativeHandle, GdiObjectType.Brush);
                this.nativeHandle = IntPtr.Zero;
            }
            if (disposing)
                GC.SuppressFinalize(this);
        }

        ~WindowsBrush()
        {
            this.Dispose(false);
        }

        public Color Color
        {
            get { return this.color; }
        }

        protected DeviceContext DC
        {
            get { return this.dc; }
        }

        public IntPtr HBrush
        {
            get { return this.NativeHandle; }
        }

        protected IntPtr NativeHandle
        {
            get
            {
                if (this.nativeHandle == IntPtr.Zero)
                    this.CreateBrush();
                return this.nativeHandle;
            }
            set { this.nativeHandle = value; }
        }
    }

    internal enum DeviceContextGraphicsMode
    {
        Advanced = 2,
        Compatible = 1,
        ModifyWorldIdentity = 1
    }

    internal enum RegionCombineMode
    {
        AND = 1,
        COPY = 5,
        DIFF = 4,
        MAX = 5,
        MIN = 1,
        OR = 2,
        XOR = 3
    }

    internal sealed class WindowsRegion : MarshalByRefObject, ICloneable, IDisposable
    {
        private IntPtr nativeHandle;
        private bool ownHandle;

        private WindowsRegion()
        {
        }

        public WindowsRegion(Rectangle rect)
        {
            this.CreateRegion(rect);
        }

        public WindowsRegion(int x, int y, int width, int height)
        {
            this.CreateRegion(new Rectangle(x, y, width, height));
        }

        public object Clone()
        {
            if (!this.IsInfinite)
                return new WindowsRegion(this.ToRectangle());
            return new WindowsRegion();
        }

        public IntNativeMethods.RegionFlags CombineRegion(
            WindowsRegion region1,
            WindowsRegion region2,
            RegionCombineMode mode)
        {
            return IntUnsafeNativeMethods.CombineRgn(
                new HandleRef(this, this.HRegion),
                new HandleRef(region1, region1.HRegion),
                new HandleRef(region2, region2.HRegion),
                mode);
        }

        private void CreateRegion(Rectangle rect)
        {
            this.nativeHandle = IntSafeNativeMethods.CreateRectRgn(
                rect.X,
                rect.Y,
                rect.X + rect.Width,
                rect.Y + rect.Height);
            this.ownHandle = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (this.nativeHandle != IntPtr.Zero) {
                if (this.ownHandle)
                    IntUnsafeNativeMethods.DeleteObject(new HandleRef(this, this.nativeHandle));
                this.nativeHandle = IntPtr.Zero;
                if (disposing)
                    GC.SuppressFinalize(this);
            }
        }

        ~WindowsRegion()
        {
            this.Dispose(false);
        }

        public static WindowsRegion FromHregion(IntPtr hRegion, bool takeOwnership)
        {
            WindowsRegion region = new WindowsRegion();
            if (hRegion != IntPtr.Zero) {
                region.nativeHandle = hRegion;
                if (takeOwnership) {
                    region.ownHandle = true;
                    HandleCollector.Add(hRegion, IntSafeNativeMethods.CommonHandles.GDI);
                }
            }
            return region;
        }

        public static WindowsRegion FromRegion(Region region, Graphics g)
        {
            if (region.IsInfinite(g))
                return new WindowsRegion();
            return FromHregion(region.GetHrgn(g), true);
        }

        public Rectangle ToRectangle()
        {
            if (this.IsInfinite)
                return new Rectangle(-2147483647, -2147483647, 0x7fffffff, 0x7fffffff);
            IntNativeMethods.RECT clipRect = new IntNativeMethods.RECT();
            IntUnsafeNativeMethods.GetRgnBox(new HandleRef(this, this.nativeHandle), ref clipRect);
            return new Rectangle(new Point(clipRect.left, clipRect.top), clipRect.Size);
        }

        public IntPtr HRegion
        {
            get { return this.nativeHandle; }
        }

        public bool IsInfinite
        {
            get { return (this.nativeHandle == IntPtr.Zero); }
        }
    }

    [Flags]
    internal enum IntTextFormatFlags
    {
        Bottom = 8,
        CalculateRectangle = 0x400,
        Default = 0,
        EndEllipsis = 0x8000,
        ExpandTabs = 0x40,
        ExternalLeading = 0x200,
        HidePrefix = 0x100000,
        HorizontalCenter = 1,
        Internal = 0x1000,
        Left = 0,
        ModifyString = 0x10000,
        NoClipping = 0x100,
        NoFullWidthCharacterBreak = 0x80000,
        NoPrefix = 0x800,
        PathEllipsis = 0x4000,
        PrefixOnly = 0x200000,
        Right = 2,
        RightToLeft = 0x20000,
        SingleLine = 0x20,
        TabStop = 0x80,
        TextBoxControl = 0x2000,
        Top = 0,
        VerticalCenter = 4,
        WordBreak = 0x10,
        WordEllipsis = 0x40000
    }

    internal sealed class WindowsPen : MarshalByRefObject, ICloneable, IDisposable
    {
        private readonly Color color;
        private const int cosmeticPenWidth = 1;
        private const int dashStyleMask = 15;
        private readonly DeviceContext dc;
        private const int endCapMask = 0xf00;
        private const int joinMask = 0xf000;
        private IntPtr nativeHandle;
        private WindowsPenStyle style;
        private readonly int width;
        private WindowsBrush wndBrush;

        public WindowsPen(DeviceContext dc)
            : this(dc, WindowsPenStyle.Cosmetic, 1, Color.Black)
        {
        }

        public WindowsPen(DeviceContext dc, Color color)
            : this(dc, WindowsPenStyle.Cosmetic, 1, color)
        {
        }

        public WindowsPen(DeviceContext dc, WindowsBrush windowsBrush)
            : this(dc, WindowsPenStyle.Cosmetic, 1, windowsBrush)
        {
        }

        public WindowsPen(DeviceContext dc, WindowsPenStyle style, int width, Color color)
        {
            this.style = style;
            this.width = width;
            this.color = color;
            this.dc = dc;
        }

        public WindowsPen(DeviceContext dc, WindowsPenStyle style, int width, WindowsBrush windowsBrush)
        {
            this.style = style;
            this.wndBrush = (WindowsBrush)windowsBrush.Clone();
            this.width = width;
            this.color = windowsBrush.Color;
            this.dc = dc;
        }

        public object Clone()
        {
            if (this.wndBrush == null)
                return new WindowsPen(this.dc, this.style, this.width, this.color);
            return new WindowsPen(this.dc, this.style, this.width, (WindowsBrush)this.wndBrush.Clone());
        }

        private void CreatePen()
        {
            if (this.width > 1)
                this.style |= WindowsPenStyle.Geometric;
            if (this.wndBrush == null) {
                this.nativeHandle = IntSafeNativeMethods.CreatePen(
                    (int)this.style,
                    this.width,
                    ColorTranslator.ToWin32(this.color));
            } else {
                IntNativeMethods.LOGBRUSH lplb = new IntNativeMethods.LOGBRUSH {
                    lbColor = ColorTranslator.ToWin32(this.wndBrush.Color),
                    lbStyle = 0,
                    lbHatch = 0
                };
                this.nativeHandle = IntSafeNativeMethods.ExtCreatePen(
                    (int)this.style,
                    this.width,
                    lplb,
                    0,
                    null);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if ((this.nativeHandle != IntPtr.Zero) && (this.dc != null)) {
                this.dc.DeleteObject(this.nativeHandle, GdiObjectType.Pen);
                this.nativeHandle = IntPtr.Zero;
            }
            if (this.wndBrush != null) {
                this.wndBrush.Dispose();
                this.wndBrush = null;
            }
            if (disposing)
                GC.SuppressFinalize(this);
        }

        ~WindowsPen()
        {
            this.Dispose(false);
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}: Style={1}, Color={2}, Width={3}, Brush={4}",
                new object[] {
                    base.GetType().Name, this.style, this.color, this.width,
                    (this.wndBrush != null) ? this.wndBrush.ToString() : "null"
                });
        }

        public IntPtr HPen
        {
            get
            {
                if (this.nativeHandle == IntPtr.Zero)
                    this.CreatePen();
                return this.nativeHandle;
            }
        }
    }

    internal sealed class WindowsGraphics : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private DeviceContext dc;
        private bool disposeDc;
        public const int GdiUnsupportedFlagMask = -16777216;
        private Graphics graphics;
        private const float ItalicPaddingFactor = 0.5f;
        public static readonly Size MaxSize = new Size(0x7fffffff, 0x7fffffff);
        private TextPaddingOptions paddingFlags;

        public WindowsGraphics(DeviceContext dc)
        {
            this.dc = dc;
            this.dc.SaveHdc();
        }

        public static Rectangle AdjustForVerticalAlignment(
            HandleRef hdc,
            string text,
            Rectangle bounds,
            IntTextFormatFlags flags,
            IntNativeMethods.DRAWTEXTPARAMS dtparams)
        {
            if (((((flags & IntTextFormatFlags.Bottom) == IntTextFormatFlags.Default) &&
                  ((flags & IntTextFormatFlags.VerticalCenter) == IntTextFormatFlags.Default)) ||
                 ((flags & IntTextFormatFlags.SingleLine) != IntTextFormatFlags.Default)) ||
                ((flags & IntTextFormatFlags.CalculateRectangle) != IntTextFormatFlags.Default))
                return bounds;
            IntNativeMethods.RECT lpRect = new IntNativeMethods.RECT(bounds);
            flags |= IntTextFormatFlags.CalculateRectangle;
            int num = IntUnsafeNativeMethods.DrawTextEx(hdc, text, ref lpRect, (int)flags, dtparams);
            if (num > bounds.Height)
                return bounds;
            Rectangle rectangle = bounds;
            if ((flags & IntTextFormatFlags.VerticalCenter) != IntTextFormatFlags.Default) {
                rectangle.Y = (rectangle.Top + (rectangle.Height / 2)) - (num / 2);
                return rectangle;
            }
            rectangle.Y = rectangle.Bottom - num;
            return rectangle;
        }

        public static WindowsGraphics CreateMeasurementWindowsGraphics()
        {
            return new WindowsGraphics(DeviceContext.FromCompatibleDC(IntPtr.Zero)) { disposeDc = true };
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (this.dc != null) {
                try {
                    this.dc.RestoreHdc();
                    if (this.disposeDc)
                        this.dc.Dispose(disposing);
                    if (this.graphics != null) {
                        this.graphics.ReleaseHdcInternal(this.dc.Hdc);
                        this.graphics = null;
                    }
                } catch (Exception exception) {
                    if (ClientUtils.IsSecurityOrCriticalException(exception))
                        throw;
                } finally {
                    this.dc = null;
                }
            }
        }

        public void DrawAndFillEllipse(WindowsPen pen, WindowsBrush brush, Rectangle bounds)
        {
            this.DrawEllipse(pen, brush, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }

        private void DrawEllipse(
            WindowsPen pen,
            WindowsBrush brush,
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(pen, pen.HPen));
            if (brush != null)
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(brush, brush.HBrush));
            IntUnsafeNativeMethods.Ellipse(hdc, nLeftRect, nTopRect, nRightRect, nBottomRect);
        }

        public void DrawLine(WindowsPen pen, Point p1, Point p2)
        {
            this.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawLine(WindowsPen pen, int x1, int y1, int x2, int y2)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            DeviceContextBinaryRasterOperationFlags binaryRasterOperation = this.dc.BinaryRasterOperation;
            DeviceContextBackgroundMode backgroundMode = this.dc.BackgroundMode;
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen) {
                binaryRasterOperation =
                    this.dc.SetRasterOperation(DeviceContextBinaryRasterOperationFlags.CopyPen);
            }
            if (backgroundMode != DeviceContextBackgroundMode.Transparent)
                backgroundMode = this.dc.SetBackgroundMode(DeviceContextBackgroundMode.Transparent);
            if (pen != null)
                this.dc.SelectObject(pen.HPen, GdiObjectType.Pen);
            IntNativeMethods.POINT pt = new IntNativeMethods.POINT();
            IntUnsafeNativeMethods.MoveToEx(hdc, x1, y1, pt);
            IntUnsafeNativeMethods.LineTo(hdc, x2, y2);
            if (backgroundMode != DeviceContextBackgroundMode.Transparent)
                this.dc.SetBackgroundMode(backgroundMode);
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
                this.dc.SetRasterOperation(binaryRasterOperation);
            IntUnsafeNativeMethods.MoveToEx(hdc, pt.x, pt.y, null);
        }

        public void DrawPie(WindowsPen pen, Rectangle bounds, float startAngle, float sweepAngle)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
                IntUnsafeNativeMethods.SelectObject(hdc, new HandleRef(pen, pen.HPen));
            int num = Math.Min(bounds.Width, bounds.Height);
            Point point = new Point(bounds.X + (num / 2), bounds.Y + (num / 2));
            int radius = num / 2;
            IntUnsafeNativeMethods.BeginPath(hdc);
            IntUnsafeNativeMethods.MoveToEx(hdc, point.X, point.Y, null);
            IntUnsafeNativeMethods.AngleArc(hdc, point.X, point.Y, radius, startAngle, sweepAngle);
            IntUnsafeNativeMethods.LineTo(hdc, point.X, point.Y);
            IntUnsafeNativeMethods.EndPath(hdc);
            IntUnsafeNativeMethods.StrokePath(hdc);
        }

        public void DrawRectangle(WindowsPen pen, Rectangle rect)
        {
            this.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(WindowsPen pen, int x, int y, int width, int height)
        {
            HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
            if (pen != null)
                this.dc.SelectObject(pen.HPen, GdiObjectType.Pen);
            DeviceContextBinaryRasterOperationFlags binaryRasterOperation = this.dc.BinaryRasterOperation;
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen) {
                binaryRasterOperation =
                    this.dc.SetRasterOperation(DeviceContextBinaryRasterOperationFlags.CopyPen);
            }
            IntUnsafeNativeMethods.SelectObject(
                hdc,
                new HandleRef(null, IntUnsafeNativeMethods.GetStockObject(5)));
            IntUnsafeNativeMethods.Rectangle(hdc, x, y, x + width, y + height);
            if (binaryRasterOperation != DeviceContextBinaryRasterOperationFlags.CopyPen)
                this.dc.SetRasterOperation(binaryRasterOperation);
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor)
        {
            this.DrawText(text, font, pt, foreColor, Color.Empty, IntTextFormatFlags.Default);
        }

        public void DrawText(string text, WindowsFont font, Rectangle bounds, Color foreColor)
        {
            this.DrawText(text, font, bounds, foreColor, Color.Empty);
        }

        public void DrawText(string text, WindowsFont font, Point pt, Color foreColor, Color backColor)
        {
            this.DrawText(text, font, pt, foreColor, backColor, IntTextFormatFlags.Default);
        }

        public void DrawText(
            string text,
            WindowsFont font,
            Point pt,
            Color foreColor,
            IntTextFormatFlags flags)
        {
            this.DrawText(text, font, pt, foreColor, Color.Empty, flags);
        }

        public void DrawText(
            string text,
            WindowsFont font,
            Rectangle bounds,
            Color foreColor,
            Color backColor)
        {
            this.DrawText(
                text,
                font,
                bounds,
                foreColor,
                backColor,
                IntTextFormatFlags.VerticalCenter | IntTextFormatFlags.HorizontalCenter);
        }

        public void DrawText(
            string text,
            WindowsFont font,
            Rectangle bounds,
            Color color,
            IntTextFormatFlags flags)
        {
            this.DrawText(text, font, bounds, color, Color.Empty, flags);
        }

        public void DrawText(
            string text,
            WindowsFont font,
            Point pt,
            Color foreColor,
            Color backColor,
            IntTextFormatFlags flags)
        {
            Rectangle bounds = new Rectangle(pt.X, pt.Y, 0x7fffffff, 0x7fffffff);
            this.DrawText(text, font, bounds, foreColor, backColor, flags);
        }

        public void DrawText(
            string text,
            WindowsFont font,
            Rectangle bounds,
            Color foreColor,
            Color backColor,
            IntTextFormatFlags flags)
        {
            if (!string.IsNullOrEmpty(text) && (foreColor != Color.Transparent)) {
                HandleRef hdc = new HandleRef(this.dc, this.dc.Hdc);
                if (this.dc.TextAlignment != DeviceContextTextAlignment.Top)
                    this.dc.SetTextAlignment(DeviceContextTextAlignment.Top);
                if (!foreColor.IsEmpty && (foreColor != this.dc.TextColor))
                    this.dc.SetTextColor(foreColor);
                if (font != null)
                    this.dc.SelectFont(font);
                DeviceContextBackgroundMode newMode = (backColor.IsEmpty || (backColor == Color.Transparent))
                    ? DeviceContextBackgroundMode.Transparent
                    : DeviceContextBackgroundMode.Opaque;
                if (this.dc.BackgroundMode != newMode)
                    this.dc.SetBackgroundMode(newMode);
                if ((newMode != DeviceContextBackgroundMode.Transparent) &&
                    (backColor != this.dc.BackgroundColor))
                    this.dc.SetBackgroundColor(backColor);
                IntNativeMethods.DRAWTEXTPARAMS textMargins = this.GetTextMargins(font);
                bounds = AdjustForVerticalAlignment(hdc, text, bounds, flags, textMargins);
                if (bounds.Width == MaxSize.Width)
                    bounds.Width -= bounds.X;
                if (bounds.Height == MaxSize.Height)
                    bounds.Height -= bounds.Y;
                IntNativeMethods.RECT lpRect = new IntNativeMethods.RECT(bounds);
                IntUnsafeNativeMethods.DrawTextEx(hdc, text, ref lpRect, (int)flags, textMargins);
            }
        }

        public void FillRectangle(WindowsBrush brush, Rectangle rect)
        {
            this.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle(WindowsBrush brush, int x, int y, int width, int height)
        {
            HandleRef hDC = new HandleRef(this.dc, this.dc.Hdc);
            IntPtr hBrush = brush.HBrush;
            IntNativeMethods.RECT rect = new IntNativeMethods.RECT(x, y, x + width, y + height);
            IntUnsafeNativeMethods.FillRect(hDC, ref rect, new HandleRef(brush, hBrush));
        }

        ~WindowsGraphics()
        {
            this.Dispose(false);
        }

        public static WindowsGraphics FromGraphics(Graphics g)
        {
            ApplyGraphicsProperties all = ApplyGraphicsProperties.All;
            return FromGraphics(g, all);
        }

        public static WindowsGraphics FromGraphics(Graphics g, ApplyGraphicsProperties properties)
        {
            WindowsRegion wr = null;
            float[] elements = null;
            Region region = null;
            Matrix matrix = null;
            if (((properties & ApplyGraphicsProperties.TranslateTransform) != ApplyGraphicsProperties.None) ||
                ((properties & ApplyGraphicsProperties.Clipping) != ApplyGraphicsProperties.None)) {
                object[] contextInfo = g.GetContextInfo() as object[];
                if ((contextInfo != null) && (contextInfo.Length == 2)) {
                    region = contextInfo[0] as Region;
                    matrix = contextInfo[1] as Matrix;
                }
                if (matrix != null) {
                    if ((properties & ApplyGraphicsProperties.TranslateTransform) !=
                        ApplyGraphicsProperties.None)
                        elements = matrix.Elements;
                    matrix.Dispose();
                }
                if (region != null) {
                    if (((properties & ApplyGraphicsProperties.Clipping) != ApplyGraphicsProperties.None) &&
                        !region.IsInfinite(g))
                        wr = WindowsRegion.FromRegion(region, g);
                    region.Dispose();
                }
            }
            WindowsGraphics graphics = FromHdc(g.GetHdc());
            graphics.graphics = g;
            if (wr != null) {
                using (wr)
                    graphics.DeviceContext.IntersectClip(wr);
            }
            if (elements != null)
                graphics.DeviceContext.TranslateTransform((int)elements[4], (int)elements[5]);
            return graphics;
        }

        public static WindowsGraphics FromHdc(IntPtr hDc)
        {
            return new WindowsGraphics(DeviceContext.FromHdc(hDc)) { disposeDc = true };
        }

        public static WindowsGraphics FromHwnd(IntPtr hWnd)
        {
            return new WindowsGraphics(DeviceContext.FromHwnd(hWnd)) { disposeDc = true };
        }

        public IntPtr GetHdc()
        {
            return this.dc.Hdc;
        }

        public Color GetNearestColor(Color color)
        {
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            return
                ColorTranslator.FromWin32(
                    IntUnsafeNativeMethods.GetNearestColor(hDC, ColorTranslator.ToWin32(color)));
        }

        public float GetOverhangPadding(WindowsFont font)
        {
            WindowsFont font2 = font;
            if (font2 == null)
                font2 = this.dc.Font;
            float num = ((float)font2.Height) / 6f;
            if (font2 != font)
                font2.Dispose();
            return num;
        }

        public Size GetTextExtent(string text, WindowsFont font)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;
            IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            if (font != null)
                this.dc.SelectFont(font);
            IntUnsafeNativeMethods.GetTextExtentPoint32(hDC, text, size);
            if ((font != null) && !MeasurementDCInfo.IsMeasurementDC(this.dc))
                this.dc.ResetFont();
            return new Size(size.cx, size.cy);
        }

        public IntNativeMethods.DRAWTEXTPARAMS GetTextMargins(WindowsFont font)
        {
            int leftMargin = 0;
            int rightMargin = 0;
            float overhangPadding = 0f;
            switch (this.TextPadding) {
                case TextPaddingOptions.GlyphOverhangPadding:
                    overhangPadding = this.GetOverhangPadding(font);
                    leftMargin = (int)Math.Ceiling((double)overhangPadding);
                    rightMargin = (int)Math.Ceiling((double)(overhangPadding * 1.5f));
                    break;

                case TextPaddingOptions.LeftAndRightPadding:
                    overhangPadding = this.GetOverhangPadding(font);
                    leftMargin = (int)Math.Ceiling((double)(2f * overhangPadding));
                    rightMargin = (int)Math.Ceiling((double)(overhangPadding * 2.5f));
                    break;
            }
            return new IntNativeMethods.DRAWTEXTPARAMS(leftMargin, rightMargin);
        }

        public IntNativeMethods.TEXTMETRIC GetTextMetrics()
        {
            IntNativeMethods.TEXTMETRIC lptm = new IntNativeMethods.TEXTMETRIC();
            HandleRef hDC = new HandleRef(this.dc, this.dc.Hdc);
            bool flag = this.dc.MapMode != DeviceContextMapMode.Text;
            if (flag)
                this.dc.SaveHdc();
            try {
                if (flag) {
                    DeviceContextMapMode mode = this.dc.SetMapMode(DeviceContextMapMode.Text);
                }
                IntUnsafeNativeMethods.GetTextMetrics(hDC, ref lptm);
            } finally {
                if (flag)
                    this.dc.RestoreHdc();
            }
            return lptm;
        }

        public Size MeasureText(string text, WindowsFont font)
        {
            return this.MeasureText(text, font, MaxSize, IntTextFormatFlags.Default);
        }

        public Size MeasureText(string text, WindowsFont font, Size proposedSize)
        {
            return this.MeasureText(text, font, proposedSize, IntTextFormatFlags.Default);
        }

        public Size MeasureText(string text, WindowsFont font, Size proposedSize, IntTextFormatFlags flags)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;
            IntNativeMethods.DRAWTEXTPARAMS lpDTParams = null;
            if (MeasurementDCInfo.IsMeasurementDC(this.DeviceContext))
                lpDTParams = MeasurementDCInfo.GetTextMargins(this, font);
            if (lpDTParams == null)
                lpDTParams = this.GetTextMargins(font);
            int num = (1 + lpDTParams.iLeftMargin) + lpDTParams.iRightMargin;
            if (proposedSize.Width <= num)
                proposedSize.Width = num;
            if (proposedSize.Height <= 0)
                proposedSize.Height = 1;
            IntNativeMethods.RECT lpRect = IntNativeMethods.RECT.FromXYWH(
                0,
                0,
                proposedSize.Width,
                proposedSize.Height);
            HandleRef hDC = new HandleRef(null, this.dc.Hdc);
            if (font != null)
                this.dc.SelectFont(font);
            if ((proposedSize.Height >= MaxSize.Height) &&
                ((flags & IntTextFormatFlags.SingleLine) != IntTextFormatFlags.Default))
                flags &= ~(IntTextFormatFlags.Bottom | IntTextFormatFlags.VerticalCenter);
            if (proposedSize.Width == MaxSize.Width)
                flags &= ~IntTextFormatFlags.WordBreak;
            flags |= IntTextFormatFlags.CalculateRectangle;
            IntUnsafeNativeMethods.DrawTextEx(hDC, text, ref lpRect, (int)flags, lpDTParams);
            return lpRect.Size;
        }

        public void ReleaseHdc()
        {
            this.dc.Dispose();
        }

        public DeviceContext DeviceContext
        {
            get { return this.dc; }
        }

        public TextPaddingOptions TextPadding
        {
            get { return this.paddingFlags; }
            set
            {
                if (this.paddingFlags != value)
                    this.paddingFlags = value;
            }
        }
    }

    public class ClientUtils
    {
        public static bool IsCriticalException(Exception ex)
        {
            return
                ex is NullReferenceException ||
                ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is ThreadAbortException ||
                ex is IndexOutOfRangeException ||
                ex is AccessViolationException;
        }

        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            return ex is SecurityException || IsCriticalException(ex);
        }

        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            return ((value >= minValue) && (value <= maxValue));
        }

        public static bool IsEnumValid(
            Enum enumValue,
            int value,
            int minValue,
            int maxValue,
            int maxNumberOfBitsOn)
        {
            return (((value >= minValue) && (value <= maxValue)) &&
                    (GetBitCount((uint)value) <= maxNumberOfBitsOn));
        }

        public static bool IsEnumValid_Masked(Enum enumValue, int value, uint mask)
        {
            return ((value & mask) == value);
        }

        public static bool IsEnumValid_NotSequential(Enum enumValue, int value, params int[] enumValues)
        {
            for (int i = 0; i < enumValues.Length; i++) {
                if (enumValues[i] == value)
                    return true;
            }
            return false;
        }

        public static int GetBitCount(uint x)
        {
            int num = 0;
            while (x > 0) {
                x &= x - 1;
                num++;
            }
            return num;
        }

        internal class WeakRefCollection : IList, ICollection, IEnumerable
        {
            private readonly ArrayList _innerList;
            private int refCheckThreshold;

            internal WeakRefCollection()
            {
                this.refCheckThreshold = 0x7fffffff;
                this._innerList = new ArrayList(4);
            }

            internal WeakRefCollection(int size)
            {
                this.refCheckThreshold = 0x7fffffff;
                this._innerList = new ArrayList(size);
            }

            public int Add(object value)
            {
                if (this.Count > this.RefCheckThreshold)
                    this.ScavengeReferences();
                return this.InnerList.Add(this.CreateWeakRefObject(value));
            }

            public void Clear()
            {
                this.InnerList.Clear();
            }

            public bool Contains(object value)
            {
                return this.InnerList.Contains(this.CreateWeakRefObject(value));
            }

            private static void Copy(
                WeakRefCollection sourceList,
                int sourceIndex,
                WeakRefCollection destinationList,
                int destinationIndex,
                int length)
            {
                if (sourceIndex < destinationIndex) {
                    sourceIndex += length;
                    destinationIndex += length;
                    while (length > 0) {
                        destinationList.InnerList[--destinationIndex] = sourceList.InnerList[--sourceIndex];
                        length--;
                    }
                } else {
                    while (length > 0) {
                        destinationList.InnerList[destinationIndex++] = sourceList.InnerList[sourceIndex++];
                        length--;
                    }
                }
            }

            public void CopyTo(Array array, int index)
            {
                this.InnerList.CopyTo(array, index);
            }

            private WeakRefObject CreateWeakRefObject(object value)
            {
                if (value == null)
                    return null;
                return new WeakRefObject(value);
            }

            public override bool Equals(object obj)
            {
                WeakRefCollection refs = obj as WeakRefCollection;
                if (refs != this) {
                    if ((refs == null) || (this.Count != refs.Count))
                        return false;
                    for (int i = 0; i < this.Count; i++) {
                        if ((this.InnerList[i] != refs.InnerList[i]) &&
                            ((this.InnerList[i] == null) || !this.InnerList[i].Equals(refs.InnerList[i])))
                            return false;
                    }
                }
                return true;
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerList.GetEnumerator();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public int IndexOf(object value)
            {
                return this.InnerList.IndexOf(this.CreateWeakRefObject(value));
            }

            public void Insert(int index, object value)
            {
                this.InnerList.Insert(index, this.CreateWeakRefObject(value));
            }

            public void Remove(object value)
            {
                this.InnerList.Remove(this.CreateWeakRefObject(value));
            }

            public void RemoveAt(int index)
            {
                this.InnerList.RemoveAt(index);
            }

            public void RemoveByHashCode(object value)
            {
                if (value != null) {
                    int hashCode = value.GetHashCode();
                    for (int i = 0; i < this.InnerList.Count; i++) {
                        if ((this.InnerList[i] != null) && (this.InnerList[i].GetHashCode() == hashCode)) {
                            this.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

            public void ScavengeReferences()
            {
                int index = 0;
                int count = this.Count;
                for (int i = 0; i < count; i++) {
                    if (this[index] == null)
                        this.InnerList.RemoveAt(index);
                    else
                        index++;
                }
            }

            public int Count
            {
                get { return this.InnerList.Count; }
            }

            internal ArrayList InnerList
            {
                get { return this._innerList; }
            }

            public bool IsFixedSize
            {
                get { return this.InnerList.IsFixedSize; }
            }

            public bool IsReadOnly
            {
                get { return this.InnerList.IsReadOnly; }
            }

            public object this[int index]
            {
                get
                {
                    WeakRefObject obj2 = this.InnerList[index] as WeakRefObject;
                    if ((obj2 != null) && obj2.IsAlive)
                        return obj2.Target;
                    return null;
                }
                set { this.InnerList[index] = this.CreateWeakRefObject(value); }
            }

            public int RefCheckThreshold
            {
                get { return this.refCheckThreshold; }
                set { this.refCheckThreshold = value; }
            }

            bool ICollection.IsSynchronized
            {
                get { return this.InnerList.IsSynchronized; }
            }

            object ICollection.SyncRoot
            {
                get { return this.InnerList.SyncRoot; }
            }

            internal class WeakRefObject
            {
                private readonly int hash;
                private readonly WeakReference weakHolder;

                internal WeakRefObject(object obj)
                {
                    this.weakHolder = new WeakReference(obj);
                    this.hash = obj.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    WeakRefObject obj2 =
                        obj as WeakRefObject;
                    if (obj2 == this)
                        return true;
                    if (obj2 == null)
                        return false;
                    return ((obj2.Target == this.Target) ||
                            ((this.Target != null) && this.Target.Equals(obj2.Target)));
                }

                public override int GetHashCode()
                {
                    return this.hash;
                }

                internal bool IsAlive
                {
                    get { return this.weakHolder.IsAlive; }
                }

                internal object Target
                {
                    get { return this.weakHolder.Target; }
                }
            }
        }
    }

    internal class IntNativeMethods
    {
        public const int ALTERNATE = 1;
        public const int ANSI_CHARSET = 0;
        public const int ANTIALIASED_QUALITY = 4;
        public const int BI_BITFIELDS = 3;
        public const int BI_RGB = 0;
        public const int BITMAPINFO_MAX_COLORSIZE = 0x100;
        public const int BITSPIXEL = 12;
        public const int BLACKNESS = 0x42;
        public const int BS_HATCHED = 2;
        public const int BS_SOLID = 0;
        public const int CAPTUREBLT = 0x40000000;
        public const int CLEARTYPE_NATURAL_QUALITY = 6;
        public const int CLEARTYPE_QUALITY = 5;
        public const int CLIP_DEFAULT_PRECIS = 0;
        public const int CP_ACP = 0;
        public const int DEFAULT_CHARSET = 1;
        public const int DEFAULT_GUI_FONT = 0x11;
        public const int DEFAULT_QUALITY = 0;
        public const int DIB_RGB_COLORS = 0;
        public const int DRAFT_QUALITY = 1;
        public const int DSTINVERT = 0x550009;
        public const int DT_BOTTOM = 8;
        public const int DT_CALCRECT = 0x400;
        public const int DT_CENTER = 1;
        public const int DT_EDITCONTROL = 0x2000;
        public const int DT_END_ELLIPSIS = 0x8000;
        public const int DT_EXPANDTABS = 0x40;
        public const int DT_EXTERNALLEADING = 0x200;
        public const int DT_HIDEPREFIX = 0x100000;
        public const int DT_INTERNAL = 0x1000;
        public const int DT_LEFT = 0;
        public const int DT_MODIFYSTRING = 0x10000;
        public const int DT_NOCLIP = 0x100;
        public const int DT_NOFULLWIDTHCHARBREAK = 0x80000;
        public const int DT_NOPREFIX = 0x800;
        public const int DT_PATH_ELLIPSIS = 0x4000;
        public const int DT_PREFIXONLY = 0x200000;
        public const int DT_RIGHT = 2;
        public const int DT_RTLREADING = 0x20000;
        public const int DT_SINGLELINE = 0x20;
        public const int DT_TABSTOP = 0x80;
        public const int DT_TOP = 0;
        public const int DT_VCENTER = 4;
        public const int DT_WORD_ELLIPSIS = 0x40000;
        public const int DT_WORDBREAK = 0x10;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_DEFAULT = 0x1200;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FW_BOLD = 700;
        public const int FW_DONTCARE = 0;
        public const int FW_NORMAL = 400;
        public const int HOLLOW_BRUSH = 5;
        public const int MaxTextLengthInWin9x = 0x2000;
        public const int MERGECOPY = 0xc000ca;
        public const int MERGEPAINT = 0xbb0226;
        public const int NONANTIALIASED_QUALITY = 3;
        public const int NOTSRCCOPY = 0x330008;
        public const int NOTSRCERASE = 0x1100a6;
        public const int OBJ_BITMAP = 7;
        public const int OBJ_BRUSH = 2;
        public const int OBJ_DC = 3;
        public const int OBJ_ENHMETADC = 12;
        public const int OBJ_EXTPEN = 11;
        public const int OBJ_FONT = 6;
        public const int OBJ_MEMDC = 10;
        public const int OBJ_METADC = 4;
        public const int OBJ_PEN = 1;
        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_TT_PRECIS = 4;
        public const int PATCOPY = 0xf00021;
        public const int PATINVERT = 0x5a0049;
        public const int PATPAINT = 0xfb0a09;
        public const int PROOF_QUALITY = 2;
        public const int SPI_GETICONTITLELOGFONT = 0x1f;
        public const int SPI_GETNONCLIENTMETRICS = 0x29;
        public const int SRCAND = 0x8800c6;
        public const int SRCCOPY = 0xcc0020;
        public const int SRCERASE = 0x440328;
        public const int SRCINVERT = 0x660046;
        public const int SRCPAINT = 0xee0086;
        public const int WHITENESS = 0xff0062;
        public const int WINDING = 2;

        [StructLayout(LayoutKind.Sequential)]
        public class DRAWTEXTPARAMS
        {
            private int cbSize;
            public int iTabLength;
            public int iLeftMargin;
            public int iRightMargin;
            public int uiLengthDrawn;

            public DRAWTEXTPARAMS()
            {
                this.cbSize = Marshal.SizeOf(typeof(DRAWTEXTPARAMS));
            }

            public DRAWTEXTPARAMS(DRAWTEXTPARAMS original)
            {
                this.cbSize = Marshal.SizeOf(typeof(DRAWTEXTPARAMS));
                this.iLeftMargin = original.iLeftMargin;
                this.iRightMargin = original.iRightMargin;
                this.iTabLength = original.iTabLength;
            }

            public DRAWTEXTPARAMS(int leftMargin, int rightMargin)
            {
                this.cbSize = Marshal.SizeOf(typeof(DRAWTEXTPARAMS));
                this.iLeftMargin = leftMargin;
                this.iRightMargin = rightMargin;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class LOGBRUSH
        {
            public int lbStyle;
            public int lbColor;
            public int lbHatch;
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
                this.lfHeight = lf.lfHeight;
                this.lfWidth = lf.lfWidth;
                this.lfEscapement = lf.lfEscapement;
                this.lfOrientation = lf.lfOrientation;
                this.lfWeight = lf.lfWeight;
                this.lfItalic = lf.lfItalic;
                this.lfUnderline = lf.lfUnderline;
                this.lfStrikeOut = lf.lfStrikeOut;
                this.lfCharSet = lf.lfCharSet;
                this.lfOutPrecision = lf.lfOutPrecision;
                this.lfClipPrecision = lf.lfClipPrecision;
                this.lfQuality = lf.lfQuality;
                this.lfPitchAndFamily = lf.lfPitchAndFamily;
                this.lfFaceName = lf.lfFaceName;
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

            public Point ToPoint()
            {
                return new Point(this.x, this.y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rectangle r)
            {
                this.left = r.Left;
                this.top = r.Top;
                this.right = r.Right;
                this.bottom = r.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public Size Size
            {
                get { return new Size(this.right - this.left, this.bottom - this.top); }
            }

            public Rectangle ToRectangle()
            {
                return new Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
            }
        }

        public enum RegionFlags
        {
            ERROR,
            NULLREGION,
            SIMPLEREGION,
            COMPLEXREGION
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SIZE
        {
            public int cx;
            public int cy;

            public SIZE()
            {
            }

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }

            public Size ToSize()
            {
                return new Size(this.cx, this.cy);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TEXTMETRICA
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public byte tmFirstChar;
            public byte tmLastChar;
            public byte tmDefaultChar;
            public byte tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }
    }

    internal delegate void HandleChangeEventHandler(
        string handleType,
        IntPtr handleValue,
        int currentHandleCount);

    internal sealed class HandleCollector
    {
        private static int handleTypeCount;
        private static HandleType[] handleTypes;
        private static readonly object internalSyncObject = new object();
        private static int suspendCount;

        internal static event HandleChangeEventHandler HandleAdded;

        internal static event HandleChangeEventHandler HandleRemoved;

        internal static IntPtr Add(IntPtr handle, int type)
        {
            handleTypes[type - 1].Add(handle);
            return handle;
        }

        internal static int RegisterType(string typeName, int expense, int initialThreshold)
        {
            lock (internalSyncObject) {
                if ((handleTypeCount == 0) || (handleTypeCount == handleTypes.Length)) {
                    HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
                    if (handleTypes != null)
                        Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
                    handleTypes = destinationArray;
                }
                handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                return handleTypeCount;
            }
        }

        internal static IntPtr Remove(IntPtr handle, int type)
        {
            return handleTypes[type - 1].Remove(handle);
        }

        internal static void ResumeCollect()
        {
            bool flag = false;
            lock (internalSyncObject) {
                if (suspendCount > 0)
                    suspendCount--;
                if (suspendCount == 0) {
                    for (int i = 0; i < handleTypeCount; i++) {
                        lock (handleTypes[i]) {
                            if (handleTypes[i].NeedCollection())
                                flag = true;
                        }
                    }
                }
            }
            if (flag)
                GC.Collect();
        }

        internal static void SuspendCollect()
        {
            lock (internalSyncObject) {
                suspendCount++;
            }
        }

        private class HandleType
        {
            private readonly int deltaPercent;
            private int handleCount;
            private readonly int initialThreshHold;
            internal readonly string name;
            private int threshHold;

            internal HandleType(string name, int expense, int initialThreshHold)
            {
                this.name = name;
                this.initialThreshHold = initialThreshHold;
                this.threshHold = initialThreshHold;
                this.deltaPercent = 100 - expense;
            }

            internal void Add(IntPtr handle)
            {
                if (handle != IntPtr.Zero) {
                    bool flag = false;
                    int currentHandleCount = 0;
                    lock (this) {
                        this.handleCount++;
                        flag = this.NeedCollection();
                        currentHandleCount = this.handleCount;
                    }
                    lock (internalSyncObject) {
                        if (HandleAdded != null)
                            HandleAdded(this.name, handle, currentHandleCount);
                    }
                    if (flag && flag) {
                        GC.Collect();
                        int millisecondsTimeout = (100 - this.deltaPercent) / 4;
                        Thread.Sleep(millisecondsTimeout);
                    }
                }
            }

            internal int GetHandleCount()
            {
                lock (this) {
                    return this.handleCount;
                }
            }

            internal bool NeedCollection()
            {
                if (suspendCount <= 0) {
                    if (this.handleCount > this.threshHold) {
                        this.threshHold = this.handleCount + ((this.handleCount * this.deltaPercent) / 100);
                        return true;
                    }
                    int num = (100 * this.threshHold) / (100 + this.deltaPercent);
                    if ((num >= this.initialThreshHold) && (this.handleCount < ((int)(num * 0.9f))))
                        this.threshHold = num;
                }
                return false;
            }

            internal IntPtr Remove(IntPtr handle)
            {
                if (handle != IntPtr.Zero) {
                    int currentHandleCount = 0;
                    lock (this) {
                        this.handleCount--;
                        if (this.handleCount < 0)
                            this.handleCount = 0;
                        currentHandleCount = this.handleCount;
                    }
                    lock (internalSyncObject) {
                        if (HandleRemoved != null)
                            HandleRemoved(this.name, handle, currentHandleCount);
                    }
                }
                return handle;
            }
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class IntSafeNativeMethods
    {
        public static IntPtr CreatePen(int fnStyle, int nWidth, int crColor)
        {
            return HandleCollector.Add(IntCreatePen(fnStyle, nWidth, crColor), CommonHandles.GDI);
        }

        public static IntPtr CreateRectRgn(int x1, int y1, int x2, int y2)
        {
            return HandleCollector.Add(IntCreateRectRgn(x1, y1, x2, y2), CommonHandles.GDI);
        }

        public static IntPtr CreateSolidBrush(int crColor)
        {
            return HandleCollector.Add(IntCreateSolidBrush(crColor), CommonHandles.GDI);
        }

        public static IntPtr ExtCreatePen(
            int fnStyle,
            int dwWidth,
            IntNativeMethods.LOGBRUSH lplb,
            int dwStyleCount,
            int[] lpStyle)
        {
            return HandleCollector.Add(
                IntExtCreatePen(fnStyle, dwWidth, lplb, dwStyleCount, lpStyle),
                CommonHandles.GDI);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool GdiFlush();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetUserDefaultLCID();

        [DllImport("gdi32.dll", EntryPoint = "CreatePen", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        private static extern IntPtr IntCreatePen(int fnStyle, int nWidth, int crColor);

        [DllImport("gdi32.dll", EntryPoint = "CreateRectRgn", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntCreateRectRgn(int x1, int y1, int x2, int y2);

        [DllImport("gdi32.dll", EntryPoint = "CreateSolidBrush", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        private static extern IntPtr IntCreateSolidBrush(int crColor);

        [DllImport("gdi32.dll", EntryPoint = "ExtCreatePen", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        private static extern IntPtr IntExtCreatePen(
            int fnStyle,
            int dwWidth,
            IntNativeMethods.LOGBRUSH lplb,
            int dwStyleCount,
            [MarshalAs(UnmanagedType.LPArray)] int[] lpStyle);

        public sealed class CommonHandles
        {
            public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int GDI = HandleCollector.RegisterType("GDI", 90, 50);
            public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class IntUnsafeNativeMethods
    {
        public static bool AngleArc(HandleRef hDC, int x, int y, int radius, float startAngle, float endAngle)
        {
            return IntAngleArc(hDC, x, y, radius, startAngle, endAngle);
        }

        public static bool Arc(
            HandleRef hDC,
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nXStartArc,
            int nYStartArc,
            int nXEndArc,
            int nYEndArc)
        {
            return IntArc(
                hDC,
                nLeftRect,
                nTopRect,
                nRightRect,
                nBottomRect,
                nXStartArc,
                nYStartArc,
                nXEndArc,
                nYEndArc);
        }

        public static bool BeginPath(HandleRef hDC)
        {
            return IntBeginPath(hDC);
        }

        public static IntNativeMethods.RegionFlags CombineRgn(
            HandleRef hRgnDest,
            HandleRef hRgnSrc1,
            HandleRef hRgnSrc2,
            RegionCombineMode combineMode)
        {
            if (((hRgnDest.Wrapper != null) && (hRgnSrc1.Wrapper != null)) && (hRgnSrc2.Wrapper != null))
                return IntCombineRgn(hRgnDest, hRgnSrc1, hRgnSrc2, combineMode);
            return IntNativeMethods.RegionFlags.ERROR;
        }

        public static IntPtr CreateCompatibleDC(HandleRef hDC)
        {
            return HandleCollector.Add(IntCreateCompatibleDC(hDC), IntSafeNativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateDC(
            string lpszDriverName,
            string lpszDeviceName,
            string lpszOutput,
            HandleRef lpInitData)
        {
            return HandleCollector.Add(
                IntCreateDC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData),
                IntSafeNativeMethods.CommonHandles.HDC);
        }

        public static IntPtr CreateFontIndirect(object lf)
        {
            return HandleCollector.Add(IntCreateFontIndirect(lf), IntSafeNativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateIC(
            string lpszDriverName,
            string lpszDeviceName,
            string lpszOutput,
            HandleRef lpInitData)
        {
            return HandleCollector.Add(
                IntCreateIC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData),
                IntSafeNativeMethods.CommonHandles.HDC);
        }

        public static bool DeleteDC(HandleRef hDC)
        {
            HandleCollector.Remove((IntPtr)hDC, IntSafeNativeMethods.CommonHandles.GDI);
            return IntDeleteDC(hDC);
        }

        public static bool DeleteHDC(HandleRef hDC)
        {
            HandleCollector.Remove((IntPtr)hDC, IntSafeNativeMethods.CommonHandles.HDC);
            return IntDeleteDC(hDC);
        }

        public static bool DeleteObject(HandleRef hObject)
        {
            HandleCollector.Remove((IntPtr)hObject, IntSafeNativeMethods.CommonHandles.GDI);
            return IntDeleteObject(hObject);
        }

        public static int DrawText(HandleRef hDC, string text, ref IntNativeMethods.RECT lpRect, int nFormat)
        {
            if (Marshal.SystemDefaultCharSize == 1) {
                lpRect.top = Math.Min(0x7fff, lpRect.top);
                lpRect.left = Math.Min(0x7fff, lpRect.left);
                lpRect.right = Math.Min(0x7fff, lpRect.right);
                lpRect.bottom = Math.Min(0x7fff, lpRect.bottom);
                int num2 = WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                byte[] pOutBytes = new byte[num2];
                WideCharToMultiByte(
                    0,
                    0,
                    text,
                    text.Length,
                    pOutBytes,
                    pOutBytes.Length,
                    IntPtr.Zero,
                    IntPtr.Zero);
                num2 = Math.Min(num2, 0x2000);
                return DrawTextA(hDC, pOutBytes, num2, ref lpRect, nFormat);
            }
            return DrawTextW(hDC, text, text.Length, ref lpRect, nFormat);
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int DrawTextA(
            HandleRef hDC,
            byte[] lpszString,
            int byteCount,
            ref IntNativeMethods.RECT lpRect,
            int nFormat);

        public static int DrawTextEx(
            HandleRef hDC,
            string text,
            ref IntNativeMethods.RECT lpRect,
            int nFormat,
            [In] [Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams)
        {
            if (Marshal.SystemDefaultCharSize == 1) {
                lpRect.top = Math.Min(0x7fff, lpRect.top);
                lpRect.left = Math.Min(0x7fff, lpRect.left);
                lpRect.right = Math.Min(0x7fff, lpRect.right);
                lpRect.bottom = Math.Min(0x7fff, lpRect.bottom);
                int num2 = WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                byte[] pOutBytes = new byte[num2];
                WideCharToMultiByte(
                    0,
                    0,
                    text,
                    text.Length,
                    pOutBytes,
                    pOutBytes.Length,
                    IntPtr.Zero,
                    IntPtr.Zero);
                num2 = Math.Min(num2, 0x2000);
                return DrawTextExA(hDC, pOutBytes, num2, ref lpRect, nFormat, lpDTParams);
            }
            return DrawTextExW(hDC, text, text.Length, ref lpRect, nFormat, lpDTParams);
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int DrawTextExA(
            HandleRef hDC,
            byte[] lpszString,
            int byteCount,
            ref IntNativeMethods.RECT lpRect,
            int nFormat,
            [In] [Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int DrawTextExW(
            HandleRef hDC,
            string lpszString,
            int nCount,
            ref IntNativeMethods.RECT lpRect,
            int nFormat,
            [In] [Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int DrawTextW(
            HandleRef hDC,
            string lpszString,
            int nCount,
            ref IntNativeMethods.RECT lpRect,
            int nFormat);

        public static bool Ellipse(HandleRef hDc, int x1, int y1, int x2, int y2)
        {
            return IntEllipse(hDc, x1, y1, x2, y2);
        }

        public static bool EndPath(HandleRef hDC)
        {
            return IntEndPath(hDC);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ExtTextOut(
            HandleRef hdc,
            int x,
            int y,
            int options,
            ref IntNativeMethods.RECT rect,
            string str,
            int length,
            int[] spacing);

        public static bool FillRect(HandleRef hDC, [In] ref IntNativeMethods.RECT rect, HandleRef hbrush)
        {
            return IntFillRect(hDC, ref rect, hbrush);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetBkColor(HandleRef hDC);

        public static int GetBkMode(HandleRef hDC)
        {
            return IntGetBkMode(hDC);
        }

        public static int GetClipRgn(HandleRef hDC, HandleRef hRgn)
        {
            return IntGetClipRgn(hDC, hRgn);
        }

        public static IntPtr GetCurrentObject(HandleRef hDC, int uObjectType)
        {
            return IntGetCurrentObject(hDC, uObjectType);
        }

        public static IntPtr GetDC(HandleRef hWnd)
        {
            return HandleCollector.Add(IntGetDC(hWnd), IntSafeNativeMethods.CommonHandles.HDC);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetGraphicsMode(HandleRef hDC);

        public static int GetMapMode(HandleRef hDC)
        {
            return IntGetMapMode(hDC);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetNearestColor(HandleRef hDC, int color);

        public static int GetObject(HandleRef hBrush, IntNativeMethods.LOGBRUSH lb)
        {
            return IntGetObject(hBrush, Marshal.SizeOf(typeof(IntNativeMethods.LOGBRUSH)), lb);
        }

        public static int GetObject(HandleRef hFont, IntNativeMethods.LOGFONT lp)
        {
            return IntGetObject(hFont, Marshal.SizeOf(typeof(IntNativeMethods.LOGFONT)), lp);
        }

        public static IntNativeMethods.RegionFlags GetRgnBox(
            HandleRef hRgn,
            [In] [Out] ref IntNativeMethods.RECT clipRect)
        {
            return IntGetRgnBox(hRgn, ref clipRect);
        }

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern int GetROP2(HandleRef hdc);

        public static IntPtr GetStockObject(int nIndex)
        {
            return IntGetStockObject(nIndex);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextAlign(HandleRef hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextColor(HandleRef hDC);

        public static int GetTextExtentPoint32(
            HandleRef hDC,
            string text,
            [In] [Out] IntNativeMethods.SIZE size)
        {
            int length = text.Length;
            if (Marshal.SystemDefaultCharSize == 1) {
                byte[] pOutBytes =
                    new byte[WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero)];
                WideCharToMultiByte(
                    0,
                    0,
                    text,
                    text.Length,
                    pOutBytes,
                    pOutBytes.Length,
                    IntPtr.Zero,
                    IntPtr.Zero);
                length = Math.Min(text.Length, 0x2000);
                return GetTextExtentPoint32A(hDC, pOutBytes, length, size);
            }
            return GetTextExtentPoint32W(hDC, text, text.Length, size);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextExtentPoint32A(
            HandleRef hDC,
            byte[] lpszString,
            int byteCount,
            [In] [Out] IntNativeMethods.SIZE size);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextExtentPoint32W(
            HandleRef hDC,
            string text,
            int len,
            [In] [Out] IntNativeMethods.SIZE size);

        public static int GetTextMetrics(HandleRef hDC, ref IntNativeMethods.TEXTMETRIC lptm)
        {
            if (Marshal.SystemDefaultCharSize == 1) {
                IntNativeMethods.TEXTMETRICA textmetrica = new IntNativeMethods.TEXTMETRICA();
                int textMetricsA = GetTextMetricsA(hDC, ref textmetrica);
                lptm.tmHeight = textmetrica.tmHeight;
                lptm.tmAscent = textmetrica.tmAscent;
                lptm.tmDescent = textmetrica.tmDescent;
                lptm.tmInternalLeading = textmetrica.tmInternalLeading;
                lptm.tmExternalLeading = textmetrica.tmExternalLeading;
                lptm.tmAveCharWidth = textmetrica.tmAveCharWidth;
                lptm.tmMaxCharWidth = textmetrica.tmMaxCharWidth;
                lptm.tmWeight = textmetrica.tmWeight;
                lptm.tmOverhang = textmetrica.tmOverhang;
                lptm.tmDigitizedAspectX = textmetrica.tmDigitizedAspectX;
                lptm.tmDigitizedAspectY = textmetrica.tmDigitizedAspectY;
                lptm.tmFirstChar = (char)textmetrica.tmFirstChar;
                lptm.tmLastChar = (char)textmetrica.tmLastChar;
                lptm.tmDefaultChar = (char)textmetrica.tmDefaultChar;
                lptm.tmBreakChar = (char)textmetrica.tmBreakChar;
                lptm.tmItalic = textmetrica.tmItalic;
                lptm.tmUnderlined = textmetrica.tmUnderlined;
                lptm.tmStruckOut = textmetrica.tmStruckOut;
                lptm.tmPitchAndFamily = textmetrica.tmPitchAndFamily;
                lptm.tmCharSet = textmetrica.tmCharSet;
                return textMetricsA;
            }
            return GetTextMetricsW(hDC, ref lptm);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextMetricsA(
            HandleRef hDC,
            [In] [Out] ref IntNativeMethods.TEXTMETRICA lptm);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int GetTextMetricsW(
            HandleRef hDC,
            [In] [Out] ref IntNativeMethods.TEXTMETRIC lptm);

        public static bool GetViewportExtEx(HandleRef hdc, [In] [Out] IntNativeMethods.SIZE lpSize)
        {
            return IntGetViewportExtEx(hdc, lpSize);
        }

        public static bool GetViewportOrgEx(HandleRef hdc, [In] [Out] IntNativeMethods.POINT lpPoint)
        {
            return IntGetViewportOrgEx(hdc, lpPoint);
        }

        [DllImport("gdi32.dll", EntryPoint = "AngleArc", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntAngleArc(
            HandleRef hDC,
            int x,
            int y,
            int radius,
            float startAngle,
            float endAngle);

        [DllImport("gdi32.dll", EntryPoint = "Arc", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntArc(
            HandleRef hDC,
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nXStartArc,
            int nYStartArc,
            int nXEndArc,
            int nYEndArc);

        [DllImport("gdi32.dll", EntryPoint = "BeginPath", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntBeginPath(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "CombineRgn", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntNativeMethods.RegionFlags IntCombineRgn(
            HandleRef hRgnDest,
            HandleRef hRgnSrc1,
            HandleRef hRgnSrc2,
            RegionCombineMode combineMode);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr IntCreateDC(
            string lpszDriverName,
            string lpszDeviceName,
            string lpszOutput,
            HandleRef lpInitData);

        [DllImport("gdi32.dll", EntryPoint = "CreateFontIndirect", CharSet = CharSet.Auto, SetLastError = true
            )]
        public static extern IntPtr IntCreateFontIndirect(
            [In] [Out] [MarshalAs(UnmanagedType.AsAny)] object lf);

        [DllImport("gdi32.dll", EntryPoint = "CreateIC", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr IntCreateIC(
            string lpszDriverName,
            string lpszDeviceName,
            string lpszOutput,
            HandleRef lpInitData);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntDeleteDC(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntDeleteObject(HandleRef hObject);

        [DllImport("gdi32.dll", EntryPoint = "Ellipse", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntEllipse(HandleRef hDc, int x1, int y1, int x2, int y2);

        [DllImport("gdi32.dll", EntryPoint = "EndPath", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntEndPath(HandleRef hDC);

        [DllImport("user32.dll", EntryPoint = "FillRect", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntFillRect(
            HandleRef hdc,
            [In] ref IntNativeMethods.RECT rect,
            HandleRef hbrush);

        [DllImport("gdi32.dll", EntryPoint = "GetBkMode", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntGetBkMode(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "GetClipRgn", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntGetClipRgn(HandleRef hDC, HandleRef hRgn);

        [DllImport("gdi32.dll", EntryPoint = "GetCurrentObject", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntGetCurrentObject(HandleRef hDC, int uObjectType);

        [DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntGetDC(HandleRef hWnd);

        [DllImport("gdi32.dll", EntryPoint = "GetMapMode", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntGetMapMode(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "GetObject", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int IntGetObject(
            HandleRef hBrush,
            int nSize,
            [In] [Out] IntNativeMethods.LOGBRUSH lb);

        [DllImport("gdi32.dll", EntryPoint = "GetObject", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int IntGetObject(
            HandleRef hFont,
            int nSize,
            [In] [Out] IntNativeMethods.LOGFONT lf);

        [DllImport("gdi32.dll", EntryPoint = "GetRgnBox", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntNativeMethods.RegionFlags IntGetRgnBox(
            HandleRef hRgn,
            [In] [Out] ref IntNativeMethods.RECT clipRect);

        [DllImport("gdi32.dll", EntryPoint = "GetStockObject", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntGetStockObject(int nIndex);

        [DllImport("gdi32.dll", EntryPoint = "GetViewportExtEx", SetLastError = true, ExactSpelling = true)]
        public static extern bool IntGetViewportExtEx(HandleRef hdc, [In] [Out] IntNativeMethods.SIZE lpSize);

        [DllImport("gdi32.dll", EntryPoint = "GetViewportOrgEx", SetLastError = true, ExactSpelling = true)]
        public static extern bool IntGetViewportOrgEx(
            HandleRef hdc,
            [In] [Out] IntNativeMethods.POINT lpPoint);

        [DllImport("gdi32.dll", EntryPoint = "LineTo", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntLineTo(HandleRef hdc, int x, int y);

        [DllImport("gdi32.dll", EntryPoint = "MoveToEx", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntMoveToEx(HandleRef hdc, int x, int y, IntNativeMethods.POINT pt);

        [DllImport("gdi32.dll", EntryPoint = "OffsetViewportOrgEx", CharSet = CharSet.Auto,
            SetLastError = true, ExactSpelling = true)]
        public static extern bool IntOffsetViewportOrgEx(
            HandleRef hDC,
            int nXOffset,
            int nYOffset,
            [In] [Out] IntNativeMethods.POINT point);

        [DllImport("gdi32.dll", EntryPoint = "Rectangle", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntRectangle(HandleRef hdc, int left, int top, int right, int bottom);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "RestoreDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntRestoreDC(HandleRef hDC, int nSavedDC);

        [DllImport("gdi32.dll", EntryPoint = "SaveDC", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntSaveDC(HandleRef hDC);

        [DllImport("gdi32.dll", EntryPoint = "SelectClipRgn", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntNativeMethods.RegionFlags IntSelectClipRgn(HandleRef hDC, HandleRef hRgn);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern IntPtr IntSelectObject(HandleRef hdc, HandleRef obj);

        [DllImport("gdi32.dll", EntryPoint = "SetBkMode", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntSetBkMode(HandleRef hDC, int nBkMode);

        [DllImport("gdi32.dll", EntryPoint = "SetGraphicsMode", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntSetGraphicsMode(HandleRef hDC, int iMode);

        [DllImport("gdi32.dll", EntryPoint = "SetMapMode", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern int IntSetMapMode(HandleRef hDC, int nMapMode);

        [DllImport("gdi32.dll", EntryPoint = "SetViewportExtEx", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntSetViewportExtEx(
            HandleRef hDC,
            int x,
            int y,
            [In] [Out] IntNativeMethods.SIZE size);

        [DllImport("gdi32.dll", EntryPoint = "SetViewportOrgEx", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntSetViewportOrgEx(
            HandleRef hDC,
            int x,
            int y,
            [In] [Out] IntNativeMethods.POINT point);

        [DllImport("gdi32.dll", EntryPoint = "StrokePath", CharSet = CharSet.Auto, SetLastError = true,
            ExactSpelling = true)]
        public static extern bool IntStrokePath(HandleRef hDC);

        public static bool LineTo(HandleRef hdc, int x, int y)
        {
            return IntLineTo(hdc, x, y);
        }

        public static bool MoveToEx(HandleRef hdc, int x, int y, IntNativeMethods.POINT pt)
        {
            return IntMoveToEx(hdc, x, y, pt);
        }

        public static bool OffsetViewportOrgEx(
            HandleRef hDC,
            int nXOffset,
            int nYOffset,
            [In] [Out] IntNativeMethods.POINT point)
        {
            return IntOffsetViewportOrgEx(hDC, nXOffset, nYOffset, point);
        }

        public static bool Rectangle(HandleRef hdc, int left, int top, int right, int bottom)
        {
            return IntRectangle(hdc, left, top, right, bottom);
        }

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            HandleCollector.Remove((IntPtr)hDC, IntSafeNativeMethods.CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }

        public static bool RestoreDC(HandleRef hDC, int nSavedDC)
        {
            return IntRestoreDC(hDC, nSavedDC);
        }

        public static int SaveDC(HandleRef hDC)
        {
            return IntSaveDC(hDC);
        }

        public static IntNativeMethods.RegionFlags SelectClipRgn(HandleRef hDC, HandleRef hRgn)
        {
            return IntSelectClipRgn(hDC, hRgn);
        }

        public static IntPtr SelectObject(HandleRef hdc, HandleRef obj)
        {
            return IntSelectObject(hdc, obj);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);

        public static int SetBkMode(HandleRef hDC, int nBkMode)
        {
            return IntSetBkMode(hDC, nBkMode);
        }

        public static int SetGraphicsMode(HandleRef hDC, int iMode)
        {
            iMode = IntSetGraphicsMode(hDC, iMode);
            return iMode;
        }

        public static int SetMapMode(HandleRef hDC, int nMapMode)
        {
            return IntSetMapMode(hDC, nMapMode);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetROP2(HandleRef hDC, int nDrawMode);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetTextAlign(HandleRef hDC, int nMode);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetTextColor(HandleRef hDC, int crColor);

        public static bool SetViewportExtEx(
            HandleRef hDC,
            int x,
            int y,
            [In] [Out] IntNativeMethods.SIZE size)
        {
            return IntSetViewportExtEx(hDC, x, y, size);
        }

        public static bool SetViewportOrgEx(
            HandleRef hDC,
            int x,
            int y,
            [In] [Out] IntNativeMethods.POINT point)
        {
            return IntSetViewportOrgEx(hDC, x, y, point);
        }

        public static bool StrokePath(HandleRef hDC)
        {
            return IntStrokePath(hDC);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int WideCharToMultiByte(
            int codePage,
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] string wideStr,
            int chars,
            [In] [Out] byte[] pOutBytes,
            int bufferBytes,
            IntPtr defaultChar,
            IntPtr pDefaultUsed);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr WindowFromDC(HandleRef hDC);
    }
}
