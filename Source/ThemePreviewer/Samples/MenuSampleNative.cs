namespace ThemePreviewer.Samples
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class MenuSampleNative : UserControl
    {

#if !NETCOREAPP
        private MainMenu menu;
#endif

        public MenuSampleNative()
        {
            InitializeComponent();
#if !NETCOREAPP
            menu = BuildMenu();
            ContextMenu = BuildContextMenu();
#endif

            var openButton = new Button();
            openButton.UseVisualStyleBackColor = true;
            openButton.Text = "Open form";
            openButton.Click += (sender, args) => {
                var form = new Form {
                    TopLevel = true,
                    BackColor = SystemColors.Window,
                    FormBorderStyle = FormBorderStyle.Sizable
                };
#if !NETCOREAPP
                form.Menu = menu;
#endif
                form.Show();
            };

            Controls.Add(openButton);
        }

#if !NETCOREAPP
        private MainMenu BuildMenu()
        {
            var menu = new MainMenu();
            foreach (var node in ItemGenerator.GetMenu().Children)
                menu.MenuItems.Add(BuildMenu(node));
            return menu;
        }

        private ContextMenu BuildContextMenu()
        {
            var menu = new ContextMenu();
            foreach (var node in ItemGenerator.GetMenu().Children)
                menu.MenuItems.Add(BuildMenu(node));
            return menu;
        }
#endif

        private MenuItem BuildMenu(MenuNode node)
        {
            var item = new MenuItem {
                Text = node.IsSeparator ? "-" : node.Text,
                Enabled = node.IsEnabled,
                Checked = node.IsChecked,
                RadioCheck = node.IsRadio,
                Shortcut = GetNativeShortcut(node.InputGestureText)
            };
            foreach (var childNode in node.Children)
                item.MenuItems.Add(BuildMenu(childNode));
            return item;
        }

        private static Shortcut GetNativeShortcut(string shortcut)
        {
            if (shortcut == null)
                return Shortcut.None;
            return (Shortcut)Enum.Parse(typeof(Shortcut), shortcut.Replace("+", ""));
        }
    }
}
