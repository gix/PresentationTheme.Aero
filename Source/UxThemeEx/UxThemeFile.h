﻿#pragma once
#include "Primitives.h"
#include <windows.h>

namespace uxtheme
{

struct CUxThemeFile
{
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

    HRESULT CreateFileW(wchar_t* pszSharableSectionName,
                        unsigned cchSharableSectionName,
                        int iSharableSectionLength,
                        wchar_t* pszNonSharableSectionName,
                        unsigned cchNonSharableSectionName,
                        int iNonSharableSectionLength,
                        int fReserve);
    void CloseFile();
    HRESULT OpenFromHandle(HANDLE hSharableSection, HANDLE hNonSharableSection,
                           DWORD desiredAccess, bool cleanupOnFailure);
    HRESULT ValidateThemeData(bool fullCheck) const;
    bool ValidateObj() const;
    LOGFONTW const* GetFontByIndex(unsigned short index) const;
    static HRESULT GetGlobalTheme(HANDLE* phSharableSection, HANDLE* phNonSharableSection);

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
