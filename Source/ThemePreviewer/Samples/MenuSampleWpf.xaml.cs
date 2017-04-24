namespace ThemePreviewer.Samples
{
    using System.Windows.Controls;

    public partial class MenuSampleWpf
    {
        public MenuSampleWpf()
        {
            InitializeComponent();
            menu.Items.Clear();
            foreach (var node in ItemGenerator.GetMenu().Children)
                menu.Items.Add(BuildMenu(node));
        }

        private object BuildMenu(ItemGenerator.MenuNode node)
        {
            if (node.IsSeparator)
                return new Separator();

            var item = new MenuItem {
                Header = node.Text.Replace('&', '_'),
                IsEnabled = node.IsEnabled,
                IsChecked = node.IsChecked,
                InputGestureText = node.InputGestureText
            };
            foreach (var childNode in node.Children)
                item.Items.Add(BuildMenu(childNode));
            return item;
        }
    }
}
