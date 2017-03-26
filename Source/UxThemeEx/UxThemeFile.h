#pragma once
#include "Primitives.h"
#include <windows.h>
#include <cassert>

namespace uxtheme
{

struct CUxThemeFile
{
    char _szHead[8];
    THEMEHDR* _pbSharableData;
    void* _hSharableSection;
    char* _pbNonSharableData;
    void* _hNonSharableSection;
    unsigned int _Debug_Generation;
    unsigned int _Debug_ChangeID;
    char _szTail[4];

    int _cbSharableData = 0;
    int _cbNonSharableData = 0;

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

    void CloseFile();

    HRESULT CreateFileW(wchar_t* pszSharableSectionName,
                        unsigned cchSharableSectionName,
                        int iSharableSectionLength,
                        wchar_t* pszNonSharableSectionName,
                        unsigned cchNonSharableSectionName,
                        int iNonSharableSectionLength,
                        int fReserve);
};

} // namespace uxtheme
