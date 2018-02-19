namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class ScrollBarSampleNative : UserControl, IOptionControl
    {
        public ScrollBarSampleNative()
        {
            InitializeComponent();

            var options = new OptionList();
            options.AddOption(
                "Enabled", new ScrollBar[] { vScrollBar1, hScrollBar1 }, x => x.Enabled);
            Options = options;
        }

        public IReadOnlyList<Option> Options { get; }
    }
}
