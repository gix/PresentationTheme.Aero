#include "UxThemeDllHelper.h"

#include "SymbolContext.h"
#include "Utils.h"

namespace uxtheme
{

UxThemeDllHelper const& UxThemeDllHelper::Get()
{
    static UxThemeDllHelper instance;
    return instance;
}

struct UxThemeInfo
{
    VersionInfo Version{};
    uintptr_t addresses[UxThemeDllHelper::ProcCount];
};

static UxThemeInfo const KnownInfos[] = {
    // Windows 10 1803
    {
        {10, 0, 17134, 1},
        0x171B01890 - 0x171B00000,
        0x171B24264 - 0x171B00000,
        0x171B03E04 - 0x171B00000,
    },
    // Windows 10 1709
    {
        {10, 0, 16299, 15},
        0x171B0C1F0 - 0x171B00000,
        0x171B21764 - 0x171B00000,
        0x171B12BF8 - 0x171B00000,
    },
    // Windows 10 1703
    {
        {10, 0, 15063, 0},
        0x171B106D0 - 0x171B00000,
        0x171B231A4 - 0x171B00000,
        0x171B16F34 - 0x171B00000,
    },
};

UxThemeDllHelper::UxThemeDllHelper()
{
    auto const uxthemeModule = GetModuleHandleW(L"uxtheme");
    if (!uxthemeModule)
        return;

    SymbolContext ctx;
    if (FAILED(ctx.Initialize()))
        return;

    if (FAILED(ctx.LoadModule(uxthemeModule)))
        return;

    UxThemeInfo const* knownInfo = nullptr;
    if (VersionInfo version;
        SUCCEEDED(GetModuleFileVersion(uxthemeModule, version))) {
        for (auto& entry : KnownInfos) {
            if (entry.Version == version) {
                knownInfo = &entry;
                break;
            }
        }
    }

    char const* ProcNames[ProcCount];
    ProcNames[Proc_CThemeMenuBar_DrawItem] = "CThemeMenuBar::DrawItem";
    ProcNames[Proc_CThemeMenuMetrics_FlushAll] = "CThemeMenuMetrics::FlushAll";
    ProcNames[Proc_OpenThemeDataExInternal] = "OpenThemeDataExInternal";

    for (unsigned i = 0; i < ProcCount; ++i) {
        if (FAILED(ctx.GetSymbolAddress(uxthemeModule, ProcNames[i],
                                        addresses[i])) &&
            knownInfo) {
            addresses[i] = (uintptr_t)uxthemeModule + knownInfo->addresses[i];
        }
    }
}

} // namespace uxtheme
