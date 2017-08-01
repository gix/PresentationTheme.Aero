namespace ThemePreviewer.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using ThemeCore;
    using ThemeCore.Native;
    using Size = System.Drawing.Size;
    using SystemColors = System.Drawing.SystemColors;

    public partial class FakeMenu : Control
    {
        private MenuNode rootNode;

        public FakeMenu()
        {
            InitializeComponent();
            AutoSize = true;
        }

        [DefaultValue(true)]
        public new bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        public MenuNode RootNode
        {
            get => rootNode;
            set
            {
                rootNode = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            base.OnPaint(args);

            if (RootNode == null)
                return;

            CreateRenderer(args.Graphics, args.ClipRectangle).Render();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (RootNode == null)
                return base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics()) {
                var size = CreateRenderer(g, new Rectangle(0, 0, Width, Height)).Measure();
                return new Size(Width, size.Height);
            }
        }

        private MenuRenderer CreateRenderer(Graphics g, Rectangle clipBounds)
        {
            return new MenuRenderer(g, Font, clipBounds, RootNode);
        }
    }

    public class SelectFontScope : IDisposable
    {
        private readonly Graphics graphics;
        private readonly IntPtr hfont;
        private readonly IntPtr hfontOld;

        public SelectFontScope(Graphics graphics, Font font)
        {
            this.graphics = graphics;
            hfont = font.ToHfont();

            var hdc = graphics.GetHdc();
            hfontOld = NativeMethods.SelectObject(hdc, hfont);
            graphics.ReleaseHdc(hdc);
        }

        public void Dispose()
        {
            var hdc = graphics.GetHdc();
            NativeMethods.SelectObject(hdc, hfontOld);
            graphics.ReleaseHdc(hdc);

            NativeMethods.DeleteObject(hfont);
        }
    }

    public class MenuRenderer
    {
        private readonly ThemeMenuMetrics menuMetrics = new ThemeMenuMetrics();
        private readonly Graphics g;
        private readonly Font font;
        private readonly Rectangle bounds;
        private readonly MenuNode root;
        private readonly int barHeight;

        public MenuRenderer(Graphics g, Font font, Rectangle bounds, MenuNode root)
        {
            this.g = g;
            this.font = font;
            this.bounds = bounds;
            this.root = root;

            barHeight = (int)SystemParameters.MenuButtonHeight;
        }

        public Size Measure()
        {
            int height = 0;
            height += (int)SystemParameters.MenuButtonHeight;

            var menuPopup = new ThemeMenuPopup(menuMetrics);
            if (root.Children.Count > 0 && root.Children[0].Children.Count > 0) {
                height += 2 * 3;

                using (var hdc = g.Hdc()) {
                    foreach (var item in root.Children[0].Children) {
                        var ummi = new UAHMEASUREMENUITEM();
                        ummi.mis.item = item;
                        menuPopup.MeasureItem(ummi, hdc, out Size size);
                        height += size.Height;
                    }
                }
            }

            return new Size(200, height);
        }

        public void Render()
        {
            using (new SelectFontScope(g, font)) {
                RenderBar();
                RenderPopup(root.Children[0]);
            }
        }

        private void RenderBar()
        {
            var barBounds = new Rectangle(
                bounds.X, bounds.Y, bounds.Width, barHeight);

            using (var pen = new Pen(SystemColors.MenuBar))
                g.DrawLine(pen, barBounds.Left, barBounds.Bottom + 1, barBounds.Right, barBounds.Bottom + 1);

            var menuBar = new ThemeMenuBar(menuMetrics);
            var location = bounds.Location;

            using (var hdc = g.Hdc()) {
                menuBar.DrawClientArea(hdc, barBounds);

                for (int i = 0; i < root.Children.Count; ++i) {
                    var item = root.Children[i];
                    var text = item.Text?.Replace("&", "");

                    var ummi = new UAHMEASUREMENUITEM();
                    ummi.mis.item = item;
                    menuBar.MeasureItem(ummi, hdc, out Size itemSize);

                    var udmi = new UAHDRAWMENUITEM();
                    udmi.umi = ummi.umi;
                    udmi.umi.umpm.rgcx[0] = 22;
                    udmi.umi.umpm.rgcx[2] = 118;
                    udmi.umi.umpm.rgcx[3] = 36;
                    udmi.dis.rcItem = new Rectangle(
                        location.X, location.Y, itemSize.Width, itemSize.Height);
                    udmi.dis.text = text;
                    udmi.dis.hot = i == 0;
                    menuBar.DrawItem(item, hdc, udmi);

                    location.Offset(itemSize.Width, 0);
                }
            }
        }

        private void RenderPopup(MenuNode menuNode)
        {
            var windowRect = new Rectangle(
                bounds.X, bounds.Y + barHeight, bounds.Width, bounds.Bottom - barHeight);
            var clientRect = windowRect;
            clientRect.Inflate(-3, -3);

            var menuPopup = new ThemeMenuPopup(menuMetrics);
            using (var hdc = g.Hdc()) {
                menuPopup.DrawNonClientArea(hdc, windowRect);
                menuPopup.DrawClientArea(hdc, clientRect);
            }

            var location = clientRect.Location;

            using (var hdc = g.Hdc()) {
                for (int i = 0; i < menuNode.Children.Count; ++i) {
                    var item = menuNode.Children[i];
                    var text = item.Text?.Replace("&", "");

                    var ummi = new UAHMEASUREMENUITEM();
                    ummi.mis.item = item;
                    menuPopup.MeasureItem(ummi, hdc, out Size itemSize);

                    var udmi = new UAHDRAWMENUITEM();
                    udmi.umi = ummi.umi;
                    udmi.umi.umpm.rgcx[0] = 22;
                    udmi.umi.umpm.rgcx[2] = 118;
                    udmi.umi.umpm.rgcx[3] = 36;
                    udmi.dis.rcItem = new Rectangle(
                        location.X, location.Y, clientRect.Width, itemSize.Height);
                    udmi.dis.text = text;
                    udmi.dis.hot = i == 0;
                    menuPopup.DrawItem(item, hdc, udmi);

                    location.Offset(0, itemSize.Height);
                }
            }
        }

        private VisualStyleRenderer R<TPart, TState>(string className, TPart part, TState state)
        {
            return new VisualStyleRenderer(className, (int)(object)part, (int)(object)state);
        }

        private class ThemeMenuMetrics
        {
            public SafeThemeHandle hTheme;
            public int iBarBackground;
            public int iBarItem;
            public int iBarBorderSize;
            public MARGINS marBarItem;
            public int iPopupBackground;
            public int iPopupBorders;
            public int iPopupCheck;
            public int iPopupCheckBackground;
            public int iPopupSubmenu;
            public int iPopupGutter;
            public int iPopupItem;
            public int iPopupSeparator;
            public MARGINS marPopupCheckBackground;
            public MARGINS marPopupItem;
            public MARGINS[] marPopupItems = new MARGINS[4];
            public MARGINS marPopupSubmenu;
            public MARGINS marPopupOwnerDrawnItem;
            public Size sizePopupCheck;
            public Size sizePopupSubmenu;
            public Size sizePopupSeparator;
            public int iPopupBorderSize;
            public int iPopupBgBorderSize;

            public ThemeMenuMetrics()
            {
                hTheme = StyleNativeMethods.OpenThemeData(IntPtr.Zero, "MENU");
                if (hTheme.IsInvalid)
                    return;

                iBarBackground = (int)MENUPARTS.MENU_BARBACKGROUND;
                iBarItem = (int)MENUPARTS.MENU_BARITEM;
                iPopupBackground = (int)MENUPARTS.MENU_POPUPBACKGROUND;
                iPopupBorders = (int)MENUPARTS.MENU_POPUPBORDERS;
                iPopupCheck = (int)MENUPARTS.MENU_POPUPCHECK;
                iPopupCheckBackground = (int)MENUPARTS.MENU_POPUPCHECKBACKGROUND;
                iPopupSubmenu = (int)MENUPARTS.MENU_POPUPSUBMENU;
                iPopupGutter = (int)MENUPARTS.MENU_POPUPGUTTER;
                iPopupItem = (int)MENUPARTS.MENU_POPUPITEM;
                iPopupSeparator = (int)MENUPARTS.MENU_POPUPSEPARATOR;

                StyleNativeMethods.GetThemePartSize(hTheme, IntPtr.Zero, iPopupCheck, 0, null, ThemeSize.True, out sizePopupCheck);
                StyleNativeMethods.GetThemePartSize(hTheme, IntPtr.Zero, iPopupSeparator, 0, null, ThemeSize.True, out sizePopupSeparator);
                StyleNativeMethods.GetThemePartSize(hTheme, IntPtr.Zero, iPopupSubmenu, 0, null, ThemeSize.True, out sizePopupSubmenu);
                StyleNativeMethods.GetThemeColor(hTheme, iBarItem, 0, (int)TMT.BORDERSIZE, out iBarBorderSize);
                StyleNativeMethods.GetThemeColor(hTheme, iPopupItem, 0, (int)TMT.BORDERSIZE, out iPopupBorderSize);
                StyleNativeMethods.GetThemeColor(hTheme, iPopupBackground, 0, (int)TMT.BORDERSIZE, out iPopupBgBorderSize);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iBarItem, 0, (int)TMT.CONTENTMARGINS, null, out marBarItem);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iPopupCheck, 0, (int)TMT.CONTENTMARGINS, null, out marPopupItems[0]);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iPopupCheck, 0, (int)TMT.CONTENTMARGINS, null, out marPopupItems[1]);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iPopupCheckBackground, 0, (int)TMT.CONTENTMARGINS, null, out marPopupCheckBackground);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iPopupItem, 0, (int)TMT.CONTENTMARGINS, null, out marPopupItem);
                StyleNativeMethods.GetThemeMargins(hTheme, IntPtr.Zero, iPopupSubmenu, 0, (int)TMT.CONTENTMARGINS, null, out marPopupSubmenu);

                marPopupItems[2] = marPopupItem;
                marPopupItems[2].cxLeftWidth = iPopupBgBorderSize;
                marPopupItems[2].cxRightWidth = iPopupBgBorderSize;

                marPopupItems[3] = marPopupItem;
                marPopupItems[3].cxLeftWidth = 0;

                sizePopupSubmenu.Width += marPopupSubmenu.cxLeftWidth + marPopupSubmenu.cxRightWidth;
                sizePopupSubmenu.Height += marPopupSubmenu.cyTopHeight + marPopupSubmenu.cyBottomHeight;
                marPopupOwnerDrawnItem.cxRightWidth = iPopupBorderSize;
            }
        }

        private class UAHMENUITEMMETRICS
        {
            public readonly Size[] rgsizePopup = new Size[4];
        }

        private class UAHMENUPOPUPMETRICS
        {
            public readonly int[] rgcx = new int[4];
        }

        private class MEASUREITEMSTRUCT
        {
            public MenuNode item;
        }

        private class DRAWITEMSTRUCT
        {
            public Rectangle rcItem;
            public string text;
            public bool hot;
        }

        private class UAHMEASUREMENUITEM
        {
            public readonly MEASUREITEMSTRUCT mis = new MEASUREITEMSTRUCT();
            public readonly UAHMENUITEM umi = new UAHMENUITEM();
        }

        private class UAHDRAWMENUITEM
        {
            public readonly DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
            public UAHMENUITEM umi = new UAHMENUITEM();
        }

        private class UAHMENUITEM
        {
            public UAHMENUITEMMETRICS umim = new UAHMENUITEMMETRICS();
            public readonly UAHMENUPOPUPMETRICS umpm = new UAHMENUPOPUPMETRICS();
        }

        private class ThemeMenuBar
        {
            private readonly ThemeMenuMetrics metrics;

            public ThemeMenuBar(ThemeMenuMetrics metrics)
            {
                this.metrics = metrics;
            }

            public void MeasureItem(UAHMEASUREMENUITEM pummi, IntPtr hdc, out Size itemSize)
            {
                var hdcRef = new HandleRef(this, hdc);
                var item = pummi.mis.item;
                var mim = pummi.umi.umim;

                if (!string.IsNullOrEmpty(item.Text)) {
                    StyleNativeMethods.GetThemeTextExtent(
                        metrics.hTheme, hdcRef, metrics.iBarItem, 0,
                        item.Text, item.Text.Length,
                        (int)TextFormatFlags.SingleLine, null, out Rectangle rc);
                    pummi.umi.umim.rgsizePopup[1].Width = rc.Width;
                    pummi.umi.umim.rgsizePopup[1].Height = rc.Height;
                }

                int width = 0;
                int height = 0;

                bool hasBar = false;
                for (int i = 0; i < 2; ++i) {
                    Size barSize = mim.rgsizePopup[i];
                    if (barSize.Width == 0 || barSize.Height == 0)
                        continue;

                    if (!hasBar)
                        hasBar = true;
                    else
                        width += metrics.iBarBorderSize;

                    width += barSize.Width;
                    if (height <= barSize.Height)
                        height = barSize.Height;
                }

                width += metrics.marBarItem.cxLeftWidth + metrics.marBarItem.cxRightWidth;

                int menuHeight = (int)SystemParameters.MenuHeight;
                if (height <= menuHeight)
                    height = menuHeight;

                itemSize = new Size(width, height);
            }

            private void LayoutItem(
                ref Rectangle prcItem, Size[] barSizes, out DRAWITEMMETRICS pdim)
            {
                pdim = new DRAWITEMMETRICS();

                bool hasBar = false;
                int itemHeight = prcItem.Height;
                int dx = prcItem.Left + metrics.marBarItem.cxLeftWidth;
                for (int i = 0; i < 2; ++i) {
                    Size barSize = barSizes[i];
                    if (barSize.Width == 0 || barSize.Height == 0)
                        continue;

                    var rc = pdim.rgrc[i];
                    rc.Width = barSize.Width;
                    rc.Height = barSize.Height;

                    if (!hasBar)
                        hasBar = true;
                    else
                        dx += metrics.iBarBorderSize;

                    var dy = (itemHeight - barSize.Height) / 2 + prcItem.Top;
                    rc.Offset(dx, dy);
                    pdim.rgrc[i] = rc;
                    dx += barSize.Width;
                }

                if (!hasBar) {
                    pdim.rgrc[1].Width = prcItem.Width;
                    pdim.rgrc[1].Height = itemHeight;
                }
            }

            private class DRAWITEMMETRICS
            {
                public readonly Rectangle[] rgrc = new Rectangle[2];
            }

            public void DrawItem(MenuNode item, IntPtr hdc, UAHDRAWMENUITEM udmi)
            {
                LayoutItem(
                    ref udmi.dis.rcItem, udmi.umi.umim.rgsizePopup, out DRAWITEMMETRICS dim);

                var hdcRef = new HandleRef(this, hdc);
                var hTheme = metrics.hTheme;

                bool isHot = udmi.dis.hot;

                int stateId;
                if (item.IsEnabled && isHot)
                    stateId = (int)BARITEMSTATES.MBI_HOT;
                else if (item.IsEnabled)
                    stateId = (int)BARITEMSTATES.MBI_NORMAL;
                else if (isHot)
                    stateId = (int)BARITEMSTATES.MBI_DISABLEDHOT;
                else
                    stateId = (int)BARITEMSTATES.MBI_DISABLED;

                if (StyleNativeMethods.IsThemeBackgroundPartiallyTransparent(hTheme, metrics.iBarItem, stateId)) {
                    DrawClientArea(hdc, udmi.dis.rcItem);
                }

                StyleNativeMethods.DrawThemeBackground(
                    hTheme, hdcRef, metrics.iBarItem, stateId,
                    new CRECT(udmi.dis.rcItem), null);

                if (udmi.dis.text != null) {
                    TextFormatFlags textFlags = TextFormatFlags.SingleLine;
                    textFlags |= TextFormatFlags.HidePrefix;

                    StyleNativeMethods.DrawThemeText(
                        hTheme, hdcRef, metrics.iBarItem, stateId, udmi.dis.text,
                        udmi.dis.text.Length, (int)textFlags, 0, new CRECT(dim.rgrc[1]));
                }
            }

            public void DrawClientArea(IntPtr hdc, Rectangle clientRect)
            {
                StyleNativeMethods.DrawThemeBackground(
                    metrics.hTheme, new HandleRef(this, hdc),
                    metrics.iBarBackground, 0, new CRECT(clientRect), null);
            }
        }

        private class ThemeMenuPopup
        {
            private readonly ThemeMenuMetrics metrics;

            public ThemeMenuPopup(ThemeMenuMetrics metrics)
            {
                this.metrics = metrics;
            }

            public void MeasureItem(UAHMEASUREMENUITEM pummi, IntPtr hdc, out Size itemSize)
            {
                var hdcRef = new HandleRef(this, hdc);

                var item = pummi.mis.item;
                var isSeparator = item.IsSeparator;

                var mim = new UAHMENUITEMMETRICS();
                pummi.umi.umim = mim;

                int height = 0;
                int width = 0;

                if (isSeparator) {
                    mim.rgsizePopup[2].Width = metrics.marPopupItem.cxLeftWidth + metrics.marPopupItem.cxRightWidth + 1;
                    mim.rgsizePopup[2].Height = metrics.marPopupItem.cyTopHeight + metrics.marPopupItem.cyBottomHeight + metrics.sizePopupSeparator.Height;
                } else {
                    var marPopupItems = metrics.marPopupItems;
                    var marPopupCheckBackground = metrics.marPopupCheckBackground;

                    mim.rgsizePopup[0].Width =
                        metrics.sizePopupCheck.Width +
                        marPopupItems[0].cxLeftWidth +
                        marPopupItems[0].cxRightWidth;
                    mim.rgsizePopup[0].Height =
                        metrics.sizePopupCheck.Height +
                        marPopupItems[0].cyTopHeight +
                        marPopupItems[0].cyBottomHeight +
                        marPopupCheckBackground.cyTopHeight +
                        marPopupCheckBackground.cyBottomHeight;

                    if (!string.IsNullOrEmpty(item.Text)) {
                        StyleNativeMethods.GetThemeTextExtent(
                            metrics.hTheme, hdcRef, metrics.iPopupItem, 0,
                            item.Text, item.Text.Length,
                            (int)TextFormatFlags.SingleLine, null, out Rectangle rc);
                        mim.rgsizePopup[2].Width = rc.Width + marPopupItems[2].cxLeftWidth + marPopupItems[2].cxRightWidth;
                        mim.rgsizePopup[2].Height = rc.Height + marPopupItems[2].cyTopHeight + marPopupItems[2].cyBottomHeight;
                    }

                    if (!string.IsNullOrEmpty(item.InputGestureText)) {
                        StyleNativeMethods.GetThemeTextExtent(
                            metrics.hTheme, hdcRef, metrics.iPopupItem, 0,
                            item.InputGestureText, item.InputGestureText.Length,
                            (int)TextFormatFlags.SingleLine, null, out Rectangle rc);
                        mim.rgsizePopup[3].Width = rc.Width + marPopupItems[3].cxLeftWidth + marPopupItems[3].cxRightWidth;
                        mim.rgsizePopup[3].Height = rc.Height + marPopupItems[3].cyTopHeight + marPopupItems[3].cyBottomHeight;
                    }

                    width =
                        marPopupCheckBackground.cxLeftWidth +
                        marPopupCheckBackground.cxRightWidth +
                        metrics.marPopupItem.cxLeftWidth +
                        metrics.marPopupItem.cxRightWidth +
                        metrics.sizePopupSubmenu.Width;

                    if (metrics.sizePopupSubmenu.Height >= 0)
                        height = metrics.sizePopupSubmenu.Height;
                }

                for (int i = 0; i < 4; ++i) {
                    var v = mim.rgsizePopup[i].Height;
                    if (height <= v)
                        height = v;
                }

                itemSize = new Size(width, height);
            }

            private class DRAWITEMMETRICS
            {
                public Rectangle rcSelection;
                public Rectangle rcGutter;
                public Rectangle rcCheckBackground;
                public readonly Rectangle[] rgrc = new Rectangle[4];
                public Rectangle rcSubmenu;
            }

            private void LayoutItem(
                MenuNode item, ref Rectangle prcItem, UAHMENUITEM pumi,
                out DRAWITEMMETRICS pdim)
            {
                var marPopupCheckBackground = metrics.marPopupCheckBackground;
                var marPopupItem = metrics.marPopupItem;

                pdim = new DRAWITEMMETRICS();

                var height = prcItem.Bottom - prcItem.Top;
                var isSeparator = item.IsSeparator;
                var hasSubMenu = item.Children.Count != 0;

                var left = prcItem.Left + marPopupItem.cxLeftWidth;

                for (int i = 0; i < 4; ++i) {
                    var w = pumi.umpm.rgcx[i];
                    if (w != 0) {
                        int h;
                        switch (i) {
                            case 0:
                                left += marPopupCheckBackground.cxLeftWidth;
                                goto case 1;
                            case 1:
                                int v24;
                                left += w - pumi.umim.rgsizePopup[i].Width;
                                w = pumi.umim.rgsizePopup[i].Width;
                                v24 = pumi.umim.rgsizePopup[i].Height;
                                h = v24 - (marPopupCheckBackground.cyTopHeight + marPopupCheckBackground.cyBottomHeight);
                                break;

                            case 3:
                                left = prcItem.Right - metrics.sizePopupSubmenu.Width - marPopupItem.cxRightWidth - w;
                                goto default;

                            default:
                                h = pumi.umim.rgsizePopup[i].Height;
                                break;
                        }

                        var rc = Rectangle.FromLTRB(left, prcItem.Top, left + w, prcItem.Top + h);
                        rc = Rectangle.FromLTRB(
                            rc.Left + metrics.marPopupItems[i].cxLeftWidth,
                            rc.Top + metrics.marPopupItems[i].cyTopHeight,
                            rc.Right - metrics.marPopupItems[i].cxRightWidth,
                            rc.Bottom - metrics.marPopupItems[i].cyBottomHeight);
                        rc.Offset(0, (height - h) / 2);
                        pdim.rgrc[i] = rc;

                        if (i == 0 && pumi.umpm.rgcx[1] == 0 || i == 1)
                            left += marPopupCheckBackground.cxRightWidth;

                        left += w;
                    }
                }

                if (!isSeparator && hasSubMenu) {
                    if (pumi.umpm.rgcx[3] == 0)
                        left = prcItem.Right - metrics.sizePopupSubmenu.Width - marPopupItem.cxRightWidth;
                    var rc = Rectangle.FromLTRB(
                        left,
                        prcItem.Top,
                        left + metrics.sizePopupSubmenu.Width,
                        prcItem.Top + metrics.sizePopupSubmenu.Height);
                    rc = Rectangle.FromLTRB(
                        rc.Left + metrics.marPopupSubmenu.cxLeftWidth,
                        rc.Top + metrics.marPopupSubmenu.cyTopHeight,
                        rc.Right - metrics.marPopupSubmenu.cxRightWidth,
                        rc.Bottom - metrics.marPopupSubmenu.cyBottomHeight);
                    rc.Offset(0, (height - metrics.sizePopupSubmenu.Height) / 2);
                    pdim.rcSubmenu = rc;
                }

                if (pumi.umpm.rgcx[0] != 0) {
                    var v29 = pumi.umim.rgsizePopup[1].Height;
                    if (pumi.umim.rgsizePopup[0].Height > v29)
                        v29 = pumi.umim.rgsizePopup[0].Height;

                    var marPopupCheckBackgroundHMargin = marPopupCheckBackground.cxLeftWidth + marPopupCheckBackground.cxRightWidth;
                    var v30 = v29 - (marPopupCheckBackground.cyTopHeight + marPopupCheckBackground.cyBottomHeight) + marPopupCheckBackground.cyTopHeight + marPopupCheckBackground.cyBottomHeight;
                    var v31 = prcItem.Left + pumi.umpm.rgcx[0] + marPopupItem.cxLeftWidth - pumi.umim.rgsizePopup[0].Width;
                    var rc = Rectangle.FromLTRB(
                        v31,
                        prcItem.Top,
                        v31 + pumi.umim.rgsizePopup[0].Width + marPopupCheckBackgroundHMargin,
                        prcItem.Top + v30);

                    pdim.rcCheckBackground = Rectangle.FromLTRB(
                        rc.Left + marPopupCheckBackground.cxLeftWidth,
                        rc.Top + marPopupCheckBackground.cyTopHeight,
                        rc.Right - marPopupCheckBackground.cxRightWidth,
                        rc.Bottom - marPopupCheckBackground.cyBottomHeight);
                    pdim.rcCheckBackground = pdim.rcCheckBackground.OffsetRect(0, (height - v30) / 2);

                    int gutterWidth = pumi.umpm.rgcx[0] + pumi.umpm.rgcx[1] + marPopupItem.cxLeftWidth + marPopupCheckBackgroundHMargin;

                    pdim.rcGutter = Rectangle.FromLTRB(
                        prcItem.Left,
                        prcItem.Top,
                        prcItem.Left + gutterWidth,
                        prcItem.Top + height);
                }

                if (isSeparator) {
                    var rc = Rectangle.FromLTRB(
                        (pumi.umpm.rgcx[0] != 0 ? pdim.rcGutter.Right : prcItem.Left) + marPopupItem.cxLeftWidth,
                        prcItem.Top,
                        prcItem.Right - marPopupItem.cxRightWidth,
                        prcItem.Top + pumi.umim.rgsizePopup[2].Height);
                    rc = Rectangle.FromLTRB(
                        rc.Left + marPopupItem.cxLeftWidth,
                        rc.Top + marPopupItem.cyTopHeight,
                        rc.Right - marPopupItem.cxRightWidth,
                        rc.Bottom - marPopupItem.cyBottomHeight);
                    rc.Offset(0, (height - pumi.umim.rgsizePopup[2].Height) / 2);
                    pdim.rgrc[2] = rc;
                } else {
                    pdim.rcSelection = Rectangle.FromLTRB(
                        prcItem.Left + marPopupItem.cxLeftWidth,
                        prcItem.Top,
                        prcItem.Right - marPopupItem.cxRightWidth,
                        prcItem.Top + height);
                }
            }

            public void DrawItem(MenuNode item, IntPtr hdc, UAHDRAWMENUITEM udmi)
            {
                LayoutItem(item, ref udmi.dis.rcItem, udmi.umi, out DRAWITEMMETRICS dim);

                bool isHot = udmi.dis.hot;

                int stateId;
                if (item.IsEnabled && isHot)
                    stateId = (int)POPUPITEMSTATES.MPI_HOT;
                else if (item.IsEnabled)
                    stateId = (int)POPUPITEMSTATES.MPI_NORMAL;
                else if (isHot)
                    stateId = (int)POPUPITEMSTATES.MPI_DISABLEDHOT;
                else
                    stateId = (int)POPUPITEMSTATES.MPI_DISABLED;

                var hdcRef = new HandleRef(this, hdc);

                var hTheme = metrics.hTheme;
                if (StyleNativeMethods.IsThemeBackgroundPartiallyTransparent(hTheme, metrics.iPopupItem, stateId)) {
                    StyleNativeMethods.DrawThemeBackground(
                        hTheme,
                        hdcRef,
                        metrics.iPopupBackground,
                        0,
                        new CRECT(udmi.dis.rcItem),
                        null);
                }

                if (!dim.rcGutter.IsEmpty)
                    StyleNativeMethods.DrawThemeBackground(
                        hTheme,
                        hdcRef,
                        metrics.iPopupGutter,
                        0,
                        new CRECT(dim.rcGutter),
                        null);

                if (item.IsSeparator) {
                    StyleNativeMethods.DrawThemeBackground(
                        hTheme, hdcRef, metrics.iPopupSeparator, 0,
                        new CRECT(dim.rgrc[2]), null);
                    return;
                }

                StyleNativeMethods.DrawThemeBackground(
                    hTheme, hdcRef, metrics.iPopupItem, stateId,
                    new CRECT(dim.rcSelection), null);

                if (!dim.rgrc[0].IsEmpty && item.IsChecked) {
                    POPUPCHECKBACKGROUNDSTATES bgState;
                    if (item.IsEnabled)
                        bgState = POPUPCHECKBACKGROUNDSTATES.MCB_NORMAL;
                    else
                        bgState = POPUPCHECKBACKGROUNDSTATES.MCB_DISABLED;

                    POPUPCHECKSTATES checkState;
                    if (item.IsEnabled)
                        checkState = POPUPCHECKSTATES.MC_CHECKMARKNORMAL;
                    else
                        checkState = POPUPCHECKSTATES.MC_CHECKMARKDISABLED;

                    StyleNativeMethods.DrawThemeBackground(
                        hTheme, hdcRef, metrics.iPopupCheckBackground,
                        (int)bgState, new CRECT(dim.rcCheckBackground), null);

                    StyleNativeMethods.DrawThemeBackground(
                        hTheme, hdcRef, metrics.iPopupCheck,
                        (int)checkState, new CRECT(dim.rgrc[0]), null);
                }

                if (udmi.dis.text != null)
                    StyleNativeMethods.DrawThemeText(
                        hTheme, hdcRef, metrics.iPopupItem, stateId,
                        udmi.dis.text,
                        udmi.dis.text.Length,
                        (int)(TextFormatFlags.SingleLine | TextFormatFlags.Left | TextFormatFlags.Internal),
                        0,
                        new CRECT(dim.rgrc[2]));

                if (item.InputGestureText != null)
                    StyleNativeMethods.DrawThemeText(
                        hTheme, hdcRef,
                        metrics.iPopupItem,
                        stateId,
                        item.InputGestureText,
                        item.InputGestureText.Length,
                        (int)(TextFormatFlags.SingleLine | TextFormatFlags.Right),
                        0,
                        new CRECT(dim.rgrc[3]));

                if (item.Children.Count != 0)
                    StyleNativeMethods.DrawThemeBackground(
                        hTheme, hdcRef, metrics.iPopupSubmenu,
                        (int)(item.IsEnabled ? POPUPSUBMENUSTATES.MSM_NORMAL : POPUPSUBMENUSTATES.MSM_DISABLED),
                        new CRECT(dim.rcSubmenu), null);
            }

            public void DrawNonClientArea(IntPtr hdc, Rectangle windowRect)
            {
                StyleNativeMethods.DrawThemeBackground(
                    metrics.hTheme, new HandleRef(this, hdc),
                    metrics.iPopupBorders, 0, new CRECT(windowRect), null);
            }

            public void DrawClientArea(IntPtr hdc, Rectangle clientRect)
            {
                StyleNativeMethods.DrawThemeBackground(
                    metrics.hTheme, new HandleRef(this, hdc),
                    metrics.iPopupBackground, 0, new CRECT(clientRect), null);
            }
        }
    }

    public static class Ext
    {
        public static Rectangle OffsetRect(this Rectangle rect, int x, int y)
        {
            rect.Offset(x, y);
            return rect;
        }

        public static HdcScope Hdc(this Graphics g)
        {
            return new HdcScope(g);
        }

        public sealed class HdcScope : IDisposable
        {
            private readonly Graphics graphics;
            private readonly IntPtr hdc;

            public HdcScope(Graphics graphics)
            {
                this.graphics = graphics;
                hdc = graphics.GetHdc();
            }

            public void Dispose()
            {
                graphics.ReleaseHdc(hdc);
            }

            public static implicit operator IntPtr(HdcScope scope)
            {
                return scope.hdc;
            }
        }
    }
}
