#include "SymbolContext.h"

#include "Utils.h"

#include <string>
#include <vector>

namespace uxtheme
{

namespace
{

struct SymbolInfoPackage : SYMBOL_INFO_PACKAGE
{
    SymbolInfoPackage()
    {
        si.SizeOfStruct = sizeof(SYMBOL_INFO);
        si.MaxNameLen = sizeof(name);
    }
};

} // namespace

HRESULT SymbolContext::Initialize()
{
    if (!SymInitialize(GetCurrentProcess(), nullptr, FALSE) != FALSE)
        return GetLastErrorAsHResult();
    initialized = true;

    DWORD options = SymGetOptions();
    options |= SYMOPT_DEBUG;
    SymSetOptions(options);

    return S_OK;
}

HRESULT SymbolContext::Cleanup()
{
    if (initialized && !SymCleanup(GetCurrentProcess()))
        return GetLastErrorAsHResult();
    initialized = false;
    return S_OK;
}

SymbolContext::~SymbolContext()
{
    (void)Cleanup();
}

HRESULT SymbolContext::LoadModule(HMODULE module)
{
    std::wstring path;
    ENSURE_HR(GetModuleFileNameW(module, path));
    DWORD64 const baseAddr =
        SymLoadModuleExW(GetCurrentProcess(), nullptr, path.data(), nullptr,
                         reinterpret_cast<uintptr_t>(module), 0, nullptr, 0);
    if (!baseAddr)
        return GetLastErrorAsHResult();

    return S_OK;
}

HRESULT SymbolContext::UnloadModule(HMODULE module)
{
    if (!SymUnloadModule64(GetCurrentProcess(),
                           reinterpret_cast<uintptr_t>(module)))
        return GetLastErrorAsHResult();
    return S_OK;
}

HRESULT SymbolContext::GetSymbolAddress(HMODULE module, char const* symbolName,
                                        uintptr_t& address)
{
    if (!initialized)
        return E_UNEXPECTED;

    SymbolInfoPackage info;
    if (!SymFromName(GetCurrentProcess(), symbolName, &info.si))
        return GetLastErrorAsHResult();

    if (info.si.ModBase != reinterpret_cast<uintptr_t>(module))
        return E_FAIL;

    address = info.si.Address;
    return S_OK;
}

} // namespace uxtheme
