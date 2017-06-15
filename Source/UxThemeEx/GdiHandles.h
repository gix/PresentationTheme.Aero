#pragma once
#include "Handle.h"

namespace uxtheme
{

template<typename T>
struct GdiObjectTraits
{
    using HandleType = T;
    constexpr static HandleType InvalidHandle() noexcept { return nullptr; }
    constexpr static bool IsValid(HandleType h) noexcept { return h != InvalidHandle(); }
    static void Close(HandleType h) noexcept { ::DeleteObject(h); }
};

using ::DeleteObject;
template<typename T>
void DeleteObject(Handle<GdiObjectTraits<T>> const&) = delete;

using GdiFontHandle = Handle<GdiObjectTraits<HFONT>>;
using GdiBrushHandle = Handle<GdiObjectTraits<HBRUSH>>;
using GdiPenHandle = Handle<GdiObjectTraits<HPEN>>;
using GdiRegionHandle = Handle<GdiObjectTraits<HRGN>>;

} // namespace uxtheme
