namespace StyleCore.Native
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    public static class ResourceUnsafeNativeMethods
    {
        private const string Kernel32 = "kernel32.dll";
        private const string User32 = "user32.dll";
        public static readonly IntPtr RT_MESSAGETABLE = MAKEINTRESOURCE(11);

        public static bool IS_INTRESOURCE(IntPtr ptr)
        {
            return (ptr.ToInt64() >> 16) == 0;
        }

        public static IntPtr MAKEINTRESOURCE(short i)
        {
            return new IntPtr(i);
        }

        public static short MAKELANGID(ushort primaryLanguage, ushort subLanguage)
        {
            return (short)(
                (short)(unchecked((short)subLanguage) << 10) |
                unchecked((short)primaryLanguage));
        }

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeModuleHandle LoadLibraryEx(
            string lpFileName,
            IntPtr hFile,
            [MarshalAs(UnmanagedType.U4)] LoadLibraryExFlags dwFlags);

        [DllImport(Kernel32, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumResTypeProc(
            IntPtr hModule, IntPtr lpszType, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumResNameProc(
            IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumResourceTypesEx(
            SafeModuleHandle hModule,
            EnumResTypeProc lpEnumFunc,
            IntPtr lParam,
            uint dwFlags,
            uint LangId);

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumResourceNamesEx(
            SafeModuleHandle hModule,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ResourceNameMarshaler))]
            ResourceName lpszType,
            EnumResNameProc lpEnumFunc,
            IntPtr lParam,
            uint dwFlags,
            uint LangId);

        public static List<ResourceName> GetResourceTypesEx(
            SafeModuleHandle module)
        {
            var names = new List<ResourceName>();
            EnumResourceTypesEx(
                module,
                (m, type, p) => {
                    names.Add(ResourceName.FromPtr(type));
                    return true;
                },
                IntPtr.Zero,
                0,
                0);
            return names;
        }

        public static List<ResourceName> GetResourceNamesEx(
            SafeModuleHandle module, ResourceName type)
        {
            var names = new List<ResourceName>();
            EnumResourceNamesEx(
                module,
                type,
                (m, t, name, p) => {
                    names.Add(ResourceName.FromPtr(name));
                    return true;
                },
                IntPtr.Zero,
                0,
                0);
            return names;
        }

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern ResInfoHandle FindResource(
            SafeModuleHandle hModule,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ResourceNameMarshaler))]
            ResourceName lpName,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ResourceNameMarshaler))]
            ResourceName lpType);

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern ResInfoHandle FindResourceEx(
            SafeModuleHandle hModule,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ResourceNameMarshaler))]
            ResourceName lpType,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ResourceNameMarshaler))]
            ResourceName lpName,
            short wLanguage);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern ResDataHandle LoadResource(
            SafeModuleHandle hModule, ResInfoHandle hResInfo);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern ResourceBuffer LockResource(ResDataHandle hResData);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern uint SizeofResource(
            SafeModuleHandle hModule, ResInfoHandle hResInfo);

        [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int LoadString(
            SafeModuleHandle hInstance, uint uID, [Out] StringBuilder lpBuffer, int nBufferMax);

        public static string LoadString(SafeModuleHandle hInstance, uint uID)
        {
            var buffer = new StringBuilder(64);
            while (true) {
                int count = LoadString(hInstance, uID, buffer, buffer.Capacity);
                if (count == 0)
                    return null;
                if (count < buffer.Capacity - 1)
                    break;
                buffer.Capacity *= 2;
            }

            return buffer.ToString();
        }
    }

    /// <summary>
    ///   A handle to a resource information block.
    /// </summary>
    public sealed class ResInfoHandle : SafeHandleZeroIsInvalid
    {
        private ResInfoHandle()
            : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return true;
        }
    }

    public sealed class ResDataHandle : SafeHandleZeroIsInvalid
    {
        private ResDataHandle()
            : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return true;
        }
    }

    public sealed class ResourceBuffer : SafeBuffer
    {
        private ResourceBuffer()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }
}
