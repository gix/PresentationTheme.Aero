namespace ThemePreviewer.Samples
{
    using System;
    using System.Windows.Forms;

    public partial class MiscSampleNative : UserControl
    {
        public MiscSampleNative()
        {
            InitializeComponent();
            UpdateToolTip();
        }

        private void OnToolTipTextChanged(object sender, EventArgs e)
        {
            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            toolTip1.SetToolTip(button1, textBox1.Text);
        }
    }
}
