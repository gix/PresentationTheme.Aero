namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public partial class SysListViewSample : UserControl, IOptionControl
    {
        private OptionList options;

        public SysListViewSample()
        {
            InitializeComponent();

            var items = new List<ListViewItem> {
                new ListViewItem(new[] { "Item1", "439 KB", "Zip Archive" }, -1),
                new ListViewItem(new[] { "stuff.7z", "847 KB", "7z Archive" }, -1),
                new ListViewItem(new[] { "unknown10.flac", "37.453 KB", "FLAC" }, -1),
            };
            for (int i = 0; i < 30; ++i) {
                string v = i.ToString();
                items.Add(new ListViewItem(new[] { v, v, v }));
            }
            sysListView.Items.AddRange(items.ToArray());
            sysListView.FullRowSelect = true;
            sysListView.HideSelection = false;

            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem2("Foo"));
            menu.MenuItems.Add(new MenuItem2("Foo") { OwnerDraw = true });
            menu.MenuItems.Add(new MenuItem2("Foo"));
            ContextMenu = menu;

            //PaintStyles(sysListView);
            options = CreateOptions();
        }

        private class MenuItem2 : MenuItem
        {
            public MenuItem2(string text)
                : base(text)
            {
            }

            protected override void OnMeasureItem(MeasureItemEventArgs e)
            {
                //var r = new VisualStyleRenderer(VisualStyleElement.Menu.Item.Normal);
                e.ItemHeight = 20;

                base.OnMeasureItem(e);
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                try {
                    var r = new VisualStyleRenderer(
                        "LISTVIEW",
                        VisualStyleElement.ListView.Item.Selected.Part,
                        VisualStyleElement.ListView.Item.Selected.State);
                    //r.DrawBackground(e.Graphics, e.Bounds);
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }

                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(0xFF, 0xEE, 0xDD)),
                    e.Bounds);

                base.OnDrawItem(e);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DRAWITEMSTRUCT
        {
            public int CtlType;
            public int CtlID;
            public int itemID;
            public int itemAction;
            public int itemState;
            public IntPtr hwndItem = IntPtr.Zero;
            public IntPtr hDC = IntPtr.Zero;
            public RECT rcItem;
            public IntPtr itemData = IntPtr.Zero;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 43) {
                var param = (DRAWITEMSTRUCT)m.GetLParam(typeof(DRAWITEMSTRUCT));
                using (Graphics g = Graphics.FromHdcInternal(param.hDC)) {
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(0xFF, 0xEE, 0xDD)),
                        Rectangle.FromLTRB(
                            param.rcItem.left,
                            param.rcItem.top,
                            param.rcItem.right,
                            param.rcItem.bottom));
                }
            }
            base.WndProc(ref m);
        }

        public IReadOnlyList<Option> Options {
            get { return options; }
        }

        private void PaintStyles(ListView listView)
        {
            var elements = (from c in typeof(VisualStyleElement).GetNestedTypes()
                            from p in c.GetNestedTypes()
                            from s in p.GetProperties()
                            select (VisualStyleElement)s.GetValue(null, null)).ToList();

            //VisualStylesNativeMethods.SetWindowTheme(listView.Handle, "Explorer", null);
            listView.OwnerDraw = true;

            try {
                listView.Paint += (s, e) => { };
                listView.DrawColumnHeader += (s, e) => { e.DrawDefault = true; };
                listView.DrawSubItem += (s, e) => {
                    e.DrawDefault = false;
                    //e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, Brushes.Black, e.Bounds);
                    //e.DrawText(TextFormatFlags.Left);
                };
                listView.DrawItem += (s, e) => {
                    bool draw = (e.State & ListViewItemStates.Hot) != 0;
                    if (draw) {
                        e.Graphics.FillRectangle(
                            new SolidBrush(Color.FromArgb(0x99, 0x33, 0xCC, 0x99)),
                            e.Bounds);
                        e.Graphics.FillRectangle(
                            Brushes.Black,
                            e.Bounds);
                        e.DrawBackground();
                        e.DrawText();
                        e.DrawFocusRectangle();
                        e.DrawDefault = false;
                    } else {
                        e.DrawDefault = true;
                    }

                    return;

                    var r = new VisualStyleRenderer(VisualStyleElement.ListView.Item.Normal);
                    r.DrawBackground(e.Graphics, e.Bounds);
                    return;

                    var theme = VisualStylesNativeMethods.OpenThemeData(listView.Handle, "LISTVIEW");
                    if (theme.IsInvalid)
                        return;
                    var hdc = new HandleRef(this, e.Graphics.GetHdc());
                    using (theme) {
                        int h = 1000;
                        var rect = new RECT(0, 20, 90, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 1, rect, new RECT(rect.left + 1, rect.top + 1, rect.right - 1, rect.bottom - 1));

                        rect = new RECT(100, 20, 190, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 2, rect, rect);

                        rect = new RECT(200, 20, 290, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 3, rect, rect);

                        rect = new RECT(300, 20, 390, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 4, rect, rect);

                        rect = new RECT(400, 20, 490, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 5, rect, rect);

                        rect = new RECT(500, 20, 590, 20 + h);
                        VisualStylesNativeMethods.DrawThemeBackground(
                            theme, hdc, 1, 6, rect, rect);
                    }
                };
            } catch {
            }

            //var renderer = new VisualStyleRenderer(VisualStyleElement.TreeView.Item.Normal);
            //renderer.DrawBackground(g, new Rectangle(0, 0, 100, 23));
            //renderer.DrawText(g, new Rectangle(0, 0, 100, 23), "text");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0xFF, 0xCC, 0x99)), e.ClipRectangle);
        }

        private OptionList CreateOptions()
        {
            var options = new OptionList();
            options.AddOption(sysListView, "Enabled", l => l.Enabled);

            //options.AddOption(sysListView, "LVS_ALIGNLEFT", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_ALIGNMASK", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_ALIGNTOP", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_AUTOARRANGE", l => l.AutoArrange);
            options.AddOption(sysListView, "LVS_EDITLABELS", l => l.LabelEdit);
            //options.AddOption(sysListView, "LVS_ICON", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_LIST", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_NOCOLUMNHEADER", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_NOLABELWRAP", l => l.LabelWrap, true);
            //options.AddOption(sysListView, "LVS_NOSCROLL", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_NOSORTHEADER", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_OWNERDATA", l => l.VirtualMode);
            //options.AddOption(sysListView, "LVS_OWNERDRAWFIXED", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_REPORT", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_SHAREIMAGELISTS", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_SHOWSELALWAYS", l => l.HideSelection, true);
            options.AddOption(sysListView, "LVS_SINGLESEL", l => l.MultiSelect, true);
            //options.AddOption(sysListView, "LVS_SMALLICON", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_SORTASCENDING", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_SORTDESCENDING", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_TYPEMASK", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_TYPESTYLEMASK", l => l.ShowPlusMinus);

            options.AddOption(sysListView, "LVS_EX_GRIDLINES", l => l.GridLines);
            //options.AddOption(sysListView, "LVS_EX_SUBITEMIMAGES", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_EX_CHECKBOXES", l => l.CheckBoxes);
            options.AddOption(sysListView, "LVS_EX_TRACKSELECT", l => l.HoverSelection);
            options.AddOption(sysListView, "LVS_EX_HEADERDRAGDROP", l => l.AllowColumnReorder);
            options.AddOption(sysListView, "LVS_EX_FULLROWSELECT", l => l.FullRowSelect);
            options.AddOption(sysListView, "LVS_EX_ONECLICKACTIVATE", l => l.Activation, ItemActivation.OneClick, ItemActivation.Standard);
            options.AddOption(sysListView, "LVS_EX_TWOCLICKACTIVATE", l => l.Activation, ItemActivation.TwoClick, ItemActivation.Standard);
            options.AddOption(sysListView, "LVS_EX_FLATSB", l => l.FlatScrollBars);
            //options.AddOption(sysListView, "LVS_EX_REGIONAL", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_EX_INFOTIP", l => l.ShowItemToolTips);
            options.AddOption(sysListView, "LVS_EX_UNDERLINEHOT", l => l.HotTracking);
            options.AddOption(sysListView, "LVS_EX_UNDERLINECOLD", l => l.ColdTracking);
            //options.AddOption(sysListView, "LVS_EX_MULTIWORKAREAS", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_EX_LABELTIP", l => l.LabelTip);
            options.AddOption(sysListView, "LVS_EX_BORDERSELECT", l => l.BorderSelect);
            options.AddOption(sysListView, "LVS_EX_DOUBLEBUFFER", l => l.UseDoubleBuffering);
            options.AddOption(sysListView, "LVS_EX_HIDELABELS", l => l.HideLabels);
            //options.AddOption(sysListView, "LVS_EX_SINGLEROW", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_EX_SNAPTOGRID", l => l.ShowPlusMinus);
            options.AddOption(sysListView, "LVS_EX_SIMPLESELECT", l => l.SimpleSelect);
            options.AddOption(sysListView, "LVS_EX_JUSTIFYCOLUMNS", l => l.JustifyColumns);
            options.AddOption(sysListView, "LVS_EX_TRANSPARENTBKGND", l => l.TransparentBackground);
            options.AddOption(sysListView, "LVS_EX_TRANSPARENTSHADOWTEXT", l => l.TransparentShadowBackground);
            options.AddOption(sysListView, "LVS_EX_AUTOAUTOARRANGE", l => l.AutoAutoArrange);
            options.AddOption(sysListView, "LVS_EX_HEADERINALLVIEWS", l => l.HeaderInAllViews);
            options.AddOption(sysListView, "LVS_EX_AUTOCHECKSELECT", l => l.AutoCheckSelect);
            options.AddOption(sysListView, "LVS_EX_AUTOSIZECOLUMNS", l => l.AutoSizeColumns);
            //options.AddOption(sysListView, "LVS_EX_COLUMNSNAPPOINTS", l => l.ShowPlusMinus);
            //options.AddOption(sysListView, "LVS_EX_COLUMNOVERFLOW", l => l.ShowPlusMinus);

            return options;

            /*
            #define LVS_ICON                0x0000
            #define LVS_REPORT              0x0001
            #define LVS_SMALLICON           0x0002
            #define LVS_LIST                0x0003
            #define LVS_TYPEMASK            0x0003
            #define LVS_SINGLESEL           0x0004
            #define LVS_SHOWSELALWAYS       0x0008
            #define LVS_SORTASCENDING       0x0010
            #define LVS_SORTDESCENDING      0x0020
            #define LVS_SHAREIMAGELISTS     0x0040
            #define LVS_NOLABELWRAP         0x0080
            #define LVS_AUTOARRANGE         0x0100
            #define LVS_EDITLABELS          0x0200
            #define LVS_OWNERDATA           0x1000
            #define LVS_NOSCROLL            0x2000
            #define LVS_TYPESTYLEMASK       0xfc00
            #define LVS_ALIGNTOP            0x0000
            #define LVS_ALIGNLEFT           0x0800
            #define LVS_ALIGNMASK           0x0c00
            #define LVS_OWNERDRAWFIXED      0x0400
            #define LVS_NOCOLUMNHEADER      0x4000
            #define LVS_NOSORTHEADER        0x8000

            #define LVS_EX_GRIDLINES        0x00000001
            #define LVS_EX_SUBITEMIMAGES    0x00000002
            #define LVS_EX_CHECKBOXES       0x00000004
            #define LVS_EX_TRACKSELECT      0x00000008
            #define LVS_EX_HEADERDRAGDROP   0x00000010
            #define LVS_EX_FULLROWSELECT    0x00000020 // applies to report mode only
            #define LVS_EX_ONECLICKACTIVATE 0x00000040
            #define LVS_EX_TWOCLICKACTIVATE 0x00000080
            #define LVS_EX_FLATSB           0x00000100
            #define LVS_EX_REGIONAL         0x00000200
            #define LVS_EX_INFOTIP          0x00000400 // listview does InfoTips for you
            #define LVS_EX_UNDERLINEHOT     0x00000800
            #define LVS_EX_UNDERLINECOLD    0x00001000
            #define LVS_EX_MULTIWORKAREAS   0x00002000
            #define LVS_EX_LABELTIP         0x00004000 // listview unfolds partly hidden labels if it does not have infotip text
            #define LVS_EX_BORDERSELECT     0x00008000 // border selection style instead of highlight
            #define LVS_EX_DOUBLEBUFFER     0x00010000
            #define LVS_EX_HIDELABELS       0x00020000
            #define LVS_EX_SINGLEROW        0x00040000
            #define LVS_EX_SNAPTOGRID       0x00080000  // Icons automatically snap to grid.
            #define LVS_EX_SIMPLESELECT     0x00100000  // Also changes overlay rendering to top right for icon mode.
            #define LVS_EX_JUSTIFYCOLUMNS   0x00200000  // Icons are lined up in columns that use up the whole view area.
            #define LVS_EX_TRANSPARENTBKGND 0x00400000  // Background is painted by the parent via WM_PRINTCLIENT
            #define LVS_EX_TRANSPARENTSHADOWTEXT 0x00800000  // Enable shadow text on transparent backgrounds only (useful with bitmaps)
            #define LVS_EX_AUTOAUTOARRANGE  0x01000000  // Icons automatically arrange if no icon positions have been set
            #define LVS_EX_HEADERINALLVIEWS 0x02000000  // Display column header in all view modes
            #define LVS_EX_AUTOCHECKSELECT  0x08000000
            #define LVS_EX_AUTOSIZECOLUMNS  0x10000000
            #define LVS_EX_COLUMNSNAPPOINTS 0x40000000
            #define LVS_EX_COLUMNOVERFLOW   0x80000000
                        */
        }

        private static void CheckBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;
            var option = checkBox.Tag as Option;
            if (option == null)
                return;
            option.Enabled = checkBox.Checked;
        }
    }
}
