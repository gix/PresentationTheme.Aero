#pragma once
#include <windows.h>

typedef struct tagGDIDRAWSTREAM
{
    DWORD signature;      // = 0x44727753;//"Swrd"
    DWORD reserved;       // Zero value.
    DWORD hDC;            // handle to the device object of window to draw.
    RECT rcDest;          // desination rect of window to draw.
    DWORD one;            // must be 1.
    DWORD hImage;         // handle to the specia bitmap image.
    DWORD nine;           // must be 9.
    RECT rcClip;          // desination rect of window to draw.
    RECT rcSrc;           // source rect of bitmap to draw.
    DWORD drawOption;     // option flag for drawing image.
    DWORD leftArcValue;   // arc value of left side.
    DWORD rightArcValue;  // arc value of right side.
    DWORD topArcValue;    // arc value of top side.
    DWORD bottomArcValue; // arc value of bottom side.
    DWORD crTransparent;  // transparent color.
} GDIDRAWSTREAM, *PGDIDRAWSTREAM;

//typedef  BOOL(__stdcall* GdiDrawStream)(HDC hDC, DWORD dwStructSize, PGdiDrawStreamStruct pStream);
//static GdiDrawStream  GdiDrawStreamFunc = (GdiDrawStream)GetProcAddress(GetModuleHandleW(L"GDI32.DLL"), "GdiDrawStream");

struct GdiDrawStreamImport
{
    using FuncType = BOOL (*)(HDC hDC, DWORD dwStructSize, PGDIDRAWSTREAM pStream);

    BOOL operator()(HDC hDC, DWORD dwStructSize, PGDIDRAWSTREAM pStream)
    {
        if (!ptr) {
            ptr = reinterpret_cast<FuncType>(
                GetProcAddress(GetModuleHandleW(L"gdi32full"), "GdiDrawStream"));

            if (!ptr)
                return FALSE;
        }

        return ptr(hDC, dwStructSize, pStream);
    }

private:
    FuncType ptr = nullptr;
};

__declspec(selectany) GdiDrawStreamImport GdiDrawStream;

template<typename Function>
Function ResolveProc(wchar_t const* moduleName, char const* procName)
{
    HMODULE const module = GetModuleHandleW(moduleName);
    return reinterpret_cast<Function>(GetProcAddress(module, procName));
}

// WINGDIAPI HBITMAP WINAPI SetBitmapAttributes(_In_ HBITMAP hbm, DWORD dwFlags);
inline HBITMAP(WINAPI* SetBitmapAttributes)(_In_ HBITMAP hbm, DWORD dwFlags) =
    ResolveProc<decltype(SetBitmapAttributes)>(L"gdi32", "SetBitmapAttributes");
