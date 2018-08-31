#include "Global.h"

namespace uxtheme
{

std::vector<ThemeFileEntry> g_ThemeFileHandles;
HTHEMEFILE g_OverrideTheme = nullptr;

size_t ThemeFileSlotFromHandle(HTHEMEFILE hThemeFile)
{
    auto idx = static_cast<size_t>(reinterpret_cast<uintptr_t>(hThemeFile));
    if (idx >= 1 && idx < g_ThemeFileHandles.size())
        return idx;
    return 0;
}

ThemeFileEntry* ThemeFileEntryFromHandle(HTHEMEFILE hThemeFile)
{
    auto idx = ThemeFileSlotFromHandle(hThemeFile);
    return idx ? &g_ThemeFileHandles[idx] : nullptr;
}

CUxThemeFile* ThemeFileFromHandle(HTHEMEFILE hThemeFile)
{
    auto idx = ThemeFileSlotFromHandle(hThemeFile);
    return idx ? &g_ThemeFileHandles[idx].ThemeFile() : nullptr;
}

} // namespace uxtheme
