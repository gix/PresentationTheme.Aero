namespace StyleCore.Native
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>A custom marshaler for <see cref="ResourceName"/> objects.</summary>
    internal sealed class ResourceNameMarshaler : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance(string pstrCookie)
        {
            return new ResourceNameMarshaler();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return ResourceName.FromPtr(pNativeData);
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (!(ManagedObj is ResourceName))
                throw new ArgumentException(nameof(ManagedObj));
            var name = (ResourceName)ManagedObj;
            if (name.Name != null)
                return Marshal.StringToCoTaskMemUni(name.Name);
            return ResourceUnsafeNativeMethods.MAKEINTRESOURCE(name.Id);
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (!ResourceUnsafeNativeMethods.IS_INTRESOURCE(pNativeData))
                Marshal.FreeCoTaskMem(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public int GetNativeDataSize()
        {
            return IntPtr.Size;
        }
    }
}
