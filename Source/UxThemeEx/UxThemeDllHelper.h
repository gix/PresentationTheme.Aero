#pragma once
#include <cstdint>

namespace uxtheme
{

class UxThemeDllHelper
{
public:
    enum Proc
    {
        Proc_CThemeMenuBar_DrawItem,
        Proc_CThemeMenuMetrics_FlushAll,
        Proc_OpenThemeDataExInternal,
        ProcCount,
    };

    static UxThemeDllHelper const& Get();

    UxThemeDllHelper();

    void CThemeMenuMetrics_FlushAll() const
    {
        if (auto const addr = addresses[Proc_CThemeMenuMetrics_FlushAll])
            reinterpret_cast<void (*)()>(addr)();
    }

    void* OpenThemeDataExInternal_address() const
    {
        return (void*)addresses[Proc_OpenThemeDataExInternal];
    }

    void* CThemeMenuBar_DrawItem_address() const
    {
        return (void*)addresses[Proc_CThemeMenuBar_DrawItem];
    }

private:
    uintptr_t addresses[ProcCount]{};
};

} // namespace uxtheme
