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
    HRESULT ConvertToDIB(char const* lpBits, unsigned cbDIB, int* pf32bpp);

    BITMAPHEADER* GetBitmapHeader()
    {
        return reinterpret_cast<BITMAPHEADER*>(
            _stream.GetBuffer(nullptr) + sizeof(BITMAPFILEHEADER));
    }

private:
    CThemeMemStream _stream{0, FALSE};
    bool _bInited = false;
    IWICImagingFactory* _pICodecFactory = nullptr;
    IWICComponentInfo* _pInfo1 = nullptr;
    IWICComponentInfo* _pInfo2 = nullptr;
    IWICBitmapEncoderInfo* _pBitmapEncoderInfo = nullptr;
    IWICBitmapDecoderInfo* _pBitmapDecoderInfo = nullptr;
};

} // namespace uxtheme
