namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class TabControlSampleWpf : IOptionControl
    {
        private readonly OptionList options = new OptionList();
        private bool singleTab;

        public TabControlSampleWpf()
        {
            InitializeComponent();

            options.AddOption("Enabled", tabControl1, l => l.IsEnabled);
            options.AddOption("Tabs Right", tabControl1, l => l.TabStripPlacement, Dock.Right, Dock.Left);
            options.AddOption("Tabs Bottom", tabControl1, l => l.TabStripPlacement, Dock.Bottom, Dock.Top);
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
                while (tabControl1.Items.Count > 1)
                    tabControl1.Items.RemoveAt(tabControl1.Items.Count - 1);
            } else {
                tabControl1.Items.Add(new TabItem { Header = "Hardware" });
                tabControl1.Items.Add(new TabItem { Header = "Advanced" });
            }
        }

        public IReadOnlyList<Option> Options => options;
    }
}
