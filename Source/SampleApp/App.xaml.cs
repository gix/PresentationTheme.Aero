namespace SampleApp
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Diagnostics;
    using PresentationTheme.Aero;

    public partial class App
    {
        private FieldInfo SystemResources_ThemedDictionaryLoaded;
        private FieldInfo SystemResources_ThemedDictionaryUnloaded;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            const BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

            var PresentationFramework = typeof(ResourceDictionary).Assembly;

            var SystemResources_Type = PresentationFramework.GetType("System.Windows.SystemResources", true);
            SystemResources_ThemedDictionaryLoaded = SystemResources_Type?.GetField(
                "ThemedDictionaryLoaded", nonPublicStatic);
            SystemResources_ThemedDictionaryUnloaded = SystemResources_Type?.GetField(
                "ThemedDictionaryUnloaded", nonPublicStatic);

            ThemedDictionaryLoaded += OnThemedDictionaryLoaded;
            AeroTheme.SetAsCurrentTheme();
        }

        public event EventHandler<ResourceDictionaryLoadedEventArgs> ThemedDictionaryLoaded
        {
            add
            {
                var handler = (EventHandler<ResourceDictionaryLoadedEventArgs>)SystemResources_ThemedDictionaryLoaded.GetValue(null);
                handler = (EventHandler<ResourceDictionaryLoadedEventArgs>)Delegate.Combine(handler, value);
                SystemResources_ThemedDictionaryLoaded.SetValue(null, handler);
            }
            remove
            {
                var handler = (EventHandler<ResourceDictionaryLoadedEventArgs>)SystemResources_ThemedDictionaryLoaded.GetValue(null);
                handler = (EventHandler<ResourceDictionaryLoadedEventArgs>)Delegate.Remove(handler, value);
                SystemResources_ThemedDictionaryLoaded.SetValue(null, handler);
            }
        }

        public ObservableCollection<ResourceDictionaryInfo> ThemedDictionaryLoads { get; } =
            new ObservableCollection<ResourceDictionaryInfo>();

        private void OnThemedDictionaryLoaded(object sender, ResourceDictionaryLoadedEventArgs args)
        {
            ThemedDictionaryLoads.Add(args.ResourceDictionaryInfo);
        }
    }
}
