#pragma once
#include <windows.h>
#include <uxtheme.h>
#include <mutex>
#include <vector>

namespace uxtheme
{
struct CTextDraw;
struct CDrawBase;
struct CUxThemeFile;

struct CRenderObj;

struct RENDER_OBJ_ENTRY
{
    CRenderObj* pRenderObj;
    unsigned dwRecycleNum;
    int iRefCount;
    int iInUseCount;
    int iLoadId;
    int fClosing;
    HWND hwnd;
};

struct CRenderList
{
    void FreeRenderObjects(int iThemeFileLoadId);
    BOOL DeleteCheck(RENDER_OBJ_ENTRY* pEntry);

    HRESULT OpenThemeHandle(HTHEME hTheme, CRenderObj** ppRenderObj, int* piSlotNum);
    void CloseThemeHandle(int iSlotNum);

    HRESULT OpenRenderObject(CUxThemeFile* pThemeFile, int iThemeOffset,
                             int iAppNameOffset, int iClassNameOffset,
                             CDrawBase* pDrawBase, CTextDraw* pTextObj,
                             HWND hwnd, int iTargetDpi, unsigned dwOtdFlags,
                             bool fForNonClientUse, void** phTheme);
    HRESULT CloseRenderObject(HTHEME hTheme);

    std::mutex _csListLock;
    std::vector<RENDER_OBJ_ENTRY> _RenderEntries;
    int _iNextUniqueId = 0;
};

} // namespace uxtheme
