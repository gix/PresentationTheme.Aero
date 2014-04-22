namespace ThemePreviewer
{
    using System;
    using System.Windows;

    public class Theme
    {
        private readonly Func<ResourceDictionary> resources;

        public Theme(string name, string themeAssembly, string themePath)
            : this(name, GetUri(themeAssembly, themePath))
        {
        }

        public Theme(string name, Uri uri)
        {
            Name = name;

            ResourceUri = uri;
            if (uri != null)
                resources = () => App.LoadComponentFromAssembly(uri) as ResourceDictionary;
            else
                resources = () => null;
        }

        private static Uri GetUri(string themeAssembly, string themePath)
        {
            return new Uri(
                $"pack://application:,,,/{themeAssembly};component/{themePath}",
                UriKind.Absolute);
        }

        public string Name { get; }
        public Uri ResourceUri { get; }
        public ResourceDictionary CreateResources() => resources();
    }
}
