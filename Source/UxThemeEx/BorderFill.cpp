#include "BorderFill.h"
#include "RenderObj.h"
#include "DrawHelp.h"
#include <algorithm>

namespace uxtheme
{

bool CBorderFill::KeyProperty(int iPropId)
{
    switch (iPropId) {
    case TMT_BORDERSIZE:
    case TMT_ROUNDCORNERWIDTH:
    case TMT_ROUNDCORNERHEIGHT:
    case TMT_GRADIENTRATIO1:
    case TMT_GRADIENTRATIO2:
    case TMT_GRADIENTRATIO3:
    case TMT_GRADIENTRATIO4:
    case TMT_GRADIENTRATIO5:
    case TMT_CONTENTMARGINS:
    case TMT_BORDERCOLOR:
    case TMT_FILLCOLOR:
    case TMT_GRADIENTCOLOR1:
    case TMT_GRADIENTCOLOR2:
    case TMT_GRADIENTCOLOR3:
    case TMT_GRADIENTCOLOR4:
    case TMT_GRADIENTCOLOR5:
    case TMT_BGTYPE:
    case TMT_BORDERTYPE:
    case TMT_FILLTYPE:
        return true;

    default:
        return false;
    }
}

HRESULT CBorderFill::PackProperties(CRenderObj* pRender, bool fNoDraw, int iPartId, int iStateId)
{
    static_assert(std::is_trivially_copyable_v<CBorderFill>);
    memset(this, 0, sizeof(CBorderFill));

    _iSourcePartId = iPartId;
    _eBgType = BT_BORDERFILL;
    _iSourceStateId = iStateId;

    if (fNoDraw) {
        _fNoDraw = TRUE;
        return S_OK;
    }

    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_BORDERTYPE, (int*)&_eBorderType) < 0)
        _eBorderType = BT_RECT;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_BORDERCOLOR, (int*)&_crBorder) < 0)
        _crBorder = 0;

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_BORDERSIZE, &_iBorderSize) < 0)
        _iBorderSize = 1;

    if (_eBorderType == BT_ROUNDRECT) {
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_ROUNDCORNERWIDTH, &_iRoundCornerWidth) < 0)
            _iRoundCornerWidth = 80;
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_ROUNDCORNERHEIGHT, &_iRoundCornerHeight) < 0)
            _iRoundCornerHeight = 80;
    }

    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_FILLTYPE, (int*)&_eFillType) < 0)
        _eFillType = FT_SOLID;

    if (_eFillType == FT_SOLID) {
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_FILLCOLOR, (int*)&_crFill) < 0) {
            _crFill = 0xFFFFFF;
        }
    } else if (_eFillType == FT_TILEIMAGE) {
        _iDibOffset = pRender->GetValueIndex(iPartId, iStateId, TMT_DIBDATA);
        if (_iDibOffset == -1)
            _iDibOffset = 0;
    } else {
        _iGradientPartCount = 0;
        for (int prop = TMT_GRADIENTRATIO1; prop < TMT_GRADIENTRATIO5; ++prop) {
            int gradientColor;
            if (pRender->ExternalGetInt(iPartId, iStateId, prop + 1404, &gradientColor) < 0)
                break;

            int gradientRatio;
            if (pRender->ExternalGetInt(iPartId, iStateId, prop, &gradientRatio) < 0)
                gradientRatio = 0;

            _crGradientColors[_iGradientPartCount] = gradientColor;
            _iGradientRatios[_iGradientPartCount++] = gradientRatio;
        }
    }

    if (pRender->ExternalGetMargins(
        nullptr, iPartId, iStateId, TMT_CONTENTMARGINS, nullptr, &_ContentMargins) < 0) {
        _ContentMargins.cxLeftWidth = _iBorderSize;
        _ContentMargins.cxRightWidth = _iBorderSize;
        _ContentMargins.cyTopHeight = _iBorderSize;
        _ContentMargins.cyBottomHeight = _iBorderSize;
    }

    return S_OK;
}

HRESULT CBorderFill::GetPartSize(THEMESIZE eSize, _Out_ SIZE* psz) const
{
    if (eSize == TS_MIN) {
        psz->cx = std::max(1, 2 * _iBorderSize);
        psz->cy = std::max(1, 2 * _iBorderSize);
    } else if (eSize == TS_TRUE) {
        psz->cx = 2 * _iBorderSize + 1;
        psz->cy = 2 * _iBorderSize + 1;
    } else {
        return E_INVALIDARG;
    }

    return S_OK;
}

HRESULT CBorderFill::GetBackgroundRegion(CRenderObj* pRender, RECT const* pRect,
                                         HRGN* pRegion)
{
    if (!IsBackgroundPartiallyTransparent()) {
        HRGN hrgn = CreateRectRgn(pRect->left, pRect->top, pRect->right, pRect->bottom);
        if (!hrgn)
            return MakeErrorLast();

        *pRegion = hrgn;
        return S_OK;
    }

    MemoryDC hdcMemory;
    ENSURE_HR(hdcMemory.OpenDC(nullptr, pRect->right - pRect->left,
                               pRect->bottom - pRect->top));

    if (!BeginPath(hdcMemory.Get()))
        return MakeErrorLast();

    DTBGOPTS opts;
    opts.dwSize = sizeof(opts);
    opts.dwFlags = DTBG_COMPUTINGREGION;
    opts.rcClip = {};

    ENSURE_HR(DrawBackground(pRender, hdcMemory.Get(), pRect, &opts));
    if (!EndPath(hdcMemory.Get()))
        return MakeErrorLast();

    HRGN hrgn = PathToRegion(hdcMemory.Get());
    if (!hrgn)
        return MakeErrorLast();

    *pRegion = hrgn;
    return S_OK;
}

HRESULT CBorderFill::GetBackgroundExtent(
    CRenderObj* pRender, RECT const* pContentRect, RECT* pExtentRect)
{
    pExtentRect->left = pContentRect->left - _ContentMargins.cxLeftWidth;
    pExtentRect->top = pContentRect->top - _ContentMargins.cyTopHeight;
    pExtentRect->right = pContentRect->right + _ContentMargins.cxRightWidth;
    pExtentRect->bottom = pContentRect->bottom + _ContentMargins.cyBottomHeight;
    return S_OK;
}

HRESULT CBorderFill::DrawBackground(
    CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, DTBGOPTS const* pOptions)
{
    RECT const* pClipRect = nullptr;
    BOOL fGettingRegion = FALSE;
    BOOL fBorder = TRUE;
    BOOL fContent = TRUE;
    if (pOptions) {
        if (pOptions->dwFlags & DTBG_CLIPRECT)
            pClipRect = &pOptions->rcClip;
        if (pOptions->dwFlags & DTBG_OMITBORDER)
            fBorder = FALSE;
        if (pOptions->dwFlags & DTBG_OMITCONTENT)
            fContent = FALSE;
        if (pOptions->dwFlags & DTBG_COMPUTINGREGION)
            fGettingRegion = DTBG_CLIPRECT;
    }

    if (_fNoDraw)
        return S_OK;

    if (_eFillType != FT_SOLID || _eBorderType != BT_RECT)
        return DrawComplexBackground(
            pRender,
            hdcOrig,
            pRect,
            fGettingRegion,
            fBorder,
            fContent,
            pClipRect);

    if (_iBorderSize == 0) {
        if (!fContent)
            return S_OK;
        RECT rc = *pRect;
        if (pClipRect)
            IntersectRect(&rc, &rc, pClipRect);
        COLORREF const oldBkColor = SetBkColor(hdcOrig, _crFill);
        ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);
        SetBkColor(hdcOrig, oldBkColor);
    } else {
        COLORREF const oldBkColor = GetBkColor(hdcOrig);
        if (fBorder) {
            RECT rc;
            SetBkColor(hdcOrig, _crBorder);
            SetRect(&rc, pRect->left, pRect->top, pRect->left + _iBorderSize,
                    pRect->bottom);
            if (pClipRect)
                IntersectRect(&rc, &rc, pClipRect);
            ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);

            SetRect(&rc, pRect->right - _iBorderSize, pRect->top,
                    pRect->right, pRect->bottom);
            if (pClipRect)
                IntersectRect(&rc, &rc, pClipRect);
            ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);

            SetRect(&rc, pRect->left, pRect->top, pRect->right,
                    pRect->top + _iBorderSize);
            if (pClipRect)
                IntersectRect(&rc, &rc, pClipRect);
            ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);

            SetRect(&rc, pRect->left, pRect->bottom - _iBorderSize,
                    pRect->right, pRect->bottom);
            if (pClipRect)
                IntersectRect(&rc, &rc, pClipRect);
            ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);
        }

        if (fContent) {
            RECT rc = *pRect;
            rc.right -= _iBorderSize;
            rc.top += _iBorderSize;
            rc.bottom -= _iBorderSize;
            rc.left += _iBorderSize;
            if (pClipRect)
                IntersectRect(&rc, &rc, pClipRect);
            SetBkColor(hdcOrig, _crFill);
            ExtTextOutW(hdcOrig, 0, 0, ETO_OPAQUE, &rc, nullptr, 0, nullptr);
        }

        SetBkColor(hdcOrig, oldBkColor);
    }

    return S_OK;
}

HRESULT CBorderFill::DrawComplexBackground(
    CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, BOOL fGettingRegion,
    BOOL fBorder, BOOL fContent, RECT const* pClipRect)
{
    return E_NOTIMPL;
}

HRESULT CBorderFill::HitTestBackground(
    CRenderObj* pRender, int iStateId, DWORD dwHTFlags, RECT const* pRect,
    HRGN hrgn, POINT ptTest, WORD* pwHitCode)
{
    *pwHitCode = HitTestRect(dwHTFlags, pRect, &_ContentMargins, &ptTest);
    return S_OK;
}

} // namespace uxtheme
