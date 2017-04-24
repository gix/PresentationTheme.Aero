namespace ThemePreviewer
{
    using System.Windows;
    using System.Windows.Media;
    using PresentationTheme.Aero.Win10;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTextFormattingFlagClicked(object sender, RoutedEventArgs e)
        {
            var mode = textFormattingFlag.IsChecked == true ?
                TextFormattingMode.Display : TextFormattingMode.Ideal;

            TextOptions.SetTextFormattingMode(this, mode);
        }

        private void OnAnimationFlagClicked(object sender, RoutedEventArgs e)
        {
            AeroWin10Theme.UseAnimationsOverride = animationFlag.IsChecked;
        }
    }
}
