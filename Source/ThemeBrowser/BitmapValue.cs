namespace ThemeBrowser
{
    using System.Windows.Media.Imaging;

    internal class BitmapValue
    {
        public BitmapValue(BitmapSource source)
        {
            Source = source;
        }

        public BitmapSource Source { get; }
    }
}