namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;

    public partial class ToolBarSampleWpf : IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public ToolBarSampleWpf()
        {
            InitializeComponent();

            options.AddOption("Enabled", toolBarTray, x => x.IsEnabled);
            options.AddOption("IsLocked", toolBarTray, x => x.IsLocked);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
