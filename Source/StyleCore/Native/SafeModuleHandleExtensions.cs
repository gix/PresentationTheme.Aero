namespace StyleCore.Native
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class SafeModuleHandleExtensions
    {
        public static unsafe byte* DangerousGetPointer(this SafeBuffer buffer)
        {
            byte* pointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                buffer.AcquirePointer(ref pointer);
            } finally {
                if (pointer != null)
                    buffer.ReleasePointer();
            }

            return pointer;
        }

        public static uint ResourceSize(this SafeModuleHandle module, ResInfoHandle resource)
        {
            return ResourceUnsafeNativeMethods.SizeofResource(module, resource);
        }

        public static ResInfoHandle FindResource(
            this SafeModuleHandle module, ResourceName type, ResourceName name)
        {
            return ResourceUnsafeNativeMethods.FindResource(module, name, type);
        }

        public static ResInfoHandle FindResourceEx(
            this SafeModuleHandle module, ResourceName type, ResourceName name, short language)
        {
            return ResourceUnsafeNativeMethods.FindResourceEx(module, type, name, language);
        }

        public static ResourceBuffer LoadResource(
            this SafeModuleHandle module, ResInfoHandle resource)
        {
            if (resource.IsInvalid)
                throw new ArgumentException("Invalid resource info handle.");

            ResDataHandle data = ResourceUnsafeNativeMethods.LoadResource(module, resource);
            if (data.IsInvalid)
                throw new Win32Exception();

            uint size = ResourceUnsafeNativeMethods.SizeofResource(module, resource);
            if (size == 0)
                throw new Win32Exception();

            ResourceBuffer ptr = ResourceUnsafeNativeMethods.LockResource(data);
            ptr.Initialize(size);

            return ptr;
        }

        public static UnmanagedMemoryAccessor LoadResourceAccessor(
            this SafeModuleHandle module, ResInfoHandle resource)
        {
            var ptr = module.LoadResource(resource);
            var size = (long)ptr.ByteLength;
            return new UnmanagedMemoryAccessor(ptr, 0, size, FileAccess.Read);
        }

        public static unsafe UnmanagedMemoryStream LoadResourceStream(
            this SafeModuleHandle module, ResInfoHandle resInfo)
        {
            var ptr = module.LoadResource(resInfo);
            var size = (long)ptr.ByteLength;
            return new UnmanagedMemoryStream(
                ptr.DangerousGetPointer(), size, size, FileAccess.Read);
        }

        public static byte[] LoadResourceData(
            this SafeModuleHandle module, ResInfoHandle resource)
        {
            var ptr = module.LoadResource(resource);
            var size = (long)ptr.ByteLength;
            var data = new byte[size];
            Marshal.Copy(ptr.DangerousGetHandle(), data, 0, data.Length);
            return data;
        }

        public static T LoadResourceData<T>(
            this SafeModuleHandle module, ResInfoHandle resource)
        {
            var ptr = module.LoadResource(resource);
            var size = (long)ptr.ByteLength;
            var type = typeof(T);
            if (size != Marshal.SizeOf(type))
                return default(T);

            if (type.IsValueType && !type.IsPrimitive)
                return Marshal.PtrToStructure<T>(ptr.DangerousGetHandle());

            if (type == typeof(sbyte) || type == typeof(byte))
                return (T)(object)Marshal.ReadByte(ptr.DangerousGetHandle());
            if (type == typeof(short) || type == typeof(ushort))
                return (T)(object)Marshal.ReadInt16(ptr.DangerousGetHandle());
            if (type == typeof(int) || type == typeof(uint))
                return (T)(object)Marshal.ReadInt32(ptr.DangerousGetHandle());
            if (type == typeof(long) || type == typeof(ulong))
                return (T)(object)Marshal.ReadInt64(ptr.DangerousGetHandle());

            throw new ArgumentException("Unable to marshal to type T.");
        }
    }
}
