namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class DataGridSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options;

        public DataGridSampleNative()
        {
            InitializeComponent();
            grid.DataSource = new ItemsCollection();

            options = new OptionList();
            options.AddOption("Enabled", grid, t => t.Enabled);
            options.AddOption("Multiple Rows", grid, c => c.MultiSelect);
            options.AddOption("Full Row Select", grid, c => c.SelectionMode,
                DataGridViewSelectionMode.FullRowSelect,
                DataGridViewSelectionMode.RowHeaderSelect);
            options.AddOption("Show Grid Lines", grid, c => c.CellBorderStyle,
                DataGridViewCellBorderStyle.Single,
                DataGridViewCellBorderStyle.None);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
