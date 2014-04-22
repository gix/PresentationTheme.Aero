namespace ThemePreviewer
{
    using System;

    public static class BuiltinThemes
    {
        public static Uri ClassicUri = new Uri(
            "pack://application:,,,/PresentationFramework.Classic;V4.0.0.0;31bf3856ad364e35;component/Themes/classic.xaml",
            UriKind.Absolute);
        public static Uri AeroUri = new Uri(
            "pack://application:,,,/PresentationFramework.Aero;V4.0.0.0;31bf3856ad364e35;component/Themes/Aero.NormalColor.xaml",
            UriKind.Absolute);
        public static Uri Aero2Uri = new Uri(
            "pack://application:,,,/PresentationFramework.Aero2;V4.0.0.0;31bf3856ad364e35;component/Themes/Aero2.NormalColor.xaml",
            UriKind.Absolute);
        public static Uri RoyaleUri = new Uri(
            "pack://application:,,,/PresentationFramework.Royale;V4.0.0.0;31bf3856ad364e35;component/Themes/Royale.NormalColor.xaml",
            UriKind.Absolute);
    }
}
