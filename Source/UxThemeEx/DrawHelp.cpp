#include "DrawHelp.h"

namespace uxtheme
{

static WORD _HitTestRectLeft(RECT const* prc, int cxMargin, int cyMargin,
                             POINT const* pt, WORD wMiss)
{
    if (pt->x <= prc->left + cxMargin)
        return HTLEFT;
    return wMiss;
}

static WORD _HitTestRectTop(RECT const* prc, int cxMargin, int cyMargin,
                            POINT const* pt, WORD wMiss)
{
    if (pt->y <= prc->top + cyMargin)
        return HTTOP;
    return wMiss;
}

static WORD _HitTestRectRight(RECT const* prc, int cxMargin, int cyMargin,
                              POINT const* pt, WORD wMiss)
{
    if (pt->x >= prc->right - cxMargin)
        return HTRIGHT;
    return wMiss;
}

static WORD _HitTestRectBottom(RECT const* prc, int cxMargin, int cyMargin,
                               POINT const* pt, WORD wMiss)
{
    if (pt->y >= prc->bottom - cyMargin)
        return HTBOTTOM;
    return wMiss;
}

using HITTESTRECTPROC = WORD(RECT const* prc, int cxMargin, int cyMargin, POINT const* pt, WORD wMiss);

static WORD _HitTestRectCorner(
    HITTESTRECTPROC pfnX, HITTESTRECTPROC pfnY, RECT const* prc, int cxMargin,
    int cyMargin, int cxMargin2, int cyMargin2, POINT const* pt, WORD wHitC,
    WORD wHitX, WORD wHitY, WORD wMiss)
{
    WORD retX = pfnX(prc, cxMargin, cyMargin, pt, wMiss);
    WORD retY = pfnY(prc, cxMargin, cyMargin, pt, wMiss);

    if (retX != wMiss && retY != wMiss)
        return wHitC;

    if (retX != wMiss) {
        if (wHitX == pfnX(prc, cxMargin2, cyMargin2, pt, wHitX))
            return wHitX;
        return wHitC;
    }

    if (retY != wMiss) {
        if (wHitY == pfnY(prc, cxMargin2, cyMargin2, pt, wHitY))
            return wHitY;
        return wHitC;
    }

    return wMiss;
}

static WORD _HitTestRectTopLeft(RECT const* prc, int cxMargin, int cyMargin,
                                POINT const* pt, WORD wMiss)
{
    return _HitTestRectCorner(
        _HitTestRectLeft,
        _HitTestRectTop,
        prc,
        cxMargin,
        cyMargin,
        GetSystemMetrics(SM_CXVSCROLL),
        GetSystemMetrics(SM_CYHSCROLL),
        pt,
        HTTOPLEFT,
        HTLEFT,
        HTTOP,
        wMiss);
}

static WORD _HitTestRectTopRight(RECT const* prc, int cxMargin, int cyMargin,
                                 POINT const* pt, WORD wMiss)
{
    return _HitTestRectCorner(
        _HitTestRectRight,
        _HitTestRectTop,
        prc,
        cxMargin,
        cyMargin,
        GetSystemMetrics(SM_CXVSCROLL),
        GetSystemMetrics(SM_CYHSCROLL),
        pt,
        HTTOPRIGHT,
        HTRIGHT,
        HTTOP,
        wMiss);
}

static WORD _HitTestRectBottomLeft(RECT const* prc, int cxMargin, int cyMargin,
                                   POINT const* pt, WORD wMiss)
{
    return _HitTestRectCorner(
        _HitTestRectLeft,
        _HitTestRectBottom,
        prc,
        cxMargin,
        cyMargin,
        GetSystemMetrics(SM_CXVSCROLL),
        GetSystemMetrics(SM_CYHSCROLL),
        pt,
        HTBOTTOMLEFT,
        HTLEFT,
        HTBOTTOM,
        wMiss);
}

static WORD _HitTestRectBottomRight(RECT const* prc, int cxMargin, int cyMargin,
                                    POINT const* pt, WORD wMiss)
{
    return _HitTestRectCorner(
        _HitTestRectRight,
        _HitTestRectBottom,
        prc,
        cxMargin,
        cyMargin,
        GetSystemMetrics(SM_CXVSCROLL),
        GetSystemMetrics(SM_CYHSCROLL),
        pt,
        HTBOTTOMRIGHT,
        HTRIGHT,
        HTBOTTOM,
        wMiss);
}

static WORD _HitTestResizingRect(
    DWORD dwHTFlags, RECT const* prc, POINT const* pt, WORD w9GridHit, WORD wMiss)
{
    WORD hit = wMiss;
    bool const caption = dwHTFlags & HTTB_CAPTION;
    bool const rbTop = dwHTFlags & HTTB_RESIZINGBORDER_TOP;
    bool const rbLeft = dwHTFlags & HTTB_RESIZINGBORDER_LEFT;
    bool const rbBottom = dwHTFlags & HTTB_RESIZINGBORDER_BOTTOM;
    bool const rbRight = dwHTFlags & HTTB_RESIZINGBORDER_RIGHT;

    int const cxEdge = GetSystemMetrics(SM_CXEDGE);
    int const cyEdge = GetSystemMetrics(SM_CYEDGE);
    int const cxSizeFrame = GetSystemMetrics(SM_CXSIZEFRAME);
    int const cySizeFrame = GetSystemMetrics(SM_CYSIZEFRAME);
    int const cxPaddedBorder = GetSystemMetrics(SM_CXPADDEDBORDER);

    auto cxMargin = cxEdge + cxSizeFrame + cxPaddedBorder;
    auto cyMargin = cyEdge + cySizeFrame + cxPaddedBorder;

    if (w9GridHit == HTLEFT) {
        if (rbLeft) {
            if (rbTop) {
                hit = _HitTestRectTopLeft(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTLEFT)
                    return hit;
            }
            if (rbBottom) {
                hit = _HitTestRectBottomLeft(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTLEFT)
                    return hit;
            }

            return _HitTestRectLeft(prc, cxMargin, cyMargin, pt, wMiss);
        }
    } else if (w9GridHit == HTRIGHT) {
        if (rbRight) {
            if (rbTop) {
                hit = _HitTestRectTopRight(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTRIGHT)
                    return hit;
            }
            if (rbBottom) {
                hit = _HitTestRectBottomRight(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTRIGHT)
                    return hit;
            }

            return _HitTestRectRight(prc, cxMargin, cyMargin, pt, wMiss);
        }
    } else if (w9GridHit == HTTOP) {
        if (caption) {
            hit = HTCAPTION;
            wMiss = HTCAPTION;
        }
        if (rbTop) {
            if (rbLeft) {
                hit = _HitTestRectTopLeft(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTTOP)
                    return hit;
            }
            if (rbRight) {
                hit = _HitTestRectTopRight(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTTOP)
                    return hit;
            }
            return _HitTestRectTop(prc, cxMargin, cyMargin, pt, wMiss);
        }
    } else if (w9GridHit == HTTOPLEFT) {
        if (caption) {
            hit = HTCAPTION;
            wMiss = HTCAPTION;
        }
        if (rbTop && rbLeft)
            return _HitTestRectTopLeft(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbTop)
            return _HitTestRectTop(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbLeft)
            return _HitTestRectLeft(prc, cxMargin, cyMargin, pt, wMiss);
    } else if (w9GridHit == HTTOPRIGHT) {
        if (caption) {
            hit = HTCAPTION;
            wMiss = HTCAPTION;
        }
        if (rbTop && rbRight)
            return _HitTestRectTopRight(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbTop)
            return _HitTestRectTop(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbRight)
            return _HitTestRectRight(prc, cxMargin, cyMargin, pt, wMiss);
    } else if (w9GridHit == HTBOTTOM) {
        if (rbBottom) {
            if (rbLeft) {
                hit = _HitTestRectBottomLeft(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTBOTTOM)
                    return hit;
            }
            if (rbRight) {
                hit = _HitTestRectBottomRight(prc, cxMargin, cyMargin, pt, wMiss);
                if (hit != HTBOTTOM)
                    return hit;
            }
            return _HitTestRectBottom(prc, cxMargin, cyMargin, pt, wMiss);
        }
    } else if (w9GridHit == HTBOTTOMLEFT) {
        if (rbBottom && rbLeft)
            return _HitTestRectBottomLeft(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbBottom)
            return _HitTestRectBottom(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbLeft)
            return _HitTestRectLeft(prc, cxMargin, cyMargin, pt, wMiss);
    } else if (w9GridHit == HTBOTTOMRIGHT) {
        if (rbBottom && rbRight)
            return _HitTestRectBottomRight(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbBottom)
            return _HitTestRectBottom(prc, cxMargin, cyMargin, pt, wMiss);
        if (rbRight)
            return _HitTestRectRight(prc, cxMargin, cyMargin, pt, wMiss);
    }

    return hit;
}

static WORD _HitTestResizingTemplate(
    DWORD dwHTFlags, HRGN hrgn, POINT const *pt, DWORD w9GridHit, DWORD wMiss)
{
    bool const caption = dwHTFlags & HTTB_CAPTION;
    bool const rbLeft = dwHTFlags & HTTB_RESIZINGBORDER_LEFT;
    bool const rbTop = dwHTFlags & HTTB_RESIZINGBORDER_TOP;
    bool const rbRight = dwHTFlags & HTTB_RESIZINGBORDER_RIGHT;
    bool const rbBottom = dwHTFlags & HTTB_RESIZINGBORDER_BOTTOM;

    WORD hit = wMiss;
    switch (w9GridHit) {
    case HTLEFT:
        if (!rbLeft)
            return hit;
        break;
    case HTRIGHT:
        if (!rbRight)
            return hit;
        break;
    case HTTOP:
        if (caption)
            hit = HTCAPTION;
        if (!rbTop)
            return hit;
        break;
    case HTTOPLEFT:
        if (caption)
            hit = HTCAPTION;
        if (!rbTop || !rbLeft)
            return hit;
        break;
    case HTTOPRIGHT:
        if (caption)
            hit = HTCAPTION;
        if (!rbTop || !rbRight)
            return hit;
        break;
    case HTBOTTOM:
        if (!rbBottom)
            return hit;
        break;
    case HTBOTTOMLEFT:
        if (!rbBottom || !rbLeft)
            return hit;
        break;
    case HTBOTTOMRIGHT:
        if (!rbBottom || !rbRight)
            return hit;
        break;
    }

    if (PtInRegion(hrgn, pt->x, pt->y))
        return w9GridHit;

    return wMiss;
}

WORD HitTest9Grid(RECT const* prc, MARGINS const* margins, POINT const* pt,
                  bool fCheckLeftMarginZero)
{
    if (!fCheckLeftMarginZero || margins->cxLeftWidth != 0 || margins->cxRightWidth == 0) {
        if (HTLEFT == _HitTestRectLeft(prc, margins->cxLeftWidth, 0, pt, HTCLIENT)) {
            if (HTTOP == _HitTestRectTop(prc, 0, margins->cyTopHeight, pt, HTCLIENT))
                return HTTOPLEFT;
            if (HTBOTTOM == _HitTestRectBottom(prc, 0, margins->cyBottomHeight, pt, HTCLIENT))
                return HTBOTTOMLEFT;
            return HTLEFT;
        }
    }

    if (HTRIGHT == _HitTestRectRight(prc, margins->cxRightWidth, 0, pt, HTCLIENT)) {
        if (HTTOP == _HitTestRectTop(prc, 0, margins->cyTopHeight, pt, HTCLIENT))
            return HTTOPRIGHT;
        if (HTBOTTOM == _HitTestRectBottom(prc, 0, margins->cyBottomHeight, pt, HTCLIENT))
            return HTBOTTOMRIGHT;
        return HTRIGHT;
    }

    if (HTTOP == _HitTestRectTop(prc, 0, margins->cyTopHeight, pt, HTCLIENT))
        return HTTOP;
    if (HTBOTTOM == _HitTestRectBottom(prc, 0, margins->cyBottomHeight, pt, HTCLIENT))
        return HTBOTTOM;

    return HTCLIENT;
}

WORD HitTestRect(DWORD dwHTFlags, RECT const* prc, MARGINS const* margins,
                 POINT const* pt)
{
    if (!PtInRect(prc, *pt))
        return HTNOWHERE;

    WORD hit = HitTest9Grid(prc, margins, pt, dwHTFlags & 0x400);
    if (hit == HTCLIENT)
        return hit;

    if (dwHTFlags & HTTB_RESIZINGBORDER)
        return _HitTestResizingRect(dwHTFlags, prc, pt, hit, HTBORDER);

    if (dwHTFlags & (HTTB_FIXEDBORDER | HTTB_CAPTION)) {
        if ((hit == HTTOP || hit == HTTOPLEFT || hit == HTTOPRIGHT) && dwHTFlags & HTTB_CAPTION)
            return HTCAPTION;
        return HTBORDER;
    }

    return hit;
}

WORD HitTestTemplate(DWORD dwHTFlags, RECT const* prc, HRGN hrgn,
                     MARGINS const* margins, POINT const* pt)
{

    if (!PtInRect(prc, *pt))
        return HTNOWHERE;

    WORD hit = HitTest9Grid(prc, margins, pt, (dwHTFlags >> 10) & 1);
    if (hit == HTCLIENT)
        return hit;

    if (dwHTFlags & HTTB_RESIZINGBORDER)
        return _HitTestResizingTemplate(dwHTFlags, hrgn, pt, hit, HTBORDER);

    if (dwHTFlags & (HTTB_FIXEDBORDER | HTTB_CAPTION)) {
        if ((hit == HTTOP || hit == HTTOPLEFT || hit == HTTOPRIGHT) && dwHTFlags & HTTB_CAPTION)
            return HTCAPTION;
        return HTBORDER;
    }

    return hit;
}

} // namespace uxtheme
