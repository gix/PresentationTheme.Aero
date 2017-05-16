#include "UxThemeEx.h"

#include "Global.h"
#include "Utils.h"

#include <array>
#include <unordered_map>
#include <easyhook.h>

typedef struct _HOOK_ACL_
{
    ULONG Count;
    BOOL IsExclusive;
    ULONG Entries[MAX_ACE_COUNT];
} HOOK_ACL;

typedef struct _LOCAL_HOOK_INFO_
{
    PLOCAL_HOOK_INFO Next;
    ULONG NativeSize;
    UCHAR* TargetProc;
    ULONGLONG TargetBackup;
    ULONGLONG TargetBackup_x64;
    ULONGLONG HookCopy;
    ULONG EntrySize;
    UCHAR* Trampoline;
    ULONG HLSIndex;
    ULONG HLSIdent;
    void* Callback;
    HOOK_ACL LocalACL;
    ULONG Signature;
    TRACED_HOOK_HANDLE Tracking;
    void* RandomValue; // fixed
    void* HookIntro; // fixed
    UCHAR* OldProc; // fixed
    UCHAR* HookProc; // fixed
    void* HookOutro; // fixed
    int* IsExecutedPtr; // fixed
} LOCAL_HOOK_INFO, *PLOCAL_HOOK_INFO;

template<typename T>
struct HookTraceInfo : HOOK_TRACE_INFO
{
    T* Orig() { return (T*)Link->OldProc; }
};

typedef struct __declspec(align(8)) tagUAHMENU
{
    HMENU hmenu;
    HDC hdc;
    DWORD dwFlags;
} UAHMENU, *PUAHMENU;

typedef union tagUAHMENUITEMMETRICS
{
    SIZE rgsizeBar[2];
    SIZE rgsizePopup[4];
} UAHMENUITEMMETRICS, *PUAHMENUITEMMETRICS;

typedef struct tagUAHMENUPOPUPMETRICS
{
    int rgcx[4];
    BOOL fUnused : 1;
} UAHMENUPOPUPMETRICS, *PUAHMENUPOPUPMETRICS;

typedef struct tagUAHMENUITEM
{
    int iPosition;
    UAHMENUITEMMETRICS umim;
    UAHMENUPOPUPMETRICS umpm;
} UAHMENUITEM, *PUAHMENUITEM;

typedef struct tagUAHDRAWMENUITEM
{
    DRAWITEMSTRUCT dis;
    UAHMENU um;
    UAHMENUITEM umi;
} UAHDRAWMENUITEM, *PUAHDRAWMENUITEM;

typedef struct tagUAHMEASUREMENUITEM
{
    tagMEASUREITEMSTRUCT mis;
    tagUAHMENU um;
    tagUAHMENUITEM umi;
} UAHMEASUREMENUITEM, *PUAHMEASUREMENUITEM;

struct CBaseRefObj
{
    virtual DWORD AddRef();
    virtual DWORD Release();
};

struct CThemeMenuMetrics : CBaseRefObj
{
    HTHEME hTheme;
    HWND hwndTheme;
    int dpi;
    int iBarBackground;
    int iBarItem;
    int iBarBorderSize;
    MARGINS marBarItem;
    int iPopupBackground;
    int iPopupBorders;
    int iPopupCheck;
    int iPopupCheckBackground;
    int iPopupSubmenu;
    int iPopupGutter;
    int iPopupItem;
    int iPopupSeparator;
    MARGINS marPopupCheckBackground;
    MARGINS marPopupItem;
    MARGINS marPopupItems[4];
    MARGINS marPopupSubmenu;
    MARGINS marPopupOwnerDrawnItem;
    SIZE sizePopupCheck;
    SIZE sizePopupSubmenu;
    SIZE sizePopupSeparator;
    int iPopupBorderSize;
    int iPopupBgBorderSize;
};

struct CThemeMenu : CBaseRefObj
{
    virtual void DrawClientArea(HWND hwnd, HMENU menu, HDC hdc, RECT* prcClip);
    virtual void DrawItem(HWND hwnd, UAHDRAWMENUITEM* pudmi);
    virtual void MeasureItem(UAHMEASUREMENUITEM* pummi, SIZE* psizeItem);
    CThemeMenuMetrics* _spMetrics;
};

struct CThemeMenuBar : CThemeMenu
{
};

using namespace uxtheme;

static std::unordered_map<HTHEME, HTHEME> g_ThemeHandleMap;

#define DECLARE_HOOK_INFO(name) \
    HookTraceInfo<decltype(name)> PFn ## name

union HookHandles
{
    struct TypedHooks
    {
        DECLARE_HOOK_INFO(OpenThemeData);
        DECLARE_HOOK_INFO(OpenThemeDataEx);
        DECLARE_HOOK_INFO(GetThemeAnimationProperty);
        DECLARE_HOOK_INFO(GetThemeAnimationTransform);
        DECLARE_HOOK_INFO(GetThemeBackgroundContentRect);
        DECLARE_HOOK_INFO(GetThemeBackgroundExtent);
        DECLARE_HOOK_INFO(GetThemeBackgroundRegion);
        DECLARE_HOOK_INFO(GetThemeBitmap);
        DECLARE_HOOK_INFO(GetThemeBool);
        DECLARE_HOOK_INFO(GetThemeColor);
        DECLARE_HOOK_INFO(GetThemeEnumValue);
        DECLARE_HOOK_INFO(GetThemeFilename);
        DECLARE_HOOK_INFO(GetThemeFont);
        DECLARE_HOOK_INFO(GetThemeInt);
        DECLARE_HOOK_INFO(GetThemeIntList);
        DECLARE_HOOK_INFO(GetThemeMargins);
        DECLARE_HOOK_INFO(GetThemeMetric);
        DECLARE_HOOK_INFO(GetThemePartSize);
        DECLARE_HOOK_INFO(GetThemePosition);
        DECLARE_HOOK_INFO(GetThemePropertyOrigin);
        DECLARE_HOOK_INFO(GetThemeRect);
        DECLARE_HOOK_INFO(GetThemeStream);
        DECLARE_HOOK_INFO(GetThemeString);
        DECLARE_HOOK_INFO(GetThemeSysBool);
        DECLARE_HOOK_INFO(GetThemeSysColor);
        DECLARE_HOOK_INFO(GetThemeSysColorBrush);
        DECLARE_HOOK_INFO(GetThemeSysFont);
        DECLARE_HOOK_INFO(GetThemeSysSize);
        DECLARE_HOOK_INFO(GetThemeSysString);
        DECLARE_HOOK_INFO(GetThemeTextExtent);
        DECLARE_HOOK_INFO(GetThemeTextMetrics);
        DECLARE_HOOK_INFO(GetThemeTimingFunction);
        DECLARE_HOOK_INFO(GetThemeTransitionDuration);
        DECLARE_HOOK_INFO(IsThemePartDefined);
        DECLARE_HOOK_INFO(IsThemeBackgroundPartiallyTransparent);
        DECLARE_HOOK_INFO(DrawThemeEdge);
        DECLARE_HOOK_INFO(DrawThemeIcon);
        DECLARE_HOOK_INFO(DrawThemeBackground);
        DECLARE_HOOK_INFO(DrawThemeBackgroundEx);
        DECLARE_HOOK_INFO(DrawThemeText);
        DECLARE_HOOK_INFO(DrawThemeTextEx);
        HookTraceInfo<HTHEME(HWND hwnd, wchar_t const* pszClassIdList,
                             unsigned dwFlags, wchar_t const* pszApiName,
                             int iForDPI)> PFnOpenThemeDataExInternal;
        HookTraceInfo<void(CThemeMenuBar*, HWND hwnd, UAHDRAWMENUITEM* pudmi)> PFnCThemeMenuBar_DrawItem;
        DECLARE_HOOK_INFO(GetSysColor);
    } t;
    HOOK_TRACE_INFO handles[sizeof(TypedHooks) / sizeof(HOOK_TRACE_INFO)];
} g_hookHandles;

static_assert(sizeof(HookHandles::handles) == sizeof(HookHandles::t),
              "Hook handle table members have inconsistent size.");

static HTHEME FindThemeHandle(HTHEME hTheme)
{
    if (!g_OverrideTheme)
        return nullptr;

    auto it = g_ThemeHandleMap.find(hTheme);
    if (it == g_ThemeHandleMap.end()) {
        //using OpenThemeDataHookFn = HTHEME WINAPI(HWND hwnd, LPCWSTR pszClassList);
        //auto OpenThemeDataHookOrig = (OpenThemeDataHookFn*)g_hookHandles[0].Link->OldProc;

        //HTHEME hThemeMenu = OpenThemeDataHookOrig(nullptr, L"MENU");
        //if (hThemeMenu == hTheme) {
        //    HTHEME hThemeOverride = UxOpenThemeData(g_OverrideTheme, nullptr, L"MENU");
        //    g_ThemeHandleMap.insert({hTheme, hThemeOverride});
        //    return hThemeOverride;
        //}

        //g_ThemeHandleMap.insert({hTheme, nullptr});
        return nullptr;
    }

    return it->second;
}

static HTHEME WINAPI OpenThemeDataHook(
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList)
{
    if (hwnd && g_OverrideTheme) {
        HTHEME hOrigTheme = OpenThemeData(hwnd, pszClassList);
        HTHEME hTheme = UxOpenThemeData(g_OverrideTheme, hwnd, pszClassList);
        if (hTheme) {
            g_ThemeHandleMap[hOrigTheme] = hTheme;
            return hOrigTheme;
        }
    }

    return OpenThemeData(hwnd, pszClassList);
}

static HTHEME WINAPI OpenThemeDataExHook(
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList,
    _In_ DWORD dwFlags)
{
    if (hwnd && g_OverrideTheme) {
        HTHEME hOrigTheme = OpenThemeData(hwnd, pszClassList);
        HTHEME hTheme = UxOpenThemeDataEx(g_OverrideTheme, hwnd, pszClassList, dwFlags);
        if (hTheme) {
            g_ThemeHandleMap[hOrigTheme] = hTheme;
            return hOrigTheme;
        }
    }

    return OpenThemeDataEx(hwnd, pszClassList, dwFlags);
}

static HRESULT WINAPI CloseThemeDataHook(
    _In_ HTHEME hTheme)
{
    if (g_OverrideTheme)
        return UxCloseThemeData(g_OverrideTheme, hTheme);
    return CloseThemeData(hTheme);
}

static HRESULT WINAPI GetThemeAnimationPropertyHook(
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ TA_PROPERTY eProperty,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) VOID *pvProperty,
    _In_ DWORD cbSize,
    _Out_ DWORD *pcbSizeOut)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeAnimationProperty(
            g_OverrideTheme, hThemeOverride, iStoryboardId, iTargetId,
            eProperty, pvProperty, cbSize, pcbSizeOut);
    return GetThemeAnimationProperty(hTheme, iStoryboardId, iTargetId,
                                     eProperty, pvProperty, cbSize, pcbSizeOut);
}

static HRESULT WINAPI GetThemeAnimationTransformHook(
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ DWORD dwTransformIndex,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TRANSFORM *pTransform,
    _In_ DWORD cbSize,
    _Out_ DWORD *pcbSizeOut)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeAnimationTransform(
            g_OverrideTheme, hThemeOverride, iStoryboardId, iTargetId,
            dwTransformIndex, pTransform, cbSize, pcbSizeOut);
    return GetThemeAnimationTransform(hTheme, iStoryboardId, iTargetId,
                                      dwTransformIndex, pTransform, cbSize,
                                      pcbSizeOut);
}

static HRESULT WINAPI GetThemeBackgroundContentRectHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pBoundingRect,
    _Out_ LPRECT pContentRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeBackgroundContentRect(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId,
            pBoundingRect, pContentRect);
    return GetThemeBackgroundContentRect(hTheme, hdc, iPartId, iStateId,
                                         pBoundingRect, pContentRect);
}

static HRESULT WINAPI GetThemeBackgroundExtentHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pContentRect,
    _Out_ LPRECT pExtentRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeBackgroundExtent(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId,
            pContentRect, pExtentRect);
    return GetThemeBackgroundExtent(hTheme, hdc, iPartId, iStateId,
                                    pContentRect, pExtentRect);
}

static HRESULT WINAPI GetThemeBackgroundRegionHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _Out_ HRGN* pRegion)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeBackgroundRegion(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId,
            pRect, pRegion);
    return GetThemeBackgroundRegion(hTheme, hdc, iPartId, iStateId,
                                    pRect, pRegion);
}

static HRESULT WINAPI GetThemeBitmapHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _In_ ULONG dwFlags,
    _Out_ HBITMAP* phBitmap)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeBitmap(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId,
            dwFlags, phBitmap);
    return GetThemeBitmap(hTheme, iPartId, iStateId, iPropId, dwFlags, phBitmap);
}

static HRESULT WINAPI GetThemeBoolHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ BOOL* pfVal)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeBool(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pfVal);
    return GetThemeBool(hTheme, iPartId, iStateId, iPropId, pfVal);
}

static HRESULT WINAPI GetThemeColorHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ COLORREF* pColor)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeColor(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pColor);
    return GetThemeColor(hTheme, iPartId, iStateId, iPropId, pColor);
}

static HRESULT WINAPI GetThemeEnumValueHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeEnumValue(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, piVal);
    return GetThemeEnumValue(hTheme, iPartId, iStateId, iPropId, piVal);
}

static HRESULT WINAPI GetThemeFilenameHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszThemeFileName,
    _In_ int cchMaxBuffChars)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeFilename(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId,
            pszThemeFileName, cchMaxBuffChars);
    return GetThemeFilename(hTheme, iPartId, iStateId, iPropId,
                            pszThemeFileName, cchMaxBuffChars);
}

static HRESULT WINAPI GetThemeFontHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LOGFONTW* pFont)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeFont(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, iPropId, pFont);
    return GetThemeFont(hTheme, hdc, iPartId, iStateId, iPropId, pFont);
}

static HRESULT WINAPI GetThemeIntHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeInt(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, piVal);
    return GetThemeInt(hTheme, iPartId, iStateId, iPropId, piVal);
}

static HRESULT WINAPI GetThemeIntListHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ INTLIST* pIntList)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeIntList(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pIntList);
    return GetThemeIntList(hTheme, iPartId, iStateId, iPropId, pIntList);
}

static HRESULT WINAPI GetThemeMarginsHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _In_opt_ LPCRECT prc,
    _Out_ MARGINS* pMargins)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeMargins(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, iPropId, prc, pMargins);
    return GetThemeMargins(hTheme, hdc, iPartId, iStateId, iPropId, prc, pMargins);
}

static HRESULT WINAPI GetThemeMetricHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeMetric(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, iPropId, piVal);
    return GetThemeMetric(hTheme, hdc, iPartId, iStateId, iPropId, piVal);
}

static HRESULT WINAPI GetThemePartSizeHook(
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_opt_ LPCRECT prc,
    _In_ enum THEMESIZE eSize,
    _Out_ SIZE* psz)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemePartSize(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, prc, eSize, psz);
    return GetThemePartSize(hTheme, hdc, iPartId, iStateId, prc, eSize, psz);
}

static HRESULT WINAPI GetThemePositionHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ POINT* pPoint)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemePosition(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pPoint);
    return GetThemePosition(hTheme, iPartId, iStateId, iPropId, pPoint);
}

static HRESULT WINAPI GetThemePropertyOriginHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ enum PROPERTYORIGIN* pOrigin)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemePropertyOrigin(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pOrigin);
    return GetThemePropertyOrigin(hTheme, iPartId, iStateId, iPropId, pOrigin);
}

static HRESULT WINAPI GetThemeRectHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LPRECT pRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeRect(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId, pRect);
    return GetThemeRect(hTheme, iPartId, iStateId, iPropId, pRect);
}

static BOOL WINAPI GetThemeSysBoolHook(
    _In_opt_ HTHEME hTheme,
    _In_ int iBoolId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysBool(
            g_OverrideTheme, hThemeOverride, iBoolId);
    return GetThemeSysBool(hTheme, iBoolId);
}

static COLORREF WINAPI GetThemeSysColorHook(
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysColor(
            g_OverrideTheme, hThemeOverride, iColorId);
    return GetThemeSysColor(hTheme, iColorId);
}

static HBRUSH WINAPI GetThemeSysColorBrushHook(
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysColorBrush(
            g_OverrideTheme, hThemeOverride, iColorId);
    return GetThemeSysColorBrush(hTheme, iColorId);
}

static HRESULT WINAPI GetThemeSysFontHook(
    _In_opt_ HTHEME hTheme,
    _In_ int iFontId,
    _Out_ LOGFONTW* plf)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysFont(
            g_OverrideTheme, hThemeOverride, iFontId, plf);
    return GetThemeSysFont(hTheme, iFontId, plf);
}

static HRESULT WINAPI GetThemeSysSizeHook(
    _In_opt_ HTHEME hTheme,
    _In_ int iSizeId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysSize(
            g_OverrideTheme, hThemeOverride, iSizeId);
    return GetThemeSysSize(hTheme, iSizeId);
}

static HRESULT WINAPI GetThemeSysStringHook(
    _In_ HTHEME hTheme,
    _In_ int iStringId,
    _Out_writes_(cchMaxStringChars) LPWSTR pszStringBuff,
    _In_ int cchMaxStringChars)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeSysString(
            g_OverrideTheme, hThemeOverride, iStringId, pszStringBuff,
            cchMaxStringChars);
    return GetThemeSysString(hTheme, iStringId, pszStringBuff,
                             cchMaxStringChars);
}

static HRESULT WINAPI GetThemeStreamHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ VOID** ppvStream,
    _Out_opt_ DWORD* pcbStream,
    _In_opt_ HINSTANCE hInst)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeStream(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId,
            ppvStream, pcbStream, hInst);
    return GetThemeStream(hTheme, iPartId, iStateId, iPropId,
                          ppvStream, pcbStream, hInst);
}

static HRESULT WINAPI GetThemeStringHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszBuff,
    _In_ int cchMaxBuffChars)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeString(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId, iPropId,
            pszBuff, cchMaxBuffChars);
    return GetThemeString(hTheme, iPartId, iStateId, iPropId,
                          pszBuff, cchMaxBuffChars);
}

static HRESULT WINAPI GetThemeTextExtentHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchCharCount) LPCWSTR pszText,
    _In_ int cchCharCount,
    _In_ DWORD dwTextFlags,
    _In_opt_ LPCRECT pBoundingRect,
    _Out_ LPRECT pExtentRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeTextExtent(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, pszText,
            cchCharCount, dwTextFlags, pBoundingRect, pExtentRect);
    return GetThemeTextExtent(hTheme, hdc, iPartId, iStateId, pszText,
                              cchCharCount, dwTextFlags, pBoundingRect, pExtentRect);
}

static HRESULT WINAPI GetThemeTextMetricsHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _Out_ TEXTMETRICW* ptm)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeTextMetrics(
            g_OverrideTheme, hThemeOverride, hdc, iPartId, iStateId, ptm);
    return GetThemeTextMetrics(hTheme, hdc, iPartId, iStateId, ptm);
}

static HRESULT WINAPI GetThemeTimingFunctionHook(
    _In_ HTHEME hTheme,
    _In_ int iTimingFunctionId,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TIMINGFUNCTION* pTimingFunction,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeTimingFunction(
            g_OverrideTheme, hThemeOverride, iTimingFunctionId, pTimingFunction,
            cbSize, pcbSizeOut);
    return GetThemeTimingFunction(hTheme, iTimingFunctionId, pTimingFunction,
                                  cbSize, pcbSizeOut);
}

static BOOL WINAPI GetThemeTransitionDurationHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateIdFrom,
    _In_ int iStateIdTo,
    _In_ int iPropId,
    _Out_ DWORD *pdwDuration)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxGetThemeTransitionDuration(
            g_OverrideTheme, hThemeOverride, iPartId, iStateIdFrom, iStateIdTo,
            iPropId, pdwDuration);
    return GetThemeTransitionDuration(hTheme, iPartId, iStateIdFrom, iStateIdTo,
                                      iPropId, pdwDuration);
}

static BOOL WINAPI IsThemePartDefinedHook(HTHEME hTheme, int iPartId, int iStateId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxIsThemePartDefined(g_OverrideTheme, hThemeOverride, iPartId, iStateId);
    return IsThemePartDefined(hTheme, iPartId, iStateId);
}

static BOOL WINAPI IsThemeBackgroundPartiallyTransparentHook(
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxIsThemeBackgroundPartiallyTransparent(
            g_OverrideTheme, hThemeOverride, iPartId, iStateId);
    return IsThemeBackgroundPartiallyTransparent(hTheme, iPartId, iStateId);
}

static HRESULT WINAPI DrawThemeEdgeHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pDestRect,
    _In_ UINT uEdge,
    _In_ UINT uFlags,
    _Out_opt_ LPRECT pContentRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeEdge(g_OverrideTheme, hThemeOverride, hdc, iPartId,
                               iStateId, pDestRect, uEdge, uFlags, pContentRect);
    return DrawThemeEdge(hTheme, hdc, iPartId, iStateId, pDestRect, uEdge,
                         uFlags, pContentRect);
}

static HRESULT WINAPI DrawThemeIconHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_ HIMAGELIST himl,
    _In_ int iImageIndex)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeIcon(g_OverrideTheme, hThemeOverride, hdc, iPartId,
                               iStateId, pRect, himl, iImageIndex);
    return DrawThemeIcon(hTheme, hdc, iPartId, iStateId, pRect, himl, iImageIndex);
}

static HRESULT WINAPI DrawThemeBackgroundHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_opt_ LPCRECT pClipRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeBackground(g_OverrideTheme, hThemeOverride, hdc,
                                     iPartId, iStateId, pRect, pClipRect);
    return DrawThemeBackground(hTheme, hdc, iPartId, iStateId, pRect, pClipRect);
}

static HRESULT WINAPI DrawThemeBackgroundExHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_opt_ DTBGOPTS const* pOptions)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeBackgroundEx(g_OverrideTheme, hThemeOverride, hdc,
                                       iPartId, iStateId, pRect, pOptions);
    return DrawThemeBackgroundEx(hTheme, hdc, iPartId, iStateId, pRect, pOptions);
}

static HRESULT WINAPI DrawThemeTextHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchText) LPCWSTR pszText,
    _In_ int cchText,
    _In_ DWORD dwTextFlags,
    _In_ DWORD dwTextFlags2,
    _In_ LPCRECT pRect)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeText(g_OverrideTheme, hThemeOverride, hdc, iPartId,
                               iStateId, pszText, cchText, dwTextFlags,
                               dwTextFlags2, pRect);
    return DrawThemeText(hTheme, hdc, iPartId, iStateId, pszText, cchText,
                         dwTextFlags, dwTextFlags2, pRect);
}

static HRESULT WINAPI DrawThemeTextExHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchText) LPCWSTR pszText,
    _In_ int cchText,
    _In_ DWORD dwTextFlags,
    _Inout_ LPRECT pRect,
    _In_opt_ DTTOPTS const* pOptions)
{
    HTHEME hThemeOverride = FindThemeHandle(hTheme);
    if (hThemeOverride)
        return UxDrawThemeTextEx(g_OverrideTheme, hThemeOverride, hdc, iPartId,
                                 iStateId, pszText, cchText, dwTextFlags, pRect,
                                 pOptions);
    return DrawThemeTextEx(hTheme, hdc, iPartId, iStateId, pszText, cchText,
                           dwTextFlags, pRect, pOptions);
}

static HTHEME WINAPI OpenThemeDataExInternalHook(
    HWND hwnd, wchar_t const* pszClassIdList,
    unsigned dwFlags, wchar_t const* pszApiName, int iForDPI)
{
    HTHEME hThemeOrig = g_hookHandles.t.PFnOpenThemeDataExInternal.Orig()(
        hwnd, pszClassIdList, dwFlags, pszApiName, iForDPI);

    if (!g_OverrideTheme)
        return hThemeOrig;

    HTHEME hThemeOverride = OpenThemeDataExInternal(
        g_OverrideTheme, hwnd, pszClassIdList, dwFlags, pszApiName, iForDPI);
    g_ThemeHandleMap[hThemeOrig] = hThemeOverride;
    return hThemeOrig;
}

static BOOL ExGetMenuItemInfo(HMENU hMenu, unsigned int wID, BOOL fByPos, MENUITEMINFOW* lpmii)
{
    if (!GetMenuItemInfoW(hMenu, wID, fByPos, lpmii))
        return FALSE;

    if (lpmii->fMask & MIIM_STRING && lpmii->cch != 0) {
        if (lpmii->dwTypeData[0] == 8) {
            ++lpmii->dwTypeData;
            --lpmii->cch;
        }
    }

    return TRUE;
}

static bool IsOemBitmap(HBITMAP hbmp)
{
    auto val = narrow_cast<DWORD>(reinterpret_cast<uintptr_t>(hbmp));
    return val != 0 && (val <= 3 || val >= 5 && val <= 11);
}

static void CThemeMenuBar_DrawItemHook(CThemeMenuBar* this_, HWND hwnd,
                                       UAHDRAWMENUITEM* pudmi)
{
    std::array<wchar_t, MAX_PATH> textBuffer;

    MENUITEMINFOW mii = {};
    mii.cbSize = sizeof(mii);
    mii.fMask = MIIM_FTYPE | MIIM_BITMAP | MIIM_STRING;
    mii.dwTypeData = textBuffer.data();
    mii.cch = narrow_cast<unsigned>(textBuffer.size());

    if (!ExGetMenuItemInfo(pudmi->um.hmenu, pudmi->umi.iPosition, TRUE, &mii))
        return;

    if (mii.fType & MIIM_FTYPE) {
        //CThemeMenu::DrawOwnerDrawnItem(this_, hwnd, 0, pudmi, fForGlyph, ptsInfo);
        return;
    }

    RECT dstRects[2] = {};
    {
        bool hasBar = false;
        long itemHeight = pudmi->dis.rcItem.bottom - pudmi->dis.rcItem.top;
        int dx = pudmi->dis.rcItem.left + this_->_spMetrics->marBarItem.cxLeftWidth;
        for (int i = 0; i < 2; ++i) {
            SIZE const& barSize = pudmi->umi.umim.rgsizeBar[i];
            RECT& rect = dstRects[i];

            if (barSize.cx != 0 && barSize.cy != 0) {
                rect.right = barSize.cx;
                rect.bottom = barSize.cy;

                if (!hasBar)
                    hasBar = true;
                else
                    dx += this_->_spMetrics->iBarBorderSize;
                auto dy = (itemHeight - barSize.cy) / 2 + pudmi->dis.rcItem.top;
                OffsetRect(&rect, dx, dy);
                dx += barSize.cx;
            }
        }

        if (!hasBar) {
            dstRects[1].bottom = itemHeight;
            dstRects[1].right = pudmi->dis.rcItem.right - pudmi->dis.rcItem.left;
        }
    }

    int iPartId = this_->_spMetrics->iBarItem;
    int iStateId;
    if (pudmi->dis.itemState & (ODS_INACTIVE | ODS_DISABLED)) {
        if (pudmi->dis.itemState & ODS_HOTLIGHT)
            iStateId = MBI_DISABLEDHOT;
        else if (pudmi->dis.itemState & ODS_SELECTED)
            iStateId = MBI_DISABLEDPUSHED;
        else
            iStateId = MBI_DISABLED;
    } else if (pudmi->dis.itemState & ODS_HOTLIGHT)
        iStateId = MBI_HOT;
    else if (pudmi->dis.itemState & ODS_SELECTED)
        iStateId = MBI_PUSHED;
    else
        iStateId = MBI_NORMAL;

    if (IsThemeBackgroundPartiallyTransparent(this_->_spMetrics->hTheme, iPartId, iStateId)) {
        this_->DrawClientArea(hwnd, pudmi->um.hmenu,
                              pudmi->dis.hDC, &pudmi->dis.rcItem);
    }

    if (!mii.hbmpItem || !IsOemBitmap(mii.hbmpItem)) {
        DrawThemeBackground(this_->_spMetrics->hTheme, pudmi->dis.hDC,
                            iPartId, iStateId, &pudmi->dis.rcItem,
                            nullptr);
    }

    if (mii.hbmpItem) {
        //this_->DrawItemBitmap(hwnda, pudmi->dis.hDC, mii.hbmpItem, 0, iStateId, ...);
    }

    if (mii.cch != 0) {
        int textFlags = DT_SINGLELINE;
        if (pudmi->dis.itemState & ODS_NOACCEL)
            textFlags |= DT_HIDEPREFIX;

        DrawThemeText(this_->_spMetrics->hTheme, pudmi->dis.hDC, iPartId,
                      iStateId, mii.dwTypeData, mii.cch, textFlags, 0, &dstRects[1]);
    }
}

#define ADD_HOOK(name) \
    st = LhInstallHook(GetProcAddress(uxtheme, #name), \
                       name##Hook, nullptr, &g_hookHandles.t.PFn##name); \
    if (FAILED(st)) \
        return E_FAIL;

#define ADD_HOOK2(name, addr) \
    st = LhInstallHook(addr, \
                       name##Hook, nullptr, &g_hookHandles.t.PFn##name); \
    if (FAILED(st)) \
        return E_FAIL;

THEMEEXAPI UxOverrideTheme(_In_ HTHEMEFILE hThemeFile)
{
    if (hThemeFile && !ThemeFileSlotFromHandle(hThemeFile))
        return E_HANDLE;

    g_OverrideTheme = hThemeFile;
    UxBroadcastThemeChange();
    return S_OK;
}

extern "C" DWORD WINAPI GetSysColorHook(int nIndex)
{
    return GetSysColorEx(nIndex);
}

THEMEEXAPI UxHook()
{
    for (HOOK_TRACE_INFO& hHook : g_hookHandles.handles) {
        LhUninstallHook(&hHook);
        hHook = {};
    }

    LhWaitForPendingRemovals();

    HMODULE uxtheme = GetModuleHandleW(L"uxtheme");
    HMODULE user32 = GetModuleHandleW(L"user32");

    NTSTATUS st;

    ADD_HOOK(OpenThemeData);
    ADD_HOOK(OpenThemeDataEx);
    //ADD_HOOK(CloseThemeData);
    ADD_HOOK2(OpenThemeDataExInternal, (void*)((uintptr_t)uxtheme + 0x171B16F34 - 0x171B00000));
    ADD_HOOK2(CThemeMenuBar_DrawItem, (void*)((uintptr_t)uxtheme + 0x171B106D0 - 0x171B00000));
    ADD_HOOK(GetThemeAnimationProperty);
    ADD_HOOK(GetThemeAnimationTransform);
    ADD_HOOK(GetThemeBackgroundContentRect);
    ADD_HOOK(GetThemeBackgroundExtent);
    ADD_HOOK(GetThemeBackgroundRegion);
    ADD_HOOK(GetThemeBitmap);
    ADD_HOOK(GetThemeBool);
    ADD_HOOK(GetThemeColor);
    ADD_HOOK(GetThemeEnumValue);
    ADD_HOOK(GetThemeFilename);
    ADD_HOOK(GetThemeFont);
    ADD_HOOK(GetThemeInt);
    ADD_HOOK(GetThemeIntList);
    ADD_HOOK(GetThemeMargins);
    ADD_HOOK(GetThemeMetric);
    ADD_HOOK(GetThemePartSize);
    ADD_HOOK(GetThemePosition);
    ADD_HOOK(GetThemePropertyOrigin);
    ADD_HOOK(GetThemeRect);
    ADD_HOOK(GetThemeStream);
    ADD_HOOK(GetThemeString);
    ADD_HOOK(GetThemeSysBool);
    ADD_HOOK(GetThemeSysColor);
    ADD_HOOK(GetThemeSysColorBrush);
    ADD_HOOK(GetThemeSysFont);
    ADD_HOOK(GetThemeSysSize);
    ADD_HOOK(GetThemeSysString);
    ADD_HOOK(GetThemeTextExtent);
    ADD_HOOK(GetThemeTextMetrics);
    ADD_HOOK(GetThemeTimingFunction);
    ADD_HOOK(GetThemeTransitionDuration);
    ADD_HOOK(IsThemePartDefined);
    ADD_HOOK(IsThemeBackgroundPartiallyTransparent);
    ADD_HOOK(DrawThemeEdge);
    ADD_HOOK(DrawThemeIcon);
    ADD_HOOK(DrawThemeBackground);
    ADD_HOOK(DrawThemeBackgroundEx);
    ADD_HOOK(DrawThemeText);
    ADD_HOOK(DrawThemeTextEx);
    ADD_HOOK2(GetSysColor, GetProcAddress(user32, "GetSysColor"));

    ULONG aclEntries[1] = {0};
    for (HOOK_TRACE_INFO& hHook : g_hookHandles.handles)
        LhSetInclusiveACL(aclEntries, 1, &hHook);

    UxBroadcastThemeChange();
    return S_OK;
}

THEMEEXAPI UxUnhook()
{
    for (HOOK_TRACE_INFO& hHook : g_hookHandles.handles) {
        LhUninstallHook(&hHook);
        hHook = {};
    }

    LhWaitForPendingRemovals();
    UxBroadcastThemeChange();
    return S_OK;
}

THEMEEXAPI_(void) UxBroadcastThemeChange()
{
    SendThemeChangedProcessLocal();
}
