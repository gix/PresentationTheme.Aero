#include "Global.h"
#include "ThemeLoader.h"
#include "RenderObj.h"
#include "BorderFill.h"
#include "TextDraw.h"
#include "RenderList.h"
#include "UxThemeEx.h"
#include "Utils.h"

#include <mutex>
#include <map>

using namespace uxtheme;

#if !defined(BUILD_UXTHEMEEX)
#define THEMEEXAPI          EXTERN_C DECLSPEC_IMPORT HRESULT STDAPICALLTYPE
#define THEMEEXAPI_(type)   EXTERN_C DECLSPEC_IMPORT type STDAPICALLTYPE
#else
#define THEMEEXAPI          STDAPI
#define THEMEEXAPI_(type)   STDAPI_(type)
#endif

static HRESULT LoadThemeLibrary(wchar_t const* pszThemePath, HMODULE* phInst, int* pnVersion)
{
    if (pnVersion)
        *pnVersion = 0;

    ModuleHandle module{LoadLibraryExW(pszThemePath, nullptr, LOAD_LIBRARY_AS_DATAFILE)};
    if (!module)
        return MakeErrorLast();

    void* data;
    unsigned len;
    HRESULT hr = GetPtrToResource(
        module, L"PACKTHEM_VERSION", MAKEINTRESOURCEW(1), &data, &len);

    if (SUCCEEDED(hr) && len == 2) {
        short version = *static_cast<short*>(data);
        if (pnVersion)
            *pnVersion = version;

        if (version == 4) {
            *phInst = module.Detach();
            return S_OK;
        }
    }

    return 0x8007000B;
}

struct CThemeApiHelper
{
    wchar_t const* _pszFuncName = nullptr;
    int _iRenderSlotNum = -1;
    int _iEntryValue = -1;

    ~CThemeApiHelper() { CloseHandle(); }

    HRESULT OpenHandle(HTHEMEFILE hThemeFile, HTHEME hTheme, CRenderObj** pRender);
    void CloseHandle();
};

HRESULT CThemeApiHelper::OpenHandle(
    HTHEMEFILE hThemeFile, HTHEME hTheme, CRenderObj** pRender)
{
    auto themeFile = ThemeFileFromHandle(hThemeFile);
    if (!themeFile)
        return E_HANDLE;

    ENSURE_HR(g_pRenderList.OpenThemeHandle(hTheme, pRender, &_iRenderSlotNum));

    return S_OK;
}

void CThemeApiHelper::CloseHandle()
{
    if (_iRenderSlotNum > -1)
        g_pRenderList.CloseThemeHandle(_iRenderSlotNum);
}

THEMEEXAPI UxOpenThemeFile(
    _In_ wchar_t const* themeFileName,
    _Out_ HTHEMEFILE* phThemeFile)
{
    HMODULE module;
    ENSURE_HR(LoadThemeLibrary(themeFileName, &module, nullptr));

    HANDLE reuseSection = nullptr;

    CThemeLoader loader;
    ENSURE_HR(loader.LoadTheme(module, themeFileName, &reuseSection, TRUE));

    std::unique_ptr<CUxThemeFile> themeFile(new CUxThemeFile(std::move(loader._LoadingThemeFile)));

    ThemeFileEntry entry{Handle(reuseSection), std::move(themeFile)};

    if (g_ThemeFileHandles.empty())
        g_ThemeFileHandles.resize(1);

    size_t i = 1;
    for (; i < g_ThemeFileHandles.size(); ++i) {
        if (!g_ThemeFileHandles[i].ThemeFile) {
            g_ThemeFileHandles[i] = std::move(entry);
            break;
        }
    }

    if (i == g_ThemeFileHandles.size())
        g_ThemeFileHandles.push_back(std::move(entry));
    *phThemeFile = reinterpret_cast<HTHEMEFILE>(static_cast<uintptr_t>(i));

    return S_OK;
}

THEMEEXAPI UxCloseThemeFile(_In_ HTHEMEFILE hThemeFile)
{
    auto slot = ThemeFileSlotFromHandle(hThemeFile);
    if (!slot)
        return E_HANDLE;

    auto& themeFile = *g_ThemeFileHandles[slot].ThemeFile;
    auto nonSharableDataHdr = (NONSHARABLEDATAHDR*)themeFile._pbNonSharableData;

    g_pRenderList.FreeRenderObjects(nonSharableDataHdr->iLoadId);
    g_ThemeFileHandles[slot] = ThemeFileEntry();
    g_OverrideTheme = nullptr;

    return S_OK;
}

THEMEEXAPI UxGetThemeAnimationProperty(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ TA_PROPERTY eProperty,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) VOID* pvProperty,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeAnimationTransform(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ DWORD dwTransformIndex,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TRANSFORM* pTransform,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeBackgroundContentRect(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pBoundingRect,
    _Out_ LPRECT pContentRect)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeBackgroundExtent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pContentRect,
    _Out_ LPRECT pExtentRect)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeBackgroundRegion(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _Out_ HRGN* pRegion)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeBitmap(
    _In_ HTHEMEFILE hThemeFile, _In_ HTHEME hTheme, int iPartId,
    int iStateId, int iPropId, ULONG dwFlags, _Out_ HBITMAP* phBitmap)
{
    if (!phBitmap)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CDrawBase* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType)
        return E_FAIL;

    ENSURE_HR(static_cast<CImageFile*>(partObj)->GetBitmap(
        renderObj, iPropId, dwFlags, phBitmap));
    return S_OK;
}

THEMEEXAPI UxGetThemeBool(
    _In_ HTHEMEFILE hThemeFile, _In_ HTHEME hTheme, int iPartId, int iStateId,
    int iPropId, _Out_ BOOL* pfVal)
{
    if (!pfVal)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetBool(iPartId, iStateId, iPropId, pfVal));
    return S_OK;
}

THEMEEXAPI UxGetThemeColor(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ COLORREF* pColor)
{
    if (!pColor)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetInt(iPartId, iStateId, iPropId, (int*)pColor));
    return S_OK;
}

THEMEEXAPI UxGetThemeDocumentationProperty(
    _In_ HTHEMEFILE hThemeFile,
    _In_ LPCWSTR pszThemeName,
    _In_ LPCWSTR pszPropertyName,
    _Out_writes_(cchMaxValChars) LPWSTR pszValueBuff,
    _In_ int cchMaxValChars)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeEnumValue(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    if (!piVal)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetEnumValue(iPartId, iStateId, iPropId, piVal));
    return S_OK;
}

THEMEEXAPI UxGetThemeFilename(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszThemeFileName,
    _In_ int cchMaxBuffChars)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeFont(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LOGFONTW* pFont)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeInt(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    if (!piVal)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetInt(iPartId, iStateId, iPropId, piVal));
    return S_OK;
}

THEMEEXAPI UxGetThemeIntList(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ INTLIST* pIntList)
{
    if (!pIntList)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetIntList(iPartId, iStateId, iPropId, pIntList));
    return S_OK;
}

THEMEEXAPI UxGetThemeMargins(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _In_opt_ LPCRECT prc,
    _Out_ MARGINS* pMargins)
{
    if (!pMargins)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetMargins(hdc, iPartId, iStateId, iPropId,
                                            prc, pMargins));
    return S_OK;
}

THEMEEXAPI UxGetThemeMetric(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal)
{
    if (!piVal)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetMetric(hdc, iPartId, iStateId, iPropId, piVal));
    return S_OK;
}

THEMEEXAPI UxGetThemePartSize(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_opt_ LPCRECT prc,
    _In_ enum THEMESIZE eSize,
    _Out_ SIZE* psz)
{
    if (!psz)
        return E_POINTER;

    HRESULT hr;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));

    CDrawBase* partObj;
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == BT_BORDERFILL)
        return static_cast<CBorderFill*>(partObj)->GetPartSize(eSize, psz);
    else
        return static_cast<CImageFile*>(partObj)->GetPartSize(
            renderObj, hdc, prc, eSize, psz);
}

THEMEEXAPI UxGetThemePosition(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ POINT* pPoint)
{
    if (!pPoint)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetPosition(iPartId, iStateId, iPropId, pPoint));
    return S_OK;
}

THEMEEXAPI UxGetThemePropertyOrigin(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ enum PROPERTYORIGIN* pOrigin)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeRect(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LPRECT pRect)
{
    if (!pRect)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetRect(iPartId, iStateId, iPropId, pRect));
    return S_OK;
}

THEMEEXAPI UxGetThemeStream(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ VOID** ppvStream,
    _Out_opt_ DWORD* pcbStream,
    _In_opt_ HINSTANCE hInst)
{
    if (!ppvStream)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->ExternalGetStream(iPartId, iStateId, iPropId, ppvStream, pcbStream, hInst));
    return S_OK;
}

THEMEEXAPI UxGetThemeString(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszBuff,
    _In_ int cchMaxBuffChars)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI_(BOOL) UxGetThemeSysBool(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iBoolId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return FALSE;
}

THEMEEXAPI_(COLORREF) UxGetThemeSysColor(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return 0;
}

THEMEEXAPI_(HBRUSH) UxGetThemeSysColorBrush(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return nullptr;
}

THEMEEXAPI UxGetThemeSysFont(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iFontId,
    _Out_ LOGFONTW* plf)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeSysInt(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iIntId,
    _Out_ int* piValue)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI_(int) UxGetThemeSysSize(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iSizeId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return 0;
}

THEMEEXAPI UxGetThemeSysString(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStringId,
    _Out_writes_(cchMaxStringChars) LPWSTR pszStringBuff,
    _In_ int cchMaxStringChars)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeTextExtent(
    _In_ HTHEMEFILE hThemeFile,
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
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeTextMetrics(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _Out_ TEXTMETRICW* ptm)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeTimingFunction(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iTimingFunctionId,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TIMINGFUNCTION* pTimingFunction,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxGetThemeTransitionDuration(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateIdFrom,
    _In_ int iStateIdTo,
    _In_ int iPropId,
    _Out_ DWORD* pdwDuration)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI_(BOOL) UxIsThemePartDefined(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return FALSE;
}

THEMEEXAPI_(BOOL) UxIsThemeBackgroundPartiallyTransparent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId)
{
    SetLastError(E_NOTIMPL); //FIXME
    return FALSE;
}

THEMEEXAPI UxHitTestThemeBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ DWORD dwOptions,
    _In_ LPCRECT pRect,
    _In_opt_ HRGN hrgn,
    _In_ POINT ptTest,
    _Out_ WORD* pwHitTestCode)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxDrawThemeEdge(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pDestRect,
    _In_ UINT uEdge,
    _In_ UINT uFlags,
    _Out_opt_ LPRECT pContentRect)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxDrawThemeIcon(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_ HIMAGELIST himl,
    _In_ int iImageIndex)
{
    return TRACE_HR(E_NOTIMPL); //FIXME
}

THEMEEXAPI UxDrawThemeBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_opt_ LPCRECT pClipRect)
{
    if (!hdc)
        return E_HANDLE;
    if (!pRect)
        return E_POINTER;

    CThemeApiHelper helper;

    CRenderObj* pRender;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &pRender));

    CDrawBase* partObj;
    ENSURE_HR(pRender->GetPartObject(iPartId, iStateId, &partObj));

    DTBGOPTS options = {};
    if (pClipRect) {
        options.dwFlags |= DTBG_CLIPRECT;
        options.rcClip = *pClipRect;
    }

    if (partObj->_eBgType == 1)
        return static_cast<CBorderFill*>(partObj)->DrawBackground(
            pRender, hdc, pRect, &options);

    return static_cast<CImageFile*>(partObj)->DrawBackground(
        pRender, hdc, iStateId, pRect, &options);
}

THEMEEXAPI UxDrawThemeBackgroundEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_ DTBGOPTS const* pOptions)
{
    CThemeApiHelper helper;
    CRenderObj* pRender;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &pRender));

    if (!hdc)
        return E_HANDLE;
    if (!pRect)
        return E_POINTER;
    if (pOptions && pOptions->dwSize != 24)
        return E_FAIL;

    CDrawBase* partObj;
    ENSURE_HR(pRender->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == 1)
        return static_cast<CBorderFill*>(partObj)->DrawBackground(
            pRender, hdc, pRect, pOptions);

    return static_cast<CImageFile*>(partObj)->DrawBackground(
        pRender, hdc, iStateId, pRect, pOptions);
}

THEMEEXAPI UxDrawThemeParentBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HWND hwnd,
    _In_ HDC hdc,
    _In_opt_ RECT const* prc)
{

}

THEMEEXAPI UxDrawThemeParentBackgroundEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HWND hwnd,
    _In_ HDC hdc,
    _In_ DWORD dwFlags,
    _In_opt_ RECT const* prc)
{
    HWND hwndParent; // rbp@3
    signed int v9; // er15@4
    HRGN v10; // rax@4
    HRGN v11; // rsi@4
    int v12; // eax@5
    HWND v13; // rcx@12
    char v14; // al@16
    HRESULT v15; // edi@17
    DWORD v16; // eax@18
    UINT v18; // edi@28
    HGDIOBJ v19; // r13@28
    char v20; // r14@28
    HGDIOBJ v21; // rax@32
    HGDIOBJ v22; // r13@32
    UINT v23; // eax@32
    int v24; // er14@32
    int v25; // edi@32
    DWORD v26; // eax@32
    char v27; // di@35
    char v28; // al@36
    char v29; // cl@36
    signed int v30; // [sp+30h] [bp-98h]@3
    char top; // [sp+34h] [bp-94h]@15
    int topa; // [sp+34h] [bp-94h]@32
    UINT flags; // [sp+38h] [bp-90h]@13
    int flagsa; // [sp+38h] [bp-90h]@32
    DWORD v35; // [sp+3Ch] [bp-8Ch]@8
    tagPOINT point; // [sp+40h] [bp-88h]@15
    HGDIOBJ h; // [sp+48h] [bp-80h]@28
    HGDIOBJ v38; // [sp+50h] [bp-78h]@32
    tagRECT Rect; // [sp+58h] [bp-70h]@12
    tagRECT rect; // [sp+68h] [bp-60h]@28
    tagRECT v41; // [sp+78h] [bp-50h]@46

    if (!IsWindow(hwnd) || !hdc)
        return E_HANDLE;

    v30 = 0;
    hwndParent = GetParent(hwnd);
    if ((unsigned __int64)GetPropW(hwndParent, (LPCWSTR)0xA915) & 1)
        return 1;

    v9 = 0;
    v10 = CreateRectRgn(0, 0, 1, 1);
    v11 = v10;
    if (v10 && (v12 = GetClipRgn(hdc, v10), v12 != -1))
    {
        if (!v12)
        {
            DeleteObject(v11);
            v11 = 0i64;
        }
        v9 = 1;
        v35 = 0;
    } else
    {
        v16 = MakeErrorLast();
        v35 = v16;
        if ((v16 & 0x80000000) != 0)
        {
            if (dwFlags || prc)
            {
                v15 = v16;
            LABEL_19:
                if ((v16 & 0x80000000) == 0 && v9)
                    SelectClipRgn(hdc, v11);
                if (v11)
                    DeleteObject(v11);
                return v15;
            }
        LABEL_11:
            if (dwFlags & 1)
            {
                GetWindowRect(hwnd, &Rect);
                v13 = 0i64;
            } else
            {
                GetClientRect(hwnd, &Rect);
                v13 = hwnd;
            }
            MapWindowPoints(v13, hwndParent, (LPPOINT)&Rect, 2u);
            SetPropW(hwndParent, (LPCWSTR)0xA915, (HANDLE)1);
            flags = 0;
            if (dwFlags & 4)
                flags = SetBoundsRect(hdc, 0i64, 5u);
            GetViewportOrgEx(hdc, &point);
            SetViewportOrgEx(hdc, point.x - Rect.left, point.y - Rect.top, &point);
            SendMessageW(hwndParent, WM_ERASEBKGND, (WPARAM)hdc, 0i64);
            SendMessageW(hwndParent, WM_PRINTCLIENT, (WPARAM)hdc, 4i64);
            top = 1;
            if (dwFlags & 4)
            {
                v28 = GetBoundsRect(hdc, &rect, 0);
                v29 = 1;
                if ((v28 & 3) == 1)
                    v29 = 0;
                top = v29;
                SetBoundsRect(hdc, 0i64, flags);
            }
            SetViewportOrgEx(hdc, point.x, point.y, 0i64);
            v14 = (unsigned __int64)GetPropW(hwndParent, (LPCWSTR)0xA915);
            if (!(v14 & 2))
                goto LABEL_17;
            v15 = 1;
            v30 = 1;
            if ((v14 & 4) == 0 && (dwFlags & 4) != 0 && top)
            {
                v15 = 0;
                goto LABEL_18;
            }
            if (!(dwFlags & 2))
            {
            LABEL_18:
                RemovePropW(hwndParent, (LPCWSTR)0xA915);
                v16 = v35;
                goto LABEL_19;
            }
            v18 = SetBoundsRect(hdc, 0i64, 5u);
            h = (HGDIOBJ)SendMessageW(hwndParent, WM_CTLCOLORSTATIC, (WPARAM)hdc, (LPARAM)hwnd);
            v19 = h;
            v20 = 1;
            if ((GetBoundsRect(hdc, &rect, 0) & 3) == 1)
                v20 = 0;
            SetBoundsRect(hdc, 0i64, v18);
            if (!v19)
            {
                v15 = 1;
                if (v20)
                    v15 = 0;
                goto LABEL_18;
            }
            if (!prc)
            {
                if (!GetClipBox(hdc, &v41))
                {
                LABEL_17:
                    v15 = v30;
                    goto LABEL_18;
                }
                prc = &v41;
            }
            v21 = GetStockObject(8);
            v22 = SelectObject(hdc, v21);
            v38 = SelectObject(hdc, h);
            v23 = SetBoundsRect(hdc, 0i64, 5u);
            v24 = prc->left;
            v25 = prc->right + 1;
            flagsa = prc->bottom + 1;
            topa = prc->top;
            v26 = GetLayout(hdc);
            if (v26 != -1 && v26 & 1)
            {
                --v24;
                --v25;
            }
            Rectangle(hdc, v24, topa, v25, flagsa);
            v27 = GetBoundsRect(hdc, &rect, 0);
            SetBoundsRect(hdc, 0i64, (UINT)v23);
            v15 = (v27 & 3) == 1;
            SelectObject(hdc, v38);
            SelectObject(hdc, v22);
            goto LABEL_18;
        }
    }
    if (prc)
        IntersectClipRect(hdc, prc->left, prc->top, prc->right, prc->bottom);
    goto LABEL_11;
}

THEMEEXAPI UxDrawThemeText(
    _In_ HTHEMEFILE hThemeFile,
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
    if (!hdc)
        return E_HANDLE;

    if (cchText == -1) {
        if (!pszText)
            return E_POINTER;
    } else if (!pszText && 2 * cchText) {
        return E_POINTER;
    }

    if (!pRect)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));

    CTextDraw* partObj;
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));
    ENSURE_HR(partObj->DrawTextW(renderObj, hdc, iPartId, iStateId, pszText,
                                 cchText, dwTextFlags, pRect, nullptr));

    return S_OK;
}

THEMEEXAPI UxDrawThemeTextEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchText) LPCWSTR pszText,
    _In_ int cchText,
    _In_ DWORD dwTextFlags,
    _Inout_ LPRECT pRect,
    _In_opt_ DTTOPTS const* pOptions);
