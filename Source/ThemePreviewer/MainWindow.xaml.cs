namespace ThemePreviewer
{
    using System.Windows;
    using System.Windows.Media;
    using PresentationTheme.Aero;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            animationFlag.IsChecked = SystemVisualStateManager.Instance.UseAnimationsOverride;
        }

        private void OnTextFormattingFlagClicked(object sender, RoutedEventArgs e)
        {
            var mode = textFormattingFlag.IsChecked == true ?
                TextFormattingMode.Display : TextFormattingMode.Ideal;

            TextOptions.SetTextFormattingMode(this, mode);
        }

        private void OnAnimationFlagClicked(object sender, RoutedEventArgs e)
        {
            SystemVisualStateManager.Instance.UseAnimationsOverride = animationFlag.IsChecked;
        }
    }
}
