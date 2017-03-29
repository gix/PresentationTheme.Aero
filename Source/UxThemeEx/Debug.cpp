#include "Debug.h"
#include <unordered_set>

namespace uxtheme
{

static std::unordered_set<ENTRYHDR const*> entryHeaders;
static std::unordered_set<PARTJUMPTABLEHDR const*> partJumpTableHeaders;
static std::unordered_set<STATEJUMPTABLEHDR const*> stateJumpTableHeaders;
static std::unordered_set<PARTOBJHDR const*> partObjHeaders;
static std::unordered_set<CDrawBase const*> drawObjs;
static std::unordered_set<CTextDraw const*> textObjs;

static void FailFast()
{
    RaiseFailFastException(nullptr, nullptr, 0);
}

template<typename Container, typename T>
static bool contains(Container& c, T const& value)
{
    //return true;
    return c.find(value) != std::end(c);
}

void ClearDebugInfo()
{
    entryHeaders.clear();
    partJumpTableHeaders.clear();
    stateJumpTableHeaders.clear();
    partObjHeaders.clear();
    drawObjs.clear();
    textObjs.clear();
}

void RegisterPtr(ENTRYHDR const* hdr)
{
    entryHeaders.insert(hdr);
}

void RegisterPtr(PARTJUMPTABLEHDR const* hdr)
{
    partJumpTableHeaders.insert(hdr);
}

void RegisterPtr(STATEJUMPTABLEHDR const* hdr)
{
    stateJumpTableHeaders.insert(hdr);
}

void RegisterPtr(PARTOBJHDR const* hdr)
{
    partObjHeaders.insert(hdr);
}

void RegisterPtr(CDrawBase const* obj)
{
    drawObjs.insert(obj);
}

void RegisterPtr(CTextDraw const* obj)
{
    textObjs.insert(obj);
}

void ValidatePtr(ENTRYHDR const* hdr)
{
    if (!contains(entryHeaders, hdr))
        FailFast();
}

void ValidatePtr(PARTJUMPTABLEHDR const* hdr)
{
    if (!contains(partJumpTableHeaders, hdr))
        FailFast();
}

void ValidatePtr(STATEJUMPTABLEHDR const* hdr)
{
    if (!contains(stateJumpTableHeaders, hdr))
        FailFast();
}

void ValidatePtr(PARTOBJHDR const* hdr)
{
    if (!contains(partObjHeaders, hdr))
        FailFast();
}

void ValidatePtr(CDrawBase const* obj)
{
    if (!contains(drawObjs, obj))
        FailFast();
}

void ValidatePtr(CTextDraw const* obj)
{
    if (!contains(textObjs, obj))
        FailFast();
}

} // namespace uxtheme
