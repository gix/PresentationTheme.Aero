namespace ThemePreviewer.Samples
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class MenuSampleNative : UserControl
    {
        private MainMenu menu;

        public MenuSampleNative()
        {
            InitializeComponent();
            menu = BuildMenu();
            ContextMenu = BuildContextMenu();

            var openButton = new Button();
            openButton.UseVisualStyleBackColor = true;
            openButton.Text = "Open form";
            openButton.Click += (sender, args) => {
                var form = new Form {
                    TopLevel = true,
                    BackColor = SystemColors.Window,
                    FormBorderStyle = FormBorderStyle.Sizable
                };
                form.Menu = menu;
                form.Show();
            };

            Controls.Add(openButton);
        }

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
