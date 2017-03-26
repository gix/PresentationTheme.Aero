#include "ThemeLoader.h"

#include "DpiInfo.h"
#include "BorderFill.h"
#include "RenderObj.h"
#include "TextDraw.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"

#include <algorithm>
#include <shlwapi.h>
#include <strsafe.h>
#include <vssym32.h>

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

    //InitThemeMetrics(&_LoadThemeMetrics);
    _dwPageSize = systemInfo.dwPageSize;
    _wCurrentLangID = 0;
    _iCurrentScreenPpi = 96;
}

HRESULT CThemeLoader::AllocateThemeFileBytes(char* upb, unsigned int dwAdditionalLen)
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
    wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId, int iIndex, int iLen)
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

HRESULT CThemeLoader::AddData(short sTypeNum, unsigned char ePrimVal, void const* pData, unsigned dwLen)
{
    if (ePrimVal == 210) {
        unsigned short idx = 0;
        ENSURE_HR(GetFontTableIndex((LOGFONTW*)pData, &idx));
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
    unsigned int cbStruct;
    unsigned int ulFlags;
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

    ENSURE_HR(unpack->Initialize(pvsl->hInstVS, 0, pvsl->ulFlags & 1, fIsLiteVisualStyle));
    ENSURE_HR(unpack->LoadRootMap(pvsl->pfnCB));
    ENSURE_HR(unpack->LoadClassDataMap(pvsl->pszColorVariant, pvsl->pszSizeVariant, pvsl->pfnCB));
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
    MIXEDPTRS* u, wchar_t const* pszSrc, unsigned int cchSrc, int* piOffSet)
{
    unsigned paddedLen = Align8(2 * (cchSrc + 1));
    ENSURE_HR(AllocateThemeFileBytes(u->pb, paddedLen));
    ENSURE_HR(StringCchCopyW((wchar_t *)u->pb, cchSrc + 1, pszSrc));

    if (piOffSet)
        *piOffSet = (char*)(u->pi) - (char*)(_LoadingThemeFile._pbSharableData);
    u->pb += paddedLen;

    return S_OK;
}

HRESULT CThemeLoader::EmitObject(
    MIXEDPTRS* u, short propnum, char privnum, void* pHdr, unsigned dwHdrLen,
    void* pObj, unsigned int dwObjLen)
{
    ENSURE_HR(EmitEntryHdr(u, propnum, privnum));
    ENSURE_HR(AllocateThemeFileBytes(u->pb, dwHdrLen));

    memcpy(u->pb, pHdr, dwHdrLen);
    u->pb += dwHdrLen;

    auto paddedLen = Align8(dwObjLen);
    ENSURE_HR(AllocateThemeFileBytes(u->pb, paddedLen));
    memcpy(u->pb, pObj, dwObjLen);
    u->pb += paddedLen;

    EndEntry(u);

    return S_OK;
}

HRESULT CThemeLoader::GetFontTableIndex(LOGFONTW* pFont, unsigned short* pIndex)
{
    if (_fontTable.empty()) {
        LOGFONTW t;
        memset(&t, 0, sizeof(LOGFONTW));
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

HRESULT CThemeLoader::PackDrawObject(MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId)
{
    int bgType;
    if (pRender->ExternalGetEnumValue(iPartId, iStateId/*0*/, TMT_BGTYPE, &bgType) < 0)
        bgType = BT_BORDERFILL;

    PARTOBJHDR partHdr;
    partHdr.iPartId = iPartId;
    partHdr.iStateId = iStateId/*0*/;

    if (bgType == BT_NONE || bgType == BT_BORDERFILL) {
        CBorderFill borderFill;
        ENSURE_HR(borderFill.PackProperties(pRender, bgType == BT_NONE, iPartId, iStateId/*0*/));
        return EmitObject(u, 16, 16, &partHdr, sizeof(partHdr), &borderFill, sizeof(borderFill));
    }

    CMaxImageFile imgFile;
    memset(imgFile.MultiDibs, 0, sizeof(imgFile.MultiDibs));
    ENSURE_HR(imgFile.PackProperties(pRender, iPartId, iStateId/*0*/));

    DIBINFO* pScaledDibInfo = nullptr;
    HRESULT hr = imgFile.CreateScaledBackgroundImage(pRender, iPartId, iStateId/*0*/, &pScaledDibInfo);
    if (hr < 0)
        return hr;

    if (hr == 0)
        ENSURE_HR(MakeStockObject(pRender, pScaledDibInfo));

    int v14 = imgFile._iMultiImageCount;
    for (int i = 0; ; ++i) {
        auto dibinfo = imgFile.EnumImageFiles(i);
        if (!dibinfo)
            break;
        ENSURE_HR(PackImageFileInfo(dibinfo, &imgFile, u, pRender, iPartId/*0*/, iStateId/*0*/));
        if (hr < 0)
            return hr;
    }

    return EmitObject(u, 16, 16, &partHdr, sizeof(partHdr), &imgFile, sizeof(DIBINFO) * v14 + sizeof(CImageFile));
}

BOOL CThemeLoader::KeyTextPropertyFound(int iStateDataOffset)
{
    unsigned int v2; // er9@1
    char* i; // r8@1

    v2 = 0;
    for (i = &_LoadingThemeFile._pbSharableData->szSignature[iStateDataOffset];
         *(WORD *)i != 12;
         i = (char *)(*(DWORD *)(i + 4) + i + 8))
    {
        if ((unsigned int)CTextDraw::KeyProperty(*(WORD *)i))
            return 1;
    }
    return v2;
}

HRESULT CThemeLoader::PackTextObjects(
    MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, int fGlobals)
{
    HRESULT hr; // er10@1
    int v6; // edi@1
    int v9; // er15@2
    int v10; // eax@3
    MIXEDPTRS* v11; // r11@3
    char* v12; // rbx@4
    int v13; // er14@5
    __int64 v14; // rsi@5
    int v15; // eax@5
    __int64 v16; // r12@5
    int v17; // edx@6

    hr = 0;
    v6 = 0;
    if (iMaxPart >= 0)
    {
        v9 = fGlobals;
        do
        {
            v10 = GetPartOffset(pRender, v6);
            if (v10 != -1)
            {
                v12 = &_LoadingThemeFile._pbSharableData->szSignature[v10];
                if (*(WORD *)v12 == 11)
                {
                    v13 = 0;
                    v14 = 0i64;
                    v15 = (unsigned __int8)v12[8] - 1;
                    v16 = v15;
                    if (v15 >= 0)
                    {
                        do
                        {
                            v17 = *(DWORD *)&v12[4 * v14 + 16];
                            if (v17 != -1 && (v9 || KeyTextPropertyFound(v17)))
                            {
                                hr = PackTextObject(uOut, pRender, v6, v13);
                                if (hr < 0)
                                    return hr;
                                v11 = uOut;
                                v9 = 0;
                            }
                            ++v13;
                            ++v14;
                        } while (v14 <= v16);
                    }
                } else if (v9 || KeyTextPropertyFound(v10))
                {
                    hr = PackTextObject(uOut, pRender, v6, 0);
                    if (hr < 0)
                        return hr;
                }
            }
            ++v6;
        } while (v6 <= iMaxPart);
    }
    return hr;
}

HRESULT CThemeLoader::PackTextObject(MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId)
{
    CTextDraw textDraw;
    ENSURE_HR(textDraw.PackProperties(pRender, iPartId, iStateId));
    return EmitObject(u, 17, 17, &iPartId, sizeof(iPartId), &textDraw, sizeof(textDraw));
}

int CThemeLoader::GetScreenPpi()
{
    return _iCurrentScreenPpi;
}

HRESULT CThemeLoader::PackMetrics()
{
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

    *phReuseSection = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, maxSize, nullptr);
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
    HRESULT hr; // edi@1
    NONSHARABLEDATAHDR* nonSharableDataHdr; // r13@1
    NONSHARABLEDATAHDR* v5; // r15@1
    REUSEDATAHDR* v7; // rsi@3
    int v8; // ecx@4
    HBITMAP* bitmapIt; // rbx@6
    __int64 iBitmapsOffset; // rbx@7
    __int64 iDIBReuseRecordsCount; // rax@7
    __int64 iDIBReuseRecordsOffset; // rsi@7
    DIBREUSEDATA* dibReuseRec; // rsi@7
    HBITMAP* bitmapsEnd; // rax@7 MAPDST
    int v15; // eax@9
    int v16; // eax@9
    HBITMAP v17; // rax@10
    HBITMAP v18; // r12@12
    HBITMAP v19; // rax@14
    char* v20; // rsi@24
    void* v21; // rax@26
    __int64 v25; // [sp+40h] [bp-29h]@11
    REUSEDATAHDR* reuseDataHdr; // [sp+48h] [bp-21h]@1 MAPDST
    _BITMAPHEADER bitmapHdr; // [sp+50h] [bp-19h]@7

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

HRESULT CThemeLoader::PackAndLoadTheme(
    void* hFile, wchar_t const* pszThemeName, wchar_t const* pszColorParam,
    wchar_t const* pszSizeParam, unsigned int cbMaxDesiredSharableSectionSize,
    wchar_t* pszSharableSectionName, unsigned int cchSharableSectionName,
    wchar_t* pszNonSharableSectionName, unsigned int cchNonSharableSectionName,
    void** phReuseSection,
    PFNALLOCSECTIONS pfnAllocSections)
{
    HRESULT hr = S_OK;

    ENSURE_HR(PackMetrics());

    if (pfnAllocSections) {
        hr = pfnAllocSections(
            &_LoadingThemeFile,
            pszSharableSectionName,
            cchSharableSectionName,
            cbMaxDesiredSharableSectionSize,
            pszNonSharableSectionName,
            cchNonSharableSectionName,
            8 * _rgDIBDataArray.size() + 16,
            1);
    } else {
        hr = _LoadingThemeFile.CreateFileW(
            pszSharableSectionName,
            cchSharableSectionName,
            cbMaxDesiredSharableSectionSize,
            pszNonSharableSectionName,
            cchNonSharableSectionName,
            8 * _rgDIBDataArray.size() + 16,
            0);
    }

    if (hr >= 0) {
        if (phReuseSection) {
            hr = CreateReuseSection(pszSharableSectionName, phReuseSection);
            if (hr < 0)
                return hr;
            hr = CopyNonSharableDataToLive(*phReuseSection);
        } else {
            hr = CopyDummyNonSharableDataToLive();
        }

        if (hr >= 0)
            hr = CopyLocalThemeToLive(
                hFile,
                cbMaxDesiredSharableSectionSize,
                pszThemeName,
                pszColorParam,
                pszSizeParam);
    }

    return hr;
}

HRESULT CThemeLoader::CopyLocalThemeToLive(
    void* hFile, int iTotalLength, wchar_t const* pszThemeName,
    wchar_t const* pszColorParam, wchar_t const* pszSizeParam)
{
    auto themeHdr = (THEMEHDR*)VirtualAlloc(_LoadingThemeFile._pbSharableData, 0x60, MEM_COMMIT, PAGE_READWRITE);
    if (!themeHdr)
        return 0x80070008;

    DpiInfo* v16;
    MIXEDPTRS u;

    _hdr = themeHdr;
    _iGlobalsOffset = -1;
    _iSysMetricsOffset = -1;
    u.pb = (char*)(themeHdr + 1) + 4;
    _hdr->dwTotalLength = iTotalLength;
    *(uint64_t*)_hdr = 0x4D48544E49474542;
    _hdr->dwVersion = 65543;
    _hdr->iDllNameOffset = 0;
    _hdr->iColorParamOffset = 0;
    _hdr->iSizeParamOffset = 0;
    _hdr->dwLangID = _wCurrentLangID;
    v16 = (DpiInfo *)_hdr;
    v16[3]._nNonStandardDpi = GetScreenDpi();
    v16->Ensure(0);
    _hdr->dwLoadDPIs = g_DpiInfo._nDpiPlateausCurrentlyPresent;
    _hdr->iLoadPPI = GetScreenPpi();
    _hdr->iGlobalsOffset = 0;
    _hdr->iGlobalsTextObjOffset = 0;
    _hdr->iGlobalsDrawObjOffset = 0;
    _hdr->iFontsOffset = 0;
    _hdr->cFonts = _fontTable.size();
    _hdr->ftModifTimeStamp = FILETIME();
    GetFileTime(hFile, nullptr, nullptr, &_hdr->ftModifTimeStamp);
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

    int* v28 = (int*)_LoadThemeMetrics.iStringOffsets;
    for (std::wstring const& str : _LoadThemeMetrics.wsStrings) {
        if (size_t len = str.length())
            ENSURE_HR(EmitString(&u, str.c_str(), len, v28));
        else
            *v28 = 0;
        ++v28;
    }

    _hdr->iStringsLength = (uintptr_t)u.pb - (uintptr_t)stringsBegin;

    _hdr->iSectionIndexOffset = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData;
    _hdr->iSectionIndexLength = sizeof(APPCLASSLIVE) * _LocalIndexes.size();
    ENSURE_HR(AllocateThemeFileBytes(u.pb, _hdr->iSectionIndexLength));

    char* v33 = u.pb;
    u.pb += _hdr->iSectionIndexLength;
    if (_LocalIndexes.size() > 0) {
        auto liveClasses = (APPCLASSLIVE*)v33;
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
                ENSURE_HR(EmitEntryHdr(&u, 1, 1));
                ENSURE_HR(EmitAndCopyBlock(&u, &_LoadThemeMetrics, sizeof(THEMEMETRICS)));
                EndEntry(&u);

                ENSURE_HR(EmitEntryHdr(&u, 12, 12));
                ENSURE_HR(AllocateThemeFileBytes(u.pb, 8));
                *(int*)u.pb = -1;
                u.pb += 8;
                EndEntry(&u);
            } else {
                ENSURE_HR(CopyClassGroup(&pac, &u, &liveCls));
            }

            liveCls.iLen = (uintptr_t)u.pb - (uintptr_t)_LoadingThemeFile._pbSharableData - liveCls.iIndex;
        }
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

struct __declspec(align(4)) PARTJUMPTABLEHDR
{
    int iBaseClassIndex;
    int iFirstDrawObjIndex;
    int iFirstTextObjIndex;
    char cParts;
};

HRESULT CThemeLoader::CopyClassGroup(APPCLASSLOCAL* pac, MIXEDPTRS* u, APPCLASSLIVE* pacl)
{
    int fGlobalsGroup; // er15@1
    HRESULT hr; // ebx@1
    int* partJumpTable; // r12@4
    int iPartZeroIndex; // ebp@7
    int screenDpi; // eax@11
    int v21; // er9@11
    char* v26; // [sp+70h] [bp-48h]@1
    CRenderObj* pRender; // [sp+C0h] [bp+8h]@1

    pRender = 0i64;
    v26 = u->pb;
    fGlobalsGroup = _iGlobalsOffset == (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

    int cParts = pac->iMaxPartNum + 1;

    ENSURE_HR(EmitEntryHdr(u, 10, 10));
    ENSURE_HR(AllocateThemeFileBytes(u->pb, sizeof(PARTJUMPTABLEHDR) + 4 * cParts));

    auto partJumpTableHdr = (PARTJUMPTABLEHDR*)u->pb;
    partJumpTableHdr->iBaseClassIndex = pacl->iBaseClassIndex;
    partJumpTableHdr->iFirstTextObjIndex = 0;
    partJumpTableHdr->cParts = cParts;
    u->pb = (char*)(partJumpTableHdr + 1);

    partJumpTable = (int*)u->pb;
    for (int i = cParts; i; --i) {
        *(int*)u->pb = -1;
        u->pb += 4;
    }
    EndEntry(u);

    iPartZeroIndex = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
    for (int p = 0; p <= pac->iMaxPartNum; ++p)
        CopyPartGroup(pac, u, p, partJumpTable, iPartZeroIndex, pacl->iBaseClassIndex, fGlobalsGroup);

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
        0,
        0,
        &pRender);

    if (hr >= 0) {
        if (screenDpi != 96)
            pRender->_dwOtdFlags |= OTD_FORCE_RECT_SIZING;

        if (fGlobalsGroup)
            _iGlobalsDrawObj = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

        *((DWORD *)partJumpTableHdr + 1) = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
        hr = PackDrawObjects(u, pRender, pac->iMaxPartNum, fGlobalsGroup);
        if (hr >= 0) {
            if (fGlobalsGroup)
                _iGlobalsTextObj = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
            *((DWORD *)partJumpTableHdr + 2) = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
            hr = PackTextObjects(u, pRender, pac->iMaxPartNum, fGlobalsGroup);
            if (hr >= 0)
            {
                hr = EmitEntryHdr(u, 20, 20);
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
    if (entryHdr->usTypeNum != 10)
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
        if (hdr->usTypeNum == 12)
            break;

        switch (hdr->usTypeNum) {
        case TMT_BORDERSIZE:
        case TMT_ROUNDCORNERWIDTH:
        case TMT_ROUNDCORNERHEIGHT:
        case TMT_GRADIENTRATIO1:
        case TMT_GRADIENTRATIO2:
        case TMT_GRADIENTRATIO3:
        case TMT_GRADIENTRATIO4:
        case TMT_GRADIENTRATIO5:
        case TMT_CONTENTMARGINS:
        case TMT_BORDERCOLOR:
        case TMT_FILLCOLOR:
        case TMT_GRADIENTCOLOR1:
        case TMT_GRADIENTCOLOR2:
        case TMT_GRADIENTCOLOR3:
        case TMT_GRADIENTCOLOR4:
        case TMT_GRADIENTCOLOR5:
        case TMT_BGTYPE:
        case TMT_BORDERTYPE:
        case TMT_FILLTYPE:
            return TRUE;
        default:
            if (CImageFile::KeyProperty(hdr->usTypeNum))
                return TRUE;
        }

        ptr += sizeof(ENTRYHDR) + hdr->dwDataLen;
    }

    return FALSE;
}

HRESULT CThemeLoader::PackDrawObjects(MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, int fGlobals)
{
    for (int partId = 0; partId <= iMaxPart; ++partId) {
        int offset = GetPartOffset(pRender, partId);
        if (offset == -1)
            continue;

        auto entryHdr = (ENTRYHDR *)((char*)_LoadingThemeFile._pbSharableData + offset);
        if (entryHdr->usTypeNum == 11) {
            auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entryHdr + 1);
            auto jumpTable = (int*)(jumpTableHdr + 1);

            for (int stateId = 0; stateId <= jumpTableHdr->cStates - 1; ++stateId) {
                int iStateDataOffset = jumpTable[stateId];
                if (iStateDataOffset != -1 && (fGlobals || KeyDrawPropertyFound(iStateDataOffset))) {
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
    if (piPartJumpTable)
        piPartJumpTable[iPartId] = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);

    int* stateJumpTable = nullptr;
    if (iMaxStateId > 0) {
        int cStates = iMaxStateId + 1;
        ENSURE_HR(EmitEntryHdr(u, 11, 11));
        ENSURE_HR(AllocateThemeFileBytes(u->pb, sizeof(STATEJUMPTABLEHDR) + sizeof(int) * cStates));

        auto stateJumpTableHdr = (STATEJUMPTABLEHDR*)u->pb;
        stateJumpTableHdr->cStates = cStates;

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

            hr = EmitAndCopyBlock(u, block, psi.iLen);
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

    HRESULT v4; // er9@1
    HBITMAP v10; // rax@4
    v4 = 1;
    //LODWORD(v10) = SetBitmapAttributes(hbmp, (unsigned int)v4);
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
    tagBITMAPINFOHEADER* _hdrBitmap;
    int _iWidth;
    int _iHeight;
    char* _buffer;
    ~CBitmapPixels();
    DWORD OpenBitmap(HDC__* hdc, HBITMAP__* bitmap, int fForceRGB32, unsigned** pPixels, int* piWidth, int* piHeight, int* piBytesPerPixel, int* piBytesPerRow, int* piPreviousBytesPerPixel, unsigned cbBytesBefore);
    void CloseBitmap(HDC__* hdc, HBITMAP__* hBitmap);
};

CBitmapPixels::~CBitmapPixels()
{
    if (_buffer)
        free(_buffer);
}

DWORD CBitmapPixels::OpenBitmap(
    HDC__* hdc, HBITMAP__* bitmap, int fForceRGB32, unsigned int** pPixels, int* piWidth, int* piHeight, int* piBytesPerPixel, int* piBytesPerRow, int* piPreviousBytesPerPixel, unsigned int cbBytesBefore)
{
    DWORD hr; // eax@2
    HDC v14; // rsi@3
    __int64 v15; // rdx@5
    unsigned __int64 v16; // rcx@5
    int v17; // edi@7
    unsigned __int64 v18; // rcx@7
    tagBITMAPINFOHEADER* v19; // rax@9
    BITMAP pv; // [sp+40h] [bp-38h]@5

    if (pPixels)
    {
        v14 = GetWindowDC(0i64);
        if (v14)
        {
            GetObjectW(bitmap, sizeof(BITMAP), &pv);
            v15 = (unsigned int)pv.bmHeight;
            v16 = 4i64 * (unsigned int)pv.bmWidth;
            this->_iWidth = pv.bmWidth;
            this->_iHeight = v15;
            if (v16 <= 0xFFFFFFFF
                && (unsigned int)v16 <= 0x7FFFFFFC
                && (v17 = (v16 + 3) & 0xFFFFFFFC, v18 = v15 * (unsigned int)v17, v18 <= 0xFFFFFFFF)
                && (signed int)v18 + 0x8C >= (unsigned int)v18
                && (v19 = (tagBITMAPINFOHEADER *)malloc((unsigned int)(v18 + 0x8C)),
                (this->_buffer = (char *)v19) != 0i64))
            {
                this->_hdrBitmap = v19;
                memset(v19, 0, 0x28ui64);
                this->_hdrBitmap->biSize = 40;
                this->_hdrBitmap->biWidth = this->_iWidth;
                this->_hdrBitmap->biHeight = this->_iHeight;
                this->_hdrBitmap->biPlanes = 1;
                this->_hdrBitmap->biBitCount = 32;
                this->_hdrBitmap->biCompression = 0;
                GetDIBits(
                    v14,
                    bitmap,
                    0,
                    this->_iHeight,
                    (char *)this->_hdrBitmap + 4 * this->_hdrBitmap->biClrUsed + this->_hdrBitmap->biSize,
                    (LPBITMAPINFO)this->_hdrBitmap,
                    0);
                ReleaseDC(0i64, v14);
                *pPixels = (unsigned int *)((char *)&this->_hdrBitmap->biSize
                                            + 4 * this->_hdrBitmap->biClrUsed
                                            + this->_hdrBitmap->biSize);
                if (piWidth)
                    *piWidth = this->_iWidth;
                if (piHeight)
                    *piHeight = this->_iHeight;
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

void CBitmapPixels::CloseBitmap(HDC__* hdc, HBITMAP__* hBitmap)
{
    HDC v5; // rsi@4

    if (this->_hdrBitmap && this->_buffer)
    {
        if (hBitmap)
        {
            v5 = GetWindowDC(0i64);
            SetDIBits(
                v5,
                hBitmap,
                0,
                this->_iHeight,
                (char *)this->_hdrBitmap + 4 * this->_hdrBitmap->biClrUsed + this->_hdrBitmap->biSize,
                (const BITMAPINFO *)this->_hdrBitmap,
                0);
            if (v5)
                ReleaseDC(0i64, v5);
        }
        free(this->_buffer);
        this->_hdrBitmap = 0i64;
        this->_buffer = 0i64;
    }
}

HRESULT CThemeLoader::PackImageFileInfo(
    DIBINFO* pdi, CImageFile* pImageObj, MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId)
{
    HRESULT hr; // edi@1
    signed __int64 v11; // r13@4
    __int64 v12; // rbx@5
    char* v13; // rax@6
    signed __int64 v14; // r12@6
    int v15; // edx@9
    TMBITMAPHEADER* v16; // rax@10
    HBITMAP__* v17; // r8@10
    int v18; // edx@10
    DWORD v19; // eax@11
    HDC__* v20; // rdx@11
    int v21; // er9@11
    HBITMAP__* v22; // r12@11
    int v23; // eax@13
    signed __int64 v24; // r12@14
    int* v26; // [sp+48h] [bp-90h]@0
    unsigned int v27; // [sp+50h] [bp-88h]@0
    int v28; // [sp+60h] [bp-78h]@6
    void* v29; // [sp+68h] [bp-70h]@13
    int v30; // [sp+70h] [bp-68h]@12
    int v31; // [sp+74h] [bp-64h]@12
    HBITMAP__* v32; // [sp+78h] [bp-60h]@6
    unsigned int* v33; // [sp+80h] [bp-58h]@12
    signed __int64 v34; // [sp+88h] [bp-50h]@6
    int iPartIda; // [sp+108h] [bp+30h]@13

    hr = 0;
    if (iStateId != 0 || !pdi->fPartiallyTransparent || pdi->iDibOffset <= 0)
        return hr;

    v11 = pImageObj->_iImageCount;
    pdi->iRgnListOffset = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData) + 8;

    hr = EmitEntryHdr(u, 18, 18);
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
    memset(u->pb + 8, 0, (unsigned int)v12);

    u->pb = (char *)(v12 + v14);

    v32 = 0;
    v28 = 0;
    if (pImageObj->_iImageCount >= 0) {
        if (pdi->fPartiallyTransparent) {
            if (pRender->_pbSharableData && pdi->iDibOffset > 0) {
                v16 = pRender->GetBitmapHeader(pdi->iDibOffset);
                v28 = pRender->_phBitmapsArray[v16->iBitmapIndex].hBitmap != nullptr;
            }

            v19 = pRender->ExternalGetBitmap(0, pdi->iDibOffset, 1u, &v32);
            v22 = v32;
            hr = v19;
            if ((v19 & 0x80000000) == 0) {
                CBitmapPixels v35;
                hr = v35.OpenBitmap(
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
                        //do {
                        //    v29 = 0;
                        //    hr = pImageObj->BuildRgnData(
                        //        v33,
                        //        v31,
                        //        v30,
                        //        pdi,
                        //        pRender,
                        //        v23,
                        //        (_RGNDATA **)&v29,
                        //        &iStateId);
                        //    if (hr >= 0) {
                        //        if (iStateId) {
                        //            *(DWORD *)(v34 + 4 * v24) = (uintptr_t)(u->pi) - (uintptr_t)(_LoadingThemeFile._pbSharableData);
                        //            hr = EmitEntryHdr(u, 19, 19);
                        //            if (hr >= 0) {
                        //                hr = EmitAndCopyBlock(u, v29, iStateId);
                        //                EndEntry(u);
                        //            }
                        //        }
                        //    }
                        //    free(v29);
                        //    ++v24;
                        //    v23 = iPartIda++ + 1;
                        //} while (v24 <= v11);
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
