#include "UxThemeFile.h"

#include "Utils.h"
#include <cassert>
#include <strsafe.h>
#include "Sections.h"

namespace uxtheme
{

CUxThemeFile::CUxThemeFile()
    : _pbSharableData(nullptr)
    , _hSharableSection(nullptr)
    , _pbNonSharableData(nullptr)
    , _hNonSharableSection(nullptr)
    , _Debug_Generation(-1)
    , _Debug_ChangeID(-1)
{
    StringCchCopyA(_szHead, 8, "thmfile");
    StringCchCopyA(_szTail, 4, "end");
}

CUxThemeFile::~CUxThemeFile()
{
    if (_pbSharableData || _hSharableSection || _pbNonSharableData || _hNonSharableSection)
        CloseFile();
    StringCchCopyA(_szHead, 8, "deleted");
}

HRESULT CUxThemeFile::CreateFileW(
    wchar_t* pszSharableSectionName, unsigned cchSharableSectionName,
    int iSharableSectionLength, wchar_t* pszNonSharableSectionName,
    unsigned cchNonSharableSectionName, int iNonSharableSectionLength,
    int fReserve)
{
    _pbSharableData = (THEMEHDR*)VirtualAlloc(nullptr, iSharableSectionLength, MEM_RESERVE, PAGE_READWRITE);
    _cbSharableData = iSharableSectionLength;
    _pbNonSharableData = (char*)VirtualAlloc(nullptr, iNonSharableSectionLength, MEM_RESERVE, PAGE_READWRITE);
    _cbNonSharableData = iNonSharableSectionLength;
    return S_OK;
}

void CUxThemeFile::CloseFile()
{
    if (_hSharableSection && _hNonSharableSection && _pbNonSharableData) {
        if (*_pbNonSharableData & 4 && (!_pbNonSharableData || !(*_pbNonSharableData & 2)))
            ; // ClearStockObjects(_hNonSharableSection, 0);
    }

    if (_pbSharableData)
        VirtualFree(_pbSharableData, 0, MEM_RELEASE);
    if (_hSharableSection)
        CloseHandle(_hSharableSection);
    if (_pbNonSharableData)
        VirtualFree(_pbNonSharableData, 0, MEM_RELEASE);
    if (_hNonSharableSection)
        CloseHandle(_hNonSharableSection);

    _pbSharableData = nullptr;
    _hSharableSection = nullptr;
    _pbNonSharableData = nullptr;
    _hNonSharableSection = nullptr;
}

LOGFONTW const* CUxThemeFile::GetFontByIndex(unsigned short index) const
{
    if (index >= _pbSharableData->cFonts)
        assert("FRE: index < pHeader->cFonts");

    auto ptr = reinterpret_cast<char*>(&_pbSharableData[index]) +
        _pbSharableData->iFontsOffset;
    return reinterpret_cast<LOGFONTW const*>(ptr);
}

HRESULT CUxThemeFile::GetGlobalTheme(HANDLE* phSharableSection, HANDLE* phNonSharableSection)
{
    RootSection rootSection(FILE_MAP_READ, FILE_MAP_READ);
    ROOTSECTION* rootSectionData;
    ENSURE_HR(rootSection.GetRootSectionData(&rootSectionData));

    Section section(FILE_MAP_READ, FILE_MAP_READ);
    ENSURE_HR(section.OpenSection(rootSectionData->szSharableSectionName, true));
    auto hdr = static_cast<THEMEHDR const*>(section.View());



    PEB *pPEB; // rax@1
    signed int v5; // ebx@1
    const wchar_t *v6; // rdi@3
    ROOTSECTION *ppRootSection; // [sp+20h] [bp-C0h]@1
    CSection_2 v9; // [sp+30h] [bp-B0h]@1
    CSection v10; // [sp+240h] [bp+160h]@1
    CRootSection v11; // [sp+470h] [bp+390h]@1

    *phSharableSection = 0i64;
    *phNonSharableSection = 0i64;
    pPEB = (_PEB *)*MK_FP(__GS__, 96i64);
    ppRootSection = 0i64;
    CRootSection::CRootSection(&v11, pPEB->SessionId, 4u, 4u);
    v10._dwDesiredSectionAccess = 4;
    _mm_store_si128((__m128i *)&v10._hSection, 0i64);
    _mm_store_si128((__m128i *)&v9._hSection, 0i64);
    v10._dwDesiredViewAccess = 4;
    v10._szSectionName[0] = 0;
    v10.vfptr = (CSectionVtbl *)&CDataSection::`vftable';
        v9._dwDesiredSectionAccess = 4;
    v9._dwDesiredViewAccess = 4;
    v9._szSectionName[0] = 0;
    v9.vfptr = (CSectionVtbl *)&CDataSection::`vftable';
        v5 = CRootSection::GetRootSectionData(&v11, &ppRootSection);
    if (v5 >= 0)
    {
        if (ppRootSection->szSharableSectionName[0]
            && (v6 = ppRootSection->szNonSharableSectionName, ppRootSection->szNonSharableSectionName[0]))
        {
            v5 = CSection::OpenSection(&v10, ppRootSection->szSharableSectionName, 0);
            if (v5 >= 0)
            {
                v5 = CSection::OpenSection((CSection *)&v9, v6, 0);
                if (v5 >= 0)
                {
                    *phSharableSection = v10._hSection;
                    *phNonSharableSection = v9._hSection;
                    v10._hSection = 0i64;
                    v9._hSection = 0i64;
                }
            }
        } else
        {
            v5 = -2147023728;
        }
    }
    v9.vfptr = (CSectionVtbl *)&CDataSection::`vftable';
        CSection::~CSection((CSection *)&v9);
    v10.vfptr = (CSectionVtbl *)&CDataSection::`vftable';
        CSection::~CSection(&v10);
    v11.vfptr = (CSectionVtbl *)CRootSection::`vftable';
        CSection::~CSection((CSection *)&v11.vfptr);
    return (unsigned int)v5;
}

} // namespace uxtheme
