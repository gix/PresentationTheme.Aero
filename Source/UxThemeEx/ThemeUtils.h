#pragma once
#include "Utils.h"
#include <windows.h>

namespace uxtheme
{

class BitmapPixels
{
public:
    HRESULT OpenBitmap(HDC hdc, HBITMAP bitmap, bool fForceRGB32,
                       DWORD** pPixels, int* piWidth, int* piHeight,
                       int* piBytesPerPixel, int* piBytesPerRow,
                       int* piPreviousBytesPerPixel, unsigned cbBytesBefore);
    void CloseBitmap(HDC hdc, HBITMAP hBitmap);

    BITMAPINFOHEADER const* BitmapHeader() const { return _hdrBitmap; }

private:
    malloc_ptr<BYTE[]> _buffer;
    BITMAPINFOHEADER* _hdrBitmap = nullptr;
    int _iWidth = 0;
    int _iHeight = 0;
};

} // namespace uxtheme
