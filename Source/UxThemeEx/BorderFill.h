#pragma once
#include "DrawBase.h"
#include <windows.h>
#include <uxtheme.h>
#include <vssym32.h>

namespace uxtheme
{

class CRenderObj;

class CBorderFill : public CDrawBase
{
public:
    static bool KeyProperty(int iPropId);
    HRESULT PackProperties(CRenderObj* pRender, bool fNoDraw, int iPartId, int iStateId);
    HRESULT GetPartSize(THEMESIZE eSize, _Out_ SIZE* psz) const;

    bool IsBackgroundPartiallyTransparent() const
    {
        return _eBorderType != 0 || _fNoDraw;
    }

    HRESULT GetBackgroundRegion(CRenderObj* pRender, RECT const* pRect, HRGN* pRegion);
    HRESULT GetBackgroundExtent(CRenderObj* pRender, RECT const* pContentRect, RECT* pExtentRect);
    HRESULT DrawBackground(CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, DTBGOPTS const* pOptions);
    HRESULT DrawComplexBackground(CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, BOOL fGettingRegion, BOOL fBorder, BOOL fContent, RECT const* pClipRect);
    HRESULT HitTestBackground(
        CRenderObj* pRender, int iStateId, DWORD dwHTFlags, RECT const* pRect,
        HRGN hrgn, POINT ptTest, WORD* pwHitCode);

    BOOL _fNoDraw;
    BORDERTYPE _eBorderType;
    COLORREF _crBorder;
    int _iBorderSize;
    int _iRoundCornerWidth;
    int _iRoundCornerHeight;
    FILLTYPE _eFillType;
    COLORREF _crFill;
    int _iDibOffset;
    MARGINS _ContentMargins;
    int _iGradientPartCount;
    COLORREF _crGradientColors[5];
    int _iGradientRatios[5];
    int _iSourcePartId;
    int _iSourceStateId;
};

} // namespace uxtheme
