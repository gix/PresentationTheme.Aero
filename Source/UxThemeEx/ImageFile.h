#pragma once
#include "DrawBase.h"
#include "Primitives.h"
#include "VSUnpack.h"
#include <uxtheme.h>
#include <vssym32.h>
#include <windows.h>

namespace uxtheme
{

class CRenderObj;

class CImageFile : public CDrawBase
{
public:
    static BOOL KeyProperty(int iPropId);

    HRESULT SetImageInfo(DIBINFO* pdi, CRenderObj const* pRender, int iPartId,
                         int iStateId);
    HRESULT ScaleMargins(MARGINS* pMargins, HDC hdcOrig, CRenderObj* pRender,
                         DIBINFO const* pdi, SIZE const* pszDraw, float* pfx, float* pfy);
    HRESULT GetScaledContentMargins(CRenderObj* pRender, HDC hdc, RECT const* prcDest,
                                    MARGINS* pMargins);
    HRESULT GetBackgroundContentRect(CRenderObj* pRender, HDC hdc,
                                     RECT const* pBoundingRect, RECT* pContentRect);
    DIBINFO* SelectCorrectImageFile(CRenderObj* pRender, HDC hdc, RECT const* prc,
                                    bool fForGlyph, TRUESTRETCHINFO* ptsInfo);
    HRESULT DrawFontGlyph(CRenderObj* pRender, HDC hdc, RECT* prc,
                          DTBGOPTS const* pOptions);
    HRESULT DrawBackground(CRenderObj* pRender, HDC hdc, int iStateId, RECT const* pRect,
                           DTBGOPTS const* pOptions);
    HRESULT DrawImageInfo(DIBINFO* pdi, CRenderObj* pRender, HDC hdc, int iStateId,
                          RECT const* pRect, DTBGOPTS const* pOptions,
                          TRUESTRETCHINFO* ptsInfo);
    HRESULT DrawBackgroundDS(DIBINFO* pdi, TMBITMAPHEADER* pThemeBitmapHeader,
                             bool fStock, CRenderObj* pRender, HDC hdc, int iStateId,
                             RECT* pRect, bool fForceStretch, MARGINS* pmarDest,
                             float xMarginFactor, float yMarginFactor,
                             DTBGOPTS const* pOptions) const;
    HRESULT GetBackgroundRegion(CRenderObj* pRender, HDC hdc, int iStateId,
                                RECT const* pRect, HRGN* pRegion);
    HRESULT HitTestBackground(CRenderObj* pRender, HDC hdc, int iStateId, DWORD dwHTFlags,
                              RECT const* pRect, HRGN hrgn, POINT ptTest,
                              WORD* pwHitCode);
    HRESULT GetBackgroundExtent(CRenderObj* pRender, HDC hdc, RECT const* pContentRect,
                                RECT* pExtentRect);
    void GetOffsets(int iStateId, DIBINFO const* pdi, int* piXOffset,
                    int* piYOffset) const;
    HRESULT BuildRgnData(DWORD* prgdwPixels, int cWidth, int cHeight, DIBINFO* pdi,
                         CRenderObj* pRender, int iStateId, RGNDATA** ppRgnData,
                         int* piDataLen);
    HRESULT PackProperties(CRenderObj* pRender, int iPartId, int iStateId);
    HRESULT GetBitmap(CRenderObj* pRender, int iPropId, unsigned dwFlags,
                      HBITMAP* phBitmap);
    HRESULT GetPartSize(CRenderObj* pRender, HDC hdc, RECT const* prc, THEMESIZE eSize,
                        SIZE* psz);
    void GetDrawnImageSize(DIBINFO const* pdi, RECT const* pRect,
                           TRUESTRETCHINFO const* ptsInfo, SIZE* pszDraw);
    HRESULT CreateScaledBackgroundImage(CRenderObj* pRender, int iPartId, int iStateId,
                                        DIBINFO** pScaledDibInfo);
    DIBINFO* EnumImageFiles(int iIndex);
    BOOL HasSizingMargin() const;
    DIBINFO* FindBackgroundImageToScale();

    DIBINFO _ImageInfo;
    DIBINFO _ScaledImageInfo;
    int _iMultiImageCount;
    IMAGESELECTTYPE _eImageSelectType;
    int _iImageCount;
    IMAGELAYOUT _eImageLayout;
    BOOL _fMirrorImage;
    TRUESIZESCALINGTYPE _eTrueSizeScalingType;
    HALIGN _eHAlign;
    VALIGN _eVAlign;
    BOOL _fBgFill;
    COLORREF _crFill;
    int _iTrueSizeStretchMark;
    BOOL _fUniformSizing;
    BOOL _fIntegralSizing;
    MARGINS _SizingMargins;
    MARGINS _ContentMargins;
    BOOL _fSourceGrow;
    BOOL _fSourceShrink;
    BOOL _fGlyphOnly;
    GLYPHTYPE _eGlyphType;
    COLORREF _crGlyphTextColor;
    unsigned short _iGlyphFontIndex;
    int _iGlyphIndex;
    DIBINFO _GlyphInfo;
    int _iSourcePartId;
    int _iSourceStateId;
};

class CMaxImageFile : public CImageFile
{
public:
    CMaxImageFile() { memset(MultiDibs, 0, sizeof(MultiDibs)); }

    DIBINFO* MultiDibPtr(int iIndex)
    {
        if (iIndex >= 0 || iIndex < _iMultiImageCount)
            return &MultiDibs[iIndex];
        return nullptr;
    }

private:
    DIBINFO MultiDibs[7];
};

} // namespace uxtheme
