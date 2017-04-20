#pragma once
#include <cstdint>
#include <memory>
#include <objidl.h>

namespace uxtheme
{

class ThemeMemStream : public IStream
{
public:
    ThemeMemStream(uint64_t initialSize, bool read);
    virtual ~ThemeMemStream() = default;

    STDMETHODIMP QueryInterface(REFIID riid, _COM_Outptr_ void** ppvObject) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;

    STDMETHODIMP Read(
        _Out_writes_bytes_to_(cb, *pcbRead)  void* pv,
        _In_ ULONG cb,
        _Out_opt_ ULONG* pcbRead) override;

    STDMETHODIMP Write(
        _In_reads_bytes_(cb)  void const* pv,
        _In_  ULONG cb,
        _Out_opt_ ULONG* pcbWritten) override;

    STDMETHODIMP Seek(
        LARGE_INTEGER liMove,
        DWORD dwOrigin,
        _Out_opt_  ULARGE_INTEGER* pliNewPos) override;

    STDMETHODIMP SetSize(ULARGE_INTEGER uli) override;

    STDMETHODIMP CopyTo(
        _In_  IStream* pstm,
        ULARGE_INTEGER cb,
        _Out_opt_  ULARGE_INTEGER* pcbRead,
        _Out_opt_  ULARGE_INTEGER* pcbWritten) override;

    STDMETHODIMP Commit(DWORD grfCommitFlags) override;

    STDMETHODIMP Revert() override;

    STDMETHODIMP LockRegion(
        ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType) override;

    STDMETHODIMP UnlockRegion(
        ULARGE_INTEGER libOffset, ULARGE_INTEGER cb, DWORD dwLockType) override;

    STDMETHODIMP Stat(_Out_ STATSTG* pStg, DWORD dwStatFlag) override;

    STDMETHODIMP Clone(_COM_Outptr_opt_ IStream** ppstm) override;

    uint8_t* GetBuffer(uint64_t* cbSize);
    void Clear(BOOL fFree);
    HRESULT SetMaxSize(uint64_t cbSize);

private:
    bool read_ = false;
    std::unique_ptr<uint8_t[]> buffer_;
    uint64_t position_ = 0;
    uint64_t size_ = 0;
    uint64_t maxSize_ = 0;
    uint64_t initialSize_ = 0;
    unsigned refCount_;
};

} // namespace uxtheme
