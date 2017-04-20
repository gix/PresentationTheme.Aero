#include "RenderList.h"
#include "RenderObj.h"
#include "Utils.h"
#include "DpiInfo.h"

namespace uxtheme
{

static HTHEME MakeThemeHandle(int slot, int recycleNum)
{
    auto value = static_cast<uintptr_t>(
        (static_cast<unsigned short>(recycleNum) << 16) |
        (static_cast<unsigned short>(slot) & 0xFFFF));

    // Ensures that we never overlap the range of native handles.
    value = ~value;

    return reinterpret_cast<HTHEME>(value);
}

static void SplitThemeHandle(HTHEME hTheme, unsigned* slot, unsigned* recycleNum)
{
    auto value = reinterpret_cast<uintptr_t>(hTheme);
    value = ~value;

    *slot = value & 0xFFFF;
    *recycleNum = (value >> 16) & 0xFFFF;
}

static bool IsThemeHandle(HTHEME hTheme)
{
    // FIXME: 32-bit
    auto value = reinterpret_cast<uintptr_t>(hTheme);
    return (value >> 32) != 0;
}

void CRenderList::FreeRenderObjects(int iThemeFileLoadId)
{
    std::lock_guard<std::mutex> lock(_csListLock);

    for (RENDER_OBJ_ENTRY& entry : _RenderEntries) {
        if (entry.pRenderObj && (iThemeFileLoadId == -1 || entry.iLoadId == iThemeFileLoadId)) {
            entry.iRefCount = 0;
            entry.fClosing = 1;
            DeleteCheck(&entry);
        }
    }
}

BOOL CRenderList::DeleteCheck(RENDER_OBJ_ENTRY* pEntry)
{
    if (pEntry->iInUseCount != 0 || pEntry->iRefCount != 0)
        return FALSE;

    delete pEntry->pRenderObj;
    pEntry->pRenderObj = nullptr;
    pEntry->fClosing = 0;
    return TRUE;
}

HRESULT CRenderList::OpenThemeHandle(HTHEME hTheme, CRenderObj** ppRenderObj, int* piSlotNum)
{
    std::lock_guard<std::mutex> lock(_csListLock);

    unsigned slot;
    unsigned recycleNum;
    SplitThemeHandle(hTheme, &slot, &recycleNum);

    if (slot >= _RenderEntries.size())
        return E_HANDLE;

    RENDER_OBJ_ENTRY& entry = _RenderEntries[slot];
    if (!entry.pRenderObj || entry.fClosing || entry.dwRecycleNum != recycleNum)
        return E_HANDLE;

    ++entry.iInUseCount;
    *ppRenderObj = entry.pRenderObj;
    *piSlotNum = slot;

    return S_OK;
}

void CRenderList::CloseThemeHandle(int iSlotNum)
{
    std::lock_guard<std::mutex> lock(_csListLock);

    RENDER_OBJ_ENTRY& entry = _RenderEntries[iSlotNum];
    if (entry.iInUseCount > 0) {
        --entry.iInUseCount;
        if (entry.iRefCount == 0 && entry.iInUseCount == 0) {
            delete entry.pRenderObj;
            entry.pRenderObj = nullptr;
            entry.fClosing = 0;
        }
    }
}

HRESULT CRenderList::OpenRenderObject(
    CUxThemeFile* pThemeFile, int iThemeOffset, int iAppNameOffset,
    int iClassNameOffset, CDrawBase* pDrawBase, CTextDraw* pTextObj,
    HWND hwnd, int iTargetDpi, unsigned dwOtdFlags, bool fForNonClientUse,
    HTHEME* phTheme)
{
    bool isStronglyAssociatedDpi;
    if (iTargetDpi) {
        isStronglyAssociatedDpi = true;
    //} else if (hwnd && ThemeHasPerWindowDPI(hwnd, fForNonClientUse)) {
    //    iTargetDpi = GetWindowDPI(hwnd);
    //    isStronglyAssociatedDpi = true;
    } else {
        iTargetDpi = GetScreenDpi();
        isStronglyAssociatedDpi = false;
    }

    std::lock_guard<std::mutex> lock(_csListLock);

    int nextAvailSlot = -1;

    for (int i = 0; i < _RenderEntries.size(); ++i) {
        auto& entry = _RenderEntries[i];
        if (!entry.pRenderObj) {
            if (nextAvailSlot == -1)
                nextAvailSlot = i;
            continue;
        }

        auto obj = entry.pRenderObj;
        if (!entry.fClosing &&
            obj->_pThemeFile == pThemeFile &&
            obj->GetThemeOffset() == iThemeOffset &&
            obj->GetAssociatedDpi() == iTargetDpi &&
            !obj->IsStronglyAssociatedDpi()) {
            ++entry.iRefCount;
            *phTheme = MakeThemeHandle(i, entry.dwRecycleNum);
            return S_OK;
        }
    }

    CRenderObj* renderObj = nullptr;
    ENSURE_HR(CRenderObj::Create(
        pThemeFile, 0, iThemeOffset, iAppNameOffset, iClassNameOffset,
        ++_iNextUniqueId, false, pDrawBase, pTextObj, iTargetDpi,
        isStronglyAssociatedDpi, dwOtdFlags, &renderObj));

    int iLoadId = 0;
    if (auto nonSharableHdr = (NONSHARABLEDATAHDR*)renderObj->_pbNonSharableData)
        iLoadId = nonSharableHdr->iLoadId;

    RENDER_OBJ_ENTRY entry = {};
    entry.hwnd = hwnd;
    entry.pRenderObj = renderObj;
    entry.dwRecycleNum = 1;
    entry.iRefCount = 1;
    entry.iInUseCount = 0;
    entry.iLoadId = iLoadId;

    int usedSlot;
    if (nextAvailSlot == -1) {
        _RenderEntries.push_back(entry);
        usedSlot = narrow_cast<int>(_RenderEntries.size() - 1);
    } else {
        _RenderEntries[nextAvailSlot] = entry;
        usedSlot = nextAvailSlot;
    }

    *phTheme = MakeThemeHandle(usedSlot, entry.dwRecycleNum);
    return S_OK;
}

HRESULT CRenderList::CloseRenderObject(HTHEME hTheme)
{
    unsigned slot;
    unsigned recycleNum;
    SplitThemeHandle(hTheme, &slot, &recycleNum);

    std::lock_guard<std::mutex> lock(_csListLock);

    if (slot >= _RenderEntries.size())
        return E_HANDLE;

    auto& entry = _RenderEntries[slot];
    if (!entry.pRenderObj || entry.fClosing || entry.dwRecycleNum != recycleNum)
        return E_HANDLE;

    if (entry.iRefCount > 0)
        --entry.iRefCount;

    if (entry.iRefCount == 0 && entry.iInUseCount == 0) {
        delete entry.pRenderObj;
        entry.pRenderObj = nullptr;
        entry.fClosing = 0;
    }

    return S_OK;
}

} // namespace uxtheme
