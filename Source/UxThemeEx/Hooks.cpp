#include "UxThemeEx.h"

#include "Global.h"

#include <map>
#include <easyhook.h>

using namespace uxtheme;

static std::map<HTHEME, HTHEME> g_ThemeHandleMap;

static HRESULT MatchThemeClass(
    wchar_t const* pszClassId, CUxThemeFile* pThemeFile, int* piOffset,
    int* piAppNameOffset, int* piClassNameOffset)
{
    // FIXME
    *piAppNameOffset = 0;

    auto liveClasses = (APPCLASSLIVE*)((char *)pThemeFile->_pbSharableData + pThemeFile->_pbSharableData->iSectionIndexOffset);
    int cClasses = pThemeFile->_pbSharableData->iSectionIndexLength / sizeof(APPCLASSLIVE);
    if (!cClasses)
        return 0x80070490;

    for (int i = 0; i < cClasses; ++i) {
        auto const& liveClass = liveClasses[i];
        auto const& classInfo = liveClass.AppClassInfo;

        if (classInfo.iAppNameIndex || !classInfo.iClassNameIndex)
            continue;

        auto currClassName = reinterpret_cast<wchar_t const*>(
            Advance(pThemeFile->_pbSharableData, classInfo.iClassNameIndex));

        if ((pszClassId && currClassName && AsciiStrCmpI(pszClassId, currClassName) == 0)
            || (!pszClassId && !currClassName)) {
            *piOffset = liveClass.iIndex;
            *piClassNameOffset = classInfo.iClassNameIndex;
            return S_OK;
        }
    }

    return 0x80070490;
}

static HTHEME OpenThemeDataExInternal(
    HWND hwnd, wchar_t const* pszClassIdList, unsigned dwFlags,
    wchar_t const* pszApiName, int iForDPI)
{
    if (hwnd && !IsWindow(hwnd)) {
        SetLastError(0x80070006);
        return nullptr;
    }

    if (!pszClassIdList) {
        SetLastError(E_POINTER);
        return nullptr;
    }

    auto themeFile = ThemeFileFromHandle(g_OverrideTheme);
    if (!themeFile)
        return nullptr;

    int themeOffset;
    int appNameOffset;
    int classNameOffset;
    if (FAILED(MatchThemeClass(pszClassIdList, themeFile, &themeOffset, &appNameOffset, &classNameOffset)))
        return nullptr;

    HTHEME hTheme;
    if (FAILED(g_pRenderList.OpenRenderObject(
        themeFile, themeOffset, appNameOffset, classNameOffset, nullptr, nullptr,
        hwnd, 0, dwFlags, false, &hTheme)))
        return nullptr;

    return hTheme;
}

static HTHEME WINAPI OpenThemeDataHook(
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList)
{
    if (hwnd && g_OverrideTheme) {
        HTHEME hOrigTheme = OpenThemeData(hwnd, pszClassList);
        HTHEME hTheme = OpenThemeDataExInternal(hwnd, pszClassList, 0, nullptr, 0);
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
        HTHEME hTheme = OpenThemeDataExInternal(hwnd, pszClassList, dwFlags, nullptr, 0);
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
        return g_pRenderList.CloseRenderObject(hTheme);

    return CloseThemeData(hTheme);
}

static HRESULT WINAPI DrawThemeBackgroundHook(
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_opt_ LPCRECT pClipRect)
{
    auto it = g_ThemeHandleMap.find(hTheme);
    if (g_OverrideTheme && it != g_ThemeHandleMap.end()) //IsThemeHandle(hTheme))
        return UxDrawThemeBackground(g_OverrideTheme, it->second, hdc, iPartId,
                                     iStateId, pRect, pClipRect);
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
    auto it = g_ThemeHandleMap.find(hTheme);
    if (g_OverrideTheme && it != g_ThemeHandleMap.end())
        return UxDrawThemeBackgroundEx(g_OverrideTheme, it->second, hdc, iPartId,
                                       iStateId, pRect, pOptions);
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
    auto it = g_ThemeHandleMap.find(hTheme);
    if (false && g_OverrideTheme && it != g_ThemeHandleMap.end()) // FIXME
        return UxDrawThemeText(g_OverrideTheme, hTheme, hdc, iPartId, iStateId,
                               pszText, cchText, dwTextFlags, dwTextFlags2,
                               pRect);
    return DrawThemeText(hTheme, hdc, iPartId, iStateId, pszText, cchText,
                         dwTextFlags, dwTextFlags2, pRect);
}

static HOOK_TRACE_INFO g_hookHandles[10] = {};

THEMEEXAPI UxOverrideTheme(_In_ wchar_t const* themeFileName)
{
    HTHEMEFILE hThemeFile;
    ENSURE_HR(UxOpenThemeFile(themeFileName, &hThemeFile));

    if (g_OverrideTheme)
        if (FAILED(UxCloseThemeFile(g_OverrideTheme)))
            return E_FAIL;

    g_OverrideTheme = hThemeFile;
    return S_OK;
}

THEMEEXAPI UxHook()
{
    for (HOOK_TRACE_INFO& hHook : g_hookHandles) {
        LhUninstallHook(&hHook);
        hHook = {};
    }

    HMODULE uxtheme = GetModuleHandleW(L"uxtheme");

    NTSTATUS st = 0;
    int idx = 0;

    st = LhInstallHook(GetProcAddress(uxtheme, "OpenThemeData"),
                       OpenThemeDataHook, nullptr, &g_hookHandles[idx++]);
    if (FAILED(st))
        return E_FAIL;

    st = LhInstallHook(GetProcAddress(uxtheme, "OpenThemeDataEx"),
                       OpenThemeDataExHook, nullptr, &g_hookHandles[idx++]);
    if (FAILED(st))
        return E_FAIL;

    //st = LhInstallHook(GetProcAddress(uxtheme, "CloseThemeData"),
    //                   CloseThemeDataHook, nullptr, &g_hookHandles[idx++]);
    //if (FAILED(st))
    //    return E_FAIL;

    st = LhInstallHook(GetProcAddress(uxtheme, "DrawThemeBackground"),
                       DrawThemeBackgroundHook, nullptr, &g_hookHandles[idx++]);
    if (FAILED(st))
        return E_FAIL;

    st = LhInstallHook(GetProcAddress(uxtheme, "DrawThemeBackgroundEx"),
                       DrawThemeBackgroundExHook, nullptr, &g_hookHandles[idx++]);
    if (FAILED(st))
        return E_FAIL;

    st = LhInstallHook(GetProcAddress(uxtheme, "DrawThemeText"),
                       DrawThemeTextHook, nullptr, &g_hookHandles[idx++]);
    if (FAILED(st))
        return E_FAIL;

    ULONG aclEntries[1] = {0};
    for (HOOK_TRACE_INFO& hHook : g_hookHandles)
        LhSetInclusiveACL(aclEntries, 1, &hHook);

    SendThemeChangedProcessLocal();
    return S_OK;
}

THEMEEXAPI UxUnhook()
{
    for (HOOK_TRACE_INFO& hHook : g_hookHandles) {
        LhUninstallHook(&hHook);
        hHook = {};
    }

    LhWaitForPendingRemovals();
    SendThemeChangedProcessLocal();
    return S_OK;
}
