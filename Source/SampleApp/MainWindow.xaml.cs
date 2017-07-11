namespace SampleApp
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Diagnostics;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<ResourceDictionaryInfo> ThemedDictionaryLoads =>
            ((App)Application.Current).ThemedDictionaryLoads;

        private void OnExitMenuItemClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
