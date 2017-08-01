namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class DataGridSampleWpf : IOptionControl
    {
        public DataGridSampleWpf()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ItemsCollection();

            var options = new OptionList();
            options.AddOption("Enabled", dataGrid, l => l.IsEnabled);
            options.AddOption("Multiple Rows",
                dataGrid, c => c.SelectionMode,
                DataGridSelectionMode.Extended,
                DataGridSelectionMode.Single);
            options.AddOption("Full Row Select",
                dataGrid, c => c.SelectionUnit,
                DataGridSelectionUnit.FullRow,
                DataGridSelectionUnit.CellOrRowHeader);
            options.AddEnumOption("Grid Lines",
                dataGrid, c => c.GridLinesVisibility);
            options.AddEnumOption("Headers",
                dataGrid, c => c.HeadersVisibility);
            options.AddOption("Wide Row Header",
                dataGrid, c => c.RowHeaderWidth,
                25,
                double.NaN);
            options.AddOption("Frozen Columns", dataGrid, c => c.FrozenColumnCount);
            options.AddOption("Row Details", dataGrid,
                c => c.RowDetailsVisibilityMode,
                DataGridRowDetailsVisibilityMode.Visible,
                DataGridRowDetailsVisibilityMode.Collapsed);
            Options = options;
        }

        public ItemsCollection Items { get; }

        public IReadOnlyList<Option> Options { get; }
    }
}
