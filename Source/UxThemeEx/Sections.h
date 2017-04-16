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
    HRESULT OpenSection(wchar_t const* sectionName, bool mapView);

    void* View() const { return sectionData.Get(); }

protected:
    SectionHandle sectionHandle;
    FileViewHandle sectionData;
    DWORD desiredSectionAccess;
    DWORD desiredViewAccess;
};

class RootSection : public Section
{
public:
    RootSection(DWORD desiredSectionAccess, DWORD desiredViewAccess);
    HRESULT GetRootSectionData(ROOTSECTION** ppRootSection);

private:
    std::array<wchar_t, MAX_PATH> sectionName;
};

} // namespace uxtheme
