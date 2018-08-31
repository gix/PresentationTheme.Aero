#include "ThemePNGDecoder.h"
#include "Utils.h"

namespace uxtheme
{


HRESULT CThemePNGDecoder::_Init()
{
    if (_bInited)
        return E_FAIL;

    ENSURE_HR(CoCreateInstance(
        CLSID_WICImagingFactory,
        NULL,
        CLSCTX_INPROC_SERVER,
        IID_IWICImagingFactory,
        (LPVOID*)&_pICodecFactory));
    //ENSURE_HR(WICCreateImagingFactory_Proxy(567, &_pICodecFactory));
    ENSURE_HR(_pICodecFactory->CreateComponentInfo(CLSID_WICPngDecoder2, &_pInfo1));
    ENSURE_HR(_pInfo1->QueryInterface(&_pBitmapDecoderInfo));
    ENSURE_HR(_pICodecFactory->CreateComponentInfo(CLSID_WICBmpEncoder, &_pInfo2));
    ENSURE_HR(_pInfo2->QueryInterface(&_pBitmapEncoderInfo));
    ENSURE_HR(_stream.SetMaxSize(0x100000));

    _stream.AddRef();
    _bInited = true;

    return S_OK;
}

HRESULT CThemePNGDecoder::ConvertToDIB(BYTE const* lpBits, unsigned cbDIB,
                                       bool* pf32bpp)
{
    if (!_bInited)
        ENSURE_HR(_Init());

    IWICStream* stream = nullptr;
    IWICBitmapDecoder* decoder = nullptr;
    IWICBitmapFrameDecode* decoderFrame = nullptr;
    IWICBitmapEncoder* encoder = nullptr;
    IWICBitmapFrameEncode* encoderFrame = nullptr;

    unsigned width = 0;
    unsigned height = 0;
    double dpiX = 0;
    double dpiY = 0;
    unsigned frameCount = 0;

    HRESULT hr = _pICodecFactory->CreateStream(&stream);
    if (hr >= 0)
        hr = stream->InitializeFromMemory((WICInProcPointer)lpBits, cbDIB);

    if (hr >= 0)
        hr = _pBitmapDecoderInfo->CreateInstance(&decoder);
    if (hr >= 0)
        hr = decoder->Initialize(stream, WICDecodeMetadataCacheOnDemand);
    if (hr >= 0)
        hr = decoder->GetFrameCount(&frameCount);
    if (hr >= 0)
        if (frameCount < 1)
            hr = E_FAIL;

    if (hr >= 0)
        hr = decoder->GetFrame(0, &decoderFrame);
    if (hr >= 0)
        hr = decoderFrame->GetSize(&width, &height);
    if (hr >= 0)
        hr = decoderFrame->GetResolution(&dpiX, &dpiY);

    GUID pixelFormat = GUID_WICPixelFormat1bppIndexed;
    if (hr >= 0)
        hr = decoderFrame->GetPixelFormat(&pixelFormat);

    if (hr >= 0) {
        if (pixelFormat == GUID_WICPixelFormat24bppBGR)
            *pf32bpp = false;
        else if (pixelFormat == GUID_WICPixelFormat32bppBGRA)
            *pf32bpp = true;
        else
            hr = E_FAIL;
    }

    if (hr >= 0)
        hr = _pBitmapEncoderInfo->CreateInstance(&encoder);

    if (hr >= 0) {
        int v24 = 4 * width * width;
        if (cbDIB > v24)
            v24 = cbDIB;
        hr = _stream.SetMaxSize(v24);
    }
    if (hr >= 0)
        hr = encoder->Initialize(&_stream, WICBitmapEncoderNoCache);
    if (hr >= 0)
        hr = encoder->CreateNewFrame(&encoderFrame, nullptr);
    if (hr >= 0)
        hr = encoderFrame->Initialize(nullptr);
    if (hr >= 0)
        hr = encoderFrame->SetSize(width, height);
    if (hr >= 0) {
        GUID format = GUID_WICPixelFormat32bppBGRA;
        hr = encoderFrame->SetPixelFormat(&format);
    }
    if (hr >= 0)
        hr = encoderFrame->SetResolution(dpiX, dpiY);

    if (hr >= 0) {
        WICRect rect = {};
        rect.Width = width;
        rect.Height = height;
        hr = encoderFrame->WriteSource(decoderFrame, &rect);
    }

    if (hr >= 0)
        hr = encoderFrame->Commit();
    if (hr >= 0)
        hr = encoder->Commit();

    SafeRelease(encoderFrame);
    SafeRelease(encoder);
    SafeRelease(decoderFrame);
    SafeRelease(decoder);
    SafeRelease(stream);

    return hr;
}

} // namespace uxtheme
