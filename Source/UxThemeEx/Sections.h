#pragma once
#include "Handle.h"
#include <array>
#include <windows.h>

namespace uxtheme
{

struct ROOTSECTION
{
    wchar_t szSharableSectionName[260];
    wchar_t szNonSharableSectionName[260];
    unsigned dwClientChangeNumber;
};

class Section
{
public:
    Section(DWORD desiredSectionAccess, DWORD desiredViewAccess);

protected:
    HRESULT OpenSection(wchar_t const* sectionName, bool mapView);

    std::array<wchar_t, MAX_PATH> sectionName;
    SectionHandle sectionHandle;
    FileViewHandle<> sectionData;
    DWORD desiredSectionAccess;
    DWORD desiredViewAccess;
};

enum class ThemeDataNamespace
{
    Unnamed = 0x0,
    Global = 0x1,
    Session = 0x2,
};

class DataSection : public Section
{
public:
    DataSection(DWORD desiredSectionAccess, DWORD desiredViewAccess);

    HRESULT Open(wchar_t const* name, bool mapView)
    {
        return OpenSection(name, mapView);
    }

    void DetachSectionHandle(HANDLE* phSection)
    {
        *phSection = sectionHandle.Detach();
    }

private:
    HRESULT MakeThemeDataSectionName(wchar_t* pszFullName, unsigned cchMax,
                                     ThemeDataNamespace dataNamespace,
                                     unsigned ulSessionID);
};

class RootSection : public Section
{
public:
    RootSection(ULONG sessionId, DWORD desiredSectionAccess, DWORD desiredViewAccess);
    HRESULT GetRootSectionData(ROOTSECTION** ppRootSection);
};

} // namespace uxtheme
