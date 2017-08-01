namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class ProgressBarSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public ProgressBarSampleNative()
        {
            InitializeComponent();

            CreateOptions();
        }

        private void CreateOptions()
        {
            options.Add(new IntControlOption<ProgressBar>(
                new[] { progressBar7 },
                "MarqueeSpeed",
                c => c.MarqueeAnimationSpeed,
                (c, v) => c.MarqueeAnimationSpeed = v));
        }

        public IReadOnlyList<Option> Options => options;

        private void OnValueButtonClicked(object sender, System.EventArgs e)
        {
            progressBar9.Value = (progressBar9.Value + 15) % progressBar9.Maximum;
        }

        private void OnChangeStateButtonClicked(object sender, System.EventArgs e)
        {
            progressBar9.State = (ProgressBarState)((int)progressBar9.State % (int)ProgressBarState.Paused + 1);
        }
    }
}
