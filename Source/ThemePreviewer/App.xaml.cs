namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using PresentationTheme.Aero;

    public partial class App
    {
        private MainWindow window;
        private MainWindowViewModel viewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var opts = ParseArgs(e.Args);
            var themeInfoProvider = new ThemeInfoProvider();

            WpfThemeInfo wpfTheme;
            NativeThemeInfo nativeTheme;
            SelectThemes(opts, themeInfoProvider, out wpfTheme, out nativeTheme);

            if (wpfTheme != null) {
                var uri = wpfTheme.ResourceUri;
                if (!ThemeHelper.SetPresentationFrameworkTheme(uri))
                    MessageBox.Show($"Failed to load {uri}.");
            }

            window = new MainWindow();
            MainWindow = window;

            viewModel = new MainWindowViewModel(themeInfoProvider);
            viewModel.CurrentWpfTheme = wpfTheme;
            window.DataContext = viewModel;

            if (opts.WindowBounds != null) {
                var bounds = opts.WindowBounds.Value;
                window.Left = bounds.Left;
                window.Top = bounds.Top;
                window.Width = bounds.Width;
                window.Height = bounds.Height;
                window.WindowStartupLocation = WindowStartupLocation.Manual;
            } else {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (opts.TabIndex != null && opts.TabIndex >= 0 && opts.TabIndex < viewModel.Pages.Count)
                viewModel.CurrentPage = viewModel.Pages[opts.TabIndex.Value];

            window.Show();

            if (nativeTheme != null)
                viewModel.OverrideNativeTheme(nativeTheme).Forget();
        }

        private void SelectThemes(
            Options opts, ThemeInfoProvider themeInfoProvider,
            out WpfThemeInfo wpfTheme, out NativeThemeInfo nativeTheme)
        {
            wpfTheme = null;
            nativeTheme = null;

            if (opts.Theme != null) {
                string name = ExpandShortThemeName(opts.Theme);
                var theme = themeInfoProvider.Themes.FirstOrDefault(x => x.Name == name);
                if (theme != null) {
                    wpfTheme = theme.WpfTheme;
                    nativeTheme = theme.NativeTheme;
                }
            }

            if (opts.WpfTheme != null) {
                string name = ExpandShortThemeName(opts.WpfTheme);
                wpfTheme = themeInfoProvider.WpfThemes.FirstOrDefault(x => x.Name == name);
            }
            if (wpfTheme == null && opts.WpfThemeResourceUri != null) {
                wpfTheme = themeInfoProvider.WpfThemes.FirstOrDefault(
                    x => x.ResourceUri == opts.WpfThemeResourceUri);

                if (wpfTheme == null) {
                    wpfTheme = new WpfThemeInfo(opts.WpfThemeResourceUri.ToString(), opts.WpfThemeResourceUri);
                }
            }

            if (opts.NativeTheme != null) {
                string name = ExpandShortThemeName(opts.NativeTheme);
                nativeTheme = themeInfoProvider.NativeThemes.FirstOrDefault(x => x.Name == name);
            }
            if (nativeTheme == null && opts.NativeThemeFile != null) {
                nativeTheme = themeInfoProvider.NativeThemes.FirstOrDefault(x => string.Equals(x.Path.FullName,
                    opts.NativeThemeFile.FullName, StringComparison.OrdinalIgnoreCase));

                if (nativeTheme == null) {
                    nativeTheme = NativeThemeInfo.FromPath(
                        opts.NativeThemeFile.FullName,
                        new UxThemeLoadParams { IsHighContrast = opts.IsHighContrast });
                }
            }
        }

        private static string ExpandShortThemeName(string abbreviation)
        {
            switch (abbreviation) {
                case "aero6": return "Aero (Windows Vista)";
                case "aero7": return "Aero (Windows 7)";
                case "aero8": return "Aero (Windows 8)";
                case "aero10": return "Aero (Windows 10)";
                case "aerolite6": return "Aero (Windows Vista)";
                case "aerolite7": return "Aero Lite (Windows 7)";
                case "aerolite8": return "Aero Lite (Windows 8)";
                case "aerolite10": return "Aero Lite (Windows 10)";
                case "hc": return "High Contrast (Windows 10)";
                case "hc1": return "High Contrast #1 (Windows 10)";
                case "hcdebuglight": return "High Contrast Debug Light (Windows 10)";
                case "hcdebugdark": return "High Contrast Debug Dark (Windows 10)";
                case "hcwhite": return "High Contrast White (Windows 10)";
                default: return null;
            }
        }

        public void RestartWithDifferentTheme(WpfThemeInfo wpfThemeInfo)
        {
            if (wpfThemeInfo.ResourceUri != null)
                Restart($"-theme:{wpfThemeInfo.ResourceUri}");
            else
                Restart();
        }

        private class Options
        {
            public Rect? WindowBounds { get; set; }
            public int? TabIndex { get; set; }

            public string Theme { get; set; }
            public string WpfTheme { get; set; }
            public string NativeTheme { get; set; }
            public Uri WpfThemeResourceUri { get; set; }
            public FileInfo NativeThemeFile { get; set; }
            public bool IsHighContrast { get; set; }
        }

        private Options ParseArgs(string[] args)
        {
            var opts = new Options();
            foreach (var arg in args) {
                if (TryGetArg(arg, "-theme:", out string theme)) {
                    opts.Theme = theme;
                } else if (TryGetArg(arg, "-wpftheme:", UriKind.Absolute, out Uri uri)) {
                    opts.WpfThemeResourceUri = uri;
                } else if (TryGetArg(arg, "-wpftheme:", out string wpfTheme)) {
                    opts.WpfTheme = wpfTheme;
                } else if (TryGetArg(arg, "-nativetheme:", out string value)) {
                    var nativeTheme = new FileInfo(value);
                    if (nativeTheme.Exists)
                        opts.NativeThemeFile = nativeTheme;
                    else
                        opts.NativeTheme = value;
                } else if (TryGetArg(arg, "-pos:", out Rect bounds)) {
                    opts.WindowBounds = bounds;
                } else if (TryGetArg(arg, "-tab:", out int tab)) {
                    opts.TabIndex = tab;
                } else if (arg == "-hc") {
                    opts.IsHighContrast = true;
                }
            }

            return opts;
        }

        private static bool TryParseRect(string value, out Rect rect)
        {
            var converter = new RectConverter();
            try {
                rect = (Rect)converter.ConvertFromInvariantString(value);
                return true;
            } catch {
                rect = new Rect();
                return false;
            }
        }

        private static bool TryGetArg(string arg, string prefix, out string value)
        {
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                value = arg.Substring(prefix.Length);
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryGetArg(string arg, string prefix, out Rect value)
        {
            value = new Rect();
            return
                TryGetArg(arg, prefix, out string stringValue) &&
                TryParseRect(stringValue, out value);
        }

        private static bool TryGetArg(string arg, string prefix, out int value)
        {
            value = 0;
            return
                TryGetArg(arg, prefix, out string stringValue) &&
                int.TryParse(stringValue, out value);
        }

        private static bool TryGetArg(string arg, string prefix, UriKind uriKind, out Uri value)
        {
            value = null;
            return
                TryGetArg(arg, prefix, out string stringValue) &&
                Uri.TryCreate(stringValue, uriKind, out value);
        }

        public void Restart(params string[] args)
        {
            var bounds = new Rect(window.Left, window.Top, window.Width, window.Height).Round();

            var allArgs = new List<string>();
            allArgs.Add($"-pos:{new RectConverter().ConvertToInvariantString(bounds)}");

            viewModel.AppendCommandLineArgs(allArgs);
            allArgs.AddRange(args);

            var exePath = Assembly.GetExecutingAssembly().Location;
            var process = Process.Start(exePath, ProcessUtils.GetCommandLineArgs(allArgs));
            if (process.WaitForMainWindow(2000))
                Thread.Sleep(100);
            Shutdown();
        }
    }
}
