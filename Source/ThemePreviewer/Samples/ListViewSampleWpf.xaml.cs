namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class ListViewSampleWpf : IOptionControl
    {
        private readonly OptionList options;

        public ListViewSampleWpf()
        {
            InitializeComponent();

            foreach (var item in ItemGenerator.Generate()) {
                var tuple = Tuple.Create(item[0], item[1], item[2]);
                lv1.Items.Add(tuple);
                lv2.Items.Add(tuple);
            }

            options = new OptionList();
            options.AddOption("Enabled", lv1, l => l.IsEnabled);
            options.AddOption("Enabled", lv2, l => l.IsEnabled);

            options.Add(new GenericBoolOption(
                "GridView", () => lv1.View is GridView,
                v => {
                    lv1.View = v ? gridView1 : null;
                    lv2.View = v ? gridView2 : null;
                }));
        }

        public IReadOnlyList<Option> Options => options;
    }
}
