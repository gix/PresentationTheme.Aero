namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows;

    public partial class ProgressBarSampleWpf : IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public ProgressBarSampleWpf()
        {
            InitializeComponent();
            CreateOptions();
            valueButton.Click += OnValueButtonClicked;
        }

        public IReadOnlyList<Option> Options => options;

        private void OnValueButtonClicked(object sender, RoutedEventArgs args)
        {
            bar.Value = (int)(bar.Value + 15) % (int)bar.Maximum;
        }

        private void CreateOptions()
        {
            options.AddOption("Indeterminate", new[] { bar1, bar2, bar3 }, x => x.IsIndeterminate);
        }
    }
}
