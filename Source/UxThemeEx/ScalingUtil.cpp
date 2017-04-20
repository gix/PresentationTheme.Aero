#include "ScalingUtil.h"

#include "DpiInfo.h"
#include "RenderObj.h"

namespace uxtheme
{

int ScaleThemeSize(HDC hdc, _In_ CRenderObj const* pRender, int iValue)
{
    int dpi;
    if (!hdc || (pRender->IsStronglyAssociatedDpi() && IsScreenDC(hdc)))
        dpi = pRender->GetAssociatedDpi();
    else
        dpi = GetDeviceCaps(hdc, LOGPIXELSX);

    return MulDiv(iValue, dpi, 96);
}

void ScaleThemeFont(HDC hdc, _In_ CRenderObj const* pRender, _In_ LOGFONTW* plf)
{
    if (plf->lfHeight < 0)
        plf->lfHeight = ScaleThemeSize(hdc, pRender, plf->lfHeight);
}

void ScaleFontForScreenDpi(_In_ LOGFONTW* plf)
{
    if (plf->lfHeight < 0)
        plf->lfHeight = MulDiv(plf->lfHeight, GetScreenDpi(), 96);
}

void ScaleMargins(MARGINS* margins, unsigned targetDpi)
{
    if (!margins)
        return;

    margins->cxLeftWidth = MulDiv(margins->cxLeftWidth, targetDpi, 96);
    margins->cxRightWidth = MulDiv(margins->cxRightWidth, targetDpi, 96);
    margins->cyTopHeight = MulDiv(margins->cyTopHeight, targetDpi, 96);
    margins->cyBottomHeight = MulDiv(margins->cyBottomHeight, targetDpi, 96);
}

} // namespace uxtheme
