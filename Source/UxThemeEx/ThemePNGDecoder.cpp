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

HRESULT CThemePNGDecoder::ConvertToDIB(char const* lpBits, unsigned cbDIB, int* pf32bpp)
{
    HRESULT hr;
    int v24;
    unsigned v1;
    unsigned v2;
    IWICBitmapFrameEncode* encoderFrame;
    IWICBitmapFrameDecode* decoderFrame;
    IWICBitmapEncoder* encoder;
    IWICBitmapDecoder* decoder;
    unsigned v46;
    IWICStream* stream;
    double v49;
    double v48;
    GUID v50;
    GUID v53;

    if (_bInited || (hr = _Init(), hr >= 0))
    {
        stream = 0;
        v49 = 0;
        v48 = 0;
        decoder = 0;
        decoderFrame = 0;
        v50 = GUID_WICPixelFormat1bppIndexed;
        encoder = 0;
        encoderFrame = 0;
        v1 = 0;
        v2 = 0;
        v46 = 0;
        v53 = GUID_WICPixelFormat32bppBGRA;
        hr = _pICodecFactory->CreateStream(&stream);
        if (hr >= 0)
        {
            hr = stream->InitializeFromMemory((WICInProcPointer)lpBits, cbDIB);
            if (hr >= 0)
            {
                hr = _pBitmapDecoderInfo->CreateInstance(&decoder);
                if (hr >= 0)
                {
                    hr = decoder->Initialize(stream, WICDecodeMetadataCacheOnDemand);
                    if (hr >= 0)
                    {
                        hr = decoder->GetFrameCount(&v46);
                        if (hr >= 0)
                        {
                            if ((unsigned)v46 < 1)
                                hr = -2147467259;
                            if (hr >= 0)
                            {
                                hr = decoder->GetFrame(0, &decoderFrame);
                                if (hr >= 0)
                                {
                                    hr = decoderFrame->GetSize(&v1, &v2);
                                    if (hr >= 0)
                                    {
                                        hr = decoderFrame->GetResolution(&v49, &v48);
                                        if (hr >= 0)
                                        {
                                            hr = decoderFrame->GetPixelFormat(&v50);
                                            if (hr >= 0)
                                            {
                                                if (v50 != GUID_WICPixelFormat24bppBGR)
                                                {
                                                    if (v50 != GUID_WICPixelFormat32bppBGRA)
                                                        hr = E_FAIL;
                                                    else
                                                        *pf32bpp = 1;
                                                } else
                                                {
                                                    *pf32bpp = 0;
                                                }
                                                if (hr >= 0)
                                                {
                                                    hr = _pBitmapEncoderInfo->CreateInstance(&encoder);
                                                    if (hr >= 0)
                                                    {
                                                        v24 = 4 * v1 * v1;
                                                        if (cbDIB > v24)
                                                            v24 = cbDIB;
                                                        hr = _stream.SetMaxSize((unsigned)v24);
                                                        if (hr >= 0)
                                                        {
                                                            hr = encoder->Initialize(&_stream, WICBitmapEncoderNoCache);
                                                            if (hr >= 0)
                                                            {
                                                                hr = encoder->CreateNewFrame(&encoderFrame, nullptr);
                                                                if (hr >= 0)
                                                                {
                                                                    hr = encoderFrame->Initialize(nullptr);
                                                                    if (hr >= 0)
                                                                    {
                                                                        hr = encoderFrame->SetSize(v1, v2);
                                                                        if (hr >= 0)
                                                                        {
                                                                            hr = encoderFrame->SetPixelFormat(&v53);
                                                                            if (hr >= 0)
                                                                            {
                                                                                hr = encoderFrame->SetResolution(v49, v48);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        WICRect v51 = {};
        v51.Width = v1;
        v51.Height = v2;
        if (hr >= 0)
        {
            hr = encoderFrame->WriteSource(decoderFrame, &v51);
            if (hr >= 0)
            {
                hr = encoderFrame->Commit();
                if (hr >= 0)
                {
                    hr = encoder->Commit();
                }
            }
        }

        SafeRelease(encoderFrame);
        SafeRelease(encoder);
        SafeRelease(decoderFrame);
        SafeRelease(decoder);
        SafeRelease(stream);
    }
    return hr;
}

} // namespace uxtheme
