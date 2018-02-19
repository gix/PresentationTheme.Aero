namespace ThemeCore.Native
{
    public sealed class SafeThemeFileHandle : SafeHandleZeroIsInvalid
    {
        private SafeThemeFileHandle()
            : base(true)
        {
        }

        public static SafeThemeFileHandle Zero => new SafeThemeFileHandle();

        protected override bool ReleaseHandle()
        {
            return UxThemeExNativeMethods.UxCloseThemeFile(handle) >= 0;
        }
    }
}
