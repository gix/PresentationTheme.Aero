namespace PresentationTheme.Aero
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Packaging;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;

    public sealed class ThemeHelper
    {
        private const int WM_THEMECHANGED = 0x31A;

        // Custom theme resources have to be re-inserted when a WM_THEMECHANGED
        // message is received. This requires a strong reference.
        private static readonly Lazy<ThemeHelper> Instance = new Lazy<ThemeHelper>(
            () => new ThemeHelper());

        private readonly Assembly PresentationFramework;

        private readonly FieldInfo SystemResources__hwndNotify;
        private readonly PropertyInfo SystemResources_ThemeDictionaryLock;
        private readonly MethodInfo SystemResources_EnsureResourceChangeListener;
        private readonly MethodInfo SystemResources_EnsureDictionarySlot;
        private readonly MethodInfo SystemResources_InvalidateResources;

        private readonly PropertyInfo SecurityCriticalDataClass_Value;
        private readonly Type HwndWrapperHook_Type;
        private readonly MethodInfo HwndWrapper_AddHookLast;

        private readonly FieldInfo ResourceDictionaries__preventReEnter;
        private readonly FieldInfo ResourceDictionaries__themedDictionary;
        private readonly FieldInfo ResourceDictionaries__themedLoaded;
        private readonly ConstructorInfo ResourceDictionaries_Ctor;
        private readonly MethodInfo ResourceDictionaries_LoadDictionary;
        private readonly MethodInfo ResourceDictionaries_LoadThemedDictionary;
        private readonly MethodInfo ResourceDictionaries_ClearThemedDictionary;

        private readonly PropertyInfo ResourceDictionary_IsThemeDictionary;

        private readonly bool valid;
        private Delegate hwndWrapperHook;

        private readonly Dictionary<Assembly, Uri> themeResources =
            new Dictionary<Assembly, Uri>();

        public ThemeHelper()
        {
            const BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
            const BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

            PresentationFramework = typeof(ResourceDictionary).Assembly;
            var WindowsBase = typeof(Dispatcher).Assembly;

            var SystemResources_Type = PresentationFramework?.GetType("System.Windows.SystemResources", true);
            SystemResources__hwndNotify = SystemResources_Type?.GetField(
                "_hwndNotify", nonPublicStatic);
            SystemResources_ThemeDictionaryLock = SystemResources_Type?.GetProperty(
                "ThemeDictionaryLock", nonPublicStatic);
            SystemResources_EnsureDictionarySlot = SystemResources_Type?.GetMethod(
                "EnsureDictionarySlot", nonPublicStatic, null, new[] { typeof(Assembly) }, null);
            SystemResources_EnsureResourceChangeListener = SystemResources_Type?.GetMethod(
                "EnsureResourceChangeListener", nonPublicStatic, null, Type.EmptyTypes, null);
            SystemResources_InvalidateResources = SystemResources_Type?.GetMethod(
                "InvalidateResources", nonPublicStatic, null, new[] { typeof(bool) }, null);

            SecurityCriticalDataClass_Value = SystemResources__hwndNotify?.FieldType.GetProperty(
                "Value", nonPublicInstance);

            HwndWrapperHook_Type = WindowsBase.GetType("MS.Win32.HwndWrapperHook", true);
            var HwndWrapper_Type = WindowsBase.GetType("MS.Win32.HwndWrapper", true);
            HwndWrapper_AddHookLast = HwndWrapper_Type?.GetMethod(
                "AddHookLast", nonPublicInstance, null, new[] { HwndWrapperHook_Type }, null);

            var ResourceDictionaries_Type = SystemResources_Type?.GetNestedType(
                        "ResourceDictionaries", BindingFlags.NonPublic);
            ResourceDictionaries__themedDictionary = ResourceDictionaries_Type?.GetField(
                "_themedDictionary", nonPublicInstance);
            ResourceDictionaries__themedLoaded = ResourceDictionaries_Type?.GetField(
                "_themedLoaded", nonPublicInstance);
            ResourceDictionaries__preventReEnter = ResourceDictionaries_Type?.GetField(
                "_preventReEnter", nonPublicInstance);
            ResourceDictionaries_Ctor = ResourceDictionaries_Type?.GetConstructor(
                nonPublicInstance, null, new[] { typeof(Assembly) }, null);
            ResourceDictionaries_LoadDictionary = ResourceDictionaries_Type?.GetMethod(
                "LoadDictionary", nonPublicInstance, null,
                new[] {
                    typeof(Assembly), typeof(string), typeof(string),
                    typeof(bool), typeof(Uri).MakeByRefType()
                },
                null);
            ResourceDictionaries_LoadThemedDictionary = ResourceDictionaries_Type?.GetMethod(
                "LoadThemedDictionary", nonPublicInstance, null,
                new[] { typeof(bool) }, null);
            ResourceDictionaries_ClearThemedDictionary = ResourceDictionaries_Type?.GetMethod(
                "ClearThemedDictionary", nonPublicInstance, null, Type.EmptyTypes, null);

            ResourceDictionary_IsThemeDictionary = typeof(ResourceDictionary).GetProperty(
                "IsThemeDictionary", nonPublicInstance);

            valid =
                SystemResources__hwndNotify != null &&
                SystemResources_ThemeDictionaryLock != null &&
                SystemResources_EnsureDictionarySlot != null &&
                SystemResources_EnsureResourceChangeListener != null &&
                SystemResources_InvalidateResources != null &&
                SecurityCriticalDataClass_Value != null &&
                HwndWrapperHook_Type != null &&
                HwndWrapper_AddHookLast != null &&
                ResourceDictionaries__preventReEnter != null &&
                ResourceDictionaries__themedDictionary != null &&
                ResourceDictionaries__themedLoaded != null &&
                ResourceDictionaries_Ctor != null &&
                ResourceDictionaries_LoadDictionary != null &&
                ResourceDictionaries_LoadThemedDictionary != null &&
                ResourceDictionaries_ClearThemedDictionary != null &&
                ResourceDictionary_IsThemeDictionary != null;

            if (valid)
                HookThemeChangedMessage();
        }

        private void HookThemeChangedMessage()
        {
            hwndWrapperHook = Delegate.CreateDelegate(
                HwndWrapperHook_Type, this, nameof(SystemThemeFilterMessage));

            SystemResources_EnsureResourceChangeListener.Invoke(null, null);
            var hwndNotify = SystemResources__hwndNotify.GetValue(null);
            var hwndWrapper = SecurityCriticalDataClass_Value.GetValue(hwndNotify);
            HwndWrapper_AddHookLast.Invoke(hwndWrapper, new object[] { hwndWrapperHook });
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
            foreach (var entry in themeResources)
                SetThemeResources(entry.Key, entry.Value);
        }

        public static bool SetPresentationFrameworkTheme(Uri themeUri)
        {
            var instance = Instance.Value;
            return instance.SetThemeInternal(instance.PresentationFramework, themeUri);
        }

        public static bool ClearPresentationFrameworkTheme()
        {
            var instance = Instance.Value;
            return instance.ClearThemeInternal(instance.PresentationFramework);
        }

        public static bool SetTheme(Assembly assembly, Uri themeUri)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (themeUri == null)
                throw new ArgumentNullException(nameof(themeUri));

            return Instance.Value.SetThemeInternal(assembly, themeUri);
        }

        public static bool ClearTheme(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return Instance.Value.ClearThemeInternal(assembly);
        }

        private bool SetThemeInternal(Assembly assembly, Uri themeUri)
        {
            if (!valid)
                return false;

            themeResources.Add(assembly, themeUri);
            return SetThemeResources(assembly, themeUri);
        }

        private bool ClearThemeInternal(Assembly assembly)
        {
            if (!valid)
                return false;

            themeResources.Remove(assembly);
            lock (SystemResources_ThemeDictionaryLock.GetValue(null)) {
                var resourceDictionaries = SystemResources_EnsureDictionarySlot.Invoke(
                    null, new object[] { assembly });
                ResourceDictionaries_ClearThemedDictionary.Invoke(resourceDictionaries, null);
            }

            return true;
        }

        private bool SetThemeResources(Assembly assembly, Uri themeUri)
        {
            Assembly resourceAssembly;
            string resourceName;
            GetAssemblyAndPartNameFromPackAppUri(themeUri, out resourceAssembly, out resourceName);

            return SetThemeResources(assembly, resourceAssembly, resourceName);
        }

        private bool SetThemeResources(Assembly assembly, Assembly resourceAssembly, string resourceName)
        {
            if (resourceName == null)
                throw new ArgumentNullException(nameof(resourceName));

            if (Path.GetExtension(resourceName).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                resourceName = Path.ChangeExtension(resourceName, null);

            resourceName = resourceName.ToLowerInvariant();

            var overrideResources = LoadThemedDictionary(resourceAssembly, resourceName);
            if (overrideResources == null)
                return false;

            return SetThemeResources(assembly, overrideResources);
        }

        private bool SetThemeResources(Assembly assembly, ResourceDictionary overrideResources)
        {
            lock (SystemResources_ThemeDictionaryLock.GetValue(null)) {
                var resourceDictionaries = SystemResources_EnsureDictionarySlot.Invoke(null, new object[] { assembly });
                var defaultResources = (ResourceDictionary)ResourceDictionaries_LoadThemedDictionary.Invoke(
                    resourceDictionaries, new object[] { false });

                ResourceDictionary resources;
                if (defaultResources != null) {
                    resources = new ResourceDictionary();
                    resources.MergedDictionaries.Add(defaultResources);
                    resources.MergedDictionaries.Add(overrideResources);
                    ResourceDictionary_IsThemeDictionary.SetValue(resources, true);
                } else {
                    resources = overrideResources;
                }

                ResourceDictionaries__themedDictionary.SetValue(resourceDictionaries, resources);
            }

            return true;
        }

        private ResourceDictionary LoadThemedDictionary(Assembly resourceAssembly, string resourceName)
        {
            var resourceDictionaries = ResourceDictionaries_Ctor.Invoke(new object[] { resourceAssembly });
            Uri themedDictionarySourceUri;
            return LoadThemedDictionary(
                resourceDictionaries, resourceAssembly, resourceAssembly.GetName().Name, resourceName,
                false, out themedDictionarySourceUri);
        }

        private ResourceDictionary LoadThemedDictionary(
            object resourceDictionaries, Assembly assembly, string assemblyName, string resourceName,
            bool isTraceEnabled, out Uri dictionarySourceUri)
        {
            ResourceDictionaries__preventReEnter.SetValue(resourceDictionaries, true);
            try {
                var parms = new object[] { assembly, assemblyName, resourceName, isTraceEnabled, null };
                var result = (ResourceDictionary)ResourceDictionaries_LoadDictionary.Invoke(
                    resourceDictionaries,
                    parms);
                dictionarySourceUri = (Uri)parms[4];

                ResourceDictionaries__themedDictionary.SetValue(resourceDictionaries, result);
                ResourceDictionaries__themedLoaded.SetValue(resourceDictionaries, true);

                return result;
            } finally {
                ResourceDictionaries__preventReEnter.SetValue(resourceDictionaries, false);
            }
        }

        private const string PackageApplicationBaseUriEscaped = "application:///";

        private static bool IsPackApplicationUri(Uri uri)
        {
            return
                uri != null &&

                // Is the "outer" URI absolute?
                uri.IsAbsoluteUri &&

                // Does the "outer" URI have the pack: scheme?
                string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) &&

                // Does the "inner" URI have the application: scheme
                string.Equals(
                    PackUriHelper.GetPackageUri(uri).GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped),
                    PackageApplicationBaseUriEscaped,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static void GetAssemblyAndPartNameFromPackAppUri(
            Uri uri, out Assembly assembly, out string partName)
        {
            if (!IsPackApplicationUri(uri))
                throw new ArgumentException("Invalid pack application uri.", nameof(uri));

            // Generate a relative Uri which gets rid of the pack://application:,,, authority part.
            var partUri = new Uri(uri.AbsolutePath, UriKind.Relative);

            AssemblyName assemblyName;
            GetAssemblyNameAndPart(partUri, out assemblyName, out partName);

            if (assemblyName == null) {
                // The uri doesn't contain ";component". it should map to the enty application assembly.
                assembly = Assembly.GetEntryAssembly();

                // The partName returned from GetAssemblyNameAndPart should be escaped.
                Debug.Assert(string.Equals(partName, uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped), StringComparison.OrdinalIgnoreCase));
            } else {
                assembly = Assembly.Load(assemblyName);
            }
        }

        private const string WrongFirstSegmentMessage =
            "The required pattern for URI containing \";component\" is \"AssemblyName;Vxxxx;PublicKey;component\", where Vxxxx is the assembly version and PublicKey is the 16-character string representing the assembly public key token. Vxxxx and PublicKey are optional.";
        private const string ComponentSuffix = ";component";
        private const string VersionPrefix = "v";
        private const char ComponentDelimiter = ';';

        private static void GetAssemblyNameAndPart(
            Uri uri, out AssemblyName asmName, out string partName)
        {
            Debug.Assert(uri != null && !uri.IsAbsoluteUri, "This method accepts relative uri only.");

            string original = uri.ToString(); // only relative Uri here (enforced by Package)

            // Start and end points for the first segment in the Uri.
            int start = 0;
            int end;

            if (original[0] == '/')
                start = 1;

            asmName = null;
            partName = original.Substring(start);

            end = original.IndexOf('/', start);

            string firstSegment = string.Empty;
            bool hasComponent = false;

            if (end > 0) {
                firstSegment = original.Substring(start, end - start);

                if (firstSegment.EndsWith(ComponentSuffix, StringComparison.OrdinalIgnoreCase)) {
                    partName = original.Substring(end + 1);
                    hasComponent = true;
                }
            }

            if (!hasComponent)
                return;

            string[] assemblyInfo = firstSegment.Split(ComponentDelimiter);

            int count = assemblyInfo.Length;
            if (count < 2 || count > 4)
                throw new UriFormatException(WrongFirstSegmentMessage);

            // If the uri contains escaping character,
            // Convert it back to normal unicode string
            // so that the string as assembly name can be
            // recognized by Assembly.Load later.
            string assemblyName = Uri.UnescapeDataString(assemblyInfo[0]);
            string assemblyVersion = string.Empty;
            string assemblyKey = string.Empty;

            for (int i = 1; i < count - 1; ++i) {
                if (assemblyInfo[i].StartsWith(VersionPrefix, StringComparison.OrdinalIgnoreCase)) {
                    if (!string.IsNullOrEmpty(assemblyVersion))
                        throw new UriFormatException(WrongFirstSegmentMessage);
                    assemblyVersion = assemblyInfo[i].Substring(1); // Get rid of the leading "v"
                } else {
                    if (!string.IsNullOrEmpty(assemblyKey))
                        throw new UriFormatException(WrongFirstSegmentMessage);
                    assemblyKey = assemblyInfo[i];
                }
            }

            if (!string.IsNullOrEmpty(assemblyName)) {
                asmName = new AssemblyName(assemblyName);

                // We always use the primary assembly (culture neutral) for resource
                // manager. If the required resource lives in a satellite assembly,
                // ResourceManager can find the right satellite assembly later.
                asmName.CultureInfo = new CultureInfo(string.Empty);

                if (!string.IsNullOrEmpty(assemblyVersion))
                    asmName.Version = new Version(assemblyVersion);

                if (!string.IsNullOrEmpty(assemblyKey)) {
                    int byteCount = assemblyKey.Length / 2;
                    byte[] keyToken = new byte[byteCount];
                    for (int i = 0; i < byteCount; ++i) {
                        string byteString = assemblyKey.Substring(i * 2, 2);
                        keyToken[i] = byte.Parse(byteString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }

                    asmName.SetPublicKeyToken(keyToken);
                }
            }
        }
    }
}
