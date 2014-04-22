namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WpfDataGridSample.xaml
    /// </summary>
    public partial class DataGridSampleWpf : IOptionControl
    {
        public DataGridSampleWpf()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ItemsCollection();
            var options = new OptionList();
            options.AddOption("Enabled", dataGrid, l => l.IsEnabled);
            options.AddOption("Multiple Rows", dataGrid, c => c.SelectionMode, DataGridSelectionMode.Extended, DataGridSelectionMode.Single);
            options.AddOption("Full Row Select", dataGrid, c => c.SelectionUnit, DataGridSelectionUnit.FullRow, DataGridSelectionUnit.CellOrRowHeader);
            Options = options;
        }

        public ItemsCollection Items { get; private set; }

        public IReadOnlyList<Option> Options { get; private set; }
    }
}
