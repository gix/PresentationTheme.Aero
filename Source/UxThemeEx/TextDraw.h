#pragma once
#include "UxThemeEx.h"

#include <vssym32.h>
#include <windows.h>
#include <uxtheme.h>

namespace uxtheme
{

class CRenderObj;

class CTextDraw
{
public:
    static BOOL KeyProperty(int iPropId);
    HRESULT PackProperties(CRenderObj* pRender, int iPartId, int iStateId);
    HRESULT GetTextExtent(CRenderObj* pRender, HDC hdc, int iPartId, int iStateId,
                          wchar_t const* _pszText, int iCharCount, unsigned dwTextFlags,
                          RECT const* pBoundingRect, RECT* pExtentRect);
    HRESULT GetTextMetricsW(CRenderObj* pRender, HDC hdc, int iPartId, int iStateId, TEXTMETRICW* ptm);
    HRESULT DrawTextW(HTHEMEFILE hThemeFile,CRenderObj* pRender, HDC hdc,
                      int iPartId, int iStateId, wchar_t const* pszText,
                      unsigned dwCharCount, unsigned dwTextFlags, RECT* pRect,
                      DTTOPTS const* pOptions);

    int _fComposited;
    COLORREF _crText;
    COLORREF _crEdgeLight;
    COLORREF _crEdgeHighlight;
    COLORREF _crEdgeShadow;
    COLORREF _crEdgeDkShadow;
    COLORREF _crEdgeFill;
    POINT _ptShadowOffset;
    unsigned _crShadow;
    TEXTSHADOWTYPE _eShadowType;
    int _iBorderSize;
    COLORREF _crBorder;
    COLORREF _crGlow;
    int _fApplyOverlay;
    int _iGlowSize;
    int _iGlowIntensity;
    unsigned short _iFontIndex;
    int _fItalicFont;
    int _iSourcePartId;
    int _iSourceStateId;
};

} // namespace uxtheme
