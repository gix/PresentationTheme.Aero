#pragma once
#include "ImageFile.h"
#include "Primitives.h"
#include "VSUnpack.h"

#include <vector>
#include <memory>
#include <mutex>

namespace uxtheme
{

struct CTextDraw;
struct CUxThemeFile;

struct CStateIdObjectCache
{
    std::vector<CDrawBase*> DrawObjs;
    std::vector<CTextDraw*> TextObjs;

    template<typename T>
    HRESULT Expand(int cStates)
    {
        if (cStates > 96)
            return E_FAIL;

        GetObjects<T>().resize(cStates + 1);
        return S_OK;
    }

    template<typename T>
    std::vector<T*>& GetObjects();
};

template<>
inline std::vector<CDrawBase*>& CStateIdObjectCache::GetObjects<CDrawBase>()
{
    return DrawObjs;
}

template<>
inline std::vector<CTextDraw*>& CStateIdObjectCache::GetObjects<CTextDraw>()
{
    return TextObjs;
}

struct DiagnosticAnimationXML
{
};

struct CImageDecoder
{
};

struct CImageEncoder
{
};

struct CRenderCache
{
};

struct __declspec(align(8)) CRenderObj
{
    CRenderObj(CUxThemeFile* pThemeFile, int iCacheSlot, int iThemeOffset,
               int iClassNameOffset, long long iUniqueId, bool fEnableCache,
               int iTargetDpi, bool fIsStronglyAssociatedDpi, unsigned dwOtdFlags);
    static HRESULT Create(CUxThemeFile* pThemeFile, int iCacheSlot,
                          int iThemeOffset, int iAppNameOffset,
                          int iClassNameOffset, long long iUniqueId,
                          bool fEnableCache, CDrawBase* pBaseObj,
                          CTextDraw* pTextObj, int iTargetDpi,
                          bool fIsStronglyAssociatedDpi, unsigned dwOtdFlags,
                          CRenderObj** ppObj);
    void GetEffectiveDpi(HDC hdc, int* px, int* py);
    TMBITMAPHEADER* GetBitmapHeader(int iDibOffset) const;
    bool GetFontTableIndex(int iPartId, int iStateId, int iPropId, unsigned short* pFontIndex) const;
    int GetValueIndex(int iPartId, int iStateId, int iTarget) const;
    HRESULT ExternalGetEnumValue(int iPartId, int iStateId, int iPropId, int* piVal) const;
    HRESULT ExternalGetBool(int iPartId, int iStateId, int iPropId, int* pfVal) const;
    HRESULT ExternalGetInt(int iPartId, int iStateId, int iPropId, int* piVal) const;
    HRESULT ExternalGetIntList(int iPartId, int iStateId, int iPropId, INTLIST* pIntList) const;
    HRESULT ExternalGetPosition(int iPartId, int iStateId, int iPropId, POINT* pPoint) const;
    HRESULT ExternalGetRect(int iPartId, int iStateId, int iPropId, RECT* pRect) const;
    HRESULT ExternalGetStream(int iPartId, int iStateId, int iPropId, void** ppvStream, DWORD* pcbStream, HINSTANCE hInst) const;
    HRESULT ExternalGetBitmap(HDC hdc, int iDibOffset, unsigned dwFlags, HBITMAP* phBitmap);
    HRESULT ExternalGetMargins(HDC hdc, int iPartId, int iStateId, int iPropId,
                               RECT const* formal, MARGINS* pMargins) const;
    HRESULT ExternalGetMetric(HDC hdc, int iPartId, int iStateId, int iPropId, int* piVal);

    HRESULT ExpandPartObjectCache(int cParts);

    template<typename T>
    T* FindClassPartObject(char* const pb, int iPartId, int iStateId);

    template<class T>
    T* FindBaseClassPartObject(int iPartId, int iStateId);

    template<typename T>
    T* FixupPartObjectCache(int iPartId, int iStateId);

    template<typename T>
    HRESULT GetPartObject(int iPartId, int iStateId, T** ppvObj);

    int GetThemeOffset() const
    {
        return (uintptr_t)_pbSectionData - (uintptr_t)_pbSharableData;
    }

    CUxThemeFile* _pThemeFile;
    int _iCacheSlot;
    __int64 _iUniqueId;
    char* _pbSharableData;
    char* _pbSectionData;
    char* _pbNonSharableData;
    HBITMAP64* _phBitmapsArray;
    bool _fCacheEnabled;
    bool _fCloseThemeFile;
    bool _fPartCacheInitialized;
    THEMEMETRICS* _ptm;
    wchar_t const* _pszClassName;
    std::vector<std::unique_ptr<CStateIdObjectCache>> _pParts;
    unsigned int _dwOtdFlags;
    //CAutoRefPtr<CTrackPartsObj> _spTrackingObj;
    CImageDecoder* _pPngDecoder;
    CImageEncoder* _pPngEncoder;
    CRenderCache* _pCacheObj;
    std::mutex _lock;
    DiagnosticAnimationXML*_pDiagnosticXML;
    bool _fDiagnosticModeEnabled;
    int _iAssociatedDpi;
    bool _fIsStronglyAssociatedDpi;
};

} // namespace uxtheme
