#pragma once
#include "Handle.h"

namespace uxtheme
{

struct GdiRegionHandleTraits
{
    using HandleType = HRGN;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::DeleteObject(h); }
};

using GdiRegionHandle = Handle<GdiRegionHandleTraits>;

} // namespace uxtheme
