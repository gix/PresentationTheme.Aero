namespace NetCoreApp
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Diagnostics;
    using PresentationTheme.Aero;

    public partial class App
    {
        public bool UseThemeManager { get; set; }
        public bool UseAeroTheme { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UseThemeManager = !e.Args.Contains("-tm:0");
            UseAeroTheme = !e.Args.Contains("-aero:0");

            var isEnableProperty = typeof(ResourceDictionaryDiagnostics).GetProperty(
                                       "IsEnabled", BindingFlags.NonPublic | BindingFlags.Static)
                                   ?? throw new InvalidOperationException("ResourceDictionaryDiagnostics.IsEnabled missing");

            isEnableProperty.SetValue(null, true);
            ResourceDictionaryDiagnostics.ThemedResourceDictionaryLoaded += OnThemedDictionaryLoaded;

            if (UseThemeManager)
                ThemeManager.Install();
            if (UseAeroTheme)
                AeroTheme.SetAsCurrentTheme();
        }

        public ObservableCollection<ResourceDictionaryLoad> ThemedDictionaryLoads { get; } =
            new ObservableCollection<ResourceDictionaryLoad>();

        private void OnThemedDictionaryLoaded(object sender, ResourceDictionaryLoadedEventArgs args)
        {
            ThemedDictionaryLoads.Add(new ResourceDictionaryLoad(args.ResourceDictionaryInfo));
        }

        public void Restart()
        {
            string exePath = GetExePath();

            var process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.ArgumentList.Add($"-tm:{(UseThemeManager ? 1 : 0)}");
            process.StartInfo.ArgumentList.Add($"-aero:{(UseAeroTheme ? 1 : 0)}");
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.Start();
            Shutdown();
        }

        private static string GetExePath()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            if (Path.GetExtension(exePath) == ".dll")
                exePath = Path.Combine(Path.GetDirectoryName(exePath), Path.ChangeExtension(exePath, ".exe"));
            return exePath;
        }
    }

    public class AssemblyInfo
    {
        public AssemblyInfo(Assembly assembly)
        {
            Name = assembly.GetName();
            Location = assembly.Location;
            FileName = Path.GetFileName(assembly.Location);
        }

        public AssemblyName Name { get; }
        public string FileName { get; }
        public string Location { get; }

        public override string ToString()
        {
            return $"{Name.Name} ({Name.Version}, {FileName})";
        }
    }

    public class ResourceDictionaryLoad
    {
        public ResourceDictionaryLoad(ResourceDictionaryInfo info)
        {
            Time = DateTime.Now;
            Assembly = new AssemblyInfo(info.Assembly);
            ResourceDictionaryAssembly = new AssemblyInfo(info.ResourceDictionaryAssembly);
            SourceUri = info.SourceUri;
        }

        public DateTime Time { get; }
        public AssemblyInfo Assembly { get; }
        public AssemblyInfo ResourceDictionaryAssembly { get; }
        public Uri SourceUri { get; }
    }
}
