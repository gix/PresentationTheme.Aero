#pragma once
#include <algorithm>
#include <memory>
#include <windows.h>

namespace uxtheme
{

template<typename T, typename U>
constexpr T narrow_cast(U&& u) noexcept
{
    return static_cast<T>(std::forward<U>(u));
}

namespace details
{
template<typename T>
struct unique_if
{
    using unique_single = std::unique_ptr<T>;
};

template<typename T>
struct unique_if<T[]>
{
    using unique_array_unknown_bound = std::unique_ptr<T[]>;
};

template<typename T, size_t N>
struct unique_if<T[N]>
{
    using unique_array_known_bound = void;
};
} // namespace details


template<typename T, typename... Args>
typename details::unique_if<T>::unique_single
  make_unique_nothrow(Args&&... args)
{
    return std::unique_ptr<T>(new(std::nothrow) T(std::forward<Args>(args)...));
}

template<typename T>
typename details::unique_if<T>::unique_array_unknown_bound
  make_unique_nothrow(size_t size)
{
    using Elem = std::remove_extent_t<T>;
    return std::unique_ptr<T>(new(std::nothrow) Elem[size]());
}

template<typename T, typename... Args>
typename details::unique_if<T>::unique_array_known_bound
  make_unique_nothrow(Args&&...) = delete;


template<typename T>
void SafeRelease(T*& ptr)
{
    if (ptr) {
        ptr->Release();
        ptr = nullptr;
    }
}


template<typename T, size_t N> constexpr size_t countof(T(&/*array*/)[N]) { return N; }

template<size_t N> constexpr size_t lengthof(char(&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(char const (&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(wchar_t(&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(wchar_t const (&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(char16_t(&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(char16_t const (&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(char32_t(&/*array*/)[N]) { return N - 1; }
template<size_t N> constexpr size_t lengthof(char32_t const (&/*array*/)[N]) { return N - 1; }


template<typename T, size_t N>
static void fill(T(&arr)[N], T const& val)
{
    std::fill_n(arr, N, val);
}

template<typename T, size_t N>
static void fill_zero(T(&arr)[N])
{
    std::fill_n(arr, N, T());
}

template<typename T>
static std::enable_if_t<std::is_pointer_v<T>> fill_zero(T ptr)
{
    using V = std::remove_pointer_t<T>;
    static_assert(std::is_trivially_copyable<V>::value, "T must be trivially copyable.");
    std::fill_n(reinterpret_cast<char*>(ptr), sizeof(V), 0);
}


template<typename T>
static T* Advance(T* ptr, unsigned n)
{
    return reinterpret_cast<T*>(reinterpret_cast<char*>(ptr) + n);
}

template<typename T>
static T const* Advance(T const* ptr, unsigned n)
{
    return reinterpret_cast<T const*>(reinterpret_cast<char const*>(ptr) + n);
}

template<typename T>
constexpr bool IsPowerOf2(T value)
{
    return (value & (value - 1)) == 0;
}

#define UXTHEME_ALWAYS_INLINE _forceinline

template<int Alignment, typename T>
UXTHEME_ALWAYS_INLINE T AlignPower2(T value)
{
    static_assert(Alignment != 0 && IsPowerOf2(Alignment));
    return (value + Alignment - 1) & ~(Alignment - 1);
}

template<typename T>
UXTHEME_ALWAYS_INLINE T Align8(T value) { return AlignPower2<8>(value); }

template<typename T>
static T AlignTo(T value, T alignment)
{
    auto remainder = value % alignment;
    if (remainder != 0)
        value += alignment - remainder;
    return value;
}

bool IsHighContrastMode();

#define ENSURE_HR(expr) \
    do { \
        HRESULT hr_ = (expr); \
        if (FAILED(hr_)) { \
            TRACE_HR(hr_); \
            return hr_; \
        } \
    } while (false)

#define ENSURE_HR_EX(exclude, expr) \
    do { \
        HRESULT hr_ = (expr); \
        if (hr_ != (exclude) && FAILED(hr_)) { \
            TRACE_HR(hr_); \
            return hr_; \
        } \
    } while (false)

#define TRACE_HR(hr) \
    (true ? ::uxtheme::TraceHResult((hr), __FILE__, __LINE__, __FUNCTION__) : (hr))

long TraceHResult(long hresult, char const* file = nullptr,
    int lineNumber = 0, char const* function = nullptr);

inline HRESULT MakeErrorLast()
{
    DWORD ec = GetLastError();
    if (ec)
        return HRESULT_FROM_WIN32(ec);
    return E_UNEXPECTED;
}

int AsciiStrCmpI(wchar_t const* dst, wchar_t const* src);

HRESULT GetPtrToResource(HMODULE hInst, wchar_t const* pszResType,
                         wchar_t const* pszResName, void** ppBytes,
                         unsigned* pdwBytes);


using THREADENUMPROC = BOOL(CALLBACK*)(DWORD ownerProcessId, DWORD threadId, LPARAM param);

BOOL EnumProcessThreads(
    _In_ DWORD processId,
    _In_ THREADENUMPROC callback,
    _In_opt_ LPARAM param);

BOOL EnumProcessWindows(
    _In_ DWORD processId,
    _In_ WNDENUMPROC callback,
    _In_opt_ LPARAM param);

void SafeSendMessage(HWND hwnd, DWORD uMsg, WPARAM wParam, LPARAM lParam);
void SendThemeChanged(HWND hwnd);
void SendThemeChangedProcessLocal();

} // namespace uxtheme
