namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class TabControlSampleWpf : IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public TabControlSampleWpf()
        {
            InitializeComponent();

            options.AddOption("Enabled", tabControl1, l => l.IsEnabled);
            options.AddOption("Tabs Right", tabControl1, l => l.TabStripPlacement, Dock.Right, Dock.Left);
            options.AddOption("Tabs Bottom",tabControl1, l => l.TabStripPlacement, Dock.Bottom, Dock.Top);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
