namespace ThemeTestApp.Samples
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WpfDataGridSample.xaml
    /// </summary>
    public partial class WpfDataGridSample : IOptionControl
    {
        public WpfDataGridSample()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ItemsCollection();
            var options = new OptionList();
            options.AddOption(dataGrid, "Enabled", l => l.IsEnabled);
            options.AddOption(dataGrid, "Multiple Rows", c => c.SelectionMode, DataGridSelectionMode.Extended, DataGridSelectionMode.Single);
            options.AddOption(dataGrid, "Full Row Select", c => c.SelectionUnit, DataGridSelectionUnit.FullRow, DataGridSelectionUnit.CellOrRowHeader);
            Options = options;
        }

        public ItemsCollection Items { get; private set; }

        public IReadOnlyList<Option> Options { get; private set; }
    }
}
