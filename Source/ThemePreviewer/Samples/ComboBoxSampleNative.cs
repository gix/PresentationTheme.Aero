namespace ThemePreviewer.Samples
{
    using System.Windows.Forms;

    public partial class ComboBoxSampleNative : UserControl
    {
        public ComboBoxSampleNative()
        {
            InitializeComponent();
            comboBox.SelectedIndex = 0;
            comboBoxDisabled.SelectedIndex = 0;

            comboBoxDisabledFlag.CheckedChanged += (s, e) => {
                comboBoxDisabled.Enabled = comboBoxDisabledFlag.Checked;
            };
            comboBoxEditableDisabledFlag.CheckedChanged += (s, e) => {
                comboBoxEditableDisabled.Enabled = comboBoxEditableDisabledFlag.Checked;
            };
        }
    }
}
