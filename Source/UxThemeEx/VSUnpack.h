#pragma once
#include <string>
#include <vector>
#include <windows.h>
#include <uxtheme.h>

namespace uxtheme
{

class CThemePNGDecoder;

struct VSRECORD
{
    int lSymbolVal = 0;
    int lType = 0;
    int iClass = -1;
    int iPart = 0;
    int iState = 0;
    unsigned uResID = 0;
    int lReserved = 0;
    int cbData = 0;

    void* operator new(size_t size, std::nothrow_t) noexcept
    {
        return ::operator new(size, std::nothrow);
    }

    void* operator new(size_t size, size_t extraSize, std::nothrow_t) noexcept
    {
        return ::operator new(size + extraSize, std::nothrow);
    }
};

struct _HCIMAGEPROPERTIES
{
    int lHCBorderColor;
    int lHCBackgroundColor;
};

struct _IMAGEPROPERTIES
{
    unsigned dwBorderColor;
    unsigned dwBackgroundColor;
};

struct TMBITMAPHEADER
{
    unsigned dwSize;
    int iBitmapIndex;
    int fPartiallyTransparent;
};

struct BITMAPHDR
{
    void* buffer;
    int size;
};

struct IParserCallBack
{
    virtual ~IParserCallBack() = default;
    virtual HRESULT AddIndex(wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId, int iIndex, int iLen) = 0;
    virtual HRESULT AddData(short sTypeNum, unsigned char ePrimVal, void const* pData, unsigned dwLen) = 0;
    virtual int GetNextDataIndex() = 0;
    virtual int AddToDIBDataArray(void* pDIBBits, __int16 width, __int16 height) = 0;
    virtual HRESULT AddBaseClass(int idClass, int idBaseClass) = 0;
    virtual int GetScreenPpi() = 0;
};

struct CVSUnpack
{
    HMODULE _hInst = nullptr;
    int _nVersion = 0;
    void* _pvRootMap = nullptr;
    int _cbRootMap = 0;
    void* _pvVariantMap = nullptr;
    int _cbVariantMap = 0;
    void* _pbClassData = nullptr;
    int _cbClassData = 0;
    std::vector<std::wstring> _rgClassNames;
    void* _hSymbols = nullptr;
    int _fGlobal = 1;
    char* _rgfPartiallyTransparent = nullptr;
    int* _rgBitmapIndices = nullptr;
    unsigned _cBitmaps = 0;
    unsigned _cbBuffer = 0;
    char* _pBuffer = nullptr;
    int _fIsLiteVisualStyle = 0;
    VSRECORD* _rgImageDpiRec[7] = {};
    VSRECORD* _rgImageRec[7] = {};
    VSRECORD* _rgComposedImageRec[7] = {};
    VSRECORD* _rgImagePRec[7] = {};
    VSRECORD* _rgGlyphImagePRec[7] = {};
    VSRECORD* _rgContentMarginsPRec[7] = {};
    VSRECORD* _rgSizingMarginsPRec[7] = {};
    bool _rgPlateauRec[7] = {};
    int _rgPlateauPpiMapping[7] = {0};
    CThemePNGDecoder* _pDecoder = nullptr;

    static bool _DelayRecord(VSRECORD* pRec);

    HRESULT Initialize(HMODULE hInstSrc, int nVersion, int fGlobal, int fIsLiteVisualStyle);

    HRESULT GetRootMap(void** ppvRMap, int* pcbRMap);
    HRESULT GetVariantMap(void** ppvVMap, int* pcbVMap);
    HRESULT GetClassData(wchar_t const* pszColorVariant, wchar_t const* pszSizeVariant, void** pvMap, int* pcbMap);

    HRESULT LoadRootMap(IParserCallBack* pfnCB);
    HRESULT LoadClassDataMap(wchar_t const* pszColor, wchar_t const* pszSize, IParserCallBack* pfnCB);
    HRESULT LoadBaseClassDataMap(IParserCallBack* pfnCB);

    HRESULT _FindVSRecord(void* pvRecBuf, int cbRecBuf, int iClass, int iPart, int iState, int lSymbolVal, VSRECORD** ppRec);
    HRESULT _GetPropertyValue(void* pvBits, int cbBits, int iClass, int iPart, int iState, int lSymbolVal, void* pvValue, int* pcbValue);
    HRESULT _GetImagePropertiesForHC(_IMAGEPROPERTIES** ppImageProperties, _HCIMAGEPROPERTIES* pHCImageProperties, int iImageCount);
    HRESULT _CreateImageFromProperties(_IMAGEPROPERTIES* pImageProperties, int iImageCount, MARGINS* pSizingMargins, MARGINS* pTransparentMargins, char** ppbNewBitmap, int* pcbNewBitmap);
    HRESULT _EnsureBufferSize(unsigned cbBytes);
    HRESULT _ExpandVSRecordForColor(IParserCallBack* pfnCB, VSRECORD* pRec, char* pbData, int cbData, bool* pfIsColor);
    HRESULT _ExpandVSRecordForMargins(IParserCallBack* pfnCB, VSRECORD* pRec, char* pbData, int cbData, bool* pfIsMargins);
    HRESULT _ExpandVSRecordData(IParserCallBack* pfnCB, VSRECORD* pRec, char* pbData, int cbData);
    HRESULT _AddVSDataRecord(IParserCallBack* pfnCB, HMODULE hInst, VSRECORD* pRec);
    HRESULT _InitializePlateauPpiMapping(VSRECORD* pRec);
    HRESULT _FlushDelayedRecords(IParserCallBack* pfnCB);
    HRESULT _FlushDelayedPlateauRecords(IParserCallBack* pfnCB);
    HRESULT _AddScaledBackgroundDataRecord(IParserCallBack* pfnCB);
    HRESULT _SavePlateauRecord(VSRECORD* pRec);
    HRESULT _FixSymbolAndAddVSDataRecord(IParserCallBack* pfnCB, VSRECORD* pRec, int lSymbolVal);
    HRESULT _SaveRecord(VSRECORD* pRec);
    bool _IsTrueSizeImage(VSRECORD* pRec);
    int _FindClass(wchar_t const* pszClass);
    HRESULT LoadAnimationDataMap(IParserCallBack* pfnCB);
};

} // namespace uxtheme
