namespace ThemeCore.Native
{
    using System;

    [Flags]
    public enum LoadLibraryExFlags : uint
    {
        /// <native>DONT_RESOLVE_DLL_REFERENCES</native>
        DontResolveDllReferences = 0x00000001,
        /// <native>LOAD_IGNORE_CODE_AUTHZ_LEVEL</native>
        LoadIgnoreCodeAuthzLevel = 0x00000010,
        /// <native>LOAD_LIBRARY_AS_DATAFILE</native>
        AsDatafile = 0x00000002,
        /// <native>LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE</native>
        AsDatafileExclusive = 0x00000040,
        /// <native>LOAD_LIBRARY_AS_IMAGE_RESOURCE</native>
        AsImageResource = 0x00000020,
        /// <native>LOAD_LIBRARY_SEARCH_APPLICATION_DIR</native>
        SearchApplicationDir = 0x00000200,
        /// <native>LOAD_LIBRARY_SEARCH_DEFAULT_DIRS</native>
        SearchDefaultDirs = 0x00001000,
        /// <native>LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR</native>
        SearchDllLoadDir = 0x00000100,
        /// <native>LOAD_LIBRARY_SEARCH_SYSTEM32</native>
        SearchSystem32 = 0x00000800,
        /// <native>LOAD_LIBRARY_SEARCH_USER_DIRS</native>
        SearchUserDirs = 0x00000400,
        /// <native>LOAD_WITH_ALTERED_SEARCH_PATH</native>
        LoadWithAlteredSearchPath = 0x00000008,
    }
}
