namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Windows.Forms;
    using Controls;

    public partial class ListViewSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options;

        public ListViewSampleNative()
        {
            InitializeComponent();

            sysListView.Items.AddRange(CreateItems().ToArray());
            sysListView.FullRowSelect = true;
            sysListView.HideSelection = false;
            sysListView.AllowColumnReorder = true;
            sysListView.ColumnClick += (s, e) => sysListView.ToggleSort(e.Column);

            explorerListView.Items.AddRange(CreateItems().ToArray());
            explorerListView.FullRowSelect = true;
            explorerListView.HideSelection = false;
            sysListView.AllowColumnReorder = true;
            explorerListView.ColumnClick += (s, e) => explorerListView.ToggleSort(e.Column);

            sysListView.Sort(0, SortOrder.Ascending);
            explorerListView.Sort(0, SortOrder.Descending);

            options = new OptionList();
            CreateOptions();
        }

        private IEnumerable<ListViewItem> CreateItems()
        {
            return ItemGenerator.Generate().Select(x => new ListViewItem(x));
        }

        public IReadOnlyList<Option> Options => options;

        private void AddOption(string name, Expression<Func<ListViewEx, bool>> expression,
            bool negated = false)
        {
            options.AddOption(name, sysListView, explorerListView, expression, negated);
        }

        private void AddOption<T>(
            string name, Expression<Func<ListViewEx, T>> expression, T trueValue, T falseValue)
        {
            options.AddOption(name, sysListView, explorerListView, expression, trueValue, falseValue);
        }

        private void CreateOptions()
        {
            AddOption("Enabled", l => l.Enabled);

            //AddOption("LVS_ALIGNLEFT", l => l.ShowPlusMinus);
            //AddOption("LVS_ALIGNMASK", l => l.ShowPlusMinus);
            //AddOption("LVS_ALIGNTOP", l => l.ShowPlusMinus);
            AddOption("LVS_AUTOARRANGE", l => l.AutoArrange);
            AddOption("LVS_EDITLABELS", l => l.LabelEdit);
            //AddOption("LVS_ICON", l => l.ShowPlusMinus);
            //AddOption("LVS_LIST", l => l.ShowPlusMinus);
            //AddOption("LVS_NOCOLUMNHEADER", l => l.ShowPlusMinus);
            AddOption("LVS_NOLABELWRAP", l => l.LabelWrap, true);
            //AddOption("LVS_NOSCROLL", l => l.ShowPlusMinus);
            //AddOption("LVS_NOSORTHEADER", l => l.ShowPlusMinus);
            AddOption("LVS_OWNERDATA", l => l.VirtualMode);
            //AddOption("LVS_OWNERDRAWFIXED", l => l.ShowPlusMinus);
            //AddOption("LVS_REPORT", l => l.ShowPlusMinus);
            //AddOption("LVS_SHAREIMAGELISTS", l => l.ShowPlusMinus);
            AddOption("LVS_SHOWSELALWAYS", l => l.HideSelection, true);
            AddOption("LVS_SINGLESEL", l => l.MultiSelect, true);
            //AddOption("LVS_SMALLICON", l => l.ShowPlusMinus);
            //AddOption("LVS_SORTASCENDING", l => l.ShowPlusMinus);
            //AddOption("LVS_SORTDESCENDING", l => l.ShowPlusMinus);
            //AddOption("LVS_TYPEMASK", l => l.ShowPlusMinus);
            //AddOption("LVS_TYPESTYLEMASK", l => l.ShowPlusMinus);

            AddOption("LVS_EX_GRIDLINES", l => l.GridLines);
            //AddOption("LVS_EX_SUBITEMIMAGES", l => l.ShowPlusMinus);
            AddOption("LVS_EX_CHECKBOXES", l => l.CheckBoxes);
            AddOption("LVS_EX_TRACKSELECT", l => l.HoverSelection);
            AddOption("LVS_EX_HEADERDRAGDROP", l => l.AllowColumnReorder);
            AddOption("LVS_EX_FULLROWSELECT", l => l.FullRowSelect);
            AddOption("LVS_EX_ONECLICKACTIVATE", l => l.Activation, ItemActivation.OneClick, ItemActivation.Standard);
            AddOption("LVS_EX_TWOCLICKACTIVATE", l => l.Activation, ItemActivation.TwoClick, ItemActivation.Standard);
            AddOption("LVS_EX_FLATSB", l => l.FlatScrollBars);
            //AddOption("LVS_EX_REGIONAL", l => l.ShowPlusMinus);
            AddOption("LVS_EX_INFOTIP", l => l.ShowItemToolTips);
            AddOption("LVS_EX_UNDERLINEHOT", l => l.HotTracking);
            AddOption("LVS_EX_UNDERLINECOLD", l => l.ColdTracking);
            //AddOption("LVS_EX_MULTIWORKAREAS", l => l.ShowPlusMinus);
            AddOption("LVS_EX_LABELTIP", l => l.LabelTip);
            AddOption("LVS_EX_BORDERSELECT", l => l.BorderSelect);
            AddOption("LVS_EX_DOUBLEBUFFER", l => l.UseDoubleBuffering);
            AddOption("LVS_EX_HIDELABELS", l => l.HideLabels);
            //AddOption("LVS_EX_SINGLEROW", l => l.ShowPlusMinus);
            //AddOption("LVS_EX_SNAPTOGRID", l => l.ShowPlusMinus);
            AddOption("LVS_EX_SIMPLESELECT", l => l.SimpleSelect);
            AddOption("LVS_EX_JUSTIFYCOLUMNS", l => l.JustifyColumns);
            AddOption("LVS_EX_TRANSPARENTBKGND", l => l.TransparentBackground);
            AddOption("LVS_EX_TRANSPARENTSHADOWTEXT", l => l.TransparentShadowBackground);
            AddOption("LVS_EX_AUTOAUTOARRANGE", l => l.AutoAutoArrange);
            AddOption("LVS_EX_HEADERINALLVIEWS", l => l.HeaderInAllViews);
            AddOption("LVS_EX_AUTOCHECKSELECT", l => l.AutoCheckSelect);
            AddOption("LVS_EX_AUTOSIZECOLUMNS", l => l.AutoSizeColumns);
            //AddOption("LVS_EX_COLUMNSNAPPOINTS", l => l.ShowPlusMinus);
            //AddOption("LVS_EX_COLUMNOVERFLOW", l => l.ShowPlusMinus);

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
    }
}
