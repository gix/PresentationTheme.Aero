#pragma once
#include <windows.h>
#include <uxtheme.h>

#if !defined(BUILD_UXTHEMEEX)
#define THEMEEXAPI          EXTERN_C DECLSPEC_IMPORT HRESULT STDAPICALLTYPE
#define THEMEEXAPI_(type)   EXTERN_C DECLSPEC_IMPORT type STDAPICALLTYPE
#else
#define THEMEEXAPI          STDAPI
#define THEMEEXAPI_(type)   STDAPI_(type)
#endif

using HTHEMEFILE = HANDLE;

THEMEEXAPI UxOpenThemeFile(
    _In_ wchar_t const* themeFileName,
    _Out_ HTHEMEFILE* phThemeFile);

THEMEEXAPI UxCloseThemeFile(_In_ HTHEMEFILE hThemeFile);

THEMEEXAPI_(HTHEME) UxOpenThemeData(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList);

THEMEAPI_(HTHEME) UxOpenThemeDataEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HWND hwnd,
    _In_ LPCWSTR pszClassList,
    _In_ DWORD dwFlags);

THEMEEXAPI UxCloseThemeData(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme);

THEMEEXAPI UxGetThemeAnimationProperty(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ TA_PROPERTY eProperty,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) VOID* pvProperty,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut);

THEMEEXAPI UxGetThemeAnimationTransform(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStoryboardId,
    _In_ int iTargetId,
    _In_ DWORD dwTransformIndex,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TRANSFORM* pTransform,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut);

THEMEEXAPI UxGetThemeBackgroundContentRect(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pBoundingRect,
    _Out_ LPRECT pContentRect);

THEMEEXAPI UxGetThemeBackgroundExtent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pContentRect,
    _Out_ LPRECT pExtentRect);

THEMEEXAPI UxGetThemeBackgroundRegion(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _Out_ HRGN* pRegion);

THEMEEXAPI UxGetThemeBitmap(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _In_ ULONG dwFlags,
    _Out_ HBITMAP* phBitmap);

THEMEEXAPI UxGetThemeBool(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ BOOL* pfVal);

THEMEEXAPI UxGetThemeColor(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ COLORREF* pColor);

THEMEEXAPI UxGetThemeDocumentationProperty(
    _In_ HTHEMEFILE hThemeFile,
    _In_ LPCWSTR pszThemeName,
    _In_ LPCWSTR pszPropertyName,
    _Out_writes_(cchMaxValChars) LPWSTR pszValueBuff,
    _In_ int cchMaxValChars);

THEMEEXAPI UxGetThemeEnumValue(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal);

THEMEEXAPI UxGetThemeFilename(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszThemeFileName,
    _In_ int cchMaxBuffChars);

THEMEEXAPI UxGetThemeFont(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LOGFONTW* pFont);

THEMEEXAPI UxGetThemeInt(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal);

THEMEEXAPI UxGetThemeIntList(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ INTLIST* pIntList);

THEMEEXAPI UxGetThemeMargins(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _In_opt_ LPCRECT prc,
    _Out_ MARGINS* pMargins);

THEMEEXAPI UxGetThemeMetric(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ int* piVal);

THEMEEXAPI UxGetThemePartSize(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_opt_ LPCRECT prc,
    _In_ enum THEMESIZE eSize,
    _Out_ SIZE* psz);

THEMEEXAPI UxGetThemePosition(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ POINT* pPoint);

THEMEEXAPI UxGetThemePropertyOrigin(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ enum PROPERTYORIGIN* pOrigin);

THEMEEXAPI UxGetThemeRect(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ LPRECT pRect);

THEMEEXAPI UxGetThemeStream(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_ VOID** ppvStream,
    _Out_opt_ DWORD* pcbStream,
    _In_opt_ HINSTANCE hInst);

THEMEEXAPI UxGetThemeString(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ int iPropId,
    _Out_writes_(cchMaxBuffChars) LPWSTR pszBuff,
    _In_ int cchMaxBuffChars);

THEMEEXAPI_(BOOL) UxGetThemeSysBool(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iBoolId);

THEMEEXAPI_(COLORREF) UxGetThemeSysColor(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId);

THEMEEXAPI_(HBRUSH) UxGetThemeSysColorBrush(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iColorId);

THEMEEXAPI UxGetThemeSysFont(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iFontId,
    _Out_ LOGFONTW* plf);

THEMEEXAPI UxGetThemeSysInt(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iIntId,
    _Out_ int* piValue);

THEMEEXAPI_(int) UxGetThemeSysSize(
    _In_ HTHEMEFILE hThemeFile,
    _In_opt_ HTHEME hTheme,
    _In_ int iSizeId);

THEMEEXAPI UxGetThemeSysString(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iStringId,
    _Out_writes_(cchMaxStringChars) LPWSTR pszStringBuff,
    _In_ int cchMaxStringChars);

THEMEEXAPI UxGetThemeTextExtent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchCharCount) LPCWSTR pszText,
    _In_ int cchCharCount,
    _In_ DWORD dwTextFlags,
    _In_opt_ LPCRECT pBoundingRect,
    _Out_ LPRECT pExtentRect);

THEMEEXAPI UxGetThemeTextMetrics(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _Out_ TEXTMETRICW* ptm);

THEMEEXAPI UxGetThemeTimingFunction(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iTimingFunctionId,
    _Out_writes_bytes_to_opt_(cbSize, *pcbSizeOut) TA_TIMINGFUNCTION* pTimingFunction,
    _In_ DWORD cbSize,
    _Out_ DWORD* pcbSizeOut);

THEMEEXAPI UxGetThemeTransitionDuration(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateIdFrom,
    _In_ int iStateIdTo,
    _In_ int iPropId,
    _Out_ DWORD* pdwDuration);

THEMEEXAPI_(BOOL) UxIsThemePartDefined(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId);

THEMEEXAPI_(BOOL) UxIsThemeBackgroundPartiallyTransparent(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ int iPartId,
    _In_ int iStateId);

THEMEEXAPI UxHitTestThemeBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_opt_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ DWORD dwOptions,
    _In_ LPCRECT pRect,
    _In_opt_ HRGN hrgn,
    _In_ POINT ptTest,
    _Out_ WORD* pwHitTestCode);

THEMEEXAPI UxDrawThemeEdge(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pDestRect,
    _In_ UINT uEdge,
    _In_ UINT uFlags,
    _Out_opt_ LPRECT pContentRect);

THEMEEXAPI UxDrawThemeIcon(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_ HIMAGELIST himl,
    _In_ int iImageIndex);

THEMEEXAPI UxDrawThemeBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_opt_ LPCRECT pClipRect);

THEMEEXAPI UxDrawThemeBackgroundEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_ LPCRECT pRect,
    _In_ DTBGOPTS const* pOptions);

THEMEEXAPI UxDrawThemeParentBackground(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HWND hwnd,
    _In_ HDC hdc,
    _In_opt_ RECT const* prc);

THEMEEXAPI UxDrawThemeParentBackgroundEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HWND hwnd,
    _In_ HDC hdc,
    _In_ DWORD dwFlags,
    _In_opt_ RECT const* prc);

THEMEEXAPI UxDrawThemeText(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchText) LPCWSTR pszText,
    _In_ int cchText,
    _In_ DWORD dwTextFlags,
    _In_ DWORD dwTextFlags2,
    _In_ LPCRECT pRect);

THEMEEXAPI UxDrawThemeTextEx(
    _In_ HTHEMEFILE hThemeFile,
    _In_ HTHEME hTheme,
    _In_ HDC hdc,
    _In_ int iPartId,
    _In_ int iStateId,
    _In_reads_(cchText) LPCWSTR pszText,
    _In_ int cchText,
    _In_ DWORD dwTextFlags,
    _Inout_ LPRECT pRect,
    _In_opt_ DTTOPTS const* pOptions);

THEMEEXAPI UxOverrideTheme(_In_ HTHEMEFILE hThemeFile);
THEMEEXAPI UxHook();
THEMEEXAPI UxUnhook();
THEMEEXAPI_(void) UxBroadcastThemeChange();
