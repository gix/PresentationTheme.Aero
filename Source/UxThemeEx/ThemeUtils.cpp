#include "ThemeUtils.h"
#include <intsafe.h>

namespace uxtheme
{

HRESULT BitmapPixels::OpenBitmap(HDC hdc, HBITMAP bitmap, bool fForceRGB32,
                                 unsigned** pPixels, int* piWidth,
                                 int* piHeight, int* piBytesPerPixel,
                                 int* piBytesPerRow,
                                 int* piPreviousBytesPerPixel,
                                 unsigned cbBytesBefore)
{
    if (!pPixels)
        return E_INVALIDARG;

    OptionalDC screenDC{hdc};
    if (!screenDC)
        return MakeErrorLast();

    BITMAP bminfo;
    GetObjectW(bitmap, sizeof(BITMAP), &bminfo);

    _iWidth = bminfo.bmWidth;
    _iHeight = bminfo.bmHeight;

    unsigned bytesPerRow;
    if (FAILED(UIntMult(4u, bminfo.bmWidth, &bytesPerRow)) || bytesPerRow > 0x7FFFFFFC)
        return E_OUTOFMEMORY;

    bytesPerRow = AlignPower2<4>(bytesPerRow);
    unsigned cbPixels;
    if (FAILED(UIntMult(AlignPower2<4>(bytesPerRow), bminfo.bmHeight, &cbPixels)) ||
        FAILED(UIntAdd(cbPixels, sizeof(BITMAPINFOHEADER) + 100, &cbPixels)))
        return E_OUTOFMEMORY;

    _buffer = make_unique_malloc<BYTE[]>(cbPixels);
    if (!_buffer)
        return E_OUTOFMEMORY;

    _hdrBitmap = reinterpret_cast<BITMAPINFOHEADER*>(_buffer.get());
    fill_zero(*_hdrBitmap);
    _hdrBitmap->biSize = sizeof(BITMAPINFOHEADER);
    _hdrBitmap->biWidth = _iWidth;
    _hdrBitmap->biHeight = _iHeight;
    _hdrBitmap->biPlanes = 1;
    _hdrBitmap->biBitCount = 32;
    _hdrBitmap->biCompression = 0;

    if (!GetDIBits(screenDC, bitmap, 0, _iHeight, GetBitmapBits(_hdrBitmap),
                   reinterpret_cast<LPBITMAPINFO>(_hdrBitmap), DIB_RGB_COLORS))
        return E_FAIL;

    *pPixels = reinterpret_cast<unsigned*>(GetBitmapBits(_hdrBitmap));

    if (piWidth)
        *piWidth = _iWidth;
    if (piHeight)
        *piHeight = _iHeight;
    if (piBytesPerPixel)
        *piBytesPerPixel = 4;
    if (piBytesPerRow)
        *piBytesPerRow = bytesPerRow;

    return S_OK;
}

void BitmapPixels::CloseBitmap(HDC hdc, HBITMAP hBitmap)
{
    if (!_hdrBitmap || !_buffer)
        return;

    if (hBitmap) {
        HDC screenDC = GetWindowDC(nullptr);
        SetDIBits(
            screenDC,
            hBitmap,
            0,
            _iHeight,
            (BYTE*)_hdrBitmap + 4 * _hdrBitmap->biClrUsed + _hdrBitmap->biSize,
            (const BITMAPINFO *)_hdrBitmap,
            0);
        if (screenDC)
            ReleaseDC(nullptr, screenDC);
    }

    _buffer.reset();
    _hdrBitmap = nullptr;
}

} // namespace uxtheme
