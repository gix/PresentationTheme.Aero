#pragma once
#include "DrawBase.h"
#include <windows.h>
#include <uxtheme.h>
#include <vssym32.h>

namespace uxtheme
{

struct CRenderObj;

struct CBorderFill : CDrawBase
{
    HRESULT PackProperties(CRenderObj* pRender, int fNoDraw, int iPartId, int iStateId);
    HRESULT GetPartSize(THEMESIZE eSize, _Out_ SIZE* psz);
    HRESULT DrawBackground(CRenderObj* pRender, HDC hdcOrig, RECT const* pRect, DTBGOPTS const* pOptions);

    int _fNoDraw;
    BORDERTYPE _eBorderType;
    unsigned int _crBorder;
    int _iBorderSize;
    int _iRoundCornerWidth;
    int _iRoundCornerHeight;
    FILLTYPE _eFillType;
    unsigned int _crFill;
    int _iDibOffset;
    MARGINS _ContentMargins;
    int _iGradientPartCount;
    unsigned int _crGradientColors[5];
    int _iGradientRatios[5];
    int _iSourcePartId;
    int _iSourceStateId;
};

} // namespace uxtheme
