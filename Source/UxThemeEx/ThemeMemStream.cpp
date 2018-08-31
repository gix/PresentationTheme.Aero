#include "ThemeMemStream.h"
#include "Utils.h"

namespace uxtheme
{

ThemeMemStream::ThemeMemStream(uint64_t initialSize, bool read)
    : read_(read)
    , initialSize_(initialSize)
    , refCount_(1)
{
    Clear(0);
}

HRESULT ThemeMemStream::QueryInterface(REFIID riid, _COM_Outptr_ void** ppvObject)
{
    if (!ppvObject)
        return E_POINTER;

    if (riid == IID_IUnknown)
        *ppvObject = static_cast<IUnknown*>(this);
    else if (riid == IID_ISequentialStream)
        *ppvObject = static_cast<ISequentialStream*>(this);
    else if (riid == IID_IStream)
        *ppvObject = static_cast<IStream*>(this);
    else {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    if (*ppvObject)
        AddRef();
    return S_OK;
}

ULONG ThemeMemStream::AddRef()
{
    return refCount_ += 1;
}

ULONG ThemeMemStream::Release()
{
    return refCount_ -= 1;
}

HRESULT ThemeMemStream::Read(
    _Out_writes_bytes_to_(cb, *pcbRead)  void* pv,
    _In_ ULONG cb,
    _Out_opt_ ULONG* pcbRead)
{
    if (position_ >= size_) {
        *pcbRead = 0;
        return E_FAIL;
    }

    if (position_ + cb > size_)
        cb = size_ - position_;

    if (cb) {
        memcpy(pv, &buffer_[position_], cb);
        this->position_ += cb;
    }

    *pcbRead = cb;
    return cb == 0 ? E_FAIL : S_OK;
}

HRESULT ThemeMemStream::Write(
    _In_reads_bytes_(cb) void const* pv,
    _In_  ULONG cb,
    _Out_opt_ ULONG* pcbWritten)
{
    if (!pv)
        return E_INVALIDARG;

    size_t cbWritten = cb;
    if (cb) {
        uint64_t newSize = initialSize_;
        if (position_ + cb > newSize)
            newSize = position_ + cb;

        if (newSize > maxSize_) {
            size_t v11 = ((newSize / 8192) + 1) * 8192;
            auto newBuf = make_unique_nothrow<uint8_t[]>(v11);
            if (!newBuf)
                return E_OUTOFMEMORY;
            if (buffer_) {
                memcpy(newBuf.get(), buffer_.get(), maxSize_);
                buffer_.reset();
            }

            maxSize_ = v11;
            buffer_ = std::move(newBuf);
        }

        if (!buffer_ && !position_ || buffer_.get() + position_ < buffer_.get() || (uint64_t)(buffer_.get() + position_) < position_)
            return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);

        memcpy(&buffer_[position_], pv, cbWritten);

        position_ += cbWritten;
        if (position_ > size_)
            size_ = position_;
    }

    if (pcbWritten)
        *pcbWritten = cbWritten;

    return S_OK;
}

HRESULT ThemeMemStream::Seek(
    LARGE_INTEGER liMove,
    DWORD dwOrigin,
    _Out_opt_  ULARGE_INTEGER* pliNewPos)
{
    if (dwOrigin == 0)
        position_ = liMove.LowPart;
    else if (dwOrigin == 1)
        position_ = position_ + liMove.LowPart;
    else if (dwOrigin == 2)
        position_ = size_ - liMove.LowPart;
    else
        position_ = 0;

    if (pliNewPos)
        pliNewPos->QuadPart = position_;
    return S_OK;
}

HRESULT ThemeMemStream::SetSize(ULARGE_INTEGER uli)
{
    size_ = initialSize_;
    if (uli.QuadPart > size_)
        size_ = uli.QuadPart;

    if (position_ > size_)
        position_ = size_;

    if (size_ > maxSize_) {
        auto newBuf = make_unique_nothrow<uint8_t[]>(size_ + 0x2000);
        if (buffer_) {
            memcpy(newBuf.get(), buffer_.get(), maxSize_);
            buffer_.reset();
        }

        buffer_ = std::move(newBuf);
        maxSize_ = size_ + 0x2000;
    }

    return S_OK;
}

HRESULT ThemeMemStream::CopyTo(
    _In_  IStream* pstm,
    ULARGE_INTEGER cb,
    _Out_opt_  ULARGE_INTEGER* pcbRead,
    _Out_opt_  ULARGE_INTEGER* pcbWritten)
{
    return STG_E_UNIMPLEMENTEDFUNCTION;
}

HRESULT ThemeMemStream::Commit(DWORD grfCommitFlags)
{
    return S_OK;
}

HRESULT ThemeMemStream::Revert()
{
    return STG_E_UNIMPLEMENTEDFUNCTION;
}

HRESULT ThemeMemStream::LockRegion(
    ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType)
{
    return STG_E_UNIMPLEMENTEDFUNCTION;
}

HRESULT ThemeMemStream::UnlockRegion(
    ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType)
{
    return STG_E_UNIMPLEMENTEDFUNCTION;
}

HRESULT ThemeMemStream::Stat(_Out_ STATSTG* pStg, DWORD dwStatFlag)
{
    SYSTEMTIME systemTime;
    GetSystemTime(&systemTime);

    *pStg = STATSTG();
    pStg->type = 2;
    pStg->pwcsName = nullptr; // (wchar_t *)&pszAppName;
    pStg->cbSize.QuadPart = size_;
    SystemTimeToFileTime(&systemTime, &pStg->mtime);
    pStg->ctime = pStg->mtime;
    pStg->atime = pStg->mtime;
    pStg->grfMode = read_ == 0;
    return S_OK;
}

HRESULT ThemeMemStream::Clone(_COM_Outptr_opt_ IStream** ppstm)
{
    return STG_E_UNIMPLEMENTEDFUNCTION;
}

uint8_t* ThemeMemStream::GetBuffer(uint64_t* cbSize)
{
    if (cbSize)
        *cbSize = size_;
    return buffer_.get();
}

void ThemeMemStream::Clear(BOOL fFree)
{
    if (fFree && !read_)
        buffer_.reset();

    buffer_ = nullptr;
    position_ = 0;
    size_ = 0;
    maxSize_ = 0;
}

HRESULT ThemeMemStream::SetMaxSize(uint64_t cbSize)
{
    if (cbSize < size_) {
        position_ = 0;
        return S_OK;
    }

    Clear(TRUE);
    ULARGE_INTEGER size;
    size.QuadPart = cbSize;
    return SetSize(size);
}

} // namespace uxtheme
