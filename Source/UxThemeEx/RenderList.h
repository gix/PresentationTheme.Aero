#pragma once
#include <mutex>
#include <uxtheme.h>
#include <vector>
#include <windows.h>

namespace uxtheme
{
class CDrawBase;
class CRenderObj;
class CTextDraw;
class CUxThemeFile;

struct RENDER_OBJ_ENTRY
{
    CRenderObj* pRenderObj;
    unsigned dwRecycleNum;
    int iRefCount;
    int iInUseCount;
    int iLoadId;
    bool fClosing;
    HWND hwnd;
};

class CRenderList
{
public:
    void FreeRenderObjects(int iThemeFileLoadId);
    BOOL DeleteCheck(RENDER_OBJ_ENTRY* pEntry);

    HRESULT OpenThemeHandle(HTHEME hTheme, CRenderObj** ppRenderObj, int* piSlotNum);
    void CloseThemeHandle(int iSlotNum);

    HRESULT OpenRenderObject(CUxThemeFile* pThemeFile, int iThemeOffset,
                             int iAppNameOffset, int iClassNameOffset,
                             CDrawBase* pDrawBase, CTextDraw* pTextObj, HWND hwnd,
                             int iTargetDpi, unsigned dwOtdFlags, bool fForNonClientUse,
                             void** phTheme);
    HRESULT CloseRenderObject(HTHEME hTheme);

private:
    std::mutex _csListLock;
    std::vector<RENDER_OBJ_ENTRY> _RenderEntries;
    int _iNextUniqueId = 0;
};

} // namespace uxtheme
