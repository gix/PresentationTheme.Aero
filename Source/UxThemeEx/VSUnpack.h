#pragma once
#include "DpiInfo.h"
#include "Utils.h"
#include <array>
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

struct HCIMAGEPROPERTIES
{
    LONG lHCBorderColor;
    LONG lHCBackgroundColor;
};

struct IMAGEPROPERTIES
{
    DWORD dwBorderColor;
    DWORD dwBackgroundColor;
};

struct TMBITMAPHEADER
{
    DWORD dwSize;
    int iBitmapIndex;
    BOOL fPartiallyTransparent;
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
    virtual int AddToDIBDataArray(void* pDIBBits, short width, short height) = 0;
    virtual HRESULT AddBaseClass(int idClass, int idBaseClass) = 0;
    virtual int GetScreenPpi() = 0;
};

class CVSUnpack
{
public:
    static bool _DelayRecord(VSRECORD* pRec);

    HRESULT Initialize(HMODULE hInstSrc, int nVersion, bool fGlobal,
                       bool fIsLiteVisualStyle, bool fHighContrast);

    HRESULT GetRootMap(void** ppvRMap, int* pcbRMap);
    HRESULT GetVariantMap(void** ppvVMap, int* pcbVMap);
    HRESULT GetClassData(wchar_t const* pszColorVariant,
                         wchar_t const* pszSizeVariant, void** pvMap,
                         int* pcbMap);

    HRESULT LoadRootMap(IParserCallBack* pfnCB);
    HRESULT LoadClassDataMap(wchar_t const* pszColor, wchar_t const* pszSize, IParserCallBack* pfnCB);
    HRESULT LoadBaseClassDataMap(IParserCallBack* pfnCB);
    HRESULT LoadAnimationDataMap(IParserCallBack* pfnCB);

private:
    HRESULT _FindVSRecord(void* pvRecBuf, int cbRecBuf, int iClass, int iPart,
                          int iState, int lSymbolVal, VSRECORD** ppRec);
    HRESULT _GetPropertyValue(void* pvBits, int cbBits, int iClass, int iPart,
                              int iState, int lSymbolVal, void* pvValue,
                              int* pcbValue);
    HRESULT _CreateImageFromProperties(IMAGEPROPERTIES const* pImageProperties,
                                       int iImageCount,
                                       MARGINS const* pSizingMargins,
                                       MARGINS const* pTransparentMargins,
                                       BYTE** ppbNewBitmap, int* pcbNewBitmap);
    HRESULT _GetImagePropertiesForHC(IMAGEPROPERTIES** ppImageProperties,
                                     HCIMAGEPROPERTIES const* pHCImageProperties,
                                     int iImageCount);
    HRESULT _EnsureBufferSize(unsigned cbBytes);

    HRESULT _ExpandVSRecordForColor(IParserCallBack* pfnCB, VSRECORD* pRec, BYTE* pbData, int cbData, bool* pfIsColor);
    HRESULT _ExpandVSRecordForMargins(IParserCallBack* pfnCB, VSRECORD* pRec, BYTE* pbData, int cbData, bool* pfIsMargins);
    HRESULT _ExpandVSRecordData(IParserCallBack* pfnCB, VSRECORD* pRec, BYTE* pbData, int cbData);

    HRESULT _AddVSDataRecord(IParserCallBack* pfnCB, HMODULE hInst, VSRECORD* pRec);
    HRESULT _AddScaledBackgroundDataRecord(IParserCallBack* pfnCB);

    HRESULT _InitializePlateauPpiMapping(VSRECORD* pRec);
    HRESULT _ClearDpiRecords();
    HRESULT _SaveRecord(VSRECORD* pRec);
    HRESULT _FlushDelayedRecords(IParserCallBack* pfnCB);
    HRESULT _ClearPlateauRecords();
    HRESULT _SavePlateauRecord(VSRECORD* pRec);
    HRESULT _FlushDelayedPlateauRecords(IParserCallBack* pfnCB);
    HRESULT _FixSymbolAndAddVSDataRecord(IParserCallBack* pfnCB, VSRECORD* pRec, int lSymbolVal);

    bool _IsTrueSizeImage(VSRECORD* pRec);
    int _FindClass(wchar_t const* pszClass);

    bool IsHighContrastMode() const { return _fIsHighContrast; }

    HMODULE _hInst = nullptr;
    int _nVersion = 0;
    void* _pvRootMap = nullptr;
    int _cbRootMap = 0;
    void* _pvVariantMap = nullptr;
    int _cbVariantMap = 0;
    void* _pbClassData = nullptr;
    int _cbClassData = 0;
    std::vector<std::wstring> _rgClassNames;
    HANDLE _hSymbols = nullptr;
    bool _fGlobal = true;
    malloc_ptr<BYTE[]> _rgfPartiallyTransparent;
    malloc_ptr<int[]> _rgBitmapIndices;
    unsigned _cBitmaps = 0;
    unsigned _cbBuffer = 0;
    malloc_ptr<BYTE[]> _pBuffer;
    bool _fIsLiteVisualStyle = false;
    bool _fIsHighContrast = false;
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgImageDpiRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgImageRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgComposedImageRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgImagePRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgGlyphImagePRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgContentMarginsPRec = {};
    std::array<VSRECORD*, DPI_PLATEAU_COUNT> _rgSizingMarginsPRec = {};
    std::array<bool, DPI_PLATEAU_COUNT> _rgPlateauRec = {};
    std::array<int, DPI_PLATEAU_COUNT> _rgPlateauPpiMapping = {};
    CThemePNGDecoder* _pDecoder = nullptr;
};

} // namespace uxtheme
