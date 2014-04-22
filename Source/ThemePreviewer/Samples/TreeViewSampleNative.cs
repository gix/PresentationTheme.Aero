namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Windows.Forms;
    using Controls;

    public partial class TreeViewSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public TreeViewSampleNative()
        {
            InitializeComponent();
            AddItem(sysTreeView.Nodes, ItemGenerator.GetTree());
            AddItem(explorerTreeView.Nodes, ItemGenerator.GetTree());

            CreateOptions();
        }

        public IReadOnlyList<Option> Options => options;

        private void AddItem(TreeNodeCollection items, ItemGenerator.TreeNode root)
        {
            var item = new TreeNode(root.Name);
            item.Expand();

            foreach (var child in root.Children)
                AddItem(item.Nodes, child);

            items.Add(item);
        }

        private void OnStateChanged(object sender, StateEventArgs stateEventArgs)
        {
            Expand(sysTreeView.Nodes);
        }

        private void AddOption(string name, Expression<Func<TreeViewEx, bool>> expression,
            bool negated = false)
        {
            options.AddOption(name, sysTreeView, explorerTreeView, expression, negated);
        }

        private void AddOption(string name, Expression<Func<TreeViewEx, int>> expression)
        {
            options.AddOption(name, sysTreeView, explorerTreeView, expression);
        }

        private void AddOption<T>(
            string name, Expression<Func<TreeViewEx, T>> expression, T trueValue, T falseValue)
        {
            options.AddOption(name, sysTreeView, explorerTreeView, expression, trueValue, falseValue);
        }

        private void CreateOptions()
        {
            options.StateChanged += OnStateChanged;

            AddOption("Enabled", t => t.Enabled);
            AddOption("TVS_HASBUTTONS", t => t.ShowPlusMinus);
            AddOption("TVS_HASLINES", t => t.ShowLines);
            AddOption("TVS_LINESATROOT", t => t.ShowRootLines);
            AddOption("TVS_EDITLABELS", t => t.LabelEdit);
            //AddOption("TVS_DISABLEDRAGDROP     0x0010
            AddOption("TVS_SHOWSELALWAYS", t => t.HideSelection, true);
            AddOption("TVS_RTLREADING", t => t.RightToLeftLayout);
            //AddOption("TVS_NOTOOLTIPS          0x0080
            AddOption("TVS_CHECKBOXES", t => t.CheckBoxes);
            AddOption("TVS_TRACKSELECT", t => t.HotTracking);
            //AddOption("TVS_SINGLEEXPAND        0x0400
            AddOption("TVS_INFOTIP", t => t.ShowNodeToolTips);
            AddOption("TVS_FULLROWSELECT", t => t.FullRowSelect);
            AddOption("TVS_NOSCROLL", t => t.Scrollable, true);
            //AddOption("TVS_NONEVENHEIGHT       0x4000  setOddHeight
            //AddOption("TVS_NOHSCROLL           0x8000 // TVS_NOSCROLL overrides this

            AddOption("TVS_EX_MULTISELECT", t => t.MultiSelect);
            AddOption("TVS_EX_DOUBLEBUFFER", t => t.UseDoubleBuffering);
            AddOption("TVS_EX_NOINDENTSTATE", t => t.NoIndent);
            AddOption("TVS_EX_RICHTOOLTIP", t => t.RichTooltip);
            AddOption("TVS_EX_AUTOHSCROLL", t => t.AutoHorizontalScroll);
            AddOption("TVS_EX_FADEINOUTEXPANDOS", t => t.FadeInOutExpandos);
            AddOption("TVS_EX_PARTIALCHECKBOXES", t => t.PartialCheckBoxes);
            AddOption("TVS_EX_EXCLUSIONCHECKBOXES", t => t.ExclusionCheckBoxes);
            AddOption("TVS_EX_DIMMEDCHECKBOXES", t => t.DimmedCheckBoxes);
            AddOption("TVS_EX_DRAWIMAGEASYNC", t => t.DrawImageAsync);

            AddOption("ItemHeight", t => t.ItemHeight);
            AddOption("Indent", t => t.Indent);

            /*
#define TVS_HASBUTTONS          0x0001  ShowPlusMinus
#define TVS_HASLINES            0x0002  ShowLines
#define TVS_LINESATROOT         0x0004  ShowRootLines
#define TVS_EDITLABELS          0x0008  LabelEdit
#define TVS_DISABLEDRAGDROP     0x0010
#define TVS_SHOWSELALWAYS       0x0020  !HideSelection
#define TVS_RTLREADING          0x0040  RightToLeftLayout
#define TVS_NOTOOLTIPS          0x0080
#define TVS_CHECKBOXES          0x0100  CheckBoxes
#define TVS_TRACKSELECT         0x0200  HotTracking
#define TVS_SINGLEEXPAND        0x0400
#define TVS_INFOTIP             0x0800  ShowNodeToolTips
#define TVS_FULLROWSELECT       0x1000  FullRowSelect
#define TVS_NOSCROLL            0x2000  !Scrollable
#define TVS_NONEVENHEIGHT       0x4000  setOddHeight
#define TVS_NOHSCROLL           0x8000 // TVS_NOSCROLL overrides this

#define WS_OVERLAPPED       0x00000000L
#define WS_POPUP            0x80000000L
#define WS_CHILD            0x40000000L
#define WS_MINIMIZE         0x20000000L
#define WS_VISIBLE          0x10000000L
#define WS_DISABLED         0x08000000L
#define WS_CLIPSIBLINGS     0x04000000L
#define WS_CLIPCHILDREN     0x02000000L
#define WS_MAXIMIZE         0x01000000L
#define WS_CAPTION          0x00C00000L     // WS_BORDER | WS_DLGFRAME
#define WS_BORDER           0x00800000L   borderStyle == BorderStyle.Fixed
#define WS_DLGFRAME         0x00400000L
#define WS_VSCROLL          0x00200000L  default
#define WS_HSCROLL          0x00100000L  default
#define WS_SYSMENU          0x00080000L
#define WS_THICKFRAME       0x00040000L
#define WS_GROUP            0x00020000L
#define WS_TABSTOP          0x00010000L

#define WS_MINIMIZEBOX      0x00020000L
#define WS_MAXIMIZEBOX      0x00010000L
#define WS_EX_CLIENTEDGE        0x00000200L   borderStyle == BorderStyle.Fixed3D
             */
        }

        private static void CheckBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;
            var option = checkBox.Tag as BoolOption;
            if (option == null)
                return;
            option.Enabled = checkBox.Checked;
        }

        private static void Expand(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes) {
                node.Expand();
                Expand(node.Nodes);
            }
        }
    }
}
