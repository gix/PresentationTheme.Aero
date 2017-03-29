#pragma once
#include <windows.h>

namespace uxtheme
{

struct DpiInfo
{
    unsigned _nDpiPlateausCurrentlyPresent = 0;
    unsigned _nNonStandardDpi = 0;
    int _fIsInitialized = 0;
    void Clear();
    HRESULT Ensure(unsigned a2);
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
