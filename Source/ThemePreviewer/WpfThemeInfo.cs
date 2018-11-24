namespace ThemePreviewer
{
    using System;
    using System.IO;
    using System.IO.Packaging;
    using System.Reflection;
    using System.Text.RegularExpressions;
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

            BaseName = Regex.Match(name, @"\A([^()]+?)(\(.*\))?\z").Groups[1].Value;
        }

        public string BaseName { get; }
        public string Name { get; }
        public Uri ResourceUri { get; }
        public UxColorScheme CustomColors { get; }

        public ResourceDictionary CreateResources()
        {
            try {
                if (ResourceUri != null)
                    return LoadComponentFromAssembly(ResourceUri) as ResourceDictionary;
            } catch (FileNotFoundException) {
            } catch (FileLoadException) {
            } catch (BadImageFormatException) {
            }

            return null;
        }

        private static object LoadComponentFromAssembly(Uri uri)
        {
#if NETCOREAPP3_0
            var method = typeof(PackUriHelper).GetMethod("ValidateAndGetPackUriComponents",
                BindingFlags.NonPublic | BindingFlags.Static);
            var args = new object[3];
            args[0] = uri;
            method.Invoke(null, args);
            var partUri = (Uri)args[2];
            return partUri;
#else
            return Application.LoadComponent(PackUriHelper.GetPartUri(uri));
#endif
        }

        private static Uri GetUri(string themeAssembly, string themePath)
        {
            return new Uri(
                $"pack://application:,,,/{themeAssembly};component/{themePath}",
                UriKind.Absolute);
        }
    }
}
