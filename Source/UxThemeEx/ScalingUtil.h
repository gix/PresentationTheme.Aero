#pragma once
#include <windows.h>
#include <uxtheme.h>

namespace uxtheme
{
class CRenderObj;

int ScaleThemeSize(HDC hdc, _In_ CRenderObj const* pRender, int iValue);
void ScaleThemeFont(HDC hdc, _In_ CRenderObj const* pRender, _In_ LOGFONTW* plf);
void ScaleFontForScreenDpi(_In_ LOGFONTW* plf);
void ScaleMargins(MARGINS* margins, unsigned targetDpi);
} // namespace uxtheme
