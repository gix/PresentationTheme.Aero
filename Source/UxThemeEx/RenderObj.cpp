#include "RenderObj.h"

#include "Debug.h"
#include "DpiInfo.h"
#include "ScalingUtil.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"
#include <strsafe.h>

namespace uxtheme
{

static ENTRYHDR const* GetEntryHeader(void const* data)
{
    return (ENTRYHDR const*)((char const*)data - sizeof(ENTRYHDR));
}

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
        _pbSharableData = (BYTE*)pThemeFile->_pbSharableData;
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
        _pszClassName = (const wchar_t*)((char*)pThemeFile->_pbSharableData + iClassNameOffset);
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

    auto data = (USHORT*)&_pbSharableData[idx];
    if (GetEntryHeader(data)->ePrimVal != TMT_FONT)
        return E_INVALIDARG;

    LOGFONTW font = *_pThemeFile->GetFontByIndex(*data);

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

    std::memcpy(piVal, &_pbSharableData[idx], sizeof(int));
    return S_OK;
}

HRESULT CRenderObj::ExternalGetIntList(int iPartId, int iStateId, int iPropId,
                                       INTLIST* pIntList) const
{
    if (!pIntList)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    std::memcpy(pIntList, &_pbSharableData[idx], sizeof(INTLIST));
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

    std::memcpy(pPoint, &_pbSharableData[idx], sizeof(POINT));
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

HRESULT CRenderObj::ExternalGetStream(int iPartId, int iStateId, int iPropId,
                                      void** ppvStream, DWORD* pcbStream,
                                      HINSTANCE hInst) const
{
    if (!ppvStream)
        return E_POINTER;

    int idx = GetValueIndex(iPartId, iStateId, iPropId);
    if (idx < 0)
        return 0x80070490;

    BYTE* v10 = &_pbSharableData[idx];
    int v11 = *((int*)v10 - 1);
    DWORD x2 = *((DWORD*)v10);
    int x1 = *((int*)v10 + 1);
    if (iPropId != TMT_DISKSTREAM) {
        *ppvStream = v10;
        if (pcbStream)
            *pcbStream = v11;
        return S_OK;
    }

    if (!hInst)
        return E_INVALIDARG;

    HGLOBAL hResData = LoadResource(hInst, (HRSRC)((BYTE*)hInst + x2));
    if (!hResData)
        return 0x80070490;

    if (!IsHighContrastMode() || !_IsDWMAtlas()) {
        *ppvStream = LockResource(hResData);
        if (pcbStream) {
            *pcbStream = x1;
        }
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
    HBITMAP hBitmap = BitmapIndexToHandle(hdr->iBitmapIndex);
    if (!hBitmap) {
        int v11 = iDibOffset + sizeof(TMBITMAPHEADER);
        if (v11)
            return _BitmapFromDib(hdc, (char*)hdr + hdr->dwSize, phBitmap);
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

BYTE const* CRenderObj::GetLastValidThemeByte() const
{
    auto themeHdr = (THEMEHDR const*)_pbSharableData;
    return _pbSharableData + themeHdr->dwTotalLength - 1 - 8;
}

HRESULT CRenderObj::GetPropertyOrigin(
    int iPartId, int iStateId, int iTarget, PROPERTYORIGIN* pOrigin)
{
    if (!iTarget)
        return E_FAIL;
    if (!_pbSectionData)
        return E_FAIL;
    if (!pOrigin)
        return E_POINTER;

    PROPERTYORIGIN origin = PO_CLASS;

    auto const lastValidByte = GetLastValidThemeByte();
    for (auto const* ptr = _pbSectionData; ptr <= lastValidByte;) {
        auto entry = (ENTRYHDR const*)ptr;

        if (entry->usTypeNum == TMT_PARTJUMPTBL) {
            auto tableHdr = (PARTJUMPTABLEHDR const*)(entry + 1);
            auto table = (int const*)(tableHdr + 1);

            int index;
            if (iPartId > 0 && iPartId < tableHdr->cParts && table[iPartId] != -1)
                index = table[iPartId];
            else
                index = table[0];

            origin = index == table[0] ? PO_CLASS : PO_PART;
            ptr = &_pbSharableData[index];
        } else if (entry->usTypeNum == TMT_STATEJUMPTBL) {
            auto tableHdr = (STATEJUMPTABLEHDR const*)(entry + 1);
            auto table = (int const*)(tableHdr + 1);

            int index;
            if (iStateId > 0 && iStateId < tableHdr->cStates && table[iStateId] != -1)
                index = table[iStateId];
            else
                index = table[0];

            origin = index == table[0] ? PO_PART : PO_STATE;
            ptr = &_pbSharableData[index];
        } else {
            if (iTarget == -1) {
                if (iStateId > 0 && origin == PO_STATE) {
                    *pOrigin = origin;
                    return S_OK;
                }
                if (iStateId == 0 && origin == PO_PART) {
                    *pOrigin = origin;
                    return S_OK;
                }
            }

            if (entry->usTypeNum == iTarget) {
                *pOrigin = origin;
                return S_OK;
            }

            if (entry->ePrimVal == TMT_12) {
                int index = *(DWORD const*)(entry + 1);
                if (index == -1) {
                    *pOrigin = PO_NOTFOUND;
                    return S_OK;
                }

                origin = (PROPERTYORIGIN)(origin + 1);
                ptr = &_pbSharableData[index];
            } else {
                ptr = (BYTE const*)(entry + 1) + entry->dwDataLen;
            }
        }
    }

    return E_FAIL;
}

int CRenderObj::GetValueIndex(int iPartId, int iStateId, int iTarget) const
{
    if (!iTarget)
        return -1;

    BYTE const* ptr = _pbSectionData;
    if (!ptr)
        return -1;

    BYTE const* const end = GetLastValidThemeByte();

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
                return (uintptr_t)(entry + 1) - (uintptr_t)_pbSharableData;

            if (entry->ePrimVal == TMT_12) {
                int index = *(DWORD const*)(entry + 1);
                if (index == -1)
                    break;
                ptr = &_pbSharableData[index];
            } else {
                ptr = (BYTE const*)(entry + 1) + entry->dwDataLen;
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

    u->pb = (BYTE*)hdr->Next();
    return reinterpret_cast<PARTOBJHDR*>(hdr + 1);
}

template<>
PARTOBJHDR* CRenderObj::GetNextPartObject<CTextDraw>(MIXEDPTRS* u)
{
    auto hdr = reinterpret_cast<ENTRYHDR*>(u->pb);
    ValidatePtr(hdr);
    if (hdr->usTypeNum != TMT_TEXTOBJ)
        return nullptr;

    u->pb = (BYTE*)hdr + sizeof(ENTRYHDR) + hdr->dwDataLen;
    return reinterpret_cast<PARTOBJHDR*>(hdr + 1);
}

template<typename T>
static int GetFirstObjIndex(BYTE* const pb);

template<>
static int GetFirstObjIndex<CDrawBase>(BYTE* const pb)
{
    auto jumpTable = (PARTJUMPTABLEHDR*)((ENTRYHDR*)pb + 1);
    ValidatePtr(jumpTable);
    return jumpTable->iFirstDrawObjIndex;
}

template<>
static int GetFirstObjIndex<CTextDraw>(BYTE* const pb)
{
    auto jumpTable = (PARTJUMPTABLEHDR*)((ENTRYHDR*)pb + 1);
    ValidatePtr(jumpTable);
    return jumpTable->iFirstTextObjIndex;
}

template<typename T>
T* CRenderObj::FindClassPartObject(BYTE* const pb, int iPartId, int iStateId)
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
    auto const end = (BYTE const*)hdr + hdr->dwTotalLength - 8;

    for (BYTE* ptr = _pbSectionData; ptr < end;) {
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
    RGNDATA *pRgnData, RECT *const prcImage, MARGINS *pMargins)
{
    int const sw = prcImage->left;
    int const sh = prcImage->top;
    int const lw = sw + pMargins->cxLeftWidth;
    int const rw = prcImage->right - pMargins->cxRightWidth;
    int const th = sh + pMargins->cyTopHeight;
    int const bh = prcImage->bottom - pMargins->cyBottomHeight;

    auto pt = (POINT*)pRgnData->Buffer;
    auto pByte = (BYTE*)&pRgnData->Buffer[pRgnData->rdh.nRgnSize];

    for (DWORD i = 0; i < 2 * pRgnData->rdh.nCount; ++pt, ++pByte, ++i) {
        if (pt->x < lw) {
            pt->x -= sw;
            if (pt->y < th) {
                *pByte = 0;
                pt->y -= sh;
            } else if (pt->y >= bh) {
                *pByte = 6;
                pt->y -= bh;
            } else {
                *pByte = 3;
                pt->y -= th;
            }
        } else if (pt->x >= rw) {
            pt->x -= rw;
            if (pt->y < th) {
                *pByte = 2;
                pt->y -= sh;
            } else if (pt->y >= bh) {
                *pByte = 8;
                pt->y -= bh;
            } else {
                *pByte = 5;
                pt->y -= th;
            }
        } else {
            pt->x -= lw;
            if (pt->y < th) {
                *pByte = 1;
                pt->y -= sh;
            } else if (pt->y >= bh) {
                *pByte = 7;
                pt->y -= bh;
            } else {
                *pByte = 4;
                pt->y -= th;
            }
        }
    }

    return S_OK;
}

template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CDrawBase** ppvObj);
template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CTextDraw** ppvObj);

} // namespace uxtheme
