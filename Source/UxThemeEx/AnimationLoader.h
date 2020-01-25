#pragma once
#include <uxtheme.h>
#include <vector>
#include <windows.h>

namespace uxtheme
{

struct AnimationProperty
{
    TA_PROPERTY_FLAG eFlags;
    DWORD dwTransformCount;
    DWORD dwStaggerDelay;
    DWORD dwStaggerDelayCap;
    float rStaggerDelayFactor;
    DWORD dwZIndex;
    DWORD dwBackgroundPartId;
    DWORD dwTuningLevel;
    float rPerspective;
};

class alignas(8) CAnimationDataSerializer
{
public:
    virtual ~CAnimationDataSerializer() = default;

private:
    BYTE* _pStream;
    DWORD _uStreamLength;
};

class CTransformSerializer : CAnimationDataSerializer
{
public:
    struct Header
    {
        DWORD _cbSize;
        DWORD _dwOffsetProperty;
        DWORD _dwOffsetTransform;

        AnimationProperty const* Property(DWORD availableSize) const
        {
            if (availableSize >= _cbSize)
                return (AnimationProperty*)((BYTE*)this + _dwOffsetProperty);
            return nullptr;
        }

        TA_TRANSFORM const* Transform() const
        {
            return (TA_TRANSFORM*)((BYTE*)this + _dwOffsetTransform);
        }
    };

    static DWORD GetTransformSize(TA_TRANSFORM const* transform);
    static DWORD GetTransformActualSize(TA_TRANSFORM const* transform);
    static TA_TRANSFORM const* GetTransformByIndex(BYTE* pb, DWORD dwIndex, DWORD cbSize);

    AnimationProperty _property;
    std::vector<TA_TRANSFORM*> _rgTransforms;
    TA_TRANSFORM* _pCurrentTransform;
};

} // namespace uxtheme
