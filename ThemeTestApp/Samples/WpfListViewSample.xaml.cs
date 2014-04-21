namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for WpfListViewSample.xaml
    /// </summary>
    public partial class WpfListViewSample : IOptionControl
    {
        private readonly OptionList options;

        public WpfListViewSample()
        {
            InitializeComponent();

            var items = new[] {
                Tuple.Create("Item1", "439 KB", "Zip Archive"),
                Tuple.Create("stuff.7z", "847 KB", "7z Archive"),
                Tuple.Create("unknown10.flac", "37.453 KB", "FLAC"),
                Tuple.Create("1", "", ""),
                Tuple.Create("2", "", ""),
                Tuple.Create("3", "", ""),
                Tuple.Create("4", "", ""),
                Tuple.Create("5", "", ""),
                Tuple.Create("6", "", ""),
                Tuple.Create("7", "", ""),
                Tuple.Create("8", "", ""),
                Tuple.Create("9", "", ""),
                Tuple.Create("10", "", ""),
                Tuple.Create("11", "", ""),
                Tuple.Create("12", "", ""),
                Tuple.Create("13", "", ""),
                Tuple.Create("14", "", ""),
                Tuple.Create("15", "", ""),
                Tuple.Create("16", "", ""),
                Tuple.Create("17", "", ""),
                Tuple.Create("18", "", ""),
                Tuple.Create("19", "", ""),
                Tuple.Create("20", "", ""),
                Tuple.Create("21", "", ""),
                Tuple.Create("22", "", ""),
                Tuple.Create("23", "", ""),
                Tuple.Create("24", "", ""),
                Tuple.Create("25", "", ""),
                Tuple.Create("26", "", ""),
                Tuple.Create("27", "", ""),
                Tuple.Create("28", "", ""),
                Tuple.Create("29", "", ""),
                Tuple.Create("30", "", ""),
            };

            foreach (var item in items) {
                lv1.Items.Add(item);
                lv2.Items.Add(item);
            }

            options = new OptionList();
            options.AddOption(lv1, "Enabled", l => l.IsEnabled);
        }

        public IReadOnlyList<Option> Options
        {
            get { return options; }
        }
    }
}
