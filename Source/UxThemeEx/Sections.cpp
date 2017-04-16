#include "Sections.h"
#include "Utils.h"

#include <ntstatus.h>
#include <strsafe.h>
#include <winternl.h>

NTSYSAPI NTSTATUS NTAPI NtOpenSection(
    _Out_ PHANDLE            SectionHandle,
          _In_  ACCESS_MASK        DesiredAccess,
          _In_  POBJECT_ATTRIBUTES ObjectAttributes);

namespace uxtheme
{

Section::Section(DWORD desiredSectionAccess, DWORD desiredViewAccess)
    : desiredSectionAccess(desiredSectionAccess)
    , desiredViewAccess(desiredViewAccess)
{
}

HRESULT Section::OpenSection(wchar_t const* sectionName, bool mapView)
{
    UNICODE_STRING name;
    RtlInitUnicodeString(&name, sectionName);

    OBJECT_ATTRIBUTES objA;
    InitializeObjectAttributes(&objA, &name, OBJ_CASE_INSENSITIVE, nullptr,
                               nullptr);

    ModuleHandle ntdllHandle{LoadLibraryW(L"ntdll.dll")};
    auto NtOpenSectionPtr = (decltype(NtOpenSection)*)GetProcAddress(ntdllHandle, "NtOpenSection");

    SectionHandle sectionHandle;
    NTSTATUS st = NtOpenSectionPtr(sectionHandle.CloseAndGetAddressOf(),
                                   desiredSectionAccess, &objA);
    if (st != STATUS_SUCCESS)
        return st;

    if (mapView) {
        FileViewHandle sectionData{MapViewOfFile(sectionHandle, desiredViewAccess, 0, 0, 0)};
        if (!sectionData)
            return MakeErrorLast();

        this->sectionData = std::move(sectionData);
    }

    this->sectionHandle = std::move(sectionHandle);
    return S_OK;
}

RootSection::RootSection(DWORD desiredSectionAccess, DWORD desiredViewAccess)
    : Section(desiredSectionAccess, desiredViewAccess)
{
    DWORD sessionId = NtCurrentTeb()->ProcessEnvironmentBlock->SessionId;
    if (sessionId)
        StringCchPrintfW(sectionName.data(), sectionName.size(),
                         L"\\Sessions\\%d\\Windows\\ThemeSection", sessionId);
    else
        StringCchPrintfW(sectionName.data(), sectionName.size(),
                         L"\\Windows\\ThemeSection");
}

HRESULT RootSection::GetRootSectionData(ROOTSECTION** ppRootSection)
{
    *ppRootSection = nullptr;
    ENSURE_HR(OpenSection(sectionName.data(), true));
    if (!sectionData)
        return E_OUTOFMEMORY;

    *ppRootSection = static_cast<ROOTSECTION*>(sectionData.Get());
    return S_OK;
}

} // namespace uxtheme
