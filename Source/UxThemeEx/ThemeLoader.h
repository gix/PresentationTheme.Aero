#pragma once
#include "ImageFile.h"
#include "UxThemeFile.h"
#include "VSUnpack.h"
#include <vector>
#include <windows.h>

namespace uxtheme
{
class CRenderObj;

struct LOADTHEMEMETRICS : THEMEMETRICS
{
    std::wstring wsStrings[4];
};

typedef HRESULT(*PFNALLOCSECTIONS)(
    CUxThemeFile *, wchar_t *, unsigned, int, wchar_t *, unsigned, int, int);

class CThemeLoader : public IParserCallBack
{
public:
    CThemeLoader();

    HRESULT AddIndex(wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId, int iIndex, int iLen) override;
    HRESULT AddData(short sTypeNum, unsigned char ePrimVal, void const* pData, unsigned dwLen) override;
    int GetNextDataIndex() override { return _iLocalLen; }
    int AddToDIBDataArray(void* pDIBBits, short width, short height) override;
    HRESULT AddBaseClass(int idClass, int idBaseClass) override;
    int GetScreenPpi() override;

    HRESULT AllocateThemeFileBytes(BYTE* upb, unsigned dwAdditionalLen);
    HRESULT EmitEntryHdr(MIXEDPTRS* u, short propnum, BYTE privnum);
    int EndEntry(MIXEDPTRS* u);
    BOOL IndexExists(wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId);
    HRESULT AddMissingParent(wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId);
    HRESULT AddDataInternal(short sTypeNum, unsigned char ePrimVal, void const* pData, unsigned dwLen);
    void FreeLocalTheme();
    HRESULT LoadTheme(HINSTANCE hInst, wchar_t const* pszThemeName,
                      wchar_t const* pszColorParam,
                      wchar_t const*pszSizeParam, HANDLE* phReuseSection,
                      bool fGlobalTheme, bool fHighContrast);
    HRESULT EmitString(MIXEDPTRS* u, wchar_t const* pszSrc, unsigned cchSrc, int* piOffSet);
    HRESULT EmitObject(MIXEDPTRS* u, short propnum, unsigned char privnum, void* pHdr, unsigned dwHdrLen, void* pObj, unsigned dwObjLen, CRenderObj*);
    HRESULT MakeStockObject(CRenderObj* pRender, DIBINFO* pdi);
    HRESULT PackImageFileInfo(DIBINFO* pdi, CImageFile* pImageObj, MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId);
    HRESULT EmitAndCopyBlock(MIXEDPTRS* u, void const* pSrc, unsigned dwLen);
    HRESULT PackDrawObject(MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId);
    BOOL KeyTextPropertyFound(int iStateDataOffset);
    HRESULT PackTextObjects(MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, bool fGlobals);
    HRESULT PackTextObject(MIXEDPTRS* u, CRenderObj* pRender, int iPartId, int iStateId);
    HRESULT PackMetrics();
    HRESULT CopyDummyNonSharableDataToLive();
    HRESULT CreateReuseSection(wchar_t const* pszSharableSectionName, void** phReuseSection);
    HRESULT CopyNonSharableDataToLive(HANDLE hReuseSection);
    HRESULT PackAndLoadTheme(HANDLE hFile,
                             wchar_t const* pszThemeName,
                             wchar_t const* pszColorParam,
                             wchar_t const* pszSizeParam,
                             unsigned cbMaxDesiredSharableSectionSize,
                             wchar_t* pszSharableSectionName,
                             unsigned cchSharableSectionName,
                             wchar_t* pszNonSharableSectionName,
                             unsigned cchNonSharableSectionName,
                             HANDLE* phReuseSection,
                             PFNALLOCSECTIONS pfnAllocSections);
    HRESULT CopyLocalThemeToLive(void* hFile,
                                 int iTotalLength,
                                 wchar_t const* pszThemeName,
                                 wchar_t const* pszColorParam,
                                 wchar_t const* pszSizeParam);
    HRESULT CopyClassGroup(APPCLASSLOCAL* pac, MIXEDPTRS* u, APPCLASSLIVE* pacl);
    int GetPartOffset(CRenderObj* pRender, int iPartId);
    BOOL KeyDrawPropertyFound(int iStateDataOffset);
    HRESULT PackDrawObjects(MIXEDPTRS* uOut, CRenderObj* pRender, int iMaxPart, bool fGlobals);
    HRESULT CopyPartGroup(APPCLASSLOCAL* pac, MIXEDPTRS* u, int iPartId, int* piPartJumpTable, int iPartZeroIndex, int iBaseClassIndex, bool fGlobalsGroup);

    HRESULT AddIndexInternal(wchar_t const* pszAppName, wchar_t const* pszClassName, int iPartId, int iStateId, int iIndex, int iLen);
    HRESULT GetFontTableIndex(LOGFONTW const* pFont, unsigned short* pIndex);

    std::wstring _wsThemeFileName;
    int _iGlobalsOffset;
    int _iSysMetricsOffset;
    int _iGlobalsTextObj;
    int _iGlobalsDrawObj;
    BYTE* _pbLocalData;
    int _iLocalLen;
    std::vector<APPCLASSLOCAL> _LocalIndexes;
    ENTRYHDR* _pbEntryHdrs[5];
    int _iEntryHdrLevel;
    CUxThemeFile _LoadingThemeFile;
    LOADTHEMEMETRICS _LoadThemeMetrics;
    bool _fGlobalTheme;
    int _iCurrentScreenPpi;
    unsigned short _wCurrentLangID;
    THEMEHDR* _hdr;
    unsigned _dwPageSize;
    std::vector<DIBDATA> _rgDIBDataArray;
    std::vector<int> _rgBaseClassIds;
    std::vector<LOGFONTW> _fontTable;
};

} // namespace uxtheme
