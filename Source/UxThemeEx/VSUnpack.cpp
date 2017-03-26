#include "VSUnpack.h"

#include "DpiInfo.h"
#include "ThemePNGDecoder.h"
#include "Utils.h"
#include "UxThemeHelpers.h"

#include <cassert>
#include <vssym32.h>
#include <strsafe.h>

namespace uxtheme
{

enum THEMEPRIMITIVEID
{
    TPID_BITMAPIMAGE = 0,
    TPID_BITMAPIMAGE1 = 1,
    TPID_BITMAPIMAGE2 = 2,
    TPID_BITMAPIMAGE3 = 3,
    TPID_BITMAPIMAGE4 = 4,
    TPID_BITMAPIMAGE5 = 5,
    TPID_BITMAPIMAGE6 = 6,
    TPID_BITMAPIMAGE7 = 7,
    TPID_STOCKBITMAPIMAGE = 8,
    TPID_GLYPHIMAGE = 9,
    TPID_ATLASINPUTIMAGE = 10,
    TPID_ATLASIMAGE = 11,
    TPID_ENUM = 12,
    TPID_STRING = 13,
    TPID_INT = 14,
    TPID_BOOL = 15,
    TPID_COLOR = 16,
    TPID_MARGINS = 17,
    TPID_FILENAME = 18,
    TPID_SIZE = 19,
    TPID_POSITION = 20,
    TPID_RECT = 21,
    TPID_FONT = 22,
    TPID_INTLIST = 23,
    TPID_DISKSTREAM = 24,
    TPID_STREAM = 25,
    TPID_ANIMATION = 26,
    TPID_TIMINGFUNCTION = 27,
    TPID_SIMPLIFIEDIMAGETYPE = 28,
    TPID_HIGHCONTRASTCOLORTYPE = 29,
    TPID_BITMAPIMAGETYPE = 30,
    TPID_COMPOSEDIMAGETYPE = 31,
    TPID_FLOAT = 32,
    TPID_FLOATLIST = 33,
};

enum VSRESOURCETYPE
{
    VSRT_NIL = 0,
    VSRT_STRING = 1,
    VSRT_BITMAP = 2,
    VSRT_STREAM = 3,
};

struct _VSRESOURCE
{
    VSRESOURCETYPE type;
    unsigned int uResID;
    std::wstring strValue;
    std::wstring strComment;
};

struct VSERRORCONTEXT
{
    _iobuf* _pLog;
    HINSTANCE__* _hInstErrorRes;
    wchar_t const* _pszSource;
    unsigned int _nLineNumber;
    wchar_t const* _pszTool;
};

struct VSCOLORPROPENTRY
{
    int lHCSymbolVal;
    int lSymbolVal;
};

static VSCOLORPROPENTRY vscolorprops[13] = {
    {TMT_HCBORDERCOLOR, TMT_BORDERCOLOR},
    {TMT_HCFILLCOLOR, TMT_FILLCOLOR},
    {TMT_HCTEXTCOLOR, TMT_TEXTCOLOR},
    {TMT_HCEDGEHIGHLIGHTCOLOR, TMT_EDGEHIGHLIGHTCOLOR},
    {TMT_HCEDGESHADOWCOLOR, TMT_EDGESHADOWCOLOR},
    {TMT_HCTEXTBORDERCOLOR, TMT_TEXTBORDERCOLOR},
    {TMT_HCTEXTSHADOWCOLOR, TMT_TEXTSHADOWCOLOR},
    {TMT_HCGLOWCOLOR, TMT_GLOWCOLOR},
    {TMT_HCHEADING1TEXTCOLOR, TMT_HEADING1TEXTCOLOR},
    {TMT_HCHEADING2TEXTCOLOR, TMT_HEADING2TEXTCOLOR},
    {TMT_HCBODYTEXTCOLOR, TMT_BODYTEXTCOLOR},
    {TMT_5121, TMT_5106},
    {TMT_HCHOTTRACKING, TMT_HOTTRACKING},
};

struct PROPERTYMAP
{
    THEMEPRIMITIVEID tpi;
    int lSymbolVal;
    int lType;
    int cbType;
};

static PROPERTYMAP property_map[34] = {
    {TPID_BITMAPIMAGE, TMT_IMAGEFILE, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE1, TMT_IMAGEFILE1, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE2, TMT_IMAGEFILE2, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE3, TMT_IMAGEFILE3, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE4, TMT_IMAGEFILE4, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE5, TMT_IMAGEFILE5, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE6, TMT_IMAGEFILE6, TMT_FILENAME, 16},
    {TPID_BITMAPIMAGE7, TMT_IMAGEFILE7, TMT_FILENAME, 16},
    {TPID_STOCKBITMAPIMAGE, 3007, TMT_FILENAME, 16},
    {TPID_GLYPHIMAGE, TMT_GLYPHIMAGEFILE, TMT_FILENAME, 16},
    {TPID_ATLASIMAGE, TMT_ATLASIMAGE, TMT_DISKSTREAM, -1},
    {TPID_ATLASINPUTIMAGE, TMT_ATLASINPUTIMAGE, TMT_STRING, 16},
    {TPID_ENUM, 0, TMT_ENUM, 4},
    {TPID_STRING, 0, TMT_STRING, -1},
    {TPID_INT, 0, TMT_INT, 4},
    {TPID_BOOL, 0, TMT_BOOL, 4},
    {TPID_COLOR, 0, TMT_COLOR, 4},
    {TPID_MARGINS, 0, TMT_MARGINS, 16},
    {TPID_FILENAME, 0, TMT_FILENAME, -1},
    {TPID_SIZE, 0, TMT_SIZE, 4},
    {TPID_POSITION, 0, TMT_POSITION, TMT_GLYPHDIBDATA},
    {TPID_RECT, 0, TMT_RECT, 16},
    {TPID_FONT, 0, TMT_FONT, 92},
    {TPID_INTLIST, 0, TMT_INTLIST, -1},
    {TPID_DISKSTREAM, 0, TMT_DISKSTREAM, -1},
    {TPID_STREAM, 0, TMT_STREAM, -1},
    {TPID_ANIMATION, 20000, 241, -1},
    {TPID_TIMINGFUNCTION, 20100, 242, -1},
    {TPID_SIMPLIFIEDIMAGETYPE, 0, 240, -1},
    {TPID_HIGHCONTRASTCOLORTYPE, 0, 241, 4},
    {TPID_BITMAPIMAGETYPE, 0, 242, 16},
    {TPID_COMPOSEDIMAGETYPE, 0, 243, 16},
    {TPID_FLOAT, 0, TMT_FLOAT, 4},
    {TPID_FLOATLIST, 0, TMT_FLOATLIST, -1},
};

static int GetThemePrimitiveID(int lSymbolVal, int lType)
{
    PROPERTYMAP const* entry = property_map;

    int t = -1;
    for (int i = 0; i < 34; ++i, ++entry) {
        if (entry->lType == lType) {
            t = i;
            if (entry->lSymbolVal && entry->lSymbolVal == lSymbolVal)
                return i;
        }
    }

    if (t >= 0)
        return t;

    return -1;
}

static wchar_t c_szAMAP[] = L"AMAP";
static wchar_t c_szCMAP[] = L"CMAP";
static wchar_t c_szRMAP[] = L"RMAP";
static wchar_t c_szVMAP[] = L"VMAP";
static wchar_t c_szBCMAP[] = L"BCMAP";
static wchar_t c_szDocumentationElement[] = L"documentation";
static wchar_t c_szGlobalsElement[] = L"globals";
static wchar_t c_szSysmetsElement[] = L"sysmetrics";
static wchar_t c_szTimingFunctionElement_0[] = L"timingfunction";

static wchar_t const pszAppName[] = L"";

static _VSRECORD* GetNextVSRecord(_VSRECORD* pRec, int cbBuf, int* pcbPos)
{
    int size = sizeof(_VSRECORD);
    if (!pRec->uResID)
        size += pRec->cbData;

    size = Align8(size);
    int newPos = *pcbPos + size;
    if (newPos >= cbBuf)
        return nullptr;

    auto result = Advance(pRec, size);
    *pcbPos = newPos;
    return result;
}

static HRESULT _AllocateRecordPlusData(
    void const * pvData, unsigned cbData, _VSRECORD** ppRecord, int* pcbRecord)
{
    *ppRecord = nullptr;
    *pcbRecord = 0;

    auto size = sizeof(_VSRECORD) + cbData;
    auto pRec = static_cast<_VSRECORD*>(malloc(size));
    if (!pRec)
        return E_OUTOFMEMORY;

    memset(pRec, 0, size);
    pRec->iClass = -1;
    pRec->iPart = 0;
    pRec->iState = 0;
    pRec->cbData = cbData;

    memcpy(&pRec[1], pvData, cbData);

    *ppRecord = pRec;
    *pcbRecord = static_cast<int>(size);
    return S_OK;
}

static HRESULT _ParseBool(wchar_t const* pszValue, BOOL* pf)
{
    if (AsciiStrCmpI(pszValue, L"true") == 0 || AsciiStrCmpI(pszValue, L"1") == 0) {
        *pf = 1;
        return S_OK;
    }

    if (AsciiStrCmpI(pszValue, L"false") == 0 || AsciiStrCmpI(pszValue, L"0") == 0) {
        *pf = 0;
        return S_OK;
    }

    return E_INVALIDARG;
}

static HRESULT ParseFont(
    wchar_t const* pszValue, LOGFONTW* plf, unsigned long long cchComment, wchar_t* pszComment)
{
    return S_OK;
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT ParseIntegerTokenList(wchar_t const** ppsz, int* prgVal, int* pcVal)
{
    return S_OK;
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT ParseIntlist(wchar_t const* pszValue, int** pprgIntegers, int* pcIntegers)
{
    return S_OK;
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT GetResourceOffset(
    HMODULE hInst, wchar_t const* pszResType, wchar_t const* pszResName,
    unsigned* pdwOffset, unsigned* pdwBytes)
{
    *pdwOffset = 0;
    *pdwBytes = 0;

    HRSRC res = FindResourceExW(hInst, pszResType, pszResName, 0);
    if (!res)
        return MakeErrorLast();

    DWORD size = SizeofResource(hInst, res);
    if (size == 0)
        return MakeErrorLast();

    *pdwOffset = (uintptr_t)res - (uintptr_t)hInst;
    *pdwBytes = size;
    return S_OK;
}

static HRESULT ParseIntegerToken(wchar_t const** ppsz, wchar_t const* pszDelim, int* pnValue)
{
    *pnValue = 0;
    return S_OK;
}

static HRESULT AllocImageFileRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return E_ABORT;
}

static HRESULT AllocImageFileResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned int uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    auto pRec = static_cast<_VSRECORD*>(malloc(sizeof(_VSRECORD)));
    if (!pRec)
        return E_OUTOFMEMORY;

    pvsr->uResID = uResID;
    pvsr->type = VSRT_BITMAP;
    pvsr->strValue = pszValue;

    pRec->uResID = uResID;
    pRec->cbData = cbType;

    *ppRecord = pRec;
    *pcbRecord = 32;
    return S_OK;
}

static HRESULT LoadImageFileRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    void* data;
    unsigned v11;

    HRESULT hr = GetPtrToResource(hInst, L"IMAGE",
                                  MAKEINTRESOURCEW(pRecord->uResID),
                                  &data, &v11);

    if (hr >= 0) {
        if (*(WORD *)data != 0x4D42) {
            *(void**)pvData = data;
            *((DWORD*)pvData + 2) = v11;
            *pcbData = 16;
            return hr;
        }
        if (!((*((WORD *)data + 14) - 24) & 0xFFF7)) {
            *(void**)pvData = data; +14;
            *((DWORD*)pvData + 2) = 0;
            *pcbData = 16;
            return hr;
        }
    }

    *pcbData = 0;
    return E_FAIL;
}

static void DestroyImageFileRes(void* pvData)
{
}

static HRESULT AllocEnumRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int value = *(int*)pszValue;
    return _AllocateRecordPlusData(&value, sizeof(value), ppRecord, pcbRecord);
}

static HRESULT AllocStringRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int length = lstrlenW(pszValue);
    return _AllocateRecordPlusData(pszValue, 2 * (length + 1), ppRecord, pcbRecord);
}

static HRESULT AllocIntRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int value;
    ENSURE_HR(ParseIntegerToken(&pszValue, L", ", &value));
    return _AllocateRecordPlusData(&value, sizeof(value), ppRecord, pcbRecord);
}

static HRESULT AllocAtlasInputImageRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocAtlasImageResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned int uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT LoadDiskStreamRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    unsigned offset;
    unsigned length;
    ENSURE_HR(GetResourceOffset(
        hInst, L"STREAM", MAKEINTRESOURCEW(pRecord->uResID), &offset, &length));

    if (*pcbData >= 8)
        *pcbData = 8;

    memcpy(pvData, &offset, *pcbData);
    return S_OK;
}

static HRESULT AllocBoolRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocRGBRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocRectRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocPositionRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocFontRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocIntlistRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocAnimationRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocFloatRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocFloatListRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocStringResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    auto pRec = static_cast<_VSRECORD*>(malloc(sizeof(_VSRECORD)));
    if (!pRec)
        return E_OUTOFMEMORY;

    pvsr->uResID = uResID;
    pvsr->type = VSRT_STRING;
    pvsr->strValue = pszValue;

    if (cbType == -1)
        cbType = 2 * (lstrlenW(pvsr->strValue.c_str()) + 1);
    pRec->uResID = uResID;
    pRec->cbData = cbType;

    *ppRecord = pRec;
    *pcbRecord = sizeof(_VSRECORD);
    return S_OK;
}

static HRESULT AllocFontResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    if (cbType < sizeof(LOGFONTW))
        return E_INVALIDARG;

    wchar_t comment[1000] = {};

    LOGFONTW lf;
    ENSURE_HR(ParseFont(pszValue, &lf, 1000, comment));
    ENSURE_HR(AllocStringResRecord(pszValue, ppRecord, cbType, pcbRecord, uResID, pvsr, pEcx));

    pvsr->strComment = comment;
    return S_OK;
}

static HRESULT AllocIntlistResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    LPVOID lpMem;
    int v13;

    if (ParseIntlist(pszValue, (int **)&lpMem, &v13) >= 0) {
        cbType = 4 * v13;
        free(lpMem);
    }

    return AllocStringResRecord(pszValue, ppRecord, cbType, pcbRecord, uResID, pvsr, pEcx);
}

static HRESULT LoadStringRes(HMODULE hInst, _VSRECORD* pRecord, wchar_t* pszData, int cchData)
{
    *pszData = 0;
    return LoadStringW(hInst, pRecord->uResID, pszData, cchData) == 0 ? 0x80070490 : 0;
}

static HRESULT LoadStringRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    return LoadStringRes(hInst, pRecord, (wchar_t*)pvData, *pcbData / sizeof(wchar_t));
}

static HRESULT LoadIntRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    wchar_t const* ppsz = buffer;

    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));
    return ParseIntegerToken(&ppsz, L", ", (int*)pvData);
}

static HRESULT LoadBoolRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));
    return _ParseBool(buffer, (BOOL*)pvData);
}

static HRESULT LoadRectRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    int values[4];
    int pcVal = 4;
    wchar_t* ppsz = buffer;
    ENSURE_HR(ParseIntegerTokenList((const wchar_t **)&ppsz, values, &pcVal));

    auto pRect = (RECT *)pvData;
    pRect->left = values[0];
    pRect->top = values[1];
    pRect->right = values[2];
    pRect->bottom = values[3];
    return S_OK;
}

static HRESULT LoadRGBRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    wchar_t* ppsz = buffer;
    int values[3];
    int pcVal = 3;
    ENSURE_HR(ParseIntegerTokenList((const wchar_t **)&ppsz, values, &pcVal));

    *(unsigned*)pvData = (values[0] & 0xFF) | ((values[2] & 0xFF) << 16) | ((values[1] & 0xFF) << 8);
    return S_OK;
}

static HRESULT AllocStreamResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocDiskStreamResRecord(
    wchar_t const* pszValue, _VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, _VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT LoadStreamRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    void* ptr = nullptr;
    int size;
    ENSURE_HR(GetPtrToResource(hInst, L"STREAM", MAKEINTRESOURCEW(pRecord->uResID), &ptr, (unsigned*)&size));

    if (size > *pcbData)
        size = *pcbData;

    memcpy(pvData, ptr, size);
    *pcbData = size;

    return S_OK;
}

static HRESULT LoadPositionRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    wchar_t* ppsz = buffer;
    int values[2];
    int pcVal = 2;

    ENSURE_HR(ParseIntegerTokenList((const wchar_t **)&ppsz, values, &pcVal));

    auto pt = (POINT*)pvData;
    pt->x = values[0];
    pt->y = values[1];
    return S_OK;
}

static HRESULT LoadIntlistRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[256];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 256));

    int* integers;
    int pcIntegers;
    ENSURE_HR(ParseIntlist(buffer, &integers, &pcIntegers));

    size_t cBytes = 4 * pcIntegers;
    if (cBytes > *pcbData)
        cBytes = *pcbData;

    memcpy(pvData, integers, cBytes);
    *pcbData = cBytes;
    free(integers);

    return S_OK;
}

static HRESULT LoadFontRes(HMODULE hInst, _VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData || *pcbData < sizeof(LOGFONTW))
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));
    return ParseFont(buffer, (LOGFONTW*)pvData, 0, nullptr);
}

struct PARSETABLE
{
    THEMEPRIMITIVEID tpi;
    HRESULT(*pfnAlloc)(const wchar_t *, _VSRECORD **, int, int *, VSERRORCONTEXT *);
    HRESULT(*pfnAllocRes)(const wchar_t *, _VSRECORD **, int, int *, unsigned int, _VSRESOURCE *, VSERRORCONTEXT *);
    HRESULT(*pfnLoad)(HINSTANCE__ *, _VSRECORD *, void *, int *);
    void(*pfnUnload)(void *);
};

static PARSETABLE parse_table[34] = {
    {TPID_BITMAPIMAGE,           AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE1,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE2,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE3,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE4,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE5,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE6,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_BITMAPIMAGE7,          AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_STOCKBITMAPIMAGE,      AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_GLYPHIMAGE,            AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_ATLASIMAGE,            AllocImageFileRecord, AllocAtlasImageResRecord, LoadDiskStreamRes},
    {TPID_ATLASINPUTIMAGE,       AllocAtlasInputImageRecord, AllocStringResRecord, LoadStringRes},
    {TPID_ENUM,                  AllocEnumRecord, AllocStringResRecord, LoadIntRes},
    {TPID_STRING,                AllocStringRecord, AllocStringResRecord, LoadStringRes},
    {TPID_INT,                   AllocIntRecord, AllocStringResRecord, LoadIntRes},
    {TPID_BOOL,                  AllocBoolRecord, AllocStringResRecord, LoadBoolRes},
    {TPID_COLOR,                 AllocRGBRecord, AllocStringResRecord, LoadRGBRes},
    {TPID_MARGINS,               AllocRectRecord, AllocStringResRecord, LoadRectRes},
    {TPID_FILENAME,              AllocStringRecord, AllocStringResRecord, LoadStringRes},
    {TPID_SIZE,                  AllocIntRecord, AllocStringResRecord, LoadIntRes},
    {TPID_POSITION,              AllocPositionRecord, AllocStringResRecord, LoadPositionRes},
    {TPID_RECT,                  AllocRectRecord, AllocStringResRecord, LoadRectRes},
    {TPID_FONT,                  AllocFontRecord, AllocFontResRecord, LoadFontRes},
    {TPID_INTLIST,               AllocIntlistRecord, AllocIntlistResRecord, LoadIntlistRes},
    {TPID_DISKSTREAM,            AllocImageFileRecord, AllocDiskStreamResRecord, LoadDiskStreamRes},
    {TPID_STREAM,                AllocImageFileRecord, AllocStreamResRecord, LoadStreamRes},
    {TPID_ANIMATION,             AllocAnimationRecord},
    {TPID_TIMINGFUNCTION,        AllocAnimationRecord},
    {TPID_SIMPLIFIEDIMAGETYPE},
    {TPID_HIGHCONTRASTCOLORTYPE, AllocEnumRecord},
    {TPID_BITMAPIMAGETYPE,       AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_COMPOSEDIMAGETYPE,     AllocImageFileRecord, AllocImageFileResRecord, LoadImageFileRes, DestroyImageFileRes},
    {TPID_FLOAT,                 AllocFloatRecord},
    {TPID_FLOATLIST,             AllocFloatListRecord},
};

static HRESULT _GenerateEmptySection(
    IParserCallBack* pfnCB, wchar_t const* pszAppName, wchar_t const* pszClassName,
    int iPartId, int iStateId)
{
    int begin = pfnCB->GetNextDataIndex();
    int value = 0;
    if (FAILED(pfnCB->AddData(12, 12, &value, sizeof(value))))
        return E_FAIL;

    int length = pfnCB->GetNextDataIndex() - begin;
    return pfnCB->AddIndex(pszAppName, pszClassName, iPartId, iStateId, begin, length);
}

static HRESULT _TerminateSection(
    IParserCallBack* pfnCB, wchar_t const* pszApp, wchar_t const* pszClass,
    int iPart, int iState, int iStartOfSection)
{
    int value = 0;
    if (FAILED(pfnCB->AddData(12, 12, &value, sizeof(value))))
        return E_FAIL;

    int length = pfnCB->GetNextDataIndex() - iStartOfSection;
    return pfnCB->AddIndex(pszApp, pszClass, iPart, iState, iStartOfSection, length);
}

void _ParseClassName(
    wchar_t const* pszClassSpec, wchar_t* pszAppNameBuf, unsigned int cchAppNameBuf,
    wchar_t* pszClassNameBuf, unsigned int cchClassNameBuf)
{
    wchar_t const* classSpecPtr;
    wchar_t* v9;
    unsigned int v10;
    signed __int64 v11;

    *pszAppNameBuf = 0;
    classSpecPtr = pszClassSpec;
    v9 = pszClassNameBuf;
    v10 = 0;
    if (*pszClassSpec)
    {
        v11 = 0;
        do
        {
            if (v10 >= 0x103)
                break;
            if (58 == *classSpecPtr && 58 == classSpecPtr[1])
            {
                StringCchCopyNW(pszAppNameBuf, cchAppNameBuf, pszClassSpec, (signed int)(v11 >> 1));
                classSpecPtr += 2;
                v9 = pszClassNameBuf;
                v11 += 4;
                v10 = 0;
            }
            v11 += 2;
            *v9 = *classSpecPtr;
            ++classSpecPtr;
            ++v9;
            ++v10;
        } while (*classSpecPtr);
    }
    *v9 = 0;
}

static HRESULT LoadVSRecordData(HMODULE hInstVS, _VSRECORD* pRecord, void* pvBuf, int* pcbBuf)
{
    if (!pRecord || !pcbBuf)
        return E_INVALIDARG;

    if (!pvBuf) {
        *pcbBuf = pRecord->cbData;
        return S_OK;
    }

    if (*pcbBuf < 0 || pRecord->cbData < 0 || pRecord->cbData > *pcbBuf)
        return E_INVALIDARG;

    int id = GetThemePrimitiveID(pRecord->lSymbolVal, pRecord->lType);
    if (id < 0)
        return E_FAIL;

    if (pRecord->uResID) {
        int cbType = property_map[id].cbType;
        if (cbType > 0 && cbType != pRecord->cbData)
            return E_ABORT;
        return parse_table[id].pfnLoad(hInstVS, pRecord, pvBuf, pcbBuf);
    } else {
        memcpy(pvBuf, &pRecord[1], pRecord->cbData);
        return S_OK;
    }
}

static void UnloadVSRecordData(_VSRECORD* pRecord, void* pvBuf)
{
    if (pRecord && pvBuf) {
        int id = GetThemePrimitiveID(pRecord->lSymbolVal, pRecord->lType);
        if (id >= 0) {
            if (auto unload = parse_table[id].pfnUnload)
                unload(pvBuf);
        }
    }
}

static BOOL IsHighContrastMode()
{
    return FALSE;
}

static DWORD MapEnumToSysColor(HIGHCONTRASTCOLOR hcColor)
{
    switch (hcColor) {
    case HCC_COLOR_ACTIVECAPTION: return GetSysColor(COLOR_ACTIVECAPTION);
    case HCC_COLOR_CAPTIONTEXT: return GetSysColor(COLOR_CAPTIONTEXT);
    case HCC_COLOR_BTNFACE: return GetSysColor(COLOR_BTNFACE);
    case HCC_COLOR_BTNTEXT: return GetSysColor(COLOR_BTNTEXT);
    case HCC_COLOR_DESKTOP: return GetSysColor(COLOR_BACKGROUND);
    case HCC_COLOR_GRAYTEXT: return GetSysColor(COLOR_GRAYTEXT);
    case HCC_COLOR_HOTLIGHT: return GetSysColor(COLOR_HOTLIGHT);
    case HCC_COLOR_INACTIVECAPTION: return GetSysColor(COLOR_INACTIVECAPTION);
    case HCC_COLOR_INACTIVECAPTIONTEXT: return GetSysColor(COLOR_INACTIVECAPTIONTEXT);
    case HCC_COLOR_HIGHLIGHT: return GetSysColor(COLOR_HIGHLIGHT);
    case HCC_COLOR_HIGHLIGHTTEXT: return GetSysColor(COLOR_HIGHLIGHTTEXT);
    case HCC_COLOR_WINDOW: return GetSysColor(COLOR_WINDOW);
    case HCC_COLOR_WINDOWTEXT: return GetSysColor(COLOR_WINDOWTEXT);
    default: return 0;
    }
}

template<typename T>
T SwapRGB(T value)
{
    return ((value & 0xFF) << 16) | (value & 0xFF00) | ((value >> 16) & 0xFF);
}

static char* GetBitmapBits(BITMAPINFOHEADER* header)
{
    return reinterpret_cast<char*>(header) + header->biSize + 4 * header->biClrUsed;
}

static unsigned Colorize(unsigned rgbaSrc, unsigned rgbTint)
{
    unsigned alpha = rgbaSrc >> 24;
    unsigned color = rgbTint | (alpha << 24);

    if (alpha == 0)
        return 0;
    if (alpha == 0xFF)
        return color;

    unsigned r = (color >> 16) & 0xFF;
    unsigned g = (color >> 8) & 0xFF;
    unsigned b = (color & 0xFF);

    r = alpha * r + 0x80;
    g = alpha * g + 0x80;
    b = alpha * b + 0x80;

    r = (r + (r / 256)) / 256;
    g = (g + (g / 256)) / 256;
    b = (b + (b / 256)) / 256;

    return (alpha << 24) | (r << 16) | (g << 8) | b;
}

void ColorizeGlyphByAlpha(char* pBytes, BITMAPINFOHEADER* pBitmapHdr, unsigned int dwRGBColor)
{
    int const tint = SwapRGB(dwRGBColor);
    unsigned const* src = reinterpret_cast<unsigned*>(GetBitmapBits(pBitmapHdr));
    unsigned* dst = reinterpret_cast<unsigned*>(pBytes);

    for (int i = 0; i < pBitmapHdr->biWidth * pBitmapHdr->biHeight; ++i)
        *dst++ = Colorize(*src++, tint);
}

static void ColorizeAndComposeImages(
    char* pDstImageBytes, char* pSrcBGImageBytes, char* pSrcFGImageBytes,
    SIZE imageSize, unsigned int* pdwBGColor, unsigned int* pdwFGColor)
{
    auto srcFg = reinterpret_cast<unsigned*>(pSrcFGImageBytes);
    auto srcBg = reinterpret_cast<unsigned*>(pSrcBGImageBytes);
    auto dst = reinterpret_cast<unsigned*>(pDstImageBytes);

    for (long i = imageSize.cx * imageSize.cy; i; --i) {
        unsigned fgClr;
        if (pdwFGColor)
            fgClr = Colorize(*srcFg, SwapRGB(*pdwFGColor));
        else
            fgClr = *srcFg;
        ++srcFg;

        unsigned bgClr;
        if (pdwBGColor)
            bgClr = Colorize(*srcBg, SwapRGB(*pdwBGColor));
        else
            bgClr = *srcBg;
        ++srcBg;

        *dst = bgClr;
        if (fgClr & 0xFF000000) {
            unsigned v17 = ~((fgClr >> 24) & 0xFF);
            if (v17) {
                unsigned v18 = v17 * (bgClr & 0xFF00FF) + 0x800080;
                unsigned v19 = v17 * ((bgClr >> 8) & 0xFF00FF) + 0x800080;
                unsigned y = (v19 + ((v19 >> 8) & 0xFF00FF));
                unsigned z = (v19 + ((v19 >> 8) & 0xFFFF00FF));
                unsigned g = (v18 + ((v18 >> 8) & 0xFF00FF));
                unsigned x = (y ^ (z ^ (g >> 8)) & 0xFF00FF);
                *dst = fgClr + x;
            } else {
                *dst = fgClr;
            }
        }
        ++dst;
    }
}

void ColorizeGlyphByAlphaComposition(
    char* pBytes, tagBITMAPINFOHEADER* pBitmapHdr, int iImageCount,
    unsigned* pdwGlyphBGColor, unsigned int* pdwGlyphColor)
{
    SIZE size;
    size.cx = pBitmapHdr->biWidth;
    size.cy = pBitmapHdr->biHeight / (2 * iImageCount);

    int const byteCount = 4 * size.cx * size.cy;
    char* srcFg = GetBitmapBits(pBitmapHdr);
    char* srcBg = srcFg + byteCount;

    for (int i = 0; i < iImageCount; ++i) {
        ColorizeAndComposeImages(pBytes, srcBg, srcFg, size, pdwGlyphBGColor, pdwGlyphColor);
        pBytes += byteCount;
        srcFg += 2 * byteCount;
        srcBg += 2 * byteCount;
    }
}

void Convert24to32BPP(char* pBytes, BITMAPINFOHEADER* pBitmapHdr)
{
    long const stride = pBitmapHdr->biWidth;
    long const v5 = 3 * (stride + 1) & 0xFFFFFFFC;

    auto const* src = (unsigned char*)GetBitmapBits(pBitmapHdr);
    for (long row = 0; row < pBitmapHdr->biHeight; ++row) {
        unsigned* pRow = (unsigned *)&pBytes[4 * row * stride];

        for (long col = 0; col < stride; ++col) {
            unsigned char r = src[3 * col];
            unsigned char g = src[3 * col + 1];
            unsigned char b = src[3 * col + 2];
            pRow[col] = 0xFF000000u | (b << 16) | (g << 8) | r;
        }

        src += v5;
    }
}

static HRESULT _ReadVSVariant(char* pbVariantList, int cbVariantList, int* pcbPos, wchar_t** ppszName, wchar_t** ppszSize, wchar_t** ppszColor)
{
    HRESULT v6; // edi@1
    int v10; // ebp@3
    wchar_t*** v11; // r14@3
    unsigned __int64 v12; // rsi@5
    wchar_t** v13; // r12@6
    signed __int64 v14; // ax@7
    wchar_t* v15; // rax@9
    HRESULT hr; // eax@12

    v6 = 0;
    if (cbVariantList < 4 || *pcbPos >= cbVariantList)
        return STRSAFE_E_END_OF_FILE;

    v10 = 0;
    v11 = &ppszName;
    while (1) {
        if ((unsigned __int64)v10 >= 3)
            return v6;
        v12 = *(DWORD *)&pbVariantList[*pcbPos];
        if (!(DWORD)v12)
            break;
        *pcbPos += 4;
        v13 = *v11;
        if (*v11)
        {
            v14 = 2 * v12;
            v15 = (wchar_t *)malloc(v14);
            *v13 = v15;
            if (!v15)
                return E_OUTOFMEMORY;
            StringCchCopyNW(v15, (unsigned int)v12, (const wchar_t *)&pbVariantList[*pcbPos], (unsigned int)(v12 - 1));
        }
        ++v10;
        ++v11;
        *pcbPos += Align8(2 * v12);
        if (*pcbPos >= cbVariantList)
            return v6;
    }
    hr = E_INVALIDARG;
    return hr;
}

HRESULT CVSUnpack::Initialize(HMODULE hInstSrc, int nVersion, int fGlobal, int fIsLiteVisualStyle)
{
    _hInst = hInstSrc;
    _nVersion = nVersion;
    _fGlobal = fGlobal;
    _fIsLiteVisualStyle = fIsLiteVisualStyle;

    unsigned size;

    HRESULT hr = S_OK;
    void* data;

    if (!nVersion) {
        ENSURE_HR(GetPtrToResource(
            hInstSrc, L"PACKTHEM_VERSION", MAKEINTRESOURCEW(1), &data, &size));
        _nVersion = *static_cast<short*>(data);
    }

    ENSURE_HR(GetPtrToResource(_hInst, c_szCMAP, c_szCMAP, &data, &size));

    if (data) {
        auto str = static_cast<wchar_t const*>(data);
        while (size > 0) {
            _rgClassNames.emplace_back(str);
            unsigned byteLen = Align8(2 * (_rgClassNames.back().length() + 1));
            size -= byteLen;
            str = Advance(str, byteLen);
        }
    }

    _cBitmaps = 1000;
    _rgBitmapIndices = new(std::nothrow) int[1000];
    if (!_rgBitmapIndices)
        return E_OUTOFMEMORY;

    unsigned __int64 v17 = (unsigned __int64)&_rgBitmapIndices[_cBitmaps];
    unsigned v18 = (4 * _cBitmaps + 3) >> 2;

    int* indexPtr = _rgBitmapIndices;
    if ((unsigned __int64)indexPtr > v17)
        v18 = 0;

    for (unsigned __int64 v16 = 0; v16 < v18; ++v16)
        *indexPtr++ = -1;

    _rgfPartiallyTransparent = new(std::nothrow) char[(_cBitmaps >> 3) + 1];
    if (!_rgfPartiallyTransparent)
        return E_OUTOFMEMORY;

    return hr;
}

HRESULT CVSUnpack::GetRootMap(void** ppvRMap, int* pcbRMap)
{
    *ppvRMap = nullptr;
    *pcbRMap = 0;

    if (!_pvRootMap)
        ENSURE_HR(GetPtrToResource(
            _hInst, c_szRMAP, c_szRMAP, &_pvRootMap, (unsigned*)&_cbRootMap));

    *ppvRMap = _pvRootMap;
    *pcbRMap = _cbRootMap;
    return S_OK;
}

HRESULT CVSUnpack::LoadRootMap(IParserCallBack* pfnCB)
{
    void* buf;
    int cbBuf;
    ENSURE_HR(GetRootMap(&buf, &cbBuf));

    int classIdx = _FindClass(c_szDocumentationElement);

    int pcbPos = 0;
    for (auto it = static_cast<_VSRECORD*>(buf); it; it = GetNextVSRecord(it, cbBuf, &pcbPos)) {
        if (it->iClass == classIdx)
            ENSURE_HR(_AddVSDataRecord(pfnCB, _hInst, it));
    }

    return S_OK;
}

HRESULT CVSUnpack::GetVariantMap(void** ppvVMap, int* pcbVMap)
{
    *ppvVMap = nullptr;
    *pcbVMap = 0;

    if (!_pvVariantMap)
        ENSURE_HR(GetPtrToResource(_hInst, c_szVMAP, c_szVMAP, &_pvVariantMap,
        (unsigned*)&_cbVariantMap));

    *ppvVMap = _pvVariantMap;
    *pcbVMap = _cbVariantMap;
    return S_OK;
}

HRESULT CVSUnpack::GetClassData(wchar_t const* pszColorVariant, wchar_t const* pszSizeVariant, void** pvMap, int* pcbMap)
{
    signed int v5; // edi@1
    signed int v10; // ebx@1
    int* v11; // r12@6
    int pcbVMap; // [sp+30h] [bp-30h]@5
    wchar_t* ppszName; // [sp+38h] [bp-28h]@8
    wchar_t* v15; // [sp+40h] [bp-20h]@8
    wchar_t* dst; // [sp+48h] [bp-18h]@8
    void* ppvVMap; // [sp+50h] [bp-10h]@5
    int pcbPos; // [sp+98h] [bp+38h]@6

    v5 = 0;
    v10 = -2147024809;
    if (pszColorVariant)
    {
        if (*pszColorVariant)
        {
            if (pszSizeVariant)
            {
                if (*pszSizeVariant)
                {
                    v10 = GetVariantMap(&ppvVMap, &pcbVMap);
                    if (v10 >= 0)
                    {
                        v11 = pcbMap;
                        pcbPos = 0;
                        do
                        {
                            if (v10 < 0)
                                break;
                            ppszName = 0;
                            v15 = 0;
                            dst = 0;
                            v10 = _ReadVSVariant((char*)ppvVMap, pcbVMap, &pcbPos, &ppszName, &dst, &v15);
                            if (v10 >= 0)
                            {
                                if (!AsciiStrCmpI(dst, pszSizeVariant) && !AsciiStrCmpI(v15, pszColorVariant))
                                {
                                    v5 = 1;
                                    v10 = GetPtrToResource(_hInst, L"VARIANT", ppszName, pvMap, (unsigned int *)v11);
                                }
                                free(ppszName);
                                ppszName = 0;
                                free(v15);
                                v15 = 0;
                                free(dst);
                            }
                        } while (!v5);
                        if (!v5)
                            v10 = -2147024809;
                    }
                }
            }
        }
    }
    return (unsigned int)v10;
}

HRESULT CVSUnpack::LoadClassDataMap(wchar_t const* pszColor, wchar_t const* pszSize, IParserCallBack* pfnCB)
{
    HRESULT hr = S_OK; // ebx@1
    signed int currClassId; // er12@2
    __int32 sysmetsElementId; // eax@2 MAPDST
    int iStartOfSection; // er15@2
    _VSRECORD* pRec; // rdi@2
    bool hasDelayedRecords; // er13@2
    __int64 isNewClass; // rcx@4
    unsigned int isNewPart; // er8@4
    bool isNewState; // zf@4
    __int64 v16; // rax@17
    int v17; // eax@31
    HRESULT v19; // eax@39
    unsigned int pcbMap; // [sp+20h] [bp-A8h]@1
    int iState; // [sp+30h] [bp-98h]@2
    bool hasPlateauRecords; // [sp+34h] [bp-94h]@2
    int iPart; // [sp+38h] [bp-90h]@2
    bool hasGlobalsSection; // [sp+3Ch] [bp-8Ch]@2
    bool hasSysmetsSection; // [sp+40h] [bp-88h]@2
    __int32 globalsElementId; // [sp+44h] [bp-84h]@2
    int cbBuf; // [sp+48h] [bp-80h]@1
    void* pvMap; // [sp+58h] [bp-70h]@1
    int pcbPos; // [sp+60h] [bp-68h]@2
    wchar_t pszClass[230]; // [sp+70h] [bp-58h]@2
    wchar_t pszApp[260]; // [sp+248h] [bp+180h]@2

    ENSURE_HR(GetClassData(pszColor, pszSize, &pvMap, &cbBuf));

    pcbPos = 0;
    iPart = 0;
    iState = 0;
    currClassId = -1;
    hasGlobalsSection = 0;
    hasSysmetsSection = 0;
    globalsElementId = _FindClass(c_szGlobalsElement);
    sysmetsElementId = _FindClass(c_szSysmetsElement);
    iStartOfSection = -1;
    pRec = (_VSRECORD *)pvMap;
    pszClass[0] = 0;
    pszApp[0] = 0;
    _pbClassData = pvMap;
    _cbClassData = cbBuf;
    hasDelayedRecords = 1;
    hasPlateauRecords = 0;
    if (pRec) {
        while (hr >= 0) {
            isNewClass = currClassId != pRec->iClass;
            isNewPart = iPart != pRec->iPart;
            isNewState = iState != pRec->iState;

            if (isNewClass || isNewPart || isNewState) {
                if (hasDelayedRecords)
                    ENSURE_HR(_FlushDelayedRecords(pfnCB));

                ENSURE_HR(_AddScaledBackgroundDataRecord(pfnCB));
                if (hasPlateauRecords)
                    ENSURE_HR(_FlushDelayedPlateauRecords(pfnCB));

                if (iStartOfSection >= 0)
                    ENSURE_HR(_TerminateSection(pfnCB, pszApp, pszClass, iPart, iState, iStartOfSection));

                pszApp[0] = 0;
                pszClass[0] = 0;
                if (pRec->iClass <= -1 || pRec->iClass >= _rgClassNames.size())
                    return E_ABORT;

                _ParseClassName(_rgClassNames[pRec->iClass].c_str(), pszApp, 260, pszClass, 230);

                hasDelayedRecords = 0;
                hasPlateauRecords = 0;
                if (pRec->iClass == globalsElementId) {
                    hasGlobalsSection = 1;
                } else if (pRec->iClass == sysmetsElementId) {
                    hasSysmetsSection = 1;
                } else {
                    if (!hasGlobalsSection) {
                        hr = _GenerateEmptySection(pfnCB, nullptr, L"globals", 0, 0);
                        hasGlobalsSection = hr >= 0;
                        if (hr < 0)
                            return hr;
                    }

                    if (!hasSysmetsSection) {
                        hr = _GenerateEmptySection(pfnCB, nullptr, L"SysMetrics", 0, 0);
                        hasSysmetsSection = hr >= 0;
                        if (hr < 0)
                            return hr;
                    }

                    if (isNewClass && (pRec->iPart || pRec->iState)) {
                        ENSURE_HR(_GenerateEmptySection(pfnCB, pszApp, pszClass, 0, 0));
                        v17 = 0;
                    } else {
                        v17 = iState;
                    }

                    if (isNewPart && pRec->iState && !v17)
                        ENSURE_HR(_GenerateEmptySection(pfnCB, pszApp, pszClass, pRec->iPart, 0));
                }

                currClassId = pRec->iClass;
                iPart = pRec->iPart;
                iState = pRec->iState;
                iStartOfSection = pfnCB->GetNextDataIndex();
            }

            if (_DelayRecord(pRec)) {
                hr = _SaveRecord(pRec);
                hasDelayedRecords = 1;
            } else if (pRec->lSymbolVal >= TMT_5131 && pRec->lSymbolVal <= TMT_5142) {
                hr = _SavePlateauRecord(pRec);
                hasPlateauRecords = 1;
            } else if (pRec->iClass == globalsElementId && pRec->lSymbolVal >= 5128 && pRec->lSymbolVal <= 5130)
                hr = _InitializePlateauPpiMapping(pRec);
            else
                hr = _AddVSDataRecord(pfnCB, _hInst, pRec);

            pRec = GetNextVSRecord(pRec, cbBuf, &pcbPos);
            if (!pRec)
                break;
        }
    }

    ENSURE_HR(hr);

    if (hasDelayedRecords)
        ENSURE_HR(_FlushDelayedRecords(pfnCB));
    ENSURE_HR(_AddScaledBackgroundDataRecord(pfnCB));

    if (hasPlateauRecords)
        ENSURE_HR(_FlushDelayedPlateauRecords(pfnCB));

    if (iStartOfSection >= 0)
        ENSURE_HR(_TerminateSection(pfnCB, pszApp, pszClass, iPart, iState, iStartOfSection));

    return hr;
}

HRESULT CVSUnpack::LoadBaseClassDataMap(IParserCallBack* pfnCB)
{
    int* ptr;
    unsigned int size;

    ENSURE_HR(GetPtrToResource(_hInst, c_szBCMAP, c_szBCMAP, (void **)&ptr, &size));

    if (ptr && size) {
        int count = *ptr++;
        for (int i = 0; i < count; ++ptr)
            ENSURE_HR(pfnCB->AddBaseClass(i++, *ptr));
    }

    return S_OK;
}

int CVSUnpack::_FindClass(wchar_t const* pszClass)
{
    int idx = 0;
    for (auto const& str : _rgClassNames) {
        if (AsciiStrCmpI(str.c_str(), pszClass) == 0)
            return idx;
        ++idx;
    }

    return 0xFFFFFFFF;
}

HRESULT CVSUnpack::LoadAnimationDataMap(IParserCallBack* pfnCB)
{
    void* pbBuf = nullptr;
    unsigned cbBuf = 0;
    ENSURE_HR(GetPtrToResource(_hInst, c_szAMAP, c_szAMAP, &pbBuf, &cbBuf));
    if (!pbBuf || !cbBuf)
        return S_OK;

    int const timingClass = _FindClass(c_szTimingFunctionElement_0);

    HRESULT hr = S_OK;
    int currClass = 0;
    int currPart = 0;

    int pos = 0;
    for (auto pRec = static_cast<_VSRECORD*>(pbBuf); pRec != nullptr;
         pRec = GetNextVSRecord(pRec, cbBuf, &pos)) {
        if (hr < 0)
            break;

        if (currClass != pRec->iClass) {
            currClass = pRec->iClass;
            hr = _GenerateEmptySection(pfnCB, pszAppName, _rgClassNames[pRec->iClass].c_str(), 0, 0);
        }

        if (currClass != timingClass && currPart != pRec->iPart) {
            currPart = pRec->iPart;
            hr = _GenerateEmptySection(
                pfnCB,
                pszAppName,
                _rgClassNames[pRec->iClass].c_str(),
                pRec->iPart,
                0);
        }

        if (hr >= 0) {
            int index = pfnCB->GetNextDataIndex();
            hr = _AddVSDataRecord(pfnCB, _hInst, pRec);
            if (hr >= 0)
                hr = _TerminateSection(
                    pfnCB,
                    pszAppName,
                    _rgClassNames[pRec->iClass].c_str(),
                    pRec->iPart,
                    pRec->iState,
                    index);
        }
    }

    return hr;
}

HRESULT CVSUnpack::_FindVSRecord(
    void* pvRecBuf, int cbRecBuf, int iClass, int iPart, int iState,
    int lSymbolVal, _VSRECORD** ppRec)
{
    int pcbPos = 0;

    auto pRec = (_VSRECORD*)pvRecBuf;
    if (iClass > -1) {
        while (pRec) {
            if (pRec->iClass == iClass && pRec->iPart == iPart &&
                pRec->iState == iState && pRec->lSymbolVal == lSymbolVal) {
                *ppRec = pRec;
                return S_OK;
            }

            pRec = GetNextVSRecord(pRec, cbRecBuf, &pcbPos);
        }
    }

    *ppRec = nullptr;
    return 0x80070491;
}

HRESULT CVSUnpack::_GetPropertyValue(
    void* pvBits, int cbBits, int iClass, int iPart, int iState, int lSymbolVal,
    void* pvValue, int* pcbValue)
{
    _VSRECORD* pRecord;
    HRESULT hr = _FindVSRecord(pvBits, cbBits, iClass, iPart, iState, lSymbolVal, &pRecord);
    if (FAILED(hr))
        return hr;
    return LoadVSRecordData(_hInst, pRecord, pvValue, pcbValue);
}

HRESULT CVSUnpack::_GetImagePropertiesForHC(
    _IMAGEPROPERTIES** ppImageProperties, _HCIMAGEPROPERTIES* pHCImageProperties, int iImageCount)
{
    auto props = new(std::nothrow) _IMAGEPROPERTIES[iImageCount];
    if (!props)
        return E_OUTOFMEMORY;

    for (int i = 0; i < iImageCount; ++i) {
        DWORD borderColor = MapEnumToSysColor(
            (HIGHCONTRASTCOLOR)pHCImageProperties[i].lHCBorderColor);
        DWORD backgroundColor = MapEnumToSysColor(
            (HIGHCONTRASTCOLOR)pHCImageProperties[i].lHCBackgroundColor);

        props[i].dwBorderColor =
            0xFF000000u |
            ((borderColor & 0xFF) << 16) |
            (borderColor & 0xFF00) |
            ((borderColor >> 16) & 0xFF);
        props[i].dwBackgroundColor =
            0xFF000000u |
            ((backgroundColor & 0xFF) << 16) |
            (backgroundColor & 0xFF00) |
            ((backgroundColor >> 16) & 0xFF);
    }

    *ppImageProperties = props;

    return S_OK;
}

HRESULT CVSUnpack::_CreateImageFromProperties(
    _IMAGEPROPERTIES* pImageProperties, int iImageCount, MARGINS* pSizingMargins,
    MARGINS* pTransparentMargins, char** ppbNewBitmap, int* pcbNewBitmap)
{
    MARGINS const transparentMargin = pTransparentMargins ? *pTransparentMargins : MARGINS();

    int const width =
        transparentMargin.cxLeftWidth +
        transparentMargin.cxRightWidth +
        pSizingMargins->cxLeftWidth +
        pSizingMargins->cxRightWidth +
        1;
    int const height =
        transparentMargin.cyTopHeight +
        transparentMargin.cyBottomHeight +
        pSizingMargins->cyTopHeight +
        pSizingMargins->cyBottomHeight +
        1;

    DWORD const imageSize = iImageCount * 4 * width * height;
    size_t const cBytes = sizeof(_BITMAPHEADER) + imageSize;

    auto header = static_cast<_BITMAPHEADER*>(malloc(cBytes));
    if (!header)
        return E_OUTOFMEMORY;

    header->bmih.biXPelsPerMeter = 0;
    header->bmih.biYPelsPerMeter = 0;
    header->bmih.biClrImportant = 0;
    header->bmih.biSize = 40;
    header->bmih.biWidth = width;
    header->bmih.biPlanes = 1;
    header->bmih.biBitCount = 32;
    header->bmih.biSizeImage = imageSize;
    header->bmih.biHeight = iImageCount * height;
    header->bmih.biCompression = 3;
    header->bmih.biClrUsed = 3;
    header->masks[0] = 0xFF0000;
    header->masks[1] = 0xFF00;
    header->masks[2] = 0xFF;

    int* pixel = reinterpret_cast<int*>(header + 1);

    for (int i = iImageCount - 1; i >= 0; --i) {
        unsigned borderColor = pImageProperties[i].dwBorderColor;
        unsigned backColor = pImageProperties[i].dwBackgroundColor;

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                bool isTransparent =
                    x < transparentMargin.cxLeftWidth ||
                    x >= width - transparentMargin.cxRightWidth ||
                    y < transparentMargin.cyBottomHeight ||
                    y >= height - transparentMargin.cyTopHeight;
                bool isBorder =
                    x != transparentMargin.cxLeftWidth + pSizingMargins->cxLeftWidth ||
                    y != height - pSizingMargins->cyTopHeight - transparentMargin.cyTopHeight - 1;

                if (isTransparent) {
                    *pixel = 0;
                } else if (isBorder)
                    *pixel = borderColor;
                else
                    *pixel = backColor;

                ++pixel;
            }
        }
    }

    *ppbNewBitmap = reinterpret_cast<char*>(header);
    *pcbNewBitmap = static_cast<int>(cBytes);
    return S_OK;
}

HRESULT CVSUnpack::_EnsureBufferSize(unsigned cbBytes)
{
    if (cbBytes > _cbBuffer) {
        auto newBuffer = (char*)malloc(cbBytes);
        if (!newBuffer)
            return E_OUTOFMEMORY;
        free(_pBuffer);

        _pBuffer = newBuffer;
        _cbBuffer = cbBytes;
    }

    return S_OK;
}

bool CVSUnpack::_IsTrueSizeImage(_VSRECORD* pRec)
{
    SIZINGTYPE type = ST_STRETCH;
    int size = sizeof(type);
    if (_GetPropertyValue(_pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                          pRec->iState, TMT_SIZINGTYPE, &type, &size) < 0)
        return false;

    return (type & 0xFFFFFFFD) == 0 ? S_FALSE : S_OK;
}

HRESULT CVSUnpack::_ExpandVSRecordForColor(
    IParserCallBack* pfnCB, _VSRECORD* pRec, char* pbData, int cbData, bool* pfIsColor)
{
    *pfIsColor = false;

    for (auto const& entry : vscolorprops) {
        if (pRec->lSymbolVal == entry.lHCSymbolVal) {
            *pfIsColor = true;
            if (!IsHighContrastMode())
                return S_FALSE;

            DWORD value = MapEnumToSysColor(*reinterpret_cast<HIGHCONTRASTCOLOR*>(pbData));
            return pfnCB->AddData(entry.lSymbolVal, TMT_COLOR, &value, sizeof(value));
        }

        if (pRec->lSymbolVal == entry.lSymbolVal) {
            *pfIsColor = true;
            if (!IsHighContrastMode())
                return S_FALSE;

            _VSRECORD* r;
            HRESULT hr = _FindVSRecord(_pbClassData, _cbClassData, pRec->iClass,
                                       pRec->iPart, pRec->iState,
                                       entry.lHCSymbolVal, &r);
            return FAILED(hr) ? S_FALSE : S_OK;
        }
    }

    return S_OK;
}

HRESULT CVSUnpack::_ExpandVSRecordForMargins(
    IParserCallBack* pfnCB, _VSRECORD* pRec, char* pbData, int cbData, bool* pfIsMargins)
{
    *pfIsMargins = false;

    if (pRec->lSymbolVal == TMT_SIZINGMARGINS) {
        *pfIsMargins = true;

        auto sizingMargins = reinterpret_cast<MARGINS*>(pbData);

        MARGINS margins = {};
        int size = 16;

        HRESULT hr = _GetPropertyValue(
            _pbClassData, _cbClassData, pRec->iClass, pRec->iPart, pRec->iState,
            TMT_TRANSPARENTMARGINS, &margins, &size);
        if (hr < 0)
            return hr == 0x80070491 ? S_FALSE : hr;

        margins.cxLeftWidth += sizingMargins->cxLeftWidth;
        margins.cxRightWidth += sizingMargins->cxRightWidth;
        margins.cyTopHeight += sizingMargins->cyTopHeight;
        margins.cyBottomHeight += sizingMargins->cyBottomHeight;

        return pfnCB->AddData(pRec->lSymbolVal, pRec->lType, &margins, sizeof(margins));
    }

    if (pRec->lSymbolVal == TMT_TRANSPARENTMARGINS) {
        *pfIsMargins = true;
        return S_OK;
    }

    return S_FALSE;
}

HRESULT CVSUnpack::_ExpandVSRecordData(IParserCallBack* pfnCB, _VSRECORD* pRec, char* pbData, int cbData)
{
    HRESULT hr;
    if (_fIsLiteVisualStyle) {
        bool isColor;
        hr = _ExpandVSRecordForColor(pfnCB, pRec, pbData, cbData, &isColor);
        if (isColor)
            return hr;
        hr = _ExpandVSRecordForMargins(pfnCB, pRec, pbData, cbData, &isColor);
        if (isColor)
            return hr;
    }

    int bitmapIdx; // er15@11
    signed int v17; // er13@12
    char* v18; // rax@16
    char ePrimVal; // r13@28
    int v24; // edx@31
    int v25; // er12@31
    LPVOID v26; // rax@31
    _BITMAPHEADER* bmpInfoHeader; // rsi@44
    void* v44; // r15@46
    char v47; // r13@63
    rsize_t v49; // r15@73
    char* v50; // rax@73
    signed int v54; // eax@89
    char* v72; // rax@110
    int v74; // er15@115
    int v79; // er8@120
    bool v80; // cf@120
    char v82; // al@126

    char* pfncb; // [sp+60h] [bp-68h]@1
    int partiallyTransparent; // [sp+68h] [bp-60h]@10
    int iImageCount; // [sp+70h] [bp-58h]@1
    int pvBuf; // [sp+74h] [bp-54h]@63
    int pcbNewBitmap; // [sp+78h] [bp-50h]@1
    LPVOID lpMem; // [sp+80h] [bp-48h]@1
    char* v97; // [sp+88h] [bp-40h]@1
    _VSRECORD* pRecord; // [sp+C8h] [bp+0h]@115
    HDC hDC; // [sp+D0h] [bp+8h]@58

    _IMAGEPROPERTIES* pImageProperties = nullptr;
    bool pImageProperties_a = false;
    bool pImageProperties_b = false;
    char pImageProperties_c = 0;
    int pImageProperties_unknown = 0;

    hr = 0;
    v97 = 0;
    iImageCount = 0;
    lpMem = 0;
    pcbNewBitmap = 0;
    pfncb = 0;

    int sTypeNum;
    int v16;

    switch (pRec->lSymbolVal) {
    case TMT_IMAGEFILE:
        sTypeNum = 2;
        v16 = 0;
        goto LABEL_131;

    case TMT_IMAGEFILE1:
    case TMT_IMAGEFILE2:
    case TMT_IMAGEFILE3:
    case TMT_IMAGEFILE4:
    case TMT_IMAGEFILE5:
    case TMT_IMAGEFILE6:
    case TMT_IMAGEFILE7:
        sTypeNum = Map_IMAGEFILE_To_DIBDATA(pRec->lSymbolVal);
        v16 = 0;
        goto LABEL_131;

    case TMT_GLYPHIMAGEFILE:
        sTypeNum = TMT_GLYPHDIBDATA;
        v16 = 0;
        goto LABEL_131;

    case TMT_5100: {
        if (IsHighContrastMode())
            return 0;

        sTypeNum = TMT_DIBDATA;
        v16 = 1;
        pImageProperties = reinterpret_cast<_IMAGEPROPERTIES*>(pbData);
        iImageCount = cbData / sizeof(_IMAGEPROPERTIES);
        goto LABEL_98;
    }

    case TMT_5101: {
        if (!IsHighContrastMode())
            return 0;

        sTypeNum = TMT_DIBDATA;
        v16 = 1;
        iImageCount = cbData / sizeof(_HCIMAGEPROPERTIES);
        v97 = pbData;
        hr = _GetImagePropertiesForHC(
            &pImageProperties,
            reinterpret_cast<_HCIMAGEPROPERTIES*>(pbData),
            iImageCount);
        if (hr < 0)
            return hr;
        goto LABEL_98;
    }

    case TMT_5102:
        return IsHighContrastMode() != 0;

    case TMT_5143:
        sTypeNum = TMT_DIBDATA;
        pImageProperties_a = 1;
        v16 = 0;
        goto LABEL_131;

    case TMT_5149:
        sTypeNum = TMT_GLYPHDIBDATA;
        pImageProperties_a = 1;
        v16 = 0;
        goto LABEL_131;

    case TMT_COMPOSEDIMAGEFILE1:
    case TMT_COMPOSEDIMAGEFILE2:
    case TMT_COMPOSEDIMAGEFILE3:
    case TMT_COMPOSEDIMAGEFILE4:
    case TMT_COMPOSEDIMAGEFILE5:
    case TMT_COMPOSEDIMAGEFILE6:
    case TMT_COMPOSEDIMAGEFILE7:
        sTypeNum = Map_COMPOSEDIMAGEFILE_To_DIBDATA(pRec->lSymbolVal);
        pImageProperties_a = 1;
        v16 = 0;
        goto LABEL_131;

    LABEL_131:
        if (cbData < 16)
            return E_INVALIDARG;
        pfncb = pbData;
        break;

    default:
        return 1;
    }

LABEL_98:
    hr = 0;
    if (!_rgBitmapIndices) {
        hr = E_FAIL;
        goto LABEL_24;
    }

    bitmapIdx = -1;
    partiallyTransparent = 0;
    pvBuf = 0;
    bmpInfoHeader = 0;
    int indicesIdx = 0;
    v17 = 0;
    int v88;

    if (!v16) {
        indicesIdx = pRec->uResID - 501;
        if (indicesIdx < 0) {
            hr = E_FAIL;
            goto LABEL_24;
        }

        unsigned cBitmapsOld = _cBitmaps;
        if (indicesIdx < _cBitmaps) {
            bitmapIdx = _rgBitmapIndices[indicesIdx];
            partiallyTransparent = _rgfPartiallyTransparent[indicesIdx / CHAR_BIT] & (1 << (indicesIdx & (CHAR_BIT - 1)));
        } else {
            _cBitmaps = 2 * cBitmapsOld;
            int* newBitmapIndices = (int*)realloc(_rgBitmapIndices, sizeof(_rgBitmapIndices[0]) * (2 * cBitmapsOld));
            if (!newBitmapIndices) {
                hr = E_OUTOFMEMORY;
            } else {
                _rgBitmapIndices = newBitmapIndices;

                if (_cBitmaps > cBitmapsOld)
                    std::fill(&_rgBitmapIndices[cBitmapsOld], &_rgBitmapIndices[_cBitmaps], -1);

                auto v71 = (char*)realloc(_rgfPartiallyTransparent, (_cBitmaps / CHAR_BIT) + 1);
                if (!v71)
                    hr = E_OUTOFMEMORY;
                else
                    _rgfPartiallyTransparent = v71;
            }
        }

        v16 = 0;
        v17 = 0;
    }

    if (hr < 0)
        return hr;

    if (bitmapIdx == -1 || !_fGlobal) {
        if (v16) {
            MARGINS sizingMargins = {};
            int v90_ = sizeof(sizingMargins);
            auto v37_ = _GetPropertyValue(
                _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                pRec->iState, TMT_SIZINGMARGINS, &sizingMargins, &v90_);

            if (v37_ != 0x80070491)
                hr = v37_;
            if (hr < 0)
                goto LABEL_24;

            MARGINS transparentMargins = {};
            int size_ = sizeof(transparentMargins);
            hr = _GetPropertyValue(
                _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                pRec->iState, TMT_TRANSPARENTMARGINS, &transparentMargins, &size_);

            if ((hr + 2147483648) & 0x80000000 || hr == 0x80070491)
                hr = _CreateImageFromProperties(
                    pImageProperties,
                    iImageCount,
                    &sizingMargins,
                    &transparentMargins,
                    (char **)&lpMem,
                    &pcbNewBitmap);
            if (hr < 0)
                goto LABEL_36;

            bmpInfoHeader = (_BITMAPHEADER*)lpMem;
            v16 = pImageProperties_unknown;
        } else {
            auto hdr = (BITMAPHDR*)pbData;
            if (hdr->size) {
                if (!_pDecoder) {
                    _pDecoder = new(std::nothrow) CThemePNGDecoder();
                    if (!_pDecoder)
                        hr = E_OUTOFMEMORY;
                }

                if (hr < 0)
                    goto LABEL_24;
                hr = _pDecoder->ConvertToDIB((const char *)hdr->buffer, hdr->size, (int *)&pvBuf);
                if (hr < 0)
                    goto LABEL_24;
                v17 = 1;
                bmpInfoHeader = _pDecoder->GetBitmapHeader();
                v16 = pImageProperties_unknown;
            } else {
                bmpInfoHeader = (_BITMAPHEADER*)hdr->buffer;
            }
        }

        v44 = 0;
        int height = bmpInfoHeader->bmih.biHeight;
        if (pImageProperties_a)
            height /= 2;

        v88 = 4 * bmpInfoHeader->bmih.biWidth * height;
        partiallyTransparent = 0;
        if (v17) {
            partiallyTransparent = pvBuf;
            if (partiallyTransparent) {
                if (!v16)
                    _rgfPartiallyTransparent[indicesIdx / CHAR_BIT] |= 1 << (indicesIdx & (CHAR_BIT - 1));
            }
        } else {
            if (v16) {
                if (!v97 && iImageCount > 0) {
                    auto prop = pImageProperties;
                    for (int v12 = 0; v12 < iImageCount; ++v12, ++prop) {
                        if ((prop->dwBackgroundColor & 0xFF000000) != 0xFF000000 ||
                            (prop->dwBorderColor & 0xFF000000) != 0xFF000000) {
                            partiallyTransparent = 1;
                            break;
                        }
                    }
                }
            } else if (bmpInfoHeader->bmih.biBitCount == 32) {
                partiallyTransparent = 1;
                _rgfPartiallyTransparent[indicesIdx / CHAR_BIT] |= 1 << (indicesIdx & (CHAR_BIT - 1));
            }
        }

        if (!_fGlobal) {
            bitmapIdx = pfnCB->AddToDIBDataArray(nullptr, 0, 0);
            v54 = hr;
            if (bitmapIdx == -1)
                v54 = -2147467259;
            hr = v54;
            ePrimVal = 204;
            goto LABEL_29;
        }

        hDC = GetWindowDC(0);
        if (!hDC) {
            hr = -2147024882;
            goto LABEL_36;
        }

        if (partiallyTransparent && _fIsLiteVisualStyle) {
            if (pRec->lSymbolVal >= 3002 && pRec->lSymbolVal <= 3006 || pRec->lSymbolVal > 3007 && pRec->lSymbolVal <= 3010 || pRec->lSymbolVal > 5143 && pRec->lSymbolVal <= 5149 || (unsigned int)(pRec->lSymbolVal - 5152) <= 1 || _IsTrueSizeImage(pRec)) {
                goto LABEL_71a;
            } else {
                goto LABEL_71;
            }
        } else goto LABEL_71;

    LABEL_71a:
        pcbNewBitmap = 0;
        v47 = 0;
        pvBuf = 0;
        pImageProperties_c = 0;
        bool flag1 = false;
        bool flag2 = false;
        if (IsHighContrastMode()) {
            int pcbNewBitmap_ = 4;
            HIGHCONTRASTCOLOR c1;
            if (SUCCEEDED(_GetPropertyValue(
                _pbClassData,
                _cbClassData,
                pRec->iClass,
                pRec->iPart,
                pRec->iState,
                5121,
                &c1,
                &pcbNewBitmap_)))
            {
                pImageProperties_c = 1;
                flag1 = true;
                indicesIdx = c1;
                pcbNewBitmap = MapEnumToSysColor(c1);
            }

            HIGHCONTRASTCOLOR c2;
            if (SUCCEEDED(_GetPropertyValue(
                _pbClassData,
                _cbClassData,
                pRec->iClass,
                pRec->iPart,
                pRec->iState,
                5102,
                &c2,
                &pcbNewBitmap_)))
            {
                v47 = 1;
                flag2 = true;
                indicesIdx = c2;
                pvBuf = MapEnumToSysColor(c2);
            }

            if (flag1 || flag2)
                goto LABEL_126;
        }

        if (!pImageProperties_a) {
            v82 = 0;
        } else {
        LABEL_126:
            v82 = 1;
            pImageProperties_b = 1;
        }

        if (hr < 0)
            goto LABEL_70;
        if (!v82)
            goto LABEL_71;

        hr = _EnsureBufferSize(v88);
        if (hr < 0)
            goto LABEL_70;
        v72 = (char*)malloc(v88);
        pfncb = v72;
        if (!v72) {
            v44 = v72;
            goto LABEL_70;
        } else {
            if (pImageProperties_a)
            {
                v74 = 1;
                v88 = 4;
                iImageCount = 1;
                if (_FindVSRecord(
                    _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                    pRec->iState, TMT_IMAGECOUNT, &pRecord) >= 0)
                {
                    hr = LoadVSRecordData(_hInst, pRecord, &iImageCount, &v88);
                    if (hr < 0) {
                        v44 = pfncb;
                        goto LABEL_70;
                    }
                    v74 = iImageCount;
                    if (iImageCount <= 0)
                        hr = -2147024809;
                }

                if (hr >= 0) {
                    v79 = v74;
                    v44 = pfncb;
                    v80 = pImageProperties_c != 0;
                    pImageProperties_c = -pImageProperties_c;
                    ColorizeGlyphByAlphaComposition(
                        (char *)pfncb,
                        &bmpInfoHeader->bmih,
                        v79,
                        (unsigned int *)((unsigned __int64)&pcbNewBitmap & -(signed __int64)v80),
                        (unsigned int *)((unsigned __int64)&pvBuf & -(signed __int64)(v47 != 0)));
                    goto LABEL_70;
                }
                v44 = pfncb;
            } else {
                if (v47) {
                    ColorizeGlyphByAlpha((char *)v72, &bmpInfoHeader->bmih, pvBuf);
                    v44 = pfncb;
                } else {
                    v44 = v72;
                }
            }

            goto LABEL_70;
        }
    } else {
        ePrimVal = -41;
        goto LABEL_29;
    }

    goto LABEL_24;

LABEL_70:
    if (!pImageProperties_b) {
    LABEL_71:
        if (hr >= 0) {
            if (bmpInfoHeader->bmih.biBitCount == 32) {
                v49 = v88;
                v50 = (char *)malloc(v88);
                pfncb = v50;
                if (!v50) {
                    v44 = v50;
                    goto LABEL_75;
                }
                memcpy_s(v50, v49, (char *)bmpInfoHeader + 4 * bmpInfoHeader->bmih.biClrUsed + bmpInfoHeader->bmih.biSize, v49);
            } else {
                hr = _EnsureBufferSize(4 * bmpInfoHeader->bmih.biWidth * bmpInfoHeader->bmih.biHeight);
                if (hr < 0)
                    goto LABEL_75;
                v50 = (char *)malloc(v88);
                pfncb = v50;
                if (!v50) {
                    v44 = v50;
                    goto LABEL_75;
                }
                Convert24to32BPP(v50, &bmpInfoHeader->bmih);
            }
            v44 = pfncb;
        }
    }

LABEL_75:
    ReleaseDC(nullptr, hDC);
    if (!v44) {
        hr = -2147024882;
        goto LABEL_36;
    } else {
        ePrimVal = 212;
        short w = bmpInfoHeader->bmih.biWidth;
        short h = bmpInfoHeader->bmih.biHeight;
        if (pImageProperties_a)
            h /= 2;

        bitmapIdx = pfnCB->AddToDIBDataArray(v44, w, h);
        if (bitmapIdx == -1) {
            hr = E_FAIL;
        } else if (!pImageProperties_unknown) {
            _rgBitmapIndices[indicesIdx] = bitmapIdx;
        }
    }

LABEL_29:
    if (hr >= 0) {
        if (_fGlobal) {
            int values[] = {12, bitmapIdx, partiallyTransparent};
            hr = pfnCB->AddData(sTypeNum, ePrimVal, values, sizeof(values));
        } else {
            _BITMAPHEADER Dst = {};
            v24 = bmpInfoHeader->bmih.biWidth;
            Dst.bmih.biSize = sizeof(BITMAPINFOHEADER);
            Dst.bmih.biWidth = bmpInfoHeader->bmih.biWidth;
            Dst.bmih.biHeight = bmpInfoHeader->bmih.biHeight;
            Dst.bmih.biPlanes = 1;
            Dst.bmih.biBitCount = 32;
            Dst.bmih.biCompression = 3;
            Dst.bmih.biClrUsed = 3;
            Dst.bmih.biSizeImage = 4 * v24 * Dst.bmih.biHeight;
            Dst.masks[0] = 0xFF0000;
            Dst.masks[1] = 0xFF00;

            v25 = sizeof(TMBITMAPHEADER) + sizeof(_BITMAPHEADER) +
                4 * bmpInfoHeader->bmih.biWidth * Dst.bmih.biHeight;

            v26 = malloc(v25);
            if (v26) {
                auto v29 = (_BITMAPHEADER*)((char*)v26 + sizeof(TMBITMAPHEADER));
                if (bmpInfoHeader->bmih.biBitCount == 32) {
                    memcpy(v29, bmpInfoHeader, v25 - sizeof(TMBITMAPHEADER));
                } else {
                    *v29 = *bmpInfoHeader;
                    v29->masks[2] = 0xFF;
                    Convert24to32BPP((char *)v26 + 64, &bmpInfoHeader->bmih);
                }
            } else {
                hr = E_OUTOFMEMORY;
            }

            if (hr >= 0) {
                auto hdr = static_cast<TMBITMAPHEADER*>(v26);
                hdr->dwSize = 12;
                hdr->iBitmapIndex = bitmapIdx;
                hdr->fPartiallyTransparent = partiallyTransparent;
                hr = pfnCB->AddData(sTypeNum, 2, v26, v25);
                free(v26);
            }
        }
    }

LABEL_36:
    if (lpMem)
        free(lpMem);

LABEL_24:
    if (v97) {
        if (pImageProperties)
            free(pImageProperties);
    }

    return hr;
}

HRESULT CVSUnpack::_AddVSDataRecord(IParserCallBack* pfnCB, HMODULE hInst, _VSRECORD* pRec)
{
    char localBuffer[256];
    int pcbBuf = pRec->cbData;

    char* pvBuf = localBuffer;
    if (pRec->cbData > 256) {
        pvBuf = new(std::nothrow) char[pRec->cbData];
        if (!pvBuf)
            return E_OUTOFMEMORY;
    }

    HRESULT hr = LoadVSRecordData(hInst, pRec, pvBuf, &pcbBuf);
    if (hr >= 0) {
        hr = _ExpandVSRecordData(pfnCB, pRec, pvBuf, pcbBuf);
        if (hr < 0) {
            UnloadVSRecordData(pRec, pvBuf);
        } else if (hr == S_FALSE) {
            hr = pfnCB->AddData(pRec->lSymbolVal, pRec->lType, pvBuf, pcbBuf);
            if (hr < 0) {
                UnloadVSRecordData(pRec, pvBuf);
                hr = E_FAIL;
            }
        }
    }

    if (pvBuf != localBuffer)
        delete[] pvBuf;

    return hr;
}

HRESULT CVSUnpack::_InitializePlateauPpiMapping(_VSRECORD* pRec)
{
    int value;
    int size = 4;
    ENSURE_HR(LoadVSRecordData(_hInst, pRec, &value, &size));
    _rgPlateauPpiMapping[pRec->lSymbolVal - 5128] = value;
    return S_OK;
}

HRESULT CVSUnpack::_FlushDelayedRecords(IParserCallBack* pfnCB)
{
    int const plateauCount = 7;
    static_assert(sizeof(_rgImageDpiRec) == plateauCount * sizeof(_rgImageDpiRec[0]), "Size mismatch");
    static_assert(sizeof(_rgImageRec) == plateauCount * sizeof(_rgImageRec[0]), "Size mismatch");
    static_assert(sizeof(_rgComposedImageRec) == plateauCount * sizeof(_rgComposedImageRec[0]), "Size mismatch");

    int minDpis[7] = {-1, -1, -1, -1, -1, -1, -1};
    char plateauFlags[7] = {};

    HRESULT hr = S_OK;
    bool hasMinDpiRecord = false;

    for (int i = 0; i < plateauCount; ++i) {
        _VSRECORD* it = _rgImageDpiRec[i];
        if (!it) {
            plateauFlags[i] = 2;
            continue;
        }

        int minDpi = -1;
        int length = sizeof(minDpi);
        hr = LoadVSRecordData(_hInst, it, &minDpi, &length);
        if (hr < 0)
            break;

        hasMinDpiRecord = true;
        minDpis[i] = minDpi;
    }

    if (hasMinDpiRecord) {
        for (int idx = -1, v13 = -1; idx < plateauCount; ++idx) {
            int dpi;
            if (idx == v13) {
                dpi = GetScreenDpi();
                v13 = -1;
            } else if (_bittest((long*)&g_DpiInfo._nDpiPlateausCurrentlyPresent, idx)) {
                dpi = Map_Ordinal_To_DpiPlateau(idx);
            } else
                continue;

            int v18 = -1;
            int v20 = 0x7FFFFFFF;
            for (int i = 0; i < plateauCount; ++i) {
                if (minDpis[i] != -1 && dpi >= minDpis[i] && dpi - minDpis[i] < v20) {
                    v18 = i;
                    v20 = dpi - minDpis[i];
                    if (dpi == minDpis[i])
                        break;
                }
            }

            if (v18 != -1)
                plateauFlags[v18] = 1;
        }
    }

    {
        int idx = 0;
        char* v28 = plateauFlags;
        for (_VSRECORD* rec : _rgImageDpiRec) {
            if (*v28 == 1) {
                hr = _FixSymbolAndAddVSDataRecord(
                    pfnCB, rec, Map_Ordinal_To_MINDPI(idx));
                ++idx;
            }
            ++v28;
        }
    }

    {
        int idx = 0;
        char* v28 = plateauFlags;
        for (_VSRECORD* rec : _rgImageRec) {
            if (rec && *v28) {
                hr = _FixSymbolAndAddVSDataRecord(
                    pfnCB, rec, Map_Ordinal_To_IMAGEFILE(idx));
                ++idx;
            }
            ++v28;
        }
    }

    {
        int idx = 0;
        char* v35 = plateauFlags;
        for (_VSRECORD* rec : _rgComposedImageRec) {
            if (rec && *v35) {
                hr = _FixSymbolAndAddVSDataRecord(
                    pfnCB, rec, Map_Ordinal_To_COMPOSEDIMAGEFILE(idx));
                ++idx;
            }
            ++v35;
        }
    }

    fill_zero(_rgImageDpiRec);
    fill_zero(_rgImageRec);
    fill_zero(_rgComposedImageRec);
    return hr;
}

HRESULT CVSUnpack::_FlushDelayedPlateauRecords(IParserCallBack* pfnCB)
{
    int idx; // rsi@1
    signed int v6; // ebp@1
    int v8; // eax@1
    bool* v10; // r8@1
    int* v11; // r9@1

    HRESULT hr = S_OK;
    idx = -1;
    v6 = 0x7FFFFFFF;
    v8 = pfnCB->GetScreenPpi();
    v10 = _rgPlateauRec;
    v11 = _rgPlateauPpiMapping;
    for (int v9 = 0; v9 < 7; ++v9)
    {
        if (*v10 && v8 - *v11 >= 0 && v8 - *v11 < v6)
        {
            idx = v9;
            v6 = v8 - *v11;
        }
        ++v11;
        ++v10;
    }

    if (idx >= 0) {
        auto imageRec = _rgImagePRec[idx];
        if (imageRec)
            hr = _FixSymbolAndAddVSDataRecord(pfnCB, imageRec, TMT_IMAGEFILE);

        auto glyphImageRec = _rgGlyphImagePRec[idx];
        if (glyphImageRec && hr >= 0)
            hr = _FixSymbolAndAddVSDataRecord(pfnCB, glyphImageRec, TMT_GLYPHIMAGEFILE);

        auto contentMarginsRec = _rgContentMarginsPRec[idx];
        if (contentMarginsRec && hr >= 0)
            hr = _FixSymbolAndAddVSDataRecord(pfnCB, contentMarginsRec, TMT_CONTENTMARGINS);

        auto sizingMarginsRec = _rgSizingMarginsPRec[idx];
        if (sizingMarginsRec && hr >= 0)
            hr = _FixSymbolAndAddVSDataRecord(pfnCB, sizingMarginsRec, TMT_SIZINGMARGINS);
    }

    fill_zero(_rgPlateauRec);
    fill_zero(_rgImagePRec);
    fill_zero(_rgGlyphImagePRec);
    fill_zero(_rgContentMarginsPRec);
    fill_zero(_rgSizingMarginsPRec);
    return hr;
}

HRESULT CVSUnpack::_AddScaledBackgroundDataRecord(IParserCallBack* pfnCB)
{
    int index = pfnCB->AddToDIBDataArray(nullptr, 0, 0);
    if (index == -1)
        return E_FAIL;

    TMBITMAPHEADER hdr;
    hdr.dwSize = sizeof(hdr);
    hdr.iBitmapIndex = index;
    hdr.fPartiallyTransparent = 0;
    ENSURE_HR(pfnCB->AddData(TMT_SCALEDBACKGROUND, -44, &hdr, sizeof(hdr)));
    return S_OK;
}

HRESULT CVSUnpack::_SavePlateauRecord(_VSRECORD* pRec)
{
    int symbol = pRec->lSymbolVal;
    int idx;

    switch (symbol) {
    case TMT_5131:
    case TMT_5132:
    case TMT_5133:
        idx = symbol - TMT_5131;
        _rgImagePRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_5134:
    case TMT_5135:
    case TMT_5136:
        idx = symbol - TMT_5134;
        _rgGlyphImagePRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_5137:
    case TMT_5138:
    case TMT_5139:
        idx = symbol - TMT_5137;
        _rgContentMarginsPRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_5140:
    case TMT_5141:
    case TMT_5142:
        idx = symbol - TMT_5140;
        _rgSizingMarginsPRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;
    }

    return S_OK;
}

HRESULT CVSUnpack::_FixSymbolAndAddVSDataRecord(
    IParserCallBack* pfnCB, _VSRECORD* pRec, int lSymbolVal)
{
    char buffer[sizeof(_VSRECORD) + 260];

    if (pRec->lSymbolVal != lSymbolVal) {
        if (pRec->uResID) {
            memcpy(&buffer, pRec, sizeof(_VSRECORD));
        } else {
            size_t size = sizeof(_VSRECORD) + pRec->cbData;
            if (size > 292)
                return E_FAIL;
            memcpy(&buffer, pRec, size);
        }

        pRec = reinterpret_cast<_VSRECORD*>(&buffer);
        pRec->lSymbolVal = lSymbolVal;
    }

    return _AddVSDataRecord(pfnCB, _hInst, pRec);
}

HRESULT CVSUnpack::_SaveRecord(_VSRECORD* pRec)
{
    int symbol = pRec->lSymbolVal;

    switch (symbol) {
    case TMT_MINDPI1:
    case TMT_MINDPI2:
    case TMT_MINDPI3:
    case TMT_MINDPI4:
    case TMT_MINDPI5:
    case TMT_MINDPI6:
    case TMT_MINDPI7:
        _rgImageDpiRec[Map_MINDPI_To_Ordinal(symbol)] = pRec;
        break;
    case TMT_IMAGEFILE1:
    case TMT_IMAGEFILE2:
    case TMT_IMAGEFILE3:
    case TMT_IMAGEFILE4:
    case TMT_IMAGEFILE5:
    case TMT_IMAGEFILE6:
    case TMT_IMAGEFILE7:
        _rgImageRec[Map_IMAGEFILE_To_Ordinal(symbol)] = pRec;
        break;
    case TMT_COMPOSEDIMAGEFILE1:
    case TMT_COMPOSEDIMAGEFILE2:
    case TMT_COMPOSEDIMAGEFILE3:
    case TMT_COMPOSEDIMAGEFILE4:
    case TMT_COMPOSEDIMAGEFILE5:
    case TMT_COMPOSEDIMAGEFILE6:
    case TMT_COMPOSEDIMAGEFILE7:
        _rgComposedImageRec[Map_COMPOSEDIMAGEFILE_To_Ordinal(symbol)] = pRec;
        break;
    }

    return S_OK;
}

bool CVSUnpack::_DelayRecord(_VSRECORD* pRec)
{
    switch (pRec->lSymbolVal) {
    case TMT_MINDPI1:
    case TMT_MINDPI2:
    case TMT_MINDPI3:
    case TMT_MINDPI4:
    case TMT_MINDPI5:
    case TMT_MINDPI6:
    case TMT_MINDPI7:
    case TMT_IMAGEFILE1:
    case TMT_IMAGEFILE2:
    case TMT_IMAGEFILE3:
    case TMT_IMAGEFILE4:
    case TMT_IMAGEFILE5:
    case TMT_IMAGEFILE6:
    case TMT_IMAGEFILE7:
    case TMT_COMPOSEDIMAGEFILE1:
    case TMT_COMPOSEDIMAGEFILE2:
    case TMT_COMPOSEDIMAGEFILE3:
    case TMT_COMPOSEDIMAGEFILE4:
    case TMT_COMPOSEDIMAGEFILE5:
    case TMT_COMPOSEDIMAGEFILE6:
    case TMT_COMPOSEDIMAGEFILE7:
        return true;
    default:
        return false;
    }
}

} // namespace uxtheme
