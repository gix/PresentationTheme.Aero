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
            dataGridView1.DataSource = new ItemsCollection();

            options = new OptionList();
            options.AddOption("Enabled", dataGridView1, t => t.Enabled);
            options.AddOption("Multiple Rows", dataGridView1, c => c.MultiSelect);
            options.AddOption("Full Row Select", dataGridView1, c => c.SelectionMode,
                DataGridViewSelectionMode.FullRowSelect,
                DataGridViewSelectionMode.RowHeaderSelect);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
