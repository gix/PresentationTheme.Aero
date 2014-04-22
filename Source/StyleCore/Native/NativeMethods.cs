namespace StyleCore.Native
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const int MESSAGE_RESOURCE_UNICODE = 1;

        [DllImport("kernel32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern IntPtr LocalFree(IntPtr hMem);
    }
}
