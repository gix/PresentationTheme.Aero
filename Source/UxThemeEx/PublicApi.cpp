#include "UxThemeEx.h"

#include "BorderFill.h"
#include "Global.h"
#include "RenderList.h"
#include "RenderObj.h"
#include "TextDraw.h"
#include "ThemeLoader.h"
#include "Utils.h"

#include <map>
#include <mutex>
#include <CommCtrl.h>
#include <CommonControls.h>
#include <strsafe.h>
#include "ScalingUtil.h"

using namespace uxtheme;

namespace uxtheme
{

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

static void FillRectClr(HDC hdc, RECT const* prc, unsigned clr)
{
    COLORREF oldBkColor = SetBkColor(hdc, clr);
    ExtTextOutW(hdc, 0, 0, ETO_OPAQUE, prc, nullptr, 0, nullptr);
    SetBkColor(hdc, oldBkColor);
}

static HRESULT _DrawEdge(HDC hdc, RECT const* pDestRect, unsigned uEdge,
                         unsigned uFlags, unsigned clrLight,
                         unsigned clrHighlight, unsigned clrShadow,
                         unsigned clrDkShadow, unsigned clrFill,
                         RECT* pContentRect)
{
    if (!hdc || !pDestRect)
        return  E_INVALIDARG;

    int xBorder = GetSystemMetrics(SM_CXBORDER);
    int yBorder = GetSystemMetrics(SM_CYBORDER);
    if (uFlags & 0x8000)
        uFlags |= BF_FLAT;

    RECT rcDst;
    CopyRect(&rcDst, pDestRect);

    unsigned uEdge2 = uEdge;

    unsigned v16 = uEdge & BDR_OUTER;
    if (!(uEdge & BDR_OUTER))
        goto LABEL_18;

    while (1) {
        unsigned clrTopLeft;
        unsigned clrBottomRight;

        if (uFlags & BF_FLAT) {
            if (uFlags & BF_MONO) {
                clrBottomRight = clrHighlight;
                if (v16 & 3)
                    clrBottomRight = clrDkShadow;
            } else {
                clrBottomRight = clrFill;
                if (v16 & 3)
                    clrBottomRight = clrShadow;
            }
            clrTopLeft = clrBottomRight;
        } else {
            if (v16 == 1) {
                clrTopLeft = clrLight;
                clrBottomRight = clrDkShadow;
                if (uFlags & 0x1000)
                    clrTopLeft = clrHighlight;
            } else if (v16 == 2) {
                clrTopLeft = clrShadow;
                clrBottomRight = clrHighlight;
                if (uFlags & BF_SOFT)
                    clrTopLeft = clrDkShadow;
            } else if (v16 == 4) {
                clrTopLeft = clrHighlight;
                clrBottomRight = clrShadow;
                if (uFlags & BF_SOFT)
                    clrTopLeft = clrLight;
            } else if (v16 == 8) {
                clrTopLeft = clrDkShadow;
                clrBottomRight = clrLight;
                if (uFlags & BF_SOFT)
                    clrTopLeft = clrShadow;
            } else
                return E_INVALIDARG;
        }

        if (uFlags & BF_RIGHT) {
            rcDst.right -= xBorder;
            RECT rc;
            rc.left = rcDst.right;
            rc.right = rcDst.right + xBorder;
            rc.top = rcDst.top;
            rc.bottom = rcDst.bottom;
            FillRectClr(hdc, &rc, clrBottomRight);
        }
        if (uFlags & BF_BOTTOM) {
            rcDst.bottom -= yBorder;
            RECT rc;
            rc.left = rcDst.left;
            rc.right = rcDst.right;
            rc.top = rcDst.bottom;
            rc.bottom = rcDst.bottom + yBorder;
            FillRectClr(hdc, &rc, clrBottomRight);
        }
        if (uFlags & BF_LEFT) {
            rcDst.left += xBorder;
            RECT rc;
            rc.left = rcDst.left - xBorder;
            rc.right = rcDst.left;
            rc.top = rcDst.top;
            rc.bottom = rcDst.bottom;
            FillRectClr(hdc, &rc, clrTopLeft);
        }
        if (uFlags & BF_TOP) {
            rcDst.top += yBorder;
            RECT rc;
            rc.left = rcDst.left;
            rc.right = rcDst.right;
            rc.top = rcDst.top - yBorder;
            rc.bottom = rcDst.top;
            FillRectClr(hdc, &rc, clrTopLeft);
        }

    LABEL_18:
        v16 = uEdge2 & BDR_INNER;
        if (!(uEdge2 & BDR_INNER))
            break;
        uEdge2 &= ~BDR_INNER;
    }

    if (uFlags & BF_MIDDLE) {
        if (uFlags & BF_MONO)
            clrFill = clrHighlight;
        FillRectClr(hdc, &rcDst, clrFill);
    }

    if (uFlags & BF_ADJUST) {
        if (pContentRect)
            CopyRect(pContentRect, &rcDst);
    }

    return S_OK;
}

static wchar_t const g_pszAppName[] = {0};

static wchar_t const* ThemeString(CUxThemeFile* pThemeFile, int iOffset)
{
    if (pThemeFile && pThemeFile->_pbSharableData && iOffset > 0)
        return (wchar_t const*)((char*)pThemeFile->_pbSharableData + iOffset);
    return g_pszAppName;
}

static HRESULT MatchThemeClass(
    wchar_t const* pszClassId, CUxThemeFile* pThemeFile, int* piOffset,
    int* piClassNameOffset)
{
    auto liveClasses = (APPCLASSLIVE*)((char *)pThemeFile->_pbSharableData + pThemeFile->_pbSharableData->iSectionIndexOffset);
    int numClasses = pThemeFile->_pbSharableData->iSectionIndexLength / sizeof(APPCLASSLIVE);
    if (!numClasses)
        return 0x80070490;

    for (int i = 0; i < numClasses; ++i) {
        auto const& liveClass = liveClasses[i];
        auto const& classInfo = liveClass.AppClassInfo;

        if (classInfo.iAppNameIndex != 0 || classInfo.iClassNameIndex == 0)
            continue;

        auto currClassName = ThemeString(pThemeFile, classInfo.iClassNameIndex);

        if ((pszClassId && currClassName && AsciiStrCmpI(pszClassId, currClassName) == 0)
            || (!pszClassId && !currClassName)) {
            *piOffset = liveClass.iIndex;
            *piClassNameOffset = classInfo.iClassNameIndex;
            return S_OK;
        }
    }

    return 0x80070490;
}

static HRESULT MatchThemeApp(
    wchar_t const* pszAppName, wchar_t const* pszClassId, CUxThemeFile* pThemeFile,
    int* piOffset, int* piAppNameOffset, int* piClassNameOffset, int fAllowInheritance)
{
    auto liveClasses = (APPCLASSLIVE*)((char *)pThemeFile->_pbSharableData + pThemeFile->_pbSharableData->iSectionIndexOffset);
    int numClasses = pThemeFile->_pbSharableData->iSectionIndexLength / sizeof(APPCLASSLIVE);

    for (int i = 0; i < numClasses; ++i) {
        auto const& liveClass = liveClasses[i];
        auto const& classInfo = liveClass.AppClassInfo;

        if (classInfo.iAppNameIndex == 0 || classInfo.iClassNameIndex == 0)
            continue;

        wchar_t const* currAppName = ThemeString(pThemeFile, classInfo.iAppNameIndex);
        if (!pszAppName || !currAppName || AsciiStrCmpI(pszAppName, currAppName) != 0)
            continue;

        auto currClassName = ThemeString(pThemeFile, classInfo.iClassNameIndex);
        if (AsciiStrCmpI(pszClassId, currClassName) == 0) {
            *piOffset = liveClass.iIndex;
            *piAppNameOffset = classInfo.iAppNameIndex;
            *piClassNameOffset = classInfo.iClassNameIndex;
            return S_OK;
        }
    }

    if (!fAllowInheritance)
        return 0x80070490;

    *piAppNameOffset = 0;
    return MatchThemeClass(pszClassId, pThemeFile, piOffset, piClassNameOffset);
}

static ATOM const SubIdListProp = 0xA910;
static ATOM const SubAppNameProp = 0xA911;
static ATOM const UnkProp_A912 = 0xA912;
static ATOM const WindowThemeProp = 0xA914;
static ATOM const UnkProp_A915 = 0xA915;

static bool GetStringProp(HWND hwnd, ATOM prop, wchar_t* buffer, size_t bufferSize)
{
    auto handle = GetPropW(hwnd, reinterpret_cast<LPCWSTR>(prop));
    if (!handle)
        return false;

    auto atom = narrow_cast<ATOM>(reinterpret_cast<uintptr_t>(handle));
    return handle && GetAtomNameW(atom, buffer, bufferSize);
}

static HRESULT MatchThemeClassList(
    HWND hwnd, wchar_t const* pszClassIdList, CUxThemeFile* pThemeFile,
    int* piOffset, int* piAppNameOffset, int* piClassNameOffset)
{
    if (!pszClassIdList)
        return E_INVALIDARG;

    wchar_t appNameBuffer[260];
    wchar_t subIdListBuffer[260];

    wchar_t* pszAppName = nullptr;
    if (hwnd) {
        if (GetStringProp(hwnd, SubIdListProp, subIdListBuffer, 260))
            pszClassIdList = subIdListBuffer;
        if (GetStringProp(hwnd, SubAppNameProp, appNameBuffer, 260))
            pszAppName = appNameBuffer;
    }

    size_t cchDest = lstrlenW(pszClassIdList) + 1;
    auto localClassIdList = make_unique_nothrow<wchar_t[]>(cchDest);
    if (!localClassIdList)
        return E_OUTOFMEMORY;

    HRESULT hr = StringCchCopyNW(localClassIdList.get(), cchDest, pszClassIdList, STRSAFE_MAX_LENGTH);
    if (hr < 0)
        return hr;

    for (wchar_t* classId = localClassIdList.get();; classId += lstrlenW(classId) + 1) {
        bool hasMoreClasses = false;
        for (wchar_t* c = classId; *c; c = CharNextW(c)) {
            if (*c == L';') {
                *c = 0;
                hasMoreClasses = true;
                break;
            }
        }

        if (!(pszAppName && *pszAppName) && *classId) {
            for (wchar_t* c = classId; *c; ++c) {
                if (c[0] == L':' && c[1] == L':') {
                    if (StringCchCopyNW(appNameBuffer, 260, classId, c - classId) >= 0) {
                        pszAppName = appNameBuffer;
                        classId = c + 2;
                        break;
                    }
                }
            }
        }

        if (pszAppName && *pszAppName) {
            hr = MatchThemeApp(pszAppName, classId, pThemeFile, piOffset,
                               piAppNameOffset, piClassNameOffset, TRUE);
        } else {
            *piAppNameOffset = 0;
            hr = MatchThemeClass(classId, pThemeFile, piOffset, piClassNameOffset);
        }

        if (hr >= 0 || !hasMoreClasses)
            break;
    }

    return hr;
}

HTHEME OpenThemeDataExInternal(
    HTHEMEFILE hThemeFile, HWND hwnd, wchar_t const* pszClassIdList,
    unsigned dwFlags, wchar_t const* pszApiName, int iForDPI)
{
    if (hwnd && !IsWindow(hwnd)) {
        SetLastError(0x80070006);
        return nullptr;
    }

    if (!pszClassIdList) {
        SetLastError((DWORD)E_POINTER);
        return nullptr;
    }

    auto themeFile = ThemeFileFromHandle(hThemeFile);
    if (!themeFile)
        return nullptr;

    int themeOffset;
    int appNameOffset;
    int classNameOffset;
    HRESULT hr = MatchThemeClassList(hwnd, pszClassIdList, themeFile, &themeOffset,
                                     &appNameOffset, &classNameOffset);
    if (FAILED(hr)) {
        SetLastError(hr);
        return nullptr;
    }

    HTHEME hTheme;
    if (FAILED(g_pRenderList.OpenRenderObject(
        themeFile, themeOffset, appNameOffset, classNameOffset, nullptr, nullptr,
        hwnd, 0, dwFlags, false, &hTheme)))
        return nullptr;

    if (!(dwFlags & OTD_NONCLIENT) && hwnd)
        SetPropW(hwnd, (LPCWSTR)UnkProp_A912, hTheme);

    return hTheme;
}

struct CThemeApiHelper
{
    ~CThemeApiHelper() { CloseHandle(); }

    HRESULT OpenHandle(HTHEMEFILE hThemeFile, HTHEME hTheme, CRenderObj** pRender);
    void CloseHandle();

    wchar_t const* _pszFuncName = nullptr;
    int _iRenderSlotNum = -1;
    int _iEntryValue = -1;
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

} // namespace uxtheme

struct LOADTHEMEFORTESTPARAMS
{
    unsigned int cbSize;
    const wchar_t *pszThemeName;
    const wchar_t *pszColorParam;
    const wchar_t *pszSizeParam;
    unsigned __int16 wDesiredLangID;
    int iDesiredDpi;
    int rgConnectedDpis[7];
    int fEmulateGlobal;
    int fForceHighContrast;
    CUxThemeFile *pLoadedUxThemeFile;
};

THEMEEXAPI UxOpenThemeFile(
    _In_ wchar_t const* themeFileName,
    _Out_ HTHEMEFILE* phThemeFile)
{
    HMODULE module;
    ENSURE_HR(LoadThemeLibrary(themeFileName, &module, nullptr));

    wchar_t const* colorParam = L"NormalColor";
    wchar_t const* sizeParam = L"NormalSize";

    LOADTHEMEFORTESTPARAMS tp = {};
    tp.cbSize = sizeof(tp);
    tp.pszThemeName = themeFileName;
    tp.pszColorParam = colorParam;
    tp.pszSizeParam = sizeParam;
    auto LoaderLoadThemeForTesting = (HRESULT(__stdcall*)(LOADTHEMEFORTESTPARAMS*))GetProcAddress(
        GetModuleHandleW(L"uxtheme"), (LPCSTR)127);
    HRESULT hr = LoaderLoadThemeForTesting(&tp);

    CThemeLoader loader;

    FileMappingHandle reuseSection;
    ENSURE_HR(loader.LoadTheme(module, themeFileName, colorParam, sizeParam,
                               reuseSection.CloseAndGetAddressOf(), TRUE));

    std::unique_ptr<CUxThemeFile> themeFile(new CUxThemeFile(std::move(loader._LoadingThemeFile)));

    ThemeFileEntry entry{std::move(reuseSection), std::move(themeFile)};

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
    if (!slot || !g_ThemeFileHandles[slot].ThemeFile)
        return E_HANDLE;

    auto& themeFile = *g_ThemeFileHandles[slot].ThemeFile;
    auto nonSharableDataHdr = (NONSHARABLEDATAHDR*)themeFile._pbNonSharableData;

    g_pRenderList.FreeRenderObjects(nonSharableDataHdr->iLoadId);
    g_ThemeFileHandles[slot] = ThemeFileEntry();
    return S_OK;
}

THEMEEXAPI_(HTHEME)
UxOpenThemeData(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList)
{
    return OpenThemeDataExInternal(hThemeFile, hwnd, pszClassList, 0, nullptr, FALSE);
}

THEMEEXAPI_(HTHEME)
UxOpenThemeDataEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList,
    _In_ DWORD dwFlags)
{
    return OpenThemeDataExInternal(hThemeFile, hwnd, pszClassList, dwFlags, nullptr, FALSE);
}

THEMEEXAPI UxCloseThemeData(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme)
{
    return g_pRenderList.CloseRenderObject(hTheme);
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
    if (!pBoundingRect || !pContentRect)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CDrawBase* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == BT_BORDERFILL)
        ENSURE_HR(static_cast<CBorderFill*>(partObj)->GetBackgroundExtent(
            renderObj, pBoundingRect, pContentRect));
    else
        ENSURE_HR(static_cast<CImageFile*>(partObj)->GetBackgroundContentRect(
            renderObj, hdc, pBoundingRect, pContentRect));
    return S_OK;
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
    if (!pContentRect || !pExtentRect)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CDrawBase* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == BT_BORDERFILL)
        ENSURE_HR(static_cast<CBorderFill*>(partObj)->GetBackgroundExtent(
            renderObj, pContentRect, pExtentRect));
    else
        ENSURE_HR(static_cast<CImageFile*>(partObj)->GetBackgroundExtent(
            renderObj, hdc, pContentRect, pExtentRect));
    return S_OK;
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

    if (partObj->_eBgType != BT_IMAGEFILE)
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetBool(iPartId, iStateId, iPropId, pfVal));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetInt(iPartId, iStateId, iPropId, (int*)pColor));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetEnumValue(iPartId, iStateId, iPropId, piVal));
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
    if (!pszThemeFileName)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetString(iPartId, iStateId, iPropId,
                                           pszThemeFileName, cchMaxBuffChars));
    return S_OK;
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
    if (!pFont)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetFont(hdc, iPartId, iStateId, iPropId,
                                         TRUE, pFont));
    return S_OK;
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetInt(iPartId, iStateId, iPropId, piVal));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetIntList(iPartId, iStateId, iPropId, pIntList));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetMargins(hdc, iPartId, iStateId, iPropId,
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetMetric(hdc, iPartId, iStateId, iPropId, piVal));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetPosition(iPartId, iStateId, iPropId, pPoint));
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
    if (!pOrigin)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPropertyOrigin(iPartId, iStateId, iPropId, pOrigin));
    return S_OK;
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetRect(iPartId, iStateId, iPropId, pRect));
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
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetStream(iPartId, iStateId, iPropId,
                                           ppvStream, pcbStream, hInst));
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
    if (!pszBuff)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR_EX(0x80070490, renderObj->ExternalGetString(iPartId, iStateId, iPropId, pszBuff,
                                           cchMaxBuffChars));
    return S_OK;
}

THEMEEXAPI_(BOOL) UxGetThemeSysBool(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iBoolId)
{
    SetLastError((DWORD)E_NOTIMPL); //FIXME
    return FALSE;
}

THEMEEXAPI_(COLORREF) UxGetThemeSysColor(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    SetLastError((DWORD)E_NOTIMPL); //FIXME
    return 0;
}

THEMEEXAPI_(HBRUSH) UxGetThemeSysColorBrush(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId)
{
    SetLastError((DWORD)E_NOTIMPL); //FIXME
    return nullptr;
}

THEMEEXAPI UxGetThemeSysFont(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iFontId,
    _Out_ LOGFONTW* plf)
{
    if (!plf)
        return E_POINTER;

    CRenderObj* renderObj = nullptr;
    CThemeApiHelper helper;
    if (hTheme)
        ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));

    if (iFontId < TMT_FIRSTFONT || iFontId > TMT_LASTFONT)
        return E_INVALIDARG;

    if (renderObj) {
        *plf = *renderObj->GetFont(iFontId - TMT_FIRSTFONT);
        ScaleFontForScreenDpi(plf);
        return S_OK;
    }

    if (iFontId == TMT_ICONTITLEFONT) {
        if (!SystemParametersInfoW(SPI_GETICONTITLELOGFONT, sizeof(LOGFONTW), plf, 0))
            return MakeErrorLast();
        return S_OK;
    }

    NONCLIENTMETRICS ncmetrics = {};
    ncmetrics.cbSize = sizeof(ncmetrics);
    if (!SystemParametersInfoW(SPI_GETNONCLIENTMETRICS, sizeof(ncmetrics), &ncmetrics, 0))
        return MakeErrorLast();

    switch (iFontId) {
    case TMT_CAPTIONFONT:
        *plf = ncmetrics.lfCaptionFont;
        break;
    case TMT_SMALLCAPTIONFONT:
        *plf = ncmetrics.lfSmCaptionFont;
        break;
    case TMT_MENUFONT:
        *plf = ncmetrics.lfMenuFont;
        break;
    case TMT_STATUSFONT:
        *plf = ncmetrics.lfStatusFont;
        break;
    case TMT_MSGBOXFONT:
        *plf = ncmetrics.lfMessageFont;
        break;
    case TMT_ICONTITLEFONT:
        break;
    default:
        assert("FRE: FALSE");
        return E_FAIL;
    }

    return S_OK;
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
    SetLastError((DWORD)E_NOTIMPL); //FIXME
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
    if (!pExtentRect)
        return E_POINTER;
    if (!pszText && cchCharCount != 0)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CTextDraw* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));
    ENSURE_HR(partObj->GetTextExtent(renderObj, hdc, iPartId, iStateId, pszText,
                                     cchCharCount, dwTextFlags, pBoundingRect,
                                     pExtentRect));
    return S_OK;
}

THEMEEXAPI UxGetThemeTextMetrics(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _Out_ TEXTMETRICW* ptm)
{
    if (!ptm)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CTextDraw* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));
    ENSURE_HR(partObj->GetTextMetricsW(renderObj, hdc, iPartId, iStateId, ptm));
    return S_OK;
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
    if (!pdwDuration)
        return E_POINTER;

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetTransitionDuration(iPartId, iStateIdFrom, iStateIdTo,
                                               iPropId, pdwDuration));
    return S_OK;
}

THEMEEXAPI_(BOOL) UxIsThemePartDefined(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId)
{
    CThemeApiHelper helper;
    CRenderObj* pRender;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &pRender));
    return pRender->IsPartDefined(iPartId, iStateId) ? TRUE : FALSE;
}

THEMEEXAPI_(BOOL) UxIsThemeBackgroundPartiallyTransparent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId)
{
    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CDrawBase* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == BT_BORDERFILL) {
        auto borderFill = static_cast<CBorderFill*>(partObj);
        return borderFill->IsBackgroundPartiallyTransparent() ? TRUE : FALSE;
    } else {
        auto imageFile = static_cast<CImageFile*>(partObj);
        return imageFile->_ImageInfo.fPartiallyTransparent;
    }
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
    CThemeApiHelper helper;
    CRenderObj* renderObj;
    CDrawBase* partObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));

    if (partObj->_eBgType == BT_BORDERFILL) {
        auto borderFill = static_cast<CBorderFill*>(partObj);
        return borderFill->HitTestBackground(
            renderObj,
            iStateId,
            dwOptions,
            pRect,
            hrgn,
            ptTest,
            pwHitTestCode);
    } else {
        auto imageFile = static_cast<CImageFile*>(partObj);
        return imageFile->HitTestBackground(
            renderObj,
            hdc,
            iStateId,
            dwOptions,
            pRect,
            hrgn,
            ptTest,
            pwHitTestCode);
    }
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
    CThemeApiHelper helper;
    CRenderObj* pRender;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &pRender));

    CTextDraw* partObj;
    ENSURE_HR(pRender->GetPartObject(iPartId, iStateId, &partObj));

    ENSURE_HR(_DrawEdge(
        hdc,
        pDestRect,
        uEdge,
        uFlags,
        partObj->_crEdgeLight,
        partObj->_crEdgeHighlight,
        partObj->_crEdgeShadow,
        partObj->_crEdgeDkShadow,
        partObj->_crEdgeFill,
        pContentRect));

    return S_OK;
}

static HRESULT DrawThemeIconEx(
    HTHEMEFILE hThemeFile,
    HTHEME hTheme,
    HDC hdc,
    int iPartId,
    int iStateId,
    RECT const* pRect,
    HIMAGELIST himl,
    int iImageIndex,
    IMAGELISTDRAWPARAMS* pParams)
{
    if (!hdc || !himl)
        return E_HANDLE;

    IMAGELISTDRAWPARAMS params = {};
    params.cbSize = sizeof(params);

    IMAGELISTDRAWPARAMS* pImageListParams = pParams;

    if (!pParams) {
        params.fStyle = ILD_TRANSPARENT;
        params.rgbBk = CLR_NONE;
        params.rgbFg = CLR_NONE;
        pImageListParams = &params;
        if (!pRect)
            return E_INVALIDARG;
    }

    CThemeApiHelper helper;
    CRenderObj* renderObj;
    ENSURE_HR(helper.OpenHandle(hThemeFile, hTheme, &renderObj));

    pImageListParams->himl = himl;
    pImageListParams->hdcDst = hdc;
    pImageListParams->i = iImageIndex;

    if (pRect) {
        pImageListParams->x = pRect->left;
        pImageListParams->y = pRect->top;
        pImageListParams->cx = pRect->right - pRect->left;
        pImageListParams->cy = pRect->bottom - pRect->top;
    }

    int iconEffect;
    if (FAILED(renderObj->ExternalGetEnumValue(iPartId, iStateId, TMT_ICONEFFECT, &iconEffect)))
        iconEffect = 0;

    if (iconEffect == 1) {
        int glowColor;
        if (FAILED(renderObj->ExternalGetInt(iPartId, iStateId, TMT_GLOWCOLOR, &glowColor)))
            glowColor = 0xFF0000;
        pImageListParams->fState |= ILS_GLOW;
        pImageListParams->crEffect = glowColor;
    } else if (iconEffect == 2) {
        int shadowColor;
        if (FAILED(renderObj->ExternalGetInt(iPartId, iStateId, TMT_SHADOWCOLOR, &shadowColor)))
            shadowColor = 0;
        pImageListParams->fState |= ILS_SHADOW;
        pImageListParams->crEffect = shadowColor;
    } else if (iconEffect == 3) {
        int saturation;
        if (FAILED(renderObj->ExternalGetInt(iPartId, iStateId, TMT_SATURATION, &saturation)))
            saturation = 128;
        pImageListParams->fState |= ILS_SATURATE;
        pImageListParams->Frame = saturation;
    } else if (iconEffect == 4) {
        int alpha;
        if (FAILED(renderObj->ExternalGetInt(iPartId, iStateId, TMT_ALPHALEVEL, &alpha)))
            alpha = 128;
        pImageListParams->fState |= ILS_ALPHA;
        pImageListParams->Frame = alpha;
    }

    IImageList* imageList = nullptr;
    HRESULT hr = HIMAGELIST_QueryInterface(
        pImageListParams->himl, IID_IImageList, (void**)&imageList);
    if (hr >= 0) {
        hr = imageList->Draw(pImageListParams);
        imageList->Release();
    }

    return hr;
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
    IMAGELISTDRAWPARAMS params = {};
    params.cbSize = sizeof(params);
    params.i = iImageIndex;
    params.x = pRect->left;
    params.cx = pRect->right - pRect->left;
    params.y = pRect->top;
    params.cy = pRect->bottom - pRect->top;
    params.hdcDst = hdc;
    params.rgbBk = CLR_NONE;
    params.rgbFg = CLR_NONE;
    params.himl = himl;
    params.fStyle = ILD_TRANSPARENT;

    CThemeApiHelper helper;
    ENSURE_HR(DrawThemeIconEx(hThemeFile, hTheme, hdc, iPartId, iStateId, pRect,
                              himl, iImageIndex, &params));
    return S_OK;
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
    DTBGOPTS* pOptions = nullptr;
    if (pClipRect) {
        options.dwFlags |= DTBG_CLIPRECT;
        options.rcClip = *pClipRect;
        pOptions = &options;
    }

    if (partObj->_eBgType == BT_BORDERFILL)
        return static_cast<CBorderFill*>(partObj)->DrawBackground(
            pRender, hdc, pRect, pOptions);

    return static_cast<CImageFile*>(partObj)->DrawBackground(
        pRender, hdc, iStateId, pRect, pOptions);
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
    return UxDrawThemeParentBackgroundEx(hThemeFile, hwnd, hdc, 0, prc);
}

THEMEEXAPI UxDrawThemeParentBackgroundEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HWND hwnd,
    _In_ HDC hdc,
    _In_ DWORD dwFlags,
    _In_opt_ RECT const* prc)
{
    HWND hwndParent;
    int v9;
    HRGN v10;
    HRGN v11;
    int v12;
    HWND v13;
    char v14;
    HRESULT v15;
    DWORD v16;
    UINT v18;
    HGDIOBJ v19;
    char v20;
    HGDIOBJ v21;
    HGDIOBJ v22;
    UINT v23;
    int v24;
    int v25;
    DWORD v26;
    int v30;
    char top;
    int topa;
    UINT flags;
    int flagsa;
    DWORD v35;
    POINT point;
    HGDIOBJ h;
    HGDIOBJ v38;
    RECT Rect;
    RECT rect;
    RECT v41;

    if (!IsWindow(hwnd) || !hdc)
        return E_HANDLE;

    v30 = 0;
    hwndParent = GetParent(hwnd);
    if ((uintptr_t)GetPropW(hwndParent, (LPCWSTR)UnkProp_A915) & 1)
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
            SetPropW(hwndParent, (LPCWSTR)UnkProp_A915, (HANDLE)1);
            flags = 0;
            if (dwFlags & 4)
                flags = SetBoundsRect(hdc, 0i64, 5u);
            GetViewportOrgEx(hdc, &point);
            SetViewportOrgEx(hdc, point.x - Rect.left, point.y - Rect.top, &point);
            SendMessageW(hwndParent, WM_ERASEBKGND, (WPARAM)hdc, 0i64);
            SendMessageW(hwndParent, WM_PRINTCLIENT, (WPARAM)hdc, 4i64);
            top = 1;
            if (dwFlags & 4) {
                UINT v28 = GetBoundsRect(hdc, &rect, 0);
                top = (v28 & (DCB_ACCUMULATE | DCB_RESET)) != DCB_RESET;
                SetBoundsRect(hdc, 0i64, flags);
            }
            SetViewportOrgEx(hdc, point.x, point.y, 0i64);
            v14 = (uintptr_t)GetPropW(hwndParent, (LPCWSTR)UnkProp_A915);
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
                RemovePropW(hwndParent, (LPCWSTR)UnkProp_A915);
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
            v15 = (GetBoundsRect(hdc, &rect, 0) & 3) == 1;
            SetBoundsRect(hdc, 0i64, (UINT)v23);
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

    RECT rect = *pRect;
    CTextDraw* partObj;
    ENSURE_HR(renderObj->GetPartObject(iPartId, iStateId, &partObj));
    ENSURE_HR(partObj->DrawTextW(hThemeFile, renderObj, hdc, iPartId, iStateId,
                                 pszText, cchText, dwTextFlags, &rect, nullptr));

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
    _In_opt_ DTTOPTS const* pOptions)
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
    ENSURE_HR(partObj->DrawTextW(hThemeFile, renderObj, hdc, iPartId, iStateId,
                                 pszText, cchText, dwTextFlags, pRect, pOptions));

    return S_OK;
}
