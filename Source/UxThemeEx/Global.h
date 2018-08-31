#pragma once
#include "Handle.h"
#include "RenderList.h"
#include "UxThemeFile.h"
#include "UxThemeEx.h"
#include <memory>

namespace uxtheme
{

class ThemeFileEntry
{
public:
    ThemeFileEntry(std::unique_ptr<CUxThemeFile> themeFile,
                   FileMappingHandle reuseSection,
                   std::unique_ptr<CRenderList> renderList)
        : themeFile(std::move(themeFile))
        , reuseSection(std::move(reuseSection))
        , renderList(std::move(renderList))
    {
    }

    ThemeFileEntry() = default;
    ThemeFileEntry(ThemeFileEntry&&) = default;
    ThemeFileEntry& operator =(ThemeFileEntry&&) = default;

    CUxThemeFile& ThemeFile() const { return *themeFile; }
    CRenderList& RenderList() const { return *renderList; }

    explicit operator bool() const { return themeFile != nullptr; }

    void Free()
    {
        auto nonSharableDataHdr = themeFile->NonSharableDataHeader();
        renderList->FreeRenderObjects(nonSharableDataHdr->iLoadId);
        reuseSection.Reset();
        themeFile.reset();
        renderList.reset();
    }

private:
    std::unique_ptr<CUxThemeFile> themeFile;
    FileMappingHandle reuseSection;
    std::unique_ptr<CRenderList> renderList;
};

extern std::vector<ThemeFileEntry> g_ThemeFileHandles;
extern HTHEMEFILE g_OverrideTheme;

size_t ThemeFileSlotFromHandle(HTHEMEFILE hThemeFile);
ThemeFileEntry* ThemeFileEntryFromHandle(HTHEMEFILE hThemeFile);
CUxThemeFile* ThemeFileFromHandle(HTHEMEFILE hThemeFile);
HTHEME OpenThemeDataExInternal(
    HTHEMEFILE hThemeFile, HWND hwnd, wchar_t const* pszClassIdList,
    unsigned dwFlags, wchar_t const* pszApiName, int iForDPI);

} // namespace uxtheme
