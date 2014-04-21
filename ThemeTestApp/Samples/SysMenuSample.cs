namespace ThemeTestApp.Samples
{
    using System.Windows.Forms;

    public partial class SysMenuSample : Form
    {
        private static int InstanceCount = 0;

        public SysMenuSample()
        {
            InstanceCount++;
            InitializeComponent();
            TopLevel = false;
            ContextMenu = contextMenu1;

            if (InstanceCount == 1) {
                var f = new SysMenuSample();
                f.TopLevel = true;
                f.FormBorderStyle = FormBorderStyle.Sizable;
                f.Show();
            }
        }
    }
}
