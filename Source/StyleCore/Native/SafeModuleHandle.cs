namespace StyleCore.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>Represents a wrapper class for a HMODULE handle.</summary>
    [SecurityCritical]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeModuleHandle : SafeHandleZeroIsInvalid
    {
        public static SafeModuleHandle Zero => new SafeModuleHandle();

        /// <summary>
        ///   Loads the specified module.
        /// </summary>
        /// <param name="fileName">
        ///   A string that specifies the file name of the module to load.
        /// </param>
        /// <param name="flags">
        ///   The action to be taken when loading the module.
        /// </param>
        /// <returns>
        ///   A <see cref="SafeModuleHandle"/> to the loaded module.
        /// </returns>
        public static SafeModuleHandle Load(string fileName, LoadLibraryExFlags flags = 0)
        {
            SafeModuleHandle handle = ResourceUnsafeNativeMethods.LoadLibraryEx(
                fileName, IntPtr.Zero, flags);
            if (handle.IsInvalid)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return handle;
        }

        /// <summary>
        ///   Loads the specified module as a data file.
        /// </summary>
        /// <param name="fileName">
        ///   A string that specifies the file name of the module to load.
        /// </param>
        /// <returns>
        ///   A <see cref="SafeModuleHandle"/> to the loaded module. The handle
        ///   can only be used to extract messages or resources. You cannot call
        ///   functions like <c>GetProcAddress</c> with it.
        /// </returns>
        /// <remarks>
        ///   Maps the file into the calling process's virtual address space as
        ///   if it were a data file. Nothing is done to execute or prepare to
        ///   execute the mapped file. The loader does not load the static imports
        ///   or perform the other usual initialization steps. Use this flag
        ///   when you want to load a DLL only to extract messages or resources
        ///   from it.
        /// </remarks>
        public static SafeModuleHandle LoadImageResource(string fileName)
        {
            const LoadLibraryExFlags ImageResourceFlags =
                LoadLibraryExFlags.AsImageResource | LoadLibraryExFlags.AsDatafile;
            return Load(fileName, ImageResourceFlags);
        }

        private SafeModuleHandle()
            : base(true)
        {
        }

        /// <summary>
        ///   Frees the loaded module and, if necessary, decrements its reference
        ///   count. When the reference count reaches zero, the module is unloaded
        ///   from the address space of the calling process and the handle is no
        ///   longer valid.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if the handle is released successfully;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return ResourceUnsafeNativeMethods.FreeLibrary(handle);
        }
    }
}
