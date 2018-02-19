namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;

    public partial class ListBoxSampleWpf : IOptionControl
    {
        private readonly OptionList options;

        public ListBoxSampleWpf()
        {
            InitializeComponent();

            foreach (var item in ItemGenerator.Generate(90)) {
                lv1.Items.Add(item[0]);
            }

            options = new OptionList();
            options.AddOption("Enabled", lv1, l => l.IsEnabled);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
