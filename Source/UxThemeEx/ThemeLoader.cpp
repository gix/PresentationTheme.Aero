#include "ThemeLoader.h"

#include "DpiInfo.h"
#include "BorderFill.h"
#include "Debug.h"
#include "Handle.h"
#include "RenderObj.h"
#include "TextDraw.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"

#include <algorithm>
#include <array>
#include <shlwapi.h>
#include <strsafe.h>
#include <vssym32.h>
#include <winnt.h>
#include <winternl.h>
#include <ntstatus.h>

NTSYSAPI NTSTATUS NTAPI NtOpenSection(
    _Out_ PHANDLE            SectionHandle,
    _In_  ACCESS_MASK        DesiredAccess,
    _In_  POBJECT_ATTRIBUTES ObjectAttributes);

namespace uxtheme
{

int CThemeLoader::AddToDIBDataArray(void* pDIBBits, short width, short height)
{
    DIBDATA d;
    d.pDIBBits = pDIBBits;
    d.dibReuseData.iDIBBitsOffset = 0;
    d.dibReuseData.iWidth = width;
    d.dibReuseData.iHeight = height;
    _rgDIBDataArray.push_back(d);
    return _rgDIBDataArray.size() - 1;
}

HRESULT CThemeLoader::AddBaseClass(int idClass, int idBaseClass)
{
    _rgBaseClassIds.push_back(idBaseClass);
    return S_OK;
}

static unsigned const DefaultColors[31] = {
    0xC8D0D4,
    0xA56E3A,
    0x6A240A,
    0x808080,
    0xC8D0D4,
    0xFFFFFF,
    0x000000,
    0x000000,
    0x000000,
    0xFFFFFF,
    0xC8D0D4,
    0xC8D0D4,
    0x808080,
    0x6A240A,
    0xFFFFFF,
    0xC8D0D4,
    0x808080,
    0x808080,
    0x000000,
    0xC8D0D4,
    0xFFFFFF,
    0x404040,
    0xC8D0D4,
    0x000000,
    0xE1FFFF,
    0xB5B5B5,
    0x800000,
    0xF0CAA6,
    0xC0C0C0,
    0xE1D3CE,
    0xF0F4F4,
};

static void SetFont(LOGFONTW *plf, wchar_t const* lszFontName, int iPointSize)
{
    fill_zero(plf);
    plf->lfWeight = 400;
    plf->lfCharSet = 1;
    plf->lfHeight = -MulDiv(iPointSize, 96, 72);
    StringCchCopyW(plf->lfFaceName, countof(plf->lfFaceName), lszFontName);
}

static HRESULT InitThemeMetrics(LOADTHEMEMETRICS* tm)
{
    memset(tm, 0, sizeof(LOADTHEMEMETRICS));
    SetFont(&tm->lfFonts[0], L"tahoma bold", 8);
    SetFont(&tm->lfFonts[1], L"tahoma", 8);
    SetFont(&tm->lfFonts[2], L"tahoma", 8);
    SetFont(&tm->lfFonts[3], L"tahoma", 8);
    SetFont(&tm->lfFonts[4], L"tahoma", 8);
    SetFont(&tm->lfFonts[5], L"tahoma", 8);
    tm->iSizes[0] = 1;
    tm->iSizes[1] = 16;
    tm->iSizes[2] = 16;
    tm->iSizes[3] = 18;
    tm->iSizes[4] = 19;
    tm->iSizes[5] = 12;
    tm->iSizes[6] = 19;
    tm->iSizes[7] = 18;
    tm->iSizes[8] = 19;
    tm->iSizes[9] = 0;
    tm->iSizes[10] = 0;
    tm->iStringOffsets[0] = 0;
    tm->iStringOffsets[1] = 0;
    tm->wsStrings[0] = L"";
    tm->wsStrings[1] = L"";
    tm->iInts[0] = 16;
    memcpy(tm->crColors, DefaultColors, sizeof(tm->crColors));
    return S_OK;
}

CThemeLoader::CThemeLoader()
    : _iGlobalsOffset(0)
    , _iSysMetricsOffset(0)
    , _iGlobalsTextObj(0)
    , _iGlobalsDrawObj(0)
    , _pbLocalData(nullptr)
    , _iLocalLen(0)
    , _iEntryHdrLevel(-1)
    , _fGlobalTheme(0)
    , _hdr(nullptr)
{
    SYSTEM_INFO systemInfo;
    GetSystemInfo(&systemInfo);

    InitThemeMetrics(&_LoadThemeMetrics);
    _dwPageSize = systemInfo.dwPageSize;
    _wCurrentLangID = 0;
    _iCurrentScreenPpi = 96;
}

HRESULT CThemeLoader::AllocateThemeFileBytes(char* upb, unsigned dwAdditionalLen)
{
    bool overflowsPage = (uintptr_t)upb / _dwPageSize != ((uintptr_t)upb + dwAdditionalLen) / _dwPageSize;

    if (overflowsPage) {
        bool z = dwAdditionalLen <= (unsigned __int64)((char *)_LoadingThemeFile._pbSharableData - upb + 0x7FFFFFFF);
        if (!z)
            return E_OUTOFMEMORY;

        SIZE_T s = (SIZE_T)(upb + dwAdditionalLen - (uint64_t)_LoadingThemeFile._pbSharableData + 1);
        if (!VirtualAlloc(_LoadingThemeFile._pbSharableData, s, MEM_COMMIT, PAGE_READWRITE))
            return E_OUTOFMEMORY;
    }

    return S_OK;
}

HRESULT CThemeLoader::EmitEntryHdr(MIXEDPTRS* u, short propnum, char privnum)
{
    if (_iEntryHdrLevel == 5)
        return E_FAIL;

    if (_LoadingThemeFile._pbSharableData)
        ENSURE_HR(AllocateThemeFileBytes(u->pb, sizeof(ENTRYHDR)));

    auto hdr = reinterpret_cast<ENTRYHDR*>(u->pb);
    hdr->usTypeNum = propnum;
    hdr->ePrimVal = privnum;
    hdr->dwDataLen = 0;
    RegisterPtr(hdr);

    ++_iEntryHdrLevel;
    u->pb = reinterpret_cast<char*>(hdr + 1);

    _pbEntryHdrs[_iEntryHdrLevel] = hdr;
    return S_OK;
}

int CThemeLoader::EndEntry(MIXEDPTRS* u)
{
    auto hdr = _pbEntryHdrs[_iEntryHdrLevel];

    unsigned len = (uintptr_t)u->pi - (uintptr_t)hdr - 8;
    unsigned alignedLen = Align8(len);

    uint8_t padding = alignedLen - len;
    if (_LoadingThemeFile._pbSharableData && FAILED(AllocateThemeFileBytes(u->pb, padding)))
        return -1;

    u->pb += padding;
    hdr->dwDataLen = alignedLen;
    --_iEntryHdrLevel;

    return padding;
}

HRESULT CThemeLoader::EmitAndCopyBlock(MIXEDPTRS* u, void const* pSrc, unsigned dwLen)
{
    unsigned paddedLen = Align8(dwLen);
    ENSURE_HR(AllocateThemeFileBytes(u->pb, paddedLen));

    memcpy(u->pb, pSrc, dwLen);
    u->pb += paddedLen;
    return S_OK;
}

BOOL CThemeLoader::IndexExists(
    wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId)
{
    APPCLASSLOCAL const* matchedClass = nullptr;

    for (auto const& cls : _LocalIndexes) {
        auto currApp = cls.csAppName.c_str();
        auto currCls = cls.csClassName.c_str();

        bool reqName = pszAppName && *pszAppName;
        bool hasName = currApp && *currApp;

        if (((!reqName && !hasName) || (reqName && hasName && AsciiStrCmpI(pszAppName, currApp) == 0)) &&
            AsciiStrCmpI(pszClassName, currCls) == 0) {
            matchedClass = &cls;
            break;
        }
    }

    if (matchedClass) {
        for (auto const& psi : matchedClass->PartStateIndexes) {
            if (psi.iPartId == iPartId && psi.iStateId == iStateId)
                return TRUE;
        }
    }

    return FALSE;
}

HRESULT CThemeLoader::AddMissingParent(
    wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId)
{
    int beginIdx = GetNextDataIndex();
    unsigned value = 0;
    ENSURE_HR(AddData(12, 12, &value, sizeof(value)));

    int length = GetNextDataIndex() - beginIdx;
    ENSURE_HR(AddIndexInternal(pszAppName, pszClassName, iPartId, iStateId, beginIdx, length));
    return S_OK;
}

HRESULT CThemeLoader::AddIndex(
    wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId, int iIndex, int iLen)
{
    if (iPartId && !IndexExists(pszAppName, pszClassName, 0, 0))
        ENSURE_HR(AddMissingParent(pszAppName, pszClassName, 0, 0));
    if (iStateId && !IndexExists(pszAppName, pszClassName, iPartId, 0))
        ENSURE_HR(AddMissingParent(pszAppName, pszClassName, iPartId, 0));

    ENSURE_HR(AddIndexInternal(pszAppName, pszClassName, iPartId, iStateId, iIndex, iLen));
    return S_OK;
}

HRESULT CThemeLoader::AddIndexInternal(
    wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId,
    int iStateId, int iIndex, int iLen)
{
    APPCLASSLOCAL* cls = nullptr;

    for (auto& c : _LocalIndexes) {
        auto currApp = c.csAppName.c_str();
        auto currCls = c.csClassName.c_str();

        bool reqName = pszAppName && *pszAppName;
        bool hasName = currApp && *currApp;

        if (((!reqName && !hasName) || (reqName && hasName && AsciiStrCmpI(pszAppName, currApp) == 0)) &&
            AsciiStrCmpI(pszClassName, currCls) == 0) {
            cls = &c;
            break;
        }
    }

    if (cls) {
        for (auto const& psi : cls->PartStateIndexes) {
            if (psi.iPartId == iPartId && psi.iStateId == iStateId)
                return 0x800700B7;
        }
    }

    if (!cls) {
        APPCLASSLOCAL c;
        c.csAppName = pszAppName;
        c.csClassName = pszClassName;
        _LocalIndexes.push_back(c);
        cls = &_LocalIndexes.back();
    }

    if (iPartId > cls->iMaxPartNum)
        cls->iMaxPartNum = iPartId;

    PART_STATE_INDEX psi;
    psi.iPartId = iPartId;
    psi.iStateId = iStateId;
    psi.iIndex = iIndex;
    psi.iLen = iLen;
    cls->PartStateIndexes.push_back(psi);

    return S_OK;
}

HRESULT CThemeLoader::AddData(short sTypeNum, unsigned char ePrimVal,
                              void const* pData, unsigned dwLen)
{
    if (ePrimVal == TMT_FONT) {
        unsigned short idx = 0;
        ENSURE_HR(GetFontTableIndex(static_cast<LOGFONTW const*>(pData), &idx));
        pData = &idx;
        dwLen = sizeof(idx);
    }

    return AddDataInternal(sTypeNum, ePrimVal, pData, dwLen);
}

HRESULT CThemeLoader::AddDataInternal(short sTypeNum, char ePrimVal, void const* pData, unsigned dwLen)
{
    if (dwLen + 16 < dwLen || dwLen + 16 > 0x7FFFFFFF - _iLocalLen)
        return 0x80070008;

    MIXEDPTRS u;
    u.pb = &_pbLocalData[_iLocalLen];

    bool overflowsPage = (uintptr_t)u.pb / _dwPageSize != ((uintptr_t)u.pb + dwLen + 15) / _dwPageSize;

    if ((overflowsPage || _iLocalLen == 0)
        && !VirtualAlloc(_pbLocalData, _iLocalLen + dwLen + 16, MEM_COMMIT, PAGE_READWRITE))
        return 0x80070008;

    ENSURE_HR(EmitEntryHdr(&u, sTypeNum, ePrimVal));

    if (dwLen) {
        memcpy(u.pb, pData, dwLen);
        u.pb += dwLen;
    }

    _iLocalLen += 8 + dwLen + EndEntry(&u);

    return S_OK;
}

struct _VISUALSTYLELOAD
{
    unsigned cbStruct;
    unsigned ulFlags;
    HMODULE hInstVS;
    wchar_t const* pszColorVariant;
    wchar_t const* pszSizeVariant;
    IParserCallBack* pfnCB;
};

HRESULT VSLoad(_VISUALSTYLELOAD* pvsl, BOOL fIsLiteVisualStyle)
{
    if (!pvsl
        || pvsl->cbStruct != 40
        || !pvsl->hInstVS
        || !pvsl->pfnCB
        || !pvsl->pszColorVariant
        || !*pvsl->pszColorVariant
        || !pvsl->pszSizeVariant
        || !*pvsl->pszSizeVariant)
        return E_INVALIDARG;

    auto unpack = make_unique_nothrow<CVSUnpack>();
    if (!unpack)
        return E_OUTOFMEMORY;

    ENSURE_HR(unpack->Initialize(pvsl->hInstVS, 0, pvsl->ulFlags & 1,
                                 fIsLiteVisualStyle));
    ENSURE_HR(unpack->LoadRootMap(pvsl->pfnCB));
    ENSURE_HR(unpack->LoadClassDataMap(pvsl->pszColorVariant,
                                       pvsl->pszSizeVariant, pvsl->pfnCB));
    ENSURE_HR(unpack->LoadBaseClassDataMap(pvsl->pfnCB));
    ENSURE_HR(unpack->LoadAnimationDataMap(pvsl->pfnCB));
    return S_OK;
}

void CThemeLoader::FreeLocalTheme()
{
    if (_pbLocalData) {
        VirtualFree(_pbLocalData, 0, MEM_RELEASE);
        _pbLocalData = nullptr;
        _iLocalLen = 0;
    }

    _LocalIndexes.clear();
    _rgBaseClassIds.clear();
}

HRESULT CThemeLoader::LoadTheme(HMODULE hInst, wchar_t const* pszThemeName,
                                HANDLE* phReuseSection, BOOL fGlobalTheme)
{
    wchar_t const* pszColorParam = L"NormalColor";
    wchar_t const* pszSizeParam = L"NormalSize";

    if (phReuseSection)
        *phReuseSection = nullptr;

    FreeLocalTheme();
    _fGlobalTheme = fGlobalTheme;
    g_DpiInfo.Clear();
    g_DpiInfo.Ensure(0);

    size_t const MinReserve = 10 * 1024 * 1024;

    HANDLE hFile = CreateFileW(
        pszThemeName, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr);
    if (!hFile)
        return MakeErrorLast();

    size_t fileSize = GetFileSize(hFile, nullptr);
    CloseHandle(hFile);

    _pbLocalData = static_cast<char*>(VirtualAlloc(
        nullptr, std::max(fileSize, MinReserve), MEM_RESERVE, PAGE_READWRITE));

    if (!_pbLocalData)
        return 0x8007000E;

    BOOL isLiteStyle = FALSE;
    if (pszThemeName)
        isLiteStyle = StrRStrIW(pszThemeName, nullptr, L"aerolite.msstyles") ? TRUE : FALSE;

    _VISUALSTYLELOAD vsl = {};
    vsl.hInstVS = hInst;
    vsl.cbStruct = 40;
    vsl.ulFlags = fGlobalTheme != 0;
    vsl.pszColorVariant = pszColorParam;
    vsl.pszSizeVariant = pszSizeParam;
    vsl.pfnCB = this;
    ENSURE_HR(VSLoad(&vsl, isLiteStyle));

    ENSURE_HR(PackAndLoadTheme(
        hFile,
        pszThemeName,
        pszColorParam,
        pszSizeParam,
        std::max(fileSize, MinReserve),
        nullptr,
        0,
        nullptr,
        0,
        phReuseSection,
        nullptr));

    return S_OK;
}

HRESULT CThemeLoader::EmitString(
    MIXEDPTRS* u, wchar_t const* pszSrc, unsigned cchSrc, int* piOffSet)
{
    unsigned paddedLen = Align8(2 * (cchSrc + 1));
    ENSURE_HR(AllocateThemeFileBytes(u->pb, paddedLen));
    ENSURE_HR(StringCchCopyW((wchar_t*)u->pb, cchSrc + 1, pszSrc));

    if (piOffSet)
        *piOffSet = (char*)(u->pi) - (char*)(_LoadingThemeFile._pbSharableData);
    u->pb += paddedLen;

    return S_OK;
}

struct PTOEntry
{
    void* ptr;
    wchar_t const* className;
    int partId;
    int stateId;
};
std::vector<PTOEntry> g_textObjEntries;
std::vector<PTOEntry> g_drawObjEntries;

HRESULT CThemeLoader::EmitObject(
    MIXEDPTRS* u, short propnum, char privnum, void* pHdr, unsigned dwHdrLen,
    void* pObj, unsigned dwObjLen, CRenderObj* pRender)
{
    ENSURE_HR(EmitEntryHdr(u, propnum, privnum));
    ENSURE_HR(AllocateThemeFileBytes(u->pb, dwHdrLen));

    memcpy(u->pb, pHdr, dwHdrLen);
    RegisterPtr((PARTOBJHDR*)u->pb);
    u->pb += dwHdrLen;

    auto paddedLen = Align8(dwObjLen);
    ENSURE_HR(AllocateThemeFileBytes(u->pb, paddedLen));
    memcpy(u->pb, pObj, dwObjLen);
    auto poh = (PARTOBJHDR*)pHdr;
    if (dwObjLen == sizeof(CTextDraw)) {
        RegisterPtr((CTextDraw*)u->pb);
        g_textObjEntries.push_back({u->pb, pRender->_pszClassName, poh->iPartId, poh->iStateId});
    } else {
        RegisterPtr((CDrawBase*)u->pb);
        g_drawObjEntries.push_back({u->pb, pRender->_pszClassName, poh->iPartId, poh->iStateId});
    }
    u->pb += paddedLen;

    EndEntry(u);

    return S_OK;
}

HRESULT CThemeLoader::GetFontTableIndex(LOGFONTW const* pFont, unsigned short* pIndex)
{
    if (_fontTable.empty()) {
        LOGFONTW t = {};
        _fontTable.push_back(t);
    }

    for (size_t i = 0; i < _fontTable.size(); ++i) {
        if (!memcmp(&_fontTable[i], pFont, sizeof(LOGFONTW))) {
            *pIndex = i;
            return S_OK;
        }
    }

    _fontTable.push_back(*pFont);
    *pIndex = _fontTable.size() - 1;

    return S_OK;
}

HRESULT CThemeLoader::PackDrawObject(
    MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId)
{
    int bgType;
    if (pRender->ExternalGetEnumValue(iPartId, iStateId/*0*/, TMT_BGTYPE, &bgType) < 0)
        bgType = BT_BORDERFILL;

    PARTOBJHDR partHdr;
    partHdr.iPartId = iPartId;
    partHdr.iStateId = iStateId/*0*/;

    if (bgType == BT_NONE || bgType == BT_BORDERFILL) {
        CBorderFill borderFill;
        ENSURE_HR(borderFill.PackProperties(
            pRender, bgType == BT_NONE, iPartId, iStateId/*0*/));
        return EmitObject(u, TMT_DRAWOBJ, TMT_DRAWOBJ, &partHdr,
                          sizeof(partHdr), &borderFill, sizeof(borderFill), pRender);
    }

    CMaxImageFile imgFile;
    ENSURE_HR(imgFile.PackProperties(pRender, iPartId, iStateId/*0*/));

    DIBINFO* pScaledDibInfo = nullptr;
    HRESULT hr = imgFile.CreateScaledBackgroundImage(
        pRender, iPartId, iStateId/*0*/, &pScaledDibInfo);
    if (hr < 0)
        return hr;
    if (hr == 0)
        ENSURE_HR(MakeStockObject(pRender, pScaledDibInfo));

    for (int i = 0; ; ++i) {
        auto pdi = imgFile.EnumImageFiles(i);
        if (!pdi)
            break;
        ENSURE_HR(PackImageFileInfo(pdi, &imgFile, u, pRender, iPartId,
                                    iStateId));
    }

    return EmitObject(u, TMT_DRAWOBJ, TMT_DRAWOBJ, &partHdr, sizeof(partHdr), &imgFile,
                      sizeof(CImageFile) + imgFile._iMultiImageCount * sizeof(DIBINFO), pRender);
}

BOOL CThemeLoader::KeyTextPropertyFound(int iStateDataOffset)
{
    for (auto hdr = (ENTRYHDR*)((char*)_LoadingThemeFile._pbSharableData + iStateDataOffset);
         hdr->usTypeNum != TMT_12;
         hdr = hdr->Next())
    {
        if (CTextDraw::KeyProperty(hdr->usTypeNum))
            return TRUE;
    }

    return FALSE;
}

HRESULT CThemeLoader::PackTextObjects(
    MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, int fGlobals)
{
    for (int partId = 0; partId <= iMaxPart; ++partId) {
        int offset = GetPartOffset(pRender, partId);
        if (offset == -1)
            continue;

        auto entryHdr = (ENTRYHDR*)((char*)_LoadingThemeFile._pbSharableData + offset);
        if (entryHdr->usTypeNum == TMT_STATEJUMPTBL) {
            auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entryHdr + 1);
            auto jumpTable = (int*)(jumpTableHdr + 1);

            for (int stateId = 0; stateId <= jumpTableHdr->cStates - 1; ++stateId) {
                int so = jumpTable[stateId];
                if (so != -1 && (fGlobals || KeyTextPropertyFound(so))) {
                    ENSURE_HR(PackTextObject(uOut, pRender, partId, stateId));
                    fGlobals = 0;
                }
            }
        } else if (fGlobals || KeyTextPropertyFound(offset)) {
            ENSURE_HR(PackTextObject(uOut, pRender, partId, 0));
        }
    }

    return S_OK;
}

HRESULT CThemeLoader::PackTextObject(
    MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId)
{
    PARTOBJHDR partHdr;
    partHdr.iPartId = iPartId;
    partHdr.iStateId = iStateId;

    CTextDraw textDraw;
    ENSURE_HR(textDraw.PackProperties(pRender, iPartId, iStateId));
    return EmitObject(u, TMT_TEXTOBJ, TMT_TEXTOBJ, &partHdr, sizeof(partHdr),
                      &textDraw, sizeof(textDraw), pRender);
}

int CThemeLoader::GetScreenPpi()
{
    return _iCurrentScreenPpi;
}

HRESULT CThemeLoader::PackMetrics()
{
    __int64 v1;
    APPCLASSLOCAL *v2;
    int v3;
    APPCLASSLOCAL *v5;
    __int64 v6;
    PART_STATE_INDEX *pIndex;
    ENTRYHDR *ptr;
    ENTRYHDR *end;
    LOGFONTW *v17;
    signed __int64 v18;
    __int64 v19;

    v1 = _LocalIndexes.size();
    v2 = 0i64;
    v3 = 0;
    if ((signed int)v1 > 0)
    {
        v5 = _LocalIndexes.data();
        v6 = 0i64;
        do
        {
            v2 = v5;
            if (!AsciiStrCmpI(v5->csClassName.c_str(), L"SysMetrics"))
                break;
            ++v3;
            ++v6;
            ++v5;
        } while (v6 < v1);
    }

    if (v3 != v1 && v2->PartStateIndexes.size())
    {
        pIndex = v2->PartStateIndexes.data();
        ptr = (ENTRYHDR *)&_pbLocalData[pIndex->iIndex];
        end = (ENTRYHDR *)((char *)ptr + pIndex->iLen);

        while (ptr < end && ptr->usTypeNum != 12) {
            char const* v11 = (char const*)&ptr[1];
            auto v21 = *(short*)ptr;

            switch ((unsigned char)ptr->ePrimVal) {
            case TMT_STRING:
                _LoadThemeMetrics.wsStrings[v21 - TMT_CSSNAME] = (wchar_t const*)v11;
                break;
            case TMT_INT:
                _LoadThemeMetrics.iInts[v21 - TMT_FIRSTINT] = *(DWORD *)v11;
                break;
            case TMT_BOOL:
                _LoadThemeMetrics.fBools[v21 - TMT_FIRSTBOOL] = *(BYTE *)v11;
                break;
            case TMT_COLOR:
                _LoadThemeMetrics.crColors[v21 - TMT_FIRSTCOLOR] = *(DWORD *)v11;
                break;
            case TMT_SIZE:
                _LoadThemeMetrics.iSizes[v21 - TMT_FIRSTSIZE] = *(DWORD *)v11;
                break;
            case TMT_FONT:
                _LoadThemeMetrics.lfFonts[v21 - TMT_FIRSTFONT] = _fontTable[*(short*)v11];
                break;
            }

            ptr = (ENTRYHDR *)((char *)v11 + *((DWORD *)ptr + 1));
        }
    }

    return S_OK;
}

HRESULT CThemeLoader::CopyDummyNonSharableDataToLive()
{
    int cDIB = _rgDIBDataArray.size();

    auto hdr = (NONSHARABLEDATAHDR *)VirtualAlloc(
        _LoadingThemeFile._pbNonSharableData,
        8 * cDIB + 16,
        MEM_COMMIT,
        PAGE_READWRITE);
    if (!hdr)
        return E_OUTOFMEMORY;

    hdr->cBitmaps = cDIB;
    hdr->iBitmapsOffset = sizeof(NONSHARABLEDATAHDR);
    hdr->dwFlags = 0;
    hdr->iLoadId = 0;
    if (&hdr[1] < (NONSHARABLEDATAHDR *)((char *)(hdr + 1) + 8 * cDIB))
        memset(&hdr[1], 0, 8 * (((unsigned __int64)(8 * cDIB - 1) >> 3) + 1));
    hdr->dwFlags |= 1;

    return S_OK;
}

HRESULT CThemeLoader::CreateReuseSection(
    wchar_t const* pszSharableSectionName, void** phReuseSection)
{
    HRESULT hr = S_OK;

    int v3 = _rgDIBDataArray.size();
    DWORD maxSize = sizeof(REUSEDATAHDR) + sizeof(DIBREUSEDATA) * v3;
    if ((8 * (BYTE)v3 + 0x14) & 3)
        maxSize = (maxSize & 0xFFFFFFFC) + 4;

    for (auto& dibData : _rgDIBDataArray) {
        if (dibData.pDIBBits) {
            dibData.dibReuseData.iDIBBitsOffset = maxSize;
            maxSize += sizeof(int) * dibData.dibReuseData.iWidth * dibData.dibReuseData.iHeight;
        } else {
            dibData.dibReuseData.iDIBBitsOffset = -1;
        }
    }

    REUSEDATAHDR* reuseDataHdr = nullptr;

    *phReuseSection = CreateFileMappingW(FileHandle::InvalidHandle(), nullptr,
                                         PAGE_READWRITE, 0, maxSize, nullptr);
    if (!*phReuseSection)
        hr = MakeErrorLast();

    if (SUCCEEDED(hr)) {
        reuseDataHdr = (REUSEDATAHDR*)MapViewOfFile(*phReuseSection, FILE_MAP_WRITE, 0, 0, 0);
        if (!reuseDataHdr)
            hr = MakeErrorLast();
    }

    if (SUCCEEDED(hr)) {
        reuseDataHdr->iDIBReuseRecordsCount = _rgDIBDataArray.size();
        reuseDataHdr->iDIBReuseRecordsOffset = sizeof(REUSEDATAHDR);
        reuseDataHdr->dwTotalLength = maxSize;

        if (pszSharableSectionName)
            hr = StringCchCopyW(reuseDataHdr->szSharableSectionName, 260, pszSharableSectionName);

        if (SUCCEEDED(hr)) {
            auto reuseIt = (DIBREUSEDATA*)((char*)reuseDataHdr + reuseDataHdr->iDIBReuseRecordsOffset);
            for (auto const& dibData : _rgDIBDataArray) {
                *reuseIt = dibData.dibReuseData;
                if (dibData.pDIBBits)
                    memcpy_s(
                    (char*)reuseDataHdr + reuseIt->iDIBBitsOffset,
                        (int)(maxSize - reuseIt->iDIBBitsOffset),
                        dibData.pDIBBits,
                        4 * reuseIt->iWidth * reuseIt->iHeight);

                ++reuseIt;
            }
        }
    }

    if (reuseDataHdr)
        UnmapViewOfFile(reuseDataHdr);

    if (FAILED(hr) && *phReuseSection) {
        CloseHandle(*phReuseSection);
        *phReuseSection = nullptr;
    }

    return hr;
}

static HRESULT GenerateNonSharableData(void* hReuseSection, void* pNonSharableData)
{
    HRESULT hr;
    NONSHARABLEDATAHDR* nonSharableDataHdr;
    NONSHARABLEDATAHDR* v5;
    REUSEDATAHDR* v7;
    int v8;
    HBITMAP* bitmapIt;
    __int64 iBitmapsOffset;
    __int64 iDIBReuseRecordsCount;
    __int64 iDIBReuseRecordsOffset;
    DIBREUSEDATA* dibReuseRec;
    HBITMAP* bitmapsEnd;
    int v15;
    int v16;
    HBITMAP v17;
    HBITMAP v18;
    HBITMAP v19;
    char* v20;
    void* v21;
    __int64 v25;
    REUSEDATAHDR* reuseDataHdr;
    _BITMAPHEADER bitmapHdr;

    hr = 0;
    nonSharableDataHdr = (NONSHARABLEDATAHDR *)pNonSharableData;
    v5 = 0i64;
    reuseDataHdr = (REUSEDATAHDR *)MapViewOfFile(hReuseSection, 4u, 0, 0, 0i64);
    if (!reuseDataHdr)
        hr = MakeErrorLast();
    v7 = 0i64;

    if (hr >= 0) {
        v7 = reuseDataHdr;
        nonSharableDataHdr->dwFlags = 0;
        v5 = nonSharableDataHdr;
        nonSharableDataHdr->iLoadId = 0;
        nonSharableDataHdr->cBitmaps = reuseDataHdr->iDIBReuseRecordsCount;
        nonSharableDataHdr->iBitmapsOffset = 16;
        if (!GetWindowDC(nullptr))
            hr = 0x80004005;
    }

    bitmapIt = 0i64;
    if (hr >= 0) {
        bitmapHdr.bmih.biWidth = 0;
        bitmapHdr.bmih.biHeight = 0;
        bitmapHdr.bmih.biSizeImage = 0;
        iBitmapsOffset = v5->iBitmapsOffset;
        bitmapHdr.bmih.biCompression = 3;
        bitmapIt = (HBITMAP *)((char *)nonSharableDataHdr + iBitmapsOffset);
        bitmapHdr.bmih.biClrUsed = 3;
        iDIBReuseRecordsCount = v7->iDIBReuseRecordsCount;
        iDIBReuseRecordsOffset = v7->iDIBReuseRecordsOffset;
        bitmapHdr.bmih.biSize = 40;
        dibReuseRec = (DIBREUSEDATA *)((char *)reuseDataHdr + iDIBReuseRecordsOffset);
        bitmapHdr.bmih.biPlanes = 1;
        bitmapHdr.bmih.biBitCount = 32;
        bitmapsEnd = &bitmapIt[iDIBReuseRecordsCount];
        bitmapHdr.masks[0] = 16711680;
        bitmapHdr.masks[1] = 0xFF00;
        bitmapHdr.masks[2] = 0xFF;
        while (bitmapIt < bitmapsEnd) {
            if (dibReuseRec->iDIBBitsOffset == -1) {
                *bitmapIt = 0i64;
            } else {
                v15 = dibReuseRec->iHeight;
                bitmapHdr.bmih.biWidth = dibReuseRec->iWidth;
                bitmapHdr.bmih.biHeight = v15;
                bitmapHdr.bmih.biSizeImage = 4 * v15 * bitmapHdr.bmih.biWidth;
                v16 = dibReuseRec->iDIBBitsOffset;

                v25 = 0i64;
                v17 = CreateDIBSection(0i64, (const BITMAPINFO *)&bitmapHdr, 0, (void **)&v25, hReuseSection, v16);

                *bitmapIt = v17;
                v18 = v17;
                if (!v17) {
                    hr = E_OUTOFMEMORY;
                    break;
                }
            }
            ++bitmapIt;
            ++dibReuseRec;
        }

        if (hr >= 0)
            v5->dwFlags |= 7u;
    }

    if (reuseDataHdr)
        UnmapViewOfFile(reuseDataHdr);

    return hr;
}

HRESULT CThemeLoader::CopyNonSharableDataToLive(void* hReuseSection)
{
    void* ptr = VirtualAlloc(
        _LoadingThemeFile._pbNonSharableData,
        8 * _rgDIBDataArray.size() + 16,
        MEM_COMMIT, PAGE_READWRITE);

    if (!ptr)
        return 0x80070008;

    return GenerateNonSharableData(hReuseSection, ptr);
}

struct ROOTSECTION
{
    wchar_t szSharableSectionName[260];
    wchar_t szNonSharableSectionName[260];
    unsigned dwClientChangeNumber;
};

struct SectionHandleTraits : NullIsInvalidHandleTraits {};
using SectionHandle = Handle<SectionHandleTraits>;

struct FileViewHandleTraits
{
    using HandleType = void*;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::UnmapViewOfFile(h); }
};
using FileViewHandle = Handle<FileViewHandleTraits>;

class Section
{
public:
    Section(DWORD desiredSectionAccess, DWORD desiredViewAccess)
        : desiredSectionAccess(desiredSectionAccess)
        , desiredViewAccess(desiredViewAccess)
    {
    }

    HRESULT OpenSection(wchar_t const* sectionName, bool mapView)
    {
        UNICODE_STRING name;
        RtlInitUnicodeString(&name, sectionName);
        OBJECT_ATTRIBUTES objA = {};
        InitializeObjectAttributes(&objA, &name, OBJ_CASE_INSENSITIVE, nullptr, nullptr);

        ModuleHandle ntdllHandle{LoadLibraryW(L"ntdll.dll")};
        SectionHandle sectionHandle;
        decltype(NtOpenSection)* NtOpenSectionPtr = (decltype(NtOpenSection)*)GetProcAddress(ntdllHandle, "NtOpenSection");

        NTSTATUS st = NtOpenSectionPtr(sectionHandle.CloseAndGetAddressOf(),
                                       desiredSectionAccess, &objA);
        if (st != STATUS_SUCCESS)
            return st;

        //SectionHandle sectionHandle{OpenFileMappingW(desiredSectionAccess, FALSE, sectionName)};
        //if (!sectionHandle)
        //    return MakeErrorLast();

        if (mapView) {
            FileViewHandle sectionData{MapViewOfFile(sectionHandle, desiredViewAccess, 0, 0, 0)};
            if (!sectionData)
                return MakeErrorLast();

            this->sectionData = std::move(sectionData);
        }

        this->sectionHandle = std::move(sectionHandle);
        return S_OK;
    }

    void* View() const { return sectionData.Get(); }

protected:
    SectionHandle sectionHandle;
    FileViewHandle sectionData;
    DWORD desiredSectionAccess;
    DWORD desiredViewAccess;
};

class RootSection : public Section
{
public:
    RootSection(DWORD desiredSectionAccess, DWORD desiredViewAccess)
        : Section(desiredSectionAccess, desiredViewAccess)
    {
        DWORD sessionId = NtCurrentTeb()->ProcessEnvironmentBlock->SessionId;
        if (sessionId)
            StringCchPrintfW(sectionName, 260, L"\\Sessions\\%d\\Windows\\ThemeSection", sessionId);
        else
            StringCchPrintfW(sectionName, 260, L"\\Windows\\ThemeSection");
    }

    HRESULT GetRootSectionData(ROOTSECTION** ppRootSection)
    {
        *ppRootSection = nullptr;
        ENSURE_HR(OpenSection(sectionName, true));
        if (!sectionData)
            return E_OUTOFMEMORY;

        *ppRootSection = static_cast<ROOTSECTION*>(sectionData.Get());
        return S_OK;
    }

private:
    wchar_t sectionName[260];
};

static void GetStrings(THEMEHDR const* hdr, std::vector<std::wstring>& strings)
{
    auto begin = (wchar_t const*)Advance(hdr, hdr->iStringsOffset);
    auto end = Advance(begin, hdr->iStringsLength);
    for (auto ptr = begin; ptr < end;) {
        strings.emplace_back(ptr);
        ptr += wcslen(ptr);
        while (ptr < end && *ptr == 0)
            ++ptr;
    }
}

template<typename T, typename = std::enable_if_t<std::is_enum_v<T>, T>>
static int FormatImpl(char* buffer, size_t bufferSize, T const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<typename T>
static int Format(char* buffer, size_t bufferSize, T const& value)
{

    return FormatImpl(buffer, bufferSize, value);
}

template<>
static int Format(char* buffer, size_t bufferSize, void* const& value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, void const* const& value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, char const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", (int)value);
}

template<>
static int Format(char* buffer, size_t bufferSize, unsigned char const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", (unsigned)value);
}

template<>
static int Format(char* buffer, size_t bufferSize, short const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, unsigned short const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, int const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, unsigned const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, long const& value)
{
    return sprintf_s(buffer, bufferSize, "%ld", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, unsigned long const& value)
{
    return sprintf_s(buffer, bufferSize, "%lu", value);
}

template<>
static int Format(char* buffer, size_t bufferSize, POINT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.x, value.y);
}

template<>
static int Format(char* buffer, size_t bufferSize, SIZE const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.cx, value.cy);
}

template<>
static int Format(char* buffer, size_t bufferSize, RECT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld,%ld,%ld)",
                     value.left, value.top, value.right, value.bottom);
}

template<>
static int Format(char* buffer, size_t bufferSize, MARGINS const& value)
{
    return sprintf_s(buffer, bufferSize, "(l:%d,r:%d,t:%d,b:%d)",
                     value.cxLeftWidth, value.cxRightWidth, value.cyTopHeight, value.cyBottomHeight);
}

class LogFile
{
public:
    LogFile(wchar_t const* path) : path(path) {}

    bool Open()
    {
        FileHandle h{CreateFileW(path.c_str(), GENERIC_WRITE, 0, nullptr,
                                 CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr)};
        if (!h)
            return false;
        hFile = std::move(h);
        return true;
    }

    void Indent(int n = 1) { indentLevel += n; }
    void Outdent(int n = 1) { indentLevel -= n; }

    template<typename... T>
    void Log(char const* format, T const&... args)
    {
        int len = sprintf_s(buffer, countof(buffer), format, args...);
        if (len <= 0)
            return;

        WriteIndent();
        DWORD written;
        WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);
    }

    template<typename T>
    void LogPair(char const* key, T const& value)
    {
        WriteIndent();

        DWORD written;

        int len = sprintf_s(buffer, countof(buffer), "%s: ", key);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        len = Format(buffer, countof(buffer), value);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        buffer[0] = '\n';
        WriteFile(hFile, buffer, 1, &written, nullptr);
    }

    void LogPair(char const* key, DIBINFO const& value)
    {
        Log("%s:\n", key);
        Indent();

        LogPair("uhbm.hBitmap64", value.uhbm.hBitmap64);
        LogPair("iDibOffset", value.iDibOffset);
        LogPair("iSingleWidth", value.iSingleWidth);
        LogPair("iSingleHeight", value.iSingleHeight);
        LogPair("iRgnListOffset", value.iRgnListOffset);
        LogPair("eSizingType", value.eSizingType);
        LogPair("fBorderOnly", value.fBorderOnly);
        LogPair("fPartiallyTransparent", value.fPartiallyTransparent);
        LogPair("iAlphaThreshold", value.iAlphaThreshold);
        LogPair("iMinDpi", value.iMinDpi);
        LogPair("szMinSize", value.szMinSize);

        Outdent();
    }

    template<typename T, size_t N>
    void LogPair(char const* key, T const (&value)[N])
    {
        DWORD written;

        for (size_t i = 0; i < N; ++i) {
            WriteIndent();

            int len = sprintf_s(buffer, countof(buffer), "%s[%zu]: ", key, i);
            if (len > 0)
                WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

            len = Format(buffer, countof(buffer), value[i]);
            if (len > 0)
                WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

            buffer[0] = '\n';
            WriteFile(hFile, buffer, 1, &written, nullptr);
        }
    }

private:
    void WriteIndent()
    {
        if (indentLevel == 0)
            return;

        size_t length = std::min(indentLevel * (size_t)2, indentBuffer.size() - 1);
        memset(indentBuffer.data(), ' ', length);

        DWORD written;
        WriteFile(hFile, indentBuffer.data(), static_cast<DWORD>(length), &written, nullptr);
    }

    std::wstring path;
    FileHandle hFile;
    char buffer[0x800];
    std::array<char, 64> indentBuffer;
    int indentLevel = 0;
};

static void Dump(LogFile& log, THEMEHDR const* hdr, CDrawBase* drawObj)
{
    if (drawObj->_eBgType == BT_IMAGEFILE) {
        auto imageFile = (CImageFile*)drawObj;
        log.LogPair("_eBgType", imageFile->_eBgType);
        log.LogPair("_iUnused", imageFile->_iUnused);
        log.LogPair("_ImageInfo", imageFile->_ImageInfo);
        log.LogPair("_ScaledImageInfo", imageFile->_ScaledImageInfo);
        log.LogPair("_iMultiImageCount", imageFile->_iMultiImageCount);
        log.LogPair("_eImageSelectType", imageFile->_eImageSelectType);
        log.LogPair("_iImageCount", imageFile->_iImageCount);
        log.LogPair("_eImageLayout", imageFile->_eImageLayout);
        log.LogPair("_fMirrorImage", imageFile->_fMirrorImage);
        log.LogPair("_eTrueSizeScalingType", imageFile->_eTrueSizeScalingType);
        log.LogPair("_eHAlign", imageFile->_eHAlign);
        log.LogPair("_eVAlign", imageFile->_eVAlign);
        log.LogPair("_fBgFill", imageFile->_fBgFill);
        log.LogPair("_crFill", imageFile->_crFill);
        log.LogPair("_iTrueSizeStretchMark", imageFile->_iTrueSizeStretchMark);
        log.LogPair("_fUniformSizing", imageFile->_fUniformSizing);
        log.LogPair("_fIntegralSizing", imageFile->_fIntegralSizing);
        log.LogPair("_SizingMargins", imageFile->_SizingMargins);
        log.LogPair("_ContentMargins", imageFile->_ContentMargins);
        log.LogPair("_fSourceGrow", imageFile->_fSourceGrow);
        log.LogPair("_fSourceShrink", imageFile->_fSourceShrink);
        log.LogPair("_fGlyphOnly", imageFile->_fGlyphOnly);
        log.LogPair("_eGlyphType", imageFile->_eGlyphType);
        log.LogPair("_crGlyphTextColor", imageFile->_crGlyphTextColor);
        log.LogPair("_iGlyphFontIndex", imageFile->_iGlyphFontIndex);
        log.LogPair("_iGlyphIndex", imageFile->_iGlyphIndex);
        log.LogPair("_GlyphInfo", imageFile->_GlyphInfo);
        log.LogPair("_iSourcePartId", imageFile->_iSourcePartId);
        log.LogPair("_iSourceStateId", imageFile->_iSourceStateId);
    } else if (drawObj->_eBgType == BT_BORDERFILL) {
        auto borderFill = (CBorderFill*)drawObj;
        log.LogPair("_eBgType", borderFill->_eBgType);
        log.LogPair("_iUnused", borderFill->_iUnused);
        log.LogPair("_fNoDraw", borderFill->_fNoDraw);
        log.LogPair("_eBorderType", borderFill->_eBorderType);
        log.LogPair("_crBorder", borderFill->_crBorder);
        log.LogPair("_iBorderSize", borderFill->_iBorderSize);
        log.LogPair("_iRoundCornerWidth", borderFill->_iRoundCornerWidth);
        log.LogPair("_iRoundCornerHeight", borderFill->_iRoundCornerHeight);
        log.LogPair("_eFillType", borderFill->_eFillType);
        log.LogPair("_crFill", borderFill->_crFill);
        log.LogPair("_iDibOffset", borderFill->_iDibOffset);
        log.LogPair("_ContentMargins", borderFill->_ContentMargins);
        log.LogPair("_iGradientPartCount", borderFill->_iGradientPartCount);
        log.LogPair("_crGradientColors", borderFill->_crGradientColors);
        log.LogPair("_iGradientRatios", borderFill->_iGradientRatios);
        log.LogPair("_iSourcePartId", borderFill->_iSourcePartId);
        log.LogPair("_iSourceStateId", borderFill->_iSourceStateId);
    } else {
        log.LogPair("_eBgType", drawObj->_eBgType);
        log.LogPair("_iUnused", drawObj->_iUnused);
    }
}

static void DumpEntry(LogFile& log, THEMEHDR const* hdr, ENTRYHDR* entry)
{
    log.Log("  (%05d %05u) %05u\n", entry->usTypeNum, entry->ePrimVal, entry->dwDataLen);

    log.Indent();

    if (entry->usTypeNum == TMT_PARTJUMPTBL) {
        auto jumpTableHdr = (PARTJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("  iBaseClassIndex: %d\n", jumpTableHdr->iBaseClassIndex);
        log.Log("  iFirstDrawObjIndex: %d\n", jumpTableHdr->iFirstDrawObjIndex);
        log.Log("  iFirstTextObjIndex: %d\n", jumpTableHdr->iFirstTextObjIndex);
        log.Log("  cParts: %d\n", jumpTableHdr->cParts);
        for (int i = 0; i < jumpTableHdr->cParts; ++i)
            log.Log("  [%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_STATEJUMPTBL) {
        auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("  cStates: %d\n", jumpTableHdr->cStates);
        for (int i = 0; i < jumpTableHdr->cStates; ++i)
            log.Log("  [%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_IMAGEINFO) {
        auto data = (char*)(entry + 1);
        auto imageCount = *(char*)data;
        log.Log("  ImageCount: %d\n", imageCount);

        auto regionOffsets = (int*)(data + 8);

        for (unsigned i = 0; i < imageCount; ++i)
            log.Log("  [Region %d]: %d\n", i, regionOffsets[i]);

        for (unsigned i = 0; i < imageCount; ++i)
            DumpEntry(log, hdr, (ENTRYHDR*)((char*)hdr + regionOffsets[i]));
    } else if (entry->usTypeNum == TMT_REGIONDATA) {
        auto data = (RGNDATA*)(entry + 1);
        log.Log("  rdh.nCount:   %lu\n", data->rdh.nCount);
        log.Log("  rdh.nRgnSize: %lu\n", data->rdh.nRgnSize);
        log.Log("  rdh.rcBound: (%d,%d,%d,%d)\n", data->rdh.rcBound.left,
                data->rdh.rcBound.top, data->rdh.rcBound.right,
                data->rdh.rcBound.bottom);
        auto rects = (RECT*)data->Buffer;
        for (DWORD i = 0; i < data->rdh.nCount; ++i) {
            log.Log("  [%lu]: (%d,%d,%d,%d)\n", i, rects[i].left,
                    rects[i].top, rects[i].right, rects[i].bottom);
        }
    } else if (entry->usTypeNum == TMT_DRAWOBJ) {
        auto objHdr = (PARTOBJHDR*)(entry + 1);
        log.Log("[p:%d, s:%d]\n", objHdr->iPartId, objHdr->iStateId);
        Dump(log, hdr, (CDrawBase*)(objHdr + 1));
    }

    log.Outdent();
}

static void DumpSectionIndex(THEMEHDR const* hdr, LogFile& log)
{
    auto begin = (APPCLASSLIVE*)Advance(hdr, hdr->iSectionIndexOffset);
    auto end = Advance(begin, hdr->iSectionIndexLength);

    for (auto p = begin; p < end; ++p) {
        auto appName = p->AppClassInfo.iAppNameIndex ?
            (wchar_t const*)Advance(hdr, p->AppClassInfo.iAppNameIndex) : L"<no app>";
        auto className = p->AppClassInfo.iClassNameIndex ?
            (wchar_t const*)Advance(hdr, p->AppClassInfo.iClassNameIndex) : L"<no class>";

        log.Log("%-10ls %-30ls  %05d %05d %05d\n",
                appName, className, p->iIndex, p->iLen, p->iBaseClassIndex);

        auto be = (ENTRYHDR*)Advance(hdr, p->iIndex);
        auto ee = Advance(be, p->iLen);

        for (auto pe = be; pe < ee; pe = pe->Next()) {
            DumpEntry(log, hdr, pe);
        }
    }
}

static void Dump(THEMEHDR const* rev, THEMEHDR const* ref)
{
    return;

    std::vector<std::wstring> str1;
    std::vector<std::wstring> str2;
    GetStrings(rev, str1);
    GetStrings(ref, str2);

    auto revMetrics = (THEMEMETRICS const*)Advance(rev, rev->iSysMetricsOffset);
    auto refMetrics = (THEMEMETRICS const*)Advance(ref, ref->iSysMetricsOffset);

    LogFile f1{L"D:\\l1.txt"};
    LogFile f2{L"D:\\l2.txt"};
    f1.Open();
    f2.Open();
    DumpSectionIndex(rev, f1);
    DumpSectionIndex(ref, f2);
    int x = 1;
}

HRESULT CThemeLoader::PackAndLoadTheme(
    void* hFile, wchar_t const* pszThemeName, wchar_t const* pszColorParam,
    wchar_t const* pszSizeParam, unsigned cbMaxDesiredSharableSectionSize,
    wchar_t* pszSharableSectionName, unsigned cchSharableSectionName,
    wchar_t* pszNonSharableSectionName, unsigned cchNonSharableSectionName,
    void** phReuseSection,
    PFNALLOCSECTIONS pfnAllocSections)
{
    ENSURE_HR(PackMetrics());

    if (pfnAllocSections) {
        ENSURE_HR(pfnAllocSections(
            &_LoadingThemeFile,
            pszSharableSectionName,
            cchSharableSectionName,
            cbMaxDesiredSharableSectionSize,
            pszNonSharableSectionName,
            cchNonSharableSectionName,
            8 * _rgDIBDataArray.size() + 16,
            1));
    } else {
        ENSURE_HR(_LoadingThemeFile.CreateFileW(
            pszSharableSectionName,
            cchSharableSectionName,
            cbMaxDesiredSharableSectionSize,
            pszNonSharableSectionName,
            cchNonSharableSectionName,
            8 * _rgDIBDataArray.size() + 16,
            0));
    }

    if (phReuseSection) {
        ENSURE_HR(CreateReuseSection(pszSharableSectionName, phReuseSection));
        ENSURE_HR(CopyNonSharableDataToLive(*phReuseSection));
    } else {
        ENSURE_HR(CopyDummyNonSharableDataToLive());
    }

    ENSURE_HR(CopyLocalThemeToLive(
        hFile,
        cbMaxDesiredSharableSectionSize,
        pszThemeName,
        pszColorParam,
        pszSizeParam));

    {
        FileHandle h{CreateFileW(L"d:\\theme-packed.rev.dat", GENERIC_WRITE, 0, nullptr,
                     CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr)};
        DWORD bytesWritten;
        WriteFile(h, _hdr, _hdr->dwTotalLength, &bytesWritten, nullptr);
        h.Close();
    }

    {
        RootSection rootSection(FILE_MAP_READ, FILE_MAP_READ);
        ROOTSECTION* rootSectionData;
        HRESULT hr = rootSection.GetRootSectionData(&rootSectionData);
        if (SUCCEEDED(hr)) {
            Section section(FILE_MAP_READ, FILE_MAP_READ);
            hr = section.OpenSection(rootSectionData->szSharableSectionName, true);
            if (SUCCEEDED(hr)) {
                auto hdr = (THEMEHDR const*)section.View();

                FileHandle h{CreateFileW(L"d:\\theme-packed.orig.dat", GENERIC_WRITE, 0, nullptr,
                                         CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr)};
                DWORD bytesWritten;
                WriteFile(h, hdr, hdr->dwTotalLength, &bytesWritten, nullptr);
                h.Close();

                Dump(_hdr, hdr);
            }
        }
    }

    return S_OK;
}

HRESULT CThemeLoader::CopyLocalThemeToLive(
    void* hFile, int iTotalLength, wchar_t const* pszThemeName,
    wchar_t const* pszColorParam, wchar_t const* pszSizeParam)
{
    auto const themeHdrSize = Align8(sizeof(THEMEHDR));
    auto const themeHdr = (THEMEHDR*)VirtualAlloc(
        _LoadingThemeFile._pbSharableData, themeHdrSize, MEM_COMMIT, PAGE_READWRITE);
    if (!themeHdr)
        return 0x80070008;

    _hdr = themeHdr;
    _iGlobalsOffset = -1;
    _iSysMetricsOffset = -1;

    _hdr->dwTotalLength = iTotalLength;
    *(uint64_t*)&_hdr->szSignature = 0x4D48544E49474542;
    _hdr->dwVersion = 65543;
    _hdr->iDllNameOffset = 0;
    _hdr->iColorParamOffset = 0;
    _hdr->iSizeParamOffset = 0;
    _hdr->dwLangID = _wCurrentLangID;
    _hdr->iLoadDPI = GetScreenDpi();
    _hdr->dwLoadDPIs = g_DpiInfo._nDpiPlateausCurrentlyPresent;
    _hdr->iLoadPPI = GetScreenPpi();
    _hdr->iGlobalsOffset = 0;
    _hdr->iGlobalsTextObjOffset = 0;
    _hdr->iGlobalsDrawObjOffset = 0;
    _hdr->iFontsOffset = 0;
    _hdr->cFonts = _fontTable.size();
    _hdr->ftModifTimeStamp = FILETIME();
    GetFileTime(hFile, nullptr, nullptr, &_hdr->ftModifTimeStamp);

    MIXEDPTRS u;
    u.pb = (char*)themeHdr + themeHdrSize;

    _hdr->iStringsOffset = (uintptr_t)u.pb - (uintptr_t)(_LoadingThemeFile._pbSharableData);

    char* stringsBegin = u.pb;
    ENSURE_HR(EmitString(&u, pszThemeName, 260, &_hdr->iDllNameOffset));

    if (int len = lstrlenW(pszColorParam))
        ENSURE_HR(EmitString(&u, pszColorParam, len, &_hdr->iColorParamOffset));
    if (int len = lstrlenW(pszSizeParam))
        ENSURE_HR(EmitString(&u, pszSizeParam, len, &_hdr->iSizeParamOffset));

    for (APPCLASSLOCAL& localCls : _LocalIndexes) {
        if (size_t len = localCls.csAppName.length()) {
            ENSURE_HR(EmitString(&u, localCls.csAppName.c_str(), len,
                                 &localCls.AppClassInfo.iAppNameIndex));
        } else {
            localCls.AppClassInfo.iAppNameIndex = 0;
        }

        if (size_t len = localCls.csClassName.length()) {
            ENSURE_HR(EmitString(&u, localCls.csClassName.c_str(), len,
                                 &localCls.AppClassInfo.iClassNameIndex));
        } else {
            localCls.AppClassInfo.iClassNameIndex = 0;
        }
    }

    {
        int* pStringOffsets = (int*)_LoadThemeMetrics.iStringOffsets;
        for (std::wstring const& str : _LoadThemeMetrics.wsStrings) {
            if (size_t len = str.length())
                ENSURE_HR(EmitString(&u, str.c_str(), len, pStringOffsets));
            else
                *pStringOffsets = 0;
            ++pStringOffsets;
        }
    }

    _hdr->iStringsLength = (uintptr_t)u.pb - (uintptr_t)stringsBegin;

    _hdr->iSectionIndexOffset = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData;
    _hdr->iSectionIndexLength = sizeof(APPCLASSLIVE) * _LocalIndexes.size();
    ENSURE_HR(AllocateThemeFileBytes(u.pb, _hdr->iSectionIndexLength));

    auto liveClasses = (APPCLASSLIVE*)u.pb;
    u.pb += _hdr->iSectionIndexLength;
    for (size_t i = 0; i < _LocalIndexes.size(); ++i) {
        APPCLASSLOCAL& pac = _LocalIndexes[i];
        APPCLASSLIVE& liveCls = liveClasses[i];
        liveCls.AppClassInfo = pac.AppClassInfo;
        liveCls.iIndex = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData;

        if (!AsciiStrCmpI(pac.csClassName.c_str(), L"globals"))
            _iGlobalsOffset = liveCls.iIndex;

        if (_rgBaseClassIds[i] == -1)
            liveCls.iBaseClassIndex = _iGlobalsOffset;
        else
            liveCls.iBaseClassIndex = liveClasses[_rgBaseClassIds[i]].iIndex;

        if (!AsciiStrCmpI(pac.csClassName.c_str(), L"sysmetrics")) {
            _iSysMetricsOffset = liveCls.iIndex;
            ENSURE_HR(EmitEntryHdr(&u, TMT_THEMEMETRICS, TMT_THEMEMETRICS));
            ENSURE_HR(EmitAndCopyBlock(&u, &_LoadThemeMetrics, sizeof(THEMEMETRICS)));
            EndEntry(&u);

            ENSURE_HR(EmitEntryHdr(&u, TMT_12, TMT_12));
            ENSURE_HR(AllocateThemeFileBytes(u.pb, 8));
            *(int*)u.pb = -1;
            u.pb += 8;
            EndEntry(&u);
        } else {
            ENSURE_HR(CopyClassGroup(&pac, &u, &liveCls));
        }

        int currIndex = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData;
        liveCls.iLen = currIndex - liveCls.iIndex;
    }

    {
        auto pb = u.pb;
        ENSURE_HR(EmitAndCopyBlock(&u, _fontTable.data(), sizeof(_fontTable[0]) * _fontTable.size()));
        _hdr->iFontsOffset = (uintptr_t)pb - (uintptr_t)_LoadingThemeFile._pbSharableData;
    }

    ENSURE_HR(EmitAndCopyBlock(&u, "ENDTHEME", 8));

    _hdr->dwTotalLength = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData;
    _hdr->iGlobalsOffset = _iGlobalsOffset;
    _hdr->iSysMetricsOffset = _iSysMetricsOffset;
    _hdr->iGlobalsTextObjOffset = _iGlobalsTextObj;
    _hdr->iGlobalsDrawObjOffset = _iGlobalsDrawObj;

    return S_OK;
}

HRESULT CThemeLoader::CopyClassGroup(APPCLASSLOCAL* pac, MIXEDPTRS* u,
                                     APPCLASSLIVE* pacl)
{
    int fGlobalsGroup;
    HRESULT hr;
    int* partJumpTable;
    int iPartZeroIndex;
    int screenDpi;
    int v21;
    char* v26;
    CRenderObj* pRender;

    pRender = 0i64;
    v26 = u->pb;
    fGlobalsGroup = _iGlobalsOffset == (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

    int cParts = pac->iMaxPartNum + 1;

    ENSURE_HR(EmitEntryHdr(u, TMT_PARTJUMPTBL, TMT_PARTJUMPTBL));
    ENSURE_HR(AllocateThemeFileBytes(
        u->pb, sizeof(PARTJUMPTABLEHDR) + sizeof(int) * cParts));

    auto partJumpTableHdr = (PARTJUMPTABLEHDR*)u->pb;
    partJumpTableHdr->iBaseClassIndex = pacl->iBaseClassIndex;
    partJumpTableHdr->iFirstTextObjIndex = 0;
    partJumpTableHdr->cParts = cParts;
    u->pb = (char*)(partJumpTableHdr + 1);
    RegisterPtr(partJumpTableHdr);

    partJumpTable = (int*)u->pb;
    for (int i = cParts; i; --i) {
        *(int*)u->pb = -1;
        u->pb += 4;
    }
    EndEntry(u);

    iPartZeroIndex = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
    for (int partId = 0; partId <= pac->iMaxPartNum; ++partId)
        CopyPartGroup(pac, u, partId, partJumpTable, iPartZeroIndex, pacl->iBaseClassIndex, fGlobalsGroup);

    screenDpi = GetScreenDpi();
    hr = CRenderObj::Create(
        &_LoadingThemeFile,
        0,
        (signed int)v26 - (uintptr_t)(_LoadingThemeFile._pbSharableData),
        0,
        pacl->AppClassInfo.iClassNameIndex,
        0,
        false,
        nullptr,
        nullptr,
        screenDpi,
        false,
        0,
        &pRender);

    if (hr >= 0) {
        if (screenDpi != 96)
            pRender->_dwOtdFlags |= OTD_FORCE_RECT_SIZING;

        if (fGlobalsGroup)
            _iGlobalsDrawObj = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

        partJumpTableHdr->iFirstDrawObjIndex = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
        hr = PackDrawObjects(u, pRender, pac->iMaxPartNum, fGlobalsGroup);
        if (hr >= 0) {
            if (fGlobalsGroup)
                _iGlobalsTextObj = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
            partJumpTableHdr->iFirstTextObjIndex = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
            hr = PackTextObjects(u, pRender, pac->iMaxPartNum, fGlobalsGroup);
            if (hr >= 0)
            {
                hr = EmitEntryHdr(u, TMT_CLSGROUPEND, TMT_CLSGROUPEND);
                if (hr >= 0)
                    EndEntry(u);
            }
        }
    }

    delete pRender;
    return hr;
}

int CThemeLoader::GetPartOffset(CRenderObj* pRender, int iPartId)
{
    auto entryHdr = (ENTRYHDR*)pRender->_pbSectionData;
    if (entryHdr->usTypeNum != TMT_PARTJUMPTBL)
        return -1;

    auto jumpTableHdr = (PARTJUMPTABLEHDR*)(entryHdr + 1);
    if (iPartId >= jumpTableHdr->cParts)
        return -1;

    auto jumpTable = (int*)(jumpTableHdr + 1);
    return jumpTable[iPartId];
}

BOOL CThemeLoader::KeyDrawPropertyFound(int iStateDataOffset)
{
    auto ptr = (char*)_LoadingThemeFile._pbSharableData + iStateDataOffset;
    for (;;) {
        auto hdr = (ENTRYHDR*)ptr;
        if (hdr->usTypeNum == TMT_12)
            break;

        if (CBorderFill::KeyProperty(hdr->usTypeNum))
            return TRUE;
        if (CImageFile::KeyProperty(hdr->usTypeNum))
            return TRUE;

        ptr += sizeof(ENTRYHDR) + hdr->dwDataLen;
    }

    return FALSE;
}

HRESULT CThemeLoader::PackDrawObjects(
    MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, int fGlobals)
{
    for (int partId = 0; partId <= iMaxPart; ++partId) {
        int offset = GetPartOffset(pRender, partId);
        if (offset == -1)
            continue;

        auto entryHdr = (ENTRYHDR*)((char*)_LoadingThemeFile._pbSharableData + offset);
        if (entryHdr->usTypeNum == TMT_STATEJUMPTBL) {
            auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entryHdr + 1);
            auto jumpTable = (int*)(jumpTableHdr + 1);

            for (int stateId = 0; stateId <= jumpTableHdr->cStates - 1; ++stateId) {
                int so = jumpTable[stateId];
                if (so != -1 && (fGlobals || KeyDrawPropertyFound(so))) {
                    ENSURE_HR(PackDrawObject(uOut, pRender, partId, stateId));
                    fGlobals = 0;
                }
            }
        } else {
            if (fGlobals || KeyDrawPropertyFound(offset))
                ENSURE_HR(PackDrawObject(uOut, pRender, partId, 0));
        }
    }

    return S_OK;
}

HRESULT CThemeLoader::CopyPartGroup(
    APPCLASSLOCAL* pac, MIXEDPTRS* u, int iPartId, int* piPartJumpTable,
    int iPartZeroIndex, int iBaseClassIndex, int fGlobalsGroup)
{
    if (pac->PartStateIndexes.empty())
        return S_OK;

    int iMaxStateId = -1;
    for (PART_STATE_INDEX const& psi : pac->PartStateIndexes) {
        if (psi.iPartId == iPartId && psi.iStateId > iMaxStateId)
            iMaxStateId = psi.iStateId;
    }

    if (iMaxStateId < 0)
        return S_OK;

    HRESULT hr = 0;
    if (piPartJumpTable) {
        piPartJumpTable[iPartId] = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
        if (piPartJumpTable[iPartId] == 0) { // FIXME: Remove
            piPartJumpTable[iPartId] = 0;
        }
    }

    int* stateJumpTable = nullptr;
    if (iMaxStateId > 0) {
        int cStates = iMaxStateId + 1;
        ENSURE_HR(EmitEntryHdr(u, TMT_STATEJUMPTBL, TMT_STATEJUMPTBL));
        ENSURE_HR(AllocateThemeFileBytes(
            u->pb, sizeof(STATEJUMPTABLEHDR) + sizeof(int) * cStates));

        auto stateJumpTableHdr = (STATEJUMPTABLEHDR*)u->pb;
        stateJumpTableHdr->cStates = cStates;
        RegisterPtr(stateJumpTableHdr);

        u->pb = (char*)(stateJumpTableHdr + 1);
        stateJumpTable = (int*)u->pb;

        for (int i = 0; i < cStates; ++i)
            stateJumpTable[i] = -1;

        u->pb += sizeof(int) * cStates;

        EndEntry(u);
    }

    int v24 = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
    for (int iState = 0; iState <= iMaxStateId; ++iState) {
        for (auto const& psi : pac->PartStateIndexes) {
            if (psi.iPartId != iPartId || psi.iStateId != iState)
                continue;

            if (stateJumpTable)
                stateJumpTable[iState] = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

            int vv;
            if (iState)
                vv = v24;
            else if (fGlobalsGroup)
                vv = -1;
            else
                vv = iPartId ? iPartZeroIndex : iBaseClassIndex;

            char* block = &_pbLocalData[psi.iIndex];
            *(int*)(_pbLocalData + psi.iIndex + psi.iLen - 8) = vv;

            auto begin = (ENTRYHDR const*)u->pb;
            auto end = Advance(begin, psi.iLen);

            hr = EmitAndCopyBlock(u, block, psi.iLen);

            for (auto p = begin; p < end;) {
                RegisterPtr(p);
                p = Advance(p, sizeof(ENTRYHDR) + p->dwDataLen);
            }
        }
    }

    return hr;
}

HRESULT CThemeLoader::MakeStockObject(CRenderObj* pRender, DIBINFO* pdi)
{
    if (!_fGlobalTheme)
        return S_FALSE;

    TMBITMAPHEADER* hdr = pRender->GetBitmapHeader(pdi->iDibOffset);
    if (!hdr)
        return S_FALSE;

    HBITMAP hbmp = pRender->_phBitmapsArray[hdr->iBitmapIndex].hBitmap;
    if (!hbmp)
        return S_FALSE;

    HRESULT v4;
    HBITMAP v10;
    v4 = 1;
    //LODWORD(v10) = SetBitmapAttributes(hbmp, (unsigned)v4);
    //if (v10) {
    pRender->_phBitmapsArray[hdr->iBitmapIndex].hBitmap = hbmp;
    //    return S_OK;
    //} else {
    //    DeleteObject(hbmp);
    //    return E_FAIL;
    //}
    return S_OK;
}

struct CBitmapPixels
{
    BITMAPINFOHEADER* _hdrBitmap;
    int _iWidth;
    int _iHeight;
    char* _buffer;
    ~CBitmapPixels();
    DWORD OpenBitmap(HDC hdc, HBITMAP bitmap, int fForceRGB32, unsigned** pPixels, int* piWidth, int* piHeight, int* piBytesPerPixel, int* piBytesPerRow, int* piPreviousBytesPerPixel, unsigned cbBytesBefore);
    void CloseBitmap(HDC hdc, HBITMAP hBitmap);
};

CBitmapPixels::~CBitmapPixels()
{
    if (_buffer)
        free(_buffer);
}

DWORD CBitmapPixels::OpenBitmap(
    HDC hdc, HBITMAP bitmap, int fForceRGB32, unsigned** pPixels, int* piWidth, int* piHeight, int* piBytesPerPixel, int* piBytesPerRow, int* piPreviousBytesPerPixel, unsigned cbBytesBefore)
{
    DWORD hr;
    HDC v14;
    __int64 v15;
    unsigned __int64 v16;
    int v17;
    unsigned __int64 v18;
    BITMAPINFOHEADER* v19;
    BITMAP pv;

    if (pPixels)
    {
        v14 = GetWindowDC(0i64);
        if (v14)
        {
            GetObjectW(bitmap, sizeof(BITMAP), &pv);
            v15 = (unsigned)pv.bmHeight;
            v16 = 4i64 * (unsigned)pv.bmWidth;
            _iWidth = pv.bmWidth;
            _iHeight = v15;
            if (v16 <= 0xFFFFFFFF
                && (unsigned)v16 <= 0x7FFFFFFC
                && (v17 = (v16 + 3) & 0xFFFFFFFC, v18 = v15 * (unsigned)v17, v18 <= 0xFFFFFFFF)
                && (signed int)v18 + 0x8C >= (unsigned)v18
                && (v19 = (BITMAPINFOHEADER *)malloc((unsigned)(v18 + 0x8C)),
                (_buffer = (char *)v19) != 0i64))
            {
                _hdrBitmap = v19;
                memset(v19, 0, 0x28ui64);
                _hdrBitmap->biSize = 40;
                _hdrBitmap->biWidth = _iWidth;
                _hdrBitmap->biHeight = _iHeight;
                _hdrBitmap->biPlanes = 1;
                _hdrBitmap->biBitCount = 32;
                _hdrBitmap->biCompression = 0;
                GetDIBits(
                    v14,
                    bitmap,
                    0,
                    _iHeight,
                    (char *)_hdrBitmap + 4 * _hdrBitmap->biClrUsed + _hdrBitmap->biSize,
                    (LPBITMAPINFO)_hdrBitmap,
                    0);
                ReleaseDC(0i64, v14);
                *pPixels = (unsigned *)((char *)&_hdrBitmap->biSize
                                            + 4 * _hdrBitmap->biClrUsed
                                            + _hdrBitmap->biSize);
                if (piWidth)
                    *piWidth = _iWidth;
                if (piHeight)
                    *piHeight = _iHeight;
                if (piBytesPerPixel)
                    *piBytesPerPixel = 4;
                if (piBytesPerRow)
                    *piBytesPerRow = v17;
                hr = 0;
            } else
            {
                hr = E_OUTOFMEMORY;
            }
        } else
        {
            hr = MakeErrorLast();
        }
    } else
    {
        hr = E_INVALIDARG;
    }
    return hr;
}

void CBitmapPixels::CloseBitmap(HDC hdc, HBITMAP hBitmap)
{
    HDC v5;

    if (_hdrBitmap && _buffer)
    {
        if (hBitmap)
        {
            v5 = GetWindowDC(0i64);
            SetDIBits(
                v5,
                hBitmap,
                0,
                _iHeight,
                (char *)_hdrBitmap + 4 * _hdrBitmap->biClrUsed + _hdrBitmap->biSize,
                (const BITMAPINFO *)_hdrBitmap,
                0);
            if (v5)
                ReleaseDC(0i64, v5);
        }
        free(_buffer);
        _hdrBitmap = 0i64;
        _buffer = 0i64;
    }
}

HRESULT CThemeLoader::PackImageFileInfo(
    DIBINFO* pdi, CImageFile* pImageObj, MIXEDPTRS* u, CRenderObj* pRender,
    int iPartId, int iStateId)
{
    HRESULT hr;
    signed __int64 v11;
    __int64 v12;
    signed __int64 v14;
    TMBITMAPHEADER* v16;
    HBITMAP v22;
    int v23;
    signed __int64 v24;
    int v28;
    void* v29;
    int v30;
    int v31;
    HBITMAP v32;
    unsigned* v33;
    signed __int64 v34;
    int iPartIda;

    hr = 0;
    if (iStateId != 0 || !pdi->fPartiallyTransparent || pdi->iDibOffset <= 0)
        return hr;
    if (pdi->iDibOffset == 56864) // FIXME
        return hr;

    v11 = pImageObj->_iImageCount;
    pdi->iRgnListOffset = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData) + 8;

    hr = EmitEntryHdr(u, TMT_IMAGEINFO, TMT_IMAGEINFO);
    if (hr < 0) {
        EndEntry(u);
        return hr;
    }

    v12 = Align8(4 * (pImageObj->_iImageCount + 1));
    hr = AllocateThemeFileBytes(u->pb, v12 + 8);
    if (hr < 0) {
        EndEntry(u);
        return hr;
    }

    *u->pb = pImageObj->_iImageCount + 1;
    v14 = (signed __int64)(u->pb + 8);
    v34 = (signed __int64)(u->pb + 8);
    memset(u->pb + 8, 0, (unsigned)v12);

    u->pb = (char *)(v14 + v12);

    v32 = 0;
    v28 = 0;
    if (pImageObj->_iImageCount >= 0) {
        if (pdi->fPartiallyTransparent) {
            if (pRender->_pbSharableData && pdi->iDibOffset > 0) {
                v16 = pRender->GetBitmapHeader(pdi->iDibOffset);
                v28 = pRender->_phBitmapsArray[v16->iBitmapIndex].hBitmap != nullptr;
            }

            hr = pRender->ExternalGetBitmap(nullptr, pdi->iDibOffset, GBF_DIRECT, &v32);
            v22 = v32;
            if (hr >= 0) {
                CBitmapPixels pixels;
                hr = pixels.OpenBitmap(
                    nullptr,
                    v32,
                    0,
                    &v33,
                    &v31,
                    &v30,
                    nullptr,
                    nullptr,
                    nullptr,
                    0);
                if (hr >= 0) {
                    v23 = 0;
                    v29 = 0;
                    iPartIda = 0;
                    if (v11 >= 0) {
                        v24 = (signed __int64)v29;
                        do {
                            RGNDATA* rgnData = nullptr;
                            int rgnDataLen;
                            hr = pImageObj->BuildRgnData(
                                v33,
                                v31,
                                v30,
                                pdi,
                                pRender,
                                v23,
                                &rgnData,
                                &rgnDataLen);
                            if (hr >= 0) {
                                if (rgnDataLen) {
                                    *(DWORD *)(v34 + 4 * v24) = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
                                    hr = EmitEntryHdr(u, TMT_REGIONDATA, TMT_REGIONDATA);
                                    if (hr >= 0) {
                                        hr = EmitAndCopyBlock(u, rgnData, rgnDataLen);
                                        EndEntry(u);
                                    }
                                }
                            }
                            free(rgnData);
                            ++v24;
                            v23 = iPartIda++ + 1;
                        } while (v24 <= v11);
                        v22 = v32;
                    }
                }
            }
            if (v22 && !v28)
                DeleteObject(v22);
        }
    }

    EndEntry(u);
    return hr;
}

} // namespace uxtheme
