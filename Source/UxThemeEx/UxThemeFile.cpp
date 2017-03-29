#include "UxThemeFile.h"
#include <strsafe.h>
#include <cassert>

namespace uxtheme
{

CUxThemeFile::CUxThemeFile()
{
    StringCchCopyA(_szHead, 8, "thmfile");
    StringCchCopyA(_szTail, 4, "end");
    _pbSharableData = nullptr;
    _hSharableSection = nullptr;
    _pbNonSharableData = nullptr;
    _hNonSharableSection = nullptr;
    _Debug_Generation = -1;
    _Debug_ChangeID = -1;
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

} // namespace uxtheme
