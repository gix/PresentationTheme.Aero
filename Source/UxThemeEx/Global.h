#pragma once
#include "Handle.h"
#include "RenderList.h"
#include "UxThemeFile.h"
#include "UxThemeEx.h"
#include <memory>

namespace uxtheme
{

struct ThemeFileEntry
{
    FileMappingHandle ReuseSection;
    std::unique_ptr<CUxThemeFile> ThemeFile;
};

extern std::vector<ThemeFileEntry> g_ThemeFileHandles;
extern CRenderList g_pRenderList;
extern HTHEMEFILE g_OverrideTheme;

size_t ThemeFileSlotFromHandle(HTHEMEFILE hThemeFile);
CUxThemeFile* ThemeFileFromHandle(HTHEMEFILE hThemeFile);
HTHEME OpenThemeDataExInternal(
    HTHEMEFILE hThemeFile, HWND hwnd, wchar_t const* pszClassIdList,
    unsigned dwFlags, wchar_t const* pszApiName, int iForDPI);

} // namespace uxtheme
