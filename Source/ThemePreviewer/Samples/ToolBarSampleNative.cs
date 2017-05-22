namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class ToolBarSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public ToolBarSampleNative()
        {
            InitializeComponent();
            options.AddOption("Enabled", toolBar1, x => x.Enabled);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
