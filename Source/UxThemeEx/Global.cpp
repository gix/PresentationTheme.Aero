#include "Global.h"

namespace uxtheme
{

std::vector<ThemeFileEntry> g_ThemeFileHandles;
CRenderList g_pRenderList;
HTHEMEFILE g_OverrideTheme = nullptr;

size_t ThemeFileSlotFromHandle(HTHEMEFILE hThemeFile)
{
    auto idx = static_cast<size_t>(reinterpret_cast<uintptr_t>(hThemeFile));
    if (idx >= 1 && idx < g_ThemeFileHandles.size())
        return idx;
    return 0;
}

CUxThemeFile* ThemeFileFromHandle(HTHEMEFILE hThemeFile)
{
    auto idx = ThemeFileSlotFromHandle(hThemeFile);
    return idx ? g_ThemeFileHandles[idx].ThemeFile.get() : nullptr;
}

} // namespace uxtheme
