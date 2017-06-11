namespace ThemeCore.Native
{
    using System;

    public sealed class SafeThemeHandle : SafeHandleZeroIsInvalid
    {
        public SafeThemeHandle()
            : base(true)
        {
        }

        private SafeThemeHandle(IntPtr handle)
            : base(false)
        {
            SetHandle(handle);
        }

        public static SafeThemeHandle NonOwning(IntPtr handle)
        {
            return new SafeThemeHandle(handle);
        }

        public static SafeThemeHandle Zero => new SafeThemeHandle();

        protected override bool ReleaseHandle()
        {
            return StyleNativeMethods.CloseThemeData(handle) >= 0;
        }
    }
}
