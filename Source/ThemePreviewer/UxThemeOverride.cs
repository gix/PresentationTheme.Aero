namespace ThemePreviewer
{
    using StyleCore.Native;

    public class UxThemeOverride
    {
        private SafeThemeFileHandle theme;

        public void SetTheme(SafeThemeFileHandle newTheme)
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

        public static SafeThemeFileHandle LoadTheme(string path)
        {
            SafeThemeFileHandle themeFile;
            UxThemeExNativeMethods.UxOpenThemeFile(path, out themeFile).ThrowIfFailed();
            return themeFile;
        }
    }
}