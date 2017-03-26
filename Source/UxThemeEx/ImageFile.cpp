#include "ImageFile.h"

#include "DpiInfo.h"
#include "RenderObj.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"
#include "gdiex.h"

namespace uxtheme
{

static void AdjustSizeMin(SIZE* psz, int ixMin, int iyMin)
{
    if (psz->cx < ixMin)
        psz->cx = ixMin;
    if (psz->cy < iyMin)
        psz->cy = iyMin;
}

static void ScaleMargins(MARGINS* pMargins, unsigned targetDpi)
{
    if (!pMargins)
        return;

    pMargins->cxLeftWidth = MulDiv(pMargins->cxLeftWidth, targetDpi, 96);
    pMargins->cxRightWidth = MulDiv(pMargins->cxRightWidth, targetDpi, 96);
    pMargins->cyTopHeight = MulDiv(pMargins->cyTopHeight, targetDpi, 96);
    pMargins->cyBottomHeight = MulDiv(pMargins->cyBottomHeight, targetDpi, 96);
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
        return &static_cast<CMaxImageFile*>(this)->MultiDibs[iIndex];

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
        auto const multiDibs = static_cast<CMaxImageFile*>(this)->MultiDibs;
        for (int i = 0; i < _iMultiImageCount; ++i) {
            DIBINFO& multiDib = multiDibs[i];
            if (multiDib.iDibOffset != 0 && multiDib.iMinDpi > minDpi) {
                pdi = &multiDib;
                minDpi = multiDib.iMinDpi;
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
    HBITMAP__* v1; // rbx@1
    HDC v2; // r15@2
    HDC v3; // rax@2
    HDC v4; // r14@2
    int v5; // eax@4
    int v6; // eax@4
    int v7; // eax@4
    HBITMAP v8; // rax@4
    HBITMAP__* v9; // r12@4
    HGDIOBJ v10; // rsi@5
    HGDIOBJ v11; // rdi@5
    int v12; // ebx@5
    BITMAP v14; // [sp+60h] [bp-29h]@1
    _BITMAPHEADER v15; // [sp+80h] [bp-9h]@4

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
        pRender->_phBitmapsArray[hdr->iBitmapIndex].hBitmap = scaledDDB;
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
    DIBINFO* pImageInfo;
    int v19;
    int v20;
    DIBINFO* v21;
    int v22;
    HRESULT v32;

    v32 = 0;
    memset(this, 0, sizeof(CImageFile));
    _eBgType = BT_IMAGEFILE;
    pImageInfo = &_ImageInfo;
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
        iPartId, iStateId, TMT_TRUESIZESCALINGTYPE, (int *)&_eTrueSizeScalingType) < 0)
        _eTrueSizeScalingType = TSST_NONE;
    if (pRender->ExternalGetEnumValue(
        iPartId, iStateId, TMT_SIZINGTYPE, (int *)&_ImageInfo.eSizingType) < 0)
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
    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_HALIGN, (int *)&_eHAlign) < 0)
        _eHAlign = HA_CENTER;
    if (pRender->ExternalGetEnumValue(iPartId, iStateId, TMT_VALIGN, (int *)&_eVAlign) < 0)
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
            return v32;
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLYPHTEXTCOLOR, (int *)&_crGlyphTextColor) < 0)
            _crGlyphTextColor = 0;
        if (pRender->ExternalGetInt(iPartId, iStateId, TMT_GLYPHINDEX, &_iGlyphIndex) < 0)
            _iGlyphIndex = 1;
    } else if (_eGlyphType == GT_IMAGEGLYPH) {
        _GlyphInfo.iMinDpi = 96;
        _GlyphInfo.iDibOffset = pRender->GetValueIndex(iPartId, iStateId, 8);
        if (_GlyphInfo.iDibOffset == -1)
            _GlyphInfo.iDibOffset = 0;
        if (_GlyphInfo.iDibOffset > 0) {
            v32 = SetImageInfo(&_GlyphInfo, pRender, iPartId, iStateId);
            if (v32 < 0)
                return v32;
        }
        pRender->ExternalGetBool(
            iPartId, iStateId, TMT_GLYPHTRANSPARENT, &_GlyphInfo.fPartiallyTransparent);
        _GlyphInfo.eSizingType = ST_TRUESIZE;
        _GlyphInfo.fBorderOnly = FALSE;
    }

    if (_eGlyphType != 0 && pRender->ExternalGetBool(iPartId, iStateId, TMT_GLYPHONLY, &_fGlyphOnly) < 0)
        _fGlyphOnly = FALSE;

    if (pRender->ExternalGetEnumValue(
        iPartId, iStateId, TMT_IMAGESELECTTYPE, (int *)&_eImageSelectType) < 0)
        _eImageSelectType = IST_NONE;

    if (_eImageSelectType != 0) {
        if (_eGlyphType == 1)
            pImageInfo = &_GlyphInfo;

        v19 = 0;
        do
        {
            switch (v19) {
            case 0:
                v20 = 3;
                break;
            case 1:
                v20 = 4;
                break;
            case 2:
                v20 = 5;
                break;
            case 3:
                v20 = 6;
                break;
            case 4:
                v20 = 7;
                break;
            case 5:
                v20 = 22;
                break;
            default:
                if (v19 != 6)
                    assert("FRE: FALSE");
                v20 = 23;
                break;
            }

            v22 = pRender->GetValueIndex(iPartId, iStateId, v20);
            if (v22 == -1)
                break;

            ++_iMultiImageCount;
            v21 = static_cast<CMaxImageFile*>(this)->MultiDibPtr(v19);
            *v21 = *pImageInfo;
            v21->iDibOffset = v22;
            v32 = SetImageInfo(v21, pRender, iPartId, iStateId);
            if (v32 < 0)
                return v32;

            if (pRender->ExternalGetInt(iPartId, iStateId,
                                        Map_Ordinal_To_MINDPI(v19), &v21->iMinDpi) < 0) {
                v21->iMinDpi = 96;
            } else {
                if (v21->iMinDpi < 1)
                    v21->iMinDpi = 1;
            }

            if (pRender->ExternalGetPosition(iPartId, iStateId,
                                             Map_Ordinal_To_MINSIZE(v19),
                                             (POINT*)&v21->szMinSize) < 0) {
                v21->szMinSize.cx = v21->iSingleWidth;
                v21->szMinSize.cy = v21->iSingleHeight;
            } else {
                AdjustSizeMin(&v21->szMinSize, 1, 1);
            }

            ++v19;
        } while (v19 < 7);

        if (_iMultiImageCount > 0) {
            auto p = static_cast<CMaxImageFile*>(this)->MultiDibPtr(0);
            *pImageInfo = *p;
        }
    }
    return v32;
}

static int const rgPropIds[] = {
    TMT_DIBDATA, TMT_3, TMT_4, TMT_5, TMT_6, TMT_7, TMT_22, TMT_23,
    TMT_GLYPHDIBDATA, TMT_HBITMAP
};

HRESULT CImageFile::GetBitmap(
    CRenderObj* pRender, int iPropId, unsigned int dwFlags, HBITMAP* phBitmap)
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
    CRenderObj* pRender, HDC hdc, RECT* prc, THEMESIZE eSize, SIZE* psz)
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
            double w = static_cast<double>(pdi->iSingleWidth);
            double h = static_cast<double>(pdi->iSingleHeight);
            double scaleX = pszDraw->cx / w;
            double scaleY = pszDraw->cy / h;
            if (scaleY < scaleX)
                pszDraw->cx = static_cast<int>(w * scaleY);
            else if (scaleX < scaleY)
                pszDraw->cy = static_cast<int>(h * scaleX);
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

    if (partiallyTransparent && pRender->ExternalGetBool(iPartId, iStateId, TMT_ALPHATHRESHOLD, &pdi->iAlphaThreshold) < 0)
        pdi->iAlphaThreshold = 255;

    HBITMAP hbmp = pRender->_phBitmapsArray[tmhdr->iBitmapIndex].hBitmap;

    int width, height;
    if (hbmp) {
        BITMAP bitmap;
        if (!GetObjectW(hbmp, 32, &bitmap))
            return E_FAIL;
        width = bitmap.bmWidth;
        height = bitmap.bmHeight;
    } else {
        auto bmphdr = (_BITMAPHEADER*)((char*)tmhdr + tmhdr->dwSize);
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
    bool cleanupDC = false;
    if (!hdcOrig) {
        hdcOrig = GetWindowDC(nullptr);
        if (hdcOrig)
            cleanupDC = true;
    }

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
                if (hdcOrig && (!v18 && !v19)) {
                    int dpiX = 0;
                    pRender->GetEffectiveDpi(hdcOrig, &dpiX, nullptr);
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

    if (cleanupDC)
        ReleaseDC(nullptr, hdcOrig);
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

    return ScaleMargins(pMargins, hdc, pRender, pdi, &size, 0i64, 0i64);
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
    CRenderObj* pRender, HDC hdc, RECT const* prc, int fForGlyph, TRUESTRETCHINFO* ptsInfo)
{
    DIBINFO* dibInfo; // r13@2
    signed int v11; // er9@3
    DIBINFO* v12; // rsi@3
    signed int v13; // er12@3
    int v14; // er14@3
    int v15; // er15@3
    signed int v17; // eax@9
    signed int v18; // edx@9
    int v23; // eax@36
    int v24; // er8@36
    int v25; // edx@36
    int v26; // ecx@37
    int v28; // er9@37
    int v29; // eax@37
    int dpiX; // edi@47
    int dpiY; // eax@48
    int v33; // er10@49
    int v35; // edx@50
    char* v36; // r8@51
    DIBINFO* v37; // rax@52
    int v38; // ecx@54
    int v41; // edx@64
    tagSIZE drawSize; // [sp+78h] [bp+20h]@37
    int dpiX_1; // [sp+78h] [bp+20h]@76

    if (fForGlyph)
        dibInfo = &_GlyphInfo;
    else
        dibInfo = &_ImageInfo;
    v11 = 0;
    v12 = 0i64;
    fForGlyph = 0;
    v14 = 1;
    v15 = 1;

    v13 = 0;
    if (!hdc) {
        hdc = GetWindowDC(nullptr);
        v11 = 0;
        if (hdc)
            v13 = 1;
    }

    if (prc) {
        v14 = prc->right - prc->left;
        v15 = prc->bottom - prc->top;
    }

    if (!fForGlyph && _ImageInfo.eSizingType)
        goto LABEL_89;

    if (pRender && pRender->_dwOtdFlags & OTD_FORCE_RECT_SIZING) {
        v11 = 1;
        fForGlyph = 1;
    }

    if (!fForGlyph) {
    LABEL_89:
        if (_eGlyphType == 1)
            goto LABEL_16;
    }

    v17 = 0;
    v18 = 0;
    if (_eImageSelectType == 2)
        v17 = 1;
    if ((v11 || _eImageSelectType == 1 || _fSourceGrow) && prc)
        v18 = 1;
    if (!v17) {
        if (v18) {
            if (_iMultiImageCount) {
                v41 = _iMultiImageCount - 1;
                if (_iMultiImageCount - 1 >= 0) {
                    while (1)
                    {
                        v12 = 0i64;
                        if (v41 >= 0 || v41 < _iMultiImageCount)
                            v12 = &static_cast<CMaxImageFile*>(this)->MultiDibs[v41];
                        if (v12->szMinSize.cx <= v14 && v12->szMinSize.cy <= v15)
                            break;
                        if (--v41 < 0)
                            goto LABEL_16;
                    }

                    if (v12)
                        goto LABEL_17;
                    goto LABEL_16;
                }
            }
        }
        goto LABEL_16;
    }
    if (!hdc)
        goto LABEL_16;

    dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
    if (GetScreenDpi() == dpiX || g_fForcedDpi) {
        dpiY = pRender->_iAssociatedDpi;
        v33 = dpiY;
    } else {
        dpiX_1 = GetDeviceCaps(hdc, LOGPIXELSX);
        dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
        v33 = dpiX_1;
        if (dpiX_1 >= dpiY)
            v33 = dpiY;
    }

    v35 = _iMultiImageCount - 1;
    if (_iMultiImageCount - 1 >= 0) {
        v36 = (char *)&static_cast<CMaxImageFile*>(this)->MultiDibs[v35];
        do
        {
            v37 = 0i64;
            if (v35 >= 0 || v35 < _iMultiImageCount)
                v37 = (DIBINFO *)v36;
            v38 = v37->iMinDpi;
            if (v38 <= v33 && (!v12 || v12->iMinDpi < v38))
                v12 = v37;
            v36 -= 56;
            --v35;
        } while (v35 >= 0);
        if (!v12)
            v12 = dibInfo;
    } else {
    LABEL_16:
        v12 = dibInfo;
    }

LABEL_17:
    if (ptsInfo) {
        ptsInfo->fForceStretch = 0;
        ptsInfo->fFullStretch = 0;
        ptsInfo->szDrawSize = SIZE();
        if (v12->eSizingType == ST_TRUESIZE) {
            if (_eTrueSizeScalingType != 0) {
                if (prc && (fForGlyph || v12->iSingleWidth > v14 || v12->iSingleHeight > v15)) {
                    ptsInfo->fFullStretch = 1;
                    ptsInfo->szDrawSize.cx = v14;
                    ptsInfo->szDrawSize.cy = v15;
                    ptsInfo->fForceStretch = 1;
                } else {
                    if (_eTrueSizeScalingType == 2) {
                        if (!hdc)
                            goto LABEL_19;
                        fForGlyph = 0;
                        int ptsInfo_ = 0;
                        pRender->GetEffectiveDpi(hdc, &fForGlyph, &ptsInfo_);
                        v23 = MulDiv(v12->iSingleWidth, fForGlyph, v12->iMinDpi);
                        v24 = v12->iMinDpi;
                        v25 = ptsInfo_;
                        ptsInfo = (TRUESTRETCHINFO*)ptsInfo_;
                    } else {
                        if (_eTrueSizeScalingType != 1 || !prc)
                            goto LABEL_19;
                        v23 = MulDiv(v12->iSingleWidth, v14, v12->szMinSize.cx);
                        v24 = v12->szMinSize.cy;
                        v25 = v15;
                    }

                    drawSize.cx = v23;
                    drawSize.cy = MulDiv(v12->iSingleHeight, v25, v24);
                    if (drawSize.cx) {
                        if (prc) {
                            if (drawSize.cx >= v14) {
                                drawSize.cx = v14;
                                drawSize.cx = v14;
                            }
                            if (drawSize.cy >= v15) {
                                drawSize.cy = v15;
                                drawSize.cy = v15;
                            }
                        }

                        if (100 * (drawSize.cx - v12->iSingleWidth) / v12->iSingleWidth >= _iTrueSizeStretchMark &&
                            100 * (drawSize.cy - v12->iSingleHeight) / v12->iSingleHeight >= _iTrueSizeStretchMark)
                        {
                            ptsInfo->szDrawSize = drawSize;
                            ptsInfo->fForceStretch = 1;
                        }
                    }
                }
            }
        }
    }
LABEL_19:
    if (!v12)
        v12 = dibInfo;
    if (v13)
        ReleaseDC(nullptr, hdc);
    return v12;
}

HRESULT CImageFile::DrawFontGlyph(CRenderObj* pRender, HDC__* hdc, tagRECT* prc, DTBGOPTS const* pOptions)
{
    // FIXME
    return S_OK;
}

HRESULT CImageFile::DrawBackground(
    CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect, DTBGOPTS const* pOptions)
{
    if (!_fGlyphOnly) {
        TRUESTRETCHINFO tsi;
        DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, pRect, 0, &tsi);
        ENSURE_HR(DrawImageInfo(pdi, pRender, hdc, iStateId, pRect, pOptions, &tsi));
    }

    if (_eGlyphType != 0) {
        RECT contentRect;
        ENSURE_HR(GetBackgroundContentRect(pRender, hdc, pRect, &contentRect));

        if (_eGlyphType == 2)
            return DrawFontGlyph(pRender, hdc, &contentRect, pOptions);

        TRUESTRETCHINFO tsi;
        DIBINFO* pdi = SelectCorrectImageFile(pRender, hdc, &contentRect, 1, &tsi);
        return DrawImageInfo(pdi, pRender, hdc, iStateId, &contentRect, pOptions, &tsi);
    }

    return S_OK;
}

HRESULT CImageFile::DrawImageInfo(
    DIBINFO* pdi, CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect,
    DTBGOPTS const* pOptions, TRUESTRETCHINFO* ptsInfo)
{
    TMBITMAPHEADER* pThemeBitmapHeader; // r12@1
    unsigned int v13; // er11@2
    int width; // er15@15
    int height; // er14@15
    signed int v21; // er9@16
    float xMarginFactor; // xmm7_4@24
    float yMarginFactor; // xmm8_4@24
    int fStock; // [sp+70h] [bp-59h]@1
    RECT rect;

    pThemeBitmapHeader = nullptr;
    fStock = 0;
    v13 = 0;
    if (pOptions)
        v13 = pOptions->dwFlags;

    if (!pdi->uhbm.hBitmap) {
        if (pRender->_pbSharableData) {
            if (pdi->iDibOffset > 0) {
                pThemeBitmapHeader = pRender->GetBitmapHeader(pdi->iDibOffset);
                fStock = 0;
                if (pRender->_phBitmapsArray[pThemeBitmapHeader->iBitmapIndex].hBitmap)
                    fStock = 1;
            }
        }

        if (pRender->_pThemeFile) {
            auto v17 = (NONSHARABLEDATAHDR*)pRender->_pThemeFile->_pbNonSharableData;
            if (!v17 || !(v17->dwFlags & 1))
                return TRACE_HR(E_FAIL);
        }

        if (!pThemeBitmapHeader)
            return TRACE_HR(E_FAIL);
    }

    if (pdi->eSizingType) {
        if (pRect) {
            width = pRect->right - pRect->left;
            height = pRect->bottom - pRect->top;
        } else {
            width = 0;
            height = 0;
        }
    } else {
        if (!ptsInfo->fForceStretch) {
            width = pdi->iSingleWidth;
            height = pdi->iSingleHeight;
        } else if (!_fIntegralSizing || ptsInfo->fFullStretch) {
            width = ptsInfo->szDrawSize.cx;
            height = ptsInfo->szDrawSize.cy;
        } else {
            width = pdi->iSingleWidth * (int)((float)ptsInfo->szDrawSize.cx / (float)pdi->iSingleWidth);
            height = pdi->iSingleHeight * (int)((float)ptsInfo->szDrawSize.cy / (float)pdi->iSingleHeight);
        }

        if (_fUniformSizing) {
            double v34 = static_cast<double>(pdi->iSingleWidth);
            double v35 = static_cast<double>(pdi->iSingleHeight);
            double v36 = width / v34;
            double v37 = height / v35;
            if (v37 > v36) {
                height = (int)(v35 * v36);
            } else if (v36 > v37) {
                width = (int)(v34 * v37);
            }
        }
    }

    v21 = 1;
    rect = *pRect;
    if (rect.right - rect.left > width) {
        v21 = 0;
        if (_eHAlign == 0) {
            rect.right = rect.left + width;
        } else if (_eHAlign == 1) {
            rect.left += (rect.right - rect.left - width) / 2;
            rect.right = rect.left + width;
        } else {
            rect.left = rect.right - width;
        }
    }

    if (rect.bottom - rect.top > height) {
        if (_eVAlign == 0) {
            rect.bottom = rect.top + height;
        } else if (_eVAlign == 1) {
            rect.top += (rect.bottom - rect.top - height) / 2;
            rect.bottom = rect.top + height;
        } else {
            rect.top = rect.bottom - height;
        }
    } else if (!v21) {
        if (!pdi->fBorderOnly && _fBgFill && !(v13 & 8)) {
            HBRUSH solidBrush = CreateSolidBrush(_crFill);
            if (!solidBrush)
                return GetLastError();
            FillRect(hdc, pRect, solidBrush);
            DeleteObject(solidBrush);
        }
    }

    if (pdi->eSizingType == 0) {
        xMarginFactor = 1.0f;
        yMarginFactor = 1.0f;
    } else {
        SIZE const drawSize = {width, height};
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

struct CBitmapCache
{
    HBITMAP _hBitmap;
    int _iWidth;
    int _iHeight;
    CRITICAL_SECTION _csBitmapCache;

    CBitmapCache();
    ~CBitmapCache();
    HBITMAP AcquireBitmap(HDC hdc, int iWidth, int iHeight);
    void ReturnBitmap();
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

        _BITMAPHEADER bmpInfo;
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

HBITMAP CreateScaledTempBitmap(
    HDC hdc, HBITMAP hSrcBitmap, int ixSrcOffset, int iySrcOffset,
    int iSrcWidth, int iSrcHeight, int iDestWidth, int iDestHeight)
{
    if (!hSrcBitmap)
        return nullptr;

    HBITMAP bmp = g_pBitmapCacheScaled.AcquireBitmap(hdc, iDestWidth, iDestHeight);
    if (!bmp)
        return nullptr;

    HDC hdcDest = CreateCompatibleDC(hdc);
    if (!hdcDest)
        return bmp;

    HGDIOBJ oldDstBmp = SelectObject(hdcDest, bmp);
    HDC hdcSrc = CreateCompatibleDC(hdc);

    if (hdcSrc) {
        SetLayout(hdcSrc, 0);
        SetLayout(hdcDest, 0);

        HGDIOBJ oldSrcBmp = SelectObject(hdcSrc, hSrcBitmap);
        int oldStretchBltMode = SetStretchBltMode(hdcDest, COLORONCOLOR);

        StretchBlt(hdcDest, 0, 0, iDestWidth, iDestHeight, hdcSrc, ixSrcOffset,
                   iySrcOffset, iSrcWidth, iSrcHeight, SRCCOPY);

        SetStretchBltMode(hdcDest, oldStretchBltMode);
        SelectObject(hdcSrc, oldSrcBmp);
        DeleteDC(hdcSrc);
    }

    SelectObject(hdcDest, oldDstBmp);
    DeleteDC(hdcDest);

    return bmp;
}

static HBITMAP CreateUnscaledBitmap(
    HDC hdc, HBITMAP hbmSrc, int cxSrcOffset, int cySrcOffset, int cxDest, int cyDest, int fTemporary)
{
    HBITMAP__* v7; // rsi@1
    HDC v10; // rax@3
    HDC v11; // rdi@3
    HGDIOBJ v12; // r14@4
    HDC v13; // rax@4
    HDC v14; // rbp@4
    HGDIOBJ v15; // rbx@5

    v7 = 0i64;
    if (hbmSrc) {
        v7 = g_pBitmapCacheUnscaled.AcquireBitmap(hdc, cxDest, cyDest);
        if (v7)
        {
            v10 = CreateCompatibleDC(hdc);
            v11 = v10;
            if (v10)
            {
                v12 = SelectObject(v10, v7);
                v13 = CreateCompatibleDC(hdc);
                v14 = v13;
                if (v13)
                {
                    SetLayout(v13, 0);
                    SetLayout(v11, 0);
                    v15 = SelectObject(v14, hbmSrc);
                    BitBlt(v11, 0, 0, cxDest, cyDest, v14, cxSrcOffset, cySrcOffset, SRCCOPY);
                    SelectObject(v14, v15);
                    DeleteDC(v14);
                }
                SelectObject(v11, v12);
                DeleteDC(v11);
            }
        }
    }
    return v7;
}

HRESULT CImageFile::DrawBackgroundDS(
    DIBINFO* pdi, TMBITMAPHEADER* pThemeBitmapHeader, int fStock,
    CRenderObj* pRender, HDC hdc, int iStateId, RECT* pRect,
    int fForceStretch, MARGINS* pmarDest, float xMarginFactor,
    float yMarginFactor, DTBGOPTS const* pOptions)
{
    int offsetY; // esi@1
    HBITMAP__* v19; // r15@1
    HRESULT hr; // er13@1 MAPDST
    int v23; // ebx@1
    int offsetX; // eax@5
    HBITMAP__* v28; // rbx@11
    RECT dstRect; // xmm0@22
    HBITMAP__* v52; // rax@62
    HBITMAP__* v65; // [sp+78h] [bp-88h]@1
    HBITMAP__* v66; // [sp+80h] [bp-80h]@1

    GDIDRAWSTREAM gds = {};
    v19 = 0i64;
    hr = 0;
    int unkWidth = pdi->iSingleWidth;
    int unkHeight = pdi->iSingleHeight;
    v65 = 0i64;
    v66 = 0i64;

    offsetX = 0;
    offsetY = 0;
    if (_eImageLayout == IL_HORIZONTAL) {
        if (iStateId > 0 && iStateId <= _iImageCount)
            offsetX = pdi->iSingleWidth * (iStateId - 1);
    } else {
        if (iStateId > 0 && iStateId <= _iImageCount)
            offsetY = pdi->iSingleHeight * (iStateId - 1);
    }

    if (pThemeBitmapHeader) {
        hr = pRender->ExternalGetBitmap(hdc, pdi->iDibOffset, GBF_DIRECT, &v19);
        if (FAILED(hr))
            goto LABEL_38;

        v28 = v19;
    } else {
        v66 = CreateUnscaledBitmap(hdc, pdi->uhbm.hBitmap, offsetX, offsetY,
                                   pdi->iSingleWidth, pdi->iSingleHeight, 0);
        v28 = v66;
        if (!v66)
            return TRACE_HR(E_FAIL);
        offsetY = 0;
        offsetX = 0;
    }

    if (xMarginFactor != 1.0 || yMarginFactor != 1.0) {
        int dpiX = 0;
        pRender->GetEffectiveDpi(hdc, &dpiX, nullptr);

        if (dpiX == GetScreenDpi()) {
            unkWidth = _ScaledImageInfo.iSingleWidth;
            unkHeight = _ScaledImageInfo.iSingleHeight;

            offsetX = MulDiv(offsetX, dpiX, 96);
            offsetY = MulDiv(offsetY, dpiX, 96);

            hr = pRender->ExternalGetBitmap(hdc, _ScaledImageInfo.iDibOffset, GBF_DIRECT, &v28);
        } else {
            unkWidth = static_cast<int>(pdi->iSingleWidth * xMarginFactor);
            unkHeight = static_cast<int>(pdi->iSingleHeight * yMarginFactor);
            v52 = CreateScaledTempBitmap(hdc, v28, offsetX, offsetY, pdi->iSingleWidth, pdi->iSingleHeight, unkWidth, unkHeight);
            v65 = v52;
            if (!v52) {
                hr = E_FAIL;
                TRACE_HR(hr);
                goto LABEL_36;
            }

            offsetY = 0;
            v28 = v52;
            offsetX = 0;
        }
    }

    if (hr < 0)
        goto exit_15;
    if (!v28) {
        hr = E_FAIL;
        TRACE_HR(hr);
        goto exit_15;
    }

    RECT srcRect;
    srcRect.left = offsetX;
    srcRect.right = offsetX + unkWidth;
    srcRect.top = offsetY;
    srcRect.bottom = offsetY + unkHeight;

    RECT clipRect = *pRect;
    if (pRect->left > pRect->right) {
        clipRect.left = pRect->right;
        clipRect.right = pRect->left;
    }
    if (pRect->top > pRect->bottom) {
        clipRect.top = pRect->bottom;
        clipRect.bottom = pRect->top;
    }

    DWORD const flags = pOptions ? pOptions->dwFlags : 0;

    dstRect = clipRect;
    if (flags & DTBG_CLIPRECT)
        IntersectRect(&dstRect, &clipRect, &pOptions->rcClip);

    gds.signature = 0x44727753i64;
    gds.hImage = (DWORD)v28;
    gds.hDC = (DWORD)hdc;
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

exit_15:
    if (v65)
        g_pBitmapCacheScaled.ReturnBitmap();

LABEL_36:
    if (v66)
        g_pBitmapCacheUnscaled.ReturnBitmap();

LABEL_38:
    if (v19) {
        if (!fStock)
            DeleteObject(v19);
    }

    return hr;
}

} // namespace uxtheme
