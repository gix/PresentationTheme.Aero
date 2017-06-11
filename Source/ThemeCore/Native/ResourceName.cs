namespace ThemeCore.Native
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public sealed class ResourceName
    {
        public static ResourceName FromPtr(IntPtr ptr)
        {
            if (ResourceUnsafeNativeMethods.IS_INTRESOURCE(ptr))
                return new ResourceName((short)ptr.ToInt64());
            return new ResourceName(Marshal.PtrToStringUni(ptr));
        }

        public ResourceName(short id)
        {
            Id = id;
        }

        public ResourceName(string name)
        {
            Name = name;
        }

        public short Id { get; }

        public string Name { get; }

        public override string ToString()
        {
            if (Name != null)
                return Name;
            return Id.ToString(CultureInfo.InvariantCulture);
        }

        public static implicit operator ResourceName(int id)
        {
            return new ResourceName(checked((short)id));
        }

        public static implicit operator ResourceName(string name)
        {
            return new ResourceName(name);
        }
    }
}
