namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using PresentationTheme.Aero.Win10;
    using PresentationTheme.Aero.Win7;
    using PresentationTheme.Aero.Win8;
    using PresentationTheme.AeroLite.Win10;

    public class ThemeInfoProvider
    {
        private readonly List<ThemeInfoPair> themes = new List<ThemeInfoPair>();
        private readonly List<WpfThemeInfo> wpfThemes = new List<WpfThemeInfo>();
        private readonly List<NativeThemeInfo> nativeThemes = new List<NativeThemeInfo>();

        public ThemeInfoProvider()
        {
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 7)", AeroWin7Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 8)", AeroWin8Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 10)", AeroWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero Lite (Windows 10)", AeroLiteWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Classic", BuiltinThemes.ClassicUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Aero", BuiltinThemes.AeroUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Aero 2", BuiltinThemes.Aero2Uri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Royale", BuiltinThemes.RoyaleUri));

            foreach (var nativeTheme in FindNativeThemes().OrderBy(x => x))
                nativeThemes.Add(nativeTheme);

            foreach (var nativeTheme in nativeThemes) {
                foreach (var wpfTheme in wpfThemes) {
                    if (nativeTheme.Name == wpfTheme.Name)
                        themes.Add(new ThemeInfoPair(nativeTheme.Name, nativeTheme, wpfTheme));
                }
            }
        }

        public IReadOnlyList<ThemeInfoPair> Themes => themes;
        public IReadOnlyList<WpfThemeInfo> WpfThemes => wpfThemes;
        public IReadOnlyList<NativeThemeInfo> NativeThemes => nativeThemes;

        private IEnumerable<NativeThemeInfo> FindNativeThemes()
        {
            return FindAllNativeThemes().GroupBy(x => x.Name).Select(x => x.Max());
        }

        private IEnumerable<NativeThemeInfo> FindAllNativeThemes()
        {
            var revisionDirs = EnumerateDataDirectories().SelectMany(x => x.EnumerateDirectories("*.*.*"));
            foreach (var revDir in revisionDirs) {
                foreach (var styleFile in revDir.EnumerateFiles("aero.msstyles")) {
                    var versionInfo = FileVersionInfo.GetVersionInfo(styleFile.FullName);
                    yield return new NativeThemeInfo("Aero", versionInfo, styleFile);
                }

                foreach (var styleFile in revDir.EnumerateFiles("aerolite.msstyles")) {
                    var versionInfo = FileVersionInfo.GetVersionInfo(styleFile.FullName);

                    yield return new NativeThemeInfo("Aero Lite", versionInfo, styleFile);
                }
            }
        }

        private static string GetExecutableDir()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            if (!string.IsNullOrEmpty(codeBase))
                return Path.GetDirectoryName(new Uri(codeBase).LocalPath);
            return null;
        }

        private IEnumerable<DirectoryInfo> EnumerateDataDirectories()
        {
            string exeDir = GetExecutableDir();
            if (exeDir == null)
                yield break;

            // Check in the directory of the executable.
            var dir = new DirectoryInfo(Path.Combine(exeDir, "Data"));
            if (dir.Exists)
                yield return dir;


            // Check in the development root directory.
            string buildDir = Path.GetDirectoryName(exeDir);
            string buildsDir = Path.GetDirectoryName(buildDir);
            string rootDir = Path.GetDirectoryName(buildsDir);
            if (rootDir != null && File.Exists(Path.Combine(rootDir, "Source", "PresentationTheme.Aero.sln"))) {
                dir = new DirectoryInfo(Path.Combine(rootDir, "Data"));
                if (dir.Exists)
                    yield return dir;
            }
        }
    }

    public class ThemeInfoPair
    {
        public ThemeInfoPair(string name, NativeThemeInfo nativeTheme, WpfThemeInfo wpfTheme)
        {
            Name = name;
            NativeTheme = nativeTheme;
            WpfTheme = wpfTheme;
        }

        public string Name { get; }
        public NativeThemeInfo NativeTheme { get; }
        public WpfThemeInfo WpfTheme { get; }
    }
}
