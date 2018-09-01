namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class TabControlSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options = new OptionList();
        private bool singleTab;

        public TabControlSampleNative()
        {
            InitializeComponent();
            CreateOptions();
        }

        public IReadOnlyList<Option> Options => options;

        private void CreateOptions()
        {
            options.AddOption("Enabled", tabControl1, x => x.Enabled);
            //options.AddOption(tabControl1, "TCS_SCROLLOPPOSITE", x => x.); // 0x0001
            options.AddOption("TCS_BOTTOM", tabControl1, x => x.Alignment, TabAlignment.Bottom, TabAlignment.Top);
            options.AddOption("TCS_RIGHT", tabControl1, x => x.Alignment, TabAlignment.Right, TabAlignment.Left);
            //options.AddOption(tabControl1, "TCS_MULTISELECT", x => x.); // 0x0004
            options.AddOption("TCS_FLATBUTTONS", tabControl1, x => x.Appearance, TabAppearance.FlatButtons, TabAppearance.Normal);
            //options.AddOption(tabControl1, "TCS_FORCEICONLEFT", x => x.); // 0x0010
            //options.AddOption(tabControl1, "TCS_FORCELABELLEFT", x => x.); // 0x0020
            options.AddOption("TCS_HOTTRACK", tabControl1, x => x.HotTrack);
            //options.AddOption(tabControl1, "TCS_VERTICAL", x => x.); // 0x0080
            options.AddOption("TCS_TABS", tabControl1, x => x.Tabs); // 0x0000
            options.AddOption("TCS_BUTTONS", tabControl1, x => x.Appearance, TabAppearance.Buttons, TabAppearance.Normal);
            //options.AddOption(tabControl1, "TCS_SINGLELINE", x => x.); // 0x0000
            options.AddOption("TCS_MULTILINE", tabControl1, x => x.Multiline);
            //options.AddOption(tabControl1, "TCS_RIGHTJUSTIFY", x => x.); // 0x0000
            options.AddOption("TCS_FIXEDWIDTH", tabControl1, x => x.SizeMode, TabSizeMode.Fixed, TabSizeMode.Normal);
            options.AddOption("TCS_RAGGEDRIGHT", tabControl1, x => x.SizeMode, TabSizeMode.Normal, TabSizeMode.FillToRight);
            //options.AddOption(tabControl1, "TCS_FOCUSONBUTTONDOWN", x => x.); // 0x1000
            options.AddOption("TCS_OWNERDRAWFIXED", tabControl1, x => x.DrawMode, TabDrawMode.OwnerDrawFixed, TabDrawMode.Normal);
            options.AddOption("TCS_TOOLTIPS", tabControl1, x => x.ShowToolTips);
            //options.AddOption(tabControl1, "TCS_FOCUSNEVER", x => x.); // 0x8000

            //options.AddOption(tabControl1, "TCS_EX_FLATSEPARATORS", x => x.); // 0x00000001
            //options.AddOption(tabControl1, "TCS_EX_REGISTERDROP", x => x.); // 0x00000002
            options.Add(new GenericBoolOption("Single Tab", GetSingleTab, SetSingleTab));
        }

        private bool GetSingleTab()
        {
            return singleTab;
        }

        private void SetSingleTab(bool value)
        {
            singleTab = value;
            if (value) {
                while (tabControl1.TabPages.Count > 1)
                    tabControl1.TabPages.RemoveAt(tabControl1.TabPages.Count - 1);
            } else {
                tabControl1.TabPages.Add(new TabPage { Text = "Hardware" });
                tabControl1.TabPages.Add(new TabPage { Text = "Advanced" });
            }
        }
    }
}
