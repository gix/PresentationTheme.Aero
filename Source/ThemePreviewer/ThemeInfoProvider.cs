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
    using PresentationTheme.HighContrast.Win10;
    using StyleCore.Native;

    public class ThemeInfoProvider
    {
        private readonly Lazy<UxColorScheme> highContrast1;
        private readonly List<ThemeInfoPair> themes = new List<ThemeInfoPair>();
        private readonly List<WpfThemeInfo> wpfThemes = new List<WpfThemeInfo>();
        private readonly List<NativeThemeInfo> nativeThemes = new List<NativeThemeInfo>();

        public ThemeInfoProvider()
        {
            highContrast1 = new Lazy<UxColorScheme>(CreateHighContrast1Scheme);

            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 7)", AeroWin7Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 8)", AeroWin8Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 10)", AeroWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero Lite (Windows 10)", AeroLiteWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast (Windows 10)", HighContrastWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast #1 (Windows 10)", HighContrastWin10Theme.ResourceUri, highContrast1.Value));
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

                    yield return new NativeThemeInfo(
                        "High Contrast", versionInfo, styleFile,
                        new UxThemeLoadParams {
                            IsHighContrast = true
                        });

                    yield return new NativeThemeInfo(
                        "High Contrast #1", versionInfo, styleFile,
                        new UxThemeLoadParams {
                            IsHighContrast = true,
                            CustomColors = highContrast1.Value
                        });
                }
            }
        }

        private UxColorScheme CreateHighContrast1Scheme()
        {
            var scheme = new UxColorScheme();
            scheme.ActiveTitle = RGB(0x00, 0x00, 0xFF);
            scheme.Background = RGB(0x00, 0x00, 0x00);
            scheme.ButtonFace = RGB(0x00, 0x00, 0x00);
            scheme.ButtonText = RGB(0xFF, 0xFF, 0xFF);
            scheme.GrayText = RGB(0x00, 0xFF, 0x00);
            scheme.Hilight = RGB(0x00, 0x80, 0x00);
            scheme.HilightText = RGB(0xFF, 0xFF, 0xFF);
            scheme.HotTrackingColor = RGB(0x80, 0x80, 0xFF);
            scheme.InactiveTitle = RGB(0x00, 0xFF, 0xFF);
            scheme.InactiveTitleText = RGB(0x00, 0x00, 0x00);
            scheme.TitleText = RGB(0xFF, 0xFF, 0xFF);
            scheme.Window = RGB(0x00, 0x00, 0x00);
            scheme.WindowText = RGB(0xFF, 0xFF, 0x00);
            scheme.Scrollbar = RGB(0x00, 0x00, 0x00);
            scheme.Menu = RGB(0x00, 0x00, 0x00);
            scheme.WindowFrame = RGB(0xFF, 0xFF, 0xFF);
            scheme.MenuText = RGB(0xFF, 0xFF, 0xFF);
            scheme.ActiveBorder = RGB(0x00, 0x00, 0xFF);
            scheme.InactiveBorder = RGB(0x00, 0xFF, 0xFF);
            scheme.AppWorkspace = RGB(0x00, 0x00, 0x00);
            scheme.ButtonShadow = RGB(0x80, 0x80, 0x80);
            scheme.ButtonHilight = RGB(0xC0, 0xC0, 0xC0);
            scheme.ButtonDkShadow = RGB(0xFF, 0xFF, 0xFF);
            scheme.ButtonLight = RGB(0xFF, 0xFF, 0xFF);
            scheme.InfoText = RGB(0xFF, 0xFF, 0x00);
            scheme.InfoWindow = RGB(0x00, 0x00, 0x00);
            scheme.ButtonAlternateFace = RGB(0xC0, 0xC0, 0xC0);
            scheme.GradientActiveTitle = RGB(0x00, 0x00, 0xFF);
            scheme.GradientInactiveTitle = RGB(0x00, 0xFF, 0xFF);
            scheme.MenuHilight = RGB(0x00, 0x80, 0x00);
            scheme.MenuBar = RGB(0x00, 0x00, 0x00);
            return scheme;
        }

        private static uint RGB(int r, int g, int b)
        {
            return (byte)r | ((uint)(byte)g << 8) | ((uint)(byte)b << 16);
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
