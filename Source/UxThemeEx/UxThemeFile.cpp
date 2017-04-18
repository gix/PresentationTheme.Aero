#include "UxThemeFile.h"

#include "Sections.h"
#include "Utils.h"
#include <cassert>
#include <strsafe.h>
#include <winternl.h>
#include <memoryapi.h>

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
    DWORD flags = PAGE_READWRITE;
    if (fReserve)
        flags |= SEC_RESERVE;
    _hSharableSection = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, flags,
                                           0, iSharableSectionLength, nullptr);
    _pbSharableData = (THEMEHDR*)MapViewOfFile(_hSharableSection, FILE_MAP_ALL_ACCESS, 0, 0, 0);
    _cbSharableData = iSharableSectionLength;

    _hNonSharableSection = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr,
                                              flags, 0, iNonSharableSectionLength,
                                              nullptr);
    _pbNonSharableData = (BYTE*)MapViewOfFile(_hNonSharableSection, FILE_MAP_ALL_ACCESS, 0, 0, 0);
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
        UnmapViewOfFile(_pbSharableData);
    if (_hSharableSection)
        CloseHandle(_hSharableSection);
    if (_pbNonSharableData)
        UnmapViewOfFile(_pbNonSharableData);
    if (_hNonSharableSection)
        CloseHandle(_hNonSharableSection);

    _pbSharableData = nullptr;
    _hSharableSection = nullptr;
    _pbNonSharableData = nullptr;
    _hNonSharableSection = nullptr;
}

HRESULT CUxThemeFile::OpenFromHandle(HANDLE hSharableSection,
                                     HANDLE hNonSharableSection,
                                     DWORD desiredAccess, bool cleanupOnFailure)
{
    if (_pbSharableData || _pbNonSharableData)
        CloseFile();

    HRESULT hr = S_OK;

    _pbSharableData = (THEMEHDR*)MapViewOfFile(hSharableSection, desiredAccess, 0, 0, 0);
    _hSharableSection = hSharableSection;
    if (!_pbSharableData)
        hr = MakeErrorLast();

    _pbNonSharableData = (BYTE*)MapViewOfFile(hNonSharableSection, desiredAccess, 0, 0, 0);
    _hNonSharableSection = hNonSharableSection;
    if (!_pbNonSharableData)
        hr = MakeErrorLast();

    if (SUCCEEDED(hr)) {
        hr = ValidateThemeData(true);
        if (FAILED(hr)) {
            CloseFile();
            hr = 0x8007000B;
        }
    }

    if (FAILED(hr) && cleanupOnFailure) {
        _pbSharableData = nullptr;
        _hSharableSection = nullptr;
        _pbNonSharableData = nullptr;
        _hNonSharableSection = nullptr;
    }

    return hr;
}

HRESULT CUxThemeFile::ValidateThemeData(bool fullCheck) const
{
    if (!ValidateObj())
        return 0x8007054F;

    auto hdr = _pbSharableData;
    if (!hdr
        || memcmp(hdr->szSignature, "BEGINTHM", 8) != 0
        || hdr->dwVersion != 65543
        || !_pbNonSharableData
        || !(*_pbNonSharableData & 1))
        return 0x8007000B;

    return S_OK;
}

bool CUxThemeFile::ValidateObj() const
{
    return memcmp(_szHead, "thmfile\0", 8) == 0 &&
           memcmp(_szTail, "end\0", 4) == 0;
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
    *phSharableSection = nullptr;
    *phNonSharableSection = nullptr;

    PEB* peb = NtCurrentTeb()->ProcessEnvironmentBlock;
    RootSection rootSection(peb->SessionId, FILE_MAP_READ, FILE_MAP_READ);
    DataSection sharableSection(FILE_MAP_READ, FILE_MAP_READ);
    DataSection nonSharableSection(FILE_MAP_READ, FILE_MAP_READ);

    ROOTSECTION* pRootSection = nullptr;
    ENSURE_HR(rootSection.GetRootSectionData(&pRootSection));
    if (!pRootSection->szSharableSectionName[0] || !pRootSection->szNonSharableSectionName[0])
        return 0x80070490;

    ENSURE_HR(sharableSection.Open(pRootSection->szSharableSectionName, false));
    ENSURE_HR(nonSharableSection.Open(pRootSection->szNonSharableSectionName, false));
    sharableSection.DetachSectionHandle(phSharableSection);
    nonSharableSection.DetachSectionHandle(phNonSharableSection);
    return S_OK;
}

} // namespace uxtheme
