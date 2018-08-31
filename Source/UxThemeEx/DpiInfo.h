#pragma once
#include <windows.h>

namespace uxtheme
{

enum PLATEAU_INDEX
{
    DPI_PLATEAU_UNSUPPORTED = -1,
    DPI_PLATEAU_96 = 0,
    DPI_PLATEAU_120 = 1,
    DPI_PLATEAU_144 = 2,
    DPI_PLATEAU_192 = 3,
    DPI_PLATEAU_240 = 4,
    DPI_PLATEAU_288 = 5,
    DPI_PLATEAU_384 = 6,
    DPI_PLATEAU_COUNT = 7,
};

enum PLATEAU
{
    PL_1_0x = 0,
    PL_1_4x = 1,
    PL_1_8x = 2,
};

class DpiInfo
{
public:
    unsigned GetCurrentlyPresentDpiPlateaus() const
    {
        return _nDpiPlateausCurrentlyPresent;
    }

    bool IsPlateauCurrentlyPresent(PLATEAU_INDEX index) const
    {
        return _nDpiPlateausCurrentlyPresent & (1 << index);
    }

    void Clear();
    HRESULT Ensure(unsigned a2);

private:
    unsigned _nDpiPlateausCurrentlyPresent = 0;
    unsigned _nNonStandardDpi = 0;
    BOOL _fIsInitialized = FALSE;
};

extern bool g_fForcedDpi;
extern int g_nScreenDpi;
extern BOOL g_fDPIAware;
extern DpiInfo g_DpiInfo;

inline void DpiInfo::Clear()
{
    if (!g_fForcedDpi) {
        _nDpiPlateausCurrentlyPresent = 0;
        _nNonStandardDpi = 0;
        _fIsInitialized = 0;
    }
}

inline HRESULT DpiInfo::Ensure(unsigned a2)
{
    //if (!g_DpiInfo._fIsInitialized)
    //    return DiscoverCurrentlyPresentPlateaus(this, a2);
    return S_OK;
}

inline int GetScreenDpi()
{
    if (!g_fForcedDpi) {
        BOOL isDpiAware = IsProcessDPIAware();
        if (g_fDPIAware != isDpiAware || !g_nScreenDpi) {
            g_fDPIAware = isDpiAware;
            g_nScreenDpi = 96;

            if (HDC hdc = GetDC(nullptr)) {
                g_nScreenDpi = GetDeviceCaps(hdc, LOGPIXELSX);
                ReleaseDC(nullptr, hdc);
            }
        }
    }

    return g_nScreenDpi;
}

inline bool IsScreenDC(HDC hdc)
{
    if (hdc)
        return GetScreenDpi() == GetDeviceCaps(hdc, LOGPIXELSX) || g_fForcedDpi;

    return false;
}

} // namespace uxtheme
