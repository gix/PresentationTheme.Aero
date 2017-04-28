#pragma once
#include "ThemeMemStream.h"
#include "UxThemeHelpers.h"
#include <wincodec.h>

namespace uxtheme
{

class CThemePNGDecoder
{
public:
    HRESULT _Init();
    HRESULT ConvertToDIB(BYTE const* lpBits, unsigned cbDIB, bool* pf32bpp);

    BITMAPINFOHEADER* GetBitmapHeader()
    {
        return reinterpret_cast<BITMAPINFOHEADER*>(
            _stream.GetBuffer(nullptr) + sizeof(BITMAPFILEHEADER));
    }

private:
    ThemeMemStream _stream{0, false};
    bool _bInited = false;
    IWICImagingFactory* _pICodecFactory = nullptr;
    IWICComponentInfo* _pInfo1 = nullptr;
    IWICComponentInfo* _pInfo2 = nullptr;
    IWICBitmapEncoderInfo* _pBitmapEncoderInfo = nullptr;
    IWICBitmapDecoderInfo* _pBitmapDecoderInfo = nullptr;
};

} // namespace uxtheme
