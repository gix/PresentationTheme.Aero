#pragma once
#include <windows.h>

namespace uxtheme
{
struct CRenderObj;

int ScaleThemeSize(HDC hdc, _In_ CRenderObj const* pRender, int iValue);
void ScaleThemeFont(HDC hdc, _In_ CRenderObj const* pRender, _In_ LOGFONTW* plf);
void ScaleFontForScreenDpi(_In_ LOGFONTW* plf);
} // namespace uxtheme
