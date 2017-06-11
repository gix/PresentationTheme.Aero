namespace ThemeCore.Native
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    ///   Represents a wrapper class for a handle allocated by LocalAlloc.
    /// </summary>
    [SecurityCritical]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeLocalMemHandle : SafeHandleZeroIsInvalid
    {
        public SafeLocalMemHandle()
            : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.LocalFree(handle) == IntPtr.Zero;
        }
    }
}
