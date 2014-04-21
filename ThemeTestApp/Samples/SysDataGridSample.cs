namespace ThemeTestApp.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class SysDataGridSample : UserControl, IOptionControl
    {
        public SysDataGridSample()
        {
            InitializeComponent();
            dataGridView1.DataSource = new ItemsCollection();

            var options = new OptionList();
            options.AddOption(dataGridView1, "Enabled", t => t.Enabled);
            options.Add(Option.Create(dataGridView1, "Multiple Rows", c => c.MultiSelect));
            options.Add(Option.Create(dataGridView1, "Full Row Select", c => c.SelectionMode, DataGridViewSelectionMode.FullRowSelect, DataGridViewSelectionMode.RowHeaderSelect));
            Options = options;
        }

        public IReadOnlyList<Option> Options { get; private set; }
    }
}
