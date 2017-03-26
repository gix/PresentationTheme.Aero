#pragma once
#include "DrawBase.h"
#include <vssym32.h>
#include <windows.h>
#include <uxtheme.h>

namespace uxtheme
{

struct CRenderObj;

struct CTextDraw : CDrawBase
{
    static BOOL KeyProperty(int iPropId);
    HRESULT PackProperties(CRenderObj* pRender, int iPartId, int iStateId);
    HRESULT DrawTextW(CRenderObj* pRender, HDC hdc, int iPartId, int iStateId,
                      wchar_t const* pszText, unsigned dwCharCount,
                      unsigned dwTextFlags, RECT const* pRect,
                      DTTOPTS const* pOptions);

    int _fComposited;
    unsigned int _crText;
    unsigned int _crEdgeLight;
    unsigned int _crEdgeHighlight;
    unsigned int _crEdgeShadow;
    unsigned int _crEdgeDkShadow;
    unsigned int _crEdgeFill;
    POINT _ptShadowOffset;
    unsigned int _crShadow;
    TEXTSHADOWTYPE _eShadowType;
    int _iBorderSize;
    unsigned int _crBorder;
    unsigned int _crGlow;
    int _fApplyOverlay;
    int _iGlowSize;
    int _iGlowIntensity;
    unsigned __int16 _iFontIndex;
    int _fItalicFont;
    int _iSourcePartId;
    int _iSourceStateId;
};

} // namespace uxtheme
