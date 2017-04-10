#pragma once
#include <cassert>
#include <utility>
#include <windows.h>

namespace uxtheme
{

template<typename Traits>
class Handle
{
    using HandleType = typename Traits::HandleType;

public:
    constexpr Handle() noexcept
        : handle(Traits::InvalidHandle())
    {
    }

    constexpr explicit Handle(HandleType handle) noexcept
        : handle(handle)
    {
    }

    ~Handle() noexcept
    {
        Close();
    }

    constexpr Handle(Handle&& source) noexcept
        : handle(source.Detach())
    {
    }

    Handle& operator =(Handle&& source) noexcept
    {
        assert(this != &source);
        Reset(source.Detach());
        return *this;
    }

    Handle(Handle const&) = delete;
    Handle& operator =(Handle const&) = delete;

    constexpr static HandleType InvalidHandle() noexcept
    {
        return Traits::InvalidHandle();
    }

    constexpr bool IsValid() const noexcept
    {
        return Traits::IsValid(handle);
    }

    void Close() noexcept
    {
        if (Traits::IsValid(handle)) {
            Traits::Close(handle);
            handle = Traits::InvalidHandle();
        }
    }

    bool Reset(HandleType handle = Traits::InvalidHandle()) noexcept
    {
        if (this->handle != handle) {
            Close();
            this->handle = handle;
        }

        return IsValid();
    }

    HandleType Detach() noexcept
    {
        return std::exchange(handle, Traits::InvalidHandle());
    }

    Handle& operator =(HandleType handle)
    {
        Reset(handle);
        return *this;
    }

    constexpr explicit operator bool() const noexcept
    {
        return Traits::IsValid(handle);
    }

    constexpr HandleType Get() const noexcept { return handle; }

    constexpr operator HandleType() const noexcept { return handle; }

    HandleType* CloseAndGetAddressOf() noexcept
    {
        Close();
        handle = Traits::InvalidHandle();
        return &handle;
    }

private:
    HandleType handle;
};

/// Prevent accidentally calling CloseHandle(HANDLE) with a scoped handle.
/// Scoped handles must be closed by calling Close().
using ::CloseHandle;

template<typename Traits>
void CloseHandle(Handle<Traits> const&) = delete;

struct MinusOneIsInvalidHandleTraits
{
    using HandleType = HANDLE;
    constexpr static HandleType InvalidHandle() noexcept { return INVALID_HANDLE_VALUE; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::CloseHandle(h); }
};

struct NullIsInvalidHandleTraits
{
    using HandleType = HANDLE;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::CloseHandle(h); }
};


struct FileHandleTraits : MinusOneIsInvalidHandleTraits {};
using FileHandle = Handle<FileHandleTraits>;

struct ProcessHandleTraits : NullIsInvalidHandleTraits {};
using ProcessHandle = Handle<ProcessHandleTraits>;

struct ThreadHandleTraits : NullIsInvalidHandleTraits {};
using ThreadHandle = Handle<ThreadHandleTraits>;

struct FileMappingHandleTraits : NullIsInvalidHandleTraits {};
using FileMappingHandle = Handle<FileMappingHandleTraits>;

struct FileViewHandleTraits
{
    using HandleType = void*;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::UnmapViewOfFile(h); }
};
using FileViewHandle = Handle<FileViewHandleTraits>;

struct ModuleHandleTraits
{
    using HandleType = HMODULE;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::FreeLibrary(h); }
};
using ModuleHandle = Handle<ModuleHandleTraits>;

} // namespace ffmf
