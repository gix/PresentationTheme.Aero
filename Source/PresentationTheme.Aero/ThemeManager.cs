namespace PresentationTheme.Aero
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;
    using Expression = System.Linq.Expressions.Expression;

    /// <summary>
    ///   Allows overriding WPF theme resources.
    /// </summary>
    /// <remarks>
    ///   WPF does not provide a way to replace the theme resources for system
    ///   controls (i.e., those contained in the PresentationFramework assembly).
    ///   Providing styles with proper type-keys in the resource inheritance
    ///   tree or application-resources is not enough since those are skipped
    ///   when looking up implicit base styles (i.e., for styles without explicit
    ///   <see cref="Style.BasedOn"/>). This class uses reflection to manipulate
    ///   the internal dictionaries WPF uses to store theme resources.
    /// </remarks>
    public sealed class ThemeManager
    {
        private const int WM_THEMECHANGED = 0x31A;

        private static readonly Lazy<ThemeManager> Instance = new Lazy<ThemeManager>(
            () => new ThemeManager());

        private readonly Assembly PresentationFramework;

        private readonly Action<object> Raise_SystemResources_ThemedDictionaryLoaded;
        private readonly Func<bool> SystemResources_IsSystemResourcesParsing_Get;
        private readonly Action<bool> SystemResources_IsSystemResourcesParsing_Set;
        private readonly Func<object> SystemResources_ThemeDictionaryLock_Get;
        private readonly Func<Assembly, object> SystemResources_EnsureDictionarySlot;

        private readonly Func<object, Assembly> ResourceDictionaries__assembly_Get;
        private readonly Action<object, Assembly> ResourceDictionaries__assembly_Set;
        private readonly Func<object, string> ResourceDictionaries__assemblyName_Get;
        private readonly Action<object, string> ResourceDictionaries__assemblyName_Set;
        private readonly Func<object, bool> ResourceDictionaries__preventReEnter_Get;
        private readonly Action<object, bool> ResourceDictionaries__preventReEnter_Set;
        private readonly Func<object, bool> ResourceDictionaries__themedLoaded_Get;
        private readonly Action<object, bool> ResourceDictionaries__themedLoaded_Set;
        private readonly Func<object, ResourceDictionaryLocation> ResourceDictionaries__themedLocation_Get;
        private readonly Func<object, ResourceDictionary> ResourceDictionaries__themedDictionary_Get;
        private readonly Action<object, ResourceDictionary> ResourceDictionaries__themedDictionary_Set;
        private readonly Action<object, Assembly> ResourceDictionaries__themedDictionaryAssembly_Set;
        private readonly Action<object, Uri> ResourceDictionaries__themedDictionarySourceUri_Set;
        private readonly Func<string> ResourceDictionaries__themedResourceName_Get;
        private readonly Action<string> ResourceDictionaries__themedResourceName_Set;
        private readonly Func<object, object> ResourceDictionaries_ThemedDictionaryInfo_Get;
        private readonly Func<Assembly, object> ResourceDictionaries_Ctor;
        private delegate ResourceDictionary LoadDictionaryDelegate(
            object dictionaries, Assembly assembly, string assemblyName, string resourceName,
            bool isTraceEnabled, out Uri dictionarySourceUri);
        private readonly LoadDictionaryDelegate ResourceDictionaries_LoadDictionary;
        private readonly Func<object, bool, ResourceDictionary> ResourceDictionaries_LoadThemedDictionary;
        private readonly Func<object, bool, ResourceDictionary> ResourceDictionaries_LoadGenericDictionary;
        private readonly Action<object> ResourceDictionaries_ClearThemedDictionary;
        private readonly Action<object> ResourceDictionaries_LoadDictionaryLocations;

        private delegate object FetchResourceDelegate(
            ResourceDictionary dictionary, object resourceKey,
            bool allowDeferredResourceReference,
            bool mustReturnDeferredResourceReference, out bool canCache);
        private readonly FetchResourceDelegate ResourceDictionary_FetchResource;

        private static readonly Assembly MsCorLib = typeof(string).Assembly;
        private static readonly Assembly PresentationCore = typeof(UIElement).Assembly;
        private static readonly Assembly WindowsBase = typeof(DependencyObject).Assembly;
        private static Dictionary<Assembly, ResourceDictionariesProxy> dictionariesMap;

        private Utils.MemoryPatch findDictionaryResourceHook;
        private bool InHookedMode => findDictionaryResourceHook != null;

        private static bool IsSystemResourcesParsing
        {
            get => Instance.Value.SystemResources_IsSystemResourcesParsing_Get();
            set => Instance.Value.SystemResources_IsSystemResourcesParsing_Set(value);
        }

        private object ThemeDictionaryLock => SystemResources_ThemeDictionaryLock_Get();

        private readonly bool valid;
        private Delegate hwndWrapperHook;

        private class ThemePolicy : IThemePolicy
        {
            private readonly Func<Uri> themeUriProvider;

            public ThemePolicy(Func<Uri> themeUriProvider)
            {
                this.themeUriProvider = themeUriProvider;
            }

            public bool MergeWithBaseResources { get; set; } = true;

            public Uri GetCurrentThemeUri()
            {
                return themeUriProvider();
            }
        }

        private readonly Dictionary<Assembly, IThemePolicy> policies =
            new Dictionary<Assembly, IThemePolicy>();

        private ThemeManager()
        {
            const BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
            const BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

            PresentationFramework = typeof(ResourceDictionary).Assembly;
            var WindowsBase = typeof(Dispatcher).Assembly;

            var ResourceDictionaryInfo_Type = PresentationFramework.GetType(
                "System.Windows.Diagnostics.ResourceDictionaryInfo");
            var ResourceDictionaryLoadedEventArgs_Type = PresentationFramework.GetType(
                "System.Windows.Diagnostics.ResourceDictionaryLoadedEventArgs");
            var ResourceDictionaryLoadedEventArgs_Ctor = ResourceDictionaryInfo_Type != null ?
                ResourceDictionaryLoadedEventArgs_Type?.GetConstructor(
                    nonPublicInstance, null, new[] { ResourceDictionaryInfo_Type }, null) : null;

            var SystemResources_Type = PresentationFramework?.GetType(
                "System.Windows.SystemResources", true);
            var SystemResources__hwndNotify = SystemResources_Type?.GetField(
                "_hwndNotify", nonPublicStatic);
            var SystemResources_ThemedDictionaryLoaded = SystemResources_Type?.GetField(
                "ThemedDictionaryLoaded", nonPublicStatic);

            if (SystemResources_ThemedDictionaryLoaded != null &&
                ResourceDictionaryLoadedEventArgs_Ctor != null) {
                // void Raise(System.Windows.Diagnostics.ResourceDictionaryInfo themedDictionaryInfo)
                // {
                //    var handler = SystemResources.ThemedDictionaryLoaded;
                //    if (handler != null)
                //        handler(null, new ResourceDictionaryLoadedEventArgs(themedDictionaryInfo);
                //}
                var param = Expression.Parameter(typeof(object), "themedDictionaryInfo");
                var handler = Expression.Parameter(
                    SystemResources_ThemedDictionaryLoaded.FieldType, "handler");
                Raise_SystemResources_ThemedDictionaryLoaded =
                    Expression.Lambda<Action<object>>(
                        Expression.Block(
                            new[] { handler },
                            Expression.Assign(
                                handler,
                                Expression.Field(null, SystemResources_ThemedDictionaryLoaded)),
                            Expression.IfThen(
                                Expression.NotEqual(handler, Expression.Constant(null)),
                                Expression.Invoke(
                                    handler,
                                    Expression.Constant(null),
                                    Expression.New(
                                        ResourceDictionaryLoadedEventArgs_Ctor,
                                        Expression.Convert(
                                            param,
                                            ResourceDictionaryInfo_Type))))),
                        param).Compile();
            }

            SystemResources_Type?.GetProperty(
                "IsSystemResourcesParsing", nonPublicStatic)
                ?.CreateDelegate(
                    out SystemResources_IsSystemResourcesParsing_Get,
                    out SystemResources_IsSystemResourcesParsing_Set);
            SystemResources_Type?.GetProperty(
                "ThemeDictionaryLock", nonPublicStatic)
                ?.CreateDelegate(out SystemResources_ThemeDictionaryLock_Get);
            SystemResources_Type?.GetMethod(
                "EnsureDictionarySlot", nonPublicStatic, null, new[] { typeof(Assembly) }, null)
                ?.CreateDelegate(out SystemResources_EnsureDictionarySlot);
            var SystemResources_EnsureResourceChangeListener = SystemResources_Type?.GetMethod(
                "EnsureResourceChangeListener", nonPublicStatic, null, Type.EmptyTypes, null);

            var SecurityCriticalDataClass_Value = SystemResources__hwndNotify?.FieldType.GetProperty(
                "Value", nonPublicInstance);

            var HwndWrapperHook_Type = WindowsBase.GetType("MS.Win32.HwndWrapperHook", true);
            var HwndWrapper_Type = WindowsBase.GetType("MS.Win32.HwndWrapper", true);
            var HwndWrapper_AddHookLast = HwndWrapperHook_Type != null ? HwndWrapper_Type?.GetMethod(
                "AddHookLast", nonPublicInstance, null, new[] { HwndWrapperHook_Type }, null) : null;

            var ResourceDictionaries_Type = SystemResources_Type?.GetNestedType(
                "ResourceDictionaries", BindingFlags.NonPublic);
            ResourceDictionaries_Type?.GetField("_assembly", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__assembly_Get,
                    out ResourceDictionaries__assembly_Set);
            ResourceDictionaries_Type?.GetField("_assemblyName", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__assemblyName_Get,
                    out ResourceDictionaries__assemblyName_Set);
            ResourceDictionaries_Type?.GetField("_preventReEnter", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__preventReEnter_Get,
                    out ResourceDictionaries__preventReEnter_Set);
            ResourceDictionaries_Type?.GetField("_themedLoaded", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__themedLoaded_Get,
                    out ResourceDictionaries__themedLoaded_Set);
            ResourceDictionaries_Type?.GetField("_themedLocation", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__themedLocation_Get,
                    out var _);
            ResourceDictionaries_Type?.GetField("_themedDictionary", nonPublicInstance)
                ?.CreateDelegate(
                    out ResourceDictionaries__themedDictionary_Get,
                    out ResourceDictionaries__themedDictionary_Set);
            ResourceDictionaries_Type?.GetField("_themedDictionaryAssembly", nonPublicInstance)
                ?.CreateDelegate(
                    out var _,
                    out ResourceDictionaries__themedDictionaryAssembly_Set);
            ResourceDictionaries_Type?.GetField("_themedDictionarySourceUri", nonPublicInstance)
                ?.CreateDelegate(
                    out var _,
                    out ResourceDictionaries__themedDictionarySourceUri_Set);
            ResourceDictionaries_Type?.GetField("_themedResourceName", nonPublicStatic)
                ?.CreateDelegate(
                    out ResourceDictionaries__themedResourceName_Get,
                    out ResourceDictionaries__themedResourceName_Set);
            ResourceDictionaries_Type?.GetProperty("ThemedDictionaryInfo", nonPublicInstance)
                ?.CreateDelegate(out ResourceDictionaries_ThemedDictionaryInfo_Get);
            ResourceDictionaries_Type?.GetConstructor(
                nonPublicInstance, null, new[] { typeof(Assembly) }, null)
                ?.CreateDelegate(out ResourceDictionaries_Ctor);
            ResourceDictionaries_Type?.GetMethod(
                "LoadDictionary", nonPublicInstance, null,
                new[] {
                    typeof(Assembly), typeof(string), typeof(string),
                    typeof(bool), typeof(Uri).MakeByRefType()
                },
                null)
                ?.CreateDelegate(out ResourceDictionaries_LoadDictionary);
            ResourceDictionaries_Type?.GetMethod(
                    "LoadThemedDictionary", nonPublicInstance, null,
                    new[] { typeof(bool) }, null)
                ?.CreateDelegate(out ResourceDictionaries_LoadThemedDictionary);
            ResourceDictionaries_Type?.GetMethod(
                    "LoadGenericDictionary", nonPublicInstance, null,
                    new[] { typeof(bool) }, null)
                ?.CreateDelegate(out ResourceDictionaries_LoadGenericDictionary);
            ResourceDictionaries_Type?.GetMethod(
                    "ClearThemedDictionary", nonPublicInstance, null, Type.EmptyTypes, null)
                ?.CreateDelegate(out ResourceDictionaries_ClearThemedDictionary);
            ResourceDictionaries_Type?.GetMethod(
                    "LoadDictionaryLocations", nonPublicInstance, null, Type.EmptyTypes, null)
                ?.CreateDelegate(out ResourceDictionaries_LoadDictionaryLocations);

            var ResourceDictionary_Type = typeof(ResourceDictionary);
            ResourceDictionary_FetchResource = (FetchResourceDelegate)ResourceDictionary_Type.GetMethod(
                "FetchResource", nonPublicInstance, null,
                new[] {
                    typeof(object), typeof(bool), typeof(bool),
                    typeof(bool).MakeByRefType()
                },
                null)?.CreateDelegate(typeof(FetchResourceDelegate));

            valid =
                SystemResources__hwndNotify != null &&
                Raise_SystemResources_ThemedDictionaryLoaded != null &&
                SystemResources_IsSystemResourcesParsing_Get != null &&
                SystemResources_IsSystemResourcesParsing_Set != null &&
                SystemResources_ThemeDictionaryLock_Get != null &&
                SystemResources_EnsureDictionarySlot != null &&
                SystemResources_EnsureResourceChangeListener != null &&
                ResourceDictionaryLoadedEventArgs_Ctor != null &&
                SecurityCriticalDataClass_Value != null &&
                HwndWrapperHook_Type != null &&
                HwndWrapper_AddHookLast != null &&
                ResourceDictionaries__assembly_Get != null &&
                ResourceDictionaries__assembly_Set != null &&
                ResourceDictionaries__assemblyName_Get != null &&
                ResourceDictionaries__assemblyName_Set != null &&
                ResourceDictionaries__preventReEnter_Get != null &&
                ResourceDictionaries__preventReEnter_Set != null &&
                ResourceDictionaries__themedLoaded_Get != null &&
                ResourceDictionaries__themedLoaded_Set != null &&
                ResourceDictionaries__themedLocation_Get != null &&
                ResourceDictionaries__themedDictionary_Get != null &&
                ResourceDictionaries__themedDictionary_Set != null &&
                ResourceDictionaries__themedDictionaryAssembly_Set != null &&
                ResourceDictionaries__themedDictionarySourceUri_Set != null &&
                ResourceDictionaries__themedResourceName_Get != null &&
                ResourceDictionaries__themedResourceName_Set != null &&
                ResourceDictionaries_ThemedDictionaryInfo_Get != null &&
                ResourceDictionaries_Ctor != null &&
                ResourceDictionaries_LoadDictionary != null &&
                ResourceDictionaries_LoadThemedDictionary != null &&
                ResourceDictionaries_LoadGenericDictionary != null &&
                ResourceDictionaries_ClearThemedDictionary != null &&
                ResourceDictionaries_LoadDictionaryLocations != null &&
                ResourceDictionary_FetchResource != null;

            if (valid) {
                try {
                    hwndWrapperHook = Delegate.CreateDelegate(
                        HwndWrapperHook_Type, this, nameof(SystemThemeFilterMessage));

                    SystemResources_EnsureResourceChangeListener.Invoke(null, null);
                    var hwndNotify = SystemResources__hwndNotify.GetValue(null);
                    var hwndWrapper = SecurityCriticalDataClass_Value.GetValue(hwndNotify);
                    HwndWrapper_AddHookLast.Invoke(hwndWrapper, new object[] { hwndWrapperHook });
                } catch {
                    valid = false;
                    throw;
                }
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the <see cref="ThemeManager"/> can
        ///   manipulate internal WPF data structures. When <see lang="false"/>
        ///   <see cref="SetTheme(Assembly,IThemePolicy)"/> and similar functions
        ///   will always return <see langword="false"/>. This indicates that
        ///   internal framework types may have changed.
        /// </summary>
        public static bool IsOperational => Instance.Value.valid;

        /// <summary>
        ///   Installs the theme manager.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   The <see cref="ThemeManager"/> is not operational, i.e.
        ///   <see cref="IsOperational"/> is <see langword="false"/>.
        /// </exception>
        /// <remarks>
        ///   Hooks into the internal system resource loading machinery via splicing
        ///   (i.e., patching code at runtime). This enables advanced theme resource
        ///   resolution for all assemblies without proactively setting custom
        ///   policies.
        /// </remarks>
        /// <seealso cref="Uninstall"/>
        public static void Install()
        {
            var instance = Instance.Value;
            if (!instance.valid)
                throw new InvalidOperationException();

            lock (instance.ThemeDictionaryLock)
                instance.InstallHook();
        }

        /// <summary>
        ///   Uninstalls the theme manager.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   The <see cref="ThemeManager"/> is not operational, i.e.
        ///   <see cref="IsOperational"/> is <see langword="false"/>.
        /// </exception>
        /// <seealso cref="Install"/>
        public static void Uninstall()
        {
            var instance = Instance.Value;
            if (!instance.valid)
                throw new InvalidOperationException();

            lock (instance.ThemeDictionaryLock)
                instance.UninstallHook();
        }

        /// <summary>
        ///   Sets the theme resources for the PresentationFramework assembly.
        /// </summary>
        /// <param name="themeUri">
        ///   The pack <see cref="Uri"/> to the location of the theme resource
        ///   dictionary to use.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="themeUri"/> is <see langword="null"/>.
        /// </exception>
        public static bool SetPresentationFrameworkTheme(Uri themeUri)
        {
            if (themeUri == null)
                throw new ArgumentNullException(nameof(themeUri));

            var instance = Instance.Value;
            return instance.SetThemeInternal(
                instance.PresentationFramework, new ThemePolicy(() => themeUri));
        }

        /// <summary>
        ///   Sets the theme resources for the PresentationFramework assembly.
        /// </summary>
        /// <param name="themePolicy">The theme policy to use.</param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="themePolicy"/> is <see langword="null"/>.
        /// </exception>
        public static bool SetPresentationFrameworkTheme(IThemePolicy themePolicy)
        {
            if (themePolicy == null)
                throw new ArgumentNullException(nameof(themePolicy));

            var instance = Instance.Value;
            return instance.SetThemeInternal(instance.PresentationFramework, themePolicy);
        }

        /// <summary>
        ///   Sets the theme resources for the PresentationFramework assembly.
        /// </summary>
        /// <param name="themeUriProvider">
        ///   A delegate providing the pack <see cref="Uri"/> to the location of
        ///   the theme resource dictionary to use. The delegate will be invoked
        ///   again to get an updated resource URI in case the system theme has
        ///   changed. If the delegate returns <see langword="null"/> the default
        ///   theme resources are used.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="themeUriProvider"/> is <see langword="null"/>.
        /// </exception>
        public static bool SetPresentationFrameworkTheme(Func<Uri> themeUriProvider)
        {
            if (themeUriProvider == null)
                throw new ArgumentNullException(nameof(themeUriProvider));

            var instance = Instance.Value;
            return instance.SetThemeInternal(
                instance.PresentationFramework, new ThemePolicy(themeUriProvider));
        }

        /// <summary>
        ///   Clears the theme resources for the PresentationFramework assembly.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        ///   Unless explicitly overridden again (using
        ///   <see cref="SetTheme(Assembly,IThemePolicy)"/> or similar), the
        ///   next time a resource is looked up the default theme resources are
        ///   loaded again.
        /// </remarks>
        public static bool ClearPresentationFrameworkTheme()
        {
            var instance = Instance.Value;
            return instance.ClearThemeInternal(instance.PresentationFramework);
        }

        /// <summary>
        ///   Sets the theme resources for the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///   An <see cref="Assembly"/> containing WPF controls.
        /// </param>
        /// <param name="themePolicy">The theme policy to use.</param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> or <paramref name="themePolicy"/> are
        ///   <see langword="null"/>.
        /// </exception>
        public static bool SetTheme(Assembly assembly, IThemePolicy themePolicy)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (themePolicy == null)
                throw new ArgumentNullException(nameof(themePolicy));

            return Instance.Value.SetThemeInternal(assembly, themePolicy);
        }

        /// <summary>
        ///   Sets the theme resources for the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///   An <see cref="Assembly"/> containing WPF controls.
        /// </param>
        /// <param name="themeUriProvider">
        ///   A delegate providing the pack <see cref="Uri"/> to the location of
        ///   the theme resource dictionary to use. The delegate will be invoked
        ///   again to get an updated resource URI in case the system theme has
        ///   changed. If the delegate returns <see langword="null"/> the default
        ///   theme resources are used.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> or <paramref name="themeUriProvider"/>
        ///   are <see langword="null"/>.
        /// </exception>
        public static bool SetTheme(Assembly assembly, Func<Uri> themeUriProvider)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (themeUriProvider == null)
                throw new ArgumentNullException(nameof(themeUriProvider));

            return Instance.Value.SetThemeInternal(
                assembly, new ThemePolicy(themeUriProvider));
        }

        /// <summary>
        ///   Clears the theme resources for the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///   An <see cref="Assembly"/> containing WPF controls.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> on success; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        ///   Unless explicitly overridden again (using
        ///   <see cref="SetTheme(Assembly,IThemePolicy)"/> or similar), the
        ///   next time a resource is looked up the default theme resources are
        ///   loaded again.
        /// </remarks>
        public static bool ClearTheme(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return Instance.Value.ClearThemeInternal(assembly);
        }

        private bool SetThemeInternal(Assembly assembly, IThemePolicy policy)
        {
            if (!valid)
                return false;

            ClearThemeInternal(assembly);
            policies[assembly] = policy;

            if (!InHookedMode)
                LoadThemeResources(assembly);

            return true;
        }

        private bool ClearThemeInternal(Assembly assembly)
        {
            if (!valid)
                return false;

            policies.Remove(assembly);
            lock (ThemeDictionaryLock) {
                var resourceDictionaries = SystemResources_EnsureDictionarySlot(assembly);
                ResourceDictionaries_ClearThemedDictionary(resourceDictionaries);
            }

            return true;
        }

        private void LoadThemeResources(Assembly assembly)
        {
            lock (ThemeDictionaryLock) {
                bool isTraceEnabled = false;
                var resourceDictionaries = EnsureDictionarySlot(assembly);
                resourceDictionaries.LoadThemedDictionary(isTraceEnabled);
            }
        }

        private IntPtr SystemThemeFilterMessage(
            IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_THEMECHANGED)
                OnThemeChanged();
            return IntPtr.Zero;
        }

        private void OnThemeChanged()
        {
            UxThemeWrapper.OnThemeChanged();
            if (!InHookedMode) {
                foreach (var assembly in policies.Keys)
                    LoadThemeResources(assembly);
            }
        }

        private void InstallHook()
        {
            if (findDictionaryResourceHook != null)
                return;

            const BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

            var PresentationFramework = typeof(ResourceDictionary).Assembly;
            var SystemResources_Type = PresentationFramework.GetType("System.Windows.SystemResources", true);
            var paramTypes = new[] {
                typeof(object),
                typeof(Type),
                typeof(ResourceKey),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool).MakeByRefType()
            };
            var SystemResources_FindDictionaryResource = SystemResources_Type?.GetMethod(
                nameof(FindDictionaryResource), nonPublicStatic, null, paramTypes, null);
            var This_FindDictionaryResource = typeof(ThemeManager).GetMethod(
                nameof(FindDictionaryResource), nonPublicStatic, null, paramTypes, null);

            findDictionaryResourceHook = Utils.HookMethod(
                SystemResources_FindDictionaryResource,
                This_FindDictionaryResource);
        }

        private void UninstallHook()
        {
            if (findDictionaryResourceHook != null) {
                findDictionaryResourceHook.Dispose();
                findDictionaryResourceHook = null;
            }
        }

        private sealed class ResourcesCandidate
        {
            public ResourcesCandidate(
                Assembly assembly, string assemblyName, string resourceName, bool mergeBaseResources)
            {
                Assembly = assembly;
                AssemblyName = assemblyName;
                ResourceName = resourceName;
                MergeBaseResources = mergeBaseResources;
            }

            public Assembly Assembly { get; }
            public string AssemblyName { get; }
            public string ResourceName { get; }
            public bool MergeBaseResources { get; }
        }

        private IEnumerable<ResourcesCandidate> EnumerateDictionaries(
            bool external, Assembly assembly, string assemblyName,
            string themedResourceName, string fallbackThemedResourceName,
            bool ignorePolicies)
        {
            Assembly externalAssembly;
            string externalAssemblyName;

            if (!ignorePolicies && policies.TryGetValue(assembly, out var policy)) {
                var themeUri = policy.GetCurrentThemeUri();
                if (themeUri != null) {
                    PackUriUtils.GetAssemblyAndPartNameFromPackAppUri(
                        themeUri, out AssemblyName resourceAssemblyName, out string resourceName);

                    // ResourceDictionaries.LoadDictionary expects an extension-less
                    // resource path since it appends ".baml".
                    if (string.Equals(Path.GetExtension(resourceName), ".xaml", StringComparison.OrdinalIgnoreCase))
                        resourceName = Path.ChangeExtension(resourceName, null);

                    resourceName = resourceName.ToLowerInvariant();

                    externalAssembly = null;
                    try {
                        externalAssembly = Assembly.Load(resourceAssemblyName);
                    } catch (FileNotFoundException) {
                    } catch (BadImageFormatException) {
                    }

                    if (externalAssembly != null)
                        yield return new ResourcesCandidate(
                            externalAssembly, externalAssembly.GetName().Name,
                            resourceName, policy.MergeWithBaseResources);
                }
            }

            if (!external)
                yield return new ResourcesCandidate(assembly, assemblyName, themedResourceName, false);

            if (ResourceDictionariesProxy.LoadExternalAssembly(
                    assembly, assemblyName, ThemeResourcesKind.Default,
                    out externalAssembly, out externalAssemblyName))
                yield return new ResourcesCandidate(externalAssembly, externalAssemblyName, themedResourceName, false);

            if (!external)
                yield return new ResourcesCandidate(assembly, assemblyName, fallbackThemedResourceName, false);

            if (ResourceDictionariesProxy.LoadExternalAssembly(
                    assembly, assemblyName, ThemeResourcesKind.Fallback,
                    out externalAssembly, out externalAssemblyName))
                yield return new ResourcesCandidate(externalAssembly, externalAssemblyName, fallbackThemedResourceName, false);

            if (UxThemeWrapper.IsActive && ResourceDictionariesProxy.LoadExternalAssembly(
                    assembly, assemblyName, ThemeResourcesKind.Classic,
                    out externalAssembly, out externalAssemblyName))
                yield return new ResourcesCandidate(externalAssembly, externalAssemblyName, fallbackThemedResourceName, false);
        }

        private static ResourceDictionariesProxy EnsureDictionarySlot(Assembly assembly)
        {
            ResourceDictionariesProxy dictionaries = null;
            if (dictionariesMap != null)
                dictionariesMap.TryGetValue(assembly, out dictionaries);
            else
                dictionariesMap = new Dictionary<Assembly, ResourceDictionariesProxy>(1);

            if (dictionaries == null) {
                var resourceDictionaries = Instance.Value.SystemResources_EnsureDictionarySlot(assembly);
                dictionaries = new ResourceDictionariesProxy(resourceDictionaries);
                dictionariesMap.Add(assembly, dictionaries);
            }

            return dictionaries;
        }

        private sealed class ResourceDictionariesProxy
        {
            private readonly object resourceDictionaries;
            private bool loadBaseResourcesOnReEnter;

            private Assembly _assembly
            {
                get => Instance.Value.ResourceDictionaries__assembly_Get(resourceDictionaries);
            }

            private string _assemblyName
            {
                get => Instance.Value.ResourceDictionaries__assemblyName_Get(resourceDictionaries);
            }

            public bool _themedLoaded
            {
                get => Instance.Value.ResourceDictionaries__themedLoaded_Get(resourceDictionaries);
                set => Instance.Value.ResourceDictionaries__themedLoaded_Set(resourceDictionaries, value);
            }

            private ResourceDictionaryLocation _themedLocation
            {
                get => Instance.Value.ResourceDictionaries__themedLocation_Get(resourceDictionaries);
            }

            public ResourceDictionary _themedDictionary
            {
                get => Instance.Value.ResourceDictionaries__themedDictionary_Get(resourceDictionaries);
                set => Instance.Value.ResourceDictionaries__themedDictionary_Set(resourceDictionaries, value);
            }

            private Assembly _themedDictionaryAssembly
            {
                set => Instance.Value.ResourceDictionaries__themedDictionaryAssembly_Set(resourceDictionaries, value);
            }

            private Uri _themedDictionarySourceUri
            {
                set => Instance.Value.ResourceDictionaries__themedDictionarySourceUri_Set(resourceDictionaries, value);
            }

            private static string _themedResourceName
            {
                get => Instance.Value.ResourceDictionaries__themedResourceName_Get();
                set => Instance.Value.ResourceDictionaries__themedResourceName_Set(value);
            }

            private bool _preventReEnter
            {
                get => Instance.Value.ResourceDictionaries__preventReEnter_Get(resourceDictionaries);
                set => Instance.Value.ResourceDictionaries__preventReEnter_Set(resourceDictionaries, value);
            }

            public ResourceDictionariesProxy(object resourceDictionaries)
            {
                this.resourceDictionaries = resourceDictionaries;
            }

            private static string ThemedResourceName
            {
                get
                {
                    if (UxThemeWrapper.IsActive)
                        return "themes/" + UxThemeWrapper.ThemeName.ToLowerInvariant() + "." +
                               UxThemeWrapper.ThemeColor.ToLowerInvariant();
                    return "themes/classic";
                }
            }

            private static string FallbackThemedResourceName
            {
                get
                {
                    if (_themedResourceName == null) {
                        if (UxThemeWrapper.IsActive)
                            _themedResourceName = "themes/" + UxThemeWrapper.FallbackThemeName.ToLowerInvariant() + "." +
                                                  UxThemeWrapper.ThemeColor.ToLowerInvariant();
                        else
                            _themedResourceName = "themes/classic";
                    }
                    return _themedResourceName;
                }
            }

            private object ThemedDictionaryInfo
            {
                get => Instance.Value.ResourceDictionaries_ThemedDictionaryInfo_Get(resourceDictionaries);
            }

            public void ClearThemedDictionary()
            {
                Instance.Value.ResourceDictionaries_ClearThemedDictionary(resourceDictionaries);
            }

            public ResourceDictionary LoadGenericDictionary(bool isTraceEnabled)
            {
                return Instance.Value.ResourceDictionaries_LoadGenericDictionary(
                    resourceDictionaries, isTraceEnabled);
            }

            public ResourceDictionary LoadThemedDictionary(bool isTraceEnabled)
            {
                if (_themedLoaded)
                    return _themedDictionary;

                LoadDictionaryLocations();
                if (_themedLocation == ResourceDictionaryLocation.None)
                    return null;

                if (_preventReEnter) {
                    if (!loadBaseResourcesOnReEnter)
                        return null;
                    return LoadThemedDictionaryLocked(isTraceEnabled, true);
                }

                IsSystemResourcesParsing = true;
                _preventReEnter = true;
                try {
                    return LoadThemedDictionaryLocked(isTraceEnabled, false);
                } finally {
                    _preventReEnter = false;
                    IsSystemResourcesParsing = false;
                }
            }

            private ResourceDictionary LoadThemedDictionaryLocked(
                bool isTraceEnabled, bool ignorePolicies)
            {
                Assembly assembly = null;
                ResourceDictionary dictionary = null;
                Uri sourceUri = null;

                var candidates = Instance.Value.EnumerateDictionaries(
                    _themedLocation == ResourceDictionaryLocation.ExternalAssembly,
                    _assembly, _assemblyName, ThemedResourceName,
                    FallbackThemedResourceName, ignorePolicies);

                foreach (var candidate in candidates) {
                    assembly = candidate.Assembly;

                    ResourceDictionary baseResources = null;
                    if (candidate.MergeBaseResources) {
                        loadBaseResourcesOnReEnter = true;
                        try {
                            baseResources = LoadThemedDictionary(isTraceEnabled);
                        } finally {
                            loadBaseResourcesOnReEnter = false;
                        }
                    }

                    dictionary = LoadDictionary(
                        candidate.Assembly, candidate.AssemblyName,
                        candidate.ResourceName, isTraceEnabled, out sourceUri);

                    if (dictionary != null) {
                        if (candidate.MergeBaseResources && baseResources != null) {
                            var resources = new ResourceDictionary();
                            resources.MergedDictionaries.Add(baseResources);
                            resources.MergedDictionaries.Add(dictionary);
                            dictionary = resources;
                        }

                        break;
                    }
                }

                _themedDictionary = dictionary;
                _themedDictionarySourceUri = sourceUri;
                _themedDictionaryAssembly = assembly;
                _themedLoaded = true;

                if (_themedDictionary != null)
                    Instance.Value.Raise_SystemResources_ThemedDictionaryLoaded(ThemedDictionaryInfo);

                return _themedDictionary;
            }

            [SecurityCritical]
            private ResourceDictionary LoadDictionary(
                Assembly assembly, string assemblyName, string resourceName,
                bool isTraceEnabled, out Uri dictionarySourceUri)
            {
                return Instance.Value.ResourceDictionaries_LoadDictionary(
                    resourceDictionaries, assembly, assemblyName, resourceName,
                    isTraceEnabled, out dictionarySourceUri);
            }

            public static bool LoadExternalAssembly(
                Assembly mainAssembly, string assemblyBaseName,
                ThemeResourcesKind themeResourcesKind, out Assembly assembly,
                out string assemblyName)
            {
                var builder = new StringBuilder(assemblyBaseName.Length + 10);
                builder.Append(assemblyBaseName);
                builder.Append('.');
                switch (themeResourcesKind) {
                    case ThemeResourcesKind.Generic:
                        builder.Append("generic");
                        break;
                    case ThemeResourcesKind.Classic:
                        builder.Append("classic");
                        break;
                    case ThemeResourcesKind.Fallback:
                        builder.Append(UxThemeWrapper.FallbackThemeName);
                        break;
                    default:
                        builder.Append(UxThemeWrapper.ThemeName);
                        break;
                }

                assemblyName = builder.ToString();
                string fullName = GetFullAssemblyNameFromPartialName(
                    mainAssembly, assemblyName);

                // There is no Assembly.Exists API to determine if an Assembly
                // exists. There is also no way to determine if an Assembly's
                // format is good prior to loading it. So, the exception must be
                // caught. assembly will continue to be null and returned.
                assembly = null;
                try {
                    assembly = Assembly.Load(fullName);
                } catch (FileNotFoundException) {
                    return false;
                } catch (BadImageFormatException) {
                    return false;
                }

                if (assemblyBaseName == "PresentationFramework" && assembly != null) {
                    Type knownTypeHelper = assembly.GetType("Microsoft.Windows.Themes.KnownTypeHelper");
                    if (knownTypeHelper != null)
                        RunClassConstructor(knownTypeHelper);
                }

                return true;
            }

            [SecuritySafeCritical]
            private static void RunClassConstructor(Type t)
            {
                RuntimeHelpers.RunClassConstructor(t.TypeHandle);
            }

            private static string GetFullAssemblyNameFromPartialName(
                Assembly protoAssembly, string partialName)
            {
                var name = new AssemblyName(protoAssembly.FullName) {
                    Name = partialName
                };
                return name.FullName;
            }

            private void LoadDictionaryLocations()
            {
                Instance.Value.ResourceDictionaries_LoadDictionaryLocations(resourceDictionaries);
            }
        }

        private static class SafeNativeMethods
        {
            [SecurityCritical]
            [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern bool IsThemeActive();
        }

        private static class UnsafeNativeMethods
        {
            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
            public static extern int GetCurrentThemeName(
                StringBuilder pszThemeFileName, int dwMaxNameChars,
                StringBuilder pszColorBuff, int dwMaxColorChars,
                StringBuilder pszSizeBuff, int cchMaxSizeChars);
        }

        private static class UxThemeWrapper
        {
            private static string themeName;
            private static string themeColor;
            private static string fallbackThemeName;

            public static bool IsActive { get; private set; } =
                !SystemParameters.HighContrast && SafeNativeMethods.IsThemeActive();

            public static string ThemeName
            {
                get
                {
                    if (!IsActive)
                        return "classic";
                    if (themeName == null)
                        EnsureThemeName();
                    return themeName;
                }
            }

            public static string ThemeColor
            {
                get
                {
                    if (themeColor == null)
                        EnsureThemeName();
                    return themeColor;
                }
            }

            public static string FallbackThemeName
            {
                get
                {
                    if (!IsActive)
                        return "classic";
                    if (fallbackThemeName == null)
                        EnsureThemeName();
                    return fallbackThemeName;
                }
            }

            [SecurityCritical]
            private static void EnsureThemeName()
            {
                var themeFileName = new StringBuilder(260);
                var colorBuff = new StringBuilder(260);

                if (UnsafeNativeMethods.GetCurrentThemeName(
                        themeFileName, themeFileName.Capacity, colorBuff, colorBuff.Capacity, null, 0) != 0) {
                    themeName = themeColor = fallbackThemeName = string.Empty;
                    return;
                }

                themeName = themeFileName.ToString();
                themeName = Path.GetFileNameWithoutExtension(themeName);
                themeColor = colorBuff.ToString();
                fallbackThemeName = themeName;

                var win7 = new Version(6, 1);
                var win8 = new Version(6, 2);
                var win10 = new Version(10, 0);
                var osVersion = AeroThemePolicy.GetRealWindowsVersion();

                if (string.Equals(themeName, "Aero", StringComparison.OrdinalIgnoreCase)) {
                    if (osVersion >= win10)
                        themeName = "Aero.Win10";
                    else if (osVersion >= win8)
                        themeName = "Aero.Win8";
                    else if (osVersion >= win7)
                        themeName = "Aero.Win7";

                    if (osVersion >= win8)
                        fallbackThemeName = "Aero2";
                } else if (string.Equals(themeName, "AeroLite", StringComparison.OrdinalIgnoreCase)) {
                    if (SystemParameters.HighContrast) {
                        if (osVersion >= win10)
                            themeName = "HighContrast.Win10";
                        else if (osVersion >= win8)
                            themeName = "HighContrast.Win8";
                    } else {
                        if (osVersion >= win10)
                            themeName = "AeroLite.Win10";
                        else if (osVersion >= win8)
                            themeName = "AeroLite.Win8";
                    }
                }
            }

            internal static void OnThemeChanged()
            {
                IsActive = !SystemParameters.HighContrast && SafeNativeMethods.IsThemeActive();
                themeName = null;
                themeColor = null;
                fallbackThemeName = null;
            }
        }

        private static object FindDictionaryResource(
            object key, Type typeKey, ResourceKey resourceKey,
            bool isTraceEnabled, bool allowDeferredResourceReference,
            bool mustReturnDeferredResourceReference, out bool canCache)
        {
            canCache = true;

            object resource = null;
            Assembly assembly = typeKey != null ? typeKey.Assembly : resourceKey.Assembly;
            if (assembly == null || IgnoreAssembly(assembly))
                return null;

            ResourceDictionariesProxy dictionaries = EnsureDictionarySlot(assembly);
            ResourceDictionary dictionary = dictionaries.LoadThemedDictionary(isTraceEnabled);

            if (dictionary != null)
                resource = LookupResourceInDictionary(dictionary, key, allowDeferredResourceReference,
                    mustReturnDeferredResourceReference, out canCache);

            if (resource == null) {
                dictionary = dictionaries.LoadGenericDictionary(isTraceEnabled);
                if (dictionary != null)
                    resource = LookupResourceInDictionary(dictionary, key, allowDeferredResourceReference,
                        mustReturnDeferredResourceReference, out canCache);
            }

            if (resource != null)
                Freeze(resource);

            return resource;
        }

        private static object LookupResourceInDictionary(
            ResourceDictionary dictionary, object key, bool allowDeferredResourceReference,
            bool mustReturnDeferredResourceReference, out bool canCache)
        {
            IsSystemResourcesParsing = true;
            try {
                return Instance.Value.ResourceDictionary_FetchResource(
                    dictionary, key, allowDeferredResourceReference,
                    mustReturnDeferredResourceReference, out canCache);
            } finally {
                IsSystemResourcesParsing = false;
            }
        }

        private static void Freeze(object resource)
        {
            if (resource is Freezable freezable && !freezable.IsFrozen)
                freezable.Freeze();
        }

        private static bool IgnoreAssembly(Assembly assembly)
        {
            return
                assembly == MsCorLib ||
                assembly == PresentationCore ||
                assembly == WindowsBase;
        }

        private enum ThemeResourcesKind
        {
            Default,
            Classic,
            Generic,
            Fallback
        }
    }

    internal static class Utils
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public short wProcessorArchitecture;
            public short wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }

        [DllImport("kernel32", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32")]
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern UIntPtr VirtualQuery(
            IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer,
            IntPtr dwLength);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool VirtualProtect(
            IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect,
            out uint lpflOldProtect);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool FlushInstructionCache(
            IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        private static UIntPtr VirtualQuery(
            IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer)
        {
            return VirtualQuery(
                lpAddress, out lpBuffer,
                (IntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
        }

        private const int PAGE_EXECUTE_READWRITE = 0x40;

        private const int PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private const int PROCESSOR_ARCHITECTURE_ARM = 5;
        private const int PROCESSOR_ARCHITECTURE_IA64 = 6;
        private const int PROCESSOR_ARCHITECTURE_INTEL = 0;

        private static ProcessorArchitecture GetProcessorArchitecture()
        {
            var si = new SYSTEM_INFO();
            GetNativeSystemInfo(ref si);
            switch (si.wProcessorArchitecture) {
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return ProcessorArchitecture.Amd64;

                case PROCESSOR_ARCHITECTURE_IA64:
                    return ProcessorArchitecture.IA64;

                case PROCESSOR_ARCHITECTURE_INTEL:
                    return ProcessorArchitecture.X86;

                case PROCESSOR_ARCHITECTURE_ARM:
                    return ProcessorArchitecture.Arm;

                default:
                    return ProcessorArchitecture.None;
            }
        }

        public static MemoryPatch HookMethod(MethodInfo source, MethodInfo target)
        {
            if (!EqualSignatures(source, target))
                throw new ArgumentException("The method signatures are not the same.", nameof(source));

            RuntimeHelpers.PrepareMethod(source.MethodHandle);
            RuntimeHelpers.PrepareMethod(target.MethodHandle);
            IntPtr srcAddr = source.MethodHandle.GetFunctionPointer();
            IntPtr tgtAddr = target.MethodHandle.GetFunctionPointer();

            var arch = GetProcessorArchitecture();

            long offset;
            byte[] jumpInst;
            switch (arch) {
                case ProcessorArchitecture.Amd64:
                    offset = tgtAddr.ToInt64() - srcAddr.ToInt64() - 5;
                    if (offset >= Int32.MinValue && offset <= Int32.MaxValue) {
                        jumpInst = new byte[] {
                            0xE9, // JMP rel32
                            (byte)(offset & 0xFF),
                            (byte)((offset >> 8) & 0xFF),
                            (byte)((offset >> 16) & 0xFF),
                            (byte)((offset >> 24) & 0xFF)
                        };
                    } else {
                        offset = tgtAddr.ToInt64();
                        jumpInst = new byte[] {
                            0x48, 0xB8, // MOV moffs64,rax
                            (byte)(offset & 0xFF),
                            (byte)((offset >> 8) & 0xFF),
                            (byte)((offset >> 16) & 0xFF),
                            (byte)((offset >> 24) & 0xFF),
                            (byte)((offset >> 32) & 0xFF),
                            (byte)((offset >> 40) & 0xFF),
                            (byte)((offset >> 48) & 0xFF),
                            (byte)((offset >> 56) & 0xFF),
                            0xFF, 0xE0 // JMP rax
                        };
                    }
                    break;

                case ProcessorArchitecture.X86:
                    offset = tgtAddr.ToInt32() - srcAddr.ToInt32() - 5;
                    jumpInst = new byte[] {
                        0xE9, // JMP rel32
                        (byte)(offset & 0xFF),
                        (byte)((offset >> 8) & 0xFF),
                        (byte)((offset >> 16) & 0xFF),
                        (byte)((offset >> 24) & 0xFF)
                    };
                    break;

                default:
                    throw new NotSupportedException(
                        $"Processor architecture {arch} is not supported.");
            }

            return PatchMemory(srcAddr, jumpInst);
        }

        public sealed class MemoryPatch : IDisposable
        {
            private readonly IntPtr address;
            private readonly byte[] backupInstructions;

            public MemoryPatch(IntPtr address, byte[] backupInstructions)
            {
                this.address = address;
                this.backupInstructions = backupInstructions;
            }

            public void Dispose()
            {
                WriteMemory(address, backupInstructions);
            }
        }

        private static MemoryPatch PatchMemory(IntPtr address, byte[] instructions)
        {
            var backup = new byte[instructions.Length];
            Marshal.Copy(address, backup, 0, backup.Length);
            if (backup.Any(x => x == 0xCC))
                throw new InvalidOperationException(
                    "Refusing to patch memory due to breakpoints/INT3 in target memory.");

            WriteMemory(address, instructions);
            return new MemoryPatch(address, backup);
        }

        private static void WriteMemory(IntPtr address, byte[] instructions)
        {
            if (VirtualQuery(address, out MEMORY_BASIC_INFORMATION mbi) == UIntPtr.Zero)
                throw new Win32Exception();

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            } finally {
                if (!VirtualProtect(mbi.BaseAddress, mbi.RegionSize, PAGE_EXECUTE_READWRITE, out uint oldProtect))
                    throw new Win32Exception();
                Marshal.Copy(instructions, 0, address, instructions.Length);
                FlushInstructionCache(GetCurrentProcess(), address, (UIntPtr)instructions.Length);
                VirtualProtect(mbi.BaseAddress, mbi.RegionSize, oldProtect, out oldProtect);
            }
        }

        private static bool EqualSignatures(MethodInfo x, MethodInfo y)
        {
            if (x.CallingConvention != y.CallingConvention)
                return false;

            if (x.ReturnType != y.ReturnType)
                return false;

            ParameterInfo[] paramsX = x.GetParameters();
            ParameterInfo[] paramsY = y.GetParameters();
            if (paramsX.Length != paramsY.Length)
                return false;

            for (int i = 0; i < paramsX.Length; ++i) {
                if (paramsX[i].ParameterType != paramsY[i].ParameterType)
                    return false;
            }

            return true;
        }

        public static void CreateDelegate<T>(this ConstructorInfo ctor, out T lambda)
        {
            var parameters = typeof(T).GetMethod("Invoke").GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();
            lambda = Expression.Lambda<T>(
                Expression.New(ctor, parameters), parameters).Compile();
        }

        public static void CreateDelegate<T>(this MethodInfo method, out T lambda)
        {
            var parameters = typeof(T).GetMethod("Invoke").GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            if (method.IsStatic) {
                lambda = Expression.Lambda<T>(
                    Expression.Call(method, parameters), parameters).Compile();
            } else {
                Expression instance = parameters[0];
                if (instance.Type != method.DeclaringType)
                    instance = Expression.Convert(instance, method.DeclaringType);

                lambda = Expression.Lambda<T>(
                    Expression.Call(instance, method, parameters.Skip(1)), parameters).Compile();
            }
        }

        public static void CreateDelegate<T>(
            this PropertyInfo property, out T getter)
        {
            getter = CreateGetter<T>(property);
        }

        public static void CreateDelegate<T1, T2>(
            this PropertyInfo property, out T1 getter, out T2 setter)
        {
            getter = CreateGetter<T1>(property);
            setter = CreateSetter<T2>(property);
        }

        public static void CreateDelegate<T, TValue>(
            this FieldInfo field, out Func<T, TValue> getter, out Action<T, TValue> setter)
        {
            var instExpr = Expression.Parameter(typeof(T));
            var valueExpr = Expression.Parameter(field.FieldType);

            Expression fieldExpr;
            if (field.DeclaringType != typeof(T)) {
                if (typeof(T) != typeof(object))
                    throw new InvalidOperationException();

                fieldExpr = Expression.Field(
                    Expression.Convert(instExpr, field.DeclaringType), field);
            } else {
                fieldExpr = Expression.Field(instExpr, field);
            }

            getter = Expression.Lambda<Func<T, TValue>>(fieldExpr, instExpr).Compile();
            setter = Expression.Lambda<Action<T, TValue>>(
                Expression.Assign(fieldExpr, valueExpr), instExpr, valueExpr).Compile();
        }

        public static void CreateDelegate<TValue>(
            this FieldInfo field, out Func<TValue> getter, out Action<TValue> setter)
        {
            if (!field.IsStatic)
                throw new InvalidOperationException();

            var valueExpr = Expression.Parameter(field.FieldType);
            var fieldExpr = Expression.Field(null, field);

            getter = Expression.Lambda<Func<TValue>>(fieldExpr).Compile();
            setter = Expression.Lambda<Action<TValue>>(
                Expression.Assign(fieldExpr, valueExpr), valueExpr).Compile();
        }


        private static T CreateGetter<T>(this PropertyInfo property)
        {
            var getMethod = property.GetMethod;
            if (getMethod == null)
                throw new ArgumentException("Property has no getter", nameof(property));

            var lambdaInvoke = typeof(T).GetMethod("Invoke");
            var parameters = lambdaInvoke.GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            var signatureMatches = lambdaInvoke.ReturnType == property.PropertyType;
            if (getMethod.IsStatic)
                signatureMatches &= parameters.Length == 1 &&
                                    property.DeclaringType == parameters[0].Type;
            else
                signatureMatches &= parameters.Length == 0;

            if (signatureMatches)
                return (T)(object)getMethod.CreateDelegate(typeof(T));

            if (getMethod.IsStatic)
                return Expression.Lambda<T>(
                    Expression.Property(null, property), parameters).Compile();

            Expression instance = parameters[0];
            if (instance.Type != property.DeclaringType)
                instance = Expression.Convert(instance, property.DeclaringType);

            return Expression.Lambda<T>(
                Expression.Property(instance, property), parameters).Compile();
        }

        private static T CreateSetter<T>(this PropertyInfo property)
        {
            var setMethod = property.SetMethod;
            if (setMethod == null)
                throw new ArgumentException("Property has no setter", nameof(property));

            var lambdaInvoke = typeof(T).GetMethod("Invoke");
            var parameters = lambdaInvoke.GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            var signatureMatches = lambdaInvoke.ReturnType == property.PropertyType;
            if (property.GetMethod.IsStatic)
                signatureMatches &= parameters.Length == 1 &&
                                    property.DeclaringType == parameters[0].Type;
            else
                signatureMatches &= parameters.Length == 0;

            if (signatureMatches)
                return (T)(object)setMethod.CreateDelegate(typeof(T));

            if (setMethod.IsStatic)
                return Expression.Lambda<T>(
                    Expression.Call(setMethod, parameters), parameters).Compile();

            Expression instance = parameters[0];
            if (instance.Type != property.DeclaringType)
                instance = Expression.Convert(instance, property.DeclaringType);

            return Expression.Lambda<T>(
                Expression.Call(instance, setMethod, parameters), parameters).Compile();
        }
    }
}
