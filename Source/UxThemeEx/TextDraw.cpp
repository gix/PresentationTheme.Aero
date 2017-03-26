#include "TextDraw.h"

#include "RenderObj.h"
#include <vssym32.h>

namespace uxtheme
{

BOOL CTextDraw::KeyProperty(int iPropId)
{
    switch (iPropId) {
    case TMT_FONT:
    case TMT_HEADING1FONT:
    case TMT_HEADING2FONT:
    case TMT_BODYFONT:
    case TMT_TEXTAPPLYOVERLAY:
    case TMT_TEXTGLOW:
    case TMT_TEXTBORDERSIZE:
    case TMT_TEXTGLOWSIZE:
    case TMT_TEXTSHADOWOFFSET:
    case TMT_TEXTCOLOR:
    case TMT_EDGELIGHTCOLOR:
    case TMT_EDGEHIGHLIGHTCOLOR:
    case TMT_EDGESHADOWCOLOR:
    case TMT_EDGEDKSHADOWCOLOR:
    case TMT_EDGEFILLCOLOR:
    case TMT_TEXTBORDERCOLOR:
    case TMT_TEXTSHADOWCOLOR:
    case TMT_HEADING1TEXTCOLOR:
    case TMT_HEADING2TEXTCOLOR:
    case TMT_BODYTEXTCOLOR:
    case TMT_TEXTSHADOWTYPE:
        return TRUE;

    default:
        return FALSE;
    }
}

HRESULT CTextDraw::PackProperties(CRenderObj* pRender, int iPartId, int iStateId)
{
    memset(this, 0, sizeof(CTextDraw));

    _iSourcePartId = iPartId;
    _iSourceStateId = iStateId;

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTCOLOR, (int *)&_crText) < 0)
        _crText = 0;

    if (pRender->ExternalGetPosition(iPartId, iStateId, 3402, &_ptShadowOffset) >= 0) {
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTSHADOWCOLOR, (int *)&_crShadow) < 0)
            _crShadow = 0;
        if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_TEXTSHADOWTYPE, (int *)&_eShadowType) < 0)
            _eShadowType = TST_NONE;
    }

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTBORDERSIZE, &_iBorderSize) < 0)
        _iBorderSize = 0;
    else {
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTBORDERCOLOR, (int *)&_crBorder) < 0)
            _crBorder = 0;
        if (pRender->ExternalGetBool(iPartId, iStateId, TMT_TEXTAPPLYOVERLAY, &_fApplyOverlay) < 0)
            _fApplyOverlay = 0;
    }

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTGLOWSIZE, &_iGlowSize) < 0)
        _iGlowSize = 0;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLOWINTENSITY, &_iGlowIntensity) < 0)
        _iGlowIntensity = 0;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLOWCOLOR, (int *)&_crGlow) < 0)
        _crGlow = 0xFFFFFF;

    pRender->GetFontTableIndex(iPartId, iStateId, 210, &_iFontIndex);
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_TEXTITALIC, &_fItalicFont) < 0)
        _fItalicFont = 0;

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_EDGELIGHTCOLOR, (int *)&_crEdgeLight) < 0)
        _crEdgeLight = 0xC0C0C0;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_EDGEHIGHLIGHTCOLOR, (int *)&_crEdgeHighlight) < 0)
        _crEdgeHighlight = 0xFFFFFF;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_EDGESHADOWCOLOR, (int *)&_crEdgeShadow) < 0)
        _crEdgeShadow = 0x808080;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_EDGEDKSHADOWCOLOR, (int *)&_crEdgeDkShadow) < 0)
        _crEdgeDkShadow = 0;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_EDGEFILLCOLOR, (int *)&_crEdgeFill) < 0)
        _crEdgeFill = _crEdgeLight;
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_COMPOSITED, &_fComposited) < 0)
        _fComposited = 0;

    if (!_fComposited) {
        int textGlow;
        if (pRender->ExternalGetBool(iPartId, iStateId, TMT_TEXTGLOW, &textGlow) < 0 || !textGlow)
            _iGlowSize = 0;
    }

    return S_OK;
}

HRESULT CTextDraw::DrawTextW(
    CRenderObj* pRender, HDC hdc, int iPartId, int iStateId,
    wchar_t const* pszText, unsigned dwCharCount, unsigned dwTextFlags,
    RECT const* pRect, DTTOPTS const* pOptions)
{
    return S_OK;
}

} // namespace uxtheme
