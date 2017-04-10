namespace ThemePreviewer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Xml;
    using PresentationTheme.Aero.Win10;
    using StyleCore.Native;

    public partial class App
    {
        private readonly HashSet<FrameworkElement> trackedElements =
            new HashSet<FrameworkElement>();

        private MainWindow window;
        private UxThemeOverride uxThemeOverride;

        public new static App Current => (App)Application.Current;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var opts = ParseArgs(e.Args);
            if (opts.ThemeResourceUri == null)
                opts.ThemeResourceUri = AeroWin10Theme.ResourceUri;

            ThemeHelper.SetPresentationFrameworkTheme(opts.ThemeResourceUri);

            window = new MainWindow();
            window.CurrentTheme = window.Themes.FirstOrDefault(x => x.ResourceUri == opts.ThemeResourceUri);
            window.ThemeChanged += OnThemeChanged;

            if (opts.WindowBounds != null) {
                var bounds = opts.WindowBounds.Value;
                window.Left = bounds.X;
                window.Top = bounds.Y;
                window.Width = bounds.Width;
                window.Height = bounds.Height;
            }

            if (opts.TabIndex != null && opts.TabIndex >= 0 && opts.TabIndex < window.Pages.Count)
                window.CurrentPage = window.Pages[opts.TabIndex.Value];

            MainWindow = window;
            window.Show();

            uxThemeOverride = new UxThemeOverride();

            OverrideNativeTheme(
                @"C:\Users\nrieck\dev\PresentationTheme.Aero\Data\10.0.14393.0\aerolite.msstyles")
                .Forget();
        }

        public async Task<bool> OverrideNativeTheme(string path)
        {
            MainWindow.IsEnabled = false;
            try {
                if (path != null) {
                    var theme = await Task.Run(() => UxThemeOverride.LoadTheme(path));
                    uxThemeOverride.SetTheme(theme);
                } else {
                    uxThemeOverride.SetTheme(SafeThemeFileHandle.Zero);
                }

                ThemeUtils.SendThemeChangedProcessLocal();
                return true;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return false;
            } finally {
                MainWindow.IsEnabled = true;
            }
        }

        private void OnThemeChanged(object sender, EventArgs args)
        {
            ReloadTheme(window.CurrentTheme);
        }

        public void RestartWithDifferentTheme(Theme theme)
        {
            if (theme.ResourceUri != null)
                Restart($"-theme:{theme.ResourceUri}");
            else
                Restart();
        }

        private void ReloadTheme(Theme theme)
        {
            var resources = theme.CreateResources();
            var refresher = new StyleReloader(Application.Current, resources, trackedElements);
            refresher.Run();
        }

        public static object LoadComponentFromAssembly(Uri uri)
        {
            return LoadComponent(PackUriHelper.GetPartUri(uri));
        }

        private class Options
        {
            public Uri ThemeResourceUri { get; set; }
            public Rect? WindowBounds { get; set; }
            public int? TabIndex { get; set; }
        }

        private Options ParseArgs(string[] args)
        {
            var opts = new Options();
            foreach (var arg in args) {
                string value;
                if (TryGetArg(arg, "-theme:", out value)) {
                    Uri uri;
                    if (Uri.TryCreate(value, UriKind.Absolute, out uri))
                        opts.ThemeResourceUri = uri;
                } else if (TryGetArg(arg, "-pos:", out value)) {
                    Rect bounds;
                    if (TryParseRect(value, out bounds))
                        opts.WindowBounds = bounds;
                } else if (TryGetArg(arg, "-tab:", out value)) {
                    int tab;
                    if (int.TryParse(value, out tab))
                        opts.TabIndex = tab;
                }
            }

            return opts;
        }

        private bool TryParseRect(string value, out Rect rect)
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

        private bool TryGetArg(string arg, string prefix, out string value)
        {
            if (arg.StartsWith(prefix)) {
                value = arg.Substring(prefix.Length);
                return true;
            }

            value = null;
            return false;
        }

        public void Restart(params string[] args)
        {
            var bounds = new Rect(
                Math.Round(window.Left), Math.Round(window.Top),
                Math.Round(window.Width), Math.Round(window.Height));
            var tabIndex = window.Pages.IndexOf(window.CurrentPage);

            var allArgs = new List<string>();
            allArgs.Add($"-pos:{new RectConverter().ConvertToInvariantString(bounds)}");
            if (tabIndex != -1)
                allArgs.Add($"-tab:{tabIndex}");
            allArgs.AddRange(args);

            var exePath = Assembly.GetExecutingAssembly().Location;
            var process = Process.Start(exePath, ProcessUtils.GetCommandLineArgs(allArgs));
            if (process.WaitForMainWindow(2000))
                Thread.Sleep(100);
            Shutdown();
        }

        public void TrackElement(FrameworkElement element)
        {
            trackedElements.Add(element);
        }

        public void UntrackElement(FrameworkElement element)
        {
            trackedElements.Remove(element);
        }
    }

    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
        }
    }

    public class StyleReloader
    {
        private readonly Application app;
        private readonly ResourceDictionary newResources;
        private readonly IEnumerable<FrameworkElement> trackedElements;

        private readonly Dictionary<Style, object> basedOnStyleToKeyMap =
            new Dictionary<Style, object>();

        private readonly HashSet<FrameworkElement> visited =
            new HashSet<FrameworkElement>();

        public StyleReloader(
            Application app, ResourceDictionary newResources,
            IEnumerable<FrameworkElement> trackedElements)
        {
            this.app = app;
            this.newResources = newResources;
            this.trackedElements = trackedElements;
        }

        public void Run()
        {
            CollectStyleKeys();
            visited.Clear();

            app.Resources.MergedDictionaries.Clear();
            if (newResources != null)
                app.Resources.MergedDictionaries.Add(newResources);

            RecreateStyles();
            visited.Clear();
        }

        private IEnumerable<DictionaryEntry> EnumerateStyles()
        {
            //foreach (var entry in EnumerateThemeStyles())
            //    yield return entry;

            foreach (var entry in EnumerateStyles(app.Resources))
                yield return entry;

            foreach (Window window in app.Windows)
                foreach (var entry in EnumerateStyles(window))
                    yield return entry;

            foreach (var element in trackedElements) {
                foreach (var entry in EnumerateStyles(element.Resources))
                    yield return entry;
            }
        }

        private IEnumerable<DictionaryEntry> EnumerateThemeStyles()
        {
            var presentationFramework = Assembly.GetAssembly(typeof(SystemFonts));
            var systemResources = presentationFramework.GetType("System.Windows.SystemResources");
            var dictionariesField = systemResources.GetField(
                "_dictionaries", BindingFlags.Static | BindingFlags.NonPublic);
            var resourceDictionaries = systemResources.GetNestedType("ResourceDictionaries", BindingFlags.NonPublic);
            var themedDictionaryField = resourceDictionaries.GetField(
                "_themedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

            var dictionaries = (IDictionary)dictionariesField.GetValue(null);
            if (dictionaries != null) {
                foreach (object themedDict in dictionaries.Values) {
                    var resources = (ResourceDictionary)themedDictionaryField.GetValue(themedDict);
                    foreach (var entry in EnumerateStyles(resources))
                        yield return entry;
                }
            }
        }

        private IEnumerable<DictionaryEntry> EnumerateStyles(FrameworkElement fe)
        {
            if (!visited.Add(fe))
                yield break;

            foreach (var entry in EnumerateStyles(fe.Resources))
                yield return entry;

            int children = VisualTreeHelper.GetChildrenCount(fe);
            for (int i = 0; i < children; ++i) {
                var child = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                if (child != null)
                    foreach (var entry in EnumerateStyles(child))
                        yield return entry;
            }
        }

        private IEnumerable<DictionaryEntry> EnumerateStyles(ResourceDictionary resources)
        {
            if (resources == null || resources == newResources)
                yield break;

            foreach (DictionaryEntry entry in resources) {
                var style = entry.Value as Style;
                if (style != null)
                    yield return entry;
            }

            foreach (var mergedDictionary in resources.MergedDictionaries) {
                foreach (var entry in EnumerateStyles(mergedDictionary))
                    yield return entry;
            }
        }

        private void CollectStyleKeys()
        {
            var styleToKeyMap = new Dictionary<Style, object>();
            foreach (var entry in EnumerateStyles())
                styleToKeyMap[(Style)entry.Value] = entry.Key;

            foreach (var entry in EnumerateStyles()) {
                var style = (Style)entry.Value;
                if (style.BasedOn != null) {
                    if (styleToKeyMap.ContainsKey(style.BasedOn))
                        basedOnStyleToKeyMap[style] = styleToKeyMap[style.BasedOn];
                }
            }
        }

        private void RecreateStyles()
        {
            RecreateStyles(k => (Style)app.FindResource(k), app.Resources);
            foreach (Window window in app.Windows)
                RecreateStyles(window);

            foreach (var element in trackedElements)
                RecreateStyles(element);
        }

        private void RecreateStyles(FrameworkElement fe)
        {
            if (!visited.Add(fe))
                return;

            if (!fe.IsLoaded) {
                fe.Loaded += OnElementLoaded;
                return;
            }

            RecreateStyles(k => (Style)fe.FindResource(k), fe.Resources);

            int children = VisualTreeHelper.GetChildrenCount(fe);
            for (int i = 0; i < children; ++i) {
                var child = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                if (child != null)
                    RecreateStyles(child);
            }
        }

        private void OnElementLoaded(object sender, RoutedEventArgs args)
        {
            var fe = (FrameworkElement)sender;
            fe.Loaded -= OnElementLoaded;
            RecreateStyles(fe);
        }

        private void RecreateStyles(Func<object, Style> findStyle, ResourceDictionary resources)
        {
            if (resources == null || resources == newResources)
                return;

            foreach (var key in new ArrayList(resources.Keys)) {
                var style = resources[key] as Style;
                if (style?.BasedOn == null)
                    continue;

                resources.Remove(key);
                resources.Add(key, Clone(findStyle, style));
            }

            foreach (var mergedDictionary in resources.MergedDictionaries)
                RecreateStyles(findStyle, mergedDictionary);
        }

        private Style Clone(Func<object, Style> findStyle, Style source)
        {
            if (!basedOnStyleToKeyMap.ContainsKey(source))
                return source;

            var styleKey = basedOnStyleToKeyMap[source];
            var basedOn = findStyle(styleKey);
            return Clone(source, basedOn);
        }

        private Style Clone(Style source, Style basedOn)
        {
            var clone = new Style(source.TargetType, basedOn);

            foreach (DictionaryEntry resource in source.Resources)
                clone.Resources.Add(resource.Key, resource.Value);

            foreach (var setter in source.Setters)
                clone.Setters.Add(CloneGeneric(setter));

            foreach (var trigger in source.Triggers)
                clone.Triggers.Add(CloneGeneric(trigger));

            return clone;
        }

        private T CloneGeneric<T>(T obj)
        {
            var buffer = new MemoryStream();

            var writer = XmlWriter.Create(buffer, new XmlWriterSettings {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });

            var xdsMgr = new XamlDesignerSerializationManager(writer);
            xdsMgr.XamlWriterMode = XamlWriterMode.Expression;
            XamlWriter.Save(obj, xdsMgr);

            buffer.Position = 0;
            var reader = XmlReader.Create(buffer);
            return (T)XamlReader.Load(reader);
        }
    }
}
