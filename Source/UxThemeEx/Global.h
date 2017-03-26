#pragma once
#include "Utils.h"
#include "UxThemeFile.h"
#include "RenderList.h"
#include <memory>
#include "UxThemeEx.h"

namespace uxtheme
{

struct ThemeFileEntry
{
    Handle ReuseSection;
    std::unique_ptr<CUxThemeFile> ThemeFile;
};

extern std::vector<ThemeFileEntry> g_ThemeFileHandles;
extern CRenderList g_pRenderList;
extern HTHEMEFILE g_OverrideTheme;

size_t ThemeFileSlotFromHandle(HTHEMEFILE hThemeFile);
CUxThemeFile* ThemeFileFromHandle(HTHEMEFILE hThemeFile);

} // namespace uxtheme
