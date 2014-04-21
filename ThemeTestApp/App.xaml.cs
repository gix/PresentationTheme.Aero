namespace ThemeTestApp
{
    using System.Windows;

    public partial class App
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
