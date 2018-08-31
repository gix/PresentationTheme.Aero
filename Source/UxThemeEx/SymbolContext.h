#pragma once
#include "Handle.h"

#include <ImageHlp.h>
#include <utility>
#include <windows.h>

namespace uxtheme
{

class SymbolContext
{
public:
    SymbolContext() = default;
    ~SymbolContext();

    SymbolContext(SymbolContext&& source) noexcept
        : initialized(std::exchange(source.initialized, false))
    {
    }

    SymbolContext& operator=(SymbolContext&& source) noexcept
    {
        initialized = std::exchange(source.initialized, false);
        return *this;
    }

    HRESULT Initialize();
    HRESULT Cleanup();
    HRESULT LoadModule(HMODULE module);
    HRESULT UnloadModule(HMODULE module);
    HRESULT GetSymbolAddress(HMODULE module, char const* symbolName,
                             uintptr_t& address);

    template <typename T>
    bool GetProc(ModuleHandle const& module, char const* symbolName, T& proc)
    {
        proc = nullptr;

        if (module.Get(symbolName, proc))
            return true;

        uintptr_t address;
        if (FAILED(GetSymbolAddress(module, symbolName, address)))
            return false;

        proc = reinterpret_cast<T>(address);
        return true;
    }

private:
    bool initialized = false;
};

} // namespace uxtheme
