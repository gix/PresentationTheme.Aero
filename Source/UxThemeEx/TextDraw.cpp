#include "TextDraw.h"

#include "RenderObj.h"
#include "ScalingUtil.h"
#include "Utils.h"
#include "UxThemeEx.h"
#include <vssym32.h>

namespace uxtheme
{

static HRESULT DrawTextWithGlow(
    HDC hdcMem, wchar_t const* pszText, unsigned cch, RECT* prc,
    DWORD dwFlags, COLORREF crText, COLORREF crGlow, unsigned nGlowRadius,
    unsigned nGlowIntensity, BOOL fPreMultiply,
    DTT_CALLBACK_PROC pfnDrawTextCallback, LPARAM lParam)
{
    return S_OK;
}

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
    static_assert(std::is_trivially_copyable_v<CTextDraw>);
    memset(this, 0, sizeof(CTextDraw));

    _iSourcePartId = iPartId;
    _iSourceStateId = iStateId;

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TEXTCOLOR, (int *)&_crText) < 0)
        _crText = 0;

    if (pRender->ExternalGetPosition(iPartId, iStateId, TMT_TEXTSHADOWOFFSET, &_ptShadowOffset) >= 0) {
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

    pRender->GetFontTableIndex(iPartId, iStateId, TMT_FONT, &_iFontIndex);
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

HRESULT CTextDraw::GetTextExtent(
    CRenderObj* pRender, HDC hdc, int iPartId, int iStateId,
    wchar_t const* _pszText, int iCharCount, unsigned dwTextFlags,
    RECT const* pBoundingRect, RECT* pExtentRect)
{
    HFONT font = nullptr;
    HFONT oldFont = nullptr;

    HRESULT hr = S_OK;
    if (_iFontIndex) {
        hr = pRender->GetScaledFontHandle(hdc, _iFontIndex, &font);
        if (hr >= 0)
            oldFont = (HFONT)SelectObject(hdc, font);
    }

    if (hr >= 0) {
        RECT rc = {};
        if (pBoundingRect)
            rc = *pBoundingRect;
        else
            SetRect(&rc, 0, 0, 0, 0);

        auto flags = dwTextFlags & 0xFFFEFFFF | DT_CALCRECT;
        if (!_fComposited) {
            if (!DrawTextExW(hdc, (LPWSTR)_pszText, iCharCount, &rc, flags, nullptr))
                hr = MakeErrorLast();
        } else {
            hr = DrawTextWithGlow(
                hdc,
                _pszText,
                iCharCount,
                &rc,
                flags,
                _crText,
                _crGlow,
                _iGlowSize,
                _iGlowIntensity,
                1,
                nullptr,
                0);
        }

        if (hr >= 0)
            *pExtentRect = rc;
    }

    if (oldFont)
        SelectObject(hdc, oldFont);
    if (font && !pRender->_fCacheEnabled)
        DeleteObject(font);

    return hr;
}

HRESULT CTextDraw::GetTextMetricsW(CRenderObj* pRender, HDC hdc, int iPartId,
                                   int iStateId, TEXTMETRICW* ptm)
{
    if (!ptm)
        return E_INVALIDARG;

    HRESULT hr = S_OK;
    HFONT oldFont = nullptr;
    HFONT font = nullptr;
    if (_iFontIndex) {
        hr = pRender->GetScaledFontHandle(hdc, _iFontIndex, &font);
        if (hr >= 0)
            oldFont = (HFONT)SelectObject(hdc, font);
    }

    if (hr >= 0) {
        if (!::GetTextMetricsW(hdc, ptm))
            hr = MakeErrorLast();
        if (oldFont)
            SelectObject(hdc, oldFont);
    }

    if (font && !pRender->_fCacheEnabled)
        DeleteObject(font);

    return hr;
}

HRESULT CTextDraw::DrawTextW(
    HTHEMEFILE hThemeFile, CRenderObj* pRender, HDC hdc, int iPartId,
    int iStateId, wchar_t const* pszText, unsigned dwCharCount,
    unsigned dwTextFlags, RECT* pRect, DTTOPTS const* pOptions)
{
    int isComposited;
    COLORREF oldTextColor;
    int hasChangedTextColor;
    POINT shadowOffset;
    int hasShadowType;
    HRESULT hr;
    HFONT v26;
    unsigned flags;
    HPEN pen46;
    HBRUSH brush;
    HPEN oldPen;
    COLORREF color;
    COLORREF crText;
    HFONT ho;
    int hasFontProp;
    unsigned glowSize;
    DWORD eShadowType;
    COLORREF crShadow;
    int applyOverlay;
    HPEN hhh;
    int hasCallback;
    int borderSize;
    COLORREF crBorder;
    HGDIOBJ oldFont;
    int oldBkMode;
    RECT rc = {};
    LOGFONTW userFont = {};
    unsigned dwTextFlagsa;

    color = 0;
    ho = 0i64;
    hhh = 0i64;
    oldTextColor = 0;
    oldFont = 0i64;
    hasChangedTextColor = 0;
    oldBkMode = SetBkMode(hdc, 1);

    shadowOffset = _ptShadowOffset;
    crText = _crText;
    crBorder = _crBorder;
    crShadow = _crShadow;
    eShadowType = _eShadowType;
    borderSize = _iBorderSize;
    applyOverlay = _fApplyOverlay;
    glowSize = _iGlowSize;

    hasFontProp = 0;
    isComposited = 0;
    hasCallback = 0;
    dwTextFlagsa = dwTextFlags & 0xFFFEFFFF;

    if (pOptions) {
        flags = pOptions->dwFlags;
        if (flags & DTT_TEXTCOLOR)
            crText = pOptions->crText;
        if (flags & DTT_BORDERCOLOR)
            crBorder = pOptions->crBorder;
        if (flags & DTT_SHADOWCOLOR)
            crShadow = pOptions->crShadow;
        if (flags & DTT_SHADOWTYPE)
            eShadowType = pOptions->iTextShadowType;
        if (flags & DTT_SHADOWOFFSET)
            shadowOffset = pOptions->ptShadowOffset;
        if (flags & DTT_BORDERSIZE)
            borderSize = pOptions->iBorderSize;
        if (flags & DTT_FONTPROP) {
            pRender->ExternalGetFont(nullptr, iPartId, iStateId,
                                     pOptions->iFontPropId, 0, &userFont);
            hasFontProp = 1;
        }
        if (flags & DTT_COLORPROP) {
            pRender->ExternalGetInt(iPartId, iStateId, pOptions->iColorPropId,
                                    reinterpret_cast<int*>(&crText));
        }
        if (flags & DTT_APPLYOVERLAY)
            applyOverlay = pOptions->fApplyOverlay;
        if (flags & DTT_GLOWSIZE)
            glowSize = pOptions->iGlowSize;
        if (flags & DTT_CALLBACK)
            hasCallback = 1;
        if (flags & DTT_COMPOSITED)
            isComposited = 1;
    }

    hasShadowType = 0;
    if (eShadowType != TST_NONE)
        hasShadowType = 1;

    if (hasFontProp) {
        ScaleThemeFont(hdc, pRender, &userFont);
        ho = CreateFontIndirectW(&userFont);
        oldFont = SelectObject(hdc, ho);
    } else if (_iFontIndex != 0) {
        hr = pRender->GetScaledFontHandle(hdc, _iFontIndex, (HFONT*)&hhh);
        if (hr < 0) {
            v26 = (HFONT)hhh;
            goto exit_17;
        }

        ho = (HFONT)hhh;
        oldFont = SelectObject(hdc, hhh);
    } else if (_fItalicFont) {
        oldFont = GetCurrentObject(hdc, OBJ_FONT);
        LOGFONT lf;
        if (GetObjectW(oldFont, sizeof(lf), &lf) == sizeof(lf)) {
            lf.lfItalic = 1;
            HFONT h4;
            if (pRender->GetCachedDisplayFontHandle(hdc, lf, &h4) >= 0) {
                ho = h4;
            } else {
                ho = CreateFontIndirectW(&lf);
                hasFontProp = 1;
            }
            if (ho)
                SelectObject(hdc, ho);
            hhh = (HPEN)h4;
        }
    }

    if (_fComposited || isComposited) {
        if (dwTextFlagsa & 0x400) {
            isComposited = 1;
        } else {
            auto v30 = (HBITMAP)GetCurrentObject(hdc, OBJ_BITMAP);
            DIBSECTION ds;
            isComposited = 0;
            if (v30 && GetObjectW(v30, sizeof(ds), &ds)
                && ds.dsBm.bmBitsPixel == 32 && ds.dsBm.bmBits)
                isComposited = 1;
        }
    }

    if (!isComposited) {
        if (glowSize) {
            HTHEME themeTextGlow = UxOpenThemeData(hThemeFile, nullptr, L"TEXTGLOW");
            if (themeTextGlow) {
                rc = *pRect;
                if (DrawTextExW(hdc, (LPWSTR)pszText, dwCharCount, &rc, dwTextFlagsa | 0x400, nullptr)) {
                    long width = rc.right - rc.left;
                    long x;
                    if (dwTextFlagsa & 2) {
                        x = pRect->right - width;
                    } else if (dwTextFlagsa & 1) {
                        x = pRect->left + ((unsigned)(pRect->right - pRect->left - width) >> 1);
                    } else {
                        x = pRect->left;
                    }

                    long height = rc.bottom - rc.top;
                    long y;
                    if (dwTextFlagsa & 8) {
                        y = pRect->bottom - height;
                    } else if (dwTextFlagsa & 4) {
                        y = pRect->top + ((unsigned)(pRect->bottom - pRect->top - height) >> 1);
                    } else {
                        y = pRect->top;
                    }

                    rc.left = x;
                    rc.right = x + width;
                    rc.top = y;
                    rc.bottom = y + height;
                    InflateRect(&rc, glowSize, glowSize);
                    UxDrawThemeBackground(hThemeFile, themeTextGlow, hdc, 1, 0, &rc, nullptr);
                    UxCloseThemeData(hThemeFile, themeTextGlow);
                }
            }
        }

        if (hasShadowType) {
            if (eShadowType == 2) {
                rc = *pRect;
                DrawShadowText(hdc, pszText, dwCharCount, &rc, dwTextFlagsa,
                               crText, crShadow, shadowOffset.x, shadowOffset.y);
                goto LABEL_111;
            } else {
                COLORREF oldTextColor2 = SetTextColor(hdc, crShadow);
                hasChangedTextColor = 1;
                color = oldTextColor2;
                rc.left = shadowOffset.x + pRect->left;
                rc.top = shadowOffset.y + pRect->top;
                rc.right = shadowOffset.x + pRect->right;
                rc.bottom = pRect->bottom;
                if (!DrawTextExW(hdc, (LPWSTR)pszText, dwCharCount, &rc, dwTextFlagsa, nullptr)) {
                    oldTextColor = color;
                    hr = MakeErrorLast();
                    v26 = ho;
                    goto exit_17;
                }

                goto LABEL_14;
            }
        } else {
            goto LABEL_14;

        LABEL_14:
            rc = *pRect;
            if (!borderSize) {
                goto LABEL_16;
            }

            if (!BeginPath(hdc)) {
                oldTextColor = color;
                hr = MakeErrorLast();
                v26 = ho;
                goto exit_17;
            }

            LOGBRUSH logBrush = {};
            logBrush.lbColor = crBorder;

            pen46 = ExtCreatePen(PS_GEOMETRIC, borderSize, &logBrush, 0, nullptr);

            brush = CreateSolidBrush(applyOverlay ? crBorder : crText);
            if (!pen46) {
                AbortPath(hdc);
                oldTextColor = color;
                hr = MakeErrorLast();
                v26 = ho;
                goto exit_17;
            }

            oldPen = (HPEN)SelectObject(hdc, pen46);
            hhh = oldPen;
            if (brush) {
                auto v72_ = SelectObject(hdc, brush);
                if (!DrawTextExW(hdc, (LPWSTR)pszText, dwCharCount, &rc,
                                 0, nullptr)) {
                    AbortPath(hdc);
                    oldTextColor = color;
                    hr = MakeErrorLast();
                    v26 = ho;
                    goto exit_17;
                }

                EndPath(hdc);
                StrokeAndFillPath(hdc);
                SelectObject(hdc, v72_);
                DeleteObject(brush);
            }

            SelectObject(hdc, oldPen);
            DeleteObject(pen46);
            goto LABEL_111;
        }

    LABEL_111:
        if (applyOverlay) {
        LABEL_16:
            if (hasChangedTextColor) {
                SetTextColor(hdc, crText);
            } else {
                color = SetTextColor(hdc, crText);
                hasChangedTextColor = 1;
            }

            hr = 0;
            if (!DrawTextExW(hdc, (wchar_t*)pszText, dwCharCount, &rc,
                             dwTextFlagsa, nullptr))
                hr = MakeErrorLast();
        } else {
            hr = 0;
        }

        oldTextColor = color;
        v26 = ho;
    } else {
        DTT_CALLBACK_PROC const callback =
            hasCallback ? pOptions->pfnDrawTextCallback : nullptr;
        LPARAM const callbackParam = hasCallback ? pOptions->lParam : 0;

        rc = *pRect;
        hr = DrawTextWithGlow(
            hdc,
            pszText,
            dwCharCount,
            &rc,
            dwTextFlagsa,
            crText,
            _crGlow,
            glowSize,
            _iGlowIntensity,
            TRUE,
            callback,
            callbackParam);

        if (hr >= 0) {
            oldTextColor = color;
            hr = 0;
        } else {
            oldTextColor = 0;
        }
        v26 = ho;
    }

exit_17:
    SetBkMode(hdc, oldBkMode);
    if (hasChangedTextColor)
        SetTextColor(hdc, oldTextColor);
    if (oldFont)
        SelectObject(hdc, oldFont);
    if (v26 && (hasFontProp || !pRender->_fCacheEnabled))
        DeleteObject(v26);

    if (FAILED(hr))
        return hr;

    if (pOptions && pOptions->dwFlags & DTT_CALCRECT && pRect)
        CopyRect(pRect, &rc);

    return hr;
}

} // namespace uxtheme
