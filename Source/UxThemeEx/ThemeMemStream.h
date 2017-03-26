#pragma once
#include <cstdint>
#include <memory>
#include <objidl.h>

namespace uxtheme
{

class CThemeMemStream : public IStream
{
public:
    virtual ~CThemeMemStream() = default;

    CThemeMemStream(uint64_t cbInitialSize, int fRead);

    STDMETHODIMP QueryInterface(REFIID riid, _COM_Outptr_ void** ppvObject) override;
    ULONG AddRef() override;
    ULONG Release() override;

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

    char* GetBuffer(uint64_t* cbSize);
    void Clear(BOOL fFree);
    HRESULT SetMaxSize(uint64_t cbSize);

private:
    int m_fRead = 0;
    std::unique_ptr<char[]> m_lpb;
    uint64_t m_uCurr = 0;
    uint64_t m_uSize = 0;
    uint64_t m_cbMax = 0;
    uint64_t m_cbInitialSize = 0;
    unsigned m_RefCnt = 0;
};

} // namespace uxtheme
