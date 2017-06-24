namespace ThemePreviewer.Samples
{
    using System.Windows.Forms;

    public partial class CombinedSampleNative : UserControl
    {
        public CombinedSampleNative()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;

            var menuRoot = new MenuNode("<root>",
                new MenuNode("&File",
                    new MenuNode("&New", new MenuNode("Item")),
                    new MenuNode("&Open", new MenuNode("Item")) { IsChecked = true },
                    new MenuNode("Recent files", new MenuNode("Item")) { IsEnabled = false, IsChecked = true },
                    new MenuNode { IsSeparator = true },
                    new MenuNode("E&xit") { InputGestureText = "Alt+F4" }
                ),
                new MenuNode("&Edit")
            );

            fakeMenu1.RootNode = menuRoot;
        }
    }
}
