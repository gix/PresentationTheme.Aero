namespace ThemeBrowser
{
    using System.Windows;
    using PresentationTheme.Aero.Win10;

    public partial class App
    {
        public App()
        {
            AeroWin10Theme.SetCurrentTheme();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            var viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;

            if (e.Args.Length == 1)
                viewModel.TryLoadTheme(e.Args[0]);

            MainWindow = window;
            window.Show();
        }
    }
}
