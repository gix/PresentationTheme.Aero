#include "RenderObj.h"
#include "UxThemeFile.h"
#include "Utils.h"
#include "DpiInfo.h"

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

static int ScaleThemeSize(HDC hdc, CRenderObj* pRender, int iValue)
{
    int dpi;
    if (!hdc || (pRender->_fIsStronglyAssociatedDpi && IsScreenDC(hdc)))
        dpi = pRender->_iAssociatedDpi;
    else
        dpi = GetDeviceCaps(hdc, LOGPIXELSX);

    return MulDiv(iValue, dpi, 96);
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

    _pPngDecoder = 0i64;
    _pPngEncoder = 0i64;
    _pCacheObj = 0i64;
}

HRESULT CRenderObj::Create(
    CUxThemeFile* pThemeFile, int iCacheSlot, int iThemeOffset,
    int iAppNameOffset, int iClassNameOffset, __int64 iUniqueId,
    bool fEnableCache, CDrawBase* pBaseObj, CTextDraw* pTextObj, int iTargetDpi,
    bool fIsStronglyAssociatedDpi, unsigned int dwOtdFlags, CRenderObj** ppObj)
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

bool CRenderObj::GetFontTableIndex(int iPartId, int iStateId, int iPropId, unsigned short* pFontIndex) const
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

HRESULT CRenderObj::ExternalGetEnumValue(int iPartId, int iStateId, int iPropId, int* piVal) const
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

HRESULT CRenderObj::ExternalGetInt(int iPartId, int iStateId, int iPropId, int* piVal) const
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

HRESULT CRenderObj::ExternalGetStream(int iPartId, int iStateId, int iPropId,
                                      void** ppvStream, DWORD* pcbStream,
                                      HINSTANCE hInst) const
{
    return TRACE_HR(E_NOTIMPL); //FIXME
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

int CRenderObj::GetValueIndex(int iPartId, int iStateId, int iTarget) const
{
    __int64 stateId;
    char* sectionDataPtr;
    unsigned __int64 v10;
    ENTRYHDR* v11;
    char* v12;
    unsigned __int16 v13;
    int v15;
    __int64 partId;
    int v18;
    //CBitArray* v19; // rdi@27

    stateId = iStateId;
    partId = iPartId;
    if (!iTarget)
        return -1;
    sectionDataPtr = _pbSectionData;
    if (!sectionDataPtr)
        return -1;
    char * const pbSharableData = _pbSharableData;
    v10 = (unsigned __int64)&pbSharableData[*((DWORD *)pbSharableData + 5) - 1 - 8];
    if ((unsigned __int64)sectionDataPtr > v10)
        return -1;

    while (1) {
        v11 = (ENTRYHDR *)sectionDataPtr;
        v12 = sectionDataPtr + 8;
        v13 = v11->usTypeNum;
        if (v11->usTypeNum == 10)
        {
            if ((signed int)partId <= 0
                || (signed int)partId >= (unsigned __int8)v12[12]
                || (v15 = *(DWORD *)&v12[4 * partId + 16], v15 == -1))
            {
                v15 = *((DWORD *)v12 + 4);
            }
            sectionDataPtr = &pbSharableData[v15];
            goto LABEL_9;
        }
        if (v13 == 11)
        {
            if ((signed int)stateId <= 0
                || (signed int)stateId >= (unsigned __int8)*v12
                || (v18 = *(DWORD *)&v12[4 * stateId + 8], v18 == -1))
            {
                v18 = *((DWORD *)v12 + 2);
            }
            sectionDataPtr = &pbSharableData[v18];
            goto LABEL_9;
        }
        if (v13 == iTarget)
            break;
        if (v11->ePrimVal == 12)
        {
            partId = *(DWORD *)v12;
            if ((DWORD)partId == -1)
                return -1;
            sectionDataPtr = &pbSharableData[partId];
        } else
        {
            sectionDataPtr = &v12[v11->dwDataLen];
        }
    LABEL_9:
        if ((unsigned __int64)sectionDataPtr > v10)
            return -1;
    }

    return v12 - (char*)(pbSharableData);

    //v16 = _spTrackingObj._p;
    //if (!v16)
    //    return (_DWORD)v12 - LODWORD(_pbSharableData);
    //v19 = &v16->_partAccessFlags;
    //if (CBitArray::GetItem(&v16->_partAccessFlags, 0))
    //{
    //    v20 = 1;
    //    goto LABEL_31;
    //}
    //if ((_DWORD)partId)
    //{
    //    v20 = partId;
    //LABEL_31:
    //    CBitArray::SetItem(v19, v20, 1);
    //}
    //return (_DWORD)v12 - LODWORD(_pbSharableData);
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

template<typename T>
T* CRenderObj::FindClassPartObject(char* const pb, int iPartId, int iStateId)
{
    auto entry = (ENTRYHDR *)&_pbSharableData[*((DWORD *)pb + 3)];
    PARTOBJHDR* stateZeroHdr = nullptr;
    PARTOBJHDR* partZeroHdr = nullptr;

    while (true) {
        while (entry->usTypeNum == 18) {
            auto pHdr = (PARTOBJHDR*)(entry + 1);
            entry = (ENTRYHDR*)((char*)pHdr + entry->dwDataLen);
        }

        PARTOBJHDR* partObjHdr = nullptr;
        if (entry->usTypeNum == 16) {
            partObjHdr = (PARTOBJHDR*)(entry + 1);
            entry = (ENTRYHDR*)((char*)partObjHdr + entry->dwDataLen);
        }

        if (partObjHdr) {
            if (partObjHdr->iPartId != iPartId) {
                if (!partObjHdr->iPartId && !partObjHdr->iStateId)
                    partZeroHdr = partObjHdr;
                continue;
            }

            if (partObjHdr->iStateId != iStateId) {
                if (!partObjHdr->iStateId)
                    stateZeroHdr = partObjHdr;
                continue;
            }

            return reinterpret_cast<T*>(partObjHdr + 1);
        }

        if (stateZeroHdr)
            partObjHdr = stateZeroHdr;
        else if (partZeroHdr)
            partObjHdr = partZeroHdr;

        if (partObjHdr)
            return reinterpret_cast<T*>(partObjHdr + 1);
        return nullptr;
    }
}

template<typename T>
T* CRenderObj::FindBaseClassPartObject(int iPartId, int iStateId)
{
    char* ptr = _pbSectionData;
    char* const end = (char*)&_pbSharableData[*((DWORD *)_pbSharableData + 5) - 1 - 8];

    if (ptr <= end) {
        while (ptr <= end) {
            ptr += 8;
            __int64 v10 = *(DWORD *)ptr;
            if ((DWORD)v10 == *((DWORD *)_pbSharableData + 17))
                break;
            ptr = &_pbSharableData[v10];
            auto partObj = FindClassPartObject<T>(&_pbSharableData[v10], iPartId, iStateId);
            if (partObj)
                return partObj;
        }

        if (ptr <= end)
            return (T*)&_pbSharableData[*((DWORD *)_pbSharableData + 19) + 16];
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

template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CDrawBase** ppvObj);
template HRESULT CRenderObj::GetPartObject(int iPartId, int iStateId, CTextDraw** ppvObj);

} // namespace uxtheme
