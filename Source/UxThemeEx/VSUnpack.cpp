#include "VSUnpack.h"

#include "DpiInfo.h"
#include "ThemePNGDecoder.h"
#include "Utils.h"
#include "UxThemeHelpers.h"

#include <cassert>
#include <vssym32.h>
#include <strsafe.h>
#include <array>

using std::array;
using std::nothrow;

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

struct VSRESOURCE
{
    VSRESOURCETYPE type;
    unsigned uResID;
    std::wstring strValue;
    std::wstring strComment;
};

struct VSERRORCONTEXT
{
    _iobuf* _pLog;
    HINSTANCE__* _hInstErrorRes;
    wchar_t const* _pszSource;
    unsigned _nLineNumber;
    wchar_t const* _pszTool;
};

struct VSCOLORPROPENTRY
{
    int lHCSymbolVal;
    int lSymbolVal;
};

static VSCOLORPROPENTRY const vscolorprops[13] = {
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
    {TMT_HCGLYPHCOLOR, TMT_5106},
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
    {TPID_ENUM, 0, TMT_ENUM, sizeof(int)},
    {TPID_STRING, 0, TMT_STRING, -1},
    {TPID_INT, 0, TMT_INT, sizeof(int)},
    {TPID_BOOL, 0, TMT_BOOL, sizeof(BOOL)},
    {TPID_COLOR, 0, TMT_COLOR, sizeof(COLORREF)},
    {TPID_MARGINS, 0, TMT_MARGINS, sizeof(MARGINS)},
    {TPID_FILENAME, 0, TMT_FILENAME, -1},
    {TPID_SIZE, 0, TMT_SIZE, sizeof(int)},
    {TPID_POSITION, 0, TMT_POSITION, sizeof(POINT)},
    {TPID_RECT, 0, TMT_RECT, sizeof(RECT)},
    {TPID_FONT, 0, TMT_FONT, sizeof(LOGFONTW)},
    {TPID_INTLIST, 0, TMT_INTLIST, -1},
    {TPID_DISKSTREAM, 0, TMT_DISKSTREAM, -1},
    {TPID_STREAM, 0, TMT_STREAM, -1},
    {TPID_ANIMATION, TMT_ANIMATION, 241, -1},
    {TPID_TIMINGFUNCTION, TMT_TIMINGFUNCTION, 242, -1},
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

static wchar_t const g_pszAppName[] = L"";

static VSRECORD* GetNextVSRecord(VSRECORD* pRec, int cbBuf, int* pcbPos)
{
    int size = sizeof(VSRECORD);
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
    void const * pvData, unsigned cbData, VSRECORD** ppRecord, int* pcbRecord)
{
    *ppRecord = nullptr;
    *pcbRecord = 0;

    auto size = sizeof(VSRECORD) + cbData;
    auto record = new(cbData, std::nothrow) VSRECORD();
    if (!record)
        return E_OUTOFMEMORY;

    memset(record, 0, size);
    record->iClass = -1;
    record->iPart = 0;
    record->iState = 0;
    record->cbData = cbData;

    memcpy(&record[1], pvData, cbData);

    *ppRecord = record;
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

static int String2Number(wchar_t const* psz)
{
    int base = 10;
    int sign = 1;
    int value = 0;

    if (*psz == L'-') {
        sign = -1;
        ++psz;
    } else if (*psz == L'+') {
        ++psz;
    }

    if (*psz == L'0') {
        ++psz;
        if (*psz == L'X' || *psz == L'x') {
            base = 16;
            ++psz;
        }
    }

    for (; *psz; ++psz) {
        wchar_t const chr = *psz;

        int digit;
        if (chr >= L'0' && chr <= L'9')
            digit = chr - L'0';
        else if (chr >= L'A' && chr <= L'F' && base == 16)
            digit = 10 + (chr - L'A');
        else if (chr >= L'a' && chr <= L'f' && base == 16)
            digit = 10 + (chr - L'a');
        else
            break;

        value = (value * base) + digit;
    }

    return sign * value;
}

static bool IsSpace(wchar_t wch)
{
    WORD charType = 0;
    GetStringTypeW(CT_CTYPE1, &wch, 1, &charType);
    return (charType & C1_SPACE) != 0;
}

static bool IsDigit(wchar_t wch)
{
    WORD charType = 0;
    GetStringTypeW(CT_CTYPE1, &wch, 1, &charType);
    return (charType & C1_DIGIT) != 0;
}

static HRESULT ParseIntegerToken(wchar_t const** ppsz, wchar_t const* pszDelim, int* pnValue)
{
    wchar_t const* ptr = *ppsz;
    int value = 0;
    if (*ppsz) {
        while (ptr && *ptr != 0 && IsSpace(*ptr))
            ++ptr;

        if (ptr && *ptr) {
            if (!IsDigit(*ptr) && *ptr != L'+' && *ptr != L'-')
                return E_FAIL;
            if (*ptr)
                value = String2Number(ptr);
        }
    }

    if (ptr) {
        while (ptr && *ptr && !wcschr(pszDelim, *ptr))
            ++ptr;
        while (ptr && *ptr && wcschr(pszDelim, *ptr))
            ++ptr;
    }

    *ppsz = ptr;
    *pnValue = value;
    return S_OK;
}

static HRESULT ParseIntegerTokenList(wchar_t const** ppsz, int* prgVal, int* pcVal)
{
    if (*pcVal == 0 && prgVal)
        return E_INVALIDARG;

    HRESULT hr = S_OK;
    int const count = *pcVal;
    int idx;
    for (idx = 0; *ppsz && !**ppsz; ++idx) {
        if (idx >= count && prgVal)
            break;

        int value;
        hr = ParseIntegerToken(ppsz, L", ", &value);
        if (hr < 0)
            break;

        if (prgVal)
            prgVal[idx] = value;
    }

    *pcVal = idx;
    return hr;
}

static HRESULT ParseIntlist(wchar_t const* pszValue, int** pprgIntegers, int* pcIntegers)
{
    *pprgIntegers = nullptr;
    *pcIntegers = 0;

    int count = 0;
    ENSURE_HR(ParseIntegerTokenList(&pszValue, nullptr, &count));

    if (count == 0 || count > MAX_INTLIST_COUNT)
        return E_INVALIDARG;

    auto values = new(std::nothrow) int[count];
    if (!values)
        return E_OUTOFMEMORY;

    HRESULT hr = ParseIntegerTokenList(&pszValue, *pprgIntegers, pcIntegers);
    if (SUCCEEDED(hr)) {
        *pprgIntegers = values;
        *pcIntegers = count;
    } else {
        delete[](*pprgIntegers);
        *pprgIntegers = nullptr;
        *pcIntegers = 0;
    }

    return hr;
}

static HRESULT _ParseStringToken(
    wchar_t const** ppsz, wchar_t const* pszDelim, wchar_t** ppszString)
{
    HRESULT hr;

    wchar_t const* const delim = pszDelim ? pszDelim : L",:;";

    int length = 0;
    for (auto c = *ppsz; *c && !wcschr(delim, *c); ++c)
        ++length;

    auto token = new(std::nothrow) wchar_t[length + 1];
    if (token) {
        StringCchCopyNW(token, length + 1, *ppsz, length);
        *ppszString = token;
        hr = 0;
    } else {
        hr = E_OUTOFMEMORY;
    }

    wchar_t v15 = 0;
    int v16 = 0;
    wchar_t const* v17 = &(*ppsz)[length];

    for (auto c = v17; *c; ++c) {
        if (!wcschr(delim, *c))
            break;
        if (*c == v15)
            break;

        v15 = *c;
        ++v16;
    }

    *ppsz = &v17[v16];

    return hr;
}

static void AppendFontResComment(
    wchar_t** ppszCommentBuf, size_t* pcchComment, wchar_t const* pszStrToAppend)
{
    if (!*ppszCommentBuf || *pcchComment == 0)
        return;

    size_t appendLen = lstrlenW(pszStrToAppend);
    HRESULT hr = StringCchCopyNW(*ppszCommentBuf, *pcchComment, pszStrToAppend, appendLen);
    if (hr == STRSAFE_E_INSUFFICIENT_BUFFER) {
        *ppszCommentBuf = nullptr;
        *pcchComment = 0;
        return;
    }

    *ppszCommentBuf += appendLen;
    *pcchComment -= appendLen;
}

static HRESULT ParseFont(
    wchar_t const* pszValue, LOGFONTW* plf, size_t cchComment, wchar_t* pszComment)
{
    fill_zero(*plf);
    plf->lfWeight = 400;
    plf->lfCharSet = 1;

    wchar_t* faceName;
    ENSURE_HR(_ParseStringToken(&pszValue, nullptr, &faceName));

    StringCchCopyNW(plf->lfFaceName, 32, faceName, 32 - 1);
    AppendFontResComment(&pszComment, &cchComment, L"{Split=', '}{@FontFace@=s'0'}");
    delete[] faceName;

    int height;
    if (ParseIntegerToken(&pszValue, L",", &height) != S_OK)
        return S_OK;

    plf->lfHeight = -MulDiv(height, 96, 72);
    AppendFontResComment(&pszComment, &cchComment, L"{@fontsize_float@=s'1','FontSize'}");

    while (pszValue && *pszValue && IsSpace(*pszValue))
        ++pszValue;

    wchar_t buffer[200];
    HRESULT hr = S_OK;
    bool locked = false;
    wchar_t* token;

    for (int idx = 2; *pszValue && _ParseStringToken(&pszValue, L" ,", &token) == S_OK; ++idx) {
        wchar_t const* format = nullptr;
        if (!AsciiStrCmpI(token, L"bold")) {
            plf->lfWeight = 700;
            format = L"{ValidStrings=s'%d',i'Bold','bold',''}";
        } else if (!AsciiStrCmpI(token, L"italic")) {
            plf->lfItalic = 1;
            format = L"{ValidStrings=s'%d',i'Italic','italic',''}";
        } else if (!AsciiStrCmpI(token, L"underline")) {
            plf->lfUnderline = 1;
            format = L"{ValidStrings=s'%d',i'Underline','underline',''}";
        } else if (!AsciiStrCmpI(token, L"strikeout")) {
            plf->lfStrikeOut = 1;
            format = L"{ValidStrings=s'%d',i'Strikeout','strikeout',''}";
        } else if (!AsciiStrCmpI(token, L"quality:cleartype")) {
            plf->lfQuality = 5;
            locked = true;
        } else if (!AsciiStrCmpI(token, L"quality:cleartype-natural")) {
            plf->lfQuality = 6;
            locked = true;
        } else if (!AsciiStrCmpI(token, L"quality:antialiased")) {
            plf->lfQuality = 4;
            locked = true;
        } else if (!AsciiStrCmpI(token, L"quality:nonantialiased")) {
            plf->lfQuality = 3;
            locked = true;
        } else if (*token == 0) {
            hr = S_OK;
        } else {
            hr = 0x80070648;
        }

        if (format) {
            if (StringCchPrintfW(buffer, countof(buffer), format, idx) == S_OK)
                AppendFontResComment(&pszComment, &cchComment, buffer);
        }

        delete[] token;
        if (hr < 0)
            return hr;
    }

    if (locked)
        AppendFontResComment(&pszComment, &cchComment, L"{Locked=s'x'}");

    return hr;
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

    *pdwOffset = narrow_cast<unsigned>((uintptr_t)res - (uintptr_t)hInst);
    *pdwBytes = size;
    return S_OK;
}

static HRESULT AllocImageFileRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return E_ABORT;
}

static HRESULT AllocImageFileResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    auto record = new(std::nothrow) VSRECORD();
    if (!record)
        return E_OUTOFMEMORY;

    pvsr->uResID = uResID;
    pvsr->type = VSRT_BITMAP;
    pvsr->strValue = pszValue;

    record->uResID = uResID;
    record->cbData = cbType;

    *ppRecord = record;
    *pcbRecord = sizeof(VSRECORD);
    return S_OK;
}

static HRESULT LoadImageFileRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                                int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    void* data;
    unsigned size;
    HRESULT hr = GetPtrToResource(hInst, L"IMAGE",
                                  MAKEINTRESOURCEW(pRecord->uResID),
                                  &data, &size);

    if (hr >= 0) {
        if (*(WORD*)data != 0x4D42) {
            *(void**)pvData = data;
            *((DWORD*)pvData + 2) = size;
            *pcbData = 16;
            return hr;
        }
        if (!((*((WORD*)data + 14) - 24) & 0xFFF7)) {
            *(void**)pvData = (BYTE*)data + 14;
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
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int value = *(int*)pszValue;
    return _AllocateRecordPlusData(&value, sizeof(value), ppRecord, pcbRecord);
}

static HRESULT AllocStringRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int length = lstrlenW(pszValue);
    return _AllocateRecordPlusData(pszValue, 2 * (length + 1), ppRecord, pcbRecord);
}

static HRESULT AllocIntRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int value;
    ENSURE_HR(ParseIntegerToken(&pszValue, L", ", &value));
    return _AllocateRecordPlusData(&value, sizeof(value), ppRecord, pcbRecord);
}

static HRESULT AllocAtlasInputImageRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    char pvData[16];
    return _AllocateRecordPlusData(&pvData, 16, ppRecord, pcbRecord);
}

static HRESULT AllocAtlasImageResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    auto record = new(std::nothrow) VSRECORD();
    if (!record)
        return E_OUTOFMEMORY;

    pvsr->uResID = uResID;
    pvsr->type = VSRT_STREAM;
    pvsr->strValue = pszValue;

    record->uResID = uResID;
    record->cbData = 8;

    *ppRecord = record;
    *pcbRecord = sizeof(VSRECORD);

    return S_OK;
}

static HRESULT LoadDiskStreamRes(HMODULE hInst, VSRECORD* pRecord,
                                 void* pvData, int* pcbData)
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
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    BOOL value;
    ENSURE_HR(_ParseBool(pszValue, &value));
    ENSURE_HR(_AllocateRecordPlusData(&value, sizeof(value), ppRecord, pcbRecord));
    return S_OK;
}

static HRESULT AllocRGBRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int values[3];
    int count = (int)countof(values);
    ENSURE_HR(ParseIntegerTokenList(&pszValue, values, &count));

    int rgb = (values[0] & 0xFF) | ((values[1] & 0xFF) << 8) | ((values[2] & 0xFF) << 16);
    ENSURE_HR(_AllocateRecordPlusData(&rgb, sizeof(rgb), ppRecord, pcbRecord));
    return S_OK;
}

static HRESULT AllocRectRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    RECT rect;
    int count = 4;
    ENSURE_HR(ParseIntegerTokenList(&pszValue, (int*)&rect, &count));
    ENSURE_HR(_AllocateRecordPlusData(&rect, sizeof(rect), ppRecord, pcbRecord));
    return S_OK;
}

static HRESULT AllocPositionRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    POINT pt;
    int count = 2;
    ENSURE_HR(ParseIntegerTokenList(&pszValue, (int*)&pt, &count));
    ENSURE_HR(_AllocateRecordPlusData(&pt, sizeof(pt), ppRecord, pcbRecord));
    return S_OK;
}

static HRESULT AllocFontRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    LOGFONTW lf;
    if (cbType < sizeof(lf))
        return E_INVALIDARG;

    ENSURE_HR(ParseFont(pszValue, &lf, 0, nullptr));
    ENSURE_HR(_AllocateRecordPlusData(&lf, sizeof(lf), ppRecord, pcbRecord));
    return S_OK;
}

static HRESULT AllocIntlistRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    int* values;
    int count;
    ENSURE_HR(ParseIntlist(pszValue, &values, &count));

    HRESULT hr = _AllocateRecordPlusData(values, sizeof(int) * count, ppRecord, pcbRecord);
    delete[] values;
    return hr;
}

static HRESULT AllocAnimationRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocFloatRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocFloatListRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocStringResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    auto record = new(std::nothrow) VSRECORD();
    if (!record)
        return E_OUTOFMEMORY;

    pvsr->uResID = uResID;
    pvsr->type = VSRT_STRING;
    pvsr->strValue = pszValue;

    if (cbType == -1)
        cbType = 2 * (lstrlenW(pvsr->strValue.c_str()) + 1);
    record->uResID = uResID;
    record->cbData = cbType;

    *ppRecord = record;
    *pcbRecord = sizeof(VSRECORD);
    return S_OK;
}

static HRESULT AllocFontResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    if (cbType < sizeof(LOGFONTW))
        return E_INVALIDARG;

    wchar_t comment[1000] = {};

    LOGFONTW lf;
    ENSURE_HR(ParseFont(pszValue, &lf, 1000, comment));
    ENSURE_HR(AllocStringResRecord(pszValue, ppRecord, cbType, pcbRecord,
                                   uResID, pvsr, pEcx));

    pvsr->strComment = comment;
    return S_OK;
}

static HRESULT AllocIntlistResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    int* values;
    int count;

    if (ParseIntlist(pszValue, &values, &count) >= 0) {
        cbType = sizeof(int) * count;
        delete[] values;
    }

    return AllocStringResRecord(pszValue, ppRecord, cbType, pcbRecord, uResID,
                                pvsr, pEcx);
}

static HRESULT LoadStringRes(HMODULE hInst, VSRECORD* pRecord,
                             wchar_t* pszData, int cchData)
{
    *pszData = 0;
    return LoadStringW(hInst, pRecord->uResID, pszData, cchData) == 0 ? HRESULT_FROM_WIN32(ERROR_NOT_FOUND) : 0;
}

static HRESULT LoadStringRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                             int* pcbData)
{
    return LoadStringRes(hInst, pRecord, (wchar_t*)pvData, *pcbData / sizeof(wchar_t));
}

static HRESULT LoadIntRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                          int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    wchar_t const* cbuffer = buffer;
    return ParseIntegerToken(&cbuffer, L", ", (int*)pvData);
}

static HRESULT LoadBoolRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                           int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));
    return _ParseBool(buffer, (BOOL*)pvData);
}

static HRESULT LoadRectRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                           int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    int values[4];
    int count = static_cast<int>(countof(values));
    wchar_t const* cbuffer = buffer;
    ENSURE_HR(ParseIntegerTokenList(&cbuffer, values, &count));

    auto rect = static_cast<RECT*>(pvData);
    rect->left = values[0];
    rect->top = values[1];
    rect->right = values[2];
    rect->bottom = values[3];
    return S_OK;
}

static HRESULT LoadRGBRes(HMODULE hInst, VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    int values[3];
    int count = (int)countof(values);
    wchar_t const* cbuffer = buffer;
    ENSURE_HR(ParseIntegerTokenList(&cbuffer, values, &count));

    *(unsigned*)pvData = (values[0] & 0xFF) | ((values[1] & 0xFF) << 8) | ((values[2] & 0xFF) << 16);
    return S_OK;
}

static HRESULT AllocStreamResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT AllocDiskStreamResRecord(
    wchar_t const* pszValue, VSRECORD** ppRecord, int cbType, int* pcbRecord,
    unsigned uResID, VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx)
{
    return TRACE_HR(E_NOTIMPL);
}

static HRESULT LoadStreamRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                             int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    void* ptr = nullptr;
    int size;
    ENSURE_HR(GetPtrToResource(hInst, L"STREAM", MAKEINTRESOURCEW(pRecord->uResID),
                               &ptr, (unsigned*)&size));

    if (size > *pcbData)
        size = *pcbData;

    memcpy(pvData, ptr, size);
    *pcbData = size;

    return S_OK;
}

static HRESULT LoadPositionRes(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                               int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[128];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 128));

    int values[2];
    int count = (int)countof(values);
    wchar_t const* cbuffer = buffer;
    ENSURE_HR(ParseIntegerTokenList(&cbuffer, values, &count));

    auto pt = (POINT*)pvData;
    pt->x = values[0];
    pt->y = values[1];
    return S_OK;
}

static HRESULT LoadIntlistRes(HMODULE hInst, VSRECORD* pRecord, void* pvData, int* pcbData)
{
    if (!pcbData || !pRecord || *pcbData < pRecord->cbData)
        return E_INVALIDARG;

    wchar_t buffer[256];
    ENSURE_HR(LoadStringRes(hInst, pRecord, buffer, 256));

    int* integers;
    int pcIntegers;
    ENSURE_HR(ParseIntlist(buffer, &integers, &pcIntegers));

    size_t cBytes = sizeof(int) * pcIntegers;
    if (cBytes > *pcbData)
        cBytes = *pcbData;

    memcpy(pvData, integers, cBytes);
    *pcbData = narrow_cast<int>(cBytes);
    delete[] integers;

    return S_OK;
}

static HRESULT LoadFontRes(HMODULE hInst, VSRECORD* pRecord, void* pvData, int* pcbData)
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
    HRESULT(*pfnAlloc)(wchar_t const* pszValue, VSRECORD** ppRecord, int cbType,
                       int* pcbRecord, VSERRORCONTEXT* pEcx);
    HRESULT(*pfnAllocRes)(wchar_t const* pszValue, VSRECORD** ppRecord,
                          int cbType, int* pcbRecord, unsigned uResID,
                          VSRESOURCE* pvsr, VSERRORCONTEXT* pEcx);
    HRESULT(*pfnLoad)(HMODULE hInst, VSRECORD* pRecord, void* pvData,
                      int* pcbData);
    void(*pfnUnload)(void* pvData);
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
    if (FAILED(pfnCB->AddData(TMT_JUMPTOPARENT, TMT_JUMPTOPARENT, &value, sizeof(value))))
        return E_FAIL;

    int length = pfnCB->GetNextDataIndex() - begin;
    return pfnCB->AddIndex(pszAppName, pszClassName, iPartId, iStateId, begin, length);
}

static HRESULT _TerminateSection(
    IParserCallBack* pfnCB, wchar_t const* pszApp, wchar_t const* pszClass,
    int iPart, int iState, int iStartOfSection)
{
    int value = 0;
    if (FAILED(pfnCB->AddData(TMT_JUMPTOPARENT, TMT_JUMPTOPARENT, &value, sizeof(value))))
        return E_FAIL;

    int length = pfnCB->GetNextDataIndex() - iStartOfSection;
    return pfnCB->AddIndex(pszApp, pszClass, iPart, iState, iStartOfSection, length);
}

static void _ParseClassName(
    wchar_t const* pszClassSpec, wchar_t* pszAppNameBuf, unsigned cchAppNameBuf,
    wchar_t* pszClassNameBuf, unsigned cchClassNameBuf)
{
    *pszAppNameBuf = 0;
    wchar_t* dst = pszClassNameBuf;
    unsigned v10 = 0;
    size_t count = 0;

    wchar_t const* src = pszClassSpec;
    while (*src && v10 < 259) {
        if (src[0] == L':' && src[1] == L':') {
            StringCchCopyNW(pszAppNameBuf, cchAppNameBuf, pszClassSpec, count);
            src += 2;
            count += 2;
            dst = pszClassNameBuf;
            v10 = 0;
            continue;
        }

        ++count;
        *dst++ = *src++;
        ++v10;
    }

    *dst = 0;
}

static HRESULT LoadVSRecordData(
    HMODULE hInstVS, VSRECORD* pRecord, void* pvBuf, int* pcbBuf)
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

static void UnloadVSRecordData(VSRECORD* pRecord, void* pvBuf)
{
    if (pRecord && pvBuf) {
        int id = GetThemePrimitiveID(pRecord->lSymbolVal, pRecord->lType);
        if (id >= 0) {
            if (auto unload = parse_table[id].pfnUnload)
                unload(pvBuf);
        }
    }
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

static uint8_t PremultiplyChannel(uint8_t channel, uint8_t alpha)
{
    // premult = alpha/255 * color/255 * 255
    //         = (alpha*color)/255
    //
    // (z/255) = z/256 * 256/255
    //         = z/256 * (1 + 1/255)
    //         = z/256 + (z/256)/255
    //         = (z + z/255)/256
    //         # Recursing once:
    //         = (z + (z + z/255)/256)/256
    //         = (z + z/256 + z/256/255) / 256
    //         # Using only 16-bit operations, loosing some precision, and
    //         # flooring the result gives:
    //         => (z + z>>8)>>8
    //         # Add 0x80/255 (= 0.5) to convert floor to round
    //         => (z+0x80 + (z+0x80)>>8 ) >> 8
    //         = lround(z/255.0) for z in [0..0xfe02]
    unsigned pre = channel * alpha + 0x80;
    return static_cast<uint8_t>((pre + (pre >> 8)) >> 8);
}

static unsigned PremultiplyColor(unsigned rgba)
{
    uint8_t r = static_cast<uint8_t>(rgba >> 0);
    uint8_t g = static_cast<uint8_t>(rgba >> 8);
    uint8_t b = static_cast<uint8_t>(rgba >> 16);
    uint8_t a = static_cast<uint8_t>(rgba >> 24);

    r = PremultiplyChannel(r, a);
    g = PremultiplyChannel(g, a);
    b = PremultiplyChannel(b, a);

    return (a << 24) | (b << 16) | (g << 8) | r;
}

static unsigned PremultiplyColor(unsigned rgba, uint8_t alpha)
{
    uint8_t r = static_cast<uint8_t>(rgba >> 0);
    uint8_t g = static_cast<uint8_t>(rgba >> 8);
    uint8_t b = static_cast<uint8_t>(rgba >> 16);
    uint8_t a = static_cast<uint8_t>(rgba >> 24);

    r = PremultiplyChannel(r, alpha);
    g = PremultiplyChannel(g, alpha);
    b = PremultiplyChannel(b, alpha);
    a = PremultiplyChannel(a, alpha);

    return (a << 24) | (b << 16) | (g << 8) | r;
}

static unsigned AlphaBlend(unsigned rgbDst, unsigned rgbaSrc)
{
    uint8_t invSrcAlpha = ~(rgbaSrc >> 24) & 0xFF;
    if (invSrcAlpha == 0)
        return rgbaSrc; // Source is opaque.

    uint8_t r = static_cast<uint8_t>(rgbDst >> 0);
    uint8_t g = static_cast<uint8_t>(rgbDst >> 8);
    uint8_t b = static_cast<uint8_t>(rgbDst >> 16);
    uint8_t a = static_cast<uint8_t>(rgbDst >> 24);

    r = PremultiplyChannel(r, invSrcAlpha);
    g = PremultiplyChannel(g, invSrcAlpha);
    b = PremultiplyChannel(b, invSrcAlpha);
    a = PremultiplyChannel(a, invSrcAlpha);

    return rgbaSrc + ((a << 24) | (b << 16) | (g << 8) | r);
}

static unsigned PixelPremultiplyAlpha(unsigned rgbaSrc, unsigned rgbTint)
{
    unsigned alpha = rgbaSrc >> 24;
    unsigned color = rgbTint | (alpha << 24);

    if (alpha == 0x00)
        return 0;
    if (alpha == 0xFF)
        return color;

    return PremultiplyColor(color);
}

void ColorizeGlyphByAlpha(BYTE* pBytes, BITMAPINFOHEADER* pBitmapHdr, unsigned rgbColor)
{
    unsigned const tint = SwapRGB(rgbColor);
    unsigned const* src = reinterpret_cast<unsigned*>(GetBitmapBits(pBitmapHdr));
    unsigned* dst = reinterpret_cast<unsigned*>(pBytes);

    for (int i = 0; i < pBitmapHdr->biWidth * pBitmapHdr->biHeight; ++i)
        *dst++ = PixelPremultiplyAlpha(*src++, tint);
}

static void ColorizeAndComposeImages(
    BYTE* pDstImageBytes, BYTE* pSrcBGImageBytes, BYTE* pSrcFGImageBytes,
    SIZE imageSize, unsigned* pBGColor, unsigned* pFGColor)
{
    auto srcFg = reinterpret_cast<unsigned*>(pSrcFGImageBytes);
    auto srcBg = reinterpret_cast<unsigned*>(pSrcBGImageBytes);
    auto dst = reinterpret_cast<unsigned*>(pDstImageBytes);

    for (long i = imageSize.cx * imageSize.cy; i; --i) {
        unsigned fgClr;
        if (pFGColor)
            fgClr = PixelPremultiplyAlpha(*srcFg, SwapRGB(*pFGColor));
        else
            fgClr = *srcFg;
        ++srcFg;

        unsigned bgClr;
        if (pBGColor)
            bgClr = PixelPremultiplyAlpha(*srcBg, SwapRGB(*pBGColor));
        else
            bgClr = *srcBg;
        ++srcBg;

        *dst = bgClr;
        if (fgClr & 0xFF000000)
            *dst = AlphaBlend(bgClr, fgClr);
        ++dst;
    }
}

void ColorizeGlyphByAlphaComposition(
    BYTE* pBytes, BITMAPINFOHEADER* pBitmapHdr, int iImageCount,
    unsigned* pGlyphBGColor, unsigned* pGlyphColor)
{
    SIZE size;
    size.cx = pBitmapHdr->biWidth;
    size.cy = pBitmapHdr->biHeight / (2 * iImageCount);

    int const byteCount = 4 * size.cx * size.cy;
    BYTE* srcFg = GetBitmapBits(pBitmapHdr);
    BYTE* srcBg = srcFg + byteCount;

    for (int i = 0; i < iImageCount; ++i) {
        ColorizeAndComposeImages(pBytes, srcBg, srcFg, size, pGlyphBGColor, pGlyphColor);
        pBytes += byteCount;
        srcFg += 2 * byteCount;
        srcBg += 2 * byteCount;
    }
}

void Convert24to32BPP(BYTE* pBytes, BITMAPINFOHEADER* pBitmapHdr)
{
    long const stride = pBitmapHdr->biWidth;
    long const v5 = 3 * (stride + 1) & 0xFFFFFFFC;

    auto const* src = (BYTE*)GetBitmapBits(pBitmapHdr);
    for (long row = 0; row < pBitmapHdr->biHeight; ++row) {
        unsigned* pRow = (unsigned*)&pBytes[4 * row * stride];

        for (long col = 0; col < stride; ++col) {
            unsigned char r = src[3 * col];
            unsigned char g = src[3 * col + 1];
            unsigned char b = src[3 * col + 2];
            pRow[col] = 0xFF000000u | (b << 16) | (g << 8) | r;
        }

        src += v5;
    }
}

static HRESULT _ReadVSVariant(BYTE* pbVariantList, int cbVariantList, int* pcbPos,
                              wchar_t** ppszName, wchar_t** ppszSize,
                              wchar_t** ppszColor)
{
    if (cbVariantList < 4 || *pcbPos >= cbVariantList)
        return STRSAFE_E_END_OF_FILE;

    for (wchar_t** ppsz : {ppszName, ppszSize, ppszColor}) {
        unsigned length;
        std::memcpy(&length, &pbVariantList[*pcbPos], sizeof(length));
        if (length == 0)
            return E_INVALIDARG;

        *pcbPos += sizeof(length);

        if (ppsz) {
            auto str = new(std::nothrow) wchar_t[length];
            *ppsz = str;
            if (!str)
                return E_OUTOFMEMORY;
            StringCchCopyNW(str, length, (wchar_t const*)&pbVariantList[*pcbPos], length - 1);
        }

        *pcbPos += Align8(sizeof(wchar_t) * length);
        if (*pcbPos >= cbVariantList)
            break;
    }

    return S_OK;
}

template<typename T>
static T* AllocateZero(size_t n)
{
    return (T*)calloc(n, sizeof(BYTE));
}

HRESULT CVSUnpack::Initialize(HMODULE hInstSrc, int nVersion, bool fGlobal,
                              bool fIsLiteVisualStyle, bool fHighContrast)
{
    _hInst = hInstSrc;
    _nVersion = nVersion;
    _fGlobal = fGlobal;
    _fIsLiteVisualStyle = fIsLiteVisualStyle;
    _fIsHighContrast = fHighContrast;

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
    _rgBitmapIndices = make_unique_malloc<int[]>(_cBitmaps);
    if (!_rgBitmapIndices)
        return E_OUTOFMEMORY;

    for (unsigned i = 0; i < _cBitmaps; ++i)
        _rgBitmapIndices[i] = -1;

    _rgfPartiallyTransparent = make_unique_calloc<BYTE[]>((_cBitmaps / CHAR_BIT) + 1);
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
    for (auto it = static_cast<VSRECORD*>(buf); it; it = GetNextVSRecord(it, cbBuf, &pcbPos)) {
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

HRESULT CVSUnpack::GetClassData(wchar_t const* pszColorVariant,
                                wchar_t const* pszSizeVariant,
                                void** pvMap, int* pcbMap)
{
    if (!pszColorVariant || !*pszColorVariant)
        return E_INVALIDARG;
    if (!pszSizeVariant || !*pszSizeVariant)
        return E_INVALIDARG;

    void* vmap;
    int cbVMap;

    HRESULT hr = GetVariantMap(&vmap, &cbVMap);
    if (hr < 0)
        return hr;

    bool variantFound = false;
    int pcbPos = 0;
    do {
        if (hr < 0)
            break;
        wchar_t* name = nullptr;
        wchar_t* size = nullptr;
        wchar_t* color = nullptr;
        hr = _ReadVSVariant((BYTE*)vmap, cbVMap, &pcbPos, &name, &size, &color);
        if (hr < 0)
            continue;

        if (!AsciiStrCmpI(size, pszSizeVariant) && !AsciiStrCmpI(color, pszColorVariant)) {
            variantFound = true;
            hr = GetPtrToResource(_hInst, L"VARIANT", name, pvMap, (unsigned*)pcbMap);
        }

        delete[] name;
        delete[] color;
        delete[] size;
    } while (!variantFound);

    if (!variantFound)
        return E_INVALIDARG;

    return hr;
}

HRESULT CVSUnpack::LoadClassDataMap(
    wchar_t const* pszColor, wchar_t const* pszSize, IParserCallBack* pfnCB)
{
    void* pvMap;
    int cbBuf;
    ENSURE_HR(GetClassData(pszColor, pszSize, &pvMap, &cbBuf));

    _pbClassData = pvMap;
    _cbClassData = cbBuf;

    int pcbPos = 0;
    int globalsElementId = _FindClass(c_szGlobalsElement);
    int sysmetsElementId = _FindClass(c_szSysmetsElement);
    int startOfSection = -1;

    bool hasDelayedRecords = true;
    bool hasPlateauRecords = false;
    bool hasGlobalsSection = false;
    bool hasSysmetsSection = false;

    int currClassId = -1;
    int currPartId = 0;
    int currStateId = 0;

    wchar_t className[230];
    wchar_t appName[260];
    className[0] = 0;
    appName[0] = 0;

    HRESULT hr = S_OK;

    if (auto pRec = static_cast<VSRECORD*>(pvMap)) {
        while (hr >= 0) {
            bool isNewClass = currClassId != pRec->iClass;
            bool isNewPart = currPartId != pRec->iPart;
            bool isNewState = currStateId != pRec->iState;

            if (isNewClass || isNewPart || isNewState) {
                if (hasDelayedRecords)
                    ENSURE_HR(_FlushDelayedRecords(pfnCB));

                ENSURE_HR(_AddScaledBackgroundDataRecord(pfnCB));
                if (hasPlateauRecords)
                    ENSURE_HR(_FlushDelayedPlateauRecords(pfnCB));

                if (startOfSection >= 0)
                    ENSURE_HR(_TerminateSection(pfnCB, appName, className,
                                                currPartId, currStateId, startOfSection));

                appName[0] = 0;
                className[0] = 0;
                if (pRec->iClass <= -1 || pRec->iClass >= _rgClassNames.size())
                    return E_ABORT;

                _ParseClassName(_rgClassNames[pRec->iClass].c_str(), appName, 260, className, 230);

                hasDelayedRecords = false;
                hasPlateauRecords = false;
                if (pRec->iClass == globalsElementId) {
                    hasGlobalsSection = true;
                } else if (pRec->iClass == sysmetsElementId) {
                    hasSysmetsSection = true;
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

                    bool v17;
                    if (isNewClass && (pRec->iPart != 0 || pRec->iState != 0)) {
                        ENSURE_HR(_GenerateEmptySection(pfnCB, appName, className, 0, 0));
                        v17 = true;
                    } else {
                        v17 = currStateId == 0;
                    }

                    if (isNewPart && pRec->iState != 0 && v17)
                        ENSURE_HR(_GenerateEmptySection(pfnCB, appName, className, pRec->iPart, 0));
                }

                currClassId = pRec->iClass;
                currPartId = pRec->iPart;
                currStateId = pRec->iState;
                startOfSection = pfnCB->GetNextDataIndex();
            }

            if (_DelayRecord(pRec)) {
                hr = _SaveRecord(pRec);
                hasDelayedRecords = true;
            } else if (pRec->lSymbolVal >= TMT_FIRSTPLATEAURECORD &&
                       pRec->lSymbolVal <= TMT_LASTPLATEAURECORD) {
                hr = _SavePlateauRecord(pRec);
                hasPlateauRecords = true;
            } else if (pRec->iClass == globalsElementId &&
                       pRec->lSymbolVal >= TMT_FIRSTPPIPLATEAU &&
                       pRec->lSymbolVal <= TMT_LASTPPIPLATEAU)
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

    if (startOfSection >= 0)
        ENSURE_HR(_TerminateSection(pfnCB, appName, className, currPartId, currStateId, startOfSection));

    return hr;
}

HRESULT CVSUnpack::LoadBaseClassDataMap(IParserCallBack* pfnCB)
{
    int* ptr;
    unsigned size;

    ENSURE_HR(GetPtrToResource(_hInst, c_szBCMAP, c_szBCMAP, (void**)&ptr, &size));

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

    return -1;
}

HRESULT CVSUnpack::LoadAnimationDataMap(IParserCallBack* pfnCB)
{
    void* pbBuf = nullptr;
    unsigned cbBuf = 0;
    HRESULT hr = GetPtrToResource(_hInst, c_szAMAP, c_szAMAP, &pbBuf, &cbBuf);
    if (hr == 0x80070715 || !pbBuf || !cbBuf)
        return S_OK;
    ENSURE_HR(hr);

    int const timingClass = _FindClass(c_szTimingFunctionElement_0);

    int currClass = 0;
    int currPart = 0;

    int pos = 0;
    for (auto pRec = static_cast<VSRECORD*>(pbBuf); pRec != nullptr;
         pRec = GetNextVSRecord(pRec, cbBuf, &pos)) {
        if (hr < 0)
            break;

        if (currClass != pRec->iClass) {
            currClass = pRec->iClass;
            hr = _GenerateEmptySection(pfnCB, g_pszAppName,
                                       _rgClassNames[pRec->iClass].c_str(), 0, 0);
        }

        if (currClass != timingClass && currPart != pRec->iPart) {
            currPart = pRec->iPart;
            hr = _GenerateEmptySection(
                pfnCB, g_pszAppName, _rgClassNames[pRec->iClass].c_str(),
                pRec->iPart, 0);
        }

        if (hr >= 0) {
            int index = pfnCB->GetNextDataIndex();
            hr = _AddVSDataRecord(pfnCB, _hInst, pRec);
            if (hr >= 0)
                hr = _TerminateSection(
                    pfnCB,
                    g_pszAppName,
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
    int lSymbolVal, VSRECORD** ppRec)
{
    int pcbPos = 0;

    auto pRec = (VSRECORD*)pvRecBuf;
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
    return HRESULT_FROM_WIN32(ERROR_NO_MATCH);
}

HRESULT CVSUnpack::_GetPropertyValue(
    void* pvBits, int cbBits, int iClass, int iPart, int iState, int lSymbolVal,
    void* pvValue, int* pcbValue)
{
    VSRECORD* pRecord;
    HRESULT hr = _FindVSRecord(pvBits, cbBits, iClass, iPart, iState, lSymbolVal, &pRecord);
    if (FAILED(hr))
        return hr;
    return LoadVSRecordData(_hInst, pRecord, pvValue, pcbValue);
}

HRESULT CVSUnpack::_GetImagePropertiesForHC(
    IMAGEPROPERTIES** ppImageProperties, HCIMAGEPROPERTIES* pHCImageProperties, int iImageCount)
{
    auto props = make_unique_nothrow<IMAGEPROPERTIES[]>(iImageCount);
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

    *ppImageProperties = props.release();

    return S_OK;
}

HRESULT CVSUnpack::_CreateImageFromProperties(
    IMAGEPROPERTIES const* pImageProperties, int iImageCount,
    MARGINS const* pSizingMargins, MARGINS const* pTransparentMargins,
    BYTE** ppbNewBitmap, int* pcbNewBitmap)
{
    MARGINS const transparentMargin =
        pTransparentMargins ? *pTransparentMargins : MARGINS();

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
    size_t const cBytes = sizeof(BITMAPHEADER) + imageSize;

    auto newBitmap = make_unique_malloc<BYTE[]>(cBytes);
    if (!newBitmap)
        return E_OUTOFMEMORY;

    auto header = reinterpret_cast<BITMAPHEADER*>(newBitmap.get());
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

    *ppbNewBitmap = newBitmap.release();
    *pcbNewBitmap = static_cast<int>(cBytes);
    return S_OK;
}

HRESULT CVSUnpack::_EnsureBufferSize(unsigned cbBytes)
{
    if (cbBytes > _cbBuffer) {
        auto newBuffer = make_unique_malloc<BYTE[]>(cbBytes);
        if (!newBuffer)
            return E_OUTOFMEMORY;

        _pBuffer = std::move(newBuffer);
        _cbBuffer = cbBytes;
    }

    return S_OK;
}

bool CVSUnpack::_IsTrueSizeImage(VSRECORD* pRec)
{
    SIZINGTYPE type = ST_STRETCH;
    int size = sizeof(type);
    if (_GetPropertyValue(_pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                          pRec->iState, TMT_SIZINGTYPE, &type, &size) < 0)
        return false;

    return (type & ~ST_TILE) == 0;
}

HRESULT CVSUnpack::_ExpandVSRecordForColor(IParserCallBack* pfnCB,
                                           VSRECORD* pRec, BYTE* pbData,
                                           int cbData, bool* pfIsColor)
{
    *pfIsColor = false;

    for (auto const& entry : vscolorprops) {
        if (pRec->lSymbolVal == entry.lHCSymbolVal) {
            *pfIsColor = true;
            if (!IsHighContrastMode())
                return S_OK;

            DWORD value = MapEnumToSysColor(*reinterpret_cast<HIGHCONTRASTCOLOR*>(pbData));
            return pfnCB->AddData(entry.lSymbolVal, TMT_COLOR, &value, sizeof(value));
        }

        if (pRec->lSymbolVal == entry.lSymbolVal) {
            *pfIsColor = true;
            if (!IsHighContrastMode())
                return S_FALSE;

            VSRECORD* r;
            HRESULT hr = _FindVSRecord(_pbClassData, _cbClassData, pRec->iClass,
                                       pRec->iPart, pRec->iState,
                                       entry.lHCSymbolVal, &r);
            return FAILED(hr) ? S_FALSE : S_OK;
        }
    }

    return S_OK;
}

HRESULT CVSUnpack::_ExpandVSRecordForMargins(IParserCallBack* pfnCB,
                                             VSRECORD* pRec, BYTE* pbData,
                                             int cbData, bool* pfIsMargins)
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
            return hr == HRESULT_FROM_WIN32(ERROR_NO_MATCH) ? S_FALSE : hr;

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

static bool TestBit(BYTE const* array, int n)
{
    return (array[n / CHAR_BIT] & (1 << (n % CHAR_BIT))) != 0;
}

static void SetBit(BYTE* array, int n)
{
    array[n / CHAR_BIT] |= 1 << (n % CHAR_BIT);
}

HRESULT CVSUnpack::_ExpandVSRecordData(IParserCallBack* pfnCB, VSRECORD* pRec,
                                       BYTE* pbData, int cbData)
{
    HRESULT hr;
    if (_fIsLiteVisualStyle) {
        bool fIsColor;
        hr = _ExpandVSRecordForColor(pfnCB, pRec, pbData, cbData, &fIsColor);
        if (fIsColor)
            return hr;
        bool fIsMargins;
        hr = _ExpandVSRecordForMargins(pfnCB, pRec, pbData, cbData, &fIsMargins);
        if (fIsMargins)
            return hr;
    }

    std::unique_ptr<IMAGEPROPERTIES[]> allocatedImageProps;
    IMAGEPROPERTIES* pImageProperties = nullptr;
    bool doGlyphColorization = false;
    int iImageCount = 0;
    BYTE* v97 = 0;

    short lDibSymbolVal;
    bool fSimplifiedImage = false;
    bool fComposedImage = false;

    switch (pRec->lSymbolVal) {
    case TMT_IMAGEFILE:
        lDibSymbolVal = TMT_DIBDATA;
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    case TMT_IMAGEFILE1:
    case TMT_IMAGEFILE2:
    case TMT_IMAGEFILE3:
    case TMT_IMAGEFILE4:
    case TMT_IMAGEFILE5:
    case TMT_IMAGEFILE6:
    case TMT_IMAGEFILE7:
        lDibSymbolVal = Map_IMAGEFILE_To_DIBDATA(pRec->lSymbolVal);
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    case TMT_GLYPHIMAGEFILE:
        lDibSymbolVal = TMT_GLYPHDIBDATA;
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    case TMT_SIMPLIFIEDIMAGE:
    {
        if (IsHighContrastMode())
            return S_OK;

        lDibSymbolVal = TMT_DIBDATA;
        fSimplifiedImage = true;
        pImageProperties = reinterpret_cast<IMAGEPROPERTIES*>(pbData);
        iImageCount = cbData / sizeof(IMAGEPROPERTIES);
        break;
    }

    case TMT_HCSIMPLIFIEDIMAGE:
    {
        if (!IsHighContrastMode())
            return S_OK;

        lDibSymbolVal = TMT_DIBDATA;
        fSimplifiedImage = true;
        iImageCount = cbData / sizeof(HCIMAGEPROPERTIES);
        v97 = pbData;
        ENSURE_HR(_GetImagePropertiesForHC(
            &pImageProperties, reinterpret_cast<HCIMAGEPROPERTIES*>(pbData),
            iImageCount));
        allocatedImageProps.reset(pImageProperties);
        break;
    }

    case TMT_HCGLYPHBGCOLOR:
        return IsHighContrastMode() != 0;

    case TMT_COMPOSEDIMAGEFILE:
        lDibSymbolVal = TMT_DIBDATA;
        fComposedImage = true;
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    case TMT_COMPOSEDGLYPHIMAGEFILE:
        lDibSymbolVal = TMT_GLYPHDIBDATA;
        fComposedImage = true;
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    case TMT_COMPOSEDIMAGEFILE1:
    case TMT_COMPOSEDIMAGEFILE2:
    case TMT_COMPOSEDIMAGEFILE3:
    case TMT_COMPOSEDIMAGEFILE4:
    case TMT_COMPOSEDIMAGEFILE5:
    case TMT_COMPOSEDIMAGEFILE6:
    case TMT_COMPOSEDIMAGEFILE7:
        lDibSymbolVal = Map_COMPOSEDIMAGEFILE_To_DIBDATA(pRec->lSymbolVal);
        fComposedImage = true;
        if (cbData < 16)
            return E_INVALIDARG;
        break;

    default:
        return S_FALSE;
    }

    if (!_rgBitmapIndices)
        return E_FAIL;

    unsigned char ePrimVal;
    BYTE* v44;

    int pvBuf;
    malloc_ptr<BYTE> allocatedImage;
    VSRECORD* pRecord;
    HDC hDC;

    hr = 0;
    int iRes = 0;
    int bitmapIdx = -1;
    int partiallyTransparent = 0;
    pvBuf = 0;
    BITMAPHEADER * bmpInfoHeader = 0;
    int v17 = 0;
    int cbNewBitmap_;

    if (!fSimplifiedImage) {
        iRes = pRec->uResID - 501;
        if (iRes < 0)
            return E_FAIL;

        unsigned cBitmapsOld = _cBitmaps;
        if (iRes < _cBitmaps) {
            bitmapIdx = _rgBitmapIndices[iRes];
            partiallyTransparent = TestBit(_rgfPartiallyTransparent.get(), iRes);
        } else {
            _cBitmaps = 2 * cBitmapsOld;
            if (!realloc(_rgBitmapIndices, sizeof(_rgBitmapIndices[0]) * (2 * cBitmapsOld)))
                return E_OUTOFMEMORY;

            if (_cBitmaps > cBitmapsOld)
                std::fill(&_rgBitmapIndices[cBitmapsOld], &_rgBitmapIndices[_cBitmaps], -1);

            if (!realloc(_rgfPartiallyTransparent, (_cBitmaps / CHAR_BIT) + 1))
                return E_OUTOFMEMORY;
        }
    }

    if (bitmapIdx != -1 && _fGlobal) {
        ePrimVal = TMT_BITMAPREF;
        goto LABEL_29;
    } else {
        if (fSimplifiedImage) {
            MARGINS sizingMargins = {};
            int valueSize = sizeof(sizingMargins);
            auto v37_ = _GetPropertyValue(
                _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                pRec->iState, TMT_SIZINGMARGINS, &sizingMargins, &valueSize);

            if (v37_ != HRESULT_FROM_WIN32(ERROR_NO_MATCH) && v37_ < 0)
                return v37_;

            MARGINS transparentMargins = {};
            valueSize = sizeof(transparentMargins);
            hr = _GetPropertyValue(_pbClassData, _cbClassData, pRec->iClass,
                                   pRec->iPart, pRec->iState,
                                   TMT_TRANSPARENTMARGINS, &transparentMargins,
                                   &valueSize);

            BYTE* pb = nullptr;
            int cbNewBitmap = 0;
            if ((hr + 2147483648) & 0x80000000 || hr == HRESULT_FROM_WIN32(ERROR_NO_MATCH))
                hr = _CreateImageFromProperties(
                    pImageProperties,
                    iImageCount,
                    &sizingMargins,
                    &transparentMargins,
                    &pb,
                    &cbNewBitmap);

            allocatedImage = malloc_ptr<BYTE>{pb};
            if (hr < 0)
                return hr;

            bmpInfoHeader = reinterpret_cast<BITMAPHEADER*>(allocatedImage.get());
        } else {
            auto hdr = reinterpret_cast<BITMAPHDR*>(pbData);
            if (hdr->size != 0) {
                if (!_pDecoder) {
                    _pDecoder = new(std::nothrow) CThemePNGDecoder();
                    if (!_pDecoder)
                        return E_OUTOFMEMORY;
                }

                ENSURE_HR(_pDecoder->ConvertToDIB(
                    static_cast<BYTE const*>(hdr->buffer), hdr->size,
                    (int*)&pvBuf));
                v17 = 1;
                bmpInfoHeader = _pDecoder->GetBitmapHeader();
            } else {
                bmpInfoHeader = static_cast<BITMAPHEADER*>(hdr->buffer);
            }
        }

        v44 = nullptr;
        int height = bmpInfoHeader->bmih.biHeight;
        if (fComposedImage)
            height /= 2;

        cbNewBitmap_ = 4 * bmpInfoHeader->bmih.biWidth * height;
        partiallyTransparent = 0;
        if (v17) {
            partiallyTransparent = pvBuf;
            if (partiallyTransparent) {
                if (!fSimplifiedImage)
                    SetBit(_rgfPartiallyTransparent.get(), iRes);
            }
        } else {
            if (fSimplifiedImage) {
                if (!v97 && iImageCount > 0) {
                    for (int i = 0; i < iImageCount; ++i) {
                        if ((pImageProperties[i].dwBackgroundColor & 0xFF000000) != 0xFF000000 ||
                            (pImageProperties[i].dwBorderColor & 0xFF000000) != 0xFF000000) {
                            partiallyTransparent = 1;
                            break;
                        }
                    }
                }
            } else if (bmpInfoHeader->bmih.biBitCount == 32) {
                partiallyTransparent = 1;
                SetBit(_rgfPartiallyTransparent.get(), iRes);
            }
        }

        if (!_fGlobal) {
            bitmapIdx = pfnCB->AddToDIBDataArray(nullptr, 0, 0);
            if (bitmapIdx == -1)
                return E_FAIL;
            ePrimVal = TMT_COLOR;
            goto LABEL_29;
        }

        hDC = GetWindowDC(0);
        if (!hDC) {
            return 0x8007000E;
        }

        if (partiallyTransparent && _fIsLiteVisualStyle) {
            if (pRec->lSymbolVal >= TMT_IMAGEFILE1 && pRec->lSymbolVal <= TMT_IMAGEFILE5 ||
                pRec->lSymbolVal >= TMT_GLYPHIMAGEFILE && pRec->lSymbolVal <= TMT_IMAGEFILE7 ||
                pRec->lSymbolVal >= TMT_COMPOSEDIMAGEFILE1 && pRec->lSymbolVal <= TMT_COMPOSEDGLYPHIMAGEFILE ||
                pRec->lSymbolVal >= TMT_COMPOSEDIMAGEFILE6 && pRec->lSymbolVal <= TMT_COMPOSEDIMAGEFILE7 ||
                _IsTrueSizeImage(pRec))
            {
                unsigned glyphColor = 0;
                unsigned glyphBGColor = 0;
                bool hasGlyphColor = false;
                bool hasGlyphBGColor = false;
                if (IsHighContrastMode()) {
                    HIGHCONTRASTCOLOR hcGlyphColor;
                    HIGHCONTRASTCOLOR hcGlyphBGColor;
                    int valueSize = sizeof(hcGlyphColor);

                    if (SUCCEEDED(_GetPropertyValue(
                        _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                        pRec->iState, TMT_HCGLYPHCOLOR, &hcGlyphColor, &valueSize))) {
                        hasGlyphColor = true;
                        glyphColor = static_cast<unsigned>(MapEnumToSysColor(hcGlyphColor));
                        iRes = hcGlyphColor;
                    }

                    if (SUCCEEDED(_GetPropertyValue(
                        _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                        pRec->iState, TMT_HCGLYPHBGCOLOR, &hcGlyphBGColor, &valueSize))) {
                        hasGlyphBGColor = true;
                        glyphBGColor = static_cast<unsigned>(MapEnumToSysColor(hcGlyphBGColor));
                        pvBuf = glyphBGColor;
                        iRes = hcGlyphBGColor;
                    }
                }

                if (fComposedImage || hasGlyphColor || hasGlyphBGColor)
                    doGlyphColorization = true;

                if (hr < 0)
                    goto LABEL_70;

                if (doGlyphColorization) {
                    hr = _EnsureBufferSize(cbNewBitmap_);
                    if (hr < 0)
                        goto LABEL_70;

                    v44 = make_unique_malloc<BYTE[]>(cbNewBitmap_).release();
                    if (v44) {
                        if (fComposedImage) {
                            cbNewBitmap_ = 4;
                            iImageCount = 1;
                            if (_FindVSRecord(
                                _pbClassData, _cbClassData, pRec->iClass, pRec->iPart,
                                pRec->iState, TMT_IMAGECOUNT, &pRecord) >= 0) {
                                hr = LoadVSRecordData(_hInst, pRecord, &iImageCount, &cbNewBitmap_);
                                if (hr < 0)
                                    goto LABEL_70;
                                if (iImageCount <= 0) {
                                    hr = E_INVALIDARG;
                                    goto LABEL_70;
                                }
                            }

                            ColorizeGlyphByAlphaComposition(
                                v44, &bmpInfoHeader->bmih, iImageCount,
                                hasGlyphBGColor ? &glyphBGColor : nullptr,
                                hasGlyphColor ? &glyphColor : nullptr);
                        } else {
                            if (hasGlyphBGColor)
                                ColorizeGlyphByAlpha(v44, &bmpInfoHeader->bmih, glyphBGColor);
                        }
                    }
                }
            }
        }
    }

LABEL_70:
    if (!doGlyphColorization) {
        if (hr >= 0) {
            if (bmpInfoHeader->bmih.biBitCount == 32) {
                v44 = (BYTE*)malloc(cbNewBitmap_);
                if (v44)
                    memcpy_s(v44, cbNewBitmap_, (BYTE*)bmpInfoHeader + 4 * bmpInfoHeader->bmih.biClrUsed + bmpInfoHeader->bmih.biSize, cbNewBitmap_);
            } else {
                hr = _EnsureBufferSize(4 * bmpInfoHeader->bmih.biWidth * bmpInfoHeader->bmih.biHeight);
                if (hr >= 0) {
                    v44 = (BYTE*)malloc(cbNewBitmap_);
                    if (v44)
                        Convert24to32BPP(v44, &bmpInfoHeader->bmih);
                }
            }
        }
    }

LABEL_75:
    ReleaseDC(nullptr, hDC);
    if (!v44)
        return HRESULT_FROM_WIN32(ERROR_OUTOFMEMORY);

    ePrimVal = TMT_HBITMAP;
    {
        short w = bmpInfoHeader->bmih.biWidth;
        short h = bmpInfoHeader->bmih.biHeight;
        if (fComposedImage)
            h /= 2;

        bitmapIdx = pfnCB->AddToDIBDataArray(v44, w, h);
    }
    if (bitmapIdx == -1)
        return E_FAIL;

    if (!fSimplifiedImage)
        _rgBitmapIndices[iRes] = bitmapIdx;

LABEL_29:
    if (hr < 0)
        return hr;

    if (_fGlobal) {
        TMBITMAPHEADER tmhdr;
        tmhdr.dwSize = sizeof(tmhdr);
        tmhdr.iBitmapIndex = bitmapIdx;
        tmhdr.fPartiallyTransparent = partiallyTransparent;
        hr = pfnCB->AddData(lDibSymbolVal, ePrimVal, &tmhdr, sizeof(tmhdr));
    } else {
        BITMAPHEADER bmp = {};
        bmp.bmih.biSize = sizeof(BITMAPINFOHEADER);
        bmp.bmih.biWidth = bmpInfoHeader->bmih.biWidth;
        bmp.bmih.biHeight = bmpInfoHeader->bmih.biHeight;
        bmp.bmih.biPlanes = 1;
        bmp.bmih.biBitCount = 32;
        bmp.bmih.biCompression = 3;
        bmp.bmih.biClrUsed = 3;
        bmp.bmih.biSizeImage = 4 * bmpInfoHeader->bmih.biWidth * bmp.bmih.biHeight;
        bmp.masks[0] = 0xFF0000;
        bmp.masks[1] = 0xFF00;

        int v25 = sizeof(TMBITMAPHEADER) + sizeof(BITMAPHEADER) +
            4 * bmp.bmih.biWidth * bmp.bmih.biHeight;

        auto buffer = make_unique_malloc<BYTE[]>(v25);
        if (!buffer)
            return E_OUTOFMEMORY;

        auto v29 = (BITMAPHEADER*)(buffer.get() + sizeof(TMBITMAPHEADER));
        if (bmpInfoHeader->bmih.biBitCount == 32) {
            memcpy(v29, bmpInfoHeader, v25 - sizeof(TMBITMAPHEADER));
        } else {
            *v29 = *bmpInfoHeader;
            v29->masks[2] = 0xFF;
            Convert24to32BPP(buffer.get() + 64, &bmpInfoHeader->bmih);
        }

        auto tmhdr = reinterpret_cast<TMBITMAPHEADER*>(buffer.get());
        tmhdr->dwSize = sizeof(tmhdr);
        tmhdr->iBitmapIndex = bitmapIdx;
        tmhdr->fPartiallyTransparent = partiallyTransparent;
        hr = pfnCB->AddData(lDibSymbolVal, 2, buffer.get(), v25);
    }

    return hr;
}

HRESULT CVSUnpack::_AddVSDataRecord(IParserCallBack* pfnCB, HMODULE hInst,
                                    VSRECORD* pRec)
{
    array<BYTE, 256> localBuffer;
    int pcbBuf = pRec->cbData;

    BYTE* pvBuf = localBuffer.data();
    if (pRec->cbData > localBuffer.size()) {
        pvBuf = new(nothrow) BYTE[pRec->cbData];
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

    if (pvBuf != localBuffer.data())
        delete[] pvBuf;

    return hr;
}

HRESULT CVSUnpack::_InitializePlateauPpiMapping(VSRECORD* pRec)
{
    int ppiValue;
    int size = sizeof(ppiValue);
    ENSURE_HR(LoadVSRecordData(_hInst, pRec, &ppiValue, &size));
    _rgPlateauPpiMapping[pRec->lSymbolVal - TMT_PPIPLATEAU1] = ppiValue;
    return S_OK;
}

HRESULT CVSUnpack::_ClearDpiRecords()
{
    fill_zero(_rgImageDpiRec);
    fill_zero(_rgImageRec);
    fill_zero(_rgComposedImageRec);
    return S_OK;
}

HRESULT CVSUnpack::_FlushDelayedRecords(IParserCallBack* pfnCB)
{
    int candidateDpis[DPI_PLATEAU_COUNT];
    std::fill(candidateDpis, candidateDpis + DPI_PLATEAU_COUNT, -1);
    uint8_t selectedDpiAssets[DPI_PLATEAU_COUNT] = {};

    HRESULT hr = S_OK;
    bool hasDpiRecord = false;

    for (int i = 0; i < DPI_PLATEAU_COUNT; ++i) {
        VSRECORD* rec = _rgImageDpiRec[i];
        if (!rec) {
            selectedDpiAssets[i] = PL_1_8x;
            continue;
        }

        int minDpi = -1;
        int length = sizeof(minDpi);
        hr = LoadVSRecordData(_hInst, rec, &minDpi, &length);
        if (hr < 0)
            break;

        hasDpiRecord = true;
        candidateDpis[i] = minDpi;
    }

    if (hasDpiRecord) {
        for (int plateau = DPI_PLATEAU_UNSUPPORTED; plateau < DPI_PLATEAU_COUNT; ++plateau) {
            int targetDpi;
            if (plateau == DPI_PLATEAU_UNSUPPORTED)
                targetDpi = GetScreenDpi();
            else if (g_DpiInfo.IsPlateauCurrentlyPresent((PLATEAU_INDEX)plateau))
                targetDpi = GetDpiPlateauByIndex(plateau);
            else
                continue;

            int flagIdx = DPI_PLATEAU_UNSUPPORTED;
            int closestDst = INT_MAX;
            for (int i = 0; i < DPI_PLATEAU_COUNT; ++i) {
                auto dst = targetDpi - candidateDpis[i];
                if (candidateDpis[i] != -1 && targetDpi >= candidateDpis[i] &&
                    dst < closestDst) {
                    flagIdx = i;
                    closestDst = dst;
                    if (targetDpi == candidateDpis[i])
                        break;
                }
            }

            if (flagIdx != DPI_PLATEAU_UNSUPPORTED)
                selectedDpiAssets[flagIdx] = PL_1_4x;
        }
    }

    for (int i = 0, idx = 0; i < _rgImageDpiRec.size(); ++i) {
        if (selectedDpiAssets[i] == PL_1_4x) {
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgImageDpiRec[i], Map_Ordinal_To_MINDPI(idx));
            ++idx;
        }
    }

    for (int i = 0, idx = 0; i < _rgImageRec.size(); ++i) {
        if (_rgImageRec[i] && selectedDpiAssets[i] != PL_1_0x) {
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgImageRec[i], Map_Ordinal_To_IMAGEFILE(idx));
            ++idx;
        }
    }

    for (int i = 0, idx = 0; i < _rgComposedImageRec.size(); ++i) {
        if (_rgComposedImageRec[i] && selectedDpiAssets[i] != PL_1_0x) {
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgComposedImageRec[i], Map_Ordinal_To_COMPOSEDIMAGEFILE(idx));
            ++idx;
        }
    }

    fill_zero(_rgImageDpiRec);
    fill_zero(_rgImageRec);
    fill_zero(_rgComposedImageRec);
    return hr;
}

HRESULT CVSUnpack::_ClearPlateauRecords()
{
    fill_zero(_rgPlateauRec);
    fill_zero(_rgImagePRec);
    fill_zero(_rgGlyphImagePRec);
    fill_zero(_rgContentMarginsPRec);
    fill_zero(_rgSizingMarginsPRec);
    return S_OK;
}

HRESULT CVSUnpack::_FlushDelayedPlateauRecords(IParserCallBack* pfnCB)
{
    int const currentPpi = pfnCB->GetScreenPpi();
    int closestPlateauIdx = -1;
    int ppiDist = INT_MAX;

    for (int i = 0; i < DPI_PLATEAU_COUNT; ++i) {
        int curDist = currentPpi - _rgPlateauPpiMapping[i];
        if (_rgPlateauRec[i] && curDist >= 0 && curDist < ppiDist) {
            closestPlateauIdx = i;
            ppiDist = curDist;
        }
    }

    HRESULT hr = S_OK;
    if (closestPlateauIdx >= 0) {
        if (_rgImagePRec[closestPlateauIdx])
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgImagePRec[closestPlateauIdx], TMT_IMAGEFILE);

        if (hr >= 0 && _rgGlyphImagePRec[closestPlateauIdx])
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgGlyphImagePRec[closestPlateauIdx],
                TMT_GLYPHIMAGEFILE);

        if (hr >= 0 && _rgContentMarginsPRec[closestPlateauIdx])
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgContentMarginsPRec[closestPlateauIdx],
                TMT_CONTENTMARGINS);

        if (hr >= 0 && _rgSizingMarginsPRec[closestPlateauIdx])
            hr = _FixSymbolAndAddVSDataRecord(
                pfnCB, _rgSizingMarginsPRec[closestPlateauIdx],
                TMT_SIZINGMARGINS);
    }

    _ClearPlateauRecords();
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
    ENSURE_HR(pfnCB->AddData(TMT_SCALEDBACKGROUND, TMT_HBITMAP, &hdr, sizeof(hdr)));
    return S_OK;
}

HRESULT CVSUnpack::_SavePlateauRecord(VSRECORD* pRec)
{
    int symbol = pRec->lSymbolVal;
    int idx;

    switch (symbol) {
    case TMT_IMAGEPLATEAU1:
    case TMT_IMAGEPLATEAU2:
    case TMT_IMAGEPLATEAU3:
        idx = symbol - TMT_IMAGEPLATEAU1;
        _rgImagePRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_GLYPHIMAGEPLATEAU1:
    case TMT_GLYPHIMAGEPLATEAU2:
    case TMT_GLYPHIMAGEPLATEAU3:
        idx = symbol - TMT_GLYPHIMAGEPLATEAU1;
        _rgGlyphImagePRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_CONTENTMARGINSPLATEAU1:
    case TMT_CONTENTMARGINSPLATEAU2:
    case TMT_CONTENTMARGINSPLATEAU3:
        idx = symbol - TMT_CONTENTMARGINSPLATEAU1;
        _rgContentMarginsPRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;

    case TMT_SIZINGMARGINSPLATEAU1:
    case TMT_SIZINGMARGINSPLATEAU2:
    case TMT_SIZINGMARGINSPLATEAU3:
        idx = symbol - TMT_SIZINGMARGINSPLATEAU1;
        _rgSizingMarginsPRec[idx] = pRec;
        _rgPlateauRec[idx] = true;
        break;
    }

    return S_OK;
}

HRESULT CVSUnpack::_FixSymbolAndAddVSDataRecord(
    IParserCallBack* pfnCB, VSRECORD* pRec, int lSymbolVal)
{
    char buffer[sizeof(VSRECORD) + 260];

    if (pRec->lSymbolVal != lSymbolVal) {
        if (pRec->uResID != 0) {
            memcpy(&buffer, pRec, sizeof(VSRECORD));
        } else {
            size_t size = sizeof(VSRECORD) + pRec->cbData;
            if (size > countof(buffer))
                return E_FAIL;
            memcpy(&buffer, pRec, size);
        }

        pRec = reinterpret_cast<VSRECORD*>(&buffer);
        pRec->lSymbolVal = lSymbolVal;
    }

    return _AddVSDataRecord(pfnCB, _hInst, pRec);
}

HRESULT CVSUnpack::_SaveRecord(VSRECORD* pRec)
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

bool CVSUnpack::_DelayRecord(VSRECORD* pRec)
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
