namespace NetCoreApp
{
    using System.Collections.ObjectModel;
    using System.Windows;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<ResourceDictionaryLoad> ThemedDictionaryLoads =>
           App.ThemedDictionaryLoads;

        public App App => (App)Application.Current;

        private void OnExitMenuItemClicked(object sender, RoutedEventArgs e)
        {
            App.Shutdown();
        }

        private void OnUseThemeManagerClicked(object sender, RoutedEventArgs e)
        {
            App.UseThemeManager = !App.UseThemeManager;
            App.Restart();
        }

        private void OnUseAeroThemeClicked(object sender, RoutedEventArgs e)
        {
            App.UseAeroTheme = !App.UseAeroTheme;
            App.Restart();
        }
    }
}
