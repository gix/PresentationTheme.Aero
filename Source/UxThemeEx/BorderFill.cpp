#include "BorderFill.h"
#include "RenderObj.h"
#include <algorithm>

namespace uxtheme
{

HRESULT CBorderFill::PackProperties(CRenderObj* pRender, int fNoDraw, int iPartId, int iStateId)
{
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

HRESULT CBorderFill::GetPartSize(THEMESIZE eSize, _Out_ SIZE* psz)
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

HRESULT CBorderFill::DrawBackground(
    CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, DTBGOPTS const* pOptions)
{
    return S_OK;
}

} // namespace uxtheme
