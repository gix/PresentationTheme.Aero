namespace ThemeBrowser
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnSelectedItemChanged(
            object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel != null)
                viewModel.SelectedItem = args.NewValue;
        }

        private void OnTreeViewContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (DataContext as MainWindowViewModel)?.GetTreeContextMenu();
            if (menu != null)
                menu.IsOpen = true;
        }
    }
}
