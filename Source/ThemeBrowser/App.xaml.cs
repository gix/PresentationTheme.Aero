namespace ThemeBrowser
{
    using System.Linq;
    using System.Windows;
    using PresentationTheme.Aero;

    public partial class App
    {
        public App()
        {
            AeroTheme.SetAsCurrentTheme();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            var viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;

            if (e.Args.Length > 0) {
                bool highContrast = e.Args.Any(x => x == "-hc");
                string path = e.Args.FirstOrDefault(x => !x.StartsWith("-"));
                if (path != null)
                    viewModel.TryLoadTheme(path, highContrast);
            }

            MainWindow = window;
            window.Show();
        }
    }
}
