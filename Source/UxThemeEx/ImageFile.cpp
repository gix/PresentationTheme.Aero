#include "ImageFile.h"

#include "DpiInfo.h"
#include "RenderObj.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"
#include "gdiex.h"

#define min(x, y) (((x) < (y)) ? (x) : (y))
#define max(x, y) (((x) > (y)) ? (x) : (y))
#include <atlimage.h>
#include "Handle.h"
#include "GdiHandles.h"
#include "DrawHelp.h"
#undef min
#undef max

namespace uxtheme
{

static void AdjustSizeMin(SIZE* psz, int ixMin, int iyMin)
{
    if (psz->cx < ixMin)
        psz->cx = ixMin;
    if (psz->cy < iyMin)
        psz->cy = iyMin;
}

static void _InPlaceUnionRect(RECT* prcDest, RECT const* prcSrc)
{
    if (prcDest->left == -1 || prcDest->left > prcSrc->left)
        prcDest->left = prcSrc->left;
    if (prcDest->right == -1 || prcDest->right < prcSrc->right)
        prcDest->right = prcSrc->right;
    if (prcDest->top == -1 || prcDest->top > prcSrc->top)
        prcDest->top = prcSrc->top;
    if (prcDest->bottom == -1 || prcDest->bottom < prcSrc->bottom)
        prcDest->bottom = prcSrc->bottom;
}

static HRGN _PixelsToRgn(
    unsigned* pdwBits, int cxImageOffset, int cyImageOffset, int cxImage,
    int cyImage, int cxSrc, int cySrc, DIBINFO* pdi)
{
    __int64 v9;
    int v15;
    unsigned v16;
    unsigned *v19;
    unsigned v20;
    __int64 v21;
    int v22;
    int v23;
    unsigned *v24;
    unsigned *v25;
    unsigned *v26;
    unsigned v27;
    int v28;
    RECT *v29;
    __int64 v30;
    unsigned *v31;

    v9 = cxImage;
    auto rgnData = make_unique_malloc<RGNDATA>(0x2020);
    if (!rgnData)
        return nullptr;

    v27 = 512;
    v29 = (RECT *)rgnData->Buffer;
    memset(rgnData.get(), 0, 0x20ui64);
    rgnData->rdh.dwSize = 32;
    rgnData->rdh.iType = RDH_RECTANGLES;
    SetRect(&rgnData->rdh.rcBound, -1, -1, -1, -1);
    v15 = cySrc - cyImageOffset - cyImage;
    if (v15 < 0)
        v15 = 0;
    v28 = v15 + cyImage - 1;
    if (v15 <= v28) {
        v21 = v9;
        v23 = cyImage - 1;
        v30 = v9;
        v22 = cxImageOffset + cxSrc * v15;
        do {
            v24 = &pdwBits[v22];
            v25 = &v24[v21 - 1];
            v19 = v24;
            if (v24 <= v25) {
                do {
                    if (pdi->fPartiallyTransparent) {
                        if (v19 > v25)
                            break;
                        do {
                            if (*((BYTE *)v19 + 3) >= pdi->iAlphaThreshold)
                                break;
                            ++v19;
                        } while (v19 <= v25);
                    }
                    if (v19 > v25)
                        break;
                    v26 = v19;
                    ++v19;
                    v31 = v26;
                    if (pdi->fPartiallyTransparent) {
                        while (v19 <= v25 && *((BYTE *)v19 + 3) >= pdi->iAlphaThreshold)
                            ++v19;
                    } else if (v19 <= v25) {
                        v19 += ((uintptr_t)((BYTE*)v25 - (BYTE*)v19) >> 2) + 1;
                    }
                    v20 = v27;
                    if (rgnData->rdh.nCount >= v27) {
                        v27 += 512;
                        if (!realloc(rgnData, 16 * (v20 + 512 + 2i64)))
                            return nullptr;
                        v26 = v31;
                        v29 = (RECT*)((BYTE*)rgnData.get() + 32);
                    }
                    SetRect(&v29[rgnData->rdh.nCount], v26 - v24, v23, v19 - v24, v23 + 1);
                    _InPlaceUnionRect(&rgnData->rdh.rcBound, &v29[rgnData->rdh.nCount]);
                    ++rgnData->rdh.nCount;
                } while (v19 <= v25);
                v21 = v30;
            }
            v22 += cxSrc;
            ++v15;
            --v23;
        } while (v15 <= v28);
    }
    if (!rgnData->rdh.nCount || rgnData->rdh.rcBound.left < 0 ||
        rgnData->rdh.rcBound.top < 0 || rgnData->rdh.rcBound.right < 0 ||
        rgnData->rdh.rcBound.bottom < 0)
        return nullptr;

    return ExtCreateRegion(nullptr, 16 * (rgnData->rdh.nCount + 2),
                           rgnData.get());
}

BOOL CImageFile::KeyProperty(int iPropId)
{
    switch (iPropId) {
    case TMT_TRANSPARENT:
    case TMT_AUTOSIZE:
    case TMT_BORDERONLY:
    case TMT_BGFILL:
    case TMT_GLYPHTRANSPARENT:
    case TMT_GLYPHONLY:
    case TMT_MIRRORIMAGE:
    case TMT_UNIFORMSIZING:
    case TMT_INTEGRALSIZING:
    case TMT_SOURCEGROW:
    case TMT_SOURCESHRINK:
    case TMT_IMAGECOUNT:
    case TMT_ALPHALEVEL:
    case TMT_ALPHATHRESHOLD:
    case TMT_GLYPHINDEX:
    case TMT_TRUESIZESTRETCHMARK:
    case TMT_MINDPI1:
    case TMT_MINDPI2:
    case TMT_MINDPI3:
    case TMT_MINDPI4:
    case TMT_MINDPI5:
    case TMT_MINDPI6:
    case TMT_MINDPI7:
    case TMT_GLYPHFONT:
    case TMT_IMAGEFILE:
    case TMT_IMAGEFILE1:
    case TMT_IMAGEFILE2:
    case TMT_IMAGEFILE3:
    case TMT_IMAGEFILE4:
    case TMT_IMAGEFILE5:
    case TMT_GLYPHIMAGEFILE:
    case TMT_IMAGEFILE6:
    case TMT_IMAGEFILE7:
    case TMT_MINSIZE:
    case TMT_MINSIZE1:
    case TMT_MINSIZE2:
    case TMT_MINSIZE3:
    case TMT_MINSIZE4:
    case TMT_MINSIZE5:
    case TMT_MINSIZE6:
    case TMT_MINSIZE7:
    case TMT_SIZINGMARGINS:
    case TMT_CONTENTMARGINS:
    case TMT_TRANSPARENTCOLOR:
    case TMT_GLYPHTEXTCOLOR:
    case TMT_GLYPHTRANSPARENTCOLOR:
    case TMT_BGTYPE:
    case TMT_SIZINGTYPE:
    case TMT_HALIGN:
    case TMT_VALIGN:
    case TMT_IMAGELAYOUT:
    case TMT_GLYPHTYPE:
    case TMT_IMAGESELECTTYPE:
    case TMT_TRUESIZESCALINGTYPE:
        return TRUE;

    default:
        return FALSE;
    }
}

DIBINFO* CImageFile::EnumImageFiles(int iIndex)
{
    if (iIndex == 0)
        return &_ImageInfo;

    if (_eGlyphType == GT_IMAGEGLYPH) {
        --iIndex;
        if (iIndex == 0)
            return &_GlyphInfo;
    }

    --iIndex;

    if (iIndex >= 0 && iIndex < _iMultiImageCount)
        return static_cast<CMaxImageFile*>(this)->MultiDibPtr(iIndex);

    if (iIndex == _iMultiImageCount && _ScaledImageInfo.iDibOffset)
        return &_ScaledImageInfo;

    return nullptr;
}

BOOL CImageFile::HasSizingMargin() const
{
    return
        _SizingMargins.cxLeftWidth != 0 ||
        _SizingMargins.cxRightWidth != 0 ||
        _SizingMargins.cyTopHeight != 0 ||
        _SizingMargins.cyBottomHeight != 0;
}

DIBINFO * CImageFile::FindBackgroundImageToScale()
{
    if (!HasSizingMargin())
        return nullptr;

    DIBINFO* pdi = nullptr;
    int minDpi = 0;

    if (_ImageInfo.iDibOffset && _ImageInfo.iMinDpi) {
        minDpi = _ImageInfo.iMinDpi;
        pdi = &_ImageInfo;
    }

    if (_eGlyphType != GT_IMAGEGLYPH) {
        for (int i = 0; i < _iMultiImageCount; ++i) {
            DIBINFO* multiDib = static_cast<CMaxImageFile*>(this)->MultiDibPtr(i);
            if (multiDib && multiDib->iDibOffset != 0 && multiDib->iMinDpi > minDpi) {
                pdi = multiDib;
                minDpi = multiDib->iMinDpi;
            }
        }
    }

    auto screenDpi = GetScreenDpi();
    if (pdi && (2 * pdi->iMinDpi > screenDpi || screenDpi < 385))
        return nullptr;

    return pdi;
}

static HBITMAP CreateScaledDDB(HBITMAP hbmp)
{
    HBITMAP v1;
    HDC v2;
    HDC v3;
    HDC v4;
    int v5;
    int v6;
    int v7;
    HBITMAP v8;
    HBITMAP v9;
    HGDIOBJ v10;
    HGDIOBJ v11;
    int v12;
    BITMAP v14;
    BITMAPHEADER v15;

    v1 = hbmp;
    if (GetObjectW(hbmp, 32, &v14))
    {
        v2 = CreateCompatibleDC(0i64);
        v3 = CreateCompatibleDC(0i64);
        v4 = v3;
        if (v2)
        {
            if (v3)
            {
                v15.bmih.biSize = 40;
                v5 = GetScreenDpi();
                v15.bmih.biWidth = MulDiv(v14.bmWidth, v5, 96);
                v6 = GetScreenDpi();
                v7 = MulDiv(v14.bmHeight, v6, 96);
                v15.bmih.biSizeImage = 0;
                v15.bmih.biXPelsPerMeter = 0;
                v15.bmih.biYPelsPerMeter = 0;
                v15.bmih.biClrImportant = 0;
                v15.bmih.biHeight = v7;
                v15.bmih.biPlanes = 1;
                v15.bmih.biBitCount = v14.bmBitsPixel;
                v15.bmih.biCompression = 3;
                v15.bmih.biClrUsed = 3;
                v15.masks[0] = 0xFF0000;
                v15.masks[1] = 0xFF00;
                v15.masks[2] = 0xFF;
                v8 = CreateDIBitmap(v2, &v15.bmih, 2u, 0i64, (const BITMAPINFO *)&v15, 0);
                v9 = v8;
                if (v8)
                {
                    v10 = SelectObject(v2, v8);
                    v11 = SelectObject(v4, v1);
                    SetLayout(v2, 0);
                    SetLayout(v4, 0);
                    v12 = SetStretchBltMode(v2, 3);
                    StretchBlt(v2, 0, 0, v15.bmih.biWidth, v15.bmih.biHeight, v4, 0, 0, v14.bmWidth, v14.bmHeight, SRCCOPY);
                    SetStretchBltMode(v2, v12);
                    SelectObject(v2, v10);
                    SelectObject(v4, v11);
                    v1 = v9;
                }
            }
            DeleteDC(v2);
        }
        if (v4)
            DeleteDC(v4);
    }
    return v1;
}

HRESULT CImageFile::CreateScaledBackgroundImage(
    CRenderObj* pRender, int iPartId, int iStateId, DIBINFO** pScaledDibInfo)
{
    *pScaledDibInfo = nullptr;

    DIBINFO* imageToScale = FindBackgroundImageToScale();
    if (!imageToScale)
        return S_FALSE;

    HBITMAP hbmp = nullptr;
    HRESULT hr = pRender->ExternalGetBitmap(nullptr, imageToScale->iDibOffset, GBF_DIRECT, &hbmp);
    if (hr < 0)
        return hr;

    if (!hbmp)
        return S_FALSE;

    int offset = pRender->GetValueIndex(iPartId, iStateId, TMT_SCALEDBACKGROUND);
    if (offset == -1)
        return S_OK;

    TMBITMAPHEADER* hdr = pRender->GetBitmapHeader(offset);
    HBITMAP scaledDDB = CreateScaledDDB(hbmp);
    if (scaledDDB) {
        pRender->SetBitmapHandle(hdr->iBitmapIndex, scaledDDB);
        hdr->fPartiallyTransparent = imageToScale->fPartiallyTransparent;

        _ScaledImageInfo = *imageToScale;

        int dpi = GetScreenDpi();
        _ScaledImageInfo.uhbm.hBitmap = nullptr;
        _ScaledImageInfo.iDibOffset = offset;
        _ScaledImageInfo.iMinDpi = dpi;
        _ScaledImageInfo.iSingleWidth = MulDiv(imageToScale->iSingleWidth, dpi, 96);
        _ScaledImageInfo.iSingleHeight = MulDiv(imageToScale->iSingleHeight, dpi, 96);
        _ScaledImageInfo.szMinSize.cx = MulDiv(imageToScale->szMinSize.cx, dpi, 96);
        _ScaledImageInfo.szMinSize.cy = MulDiv(imageToScale->szMinSize.cy, dpi, 96);
        *pScaledDibInfo = &_ScaledImageInfo;
    }

    return S_OK;
}

HRESULT CImageFile::PackProperties(CRenderObj* pRender, int iPartId, int iStateId)
{
    static_assert(std::is_trivially_copyable_v<CImageFile>);
    memset(this, 0, sizeof(CImageFile));

    _eBgType = BT_IMAGEFILE;
    _iSourcePartId = iPartId;
    _iSourceStateId = iStateId;
    _ImageInfo.iMinDpi = 96;

    _ImageInfo.iDibOffset = pRender->GetValueIndex(iPartId, iStateId, TMT_DIBDATA);
    if (_ImageInfo.iDibOffset == -1)
        _ImageInfo.iDibOffset = 0;

    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_IMAGECOUNT, &_iImageCount) < 0)
        _iImageCount = 1;

    if (_iImageCount < 1)
        _iImageCount = 1;

    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_IMAGELAYOUT, (int*)&_eImageLayout) < 0)
        _eImageLayout = IL_HORIZONTAL;

    if (_ImageInfo.iDibOffset)
        ENSURE_HR(SetImageInfo(&_ImageInfo, pRender, iPartId, iStateId));

    if (pRender->ExternalGetPosition(
        iPartId, iStateId, TMT_MINSIZE, (POINT*)&_ImageInfo.szMinSize) >= 0) {
        AdjustSizeMin(&_ImageInfo.szMinSize, 1, 1);
    } else {
        _ImageInfo.szMinSize.cx = _ImageInfo.iSingleWidth;
        _ImageInfo.szMinSize.cy = _ImageInfo.iSingleHeight;
    }

    if (pRender->ExternalGetEnumValue(
        iPartId, iStateId, TMT_TRUESIZESCALINGTYPE, (int*)&_eTrueSizeScalingType) < 0)
        _eTrueSizeScalingType = TSST_NONE;
    if (pRender->ExternalGetEnumValue(
        iPartId, iStateId, TMT_SIZINGTYPE, (int*)&_ImageInfo.eSizingType) < 0)
        _ImageInfo.eSizingType = ST_STRETCH;

    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_BORDERONLY, &_ImageInfo.fBorderOnly) < 0)
        _ImageInfo.fBorderOnly = 0;
    if (pRender->ExternalGetInt(iPartId, iStateId, TMT_TRUESIZESTRETCHMARK, &_iTrueSizeStretchMark) < 0)
        _iTrueSizeStretchMark = 0;
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_UNIFORMSIZING, &_fUniformSizing) < 0)
        _fUniformSizing = 0;
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_INTEGRALSIZING, &_fIntegralSizing) < 0)
        _fIntegralSizing = 0;

    pRender->ExternalGetBool(iPartId, iStateId, TMT_TRANSPARENT, &_ImageInfo.fPartiallyTransparent);
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_MIRRORIMAGE, &_fMirrorImage) < 0)
        _fMirrorImage = 1;
    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_HALIGN, (int*)&_eHAlign) < 0)
        _eHAlign = HA_CENTER;
    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_VALIGN, (int*)&_eVAlign) < 0)
        _eVAlign = VA_CENTER;

    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_BGFILL, &_fBgFill) >= 0
        && pRender->ExternalGetInt(iPartId, iStateId, TMT_FILLCOLOR, (int*)&_crFill) < 0)
        _crFill = 0xFFFFFF;

    if (pRender->ExternalGetMargins(
        nullptr, iPartId, iStateId, TMT_SIZINGMARGINS, nullptr, &_SizingMargins) < 0)
        _SizingMargins = {};

    if (pRender->ExternalGetMargins(
        nullptr, iPartId, iStateId, TMT_CONTENTMARGINS, nullptr, &_ContentMargins) < 0)
        _ContentMargins = _SizingMargins;

    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_SOURCEGROW, &_fSourceGrow) < 0)
        _fSourceGrow = 0;
    if (pRender->ExternalGetBool(iPartId, iStateId, TMT_SOURCESHRINK, &_fSourceShrink) < 0)
        _fSourceShrink = 0;

    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_GLYPHTYPE, (int *)&_eGlyphType) < 0)
        _eGlyphType = GT_NONE;

    if (_eGlyphType == GT_FONTGLYPH) {
        if (!pRender->GetFontTableIndex(iPartId, iStateId, TMT_GLYPHFONT, &_iGlyphFontIndex))
            return S_OK;
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLYPHTEXTCOLOR, (int *)&_crGlyphTextColor) < 0)
            _crGlyphTextColor = 0;
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLYPHINDEX, &_iGlyphIndex) < 0)
            _iGlyphIndex = 1;
    } else if (_eGlyphType == GT_IMAGEGLYPH) {
        _GlyphInfo.iMinDpi = 96;
        _GlyphInfo.iDibOffset = pRender->GetValueIndex(iPartId, iStateId, 8);
        if (_GlyphInfo.iDibOffset == -1)
            _GlyphInfo.iDibOffset = 0;
        if (_GlyphInfo.iDibOffset > 0)
            ENSURE_HR(SetImageInfo(&_GlyphInfo, pRender, iPartId, iStateId));
        pRender->ExternalGetBool(
            iPartId, iStateId, TMT_GLYPHTRANSPARENT, &_GlyphInfo.fPartiallyTransparent);
        _GlyphInfo.eSizingType = ST_TRUESIZE;
        _GlyphInfo.fBorderOnly = FALSE;
    }

    if (_eGlyphType != 0 && pRender->ExternalGetBool(iPartId, iStateId, TMT_GLYPHONLY, &_fGlyphOnly) < 0)
        _fGlyphOnly = FALSE;

    if (pRender->ExternalGetEnumValue(
        iPartId, iStateId, TMT_IMAGESELECTTYPE, (int*)&_eImageSelectType) < 0)
        _eImageSelectType = IST_NONE;

    if (_eImageSelectType != IST_NONE) {
        DIBINFO* pdi = &_ImageInfo;
        if (_eGlyphType == GT_IMAGEGLYPH)
            pdi = &_GlyphInfo;

        for (int i = 0; i < 7; ++i) {
            int index = pRender->GetValueIndex(iPartId, iStateId,
                                               Map_Ordinal_To_DIBDATA(i));
            if (index == -1)
                break;

            ++_iMultiImageCount;
            DIBINFO* pdiCurrent = static_cast<CMaxImageFile*>(this)->MultiDibPtr(i);
            *pdiCurrent = *pdi;
            pdiCurrent->iDibOffset = index;

            ENSURE_HR(SetImageInfo(pdiCurrent, pRender, iPartId, iStateId));

            if (pRender->ExternalGetInt(iPartId, iStateId,
                                        Map_Ordinal_To_MINDPI(i), &pdiCurrent->iMinDpi) >= 0) {
                if (pdiCurrent->iMinDpi < 1)
                    pdiCurrent->iMinDpi = 1;
            } else
                pdiCurrent->iMinDpi = 96;

            if (pRender->ExternalGetPosition(iPartId, iStateId,
                                             Map_Ordinal_To_MINSIZE(i),
                                             (POINT*)&pdiCurrent->szMinSize) >= 0) {
                AdjustSizeMin(&pdiCurrent->szMinSize, 1, 1);
            } else
                pdiCurrent->szMinSize = {pdiCurrent->iSingleWidth, pdiCurrent->iSingleHeight};
        }

        if (_iMultiImageCount > 0)
            *pdi = *static_cast<CMaxImageFile*>(this)->MultiDibPtr(0);
    }

    return S_OK;
}

static int const rgPropIds[] = {
    TMT_DIBDATA, TMT_DIBDATA1, TMT_DIBDATA2, TMT_DIBDATA3, TMT_DIBDATA4,
    TMT_DIBDATA5, TMT_DIBDATA6, TMT_DIBDATA7, TMT_GLYPHDIBDATA, TMT_HBITMAP
};

HRESULT CImageFile::GetBitmap(
    CRenderObj* pRender, int iPropId, unsigned dwFlags, HBITMAP* phBitmap)
{
    int index = 0;

    if (iPropId) {
        index = pRender->GetValueIndex(_iSourcePartId, _iSourceStateId, iPropId);
    } else {
        for (int const propId : rgPropIds) {
            index = pRender->GetValueIndex(_iSourcePartId, _iSourceStateId, propId);
            if (index > 0)
                break;
        }
    }

    if (index <= 0)
        return E_INVALIDARG;

    return pRender->ExternalGetBitmap(nullptr, index, dwFlags, phBitmap);
}

HRESULT CImageFile::GetPartSize(
    CRenderObj* pRender, HDC hdc, RECT const* prc, THEMESIZE eSize, SIZE* psz)
{
    TRUESTRETCHINFO tsi;

    DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, prc, FALSE, &tsi);
    if (eSize == TS_MIN) {
        MARGINS margins;
        ENSURE_HR(GetScaledContentMargins(pRender, hdc, prc, &margins));
        psz->cx = std::max(1, margins.cxLeftWidth + margins.cxRightWidth);
        psz->cy = std::max(1, margins.cyTopHeight + margins.cyBottomHeight);
    } else if (eSize == TS_TRUE) {
        psz->cx = pdi->iSingleWidth;
        psz->cy = pdi->iSingleHeight;
    } else if (eSize == TS_DRAW) {
        GetDrawnImageSize(pdi, prc, &tsi, psz);
    } else {
        return E_INVALIDARG;
    }

    return S_OK;
}

template<typename T>
static void StretchUniform(T& width, T& height, int targetWidth, int targetHeight)
{
    double const w = static_cast<double>(targetWidth);
    double const h = static_cast<double>(targetHeight);
    double const scaleX = width / w;
    double const scaleY = height / h;
    if (scaleY < scaleX)
        width = static_cast<T>(w * scaleY);
    else if (scaleX < scaleY)
        height = static_cast<T>(h * scaleX);
}

void CImageFile::GetDrawnImageSize(
    DIBINFO const* pdi, RECT const* pRect, TRUESTRETCHINFO const* ptsInfo,
    SIZE* pszDraw)
{
    if (pdi->eSizingType == ST_TRUESIZE) {
        if (ptsInfo->fForceStretch) {
            *pszDraw = ptsInfo->szDrawSize;
            if (_fIntegralSizing && !ptsInfo->fFullStretch) {
                pszDraw->cx = pdi->iSingleWidth * static_cast<int>(
                    ptsInfo->szDrawSize.cx / static_cast<float>(pdi->iSingleWidth));
                pszDraw->cy = pdi->iSingleHeight * static_cast<int>(
                    ptsInfo->szDrawSize.cy / static_cast<float>(pdi->iSingleHeight));
            }
        } else {
            pszDraw->cx = pdi->iSingleWidth;
            pszDraw->cy = pdi->iSingleHeight;
        }

        if (_fUniformSizing) {
            StretchUniform(pszDraw->cx, pszDraw->cy,
                           pdi->iSingleWidth, pdi->iSingleHeight);
        }
    } else {
        if (pRect) {
            pszDraw->cx = pRect->right - pRect->left;
            pszDraw->cy = pRect->bottom - pRect->top;
        } else {
            *pszDraw = SIZE();
        }
    }
}

HRESULT CImageFile::SetImageInfo(
    DIBINFO* pdi, CRenderObj const* pRender, int iPartId, int iStateId)
{
    if (!pRender->_pbSharableData)
        return E_FAIL;

    TMBITMAPHEADER* tmhdr = pRender->GetBitmapHeader(pdi->iDibOffset);

    int partiallyTransparent =
        pdi->fPartiallyTransparent ||
        tmhdr->fPartiallyTransparent;
    pdi->fPartiallyTransparent = partiallyTransparent;

    if (partiallyTransparent &&
        pRender->ExternalGetBool(iPartId, iStateId, TMT_ALPHATHRESHOLD, &pdi->iAlphaThreshold) < 0)
        pdi->iAlphaThreshold = 255;

    HBITMAP hbmp = pRender->BitmapIndexToHandle(tmhdr->iBitmapIndex);

    int width, height;
    if (hbmp) {
        BITMAP bitmap;
        if (!GetObjectW(hbmp, 32, &bitmap))
            return E_FAIL;
        width = bitmap.bmWidth;
        height = bitmap.bmHeight;
    } else {
        auto bmphdr = (BITMAPHEADER*)((BYTE*)tmhdr + tmhdr->dwSize);
        if (!bmphdr)
            return E_FAIL;
        width = bmphdr->bmih.biWidth;
        height = bmphdr->bmih.biHeight;
    }

    if (width != -1 && height != -1) {
        if (_eImageLayout == IL_HORIZONTAL) {
            pdi->iSingleHeight = height;
            pdi->iSingleWidth = width / _iImageCount;
        } else {
            pdi->iSingleWidth = width;
            pdi->iSingleHeight = height / _iImageCount;
        }
    }

    return S_OK;
}

HRESULT CImageFile::ScaleMargins(
    MARGINS* pMargins, HDC hdcOrig, CRenderObj* pRender, DIBINFO const* pdi,
    SIZE const* pszDraw, float* pfx, float* pfy)
{
    OptionalDC hdc{hdcOrig};
    bool forceRectSizing = pRender && pRender->_dwOtdFlags & OTD_FORCE_RECT_SIZING;

    float scaleX = 1.0f;
    float scaleY = 1.0f;
    if (_SizingMargins.cxLeftWidth != 0 ||
        _SizingMargins.cxRightWidth != 0 ||
        _SizingMargins.cyTopHeight != 0 ||
        _SizingMargins.cyBottomHeight != 0) {
        if (pszDraw->cx > 0 && pszDraw->cy > 0) {
            bool v18 = false;
            bool v19 = false;
            if (_fSourceShrink || forceRectSizing) {
                if (pszDraw->cx < pdi->szMinSize.cx)
                    v18 = true;
                if (pszDraw->cy < pdi->szMinSize.cy)
                    v19 = true;
            }

            if (_fSourceGrow || forceRectSizing) {
                if (hdc && (!v18 && !v19)) {
                    int dpiX = 0;
                    pRender->GetEffectiveDpi(hdc, &dpiX, nullptr);
                    if (dpiX >= 385 && dpiX >= 2 * pdi->iMinDpi) {
                        scaleX = (float)(dpiX / pdi->iMinDpi);
                        scaleY = (float)(dpiX / pdi->iMinDpi);
                    }
                }
            }
        }

        float v20 = std::fmin(scaleY, scaleX);
        scaleX = v20;
        if (scaleX > 1.0)
            scaleX = (int)scaleX;
        if (scaleY > 1.0)
            scaleY = (int)scaleY;

        if (scaleX != 1.0f) {
            pMargins->cxLeftWidth = (int)std::floor((pMargins->cxLeftWidth * scaleX) + 0.5);
            pMargins->cxRightWidth = (int)std::floor((pMargins->cxRightWidth * scaleX) + 0.5);
        }

        if (scaleY != 1.0f) {
            pMargins->cyTopHeight = (int)std::floor((pMargins->cyTopHeight * scaleY) + 0.5);
            pMargins->cyBottomHeight = (int)std::floor((pMargins->cyBottomHeight * scaleY) + 0.5);
        }
    }

    if (pfx)
        *pfx = scaleX;
    if (pfy)
        *pfy = scaleY;

    return S_OK;
}

HRESULT CImageFile::GetScaledContentMargins(
    CRenderObj* pRender, HDC hdc, RECT const* prcDest, MARGINS* pMargins)
{
    *pMargins = _ContentMargins;

    TRUESTRETCHINFO tsInfo;
    DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, prcDest, 0, &tsInfo);
    SIZE size;
    GetDrawnImageSize(pdi, prcDest, &tsInfo, &size);

    return ScaleMargins(pMargins, hdc, pRender, pdi, &size, nullptr, nullptr);
}

HRESULT CImageFile::GetBackgroundContentRect(
    CRenderObj* pRender, HDC hdc, RECT const* pBoundingRect, RECT* pContentRect)
{
    MARGINS contentMargins;
    ENSURE_HR(GetScaledContentMargins(pRender, hdc, pBoundingRect, &contentMargins));

    pContentRect->left = pBoundingRect->left + contentMargins.cxLeftWidth;
    pContentRect->top = pBoundingRect->top + contentMargins.cyTopHeight;
    pContentRect->right = pBoundingRect->right - contentMargins.cxRightWidth;
    pContentRect->bottom = pBoundingRect->bottom - contentMargins.cyBottomHeight;
    return S_OK;
}

DIBINFO* CImageFile::SelectCorrectImageFile(
    CRenderObj* pRender, HDC hdc, RECT const* prc, bool fForGlyph, TRUESTRETCHINFO* ptsInfo)
{
    bool releaseDC = false;
    if (!hdc) {
        hdc = GetWindowDC(nullptr);
        if (hdc)
            releaseDC = true;
    }

    DIBINFO* const pdiDefault = fForGlyph ? &_GlyphInfo : &_ImageInfo;
    int const width = prc ? prc->right - prc->left : 1;
    int const height = prc ? prc->bottom - prc->top : 1;

    bool forceSizing = false;
    if (fForGlyph || _ImageInfo.eSizingType == ST_TRUESIZE) {
        if (pRender && pRender->_dwOtdFlags & OTD_FORCE_RECT_SIZING)
            forceSizing = true;
    }

    DIBINFO* pdi = nullptr;
    if (fForGlyph || _eGlyphType != GT_IMAGEGLYPH) {
        if (_eImageSelectType == IST_DPI) {
            if (hdc) {
                int dpiX, dpiY;
                pRender->GetEffectiveDpi(hdc, &dpiX, &dpiY);
                int dpi = std::min(dpiX, dpiY);

                for (int i = _iMultiImageCount - 1; i >= 0; --i) {
                    DIBINFO* p = static_cast<CMaxImageFile*>(this)->MultiDibPtr(i);
                    if (p->iMinDpi <= dpi && !(pdi && p->iMinDpi <= pdi->iMinDpi))
                        pdi = p;
                }
            }
        } else if (_eImageSelectType == IST_SIZE || forceSizing || _fSourceGrow) {
            if (prc) {
                for (int i = _iMultiImageCount - 1; i >= 0; --i) {
                    DIBINFO* p = static_cast<CMaxImageFile*>(this)->MultiDibPtr(i);
                    if (p->szMinSize.cx <= width && p->szMinSize.cy <= height) {
                        pdi = p;
                        break;
                    }
                }
            }
        }

        if (!pdi)
            pdi = pdiDefault;
    } else {
        pdi = &_ImageInfo;
    }

    if (ptsInfo) {
        ptsInfo->fForceStretch = FALSE;
        ptsInfo->fFullStretch = FALSE;
        ptsInfo->szDrawSize = SIZE();

        if (pdi->eSizingType == ST_TRUESIZE && _eTrueSizeScalingType != TSST_NONE) {
            if (prc && (forceSizing || pdi->iSingleWidth > width || pdi->iSingleHeight > height)) {
                ptsInfo->fFullStretch = 1;
                ptsInfo->szDrawSize.cx = width;
                ptsInfo->szDrawSize.cy = height;
                ptsInfo->fForceStretch = 1;
            } else if (_eTrueSizeScalingType == TSST_DPI) {
                if (hdc) {
                    int dpiX = 0;
                    int dpiY = 0;
                    pRender->GetEffectiveDpi(hdc, &dpiX, &dpiY);
                    SIZE drawSize;
                    drawSize.cx = MulDiv(pdi->iSingleWidth, dpiX, pdi->iMinDpi);
                    drawSize.cy = MulDiv(pdi->iSingleHeight, dpiY, pdi->iMinDpi);
                    if (drawSize.cx != 0) {
                        if (prc) {
                            if (drawSize.cx >= width)
                                drawSize.cx = width;
                            if (drawSize.cy >= height)
                                drawSize.cy = height;
                        }

                        if (100 * (drawSize.cx - pdi->iSingleWidth) / pdi->iSingleWidth >= _iTrueSizeStretchMark &&
                            100 * (drawSize.cy - pdi->iSingleHeight) / pdi->iSingleHeight >= _iTrueSizeStretchMark)
                        {
                            ptsInfo->szDrawSize = drawSize;
                            ptsInfo->fForceStretch = TRUE;
                        }
                    }
                }
            } else if (_eTrueSizeScalingType == TSST_SIZE) {
                if (prc) {
                    SIZE drawSize;
                    drawSize.cx = MulDiv(pdi->iSingleWidth, width, pdi->szMinSize.cx);
                    drawSize.cy = MulDiv(pdi->iSingleHeight, height, pdi->szMinSize.cy);
                    if (drawSize.cx != 0) {
                        if (drawSize.cx >= width)
                            drawSize.cx = width;
                        if (drawSize.cy >= height)
                            drawSize.cy = height;

                        if (100 * (drawSize.cx - pdi->iSingleWidth) / pdi->iSingleWidth >= _iTrueSizeStretchMark &&
                            100 * (drawSize.cy - pdi->iSingleHeight) / pdi->iSingleHeight >= _iTrueSizeStretchMark)
                        {
                            ptsInfo->szDrawSize = drawSize;
                            ptsInfo->fForceStretch = TRUE;
                        }
                    }
                }
            }
        }
    }

    if (!pdi)
        pdi = pdiDefault;
    if (releaseDC)
        ReleaseDC(nullptr, hdc);
    return pdi;
}

HRESULT CImageFile::DrawFontGlyph(CRenderObj* pRender, HDC hdc, RECT* prc,
                                  DTBGOPTS const* pOptions)
{
    RECT const* const clipRect = (pOptions && pOptions->dwFlags & DTBG_CLIPRECT) ?
        &pOptions->rcClip : nullptr;

    SaveClipRegion scrOrig;
    HGDIOBJ oldFont = nullptr;
    COLORREF oldColor = 0;
    int oldMode = 0;

    HFONT hFont = nullptr;
    HRESULT hr = pRender->GetScaledFontHandle(hdc, _iGlyphFontIndex, &hFont);
    if (hr < 0)
        goto done;

    oldFont = SelectObject(hdc, hFont);
    if (!oldFont) {
        hr = MakeErrorLast();
        goto done;
    }

    oldColor = SetTextColor(hdc, _crGlyphTextColor);
    oldMode = SetBkMode(hdc, TRANSPARENT);

    {
        UINT format = DT_SINGLELINE;
        if (_eHAlign)
            format = (_eHAlign != 1) + 33;

        if (_eHAlign == HA_CENTER)
            format |= DT_CENTER;
        else if (_eHAlign == HA_RIGHT)
            format |= DT_RIGHT;

        if (_eVAlign == VA_CENTER)
            format |= DT_VCENTER;
        else if (VA_BOTTOM)
            format |= DT_BOTTOM;

        if (clipRect) {
            hr = scrOrig.Save(hdc);
            if (hr < 0)
                goto done;
            if (!IntersectClipRect(hdc, clipRect->left, clipRect->top, clipRect->right, clipRect->bottom)) {
                hr = MakeErrorLast();
                goto done;
            }
        }

        wchar_t text[1] = {_iGlyphIndex};
        if (!DrawTextExW(hdc, text, 1, prc, format, nullptr))
            hr = MakeErrorLast();
    }

done:
    if (clipRect)
        scrOrig.Restore(hdc);
    if (oldMode != TRANSPARENT)
        SetBkMode(hdc, oldMode);
    if (_crGlyphTextColor != oldColor)
        SetTextColor(hdc, oldColor);
    if (oldFont)
        SelectObject(hdc, oldFont);
    pRender->ReturnFontHandle(hFont);
    return hr;
}

HRESULT CImageFile::DrawBackground(
    CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect, DTBGOPTS const* pOptions)
{
    if (!_fGlyphOnly) {
        TRUESTRETCHINFO tsi;
        DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, pRect, FALSE, &tsi);
        ENSURE_HR(DrawImageInfo(pdi, pRender, hdc, iStateId, pRect, pOptions, &tsi));
    }

    if (_eGlyphType == GT_NONE)
        return S_OK;

    RECT contentRect;
    ENSURE_HR(GetBackgroundContentRect(pRender, hdc, pRect, &contentRect));

    if (_eGlyphType == GT_FONTGLYPH)
        return DrawFontGlyph(pRender, hdc, &contentRect, pOptions);

    TRUESTRETCHINFO tsi;
    DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, &contentRect, TRUE, &tsi);
    return DrawImageInfo(pdi, pRender, hdc, iStateId, &contentRect, pOptions, &tsi);
}

HRESULT CImageFile::DrawImageInfo(
    DIBINFO* pdi, CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect,
    DTBGOPTS const* pOptions, TRUESTRETCHINFO* ptsInfo)
{
    TMBITMAPHEADER* pThemeBitmapHeader = nullptr;
    bool fStock = false;
    DWORD const flags = pOptions ? pOptions->dwFlags : 0;

    if (!pdi->uhbm.hBitmap) {
        if (pRender->_pbSharableData && pdi->iDibOffset > 0) {
            pThemeBitmapHeader = pRender->GetBitmapHeader(pdi->iDibOffset);
            if (pRender->BitmapIndexToHandle(pThemeBitmapHeader->iBitmapIndex))
                fStock = true;
        }

        if (!pRender->IsReady())
            return TRACE_HR(E_FAIL);

        if (!pThemeBitmapHeader)
            return TRACE_HR(E_FAIL);
    }

    SIZE drawSize;
    GetDrawnImageSize(pdi, pRect, ptsInfo, &drawSize);

    int v21 = 0;
    RECT rect = *pRect;
    if (rect.right - rect.left > drawSize.cx) {
        v21 = 1;
        switch (_eHAlign) {
        case HA_LEFT:
            rect.right = rect.left + drawSize.cx;
            break;
        case HA_CENTER:
            rect.left += (rect.right - rect.left - drawSize.cx) / 2;
            rect.right = rect.left + drawSize.cx;
            break;
        case HA_RIGHT:
            rect.left = rect.right - drawSize.cx;
            break;
        }
    }

    if (rect.bottom - rect.top > drawSize.cy) {
        switch (_eVAlign) {
        case VA_TOP:
            rect.bottom = rect.top + drawSize.cy;
            break;
        case VA_CENTER:
            rect.top += (rect.bottom - rect.top - drawSize.cy) / 2;
            rect.bottom = rect.top + drawSize.cy;
            break;
        case VA_BOTTOM:
            rect.top = rect.bottom - drawSize.cy;
            break;
        }
    } else if (v21) {
        if (!pdi->fBorderOnly && _fBgFill && !(flags & 8)) {
            HBRUSH solidBrush = CreateSolidBrush(_crFill);
            if (!solidBrush)
                return GetLastError();
            FillRect(hdc, pRect, solidBrush);
            DeleteObject(solidBrush);
        }
    }

    float xMarginFactor;
    float yMarginFactor;
    if (pdi->eSizingType == 0) {
        xMarginFactor = 1.0f;
        yMarginFactor = 1.0f;
    } else {
        ENSURE_HR(ScaleMargins(&_SizingMargins, hdc, pRender, pdi, &drawSize,
                               &xMarginFactor, &yMarginFactor));
    }

    return DrawBackgroundDS(
        pdi,
        pThemeBitmapHeader,
        fStock,
        pRender,
        hdc,
        iStateId,
        &rect,
        ptsInfo->fForceStretch,
        &_SizingMargins,
        xMarginFactor,
        yMarginFactor,
        pOptions);
}

static bool IsMirrored(HDC hdc)
{
    DWORD layout = GetLayout(hdc);
    return layout != -1 && layout & LAYOUT_RTL;
}

class CBitmapCache
{
public:
    CBitmapCache();
    ~CBitmapCache();
    HBITMAP AcquireBitmap(HDC hdc, int iWidth, int iHeight);
    void ReturnBitmap();

private:
    HBITMAP _hBitmap;
    int _iWidth;
    int _iHeight;
    CRITICAL_SECTION _csBitmapCache;
};

CBitmapCache::CBitmapCache()
    : _hBitmap(nullptr)
    , _iWidth(0)
    , _iHeight(0)
    , _csBitmapCache()
{
    InitializeCriticalSectionAndSpinCount(&_csBitmapCache, 0);
}

CBitmapCache::~CBitmapCache()
{
    if (_hBitmap)
        DeleteObject(_hBitmap);

    if (_csBitmapCache.DebugInfo) {
        DeleteCriticalSection(&_csBitmapCache);
        _csBitmapCache = {};
    }
}

HBITMAP CBitmapCache::AcquireBitmap(HDC hdc, int iWidth, int iHeight)
{
    if (this != (CBitmapCache *)-16 && _csBitmapCache.DebugInfo)
        EnterCriticalSection(&_csBitmapCache);

    if (iWidth > _iWidth || iHeight > _iHeight || !_hBitmap) {
        if (_hBitmap) {
            DeleteObject(_hBitmap);
            _hBitmap = nullptr;
            _iWidth = 0;
            _iHeight = 0;
        }

        BITMAPHEADER bmpInfo;
        bmpInfo.bmih.biSizeImage = 0;
        bmpInfo.bmih.biXPelsPerMeter = 0;
        bmpInfo.bmih.biYPelsPerMeter = 0;
        bmpInfo.bmih.biClrImportant = 0;
        bmpInfo.bmih.biCompression = 3;
        bmpInfo.bmih.biClrUsed = 3;
        bmpInfo.bmih.biSize = 0x28;
        bmpInfo.bmih.biWidth = iWidth;
        bmpInfo.bmih.biHeight = iHeight;
        bmpInfo.bmih.biPlanes = 1;
        bmpInfo.bmih.biBitCount = 32;
        bmpInfo.masks[0] = 0xFF0000;
        bmpInfo.masks[1] = 0xFF00;
        bmpInfo.masks[2] = 0xFF;

        _hBitmap = CreateDIBitmap(hdc, &bmpInfo.bmih, 2, nullptr, (const BITMAPINFO *)&bmpInfo, 0);
        if (_hBitmap) {
            _iWidth = iWidth;
            _iHeight = iHeight;
        } else if (_csBitmapCache.DebugInfo) {
            LeaveCriticalSection(&_csBitmapCache);
        }
    }

    return _hBitmap;
}

void CBitmapCache::ReturnBitmap()
{
    if (_hBitmap) {
        if (_csBitmapCache.DebugInfo)
            LeaveCriticalSection(&_csBitmapCache);
    }
}

static CBitmapCache g_pBitmapCacheScaled;
static CBitmapCache g_pBitmapCacheUnscaled;

static HBITMAP CreateScaledTempBitmap(
    HDC hdc, HBITMAP hSrcBitmap, int ixSrcOffset, int iySrcOffset,
    int iSrcWidth, int iSrcHeight, int iDestWidth, int iDestHeight)
{
    if (!hSrcBitmap)
        return nullptr;

    HBITMAP bmp = g_pBitmapCacheScaled.AcquireBitmap(hdc, iDestWidth, iDestHeight);
    if (!bmp)
        return nullptr;

    CompatibleDC hdcDest{hdc};
    if (!hdcDest)
        return bmp;

    SelectObjectScope<HBITMAP> oldDstBmp{hdcDest, bmp};
    CompatibleDC hdcSrc{hdc};
    if (!hdcSrc)
        return bmp;

    SetLayout(hdcSrc, 0);
    SetLayout(hdcDest, 0);

    SelectObjectScope<HBITMAP> oldSrcBmp{hdcSrc, hSrcBitmap};
    StretchBltModeScope oldStretchBltMode{hdcDest, COLORONCOLOR};

    StretchBlt(hdcDest, 0, 0, iDestWidth, iDestHeight, hdcSrc, ixSrcOffset,
               iySrcOffset, iSrcWidth, iSrcHeight, SRCCOPY);

    return bmp;
}

static HBITMAP CreateUnscaledBitmap(
    HDC hdc, HBITMAP hbmSrc, int cxSrcOffset, int cySrcOffset, int cxDest,
    int cyDest, bool fTemporary)
{
    if (!hbmSrc)
        return nullptr;

    HBITMAP hbmOut = g_pBitmapCacheUnscaled.AcquireBitmap(hdc, cxDest, cyDest);
    if (!hbmOut)
        return nullptr;

    CompatibleDC hdcOut{hdc};
    if (!hdcOut)
        return hbmOut;

    SelectObjectScope<HBITMAP> hbmOutScope{hdcOut, hbmOut};

    CompatibleDC hdcSrc{hdc};
    if (!hdcSrc)
        return hbmOut;

    SetLayout(hdcSrc, 0);
    SetLayout(hdcOut, 0);
    SelectObjectScope<HBITMAP> hbmSrcScope{hdcSrc, hbmSrc};

    BitBlt(hdcOut, 0, 0, cxDest, cyDest, hdcSrc, cxSrcOffset, cySrcOffset, SRCCOPY);

    return hbmOut;
}

static void StreamInit(BYTE**, HDC, HBITMAP, RECTL*)
{
    
}

HRESULT CImageFile::DrawBackgroundDS(
    DIBINFO* pdi, TMBITMAPHEADER* pThemeBitmapHeader, bool fStock,
    CRenderObj* pRender, HDC hdc, int iStateId, RECT* pRect,
    bool fForceStretch, MARGINS* pmarDest, float xMarginFactor,
    float yMarginFactor, DTBGOPTS const* pOptions) const
{
    HRESULT hr = S_OK;
    int tempSrcWidth = pdi->iSingleWidth;
    int tempSrcHeight = pdi->iSingleHeight;
    HBITMAP hBitmapStock = nullptr;
    HBITMAP hBitmapTempUnscaled = nullptr;
    HBITMAP hBitmapTempScaled = nullptr;
    HBITMAP hDsBitmap;
    GDIDRAWSTREAM gds = {};
    RECT dstRect;
    RECT clipRect;

    DWORD const flags = pOptions ? pOptions->dwFlags : 0;

    int offsetX;
    int offsetY;
    GetOffsets(iStateId, pdi, &offsetX, &offsetY);

    if (pThemeBitmapHeader) {
        hr = pRender->ExternalGetBitmap(hdc, pdi->iDibOffset, GBF_DIRECT, &hBitmapStock);
        if (FAILED(hr))
            goto done;

        hDsBitmap = hBitmapStock;
    } else {
        hBitmapTempUnscaled =
            CreateUnscaledBitmap(hdc, pdi->uhbm.hBitmap, offsetX, offsetY,
                                 pdi->iSingleWidth, pdi->iSingleHeight, false);
        if (!hBitmapTempUnscaled)
            return TRACE_HR(E_FAIL);

        hDsBitmap = hBitmapTempUnscaled;
        offsetX = 0;
        offsetY = 0;
    }

    if (xMarginFactor != 1.0 || yMarginFactor != 1.0) {
        int dpiX = 0;
        pRender->GetEffectiveDpi(hdc, &dpiX, nullptr);

        if (dpiX == GetScreenDpi()) {
            tempSrcWidth = _ScaledImageInfo.iSingleWidth;
            tempSrcHeight = _ScaledImageInfo.iSingleHeight;

            offsetX = MulDiv(offsetX, dpiX, 96);
            offsetY = MulDiv(offsetY, dpiX, 96);

            hr = pRender->ExternalGetBitmap(hdc, _ScaledImageInfo.iDibOffset,
                                            GBF_DIRECT, &hDsBitmap);
        } else {
            tempSrcWidth = static_cast<int>(pdi->iSingleWidth * xMarginFactor);
            tempSrcHeight = static_cast<int>(pdi->iSingleHeight * yMarginFactor);

            hBitmapTempScaled = CreateScaledTempBitmap(
                hdc, hDsBitmap, offsetX, offsetY, pdi->iSingleWidth,
                pdi->iSingleHeight, tempSrcWidth, tempSrcHeight);

            if (!hBitmapTempScaled) {
                hr = E_FAIL;
                TRACE_HR(hr);
                goto done;
            }

            hDsBitmap = hBitmapTempScaled;
            offsetX = 0;
            offsetY = 0;
        }
    }

    if (FAILED(hr))
        goto done;
    if (!hDsBitmap) {
        hr = E_FAIL;
        TRACE_HR(hr);
        goto done;
    }

    RECT srcRect;
    srcRect.left = offsetX;
    srcRect.right = offsetX + tempSrcWidth;
    srcRect.top = offsetY;
    srcRect.bottom = offsetY + tempSrcHeight;

    clipRect = *pRect;
    if (pRect->left > pRect->right) {
        clipRect.left = pRect->right;
        clipRect.right = pRect->left;
    }
    if (pRect->top > pRect->bottom) {
        clipRect.top = pRect->bottom;
        clipRect.bottom = pRect->top;
    }

    dstRect = clipRect;
    if (flags & DTBG_CLIPRECT)
        IntersectRect(&dstRect, &clipRect, &pOptions->rcClip);

    gds.signature = 0x44727753i64;
    gds.hImage = (DWORD)reinterpret_cast<uintptr_t>(hDsBitmap);
    gds.hDC = (DWORD)reinterpret_cast<uintptr_t>(hdc);
    gds.rcDest = dstRect;
    gds.one = 1;
    gds.nine = 9;
    gds.drawOption = 1;

    if (!fForceStretch) {
        switch (pdi->eSizingType) {
        case ST_TILE:
            gds.drawOption = 2;
            break;
        case ST_TRUESIZE:
            gds.drawOption = 32;
            break;
        default:
            break;
        }
    }

    if (pdi->fPartiallyTransparent != 0)
        gds.drawOption |= 4;

    if (flags & DTBG_MIRRORDC || IsMirrored(hdc)) {
        if (!(flags & DTBG_NOMIRROR) && _fMirrorImage) {
            gds.drawOption |= 16;
            if (!IsMirrored(hdc))
                std::swap(clipRect.left, clipRect.right);
        }
    }

    gds.rcClip = clipRect;
    gds.rcSrc = srcRect;
    if (pdi->eSizingType == ST_TRUESIZE) {
        gds.leftArcValue = 0;
        gds.rightArcValue = 0;
        gds.topArcValue = 0;
        gds.bottomArcValue = 0;
    } else {
        gds.leftArcValue = pmarDest->cxLeftWidth;
        gds.rightArcValue = pmarDest->cxRightWidth;
        gds.topArcValue = pmarDest->cyTopHeight;
        gds.bottomArcValue = pmarDest->cyBottomHeight;
    }

    GdiDrawStream(hdc, sizeof(gds), &gds);

done:
    if (hBitmapTempScaled)
        g_pBitmapCacheScaled.ReturnBitmap();
    if (hBitmapTempUnscaled)
        g_pBitmapCacheUnscaled.ReturnBitmap();

    if (hBitmapStock) {
        if (!fStock)
            pRender->ReturnBitmap(hBitmapStock);
    }

    return hr;
}

static void FixMarginOverlaps(int szDest, int& pm1, int& pm2)
{
    int total = pm1 + pm2;
    if (total <= szDest || total <= 0)
        return;

    pm1 = static_cast<int>(((szDest * pm1) / static_cast<float>(total)) + 0.5);
    pm2 = szDest - pm1;
}

struct NINEGRIDPOS
{
    int xLeft;
    int xRight;
    int yTop;
    int yBottom;
};

static HRESULT ScaleRectsAndCreateRegion(
    RGNDATA const* prd, RECT const* prcDest, MARGINS const* pMargins,
    SIZE const* psizeSrc, HRGN* phrgn)
{
    if (!prd)
        return E_POINTER;

    MARGINS margins = *pMargins;
    FixMarginOverlaps(prcDest->right - prcDest->left, margins.cxLeftWidth,
                      margins.cxRightWidth);
    FixMarginOverlaps(prcDest->bottom - prcDest->top, margins.cyTopHeight,
                      margins.cyBottomHeight);

    int sizeToAlloc = prd->rdh.nRgnSize + 32;
    if (sizeToAlloc < 32)
        return HRESULT_FROM_WIN32(ERROR_ARITHMETIC_OVERFLOW);

    auto dstBuffer = make_unique_nothrow<char[]>(sizeToAlloc);
    if (!dstBuffer)
        return E_OUTOFMEMORY;

    auto dstRgnData = reinterpret_cast<RGNDATA*>(dstBuffer.get());
    dstRgnData->rdh = {};
    dstRgnData->rdh.dwSize = sizeof(dstRgnData->rdh);
    dstRgnData->rdh.iType = RDH_RECTANGLES;
    dstRgnData->rdh.nCount = prd->rdh.nCount;
    SetRect(&dstRgnData->rdh.rcBound, -1, -1, -1, -1);

    long const v62 = std::max(margins.cxLeftWidth - 1, 0);
    long const v63 = std::max(margins.cyTopHeight - 1, 0);

    NINEGRIDPOS limit;
    limit.xLeft = prcDest->left + margins.cxLeftWidth;
    limit.yTop = prcDest->top + margins.cyTopHeight;
    limit.xRight = prcDest->right - margins.cxRightWidth;
    limit.yBottom = prcDest->bottom - margins.cyBottomHeight;

    long xLeftOffset = psizeSrc->cx - margins.cxRightWidth - margins.cxLeftWidth;
    long yTopOffset = psizeSrc->cy - margins.cyBottomHeight - margins.cyTopHeight;
    int v61 = limit.xRight - limit.xLeft;
    int v68 = limit.yBottom - limit.yTop;
    if (xLeftOffset == 0) {
        v61 = 0;
        xLeftOffset = 1;
    }

    if (!yTopOffset) {
        v68 = 0;
        yTopOffset = 1;
    }

    RECT v41;
    int v28 = 2 * prd->rdh.nCount;
    if (v28 > 0) {
        char const* v30 = &prd->Buffer[prd->rdh.nRgnSize];
        auto pptSrc = reinterpret_cast<POINT const*>(prd->Buffer);
        auto pptAlloc = reinterpret_cast<POINT*>(dstRgnData->Buffer);

        for (int i = v28; i; --i, ++pptSrc, ++pptAlloc, ++v30) {
            auto v310 = *v30;
            if (v310 == 0) {
                pptAlloc->x = prcDest->left + std::min(v62, pptSrc->x);
                pptAlloc->y = prcDest->top + std::min(v63, pptSrc->y);
            } else if (v310 == 1) {
                pptAlloc->x = limit.xLeft + v61 * pptSrc->x / xLeftOffset;
                pptAlloc->y = prcDest->top + std::min(v63, pptSrc->y);
            } else if (v310 == 2) {
                pptAlloc->x = limit.xRight + pptSrc->x + std::min(margins.cxRightWidth - pMargins->cxRightWidth, 0);
                pptAlloc->y = prcDest->top + std::min(v63, pptSrc->y);
            } else if (v310 == 3) {
                pptAlloc->x = prcDest->left + std::min(v62, pptSrc->x);
                pptAlloc->y = limit.yTop + v68 * pptSrc->y / yTopOffset;
            } else if (v310 == 4) {
                pptAlloc->x = limit.xLeft + v61 * pptSrc->x / xLeftOffset;
                pptAlloc->y = limit.yTop + v68 * pptSrc->y / yTopOffset;
            } else if (v310 == 5) {
                pptAlloc->x = limit.xRight + pptSrc->x + std::min(margins.cxRightWidth - pMargins->cxRightWidth, 0);
                pptAlloc->y = limit.yTop + v68 * pptSrc->y / yTopOffset;
            } else if (v310 == 6) {
                pptAlloc->x = prcDest->left + std::min(v62, pptSrc->x);
                pptAlloc->y = limit.yBottom + pptSrc->y + std::min(margins.cyBottomHeight - pMargins->cyBottomHeight, 0);
            } else if (v310 == 7) {
                pptAlloc->x = limit.xLeft + v61 * pptSrc->x / xLeftOffset;
                pptAlloc->y = limit.yBottom + pptSrc->y + std::min(margins.cyBottomHeight - pMargins->cyBottomHeight, 0);
            } else if (v310 == 8) {
                pptAlloc->x = limit.xRight + pptSrc->x + std::min(margins.cxRightWidth - pMargins->cxRightWidth, 0);
                pptAlloc->y = limit.yBottom + pptSrc->y + std::min(margins.cyBottomHeight - pMargins->cyBottomHeight, 0);
            }
        }
    }

    SetRect(&v41, -1, -1, -1, -1);
    RECT* v27 = (RECT*)dstRgnData->Buffer;
    for (int i = 0; i < dstRgnData->rdh.nCount; ++i)
        _InPlaceUnionRect(&v41, &v27[i]);

    dstRgnData->rdh.rcBound = v41;

    HRGN hrgn = ExtCreateRegion(nullptr, sizeToAlloc, dstRgnData);
    if (!hrgn)
        return MakeErrorLast();

    *phrgn = hrgn;
    return S_OK;
}

HRESULT CImageFile::GetBackgroundRegion(
    CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect, HRGN* pRegion)
{
    HRESULT hr = S_OK;
    DIBINFO const* pdi = SelectCorrectImageFile(pRender, hdc, pRect, 0, nullptr);

    if (pdi->iRgnListOffset != 0 && pRender->_pbSharableData) {
        auto rgnListHdr = reinterpret_cast<REGIONLISTHDR*>(&pRender->_pbSharableData[pdi->iRgnListOffset]);
        int* rgnList = (int*)((BYTE*)rgnListHdr + 8);

        if (iStateId > rgnListHdr->cStates - 1)
            iStateId = 0;

        if (auto iRgnDataOffset = rgnList[iStateId]) {
            auto rgnData = (RGNDATA*)(&pRender->_pbSharableData[iRgnDataOffset] + sizeof(ENTRYHDR));

            SIZE sz = {};
            MARGINS marDest = _SizingMargins;
            ScaleMargins(&marDest, hdc, pRender, pdi, &sz, nullptr, nullptr);

            SIZE szSrcImage{pdi->iSingleWidth, pdi->iSingleHeight};
            HRGN hrgn;
            hr = ScaleRectsAndCreateRegion(rgnData, pRect, &marDest, &szSrcImage, &hrgn);
            if (SUCCEEDED(hr))
                *pRegion = hrgn;
            return hr;
        }
    }

    HRGN hrgn = CreateRectRgn(pRect->left, pRect->top, pRect->right, pRect->bottom);
    if (!hrgn)
        return MakeErrorLast();

    *pRegion = hrgn;
    return hr;
}

HRESULT CImageFile::HitTestBackground(
    CRenderObj* pRender, HDC hdc, int iStateId, DWORD dwHTFlags,
    RECT const* pRect, HRGN hrgn, POINT ptTest, WORD* pwHitCode)
{
    *pwHitCode = 0;
    if (!PtInRect(pRect, ptTest))
        return S_OK;

    HRESULT hr = S_OK;

    GdiRegionHandle hrgnBk;
    if (!hrgn && _ImageInfo.fPartiallyTransparent) {
        hr = GetBackgroundRegion(pRender, hdc, iStateId, pRect, hrgnBk.CloseAndGetAddressOf());
        if (hr >= 0)
            hrgn = hrgnBk;
    }

    MARGINS margins = {};
    if (dwHTFlags & HTTB_SYSTEMSIZINGMARGINS && dwHTFlags & HTTB_RESIZINGBORDER
        && !(dwHTFlags & HTTB_SIZINGTEMPLATE)) {
        int cxBorder = GetSystemMetrics(SM_CXSIZEFRAME) + GetSystemMetrics(SM_CXPADDEDBORDER);
        if (dwHTFlags & HTTB_RESIZINGBORDER_LEFT)
            margins.cxLeftWidth = cxBorder;
        if (dwHTFlags & HTTB_RESIZINGBORDER_RIGHT)
            margins.cxRightWidth = cxBorder;
        if (dwHTFlags & HTTB_RESIZINGBORDER_TOP)
            margins.cyTopHeight = cxBorder;
        if (dwHTFlags & HTTB_RESIZINGBORDER_BOTTOM)
            margins.cyBottomHeight = cxBorder;
    } else {
        ENSURE_HR(GetScaledContentMargins(pRender, hdc, pRect, &margins));
    }

    if (hrgn) {
        RECT rgnBox;
        if (!GetRgnBox(hrgn, &rgnBox))
            return hr;
        if (dwHTFlags & HTTB_SIZINGTEMPLATE)
            *pwHitCode = HitTestTemplate(dwHTFlags, &rgnBox, hrgn, &margins, &ptTest);
        else
            *pwHitCode = HitTestRect(dwHTFlags, &rgnBox, &margins, &ptTest);
    } else {
        *pwHitCode = HitTestRect(dwHTFlags, pRect, &margins, &ptTest);
    }

    return hr;
}

HRESULT CImageFile::GetBackgroundExtent(
    CRenderObj* pRender, HDC hdc, RECT const* pContentRect, RECT* pExtentRect)
{
    MARGINS margins;
    ENSURE_HR(GetScaledContentMargins(pRender, hdc, pContentRect, &margins));
    pExtentRect->left = pContentRect->left - margins.cxLeftWidth;
    pExtentRect->top = pContentRect->top - margins.cyTopHeight;
    pExtentRect->right = margins.cxRightWidth + pContentRect->right;
    pExtentRect->bottom = margins.cyBottomHeight + pContentRect->bottom;
    return S_OK;
}

void CImageFile::GetOffsets(
    int iStateId, DIBINFO const* pdi, int* piXOffset, int* piYOffset) const
{
    if (_eImageLayout == IL_HORIZONTAL) {
        if (iStateId > 0 && iStateId <= _iImageCount)
            *piXOffset = pdi->iSingleWidth * (iStateId - 1);
        else
            *piXOffset = 0;
        *piYOffset = 0;
    } else {
        if (iStateId > 0 && iStateId <= _iImageCount)
            *piYOffset = pdi->iSingleHeight * (iStateId - 1);
        else
            *piYOffset = 0;
        *piXOffset = 0;
    }
}

HRESULT CImageFile::BuildRgnData(
    unsigned* prgdwPixels, int cWidth, int cHeight, DIBINFO* pdi,
    CRenderObj* pRender, int iStateId, RGNDATA** ppRgnData, int* piDataLen)
{
    int offsetX, offsetY;
    GetOffsets(iStateId, pdi, &offsetX, &offsetY);

    MARGINS margins = _SizingMargins;
    if (GetScreenDpi() != 96) {
        SIZE size = {};
        ScaleMargins(&margins, nullptr, pRender, pdi, &size, nullptr, nullptr);
    }

    GdiRegionHandle hRgn{
        _PixelsToRgn(prgdwPixels, offsetX, offsetY,
                     pdi->iSingleWidth > 0 ? pdi->iSingleWidth : cWidth,
                     pdi->iSingleHeight > 0 ? pdi->iSingleHeight : cHeight,
                     cWidth, cHeight, pdi)};
    if (!hRgn) {
        *ppRgnData = nullptr;
        *piDataLen = 0;
        return S_OK;
    }

    DWORD len = GetRegionData(hRgn, 0, nullptr);
    if (!len)
        return MakeErrorLast();

    if (len < sizeof(RGNDATAHEADER))
        assert("len >= sizeof(RGNDATAHEADER)");

    size_t dataLen = len + 2 * ((len - sizeof(RGNDATAHEADER)) / 16);
    auto rgnData = make_unique_malloc<RGNDATA>(dataLen);

    if (!GetRegionData(hRgn, len, rgnData.get()))
        return MakeErrorLast();

    RECT rc;
    SetRect(&rc, 0, 0, pdi->iSingleWidth, pdi->iSingleHeight);
    ENSURE_HR(pRender->PrepareRegionDataForScaling(rgnData.get(), &rc, &margins));

    *ppRgnData = rgnData.release();
    *piDataLen = dataLen;
    return S_OK;
}

} // namespace uxtheme
