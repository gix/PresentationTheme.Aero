namespace ThemeBrowser
{
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Documents;
    using ThemeCore.Native;

    public partial class PreviewImageDialog
    {
        public PreviewImageDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;

            var layer = AdornerLayer.GetAdornerLayer(border);
            layer.Add(new ResizingAdorner(border));
        }
    }
}
