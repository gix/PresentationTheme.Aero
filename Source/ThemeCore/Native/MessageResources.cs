namespace ThemeCore.Native
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MESSAGE_RESOURCE_DATA
    {
        public uint NumberOfBlocks;
        //[MarshalAs(UnmanagedType.ByValArray, SizeParamIndex = 0)]
        public MESSAGE_RESOURCE_BLOCK[] Blocks;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MESSAGE_RESOURCE_BLOCK
    {
        public uint LowId;
        public uint HighId;
        public uint Offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MESSAGE_RESOURCE_ENTRY
    {
        public ushort Length;
        public ushort Flags;
        //[MarshalAs(UnmanagedType.ByValArray, SizeParamIndex = 0)]
        public string Text;
    }
}
