#include "RenderObj.h"
#include "UxThemeFile.h"
#include "Utils.h"
#include "DpiInfo.h"
#include <strsafe.h>
#include "UxThemeHelpers.h"
#include "Debug.h"

namespace uxtheme
{

static HRESULT _BitmapFromDib(HDC hdc, void* const pvDibBits, HBITMAP* phBitmap)
{
    HRESULT  hr = 0;
    HDC windowDC = nullptr;

    if (!hdc) {
        windowDC = GetWindowDC(nullptr);
        if (!windowDC)
            return MakeErrorLast();

        hdc = windowDC;
    }

    auto bitmapInfo = (BITMAPINFO const*)pvDibBits;

    void* pvBits;
    HBITMAP bitmap = CreateDIBSection(hdc, bitmapInfo, DIB_RGB_COLORS, &pvBits, nullptr, 0);

    char* srcBits = (char*)bitmapInfo + bitmapInfo->bmiHeader.biSize + 4 * bitmapInfo->bmiHeader.biClrUsed;
    if (bitmap && SetDIBits(hdc, bitmap, 0, bitmapInfo->bmiHeader.biHeight,
                            srcBits, bitmapInfo, DIB_RGB_COLORS)) {
        *phBitmap = bitmap;
    } else {
        hr = MakeErrorLast();
    }

    if (windowDC)
        ReleaseDC(nullptr, windowDC);

    if (hr < 0 && bitmap)
        DeleteObject(bitmap);
    return hr;
}

int ScaleThemeSize(HDC hdc, _In_ CRenderObj const* pRender, int iValue)
{
    int dpi;
    if (!hdc || (pRender->_fIsStronglyAssociatedDpi && IsScreenDC(hdc)))
        dpi = pRender->_iAssociatedDpi;
    else
        dpi = GetDeviceCaps(hdc, LOGPIXELSX);

    return MulDiv(iValue, dpi, 96);
}

void ScaleThemeFont(HDC hdc, _In_ CRenderObj const* pRender, _In_ LOGFONTW* plf)
{
    if (plf->lfHeight < 0)
        plf->lfHeight = ScaleThemeSize(hdc, pRender, plf->lfHeight);
}

void ScaleFontForScreenDpi(_In_ LOGFONTW* plf)
{
    if (plf->lfHeight < 0)
        plf->lfHeight = MulDiv(plf->lfHeight, GetScreenDpi(), 96);
}

CRenderObj::CRenderObj(CUxThemeFile* pThemeFile, int iCacheSlot, int iThemeOffset,
                       int iClassNameOffset, __int64 iUniqueId, bool fEnableCache,
                       int iTargetDpi, bool fIsStronglyAssociatedDpi, unsigned dwOtdFlags)
{
    _fCacheEnabled = fEnableCache;
    _dwOtdFlags = dwOtdFlags;
    _fCloseThemeFile = 0;
    _fPartCacheInitialized = 0;
    _fDiagnosticModeEnabled = 0;
    _pDiagnosticXML = nullptr;
    if (pThemeFile) {
        //v14 = g_pAppInfo ? CAppInfo::BumpRefCount(g_pAppInfo, pThemeFile) : -2147467259;
        //if (v14 >= 0)
        _fCloseThemeFile = true;
    }

    _iUniqueId = iUniqueId;
    _iAssociatedDpi = iTargetDpi;
    _fIsStronglyAssociatedDpi = fIsStronglyAssociatedDpi;
    _pThemeFile = pThemeFile;
    _iCacheSlot = iCacheSlot;
    if (pThemeFile) {
        _pbSharableData = (char*)pThemeFile->_pbSharableData;
        _pbSectionData = _pbSharableData + iThemeOffset;
        _ptm = (THEMEMETRICS *)(_pbSharableData + 8 + pThemeFile->_pbSharableData->iSysMetricsOffset);

        _pbNonSharableData = pThemeFile->_pbNonSharableData;

        auto hdr = (NONSHARABLEDATAHDR*)pThemeFile->_pbNonSharableData;
        _phBitmapsArray = (HBITMAP64 *)(_pbNonSharableData + hdr->iBitmapsOffset);
    } else {
        _pbSharableData = nullptr;
        _pbSectionData = nullptr;
        _ptm = nullptr;
        _pbNonSharableData = nullptr;
        _phBitmapsArray = nullptr;
    }

    _pszClassName = nullptr; // &pszAppName;
    if (pThemeFile && pThemeFile->_pbSharableData && iClassNameOffset > 0) {
        _pszClassName = (const wchar_t *)((char*)pThemeFile->_pbSharableData + iClassNameOffset);
    }

    _pPngDecoder = nullptr;
    _pPngEncoder = nullptr;
    _pCacheObj = nullptr;
}

CRenderObj::~CRenderObj()
{
}


HRESULT CRenderObj::Create(
    CUxThemeFile* pThemeFile, int iCacheSlot, int iThemeOffset,
    int iAppNameOffset, int iClassNameOffset, __int64 iUniqueId,
    bool fEnableCache, CDrawBase* pBaseObj, CTextDraw* pTextObj, int iTargetDpi,
    bool fIsStronglyAssociatedDpi, unsigned dwOtdFlags, CRenderObj** ppObj)
{
    *ppObj = nullptr;

    auto obj = make_unique_nothrow<CRenderObj>(
        pThemeFile,
        iCacheSlot,
        iThemeOffset,
        iClassNameOffset,
        iUniqueId,
        fEnableCache,
        iTargetDpi,
        fIsStronglyAssociatedDpi,
        dwOtdFlags);
    if (!obj)
        return E_OUTOFMEMORY;

    //if (pBaseObj || pTextObj)
    //    ENSURE_HR(obj->Init_TESTONLY(pBaseObj, pTextObj));
    //else
    //    ENSURE_HR(obj->Init((const wchar_t *)pBaseObj, 0i64, v16));

    *ppObj = obj.release();
    return S_OK;
}

void CRenderObj::GetEffectiveDpi(HDC hdc, int* px, int* py)
{
    if (hdc && !IsScreenDC(hdc)) {
        *px = GetDeviceCaps(hdc, LOGPIXELSX);
        if (py)
            *py = GetDeviceCaps(hdc, LOGPIXELSY);
    } else {
        *px = _iAssociatedDpi;
        if (py)
            *py = _iAssociatedDpi;
    }
}

TMBITMAPHEADER* CRenderObj::GetBitmapHeader(int iDibOffset) const
{
    if (_pbSharableData && iDibOffset)
        return (TMBITMAPHEADER*)(_pbSharableData + iDibOffset);
    return nullptr;
}

bool CRenderObj::GetFontTableIndex(int iPartId, int iStateId, int iPropId,
                                   unsigned short* pFontIndex) const
{
    if (!iPropId)
        assert("FRE: iPropId");

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx <= 0) {
        *pFontIndex = 0;
        return false;
    }

    *pFontIndex = *reinterpret_cast<unsigned short*>(&_pbSharableData[idx]);
    return true;
}

CRenderCache* CRenderObj::GetCacheObject()
{
    std::lock_guard<std::mutex> lock(_lock);
    if (!_pCacheObj)
        _pCacheObj = std::make_unique<CRenderCache>(this);
    return _pCacheObj.get();
}

CRenderCache::CRenderCache(CRenderObj* pRenderObj)
    : _pRenderObj(pRenderObj)
    , _plfFont(nullptr)
{
    memset(&_lfDisplayFont, 0, sizeof(_lfDisplayFont));
}


HRESULT CRenderCache::GetDisplayFontHandle(
    HDC hdc, LOGFONTW const& lf, HFONT* phFont)
{
    if (!memcmp(&_lfDisplayFont, &lf, sizeof(lf)) &&
        !lstrcmpiW(_lfDisplayFont.lfFaceName, lf.lfFaceName)) {
        *phFont = _hDisplayFont;
        return S_OK;
    }

    FreeDisplayFontHandle();
    _hDisplayFont = CreateFontIndirectW(&lf);
    if (!_hDisplayFont)
        return E_OUTOFMEMORY;

    _lfDisplayFont = lf;
    *phFont = _hDisplayFont;
    return S_OK;
}

void CRenderCache::FreeDisplayFontHandle()
{
    if (_hDisplayFont) {
        DeleteObject(_hDisplayFont);
        _hDisplayFont = nullptr;
        memset(&_lfDisplayFont, 0, sizeof(_lfDisplayFont));
    }
}

HRESULT CRenderCache::GetScaledFontHandle(
    HDC hdc, LOGFONTW const* plfUnscaled, HFONT* phFont)
{
    if (!_plfFont ||
        memcmp(_plfFont, plfUnscaled, sizeof(LOGFONTW)) ||
        lstrcmpiW(_plfFont->lfFaceName, plfUnscaled->lfFaceName))
    {
        if (_hFont) {
            DeleteObject(_hFont);
            _hFont = nullptr;
            _plfFont = nullptr;
        }

        LOGFONTW scaledFont = *plfUnscaled;
        ScaleThemeFont(hdc, _pRenderObj, &scaledFont);
        _hFont = CreateFontIndirectW(&scaledFont);
        if (!_hFont)
            return E_OUTOFMEMORY;
        _plfFont = (LOGFONTW*)plfUnscaled;
    }

    *phFont = _hFont;
    return S_OK;
}

HRESULT CRenderObj::GetCachedDisplayFontHandle(
    HDC hdc, LOGFONTW const& lf, HFONT* phFont)
{
    if (_fCacheEnabled) {
        auto cache = GetCacheObject();
        if (cache)
            return cache->GetDisplayFontHandle(hdc, lf, phFont);
        return E_FAIL;
    }

    *phFont = CreateFontIndirectW(&lf);
    if (!*phFont)
        return MakeErrorLast();
    return S_OK;
}

HRESULT CRenderObj::GetScaledFontHandle(
    HDC hdc, unsigned short fontIndex, HFONT* phFont)
{
    auto font = _pThemeFile->GetFontByIndex(fontIndex);
    if (_fCacheEnabled) {
        auto cache = GetCacheObject();
        if (!cache)
            return E_FAIL;
        return cache->GetScaledFontHandle(hdc, font, phFont);
    }

    LOGFONTW scaledFont = *font;
    ScaleThemeFont(hdc, this, &scaledFont);

    *phFont = CreateFontIndirectW(&scaledFont);
    if (!*phFont)
        return MakeErrorLast();
    return S_OK;
}

HRESULT CRenderObj::ExternalGetEnumValue(
    int iPartId, int iStateId, int iPropId, int* piVal) const
{
    if (!piVal)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    *piVal = *reinterpret_cast<int*>(&_pbSharableData[idx]);
    return S_OK;
}

HRESULT CRenderObj::ExternalGetBool(int iPartId, int iStateId, int iPropId, int* pfVal) const
{
    if (!pfVal)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    *pfVal = _pbSharableData[idx];
    return S_OK;
}

HRESULT CRenderObj::ExternalGetFont(
    HDC hdc, int iPartId, int iStateId, int iPropId, int fWantHdcScaling,
    LOGFONTW* pFont) const
{
    if (!pFont)
        return E_INVALIDARG;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    unsigned short* v10 = (unsigned short*)&_pbSharableData[idx];
    if (*((BYTE *)v10 - 6) != TMT_FONT)
        return E_INVALIDARG;

    LOGFONTW font = *_pThemeFile->GetFontByIndex(*v10);

    if (fWantHdcScaling)
        ScaleThemeFont(hdc, this, &font);

    *pFont = font;
    return S_OK;
}

HRESULT CRenderObj::ExternalGetInt(
    int iPartId, int iStateId, int iPropId, int* piVal) const
{
    if (!piVal)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    *piVal = *reinterpret_cast<int*>(&_pbSharableData[idx]);
    return S_OK;
}

HRESULT CRenderObj::ExternalGetIntList(int iPartId, int iStateId, int iPropId, INTLIST* pIntList) const
{
    if (!pIntList)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    *pIntList = *reinterpret_cast<INTLIST*>(&_pbSharableData[idx]);
    return S_OK;
}

HRESULT CRenderObj::ExternalGetPosition(int iPartId, int iStateId, int iPropId,
                                        POINT* pPoint) const
{
    if (!pPoint)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    memcpy(pPoint, &_pbSharableData[idx], sizeof(POINT));
    return S_OK;
}

HRESULT CRenderObj::ExternalGetRect(int iPartId, int iStateId, int iPropId,
                                    RECT* pRect) const
{
    if (!pRect)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    memcpy(pRect, &_pbSharableData[idx], sizeof(RECT));
    return S_OK;
}

bool CRenderObj::_IsDWMAtlas() const
{
    return CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, _pszClassName, -1, L"DWMWINDOW", -1) == 2
        || CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, _pszClassName, -1, L"DWMTOUCH", -1) == 2;
}

static bool IsHighContrastMode()
{
    return false;
}

HRESULT CRenderObj::ExternalGetStream(int iPartId, int iStateId, int iPropId,
                                      void** ppvStream, DWORD* pcbStream,
                                      HINSTANCE hInst) const
{
    if (!ppvStream)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    char* v10 = &_pbSharableData[idx];
    int v11 = *((int*)v10 - 1);
    if (iPropId != TMT_DISKSTREAM) {
        *ppvStream = v10;
        if (pcbStream)
            *pcbStream = v11;
        return 0;
    }

    if (!hInst)
        return E_INVALIDARG;

    HGLOBAL hResData = LoadResource(hInst, (HRSRC)((char*)hInst + *(DWORD*)v10));
    if (!hResData)
        return 0x80070490;

    if (!IsHighContrastMode() || !_IsDWMAtlas()) {
        *ppvStream = LockResource(hResData);
        if (pcbStream)
            *pcbStream = *((int*)v10 + 1);
        return S_OK;
    }

    return TRACE_HR(E_NOTIMPL); // FIXME
}

static HRESULT SafeStringCchCopyW(
    wchar_t* pszDest, unsigned cchDest, wchar_t const* pszSrc)
{
    if (!pszDest)
        return E_INVALIDARG;

    if (cchDest) {
        if (pszSrc)
            return StringCchCopyW(pszDest, cchDest, pszSrc);
        *pszDest = 0;
    }

    return S_OK;
}

HRESULT CRenderObj::ExternalGetString(
    int iPartId, int iStateId, int iPropId, wchar_t* pszBuff, unsigned cchBuff)
{
    if (!pszBuff)
        return E_POINTER;

    int index = GetValueIndex(iPartId, iStateId, iPropId);
    if (index < 0)
        return 0x80070490;

    auto str = reinterpret_cast<wchar_t const*>(&_pbSharableData[index]);
    auto len = reinterpret_cast<unsigned const*>(str)[-1] / sizeof(wchar_t);
    if (len > cchBuff)
        len = cchBuff;
    ENSURE_HR(SafeStringCchCopyW(pszBuff, len, str));
    return S_OK;
}

HRESULT CRenderObj::ExternalGetBitmap(
    HDC hdc, int iDibOffset, unsigned dwFlags, HBITMAP* phBitmap)
{
    if (!phBitmap)
        return E_INVALIDARG;
    if (!iDibOffset)
        return E_FAIL;
    if (!_pbSharableData)
        return E_FAIL;

    auto hdr = GetBitmapHeader(iDibOffset);
    HBITMAP hBitmap = _phBitmapsArray[hdr->iBitmapIndex].hBitmap;
    if (!hBitmap) {
        int v11 = iDibOffset + sizeof(TMBITMAPHEADER);
        if (v11)
            return _BitmapFromDib(hdc, ((char*)hdr + hdr->dwSize), phBitmap);
        return E_FAIL;
    }

    if (dwFlags & GBF_COPY) {
        *phBitmap = static_cast<HBITMAP>(CopyImage(hBitmap, IMAGE_BITMAP, 0, 0, 0));;
        if (!*phBitmap)
            return MakeErrorLast();

        return S_OK;
    }

    if (dwFlags & GBF_DIRECT) {
        *phBitmap = hBitmap;
        return S_OK;
    }

    return E_INVALIDARG;
}

HRESULT CRenderObj::ExternalGetMargins(
    HDC hdc, int iPartId, int iStateId, int iPropId, RECT const* formal,
    MARGINS* pMargins) const
{
    if (!pMargins)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    memcpy(pMargins, &_pbSharableData[idx], sizeof(MARGINS));
    return S_OK;
}

HRESULT CRenderObj::ExternalGetMetric(
    HDC hdc, int iPartId, int iStateId, int iPropId, int *piVal)
{
    int value;
    ENSURE_HR(ExternalGetInt(iPartId, iStateId, iPropId, &value));
    *piVal = ScaleThemeSize(hdc, this, value);
    return S_OK;
}

HRESULT CRenderObj::GetAnimationProperty(
    int iPartId, int iStateId, AnimationProperty** ppAnimationProperty)
{
    if (!ppAnimationProperty)
        return E_POINTER;

    *ppAnimationProperty = nullptr;
    int index = GetValueIndex(iPartId, iStateId, TMT_20000);
    if (index < 0)
        return 0x80070490;

    auto data = (unsigned*)&_pbSharableData[index];
    if (*data > *(data - 1))
        *ppAnimationProperty = (AnimationProperty*)((char*)data + data[1]);

    return S_OK;
}

HRESULT CRenderObj::GetTransitionDuration(
    int iPartId, int iStateIdFrom, int iStateIdTo, int iPropId,
    DWORD* pdwDuration)
{
    if (!pdwDuration || iStateIdFrom <= 0 || iStateIdTo <= 0)
        return E_INVALIDARG;

    *pdwDuration = 0;

    INTLIST intList;
    ENSURE_HR(ExternalGetIntList(iPartId, iStateIdFrom, iPropId, &intList));
    if (iStateIdFrom > intList.iValues[0] || iStateIdTo > intList.iValues[0])
        return E_INVALIDARG;

    if (intList.iValueCount == 1)
        *pdwDuration = 0;
    else
        *pdwDuration = intList.iValues[iStateIdTo + intList.iValues[0] * (iStateIdFrom - 1)];

    return S_OK;
}

HRESULT CRenderObj::GetPropertyOrigin(
    int iPartId, int iStateId, int iTarget, PROPERTYORIGIN* pOrigin)
{
    __int64 stateId;
    __int64 partId;
    char *ptr;
    PROPERTYORIGIN origin;
    char *v11;
    uintptr_t ptrEnd;
    char *v13;
    signed __int64 v14;
    unsigned __int16 v15;
    bool v16;
    int v17;
    __int64 v18;
    HRESULT hr;
    int v21;

    stateId = iStateId;
    partId = iPartId;
    if (!iTarget || (ptr = _pbSectionData) == 0i64)
        return  E_FAIL;

    if (!pOrigin)
        return E_POINTER;

    origin = PO_CLASS;
    v11 = _pbSharableData;
    ptrEnd = (uintptr_t)&v11[*((DWORD *)v11 + 5) - 1 - 8];
    while ((uintptr_t)ptr <= ptrEnd)
    {
        v13 = ptr;
        v14 = (signed __int64)(ptr + 8);
        v15 = *(WORD *)v13;
        if (*(WORD *)v13 == 10)
        {
            if ((int)partId <= 0
                || (int)partId >= *(BYTE *)(v14 + 12)
                || (v17 = *(DWORD *)(v14 + 4 * partId + 16), v17 == -1))
            {
                v17 = *(DWORD *)(v14 + 16);
            }
            origin = (PROPERTYORIGIN)((v17 == *(DWORD *)(v14 + 16)) + 1);
            ptr = &v11[v17];
        } else if (v15 == 11)
        {
            if ((int)stateId <= 0
                || (int)stateId >= *(BYTE *)v14
                || (v21 = *(DWORD *)(v14 + 4 * stateId + 8), v21 == -1))
            {
                v21 = *(DWORD *)(v14 + 8);
            }
            origin = PO_STATE;
            if (v21 == *(DWORD *)(v14 + 8))
                origin = PO_PART;
            ptr = &v11[v21];
        } else
        {
            if (iTarget == -1)
            {
                v16 = (DWORD)stateId == 0;
                if ((int)stateId > 0)
                {
                    if (origin == PO_STATE)
                        goto LABEL_23;
                    v16 = (DWORD)stateId == 0;
                }
                if (v16 && origin == 1)
                {
                LABEL_23:
                    *pOrigin = origin;
                    return 0;
                }
            }
            if (v15 == iTarget)
                goto LABEL_23;
            if (v13[2] == 12)
            {
                v18 = *(DWORD *)v14;
                if ((DWORD)v18 == -1)
                {
                    *pOrigin = PO_NOTFOUND;
                    return 0;
                }
                origin = (PROPERTYORIGIN)(origin + 1);
                ptr = &v11[v18];
            } else
            {
                ptr = (char *)(*((DWORD *)v13 + 1) + v14);
            }
        }
    }
    hr = E_FAIL;
    return hr;
}

int CRenderObj::GetValueIndex(int iPartId, int iStateId, int iTarget) const
{
    if (!iTarget)
        return -1;

    char const* ptr = _pbSectionData;
    if (!ptr)
        return -1;

    auto themeHdr = (THEMEHDR const*)_pbSharableData;
    char const* const end = _pbSharableData + themeHdr->dwTotalLength - 1 - 8;

    while (ptr <= end) {
        auto entry = (ENTRYHDR const*)ptr;
        ValidatePtr(entry);

        if (entry->usTypeNum == TMT_PARTJUMPTBL) {
            auto tableHdr = (PARTJUMPTABLEHDR const*)(entry + 1);
            auto table = (int const*)(tableHdr + 1);
            ValidatePtr(tableHdr);

            int index;
            if (iPartId > 0 && iPartId < tableHdr->cParts && table[iPartId] != -1)
                index = table[iPartId];
            else
                index = table[0];

            ptr = &_pbSharableData[index];
        } else if (entry->usTypeNum == TMT_STATEJUMPTBL) {
            auto tableHdr = (STATEJUMPTABLEHDR const*)(entry + 1);
            auto table = (int const*)(tableHdr + 1);
            ValidatePtr(tableHdr);

            int index;
            if (iStateId > 0 && iStateId < tableHdr->cStates && table[iStateId] != -1)
                index = table[iStateId];
            else
                index = table[0];

            ptr = &_pbSharableData[index];
        } else {
            if (entry->usTypeNum == iTarget)
                return (char const*)(entry + 1) - (char const*)_pbSharableData;

            if (entry->ePrimVal == TMT_12) {
                int index = *(DWORD const*)(char const*)(entry + 1);
                if (index == -1)
                    break;
                ptr = &_pbSharableData[index];
            } else {
                ptr = (char const*)(entry + 1) + entry->dwDataLen;
            }
        }
    }

    return -1;
}

bool CRenderObj::IsPartDefined(int iPartId, int iStateId)
{
    PROPERTYORIGIN origin;
    HRESULT hr = GetPropertyOrigin(iPartId, iStateId, -1, &origin);
    SetLastError(hr);
    if (hr < 0)
        return false;

    if (iStateId)
        return origin == PO_STATE;
    return origin == PO_PART;
}

HRESULT CRenderObj::ExpandPartObjectCache(int cParts)
{
    if (cParts > 96)
        return E_FAIL;

    size_t oldSize = _pParts.size();
    _pParts.resize(cParts + 1);

    for (size_t i = oldSize; i < _pParts.size(); ++i) {
        _pParts[i] = make_unique_nothrow<CStateIdObjectCache>();
        if (!_pParts[i])
            return E_OUTOFMEMORY;
    }

    return S_OK;
}

template<>
PARTOBJHDR* CRenderObj::GetNextPartObject<CDrawBase>(MIXEDPTRS* u)
{
    auto hdr = reinterpret_cast<ENTRYHDR*>(u->pb);
    ValidatePtr(hdr);

    while (hdr->usTypeNum == TMT_IMAGEINFO) {
        hdr = hdr->Next();
        ValidatePtr(hdr);
    }

    if (hdr->usTypeNum != TMT_DRAWOBJ)
        return nullptr;

    u->pb = (char*)hdr->Next();
    return reinterpret_cast<PARTOBJHDR*>(hdr + 1);
}

template<>
PARTOBJHDR* CRenderObj::GetNextPartObject<CTextDraw>(MIXEDPTRS* u)
{
    auto hdr = reinterpret_cast<ENTRYHDR*>(u->pb);
    ValidatePtr(hdr);
    if (hdr->usTypeNum != TMT_TEXTOBJ)
        return nullptr;

    u->pb = (char*)hdr + sizeof(ENTRYHDR) + hdr->dwDataLen;
    return reinterpret_cast<PARTOBJHDR*>(hdr + 1);
}

template<typename T>
static int GetFirstObjIndex(char* const pb);

template<>
static int GetFirstObjIndex<CDrawBase>(char* const pb)
{
    auto jumpTable = (PARTJUMPTABLEHDR*)((ENTRYHDR*)pb + 1);
    ValidatePtr(jumpTable);
    return jumpTable->iFirstDrawObjIndex;
}

template<>
static int GetFirstObjIndex<CTextDraw>(char* const pb)
{
    auto jumpTable = (PARTJUMPTABLEHDR*)((ENTRYHDR*)pb + 1);
    ValidatePtr(jumpTable);
    return jumpTable->iFirstTextObjIndex;
}

template<typename T>
T* CRenderObj::FindClassPartObject(char* const pb, int iPartId, int iStateId)
{
    MIXEDPTRS u;
    u.pb = &_pbSharableData[GetFirstObjIndex<T>(pb)];

    PARTOBJHDR* partZeroObj = nullptr;
    PARTOBJHDR* stateZeroObj = nullptr;
    PARTOBJHDR* obj;

    while (true) {
        obj = GetNextPartObject<T>(&u);
        if (!obj)
            break;

        ValidatePtr(obj);
        if (obj->iPartId == iPartId && obj->iStateId == iStateId)
            break;

        if (obj->iPartId == iPartId && obj->iStateId == 0)
            stateZeroObj = obj;
        else if (obj->iPartId == 0 && obj->iStateId == 0)
            partZeroObj = obj;
    }

    if (!obj) {
        if (stateZeroObj)
            obj = stateZeroObj;
        else if (partZeroObj)
            obj = partZeroObj;
        if (!obj)
            return nullptr;
    }

    return reinterpret_cast<T*>(obj + 1);
}

template<typename T>
T* CRenderObj::FindBaseClassPartObject(int iPartId, int iStateId)
{
    auto hdr = (THEMEHDR const*)_pbSharableData;
    char const* const end = (char const*)hdr + hdr->dwTotalLength - 8;

    for (char* ptr = _pbSectionData; ptr < end;) {
        auto entry = (ENTRYHDR*)ptr;
        auto jumpTable = (PARTJUMPTABLEHDR*)(entry + 1);

        if (jumpTable->iBaseClassIndex == hdr->iGlobalsOffset)
            return (T*)((char*)hdr + hdr->iGlobalsTextObjOffset + 0x10);

        ptr = _pbSharableData + jumpTable->iBaseClassIndex;
        if (auto obj = FindClassPartObject<T>(ptr, iPartId, iStateId))
            return obj;
    }

    return nullptr;
}

template<typename T>
T* CRenderObj::FixupPartObjectCache(int iPartId, int iStateId)
{
    if (ExpandPartObjectCache(iPartId) < 0)
        return nullptr;

    CStateIdObjectCache& state = *_pParts[iPartId];
    if (state.Expand<T>(iStateId) < 0)
        return nullptr;

    auto partObj = FindClassPartObject<T>(_pbSectionData, iPartId, iStateId);
    if (!partObj)
        partObj = FindBaseClassPartObject<T>(iPartId, iStateId);
    state.GetObjects<T>()[iStateId] = partObj;
    return partObj;
}

template<typename T>
HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, T** ppvObj)
{
    *ppvObj = nullptr;

    if (_pThemeFile) {
        auto hdr = (NONSHARABLEDATAHDR*)_pThemeFile->_pbNonSharableData;
        if (!(hdr && hdr->dwFlags & 1))
            return E_FAIL;
    }

    std::lock_guard<std::mutex> lock(_lock);
    if (!_fPartCacheInitialized) {
        if (_fCacheEnabled) {
            //v5 = CRenderObj::CachePartObjects(this);
            //if (v5 < 0)
            //    return E_FAIL;
        }
    }

    if (int cParts = _pParts.size()) {
        if (iPartId < 0)
            iPartId = 0;

        if (iPartId < cParts) {
            auto& state = _pParts[iPartId];
            if (int cStates = state->GetObjects<T>().size()) {
                if (iStateId < 0)
                    iStateId = 0;
                if (iStateId < cStates)
                    *ppvObj = state->GetObjects<T>()[iStateId];
            }
        }

        if (*ppvObj)
            return S_OK;
    }

    *ppvObj = FixupPartObjectCache<T>(iPartId, iStateId);
    if (!*ppvObj)
        return E_FAIL;

    return S_OK;
}

HRESULT CRenderObj::PrepareRegionDataForScaling(
    RGNDATA *pRgnData, RECT *const prcImage, _MARGINS *pMargins)
{
    int v4;
    POINT *v5;
    int v6;
    int v7;
    int v8;
    char *v9;
    LONG v10;
    unsigned v11;
    int v12;
    int v13;
    LONG v14;
    __int64 v15;
    LONG v16;

    v4 = prcImage->left;
    v5 = (POINT *)pRgnData->Buffer;
    v6 = prcImage->right;
    v7 = prcImage->top;
    v8 = prcImage->bottom;
    v9 = &pRgnData->Buffer[pRgnData->rdh.nRgnSize];
    v10 = v4 + pMargins->cxLeftWidth;
    v11 = 2 * pRgnData->rdh.nCount;
    v12 = v6 - pMargins->cxRightWidth;
    v13 = v7 + pMargins->cyTopHeight;
    v14 = v8 - pMargins->cyBottomHeight;
    if ((int)v11 > 0)
    {
        v15 = v11;
        do
        {
            v16 = v5->x;
            if (v5->x >= v10)
            {
                if (v16 >= v12)
                {
                    v5->x = v16 - v12;
                    if (v5->y >= v13)
                    {
                        if (v5->y >= v14)
                        {
                            *v9 = 8;
                            goto LABEL_22;
                        }
                        *v9 = 5;
                    LABEL_20:
                        v5->y -= v13;
                        goto LABEL_23;
                    }
                    *v9 = 2;
                } else
                {
                    v5->x = v16 - v10;
                    if (v5->y >= v13)
                    {
                        if (v5->y >= v14)
                        {
                            *v9 = 7;
                            goto LABEL_22;
                        }
                        *v9 = 4;
                        goto LABEL_20;
                    }
                    *v9 = 1;
                }
            } else
            {
                v5->x = v16 - v4;
                if (v5->y >= v13)
                {
                    if (v5->y >= v14)
                    {
                        *v9 = 6;
                    LABEL_22:
                        v5->y -= v14;
                        goto LABEL_23;
                    }
                    *v9 = 3;
                    goto LABEL_20;
                }
                *v9 = 0;
            }
            v5->y -= v7;
        LABEL_23:
            ++v5;
            ++v9;
            --v15;
        } while (v15);
    }
    return 0;
}

template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CDrawBase** ppvObj);
template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CTextDraw** ppvObj);

} // namespace uxtheme
