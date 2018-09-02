namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using PresentationTheme.Aero.Win10;
    using PresentationTheme.Aero.Win8;
    using PresentationTheme.AeroLite.Win10;
    using PresentationTheme.AeroLite.Win8;
    using PresentationTheme.HighContrast.Win10;
    using PresentationTheme.HighContrast.Win8;
    using ThemeCore.Native;

    public class ThemeInfoProvider
    {
        private readonly Lazy<UxColorScheme> highContrast1;
        private readonly Lazy<UxColorScheme> highContrastWhite;
        private readonly Lazy<UxColorScheme> highContrastDebugLight;
        private readonly Lazy<UxColorScheme> highContrastDebugDark;
        private readonly List<ThemeInfoPair> themes = new List<ThemeInfoPair>();
        private readonly List<WpfThemeInfo> wpfThemes = new List<WpfThemeInfo>();
        private readonly List<NativeThemeInfo> nativeThemes = new List<NativeThemeInfo>();

        public ThemeInfoProvider()
        {
            highContrast1 = new Lazy<UxColorScheme>(CreateHighContrast1Scheme);
            highContrastWhite = new Lazy<UxColorScheme>(CreateHighContrastWhiteScheme);
            highContrastDebugLight = new Lazy<UxColorScheme>(CreateHighContrastDebugLightScheme);
            highContrastDebugDark = new Lazy<UxColorScheme>(CreateHighContrastDebugDarkScheme);

            try {
                var aero7 = Assembly.Load("PresentationTheme.Aero.Win7");
                var themeType = aero7.GetType("PresentationTheme.Aero.Win7.AeroWin7Theme");
                var resourceUri = (Uri)themeType.GetProperty("ResourceUri").GetValue(null);
                wpfThemes.Add(new WpfThemeInfo("Aero (Windows 7)", resourceUri));
            } catch (Exception) {
            }

            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 8.1)", AeroWin8Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero Lite (Windows 8.1)", AeroLiteWin8Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast (Windows 8.1)", HighContrastWin8Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast #1 (Windows 8.1)", HighContrastWin8Theme.ResourceUri, highContrast1.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast White (Windows 8.1)", HighContrastWin8Theme.ResourceUri, highContrastWhite.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast Debug Light (Windows 8.1)", HighContrastWin8Theme.ResourceUri, highContrastDebugLight.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast Debug Dark (Windows 8.1)", HighContrastWin8Theme.ResourceUri, highContrastDebugDark.Value));
            wpfThemes.Add(new WpfThemeInfo("Aero (Windows 10)", AeroWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("Aero Lite (Windows 10)", AeroLiteWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast (Windows 10)", HighContrastWin10Theme.ResourceUri));
            wpfThemes.Add(new WpfThemeInfo("High Contrast #1 (Windows 10)", HighContrastWin10Theme.ResourceUri, highContrast1.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast White (Windows 10)", HighContrastWin10Theme.ResourceUri, highContrastWhite.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast Debug Light (Windows 10)", HighContrastWin10Theme.ResourceUri, highContrastDebugLight.Value));
            wpfThemes.Add(new WpfThemeInfo("High Contrast Debug Dark (Windows 10)", HighContrastWin10Theme.ResourceUri, highContrastDebugDark.Value));
            wpfThemes.Add(new WpfThemeInfo("Built-in Classic", BuiltinThemes.ClassicUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Aero", BuiltinThemes.AeroUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Aero 2", BuiltinThemes.Aero2Uri));
            wpfThemes.Add(new WpfThemeInfo("Built-in AeroLite", BuiltinThemes.AeroLiteUri));
            wpfThemes.Add(new WpfThemeInfo("Built-in Royale", BuiltinThemes.RoyaleUri));

            foreach (var nativeTheme in FindNativeThemes())
                nativeThemes.Add(nativeTheme);

            foreach (var nativeTheme in nativeThemes) {
                if (nativeTheme.Name == "Aero (Windows 10)") {
                    nativeThemes.Add(
                        new NativeThemeInfo(
                            "Aero Debug Dark ", nativeTheme.Version,
                            nativeTheme.Path, new UxThemeLoadParams {
                                IsHighContrast = false,
                                CustomColors = highContrastDebugDark.Value
                            }));
                    break;
                }
            }

            var latestNativeThemes = nativeThemes.GroupBy(x => x.Name).Select(x => x.Max());
            foreach (var nativeTheme in latestNativeThemes) {
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
            return FindAllNativeThemes().OrderBy(x => x);
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
                    foreach (var hcTheme in EnumerateHighContrastSchemes(versionInfo, styleFile))
                        yield return hcTheme;
                }
            }
        }

        private IEnumerable<NativeThemeInfo> EnumerateHighContrastSchemes(
            FileVersionInfo versionInfo, FileInfo styleFile)
        {
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

            yield return new NativeThemeInfo(
                "High Contrast White", versionInfo, styleFile,
                new UxThemeLoadParams {
                    IsHighContrast = true,
                    CustomColors = highContrastWhite.Value
                });

            yield return new NativeThemeInfo(
                "High Contrast Debug Light", versionInfo, styleFile,
                new UxThemeLoadParams {
                    IsHighContrast = true,
                    CustomColors = highContrastDebugLight.Value
                });

            yield return new NativeThemeInfo(
                "High Contrast Debug Dark", versionInfo, styleFile,
                new UxThemeLoadParams {
                    IsHighContrast = true,
                    CustomColors = highContrastDebugDark.Value
                });
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

        private UxColorScheme CreateHighContrastWhiteScheme()
        {
            var scheme = new UxColorScheme();
            scheme.ActiveTitle = RGB(0, 0, 0);
            scheme.Background = RGB(255, 255, 255);
            scheme.ButtonFace = RGB(255, 255, 255);
            scheme.ButtonText = RGB(0, 0, 0);
            scheme.GrayText = RGB(96, 0, 0);
            scheme.Hilight = RGB(55, 0, 110);
            scheme.HilightText = RGB(255, 255, 255);
            scheme.HotTrackingColor = RGB(0, 0, 159);
            scheme.InactiveTitle = RGB(255, 255, 255);
            scheme.InactiveTitleText = RGB(0, 0, 0);
            scheme.TitleText = RGB(255, 255, 255);
            scheme.Window = RGB(255, 255, 255);
            scheme.WindowText = RGB(0, 0, 0);
            scheme.Scrollbar = RGB(255, 255, 255);
            scheme.Menu = RGB(255, 255, 255);
            scheme.WindowFrame = RGB(0, 0, 0);
            scheme.MenuText = RGB(0, 0, 0);
            scheme.ActiveBorder = RGB(128, 128, 128);
            scheme.InactiveBorder = RGB(192, 192, 192);
            scheme.AppWorkspace = RGB(128, 128, 128);
            scheme.ButtonShadow = RGB(128, 128, 128);
            scheme.ButtonHilight = RGB(192, 192, 192);
            scheme.ButtonDkShadow = RGB(0, 0, 0);
            scheme.ButtonLight = RGB(192, 192, 192);
            scheme.InfoText = RGB(0, 0, 0);
            scheme.InfoWindow = RGB(255, 255, 255);
            scheme.ButtonAlternateFace = RGB(192, 192, 192);
            scheme.GradientActiveTitle = RGB(0, 0, 0);
            scheme.GradientInactiveTitle = RGB(255, 255, 255);
            scheme.MenuHilight = RGB(0, 0, 0);
            scheme.MenuBar = RGB(255, 255, 255);
            return scheme;
        }

        private UxColorScheme CreateHighContrastDebugLightScheme()
        {
            var scheme = new UxColorScheme();
            scheme.ActiveTitle = RGB(204, 112, 81);
            scheme.Background = RGB(127, 51, 51);
            scheme.ButtonFace = RGB(173, 204, 255);
            scheme.ButtonText = RGB(0, 81, 142);
            scheme.GrayText = RGB(255, 0, 255);
            scheme.Hilight = RGB(255, 255, 0);
            scheme.HilightText = RGB(142, 81, 204);
            scheme.HotTrackingColor = RGB(127, 0, 0);
            scheme.InactiveTitle = RGB(204, 173, 81);
            scheme.InactiveTitleText = RGB(51, 51, 0);
            scheme.TitleText = RGB(81, 0, 0);
            scheme.Window = RGB(204, 204, 204);
            scheme.WindowText = RGB(0, 0, 255);
            scheme.Scrollbar = RGB(51, 70, 127);
            scheme.Menu = RGB(255, 127, 204);
            scheme.WindowFrame = RGB(127, 51, 108);
            scheme.MenuText = RGB(127, 0, 81);
            scheme.ActiveBorder = RGB(127, 79, 51);
            scheme.InactiveBorder = RGB(188, 204, 81);
            scheme.AppWorkspace = RGB(117, 127, 51);
            scheme.ButtonShadow = RGB(0, 255, 255);
            scheme.ButtonHilight = RGB(51, 0, 188);
            scheme.ButtonDkShadow = RGB(255, 255, 0);
            scheme.ButtonLight = RGB(127, 127, 0);
            scheme.InfoText = RGB(81, 158, 204);
            scheme.InfoWindow = RGB(51, 98, 127);
            scheme.ButtonAlternateFace = RGB(96, 81, 204);
            scheme.GradientActiveTitle = RGB(60, 51, 127);
            scheme.GradientInactiveTitle = RGB(188, 81, 204);
            scheme.MenuHilight = RGB(117, 51, 127);
            scheme.MenuBar = RGB(204, 81, 127);
            return scheme;
        }

        private UxColorScheme CreateHighContrastDebugDarkScheme()
        {
            var scheme = new UxColorScheme();
            scheme.ActiveTitle = RGB(51, 143, 174);
            scheme.Background = RGB(128, 204, 204);
            scheme.ButtonFace = RGB(82, 51, 0);
            scheme.ButtonText = RGB(255, 174, 113);
            scheme.GrayText = RGB(0, 255, 0);
            scheme.Hilight = RGB(0, 0, 255);
            scheme.HilightText = RGB(113, 174, 51);
            scheme.HotTrackingColor = RGB(128, 255, 255);
            scheme.InactiveTitle = RGB(51, 82, 174);
            scheme.InactiveTitleText = RGB(204, 204, 255);
            scheme.TitleText = RGB(174, 255, 255);
            scheme.Window = RGB(51, 51, 51);
            scheme.WindowText = RGB(255, 255, 0);
            scheme.Scrollbar = RGB(204, 185, 128);
            scheme.Menu = RGB(0, 128, 51);
            scheme.WindowFrame = RGB(128, 204, 147);
            scheme.MenuText = RGB(128, 255, 174);
            scheme.ActiveBorder = RGB(128, 176, 204);
            scheme.InactiveBorder = RGB(67, 51, 174);
            scheme.AppWorkspace = RGB(138, 128, 204);
            scheme.ButtonShadow = RGB(255, 0, 0);
            scheme.ButtonHilight = RGB(204, 255, 67);
            scheme.ButtonDkShadow = RGB(0, 0, 255);
            scheme.ButtonLight = RGB(128, 128, 255);
            scheme.InfoText = RGB(174, 97, 51);
            scheme.InfoWindow = RGB(204, 157, 128);
            scheme.ButtonAlternateFace = RGB(159, 174, 51);
            scheme.GradientActiveTitle = RGB(195, 204, 128);
            scheme.GradientInactiveTitle = RGB(67, 174, 51);
            scheme.MenuHilight = RGB(138, 204, 128);
            scheme.MenuBar = RGB(51, 174, 128);
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
            string rootDir = FindDevelopmentRootDir(exeDir);
            if (rootDir != null && File.Exists(Path.Combine(rootDir, "Source", "PresentationTheme.Aero.sln"))) {
                dir = new DirectoryInfo(Path.Combine(rootDir, "Data"));
                if (dir.Exists)
                    yield return dir;
            }
        }

        private string FindDevelopmentRootDir(string currentDir)
        {
            while (currentDir != null) {
                string name = Path.GetFileName(currentDir);
                if (name == "PresentationTheme.Aero")
                    return currentDir;

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
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
