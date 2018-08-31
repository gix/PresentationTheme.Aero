#include "AnimationLoader.h"
#include "UxThemeEx.h"
#include "Utils.h"

namespace uxtheme
{

namespace
{

struct SymTypeConvertTable
{
    DWORD dwPGType;
    DWORD dwType;
    DWORD dwActualSize;
};

SymTypeConvertTable typeConvertTbl[] = {
    { 4, TATT_TRANSLATE_2D, sizeof(TA_TRANSFORM_2D)},
    { 5, TATT_SCALE_2D, sizeof(TA_TRANSFORM_2D)},
    { 6, TATT_ROTATE_2D, sizeof(TA_TRANSFORM_2D)},
    { 7, TATT_SKEW_2D, sizeof(TA_TRANSFORM_2D)},
    {12, TATT_CLIP, sizeof(TA_TRANSFORM_CLIP)},
    {11, TATT_OPACITY, sizeof(TA_TRANSFORM_OPACITY)},
    { 8, TATT_TRANSLATE_3D, sizeof(TA_TRANSFORM_3D)},
    { 9, TATT_SCALE_3D, sizeof(TA_TRANSFORM_3D)},
    {10, TATT_ROTATE_3D, sizeof(TA_TRANSFORM_3D)},
};

} // namespace

DWORD CTransformSerializer::GetTransformSize(TA_TRANSFORM const* transform)
{
    return Align8(GetTransformActualSize(transform));
}

DWORD CTransformSerializer::GetTransformActualSize(TA_TRANSFORM const* transform)
{
    for (auto const& entry : typeConvertTbl) {
        if (entry.dwType == transform->eTransformType)
            return entry.dwActualSize;
    }

    return 0;
}

TA_TRANSFORM const* CTransformSerializer::GetTransformByIndex(
    BYTE* pb, DWORD dwIndex, DWORD cbSize)
{
    auto const header = reinterpret_cast<Header const*>(pb);
    auto const animationProperty = header->Property(cbSize);
    if (!animationProperty)
        return nullptr;

    if (dwIndex >= animationProperty->dwTransformCount)
        return nullptr;

    TA_TRANSFORM const* transform = header->Transform();
    DWORD offset = (DWORD)((uintptr_t)transform - (uintptr_t)header);
    for (DWORD i = 0; offset < cbSize && offset < header->_cbSize; ++i) {
        if (i == dwIndex)
            return transform;
        transform = Advance(transform, GetTransformActualSize(transform));
        offset = (DWORD)((uintptr_t)transform - (uintptr_t)header);
    }

    return nullptr;
}

} // namespace uxtheme
