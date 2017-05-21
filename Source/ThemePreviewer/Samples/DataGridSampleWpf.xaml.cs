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
            options.AddOption("Show Grid Lines",
                dataGrid, c => c.GridLinesVisibility,
                DataGridGridLinesVisibility.All,
                DataGridGridLinesVisibility.None);
            Options = options;
        }

        public ItemsCollection Items { get; }

        public IReadOnlyList<Option> Options { get; }
    }
}
