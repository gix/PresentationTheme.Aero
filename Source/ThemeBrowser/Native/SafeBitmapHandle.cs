namespace ThemeBrowser.Native
{
    using System;
    using ThemeCore.Native;

    public class SafeBitmapHandle : SafeHandleZeroIsInvalid
    {
        private SafeBitmapHandle()
            : base(true)
        {
        }

        public SafeBitmapHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return GdiNativeMethods.DeleteObject(handle);
        }
    }
}
