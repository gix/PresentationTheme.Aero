#pragma once
#include <windows.h>
#include <uxtheme.h>

namespace uxtheme
{

WORD HitTestRect(DWORD dwHTFlags, RECT const* prc,
                 MARGINS const* margins, POINT const* pt);

WORD HitTest9Grid(RECT const* prc, MARGINS const* margins,
                  POINT const* pt, bool fCheckLeftMarginZero);

WORD HitTestTemplate(DWORD dwHTFlags, RECT const* prc, HRGN hrgn,
                     MARGINS const* margins, POINT const* pt);

} // namespace uxtheme
