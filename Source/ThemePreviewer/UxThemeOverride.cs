namespace ThemePreviewer
{
    using System.Threading.Tasks;
    using StyleCore.Native;

    public class UxThemeOverride
    {
        private SafeThemeFileHandle theme;

        public string CurrentOverride { get; private set; }

        public static SafeThemeFileHandle LoadTheme(string path)
        {
            SafeThemeFileHandle themeFile;
            UxThemeExNativeMethods.UxOpenThemeFile(path, out themeFile).ThrowIfFailed();
            return themeFile;
        }

        public async Task SetThemeAsync(string path)
        {
            if (path != null)
                SetTheme(await Task.Run(() => LoadTheme(path)));
            else
                SetTheme(SafeThemeFileHandle.Zero);
            CurrentOverride = path;
            ThemeUtils.SendThemeChangedProcessLocal();
        }

        private void SetTheme(SafeThemeFileHandle newTheme)
        {
            if (newTheme != null && !newTheme.IsInvalid && !newTheme.IsClosed) {
                UxThemeExNativeMethods.UxOverrideTheme(newTheme).ThrowIfFailed();
                UxThemeExNativeMethods.UxHook().ThrowIfFailed();
                theme = newTheme;
            } else {
                theme = null;
                UxThemeExNativeMethods.UxUnhook().ThrowIfFailed();
                UxThemeExNativeMethods.UxOverrideTheme(newTheme).ThrowIfFailed();
            }
        }
    }
}
