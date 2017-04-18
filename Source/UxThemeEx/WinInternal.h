#pragma once
#include <windows.h>
#include <winternl.h>

typedef struct _PEB_REAL
{
    BYTE InheritedAddressSpace;
    BYTE ReadImageFileExecOptions;
    BYTE BeingDebugged;
    union
    {
        BYTE BitField;
        struct
        {
            BYTE ImageUsesLargePages : 1;
            BYTE IsProtectedProcess : 1;
            BYTE IsImageDynamicallyRelocated : 1;
            BYTE SkipPatchingUser32Forwarders : 1;
            BYTE IsPackagedProcess : 1;
            BYTE IsAppContainer : 1;
            BYTE IsProtectedProcessLight : 1;
            BYTE IsLongPathAwareProcess : 1;
        };
    };
    BYTE Padding0[4];
    PVOID Mutant;
    PVOID ImageBaseAddress;
    PEB_LDR_DATA* Ldr;
    RTL_USER_PROCESS_PARAMETERS* ProcessParameters;
    PVOID SubSystemData;
    PVOID ProcessHeap;
    CRITICAL_SECTION* FastPebLock;
    SLIST_HEADER* volatile AtlThunkSListPtr;
    PVOID IFEOKey;
    union
    {
        ULONG CrossProcessFlags;
        struct
        {
            ULONG ProcessInJob : 1;
            ULONG ProcessInitializing : 1;
            ULONG ProcessUsingVEH : 1;
            ULONG ProcessUsingVCH : 1;
            ULONG ProcessUsingFTH : 1;
            ULONG ProcessPreviouslyThrottled : 1;
            ULONG ProcessCurrentlyThrottled : 1;
            ULONG ReservedBits0 : 25;
        };
    };
    BYTE Padding1[4];
    union
    {
        PVOID KernelCallbackTable;
        PVOID UserSharedInfoPtr;
    };
    ULONG SystemReserved[1];
    ULONG AtlThunkSListPtr32;
    PVOID ApiSetMap;
    ULONG TlsExpansionCounter;
    BYTE Padding2[4];
    PVOID TlsBitmap;
    ULONG TlsBitmapBits[2];
    PVOID ReadOnlySharedMemoryBase;
    PVOID SharedData;
    PVOID* ReadOnlyStaticServerData;
    PVOID AnsiCodePageData;
    PVOID OemCodePageData;
    PVOID UnicodeCaseTableData;
    ULONG NumberOfProcessors;
    ULONG NtGlobalFlag;
    LARGE_INTEGER CriticalSectionTimeout;
    ULONGLONG HeapSegmentReserve;
    ULONGLONG HeapSegmentCommit;
    ULONGLONG HeapDeCommitTotalFreeThreshold;
    ULONGLONG HeapDeCommitFreeBlockThreshold;
    ULONG NumberOfHeaps;
    ULONG MaximumNumberOfHeaps;
    PVOID* ProcessHeaps;
    PVOID GdiSharedHandleTable;
    PVOID ProcessStarterHelper;
    ULONG GdiDCAttributeList;
    BYTE Padding3[4];
    CRITICAL_SECTION* LoaderLock;
    ULONG OSMajorVersion;
    ULONG OSMinorVersion;
    USHORT OSBuildNumber;
    USHORT OSCSDVersion;
    ULONG OSPlatformId;
    ULONG ImageSubsystem;
    ULONG ImageSubsystemMajorVersion;
    ULONG ImageSubsystemMinorVersion;
    BYTE Padding4[4];
    ULONGLONG ActiveProcessAffinityMask;
    ULONG GdiHandleBuffer[60];
    VOID (*PostProcessInitRoutine)();
    PVOID TlsExpansionBitmap;
    ULONG TlsExpansionBitmapBits[32];
    ULONG SessionId;
    BYTE Padding5[4];
    ULARGE_INTEGER AppCompatFlags;
    ULARGE_INTEGER AppCompatFlagsUser;
    PVOID pShimData;
    PVOID AppCompatInfo;
    UNICODE_STRING CSDVersion;
    struct _ACTIVATION_CONTEXT_DATA const* ActivationContextData;
    struct _ASSEMBLY_STORAGE_MAP* ProcessAssemblyStorageMap;
    struct _ACTIVATION_CONTEXT_DATA const* SystemDefaultActivationContextData;
    struct _ASSEMBLY_STORAGE_MAP* SystemAssemblyStorageMap;
    ULONGLONG MinimumStackCommit;
    struct _FLS_CALLBACK_INFO* FlsCallback;
    LIST_ENTRY FlsListHead;
    PVOID FlsBitmap;
    ULONG FlsBitmapBits[4];
    ULONG FlsHighIndex;
    PVOID WerRegistrationData;
    PVOID WerShipAssertPtr;
    PVOID pUnused;
    PVOID pImageHeaderHash;
    union
    {
        ULONG TracingFlags;
        struct
        {
            ULONG HeapTracingEnabled : 1;
            ULONG CritSecTracingEnabled : 1;
            ULONG LibLoaderTracingEnabled : 1;
            ULONG SpareTracingBits : 29;
        };
    };
    BYTE Padding6[4];
    ULONGLONG CsrServerReadOnlySharedMemoryBase;
    ULONGLONG TppWorkerpListLock;
    LIST_ENTRY TppWorkerpList;
    PVOID WaitOnAddressHashTable[128];
} PEB_REAL, *PPEB_REAL;
