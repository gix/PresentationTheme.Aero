namespace ThemePreviewer.Samples
{
    using System.Windows.Forms;

    public partial class MenuSampleNative : Form
    {
        private static int InstanceCount;

        public MenuSampleNative()
        {
            InstanceCount++;
            InitializeComponent();
            TopLevel = false;
            ContextMenu = contextMenu1;

            if (InstanceCount == 1) {
                var b = new Button();
                b.Text = "Open form";
                b.Click += (sender, args) => {
                    var f = new MenuSampleNative();
                    f.TopLevel = true;
                    f.FormBorderStyle = FormBorderStyle.Sizable;
                    f.Show();
                };
                Controls.Add(b);
            }
        }
    }
}
