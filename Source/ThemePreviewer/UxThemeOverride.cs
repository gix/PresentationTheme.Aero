namespace ThemePreviewer
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using PresentationTheme.Aero;
    using ThemeCore.Native;

    public class UxThemeLoadParams : IEquatable<UxThemeLoadParams>
    {
        public bool IsHighContrast { get; set; }
        public UxColorScheme CustomColors { get; set; }

        public bool Equals(UxThemeLoadParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                IsHighContrast == other.IsHighContrast &&
                Equals(CustomColors, other.CustomColors);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UxThemeLoadParams);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (IsHighContrast.GetHashCode() * 397) ^ (CustomColors != null ? CustomColors.GetHashCode() : 0);
            }
        }

        public static bool operator ==(UxThemeLoadParams left, UxThemeLoadParams right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UxThemeLoadParams left, UxThemeLoadParams right)
        {
            return !Equals(left, right);
        }
    }

    public class UxThemeOverride
    {
        private SafeThemeFileHandle theme;

        public string CurrentOverride { get; private set; }

        public static SafeThemeFileHandle LoadTheme(
            string path, UxThemeLoadParams loadParams = null)
        {
            SafeThemeFileHandle themeFile;
            UxThemeExNativeMethods.UxOpenThemeFileEx(
                path,
                loadParams?.IsHighContrast ?? false,
                loadParams?.CustomColors,
                out themeFile).ThrowIfFailed();

            return themeFile;
        }

        public async Task SetThemeAsync(string path, UxThemeLoadParams loadParams = null)
        {
            if (path != null)
                SetNativeTheme(await Task.Run(() => LoadTheme(path, loadParams)));
            else
                SetNativeTheme(SafeThemeFileHandle.Zero);

            CurrentOverride = path;
            UxThemeExNativeMethods.UxBroadcastThemeChange();
        }

        private void SetNativeTheme(SafeThemeFileHandle newTheme)
        {
            var oldTheme = theme;
            if (newTheme != null && !newTheme.IsInvalid && !newTheme.IsClosed) {
                UxThemeExNativeMethods.UxOverrideTheme(newTheme).ThrowIfFailed();
                UxThemeExNativeMethods.UxHook().ThrowIfFailed();
                theme = newTheme;
            } else {
                theme = null;
                UxThemeExNativeMethods.UxUnhook().ThrowIfFailed();
                UxThemeExNativeMethods.UxOverrideTheme(SafeThemeFileHandle.Zero).ThrowIfFailed();
            }

            oldTheme?.Dispose();
        }

        public async Task SetPresentationFrameworkTheme(Dispatcher dispatcher, Uri resourceUri)
        {
            await dispatcher.InvokeAsync(() => {
                if (resourceUri != null)
                    ThemeManager.SetPresentationFrameworkTheme(resourceUri);
                else
                    ThemeManager.ClearPresentationFrameworkTheme();
                UxThemeExNativeMethods.UxBroadcastThemeChange();
            }, DispatcherPriority.ContextIdle);
        }
    }
}
