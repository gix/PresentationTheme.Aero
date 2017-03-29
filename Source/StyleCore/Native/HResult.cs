namespace StyleCore.Native
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///   HRESULT Wrapper.
    /// </summary>
    public enum HResult
    {
        /// <summary>Operation successful.</summary>
        /// <native>S_OK</native>
        OK = 0,

        /// <summary>False status.</summary>
        /// <native>S_FALSE</native>
        False = 1,

        /// <summary>Not implemented.</summary>
        /// <native>E_NOTIMPL</native>
        NotImplemented = unchecked((int)0x80004001),

        /// <summary>No such interface supported.</summary>
        /// <native>E_NOINTERFACE</native>
        NoInterface = unchecked((int)0x80004002),

        /// <summary>Pointer that is not valid.</summary>
        /// <native>E_POINTER</native>
        InvalidPointer = unchecked((int)0x80004003),

        /// <summary>Operation aborted.</summary>
        /// <native>E_ABORT</native>
        Aborted = unchecked((int)0x80004004),

        /// <summary>Unexpected failure.</summary>
        /// <native>E_UNEXPECTED</native>
        Unexpected = unchecked((int)0x8000FFFF),

        /// <summary>Unspecified failure.</summary>
        /// <native>E_FAIL</native>
        Failed = unchecked((int)0x80004005),

        /// <summary>No object for moniker.</summary>
        /// <native>MK_E_NOOBJECT</native>
        NoObject = unchecked((int)0x800401E5),

        /// <summary>The object is already registered.</summary>
        /// <native>CO_E_OBJISREG</native>
        ObjectAlreadyRegistered = unchecked((int)0x800401FC),

        /// <native>TYPE_E_ELEMENTNOTFOUND</native>
        TypeElementNotFound = unchecked((int)0x8002802B),

        /// <summary>The system cannot find the file specified.</summary>
        /// <native>DRM_E_WIN32_FILE_NOT_FOUND</native>
        FileNotFound = unchecked((int)0x80070002),

        /// <summary>General access denied error.</summary>
        /// <native>E_ACCESSDENIED</native>
        AccessDenied = unchecked((int)0x80070005),

        /// <summary>Handle that is not valid.</summary>
        /// <native>E_ABORT</native>
        InvalidHandle = unchecked((int)0x80070006),

        /// <summary>Failed to allocate necessary memory.</summary>
        /// <native>E_OUTOFMEMORY</native>
        OutOfMemory = unchecked((int)0x8007000E),

        /// <summary>One or more arguments are not valid.</summary>
        /// <native>E_INVALIDARG</native>
        InvalidArgument = unchecked((int)0x80070057),

        /// <summary>The requested resource is in use.</summary>
        ResourceInUse = unchecked((int)0x800700AA),

        ElementNotFound = unchecked((int)0x80070490),

        Cancelled = unchecked((int)0x800704C7)
    }

    public static class HResultExtensions
    {
        public static void ThrowIfFailed(this HResult hr)
        {
            if (hr.Failed())
                throw Marshal.GetExceptionForHR((int)hr);
        }

        public static bool Succeeded(this HResult hr)
        {
            return hr >= 0;
        }

        public static bool Failed(this HResult hr)
        {
            return hr < 0;
        }
    }
}
