#pragma once
#include <vector>
#include <windows.h>
#include <vssym32.h>

namespace uxtheme
{

struct THEMEMETRICS
{
    LOGFONTW lfFonts[9];
    COLORREF crColors[31];
    int iSizes[10];
    BOOL fBools[1];
    int iStringOffsets[4];
    int iInts[1];
};

struct DIBREUSEDATA
{
    int iDIBBitsOffset;
    short iWidth;
    short iHeight;
};

struct DIBDATA
{
    void* pDIBBits;
    DIBREUSEDATA dibReuseData;
};

union MIXEDPTRS
{
    BYTE* pb;
    char* pc;
    unsigned short* pw;
    short* ps;
    wchar_t* px;
    int* pi;
    DWORD* pdw;
    POINT* ppt;
    SIZE* psz;
    RECT* prc;
};

struct ENTRYHDR
{
    USHORT usTypeNum;
    BYTE ePrimVal;
    DWORD dwDataLen;

    BYTE* Data()
    {
        return reinterpret_cast<BYTE*>(
            reinterpret_cast<uintptr_t>(this) + sizeof(ENTRYHDR));
    }

    BYTE const* Data() const
    {
        return reinterpret_cast<BYTE*>(
            reinterpret_cast<uintptr_t>(this) + sizeof(ENTRYHDR));
    }

    ENTRYHDR* Next()
    {
        return reinterpret_cast<ENTRYHDR*>(
            reinterpret_cast<uintptr_t>(this) + sizeof(ENTRYHDR) + dwDataLen);
    }

    ENTRYHDR const* Next() const
    {
        return reinterpret_cast<ENTRYHDR*>(
            reinterpret_cast<uintptr_t>(this) + sizeof(ENTRYHDR) + dwDataLen);
    }
};

struct NONSHARABLEDATAHDR
{
    unsigned dwFlags;
    int iLoadId;
    int cBitmaps;
    int iBitmapsOffset;
};

struct PARTOBJHDR
{
    int iPartId;
    int iStateId;
};

struct REGIONLISTHDR
{
    char cStates;
};

struct alignas(4) PARTJUMPTABLEHDR
{
    int iBaseClassIndex;
    int iFirstDrawObjIndex;
    int iFirstTextObjIndex;
    char cParts;
};

struct alignas(8) STATEJUMPTABLEHDR
{
    char cStates;
};

struct PART_STATE_INDEX
{
    int iPartId;
    int iStateId;
    int iIndex;
    int iLen;
};

struct APPCLASSINFO
{
    int iAppNameIndex = 0;
    int iClassNameIndex = 0;
};

struct alignas(8) APPCLASSLIVE
{
    APPCLASSINFO AppClassInfo;
    int iIndex;
    int iLen;
    int iBaseClassIndex;
};

struct APPCLASSLOCAL
{
    std::wstring csAppName;
    std::wstring csClassName;
    int iMaxPartNum = 0;
    std::vector<PART_STATE_INDEX> PartStateIndexes;
    APPCLASSINFO AppClassInfo;
};

struct THEMEHDR
{
    char szSignature[8];
    unsigned dwVersion;
    FILETIME ftModifTimeStamp;
    unsigned dwTotalLength;
    int iDllNameOffset;
    int iColorParamOffset;
    int iSizeParamOffset;
    unsigned dwLangID;
    int iLoadDPI;
    unsigned dwLoadDPIs;
    int iLoadPPI;
    int iStringsOffset;
    int iStringsLength;
    int iSectionIndexOffset;
    int iSectionIndexLength;
    int iGlobalsOffset;
    int iGlobalsTextObjOffset;
    int iGlobalsDrawObjOffset;
    int iSysMetricsOffset;
    int iFontsOffset;
    int cFonts;
};

union HBITMAP64
{
    HBITMAP hBitmap;
    void* hBitmap64;
};

struct alignas(8) DIBINFO
{
    HBITMAP64 uhbm;
    int iDibOffset;
    int iSingleWidth;
    int iSingleHeight;
    int iRgnListOffset;
    SIZINGTYPE eSizingType;
    BOOL fBorderOnly;
    BOOL fPartiallyTransparent;
    int iAlphaThreshold;
    int iMinDpi;
    SIZE szMinSize;
};

struct TRUESTRETCHINFO
{
    BOOL fForceStretch;
    BOOL fFullStretch;
    SIZE szDrawSize;
};

struct REUSEDATAHDR
{
    wchar_t szSharableSectionName[MAX_PATH];
    int iDIBReuseRecordsCount;
    int iDIBReuseRecordsOffset;
    DWORD dwTotalLength;
};

} // namespace uxtheme
