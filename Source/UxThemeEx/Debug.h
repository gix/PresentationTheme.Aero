#pragma once
#include "Primitives.h"
#include "TextDraw.h"
#include "DrawBase.h"

namespace uxtheme
{

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

} // namespace uxtheme
