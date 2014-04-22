namespace StyleCore.Native
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical]
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    public abstract class SafeHandleZeroIsInvalid : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected SafeHandleZeroIsInvalid(bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get { return handle == new IntPtr(); }
        }
    }
}
