namespace ThemePreviewer
{
    using System;

    public static class BuiltinThemes
    {
        public static Uri ClassicUri { get; } = new Uri(
            "pack://application:,,,/PresentationFramework.Classic;V4.0.0.0;31bf3856ad364e35;component/Themes/classic.xaml",
            UriKind.Absolute);
        public static Uri AeroUri { get; } = new Uri(
            "pack://application:,,,/PresentationFramework.Aero;V4.0.0.0;31bf3856ad364e35;component/Themes/Aero.NormalColor.xaml",
            UriKind.Absolute);
        public static Uri Aero2Uri { get; } = new Uri(
            "pack://application:,,,/PresentationFramework.Aero2;V4.0.0.0;31bf3856ad364e35;component/Themes/Aero2.NormalColor.xaml",
            UriKind.Absolute);
        public static Uri AeroLiteUri { get; } = new Uri(
            "pack://application:,,,/PresentationFramework.AeroLite;V4.0.0.0;31bf3856ad364e35;component/Themes/AeroLite.NormalColor.xaml",
            UriKind.Absolute);
        public static Uri RoyaleUri { get; } = new Uri(
            "pack://application:,,,/PresentationFramework.Royale;V4.0.0.0;31bf3856ad364e35;component/Themes/Royale.NormalColor.xaml",
            UriKind.Absolute);
    }
}
