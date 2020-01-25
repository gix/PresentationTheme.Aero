#include "Utils.h"

#include "UxThemeDllHelper.h"
#include "UxThemeEx.h"
#include "UxThemeFile.h"

#include <atomic>
#include <cassert>
#include <strsafe.h>
#include <tlhelp32.h>
#include <windows.h>

namespace uxtheme
{

HRESULT SafeStringCchCopyW(wchar_t* pszDest, size_t cchDest, wchar_t const* pszSrc)
{
    if (!pszDest)
        return E_INVALIDARG;

    if (cchDest) {
        if (pszSrc)
            return StringCchCopyW(pszDest, cchDest, pszSrc);
        *pszDest = 0;
    }

    return S_OK;
}

HRESULT SaveClipRegion::Save(HDC hdc)
{
    if (!hRegion) {
        hRegion = CreateRectRgn(0, 0, 1, 1);
        if (!hRegion)
            return MakeErrorLast();
    }

    int clipRgn = GetClipRgn(hdc, hRegion);
    if (clipRgn == -1)
        return MakeErrorLast();

    if (clipRgn == 0)
        hRegion.Reset();

    saved = true;
    return S_OK;
}

HRESULT SaveClipRegion::Restore(HDC hdc)
{
    if (saved)
        SelectClipRgn(hdc, hRegion);
    return S_OK;
}

HRESULT MemoryDC::OpenDC(HDC hdcSource, int width, int height)
{
    HRESULT hr = S_OK;
    bool fDeskDC = false;
    if (!hdcSource) {
        hdcSource = GetWindowDC(nullptr);
        if (!hdcSource) {
            hr = MakeErrorLast();
            goto done;
        }

        fDeskDC = true;
    }

    hBitmap = CreateCompatibleBitmap(hdcSource, width, height);

    if (!hBitmap || (hdc = CreateCompatibleDC(hdcSource), !hdc) ||
        (hOldBitmap = (HBITMAP)SelectObject(hdc, hBitmap), !hOldBitmap))
        hr = MakeErrorLast();

    if (fDeskDC)
        ReleaseDC(nullptr, hdcSource);

done:
    if (FAILED(hr))
        CloseDC();
    return hr;
}

void MemoryDC::CloseDC()
{
    if (hOldBitmap) {
        SelectObject(hdc, hOldBitmap);
        hOldBitmap = nullptr;
    }
    if (hdc) {
        DeleteDC(hdc);
        hdc = nullptr;
    }
    if (hBitmap) {
        DeleteObject(hBitmap);
        hBitmap = nullptr;
    }
}

static bool highContrastEnabled = false;
static std::atomic<bool> hasCustomColorScheme = false;
static UX_COLOR_SCHEME customColorScheme = {};

bool IsHighContrastMode()
{
    return highContrastEnabled;
}

void SetHighContrastMode(bool enabled)
{
    highContrastEnabled = enabled;
}

UX_COLOR_SCHEME const* GetCustomColorScheme()
{
    return hasCustomColorScheme ? &customColorScheme : nullptr;
}

void SetCustomColorScheme(UX_COLOR_SCHEME const* scheme)
{
    if (scheme) {
        customColorScheme = *scheme;
        hasCustomColorScheme = true;
    } else {
        hasCustomColorScheme = false;
        customColorScheme = {};
    }
}

DWORD GetSysColorEx(int index)
{
    if (auto custom = GetCustomColorScheme()) {
        switch (index) {
        case COLOR_ACTIVECAPTION:
            return custom->ActiveTitle;
        case COLOR_BACKGROUND:
            return custom->Background;
        case COLOR_BTNFACE:
            return custom->ButtonFace;
        case COLOR_BTNTEXT:
            return custom->ButtonText;
        case COLOR_GRAYTEXT:
            return custom->GrayText;
        case COLOR_HIGHLIGHT:
            return custom->Hilight;
        case COLOR_HIGHLIGHTTEXT:
            return custom->HilightText;
        case COLOR_HOTLIGHT:
            return custom->HotTrackingColor;
        case COLOR_INACTIVECAPTION:
            return custom->InactiveTitle;
        case COLOR_INACTIVECAPTIONTEXT:
            return custom->InactiveTitleText;
        case COLOR_CAPTIONTEXT:
            return custom->TitleText;
        case COLOR_WINDOW:
            return custom->Window;
        case COLOR_WINDOWTEXT:
            return custom->WindowText;
        case COLOR_SCROLLBAR:
            return custom->Scrollbar;
        case COLOR_MENU:
            return custom->Menu;
        case COLOR_WINDOWFRAME:
            return custom->WindowFrame;
        case COLOR_MENUTEXT:
            return custom->MenuText;
        case COLOR_ACTIVEBORDER:
            return custom->ActiveBorder;
        case COLOR_INACTIVEBORDER:
            return custom->InactiveBorder;
        case COLOR_APPWORKSPACE:
            return custom->AppWorkspace;
        case COLOR_BTNSHADOW:
            return custom->ButtonShadow;
        case COLOR_BTNHIGHLIGHT:
            return custom->ButtonHilight;
        case COLOR_3DDKSHADOW:
            return custom->ButtonDkShadow;
        case COLOR_3DLIGHT:
            return custom->ButtonLight;
        case COLOR_INFOTEXT:
            return custom->InfoText;
        case COLOR_INFOBK:
            return custom->InfoWindow;
        //case : return custom->ButtonAlternateFace;
        case COLOR_GRADIENTACTIVECAPTION:
            return custom->GradientActiveTitle;
        case COLOR_GRADIENTINACTIVECAPTION:
            return custom->GradientInactiveTitle;
        case COLOR_MENUHILIGHT:
            return custom->MenuHilight;
        case COLOR_MENUBAR:
            return custom->MenuBar;
        }
    }

    return GetSysColor(index);
}

static wchar_t const g_pszAppName[] = {0};

wchar_t const* ThemeString(CUxThemeFile* pThemeFile, int iOffset)
{
    if (pThemeFile && pThemeFile->ThemeHeader() && iOffset > 0)
        return (wchar_t const*)((BYTE*)pThemeFile->ThemeHeader() + iOffset);
    return g_pszAppName;
}

static wchar_t const* StringFromError(wchar_t* buffer, size_t size, long ec)
{
    assert(buffer);
    *buffer = 0;
    DWORD cb = FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM, nullptr, static_cast<DWORD>(ec),
                              0, buffer, static_cast<DWORD>(size), nullptr);
    wchar_t const unk[] = L"<unknown>";
    if (!cb && size > lengthof(unk))
        (void)StringCchCopyW(buffer, size, unk);
    return buffer;
}

static bool IsDebuggerAttached()
{
    BOOL debuggerPresent = FALSE;
    return CheckRemoteDebuggerPresent(GetCurrentProcess(), &debuggerPresent) &&
           debuggerPresent;
}

static wchar_t const FallbackMessage[] = L"Failure formatting trace message.\n";

static wchar_t const* FixupMessage(wchar_t* buffer, HRESULT hr)
{
    size_t N = 1024;
    if (SUCCEEDED(hr))
        return buffer;
    if (hr == STRSAFE_E_INSUFFICIENT_BUFFER) {
        if (N >= 4) {
            buffer[N - 4] = L'.';
            buffer[N - 3] = L'.';
            buffer[N - 2] = L'.';
            buffer[N - 1] = 0;
        }
        return buffer;
    }

    return FallbackMessage;
}

void TraceFormatArgs(char const* file, int lineNumber, char const* function,
                     wchar_t const* format, va_list args)
{
    wchar_t buffer[1024];
    wchar_t* ptr = buffer;
    wchar_t* end = buffer + countof(buffer) - 1; // Leave space for '\n'.

    char const* fileName = strrchr(file, '\\');
    fileName = (fileName != nullptr) ? (fileName + 1) : file;

    HRESULT hr = S_OK;
    hr = StringCchPrintfExW(ptr, static_cast<size_t>(end - ptr), &ptr, nullptr, 0,
                            L"%ls(tid %x): %s(%d): %s: ", L"<module>",
                            GetCurrentThreadId(), fileName, lineNumber, function);

    hr = StringCchVPrintfExW(ptr, static_cast<size_t>(end - ptr), &ptr, nullptr, 0,
                             format, args);

    wchar_t const* message = FixupMessage(buffer, hr);

    if (!IsDebuggerAttached())
        return;

    if (SUCCEEDED(hr)) {
        ptr[0] = L'\n';
        ptr[1] = L'\0';
    }

    //OutputDebugStringW(message);
#if _DEBUG
    (void)_CrtDbgReportW(_CRT_WARN, nullptr, 0, nullptr, L"%ls", message);
#endif
}

static void TraceFormat(char const* file, int lineNumber, char const* function,
                        wchar_t const* format, ...)
{
    va_list args;
    va_start(args, format);
    TraceFormatArgs(file, lineNumber, function, format, args);
    va_end(args);
}

long TraceHResult(long hresult, char const* file /*= nullptr*/, int lineNumber /*= 0*/,
                  char const* function /*= nullptr*/)
{
    if (FAILED(hresult)) {
        wchar_t errorMessage[128];
        StringFromError(errorMessage, countof(errorMessage), hresult);
        TraceFormat(file, lineNumber, function, L"HResult=0x%x: %ls", hresult,
                    errorMessage);
        //if (IsDebuggerAttached())
        //    DebugBreak();
    }

    return hresult;
}

int AsciiStrCmpI(wchar_t const* dst, wchar_t const* src)
{
    signed __int64 v3;
    unsigned __int16 v4;
    wchar_t v5;
    int cmp;

    if (dst) {
        if (src) {
            v3 = (BYTE*)dst - (BYTE*)src;
            do {
                v4 = *(const wchar_t*)((BYTE*)src + v3);
                if ((unsigned __int16)(v4 - 65) <= 0x19u)
                    v4 += 32;
                v5 = *src;
                if ((unsigned __int16)(*src - 65) <= 0x19u)
                    v5 += 32;
                ++src;
            } while (v4 && v4 == v5);
            cmp = v4 - v5;
        } else {
            cmp = 1;
        }
    } else {
        cmp = -(src != nullptr);
    }

    return cmp;
}

HRESULT GetPtrToResource(HMODULE hInst, wchar_t const* pszResType,
                         wchar_t const* pszResName, void** ppBytes, unsigned* pdwBytes)
{
    *ppBytes = nullptr;
    *pdwBytes = 0;

    HRSRC hRes = FindResourceExW(hInst, pszResType, pszResName, 0);
    DWORD size;
    HGLOBAL hData;
    LPVOID ptr;

    if (hRes && (size = SizeofResource(hInst, hRes)) != 0 &&
        (hData = LoadResource(hInst, hRes)) != nullptr &&
        (ptr = LockResource(hData)) != nullptr) {
        *ppBytes = ptr;
        *pdwBytes = size;
        return S_OK;
    }

    return MakeErrorLast();
}

BOOL EnumProcessThreads(_In_ DWORD processId, _In_ THREADENUMPROC callback,
                        _In_opt_ LPARAM param)
{
    if (!callback)
        return FALSE;

    HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, processId);
    if (h == INVALID_HANDLE_VALUE)
        return FALSE;

    THREADENTRY32 te;
    te.dwSize = sizeof(te);
    if (Thread32First(h, &te)) {
        do {
            if (te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) +
                                 sizeof(te.th32OwnerProcessID) &&
                te.th32OwnerProcessID == processId) {
                callback(te.th32OwnerProcessID, te.th32ThreadID, param);
            }
            te.dwSize = sizeof(te);
        } while (Thread32Next(h, &te));
    }

    CloseHandle(h);
    return TRUE;
}

BOOL EnumProcessWindows(_In_ DWORD processId, _In_ WNDENUMPROC callback,
                        _In_opt_ LPARAM param)
{
    struct Params
    {
        WNDENUMPROC callback;
        LPARAM param;
    };

    Params ptParams{callback, param};
    auto cb = [](DWORD pid, DWORD tid, LPARAM p) -> BOOL {
        auto pp = reinterpret_cast<Params*>(p);
        EnumThreadWindows(tid, pp->callback, pp->param);
        return TRUE;
    };

    return EnumProcessThreads(processId, cb, reinterpret_cast<LPARAM>(&ptParams));
}

void SafeSendMessage(HWND hwnd, DWORD uMsg, WPARAM wParam, LPARAM lParam)
{
    DWORD_PTR result;
    if (!SendMessageTimeoutW(hwnd, uMsg, wParam, lParam, SMTO_BLOCK | SMTO_ABORTIFHUNG,
                             250, &result))
        PostMessageW(hwnd, uMsg, wParam, lParam);
}

void SendThemeChanged(HWND hwnd)
{
    SafeSendMessage(hwnd, WM_THEMECHANGED, 0, 0);
    SafeSendMessage(hwnd, WM_SYSCOLORCHANGE, 0, 0);
}

void SendThemeChangedProcessLocal()
{
    UxThemeDllHelper::Get().CThemeMenuMetrics_FlushAll();

    EnumProcessWindows(
        GetCurrentProcessId(),
        [](HWND hwnd, LPARAM param) -> BOOL {
            SendThemeChanged(hwnd);
            EnumChildWindows(
                hwnd,
                [](HWND hwndChild, LPARAM p) {
                    SendThemeChanged(hwndChild);
                    return TRUE;
                },
                0);
            return TRUE;
        },
        0);
}

HRESULT GetModuleFileNameW(HMODULE module, std::wstring& path)
{
    constexpr DWORD max = std::numeric_limits<DWORD>::max() / 2;

    for (DWORD size = MAX_PATH; size < max; size *= 2) {
        path.resize(size);
        DWORD const length = GetModuleFileNameW(module, path.data(), size + 1);
        if (length < size)
            return S_OK;
    }

    return GetLastErrorAsHResult();
}

HRESULT GetModuleFileVersion(HMODULE module, VersionInfo& version)
{
    std::wstring path;
    ENSURE_HR(GetModuleFileNameW(module, path));

    DWORD verHandle = 0;
    DWORD const versionInfoSize = GetFileVersionInfoSizeW(path.c_str(), &verHandle);
    if (versionInfoSize == 0)
        return HRESULT_FROM_WIN32(GetLastError());

    std::vector<char> verData(versionInfoSize);
    if (!GetFileVersionInfoW(path.c_str(), verHandle, versionInfoSize, verData.data()))
        return HRESULT_FROM_WIN32(GetLastError());

    void* buffer = nullptr;
    unsigned size = 0;
    if (!VerQueryValueW(verData.data(), L"\\", &buffer, &size) ||
        size < sizeof(VS_FIXEDFILEINFO))
        return HRESULT_FROM_WIN32(GetLastError());

    auto const verInfo = static_cast<VS_FIXEDFILEINFO const*>(buffer);
    if (verInfo->dwSignature != 0xFEEF04BD)
        return E_FAIL;

    version.Major = (verInfo->dwFileVersionMS >> 16) & 0xFFFFu;
    version.Minor = (verInfo->dwFileVersionMS >> 0) & 0xFFFFu;
    version.Build = (verInfo->dwFileVersionLS >> 16) & 0xFFFFu;
    version.Revision = (verInfo->dwFileVersionLS >> 0) & 0xFFFFu;
    return S_OK;
}

} // namespace uxtheme
