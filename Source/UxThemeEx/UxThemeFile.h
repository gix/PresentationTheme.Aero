#pragma once
#include "Primitives.h"
#include <windows.h>

namespace uxtheme
{

class CUxThemeFile
{
public:
    CUxThemeFile();
    ~CUxThemeFile();

    CUxThemeFile(CUxThemeFile&& source) noexcept
    {
        memcpy(this, &source, sizeof(CUxThemeFile));
        memset(&source, 0, sizeof(CUxThemeFile));
    }

    CUxThemeFile& operator=(CUxThemeFile&& source) noexcept
    {
        memcpy(this, &source, sizeof(CUxThemeFile));
        memset(&source, 0, sizeof(CUxThemeFile));
        return *this;
    }

    HRESULT CreateFileW(wchar_t* pszSharableSectionName, unsigned cchSharableSectionName,
                        int iSharableSectionLength, wchar_t* pszNonSharableSectionName,
                        unsigned cchNonSharableSectionName, int iNonSharableSectionLength,
                        bool fReserve);
    void CloseFile();
    HRESULT OpenFromHandle(HANDLE hSharableSection, HANDLE hNonSharableSection,
                           DWORD desiredAccess, bool cleanupOnFailure);
    HRESULT ValidateThemeData(bool fullCheck) const;
    bool ValidateObj() const;
    LOGFONTW const* GetFontByIndex(unsigned short index) const;
    static HRESULT GetGlobalTheme(HANDLE* phSharableSection,
                                  HANDLE* phNonSharableSection);

    THEMEHDR* ThemeHeader() { return _pbSharableData; }
    THEMEHDR const* ThemeHeader() const { return _pbSharableData; }

    NONSHARABLEDATAHDR* NonSharableDataHeader()
    {
        return (NONSHARABLEDATAHDR*)_pbNonSharableData;
    }

    NONSHARABLEDATAHDR const* NonSharableDataHeader() const
    {
        return (NONSHARABLEDATAHDR*)_pbNonSharableData;
    }

    bool IsReady() const
    {
        auto header = NonSharableDataHeader();
        return header && header->dwFlags & 1;
    }

private:
    char _szHead[8];
    THEMEHDR* _pbSharableData;
    HANDLE _hSharableSection;
    BYTE* _pbNonSharableData;
    HANDLE _hNonSharableSection;
    unsigned _Debug_Generation;
    unsigned _Debug_ChangeID;
    char _szTail[4];

    int _cbSharableData = 0;
    int _cbNonSharableData = 0;
};

} // namespace uxtheme
