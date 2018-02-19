namespace ThemePreviewer
{
    using System;
    using System.IO.Packaging;
    using System.Windows;
    using ThemeCore.Native;

    public class WpfThemeInfo
    {
        public WpfThemeInfo(
            string name, string themeAssembly, string themePath, UxColorScheme customColors = null)
            : this(name, GetUri(themeAssembly, themePath), customColors)
        {
        }

        public WpfThemeInfo(string name, Uri uri, UxColorScheme customColors = null)
        {
            Name = name;
            ResourceUri = uri;
            CustomColors = customColors;
        }

        public string Name { get; }
        public Uri ResourceUri { get; }
        public UxColorScheme CustomColors { get; }

        public ResourceDictionary CreateResources()
        {
            if (ResourceUri != null)
                return LoadComponentFromAssembly(ResourceUri) as ResourceDictionary;
            return null;
        }

        private static object LoadComponentFromAssembly(Uri uri)
        {
            return Application.LoadComponent(PackUriHelper.GetPartUri(uri));
        }

        private static Uri GetUri(string themeAssembly, string themePath)
        {
            return new Uri(
                $"pack://application:,,,/{themeAssembly};component/{themePath}",
                UriKind.Absolute);
        }
    }
}
