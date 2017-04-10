#pragma once
#include "Primitives.h"
#include "TextDraw.h"
#include "DrawBase.h"

namespace uxtheme
{
struct CUxThemeFile;

void ClearDebugInfo();

void RegisterPtr(ENTRYHDR const* hdr);
void RegisterPtr(PARTJUMPTABLEHDR const* hdr);
void RegisterPtr(STATEJUMPTABLEHDR const* hdr);
void RegisterPtr(PARTOBJHDR const* hdr);
void RegisterPtr(CTextDraw const* obj);
void RegisterPtr(CDrawBase const* obj);

void ValidatePtr(ENTRYHDR const* hdr);
void ValidatePtr(PARTJUMPTABLEHDR const* hdr);
void ValidatePtr(STATEJUMPTABLEHDR const* hdr);
void ValidatePtr(PARTOBJHDR const* hdr);
void ValidatePtr(CTextDraw const* hdr);
void ValidatePtr(CDrawBase const* hdr);

HRESULT DumpThemeToTextFile(CUxThemeFile* themeFile, wchar_t const* binPath,
                            wchar_t const* textPath);
HRESULT DumpLoadedThemeToTextFile(wchar_t const* binPath, wchar_t const* textPath);
} // namespace uxtheme
